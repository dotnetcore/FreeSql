using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.MySql
{
    class MySqlExpression : CommonExpression
    {

        public MySqlExpression(CommonUtils common) : base(common) { }

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
                        switch (gentype.ToString())
                        {
                            case "System.Boolean": return $"({getExp(operandExp)} not in ('0','false'))";
                            case "System.Byte": return $"cast({getExp(operandExp)} as unsigned)";
                            case "System.Char": return $"substr(cast({getExp(operandExp)} as char), 1, 1)";
                            case "System.DateTime": return $"cast({getExp(operandExp)} as datetime)";
                            case "System.Decimal": return $"cast({getExp(operandExp)} as decimal(36,18))";
                            case "System.Double": return $"cast({getExp(operandExp)} as decimal(32,16))";
                            case "System.Int16":
                            case "System.Int32":
                            case "System.Int64":
                            case "System.SByte": return $"cast({getExp(operandExp)} as signed)";
                            case "System.Single": return $"cast({getExp(operandExp)} as decimal(14,7))";
                            case "System.String": return $"cast({getExp(operandExp)} as char)";
                            case "System.UInt16":
                            case "System.UInt32":
                            case "System.UInt64": return $"cast({getExp(operandExp)} as unsigned)";
                            case "System.Guid": return $"substr(cast({getExp(operandExp)} as char), 1, 36)";
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
                                case "System.Byte": return $"cast({getExp(callExp.Arguments[0])} as unsigned)";
                                case "System.Char": return $"substr(cast({getExp(callExp.Arguments[0])} as char), 1, 1)";
                                case "System.DateTime": return $"cast({getExp(callExp.Arguments[0])} as datetime)";
                                case "System.Decimal": return $"cast({getExp(callExp.Arguments[0])} as decimal(36,18))";
                                case "System.Double": return $"cast({getExp(callExp.Arguments[0])} as decimal(32,16))";
                                case "System.Int16":
                                case "System.Int32":
                                case "System.Int64":
                                case "System.SByte": return $"cast({getExp(callExp.Arguments[0])} as signed)";
                                case "System.Single": return $"cast({getExp(callExp.Arguments[0])} as decimal(14,7))";
                                case "System.UInt16":
                                case "System.UInt32":
                                case "System.UInt64": return $"cast({getExp(callExp.Arguments[0])} as unsigned)";
                                case "System.Guid": return $"substr(cast({getExp(callExp.Arguments[0])} as char), 1, 36)";
                            }
                            break;
                        case "NewGuid":
                            break;
                        case "Next":
                            if (callExp.Object?.Type == typeof(Random)) return "cast(rand()*1000000000 as signed)";
                            break;
                        case "NextDouble":
                            if (callExp.Object?.Type == typeof(Random)) return "rand()";
                            break;
                        case "Random":
                            if (callExp.Method.DeclaringType.IsNumberType()) return "rand()";
                            break;
                        case "ToString":
                            if (callExp.Object != null) return $"cast({getExp(callExp.Object)} as char)";
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
                        switch (callExp.Method.Name)
                        {
                            case "Contains":
                                //判断 in
                                return $"({getExp(callExp.Arguments[argIndex])}) in {left}";
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
                    case "Now": return "now()";
                    case "UtcNow": return "utc_timestamp()";
                    case "Today": return "curdate()";
                    case "MinValue": return "cast('0001/1/1 0:00:00' as datetime)";
                    case "MaxValue": return "cast('9999/12/31 23:59:59' as datetime)";
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Date": return $"cast(date_format({left},'%Y-%m-%d') as datetime)";
                case "TimeOfDay": return $"timestampdiff(microsecond, date_format({left},'%Y-%m-%d'), {left})";
                case "DayOfWeek": return $"(dayofweek({left})-1)";
                case "Day": return $"dayofmonth({left})";
                case "DayOfYear": return $"dayofyear({left})";
                case "Month": return $"month({left})";
                case "Year": return $"year({left})";
                case "Hour": return $"hour({left})";
                case "Minute": return $"minute({left})";
                case "Second": return $"second({left})";
                case "Millisecond": return $"floor(microsecond({left})/1000)";
                case "Ticks": return $"(timestampdiff(microsecond, '0001-1-1', {left})*10)";
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
                case "Days": return $"(({left}) div {(long)1000000 * 60 * 60 * 24})";
                case "Hours": return $"(({left}) div {(long)1000000 * 60 * 60} mod 24)";
                case "Milliseconds": return $"(({left}) div 1000 mod 1000)";
                case "Minutes": return $"(({left}) div {(long)1000000 * 60} mod 60)";
                case "Seconds": return $"(({left}) div 1000000 mod 60)";
                case "Ticks": return $"(({left}) * 10)";
                case "TotalDays": return $"(({left}) / {(long)1000000 * 60 * 60 * 24})";
                case "TotalHours": return $"(({left}) / {(long)1000000 * 60 * 60})";
                case "TotalMilliseconds": return $"(({left}) / 1000)";
                case "TotalMinutes": return $"(({left}) / {(long)1000000 * 60})";
                case "TotalSeconds": return $"(({left}) / 1000000)";
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
                        if (exp.Method.Name == "StartsWith") return $"({left}) LIKE {(args0Value.EndsWith("'") ? args0Value.Insert(args0Value.Length - 1, "%") : $"concat({args0Value}, '%')")}";
                        if (exp.Method.Name == "EndsWith") return $"({left}) LIKE {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"concat('%', {args0Value})")}";
                        if (args0Value.StartsWith("'") && args0Value.EndsWith("'")) return $"({left}) LIKE {args0Value.Insert(1, "%").Insert(args0Value.Length, "%")}";
                        return $"({left}) LIKE concat('%', {args0Value}, '%')";
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
                            return $"(locate({left}, {indexOfFindStr}, {locateArgs1})-1)";
                        }
                        return $"(locate({left}, {indexOfFindStr})-1)";
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
            throw new Exception($"MySqlExpression 未实现函数表达式 {exp} 解析");
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
            throw new Exception($"MySqlExpression 未实现函数表达式 {exp} 解析");
        }
        public override string ExpressionLambdaToSqlCallDateTime(MethodCallExpression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            if (exp.Object == null)
            {
                switch (exp.Method.Name)
                {
                    case "Compare": return $"({getExp(exp.Arguments[0])} - ({getExp(exp.Arguments[1])}))";
                    case "DaysInMonth": return $"dayofmonth(last_day(concat({getExp(exp.Arguments[0])}, '-', {getExp(exp.Arguments[1])}, '-01')))";
                    case "Equals": return $"({getExp(exp.Arguments[0])} = {getExp(exp.Arguments[1])})";

                    case "IsLeapYear":
                        var isLeapYearArgs1 = getExp(exp.Arguments[0]);
                        return $"(({isLeapYearArgs1})%4=0 AND ({isLeapYearArgs1})%100<>0 OR ({isLeapYearArgs1})%400=0)";

                    case "Parse": return $"cast({getExp(exp.Arguments[0])} as datetime)";
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return $"cast({getExp(exp.Arguments[0])} as datetime)";
                }
            }
            else
            {
                var left = getExp(exp.Object);
                var args1 = exp.Arguments.Count == 0 ? null : getExp(exp.Arguments[0]);
                switch (exp.Method.Name)
                {
                    case "Add": return $"date_add({left}, interval ({args1}) microsecond)";
                    case "AddDays": return $"date_add({left}, interval ({args1}) day)";
                    case "AddHours": return $"date_add({left}, interval ({args1}) hour)";
                    case "AddMilliseconds": return $"date_add({left}, interval ({args1})*1000 microsecond)";
                    case "AddMinutes": return $"date_add({left}, interval ({args1}) minute)";
                    case "AddMonths": return $"date_add({left}, interval ({args1}) month)";
                    case "AddSeconds": return $"date_add({left}, interval ({args1}) second)";
                    case "AddTicks": return $"date_add({left}, interval ({args1})/10 microsecond)";
                    case "AddYears": return $"date_add({left}, interval ({args1}) year)";
                    case "Subtract":
                        switch ((exp.Arguments[0].Type.IsNullableType() ? exp.Arguments[0].Type.GenericTypeArguments.FirstOrDefault() : exp.Arguments[0].Type).FullName)
                        {
                            case "System.DateTime": return $"timestampdiff(microsecond, {args1}, {left})";
                            case "System.TimeSpan": return $"date_sub({left}, interval ({args1}) microsecond)";
                        }
                        break;
                    case "Equals": return $"({left} = {getExp(exp.Arguments[0])})";
                    case "CompareTo": return $"timestampdiff(microsecond,{args1},{left})";
                    case "ToString": return $"date_format({left}, '%Y-%m-%d %H:%i:%s.%f')";
                }
            }
            throw new Exception($"MySqlExpression 未实现函数表达式 {exp} 解析");
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
                    case "Parse": return $"cast({getExp(exp.Arguments[0])} as signed)";
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return $"cast({getExp(exp.Arguments[0])} as signed)";
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
                    case "ToString": return $"cast({left} as char)";
                }
            }
            throw new Exception($"MySqlExpression 未实现函数表达式 {exp} 解析");
        }
        public override string ExpressionLambdaToSqlCallConvert(MethodCallExpression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            if (exp.Object == null)
            {
                switch (exp.Method.Name)
                {
                    case "ToBoolean": return $"({getExp(exp.Arguments[0])} not in ('0','false'))";
                    case "ToByte": return $"cast({getExp(exp.Arguments[0])} as unsigned)";
                    case "ToChar": return $"substr(cast({getExp(exp.Arguments[0])} as char), 1, 1)";
                    case "ToDateTime": return $"cast({getExp(exp.Arguments[0])} as datetime)";
                    case "ToDecimal": return $"cast({getExp(exp.Arguments[0])} as decimal(36,18))";
                    case "ToDouble": return $"cast({getExp(exp.Arguments[0])} as decimal(32,16))";
                    case "ToInt16":
                    case "ToInt32":
                    case "ToInt64":
                    case "ToSByte": return $"cast({getExp(exp.Arguments[0])} as signed)";
                    case "ToSingle": return $"cast({getExp(exp.Arguments[0])} as decimal(14,7))";
                    case "ToString": return $"cast({getExp(exp.Arguments[0])} as char)";
                    case "ToUInt16":
                    case "ToUInt32":
                    case "ToUInt64": return $"cast({getExp(exp.Arguments[0])} as unsigned)";
                }
            }
            throw new Exception($"MySqlExpression 未实现函数表达式 {exp} 解析");
        }
    }
}
