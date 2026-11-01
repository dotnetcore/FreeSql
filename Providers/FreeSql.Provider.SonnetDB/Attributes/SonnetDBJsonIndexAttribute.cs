using System;

namespace FreeSql.Provider.SonnetDB.Attributes
{
    /// <summary>
    /// Declares a SonnetDB JSON path index without extending FreeSql's common index model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class SonnetDBJsonIndexAttribute : Attribute
    {
        public SonnetDBJsonIndexAttribute(string name, string fields, string jsonPath)
        {
            Name = name;
            Fields = fields;
            JsonPath = jsonPath;
        }

        public string Name { get; }
        public string Fields { get; }
        public string JsonPath { get; }
        public bool IsUnique { get; set; }
    }
}
