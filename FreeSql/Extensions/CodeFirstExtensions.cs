using System;
using System.Collections.Generic;
using System.Text;
using FreeSql.DataAnnotations;
using FreeSql.Internal;

namespace FreeSql.Extensions
{
#if net40 || NETSTANDARD2_0
    //不支持
#else
    public static  class CodeFirstExtensions
    {
        /// <summary>
        /// 动态构建Class Type
        /// </summary>
        /// <returns></returns>
        public static DynamicCompileBuilder DynamicEntity(this ICodeFirst codeFirst, string className, TableAttribute tableAttribute)
        {
            return new DynamicCompileBuilder().SetClass(className, tableAttribute);
        }

        /// <summary>
        /// 根据动态构建的Class Type生成实例并进行属性赋值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="porpertys"></param>
        /// <returns></returns>
        public static object CreateDynamicEntityInstance(this Type type,
            Dictionary<string, object> porpertys)
        {
           return DynamicCompileBuilder.CreateObjectByType(type, porpertys);
        }
    }
#endif
}
