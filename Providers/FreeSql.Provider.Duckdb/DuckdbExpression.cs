using FreeSql.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Duckdb
{
    class DuckdbExpression : CommonExpression
    {

        public DuckdbExpression(CommonUtils common) : base(common) { }

        public override string ExpressionLambdaToSqlOther(Expression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            switch (exp.NodeType)
            {
                case ExpressionType.ArrayLength:
                    var arrOper = (exp as UnaryExpression)?.Operand;
                    var arrOperExp = getExp(arrOper);
                    if (arrOperExp.StartsWith("(") || arrOperExp.EndsWith(")")) return $"len([{arrOperExp.TrimStart('(').TrimEnd(')')}])";
                    if (arrOper.Type == typeof(byte[])) return $"octet_length({getExp(arrOper)})";
                    return $"case when {arrOperExp} is null then 0 else len({arrOperExp}) end";
                case ExpressionType.Convert:
                    var operandExp = (exp as UnaryExpression)?.Operand;
                    var gentype = exp.Type.NullableTypeOrThis();
                    if (gentype != operandExp.Type.NullableTypeOrThis())
                    {
                        switch (exp.Type.NullableTypeOrThis().ToString())
                        {
                            case "System.Boolean": return $"({getExp(operandExp)} not in ('0','false'))";
                            case "System.Byte": return $"cast({getExp(operandExp)} as utinyint)";
                            case "System.Char": return $"substr(cast({getExp(operandExp)} as char), 1, 1)";
                            case "System.DateTime": return ExpressionConstDateTime(operandExp) ?? $"cast({getExp(operandExp)} as timestamp)";
                            case "System.Decimal": return $"cast({getExp(operandExp)} as decimal(36,18))";
                            case "System.Double": return $"cast({getExp(operandExp)} as double)";
                            case "System.Int16": return $"cast({getExp(operandExp)} as smallint)";
                            case "System.Int32": return $"cast({getExp(operandExp)} as integer)";
                            case "System.Int64": return $"cast({getExp(operandExp)} as bigint)";
                            case "System.SByte": return $"cast({getExp(operandExp)} as tinyint)";
                            case "System.Single": return $"cast({getExp(operandExp)} as float)";
                            case "System.String": return $"cast({getExp(operandExp)} as text)";
                            case "System.UInt16": return $"cast({getExp(operandExp)} as usmallint)";
                            case "System.UInt32": return $"cast({getExp(operandExp)} as uinteger)";
                            case "System.UInt64": return $"cast({getExp(operandExp)} as ubigint)";
                            case "System.Guid": return $"cast({getExp(operandExp)} as uuid)";
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
                                case "System.Byte": return $"cast({getExp(callExp.Arguments[0])} as utinyint)";
                                case "System.Char": return $"substr(cast({getExp(callExp.Arguments[0])} as char), 1, 1)";
                                case "System.DateTime": return ExpressionConstDateTime(callExp.Arguments[0]) ?? $"cast({getExp(callExp.Arguments[0])} as timestamp)";
                                case "System.Decimal": return $"cast({getExp(callExp.Arguments[0])} as decimal(36,18))";
                                case "System.Double": return $"cast({getExp(callExp.Arguments[0])} as double)";
                                case "System.Int16": return $"cast({getExp(callExp.Arguments[0])} as smallint)";
                                case "System.Int32": return $"cast({getExp(callExp.Arguments[0])} as integer)";
                                case "System.Int64": return $"cast({getExp(callExp.Arguments[0])} as bigint)";
                                case "System.SByte": return $"cast({getExp(callExp.Arguments[0])} as tinyint)";
                                case "System.Single": return $"cast({getExp(callExp.Arguments[0])} as float)";
                                case "System.UInt16": return $"cast({getExp(callExp.Arguments[0])} as usmallint)";
                                case "System.UInt32": return $"cast({getExp(callExp.Arguments[0])} as uinteger)";
                                case "System.UInt64": return $"cast({getExp(callExp.Arguments[0])} as ubigint)";
                                case "System.Guid": return $"cast({getExp(callExp.Arguments[0])} as uuid)";
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
                            if (callExp.Object != null)
                            {
                                if (callExp.Object.Type.NullableTypeOrThis().IsEnum)
                                {
                                    tsc.SetMapColumnTmp(null);
                                    var oldMapType = tsc.SetMapTypeReturnOld(typeof(string));
                                    var enumStr = ExpressionLambdaToSql(callExp.Object, tsc);
                                    tsc.SetMapColumnTmp(null).SetMapTypeReturnOld(oldMapType);
                                    return enumStr;
								}
								var value = ExpressionGetValue(callExp.Object, out var success);
								if (success) return formatSql(value, typeof(string), null, null);
								return callExp.Arguments.Count == 0 ? $"({getExp(callExp.Object)})::text" : null;
                            }
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
                                    return $"substring({getExp(callExp.Arguments[0])}, 1, 1)";
                            }
                        }
                    }
                    if (objType == null) objType = callExp.Method.DeclaringType;
                    if (objType != null || objType.IsArrayOrList())
                    {
                        string left = null;
                        switch (callExp.Method.Name)
                        {
                            case "Any":
                                left = objExp == null ? null : getExp(objExp);
                                if (left.StartsWith("(") || left.EndsWith(")")) left = $"[{left.TrimStart('(').TrimEnd(')')}]";
                                return $"(case when {left} is null then 0 else len({left}) end > 0)";
                            case "Contains":
                                tsc.SetMapColumnTmp(null);
                                var args1 = getExp(callExp.Arguments[argIndex]);
                                var oldMapType = tsc.SetMapTypeReturnOld(tsc.mapTypeTmp);
                                var oldDbParams = objExp?.NodeType == ExpressionType.MemberAccess ? tsc.SetDbParamsReturnOld(null) : null; //#900 UseGenerateCommandParameterWithLambda(true) 子查询 bug、以及 #1173 参数化 bug
                                tsc.isNotSetMapColumnTmp = true;
                                left = objExp == null ? null : getExp(objExp);
                                tsc.isNotSetMapColumnTmp = false;
                                tsc.SetMapColumnTmp(null).SetMapTypeReturnOld(oldMapType);
                                if (oldDbParams != null) tsc.SetDbParamsReturnOld(oldDbParams);
                                //判断 in 或 array @> array
                                if (left.StartsWith("[") && left.EndsWith("]"))
                                    return $"({args1}) in ({left.TrimStart('[').TrimEnd(']')})";
                                if (left.StartsWith("(") && left.EndsWith(")")) //在各大 Provider AdoProvider 中已约定，500元素分割, 3空格\r\n4空格
                                    return $"(({args1}) in {left.Replace(",   \r\n    \r\n", $") \r\n OR ({args1}) in (")})";
                                return $"list_contains({left}, {args1})";
                            case "Concat":
                                left = objExp == null ? null : getExp(objExp);
                                if (left.StartsWith("(") || left.EndsWith(")")) left = $"[{left.TrimStart('(').TrimEnd(')')}]";
                                var right2 = getExp(callExp.Arguments[argIndex]);
                                if (right2.StartsWith("(") || right2.EndsWith(")")) right2 = $"[{right2.TrimStart('(').TrimEnd(')')}]";
                                return $"list_concat({left}, {right2})";
                            case "GetLength":
                            case "GetLongLength":
                            case "Length":
                            case "Count":
                                left = objExp == null ? null : getExp(objExp);
                                if (left.StartsWith("(") || left.EndsWith(")")) left = $"[{left.TrimStart('(').TrimEnd(')')}]";
                                return $"case when {left} is null then 0 else len({left}) end";
                        }
                        if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                        {
                            left = objExp == null ? null : getExp(objExp);
                            switch (callExp.Method.Name)
                            {
                                case "get_Item": return $"element_at({left},{getExp(callExp.Arguments[argIndex])})[1]";
                                case "ContainsKey": return $"len(element_at({left},{getExp(callExp.Arguments[argIndex])})) > 0";
                                case "GetLength":
                                case "GetLongLength":
                                case "Count": return $"cardinality({left})";
                                case "Keys": return $"map_keys({left})";
                                case "Values": return $"map_values({left})";
                            }
                        }
                    }
                    break;
                case ExpressionType.MemberAccess:
                    var memExp = exp as MemberExpression;
                    var memParentExp = memExp.Expression?.Type;
                    if (memParentExp?.FullName == "System.Byte[]") return null;
                    if (memParentExp != null)
                    {
                        if (memParentExp.IsArrayOrList())
                        {
                            var left = getExp(memExp.Expression);
                            if (left.StartsWith("(") || left.EndsWith(")")) left = $"[{left.TrimStart('(').TrimEnd(')')}]";
                            switch (memExp.Member.Name)
                            {
                                case "Length":
                                case "Count": return $"case when {left} is null then 0 else len({left}) end";
                            }
                        }
                        if (memParentExp.IsGenericType && memParentExp.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                        {
                            var left = getExp(memExp.Expression);
                            switch (memExp.Member.Name)
                            {
                                case "Count": return $"cardinality({left})";
                                case "Keys": return $"map_keys({left})";
                                case "Values": return $"map_values({left})";
                            }
                        }
                    }
                    break;
                case ExpressionType.NewArrayInit:
                    var arrExp = exp as NewArrayExpression;
                    var arrSb = new StringBuilder();
                    arrSb.Append("[");
                    for (var a = 0; a < arrExp.Expressions.Count; a++)
                    {
                        if (a > 0) arrSb.Append(",");
                        arrSb.Append(getExp(arrExp.Expressions[a]));
                    }
                    if (arrSb.Length == 1) arrSb.Append("NULL");
                    return arrSb.Append("]").ToString();
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
                    case "Today": return "current_date";
                    case "MinValue": return "timestamp '0001-01-01 00:00:00.000'";
                    case "MaxValue": return "timestamp '9999-12-31 23:59:59.999'";
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Date": return $"({left})::date";
                case "TimeOfDay": return $"strftime({left},'%H:%M:%S')::time";
                case "DayOfWeek": return $"dayofweek({left})";
                case "Day": return $"day({left})";
                case "DayOfYear": return $"dayofyear({left})";
                case "Month": return $"month({left})";
                case "Year": return $"year({left})";
                case "Hour": return $"hour({left})";
                case "Minute": return $"minute({left})";
                case "Second": return $"second({left})";
                case "Millisecond": return $"millisecond({left})";
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
                        if (exp.Arguments.Count == 1 && exp.Arguments[0].NodeType == ExpressionType.NewArrayInit && exp.Arguments[0] is NewArrayExpression concatNewArrExp)
                            return _common.StringConcat(concatNewArrExp.Expressions.Select(a => getExp(a)).ToArray(), null);
                        return _common.StringConcat(exp.Arguments.Select(a => getExp(a)).ToArray(), null);
                    case "Format":
                        if (exp.Arguments[0].NodeType != ExpressionType.Constant) throw new Exception(CoreErrorStrings.Not_Implemented_Expression_ParameterUseConstant(exp, exp.Arguments[0]));
                        var expArgsHack = exp.Arguments.Count == 2 && exp.Arguments[1].NodeType == ExpressionType.NewArrayInit ?
                            (exp.Arguments[1] as NewArrayExpression).Expressions : exp.Arguments.Where((a, z) => z > 0);
                        //3个 {} 时，Arguments 解析出来是分开的
                        //4个 {} 时，Arguments[1] 只能解析这个出来，然后里面是 NewArray []
                        var expArgs = expArgsHack.Select(a =>
                        {
                            var atype = (a as UnaryExpression)?.Operand.Type.NullableTypeOrThis() ?? a.Type.NullableTypeOrThis();
                            if (atype == typeof(string)) return $"'||{_common.IsNull(ExpressionLambdaToSql(a, tsc), "''")}||'";
                            return $"'||{_common.IsNull($"({ExpressionLambdaToSql(a, tsc)})::text", "''")}||'";
                        }).ToArray();
                        return string.Format(ExpressionLambdaToSql(exp.Arguments[0], tsc), expArgs);
                    case "Join":
                        if (exp.IsStringJoin(out var tolistObjectExp, out var toListMethod, out var toListArgs1))
                        {
                            var newToListArgs0 = Expression.Call(tolistObjectExp, toListMethod,
                                Expression.Lambda(
                                    Expression.Call(
                                        typeof(SqlExtExtensions).GetMethod("StringJoinPgsqlGroupConcat"),
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
                        var leftLike = exp.Object.NodeType == ExpressionType.MemberAccess ? left : $"({left})";
                        var args0Value = getExp(exp.Arguments[0]);
                        if (args0Value == "NULL") return $"{leftLike} IS NULL";
                        if (exp.Method.Name == "StartsWith") return $"{left} ^@ ({args0Value})";
                        if (args0Value.Contains("%"))
                        {
                            if (exp.Method.Name == "EndsWith") return $"strpos({left}, {args0Value}) = length({left})-length({args0Value})+1";
                            return $"strpos({left}, {args0Value}) > 0";
                        }
                        if (exp.Method.Name == "EndsWith") return $"{leftLike} LIKE {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"('%' || ({args0Value})::text)")}";
                        if (args0Value.StartsWith("'") && args0Value.EndsWith("'")) return $"{leftLike} LIKE {args0Value.Insert(1, "%").Insert(args0Value.Length, "%")}";
                        return $"{leftLike} LIKE ('%' || ({args0Value})::text || '%')";
                    case "ToLower": return $"lower({left})";
                    case "ToUpper": return $"upper({left})";
                    case "Substring":
                        var substrArgs1 = getExp(exp.Arguments[0]);
                        if (long.TryParse(substrArgs1, out var testtrylng1)) substrArgs1 = (testtrylng1 + 1).ToString();
                        else substrArgs1 += "+1";
                        if (exp.Arguments.Count == 1) return $"substring({left}, {substrArgs1})";
                        return $"substring({left}, {substrArgs1}, {getExp(exp.Arguments[1])})";
                    case "IndexOf":
                        var indexOfFindStr = getExp(exp.Arguments[0]);
                        //if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32") {
                        //  var locateArgs1 = getExp(exp.Arguments[1]);
                        //  if (long.TryParse(locateArgs1, out var testtrylng2)) locateArgs1 = (testtrylng2 + 1).ToString();
                        //  else locateArgs1 += "+1";
                        //  return $"(instr({left}, {indexOfFindStr}, {locateArgs1})-1)";
                        //}
                        return $"(strpos({left}, {indexOfFindStr})-1)";
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
                    return $"round({getExp(exp.Arguments[0])}, 2)";
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
                case "Truncate": return $"trunc({getExp(exp.Arguments[0])})";
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
                    case "Compare": return $"(epoch({getExp(exp.Arguments[0])})-epoch({getExp(exp.Arguments[1])}))";
                    case "DaysInMonth": return $"day(last_day(cast({getExp(exp.Arguments[0])}||'-'||{getExp(exp.Arguments[1])}||'-01' as date)))";
                    case "Equals": return $"({getExp(exp.Arguments[0])} = {getExp(exp.Arguments[1])})";

                    case "IsLeapYear":
                        var isLeapYearArgs1 = getExp(exp.Arguments[0]);
                        return $"(({isLeapYearArgs1})%4=0 AND ({isLeapYearArgs1})%100<>0 OR ({isLeapYearArgs1})%400=0)";

                    case "Parse": return ExpressionConstDateTime(exp.Arguments[0]) ?? $"cast({getExp(exp.Arguments[0])} as timestamp)";
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return ExpressionConstDateTime(exp.Arguments[0]) ?? $"cast({getExp(exp.Arguments[0])} as timestamp)";
                }
            }
            else
            {
                var left = getExp(exp.Object);
                var args1 = exp.Arguments.Count == 0 ? null : getExp(exp.Arguments[0]);
                switch (exp.Method.Name)
                {
                    case "AddDays": return $"date_add({left},cast(({args1})||' days' as interval))";
                    case "AddHours": return $"date_add({left},cast(({args1})||' hours' as interval))";
                    case "AddMilliseconds": return $"date_add({left},cast(({args1})||' milliseconds' as interval))";
                    case "AddMinutes": return $"date_add({left},cast(({args1})||' minutes' as interval))";
                    case "AddMonths": return $"date_add({left},cast(({args1})||' months' as interval))";
                    case "AddSeconds": return $"date_add({left},cast(({args1})||' seconds' as interval))";
                    case "AddTicks": return $"date_add({left},cast((({args1})/10)||' microseconds' as interval))";
                    case "AddYears": return $"date_add({left},cast(({args1})||' years' as interval))";
                    case "Subtract":
                        switch ((exp.Arguments[0].Type.IsNullableType() ? exp.Arguments[0].Type.GetGenericArguments().FirstOrDefault() : exp.Arguments[0].Type).FullName)
                        {
                            case "System.DateTime": return $"(epoch({left})-epoch({args1}))";
                        }
                        break;
                    case "Equals": return $"({left} = {args1})";
                    case "CompareTo": return $"(epoch({left})-epoch({args1}))";
                    case "ToString":
                        if (exp.Arguments.Count == 0) return $"strftime({left},'%Y-%m-%d %H:%M:%S')";
                        switch (args1)
                        {
                            case "'yyyy-MM-dd HH:mm:ss'": return $"strftime({left},'%Y-%m-%d %H:%M:%S')";
                            case "'yyyy-MM-dd HH:mm'": return $"strftime({left},'%Y-%m-%d %H:%M')";
                            case "'yyyy-MM-dd HH'": return $"strftime({left},'%Y-%m-%d %H')";
                            case "'yyyy-MM-dd'": return $"strftime({left},'%Y-%m-%d')";
                            case "'yyyy-MM'": return $"strftime({left},'%Y-%m')";
                            case "'yyyyMMddHHmmss'": return $"strftime({left},'%Y%m%d%H%M%S')";
                            case "'yyyyMMddHHmm'": return $"strftime({left},'%Y%m%d%H%M')";
                            case "'yyyyMMddHH'": return $"strftime({left},'%Y%m%d%H')";
                            case "'yyyyMMdd'": return $"strftime({left},'%Y%m%d')";
                            case "'yyyyMM'": return $"strftime({left},'%Y%m')";
                            case "'yyyy'": return $"strftime({left},'%Y')";
                            case "'HH:mm:ss'": return $"strftime({left},'%H:%M:%S')";
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
                                case "yy": argsSpts[a] = $"substr(strftime({left},'%Y'),3,2)"; break;
                                case "M": argsSpts[a] = $"strftime({left},'%-m')"; break;
                                case "d": argsSpts[a] = $"strftime({left},'%-d')"; break;
                                case "H": argsSpts[a] = $"strftime({left},'%-H')"; break;
                                case "hh": argsSpts[a] = $"strftime({left},'%I')"; break;
                                case "h": argsSpts[a] = $"strftime({left},'%-I')"; break;
                                case "m": argsSpts[a] = $"strftime({left},'%-M')"; break;
                                case "s": argsSpts[a] = $"strftime({left},'%-S')"; break;
                                case "tt": argsSpts[a] = $"strftime({left},'%p')"; break;
                                case "t": argsSpts[a] = $"substr(strftime({left},'%p'),1,1)"; break;
                                default:
                                    var argsSptsA = argsSpts[a];
                                    if (argsSptsA.StartsWith("'")) argsSptsA = argsSptsA.Substring(1);
                                    if (argsSptsA.EndsWith("'")) argsSptsA = argsSptsA.Remove(argsSptsA.Length - 1);
                                    argsSpts[a] = argsFinds.Any(m => argsSptsA.Contains(m)) ? $"strftime({left},'{argsSptsA}')" : $"'{argsSptsA}'";
                                    break;
                            }
                        }
                        if (argsSpts.Length > 0) args1 = $"({string.Join(" || ", argsSpts.Where(a => a != "''"))})";
                        return args1.Replace("%_a1", "%m").Replace("%_a2", "%d").Replace("%_a3", "%H").Replace("%_a4", "%M");
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
                    case "ToByte": return $"cast({getExp(exp.Arguments[0])} as utinyint)";
                    case "ToChar": return $"substr(cast({getExp(exp.Arguments[0])} as char), 1, 1)";
                    case "ToDateTime": return ExpressionConstDateTime(exp.Arguments[0]) ?? $"cast({getExp(exp.Arguments[0])} as timestamp)";
                    case "ToDecimal": return $"cast({getExp(exp.Arguments[0])} as decimal(36,18))";
                    case "ToDouble": return $"cast({getExp(exp.Arguments[0])} as double)";
                    case "ToInt16": return $"cast({getExp(exp.Arguments[0])} as smallint)";
                    case "ToInt32": return $"cast({getExp(exp.Arguments[0])} as integer)";
                    case "ToInt64": return $"cast({getExp(exp.Arguments[0])} as bigint)";
                    case "ToSByte": return $"cast({getExp(exp.Arguments[0])} as tinyint)";
                    case "ToSingle": return $"cast({getExp(exp.Arguments[0])} as float)";
                    case "ToString": return $"cast({getExp(exp.Arguments[0])} as text)";
                    case "ToUInt16": return $"cast({getExp(exp.Arguments[0])} as usmallint)";
                    case "ToUInt32": return $"cast({getExp(exp.Arguments[0])} as uinteger)";
                    case "ToUInt64": return $"cast({getExp(exp.Arguments[0])} as ubigint)";
                }
            }
            return null;
        }
    }
}
