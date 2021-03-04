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
            var select = base.OrmSelect(dywhere).AsTable(_repo.AsTableSelectValueInternal);

            var filters = (_repo.DataFilter as DataFilter<TEntity>)._filters;
            foreach (var filter in filters.Where(a => a.Value.IsEnabled == true)) select.Where(filter.Value.Expression);
            var disableFilter = filters.Where(a => a.Value.IsEnabled == false).Select(a => a.Key).ToArray();
            if (disableFilter.Any()) select.DisableGlobalFilter();
            return select;
        }
        internal ISelect<TEntity> OrmSelectInternal(object dywhere) => OrmSelect(dywhere);
        protected override IUpdate<TEntity> OrmUpdate(IEnumerable<TEntity> entitys)
        {
            var update = base.OrmUpdate(entitys).AsTable(_repo.AsTableValueInternal);
            var filters = (_repo.DataFilter as DataFilter<TEntity>)._filters;
            foreach (var filter in filters.Where(a => a.Value.IsEnabled == true))
            {
                if (entitys != null)
                    foreach (var entity in entitys)
                        if (filter.Value.ExpressionDelegate?.Invoke(entity) == false)
                            throw new Exception($"FreeSql.Repository Update 失败，因为设置了过滤器 {filter.Key}: {filter.Value.Expression}，更新的数据不符合 {_db.OrmOriginal.GetEntityString(_entityType, entity)}");
                update.Where(filter.Value.Expression);
            }
            var disableFilter = filters.Where(a => a.Value.IsEnabled == false).Select(a => a.Key).ToArray();
            if (disableFilter.Any()) update.DisableGlobalFilter();
            return update;
        }
        internal IUpdate<TEntity> OrmUpdateInternal(IEnumerable<TEntity> entitys) => OrmUpdate(entitys);
        protected override IDelete<TEntity> OrmDelete(object dywhere)
        {
            var delete = base.OrmDelete(dywhere).AsTable(_repo.AsTableValueInternal);
            var filters = (_repo.DataFilter as DataFilter<TEntity>)._filters;
            foreach (var filter in filters.Where(a => a.Value.IsEnabled == true)) delete.Where(filter.Value.Expression);
            var disableFilter = filters.Where(a => a.Value.IsEnabled == false).Select(a => a.Key).ToArray();
            if (disableFilter.Any()) delete.DisableGlobalFilter();
            return delete;
        }
        internal IDelete<TEntity> OrmDeleteInternal(object dywhere) => OrmDelete(dywhere);
        protected override IInsert<TEntity> OrmInsert(TEntity entity) => OrmInsert(new[] { entity });
        protected override IInsert<TEntity> OrmInsert(IEnumerable<TEntity> entitys)
        {
            var insert = base.OrmInsert(entitys).AsTable(_repo.AsTableValueInternal);
            var filters = (_repo.DataFilter as DataFilter<TEntity>)._filters.Where(a => a.Value.IsEnabled == true);
            foreach (var filter in filters)
            {
                if (entitys != null)
                    foreach (var entity in entitys)
                        if (filter.Value.ExpressionDelegate?.Invoke(entity) == false)
                            throw new Exception($"FreeSql.Repository Insert 失败，因为设置了过滤器 {filter.Key}: {filter.Value.Expression}，插入的数据不符合 {_db.OrmOriginal.GetEntityString(_entityType, entity)}");
            }
            return insert;
        }
        internal IInsert<TEntity> OrmInsertInternal(TEntity entity) => OrmInsert(entity);
        internal IInsert<TEntity> OrmInsertInternal(IEnumerable<TEntity> entitys) => OrmInsert(entitys);
    }
}
