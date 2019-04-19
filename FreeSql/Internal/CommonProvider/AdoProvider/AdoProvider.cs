using Microsoft.Extensions.Logging;
using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;

namespace FreeSql.Internal.CommonProvider {
	abstract partial class AdoProvider : IAdo {

		protected abstract void ReturnConnection(ObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex);
		protected abstract DbCommand CreateCommand();
		protected abstract DbParameter[] GetDbParamtersByObject(string sql, object obj);
		public Action<DbCommand> AopCommandExecuting { get; set; }
		public Action<DbCommand, string> AopCommandExecuted { get; set; }

		protected bool IsTracePerformance => AopCommandExecuted != null;
		
		public ObjectPool<DbConnection> MasterPool { get; protected set; }
		public List<ObjectPool<DbConnection>> SlavePools { get; } = new List<ObjectPool<DbConnection>>();
		public DataType DataType { get; }
		protected ICache _cache { get; set; }
		protected ILogger _log { get; set; }
		protected CommonUtils _util { get; set; }
		protected int slaveUnavailables = 0;
		private object slaveLock = new object();
		private Random slaveRandom = new Random();

		public AdoProvider(ICache cache, ILogger log, DataType dataType) {
			this._cache = cache;
			this._log = log;
			this.DataType = dataType;
		}

		void LoggerException(ObjectPool<DbConnection> pool, (DbCommand cmd, bool isclose) pc, Exception e, DateTime dt, StringBuilder logtxt, bool isThrowException = true) {
			var cmd = pc.cmd;
			if (pc.isclose) pc.cmd.Connection.Close();
			if (IsTracePerformance) {
				TimeSpan ts = DateTime.Now.Subtract(dt);
				if (e == null && ts.TotalMilliseconds > 100)
					_log.LogWarning(logtxt.Insert(0, $"{pool?.Policy.Name}（执行SQL）语句耗时过长{ts.TotalMilliseconds}ms\r\n{cmd.CommandText}\r\n").ToString());
				else
					logtxt.Insert(0, $"{pool?.Policy.Name}（执行SQL）耗时{ts.TotalMilliseconds}ms\r\n{cmd.CommandText}\r\n").ToString();
			}

			if (e == null) {
				AopCommandExecuted?.Invoke(cmd, logtxt.ToString());
				return;
			}

			StringBuilder log = new StringBuilder();
			log.Append(pool?.Policy.Name).Append("数据库出错（执行SQL）〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓\r\n").Append(cmd.CommandText).Append("\r\n");
			foreach (DbParameter parm in cmd.Parameters)
				log.Append(parm.ParameterName.PadRight(20, ' ')).Append(" = ").Append((parm.Value ?? DBNull.Value) == DBNull.Value ? "NULL" : parm.Value).Append("\r\n");

			log.Append(e.Message);
			_log.LogError(log.ToString());

			if (cmd.Transaction != null) {
				var curTran = TransactionCurrentThread;
				if (cmd.Transaction != TransactionCurrentThread) {
					//cmd.Transaction.Rollback();
				} else
					RollbackTransaction();
			}

			AopCommandExecuted?.Invoke(cmd, log.ToString());

			cmd.Parameters.Clear();
			if (isThrowException) throw e;
		}

		internal static ConcurrentDictionary<Type, PropertyInfo[]> dicQueryTypeGetProperties = new ConcurrentDictionary<Type, PropertyInfo[]>();
		public List<T> Query<T>(string cmdText, object parms = null) => Query<T>(null, null, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public List<T> Query<T>(DbTransaction transaction, string cmdText, object parms = null) => Query<T>(null, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public List<T> Query<T>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => Query<T>(connection, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public List<T> Query<T>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => Query<T>(null, null, cmdType, cmdText, cmdParms);
		public List<T> Query<T>(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => Query<T>(null, transaction, cmdType, cmdText, cmdParms);
		public List<T> Query<T>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			var ret = new List<T>();
			if (string.IsNullOrEmpty(cmdText)) return ret;
			var type = typeof(T);
			int[] indexes = null;
			var props = dicQueryTypeGetProperties.GetOrAdd(type, k => type.GetProperties());
			ExecuteReader(connection, transaction, dr => {
				if (indexes == null) {
					var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
					for (var a = 0; a < dr.FieldCount; a++)
						dic.Add(dr.GetName(a), a);
					indexes = props.Select(a => dic.TryGetValue(a.Name, out var tryint) ? tryint : -1).ToArray();
				}
				ret.Add((T)Utils.ExecuteArrayRowReadClassOrTuple(type, indexes, dr, 0, _util).Value);
			}, cmdType, cmdText, cmdParms);
			return ret;
		}
		public void ExecuteReader(Action<DbDataReader> readerHander, string cmdText, object parms = null) => ExecuteReader(null, null, readerHander, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public void ExecuteReader(DbTransaction transaction, Action<DbDataReader> readerHander, string cmdText, object parms = null) => ExecuteReader(null, transaction, readerHander, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public void ExecuteReader(DbConnection connection, DbTransaction transaction, Action<DbDataReader> readerHander, string cmdText, object parms = null) => ExecuteReader(connection, transaction, readerHander, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public void ExecuteReader(Action<DbDataReader> readerHander, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteReader(null, null, readerHander, cmdType, cmdText, cmdParms);
		public void ExecuteReader(DbTransaction transaction, Action<DbDataReader> readerHander, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteReader(null, transaction, readerHander, cmdType, cmdText, cmdParms);
		public void ExecuteReader(DbConnection connection, DbTransaction transaction,  Action<DbDataReader> readerHander, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
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
						pool = availables.Count == 1 ? availables[0] : availables[slaveRandom.Next(availables.Count)];
					}
				}
			}

			Object<DbConnection> conn = null;
			var pc = PrepareCommand(connection, transaction, cmdType, cmdText, cmdParms, logtxt);
			if (IsTracePerformance) logtxt.Append("PrepareCommand: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
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
							if (IsTracePerformance) logtxt.Append("ReleaseConnection: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms");
						}
						LoggerException(pool, pc, new Exception($"连接失败，准备切换其他可用服务器"), dt, logtxt, false);
						pc.cmd.Parameters.Clear();
						ExecuteReader(readerHander, cmdType, cmdText, cmdParms);
						return;
					}
				} else {
					//主库查询
					if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = pool.Get()).Value;
				}
				if (IsTracePerformance) {
					logtxt.Append("Open: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
					logtxt_dt = DateTime.Now;
				}
				using (var dr = pc.cmd.ExecuteReader()) {
					if (IsTracePerformance) logtxt.Append("ExecuteReader: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
					while (true) {
						if (IsTracePerformance) logtxt_dt = DateTime.Now;
						bool isread = dr.Read();
						if (IsTracePerformance) logtxt.Append("	dr.Read: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
						if (isread == false) break;

						if (readerHander != null) {
							object[] values = null;
							if (IsTracePerformance) {
								logtxt_dt = DateTime.Now;
								values = new object[dr.FieldCount];
								dr.GetValues(values);
								logtxt.Append("	dr.GetValues: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
								logtxt_dt = DateTime.Now;
							}
							readerHander(dr);
							if (IsTracePerformance) logtxt.Append("	readerHander: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms (").Append(string.Join(", ", values)).Append(")\r\n");
						}
					}
					if (IsTracePerformance) logtxt_dt = DateTime.Now;
					dr.Close();
				}
				if (IsTracePerformance) logtxt.Append("ExecuteReader_dispose: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
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
		public object[][] ExecuteArray(string cmdText, object parms = null) => ExecuteArray(null, null, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public object[][] ExecuteArray(DbTransaction transaction, string cmdText, object parms = null) => ExecuteArray(null, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public object[][] ExecuteArray(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => ExecuteArray(connection, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public object[][] ExecuteArray(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteArray(null, null, cmdType, cmdText, cmdParms);
		public object[][] ExecuteArray(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteArray(null, transaction, cmdType, cmdText, cmdParms);
		public object[][] ExecuteArray(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			List<object[]> ret = new List<object[]>();
			ExecuteReader(connection, transaction, dr => {
				object[] values = new object[dr.FieldCount];
				dr.GetValues(values);
				ret.Add(values);
			}, cmdType, cmdText, cmdParms);
			return ret.ToArray();
		}
		public DataTable ExecuteDataTable(string cmdText, object parms = null) => ExecuteDataTable(null, null, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public DataTable ExecuteDataTable(DbTransaction transaction, string cmdText, object parms = null) => ExecuteDataTable(null, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public DataTable ExecuteDataTable(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => ExecuteDataTable(connection, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public DataTable ExecuteDataTable(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteDataTable(null, null, cmdType, cmdText, cmdParms);
		public DataTable ExecuteDataTable(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteDataTable(null, transaction, cmdType, cmdText, cmdParms);
		public DataTable ExecuteDataTable(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			var ret = new DataTable();
			ExecuteReader(connection, transaction, dr => {
				if (ret.Columns.Count == 0)
					for (var a = 0; a < dr.FieldCount; a++) ret.Columns.Add(dr.GetName(a));
				object[] values = new object[ret.Columns.Count];
				dr.GetValues(values);
				ret.Rows.Add(values);
			}, cmdType, cmdText, cmdParms);
			return ret;
		}
		public int ExecuteNonQuery(string cmdText, object parms = null) => ExecuteNonQuery(null, null, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public int ExecuteNonQuery(DbTransaction transaction, string cmdText, object parms = null) => ExecuteNonQuery(null, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public int ExecuteNonQuery(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => ExecuteNonQuery(connection, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteNonQuery(null, null, cmdType, cmdText, cmdParms);
		public int ExecuteNonQuery(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteNonQuery(null, transaction, cmdType, cmdText, cmdParms);
		public int ExecuteNonQuery(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			if (string.IsNullOrEmpty(cmdText)) return 0;
			var dt = DateTime.Now;
			var logtxt = new StringBuilder();
			var logtxt_dt = DateTime.Now;
			Object<DbConnection> conn = null;
			var pc = PrepareCommand(connection, transaction, cmdType, cmdText, cmdParms, logtxt);
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
				if (IsTracePerformance) logtxt.Append("ReleaseConnection: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms");
			}
			LoggerException(this.MasterPool, pc, ex, dt, logtxt);
			pc.cmd.Parameters.Clear();
			return val;
		}
		public object ExecuteScalar(string cmdText, object parms = null) => ExecuteScalar(null, null, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public object ExecuteScalar(DbTransaction transaction, string cmdText, object parms = null) => ExecuteScalar(null, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public object ExecuteScalar(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => ExecuteScalar(connection, transaction, CommandType.Text, cmdText, GetDbParamtersByObject(cmdText, parms));
		public object ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteScalar(null, null, cmdType, cmdText, cmdParms);
		public object ExecuteScalar(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteScalar(null, transaction, cmdType, cmdText, cmdParms);
		public object ExecuteScalar(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) {
			if (string.IsNullOrEmpty(cmdText)) return null;
			var dt = DateTime.Now;
			var logtxt = new StringBuilder();
			var logtxt_dt = DateTime.Now;
			Object<DbConnection> conn = null;
			var pc = PrepareCommand(connection, transaction, cmdType, cmdText, cmdParms, logtxt);
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
				if (IsTracePerformance) logtxt.Append("ReleaseConnection: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms");
			}
			LoggerException(this.MasterPool, pc, ex, dt, logtxt);
			pc.cmd.Parameters.Clear();
			return val;
		}

		(DbCommand cmd, bool isclose) PrepareCommand(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, StringBuilder logtxt) {
			var dt = DateTime.Now;
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
				var tran = transaction ?? TransactionCurrentThread;
				if (IsTracePerformance) logtxt.Append("	PrepareCommand_part1: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms cmdParms: ").Append(cmd.Parameters.Count).Append("\r\n");

				if (tran != null && connection == null) {
					if (IsTracePerformance) dt = DateTime.Now;
					cmd.Connection = tran.Connection;
					cmd.Transaction = tran;
					if (IsTracePerformance) logtxt.Append("	PrepareCommand_tran!=null: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
				}
			} else {
				if (connection.State != ConnectionState.Open) {
					if (IsTracePerformance) dt = DateTime.Now;
					connection.Open();
					if (IsTracePerformance) logtxt.Append("	PrepareCommand_ConnectionOpen: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
					isclose = true;
				}
				cmd.Connection = connection;
				if (transaction.Connection == connection)
					cmd.Transaction = transaction;
			}

			if (IsTracePerformance) dt = DateTime.Now;
			AutoCommitTransaction();
			if (IsTracePerformance) logtxt.Append("   AutoCommitTransaction: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");

			AopCommandExecuting?.Invoke(cmd);
			return (cmd, isclose);
		}
	}
}
