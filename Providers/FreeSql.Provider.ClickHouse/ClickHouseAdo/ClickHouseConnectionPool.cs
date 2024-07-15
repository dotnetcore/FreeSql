﻿using FreeSql.Internal.ObjectPool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClickHouse.Client.ADO;

namespace FreeSql.ClickHouse
{

    class ClickHouseConnectionPool : ObjectPool<DbConnection>
    {

        internal Action availableHandler;
        internal Action unavailableHandler;

        public ClickHouseConnectionPool(string name, string connectionString, Action availableHandler, Action unavailableHandler) : base(null)
        {
            this.availableHandler = availableHandler;
            this.unavailableHandler = unavailableHandler;
            var policy = new ClickHouseConnectionPoolPolicy
            {
                _pool = this,
                Name = name
            };
            this.Policy = policy;
            policy.ConnectionString = connectionString;
        }

        public void Return(Object<DbConnection> obj, Exception exception, bool isRecreate = false)
        {
            if (exception != null && exception is ClickHouseException)
            {
                if (obj.Value.Ping() == false)
                    base.SetUnavailable(exception, obj.LastGetTimeCopy);
            }
            base.Return(obj, isRecreate);
        }
    }
    public class ClickHouseException : Exception
    {
        public int Code;

        public string Name;
        public string ServerStackTrace;

        public ClickHouseException() { }

        public ClickHouseException(string message) : base(message) { }

        public ClickHouseException(string message, Exception innerException) : base(message, innerException) { }
    }
    class ClickHouseConnectionPoolPolicy : IPolicy<DbConnection>
    {

        internal ClickHouseConnectionPool _pool;
        public string Name { get; set; } = $"ClickHouse ClickHouseConnection {CoreStrings.S_ObjectPool}";
        public int PoolSize { get; set; } = 100;
        public TimeSpan SyncGetTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromSeconds(20);
        public int AsyncGetCapacity { get; set; } = 10000;
        public bool IsThrowGetTimeoutException { get; set; } = true;
        public bool IsAutoDisposeWithSystem { get; set; } = true;
        public int CheckAvailableInterval { get; set; } = 2;
        public int Weight { get; set; } = 1;

        static ConcurrentDictionary<string, int> dicConnStrIncr =FreeSql.Internal. Utils.GlobalCacheFactory.CreateCacheItem(new ConcurrentDictionary<string, int>(StringComparer.CurrentCultureIgnoreCase));
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
                if (m.Success)
                {
                    PoolSize = int.Parse(m.Groups[1].Value);
                    _connectionString = Regex.Replace(_connectionString, pattern, "", RegexOptions.IgnoreCase);
                }

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

        public DbConnection OnCreate()
        {
            var conn = new ClickHouseConnection(_connectionString);
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
                    _pool.SetUnavailable(new Exception(CoreStrings.S_ConnectionStringError), obj.LastGetTimeCopy);
                    throw new Exception(CoreStrings.S_ConnectionStringError_Check(this.Name));
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
                        throw ex;
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
                    _pool.SetUnavailable(new Exception(CoreStrings.S_ConnectionStringError), obj.LastGetTimeCopy);
                    throw new Exception(CoreStrings.S_ConnectionStringError_Check(this.Name));
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
                        throw ex;
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
