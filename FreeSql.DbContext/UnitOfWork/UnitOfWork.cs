using FreeSql.Internal.ObjectPool;
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
        static int _seed;
        /// <summary>
        /// 正在使用中的工作单元（调试）
        /// </summary>
        public static ConcurrentDictionary<string, UnitOfWork> DebugBeingUsed { get; } = new ConcurrentDictionary<string, UnitOfWork>();

        protected IFreeSql _fsql;
        protected Object<DbConnection> _conn;
        protected DbTransaction _tran;
        protected Aop.TraceBeforeEventArgs _tranBefore;
        protected Aop.TraceBeforeEventArgs _uowBefore;

        /// <summary>
        /// 开启事务后有值，是 UnitOfWork 的唯一标识<para></para>
        /// 格式：yyyyMMdd_HHmmss_种子id<para></para>
        /// 例如：20191121_214504_1
        /// </summary>
        public string Id { get; private set; }

        public UnitOfWork(IFreeSql fsql)
        {
            _fsql = fsql;
            if (_fsql == null) throw new ArgumentNullException(nameof(fsql));

            _uowBefore = new Aop.TraceBeforeEventArgs("UnitOfWork", null);
            _fsql.Aop.TraceBeforeHandler?.Invoke(this, _uowBefore);
        }

        void ReturnObject()
        {
            if (string.IsNullOrEmpty(this.Id) == false && DebugBeingUsed.TryRemove(this.Id, out var old))
                this.Id = null;

            _fsql.Ado.MasterPool.Return(_conn);
            _tran = null;
            _conn = null;
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

        DbContextScopedFreeSql _ormScoped;
        public IFreeSql Orm => _ormScoped ?? (_ormScoped = DbContextScopedFreeSql.Create(_fsql, null, () => this));

        public IsolationLevel? IsolationLevel { get; set; }

        public DbTransaction GetOrBeginTransaction(bool isCreate = true)
        {
            if (_tran != null) return _tran;
            if (isCreate == false) return null;
            if (!Enable) return null;
            if (_conn != null) _fsql.Ado.MasterPool.Return(_conn);

            _tranBefore = new Aop.TraceBeforeEventArgs("BeginTransaction", IsolationLevel);
            _fsql?.Aop.TraceBeforeHandler?.Invoke(this, _tranBefore);
            try
            {
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
            }
            catch (Exception ex)
            {
                _fsql?.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(_tranBefore, "失败", ex));
#pragma warning disable CA2200 // 再次引发以保留堆栈详细信息
                throw ex;
#pragma warning restore CA2200 // 再次引发以保留堆栈详细信息
            }
            return _tran;
        }

        public void Commit()
        {
            var isCommited = false;
            try
            {
                if (_tran != null)
                {
                    if (_tran.Connection != null) _tran.Commit();
                    isCommited = true;
                    _fsql?.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(_tranBefore, "提交", null));

                    if (EntityChangeReport != null && EntityChangeReport.OnChange != null && EntityChangeReport.Report.Any() == true)
                        EntityChangeReport.OnChange.Invoke(EntityChangeReport.Report);
                }
            }
            catch (Exception ex)
            {
                if (isCommited == false)
                    _fsql?.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(_tranBefore, "提交失败", ex));
#pragma warning disable CA2200 // 再次引发以保留堆栈详细信息
                throw ex;
#pragma warning restore CA2200 // 再次引发以保留堆栈详细信息
            }
            finally
            {
                ReturnObject();
                _tranBefore = null;
            }
        }
        public void Rollback()
        {
            var isRollbacked = false;
            try
            {
                if (_tran != null)
                {
                    if (_tran.Connection != null) _tran.Rollback();
                    isRollbacked = true;
                    _fsql?.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(_tranBefore, "回滚", null));
                }
            }
            catch (Exception ex)
            {
                if (isRollbacked == false)
                    _fsql?.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(_tranBefore, "回滚失败", ex));
#pragma warning disable CA2200 // 再次引发以保留堆栈详细信息
                throw ex;
#pragma warning restore CA2200 // 再次引发以保留堆栈详细信息
            }
            finally
            {
                ReturnObject();
                _tranBefore = null;
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
            }
            finally
            {
                _fsql?.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(_uowBefore, "释放", null));
                GC.SuppressFinalize(this);
            }
        }
    }
}
