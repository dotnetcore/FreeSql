using System;

namespace FreeSql.Extensions.ZeroEntity.Models
{
    public class SchemaValidationException : Exception
    {
        public SchemaValidationException(string message) : base(message) { }
    }
}
