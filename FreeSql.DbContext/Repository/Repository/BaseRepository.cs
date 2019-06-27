using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql
{
    public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity>
        where TEntity : class
    {

        internal RepositoryDbContext _dbPriv;
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

        ~BaseRepository()
        {
            this.Dispose();
        }
        bool _isdisposed = false;
        public void Dispose()
        {
            if (_isdisposed) return;
            try
            {
                _dbsetPriv?.Dispose();
                _dbPriv?.Dispose();
                this.DataFilter.Dispose();
            }
            finally
            {
                _isdisposed = true;
                GC.SuppressFinalize(this);
            }
        }
        public Type EntityType => _dbsetPriv?.EntityType ?? typeof(TEntity);
        public void AsType(Type entityType) => _dbset.AsType(entityType);

        public IFreeSql Orm { get; private set; }
        public IUnitOfWork UnitOfWork { get; set; }
        public IUpdate<TEntity> UpdateDiy => _dbset.OrmUpdateInternal(null);

        public ISelect<TEntity> Select => _dbset.OrmSelectInternal(null);
        public ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => _dbset.OrmSelectInternal(null).Where(exp);
        public ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => _dbset.OrmSelectInternal(null).WhereIf(condition, exp);

        public int Delete(Expression<Func<TEntity, bool>> predicate) => _dbset.OrmDeleteInternal(null).Where(predicate).ExecuteAffrows();
        public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate) => _dbset.OrmDeleteInternal(null).Where(predicate).ExecuteAffrowsAsync();

        public int Delete(TEntity entity)
        {
            _dbset.Remove(entity);
            return _db.SaveChanges();
        }
        public Task<int> DeleteAsync(TEntity entity)
        {
            _dbset.Remove(entity);
            return _db.SaveChangesAsync();
        }
        public int Delete(IEnumerable<TEntity> entitys)
        {
            _dbset.RemoveRange(entitys);
            return _db.SaveChanges();
        }
        public Task<int> DeleteAsync(IEnumerable<TEntity> entitys)
        {
            _dbset.RemoveRange(entitys);
            return _db.SaveChangesAsync();
        }

        public virtual TEntity Insert(TEntity entity)
        {
            _dbset.Add(entity);
            _db.SaveChanges();
            return entity;
        }
        async public virtual Task<TEntity> InsertAsync(TEntity entity)
        {
            await _dbset.AddAsync(entity);
            _db.SaveChanges();
            return entity;
        }
        public virtual List<TEntity> Insert(IEnumerable<TEntity> entitys)
        {
            _dbset.AddRange(entitys);
            _db.SaveChanges();
            return entitys.ToList();
        }
        async public virtual Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys)
        {
            await _dbset.AddRangeAsync(entitys);
            await _db.SaveChangesAsync();
            return entitys.ToList();
        }

        public int Update(TEntity entity)
        {
            _dbset.Update(entity);
            return _db.SaveChanges();
        }
        public Task<int> UpdateAsync(TEntity entity)
        {
            _dbset.Update(entity);
            return _db.SaveChangesAsync();
        }
        public int Update(IEnumerable<TEntity> entitys)
        {
            _dbset.UpdateRange(entitys);
            return _db.SaveChanges();
        }
        public Task<int> UpdateAsync(IEnumerable<TEntity> entitys)
        {
            _dbset.UpdateRange(entitys);
            return _db.SaveChangesAsync();
        }

        public void Attach(TEntity data) => _db.Attach(data);
        public void Attach(IEnumerable<TEntity> data) => _db.AttachRange(data);
        public void FlushState() => _dbset.FlushState();

        public TEntity InsertOrUpdate(TEntity entity)
        {
            _dbset.AddOrUpdate(entity);
            _db.SaveChanges();
            return entity;
        }
        async public Task<TEntity> InsertOrUpdateAsync(TEntity entity)
        {
            await _dbset.AddOrUpdateAsync(entity);
            _db.SaveChanges();
            return entity;
        }
    }

    public abstract class BaseRepository<TEntity, TKey> : BaseRepository<TEntity>, IBaseRepository<TEntity, TKey>
        where TEntity : class
    {

        public BaseRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter, Func<string, string> asTable = null) : base(fsql, filter, asTable)
        {
        }

        public int Delete(TKey id)
        {
            var stateKey = string.Concat(id);
            _dbset._statesInternal.TryRemove(stateKey, out var trystate);
            return _dbset.OrmDeleteInternal(id).ExecuteAffrows();
        }
        public Task<int> DeleteAsync(TKey id)
        {
            var stateKey = string.Concat(id);
            _dbset._statesInternal.TryRemove(stateKey, out var trystate);
            return _dbset.OrmDeleteInternal(id).ExecuteAffrowsAsync();
        }

        public TEntity Find(TKey id) => _dbset.OrmSelectInternal(id).ToOne();
        public Task<TEntity> FindAsync(TKey id) => _dbset.OrmSelectInternal(id).ToOneAsync();

        public TEntity Get(TKey id) => Find(id);
        public Task<TEntity> GetAsync(TKey id) => FindAsync(id);
    }
}
