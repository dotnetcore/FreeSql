using System;
using System.Collections.Concurrent;

namespace FreeSql.DataAnnotations
{
    public class TableAttribute : Attribute
    {

        /// <summary>
        /// 数据库表名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 指定数据库旧的表名，修改实体命名时，同时设置此参数为修改之前的值，CodeFirst才可以正确修改数据库表；否则将视为【创建新表】
        /// </summary>
        public string OldName { get; set; }
        /// <summary>
        /// 查询过滤SQL，实现类似 a.IsDeleted = 1 功能
        /// </summary>
        public string SelectFilter { get; set; }

        internal bool? _DisableSyncStructure;
        /// <summary>
        /// 禁用 CodeFirst 同步结构迁移
        /// </summary>
        public bool DisableSyncStructure { get => _DisableSyncStructure ?? false; set => _DisableSyncStructure = value; }

        internal ConcurrentDictionary<string, ColumnAttribute> _columns { get; } = new ConcurrentDictionary<string, ColumnAttribute>(StringComparer.CurrentCultureIgnoreCase);
    }
}
