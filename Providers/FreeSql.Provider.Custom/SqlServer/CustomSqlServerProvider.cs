using FreeSql.Internal.CommonProvider;
using System;
using System.Data.Common;
using System.Threading;

namespace FreeSql.Custom.SqlServer
{

    public class CustomSqlServerProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new CustomSqlServerSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new CustomSqlServerInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new CustomSqlServerUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new CustomSqlServerDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new CustomSqlServerInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public CustomSqlServerProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new CustomSqlServerUtils(this);
            this.InternalCommonExpression = new CustomSqlServerExpression(this.InternalCommonUtils);

            this.Ado = new CustomSqlServerAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new CustomSqlServerDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new CustomSqlServerCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);

            if (this.Ado.MasterPool != null)
                try
                {
                    using (var conn = this.Ado.MasterPool.Get())
                    {
                        (this.InternalCommonUtils as CustomSqlServerUtils).ServerVersion = int.Parse(conn.Value.ServerVersion.Split('.')[0]);
                    }
                }
                catch
                {
                }
        }

        ~CustomSqlServerProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            try
            {
                (this.Ado as AdoProvider)?.Dispose();
            }
            finally
            {
                FreeSqlCustomAdapterGlobalExtensions._dicCustomAdater.TryRemove(Ado.Identifier, out var tryada);
                FreeSqlCustomAdapterGlobalExtensions._dicDbProviderFactory.TryRemove(Ado.Identifier, out var trydbpf);
            }
        }
    }
}
