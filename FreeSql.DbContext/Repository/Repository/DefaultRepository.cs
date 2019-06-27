using System;
using System.Linq.Expressions;

namespace FreeSql
{
    public class DefaultRepository<TEntity, TKey> :
        BaseRepository<TEntity, TKey>
        where TEntity : class
    {

        public DefaultRepository(IFreeSql fsql) : base(fsql, null, null)
        {

        }

        public DefaultRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter) : base(fsql, filter, null)
        {
        }
    }
}
