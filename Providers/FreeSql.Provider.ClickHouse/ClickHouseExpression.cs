using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.ClickHouse
{
    class ClickHouseExpression : CommonExpression
    {

        public ClickHouseExpression(CommonUtils common) : base(common) { }

        public override string ExpressionLambdaToSqlOther(Expression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            switch (exp.NodeType)
            {
                case ExpressionType.ArrayLength:
                    var arrOper = (exp as UnaryExpression)?.Operand;
                    if (arrOper.Type == typeof(byte[])) return $"length({getExp(arrOper)})";
                    break;
                case ExpressionType.Convert:
                    var operandExp = (exp as UnaryExpression)?.Operand;
                    var gentype = exp.Type.NullableTypeOrThis();
                    if (gentype != operandExp.Type.NullableTypeOrThis())
                    {
                        switch (gentype.ToString())
                        {
                            case "System.Boolean": return $"({getExp(operandExp)} not in ('0','false'))";
                            case "System.Byte": return $"cast({getExp(operandExp)} as Int8)";
                            case "System.Char": return $"substr(cast({getExp(operandExp)} as String), 1, 1)";
                            case "System.DateTime": return ExpressionConstDateTime(operandExp) ?? $"cast({getExp(operandExp)} as DateTime)";
                            case "System.Decimal": return $"cast({getExp(operandExp)} as Decimal128(19))";
                            case "System.Double": return $"cast({getExp(operandExp)} as Float64)";
                            case "System.Int16": return $"cast({getExp(operandExp)} as Int16)";
                            case "System.Int32": return $"cast({getExp(operandExp)} as Int32)";
                            case "System.Int64": return $"cast({getExp(operandExp)} as Int64)";
                            case "System.SByte": return $"cast({getExp(operandExp)} as UInt8)";
                            case "System.Single": return $"cast({getExp(operandExp)} as Float32)";
                            case "System.String": return $"cast({getExp(operandExp)} as String)";
                            case "System.UInt16": return $"cast({getExp(operandExp)} as UInt16)";
                            case "System.UInt32": return $"cast({getExp(operandExp)} as UInt32)";
                            case "System.UInt64": return $"cast({getExp(operandExp)} as UInt64)";
                            case "System.Guid": return $"substr(cast({getExp(operandExp)} as String), 1, 36)";
                        }
                    }
                    break;
                case ExpressionType.Call:
                    var callExp = exp as MethodCallExpression;

                    switch (callExp.Method.Name)
                    {
                        case "Parse":
                        case "TryParse":
                            switch (callExp.Method.DeclaringType.NullableTypeOrThis().ToString())
                            {
                                case "System.Boolean": return $"({getExp(callExp.Arguments[0])} not in ('0','false'))";
                                case "System.Byte": return $"cast({getExp(callExp.Arguments[0])} as Int8)";
                                case "System.Char": return $"substr(cast({getExp(callExp.Arguments[0])} as String), 1, 1)";
                                case "System.DateTime": return ExpressionConstDateTime(callExp.Arguments[0]) ?? $"cast({getExp(callExp.Arguments[0])} as DateTime)";
                                case "System.Decimal": return $"cast({getExp(callExp.Arguments[0])} as Decimal128(19))";
                                case "System.Double": return $"cast({getExp(callExp.Arguments[0])} as Float64)";
                                case "System.Int16": return $"cast({getExp(callExp.Arguments[0])} as Int16)";
                                case "System.Int32": return $"cast({getExp(callExp.Arguments[0])} as Int32)";
                                case "System.Int64": return $"cast({getExp(callExp.Arguments[0])} as Int64)";
                                case "System.SByte": return $"cast({getExp(callExp.Arguments[0])} as UInt8)";
                                case "System.Single": return $"cast({getExp(callExp.Arguments[0])} as Float32)";
                                case "System.UInt16": return $"cast({getExp(callExp.Arguments[0])} as UInt16)";
                                case "System.UInt32": return $"cast({getExp(callExp.Arguments[0])} as UInt32)";
                                case "System.UInt64": return $"cast({getExp(callExp.Arguments[0])} as UInt64)";
                                case "System.Guid": return $"substr(cast({getExp(callExp.Arguments[0])} as String), 1, 36)";
                            }
                            return null;
                        case "NewGuid":
                            return null;
                        case "Next":
                            if (callExp.Object?.Type == typeof(Random)) return "cast(rand()*1000000000 as Int64)";
                            return null;
                        case "NextDouble":
                            if (callExp.Object?.Type == typeof(Random)) return "rand()";
                            return null;
                        case "Random":
                            if (callExp.Method.DeclaringType.IsNumberType()) return "rand()";
                            return null;
                        case "ToString":
                            if (callExp.Object != null) return callExp.Arguments.Count == 0 ? $"cast({getExp(callExp.Object)} as String)" : null;
                            return null;
                    }

                    var objExp = callExp.Object;
                    var objType = objExp?.Type;
                    if (objType?.FullName == "System.Byte[]") return null;

                    var argIndex = 0;
                    if (objType == null && callExp.Method.DeclaringType == typeof(Enumerable))
                    {
                        objExp = callExp.Arguments.FirstOrDefault();
                        objType = objExp?.Type;
                        argIndex++;

                        if (objType == typeof(string))
                        {
                            switch (callExp.Method.Name)
                            {
                                case "First":
                                case "FirstOrDefault":
                                    return $"substr({getExp(callExp.Arguments[0])}, 1, 1)";
                            }
                        }
                    }
                    if (objType == null) objType = callExp.Method.DeclaringType;
                    if (objType != null || objType.IsArrayOrList())
                    {
                        if (argIndex >= callExp.Arguments.Count) break;
                        tsc.SetMapColumnTmp(null);
                        var args1 = getExp(callExp.Arguments[argIndex]);
                        var oldMapType = tsc.SetMapTypeReturnOld(tsc.mapTypeTmp);
                        var oldDbParams = objExp?.NodeType == ExpressionType.MemberAccess ? tsc.SetDbParamsReturnOld(null) : null; //#900 UseGenerateCommandParameterWithLambda(true) 子查询 bug、以及 #1173 参数化 bug
                        tsc.isNotSetMapColumnTmp = true;
                        var left = objExp == null ? null : getExp(objExp);
                        tsc.isNotSetMapColumnTmp = false;
                        tsc.SetMapColumnTmp(null).SetMapTypeReturnOld(oldMapType);
                        if (oldDbParams != null) tsc.SetDbParamsReturnOld(oldDbParams);
                        switch (callExp.Method.Name)
                        {
                            case "Contains":
                                //判断 in //在各大 Provider AdoProvider 中已约定，500元素分割, 3空格\r\n4空格
                                return $"(({args1}) in {left.Replace(",   \r\n    \r\n", $") \r\n OR ({args1}) in (")})";
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
                        if (a % 500 == 499) arrSb.Append("   \r\n    \r\n"); //500元素分割, 3空格\r\n4空格
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
                        if (typeof(IEnumerable).IsAssignableFrom(newExp.Arguments[0].Type) == false) return "(NULL)";
                        return getExp(newExp.Arguments[0]);
                    }
                    return null;
            }
            return null;
        }

        public override string ExpressionLambdaToSqlMemberAccessString(MemberExpression exp, ExpTSC tsc)
        {
            if (exp.Expression == null)
            {
                switch (exp.Member.Name)
                {
                    case "Empty": return "''";
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Length": return $"char_length({left})";
            }
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
                    case "Today": return "curdate()";
                    case "MinValue": return "cast('0001/1/1 0:00:00' as DateTime)";
                    case "MaxValue": return "cast('9999/12/31 23:59:59' as DateTime)";
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            if ((exp.Expression as MemberExpression)?.Expression.NodeType == ExpressionType.Constant)
                left = $"toDateTime({left})";
            
            //IsDate(left);
            switch (exp.Member.Name)
            {
                case "Date": return $"toDate({left})";
                case "TimeOfDay": return $"dateDiff(second, toDate({left}), toDateTime({left}))*1000000";
                case "DayOfWeek": return $"(toDayOfWeek({left})-1)";
                case "Day": return $"toDayOfMonth({left})";
                case "DayOfYear": return $"toDayOfYear({left})";
                case "Month": return $"toMonth({left})";
                case "Year": return $"toYear({left})";
                case "Hour": return $"toHour({left})";
                case "Minute": return $"toMinute({left})";
                case "Second": return $"toSecond({left})";
                case "Millisecond": return $"(toSecond({left})/1000)";
                case "Ticks": return $"(dateDiff(second, toDate('0001-1-1'), toDateTime({left}))*10000000+621355968000000000)";
            }
            return null;
        }
        public bool IsInt(string _string)
        {
            if (string.IsNullOrEmpty(_string))
                return false;
            int i = 0;
            return int.TryParse(_string, out i);
        }
        public bool IsDate(string date)
        {
            if (string.IsNullOrEmpty(date))
                return true;
            return DateTime.TryParse(date, out var time);
        }
        public override string ExpressionLambdaToSqlMemberAccessTimeSpan(MemberExpression exp, ExpTSC tsc)
        {
            if (exp.Expression == null)
            {
                switch (exp.Member.Name)
                {
                    case "Zero": return "0";
                    case "MinValue": return "-922337203685477580"; //微秒 Ticks / 10
                    case "MaxValue": return "922337203685477580";
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Days": return $"intDiv(({left})/{60 * 60 * 24})";
                case "Hours": return $"intDiv(({left})/{60 * 60}%24)";
                case "Milliseconds": return $"(cast({left} as Int64)*1000)";
                case "Minutes": return $"intDiv(({left})/60%60)";
                case "Seconds": return $"(({left})%60)";
                case "Ticks": return $"(intDiv({left} as Int64)*10000000)";
                case "TotalDays": return $"(({left})/{60 * 60 * 24})";
                case "TotalHours": return $"(({left})/{60 * 60})";
                case "TotalMilliseconds": return $"(cast({left} as Int64)*1000)";
                case "TotalMinutes": return $"(({left})/60)";
                case "TotalSeconds": return $"({left})";
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
                        return $"({arg2} is null or {arg2} = '' or ltrim({arg2}) = '')";
                    case "Concat":
                        return _common.StringConcat(exp.Arguments.Select(a => getExp(a)).ToArray(), null);
                    case "Format":
                        if (exp.Arguments[0].NodeType != ExpressionType.Constant) throw new Exception(CoreStrings.Not_Implemented_Expression_ParameterUseConstant(exp,exp.Arguments[0]));
                        if (exp.Arguments.Count == 1) return ExpressionLambdaToSql(exp.Arguments[0], tsc);
                        var expArgsHack = exp.Arguments.Count == 2 && exp.Arguments[1].NodeType == ExpressionType.NewArrayInit ?
                            (exp.Arguments[1] as NewArrayExpression).Expressions : exp.Arguments.Where((a, z) => z > 0);
                        //3个 {} 时，Arguments 解析出来是分开的
                        //4个 {} 时，Arguments[1] 只能解析这个出来，然后里面是 NewArray []
                        var expArgs = expArgsHack.Select(a => $"',{_common.IsNull(ExpressionLambdaToSql(a, tsc), "''")},'").ToArray();
                        return $"concat({string.Format(ExpressionLambdaToSql(exp.Arguments[0], tsc), expArgs)})";
                    case "Join":
                        if (exp.IsStringJoin(out var tolistObjectExp, out var toListMethod, out var toListArgs1))
                        {
                            var newToListArgs0 = Expression.Call(tolistObjectExp, toListMethod,
                                Expression.Lambda(
                                    Expression.Call(
                                        typeof(SqlExtExtensions).GetMethod("StringJoinClickHouseGroupConcat"),
                                        Expression.Convert(toListArgs1.Body, typeof(object)),
                                        Expression.Convert(exp.Arguments[0], typeof(object))),
                                    toListArgs1.Parameters));
                            var newToListSql = getExp(newToListArgs0);
                            return newToListSql;
                        }
                        break;
                }
            }
            else
            {
                var left = getExp(exp.Object);
                switch (exp.Method.Name)
                {
                    case "StartsWith":
                    case "EndsWith":
                    case "Contains":
                        var args0Value = getExp(exp.Arguments[0]);
                        if (args0Value == "NULL") return $"({left}) IS NULL";
                        if (exp.Method.Name == "StartsWith") return $"positionCaseInsensitive({left}, {args0Value}) = 1";
                        if (exp.Method.Name == "EndsWith") return $"({left}) LIKE {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"concat('%', {args0Value})")}";
                        return $"positionCaseInsensitive({left}, {args0Value}) > 0";
                    case "ToLower": return $"lower({left})";
                    case "ToUpper": return $"upper({left})";
                    case "Substring":
                        var substrArgs1 = getExp(exp.Arguments[0]);
                        if (long.TryParse(substrArgs1, out var testtrylng1)) substrArgs1 = (testtrylng1 + 1).ToString();
                        else substrArgs1 += "+1";
                        if (exp.Arguments.Count == 1) return $"substr({left}, {substrArgs1})";
                        return $"substr({left}, {substrArgs1}, {getExp(exp.Arguments[1])})";
                    case "IndexOf":
                        var indexOfFindStr = getExp(exp.Arguments[0]);
                        if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32")
                        {
                            var locateArgs1 = getExp(exp.Arguments[1]);
                            if (long.TryParse(locateArgs1, out var testtrylng2)) locateArgs1 = (testtrylng2 + 1).ToString();
                            else locateArgs1 += "+1";
                            return $"(locate({indexOfFindStr}, {left}, {locateArgs1})-1)";
                        }
                        return $"(locate({indexOfFindStr}, {left})-1)";
                    case "PadLeft":
                        if (exp.Arguments.Count == 1) return $"lpad({left}, {getExp(exp.Arguments[0])})";
                        return $"lpad({left}, {getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                    case "PadRight":
                        if (exp.Arguments.Count == 1) return $"rpad({left}, {getExp(exp.Arguments[0])})";
                        return $"rpad({left}, {getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                    case "Trim":
                    case "TrimStart":
                    case "TrimEnd":
                        if (exp.Arguments.Count == 0)
                        {
                            if (exp.Method.Name == "Trim") return $"trim({left})";
                            if (exp.Method.Name == "TrimStart") return $"ltrim({left})";
                            if (exp.Method.Name == "TrimEnd") return $"rtrim({left})";
                        }
                        foreach (var argsTrim02 in exp.Arguments)
                        {
                            var argsTrim01s = new[] { argsTrim02 };
                            if (argsTrim02.NodeType == ExpressionType.NewArrayInit)
                            {
                                var arritem = argsTrim02 as NewArrayExpression;
                                argsTrim01s = arritem.Expressions.ToArray();
                            }
                            foreach (var argsTrim01 in argsTrim01s)
                            {
                                if (exp.Method.Name == "Trim") left = $"trim({getExp(argsTrim01)} from {left})";
                                if (exp.Method.Name == "TrimStart") left = $"trim(leading {getExp(argsTrim01)} from {left})";
                                if (exp.Method.Name == "TrimEnd") left = $"trim(trailing {getExp(argsTrim01)} from {left})";
                            }
                        }
                        return left;
                    case "Replace": return $"replace({left}, {getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                    case "CompareTo": return $"strcmp({left}, {getExp(exp.Arguments[0])})";
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
                case "Sign": return $"sign({getExp(exp.Arguments[0])})";
                case "Floor": return $"floor({getExp(exp.Arguments[0])})";
                case "Ceiling": return $"ceiling({getExp(exp.Arguments[0])})";
                case "Round":
                    if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32") return $"round({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                    return $"round({getExp(exp.Arguments[0])})";
                case "Exp": return $"exp({getExp(exp.Arguments[0])})";
                case "Log": return $"log({getExp(exp.Arguments[0])})";
                case "Log10": return $"log10({getExp(exp.Arguments[0])})";
                case "Pow": return $"pow({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                case "Sqrt": return $"sqrt({getExp(exp.Arguments[0])})";
                case "Cos": return $"cos({getExp(exp.Arguments[0])})";
                case "Sin": return $"sin({getExp(exp.Arguments[0])})";
                case "Tan": return $"tan({getExp(exp.Arguments[0])})";
                case "Acos": return $"acos({getExp(exp.Arguments[0])})";
                case "Asin": return $"asin({getExp(exp.Arguments[0])})";
                case "Atan": return $"atan({getExp(exp.Arguments[0])})";
                case "Atan2": return $"atan2({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                case "Truncate": return $"truncate({getExp(exp.Arguments[0])}, 0)";
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
                    case "Compare": return $"({getExp(exp.Arguments[0])} - ({getExp(exp.Arguments[1])}))";
                    case "DaysInMonth": return $"toDayOfMonth(subtractDays(addMonths(toStartOfMonth(concat({getExp(exp.Arguments[0])}, '-', {getExp(exp.Arguments[1])}, '-01')), 1), 1))";
                    case "Equals": return $"({getExp(exp.Arguments[0])} = {getExp(exp.Arguments[1])})";

                    case "IsLeapYear":
                        var isLeapYearArgs1 = getExp(exp.Arguments[0]);
                        return $"(({isLeapYearArgs1})%4=0 AND ({isLeapYearArgs1})%100<>0 OR ({isLeapYearArgs1})%400=0)";

                    case "Parse": return ExpressionConstDateTime(exp.Arguments[0]) ?? $"cast({getExp(exp.Arguments[0])} as DateTime)";
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return ExpressionConstDateTime(exp.Arguments[0]) ?? $"cast({getExp(exp.Arguments[0])} as DateTime)";
                }
            }
            else
            {
                var left = getExp(exp.Object);
                var args1 = exp.Arguments.Count == 0 ? null : getExp(exp.Arguments[0]);
                switch (exp.Method.Name)
                {
                    case "Add": return $"addSeconds(toDateTime({left}), {args1})";
                    case "AddDays": return $"addDays(toDateTime({left}), {args1})";
                    case "AddHours": return $"addHours(toDateTime({left}), {args1})";
                    case "AddMilliseconds": return $"addSeconds(toDateTime({left}), {args1}/1000)";
                    case "AddMinutes": return $"addMinutes(toDateTime({left}),{args1})";
                    case "AddMonths": return $"addMonths(toDateTime({left}),{args1})";
                    case "AddSeconds": return $"addSeconds(toDateTime({left}),{args1})";
                    case "AddTicks": return $"addSeconds(toDateTime({left}), {args1}/10000000)";
                    case "AddYears": return $"addYears(toDateTime({left}),{args1})";
                    case "Subtract":
                        switch ((exp.Arguments[0].Type.IsNullableType() ? exp.Arguments[0].Type.GetGenericArguments().FirstOrDefault() : exp.Arguments[0].Type).FullName)
                        {
                            case "System.DateTime": return $"dateDiff(second, {args1}, toDateTime({left}))";
                            case "System.TimeSpan": return $"addSeconds(toDateTime({left}),(({args1})*-1))";
                        }
                        break;
                    case "Equals": return $"({left} = {args1})";
                    case "CompareTo": return $"dateDiff(second,{args1},toDateTime({left}))";
                    case "ToString":
                        if (exp.Arguments.Count == 0) return $"date_format({left},'%Y-%m-%d %H:%M:%S.%f')";
                        switch (args1)
                        {
                            case "'yyyy-MM-dd HH:mm:ss'": return $"formatDateTime(toDateTime({left}),'%Y-%m-%d %H:%M:%S')";
                            case "'yyyy-MM-dd HH:mm'": return $"formatDateTime(toDateTime({left}),'%Y-%m-%d %H:%M')";
                            case "'yyyy-MM-dd HH'": return $"formatDateTime(toDateTime({left}),'%Y-%m-%d %H')";
                            case "'yyyy-MM-dd'": return $"formatDateTime(toDateTime({left}),'%Y-%m-%d')";
                            case "'yyyy-MM'": return $"formatDateTime(toDateTime({left}),'%Y-%m')";
                            case "'yyyyMMddHHmmss'": return $"formatDateTime(toDateTime({left}),'%Y%m%d%H%M%S')";
                            case "'yyyyMMddHHmm'": return $"formatDateTime(toDateTime({left}),'%Y%m%d%H%M')";
                            case "'yyyyMMddHH'": return $"formatDateTime(toDateTime({left}),'%Y%m%d%H')";
                            case "'yyyyMMdd'": return $"formatDateTime(toDateTime({left}),'%Y%m%d')";
                            case "'yyyyMM'": return $"formatDateTime(toDateTime({left}),'%Y%m')";
                            case "'yyyy'": return $"formatDateTime(toDateTime({left}),'%Y')";
                            case "'HH:mm:ss'": return $"formatDateTime(toDateTime({left}),'%H:%M:%S')";
                        }
                        args1 = Regex.Replace(args1, "(yyyy|yy|MM|M|dd|d|HH|H|hh|h|mm|ss|tt)", m =>
                        {
                            switch (m.Groups[1].Value)
                            {
                                case "yyyy": return $"%Y";
                                case "yy": return $"%y";
                                case "MM": return $"%_a1";
                                case "M": return $"%c";
                                case "dd": return $"%d";
                                case "d": return $"%e";
                                case "HH": return $"%H";
                                case "H": return $"%k";
                                case "hh": return $"%h";
                                case "h": return $"%l";
                                case "mm": return $"%i";
                                case "ss": return $"%_a2";
                                case "tt": return $"%p";
                            }
                            return m.Groups[0].Value;
                        });
                        var argsFinds = new[] { "%Y", "%y", "%_a1", "%c", "%d", "%e", "%H", "%k", "%h", "%l", "%M", "%_a2", "%p" };
                        var argsSpts = Regex.Split(args1, "(m|s|t)");
                        for (var a = 0; a < argsSpts.Length; a++)
                        {
                            switch (argsSpts[a])
                            {
                                case "m": argsSpts[a] = $"case when substr(formatDateTime(toDateTime({left}),'%M'),1,1) = '0' then substr(formatDateTime(toDateTime({left}),'%M'),2,1) else formatDateTime(toDateTime({left}),'%M') end"; break;
                                case "s": argsSpts[a] = $"case when substr(formatDateTime(toDateTime({left}),'%S'),1,1) = '0' then substr(formatDateTime(toDateTime({left}),'%S'),2,1) else formatDateTime(toDateTime({left}),'%S') end"; break;
                                case "t": argsSpts[a] = $"trim(trailing 'M' from formatDateTime(toDateTime({left}),'%p'))"; break;
                                default:
                                    var argsSptsA = argsSpts[a];
                                    if (argsSptsA.StartsWith("'")) argsSptsA = argsSptsA.Substring(1);
                                    if (argsSptsA.EndsWith("'")) argsSptsA = argsSptsA.Remove(argsSptsA.Length - 1);
                                    argsSpts[a] = argsFinds.Any(m => argsSptsA.Contains(m)) ? $"formatDateTime(toDateTime({left}),'{argsSptsA}')" : $"'{argsSptsA}'"; 
                                    break;
                            }
                        }
                        if (argsSpts.Length > 0) args1 = $"concat({string.Join(", ", argsSpts.Where(a => a != "''"))})";
                        return args1.Replace("%_a1", "%m").Replace("%_a2", "%S");
                }
            }
            return null;
        }
        public override string ExpressionLambdaToSqlCallTimeSpan(MethodCallExpression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            if (exp.Object == null)
            {
                switch (exp.Method.Name)
                {
                    case "Compare": return $"({getExp(exp.Arguments[0])}-({getExp(exp.Arguments[1])}))";
                    case "Equals": return $"({getExp(exp.Arguments[0])} = {getExp(exp.Arguments[1])})";
                    case "FromDays": return $"(({getExp(exp.Arguments[0])})*{(long)1000000 * 60 * 60 * 24})";
                    case "FromHours": return $"(({getExp(exp.Arguments[0])})*{(long)1000000 * 60 * 60})";
                    case "FromMilliseconds": return $"(({getExp(exp.Arguments[0])})*1000)";
                    case "FromMinutes": return $"(({getExp(exp.Arguments[0])})*{(long)1000000 * 60})";
                    case "FromSeconds": return $"(({getExp(exp.Arguments[0])})*1000000)";
                    case "FromTicks": return $"(({getExp(exp.Arguments[0])})/10)";
                    case "Parse": return $"cast({getExp(exp.Arguments[0])} as Int64)";
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return $"cast({getExp(exp.Arguments[0])} as Int64)";
                }
            }
            else
            {
                var left = getExp(exp.Object);
                var args1 = exp.Arguments.Count == 0 ? null : getExp(exp.Arguments[0]);
                switch (exp.Method.Name)
                {
                    case "Add": return $"({left}+{args1})";
                    case "Subtract": return $"({left}-({args1}))";
                    case "Equals": return $"({left} = {args1})";
                    case "CompareTo": return $"({left}-({args1}))";
                    case "ToString": return $"cast({left} as String)";
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
                    case "ToByte": return $"cast({getExp(exp.Arguments[0])} as Int8)";
                    case "ToChar": return $"substr(cast({getExp(exp.Arguments[0])} as String), 1, 1)";
                    case "ToDateTime": return ExpressionConstDateTime(exp.Arguments[0]) ?? $"cast({getExp(exp.Arguments[0])} as DateTime)";
                    case "ToDecimal": return $"cast({getExp(exp.Arguments[0])} as Decimal128(19))";
                    case "ToDouble": return $"cast({getExp(exp.Arguments[0])} as Float64)";
                    case "ToInt16":
                    case "ToInt32":
                    case "ToInt64":
                    case "ToSByte": return $"cast({getExp(exp.Arguments[0])} as UInt8)";
                    case "ToSingle": return $"cast({getExp(exp.Arguments[0])} as Float32)";
                    case "ToString": return $"cast({getExp(exp.Arguments[0])} as String)";
                    case "ToUInt16":
                    case "ToUInt32":
                    case "ToUInt64": return $"cast({getExp(exp.Arguments[0])} as UInt64)";
                }
            }
            return null;
        }
    }
}
