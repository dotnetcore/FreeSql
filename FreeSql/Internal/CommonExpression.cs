using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FreeSql.Internal {
	internal abstract class CommonExpression {

		internal CommonUtils _common;
		internal CommonExpression(CommonUtils common) {
			_common = common;
		}

		internal bool ReadAnonymousField(List<SelectTableInfo> _tables, StringBuilder field, ReadAnonymousTypeInfo parent, ref int index, Expression exp, Func<Expression[], string> getSelectGroupingMapString) {
			switch (exp.NodeType) {
				case ExpressionType.Quote: return ReadAnonymousField(_tables, field, parent, ref index, (exp as UnaryExpression)?.Operand, getSelectGroupingMapString);
				case ExpressionType.Lambda: return ReadAnonymousField(_tables, field, parent, ref index, (exp as LambdaExpression)?.Body, getSelectGroupingMapString);
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked:
					parent.DbField = $"-({ExpressionLambdaToSql(exp, _tables, null, getSelectGroupingMapString, SelectTableInfoType.From, true)})";
					field.Append(", ").Append(parent.DbField);
					if (index >= 0) field.Append(" as").Append(++index);
					return false;
				case ExpressionType.Convert: return ReadAnonymousField(_tables, field, parent, ref index, (exp as UnaryExpression)?.Operand, getSelectGroupingMapString);
				case ExpressionType.Constant:
					var constExp = exp as ConstantExpression;
					//处理自定义SQL语句，如： ToList(new { 
					//	ccc = "now()", 
					//	partby = "sum(num) over(PARTITION BY server_id,os,rid,chn order by id desc)"
					//})，有缺点即 ccc partby 接受类型都是 string，可配合 Convert.ToXxx 类型转换，请看下面的兼容
					parent.DbField = constExp.Type.FullName == "System.String" ? (constExp.Value?.ToString() ?? "NULL") : _common.FormatSql("{0}", constExp?.Value);
					field.Append(", ").Append(parent.DbField);
					if (index >= 0) field.Append(" as").Append(++index);
					return false;
				case ExpressionType.Call:
					var callExp = exp as MethodCallExpression;
					//处理自定义SQL语句，如： ToList(new { 
					//	ccc = Convert.ToDateTime("now()"), 
					//	partby = Convert.ToDecimal("sum(num) over(PARTITION BY server_id,os,rid,chn order by id desc)")
					//})
					if (callExp.Method?.DeclaringType.FullName == "System.Convert" &&
						callExp.Method.Name.StartsWith("To") &&
						callExp.Arguments[0].NodeType == ExpressionType.Constant &&
						callExp.Arguments[0].Type.FullName == "System.String")
						parent.DbField = (callExp.Arguments[0] as ConstantExpression).Value?.ToString() ?? "NULL";
					else
						parent.DbField = ExpressionLambdaToSql(exp, _tables, null, getSelectGroupingMapString, SelectTableInfoType.From, true);
					field.Append(", ").Append(parent.DbField);
					if (index >= 0) field.Append(" as").Append(++index);
					return false;
				case ExpressionType.MemberAccess:
					if (_common.GetTableByEntity(exp.Type) != null) { //加载表所有字段
						var map = new List<SelectColumnInfo>();
						ExpressionSelectColumn_MemberAccess(_tables, map, SelectTableInfoType.From, exp, true, getSelectGroupingMapString);
						parent.Consturctor = map.First().Table.Table.Type.GetConstructor(new Type[0]);
						parent.ConsturctorType = ReadAnonymousTypeInfoConsturctorType.Properties;
						for (var idx = 0; idx < map.Count; idx++) {
							var child = new ReadAnonymousTypeInfo {
								Property = map.First().Table.Table.Type.GetProperty(map[idx].Column.CsName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance),
								CsName = map[idx].Column.CsName, DbField = $"{map[idx].Table.Alias}.{_common.QuoteSqlName(map[idx].Column.Attribute.Name)}" };
							field.Append(", ").Append(_common.QuoteReadColumn(map[idx].Column.CsType, child.DbField));
							if (index >= 0) field.Append(" as").Append(++index);
							parent.Childs.Add(child);
						}
					} else {
						parent.DbField = ExpressionLambdaToSql(exp, _tables, null, getSelectGroupingMapString, SelectTableInfoType.From, true);
						field.Append(", ").Append(parent.DbField);
						if (index >= 0) field.Append(" as").Append(++index);
						return false;
					}
					return false;
				case ExpressionType.New:
					var newExp = exp as NewExpression;
					parent.Consturctor = newExp.Type.GetConstructors()[0];
					parent.ConsturctorType = ReadAnonymousTypeInfoConsturctorType.Arguments;
					for (var a = 0; a < newExp.Members.Count; a++) {
						var child = new ReadAnonymousTypeInfo {
							Property = newExp.Type.GetProperty(newExp.Members[a].Name, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance),
							CsName = newExp.Members[a].Name, CsType = newExp.Arguments[a].Type };
						parent.Childs.Add(child);
						ReadAnonymousField(_tables, field, child, ref index, newExp.Arguments[a], getSelectGroupingMapString);
					}
					return true;
			}
			parent.DbField = $"({ExpressionLambdaToSql(exp, _tables, null, getSelectGroupingMapString, SelectTableInfoType.From, true)})";
			field.Append(", ").Append(parent.DbField);
			if (index >= 0) field.Append(" as").Append(++index);
			return false;
		}
		internal object ReadAnonymous(ReadAnonymousTypeInfo parent, DbDataReader dr, ref int index) {
			if (parent.Childs.Any() == false) return dr.GetValue(++index);
			switch (parent.ConsturctorType) {
				case ReadAnonymousTypeInfoConsturctorType.Arguments:
					var args = new object[parent.Childs.Count];
					for (var a = 0; a < parent.Childs.Count; a++) {
						args[a] = Utils.GetDataReaderValue(parent.Childs[a].CsType, ReadAnonymous(parent.Childs[a], dr, ref index));
					}
					return parent.Consturctor.Invoke(args);
				case ReadAnonymousTypeInfoConsturctorType.Properties:
					var ret = parent.Consturctor.Invoke(null);
					for (var b = 0; b < parent.Childs.Count; b++) {
						var prop = parent.Childs[b].Property;
						prop.SetValue(ret, Utils.GetDataReaderValue(prop.PropertyType, ReadAnonymous(parent.Childs[b], dr, ref index)), null);
					}
					return ret;
			}
			return null;
		}

		internal string ExpressionConstant(ConstantExpression exp) => _common.FormatSql("{0}", exp?.Value);

		internal string ExpressionSelectColumn_MemberAccess(List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, Expression exp, bool isQuoteName, Func<Expression[], string> getSelectGroupingMapString) {
			return ExpressionLambdaToSql(exp, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
		}

		internal string[] ExpressionSelectColumns_MemberAccess_New_NewArrayInit(List<SelectTableInfo> _tables, Expression exp, bool isQuoteName, Func<Expression[], string> getSelectGroupingMapString) {
			switch (exp?.NodeType) {
				case ExpressionType.Quote: return ExpressionSelectColumns_MemberAccess_New_NewArrayInit(_tables, (exp as UnaryExpression)?.Operand, isQuoteName, getSelectGroupingMapString);
				case ExpressionType.Lambda: return ExpressionSelectColumns_MemberAccess_New_NewArrayInit(_tables, (exp as LambdaExpression)?.Body, isQuoteName, getSelectGroupingMapString);
				case ExpressionType.Convert: return ExpressionSelectColumns_MemberAccess_New_NewArrayInit(_tables, (exp as UnaryExpression)?.Operand, isQuoteName, getSelectGroupingMapString);
				case ExpressionType.Constant: return new[] { ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, isQuoteName, getSelectGroupingMapString) };
				case ExpressionType.MemberAccess: return new[] { ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, isQuoteName, getSelectGroupingMapString) };
				case ExpressionType.New:
					var newExp = exp as NewExpression;
					if (newExp == null) break;
					var newExpMembers = new string[newExp.Members.Count];
					for (var a = 0; a < newExpMembers.Length; a++) newExpMembers[a] = ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, newExp.Arguments[a], isQuoteName, getSelectGroupingMapString);
					return newExpMembers;
				case ExpressionType.NewArrayInit:
					var newArr = exp as NewArrayExpression;
					if (newArr == null) break;
					var newArrMembers = new List<string>();
					foreach (var newArrExp in newArr.Expressions) newArrMembers.AddRange(ExpressionSelectColumns_MemberAccess_New_NewArrayInit(_tables, newArrExp, isQuoteName, getSelectGroupingMapString));
					return newArrMembers.ToArray();
			}
			return new string[0];
		}

		static readonly Dictionary<ExpressionType, string> dicExpressionOperator = new Dictionary<ExpressionType, string>() {
			{ ExpressionType.OrElse, "OR" },
			{ ExpressionType.Or, "|" },
			{ ExpressionType.AndAlso, "AND" },
			{ ExpressionType.And, "&" },
			{ ExpressionType.GreaterThan, ">" },
			{ ExpressionType.GreaterThanOrEqual, ">=" },
			{ ExpressionType.LessThan, "<" },
			{ ExpressionType.LessThanOrEqual, "<=" },
			{ ExpressionType.NotEqual, "<>" },
			{ ExpressionType.Add, "+" },
			{ ExpressionType.Subtract, "-" },
			{ ExpressionType.Multiply, "*" },
			{ ExpressionType.Divide, "/" },
			{ ExpressionType.Modulo, "%" },
			{ ExpressionType.Equal, "=" },
		};
		internal string ExpressionWhereLambdaNoneForeignObject(List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Expression exp, Func<Expression[], string> getSelectGroupingMapString) {
			var sql = ExpressionLambdaToSql(exp, _tables, _selectColumnMap, getSelectGroupingMapString, SelectTableInfoType.From, true);
			switch(sql) {
				case "1":
				case "'t'": return "1=1";
				case "0":
				case "'f'": return "1=2";
				default:return sql;
			}
		}

		internal string ExpressionWhereLambda(List<SelectTableInfo> _tables, Expression exp, Func<Expression[], string> getSelectGroupingMapString) {
			var sql = ExpressionLambdaToSql(exp, _tables, null, getSelectGroupingMapString, SelectTableInfoType.From, true);
			switch (sql) {
				case "1":
				case "'t'": return "1=1";
				case "0":
				case "'f'": return "1=2";
				default: return sql;
			}
		}
		internal void ExpressionJoinLambda(List<SelectTableInfo> _tables, SelectTableInfoType tbtype, Expression exp, Func<Expression[], string> getSelectGroupingMapString) {
			var tbidx = _tables.Count;
			var filter = ExpressionLambdaToSql(exp, _tables, null, getSelectGroupingMapString, tbtype, true);
			switch (filter) {
				case "1":
				case "'t'": filter = "1=1"; break;
				case "0":
				case "'f'": filter = "1=2"; break;
				default: break;
			}
			if (_tables.Count > tbidx) {
				_tables[tbidx].Type = tbtype;
				_tables[tbidx].On = filter;
				for (var a = tbidx + 1; a < _tables.Count; a++)
					_tables[a].Type = SelectTableInfoType.From;
			} else {
				var find = _tables.Where((a, c) => c > 0 && (a.Type == tbtype || a.Type == SelectTableInfoType.From) && string.IsNullOrEmpty(a.On)).LastOrDefault();
				if (find != null) {
					find.Type = tbtype;
					find.On = filter;
				}
			}
		}

		internal string ExpressionLambdaToSql(Expression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName) {
			if (exp == null) return "";
			switch (exp.NodeType) {
				case ExpressionType.Not: return $"not({ExpressionLambdaToSql((exp as UnaryExpression)?.Operand, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
				case ExpressionType.Quote: return ExpressionLambdaToSql((exp as UnaryExpression)?.Operand, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
				case ExpressionType.Lambda: return ExpressionLambdaToSql((exp as LambdaExpression)?.Body, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
				case ExpressionType.Convert: return ExpressionLambdaToSql((exp as UnaryExpression)?.Operand, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked: return "-" + ExpressionLambdaToSql((exp as UnaryExpression)?.Operand, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
				case ExpressionType.Constant: return _common.FormatSql("{0}", (exp as ConstantExpression)?.Value);
				case ExpressionType.Conditional:
					var condExp = exp as ConditionalExpression;
					return $"case when {ExpressionLambdaToSql(condExp.Test, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)} then {ExpressionLambdaToSql(condExp.IfTrue, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)} else {ExpressionLambdaToSql(condExp.IfFalse, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)} end";
				case ExpressionType.Call:
					var exp3 = exp as MethodCallExpression;
					var callType = exp3.Object?.Type ?? exp3.Method.DeclaringType;
					switch (callType.FullName) {
						case "System.String": return ExpressionLambdaToSqlCallString(exp3, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
						case "System.Math": return ExpressionLambdaToSqlCallMath(exp3, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
						case "System.DateTime": return ExpressionLambdaToSqlCallDateTime(exp3, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
						case "System.TimeSpan": return ExpressionLambdaToSqlCallTimeSpan(exp3, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
						case "System.Convert": return ExpressionLambdaToSqlCallConvert(exp3, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
					}
					if (callType.FullName.StartsWith("FreeSql.ISelectGroupingAggregate`")) {
						switch (exp3.Method.Name) {
							case "Count": return "count(1)";
							case "Sum": return $"sum({ExpressionLambdaToSql(exp3.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
							case "Avg": return $"avg({ExpressionLambdaToSql(exp3.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
							case "Max": return $"max({ExpressionLambdaToSql(exp3.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
							case "Min": return $"min({ExpressionLambdaToSql(exp3.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName)})";
						}
					}
					if (callType.FullName.StartsWith("FreeSql.ISelect`")) { //子表查询
						if (exp3.Method.Name == "Any") { //exists
							var anyArgs = exp3.Arguments;
							var exp3Stack = new Stack<Expression>();
							var exp3tmp = exp3.Object;
							if (exp3tmp != null && anyArgs.Any())
								exp3Stack.Push(Expression.Call(exp3tmp, callType.GetMethod("Where", anyArgs.Select(a => a.Type).ToArray()), anyArgs.ToArray()));
							while (exp3tmp != null) {
								exp3Stack.Push(exp3tmp);
								switch (exp3tmp.NodeType) {
									case ExpressionType.Call:
										var exp3tmpCall = (exp3tmp as MethodCallExpression);
										exp3tmp = exp3tmpCall.Object == null ? exp3tmpCall.Arguments.FirstOrDefault() : exp3tmpCall.Object;
										continue;
									case ExpressionType.MemberAccess: exp3tmp = (exp3tmp as MemberExpression).Expression; continue;
								}
								break;
							}
							object fsql = null;
							List<SelectTableInfo> fsqltables = null;
							var fsqltable1SetAlias = false;
							Type fsqlType = null;
							while (exp3Stack.Any()) {
								exp3tmp = exp3Stack.Pop();
								if (exp3tmp.Type.FullName.StartsWith("FreeSql.ISelect`") && fsql == null) {
									if (exp3tmp.NodeType == ExpressionType.Call) {
										var exp3tmpCall = (exp3tmp as MethodCallExpression);
										if (exp3tmpCall.Method.Name == "AsSelect" && exp3tmpCall.Object == null) {
											var exp3tmpArg1Type = exp3tmpCall.Arguments.FirstOrDefault()?.Type;
											if (exp3tmpArg1Type != null) {
												var exp3tmpEleType = exp3tmpArg1Type.GetElementType() ?? exp3tmpArg1Type.GenericTypeArguments.FirstOrDefault();
												if (exp3tmpEleType != null) {
													fsql = typeof(IFreeSql).GetMethod("Select", new Type[0]).MakeGenericMethod(exp3tmpEleType).Invoke(_common._orm, null);
												}
											}
										}
									}
									if (fsql == null) fsql = Expression.Lambda(exp3tmp).Compile().DynamicInvoke();
									fsqlType = fsql?.GetType();
									if (fsqlType == null) break;
									fsqlType.GetField("_limit", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fsql, 1);
									fsqltables = fsqlType.GetField("_tables", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(fsql) as List<SelectTableInfo>;
									//fsqltables[0].Alias = $"{_tables[0].Alias}_{fsqltables[0].Alias}";
									fsqltables.AddRange(_tables.Select(a => new SelectTableInfo {
										Alias = a.Type == SelectTableInfoType.Parent ? a.Alias : $"__parent_{a.Alias}_parent__",
										On = "1=1",
										Table = a.Table,
										Type = SelectTableInfoType.Parent
									}));
								} else if (fsqlType != null) {
									var call3Exp = exp3tmp as MethodCallExpression;
									var method = fsqlType.GetMethod(call3Exp.Method.Name, call3Exp.Arguments.Select(a => a.Type).ToArray());
									if (call3Exp.Method.ContainsGenericParameters) method.MakeGenericMethod(call3Exp.Method.GetGenericArguments());
									var parms = method.GetParameters();
									var args = new object[call3Exp.Arguments.Count];
									for (var a = 0; a < args.Length; a++) {
										var argExp = (call3Exp.Arguments[a] as UnaryExpression)?.Operand;
										if (argExp != null && argExp.NodeType == ExpressionType.Lambda) {
											if (fsqltable1SetAlias == false) {
												fsqltables[0].Alias = (argExp as LambdaExpression).Parameters.First().Name;
												fsqltable1SetAlias = true;
											}
										}
										args[a] = argExp;
										//if (args[a] == null) ExpressionLambdaToSql(call3Exp.Arguments[a], fsqltables, null, null, SelectTableInfoType.From, true);
									}
									method.Invoke(fsql, args);
								}
							}
							if (fsql != null) {
								var sql = fsqlType.GetMethod("ToSql", new Type[] { typeof(string) })?.Invoke(fsql, new object[] { "1" })?.ToString();
								if (string.IsNullOrEmpty(sql) == false) {
									foreach (var tb in _tables)
										sql = sql.Replace($"__parent_{tb.Alias}_parent__", tb.Alias);
									return $"exists({sql})";
								}
							}
						}
					}
					var eleType = callType.GetElementType() ?? callType.GenericTypeArguments.FirstOrDefault();
					if (eleType != null && typeof(IEnumerable<>).MakeGenericType(eleType).IsAssignableFrom(callType)) { //集合导航属性子查询
						if (exp3.Method.Name == "Any") { //exists
							
						}
					}
					var other3Exp = ExpressionLambdaToSqlOther(exp3, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
					if (string.IsNullOrEmpty(other3Exp) == false) return other3Exp;
					throw new Exception($"未实现函数表达式 {exp3} 解析");
				case ExpressionType.MemberAccess:
					var exp4 = exp as MemberExpression;
					if (exp4.Expression != null && exp4.Expression.Type.IsArray == false && exp4.Expression.Type.IsNullableType()) return ExpressionLambdaToSql(exp4.Expression, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
					var extRet = "";
					var memberType = exp4.Expression?.Type ?? exp4.Type;
					switch (memberType.FullName) {
						case "System.String": extRet = ExpressionLambdaToSqlMemberAccessString(exp4, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName); break;
						case "System.DateTime": extRet = ExpressionLambdaToSqlMemberAccessDateTime(exp4, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName); break;
						case "System.TimeSpan": extRet = ExpressionLambdaToSqlMemberAccessTimeSpan(exp4, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName); break;
					}
					if (string.IsNullOrEmpty(extRet) == false) return extRet;
					var other4Exp = ExpressionLambdaToSqlOther(exp4, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
					if (string.IsNullOrEmpty(other4Exp) == false) return other4Exp;

					var expStack = new Stack<Expression>();
					expStack.Push(exp);
					MethodCallExpression callExp = null;
					var exp2 = exp4.Expression;
					while (true) {
						switch(exp2.NodeType) {
							case ExpressionType.Constant:
								expStack.Push(exp2);
								break;
							case ExpressionType.Parameter:
								expStack.Push(exp2);
								break;
							case ExpressionType.MemberAccess:
								expStack.Push(exp2);
								exp2 = (exp2 as MemberExpression).Expression;
								if (exp2 == null) break;
								continue;
							case ExpressionType.Call:
								callExp = exp2 as MethodCallExpression;
								expStack.Push(exp2);
								exp2 = callExp.Object;
								if (exp2 == null) break;
								continue;
						}
						break;
					}
					if (expStack.First().NodeType != ExpressionType.Parameter) return _common.FormatSql("{0}", Expression.Lambda(exp).Compile().DynamicInvoke());
					if (callExp != null) return ExpressionLambdaToSql(callExp, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
					if (getSelectGroupingMapString != null && expStack.First().Type.FullName.StartsWith("FreeSql.ISelectGroupingAggregate`")) {
						if (getSelectGroupingMapString != null) {
							var expText = getSelectGroupingMapString(expStack.Where((a, b) => b >= 2).ToArray());
							if (string.IsNullOrEmpty(expText) == false) return expText;
						}
					}

					if (_tables == null) {
						var pp = expStack.Pop() as ParameterExpression;
						var memberExp = expStack.Pop() as MemberExpression;
						var tb = _common.GetTableByEntity(pp.Type);
						if (tb.ColumnsByCs.ContainsKey(memberExp.Member.Name) == false) throw new ArgumentException($"{tb.DbName} 找不到列 {memberExp.Member.Name}");
						if (_selectColumnMap != null) {
							_selectColumnMap.Add(new SelectColumnInfo { Table = null, Column = tb.ColumnsByCs[memberExp.Member.Name] });
						}
						var name = tb.ColumnsByCs[memberExp.Member.Name].Attribute.Name;
						if (isQuoteName) name = _common.QuoteSqlName(name);
						return name;
					}
					Func<TableInfo, string, bool, SelectTableInfo> getOrAddTable = (tbtmp, alias, isa) => {
						var finds = _tables.Where((a2, c2) => (isa || c2 > 0) && a2.Table.CsName == tbtmp.CsName).ToArray(); //外部表，内部表一起查
						if (finds.Length > 1) {
							finds = _tables.Where((a2, c2) => a2.Table.CsName == tbtmp.CsName && a2.Type == SelectTableInfoType.Parent && a2.Alias == $"__parent_{alias}_parent__").ToArray(); //查询外部表
							if (finds.Any() == false) {
								finds = _tables.Where((a2, c2) => (isa || c2 > 0) && a2.Table.CsName == tbtmp.CsName && a2.Type != SelectTableInfoType.Parent).ToArray(); //查询内部表
								if (finds.Length > 1) finds = _tables.Where((a2, c2) => (isa || c2 > 0) && a2.Table.CsName == tbtmp.CsName && a2.Type != SelectTableInfoType.Parent && a2.Alias == alias).ToArray();
							}
						}
						var find = finds.FirstOrDefault();
						if (find == null) _tables.Add(find = new SelectTableInfo { Table = tbtmp, Alias = alias, On = null, Type = tbtype });
						return find;
					};

					TableInfo tb2 = null;
					string alias2 = "", name2 = "";
					SelectTableInfo find2 = null;
					while (expStack.Count > 0) {
						exp2 = expStack.Pop();
						switch (exp2.NodeType) {
							case ExpressionType.Constant:
								throw new NotImplementedException("未实现 MemberAccess 下的 Constant");
							case ExpressionType.Parameter:
							case ExpressionType.MemberAccess:
								
								var exp2Type = exp2.Type;
								if (exp2Type.FullName.StartsWith("FreeSql.ISelectGroupingAggregate`")) exp2Type = exp2Type.GenericTypeArguments.FirstOrDefault() ?? exp2.Type;
								var tb2tmp = _common.GetTableByEntity(exp2Type);
								var mp2 = exp2 as MemberExpression;
								if (mp2?.Member.Name == "Key" && mp2.Expression.Type.FullName.StartsWith("FreeSql.ISelectGroupingAggregate`")) continue;
								if (tb2tmp != null) {
									if (exp2.NodeType == ExpressionType.Parameter) alias2 = (exp2 as ParameterExpression).Name;
									else alias2 = $"{alias2}__{mp2.Member.Name}";
									find2 = getOrAddTable(tb2tmp, alias2, exp2.NodeType == ExpressionType.Parameter);
									alias2 = find2.Alias;
									tb2 = tb2tmp;
								}
								if (mp2 == null || expStack.Any()) continue;
								if (tb2.ColumnsByCs.ContainsKey(mp2.Member.Name) == false) { //如果选的是对象，附加所有列
									if (_selectColumnMap != null) {
										var tb3 = _common.GetTableByEntity(mp2.Type);
										if (tb3 != null) {
											var find3 = getOrAddTable(tb2tmp, $"{alias2}__{mp2.Member.Name}", exp2.NodeType == ExpressionType.Parameter);

											foreach (var tb3c in tb3.Columns.Values)
												_selectColumnMap.Add(new SelectColumnInfo { Table = find3, Column = tb3c });
											if (tb3.Columns.Any()) return "";
										}
									}
									throw new ArgumentException($"{tb2.DbName} 找不到列 {mp2.Member.Name}");
								}
								var col2 = tb2.ColumnsByCs[mp2.Member.Name];
								if (_selectColumnMap != null && find2 != null) {
									_selectColumnMap.Add(new SelectColumnInfo { Table = find2, Column = col2 });
									return "";
								}
								name2 = tb2.ColumnsByCs[mp2.Member.Name].Attribute.Name;
								break;
							case ExpressionType.Call:break;
						}
					}
					if (isQuoteName) name2 = _common.QuoteSqlName(name2);
					return $"{alias2}.{name2}";
			}
			var expBinary = exp as BinaryExpression;
			if (expBinary == null) {
				var other99Exp = ExpressionLambdaToSqlOther(exp, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
				if (string.IsNullOrEmpty(other99Exp) == false) return other99Exp;
				return "";
			}
			if (expBinary.NodeType == ExpressionType.Coalesce) {
				return _common.IsNull(
					ExpressionLambdaToSql(expBinary.Left, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName),
					ExpressionLambdaToSql(expBinary.Right, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName));
			}
			if (dicExpressionOperator.TryGetValue(expBinary.NodeType, out var tryoper) == false) return "";
			var left = ExpressionLambdaToSql(expBinary.Left, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
			var right = ExpressionLambdaToSql(expBinary.Right, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName);
			if (left == "NULL") {
				var tmp = right;
				right = left;
				left = tmp;
			}
			if (right == "NULL") tryoper = tryoper == "=" ? " IS " : " IS NOT ";
			if (tryoper == "+" && (expBinary.Left.Type.FullName == "System.String" || expBinary.Right.Type.FullName == "System.String")) return _common.StringConcat(left, right, expBinary.Left.Type, expBinary.Right.Type);
			if (tryoper == "%") return _common.Mod(left, right, expBinary.Left.Type, expBinary.Right.Type);
			return $"{left} {tryoper} {right}";
		}

		internal abstract string ExpressionLambdaToSqlMemberAccessString(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName);
		internal abstract string ExpressionLambdaToSqlMemberAccessDateTime(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName);
		internal abstract string ExpressionLambdaToSqlMemberAccessTimeSpan(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName);
		internal abstract string ExpressionLambdaToSqlCallString(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName);
		internal abstract string ExpressionLambdaToSqlCallMath(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName);
		internal abstract string ExpressionLambdaToSqlCallDateTime(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName);
		internal abstract string ExpressionLambdaToSqlCallTimeSpan(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName);
		internal abstract string ExpressionLambdaToSqlCallConvert(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName);
		internal abstract string ExpressionLambdaToSqlOther(Expression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName);
	}
}
