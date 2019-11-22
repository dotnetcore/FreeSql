using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace FreeSql.DataAnnotations
{
    /// <summary>
    /// 自定义表达式函数解析<para></para>
    /// 注意：请使用静态扩展类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExpressionCallAttribute : Attribute
    {
    }

    public class ExpressionCallContext
    {
        /// <summary>
        /// 数据库类型，可用于适配多种数据库环境
        /// </summary>
        public DataType DataType { get; internal set; }

        /// <summary>
        /// 已解析的表达式中参数内容
        /// </summary>
        public Dictionary<string, string> Values { get; } = new Dictionary<string, string>();

        /// <summary>
        /// 主对象的参数化对象，可重塑其属性
        /// </summary>
        public DbParameter DbParameter { get; internal set; }
    }
}
