using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;

namespace FreeSql
{
    public class UnitOfWork : IUnitOfWork
    {
#if netcoreapp
        public static readonly AsyncLocal<IUnitOfWork> Current = new AsyncLocal<IUnitOfWork>();
#endif

        static int _seed;
        /// <summary>
        /// 正在使用中的工作单元（调试）
        /// </summary>
        public static ConcurrentDictionary<string, UnitOfWork> DebugBeingUsed { get; } = new ConcurrentDictionary<string, UnitOfWork>();

        protected IFreeSql _fsql;
        protected Object<DbConnection> _conn;
        protected DbTransaction _tran;

        /// <summary>
        /// 开启事务后有值，是 UnitOfWork 的唯一标识<para></para>
        /// 格式：yyyyMMdd_HHmmss_种子id<para></para>
        /// 例如：20191121_214504_1
        /// </summary>
        public string Id { get; private set; }

        public UnitOfWork(IFreeSql fsql)
        {
            _fsql = fsql;
#if netcoreapp
            Current.Value = this;
#endif
        }

        void ReturnObject()
        {
            if (string.IsNullOrEmpty(this.Id) == false && DebugBeingUsed.TryRemove(this.Id, out var old))
                this.Id = null;

            _fsql.Ado.MasterPool.Return(_conn);
            _tran = null;
            _conn = null;
#if netcoreapp
            Current.Value = null;
#endif
            EntityChangeReport?.Report.Clear();
        }

        public bool Enable { get; private set; } = true;

        public void Close()
        {
            if (_tran != null)
                throw new Exception("已开启事务，不能禁用工作单元");

            Enable = false;
        }
        public void Open()
        {
            Enable = true;
        }

        public IsolationLevel? IsolationLevel { get; set; }

        public DbTransaction GetOrBeginTransaction(bool isCreate = true)
        {
            if (_tran != null) return _tran;
            if (isCreate == false) return null;
            if (!Enable) return null;
            if (_conn != null) _fsql.Ado.MasterPool.Return(_conn);

            _conn = _fsql.Ado.MasterPool.Get();
            try
            {
                _tran = IsolationLevel == null ?
                    _conn.Value.BeginTransaction() :
                    _conn.Value.BeginTransaction(IsolationLevel.Value);

                this.Id = $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}_{Interlocked.Increment(ref _seed)}";
                DebugBeingUsed.TryAdd(this.Id, this);
            }
            catch
            {
                ReturnObject();
                throw;
            }
            return _tran;
        }

        public void Commit()
        {
            try
            {
                if (_tran != null)
                {
                    _tran.Commit();
                    if (EntityChangeReport != null && EntityChangeReport.OnChange != null && EntityChangeReport.Report.Any() == true)
                        EntityChangeReport.OnChange.Invoke(EntityChangeReport.Report);
                }
            }
            finally
            {
                ReturnObject();
            }
        }
        public void Rollback()
        {
            try
            {
                if (_tran != null) _tran.Rollback();
            }
            finally
            {
                ReturnObject();
            }
        }

        public DbContext.EntityChangeReport EntityChangeReport { get; } = new DbContext.EntityChangeReport();

        ~UnitOfWork() => this.Dispose();
        int _disposeCounter;
        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            try
            {
                this.Rollback();
                this.Close();
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}
