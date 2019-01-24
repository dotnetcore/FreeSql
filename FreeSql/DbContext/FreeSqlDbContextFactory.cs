using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace FreeSql.DbContext
{
    public class FreeSqlDbContextFactory
    {
        private ConcurrentDictionary<Type, FreeSqlBuilder> Dictionary = new ConcurrentDictionary<Type, FreeSqlBuilder>();
        private CodeFirstFactory CodeFirstFactory = new CodeFirstFactory();

        public void Add<TDbContext>(FreeSqlBuilder builder) where TDbContext:FreeSqlDbContext
        {
            Dictionary.TryAdd(typeof(TDbContext), builder);
        }

        public TDbContext Get<TDbContext>() where TDbContext : FreeSqlDbContext
        {
            if (Dictionary.ContainsKey(typeof(TDbContext)))
            {
                var type = typeof(TDbContext);
                var build = Dictionary[type];

                var context = Activator.CreateInstance(type, build) as TDbContext;
                if (context != null)
                {
                    var method = type.GetMethod("InitDbContext", BindingFlags.Instance | BindingFlags.NonPublic);
                    method?.Invoke(context, new[] { CodeFirstFactory });
                }

                return context;
            }

            return null;
        }
    }
}
