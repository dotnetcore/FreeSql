using FreeSql.Internal;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Firebird
{
    class FirebirdExpression : CommonExpression
    {

        public FirebirdExpression(CommonUtils common) : base(common) { }

        public override string ExpressionLambdaToSqlOther(Expression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            switch (exp.NodeType)
            {
                case ExpressionType.ArrayLength:
                    var arrOper = (exp as UnaryExpression)?.Operand;
                    if (arrOper.Type == typeof(byte[])) return $"octet_length({getExp(arrOper)})";
                    break;
                case ExpressionType.Convert:
                    var operandExp = (exp as UnaryExpression)?.Operand;
                    var gentype = exp.Type.NullableTypeOrThis();
                    if (gentype != operandExp.Type.NullableTypeOrThis())
                    {
                        switch (gentype.ToString())
                        {
                            case "System.Boolean": return $"({getExp(operandExp)} not in ('0','false'))";
                            case "System.Byte": return $"cast({getExp(operandExp)} as smallint)";
                            case "System.Char": return $"substring(cast({getExp(operandExp)} as varchar(10)) from 1 for 1)";
                            case "System.DateTime": return $"cast({getExp(operandExp)} as timestamp)";
                            case "System.Decimal": return $"cast({getExp(operandExp)} as decimal(18,6))";
                            case "System.Double": return $"cast({getExp(operandExp)} as decimal(18,10))";
                            case "System.Int16": return $"cast({getExp(operandExp)} as smallint)";
                            case "System.Int32": return $"cast({getExp(operandExp)} as integer)";
                            case "System.Int64": return $"cast({getExp(operandExp)} as bigint)";
                            case "System.SByte": return $"cast({getExp(operandExp)} as smallint)";
                            case "System.Single": return $"cast({getExp(operandExp)} as decimal(14,7))";
                            case "System.String": return $"cast({getExp(operandExp)} as blob sub_type 1)";
                            case "System.UInt16": return $"cast({getExp(operandExp)} as integer)";
                            case "System.UInt32": return $"cast({getExp(operandExp)} as bigint)";
                            case "System.UInt64": return $"cast({getExp(operandExp)} as decimal(21,0))";
                            case "System.Guid": return $"substring(cast({getExp(operandExp)} as char(36)) from 1 for 36)";
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
                                case "System.Byte": return $"cast({getExp(callExp.Arguments[0])} as smallint)";
                                case "System.Char": return $"substring(cast({getExp(callExp.Arguments[0])} as varchar(10)) from 1 for 1)";
                                case "System.DateTime": return $"cast({getExp(callExp.Arguments[0])} as timestamp)";
                                case "System.Decimal": return $"cast({getExp(callExp.Arguments[0])} as decimal(18,6))";
                                case "System.Double": return $"cast({getExp(callExp.Arguments[0])} as decimal(18,10))";
                                case "System.Int16": return $"cast({getExp(callExp.Arguments[0])} as smallint)";
                                case "System.Int32": return $"cast({getExp(callExp.Arguments[0])} as integer)";
                                case "System.Int64": return $"cast({getExp(callExp.Arguments[0])} as bigint)";
                                case "System.SByte": return $"cast({getExp(callExp.Arguments[0])} as smallint)";
                                case "System.Single": return $"cast({getExp(callExp.Arguments[0])} as decimal(14,7))";
                                case "System.String": return $"cast({getExp(callExp.Arguments[0])} as blob sub_type 1)";
                                case "System.UInt16": return $"cast({getExp(callExp.Arguments[0])} as integer)";
                                case "System.UInt32": return $"cast({getExp(callExp.Arguments[0])} as bigint)";
                                case "System.UInt64": return $"cast({getExp(callExp.Arguments[0])} as decimal(18,0))";
                                case "System.Guid": return $"substring(cast({getExp(callExp.Arguments[0])} as char(36)) from 1 for 36)";
                            }
                            return null;
                        case "NewGuid":
                            return null;
                        case "Next":
                            if (callExp.Object?.Type == typeof(Random)) return "cast(rand()*1000000000 as integer)";
                            return null;
                        case "NextDouble":
                            if (callExp.Object?.Type == typeof(Random)) return "rand()";
                            return null;
                        case "Random":
                            if (callExp.Method.DeclaringType.IsNumberType()) return "rand()";
                            return null;
                        case "ToString":
                            if (callExp.Object != null) return callExp.Arguments.Count == 0 ? $"cast({getExp(callExp.Object)} as blob sub_type 1)" : null;
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
                    }
                    if (objType == null) objType = callExp.Method.DeclaringType;
                    if (objType != null || objType.IsArrayOrList())
                    {
                        if (argIndex >= callExp.Arguments.Count) break;
                        tsc.SetMapColumnTmp(null);
                        var args1 = getExp(callExp.Arguments[argIndex]);
                        var oldMapType = tsc.SetMapTypeReturnOld(tsc.mapTypeTmp);
                        var oldDbParams = tsc.SetDbParamsReturnOld(null);
                        var left = objExp == null ? null : getExp(objExp);
                        tsc.SetMapColumnTmp(null).SetMapTypeReturnOld(oldMapType);
                        tsc.SetDbParamsReturnOld(oldDbParams);
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
                    case "Today": return "current_date";
                    case "MinValue": return "timestamp '0001/1/1 0:00:00'";
                    case "MaxValue": return "timestamp'9999/12/31 23:59:59'";
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Date": return $"cast(extract(year from {left}) || '-' || extract(month from {left}) || '-' || extract(day from {left}) as timestamp)";
                case "TimeOfDay": return $"datediff(second from cast(extract(year from {left}) || '-' || extract(month from {left}) || '-' || extract(day from {left}) as timestamp) to {left})";
                case "DayOfWeek": return $"extract(weekday from {left})";
                case "Day": return $"extract(day from {left})";
                case "DayOfYear": return $"datediff(day from {left} to cast(extract(year from {left}) || '-' || extract(month from {left}) || '-' || extract(day from {left}) as timestamp))";
                case "Month": return $"extract(month from {left})";
                case "Year": return $"extract(year from {left})";
                case "Hour": return $"extract(hour from {left})";
                case "Minute": return $"extract(minute from {left})";
                case "Second": return $"extract(second from {left})";
                case "Millisecond": return $"extract(millisecond from {left})";
                case "Ticks": return $"(extract(second from {left})*10000000+621355968000000000)";
            }
            return null;
        }
        public override string ExpressionLambdaToSqlMemberAccessTimeSpan(MemberExpression exp, ExpTSC tsc)
        {
            if (exp.Expression == null)
            {
                switch (exp.Member.Name)
                {
                    case "Zero": return "0";
                    case "MinValue": return "-922337203685.477580"; //秒 Ticks / 1000,000,0
                    case "MaxValue": return "922337203685.477580";
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Days": return $"floor(({left})/{60 * 60 * 24})";
                case "Hours": return $"mod(({left})/{60 * 60},24)";
                case "Milliseconds": return $"(cast({left} as bigint)*1000)";
                case "Minutes": return $"mod(({left})/60,60)";
                case "Seconds": return $"mod({left},60)";
                case "Ticks": return $"(cast({left} as bigint)*10000000)";
                case "TotalDays": return $"(({left})/{60 * 60 * 24})";
                case "TotalHours": return $"(({left})/{60 * 60})";
                case "TotalMilliseconds": return $"(cast({left} as bigint)*1000)";
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
                        return $"({arg2} is null or {arg2} = '' or trim({arg2}) = '')";
                    case "Concat":
                        return _common.StringConcat(exp.Arguments.Select(a => getExp(a)).ToArray(), null);
                    case "Format":
                        if (exp.Arguments[0].NodeType != ExpressionType.Constant) throw new Exception($"未实现函数表达式 {exp} 解析，参数 {exp.Arguments[0]} 必须为常量");
                        var expArgsHack = exp.Arguments.Count == 2 && exp.Arguments[1].NodeType == ExpressionType.NewArrayInit ?
                            (exp.Arguments[1] as NewArrayExpression).Expressions : exp.Arguments.Where((a, z) => z > 0);
                        //3个 {} 时，Arguments 解析出来是分开的
                        //4个 {} 时，Arguments[1] 只能解析这个出来，然后里面是 NewArray []
                        var expArgs = expArgsHack.Select(a => $"'||{_common.IsNull(ExpressionLambdaToSql(a, tsc), "''")}||'").ToArray();
                        return string.Format(ExpressionLambdaToSql(exp.Arguments[0], tsc), expArgs);
                    case "Join":
                        if (exp.IsStringJoin(out var tolistObjectExp, out var toListMethod, out var toListArgs1))
                        {
                            var newToListArgs0 = Expression.Call(tolistObjectExp, toListMethod,
                                Expression.Lambda(
                                    Expression.Call(
                                        typeof(SqlExtExtensions).GetMethod("StringJoinFirebirdList"),
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
                        if (exp.Method.Name == "StartsWith") return $"({left}) LIKE {(args0Value.EndsWith("'") ? args0Value.Insert(args0Value.Length - 1, "%") : $"({args0Value})||'%'")}";
                        if (exp.Method.Name == "EndsWith") return $"({left}) LIKE {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"'%'||({args0Value})")}";
                        if (args0Value.StartsWith("'") && args0Value.EndsWith("'")) return $"({left}) CONTAINING {args0Value}";
                        return $"({left}) CONTAINING ({args0Value})";
                    case "ToLower": return $"lower({left})";
                    case "ToUpper": return $"upper({left})";
                    case "Substring":
                        var substrArgs1 = getExp(exp.Arguments[0]);
                        if (long.TryParse(substrArgs1, out var testtrylng1)) substrArgs1 = (testtrylng1 + 1).ToString();
                        else substrArgs1 += "+1";
                        if (exp.Arguments.Count == 1) return $"substring({left} from {substrArgs1})";
                        return $"substring({left} from {substrArgs1} for {getExp(exp.Arguments[1])})";
                    case "IndexOf":
                        var indexOfFindStr = getExp(exp.Arguments[0]);
                        if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32")
                        {
                            var locateArgs1 = getExp(exp.Arguments[1]);
                            if (long.TryParse(locateArgs1, out var testtrylng2)) locateArgs1 = (testtrylng2 + 1).ToString();
                            else locateArgs1 += "+1";
                            return $"(position({indexOfFindStr}, {left}, {locateArgs1})-1)";
                        }
                        return $"(position({indexOfFindStr}, {left})-1)";
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
                            if (exp.Method.Name == "TrimStart") return $"trim(leading from {left})";
                            if (exp.Method.Name == "TrimEnd") return $"trim(trailing from {left})";
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
                    case "CompareTo": return $"case when {left} = {getExp(exp.Arguments[0])} then 0 when {left} > {getExp(exp.Arguments[0])} then 1 else -1 end";
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
                case "Log": return $"log({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                case "Log10": return $"log10({getExp(exp.Arguments[0])})";
                case "Pow": return $"power({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                case "Sqrt": return $"sqrt({getExp(exp.Arguments[0])})";
                case "Cos": return $"cos({getExp(exp.Arguments[0])})";
                case "Sin": return $"sin({getExp(exp.Arguments[0])})";
                case "Tan": return $"tan({getExp(exp.Arguments[0])})";
                case "Acos": return $"acos({getExp(exp.Arguments[0])})";
                case "Asin": return $"asin({getExp(exp.Arguments[0])})";
                case "Atan": return $"atan({getExp(exp.Arguments[0])})";
                case "Atan2": return $"atan2({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                case "Truncate": return $"trunc({getExp(exp.Arguments[0])}, 0)";
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
                    case "DaysInMonth": return $"extract(day from dateadd(-1 day to dateadd(1 month to cast({getExp(exp.Arguments[0])}||'-'||{getExp(exp.Arguments[1])}||'-01' as date))))";
                    case "Equals": return $"({getExp(exp.Arguments[0])} = {getExp(exp.Arguments[1])})";

                    case "IsLeapYear":
                        var isLeapYearArgs1 = getExp(exp.Arguments[0]);
                        return $"mod({isLeapYearArgs1},4)=0 AND mod({isLeapYearArgs1},100)<>0 OR mod({isLeapYearArgs1},400)=0";

                    case "Parse": return $"cast({getExp(exp.Arguments[0])} as timestamp)";
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return $"cast({getExp(exp.Arguments[0])} as timestamp)";
                }
            }
            else
            {
                var left = getExp(exp.Object);
                var args1 = exp.Arguments.Count == 0 ? null : getExp(exp.Arguments[0]);
                switch (exp.Method.Name)
                {
                    case "Add": return $"dateadd({args1} second to {left})";
                    case "AddDays": return $"dateadd({args1} day to {left})";
                    case "AddHours": return $"dateadd({args1} hour to {left})";
                    case "AddMilliseconds": return $"dateadd(({args1})/1000 second to {left})";
                    case "AddMinutes": return $"dateadd({args1} minute to {left})";
                    case "AddMonths": return $"dateadd({args1} month to {left})";
                    case "AddSeconds": return $"dateadd({args1} second to {left})";
                    case "AddTicks": return $"dateadd(({args1})/10000000 second to {left})";
                    case "AddYears": return $"dateadd({args1} year to {left})";
                    case "Subtract":
                        switch ((exp.Arguments[0].Type.IsNullableType() ? exp.Arguments[0].Type.GetGenericArguments().FirstOrDefault() : exp.Arguments[0].Type).FullName)
                        {
                            case "System.DateTime": return $"datediff(second from {left} to {args1})";
                            case "System.TimeSpan": return $"dateadd(({args1})*-1 second to {left})";
                        }
                        break;
                    case "Equals": return $"({left} = {args1})";
                    case "CompareTo": return $"datediff(second from {left} to {args1})";
                    case "ToString":
                        if (left.EndsWith(" as timestamp)") == false && left.StartsWith("timestamp") == false) left = $"cast({left} as timestamp)";
                        if (exp.Arguments.Count == 0) return $"lpad(extract(year from {left}),4,'0')||'-'||lpad(extract(month from {left}),2,'0')||'-'||lpad(extract(day from {left}),2,'0')||' '||lpad(extract(hour from {left}),2,'0')||':'||lpad(extract(minute from {left}),2,'0')||':'||lpad(trunc(extract(second from {left})),2,'0')";
                        switch (args1.TrimStart('N'))
                        {
                            case "'yyyyMMdd'": return $"lpad(extract(year from {left}),4,'0')||lpad(extract(month from {left}),2,'0')||lpad(extract(day from {left}),2,'0')";
                            case "'yyyyMM'": return $"lpad(extract(year from {left}),4,'0')||lpad(extract(month from {left}),2,'0')";
                            case "'yyyy'": return $"lpad(extract(year from {left}),4,'0')";
                        }
                        var isMatched = false;
                        args1 = Regex.Replace(args1, "(yyyy|yy|MM|M|dd|d|HH|H|hh|h|mm|m|ss|s|tt|t)", m =>
                        {
                            isMatched = true;
                            switch (m.Groups[1].Value)
                            {
                                case "yyyy": return $"'||lpad(extract(year from {left}),4,'0')||'";
                                case "yy": return $"'||substring(lpad(extract(year from {left}),4,'0') from 3 for 2)||'";
                                case "MM": return $"'||lpad(extract(month from {left}),2,'0')||'";
                                case "M": return $"'||extract(month from {left})||'";
                                case "dd": return $"'||lpad(extract(day from {left}),2,'0')||'";
                                case "d": return $"'||extract(day from {left})||'";
                                case "HH": return $"'||lpad(extract(hour from {left}),2,'0')||'";
                                case "H": return $"'||extract(hour from {left})||'";
                                case "hh": return $"'||case when mod(extract(hour from {left}),12) = 0 then '12' else lpad(mod(extract(hour from {left}),12),2,'0') end||'";
                                case "h": return $"'||trim(case when mod(extract(hour from {left}),12) = 0 then '12' else mod(extract(hour from {left}),12) end)||'";
                                case "mm": return $"'||lpad(extract(minute from {left}),2,'0')||'";
                                case "m": return $"'||extract(minute from {left})||'";
                                case "ss": return $"'||lpad(trunc(extract(second from {left})),2,'0')||'";
                                case "s": return $"'||trunc(extract(second from {left}))||'";
                                case "tt": return $"'||case when extract(hour from {left}) >= 12 then 'PM' else 'AM' end||'";
                                case "t": return $"'||case when extract(hour from {left}) >= 12 then 'P' else 'A' end||'";
                            }
                            return m.Groups[0].Value;
                        });
                        return isMatched == false ? args1 : $"({args1})";
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
                    case "FromDays": return $"(({getExp(exp.Arguments[0])})*{60 * 60 * 24})";
                    case "FromHours": return $"(({getExp(exp.Arguments[0])})*{60 * 60})";
                    case "FromMilliseconds": return $"(({getExp(exp.Arguments[0])})/1000)";
                    case "FromMinutes": return $"(({getExp(exp.Arguments[0])})*60)";
                    case "FromSeconds": return $"(({getExp(exp.Arguments[0])}))";
                    case "FromTicks": return $"(({getExp(exp.Arguments[0])})/10000000)";
                    case "Parse": return $"cast({getExp(exp.Arguments[0])} as bigint)";
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return $"cast({getExp(exp.Arguments[0])} as bigint)";
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
                    case "ToString": return $"cast({left} as varchar(50))";
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
                    case "ToByte": return $"cast({getExp(exp.Arguments[0])} as smallint)";
                    case "ToChar": return $"substring(cast({getExp(exp.Arguments[0])} as varchar(10)) from 1 for 1)";
                    case "ToDateTime": return $"cast({getExp(exp.Arguments[0])} as timestamp)";
                    case "ToDecimal": return $"cast({getExp(exp.Arguments[0])} as decimal(18,6))";
                    case "ToDouble": return $"cast({getExp(exp.Arguments[0])} as decimal(18,10))";
                    case "ToInt16": return $"cast({getExp(exp.Arguments[0])} as smallint)";
                    case "ToInt32": return $"cast({getExp(exp.Arguments[0])} as integer)";
                    case "ToInt64": return $"cast({getExp(exp.Arguments[0])} as bigint)";
                    case "ToSByte": return $"cast({getExp(exp.Arguments[0])} as smallint)";
                    case "ToSingle": return $"cast({getExp(exp.Arguments[0])} as decimal(14,7))";
                    case "ToString": return $"cast({getExp(exp.Arguments[0])} as blob sub_type 1)";
                    case "ToUInt16": return $"cast({getExp(exp.Arguments[0])} as integer)";
                    case "ToUInt32": return $"cast({getExp(exp.Arguments[0])} as bigint)";
                    case "ToUInt64": return $"cast({getExp(exp.Arguments[0])} as decimal(18,0))";
                }
            }
            return null;
        }
    }
}
