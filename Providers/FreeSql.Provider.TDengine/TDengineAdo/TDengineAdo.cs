using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;

namespace FreeSql.Provider.TDengine.TDengineAdo
{
    internal class TDengineAdo : AdoProvider
    {
        public TDengineAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings, Func<DbConnection> connectionFactory) : base(DataType.TDengine, masterConnectionString, slaveConnectionStrings)
        {

        }

        public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn)
        {
            throw new NotImplementedException();
        }

        public override DbCommand CreateCommand()
        {
            throw new NotImplementedException();
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj)
        {
            throw new NotImplementedException();
        }

        public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}
