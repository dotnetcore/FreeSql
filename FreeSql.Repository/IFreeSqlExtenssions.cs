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

		if (filter != null) return new DefaultRepository<TEntity, TKey>(that, filter);
		return dicGetRepository
			.GetOrAdd(typeof(TEntity), key1 => new ConcurrentDictionary<Type, IRepository>())
			.GetOrAdd(typeof(TKey), key2 => new DefaultRepository<TEntity, TKey>(that, null)) as DefaultRepository<TEntity, TKey>;
	}
	static ConcurrentDictionary<Type,
		ConcurrentDictionary<Type,
			IRepository>
		> dicGetRepository = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, IRepository>>();

	/// <summary>
	/// 返回仓库类，适用 Insert 方法无须返回插入的数据
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <param name="that"></param>
	/// <param name="filter">数据过滤 + 验证</param>
	/// <param name="asTable">分表规则，参数：旧表名；返回：新表名 https://github.com/2881099/FreeSql/wiki/Repository</param>
	/// <returns></returns>
	public static GuidRepository<TEntity> GetGuidRepository<TEntity>(this IFreeSql that, Expression<Func<TEntity, bool>> filter = null, Func<string, string> asTable = null) where TEntity : class {

		if (filter != null || asTable != null) return new GuidRepository<TEntity>(that, filter, asTable);
		return dicGetGuidRepository
			.GetOrAdd(typeof(TEntity), key1 => new GuidRepository<TEntity>(that, null, null)) as GuidRepository<TEntity>;
	}
	static ConcurrentDictionary<Type, IRepository> dicGetGuidRepository = new ConcurrentDictionary<Type, IRepository>();
}