﻿#if oledb
using System.Data.OleDb;
using OracleException = System.Data.OleDb.OleDbException;
using OracleConnection = System.Data.OleDb.OleDbConnection;
#else
using Oracle.ManagedDataAccess.Client;
#endif
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FreeSql.Oracle
{

    class OracleConnectionPool : ObjectPool<DbConnection>
    {

        internal Action availableHandler;
        internal Action unavailableHandler;
        internal string UserId { get; set; }

        public OracleConnectionPool(string name, string connectionString, Action availableHandler, Action unavailableHandler) : base(null)
        {
            this.availableHandler = availableHandler;
            this.unavailableHandler = unavailableHandler;
            this.UserId = OracleConnectionPool.GetUserId(connectionString);

            var policy = new OracleConnectionPoolPolicy
            {
                _pool = this,
                Name = name
            };
            this.Policy = policy;
            policy.ConnectionString = connectionString;
        }

        public static string GetUserId(string connectionString)
        {
            var userIdMatch = Regex.Match(connectionString, @"(User\s+Id|Uid)\s*=\s*([^;]+)", RegexOptions.IgnoreCase);
            if (userIdMatch.Success == false) throw new Exception(@"从 ConnectionString 中无法匹配 (User\s+Id|Uid)\s*=\s*([^;]+)");
            return userIdMatch.Groups[2].Value.Trim().ToUpper();
        }

        public void Return(Object<DbConnection> obj, Exception exception, bool isRecreate = false)
        {
            if (exception != null && exception is OracleException)
            {
                if (obj.Value.Ping() == false)
                    base.SetUnavailable(exception, obj.LastGetTimeCopy);
            }
            base.Return(obj, isRecreate);
        }

        public static DbConnection CreateConnection(string connectionString)
        {
            var conn = new OracleConnection(connectionString);
            return conn;
        }
    }

    class OracleConnectionPoolPolicy : IPolicy<DbConnection>
    {

        internal OracleConnectionPool _pool;
        public string Name { get; set; } = $"Oracle Connection {CoreErrorStrings.S_ObjectPool}";
        public int PoolSize { get; set; } = 100;
        public TimeSpan SyncGetTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromSeconds(20);
        public int AsyncGetCapacity { get; set; } = 10000;
        public bool IsThrowGetTimeoutException { get; set; } = true;
        public bool IsAutoDisposeWithSystem { get; set; } = true;
        public int CheckAvailableInterval { get; set; } = 2;
        public int Weight { get; set; } = 1;

        static ConcurrentDictionary<string, int> dicConnStrIncr = new ConcurrentDictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
        private string _connectionString;
        public string ConnectionString
        {
            get => _connectionString;
            set
            {
                _connectionString = value ?? "";

                var minPoolSize = 0;
                var pattern = @"Min\s*pool\s*size\s*=\s*(\d+)";
                var m = Regex.Match(_connectionString, pattern, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    minPoolSize = int.Parse(m.Groups[1].Value);
                    _connectionString = Regex.Replace(_connectionString, pattern, "", RegexOptions.IgnoreCase);
                }

                pattern = @"Max\s*pool\s*size\s*=\s*(\d+)";
                m = Regex.Match(_connectionString, pattern, RegexOptions.IgnoreCase);
                if (m.Success == false || int.TryParse(m.Groups[1].Value, out var poolsize) == false || poolsize <= 0) poolsize = Math.Max(100, minPoolSize);
                var connStrIncr = dicConnStrIncr.AddOrUpdate(_connectionString, 1, (oldkey, oldval) => Math.Min(5, oldval + 1));
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

                FreeSql.Internal.CommonUtils.PrevReheatConnectionPool(_pool, minPoolSize);
            }
        }

        public bool OnCheckAvailable(Object<DbConnection> obj)
        {
            if (obj.Value == null) return false;
            if (obj.Value.State == ConnectionState.Closed) obj.Value.Open();
            return obj.Value.Ping(true);
        }

        public DbConnection OnCreate() => OracleConnectionPool.CreateConnection(_connectionString);

        public void OnDestroy(DbConnection obj)
        {
            try { if (obj.State != ConnectionState.Closed) obj.Close(); } catch { }
#if oledb
#else
            try { OracleConnection.ClearPool(obj as OracleConnection); } catch { }
#endif
            obj.Dispose();
        }

        public void OnGet(Object<DbConnection> obj)
        {

            if (_pool.IsAvailable)
            {
                if (obj.Value == null)
                {
                    _pool.SetUnavailable(new Exception(CoreErrorStrings.S_ConnectionStringError), obj.LastGetTimeCopy);
                    throw new Exception(CoreErrorStrings.S_ConnectionStringError_Check(this.Name));
                }

                if (obj.Value.State != ConnectionState.Open || DateTime.Now.Subtract(obj.LastReturnTime).TotalSeconds > 60 && obj.Value.Ping() == false)
                {

                    try
                    {
                        obj.Value.Open();
                    }
                    catch (Exception ex)
                    {
                        if (_pool.SetUnavailable(ex, obj.LastGetTimeCopy) == true)
                            throw new Exception($"【{this.Name}】Block access and wait for recovery: {ex.Message}");
                        throw;
                    }
                }
            }
        }

#if net40
#else
        async public Task OnGetAsync(Object<DbConnection> obj)
        {

            if (_pool.IsAvailable)
            {
                if (obj.Value == null)
                {
                    _pool.SetUnavailable(new Exception(CoreErrorStrings.S_ConnectionStringError), obj.LastGetTimeCopy);
                    throw new Exception(CoreErrorStrings.S_ConnectionStringError_Check(this.Name));
                }

                if (obj.Value.State != ConnectionState.Open || DateTime.Now.Subtract(obj.LastReturnTime).TotalSeconds > 60 && (await obj.Value.PingAsync()) == false)
                {

                    try
                    {
                        await obj.Value.OpenAsync();
                    }
                    catch (Exception ex)
                    {
                        if (_pool.SetUnavailable(ex, obj.LastGetTimeCopy) == true)
                            throw new Exception($"【{this.Name}】Block access and wait for recovery: {ex.Message}");
                        throw;
                    }
                }
            }
        }
#endif

        public void OnGetTimeout()
        {

        }

        public void OnReturn(Object<DbConnection> obj)
        {
            //if (obj?.Value != null && obj.Value.State != ConnectionState.Closed) try { obj.Value.Close(); } catch { }
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
            cmd.CommandText = "select 1 from dual";
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

#if net40
#else
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
#endif
    }
}
