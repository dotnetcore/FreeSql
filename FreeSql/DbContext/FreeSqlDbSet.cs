using System;
using System.Collections.Generic;

namespace FreeSql.DbContext
{
    public class FreeSqlDbSet<TEntity> :
        IFreeSqlDbSet<TEntity>
        where TEntity:class
    {
        private IFreeSql Context { get; }

        public ICache Cache => Context.Cache;

        public IAdo Ado => Context.Ado;

        public ICodeFirst CodeFirst => Context.CodeFirst;

        public IDbFirst DbFirst => Context.DbFirst;

        public FreeSqlDbSet(IFreeSql context)
        {
            Context = context;
        }

        public IInsert<TEntity> Insert()
        {
            return Context.Insert<TEntity>();
        }

        public IInsert<TEntity> Insert(TEntity source)
        {
            return Context.Insert(source);
        }

        public IInsert<TEntity> Insert(IEnumerable<TEntity> source)
        {
            return Context.Insert(source);
        }

        public IUpdate<TEntity> Update()
        {
            return Context.Update<TEntity>();
        }

        public IUpdate<TEntity> Update(object dywhere)
        {
            return Context.Update<TEntity>(dywhere);
        }

        public ISelect<TEntity> Select()
        {
            return Context.Select<TEntity>();
        }

        public ISelect<TEntity> Select(object dywhere)
        {
            return Context.Select<TEntity>(dywhere);
        }

        public IDelete<TEntity> Delete()
        {
            return Context.Delete<TEntity>();
        }

        public IDelete<TEntity> Delete(object dywhere)
        {
            return Context.Delete<TEntity>(dywhere);
        }
    }
}
