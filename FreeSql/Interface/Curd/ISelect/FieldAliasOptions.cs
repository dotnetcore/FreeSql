using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql
{
    public enum FieldAliasOptions
    {
        /// <summary>
        /// 自动产生 as1, as2, as3 .... 字段别名<para></para>
        /// 这种方法可以最大程度防止多表，存在相同字段的问题
        /// </summary>
        AsIndex,

        
        /// <summary>
        /// 使用属性名作为字段别名
        /// </summary>
        AsProperty
    }
}
