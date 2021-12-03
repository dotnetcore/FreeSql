using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.ClickHouse.Curd;
using System;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;

namespace FreeSql.ClickHouse
{

    public class ClickHouseProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new ClickHouseSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new ClickHouseInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new ClickHouseUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new ClickHouseDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>()
        {
            throw new NotImplementedException();
        }
        public ClickHouseProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new ClickHouseUtils(this);
            this.InternalCommonExpression = new ClickHouseExpression(this.InternalCommonUtils);

            this.Ado = new ClickHouseAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new ClickHouseDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new ClickHouseCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        ~ClickHouseProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
