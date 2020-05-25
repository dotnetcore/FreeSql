#if netcoreapp
using FreeSql;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FreeSqlRepositoryDependencyInjection
    {

        /// <summary>
        /// 批量注入 Repository，可以参考代码自行调整
        /// </summary>
        /// <param name="services"></param>
        /// <param name="globalDataFilter"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddFreeRepository(this IServiceCollection services, Action<FluentDataFilter> globalDataFilter = null, params Assembly[] assemblies)
        {
            if (globalDataFilter != null)
            {
                DataFilterUtil._globalDataFilter = globalDataFilter;
                //如果看到了这里的代码，想自己调整，但因为 _globalDataFilter 是内部属性，无法修改？
                //请考虑改用 fsql.GlobalFilter.Apply
            }

            services.AddScoped(typeof(IBaseRepository<>), typeof(GuidRepository<>));
            services.AddScoped(typeof(BaseRepository<>), typeof(GuidRepository<>));

            services.AddScoped(typeof(IBaseRepository<,>), typeof(DefaultRepository<,>));
            services.AddScoped(typeof(BaseRepository<,>), typeof(DefaultRepository<,>));

            if (assemblies?.Any() == true)
                foreach (var asse in assemblies)
                    foreach (var repo in asse.GetTypes().Where(a => a.IsAbstract == false && typeof(IBaseRepository).IsAssignableFrom(a)))
                        services.AddScoped(repo);

            return services;
        }
    }
}
#endif