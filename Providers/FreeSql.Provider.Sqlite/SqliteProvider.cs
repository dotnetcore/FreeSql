using FreeSql.Internal.CommonProvider;
using FreeSql.Sqlite.Curd;
using System;
using System.Data.Common;
using System.Threading;

namespace FreeSql.Sqlite
{

    public class SqliteProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new SqliteSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new SqliteInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new SqliteUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new SqliteDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new SqliteInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public SqliteProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new SqliteUtils(this);
            this.InternalCommonExpression = new SqliteExpression(this.InternalCommonUtils);

            this.Ado = new SqliteAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.CodeFirst = new SqliteCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.DbFirst = new SqliteDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            if (connectionFactory != null) this.CodeFirst.IsNoneCommandParameter = true;
        }

        ~SqliteProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
