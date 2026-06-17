// SonnetDBExpression.cs
// SonnetDB 提供程序的 C# Lambda 表达式 → SQL 片段翻译器。
//
// 继承 FreeSql.Internal.CommonExpression，按需重写各类节点的翻译方法：
//   ExpressionLambdaToSqlOther        — 处理类型转换、Contains(IN展开)、数组/列表字面量
//   ExpressionLambdaToSqlMemberAccess — 处理静态成员（string.Empty / DateTime.Now 等）
//   ExpressionLambdaToSqlCallString   — 翻译 string 实例方法（ToLower/Trim/Contains 等）
//   ExpressionLambdaToSqlCallMath     — 翻译 Math 静态方法（Abs/Round/Sqrt 等）
//   ExpressionLambdaToSqlCallDateTime — 翻译 DateTime 方法（AddSeconds/AddDays 等时间加减）
//   ExpressionLambdaToSqlCallConvert  — 翻译 Convert 静态方法（ToBoolean/ToDouble 等）
//
// SonnetDB 特有说明：
//   • time 列存储 Unix 毫秒整数，DateTime 运算以毫秒为单位做整数加减。
//   • SonnetDB 不支持 CAST，数值类型转换直接透传原始 SQL 列名即可。

using FreeSql.Internal;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.SonnetDB
{
    /// <summary>
    /// SonnetDB 专用 Lambda 表达式翻译器，将 C# 表达式翻译为 SonnetDB SQL 片段。
    /// </summary>
    class SonnetDBExpression : CommonExpression
    {
        public SonnetDBExpression(CommonUtils common) : base(common) { }

        /// <summary>
        /// 处理其他类型表达式节点（Convert 类型转换、Contains IN 展开、数组/列表字面量）。
        /// </summary>
        public override string ExpressionLambdaToSqlOther(Expression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            switch (exp.NodeType)
            {
                case ExpressionType.Convert:
                    // 处理 C# 隐式/显式类型转换表达式。
                    // SonnetDB 不支持 CAST，数值类型直接透传原始列 SQL；
                    // Boolean 转换使用 NOT IN ('0','false') 模拟；
                    // DateTime 转换尝试提取常量时间戳，否则直接透传。
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
                                // SonnetDB 中数值类型之间可直接比较，无需 CAST，透传原列即可。
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
                                // 集合 Contains 翻译为 IN 子句。
                                // 若集合元素超过 500 个，分批拆分为多个 IN (...)，用 OR 连接，
                                // 避免单次 IN 过长超出 SonnetDB SQL 长度限制。
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
                    // 数组字面量 new[]{ v1, v2, v3 } 翻译为 SQL (v1, v2, v3)，用于 IN 子句。
                    // 每 500 个元素插入换行，避免超长日志。
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
                    // List<T> 字面量 new List<T>{ v1, v2 } 翻译为 SQL (v1, v2)。
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

        /// <summary>
        /// 处理 string 类型静态成员访问。
        /// <para><c>string.Empty</c> → SQL 空字符串 <c>''</c>。</para>
        /// </summary>
        public override string ExpressionLambdaToSqlMemberAccessString(MemberExpression exp, ExpTSC tsc)
        {
            if (exp.Expression == null && exp.Member.Name == "Empty") return "''";
            return null;
        }

        /// <summary>
        /// 处理 DateTime 类型静态成员访问。
        /// <para>SonnetDB 的 time 列存储 Unix 毫秒整数，因此：</para>
        /// <list type="bullet">
        ///   <item><c>DateTime.Now</c> → 当前时区的 Unix 毫秒时间戳</item>
        ///   <item><c>DateTime.UtcNow</c> → UTC 的 Unix 毫秒时间戳</item>
        ///   <item><c>DateTime.Today</c> → 当天零时的 Unix 毫秒时间戳（DateTimeOffset）</item>
        ///   <item><c>DateTime.MinValue</c> → 0（Unix 纪元起点）</item>
        ///   <item><c>DateTime.MaxValue</c> → Int64.MaxValue</item>
        /// </list>
        /// </summary>
        public override string ExpressionLambdaToSqlMemberAccessDateTime(MemberExpression exp, ExpTSC tsc)
        {
            if (exp.Expression == null)
            {
                switch (exp.Member.Name)
                {
                    case "Now": return _common.Now;
                    case "UtcNow": return _common.NowUtc;
                    case "Today":
                        // 取当天零时对应的 Unix 毫秒时间戳。
                        var now = DateTime.Today;
                        return new DateTimeOffset(now).ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
                    case "MinValue": return "0";
                    case "MaxValue": return long.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// 翻译 <see cref="string"/> 实例方法调用。
        /// <para>支持：ToLower、ToUpper、Trim/TrimStart/TrimEnd、Equals、
        /// StartsWith、EndsWith、Contains（均翻译为 LIKE 表达式）、
        /// IsNullOrEmpty、IsNullOrWhiteSpace、Concat（→ concat(...)）。</para>
        /// </summary>
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
                        // string.Concat 翻译为 SonnetDB concat(...) 函数。
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
                    case "TrimStart": return exp.Arguments.Count == 0 ? $"ltrim({left})" : null;
                    case "TrimEnd": return exp.Arguments.Count == 0 ? $"rtrim({left})" : null;
                    case "Equals": return $"({left} = {getExp(exp.Arguments[0])})";
                    case "StartsWith":
                    {
                        // 若参数是编译期常量则直接内联字符串（避免 concat 开销）。
                        var val = ExpressionGetValue(exp.Arguments[0], out var ok);
                        if (ok) return $"({left} like '{val?.ToString().Replace("'", "''")}%')";
                        return $"({left} like concat({getExp(exp.Arguments[0])}, '%'))";
                    }
                    case "EndsWith":
                    {
                        var val = ExpressionGetValue(exp.Arguments[0], out var ok);
                        if (ok) return $"({left} like '%{val?.ToString().Replace("'", "''")}')";
                        return $"({left} like concat('%', {getExp(exp.Arguments[0])}))";
                    }
                    case "Contains":
                    {
                        // string.Contains 翻译为 LIKE '%...%'。
                        var val = ExpressionGetValue(exp.Arguments[0], out var ok);
                        if (ok) return $"({left} like '%{val?.ToString().Replace("'", "''")}%')";
                        return $"({left} like concat('%', {getExp(exp.Arguments[0])}, '%'))";
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 翻译 <see cref="Math"/> 静态方法调用。
        /// <para>支持：Abs、Round（可选精度）、Sqrt、Log（可选底数）、Exp、
        /// Ceiling（→ ceil）、Floor、Pow（→ power）。</para>
        /// </summary>
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
                    // Math.Log(x, base) → log(x, base)；单参数版 → log(x)（自然对数）。
                    if (exp.Arguments.Count > 1) return $"log({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                    return $"log({getExp(exp.Arguments[0])})";
                case "Exp": return $"exp({getExp(exp.Arguments[0])})";
                case "Ceiling": return $"ceil({getExp(exp.Arguments[0])})";   // C# Ceiling → SonnetDB ceil
                case "Floor": return $"floor({getExp(exp.Arguments[0])})";
                case "Pow": return $"power({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
            }
            return null;
        }

        /// <summary>
        /// 翻译 <see cref="DateTime"/> 及 <see cref="DateTimeOffset"/> 方法调用。
        /// <para>SonnetDB 的 time 列是 Unix 毫秒整数，所有时间加减均通过整数运算实现：</para>
        /// <list type="table">
        ///   <listheader><term>C# 方法</term><description>SQL 翻译</description></listheader>
        ///   <item><term>AddMilliseconds(n)</term><description>time + n</description></item>
        ///   <item><term>AddSeconds(n)</term><description>time + (n * 1000)</description></item>
        ///   <item><term>AddMinutes(n)</term><description>time + (n * 60000)</description></item>
        ///   <item><term>AddHours(n)</term><description>time + (n * 3600000)</description></item>
        ///   <item><term>AddDays(n)</term><description>time + (n * 86400000)</description></item>
        ///   <item><term>AddTicks(n)</term><description>time + (n / 10000)（1 tick = 100ns = 0.0001ms）</description></item>
        /// </list>
        /// </summary>
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
                        // 尝试将常量字符串解析为 Unix 毫秒时间戳；若无法解析则透传原始 SQL。
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
                    case "AddSeconds":      return $"({left} + ({args1} * 1000))";
                    case "AddMinutes":      return $"({left} + ({args1} * 60000))";
                    case "AddHours":        return $"({left} + ({args1} * 3600000))";
                    case "AddDays":         return $"({left} + ({args1} * 86400000))";
                    case "AddTicks":        return $"({left} + ({args1} / 10000))";   // 1 tick = 100ns
                    case "Equals":          return $"({left} = {args1})";
                    case "CompareTo":       return $"({left} - {args1})";
                }
            }
            return null;
        }

        /// <summary>
        /// 翻译 <see cref="Convert"/> 静态方法调用。
        /// <para>SonnetDB 不支持 CAST，数值类型转换直接透传原列 SQL 即可。
        /// ToBoolean 使用 NOT IN ('0','false') 模拟。</para>
        /// </summary>
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
                        // SonnetDB 内部数值类型自动兼容，直接透传。
                        return getExp(exp.Arguments[0]);
                }
            }
            return null;
        }
    }
}
