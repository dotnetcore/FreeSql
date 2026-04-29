using FreeSql.Internal;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.SonnetDB
{
    class SonnetDBExpression : CommonExpression
    {
        public SonnetDBExpression(CommonUtils common) : base(common) { }

        public override string ExpressionLambdaToSqlOther(Expression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            switch (exp.NodeType)
            {
                case ExpressionType.Convert:
                    var operandExp = (exp as UnaryExpression)?.Operand;
                    var gentype = exp.Type.NullableTypeOrThis();
                    if (operandExp != null && gentype != operandExp.Type.NullableTypeOrThis())
                    {
                        switch (gentype.ToString())
                        {
                            case "System.Boolean": return $"({getExp(operandExp)} not in ('0','false'))";
                            case "System.String": return $"{getExp(operandExp)}";
                            case "System.DateTime": return ExpressionConstDateTime(operandExp) ?? getExp(operandExp);
                            case "System.Decimal":
                            case "System.Double":
                            case "System.Single":
                            case "System.Int16":
                            case "System.Int32":
                            case "System.Int64":
                            case "System.Byte":
                            case "System.SByte":
                            case "System.UInt16":
                            case "System.UInt32":
                            case "System.UInt64":
                                return getExp(operandExp);
                        }
                    }
                    break;
                case ExpressionType.Call:
                    var callExp = exp as MethodCallExpression;
                    switch (callExp.Method.Name)
                    {
                        case "ToString":
                            if (callExp.Object != null)
                            {
                                var value = ExpressionGetValue(callExp.Object, out var success);
                                if (success) return formatSql(value, typeof(string), null, null);
                                return callExp.Arguments.Count == 0 ? getExp(callExp.Object) : null;
                            }
                            return null;
                    }

                    var objExp = callExp.Object;
                    var objType = objExp?.Type;
                    var argIndex = 0;
                    if (objType == null && (callExp.Method.DeclaringType == typeof(Enumerable) || callExp.Method.DeclaringType.FullName == "System.MemoryExtensions"))
                    {
                        objExp = callExp.Arguments.FirstOrDefault();
                        objType = objExp?.Type;
                        argIndex++;
                    }
                    if (objType == null) objType = callExp.Method.DeclaringType;
                    if (objType != null || objType.IsArrayOrList())
                    {
                        if (argIndex >= callExp.Arguments.Count) break;
                        switch (callExp.Method.Name)
                        {
                            case "Contains":
                                tsc.SetMapColumnTmp(null);
                                var args1 = getExp(callExp.Arguments[argIndex]);
                                var oldMapType = tsc.SetMapTypeReturnOld(tsc.mapTypeTmp);
                                var oldDbParams = objExp?.NodeType == ExpressionType.MemberAccess ? tsc.SetDbParamsReturnOld(null) : null;
                                tsc.isNotSetMapColumnTmp = true;
                                var left = objExp == null ? null : getExp(objExp);
                                tsc.isNotSetMapColumnTmp = false;
                                tsc.SetMapColumnTmp(null).SetMapTypeReturnOld(oldMapType);
                                if (oldDbParams != null) tsc.SetDbParamsReturnOld(oldDbParams);
                                if (left != null && left.StartsWith("(") && left.EndsWith(")"))
                                    return $"(({args1}) in {left.Replace(",   \r\n    \r\n", $") \r\n OR ({args1}) in (")})";
                                break;
                        }
                    }
                    break;
                case ExpressionType.NewArrayInit:
                    var arrExp = exp as NewArrayExpression;
                    var arrSb = new StringBuilder();
                    arrSb.Append("(");
                    for (var a = 0; a < arrExp.Expressions.Count; a++)
                    {
                        if (a > 0) arrSb.Append(",");
                        if (a % 500 == 499) arrSb.Append("   \r\n    \r\n");
                        arrSb.Append(getExp(arrExp.Expressions[a]));
                    }
                    if (arrSb.Length == 1) arrSb.Append("NULL");
                    return arrSb.Append(")").ToString();
                case ExpressionType.ListInit:
                    var listExp = exp as ListInitExpression;
                    var listSb = new StringBuilder();
                    listSb.Append("(");
                    for (var a = 0; a < listExp.Initializers.Count; a++)
                    {
                        if (listExp.Initializers[a].Arguments.Any() == false) continue;
                        if (a > 0) listSb.Append(",");
                        listSb.Append(getExp(listExp.Initializers[a].Arguments.FirstOrDefault()));
                    }
                    if (listSb.Length == 1) listSb.Append("NULL");
                    return listSb.Append(")").ToString();
                case ExpressionType.New:
                    var newExp = exp as NewExpression;
                    if (typeof(IList).IsAssignableFrom(newExp.Type))
                    {
                        if (newExp.Arguments.Count == 0) return "(NULL)";
                        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(newExp.Arguments[0].Type) == false) return "(NULL)";
                        return getExp(newExp.Arguments[0]);
                    }
                    return null;
            }
            return null;
        }

        public override string ExpressionLambdaToSqlMemberAccessString(MemberExpression exp, ExpTSC tsc)
        {
            if (exp.Expression == null && exp.Member.Name == "Empty") return "''";
            return null;
        }

        public override string ExpressionLambdaToSqlMemberAccessDateTime(MemberExpression exp, ExpTSC tsc)
        {
            if (exp.Expression == null)
            {
                switch (exp.Member.Name)
                {
                    case "Now": return _common.Now;
                    case "UtcNow": return _common.NowUtc;
                    case "Today":
                        var now = DateTime.Today;
                        return new DateTimeOffset(now).ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
                    case "MinValue": return "0";
                    case "MaxValue": return long.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                return null;
            }
            return null;
        }

        public override string ExpressionLambdaToSqlCallString(MethodCallExpression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            if (exp.Object == null)
            {
                switch (exp.Method.Name)
                {
                    case "IsNullOrEmpty":
                        var arg1 = getExp(exp.Arguments[0]);
                        return $"({arg1} is null or {arg1} = '')";
                    case "IsNullOrWhiteSpace":
                        var arg2 = getExp(exp.Arguments[0]);
                        return $"({arg2} is null or {arg2} = '' or trim({arg2}) = '')";
                    case "Concat":
                        if (exp.Arguments.Count == 1 && exp.Arguments[0].NodeType == ExpressionType.NewArrayInit && exp.Arguments[0] is NewArrayExpression concatNewArrExp)
                            return _common.StringConcat(concatNewArrExp.Expressions.Select(a => getExp(a)).ToArray(), null);
                        return _common.StringConcat(exp.Arguments.Select(a => getExp(a)).ToArray(), null);
                }
            }
            else
            {
                var left = getExp(exp.Object);
                switch (exp.Method.Name)
                {
                    case "ToLower": return $"lower({left})";
                    case "ToUpper": return $"upper({left})";
                    case "Trim": return exp.Arguments.Count == 0 ? $"trim({left})" : null;
                    case "Equals": return $"({left} = {getExp(exp.Arguments[0])})";
                }
            }
            return null;
        }

        public override string ExpressionLambdaToSqlCallMath(MethodCallExpression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            switch (exp.Method.Name)
            {
                case "Abs": return $"abs({getExp(exp.Arguments[0])})";
                case "Round":
                    if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32")
                        return $"round({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                    return $"round({getExp(exp.Arguments[0])})";
                case "Sqrt": return $"sqrt({getExp(exp.Arguments[0])})";
                case "Log":
                    if (exp.Arguments.Count > 1) return $"log({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                    return $"log({getExp(exp.Arguments[0])})";
            }
            return null;
        }

        public override string ExpressionLambdaToSqlCallDateTime(MethodCallExpression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            if (exp.Object == null)
            {
                switch (exp.Method.Name)
                {
                    case "Equals": return $"({getExp(exp.Arguments[0])} = {getExp(exp.Arguments[1])})";
                    case "Parse":
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact":
                        return ExpressionConstDateTime(exp.Arguments[0]) ?? getExp(exp.Arguments[0]);
                }
            }
            else
            {
                var left = getExp(exp.Object);
                var args1 = exp.Arguments.Count == 0 ? null : getExp(exp.Arguments[0]);
                switch (exp.Method.Name)
                {
                    case "AddMilliseconds": return $"({left} + {args1})";
                    case "AddSeconds": return $"({left} + ({args1} * 1000))";
                    case "AddMinutes": return $"({left} + ({args1} * 60000))";
                    case "AddHours": return $"({left} + ({args1} * 3600000))";
                    case "AddDays": return $"({left} + ({args1} * 86400000))";
                    case "AddTicks": return $"({left} + ({args1} / 10000))";
                    case "Equals": return $"({left} = {args1})";
                    case "CompareTo": return $"({left} - {args1})";
                }
            }
            return null;
        }

        public override string ExpressionLambdaToSqlCallConvert(MethodCallExpression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            if (exp.Object == null)
            {
                switch (exp.Method.Name)
                {
                    case "ToBoolean": return $"({getExp(exp.Arguments[0])} not in ('0','false'))";
                    case "ToDateTime": return ExpressionConstDateTime(exp.Arguments[0]) ?? getExp(exp.Arguments[0]);
                    case "ToString": return getExp(exp.Arguments[0]);
                    case "ToByte":
                    case "ToChar":
                    case "ToDecimal":
                    case "ToDouble":
                    case "ToInt16":
                    case "ToInt32":
                    case "ToInt64":
                    case "ToSByte":
                    case "ToSingle":
                    case "ToUInt16":
                    case "ToUInt32":
                    case "ToUInt64":
                        return getExp(exp.Arguments[0]);
                }
            }
            return null;
        }
    }
}
