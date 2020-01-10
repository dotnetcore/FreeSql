using FreeSql.DatabaseModel;
using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Odbc.SqlServer
{
    class OdbcSqlServerDbFirst : IDbFirst
    {
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        public OdbcSqlServerDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public int GetDbType(DbColumnInfo column) => (int)GetSqlDbType(column);
        OdbcType GetSqlDbType(DbColumnInfo column)
        {
            switch (column.DbTypeText.ToLower())
            {
                case "bit": return OdbcType.Bit;
                case "tinyint": return OdbcType.TinyInt;
                case "smallint": return OdbcType.SmallInt;
                case "int": return OdbcType.Int;
                case "bigint": return OdbcType.BigInt;
                case "numeric":
                case "decimal": return OdbcType.Decimal;
                case "smallmoney": return OdbcType.Decimal;
                case "money": return OdbcType.Decimal;
                case "float": return OdbcType.Double;
                case "real": return OdbcType.Real;
                case "date": return OdbcType.Date;
                case "datetime":
                case "datetime2": return OdbcType.DateTime;
                case "datetimeoffset": return OdbcType.DateTime;
                case "smalldatetime": return OdbcType.SmallDateTime;
                case "time": return OdbcType.Time;
                case "char": return OdbcType.Char;
                case "varchar": return OdbcType.VarChar;
                case "text": return OdbcType.Text;
                case "nchar": return OdbcType.NChar;
                case "nvarchar": return OdbcType.NVarChar;
                case "ntext": return OdbcType.NText;
                case "binary": return OdbcType.Binary;
                case "varbinary": return OdbcType.VarBinary;
                case "image": return OdbcType.Image;
                case "timestamp": return OdbcType.Timestamp;
                case "uniqueidentifier": return OdbcType.UniqueIdentifier;
                default: return OdbcType.NVarChar;
            }
        }

        static readonly Dictionary<int, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)> _dicDbToCs = new Dictionary<int, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)>() {
                { (int)OdbcType.Bit, ("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },

                { (int)OdbcType.TinyInt, ("(byte?)", "byte.Parse({0})", "{0}.ToString()", "byte?", typeof(byte), typeof(byte?), "{0}.Value", "GetByte") },
                { (int)OdbcType.SmallInt, ("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(short), typeof(short?), "{0}.Value", "GetInt16") },
                { (int)OdbcType.Int, ("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(int), typeof(int?), "{0}.Value", "GetInt32") },
                { (int)OdbcType.BigInt, ("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },

                { (int)OdbcType.Decimal, ("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },
                { (int)OdbcType.Double, ("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
                { (int)OdbcType.Real, ("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?), "{0}.Value", "GetFloat") },

                { (int)OdbcType.Time, ("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },
                { (int)OdbcType.Date, ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)OdbcType.DateTime, ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)OdbcType.SmallDateTime, ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },

                { (int)OdbcType.Binary, ("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },
                { (int)OdbcType.VarBinary, ("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },
                { (int)OdbcType.Image, ("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },
                { (int)OdbcType.Timestamp, ("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },

                { (int)OdbcType.Char, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)OdbcType.VarChar, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)OdbcType.Text, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)OdbcType.NChar, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)OdbcType.NVarChar, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)OdbcType.NText, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },

                { (int)OdbcType.UniqueIdentifier, ("(Guid?)", "Guid.Parse({0})", "{0}.ToString()", "Guid?", typeof(Guid), typeof(Guid?), "{0}.Value", "GetGuid") },
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
            var sql = @" select name from sys.databases where name not in ('master','tempdb','model','msdb')";
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
                if (string.IsNullOrEmpty(db)) continue;

                var loc1 = new List<DbTableInfo>();
                var loc2 = new Dictionary<int, DbTableInfo>();
                var loc3 = new Dictionary<int, Dictionary<string, DbColumnInfo>>();

                var sql = $@"
use [{db}];
select 
 a.Object_id
,b.name 'Owner'
,a.name 'Name'
,(select value from sys.extended_properties where major_id = a.object_id AND minor_id = 0 AND name = 'MS_Description') 'Comment'
,'TABLE' type
from sys.tables a
inner join sys.schemas b on b.schema_id = a.schema_id
where not(b.name = 'dbo' and a.name = 'sysdiagrams')
union all
select
 a.Object_id
,b.name 'Owner'
,a.name 'Name'
,(select value from sys.extended_properties where major_id = a.object_id AND minor_id = 0 AND name = 'MS_Description') 'Comment'
,'VIEW' type
from sys.views a
inner join sys.schemas b on b.schema_id = a.schema_id
union all
select 
 a.Object_id
,b.name 'Owner'
,a.name 'Name'
,(select value from sys.extended_properties where major_id = a.object_id AND minor_id = 0 AND name = 'MS_Description') 'Comment'
,'StoreProcedure' type
from sys.procedures a
inner join sys.schemas b on b.schema_id = a.schema_id
where a.type = 'P' and charindex('diagram', a.name) = 0
order by type desc, b.name, a.name
;
use [{olddatabase}];
";
                var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                var loc6 = new List<int[]>();
                var loc66 = new List<int[]>();
                var loc6_1000 = new List<int>();
                var loc66_1000 = new List<int>();
                foreach (object[] row in ds)
                {
                    int object_id = int.Parse(string.Concat(row[0]));
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
                Func<List<int[]>, StringBuilder> getloc8Sb = loclist =>
                {
                    if (loclist.Count == 0) return new StringBuilder();
                    var loc8sb = new StringBuilder().Append("(");
                    for (var loc8sbidx = 0; loc8sbidx < loclist.Count; loc8sbidx++)
                    {
                        if (loc8sbidx > 0) loc8sb.Append(" OR ");
                        loc8sb.Append("a.table_name in (");
                        for (var loc8sbidx2 = 0; loc8sbidx2 < loclist[loc8sbidx].Length; loc8sbidx2++)
                        {
                            if (loc8sbidx2 > 0) loc8sb.Append(",");
                            loc8sb.Append(loclist[loc8sbidx][loc8sbidx2]);
                        }
                        loc8sb.Append(")");
                    }
                    loc8sb.Append(")");
                    return loc8sb;
                };
                var loc8 = getloc8Sb(loc6);
                var loc88 = getloc8Sb(loc66);

                var tsql_place = @"

select 
isnull(e.name,'') + '.' + isnull(d.name,'')
,a.Object_id
,a.name 'Column'
,b.name 'Type'
,case
 when b.name in ('Text', 'NText', 'Image') then -1
 when b.name in ('NChar', 'NVarchar') then a.max_length / 2
 else a.max_length end 'Length'
,b.name + case 
 when b.name in ('Char', 'VarChar', 'NChar', 'NVarChar', 'Binary', 'VarBinary') then '(' + 
  case when a.max_length = -1 then 'MAX' 
  when b.name in ('NChar', 'NVarchar') then cast(a.max_length / 2 as varchar)
  else cast(a.max_length as varchar) end + ')'
 when b.name in ('Numeric', 'Decimal') then '(' + cast(a.precision as varchar) + ',' + cast(a.scale as varchar) + ')'
 else '' end as 'SqlType'
,(select value from sys.extended_properties where major_id = a.object_id AND minor_id = a.column_id AND name = 'MS_Description') 'Comment'
{0} a
inner join sys.types b on b.user_type_id = a.user_type_id
left join sys.tables d on d.object_id = a.object_id
left join sys.schemas e on e.schema_id = d.schema_id
where {1}
";
                sql = string.Format(tsql_place, @"
,a.is_nullable 'IsNullable'
,a.is_identity 'IsIdentity'
from sys.columns", loc8.ToString().Replace("a.table_name", "a.object_id"));
                if (loc88.Length > 0)
                {
                    sql += "union all" +
                    string.Format(tsql_place.Replace(
                        " select value from sys.extended_properties where major_id = a.object_id AND minor_id = a.column_id",
                        " select value from sys.extended_properties where major_id = a.object_id AND minor_id = a.parameter_id"), @"
,cast(0 as bit) 'IsNullable'
,a.is_output 'IsIdentity'
from sys.parameters", loc88.ToString().Replace("a.table_name", "a.object_id"));
                }
                sql = $"use [{db}];{sql};use [{olddatabase}]; ";
                ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                foreach (object[] row in ds)
                {
                    var table_id = string.Concat(row[0]);
                    var object_id = int.Parse(string.Concat(row[1]));
                    var column = string.Concat(row[2]);
                    var type = string.Concat(row[3]);
                    var max_length = int.Parse(string.Concat(row[4]));
                    var sqlType = string.Concat(row[5]);
                    var comment = string.Concat(row[6]);
                    var is_nullable = bool.Parse(string.Concat(row[7]));
                    var is_identity = bool.Parse(string.Concat(row[8]));
                    if (max_length == 0) max_length = -1;

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
use [{db}];
select 
 a.object_id 'Object_id'
,c.name 'Column'
,b.name 'Index_id'
,b.is_unique 'IsUnique'
,b.is_primary_key 'IsPrimaryKey'
,cast(case when b.type_desc = 'CLUSTERED' then 1 else 0 end as bit) 'IsClustered'
,case when a.is_descending_key = 1 then 1 else 0 end 'IsDesc'
from sys.index_columns a
inner join sys.indexes b on b.object_id = a.object_id and b.index_id = a.index_id
left join sys.columns c on c.object_id = a.object_id and c.column_id = a.column_id
where {loc8.ToString().Replace("a.table_name", "a.object_id")}
;
use [{olddatabase}];
";
                ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                var indexColumns = new Dictionary<int, Dictionary<string, DbIndexInfo>>();
                var uniqueColumns = new Dictionary<int, Dictionary<string, DbIndexInfo>>();
                foreach (object[] row in ds)
                {
                    int object_id = int.Parse(string.Concat(row[0]));
                    string column = string.Concat(row[1]);
                    string index_id = string.Concat(row[2]);
                    bool is_unique = bool.Parse(string.Concat(row[3]));
                    bool is_primary_key = bool.Parse(string.Concat(row[4]));
                    bool is_clustered = bool.Parse(string.Concat(row[5]));
                    bool is_desc = string.Concat(row[6]) == "1";

                    if (loc3.ContainsKey(object_id) == false || loc3[object_id].ContainsKey(column) == false) continue;
                    DbColumnInfo loc9 = loc3[object_id][column];
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
use [{db}];
select 
 b.object_id 'Object_id'
,c.name 'Column'
,e.name 'FKId'
,a.referenced_object_id
,cast(1 as bit) 'IsForeignKey'
,d.name 'Referenced_Column'
,null 'Referenced_Sln'
,null 'Referenced_Table'
from sys.foreign_key_columns a
inner join sys.tables b on b.object_id = a.parent_object_id
inner join sys.columns c on c.object_id = a.parent_object_id and c.column_id = a.parent_column_id
inner join sys.columns d on d.object_id = a.referenced_object_id and d.column_id = a.referenced_column_id
left join sys.foreign_keys e on e.object_id = a.constraint_object_id
where {loc8.ToString().Replace("a.table_name", "b.object_id")}
;
use [{olddatabase}];
";
                ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                var fkColumns = new Dictionary<int, Dictionary<string, DbForeignInfo>>();
                foreach (object[] row in ds)
                {
                    int object_id, referenced_object_id;
                    int.TryParse(string.Concat(row[0]), out object_id);
                    var column = string.Concat(row[1]);
                    string fk_id = string.Concat(row[2]);
                    int.TryParse(string.Concat(row[3]), out referenced_object_id);
                    var is_foreign_key = bool.Parse(string.Concat(row[4]));
                    var referenced_column = string.Concat(row[5]);
                    var referenced_db = string.Concat(row[6]);
                    var referenced_table = string.Concat(row[7]);
                    DbColumnInfo loc9 = loc3[object_id][column];
                    DbTableInfo loc10 = null;
                    DbColumnInfo loc11 = null;
                    bool isThisSln = referenced_object_id != 0;

                    if (isThisSln)
                    {
                        loc10 = loc2[referenced_object_id];
                        loc11 = loc3[referenced_object_id][referenced_column];
                    }
                    else
                    {

                    }
                    Dictionary<string, DbForeignInfo> loc12 = null;
                    DbForeignInfo loc13 = null;
                    if (!fkColumns.TryGetValue(object_id, out loc12))
                        fkColumns.Add(object_id, loc12 = new Dictionary<string, DbForeignInfo>());
                    if (!loc12.TryGetValue(fk_id, out loc13))
                        loc12.Add(fk_id, loc13 = new DbForeignInfo { Table = loc2[object_id], ReferencedTable = loc10 });
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