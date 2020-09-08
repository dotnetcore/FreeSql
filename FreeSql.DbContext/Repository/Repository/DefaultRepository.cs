using System;
using System.Linq.Expressions;

namespace FreeSql
{
    public class DefaultRepository<TEntity, TKey> : BaseRepository<TEntity, TKey> where TEntity : class
    {
        public DefaultRepository(IFreeSql fsql) : base(fsql, null, null) { }
        public DefaultRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter) : base(fsql, filter, null) { }
        public DefaultRepository(IFreeSql fsql, UnitOfWorkManager uowManger) : base(uowManger?.Orm ?? fsql, null, null)
        {
            uowManger?.Binding(this);
        }
    }

    public class GuidRepository<TEntity> : BaseRepository<TEntity, Guid> where TEntity : class
    {
        public GuidRepository(IFreeSql fsql) : this(fsql, null, null) { }
        public GuidRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter, Func<string, string> asTable) : base(fsql, filter, asTable) { }
        public GuidRepository(IFreeSql fsql, UnitOfWorkManager uowManger) : base(uowManger?.Orm ?? fsql, null, null)
        {
            uowManger?.Binding(this);
        }
    }
}
