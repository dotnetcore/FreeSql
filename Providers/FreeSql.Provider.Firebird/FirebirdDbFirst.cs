using FirebirdSql.Data.FirebirdClient;
using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Firebird
{
    class FirebirdDbFirst : IDbFirst
    {
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        public FirebirdDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public int GetDbType(DbColumnInfo column) => (int)GetFbDbType(column);
        FbDbType GetFbDbType(DbColumnInfo column)
        {
            var dbtype = column.DbTypeText;
            var isarray = dbtype.EndsWith("[]");
            if (isarray) dbtype = dbtype.Remove(dbtype.Length - 2);
            FbDbType ret = FbDbType.VarChar;
            switch (dbtype.ToLower().TrimStart('_'))
            {
                case "bigint": ret = FbDbType.BigInt; break;
                case "blob": ret = FbDbType.Binary; break;
                case "nchar":
                case "char":
                case "character": ret = FbDbType.Char; break;
                case "date": ret = FbDbType.Date; break;
                case "decimal": ret = FbDbType.Decimal; break;
                case "double":
                case "double precision": ret = FbDbType.Double; break;
                case "float": ret = FbDbType.Float; break;
                case "integer":
                case "int": ret = FbDbType.Integer; break;
                case "numeric":
                case "numeric precision": ret = FbDbType.Numeric; break;
                case "smallint": ret = FbDbType.SmallInt; break;
                case "time": ret = FbDbType.Time; break;
                case "timestamp": ret = FbDbType.TimeStamp; break;
                case "varchar":
                case "char varying":
                case "character varying": ret = FbDbType.VarChar; break;

                case "text": ret = FbDbType.Text; break;
                case "boolean": ret = FbDbType.Boolean; break;
                case "char(36)": ret = FbDbType.Guid; break;
            }
            return isarray ? (ret | FbDbType.Array) : ret;
        }

        static readonly Dictionary<int, DbToCs> _dicDbToCs = new Dictionary<int, DbToCs>() {
            { (int)FbDbType.SmallInt, new DbToCs("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(short), typeof(short?), "{0}.Value", "GetInt16") },
            { (int)FbDbType.Integer, new DbToCs("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(int), typeof(int?), "{0}.Value", "GetInt32") },
            { (int)FbDbType.BigInt, new DbToCs("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },
            { (int)FbDbType.Numeric, new DbToCs("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },
            { (int)FbDbType.Float, new DbToCs("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?), "{0}.Value", "GetFloat") },
            { (int)FbDbType.Double, new DbToCs("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
            { (int)FbDbType.Decimal, new DbToCs("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },

            { (int)FbDbType.Char, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
            { (int)FbDbType.VarChar, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
            { (int)FbDbType.Text, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },

            { (int)FbDbType.TimeStamp, new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
            { (int)FbDbType.Date, new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
            { (int)FbDbType.Time, new DbToCs("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },

            { (int)FbDbType.Boolean, new DbToCs("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },
            { (int)FbDbType.Binary, new DbToCs("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },

            { (int)FbDbType.Guid, new DbToCs("(Guid?)", "Guid.Parse({0})", "{0}.ToString()", "Guid", typeof(Guid), typeof(Guid?), "{0}", "GetString") },
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
            var sql = @" select trim(rdb$owner_name) from rdb$roles";
            var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            return ds.Select(a => a.FirstOrDefault()?.ToString()).Distinct().ToList();
        }

        public bool ExistsTable(string name, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var tbname = _commonUtils.SplitTableName(name);
            if (ignoreCase) tbname = tbname.Select(a => a.ToUpper()).ToArray();
            var sql = $" select 1 from rdb$relations where rdb$system_flag=0 and {(ignoreCase ? "upper(trim(rdb$relation_name))" : "trim(rdb$relation_name)")} = {_commonUtils.FormatSql("{0}", tbname.Last())}";
            return string.Concat(_orm.Ado.ExecuteScalar(CommandType.Text, sql)) == "1";
        }

        public DbTableInfo GetTableByName(string name, bool ignoreCase = true) => GetTables(null, name, ignoreCase)?.FirstOrDefault();
        public List<DbTableInfo> GetTablesByDatabase(params string[] database) => GetTables(database, null, false);

        public List<DbTableInfo> GetTables(string[] database, string tablename, bool ignoreCase)
        {
            string[] tbname = null;
            if (string.IsNullOrEmpty(tablename) == false)
            {
                tbname = _commonUtils.SplitTableName(tablename);
                if (ignoreCase) tbname = tbname.Select(a => a.ToUpper()).ToArray();
            }

            var loc1 = new List<DbTableInfo>();
            var loc2 = new Dictionary<string, DbTableInfo>();
            var loc3 = new Dictionary<string, Dictionary<string, DbColumnInfo>>();

            var sql = @"
select
trim(rdb$relation_name) as id,
trim(rdb$owner_name) as owner,
trim(rdb$relation_name) as name,
trim(rdb$external_description) as comment,
rdb$relation_type as type
from rdb$relations
where rdb$system_flag=0" + (tbname == null ? "" : $" and {(ignoreCase ? "upper(trim(rdb$relation_name))" : "trim(rdb$relation_name)")} = {_commonUtils.FormatSql("{0}", tbname.Last())}");
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
                DbTableType type = DbTableType.TABLE;
                switch (string.Concat(row[4]))
                {
                    case "1": type = DbTableType.VIEW; break;
                }
                if (database?.Length == 1)
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
                loc8.Append("trim(d.rdb$relation_name) in (");
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
trim(d.rdb$relation_name),
trim(a.rdb$field_name),
case
  when b.rdb$field_sub_type = 2 then 'DECIMAL'
  when b.rdb$field_type = 14 then 'CHAR'
  when b.rdb$field_type = 37 then 'VARCHAR'
  when b.rdb$field_type = 8 then 'INTEGER'
  when b.rdb$field_type = 16 then 'BIGINT'
  when b.rdb$field_type = 27 then 'DOUBLE PRECISION'
  when b.rdb$field_type = 7 then 'SMALLINT'
  else
    (select trim(rdb$type_name) from rdb$types where rdb$type = b.rdb$field_type and rdb$field_name = 'RDB$FIELD_TYPE' rows 1) || 
    coalesce((select ' SUB_TYPE ' || rdb$type from rdb$types where b.rdb$field_type = 261 and rdb$type = b.rdb$field_sub_type and rdb$field_name = 'RDB$FIELD_SUB_TYPE' rows 1),'')
  end || trim(case when b.rdb$dimensions = 1 then '[]' else '' end),
b.rdb$character_length,
case
  when b.rdb$field_sub_type = 2 then (select 'DECIMAL(' || rdb$field_precision || ',' || abs(rdb$field_scale) || ')' from rdb$types where b.rdb$field_sub_type = 2 and rdb$type = b.rdb$field_sub_type and rdb$field_name = 'RDB$FIELD_SUB_TYPE' rows 1)
  when b.rdb$field_type = 14 then 'CHAR(' || b.rdb$character_length || ')'
  when b.rdb$field_type = 37 then 'VARCHAR(' || b.rdb$character_length || ')'
  when b.rdb$field_type = 8 then 'INTEGER'
  when b.rdb$field_type = 16 then 'BIGINT'
  when b.rdb$field_type = 27 then 'DOUBLE PRECISION'
  when b.rdb$field_type = 7 then 'SMALLINT'
  else
    (select trim(rdb$type_name) from rdb$types where rdb$type = b.rdb$field_type and rdb$field_name = 'RDB$FIELD_TYPE' rows 1) || 
    coalesce((select ' SUB_TYPE ' || rdb$type from rdb$types where b.rdb$field_type = 261 and rdb$type = b.rdb$field_sub_type and rdb$field_name = 'RDB$FIELD_SUB_TYPE' rows 1),'')
  end || trim(case when b.rdb$dimensions = 1 then '[]' else '' end),
case when a.rdb$null_flag = 1 then 0 else 1 end,
{((_orm.Ado as FirebirdAdo)?.IsFirebird2_5 == true ? "0" : "case when a.rdb$identity_type = 1 then 1 else 0 end")},
a.rdb$description,
a.rdb$default_value
from rdb$relation_fields a
inner join rdb$fields b on b.rdb$field_name = a.rdb$field_source
inner join rdb$relations d on d.rdb$relation_name = a.rdb$relation_name
where a.rdb$system_flag = 0 and {loc8}
order by a.rdb$relation_name, a.rdb$field_position
";
            ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            if (ds == null) return loc1;

            var position = 0;
            foreach (var row in ds)
            {
                string table_id = string.Concat(row[0]);
                string column = string.Concat(row[1]);
                string type = string.Concat(row[2]).Trim();
                //long max_length = long.Parse(string.Concat(row[3]));
                string sqlType = string.Concat(row[4]).Trim();
                var m_len = Regex.Match(sqlType, @"\w+\((\d+)");
                int max_length = m_len.Success ? int.Parse(m_len.Groups[1].Value) : -1;
                bool is_nullable = string.Concat(row[5]) == "1";
                bool is_identity = string.Concat(row[6]) == "1";
                string comment = string.Concat(row[7]);
                string defaultValue = string.Concat(row[8]);
                if (max_length == 0) max_length = -1;
                if (database?.Length == 1)
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

            sql = string.Format(@"
select
trim(d.rdb$relation_name),
trim(c.rdb$field_name),
trim(d.rdb$index_name),
case when d.rdb$unique_flag = 1 then 1 else 0 end,
case when exists(select first 1 1 from rdb$relation_constraints where rdb$index_name = d.rdb$index_name and rdb$constraint_type = 'PRIMARY KEY') then 1 else 0 end,
0,
0
from rdb$indices d
inner join rdb$index_segments c on c.rdb$index_name = d.rdb$index_name
where {0}", loc8);
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
                if (database?.Length == 1)
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