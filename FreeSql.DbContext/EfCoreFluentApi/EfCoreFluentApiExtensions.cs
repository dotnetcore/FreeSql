using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
}
