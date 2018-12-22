using Microsoft.Extensions.Logging;
using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace FreeSql.Internal.CommonProvider {
	abstract partial class AdoProvider : IAdo {

		protected abstract void ReturnConnection(ObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex);
		protected abstract DbCommand CreateCommand();
		protected abstract DbParameter[] GetDbParamtersByObject(string sql, object obj);

		public bool IsTracePerformance { get; set; } = string.Compare(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", true) == 0;
		
		public ObjectPool<DbConnection> MasterPool { get; protected set; }
		public List<ObjectPool<DbConnection>> SlavePools { get; } = new List<ObjectPool<DbConnection>>();
		protected ICache _cache { get; set; }
		protected ILogger _log { get; set; }
		protected int slaveUnavailables = 0;
		private object slaveLock = new object();
		private Random slaveRandom = new Random();

		public AdoProvider(ICache cache, ILogger log) {
			this._cache = cache;
			this._log = log;
		}

		void LoggerException(ObjectPool<DbConnection> pool, DbCommand cmd, Exception e, DateTime dt, string logtxt, bool isThrowException = true) {
			if (IsTracePerformance) {
				TimeSpan ts = DateTime.Now.Subtract(dt);
				if (e == null && ts.TotalMilliseconds > 100)
					_log.LogWarning($"{pool.Policy.Name}执行SQL语句耗时过长{ts.TotalMilliseconds}ms\r\n{cmd.CommandText}\r\n{logtxt}");
			}

			if (e == null) return;
			string log = $"{pool.Policy.Name}数据库出错（执行SQL）〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓\r\n{cmd.CommandText}\r\n";
			foreach (DbParameter parm in cmd.Parameters)
				log += parm.ParameterName.PadRight(20, ' ') + " = " + (parm.Value ?? "NULL") + "\r\n";

			log += e.Message;
			_log.LogError(log);

			RollbackTransaction();
			cmd.Parameters.Clear();
			if (isThrowException) throw e;
		}

		public List<T> Query<T>(string sql, object parms = null) => Query<T>(CommandType.Text, sql, GetDbParamtersByObject(sql, parms));
		public List<T> Query<T>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			var names = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
			var ds = new List<object[]>();
			ExecuteReader(dr => {
				if (names.Any() == false)
					for (var a = 0; a < dr.FieldCount; a++) names.Add(dr.GetName(a), a);
				object[] values = new object[dr.FieldCount];
				dr.GetValues(values);
				ds.Add(values);
			}, cmdType, cmdText, cmdParms);
			var ret = new List<T>();
			foreach (var row in ds) {
				var read = Utils.ExecuteArrayRowReadClassOrTuple(typeof(T), names, row);
				ret.Add(read.value == null ? default(T) : (T) read.value);
			}
			return ret;
		}
		public void ExecuteReader(Action<DbDataReader> readerHander, string sql, object parms = null) => ExecuteReader(readerHander, CommandType.Text, sql, GetDbParamtersByObject(sql, parms));
		public void ExecuteReader(Action<DbDataReader> readerHander, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
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
					pool = availables.Count == 1 ? availables[0] : availables[slaveRandom.Next(availables.Count)];
				}
			}

			Object<DbConnection> conn = null;
			var pc = PrepareCommand(cmdType, cmdText, cmdParms, ref logtxt);
			if (IsTracePerformance) logtxt += $"PrepareCommand: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms\r\n";
			Exception ex = null;
			try {
				if (IsTracePerformance) logtxt_dt = DateTime.Now;
				if (isSlave) {
					//从库查询切换，恢复
					bool isSlaveFail = false;
					try {
						if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = pool.Get()).Value;
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
						LoggerException(pool, pc.cmd, new Exception($"连接失败，准备切换其他可用服务器"), dt, logtxt, false);
						pc.cmd.Parameters.Clear();
						ExecuteReader(readerHander, cmdType, cmdText, cmdParms);
						return;
					}
				} else {
					//主库查询
					if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = pool.Get()).Value;
				}
				if (IsTracePerformance) {
					logtxt += $"Open: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms\r\n";
					logtxt_dt = DateTime.Now;
				}
				using (var dr = pc.cmd.ExecuteReader()) {
					if (IsTracePerformance) logtxt += $"ExecuteReader: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms\r\n";
					while (true) {
						if (IsTracePerformance) logtxt_dt = DateTime.Now;
						bool isread = dr.Read();
						if (IsTracePerformance) logtxt += $"	dr.Read: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms\r\n";
						if (isread == false) break;

						if (readerHander != null) {
							object[] values = null;
							if (IsTracePerformance) {
								logtxt_dt = DateTime.Now;
								values = new object[dr.FieldCount];
								dr.GetValues(values);
								logtxt += $"	dr.GetValues: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms\r\n";
								logtxt_dt = DateTime.Now;
							}
							readerHander(dr);
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
			LoggerException(pool, pc.cmd, ex, dt, logtxt);
			pc.cmd.Parameters.Clear();
		}
		public object[][] ExecuteArray(string sql, object parms = null) => ExecuteArray(CommandType.Text, sql, GetDbParamtersByObject(sql, parms));
		public object[][] ExecuteArray(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			List<object[]> ret = new List<object[]>();
			ExecuteReader(dr => {
				object[] values = new object[dr.FieldCount];
				dr.GetValues(values);
				ret.Add(values);
			}, cmdType, cmdText, cmdParms);
			return ret.ToArray();
		}
		public DataTable ExecuteDataTable(string sql, object parms = null) => ExecuteDataTable(CommandType.Text, sql, GetDbParamtersByObject(sql, parms));
		public DataTable ExecuteDataTable(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			var ret = new DataTable();
			ExecuteReader(dr => {
				if (ret.Columns.Count == 0)
					for (var a = 0; a < dr.FieldCount; a++) ret.Columns.Add(dr.GetName(a));
				object[] values = new object[ret.Columns.Count];
				dr.GetValues(values);
				ret.Rows.Add(values);
			}, cmdType, cmdText, cmdParms);
			return ret;
		}
		public int ExecuteNonQuery(string sql, object parms = null) => ExecuteNonQuery(CommandType.Text, sql, GetDbParamtersByObject(sql, parms));
		public int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			DateTime dt = DateTime.Now;
			string logtxt = "";
			DateTime logtxt_dt = DateTime.Now;
			Object<DbConnection> conn = null;
			var pc = PrepareCommand(cmdType, cmdText, cmdParms, ref logtxt);
			int val = 0;
			Exception ex = null;
			try {
				if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = this.MasterPool.Get()).Value;
				val = pc.cmd.ExecuteNonQuery();
			} catch (Exception ex2) {
				ex = ex2;
			}

			if (conn != null) {
				if (IsTracePerformance) logtxt_dt = DateTime.Now;
				ReturnConnection(MasterPool, conn, ex); //this.MasterPool.Return(conn, ex);
				if (IsTracePerformance) logtxt += $"ReleaseConnection: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms";
			}
			LoggerException(this.MasterPool, pc.cmd, ex, dt, logtxt);
			pc.cmd.Parameters.Clear();
			return val;
		}
		public object ExecuteScalar(string sql, object parms = null) => ExecuteScalar(CommandType.Text, sql, GetDbParamtersByObject(sql, parms));
		public object ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			DateTime dt = DateTime.Now;
			string logtxt = "";
			DateTime logtxt_dt = DateTime.Now;
			Object<DbConnection> conn = null;
			var pc = PrepareCommand(cmdType, cmdText, cmdParms, ref logtxt);
			object val = null;
			Exception ex = null;
			try {
				if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = this.MasterPool.Get()).Value;
				val = pc.cmd.ExecuteScalar();
			} catch (Exception ex2) {
				ex = ex2;
			}

			if (conn != null) {
				if (IsTracePerformance) logtxt_dt = DateTime.Now;
				ReturnConnection(MasterPool, conn, ex); //this.MasterPool.Return(conn, ex);
				if (IsTracePerformance) logtxt += $"ReleaseConnection: {DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds}ms Total: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms";
			}
			LoggerException(this.MasterPool, pc.cmd, ex, dt, logtxt);
			pc.cmd.Parameters.Clear();
			return val;
		}

		private (DbTransaction tran, DbCommand cmd) PrepareCommand(CommandType cmdType, string cmdText, DbParameter[] cmdParms, ref string logtxt) {
			var dt = DateTime.Now;
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

			var tran = TransactionCurrentThread;
			if (IsTracePerformance) logtxt += $"	PrepareCommand_part1: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms cmdParms: {cmd.Parameters.Count}\r\n";

			if (tran != null) {
				if (IsTracePerformance) dt = DateTime.Now;
				cmd.Connection = tran.Connection;
				cmd.Transaction = tran;
				if (IsTracePerformance) logtxt += $"	PrepareCommand_tran!=null: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms\r\n";
			}

			if (IsTracePerformance) dt = DateTime.Now;
			AutoCommitTransaction();
			if (IsTracePerformance) logtxt += $"	AutoCommitTransaction: {DateTime.Now.Subtract(dt).TotalMilliseconds}ms\r\n";

			return (tran, cmd);
		}
	}
}
