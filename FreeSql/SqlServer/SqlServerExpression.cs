using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FreeSql.SqlServer {
	class SqlServerExpression : CommonExpression {

		public SqlServerExpression(CommonUtils common) : base(common) { }

		internal override string ExpressionLambdaToSqlMemberAccessString(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Expression == null) {
				switch (exp.Member.Name) {
					case "Empty": return "''";
				}
				return null;
			}
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Member.Name) {
				case "Length": return $"len({left})";
			}
			return null;
		}
		internal override string ExpressionLambdaToSqlMemberAccessDateTime(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Expression == null) {
				switch (exp.Member.Name) {
					case "Now": return "getdate()";
					case "UtcNow": return "getutcdate()";
					case "Today": return "convert(char(10),getdate(),120)";
					case "MinValue": return "'1753/1/1 0:00:00'";
					case "MaxValue": return "'9999/12/31 23:59:59'";
				}
				return null;
			}
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Member.Name) {
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
		internal override string ExpressionLambdaToSqlMemberAccessTimeSpan(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Expression == null) {
				switch (exp.Member.Name) {
					case "Zero": return "0";
					case "MinValue": return "-922337203685477580"; //微秒 Ticks / 10
					case "MaxValue": return "922337203685477580";
				}
				return null;
			}
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Member.Name) {
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

		internal override string ExpressionLambdaToSqlCallString(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Method.Name) {
				case "StartsWith":
				case "EndsWith":
				case "Contains":
					var args0Value = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
					if (args0Value == "NULL") return $"({left}) IS NULL";
					if (exp.Method.Name == "StartsWith") return $"({left}) LIKE {(args0Value.EndsWith("'") ? args0Value.Insert(args0Value.Length - 1, "%") : $"(cast({args0Value} as nvarchar)+'%')")}";
					if (exp.Method.Name == "EndsWith") return $"({left}) LIKE {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"('%'+cast({args0Value} as nvarchar))")}";
					if (args0Value.StartsWith("'") && args0Value.EndsWith("'")) return $"({left}) LIKE {args0Value.Insert(1, "%").Insert(args0Value.Length, "%")}";
					return $"({left}) LIKE ('%'+cast({args0Value} as nvarchar)+'%')";
				case "ToLower": return $"lower({left})";
				case "ToUpper": return $"upper({left})";
				case "Substring":
					var substrArgs1 = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
					if (long.TryParse(substrArgs1, out var testtrylng1)) substrArgs1 = (testtrylng1 + 1).ToString();
					else substrArgs1 += "+1";
					if (exp.Arguments.Count == 1) return $"left({left}, {substrArgs1})";
					return $"substring({left}, {substrArgs1}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "IndexOf":
					var indexOfFindStr = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
					if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32") {
						var locateArgs1 = ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName);
						if (long.TryParse(locateArgs1, out var testtrylng2)) locateArgs1 = (testtrylng2 + 1).ToString();
						else locateArgs1 += "+1";
						return $"(charindex({left}, {indexOfFindStr}, {locateArgs1})-1)";
					}
					return $"(charindex({left}, {indexOfFindStr})-1)";
				case "PadLeft":
					if (exp.Arguments.Count == 1) return $"lpad({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					return $"lpad({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "PadRight":
					if (exp.Arguments.Count == 1) return $"rpad({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					return $"rpad({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Trim": return $"ltrim(rtrim({left}))";
				case "TrimStart": return $"ltrim({left})";
				case "TrimEnd": return $"rtrim({left})";
				case "Replace": return $"replace({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "CompareTo": return $"({left} - {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Equals": return $"({left} = {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
			}
			throw new Exception($"SqlServerExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallMath(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			switch (exp.Method.Name) {
				case "Abs": return $"abs({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Sign": return $"sign({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Floor": return $"floor({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Ceiling": return $"ceiling({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Round":
					if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32") return $"round({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					return $"round({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, 0)";
				case "Exp": return $"exp({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Log": return $"log({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Log10": return $"log10({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Pow": return $"power({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Sqrt": return $"sqrt({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Cos": return $"cos({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Sin": return $"sin({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Tan": return $"tan({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Acos": return $"acos({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Asin": return $"asin({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Atan": return $"atan({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Atan2": return $"atan2({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Truncate": return $"floor({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
			}
			throw new Exception($"SqlServerExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallDateTime(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Object == null) {
				switch (exp.Method.Name) {
					case "Compare": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} - ({ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)}))";
					case "DaysInMonth": return $"datepart(day, dateadd(day, -1, dateadd(month, 1, cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as varchar) + '-' + cast({ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)} as varchar) + '-1')))";
					case "Equals": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} = {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";

					case "IsLeapYear":
						var isLeapYearArgs1 = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
						return $"(({isLeapYearArgs1})%4=0 AND ({isLeapYearArgs1})%100<>0 OR ({isLeapYearArgs1})%400=0)";

					case "Parse": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as datetime)";
					case "ParseExact":
					case "TryParse":
					case "TryParseExact": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as datetime)";
				}
			} else {
				var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, tbtype, isQuoteName);
				var args1 = exp.Arguments.Count == 0 ? null : ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
				switch (exp.Method.Name) {
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
						if (exp.Arguments[0].Type.FullName == "System.DateTime" || exp.Arguments[0].Type.GenericTypeArguments.FirstOrDefault()?.FullName == "System.DateTime")
							return $"datediff(second, {args1}, {left})";
						if (exp.Arguments[0].Type.FullName == "System.TimeSpan" || exp.Arguments[0].Type.GenericTypeArguments.FirstOrDefault()?.FullName == "System.TimeSpan")
							return $"dateadd(second, {args1}*-1, {left})";
						break;
					case "Equals": return $"({left} = {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "CompareTo": return $"(({left}) - ({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}))";
					case "ToString": return $"convert(varchar, {left}, 121)";
				}
			}
			throw new Exception($"SqlServerExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallTimeSpan(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Object == null) {
				switch (exp.Method.Name) {
					case "Compare": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}-({ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)}))";
					case "Equals": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} = {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "FromDays": return $"(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})*{60 * 60 * 24})";
					case "FromHours": return $"(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})*{60 * 60})";
					case "FromMilliseconds": return $"(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})/1000)";
					case "FromMinutes": return $"(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})*60)";
					case "FromSeconds": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "FromTicks": return $"(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})/10000000)";
					case "Parse": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as bigint)";
					case "ParseExact":
					case "TryParse":
					case "TryParseExact": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as bigint)";
				}
			} else {
				var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, tbtype, isQuoteName);
				var args1 = exp.Arguments.Count == 0 ? null : ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
				switch (exp.Method.Name) {
					case "Add": return $"({left}+{args1})";
					case "Subtract": return $"({left}-({args1}))";
					case "Equals": return $"({left} = {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "CompareTo": return $"({left}-({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}))";
					case "ToString": return $"cast({left} as varchar)";
				}
			}
			throw new Exception($"SqlServerExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallConvert(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Object == null) {
				switch (exp.Method.Name) {
					case "ToBoolean": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} not in ('0','false'))";
					case "ToByte": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as tinyint)";
					case "ToChar": return $"substring(cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as nvarchar), 1, 1)";
					case "ToDateTime": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as datetime)";
					case "ToDecimal": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as decimal(36,18))";
					case "ToDouble": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as decimal(32,16))";
					case "ToInt16": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as smallint)";
					case "ToInt32": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as int)";
					case "ToInt64": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as bigint)";
					case "ToSByte": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as tinyint)";
					case "ToSingle": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as decimal(14,7))";
					case "ToString": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as nvarchar)";
					case "ToUInt16": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as smallint)";
					case "ToUInt32": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as int)";
					case "ToUInt64": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as bigint)";
				}
			}
			throw new Exception($"SqlServerExpression 未现实函数表达式 {exp} 解析");
		}
	}
}
