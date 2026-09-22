using FreeSql.DataAnnotations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql.DatabaseModel
{
    public class DbIndexInfo
    {
        public string Name { get; set; }
        public List<DbIndexColumnInfo> Columns { get; } = new List<DbIndexColumnInfo>();
        public bool IsUnique { get; set; }
    }

    public class DbIndexColumnInfo
    {
        public DbColumnInfo Column { get; set; }
        public bool IsDesc { get; set; }
    }
}