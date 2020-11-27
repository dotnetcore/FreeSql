using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{

    public interface IBaseRepository : IDisposable
    {
        Type EntityType { get; }
        IUnitOfWork UnitOfWork { get; set; }
        IFreeSql Orm { get; }

        /// <summary>
        /// 动态Type，在使用 Repository&lt;object&gt; 后使用本方法，指定实体类型
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        void AsType(Type entityType);
        /// <summary>
        /// 分表规则，参数：旧表名；返回：新表名 https://github.com/2881099/FreeSql/wiki/Repository
        /// </summary>
        /// <param name="rule"></param>
        void AsTable(Func<string, string> rule);

        /// <summary>
        /// 设置 DbContext 选项
        /// </summary>
        DbContextOptions DbContextOptions { get; set; }
    }

    public interface IBaseRepository<TEntity> : IBaseRepository
        where TEntity : class
    {
        IDataFilter<TEntity> DataFilter { get; }
        ISelect<TEntity> Select { get; }

        ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp);
        ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp);

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
        IBaseRepository<TEntity> AttachOnlyPrimary(TEntity data);

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
        int Delete(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 开始编辑数据，然后调用方法 EndEdit 分析出添加、修改、删除 SQL 语句进行执行<para></para>
        /// 场景：winform 加载表数据后，一顿添加、修改、删除操作之后，最后才点击【保存】<para></para><para></para>
        /// 示例：https://github.com/dotnetcore/FreeSql/issues/397<para></para>
        /// 注意：* 本方法只支持单表操作，不支持导航属性级联保存
        /// </summary>
        /// <param name="data"></param>
        void BeginEdit(List<TEntity> data);
        /// <summary>
        /// 完成编辑数据，进行保存动作<para></para>
        /// 该方法根据 BeginEdit 传入的数据状态分析出添加、修改、删除 SQL 语句<para></para>
        /// 注意：* 本方法只支持单表操作，不支持导航属性级联保存
        /// </summary>
        /// <param name="data">可选参数：手工传递最终的 data 值进行对比<para></para>默认：如果不传递，则使用 BeginEdit 传入的 data 引用进行对比</param>
        /// <returns></returns>
        int EndEdit(List<TEntity> data = null);

#if net40
#else
        Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default);

        Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default);
        Task<TEntity> InsertOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task SaveManyAsync(TEntity entity, string propertyName, CancellationToken cancellationToken = default);

        Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<int> DeleteAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default);
        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
#endif
    }

    public interface IBaseRepository<TEntity, TKey> : IBaseRepository<TEntity>
        where TEntity : class
    {
        TEntity Get(TKey id);
        TEntity Find(TKey id);
        int Delete(TKey id);

#if net40
#else
        Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default);
        Task<TEntity> FindAsync(TKey id, CancellationToken cancellationToken = default);
        Task<int> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
#endif
    }
}