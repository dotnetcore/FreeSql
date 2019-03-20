using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace FreeSql {
	public abstract partial class DbSet<TEntity> where TEntity : class {

		protected DbContext _ctx;

		protected ISelect<TEntity> OrmSelect(object dywhere) => _ctx._fsql.Select<TEntity>(dywhere).WithTransaction(_ctx.GetOrBeginTransaction(false));

		protected IInsert<TEntity> OrmInsert() => _ctx._fsql.Insert<TEntity>().WithTransaction(_ctx.GetOrBeginTransaction());
		protected IInsert<TEntity> OrmInsert(TEntity source) => _ctx._fsql.Insert<TEntity>(source).WithTransaction(_ctx.GetOrBeginTransaction());
		protected IInsert<TEntity> OrmInsert(TEntity[] source) => _ctx._fsql.Insert<TEntity>(source).WithTransaction(_ctx.GetOrBeginTransaction());
		protected IInsert<TEntity> OrmInsert(IEnumerable<TEntity> source) => _ctx._fsql.Insert<TEntity>(source).WithTransaction(_ctx.GetOrBeginTransaction());

		protected IUpdate<TEntity> OrmUpdate(object dywhere) => _ctx._fsql.Update<TEntity>(dywhere).WithTransaction(_ctx.GetOrBeginTransaction());
		protected IDelete<TEntity> OrmDelete(object dywhere) => _ctx._fsql.Delete<TEntity>(dywhere).WithTransaction(_ctx.GetOrBeginTransaction());

		public ISelect<TEntity> Select => this.OrmSelect(null);
		public ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => this.OrmSelect(null).Where(exp);
		public ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => this.OrmSelect(null).WhereIf(condition, exp);

		protected Dictionary<string, TEntity> _vals = new Dictionary<string, TEntity>();
		TableInfo _tablePriv;
		protected TableInfo _table => _tablePriv ?? (_tablePriv = _ctx._orm.CodeFirst.GetTableByEntity(_entityType));
		protected Type _entityType = typeof(TEntity);

		static ConcurrentDictionary<Type, Func<TEntity, string>> _dicGetEntityKeyString = new ConcurrentDictionary<Type, Func<TEntity, string>>();
		static MethodInfo MethodStringBuilderAppend = typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(object) });
		static MethodInfo MethodStringBuilderToString = typeof(StringBuilder).GetMethod("ToString", new Type[0]);
		static PropertyInfo MethodStringBuilderLength = typeof(StringBuilder).GetProperty("Length");
		static MethodInfo MethodStringConcat = typeof(string).GetMethod("Concat", new Type[]{ typeof(object) });
		string GetEntityKeyString(TEntity item) {
			var func = _dicGetEntityKeyString.GetOrAdd(_entityType, t => {
				var pks = _table.Primarys;
				var returnTarget = Expression.Label(typeof(string));
				var parm1 = Expression.Parameter(_entityType);
				var var1Sb = Expression.Variable(typeof(StringBuilder));
				var var3IsNull = Expression.Variable(typeof(bool));
				var exps = new List<Expression>();

				exps.AddRange(new Expression[] {
					Expression.Assign(var1Sb, Expression.New(typeof(StringBuilder))),
					Expression.Assign(var3IsNull, Expression.Constant(false))
				});
				for (var a = 0; a < pks.Length; a++) {
					exps.Add(
						Expression.IfThen(
							Expression.Equal(var3IsNull, Expression.Constant(false)),
							Expression.IfThenElse(
								Expression.Equal(Expression.MakeMemberAccess(parm1, _table.Properties[pks[a].CsName]), Expression.Default(pks[a].CsType)),
								Expression.Assign(var3IsNull, Expression.Constant(true)),
								Expression.Block(
									new Expression[]{
										a > 0 ? Expression.Call(var1Sb, MethodStringBuilderAppend, Expression.Constant("*|_,,_|*" )) : null,
										Expression.Call(var1Sb, MethodStringBuilderAppend,
											Expression.Convert(Expression.MakeMemberAccess(parm1, _table.Properties[pks[a].CsName]), typeof(object))
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
						Expression.Return(returnTarget, Expression.Call(var1Sb, MethodStringBuilderToString))
					)
				);
				exps.Add(Expression.Label(returnTarget, Expression.Default(typeof(string))));
				return Expression.Lambda<Func<TEntity, string>>(Expression.Block(new[] { var1Sb, var3IsNull }, exps), new[] { parm1 }).Compile();
			});
			return func(item);
		}

		static ConcurrentDictionary<Type, Action<TEntity, TEntity>> _dicCopyNewValueToEntity = new ConcurrentDictionary<Type, Action<TEntity, TEntity>>();
		void CopyNewValueToEntity(TEntity old, TEntity newvalue) {
			var func = _dicCopyNewValueToEntity.GetOrAdd(_entityType, t => {
				var parm1 = Expression.Parameter(_entityType);
				var parm2 = Expression.Parameter(_entityType);
				var exps = new List<Expression>();
				foreach (var prop in _table.Properties.Values) {
					if (_table.ColumnsByCs.ContainsKey(prop.Name)) {
						exps.Add(
							Expression.Assign(
								Expression.MakeMemberAccess(parm1, prop),
								Expression.MakeMemberAccess(parm2, prop)
							)
						);
					} else {
						exps.Add(
							Expression.Assign(
								Expression.MakeMemberAccess(parm1, prop),
								Expression.Default(prop.PropertyType)
							)
						);
					}
				}
				return Expression.Lambda<Action<TEntity, TEntity>>(Expression.Block(exps), new[] { parm1, parm2 }).Compile();
			});
			func(old, newvalue);
		}

		static ConcurrentDictionary<Type, Action<TEntity, long>> _dicSetEntityIdentityValue = new ConcurrentDictionary<Type, Action<TEntity, long>>();
		void SetEntityIdentityValue(TEntity old, long idtval) {
			var func = _dicSetEntityIdentityValue.GetOrAdd(_entityType, t => {
				var parm1 = Expression.Parameter(_entityType);
				var parm2 = Expression.Parameter(typeof(long));
				var exps = new List<Expression>();
				exps.Add(
					Expression.Assign(
						Expression.MakeMemberAccess(parm1, _table.Properties[_table.Primarys[0].CsName]),
						Expression.Convert(FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(_table.Primarys[0].CsType, Expression.Convert(parm2, typeof(object))), _table.Primarys[0].CsType)
					)
				);
				return Expression.Lambda<Action<TEntity, long>>(Expression.Block(exps), new[] { parm1, parm2 }).Compile();
			});
			func(old, idtval);
		}

		public void Add(TEntity source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			var key = GetEntityKeyString(source);
			TEntity newval = null;
			if (string.IsNullOrEmpty(key)) {
				var ids = _table.Primarys.Where(a => a.Attribute.IsIdentity).ToArray();

				switch(_ctx._orm.Ado.DataType) {
					case DataType.SqlServer:
					case DataType.PostgreSQL:
						if (ids.Length == 1 && _table.Primarys.Length == 1) {
							_ctx.ExecCommand();
							var idtval = this.OrmInsert(source).ExecuteIdentity();
							_ctx._affrows++;
							SetEntityIdentityValue(source, idtval);
						} else {
							_ctx.ExecCommand();
							newval = this.OrmInsert(source).ExecuteInserted().First();
							_ctx._affrows++;
							CopyNewValueToEntity(source, newval);
						}
						break;
					case DataType.MySql:
					case DataType.Oracle:
					case DataType.Sqlite:
						if (ids.Length == 1 && _table.Primarys.Length == 1) {
							_ctx.ExecCommand();
							var idtval = this.OrmInsert(source).ExecuteIdentity();
							_ctx._affrows++;
							SetEntityIdentityValue(source, idtval);
						} else {
							throw new Exception("DbSet.Add 失败，由于实体没有主键值，或者没有配置自增，或者自增列数不为1。");
						}
						break;
				}

				key = GetEntityKeyString(source);
			} else {
				if (_vals.ContainsKey(key))
					throw new Exception("DbSet.Add 失败，实体数据已存在，请勿重复添加。");
				_ctx.EnqueueAction(DbContext.ExecCommandInfoType.Insert, _entityType, this, source);
			}
			if (newval == null) {
				newval = Activator.CreateInstance<TEntity>();
				CopyNewValueToEntity(newval, source);
			}
			_vals.Add(key, newval);
		}
		public void AddRange(TEntity[] source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			for (var a = 0; a < source.Length; a++)
				Add(source[a]);
		}
		public void AddRange(IEnumerable<TEntity> source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			foreach(var item in source)
				Add(item);
		}

		static ConcurrentDictionary<Type, Func<TEntity, TEntity, string>> _dicCompareUpdateIngoreColumns = new ConcurrentDictionary<Type, Func<TEntity, TEntity, string>>();
		string CompareUpdateIngoreColumns(TEntity up, TEntity oldval) {
			var func = _dicCompareUpdateIngoreColumns.GetOrAdd(_entityType, t => {
				var returnTarget = Expression.Label(typeof(string));
				var parm1 = Expression.Parameter(_entityType);
				var parm2 = Expression.Parameter(_entityType);
				var var1Sb = Expression.Variable(typeof(StringBuilder));
				var exps = new List<Expression>();

				exps.AddRange(new Expression[] {
					Expression.Assign(var1Sb, Expression.New(typeof(StringBuilder)))
				});
				var a = 0;
				foreach (var prop in _table.Properties.Values) {
					if (_table.ColumnsByCs.TryGetValue(prop.Name, out var trycol) == false) continue;
					exps.Add(
						Expression.IfThen(
							Expression.Equal(
								Expression.MakeMemberAccess(parm1, prop),
								Expression.MakeMemberAccess(parm2, prop)
							),
							Expression.Block(
								new Expression[]{
									a > 0 ? Expression.Call(var1Sb, MethodStringBuilderAppend, Expression.Constant(", " )) : null,
									Expression.Call(var1Sb, MethodStringBuilderAppend, Expression.Constant(trycol.Attribute.Name))
								}.Where(c => c != null).ToArray()
							)
						)
					);
					a++;
				}
				exps.Add(Expression.Return(returnTarget, Expression.Call(var1Sb, MethodStringBuilderToString)));
				exps.Add(Expression.Label(returnTarget, Expression.Default(typeof(string))));
				return Expression.Lambda<Func<TEntity, TEntity, string>>(Expression.Block(new[] { var1Sb }, exps), new[] { parm1, parm2 }).Compile();
			});
			return func(up, oldval);
		}
		int DbContextBetchUpdate(TEntity[] ups, bool isLiveUpdate) {
			if (ups.Any() == false) return 0;
			var uplst1 = ups[ups.Length - 1];
			var uplst2 = ups.Length > 1 ? ups[ups.Length - 2] : null;

			var lstkey1 = GetEntityKeyString(uplst1);
			if (_vals.TryGetValue(lstkey1, out var lstval1) == false) throw new Exception("DbSet.Update 失败，实体应该先查询再修改。");
			TEntity lstval2 = default(TEntity);
			if (uplst2 != null) {
				var lstkey2 = GetEntityKeyString(uplst2);
				if (_vals.TryGetValue(lstkey2, out lstval2) == false) throw new Exception("DbSet.Update 失败，实体应该先查询再修改。");
			}

			var cuig1 = CompareUpdateIngoreColumns(uplst1, lstval1);
			var cuig2 = uplst2 != null ? CompareUpdateIngoreColumns(uplst2, lstval2) : null;
			if (uplst2 != null && string.Compare(cuig1, cuig2, true) != 0) {
				//最后一个不保存
				var ignores = cuig2.Split(new[] { ", " }, StringSplitOptions.None);
				var source = ups.ToList();
				source.RemoveAt(ups.Length - 1);
				var affrows = this.OrmUpdate(null).SetSource(source).IgnoreColumns(ignores).ExecuteAffrows();
				foreach(var newval in source) {
					var newkey = GetEntityKeyString(newval);
					if (_vals.TryGetValue(newkey, out var tryold))
						CopyNewValueToEntity(tryold, newval);
				}
				return affrows;
			} else if (isLiveUpdate) {
				//立即保存
				var ignores = cuig1.Split(new[] { ", " }, StringSplitOptions.None);
				var affrows = this.OrmUpdate(null).SetSource(ups).IgnoreColumns(ignores).ExecuteAffrows();
				foreach (var newval in ups) {
					var newkey = GetEntityKeyString(newval);
					if (_vals.TryGetValue(newkey, out var tryold))
						CopyNewValueToEntity(tryold, newval);
				}
				return Math.Min(ups.Length, affrows);
			}
			//等待下次对比再保存
			return 0;
		}

		public void Update(TEntity source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (_table.Primarys.Any() == false) throw new Exception("DbSet.Update 失败，实体没有主键。");
			var key = GetEntityKeyString(source);
			if (string.IsNullOrEmpty(key)) throw new Exception("DbSet.Update 失败，实体没有设置主键值。");

			var snap = Activator.CreateInstance<TEntity>();
			CopyNewValueToEntity(snap, source);
			if (_vals.TryGetValue(key, out var val) == false) _vals.Add(key, snap);

			_ctx.EnqueueAction(DbContext.ExecCommandInfoType.Update, _entityType, this, snap);
		}
		public void UpdateRange(TEntity[] source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			for (var a = 0; a < source.Length; a++)
				Update(source[a]);
		}
		public void UpdateRange(IEnumerable<TEntity> source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			foreach (var item in source)
				Update(item);
		}

		int DbContextBetchRemove(TEntity[] dels) {
			if (dels.Any() == false) return 0;

			var affrows = this.OrmDelete(dels).ExecuteAffrows();
			foreach(var del in dels) {
				var key = GetEntityKeyString(del);
				_vals.Remove(key);
			}
			return affrows;
		}

		public void Remove(TEntity source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (_table.Primarys.Any() == false) throw new Exception("DbSet.Remove 失败，实体没有主键。");
			var key = GetEntityKeyString(source);
			if (string.IsNullOrEmpty(key)) throw new Exception("DbSet.Remove 失败，实体没有设置主键值。");

			var snap = Activator.CreateInstance<TEntity>();
			CopyNewValueToEntity(snap, source);
			if (_vals.TryGetValue(key, out var val) == false) _vals.Add(key, snap);

			_ctx.EnqueueAction(DbContext.ExecCommandInfoType.Delete, _entityType, this, snap);
		}
		public void RemoveRange(TEntity[] source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			for (var a = 0; a < source.Length; a++)
				Remove(source[a]);
		}
		public void RemoveRange(IEnumerable<TEntity> source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			foreach (var item in source)
				Remove(item);
		}
	}

	internal class BaseDbSet<TEntity> : DbSet<TEntity> where TEntity : class {
		
		public BaseDbSet(DbContext ctx) {
			_ctx = ctx;
		}
	}
}
