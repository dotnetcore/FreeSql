using SafeObjectPool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FreeSql.Sqlite
{

    class SqliteConnectionPool : ObjectPool<DbConnection>
    {

        internal Action availableHandler;
        internal Action unavailableHandler;

        public SqliteConnectionPool(string name, string connectionString, Action availableHandler, Action unavailableHandler) : base(null)
        {
            policy = new SqliteConnectionPoolPolicy
            {
                _pool = this,
                Name = name
            };
            this.Policy = policy;
            policy.ConnectionString = connectionString;

            this.availableHandler = availableHandler;
            this.unavailableHandler = unavailableHandler;
        }

        public void Return(Object<DbConnection> obj, Exception exception, bool isRecreate = false)
        {
            if (exception != null && exception is SQLiteException)
            {
                try { if (obj.Value.Ping() == false) obj.Value.OpenAndAttach(policy.Attaches); } catch { base.SetUnavailable(exception); }
            }
            base.Return(obj, isRecreate);
        }

        internal SqliteConnectionPoolPolicy policy;
    }

    class SqliteConnectionPoolPolicy : IPolicy<DbConnection>
    {

        internal SqliteConnectionPool _pool;
        public string Name { get; set; } = "Sqlite SQLiteConnection 对象池";
        public int PoolSize { get; set; } = 100;
        public TimeSpan SyncGetTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan IdleTimeout { get; set; } = TimeSpan.Zero;
        public int AsyncGetCapacity { get; set; } = 10000;
        public bool IsThrowGetTimeoutException { get; set; } = true;
        public int CheckAvailableInterval { get; set; } = 5;
        public string[] Attaches = new string[0];

        static ConcurrentDictionary<string, int> dicConnStrIncr = new ConcurrentDictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
        private string _connectionString;
        public string ConnectionString
        {
            get => _connectionString;
            set
            {
                _connectionString = value ?? "";

                var pattern = @"Max\s*pool\s*size\s*=\s*(\d+)";
                Match m = Regex.Match(_connectionString, pattern, RegexOptions.IgnoreCase);
                if (m.Success == false || int.TryParse(m.Groups[1].Value, out var poolsize) == false || poolsize <= 0) poolsize = 100;
                var connStrIncr = dicConnStrIncr.AddOrUpdate(_connectionString, 1, (oldkey, oldval) => oldval + 1);
                PoolSize = poolsize + connStrIncr;
                _connectionString = m.Success ?
                    Regex.Replace(_connectionString, pattern, $"Max pool size={PoolSize}", RegexOptions.IgnoreCase) :
                    $"{_connectionString};Max pool size={PoolSize}";

                pattern = @"Connection\s*LifeTime\s*=\s*(\d+)";
                m = Regex.Match(_connectionString, pattern, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    IdleTimeout = TimeSpan.FromSeconds(int.Parse(m.Groups[1].Value));
                    _connectionString = Regex.Replace(_connectionString, pattern, "", RegexOptions.IgnoreCase);
                }

                var minPoolSize = 0;
                pattern = @"Min\s*pool\s*size\s*=\s*(\d+)";
                m = Regex.Match(_connectionString, pattern, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    minPoolSize = int.Parse(m.Groups[1].Value);
                    _connectionString = Regex.Replace(_connectionString, pattern, "", RegexOptions.IgnoreCase);
                }

                var att = Regex.Split(_connectionString, @"Attachs\s*=\s*", RegexOptions.IgnoreCase);
                if (att.Length == 2)
                {
                    var idx = att[1].IndexOf(';');
                    Attaches = (idx == -1 ? att[1] : att[1].Substring(0, idx)).Split(',');
                }

                FreeSql.Internal.CommonUtils.PrevReheatConnectionPool(_pool, minPoolSize);
            }
        }

        public bool OnCheckAvailable(Object<DbConnection> obj)
        {
            if (obj.Value.State == ConnectionState.Closed) obj.Value.OpenAndAttach(Attaches);
            return obj.Value.Ping(true);
        }

        public DbConnection OnCreate()
        {
            var conn = new SQLiteConnection(_connectionString);
            return conn;
        }

        public void OnDestroy(DbConnection obj)
        {
            if (obj.State != ConnectionState.Closed) obj.Close();
            obj.Dispose();
        }

        public void OnGet(Object<DbConnection> obj)
        {

            if (_pool.IsAvailable)
            {
                if (obj.Value == null)
                {
                    if (_pool.SetUnavailable(new Exception("连接字符串错误")) == true)
                        throw new Exception($"【{this.Name}】连接字符串错误，请检查。");
                    return;
                }

                if (obj.Value.State != ConnectionState.Open || DateTime.Now.Subtract(obj.LastReturnTime).TotalSeconds > 60 && obj.Value.Ping() == false)
                {

                    try
                    {
                        obj.Value.OpenAndAttach(Attaches);
                    }
                    catch (Exception ex)
                    {
                        if (_pool.SetUnavailable(ex) == true)
                            throw new Exception($"【{this.Name}】状态不可用，等待后台检查程序恢复方可使用。{ex.Message}");
                    }
                }
            }
        }

        async public Task OnGetAsync(Object<DbConnection> obj)
        {

            if (_pool.IsAvailable)
            {
                if (obj.Value == null)
                {
                    if (_pool.SetUnavailable(new Exception("连接字符串错误")) == true)
                        throw new Exception($"【{this.Name}】连接字符串错误，请检查。");
                    return;
                }

                if (obj.Value.State != ConnectionState.Open || DateTime.Now.Subtract(obj.LastReturnTime).TotalSeconds > 60 && (await obj.Value.PingAsync()) == false)
                {

                    try
                    {
                        await obj.Value.OpenAndAttachAsync(Attaches);
                    }
                    catch (Exception ex)
                    {
                        if (_pool.SetUnavailable(ex) == true)
                            throw new Exception($"【{this.Name}】状态不可用，等待后台检查程序恢复方可使用。{ex.Message}");
                    }
                }
            }
        }

        public void OnGetTimeout()
        {

        }

        public void OnReturn(Object<DbConnection> obj)
        {

        }

        public void OnAvailable()
        {
            _pool.availableHandler?.Invoke();
        }

        public void OnUnavailable()
        {
            _pool.unavailableHandler?.Invoke();
        }
    }
    static class DbConnectionExtensions
    {

        static DbCommand PingCommand(DbConnection conn)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandTimeout = 5;
            cmd.CommandText = "select 1";
            return cmd;
        }
        public static bool Ping(this DbConnection that, bool isThrow = false)
        {
            try
            {
                PingCommand(that).ExecuteNonQuery();
                return true;
            }
            catch
            {
                if (that.State != ConnectionState.Closed) try { that.Close(); } catch { }
                if (isThrow) throw;
                return false;
            }
        }
        async public static Task<bool> PingAsync(this DbConnection that, bool isThrow = false)
        {
            try
            {
                await PingCommand(that).ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                if (that.State != ConnectionState.Closed) try { that.Close(); } catch { }
                if (isThrow) throw;
                return false;
            }
        }

        public static void OpenAndAttach(this DbConnection that, string[] attach)
        {
            that.Open();

            if (attach?.Any() == true)
            {
                var sb = new StringBuilder();
                foreach (var att in attach)
                    sb.Append($"attach database [{att}] as [{att.Split('.').First()}];\r\n");

                var cmd = that.CreateCommand();
                cmd.CommandText = sb.ToString();
                cmd.ExecuteNonQuery();
            }
        }
        async public static Task OpenAndAttachAsync(this DbConnection that, string[] attach)
        {
            await that.OpenAsync();

            if (attach?.Any() == true)
            {
                var sb = new StringBuilder();
                foreach (var att in attach)
                    sb.Append($"attach database [{att}] as [{att.Split('.').First()}];\r\n");

                var cmd = that.CreateCommand();
                cmd.CommandText = sb.ToString();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
