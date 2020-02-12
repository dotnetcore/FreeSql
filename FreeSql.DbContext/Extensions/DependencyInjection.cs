#if netcoreapp
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace FreeSql
{
    public static class DbContextDependencyInjection
    {
        static IServiceCollection AddFreeDbContext(this IServiceCollection services, Type dbContextType, Action<DbContextOptionsBuilder> options)
        {
            services.AddScoped(dbContextType, sp =>
            {
                DbContext ctx = null;
                try
                {
                    var ctor = dbContextType. GetConstructors().FirstOrDefault();
                    var ctorParams = ctor.GetParameters().Select(a => sp.GetService(a.ParameterType)).ToArray();
                    ctx = Activator.CreateInstance(dbContextType, ctorParams) as DbContext;
                }
                catch(Exception ex)
                {
                    throw new Exception($"AddFreeDbContext 发生错误，请检查 {dbContextType.Name} 的构造参数都已正确注入", ex);
                }
                if (ctx != null && ctx._ormPriv == null)
                {
                    var builder = new DbContextOptionsBuilder();
                    options(builder);
                    ctx._ormPriv = builder._fsql;
                    ctx._optionsPriv = builder._options;

                    if (ctx._ormPriv == null)
                        throw new Exception("请在 OnConfiguring 或 AddFreeDbContext 中配置 UseFreeSql");

                    ctx.InitPropSets();
                }
                return ctx;
            });
            return services;
        }

        public static IServiceCollection AddFreeDbContext<TDbContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> options) where TDbContext : DbContext => 
            AddFreeDbContext(services, typeof(TDbContext), options);

        public static IServiceCollection AddFreeDbContext(this IServiceCollection services, Action<DbContextOptionsBuilder> options, Assembly[] assemblies)
        {
            if (assemblies?.Any() == true)
                foreach (var asse in assemblies)
                    foreach (var dbType in asse.GetTypes().Where(a => a.IsAbstract == false && typeof(DbContext).IsAssignableFrom(a)))
                        AddFreeDbContext(services, dbType, options);
            return services;
        }
    }
}
#endif