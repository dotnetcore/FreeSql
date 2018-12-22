using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider {
	partial class AdoProvider {
		public Task<List<T>> QueryAsync<T>(string sql, object parms = null) => QueryAsync<T>(CommandType.Text, sql, GetDbParamtersByObject(sql, parms));
		async public Task<List<T>> QueryAsync<T>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			var names = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
			var ds = new List<object[]>();
			await ExecuteReaderAsync(async dr => {
				if (names.Any() == false)
					for (var a = 0; a < dr.FieldCount; a++) names.Add(dr.GetName(a), a);
				object[] values = new object[dr.FieldCount];
				for (int a = 0; a < values.Length; a++) if (!await dr.IsDBNullAsync(a)) values[a] = await dr.GetFieldValueAsync<object>(a);
				ds.Add(values);
			}, cmdType, cmdText, cmdParms);
			var ret = new List<T>();
			foreach (var row in ds) {
				var read = Utils.ExecuteArrayRowReadClassOrTuple(typeof(T), names, row);
				ret.Add(read.value == null ? default(T) : (T) read.value);
			}
			return ret;
		}
		public Task ExecuteReaderAsync(Func<DbDataReader, Task> readerHander, string sql, object parms = null) => ExecuteReaderAsync(readerHander, CommandType.Text, sql, GetDbParamtersByObject(sql, parms));
		async public Task ExecuteReaderAsync(Func<DbDataReader, Task> readerHander, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			DateTime dt = DateTime.Now;
			string logtxt = "";
			DateTime logtxt_dt = DateTime.Now;
			var pool = this.MasterPool;
			bool isSlave = false;

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

			Object<DbConnection> conn = null;
			var cmd = PrepareCommandAsync(cmdType, cmdText, cmdParms, ref logtxt);
			if (IsTracePerformance) logtxt += $"PrepareCommand: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms\r\n";
			Exception ex = null;
			try {
				if (IsTracePerformance) logtxt_dt = DateTime.Now;
				if (isSlave) {
					//从库查询切换，恢复
					bool isSlaveFail = false;
					try {
						if (cmd.Connection == null) cmd.Connection = (conn = await pool.GetAsync()).Value;
						//if (slaveRandom.Next(100) % 2 == 0) throw new Exception("测试从库抛出异常");
					} catch {
						isSlaveFail = true;
					}
					if (isSlaveFail) {
						if (conn != null) {
							if (IsTracePerformance) logtxt_dt = DateTime.Now;
							ReturnConnection(pool, conn, ex); //pool.Return(conn, ex);
							if (IsTracePerformance) logtxt += $"ReleaseConnection: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms";
						}
						LoggerException(pool, cmd, new Exception($"连接失败，准备切换其他可用服务器"), dt, logtxt, false);
						cmd.Parameters.Clear();
						await ExecuteReaderAsync(readerHander, cmdType, cmdText, cmdParms);
						return;
					}
				} else {
					//主库查询
					if (cmd.Connection == null) cmd.Connection = (conn = await pool.GetAsync()).Value;
				}
				if (IsTracePerformance) {
					logtxt += $"Open: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms\r\n";
					logtxt_dt = DateTime.Now;
				}
				using (var dr = await cmd.ExecuteReaderAsync()) {
					if (IsTracePerformance) logtxt += $"ExecuteReader: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms\r\n";
					while (true) {
						if (IsTracePerformance) logtxt_dt = DateTime.Now;
						bool isread = await dr.ReadAsync();
						if (IsTracePerformance) logtxt += $"	dr.Read: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms\r\n";
						if (isread == false) break;

						if (readerHander != null) {
							object[] values = null;
							if (IsTracePerformance) {
								logtxt_dt = DateTime.Now;
								values = new object[dr.FieldCount];
								for (int a = 0; a < values.Length; a++) if (!await dr.IsDBNullAsync(a)) values[a] = await dr.GetFieldValueAsync<object>(a);
								logtxt += $"	dr.GetValues: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms\r\n";
								logtxt_dt = DateTime.Now;
							}
							await readerHander(dr);
							if (IsTracePerformance) logtxt += $"	readerHander: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms ({string.Join(",", values)})\r\n";
						}
					}
					if (IsTracePerformance) logtxt_dt = DateTime.Now;
					dr.Close();
				}
				if (IsTracePerformance) logtxt += $"ExecuteReader_dispose: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms\r\n";
			} catch (Exception ex2) {
				ex = ex2;
			}

			if (conn != null) {
				if (IsTracePerformance) logtxt_dt = DateTime.Now;
				ReturnConnection(pool, conn, ex); //pool.Return(conn, ex);
				if (IsTracePerformance) logtxt += $"ReleaseConnection: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms";
			}
			LoggerException(pool, cmd, ex, dt, logtxt);
			cmd.Parameters.Clear();
		}
		public Task ExecuteArrayAsync(string sql, object parms = null) => ExecuteArrayAsync(CommandType.Text, sql, GetDbParamtersByObject(sql, parms));
		async public Task<object[][]> ExecuteArrayAsync(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			List<object[]> ret = new List<object[]>();
			await ExecuteReaderAsync(async dr => {
				object[] values = new object[dr.FieldCount];
				for (int a = 0; a < values.Length; a++) if (!await dr.IsDBNullAsync(a)) values[a] = await dr.GetFieldValueAsync<object>(a);
				ret.Add(values);
			}, cmdType, cmdText, cmdParms);
			return ret.ToArray();
		}
		public Task<DataTable> ExecuteDataTableAsync(string sql, object parms = null) => ExecuteDataTableAsync(CommandType.Text, sql, GetDbParamtersByObject(sql, parms));
		async public Task<DataTable> ExecuteDataTableAsync(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			var ret = new DataTable();
			await ExecuteReaderAsync(async dr => {
				if (ret.Columns.Count == 0)
					for (var a = 0; a < dr.FieldCount; a++) ret.Columns.Add(dr.GetName(a));
				object[] values = new object[ret.Columns.Count];
				for (int a = 0; a < values.Length; a++) if (!await dr.IsDBNullAsync(a)) values[a] = await dr.GetFieldValueAsync<object>(a);
				ret.Rows.Add(values);
			}, cmdType, cmdText, cmdParms);
			return ret;
		}
		public Task<int> ExecuteNonQueryAsync(string sql, object parms = null) => ExecuteNonQueryAsync(CommandType.Text, sql, GetDbParamtersByObject(sql, parms));
		async public Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			DateTime dt = DateTime.Now;
			string logtxt = "";
			Object<DbConnection> conn = null;
			var cmd = PrepareCommandAsync(cmdType, cmdText, cmdParms, ref logtxt);
			var logtxt_dt = DateTime.Now;
			int val = 0;
			Exception ex = null;
			try {
				if (cmd.Connection == null) cmd.Connection = (conn = await this.MasterPool.GetAsync()).Value;
				val = await cmd.ExecuteNonQueryAsync();
			} catch (Exception ex2) {
				ex = ex2;
			}

			if (conn != null) {
				if (IsTracePerformance) logtxt_dt = DateTime.Now;
				ReturnConnection(MasterPool, conn, ex); //this.MasterPool.Return(conn, ex);
				if (IsTracePerformance) logtxt += $"ReleaseConnection: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms";
			}
			LoggerException(this.MasterPool, cmd, ex, dt, logtxt);
			cmd.Parameters.Clear();
			return val;
		}
		public Task<object> ExecuteScalarAsync(string sql, object parms = null) => ExecuteScalarAsync(CommandType.Text, sql, GetDbParamtersByObject(sql, parms));
		async public Task<object> ExecuteScalarAsync(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			var dt = DateTime.Now;
			var logtxt = "";
			Object<DbConnection> conn = null;
			var cmd = PrepareCommandAsync(cmdType, cmdText, cmdParms, ref logtxt);
			var logtxt_dt = DateTime.Now;
			object val = null;
			Exception ex = null;
			try {
				if (cmd.Connection == null) cmd.Connection = (conn = await this.MasterPool.GetAsync()).Value;
				val = await cmd.ExecuteScalarAsync();
			} catch (Exception ex2) {
				ex = ex2;
			}

			if (conn != null) {
				if (IsTracePerformance) logtxt_dt = DateTime.Now;
				ReturnConnection(MasterPool, conn, ex); //this.MasterPool.Return(conn, ex);
				if (IsTracePerformance) logtxt += $"ReleaseConnection: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms";
			}
			LoggerException(this.MasterPool, cmd, ex, dt, logtxt);
			cmd.Parameters.Clear();
			return val;
		}

		private DbCommand PrepareCommandAsync(CommandType cmdType, string cmdText, DbParameter[] cmdParms, ref string logtxt) {
			DateTime dt = DateTime.Now;
			DbCommand cmd = CreateCommand();
			cmd.CommandType = cmdType;
			cmd.CommandText = cmdText;

			if (cmdParms != null) {
				foreach (var parm in cmdParms) {
					if (parm == null) continue;
					if (parm.Value == null) parm.Value = DBNull.Value;
					cmd.Parameters.Add(parm);
				}
			}

			if (IsTracePerformance) logtxt += $"	PrepareCommand_tran==null: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms cmdParms: {cmd.Parameters.Count}\r\n";

			return cmd;
		}
	}
}
