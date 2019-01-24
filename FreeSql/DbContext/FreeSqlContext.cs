using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace FreeSql.DbContext
{
    public class FreeSqlDbContext
    {
        public IFreeSql Context;

        public FreeSqlDbContext(FreeSqlBuilder build)
        {
            Context = build.Build();
        }

        protected void InitDbContext(CodeFirstFactory codeFirstFactory)
        {
            this.InitDbContext();

            if (!codeFirstFactory.Contains(this.GetType()))
            {
                InitCodeFirst(Context.CodeFirst);

                codeFirstFactory.TryAdd(GetType(), Context.CodeFirst);
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

    public sealed class CodeFirstFactory
    {
        private ConcurrentDictionary<Type, ICodeFirst> Dictionary = new ConcurrentDictionary<Type, ICodeFirst>();

        public bool Contains(Type type)
        {
            return Dictionary.ContainsKey(type);
        }

        public bool TryAdd(Type type,ICodeFirst codeFirst)
        {
            return Dictionary.TryAdd(type, codeFirst);
        }
    }

    internal static class FreeSqlDbContextExtension
    {
        public static void InitDbContext(this FreeSqlDbContext context)
        {
            var type = context.GetType();
            
            foreach(var propertyInfo in
                type.GetProperties(
                    BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance
                )
            )
            {
                var dbSet = Activator.CreateInstance(propertyInfo.PropertyType, context.Context);

                propertyInfo.SetValue(context, dbSet);
            }
        }
    }
}
