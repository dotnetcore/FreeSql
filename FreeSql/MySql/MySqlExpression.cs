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
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Member.Name) {
				case "Length": return $"char_length({left})";
			}
			return null;
		}

		internal override string ExpressionLambdaToSqlMemberAccessMath(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Member.Name) {
				case "PI": return $"pi()";
			}
			return null;
		}

		internal override string ExpressionLambdaToSqlMemberAccessDateTime(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Expression == null && exp.Member.Name == "Now") return "now()";
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Member.Name) {
				case "DayOfWeek": return $"(dayofweek({left}) - 1)";
				case "Day": return $"dayofmonth({left})";
				case "DayOfYear": return $"dayofyear({left})";
				case "Month": return $"month({left})";
				case "Year": return $"year({left})";
				case "Hour": return $"hour({left})";
				case "Minute": return $"minute({left})";
				case "Second": return $"second({left})";
				case "Millisecond": return $"floor(microsecond({left}) / 1000)";
				case "Ticks": return $"(time_to_sec({left}) * 10000000 + 621355968000000000)";
			}
			return null;
		}

		internal override string ExpressionLambdaToSqlMemberAccessTimeSpan(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Member.Name) {
				case "Days": return $"floor(time_to_sec({left}) / {60 * 60 * 24})";
				case "Hours": return $"extract(hour from {left})";
				case "Milliseconds": return $"floor(extract(microsecond from {left}) / 1000)";
				case "Minutes": return $"extract(minute from {left})";
				case "Seconds": return $"extract(second from {left})";
				case "Ticks": return $"(time_to_sec({left}) * 10000000 + extract(microsecond from {left}) * 10)";
				case "TotalDays": return $"floor(extract(hour from {left}) / 24)";
				case "TotalHours": return $"floor(time_to_sec({left}) / {60 * 60})";
				case "TotalMilliseconds": return $"(time_to_sec({left}) * 1000 + floor(extract(microsecond from {left}) / 1000))";
				case "TotalMinutes": return $"floor(time_to_sec({left}) / 60)";
				case "TotalSeconds": return $"time_to_sec({left})";
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
					if (exp.Method.Name == "StartsWith") return $"({left}) LIKE {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"concat('%', {args0Value})")}";
					if (exp.Method.Name == "EndsWith") return $"({left}) LIKE {(args0Value.EndsWith("'") ? args0Value.Insert(args0Value.Length - 1, "%") : $"concat({args0Value}, '%')")}";
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
			var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, tbtype, isQuoteName);
			var args1 = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Method.Name) {
				case "Add": return $"date_add({left}, interval (time_to_sec({args1}) * 1000000 + extract(microsecond from {args1})) microsecond)";
				case "AddDays": return $"date_add({left}, interval {args1} day)";
				case "AddHours": return $"date_add({left}, interval {args1} hour)";
				case "AddMilliseconds": return $"date_add({left}, interval {args1} microsecond)";
				case "AddMinutes": return $"date_add({left}, interval {args1} minute)";
				case "AddMonths": return $"date_add({left}, interval {args1} month)";
				case "AddSeconds": return $"date_add({left}, interval {args1} second)";
				case "AddTicks": return $"date_add({left}, interval ({args1}) / 10 microsecond)";
				case "AddYears": return $"date_add({left}, interval {args1} year)";
				case "Subtract":
					if (exp.Arguments[0].Type.FullName == "System.DateTime" || exp.Arguments[0].Type.GenericTypeArguments.FirstOrDefault()?.FullName == "System.DateTime")
						return $"({left} - {args1})";
					if (exp.Arguments[0].Type.FullName == "System.TimeSpan" || exp.Arguments[0].Type.GenericTypeArguments.FirstOrDefault()?.FullName == "System.TimeSpan")
						return $"date_sub({left}, interval (time_to_sec({args1}) * 1000000 + extract(microsecond from {args1})) microsecond)";
					break;
			}
			throw new Exception($"MySqlExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallTimeSpan(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, tbtype, isQuoteName);
			var args1 = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Method.Name) {
				case "Add": return $"(date_add('1970-1-1', interval (time_to_sec({left}) * 1000000 + extract(microsecond from {left}) + time_to_sec({args1}) * 1000000 + extract(microsecond from {args1})) microsecond)) microsecond) - date_add('1970-1-1', interval 0 microsecond) second))";
				case "Subtract": return $"(date_add('1970-1-1', interval (time_to_sec({left}) * 1000000 + extract(microsecond from {left}) - time_to_sec({args1}) * 1000000 - extract(microsecond from {args1})) microsecond) - date_add('1970-1-1', interval 0 second))";
			}
			throw new Exception($"MySqlExpression 未现实函数表达式 {exp} 解析");
		}
	}
}
