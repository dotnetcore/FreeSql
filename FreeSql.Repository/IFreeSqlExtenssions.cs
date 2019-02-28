using FreeSql;
using System;
using System.Collections.Generic;
using System.Text;

public static class IFreeSqlExtenssions {

	/// <summary>
	/// 返回默认仓库类
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	/// <param name="that"></param>
	/// <returns></returns>
	public static IRepository<TEntity, TKey> GetRepository<TEntity, TKey>(this IFreeSql that) where TEntity : class {

		return new DefaultRepository<TEntity, TKey>(that);
	}

	/// <summary>
	/// 返回仓库类，适用 Insert 方法无须返回插入的数据
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <param name="that"></param>
	/// <returns></returns>
	public static IRepository<TEntity, Guid> GetGuidRepository<TEntity>(this IFreeSql that) where TEntity : class {

		return new GuidRepository<TEntity>(that);
	}
}