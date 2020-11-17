using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{
    internal class RepositoryDbContext : DbContext
    {

        protected IBaseRepository _repo;
        public RepositoryDbContext(IFreeSql orm, IBaseRepository repo) : base()
        {
            _ormScoped = DbContextScopedFreeSql.Create(orm, () => this, () => repo.UnitOfWork);
            _isUseUnitOfWork = false;
            UnitOfWork = repo.UnitOfWork;
            _repo = repo;
        }

        static ConcurrentDictionary<Type, FieldInfo> _dicGetRepositoryDbField = new ConcurrentDictionary<Type, FieldInfo>();
        static FieldInfo GetRepositoryDbField(Type type) => _dicGetRepositoryDbField.GetOrAdd(type, tp => typeof(BaseRepository<,>).MakeGenericType(tp, typeof(int)).GetField("_dbPriv", BindingFlags.Instance | BindingFlags.NonPublic));
        public override IDbSet Set(Type entityType)
        {
            if (_dicSet.ContainsKey(entityType)) return _dicSet[entityType];

            var tb = OrmOriginal.CodeFirst.GetTableByEntity(entityType);
            if (tb == null) return null;

            object repo = _repo;
            if (entityType != _repo.EntityType)
            {
                repo = Activator.CreateInstance(typeof(DefaultRepository<,>).MakeGenericType(entityType, typeof(int)), _repo.Orm);
                (repo as IBaseRepository).UnitOfWork = _repo.UnitOfWork;
                GetRepositoryDbField(entityType).SetValue(repo, this);

                if (typeof(IBaseRepository<>).MakeGenericType(_repo.EntityType).IsAssignableFrom(_repo.GetType()))
                    typeof(RepositoryDbContext).GetMethod("SetRepositoryDataFilter").MakeGenericMethod(_repo.EntityType)
                        .Invoke(null, new object[] { repo, _repo });
            }

            var sd = Activator.CreateInstance(typeof(RepositoryDbSet<>).MakeGenericType(entityType), repo) as IDbSet;
            _listSet.Add(sd);
            if (entityType != typeof(object)) _dicSet.Add(entityType, sd);
            return sd;
        }

        public static void SetRepositoryDataFilter<TEntity>(object repo, IBaseRepository<TEntity> baseRepo) where TEntity : class
        {
            var filter = baseRepo.DataFilter as DataFilter<TEntity>;
            DataFilterUtil.SetRepositoryDataFilter(repo, fl =>
            {
                foreach (var f in filter._filters)
                    fl.Apply<TEntity>(f.Key, f.Value.Expression);
            });
        }

        int SaveChangesSuccess()
        {
            int ret;
            try
            {
                if (UnitOfWork?.EntityChangeReport != null)
                {
                    UnitOfWork.EntityChangeReport.Report.AddRange(_entityChangeReport);
                    if (UnitOfWork.EntityChangeReport.OnChange == null) UnitOfWork.EntityChangeReport.OnChange = Options.OnEntityChange;
                } else
                    EmitOnEntityChange(_entityChangeReport);
            }
            finally
            {
                _entityChangeReport.Clear();
                ret = _affrows;
                _affrows = 0;
            }
            return ret;
        }
        public override int SaveChanges()
        {
            FlushCommand();
            return SaveChangesSuccess();
        }
#if net40
#else
        async public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await FlushCommandAsync(cancellationToken);
            return SaveChangesSuccess();
        }
#endif
    }
}
