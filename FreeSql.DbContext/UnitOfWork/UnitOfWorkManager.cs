
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;

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
            if (fsql == null) throw new ArgumentNullException($"{nameof(UnitOfWorkManager)} 构造参数 {nameof(fsql)} 不能为 null");
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
                case Propagation.Mandatory: return FindedUowCreateVirtual() ?? throw new Exception("Propagation_Mandatory: 使用当前事务，如果没有当前事务，就抛出异常");
                case Propagation.NotSupported: return CreateUowNothing(true);
                case Propagation.Never:
                    var isNotSupported = _allUows.LastOrDefault()?.IsNotSupported ?? false;
                    if (isNotSupported == false)
                    {
                        for (var a = _rawUows.Count - 1; a >= 0; a--)
                            if (_rawUows[a].Uow.GetOrBeginTransaction(false) != null)
                                throw new Exception("Propagation_Never: 以非事务方式执行操作，如果当前事务存在则抛出异常");
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
            var uow = new UnitOfWorkOrginal(new UnitOfWork(OrmOriginal));
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
        class UnitOfWorkOrginal : IUnitOfWork
        {
            IUnitOfWork _baseUow;
            internal Action OnDispose;
            public UnitOfWorkOrginal(IUnitOfWork baseUow) => _baseUow = baseUow;
            public IFreeSql Orm => _baseUow.Orm;
            public IsolationLevel? IsolationLevel { get => _baseUow.IsolationLevel; set => _baseUow.IsolationLevel = value; }
            public DbContext.EntityChangeReport EntityChangeReport => _baseUow.EntityChangeReport;

            public DbTransaction GetOrBeginTransaction(bool isCreate = true) => _baseUow.GetOrBeginTransaction(isCreate);
            public void Commit() => _baseUow.Commit();
            public void Rollback() => _baseUow.Rollback();
            public void Dispose()
            {
                _baseUow.Dispose();
                OnDispose?.Invoke();
            }
        }
        class UnitOfWorkVirtual : IUnitOfWork
        {
            IUnitOfWork _baseUow;
            internal Action OnDispose;
            public UnitOfWorkVirtual(IUnitOfWork baseUow) => _baseUow = baseUow;
            public IFreeSql Orm => _baseUow.Orm;
            public IsolationLevel? IsolationLevel { get => _baseUow.IsolationLevel; set { } }
            public DbContext.EntityChangeReport EntityChangeReport => _baseUow.EntityChangeReport;

            public DbTransaction GetOrBeginTransaction(bool isCreate = true) => _baseUow.GetOrBeginTransaction(isCreate);
            public void Commit() { }
            public void Rollback() => _baseUow.Rollback();
            public void Dispose() => OnDispose?.Invoke();
        }
        class UnitOfWorkNothing : IUnitOfWork
        {
            internal IFreeSql _fsql;
            internal Action OnDispose;
            public UnitOfWorkNothing(IFreeSql fsql) => _fsql = fsql;
            public IFreeSql Orm => _fsql;
            public IsolationLevel? IsolationLevel { get; set; }
            public DbContext.EntityChangeReport EntityChangeReport { get; } = new DbContext.EntityChangeReport();

            public DbTransaction GetOrBeginTransaction(bool isCreate = true) => null;
            public void Commit()
            {
                if (EntityChangeReport != null && EntityChangeReport.OnChange != null && EntityChangeReport.Report.Any() == true)
                    EntityChangeReport.OnChange.Invoke(EntityChangeReport.Report);
                EntityChangeReport?.Report.Clear();
            }
            public void Rollback() => EntityChangeReport?.Report.Clear();
            public void Dispose() => OnDispose?.Invoke();
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
