using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using FreeSql;

public static class FreeSqlJsonMapCoreExtensions
{
    static int _isAoped = 0;
    static ConcurrentDictionary<Type, bool> _dicTypes = new ConcurrentDictionary<Type, bool>();
    static MethodInfo MethodJsonConvertDeserializeObject = typeof(JsonConvert).GetMethod("DeserializeObject", new[] { typeof(string), typeof(Type) });
    static MethodInfo MethodJsonConvertSerializeObject = typeof(JsonConvert).GetMethod("SerializeObject", new[] { typeof(object), typeof(JsonSerializerSettings) });
    static ConcurrentDictionary<Type, ConcurrentDictionary<string, bool>> _dicJsonMapFluentApi = new ConcurrentDictionary<Type, ConcurrentDictionary<string, bool>>();

    public static ColumnFluent JsonMap(this ColumnFluent col)
    {
        _dicJsonMapFluentApi.GetOrAdd(col._entityType, et => new ConcurrentDictionary<string, bool>())
            .GetOrAdd(col._property.Name, pn => true);
        return col;
    }

    /// <summary>
    /// 当实体类属性为【对象】时，并且标记特性 [JsonMap] 时，该属性将以JSON形式映射存储
    /// </summary>
    /// <returns></returns>
    public static void UseJsonMap(this IFreeSql that)
    {
        UseJsonMap(that, JsonConvert.DefaultSettings?.Invoke() ?? new JsonSerializerSettings());
    }

    public static void UseJsonMap(this IFreeSql that, JsonSerializerSettings settings)
    {
        if (Interlocked.CompareExchange(ref _isAoped, 1, 0) == 0)
        {
            FreeSql.Internal.Utils.GetDataReaderValueBlockExpressionSwitchTypeFullName.Add((LabelTarget returnTarget, Expression valueExp, Type type) =>
            {
                if (_dicTypes.ContainsKey(type)) return Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodJsonConvertDeserializeObject, Expression.Convert(valueExp, typeof(string)), Expression.Constant(type)), type));
                return null;
            });
        }

        that.Aop.ConfigEntityProperty += new EventHandler<FreeSql.Aop.ConfigEntityPropertyEventArgs>((s, e) =>
        {
            var isJsonMap = e.Property.GetCustomAttributes(typeof(JsonMapAttribute), false).Any() || _dicJsonMapFluentApi.TryGetValue(e.EntityType, out var tryjmfu) && tryjmfu.ContainsKey(e.Property.Name);
            if (isJsonMap)
            {
                e.ModifyResult.MapType = typeof(string);
                
                // pgsql默认使用jsonb，如果Column特性指定了DbType/TypeName，则优先使用特性指定的数据类型
                var typeName =
                    (e.Property.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault() as ColumnAttribute)
                    ?.DbType;
                if (string.IsNullOrWhiteSpace(typeName))
                {
                    var attr = e.Property
                        .GetCustomAttributes(false)
                        .FirstOrDefault(a => ((a as Attribute)?.TypeId as Type)?.FullName ==
                                             "System.ComponentModel.DataAnnotations.Schema.ColumnAttribute");
                    if (attr != null)
                    {
                        typeName = attr.GetType().GetProperties()
                            .FirstOrDefault(a => a.PropertyType == typeof(string) && a.Name == "TypeName")
                            ?.GetValue(attr, null)?.ToString();
                    }
                }
                
                if (string.IsNullOrWhiteSpace(typeName))
                {
                    if (s is IFreeSql freeSql && (freeSql.Ado.DataType == DataType.PostgreSQL || freeSql.Ado.DataType == DataType.OdbcPostgreSQL))
                    {
                        e.ModifyResult.DbType = "jsonb";   
                    }
                    else
                    {
                        e.ModifyResult.StringLength = -2;
                    }
                }
                
                if (_dicTypes.TryAdd(e.Property.PropertyType, true))
                {
                    FreeSql.Internal.Utils.GetDataReaderValueBlockExpressionObjectToStringIfThenElse.Add((LabelTarget returnTarget, Expression valueExp, Expression elseExp, Type type) =>
                    {
                        return Expression.IfThenElse(
                            Expression.TypeIs(valueExp, e.Property.PropertyType),
                            Expression.Return(returnTarget, Expression.Call(MethodJsonConvertSerializeObject, Expression.Convert(valueExp, typeof(object)), Expression.Constant(settings)), typeof(object)),
                            elseExp);
                    });
                }
            }
        });
    }
}

