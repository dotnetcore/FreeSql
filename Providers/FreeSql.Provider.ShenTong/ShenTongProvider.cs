using FreeSql.Internal.CommonProvider;
using FreeSql.ShenTong.Curd;
using System;
using System.Data.Common;
using System.Threading;

namespace FreeSql.ShenTong
{

    public class ShenTongProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new ShenTongSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new ShenTongInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new ShenTongUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new ShenTongDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new ShenTongInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public ShenTongProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new ShenTongUtils(this);
            this.InternalCommonExpression = new ShenTongExpression(this.InternalCommonUtils);

            this.Ado = new ShenTongAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new ShenTongDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new ShenTongCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        ~ShenTongProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
