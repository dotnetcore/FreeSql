using FreeSql.Extensions.EntityUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#if net40
#else
namespace FreeSql
{
    partial class BaseRepository<TEntity>
        where TEntity : class
    {

        async public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var delete = _dbset.OrmDeleteInternal(null).Where(predicate);
            var sql = delete.ToSql();
            var affrows = await delete.ExecuteAffrowsAsync();
            _db._entityChangeReport.Add(new DbContext.EntityChangeReport.ChangeInfo { Object = sql, Type = DbContext.EntityChangeType.SqlRaw });
            return affrows;
        }

        public Task<int> DeleteAsync(TEntity entity)
        {
            _dbset.Remove(entity);
            return _db.SaveChangesAsync();
        }
        public Task<int> DeleteAsync(IEnumerable<TEntity> entitys)
        {
            _dbset.RemoveRange(entitys);
            return _db.SaveChangesAsync();
        }

        async public virtual Task<TEntity> InsertAsync(TEntity entity)
        {
            await _dbset.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
        async public virtual Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys)
        {
            await _dbset.AddRangeAsync(entitys);
            await _db.SaveChangesAsync();
            return entitys.ToList();
        }

        public Task<int> UpdateAsync(TEntity entity)
        {
            _dbset.Update(entity);
            return _db.SaveChangesAsync();
        }
        public Task<int> UpdateAsync(IEnumerable<TEntity> entitys)
        {
            _dbset.UpdateRange(entitys);
            return _db.SaveChangesAsync();
        }

        async public Task<TEntity> InsertOrUpdateAsync(TEntity entity)
        {
            await _dbset.AddOrUpdateAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        async public Task SaveManyAsync(TEntity entity, string propertyName)
        {
            await _dbset.SaveManyAsync(entity, propertyName);
            await _db.SaveChangesAsync();
        }
    }

    partial class BaseRepository<TEntity, TKey>
    {

        public Task<int> DeleteAsync(TKey id) => DeleteAsync(CheckTKeyAndReturnIdEntity(id));

        public Task<TEntity> FindAsync(TKey id) => _dbset.OrmSelectInternal(CheckTKeyAndReturnIdEntity(id)).ToOneAsync();

        public Task<TEntity> GetAsync(TKey id) => FindAsync(id);
    }
}
#endif