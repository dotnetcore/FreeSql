using System;
using System.Linq.Expressions;

namespace FreeSql
{

    public interface IRepositoryUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// 在工作单元内创建默认仓库类，工作单元下的仓储操作具有事务特点
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="filter">数据过滤 + 验证</param>
        /// <returns></returns>
        IBaseRepository<TEntity, TKey> GetRepository<TEntity, TKey>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class;

        /// <summary>
        /// 在工作单元内创建联合主键的仓储类，工作单元下的仓储操作具有事务特点
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="filter">数据过滤 + 验证</param>
        /// <returns></returns>
        IBaseRepository<TEntity> GetRepository<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class;

        /// <summary>
        /// 在工作单元内创建仓库类，工作单元下的仓储操作具有事务特点
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="filter">数据过滤 + 验证</param>
        /// <param name="asTable">分表规则，参数：旧表名；返回：新表名 https://github.com/2881099/FreeSql/wiki/Repository</param>
        /// <returns></returns>
        IBaseRepository<TEntity, Guid> GetGuidRepository<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<string, string> asTable = null) where TEntity : class;
    }

    class RepositoryUnitOfWork : UnitOfWork, IRepositoryUnitOfWork
    {

        public RepositoryUnitOfWork(IFreeSql fsql) : base(fsql)
        {
        }

        public IBaseRepository<TEntity, Guid> GetGuidRepository<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<string, string> asTable = null) where TEntity : class
        {
            var repo = new GuidRepository<TEntity>(_fsql, filter, asTable);
            repo.UnitOfWork = this;
            return repo;
        }

        public IBaseRepository<TEntity, TKey> GetRepository<TEntity, TKey>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            var repo = new DefaultRepository<TEntity, TKey>(_fsql, filter);
            repo.UnitOfWork = this;
            return repo;
        }

        public IBaseRepository<TEntity> GetRepository<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            var repo = new DefaultRepository<TEntity, int>(_fsql, filter);
            repo.UnitOfWork = this;
            return repo;
        }
    }
}
