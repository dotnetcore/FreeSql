using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using KdbndpTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection;
using System.Threading;

namespace FreeSql.KingbaseES
{

    public class KingbaseESProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
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
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(KdbndpPoint)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(KdbndpLine)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(KdbndpLSeg)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(KdbndpBox)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(KdbndpPath)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(KdbndpPolygon)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(KdbndpCircle)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof((IPAddress Address, int Subnet))] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(IPAddress)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PhysicalAddress)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(KdbndpRange<int>)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(KdbndpRange<long>)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(KdbndpRange<decimal>)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(KdbndpRange<DateTime>)] = true;

                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(Dictionary<string, string>)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(JToken)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(JObject)] = true;
                Utils.dicExecuteArrayRowReadClassOrTuple[typeof(JArray)] = true;

                var MethodJTokenFromObject = typeof(JToken).GetMethod("FromObject", new[] { typeof(object) });
                var MethodJObjectFromObject = typeof(JObject).GetMethod("FromObject", new[] { typeof(object) });
                var MethodJArrayFromObject = typeof(JArray).GetMethod("FromObject", new[] { typeof(object) });
                var MethodJTokenParse = typeof(JToken).GetMethod("Parse", new[] { typeof(string) });
                var MethodJObjectParse = typeof(JObject).GetMethod("Parse", new[] { typeof(string) });
                var MethodJArrayParse = typeof(JArray).GetMethod("Parse", new[] { typeof(string) });
                var MethodJsonConvertDeserializeObject = typeof(JsonConvert).GetMethod("DeserializeObject", new[] { typeof(string), typeof(Type) });
                var MethodToString = typeof(Utils).GetMethod("ToStringConcat", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object) }, null);
                Utils.GetDataReaderValueBlockExpressionSwitchTypeFullName.Add((LabelTarget returnTarget, Expression valueExp, Type type) =>
                {
                    switch (type.FullName)
                    {
                        case "Newtonsoft.Json.Linq.JToken":
                            return Expression.IfThenElse(
                                Expression.TypeIs(valueExp, typeof(string)),
                                Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodJTokenParse, Expression.Convert(valueExp, typeof(string))), typeof(JToken))),
                                Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodJTokenFromObject, valueExp), typeof(JToken))));
                        case "Newtonsoft.Json.Linq.JObject":
                            return Expression.IfThenElse(
                                Expression.TypeIs(valueExp, typeof(string)),
                                Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodJObjectParse, Expression.Convert(valueExp, typeof(string))), typeof(JObject))),
                                Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodJObjectFromObject, valueExp), typeof(JObject))));
                        case "Newtonsoft.Json.Linq.JArray":
                            return Expression.IfThenElse(
                                Expression.TypeIs(valueExp, typeof(string)),
                                Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodJArrayParse, Expression.Convert(valueExp, typeof(string))), typeof(JArray))),
                                Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodJArrayFromObject, valueExp), typeof(JArray))));
                    }
                    if (typeof(IList).IsAssignableFrom(type))
                        return Expression.IfThenElse(
                            Expression.TypeIs(valueExp, typeof(string)),
                            Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodJsonConvertDeserializeObject, Expression.Convert(valueExp, typeof(string)), Expression.Constant(type, typeof(Type))), type)),
                            Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodJsonConvertDeserializeObject, Expression.Convert(Expression.Call(MethodToString, valueExp), typeof(string)), Expression.Constant(type, typeof(Type))), type)));
                    return null;
                });

                Select0Provider._dicMethodDataReaderGetValue[typeof(Guid)] = typeof(DbDataReader).GetMethod("GetGuid", new Type[] { typeof(int) });
            }
        }

        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new KingbaseESSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new KingbaseESInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new KingbaseESUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new KingbaseESDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new KingbaseESInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public KingbaseESProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            InitInternal();
            this.InternalCommonUtils = new KingbaseESUtils(this);
            this.InternalCommonExpression = new KingbaseESExpression(this.InternalCommonUtils);

            this.Ado = new KingbaseESAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new KingbaseESDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new KingbaseESCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        ~KingbaseESProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
