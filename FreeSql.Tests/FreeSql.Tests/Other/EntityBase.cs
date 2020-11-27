using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Zeus.Utility.Entity
{
    /// <summary>
    /// 实体类基类
    /// </summary>
    public abstract class EntityBase<TKey> : IEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// 初始化一个<see cref="EntityBase{TKey}"/>类型的新实例
        /// </summary>
        protected EntityBase()
        {
            
        }

        /// <summary>
        /// 获取或设置主键
        /// </summary>
        [Key]
        public virtual TKey ID { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 判断两个实体是否是同一数据记录的实体
        /// </summary>
        /// <param name="obj">要比较的实体信息</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is EntityBase<TKey> entity))
            {
                return false;
            }
            return IsKeyEqual(entity.ID, ID);
        }

        /// <summary>
        /// 实体Id是否相等
        /// </summary>
        public static bool IsKeyEqual(TKey id1, TKey id2)
        {
            if (id1 == null && id2 == null)
            {
                return true;
            }
            if (id1 == null || id2 == null)
            {
                return false;
            }

            return Equals(id1, id2);
        }

        /// <summary>
        /// 用作特定类型的哈希函数
        /// </summary>
        /// <returns>
        /// 当前 <see cref="T:System.Object"/> 的哈希代码。<br/>
        /// 如果<c>Id</c>为<c>null</c>则返回0，
        /// 如果不为<c>null</c>则返回<c>Id</c>对应的哈希值
        /// </returns>
        public override int GetHashCode()
        {
            if (ID == null)
            {
                return 0;
            }
            return ID.ToString().GetHashCode();
        }
    }
}
