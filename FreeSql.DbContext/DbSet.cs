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
using FreeSql.Extensions;

namespace FreeSql {
	public abstract partial class DbSet<TEntity> where TEntity : class {

		protected DbContext _ctx;
		IFreeSql _fsql => _ctx._fsql;

		protected ISelect<TEntity> OrmSelect(object dywhere) => _fsql.Select<TEntity>(dywhere).WithTransaction(_ctx.GetOrBeginTransaction(false)).TrackToList(TrackToList);

		protected IInsert<TEntity> OrmInsert() => _fsql.Insert<TEntity>().WithTransaction(_ctx.GetOrBeginTransaction());
		protected IInsert<TEntity> OrmInsert(TEntity source) => _fsql.Insert<TEntity>(source).WithTransaction(_ctx.GetOrBeginTransaction());
		protected IInsert<TEntity> OrmInsert(TEntity[] source) => _fsql.Insert<TEntity>(source).WithTransaction(_ctx.GetOrBeginTransaction());
		protected IInsert<TEntity> OrmInsert(IEnumerable<TEntity> source) => _fsql.Insert<TEntity>(source).WithTransaction(_ctx.GetOrBeginTransaction());

		protected IUpdate<TEntity> OrmUpdate(object dywhere) => _fsql.Update<TEntity>(dywhere).WithTransaction(_ctx.GetOrBeginTransaction());
		protected IDelete<TEntity> OrmDelete(object dywhere) => _fsql.Delete<TEntity>(dywhere).WithTransaction(_ctx.GetOrBeginTransaction());

		public ISelect<TEntity> Select => this.OrmSelect(null);
		public ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => this.OrmSelect(null).Where(exp);
		public ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => this.OrmSelect(null).WhereIf(condition, exp);

		protected Dictionary<string, EntityState> _vals = new Dictionary<string, EntityState>();
		TableInfo _tablePriv;
		protected TableInfo _table => _tablePriv ?? (_tablePriv = _fsql.CodeFirst.GetTableByEntity(_entityType));
		ColumnInfo[] _tableIdentitysPriv;
		protected ColumnInfo[] _tableIdentitys => _tableIdentitysPriv ?? (_tableIdentitysPriv = _table.Primarys.Where(a => a.Attribute.IsIdentity).ToArray()); 
		protected Type _entityType = typeof(TEntity);

		public class EntityState {
			public TEntity Value { get; set; }
			public string Key { get; set; }
			public DateTime Time { get; set; }
		}

		int DbContextBetcAdd(EntityState[] dels) {
			if (dels.Any() == false) return 0;
			var affrows = this.OrmInsert(dels.Select(a => a.Value)).ExecuteAffrows();
			return affrows;
		}
		public void Add(TEntity source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			var key = _fsql.GetEntityKeyString(source);
			EntityState state = new EntityState();
			if (string.IsNullOrEmpty(key)) {
				switch(_fsql.Ado.DataType) {
					case DataType.SqlServer:
					case DataType.PostgreSQL:
						if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1) {
							_ctx.ExecCommand();
							var idtval = this.OrmInsert(source).ExecuteIdentity();
							_ctx._affrows++;
							_fsql.SetEntityIdentityValue(source, idtval);
						} else {
							_ctx.ExecCommand();
							state.Value = this.OrmInsert(source).ExecuteInserted().First();
							_ctx._affrows++;
							_fsql.CopyEntityValue(source, state.Value);
						}
						break;
					case DataType.MySql:
					case DataType.Oracle:
					case DataType.Sqlite:
						if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1) {
							_ctx.ExecCommand();
							var idtval = this.OrmInsert(source).ExecuteIdentity();
							_ctx._affrows++;
							_fsql.SetEntityIdentityValue(source, idtval);
						} else {
							throw new Exception($"DbSet.Add 失败，未设置主键的值，或者没有配置自增，或者自增列数不为1：{_fsql.GetEntityString(source)}");
						}
						break;
				}

				state.Key = key = _fsql.GetEntityKeyString(source);
				state.Time = DateTime.Now;
			} else {
				if (_vals.ContainsKey(key))
					throw new Exception($"DbSet.Add 失败，实体数据已存在，请勿重复添加：{_fsql.GetEntityString(source)}");
				_ctx.EnqueueAction(DbContext.ExecCommandInfoType.Insert, this, typeof(EntityState), state);
			}
			if (state.Value == null) {
				state.Value = Activator.CreateInstance<TEntity>();
				_fsql.CopyEntityValue(state.Value, source); //copy, 记录旧值版本
			}
			_vals.Add(key, state);
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

		int DbContextBetchUpdate(EntityState[] ups) => DbContextBetchUpdatePriv(ups, false);
		int DbContextBetchUpdateNow(EntityState[] ups) => DbContextBetchUpdatePriv(ups, true);
		int DbContextBetchUpdatePriv(EntityState[] ups, bool isLiveUpdate) {
			if (ups.Any() == false) return 0;
			var uplst1 = ups[ups.Length - 1];
			var uplst2 = ups.Length > 1 ? ups[ups.Length - 2] : null;

			if (_vals.TryGetValue(uplst1.Key, out var lstval1) == false) return -999;
			var lstval2 = default(EntityState);
			if (uplst2 != null && _vals.TryGetValue(uplst2.Key, out lstval2) == false) throw new Exception($"DbSet.Update 失败，实体应该先查询再修改：{_fsql.GetEntityString(uplst2.Value)}");

			var cuig1 = _fsql.CompareEntityValueReturnColumns(uplst1.Value, lstval1.Value, true);
			var cuig2 = uplst2 != null ? _fsql.CompareEntityValueReturnColumns(uplst2.Value, lstval2.Value, true) : null;
			if (uplst2 != null && string.Compare(string.Join(",", cuig1), string.Join(",", cuig2)) != 0) {
				//最后一个不保存
				var source = ups.ToList();
				source.RemoveAt(ups.Length - 1);
				var affrows = this.OrmUpdate(null).SetSource(source.Select(a => a.Value)).IgnoreColumns(cuig2).ExecuteAffrows();
				foreach (var newval in source) {
					if (_vals.TryGetValue(newval.Key, out var tryold))
						_fsql.CopyEntityValue(tryold.Value, newval.Value);
				}
				return affrows;
			} else if (isLiveUpdate) {
				//立即保存
				var source = ups;
				var affrows = this.OrmUpdate(null).SetSource(source.Select(a => a.Value)).IgnoreColumns(cuig1).ExecuteAffrows();
				foreach (var newval in source) {
					if (_vals.TryGetValue(newval.Key, out var tryold))
						_fsql.CopyEntityValue(tryold.Value, newval.Value);
				}
				return Math.Min(ups.Length, affrows);
			}
			//等待下次对比再保存
			return 0;
		}
		public void Update(TEntity source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (_table.Primarys.Any() == false) throw new Exception($"DbSet.Update 失败，实体没有主键：{_fsql.GetEntityString(source)}");
			var key = _fsql.GetEntityKeyString(source);
			if (string.IsNullOrEmpty(key)) throw new Exception($"DbSet.Update 失败，未设置主键的值：{_fsql.GetEntityString(source)}");
			if (_vals.TryGetValue(key, out var tryval) == false) throw new Exception($"DbSet.Update 失败，实体未被跟踪，更新前应该先做查询：{_fsql.GetEntityString(source)}");

			var snap = Activator.CreateInstance<TEntity>();
			_fsql.CopyEntityValue(snap, source); //copy，避免SaveChanges前对象再次被修改
			_ctx.EnqueueAction(DbContext.ExecCommandInfoType.Update, this, typeof(EntityState), new EntityState { Value = snap, Key = key, Time = DateTime.Now });
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

		int DbContextBetchRemove(EntityState[] dels) {
			if (dels.Any() == false) return 0;
			var affrows = this.OrmDelete(dels.Select(a => a.Value)).ExecuteAffrows();
			//foreach (var del in dels)
			//	_vals.Remove(del.Key);
			return affrows;
		}
		public void Remove(TEntity source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (_table.Primarys.Any() == false) throw new Exception($"DbSet.Remove 失败，实体没有主键：{_fsql.GetEntityString(source)}");
			var key = _fsql.GetEntityKeyString(source);
			if (string.IsNullOrEmpty(key)) throw new Exception($"DbSet.Remove 失败，未设置主键的值：{_fsql.GetEntityString(source)}");
			if (_vals.TryGetValue(key, out var tryval) == false) throw new Exception($"DbSet.Remove 失败，实体未被跟踪，删除前应该先做查询：{_fsql.GetEntityString(source)}");

			var snap = Activator.CreateInstance<TEntity>();
			_fsql.CopyEntityValue(snap, source); //copy，避免SaveChanges前对象再次被修改
			_ctx.EnqueueAction(DbContext.ExecCommandInfoType.Delete, this, typeof(EntityState), new EntityState { Value = snap, Key = key, Time = DateTime.Now });

			_vals.Remove(key);
			_fsql.ClearEntityPrimaryValueWithIdentityAndGuid(source);
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

		void TrackToList(object list) {
			if (list == null) return;
			var ls = list as IList<TEntity>;

			foreach (var item in ls) {
				var key = _fsql.GetEntityKeyString(item);
				if (_vals.ContainsKey(key)) {
					_fsql.CopyEntityValue(_vals[key].Value, item);
					_vals[key].Time = DateTime.Now;
				} else {
					var snap = Activator.CreateInstance<TEntity>();
					_fsql.CopyEntityValue(snap, item);
					_vals.Add(key, new EntityState { Value = snap, Key = key, Time = DateTime.Now });
				}
			}
		}
	}

	internal class BaseDbSet<TEntity> : DbSet<TEntity> where TEntity : class {
		
		public BaseDbSet(DbContext ctx) {
			_ctx = ctx;
		}
	}
}
