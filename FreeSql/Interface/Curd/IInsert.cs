using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql {
	public interface IInsert<T1> where T1 : class {

		/// <summary>
		/// 追加准备插入的实体
		/// </summary>
		/// <param name="source">实体</param>
		/// <returns></returns>
		IInsert<T1> AppendData(T1 source);
		/// <summary>
		/// 追加准备插入的实体集合
		/// </summary>
		/// <param name="source">实体集合</param>
		/// <returns></returns>
		IInsert<T1> AppendData(IEnumerable<T1> source);

		/// <summary>
		/// 只插入的列，InsertColumns(a => a.Name) | InsertColumns(a => new{a.Name,a.Time}) | InsertColumns(a => new[]{"name","time"})
		/// </summary>
		/// <param name="columns">lambda选择列</param>
		/// <returns></returns>
		IInsert<T1> InsertColumns(Expression<Func<T1, object>> columns);
		/// <summary>
		/// 忽略的列，IgnoreColumns(a => a.Name) | IgnoreColumns(a => new{a.Name,a.Time}) | IgnoreColumns(a => new[]{"name","time"})
		/// </summary>
		/// <param name="columns">lambda选择列</param>
		/// <returns></returns>
		IInsert<T1> IgnoreColumns(Expression<Func<T1, object>> columns);

		/// <summary>
		/// 返回即将执行的SQL语句
		/// </summary>
		/// <returns></returns>
		string ToSql();
		/// <summary>
		/// 执行SQL语句，返回影响的行数
		/// </summary>
		/// <returns></returns>
		int ExecuteAffrows();
		Task<int> ExecuteAffrowsAsync();
		/// <summary>
		/// 执行SQL语句，返回自增值
		/// </summary>
		/// <returns></returns>
		long ExecuteIdentity();
		Task<long> ExecuteIdentityAsync();
		/// <summary>
		/// 执行SQL语句，返回插入后的记录
		/// </summary>
		/// <returns></returns>
		List<T1> ExecuteInserted();
		Task<List<T1>> ExecuteInsertedAsync();
	}
}