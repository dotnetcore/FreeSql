// SonnetDBDbFirst.cs
// SonnetDB 数据库反向工程（DbFirst）实现。
//
// 功能：读取 SonnetDB 库/表的元数据，将其映射为 FreeSql 的 DbTableInfo / DbColumnInfo，
// 供代码生成器或运行时动态建模使用。
//
// SonnetDB 数据类型 → .NET DbType 映射规则：
//   float / double              → DbType.Double  (double?)
//   int / int64 / time 列名     → DbType.Int64   (long?)   time 列存储 Unix 毫秒整数
//   bool / boolean              → DbType.Boolean (bool?)
//   vector(N)                   → DbType.Object  (object)  向量类型，C# 侧用 object 接收
//   geopoint                    → DbType.String  (string)  地理坐标，格式 "lat,lon"
//   其他（string / tag 等）      → DbType.String  (string)
//
// 元数据查询命令：
//   SHOW DATABASES              — 列出所有数据库
//   SHOW MEASUREMENTS           — 列出当前库所有 measurement（等价于关系型"表"）
//   DESCRIBE MEASUREMENT <name> — 获取列定义（column_name / column_type / data_type）
//   SELECT * FROM <name> LIMIT 0 — 降级回退方案：通过空结果集推断列类型

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

        /// <summary>
        /// 将 SonnetDB 列的数据库类型文本映射为 <see cref="DbType"/> 整型值。
        /// <list type="table">
        ///   <listheader><term>SonnetDB 类型</term><description>DbType</description></listheader>
        ///   <item><term>float / double</term><description>Double</description></item>
        ///   <item><term>int / int64 / 列名为 time</term><description>Int64</description></item>
        ///   <item><term>bool / boolean</term><description>Boolean</description></item>
        ///   <item><term>vector(N)</term><description>Object</description></item>
        ///   <item><term>geopoint</term><description>String（格式 "lat,lon"）</description></item>
        ///   <item><term>其他</term><description>String</description></item>
        /// </list>
        /// </summary>
        public int GetDbType(DbColumnInfo column)
        {
            var dbtype = (column.DbTypeTextFull ?? column.DbTypeText ?? "").ToLowerInvariant();
            if (dbtype.Contains("float") || dbtype.Contains("double")) return (int)DbType.Double;
            // int64 类型及隐式 time 列（Unix 毫秒整数）统一映射为 Int64。
            if (dbtype.Contains("int") || string.Equals(column.Name, "time", StringComparison.OrdinalIgnoreCase)) return (int)DbType.Int64;
            if (dbtype.Contains("bool")) return (int)DbType.Boolean;
            // VECTOR(N) 向量类型，C# 侧用 object 接收原始值。
            if (dbtype.Contains("vector")) return (int)DbType.Object;
            // GEOPOINT 地理坐标，以字符串形式传输。
            if (dbtype.Contains("geopoint")) return (int)DbType.String;
            return (int)DbType.String;
        }

        /// <summary>
        /// DbType → C# 类型转换配置表。
        /// 记录每种 DbType 对应的 C# 类型名、解析表达式、序列化表达式及 DataReader 方法名。
        /// </summary>
        static readonly Dictionary<int, DbToCs> _dicDbToCs = new Dictionary<int, DbToCs>
        {
            { (int)DbType.Int64,   new DbToCs("(long?)",   "long.Parse({0})",   "{0}.ToString()", "long?",   typeof(long),   typeof(long?),   "{0}.Value", "GetInt64") },
            { (int)DbType.Double,  new DbToCs("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
            { (int)DbType.Boolean, new DbToCs("(bool?)",   "{0} == \"1\"",      "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },
            { (int)DbType.String,  new DbToCs("",          "{0}",               "{0}",            "string",  typeof(string), typeof(string),  "{0}",       "GetString") },
            { (int)DbType.Object,  new DbToCs("",          "{0}",               "{0}",            "object",  typeof(object), typeof(object),  "{0}",       "GetValue") },
        };

        public string GetCsConvert(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? (column.IsNullable ? trydc.csConvert : trydc.csConvert.Replace("?", "")) : null;
        public string GetCsParse(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csParse : null;
        public string GetCsStringify(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csStringify : null;
        public string GetCsType(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? (column.IsNullable ? trydc.csType : trydc.csType.Replace("?", "")) : null;
        public Type GetCsTypeInfo(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csTypeInfo : null;
        public string GetCsTypeValue(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csTypeValue : null;
        public string GetDataReaderMethod(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.dataReaderMethod : null;

        /// <summary>
        /// 执行 <c>SHOW DATABASES</c> 返回所有数据库名称列表。
        /// </summary>
        public List<string> GetDatabases()
        {
            try { return _orm.Ado.Query<string>("SHOW DATABASES"); }
            catch { return new List<string>(); }
        }

        /// <summary>
        /// 检查指定 measurement（表）是否存在。
        /// <para>优先使用 <c>SHOW MEASUREMENTS</c> 列举后匹配；
        /// 若命令不支持，则降级执行 <c>SELECT * FROM &lt;name&gt; LIMIT 0</c> 来判断。</para>
        /// </summary>
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
                    // 降级：用 LIMIT 0 的空结果集验证表存在性。
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

        /// <summary>
        /// 获取 SonnetDB measurement（表）的完整元数据列表。
        /// <para>流程：</para>
        /// <list type="number">
        ///   <item>执行 <c>SHOW MEASUREMENTS</c> 获取表名列表。</item>
        ///   <item>对每张表执行 <c>DESCRIBE MEASUREMENT &lt;name&gt;</c> 获取列定义。</item>
        ///   <item>若 DESCRIBE 失败，降级为 <c>SELECT * LIMIT 0</c> 推断列类型。</item>
        ///   <item>隐式 <c>time</c> 列（Int64，Unix 毫秒）始终作为首列插入。</item>
        /// </list>
        /// </summary>
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
                // SonnetDB 每张 measurement 隐式包含 time 列（Int64，存储 Unix 毫秒时间戳），始终排第一位。
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
                    // 优先使用 DESCRIBE MEASUREMENT 获取精确列元数据。
                    dt = _orm.Ado.ExecuteDataTable($"DESCRIBE MEASUREMENT {_commonUtils.QuoteSqlName(name)}");
                }
                catch
                {
                    // DESCRIBE 不支持时降级为空结果集推断。
                    tables.Add(GetTableBySelectSchema(name));
                    continue;
                }
                var pos = 2;
                foreach (DataRow row in dt.Rows)
                {
                    var columnName = GetValue(row, "column_name", 0);
                    if (string.IsNullOrEmpty(columnName)) continue;
                    var columnType = GetValue(row, "column_type", 1); // tag / field
                    var dataType = GetValue(row, "data_type", 2);     // float64 / int64 / string / boolean / vector(N) / geopoint
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

        /// <summary>
        /// 通过执行 <c>SELECT * FROM &lt;name&gt; LIMIT 0</c> 的空结果集来推断列类型。
        /// 用于 <c>DESCRIBE MEASUREMENT</c> 不可用时的降级回退。
        /// </summary>
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
                    // 根据 .NET 类型反推 SonnetDB 类型文本标签。
                    DbTypeText = column.DataType == typeof(long) ? "int64" :
                        column.DataType == typeof(double) || column.DataType == typeof(float) || column.DataType == typeof(decimal) ? "float64" :
                        column.DataType == typeof(bool) ? "boolean" :
                        column.DataType == typeof(string) ? "string" : "object",
                    // time 列不可空，其他列默认可空。
                    IsNullable = !string.Equals(column.ColumnName, "time", StringComparison.OrdinalIgnoreCase),
                    Position = pos++
                };
                dbtype.DbType = GetDbType(dbtype);
                dbtype.CsType = GetCsTypeInfo(dbtype);
                table.Columns.Add(dbtype);
            }
            return table;
        }

        /// <summary>
        /// 从 DataRow 中按列名（或回退至位置索引）读取字符串值。
        /// 用于兼容不同 SonnetDB 版本中 DESCRIBE 结果集的列名差异。
        /// </summary>
        static string GetValue(DataRow row, string columnName, int index)
        {
            if (row.Table.Columns.Contains(columnName)) return string.Concat(row[columnName]);
            return index < row.ItemArray.Length ? string.Concat(row[index]) : null;
        }

        public List<DbEnumInfo> GetEnumsByDatabase(params string[] database) => new List<DbEnumInfo>();
    }
}
