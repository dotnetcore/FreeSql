using FreeSql.DatabaseModel;
using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace FreeSql.SqlServer
{
    class SqlServerDbFirst : IDbFirst
    {
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        public SqlServerDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public int GetDbType(DbColumnInfo column) => (int)GetSqlDbType(column);
        SqlDbType GetSqlDbType(DbColumnInfo column)
        {
            switch (column.DbTypeText.ToLower())
            {
                case "bit": return SqlDbType.Bit;
                case "tinyint": return SqlDbType.TinyInt;
                case "smallint": return SqlDbType.SmallInt;
                case "int": return SqlDbType.Int;
                case "bigint": return SqlDbType.BigInt;
                case "numeric":
                case "decimal": return SqlDbType.Decimal;
                case "smallmoney": return SqlDbType.SmallMoney;
                case "money": return SqlDbType.Money;
                case "float": return SqlDbType.Float;
                case "real": return SqlDbType.Real;
                case "date": return SqlDbType.Date;
                case "datetime":
                case "datetime2": return SqlDbType.DateTime;
                case "datetimeoffset": return SqlDbType.DateTimeOffset;
                case "smalldatetime": return SqlDbType.SmallDateTime;
                case "time": return SqlDbType.Time;
                case "char": return SqlDbType.Char;
                case "varchar": return SqlDbType.VarChar;
                case "text": return SqlDbType.Text;
                case "nchar": return SqlDbType.NChar;
                case "nvarchar": return SqlDbType.NVarChar;
                case "ntext": return SqlDbType.NText;
                case "binary": return SqlDbType.Binary;
                case "varbinary": return SqlDbType.VarBinary;
                case "image": return SqlDbType.Image;
                case "timestamp": return SqlDbType.Timestamp;
                case "uniqueidentifier": return SqlDbType.UniqueIdentifier;
                default: return SqlDbType.Variant;
            }
        }

        static readonly Dictionary<int, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)> _dicDbToCs = new Dictionary<int, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)>() {
                { (int)SqlDbType.Bit, ("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },

                { (int)SqlDbType.TinyInt, ("(byte?)", "byte.Parse({0})", "{0}.ToString()", "byte?", typeof(byte), typeof(byte?), "{0}.Value", "GetByte") },
                { (int)SqlDbType.SmallInt, ("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(short), typeof(short?), "{0}.Value", "GetInt16") },
                { (int)SqlDbType.Int, ("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(int), typeof(int?), "{0}.Value", "GetInt32") },
                { (int)SqlDbType.BigInt, ("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },

                { (int)SqlDbType.SmallMoney, ("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },
                { (int)SqlDbType.Money, ("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },
                { (int)SqlDbType.Decimal, ("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },
                { (int)SqlDbType.Float, ("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
                { (int)SqlDbType.Real, ("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?), "{0}.Value", "GetFloat") },

                { (int)SqlDbType.Time, ("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },
                { (int)SqlDbType.Date, ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)SqlDbType.DateTime, ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)SqlDbType.DateTime2, ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)SqlDbType.SmallDateTime, ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)SqlDbType.DateTimeOffset, ("(DateTimeOffset?)", "new DateTimeOffset(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTimeOffset), typeof(DateTimeOffset?), "{0}.Value", "GetDateTimeOffset") },

                { (int)SqlDbType.Binary, ("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },
                { (int)SqlDbType.VarBinary, ("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },
                { (int)SqlDbType.Image, ("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },
                { (int)SqlDbType.Timestamp, ("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },

                { (int)SqlDbType.Char, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)SqlDbType.VarChar, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)SqlDbType.Text, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)SqlDbType.NChar, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)SqlDbType.NVarChar, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)SqlDbType.NText, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },

                { (int)SqlDbType.UniqueIdentifier, ("(Guid?)", "Guid.Parse({0})", "{0}.ToString()", "Guid?", typeof(Guid), typeof(Guid?), "{0}.Value", "GetGuid") },
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
,c.value
,'TABLE' type
from sys.tables a
inner join sys.schemas b on b.schema_id = a.schema_id
left join sys.extended_properties AS c ON c.major_id = a.object_id AND c.minor_id = 0 AND c.name = 'MS_Description'
where not(b.name = 'dbo' and a.name = 'sysdiagrams')
union all
select
 a.Object_id
,b.name 'Owner'
,a.name 'Name'
,c.value
,'VIEW' type
from sys.views a
inner join sys.schemas b on b.schema_id = a.schema_id
left join sys.extended_properties AS c ON c.major_id = a.object_id AND c.minor_id = 0 AND c.name = 'MS_Description'
union all
select 
 a.Object_id
,b.name 'Owner'
,a.name 'Name'
,c.value
,'StoreProcedure' type
from sys.procedures a
inner join sys.schemas b on b.schema_id = a.schema_id
left join sys.extended_properties AS c ON c.major_id = a.object_id AND c.minor_id = 0 AND c.name = 'MS_Description'
where a.type = 'P' and charindex('diagram', a.name) = 0
order by type desc, b.name, a.name
;
use [{olddatabase}];
";
                var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                var loc6 = new List<int>();
                var loc66 = new List<int>();
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
                            loc6.Add(object_id);
                            break;
                        case DbTableType.StoreProcedure:
                            loc66.Add(object_id);
                            break;
                    }
                }
                if (loc6.Count == 0) return loc1;
                var loc8 = string.Join(",", loc6.Select(a => string.Concat(a)));
                var loc88 = string.Join(",", loc66.Select(a => string.Concat(a)));

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
,c.value
{0} a
inner join sys.types b on b.user_type_id = a.user_type_id
left join sys.extended_properties AS c ON c.major_id = a.object_id AND c.minor_id = a.column_id
left join sys.tables d on d.object_id = a.object_id
left join sys.schemas e on e.schema_id = d.schema_id
where a.object_id in ({1})
";
                sql = string.Format(tsql_place, @"
,a.is_nullable 'IsNullable'
,a.is_identity 'IsIdentity'
from sys.columns", loc8);
                if (loc88.Length > 0)
                {
                    sql += "union all" +
                    string.Format(tsql_place.Replace(
                        "left join sys.extended_properties AS c ON c.major_id = a.object_id AND c.minor_id = a.column_id",
                        "left join sys.extended_properties AS c ON c.major_id = a.object_id AND c.minor_id = a.parameter_id"), @"
,cast(0 as bit) 'IsNullable'
,a.is_output 'IsIdentity'
from sys.parameters", loc88);
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
,d.name 'Index_id'
,b.is_unique 'IsUnique'
,b.is_primary_key 'IsPrimaryKey'
,cast(case when b.type_desc = 'CLUSTERED' then 1 else 0 end as bit) 'IsClustered'
,case when a.is_descending_key = 1 then 2 when a.is_descending_key = 0 then 1 else 0 end 'IsDesc'
from sys.index_columns a
inner join sys.indexes b on b.object_id = a.object_id and b.index_id = a.index_id
left join sys.columns c on c.object_id = a.object_id and c.column_id = a.column_id
left join sys.key_constraints d on d.parent_object_id = b.object_id and d.unique_index_id = b.index_id
where a.object_id in ({loc8})
;
use [{olddatabase}];
";
                ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                var indexColumns = new Dictionary<int, Dictionary<string, List<DbColumnInfo>>>();
                var uniqueColumns = new Dictionary<int, Dictionary<string, List<DbColumnInfo>>>();
                foreach (object[] row in ds)
                {
                    int object_id = int.Parse(string.Concat(row[0]));
                    string column = string.Concat(row[1]);
                    string index_id = string.Concat(row[2]);
                    bool is_unique = bool.Parse(string.Concat(row[3]));
                    bool is_primary_key = bool.Parse(string.Concat(row[4]));
                    bool is_clustered = bool.Parse(string.Concat(row[5]));
                    int is_desc = int.Parse(string.Concat(row[6]));

                    if (loc3.ContainsKey(object_id) == false || loc3[object_id].ContainsKey(column) == false) continue;
                    DbColumnInfo loc9 = loc3[object_id][column];
                    if (loc9.IsPrimary == false && is_primary_key) loc9.IsPrimary = is_primary_key;

                    Dictionary<string, List<DbColumnInfo>> loc10 = null;
                    List<DbColumnInfo> loc11 = null;
                    if (!indexColumns.TryGetValue(object_id, out loc10))
                        indexColumns.Add(object_id, loc10 = new Dictionary<string, List<DbColumnInfo>>());
                    if (!loc10.TryGetValue(index_id, out loc11))
                        loc10.Add(index_id, loc11 = new List<DbColumnInfo>());
                    loc11.Add(loc9);
                    if (is_unique && !is_primary_key)
                    {
                        if (!uniqueColumns.TryGetValue(object_id, out loc10))
                            uniqueColumns.Add(object_id, loc10 = new Dictionary<string, List<DbColumnInfo>>());
                        if (!loc10.TryGetValue(index_id, out loc11))
                            loc10.Add(index_id, loc11 = new List<DbColumnInfo>());
                        loc11.Add(loc9);
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
                        column.Value.Sort((c1, c2) => c1.Name.CompareTo(c2.Name));
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
where b.object_id in ({loc8})
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