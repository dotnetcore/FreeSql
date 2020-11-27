using FreeSql.DatabaseModel;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{
    public partial interface IAdo
    {
        /// <summary>
        /// 主库连接池
        /// </summary>
        IObjectPool<DbConnection> MasterPool { get; }
        /// <summary>
        /// 从库连接池
        /// </summary>
        List<IObjectPool<DbConnection>> SlavePools { get; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        DataType DataType { get; }
        /// <summary>
        /// UseConnectionString 时候的值
        /// </summary>
        string ConnectionString { get; }
        /// <summary>
        /// UseSalve 时候的值
        /// </summary>
        string[] SlaveConnectionStrings { get; }
        /// <summary>
        /// 唯一标识
        /// </summary>
        Guid Identifier { get; }

        #region 事务
        /// <summary>
        /// 开启事务（不支持异步）
        /// </summary>
        /// <param name="handler">事务体 () => {}</param>
        void Transaction(Action handler);
        /// <summary>
        /// 开启事务（不支持异步）
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <param name="handler">事务体 () => {}</param>
        void Transaction(IsolationLevel isolationLevel, Action handler);
        /// <summary>
        /// 当前线程的事务
        /// </summary>
        DbTransaction TransactionCurrentThread { get; }
        #endregion

        /// <summary>
        /// 将 new { id = 1 } 或者 Dictionary&lt;string, object&gt; 转换为 DbParameter[]
        /// </summary>
        /// <param name="obj">new { id = 1 } 或者 Dictionary&lt;string, object&gt;</param>
        /// <returns></returns>
        DbParameter[] GetDbParamtersByObject(object obj);

        /// <summary>
        /// SQL 命令执行类，fsql.Ado.CommandFluent("select * from user where age > @age", new { age = 25 })<para></para>
        /// .WithConnection(connection)<para></para>
        /// .WithTransaction(transaction)<para></para>
        /// .WithParameter("age", 25)<para></para>
        /// .WithParameter("id", 11)<para></para>
        /// .CommandType(CommandType.Text)<para></para>
        /// .CommandTimeout(60)<para></para>
        /// .Query&lt;T&gt;(); 或者 ExecuteNonQuery/ExecuteScalar/ExecuteDataTable/ExecuteDataSet/ExecuteArray
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        AdoCommandFluent CommandFluent(string cmdText, object parms = null);

        /// <summary>
        /// 测试数据库是否连接正确，本方法执行如下命令：<para></para>
        /// MySql/SqlServer/PostgreSQL/达梦/人大金仓/神通: SELECT 1<para></para>
        /// Oracle: SELECT 1 FROM dual<para></para>
        /// </summary>
        /// <param name="commandTimeout">命令超时设置(秒)</param>
        /// <returns>true: 成功, false: 失败</returns>
        bool ExecuteConnectTest(int commandTimeout = 0);

        /// <summary>
        /// 查询，若使用读写分离，查询【从库】条件cmdText.StartsWith("SELECT ")，否则查询【主库】
        /// </summary>
        /// <param name="fetchHandler"></param>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        void ExecuteReader(Action<FetchCallbackArgs<DbDataReader>> fetchHandler, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        void ExecuteReader(DbTransaction transaction, Action<FetchCallbackArgs<DbDataReader>> fetchHandler, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        void ExecuteReader(DbConnection connection, DbTransaction transaction, Action<FetchCallbackArgs<DbDataReader>> fetchHandler, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms);
        /// <summary>
        /// 查询，ExecuteReader(dr => {}, "select * from user where age > @age", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="fetchHandler"></param>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        void ExecuteReader(Action<FetchCallbackArgs<DbDataReader>> fetchHandler, string cmdText, object parms = null);
        void ExecuteReader(DbTransaction transaction, Action<FetchCallbackArgs<DbDataReader>> fetchHandler, string cmdText, object parms = null);
        void ExecuteReader(DbConnection connection, DbTransaction transaction, Action<FetchCallbackArgs<DbDataReader>> fetchHandler, string cmdText, object parms = null);
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        object[][] ExecuteArray(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        object[][] ExecuteArray(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        object[][] ExecuteArray(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms);
        /// <summary>
        /// 查询，ExecuteArray("select * from user where age > @age", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        object[][] ExecuteArray(string cmdText, object parms = null);
        object[][] ExecuteArray(DbTransaction transaction, string cmdText, object parms = null);
        object[][] ExecuteArray(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null);
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        DataSet ExecuteDataSet(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        DataSet ExecuteDataSet(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        DataSet ExecuteDataSet(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms);
        /// <summary>
        /// 查询，ExecuteDataSet("select * from user where age > @age; select 2", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        DataSet ExecuteDataSet(string cmdText, object parms = null);
        DataSet ExecuteDataSet(DbTransaction transaction, string cmdText, object parms = null);
        DataSet ExecuteDataSet(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null);
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        DataTable ExecuteDataTable(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        DataTable ExecuteDataTable(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        DataTable ExecuteDataTable(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms);
        /// <summary>
        /// 查询，ExecuteDataTable("select * from user where age > @age", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        DataTable ExecuteDataTable(string cmdText, object parms = null);
        DataTable ExecuteDataTable(DbTransaction transaction, string cmdText, object parms = null);
        DataTable ExecuteDataTable(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null);
        /// <summary>
        /// 在【主库】执行
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        int ExecuteNonQuery(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        int ExecuteNonQuery(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms);
        /// <summary>
        /// 在【主库】执行，ExecuteNonQuery("delete from user where age > @age", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        int ExecuteNonQuery(string cmdText, object parms = null);
        int ExecuteNonQuery(DbTransaction transaction, string cmdText, object parms = null);
        int ExecuteNonQuery(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null);
        /// <summary>
        /// 在【主库】执行
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        object ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        object ExecuteScalar(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        object ExecuteScalar(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms);
        /// <summary>
        /// 在【主库】执行，ExecuteScalar("select 1 from user where age > @age", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        object ExecuteScalar(string cmdText, object parms = null);
        object ExecuteScalar(DbTransaction transaction, string cmdText, object parms = null);
        object ExecuteScalar(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null);

        /// <summary>
        /// 执行SQL返回对象集合，Query&lt;User&gt;("select * from user where age > @age", new SqlParameter { ParameterName = "age", Value = 25 })
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        List<T> Query<T>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        List<T> Query<T>(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        List<T> Query<T>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms);
        List<T> Query<T>(Type resultType, DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms);
        T QuerySingle<T>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        /// <summary>
        /// 执行SQL返回对象集合，Query&lt;User&gt;("select * from user where age > @age", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        List<T> Query<T>(string cmdText, object parms = null);
        List<T> Query<T>(DbTransaction transaction, string cmdText, object parms = null);
        List<T> Query<T>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null);
        T QuerySingle<T>(string cmdText, object parms = null);

        /// <summary>
        /// 执行SQL返回对象集合，Query&lt;User&gt;("select * from user where age > @age; select * from address", new SqlParameter { ParameterName = "age", Value = 25 })
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        NativeTuple<List<T1>, List<T2>> Query<T1, T2>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        NativeTuple<List<T1>, List<T2>> Query<T1, T2>(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        NativeTuple<List<T1>, List<T2>> Query<T1, T2>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms);
        /// <summary>
        /// 执行SQL返回对象集合，Query&lt;User&gt;("select * from user where age > @age; select * from address", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        NativeTuple<List<T1>, List<T2>> Query<T1, T2>(string cmdText, object parms = null);
        NativeTuple<List<T1>, List<T2>> Query<T1, T2>(DbTransaction transaction, string cmdText, object parms = null);
        NativeTuple<List<T1>, List<T2>> Query<T1, T2>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null);

        NativeTuple<List<T1>, List<T2>, List<T3>> Query<T1, T2, T3>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        NativeTuple<List<T1>, List<T2>, List<T3>> Query<T1, T2, T3>(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        NativeTuple<List<T1>, List<T2>, List<T3>> Query<T1, T2, T3>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms);
        NativeTuple<List<T1>, List<T2>, List<T3>> Query<T1, T2, T3>(string cmdText, object parms = null);
        NativeTuple<List<T1>, List<T2>, List<T3>> Query<T1, T2, T3>(DbTransaction transaction, string cmdText, object parms = null);
        NativeTuple<List<T1>, List<T2>, List<T3>> Query<T1, T2, T3>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null);
        NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>> Query<T1, T2, T3, T4>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>> Query<T1, T2, T3, T4>(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>> Query<T1, T2, T3, T4>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms);
        NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>> Query<T1, T2, T3, T4>(string cmdText, object parms = null);
        NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>> Query<T1, T2, T3, T4>(DbTransaction transaction, string cmdText, object parms = null);
        NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>> Query<T1, T2, T3, T4>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null);
        NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>> Query<T1, T2, T3, T4, T5>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>> Query<T1, T2, T3, T4, T5>(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>> Query<T1, T2, T3, T4, T5>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms);
        NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>> Query<T1, T2, T3, T4, T5>(string cmdText, object parms = null);
        NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>> Query<T1, T2, T3, T4, T5>(DbTransaction transaction, string cmdText, object parms = null);
        NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>> Query<T1, T2, T3, T4, T5>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null);

#if net40
#else
        #region async
        /// <summary>
        /// 测试数据库是否连接正确，本方法执行如下命令：<para></para>
        /// MySql/SqlServer/PostgreSQL/达梦/人大金仓/神通: SELECT 1<para></para>
        /// Oracle: SELECT 1 FROM dual<para></para>
        /// </summary>
        /// <param name="commandTimeout">命令超时设置(秒)</param>
        /// <param name="cancellationToken"></param>
        /// <returns>true: 成功, false: 失败</returns>
        Task<bool> ExecuteConnectTestAsync(int commandTimeout = 0, CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询，若使用读写分离，查询【从库】条件cmdText.StartsWith("SELECT ")，否则查询【主库】
        /// </summary>
        /// <param name="readerHander"></param>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <param name="cancellationToken"></param>
        Task ExecuteReaderAsync(Func<FetchCallbackArgs<DbDataReader>, Task> readerHander, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task ExecuteReaderAsync(DbTransaction transaction, Func<FetchCallbackArgs<DbDataReader>, Task> readerHander, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task ExecuteReaderAsync(DbConnection connection, DbTransaction transaction, Func<FetchCallbackArgs<DbDataReader>, Task> readerHander, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        /// <summary>
        /// 查询，ExecuteReaderAsync(dr => {}, "select * from user where age > @age", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="readerHander"></param>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <param name="cancellationToken"></param>
        Task ExecuteReaderAsync(Func<FetchCallbackArgs<DbDataReader>, Task> readerHander, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task ExecuteReaderAsync(DbTransaction transaction, Func<FetchCallbackArgs<DbDataReader>, Task> readerHander, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task ExecuteReaderAsync(DbConnection connection, DbTransaction transaction, Func<FetchCallbackArgs<DbDataReader>, Task> readerHander, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <param name="cancellationToken"></param>
        Task<object[][]> ExecuteArrayAsync(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<object[][]> ExecuteArrayAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<object[][]> ExecuteArrayAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        /// <summary>
        /// 查询，ExecuteArrayAsync("select * from user where age > @age", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<object[][]> ExecuteArrayAsync(string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<object[][]> ExecuteArrayAsync(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<object[][]> ExecuteArrayAsync(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <param name="cancellationToken"></param>
        Task<DataSet> ExecuteDataSetAsync(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<DataSet> ExecuteDataSetAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<DataSet> ExecuteDataSetAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        /// <summary>
        /// 查询，ExecuteDataSetAsync("select * from user where age > @age; select 2", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DataSet> ExecuteDataSetAsync(string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<DataSet> ExecuteDataSetAsync(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<DataSet> ExecuteDataSetAsync(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <param name="cancellationToken"></param>
        Task<DataTable> ExecuteDataTableAsync(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<DataTable> ExecuteDataTableAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<DataTable> ExecuteDataTableAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        /// <summary>
        /// 查询，ExecuteDataTableAsync("select * from user where age > @age", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DataTable> ExecuteDataTableAsync(string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<DataTable> ExecuteDataTableAsync(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<DataTable> ExecuteDataTableAsync(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// 在【主库】执行
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <param name="cancellationToken"></param>
        Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<int> ExecuteNonQueryAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        /// <summary>
        /// 在【主库】执行，ExecuteNonQueryAsync("delete from user where age > @age", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> ExecuteNonQueryAsync(string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<int> ExecuteNonQueryAsync(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<int> ExecuteNonQueryAsync(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// 在【主库】执行
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <param name="cancellationToken"></param>
        Task<object> ExecuteScalarAsync(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<object> ExecuteScalarAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<object> ExecuteScalarAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        /// <summary>
        /// 在【主库】执行，ExecuteScalarAsync("select 1 from user where age > @age", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<object> ExecuteScalarAsync(string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<object> ExecuteScalarAsync(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<object> ExecuteScalarAsync(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行SQL返回对象集合，QueryAsync&lt;User&gt;("select * from user where age > @age", new SqlParameter { ParameterName = "age", Value = 25 })
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<T>> QueryAsync<T>(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<List<T>> QueryAsync<T>(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<List<T>> QueryAsync<T>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<List<T>> QueryAsync<T>(Type resultType, DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<T> QuerySingleAsync<T>(string cmdText, object parms = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// 执行SQL返回对象集合，QueryAsync&lt;User&gt;("select * from user where age > @age", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<T>> QueryAsync<T>(string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<List<T>> QueryAsync<T>(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<List<T>> QueryAsync<T>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<T> QuerySingleAsync<T>(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行SQL返回对象集合，Query&lt;User&gt;("select * from user where age > @age; select * from address", new SqlParameter { ParameterName = "age", Value = 25 })
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<NativeTuple<List<T1>, List<T2>>> QueryAsync<T1, T2>(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>>> QueryAsync<T1, T2>(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>>> QueryAsync<T1, T2>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        /// <summary>
        /// 执行SQL返回对象集合，Query&lt;User, Address&gt;("select * from user where age > @age; select * from address", new { age = 25 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="cmdText"></param>
        /// <param name="parms"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<NativeTuple<List<T1>, List<T2>>> QueryAsync<T1, T2>(string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>>> QueryAsync<T1, T2>(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>>> QueryAsync<T1, T2>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);

        Task<NativeTuple<List<T1>, List<T2>, List<T3>>> QueryAsync<T1, T2, T3>(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>>> QueryAsync<T1, T2, T3>(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>>> QueryAsync<T1, T2, T3>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>>> QueryAsync<T1, T2, T3>(string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>>> QueryAsync<T1, T2, T3>(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>>> QueryAsync<T1, T2, T3>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryAsync<T1, T2, T3, T4>(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryAsync<T1, T2, T3, T4>(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryAsync<T1, T2, T3, T4>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryAsync<T1, T2, T3, T4>(string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryAsync<T1, T2, T3, T4>(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryAsync<T1, T2, T3, T4>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryAsync<T1, T2, T3, T4, T5>(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryAsync<T1, T2, T3, T4, T5>(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryAsync<T1, T2, T3, T4, T5>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryAsync<T1, T2, T3, T4, T5>(string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryAsync<T1, T2, T3, T4, T5>(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryAsync<T1, T2, T3, T4, T5>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default);
        #endregion
#endif
    }
}
