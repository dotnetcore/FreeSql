using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace FreeSql
{
    internal class RepositoryDbContext : DbContext
    {

        protected IBaseRepository _repos;
        public RepositoryDbContext(IFreeSql orm, IBaseRepository repos) : base()
        {
            _orm = orm;
            _repos = repos;
            _isUseUnitOfWork = false;
            _uowPriv = _repos.UnitOfWork;
        }


        static ConcurrentDictionary<Type, FieldInfo> _dicGetRepositoryDbField = new ConcurrentDictionary<Type, FieldInfo>();
        static FieldInfo GetRepositoryDbField(Type type) => _dicGetRepositoryDbField.GetOrAdd(type, tp => typeof(BaseRepository<,>).MakeGenericType(tp, typeof(int)).GetField("_dbPriv", BindingFlags.Instance | BindingFlags.NonPublic));
        public override IDbSet Set(Type entityType)
        {
            if (_dicSet.ContainsKey(entityType)) return _dicSet[entityType];

            var tb = _orm.CodeFirst.GetTableByEntity(entityType);
            if (tb == null) return null;

            object repos = _repos;
            if (entityType != _repos.EntityType)
            {
                repos = Activator.CreateInstance(typeof(DefaultRepository<,>).MakeGenericType(entityType, typeof(int)), _repos.Orm);
                (repos as IBaseRepository).UnitOfWork = _repos.UnitOfWork;
                GetRepositoryDbField(entityType).SetValue(repos, this);

                typeof(RepositoryDbContext).GetMethod("SetRepositoryDataFilter").MakeGenericMethod(_repos.EntityType)
                    .Invoke(null, new object[] { repos, _repos });
            }

            var sd = Activator.CreateInstance(typeof(RepositoryDbSet<>).MakeGenericType(entityType), repos) as IDbSet;
            if (entityType != typeof(object)) _dicSet.Add(entityType, sd);
            return sd;
        }

        public static void SetRepositoryDataFilter<TEntity>(object repos, BaseRepository<TEntity> baseRepo) where TEntity : class
        {
            var filter = baseRepo.DataFilter as DataFilter<TEntity>;
            DataFilterUtil.SetRepositoryDataFilter(repos, fl =>
            {
                foreach (var f in filter._filters)
                    fl.Apply<TEntity>(f.Key, f.Value.Expression);
            });
        }

        public override int SaveChanges()
        {
            ExecCommand();
            var ret = _affrows;
            _affrows = 0;
            return ret;
        }
        async public override Task<int> SaveChangesAsync()
        {
            await ExecCommandAsync();
            var ret = _affrows;
            _affrows = 0;
            return ret;
        }
    }
}
