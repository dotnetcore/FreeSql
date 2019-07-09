using FreeSql.DatabaseModel;
using FreeSql.Internal;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace FreeSql.Oracle
{
    class OracleDbFirst : IDbFirst
    {
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        public OracleDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public int GetDbType(DbColumnInfo column) => (int)GetSqlDbType(column);
        OracleDbType GetSqlDbType(DbColumnInfo column)
        {
            var dbfull = column.DbTypeTextFull.ToLower();
            switch (dbfull)
            {
                case "number(1)": return OracleDbType.Boolean;

                case "number(4)": return OracleDbType.Decimal;
                case "number(6)": return OracleDbType.Int16;
                case "number(11)": return OracleDbType.Int32;
                case "number(21)": return OracleDbType.Int64;

                case "number(3)": return OracleDbType.Byte;
                case "number(5)": return OracleDbType.Decimal;
                case "number(10)": return OracleDbType.Decimal;
                case "number(20)": return OracleDbType.Decimal;

                case "float(126)": return OracleDbType.Double;
                case "float(63)": return OracleDbType.Single;
                case "number(10,2)": return OracleDbType.Decimal;

                case "interval day(2) to second(6)": return OracleDbType.IntervalDS;
                case "timestamp(6)": return OracleDbType.TimeStamp;
                case "timestamp(6) with local time zone": return OracleDbType.TimeStampLTZ;

                case "blob": return OracleDbType.Blob;
                case "nvarchar2(255)": return OracleDbType.NVarchar2;

                case "char(36 char)": return OracleDbType.Char;
            }
            switch (column.DbTypeText.ToLower())
            {
                case "number":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["number(10,2)"]);
                    return OracleDbType.Decimal;
                case "float":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["float(126)"]);
                    return OracleDbType.Double;
                case "interval day to second":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["interval day(2) to second(6)"]);
                    return OracleDbType.IntervalDS;
                case "date":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["date(7)"]);
                    return OracleDbType.IntervalDS;
                case "timestamp":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["timestamp(6)"]);
                    return OracleDbType.TimeStamp;
                case "timestamp with local time zone":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["timestamp(6) with local time zone"]);
                    return OracleDbType.TimeStampLTZ;
                case "blob":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["blob"]);
                    return OracleDbType.Blob;
                case "nvarchar2":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["nvarchar2(255)"]);
                    return OracleDbType.NVarchar2;
                case "varchar2":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["nvarchar2(255)"]);
                    return OracleDbType.Varchar2;
                case "char":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["nvarchar2(255)"]);
                    return OracleDbType.Char;
                case "nchar":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["nvarchar2(255)"]);
                    return OracleDbType.NChar;
                case "clob":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["nvarchar2(255)"]);
                    return OracleDbType.Clob;
                case "nclob":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["nvarchar2(255)"]);
                    return OracleDbType.NClob;
                case "raw":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["blob"]);
                    return OracleDbType.Raw;
                case "long raw":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["blob"]);
                    return OracleDbType.LongRaw;
                case "binary_float":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["float(63)"]);
                    return OracleDbType.BinaryFloat;
                case "binary_double":
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["float(126)"]);
                    return OracleDbType.BinaryDouble;
                case "rowid":
                default:
                    _dicDbToCs.TryAdd(dbfull, _dicDbToCs["nvarchar2(255)"]);
                    return OracleDbType.NVarchar2;
            }
            throw new NotImplementedException($"未实现 {column.DbTypeTextFull} 类型映射");
        }

        static ConcurrentDictionary<string, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)> _dicDbToCs = new ConcurrentDictionary<string, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)>();
        static OracleDbFirst()
        {
            var defaultDbToCs = new Dictionary<string, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)>() {
                { "number(1)", ("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },

                { "number(4)", ("(sbyte?)", "sbyte.Parse({0})", "{0}.ToString()", "sbyte?", typeof(sbyte), typeof(sbyte?), "{0}.Value", "GetInt16") },
                { "number(6)", ("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(short), typeof(short?), "{0}.Value", "GetInt16") },
                { "number(11)", ("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(int), typeof(int?), "{0}.Value", "GetInt32") },
                { "number(21)", ("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },

                { "number(3)", ("(byte?)", "byte.Parse({0})", "{0}.ToString()", "byte?", typeof(byte), typeof(byte?), "{0}.Value", "GetByte") },
                { "number(5)", ("(ushort?)", "ushort.Parse({0})", "{0}.ToString()", "ushort?", typeof(ushort), typeof(ushort?), "{0}.Value", "GetInt32") },
                { "number(10)", ("(uint?)", "uint.Parse({0})", "{0}.ToString()", "uint?", typeof(uint), typeof(uint?), "{0}.Value", "GetInt64") },
                { "number(20)", ("(ulong?)", "ulong.Parse({0})", "{0}.ToString()", "ulong?", typeof(ulong), typeof(ulong?), "{0}.Value", "GetDecimal") },

                { "float(126)", ("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
                { "float(63)", ("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?), "{0}.Value", "GetFloat") },
                { "number(10,2)", ("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },

                { "interval day(2) to second(6)", ("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },
                { "date(7)", ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetValue") },
                { "timestamp(6)", ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetValue") },
                { "timestamp(6) with local time zone", ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetValue") },

                { "blob", ("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },

                { "nvarchar2(255)", ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { "char(36 char)", ("(Guid?)", "Guid.Parse({0})", "{0}.ToString()", "Guid?", typeof(Guid), typeof(Guid?), "{0}.Value", "GetGuid") },
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

        public List<DbTableInfo> GetTablesByDatabase(params string[] database2)
        {
            var loc1 = new List<DbTableInfo>();
            var loc2 = new Dictionary<string, DbTableInfo>();
            var loc3 = new Dictionary<string, Dictionary<string, DbColumnInfo>>();
            var database = database2?.ToArray();

            if (database == null || database.Any() == false)
            {
                var userUsers = _orm.Ado.ExecuteScalar("select username from user_users")?.ToString();
                if (string.IsNullOrEmpty(userUsers)) return loc1;
                database = new[] { userUsers };
            }
            var databaseIn = string.Join(",", database.Select(a => _commonUtils.FormatSql("{0}", a)));
            var sql = string.Format(@"
select
a.owner || '.' || a.table_name,
a.owner,
a.table_name,
b.comments,
'TABLE'
from all_tables a
left join all_tab_comments b on b.owner = a.owner and b.table_name = a.table_name and b.table_type = 'TABLE'
where a.owner in ({0})", databaseIn);
            var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            if (ds == null) return loc1;

            var loc6 = new List<string>();
            var loc66 = new List<string>();
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
                        loc6.Add(table.Replace("'", "''"));
                        break;
                    case DbTableType.StoreProcedure:
                        loc66.Add(table.Replace("'", "''"));
                        break;
                }
            }
            if (loc6.Count == 0) return loc1;
            var loc8 = "'" + string.Join("','", loc6.ToArray()) + "'";
            var loc88 = "'" + string.Join("','", loc66.ToArray()) + "'";

            sql = string.Format(@"
select
a.owner || '.' || a.table_name,
a.column_name,
a.data_type,
a.data_length,
a.data_precision,
a.data_scale,
a.char_used,
case when a.nullable = 'Y' then 1 else 0 end,
nvl((select 1 from user_sequences where upper(sequence_name)=upper(a.table_name||'_seq_'||a.column_name)), 0),
b.comments
from all_tab_cols a
left join all_col_comments b on b.owner = a.owner and b.table_name = a.table_name and b.column_name = a.column_name
where a.owner in ({1}) and a.table_name in ({0})
", loc8, databaseIn);
            ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            if (ds == null) return loc1;

            var ds2 = new List<object[]>();
            foreach (var row in ds)
            {
                var ds2item = new object[8];
                ds2item[0] = row[0];
                ds2item[1] = row[1];
                ds2item[2] = Regex.Replace(string.Concat(row[2]), @"\(\d+\)", "");
                ds2item[4] = OracleCodeFirst.GetOracleSqlTypeFullName(new object[] { row[1], row[2], row[3], row[4], row[5], row[6] });
                ds2item[5] = string.Concat(row[7]) == "1";
                ds2item[6] = string.Concat(row[8]) == "1";
                ds2item[7] = string.Concat(row[9]);
                ds2.Add(ds2item);
            }
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
                    Coment = comment
                });
                loc3[table_id][column].DbType = this.GetDbType(loc3[table_id][column]);
                loc3[table_id][column].CsType = this.GetCsTypeInfo(loc3[table_id][column]);
            }

            sql = string.Format(@"
select
a.owner || '.' || a.table_name,
c.column_name,
c.constraint_name,
case when a.constraint_type = 'U' then 1 else 0 end,
case when a.constraint_type = 'P' then 1 else 0 end,
0,
0
from
all_constraints a,
all_cons_columns c
where
a.constraint_name = c.constraint_name
and a.owner = c.owner
and a.table_name = c.table_name
and a.constraint_type  in ('P', 'U')
and a.owner in ({1}) and a.table_name in ({0})
", loc8, databaseIn);
            ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            if (ds == null) return loc1;

            var indexColumns = new Dictionary<string, Dictionary<string, List<DbColumnInfo>>>();
            var uniqueColumns = new Dictionary<string, Dictionary<string, List<DbColumnInfo>>>();
            foreach (var row in ds)
            {
                string table_id = string.Concat(row[0]);
                string column = string.Concat(row[1]);
                string index_id = string.Concat(row[2]);
                bool is_unique = string.Concat(row[3]) == "1";
                bool is_primary_key = string.Concat(row[4]) == "1";
                bool is_clustered = string.Concat(row[5]) == "1";
                int is_desc = int.Parse(string.Concat(row[6]));
                if (database.Length == 1)
                {
                    table_id = table_id.Substring(table_id.IndexOf('.') + 1);
                }
                if (loc3.ContainsKey(table_id) == false || loc3[table_id].ContainsKey(column) == false) continue;
                var loc9 = loc3[table_id][column];
                if (loc9.IsPrimary == false && is_primary_key) loc9.IsPrimary = is_primary_key;

                Dictionary<string, List<DbColumnInfo>> loc10 = null;
                List<DbColumnInfo> loc11 = null;
                if (!indexColumns.TryGetValue(table_id, out loc10))
                    indexColumns.Add(table_id, loc10 = new Dictionary<string, List<DbColumnInfo>>());
                if (!loc10.TryGetValue(index_id, out loc11))
                    loc10.Add(index_id, loc11 = new List<DbColumnInfo>());
                loc11.Add(loc9);
                if (is_unique && !is_primary_key)
                {
                    if (!uniqueColumns.TryGetValue(table_id, out loc10))
                        uniqueColumns.Add(table_id, loc10 = new Dictionary<string, List<DbColumnInfo>>());
                    if (!loc10.TryGetValue(index_id, out loc11))
                        loc10.Add(index_id, loc11 = new List<DbColumnInfo>());
                    loc11.Add(loc9);
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
                    column.Value.Sort((c1, c2) => c1.Name.CompareTo(c2.Name));
                    loc2[table_id].UniquesDict.Add(column.Key, column.Value);
                }
            }

            sql = string.Format(@"
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
and a.owner in ({1}) and a.table_name in ({0})
", loc8, databaseIn);
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
                if (loc4.Primarys.Count == 0 && loc4.UniquesDict.Count > 0)
                {
                    foreach (var loc5 in loc4.UniquesDict.First().Value)
                    {
                        loc5.IsPrimary = true;
                        loc4.Primarys.Add(loc5);
                    }
                }
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