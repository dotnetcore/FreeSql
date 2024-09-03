using FreeSql.Internal.CommonProvider;
using System;
using System.Data.Common;

namespace FreeSql.TDengine
{
    internal class TDengineProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public TDengineProvider(string masterConnectionString, string[] slaveConnectionString,
            Func<DbConnection> connectionFactory = null)
        {
            this.Ado = new TDengineAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString,
                connectionFactory);
            this.Aop = new AopProvider();
        }

        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere)
        {
            throw new NotImplementedException();
        }

        public override IInsert<T1> CreateInsertProvider<T1>()
        {
            throw new NotImplementedException();
        }

        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere)
        {
            throw new NotImplementedException();
        }

        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere)
        {
            throw new NotImplementedException();
        }

        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}