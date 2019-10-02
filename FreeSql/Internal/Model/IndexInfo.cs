using FreeSql.DataAnnotations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql.Internal.Model
{
    public class IndexInfo
    {
        public string Name { get; set; }
        public IndexColumnInfo[] Columns { get; set; }
        public bool IsUnique { get; set; }
    }

    public class IndexColumnInfo
    {
        public ColumnInfo Column { get; set; }
        public bool IsDesc { get; set; }
    }
}