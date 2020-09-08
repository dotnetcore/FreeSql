using FreeSql.Extensions.EntityUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeSql
{
    internal class RepositoryDbSet<TEntity> : DbSet<TEntity> where TEntity : class
    {

        protected BaseRepository<TEntity> _repo;
        public RepositoryDbSet(BaseRepository<TEntity> repo)
        {
            _db = repo._db;
            _uow = repo.UnitOfWork;
            _repo = repo;
        }

        protected override ISelect<TEntity> OrmSelect(object dywhere)
        {
            var select = base.OrmSelect(dywhere);

            var filters = (_repo.DataFilter as DataFilter<TEntity>)._filters.Where(a => a.Value.IsEnabled == true);
            foreach (var filter in filters) select.Where(filter.Value.Expression);
            return select.AsTable(_repo.AsTableSelectValueInternal);
        }
        internal ISelect<TEntity> OrmSelectInternal(object dywhere) => OrmSelect(dywhere);
        protected override IUpdate<TEntity> OrmUpdate(IEnumerable<TEntity> entitys)
        {
            var update = base.OrmUpdate(entitys);
            var filters = (_repo.DataFilter as DataFilter<TEntity>)._filters.Where(a => a.Value.IsEnabled == true);
            foreach (var filter in filters)
            {
                if (entitys != null)
                    foreach (var entity in entitys)
                        if (filter.Value.ExpressionDelegate?.Invoke(entity) == false)
                            throw new Exception($"FreeSql.Repository Update 失败，因为设置了过滤器 {filter.Key}: {filter.Value.Expression}，更新的数据不符合 {_db.OrmOriginal.GetEntityString(_entityType, entity)}");
                update.Where(filter.Value.Expression);
            }
            return update.AsTable(_repo.AsTableValueInternal);
        }
        internal IUpdate<TEntity> OrmUpdateInternal(IEnumerable<TEntity> entitys) => OrmUpdate(entitys);
        protected override IDelete<TEntity> OrmDelete(object dywhere)
        {
            var delete = base.OrmDelete(dywhere);
            var filters = (_repo.DataFilter as DataFilter<TEntity>)._filters.Where(a => a.Value.IsEnabled == true);
            foreach (var filter in filters) delete.Where(filter.Value.Expression);
            return delete.AsTable(_repo.AsTableValueInternal);
        }
        internal IDelete<TEntity> OrmDeleteInternal(object dywhere) => OrmDelete(dywhere);
        protected override IInsert<TEntity> OrmInsert(TEntity entity) => OrmInsert(new[] { entity });
        protected override IInsert<TEntity> OrmInsert(IEnumerable<TEntity> entitys)
        {
            var insert = base.OrmInsert(entitys);
            var filters = (_repo.DataFilter as DataFilter<TEntity>)._filters.Where(a => a.Value.IsEnabled == true);
            foreach (var filter in filters)
            {
                if (entitys != null)
                    foreach (var entity in entitys)
                        if (filter.Value.ExpressionDelegate?.Invoke(entity) == false)
                            throw new Exception($"FreeSql.Repository Insert 失败，因为设置了过滤器 {filter.Key}: {filter.Value.Expression}，插入的数据不符合 {_db.OrmOriginal.GetEntityString(_entityType, entity)}");
            }
            return insert.AsTable(_repo.AsTableValueInternal);
        }
        internal IInsert<TEntity> OrmInsertInternal(TEntity entity) => OrmInsert(entity);
        internal IInsert<TEntity> OrmInsertInternal(IEnumerable<TEntity> entitys) => OrmInsert(entitys);
    }
}
