using System;
using System.Text;

namespace FreeSql.DbContext
{
    public abstract class FreeSqlDbContext
    {
        public IFreeSql Context;

        public FreeSqlDbContext(IFreeSql context)
        {
            Context = context;
        }

        protected void InitDbContext(CodeFirstFactory codeFirstFactory)
        {
            this.InitDbContext();

            if (!codeFirstFactory.Contains(this.GetType()))
            {
                lock(this)
                {
                    InitCodeFirst(Context.CodeFirst);

                    codeFirstFactory.TryAdd(GetType(), Context.CodeFirst);
                }
            }
        }

        public FreeSqlDbSet<TEntity> Set<TEntity>() where TEntity:class
        {
            return new FreeSqlDbSet<TEntity>(Context);
        }

        public void Transaction(Action handler)
        {
            Context.Transaction(handler);
        }

        public void Transaction(Action handler, TimeSpan timeout)
        {
            Context.Transaction(handler, timeout);
        }

        protected virtual void InitCodeFirst(ICodeFirst CodeFirst)
        {

        }
    }
}
