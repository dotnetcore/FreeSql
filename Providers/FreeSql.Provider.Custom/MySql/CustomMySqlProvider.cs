using FreeSql.Internal.CommonProvider;
using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Threading;

namespace FreeSql.Custom.MySql
{

    public class CustomMySqlProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new CustomMySqlSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new CustomMySqlInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new CustomMySqlUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new CustomMySqlDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new CustomMySqlInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public CustomMySqlProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new CustomMySqlUtils(this);
            this.InternalCommonExpression = new CustomMySqlExpression(this.InternalCommonUtils);

            this.Ado = new CustomMySqlAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new CustomMySqlDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new CustomMySqlCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        ~CustomMySqlProvider() => this.Dispose();
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
