using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FreeSql.MySql {
	class MySqlExpression : CommonExpression {

		public MySqlExpression(CommonUtils common) : base(common) { }

		internal override string ExpressionLambdaToSqlMemberAccessString(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Expression == null) {
				switch (exp.Member.Name) {
					case "Empty": return "''";
				}
				return null;
			}
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Member.Name) {
				case "Length": return $"char_length({left})";
			}
			return null;
		}
		internal override string ExpressionLambdaToSqlMemberAccessDateTime(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Expression == null) {
				switch (exp.Member.Name) {
					case "Now": return "now()";
					case "UtcNow": return "utc_timestamp()";
					case "Today": return "curdate()";
					case "MinValue": return "cast('0001/1/1 0:00:00' as datetime)";
					case "MaxValue": return "cast('9999/12/31 23:59:59' as datetime)";
				}
				return null;
			}
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Member.Name) {
				case "Date": return $"cast(date_format({left}, '%Y-%m-%d') as datetime)";
				case "TimeOfDay": return $"(time_to_sec(date_format({left}, '1970-1-1 %H:%i:%s.%f')) * 1000000 + microsecond({left}) + 6213559680000000)";
				case "DayOfWeek": return $"(dayofweek({left}) - 1)";
				case "Day": return $"dayofmonth({left})";
				case "DayOfYear": return $"dayofyear({left})";
				case "Month": return $"month({left})";
				case "Year": return $"year({left})";
				case "Hour": return $"hour({left})";
				case "Minute": return $"minute({left})";
				case "Second": return $"second({left})";
				case "Millisecond": return $"floor(microsecond({left}) / 1000)";
				case "Ticks": return $"(time_to_sec({left}) * 10000000 + microsecond({left}) * 10 + 62135596800000000)";
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

		internal override string ExpressionLambdaToSqlCallString(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Method.Name) {
				case "StartsWith":
				case "EndsWith":
				case "Contains":
					var args0Value = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
					if (args0Value == "NULL") return $"({left}) IS NULL";
					if (exp.Method.Name == "StartsWith") return $"({left}) LIKE {(args0Value.EndsWith("'") ? args0Value.Insert(args0Value.Length - 1, "%") : $"concat({args0Value}, '%')")}";
					if (exp.Method.Name == "EndsWith") return $"({left}) LIKE {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"concat('%', {args0Value})")}";
					if (args0Value.StartsWith("'") && args0Value.EndsWith("'")) return $"({left}) LIKE {args0Value.Insert(1, "%").Insert(args0Value.Length, "%")}";
					return $"({left}) LIKE concat('%', {args0Value}, '%')";
				case "ToLower": return $"lower({left})";
				case "ToUpper": return $"upper({left})";
				case "Substring":
					var substrArgs1 = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
					if (long.TryParse(substrArgs1, out var testtrylng1)) substrArgs1 = (testtrylng1 + 1).ToString();
					else substrArgs1 += " + 1";
					if (exp.Arguments.Count == 1) return $"substr({left}, {substrArgs1})";
					return $"substr({left}, {substrArgs1}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "IndexOf":
					var indexOfFindStr = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
					if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32") {
						var locateArgs1 = ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName);
						if (long.TryParse(locateArgs1, out var testtrylng2)) locateArgs1 = (testtrylng2 + 1).ToString();
						else locateArgs1 += " + 1";
						return $"(locate({left}, {indexOfFindStr}, {locateArgs1}) - 1)";
					}
					return $"(locate({left}, {indexOfFindStr}) - 1)";
				case "PadLeft":
					if (exp.Arguments.Count == 1) return $"lpad({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					return $"lpad({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "PadRight":
					if (exp.Arguments.Count == 1) return $"rpad({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					return $"rpad({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Trim":
				case "TrimStart":
				case "TrimEnd":
					if (exp.Arguments.Count == 0) {
						if (exp.Method.Name == "Trim") return $"trim({left})";
						if (exp.Method.Name == "TrimStart") return $"ltrim({left})";
						if (exp.Method.Name == "TrimEnd") return $"rtrim({left})";
					}
					foreach (var argsTrim02 in exp.Arguments) {
						var argsTrim01s = new[] { argsTrim02 };
						if (argsTrim02.NodeType == ExpressionType.NewArrayInit) {
							var arritem = argsTrim02 as NewArrayExpression;
							argsTrim01s = arritem.Expressions.ToArray();
						}
						foreach (var argsTrim01 in argsTrim01s) {
							if (exp.Method.Name == "Trim") left = $"trim({ExpressionLambdaToSql(argsTrim01, _tables, _selectColumnMap, tbtype, isQuoteName)} from {left})";
							if (exp.Method.Name == "TrimStart") left = $"trim(leading {ExpressionLambdaToSql(argsTrim01, _tables, _selectColumnMap, tbtype, isQuoteName)} from {left})";
							if (exp.Method.Name == "TrimEnd") left = $"trim(trailing {ExpressionLambdaToSql(argsTrim01, _tables, _selectColumnMap, tbtype, isQuoteName)} from {left})";
						}
					}
					return left;
				case "Replace": return $"replace({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "CompareTo": return $"strcmp({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Equals": return $"({left} = {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
			}
			throw new Exception($"MySqlExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallMath(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			switch (exp.Method.Name) {
				case "Abs": return $"abs({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Sign": return $"sign({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Floor": return $"floor({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Ceiling": return $"ceiling({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Round":
					if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32") return $"round({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					return $"round({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Exp": return $"exp({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Log": return $"log({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Log10": return $"log10({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Pow": return $"pow({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Sqrt": return $"sqrt({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Cos": return $"cos({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Sin": return $"sin({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Tan": return $"tan({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Acos": return $"acos({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Asin": return $"asin({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Atan": return $"atan({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Atan2": return $"atan2({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "Truncate": return $"truncate({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, 0)";
			}
			throw new Exception($"MySqlExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallDateTime(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Object == null) {
				switch (exp.Method.Name) {
					case "Compare": return $"(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}) - ({ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)}))";
					case "DaysInMonth": return $"dayofmonth(last_day(concat({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, '-', {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)}, '-01')))";
					case "Equals": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} = {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";

					case "IsLeapYear":
						var isLeapYearArgs1 = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
						return $"(({isLeapYearArgs1}) % 4 = 0 AND ({isLeapYearArgs1}) % 100 <> 0 OR ({isLeapYearArgs1}) % 400 = 0)";

					case "Parse": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as datetime)";
					case "ParseExact":
					case "TryParse":
					case "TryParseExact": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as datetime)";
				}
				return null;
			}
			var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, tbtype, isQuoteName);
			var args1 = exp.Arguments.Count == 0 ? null : ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Method.Name) {
				case "Add": return $"date_add({left}, interval ({args1}) microsecond)";
				case "AddDays": return $"date_add({left}, interval ({args1}) day)";
				case "AddHours": return $"date_add({left}, interval ({args1}) hour)";
				case "AddMilliseconds": return $"date_add({left}, interval ({args1}) * 1000 microsecond)";
				case "AddMinutes": return $"date_add({left}, interval ({args1}) minute)";
				case "AddMonths": return $"date_add({left}, interval ({args1}) month)";
				case "AddSeconds": return $"date_add({left}, interval ({args1}) second)";
				case "AddTicks": return $"date_add({left}, interval ({args1}) / 10 microsecond)";
				case "AddYears": return $"date_add({left}, interval ({args1}) year)";
				case "Subtract":
					if (exp.Arguments[0].Type.FullName == "System.DateTime" || exp.Arguments[0].Type.GenericTypeArguments.FirstOrDefault()?.FullName == "System.DateTime")
						return $"((time_to_sec({left}) - time_to_sec({args1})) * 1000000 + microsecond({left}) - microsecond({args1}))";
					if (exp.Arguments[0].Type.FullName == "System.TimeSpan" || exp.Arguments[0].Type.GenericTypeArguments.FirstOrDefault()?.FullName == "System.TimeSpan")
						return $"date_sub({left}, interval ({args1}) microsecond)";
					break;
				case "Equals": return $"({left} = {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "CompareTo": return $"(({left}) - ({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}))";
				case "ToString": return $"date_format({left}, '%Y-%m-%d %H:%i:%s.%f')";
			}
			throw new Exception($"MySqlExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallTimeSpan(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Object == null) {
				switch (exp.Method.Name) {
					case "Compare": return $"(({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}) - ({ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)}))";
					case "Equals": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} = {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "FromDays": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} * {(long)1000000 * 60 * 60 * 24})";
					case "FromHours": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} * {(long)1000000 * 60 * 60})";
					case "FromMilliseconds": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} * 1000)";
					case "FromMinutes": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} * {(long)1000000 * 60})";
					case "FromSeconds": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} * 1000000)";
					case "FromTicks": return $"({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} / 10)";
					case "Parse": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as signed)";
					case "ParseExact":
					case "TryParse":
					case "TryParseExact": return $"cast({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} as signed)";
				}
				return null;
			}
			var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, tbtype, isQuoteName);
			var args1 = exp.Arguments.Count == 0 ? null : ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Method.Name) {
				case "Add": return $"({left} + {args1})";
				case "Subtract": return $"({left} - {args1})";
				case "Equals": return $"({left} = {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "CompareTo": return $"(({left}) - ({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}))";
				case "ToString": return $"date_format(date_add(cast('0001/1/1 0:00:00' as datetime), interval ({left}) microsecond), '%Y-%m-%d %H:%i:%s.%f')";
			}
			throw new Exception($"MySqlExpression 未现实函数表达式 {exp} 解析");
			
		}
	}
}
