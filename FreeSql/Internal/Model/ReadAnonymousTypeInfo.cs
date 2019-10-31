using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FreeSql.Internal.Model
{
    public class ReadAnonymousTypeInfo
    {
        public PropertyInfo Property { get; set; }
        public string CsName { get; set; }
        public Type CsType { get; set; }
        public Type MapType { get; set; }
        public string DbField { get; set; }
        public ConstructorInfo Consturctor { get; set; }
        public ReadAnonymousTypeInfoConsturctorType ConsturctorType { get; set; }
        public List<ReadAnonymousTypeInfo> Childs = new List<ReadAnonymousTypeInfo>();
        public TableInfo Table { get; set; }
    }
    public enum ReadAnonymousTypeInfoConsturctorType { Arguments, Properties }
}
