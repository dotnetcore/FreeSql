using FreeSql.Internal.CommonProvider;
using System;
using System.Data.Common;
using System.Threading;

namespace FreeSql.KingbaseES
{

    public class KingbaseESProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new KingbaseESSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new KingbaseESInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new KingbaseESUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new KingbaseESDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new KingbaseESInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public KingbaseESProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new KingbaseESUtils(this);
            this.InternalCommonExpression = new KingbaseESExpression(this.InternalCommonUtils);

            this.Ado = new KingbaseESAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new KingbaseESDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new KingbaseESCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        ~KingbaseESProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
