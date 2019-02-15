using MySql.Data.MySqlClient;
using SafeObjectPool;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FreeSql.MySql {

	class MySqlConnectionPool : ObjectPool<DbConnection> {

		internal Action availableHandler;
		internal Action unavailableHandler;

		public MySqlConnectionPool(string name, string connectionString, Action availableHandler, Action unavailableHandler) : base(null) {
			var policy = new MySqlConnectionPoolPolicy {
				_pool = this,
				Name = name
			};
			this.Policy = policy;
			policy.ConnectionString = connectionString;

			this.availableHandler = availableHandler;
			this.unavailableHandler = unavailableHandler;
		}

		public void Return(Object<DbConnection> obj, Exception exception, bool isRecreate = false) {
			if (exception != null && exception is MySqlException) {
				try { if ((obj.Value as MySqlConnection).Ping() == false) obj.Value.Open(); } catch { base.SetUnavailable(exception); }
			}
			base.Return(obj, isRecreate);
		}
	}

	class MySqlConnectionPoolPolicy : IPolicy<DbConnection> {

		internal MySqlConnectionPool _pool;
		public string Name { get; set; } = "MySql MySqlConnection 对象池";
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
			if ((obj.Value as MySqlConnection).Ping() == false) obj.Value.Open();
			return (obj.Value as MySqlConnection).Ping();
		}

		public DbConnection OnCreate() {
			var conn = new MySqlConnection(_connectionString);
			return conn;
		}

		public void OnDestroy(DbConnection obj) {
			if (obj.State != ConnectionState.Closed) obj.Close();
			obj.Dispose();
		}

		public void OnGet(Object<DbConnection> obj) {

			if (_pool.IsAvailable) {

				if (obj.Value.State != ConnectionState.Open || DateTime.Now.Subtract(obj.LastReturnTime).TotalSeconds > 60 && (obj.Value as MySqlConnection).Ping() == false) {

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

				if (obj.Value.State != ConnectionState.Open || DateTime.Now.Subtract(obj.LastReturnTime).TotalSeconds > 60 && (obj.Value as MySqlConnection).Ping() == false) {

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
}
