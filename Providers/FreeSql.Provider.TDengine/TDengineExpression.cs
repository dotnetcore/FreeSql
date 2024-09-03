using FreeSql.Internal;
using System;
using System.Linq.Expressions;

namespace FreeSql.TDengine
{
    internal class TDengineExpression : CommonExpression
    {
        public TDengineExpression(CommonUtils common) : base(common)
        {
        }

        public override string ExpressionLambdaToSqlCallConvert(MethodCallExpression exp, ExpTSC tsc)
        {
            throw new NotImplementedException();
        }

        public override string ExpressionLambdaToSqlCallDateTime(MethodCallExpression exp, ExpTSC tsc)
        {
            throw new NotImplementedException();
        }

        public override string ExpressionLambdaToSqlCallMath(MethodCallExpression exp, ExpTSC tsc)
        {
            throw new NotImplementedException();
        }

        public override string ExpressionLambdaToSqlCallString(MethodCallExpression exp, ExpTSC tsc)
        {
            throw new NotImplementedException();
        }

        public override string ExpressionLambdaToSqlMemberAccessDateTime(MemberExpression exp, ExpTSC tsc)
        {
            throw new NotImplementedException();
        }

        public override string ExpressionLambdaToSqlMemberAccessString(MemberExpression exp, ExpTSC tsc)
        {
            throw new NotImplementedException();
        }

        public override string ExpressionLambdaToSqlOther(Expression exp, ExpTSC tsc)
        {
            throw new NotImplementedException();
        }
    }
}