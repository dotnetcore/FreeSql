using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Provider.ClickHouse.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ClickHousePartitionAttribute : Attribute
    {
        public ClickHousePartitionAttribute(string format = "toYYYYMM({0})")
        {
            Format = format;
        }

        public string Format { get; set; }
    }
}