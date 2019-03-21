using FreeSql.Internal.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FreeSql.Extensions {
	public static class EntityUtilFreeSqlExtensions {

		static MethodInfo MethodStringBuilderAppend = typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(object) });
		static MethodInfo MethodStringBuilderToString = typeof(StringBuilder).GetMethod("ToString", new Type[0]);
		static PropertyInfo MethodStringBuilderLength = typeof(StringBuilder).GetProperty("Length");
		static MethodInfo MethodStringConcat = typeof(string).GetMethod("Concat", new Type[] { typeof(object) });

		static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, string>>> _dicGetEntityKeyString = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, string>>>();
		/// <summary>
		/// 获取实体的主键值，以 "*|_,[,_|*" 分割
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="_table"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static string GetEntityKeyString<TEntity>(this IFreeSql orm, TEntity item, string splitString = "*|_,[,_|*") {
			var func = _dicGetEntityKeyString.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Func<object, string>>()).GetOrAdd(typeof(TEntity), t => {
				var _table = orm.CodeFirst.GetTableByEntity(t);
				var pks = _table.Primarys;
				var returnTarget = Expression.Label(typeof(string));
				var parm1 = Expression.Parameter(typeof(object));
				var var1Parm = Expression.Variable(t);
				var var2Sb = Expression.Variable(typeof(StringBuilder));
				var var3IsNull = Expression.Variable(typeof(bool));
				var exps = new List<Expression>(new Expression[] {
					Expression.Assign(var1Parm, Expression.TypeAs(parm1, t)),
					Expression.Assign(var2Sb, Expression.New(typeof(StringBuilder))),
					Expression.Assign(var3IsNull, Expression.Constant(false))
				});
				for (var a = 0; a < pks.Length; a++) {
					exps.Add(
						Expression.IfThen(
							Expression.Equal(var3IsNull, Expression.Constant(false)),
							Expression.IfThenElse(
								Expression.Equal(Expression.MakeMemberAccess(var1Parm, _table.Properties[pks[a].CsName]), Expression.Default(pks[a].CsType)),
								Expression.Assign(var3IsNull, Expression.Constant(true)),
								Expression.Block(
									new Expression[]{
										a > 0 ? Expression.Call(var2Sb, MethodStringBuilderAppend, Expression.Constant(splitString)) : null,
										Expression.Call(var2Sb, MethodStringBuilderAppend,
											Expression.Convert(Expression.MakeMemberAccess(var1Parm, _table.Properties[pks[a].CsName]), typeof(object))
										)
									}.Where(c => c != null).ToArray()
								)
							)
						)
					);
				}
				exps.Add(
					Expression.IfThen(
						Expression.Equal(var3IsNull, Expression.Constant(false)),
						Expression.Return(returnTarget, Expression.Call(var2Sb, MethodStringBuilderToString))
					)
				);
				exps.Add(Expression.Label(returnTarget, Expression.Default(typeof(string))));
				return Expression.Lambda<Func<object, string>>(Expression.Block(new[] { var1Parm, var2Sb, var3IsNull }, exps), new[] { parm1 }).Compile();
			});
			return func(item);
		}
		static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, string>>> _dicGetEntityString = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, string>>>();
		/// <summary>
		/// 获取实体的所有数据，以 (1, 2, xxx) 的形式
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="_table"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static string GetEntityString<TEntity>(this IFreeSql orm, TEntity item) {
			var func = _dicGetEntityString.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Func<object, string>>()).GetOrAdd(typeof(TEntity), t => {
				var _table = orm.CodeFirst.GetTableByEntity(t);
				var cols = _table.Columns;
				var returnTarget = Expression.Label(typeof(string));
				var parm1 = Expression.Parameter(typeof(object));
				var var1Parm = Expression.Variable(t);
				var var2Sb = Expression.Variable(typeof(StringBuilder));
				var var3IsNull = Expression.Variable(typeof(bool));
				var exps = new List<Expression>(new Expression[] {
					Expression.Assign(var1Parm, Expression.TypeAs(parm1, t)),
					Expression.Assign(var2Sb, Expression.New(typeof(StringBuilder))),
					Expression.Assign(var3IsNull, Expression.Constant(false)),
					Expression.Call(var2Sb, MethodStringBuilderAppend, Expression.Constant("(" ))
				});
				var a = 0;
				foreach (var col in cols.Values) {
					exps.Add(
						Expression.IfThen(
							Expression.Equal(var3IsNull, Expression.Constant(false)),
							Expression.IfThenElse(
								Expression.Equal(Expression.MakeMemberAccess(var1Parm, _table.Properties[col.CsName]), Expression.Default(col.CsType)),
								Expression.Assign(var3IsNull, Expression.Constant(true)),
								Expression.Block(
									new Expression[]{
										a > 0 ? Expression.Call(var2Sb, MethodStringBuilderAppend, Expression.Constant(", " )) : null,
										Expression.Call(var2Sb, MethodStringBuilderAppend,
											Expression.Convert(Expression.MakeMemberAccess(var1Parm, _table.Properties[col.CsName]), typeof(object))
										)
									}.Where(c => c != null).ToArray()
								)
							)
						)
					);
					a++;
				}
				exps.AddRange(new Expression[] {
					Expression.Call(var2Sb, MethodStringBuilderAppend, Expression.Constant(")" )),
					Expression.IfThen(
						Expression.Equal(var3IsNull, Expression.Constant(false)),
						Expression.Return(returnTarget, Expression.Call(var2Sb, MethodStringBuilderToString))
					)
				});
				exps.Add(Expression.Label(returnTarget, Expression.Default(typeof(string))));
				return Expression.Lambda<Func<object, string>>(Expression.Block(new[] { var1Parm, var2Sb, var3IsNull }, exps), new[] { parm1 }).Compile();
			});
			return func(item);
		}

		/// <summary>
		/// 使用新实体的值，复盖旧实体的值
		/// </summary>
		static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object, object>>> _dicCopyNewValueToEntity = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object, object>>>();
		public static void CopyEntityValue<TEntity>(this IFreeSql orm, TEntity oldValue, TEntity newValue) {
			var func = _dicCopyNewValueToEntity.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Action<object, object>>()).GetOrAdd(typeof(TEntity), t => {
				var _table = orm.CodeFirst.GetTableByEntity(t);
				var parm1 = Expression.Parameter(typeof(object));
				var parm2 = Expression.Parameter(typeof(object));
				var var1Parm = Expression.Variable(t);
				var var2Parm = Expression.Variable(t);
				var exps = new List<Expression>(new Expression[] {
					Expression.Assign(var1Parm, Expression.TypeAs(parm1, t)),
					Expression.Assign(var2Parm, Expression.TypeAs(parm2, t))
				});
				foreach (var prop in _table.Properties.Values) {
					if (_table.ColumnsByCs.ContainsKey(prop.Name)) {
						exps.Add(
							Expression.Assign(
								Expression.MakeMemberAccess(var1Parm, prop),
								Expression.MakeMemberAccess(var2Parm, prop)
							)
						);
					} else {
						exps.Add(
							Expression.Assign(
								Expression.MakeMemberAccess(var1Parm, prop),
								Expression.Default(prop.PropertyType)
							)
						);
					}
				}
				return Expression.Lambda<Action<object, object>>(Expression.Block(new[] { var1Parm, var2Parm }, exps), new[] { parm1, parm2 }).Compile();
			});
			func(oldValue, newValue);
		}

		static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object, long>>> _dicSetEntityIdentityValue = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object, long>>>();
		/// <summary>
		/// 设置实体的自增字段值（若存在）
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="orm"></param>
		/// <param name="item"></param>
		/// <param name="idtval"></param>
		public static void SetEntityIdentityValue<TEntity>(this IFreeSql orm, TEntity item, long idtval) {
			var func = _dicSetEntityIdentityValue.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Action<object, long>>()).GetOrAdd(typeof(TEntity), t => {
				var _table = orm.CodeFirst.GetTableByEntity(t);
				var identitys = _table.Primarys.Where(a => a.Attribute.IsIdentity);
				var parm1 = Expression.Parameter(typeof(object));
				var parm2 = Expression.Parameter(typeof(long));
				var var1Parm = Expression.Variable(t);
				var exps = new List<Expression>(new Expression[] {
					Expression.Assign(var1Parm, Expression.TypeAs(parm1, t))
				});
				foreach (var pk in identitys) {
					exps.Add(
						Expression.Assign(
							Expression.MakeMemberAccess(var1Parm, _table.Properties[pk.CsName]),
							Expression.Convert(FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(pk.CsType, Expression.Convert(parm2, typeof(object))), pk.CsType)
						)
					);
				}
				return Expression.Lambda<Action<object, long>>(Expression.Block(new[] { var1Parm }, exps), new[] { parm1, parm2 }).Compile();
			});
			func(item, idtval);
		}
		static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object>>> _dicClearEntityPrimaryValueWithIdentityAndGuid = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object>>>();
		/// <summary>
		/// 清除实体的主键值，将自增、Guid类型的主键值清除
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="orm"></param>
		/// <param name="item"></param>
		public static void ClearEntityPrimaryValueWithIdentityAndGuid<TEntity>(this IFreeSql orm, TEntity item) {
			var func = _dicClearEntityPrimaryValueWithIdentityAndGuid.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Action<object>>()).GetOrAdd(typeof(TEntity), t => {
				var _table = orm.CodeFirst.GetTableByEntity(t);
				var identitys = _table.Primarys.Where(a => a.Attribute.IsIdentity);
				var parm1 = Expression.Parameter(typeof(object));
				var var1Parm = Expression.Variable(t);
				var exps = new List<Expression>(new Expression[] {
					Expression.Assign(var1Parm, Expression.TypeAs(parm1, t))
				});
				foreach (var pk in _table.Primarys) {
					if (pk.CsType == typeof(Guid) || pk.CsType == typeof(Guid?) ||
						pk.Attribute.IsIdentity) {
						exps.Add(
							Expression.Assign(
								Expression.MakeMemberAccess(var1Parm, _table.Properties[pk.CsName]),
								Expression.Default(pk.CsType)
							)
						);
					}
				}
				return Expression.Lambda<Action<object>>(Expression.Block(new[] { var1Parm }, exps), new[] { parm1 }).Compile();
			});
			func(item);
		}

		static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, object, bool, string[]>>> _dicCompareEntityValueReturnColumns = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Func<object, object, bool, string[]>>>();
		/// <summary>
		/// 对比两个实体值，返回相同/或不相同的列名
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="orm"></param>
		/// <param name="up"></param>
		/// <param name="oldval"></param>
		/// <returns></returns>
		public static string[] CompareEntityValueReturnColumns<TEntity>(this IFreeSql orm, TEntity up, TEntity oldval, bool isEqual) {
			var func = _dicCompareEntityValueReturnColumns.GetOrAdd(orm.Ado.DataType, dt => new  ConcurrentDictionary<Type, Func<object, object, bool, string[]>>()).GetOrAdd(typeof(TEntity), t => {
				var _table = orm.CodeFirst.GetTableByEntity(t);
				var returnTarget = Expression.Label(typeof(string[]));
				var parm1 = Expression.Parameter(typeof(object));
				var parm2 = Expression.Parameter(typeof(object));
				var parm3 = Expression.Parameter(typeof(bool));
				var var1Ret = Expression.Variable(typeof(List<string>));
				var var1Parm = Expression.Variable(t);
				var var2Parm = Expression.Variable(t);
				var exps = new List<Expression>(new Expression[] {
					Expression.Assign(var1Parm, Expression.TypeAs(parm1, t)),
					Expression.Assign(var2Parm, Expression.TypeAs(parm2, t)),
					Expression.Assign(var1Ret, Expression.New(typeof(List<string>)))
				});
				var a = 0;
				foreach (var prop in _table.Properties.Values) {
					if (_table.ColumnsByCs.TryGetValue(prop.Name, out var trycol) == false) continue;
					exps.Add(
						Expression.IfThenElse(
							Expression.Equal(
								Expression.MakeMemberAccess(var1Parm, prop),
								Expression.MakeMemberAccess(var2Parm, prop)
							),
							Expression.IfThen(
								Expression.IsTrue(parm3),
								Expression.Call(var1Ret, typeof(List<string>).GetMethod("Add", new Type[] { typeof(string) }), Expression.Constant(trycol.Attribute.Name))
							),
							Expression.IfThen(
								Expression.IsFalse(parm3),
								Expression.Call(var1Ret, typeof(List<string>).GetMethod("Add", new Type[] { typeof(string) }), Expression.Constant(trycol.Attribute.Name))
							)
						)
					);
					a++;
				}
				exps.Add(Expression.Return(returnTarget, Expression.Call(var1Ret, typeof(List<string>).GetMethod("ToArray", new Type[0]))));
				exps.Add(Expression.Label(returnTarget, Expression.Constant(new string[0])));
				return Expression.Lambda<Func<object, object, bool, string[]>>(Expression.Block(new[] { var1Ret, var1Parm, var2Parm }, exps), new[] { parm1, parm2, parm3 }).Compile();
			});
			return func(up, oldval, isEqual);
		}
	}
}
