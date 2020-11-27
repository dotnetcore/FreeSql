using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.MySql.Curd;
using System;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;

namespace FreeSql.MySql
{

    public class MySqlProvider<TMark> : BaseDbProvider, IFreeSql<TMark>
    {
        static MySqlProvider()
        {
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(MygisPoint)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(MygisLineString)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(MygisPolygon)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(MygisMultiPoint)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(MygisMultiLineString)] = true;
            Utils.dicExecuteArrayRowReadClassOrTuple[typeof(MygisMultiPolygon)] = true;

            var MethodMygisGeometryParse = typeof(MygisGeometry).GetMethod("Parse", new[] { typeof(string) });
            Utils.GetDataReaderValueBlockExpressionSwitchTypeFullName.Add((LabelTarget returnTarget, Expression valueExp, Type type) =>
            {
                switch (type.FullName)
                {
                    case "MygisPoint": return Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodMygisGeometryParse, Expression.Convert(valueExp, typeof(string))), typeof(MygisPoint)));
                    case "MygisLineString": return Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodMygisGeometryParse, Expression.Convert(valueExp, typeof(string))), typeof(MygisLineString)));
                    case "MygisPolygon": return Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodMygisGeometryParse, Expression.Convert(valueExp, typeof(string))), typeof(MygisPolygon)));
                    case "MygisMultiPoint": return Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodMygisGeometryParse, Expression.Convert(valueExp, typeof(string))), typeof(MygisMultiPoint)));
                    case "MygisMultiLineString": return Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodMygisGeometryParse, Expression.Convert(valueExp, typeof(string))), typeof(MygisMultiLineString)));
                    case "MygisMultiPolygon": return Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(MethodMygisGeometryParse, Expression.Convert(valueExp, typeof(string))), typeof(MygisMultiPolygon)));
                }
                return null;
            });
        }

        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => new MySqlSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => new MySqlInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => new MySqlUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => new MySqlDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => new MySqlInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public MySqlProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new MySqlUtils(this);
            this.InternalCommonExpression = new MySqlExpression(this.InternalCommonUtils);

            this.Ado = new MySqlAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new MySqlDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new MySqlCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        ~MySqlProvider() => this.Dispose();
        int _disposeCounter;
        public override void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
