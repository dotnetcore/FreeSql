using FreeSql.Internal.Model.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Internal.Model
{
    namespace Interface
    {
        public interface ITypeHandler
        {
            Type Type { get; }
            object Deserialize(object value);
            object Serialize(object value);
        }
    }
    public abstract class TypeHandler<T> : ITypeHandler
    {
        public abstract T Deserialize(object value);
        public abstract object Serialize(T value);

        public Type Type => typeof(T);
        object ITypeHandler.Deserialize(object value) => this.Deserialize(value);
        object ITypeHandler.Serialize(object value) => this.Serialize((T)value);
    }
}
