using FreeSql.Internal.CommonProvider;
using System;
using System.Data.Common;
using System.Threading;

namespace FreeSql.Custom.PostgreSQL
{

    public class CustomPostgreSQLProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new CustomPostgreSQLSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new CustomPostgreSQLInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new CustomPostgreSQLUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new CustomPostgreSQLDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new CustomPostgreSQLInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public CustomPostgreSQLProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new CustomPostgreSQLUtils(this);
            this.InternalCommonExpression = new CustomPostgreSQLExpression(this.InternalCommonUtils);

            this.Ado = new CustomPostgreSQLAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new CustomPostgreSQLDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new CustomPostgreSQLCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        ~CustomPostgreSQLProvider() => this.Dispose();
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
