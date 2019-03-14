using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql {
	public interface IUnitOfWork : IDisposable {

		/// <summary>
		/// 在工作单元内创建默认仓库类，工作单元下的仓储操作具有事务特点
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="filter">数据过滤 + 验证</param>
		/// <returns></returns>
		DefaultRepository<TEntity, TKey> GetRepository<TEntity, TKey>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class;

		/// <summary>
		/// 在工作单元内创建仓库类，适用 Insert 方法无须返回插入的数据，工作单元下的仓储操作具有事务特点
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="filter">数据过滤 + 验证</param>
		/// <param name="asTable">分表规则，参数：旧表名；返回：新表名 https://github.com/2881099/FreeSql/wiki/Repository</param>
		/// <returns></returns>
		GuidRepository<TEntity> GetGuidRepository<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<string, string> asTable = null) where TEntity : class;

		void Commit();

		void Rollback();
	}
}
