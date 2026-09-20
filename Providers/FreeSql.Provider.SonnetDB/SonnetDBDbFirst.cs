// SonnetDB DbFirst 实现。
//
// SonnetDB 有两种 SQL 数据模型：关系表的元数据通过 SonnetDB 3.1
// ADO.NET GetSchema 合同提供，时序测量的元数据继续通过
// SHOW/DESCRIBE 命令提供。

using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using SonnetDB.Model;

namespace FreeSql.SonnetDB
{
    class SonnetDBDbFirst : IDbFirst
    {
        readonly IFreeSql _orm;
        readonly CommonUtils _commonUtils;
        public SonnetDBDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
        }

        public int GetDbType(DbColumnInfo column)
        {
            var type = (column?.DbTypeTextFull ?? column?.DbTypeText ?? string.Empty).Trim().ToLowerInvariant();
            // GEOPOINT 是 SonnetDB 的复合标量，使用 Object 作为 ADO.NET 的兼容类别；
            // 其 CLR 类型在下面的专门映射中保留为 GeoPoint。
            if (type.Contains("geopoint")) return (int)DbType.Object;
            if (type.Contains("rowversion")) return (int)DbType.Int64;
            if (type.Contains("datetime") || type.Contains("timestamp") || type == "date" || type.StartsWith("date ")) return (int)DbType.DateTime;
            if (type.Contains("float") || type.Contains("double") || type.Contains("real") || type.Contains("decimal")) return (int)DbType.Double;
            if (type.Contains("int") || type.Contains("bigint") || type.Contains("long") || string.Equals(column?.Name, "time", StringComparison.OrdinalIgnoreCase)) return (int)DbType.Int64;
            if (type.Contains("bool")) return (int)DbType.Boolean;
            if (type.Contains("blob") || type.Contains("binary")) return (int)DbType.Binary;
            // JSON 在 SonnetDB 中以已校验的 JSON 文本传输。
            if (type.Contains("json")) return (int)DbType.String;
            if (type.Contains("vector")) return (int)DbType.Object;
            return (int)DbType.String;
        }

        static readonly Dictionary<int, DbToCs> _dicDbToCs = new Dictionary<int, DbToCs>
        {
            { (int)DbType.Int64, new DbToCs("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },
            { (int)DbType.Double, new DbToCs("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
            { (int)DbType.Boolean, new DbToCs("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },
            { (int)DbType.DateTime, new DbToCs("(DateTime?)", "DateTime.Parse({0})", "{0}.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
            { (int)DbType.Binary, new DbToCs("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },
            { (int)DbType.String, new DbToCs("", "{0}", "{0}", "string", typeof(string), typeof(string), "{0}", "GetString") },
            { (int)DbType.Object, new DbToCs("", "{0}", "{0}", "object", typeof(object), typeof(object), "{0}", "GetValue") },
        };

        public string GetCsConvert(DbColumnInfo column)
        {
            if (IsGeoPointColumn(column)) return column.IsNullable
                ? "(global::SonnetDB.Model.GeoPoint?){0}"
                : "(global::SonnetDB.Model.GeoPoint){0}";
            if (IsVectorColumn(column)) return "(float[]){0}";
            return _dicDbToCs.TryGetValue(column.DbType, out var value) ? (column.IsNullable ? value.csConvert : value.csConvert.Replace("?", "")) : null;
        }
        public string GetCsParse(DbColumnInfo column)
        {
            // ADO.NET 读取 GEOPOINT 时返回原生 GeoPoint，而不是字符串；
            // DbFirst 的转换模板因此只需做显式类型转换，避免把 POINT(...) 当作普通文本解析。
            if (IsGeoPointColumn(column)) return "(global::SonnetDB.Model.GeoPoint){0}";
            if (IsVectorColumn(column)) return "(float[]){0}";
            return _dicDbToCs.TryGetValue(column.DbType, out var value) ? value.csParse : null;
        }

        public string GetCsStringify(DbColumnInfo column)
        {
            if (IsGeoPointColumn(column)) return "{0}.ToString()";
            if (IsVectorColumn(column)) return "string.Join(\",\", {0})";
            return _dicDbToCs.TryGetValue(column.DbType, out var value) ? value.csStringify : null;
        }
        public string GetCsType(DbColumnInfo column)
        {
            if (IsGeoPointColumn(column)) return column.IsNullable ? "global::SonnetDB.Model.GeoPoint?" : "global::SonnetDB.Model.GeoPoint";
            if (IsVectorColumn(column)) return "float[]";
            return _dicDbToCs.TryGetValue(column.DbType, out var value) ? (column.IsNullable ? value.csType : value.csType.Replace("?", "")) : null;
        }
        public Type GetCsTypeInfo(DbColumnInfo column) => IsGeoPointColumn(column)
            ? (column.IsNullable ? typeof(GeoPoint?) : typeof(GeoPoint))
            : IsVectorColumn(column)
            ? typeof(float[])
            : _dicDbToCs.TryGetValue(column.DbType, out var value) ? value.csTypeInfo : null;
        public string GetCsTypeValue(DbColumnInfo column)
        {
            if (IsGeoPointColumn(column)) return column.IsNullable ? "{0}.Value" : "{0}";
            if (IsVectorColumn(column)) return "{0}";
            return _dicDbToCs.TryGetValue(column.DbType, out var value) ? value.csTypeValue : null;
        }
        public string GetDataReaderMethod(DbColumnInfo column) => IsGeoPointColumn(column)
            ? "GetValue"
            : IsVectorColumn(column) ? "GetValue"
            : _dicDbToCs.TryGetValue(column.DbType, out var value) ? value.dataReaderMethod : null;

        public List<string> GetDatabases()
        {
            try { return _orm.Ado.Query<string>("SHOW DATABASES") ?? new List<string>(); }
            catch
            {
                try
                {
                    using (var conn = _orm.Ado.MasterPool?.Get(TimeSpan.FromSeconds(5)))
                    {
                        var database = conn?.Value?.Database;
                        return string.IsNullOrEmpty(database) ? new List<string>() : new List<string> { database };
                    }
                }
                catch { return new List<string>(); }
            }
        }

        public bool ExistsTable(string name, bool ignoreCase)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var qualifiedDatabase = GetQualifiedDatabase(name);
            if (!string.IsNullOrWhiteSpace(qualifiedDatabase) &&
                !IsCurrentDatabaseSelected(new[] { qualifiedDatabase }, ignoreCase))
                return false;
            try { if (GetTables(null, name, ignoreCase).Any()) return true; }
            catch { }
            try
            {
                _orm.Ado.ExecuteDataTable($"SELECT * FROM {_commonUtils.QuoteSqlName(GetObjectName(name))} LIMIT 0");
                return true;
            }
            catch { return false; }
        }

        public DbTableInfo GetTableByName(string name, bool ignoreCase = true)
        {
            var table = GetTables(null, name, ignoreCase).FirstOrDefault();
            if (table == null) return null;

            // 单表读取时，初始集合不包含被引用表，原来的外键组装会直接跳过。
            // 按需加载被引用关系表，既保留单表查询的开销边界，也让 DbFirst
            // 的 Foreigns 合同与全库枚举保持一致。
            var referencedTables = new Dictionary<string, DbTableInfo>(StringComparer.OrdinalIgnoreCase);
            PopulateForeignKeys(new List<DbTableInfo> { table }, ignoreCase, referencedName =>
            {
                if (string.IsNullOrWhiteSpace(referencedName)) return null;
                var objectName = GetObjectName(referencedName);
                if (referencedTables.TryGetValue(objectName, out var cached)) return cached;

                var referenced = LoadRelationshipTables(null, objectName, ignoreCase)
                    .FirstOrDefault(item => NameEquals(item.Name, objectName, ignoreCase));
                referencedTables[objectName] = referenced;
                return referenced;
            });
            return table;
        }
        public List<DbTableInfo> GetTablesByDatabase(params string[] database) => GetTables(database, null, true);

        public List<DbTableInfo> GetTables(string[] database, string tablename, bool ignoreCase)
        {
            // SonnetDB 的 SHOW/DESCRIBE 目录命令只作用于当前连接库，不能把
            // 当前库的对象冒充为调用方指定的其他数据库。
            if (!IsCurrentDatabaseSelected(database, ignoreCase)) return new List<DbTableInfo>();

            // FreeSql 允许用“数据库.对象”传入限定名。SonnetDB 3.1 的目录
            // 接口不能跨库查询，不能只去掉前缀后继续查当前库的同名对象。
            var qualifiedDatabase = GetQualifiedDatabase(tablename);
            if (!string.IsNullOrWhiteSpace(qualifiedDatabase))
            {
                if (!IsCurrentDatabaseSelected(new[] { qualifiedDatabase }, ignoreCase)) return new List<DbTableInfo>();
                if (database != null && database.Length > 0 &&
                    database.Any(a => !string.IsNullOrWhiteSpace(a)) &&
                    !database.Any(a => NameEquals(UnquoteIdentifier(a?.Trim()), qualifiedDatabase, ignoreCase)))
                    return new List<DbTableInfo>();
            }

            var result = new List<DbTableInfo>();
            var requested = string.IsNullOrEmpty(tablename) ? null : GetObjectName(tablename);
            var tables = LoadRelationshipTables(database, requested, ignoreCase);
            result.AddRange(tables);

            // SonnetDB 3.1 的 GetSchema("Tables") 只返回关系表，视图需要通过
            // SHOW VIEWS 单独枚举；列结构再用 LIMIT 0 查询获取。
            foreach (var view in LoadViews(database, requested, ignoreCase))
                if (!result.Any(a => NameEquals(a.Name, view.Name, true))) result.Add(view);

            // 时序测量目录与关系表相互独立。名称冲突时优先返回关系表，
            // 因为只有关系表具备 FreeSql 的主键/索引语义。
            var measurementNames = TryQueryNames("SHOW MEASUREMENTS");
            if (measurementNames != null)
                foreach (var name in measurementNames)
                    if (MatchesName(name, requested, ignoreCase) && !result.Any(a => NameEquals(a.Name, name, true)))
                    {
                        var measurement = BuildMeasurement(name);
                        if (measurement != null) result.Add(measurement);
                    }

            // 旧版提供程序可能不支持目录命令；按名称直接查询时保留原有的
            // SELECT LIMIT 0 回退路径。
            if (result.Count == 0 && requested != null)
            {
                var measurement = BuildMeasurement(requested);
                if (measurement != null) result.Add(measurement);
                else
                {
                    var table = GetTableBySelectSchema(requested, false);
                    if (table != null) result.Add(table);
                }
            }

            result.Sort((left, right) =>
            {
                var compare = string.Compare(left.Schema, right.Schema, StringComparison.OrdinalIgnoreCase);
                return compare == 0 ? string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase) : compare;
            });
            return result;
        }

        List<DbTableInfo> LoadViews(string[] database, string requested, bool ignoreCase)
        {
            if (!IsCurrentDatabaseSelected(database, ignoreCase)) return new List<DbTableInfo>();
            var names = TryQueryNames("SHOW VIEWS");
            if (names == null)
                names = TryQueryNames("SELECT table_name FROM information_schema.views");
            if (names == null) return new List<DbTableInfo>();

            var result = new List<DbTableInfo>();
            foreach (var name in names)
            {
                if (!MatchesName(name, requested, ignoreCase)) continue;
                var view = GetTableBySelectSchema(name, false);
                if (view == null) continue;
                view.Id = "view|" + name;
                view.Type = DbTableType.VIEW;
                result.Add(view);
            }
            return result;
        }

        List<DbTableInfo> LoadRelationshipTables(string[] database, string requested, bool ignoreCase)
        {
            var result = new List<DbTableInfo>();
            var schemaTables = TryGetSchema("Tables");
            var schemaColumns = TryGetSchema("Columns");
            var schemaIndexes = TryGetSchema("Indexes");

            if (schemaTables != null)
            {
                foreach (DataRow row in schemaTables.Rows)
                {
                    var name = GetString(row, "TABLE_NAME", 2);
                    var catalog = GetString(row, "TABLE_CATALOG", 0);
                    if (string.IsNullOrEmpty(name) || !MatchesName(name, requested, ignoreCase) || !MatchesDatabase(catalog, database, ignoreCase)) continue;
                    var schema = GetString(row, "TABLE_SCHEMA", 1) ?? string.Empty;
                    var key = GetTableKey(catalog, schema, name);
                    if (result.Any(a => string.Equals(a.Id, key, StringComparison.OrdinalIgnoreCase))) continue;
                    var tableType = GetString(row, "TABLE_TYPE", 3);
                    result.Add(new DbTableInfo
                    {
                        Id = key,
                        Schema = schema,
                        Name = name,
                        Type = string.Equals(tableType, "VIEW", StringComparison.OrdinalIgnoreCase) ? DbTableType.VIEW : DbTableType.TABLE,
                        Columns = new List<DbColumnInfo>()
                    });
                }
            }
            else
            {
                var names = TryQueryNames("SHOW TABLES");
                if (names != null)
                    foreach (var name in names)
                        if (MatchesName(name, requested, ignoreCase))
                            result.Add(new DbTableInfo { Id = GetTableKey(null, null, name), Name = name, Type = DbTableType.TABLE, Columns = new List<DbColumnInfo>() });
            }

            if (schemaColumns != null) PopulateColumnsFromSchema(result, schemaColumns, ignoreCase);
            foreach (var table in result)
            {
                if (table.Columns.Count == 0 && !TryPopulateDescribeTable(table, ignoreCase))
                {
                    var selected = GetTableBySelectSchema(table.Name, false);
                    if (selected != null) CopyColumns(selected, table);
                }
            }
            if (schemaIndexes != null) PopulateIndexesFromSchema(result, schemaIndexes, ignoreCase);
            else foreach (var table in result) PopulateIndexesFromShow(table, ignoreCase);
            PopulateForeignKeys(result, ignoreCase);
            foreach (var table in result) FinalizeTable(table);
            return result;
        }

        void PopulateColumnsFromSchema(List<DbTableInfo> tables, DataTable schema, bool ignoreCase)
        {
            foreach (DataRow row in schema.Rows)
            {
                var table = FindTable(tables, GetString(row, "TABLE_NAME", 2), GetString(row, "TABLE_SCHEMA", 1), GetString(row, "TABLE_CATALOG", 0), ignoreCase);
                var name = GetString(row, "COLUMN_NAME", 3);
                if (table == null || string.IsNullOrEmpty(name)) continue;
                var type = GetString(row, "DATA_TYPE", 7) ?? "string";
                var isRowVersion = GetBool(row, "IS_ROW_VERSION", false, 12);
                var column = new DbColumnInfo
                {
                    Table = table,
                    Name = name,
                    DbTypeText = type,
                    DbTypeTextFull = isRowVersion ? type + " ROWVERSION" : type,
                    MaxLength = GetInt(row, "CHARACTER_MAXIMUM_LENGTH", 8),
                    Precision = GetInt(row, "NUMERIC_PRECISION", 9),
                    Scale = GetInt(row, "NUMERIC_SCALE", 10),
                    IsNullable = GetBool(row, "IS_NULLABLE", true, 6),
                    IsPrimary = GetBool(row, "IS_PRIMARY_KEY", false, 11),
                    IsIdentity = GetBool(row, "IS_AUTO_INCREMENT", false, 13),
                    DefaultValue = GetString(row, "COLUMN_DEFAULT", 5),
                    Position = GetInt(row, "ORDINAL_POSITION", 4)
                };
                if (column.Position <= 0) column.Position = table.Columns.Count + 1;
                ApplyColumnType(column);
                AddOrReplaceColumn(table, column, ignoreCase);
            }
        }

        void PopulateIndexesFromSchema(List<DbTableInfo> tables, DataTable schema, bool ignoreCase)
        {
            foreach (var row in schema.Rows.Cast<DataRow>()
                .OrderBy(a => GetString(a, "TABLE_NAME", 2), StringComparer.OrdinalIgnoreCase)
                .ThenBy(a => GetString(a, "INDEX_NAME", 3), StringComparer.OrdinalIgnoreCase)
                .ThenBy(a => GetInt(a, "ORDINAL_POSITION", 6)))
            {
                var table = FindTable(tables, GetString(row, "TABLE_NAME", 2), GetString(row, "TABLE_SCHEMA", 1), GetString(row, "TABLE_CATALOG", 0), ignoreCase);
                var indexName = GetString(row, "INDEX_NAME", 3);
                var column = FindColumn(table, GetString(row, "COLUMN_NAME", 5), ignoreCase);
                if (table == null || string.IsNullOrEmpty(indexName) || column == null) continue;
                AddIndex(table, indexName, GetBool(row, "IS_UNIQUE", false, 4), column,
                    // JSON_PATH 是 3.1 的可选元数据列；旧驱动缺列时不能按位置
                    // 回退，否则会把 CREATED_UTC 误读成路径。
                    GetString(row, "JSON_PATH"));
            }
        }

        void PopulateIndexesFromShow(DbTableInfo table, bool ignoreCase)
        {
            var dt = TryExecuteDataTable($"SHOW INDEXES ON {_commonUtils.QuoteSqlName(table.Name)}");
            if (dt == null) return;
            foreach (DataRow row in dt.Rows)
            {
                var indexName = GetString(row, "index_name", 0);
                var columns = GetString(row, "columns", 2);
                if (string.IsNullOrEmpty(indexName) || string.IsNullOrEmpty(columns)) continue;
                foreach (var value in columns.Split(','))
                {
                    var columnText = value.Trim();
                    string jsonPath = null;
                    var pathSeparator = columnText.IndexOf("->", StringComparison.Ordinal);
                    if (pathSeparator > 0)
                    {
                        jsonPath = columnText.Substring(pathSeparator + 2).Trim();
                        columnText = columnText.Substring(0, pathSeparator).Trim();
                    }
                    var column = FindColumn(table, UnquoteIdentifier(columnText), ignoreCase);
                    if (column != null) AddIndex(table, indexName, GetBool(row, "is_unique", false, 1), column, jsonPath);
                }
            }
        }

        void PopulateForeignKeys(List<DbTableInfo> tables, bool ignoreCase) =>
            PopulateForeignKeys(tables, ignoreCase, null);

        void PopulateForeignKeys(List<DbTableInfo> tables, bool ignoreCase,
            Func<string, DbTableInfo> getReferencedTable)
        {
            var schema = TryExecuteDataTable(
                "SELECT table_schema, constraint_name, table_name, column_name, ordinal_position, " +
                "principal_table_name, principal_column_name, on_delete " +
                "FROM information_schema.foreign_keys");
            if (schema == null) return;

            foreach (var row in schema.Rows.Cast<DataRow>()
                .OrderBy(a => GetString(a, "table_name", 2), StringComparer.OrdinalIgnoreCase)
                .ThenBy(a => GetString(a, "constraint_name", 1), StringComparer.OrdinalIgnoreCase)
                .ThenBy(a => GetInt(a, "ordinal_position", 4)))
            {
                var table = FindTable(tables, GetString(row, "table_name", 2),
                    GetString(row, "table_schema", 0), null, ignoreCase);
                if (table == null) continue;

                var referencedName = GetString(row, "principal_table_name", 5);
                var referencedObjectName = GetObjectName(referencedName);
                var referencedTable = FindTable(tables, referencedObjectName, null, null, ignoreCase) ??
                    getReferencedTable?.Invoke(referencedName);
                var column = FindColumn(table, GetString(row, "column_name", 3), ignoreCase);
                var referencedColumn = FindColumn(referencedTable, GetString(row, "principal_column_name", 6), ignoreCase);
                var constraintName = GetString(row, "constraint_name", 1);
                if (referencedTable == null || column == null || referencedColumn == null ||
                    string.IsNullOrWhiteSpace(constraintName))
                    continue;

                if (!table.ForeignsDict.TryGetValue(constraintName, out var foreign))
                {
                    foreign = new DbForeignInfo { Table = table, ReferencedTable = referencedTable };
                    table.ForeignsDict.Add(constraintName, foreign);
                }
                if (!foreign.Columns.Any(item => NameEquals(item.Name, column.Name, ignoreCase)))
                    foreign.Columns.Add(column);
                if (!foreign.ReferencedColumns.Any(item => NameEquals(item.Name, referencedColumn.Name, ignoreCase)))
                    foreign.ReferencedColumns.Add(referencedColumn);
            }
        }

        bool TryPopulateDescribeTable(DbTableInfo table, bool ignoreCase)
        {
            var quoted = _commonUtils.QuoteSqlName(table.Name);
            var dt = TryExecuteDataTable($"DESCRIBE TABLE {quoted}") ?? TryExecuteDataTable($"DESCRIBE {quoted}");
            if (dt == null) return false;
            var fallbackPosition = 1;
            foreach (DataRow row in dt.Rows)
            {
                var name = GetString(row, "column_name", 0);
                if (string.IsNullOrEmpty(name)) continue;
                var type = GetString(row, "data_type", 1) ?? "string";
                var column = new DbColumnInfo
                {
                    Table = table,
                    Name = name,
                    DbTypeText = type,
                    DbTypeTextFull = type,
                    IsNullable = GetBool(row, "is_nullable", true, 2),
                    IsPrimary = GetBool(row, "is_primary_key", false, 3),
                    Position = GetInt(row, "ordinal", 4),
                    DefaultValue = GetString(row, "column_default", 5),
                    IsIdentity = GetBool(row, "is_auto_increment", false, 6)
                };
                if (column.Position <= 0) column.Position = fallbackPosition;
                ApplyColumnType(column);
                AddOrReplaceColumn(table, column, ignoreCase);
                fallbackPosition++;
            }
            return true;
        }

        DbTableInfo BuildMeasurement(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var dt = TryExecuteDataTable($"DESCRIBE MEASUREMENT {_commonUtils.QuoteSqlName(name)}");
            if (dt == null) return null;
            var table = new DbTableInfo { Id = "measurement|" + name, Name = name, Type = DbTableType.TABLE, Columns = new List<DbColumnInfo>() };
            table.Columns.Add(new DbColumnInfo { Table = table, Name = "time", DbTypeText = "int64", DbTypeTextFull = "time", DbType = (int)DbType.Int64, CsType = typeof(long), IsNullable = false, Position = 1 });
            var position = 2;
            foreach (DataRow row in dt.Rows)
            {
                var nameValue = GetString(row, "column_name", 0);
                if (string.IsNullOrEmpty(nameValue) || string.Equals(nameValue, "time", StringComparison.OrdinalIgnoreCase)) continue;
                var role = GetString(row, "column_type", 1);
                var type = GetString(row, "data_type", 2) ?? "string";
                var column = new DbColumnInfo { Table = table, Name = nameValue, DbTypeText = type, DbTypeTextFull = string.IsNullOrEmpty(role) ? type : role + " " + type, IsNullable = true, Position = position++ };
                ApplyColumnType(column);
                table.Columns.Add(column);
            }
            FinalizeTable(table);
            return table;
        }

        DbTableInfo GetTableBySelectSchema(string name, bool measurement)
        {
            var dt = TryExecuteDataTable($"SELECT * FROM {_commonUtils.QuoteSqlName(name)} LIMIT 0");
            if (dt == null) return null;
            var table = new DbTableInfo { Id = (measurement ? "measurement|" : "table|") + name, Name = name, Type = DbTableType.TABLE, Columns = new List<DbColumnInfo>() };
            var position = 1;
            if (measurement && !dt.Columns.Cast<DataColumn>().Any(a => string.Equals(a.ColumnName, "time", StringComparison.OrdinalIgnoreCase)))
                table.Columns.Add(new DbColumnInfo { Table = table, Name = "time", DbTypeText = "int64", DbTypeTextFull = "time", DbType = (int)DbType.Int64, CsType = typeof(long), IsNullable = false, Position = position++ });
            foreach (DataColumn source in dt.Columns)
            {
                var type = GetClrTypeName(source.DataType);
                var column = new DbColumnInfo { Table = table, Name = source.ColumnName, DbTypeText = type, DbTypeTextFull = type, IsNullable = source.AllowDBNull, Position = position++ };
                ApplyColumnType(column);
                table.Columns.Add(column);
            }
            FinalizeTable(table);
            return table;
        }

        DataTable TryGetSchema(string collection)
        {
            try
            {
                if (_orm?.Ado?.MasterPool == null) return null;
                using (var conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5))) return conn?.Value?.GetSchema(collection);
            }
            catch { return null; }
        }

        List<string> TryQueryNames(string sql)
        {
            try { return (_orm.Ado.Query<string>(sql) ?? new List<string>()).Where(a => !string.IsNullOrEmpty(a)).Distinct(StringComparer.OrdinalIgnoreCase).ToList(); }
            catch { return null; }
        }

        DataTable TryExecuteDataTable(string sql)
        {
            try { return _orm.Ado.ExecuteDataTable(sql); }
            catch { return null; }
        }

        static bool IsGeoPointColumn(DbColumnInfo column)
        {
            var type = column?.DbTypeTextFull ?? column?.DbTypeText;
            return type?.IndexOf("geopoint", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsVectorColumn(DbColumnInfo column)
        {
            var type = column?.DbTypeTextFull ?? column?.DbTypeText;
            return type?.IndexOf("vector", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        void ApplyColumnType(DbColumnInfo column)
        {
            column.DbType = GetDbType(column);
            column.CsType = GetCsTypeInfo(column);
        }

        static void CopyColumns(DbTableInfo source, DbTableInfo destination)
        {
            if (source == null) return;
            if (destination == null) return;
            foreach (var sourceColumn in source.Columns)
            {
                if (sourceColumn == null) continue;
                // 回退查询可能先构造临时表，再把列复制到目录结果。不能直接重绑
                // 源列的表引用，否则后续调用者看到的源表元数据会被串改。
                destination.Columns.Add(new DbColumnInfo
                {
                    Table = destination,
                    Name = sourceColumn.Name,
                    CsType = sourceColumn.CsType,
                    DbType = sourceColumn.DbType,
                    DbTypeText = sourceColumn.DbTypeText,
                    DbTypeTextFull = sourceColumn.DbTypeTextFull,
                    MaxLength = sourceColumn.MaxLength,
                    Precision = sourceColumn.Precision,
                    Scale = sourceColumn.Scale,
                    IsPrimary = sourceColumn.IsPrimary,
                    IsIdentity = sourceColumn.IsIdentity,
                    IsNullable = sourceColumn.IsNullable,
                    Comment = sourceColumn.Comment,
                    DefaultValue = sourceColumn.DefaultValue,
                    Position = sourceColumn.Position
                });
            }
        }

        static void AddOrReplaceColumn(DbTableInfo table, DbColumnInfo column, bool ignoreCase)
        {
            var old = table.Columns.FirstOrDefault(a => NameEquals(a.Name, column.Name, ignoreCase));
            if (old != null) table.Columns.Remove(old);
            table.Columns.Add(column);
        }

        static void AddIndex(DbTableInfo table, string name, bool unique, DbColumnInfo column, string jsonPath = null)
        {
            var target = unique ? table.UniquesDict : table.IndexesDict;
            if (!target.TryGetValue(name, out var index)) target.Add(name, index = new DbIndexInfo { Name = name, IsUnique = unique });
            if (!string.IsNullOrWhiteSpace(jsonPath) && string.IsNullOrWhiteSpace(index.JsonPath))
                index.JsonPath = jsonPath.Trim();
            if (!index.Columns.Any(a => string.Equals(a.Column.Name, column.Name, StringComparison.OrdinalIgnoreCase))) index.Columns.Add(new DbIndexColumnInfo { Column = column, IsDesc = false });
        }

        static void FinalizeTable(DbTableInfo table)
        {
            table.Columns.Sort((left, right) =>
            {
                var compare = left.Position.CompareTo(right.Position);
                return compare == 0 ? string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase) : compare;
            });
            table.Primarys.Clear();
            table.Identitys.Clear();
            foreach (var column in table.Columns)
            {
                if (column.IsPrimary) table.Primarys.Add(column);
                if (column.IsIdentity) table.Identitys.Add(column);
            }
        }

        static DbTableInfo FindTable(IEnumerable<DbTableInfo> tables, string name, string schema, string catalog, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return tables.FirstOrDefault(a => NameEquals(a.Name, name, ignoreCase) && (string.IsNullOrEmpty(schema) || NameEquals(a.Schema, schema, ignoreCase)) && (string.IsNullOrEmpty(catalog) || a.Id.StartsWith(catalog + "|", StringComparison.OrdinalIgnoreCase)))
                ?? tables.FirstOrDefault(a => NameEquals(a.Name, name, ignoreCase));
        }

        static DbColumnInfo FindColumn(DbTableInfo table, string name, bool ignoreCase) => table?.Columns.FirstOrDefault(a => NameEquals(a.Name, name, ignoreCase));
        // SHOW/Schema 返回的名称已经是单个对象名，名称本身可以合法包含点号；
        // 只有调用方传入的限定名才需要按点号拆分。
        static bool MatchesName(string name, string requested, bool ignoreCase) => requested == null || NameEquals(UnquoteIdentifier(name?.Trim()), requested, ignoreCase);
        static bool NameEquals(string left, string right, bool ignoreCase) => string.Equals(left ?? string.Empty, right ?? string.Empty, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

        static string GetObjectName(string name)
        {
            var parts = SplitQualifiedName(name);
            return parts.Length == 0 ? name : UnquoteIdentifier(parts[parts.Length - 1]);
        }

        static string GetQualifiedDatabase(string name)
        {
            var parts = SplitQualifiedName(name);
            return parts.Length > 1 ? UnquoteIdentifier(parts[0]) : null;
        }

        /// <summary>
        /// 按 SQL 标识符规则拆分限定名。与 <see cref="CommonUtils.SplitTableName"/> 的
        /// 语义一致，但这里保留全部段数，以便 DbFirst 正确处理三段限定名及引号内的点号。
        /// </summary>
        static string[] SplitQualifiedName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return Array.Empty<string>();

            var parts = new List<string>();
            var segmentStart = 0;
            var closingQuote = '\0';
            for (var index = 0; index < name.Length; index++)
            {
                var current = name[index];
                if (closingQuote != '\0')
                {
                    if (current != closingQuote) continue;
                    // SQL 使用连续的引号表示标识符中的一个引号。
                    if (index + 1 < name.Length && name[index + 1] == closingQuote)
                    {
                        index++;
                        continue;
                    }
                    closingQuote = '\0';
                    continue;
                }

                if (current == '"' || current == '`')
                {
                    closingQuote = current;
                    continue;
                }
                if (current == '[')
                {
                    closingQuote = ']';
                    continue;
                }
                if (current != '.') continue;

                AddQualifiedNamePart(parts, name.Substring(segmentStart, index - segmentStart));
                segmentStart = index + 1;
            }

            AddQualifiedNamePart(parts, name.Substring(segmentStart));
            return parts.ToArray();
        }

        static void AddQualifiedNamePart(List<string> parts, string value)
        {
            value = value?.Trim();
            if (string.IsNullOrEmpty(value) == false) parts.Add(value);
        }

        static string GetTableKey(string catalog, string schema, string name) => (catalog ?? string.Empty) + "|" + (schema ?? string.Empty) + "|" + (name ?? string.Empty);
        static bool MatchesDatabase(string catalog, string[] databases, bool ignoreCase) =>
            databases == null || databases.Length == 0 || string.IsNullOrEmpty(catalog) ||
            databases.Any(a => NameEquals(UnquoteIdentifier(a?.Trim()), UnquoteIdentifier(catalog.Trim()), ignoreCase));

        bool IsCurrentDatabaseSelected(string[] databases, bool ignoreCase)
        {
            if (databases == null || databases.Length == 0) return true;
            // 空数据库名在 FreeSql 的 DbFirst 调用中表示当前连接库。
            if (databases.Any(string.IsNullOrWhiteSpace)) return true;

            string current = null;
            try
            {
                // 默认连接池每次创建独立的 SndbConnection，因此这里读取的是连接字符串
                // 指定的数据库；临时连接上的 USE 不会改变后续 DbFirst 请求的目标库。
                if (_orm?.Ado?.MasterPool != null)
                    using (var conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5)))
                    {
                        current = conn?.Value?.Database;
                        if (string.IsNullOrWhiteSpace(current)) current = conn?.Value?.DataSource;
                    }
            }
            catch { }

            // 无法取得当前库名称时，不能证明目录结果属于请求的数据库，
            // 因此返回空集合而不是泄漏当前库对象。
            if (string.IsNullOrWhiteSpace(current)) return false;
            current = UnquoteIdentifier(current.Trim());
            return databases.Any(item => NameEquals(UnquoteIdentifier(item?.Trim()), current, ignoreCase));
        }

        static string UnquoteIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            value = value.Trim();
            if ((value.StartsWith("\"") && value.EndsWith("\"")) || (value.StartsWith("`") && value.EndsWith("`"))) value = value.Substring(1, value.Length - 2);
            else if (value.StartsWith("[") && value.EndsWith("]")) value = value.Substring(1, value.Length - 2);
            return value.Replace("\"\"", "\"").Replace("``", "`").Replace("]]", "]");
        }

        static string GetString(DataRow row, string name, int index = -1)
        {
            var value = GetValue(row, name, index);
            return value == null || value == DBNull.Value ? null : Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        static int GetInt(DataRow row, string name, int index = -1)
        {
            var value = GetValue(row, name, index);
            if (value == null || value == DBNull.Value) return 0;
            if (value is int i) return i;
            if (value is long l) return l > int.MaxValue ? int.MaxValue : (int)l;
            if (value is short s) return s;
            return int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : 0;
        }

        static bool GetBool(DataRow row, string name, bool defaultValue, int index = -1)
        {
            var value = GetValue(row, name, index);
            if (value == null || value == DBNull.Value) return defaultValue;
            if (value is bool b) return b;
            if (value is byte bt) return bt != 0;
            if (value is int i) return i != 0;
            var text = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (bool.TryParse(text, out var parsed)) return parsed;
            return text == "1" || string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "y", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "t", StringComparison.OrdinalIgnoreCase);
        }

        static object GetValue(DataRow row, string name, int index)
        {
            if (row == null) return null;
            var column = row.Table.Columns.Cast<DataColumn>().FirstOrDefault(a => string.Equals(a.ColumnName, name, StringComparison.OrdinalIgnoreCase));
            return column != null ? row[column] : index >= 0 && index < row.ItemArray.Length ? row[index] : null;
        }

        static string GetClrTypeName(Type type)
        {
            if (type == typeof(bool)) return "boolean";
            if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong)) return "int64";
            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) return "float64";
            if (type == typeof(DateTime) || type == typeof(DateTimeOffset)) return "datetime";
            if (type == typeof(byte[])) return "blob";
            return "string";
        }

        public List<DbEnumInfo> GetEnumsByDatabase(params string[] database) => new List<DbEnumInfo>();
    }
}
