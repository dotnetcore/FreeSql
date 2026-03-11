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
		/// 分表规则，参数：实体类型、旧表名；返回：新表名 https://github.com/2881099/FreeSql/wiki/Repository
		/// </summary>
		/// <param name="rule"></param>
		void AsTable(Func<Type, string, string> rule);

		/// <summary>
		/// 设置 DbContext 选项
		/// </summary>
		DbContextOptions DbContextOptions { get; set; }
        /// <summary>
        /// GlobalFilter 禁用/启用控制
        /// </summary>
        RepositoryDataFilter DataFilter { get; }
    }

    public interface IBaseRepository<TEntity> : IBaseRepository
        where TEntity : class
    {
        /// <summary>
        /// 查询数据
        /// </summary>
        ISelect<TEntity> Select { get; }

        /// <summary>
        /// 筛选数据
        /// </summary>
        /// <param name="exp">Lambda 筛选表达式</param>
        /// <returns>筛选后的查询对象</returns>
        ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp);
        ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp);

        /// <summary>
        /// 插入实体
        /// </summary>
        /// <param name="entity">需要插入的实体对象</param>
        /// <returns>插入后的实体对象</returns>
        TEntity Insert(TEntity entity);
        
        /// <summary>
        /// 批量插入实体
        /// </summary>
        /// <param name="entities">需要插入的实体对象集合</param>
        /// <returns>插入后的实体对象集合</returns>
        List<TEntity> Insert(IEnumerable<TEntity> entities);

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
        /// <summary>
        /// 比较实体，计算出值发生变化的属性，以及属性变化的前后值
        /// </summary>
        /// <param name="newdata">最新的实体对象，它将与附加实体的状态对比</param>
        /// <returns>key: 属性名, value: [旧值, 新值]</returns>
        Dictionary<string, object[]> CompareState(TEntity newdata);

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="entity">需要更新的实体对象</param>
        /// <returns>返回受影响的行数</returns>
        int Update(TEntity entity);
        
        /// <summary>
        /// 更新实体集合
        /// </summary>
        /// <param name="entities">需要更新的实体对象集合</param>
        /// <returns>返回受影响的行数</returns>
        int Update(IEnumerable<TEntity> entities);

        /// <summary>
        /// 更新实体，不存在则插入
        /// </summary>
        /// <param name="entity">需要插入或更新的实体对象</param>
        /// <returns>受影响的行数</returns>
        TEntity InsertOrUpdate(TEntity entity);

        /// <summary>
        /// DIY 方式更新
        /// </summary>
        IUpdate<TEntity> UpdateDiy { get; }

        /// <summary>
        /// 根据主键删除实体
        /// </summary>
        /// <param name="entity">需要删除的实体</param>
        /// <returns>受影响的行数</returns>
        int Delete(TEntity entity);
        
        /// <summary>
        /// 批量删除实体，一样是根据主键删除
        /// </summary>
        /// <param name="entities">需要删除的实体对象集合</param>
        /// <returns>受影响的行数</returns>
        int Delete(IEnumerable<TEntity> entities);
        
        /// <summary>
        /// 根据 Lambda 表达式删除实体
        /// </summary>
        /// <param name="predicate">Lambda 表达式，用于筛选需要删除的实体</param>
        /// <returns>受影响的行数</returns>
        int Delete(Expression<Func<TEntity, bool>> predicate);
        /// <summary>
        /// 根据设置的 OneToOne/OneToMany/ManyToMany 导航属性，级联查询所有的数据库记录，删除并返回它们
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        List<object> DeleteCascadeByDatabase(Expression<Func<TEntity, bool>> predicate);

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
        Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        Task<TEntity> InsertOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task SaveManyAsync(TEntity entity, string propertyName, CancellationToken cancellationToken = default);

        Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<int> DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task<List<object>> DeleteCascadeByDatabaseAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
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