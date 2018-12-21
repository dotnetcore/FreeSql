using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Internal {
	internal abstract class CommonExpression {

		internal CommonUtils _common;
		internal CommonExpression(CommonUtils common) {
			_common = common;
		}

		internal bool ReadAnonymousField(List<SelectTableInfo> _tables, StringBuilder field, ReadAnonymousTypeInfo parent, ref int index, Expression exp) {
			switch (exp.NodeType) {
				case ExpressionType.Quote: return ReadAnonymousField(_tables, field, parent, ref index, (exp as UnaryExpression)?.Operand);
				case ExpressionType.Lambda: return ReadAnonymousField(_tables, field, parent, ref index, (exp as LambdaExpression)?.Body);
				case ExpressionType.Convert: return ReadAnonymousField(_tables, field, parent, ref index, (exp as UnaryExpression)?.Operand);
				case ExpressionType.Constant:
					var constExp = exp as ConstantExpression;
					field.Append(", ").Append(constExp?.Value).Append(" as").Append(++index);
					return false;
				case ExpressionType.MemberAccess:
					var map = new List<SelectColumnInfo>();
					ExpressionSelectColumn_MemberAccess(_tables, map, SelectTableInfoType.From, exp, true);
					if (map.Count > 1) {
						parent.Consturctor = map.First().Table.Table.Type.GetConstructor(new Type[0]);
						parent.ConsturctorType = ReadAnonymousTypeInfoConsturctorType.Properties;
					}
					for (var idx = 0; idx < map.Count; idx++) {
						field.Append(", ").Append(map[idx].Table.Alias).Append(".").Append(_common.QuoteSqlName(map[idx].Column.Attribute.Name)).Append(" as").Append(++index);
						if (map.Count > 1) parent.Childs.Add(new ReadAnonymousTypeInfo { CsName = map[idx].Column.CsName });
					}
					return false;
				case ExpressionType.New:
					var newExp = exp as NewExpression;
					parent.Consturctor = newExp.Type.GetConstructors()[0];
					parent.ConsturctorType = ReadAnonymousTypeInfoConsturctorType.Arguments;
					for (var a = 0; a < newExp.Members.Count; a++) {
						var child = new ReadAnonymousTypeInfo { CsName = newExp.Members[a].Name };
						parent.Childs.Add(child);
						ReadAnonymousField(_tables, field, child, ref index, newExp.Arguments[a]);
					}
					return true;
			}
			return false;
		}
		internal object ReadAnonymous(ReadAnonymousTypeInfo parent, object[] dr, ref int index) {
			if (parent.Childs.Any() == false) return dr[++index];
			switch (parent.ConsturctorType) {
				case ReadAnonymousTypeInfoConsturctorType.Arguments:
					var args = new object[parent.Childs.Count];
					for (var a = 0; a < parent.Childs.Count; a++) {
						args[a] = ReadAnonymous(parent.Childs[a], dr, ref index);
					}
					return parent.Consturctor.Invoke(args);
				case ReadAnonymousTypeInfoConsturctorType.Properties:
					var ret = parent.Consturctor.Invoke(null);
					for (var b = 0; b < parent.Childs.Count; b++) {
						Utils.FillPropertyValue(ret, parent.Childs[b].CsName, ReadAnonymous(parent.Childs[b], dr, ref index));
					}
					return ret;
			}
			return null;
		}

		internal string ExpressionConstant(ConstantExpression exp) => _common.FormatSql("{0}", exp?.Value);

		internal string ExpressionSelectColumn_MemberAccess(List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, Expression exp, bool isQuoteName) {
			return ExpressionLambdaToSql(exp, _tables, _selectColumnMap, tbtype, isQuoteName);
		}

		internal string[] ExpressionSelectColumns_MemberAccess_New_NewArrayInit(List<SelectTableInfo> _tables, Expression exp, bool isQuoteName) {
			switch (exp?.NodeType) {
				case ExpressionType.Quote: return ExpressionSelectColumns_MemberAccess_New_NewArrayInit(_tables, (exp as UnaryExpression)?.Operand, isQuoteName);
				case ExpressionType.Lambda: return ExpressionSelectColumns_MemberAccess_New_NewArrayInit(_tables, (exp as LambdaExpression)?.Body, isQuoteName);
				case ExpressionType.Convert: return ExpressionSelectColumns_MemberAccess_New_NewArrayInit(_tables, (exp as UnaryExpression)?.Operand, isQuoteName);
				case ExpressionType.Constant: return new[] { ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, isQuoteName) };
				case ExpressionType.MemberAccess: return new[] { ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, isQuoteName) };
				case ExpressionType.New:
					var newExp = exp as NewExpression;
					if (newExp == null) break;
					var newExpMembers = new string[newExp.Members.Count];
					for (var a = 0; a < newExpMembers.Length; a++) newExpMembers[a] = ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, newExp.Arguments[a], isQuoteName);
					return newExpMembers;
				case ExpressionType.NewArrayInit:
					var newArr = exp as NewArrayExpression;
					if (newArr == null) break;
					var newArrMembers = new List<string>();
					foreach (var newArrExp in newArr.Expressions) newArrMembers.AddRange(ExpressionSelectColumns_MemberAccess_New_NewArrayInit(_tables, newArrExp, isQuoteName));
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
		internal string ExpressionWhereLambdaNoneForeignObject(List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, Expression exp) {
			var sql = ExpressionLambdaToSql(exp, _tables, _selectColumnMap, SelectTableInfoType.From, true);
			switch(sql) {
				case "1":
				case "'t'": return "1=1";
				case "0":
				case "'f'": return "1=2";
				default:return sql;
			}
		}

		internal string ExpressionWhereLambda(List<SelectTableInfo> _tables, Expression exp) {
			var sql = ExpressionLambdaToSql(exp, _tables, null, SelectTableInfoType.From, true);
			switch (sql) {
				case "1":
				case "'t'": return "1=1";
				case "0":
				case "'f'": return "1=2";
				default: return sql;
			}
		}
		internal void ExpressionJoinLambda(List<SelectTableInfo> _tables, SelectTableInfoType tbtype, Expression exp) {
			var tbidx = _tables.Count;
			var filter = ExpressionLambdaToSql(exp, _tables, null, tbtype, true);
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
				var find = _tables.Where((a, c) => c > 0 && a.Type == tbtype && string.IsNullOrEmpty(a.On)).LastOrDefault();
				if (find != null) find.On = filter;
			}
		}

		internal string ExpressionLambdaToSql(Expression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName) {
			switch (exp.NodeType) {
				case ExpressionType.Quote: return ExpressionLambdaToSql((exp as UnaryExpression)?.Operand, _tables, _selectColumnMap, tbtype, isQuoteName);
				case ExpressionType.Lambda: return ExpressionLambdaToSql((exp as LambdaExpression)?.Body, _tables, _selectColumnMap, tbtype, isQuoteName);
				case ExpressionType.Convert: return ExpressionLambdaToSql((exp as UnaryExpression)?.Operand, _tables, _selectColumnMap, tbtype, isQuoteName);
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked: return "-" + ExpressionLambdaToSql((exp as UnaryExpression)?.Operand, _tables, _selectColumnMap, tbtype, isQuoteName);
				case ExpressionType.Constant: return _common.FormatSql("{0}", (exp as ConstantExpression)?.Value);
				case ExpressionType.Call:
					var exp3 = exp as MethodCallExpression;
					switch (exp3.Object?.Type.FullName ?? exp3.Method.DeclaringType.FullName) {
						case "System.String": return ExpressionLambdaToSqlCallString(exp3, _tables, _selectColumnMap, tbtype, isQuoteName);
						case "System.Math": return ExpressionLambdaToSqlCallMath(exp3, _tables, _selectColumnMap, tbtype, isQuoteName);
						case "System.DateTime": return ExpressionLambdaToSqlCallDateTime(exp3, _tables, _selectColumnMap, tbtype, isQuoteName);
						case "System.TimeSpan": return ExpressionLambdaToSqlCallTimeSpan(exp3, _tables, _selectColumnMap, tbtype, isQuoteName);
					}
					throw new Exception($"MySqlExpression 未现实函数表达式 {exp3} 解析");
				case ExpressionType.MemberAccess:
					var exp4 = exp as MemberExpression;
					if (exp4.Expression != null && exp4.Expression.Type.FullName.StartsWith("System.Nullable`1[")) return ExpressionLambdaToSql(exp4.Expression, _tables, _selectColumnMap, tbtype, isQuoteName);
					var extRet = "";
					switch (exp4.Expression?.Type.FullName ?? exp4.Type.FullName) {
						case "System.String": extRet = ExpressionLambdaToSqlMemberAccessString(exp4, _tables, _selectColumnMap, tbtype, isQuoteName); break;
						case "System.DateTime": extRet = ExpressionLambdaToSqlMemberAccessDateTime(exp4, _tables, _selectColumnMap, tbtype, isQuoteName); break;
						case "System.TimeSpan": extRet = ExpressionLambdaToSqlMemberAccessTimeSpan(exp4, _tables, _selectColumnMap, tbtype, isQuoteName); break;
					}
					if (string.IsNullOrEmpty(extRet) == false) return extRet;

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
					if (callExp != null) return ExpressionLambdaToSql(callExp, _tables, _selectColumnMap, tbtype, isQuoteName);
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

					TableInfo tb2 = null;
					string alias2 = "", name2 = "";
					SelectTableInfo find2 = null;
					while (expStack.Count > 0) {
						exp2 = expStack.Pop();
						switch (exp2.NodeType) {
							case ExpressionType.Constant:
								throw new NotImplementedException("未现实 MemberAccess 下的 Constant");
							case ExpressionType.Parameter:
							case ExpressionType.MemberAccess:
								var tb2tmp = _common.GetTableByEntity(exp2.Type);
								var mp2 = exp2 as MemberExpression;
								if (tb2tmp != null) {
									if (exp2.NodeType == ExpressionType.Parameter) alias2 = (exp2 as ParameterExpression).Name;
									else alias2 = $"{alias2}__{mp2.Member.Name}";
									var find2s = _tables.Where((a2, c2) => a2.Table.CsName == tb2tmp.CsName).ToArray();
									if (find2s.Length > 1) find2s = _tables.Where((a2, c2) => a2.Table.CsName == tb2tmp.CsName && a2.Alias == alias2).ToArray();
									find2 = find2s.FirstOrDefault();
									if (find2 == null) _tables.Add(find2 = new SelectTableInfo { Table = tb2tmp, Alias = alias2, On = null, Type = tbtype });
									alias2 = find2.Alias;
									tb2 = tb2tmp;
								}
								if (mp2 == null || expStack.Any()) continue;
								if (tb2.ColumnsByCs.ContainsKey(mp2.Member.Name) == false) { //如果选的是对象，附加所有列
									if (_selectColumnMap != null) {
										var tb3 = _common.GetTableByEntity(mp2.Type);
										if (tb3 != null) {
											var alias3 = $"{alias2}__{mp2.Member.Name}";
											var find3s = _tables.Where((a3, c3) => a3.Table.CsName == tb3.CsName).ToArray();
											if (find3s.Length > 1) find3s = _tables.Where((a3, c3) => a3.Table.CsName == tb3.CsName && a3.Alias == alias3).ToArray();
											var find3 = find3s.FirstOrDefault();
											if (find3 == null) _tables.Add(find3 = new SelectTableInfo { Table = tb3, Alias = alias3, On = null, Type = tbtype });
											alias3 = find3.Alias;

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
			if (expBinary == null) return "";
			if (expBinary.NodeType == ExpressionType.Coalesce) {
				return _common.IsNull(
					ExpressionLambdaToSql(expBinary.Left, _tables, _selectColumnMap, tbtype, isQuoteName),
					ExpressionLambdaToSql(expBinary.Right, _tables, _selectColumnMap, tbtype, isQuoteName));
			}
			if (dicExpressionOperator.TryGetValue(expBinary.NodeType, out var tryoper) == false) return "";
			var left = ExpressionLambdaToSql(expBinary.Left, _tables, _selectColumnMap, tbtype, isQuoteName);
			var right = ExpressionLambdaToSql(expBinary.Right, _tables, _selectColumnMap, tbtype, isQuoteName);
			if (left == "NULL") {
				var tmp = right;
				right = left;
				left = tmp;
			}
			if (right == "NULL") tryoper = tryoper == "=" ? " IS " : " IS NOT ";
			if (tryoper == "+" && (expBinary.Left.Type.FullName == "System.String" || expBinary.Right.Type.FullName == "System.String")) return _common.StringConcat(left, right, expBinary.Left.Type, expBinary.Right.Type);
			return $"{left} {tryoper} {right}";
		}

		internal abstract string ExpressionLambdaToSqlMemberAccessString(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName);
		internal abstract string ExpressionLambdaToSqlMemberAccessDateTime(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName);
		internal abstract string ExpressionLambdaToSqlMemberAccessTimeSpan(MemberExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName);
		internal abstract string ExpressionLambdaToSqlCallString(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName);
		internal abstract string ExpressionLambdaToSqlCallMath(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName);
		internal abstract string ExpressionLambdaToSqlCallDateTime(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName);
		internal abstract string ExpressionLambdaToSqlCallTimeSpan(MethodCallExpression exp, List<SelectTableInfo> _tables, List<SelectColumnInfo> _selectColumnMap, SelectTableInfoType tbtype, bool isQuoteName);
	}
}
