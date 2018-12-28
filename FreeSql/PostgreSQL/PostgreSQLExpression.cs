using FreeSql.Internal;
using FreeSql.Internal.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.PostgreSQL {
	class PostgreSQLExpression : CommonExpression {

		public PostgreSQLExpression(CommonUtils common) : base(common) { }

		internal override string ExpressionLambdaToSqlOther(Expression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName) {
			switch (exp.NodeType) {
				case ExpressionType.ArrayLength:
					var arrOperExp = ExpressionLambdaToSql((exp as UnaryExpression).Operand, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
					if (arrOperExp.StartsWith("(") || arrOperExp.EndsWith(")")) return $"array_length(array[{arrOperExp.TrimStart('(').TrimEnd(')')}])";
					return $"case when {arrOperExp} is null then 0 else array_length({arrOperExp},1) end";
				case ExpressionType.Call:
					var callExp = exp as MethodCallExpression;
					var objExp = callExp.Object;
					var objType = objExp?.Type;
					if (objType?.FullName == "System.Byte[]") return null;

					var argIndex = 0;
					if (objType == null && callExp.Method.DeclaringType.FullName == typeof(Enumerable).FullName) {
						objExp = callExp.Arguments.FirstOrDefault();
						objType = objExp?.Type;
						argIndex++;
					}
					if (objType == null) objType = callExp.Method.DeclaringType;
					if (objType != null) {
						var left = objExp == null ? null : ExpressionLambdaToSql(objExp, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
						if (objType.IsArray == true) {
							if (left.StartsWith("(") || left.EndsWith(")")) left = $"array[{left.TrimStart('(').TrimEnd(')')}]";
							switch (callExp.Method.Name) {
								case "Any": return $"(case when {left} is null then 0 else array_length({left},1) end > 0)";
								case "Contains": return $"({left} @> array[{ExpressionLambdaToSql(callExp.Arguments[argIndex], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)}])";
								case "Concat":
									var right2 = ExpressionLambdaToSql(callExp.Arguments[argIndex], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
									if (right2.StartsWith("(") || right2.EndsWith(")")) right2 = $"array[{right2.TrimStart('(').TrimEnd(')')}]";
									return $"({left} || {right2})";
								case "GetLength":
								case "GetLongLength":
								case "Length":
								case "Count": return $"case when {left} is null then 0 else array_length({left},1) end";
							}
						}
						switch (objType.FullName) {
							case "Newtonsoft.Json.Linq.JToken":
							case "Newtonsoft.Json.Linq.JObject":
							case "Newtonsoft.Json.Linq.JArray":
								switch (callExp.Method.Name) {
									case "Any": return $"(jsonb_array_length(coalesce({left},'[]')) > 0)";
									case "Contains":
										var json = ExpressionLambdaToSql(callExp.Arguments[argIndex], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
										if (json.StartsWith("'") && json.EndsWith("'")) return $"(coalesce({left},'{{}}') @> {_common.FormatSql("{0}", JToken.Parse(json.Trim('\'')))})";
										return $"(coalesce({left},'{{}}') @> ({json})::jsonb)";
									case "ContainsKey": return $"(coalesce({left},'{{}}') ? {ExpressionLambdaToSql(callExp.Arguments[argIndex], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
									case "Concat":
										var right2 = ExpressionLambdaToSql(callExp.Arguments[argIndex], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
										return $"(coalesce({left},'{{}}') || {right2})";
									case "LongCount":
									case "Count": return $"jsonb_array_length(coalesce({left},'[]'))";
									case "Parse":
										var json2 = ExpressionLambdaToSql(callExp.Arguments[argIndex], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
										if (json2.StartsWith("'") && json2.EndsWith("'")) return _common.FormatSql("{0}", JToken.Parse(json2.Trim('\'')));
										return $"({json2})::jsonb";
								}
								break;
						}
						if (objType.FullName == typeof(Dictionary<string, string>).FullName) {
							switch (callExp.Method.Name) {
								case "Contains":
									var right = ExpressionLambdaToSql(callExp.Arguments[argIndex], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
									return $"({left} @> ({right}))";
								case "ContainsKey": return $"({left} ? {ExpressionLambdaToSql(callExp.Arguments[argIndex], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
								case "Concat": return $"({left} || {ExpressionLambdaToSql(callExp.Arguments[argIndex], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
								case "GetLength":
								case "GetLongLength":
								case "Count": return $"case when {left} is null then 0 else array_length(akeys({left}),1) end";
								case "Keys": return $"akeys({left})";
								case "Values": return $"avals({left})";
							}
						}
					}
					break;
				case ExpressionType.MemberAccess:
					var memExp = exp as MemberExpression;
					var memParentExp = memExp.Expression?.Type;
					if (memParentExp?.FullName == "System.Byte[]") return null;
					if (memParentExp != null) {
						var left = ExpressionLambdaToSql(memExp.Expression, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
						if (memParentExp.IsArray == true) {
							if (left.StartsWith("(") || left.EndsWith(")")) left = $"array[{left.TrimStart('(').TrimEnd(')')}]";
							switch (memExp.Member.Name) {
								case "Length":
								case "Count": return $"case when {left} is null then 0 else array_length({left},1) end";
							}
						}
						switch (memParentExp.FullName) {
							case "Newtonsoft.Json.Linq.JToken":
							case "Newtonsoft.Json.Linq.JObject":
							case "Newtonsoft.Json.Linq.JArray":
								switch (memExp.Member.Name) {
									case "Count": return $"jsonb_array_length(coalesce({left},'[]'))";
								}
								break;
						}
						if (memParentExp.FullName == typeof(Dictionary<string, string>).FullName) {
							switch (memExp.Member.Name) {
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
					for (var a = 0; a < arrExp.Expressions.Count; a++) {
						if (a > 0) arrSb.Append(",");
						arrSb.Append(ExpressionLambdaToSql(arrExp.Expressions[a], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName));
					}
					return arrSb.Append("]").ToString();
			}
			return null;
		}

		internal override string ExpressionLambdaToSqlMemberAccessString(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Expression == null) {
				switch (exp.Member.Name) {
					case "Empty": return "''";
				}
				return null;
			}
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
			switch (exp.Member.Name) {
				case "Length": return $"char_length({left})";
			}
			return null;
		}
		internal override string ExpressionLambdaToSqlMemberAccessDateTime(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Expression == null) {
				switch (exp.Member.Name) {
					case "Now": return "current_timestamp";
					case "UtcNow": return "(current_timestamp at time zone 'UTC')";
					case "Today": return "current_date";
					case "MinValue": return "'0001/1/1 0:00:00'::timestamp";
					case "MaxValue": return "'9999/12/31 23:59:59'::timestamp";
				}
				return null;
			}
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
			switch (exp.Member.Name) {
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
		internal override string ExpressionLambdaToSqlMemberAccessTimeSpan(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Expression == null) {
				switch (exp.Member.Name) {
					case "Zero": return "0";
					case "MinValue": return "-922337203685477580"; //微秒 Ticks / 10
					case "MaxValue": return "922337203685477580";
				}
				return null;
			}
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
			switch (exp.Member.Name) {
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

		internal override string ExpressionLambdaToSqlCallString(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName) {
			var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
			switch (exp.Method.Name) {
				case "StartsWith":
				case "EndsWith":
				case "Contains":
					var args0Value = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
					if (args0Value == "NULL") return $"({left}) IS NULL";
					var likeOpt = "LIKE";
					if (exp.Arguments.Count > 1) {
						if (exp.Arguments[1].Type == typeof(bool) ||
							exp.Arguments[1].Type == typeof(StringComparison) && ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName).Contains("IgnoreCase")) likeOpt = "ILIKE";
					}
					if (exp.Method.Name == "StartsWith") return $"({left}) {likeOpt} {(args0Value.EndsWith("'") ? args0Value.Insert(args0Value.Length - 1, "%") : $"(({args0Value})::varchar || '%')")}";
					if (exp.Method.Name == "EndsWith") return $"({left}) {likeOpt} {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"('%' || ({args0Value})::varchar)")}";
					if (args0Value.StartsWith("'") && args0Value.EndsWith("'")) return $"({left}) {likeOpt} {args0Value.Insert(1, "%").Insert(args0Value.Length, "%")}";
					return $"({left}) {likeOpt} ('%' || ({args0Value})::varchar || '%')";
				case "ToLower": return $"lower({left})";
				case "ToUpper": return $"upper({left})";
				case "Substring":
					var substrArgs1 = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
					if (long.TryParse(substrArgs1, out var testtrylng1)) substrArgs1 = (testtrylng1 + 1).ToString();
					else substrArgs1 += "+1";
					if (exp.Arguments.Count == 1) return $"substr({left}, {substrArgs1})";
					return $"substr({left}, {substrArgs1}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "IndexOf": return $"(strpos({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})-1)";
				case "PadLeft":
					if (exp.Arguments.Count == 1) return $"lpad({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
					return $"lpad({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "PadRight":
					if (exp.Arguments.Count == 1) return $"rpad({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
					return $"rpad({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Trim":
				case "TrimStart":
				case "TrimEnd":
					if (exp.Arguments.Count == 0) {
						if (exp.Method.Name == "Trim") return $"trim({left})";
						if (exp.Method.Name == "TrimStart") return $"ltrim({left})";
						if (exp.Method.Name == "TrimEnd") return $"rtrim({left})";
					}
					var trimArg1 = "";
					var trimArg2 = "";
					foreach (var argsTrim02 in exp.Arguments) {
						var argsTrim01s = new[] { argsTrim02 };
						if (argsTrim02.NodeType == ExpressionType.NewArrayInit) {
							var arritem = argsTrim02 as NewArrayExpression;
							argsTrim01s = arritem.Expressions.ToArray();
						}
						foreach (var argsTrim01 in argsTrim01s) {
							var trimChr = ExpressionLambdaToSql(argsTrim01, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName).Trim('\'');
							if (trimChr.Length == 1) trimArg1 += trimChr;
							else trimArg2 += $" || ({trimChr})";
						}
					}
					if (exp.Method.Name == "Trim") left = $"trim({left}, {_common.FormatSql("{0}", trimArg1)}{trimArg2})";
					if (exp.Method.Name == "TrimStart") left = $"ltrim({left}, {_common.FormatSql("{0}", trimArg1)}{trimArg2})";
					if (exp.Method.Name == "TrimEnd") left = $"rtrim({left}, {_common.FormatSql("{0}", trimArg1)}{trimArg2})";
					return left;
				case "Replace": return $"replace({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "CompareTo": return $"case when {left} = {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)} then 0 when {left} > {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)} then 1 else -1 end";
				case "Equals": return $"({left} = ({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::varchar)";
			}
			throw new Exception($"PostgreSQLExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallMath(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName) {
			switch (exp.Method.Name) {
				case "Abs": return $"abs({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Sign": return $"sign({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Floor": return $"floor({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Ceiling": return $"ceiling({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Round":
					if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32") return $"round({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
					return $"round({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Exp": return $"exp({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Log": return $"log({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Log10": return $"log10({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Pow": return $"pow({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Sqrt": return $"sqrt({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Cos": return $"cos({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Sin": return $"sin({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Tan": return $"tan({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Acos": return $"acos({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Asin": return $"asin({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Atan": return $"atan({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Atan2": return $"atan2({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case "Truncate": return $"trunc({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)}, 0)";
			}
			throw new Exception($"PostgreSQLExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallDateTime(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Object == null) {
				switch (exp.Method.Name) {
					case "Compare": return $"extract(epoch from ({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::timestamp-({ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::timestamp)";
					case "DaysInMonth": return $"extract(day from ({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)} || '-' || {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)} || '-01')::timestamp+'1 month'::interval-'1 day'::interval)";
					case "Equals": return $"(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::timestamp = ({ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::timestamp)";

					case "IsLeapYear":
						var isLeapYearArgs1 = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
						return $"(({isLeapYearArgs1})::int8%4=0 AND ({isLeapYearArgs1})::int8%100<>0 OR ({isLeapYearArgs1})::int8%400=0)";

					case "Parse": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::timestamp";
					case "ParseExact":
					case "TryParse":
					case "TryParseExact": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::timestamp";
				}
			} else {
				var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
				var args1 = exp.Arguments.Count == 0 ? null : ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
				switch (exp.Method.Name) {
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
						if (exp.Arguments[0].Type.FullName == "System.DateTime" || exp.Arguments[0].Type.GenericTypeArguments.FirstOrDefault()?.FullName == "System.DateTime")
							return $"(extract(epoch from ({left})::timestamp-({args1})::timestamp)*1000000)";
						if (exp.Arguments[0].Type.FullName == "System.TimeSpan" || exp.Arguments[0].Type.GenericTypeArguments.FirstOrDefault()?.FullName == "System.TimeSpan")
							return $"(({left})::timestamp-(({args1})||' microseconds')::interval)";
						break;
					case "Equals": return $"({left} = ({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::timestamp)";
					case "CompareTo": return $"extract(epoch from ({left})::timestamp-({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::timestamp)";
					case "ToString": return $"to_char({left}, 'YYYY-MM-DD HH24:MI:SS.US')";
				}
			}
			throw new Exception($"PostgreSQLExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallTimeSpan(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Object == null) {
				switch (exp.Method.Name) {
					case "Compare": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)}-({ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)}))";
					case "Equals": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)} = {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
					case "FromDays": return $"(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})*{(long)1000000 * 60 * 60 * 24})";
					case "FromHours": return $"(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})*{(long)1000000 * 60 * 60})";
					case "FromMilliseconds": return $"(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})*1000)";
					case "FromMinutes": return $"(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})*{(long)1000000 * 60})";
					case "FromSeconds": return $"(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})*1000000)";
					case "FromTicks": return $"(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})/10)";
					case "Parse": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::int8";
					case "ParseExact":
					case "TryParse":
					case "TryParseExact": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::int8";
				}
			} else {
				var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
				var args1 = exp.Arguments.Count == 0 ? null : ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
				switch (exp.Method.Name) {
					case "Add": return $"({left}+{args1})";
					case "Subtract": return $"({left}-({args1}))";
					case "Equals": return $"({left} = {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
					case "CompareTo": return $"({left}-({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)}))";
					case "ToString": return $"({left})::varchar";
				}
			}
			throw new Exception($"PostgreSQLExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallConvert(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Object == null) {
				switch (exp.Method.Name) {
					case "ToBoolean": return $"(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::varchar not in ('0','false','f','no'))";
					case "ToByte": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::int2";
					case "ToChar": return $"substr(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::char, 1, 1)";
					case "ToDateTime": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::timestamp";
					case "ToDecimal": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::numeric";
					case "ToDouble": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::float8";
					case "ToInt16": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::int2";
					case "ToInt32": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::int4";
					case "ToInt64": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::int8";
					case "ToSByte": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::int2";
					case "ToSingle": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::float4";
					case "ToString": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::varchar";
					case "ToUInt16": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::int2";
					case "ToUInt32": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::int4";
					case "ToUInt64": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})::int8";
				}
			}
			throw new Exception($"PostgreSQLExpression 未现实函数表达式 {exp} 解析");
		}
	}
}
