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

        IUnitOfWork _uowPriv;
        internal override IUnitOfWork _uow
        {
            get => _uowPriv;
            set
            {
                _uowPriv = value;
                foreach (var dbset in _dicDbSetObjects.Values) //配合 UnitOfWorkManager
                    dbset._uow = _uowPriv;
            }
        }

        protected override ISelect<TEntity> OrmSelect(object dywhere)
        {
            var select = base.OrmSelect(dywhere);
            if (_repo._asTablePriv != null) select.AsTable(_repo._asTablePriv);
            var disableFilter = _repo.DataFilter._filtersByOrm.Where(a => a.Value.IsEnabled == false).Select(a => a.Key).ToArray();
            if (disableFilter.Any()) select.DisableGlobalFilter(disableFilter);
            return select;
        }
        internal ISelect<TEntity> OrmSelectInternal(object dywhere) => OrmSelect(dywhere);
        protected override IUpdate<TEntity> OrmUpdate(IEnumerable<TEntity> entitys)
        {
            var update = base.OrmUpdate(entitys);
			if (_repo._asTablePriv != null) update.AsTable(old => _repo._asTablePriv(_entityType, old));
            var disableFilter = _repo.DataFilter._filtersByOrm.Where(a => a.Value.IsEnabled == false).Select(a => a.Key).ToArray();
            if (disableFilter.Any()) update.DisableGlobalFilter(disableFilter);
            return update;
        }
        internal IUpdate<TEntity> OrmUpdateInternal(IEnumerable<TEntity> entitys) => OrmUpdate(entitys);
        protected override IDelete<TEntity> OrmDelete(object dywhere)
        {
            var delete = base.OrmDelete(dywhere);
			if (_repo._asTablePriv != null) delete.AsTable(old => _repo._asTablePriv(_entityType, old));
            var disableFilter = _repo.DataFilter._filtersByOrm.Where(a => a.Value.IsEnabled == false).Select(a => a.Key).ToArray();
            if (disableFilter.Any()) delete.DisableGlobalFilter(disableFilter);
            return delete;
        }
        internal IDelete<TEntity> OrmDeleteInternal(object dywhere) => OrmDelete(dywhere);

		protected override IDelete<object> OrmDeleteAsType(Type entityType)
		{
			var delete = base.OrmDeleteAsType(entityType);
			if (_repo._asTablePriv != null) delete.AsTable(old => _repo._asTablePriv(_entityType, old));
            return delete;
		}

		protected override IInsert<TEntity> OrmInsert(TEntity entity) => OrmInsert(new[] { entity });
        protected override IInsert<TEntity> OrmInsert(IEnumerable<TEntity> entitys)
        {
            var insert = base.OrmInsert(entitys);
			if (_repo._asTablePriv != null) insert.AsTable(old => _repo._asTablePriv(_entityType, old));
            return insert;
        }
        internal IInsert<TEntity> OrmInsertInternal(TEntity entity) => OrmInsert(entity);
        internal IInsert<TEntity> OrmInsertInternal(IEnumerable<TEntity> entitys) => OrmInsert(entitys);
    }
}
