using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.SqlServer
{
    class SqlServerExpression : CommonExpression
    {

        public SqlServerExpression(CommonUtils common) : base(common) { }

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
                            case "System.Boolean": return $"(cast({getExp(operandExp)} as varchar) not in ('0','false'))";
                            case "System.Byte": return $"cast({getExp(operandExp)} as tinyint)";
                            case "System.Char": return $"substring(cast({getExp(operandExp)} as nvarchar),1,1)";
                            case "System.DateTime": return $"cast({getExp(operandExp)} as datetime)";
                            case "System.Decimal": return $"cast({getExp(operandExp)} as decimal(36,18))";
                            case "System.Double": return $"cast({getExp(operandExp)} as decimal(32,16))";
                            case "System.Int16": return $"cast({getExp(operandExp)} as smallint)";
                            case "System.Int32": return $"cast({getExp(operandExp)} as int)";
                            case "System.Int64": return $"cast({getExp(operandExp)} as bigint)";
                            case "System.SByte": return $"cast({getExp(operandExp)} as tinyint)";
                            case "System.Single": return $"cast({getExp(operandExp)} as decimal(14,7))";
                            case "System.String": return operandExp.Type.NullableTypeOrThis() == typeof(Guid) ? $"cast({getExp(operandExp)} as varchar(36))" : $"cast({getExp(operandExp)} as nvarchar)";
                            case "System.UInt16": return $"cast({getExp(operandExp)} as smallint)";
                            case "System.UInt32": return $"cast({getExp(operandExp)} as int)";
                            case "System.UInt64": return $"cast({getExp(operandExp)} as bigint)";
                            case "System.Guid": return $"cast({getExp(operandExp)} as uniqueidentifier)";
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
                                case "System.Boolean": return $"(cast({getExp(callExp.Arguments[0])} as varchar) not in ('0','false'))";
                                case "System.Byte": return $"cast({getExp(callExp.Arguments[0])} as tinyint)";
                                case "System.Char": return $"substring(cast({getExp(callExp.Arguments[0])} as nvarchar),1,1)";
                                case "System.DateTime": return $"cast({getExp(callExp.Arguments[0])} as datetime)";
                                case "System.Decimal": return $"cast({getExp(callExp.Arguments[0])} as decimal(36,18))";
                                case "System.Double": return $"cast({getExp(callExp.Arguments[0])} as decimal(32,16))";
                                case "System.Int16": return $"cast({getExp(callExp.Arguments[0])} as smallint)";
                                case "System.Int32": return $"cast({getExp(callExp.Arguments[0])} as int)";
                                case "System.Int64": return $"cast({getExp(callExp.Arguments[0])} as bigint)";
                                case "System.SByte": return $"cast({getExp(callExp.Arguments[0])} as tinyint)";
                                case "System.Single": return $"cast({getExp(callExp.Arguments[0])} as decimal(14,7))";
                                case "System.UInt16": return $"cast({getExp(callExp.Arguments[0])} as smallint)";
                                case "System.UInt32": return $"cast({getExp(callExp.Arguments[0])} as int)";
                                case "System.UInt64": return $"cast({getExp(callExp.Arguments[0])} as bigint)";
                                case "System.Guid": return $"cast({getExp(callExp.Arguments[0])} as uniqueidentifier)";
                            }
                            break;
                        case "NewGuid":
                            switch (callExp.Method.DeclaringType.NullableTypeOrThis().ToString())
                            {
                                case "System.Guid": return $"newid()";
                            }
                            break;
                        case "Next":
                            if (callExp.Object?.Type == typeof(Random)) return "cast(rand()*1000000000 as int)";
                            break;
                        case "NextDouble":
                            if (callExp.Object?.Type == typeof(Random)) return "rand()";
                            break;
                        case "Random":
                            if (callExp.Method.DeclaringType.IsNumberType()) return "rand()";
                            break;
                        case "ToString":
                            if (callExp.Object != null) return callExp.Object.Type.NullableTypeOrThis() == typeof(Guid) ? $"cast({getExp(callExp.Object)} as varchar(36))" : $"cast({getExp(callExp.Object)} as nvarchar)";
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
                case "Length": return $"len({left})";
            }
            return null;
        }
        public override string ExpressionLambdaToSqlMemberAccessDateTime(MemberExpression exp, ExpTSC tsc)
        {
            if (exp.Expression == null)
            {
                switch (exp.Member.Name)
                {
                    case "Now": return "getdate()";
                    case "UtcNow": return "getutcdate()";
                    case "Today": return "convert(char(10),getdate(),120)";
                    case "MinValue": return "'1753/1/1 0:00:00'";
                    case "MaxValue": return "'9999/12/31 23:59:59'";
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Date": return $"convert(char(10),{left},120)";
                case "TimeOfDay": return $"datediff(second, convert(char(10),{left},120), {left})";
                case "DayOfWeek": return $"(datepart(weekday, {left})-1)";
                case "Day": return $"datepart(day, {left})";
                case "DayOfYear": return $"datepart(dayofyear, {left})";
                case "Month": return $"datepart(month, {left})";
                case "Year": return $"datepart(year, {left})";
                case "Hour": return $"datepart(hour, {left})";
                case "Minute": return $"datepart(minute, {left})";
                case "Second": return $"datepart(second, {left})";
                case "Millisecond": return $"(datepart(millisecond, {left})/1000)";
                case "Ticks": return $"(cast(datediff(second, '1970-1-1', {left}) as bigint)*10000000+621355968000000000)";
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
                    case "Concat":
                        return _common.StringConcat(exp.Arguments.Select(a => getExp(a)).ToArray(), exp.Arguments.Select(a => a.Type).ToArray());
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
                        if (exp.Method.Name == "StartsWith") return $"({left}) LIKE {(args0Value.EndsWith("'") ? args0Value.Insert(args0Value.Length - 1, "%") : $"(cast({args0Value} as nvarchar)+'%')")}";
                        if (exp.Method.Name == "EndsWith") return $"({left}) LIKE {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"('%'+cast({args0Value} as nvarchar))")}";
                        if (args0Value.StartsWith("'") && args0Value.EndsWith("'")) return $"({left}) LIKE {args0Value.Insert(1, "%").Insert(args0Value.Length, "%")}";
                        return $"({left}) LIKE ('%'+cast({args0Value} as nvarchar)+'%')";
                    case "ToLower": return $"lower({left})";
                    case "ToUpper": return $"upper({left})";
                    case "Substring":
                        var substrArgs1 = getExp(exp.Arguments[0]);
                        if (long.TryParse(substrArgs1, out var testtrylng1)) substrArgs1 = (testtrylng1 + 1).ToString();
                        else substrArgs1 += "+1";
                        if (exp.Arguments.Count == 1) return $"left({left}, {substrArgs1})";
                        return $"substring({left}, {substrArgs1}, {getExp(exp.Arguments[1])})";
                    case "IndexOf":
                        var indexOfFindStr = getExp(exp.Arguments[0]);
                        if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32")
                        {
                            var locateArgs1 = getExp(exp.Arguments[1]);
                            if (long.TryParse(locateArgs1, out var testtrylng2)) locateArgs1 = (testtrylng2 + 1).ToString();
                            else locateArgs1 += "+1";
                            return $"(charindex({left}, {indexOfFindStr}, {locateArgs1})-1)";
                        }
                        return $"(charindex({left}, {indexOfFindStr})-1)";
                    case "PadLeft":
                        if (exp.Arguments.Count == 1) return $"lpad({left}, {getExp(exp.Arguments[0])})";
                        return $"lpad({left}, {getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                    case "PadRight":
                        if (exp.Arguments.Count == 1) return $"rpad({left}, {getExp(exp.Arguments[0])})";
                        return $"rpad({left}, {getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                    case "Trim": return $"ltrim(rtrim({left}))";
                    case "TrimStart": return $"ltrim({left})";
                    case "TrimEnd": return $"rtrim({left})";
                    case "Replace": return $"replace({left}, {getExp(exp.Arguments[0])}, {getExp(exp.Arguments[1])})";
                    case "CompareTo": return $"({left} - {getExp(exp.Arguments[0])})";
                    case "Equals": return $"({left} = {getExp(exp.Arguments[0])})";
                }
            }
            throw new Exception($"SqlServerExpression 未实现函数表达式 {exp} 解析");
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
                    return $"round({getExp(exp.Arguments[0])}, 0)";
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
                case "Truncate": return $"floor({getExp(exp.Arguments[0])})";
            }
            throw new Exception($"SqlServerExpression 未实现函数表达式 {exp} 解析");
        }
        public override string ExpressionLambdaToSqlCallDateTime(MethodCallExpression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            if (exp.Object == null)
            {
                switch (exp.Method.Name)
                {
                    case "Compare": return $"({getExp(exp.Arguments[0])} - ({getExp(exp.Arguments[1])}))";
                    case "DaysInMonth": return $"datepart(day, dateadd(day, -1, dateadd(month, 1, cast({getExp(exp.Arguments[0])} as varchar) + '-' + cast({getExp(exp.Arguments[1])} as varchar) + '-1')))";
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
                    case "Add": return $"dateadd(second, {args1}, {left})";
                    case "AddDays": return $"dateadd(day, {args1}, {left})";
                    case "AddHours": return $"dateadd(hour, {args1}, {left})";
                    case "AddMilliseconds": return $"dateadd(second, ({args1})/1000, {left})";
                    case "AddMinutes": return $"dateadd(minute, {args1}, {left})";
                    case "AddMonths": return $"dateadd(month, {args1}, {left})";
                    case "AddSeconds": return $"dateadd(second, {args1}, {left})";
                    case "AddTicks": return $"dateadd(second, ({args1})/10000000, {left})";
                    case "AddYears": return $"dateadd(year, {args1}, {left})";
                    case "Subtract":
                        switch ((exp.Arguments[0].Type.IsNullableType() ? exp.Arguments[0].Type.GenericTypeArguments.FirstOrDefault() : exp.Arguments[0].Type).FullName)
                        {
                            case "System.DateTime": return $"datediff(second, {args1}, {left})";
                            case "System.TimeSpan": return $"dateadd(second, {args1}*-1, {left})";
                        }
                        break;
                    case "Equals": return $"({left} = {getExp(exp.Arguments[0])})";
                    case "CompareTo": return $"datediff(second,{getExp(exp.Arguments[0])},{left})";
                    case "ToString": return $"convert(varchar, {left}, 121)";
                }
            }
            throw new Exception($"SqlServerExpression 未实现函数表达式 {exp} 解析");
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
                    case "FromSeconds": return $"({getExp(exp.Arguments[0])})";
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
                    case "Equals": return $"({left} = {getExp(exp.Arguments[0])})";
                    case "CompareTo": return $"({left}-({getExp(exp.Arguments[0])}))";
                    case "ToString": return $"cast({left} as varchar)";
                }
            }
            throw new Exception($"SqlServerExpression 未实现函数表达式 {exp} 解析");
        }
        public override string ExpressionLambdaToSqlCallConvert(MethodCallExpression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            if (exp.Object == null)
            {
                switch (exp.Method.Name)
                {
                    case "ToBoolean": return $"(cast({getExp(exp.Arguments[0])} as varchar) not in ('0','false'))";
                    case "ToByte": return $"cast({getExp(exp.Arguments[0])} as tinyint)";
                    case "ToChar": return $"substring(cast({getExp(exp.Arguments[0])} as nvarchar),1,1)";
                    case "ToDateTime": return $"cast({getExp(exp.Arguments[0])} as datetime)";
                    case "ToDecimal": return $"cast({getExp(exp.Arguments[0])} as decimal(36,18))";
                    case "ToDouble": return $"cast({getExp(exp.Arguments[0])} as decimal(32,16))";
                    case "ToInt16": return $"cast({getExp(exp.Arguments[0])} as smallint)";
                    case "ToInt32": return $"cast({getExp(exp.Arguments[0])} as int)";
                    case "ToInt64": return $"cast({getExp(exp.Arguments[0])} as bigint)";
                    case "ToSByte": return $"cast({getExp(exp.Arguments[0])} as tinyint)";
                    case "ToSingle": return $"cast({getExp(exp.Arguments[0])} as decimal(14,7))";
                    case "ToString": return exp.Arguments[0].Type.NullableTypeOrThis() == typeof(Guid) ? $"cast({getExp(exp.Arguments[0])} as varchar(36))" : $"cast({getExp(exp.Arguments[0])} as nvarchar)";
                    case "ToUInt16": return $"cast({getExp(exp.Arguments[0])} as smallint)";
                    case "ToUInt32": return $"cast({getExp(exp.Arguments[0])} as int)";
                    case "ToUInt64": return $"cast({getExp(exp.Arguments[0])} as bigint)";
                }
            }
            throw new Exception($"SqlServerExpression 未实现函数表达式 {exp} 解析");
        }
    }
}
