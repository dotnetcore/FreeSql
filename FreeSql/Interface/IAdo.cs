using FreeSql.DatabaseModel;
using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql {
	public partial interface IAdo {
		/// <summary>
		/// 主库连接池
		/// </summary>
		ObjectPool<DbConnection> MasterPool { get; }
		/// <summary>
		/// 从库连接池
		/// </summary>
		List<ObjectPool<DbConnection>> SlavePools { get; }
		/// <summary>
		/// 监视数据库命令对象(执行前，调试)
		/// </summary>
		Action<DbCommand> AopCommandExecuting { get; set; }
		/// <summary>
		/// 监视数据库命令对象(执行后，用于监视执行性能)
		/// </summary>
		Action<DbCommand, string> AopCommandExecuted { get; set; }

		#region 事务
		/// <summary>
		/// 开启事务（不支持异步），60秒未执行完将自动提交
		/// </summary>
		/// <param name="handler">事务体 () => {}</param>
		void Transaction(Action handler);
		/// <summary>
		/// 开启事务（不支持异步）
		/// </summary>
		/// <param name="handler">事务体 () => {}</param>
		/// <param name="timeout">超时，未执行完将自动提交</param>
		void Transaction(Action handler, TimeSpan timeout);
		/// <summary>
		/// 当前线程的事务
		/// </summary>
		DbTransaction TransactionCurrentThread { get; }
		/// <summary>
		/// 事务完成前预删除缓存
		/// </summary>
		/// <param name="keys"></param>
		void TransactionPreRemoveCache(params string[] keys);
		#endregion

		/// <summary>
		/// 查询，若使用读写分离，查询【从库】条件cmdText.StartsWith("SELECT ")，否则查询【主库】
		/// </summary>
		/// <param name="readerHander"></param>
		/// <param name="cmdType"></param>
		/// <param name="cmdText"></param>
		/// <param name="cmdParms"></param>
		void ExecuteReader(Action<DbDataReader> readerHander, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
		/// <summary>
		/// 查询，ExecuteReader(dr => {}, "select * from user where age > @age", new { age = 25 })
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="parms"></param>
		void ExecuteReader(Action<DbDataReader> readerHander, string cmdText, object parms = null);
		/// <summary>
		/// 查询
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="cmdParms"></param>
		object[][] ExecuteArray(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
		/// <summary>
		/// 查询，ExecuteArray("select * from user where age > @age", new { age = 25 })
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="parms"></param>
		/// <returns></returns>
		object[][] ExecuteArray(string cmdText, object parms = null);
		/// <summary>
		/// 查询
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="cmdParms"></param>
		DataTable ExecuteDataTable(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
		/// <summary>
		/// 查询，ExecuteDataTable("select * from user where age > @age", new { age = 25 })
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="parms"></param>
		/// <returns></returns>
		DataTable ExecuteDataTable(string cmdText, object parms = null);
		/// <summary>
		/// 在【主库】执行
		/// </summary>
		/// <param name="cmdType"></param>
		/// <param name="cmdText"></param>
		/// <param name="cmdParms"></param>
		int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
		/// <summary>
		/// 在【主库】执行，ExecuteNonQuery("delete from user where age > @age", new { age = 25 })
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="parms"></param>
		/// <returns></returns>
		int ExecuteNonQuery(string cmdText, object parms = null);
		/// <summary>
		/// 在【主库】执行
		/// </summary>
		/// <param name="cmdType"></param>
		/// <param name="cmdText"></param>
		/// <param name="cmdParms"></param>
		object ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
		/// <summary>
		/// 在【主库】执行，ExecuteScalar("select 1 from user where age > @age", new { age = 25 })
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="parms"></param>
		/// <returns></returns>
		object ExecuteScalar(string cmdText, object parms = null);

		/// <summary>
		/// 执行SQL返回对象集合，Query&lt;User&gt;("select * from user where age > @age", new SqlParameter { ParameterName = "age", Value = 25 })
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cmdType"></param>
		/// <param name="cmdText"></param>
		/// <param name="cmdParms"></param>
		/// <returns></returns>
		List<T> Query<T>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
		/// <summary>
		/// 执行SQL返回对象集合，Query&lt;User&gt;("select * from user where age > @age", new { age = 25 })
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cmdText"></param>
		/// <param name="parms"></param>
		/// <returns></returns>
		List<T> Query<T>(string cmdText, object parms = null);

		#region async
		/// <summary>
		/// 查询，若使用读写分离，查询【从库】条件cmdText.StartsWith("SELECT ")，否则查询【主库】
		/// </summary>
		/// <param name="readerHander"></param>
		/// <param name="cmdType"></param>
		/// <param name="cmdText"></param>
		/// <param name="cmdParms"></param>
		Task ExecuteReaderAsync(Func<DbDataReader, Task> readerHander, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
		/// <summary>
		/// 查询，ExecuteReaderAsync(dr => {}, "select * from user where age > @age", new { age = 25 })
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="parms"></param>
		Task ExecuteReaderAsync(Func<DbDataReader, Task> readerHander, string cmdText, object parms = null);
		/// <summary>
		/// 查询
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="cmdParms"></param>
		Task<object[][]> ExecuteArrayAsync(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
		/// <summary>
		/// 查询，ExecuteArrayAsync("select * from user where age > @age", new { age = 25 })
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="parms"></param>
		/// <returns></returns>
		Task<object[][]> ExecuteArrayAsync(string cmdText, object parms = null);
		/// <summary>
		/// 查询
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="cmdParms"></param>
		Task<DataTable> ExecuteDataTableAsync(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
		/// <summary>
		/// 查询，ExecuteDataTableAsync("select * from user where age > @age", new { age = 25 })
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="parms"></param>
		/// <returns></returns>
		Task<DataTable> ExecuteDataTableAsync(string cmdText, object parms = null);
		/// <summary>
		/// 在【主库】执行
		/// </summary>
		/// <param name="cmdType"></param>
		/// <param name="cmdText"></param>
		/// <param name="cmdParms"></param>
		Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
		/// <summary>
		/// 在【主库】执行，ExecuteNonQueryAsync("delete from user where age > @age", new { age = 25 })
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="parms"></param>
		/// <returns></returns>
		Task<int> ExecuteNonQueryAsync(string cmdText, object parms = null);
		/// <summary>
		/// 在【主库】执行
		/// </summary>
		/// <param name="cmdType"></param>
		/// <param name="cmdText"></param>
		/// <param name="cmdParms"></param>
		Task<object> ExecuteScalarAsync(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
		/// <summary>
		/// 在【主库】执行，ExecuteScalarAsync("select 1 from user where age > @age", new { age = 25 })
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="parms"></param>
		/// <returns></returns>
		Task<object> ExecuteScalarAsync(string cmdText, object parms = null);

		/// <summary>
		/// 执行SQL返回对象集合，QueryAsync&lt;User&gt;("select * from user where age > @age", new SqlParameter { ParameterName = "age", Value = 25 })
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cmdType"></param>
		/// <param name="cmdText"></param>
		/// <param name="cmdParms"></param>
		/// <returns></returns>
		Task<List<T>> QueryAsync<T>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
		/// <summary>
		/// 执行SQL返回对象集合，QueryAsync&lt;User&gt;("select * from user where age > @age", new { age = 25 })
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cmdText"></param>
		/// <param name="parms"></param>
		/// <returns></returns>
		Task<List<T>> QueryAsync<T>(string cmdText, object parms = null);
		#endregion
	}
}
