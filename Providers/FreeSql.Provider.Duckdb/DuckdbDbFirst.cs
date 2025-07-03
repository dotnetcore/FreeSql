using DuckDB.NET.Native;
using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Duckdb
{
    class DuckdbDbFirst : IDbFirst
    {
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        public DuckdbDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public int GetDbType(DbColumnInfo column) => (int)GetSqlDbType(column);
        DuckDBType GetSqlDbType(DbColumnInfo column)
        {
            var dbtype = column.DbTypeText ?? "";
            var isarray = Regex.IsMatch(dbtype, @"\[\d*\]$") == true;
            if (isarray) dbtype = dbtype.Remove(dbtype.Length - 2);
            DuckDBType ret = DuckDBType.Invalid;
            switch (dbtype.ToLower())
            {
                case "bigint":
                case "int8":
                case "long":
                    _dicDbToCs.TryAdd(dbtype, _dicDbToCs["bigint"]);
                    ret = DuckDBType.BigInt; break;
                case "ubigint": ret = DuckDBType.UnsignedBigInt; break;
                case "bit":
                case "bitstring": ret = DuckDBType.Bit; break;
                case "blob":
                case "bytea":
                case "binary":
                case "varbinary":
                    _dicDbToCs.TryAdd(dbtype, _dicDbToCs["blob"]);
                    ret = DuckDBType.Blob; break;
                case "boolean":
                case "bool":
                case "logical":
                    _dicDbToCs.TryAdd(dbtype, _dicDbToCs["boolean"]);
                    ret = DuckDBType.Boolean; break;
                case "date":
                    _dicDbToCs.TryAdd(dbtype, _dicDbToCs["timestamp"]);
                    ret = DuckDBType.Date; break;
                case "decimal":
                case "numeric":
                    _dicDbToCs.TryAdd(dbtype, _dicDbToCs["decimal(10,2)"]);
                    ret = DuckDBType.Decimal; break;
                case "double":
                case "float8":
                    _dicDbToCs.TryAdd(dbtype, _dicDbToCs["double"]);
                    ret = DuckDBType.Double; break;
                case "float":
                case "float4":
                case "real":
                    _dicDbToCs.TryAdd(dbtype, _dicDbToCs["float"]);
                    ret = DuckDBType.Float; break;
                case "hugeint": ret = DuckDBType.HugeInt; break;
                case "uhugeint":
                    _dicDbToCs.TryAdd(dbtype, _dicDbToCs["hugeint"]);
                    ret = DuckDBType.UnsignedHugeInt; break;
                case "integer":
                case "int4":
                case "int":
                case "signed":
                    _dicDbToCs.TryAdd(dbtype, _dicDbToCs["integer"]);
                    ret = DuckDBType.Integer; break;
                case "uinteger": ret = DuckDBType.UnsignedInteger; break;
                case "interval":
                    _dicDbToCs.TryAdd(dbtype, _dicDbToCs["time"]);
                    ret = DuckDBType.Interval; break;
                case "smallint":
                case "int2":
                case "short": 
                    _dicDbToCs.TryAdd(dbtype, _dicDbToCs["smallint"]);
                    ret = DuckDBType.SmallInt; break;
                case "usmallint": ret = DuckDBType.UnsignedSmallInt; break;
                case "time": ret = DuckDBType.Time; break;
                case "timestamp with time zone":
                case "timestamptz":
                    _dicDbToCs.TryAdd(dbtype, _dicDbToCs["timestamp"]);
                    ret = DuckDBType.TimestampTz; break;
                case "timestamp":
                case "datetime":
                    _dicDbToCs.TryAdd(dbtype, _dicDbToCs["timestamp"]);
                    ret = DuckDBType.Timestamp; break;
                case "tinyint":
                case "int1":
                    _dicDbToCs.TryAdd(dbtype, _dicDbToCs["tinyint"]);
                    ret = DuckDBType.TinyInt; break;
                case "utinyint": ret = DuckDBType.UnsignedTinyInt; break;
                case "uuid": ret = DuckDBType.Uuid; break;
                case "varchar":
                case "char":
                case "bpchar":
                case "text":
                case "string":
                    _dicDbToCs.TryAdd(dbtype, _dicDbToCs["varchar(255)"]);
                    ret = DuckDBType.Varchar; break;
                default:
                    if (dbtype.StartsWith("struct("))
                        ret = DuckDBType.Struct;
                    else if (dbtype.StartsWith("map("))
                        ret = DuckDBType.Map;
                    else if (dbtype.StartsWith("union("))
                        ret = DuckDBType.Union;
                    break;
            }
            return isarray ? (ret | DuckDBType.Array) : ret;
        }

        static ConcurrentDictionary<string, DbToCs> _dicDbToCs = new ConcurrentDictionary<string, DbToCs>(StringComparer.CurrentCultureIgnoreCase);
        static DuckdbDbFirst()
        {
            var defaultDbToCs = new Dictionary<string, DbToCs>() {
                { "boolean", new DbToCs("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },

                { "tinyint", new DbToCs("(byte?)", "byte.Parse({0})", "{0}.ToString()", "byte?", typeof(ushort), typeof(ushort?), "{0}.Value", "GetByte") },
                { "smallint", new DbToCs("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(short), typeof(short?), "{0}.Value", "GetInt16") },
                { "integer", new DbToCs("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(int), typeof(int?), "{0}.Value", "GetInt32") },
                { "bigint", new DbToCs("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },

                { "utinyint", new DbToCs("(sbyte?)", "sbyte.Parse({0})", "{0}.ToString()", "sbyte?", typeof(ushort), typeof(ushort?), "{0}.Value", "GetInt16") },
                { "usmallint", new DbToCs("(ushort?)", "ushort.Parse({0})", "{0}.ToString()", "ushort?", typeof(ushort), typeof(ushort?), "{0}.Value", "GetInt32") },
                { "uinteger", new DbToCs("(uint?)", "uint.Parse({0})", "{0}.ToString()", "uint?", typeof(uint), typeof(uint?), "{0}.Value", "GetInt64") },
                { "ubigint", new DbToCs("(ulong?)", "ulong.Parse({0})", "{0}.ToString()", "ulong?", typeof(ulong), typeof(ulong?), "{0}.Value", "GetDecimal") },

                { "double", new DbToCs("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
                { "float", new DbToCs("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?), "{0}.Value", "GetFloat") },
                { "decimal(10,2)", new DbToCs("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },

                { "time", new DbToCs("(TimeSpan?)", "TimeSpan.FromSeconds(long.Parse({0}))", "{0}.TotalSeconds.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },
                { "timestamp", new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetValue") },

                { "blob", new DbToCs("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },

                { "varchar(255)", new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { "uuid", new DbToCs("(Guid?)", "Guid.Parse({0})", "{0}.ToString()", "Guid?", typeof(Guid), typeof(Guid?), "{0}.Value", "GetGuid") },

                { "hugeint", new DbToCs("(BigInteger?)", "FreeSql.Internal.Utils.ToBigInteger({0})", "{0}.ToString()", "BigInteger?", typeof(BigInteger), typeof(BigInteger?), "{0}.Value", "GetValue") },
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
            return _orm.Ado.ExecuteArray("PRAGMA database_list").Select(a => string.Concat(a[1])).ToList();
        }

        public bool ExistsTable(string name, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var tbname = _commonUtils.SplitTableName(name);
            if (tbname?.Length == 1) tbname = new[] { "main", tbname[0] };
            if (ignoreCase) tbname = tbname.Select(a => a.ToLower()).ToArray();
            var sql = $@" select 1 from {_commonUtils.QuoteSqlName(tbname[0])}.sqlite_master where type='table' and {(ignoreCase ? "lower(tbl_name)" : "tbl_name")}={_commonUtils.FormatSql("{0}", tbname[1])}";
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
                if (tbname?.Length == 1) tbname = new[] { "main", tbname[0] };
                if (ignoreCase) tbname = tbname.Select(a => a.ToLower()).ToArray();
                database = new[] { tbname[0] };
            }
            else if (database == null || database.Any() == false)
                database = GetDatabases().ToArray();
            if (database.Any() == false) return loc1;

            Action<object[], int> addColumn = (row, position) =>
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
                bool is_primary = string.Concat(row[7]) == "1";
                string comment = string.Concat(row[8]);
                string defaultValue = string.Concat(row[9]);
                if (max_length == 0) max_length = -1;
                loc3[table_id].Add(column, new DbColumnInfo
                {
                    Name = column,
                    MaxLength = max_length,
                    IsIdentity = is_identity,
                    IsNullable = is_nullable,
                    IsPrimary = is_primary,
                    DbTypeText = type,
                    DbTypeTextFull = sqlType,
                    Table = loc2[table_id],
                    Comment = comment,
                    DefaultValue = defaultValue,
                    Position = position
                });
                loc3[table_id][column].DbType = this.GetDbType(loc3[table_id][column]);
                loc3[table_id][column].CsType = this.GetCsTypeInfo(loc3[table_id][column]);
            };

            foreach (var db in database)
            {
                var sql = $@"
select 
'{db}.' || tbl_name,
'{db}',
tbl_name,
'',
'TABLE',
sql
from {db}.sqlite_master where type='table'{(tbname == null ? "" : $" and {(ignoreCase ? "lower(tbl_name)" : "tbl_name")}={_commonUtils.FormatSql("{0}", tbname[1])}")}";
                var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) continue;

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

                    if (type == DbTableType.TABLE && table != "sqlite_sequence")
                    {
                        var cols = _orm.Ado.ExecuteArray(CommandType.Text, $"PRAGMA table_info({_commonUtils.FormatSql("{0}", $"{db}.{table}")})");
                        var position = 0;
                        foreach (var col in cols)
                        {
                            var col_name = string.Concat(col[1]);
                            var is_identity = string.Concat(col[4]).StartsWith("nextval('");

                            var ds2item = new object[10];
                            ds2item[0] = table_id;
                            ds2item[1] = col_name;
                            ds2item[2] = Regex.Replace(string.Concat(col[2]), @"\(\d+(\b*,\b*\d+)?\)", "").ToUpper();
                            ds2item[4] = string.Concat(col[2]).ToUpper();
                            ds2item[5] = string.Concat(col[5]) == "False" && string.Concat(col[3]) == "False" ? 1 : 0;
                            ds2item[6] = is_identity ? 1 : 0;
                            ds2item[7] = string.Concat(col[5]) == "True" ? 1 : 0;
                            ds2item[8] = "";
                            ds2item[9] = string.Concat(col[4]);
                            addColumn(ds2item, ++position);
                        }
                    }
                }
                if (loc6_1000.Count > 0) loc6.Add(loc6_1000.ToArray());
                if (loc66_1000.Count > 0) loc66.Add(loc66_1000.ToArray());

                if (loc6.Count == 0) continue;
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
                    //if (compare == 0) compare = c1.Name.CompareTo(c2.Name);
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