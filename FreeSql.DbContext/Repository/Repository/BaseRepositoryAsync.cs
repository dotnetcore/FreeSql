using FreeSql.Extensions.EntityUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

#if net40
#else
namespace FreeSql
{
    partial class BaseRepository<TEntity>
        where TEntity : class
    {

        async virtual public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var delete = _dbset.OrmDeleteInternal(null).Where(predicate);
            var sql = delete.ToSql();
            var affrows = await delete.ExecuteAffrowsAsync(cancellationToken);
            _db._entityChangeReport.Add(new DbContext.EntityChangeReport.ChangeInfo { Object = sql, Type = DbContext.EntityChangeType.SqlRaw });
            return affrows;
        }
        public virtual Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _dbset.Remove(entity);
            return _db.SaveChangesAsync(cancellationToken);
        }
        public virtual Task<int> DeleteAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default)
        {
            _dbset.RemoveRange(entitys);
            return _db.SaveChangesAsync(cancellationToken);
        }

        async public virtual Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _dbset.AddAsync(entity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return entity;
        }
        async public virtual Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default)
        {
            await _dbset.AddRangeAsync(entitys, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return entitys.ToList();
        }

        public virtual Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _dbset.Update(entity);
            return _db.SaveChangesAsync(cancellationToken);
        }
        public virtual Task<int> UpdateAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default)
        {
            _dbset.UpdateRange(entitys);
            return _db.SaveChangesAsync(cancellationToken);
        }

        async public virtual Task<TEntity> InsertOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _dbset.AddOrUpdateAsync(entity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return entity;
        }

        async public Task SaveManyAsync(TEntity entity, string propertyName, CancellationToken cancellationToken = default)
        {
            await _dbset.SaveManyAsync(entity, propertyName, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    partial class BaseRepository<TEntity, TKey>
    {
        public virtual Task<int> DeleteAsync(TKey id, CancellationToken cancellationToken = default) => DeleteAsync(CheckTKeyAndReturnIdEntity(id), cancellationToken);
        public virtual Task<TEntity> FindAsync(TKey id, CancellationToken cancellationToken = default) => _dbset.OrmSelectInternal(CheckTKeyAndReturnIdEntity(id)).ToOneAsync(cancellationToken);
        public Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default) => _dbset.OrmSelectInternal(CheckTKeyAndReturnIdEntity(id)).ToOneAsync(cancellationToken);
    }
}
#endif