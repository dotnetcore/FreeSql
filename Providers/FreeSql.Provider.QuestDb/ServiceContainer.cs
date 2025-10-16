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
        private static IServiceProvider _serviceProvider;

        internal static void Initialize(Action<IServiceCollection> service)
        {
            _services = new ServiceCollection();
            service?.Invoke(_services);
            _serviceProvider = _services.BuildServiceProvider();
        }

        internal static T GetService<T>()
        {
            return _serviceProvider == null ? default : _serviceProvider.GetService<T>();
        }
    }
}