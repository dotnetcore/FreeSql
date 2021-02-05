using FreeSql.Custom;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;

namespace FreeSql.Custom
{

    public class CustomProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new CustomSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new CustomInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new CustomUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new CustomDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => throw new NotImplementedException("FreeSql.Provider.Custom 未实现该功能");

        public override IDbFirst DbFirst => throw new NotImplementedException("FreeSql.Provider.Custom 未实现该功能");

        public CustomProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new CustomUtils(this);
            this.InternalCommonExpression = new CustomExpression(this.InternalCommonUtils);

            this.Ado = new CustomAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.CodeFirst = new CustomCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);

            var _utils = InternalCommonUtils as CustomUtils;
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
                                    e.ModifyResult.DbType = tryval > 0 ? $"{_utils.Adapter.MappingDbTypeVarChar}({tryval})" : _utils.Adapter.MappingDbTypeText;
                                    break;
                            }
                        }
                    }
                }
            });
        }

        ~CustomProvider() => this.Dispose();
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
            }
        }
    }
}

public static class FreeSqlCustomAdapterGlobalExtensions
{
    internal static CustomAdapter DefaultAdapter = new CustomAdapter();
    internal static ConcurrentDictionary<Guid, CustomAdapter> _dicCustomAdater = new ConcurrentDictionary<Guid, CustomAdapter>();
    public static void SetCustomAdapter(this IFreeSql that, CustomAdapter adapter) => _dicCustomAdater.AddOrUpdate(that.Ado.Identifier, adapter, (fsql, old) => adapter);
    internal static CustomAdapter GetCustomAdapter(this IFreeSql that) => _dicCustomAdater.TryGetValue(that.Ado.Identifier, out var tryada) ? tryada : DefaultAdapter;
}
