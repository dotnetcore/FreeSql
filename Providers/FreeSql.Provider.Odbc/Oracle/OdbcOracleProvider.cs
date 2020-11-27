using FreeSql.Internal.CommonProvider;
using System;
using System.Data.Common;
using System.Threading;

namespace FreeSql.Odbc.Oracle
{

    public class OdbcOracleProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new OdbcOracleSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new OdbcOracleInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new OdbcOracleUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new OdbcOracleDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new OdbcOracleInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public OdbcOracleProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new OdbcOracleUtils(this);
            this.InternalCommonExpression = new OdbcOracleExpression(this.InternalCommonUtils);

            this.Ado = new OdbcOracleAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new OdbcOracleDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new OdbcOracleCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);

            //this.Aop.AuditValue += new EventHandler<Aop.AuditValueEventArgs>((_, e) =>
            //{
            //    if (e.Value == null && e.Column.Attribute.IsPrimary == false && e.Column.Attribute.IsIdentity == false)
            //        e.Value = Utils.GetDataReaderValue(e.Property.PropertyType.NullableTypeOrThis(), e.Column.Attribute.DbDefautValue);
            //});
        }

        ~OdbcOracleProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
