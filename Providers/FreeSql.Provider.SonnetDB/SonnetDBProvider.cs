using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.SonnetDB.Curd;
using System;
using System.Data.Common;
using System.Threading;

namespace FreeSql.SonnetDB
{
    /// <summary>
    /// FreeSql provider for SonnetDB. SonnetDB's current SQL surface supports INSERT, SELECT and DELETE; UPDATE and UPSERT are not supported.
    /// </summary>
    public class SonnetDBProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        const string UnsupportedUpdateMessage = "FreeSql.Provider.SonnetDB supports INSERT, SELECT and DELETE. SonnetDB SQL does not support UPDATE or UPSERT.";

        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) =>
            new SonnetDBSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public override IInsert<T1> CreateInsertProvider<T1>() =>
            new SonnetDBInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) =>
            throw new NotSupportedException(UnsupportedUpdateMessage);

        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) =>
            new SonnetDBDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() =>
            throw new NotSupportedException(UnsupportedUpdateMessage);

        public SonnetDBProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new SonnetDBUtils(this);
            this.InternalCommonExpression = new SonnetDBExpression(this.InternalCommonUtils);
            this.Ado = new SonnetDBAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();
            this.DbFirst = new SonnetDBDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new SonnetDBCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        ~SonnetDBProvider() => this.Dispose();
        int _disposeCounter;

        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
