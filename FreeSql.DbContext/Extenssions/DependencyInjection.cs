#if ns20

using Microsoft.Extensions.DependencyInjection;
using System;

namespace FreeSql
{
    public static class DbContextDependencyInjection
    {

        public static IServiceCollection AddFreeDbContext<TDbContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> options) where TDbContext : DbContext
        {

            services.AddScoped<TDbContext>(sp =>
            {
                var ctx = Activator.CreateInstance<TDbContext>();

                if (ctx._orm == null)
                {
                    var builder = new DbContextOptionsBuilder();
                    options(builder);
                    ctx._orm = builder._fsql;

                    if (ctx._orm == null)
                        throw new Exception("请在 OnConfiguring 或 AddFreeDbContext 中配置 UseFreeSql");

                    ctx.InitPropSets();
                }

                return ctx;
            });

            return services;
        }
    }
}
#endif