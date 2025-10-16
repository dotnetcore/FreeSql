using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace FreeSql.Provider.QuestDb
{
    internal class ServiceContainer
    {
        private static IServiceCollection _services;
        internal static IServiceProvider ServiceProvider { get; private set; }

        internal static void Initialize(Action<IServiceCollection> service)
        {
            _services = new ServiceCollection();
            service?.Invoke(_services);
            ServiceProvider = _services.BuildServiceProvider();
        }

        internal static T GetService<T>()
        {
            return ServiceProvider.GetService<T>();
        }
    }
}