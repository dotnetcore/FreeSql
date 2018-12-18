using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql.PostgreSQL {
	class PostgreSQLExpression : CommonExpression {

		public PostgreSQLExpression(CommonUtils common) : base(common) { }

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

			//dayofweek = DayOfWeek
			//dayofmonth = Day
			//dayofyear = DayOfYear
			//month = Month
			//year = Year
			//hour = Hour
			//minute = Minute
			//second = Second
			/*
			 * date_add(date,interval expr type)  
			date_sub(date,interval expr type)    
			adddate(date,interval expr type)    
			subdate(date,interval expr type)  
			对日期时间进行加减法运算  
			(adddate()和subdate()是date_add()和date_sub()的同义词,也
			可以用运算符+和-而不是函数  
			date是一个datetime或date值,expr对date进行加减法的一个表
			达式字符串type指明表达式expr应该如何被解释  
			　[type值 含义 期望的expr格式]:  
			　second 秒 seconds    
			　minute 分钟 minutes    
			　hour 时间 hours    
			　day 天 days    
			　month 月 months    
			　year 年 years    
			　minute_second 分钟和秒 "minutes:seconds"    
			　hour_minute 小时和分钟 "hours:minutes"    
			　day_hour 天和小时 "days hours"    
			　year_month 年和月 "years-months"    
			　hour_second 小时, 分钟， "hours:minutes:seconds"    
			　day_minute 天, 小时, 分钟 "days hours:minutes"    
			　day_second 天, 小时, 分钟, 秒 "days
			 hours:minutes:seconds" 
　expr中允许任何标点做分隔符,如果所有是date值时结果是一个
date值,否则结果是一个datetime值)  
　如果type关键词不完整,则mysql从右端取值,day_second因为缺
少小时分钟等于minute_second)  
　如果增加month、year_month或year,天数大于结果月份的最大天
数则使用最大天数)    
mysql> select "1997-12-31 23:59:59" + interval 1 second;  

　　-> 1998-01-01 00:00:00    
mysql> select interval 1 day + "1997-12-31";    
　　-> 1998-01-01    
mysql> select "1998-01-01" - interval 1 second;    
　　-> 1997-12-31 23:59:59    
mysql> select date_add("1997-12-31 23:59:59",interval 1
second);    
　　-> 1998-01-01 00:00:00    
mysql> select date_add("1997-12-31 23:59:59",interval 1
day);    
　　-> 1998-01-01 23:59:59    
mysql> select date_add("1997-12-31 23:59:59",interval
"1:1" minute_second);    
　　-> 1998-01-01 00:01:00    
mysql> select date_sub("1998-01-01 00:00:00",interval "1
1:1:1" day_second);    
　　-> 1997-12-30 22:58:59    
mysql> select date_add("1998-01-01 00:00:00", interval "-1
10" day_hour);  
　　-> 1997-12-30 14:00:00    
mysql> select date_sub("1998-01-02", interval 31 day);    
　　-> 1997-12-02    
mysql> select extract(year from "1999-07-02");    
　　-> 1999    
mysql> select extract(year_month from "1999-07-02
01:02:03");    
　　-> 199907    
mysql> select extract(day_minute from "1999-07-02
01:02:03");    
　　-> 20102    
			 */

			//convert
			var xxx = DateTime.Now.ToString("");


			throw new Exception($"MySqlExpression 未现实函数表达式 {exp} 解析");
		}
	}
}
