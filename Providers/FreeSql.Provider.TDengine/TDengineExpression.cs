using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FreeSql.Internal;

namespace FreeSql.Provider.TDengine
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