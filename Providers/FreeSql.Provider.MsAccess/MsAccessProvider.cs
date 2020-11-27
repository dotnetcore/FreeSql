using FreeSql.Internal.CommonProvider;
using FreeSql.MsAccess.Curd;
using System;
using System.Data.Common;
using System.Threading;

namespace FreeSql.MsAccess
{

    public class MsAccessProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new MsAccessSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new MsAccessInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new MsAccessUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new MsAccessDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => throw new NotImplementedException();

        public override IDbFirst DbFirst => throw new NotImplementedException("FreeSql.Provider.Sqlite 未实现该功能");
        public MsAccessProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new MsAccessUtils(this);
            this.InternalCommonExpression = new MsAccessExpression(this.InternalCommonUtils);

            this.Ado = new MsAccessAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.CodeFirst = new MsAccessCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }
        ~MsAccessProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
