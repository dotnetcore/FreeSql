using System;
using System.Linq.Expressions;

namespace FreeSql
{
    public class GuidRepository<TEntity> :
        BaseRepository<TEntity, Guid>
        where TEntity : class
    {

        public GuidRepository(IFreeSql fsql) : this(fsql, null, null)
        {

        }
        public GuidRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter, Func<string, string> asTable) : base(fsql, filter, asTable)
        {
        }
    }
}
