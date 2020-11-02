using FreeSql.Internal.CommonProvider;
using System;
using System.Data.Common;
using System.Threading;

namespace FreeSql.Odbc.SqlServer
{

    public class OdbcSqlServerProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new OdbcSqlServerSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new OdbcSqlServerInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new OdbcSqlServerUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new OdbcSqlServerDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new OdbcSqlServerInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public OdbcSqlServerProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new OdbcSqlServerUtils(this);
            this.InternalCommonExpression = new OdbcSqlServerExpression(this.InternalCommonUtils);

            this.Ado = new OdbcSqlServerAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new OdbcSqlServerDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new OdbcSqlServerCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);

            if (this.Ado.MasterPool != null)
                try
                {
                    using (var conn = this.Ado.MasterPool.Get())
                    {
                        (this.InternalCommonUtils as OdbcSqlServerUtils).ServerVersion = int.Parse(conn.Value.ServerVersion.Split('.')[0]);
                    }
                }
                catch
                {
                }
        }

        ~OdbcSqlServerProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
