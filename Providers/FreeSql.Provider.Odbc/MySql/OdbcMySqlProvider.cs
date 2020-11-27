using FreeSql.Internal.CommonProvider;
using System;
using System.Data.Common;
using System.Threading;

namespace FreeSql.Odbc.MySql
{

    public class OdbcMySqlProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new OdbcMySqlSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new OdbcMySqlInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new OdbcMySqlUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new OdbcMySqlDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new OdbcMySqlInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public OdbcMySqlProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new OdbcMySqlUtils(this);
            this.InternalCommonExpression = new OdbcMySqlExpression(this.InternalCommonUtils);

            this.Ado = new OdbcMySqlAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new OdbcMySqlDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new OdbcMySqlCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        ~OdbcMySqlProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
