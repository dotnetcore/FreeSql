using System;
using System.Collections.Generic;
using System.Text;

namespace Zeus.Utility.Entity
{
    /// <summary>
    /// 数据模型接口
    /// </summary>
    public interface IEntity<out TKey>
    {
        /// <summary>
        /// 实体唯一标识，主键
        /// </summary>
        TKey ID { get; }
        /// <summary>
        /// 创建时间
        /// </summary>
        DateTime CreatedAt { get; set; }
    }
}
