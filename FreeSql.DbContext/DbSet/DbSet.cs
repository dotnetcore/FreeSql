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

		internal DbContext _ctx;
		IFreeSql _fsql => _ctx._fsql;

		ISelect<TEntity> OrmSelect(object dywhere) {
			_ctx.ExecCommand(); //查询前先提交，否则会出脏读
			return _fsql.Select<TEntity>(dywhere).WithTransaction(_ctx.GetOrBeginTransaction(false)).TrackToList(TrackToList);
		}

		IInsert<TEntity> OrmInsert() => _fsql.Insert<TEntity>().WithTransaction(_ctx.GetOrBeginTransaction());
		IInsert<TEntity> OrmInsert(TEntity data) => _fsql.Insert<TEntity>(data).WithTransaction(_ctx.GetOrBeginTransaction());
		IInsert<TEntity> OrmInsert(TEntity[] data) => _fsql.Insert<TEntity>(data).WithTransaction(_ctx.GetOrBeginTransaction());
		IInsert<TEntity> OrmInsert(IEnumerable<TEntity> data) => _fsql.Insert<TEntity>(data).WithTransaction(_ctx.GetOrBeginTransaction());

		IUpdate<TEntity> OrmUpdate(object dywhere) => _fsql.Update<TEntity>(dywhere).WithTransaction(_ctx.GetOrBeginTransaction());
		IDelete<TEntity> OrmDelete(object dywhere) => _fsql.Delete<TEntity>(dywhere).WithTransaction(_ctx.GetOrBeginTransaction());

		public ISelect<TEntity> Select => this.OrmSelect(null);
		public ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => this.OrmSelect(null).Where(exp);
		public ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => this.OrmSelect(null).WhereIf(condition, exp);

		Dictionary<string, EntityState> _states = new Dictionary<string, EntityState>();
		TableInfo _tablePriv;
		TableInfo _table => _tablePriv ?? (_tablePriv = _fsql.CodeFirst.GetTableByEntity(_entityType));
		ColumnInfo[] _tableIdentitysPriv;
		ColumnInfo[] _tableIdentitys => _tableIdentitysPriv ?? (_tableIdentitysPriv = _table.Primarys.Where(a => a.Attribute.IsIdentity).ToArray()); 
		Type _entityType = typeof(TEntity);

		bool _versionColumnPrivPrivIsInit = false;
		ColumnInfo _versionColumnPriv;
		ColumnInfo _versionColumn {
			get {
				if (_versionColumnPrivPrivIsInit == false) {
					var vc = _table.Properties.Where(a => _table.ColumnsByCs.ContainsKey(a.Key) && a.Value.GetCustomAttributes(typeof(VersionAttribute), false).Any());
					if (vc.Any()) {
						var col = _table.ColumnsByCs[vc.Last().Key];
						if (col.CsType.IsNullableType() || col.CsType.IsNumberType() == false)
							throw new Exception($"属性{col.CsName} 被标注为行级锁(Version)，但其必须为数字类型，并且不可为 Nullable");
						_versionColumnPriv = col;
					}
					_versionColumnPrivPrivIsInit = true;
				}
				return _versionColumnPriv;
			}
		}

		public class EntityState {
			public EntityState(TEntity value, string key) {
				this.Value = value;
				this.Key = key;
				this.Time = DateTime.Now;
			}
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
		bool CanAdd(TEntity[] data, bool isThrow) {
			if (data == null) {
				if (isThrow) throw new ArgumentNullException(nameof(data));
				return false;
			}
			foreach (var s in data) if (CanAdd(s, isThrow) == false) return false;
			return true;
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

		bool CanUpdate(TEntity[] data, bool isThrow) {
			if (data == null) {
				if (isThrow) throw new ArgumentNullException(nameof(data));
				return false;
			}
			foreach (var s in data) if (CanUpdate(s, isThrow) == false) return false;
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

		bool CanRemove(TEntity[] data, bool isThrow) {
			if (data == null) {
				if (isThrow) throw new ArgumentNullException(nameof(data));
				return false;
			}
			foreach (var s in data) if (CanRemove(s, isThrow) == false) return false;
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
			if (_states.TryGetValue(key, out var tryval) == false) {
				if (isThrow) throw new Exception($"不可更新，数据未被跟踪，应该先查询：{_fsql.GetEntityString(data)}");
				return false;
			}
			return true;
		}
		#endregion
		
		void TrackToList(object list) {
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
	}

	internal class BaseDbSet<TEntity> : DbSet<TEntity> where TEntity : class {
		
		public BaseDbSet(DbContext ctx) {
			_ctx = ctx;
		}
	}
}
