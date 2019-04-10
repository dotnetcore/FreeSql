using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider {
	partial class AdoProvider {
		public Task<List<T>> QueryAsync<T>(string cmdText, object parms = null) => QueryAsync<T>(null, null, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task<List<T>> QueryAsync<T>(DbTransaction transaction, string cmdText, object parms = null) => QueryAsync<T>(null, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task<List<T>> QueryAsync<T>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => QueryAsync<T>(connection, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task<List<T>> QueryAsync<T>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => QueryAsync<T>(null, null, cmdType, cmdText, cmdParms);
		public Task<List<T>> QueryAsync<T>(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => QueryAsync<T>(null, transaction, cmdType, cmdText, cmdParms);
		async public Task<List<T>> QueryAsync<T>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			var ret = new List<T>();
			if (string.IsNullOrEmpty(cmdText)) return ret;
			var type = typeof(T);
			int[] indexes = null;
			var props = dicQueryTypeGetProperties.GetOrAdd(type, k => type.GetProperties());
			await ExecuteReaderAsync(connection, transaction, dr => {
				if (indexes == null) {
					var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
					for (var a = 0; a < dr.FieldCount; a++)
						dic.Add(dr.GetName(a), a);
					indexes = props.Select(a => dic.TryGetValue(a.Name, out var tryint) ? tryint : -1).ToArray();
				}
				ret.Add((T)Utils.ExecuteArrayRowReadClassOrTuple(type, indexes, dr, 0, _util).Value);
				return Task.CompletedTask;
			}, cmdType, cmdText, cmdParms);
			return ret;
		}
		public Task ExecuteReaderAsync(Func<DbDataReader, Task> readerHander, string cmdText, object parms = null) => ExecuteReaderAsync(null, null, readerHander, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task ExecuteReaderAsync(DbTransaction transaction, Func<DbDataReader, Task> readerHander, string cmdText, object parms = null) => ExecuteReaderAsync(null, transaction, readerHander, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task ExecuteReaderAsync(DbConnection connection, DbTransaction transaction, Func<DbDataReader, Task> readerHander, string cmdText, object parms = null) => ExecuteReaderAsync(connection, transaction, readerHander, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task ExecuteReaderAsync(Func<DbDataReader, Task> readerHander, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteReaderAsync(null, null, readerHander, cmdType, cmdText, cmdParms);
		public Task ExecuteReaderAsync(DbTransaction transaction, Func<DbDataReader, Task> readerHander, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteReaderAsync(null, transaction, readerHander, cmdType, cmdText, cmdParms);
		async public Task ExecuteReaderAsync(DbConnection connection, DbTransaction transaction, Func<DbDataReader, Task> readerHander, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			if (string.IsNullOrEmpty(cmdText)) return;
			var dt = DateTime.Now;
			var logtxt = new StringBuilder();
			var logtxt_dt = DateTime.Now;
			var pool = this.MasterPool;
			var isSlave = false;

			if (transaction == null && connection == null) {
				//读写分离规则
				if (this.SlavePools.Any() && cmdText.StartsWith("SELECT ", StringComparison.CurrentCultureIgnoreCase)) {
					var availables = slaveUnavailables == 0 ?
						//查从库
						this.SlavePools : (
						//查主库
						slaveUnavailables == this.SlavePools.Count ? new List<ObjectPool<DbConnection>>() :
						//查从库可用
						this.SlavePools.Where(sp => sp.IsAvailable).ToList());
					if (availables.Any()) {
						isSlave = true;
						pool = availables.Count == 1 ? this.SlavePools[0] : availables[slaveRandom.Next(availables.Count)];
					}
				}
			}

			Object<DbConnection> conn = null;
			var pc = await PrepareCommandAsync(connection, transaction, cmdType, cmdText, cmdParms, logtxt);
			if (IsTracePerformance) logtxt.Append("PrepareCommandAsync: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
			Exception ex = null;
			try {
				if (IsTracePerformance) logtxt_dt = DateTime.Now;
				if (isSlave) {
					//从库查询切换，恢复
					bool isSlaveFail = false;
					try {
						if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = await pool.GetAsync()).Value;
						//if (slaveRandom.Next(100) % 2 == 0) throw new Exception("测试从库抛出异常");
					} catch {
						isSlaveFail = true;
					}
					if (isSlaveFail) {
						if (conn != null) {
							if (IsTracePerformance) logtxt_dt = DateTime.Now;
							ReturnConnection(pool, conn, ex); //pool.Return(conn, ex);
							if (IsTracePerformance) logtxt.Append("ReleaseConnection: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms");
						}
						LoggerException(pool, pc, new Exception($"连接失败，准备切换其他可用服务器"), dt, logtxt, false);
						pc.cmd.Parameters.Clear();
						await ExecuteReaderAsync(readerHander, cmdType, cmdText, cmdParms);
						return;
					}
				} else {
					//主库查询
					if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = await pool.GetAsync()).Value;
				}
				if (IsTracePerformance) {
					logtxt.Append("OpenAsync: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
					logtxt_dt = DateTime.Now;
				}
				using (var dr = await pc.cmd.ExecuteReaderAsync()) {
					if (IsTracePerformance) logtxt.Append("ExecuteReaderAsync: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
					while (true) {
						if (IsTracePerformance) logtxt_dt = DateTime.Now;
						bool isread = await dr.ReadAsync();
						if (IsTracePerformance) logtxt.Append("	dr.ReadAsync: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
						if (isread == false) break;

						if (readerHander != null) {
							object[] values = null;
							if (IsTracePerformance) {
								logtxt_dt = DateTime.Now;
								values = new object[dr.FieldCount];
								for (int a = 0; a < values.Length; a++) if (!await dr.IsDBNullAsync(a)) values[a] = await dr.GetFieldValueAsync<object>(a);
								logtxt.Append("	dr.GetValues: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
								logtxt_dt = DateTime.Now;
							}
							await readerHander(dr);
							if (IsTracePerformance) logtxt.Append("	readerHanderAsync: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms (").Append(string.Join(", ", values)).Append(")\r\n");
						}
					}
					if (IsTracePerformance) logtxt_dt = DateTime.Now;
					dr.Close();
				}
				if (IsTracePerformance) logtxt.Append("ExecuteReaderAsync_dispose: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
			} catch (Exception ex2) {
				ex = ex2;
			}

			if (conn != null) {
				if (IsTracePerformance) logtxt_dt = DateTime.Now;
				ReturnConnection(pool, conn, ex); //pool.Return(conn, ex);
				if (IsTracePerformance) logtxt.Append("ReleaseConnection: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms");
			}
			LoggerException(pool, pc, ex, dt, logtxt);
			pc.cmd.Parameters.Clear();
		}
		public Task<object[][]> ExecuteArrayAsync(string cmdText, object parms = null) => ExecuteArrayAsync(null, null, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task<object[][]> ExecuteArrayAsync(DbTransaction transaction, string cmdText, object parms = null) => ExecuteArrayAsync(null, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task<object[][]> ExecuteArrayAsync(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => ExecuteArrayAsync(connection, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task<object[][]> ExecuteArrayAsync(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteArrayAsync(null, null, cmdType, cmdText, cmdParms);
		public Task<object[][]> ExecuteArrayAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteArrayAsync(null, transaction, cmdType, cmdText, cmdParms);
		async public Task<object[][]> ExecuteArrayAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			List<object[]> ret = new List<object[]>();
			await ExecuteReaderAsync(connection, transaction, async dr => {
				object[] values = new object[dr.FieldCount];
				for (int a = 0; a < values.Length; a++) if (!await dr.IsDBNullAsync(a)) values[a] = await dr.GetFieldValueAsync<object>(a);
				ret.Add(values);
			}, cmdType, cmdText, cmdParms);
			return ret.ToArray();
		}
		public Task<DataTable> ExecuteDataTableAsync(string cmdText, object parms = null) => ExecuteDataTableAsync(null, null, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task<DataTable> ExecuteDataTableAsync(DbTransaction transaction, string cmdText, object parms = null) => ExecuteDataTableAsync(null, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task<DataTable> ExecuteDataTableAsync(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => ExecuteDataTableAsync(connection, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task<DataTable> ExecuteDataTableAsync(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteDataTableAsync(null, null, cmdType, cmdText, cmdParms);
		public Task<DataTable> ExecuteDataTableAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteDataTableAsync(null, transaction, cmdType, cmdText, cmdParms);
		async public Task<DataTable> ExecuteDataTableAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			var ret = new DataTable();
			await ExecuteReaderAsync(connection, transaction, async dr => {
				if (ret.Columns.Count == 0)
					for (var a = 0; a < dr.FieldCount; a++) ret.Columns.Add(dr.GetName(a));
				object[] values = new object[ret.Columns.Count];
				for (int a = 0; a < values.Length; a++) if (!await dr.IsDBNullAsync(a)) values[a] = await dr.GetFieldValueAsync<object>(a);
				ret.Rows.Add(values);
			}, cmdType, cmdText, cmdParms);
			return ret;
		}
		public Task<int> ExecuteNonQueryAsync(string cmdText, object parms = null) => ExecuteNonQueryAsync(null, null, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task<int> ExecuteNonQueryAsync(DbTransaction transaction, string cmdText, object parms = null) => ExecuteNonQueryAsync(null, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task<int> ExecuteNonQueryAsync(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => ExecuteNonQueryAsync(connection, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteNonQueryAsync(null, null, cmdType, cmdText, cmdParms);
		public Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteNonQueryAsync(null, transaction, cmdType, cmdText, cmdParms);
		async public Task<int> ExecuteNonQueryAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			if (string.IsNullOrEmpty(cmdText)) return 0;
			var dt = DateTime.Now;
			var logtxt = new StringBuilder();
			var logtxt_dt = DateTime.Now;
			Object<DbConnection> conn = null;
			var pc = await PrepareCommandAsync(connection, transaction, cmdType, cmdText, cmdParms, logtxt);
			int val = 0;
			Exception ex = null;
			try {
				if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = await this.MasterPool.GetAsync()).Value;
				val = await pc.cmd.ExecuteNonQueryAsync();
			} catch (Exception ex2) {
				ex = ex2;
			}

			if (conn != null) {
				if (IsTracePerformance) logtxt_dt = DateTime.Now;
				ReturnConnection(MasterPool, conn, ex); //this.MasterPool.Return(conn, ex);
				if (IsTracePerformance) logtxt.Append("ReleaseConnection: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms");
			}
			LoggerException(this.MasterPool, pc, ex, dt, logtxt);
			pc.cmd.Parameters.Clear();
			return val;
		}
		public Task<object> ExecuteScalarAsync(string cmdText, object parms = null) => ExecuteScalarAsync(null, null, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task<object> ExecuteScalarAsync(DbTransaction transaction, string cmdText, object parms = null) => ExecuteScalarAsync(null, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task<object> ExecuteScalarAsync(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => ExecuteScalarAsync(connection, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public Task<object> ExecuteScalarAsync(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteScalarAsync(null, null, cmdType, cmdText, cmdParms);
		public Task<object> ExecuteScalarAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteScalarAsync(null, transaction, cmdType, cmdText, cmdParms);
		async public Task<object> ExecuteScalarAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			if (string.IsNullOrEmpty(cmdText)) return null;
			var dt = DateTime.Now;
			var logtxt = new StringBuilder();
			var logtxt_dt = DateTime.Now;
			Object<DbConnection> conn = null;
			var pc = await PrepareCommandAsync(connection, transaction, cmdType, cmdText, cmdParms, logtxt);
			object val = null;
			Exception ex = null;
			try {
				if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = await this.MasterPool.GetAsync()).Value;
				val = await pc.cmd.ExecuteScalarAsync();
			} catch (Exception ex2) {
				ex = ex2;
			}

			if (conn != null) {
				if (IsTracePerformance) logtxt_dt = DateTime.Now;
				ReturnConnection(MasterPool, conn, ex); //this.MasterPool.Return(conn, ex);
				if (IsTracePerformance) logtxt.Append("ReleaseConnection: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms");
			}
			LoggerException(this.MasterPool, pc, ex, dt, logtxt);
			pc.cmd.Parameters.Clear();
			return val;
		}

		async Task<(DbCommand cmd, bool isclose)> PrepareCommandAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, StringBuilder logtxt) {
			DateTime dt = DateTime.Now;
			DbCommand cmd = CreateCommand();
			bool isclose = false;
			cmd.CommandType = cmdType;
			cmd.CommandText = cmdText;

			if (cmdParms != null) {
				foreach (var parm in cmdParms) {
					if (parm == null) continue;
					if (parm.Value == null) parm.Value = DBNull.Value;
					cmd.Parameters.Add(parm);
				}
			}

			if (connection == null) {
				var tran = transaction;

				if (tran != null) {
					if (IsTracePerformance) dt = DateTime.Now;
					cmd.Connection = tran.Connection;
					cmd.Transaction = tran;
					if (IsTracePerformance) logtxt.Append("	PrepareCommandAsync_tran!=null: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
				}
			} else {
				if (connection.State != ConnectionState.Open) {
					if (IsTracePerformance) dt = DateTime.Now;
					await connection.OpenAsync();
					if (IsTracePerformance) logtxt.Append("	PrepareCommand_ConnectionOpenAsync: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
					isclose = true;
				}
				cmd.Connection = connection;
			}

			if (IsTracePerformance) logtxt.Append("	PrepareCommandAsync ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms cmdParms: ").Append(cmd.Parameters.Count).Append("\r\n");

			AopCommandExecuting?.Invoke(cmd);
			return (cmd, isclose);
		}
	}
}
