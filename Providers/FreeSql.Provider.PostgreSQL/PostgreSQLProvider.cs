using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.PostgreSQL.Curd;
using Newtonsoft.Json.Linq;
using Npgsql.LegacyPostgis;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace FreeSql.PostgreSQL
{

    public class PostgreSQLProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {

        static PostgreSQLProvider()
        {
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

            var MethodJTokenParse = typeof(JToken).GetMethod("Parse", new[] { typeof(string) });
            var MethodJObjectParse = typeof(JObject).GetMethod("Parse", new[] { typeof(string) });
            var MethodJArrayParse = typeof(JArray).GetMethod("Parse", new[] { typeof(string) });
            Utils.GetDataReaderValueBlockExpressionSwitchTypeFullName.Add((LabelTarget returnTarget, Expression valueExp, Type type) =>
            {
                switch (type.FullName)
                {
                    case "Newtonsoft.Json.Linq.JToken": return Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodJTokenParse, Expression.Convert(valueExp, typeof(string))), typeof(JToken)));
                    case "Newtonsoft.Json.Linq.JObject": return Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodJObjectParse, Expression.Convert(valueExp, typeof(string))), typeof(JObject)));
                    case "Newtonsoft.Json.Linq.JArray": return Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodJArrayParse, Expression.Convert(valueExp, typeof(string))), typeof(JArray)));
                    case "Npgsql.LegacyPostgis.PostgisGeometry": return Expression.Return(returnTarget, valueExp);
                    case "NetTopologySuite.Geometries.Geometry": return Expression.Return(returnTarget, valueExp);
                }
                return null;
            });
        }

        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new PostgreSQLSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new PostgreSQLInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new PostgreSQLUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new PostgreSQLDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new PostgreSQLInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public PostgreSQLProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new PostgreSQLUtils(this);
            this.InternalCommonExpression = new PostgreSQLExpression(this.InternalCommonUtils);

            this.Ado = new PostgreSQLAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new PostgreSQLDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new PostgreSQLCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        ~PostgreSQLProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
