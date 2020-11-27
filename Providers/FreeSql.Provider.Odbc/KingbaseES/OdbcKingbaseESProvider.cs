using FreeSql.Internal.CommonProvider;
using System;
using System.Data.Common;
using System.Threading;

namespace FreeSql.Odbc.KingbaseES
{

    public class OdbcKingbaseESProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new OdbcKingbaseESSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new OdbcKingbaseESInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new OdbcKingbaseESUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new OdbcKingbaseESDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new OdbcKingbaseESInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public OdbcKingbaseESProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new OdbcKingbaseESUtils(this);
            this.InternalCommonExpression = new OdbcKingbaseESExpression(this.InternalCommonUtils);

            this.Ado = new OdbcKingbaseESAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new OdbcKingbaseESDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new OdbcKingbaseESCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        ~OdbcKingbaseESProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
