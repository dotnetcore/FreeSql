using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Extensions.EfCoreFluentApi;
using FreeSql.Internal.CommonProvider;

partial class FreeSqlDbContextExtensions
{
    /// <summary>
    /// EFCore 95% 相似的 FluentApi 扩展方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="codeFirst"></param>
    /// <param name="modelBuilder"></param>
    /// <returns></returns>
    public static ICodeFirst Entity<T>(this ICodeFirst codeFirst, Action<EfCoreTableFluent<T>> modelBuilder)
    {
        var cf = codeFirst as CodeFirstProvider;
        codeFirst.ConfigEntity<T>(tf => modelBuilder(new EfCoreTableFluent<T>(cf._orm, tf)));
        return codeFirst;
    }
    /// <summary>
    /// EFCore 95% 相似的 FluentApi 扩展方法
    /// </summary>
    /// <param name="codeFirst"></param>
    /// <param name="entityType">实体类型</param>
    /// <param name="modelBuilder"></param>
    /// <returns></returns>
    public static ICodeFirst Entity(this ICodeFirst codeFirst, Type entityType, Action<EfCoreTableFluent> modelBuilder)
    {
        var cf = codeFirst as CodeFirstProvider;
        codeFirst.ConfigEntity(entityType, tf => modelBuilder(new EfCoreTableFluent(cf._orm, tf, entityType)));
        return codeFirst;
    }

    public static ICodeFirst ApplyConfiguration<TEntity>(this ICodeFirst codeFirst, IEntityTypeConfiguration<TEntity> configuration) where TEntity : class
    {
        return codeFirst.Entity<TEntity>(eb =>
        {
            configuration.Configure(eb);
        });
    }
#if net40
#else
    static IEnumerable<MethodInfo> GetExtensionMethods(this Assembly assembly, Type extendedType)
    {
        var query = from type in assembly.GetTypes()
                    where !type.IsGenericType && !type.IsNested
                    from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    where method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false)
                    where method.GetParameters()[0].ParameterType == extendedType
                    select method;
        return query;
    }

    /// <summary>
    /// 根据Assembly扫描所有继承IEntityTypeConfiguration&lt;T&gt;的配置类
    /// </summary>
    /// <param name="codeFirst"></param>
    /// <param name="assembly"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static ICodeFirst ApplyConfigurationsFromAssembly(this ICodeFirst codeFirst, Assembly assembly, Func<Type, bool> predicate = null)
    {
        IEnumerable<TypeInfo> typeInfos = assembly.DefinedTypes.Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition);

        MethodInfo methodInfo = typeof(FreeSqlDbContextExtensions).Assembly.GetExtensionMethods(typeof(ICodeFirst))
            .Single((e) => e.Name == "Entity" && e.ContainsGenericParameters);

        if (methodInfo == null) return codeFirst;

        foreach (TypeInfo constructibleType in typeInfos)
        {
            if (constructibleType.GetConstructor(Type.EmptyTypes) == null || predicate != null && !predicate(constructibleType))
            {
                continue;
            }

            foreach (var @interface in constructibleType.GetInterfaces())
            {
                if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                {
                    var type = @interface.GetGenericArguments().First();
                    var efFluentType = typeof(EfCoreTableFluent<>).MakeGenericType(type);
                    var actionType = typeof(Action<>).MakeGenericType(efFluentType);

                    //1.需要实体和Configuration配置
                    //codeFirst.Entity<ToDoItem>(eb =>
                    //{
                    //    new ToDoItemConfiguration().Configure(eb);
                    //});

                    //2.需要实体
                    //Action<EfCoreTableFluent<ToDoItem>> x = new Action<EfCoreTableFluent<ToDoItem>>(e =>
                    //{
                    //    object o = Activator.CreateInstance(constructibleType);
                    //    constructibleType.GetMethod("ApplyConfiguration")?.Invoke(o, new object[1] { e });
                    //});
                    //codeFirst.Entity<ToDoItem>(x);

                    //3.实现动态调用泛型委托
                    DelegateBuilder delegateBuilder = new DelegateBuilder(constructibleType);
                    MethodInfo applyconfigureMethod = delegateBuilder.GetType().GetMethod("ApplyConfiguration")?.MakeGenericMethod(type);
                    if (applyconfigureMethod == null) continue;
                    Delegate @delegate = Delegate.CreateDelegate(actionType, delegateBuilder, applyconfigureMethod);

                    methodInfo.MakeGenericMethod(type).Invoke(null, new object[2]
                    {
                        codeFirst,
                        @delegate
                    });

                }
            }
        }

        return codeFirst;
    }
    class DelegateBuilder
    {
        private readonly Type type;

        public DelegateBuilder(Type type)
        {
            this.type = type;
        }
        public void ApplyConfiguration<T>(EfCoreTableFluent<T> ex)
        {
            object o = Activator.CreateInstance(type);
            type.GetMethod("Configure")?.Invoke(o, new object[1] { ex });
        }
    }
#endif
}
