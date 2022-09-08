using FreeSql.Internal.CommonProvider;
using System;
using System.Data.Common;
using System.Threading;

namespace FreeSql.Custom.Oracle
{

    public class CustomOracleProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new CustomOracleSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new CustomOracleInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new CustomOracleUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new CustomOracleDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new CustomOracleInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public CustomOracleProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new CustomOracleUtils(this);
            this.InternalCommonExpression = new CustomOracleExpression(this.InternalCommonUtils);

            this.Ado = new CustomOracleAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new CustomOracleDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new CustomOracleCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);

            //this.Aop.AuditValue += new EventHandler<Aop.AuditValueEventArgs>((_, e) =>
            //{
            //    if (e.Value == null && e.Column.Attribute.IsPrimary == false && e.Column.Attribute.IsIdentity == false)
            //        e.Value = Utils.GetDataReaderValue(e.Property.PropertyType.NullableTypeOrThis(), e.Column.Attribute.DbDefautValue);
            //});
        }

        ~CustomOracleProvider() => this.Dispose();
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
