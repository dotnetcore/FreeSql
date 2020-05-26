using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Odbc.KingbaseES
{
    class OdbcKingbaseESDbFirst : IDbFirst
    {
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        public OdbcKingbaseESDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public int GetDbType(DbColumnInfo column) => (int)GetSqlDbType(column);
        OdbcType GetSqlDbType(DbColumnInfo column)
        {
            var dbtype = column.DbTypeText;
            var isarray = dbtype.EndsWith("[]");
            if (isarray) dbtype = dbtype.Remove(dbtype.Length - 2);
            var ret = OdbcType.VarChar;
            switch (dbtype.ToLower().TrimStart('_'))
            {
                case "tinyint": ret = OdbcType.TinyInt; break;
                case "int2": ret = OdbcType.SmallInt; break;
                case "int4": ret = OdbcType.Int; break;
                case "int8": ret = OdbcType.BigInt; break;
                case "numeric": ret = OdbcType.Numeric; break;
                case "float4": ret = OdbcType.Real; break;
                case "float8": ret = OdbcType.Double; break;
                case "money": ret = OdbcType.Numeric; break;

                case "char": ret = column.MaxLength == 36 ? OdbcType.UniqueIdentifier : OdbcType.Char; break;
                case "bpchar": ret = OdbcType.Char; break;
                case "varchar": ret = OdbcType.VarChar; break;
                case "text": ret = OdbcType.Text; break;

                case "timestamp": ret = OdbcType.Timestamp; break;
                case "timestamptz": ret = OdbcType.Timestamp; break;
                case "date": ret = OdbcType.Date; break;
                case "time": ret = OdbcType.Time; break;
                case "timetz": ret = OdbcType.Time; break;
                case "interval": ret = OdbcType.Time; break;

                case "bool": ret = OdbcType.Bit; break;
                case "blob": ret = OdbcType.VarBinary; break;
                case "bytea": ret = OdbcType.VarBinary; break;
                case "bit": ret = OdbcType.Bit; break;
                case "varbit": ret = OdbcType.VarBinary; break;

                case "uuid": ret = OdbcType.UniqueIdentifier; break;
            }
            return ret;
        }

        static ConcurrentDictionary<int, DbToCs> _dicDbToCs = new ConcurrentDictionary<int, DbToCs>();
        static OdbcKingbaseESDbFirst()
        {
            var defaultDbToCs = new Dictionary<int, DbToCs>() {
                { (int)OdbcType.TinyInt, new DbToCs("(sbyte?)", "sbyte.Parse({0})", "{0}.ToString()", "sbyte?", typeof(short), typeof(short?), "{0}.Value", "GetInt16") },
                { (int)OdbcType.SmallInt, new DbToCs("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(int), typeof(int?), "{0}.Value", "GetInt16") },
                { (int)OdbcType.Int, new DbToCs("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(long), typeof(long?), "{0}.Value", "GetInt32") },
                { (int)OdbcType.BigInt, new DbToCs("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },
                { (int)OdbcType.Real, new DbToCs("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?), "{0}.Value", "GetFloat") },
                { (int)OdbcType.Double, new DbToCs("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
                { (int)OdbcType.Numeric, new DbToCs("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },

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
            foreach (var kv in defaultDbToCs)
                _dicDbToCs.TryAdd(kv.Key, kv.Value);
        }

        public string GetCsConvert(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? (column.IsNullable ? trydc.csConvert : trydc.csConvert.Replace("?", "")) : null;
        public string GetCsParse(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csParse : null;
        public string GetCsStringify(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csStringify : null;
        public string GetCsType(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? (column.IsNullable ? trydc.csType : trydc.csType.Replace("?", "")) : null;
        public Type GetCsTypeInfo(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csTypeInfo : null;
        public string GetCsTypeValue(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csTypeValue : null;
        public string GetDataReaderMethod(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.dataReaderMethod : null;

        public List<string> GetDatabases()
        {
            var sql = @" select schema_name from information_schema.schemata where schema_owner<>'SYSTEM'";
            var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            return new[] { "PUBLIC" }.Concat(ds.Select(a => a.FirstOrDefault()?.ToString())).ToList();
        }

        public List<DbTableInfo> GetTablesByDatabase(params string[] database2) => throw new NotImplementedException();

        public List<DbEnumInfo> GetEnumsByDatabase(params string[] database)
        {
            return new List<DbEnumInfo>();
        }
    }
}