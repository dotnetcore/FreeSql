using FreeSql.DataAnnotations;
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
            void FluentApi(ColumnFluent col);
        }
    }
    public abstract class TypeHandler<T> : ITypeHandler
    {
        public abstract T Deserialize(object value);
        public abstract object Serialize(T value);
        public virtual void FluentApi(ColumnFluent col) { }

        public Type Type => typeof(T);
        object ITypeHandler.Deserialize(object value) => this.Deserialize(value);
        object ITypeHandler.Serialize(object value) => this.Serialize((T)value);
    }
}
