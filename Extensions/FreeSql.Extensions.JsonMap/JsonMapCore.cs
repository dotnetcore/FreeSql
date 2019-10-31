using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FreeSql.Extensions
{
    public static class JsonMapCore
    {
        static bool _isAoped = false;
        static object _isAopedLock = new object();
        static ConcurrentDictionary<Type, bool> _dicTypes = new ConcurrentDictionary<Type, bool>();
        static MethodInfo MethodJsonConvertDeserializeObject = typeof(JsonConvert).GetMethod("DeserializeObject", new[] { typeof(string), typeof(Type) });
        static MethodInfo MethodJsonConvertSerializeObject = typeof(JsonConvert).GetMethod("SerializeObject", new[] { typeof(object) });

        /// <summary>
        /// 当实体类属性为【对象】时，并且标记特性 [JsonMap] 时，该属性将以JSON形式映射存储
        /// </summary>
        /// <returns></returns>
        public static void UseJsonMap(this IFreeSql that)
        {
            if (_isAoped == false)
                lock(_isAopedLock)
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
                                            Expression.Return(returnTarget, Expression.Call(MethodJsonConvertSerializeObject, Expression.Convert(valueExp, typeof(object))), typeof(object)),
                                            elseExp);
                                    });
                                }
                            }
                        });
                    }
        }
    }
}
