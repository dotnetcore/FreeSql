using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
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

		static ConcurrentDictionary<Type, PropertyInfo[]> _dicReadAnonymousFieldDtoPropertys = new ConcurrentDictionary<Type, PropertyInfo[]>();
		internal bool ReadAnonymousField(List<SelectTableInfo> _tables, StringBuilder field, ReadAnonymousTypeInfo parent, ref int index, Expression exp, Func<Expression[], string> getSelectGroupingMapString) {
			switch (exp.NodeType) {
				case ExpressionType.Quote: return ReadAnonymousField(_tables, field, parent, ref index, (exp as UnaryExpression)?.Operand, getSelectGroupingMapString);
				case ExpressionType.Lambda: return ReadAnonymousField(_tables, field, parent, ref index, (exp as LambdaExpression)?.Body, getSelectGroupingMapString);
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked:
					parent.DbField = $"-({ExpressionLambdaToSql(exp, _tables, null, getSelectGroupingMapString, SelectTableInfoType.From, true, false, ExpressionStyle.Where)})";
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
						parent.DbField = ExpressionLambdaToSql(exp, _tables, null, getSelectGroupingMapString, SelectTableInfoType.From, true, false, ExpressionStyle.Where);
					field.Append(", ").Append(parent.DbField);
					if (index >= 0) field.Append(" as").Append(++index);
					return false;
				case ExpressionType.Parameter:
				case ExpressionType.MemberAccess:
					if (_common.GetTableByEntity(exp.Type) != null) { //加载表所有字段
						var map = new List<SelectColumnInfo>();
						ExpressionSelectColumn_MemberAccess(_tables, map, SelectTableInfoType.From, exp, true, getSelectGroupingMapString);
						var tb = parent.Table = map.First().Table.Table;
						parent.Consturctor = tb.Type.GetConstructor(new Type[0]);
						parent.ConsturctorType = ReadAnonymousTypeInfoConsturctorType.Properties;
						for (var idx = 0; idx < map.Count; idx++) {
							var child = new ReadAnonymousTypeInfo {
								Property = tb.Properties.TryGetValue(map[idx].Column.CsName, out var tryprop) ? tryprop : tb.Type.GetProperty(map[idx].Column.CsName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance),
								CsName = map[idx].Column.CsName, DbField = $"{map[idx].Table.Alias}.{_common.QuoteSqlName(map[idx].Column.Attribute.Name)}",
								CsType = map[idx].Column.CsType
							};
							field.Append(", ").Append(_common.QuoteReadColumn(map[idx].Column.CsType, child.DbField));
							if (index >= 0) field.Append(" as").Append(++index);
							parent.Childs.Add(child);
						}
					} else {
						parent.CsType = exp.Type;
						parent.DbField = ExpressionLambdaToSql(exp, _tables, null, getSelectGroupingMapString, SelectTableInfoType.From, true, false, ExpressionStyle.Where);
						field.Append(", ").Append(parent.DbField);
						if (index >= 0) field.Append(" as").Append(++index);
						return false;
					}
					return false;
				case ExpressionType.MemberInit:
					var initExp = exp as MemberInitExpression;
					parent.Consturctor = initExp.NewExpression.Type.GetConstructors()[0];
					parent.ConsturctorType = ReadAnonymousTypeInfoConsturctorType.Properties;
					if (initExp.Bindings?.Count > 0) {
						//指定 dto映射
						for (var a = 0; a < initExp.Bindings.Count; a++) {
							var initAssignExp = (initExp.Bindings[a] as MemberAssignment);
							if (initAssignExp == null) continue;
							var child = new ReadAnonymousTypeInfo {
								Property = initExp.Type.GetProperty(initExp.Bindings[a].Member.Name, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance),
								CsName = initExp.Bindings[a].Member.Name,
								CsType = initAssignExp.Expression.Type
							};
							parent.Childs.Add(child);
							ReadAnonymousField(_tables, field, child, ref index, initAssignExp.Expression, getSelectGroupingMapString);
						}
					} else {
						//dto 映射
						var dtoProps = _dicReadAnonymousFieldDtoPropertys.GetOrAdd(initExp.NewExpression.Type, dtoType => dtoType.GetProperties());
						foreach (var dtoProp in dtoProps) {
							foreach (var dtTb in _tables) {
								if (dtTb.Table.Columns.TryGetValue(dtoProp.Name, out var trydtocol)) {
									var child = new ReadAnonymousTypeInfo {
										Property = dtoProp,
										CsName = dtoProp.Name,
										CsType = dtoProp.PropertyType
									};
									parent.Childs.Add(child);
									if (dtTb.Parameter != null)
										ReadAnonymousField(_tables, field, child, ref index, Expression.Property(dtTb.Parameter, dtTb.Table.Properties[trydtocol.CsName]), getSelectGroupingMapString);
									else {
										child.DbField = $"{dtTb.Alias}.{_common.QuoteSqlName(trydtocol.Attribute.Name)}";
										field.Append(", ").Append(child.DbField);
										if (index >= 0) field.Append(" as").Append(++index);
									}
									break;
								}
							}
						}
						if (parent.Childs.Any() == false) throw new Exception($"映射异常：{initExp.NewExpression.Type.Name} 没有一个属性名相同");
					}
					return true;
				case ExpressionType.New:
					var newExp = exp as NewExpression;
					parent.Consturctor = newExp.Type.GetConstructors()[0];
					parent.ConsturctorType = ReadAnonymousTypeInfoConsturctorType.Arguments;
					if (newExp.Members?.Count > 0) {
						for (var a = 0; a < newExp.Members.Count; a++) {
							var child = new ReadAnonymousTypeInfo {
								Property = newExp.Type.GetProperty(newExp.Members[a].Name, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance),
								CsName = newExp.Members[a].Name,
								CsType = newExp.Arguments[a].Type
							};
							parent.Childs.Add(child);
							ReadAnonymousField(_tables, field, child, ref index, newExp.Arguments[a], getSelectGroupingMapString);
						}
					} else {
						//dto 映射
						parent.ConsturctorType = ReadAnonymousTypeInfoConsturctorType.Properties;
						var dtoProps = _dicReadAnonymousFieldDtoPropertys.GetOrAdd(newExp.Type, dtoType => dtoType.GetProperties());
						foreach (var dtoProp in dtoProps) {
							foreach(var dtTb in _tables) {
								if (dtTb.Table.ColumnsByCs.TryGetValue(dtoProp.Name, out var trydtocol)) {
									var child = new ReadAnonymousTypeInfo {
										Property = dtoProp,
										CsName = dtoProp.Name,
										CsType = dtoProp.PropertyType
									};
									parent.Childs.Add(child);
									if (dtTb.Parameter != null)
										ReadAnonymousField(_tables, field, child, ref index, Expression.Property(dtTb.Parameter, dtTb.Table.Properties[trydtocol.CsName]), getSelectGroupingMapString);
									else {
										child.DbField = $"{dtTb.Alias}.{_common.QuoteSqlName(trydtocol.Attribute.Name)}";
										field.Append(", ").Append(child.DbField);
										if (index >= 0) field.Append(" as").Append(++index);
									}
									break;
								}
							}
						}
						if (parent.Childs.Any() == false) throw new Exception($"映射异常：{newExp.Type.Name} 没有一个属性名相同");
					}
					return true;
			}
			parent.DbField = $"({ExpressionLambdaToSql(exp, _tables, null, getSelectGroupingMapString, SelectTableInfoType.From, true, false, ExpressionStyle.Where)})";
			field.Append(", ").Append(parent.DbField);
			if (index >= 0) field.Append(" as").Append(++index);
			return false;
		}
		internal object ReadAnonymous(ReadAnonymousTypeInfo parent, DbDataReader dr, ref int index, bool notRead) {
			if (parent.Childs.Any() == false) {
				if (notRead) {
					++index;
					return Utils.GetDataReaderValue(parent.CsType, null);
				}
				return Utils.GetDataReaderValue(parent.CsType, dr.GetValue(++index));
			}
			switch (parent.ConsturctorType) {
				case ReadAnonymousTypeInfoConsturctorType.Arguments:
					var args = new object[parent.Childs.Count];
					for (var a = 0; a < parent.Childs.Count; a++) {
						var objval = ReadAnonymous(parent.Childs[a], dr, ref index, notRead);
						if (notRead == false)
							args[a] = objval;
					}
					return parent.Consturctor.Invoke(args);
				case ReadAnonymousTypeInfoConsturctorType.Properties:
					var ret = parent.Consturctor.Invoke(null);
					var isnull = notRead;
					for (var b = 0; b < parent.Childs.Count; b++) {
						var prop = parent.Childs[b].Property;
						var objval = ReadAnonymous(parent.Childs[b], dr, ref index, notRead);
						if (isnull == false && objval == null && parent.Table != null && parent.Table.ColumnsByCs.TryGetValue(parent.Childs[b].CsName, out var trycol) && trycol.Attribute.IsPrimary)
							isnull = true;
						if (isnull == false)
							prop.SetValue(ret, objval, null);
					}
					return isnull ? null : ret;
			}
			return null;
		}

		internal string ExpressionConstant(ConstantExpression exp) => _common.FormatSql("{0}", exp?.Value);

		internal string ExpressionSelectColumn_MemberAccess(List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, Expression exp, bool isQuoteName, Func<Expression[], string> getSelectGroupingMapString) {
			return ExpressionLambdaToSql(exp, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, false, ExpressionStyle.SelectColumns);
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
			var sql = ExpressionLambdaToSql(exp, _tables, _selectColumnMap, getSelectGroupingMapString, SelectTableInfoType.From, true, false, ExpressionStyle.Where);
			switch(sql) {
				case "1":
				case "'t'": return "1=1";
				case "0":
				case "'f'": return "1=2";
				default:return sql;
			}
		}

		internal string ExpressionWhereLambda(List<SelectTableInfo> _tables, Expression exp, Func<Expression[], string> getSelectGroupingMapString) {
			var sql = ExpressionLambdaToSql(exp, _tables, null, getSelectGroupingMapString, SelectTableInfoType.From, true, false, ExpressionStyle.Where);
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
			var filter = ExpressionLambdaToSql(exp, _tables, null, getSelectGroupingMapString, tbtype, true, false, ExpressionStyle.Where);
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
		static ConcurrentDictionary<Type, MethodInfo> _dicExpressionLambdaToSqlAsSelectMethodInfo = new ConcurrentDictionary<Type, MethodInfo>();
		static ConcurrentDictionary<Type, MethodInfo> _dicExpressionLambdaToSqlAsSelectWhereMethodInfo = new ConcurrentDictionary<Type, MethodInfo>();
		static ConcurrentDictionary<Type, MethodInfo> _dicExpressionLambdaToSqlAsSelectWhereSqlMethodInfo = new ConcurrentDictionary<Type, MethodInfo>();
		static ConcurrentDictionary<Type, MethodInfo> _dicExpressionLambdaToSqlAsSelectAnyMethodInfo = new ConcurrentDictionary<Type, MethodInfo>();
		static ConcurrentDictionary<Type, PropertyInfo> _dicNullableValueProperty = new ConcurrentDictionary<Type, PropertyInfo>();
		static ConcurrentDictionary<Type, Expression> _dicFreeSqlGlobalExtensionsAsSelectExpression = new ConcurrentDictionary<Type, Expression>();
		internal string ExpressionLambdaToSql(Expression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName, bool isDisableDiyParse, ExpressionStyle style) {
			if (exp == null) return "";
			if (isDisableDiyParse == false && _common._orm.Aop.ParseExpression != null) {
				var args = new AopParseExpressionEventArgs(exp, ukexp => ExpressionLambdaToSql(exp, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, true, style));
				_common._orm.Aop.ParseExpression?.Invoke(this, args);
				if (string.IsNullOrEmpty(args.Result) == false) return args.Result;
			}
			switch (exp.NodeType) {
				case ExpressionType.Not: return $"not({ExpressionLambdaToSql((exp as UnaryExpression)?.Operand, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style)})";
				case ExpressionType.Quote: return ExpressionLambdaToSql((exp as UnaryExpression)?.Operand, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
				case ExpressionType.Lambda: return ExpressionLambdaToSql((exp as LambdaExpression)?.Body, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
				case ExpressionType.TypeAs:
				case ExpressionType.Convert:
					//var othercExp = ExpressionLambdaToSqlOther(exp, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
					//if (string.IsNullOrEmpty(othercExp) == false) return othercExp;
					return ExpressionLambdaToSql((exp as UnaryExpression)?.Operand, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked: return "-" + ExpressionLambdaToSql((exp as UnaryExpression)?.Operand, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
				case ExpressionType.Constant: return _common.FormatSql("{0}", (exp as ConstantExpression)?.Value);
				case ExpressionType.Conditional:
					var condExp = exp as ConditionalExpression;
					return $"case when {ExpressionLambdaToSql(condExp.Test, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style)} then {ExpressionLambdaToSql(condExp.IfTrue, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style)} else {ExpressionLambdaToSql(condExp.IfFalse, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style)} end";
				case ExpressionType.Call:
					var exp3 = exp as MethodCallExpression;
					var callType = exp3.Object?.Type ?? exp3.Method.DeclaringType;
					switch (callType.FullName) {
						case "System.String": return ExpressionLambdaToSqlCallString(exp3, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
						case "System.Math": return ExpressionLambdaToSqlCallMath(exp3, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
						case "System.DateTime": return ExpressionLambdaToSqlCallDateTime(exp3, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
						case "System.TimeSpan": return ExpressionLambdaToSqlCallTimeSpan(exp3, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
						case "System.Convert": return ExpressionLambdaToSqlCallConvert(exp3, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
					}
					if (exp3.Method.Name == "Equals" && exp3.Object != null && exp3.Arguments.Count > 0) {
						var tmptryoper = "=";
						var tmpleft = ExpressionLambdaToSql(exp3.Object, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
						var tmpright = ExpressionLambdaToSql(exp3.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
						if (tmpleft == "NULL") {
							var tmp33 = tmpright;
							tmpright = tmpleft;
							tmpleft = tmp33;
						}
						if (tmpright == "NULL") tmptryoper = " IS ";
						return $"{tmpleft} {tmptryoper} {tmpright}";
					}
					if (callType.FullName.StartsWith("FreeSql.ISelectGroupingAggregate`")) {
						switch (exp3.Method.Name) {
							case "Count": return "count(1)";
							case "Sum": return $"sum({ExpressionLambdaToSql(exp3.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style)})";
							case "Avg": return $"avg({ExpressionLambdaToSql(exp3.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style)})";
							case "Max": return $"max({ExpressionLambdaToSql(exp3.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style)})";
							case "Min": return $"min({ExpressionLambdaToSql(exp3.Arguments[0], _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style)})";
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
							Stack<Expression> asSelectBefores = new Stack<Expression>();
							var asSelectSql = "";
							Type asSelectEntityType = null;
							MemberExpression asSelectParentExp1 = null;
							Expression asSelectParentExp = null;
							while (exp3Stack.Any()) {
								exp3tmp = exp3Stack.Pop();
								if (exp3tmp.Type.FullName.StartsWith("FreeSql.ISelect`") && fsql == null) {
									if (exp3tmp.NodeType == ExpressionType.Call) {
										var exp3tmpCall = (exp3tmp as MethodCallExpression);
										if (exp3tmpCall.Method.Name == "AsSelect" && exp3tmpCall.Object == null) {
											var exp3tmpArg1Type = exp3tmpCall.Arguments.FirstOrDefault()?.Type;
											if (exp3tmpArg1Type != null) {
												asSelectEntityType = exp3tmpArg1Type.GetElementType() ?? exp3tmpArg1Type.GenericTypeArguments.FirstOrDefault();
												if (asSelectEntityType != null) {
													fsql = _dicExpressionLambdaToSqlAsSelectMethodInfo.GetOrAdd(asSelectEntityType, asSelectEntityType2 => typeof(IFreeSql).GetMethod("Select", new Type[0]).MakeGenericMethod(asSelectEntityType2))
														.Invoke(_common._orm, null);

													if (asSelectBefores.Any()) {
														asSelectParentExp1 = asSelectBefores.Pop() as MemberExpression;
														if (asSelectBefores.Any()) {
															asSelectParentExp = asSelectBefores.Pop();
															if (asSelectParentExp != null) {
																var testExecuteExp = asSelectParentExp;
																if (asSelectParentExp.NodeType == ExpressionType.Parameter) //执行leftjoin关联
																	testExecuteExp = Expression.Property(testExecuteExp, _common.GetTableByEntity(asSelectParentExp.Type).Properties.First().Value);
																asSelectSql = ExpressionLambdaToSql(testExecuteExp, _tables, new List<SelectColumnInfo>(), getSelectGroupingMapString, SelectTableInfoType.LeftJoin, isQuoteName, true, ExpressionStyle.AsSelect);
															}
														}
													}
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
										Alias = a.Alias,
										On = "1=1",
										Table = a.Table,
										Type = SelectTableInfoType.Parent,
										Parameter = a.Parameter
									}));
								} else if (fsqlType != null) {
									var call3Exp = exp3tmp as MethodCallExpression;
									var method = fsqlType.GetMethod(call3Exp.Method.Name, call3Exp.Arguments.Select(a => a.Type).ToArray());
									if (call3Exp.Method.ContainsGenericParameters) method.MakeGenericMethod(call3Exp.Method.GetGenericArguments());
									var parms = method.GetParameters();
									var args = new object[call3Exp.Arguments.Count];
									for (var a = 0; a < args.Length; a++) {
										var arg3Exp = call3Exp.Arguments[a];
										if (arg3Exp.NodeType == ExpressionType.Constant) {
											args[a] = (arg3Exp as ConstantExpression)?.Value;
										} else {
											var argExp = (arg3Exp as UnaryExpression)?.Operand;
											if (argExp != null && argExp.NodeType == ExpressionType.Lambda) {
												if (fsqltable1SetAlias == false) {
													fsqltables[0].Alias = (argExp as LambdaExpression).Parameters.First().Name;
													fsqltable1SetAlias = true;
												}
											}
											args[a] = argExp;
											//if (args[a] == null) ExpressionLambdaToSql(call3Exp.Arguments[a], fsqltables, null, null, SelectTableInfoType.From, true);
										}
									}
									method.Invoke(fsql, args);
								}
								if (fsql == null) asSelectBefores.Push(exp3tmp);
							}
							if (fsql != null) {
								if (asSelectParentExp != null) { //执行 asSelect() 的关联，OneToMany，ManyToMany
									var fsqlWhere = _dicExpressionLambdaToSqlAsSelectWhereMethodInfo.GetOrAdd(asSelectEntityType, asSelectEntityType3 =>
										typeof(ISelect<>).MakeGenericType(asSelectEntityType3).GetMethod("Where", new[] {
											typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(asSelectEntityType3, typeof(bool)))
									}));
									var parm123Tb = _common.GetTableByEntity(asSelectParentExp.Type);
									var parm123Ref = parm123Tb.GetTableRef(asSelectParentExp1.Member.Name, true);
									var fsqlWhereParam = fsqltables.First().Parameter; //Expression.Parameter(asSelectEntityType);
									Expression fsqlWhereExp = null;
									if (parm123Ref.RefType == TableRefType.ManyToMany) {
										//g.mysql.Select<Tag>().Where(a => g.mysql.Select<Song_tag>().Where(b => b.Tag_id == a.Id && b.Song_id == 1).Any());
										var manyTb = _common.GetTableByEntity(parm123Ref.RefMiddleEntityType);
										var manySubSelectWhere = _dicExpressionLambdaToSqlAsSelectWhereMethodInfo.GetOrAdd(parm123Ref.RefMiddleEntityType, refMiddleEntityType3 =>
											typeof(ISelect<>).MakeGenericType(refMiddleEntityType3).GetMethod("Where", new[] {
											typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(refMiddleEntityType3, typeof(bool)))
										}));
										var manySubSelectWhereSql = _dicExpressionLambdaToSqlAsSelectWhereSqlMethodInfo.GetOrAdd(parm123Ref.RefMiddleEntityType, refMiddleEntityType3 =>
											typeof(ISelect0<,>).MakeGenericType(typeof(ISelect<>).MakeGenericType(refMiddleEntityType3), refMiddleEntityType3).GetMethod("Where", new[] { typeof(string), typeof(object) }));
										var manySubSelectAny = _dicExpressionLambdaToSqlAsSelectAnyMethodInfo.GetOrAdd(parm123Ref.RefMiddleEntityType, refMiddleEntityType3 =>
											typeof(ISelect0<,>).MakeGenericType(typeof(ISelect<>).MakeGenericType(refMiddleEntityType3), refMiddleEntityType3).GetMethod("Any", new Type[0]));
										var manySubSelectAsSelectExp = _dicFreeSqlGlobalExtensionsAsSelectExpression.GetOrAdd(parm123Ref.RefMiddleEntityType, refMiddleEntityType3 =>
											Expression.Call(
												typeof(FreeSqlGlobalExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(mfil => mfil.Name == "AsSelect" && mfil.GetParameters().Length == 1).FirstOrDefault()?.MakeGenericMethod(refMiddleEntityType3),
												Expression.Constant(Activator.CreateInstance(typeof(List<>).MakeGenericType(refMiddleEntityType3)))
											));
										var manyMainParam = _tables[0].Parameter;
										var manySubSelectWhereParam = Expression.Parameter(parm123Ref.RefMiddleEntityType, $"M{fsqlWhereParam.Name}_M{asSelectParentExp.ToString().Replace(".", "__")}");//, $"{fsqlWhereParam.Name}__");
										Expression manySubSelectWhereExp = null;
										for (var mn = 0; mn < parm123Ref.Columns.Count; mn++) {
											var col1 = parm123Ref.MiddleColumns[mn];
											var col2 = parm123Ref.Columns[mn];
											var pexp1 = Expression.Property(manySubSelectWhereParam, col1.CsName);
											var pexp2 = Expression.Property(asSelectParentExp, col2.CsName);
											if (col1.CsType != col2.CsType) {
												if (col1.CsType.IsNullableType()) pexp1 = Expression.Property(pexp1, _dicNullableValueProperty.GetOrAdd(col1.CsType, ct1 => ct1.GetProperty("Value")));
												if (col2.CsType.IsNullableType()) pexp2 = Expression.Property(pexp2, _dicNullableValueProperty.GetOrAdd(col2.CsType, ct2 => ct2.GetProperty("Value")));
											}
											var tmpExp = Expression.Equal(pexp1, pexp2);
											if (mn == 0) manySubSelectWhereExp = tmpExp;
											else manySubSelectWhereExp = Expression.AndAlso(manySubSelectWhereExp, tmpExp);
										}
										var manySubSelectExpBoy = Expression.Call(
											manySubSelectAsSelectExp,
											manySubSelectWhere,
											Expression.Lambda(
												manySubSelectWhereExp,
												manySubSelectWhereParam
											)
										);
										Expression fsqlManyWhereExp = null;
										for (var mn = 0; mn < parm123Ref.RefColumns.Count; mn++) {
											var col1 = parm123Ref.RefColumns[mn];
											var col2 = parm123Ref.MiddleColumns[mn + parm123Ref.Columns.Count + mn];
											var pexp1 = Expression.Property(fsqlWhereParam, col1.CsName);
											var pexp2 = Expression.Property(manySubSelectWhereParam, col2.CsName);
											if (col1.CsType != col2.CsType) {
												if (col1.CsType.IsNullableType()) pexp1 = Expression.Property(pexp1, _dicNullableValueProperty.GetOrAdd(col1.CsType, ct1 => ct1.GetProperty("Value")));
												if (col2.CsType.IsNullableType()) pexp2 = Expression.Property(pexp2, _dicNullableValueProperty.GetOrAdd(col2.CsType, ct2 => ct2.GetProperty("Value")));
											}
											var tmpExp = Expression.Equal(pexp1, pexp2);
											if (mn == 0) fsqlManyWhereExp = tmpExp;
											else fsqlManyWhereExp = Expression.AndAlso(fsqlManyWhereExp, tmpExp);
										}
										fsqltables.Add(new SelectTableInfo { Alias = manySubSelectWhereParam.Name, Parameter = manySubSelectWhereParam, Table = manyTb, Type = SelectTableInfoType.Parent });
										fsqlWhere.Invoke(fsql, new object[] { Expression.Lambda(fsqlManyWhereExp, fsqlWhereParam) });
										var sql2 = fsqlType.GetMethod("ToSql", new Type[] { typeof(string) })?.Invoke(fsql, new object[] { "1" })?.ToString();
										if (string.IsNullOrEmpty(sql2) == false)
											manySubSelectExpBoy = Expression.Call(manySubSelectExpBoy, manySubSelectWhereSql, Expression.Constant($"exists({sql2.Replace("\r\n", "\r\n\t")})"), Expression.Constant(null));
										manySubSelectExpBoy = Expression.Call(manySubSelectExpBoy, manySubSelectAny);
										asSelectBefores.Clear();

										return ExpressionLambdaToSql(manySubSelectExpBoy, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
									}
									for (var mn = 0; mn < parm123Ref.Columns.Count; mn++) {
										var col1 = parm123Ref.RefColumns[mn];
										var col2 = parm123Ref.Columns[mn];
										var pexp1 = Expression.Property(fsqlWhereParam, col1.CsName);
										var pexp2 = Expression.Property(asSelectParentExp, col2.CsName);
										if (col1.CsType != col2.CsType) {
											if (col1.CsType.IsNullableType()) pexp1 = Expression.Property(pexp1, _dicNullableValueProperty.GetOrAdd(col1.CsType, ct1 => ct1.GetProperty("Value")));
											if (col2.CsType.IsNullableType()) pexp2 = Expression.Property(pexp2, _dicNullableValueProperty.GetOrAdd(col2.CsType, ct2 => ct2.GetProperty("Value")));
										}
										var tmpExp = Expression.Equal(pexp1, pexp2);
										if (mn == 0) fsqlWhereExp = tmpExp;
										else fsqlWhereExp = Expression.AndAlso(fsqlWhereExp, tmpExp);
									}
									fsqlWhere.Invoke(fsql, new object[] { Expression.Lambda(fsqlWhereExp, fsqlWhereParam) });
								}
								asSelectBefores.Clear();
								var sql = fsqlType.GetMethod("ToSql", new Type[] { typeof(string) })?.Invoke(fsql, new object[] { "1" })?.ToString();
								if (string.IsNullOrEmpty(sql) == false)
									return $"exists({sql.Replace("\r\n", "\r\n\t")})";
							}
							asSelectBefores.Clear();
						}
					}
					//var eleType = callType.GetElementType() ?? callType.GenericTypeArguments.FirstOrDefault();
					//if (eleType != null && typeof(IEnumerable<>).MakeGenericType(eleType).IsAssignableFrom(callType)) { //集合导航属性子查询
					//	if (exp3.Method.Name == "Any") { //exists
							
					//	}
					//}
					var other3Exp = ExpressionLambdaToSqlOther(exp3, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
					if (string.IsNullOrEmpty(other3Exp) == false) return other3Exp;
					throw new Exception($"未实现函数表达式 {exp3} 解析");
				case ExpressionType.Parameter:
				case ExpressionType.MemberAccess:
					var exp4 = exp as MemberExpression;
					if (exp4 != null) {
						if (exp4.Expression != null && exp4.Expression.Type.IsArray == false && exp4.Expression.Type.IsNullableType()) return ExpressionLambdaToSql(exp4.Expression, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
						var extRet = "";
						var memberType = exp4.Expression?.Type ?? exp4.Type;
						switch (memberType.FullName) {
							case "System.String": extRet = ExpressionLambdaToSqlMemberAccessString(exp4, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style); break;
							case "System.DateTime": extRet = ExpressionLambdaToSqlMemberAccessDateTime(exp4, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style); break;
							case "System.TimeSpan": extRet = ExpressionLambdaToSqlMemberAccessTimeSpan(exp4, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style); break;
						}
						if (string.IsNullOrEmpty(extRet) == false) return extRet;
						var other4Exp = ExpressionLambdaToSqlOther(exp4, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
						if (string.IsNullOrEmpty(other4Exp) == false) return other4Exp;
					}
					var expStack = new Stack<Expression>();
					expStack.Push(exp);
					MethodCallExpression callExp = null;
					var exp2 = exp4?.Expression;
					while (true) {
						switch(exp2?.NodeType) {
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
							case ExpressionType.TypeAs:
							case ExpressionType.Convert:
								var oper2 = (exp2 as UnaryExpression).Operand;
								if (oper2.NodeType == ExpressionType.Parameter) {
									var oper2Parm = oper2 as ParameterExpression;
									expStack.Push(Expression.Parameter(exp2.Type, oper2Parm.Name));
								} else
									expStack.Push(oper2);
								break;
						}
						break;
					}
					if (expStack.First().NodeType != ExpressionType.Parameter) return _common.FormatSql("{0}", Expression.Lambda(exp).Compile().DynamicInvoke());
					if (callExp != null) return ExpressionLambdaToSql(callExp, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
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
					Func<TableInfo, string, bool, ParameterExpression, MemberExpression, SelectTableInfo> getOrAddTable = (tbtmp, alias, isa, parmExp, mp) => {
						var finds = new SelectTableInfo[0];
						if (style == ExpressionStyle.SelectColumns) {
							finds = _tables.Where(a => a.Table.Type == tbtmp.Type).ToArray();
							if (finds.Any()) finds = new[] { finds.First() };
						}
						if (finds.Length != 1 && isa && parmExp != null)
							finds = _tables.Where(a => a.Parameter == parmExp).ToArray();
						if (finds.Length != 1) {
							var navdot = string.IsNullOrEmpty(alias) ? new SelectTableInfo[0] : _tables.Where(a2 => a2.Parameter != null && alias.StartsWith($"{a2.Alias}__")).ToArray();
							if (navdot.Length > 0) {
								var isthis = navdot[0] == _tables[0];
								finds = _tables.Where(a2 => (isa && a2.Parameter != null || !isa && a2.Parameter == null) &&
									a2.Table.Type == tbtmp.Type && a2.Alias == alias && a2.Alias.StartsWith($"{navdot[0].Alias}__") &&
									(isthis && a2.Type != SelectTableInfoType.Parent || !isthis && a2.Type == SelectTableInfoType.Parent)).ToArray();
								if (finds.Length == 0)
									finds = _tables.Where(a2 => 
										 a2.Table.Type == tbtmp.Type && a2.Alias == alias && a2.Alias.StartsWith($"{navdot[0].Alias}__") &&
										 (isthis && a2.Type != SelectTableInfoType.Parent || !isthis && a2.Type == SelectTableInfoType.Parent)).ToArray();
							} else {
								finds = _tables.Where(a2 => (isa && a2.Parameter != null || isa && a2.Parameter == null) &&
									a2.Table.Type == tbtmp.Type && a2.Alias == alias).ToArray();
								if (finds.Length != 1) {
									finds = _tables.Where(a2 => (isa && a2.Parameter != null || isa && a2.Parameter == null) &&
										a2.Table.Type == tbtmp.Type).ToArray();
									if (finds.Length != 1) {
										finds = _tables.Where(a2 => (isa && a2.Parameter != null || isa && a2.Parameter == null) &&
											a2.Table.Type == tbtmp.Type).ToArray();
										if (finds.Length != 1)
											finds = _tables.Where(a2 => a2.Table.Type == tbtmp.Type).ToArray();
									}
								}
							}
							//finds = _tables.Where((a2, c2) => (isa || a2.Parameter == null) && a2.Table.CsName == tbtmp.CsName && (isthis && a2.Type != SelectTableInfoType.Parent || !isthis)).ToArray(); //外部表，内部表一起查
							//if (finds.Length > 1) {
							//	finds = _tables.Where((a2, c2) => (isa || a2.Parameter == null) && a2.Table.CsName == tbtmp.CsName && a2.Type == SelectTableInfoType.Parent && a2.Alias == alias).ToArray(); //查询外部表
							//	if (finds.Any() == false) {
							//		finds = _tables.Where((a2, c2) => (isa || a2.Parameter == null) && a2.Table.CsName == tbtmp.CsName && a2.Type != SelectTableInfoType.Parent).ToArray(); //查询内部表
							//		if (finds.Length > 1)
							//			finds = _tables.Where((a2, c2) => (isa || a2.Parameter == null) && a2.Table.CsName == tbtmp.CsName && a2.Type != SelectTableInfoType.Parent && a2.Alias == alias).ToArray();
							//	}
							//}
						}
						var find = finds.Length == 1 ? finds.First() : null;
						if (find != null && isa && parmExp != null && find.Parameter != parmExp)
							find.Parameter = parmExp;
						if (find == null) {
							_tables.Add(find = new SelectTableInfo { Table = tbtmp, Alias = alias, On = null, Type = mp == null ? tbtype : SelectTableInfoType.LeftJoin, Parameter = isa ? parmExp : null });
							if (mp?.Expression != null) { //导航条件，OneToOne、ManyToOne
								var firstTb = _tables.First().Table;
								var parentTb = _common.GetTableByEntity(mp.Expression.Type);
								var parentTbRef = parentTb.GetTableRef(mp.Member.Name, style == ExpressionStyle.AsSelect);
								if (parentTbRef != null) {
									Expression navCondExp = null;
									for (var mn = 0; mn < parentTbRef.Columns.Count; mn++) {
										var col1 = parentTbRef.RefColumns[mn];
										var col2 = parentTbRef.Columns[mn];
										var pexp1 = Expression.Property(mp, col1.CsName);
										var pexp2 = Expression.Property(mp.Expression, col2.CsName);
										if (col1.CsType != col2.CsType) {
											if (col1.CsType.IsNullableType()) pexp1 = Expression.Property(pexp1, _dicNullableValueProperty.GetOrAdd(col1.CsType, ct1 => ct1.GetProperty("Value")));
											if (col2.CsType.IsNullableType()) pexp2 = Expression.Property(pexp2, _dicNullableValueProperty.GetOrAdd(col2.CsType, ct2 => ct2.GetProperty("Value")));
										}
										var tmpExp = Expression.Equal(pexp1, pexp2);
										if (mn == 0) navCondExp = tmpExp;
										else navCondExp = Expression.AndAlso(navCondExp, tmpExp);
									}
									if (find.Type == SelectTableInfoType.InnerJoin ||
										find.Type == SelectTableInfoType.LeftJoin ||
										find.Type == SelectTableInfoType.RightJoin)
										find.On = ExpressionLambdaToSql(navCondExp, _tables, null, null, find.Type, isQuoteName, isDisableDiyParse, style);
									else
										find.NavigateCondition = ExpressionLambdaToSql(navCondExp, _tables, null, null, find.Type, isQuoteName, isDisableDiyParse, style);
								}
							}
						}
						return find;
					};

					TableInfo tb2 = null;
					ParameterExpression parmExp2 = null;
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
									if (exp2.NodeType == ExpressionType.Parameter) {
										parmExp2 = (exp2 as ParameterExpression);
										alias2 = parmExp2.Name;
									} else alias2 = $"{alias2}__{mp2.Member.Name}";
									find2 = getOrAddTable(tb2tmp, alias2, exp2.NodeType == ExpressionType.Parameter, parmExp2, mp2);
									alias2 = find2.Alias;
									tb2 = tb2tmp;
								}
								if (exp2.NodeType == ExpressionType.Parameter && expStack.Any() == false) { //附加选择的参数所有列
									if (_selectColumnMap != null) {
										foreach (var tb2c in tb2.Columns.Values)
											_selectColumnMap.Add(new SelectColumnInfo { Table = find2, Column = tb2c });
										if (tb2.Columns.Any()) return "";
									}
								}
								if (mp2 == null || expStack.Any()) continue;
								if (tb2.ColumnsByCs.ContainsKey(mp2.Member.Name) == false) { //如果选的是对象，附加所有列
									if (_selectColumnMap != null) {
										var tb3 = _common.GetTableByEntity(mp2.Type);
										if (tb3 != null) {
											var find3 = getOrAddTable(tb2tmp, alias2 /*$"{alias2}__{mp2.Member.Name}"*/, exp2.NodeType == ExpressionType.Parameter, parmExp2, mp2);

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
				var other99Exp = ExpressionLambdaToSqlOther(exp, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
				if (string.IsNullOrEmpty(other99Exp) == false) return other99Exp;
				return "";
			}
			if (expBinary.NodeType == ExpressionType.Coalesce) {
				return _common.IsNull(
					ExpressionLambdaToSql(expBinary.Left, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style),
					ExpressionLambdaToSql(expBinary.Right, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style));
			}
			if (dicExpressionOperator.TryGetValue(expBinary.NodeType, out var tryoper) == false) return "";
			var left = ExpressionLambdaToSql(expBinary.Left, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
			var right = ExpressionLambdaToSql(expBinary.Right, _tables, _selectColumnMap, getSelectGroupingMapString, tbtype, isQuoteName, isDisableDiyParse, style);
			if (left == "NULL") {
				var tmp = right;
				right = left;
				left = tmp;
			}
			if (right == "NULL") tryoper = tryoper == "=" ? " IS " : " IS NOT ";
			if (tryoper == "+" && (expBinary.Left.Type.FullName == "System.String" || expBinary.Right.Type.FullName == "System.String")) return _common.StringConcat(left, right, expBinary.Left.Type, expBinary.Right.Type);
			if (tryoper == "%") return _common.Mod(left, right, expBinary.Left.Type, expBinary.Right.Type);
			if (_common._orm.Ado.DataType == DataType.MySql) {
				//处理c#变态enum convert， a.EnumType1 == Xxx.Xxx，被转成了 Convert(a.EnumType1, Int32) == 1
				if (expBinary.Left.NodeType == ExpressionType.Convert && expBinary.Right.NodeType == ExpressionType.Constant) {
					if (long.TryParse(right, out var tryenumLong)) {
						var enumType = (expBinary.Left as UnaryExpression)?.Operand.Type;
						if (enumType?.IsEnum == true)
							right = _common.FormatSql("{0}", Enum.Parse(enumType, right));
					}
				} else if (expBinary.Left.NodeType == ExpressionType.Constant && expBinary.Right.NodeType == ExpressionType.Convert) {
					if (long.TryParse(left, out var tryenumLong)) {
						var enumType = (expBinary.Right as UnaryExpression)?.Operand.Type;
						if (enumType?.IsEnum == true)
							left = _common.FormatSql("{0}", Enum.Parse(enumType, left));
					}
				}
			}
			return $"{left} {tryoper} {right}";
		}

		internal abstract string ExpressionLambdaToSqlMemberAccessString(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName, bool isDisableDiyParse, ExpressionStyle style);
		internal abstract string ExpressionLambdaToSqlMemberAccessDateTime(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName, bool isDisableDiyParse, ExpressionStyle style);
		internal abstract string ExpressionLambdaToSqlMemberAccessTimeSpan(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName, bool isDisableDiyParse, ExpressionStyle style);
		internal abstract string ExpressionLambdaToSqlCallString(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName, bool isDisableDiyParse, ExpressionStyle style);
		internal abstract string ExpressionLambdaToSqlCallMath(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName, bool isDisableDiyParse, ExpressionStyle style);
		internal abstract string ExpressionLambdaToSqlCallDateTime(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName, bool isDisableDiyParse, ExpressionStyle style);
		internal abstract string ExpressionLambdaToSqlCallTimeSpan(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName, bool isDisableDiyParse, ExpressionStyle style);
		internal abstract string ExpressionLambdaToSqlCallConvert(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName, bool isDisableDiyParse, ExpressionStyle style);
		internal abstract string ExpressionLambdaToSqlOther(Expression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Func<Expression[], string> getSelectGroupingMapString, SelectTableInfoType tbtype, bool isQuoteName, bool isDisableDiyParse, ExpressionStyle style);

		internal enum ExpressionStyle {
			Where, AsSelect, SelectColumns
		}
	}
}
