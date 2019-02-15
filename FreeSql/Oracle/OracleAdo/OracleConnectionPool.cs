using Oracle.ManagedDataAccess.Client;
using SafeObjectPool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FreeSql.Oracle {

	class OracleConnectionPool : ObjectPool<DbConnection> {

		internal Action availableHandler;
		internal Action unavailableHandler;
		internal string UserId { get; set; }

		public OracleConnectionPool(string name, string connectionString, Action availableHandler, Action unavailableHandler) : base(null) {
			var userIdMatch = Regex.Match(connectionString, @"User\s+Id\s*=\s*([^;]+)", RegexOptions.IgnoreCase);
			if (userIdMatch.Success == false) throw new Exception(@"从 ConnectionString 中无法匹配 User\s+Id\s+=([^;]+)");
			this.UserId = userIdMatch.Groups[1].Value.Trim().ToUpper();

			var policy = new OracleConnectionPoolPolicy {
				_pool = this,
				Name = name
			};
			this.Policy = policy;
			policy.ConnectionString = connectionString;

			this.availableHandler = availableHandler;
			this.unavailableHandler = unavailableHandler;
		}

		public void Return(Object<DbConnection> obj, Exception exception, bool isRecreate = false) {
			if (exception != null && exception is OracleException) {

				if (exception is System.IO.IOException) {

					base.SetUnavailable(exception);

				} else if (obj.Value.Ping() == false) {

					base.SetUnavailable(exception);
				}
			}
			base.Return(obj, isRecreate);
		}
	}

	class OracleConnectionPoolPolicy : IPolicy<DbConnection> {

		internal OracleConnectionPool _pool;
		public string Name { get; set; } = "Oracle Connection 对象池";
		public int PoolSize { get; set; } = 100;
		public TimeSpan SyncGetTimeout { get; set; } = TimeSpan.FromSeconds(10);
		public int AsyncGetCapacity { get; set; } = 10000;
		public bool IsThrowGetTimeoutException { get; set; } = true;
		public int CheckAvailableInterval { get; set; } = 5;

		static ConcurrentDictionary<string, int> dicConnStrIncr = new ConcurrentDictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
		private string _connectionString;
		public string ConnectionString {
			get => _connectionString;
			set {
				var connStr = value ?? "";
				var poolsizePatern = @"Max\s*pool\s*size\s*=\s*(\d+)";
				Match m = Regex.Match(connStr, poolsizePatern, RegexOptions.IgnoreCase);
				if (m.Success == false || int.TryParse(m.Groups[1].Value, out var poolsize) == false || poolsize <= 0) poolsize = 100;
				var connStrIncr = dicConnStrIncr.AddOrUpdate(connStr, 1, (oldkey, oldval) => oldval + 1);
				PoolSize = poolsize + connStrIncr;
				_connectionString = m.Success ?
					Regex.Replace(connStr, poolsizePatern, $"Max pool size={PoolSize}", RegexOptions.IgnoreCase) :
					$"{connStr};Max pool size={PoolSize}";

				var initConns = new Object<DbConnection>[poolsize];
				for (var a = 0; a < poolsize; a++) try { initConns[a] = _pool.Get(); } catch { }
				foreach (var conn in initConns) _pool.Return(conn);
			}
		}

		public bool OnCheckAvailable(Object<DbConnection> obj) {
			if (obj.Value.State == ConnectionState.Closed) obj.Value.Open();
			var cmd = obj.Value.CreateCommand();
			cmd.CommandText = "select 1";
			cmd.ExecuteNonQuery();
			return true;
		}

		public DbConnection OnCreate() {
			var conn = new OracleConnection(_connectionString);
			return conn;
		}

		public void OnDestroy(DbConnection obj) {
			if (obj.State != ConnectionState.Closed) obj.Close();
			obj.Dispose();
		}

		public void OnGet(Object<DbConnection> obj) {

			if (_pool.IsAvailable) {

				if (obj.Value.State != ConnectionState.Open || DateTime.Now.Subtract(obj.LastReturnTime).TotalSeconds > 60 && obj.Value.Ping() == false) {

					try {
						obj.Value.Open();
					} catch (Exception ex) {
						if (_pool.SetUnavailable(ex) == true)
							throw new Exception($"【{this.Name}】状态不可用，等待后台检查程序恢复方可使用。{ex.Message}");
					}
				}
			}
		}

		async public Task OnGetAsync(Object<DbConnection> obj) {

			if (_pool.IsAvailable) {

				if (obj.Value.State != ConnectionState.Open || DateTime.Now.Subtract(obj.LastReturnTime).TotalSeconds > 60 && obj.Value.Ping() == false) {

					try {
						await obj.Value.OpenAsync();
					} catch (Exception ex) {
						if (_pool.SetUnavailable(ex) == true)
							throw new Exception($"【{this.Name}】状态不可用，等待后台检查程序恢复方可使用。{ex.Message}");
					}
				}
			}
		}

		public void OnGetTimeout() {

		}

		public void OnReturn(Object<DbConnection> obj) {

		}

		public void OnAvailable() {
			_pool.availableHandler?.Invoke();
		}

		public void OnUnavailable() {
			_pool.unavailableHandler?.Invoke();
		}
	}

	static class DbConnectionExtensions {

		public static bool Ping(this DbConnection that) {
			try {
				var cmd = that.CreateCommand();
				cmd.CommandText = "select 1 from dual";
				cmd.ExecuteNonQuery();
				return true;
			} catch {
				try { that.Close(); } catch { }
				return false;
			}
		}
	}
}
