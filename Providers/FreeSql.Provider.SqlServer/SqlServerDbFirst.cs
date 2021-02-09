using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

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

        static readonly Dictionary<int, DbToCs> _dicDbToCs = new Dictionary<int, DbToCs>() {
                { (int)SqlDbType.Bit, new DbToCs("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },

                { (int)SqlDbType.TinyInt, new DbToCs("(byte?)", "byte.Parse({0})", "{0}.ToString()", "byte?", typeof(byte), typeof(byte?), "{0}.Value", "GetByte") },
                { (int)SqlDbType.SmallInt, new DbToCs("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(short), typeof(short?), "{0}.Value", "GetInt16") },
                { (int)SqlDbType.Int, new DbToCs("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(int), typeof(int?), "{0}.Value", "GetInt32") },
                { (int)SqlDbType.BigInt, new DbToCs("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },

                { (int)SqlDbType.SmallMoney, new DbToCs("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },
                { (int)SqlDbType.Money, new DbToCs("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },
                { (int)SqlDbType.Decimal, new DbToCs("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },
                { (int)SqlDbType.Float, new DbToCs("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
                { (int)SqlDbType.Real, new DbToCs("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?), "{0}.Value", "GetFloat") },

                { (int)SqlDbType.Time, new DbToCs("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },
                { (int)SqlDbType.Date, new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)SqlDbType.DateTime, new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)SqlDbType.DateTime2, new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)SqlDbType.SmallDateTime, new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)SqlDbType.DateTimeOffset, new DbToCs("(DateTimeOffset?)", "new DateTimeOffset(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTimeOffset), typeof(DateTimeOffset?), "{0}.Value", "GetDateTimeOffset") },

                { (int)SqlDbType.Binary, new DbToCs("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },
                { (int)SqlDbType.VarBinary, new DbToCs("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },
                { (int)SqlDbType.Image, new DbToCs("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },
                { (int)SqlDbType.Timestamp, new DbToCs("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },

                { (int)SqlDbType.Char, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)SqlDbType.VarChar, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)SqlDbType.Text, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)SqlDbType.NChar, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)SqlDbType.NVarChar, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)SqlDbType.NText, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },

                { (int)SqlDbType.UniqueIdentifier, new DbToCs("(Guid?)", "Guid.Parse({0})", "{0}.ToString()", "Guid?", typeof(Guid), typeof(Guid?), "{0}.Value", "GetGuid") },
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

        public bool ExistsTable(string name, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var olddatabase = "";
            using (var conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5)))
            {
                olddatabase = conn.Value.Database;
            }
            var tbname = _commonUtils.SplitTableName(name);
            if (tbname?.Length == 1) tbname = new[] { olddatabase, "dbo", tbname[0] };
            if (tbname?.Length == 2) tbname = new[] { olddatabase, tbname[0], tbname[1] };
            tbname = tbname.Select(a => a.ToLower()).ToArray();
            var sql = $@"
use [{tbname[0]}];
select 
1
from sys.tables a
inner join sys.schemas b on b.schema_id = a.schema_id
where lower(b.name)={_commonUtils.FormatSql("{0}", tbname[1])} and lower(a.name)={_commonUtils.FormatSql("{0}", tbname[2])}
;
use [{olddatabase}];
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
                if (tbname?.Length == 1) tbname = new[] { olddatabase, "dbo", tbname[0] };
                if (tbname?.Length == 2) tbname = new[] { olddatabase, tbname[0], tbname[1] };
                tbname = tbname.Select(a => a.ToLower()).ToArray();
                dbs = new[] { tbname[0] };
            }
            var tables = new List<DbTableInfo>();

            foreach (var db in dbs)
            {
                if (string.IsNullOrEmpty(db)) continue;

                var loc1 = new List<DbTableInfo>();
                var loc2 = new Dictionary<int, DbTableInfo>();
                var loc3 = new Dictionary<int, Dictionary<string, DbColumnInfo>>();

                var sql = $@"
use [{db}];
select * from (
select 
 a.object_id
,b.name 'owner'
,a.name 'name'
,(select value from sys.extended_properties where major_id = a.object_id AND minor_id = 0 AND name = 'MS_Description') 'comment'
,'TABLE' type
from sys.tables a
inner join sys.schemas b on b.schema_id = a.schema_id
where not(b.name = 'dbo' and a.name = 'sysdiagrams')
union all
select
 a.object_id
,b.name 'owner'
,a.name 'name'
,(select value from sys.extended_properties where major_id = a.object_id AND minor_id = 0 AND name = 'MS_Description') 'comment'
,'VIEW' type
from sys.views a
inner join sys.schemas b on b.schema_id = a.schema_id
union all
select 
 a.object_id
,b.name 'owner'
,a.name 'name'
,(select value from sys.extended_properties where major_id = a.object_id AND minor_id = 0 AND name = 'MS_Description') 'comment'
,'StoreProcedure' type
from sys.procedures a
inner join sys.schemas b on b.schema_id = a.schema_id
where a.type = 'P' and charindex('diagram', a.name) = 0
) ft_dbf{(tbname == null ? "" : _commonUtils.FormatSql(" where lower([owner])={0} and lower([name])={1}", new[] { tbname[1], tbname[2] }))}
order by type desc, [owner], [name];
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
,a.object_id
,a.name 'column'
,b.name 'type'
,case
 when b.name in ('text', 'ntext', 'image') then -1
 when b.name in ('nchar', 'nvarchar') then a.max_length / 2
 else a.max_length end 'length'
,b.name + case 
 when b.name in ('char', 'varchar', 'nchar', 'nvarchar', 'binary', 'varbinary') then '(' + 
  case when a.max_length = -1 then 'MAX' 
  when b.name in ('nchar', 'nvarchar') then cast(a.max_length / 2 as varchar)
  else cast(a.max_length as varchar) end + ')'
 when b.name in ('numeric', 'decimal') then '(' + cast(a.precision as varchar) + ',' + cast(a.scale as varchar) + ')'
 else '' end as 'sqltype'
,( select value from sys.extended_properties where major_id = a.object_id AND minor_id = a.column_id AND name = 'MS_Description') 'comment'
{0} a
inner join sys.types b on b.user_type_id = a.user_type_id
left join sys.tables d on d.object_id = a.object_id
left join sys.schemas e on e.schema_id = d.schema_id{2}
where {1}
";
                sql = string.Format(tsql_place, @"
,a.is_nullable 'isnullable'
,a.is_identity 'isidentity'
,f.text as 'defaultvalue'
from sys.columns", loc8.ToString().Replace("a.table_name", "a.object_id"), @"
left join syscomments f on f.id = a.default_object_id
");
                if (loc88.Length > 0)
                {
                    sql += "union all" +
                    string.Format(tsql_place.Replace(
                        " select value from sys.extended_properties where major_id = a.object_id AND minor_id = a.column_id",
                        " select value from sys.extended_properties where major_id = a.object_id AND minor_id = a.parameter_id"), @"
,cast(0 as bit) 'isnullable'
,a.is_output 'isidentity'
,'' as 'defaultvalue'
from sys.parameters", loc88.ToString().Replace("a.table_name", "a.object_id"), "");
                }
                sql = $"use [{db}];{sql};use [{olddatabase}]; ";
                ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                var position = 0;
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
                    var defaultValue = string.Concat(row[9]);
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
                        Coment = comment,
                        DefaultValue = defaultValue,
                        Position = ++position
                    });
                    loc3[object_id][column].DbType = this.GetDbType(loc3[object_id][column]);
                    loc3[object_id][column].CsType = this.GetCsTypeInfo(loc3[object_id][column]);
                }

                sql = $@"
use [{db}];
select 
 a.object_id 'object_id'
,c.name 'column'
,b.name 'index_id'
,b.is_unique 'isunique'
,b.is_primary_key 'isprimarykey'
,cast(case when b.type_desc = 'CLUSTERED' then 1 else 0 end as bit) 'isclustered'
,case when a.is_descending_key = 1 then 1 else 0 end 'isdesc'
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

                if (tbname == null)
                {
                    sql = $@"
use [{db}];
select 
 b.object_id 'object_id'
,c.name 'column'
,e.name 'fkid'
,a.referenced_object_id
,cast(1 as bit) 'isforeignkey'
,d.name 'referenced_column'
,null 'referenced_sln'
,null 'referenced_table'
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