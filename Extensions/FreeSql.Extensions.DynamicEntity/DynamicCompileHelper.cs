using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql.Extensions.DynamicEntity
{
    /// <summary>
    /// 动态创建对象帮助类
    /// </summary>
    public class DynamicCompileHelper
    {
        /// <summary>
        /// 动态构建Class - Type
        /// </summary>
        /// <returns></returns>
        public static DynamicCompileBuilder DynamicBuilder()
        {
            return new DynamicCompileBuilder();
        }

        /// <summary>
        /// 委托缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, Delegate>
            DelegateCache = new ConcurrentDictionary<string, Delegate>();

        /// <summary>
        /// 设置动态对象的属性值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="porpertys"></param>
        /// <returns></returns>
        public static object CreateObjectByType(Type type, Dictionary<string, object> porpertys)
        {
            if (type == null)
                return null;
            object istance = Activator.CreateInstance(type);
            if (istance == null)
                return null;
            //根据key确定缓存
            var cacheKey = $"{type.GetHashCode()}{porpertys.GetHashCode()}";
            var dynamicDelegate = DelegateCache.GetOrAdd(cacheKey, key =>
            {
                //表达式目录树构建委托
                var typeParam = Expression.Parameter(type);
                var dicParamType = typeof(Dictionary<string, object>);
                var dicParam = Expression.Parameter(dicParamType);
                var exps = new List<Expression>();
                var tempRef = Expression.Variable(typeof(object));
                foreach (var pinfo in porpertys)
                {
                    var propertyInfo = type.GetProperty(pinfo.Key);
                    if (propertyInfo == null)
                        continue;
                    var propertyName = Expression.Constant(pinfo.Key, typeof(string));
                    exps.Add(Expression.Call(dicParam, dicParamType.GetMethod("TryGetValue"), propertyName, tempRef));
                    exps.Add(Expression.Assign(Expression.MakeMemberAccess(typeParam, propertyInfo),
                        Expression.Convert(tempRef, propertyInfo.PropertyType)));
                    exps.Add(Expression.Assign(tempRef, Expression.Default(typeof(object))));
                }

                var returnTarget = Expression.Label(type);
                exps.Add(Expression.Return(returnTarget, typeParam));
                exps.Add(Expression.Label(returnTarget, Expression.Default(type)));
                var block = Expression.Block(new[] { tempRef }, exps);
                var @delegate = Expression.Lambda(block, typeParam, dicParam).Compile();
                return @delegate;
            });
            var dynamicInvoke = dynamicDelegate.DynamicInvoke(istance, porpertys);
            return dynamicInvoke;
        }
    }
}