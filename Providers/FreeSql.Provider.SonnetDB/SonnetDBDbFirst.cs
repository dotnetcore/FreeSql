using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
            var dbtype = (column.DbTypeTextFull ?? column.DbTypeText ?? "").ToLowerInvariant();
            if (dbtype.Contains("float") || dbtype.Contains("double")) return (int)DbType.Double;
            if (dbtype.Contains("int") || string.Equals(column.Name, "time", StringComparison.OrdinalIgnoreCase)) return (int)DbType.Int64;
            if (dbtype.Contains("bool")) return (int)DbType.Boolean;
            if (dbtype.Contains("vector")) return (int)DbType.Object;
            return (int)DbType.String;
        }

        static readonly Dictionary<int, DbToCs> _dicDbToCs = new Dictionary<int, DbToCs>
        {
            { (int)DbType.Int64, new DbToCs("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },
            { (int)DbType.Double, new DbToCs("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
            { (int)DbType.Boolean, new DbToCs("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },
            { (int)DbType.String, new DbToCs("", "{0}", "{0}", "string", typeof(string), typeof(string), "{0}", "GetString") },
            { (int)DbType.Object, new DbToCs("", "{0}", "{0}", "object", typeof(object), typeof(object), "{0}", "GetValue") },
        };

        public string GetCsConvert(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? (column.IsNullable ? trydc.csConvert : trydc.csConvert.Replace("?", "")) : null;
        public string GetCsParse(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csParse : null;
        public string GetCsStringify(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csStringify : null;
        public string GetCsType(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? (column.IsNullable ? trydc.csType : trydc.csType.Replace("?", "")) : null;
        public Type GetCsTypeInfo(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csTypeInfo : null;
        public string GetCsTypeValue(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csTypeValue : null;
        public string GetDataReaderMethod(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.dataReaderMethod : null;

        public List<string> GetDatabases()
        {
            try { return _orm.Ado.Query<string>("SHOW DATABASES"); }
            catch { return new List<string>(); }
        }

        public bool ExistsTable(string name, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var tbname = _commonUtils.SplitTableName(name).LastOrDefault() ?? name;
            try
            {
                var names = _orm.Ado.Query<string>("SHOW MEASUREMENTS");
                return ignoreCase ? names.Any(a => string.Equals(a, tbname, StringComparison.OrdinalIgnoreCase)) : names.Any(a => a == tbname);
            }
            catch
            {
                try
                {
                    _orm.Ado.ExecuteDataTable($"SELECT * FROM {_commonUtils.QuoteSqlName(tbname)} LIMIT 0");
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public DbTableInfo GetTableByName(string name, bool ignoreCase = true) => GetTables(null, name, ignoreCase)?.FirstOrDefault();

        public List<DbTableInfo> GetTablesByDatabase(params string[] database) => GetTables(database, null, true);

        public List<DbTableInfo> GetTables(string[] database, string tablename, bool ignoreCase)
        {
            List<string> names;
            try
            {
                names = _orm.Ado.Query<string>("SHOW MEASUREMENTS");
            }
            catch
            {
                if (string.IsNullOrEmpty(tablename)) return new List<DbTableInfo>();
                var tbname = _commonUtils.SplitTableName(tablename).LastOrDefault() ?? tablename;
                return ExistsTable(tbname, ignoreCase) ? new List<DbTableInfo> { GetTableBySelectSchema(tbname) } : new List<DbTableInfo>();
            }
            if (!string.IsNullOrEmpty(tablename))
            {
                var tbname = _commonUtils.SplitTableName(tablename).LastOrDefault() ?? tablename;
                names = names.Where(a => ignoreCase ? string.Equals(a, tbname, StringComparison.OrdinalIgnoreCase) : a == tbname).ToList();
            }

            var tables = new List<DbTableInfo>();
            foreach (var name in names)
            {
                var table = new DbTableInfo { Name = name, Type = DbTableType.TABLE, Columns = new List<DbColumnInfo>() };
                table.Columns.Add(new DbColumnInfo
                {
                    Table = table,
                    Name = "time",
                    DbTypeText = "int64",
                    DbTypeTextFull = "time",
                    DbType = (int)DbType.Int64,
                    CsType = typeof(long),
                    IsNullable = false,
                    Position = 1
                });

                DataTable dt;
                try
                {
                    dt = _orm.Ado.ExecuteDataTable($"DESCRIBE MEASUREMENT {_commonUtils.QuoteSqlName(name)}");
                }
                catch
                {
                    tables.Add(GetTableBySelectSchema(name));
                    continue;
                }
                var pos = 2;
                foreach (DataRow row in dt.Rows)
                {
                    var columnName = GetValue(row, "column_name", 0);
                    if (string.IsNullOrEmpty(columnName)) continue;
                    var columnType = GetValue(row, "column_type", 1);
                    var dataType = GetValue(row, "data_type", 2);
                    var dbtype = new DbColumnInfo
                    {
                        Table = table,
                        Name = columnName,
                        DbTypeText = dataType,
                        DbTypeTextFull = $"{columnType} {dataType}".Trim(),
                        IsNullable = true,
                        Position = pos++
                    };
                    dbtype.DbType = GetDbType(dbtype);
                    dbtype.CsType = GetCsTypeInfo(dbtype);
                    table.Columns.Add(dbtype);
                }
                tables.Add(table);
            }
            return tables;
        }

        DbTableInfo GetTableBySelectSchema(string name)
        {
            var dt = _orm.Ado.ExecuteDataTable($"SELECT * FROM {_commonUtils.QuoteSqlName(name)} LIMIT 0");
            var table = new DbTableInfo { Name = name, Type = DbTableType.TABLE, Columns = new List<DbColumnInfo>() };
            var pos = 1;
            foreach (DataColumn column in dt.Columns)
            {
                var dbtype = new DbColumnInfo
                {
                    Table = table,
                    Name = column.ColumnName,
                    DbTypeText = column.DataType == typeof(long) ? "int64" :
                        column.DataType == typeof(double) || column.DataType == typeof(float) || column.DataType == typeof(decimal) ? "float64" :
                        column.DataType == typeof(bool) ? "boolean" :
                        column.DataType == typeof(string) ? "string" : "object",
                    IsNullable = !string.Equals(column.ColumnName, "time", StringComparison.OrdinalIgnoreCase),
                    Position = pos++
                };
                dbtype.DbType = GetDbType(dbtype);
                dbtype.CsType = GetCsTypeInfo(dbtype);
                table.Columns.Add(dbtype);
            }
            return table;
        }

        static string GetValue(DataRow row, string columnName, int index)
        {
            if (row.Table.Columns.Contains(columnName)) return string.Concat(row[columnName]);
            return index < row.ItemArray.Length ? string.Concat(row[index]) : null;
        }

        public List<DbEnumInfo> GetEnumsByDatabase(params string[] database) => new List<DbEnumInfo>();
    }
}
