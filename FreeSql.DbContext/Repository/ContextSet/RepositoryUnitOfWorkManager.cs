using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace FreeSql
{
    /// <summary>
    /// 仓储的工作单元管理器
    /// </summary>
    public class RepositoryUnitOfWorkManager : IDisposable
    {
        IFreeSql _fsql;
        List<IRepositoryUnitOfWork> _uows = new List<IRepositoryUnitOfWork>();
        bool _isNotSupported = false;

        public RepositoryUnitOfWorkManager(IFreeSql fsql)
        {
            _fsql = fsql ?? throw new ArgumentNullException($"{nameof(RepositoryUnitOfWorkManager)} 构造参数 {nameof(fsql)} 不能为 null");
        }

        ~RepositoryUnitOfWorkManager() => this.Dispose();
        int _disposeCounter;
        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            try
            {
                Exception exception = null;
                for (var a = _uows.Count - 1; a >= 0; a--)
                {
                    try
                    {
                        if (exception == null) _uows[a].Commit();
                        else _uows[a].Rollback();
                    }
                    catch (Exception ex)
                    {
                        if (exception == null) exception = ex;
                    }
                }
                if (exception != null) throw exception;
            }
            finally
            {
                _uows.Clear();
                GC.SuppressFinalize(this);
            }
        }

        public enum Propagation
        {
            /// <summary>
            /// 如果当前没有事务，就新建一个事务，如果已存在一个事务中，加入到这个事务中，默认的选择。
            /// </summary>
            Requierd,
            /// <summary>
            /// 支持当前事务，如果没有当前事务，就以非事务方法执行。
            /// </summary>
            Supports,
            /// <summary>
            /// 使用当前事务，如果没有当前事务，就抛出异常。
            /// </summary>
            Mandatory,
            /// <summary>
            /// 以非事务方式执行操作，如果当前存在事务，就把当前事务挂起。
            /// </summary>
            NotSupported,
            /// <summary>
            /// 以非事务方式执行操作，如果当前事务存在则抛出异常。
            /// </summary>
            Never,
            /// <summary>
            /// 如果当前存在事务，则在嵌套事务内执行。如果当前没有事务，就新建一个事务。
            /// </summary>
            Nested
        }

        public IRepositoryUnitOfWork Begin(Propagation propagation, IsolationLevel? isolationLevel = null)
        {
            if (propagation == Propagation.Requierd)
            {
                if (_isNotSupported == false)
                {
                    for (var a = _uows.Count - 1; a >= 0; a--)
                        if (_uows[a].GetOrBeginTransaction(false) != null)
                            return new UnitOfWorkProxy(_uows[a]);
                }
                var uow = new RepositoryUnitOfWork(_fsql);
                if (isolationLevel != null) uow.IsolationLevel = isolationLevel.Value;
                try { uow.GetOrBeginTransaction(); }
                catch { uow.Dispose(); throw; }
                _uows.Add(uow);
                return uow;
            }
            if (propagation == Propagation.Supports)
            {
                if (_isNotSupported == false)
                {
                    for (var a = _uows.Count - 1; a >= 0; a--)
                        if (_uows[a].GetOrBeginTransaction(false) != null)
                            return new UnitOfWorkProxy(_uows[a]);
                }
                return new UnitOfWorkNothing(_fsql);
            }
            if (propagation == Propagation.Mandatory)
            {
                if (_isNotSupported == false)
                {
                    for (var a = _uows.Count - 1; a >= 0; a--)
                        if (_uows[a].GetOrBeginTransaction(false) != null)
                            return new UnitOfWorkProxy(_uows[a]);
                    throw new Exception("Propagation_Mandatory: 使用当前事务，如果没有当前事务，就抛出异常");
                }
                throw new Exception("Propagation_Mandatory: 使用当前事务，如果没有当前事务，就抛出异常（NotSupported 事务挂起中）");
            }
            if (propagation == Propagation.NotSupported)
            {
                if (_isNotSupported == false)
                {
                    _isNotSupported = true;
                    return new UnitOfWorkNothing(_fsql) { OnDispose = () => _isNotSupported = false };
                }
                return new UnitOfWorkNothing(_fsql);
            }
            if (propagation == Propagation.Never)
            {
                if (_isNotSupported == false)
                {
                    for (var a = _uows.Count - 1; a >= 0; a--)
                        if (_uows[a].GetOrBeginTransaction(false) != null)
                            throw new Exception("Propagation_Never: 以非事务方式执行操作，如果当前事务存在则抛出异常");
                }
                return new UnitOfWorkNothing(_fsql);
            }
            if (propagation == Propagation.Nested)
            {
                var uow = new RepositoryUnitOfWork(_fsql);
                if (isolationLevel != null) uow.IsolationLevel = isolationLevel.Value;
                try { uow.GetOrBeginTransaction(); }
                catch { uow.Dispose(); throw; }
                _uows.Add(uow);
                return uow;
            }
            throw new NotImplementedException();
        }

        class UnitOfWorkProxy : IRepositoryUnitOfWork
        {
            IRepositoryUnitOfWork _baseUow;
            public UnitOfWorkProxy(IRepositoryUnitOfWork baseUow) => _baseUow = baseUow;
            public IsolationLevel? IsolationLevel { get => _baseUow.IsolationLevel; set { } }
            public DbContext.EntityChangeReport EntityChangeReport => _baseUow.EntityChangeReport;

            public bool Enable => _baseUow.Enable;
            public void Close() => _baseUow.Close();
            public void Open() => _baseUow.Open();

            public DbTransaction GetOrBeginTransaction(bool isCreate = true) => _baseUow.GetOrBeginTransaction(isCreate);
            public void Commit() => this.Dispose();
            public void Rollback() => _baseUow.Rollback();
            public void Dispose() { }

            public IBaseRepository<TEntity, TKey> GetRepository<TEntity, TKey>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class => _baseUow.GetRepository<TEntity, TKey>(filter);
            public IBaseRepository<TEntity> GetRepository<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class => _baseUow.GetRepository<TEntity>(filter);
            public IBaseRepository<TEntity, Guid> GetGuidRepository<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<string, string> asTable = null) where TEntity : class => _baseUow.GetGuidRepository<TEntity>(filter);
        }
        class UnitOfWorkNothing : IRepositoryUnitOfWork
        {
            internal IFreeSql _fsql;
            internal Action OnDispose;
            public UnitOfWorkNothing(IFreeSql fsql) => _fsql = fsql;
            public IsolationLevel? IsolationLevel { get; set; }
            public DbContext.EntityChangeReport EntityChangeReport { get; } = new DbContext.EntityChangeReport();

            public bool Enable { get; }
            public void Close() { }
            public void Open() { }

            public DbTransaction GetOrBeginTransaction(bool isCreate = true) => null;
            public void Commit()
            {
                if (EntityChangeReport != null && EntityChangeReport.OnChange != null && EntityChangeReport.Report.Any() == true)
                    EntityChangeReport.OnChange.Invoke(EntityChangeReport.Report);
                this.Dispose();
            }
            public void Rollback() => this.Dispose();
            public void Dispose() {
                EntityChangeReport?.Report.Clear();
                OnDispose?.Invoke();
            }

            public IBaseRepository<TEntity, TKey> GetRepository<TEntity, TKey>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class => new DefaultRepository<TEntity, TKey>(_fsql, filter);
            public IBaseRepository<TEntity> GetRepository<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class => new DefaultRepository<TEntity,  int>(_fsql, filter);
            public IBaseRepository<TEntity, Guid> GetGuidRepository<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<string, string> asTable = null) where TEntity : class => new GuidRepository<TEntity>(_fsql, filter, asTable);
        }
    }
}
