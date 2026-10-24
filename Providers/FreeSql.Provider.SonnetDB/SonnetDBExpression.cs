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
//   • 测量的 time 列存储 Unix 毫秒；关系表 DATETIME 保留原生时间精度。
//   • TimeSpan 的列值、参数和运算统一使用 ticks，结果投影按需适配公共读取器。
//   • SQL CAST 映射 SonnetDB 的 INT、FLOAT、DECIMAL、STRING 和 BOOL 类型。

using FreeSql.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

namespace FreeSql.SonnetDB
{
    /// <summary>
    /// SonnetDB 专用 Lambda 表达式翻译器，将 C# 表达式翻译为 SonnetDB SQL 片段。
    /// </summary>
    class SonnetDBExpression : CommonExpression
    {
        public SonnetDBExpression(CommonUtils common) : base(common) { }

        static readonly AsyncLocal<DurationProjectionScope> _durationProjection = new AsyncLocal<DurationProjectionScope>();

        internal static IDisposable BeginDurationProjection(LambdaExpression selector) =>
            new DurationProjectionScope(selector?.Body);

        sealed class DurationProjectionScope : IDisposable
        {
            readonly DurationProjectionScope _previous;
            internal readonly HashSet<Expression> Leaves = new HashSet<Expression>();

            internal DurationProjectionScope(Expression expression)
            {
                _previous = _durationProjection.Value;
                Collect(expression);
                _durationProjection.Value = this;
            }

            void Collect(Expression expression)
            {
                if (expression == null) return;
                if (expression.Type.NullableTypeOrThis() == typeof(TimeSpan))
                {
                    Leaves.Add(expression);
                    // ReadAnonymousField removes conversions and folds constant
                    // conditionals before invoking the expression parser.
                    if (expression is UnaryExpression unary) Collect(unary.Operand);
                    if (expression is ConditionalExpression conditional && conditional.Test.CanDynamicInvoke())
                    {
                        Collect(conditional.IfTrue);
                        Collect(conditional.IfFalse);
                    }
                    return;
                }
                if (expression is NewExpression constructor)
                    foreach (var argument in constructor.Arguments) Collect(argument);
                else if (expression is MemberInitExpression initializer)
                {
                    foreach (var argument in initializer.NewExpression.Arguments) Collect(argument);
                    foreach (var assignment in initializer.Bindings.OfType<MemberAssignment>()) Collect(assignment.Expression);
                }
                else if (expression is UnaryExpression conversion &&
                    (conversion.NodeType == ExpressionType.Convert || conversion.NodeType == ExpressionType.ConvertChecked))
                    Collect(conversion.Operand);
            }

            public void Dispose() => _durationProjection.Value = _previous;
        }

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

        // TimeSpan 在 SQL 表达式中统一用 ticks 表示，与实体列和常量绑定一致。
        // 关系表和时序测量都按这个单位比较，避免引入数据库端时间类型。
        static string ToSqlTimeOfDay(string left)
        {
            return $"((date_part('hour', {left}) * 36000000000) + " +
                $"(date_part('minute', {left}) * 600000000) + " +
                $"(date_part('second', {left}) * 10000000) + " +
                $"(date_part('millisecond', {left}) * 10000) + " +
                $"(date_part('microsecond', {left}) * 10) + " +
                $"(date_part('nanosecond', {left}) / 100))";
        }

        static string ToSqlDateDifference(string memberName, string left, string right)
        {
            // date_diff 返回向零截断的 Int64 差值，与 TimeSpan 负数组件一致。
            // 先取 tick 再换算 Total*，保留关系表 DATETIME 的亚毫秒精度。
            var ticks = $"date_diff('tick', {right}, {left})";
            switch (memberName)
            {
                case "TotalDays": return $"({ticks} / 864000000000.0)";
                case "TotalHours": return $"({ticks} / 36000000000.0)";
                case "TotalMinutes": return $"({ticks} / 600000000.0)";
                case "TotalSeconds": return $"({ticks} / 10000000.0)";
                case "TotalMilliseconds": return $"({ticks} / 10000.0)";
                case "Days": return $"date_diff('day', {right}, {left})";
                case "Hours": return $"(date_diff('hour', {right}, {left}) % 24)";
                case "Minutes": return $"(date_diff('minute', {right}, {left}) % 60)";
                case "Seconds": return $"(date_diff('second', {right}, {left}) % 60)";
                case "Milliseconds": return $"(date_diff('millisecond', {right}, {left}) % 1000)";
                case "Ticks": return ticks;
                default: throw UnsupportedTimeSpanMember(memberName);
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
            var projection = _durationProjection.Value;
            if (projection?.Leaves.Contains(expression) == true)
            {
                // Only result leaves need invariant text for the existing core
                // reader. Every expression below the leaf keeps numeric ticks.
                _durationProjection.Value = null;
                try { result = SonnetDBUtils.FormatTimeSpanReadSql(parse(expression)); }
                finally { _durationProjection.Value = projection; }
                return true;
            }
            if (expression is MethodCallExpression vectorCall && vectorCall.Method.DeclaringType == typeof(SonnetDBFunctions))
            {
                string function = null;
                switch (vectorCall.Method.Name)
                {
                    case nameof(SonnetDBFunctions.CosineDistance): function = "cosine_distance"; break;
                    case nameof(SonnetDBFunctions.L2Distance): function = "l2_distance"; break;
                    case nameof(SonnetDBFunctions.InnerProduct): function = "inner_product"; break;
                    case nameof(SonnetDBFunctions.VectorNorm): function = "vector_norm"; break;
                }
                if (function != null)
                {
                    // The older ExpressionCall adapter attempts to convert SQL
                    // parameter names back to CLR arrays before calling a helper.
                    // Parse the arguments directly so native VECTOR bindings survive.
                    result = function + "(" + string.Join(", ", vectorCall.Arguments.Select(argument =>
                        SonnetDBFunctions.NormalizeVectorSql(argument, parse(argument)))) + ")";
                    return true;
                }
            }
            if (expression is UnaryExpression conversion &&
                (conversion.NodeType == ExpressionType.Convert || conversion.NodeType == ExpressionType.ConvertChecked))
            {
                var targetType = conversion.Type.NullableTypeOrThis();
                var sourceType = conversion.Operand.Type.NullableTypeOrThis();
                // 此版本公共解析器直接剥离 Convert，必须在提供程序 AOP 桥接中保留
                // 显式数值 CAST，才能在条件和嵌套算术内执行转换。
                if (targetType != sourceType && targetType.IsNumberType() && sourceType.IsNumberType())
                {
                    var sqlType = targetType == typeof(decimal) ? "DECIMAL" :
                        targetType == typeof(float) || targetType == typeof(double) ? "FLOAT" : "INT";
                    result = $"cast({parse(conversion.Operand)} as {sqlType})";
                    return true;
                }
            }
            if (expression is BinaryExpression shift &&
                (shift.NodeType == ExpressionType.LeftShift || shift.NodeType == ExpressionType.RightShift))
            {
                var type = shift.Type.NullableTypeOrThis();
                var countMask = type == typeof(long) || type == typeof(ulong) ? 63 : 31;
                var function = shift.NodeType == ExpressionType.LeftShift ? "shift_left" : "shift_right";
                var shifted = $"{function}({parse(shift.Left)}, ({parse(shift.Right)} & {countMask}))";
                // SQL 位移按 Int64 执行；CLR Int32 左移必须丢弃高 32 位并保留符号。
                result = type == typeof(int)
                    ? $"(({shifted} & 2147483647) - ({shifted} & 2147483648))"
                    : type == typeof(uint) ? $"({shifted} & 4294967295)" : shifted;
                return true;
            }
            if (expression is BinaryExpression dateDifference &&
                dateDifference.NodeType == ExpressionType.Subtract &&
                dateDifference.Type.NullableTypeOrThis() == typeof(TimeSpan) &&
                IsDateType(dateDifference.Left.Type.NullableTypeOrThis()) &&
                IsDateType(dateDifference.Right.Type.NullableTypeOrThis()))
            {
                result = $"date_diff('tick', {parse(dateDifference.Right)}, {parse(dateDifference.Left)})";
                return true;
            }
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

                if (member.Member.DeclaringType == typeof(TimeSpan))
                {
                    if (member.Expression == null && expression.CanDynamicInvoke())
                    {
                        var constant = Expression.Lambda(expression).Compile().DynamicInvoke();
                        result = constant is TimeSpan span
                            ? span.Ticks == long.MinValue ? "(-9223372036854775807 - 1)" :
                                span.Ticks.ToString(CultureInfo.InvariantCulture)
                            : Convert.ToString(constant, CultureInfo.InvariantCulture);
                    }
                    else result = ToSqlTimeSpanValueMember(member.Member.Name, parse(member.Expression));
                    return true;
                }
            }

            if (expression is MethodCallExpression call &&
                call.Method.DeclaringType == typeof(TimeSpan))
            {
                if (call.CanDynamicInvoke())
                {
                    var value = Expression.Lambda(call).Compile().DynamicInvoke();
                    if (value is TimeSpan timeSpan)
                    {
                        result = timeSpan.Ticks == long.MinValue ? "(-9223372036854775807 - 1)" :
                            timeSpan.Ticks.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                }
                if (call.Object == null && call.Arguments.Count == 1)
                {
                    result = ToSqlTimeSpanFactory(call.Method.Name, parse(call.Arguments[0]));
                    return true;
                }
                throw UnsupportedTimeSpanMethod(call.Method.Name);
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
                    // 显式数值转换必须在数据库执行，尤其在 WHERE/算术表达式中
                    // 不能依赖最终结果物化时的 CLR 类型转换。
                    var operandExp = (exp as UnaryExpression)?.Operand;
                    var gentype = exp.Type.NullableTypeOrThis();
                    if (operandExp != null && gentype != operandExp.Type.NullableTypeOrThis())
                    {
                        switch (gentype.ToString())
                        {
                            case "System.Boolean": return $"cast({getExp(operandExp)} as BOOL)";
                            case "System.String": return $"cast({getExp(operandExp)} as STRING)";
                            case "System.DateTime": return ExpressionConstDateTime(operandExp) ?? getExp(operandExp);
                            case "System.Decimal": return $"cast({getExp(operandExp)} as DECIMAL)";
                            case "System.Double":
                            case "System.Single": return $"cast({getExp(operandExp)} as FLOAT)";
                            case "System.Int16":
                            case "System.Int32":
                            case "System.Int64":
                            case "System.Byte":
                            case "System.SByte":
                            case "System.UInt16":
                            case "System.UInt32":
                            case "System.UInt64":
                                return $"cast({getExp(operandExp)} as INT)";
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
                return $"length({ExpressionLambdaToSql(exp.Expression, tsc)})";
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
                    case "MinValue": return "(-9223372036854775807 - 1)";
                    case "MaxValue": return "9223372036854775807";
                    case "TicksPerMillisecond": return "10000";
                    case "TicksPerSecond": return "10000000";
                    case "TicksPerMinute": return "600000000";
                    case "TicksPerHour": return "36000000000";
                    case "TicksPerDay": return "864000000000";
                }
                throw UnsupportedTimeSpanMember(exp.Member.Name);
            }

            return ToSqlTimeSpanValueMember(exp.Member.Name, getExp(exp.Expression));
        }

        static string ToSqlTimeSpanValueMember(string memberName, string ticks)
        {
            // DECIMAL 整数商避免 Int64 ticks 先转 Double 后在组件边界产生误差。
            string quotient(long divisor) => $"cast((cast({ticks} as DECIMAL) / {divisor}) as INT)";
            switch (memberName)
            {
                case "TotalDays": return $"({ticks} / 864000000000.0)";
                case "TotalHours": return $"({ticks} / 36000000000.0)";
                case "TotalMinutes": return $"({ticks} / 600000000.0)";
                case "TotalSeconds": return $"({ticks} / 10000000.0)";
                case "TotalMilliseconds": return $"({ticks} / 10000.0)";
                case "Ticks": return $"({ticks} + 0)";
                case "Days": return quotient(TimeSpan.TicksPerDay);
                case "Hours": return $"({quotient(TimeSpan.TicksPerHour)} % 24)";
                case "Minutes": return $"({quotient(TimeSpan.TicksPerMinute)} % 60)";
                case "Seconds": return $"({quotient(TimeSpan.TicksPerSecond)} % 60)";
                case "Milliseconds": return $"({quotient(TimeSpan.TicksPerMillisecond)} % 1000)";
                default: throw UnsupportedTimeSpanMember(memberName);
            }
        }

        static string ToSqlTimeSpanCall(MethodCallExpression exp, ExpTSC tsc,
            Func<Expression, string> getExp)
        {
            if (exp.Object != null || exp.Arguments.Count != 1)
                throw UnsupportedTimeSpanMethod(exp.Method.Name);

            return ToSqlTimeSpanFactory(exp.Method.Name, getExp(exp.Arguments[0]));
        }

        static string ToSqlTimeSpanFactory(string methodName, string argument)
        {
            switch (methodName)
            {
                case "FromDays": return $"cast(({argument} * 864000000000.0) as INT)";
                case "FromHours": return $"cast(({argument} * 36000000000.0) as INT)";
                case "FromMinutes": return $"cast(({argument} * 600000000.0) as INT)";
                case "FromSeconds": return $"cast(({argument} * 10000000.0) as INT)";
                case "FromMilliseconds": return $"cast(({argument} * 10000.0) as INT)";
                case "FromMicroseconds": return $"cast(({argument} * 10.0) as INT)";
                case "FromTicks": return argument;
                default: throw UnsupportedTimeSpanMethod(methodName);
            }
        }

        /// <summary>
        /// 翻译 <see cref="string"/> 实例方法调用。
        /// <para>支持：ToLower、ToUpper、Equals、
        /// StartsWith、EndsWith、Contains、Substring、IndexOf、Replace、Trim 系列、
        /// IsNullOrEmpty、IsNullOrWhiteSpace、Concat。带文化或 StringComparison 的重载
        /// 不能保持 .NET 语义时在翻译阶段明确拒绝。</para>
        /// </summary>
        public override string ExpressionLambdaToSqlCallString(MethodCallExpression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);

            // SonnetDB 的字符串函数使用 Ordinal 规则，没有 StringComparison 参数语义。
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
                    {
                        var function = exp.Method.Name == "Trim" ? "trim" :
                            exp.Method.Name == "TrimStart" ? "ltrim" : "rtrim";
                        if (exp.Arguments.Count == 0) return $"{function}({left})";
                        if (exp.Arguments.Count == 1)
                        {
                            var trimCharacters = ExpressionGetValue(exp.Arguments[0], out var success);
                            if (success)
                            {
                                var characters = trimCharacters is char[] array ? new string(array) :
                                    trimCharacters is char character ? character.ToString() : null;
                                // 空字符数组和 null 均采用 .NET 的空白裁剪规则。
                                if (string.IsNullOrEmpty(characters)) return $"{function}({left})";
                                return $"{function}({left}, {formatSql(characters, typeof(string), null, null)})";
                            }
                            if (exp.Arguments[0].Type == typeof(char))
                                return $"{function}({left}, {getExp(exp.Arguments[0])})";
                        }
                        throw UnsupportedStringMethod(exp.Method.Name, "动态字符数组重载");
                    }
                    case "Substring":
                        if (exp.Arguments.Count == 1)
                            return $"substring({left}, ({getExp(exp.Arguments[0])} + 1))";
                        if (exp.Arguments.Count == 2)
                            return $"substring({left}, ({getExp(exp.Arguments[0])} + 1), {getExp(exp.Arguments[1])})";
                        throw UnsupportedStringMethod(exp.Method.Name, "当前重载");
                    case "IndexOf":
                        if (exp.Arguments.Count == 1)
                            return $"(position({getExp(exp.Arguments[0])} IN {left}) - 1)";
                        throw UnsupportedStringMethod(exp.Method.Name, "起始位置或长度重载");
                    case "Replace":
                        if (exp.Arguments.Count == 2)
                            return $"replace({left}, {getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                        throw UnsupportedStringMethod(exp.Method.Name, "当前重载");
                    case "PadLeft":
                    case "PadRight":
                        var paddingFunction = exp.Method.Name == "PadLeft" ? "lpad" : "rpad";
                        if (exp.Arguments.Count == 1)
                            return $"{paddingFunction}({left}, {getExp(exp.Arguments[0])})";
                        if (exp.Arguments.Count == 2 && exp.Arguments[1].Type == typeof(char))
                            return $"{paddingFunction}({left}, {getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                        throw UnsupportedStringMethod(exp.Method.Name, "当前重载");
                    case "Equals":
                        if (exp.Arguments.Count != 1)
                            throw UnsupportedStringMethod(exp.Method.Name, "参数数量不是 1 的重载");
                        return $"({left} = {getExp(exp.Arguments[0])})";
                    case "StartsWith":
                    {
                        return BuildLikeExpression(left, exp, LikePatternKind.StartsWith, getExp);
                    }
                    case "EndsWith":
                    {
                        return BuildLikeExpression(left, exp, LikePatternKind.EndsWith, getExp);
                    }
                    case "Contains":
                    {
                        return BuildLikeExpression(left, exp, LikePatternKind.Contains, getExp);
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

        static string BuildLikeExpression(string left, MethodCallExpression exp, LikePatternKind kind,
            Func<Expression, string> getExp)
        {
            if (exp.Arguments.Count != 1)
                throw UnsupportedStringMethod(exp.Method.Name, "参数数量不是 1 的重载");

            // 常量继续使用已转义 LIKE；动态模式使用字符串谓词，避免把
            // 数据中的 %、_ 或反斜杠解释为模式字符。
            var value = ExpressionGetValue(exp.Arguments[0], out var success);
            if (!success)
            {
                var function = kind == LikePatternKind.StartsWith ? "starts_with" :
                    kind == LikePatternKind.EndsWith ? "ends_with" : "contains";
                return $"{function}({left}, {getExp(exp.Arguments[0])})";
            }
            if (value == null) throw UnsupportedStringMethod(exp.Method.Name, "null 模式参数");

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
                $"SonnetDB 不支持 string.{method} 的{detail}，无法保持 .NET 字符串比较语义；请在应用层处理。");
        }

        /// <summary>
        /// 翻译 <see cref="Math"/> 静态方法调用。
        /// <para>支持：Abs、Round（可选精度）、Sqrt、Log（可选底数）、
        /// Exp、Ceiling、Floor、Pow。</para>
        /// </summary>
        public override string ExpressionLambdaToSqlCallMath(MethodCallExpression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            if (exp.Arguments.Count > 0 && exp.Arguments[0].Type.NullableTypeOrThis() == typeof(decimal))
            {
                var value = getExp(exp.Arguments[0]);
                var remainder = $"({value} % 1)";
                var truncated = $"({value} - {remainder})";
                switch (exp.Method.Name)
                {
                    case "Abs": return $"(case when {value} < 0 then -({value}) else {value} end)";
                    case "Truncate": return truncated;
                    case "Floor": return $"({truncated} - (case when {remainder} < 0 then 1 else 0 end))";
                    case "Ceiling": return $"({truncated} + (case when {remainder} > 0 then 1 else 0 end))";
                    case "Sign": return $"(case when {value} < 0 then -1 when {value} > 0 then 1 else 0 end)";
                    case "Round":
                        if (exp.Arguments.Count == 1)
                            return $"round({value})";
                        if (exp.Arguments.Count == 2 && exp.Arguments[1].Type == typeof(int))
                        {
                            ValidateDecimalRoundDigits(exp.Arguments[1]);
                            return $"round({value}, {getExp(exp.Arguments[1])})";
                        }
                        throw UnsupportedMathMethod("Round", "MidpointRounding 或其他重载");
                }
            }
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
                case "Exp": return $"exp({getExp(exp.Arguments[0])})";
                case "Ceiling": return $"ceil({getExp(exp.Arguments[0])})";
                case "Floor": return $"floor({getExp(exp.Arguments[0])})";
                case "Pow": return $"power({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                case "Sin": return $"sin({getExp(exp.Arguments[0])})";
                case "Cos": return $"cos({getExp(exp.Arguments[0])})";
                case "Tan": return $"tan({getExp(exp.Arguments[0])})";
                case "Asin": return $"asin({getExp(exp.Arguments[0])})";
                case "Acos": return $"acos({getExp(exp.Arguments[0])})";
                case "Atan": return $"atan({getExp(exp.Arguments[0])})";
                case "Atan2": return $"atan2({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                case "Truncate":
                    var truncateValue = getExp(exp.Arguments[0]);
                    return $"(case when {truncateValue} < 0 then ceil({truncateValue}) else floor({truncateValue}) end)";
            }
            return null;
        }

        static void ValidateDecimalRoundDigits(Expression expression)
        {
            if (expression is ConstantExpression constant && constant.Value is int digits && (digits < 0 || digits > 28))
                throw new NotSupportedException("SonnetDB DECIMAL round 的 digits 必须在 0 到 28 之间。");
        }

        static NotSupportedException UnsupportedMathMethod(string method, string detail)
        {
            return new NotSupportedException(
                $"SonnetDB 不支持 Math.{method} 的{detail}，无法保持 .NET 数学语义；请在应用层处理。");
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
                    case "ToString":
                        if (exp.Arguments.Count == 1 && exp.Arguments[0].Type == typeof(string))
                            return $"date_format({left}, {args1})";
                        throw UnsupportedDateTimeMethod("ToString 当前重载");
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
                            return $"date_diff('tick', {args1}, {left})";
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
            return ToSqlDateDifference(memberName, left, right);
        }

        static NotSupportedException UnsupportedDateTimeMethod(string method)
        {
            return new NotSupportedException(
                $"SonnetDB 不支持 DateTime.{method} 的当前翻译，无法保证时间语义；请改用已支持的日期函数或在应用层处理。");
        }

        static NotSupportedException UnsupportedDateTimeMember(string member)
        {
            return new NotSupportedException(
                $"SonnetDB 不支持 DateTime.{member} 的当前翻译，无法保证时间语义；请改用已支持的日期成员或在应用层处理。");
        }

        static NotSupportedException UnsupportedTimeSpanMember(string member)
        {
            return new NotSupportedException(
                $"SonnetDB 不支持 TimeSpan.{member} 的当前翻译；请从 DateTime 或 DateTimeOffset 的差值访问该成员，或在应用层处理。");
        }

        static NotSupportedException UnsupportedTimeSpanMethod(string method)
        {
            return new NotSupportedException(
                $"SonnetDB 不支持 TimeSpan.{method} 的当前翻译，无法保证时间语义；请在应用层处理。");
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
            if (exp.Method.Name == "ToString" && exp.Arguments.Count == 1 &&
                exp.Arguments[0].Type == typeof(string))
                return $"date_format({left}, {getExp(exp.Arguments[0])})";
            if (exp.Method.Name == "Subtract")
            {
                if (exp.Arguments.Count == 1 && exp.Arguments[0].Type == typeof(DateTimeOffset))
                    return $"date_diff('tick', {getExp(exp.Arguments[0])}, {left})";
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
                $"SonnetDB 不支持 DateTimeOffset.{method} 的当前翻译，无法保证时间语义；请改用已支持的日期函数或在应用层处理。");
        }

        static NotSupportedException UnsupportedDateTimeOffsetMember(string member)
        {
            return new NotSupportedException(
                $"SonnetDB 不支持 DateTimeOffset.{member} 的当前翻译，无法保证时间语义；请改用已支持的日期成员或在应用层处理。");
        }

        /// <summary>
        /// 翻译 <see cref="Convert"/> 静态方法调用。
        /// <para>映射到 SonnetDB CAST；Convert 的浮点到整数转换先按
        /// .NET 默认的 ToEven 模式舍入，再执行整数转换。</para>
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
                    case "ToBoolean":
                        var booleanSource = getExp(exp.Arguments[0]);
                        return exp.Arguments[0].Type.NullableTypeOrThis().IsNumberType()
                            ? $"({booleanSource} <> 0)"
                            : $"coalesce(cast({booleanSource} as BOOL), false)";
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
                    case "ToString": return $"coalesce(cast({getExp(exp.Arguments[0])} as STRING), '')";
                    case "ToDecimal": return $"cast({getExp(exp.Arguments[0])} as DECIMAL)";
                    case "ToDouble":
                    case "ToSingle": return $"cast({getExp(exp.Arguments[0])} as FLOAT)";
                    case "ToByte":
                    case "ToInt16":
                    case "ToInt32":
                    case "ToInt64":
                    case "ToSByte":
                    case "ToUInt16":
                    case "ToUInt32":
                    case "ToUInt64":
                        var integerSource = getExp(exp.Arguments[0]);
                        var integerSourceType = exp.Arguments[0].Type.NullableTypeOrThis();
                        if (integerSourceType == typeof(decimal))
                            integerSource = RoundDecimalToInteger(integerSource);
                        else if (integerSourceType == typeof(float) || integerSourceType == typeof(double))
                            integerSource = $"round({integerSource})";
                        return $"cast({integerSource} as INT)";
                    case "ToChar": throw UnsupportedConvertMethod(exp.Method.Name, "字符转换");
                }
            }
            throw UnsupportedConvertMethod(exp.Method.Name, "当前重载");
        }

        static NotSupportedException UnsupportedConvertMethod(string method, string detail)
        {
            return new NotSupportedException(
                $"SonnetDB 不支持 Convert.{method} 的{detail}，无法保持 .NET 转换语义；请在应用层处理。");
        }

        static string RoundDecimalToInteger(string value)
        {
            // 余数和加减全部保留 DECIMAL；恰好半整数时按偶数方向取整。
            var remainder = $"({value} % 1)";
            var truncated = $"({value} - {remainder})";
            return $"(case when {remainder} >= 0.5 and ({remainder} > 0.5 or ({truncated} % 2) <> 0) " +
                $"then ({truncated} + 1) when {remainder} <= -0.5 and ({remainder} < -0.5 or ({truncated} % 2) <> 0) " +
                $"then ({truncated} - 1) else {truncated} end)";
        }
    }
}
