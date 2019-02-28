using FreeSql;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;

public static class IFreeSqlExtenssions {

	/// <summary>
	/// 返回默认仓库类
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	/// <param name="that"></param>
	/// <returns></returns>
	public static DefaultRepository<TEntity, TKey> GetRepository<TEntity, TKey>(this IFreeSql that) where TEntity : class {

		return dicGetRepository.GetOrAdd(typeof(TEntity), type1 => new ConcurrentDictionary<Type, IRepository>())
			.GetOrAdd(typeof(TKey), type2 => new DefaultRepository<TEntity, TKey>(that)) as DefaultRepository<TEntity, TKey>;
	}
	static ConcurrentDictionary<Type, ConcurrentDictionary<Type, IRepository>> dicGetRepository = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, IRepository>>();

	/// <summary>
	/// 返回仓库类，适用 Insert 方法无须返回插入的数据
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <param name="that"></param>
	/// <returns></returns>
	public static GuidRepository<TEntity> GetGuidRepository<TEntity>(this IFreeSql that) where TEntity : class {

		return dicGetGuidRepository.GetOrAdd(typeof(TEntity), type1 => new GuidRepository<TEntity>(that)) as GuidRepository<TEntity>;
	}
	static ConcurrentDictionary<Type, IRepository> dicGetGuidRepository = new ConcurrentDictionary<Type, IRepository>();
}