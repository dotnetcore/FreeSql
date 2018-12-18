using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql.SqlServer {
	class SqlServerExpression : CommonExpression {

		public SqlServerExpression(CommonUtils common) : base(common) { }

		internal override string ExpressionLambdaToSqlCall(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp.Object.Type.FullName == "System.String") {
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
						return $"({left}) like concat('%', {args0Value}, '%')";
					case "ToLower": return $"lower({left})";
					case "ToUpper": return $"upper({left})";
					case "Substring": return $"substr({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)} + 1, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Length": return $"char_length({left})";
					case "IndexOf":
						var indexOfFindStr = ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName);
						if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32") return $"(locate({left}, {indexOfFindStr}, ParseLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName) + 1) - 1)";
						return $"(locate({left}, {indexOfFindStr}) - 1)";
					case "PadLeft": return $"lpad({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "PadRight": return $"rpad({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Trim":
					case "TrimStart":
					case "TrimEnd":
						if (exp.Arguments.Count == 0) {
							if (exp.Method.Name == "Trim") return $"trim({left})";
							if (exp.Method.Name == "TrimStart") return $"ltrim({left})";
							if (exp.Method.Name == "TrimStart") return $"rtrim({left})";
						}
						foreach (var argsTrim01 in exp.Arguments) {
							if (exp.Method.Name == "Trim") left = $"trim({ExpressionLambdaToSql(argsTrim01, _tables, _selectColumnMap, tbtype, isQuoteName)} from {left})";
							if (exp.Method.Name == "TrimStart") left = $"trim(leading {ExpressionLambdaToSql(argsTrim01, _tables, _selectColumnMap, tbtype, isQuoteName)} from {left})";
							if (exp.Method.Name == "TrimStart") left = $"trim(trailing {ExpressionLambdaToSql(argsTrim01, _tables, _selectColumnMap, tbtype, isQuoteName)} from {left})";
						}
						return left;
					case "Replace": return $"replace({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "CompareTo": return $"strcmp({left}, {ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
				}
			}

			if (exp.Object.Type.FullName == "System.Math") {
				switch (exp.Method.Name) {
					case "Abs": return $"abs({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Sign": return $"sign({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Floor": return $"floor({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Round":
						if (exp.Arguments.Count > 1 && exp.Arguments[1].Type.FullName == "System.Int32") return $"round({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
						return $"round({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Exp": return $"exp({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Log": return $"log({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Log10": return $"log10({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Pow": return $"pow({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Sqrt": return $"sqrt({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "PI": return $"pi()";
					case "Cos": return $"cos({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Sin": return $"sin({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Tan": return $"tan({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Acos": return $"acos({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Asin": return $"asin({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Atan": return $"atan({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Atan2": return $"atan2({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, {ExpressionLambdaToSql(exp.Arguments[1], _tables, _selectColumnMap, tbtype, isQuoteName)})";
					case "Truncate": return $"truncate({ExpressionLambdaToSql(exp.Arguments[0], _tables, _selectColumnMap, tbtype, isQuoteName)}, 0)";
				}
			}

			throw new Exception($"SqlServerExpression 未现实函数表达式 {exp} 解析");
		}
	}
}
