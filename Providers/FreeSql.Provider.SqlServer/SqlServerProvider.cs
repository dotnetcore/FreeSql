using FreeSql.Internal.CommonProvider;
using FreeSql.SqlServer.Curd;
using System;
using System.Data.Common;
using System.Threading;

namespace FreeSql.SqlServer
{

    public class SqlServerProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new SqlServerSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new SqlServerInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new SqlServerUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new SqlServerDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new SqlServerInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public SqlServerProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new SqlServerUtils(this);
            this.InternalCommonExpression = new SqlServerExpression(this.InternalCommonUtils);

            this.Ado = new SqlServerAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new SqlServerDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new SqlServerCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);

            if (this.Ado.MasterPool != null)
                try
                {
                    using (var conn = this.Ado.MasterPool.Get())
                    {
                        (this.InternalCommonUtils as SqlServerUtils).ServerVersion = int.Parse(conn.Value.ServerVersion.Split('.')[0]);
                    }
                }
                catch
                {
                }
        }

        ~SqlServerProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
