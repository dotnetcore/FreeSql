using System;
using System.Linq.Expressions;

namespace FreeSql
{

    public interface IRepositoryUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// 在工作单元内创建联合主键的仓储类，工作单元下的仓储操作具有事务特点
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IBaseRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        IBaseRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class;
    }

    class RepositoryUnitOfWork : UnitOfWork, IRepositoryUnitOfWork
    {

        public RepositoryUnitOfWork(IFreeSql fsql) : base(fsql)
        {
        }

        public IBaseRepository<TEntity> GetRepository<TEntity>() where TEntity : class => GetRepository<TEntity, int>();
        public IBaseRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class
        {
            var repo = new DefaultRepository<TEntity, TKey>(_fsql);
            repo.UnitOfWork = this;
            return repo;
        }
    }
}
