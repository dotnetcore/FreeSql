using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace FreeSql.Provider.QuestDb
{
    internal class QuestDbContainer
    {
        private static IServiceCollection Services;
        public static IServiceProvider ServiceProvider { get; private set; }

        internal static void Initialize(Action<IServiceCollection> service)
        {
            Services = new ServiceCollection();
            service?.Invoke(Services);
            ServiceProvider = Services.BuildServiceProvider();
        }

        internal static T GetService<T>()
        {
            return ServiceProvider.GetService<T>();
        }
    }
}