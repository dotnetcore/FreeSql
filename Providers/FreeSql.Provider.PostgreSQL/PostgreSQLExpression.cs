using FreeSql.Internal;
using FreeSql.Internal.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.PostgreSQL
{
    class PostgreSQLExpression : CommonExpression
    {

        public PostgreSQLExpression(CommonUtils common) : base(common) { }

        public override string ExpressionLambdaToSqlOther(Expression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            switch (exp.NodeType)
            {
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
                            case "System.String": return $"({getExp(operandExp)})::varchar";
                            case "System.UInt16": return $"({getExp(operandExp)})::int2";
                            case "System.UInt32": return $"({getExp(operandExp)})::int4";
                            case "System.UInt64": return $"({getExp(operandExp)})::int8";
                            case "System.Guid": return $"({getExp(operandExp)})::uuid";
                        }
                    }
                    break;
                case ExpressionType.ArrayLength:
                    var arrOperExp = getExp((exp as UnaryExpression).Operand);
                    if (arrOperExp.StartsWith("(") || arrOperExp.EndsWith(")")) return $"array_length(array[{arrOperExp.TrimStart('(').TrimEnd(')')}],1)";
                    return $"case when {arrOperExp} is null then 0 else array_length({arrOperExp},1) end";
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
                            break;
                        case "Next":
                            if (callExp.Object?.Type == typeof(Random)) return "(random()*1000000000)::int4";
                            break;
                        case "NextDouble":
                            if (callExp.Object?.Type == typeof(Random)) return "random()";
                            break;
                        case "Random":
                            if (callExp.Method.DeclaringType.IsNumberType()) return "random()";
                            break;
                        case "ToString":
                            if (callExp.Object != null) return $"({getExp(callExp.Object)})::varchar";
                            break;
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
                    if (objType != null || objType.IsArray || typeof(IList).IsAssignableFrom(callExp.Method.DeclaringType))
                    {
                        var left = objExp == null ? null : getExp(objExp);
                        switch (objType.FullName)
                        {
                            case "Newtonsoft.Json.Linq.JToken":
                            case "Newtonsoft.Json.Linq.JObject":
                            case "Newtonsoft.Json.Linq.JArray":
                                switch (callExp.Method.Name)
                                {
                                    case "Any": return $"(jsonb_array_length(coalesce({left},'[]')) > 0)";
                                    case "Contains":
                                        var json = getExp(callExp.Arguments[argIndex]);
                                        if (objType == typeof(JArray))
                                            return $"(coalesce({left},'[]') ? ({json})::varchar)";
                                        if (json.StartsWith("'") && json.EndsWith("'"))
                                            return $"(coalesce({left},'{{}}') @> {_common.FormatSql("{0}", JToken.Parse(json.Trim('\'')))})";
                                        return $"(coalesce({left},'{{}}') @> ({json})::jsonb)";
                                    case "ContainsKey": return $"(coalesce({left},'{{}}') ? {getExp(callExp.Arguments[argIndex])})";
                                    case "Concat":
                                        var right2 = getExp(callExp.Arguments[argIndex]);
                                        return $"(coalesce({left},'{{}}') || {right2})";
                                    case "LongCount":
                                    case "Count": return $"jsonb_array_length(coalesce({left},'[]'))";
                                    case "Parse":
                                        var json2 = getExp(callExp.Arguments[argIndex]);
                                        if (json2.StartsWith("'") && json2.EndsWith("'")) return _common.FormatSql("{0}", JToken.Parse(json2.Trim('\'')));
                                        return $"({json2})::jsonb";
                                }
                                break;
                        }
                        if (objType.FullName == typeof(Dictionary<string, string>).FullName)
                        {
                            switch (callExp.Method.Name)
                            {
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
                                if (left.StartsWith("(") || left.EndsWith(")")) left = $"array[{left.TrimStart('(').TrimEnd(')')}]";
                                return $"(case when {left} is null then 0 else array_length({left},1) end > 0)";
                            case "Contains":
                                //判断 in 或 array @> array
                                var right1 = getExp(callExp.Arguments[argIndex]);
                                if (left.StartsWith("array[") || left.EndsWith("]"))
                                    return $"{right1} in ({left.Substring(6, left.Length - 7)})";
                                if (left.StartsWith("(") || left.EndsWith(")"))
                                    return $"{right1} in {left}";
                                if (right1.StartsWith("(") || right1.EndsWith(")")) right1 = $"array[{right1.TrimStart('(').TrimEnd(')')}]";
                                return $"({left} @> array[{right1}])";
                            case "Concat":
                                if (left.StartsWith("(") || left.EndsWith(")")) left = $"array[{left.TrimStart('(').TrimEnd(')')}]";
                                var right2 = getExp(callExp.Arguments[argIndex]);
                                if (right2.StartsWith("(") || right2.EndsWith(")")) right2 = $"array[{right2.TrimStart('(').TrimEnd(')')}]";
                                return $"({left} || {right2})";
                            case "GetLength":
                            case "GetLongLength":
                            case "Length":
                            case "Count":
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
                        switch (memParentExp.FullName)
                        {
                            case "Newtonsoft.Json.Linq.JToken":
                            case "Newtonsoft.Json.Linq.JObject":
                            case "Newtonsoft.Json.Linq.JArray":
                                var left = getExp(memExp.Expression);
                                switch (memExp.Member.Name)
                                {
                                    case "Count": return $"jsonb_array_length(coalesce({left},'[]'))";
                                }
                                break;
                        }
                        if (memParentExp.FullName == typeof(Dictionary<string, string>).FullName)
                        {
                            var left = getExp(memExp.Expression);
                            switch (memExp.Member.Name)
                            {
                                case "Count": return $"case when {left} is null then 0 else array_length(akeys({left}),1) end";
                                case "Keys": return $"akeys({left})";
                                case "Values": return $"avals({left})";
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
                    case "Now": return "current_timestamp";
                    case "UtcNow": return "(current_timestamp at time zone 'UTC')";
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
                    case "Concat":
                        return _common.StringConcat(exp.Arguments.Select(a => getExp(a)).ToArray(), null);
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
                                exp.Arguments[1].Type == typeof(StringComparison) && getExp(exp.Arguments[0]).Contains("IgnoreCase")) likeOpt = "ILIKE";
                        }
                        if (exp.Method.Name == "StartsWith") return $"({left}) {likeOpt} {(args0Value.EndsWith("'") ? args0Value.Insert(args0Value.Length - 1, "%") : $"(({args0Value})::varchar || '%')")}";
                        if (exp.Method.Name == "EndsWith") return $"({left}) {likeOpt} {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"('%' || ({args0Value})::varchar)")}";
                        if (args0Value.StartsWith("'") && args0Value.EndsWith("'")) return $"({left}) {likeOpt} {args0Value.Insert(1, "%").Insert(args0Value.Length, "%")}";
                        return $"({left}) {likeOpt} ('%' || ({args0Value})::varchar || '%')";
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
                    case "Equals": return $"({left} = ({getExp(exp.Arguments[0])})::varchar)";
                }
            }
            throw new Exception($"PostgreSQLExpression 未实现函数表达式 {exp} 解析");
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
                case "Truncate": return $"trunc({getExp(exp.Arguments[0])}, 0)";
            }
            throw new Exception($"PostgreSQLExpression 未实现函数表达式 {exp} 解析");
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
                    case "Add": return $"(({left})::timestamp+(({args1})||' microseconds')::interval)";
                    case "AddDays": return $"(({left})::timestamp+(({args1})||' day')::interval)";
                    case "AddHours": return $"(({left})::timestamp+(({args1})||' hour')::interval)";
                    case "AddMilliseconds": return $"(({left})::timestamp+(({args1})||' milliseconds')::interval)";
                    case "AddMinutes": return $"(({left})::timestamp+(({args1})||' minute')::interval)";
                    case "AddMonths": return $"(({left})::timestamp+(({args1})||' month')::interval)";
                    case "AddSeconds": return $"(({left})::timestamp+(({args1})||' second')::interval)";
                    case "AddTicks": return $"(({left})::timestamp+(({args1})/10||' microseconds')::interval)";
                    case "AddYears": return $"(({left})::timestamp+(({args1})||' year')::interval)";
                    case "Subtract":
                        switch ((exp.Arguments[0].Type.IsNullableType() ? exp.Arguments[0].Type.GenericTypeArguments.FirstOrDefault() : exp.Arguments[0].Type).FullName)
                        {
                            case "System.DateTime": return $"(extract(epoch from ({left})::timestamp-({args1})::timestamp)*1000000)";
                            case "System.TimeSpan": return $"(({left})::timestamp-(({args1})||' microseconds')::interval)";
                        }
                        break;
                    case "Equals": return $"({left} = ({getExp(exp.Arguments[0])})::timestamp)";
                    case "CompareTo": return $"extract(epoch from ({left})::timestamp-({getExp(exp.Arguments[0])})::timestamp)";
                    case "ToString": return $"to_char({left}, 'YYYY-MM-DD HH24:MI:SS.US')";
                }
            }
            throw new Exception($"PostgreSQLExpression 未实现函数表达式 {exp} 解析");
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
                    case "Equals": return $"({left} = {getExp(exp.Arguments[0])})";
                    case "CompareTo": return $"({left}-({getExp(exp.Arguments[0])}))";
                    case "ToString": return $"({left})::varchar";
                }
            }
            throw new Exception($"PostgreSQLExpression 未实现函数表达式 {exp} 解析");
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
                    case "ToString": return $"({getExp(exp.Arguments[0])})::varchar";
                    case "ToUInt16": return $"({getExp(exp.Arguments[0])})::int2";
                    case "ToUInt32": return $"({getExp(exp.Arguments[0])})::int4";
                    case "ToUInt64": return $"({getExp(exp.Arguments[0])})::int8";
                }
            }
            throw new Exception($"PostgreSQLExpression 未实现函数表达式 {exp} 解析");
        }
    }
}
