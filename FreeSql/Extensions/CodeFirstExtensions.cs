using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using FreeSql.DataAnnotations;
using FreeSql.Internal;

namespace FreeSql.Extensions
{
#if net40 || NETSTANDARD2_0
#else
    public static class CodeFirstExtensions
    {
        /// <summary>
        /// 动态构建Class Type
        /// </summary>
        /// <returns></returns>
        public static DynamicCompileBuilder DynamicEntity(this ICodeFirst codeFirst, string className,
            TableAttribute tableAttribute)
        {
            return new DynamicCompileBuilder().SetClass(className, tableAttribute);
        }

    }
#endif
}