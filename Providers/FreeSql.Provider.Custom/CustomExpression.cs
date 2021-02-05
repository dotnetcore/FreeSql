using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Custom
{
    class CustomExpression : CommonExpression
    {
        CustomUtils _utils;
        public CustomExpression(CommonUtils common) : base(common)
        {
            _utils = common as CustomUtils;
        }

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
                            case "System.Boolean": return _utils.Adapter.LambdaConvert_ToBoolean(operandExp.Type, getExp(operandExp));
                            case "System.Byte": return _utils.Adapter.LambdaConvert_ToByte(operandExp.Type, getExp(operandExp));
                            case "System.Char": return _utils.Adapter.LambdaConvert_ToChar(operandExp.Type, getExp(operandExp));
                            case "System.DateTime": return _utils.Adapter.LambdaConvert_ToDateTime(operandExp.Type, getExp(operandExp));
                            case "System.Decimal": return _utils.Adapter.LambdaConvert_ToDecimal(operandExp.Type, getExp(operandExp));
                            case "System.Double": return _utils.Adapter.LambdaConvert_ToDouble(operandExp.Type, getExp(operandExp));
                            case "System.Int16": return _utils.Adapter.LambdaConvert_ToInt16(operandExp.Type, getExp(operandExp));
                            case "System.Int32": return _utils.Adapter.LambdaConvert_ToInt32(operandExp.Type, getExp(operandExp));
                            case "System.Int64": return _utils.Adapter.LambdaConvert_ToInt64(operandExp.Type, getExp(operandExp));
                            case "System.SByte": return _utils.Adapter.LambdaConvert_ToSByte(operandExp.Type, getExp(operandExp));
                            case "System.Single": return _utils.Adapter.LambdaConvert_ToSingle(operandExp.Type, getExp(operandExp));
                            case "System.String": return _utils.Adapter.LambdaConvert_ToString(operandExp.Type, getExp(operandExp));
                            case "System.UInt16": return _utils.Adapter.LambdaConvert_ToUInt16(operandExp.Type, getExp(operandExp));
                            case "System.UInt32": return _utils.Adapter.LambdaConvert_ToUInt32(operandExp.Type, getExp(operandExp));
                            case "System.UInt64": return _utils.Adapter.LambdaConvert_ToUInt64(operandExp.Type, getExp(operandExp));
                            case "System.Guid": return _utils.Adapter.LambdaConvert_ToGuid(operandExp.Type, getExp(operandExp));
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
                                case "System.Boolean": return _utils.Adapter.LambdaConvert_ToBoolean(callExp.Method.DeclaringType, getExp(callExp.Arguments[0]));
                                case "System.Byte": return _utils.Adapter.LambdaConvert_ToByte(callExp.Method.DeclaringType, getExp(callExp.Arguments[0]));
                                case "System.Char": return _utils.Adapter.LambdaConvert_ToChar(callExp.Method.DeclaringType, getExp(callExp.Arguments[0]));
                                case "System.DateTime": return _utils.Adapter.LambdaConvert_ToDateTime(callExp.Method.DeclaringType, getExp(callExp.Arguments[0]));
                                case "System.Decimal": return _utils.Adapter.LambdaConvert_ToDecimal(callExp.Method.DeclaringType, getExp(callExp.Arguments[0]));
                                case "System.Double": return _utils.Adapter.LambdaConvert_ToDouble(callExp.Method.DeclaringType, getExp(callExp.Arguments[0]));
                                case "System.Int16": return _utils.Adapter.LambdaConvert_ToInt16(callExp.Method.DeclaringType, getExp(callExp.Arguments[0]));
                                case "System.Int32": return _utils.Adapter.LambdaConvert_ToInt32(callExp.Method.DeclaringType, getExp(callExp.Arguments[0]));
                                case "System.Int64": return _utils.Adapter.LambdaConvert_ToInt64(callExp.Method.DeclaringType, getExp(callExp.Arguments[0]));
                                case "System.SByte": return _utils.Adapter.LambdaConvert_ToSByte(callExp.Method.DeclaringType, getExp(callExp.Arguments[0]));
                                case "System.Single": return _utils.Adapter.LambdaConvert_ToSingle(callExp.Method.DeclaringType, getExp(callExp.Arguments[0]));
                                case "System.UInt16": return _utils.Adapter.LambdaConvert_ToUInt16(callExp.Method.DeclaringType, getExp(callExp.Arguments[0]));
                                case "System.UInt32": return _utils.Adapter.LambdaConvert_ToUInt32(callExp.Method.DeclaringType, getExp(callExp.Arguments[0]));
                                case "System.UInt64": return _utils.Adapter.LambdaConvert_ToUInt64(callExp.Method.DeclaringType, getExp(callExp.Arguments[0]));
                                case "System.Guid": return _utils.Adapter.LambdaConvert_ToGuid(callExp.Method.DeclaringType, getExp(callExp.Arguments[0]));
                            }
                            return null;
                        case "NewGuid":
                            switch (callExp.Method.DeclaringType.NullableTypeOrThis().ToString())
                            {
                                case "System.Guid": return _utils.Adapter.LambdaGuid_NewGuid;
                            }
                            return null;
                        case "Next":
                            if (callExp.Object?.Type == typeof(Random)) return _utils.Adapter.LambdaRandom_Next;
                            return null;
                        case "NextDouble":
                            if (callExp.Object?.Type == typeof(Random)) return _utils.Adapter.LambdaRandom_NextDouble;
                            return null;
                        case "Random":
                            if (callExp.Method.DeclaringType.IsNumberType()) return _utils.Adapter.LambdaRandom_NextDouble;
                            return null;
                        case "ToString":
                            if (callExp.Object != null) return callExp.Arguments.Count == 0 ? _utils.Adapter.LambdaConvert_ToString(callExp.Object.Type, getExp(callExp.Object)) : null;
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
                                    return _utils.Adapter.LambdaString_Substring(getExp(callExp.Arguments[0]), "1", "1");
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
                case "Length": return _utils.Adapter.LambdaString_Length(left);
            }
            return null;
        }
        public override string ExpressionLambdaToSqlMemberAccessDateTime(MemberExpression exp, ExpTSC tsc)
        {
            if (exp.Expression == null)
            {
                switch (exp.Member.Name)
                {
                    case "Now": return _utils.Adapter.LambdaDateTime_Now;
                    case "UtcNow": return _utils.Adapter.LambdaDateTime_UtcNow;
                    case "Today": return _utils.Adapter.LambdaDateTime_Today;
                    case "MinValue": return _utils.Adapter.LambdaDateTime_MinValue;
                    case "MaxValue": return _utils.Adapter.LambdaDateTime_MaxValue;
                }
                return null;
            }
            var left = ExpressionLambdaToSql(exp.Expression, tsc);
            switch (exp.Member.Name)
            {
                case "Date": return _utils.Adapter.LambdaDateTime_Date(left);
                case "TimeOfDay": return _utils.Adapter.LambdaDateTime_TimeOfDay(left);
                case "DayOfWeek": return _utils.Adapter.LambdaDateTime_DayOfWeek(left);
                case "Day": return _utils.Adapter.LambdaDateTime_Day(left);
                case "DayOfYear": return _utils.Adapter.LambdaDateTime_DayOfYear(left);
                case "Month": return _utils.Adapter.LambdaDateTime_Month(left);
                case "Year": return _utils.Adapter.LambdaDateTime_Year(left);
                case "Hour": return _utils.Adapter.LambdaDateTime_Hour(left);
                case "Minute": return _utils.Adapter.LambdaDateTime_Minute(left);
                case "Second": return _utils.Adapter.LambdaDateTime_Second(left);
                case "Millisecond": return _utils.Adapter.LambdaDateTime_Millisecond(left);
                case "Ticks": return _utils.Adapter.LambdaDateTime_Ticks(left);
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
                case "Days": return _utils.Adapter.LambdaMath_Floor($"({left})/{60 * 60 * 24}");
                case "Hours": return _utils.Adapter.LambdaMath_Floor($"({left})/{60 * 60}%24");
                case "Milliseconds": return $"({_utils.Adapter.CastSql(left, _utils.Adapter.MappingDbTypeBigInt)}*1000)";
                case "Minutes": return _utils.Adapter.LambdaMath_Floor($"({left})/60%60");
                case "Seconds": return $"(({left})%60)";
                case "Ticks": return $"({_utils.Adapter.CastSql(left, _utils.Adapter.MappingDbTypeBigInt)}*10000000)";
                case "TotalDays": return $"(({left})/{60 * 60 * 24})";
                case "TotalHours": return $"(({left})/{60 * 60})";
                case "TotalMilliseconds": return $"({_utils.Adapter.CastSql(left, _utils.Adapter.MappingDbTypeBigInt)}*1000)";
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
                    case "IsNullOrEmpty": return _utils.Adapter.LambdaString_IsNullOrEmpty(getExp(exp.Arguments[0]));
                    case "IsNullOrWhiteSpace": return _utils.Adapter.LambdaString_IsNullOrWhiteSpace(getExp(exp.Arguments[0]));
                    case "Concat": return _common.StringConcat(exp.Arguments.Select(a => getExp(a)).ToArray(), exp.Arguments.Select(a => a.Type).ToArray());
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
                        if (exp.Method.Name == "StartsWith") return $"({left}) LIKE {(args0Value.EndsWith("'") ? args0Value.Insert(args0Value.Length - 1, "%") : $"({_common.StringConcat(new string[] { args0Value, "'%'" }, new[] { typeof(int), typeof(string) })})")}";
                        if (exp.Method.Name == "EndsWith") return $"({left}) LIKE {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"({_common.StringConcat(new string[] { "'%'", args0Value }, new[] { typeof(string), typeof(int) })})")}";
                        if (args0Value.StartsWith("'") && args0Value.EndsWith("'")) return $"({left}) LIKE {args0Value.Insert(1, "%").Insert(args0Value.Length, "%")}";
                        return $"({left}) LIKE ({_common.StringConcat(new string[] { "'%'", args0Value, "'%'" }, new[] { typeof(string), typeof(int), typeof(string)})})";
                    case "ToLower": return _utils.Adapter.LambdaString_ToLower(left);
                    case "ToUpper": return _utils.Adapter.LambdaString_ToUpper(left);
                    case "Substring":
                        var substrArgs1 = getExp(exp.Arguments[0]);
                        if (long.TryParse(substrArgs1, out var testtrylng1)) substrArgs1 = (testtrylng1 + 1).ToString();
                        else substrArgs1 += "+1";
                        if (exp.Arguments.Count == 1) return _utils.Adapter.LambdaString_Substring(left, substrArgs1, null);
                        return _utils.Adapter.LambdaString_Substring(left, substrArgs1, getExp(exp.Arguments[1]));
                    case "IndexOf":
                        var indexOfFindStr = getExp(exp.Arguments[0]);
                        if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32")
                        {
                            var locateArgs1 = getExp(exp.Arguments[1]);
                            if (long.TryParse(locateArgs1, out var testtrylng2)) locateArgs1 = (testtrylng2 + 1).ToString();
                            else locateArgs1 += "+1";
                            return _utils.Adapter.LambdaString_IndexOf(left, indexOfFindStr, locateArgs1);
                        }
                        return _utils.Adapter.LambdaString_IndexOf(left, indexOfFindStr, null);
                    case "PadLeft":
                        if (exp.Arguments.Count == 1) return _utils.Adapter.LambdaString_PadLeft(left, getExp(exp.Arguments[0]), null);
                        return _utils.Adapter.LambdaString_PadLeft(left, getExp(exp.Arguments[0]), getExp(exp.Arguments[1]));
                    case "PadRight":
                        if (exp.Arguments.Count == 1) return _utils.Adapter.LambdaString_PadRight(left, getExp(exp.Arguments[0]), null);
                        return _utils.Adapter.LambdaString_PadRight(left, getExp(exp.Arguments[0]), getExp(exp.Arguments[1]));
                    case "Trim": return _utils.Adapter.LambdaString_Trim(left);
                    case "TrimStart": return _utils.Adapter.LambdaString_TrimStart(left);
                    case "TrimEnd": return _utils.Adapter.LambdaString_TrimEnd(left);
                    case "Replace": return _utils.Adapter.LambdaString_Replace(left, getExp(exp.Arguments[0]), getExp(exp.Arguments[1]));
                    case "CompareTo": return _utils.Adapter.LambdaString_CompareTo(left, getExp(exp.Arguments[0]));
                    case "Equals": return _utils.Adapter.LambdaString_Equals(left, getExp(exp.Arguments[0]));
                }
            }
            return null;
        }
        public override string ExpressionLambdaToSqlCallMath(MethodCallExpression exp, ExpTSC tsc)
        {
            Func<Expression, string> getExp = exparg => ExpressionLambdaToSql(exparg, tsc);
            switch (exp.Method.Name)
            {
                case "Abs": return _utils.Adapter.LambdaMath_Abs(getExp(exp.Arguments[0]));
                case "Sign": return _utils.Adapter.LambdaMath_Sign(getExp(exp.Arguments[0]));
                case "Floor": return _utils.Adapter.LambdaMath_Floor(getExp(exp.Arguments[0]));
                case "Ceiling": return _utils.Adapter.LambdaMath_Ceiling(getExp(exp.Arguments[0]));
                case "Round":
                    if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32") return _utils.Adapter.LambdaMath_Round(getExp(exp.Arguments[0]), getExp(exp.Arguments[1]));
                    return _utils.Adapter.LambdaMath_Round(getExp(exp.Arguments[0]), "0");
                case "Exp": return _utils.Adapter.LambdaMath_Exp(getExp(exp.Arguments[0]));
                case "Log": return _utils.Adapter.LambdaMath_Log(getExp(exp.Arguments[0]));
                case "Log10": return _utils.Adapter.LambdaMath_Log10(getExp(exp.Arguments[0]));
                case "Pow": return _utils.Adapter.LambdaMath_Pow(getExp(exp.Arguments[0]), getExp(exp.Arguments[1]));
                case "Sqrt": return _utils.Adapter.LambdaMath_Sqrt(getExp(exp.Arguments[0]));
                case "Cos": return  _utils.Adapter.LambdaMath_Cos(getExp(exp.Arguments[0]));
                case "Sin": return  _utils.Adapter.LambdaMath_Sin(getExp(exp.Arguments[0]));
                case "Tan": return  _utils.Adapter.LambdaMath_Tan(getExp(exp.Arguments[0]));
                case "Acos": return _utils.Adapter.LambdaMath_Acos(getExp(exp.Arguments[0]));
                case "Asin": return _utils.Adapter.LambdaMath_Asin(getExp(exp.Arguments[0]));
                case "Atan": return _utils.Adapter.LambdaMath_Atan(getExp(exp.Arguments[0]));
                case "Atan2": return _utils.Adapter.LambdaMath_Atan2(getExp(exp.Arguments[0]), getExp(exp.Arguments[1]));
                case "Truncate": return _utils.Adapter.LambdaMath_Truncate(getExp(exp.Arguments[0]));
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
                    case "Compare": return _utils.Adapter.LambdaDateTime_CompareTo(getExp(exp.Arguments[0]), getExp(exp.Arguments[1]));
                    case "DaysInMonth": return _utils.Adapter.LambdaDateTime_DaysInMonth(getExp(exp.Arguments[0]), getExp(exp.Arguments[1]));
                    case "Equals": return _utils.Adapter.LambdaDateTime_Equals(getExp(exp.Arguments[0]), getExp(exp.Arguments[1]));

                    case "IsLeapYear": return _utils.Adapter.LambdaDateTime_IsLeapYear(getExp(exp.Arguments[0]));

                    case "Parse": return _utils.Adapter.LambdaConvert_ToDateTime(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return _utils.Adapter.LambdaConvert_ToDateTime(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                }
            }
            else
            {
                var left = getExp(exp.Object);
                var args1 = exp.Arguments.Count == 0 ? null : getExp(exp.Arguments[0]);
                switch (exp.Method.Name)
                {
                    case "Add": return _utils.Adapter.LambdaDateTime_Add(left, args1);
                    case "AddDays": return _utils.Adapter.LambdaDateTime_AddDays(left, args1);
                    case "AddHours": return _utils.Adapter.LambdaDateTime_AddHours(left, args1);
                    case "AddMilliseconds": return _utils.Adapter.LambdaDateTime_AddMilliseconds(left, args1);
                    case "AddMinutes": return _utils.Adapter.LambdaDateTime_AddMinutes(left, args1);
                    case "AddMonths": return _utils.Adapter.LambdaDateTime_AddMonths(left, args1);
                    case "AddSeconds": return _utils.Adapter.LambdaDateTime_AddSeconds(left, args1);
                    case "AddTicks": return _utils.Adapter.LambdaDateTime_AddTicks(left, args1);
                    case "AddYears": return _utils.Adapter.LambdaDateTime_AddYears(left, args1);
                    case "Subtract":
                        switch ((exp.Arguments[0].Type.IsNullableType() ? exp.Arguments[0].Type.GetGenericArguments().FirstOrDefault() : exp.Arguments[0].Type).FullName)
                        {
                            case "System.DateTime": return _utils.Adapter.LambdaDateTime_Subtract(left, args1);
                            case "System.TimeSpan": return _utils.Adapter.LambdaDateTime_SubtractTimeSpan(left, args1);
                        }
                        break;
                    case "Equals": return _utils.Adapter.LambdaDateTime_Equals(left, args1);
                    case "CompareTo": return _utils.Adapter.LambdaDateTime_CompareTo(left, args1);
                    case "ToString": return exp.Arguments.Count == 0 ? _utils.Adapter.LambdaDateTime_ToString(left) : null;
                }
            }
            return null;
        }

        public virtual string LambdaDateSpan_Add(string operand, string value) => $"({operand}+{value})";
        public virtual string LambdaDateSpan_Subtract(string operand, string value) => $"({operand}-({value}))";
        public virtual string LambdaDateSpan_Equals(string oldvalue, string newvalue) => $"({oldvalue} = {newvalue})";
        public virtual string LambdaDateSpan_CompareTo(string oldvalue, string newvalue) => $"({oldvalue}-({newvalue}))";

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
                    case "Parse": return _utils.Adapter.LambdaConvert_ToInt64(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ParseExact":
                    case "TryParse":
                    case "TryParseExact": return _utils.Adapter.LambdaConvert_ToInt64(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
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
                    case "ToString": return _utils.Adapter.LambdaConvert_ToString(exp.Type, left);
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
                    case "ToBoolean": return _utils.Adapter.LambdaConvert_ToBoolean(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ToByte": return _utils.Adapter.LambdaConvert_ToByte(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ToChar": return _utils.Adapter.LambdaConvert_ToChar(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ToDateTime": return _utils.Adapter.LambdaConvert_ToDateTime(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ToDecimal": return _utils.Adapter.LambdaConvert_ToDecimal(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ToDouble": return _utils.Adapter.LambdaConvert_ToDouble(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ToInt16": return _utils.Adapter.LambdaConvert_ToInt16(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ToInt32": return _utils.Adapter.LambdaConvert_ToInt32(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ToInt64": return _utils.Adapter.LambdaConvert_ToInt64(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ToSByte": return _utils.Adapter.LambdaConvert_ToSByte(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ToSingle": return _utils.Adapter.LambdaConvert_ToSingle(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ToString": return _utils.Adapter.LambdaConvert_ToString(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ToUInt16": return _utils.Adapter.LambdaConvert_ToUInt16(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ToUInt32": return _utils.Adapter.LambdaConvert_ToUInt32(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                    case "ToUInt64": return _utils.Adapter.LambdaConvert_ToUInt64(exp.Arguments[0].Type, getExp(exp.Arguments[0]));
                }
            }
            return null;
        }
    }
}
