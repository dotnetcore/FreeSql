using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace FreeSql
{

    class DbContextDbSet<TEntity> : DbSet<TEntity> where TEntity : class
    {
        public DbContextDbSet(DbContext ctx)
        {
            _db = ctx;
            _uow = ctx.UnitOfWork;
        }
    }

    public interface IDbSet : IDisposable
    {
        Type EntityType { get; }
    }
    public abstract partial class DbSet<TEntity> : IDbSet where TEntity : class
    {
        internal DbContext _db { get; set; }
        internal IUnitOfWork _uow { get; set; }

        protected virtual ISelect<TEntity> OrmSelect(object dywhere)
        {
            DbContextExecCommand(); //查询前先提交，否则会出脏读
            return _db.Orm.Select<TEntity>().AsType(_entityType).WithTransaction(_uow?.GetOrBeginTransaction(false)).TrackToList(TrackToList).WhereDynamic(dywhere);
        }

        ~DbSet() => this.Dispose();
        int _disposeCounter;
        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            try
            {
                this._dicUpdateTimes.Clear();
                this._states.Clear();
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        protected virtual IInsert<TEntity> OrmInsert() => _db.Orm.Insert<TEntity>().AsType(_entityType).WithTransaction(_uow?.GetOrBeginTransaction());
        protected virtual IInsert<TEntity> OrmInsert(TEntity data) => _db.Orm.Insert<TEntity>().AsType(_entityType).WithTransaction(_uow?.GetOrBeginTransaction()).AppendData(data);
        protected virtual IInsert<TEntity> OrmInsert(IEnumerable<TEntity> data) => _db.Orm.Insert<TEntity>().AsType(_entityType).WithTransaction(_uow?.GetOrBeginTransaction()).AppendData(data);

        protected virtual IUpdate<TEntity> OrmUpdate(IEnumerable<TEntity> entitys) => _db.Orm.Update<TEntity>().AsType(_entityType).SetSource(entitys).WithTransaction(_uow?.GetOrBeginTransaction());
        protected virtual IDelete<TEntity> OrmDelete(object dywhere) => _db.Orm.Delete<TEntity>().AsType(_entityType).WithTransaction(_uow?.GetOrBeginTransaction()).WhereDynamic(dywhere);

        internal void EnqueueToDbContext(DbContext.EntityChangeType changeType, EntityState state) =>
            _db.EnqueueAction(changeType, this, typeof(EntityState), _entityType, state);

        internal void IncrAffrows(int affrows) =>
            _db._affrows += affrows;

        internal void TrackToList(object list)
        {
            if (list == null) return;
            var ls = list as IList<TEntity>;
            if (ls == null)
            {
                var ie = list as IEnumerable;
                if (ie == null) return;
                foreach (var item in ie)
                {
                    if (item == null) return;
                    var itemType = item.GetType();
                    if (itemType == typeof(object)) return;
                    if (itemType.FullName.StartsWith("Submission#")) itemType = itemType.BaseType;
                    if (_db.Orm.CodeFirst.GetTableByEntity(itemType)?.Primarys.Any() != true) return;
                    var dbset = _db.Set(itemType);
                    dbset?.GetType().GetMethod("TrackToList", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(dbset, new object[] { list });
                    return;
                }
                return;
            }
            if (_table?.Primarys.Any() != true) return;
            foreach (var item in ls)
            {
                var key = _db.Orm.GetEntityKeyString(_entityType, item, false);
                if (key == null) continue;
                _states.AddOrUpdate(key, k => CreateEntityState(item), (k, ov) =>
                {
                    _db.Orm.MapEntityValue(_entityType, item, ov.Value);
                    ov.Time = DateTime.Now;
                    return ov;
                });
            }
        }

        public ISelect<TEntity> Select => this.OrmSelect(null);
        public ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => this.OrmSelect(null).Where(exp);
        public ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => this.OrmSelect(null).WhereIf(condition, exp);

        protected ConcurrentDictionary<string, EntityState> _states = new ConcurrentDictionary<string, EntityState>();
        internal ConcurrentDictionary<string, EntityState> _statesInternal => _states;
        TableInfo _tablePriv;
        protected TableInfo _table => _tablePriv ?? (_tablePriv = _db.Orm.CodeFirst.GetTableByEntity(_entityType));
        ColumnInfo[] _tableIdentitysPriv, _tableServerTimesPriv;
        protected ColumnInfo[] _tableIdentitys => _tableIdentitysPriv ?? (_tableIdentitysPriv = _table.Primarys.Where(a => a.Attribute.IsIdentity).ToArray());
        protected ColumnInfo[] _tableServerTimes => _tableServerTimesPriv ?? (_tableServerTimesPriv = _table.Primarys.Where(a => a.Attribute.ServerTime != DateTimeKind.Unspecified).ToArray());
        protected Type _entityType = typeof(TEntity);
        public Type EntityType => _entityType;

        /// <summary>
        /// 动态Type，在使用 DbSet&lt;object&gt; 后使用本方法，指定实体类型
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public DbSet<TEntity> AsType(Type entityType)
        {
            if (entityType == typeof(object)) throw new Exception("ISelect.AsType 参数不支持指定为 object");
            if (entityType == _entityType) return this;
            var newtb = _db.Orm.CodeFirst.GetTableByEntity(entityType);
            _entityType = entityType;
            _tablePriv = newtb ?? throw new Exception("DbSet.AsType 参数错误，请传入正确的实体类型");
            _tableIdentitysPriv = null;
            _tableServerTimesPriv = null;
            return this;
        }

        Dictionary<Type, DbSet<object>> _dicDbSetObjects = new Dictionary<Type, DbSet<object>>();
        DbSet<object> GetDbSetObject(Type et)
        {
            if (_dicDbSetObjects.TryGetValue(et, out var tryds)) return tryds;
            _dicDbSetObjects.Add(et, tryds = _db.Set<object>().AsType(et));
            if (_db.InternalDicSet.TryGetValue(et, out var tryds2))
            {
                var copyTo = typeof(DbSet<>).MakeGenericType(et).GetMethod("StatesCopyToDbSetObject", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(DbSet<object>) }, null);
                copyTo?.Invoke(tryds2, new object[] { tryds });
            }
            return tryds;
        }
        void StatesCopyToDbSetObject(DbSet<object> ds)
        {
            ds.AttachRange(_states.Values.OrderBy(a => a.Time).Select(a => a.Value).ToArray());
        }

        public class EntityState
        {
            public EntityState(TEntity value, string key)
            {
                this.Value = value;
                this.Key = key;
                this.Time = DateTime.Now;
            }
            public TEntity OldValue { get; set; }
            public TEntity Value { get; set; }
            public string Key { get; set; }
            public DateTime Time { get; set; }
        }
        /// <summary>
        /// 附加实体，可用于不查询就更新或删除
        /// </summary>
        /// <param name="data"></param>
        public void Attach(TEntity data) => AttachRange(new[] { data });
        public void AttachRange(IEnumerable<TEntity> data)
        {
            if (data == null || data.Any() == false) return;
            if (_table.Primarys.Any() == false) throw new Exception($"不可附加，实体没有主键：{_db.Orm.GetEntityString(_entityType, data.First())}");
            foreach (var item in data)
            {
                var key = _db.Orm.GetEntityKeyString(_entityType, item, false);
                if (string.IsNullOrEmpty(key)) throw new Exception($"不可附加，未设置主键的值：{_db.Orm.GetEntityString(_entityType, item)}");

                _states.AddOrUpdate(key, k => CreateEntityState(item), (k, ov) =>
                {
                    _db.Orm.MapEntityValue(_entityType, item, ov.Value);
                    ov.Time = DateTime.Now;
                    return ov;
                });
            }
        }
        /// <summary>
        /// 附加实体，并且只附加主键值，可用于不更新属性值为null或默认值的字段
        /// </summary>
        /// <param name="data"></param>
        public DbSet<TEntity> AttachOnlyPrimary(TEntity data)
        {
            if (data == null) return this;
            var pkitem = (TEntity)Activator.CreateInstance(_entityType);
            foreach (var pk in _db.Orm.CodeFirst.GetTableByEntity(_entityType).Primarys)
            {
                var colVal = _db.Orm.GetEntityValueWithPropertyName(_entityType, data, pk.CsName);
                _db.Orm.SetEntityValueWithPropertyName(_entityType, pkitem, pk.CsName, colVal);
            }
            this.Attach(pkitem);
            return this;
        }

        /// <summary>
        /// 清空状态数据
        /// </summary>
        public void FlushState()
        {
            _states.Clear();
        }

        #region Utils
        EntityState CreateEntityState(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var key = _db.Orm.GetEntityKeyString(_entityType, data, false);
            var state = new EntityState((TEntity)Activator.CreateInstance(_entityType), key);
            _db.Orm.MapEntityValue(_entityType, data, state.Value);
            return state;
        }
        bool? ExistsInStates(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var key = _db.Orm.GetEntityKeyString(_entityType, data, false);
            if (string.IsNullOrEmpty(key)) return null;
            return _states.ContainsKey(key);
        }

        bool CanAdd(IEnumerable<TEntity> data, bool isThrow)
        {
            if (data == null)
            {
                if (isThrow) throw new ArgumentNullException(nameof(data));
                return false;
            }
            if (data.Any() == false) return false;
            foreach (var s in data) if (CanAdd(s, isThrow) == false) return false;
            return true;
        }
        bool CanAdd(TEntity data, bool isThrow)
        {
            if (data == null)
            {
                if (isThrow) throw new ArgumentNullException(nameof(data));
                return false;
            }
            if (_table.Primarys.Any() == false)
            {
                if (isThrow) throw new Exception($"不可添加，实体没有主键：{_db.Orm.GetEntityString(_entityType, data)}");
                return false;
            }
            FreeSql.Internal.CommonProvider.InsertProvider<TEntity>.AuditDataValue(this, data, _db.Orm, _table, null);
            var key = _db.Orm.GetEntityKeyString(_entityType, data, true);
            if (string.IsNullOrEmpty(key))
            {
                switch (_db.Orm.Ado.DataType)
                {
                    case DataType.SqlServer:
                    case DataType.OdbcSqlServer:
                    case DataType.PostgreSQL:
                    case DataType.OdbcPostgreSQL:
                        return true;
                    default:
                        if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1)
                            return true;
                        if (isThrow) throw new Exception($"不可添加，未设置主键的值：{_db.Orm.GetEntityString(_entityType, data)}");
                        return false;
                }
            }
            else
            {
                if (_states.ContainsKey(key))
                {
                    if (isThrow) throw new Exception($"不可添加，已存在于状态管理：{_db.Orm.GetEntityString(_entityType, data)}");
                    return false;
                }
                var idval = _db.Orm.GetEntityIdentityValueWithPrimary(_entityType, data);
                if (idval > 0)
                {
                    if (isThrow) throw new Exception($"不可添加，自增属性有值：{_db.Orm.GetEntityString(_entityType, data)}");
                    return false;
                }
            }
            return true;
        }

        bool CanUpdate(IEnumerable<TEntity> data, bool isThrow)
        {
            if (data == null)
            {
                if (isThrow) throw new ArgumentNullException(nameof(data));
                return false;
            }
            if (data.Any() == false) return false;
            foreach (var s in data) if (CanUpdate(s, isThrow) == false) return false;
            return true;
        }
        bool CanUpdate(TEntity data, bool isThrow)
        {
            if (data == null)
            {
                if (isThrow) throw new ArgumentNullException(nameof(data));
                return false;
            }
            if (_table.Primarys.Any() == false)
            {
                if (isThrow) throw new Exception($"不可更新，实体没有主键：{_db.Orm.GetEntityString(_entityType, data)}");
                return false;
            }
            FreeSql.Internal.CommonProvider.UpdateProvider<TEntity>.AuditDataValue(this, data, _db.Orm, _table, null);
            var key = _db.Orm.GetEntityKeyString(_entityType, data, false);
            if (string.IsNullOrEmpty(key))
            {
                if (isThrow) throw new Exception($"不可更新，未设置主键的值：{_db.Orm.GetEntityString(_entityType, data)}");
                return false;
            }
            if (_states.TryGetValue(key, out var tryval) == false)
            {
                if (isThrow) throw new Exception($"不可更新，数据未被跟踪，应该先查询 或者 Attach：{_db.Orm.GetEntityString(_entityType, data)}");
                return false;
            }
            return true;
        }

        bool CanRemove(IEnumerable<TEntity> data, bool isThrow)
        {
            if (data == null)
            {
                if (isThrow) throw new ArgumentNullException(nameof(data));
                return false;
            }
            if (data.Any() == false) return false;
            foreach (var s in data) if (CanRemove(s, isThrow) == false) return false;
            return true;
        }
        bool CanRemove(TEntity data, bool isThrow)
        {
            if (data == null)
            {
                if (isThrow) throw new ArgumentNullException(nameof(data));
                return false;
            }
            if (_table.Primarys.Any() == false)
            {
                if (isThrow) throw new Exception($"不可删除，实体没有主键：{_db.Orm.GetEntityString(_entityType, data)}");
                return false;
            }
            var key = _db.Orm.GetEntityKeyString(_entityType, data, false);
            if (string.IsNullOrEmpty(key))
            {
                if (isThrow) throw new Exception($"不可删除，未设置主键的值：{_db.Orm.GetEntityString(_entityType, data)}");
                return false;
            }
            //if (_states.TryGetValue(key, out var tryval) == false) {
            //	if (isThrow) throw new Exception($"不可删除，数据未被跟踪，应该先查询：{_fsql.GetEntityString(_entityType, data)}");
            //	return false;
            //}
            return true;
        }
        #endregion
    }
}
