using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamicBuilder
{
#if net40 || NETSTANDARD2_0
#else
    public static  class TypeExtensions
    {
        /// <summary>
        /// 根据动态构建的Class Type生成实例并进行属性赋值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="porpertys"></param>
        /// <returns></returns>
        public static object CreateDynamicEntityInstance(this Type type, IFreeSql fsql,
            Dictionary<string, object> porpertys)
        {
            return DynamicCompileBuilder.CreateObjectByTypeByCodeFirst(fsql, type, porpertys);
        }

        /// <summary>
        /// 设置对象属性值
        /// </summary>
        /// <param name="fsql"></param>
        /// <returns></returns>
        public static void SetPropertyValue(this Type type, IFreeSql fsql, ref object obj, string propertyName,
            object propertyValue)
        {
            var table = fsql.CodeFirst.GetTableByEntity(obj.GetType());
            table.ColumnsByCs[propertyName].SetValue(obj, propertyValue);
        }
    }
#endif
}
