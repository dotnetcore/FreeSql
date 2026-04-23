using System;

namespace FreeSql
{
    /// <summary>
    /// Default repository bound to a typed FreeSql marker.
    /// </summary>
    public class DefaultRepository<TEntity, TKey, TMark> : BaseRepository<TEntity, TKey>
        where TEntity : class
    {
        public DefaultRepository(IFreeSql<TMark> fsql) : base(fsql) { }
        public DefaultRepository(IFreeSql<TMark> fsql, UnitOfWorkManager<TMark> uowManger) : base(uowManger?.Orm ?? fsql)
        {
            uowManger?.Binding(this);
        }
    }

    class GuidRepository<TEntity, TMark> : BaseRepository<TEntity, Guid>
        where TEntity : class
    {
        public GuidRepository(IFreeSql<TMark> fsql) : base(fsql) { }
        public GuidRepository(IFreeSql<TMark> fsql, UnitOfWorkManager<TMark> uowManger) : base(uowManger?.Orm ?? fsql)
        {
            uowManger?.Binding(this);
        }
    }
}
