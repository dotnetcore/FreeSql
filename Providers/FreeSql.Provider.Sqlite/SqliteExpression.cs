using FreeSql.Internal;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Sqlite
{
    class SqliteExpression : CommonExpression
    {

        public SqliteExpression(CommonUtils common) : base(common) { }

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
                        switch (exp.Type.NullableTypeOrThis().ToString())
                        {
                            case "System.Boolean": return $"({getExp(operandExp)} not in ('0','false'))";
                            case "System.Byte": return $"cast({getExp(operandExp)} as int2)";
                            case "System.Char": return $"substr(cast({getExp(operandExp)} as character), 1, 1)";
                            case "System.DateTime": return $"datetime({getExp(operandExp)})";
                            case "System.Decimal": return $"cast({getExp(operandExp)} as decimal(36,18))";
                            case "System.Double": return $"cast({getExp(operandExp)} as double)";
                            case "System.Int16":
                            case "System.Int32":
                            case "System.Int64":
                            case "System.SByte": return $"cast({getExp(operandExp)} as smallint)";
                            case "System.Single": return $"cast({getExp(operandExp)} as float)";
                            case "System.String": return $"cast({getExp(operandExp)} as character)";
                            case "System.UInt16": return $"cast({getExp(operandExp)} as unsigned)";
                            case "System.UInt32": return $"cast({getExp(operandExp)} as decimal(10,0))";
                            case "System.UInt64": return $"cast({getExp(operandExp)} as decimal(21,0))";
                            case "System.Guid": return $"substr(cast({getExp(operandExp)} as character), 1, 36)";
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
                                case "System.Byte": return $"cast({getExp(callExp.Arguments[0])} as int2)";
                                case "System.Char": return $"substr(cast({getExp(callExp.Arguments[0])} as character), 1, 1)";
                                case "System.DateTime": return $"datetime({getExp(callExp.Arguments[0])})";
                                case "System.Decimal": return $"cast({getExp(callExp.Arguments[0])} as decimal(36,18))";
                                case "System.Double": return $"cast({getExp(callExp.Arguments[0])} as double)";
                                case "System.Int16":
                                case "System.Int32":
                                case "System.Int64":
                                case "System.SByte": return $"cast({getExp(callExp.Arguments[0])} as smallint)";
                                case "System.Single": return $"cast({getExp(callExp.Arguments[0])} as float)";
                                case "System.UInt16": return $"cast({getExp(callExp.Arguments[0])} as unsigned)";
                                case "System.UInt32": return $"cast({getExp(callExp.Arguments[0])} as decimal(10,0))";
                                case "System.UInt64": return $"cast({getExp(callExp.Arguments[0])} as decimal(21,0))";
                                case "System.Guid": return $"substr(cast({getExp(callExp.Arguments[0])} as character), 1, 36)";
                            }
                            return null;
                        case "NewGuid":
                            return null;
                        case "Next":
                            if (callExp.Object?.Type == typeof(Random)) return "cast(random()*1000000000 as int)";
                            return null;
                        case "NextDouble":
                            if (callExp.Object?.Type == typeof(Random)) return "random()";
                            return null;
                        case "Random":
                            if (callExp.Method.DeclaringType.IsNumberType()) return "random()";
                            return null;
                        case "ToString":
                            if (callExp.Object != null) return callExp.Arguments.Count == 0 ? $"cast({getExp(callExp.Object)} as character)" : null;
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
                    case "Today": return "date(current_timestamp,'localtime')";
                    case "MinValue": return "datetime('0001-01-01 00:00:00.000')";
                    case "MaxValue": return "datetime('9999-12-31 23:59:59.999')";
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Date": return $"date({left})";
                case "TimeOfDay": return $"strftime('%s',{left})";
                case "DayOfWeek": return $"strftime('%w',{left})";
                case "Day": return $"strftime('%d',{left})";
                case "DayOfYear": return $"strftime('%j',{left})";
                case "Month": return $"strftime('%m',{left})";
                case "Year": return $"strftime('%Y',{left})";
                case "Hour": return $"strftime('%H',{left})";
                case "Minute": return $"strftime('%M',{left})";
                case "Second": return $"strftime('%S',{left})";
                case "Millisecond": return $"(strftime('%f',{left})-strftime('%S',{left}))";
                case "Ticks": return $"(strftime('%s',{left})*10000000+621355968000000000)";
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
                case "Hours": return $"floor(({left})/{60 * 60}%24)";
                case "Milliseconds": return $"(cast({left} as bigint)*1000)";
                case "Minutes": return $"floor(({left})/60%60)";
                case "Seconds": return $"(({left})%60)";
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
                                        typeof(SqlExtExtensions).GetMethod("StringJoinSqliteGroupConcat"),
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
                        if (args0Value.StartsWith("'") && args0Value.EndsWith("'")) return $"({left}) LIKE {args0Value.Insert(1, "%").Insert(args0Value.Length, "%")}";
                        return $"({left}) LIKE '%'||({args0Value})||'%'";
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
                        //if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32") {
                        //	var locateArgs1 = getExp(exp.Arguments[1]);
                        //	if (long.TryParse(locateArgs1, out var testtrylng2)) locateArgs1 = (testtrylng2 + 1).ToString();
                        //	else locateArgs1 += "+1";
                        //	return $"(instr({left}, {indexOfFindStr}, {locateArgs1})-1)";
                        //}
                        return $"(instr({left}, {indexOfFindStr})-1)";
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
                        var trimArg1 = "";
                        var trimArg2 = "";
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
                                var trimChr = getExp(argsTrim01).Trim('\'');
                                if (trimChr.Length == 1) trimArg1 += trimChr;
                                else trimArg2 += $" || ({trimChr})";
                            }
                        }
                        if (exp.Method.Name == "Trim") left = $"trim({left}, {_common.FormatSql("{0}", trimArg1)}{trimArg2})";
                        if (exp.Method.Name == "TrimStart") left = $"ltrim({left}, {_common.FormatSql("{0}", trimArg1)}{trimArg2})";
                        if (exp.Method.Name == "TrimEnd") left = $"rtrim({left}, {_common.FormatSql("{0}", trimArg1)}{trimArg2})";
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
                case "Log": return $"log({getExp(exp.Arguments[0])})";
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
                    //case "Truncate": return $"truncate({getExp(exp.Arguments[0])}, 0)";
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
                    case "Compare": return $"(strftime('%s',{getExp(exp.Arguments[0])}) -strftime('%s',{getExp(exp.Arguments[1])}))";
                    case "DaysInMonth": return $"strftime('%d',date({getExp(exp.Arguments[0])}||'-01-01',{getExp(exp.Arguments[1])}||' months','-1 days'))";
                    case "Equals": return $"({getExp(exp.Arguments[0])} = {getExp(exp.Arguments[1])})";

                    case "IsLeapYear":
                        var isLeapYearArgs1 = getExp(exp.Arguments[0]);
                        return $"(({isLeapYearArgs1})%4=0 AND ({isLeapYearArgs1})%100<>0 OR ({isLeapYearArgs1})%400=0)";

                    case "Parse": return $"datetime({getExp(exp.Arguments[0])})";
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return $"datetime({getExp(exp.Arguments[0])})";
                }
            }
            else
            {
                var left = getExp(exp.Object);
                var args1 = exp.Arguments.Count == 0 ? null : getExp(exp.Arguments[0]);
                switch (exp.Method.Name)
                {
                    case "Add": return $"datetime({left},({args1})||' seconds')";
                    case "AddDays": return $"datetime({left},({args1})||' days')";
                    case "AddHours": return $"datetime({left},({args1})||' hours')";
                    case "AddMilliseconds": return $"datetime({left},(({args1})/1000)||' seconds')";
                    case "AddMinutes": return $"datetime({left},({args1})||' seconds')";
                    case "AddMonths": return $"datetime({left},({args1})||' months')";
                    case "AddSeconds": return $"datetime({left},({args1})||' seconds')";
                    case "AddTicks": return $"datetime({left},(({args1})/10000000)||' seconds')";
                    case "AddYears": return $"datetime({left},({args1})||' years')";
                    case "Subtract":
                        switch ((exp.Arguments[0].Type.IsNullableType() ? exp.Arguments[0].Type.GetGenericArguments().FirstOrDefault() : exp.Arguments[0].Type).FullName)
                        {
                            case "System.DateTime": return $"(strftime('%s',{left})-strftime('%s',{args1}))";
                            case "System.TimeSpan": return $"datetime({left},(({args1})*-1)||' seconds')";
                        }
                        break;
                    case "Equals": return $"({left} = {args1})";
                    case "CompareTo": return $"(strftime('%s',{left})-strftime('%s',{args1}))";
                    case "ToString":
                        if (exp.Arguments.Count == 0) return $"strftime('%Y-%m-%d %H:%M:%f',{left})";
                        switch (args1)
                        {
                            case "'yyyy-MM-dd HH:mm:ss'": return $"strftime('%Y-%m-%d %H:%M:%S',{left})";
                            case "'yyyy-MM-dd HH:mm'": return $"strftime('%Y-%m-%d %H:%M',{left})";
                            case "'yyyy-MM-dd HH'": return $"strftime('%Y-%m-%d %H',{left})";
                            case "'yyyy-MM-dd'": return $"strftime('%Y-%m-%d',{left})";
                            case "'yyyy-MM'": return $"strftime('%Y-%m',{left})";
                            case "'yyyyMMddHHmmss'": return $"strftime('%Y%m%d%H%M%S',{left})";
                            case "'yyyyMMddHHmm'": return $"strftime('%Y%m%d%H%M',{left})";
                            case "'yyyyMMddHH'": return $"strftime('%Y%m%d%H',{left})";
                            case "'yyyyMMdd'": return $"strftime('%Y%m%d',{left})";
                            case "'yyyyMM'": return $"strftime('%Y%m',{left})";
                            case "'yyyy'": return $"strftime('%Y',{left})";
                            case "'HH:mm:ss'": return $"strftime('%H:%M:%S',{left})";
                        }
                        args1 = Regex.Replace(args1, "(yyyy|MM|dd|HH|mm|ss)", m =>
                        {
                            switch (m.Groups[1].Value)
                            {
                                case "yyyy": return $"%Y";
                                case "MM": return $"%_a1";
                                case "dd": return $"%_a2";
                                case "HH": return $"%_a3";
                                case "mm": return $"%_a4";
                                case "ss": return $"%S";
                            }
                            return m.Groups[0].Value;
                        });
                        var argsFinds = new[] { "%Y", "%_a1", "%_a2", "%_a3", "%_a4", "%S" };
                        var argsSpts = Regex.Split(args1, "(yy|M|d|H|hh|h|m|s|tt|t)");
                        for (var a = 0; a < argsSpts.Length; a++)
                        {
                            switch (argsSpts[a])
                            {
                                case "yy": argsSpts[a] = $"substr(strftime('%Y',{left}),3,2)"; break;
                                case "M": argsSpts[a] = $"ltrim(strftime('%m',{left}),'0')"; break;
                                case "d": argsSpts[a] = $"ltrim(strftime('%d',{left}),'0')"; break;
                                case "H": argsSpts[a] = $"case when substr(strftime('%H',{left}),1,1) = '0' then substr(strftime('%H',{left}),2,1) else strftime('%H',{left}) end"; break;
                                case "hh": argsSpts[a] = $"case cast(case when substr(strftime('%H',{left}),1,1) = '0' then substr(strftime('%H',{left}),2,1) else strftime('%H',{left}) end as smallint) % 12 when 0 then '12' when 1 then '01' when 2 then '02' when 3 then '03' when 4 then '04' when 5 then '05' when 6 then '06' when 7 then '07' when 8 then '08' when 9 then '09' when 10 then '10' when 11 then '11' end"; break;
                                case "h": argsSpts[a] = $"case cast(case when substr(strftime('%H',{left}),1,1) = '0' then substr(strftime('%H',{left}),2,1) else strftime('%H',{left}) end as smallint) % 12 when 0 then '12' when 1 then '1' when 2 then '2' when 3 then '3' when 4 then '4' when 5 then '5' when 6 then '6' when 7 then '7' when 8 then '8' when 9 then '9' when 10 then '10' when 11 then '11' end"; break;
                                case "m": argsSpts[a] = $"case when substr(strftime('%M',{left}),1,1) = '0' then substr(strftime('%M',{left}),2,1) else strftime('%M',{left}) end"; break;
                                case "s": argsSpts[a] = $"case when substr(strftime('%S',{left}),1,1) = '0' then substr(strftime('%S',{left}),2,1) else strftime('%S',{left}) end"; break;
                                case "tt": argsSpts[a] = $"case when cast(case when substr(strftime('%H',{left}),1,1) = '0' then substr(strftime('%H',{left}),2,1) else strftime('%H',{left}) end as smallint) >= 12 then 'PM' else 'AM' end"; break;
                                case "t": argsSpts[a] = $"case when cast(case when substr(strftime('%H',{left}),1,1) = '0' then substr(strftime('%H',{left}),2,1) else strftime('%H',{left}) end as smallint) >= 12 then 'P' else 'A' end"; break;
                                default:
                                    var argsSptsA = argsSpts[a];
                                    if (argsSptsA.StartsWith("'")) argsSptsA = argsSptsA.Substring(1);
                                    if (argsSptsA.EndsWith("'")) argsSptsA = argsSptsA.Remove(argsSptsA.Length - 1);
                                    argsSpts[a] = argsFinds.Any(m => argsSptsA.Contains(m)) ? $"strftime('{argsSptsA}',{left})" : $"'{argsSptsA}'"; 
                                    break;
                            }
                        }
                        if (argsSpts.Length > 0) args1 = $"({string.Join(" || ", argsSpts.Where(a => a != "''"))})";
                        return args1.Replace("%_a1", "%m").Replace("%_a2", "%d").Replace("%_a3", "%H").Replace("%_a4", "%M");
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
                    case "ToString": return $"cast({left} as character)";
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
                    case "ToByte": return $"cast({getExp(exp.Arguments[0])} as int2)";
                    case "ToChar": return $"substr(cast({getExp(exp.Arguments[0])} as character), 1, 1)";
                    case "ToDateTime": return $"datetime({getExp(exp.Arguments[0])})";
                    case "ToDecimal": return $"cast({getExp(exp.Arguments[0])} as decimal(36,18))";
                    case "ToDouble": return $"cast({getExp(exp.Arguments[0])} as double)";
                    case "ToInt16":
                    case "ToInt32":
                    case "ToInt64":
                    case "ToSByte": return $"cast({getExp(exp.Arguments[0])} as smallint)";
                    case "ToSingle": return $"cast({getExp(exp.Arguments[0])} as float)";
                    case "ToString": return $"cast({getExp(exp.Arguments[0])} as character)";
                    case "ToUInt16": return $"cast({getExp(exp.Arguments[0])} as unsigned)";
                    case "ToUInt32": return $"cast({getExp(exp.Arguments[0])} as decimal(10,0))";
                    case "ToUInt64": return $"cast({getExp(exp.Arguments[0])} as decimal(21,0))";
                }
            }
            return null;
        }
    }
}
