using FreeSql.Duckdb.Curd;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Numerics;
using System.Threading;

namespace FreeSql.Duckdb
{

    public class DuckdbProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        static int _firstInit = 1;
        static void InitInternal()
        {
            if (Interlocked.Exchange(ref _firstInit, 0) == 1) //不能放在 static ctor .NetFramework 可能报初始化类型错误
            {
#if net60
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(DateOnly)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(TimeOnly)] = true;
#endif
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(BigInteger)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(BitArray)] = true;
                Select0Provider._dicMethodDataReaderGetValue[typeof(Guid)] = typeof(DbDataReader).GetMethod("GetGuid", new Type[] { typeof(int) });
            }
        }

        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new DuckdbSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new DuckdbInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new DuckdbUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new DuckdbDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new DuckdbInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public DuckdbProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            InitInternal();
            this.InternalCommonUtils = new DuckdbUtils(this);
            this.InternalCommonExpression = new DuckdbExpression(this.InternalCommonUtils);

            this.Ado = new DuckdbAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.CodeFirst = new DuckdbCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.DbFirst = new DuckdbDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            if (connectionFactory != null) this.CodeFirst.IsNoneCommandParameter = true;

            this.Aop.ConfigEntityProperty += (s, e) =>
            {
                //duckdb map 类型
                if (e.Property.PropertyType.IsGenericType && e.Property.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    Utils.dicExecuteArrayRowReadClassOrTuple[e.Property.PropertyType] = true;
                    e.ModifyResult.DbType = CodeFirst.GetDbInfo(e.Property.PropertyType)?.dbtype;
                }
            };
        }

        ~DuckdbProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
