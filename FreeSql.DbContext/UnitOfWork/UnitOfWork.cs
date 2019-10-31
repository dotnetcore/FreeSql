using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;

namespace FreeSql
{
    public class UnitOfWork : IUnitOfWork
    {
#if ns20
        public static readonly AsyncLocal<IUnitOfWork> Current = new AsyncLocal<IUnitOfWork>();
#endif

        protected IFreeSql _fsql;
        protected Object<DbConnection> _conn;
        protected DbTransaction _tran;

        public UnitOfWork(IFreeSql fsql)
        {
            _fsql = fsql;
#if ns20
            Current.Value = this;
#endif
        }

        void ReturnObject()
        {
            _fsql.Ado.MasterPool.Return(_conn);
            _tran = null;
            _conn = null;
#if ns20
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

        ~UnitOfWork()
        {
            this.Dispose();
        }
        bool _isdisposed = false;
        public void Dispose()
        {
            if (_isdisposed) return;
            _isdisposed = true;
            this.Rollback();
            this.Close();
            GC.SuppressFinalize(this);
        }
    }
}
