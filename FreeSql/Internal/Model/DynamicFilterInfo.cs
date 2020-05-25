using FreeSql;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FreeSql.Internal.Model
{
    /// <summary>
    /// 动态过滤条件
    /// </summary>
    public class DynamicFilterInfo
    {
        /// <summary>
        /// 属性名：Name<para></para>
        /// 导航属性：Parent.Name<para></para>
        /// 多表：b.Name<para></para>
        /// </summary>
        public string Field { get; set; }
        /// <summary>
        /// 操作符
        /// </summary>
        public DynamicFilterOperator Operator { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Filters 下的逻辑运算符
        /// </summary>
        public DynamicFilterLogic Logic { get; set; }
        /// <summary>
        /// 子过滤条件，它与当前的逻辑关系是 And<para></para>
        /// 注意：当前 Field 可以留空
        /// </summary>
        public List<DynamicFilterInfo> Filters { get; set; }
    }

    public enum DynamicFilterLogic { And, Or }
    public enum DynamicFilterOperator
    {
        /// <summary>
        /// like
        /// </summary>
        Contains,
        StartsWith,
        EndsWith,
        NotContains,
        NotStartsWith,
        NotEndsWith,

        /// <summary>
        /// =
        /// </summary>
        Equals,
        Eq,
        /// <summary>
        /// &lt;&gt;
        /// </summary>
        NotEqual,

        /// <summary>
        /// &gt;
        /// </summary>
        GreaterThan,
        /// <summary>
        /// &gt;=
        /// </summary>
        GreaterThanOrEqual,
        /// <summary>
        /// &lt;
        /// </summary>
        LessThan,
        /// <summary>
        /// &lt;=
        /// </summary>
        LessThanOrEqual,
    }
}
