using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{
    public class DbConnectionPool : IObjectPool<DbConnection>
    {
        internal DataType _dataType;
        internal Func<DbConnection> _connectionFactory;
        public DbConnection TestConnection { get; }
        public bool IsSingletonConnection { get; }
        int _id;
        public DbConnectionPool(DataType dataType, Func<DbConnection> connectionFactory)
        {
            #region Test connectionFactory
            //情况1：() => new SqlConnection(...)
            //情况2：() => conn
            DbConnection conn1 = null;
            DbConnection conn2 = null;
            try
            {
                conn1 = connectionFactory(); //测试 conn
                conn2 = connectionFactory();

                TestConnection = conn1; //赋值创建 Command，兼容 Mono.Data.Sqlite
                IsSingletonConnection = conn1 == conn2;
            }
            catch { }
            finally
            {
                if (conn1 != conn2)
                {
                    if (conn1?.State == ConnectionState.Open) try { conn1?.Close(); } catch { }
                    if (conn2?.State == ConnectionState.Open) try { conn2?.Close(); } catch { }
                }
            }
            #endregion

            _dataType = dataType;
            _connectionFactory = connectionFactory;
            Policy = new DbConnectionPoolPolicy(this);
        }

        public IPolicy<DbConnection> Policy { get; }

        public bool IsAvailable => true;
        public Exception UnavailableException => null;
        public DateTime? UnavailableTime => null;
        public string Statistics => "throw new NotImplementedException()";
        public string StatisticsFullily => "throw new NotImplementedException()";

        public void Dispose()
        {
        }

        public Object<DbConnection> Get(TimeSpan? timeout = null)
        {
            var conn = _connectionFactory();
            if (conn.State != ConnectionState.Open)
                conn.Open();
            return Object<DbConnection>.InitWith(this, Interlocked.Increment(ref _id), conn);
        }

#if net40
#else
        async public Task<Object<DbConnection>> GetAsync()
        {
            var conn = _connectionFactory();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();
            return Object<DbConnection>.InitWith(this, Interlocked.Increment(ref _id), conn);
        }
#endif

        public void Return(Object<DbConnection> obj, bool isReset = false)
        {
            if (obj == null || obj.Value == null) return;
            if (IsSingletonConnection) return;
            if (obj.Value.State != ConnectionState.Closed)
                obj.Value.Close();
            if (_dataType == DataType.Sqlite)
                obj.Value.Dispose();
        }

        public bool SetUnavailable(Exception exception)
        {
            throw new NotImplementedException();
        }
    }

    internal class DbConnectionPoolPolicy : IPolicy<DbConnection>
    {
        DbConnectionPool Pool;
        public DbConnectionPoolPolicy(DbConnectionPool pool)
        {
            this.Pool = pool;
        }

        public string Name { get; set; } = typeof(DbConnectionPoolPolicy).GetType().FullName;
        public int PoolSize { get; set; } = 1000;
        public TimeSpan SyncGetTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromSeconds(50);
        public int AsyncGetCapacity { get; set; } = 10000;
        public bool IsThrowGetTimeoutException { get; set; } = true;
        public int CheckAvailableInterval { get; set; } = 5;

        public DbConnection OnCreate()
        {
            var conn = Pool._connectionFactory();
            if (conn.State != ConnectionState.Open)
                conn.Open();
            return conn;
        }

        public void OnDestroy(DbConnection obj)
        {
            if (obj != null)
            {
                if (obj.State != ConnectionState.Closed)
                    obj.Close();
                //obj.Dispose();
            }
        }

        public void OnGet(Object<DbConnection> obj)
        {
        }

#if net40
#else
        public Task OnGetAsync(Object<DbConnection> obj)
        {
            return Task.FromResult(true);
        }
#endif

        public void OnGetTimeout()
        {
        }

        public void OnReturn(Object<DbConnection> obj)
        {
        }

        public bool OnCheckAvailable(Object<DbConnection> obj)
        {
            return true;
        }

        public void OnAvailable()
        {
        }

        public void OnUnavailable()
        {
        }
    }
}
