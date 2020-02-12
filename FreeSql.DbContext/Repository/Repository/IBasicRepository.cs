using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface IBasicRepository<TEntity> : IReadOnlyRepository<TEntity>
        where TEntity : class
    {
        TEntity Insert(TEntity entity);
        List<TEntity> Insert(IEnumerable<TEntity> entitys);

        /// <summary>
        /// 清空状态数据
        /// </summary>
        void FlushState();
        /// <summary>
        /// 附加实体，可用于不查询就更新或删除
        /// </summary>
        /// <param name="entity"></param>
        void Attach(TEntity entity);
        void Attach(IEnumerable<TEntity> entity);
        /// <summary>
        /// 附加实体，并且只附加主键值，可用于不更新属性值为null或默认值的字段
        /// </summary>
        /// <param name="data"></param>
        IBasicRepository<TEntity> AttachOnlyPrimary(TEntity data);

        int Update(TEntity entity);
        int Update(IEnumerable<TEntity> entitys);

        TEntity InsertOrUpdate(TEntity entity);
        /// <summary>
        /// 保存实体的指定 ManyToMany/OneToMany 导航属性（完整对比）<para></para>
        /// 场景：在关闭级联保存功能之后，手工使用本方法<para></para>
        /// 例子：保存商品的 OneToMany 集合属性，SaveMany(goods, "Skus")<para></para>
        /// 当 goods.Skus 为空(非null)时，会删除表中已存在的所有数据<para></para>
        /// 当 goods.Skus 不为空(非null)时，添加/更新后，删除表中不存在 Skus 集合属性的所有记录
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="propertyName">属性名</param>
        void SaveMany(TEntity entity, string propertyName);

        IUpdate<TEntity> UpdateDiy { get; }

        int Delete(TEntity entity);
        int Delete(IEnumerable<TEntity> entitys);

#if net40
#else
        Task<TEntity> InsertAsync(TEntity entity);
        Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys);

        Task<int> UpdateAsync(TEntity entity);
        Task<int> UpdateAsync(IEnumerable<TEntity> entitys);
        Task<TEntity> InsertOrUpdateAsync(TEntity entity);
        Task SaveManyAsync(TEntity entity, string propertyName);

        Task<int> DeleteAsync(TEntity entity);
        Task<int> DeleteAsync(IEnumerable<TEntity> entitys);
#endif
    }

    public interface IBasicRepository<TEntity, TKey> : IBasicRepository<TEntity>, IReadOnlyRepository<TEntity, TKey>
        where TEntity : class
    {
        int Delete(TKey id);

#if net40
#else
        Task<int> DeleteAsync(TKey id);
#endif
    }
}

