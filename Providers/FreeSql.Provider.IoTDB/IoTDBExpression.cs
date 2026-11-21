using System.Linq.Expressions;
using FreeSql.Internal;

namespace FreeSql.IoTDB
{
    internal sealed class IoTDBExpression : CommonExpression
    {
        public IoTDBExpression(CommonUtils common) : base(common) { }

        public override string ExpressionLambdaToSqlMemberAccessString(MemberExpression exp, ExpTSC tsc) => null;
        public override string ExpressionLambdaToSqlMemberAccessDateTime(MemberExpression exp, ExpTSC tsc) => null;
        public override string ExpressionLambdaToSqlCallString(MethodCallExpression exp, ExpTSC tsc) => null;
        public override string ExpressionLambdaToSqlCallMath(MethodCallExpression exp, ExpTSC tsc) => null;
        public override string ExpressionLambdaToSqlCallDateTime(MethodCallExpression exp, ExpTSC tsc) => null;
        public override string ExpressionLambdaToSqlCallConvert(MethodCallExpression exp, ExpTSC tsc) => null;
        public override string ExpressionLambdaToSqlOther(Expression exp, ExpTSC tsc) => null;
    }
}
