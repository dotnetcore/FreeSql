using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FreeSql.PostgreSQL {
	class PostgreSQLExpression : CommonExpression {

		public PostgreSQLExpression(CommonUtils common) : base(common) { }

		internal override string ExpressionLambdaToSqlMemberAccessString(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Member.Name) {
				case "Length": return $"char_length({left})";
			}
			throw new Exception($"PostgreSQLExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlMemberAccessDateTime(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Expression == null) {
				switch (exp.Member.Name) {
					case "Now": return "current_timestamp";
					case "UtcNow": return "(current_timestamp at time zone 'UTC')";
					case "Today": return "current_date";
				}
			}
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Member.Name) {
				case "DayOfWeek": return $"extract(dow from ({left})::timestamp)";
				case "Day": return $"extract(day from ({left})::timestamp)";
				case "DayOfYear": return $"extract(doy from ({left})::timestamp)";
				case "Month": return $"extract(month from ({left})::timestamp)";
				case "Year": return $"extract(year from ({left})::timestamp)";
				case "Hour": return $"extract(hour from ({left})::timestamp)";
				case "Minute": return $"extract(minute from ({left})::timestamp)";
				case "Second": return $"extract(second from ({left})::timestamp)";
				case "Millisecond": return $"(extract(milliseconds from ({left})::timestamp) - extract(second from ({left})::timestamp) * 1000)";
				case "Ticks": return $"(extract(epoch from ({left})::timestamp) * 10000000 + 621355968000000000)";
			}
			throw new Exception($"PostgreSQLExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlMemberAccessTimeSpan(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Member.Name) {
				case "Days": return $"floor(extract(epoch from ({left})::interval) / {60 * 60 * 24})";
				case "Hours": return $"extract(hour from ({left})::interval)";
				case "Milliseconds": return $"(extract(milliseconds from ({left})::interval) - extract(second from ({left})::interval) * 1000)";
				case "Minutes": return $"extract(minute from ({left})::interval)";
				case "Seconds": return $"extract(second from ({left})::interval)";
				case "Ticks": return $"(extract(epoch from ({left})::interval) * 10000000)";
				case "TotalDays": return $"floor(extract(epoch from ({left})::interval) / {60 * 60 * 24})";
				case "TotalHours": return $"floor(extract(epoch from ({left})::interval) / {60 * 60})";
				case "TotalMilliseconds": return $"(epoch from ({left})::interval + extract(milliseconds from ({left})::interval) - extract(second from ({left})::interval) * 1000)";
				case "TotalMinutes": return $"floor(extract(epoch from ({left})::interval) / 60)";
				case "TotalSeconds": return $"extract(epoch from ({left})::interval)";
			}
			throw new Exception($"PostgreSQLExpression 未现实函数表达式 {exp} 解析");
		}

		internal override string ExpressionLambdaToSqlCallString(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Method.Name) {
				case "StartsWith":
				case "EndsWith":
				case "Contains":
					var args0Value = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
					if (args0Value == "NULL") return $"({left}) IS NULL";
					var likeOpt = "LIKE";
					if (exp.Arguments.Count > 1) {
						if (exp.Arguments[1].Type == typeof(bool) ||
							exp.Arguments[1].Type == typeof(StringComparison) && ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName).Contains("IgnoreCase")) likeOpt = "ILIKE";
					}
					if (exp.Method.Name == "StartsWith") return $"({left}) {likeOpt} {(args0Value.EndsWith("'") ? args0Value.Insert(args0Value.Length - 1, "%") : $"(({args0Value})::varchar || '%')")}";
					if (exp.Method.Name == "EndsWith") return $"({left}) {likeOpt} {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"('%' || ({args0Value})::varchar)")}";
					if (args0Value.StartsWith("'") && args0Value.EndsWith("'")) return $"({left}) {likeOpt} {args0Value.Insert(1, "%").Insert(args0Value.Length, "%")}";
					return $"({left}) {likeOpt} ('%' || ({args0Value})::varchar || '%')";
				case "ToLower": return $"lower({left})";
				case "ToUpper": return $"upper({left})";
				case "Substring":
					var substrArgs1 = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
					if (long.TryParse(substrArgs1, out var testtrylng1)) substrArgs1 = (testtrylng1 + 1).ToString();
					else substrArgs1 += " + 1";
					if (exp.Arguments.Count == 1) return $"substr({left}, {substrArgs1})";
					return $"substr({left}, {substrArgs1}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "IndexOf": return $"(strpos({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}) - 1)";
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
					var trimArg = "";
					foreach (var argsTrim02 in exp.Arguments) {
						var argsTrim01s = new[] { argsTrim02 };
						if (argsTrim02.NodeType == ExpressionType.NewArrayInit) {
							var arritem = argsTrim02 as NewArrayExpression;
							argsTrim01s = arritem.Expressions.ToArray();
						}
						foreach (var argsTrim01 in argsTrim01s) {
							var trimChr = ExpressionLambdaToSql(argsTrim01, _tables, _selectColumnMap, tbtype, isQuoteName).Trim('"');
							if (trimChr.Length == 1) trimArg += trimChr;
							else trimArg += $" || ({trimArg})";
						}
					}
					if (exp.Method.Name == "Trim") left = $"trim({left}, {trimArg})";
					if (exp.Method.Name == "TrimStart") left = $"ltrim({left}, {trimArg})";
					if (exp.Method.Name == "TrimEnd") left = $"rtrim({left}, {trimArg})";
					return left;
				case "Replace": return $"replace({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "CompareTo": return $"strcmp({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
			}
			throw new Exception($"PostgreSQLExpression 未现实函数表达式 {exp} 解析");
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
				case "Truncate": return $"trunc({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, 0)";
			}
			throw new Exception($"PostgreSQLExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallDateTime(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, tbtype, isQuoteName);
			var args1 = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Method.Name) {
				case "Add": return $"(({left})::timestamp + ({args1})::interval)";
				case "AddDays": return $"(({left})::timestamp + (({args1}) || ' day')::interval)";
				case "AddHours": return $"(({left})::timestamp + (({args1}) || ' hour')::interval)";
				case "AddMilliseconds": return $"(({left})::timestamp + (({args1}) || ' milliseconds')::interval)";
				case "AddMinutes": return $"(({left})::timestamp + (({args1}) || ' minute')::interval)";
				case "AddMonths": return $"(({left})::timestamp + (({args1}) || ' month')::interval)";
				case "AddSeconds": return $"(({left})::timestamp + (({args1}) || ' second')::interval)";
				case "AddTicks": return $"(({left})::timestamp + (({args1}) / 10 || ' microseconds')::interval)";
				case "AddYears": return $"(({left})::timestamp + (({args1}) || ' year')::interval)";
				case "Subtract":
					if (exp.Arguments[0].Type.FullName == "System.DateTime" || exp.Arguments[0].Type.GenericTypeArguments.FirstOrDefault()?.FullName == "System.DateTime")
						return $"(({left})::timestamp - ({args1})::timestamp)";
					if (exp.Arguments[0].Type.FullName == "System.TimeSpan" || exp.Arguments[0].Type.GenericTypeArguments.FirstOrDefault()?.FullName == "System.TimeSpan")
						return $"(({left})::timestamp - ({args1})::interval)";
					break;
			}
			throw new Exception($"PostgreSQLExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallTimeSpan(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, tbtype, isQuoteName);
			var args1 = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Method.Name) {
				case "Add": return $"(({left})::interval + ({args1})::interval)";
				case "Subtract": return $"(({left})::interval - ({args1})::interval)";
			}
			throw new Exception($"PostgreSQLExpression 未现实函数表达式 {exp} 解析");
		}
	}
}
