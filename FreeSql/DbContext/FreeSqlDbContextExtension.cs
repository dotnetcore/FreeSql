using System;
using System.Reflection;

namespace FreeSql.DbContext
{
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
