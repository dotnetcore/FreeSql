using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.SonnetDB.Curd;
using System;
using System.Data.Common;
using System.Threading;

namespace FreeSql.SonnetDB
{
    /// <summary>
    /// SonnetDB 3.1 的 FreeSql 提供程序。
    /// 标记 <see cref="Provider.SonnetDB.Attributes.SonnetDBTableAttribute"/> 的实体使用
    /// 关系表语义；未标记的旧实体继续使用时序测量语义。
    /// </summary>
    public class SonnetDBProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        internal const string UnsupportedUpsertMessage =
            "SonnetDB 3.1 尚未提供原子的 INSERT ... ON CONFLICT/UPSERT 操作，" +
            "因此 FreeSql InsertOrUpdate 不可用；如能接受竞态风险，请在显式事务中分别执行插入和更新。";

        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) =>
            new SonnetDBSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public override IInsert<T1> CreateInsertProvider<T1>() =>
            new SonnetDBInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) =>
            new SonnetDBUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) =>
            new SonnetDBDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() =>
            throw new NotSupportedException(UnsupportedUpsertMessage);

        public SonnetDBProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            SonnetDBTypeHandlers.EnsureRegistered();
            SonnetDBVectorTypeHandlers.EnsureRegistered();
            SonnetDBJsonTypeHandlers.EnsureRegistered();
            RegisterReaderOverrides();
            this.InternalCommonUtils = new SonnetDBUtils(this);
            this.InternalCommonExpression = new SonnetDBExpression(this.InternalCommonUtils);
            this.Ado = new SonnetDBAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();
            this.DbFirst = new SonnetDBDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new SonnetDBCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        static void RegisterReaderOverrides()
        {
            lock (Select0Provider._dicMethodDataReaderGetValueOverride)
            {
                if (Select0Provider._dicMethodDataReaderGetValueOverride.TryGetValue(
                    DataType.SonnetDB, out var overrides) == false)
                {
                    overrides = new System.Collections.Generic.Dictionary<Type, System.Reflection.MethodInfo>();
                    Select0Provider._dicMethodDataReaderGetValueOverride.Add(DataType.SonnetDB, overrides);
                }

                if (overrides.ContainsKey(typeof(DateTimeOffset)) == false)
                    overrides.Add(
                        typeof(DateTimeOffset),
                        typeof(SonnetDBAdo).GetMethod(nameof(SonnetDBAdo.ReadDateTimeOffset),
                            new[] { typeof(DbDataReader), typeof(int) }));
                if (overrides.ContainsKey(typeof(DateTime)) == false)
                    overrides.Add(
                        typeof(DateTime),
                        typeof(SonnetDBAdo).GetMethod(nameof(SonnetDBAdo.ReadDateTime),
                            new[] { typeof(DbDataReader), typeof(int) }));
            }
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
