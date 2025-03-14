﻿using FreeSql.Internal.ObjectPool;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TDengine.Data.Client;
using TDengine.Driver;
using TDengine.Driver.Client;

namespace FreeSql.TDengine
{
    internal class TDengineConnectionPool : ObjectPool<DbConnection>
    {
        internal Action AvailableHandler;

        internal Action UnavailableHandler;

        public TDengineConnectionPool(string name, string connectionString, Action availableHandler,
            Action unavailableHandler) : base(null)
        {
            this.AvailableHandler = availableHandler;
            this.UnavailableHandler = unavailableHandler;
            var policy = new TDengineConnectionPoolPolicy
            {
                InternalPool = this,
                Name = name
            };
            this.Policy = policy;
            policy.ConnectionString = connectionString;
        }

        public void Return(Object<DbConnection> obj, Exception exception, bool isRecreate = false)
        {
            base.Return(obj, isRecreate);
        }
    }

    internal class TDengineConnectionPoolPolicy : IPolicy<DbConnection>
    {
        internal TDengineConnectionPool InternalPool;
        public string Name { get; set; } = $"TDengine Connection {CoreErrorStrings.S_ObjectPool}";
        public int PoolSize { get; set; } = 50;
        public TimeSpan SyncGetTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromSeconds(20);
        public int AsyncGetCapacity { get; set; } = 10000;
        public bool IsThrowGetTimeoutException { get; set; } = true;
        public bool IsAutoDisposeWithSystem { get; set; } = true;
        public int CheckAvailableInterval { get; set; } = 2;
        public int Weight { get; set; } = 1;

        static readonly ConcurrentDictionary<string, int> DicConnStrIncr =
            new ConcurrentDictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);

        private string _connectionString;

        public string ConnectionString
        {
            get => _connectionString;
            set
            {
                _connectionString = value ?? "";

                var minPoolSize = 0;
                var pattern = @"Min(imum)?\s*pool\s*size\s*=\s*(\d+)";
                var m = Regex.Match(_connectionString, pattern, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    minPoolSize = int.Parse(m.Groups[2].Value);
                    _connectionString = Regex.Replace(_connectionString, pattern, "", RegexOptions.IgnoreCase);
                }

                pattern = @"Max(imum)?\s*pool\s*size\s*=\s*(\d+)";
                m = Regex.Match(_connectionString, pattern, RegexOptions.IgnoreCase);
                if (m.Success == false || int.TryParse(m.Groups[2].Value, out var poolsize) == false || poolsize <= 0)
                    poolsize = Math.Max(50, minPoolSize);
                var connStrIncr =
                    DicConnStrIncr.AddOrUpdate(_connectionString, 1, (oldkey, oldval) => Math.Min(5, oldval + 1));
                PoolSize = poolsize + connStrIncr;
                _connectionString = m.Success
                    ? Regex.Replace(_connectionString, pattern, $"Maximum pool size={PoolSize}",
                        RegexOptions.IgnoreCase)
                    : $"{_connectionString};Maximum pool size={PoolSize}";

                pattern = @"Connection\s*LifeTime\s*=\s*(\d+)";
                m = Regex.Match(_connectionString, pattern, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    IdleTimeout = TimeSpan.FromSeconds(int.Parse(m.Groups[1].Value));
                    _connectionString = Regex.Replace(_connectionString, pattern, "", RegexOptions.IgnoreCase);
                }

                FreeSql.Internal.CommonUtils.PrevReheatConnectionPool(InternalPool, minPoolSize);
            }
        }

        public DbConnection OnCreate()
        {
            var conn = new TDengineConnection(_connectionString);
            return conn;
        }

        public void OnDestroy(DbConnection obj)
        {
            if (obj.State != ConnectionState.Closed) obj.Close();
            obj.Dispose();
        }

        public void OnGetTimeout()
        {
        }

        public void OnGet(Object<DbConnection> obj)
        {
            if (InternalPool.IsAvailable)
            {
                if (obj.Value == null)
                {
                    InternalPool.SetUnavailable(new Exception(CoreErrorStrings.S_ConnectionStringError),
                        obj.LastGetTimeCopy);
                    throw new Exception(CoreErrorStrings.S_ConnectionStringError_Check(this.Name));
                }

                if (obj.Value.State != ConnectionState.Open ||
                    DateTime.Now.Subtract(obj.LastReturnTime).TotalSeconds > 60 && obj.Value.Ping() == false)
                {
                    try
                    {
                        obj.Value.Open();
                    }
                    catch (Exception ex)
                    {
                        if (InternalPool.SetUnavailable(ex, obj.LastGetTimeCopy) == true)
                            throw new Exception($"【{this.Name}】Block access and wait for recovery: {ex.Message}");
                        throw ex;
                    }
                }
            }
        }

#if net40
#else
        public async Task OnGetAsync(Object<DbConnection> obj)
        {
            if (InternalPool.IsAvailable)
            {
                if (obj.Value == null)
                {
                    InternalPool.SetUnavailable(new Exception(CoreErrorStrings.S_ConnectionStringError),
                        obj.LastGetTimeCopy);
                    throw new Exception(CoreErrorStrings.S_ConnectionStringError_Check(this.Name));
                }

                if (obj.Value.State != ConnectionState.Open ||
                    DateTime.Now.Subtract(obj.LastReturnTime).TotalSeconds > 60 &&
                    (await obj.Value.PingAsync()) == false)
                {
                    try
                    {
                        await obj.Value.OpenAsync();
                    }
                    catch (Exception ex)
                    {
                        if (InternalPool.SetUnavailable(ex, obj.LastGetTimeCopy) == true)
                            throw new Exception($"【{this.Name}】Block access and wait for recovery: {ex.Message}");
                        throw ex;
                    }
                }
            }
        }
#endif
        public void OnReturn(Object<DbConnection> obj)
        {
        }

        public bool OnCheckAvailable(Object<DbConnection> obj)
        {
            if (obj.Value == null) return false;
            if (obj.Value.State == ConnectionState.Closed) obj.Value.Open();
            return obj.Value.Ping(true);
        }

        public void OnAvailable()
        {
            InternalPool.AvailableHandler?.Invoke();
        }

        public void OnUnavailable()
        {
            InternalPool.UnavailableHandler?.Invoke();
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
                if (that.State != ConnectionState.Closed)
                    try
                    {
                        that.Close();
                    }
                    catch
                    {
                        // ignored
                    }

                if (isThrow) throw;
                return false;
            }
        }

#if net40
#else
        public static async Task<bool> PingAsync(this DbConnection that, bool isThrow = false)
        {
            try
            {
                await PingCommand(that).ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                if (that.State != ConnectionState.Closed)
                    try
                    {
                        that.Close();
                    }
                    catch
                    {
                    }

                if (isThrow) throw;
                return false;
            }
        }
#endif
    }
}