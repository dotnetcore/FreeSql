#if ns20

using System;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace FreeSql
{
    public static class FreeSqlRepositoryDependencyInjection
    {

        public static IServiceCollection AddFreeRepository(this IServiceCollection services, Action<FluentDataFilter> globalDataFilter = null, params Assembly[] assemblies)
        {

            DataFilterUtil._globalDataFilter = globalDataFilter;

            services.AddScoped(typeof(IReadOnlyRepository<>), typeof(GuidRepository<>));
            services.AddScoped(typeof(IBasicRepository<>), typeof(GuidRepository<>));
            services.AddScoped(typeof(BaseRepository<>), typeof(GuidRepository<>));
            services.AddScoped(typeof(GuidRepository<>));

            services.AddScoped(typeof(IReadOnlyRepository<,>), typeof(DefaultRepository<,>));
            services.AddScoped(typeof(IBasicRepository<,>), typeof(DefaultRepository<,>));
            services.AddScoped(typeof(BaseRepository<,>), typeof(DefaultRepository<,>));
            services.AddScoped(typeof(DefaultRepository<,>));

            if (assemblies?.Any() == true)
                foreach (var asse in assemblies)
                    foreach (var repo in asse.GetTypes().Where(a => a.IsAbstract == false && typeof(IBaseRepository).IsAssignableFrom(a)))
                        services.AddScoped(repo);

            return services;
        }
    }
}

#endif