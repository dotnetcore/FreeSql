using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface IInsertOrUpdate<T1> where T1 : class
    {

        /// <summary>
        /// 指定事务对象
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        IInsertOrUpdate<T1> WithTransaction(DbTransaction transaction);
        /// <summary>
        /// 指定事务对象
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        IInsertOrUpdate<T1> WithConnection(DbConnection connection);
        /// <summary>
        /// 命令超时设置(秒)
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        IInsertOrUpdate<T1> CommandTimeout(int timeout);

        /// <summary>
        /// 添加或更新，设置实体
        /// </summary>
        /// <param name="source">实体</param>
        /// <returns></returns>
        IInsertOrUpdate<T1> SetSource(T1 source);
        /// <summary>
        /// 添加或更新，设置实体集合
        /// </summary>
        /// <param name="source">实体集合</param>
        /// <returns></returns>
        IInsertOrUpdate<T1> SetSource(IEnumerable<T1> source);

        /// <summary>
        /// 当记录存在时，什么都不做<para></para>
        /// 换句话：只有记录不存在时才插入
        /// </summary>
        /// <returns></returns>
        IInsertOrUpdate<T1> IfExistsDoNothing();

        /// <summary>
        /// 当记录存在时，指定只更新的字段，UpdateColumns(a => a.Name) | UpdateColumns(a => new{a.Name,a.Time}) | UpdateColumns(a => new[]{"name","time"})
        /// </summary>
        /// <param name="columns">lambda选择列</param>
        /// <returns></returns>
        IInsertOrUpdate<T1> UpdateColumns(Expression<Func<T1, object>> columns);
        /// <summary>
        /// 当记录存在时，指定只更新的字段
        /// </summary>
        /// <param name="columns">属性名，或者字段名</param>
        /// <returns></returns>
        IInsertOrUpdate<T1> UpdateColumns(string[] columns);

        /// <summary>
        /// 设置表名规则，可用于分库/分表，参数1：默认表名；返回值：新表名；
        /// </summary>
        /// <param name="tableRule"></param>
        /// <returns></returns>
        IInsertOrUpdate<T1> AsTable(Func<string, string> tableRule);
        /// <summary>
        /// 动态Type，在使用 Update&lt;object&gt; 后使用本方法，指定实体类型
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        IInsertOrUpdate<T1> AsType(Type entityType);
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

#if net40
#else
        Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default);
#endif
    }
}