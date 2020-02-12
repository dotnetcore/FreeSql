using FreeSql.DatabaseModel;
using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Odbc.PostgreSQL
{
    class OdbcPostgreSQLDbFirst : IDbFirst
    {
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        public OdbcPostgreSQLDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public int GetDbType(DbColumnInfo column) => (int)GetOdbcType(column);
        OdbcType GetOdbcType(DbColumnInfo column)
        {
            var dbtype = column.DbTypeText;
            var isarray = dbtype.EndsWith("[]");
            if (isarray) dbtype = dbtype.Remove(dbtype.Length - 2);
            OdbcType ret = OdbcType.VarChar;
            switch (dbtype.ToLower().TrimStart('_'))
            {
                case "int2": ret = OdbcType.SmallInt; break;
                case "int4": ret = OdbcType.Int; break;
                case "int8": ret = OdbcType.BigInt; break;
                case "numeric": ret = OdbcType.Numeric; break;
                case "float4": ret = OdbcType.Real; break;
                case "float8": ret = OdbcType.Double; break;
                case "money": ret = OdbcType.Numeric; break;

                case "bpchar": ret = OdbcType.Char; break;
                case "varchar": ret = OdbcType.VarChar; break;
                case "text": ret = OdbcType.Text; break;

                case "timestamp": ret = OdbcType.DateTime; break;
                case "timestamptz": ret = OdbcType.DateTime; break;
                case "date": ret = OdbcType.Date; break;
                case "time": ret = OdbcType.Time; break;
                case "timetz": ret = OdbcType.Time; break;
                case "interval": ret = OdbcType.Time; break;

                case "bool": ret = OdbcType.Bit; break;
                case "bytea": ret = OdbcType.VarBinary; break;
                case "bit": ret = OdbcType.Bit; break;
                case "varbit": ret = OdbcType.VarBinary; break;

                case "json": ret = OdbcType.VarChar; break;
                case "jsonb": ret = OdbcType.VarChar; break;
                case "uuid": ret = OdbcType.UniqueIdentifier; break;
            }
            return ret;
        }

        static readonly Dictionary<int, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)> _dicDbToCs = new Dictionary<int, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)>() {
                { (int)OdbcType.SmallInt, ("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(short), typeof(short?), "{0}.Value", "GetInt16") },
                { (int)OdbcType.Int, ("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(int), typeof(int?), "{0}.Value", "GetInt32") },
                { (int)OdbcType.BigInt, ("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },
                { (int)OdbcType.Numeric, ("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },
                { (int)OdbcType.Real, ("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?), "{0}.Value", "GetFloat") },
                { (int)OdbcType.Double, ("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },

                { (int)OdbcType.Char, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)OdbcType.VarChar, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)OdbcType.Text, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },

                { (int)OdbcType.DateTime,  ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)OdbcType.Date,  ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)OdbcType.Time, ("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },

                { (int)OdbcType.Bit, ("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },
                { (int)OdbcType.VarBinary, ("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },

                { (int)OdbcType.UniqueIdentifier, ("(Guid?)", "Guid.Parse({0})", "{0}.ToString()", "Guid", typeof(Guid), typeof(Guid?), "{0}", "GetString") },
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
            var sql = @" select datname from pg_database where datname not in ('template1', 'template0')";
            var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            return ds.Select(a => a.FirstOrDefault()?.ToString()).ToList();
        }

        public List<DbTableInfo> GetTablesByDatabase(params string[] database)
        {
            var olddatabase = "";
            using (var conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5)))
            {
                olddatabase = conn.Value.Database;
            }
            var dbs = database == null || database.Any() == false ? new[] { olddatabase } : database;
            var tables = new List<DbTableInfo>();

            foreach (var db in dbs)
            {
                if (string.IsNullOrEmpty(db) || string.Compare(db, olddatabase, true) != 0) continue;

                var loc1 = new List<DbTableInfo>();
                var loc2 = new Dictionary<string, DbTableInfo>();
                var loc3 = new Dictionary<string, Dictionary<string, DbColumnInfo>>();

                var sql = $@"
select
b.nspname || '.' || a.tablename,
a.schemaname,
a.tablename ,
d.description,
'TABLE'
from pg_tables a
inner join pg_namespace b on b.nspname = a.schemaname
inner join pg_class c on c.relnamespace = b.oid and c.relname = a.tablename
left join pg_description d on d.objoid = c.oid and objsubid = 0
where a.schemaname not in ('pg_catalog', 'information_schema', 'topology')
and b.nspname || '.' || a.tablename not in ('public.spatial_ref_sys')

union all

select
b.nspname || '.' || a.relname,
b.nspname,
a.relname,
d.description,
'VIEW'
from pg_class a
inner join pg_namespace b on b.oid = a.relnamespace
left join pg_description d on d.objoid = a.oid and objsubid = 0
where b.nspname not in ('pg_catalog', 'information_schema') and a.relkind in ('m','v') 
and b.nspname || '.' || a.relname not in ('public.geography_columns','public.geometry_columns','public.raster_columns','public.raster_overviews')
";
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
coalesce((select 1 from pg_sequences where sequencename = {0} || '_' || {1} || '_' || a.attname || '_sequence_name' limit 1),0) is_identity,
--e.adsrc as is_identity,
d.description as comment,
a.attndims,
case when t.typelem = 0 then t.typtype else t2.typtype end,
ns2.nspname,
a.attnum
from pg_class c
inner join pg_attribute a on a.attnum > 0 and a.attrelid = c.oid
inner join pg_type t on t.oid = a.atttypid
left join pg_type t2 on t2.oid = t.typelem
left join pg_description d on d.objoid = a.attrelid and d.objsubid = a.attnum
left join pg_attrdef e on e.adrelid = a.attrelid and e.adnum = a.attnum
inner join pg_namespace ns on ns.oid = c.relnamespace
inner join pg_namespace ns2 on ns2.oid = t.typnamespace
where {loc8.ToString().Replace("a.table_name", "ns.nspname || '.' || c.relname")}";
                ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                foreach (object[] row in ds)
                {
                    var object_id = string.Concat(row[0]);
                    var column = string.Concat(row[1]);
                    var type = string.Concat(row[2]);
                    var max_length = int.Parse(string.Concat(row[3]));
                    var sqlType = string.Concat(row[4]);
                    var is_nullable = string.Concat(row[5]) == "1";
                    var is_identity = string.Concat(row[6]) == "1"; //string.Concat(row[6]).StartsWith(@"nextval('") && string.Concat(row[6]).EndsWith(@"'::regclass)");
                    var comment = string.Concat(row[7]);
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
                        Coment = comment
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
case when pg_index_column_has_property(b.oid, c.attnum, 'desc') = 't' then 1 else 0 end IsDesc,
a.indkey::text,
c.attnum
from pg_index a
inner join pg_class b on b.oid = a.indexrelid
inner join pg_attribute c on c.attnum > 0 and c.attrelid = b.oid
inner join pg_namespace ns on ns.oid = b.relnamespace
inner join pg_class d on d.oid = a.indrelid
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

                sql = $@"
select
ns.nspname || '.' || b.relname as table_id, 
array(select attname from pg_attribute where attrelid = a.conrelid and attnum = any(a.conkey)) as column_name,
a.conname as FKId,
ns2.nspname || '.' || c.relname as ref_table_id, 
1 as IsForeignKey,
array(select attname from pg_attribute where attrelid = a.confrelid and attnum = any(a.confkey)) as ref_column,
null ref_sln,
null ref_table
from  pg_constraint a
inner join pg_class b on b.oid = a.conrelid
inner join pg_class c on c.oid = a.confrelid
inner join pg_namespace ns on ns.oid = b.relnamespace
inner join pg_namespace ns2 on ns2.oid = c.relnamespace
where {loc8.ToString().Replace("a.table_name", "ns.nspname || '.' || b.relname")}
";
                ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                var fkColumns = new Dictionary<string, Dictionary<string, DbForeignInfo>>();
                foreach (object[] row in ds)
                {
                    var table_id = string.Concat(row[0]);
                    var column = row[1] as string[];
                    var fk_id = string.Concat(row[2]);
                    var ref_table_id = string.Concat(row[3]);
                    var is_foreign_key = string.Concat(row[4]) == "1";
                    var referenced_column = row[5] as string[];
                    var referenced_db = string.Concat(row[6]);
                    var referenced_table = string.Concat(row[7]);

                    if (loc2.ContainsKey(ref_table_id) == false) continue;

                    Dictionary<string, DbForeignInfo> loc12 = null;
                    DbForeignInfo loc13 = null;
                    if (!fkColumns.TryGetValue(table_id, out loc12))
                        fkColumns.Add(table_id, loc12 = new Dictionary<string, DbForeignInfo>());
                    if (!loc12.TryGetValue(fk_id, out loc13))
                        loc12.Add(fk_id, loc13 = new DbForeignInfo { Table = loc2[table_id], ReferencedTable = loc2[ref_table_id] });

                    for (int a = 0; a < column.Length; a++)
                    {
                        loc13.Columns.Add(loc3[table_id][column[a]]);
                        loc13.ReferencedColumns.Add(loc3[ref_table_id][referenced_column[a]]);
                    }
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
                tables.AddRange(loc1);
            }
            return tables;
        }

        public List<DbEnumInfo> GetEnumsByDatabase(params string[] database)
        {
            if (database == null || database.Length == 0) return new List<DbEnumInfo>();
            var drs = _orm.Ado.Query<(string name, string label)>(CommandType.Text, _commonUtils.FormatSql(@"
select
ns.nspname || '.' || a.typname,
b.enumlabel
from pg_type a
inner join pg_enum b on b.enumtypid = a.oid
inner join pg_namespace ns on ns.oid = a.typnamespace
where a.typtype = 'e' and ns.nspname in (SELECT ""schema_name"" FROM information_schema.schemata where catalog_name in {0})", database));
            var ret = new Dictionary<string, Dictionary<string, string>>();
            foreach (var dr in drs)
            {
                if (ret.TryGetValue(dr.name, out var labels) == false) ret.Add(dr.name, labels = new Dictionary<string, string>());
                var key = dr.label;
                if (Regex.IsMatch(key, @"^[\u0391-\uFFE5a-zA-Z_\$][\u0391-\uFFE5a-zA-Z_\$\d]*$") == false)
                    key = $"Unkown{ret[dr.name].Count + 1}";
                if (labels.ContainsKey(key) == false) labels.Add(key, dr.label);
            }
            return ret.Select(a => new DbEnumInfo { Name = a.Key, Labels = a.Value }).ToList();
        }
    }
}