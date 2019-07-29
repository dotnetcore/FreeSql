using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 包括 CreateTime/UpdateTime/IsDeleted、CRUD 异步方法、以及 ID 主键定义 的实体基类
/// <para></para>
/// 当 TKey 为 int/long 时，Id 主键被设为自增值主键
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
[Table(DisableSyncStructure = true)]
public abstract class BaseEntityAsync<TEntity, TKey> : BaseEntityAsync<TEntity> where TEntity : class
{
    static BaseEntityAsync()
    {
        var tkeyType = typeof(TKey)?.NullableTypeOrThis();
        if (tkeyType == typeof(int) || tkeyType == typeof(long))
            Orm.CodeFirst.ConfigEntity(typeof(TEntity),
                t => t.Property("Id").IsIdentity(true));
    }

    /// <summary>
    /// 主键
    /// </summary>
    public virtual TKey Id { get; set; }

    /// <summary>
    /// 根据主键值获取数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    async public static Task<TEntity> FindAsync(TKey id)
    {
        var item = await Select.WhereDynamic(id).FirstAsync();
        (item as BaseEntity<TEntity>)?.Attach();
        return item;
    }
}
