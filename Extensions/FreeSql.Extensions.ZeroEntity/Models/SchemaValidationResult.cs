using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Extensions.ZeroEntity.Models
{
    public class SchemaValidationResult
    {
        public readonly static SchemaValidationResult _schemaValidationResult = new SchemaValidationResult("");

        public static SchemaValidationResult SuccessedResult => _schemaValidationResult;

        public SchemaValidationResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; set; }
        public bool Succeeded { get; set; } = false;
    }
}
