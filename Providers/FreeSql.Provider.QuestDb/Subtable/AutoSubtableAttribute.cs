using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Provider.QuestDb.Subtable
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class AutoSubtableAttribute : Attribute
    {
        public SubtableType? SubtableType
        {
            get; set;
        }
        public AutoSubtableAttribute(SubtableType type)
        {
            SubtableType = type;
        }
    }
}
