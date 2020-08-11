using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OscarClient;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.ShenTong
{
    class ShenTongDbFirst : IDbFirst
    {
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        public ShenTongDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public int GetDbType(DbColumnInfo column) => (int)GetOscarDbType(column);
        OscarDbType GetOscarDbType(DbColumnInfo column)
        {
            var dbtype = column.DbTypeText;
            var isarray = dbtype.EndsWith("[]");
            if (isarray) dbtype = dbtype.Remove(dbtype.Length - 2);
            OscarDbType ret = OscarDbType.Oidvector;
            switch (dbtype.ToLower().TrimStart('_'))
            {
                case "int2": ret = OscarDbType.SmallInt; break;
                case "int4": ret = OscarDbType.Integer; break;
                case "int8": ret = OscarDbType.BigInt; break;
                case "numeric": ret = OscarDbType.Numeric; break;
                case "float4": ret = OscarDbType.Real; break;
                case "float8": ret = OscarDbType.Double; break;

                case "bpchar": ret = OscarDbType.Char; break;
                case "varchar": ret = OscarDbType.VarChar; break;
                case "text": ret = OscarDbType.Text; break;

                case "timestamp": ret = OscarDbType.TimeStamp; break;
                case "timestamptz": ret = OscarDbType.TimestampTZ; break;
                case "date": ret = OscarDbType.Date; break;
                case "time": ret = OscarDbType.Time; break;
                case "timetz": ret = OscarDbType.TimeTZ; break;
                case "interval": ret = OscarDbType.Interval; break;

                case "bool": ret = OscarDbType.Boolean; break;
                case "bytea": ret = OscarDbType.Bytea; break;
                case "bit": ret = OscarDbType.Bit; break;

                case "uuid": ret = OscarDbType.Char; break;
            }
            return isarray ? (ret | OscarDbType.Array) : ret;
        }

        static readonly Dictionary<int, DbToCs> _dicDbToCs = new Dictionary<int, DbToCs>() {
                { (int)OscarDbType.SmallInt, new DbToCs("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(short), typeof(short?), "{0}.Value", "GetInt16") },
                { (int)OscarDbType.Integer, new DbToCs("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(int), typeof(int?), "{0}.Value", "GetInt32") },
                { (int)OscarDbType.BigInt, new DbToCs("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },
                { (int)OscarDbType.Numeric, new DbToCs("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },
                { (int)OscarDbType.Real, new DbToCs("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?), "{0}.Value", "GetFloat") },
                { (int)OscarDbType.Double, new DbToCs("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },

                { (int)OscarDbType.Char, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)OscarDbType.VarChar, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)OscarDbType.Text, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },

                { (int)OscarDbType.TimeStamp, new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)OscarDbType.TimestampTZ, new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)OscarDbType.Date, new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)OscarDbType.Time, new DbToCs("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },
                { (int)OscarDbType.TimeTZ, new DbToCs("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },
                { (int)OscarDbType.Interval, new DbToCs("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },

                { (int)OscarDbType.Boolean, new DbToCs("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },
                { (int)OscarDbType.Bytea, new DbToCs("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },

				/*** array ***/

				{ (int)(OscarDbType.SmallInt | OscarDbType.Array), new DbToCs("(short[])", "JsonConvert.DeserializeObject<short[]>({0})", "JsonConvert.SerializeObject({0})", "short[]", typeof(short[]), typeof(short[]), "{0}", "GetValue") },
                { (int)(OscarDbType.Integer | OscarDbType.Array), new DbToCs("(int[])", "JsonConvert.DeserializeObject<int[]>({0})", "JsonConvert.SerializeObject({0})", "int[]", typeof(int[]), typeof(int[]), "{0}", "GetValue") },
                { (int)(OscarDbType.BigInt | OscarDbType.Array), new DbToCs("(long[])", "JsonConvert.DeserializeObject<long[]>({0})", "JsonConvert.SerializeObject({0})", "long[]", typeof(long[]), typeof(long[]), "{0}", "GetValue") },
                { (int)(OscarDbType.Numeric | OscarDbType.Array), new DbToCs("(decimal[])", "JsonConvert.DeserializeObject<decimal[]>({0})", "JsonConvert.SerializeObject({0})", "decimal[]", typeof(decimal[]), typeof(decimal[]), "{0}", "GetValue") },
                { (int)(OscarDbType.Real | OscarDbType.Array), new DbToCs("(float[])", "JsonConvert.DeserializeObject<float[]>({0})", "JsonConvert.SerializeObject({0})", "float[]", typeof(float[]), typeof(float[]), "{0}", "GetValue") },
                { (int)(OscarDbType.Double | OscarDbType.Array), new DbToCs("(double[])", "JsonConvert.DeserializeObject<double[]>({0})", "JsonConvert.SerializeObject({0})", "double[]", typeof(double[]), typeof(double[]), "{0}", "GetValue") },

                { (int)(OscarDbType.Char | OscarDbType.Array), new DbToCs("(string[])", "JsonConvert.DeserializeObject<string[]>({0})", "JsonConvert.SerializeObject({0})", "string[]", typeof(string[]), typeof(string[]), "{0}", "GetValue") },
                { (int)(OscarDbType.VarChar | OscarDbType.Array), new DbToCs("(string[])", "JsonConvert.DeserializeObject<string[]>({0})", "JsonConvert.SerializeObject({0})", "string[]", typeof(string[]), typeof(string[]), "{0}", "GetValue") },
                { (int)(OscarDbType.Text | OscarDbType.Array), new DbToCs("(string[])", "JsonConvert.DeserializeObject<string[]>({0})", "JsonConvert.SerializeObject({0})", "string[]", typeof(string[]), typeof(string[]), "{0}", "GetValue") },

                { (int)(OscarDbType.TimeStamp | OscarDbType.Array), new DbToCs("(DateTime[])", "JsonConvert.DeserializeObject<DateTime[]>({0})", "JsonConvert.SerializeObject({0})", "DateTime[]", typeof(DateTime[]), typeof(DateTime[]), "{0}", "GetValue") },
                { (int)(OscarDbType.TimestampTZ | OscarDbType.Array), new DbToCs("(DateTime[])", "JsonConvert.DeserializeObject<DateTime[]>({0})", "JsonConvert.SerializeObject({0})", "DateTime[]", typeof(DateTime[]), typeof(DateTime[]), "{0}", "GetValue") },
                { (int)(OscarDbType.Date | OscarDbType.Array), new DbToCs("(DateTime[])", "JsonConvert.DeserializeObject<DateTime[]>({0})", "JsonConvert.SerializeObject({0})", "DateTime[]", typeof(DateTime[]), typeof(DateTime[]), "{0}", "GetValue") },
                { (int)(OscarDbType.Time | OscarDbType.Array), new DbToCs("(TimeSpan[])", "JsonConvert.DeserializeObject<TimeSpan[]>({0})", "JsonConvert.SerializeObject({0})", "TimeSpan[]", typeof(TimeSpan[]), typeof(TimeSpan[]), "{0}", "GetValue") },
                { (int)(OscarDbType.TimeTZ | OscarDbType.Array), new DbToCs("(TimeSpan[])", "JsonConvert.DeserializeObject<TimeSpan[]>({0})", "JsonConvert.SerializeObject({0})", "TimeSpan[]", typeof(TimeSpan[]), typeof(TimeSpan[]), "{0}", "GetValue") },
                { (int)(OscarDbType.Interval | OscarDbType.Array), new DbToCs("(TimeSpan[])", "JsonConvert.DeserializeObject<TimeSpan[]>({0})", "JsonConvert.SerializeObject({0})", "TimeSpan[]", typeof(TimeSpan[]), typeof(TimeSpan[]), "{0}", "GetValue") },

                { (int)(OscarDbType.Boolean | OscarDbType.Array), new DbToCs("(bool[])", "JsonConvert.DeserializeObject<bool[]>({0})", "JsonConvert.SerializeObject({0})", "bool[]", typeof(bool[]), typeof(bool[]), "{0}", "GetValue") },
                { (int)(OscarDbType.Bytea | OscarDbType.Array), new DbToCs("(byte[][])", "JsonConvert.DeserializeObject<byte[][]>({0})", "JsonConvert.SerializeObject({0})", "byte[][]", typeof(byte[][]), typeof(byte[][]), "{0}", "GetValue") },
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
            var sql = @" select datname from sys_database";
            var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            return ds.Select(a => a.FirstOrDefault()?.ToString()).ToList();
        }

        public bool ExistsTable(string name, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var tbname = _commonUtils.SplitTableName(name);
            if (tbname?.Length == 1) tbname = new[] { "PUBLIC", tbname[0] };
            if (ignoreCase) tbname = tbname.Select(a => a.ToLower()).ToArray();
            var sql = $@"
select
1
from sys_class a
inner join sys_namespace b on b.oid = a.relnamespace
where {(ignoreCase ? "lower(b.nspname)" : "b.nspname")}={_commonUtils.FormatSql("{0}", tbname[0])} and {(ignoreCase ? "lower(a.relname)" : "a.relname")}={_commonUtils.FormatSql("{0}", tbname[1])} and a.relkind in ('r')
";
            return string.Concat(_orm.Ado.ExecuteScalar(CommandType.Text, sql)) == "1";
        }

        public DbTableInfo GetTableByName(string name, bool ignoreCase = true) => GetTables(null, name, ignoreCase)?.FirstOrDefault();
        public List<DbTableInfo> GetTablesByDatabase(params string[] database) => GetTables(database, null, false);

        public List<DbTableInfo> GetTables(string[] database, string tablename, bool ignoreCase)
        {
            var olddatabase = "";
            using (var conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5)))
            {
                olddatabase = conn.Value.Database;
            }
            string[] tbname = null;
            string[] dbs = database == null || database.Any() == false ? new[] { olddatabase } : database;
            if (string.IsNullOrEmpty(tablename) == false)
            {
                tbname = _commonUtils.SplitTableName(tablename);
                if (tbname?.Length == 1) tbname = new[] { "PUBLIC", tbname[0] };
                if (ignoreCase) tbname = tbname.Select(a => a.ToLower()).ToArray();
                dbs = new[] { olddatabase };
            }
            var tables = new List<DbTableInfo>();

            foreach (var db in dbs)
            {
                if (string.IsNullOrEmpty(db) || string.Compare(db, olddatabase, true) != 0) continue;

                var loc1 = new List<DbTableInfo>();
                var loc2 = new Dictionary<string, DbTableInfo>();
                var loc3 = new Dictionary<string, Dictionary<string, DbColumnInfo>>();

                var sql = $@"
{(tbname == null ? "" : $"select * from (")}select
b.nspname || '.' || a.relname,
b.nspname,
a.relname,
d.description,
'TABLE'
from sys_class a
inner join sys_namespace b on b.oid = a.relnamespace
left join sys_description d on d.objoid = a.oid and objsubid = 0
where b.nspname not in ('DIRECTORIES', 'INFO_SCHEM', 'REPLICATION', 'STAGENT', 'SYSAUDIT', 'SYSDBA', 'SYSFTSDBA', 'SYSSECURE', 'SYS_GLOBAL_TEMP', 'WMSYS') and a.relkind in ('r') 
and b.nspname || '.' || a.relname not in ('PUBLIC.SPATIAL_REF_SYS')

union all

select
b.nspname || '.' || a.relname,
b.nspname,
a.relname,
d.description,
'VIEW'
from sys_class a
inner join sys_namespace b on b.oid = a.relnamespace
left join sys_description d on d.objoid = a.oid and objsubid = 0
where b.nspname not in ('DIRECTORIES', 'INFO_SCHEM', 'REPLICATION', 'STAGENT', 'SYSAUDIT', 'SYSDBA', 'SYSFTSDBA', 'SYSSECURE', 'SYS_GLOBAL_TEMP', 'WMSYS') and a.relkind in ('m','v') 
and b.nspname || '.' || a.relname not in ('PUBLIC.GEOGRAPHY_COLUMNS','PUBLIC.GEOMETRY_COLUMNS','PUBLIC.RASTER_COLUMNS','PUBLIC.RASTER_OVERVIEWS','PUBLIC.DBA_JOBS')
{(tbname == null ? "" : $") ft_dbf where {(ignoreCase ? "lower(nspname)" : "nspname")}={_commonUtils.FormatSql("{0}", tbname[0])} and {(ignoreCase ? "lower(relname)" : "relname")}={_commonUtils.FormatSql("{0}", tbname[1])}")}";
                var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                var loc6 = new List<string[]>();
                var loc66 = new List<string[]>();
                var loc6_1000 = new List<string>();
                var loc66_1000 = new List<string>();
                foreach (object[] row in ds)
                {
                    var object_id = string.Concat(row[0]);
                    var owner = string.Concat(row[1]);
                    var table = string.Concat(row[2]);
                    var comment = string.Concat(row[3]);
                    Enum.TryParse<DbTableType>(string.Concat(row[4]), out var type);
                    loc2.Add(object_id, new DbTableInfo { Id = object_id.ToString(), Schema = owner, Name = table, Comment = comment, Type = type });
                    loc3.Add(object_id, new Dictionary<string, DbColumnInfo>());
                    switch (type)
                    {
                        case DbTableType.VIEW:
                        case DbTableType.TABLE:
                            loc6_1000.Add(object_id);
                            if (loc6_1000.Count >= 500)
                            {
                                loc6.Add(loc6_1000.ToArray());
                                loc6_1000.Clear();
                            }
                            break;
                        case DbTableType.StoreProcedure:
                            loc66_1000.Add(object_id);
                            if (loc66_1000.Count >= 500)
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
ns.nspname || '.' || c.relname as id, 
a.attname,
t.typname,
case when a.atttypmod > 0 and a.atttypmod < 32767 then a.atttypmod - 4 else a.attlen end len,
case when t.typelem = 0 then t.typname else t2.typname end,
case when a.attnotnull then 0 else 1 end as is_nullable,
--e.adsrc as is_identity, pg12以下
(select sys_get_expr(adbin, adrelid) from sys_attrdef where adrelid = e.adrelid and adnum = e.adnum limit 1) is_identity,
d.description as comment,
a.attndims,
case when t.typelem = 0 then t.typtype else t2.typtype end,
ns2.nspname,
a.attnum
from sys_class c
inner join sys_attribute a on a.attnum > 0 and a.attrelid = c.oid
inner join sys_type t on t.oid = a.atttypid
left join sys_type t2 on t2.oid = t.typelem
left join sys_description d on d.objoid = a.attrelid and d.objsubid = a.attnum
left join sys_attrdef e on e.adrelid = a.attrelid and e.adnum = a.attnum
inner join sys_namespace ns on ns.oid = c.relnamespace
inner join sys_namespace ns2 on ns2.oid = t.typnamespace
where {loc8.ToString().Replace("a.table_name", "ns.nspname || '.' || c.relname")}";
                ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                var position = 0;
                foreach (object[] row in ds)
                {
                    var object_id = string.Concat(row[0]);
                    var column = string.Concat(row[1]);
                    var type = string.Concat(row[2]);
                    var max_length = int.Parse(string.Concat(row[3]));
                    var sqlType = string.Concat(row[4]);
                    var is_nullable = string.Concat(row[5]) == "1";
                    var is_identity = string.Concat(row[6]).StartsWith(@"NEXTVAL('") && (string.Concat(row[6]).EndsWith(@"'::text)") || string.Concat(row[6]).EndsWith(@"')"));
                    var comment = string.Concat(row[7]);
                    var defaultValue = string.Concat(row[6]);
                    int attndims = int.Parse(string.Concat(row[8]));
                    string typtype = string.Concat(row[9]);
                    string owner = string.Concat(row[10]);
                    int attnum = int.Parse(string.Concat(row[11]));
                    switch (sqlType.ToLower())
                    {
                        case "bool": case "name": case "bit": case "varbit": case "bpchar": case "varchar": case "bytea": case "text": case "uuid": break;
                        default: max_length *= 8; break;
                    }
                    if (max_length <= 0) max_length = -1;
                    if (type.StartsWith("_"))
                    {
                        type = type.Substring(1);
                        if (attndims == 0) attndims++;
                    }
                    if (sqlType.StartsWith("_")) sqlType = sqlType.Substring(1);
                    if (max_length > 0)
                    {
                        switch (sqlType.ToLower())
                        {
                            //case "numeric": sqlType += $"({max_length})"; break;
                            case "bpchar": case "varchar": case "bytea": case "bit": case "varbit": sqlType += $"({max_length})"; break;
                        }
                    }
                    if (attndims > 0) type += "[]";

                    loc3[object_id].Add(column, new DbColumnInfo
                    {
                        Name = column,
                        MaxLength = max_length,
                        IsIdentity = is_identity,
                        IsNullable = is_nullable,
                        IsPrimary = false,
                        DbTypeText = type,
                        DbTypeTextFull = sqlType,
                        Table = loc2[object_id],
                        Coment = comment,
                        DefaultValue = defaultValue,
                        Position = ++position
                    });
                    loc3[object_id][column].DbType = this.GetDbType(loc3[object_id][column]);
                    loc3[object_id][column].CsType = this.GetCsTypeInfo(loc3[object_id][column]);
                }

                sql = $@"
select
ns.nspname || '.' || d.relname as table_id, 
c.attname,
b.relname as index_id,
case when a.indisunique then 1 else 0 end IsUnique,
case when a.indisprimary then 1 else 0 end IsPrimary,
case when a.indisclustered then 0 else 1 end IsClustered,
--case when sys_index_column_has_property(b.oid, c.attnum, 'desc') = 't' then 1 else 0 end IsDesc,
0,
a.indkey,
c.attnum
from sys_index a
inner join sys_class b on b.oid = a.indexrelid
inner join sys_attribute c on c.attnum > 0 and c.attrelid = b.oid
inner join sys_namespace ns on ns.oid = b.relnamespace
inner join sys_class d on d.oid = a.indrelid
where {loc8.ToString().Replace("a.table_name", "ns.nspname || '.' || d.relname")}
";
                ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                var indexColumns = new Dictionary<string, Dictionary<string, DbIndexInfo>>();
                var uniqueColumns = new Dictionary<string, Dictionary<string, DbIndexInfo>>();
                foreach (object[] row in ds)
                {
                    var object_id = string.Concat(row[0]);
                    var column = string.Concat(row[1]);
                    var index_id = string.Concat(row[2]);
                    var is_unique = string.Concat(row[3]) == "1";
                    var is_primary_key = string.Concat(row[4]) == "1";
                    var is_clustered = string.Concat(row[5]) == "1";
                    var is_desc = string.Concat(row[6]) == "1";
                    var inkey = string.Concat(row[7]).Split(' ');
                    var attnum = int.Parse(string.Concat(row[8]));
                    attnum = int.Parse(inkey[attnum - 1]);
                    foreach (string tc in loc3[object_id].Keys)
                    {
                        if (loc3[object_id][tc].DbTypeText.EndsWith("[]"))
                        {
                            column = tc;
                            break;
                        }
                    }
                    if (loc3.ContainsKey(object_id) == false || loc3[object_id].ContainsKey(column) == false) continue;
                    var loc9 = loc3[object_id][column];
                    if (loc9.IsPrimary == false && is_primary_key) loc9.IsPrimary = is_primary_key;

                    Dictionary<string, DbIndexInfo> loc10 = null;
                    DbIndexInfo loc11 = null;
                    if (!indexColumns.TryGetValue(object_id, out loc10))
                        indexColumns.Add(object_id, loc10 = new Dictionary<string, DbIndexInfo>());
                    if (!loc10.TryGetValue(index_id, out loc11))
                        loc10.Add(index_id, loc11 = new DbIndexInfo());
                    loc11.Columns.Add(new DbIndexColumnInfo { Column = loc9, IsDesc = is_desc });
                    if (is_unique && !is_primary_key)
                    {
                        if (!uniqueColumns.TryGetValue(object_id, out loc10))
                            uniqueColumns.Add(object_id, loc10 = new Dictionary<string, DbIndexInfo>());
                        if (!loc10.TryGetValue(index_id, out loc11))
                            loc10.Add(index_id, loc11 = new DbIndexInfo());
                        loc11.Columns.Add(new DbIndexColumnInfo { Column = loc9, IsDesc = is_desc });
                    }
                }
                foreach (var object_id in indexColumns.Keys)
                {
                    foreach (var column in indexColumns[object_id])
                        loc2[object_id].IndexesDict.Add(column.Key, column.Value);
                }
                foreach (var object_id in uniqueColumns.Keys)
                {
                    foreach (var column in uniqueColumns[object_id])
                    {
                        column.Value.Columns.Sort((c1, c2) => c1.Column.Name.CompareTo(c2.Column.Name));
                        loc2[object_id].UniquesDict.Add(column.Key, column.Value);
                    }
                }

                if (tbname == null)
                {
                    sql = $@"
select
a.pktable_schem || '.' || a.pktable_name,
a.pkcolumn_name,
a.fk_name,
a.fktable_schem || '.' || a.fktable_name,
1,
a.fkcolumn_name
from v_sys_foreign_keys a
where {loc8.ToString().Replace("a.table_name", "a.pktable_schem || '.' || a.pktable_name")}
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
                tables.AddRange(loc1);
            }
            return tables;
        }

        public List<DbEnumInfo> GetEnumsByDatabase(params string[] database) => new List<DbEnumInfo>();
    }
}