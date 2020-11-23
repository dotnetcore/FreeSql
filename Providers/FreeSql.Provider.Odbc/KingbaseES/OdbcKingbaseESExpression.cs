using FreeSql.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Odbc.KingbaseES
{
    class OdbcKingbaseESExpression : CommonExpression
    {

        public OdbcKingbaseESExpression(CommonUtils common) : base(common) { }

        public override string ExpressionLambdaToSqlOther(Expression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            switch (exp.NodeType)
            {
                case ExpressionType.ArrayLength:
                    var arrOper = (exp as UnaryExpression)?.Operand;
                    var arrOperExp = getExp(arrOper);
                    if (arrOperExp.StartsWith("(") || arrOperExp.EndsWith(")")) return $"array_length(array[{arrOperExp.TrimStart('(').TrimEnd(')')}],1)";
                    if (arrOper.Type == typeof(byte[])) return $"octet_length({getExp(arrOper)})";
                    return $"case when {arrOperExp} is null then 0 else array_length({arrOperExp},1) end";
                case ExpressionType.Convert:
                    var operandExp = (exp as UnaryExpression)?.Operand;
                    var gentype = exp.Type.NullableTypeOrThis();
                    if (gentype != operandExp.Type.NullableTypeOrThis())
                    {
                        switch (exp.Type.NullableTypeOrThis().ToString())
                        {
                            case "System.Boolean": return $"(({getExp(operandExp)})::varchar not in ('0','false','f','no'))";
                            case "System.Byte": return $"({getExp(operandExp)})::int2";
                            case "System.Char": return $"substr(({getExp(operandExp)})::char, 1, 1)";
                            case "System.DateTime": return $"({getExp(operandExp)})::timestamp";
                            case "System.Decimal": return $"({getExp(operandExp)})::numeric";
                            case "System.Double": return $"({getExp(operandExp)})::float8";
                            case "System.Int16": return $"({getExp(operandExp)})::int2";
                            case "System.Int32": return $"({getExp(operandExp)})::int4";
                            case "System.Int64": return $"({getExp(operandExp)})::int8";
                            case "System.SByte": return $"({getExp(operandExp)})::int2";
                            case "System.Single": return $"({getExp(operandExp)})::float4";
                            case "System.String": return $"({getExp(operandExp)})::text";
                            case "System.UInt16": return $"({getExp(operandExp)})::int2";
                            case "System.UInt32": return $"({getExp(operandExp)})::int4";
                            case "System.UInt64": return $"({getExp(operandExp)})::int8";
                            case "System.Guid": return $"({getExp(operandExp)})::uuid";
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
                                case "System.Boolean": return $"(({getExp(callExp.Arguments[0])})::varchar not in ('0','false','f','no'))";
                                case "System.Byte": return $"({getExp(callExp.Arguments[0])})::int2";
                                case "System.Char": return $"substr(({getExp(callExp.Arguments[0])})::char, 1, 1)";
                                case "System.DateTime": return $"({getExp(callExp.Arguments[0])})::timestamp";
                                case "System.Decimal": return $"({getExp(callExp.Arguments[0])})::numeric";
                                case "System.Double": return $"({getExp(callExp.Arguments[0])})::float8";
                                case "System.Int16": return $"({getExp(callExp.Arguments[0])})::int2";
                                case "System.Int32": return $"({getExp(callExp.Arguments[0])})::int4";
                                case "System.Int64": return $"({getExp(callExp.Arguments[0])})::int8";
                                case "System.SByte": return $"({getExp(callExp.Arguments[0])})::int2";
                                case "System.Single": return $"({getExp(callExp.Arguments[0])})::float4";
                                case "System.UInt16": return $"({getExp(callExp.Arguments[0])})::int2";
                                case "System.UInt32": return $"({getExp(callExp.Arguments[0])})::int4";
                                case "System.UInt64": return $"({getExp(callExp.Arguments[0])})::int8";
                                case "System.Guid": return $"({getExp(callExp.Arguments[0])})::uuid";
                            }
                            break;
                        case "NewGuid":
                            return null;
                        case "Next":
                            if (callExp.Object?.Type == typeof(Random)) return "(random()*1000000000)::int4";
                            return null;
                        case "NextDouble":
                            if (callExp.Object?.Type == typeof(Random)) return "random()";
                            return null;
                        case "Random":
                            if (callExp.Method.DeclaringType.IsNumberType()) return "random()";
                            return null;
                        case "ToString":
                            if (callExp.Object != null) return callExp.Arguments.Count == 0 ? $"({getExp(callExp.Object)})::text" : null;
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
                        string left = null;
                        if (objType == typeof(Dictionary<string, string>))
                        {
                            left = objExp == null ? null : getExp(objExp);
                            switch (callExp.Method.Name)
                            {
                                case "get_Item": return $"{left}->{getExp(callExp.Arguments[argIndex])}";
                                case "Contains":
                                    var right = getExp(callExp.Arguments[argIndex]);
                                    return $"({left} @> ({right}))";
                                case "ContainsKey": return $"({left} ? {getExp(callExp.Arguments[argIndex])})";
                                case "Concat": return $"({left} || {getExp(callExp.Arguments[argIndex])})";
                                case "GetLength":
                                case "GetLongLength":
                                case "Count": return $"case when {left} is null then 0 else array_length(akeys({left}),1) end";
                                case "Keys": return $"akeys({left})";
                                case "Values": return $"avals({left})";
                            }
                        }
                        switch (callExp.Method.Name)
                        {
                            case "Any":
                                left = objExp == null ? null : getExp(objExp);
                                if (left.StartsWith("(") || left.EndsWith(")")) left = $"array[{left.TrimStart('(').TrimEnd(')')}]";
                                return $"(case when {left} is null then 0 else array_length({left},1) end > 0)";
                            case "Contains":
                                tsc.SetMapColumnTmp(null);
                                var args1 = getExp(callExp.Arguments[argIndex]);
                                var oldMapType = tsc.SetMapTypeReturnOld(tsc.mapTypeTmp);
                                var oldDbParams = tsc.SetDbParamsReturnOld(null);
                                left = objExp == null ? null : getExp(objExp);
                                tsc.SetMapColumnTmp(null).SetMapTypeReturnOld(oldMapType);
                                tsc.SetDbParamsReturnOld(oldDbParams);
                                //判断 in 或 array @> array
                                if (left.StartsWith("array[") || left.EndsWith("]"))
                                    return $"({args1}) in ({left.Substring(6, left.Length - 7)})";
                                if (left.StartsWith("(") || left.EndsWith(")")) //在各大 Provider AdoProvider 中已约定，500元素分割, 3空格\r\n4空格
                                    return $"(({args1}) in {left.Replace(",   \r\n    \r\n", $") \r\n OR ({args1}) in (")})";
                                if (args1.StartsWith("(") || args1.EndsWith(")")) args1 = $"array[{args1.TrimStart('(').TrimEnd(')')}]";
                                args1 = $"array[{args1}]";
                                if (objExp != null)
                                {
                                    var dbinfo = _common._orm.CodeFirst.GetDbInfo(objExp.Type);
                                    if (dbinfo != null) args1 = $"{args1}::{dbinfo.dbtype}";
                                }
                                return $"({left} @> {args1})";
                            case "Concat":
                                left = objExp == null ? null : getExp(objExp);
                                if (left.StartsWith("(") || left.EndsWith(")")) left = $"array[{left.TrimStart('(').TrimEnd(')')}]";
                                var right2 = getExp(callExp.Arguments[argIndex]);
                                if (right2.StartsWith("(") || right2.EndsWith(")")) right2 = $"array[{right2.TrimStart('(').TrimEnd(')')}]";
                                return $"({left} || {right2})";
                            case "GetLength":
                            case "GetLongLength":
                            case "Length":
                            case "Count":
                                left = objExp == null ? null : getExp(objExp);
                                if (left.StartsWith("(") || left.EndsWith(")")) left = $"array[{left.TrimStart('(').TrimEnd(')')}]";
                                return $"case when {left} is null then 0 else array_length({left},1) end";
                        }
                    }
                    break;
                case ExpressionType.MemberAccess:
                    var memExp = exp as MemberExpression;
                    var memParentExp = memExp.Expression?.Type;
                    if (memParentExp?.FullName == "System.Byte[]") return null;
                    if (memParentExp != null)
                    {
                        if (memParentExp.IsArray == true)
                        {
                            var left = getExp(memExp.Expression);
                            if (left.StartsWith("(") || left.EndsWith(")")) left = $"array[{left.TrimStart('(').TrimEnd(')')}]";
                            switch (memExp.Member.Name)
                            {
                                case "Length":
                                case "Count": return $"case when {left} is null then 0 else array_length({left},1) end";
                            }
                        }
                    }
                    break;
                case ExpressionType.NewArrayInit:
                    var arrExp = exp as NewArrayExpression;
                    var arrSb = new StringBuilder();
                    arrSb.Append("array[");
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
                    case "MinValue": return "'0001/1/1 0:00:00'::timestamp";
                    case "MaxValue": return "'9999/12/31 23:59:59'::timestamp";
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Date": return $"({left})::date";
                case "TimeOfDay": return $"(extract(epoch from ({left})::time)*1000000)";
                case "DayOfWeek": return $"extract(dow from ({left})::timestamp)";
                case "Day": return $"extract(day from ({left})::timestamp)";
                case "DayOfYear": return $"extract(doy from ({left})::timestamp)";
                case "Month": return $"extract(month from ({left})::timestamp)";
                case "Year": return $"extract(year from ({left})::timestamp)";
                case "Hour": return $"extract(hour from ({left})::timestamp)";
                case "Minute": return $"extract(minute from ({left})::timestamp)";
                case "Second": return $"extract(second from ({left})::timestamp)";
                case "Millisecond": return $"(extract(milliseconds from ({left})::timestamp)-extract(second from ({left})::timestamp)*1000)";
                case "Ticks": return $"(extract(epoch from ({left})::timestamp)*10000000+621355968000000000)";
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
                    case "MinValue": return "-922337203685477580"; //微秒 Ticks / 10
                    case "MaxValue": return "922337203685477580";
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Days": return $"floor(({left})/{(long)1000000 * 60 * 60 * 24})";
                case "Hours": return $"floor(({left})/{(long)1000000 * 60 * 60}%24)";
                case "Milliseconds": return $"(floor(({left})/1000)::int8%1000)";
                case "Minutes": return $"(floor(({left})/{(long)1000000 * 60})::int8%60)";
                case "Seconds": return $"(floor(({left})/1000000)::int8%60)";
                case "Ticks": return $"(({left})*10)";
                case "TotalDays": return $"(({left})/{(long)1000000 * 60 * 60 * 24})";
                case "TotalHours": return $"(({left})/{(long)1000000 * 60 * 60})";
                case "TotalMilliseconds": return $"(({left})/1000)";
                case "TotalMinutes": return $"(({left})/{(long)1000000 * 60})";
                case "TotalSeconds": return $"(({left})/1000000)";
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
                        var args0Value = getExp(exp.Arguments[0]);
                        if (args0Value == "NULL") return $"({left}) IS NULL";
                        var likeOpt = "LIKE";
                        if (exp.Arguments.Count > 1)
                        {
                            if (exp.Arguments[1].Type == typeof(bool) ||
                                exp.Arguments[1].Type == typeof(StringComparison)) likeOpt = "ILIKE";
                        }
                        if (exp.Method.Name == "StartsWith") return $"({left}) {likeOpt} {(args0Value.EndsWith("'") ? args0Value.Insert(args0Value.Length - 1, "%") : $"(({args0Value})::text || '%')")}";
                        if (exp.Method.Name == "EndsWith") return $"({left}) {likeOpt} {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"('%' || ({args0Value})::text)")}";
                        if (args0Value.StartsWith("'") && args0Value.EndsWith("'")) return $"({left}) {likeOpt} {args0Value.Insert(1, "%").Insert(args0Value.Length, "%")}";
                        return $"({left}) {likeOpt} ('%' || ({args0Value})::text || '%')";
                    case "ToLower": return $"lower({left})";
                    case "ToUpper": return $"upper({left})";
                    case "Substring":
                        var substrArgs1 = getExp(exp.Arguments[0]);
                        if (long.TryParse(substrArgs1, out var testtrylng1)) substrArgs1 = (testtrylng1 + 1).ToString();
                        else substrArgs1 += "+1";
                        if (exp.Arguments.Count == 1) return $"substr({left}, {substrArgs1})";
                        return $"substr({left}, {substrArgs1}, {getExp(exp.Arguments[1])})";
                    case "IndexOf": return $"(strpos({left}, {getExp(exp.Arguments[0])})-1)";
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
                                if (exp.Method.Name == "Trim") left = $"trim(both {getExp(argsTrim01)} from {left})";
                                if (exp.Method.Name == "TrimStart") left = $"ltrim({left},{getExp(argsTrim01)})";
                                if (exp.Method.Name == "TrimEnd") left = $"rtrim({left},{getExp(argsTrim01)})";
                            }
                        }
                        return left;
                    case "Replace": return $"replace({left}, {getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                    case "CompareTo": return $"case when {left} = {getExp(exp.Arguments[0])} then 0 when {left} > {getExp(exp.Arguments[0])} then 1 else -1 end";
                    case "Equals": return $"({left} = ({getExp(exp.Arguments[0])})::text)";
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
                case "Log":
                    if (exp.Arguments.Count > 1) return $"log({getExp(exp.Arguments[1])},{getExp(exp.Arguments[0])})";
                    return $"log(2.7182818284590451,{getExp(exp.Arguments[0])})";
                case "Log10": return $"log({getExp(exp.Arguments[0])})";
                case "Pow": return $"pow({getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
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
                    case "Compare": return $"extract(epoch from ({getExp(exp.Arguments[0])})::timestamp-({getExp(exp.Arguments[1])})::timestamp)";
                    case "DaysInMonth": return $"extract(day from ({getExp(exp.Arguments[0])} || '-' || {getExp(exp.Arguments[1])} || '-01')::timestamp+'1 month'::interval-'1 day'::interval)";
                    case "Equals": return $"(({getExp(exp.Arguments[0])})::timestamp = ({getExp(exp.Arguments[1])})::timestamp)";

                    case "IsLeapYear":
                        var isLeapYearArgs1 = getExp(exp.Arguments[0]);
                        return $"(({isLeapYearArgs1})::int8%4=0 AND ({isLeapYearArgs1})::int8%100<>0 OR ({isLeapYearArgs1})::int8%400=0)";

                    case "Parse": return $"({getExp(exp.Arguments[0])})::timestamp";
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return $"({getExp(exp.Arguments[0])})::timestamp";
                }
            }
            else
            {
                var left = getExp(exp.Object);
                var args1 = exp.Arguments.Count == 0 ? null : getExp(exp.Arguments[0]);
                switch (exp.Method.Name)
                {
                    case "Add": return $"(({left})::timestamp+((({args1})/1000)||' milliseconds')::interval)";
                    case "AddDays": return $"(({left})::timestamp+(({args1})||' day')::interval)";
                    case "AddHours": return $"(({left})::timestamp+(({args1})||' hour')::interval)";
                    case "AddMilliseconds": return $"(({left})::timestamp+(({args1})||' milliseconds')::interval)";
                    case "AddMinutes": return $"(({left})::timestamp+(({args1})||' minute')::interval)";
                    case "AddMonths": return $"(({left})::timestamp+(({args1})||' month')::interval)";
                    case "AddSeconds": return $"(({left})::timestamp+(({args1})||' second')::interval)";
                    case "AddTicks": return $"(({left})::timestamp+(({args1})/10||' microseconds')::interval)";
                    case "AddYears": return $"(({left})::timestamp+(({args1})||' year')::interval)";
                    case "Subtract":
                        switch ((exp.Arguments[0].Type.IsNullableType() ? exp.Arguments[0].Type.GetGenericArguments().FirstOrDefault() : exp.Arguments[0].Type).FullName)
                        {
                            case "System.DateTime": return $"(extract(epoch from ({left})::timestamp-({args1})::timestamp)*1000000)";
                            case "System.TimeSpan": return $"(({left})::timestamp-((({args1})/1000)||' milliseconds')::interval)";
                        }
                        break;
                    case "Equals": return $"({left} = ({args1})::timestamp)";
                    case "CompareTo": return $"extract(epoch from ({left})::timestamp-({args1})::timestamp)";
                    case "ToString":
                        if (left.EndsWith("::timestamp") == false) left = $"({left})::timestamp";
                        if (exp.Arguments.Count == 0) return $"to_char({left},'YYYY-MM-DD HH24:MI:SS.US')";
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
                    case "Compare": return $"({getExp(exp.Arguments[0])}-({getExp(exp.Arguments[1])}))";
                    case "Equals": return $"({getExp(exp.Arguments[0])} = {getExp(exp.Arguments[1])})";
                    case "FromDays": return $"(({getExp(exp.Arguments[0])})*{(long)1000000 * 60 * 60 * 24})";
                    case "FromHours": return $"(({getExp(exp.Arguments[0])})*{(long)1000000 * 60 * 60})";
                    case "FromMilliseconds": return $"(({getExp(exp.Arguments[0])})*1000)";
                    case "FromMinutes": return $"(({getExp(exp.Arguments[0])})*{(long)1000000 * 60})";
                    case "FromSeconds": return $"(({getExp(exp.Arguments[0])})*1000000)";
                    case "FromTicks": return $"(({getExp(exp.Arguments[0])})/10)";
                    case "Parse": return $"({getExp(exp.Arguments[0])})::int8";
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return $"({getExp(exp.Arguments[0])})::int8";
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
                    case "ToString": return $"({left})::varchar";
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
                    case "ToBoolean": return $"(({getExp(exp.Arguments[0])})::varchar not in ('0','false','f','no'))";
                    case "ToByte": return $"({getExp(exp.Arguments[0])})::int2";
                    case "ToChar": return $"substr(({getExp(exp.Arguments[0])})::char, 1, 1)";
                    case "ToDateTime": return $"({getExp(exp.Arguments[0])})::timestamp";
                    case "ToDecimal": return $"({getExp(exp.Arguments[0])})::numeric";
                    case "ToDouble": return $"({getExp(exp.Arguments[0])})::float8";
                    case "ToInt16": return $"({getExp(exp.Arguments[0])})::int2";
                    case "ToInt32": return $"({getExp(exp.Arguments[0])})::int4";
                    case "ToInt64": return $"({getExp(exp.Arguments[0])})::int8";
                    case "ToSByte": return $"({getExp(exp.Arguments[0])})::int2";
                    case "ToSingle": return $"({getExp(exp.Arguments[0])})::float4";
                    case "ToString": return $"({getExp(exp.Arguments[0])})::text";
                    case "ToUInt16": return $"({getExp(exp.Arguments[0])})::int2";
                    case "ToUInt32": return $"({getExp(exp.Arguments[0])})::int4";
                    case "ToUInt64": return $"({getExp(exp.Arguments[0])})::int8";
                }
            }
            return null;
        }
    }
}
