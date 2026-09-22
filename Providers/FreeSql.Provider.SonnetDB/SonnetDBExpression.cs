// SonnetDB 表达式翻译实现。
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
using System.Globalization;
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

        static bool IsRelationshipTable(ExpTSC tsc)
        {
            var table = tsc?.mapColumnTmp?.Table ?? tsc?.currentTable;
            if (table != null) return SonnetDBModel.IsTable(table);

            // 嵌套函数可能会清空当前列映射，单表查询仍可由根表判断模型类型。
            return tsc?._tables?.Count == 1 &&
                tsc._tables[0]?.Table != null &&
                SonnetDBModel.IsTable(tsc._tables[0].Table);
        }

        static string GetDatePart(string memberName)
        {
            switch (memberName)
            {
                case "DayOfYear": return "day_of_year";
                case "DayOfWeek": return "day_of_week";
                default: return memberName.ToLowerInvariant();
            }
        }

        static string GetDateAddPart(string methodName)
        {
            switch (methodName)
            {
                case "AddYears": return "year";
                case "AddMonths": return "month";
                case "AddDays": return "day";
                case "AddHours": return "hour";
                case "AddMinutes": return "minute";
                case "AddSeconds": return "second";
                case "AddMilliseconds": return "millisecond";
                case "AddMicroseconds": return "microsecond";
                case "AddTicks": return "tick";
                default: return null;
            }
        }

        // SonnetDB 的日期函数返回日期分量，TimeOfDay 统一换算为当天经过的毫秒数。
        // 关系表和时序测量都按这个单位比较，避免引入数据库端时间类型。
        static string ToSqlTimeOfDay(string left)
        {
            return $"((date_part('hour', {left}) * 3600000) + " +
                $"(date_part('minute', {left}) * 60000) + " +
                $"(date_part('second', {left}) * 1000) + " +
                $"date_part('millisecond', {left}))";
        }

        static string ToSqlDateDifference(string memberName, string left, string right)
        {
            var difference = $"(to_unix_milliseconds({left}) - to_unix_milliseconds({right}))";
            switch (memberName)
            {
                case "TotalDays": return $"({difference} / 86400000.0)";
                case "TotalHours": return $"({difference} / 3600000.0)";
                case "TotalMinutes": return $"({difference} / 60000.0)";
                case "TotalSeconds": return $"({difference} / 1000.0)";
                case "TotalMilliseconds": return difference;
                // SonnetDB 3.1 没有 floor/date_diff，无法保持负数和跨日
                // TimeSpan 组件（Days/Hours 等）的 .NET 语义，不能生成近似 SQL。
                case "Days":
                case "Hours":
                case "Minutes":
                case "Seconds":
                case "Milliseconds":
                case "Ticks":
                    throw UnsupportedTimeSpanMember(memberName);
                default:
                    throw UnsupportedTimeSpanMember(memberName);
            }
        }

        /// <summary>
        /// The older FreeSql core rejects TimeSpan method/member nodes before
        /// dispatching to <see cref="ExpressionLambdaToSqlOther"/>. Keep the
        /// compatibility bridge in the provider so the common expression
        /// parser does not need a SonnetDB-specific branch.
        /// </summary>
        internal static bool TryTranslateAopExpression(Expression expression,
            Func<Expression, string> parse, out string result)
        {
            result = null;
            if (expression is MemberExpression member &&
                (member.Member.DeclaringType == typeof(TimeSpan) || member.Member.Name == "TimeOfDay"))
            {
                if (member.Expression is BinaryExpression binary &&
                    binary.NodeType == ExpressionType.Subtract &&
                    binary.Type.NullableTypeOrThis() == typeof(TimeSpan))
                {
                    var leftType = binary.Left.Type.NullableTypeOrThis();
                    var rightType = binary.Right.Type.NullableTypeOrThis();
                    if (IsDateType(leftType) && IsDateType(rightType))
                    {
                        result = ToSqlDateDifference(member.Member.Name,
                            parse(binary.Left), parse(binary.Right));
                        return true;
                    }
                }

                if (member.Expression is MethodCallExpression subtract &&
                    subtract.Method.Name == "Subtract" &&
                    subtract.Arguments.Count == 1 &&
                    IsDateType(subtract.Object?.Type.NullableTypeOrThis()) &&
                    IsDateType(subtract.Arguments[0].Type.NullableTypeOrThis()))
                {
                    result = ToSqlDateDifference(member.Member.Name,
                        parse(subtract.Object), parse(subtract.Arguments[0]));
                    return true;
                }

                if (member.Member.Name == "TimeOfDay" && member.Expression != null)
                {
                    var sourceType = member.Expression.Type.NullableTypeOrThis();
                    if (sourceType == typeof(DateTime) || sourceType == typeof(DateTimeOffset))
                    {
                        result = ToSqlTimeOfDay(parse(member.Expression));
                        return true;
                    }
                }

                throw UnsupportedTimeSpanMember(member.Member.Name);
            }

            if (expression is MethodCallExpression call &&
                call.Method.DeclaringType == typeof(TimeSpan) &&
                call.Arguments.All(a => a.CanDynamicInvoke()))
            {
                var value = Expression.Lambda(call).Compile().DynamicInvoke();
                if (value is TimeSpan timeSpan)
                {
                    result = timeSpan.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
                    return true;
                }
            }

            return false;
        }

        static bool IsDateType(Type type) => type == typeof(DateTime) || type == typeof(DateTimeOffset);

        /// <summary>
        /// 处理其他类型表达式节点（Convert 类型转换、Contains IN 展开、数组/列表字面量）。
        /// </summary>
        public override string ExpressionLambdaToSqlOther(Expression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            switch (exp.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExp = exp as MemberExpression;
                    if (memberExp?.Member.DeclaringType == typeof(TimeSpan))
                    {
                        // DateTime/DateTimeOffset 二元减法同样返回 TimeSpan；公共解析器
                        // 无法仅凭类型区分两者，因此在这里统一按 Unix 毫秒计算差值。
                        if (memberExp.Expression is BinaryExpression dateSubtract &&
                            dateSubtract.NodeType == ExpressionType.Subtract &&
                            dateSubtract.Type.NullableTypeOrThis() == typeof(TimeSpan))
                        {
                            var leftType = dateSubtract.Left.Type.NullableTypeOrThis();
                            var rightType = dateSubtract.Right.Type.NullableTypeOrThis();
                            if ((leftType == typeof(DateTime) || leftType == typeof(DateTimeOffset)) &&
                                (rightType == typeof(DateTime) || rightType == typeof(DateTimeOffset)))
                                return ToSqlDateDifference(memberExp.Member.Name,
                                    getExp(dateSubtract.Left), getExp(dateSubtract.Right));
                        }

                        // DateTimeOffset.Subtract 不会经过 CommonExpression 的
                        // DateTime 专用分支，这里补上其 Total* 差值翻译。
                        if (memberExp.Expression is MethodCallExpression offsetSubtract &&
                            offsetSubtract.Method.Name == "Subtract" &&
                            (offsetSubtract.Method.DeclaringType == typeof(DateTimeOffset) ||
                             offsetSubtract.Object?.Type.NullableTypeOrThis() == typeof(DateTimeOffset)) &&
                            offsetSubtract.Arguments.Count == 1 &&
                            offsetSubtract.Arguments[0].Type.NullableTypeOrThis() == typeof(DateTimeOffset))
                        {
                            var offsetLeft = getExp(offsetSubtract.Object);
                            var offsetRight = getExp(offsetSubtract.Arguments[0]);
                            return ToSqlDateDifference(memberExp.Member.Name, offsetLeft, offsetRight);
                        }

                        return ToSqlTimeSpanMember(memberExp, tsc, getExp);
                    }
                    if (memberExp?.Member.DeclaringType == typeof(DateTimeOffset))
                        return ToSqlDateTimeOffsetMember(memberExp, tsc, getExp);
                    break;
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
                    if (callExp?.Method.DeclaringType == typeof(DateTimeOffset))
                        return ToSqlDateTimeOffsetCall(callExp, tsc, getExp);
                    if (callExp?.Method.DeclaringType == typeof(TimeSpan))
                        return ToSqlTimeSpanCall(callExp, tsc, getExp);
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
                    if (newExp.Type == typeof(global::SonnetDB.Model.GeoPoint) && newExp.Arguments.Count == 2)
                    {
                        return $"POINT({getExp(newExp.Arguments[0])}, {getExp(newExp.Arguments[1])})";
                    }
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
            if (string.Equals(exp.Member.Name, "Length", StringComparison.Ordinal))
                throw new NotSupportedException(
                    "SonnetDB 3.1 未注册 string.Length 所需的 length 函数；请在应用层计算字符串长度，或等待 SonnetDB 补齐该函数。");
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
                if (IsRelationshipTable(tsc))
                {
                    switch (exp.Member.Name)
                    {
                        case "Now": return "current_datetime()";
                        case "UtcNow": return "current_utc_datetime()";
                        case "Today": return "date_only(current_datetime())";
                        case "MinValue":
                        case "MaxValue":
                            return formatSql(
                                exp.Member.Name == "MinValue" ? DateTime.MinValue : DateTime.MaxValue,
                                typeof(DateTime), tsc.mapColumnTmp, tsc.dbParams);
                    }
                }
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

            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Date": return $"date_only({left})";
                case "TimeOfDay": return ToSqlTimeOfDay(left);
                case "DayOfWeek":
                case "DayOfYear":
                case "Day":
                case "Month":
                case "Year":
                case "Hour":
                case "Minute":
                case "Second":
                case "Millisecond":
                    return $"date_part('{GetDatePart(exp.Member.Name)}', {left})";
            }
            throw UnsupportedDateTimeMember(exp.Member.Name);
        }

        static string ToSqlTimeSpanMember(MemberExpression exp, ExpTSC tsc,
            Func<Expression, string> getExp)
        {
            if (exp.Expression == null)
            {
                switch (exp.Member.Name)
                {
                    case "Zero": return "0";
                    case "MinValue": return "(-9223372036854775807 / 10000.0)";
                    case "MaxValue": return "(9223372036854775807 / 10000.0)";
                    case "TicksPerMillisecond": return "10000";
                    case "TicksPerSecond": return "10000000";
                    case "TicksPerMinute": return "600000000";
                    case "TicksPerHour": return "36000000000";
                    case "TicksPerDay": return "864000000000";
                }
                throw UnsupportedTimeSpanMember(exp.Member.Name);
            }

            var left = getExp(exp.Expression);
            switch (exp.Member.Name)
            {
                case "TotalDays": return $"({left} / 86400000.0)";
                case "TotalHours": return $"({left} / 3600000.0)";
                case "TotalMinutes": return $"({left} / 60000.0)";
                case "TotalSeconds": return $"({left} / 1000.0)";
                case "TotalMilliseconds": return left;
                case "Ticks": return $"({left} * 10000)";
                case "Days":
                case "Hours":
                case "Minutes":
                case "Seconds":
                case "Milliseconds":
                    throw UnsupportedTimeSpanMember(exp.Member.Name);
                default:
                    throw UnsupportedTimeSpanMember(exp.Member.Name);
            }
        }

        static string ToSqlTimeSpanCall(MethodCallExpression exp, ExpTSC tsc,
            Func<Expression, string> getExp)
        {
            if (exp.Object != null || exp.Arguments.Count != 1)
                throw UnsupportedTimeSpanMethod(exp.Method.Name);

            var argument = getExp(exp.Arguments[0]);
            switch (exp.Method.Name)
            {
                case "FromDays": return $"({argument} * 86400000.0)";
                case "FromHours": return $"({argument} * 3600000.0)";
                case "FromMinutes": return $"({argument} * 60000.0)";
                case "FromSeconds": return $"({argument} * 1000.0)";
                case "FromMilliseconds": return argument;
                case "FromMicroseconds": return $"({argument} / 1000.0)";
                case "FromTicks": return $"({argument} / 10000.0)";
                default: throw UnsupportedTimeSpanMethod(exp.Method.Name);
            }
        }

        /// <summary>
        /// 翻译 <see cref="string"/> 实例方法调用。
        /// <para>支持：ToLower、ToUpper、Equals、
        /// StartsWith、EndsWith、Contains（常量模式翻译为已转义的 LIKE 表达式）、
        /// IsNullOrEmpty、IsNullOrWhiteSpace、Concat（→ concat(...)）。SonnetDB 3.1 的方法名。
        /// 没有 trim/ltrim/rtrim，Trim 系列会在翻译阶段明确拒绝；
        /// StringComparison 和无法静态取得的动态 LIKE 模式也会明确拒绝。</para>
        /// </summary>
        public override string ExpressionLambdaToSqlCallString(MethodCallExpression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);

            // SonnetDB 3.1 只有大小写敏感的字符串比较，没有 StringComparison 参数语义。
            // 不能忽略该参数，否则 OrdinalIgnoreCase 等调用会被错误翻译成大小写敏感比较。
            if (exp.Method.GetParameters().Any(a => a.ParameterType == typeof(StringComparison)))
                throw UnsupportedStringMethod(exp.Method.Name, "StringComparison 重载");

            if (exp.Object == null)
            {
                switch (exp.Method.Name)
                {
                    case "Equals":
                        if (exp.Arguments.Count != 2)
                            throw UnsupportedStringMethod(exp.Method.Name, "参数数量不是 2 的重载");
                        return $"({getExp(exp.Arguments[0])} = {getExp(exp.Arguments[1])})";
                    case "IsNullOrEmpty":
                        var arg1 = getExp(exp.Arguments[0]);
                        return $"({arg1} is null or {arg1} = '')";
                    case "IsNullOrWhiteSpace":
                        var arg2 = getExp(exp.Arguments[0]);
                        // 用已注册的 regexp_like 表达空白判断，避免生成 SonnetDB
                        // 3.1 不存在的 trim 函数。
                        return $"({arg2} is null or {arg2} = '' or regexp_like({arg2}, '^\\s*$'))";
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
                    case "Trim":
                    case "TrimStart":
                    case "TrimEnd":
                        throw UnsupportedScalarFunction(exp.Method.Name, "trim/ltrim/rtrim");
                    case "Equals":
                        if (exp.Arguments.Count != 1)
                            throw UnsupportedStringMethod(exp.Method.Name, "参数数量不是 1 的重载");
                        return $"({left} = {getExp(exp.Arguments[0])})";
                    case "StartsWith":
                    {
                        return BuildLikeExpression(left, exp, LikePatternKind.StartsWith);
                    }
                    case "EndsWith":
                    {
                        return BuildLikeExpression(left, exp, LikePatternKind.EndsWith);
                    }
                    case "Contains":
                    {
                        return BuildLikeExpression(left, exp, LikePatternKind.Contains);
                    }
                }
            }
            return null;
        }

        enum LikePatternKind
        {
            StartsWith,
            EndsWith,
            Contains
        }

        static string BuildLikeExpression(string left, MethodCallExpression exp, LikePatternKind kind)
        {
            if (exp.Arguments.Count != 1)
                throw UnsupportedStringMethod(exp.Method.Name, "参数数量不是 1 的重载");

            // SonnetDB 的 LIKE 匹配器把反斜杠作为转义字符，但 3.1 没有
            // replace/ESCAPE 语法可用于运行时参数；动态模式若直接拼接会把
            // 参数中的 % 或 _ 错当成通配符，因此必须在翻译阶段明确拒绝。
            var value = ExpressionGetValue(exp.Arguments[0], out var success);
            if (!success || value == null)
                throw new NotSupportedException(
                    $"SonnetDB 3.1 无法安全翻译 string.{exp.Method.Name} 的动态模式；" +
                    "LIKE 模式中的 %、_ 和反斜杠需要在应用层先转义。");

            var escaped = EscapeLikeLiteral(Convert.ToString(value, CultureInfo.InvariantCulture));
            var pattern = kind switch
            {
                LikePatternKind.StartsWith => $"'{escaped}%'",
                LikePatternKind.EndsWith => $"'%{escaped}'",
                _ => $"'%{escaped}%'"
            };
            return $"({left} like {pattern})";
        }

        static string EscapeLikeLiteral(string value)
        {
            if (value == null) return string.Empty;
            // 先转义反斜杠，再转义通配符，最后转义 SQL 单引号。
            return value.Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("%", "\\%", StringComparison.Ordinal)
                .Replace("_", "\\_", StringComparison.Ordinal)
                .Replace("'", "''", StringComparison.Ordinal);
        }

        static NotSupportedException UnsupportedStringMethod(string method, string detail)
        {
            return new NotSupportedException(
                $"SonnetDB 3.1 不支持 string.{method} 的{detail}，无法保持 .NET 字符串比较语义；请在应用层处理。");
        }

        /// <summary>
        /// 翻译 <see cref="Math"/> 静态方法调用。
        /// <para>支持：Abs、Round（可选精度）、Sqrt、Log（可选底数）。SonnetDB 3.1
        /// 未注册 Exp、Ceiling、Floor、Pow，调用时会在翻译阶段明确拒绝。</para>
        /// </summary>
        public override string ExpressionLambdaToSqlCallMath(MethodCallExpression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            switch (exp.Method.Name)
            {
                case "Abs": return $"abs({getExp(exp.Arguments[0])})";
                case "Round":
                    // SonnetDB round 只有 (value) 和 (value, digits) 两种重载，
                    // 不接受 MidpointRounding；忽略舍入模式会悄悄改变 .NET 结果。
                    if (exp.Arguments.Count == 1)
                        return $"round({getExp(exp.Arguments[0])})";
                    if (exp.Arguments.Count == 2 && exp.Arguments[1].Type == typeof(int))
                        return $"round({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                    throw UnsupportedMathMethod("Round", "MidpointRounding 或其他重载");
                case "Sqrt": return $"sqrt({getExp(exp.Arguments[0])})";
                case "Log":
                    // Math.Log(x, base) → log(x, base)；单参数版 → log(x)（自然对数）。
                    if (exp.Arguments.Count > 1) return $"log({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                    return $"log({getExp(exp.Arguments[0])})";
                case "Exp":
                case "Ceiling":
                case "Floor":
                case "Pow":
                    throw UnsupportedScalarFunction(exp.Method.Name, "exp/ceil/floor/power");
            }
            return null;
        }

        static NotSupportedException UnsupportedMathMethod(string method, string detail)
        {
            return new NotSupportedException(
                $"SonnetDB 3.1 不支持 Math.{method} 的{detail}，无法保持 .NET 数学语义；请在应用层处理。");
        }

        static NotSupportedException UnsupportedScalarFunction(string method, string functions)
        {
            return new NotSupportedException(
                $"SonnetDB 3.1 不支持 {functions} 标量函数，无法翻译 Math.{method} 或字符串 Trim；请改用已支持函数或在应用层处理。");
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
        ///   <item><term>AddYears/AddMonths/AddMicroseconds</term><description>通过 date_add_datetime 转换日期后再映射回 time</description></item>
        ///   <item><term>AddTicks(n)</term><description>time + (n / 10000)（1 个时钟刻度 tick = 100ns = 0.0001ms）</description></item>
        ///   <item><term>Subtract(DateTime)</term><description>转换为 Unix 毫秒差，再按秒提供 TimeSpan 属性</description></item>
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
                        if (exp.Arguments.Count != 1)
                            throw UnsupportedDateTimeMethod("Parse 多参数重载");
                        return TryGetDateTimeLiteral(exp.Arguments[0]) ??
                            throw UnsupportedDateTimeMethod("Parse 动态字符串参数");
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact":
                        throw UnsupportedDateTimeMethod(exp.Method.Name);
                    case "Compare":
                    case "DaysInMonth":
                    case "IsLeapYear":
                        throw UnsupportedDateTimeMethod(exp.Method.Name);
                }
            }
            else
            {
                if (exp.Method.Name == "Subtract" &&
                    (exp.Arguments.Count != 1 || exp.Arguments[0].Type != typeof(DateTime)))
                    throw UnsupportedDateTimeMethod(exp.Method.Name);

                var left = getExp(exp.Object);
                var relationshipTable = IsRelationshipTable(tsc);
                var args1 = exp.Arguments.Count == 0 ? null : getExp(exp.Arguments[0]);
                var dateAddPart = GetDateAddPart(exp.Method.Name);
                if (relationshipTable && dateAddPart != null)
                    return $"date_add_datetime({left}, {args1}, '{dateAddPart}')";

                // 时序测量的 time 列以 Unix 毫秒整数存储。年/月/微秒无法用
                // 固定毫秒常量换算，借助 3.1 的日期函数完成转换后再转回整数。
                if (!relationshipTable && (dateAddPart == "year" || dateAddPart == "month" || dateAddPart == "microsecond"))
                    return $"to_unix_milliseconds(date_add_datetime({left}, {args1}, '{dateAddPart}'))";
                switch (exp.Method.Name)
                {
                    case "AddMilliseconds": return $"({left} + {args1})";
                    case "AddSeconds":      return $"({left} + ({args1} * 1000))";
                    case "AddMinutes":      return $"({left} + ({args1} * 60000))";
                    case "AddHours":        return $"({left} + ({args1} * 3600000))";
                    case "AddDays":         return $"({left} + ({args1} * 86400000))";
                    case "AddTicks":        return $"({left} + ({args1} / 10000))";   // 1 个时钟刻度 = 100ns
                    case "Equals":          return $"({left} = {args1})";
                    case "CompareTo":
                        return relationshipTable
                            ? $"(to_unix_milliseconds({left}) - to_unix_milliseconds({args1}))"
                            : $"({left} - {args1})";
                    case "Subtract":
                        if (exp.Arguments.Count == 1 && exp.Arguments[0].Type == typeof(DateTime))
                            return relationshipTable
                                ? $"(to_unix_milliseconds({left}) - to_unix_milliseconds({args1}))"
                                : $"({left} - {args1})";
                        throw UnsupportedDateTimeMethod(exp.Method.Name);
                    case "ToUniversalTime":
                        return relationshipTable
                            ? $"to_utc_datetime({left})"
                            : $"to_unix_milliseconds(to_utc_datetime({left}))";
                    case "ToLocalTime":
                        return relationshipTable
                            ? $"to_local_datetime({left})"
                            : $"to_unix_milliseconds(to_local_datetime({left}))";
                }
            }
            throw UnsupportedDateTimeMethod(exp.Method.Name);
        }

        // 仅把编译期常量或纯闭包成员解析为时间字面量；含查询参数的表达式
        // 不能透传给 SonnetDB，因为 3.1 的日期函数不接受字符串参数。
        string TryGetDateTimeLiteral(Expression expression)
        {
            try
            {
                var literal = ExpressionConstDateTime(expression);
                if (literal != null) return literal;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (InvalidCastException)
            {
                return null;
            }

            if (!IsCapturedValueExpression(expression)) return null;

            object value;
            try
            {
                value = ExpressionGetValue(expression, out var success);
                if (!success || value == null) return null;
            }
            catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
            {
                return null;
            }

            try
            {
                var dateTime = value switch
                {
                    DateTime dt => dt,
                    DateTimeOffset dto => dto.DateTime,
                    _ => Convert.ToDateTime(value, CultureInfo.CurrentCulture)
                };
                return formatSql(dateTime, typeof(DateTime), null, null);
            }
            catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
            {
                return null;
            }
        }

        static bool IsCapturedValueExpression(Expression expression)
        {
            while (expression is UnaryExpression unary &&
                (unary.NodeType == ExpressionType.Convert ||
                 unary.NodeType == ExpressionType.ConvertChecked ||
                 unary.NodeType == ExpressionType.TypeAs))
                expression = unary.Operand;

            while (expression is MemberExpression member)
            {
                expression = member.Expression;
                if (expression == null) return false;
            }
            return expression is ConstantExpression;
        }

        public override string ExpressionLambdaToSqlCallDateDiff(string memberName, Expression date1, Expression date2, ExpTSC tsc)
        {
            var left = ExpressionLambdaToSql(date1, tsc);
            var right = ExpressionLambdaToSql(date2, tsc);
            var difference = IsRelationshipTable(tsc)
                ? $"(to_unix_milliseconds({left}) - to_unix_milliseconds({right}))"
                : $"({left} - {right})";
            switch (memberName)
            {
                case "TotalDays": return $"({difference} / 86400000.0)";
                case "TotalHours": return $"({difference} / 3600000.0)";
                case "TotalMinutes": return $"({difference} / 60000.0)";
                case "TotalSeconds": return $"({difference} / 1000.0)";
                case "TotalMilliseconds": return difference;
                default: throw UnsupportedTimeSpanMember(memberName);
            }
        }

        static NotSupportedException UnsupportedDateTimeMethod(string method)
        {
            return new NotSupportedException(
                $"SonnetDB 3.1 不支持 DateTime.{method} 的当前翻译，无法保证时间语义；请改用已支持的日期函数或在应用层处理。");
        }

        static NotSupportedException UnsupportedDateTimeMember(string member)
        {
            return new NotSupportedException(
                $"SonnetDB 3.1 不支持 DateTime.{member} 的当前翻译，无法保证时间语义；请改用已支持的日期成员或在应用层处理。");
        }

        static NotSupportedException UnsupportedTimeSpanMember(string member)
        {
            return new NotSupportedException(
                $"SonnetDB 3.1 不支持 TimeSpan.{member} 的当前翻译；数据库缺少可保持 .NET 组件语义的日期差函数，请改用 TotalDays、TotalHours、TotalMinutes、TotalSeconds 或 TotalMilliseconds。");
        }

        static NotSupportedException UnsupportedTimeSpanMethod(string method)
        {
            return new NotSupportedException(
                $"SonnetDB 3.1 不支持 TimeSpan.{method} 的当前翻译，无法保证时间语义；请在应用层处理。");
        }

        static string ToSqlDateTimeOffsetMember(MemberExpression exp, ExpTSC tsc,
            Func<Expression, string> getExp)
        {
            if (exp.Expression == null)
            {
                switch (exp.Member.Name)
                {
                    case "Now": return "current_datetime_offset()";
                    case "UtcNow": return "current_utc_datetime_offset()";
                }
                throw UnsupportedDateTimeOffsetMember(exp.Member.Name);
            }

            var left = getExp(exp.Expression);
            switch (exp.Member.Name)
            {
                case "Date": return $"date_only({left})";
                case "TimeOfDay": return ToSqlTimeOfDay(left);
                case "DayOfWeek":
                case "DayOfYear":
                case "Day":
                case "Month":
                case "Year":
                case "Hour":
                case "Minute":
                case "Second":
                case "Millisecond":
                    return $"date_part('{GetDatePart(exp.Member.Name)}', {left})";
                case "DateTime": return $"to_datetime({left})";
                case "UtcDateTime": return $"to_utc_datetime({left})";
                case "LocalDateTime": return $"to_local_datetime({left})";
            }
            throw UnsupportedDateTimeOffsetMember(exp.Member.Name);
        }

        static string ToSqlDateTimeOffsetCall(MethodCallExpression exp, ExpTSC tsc,
            Func<Expression, string> getExp)
        {
            if (exp.Object == null)
                throw UnsupportedDateTimeOffsetMethod(exp.Method.Name);

            var left = getExp(exp.Object);
            if (exp.Method.Name == "Subtract")
            {
                if (exp.Arguments.Count == 1 && exp.Arguments[0].Type == typeof(DateTimeOffset))
                    return $"(to_unix_milliseconds({left}) - to_unix_milliseconds({getExp(exp.Arguments[0])}))";
                throw UnsupportedDateTimeOffsetMethod(exp.Method.Name);
            }
            var dateAddPart = GetDateAddPart(exp.Method.Name);
            if (dateAddPart != null && exp.Arguments.Count == 1)
                return $"date_add_datetime_offset({left}, {getExp(exp.Arguments[0])}, '{dateAddPart}')";

            if (exp.Arguments.Count == 0)
            {
                if (exp.Method.Name == "ToUnixTimeMilliseconds") return $"to_unix_milliseconds({left})";
                if (exp.Method.Name == "ToUnixTimeSeconds") return $"to_unix_seconds({left})";
            }
            throw UnsupportedDateTimeOffsetMethod(exp.Method.Name);
        }

        static NotSupportedException UnsupportedDateTimeOffsetMethod(string method)
        {
            return new NotSupportedException(
                $"SonnetDB 3.1 不支持 DateTimeOffset.{method} 的当前翻译，无法保证时间语义；请改用已支持的日期函数或在应用层处理。");
        }

        static NotSupportedException UnsupportedDateTimeOffsetMember(string member)
        {
            return new NotSupportedException(
                $"SonnetDB 3.1 不支持 DateTimeOffset.{member} 的当前翻译，无法保证时间语义；请改用已支持的日期成员或在应用层处理。");
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
                if (exp.Arguments.Count != 1)
                    throw UnsupportedConvertMethod(exp.Method.Name, "带 IFormatProvider 等额外参数的重载");

                switch (exp.Method.Name)
                {
                    case "ToBoolean": return $"({getExp(exp.Arguments[0])} not in ('0','false'))";
                    case "ToDateTime":
                    {
                        var literal = TryGetDateTimeLiteral(exp.Arguments[0]);
                        if (literal != null) return literal;
                        var sourceType = exp.Arguments[0].Type.NullableTypeOrThis();
                        if (sourceType == typeof(DateTime) || sourceType == typeof(DateTimeOffset) ||
                            sourceType.IsNumberType())
                            return getExp(exp.Arguments[0]);
                        throw UnsupportedConvertMethod("ToDateTime", "动态字符串参数");
                    }
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
            throw UnsupportedConvertMethod(exp.Method.Name, "当前重载");
        }

        static NotSupportedException UnsupportedConvertMethod(string method, string detail)
        {
            return new NotSupportedException(
                $"SonnetDB 3.1 不支持 Convert.{method} 的{detail}，无法保持 .NET 转换语义；请在应用层处理。");
        }
    }
}
