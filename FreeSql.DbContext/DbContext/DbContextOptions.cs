
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeSql
{
    public class DbContextOptions
    {
        /// <summary>
        /// 是否开启 一对一(OneToOne)、一对多(OneToMany)、多对多(ManyToMany) 级联保存功能<para></para>
        /// <para></para>
        /// 【一对一】模型下，保存时级联保存 OneToOne 属性。
        /// <para></para>
        /// 【一对多】模型下，保存时级联保存 OneToMany 集合属性。出于安全考虑我们没做完整对比，只针对实体属性集合的添加或更新操作，因此不会删除数据库表已有的数据。<para></para>
        /// 完整对比的功能使用起来太危险，试想下面的场景：<para></para>
        /// - 保存的时候，实体的属性集合为空时(!=null)，表记录全部删除？<para></para>
        /// - 保存的时候，由于数据库子表的记录很多，只想保存子表的部分数据，又或者只需要添加，如何操作？
        /// <para></para>
        /// 【多对多】模型下，对中间表的保存是完整对比操作，对外部实体的只作新增操作（*注意不会更新）<para></para>
        /// - 属性集合为空时(!=null)，删除他们的所有关联数据（中间表）<para></para>
        /// - 属性集合不为空时，与数据库存在的关联数据（中间表）完全对比，计算出应该删除和添加的记录
        /// </summary>
        public bool EnableCascadeSave { get; set; } = false;

        /// <summary>
        /// 因增加支持 OneToOne 级联保存，和基于内存的级联删除，已改名为 EnableCascadeSave
        /// </summary>
        [Obsolete("因增加支持 OneToOne 级联保存，和基于内存的级联删除，已改名为 EnableCascadeSave")]
        public bool EnableAddOrUpdateNavigateList
        {
            get => EnableCascadeSave;
            set => EnableCascadeSave = value;
        }

        /// <summary>
        /// 使用无参数化设置（对应 IInsert/IUpdate）
        /// </summary>
        public bool? NoneParameter { get; set; }

        /// <summary>
        /// 是否开启 IFreeSql GlobalFilter 功能（默认：true）
        /// </summary>
        public bool EnableGlobalFilter { get; set; } = true;

        /// <summary>
        /// 实体变化事件
        /// </summary>
        public Action<List<DbContext.EntityChangeReport.ChangeInfo>> OnEntityChange { get; set; }

        /// <summary>
        /// DbContext/Repository 审计值，适合 Scoped IOC 中获取登陆信息
        /// </summary>
        public Action<DbContextAuditValueEventArgs> AuditValue;
    }

    public class DbContextAuditValueEventArgs : EventArgs
    {
        public DbContextAuditValueEventArgs(Aop.AuditValueType auditValueType, Type entityType, object obj)
        {
            this.AuditValueType = auditValueType;
            this.EntityType = entityType;
            this.Object = obj;
        }

        /// <summary>
        /// 类型
        /// </summary>
        public Aop.AuditValueType AuditValueType { get; }
        /// <summary>
        /// 类型
        /// </summary>
        public Type EntityType { get; }
        /// <summary>
        /// 实体对象
        /// </summary>
        public object Object { get; }
    }
}
