using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;

namespace FreeSql.TDengine
{
    public class TDengineDbFirst : IDbFirst
    {
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;

        public TDengineDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public List<string> GetDatabases()
        {
            var sql = @"SHOW DATABASES;";
            var ds = _orm.Ado.Query<string>(sql);
            return ds;
        }

        public List<DbTableInfo> GetTablesByDatabase(params string[] database)
        {
            throw new NotImplementedException($"FreeSql.Provider.TDengine {CoreErrorStrings.S_Not_Implemented_Feature}");
        }

        public DbTableInfo GetTableByName(string name, bool ignoreCase = true)
        {
            throw new NotImplementedException($"FreeSql.Provider.TDengine {CoreErrorStrings.S_Not_Implemented_Feature}");
        }

        public bool ExistsTable(string name, bool ignoreCase = true)
        {
            var sb = new StringBuilder();
            sb.Append("select count(1) from information_schema.ins_tables where ");
            var where = ignoreCase ? $"LOWER(table_name) = LOWER('{name}')" : $"table_name = '{name}'";
            sb.Append(where);
            var sql = sb.ToString();
            var executeScalar = _orm.Ado.ExecuteScalar(sql);
            var result = Convert.ToInt32(executeScalar);
            return result > 0;
        }

        public int GetDbType(DbColumnInfo column) => (int)GetTDengineDbType(column);

        DbType GetTDengineDbType(DbColumnInfo column)
        {
            switch (column.DbTypeText)
            {
                case "DOUBLE": return DbType.Double;
                case "FLOAT": return DbType.Single;
                case "TIMESTAMP": return DbType.DateTime;
                case "BOOL": return DbType.Boolean;
                case "NCHAR": return DbType.String;
                case "TINYINT UNSIGNED": return DbType.Byte;
                case "SMALLINT UNSIGNED": return DbType.UInt16;
                case "INT UNSIGNED": return DbType.UInt32;
                case "BIGINT UNSIGNED": return DbType.UInt64;
                case "SMALLINT": return DbType.Int16;
                case "INT": return DbType.Int32;
                case "BIGINT": return DbType.Int64;
                default: return DbType.String;
            }
        }

        static readonly Dictionary<int, DbToCs> _dicDbToCs = new Dictionary<int, DbToCs>()
        {
        };

        public string GetCsConvert(DbColumnInfo column)
        {
            throw new NotImplementedException($"FreeSql.Provider.TDengine {CoreErrorStrings.S_Not_Implemented_Feature}");
            //return _dicDbToCs.TryGetValue(column.DbType, out var trydc)
            //     ? (column.IsNullable ? trydc.csConvert : trydc.csConvert.Replace("?", ""))
            //     : null;
        }

        public string GetCsParse(DbColumnInfo column)
        {
            throw new NotImplementedException($"FreeSql.Provider.TDengine {CoreErrorStrings.S_Not_Implemented_Feature}");
            //return _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csParse : null;
        }

        public string GetCsStringify(DbColumnInfo column)
        {
            throw new NotImplementedException($"FreeSql.Provider.TDengine {CoreErrorStrings.S_Not_Implemented_Feature}");
            //return _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csStringify : null;
        }


        public string GetCsType(DbColumnInfo column)
        {
            throw new NotImplementedException($"FreeSql.Provider.TDengine {CoreErrorStrings.S_Not_Implemented_Feature}");
            //return _dicDbToCs.TryGetValue(column.DbType, out var trydc)
            //    ? (column.IsNullable ? trydc.csType : trydc.csType.Replace("?", ""))
            //    : null;
        }

        public Type GetCsTypeInfo(DbColumnInfo column)
        {
            throw new NotImplementedException($"FreeSql.Provider.TDengine {CoreErrorStrings.S_Not_Implemented_Feature}");
            //return _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csTypeInfo : null;
        }

        public string GetCsTypeValue(DbColumnInfo column)
        {
            throw new NotImplementedException($"FreeSql.Provider.TDengine {CoreErrorStrings.S_Not_Implemented_Feature}");
            //return _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csTypeValue : null;
        }

        public string GetDataReaderMethod(DbColumnInfo column)
        {
            throw new NotImplementedException($"FreeSql.Provider.TDengine {CoreErrorStrings.S_Not_Implemented_Feature}");
            //return _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.dataReaderMethod : null;
        }

        public List<DbEnumInfo> GetEnumsByDatabase(params string[] database)
        {

            throw new NotImplementedException($"FreeSql.Provider.TDengine {CoreErrorStrings.S_Not_Implemented_Feature}");
        }
    }
}