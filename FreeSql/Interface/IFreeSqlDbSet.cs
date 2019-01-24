using FreeSql;
using System.Collections.Generic;

public interface IFreeSqlDbSet<TEntity> where TEntity:class
{
    /// <summary>
    /// 插入数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    IInsert<TEntity> Insert();
    /// <summary>
    /// 插入数据，传入实体
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    IInsert<TEntity> Insert(TEntity source);
    /// <summary>
    /// 插入数据，传入实体集合
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    IInsert<TEntity> Insert(IEnumerable<TEntity> source);

    /// <summary>
    /// 修改数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    IUpdate<TEntity> Update();
    /// <summary>
    /// 修改数据，传入动态对象如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
    /// <returns></returns>
    IUpdate<TEntity> Update(object dywhere);

    /// <summary>
    /// 查询数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    ISelect<TEntity> Select();
    /// <summary>
    /// 查询数据，传入动态对象如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
    /// <returns></returns>
    ISelect<TEntity> Select(object dywhere);

    /// <summary>
    /// 删除数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    IDelete<TEntity> Delete();
    /// <summary>
    /// 删除数据，传入动态对象如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
    /// <returns></returns>
    IDelete<TEntity> Delete(object dywhere);
}
