using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Odbc;

namespace FreeSql.GBase
{
    class GBaseDbFirst : IDbFirst
    {
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        public GBaseDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public int GetDbType(DbColumnInfo column) => (int)GetOdbcType(column);
        OdbcType GetOdbcType(DbColumnInfo column)
        {
            var dbtype = column.DbTypeText;
            OdbcType ret = OdbcType.VarChar;
            switch (dbtype.ToLower().TrimStart('_'))
            {
                case "int8":
                case "serial8":
                case "bigserial":
                case "bigint": ret = OdbcType.BigInt; break;
                case "byte":
                case "blob": ret = OdbcType.VarBinary; break;
                case "nchar": ret = OdbcType.NChar; break;
                case "char":
                case "character": ret = OdbcType.Char; break;
                case "date": ret = OdbcType.Date; break;
                case "dec":
                case "decimal": ret = OdbcType.Decimal; break;
                case "double":
                case "double precision":
                case "float": ret = OdbcType.Double; break;
                case "real":
                case "smallfloat": ret = OdbcType.Real; break;
                case "serial":
                case "integer":
                case "int": ret = OdbcType.Int; break;
                case "numeric":
                case "numeric precision": ret = OdbcType.Decimal; break;
                case "smallint": ret = OdbcType.SmallInt; break;
                case "interval": ret = OdbcType.Time; break;
                case "datetime":
                case "timestamp": ret = OdbcType.DateTime; break;
                case "varchar":
                case "char varying":
                case "character varying": ret = OdbcType.VarChar; break;
                case "nvarchar": ret = OdbcType.NVarChar; break;

                case "text": ret = OdbcType.Text; break;
                case "boolean": ret = OdbcType.Bit; break;
                case "char(36)": ret = OdbcType.UniqueIdentifier; break;
            }
            return ret;
        }

        static readonly Dictionary<int, DbToCs> _dicDbToCs = new Dictionary<int, DbToCs>() {
            { (int)OdbcType.SmallInt, new DbToCs("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(short), typeof(short?), "{0}.Value", "GetInt16") },
            { (int)OdbcType.Int, new DbToCs("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(int), typeof(int?), "{0}.Value", "GetInt32") },
            { (int)OdbcType.BigInt, new DbToCs("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },
            { (int)OdbcType.Decimal, new DbToCs("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },
            { (int)OdbcType.Real, new DbToCs("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?), "{0}.Value", "GetFloat") },
            { (int)OdbcType.Double, new DbToCs("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
            { (int)OdbcType.Decimal, new DbToCs("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },

            { (int)OdbcType.Char, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
            { (int)OdbcType.VarChar, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
            { (int)OdbcType.Text, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },

            { (int)OdbcType.DateTime, new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
            { (int)OdbcType.Date, new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
            { (int)OdbcType.Time, new DbToCs("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },

            { (int)OdbcType.Bit, new DbToCs("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },
            { (int)OdbcType.VarBinary, new DbToCs("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },

            { (int)OdbcType.UniqueIdentifier, new DbToCs("(Guid?)", "Guid.Parse({0})", "{0}.ToString()", "Guid", typeof(Guid), typeof(Guid?), "{0}", "GetString") },
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
            throw new NotImplementedException();
        }

        public bool ExistsTable(string name, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var tbname = _commonUtils.SplitTableName(name);
            if (ignoreCase) tbname = tbname.Select(a => a.ToUpper()).ToArray();
            var sql = $" select 1 from systables where tabtype='T' and {(ignoreCase ? "upper(trim(tabname))" : "trim(tabname)")} = {_commonUtils.FormatSql("{0}", tbname.Last())}";
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
a.""owner"" || a.tabname as id,
trim(a.""owner"") as owner,
trim(a.tabname) as name,
trim(b.comments) as comment,
a.tabtype as type
from systables a
left join syscomments b on b.tabname = a.tabname
where a.tabtype in ('T', 'V')" + (tbname == null ? "" : $" and {(ignoreCase ? "upper(trim(a.tabname))" : "trim(a.tabname)")} = {_commonUtils.FormatSql("{0}", tbname.Last())}");
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
                    case "V": type = DbTableType.VIEW; break;
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
                loc1.Add(loc2[table_id]);
            }
            if (loc6_1000.Count > 0) loc6.Add(loc6_1000.ToArray());
            if (loc66_1000.Count > 0) loc66.Add(loc66_1000.ToArray());

            //todo: ...
            return loc1;
        }

        public List<DbEnumInfo> GetEnumsByDatabase(params string[] database)
        {
            return new List<DbEnumInfo>();
        }
    }
}