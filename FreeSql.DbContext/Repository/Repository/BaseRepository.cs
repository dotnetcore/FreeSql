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
        internal RepositoryDbContext _db => _dbPriv ?? (_dbPriv = new RepositoryDbContext(Orm, this));
        internal RepositoryDbSet<TEntity> _dbsetPriv;
        internal RepositoryDbSet<TEntity> _dbset => _dbsetPriv ?? (_dbsetPriv = _db.Set<TEntity>() as RepositoryDbSet<TEntity>);

        public IDataFilter<TEntity> DataFilter { get; } = new DataFilter<TEntity>();
        Func<string, string> _asTableVal;
        protected Func<string, string> AsTable
        {
            get => _asTableVal;
            set
            {
                _asTableVal = value;
                AsTableSelect = value == null ? null : new Func<Type, string, string>((a, b) => a == EntityType ? value(b) : null);
            }
        }
        internal Func<string, string> AsTableInternal => AsTable;
        protected Func<Type, string, string> AsTableSelect { get; private set; }
        internal Func<Type, string, string> AsTableSelectInternal => AsTableSelect;

        protected void ApplyDataFilter(string name, Expression<Func<TEntity, bool>> exp) => DataFilter.Apply(name, exp);

        protected BaseRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter, Func<string, string> asTable = null)
        {
            Orm = fsql;
            DataFilterUtil.SetRepositoryDataFilter(this, null);
            DataFilter.Apply("", filter);
            AsTable = asTable;
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
        public DbContextOptions DbContextOptions { get => _db.Options; set => _db.Options = value; }

        public IFreeSql Orm { get; private set; }
        IUnitOfWork _unitOfWork;
        public IUnitOfWork UnitOfWork
        {
            set
            {
                _unitOfWork = value;
                if (_dbsetPriv != null) _dbsetPriv._uow = _unitOfWork; //防止 dbset 对象已经存在，再次设置 UnitOfWork 无法生效，所以作此判断重新设置
            }
            get => _unitOfWork;
        }
        public IUpdate<TEntity> UpdateDiy => _dbset.OrmUpdateInternal(null);

        public ISelect<TEntity> Select => _dbset.OrmSelectInternal(null);
        public ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => _dbset.OrmSelectInternal(null).Where(exp);
        public ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => _dbset.OrmSelectInternal(null).WhereIf(condition, exp);

        public int Delete(Expression<Func<TEntity, bool>> predicate)
        {
            var delete = _dbset.OrmDeleteInternal(null).Where(predicate);
            var sql = delete.ToSql();
            var affrows = delete.ExecuteAffrows();
            _db._entityChangeReport.Add(new DbContext.EntityChangeReport.ChangeInfo { Object = sql, Type = DbContext.EntityChangeType.SqlRaw });
            return affrows;
        }

        public int Delete(TEntity entity)
        {
            _dbset.Remove(entity);
            return _db.SaveChanges();
        }
        public int Delete(IEnumerable<TEntity> entitys)
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

        public int Update(TEntity entity)
        {
            _dbset.Update(entity);
            return _db.SaveChanges();
        }
        public int Update(IEnumerable<TEntity> entitys)
        {
            _dbset.UpdateRange(entitys);
            return _db.SaveChanges();
        }

        public void Attach(TEntity data) => _db.Attach(data);
        public void Attach(IEnumerable<TEntity> data) => _db.AttachRange(data);
        public IBasicRepository<TEntity> AttachOnlyPrimary(TEntity data)
        {
            _db.AttachOnlyPrimary(data);
            return this;
        }
        public void FlushState() => _dbset.FlushState();

        public TEntity InsertOrUpdate(TEntity entity)
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
    }

    public abstract partial class BaseRepository<TEntity, TKey> : BaseRepository<TEntity>, IBaseRepository<TEntity, TKey>
        where TEntity : class
    {

        public BaseRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter, Func<string, string> asTable = null) : base(fsql, filter, asTable)
        {
        }

        TEntity CheckTKeyAndReturnIdEntity(TKey id)
        {
            var tb = _db.Orm.CodeFirst.GetTableByEntity(EntityType);
            if (tb.Primarys.Length != 1) throw new Exception($"实体类型 {EntityType.Name} 主键数量不为 1，无法使用该方法");
            if (tb.Primarys[0].CsType.NullableTypeOrThis() != typeof(TKey).NullableTypeOrThis()) throw new Exception($"实体类型 {EntityType.Name} 主键类型不为 {typeof(TKey).FullName}，无法使用该方法");
            var obj = Activator.CreateInstance(tb.Type);
            _db.Orm.SetEntityValueWithPropertyName(tb.Type, obj, tb.Primarys[0].CsName, id);
            var  ret = obj as TEntity;
            if (ret == null) throw new Exception($"实体类型 {EntityType.Name} 无法转换为 {typeof(TEntity).Name}，无法使用该方法");
            return ret;
        }

        public int Delete(TKey id) => Delete(CheckTKeyAndReturnIdEntity(id));

        public TEntity Find(TKey id) => _dbset.OrmSelectInternal(CheckTKeyAndReturnIdEntity(id)).ToOne();

        public TEntity Get(TKey id) => Find(id);
    }
}
