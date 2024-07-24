using System;
using System.Linq.Expressions;

namespace FreeSql
{
    class DefaultRepository<TEntity, TKey> : BaseRepository<TEntity, TKey> where TEntity : class
    {
        public DefaultRepository(IFreeSql fsql) : base(fsql) { }
        public DefaultRepository(IFreeSql fsql, UnitOfWorkManager uowManger) : base(uowManger?.Orm ?? fsql)
        {
            uowManger?.Binding(this);
        }
    }

    class GuidRepository<TEntity> : BaseRepository<TEntity, Guid> where TEntity : class
    {
        public GuidRepository(IFreeSql fsql) : base(fsql) { }
        public GuidRepository(IFreeSql fsql, UnitOfWorkManager uowManger) : base(uowManger?.Orm ?? fsql)
        {
            uowManger?.Binding(this);
        }
    }
}
