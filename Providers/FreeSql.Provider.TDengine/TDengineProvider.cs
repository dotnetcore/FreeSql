using FreeSql.Internal.CommonProvider;
using System;
using System.Data.Common;
using FreeSql.TDengine.Curd;

namespace FreeSql.TDengine
{
    internal class TDengineProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public TDengineProvider(string masterConnectionString, string[] slaveConnectionString,
            Func<DbConnection> connectionFactory = null)
        {

            this.InternalCommonUtils = new TDengineUtils(this);
            this.InternalCommonExpression = new TDengineExpression(this.InternalCommonUtils);
            this.Ado = new TDengineAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString,
                connectionFactory);
            this.Aop = new AopProvider();
            this.DbFirst = new TDengineDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new TDengineCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new TDengineSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

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