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

    public class OdbcProvider<TMark> : IFreeSql<TMark>
    {

        public ISelect<T1> Select<T1>() where T1 : class => new OdbcSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public ISelect<T1> Select<T1>(object dywhere) where T1 : class => new OdbcSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public IInsert<T1> Insert<T1>() where T1 : class => new OdbcInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public IInsert<T1> Insert<T1>(T1 source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(T1[] source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(List<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(IEnumerable<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IUpdate<T1> Update<T1>() where T1 : class => new OdbcUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public IUpdate<T1> Update<T1>(object dywhere) where T1 : class => new OdbcUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public IDelete<T1> Delete<T1>() where T1 : class => new OdbcDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public IDelete<T1> Delete<T1>(object dywhere) where T1 : class => new OdbcDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public IAdo Ado { get; }
        public IAop Aop { get; }
        public ICodeFirst CodeFirst { get; }
        public IDbFirst DbFirst => throw new NotImplementedException("FreeSql.Odbc.Default 未实现该功能");

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

        internal CommonUtils InternalCommonUtils { get; }
        internal CommonExpression InternalCommonExpression { get; }

        public void Transaction(Action handler) => Ado.Transaction(handler);
        public void Transaction(TimeSpan timeout, Action handler) => Ado.Transaction(timeout, handler);
        public void Transaction(IsolationLevel isolationLevel, TimeSpan timeout, Action handler) => Ado.Transaction(isolationLevel, timeout, handler);

        public GlobalFilter GlobalFilter { get; } = new GlobalFilter();

        ~OdbcProvider() => this.Dispose();
        int _disposeCounter;
        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            try
            {
                (this.Ado as AdoProvider)?.Dispose();
            }
            finally
            {
                FreeSqlOdbcGlobalExtensions._dicOdbcAdater.TryRemove(this as IFreeSql, out var tryada);
            }
        }
    }
}

partial class FreeSqlOdbcGlobalExtensions
{
    internal static OdbcAdapter DefaultOdbcAdapter = new OdbcAdapter();
    internal static ConcurrentDictionary<IFreeSql, OdbcAdapter> _dicOdbcAdater = new ConcurrentDictionary<IFreeSql, OdbcAdapter>();
    public static void SetOdbcAdapter(this IFreeSql that, OdbcAdapter adapter) => _dicOdbcAdater.AddOrUpdate(that, adapter, (fsql, old) => adapter);
    internal static OdbcAdapter GetOdbcAdapter(this IFreeSql that) => _dicOdbcAdater.TryGetValue(that, out var tryada) ? tryada : DefaultOdbcAdapter;
}
