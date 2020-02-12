using FreeSql.DatabaseModel;
using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Odbc.MySql
{
    class OdbcMySqlDbFirst : IDbFirst
    {
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        public OdbcMySqlDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public int GetDbType(DbColumnInfo column) => (int)GetOdbcType(column);
        OdbcType GetOdbcType(DbColumnInfo column)
        {
            var is_unsigned = column.DbTypeTextFull.ToLower().EndsWith(" unsigned");
            switch (column.DbTypeText.ToLower())
            {
                case "bit": return OdbcType.Bit;

                case "tinyint": return is_unsigned ? OdbcType.TinyInt : OdbcType.SmallInt;
                case "smallint": return is_unsigned ? OdbcType.Int : OdbcType.SmallInt;
                case "mediumint": return is_unsigned ? OdbcType.Int : OdbcType.Int;
                case "int": return is_unsigned ? OdbcType.BigInt : OdbcType.Int;
                case "bigint": return is_unsigned ? OdbcType.Decimal : OdbcType.BigInt;

                case "real":
                case "double": return OdbcType.Double;
                case "float": return OdbcType.Real;
                case "numeric":
                case "decimal": return OdbcType.Decimal;

                case "year": return OdbcType.Int;
                case "time": return OdbcType.Time;
                case "date": return OdbcType.Date;
                case "timestamp": return OdbcType.Timestamp;
                case "datetime": return OdbcType.DateTime;

                case "tinyblob": return OdbcType.VarBinary;
                case "blob": return OdbcType.VarBinary;
                case "mediumblob": return OdbcType.VarBinary;
                case "longblob": return OdbcType.VarBinary;

                case "binary": return OdbcType.Binary;
                case "varbinary": return OdbcType.VarBinary;

                case "tinytext": return OdbcType.Text;
                case "text": return OdbcType.Text;
                case "mediumtext": return OdbcType.Text;
                case "longtext": return OdbcType.Text;

                case "char": return column.MaxLength == 36 ? OdbcType.Char : OdbcType.VarChar;
                case "varchar": return OdbcType.VarChar;

                case "set": return OdbcType.VarChar;
                case "enum": return OdbcType.VarChar;

                default: return OdbcType.VarChar;
            }
        }

        static readonly Dictionary<int, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)> _dicDbToCs = new Dictionary<int, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)>() {
                { (int)OdbcType.Bit, ("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },

                { (int)OdbcType.TinyInt, ("(sbyte?)", "sbyte.Parse({0})", "{0}.ToString()", "sbyte?", typeof(sbyte), typeof(sbyte?), "{0}.Value", "GetByte") },
                { (int)OdbcType.SmallInt, ("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(short), typeof(short?), "{0}.Value", "GetInt16") },
                { (int)OdbcType.Int, ("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(int), typeof(int?), "{0}.Value", "GetInt32") },
                { (int)OdbcType.BigInt, ("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },

                { (int)OdbcType.Double, ("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
                { (int)OdbcType.Real, ("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?), "{0}.Value", "GetFloat") },
                { (int)OdbcType.Decimal, ("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },

                { (int)OdbcType.Time, ("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },
                { (int)OdbcType.Date, ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)OdbcType.Timestamp, ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)OdbcType.DateTime, ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },

                { (int)OdbcType.Binary, ("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },
                { (int)OdbcType.VarBinary, ("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },

                { (int)OdbcType.Text, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },

                { (int)OdbcType.Char, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)OdbcType.UniqueIdentifier, ("(Guid?)", "Guid.Parse({0})", "{0}.ToString()", "Guid?", typeof(Guid), typeof(Guid?), "{0}", "GetString") },
                { (int)OdbcType.VarChar, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
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
            var sql = @" select schema_name from information_schema.schemata where schema_name not in ('information_schema', 'mysql', 'performance_schema')";
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
                using (var conn = _orm.Ado.MasterPool.Get())
                {
                    if (string.IsNullOrEmpty(conn.Value.Database)) return loc1;
                    database = new[] { conn.Value.Database };
                }
            }
            var databaseIn = string.Join(",", database.Select(a => _commonUtils.FormatSql("{0}", a)));
            var sql = string.Format(@"
select 
concat(a.table_schema, '.', a.table_name) 'id',
a.table_schema 'schema',
a.table_name 'table',
a.table_comment,
a.table_type 'type'
from information_schema.tables a
where a.table_schema in ({0})", databaseIn);
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
                        if (loc6_1000.Count >= 500)
                        {
                            loc6.Add(loc6_1000.ToArray());
                            loc6_1000.Clear();
                        }
                        break;
                    case DbTableType.StoreProcedure:
                        loc66_1000.Add(table.Replace("'", "''"));
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

            sql = string.Format(@"
select
concat(a.table_schema, '.', a.table_name),
a.column_name,
a.data_type,
ifnull(a.character_maximum_length, 0) 'len',
a.column_type,
case when a.is_nullable = 'YES' then 1 else 0 end 'is_nullable',
case when locate('auto_increment', a.extra) > 0 then 1 else 0 end 'is_identity',
a.column_comment 'comment'
from information_schema.columns a
where a.table_schema in ({1}) and {0}
", loc8, databaseIn);
            ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            if (ds == null) return loc1;

            foreach (var row in ds)
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
concat(a.table_schema, '.', a.table_name) 'table_id',
a.column_name,
a.index_name 'index_id',
case when a.non_unique = 0 then 1 else 0 end 'IsUnique',
case when a.index_name = 'PRIMARY' then 1 else 0 end 'IsPrimaryKey',
0 'IsClustered',
0 'IsDesc'
from information_schema.statistics a
where a.table_schema in ({1}) and {0}
", loc8, databaseIn);
            ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            if (ds == null) return loc1;

            var indexColumns = new Dictionary<string, Dictionary<string, DbIndexInfo>>();
            var uniqueColumns = new Dictionary<string, Dictionary<string, DbIndexInfo>>();
            foreach (var row in ds)
            {
                string table_id = string.Concat(row[0]);
                string column = string.Concat(row[1]);
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

            sql = string.Format(@"
select 
concat(a.constraint_schema, '.', a.table_name) 'table_id',
a.column_name,
a.constraint_name 'FKId',
concat(a.referenced_table_schema, '.', a.referenced_table_name) 'ref_table_id',
1 'IsForeignKey',
a.referenced_column_name 'ref_column'
from information_schema.key_column_usage a
where a.constraint_schema in ({1}) and {0} and not isnull(position_in_unique_constraint)
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