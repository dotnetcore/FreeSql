using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.MySql.Curd;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;

namespace FreeSql.MySql
{

    public class MySqlProvider<TMark> : IFreeSql<TMark>
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
            Utils.GetDataReaderValueBlockExpressionSwitchTypeFullName.Add((LabelTarget returnTarget, Expression valueExp, string typeFullName) =>
            {
                switch (typeFullName)
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

        public ISelect<T1> Select<T1>() where T1 : class => new MySqlSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public ISelect<T1> Select<T1>(object dywhere) where T1 : class => new MySqlSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public IInsert<T1> Insert<T1>() where T1 : class => new MySqlInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public IInsert<T1> Insert<T1>(T1 source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(T1[] source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(IEnumerable<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IUpdate<T1> Update<T1>() where T1 : class => new MySqlUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public IUpdate<T1> Update<T1>(object dywhere) where T1 : class => new MySqlUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public IDelete<T1> Delete<T1>() where T1 : class => new MySqlDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public IDelete<T1> Delete<T1>(object dywhere) where T1 : class => new MySqlDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public IAdo Ado { get; }
        public IAop Aop { get; }
        public ICodeFirst CodeFirst { get; }
        public IDbFirst DbFirst { get; }
        public MySqlProvider(string masterConnectionString, string[] slaveConnectionString)
        {
            this.InternalCommonUtils = new MySqlUtils(this);
            this.InternalCommonExpression = new MySqlExpression(this.InternalCommonUtils);

            this.Ado = new MySqlAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString);
            this.Aop = new AopProvider();

            this.DbFirst = new MySqlDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new MySqlCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        internal CommonUtils InternalCommonUtils { get; }
        internal CommonExpression InternalCommonExpression { get; }

        public void Transaction(Action handler) => Ado.Transaction(handler);

        public void Transaction(Action handler, TimeSpan timeout) => Ado.Transaction(handler, timeout);

        ~MySqlProvider()
        {
            this.Dispose();
        }
        bool _isdisposed = false;
        public void Dispose()
        {
            if (_isdisposed) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
