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
using FreeSql.Internal.CommonProvider;

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
            DbContextFlushCommand(); //查询前先提交，否则会出脏读
            var select = _db.OrmOriginal.Select<TEntity>().AsType(_entityType).WithTransaction(_uow?.GetOrBeginTransaction(false)).TrackToList(TrackToList).WhereDynamic(dywhere);
            if (_db.Options.EnableGlobalFilter == false) select.DisableGlobalFilter();
            return select;
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
                this._statesEditing.Clear();
                this._dataEditing = null;
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        protected virtual IInsert<TEntity> OrmInsert()
        {
            var insert = _db.OrmOriginal.Insert<TEntity>().AsType(_entityType).WithTransaction(_uow?.GetOrBeginTransaction());
            if (_db.Options.NoneParameter != null) insert.NoneParameter(_db.Options.NoneParameter.Value);
            return insert;
        }
        protected virtual IInsert<TEntity> OrmInsert(TEntity data)
        {
            var insert = OrmInsert();
            if (data != null) (insert as InsertProvider<TEntity>)._source.Add(data); //防止 Aop.AuditValue 触发两次
            return insert;
        }
        protected virtual IInsert<TEntity> OrmInsert(IEnumerable<TEntity> data)
        {
            var insert = OrmInsert();
            if (data != null) (insert as InsertProvider<TEntity>)._source.AddRange(data.Where(a => a != null)); //防止 Aop.AuditValue 触发两次
            return insert;
        }

        protected virtual IUpdate<TEntity> OrmUpdate(IEnumerable<TEntity> entitys)
        {
            var update = _db.OrmOriginal.Update<TEntity>().AsType(_entityType).WithTransaction(_uow?.GetOrBeginTransaction());
            if (_db.Options.NoneParameter != null) update.NoneParameter(_db.Options.NoneParameter.Value);
            if (_db.Options.EnableGlobalFilter == false) update.DisableGlobalFilter();
            if (entitys != null) (update as UpdateProvider<TEntity>)._source.AddRange(entitys.Where(a => a != null)); //防止 Aop.AuditValue 触发两次
            return update;
        }
        protected virtual IDelete<TEntity> OrmDelete(object dywhere)
        {
            var delete = _db.OrmOriginal.Delete<TEntity>().AsType(_entityType).WithTransaction(_uow?.GetOrBeginTransaction()).WhereDynamic(dywhere);
            if (_db.Options.EnableGlobalFilter == false) delete.DisableGlobalFilter();
            return delete;
        }

        internal void EnqueueToDbContext(DbContext.EntityChangeType changeType, EntityState state) =>
            _db.EnqueuePreCommand(changeType, this, typeof(EntityState), _entityType, state);

        internal void IncrAffrows(int affrows) =>
            _db._affrows += affrows;

        internal void TrackToList(object list)
        {
            if (list == null) return;
            var ls = list as IEnumerable<TEntity>;
            if (ls == null)
            {
                var ie = list as IEnumerable;
                if (ie == null) return;
                foreach (var item in ie)
                {
                    if (item == null) return;
                    var itemType = item.GetType();
                    if (itemType == typeof(object)) return;
                    if (itemType.FullName.Contains("FreeSqlLazyEntity__")) itemType = itemType.BaseType;
                    if (_db.OrmOriginal.CodeFirst.GetTableByEntity(itemType)?.Primarys.Any() != true) return;
                    if (itemType.GetConstructor(Type.EmptyTypes) == null) return;
                    var dbset = _db.Set(itemType);
                    dbset?.GetType().GetMethod("TrackToList", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(dbset, new object[] { list });
                    return;
                }
                return;
            }
            if (_table?.Primarys.Any() != true) return;
            foreach (var item in ls)
            {
                var key = _db.OrmOriginal.GetEntityKeyString(_entityType, item, false);
                if (key == null) continue;
                _states.AddOrUpdate(key, k => CreateEntityState(item), (k, ov) =>
                {
                    _db.OrmOriginal.MapEntityValue(_entityType, item, ov.Value);
                    ov.Time = DateTime.Now;
                    return ov;
                });
            }
        }

        public ISelect<TEntity> Select => this.OrmSelect(null);
        public ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => this.OrmSelect(null).Where(exp);
        public ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => this.OrmSelect(null).WhereIf(condition, exp);

        protected ConcurrentDictionary<string, EntityState> _states = new ConcurrentDictionary<string, EntityState>();
        TableInfo _tablePriv;
        protected TableInfo _table => _tablePriv ?? (_tablePriv = _db.OrmOriginal.CodeFirst.GetTableByEntity(_entityType));
        ColumnInfo[] _tableIdentitysPriv, _tableReturnColumnsPriv;
        protected ColumnInfo[] _tableIdentitys => _tableIdentitysPriv ?? (_tableIdentitysPriv = _table.ColumnsByPosition.Where(a => a.Attribute.IsIdentity).ToArray());
        protected ColumnInfo[] _tableReturnColumns => _tableReturnColumnsPriv ?? (_tableReturnColumnsPriv = _table.ColumnsByPosition.Where(a => a.Attribute.IsIdentity || string.IsNullOrWhiteSpace(a.DbInsertValue) == false).ToArray());
        protected Type _entityType = typeof(TEntity);
        public Type EntityType => _entityType;

        /// <summary>
        /// 动态Type，在使用 DbSet&lt;object&gt; 后使用本方法，指定实体类型
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public DbSet<TEntity> AsType(Type entityType)
        {
            if (entityType == typeof(object)) throw new Exception(CoreStrings.TypeAsType_NotSupport_Object("DbSet"));
            if (entityType == _entityType) return this;
            var newtb = _db.OrmOriginal.CodeFirst.GetTableByEntity(entityType);
            _tablePriv = newtb ?? throw new Exception(CoreStrings.Type_AsType_Parameter_Error("DbSet"));
            _tableIdentitysPriv = null;
            _tableReturnColumnsPriv = null;
            _entityType = entityType;
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
        void StatesRemoveByObjects(IEnumerable<object> data)
        {
            if (data == null) return;
            foreach (var item in data)
            {
                var stateKey = _db.OrmOriginal.GetEntityKeyString(_entityType, item, false);
                _states.TryRemove(stateKey, out var trystate);
            }
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
            if (_table.Primarys.Any() == false) throw new Exception(DbContextStrings.CannotAttach_EntityHasNo_PrimaryKey(_db.OrmOriginal.GetEntityString(_entityType, data.First())));
            foreach (var item in data)
            {
                var key = _db.OrmOriginal.GetEntityKeyString(_entityType, item, false);
                if (string.IsNullOrEmpty(key)) throw new Exception(DbContextStrings.CannotAttach_PrimaryKey_NotSet(_db.OrmOriginal.GetEntityString(_entityType, item)));

                _states.AddOrUpdate(key, k => CreateEntityState(item), (k, ov) =>
                {
                    _db.OrmOriginal.MapEntityValue(_entityType, item, ov.Value);
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
            var pkitem = (TEntity)_entityType.CreateInstanceGetDefaultValue();
            foreach (var pk in _db.OrmOriginal.CodeFirst.GetTableByEntity(_entityType).Primarys)
            {
                var colVal = _db.OrmOriginal.GetEntityValueWithPropertyName(_entityType, data, pk.CsName);
                _db.OrmOriginal.SetEntityValueWithPropertyName(_entityType, pkitem, pk.CsName, colVal);
            }
            this.Attach(pkitem);
            return this;
        }

        /// <summary>
        /// 比较实体，计算出值发生变化的属性，以及属性变化的前后值
        /// </summary>
        /// <param name="newdata">最新的实体对象，它将与附加实体的状态对比</param>
        /// <returns>key: 属性名, value: [旧值, 新值]</returns>
        public Dictionary<string, object[]> CompareState(TEntity newdata)
        {
            if (newdata == null) return null;
            if (_table.Primarys.Any() == false) throw new Exception(DbContextStrings.Incomparable_EntityHasNo_PrimaryKey(_db.OrmOriginal.GetEntityString(_entityType, newdata)));
            var key = _db.OrmOriginal.GetEntityKeyString(_entityType, newdata, false);
            if (string.IsNullOrEmpty(key)) throw new Exception(DbContextStrings.Incomparable_PrimaryKey_NotSet(_db.OrmOriginal.GetEntityString(_entityType, newdata)));
            if (_states.TryGetValue(key, out var oldState) == false || oldState == null)
                return _table.ColumnsByCs.ToDictionary(a => a.Key, a => new object[]
                {
                    _db.OrmOriginal.GetEntityValueWithPropertyName(_entityType, newdata, a.Key),
                    null
                });

            return _db.OrmOriginal.CompareEntityValueReturnColumns(_entityType, oldState.Value, newdata, false).ToDictionary(a => a, a => new object[]
            {
                _db.OrmOriginal.GetEntityValueWithPropertyName(_entityType, newdata, a),
                _db.OrmOriginal.GetEntityValueWithPropertyName(_entityType, oldState.Value, a)
            });
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
            var key = _db.OrmOriginal.GetEntityKeyString(_entityType, data, false);
            var state = new EntityState((TEntity)_entityType.CreateInstanceGetDefaultValue(), key);
           _db.OrmOriginal.MapEntityValue(_entityType, data, state.Value);
            return state;
        }
        bool? ExistsInStates(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var key = _db.OrmOriginal.GetEntityKeyString(_entityType, data, false);
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
                if (isThrow) throw new Exception(DbContextStrings.CannotAdd_EntityHasNo_PrimaryKey(_db.OrmOriginal.GetEntityString(_entityType, data)));
                return false;
            }
            FreeSql.Internal.CommonProvider.InsertProvider<TEntity>.AuditDataValue(this, data, _db.OrmOriginal, _table, null);
            var key = _db.OrmOriginal.GetEntityKeyString(_entityType, data, true);
            if (string.IsNullOrEmpty(key))
            {
                switch (_db.OrmOriginal.Ado.DataType)
                {
                    case DataType.SqlServer:
                    case DataType.OdbcSqlServer:
                    case DataType.PostgreSQL:
                    case DataType.OdbcPostgreSQL:
                    case DataType.KingbaseES:
                    case DataType.OdbcKingbaseES:
                    case DataType.ShenTong:
                    case DataType.ClickHouse:
                        return true;
                    default:
                        if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1)
                            return true;
                        if (isThrow) throw new Exception(DbContextStrings.CannotAdd_PrimaryKey_NotSet(_db.OrmOriginal.GetEntityString(_entityType, data)));
                        return false;
                }
            }
            else
            {
                if (_states.ContainsKey(key))
                {
                    if (isThrow) throw new Exception(DbContextStrings.CannotAdd_AlreadyExistsInStateManagement(_db.OrmOriginal.GetEntityString(_entityType, data)));
                    return false;
                }
                if (_db.OrmOriginal.Ado.DataType == DataType.ClickHouse) return true;
                var idval = _db.OrmOriginal.GetEntityIdentityValueWithPrimary(_entityType, data);
                if (idval > 0)
                {
                    if (isThrow) throw new Exception(DbContextStrings.CannotAdd_SelfIncreasingHasValue(_db.OrmOriginal.GetEntityString(_entityType, data)));
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
                if (isThrow) throw new Exception(DbContextStrings.CannotUpdate_EntityHasNo_PrimaryKey(_db.OrmOriginal.GetEntityString(_entityType, data)));
                return false;
            }
            FreeSql.Internal.CommonProvider.UpdateProvider<TEntity>.AuditDataValue(this, data, _db.OrmOriginal, _table, null);
            var key = _db.OrmOriginal.GetEntityKeyString(_entityType, data, false);
            if (string.IsNullOrEmpty(key))
            {
                if (isThrow) throw new Exception(DbContextStrings.CannotUpdate_PrimaryKey_NotSet(_db.OrmOriginal.GetEntityString(_entityType, data)));
                return false;
            }
            if (_states.TryGetValue(key, out var tryval) == false)
            {
                if (isThrow) throw new Exception(DbContextStrings.CannotUpdate_DataShouldQueryOrAttach(_db.OrmOriginal.GetEntityString(_entityType, data)));
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
                if (isThrow) throw new Exception(DbContextStrings.CannotDelete_EntityHasNo_PrimaryKey(_db.OrmOriginal.GetEntityString(_entityType, data)));
                return false;
            }
            var key = _db.OrmOriginal.GetEntityKeyString(_entityType, data, false);
            if (string.IsNullOrEmpty(key))
            {
                if (isThrow) throw new Exception(DbContextStrings.CannotDelete_PrimaryKey_NotSet(_db.OrmOriginal.GetEntityString(_entityType, data)));
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
