using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Interface
{
    public interface IGlobalCacheFactory
    {
        T CreateCacheItem<T>(T defaultValue = null) where T : class, new();
        T CreateCacheItem<T>() where T : new();
    }
}
