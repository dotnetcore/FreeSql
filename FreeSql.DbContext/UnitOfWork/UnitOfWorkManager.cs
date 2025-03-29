﻿
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{
    /// <summary>
    /// 工作单元管理器
    /// </summary>
    public class UnitOfWorkManager : IDisposable
    {
        internal DbContextScopedFreeSql _ormScoped;
        internal IFreeSql OrmOriginal => _ormScoped?._originalFsql;
        public IFreeSql Orm => _ormScoped;
        List<UowInfo> _rawUows = new List<UowInfo>();
        List<UowInfo> _allUows = new List<UowInfo>();
        List<RepoInfo> _repos = new List<RepoInfo>();

        public UnitOfWorkManager(IFreeSql fsql)
        {
            if (fsql == null) throw new ArgumentNullException(DbContextErrorStrings.UnitOfWorkManager_Construction_CannotBeNull(nameof(UnitOfWorkManager), nameof(fsql)));
            _ormScoped = DbContextScopedFreeSql.Create(fsql, null, () => this.Current);
        }

        #region Dispose
        ~UnitOfWorkManager() => this.Dispose();
        int _disposeCounter;
        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            try
            {
                Exception exception = null;
                for (var a = _rawUows.Count - 1; a >= 0; a--)
                {
                    try
                    {
                        if (exception == null) _rawUows[a].Uow.Commit();
                        else _rawUows[a].Uow.Rollback();
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
                _rawUows.Clear();
                _allUows.Clear();
                _repos.Clear();
                GC.SuppressFinalize(this);
            }
        }
        #endregion

        /// <summary>
        /// 当前的工作单元
        /// </summary>
        public IUnitOfWork Current => _allUows.LastOrDefault()?.Uow;

        /// <summary>
        /// 将仓储的事务交给我管理
        /// </summary>
        /// <param name="repository"></param>
        public void Binding(IBaseRepository repository)
        {
            var repoInfo = new RepoInfo(repository);
            repository.UnitOfWork = Current;
            if (_repos.Any(a => a.Repository == repository)) return;
            _repos.Add(repoInfo);
        }
        void SetAllRepositoryUow()
        {
            foreach (var repo in _repos)
                repo.Repository.UnitOfWork = Current ?? repo.OrginalUow;
        }

        /// <summary>
        /// 创建工作单元
        /// </summary>
        /// <param name="propagation">事务传播方式</param>
        /// <param name="isolationLevel">事务隔离级别</param>
        /// <returns></returns>
        public IUnitOfWork Begin(Propagation propagation = Propagation.Required, IsolationLevel? isolationLevel = null)
        {
            switch (propagation)
            {
                case Propagation.Required: return FindedUowCreateVirtual() ?? CreateUow(isolationLevel);
                case Propagation.Supports: return FindedUowCreateVirtual() ?? CreateUowNothing(_allUows.LastOrDefault()?.IsNotSupported ?? false);
                case Propagation.Mandatory: return FindedUowCreateVirtual() ?? throw new Exception(DbContextErrorStrings.Propagation_Mandatory);
                case Propagation.NotSupported: return CreateUowNothing(true);
                case Propagation.Never:
                    var isNotSupported = _allUows.LastOrDefault()?.IsNotSupported ?? false;
                    if (isNotSupported == false)
                    {
                        for (var a = _rawUows.Count - 1; a >= 0; a--)
                            if (_rawUows[a].Uow.GetOrBeginTransaction(false) != null)
                                throw new Exception(DbContextErrorStrings.Propagation_Never);
                    }
                    return CreateUowNothing(isNotSupported);
                case Propagation.Nested: return CreateUow(isolationLevel);
                default: throw new NotImplementedException();
            }
        }

        IUnitOfWork FindedUowCreateVirtual()
        {
            var isNotSupported = _allUows.LastOrDefault()?.IsNotSupported ?? false;
            if (isNotSupported == false)
            {
                for (var a = _rawUows.Count - 1; a >= 0; a--)
                    if (_rawUows[a].Uow.GetOrBeginTransaction(false) != null)
                    {
                        var uow = new UnitOfWorkVirtual(_rawUows[a].Uow);
                        var uowInfo = new UowInfo(uow, UowInfo.UowType.Virtual, isNotSupported);
                        uow.OnDispose = () => _allUows.Remove(uowInfo);
                        _allUows.Add(uowInfo);
                        SetAllRepositoryUow();
                        return uow;
                    }
            }
            return null;
        }
        IUnitOfWork CreateUowNothing(bool isNotSupported)
        {
            var uow = new UnitOfWorkNothing(Orm);
            var uowInfo = new UowInfo(uow, UowInfo.UowType.Nothing, isNotSupported);
            uow.OnDispose = () => _allUows.Remove(uowInfo);
            _allUows.Add(uowInfo);
            SetAllRepositoryUow();
            return uow;
        }
        IUnitOfWork CreateUow(IsolationLevel? isolationLevel)
        {
            var uow = new UnitOfWorkOriginal(new UnitOfWork(OrmOriginal));
            var uowInfo = new UowInfo(uow, UowInfo.UowType.Orginal, false);
            if (isolationLevel != null) uow.IsolationLevel = isolationLevel.Value;
            try { uow.GetOrBeginTransaction(); }
            catch { uow.Dispose(); throw; }

            uow.OnDispose = () =>
            {
                _rawUows.Remove(uowInfo);
                _allUows.Remove(uowInfo);
                SetAllRepositoryUow();
            };
            _rawUows.Add(uowInfo);
            _allUows.Add(uowInfo);
            SetAllRepositoryUow();
            return uow;
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// 创建工作单元
        /// </summary>
        /// <param name="propagation">事务传播方式</param>
        /// <param name="isolationLevel">事务隔离级别</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        public async Task<IUnitOfWork> BeginAsync(Propagation propagation = Propagation.Required, IsolationLevel? isolationLevel = null, CancellationToken cancellationToken = default)
        {
            switch (propagation)
            {
                case Propagation.Required: return await FindedUowCreateVirtualAsync(cancellationToken) ?? await CreateUowAsync(isolationLevel, cancellationToken);
                case Propagation.Supports: return await FindedUowCreateVirtualAsync(cancellationToken) ?? CreateUowNothing(_allUows.LastOrDefault()?.IsNotSupported ?? false);
                case Propagation.Mandatory: return await FindedUowCreateVirtualAsync(cancellationToken) ?? throw new Exception(DbContextStrings.Propagation_Mandatory);
                case Propagation.NotSupported: return CreateUowNothing(true);
                case Propagation.Never:
                    var isNotSupported = _allUows.LastOrDefault()?.IsNotSupported ?? false;
                    if (isNotSupported == false)
                    {
                        for (var a = _rawUows.Count - 1; a >= 0; a--)
                            if (await _rawUows[a].Uow.GetOrBeginTransactionAsync(false, cancellationToken) != null)
                                throw new Exception(DbContextStrings.Propagation_Never);
                    }
                    return CreateUowNothing(isNotSupported);
                case Propagation.Nested: return await CreateUowAsync(isolationLevel, cancellationToken);
                default: throw new NotImplementedException();
            }
        }

        async Task<IUnitOfWork> CreateUowAsync(IsolationLevel? isolationLevel, CancellationToken cancellationToken = default)
        {
            var uow = new UnitOfWorkOriginal(new UnitOfWork(OrmOriginal));
            var uowInfo = new UowInfo(uow, UowInfo.UowType.Orginal, false);
            if (isolationLevel != null) uow.IsolationLevel = isolationLevel.Value;
            try { await uow.GetOrBeginTransactionAsync(true, cancellationToken); }
            catch { uow.Dispose(); throw; }

            uow.OnDispose = () =>
            {
                _rawUows.Remove(uowInfo);
                _allUows.Remove(uowInfo);
                SetAllRepositoryUow();
            };
            _rawUows.Add(uowInfo);
            _allUows.Add(uowInfo);
            SetAllRepositoryUow();
            return uow;
        }
        async Task<IUnitOfWork> FindedUowCreateVirtualAsync(CancellationToken cancellationToken = default)
        {
            var isNotSupported = _allUows.LastOrDefault()?.IsNotSupported ?? false;
            if (isNotSupported == false)
            {
                for (var a = _rawUows.Count - 1; a >= 0; a--)
                    if (await _rawUows[a].Uow.GetOrBeginTransactionAsync(false, cancellationToken) != null)
                    {
                        var uow = new UnitOfWorkVirtual(_rawUows[a].Uow);
                        var uowInfo = new UowInfo(uow, UowInfo.UowType.Virtual, false);
                        uow.OnDispose = () => _allUows.Remove(uowInfo);
                        _allUows.Add(uowInfo);
                        SetAllRepositoryUow();
                        return uow;
                    }
            }
            return null;
        }
#endif
        class RepoInfo
        {
            public IBaseRepository Repository;
            public IUnitOfWork OrginalUow;

            public RepoInfo(IBaseRepository repository)
            {
                this.Repository = repository;
                this.OrginalUow = repository.UnitOfWork;
            }
        }
        class UowInfo
        {
            public IUnitOfWork Uow;
            public UowType Type;
            public bool IsNotSupported;
            public enum UowType { Orginal, Virtual, Nothing }

            public UowInfo(IUnitOfWork uow, UowType type, bool isNotSupported)
            {
                this.Uow = uow;
                this.Type = type;
                this.IsNotSupported = isNotSupported;
            }
        }
        class UnitOfWorkOriginal : IUnitOfWork
        {
            IUnitOfWork _baseUow;
            internal Action OnDispose;
            public UnitOfWorkOriginal(IUnitOfWork baseUow) => _baseUow = baseUow;
            public IFreeSql Orm => _baseUow.Orm;
            public IsolationLevel? IsolationLevel { get => _baseUow.IsolationLevel; set => _baseUow.IsolationLevel = value; }
            public DbContext.EntityChangeReport EntityChangeReport => _baseUow.EntityChangeReport;
            public Dictionary<string, object> States => _baseUow.States;

            public DbTransaction GetOrBeginTransaction(bool isCreate = true) => _baseUow.GetOrBeginTransaction(isCreate);
            public void Commit() => _baseUow.Commit();
            public void Rollback() => _baseUow.Rollback();
            public void Dispose()
            {
                _baseUow.Dispose();
                OnDispose?.Invoke();
            }
#if NETCOREAPP3_1_OR_GREATER
            public Task<DbTransaction> GetOrBeginTransactionAsync(bool isCreate = true, CancellationToken cancellationToken = default) => _baseUow.GetOrBeginTransactionAsync(isCreate, cancellationToken);
            public Task CommitAsync(CancellationToken cancellationToken = default) => _baseUow.CommitAsync(cancellationToken);
            public Task RollbackAsync(CancellationToken cancellationToken = default) => _baseUow.RollbackAsync(cancellationToken);
#endif
        }
        class UnitOfWorkVirtual : IUnitOfWork
        {
            IUnitOfWork _baseUow;
            internal Action OnDispose;
            public UnitOfWorkVirtual(IUnitOfWork baseUow) => _baseUow = baseUow;
            public IFreeSql Orm => _baseUow.Orm;
            public IsolationLevel? IsolationLevel { get => _baseUow.IsolationLevel; set { } }
            public DbContext.EntityChangeReport EntityChangeReport => _baseUow.EntityChangeReport;
            public Dictionary<string, object> States => _baseUow.States;

            public DbTransaction GetOrBeginTransaction(bool isCreate = true) => _baseUow.GetOrBeginTransaction(isCreate);
            public void Commit() { }
            public void Rollback() => _baseUow.Rollback();
            public void Dispose() => OnDispose?.Invoke();

#if NETCOREAPP3_1_OR_GREATER
            public Task<DbTransaction> GetOrBeginTransactionAsync(bool isCreate = true, CancellationToken cancellationToken = default) => _baseUow.GetOrBeginTransactionAsync(isCreate, cancellationToken);
            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task RollbackAsync(CancellationToken cancellationToken = default) => _baseUow.RollbackAsync(cancellationToken);
#endif
        }
        class UnitOfWorkNothing : IUnitOfWork
        {
            internal IFreeSql _fsql;
            internal Action OnDispose;
            public UnitOfWorkNothing(IFreeSql fsql) => _fsql = fsql;
            public IFreeSql Orm => _fsql;
            public IsolationLevel? IsolationLevel { get; set; }
            public DbContext.EntityChangeReport EntityChangeReport { get; } = new DbContext.EntityChangeReport();
            public Dictionary<string, object> States { get; } = new Dictionary<string, object>();

            public DbTransaction GetOrBeginTransaction(bool isCreate = true) => null;
            public void Commit()
            {
                if (EntityChangeReport?.OnChange != null && EntityChangeReport.Report.Any())
                    EntityChangeReport.OnChange.Invoke(EntityChangeReport.Report);
                EntityChangeReport?.Report.Clear();
            }
            public void Rollback() => EntityChangeReport?.Report.Clear();
            public void Dispose() => OnDispose?.Invoke();

#if NETCOREAPP3_1_OR_GREATER
            public Task<DbTransaction> GetOrBeginTransactionAsync(bool isCreate = true, CancellationToken cancellationToken = default) => null;
            public Task CommitAsync(CancellationToken cancellationToken = default)
            {
                if (EntityChangeReport?.OnChange != null && EntityChangeReport.Report.Any())
                    EntityChangeReport.OnChange.Invoke(EntityChangeReport.Report);
                EntityChangeReport?.Report.Clear();
                return Task.CompletedTask;
            }
            public Task RollbackAsync(CancellationToken cancellationToken = default)
            {
                EntityChangeReport?.Report.Clear();
                return Task.CompletedTask;
            }
#endif
        }
    }

    /// <summary>
    /// 事务传播方式
    /// </summary>
    public enum Propagation
    {
        /// <summary>
        /// 如果当前没有事务，就新建一个事务，如果已存在一个事务中，加入到这个事务中，默认的选择。
        /// </summary>
        Required,
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
        /// 以嵌套事务方式执行。
        /// </summary>
        Nested
    }
}
