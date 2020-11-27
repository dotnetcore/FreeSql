using FreeSql.Internal;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Odbc.Dameng
{
    class OdbcDamengExpression : CommonExpression
    {

        public OdbcDamengExpression(CommonUtils common) : base(common) { }

        public override string ExpressionLambdaToSqlOther(Expression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            switch (exp.NodeType)
            {
                case ExpressionType.ArrayLength:
                    var arrOper = (exp as UnaryExpression)?.Operand;
                    if (arrOper.Type == typeof(byte[])) return $"lengthb({getExp(arrOper)})";
                    break;
                case ExpressionType.Convert:
                    var operandExp = (exp as UnaryExpression)?.Operand;
                    var gentype = exp.Type.NullableTypeOrThis();
                    if (gentype != operandExp.Type.NullableTypeOrThis())
                    {
                        switch (exp.Type.NullableTypeOrThis().ToString())
                        {
                            //case "System.Boolean": return $"({getExp(operandExp)} not in ('0','false'))";
                            case "System.Byte": return $"cast({getExp(operandExp)} as number)";
                            case "System.Char": return $"substr(to_char({getExp(operandExp)}), 1, 1)";
                            case "System.DateTime": return $"to_timestamp({getExp(operandExp)},'YYYY-MM-DD HH24:MI:SS.FF6')";
                            case "System.Decimal": return $"cast({getExp(operandExp)} as number)";
                            case "System.Double": return $"cast({getExp(operandExp)} as number)";
                            case "System.Int16":
                            case "System.Int32":
                            case "System.Int64":
                            case "System.SByte": return $"cast({getExp(operandExp)} as number)";
                            case "System.Single": return $"cast({getExp(operandExp)} as number)";
                            case "System.String": return $"to_char({getExp(operandExp)})";
                            case "System.UInt16":
                            case "System.UInt32":
                            case "System.UInt64": return $"cast({getExp(operandExp)} as number)";
                            case "System.Guid":
                                if (tsc.mapType == typeof(byte[])) return $"hextoraw({getExp(operandExp)})";
                                return $"to_char({getExp(operandExp)})";
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
                                //case "System.Boolean": return $"({getExp(callExp.Arguments[0])} not in ('0','false'))";
                                case "System.Byte": return $"cast({getExp(callExp.Arguments[0])} as number)";
                                case "System.Char": return $"substr(to_char({getExp(callExp.Arguments[0])}), 1, 1)";
                                case "System.DateTime": return $"to_timestamp({getExp(callExp.Arguments[0])},'YYYY-MM-DD HH24:MI:SS.FF6')";
                                case "System.Decimal": return $"cast({getExp(callExp.Arguments[0])} as number)";
                                case "System.Double": return $"cast({getExp(callExp.Arguments[0])} as number)";
                                case "System.Int16":
                                case "System.Int32":
                                case "System.Int64":
                                case "System.SByte": return $"cast({getExp(callExp.Arguments[0])} as number)";
                                case "System.Single": return $"cast({getExp(callExp.Arguments[0])} as number)";
                                case "System.UInt16":
                                case "System.UInt32":
                                case "System.UInt64": return $"cast({getExp(callExp.Arguments[0])} as number)";
                                case "System.Guid":
                                    if (tsc.mapType == typeof(byte[])) return $"hextoraw({getExp(callExp.Arguments[0])})";
                                    return $"to_char({getExp(callExp.Arguments[0])})";
                            }
                            return null;
                        case "NewGuid":
                            return null;
                        case "Next":
                            if (callExp.Object?.Type == typeof(Random)) return "cast(dbms_random.value*1000000000 as number)";
                            return null;
                        case "NextDouble":
                            if (callExp.Object?.Type == typeof(Random)) return "dbms_random.value";
                            return null;
                        case "Random":
                            if (callExp.Method.DeclaringType.IsNumberType()) return "dbms_random.value";
                            return null;
                        case "ToString":
                            if (callExp.Object != null) return callExp.Arguments.Count == 0 ? $"to_char({getExp(callExp.Object)})" : null;
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
                case "Length": return $"length({left})";
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
                    case "Today": return "trunc(systimestamp)";
                    case "MinValue": return "to_timestamp('0001-01-01 00:00:00','YYYY-MM-DD HH24:MI:SS.FF6')";
                    case "MaxValue": return "to_timestamp('9999-12-31 23:59:59','YYYY-MM-DD HH24:MI:SS.FF6')";
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Date": return $"trunc({left})";
                case "TimeOfDay": return $"(cast({left} as timestamp with time zone)-trunc({left}))";
                case "DayOfWeek": return $"case when to_char({left},'D')='7' then 0 else cast(to_char({left},'D') as number) end";
                case "Day": return $"cast(to_char({left},'DD') as number)";
                case "DayOfYear": return $"cast(to_char({left},'DDD') as number)";
                case "Month": return $"cast(to_char({left},'MM') as number)";
                case "Year": return $"cast(to_char({left},'YYYY') as number)";
                case "Hour": return $"cast(to_char({left},'HH24') as number)";
                case "Minute": return $"cast(to_char({left},'MI') as number)";
                case "Second": return $"cast(to_char({left},'SS') as number)";
                case "Millisecond": return $"cast(to_char({left},'FF3') as number)";
                case "Ticks": return $"cast(to_char({left},'FF7') as number)";
            }
            return null;
        }
        public override string ExpressionLambdaToSqlMemberAccessTimeSpan(MemberExpression exp, ExpTSC tsc)
        {
            if (exp.Expression == null)
            {
                switch (exp.Member.Name)
                {
                    case "Zero": return "numtodsinterval(0,'second')";
                    case "MinValue": return "numtodsinterval(-233720368.5477580,'second')";
                    case "MaxValue": return "numtodsinterval(233720368.5477580,'second')";
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Days": return $"extract(day from {left})";
                case "Hours": return $"extract(hour from {left})";
                case "Milliseconds": return $"cast(substr(extract(second from {left})-floor(extract(second from {left})),3,3) as number)";
                case "Minutes": return $"extract(minute from {left})";
                case "Seconds": return $"floor(extract(second from {left}))";
                case "Ticks": return $"(extract(day from {left})*86400+extract(hour from {left})*3600+extract(minute from {left})*60+extract(second from {left}))*10000000";
                case "TotalDays": return $"extract(day from {left})";
                case "TotalHours": return $"(extract(day from {left})*24+extract(hour from {left}))";
                case "TotalMilliseconds": return $"(extract(day from {left})*86400+extract(hour from {left})*3600+extract(minute from {left})*60+extract(second from {left}))*1000";
                case "TotalMinutes": return $"(extract(day from {left})*1440+extract(hour from {left})*60+extract(minute from {left}))";
                case "TotalSeconds": return $"(extract(day from {left})*86400+extract(hour from {left})*3600+extract(minute from {left})*60+extract(second from {left}))";
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
                                        typeof(SqlExtExtensions).GetMethod("StringJoinOracleGroupConcat"),
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
                        if (exp.Method.Name == "StartsWith") return $"({left}) LIKE {(args0Value.EndsWith("'") ? args0Value.Insert(args0Value.Length - 1, "%") : $"(to_char({args0Value})||'%')")}";
                        if (exp.Method.Name == "EndsWith") return $"({left}) LIKE {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"('%'||to_char({args0Value}))")}";
                        if (args0Value.StartsWith("'") && args0Value.EndsWith("'")) return $"({left}) LIKE {args0Value.Insert(1, "%").Insert(args0Value.Length, "%")}";
                        return $"({left}) LIKE ('%'||to_char({args0Value})||'%')";
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
                            return $"(instr({left}, {indexOfFindStr}, {locateArgs1}, 1)-1)";
                        }
                        return $"(instr({left}, {indexOfFindStr}, 1, 1))-1";
                    case "PadLeft":
                        if (exp.Arguments.Count == 1) return $"lpad({left}, {getExp(exp.Arguments[0])}, ' ')";
                        return $"lpad({left}, {getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                    case "PadRight":
                        if (exp.Arguments.Count == 1) return $"rpad({left}, {getExp(exp.Arguments[0])}, ' ')";
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
                                if (exp.Method.Name == "Trim") left = $"trim(both {getExp(argsTrim01)} from {left})";
                                if (exp.Method.Name == "TrimStart") left = $"ltrim({left},{getExp(argsTrim01)})";
                                if (exp.Method.Name == "TrimEnd") left = $"rtrim({left},{getExp(argsTrim01)})";
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
                case "Ceiling": return $"ceil({getExp(exp.Arguments[0])})";
                case "Round":
                    if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32") return $"round({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                    return $"round({getExp(exp.Arguments[0])})";
                case "Exp": return $"exp({getExp(exp.Arguments[0])})";
                case "Log":
                    if (exp.Arguments.Count > 1) return $"log({getExp(exp.Arguments[1])},{getExp(exp.Arguments[0])})";
                    return $"log(2.7182818284590451,{getExp(exp.Arguments[0])})";
                case "Log10": return $"log(10,{getExp(exp.Arguments[0])})";
                case "Pow": return $"power({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                case "Sqrt": return $"sqrt({getExp(exp.Arguments[0])})";
                case "Cos": return $"cos({getExp(exp.Arguments[0])})";
                case "Sin": return $"sin({getExp(exp.Arguments[0])})";
                case "Tan": return $"tan({getExp(exp.Arguments[0])})";
                case "Acos": return $"acos({getExp(exp.Arguments[0])})";
                case "Asin": return $"asin({getExp(exp.Arguments[0])})";
                case "Atan": return $"atan({getExp(exp.Arguments[0])})";
                //case "Atan2": return $"atan2({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
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
                    case "Compare": return $"extract(day from ({getExp(exp.Arguments[0])}-({getExp(exp.Arguments[1])})))";
                    case "DaysInMonth": return $"cast(to_char(last_day(to_date(({getExp(exp.Arguments[0])})||'-'||({getExp(exp.Arguments[1])})||'-01','yyyy-mm-dd')),'DD') as number)";
                    case "Equals": return $"({getExp(exp.Arguments[0])} = {getExp(exp.Arguments[1])})";

                    case "IsLeapYear":
                        var isLeapYearArgs1 = getExp(exp.Arguments[0]);
                        return $"(mod({isLeapYearArgs1},4)=0 AND mod({isLeapYearArgs1},100)<>0 OR mod({isLeapYearArgs1},400)=0)";

                    case "Parse": return $"to_timestamp({getExp(exp.Arguments[0])},'YYYY-MM-DD HH24:MI:SS.FF6')";
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return $"to_timestamp({getExp(exp.Arguments[0])},'YYYY-MM-DD HH24:MI:SS.FF6')";
                }
            }
            else
            {
                var left = getExp(exp.Object);
                var args1 = exp.Arguments.Count == 0 ? null : getExp(exp.Arguments[0]);
                switch (exp.Method.Name)
                {
                    case "Add": return $"({left}+{args1})";
                    case "AddDays": return $"({left}+{args1})";
                    case "AddHours": return $"({left}+({args1})/24)";
                    case "AddMilliseconds": return $"({left}+({args1})/86400000)";
                    case "AddMinutes": return $"({left}+({args1})/1440)";
                    case "AddMonths": return $"add_months({left},{args1})";
                    case "AddSeconds": return $"({left}+({args1})/86400)";
                    case "AddTicks": return $"({left}+({args1})/864000000000)";
                    case "AddYears": return $"add_months({left},({args1})*12)";
                    case "Subtract":
                        switch ((exp.Arguments[0].Type.IsNullableType() ? exp.Arguments[0].Type.GetGenericArguments().FirstOrDefault() : exp.Arguments[0].Type).FullName)
                        {
                            case "System.DateTime": return $"(cast({left} as timestamp with time zone)-{args1})";
                            case "System.TimeSpan": return $"({left}-{args1})";
                        }
                        break;
                    case "Equals": return $"({left} = {args1})";
                    case "CompareTo": return $"extract(day from ({left}-({args1})))";
                    case "ToString":
                        if (left.StartsWith("'") || left.EndsWith("'")) left = $"to_timestamp({left},'YYYY-MM-DD HH24:MI:SS.FF6')";
                        if (exp.Arguments.Count == 0) return $"to_char({left},'YYYY-MM-DD HH24:MI:SS.FF6')";
                        switch (args1)
                        {
                            case "'yyyy-MM-dd HH:mm:ss'": return $"to_char({left},'YYYY-MM-DD HH24:MI:SS')";
                            case "'yyyy-MM-dd HH:mm'": return $"to_char({left},'YYYY-MM-DD HH24:MI')";
                            case "'yyyy-MM-dd HH'": return $"to_char({left},'YYYY-MM-DD HH24')";
                            case "'yyyy-MM-dd'": return $"to_char({left},'YYYY-MM-DD')";
                            case "'yyyy-MM'": return $"to_char({left},'YYYY-MM')";
                            case "'yyyyMMddHHmmss'": return $"to_char({left},'YYYYMMDDHH24MISS')";
                            case "'yyyyMMddHHmm'": return $"to_char({left},'YYYYMMDDHH24MI')";
                            case "'yyyyMMddHH'": return $"to_char({left},'YYYYMMDDHH24')";
                            case "'yyyyMMdd'": return $"to_char({left},'YYYYMMDD')";
                            case "'yyyyMM'": return $"to_char({left},'YYYYMM')";
                            case "'yyyy'": return $"to_char({left},'YYYY')";
                            case "'HH:mm:ss'": return $"to_char({left},'HH24:MI:SS')";
                        }
                        args1 = Regex.Replace(args1, "(yyyy|yy|MM|dd|HH|hh|mm|ss|tt)", m =>
                        {
                            switch (m.Groups[1].Value)
                            {
                                case "yyyy": return "YYYY";
                                case "yy": return "YY";
                                case "MM": return "%_a1";
                                case "dd": return "%_a2";
                                case "HH": return "%_a3";
                                case "mm": return "%_a4";
                                case "ss": return "SS";
                                case "tt": return "%_a5";
                            }
                            return m.Groups[0].Value;
                        });
                        var argsFinds = new[] { "YYYY", "YY", "%_a1", "%_a2", "%_a3", "%_a4", "SS", "%_a5" };
                        var argsSpts = Regex.Split(args1, "(M|d|H|hh|h|m|s|t)");
                        for (var a = 0; a < argsSpts.Length; a++)
                        {
                            switch (argsSpts[a])
                            {
                                case "M": argsSpts[a] = $"ltrim(to_char({left},'MM'),'0')"; break;
                                case "d": argsSpts[a] = $"case when substr(to_char({left},'DD'),1,1) = '0' then substr(to_char({left},'DD'),2,1) else to_char({left},'DD') end"; break;
                                case "H": argsSpts[a] = $"case when substr(to_char({left},'HH24'),1,1) = '0' then substr(to_char({left},'HH24'),2,1) else to_char({left},'HH24') end"; break;
                                case "hh": argsSpts[a] = $"case mod(cast(case when substr(to_char({left},'HH24'),1,1) = '0' then substr(to_char({left},'HH24'),2,1) else to_char({left},'HH24') end as number),12) when 0 then '12' when 1 then '01' when 2 then '02' when 3 then '03' when 4 then '04' when 5 then '05' when 6 then '06' when 7 then '07' when 8 then '08' when 9 then '09' when 10 then '10' when 11 then '11' end"; break;
                                case "h": argsSpts[a] = $"case mod(cast(case when substr(to_char({left},'HH24'),1,1) = '0' then substr(to_char({left},'HH24'),2,1) else to_char({left},'HH24') end as number),12) when 0 then '12' when 1 then '1' when 2 then '2' when 3 then '3' when 4 then '4' when 5 then '5' when 6 then '6' when 7 then '7' when 8 then '8' when 9 then '9' when 10 then '10' when 11 then '11' end"; break;
                                case "m": argsSpts[a] = $"case when substr(to_char({left},'MI'),1,1) = '0' then substr(to_char({left},'MI'),2,1) else to_char({left},'MI') end"; break;
                                case "s": argsSpts[a] = $"case when substr(to_char({left},'SS'),1,1) = '0' then substr(to_char({left},'SS'),2,1) else to_char({left},'SS') end"; break;
                                case "t": argsSpts[a] = $"rtrim(to_char({left},'AM'),'M')"; break;
                                default:
                                    var argsSptsA = argsSpts[a];
                                    if (argsSptsA.StartsWith("'")) argsSptsA = argsSptsA.Substring(1);
                                    if (argsSptsA.EndsWith("'")) argsSptsA = argsSptsA.Remove(argsSptsA.Length - 1);
                                    argsSpts[a] = argsFinds.Any(m => argsSptsA.Contains(m)) ? $"to_char({left},'{argsSptsA}')" : $"'{argsSptsA}'"; 
                                    break;
                                    //达梦 to_char(to_timestamp('2020-02-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'),' ') 无效
                            }
                        }
                        if (argsSpts.Length > 0) args1 = $"({string.Join(" || ", argsSpts.Where(a => a != "''"))})";
                        return args1.Replace("%_a1", "MM").Replace("%_a2", "DD").Replace("%_a3", "HH24").Replace("%_a4", "MI").Replace("%_a5", "AM");
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
                    case "Compare": return $"extract(day from ({getExp(exp.Arguments[0])}-({getExp(exp.Arguments[1])})))";
                    case "Equals": return $"({getExp(exp.Arguments[0])} = {getExp(exp.Arguments[1])})";
                    case "FromDays": return $"numtodsinterval(({getExp(exp.Arguments[0])})*{(long)60 * 60 * 24},'second')";
                    case "FromHours": return $"numtodsinterval(({getExp(exp.Arguments[0])})*{(long)60 * 60},'second')";
                    case "FromMilliseconds": return $"numtodsinterval(({getExp(exp.Arguments[0])})/1000,'second')";
                    case "FromMinutes": return $"numtodsinterval(({getExp(exp.Arguments[0])})*60,'second')";
                    case "FromSeconds": return $"numtodsinterval(({getExp(exp.Arguments[0])}),'second')";
                    case "FromTicks": return $"numtodsinterval(({getExp(exp.Arguments[0])})/10000000,'second')";
                    case "Parse": return $"cast({getExp(exp.Arguments[0])} as interval day(9) to second(6))";
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return $"cast({getExp(exp.Arguments[0])} as interval day(9) to second(6))";
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
                    case "CompareTo": return $"extract(day from ({left}-({args1})))";
                    case "ToString": return $"to_char({left})";
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
                    //case "ToBoolean": return $"({getExp(exp.Arguments[0])} not in ('0','false'))";
                    case "ToByte": return $"cast({getExp(exp.Arguments[0])} as number)";
                    case "ToChar": return $"substr(to_char({getExp(exp.Arguments[0])}), 1, 1)";
                    case "ToDateTime": return $"to_timestamp({getExp(exp.Arguments[0])},'YYYY-MM-DD HH24:MI:SS.FF6')";
                    case "ToDecimal": return $"cast({getExp(exp.Arguments[0])} as number)";
                    case "ToDouble": return $"cast({getExp(exp.Arguments[0])} as number)";
                    case "ToInt16":
                    case "ToInt32":
                    case "ToInt64":
                    case "ToSByte": return $"cast({getExp(exp.Arguments[0])} as number)";
                    case "ToSingle": return $"cast({getExp(exp.Arguments[0])} as number)";
                    case "ToString": return $"to_char({getExp(exp.Arguments[0])})";
                    case "ToUInt16":
                    case "ToUInt32":
                    case "ToUInt64": return $"cast({getExp(exp.Arguments[0])} as number)";
                }
            }
            return null;
        }
    }
}
