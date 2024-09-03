using System;
using System.Collections.Generic;
using FreeSql.DatabaseModel;
using FreeSql.Internal;

namespace  FreeSql.TDengine
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
            throw new NotImplementedException();
        }

        public List<DbTableInfo> GetTablesByDatabase(params string[] database)
        {
            throw new NotImplementedException();
        }

        public DbTableInfo GetTableByName(string name, bool ignoreCase = true)
        {
            throw new NotImplementedException();
        }

        public bool ExistsTable(string name, bool ignoreCase = true)
        {
            throw new NotImplementedException();
        }

        public int GetDbType(DbColumnInfo column)
        {
            throw new NotImplementedException();
        }

        public string GetCsConvert(DbColumnInfo column)
        {
            throw new NotImplementedException();
        }

        public string GetCsTypeValue(DbColumnInfo column)
        {
            throw new NotImplementedException();
        }

        public string GetCsType(DbColumnInfo column)
        {
            throw new NotImplementedException();
        }

        public Type GetCsTypeInfo(DbColumnInfo column)
        {
            throw new NotImplementedException();
        }

        public string GetDataReaderMethod(DbColumnInfo column)
        {
            throw new NotImplementedException();
        }

        public string GetCsStringify(DbColumnInfo column)
        {
            throw new NotImplementedException();
        }

        public string GetCsParse(DbColumnInfo column)
        {
            throw new NotImplementedException();
        }

        public List<DbEnumInfo> GetEnumsByDatabase(params string[] database)
        {
            throw new NotImplementedException();
        }
    }
}