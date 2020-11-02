using FreeSql.Firebird.Curd;
using FreeSql.Internal.CommonProvider;
using System;
using System.Data.Common;
using System.Threading;

namespace FreeSql.Firebird
{

    public class FirebirdProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new FirebirdSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new FirebirdInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new FirebirdUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new FirebirdDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new FirebirdInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public FirebirdProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new FirebirdUtils(this);
            this.InternalCommonExpression = new FirebirdExpression(this.InternalCommonUtils);

            this.Ado = new FirebirdAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new FirebirdDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new FirebirdCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);

            if ((this.Ado as FirebirdAdo).IsFirebird2_5)
                this.Aop.ConfigEntityProperty += (_, e) =>
                {
                    if (e.Property.PropertyType.NullableTypeOrThis() == typeof(bool))
                        e.ModifyResult.MapType = typeof(short);
                };
        }

        ~FirebirdProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
