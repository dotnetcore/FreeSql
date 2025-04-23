﻿using FreeSql;
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
    [Serializable]
    public class DynamicFilterInfo
    {
        /// <summary>
        /// 使用所有逻辑运算符，默认为False。
        /// False：按照原来的模式，只有分组的逻辑运算符会生成到查询条件中,只有OR分组会生成括号。
        /// True：按照新的模式，所有的逻辑运算符都会生成到查询条件中，所有分组(AND或OR)都会生成括号。
        /// </summary>
        public bool UseAllLogic { get; set; }

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
        public object Value { get; set; }

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

    public enum DynamicFilterLogic
    { And, Or }

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
        /// =<para></para>
        /// Equal/Equals/Eq 效果相同
        /// </summary>
        Equal,

        /// <summary>
        /// =<para></para>
        /// Equal/Equals/Eq 效果相同
        /// </summary>
        Equals,

        /// <summary>
        /// =<para></para>
        /// Equal/Equals/Eq 效果相同
        /// </summary>
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

        /// <summary>
        /// &gt;= and &lt;<para></para>
        /// 此时 Value 的值格式为逗号分割：value1,value2 或者数组
        /// </summary>
        Range,

        /// <summary>
        /// &gt;= and &lt;<para></para>
        /// 此时 Value 的值格式为逗号分割：date1,date2 或者数组<para></para>
        /// 这是专门为日期范围查询定制的操作符，它会处理 date2 + 1，比如：<para></para>
        /// 当 date2 选择的是 2020-05-30，那查询的时候是 &lt; 2020-05-31<para></para>
        /// 当 date2 选择的是 2020-05，那查询的时候是 &lt; 2020-06<para></para>
        /// 当 date2 选择的是 2020，那查询的时候是 &lt; 2021<para></para>
        /// 当 date2 选择的是 2020-05-30 12，那查询的时候是 &lt; 2020-05-30 13<para></para>
        /// 当 date2 选择的是 2020-05-30 12:30，那查询的时候是 &lt; 2020-05-30 12:31<para></para>
        /// 并且 date2 只支持以上 5 种格式 (date1 没有限制)
        /// </summary>
        DateRange,

        /// <summary>
        /// in (1,2,3)<para></para>
        /// 此时 Value 的值格式为逗号分割：value1,value2,value3... 或者数组
        /// </summary>
        Any,

        /// <summary>
        /// not in (1,2,3)<para></para>
        /// 此时 Value 的值格式为逗号分割：value1,value2,value3... 或者数组
        /// </summary>
        NotAny,

        /// <summary>
        /// 自定义解析，此时 Field 为反射信息，Value 为静态方法的参数(string/Expression)<para></para>
        /// 示范：{ Operator: "Custom", Field: "RawSql webapp1.DynamicFilterCustom,webapp1", Value: "(id,name) in ((1,'k'),(2,'m'))" }<para></para>
        /// 注意：使用者自己承担【注入风险】<para></para>
        /// 静态方法定义示范：<para></para>
        /// namespace webapp1<para></para>
        /// {<para></para>
        /// public class DynamicFilterCustom<para></para>
        /// {<para></para>
        /// [DynamicFilterCustom]<para></para>
        /// public static string RawSql(object sender, string value) => value;<para></para>
        /// }<para></para>
        /// }<para></para>
        /// </summary>
        Custom
    }

    /// <summary>
    /// 授权 DynamicFilter 支持 Custom 自定义解析
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DynamicFilterCustomAttribute : Attribute
    { }
}