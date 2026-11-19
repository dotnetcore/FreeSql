using System;
using System.Data.Common;
using System.Threading;
using FreeSql.Internal.CommonProvider;
using FreeSql.IoTDB.Curd;

namespace FreeSql.IoTDB
{
    public sealed class IoTDBProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public IoTDBProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            InternalCommonUtils = new IoTDBUtils(this);
            InternalCommonExpression = new IoTDBExpression(InternalCommonUtils);
            Ado = new IoTDBAdo(InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            Aop = new AopProvider();
            DbFirst = new IoTDBDbFirst(this, InternalCommonUtils);
            CodeFirst = new IoTDBCodeFirst(this, InternalCommonUtils, InternalCommonExpression);
        }

        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) =>
            new IoTDBSelect<T1>(this, InternalCommonUtils, InternalCommonExpression, dywhere);

        public override IInsert<T1> CreateInsertProvider<T1>() =>
            new IoTDBInsert<T1>(this, InternalCommonUtils, InternalCommonExpression);

        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) =>
            new IoTDBUpdate<T1>(this, InternalCommonUtils, InternalCommonExpression, dywhere);

        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) =>
            new IoTDBDelete<T1>(this, InternalCommonUtils, InternalCommonExpression, dywhere);

        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() =>
            new IoTDBInsertOrUpdate<T1>(this, InternalCommonUtils, InternalCommonExpression);

        ~IoTDBProvider() => Dispose();

        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (Ado as AdoProvider)?.Dispose();
        }
    }
}
