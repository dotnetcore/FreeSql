using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
#if NETCORE30
using System.Text.Json;
#endif
namespace FreeSql.Extensions
{
    public static class JsonMapCore
    {
        static bool _isAoped = false;
        static object _isAopedLock = new object();
        static ConcurrentDictionary<Type, bool> _dicTypes = new ConcurrentDictionary<Type, bool>();
#if NETCORE30
        static MethodInfo MethodJsonConvertDeserializeObject = typeof(JsonSerializer).GetMethod(nameof(JsonSerializer.Deserialize), new[] { typeof(string), typeof(Type) });

        static MethodInfo MethodJsonConvertSerializeObject = typeof(JsonSerializer).GetMethod(nameof(JsonSerializer.Serialize), new[] { typeof(object), typeof(Type), typeof(JsonSerializerOptions) });
#else
        static MethodInfo MethodJsonConvertDeserializeObject = typeof(JsonConvert).GetMethod(nameof(JsonConvert.DeserializeObject), new[] { typeof(string), typeof(Type) });

        static MethodInfo MethodJsonConvertSerializeObject = typeof(JsonConvert).GetMethod(nameof(JsonConvert.SerializeObject), new[] { typeof(object), typeof(JsonSerializerSettings) });
#endif
        /// <summary>
        /// 当实体类属性为【对象】时，并且标记特性 [JsonMap] 时，该属性将以JSON形式映射存储
        /// </summary>
        /// <returns></returns>
        public static void UseJsonMap(this IFreeSql that)
        {
            UseJsonMap(that,
#if NETCORE30
       new JsonSerializerOptions()

#else
      new JsonSerializerSettings()
#endif
      );}
#if NETCORE30
        public static void UseJsonMap(this IFreeSql that, JsonSerializerOptions settings)
#else
        public static void UseJsonMap(this IFreeSql that, JsonSerializerSettings settings)
#endif

        {
            if (_isAoped == false)
                lock (_isAopedLock)
                    if (_isAoped == false)
                    {
                        _isAoped = true;

                        FreeSql.Internal.Utils.GetDataReaderValueBlockExpressionSwitchTypeFullName.Add((LabelTarget returnTarget, Expression valueExp, Type type) =>
                        {
                            if (_dicTypes.ContainsKey(type)) return Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodJsonConvertDeserializeObject, Expression.Convert(valueExp, typeof(string)), Expression.Constant(type)), type));
                            return null;
                        });

                        that.Aop.ConfigEntityProperty += new EventHandler<Aop.ConfigEntityPropertyEventArgs>((s, e) =>
                        {
                            if (e.Property.GetCustomAttributes(typeof(JsonMapAttribute), false).Any())
                            {
                                e.ModifyResult.MapType = typeof(string);
                                if (_dicTypes.TryAdd(e.Property.PropertyType, true))
                                {
                                    FreeSql.Internal.Utils.GetDataReaderValueBlockExpressionObjectToStringIfThenElse.Add((LabelTarget returnTarget, Expression valueExp, Expression elseExp, Type type) =>
                                    {
                                        return Expression.IfThenElse(
                                            Expression.TypeEqual(valueExp, e.Property.PropertyType),
                                            Expression.Return(returnTarget, Expression.Call(MethodJsonConvertSerializeObject,
#if NETCORE30
    Expression.Convert(valueExp, typeof(object)), Expression.Constant(valueExp.Type), Expression.Constant(settings))
#else
   Expression.Convert(valueExp, typeof(object)), Expression.Constant(settings))
#endif
                                            , typeof(object)),
                                            elseExp);
                                    });
                                }
                            }
                        });
                    }
        }

    }
}
