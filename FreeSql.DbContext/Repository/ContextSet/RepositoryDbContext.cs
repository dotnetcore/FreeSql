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

        static ConcurrentDictionary<Type, ConcurrentDictionary<string, FieldInfo>> _dicGetRepositoryDbField = new ConcurrentDictionary<Type, ConcurrentDictionary<string, FieldInfo>>();
        static FieldInfo GetRepositoryDbField(Type type, string fieldName) => _dicGetRepositoryDbField.GetOrAdd(type, tp => new ConcurrentDictionary<string, FieldInfo>()).GetOrAdd(fieldName, fn =>
            typeof(BaseRepository<,>).MakeGenericType(type, typeof(int)).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic));
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
				GetRepositoryDbField(entityType, "_dbPriv").SetValue(repo, this);
                GetRepositoryDbField(entityType, "_asTablePriv").SetValue(repo,
                    _repo.GetType().GetField("_asTablePriv", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_repo));
				    //GetRepositoryDbField(_repo.EntityType, "_asTablePriv").GetValue(_repo));
            }

            var sd = Activator.CreateInstance(typeof(RepositoryDbSet<>).MakeGenericType(entityType), repo) as IDbSet;
            _listSet.Add(sd);
            if (entityType != typeof(object)) _dicSet.Add(entityType, sd);
            return sd;
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
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await FlushCommandAsync(cancellationToken);
            return SaveChangesSuccess();
        }
#endif
    }
}
