#if net40
#else
using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{
    partial class AggregateRootRepository<TEntity>
    {
        //以下临时调用同步方法
        public Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default) => Task.FromResult(Insert(entity));
        public Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default) => Task.FromResult(Insert(entitys));
        public Task<TEntity> InsertOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default) => Task.FromResult(InsertOrUpdate(entity));
        async public Task SaveManyAsync(TEntity entity, string propertyName, CancellationToken cancellationToken = default)
        {
            SaveMany(entity, propertyName);
        }

        public Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) => Task.FromResult(Update(entity));
        public Task<int> UpdateAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default) => Task.FromResult(Update(entitys));

        public Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default) => Task.FromResult(Delete(entity));
        public Task<int> DeleteAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default) => Task.FromResult(Delete(entitys));
        public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) => Task.FromResult(Delete(predicate));
        public Task<List<object>> DeleteCascadeByDatabaseAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) => Task.FromResult(DeleteCascadeByDatabase(predicate));

    }
}
#endif