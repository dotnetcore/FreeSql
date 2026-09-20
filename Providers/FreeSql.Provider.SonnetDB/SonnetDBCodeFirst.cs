// SonnetDB CodeFirst 实现。
using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.DatabaseModel;
using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SonnetDB.Documents;
using SonnetDB.Sql;
using SndbGeoPoint = global::SonnetDB.Model.GeoPoint;

namespace FreeSql.SonnetDB
{
    class SonnetDBCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {
        public SonnetDBCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        static readonly object _dicCsToDbLock = new object();

        /// <summary>
        /// C# 完整类型名 → SonnetDB 类型映射表。
        /// <para>DateTime / DateTimeOffset 映射为 INT（Unix 毫秒），不使用 SonnetDB 原生时间类型。</para>
        /// <para>所有整型（含无符号）均映射为 INT，SonnetDB 内部存储为 int64。</para>
        /// </summary>
        static readonly Dictionary<string, CsToDb<DbType>> _dicCsToDb = new Dictionary<string, CsToDb<DbType>>
        {
            { typeof(bool).FullName,            CsToDb.New(DbType.Boolean, "BOOL",  "BOOL",  null,  false, false) },
            { typeof(bool?).FullName,           CsToDb.New(DbType.Boolean, "BOOL",  "BOOL",  null,  true,  null) },
            { typeof(short).FullName,           CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, 0) },
            { typeof(short?).FullName,          CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(int).FullName,             CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, 0) },
            { typeof(int?).FullName,            CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(long).FullName,            CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, 0L) },
            { typeof(long?).FullName,           CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(byte).FullName,            CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, 0) },
            { typeof(byte?).FullName,           CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(sbyte).FullName,           CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, 0) },
            { typeof(sbyte?).FullName,          CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(ushort).FullName,          CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, 0) },
            { typeof(ushort?).FullName,         CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(uint).FullName,            CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, 0) },
            { typeof(uint?).FullName,           CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(ulong).FullName,           CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, 0) },
            { typeof(ulong?).FullName,          CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(float).FullName,           CsToDb.New(DbType.Double,  "FLOAT", "FLOAT", false, false, 0) },
            { typeof(float?).FullName,          CsToDb.New(DbType.Double,  "FLOAT", "FLOAT", false, true,  null) },
            { typeof(double).FullName,          CsToDb.New(DbType.Double,  "FLOAT", "FLOAT", false, false, 0) },
            { typeof(double?).FullName,         CsToDb.New(DbType.Double,  "FLOAT", "FLOAT", false, true,  null) },
            { typeof(decimal).FullName,         CsToDb.New(DbType.Double,  "FLOAT", "FLOAT", false, false, 0) },
            { typeof(decimal?).FullName,        CsToDb.New(DbType.Double,  "FLOAT", "FLOAT", false, true,  null) },
            { typeof(string).FullName,          CsToDb.New(DbType.String,  "STRING","STRING",false, true,  null) },
            { typeof(char).FullName,            CsToDb.New(DbType.String,  "STRING","STRING",false, true,  null) },
            { typeof(char?).FullName,           CsToDb.New(DbType.String,  "STRING","STRING",false, true,  null) },
            { typeof(Guid).FullName,            CsToDb.New(DbType.String,  "STRING","STRING",false, false, Guid.Empty) },
            { typeof(Guid?).FullName,           CsToDb.New(DbType.String,  "STRING","STRING",false, true,  null) },
            // BLOB 仅用于关系表模型；时序测量实体仍使用标量 FIELD 类型。
            { typeof(byte[]).FullName,          CsToDb.New(DbType.Binary,  "BLOB",  "BLOB",  false, true,  null) },
            // VECTOR 仅用于显式声明维度的时序 FIELD；关系表会在 DDL 阶段拒绝。
            { typeof(float[]).FullName,          CsToDb.New(DbType.Object,  "VECTOR", "VECTOR", false, true,  null) },
            // DateTime / DateTimeOffset 存储为 Unix 毫秒整数（与隐式 time 列语义一致）。
            { typeof(DateTime).FullName,        CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, new DateTime(1970, 1, 1)) },
            { typeof(DateTime?).FullName,       CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(DateTimeOffset).FullName,  CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, DateTimeOffset.UnixEpoch) },
            { typeof(DateTimeOffset?).FullName, CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            // SonnetDB 原生 GEOPOINT；关系表建表路径会明确拒绝该类型。
            { typeof(SndbGeoPoint).FullName,    CsToDb.New(DbType.String,  "GEOPOINT", "GEOPOINT", false, false, new SndbGeoPoint(0, 0)) },
            { typeof(SndbGeoPoint?).FullName,   CsToDb.New(DbType.String,  "GEOPOINT", "GEOPOINT", false, true,  null) },
            // JSON DOM 在通用 FreeSql 类型扫描阶段按 STRING 处理；关系表 DDL
            // 会在 GetTableDataType 中恢复为 JSON，时序测量则保持 FIELD STRING。
            { typeof(JsonDocument).FullName,   CsToDb.New(DbType.String, "STRING", "STRING", false, true,  null) },
            { typeof(JsonElement).FullName,    CsToDb.New(DbType.String, "STRING", "STRING", false, false, default(JsonElement)) },
            { typeof(JsonElement?).FullName,   CsToDb.New(DbType.String, "STRING", "STRING", false, true,  null) },
            { typeof(JsonNode).FullName,       CsToDb.New(DbType.String, "STRING", "STRING", false, true,  null) },
            { typeof(JsonObject).FullName,     CsToDb.New(DbType.String, "STRING", "STRING", false, true,  null) },
            { typeof(JsonArray).FullName,      CsToDb.New(DbType.String, "STRING", "STRING", false, true,  null) },
            { typeof(JsonValue).FullName,      CsToDb.New(DbType.String, "STRING", "STRING", false, true,  null) },
        };

        public override DbInfoResult GetDbInfo(Type type)
        {
            if (type != null && type.NullableTypeOrThis() == typeof(BigInteger))
                throw new NotSupportedException(
                    "SonnetDB 3.1 不支持 BigInteger/任意精度整数；请改用 long/ulong（SonnetDB INT 为 int64），" +
                    "或等待 SonnetDB 提供任意精度数值类型。" );
            if (_dicCsToDb.TryGetValue(type.FullName, out var trydc))
                return new DbInfoResult((int)trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable, trydc.defaultValue);

            if (type.IsArray) return null;
            var enumType = type.IsEnum ? type : null;
            if (enumType == null && type.IsNullableType() && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments.First().IsEnum)
                enumType = type.GenericTypeArguments.First();
            if (enumType != null)
            {
                // 枚举类型统一映射为 INT，存储枚举底层整数值。
                var newItem = CsToDb.New(DbType.Int64, "INT", "INT", false, type.IsEnum ? false : true, enumType.CreateInstanceGetDefaultValue());
                if (_dicCsToDb.ContainsKey(type.FullName) == false)
                {
                    lock (_dicCsToDbLock)
                    {
                        if (_dicCsToDb.ContainsKey(type.FullName) == false) _dicCsToDb.Add(type.FullName, newItem);
                    }
                }
                return new DbInfoResult((int)newItem.type, newItem.dbtype, newItem.dbtypeFull, newItem.isnullable, newItem.defaultValue);
            }
            return null;
        }

        /// <summary>
        /// 为实体生成 SonnetDB DDL。未标记的实体沿用历史时序测量模型；
        /// 标记 <c>[SonnetDBTable]</c> 的实体使用 SonnetDB 3.1 关系表模型。
        /// </summary>
        protected override string GetComparisonDDLStatements(params TypeSchemaAndName[] objects)
        {
            var sb = new StringBuilder();
            foreach (var obj in objects)
            {
                var tb = obj.tableSchema;
                var typeName = tb?.Type?.FullName ?? obj?.tableSchema?.Type?.FullName ?? "未知类型";
                if (tb == null) throw new InvalidOperationException($"SonnetDB 无法迁移类型 '{typeName}'。");
                if (tb.Columns.Any() == false) throw new InvalidOperationException($"SonnetDB 类型 '{typeName}' 没有可映射的列。");

                var tbname = string.IsNullOrEmpty(obj.tableName) ? tb.DbName : obj.tableName;
                var tableParts = _commonUtils.SplitTableName(tbname);
                tbname = tableParts.LastOrDefault() ?? tbname;

                // 模型标记必须显式指定。没有标记的实体（包括含字符串属性的实体）
                // 必须继续按时序测量处理，以保持原有提供程序模型兼容性。
                if (SonnetDBModel.IsTable(tb))
                {
                    AppendTableDDL(sb, tb, tbname);
                    continue;
                }

                // 若时序测量已存在则跳过（SonnetDB CodeFirst 仅支持建表，不支持 ALTER）。
                if (ExistsMeasurement(tbname)) continue;

                var fieldCount = 0;
                var ddlColumns = new List<string>();
                foreach (var col in tb.ColumnsByPosition)
                {
                    // time 列是 SonnetDB 隐式内置列，无需在创建时序测量的语句中声明。
                    if (IsTimeColumn(col)) continue;
                    if (col.Attribute.MapType.NullableTypeOrThis() == typeof(byte[]))
                        throw new NotSupportedException(
                            $"SonnetDB 时序测量列 '{col.Attribute.Name}' 不能使用 BLOB；" +
                            "如需关系表 BLOB 支持，请为实体添加 [SonnetDBTable]。");
                    var ddl = GetColumnDefinition(tb, col);
                    if (ddl.IndexOf(" FIELD", StringComparison.OrdinalIgnoreCase) >= 0) fieldCount++;
                    ddlColumns.Add(ddl);
                }

                // SonnetDB 要求每张时序测量至少包含一个 FIELD 列。
                if (fieldCount == 0)
                    throw new Exception($"SonnetDB 时序测量 '{tbname}' 至少需要一个 FIELD 列；" +
                        "请为属性添加 [SonnetDBField]，或将 Column.DbType 设置为 \"FIELD <type>\"。");

                if (sb.Length > 0) sb.AppendLine();
                sb.Append("CREATE MEASUREMENT ").Append(_commonUtils.QuoteSqlName(tbname)).Append(" (");
                for (var a = 0; a < ddlColumns.Count; a++)
                {
                    if (a > 0) sb.Append(",");
                    sb.AppendLine().Append("  ").Append(ddlColumns[a]);
                }
                sb.AppendLine().Append(");");
            }

            return sb.Length == 0 ? null : sb.ToString();
        }

        /// <summary>
        /// SonnetDB ADO 命令每次只执行一条 SQL。FreeSql 为便于检查和 AOP
        /// 仍返回完整 DDL 文本，因此这里按词法分析器识别出的语句边界逐条执行，
        /// 不把以分号连接的多条语句一次性传给同一个命令。SonnetDB 词法分析器
        /// 能够区分引号内默认值中的分号，避免误判语句边界。
        /// </summary>
        public override int ExecuteDDLStatements(string ddl)
        {
            if (string.IsNullOrWhiteSpace(ddl)) return 0;
            if (_orm.Ado.TransactionCurrentThread != null)
                throw new InvalidOperationException("SonnetDB 3.1 不支持在事务中执行 DDL；请先提交或回滚当前事务。");

            var tokens = SqlLexer.Tokenize(ddl);
            var start = 0;
            var hasStatementToken = false;
            var affrows = 0;
            foreach (var token in tokens)
            {
                if (token.Kind == TokenKind.EndOfFile) break;
                if (token.Kind != TokenKind.Semicolon)
                {
                    hasStatementToken = true;
                    continue;
                }

                if (hasStatementToken)
                    affrows += ExecuteDDLStatement(ddl.Substring(start, token.Position - start));
                start = token.Position + 1;
                hasStatementToken = false;
            }
            if (hasStatementToken)
                affrows += ExecuteDDLStatement(ddl.Substring(start));
            return affrows;
        }

        int ExecuteDDLStatement(string sql)
        {
            sql = sql?.Trim();
            return string.IsNullOrEmpty(sql) ? 0 : _orm.Ado.ExecuteNonQuery(CommandType.Text, sql);
        }

        /// <summary>
        /// 生成关系表的创建和安全差异迁移 DDL。不会删除未映射的列或索引，
        /// 且不会修改主键、自增列和行版本列的定义。
        /// </summary>
        void AppendTableDDL(StringBuilder sb, TableInfo tb, string tbname)
        {
            if (tb.Primarys == null || tb.Primarys.Length == 0)
                throw new InvalidOperationException(
                    $"SonnetDB 关系表 '{tbname}' 必须至少声明一个主键列。");

            var existingName = tbname;
            var tableExists = ExistsTable(tbname);
            if (!tableExists)
            {
                var oldName = string.Equals(GetObjectName(tb.DbName), tbname, StringComparison.OrdinalIgnoreCase)
                    ? GetObjectName(tb.DbOldName)
                    : null;
                if (!string.IsNullOrWhiteSpace(oldName) && ExistsTable(oldName))
                {
                    AppendDdlSeparator(sb);
                    sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(oldName))
                        .Append(" RENAME TO ").Append(_commonUtils.QuoteSqlName(tbname)).Append(";");
                    existingName = oldName;
                    tableExists = true;
                }
            }

            DbTableInfo existingTable = null;
            if (!tableExists)
            {
                var ddlColumns = new List<string>();
                foreach (var col in tb.ColumnsByPosition)
                    ddlColumns.Add(GetTableColumnDefinition(col));

                AppendDdlSeparator(sb);
                sb.Append("CREATE TABLE IF NOT EXISTS ").Append(_commonUtils.QuoteSqlName(tbname)).Append(" (");
                for (var a = 0; a < ddlColumns.Count; a++)
                {
                    sb.AppendLine().Append("  ").Append(ddlColumns[a]).Append(",");
                }

                sb.AppendLine().Append("  PRIMARY KEY (");
                for (var a = 0; a < tb.Primarys.Length; a++)
                {
                    if (a > 0) sb.Append(", ");
                    sb.Append(_commonUtils.QuoteSqlName(tb.Primarys[a].Attribute.Name));
                }
                sb.AppendLine(")").Append(");");
            }
            else
            {
                existingTable = _orm.DbFirst.GetTableByName(existingName);
                if (existingTable == null)
                    throw new InvalidOperationException($"SonnetDB 无法读取关系表 '{existingName}' 的结构，已取消迁移。");
                AppendTableAlterDDL(sb, tb, tbname, existingTable);
            }

            // SonnetDB 普通索引不接受 ASC/DESC 修饰符；JSON 路径索引还需要比较路径。
            // 新表创建失败后仍可安全重试索引创建。
            foreach (var index in tb.Indexes ?? Array.Empty<IndexInfo>())
            {
                if (index == null || index.Columns == null || index.Columns.Length == 0 || string.IsNullOrWhiteSpace(index.Name))
                    continue;
                var indexName = ReplaceIndexName(index.Name, tbname);
                var existingIndex = FindIndex(existingTable, indexName);
                if (existingIndex != null && !IsSameIndex(existingIndex, index))
                {
                    AppendDdlSeparator(sb);
                    sb.Append("DROP INDEX ").Append(_commonUtils.QuoteSqlName(indexName))
                        .Append(" ON ").Append(_commonUtils.QuoteSqlName(tbname)).Append(";");
                    existingIndex = null;
                }
                if (existingIndex == null)
                    AppendCreateIndexDDL(sb, tbname, indexName, index);
            }
        }

        void AppendTableAlterDDL(StringBuilder sb, TableInfo tb, string tbname, DbTableInfo existingTable)
        {
            var existingColumns = existingTable.Columns
                .Where(a => a != null && string.IsNullOrWhiteSpace(a.Name) == false)
                .ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var col in tb.ColumnsByPosition)
            {
                var name = col.Attribute.Name;
                if (!existingColumns.TryGetValue(name, out var existing))
                {
                    var oldName = col.Attribute.OldName;
                    if (!string.IsNullOrWhiteSpace(oldName) && existingColumns.TryGetValue(oldName, out existing))
                    {
                        if (existing.IsPrimary)
                            throw new NotSupportedException($"SonnetDB 不支持重命名主键列 '{oldName}'；请重建关系表 '{tbname}'。");
                        AppendDdlSeparator(sb);
                        sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tbname))
                            .Append(" RENAME COLUMN ").Append(_commonUtils.QuoteSqlName(oldName))
                            .Append(" TO ").Append(_commonUtils.QuoteSqlName(name)).Append(";");
                    }
                    else
                    {
                        AppendAddColumnDDL(sb, tbname, col);
                        continue;
                    }
                }
                AppendAlterColumnDDL(sb, tbname, col, existing);
            }
        }

        void AppendAddColumnDDL(StringBuilder sb, string tbname, ColumnInfo col)
        {
            var attr = col.Attribute;
            if (attr.IsPrimary || attr.IsIdentity || attr.IsVersion)
                throw new NotSupportedException($"SonnetDB 不支持为既有关系表新增主键、自增或行版本列 '{attr.Name}'；请重建表。");
            if (!attr.IsNullable && string.IsNullOrWhiteSpace(GetTableDefaultExpression(col)))
                throw new InvalidOperationException($"SonnetDB 为既有关系表新增 NOT NULL 列 '{attr.Name}' 时必须配置 DEFAULT。");

            AppendDdlSeparator(sb);
            sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tbname)).Append(" ADD COLUMN ")
                .Append(GetTableColumnDefinition(col)).Append(";");
        }

        void AppendAlterColumnDDL(StringBuilder sb, string tbname, ColumnInfo col, DbColumnInfo existing)
        {
            var attr = col.Attribute;
            var expectedType = GetTableDataType(col);
            var expectedNullable = attr.IsNullable && !attr.IsPrimary && !attr.IsIdentity && !attr.IsVersion;
            var expectedDefault = GetTableDefaultExpression(col);
            var actualType = NormalizeTableTypeAlias(existing.DbTypeTextFull) ?? NormalizeTableTypeAlias(existing.DbTypeText);
            var actualVersion = (existing.DbTypeTextFull ?? string.Empty).IndexOf("ROWVERSION", StringComparison.OrdinalIgnoreCase) >= 0;
            var structuralMismatch = !string.Equals(expectedType, actualType, StringComparison.OrdinalIgnoreCase)
                || expectedNullable != existing.IsNullable;
            var defaultMismatch = !IsSameDefault(existing.DefaultValue, expectedDefault);

            if (attr.IsPrimary != existing.IsPrimary || attr.IsIdentity != existing.IsIdentity || attr.IsVersion != actualVersion)
                throw new NotSupportedException($"SonnetDB 不支持通过 CodeFirst 修改关系表列 '{attr.Name}' 的主键、自增或行版本属性；请重建表。");
            if (!structuralMismatch && !defaultMismatch) return;
            if (attr.IsPrimary || attr.IsIdentity || attr.IsVersion)
                throw new NotSupportedException($"SonnetDB 不支持通过 CodeFirst 修改关系表特殊列 '{attr.Name}' 的类型、空值约束或默认值；请重建表。");
            if (!expectedNullable && string.IsNullOrWhiteSpace(expectedDefault))
                throw new InvalidOperationException($"SonnetDB 将既有列 '{attr.Name}' 设为 NOT NULL 时必须配置 DEFAULT。");

            AppendDdlSeparator(sb);
            sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tbname)).Append(" ALTER COLUMN ")
                .Append(_commonUtils.QuoteSqlName(attr.Name)).Append(" TYPE ").Append(expectedType)
                .Append(expectedNullable ? " NULL" : " NOT NULL")
                .Append(string.IsNullOrWhiteSpace(expectedDefault) ? " DROP DEFAULT" : " SET DEFAULT " + expectedDefault)
                .Append(";");
        }

        void AppendCreateIndexDDL(StringBuilder sb, string tbname, string indexName, IndexInfo index)
        {
            AppendDdlSeparator(sb);
            if (!string.IsNullOrWhiteSpace(index.JsonPath))
            {
                if (index.IsUnique)
                    throw new NotSupportedException("SonnetDB JSON 路径索引不支持唯一约束；请去掉 IsUnique。");
                if (index.IndexMethod != IndexMethod.B_Tree)
                    throw new NotSupportedException("SonnetDB JSON 路径索引不支持指定索引方法；请使用默认索引方法。");
                if (index.Columns == null || index.Columns.Length != 1)
                    throw new NotSupportedException("SonnetDB JSON 路径索引必须只包含一个 JSON 列。");
                var jsonColumn = index.Columns[0]?.Column;
                if (jsonColumn == null || !string.Equals(GetTableDataType(jsonColumn), "JSON", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"SonnetDB JSON 路径索引列 '{jsonColumn?.Attribute?.Name ?? "未知列"}' 必须是 JSON 类型。");

                sb.Append("CREATE JSON INDEX IF NOT EXISTS ").Append(_commonUtils.QuoteSqlName(indexName))
                    .Append(" ON ").Append(_commonUtils.QuoteSqlName(tbname)).Append(" (")
                    .Append(_commonUtils.QuoteSqlName(jsonColumn.Attribute.Name)).Append(", ")
                    .Append(QuoteSqlString(NormalizeJsonPath(index.JsonPath))).Append(");");
                return;
            }

            sb.Append("CREATE ");
            if (index.IsUnique) sb.Append("UNIQUE ");
            sb.Append("INDEX IF NOT EXISTS ").Append(_commonUtils.QuoteSqlName(indexName))
                .Append(" ON ").Append(_commonUtils.QuoteSqlName(tbname)).Append(" (");
            for (var a = 0; a < index.Columns.Length; a++)
            {
                if (a > 0) sb.Append(", ");
                sb.Append(_commonUtils.QuoteSqlName(index.Columns[a].Column.Attribute.Name));
            }
            sb.Append(");");
        }

        static DbIndexInfo FindIndex(DbTableInfo table, string name) => table?.Indexes
            .Concat(table.Uniques ?? new List<DbIndexInfo>())
            .FirstOrDefault(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));

        static bool IsSameIndex(DbIndexInfo existing, IndexInfo expected)
        {
            if (existing.IsUnique != expected.IsUnique || existing.Columns.Count != expected.Columns.Length) return false;
            if (!string.Equals(NormalizeJsonPath(existing.JsonPath), NormalizeJsonPath(expected.JsonPath), StringComparison.Ordinal)) return false;
            for (var a = 0; a < existing.Columns.Count; a++)
            {
                var expectedColumn = expected.Columns[a].Column.Attribute;
                var actualName = existing.Columns[a].Column.Name;
                if (!string.Equals(actualName, expectedColumn.Name, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(actualName, expectedColumn.OldName, StringComparison.OrdinalIgnoreCase)) return false;
            }
            return true;
        }

        static string NormalizeJsonPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            try
            {
                return JsonPath.Parse(path.Trim()).Text;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is FormatException)
            {
                throw new ArgumentException(
                    $"SonnetDB JSON 路径 '{path}' 无效；具体解析信息请查看内部异常。",
                    nameof(path), ex);
            }
        }

        static string QuoteSqlString(string value) => "'" + (value ?? string.Empty).Replace("'", "''") + "'";

        static bool IsSameDefault(string actual, string expected)
        {
            if (string.IsNullOrWhiteSpace(actual)) return string.IsNullOrWhiteSpace(expected);
            if (string.IsNullOrWhiteSpace(expected)) return false;
            return string.Equals(NormalizeDefault(actual), NormalizeDefault(expected), StringComparison.Ordinal);
        }

        // Whitespace and keyword casing outside literals are insignificant in SQL,
        // while the contents of a quoted string are part of the default value.  The
        // old Regex + OrdinalIgnoreCase comparison treated DEFAULT 'a' and DEFAULT
        // 'A' as equal and silently skipped a required migration.
        static string NormalizeDefault(string value)
        {
            var result = new StringBuilder(value.Length);
            var quoted = '\0';
            var pendingSpace = false;
            for (var index = 0; index < value.Length; index++)
            {
                var current = value[index];
                if (quoted != '\0')
                {
                    result.Append(current);
                    if (current == quoted)
                    {
                        // SQL escapes a quote by doubling it.
                        if (index + 1 < value.Length && value[index + 1] == quoted)
                        {
                            result.Append(value[++index]);
                        }
                        else quoted = '\0';
                    }
                    continue;
                }

                if (current == '\'' || current == '"')
                {
                    if (pendingSpace && result.Length > 0) result.Append(' ');
                    pendingSpace = false;
                    quoted = current;
                    result.Append(current);
                    continue;
                }
                if (char.IsWhiteSpace(current))
                {
                    pendingSpace = result.Length > 0;
                    continue;
                }
                if (pendingSpace && result.Length > 0) result.Append(' ');
                pendingSpace = false;
                result.Append(char.ToUpperInvariant(current));
            }

            return result.ToString().Trim('(', ')').Trim();
        }

        static string GetObjectName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var parts = name.Split('.');
            var result = parts[parts.Length - 1].Trim();
            if (result.Length > 1 && ((result[0] == '"' && result[result.Length - 1] == '"') || (result[0] == '`' && result[result.Length - 1] == '`')))
                result = result.Substring(1, result.Length - 2);
            return result;
        }

        static void AppendDdlSeparator(StringBuilder sb)
        {
            if (sb.Length > 0) sb.AppendLine();
        }

        /// <summary>
        /// 判断关系表是否已经存在。SHOW TABLES 是 SonnetDB 3.1 的稳定元数据
        /// 接口；旧服务或驱动不支持时，回退到 DESCRIBE TABLE。
        /// </summary>
        bool ExistsTable(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            try
            {
                var names = _orm.Ado.Query<string>("SHOW TABLES");
                return names.Any(a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                try
                {
                    _orm.Ado.ExecuteDataTable($"DESCRIBE TABLE {_commonUtils.QuoteSqlName(name)}");
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 通过 SonnetDB 的时序测量元数据接口判断对象是否存在。
        /// 不能使用通用 SELECT 探测：同名关系表也可能被 SELECT 成功，
        /// 从而误判为时序测量。旧服务不支持 SHOW 时，回退到
        /// DESCRIBE MEASUREMENT；该语句只解析时序测量。
        /// </summary>
        bool ExistsMeasurement(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            try
            {
                var names = _orm.Ado.Query<string>("SHOW MEASUREMENTS");
                return names.Any(a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                try
                {
                    _orm.Ado.ExecuteDataTable(
                        $"DESCRIBE MEASUREMENT {_commonUtils.QuoteSqlName(name)}");
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 生成关系表列定义。关系表和时序测量的类型空间不同，不能直接
        /// 复用 <see cref="GetColumnDefinition"/>：例如 DateTime 在时序测量
        /// 中是 Unix 毫秒 INT，而在关系表中应声明为 DATETIME。
        /// </summary>
        string GetTableColumnDefinition(ColumnInfo col)
        {
            var attr = col.Attribute;
            var dataType = GetTableDataType(col);
            var isIdentity = attr.IsIdentity;
            var isVersion = attr.IsVersion;

            if ((isIdentity || isVersion) && string.Equals(dataType, "INT", StringComparison.OrdinalIgnoreCase) == false)
                throw new InvalidOperationException(
                    $"SonnetDB 关系表列 '{attr.Name}' 必须使用 INT 才能声明 " +
                    (isIdentity ? "AUTO_INCREMENT" : "ROWVERSION") + "。" );
            if (isIdentity && isVersion)
                throw new InvalidOperationException(
                    $"SonnetDB 关系表列 '{attr.Name}' 不能同时声明 AUTO_INCREMENT 和 ROWVERSION。");

            var nullable = attr.IsNullable && !attr.IsPrimary && !isIdentity && !isVersion;
            var sb = new StringBuilder();
            sb.Append(_commonUtils.QuoteSqlName(attr.Name)).Append(' ').Append(dataType);
            if (isIdentity)
                sb.Append(" AUTO_INCREMENT");
            else if (isVersion)
                sb.Append(" ROWVERSION");
            else
                sb.Append(nullable ? " NULL" : " NOT NULL");

            var defaultExpression = GetTableDefaultExpression(col);
            if (string.IsNullOrWhiteSpace(defaultExpression) == false)
            {
                if (isIdentity || isVersion)
                    throw new InvalidOperationException(
                        $"SonnetDB 关系表列 '{attr.Name}' 不能与 " +
                        (isIdentity ? "AUTO_INCREMENT" : "ROWVERSION") + " 同时声明 DEFAULT。" );
                sb.Append(" DEFAULT ").Append(defaultExpression);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 将 FreeSql 列映射为 SonnetDB 关系表类型之一。为保证可移植性，接受
        /// 常见 DbType 别名；对于仅时序测量支持或不支持的类型直接拒绝，
        /// 不静默生成错误 DDL。
        /// </summary>
        string GetTableDataType(ColumnInfo col)
        {
            var attr = col.Attribute;
            var mapType = (attr.MapType ?? col.CsType)?.NullableTypeOrThis();
            if (mapType == null)
                throw new InvalidOperationException($"SonnetDB 无法推断列 '{attr.Name}' 的类型。");

            // SonnetDB 的 JSON 仅属于关系表类型；GetDbInfo 将 JSON DOM
            // 暂映射为 STRING 以便 FreeSql 扫描，关系表建表时再恢复为 JSON。
            if (IsJsonClrType(mapType)) return "JSON";

            // 日期时间和二进制 .NET 类型在本提供程序中有不同的历史映射，
            // 因此先处理它们，再读取生成的 DbType。
            if (mapType == typeof(DateTime) || mapType == typeof(DateTimeOffset)) return "DATETIME";
            if (mapType == typeof(byte[])) return "BLOB";

            var raw = StripTableModifiers(attr.DbType);
            var explicitType = NormalizeTableTypeAlias(raw);
            if (explicitType != null)
            {
                if (explicitType == "VECTOR" || explicitType == "GEOPOINT")
                    throw new NotSupportedException(
                        $"SonnetDB 关系表不支持 {explicitType} 列 '{attr.Name}'。");
                return explicitType;
            }

            if (mapType == typeof(bool)) return "BOOL";
            if (mapType == typeof(string) || mapType == typeof(char) || mapType == typeof(Guid)) return "STRING";
            if (mapType.IsEnum || mapType.IsNumberType()) return
                mapType == typeof(float) || mapType == typeof(double) || mapType == typeof(decimal)
                    ? "FLOAT"
                    : "INT";

            if (mapType.IsArray)
                throw new NotSupportedException(
                    $"SonnetDB 关系表列 '{attr.Name}' 使用了不支持的数组类型 '{mapType.FullName}'。");
            throw new NotSupportedException(
                $"SonnetDB 关系表列 '{attr.Name}' 的类型 '{mapType.FullName}' 不受支持；" +
                "请使用 Column(DbType = \"JSON\") 或 Column(DbType = \"BLOB\")。" );
        }

        static bool IsJsonClrType(Type type)
        {
            type = type?.NullableTypeOrThis();
            return type == typeof(JsonDocument)
                || type == typeof(JsonElement)
                || type == typeof(JsonNode)
                || type == typeof(JsonObject)
                || type == typeof(JsonArray)
                || type == typeof(JsonValue);
        }

        /// <summary>
        /// 将其他提供程序常见的类型别名规范化为 SonnetDB 的精确关系表类型名。
        /// 未知类型返回 null，以便后续使用 .NET 类型映射。
        /// </summary>
        static string NormalizeTableTypeAlias(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return null;
            var normalized = type.Trim().ToUpperInvariant();
            if (normalized.Contains("JSON")) return "JSON";
            if (normalized.Contains("BLOB") || normalized.Contains("BYTEA") || normalized.Contains("VARBINARY")) return "BLOB";
            if (normalized.Contains("DATETIME") || normalized.Contains("TIMESTAMP")) return "DATETIME";
            if (normalized.Contains("FLOAT") || normalized.Contains("DOUBLE") || normalized.Contains("REAL") || normalized.Contains("DECIMAL") || normalized.Contains("NUMERIC")) return "FLOAT";
            if (normalized.Contains("BOOL") || normalized.Contains("BOOLEAN")) return "BOOL";
            if (normalized.Contains("BIGINT") || normalized.Contains("INTEGER") || normalized.Contains("INT64") || normalized == "INT" || normalized.StartsWith("INT ")) return "INT";
            if (normalized.Contains("STRING") || normalized.Contains("VARCHAR") || normalized.Contains("TEXT") || normalized.Contains("CHAR")) return "STRING";
            if (normalized.Contains("VECTOR")) return "VECTOR";
            if (normalized.Contains("GEOPOINT")) return "GEOPOINT";
            return null;
        }

        /// <summary>
        /// 去除 FreeSql/SQL 修饰符并保留基础类型。默认值单独解析，
        /// 因为默认表达式可能包含空格或函数调用。
        /// </summary>
        static string StripTableModifiers(string dbType)
        {
            if (string.IsNullOrWhiteSpace(dbType)) return null;
            var normalized = Regex.Replace(dbType.Trim(), @"\bDEFAULT\b.*$", "", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, @"\bROWVERSION\b|\bAUTO[_]?INCREMENT\b|\bAUTOINCREMENT\b|\bIDENTITY\b", "", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, @"\s+NOT\s+NULL\b|\s+NULL\b", "", RegexOptions.IgnoreCase);
            return Regex.Replace(normalized, @"\s+", " ").Trim();
        }

        /// <summary>
        /// 只读取显式配置的默认值。DbDefaultValue 还可能包含不可空属性的
        /// .NET 零值，这些零值不能意外成为关系表的数据库 DEFAULT。
        /// </summary>
        static string GetTableDefaultExpression(ColumnInfo col)
        {
            var attr = col.Attribute;
            if (attr.IsIdentity || attr.IsVersion) return null;
            var dbType = attr.DbType ?? string.Empty;
            var match = Regex.Match(dbType,
                @"\bDEFAULT\s+(.+?)(?=\s+(?:NOT\s+NULL|NULL|ROWVERSION|AUTO[_]?INCREMENT|AUTOINCREMENT|IDENTITY)\b\s*$|\s*$)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success) return match.Groups[1].Value.Trim();
            if (string.IsNullOrWhiteSpace(attr.InsertValueSql) == false) return attr.InsertValueSql.Trim();
            if (attr.ServerTime == DateTimeKind.Local) return "CURRENT_DATETIME()";
            if (attr.ServerTime == DateTimeKind.Utc) return "CURRENT_UTC_DATETIME()";
            return null;
        }

        /// <summary>
        /// 为单列生成 DDL 片段，格式为 <c>"colName" TAG</c> 或 <c>"colName" FIELD FLOAT</c> 等。
        /// <para>判断列角色的优先级：显式 DbType 前缀 → [SonnetDBTag]/[SonnetDBField] 特性 → 类型推断。</para>
        /// </summary>
        string GetColumnDefinition(TableInfo tb, ColumnInfo col)
        {
            var quotedName = _commonUtils.QuoteSqlName(col.Attribute.Name);
            var dbType = (col.Attribute.DbType ?? "").Trim();
            if ((col.Attribute.MapType ?? col.CsType)?.NullableTypeOrThis() == typeof(float[])
                && dbType.IndexOf("VECTOR", StringComparison.OrdinalIgnoreCase) < 0)
                throw new NotSupportedException(
                    $"SonnetDB VECTOR 列 '{col.Attribute.Name}' 必须显式声明维度，例如 DbType = \"FIELD VECTOR(3)\"。");
            // 如果 DbType 已经显式包含 TAG 或 FIELD 前缀，直接规范化后使用。
            if (dbType.StartsWith("TAG", StringComparison.OrdinalIgnoreCase) ||
                dbType.StartsWith("FIELD", StringComparison.OrdinalIgnoreCase))
                return $"{quotedName} {NormalizeColumnDefinition(dbType)}";

            // 根据特性或类型推断列角色。
            if (IsTagColumn(tb, col)) return $"{quotedName} TAG";
            return $"{quotedName} FIELD {NormalizeFieldType(dbType)}";
        }

        /// <summary>
        /// 判断列是否应映射为 TAG（索引字符串维度）。
        /// <para>优先级：[SonnetDBTag] > [SonnetDBField] > 类型推断（string/char/Guid → TAG）。</para>
        /// </summary>
        bool IsTagColumn(TableInfo tb, ColumnInfo col)
        {
            // JSON 是测量模型中的 FIELD STRING；不能因 JsonMap 将 CLR 类型改成
            // string 就被默认规则误判为 TAG。
            if ((col.Attribute?.DbType ?? string.Empty).IndexOf("JSON", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;
            if (tb.Properties.TryGetValue(col.CsName, out var property))
            {
                if (property.GetCustomAttribute<SonnetDBTagAttribute>() != null) return true;
                if (property.GetCustomAttribute<SonnetDBFieldAttribute>() != null) return false;
            }
            // 字符串类型及 Guid 默认视为 TAG（维度字段）。
            var mapType = col.Attribute.MapType.NullableTypeOrThis();
            return mapType == typeof(string) || mapType == typeof(char) || mapType == typeof(Guid);
        }

        /// <summary>
        /// 判断列是否为隐式 time 列（建表时无需声明）。
        /// </summary>
        static bool IsTimeColumn(ColumnInfo col) => string.Equals(col.Attribute.Name, "time", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 规范化完整列定义字符串（含 TAG/FIELD 前缀）。
        /// 去除 NOT NULL / DEFAULT 等修饰词，统一大写关键字。
        /// </summary>
        static string NormalizeColumnDefinition(string dbType)
        {
            var normalized = StripColumnModifiers(dbType);
            if (normalized.StartsWith("TAG", StringComparison.OrdinalIgnoreCase))
            {
                var tagType = normalized.Substring(3).Trim();
                return string.IsNullOrEmpty(tagType) || tagType.Equals("STRING", StringComparison.OrdinalIgnoreCase) ? "TAG" : $"TAG {tagType}";
            }
            if (normalized.StartsWith("FIELD", StringComparison.OrdinalIgnoreCase))
                return $"FIELD {NormalizeFieldType(normalized.Substring(5))}";
            return normalized;
        }

        /// <summary>
        /// 将 DbType 字符串规范化为 SonnetDB FIELD 数据类型。
        /// <list type="bullet">
        ///   <item>VECTOR(N)   — 直接透传，保留维度数</item>
        ///   <item>GEOPOINT    — 规范化为大写</item>
        ///   <item>float/double/decimal/real → FLOAT</item>
        ///   <item>int/bigint/long/int64     → INT</item>
        ///   <item>bool/boolean              → BOOL</item>
        ///   <item>text/varchar/char/string  → STRING</item>
        ///   <item>空或未知                   → STRING（默认）</item>
        /// </list>
        /// </summary>
        static string NormalizeFieldType(string dbType)
        {
            if (string.IsNullOrWhiteSpace(dbType)) return "STRING";
            var normalized = StripColumnModifiers(dbType).ToUpperInvariant();
            // VECTOR(N) 透传，保留括号内的维度参数。
            if (normalized.Contains("VECTOR")) return normalized;
            if (normalized.Contains("GEOPOINT")) return "GEOPOINT";
            if (normalized.Contains("FLOAT") || normalized.Contains("DOUBLE") || normalized.Contains("REAL") || normalized.Contains("DECIMAL")) return "FLOAT";
            if (normalized.Contains("BIGINT") || normalized.Contains("LONG") || normalized.Contains("INT64") || normalized.Contains("INTEGER") || normalized.Contains("INT")) return "INT";
            if (normalized.Contains("BOOLEAN") || normalized.Contains("BOOL")) return "BOOL";
            // 时序测量没有 JSON FIELD；JSON DOM 类型在该模型中按字符串保存。
            if (normalized.Contains("JSON")) return "STRING";
            if (normalized.Contains("TEXT") || normalized.Contains("VARCHAR") || normalized.Contains("CHAR") || normalized.Contains("STRING")) return "STRING";
            return normalized;
        }

        /// <summary>
        /// 去除列定义中的 NOT NULL / NULL / DEFAULT 等修饰词，并规范化空白。
        /// 同时将 float64/int64/boolean/text/varchar/char 别名统一为 SonnetDB 标准类型名。
        /// </summary>
        static string StripColumnModifiers(string dbType)
        {
            var normalized = Regex.Replace(dbType.Trim(), @"\s+NOT\s+NULL\b|\s+NULL\b", "", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, @"\s+DEFAULT\s+('[^']*'|""[^""]*""|\S+)", "", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
            switch (normalized.ToUpperInvariant())
            {
                case "FLOAT64":  return "FLOAT";
                case "INT64":    return "INT";
                case "BOOLEAN":  return "BOOL";
                case "TEXT":
                case "VARCHAR":
                case "CHAR":     return "STRING";
                default:         return normalized;
            }
        }
    }
}
