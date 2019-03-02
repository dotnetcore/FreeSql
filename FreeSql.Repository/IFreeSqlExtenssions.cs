using FreeSql;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Linq.Expressions;

public static class IFreeSqlExtenssions {

	/// <summary>
	/// 返回默认仓库类
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	/// <param name="that"></param>
	/// <param name="filter">数据过滤 + 验证</param>
	/// <returns></returns>
	public static DefaultRepository<TEntity, TKey> GetRepository<TEntity, TKey>(this IFreeSql that, Expression<Func<TEntity, bool>> filter = null) where TEntity : class {

		return dicGetRepository
			.GetOrAdd(typeof(TEntity), key1 => new ConcurrentDictionary<Type, ConcurrentDictionary<string, IRepository>>())
			.GetOrAdd(typeof(TKey), key2 => new ConcurrentDictionary<string, IRepository>())
			.GetOrAdd(string.Concat(filter), key3 => new DefaultRepository<TEntity, TKey>(that, filter)) as DefaultRepository<TEntity, TKey>;
	}
	static ConcurrentDictionary<Type,
		ConcurrentDictionary<Type,
			ConcurrentDictionary<string, IRepository>>
		> dicGetRepository = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, ConcurrentDictionary<string, IRepository>>>();

	/// <summary>
	/// 返回仓库类，适用 Insert 方法无须返回插入的数据
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <param name="that"></param>
	/// <param name="filter">数据过滤 + 验证</param>
	/// <returns></returns>
	public static GuidRepository<TEntity> GetGuidRepository<TEntity>(this IFreeSql that, Expression<Func<TEntity, bool>> filter = null) where TEntity : class {

		return dicGetGuidRepository
			.GetOrAdd(typeof(TEntity), key1 => new ConcurrentDictionary<string, IRepository>())
			.GetOrAdd(string.Concat(filter), key2 => new GuidRepository<TEntity>(that, filter)) as GuidRepository<TEntity>;
	}
	static ConcurrentDictionary<Type,
		ConcurrentDictionary<string, IRepository>> dicGetGuidRepository = new ConcurrentDictionary<Type, ConcurrentDictionary<string, IRepository>>();
}