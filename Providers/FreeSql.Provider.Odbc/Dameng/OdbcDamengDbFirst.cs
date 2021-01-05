using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Odbc.Dameng
{
    class OdbcDamengDbFirst : IDbFirst
    {
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        public OdbcDamengDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public int GetDbType(DbColumnInfo column) => (int)GetSqlDbType(column);
        OdbcType GetSqlDbType(DbColumnInfo column)
        {
            var dbfull = column.DbTypeTextFull.ToLower();
            switch (dbfull)
            {
                case "number(1)": return OdbcType.Bit;

                case "number(4)": return OdbcType.SmallInt;
                case "number(6)": return OdbcType.SmallInt;
                case "number(11)": return OdbcType.Int;
                case "number(21)": return OdbcType.BigInt;

                case "number(3)": return OdbcType.TinyInt;
                case "number(5)": return OdbcType.SmallInt;
                case "number(10)": return OdbcType.BigInt;
                case "number(20)": return OdbcType.Decimal;

                case "float(126)": return OdbcType.Double;
                case "float(63)": return OdbcType.Real;
                case "number(10,2)": return OdbcType.Decimal;

                case "interval day(2) to second(6)": return OdbcType.Time;
                case "timestamp(6)": return OdbcType.DateTime;
                case "timestamp(6) with local time zone": return OdbcType.DateTime;

                case "blob": return OdbcType.VarBinary;
                case "nvarchar2(255)": return OdbcType.NVarChar;

                case "char(36)": return OdbcType.Char;
            }
            switch (column.DbTypeText.ToLower())
            {
                case "bit":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["number(1)"]);
                    return OdbcType.Bit;
                case "smallint":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["number(4)"]);
                    return OdbcType.SmallInt;
                case "byte":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["number(3)"]);
                    return OdbcType.TinyInt;
                case "tinyint":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["number(3)"]);
                    return OdbcType.TinyInt;
                case "int":
                case "integer":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["number(11)"]);
                    return OdbcType.Int;
                case "bigint":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["number(21)"]);
                    return OdbcType.Int;
                case "dec":
                case "decimal":
                case "numeric":
                case "number":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["number(10,2)"]);
                    return OdbcType.Decimal;
                case "time":
                case "interval day to second":
                case "interval year to month":
                case "interval year":
                case "interval month":
                case "interval day":
                case "interval day to hour":
                case "interval day to minute":
                case "interval hour":
                case "interval hour to minute":
                case "interval hour to second":
                case "interval minute":
                case "interval minute to second":
                case "interval second":
                case "time with time zone":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["interval day(2) to second(6)"]);
                    return OdbcType.Time;
                case "date":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["date"]);
                    return OdbcType.DateTime;
                case "timestamp":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["timestamp(6)"]);
                    return OdbcType.DateTime;
                case "timestamp with local time zone":
                case "timestamp with time zone":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["timestamp(6) with local time zone"]);
                    return OdbcType.DateTime;
                case "binary":
                case "varbinary":
                case "blob":
                case "image":
                case "longvarbinary":
                case "bfile":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["blob"]);
                    return OdbcType.VarBinary;
                case "nvarchar2":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["nvarchar2(255)"]);
                    return OdbcType.NVarChar;
                case "varchar":
                case "varchar2":
                case "text":
                case "longvarchar":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["nvarchar2(255)"]);
                    return OdbcType.NVarChar;
                case "character":
                case "char":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["nvarchar2(255)"]);
                    return OdbcType.Char;
                case "nchar":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["nvarchar2(255)"]);
                    return OdbcType.NChar;
                case "clob":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["nvarchar2(255)"]);
                    return OdbcType.NVarChar;
                case "nclob":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["nvarchar2(255)"]);
                    return OdbcType.NVarChar;
                case "raw":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["blob"]);
                    return OdbcType.VarBinary;
                case "long raw":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["blob"]);
                    return OdbcType.VarBinary;
                case "real":
                case "binary_float":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["float(63)"]);
                    return OdbcType.Real;
                case "double":
                case "float":
                case "double precision":
                case "binary_double":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["float(126)"]);
                    return OdbcType.Double;
                case "rowid":
                default:
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["nvarchar2(255)"]);
                    return OdbcType.NVarChar;
            }
            throw new NotImplementedException($"未实现 {column.DbTypeTextFull} 类型映射");
        }

        static ConcurrentDictionary<string, DbToCs> _dicDbToCs = new ConcurrentDictionary<string, DbToCs>(StringComparer.CurrentCultureIgnoreCase);
        static OdbcDamengDbFirst()
        {
            var defaultDbToCs = new Dictionary<string, DbToCs>() {
                { "number(1)", new DbToCs("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },

                { "number(4)", new DbToCs("(sbyte?)", "sbyte.Parse({0})", "{0}.ToString()", "sbyte?", typeof(sbyte), typeof(sbyte?), "{0}.Value", "GetInt16") },
                { "number(6)", new DbToCs("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(short), typeof(short?), "{0}.Value", "GetInt16") },
                { "number(11)", new DbToCs("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(int), typeof(int?), "{0}.Value", "GetInt32") },
                { "number(21)", new DbToCs("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },

                { "number(3)", new DbToCs("(byte?)", "byte.Parse({0})", "{0}.ToString()", "byte?", typeof(byte), typeof(byte?), "{0}.Value", "GetByte") },
                { "number(5)", new DbToCs("(ushort?)", "ushort.Parse({0})", "{0}.ToString()", "ushort?", typeof(ushort), typeof(ushort?), "{0}.Value", "GetInt32") },
                { "number(10)", new DbToCs("(uint?)", "uint.Parse({0})", "{0}.ToString()", "uint?", typeof(uint), typeof(uint?), "{0}.Value", "GetInt64") },
                { "number(20)", new DbToCs("(ulong?)", "ulong.Parse({0})", "{0}.ToString()", "ulong?", typeof(ulong), typeof(ulong?), "{0}.Value", "GetDecimal") },

                { "float(126)", new DbToCs("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
                { "float(63)", new DbToCs("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?), "{0}.Value", "GetFloat") },
                { "number(10,2)", new DbToCs("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },

                { "interval day(2) to second(6)", new DbToCs("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },
                { "date", new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetValue") },
                { "timestamp(6)", new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetValue") },
                { "timestamp(6) with local time zone", new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetValue") },

                { "blob", new DbToCs("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },

                { "nvarchar2(255)", new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { "char(36 char)", new DbToCs("(Guid?)", "Guid.Parse({0})", "{0}.ToString()", "Guid?", typeof(Guid), typeof(Guid?), "{0}.Value", "GetGuid") },
            };
            foreach (var kv in defaultDbToCs)
                _dicDbToCs.TryAdd(kv.Key, kv.Value);
        }


        public string GetCsConvert(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbTypeTextFull, out var trydc) ? (column.IsNullable ? trydc.csConvert : trydc.csConvert.Replace("?", "")) : null;
        public string GetCsParse(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbTypeTextFull, out var trydc) ? trydc.csParse : null;
        public string GetCsStringify(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbTypeTextFull, out var trydc) ? trydc.csStringify : null;
        public string GetCsType(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbTypeTextFull, out var trydc) ? (column.IsNullable ? trydc.csType : trydc.csType.Replace("?", "")) : null;
        public Type GetCsTypeInfo(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbTypeTextFull, out var trydc) ? trydc.csTypeInfo : null;
        public string GetCsTypeValue(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbTypeTextFull, out var trydc) ? trydc.csTypeValue : null;
        public string GetDataReaderMethod(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbTypeTextFull, out var trydc) ? trydc.dataReaderMethod : null;

        public List<string> GetDatabases()
        {
            var sql = @" select username from all_users";
            var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            return ds.Select(a => a.FirstOrDefault()?.ToString()).ToList();
        }

        public bool ExistsTable(string name, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var tbname = _commonUtils.SplitTableName(name);
            if (tbname?.Length == 1)
            {
                var userId = (_orm.Ado.MasterPool as OdbcDamengConnectionPool)?.UserId;
                if (string.IsNullOrEmpty(userId))
                    using (var conn = _orm.Ado.MasterPool.Get())
                    {
                        userId = OdbcDamengConnectionPool.GetUserId(conn.Value.ConnectionString);
                    }
                tbname = new[] { userId, tbname[0] };
            }
            if (ignoreCase) tbname = tbname.Select(a => a.ToLower()).ToArray();
            var sql = $" select 1 from all_tab_comments where {(ignoreCase ? "lower(owner)" : "owner")}={_commonUtils.FormatSql("{0}", tbname[0])} and {(ignoreCase ? "lower(table_name)" : "table_name")}={_commonUtils.FormatSql("{0}", tbname[1])}";
            return string.Concat(_orm.Ado.ExecuteScalar(CommandType.Text, sql)) == "1";
        }

        public DbTableInfo GetTableByName(string name, bool ignoreCase = true) => GetTables(null, name, ignoreCase)?.FirstOrDefault();
        public List<DbTableInfo> GetTablesByDatabase(params string[] database) => GetTables(database, null, false);

        public List<DbTableInfo> GetTables(string[] database, string tablename, bool ignoreCase)
        {
            var loc1 = new List<DbTableInfo>();
            var loc2 = new Dictionary<string, DbTableInfo>();
            var loc3 = new Dictionary<string, Dictionary<string, DbColumnInfo>>();
            string[] tbname = null;
            if (string.IsNullOrEmpty(tablename) == false)
            {
                tbname = _commonUtils.SplitTableName(tablename);
                if (tbname?.Length == 1)
                {
                    var userUsers = _orm.Ado.ExecuteScalar(" select username from user_users")?.ToString();
                    if (string.IsNullOrEmpty(userUsers)) return loc1;
                    tbname = new[] { userUsers, tbname[0] };
                }
                if (ignoreCase) tbname = tbname.Select(a => a.ToLower()).ToArray();
                database = new[] { tbname[0] };
            }
            else if (database == null || database.Any() == false)
            {
                var userUsers = _orm.Ado.ExecuteScalar(" select username from user_users")?.ToString();
                if (string.IsNullOrEmpty(userUsers)) return loc1;
                database = new[] { userUsers };
            }

            var databaseIn = string.Join(",", database.Select(a => _commonUtils.FormatSql("{0}", a)));
            var sql = $@"
select
a.owner || '.' || a.table_name,
a.owner,
a.table_name,
b.comments,
'TABLE'
from all_tables a
left join all_tab_comments b on b.owner = a.owner and b.table_name = a.table_name and b.table_type = 'TABLE'
where {(ignoreCase ? "lower(a.owner)" : "a.owner")} in ({databaseIn}){(tbname == null ? "" : $" and {(ignoreCase ? "lower(a.table_name)" : "a.table_name")}={_commonUtils.FormatSql("{0}", tbname[1])}")}

UNION ALL

select
a.owner || '.' || a.view_name,
a.owner,
a.view_name,
b.comments,
'VIEW' AS tp
from all_views a
left join all_tab_comments b on b.owner = a.owner and b.table_name = a.view_name and b.table_type = 'VIEW'
where {(ignoreCase ? "lower(a.owner)" : "a.owner")} in ({databaseIn}){(tbname == null ? "" : $" and {(ignoreCase ? "lower(a.view_name)" : "a.view_name")}={_commonUtils.FormatSql("{0}", tbname[1])}")}";
            var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            if (ds == null) return loc1;

            var loc6 = new List<string[]>();
            var loc66 = new List<string[]>();
            var loc6_1000 = new List<string>();
            var loc66_1000 = new List<string>();
            foreach (var row in ds)
            {
                var table_id = string.Concat(row[0]);
                var schema = string.Concat(row[1]);
                var table = string.Concat(row[2]);
                var comment = string.Concat(row[3]);
                var type = string.Concat(row[4]) == "VIEW" ? DbTableType.VIEW : DbTableType.TABLE;
                if (database.Length == 1)
                {
                    table_id = table_id.Substring(table_id.IndexOf('.') + 1);
                    schema = "";
                }
                loc2.Add(table_id, new DbTableInfo { Id = table_id, Schema = schema, Name = table, Comment = comment, Type = type });
                loc3.Add(table_id, new Dictionary<string, DbColumnInfo>());
                switch (type)
                {
                    case DbTableType.TABLE:
                    case DbTableType.VIEW:
                        loc6_1000.Add(table.Replace("'", "''"));
                        if (loc6_1000.Count >= 999)
                        {
                            loc6.Add(loc6_1000.ToArray());
                            loc6_1000.Clear();
                        }
                        break;
                    case DbTableType.StoreProcedure:
                        loc66_1000.Add(table.Replace("'", "''"));
                        if (loc66_1000.Count >= 999)
                        {
                            loc66.Add(loc66_1000.ToArray());
                            loc66_1000.Clear();
                        }
                        break;
                }
            }
            if (loc6_1000.Count > 0) loc6.Add(loc6_1000.ToArray());
            if (loc66_1000.Count > 0) loc66.Add(loc66_1000.ToArray());

            if (loc6.Count == 0) return loc1;
            var loc8 = new StringBuilder().Append("(");
            for (var loc8idx = 0; loc8idx < loc6.Count; loc8idx++)
            {
                if (loc8idx > 0) loc8.Append(" OR ");
                loc8.Append("a.table_name in (");
                for (var loc8idx2 = 0; loc8idx2 < loc6[loc8idx].Length; loc8idx2++)
                {
                    if (loc8idx2 > 0) loc8.Append(",");
                    loc8.Append($"'{loc6[loc8idx][loc8idx2]}'");
                }
                loc8.Append(")");
            }
            loc8.Append(")");

            sql = $@"
select
a.owner || '.' || a.table_name,
a.column_name,
a.data_type,
a.data_length,
a.data_precision,
a.data_scale,
a.char_used,
case when a.nullable = 'N' then 0 else 1 end,
nvl((select 1 from user_sequences where upper(sequence_name)=upper(a.table_name||'_seq_'||a.column_name) and rownum < 2), 0),
b.comments,
a.data_default
from all_tab_cols a
left join all_col_comments b on b.owner = a.owner and b.table_name = a.table_name and b.column_name = a.column_name
where {(ignoreCase ? "lower(a.owner)" : "a.owner")} in ({databaseIn}) and {loc8}
";
            ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            if (ds == null) return loc1;

            var ds2 = new List<object[]>();
            foreach (var row in ds)
            {
                var ds2item = new object[9];
                ds2item[0] = row[0];
                ds2item[1] = row[1];
                ds2item[2] = Regex.Replace(string.Concat(row[2]), @"\(\d+\)", "");
                ds2item[4] = OdbcDamengCodeFirst.GetDamengSqlTypeFullName(new object[] { row[1], row[2], row[3], row[4], row[5], row[6] });
                ds2item[5] = string.Concat(row[7]);
                ds2item[6] = string.Concat(row[8]);
                ds2item[7] = string.Concat(row[9]);
                ds2item[8] = string.Concat(row[10]);
                ds2.Add(ds2item);
            }
            var position = 0;
            foreach (var row in ds2)
            {
                string table_id = string.Concat(row[0]);
                string column = string.Concat(row[1]);
                string type = string.Concat(row[2]);
                //long max_length = long.Parse(string.Concat(row[3]));
                string sqlType = string.Concat(row[4]);
                var m_len = Regex.Match(sqlType, @"\w+\((\d+)");
                int max_length = m_len.Success ? int.Parse(m_len.Groups[1].Value) : -1;
                bool is_nullable = string.Concat(row[5]) == "1";
                bool is_identity = string.Concat(row[6]) == "1";
                string comment = string.Concat(row[7]);
                string defaultValue = string.Concat(row[8]);
                if (max_length == 0) max_length = -1;
                if (database.Length == 1)
                {
                    table_id = table_id.Substring(table_id.IndexOf('.') + 1);
                }
                loc3[table_id].Add(column, new DbColumnInfo
                {
                    Name = column,
                    MaxLength = max_length,
                    IsIdentity = is_identity,
                    IsNullable = is_nullable,
                    IsPrimary = false,
                    DbTypeText = type,
                    DbTypeTextFull = sqlType,
                    Table = loc2[table_id],
                    Coment = comment,
                    DefaultValue = defaultValue,
                    Position = ++position
                });
                loc3[table_id][column].DbType = this.GetDbType(loc3[table_id][column]);
                loc3[table_id][column].CsType = this.GetCsTypeInfo(loc3[table_id][column]);
            }

            sql = $@"
select
a.table_owner || '.' || a.table_name,
c.column_name,
c.index_name,
case when a.uniqueness = 'UNIQUE' then 1 else 0 end,
case when exists(select 1 from all_constraints where index_name = a.index_name and constraint_type = 'P') then 1 else 0 end,
0,
case when c.descend = 'DESC' then 1 else 0 end,
c.column_position
from all_indexes a,
all_ind_columns c 
where a.index_name = c.index_name
and a.table_owner = c.table_owner
and a.table_name = c.table_name
and {(ignoreCase ? "lower(a.table_owner)" : "a.table_owner")} in ({databaseIn}) and {loc8}
";
            ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            if (ds == null) return loc1;

            var indexColumns = new Dictionary<string, Dictionary<string, DbIndexInfo>>();
            var uniqueColumns = new Dictionary<string, Dictionary<string, DbIndexInfo>>();
            foreach (var row in ds)
            {
                string table_id = string.Concat(row[0]);
                string column = string.Concat(row[1]).Trim('"');
                string index_id = string.Concat(row[2]);
                bool is_unique = string.Concat(row[3]) == "1";
                bool is_primary_key = string.Concat(row[4]) == "1";
                bool is_clustered = string.Concat(row[5]) == "1";
                bool is_desc = string.Concat(row[6]) == "1";
                if (database.Length == 1)
                    table_id = table_id.Substring(table_id.IndexOf('.') + 1);
                if (loc3.ContainsKey(table_id) == false || loc3[table_id].ContainsKey(column) == false) continue;
                var loc9 = loc3[table_id][column];
                if (loc9.IsPrimary == false && is_primary_key) loc9.IsPrimary = is_primary_key;
                if (is_primary_key) continue;

                Dictionary<string, DbIndexInfo> loc10 = null;
                DbIndexInfo loc11 = null;
                if (!indexColumns.TryGetValue(table_id, out loc10))
                    indexColumns.Add(table_id, loc10 = new Dictionary<string, DbIndexInfo>());
                if (!loc10.TryGetValue(index_id, out loc11))
                    loc10.Add(index_id, loc11 = new DbIndexInfo());
                loc11.Columns.Add(new DbIndexColumnInfo { Column = loc9, IsDesc = is_desc });
                if (is_unique && !is_primary_key)
                {
                    if (!uniqueColumns.TryGetValue(table_id, out loc10))
                        uniqueColumns.Add(table_id, loc10 = new Dictionary<string, DbIndexInfo>());
                    if (!loc10.TryGetValue(index_id, out loc11))
                        loc10.Add(index_id, loc11 = new DbIndexInfo());
                    loc11.Columns.Add(new DbIndexColumnInfo { Column = loc9, IsDesc = is_desc });
                }
            }
            foreach (string table_id in indexColumns.Keys)
            {
                foreach (var column in indexColumns[table_id])
                    loc2[table_id].IndexesDict.Add(column.Key, column.Value);
            }
            foreach (string table_id in uniqueColumns.Keys)
            {
                foreach (var column in uniqueColumns[table_id])
                {
                    column.Value.Columns.Sort((c1, c2) => c1.Column.Name.CompareTo(c2.Column.Name));
                    loc2[table_id].UniquesDict.Add(column.Key, column.Value);
                }
            }

            if (tbname == null)
            {
                sql = $@"
select
a.owner || '.' || a.table_name,
c.column_name,
c.constraint_name,
b.owner || '.' || b.table_name,
1,
d.column_name

-- a.owner 外键拥有者,
-- a.table_name 外键表,
-- c.column_name 外键列,
-- b.owner 主键拥有者,
-- b.table_name 主键表,
-- d.column_name 主键列,
-- c.constraint_name 外键名,
-- d.constraint_name 主键名

from
all_constraints a,
all_constraints b,
all_cons_columns c, --外键表
all_cons_columns d  --主键表
where
a.r_constraint_name = b.constraint_name
and a.constraint_type = 'R'
and b.constraint_type = 'P'
and a.r_owner = b.owner
and a.constraint_name = c.constraint_name
and b.constraint_name = d.constraint_name
and a.owner = c.owner
and a.table_name = c.table_name
and b.owner = d.owner
and b.table_name = d.table_name
and {(ignoreCase ? "lower(a.owner)" : "a.owner")} in ({databaseIn}) and {loc8}
";
                ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                var fkColumns = new Dictionary<string, Dictionary<string, DbForeignInfo>>();
                foreach (var row in ds)
                {
                    string table_id = string.Concat(row[0]);
                    string column = string.Concat(row[1]);
                    string fk_id = string.Concat(row[2]);
                    string ref_table_id = string.Concat(row[3]);
                    bool is_foreign_key = string.Concat(row[4]) == "1";
                    string referenced_column = string.Concat(row[5]);
                    if (database.Length == 1)
                    {
                        table_id = table_id.Substring(table_id.IndexOf('.') + 1);
                        ref_table_id = ref_table_id.Substring(ref_table_id.IndexOf('.') + 1);
                    }
                    if (loc3.ContainsKey(table_id) == false || loc3[table_id].ContainsKey(column) == false) continue;
                    var loc9 = loc3[table_id][column];
                    if (loc2.ContainsKey(ref_table_id) == false) continue;
                    var loc10 = loc2[ref_table_id];
                    var loc11 = loc3[ref_table_id][referenced_column];

                    Dictionary<string, DbForeignInfo> loc12 = null;
                    DbForeignInfo loc13 = null;
                    if (!fkColumns.TryGetValue(table_id, out loc12))
                        fkColumns.Add(table_id, loc12 = new Dictionary<string, DbForeignInfo>());
                    if (!loc12.TryGetValue(fk_id, out loc13))
                        loc12.Add(fk_id, loc13 = new DbForeignInfo { Table = loc2[table_id], ReferencedTable = loc10 });
                    loc13.Columns.Add(loc9);
                    loc13.ReferencedColumns.Add(loc11);
                }
                foreach (var table_id in fkColumns.Keys)
                    foreach (var fk in fkColumns[table_id])
                        loc2[table_id].ForeignsDict.Add(fk.Key, fk.Value);
            }

            foreach (var table_id in loc3.Keys)
            {
                foreach (var loc5 in loc3[table_id].Values)
                {
                    loc2[table_id].Columns.Add(loc5);
                    if (loc5.IsIdentity) loc2[table_id].Identitys.Add(loc5);
                    if (loc5.IsPrimary) loc2[table_id].Primarys.Add(loc5);
                }
            }
            foreach (var loc4 in loc2.Values)
            {
                //if (loc4.Primarys.Count == 0 && loc4.UniquesDict.Count > 0)
                //{
                //    foreach (var loc5 in loc4.UniquesDict.First().Value.Columns)
                //    {
                //        loc5.Column.IsPrimary = true;
                //        loc4.Primarys.Add(loc5.Column);
                //    }
                //}
                loc4.Primarys.Sort((c1, c2) => c1.Name.CompareTo(c2.Name));
                loc4.Columns.Sort((c1, c2) =>
                {
                    int compare = c2.IsPrimary.CompareTo(c1.IsPrimary);
                    if (compare == 0)
                    {
                        bool b1 = loc4.ForeignsDict.Values.Where(fk => fk.Columns.Where(c3 => c3.Name == c1.Name).Any()).Any();
                        bool b2 = loc4.ForeignsDict.Values.Where(fk => fk.Columns.Where(c3 => c3.Name == c2.Name).Any()).Any();
                        compare = b2.CompareTo(b1);
                    }
                    if (compare == 0) compare = c1.Name.CompareTo(c2.Name);
                    return compare;
                });
                loc1.Add(loc4);
            }
            loc1.Sort((t1, t2) =>
            {
                var ret = t1.Schema.CompareTo(t2.Schema);
                if (ret == 0) ret = t1.Name.CompareTo(t2.Name);
                return ret;
            });

            loc2.Clear();
            loc3.Clear();
            return loc1;
        }

        public List<DbEnumInfo> GetEnumsByDatabase(params string[] database)
        {
            return new List<DbEnumInfo>();
        }
    }
}