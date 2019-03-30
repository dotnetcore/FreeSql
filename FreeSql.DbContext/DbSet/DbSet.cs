using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FreeSql {

	internal class DbContextDbSet<TEntity> : DbSet<TEntity> where TEntity : class {

		public DbContextDbSet(DbContext ctx) {
			_ctx = ctx;
			_uow = ctx._uow;
			_fsql = ctx._fsql;
		}
	}

	public abstract partial class DbSet<TEntity> : IDisposable where TEntity : class {

		internal DbContext _ctx;
		internal IUnitOfWork _uow;
		internal IFreeSql _fsql;

		protected virtual ISelect<TEntity> OrmSelect(object dywhere) {
			DbContextExecCommand(); //查询前先提交，否则会出脏读
			return _fsql.Select<TEntity>(dywhere).WithTransaction(_uow?.GetOrBeginTransaction(false)).TrackToList(TrackToList);
		}

		~DbSet() {
			this.Dispose();
		}
		bool _isdisposed = false;
		public void Dispose() {
			if (_isdisposed) return;
			try {
				this._dicUpdateTimes.Clear();
				this._states.Clear();
			} finally {
				_isdisposed = true;
				GC.SuppressFinalize(this);
			}
		}

		protected virtual IInsert<TEntity> OrmInsert() => _fsql.Insert<TEntity>().WithTransaction(_uow?.GetOrBeginTransaction());
		protected virtual IInsert<TEntity> OrmInsert(TEntity data) => _fsql.Insert<TEntity>(data).WithTransaction(_uow?.GetOrBeginTransaction());
		protected virtual IInsert<TEntity> OrmInsert(IEnumerable<TEntity> data) => _fsql.Insert<TEntity>(data).WithTransaction(_uow?.GetOrBeginTransaction());

		protected virtual IUpdate<TEntity> OrmUpdate(IEnumerable<TEntity> entitys) => _fsql.Update<TEntity>().SetSource(entitys).WithTransaction(_uow?.GetOrBeginTransaction());
		protected virtual IDelete<TEntity> OrmDelete(object dywhere) => _fsql.Delete<TEntity>(dywhere).WithTransaction(_uow?.GetOrBeginTransaction());

		internal void EnqueueToDbContext(DbContext.ExecCommandInfoType actionType, EntityState state) {
			_ctx.EnqueueAction(actionType, this, typeof(EntityState), state);
		}
		internal void IncrAffrows(int affrows) {
			_ctx._affrows += affrows;
		}
		internal void TrackToList(object list) {
			if (list == null) return;
			var ls = list as IList<TEntity>;
			if (ls == null) return;

			foreach (var item in ls) {
				var key = _fsql.GetEntityKeyString(item);
				if (_states.ContainsKey(key)) {
					_fsql.MapEntityValue(item, _states[key].Value);
					_states[key].Time = DateTime.Now;
				} else {
					_states.Add(key, CreateEntityState(item));
				}
			}
		}

		public ISelect<TEntity> Select => this.OrmSelect(null);
		public ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => this.OrmSelect(null).Where(exp);
		public ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => this.OrmSelect(null).WhereIf(condition, exp);

		protected Dictionary<string, EntityState> _states = new Dictionary<string, EntityState>();
		internal Dictionary<string, EntityState> _statesInternal => _states;
		TableInfo _tablePriv;
		protected TableInfo _table => _tablePriv ?? (_tablePriv = _fsql.CodeFirst.GetTableByEntity(_entityType));
		ColumnInfo[] _tableIdentitysPriv;
		protected ColumnInfo[] _tableIdentitys => _tableIdentitysPriv ?? (_tableIdentitysPriv = _table.Primarys.Where(a => a.Attribute.IsIdentity).ToArray());
		protected Type _entityType = typeof(TEntity);
		internal Type _entityTypeInternal => _entityType;

		public class EntityState {
			public EntityState(TEntity value, string key) {
				this.Value = value;
				this.Key = key;
				this.Time = DateTime.Now;
			}
			public TEntity OldValue { get; set; }
			public TEntity Value { get; set; }
			public string Key { get; set; }
			public DateTime Time { get; set; }
		}

		#region Utils
		EntityState CreateEntityState(TEntity data) {
			if (data == null) throw new ArgumentNullException(nameof(data));
			var key = _fsql.GetEntityKeyString(data);
			var state = new EntityState(Activator.CreateInstance<TEntity>(), key);
			_fsql.MapEntityValue(data, state.Value);
			return state;
		}
		bool ExistsInStates(TEntity data) {
			if (data == null) throw new ArgumentNullException(nameof(data));
			var key = _fsql.GetEntityKeyString(data);
			if (string.IsNullOrEmpty(key)) return false;
			return _states.ContainsKey(key);
		}

		bool CanAdd(IEnumerable<TEntity> data, bool isThrow) {
			if (data == null) {
				if (isThrow) throw new ArgumentNullException(nameof(data));
				return false;
			}
			foreach (var s in data) if (CanAdd(s, isThrow) == false) return false;
			return true;
		}
		bool CanAdd(TEntity data, bool isThrow) {
			if (data == null) {
				if (isThrow) throw new ArgumentNullException(nameof(data));
				return false;
			}
			var key = _fsql.GetEntityKeyString(data);
			if (string.IsNullOrEmpty(key)) {
				switch (_fsql.Ado.DataType) {
					case DataType.SqlServer:
					case DataType.PostgreSQL:
						return true;
					case DataType.MySql:
					case DataType.Oracle:
					case DataType.Sqlite:
						if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1) {
							return true;
						}
						if (isThrow) throw new Exception($"不可添加，未设置主键的值：{_fsql.GetEntityString(data)}");
						return false;
				}
			} else {
				if (_states.ContainsKey(key)) {
					if (isThrow) throw new Exception($"不可添加，已存在于状态管理：{_fsql.GetEntityString(data)}");
					return false;
				}
				var idval = _fsql.GetEntityIdentityValueWithPrimary(data);
				if (idval > 0) {
					if (isThrow) throw new Exception($"不可添加，自增属性有值：{_fsql.GetEntityString(data)}");
					return false;
				}
			}
			return true;
		}

		bool CanUpdate(IEnumerable<TEntity> data, bool isThrow) {
			if (data == null) {
				if (isThrow) throw new ArgumentNullException(nameof(data));
				return false;
			}
			foreach (var s in data) if (CanUpdate(s, isThrow) == false) return false;
			return true;
		}
		bool CanUpdate(TEntity data, bool isThrow) {
			if (data == null) {
				if (isThrow) throw new ArgumentNullException(nameof(data));
				return false;
			}
			if (_table.Primarys.Any() == false) {
				if (isThrow) throw new Exception($"不可更新，实体没有主键：{_fsql.GetEntityString(data)}");
				return false;
			}
			var key = _fsql.GetEntityKeyString(data);
			if (string.IsNullOrEmpty(key)) {
				if (isThrow) throw new Exception($"不可更新，未设置主键的值：{_fsql.GetEntityString(data)}");
				return false;
			}
			if (_states.TryGetValue(key, out var tryval) == false) {
				if (isThrow) throw new Exception($"不可更新，数据未被跟踪，应该先查询：{_fsql.GetEntityString(data)}");
				return false;
			}
			return true;
		}

		bool CanRemove(IEnumerable<TEntity> data, bool isThrow) {
			if (data == null) {
				if (isThrow) throw new ArgumentNullException(nameof(data));
				return false;
			}
			foreach (var s in data) if (CanRemove(s, isThrow) == false) return false;
			return true;
		}
		bool CanRemove(TEntity data, bool isThrow) {
			if (data == null) {
				if (isThrow) throw new ArgumentNullException(nameof(data));
				return false;
			}
			if (_table.Primarys.Any() == false) {
				if (isThrow) throw new Exception($"不可删除，实体没有主键：{_fsql.GetEntityString(data)}");
				return false;
			}
			var key = _fsql.GetEntityKeyString(data);
			if (string.IsNullOrEmpty(key)) {
				if (isThrow) throw new Exception($"不可删除，未设置主键的值：{_fsql.GetEntityString(data)}");
				return false;
			}
			//if (_states.TryGetValue(key, out var tryval) == false) {
			//	if (isThrow) throw new Exception($"不可删除，数据未被跟踪，应该先查询：{_fsql.GetEntityString(data)}");
			//	return false;
			//}
			return true;
		}
		#endregion
	}
}
