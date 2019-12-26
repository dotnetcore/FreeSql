
using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeSql
{
    public class DbContextOptions
    {

        /// <summary>
        /// 是否开启一对多，多对多级联保存功能<para></para>
        /// <para></para>
        /// 【一对多】模型下， 保存时可级联保存实体的属性集合。出于使用安全考虑我们没做完整对比，只实现实体属性集合的添加或更新操作，所以不会删除实体属性集合的数据。<para></para>
        /// 完整对比的功能使用起来太危险，试想下面的场景：<para></para>
        /// - 保存的时候，实体的属性集合是空的，如何操作？记录全部删除？<para></para>
        /// - 保存的时候，由于数据库中记录非常之多，那么只想保存子表的部分数据，或者只需要添加，如何操作？<para></para>
        /// <para></para>
        /// 【多对多】模型下，我们对中间表的保存是完整对比操作，对外部实体的操作只作新增（注意不会更新）
        /// - 属性集合为空时，删除他们的所有关联数据（中间表）
        /// - 属性集合不为空时，与数据库存在的关联数据（中间表）完全对比，计算出应该删除和添加的记录
        /// </summary>
        public bool EnableAddOrUpdateNavigateList { get; set; } = true;

        /// <summary>
        /// 实体变化事件
        /// </summary>
        public Action<List<DbContext.EntityChangeReport.ChangeInfo>> OnEntityChange { get; set; }
    }
}
