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
        public Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default) => _repository.InsertAsync(entity, cancellationToken);
        public Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default) => _repository.InsertAsync(entitys, cancellationToken);
        public Task<TEntity> InsertOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default) => _repository.InsertOrUpdateAsync(entity, cancellationToken);
        public Task SaveManyAsync(TEntity entity, string propertyName, CancellationToken cancellationToken = default) => _repository.SaveManyAsync(entity, propertyName, cancellationToken);

        public Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) => _repository.UpdateAsync(entity, cancellationToken);
        public Task<int> UpdateAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default) => _repository.UpdateAsync(entitys, cancellationToken);

        public Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default) => _repository.DeleteAsync(entity, cancellationToken);
        public Task<int> DeleteAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default) => _repository.DeleteAsync(entitys, cancellationToken);
        public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) => _repository.DeleteAsync(predicate, cancellationToken);
        public Task<List<object>> DeleteCascadeByDatabaseAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) => _repository.DeleteCascadeByDatabaseAsync(predicate, cancellationToken);

    }
}
#endif