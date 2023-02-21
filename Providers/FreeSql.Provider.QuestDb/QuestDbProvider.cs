using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.PostgreSQL.Curd;
using FreeSql.QuestDb.Curd;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql.LegacyPostgis;
using NpgsqlTypes;
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

namespace FreeSql.QuestDb
{
    public class QuestDbProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        static QuestDbProvider()
        {
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(BigInteger)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(BitArray)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlPoint)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlLine)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlLSeg)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlBox)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlPath)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlPolygon)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlCircle)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof((IPAddress Address, int Subnet))] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(IPAddress)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PhysicalAddress)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlRange<int>)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlRange<long>)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlRange<decimal>)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlRange<DateTime>)] = true;

            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisPoint)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisLineString)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisPolygon)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisMultiPoint)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisMultiLineString)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisMultiPolygon)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisGeometry)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisGeometryCollection)] = true;

#if nts
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NetTopologySuite.Geometries.Point)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NetTopologySuite.Geometries.LineString)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NetTopologySuite.Geometries.Polygon)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NetTopologySuite.Geometries.MultiPoint)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NetTopologySuite.Geometries.MultiLineString)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NetTopologySuite.Geometries.MultiPolygon)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NetTopologySuite.Geometries.Geometry)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NetTopologySuite.Geometries.GeometryCollection)] = true;
#endif

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
            var MethodJsonConvertDeserializeObject =
                typeof(JsonConvert).GetMethod("DeserializeObject", new[] { typeof(string), typeof(Type) });
            var MethodToString = typeof(Utils).GetMethod("ToStringConcat", BindingFlags.Public | BindingFlags.Static,
                null, new[] { typeof(object) }, null);
            Utils.GetDataReaderValueBlockExpressionSwitchTypeFullName.Add(
                (LabelTarget returnTarget, Expression valueExp, Type type) =>
                {
                    switch (type.FullName)
                    {
                        case "Newtonsoft.Json.Linq.JToken":
                            return Expression.IfThenElse(
                                Expression.TypeIs(valueExp, typeof(string)),
                                Expression.Return(returnTarget,
                                    Expression.TypeAs(
                                        Expression.Call(MethodJTokenParse,
                                            Expression.Convert(valueExp, typeof(string))), typeof(JToken))),
                                Expression.Return(returnTarget,
                                    Expression.TypeAs(Expression.Call(MethodJTokenFromObject, valueExp),
                                        typeof(JToken))));
                        case "Newtonsoft.Json.Linq.JObject":
                            return Expression.IfThenElse(
                                Expression.TypeIs(valueExp, typeof(string)),
                                Expression.Return(returnTarget,
                                    Expression.TypeAs(
                                        Expression.Call(MethodJObjectParse,
                                            Expression.Convert(valueExp, typeof(string))), typeof(JObject))),
                                Expression.Return(returnTarget,
                                    Expression.TypeAs(Expression.Call(MethodJObjectFromObject, valueExp),
                                        typeof(JObject))));
                        case "Newtonsoft.Json.Linq.JArray":
                            return Expression.IfThenElse(
                                Expression.TypeIs(valueExp, typeof(string)),
                                Expression.Return(returnTarget,
                                    Expression.TypeAs(
                                        Expression.Call(MethodJArrayParse,
                                            Expression.Convert(valueExp, typeof(string))), typeof(JArray))),
                                Expression.Return(returnTarget,
                                    Expression.TypeAs(Expression.Call(MethodJArrayFromObject, valueExp),
                                        typeof(JArray))));
                        case "Npgsql.LegacyPostgis.PostgisGeometry":
                            return Expression.Return(returnTarget, valueExp);
                        case "NetTopologySuite.Geometries.Geometry":
                            return Expression.Return(returnTarget, valueExp);
                    }

                    if (typeof(IList).IsAssignableFrom(type))
                        return Expression.IfThenElse(
                            Expression.TypeIs(valueExp, typeof(string)),
                            Expression.Return(returnTarget,
                                Expression.TypeAs(
                                    Expression.Call(MethodJsonConvertDeserializeObject,
                                        Expression.Convert(valueExp, typeof(string)),
                                        Expression.Constant(type, typeof(Type))), type)),
                            Expression.Return(returnTarget,
                                Expression.TypeAs(
                                    Expression.Call(MethodJsonConvertDeserializeObject,
                                        Expression.Convert(Expression.Call(MethodToString, valueExp), typeof(string)),
                                        Expression.Constant(type, typeof(Type))), type)));
                    return null;
                });

            Select0Provider._dicMethodDataReaderGetValue[typeof(Guid)] =
                typeof(DbDataReader).GetMethod("GetGuid", new Type[] { typeof(int) });
        }

        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) =>
            new QuestDbSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public override IInsert<T1> CreateInsertProvider<T1>() =>
            new QuestDbInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) =>
            new QuestDbUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) =>
            new QuestDbDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() =>
            new QuestDbInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public QuestDbProvider(string masterConnectionString, string[] slaveConnectionString,
            Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new QuestDbUtils(this);
            this.InternalCommonExpression = new QuestDbExpression(this.InternalCommonUtils);

            this.Ado = new QuestDbAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString,
                connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new QuestDbDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new QuestDbCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);

            //this.Aop.AuditDataReader += (_, e) =>
            //{
            //    var dbtype = e.DataReader.GetDataTypeName(e.Index);
            //    var m = Regex.Match(dbtype, @"numeric\((\d+)\)", RegexOptions.IgnoreCase);
            //    if (m.Success && int.Parse(m.Groups[1].Value) > 19)
            //        e.Value = e.DataReader.GetFieldValue<BigInteger>(e.Index);
            //};
        }

        ~QuestDbProvider() => this.Dispose();
        int _disposeCounter;

        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}