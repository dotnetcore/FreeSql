using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Odbc.Default;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;

namespace FreeSql.Odbc.Default
{

    public class OdbcProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new OdbcSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new OdbcInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new OdbcUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new OdbcDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => throw new NotImplementedException();

        public override IDbFirst DbFirst => throw new NotImplementedException("FreeSql.Odbc.Default 未实现该功能");

        /// <summary>
        /// 生成一个普通访问功能的 IFreeSql 对象，用来访问 odbc 
        /// </summary>
        /// <param name="masterConnectionString"></param>
        /// <param name="slaveConnectionString"></param>
        /// <param name="adapter">适配器</param>
        public OdbcProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new OdbcUtils(this);
            this.InternalCommonExpression = new OdbcExpression(this.InternalCommonUtils);

            this.Ado = new OdbcAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.CodeFirst = new OdbcCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);

            var _utils = InternalCommonUtils as OdbcUtils;
            //处理 MaxLength
            this.Aop.ConfigEntityProperty += new EventHandler<Aop.ConfigEntityPropertyEventArgs>((s, e) =>
            {
                object[] attrs = null;
                try
                {
                    attrs = e.Property.GetCustomAttributes(false).ToArray(); //.net core 反射存在版本冲突问题，导致该方法异常
                }
                catch { }

                var maxlenAttr = attrs.Where(a => {
                    return ((a as Attribute)?.TypeId as Type)?.Name == "MaxLengthAttribute";
                }).FirstOrDefault();
                if (maxlenAttr != null)
                {
                    var lenProp = maxlenAttr.GetType().GetProperties().Where(a => a.PropertyType.IsNumberType()).FirstOrDefault();
                    if (lenProp != null && int.TryParse(string.Concat(lenProp.GetValue(maxlenAttr, null)), out var tryval))
                    {
                        if (tryval != 0)
                        {
                            switch (this.Ado.DataType)
                            {
                                case DataType.Sqlite:
                                    e.ModifyResult.DbType = tryval > 0 ? $"{_utils.Adapter.MappingOdbcTypeVarChar}({tryval})" : _utils.Adapter.MappingOdbcTypeText;
                                    break;
                            }
                        }
                    }
                }
            });
        }

        ~OdbcProvider() => this.Dispose();
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
                FreeSqlOdbcGlobalExtensions._dicOdbcAdater.TryRemove(Ado.Identifier, out var tryada);
            }
        }
    }
}

partial class FreeSqlOdbcGlobalExtensions
{
    internal static OdbcAdapter DefaultOdbcAdapter = new OdbcAdapter();
    internal static ConcurrentDictionary<Guid, OdbcAdapter> _dicOdbcAdater = new ConcurrentDictionary<Guid, OdbcAdapter>();
    public static void SetOdbcAdapter(this IFreeSql that, OdbcAdapter adapter) => _dicOdbcAdater.AddOrUpdate(that.Ado.Identifier, adapter, (fsql, old) => adapter);
    internal static OdbcAdapter GetOdbcAdapter(this IFreeSql that) => _dicOdbcAdater.TryGetValue(that.Ado.Identifier, out var tryada) ? tryada : DefaultOdbcAdapter;
}
