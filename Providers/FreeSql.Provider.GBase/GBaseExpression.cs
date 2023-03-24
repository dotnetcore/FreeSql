using FreeSql.Internal;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.GBase
{
    class GBaseExpression : CommonExpression
    {

        public GBaseExpression(CommonUtils common) : base(common) { }

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
                            case "System.Boolean": return $"({getExp(operandExp)} not in ('0','F','f'))";
                            case "System.Byte": return $"cast({getExp(operandExp)} as smallint)";
                            case "System.Char": return $"substring(cast({getExp(operandExp)} as varchar(10)) from 1 for 1)";
                            case "System.DateTime": return $"to_date({getExp(operandExp)})";
                            case "System.Decimal": return $"cast({getExp(operandExp)} as decimal(18,6))";
                            case "System.Double": return $"cast({getExp(operandExp)} as decimal(18,10))";
                            case "System.Int16": return $"cast({getExp(operandExp)} as smallint)";
                            case "System.Int32": return $"cast({getExp(operandExp)} as integer)";
                            case "System.Int64": return $"cast({getExp(operandExp)} as bigint)";
                            case "System.SByte": return $"cast({getExp(operandExp)} as smallint)";
                            case "System.Single": return $"cast({getExp(operandExp)} as decimal(14,7))";
                            case "System.String": return $"cast({getExp(operandExp)} as varchar(8000))";
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
                                case "System.Boolean": return $"({getExp(callExp.Arguments[0])} not in ('0','F','f'))";
                                case "System.Byte": return $"cast({getExp(callExp.Arguments[0])} as smallint)";
                                case "System.Char": return $"substring(cast({getExp(callExp.Arguments[0])} as varchar(10)) from 1 for 1)";
                                case "System.DateTime": return $"to_date({getExp(callExp.Arguments[0])})";
                                case "System.Decimal": return $"cast({getExp(callExp.Arguments[0])} as decimal(18,6))";
                                case "System.Double": return $"cast({getExp(callExp.Arguments[0])} as decimal(18,10))";
                                case "System.Int16": return $"cast({getExp(callExp.Arguments[0])} as smallint)";
                                case "System.Int32": return $"cast({getExp(callExp.Arguments[0])} as integer)";
                                case "System.Int64": return $"cast({getExp(callExp.Arguments[0])} as bigint)";
                                case "System.SByte": return $"cast({getExp(callExp.Arguments[0])} as smallint)";
                                case "System.Single": return $"cast({getExp(callExp.Arguments[0])} as decimal(14,7))";
                                case "System.String": return $"cast({getExp(callExp.Arguments[0])} as varchar(8000))";
                                case "System.UInt16": return $"cast({getExp(callExp.Arguments[0])} as integer)";
                                case "System.UInt32": return $"cast({getExp(callExp.Arguments[0])} as bigint)";
                                case "System.UInt64": return $"cast({getExp(callExp.Arguments[0])} as decimal(18,0))";
                                case "System.Guid": return $"substring(cast({getExp(callExp.Arguments[0])} as char(36)) from 1 for 36)";
                            }
                            return null;
                        case "NewGuid":
                            return null;
                        case "Next":
                            if (callExp.Object?.Type == typeof(Random)) return "random()";
                            return null;
                        case "NextDouble":
                            if (callExp.Object?.Type == typeof(Random)) return "cast(random() as float)";
                            return null;
                        case "Random":
                            if (callExp.Method.DeclaringType.IsNumberType()) return "random()";
                            return null;
                        case "ToString":
                            if (callExp.Object != null) return callExp.Arguments.Count == 0 ? $"cast({getExp(callExp.Object)} as varchar(8000))" : null;
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
                    case "Today": return "current_date";
                    case "MinValue": return "to_date('0001-1-1', 'YYYY-MM-DD')";
                    case "MaxValue": return "to_date('9999-12-31 23:59:59', 'YYYY-MM-DD HH24:MI:SS')";
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Date": return $"to_date(to_char({left}, 'YYYY-MM-DD'), 'YYYY-MM-DD')";
                case "TimeOfDay": return $"('0 '||to_char({left}, 'HH24:MI:SS.FF3'))::interval day(9) to fraction";
                case "DayOfWeek": return $"weekday({left})";
                case "Day": return $"day({left})";
                case "DayOfYear": return $"cast(to_char({left},'DDD') as int)";
                case "Month": return $"month({left})";
                case "Year": return $"year({left})";
                case "Hour": return $"cast(to_char({left},'HH24') as int)";
                case "Minute": return $"cast(to_char({left},'MI') as int)";
                case "Second": return $"cast(to_char({left},'SS') as int)";
                case "Millisecond": return $"cast(to_char({left},'FF3') as int)";
                //case "Ticks": return $"cast(to_char({left},'FF7') as bigint)";
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
                    case "MinValue": return "interval(0) day(9) to fraction"; //秒 Ticks / 1000,000,0
                    case "MaxValue": return "interval(99 23:59:59.999) day(9) to fraction";
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Days": return $"({left})::interval day(9) to day::varchar(40)::int8";
                case "Hours": return $"substr(substring_index(({left})::varchar(40),' ',-1),1,2)::int8";
                case "Milliseconds": return $"substring_index(({left})::varchar(40),'.',-1)::int8";
                case "Minutes": return $"substr(substring_index(({left})::varchar(40),' ',-1),4,2)::int8";
                case "Seconds": return $"substr(substring_index(({left})::varchar(40),' ',-1),7,2)::int8";
                case "Ticks": return $"(({left})::interval day(9) to day::varchar(40)::int8 *24*60*60*1000 + substr(substring_index(({left})::varchar(40),' ',-1),1,2)::int8 *60*60*1000 + substr(substring_index(({left})::varchar(40),' ',-1),4,2)::int8 *60*1000 + substr(substring_index(({left})::varchar(40),' ',-1),7,2)::int8 *1000 + substring_index(({left})::varchar(40),'.',-1)::int8) * 10000";
                case "TotalDays": return $"(({left})::interval day(9) to day::varchar(40)::int8 *24 + substr(substring_index(({left})::varchar(40),' ',-1),1,2)::int8) /24.0";
                case "TotalHours": return $"(({left})::interval day(9) to day::varchar(40)::int8 *24*60 + substr(substring_index(({left})::varchar(40),' ',-1),1,2)::int8 *60 + substr(substring_index(({left})::varchar(40),' ',-1),4,2)::int8) /60.0";
                case "TotalMilliseconds": return $"(({left})::interval day(9) to day::varchar(40)::int8 *24*60*60*1000 + substr(substring_index(({left})::varchar(40),' ',-1),1,2)::int8 *60*60*1000 + substr(substring_index(({left})::varchar(40),' ',-1),4,2)::int8 *60*1000 + substr(substring_index(({left})::varchar(40),' ',-1),7,2)::int8 *1000 + substring_index(({left})::varchar(40),'.',-1)::int8)";
                case "TotalMinutes": return $"(({left})::interval day(9) to day::varchar(40)::int8 *24*60*60 + substr(substring_index(({left})::varchar(40),' ',-1),1,2)::int8 *60*60 + substr(substring_index(({left})::varchar(40),' ',-1),4,2)::int8 *60 + substr(substring_index(({left})::varchar(40),' ',-1),7,2)::int8) /60.0";
                case "TotalSeconds": return $"(({left})::interval day(9) to day::varchar(40)::int8 *24*60*60*1000 + substr(substring_index(({left})::varchar(40),' ',-1),1,2)::int8 *60*60*1000 + substr(substring_index(({left})::varchar(40),' ',-1),4,2)::int8 *60*1000 + substr(substring_index(({left})::varchar(40),' ',-1),7,2)::int8 *1000 + substring_index(({left})::varchar(40),'.',-1)::int8) /1000.0";
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
                    case "Format":
                        if (exp.Arguments[0].NodeType != ExpressionType.Constant) throw new Exception(CoreStrings.Not_Implemented_Expression_ParameterUseConstant(exp,exp.Arguments[0]));
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
                                        typeof(SqlExtExtensions).GetMethod("StringJoinGBaseWmConcatText"),
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
                        if (args0Value.Contains("%"))
                        {
                            if (exp.Method.Name == "StartsWith") return $"instr({args0Value}, {left}) = 1";
                            if (exp.Method.Name == "EndsWith") return $"instr({args0Value}, {left}) = char_length({args0Value})";
                            return $"instr({args0Value}, {left}) > 0";
                        }
                        if (exp.Method.Name == "StartsWith") return $"({left}) LIKE {(args0Value.EndsWith("'") ? args0Value.Insert(args0Value.Length - 1, "%") : $"({args0Value})||'%'")}";
                        if (exp.Method.Name == "EndsWith") return $"({left}) LIKE {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"'%'||({args0Value})")}";
                        if (args0Value.StartsWith("'") && args0Value.EndsWith("'")) return $"({left}) LIKE {args0Value.Insert(1, "%").Insert(args0Value.Length, "%")}";
                        return $"({left}) LIKE ('%'||cast({args0Value} as varchar(8000))||'%')";
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
                            return $"(instr({indexOfFindStr}, {left}, {locateArgs1})-1)";
                        }
                        return $"(instr({indexOfFindStr}, {left})-1)";
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
                case "Ceiling": return $"ceil({getExp(exp.Arguments[0])})";
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
                    case "DaysInMonth": return $"cast(to_char(last_day(to_date(({getExp(exp.Arguments[0])})||'-'||({getExp(exp.Arguments[1])})||'-01','yyyy-mm-dd')),'DD') as int)";
                    case "Equals": return $"({getExp(exp.Arguments[0])} = {getExp(exp.Arguments[1])})";

                    case "IsLeapYear":
                        var isLeapYearArgs1 = getExp(exp.Arguments[0]);
                        return $"mod({isLeapYearArgs1},4)=0 AND mod({isLeapYearArgs1},100)<>0 OR mod({isLeapYearArgs1},400)=0";

                    case "Parse": return $"to_date({getExp(exp.Arguments[0])})";
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return $"to_date({getExp(exp.Arguments[0])})";
                }
            }
            else
            {
                var left = getExp(exp.Object);
                var args1 = exp.Arguments.Count == 0 ? null : getExp(exp.Arguments[0]);
                switch (exp.Method.Name)
                {
                    case "Add": return $"({left} + ({args1}))";
                    case "AddDays": return $"({left} + ({args1}) units day)";
                    case "AddHours": return $"({left} + ({args1}) units hour)";
                    case "AddMilliseconds": return $"({left} + ({args1})/1000 units fraction)";
                    case "AddMinutes": return $"({left} + ({args1}) units minute)";
                    case "AddMonths": return $"({left} + ({args1}) units month)";
                    case "AddSeconds": return $"({left} + ({args1}) units second)";
                    case "AddTicks": return $"({left} + ({args1})/10000000 units fraction)";
                    case "AddYears": return $"({left} + ({args1}) units year)";
                    case "Subtract":
                        switch ((exp.Arguments[0].Type.IsNullableType() ? exp.Arguments[0].Type.GetGenericArguments().FirstOrDefault() : exp.Arguments[0].Type).FullName)
                        {
                            case "System.DateTime": return $"({left} - {args1})";
                            case "System.TimeSpan": return $"({left} - {args1})";
                        }
                        break;
                    case "Equals": return $"({left} = {args1})";
                    case "CompareTo": return $"({left} - {args1})";
                    case "ToString":
                        var defaultFmt = "'YYYY-MM-DD HH24:MI:SS'";
                        if (left.StartsWith("'") || left.EndsWith("'"))
                        {
                            var precision = left.LastIndexOf('.');
                            if (precision != -1)
                            {
                                precision = left.Substring(precision).Length - 2;
                                if (precision > 0) defaultFmt = $"'YYYY-MM-DD HH24:MI:SS.FF{precision}'";
                            }
                            left = $"to_date({left},{defaultFmt})";
                        }
                        if (exp.Arguments.Count == 0) return $"to_char({left},{defaultFmt})";
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
                                case "yyyy": return $"YYYY";
                                case "yy": return $"YY";
                                case "MM": return $"%_a1";
                                case "dd": return $"%_a2";
                                case "HH": return $"%_a3";
                                case "hh": return $"%_a4";
                                case "mm": return $"%_a5";
                                case "ss": return $"SS";
                                case "tt": return $"%_a6";
                            }
                            return m.Groups[0].Value;
                        });
                        var argsFinds = new[] { "YYYY", "YY", "%_a1", "%_a2", "%_a3", "%_a4", "%_a5", "SS", "%_a6" };
                        var argsSpts = Regex.Split(args1, "(M|d|H|h|m|s|t)");
                        for (var a = 0; a < argsSpts.Length; a++)
                        {
                            switch (argsSpts[a])
                            {
                                case "M": argsSpts[a] = $"ltrim(to_char({left},'MM'),'0')"; break;
                                case "d": argsSpts[a] = $"case when substr(to_char({left},'DD'),1,1) = '0' then substr(to_char({left},'DD'),2,1) else to_char({left},'DD') end"; break;
                                case "H": argsSpts[a] = $"case when substr(to_char({left},'HH24'),1,1) = '0' then substr(to_char({left},'HH24'),2,1) else to_char({left},'HH24') end"; break;
                                case "h": argsSpts[a] = $"case when substr(to_char({left},'HH12'),1,1) = '0' then substr(to_char({left},'HH12'),2,1) else to_char({left},'HH12') end"; break;
                                case "m": argsSpts[a] = $"case when substr(to_char({left},'MI'),1,1) = '0' then substr(to_char({left},'MI'),2,1) else to_char({left},'MI') end"; break;
                                case "s": argsSpts[a] = $"case when substr(to_char({left},'SS'),1,1) = '0' then substr(to_char({left},'SS'),2,1) else to_char({left},'SS') end"; break;
                                case "t": argsSpts[a] = $"rtrim(to_char({left},'AM'),'M')"; break;
                                default:
                                    var argsSptsA = argsSpts[a];
                                    if (argsSptsA.StartsWith("'")) argsSptsA = argsSptsA.Substring(1);
                                    if (argsSptsA.EndsWith("'")) argsSptsA = argsSptsA.Remove(argsSptsA.Length - 1);
                                    argsSpts[a] = argsFinds.Any(m => argsSptsA.Contains(m)) ? $"to_char({left},'{argsSptsA}')" : $"'{argsSptsA}'";
                                    break;
                            }
                        }
                        if (argsSpts.Length > 0) args1 = $"({string.Join(" || ", argsSpts.Where(a => a != "''"))})";
                        return args1.Replace("%_a1", "MM").Replace("%_a2", "DD").Replace("%_a3", "HH24").Replace("%_a4", "HH12").Replace("%_a5", "MI").Replace("%_a6", "AM");
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
                    case "Compare": return $"({getExp(exp.Arguments[0])} - ({getExp(exp.Arguments[1])}))";
                    case "Equals": return $"({getExp(exp.Arguments[0])} = {getExp(exp.Arguments[1])})";
                    case "FromDays": return $"(interval(0) day(9) to fraction + ({getExp(exp.Arguments[0])}) units day)";
                    case "FromHours": return $"(interval(0) day(9) to fraction + ({getExp(exp.Arguments[0])}) units hour)";
                    case "FromMilliseconds": return $"(interval(0) day(9) to fraction + ({getExp(exp.Arguments[0])})/1000 units fraction)";
                    case "FromMinutes": return $"(interval(0) day(9) to fraction + ({getExp(exp.Arguments[0])}) units minute)";
                    case "FromSeconds": return $"(interval(0) day(9) to fraction + ({getExp(exp.Arguments[0])}) units second)";
                    case "FromTicks": return $"(interval(0) day(9) to fraction + ({getExp(exp.Arguments[0])})/10000000 units fraction)";
                    case "Parse": return $"cast({getExp(exp.Arguments[0])} as interval day(9) to fraction)";
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return $"cast({getExp(exp.Arguments[0])} as interval day(9) to fraction)";
                }
            }
            else
            {
                var left = getExp(exp.Object);
                var args1 = exp.Arguments.Count == 0 ? null : getExp(exp.Arguments[0]);
                switch (exp.Method.Name)
                {
                    case "Add": return $"({left} + {args1})";
                    case "Subtract": return $"({left} - ({args1}))";
                    case "Equals": return $"({left} = {args1})";
                    case "CompareTo": return $"({left} - ({args1}))";
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
                    case "ToDateTime": return $"to_date({getExp(exp.Arguments[0])})";
                    case "ToDecimal": return $"cast({getExp(exp.Arguments[0])} as decimal(18,6))";
                    case "ToDouble": return $"cast({getExp(exp.Arguments[0])} as decimal(18,10))";
                    case "ToInt16": return $"cast({getExp(exp.Arguments[0])} as smallint)";
                    case "ToInt32": return $"cast({getExp(exp.Arguments[0])} as integer)";
                    case "ToInt64": return $"cast({getExp(exp.Arguments[0])} as bigint)";
                    case "ToSByte": return $"cast({getExp(exp.Arguments[0])} as smallint)";
                    case "ToSingle": return $"cast({getExp(exp.Arguments[0])} as decimal(14,7))";
                    case "ToString": return $"cast({getExp(exp.Arguments[0])} as varchar(8000))";
                    case "ToUInt16": return $"cast({getExp(exp.Arguments[0])} as integer)";
                    case "ToUInt32": return $"cast({getExp(exp.Arguments[0])} as bigint)";
                    case "ToUInt64": return $"cast({getExp(exp.Arguments[0])} as decimal(18,0))";
                }
            }
            return null;
        }
    }
}
