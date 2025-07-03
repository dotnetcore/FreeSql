using FreeSql.DataAnnotations;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

class MessagePackMapInfo
{
    public Guid id { get; set; }
    [MessagePackMap]
    public MessagePackMap01 Info { get; set; }
}

[MessagePackObject]
public class MessagePackMap01
{
    [Key(0)]
    public string name { get; set; }
    [Key(1)]
    public string address { get;set; }
}

namespace FreeSql.DataAnnotations
{
    public class MessagePackMapAttribute : Attribute { }
}

public static class FreeSqlMessagePackMapCoreExtensions
{
    internal static int _isAoped = 0;
    static ConcurrentDictionary<Type, bool> _dicTypes = new ConcurrentDictionary<Type, bool>();
    static MethodInfo MethodMessagePackSerializerDeserialize = typeof(MessagePackSerializer).GetMethod("Deserialize", new[] { typeof(Type), typeof(ReadOnlyMemory<byte>), typeof(MessagePackSerializerOptions), typeof(CancellationToken) });
    static MethodInfo MethodMessagePackSerializerSerialize = typeof(MessagePackSerializer).GetMethod("Serialize", new[] { typeof(Type), typeof(object), typeof(MessagePackSerializerOptions), typeof(CancellationToken) });
    static ConcurrentDictionary<Type, ConcurrentDictionary<string, bool>> _dicMessagePackMapFluentApi = new ConcurrentDictionary<Type, ConcurrentDictionary<string, bool>>();
    static object _concurrentObj = new object();

    public static ColumnFluent MessagePackMap(this ColumnFluent col)
    {
        _dicMessagePackMapFluentApi.GetOrAdd(col._entityType, et => new ConcurrentDictionary<string, bool>())
                            .GetOrAdd(col._property.Name, pn => true);
        return col;
    }

    public static void UseMessagePackMap(this IFreeSql that)
    {
        UseMessagePackMap(that, MessagePackSerializerOptions.Standard);
    }

    public static void UseMessagePackMap(this IFreeSql that, MessagePackSerializerOptions settings)
    {
        if (Interlocked.CompareExchange(ref _isAoped, 1, 0) == 0)
        {
            FreeSql.Internal.Utils.GetDataReaderValueBlockExpressionSwitchTypeFullName.Add((LabelTarget returnTarget, Expression valueExp, Type type) =>
            {
                if (_dicTypes.ContainsKey(type)) 
                    return Expression.IfThenElse(
                    Expression.TypeIs(valueExp, type),
                    Expression.Return(returnTarget, valueExp),
                    Expression.Return(returnTarget, Expression.TypeAs(
                        Expression.Call(MethodMessagePackSerializerDeserialize,
                            Expression.Constant(type),
                            Expression.New(typeof(ReadOnlyMemory<byte>).GetConstructor(new[] { typeof(byte[]) }), Expression.Convert(valueExp, typeof(byte[]))),
                            Expression.Constant(settings, typeof(MessagePackSerializerOptions)),
                            Expression.Constant(default(CancellationToken), typeof(CancellationToken)))
                    , type))
                );
                return null;
            });
        }

        that.Aop.ConfigEntityProperty += (s, e) =>
        {
            var isMessagePackMap = e.Property.GetCustomAttributes(typeof(MessagePackMapAttribute), false).Any() || _dicMessagePackMapFluentApi.TryGetValue(e.EntityType, out var tryjmfu) && tryjmfu.ContainsKey(e.Property.Name);
            if (isMessagePackMap)
            {
                e.ModifyResult.MapType = typeof(byte[]);
                e.ModifyResult.StringLength = -2;
                if (_dicTypes.TryAdd(e.Property.PropertyType, true))
                {
                    lock (_concurrentObj)
                    {
                        FreeSql.Internal.Utils.dicExecuteArrayRowReadClassOrTuple[e.Property.PropertyType] = true;
                        FreeSql.Internal.Utils.GetDataReaderValueBlockExpressionObjectToBytesIfThenElse.Add((LabelTarget returnTarget, Expression valueExp, Expression elseExp, Type type) =>
                        {
                            return Expression.IfThenElse(
                                Expression.TypeIs(valueExp, e.Property.PropertyType),
                                Expression.Return(returnTarget,
                                    Expression.Call(MethodMessagePackSerializerSerialize,
                                        Expression.Constant(e.Property.PropertyType, typeof(Type)),
                                        Expression.Convert(valueExp, typeof(object)),
                                        Expression.Constant(settings, typeof(MessagePackSerializerOptions)),
                                        Expression.Constant(default(CancellationToken), typeof(CancellationToken)))
                                , typeof(object)),
                            elseExp);
                        });
                    }
                }
            }
        };
    }
}