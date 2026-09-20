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
        public IndexMethod IndexMethod { get; set; }

        /// <summary>
        /// JSON 路径索引的路径表达式。普通索引留空；仅由支持该能力的提供程序使用。
        /// </summary>
        public string JsonPath { get; set; }
    }

    public class IndexColumnInfo
    {
        public ColumnInfo Column { get; set; }
        public bool IsDesc { get; set; }
    }
}
