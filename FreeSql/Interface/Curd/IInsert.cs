using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface IInsert<T1> where T1 : class
    {

        /// <summary>
        /// 指定事务对象
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        IInsert<T1> WithTransaction(DbTransaction transaction);
        /// <summary>
        /// 指定事务对象
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        IInsert<T1> WithConnection(DbConnection connection);

        /// <summary>
        /// 追加准备插入的实体
        /// </summary>
        /// <param name="source">实体</param>
        /// <returns></returns>
        IInsert<T1> AppendData(T1 source);
        /// <summary>
        /// 追加准备插入的实体
        /// </summary>
        /// <param name="source">实体</param>
        /// <returns></returns>
        IInsert<T1> AppendData(T1[] source);
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
        /// 指定可插入自增字段
        /// </summary>
        /// <returns></returns>
        IInsert<T1> InsertIdentity();

        /// <summary>
        /// 不使用参数化，可通过 IFreeSql.CodeFirst.IsNotCommandParameter 全局性设置
        /// </summary>
        /// <returns></returns>
        IInsert<T1> NoneParameter();

        /// <summary>
        /// 批量执行选项设置，一般不需要使用该方法<para></para>
        /// 各数据库 values, parameters 限制不一样，默认设置：<para></para>
        /// MySql 5000 3000<para></para>
        /// PostgreSQL 5000 3000<para></para>
        /// SqlServer 1000 2100<para></para>
        /// Oracle 500 999<para></para>
        /// Sqlite 5000 999<para></para>
        /// 若没有事务传入，内部(默认)会自动开启新事务，保证拆包执行的完整性。
        /// </summary>
        /// <param name="valuesLimit">指定根据 values 数量拆分执行</param>
        /// <param name="parameterLimit">指定根据 parameters 数量拆分执行</param>
        /// <param name="autoTransaction">是否自动开启事务</param>
        /// <returns></returns>
        IInsert<T1> BatchOptions(int valuesLimit, int parameterLimit, bool autoTransaction = true);

        /// <summary>
        /// 设置表名规则，可用于分库/分表，参数1：默认表名；返回值：新表名；
        /// </summary>
        /// <param name="tableRule"></param>
        /// <returns></returns>
        IInsert<T1> AsTable(Func<string, string> tableRule);
        /// <summary>
        /// 动态Type，在使用 Insert&lt;object&gt; 后使用本方法，指定实体类型
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        IInsert<T1> AsType(Type entityType);
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
        /// <summary>
        /// 执行SQL语句，返回自增值
        /// </summary>
        /// <returns></returns>
        long ExecuteIdentity();
        /// <summary>
        /// 执行SQL语句，返回插入后的记录
        /// </summary>
        /// <returns></returns>
        List<T1> ExecuteInserted();

        /// <summary>
        /// 返回 DataTable 以便做 BulkCopy 数据做准备<para></para>
        /// 此方法会处理：<para></para>
        /// 类型、表名、字段名映射<para></para>
        /// IgnoreColumns、InsertColumns
        /// </summary>
        /// <returns></returns>
        DataTable ToDataTable();

#if net40
#else
        Task<int> ExecuteAffrowsAsync();
        Task<long> ExecuteIdentityAsync();
        Task<List<T1>> ExecuteInsertedAsync();
#endif
    }
}