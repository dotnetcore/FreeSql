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
			throw new Exception($"SqlServerExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlMemberAccessDateTime(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Expression == null) {
				switch (exp.Member.Name) {
					case "Now": return "getdate()";
					case "UtcNow": return "getutcdate()";
					case "Today": return "cast(convert(char(10),getdate(),120) as date)()";
				}
				return null;
			}
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Member.Name) {
				case "DayOfWeek": return $"(datepart(weekday, {left}) - 1)";
				case "Day": return $"datepart(day, {left})";
				case "DayOfYear": return $"datepart(dayofyear, {left})";
				case "Month": return $"datepart(month, {left})";
				case "Year": return $"datepart(year, {left})";
				case "Hour": return $"datepart(hour, {left})";
				case "Minute": return $"datepart(minute, {left})";
				case "Second": return $"datepart(second, {left})";
				case "Millisecond": return $"datepart(millisecond, {left})";
				case "Ticks": return $"(datediff(second, '1970-1-1', {left}) * 10000000 + 621355968000000000)";
			}
			throw new Exception($"SqlServerExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlMemberAccessTimeSpan(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			var left = ExpressionLambdaToSql(exp.Expression, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Member.Name) {
				case "Days": return $"datediff(day, '1970-1-1', {left})";
				case "Hours": return $"datepart(hour, '1970-1-1', {left})";
				case "Milliseconds": return $"datepart(millisecond, {left})";
				case "Minutes": return $"datepart(minute, {left})";
				case "Seconds": return $"datepart(second, {left})";
				case "Ticks": return $"(datediff(millisecond, '1970-1-1', {left}) * 10000)";
				case "TotalDays": return $"datediff(day, '1970-1-1', {left})";
				case "TotalHours": return $"datediff(hour, '1970-1-1', {left})";
				case "TotalMilliseconds": return $"datediff(millisecond, '1970-1-1', {left})";
				case "TotalMinutes": return $"datediff(minute, '1970-1-1', {left})";
				case "TotalSeconds": return $"datediff(second, '1970-1-1', {left})";
			}
			throw new Exception($"SqlServerExpression 未现实函数表达式 {exp} 解析");
		}

		internal override string ExpressionLambdaToSqlCallString(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Method.Name) {
				case "StartsWith":
				case "EndsWith":
				case "Contains":
					var args0Value = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
					if (args0Value == "NULL") return $"({left}) IS NULL";
					if (exp.Method.Name == "StartsWith") return $"({left}) LIKE {(args0Value.EndsWith("'") ? args0Value.Insert(args0Value.Length - 1, "%") : $"(cast({args0Value} as nvarchar) + '%')")}";
					if (exp.Method.Name == "EndsWith") return $"({left}) LIKE {(args0Value.StartsWith("'") ? args0Value.Insert(1, "%") : $"('%' + cast({args0Value} as nvarchar))")}";
					if (args0Value.StartsWith("'") && args0Value.EndsWith("'")) return $"({left}) LIKE {args0Value.Insert(1, "%").Insert(args0Value.Length, "%")}";
					return $"({left}) LIKE ('%' + cast({args0Value} as nvarchar) + '%')";
				case "ToLower": return $"lower({left})";
				case "ToUpper": return $"upper({left})";
				case "Substring":
					var substrArgs1 = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
					if (long.TryParse(substrArgs1, out var testtrylng1)) substrArgs1 = (testtrylng1 + 1).ToString();
					else substrArgs1 += " + 1";
					if (exp.Arguments.Count == 1) return $"substring({left}, {substrArgs1})";
					return $"substring({left}, {substrArgs1}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				case "IndexOf":
					var indexOfFindStr = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
					if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32") return $"(charindex({left}, {indexOfFindStr}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)} + 1) - 1)";
					return $"(locate({left}, {indexOfFindStr}) - 1)";
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
				case "CompareTo": return $"strcmp({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
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
					return $"round({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
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
				case "Truncate": return $"floor({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, 0)";
			}
			throw new Exception($"SqlServerExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallDateTime(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, tbtype, isQuoteName);
			var args1 = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Method.Name) {
				case "Add": return $"dateadd(millisecond, datediff(millisecond, '1970-1-1', {args1}), {left})";
				case "AddDays": return $"dateadd(day, {args1}, {left})";
				case "AddHours": return $"dateadd(hour, {args1}, {left})";
				case "AddMilliseconds": return $"dateadd(millisecond, {args1}, {left})";
				case "AddMinutes": return $"dateadd(minute, {args1}, {left})";
				case "AddMonths": return $"dateadd(month, {args1}, {left})";
				case "AddSeconds": return $"dateadd(second, {args1}, {left})";
				case "AddTicks": return $"dateadd(millisecond, {args1} / 10000, {left})";
				case "AddYears": return $"dateadd(year, {args1}, {left})";
				case "Subtract": return $"dateadd(millisecond, -datediff(millisecond, '1970-1-1', {args1}), {left})";
			}
			throw new Exception($"SqlServerExpression 未现实函数表达式 {exp} 解析");
		}
		internal override string ExpressionLambdaToSqlCallTimeSpan(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			var left = ExpressionLambdaToSql(exp.Object, _tables, _selectColumnMap, tbtype, isQuoteName);
			var args1 = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
			switch (exp.Method.Name) {
				case "Add": return $"dateadd(millisecond, datediff(millisecond, '1970-1-1', {args1}), {left})";
				case "Subtract": return $"dateadd(millisecond, -datediff(millisecond, '1970-1-1', {args1}), {left})";
			}
			throw new Exception($"SqlServerExpression 未现实函数表达式 {exp} 解析");
		}
	}
}
