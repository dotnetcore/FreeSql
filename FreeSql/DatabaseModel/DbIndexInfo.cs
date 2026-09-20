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

        /// <summary>
        /// 数据库返回的 JSON 路径索引路径。普通索引留空。
        /// </summary>
        public string JsonPath { get; set; }
    }

    public class DbIndexColumnInfo
    {
        public DbColumnInfo Column { get; set; }
        public bool IsDesc { get; set; }
    }
}
