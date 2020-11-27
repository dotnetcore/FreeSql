using FreeSql.Extensions.EntityUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{
    public abstract partial class BaseRepository<TEntity> : IBaseRepository<TEntity>
        where TEntity : class
    {

        internal RepositoryDbContext _dbPriv; //这个不能私有化，有地方需要反射获取它
        internal RepositoryDbContext _db => _dbPriv ?? (_dbPriv = new RepositoryDbContext(OrmOriginal, this));
        internal RepositoryDbSet<TEntity> _dbsetPriv;
        internal RepositoryDbSet<TEntity> _dbset => _dbsetPriv ?? (_dbsetPriv = _db.Set<TEntity>() as RepositoryDbSet<TEntity>);

        public IDataFilter<TEntity> DataFilter { get; } = new DataFilter<TEntity>();
        internal Func<string, string> AsTableValueInternal { get; private set; }
        internal Func<Type, string, string> AsTableSelectValueInternal { get; private set; }

        protected void ApplyDataFilter(string name, Expression<Func<TEntity, bool>> exp) => DataFilter.Apply(name, exp);

        protected BaseRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter, Func<string, string> asTable = null)
        {
            _ormScoped = DbContextScopedFreeSql.Create(fsql, () => _db, () => UnitOfWork);
            DataFilterUtil.SetRepositoryDataFilter(this, null);
            DataFilter.Apply("", filter);
            AsTable(asTable);
        }

        ~BaseRepository() => this.Dispose();
        int _disposeCounter;
        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            try
            {
                _dbsetPriv?.Dispose();
                _dbPriv?.Dispose();
                this.DataFilter.Dispose();
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }
        public Type EntityType => _dbsetPriv?.EntityType ?? typeof(TEntity);
        public void AsType(Type entityType) => _dbset.AsType(entityType);
        public void AsTable(Func<string, string> rule)
        {
            AsTableValueInternal = rule;
            AsTableSelectValueInternal = rule == null ? null : new Func<Type, string, string>((a, b) => a == EntityType ? rule(b) : null);
        }
        public DbContextOptions DbContextOptions { get => _db.Options; set => _db.Options = value; }

        internal DbContextScopedFreeSql _ormScoped;
        internal IFreeSql OrmOriginal => _ormScoped?._originalFsql;
        public IFreeSql Orm => _ormScoped;
        IUnitOfWork _unitOfWork;
        public IUnitOfWork UnitOfWork
        {
            set
            {
                _unitOfWork = value;
                if (_dbsetPriv != null) _dbsetPriv._uow = _unitOfWork; //防止 dbset 对象已经存在，再次设置 UnitOfWork 无法生效，所以作此判断重新设置
                if (_dbPriv != null) _dbPriv.UnitOfWork = _unitOfWork;
            }
            get => _unitOfWork;
        }
        public IUpdate<TEntity> UpdateDiy => _dbset.OrmUpdateInternal(null);

        public virtual ISelect<TEntity> Select => _dbset.OrmSelectInternal(null);
        public ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => Select.Where(exp);
        public ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => Select.WhereIf(condition, exp);

        public virtual int Delete(Expression<Func<TEntity, bool>> predicate)
        {
            var delete = _dbset.OrmDeleteInternal(null).Where(predicate);
            var sql = delete.ToSql();
            var affrows = delete.ExecuteAffrows();
            _db._entityChangeReport.Add(new DbContext.EntityChangeReport.ChangeInfo { Object = sql, Type = DbContext.EntityChangeType.SqlRaw });
            return affrows;
        }
        public virtual int Delete(TEntity entity)
        {
            _dbset.Remove(entity);
            return _db.SaveChanges();
        }
        public virtual int Delete(IEnumerable<TEntity> entitys)
        {
            _dbset.RemoveRange(entitys);
            return _db.SaveChanges();
        }

        public virtual TEntity Insert(TEntity entity)
        {
            _dbset.Add(entity);
            _db.SaveChanges();
            return entity;
        }
        public virtual List<TEntity> Insert(IEnumerable<TEntity> entitys)
        {
            _dbset.AddRange(entitys);
            _db.SaveChanges();
            return entitys.ToList();
        }

        public virtual int Update(TEntity entity)
        {
            _dbset.Update(entity);
            return _db.SaveChanges();
        }
        public virtual int Update(IEnumerable<TEntity> entitys)
        {
            _dbset.UpdateRange(entitys);
            return _db.SaveChanges();
        }

        public void Attach(TEntity data) => _dbset.Attach(data);
        public void Attach(IEnumerable<TEntity> data) => _dbset.AttachRange(data);
        public IBaseRepository<TEntity> AttachOnlyPrimary(TEntity data)
        {
            _dbset.AttachOnlyPrimary(data);
            return this;
        }
        public void FlushState() => _dbset.FlushState();

        public virtual TEntity InsertOrUpdate(TEntity entity)
        {
            _dbset.AddOrUpdate(entity);
            _db.SaveChanges();
            return entity;
        }

        public void SaveMany(TEntity entity, string propertyName)
        {
            _dbset.SaveMany(entity, propertyName);
            _db.SaveChanges();
        }

        public void BeginEdit(List<TEntity> data) => _dbset.BeginEdit(data);
        public int EndEdit(List<TEntity> data = null)
        {
            _db.FlushCommand();
            if (UnitOfWork?.GetOrBeginTransaction(true) == null && _db.OrmOriginal.Ado.TransactionCurrentThread == null)
            {
                int affrows = 0;
                IUnitOfWork olduow = UnitOfWork;
                UnitOfWork = new UnitOfWork(_db.OrmOriginal);
                try
                {
                    affrows = _dbset.EndEdit(data);
                    UnitOfWork.Commit();
                }
                catch
                {
                    UnitOfWork.Rollback();
                    throw;
                }
                finally
                {
                    UnitOfWork.Dispose();
                    UnitOfWork = olduow;
                }
                return affrows;
            }
            return _dbset.EndEdit(data);
        }
    }

    public abstract partial class BaseRepository<TEntity, TKey> : BaseRepository<TEntity>, IBaseRepository<TEntity, TKey>
        where TEntity : class
    {
        public BaseRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter, Func<string, string> asTable = null) : base(fsql, filter, asTable) { }

        TEntity CheckTKeyAndReturnIdEntity(TKey id)
        {
            var tb = _db.OrmOriginal.CodeFirst.GetTableByEntity(EntityType);
            if (tb.Primarys.Length != 1) throw new Exception($"实体类型 {EntityType.Name} 主键数量不为 1，无法使用该方法");
            if (tb.Primarys[0].CsType.NullableTypeOrThis() != typeof(TKey).NullableTypeOrThis()) throw new Exception($"实体类型 {EntityType.Name} 主键类型不为 {typeof(TKey).FullName}，无法使用该方法");
            var obj = Activator.CreateInstance(tb.Type);
            _db.OrmOriginal.SetEntityValueWithPropertyName(tb.Type, obj, tb.Primarys[0].CsName, id);
            var  ret = obj as TEntity;
            if (ret == null) throw new Exception($"实体类型 {EntityType.Name} 无法转换为 {typeof(TEntity).Name}，无法使用该方法");
            return ret;
        }

        public virtual int Delete(TKey id) => Delete(CheckTKeyAndReturnIdEntity(id));
        public virtual TEntity Find(TKey id) => _dbset.OrmSelectInternal(CheckTKeyAndReturnIdEntity(id)).ToOne();
        public TEntity Get(TKey id) => _dbset.OrmSelectInternal(CheckTKeyAndReturnIdEntity(id)).ToOne();
    }
}
