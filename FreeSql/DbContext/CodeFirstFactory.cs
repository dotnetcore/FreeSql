using System;
using System.Collections.Concurrent;

namespace FreeSql.DbContext
{
    public sealed class CodeFirstFactory
    {
        private ConcurrentDictionary<Type, ICodeFirst> Dictionary = new ConcurrentDictionary<Type, ICodeFirst>();

        public bool Contains(Type type)
        {
            return Dictionary.ContainsKey(type);
        }

        public bool TryAdd(Type type,ICodeFirst codeFirst)
        {
            return Dictionary.TryAdd(type, codeFirst);
        }
    }
}
