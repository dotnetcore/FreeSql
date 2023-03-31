using FreeSql.DatabaseModel;
using FreeSql.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using XuguClient;

namespace FreeSql.Xugu
{
    class XuguDbFirst : IDbFirst
    {
        public readonly string DefaultSchema = "SYSDBA";//默认模式
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        public XuguDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public bool IsPg10 => ServerVersion >= 10;
        public int ServerVersion
        {
            get
            {
                if (_ServerVersionValue == 0 && _orm.Ado.MasterPool != null)
                    using (var conn = _orm.Ado.MasterPool.Get())
                    {
                        try
                        {
                            _ServerVersionValue = int.Parse(conn.Value.ServerVersion.Split('.')[0]);
                        }
                        catch
                        {
                            _ServerVersionValue = 9;
                        }
                    }
                return _ServerVersionValue;
            }
        }
        int _ServerVersionValue = 0;

        public int GetDbType(DbColumnInfo column) => (int)GetXGDbType(column);
        XGDbType GetXGDbType(DbColumnInfo column)
        {
            var dbtype = column.DbTypeText;
            var isarray = dbtype?.EndsWith("[]") == true;
            if (isarray) dbtype = dbtype.Remove(dbtype.Length - 2);
            XGDbType ret = XGDbType.LongVarChar;
            switch (dbtype?.ToUpper().TrimStart('_'))
            {
                case "SMALLINT": ret = XGDbType.SmallInt; break;
                case "INTEGER": ret = XGDbType.Int; break;
                case "BIGINT": ret = XGDbType.BigInt; break;
                case "NUMERIC": ret = XGDbType.Numeric; break;
                case "FLOAT": ret = XGDbType.Real; break;
                case "DOUBLE": ret = XGDbType.Double; break;
                //case "money": ret = XGDbType.; break;

                case "CHAR": ret = XGDbType.Char; break;
                case "VARCHAR": ret = XGDbType.VarChar; break;
                case "CLOB": ret = XGDbType.LongVarChar; break;

                //case "timestamp": ret = XGDbType.DateTime; break;
                //case "timestamptz": ret = XGDbType.DateTime; break;
                //case "date": ret = XGDbType.Date; break;
                //case "time": ret = XGDbType.Time; break;
                //case "timetz": ret = XGDbType.TimeTz; break;
                //case "interval": ret = XGDbType.Interval; break;

                case "BOOLEAN": ret = XGDbType.Bool; break;
                //case "bytea": ret = XGDbType.Bytea; break;
                //case "bit": ret = XGDbType.Bool; break;
                //case "varbit": ret = XGDbType.Varbit; break;

                //case "point": ret = XGDbType.Point; break;
                //case "line": ret = XGDbType.Line; break;
                //case "lseg": ret = XGDbType.LSeg; break;
                //case "box": ret = XGDbType.Box; break;
                //case "path": ret = XGDbType.Path; break;
                //case "polygon": ret = XGDbType.Polygon; break;
                //case "circle": ret = XGDbType.Circle; break;

                //case "cidr": ret = XGDbType.Cidr; break;
                //case "inet": ret = XGDbType.Inet; break;
                //case "macaddr": ret = XGDbType.MacAddr; break;

                //case "json": ret = XGDbType.Json; break;
                //case "jsonb": ret = XGDbType.Jsonb; break;
                //case "uuid": ret = XGDbType.Uuid; break;

                //case "int4range": ret = XGDbType.Range | XGDbType.Integer; break;
                //case "int8range": ret = XGDbType.Range | XGDbType.Bigint; break;
                //case "numrange": ret = XGDbType.Range | XGDbType.Numeric; break;
                //case "tsrange": ret = XGDbType.Range | XGDbType.Timestamp; break;
                //case "tstzrange": ret = XGDbType.Range | XGDbType.TimestampTz; break;
                //case "daterange": ret = XGDbType.Range | XGDbType.Date; break;

                //case "hstore": ret = XGDbType.Hstore; break;
                //case "geometry": ret = XGDbType.Geometry; break;
            }
            return ret  ;
            //return isarray ? (ret | XGDbType.Array) : ret;
        }

        static readonly Dictionary<int, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)> _dicDbToCs = new Dictionary<int, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)>() {
                { (int)XGDbType.SmallInt, ("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(short), typeof(short?), "{0}.Value", "GetInt16") },
                { (int)XGDbType.Int, ("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(int), typeof(int?), "{0}.Value", "GetInt32") },
                { (int)XGDbType.BigInt, ("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },
                { (int)XGDbType.Numeric, ("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },
                { (int)XGDbType.Real, ("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?), "{0}.Value", "GetFloat") },
                { (int)XGDbType.Double, ("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
              
                { (int)XGDbType.Char, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)XGDbType.VarChar, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)XGDbType.LongVarChar, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },

                
                { (int)XGDbType.DateTime, ("(DateTime?)", "new DateTime({0})", "{0}.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetValue") },
              

                { (int)XGDbType.Bool, ("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },
                 
                    
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
            var sql = @" select DB_NAME from all_databases where DROPED=FALSE AND USER_ID>0";
            var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            return ds.Select(a => a.FirstOrDefault()?.ToString()).ToList();
        }

        public bool ExistsTable(string name, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var tbname = _commonUtils.SplitTableName(name);
            if (tbname?.Length == 1) tbname = new[] { DefaultSchema, tbname[0] };
            var sql = string.Format("select 1 from all_tables a inner join all_schemas b on b.schema_id = a.schema_id where b.schema_name || '.' || a.table_name = '{0}.{1}'", tbname);
           
            return string.Concat(_orm.Ado.ExecuteScalar(CommandType.Text, sql)) == "1";
        }

        public DbTableInfo GetTableByName(string name, bool ignoreCase = true) => GetTables(null, name, ignoreCase)?.FirstOrDefault();
        public List<DbTableInfo> GetTablesByDatabase(params string[] database) => GetTables(database, null,false);

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
                if (tbname?.Length == 1) tbname = new[] { DefaultSchema, tbname[0] };
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
{(tbname == null ? "" : $"select * from (")}select b.schema_name || '.' || a.table_name as ns,
b.schema_name,
a.table_name,
a.comments,
'TABLE' AS type
from all_tables a 
inner join all_schemas b on b.schema_id = a.schema_id
where a.IS_SYS=FALSE

union all

select b.schema_name || '.' || a.view_name as ns,
b.schema_name,
a.view_name,
a.comments,
'VIEW' AS type
from all_views a 
inner join all_schemas b on b.schema_id = a.schema_id
where a.IS_SYS=FALSE
{(tbname == null ? "" : $") ft_dbf where schema_name ={_commonUtils.FormatSql("{0}", tbname[0])} and ft_dbf.Table_Name={_commonUtils.FormatSql("{0}", tbname[1])}")}";
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
ns.schema_name || '.' || tb.table_name as id,
a.COL_NAME,
a.TYPE_NAME,
a.SCALE,
case when a.NOT_NULL then '0' else '1' end as is_nullable,
seq.SEQ_ID,
seq.IS_SYS,
seq.MIN_VAL,
seq.STEP_VAL,
a.COMMENTS,
tb.table_name,
a.def_val,
ns.schema_id,
a.`VARYING`
from all_columns as a  
left join all_tables tb on a.table_id=tb.table_id
left join all_schemas ns on tb.schema_id = ns.schema_id
left join all_sequences seq on seq.SEQ_ID=a.Serial_ID
where {loc8.ToString().Replace("a.table_name", "ns.schema_name || '.' || tb.table_name")}";

                ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;
                var position = 1;
                foreach (object[] row in ds)
                {
                    var object_id = string.Concat(row[0]);
                    var column = string.Concat(row[1]);
                    var type = string.Concat(row[2]);
                    var max_length = int.Parse(string.Concat(row[3]));
                    //var sqlType = string.Concat(row[4]);
                    var is_nullable = string.Concat(row[5]) == "1";

                    long.TryParse(row[5]?.ToString() ?? "0", out long SEQ_ID);
                    var SEQ_IS_SYS = (row[6]?.ToString() ?? "false")?.ToLower();

                    var is_identity = (SEQ_ID > 0 && SEQ_IS_SYS == "true");
                    var comment = string.Concat(row[9]);


                    var defaultValue = string.Concat(row[11]);
                    //int attndims = int.Parse(string.Concat(row[8]));
                    //string typtype = string.Concat(row[9]);
                    //string owner = string.Concat(row[10]);
                    //int attnum = int.Parse(string.Concat(row[11]));

                    //switch (sqlType.ToLower())
                    //{
                    //    case "bool": case "name": case "bit": case "varbit": case "bpchar": case "varchar": case "bytea": case "text": case "uuid": break;
                    //    default: max_length *= 8; break;
                    //}
                    if (max_length <= 0) max_length = -1;
                    //if (type.StartsWith("_"))
                    //{
                    //    type = type.Substring(1);
                    //    if (attndims == 0) attndims++;
                    //}
                    var sqlType = type;
                    if (max_length > 0)
                    {
                        switch (type.ToUpper())
                        {
                            //case "numeric": sqlType += $"({max_length})"; break;

                            case "NUMERIC":
                                {
                                    var data_precision = max_length % 65536;
                                    var data_scale = max_length / 65536;
                                    sqlType += $"({data_scale},{data_precision})"; break;
                                }
                               
                            //case "DATETIME":
                            //    break;
                            default:
                                sqlType += $"({max_length})"; break;
                        }
                    }
                    //if (attndims > 0) type += "[]";

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
                        Comment = comment,
                        DefaultValue = defaultValue,
                        Position = position
                    });
                    loc3[object_id][column].DbType = this.GetDbType(loc3[object_id][column]);
                    loc3[object_id][column].CsType = this.GetCsTypeInfo(loc3[object_id][column]);
                    position++;
                }

                sql = $@"
select 
ns.SCHEMA_NAME || '.' || t.table_name as table_id, 
keys,
a.index_name,
case when a.IS_UNIQUE  then 1 else 0 end as IsUnique,
case when a.IS_Primary then 1 else 0 end as IsPrimary,
0,
0,
t.table_name,
ns.SCHEMA_NAME,
a.IS_SYS
from all_INDEXES a
left join all_tables t on a.TABLE_ID=t.TABLE_ID
left join all_schemas ns on t.SCHEMA_ID=ns.SCHEMA_ID
where IS_SYS=false and {loc8.ToString().Replace("a.table_name", "ns.SCHEMA_NAME || '.' || t.table_name")}
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
                    var is_clustered = false;//= string.Concat(row[5]) == "1";
                    var is_desc = false;//string.Concat(row[6]) == "1";
                    //var inkey = string.Concat(row[7]).Split(' ');
                    //var attnum = int.Parse(string.Concat(row[8]));
                    //attnum = int.Parse(inkey[attnum - 1]);
                    //foreach (string tc in loc3[object_id].Keys) //bug: https://github.com/2881099/FreeSql.Wiki.VuePress/issues/9
                    //{
                    //    if (loc3[object_id][tc].DbTypeText.EndsWith("[]"))
                    //    {
                    //        column = tc;
                    //        break;
                    //    }
                    //}
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
schema_name || '.' || b.table_name as table_id,
cons_name as FKId,
cons_type,
define
from all_constraints as a
left join all_tables as b on a.Table_ID=b.Table_ID
left Join all_SCHEMAS AS c on b.SCHEMA_ID=c.SCHEMA_ID
where  IS_SYS=false AND {loc8.ToString().Replace("a.table_name", "schema_name || '.' || b.table_name")}
";
                    ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                    if (ds == null) return loc1;

                    var fkColumns = new Dictionary<string, Dictionary<string, DbForeignInfo>>();
                    foreach (object[] row in ds)
                    {
                        var table_id = string.Concat(row[0]);
                        var column = row[3] as string[];
                        var fk_id = string.Concat(row[1]);
                        var ref_table_id = string.Concat(row[0]);
                        var is_foreign_key = string.Concat(row[2]) == "F";
                        var referenced_column = row[5] as string[];
                        //var referenced_db = string.Concat(row[6]);
                        //var referenced_table = string.Concat(row[7]);

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

        public List<DbEnumInfo> GetEnumsByDatabase(params string[] database)
        {
            return new List<DbEnumInfo>();
        }

    }
}