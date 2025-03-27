using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{

    partial class InsertProvider<T1>
    {
#if net40
#else
        async protected virtual Task<int> SplitExecuteAffrowsAsync(int valuesLimit, int parameterLimit, CancellationToken cancellationToken = default)
        {
            var ss = SplitSource(valuesLimit, parameterLimit);
            var ret = 0;
            if (ss.Any() == false)
            {
                ClearData();
                return ret;
            }
            if (ss.Length == 1)
            {
                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                ret = await this.RawExecuteAffrowsAsync(cancellationToken);
                ClearData();
                return ret;
            }
            if (_transaction == null)
            {
                var threadTransaction = _orm.Ado.TransactionCurrentThread;
                if (threadTransaction != null) this.WithTransaction(threadTransaction);
            }

            var before = new Aop.TraceBeforeEventArgs("SplitExecuteAffrowsAsync", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                if (_transaction != null || _batchAutoTransaction == false)
                {
                    for (var a = 0; a < ss.Length; a++)
                    {
                        _source = ss[a]; 
                        _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                        ret += await this.RawExecuteAffrowsAsync(cancellationToken);
                    }
                }
                else
                {
                    if (_orm.Ado.MasterPool == null) throw new Exception(CoreErrorStrings.MasterPool_IsNull_UseTransaction);
                    using (var conn = await _orm.Ado.MasterPool.GetAsync())
                    {
                        _transaction = conn.Value.BeginTransaction();
                        var transBefore = new Aop.TraceBeforeEventArgs("BeginTransaction", null);
                        _orm.Aop.TraceBeforeHandler?.Invoke(this, transBefore);
                        try
                        {
                            for (var a = 0; a < ss.Length; a++)
                            {
                                _source = ss[a];
                                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                                ret += await this.RawExecuteAffrowsAsync(cancellationToken);
                            }
                            _transaction.Commit();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreErrorStrings.Commit, null));
                        }
                        catch (Exception ex)
                        {
                            _transaction.Rollback();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreErrorStrings.RollBack, ex));
                            throw;
                        }
                        _transaction = null;
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.TraceAfterEventArgs(before, null, exception);
                _orm.Aop.TraceAfterHandler?.Invoke(this, after);
            }
            ClearData();
            return ret;
        }

        async protected virtual Task<long> SplitExecuteIdentityAsync(int valuesLimit, int parameterLimit, CancellationToken cancellationToken = default)
        {
            var ss = SplitSource(valuesLimit, parameterLimit);
            long ret = 0;
            if (ss.Any() == false)
            {
                ClearData();
                return ret;
            }
            if (ss.Length == 1)
            {
                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                ret = await this.RawExecuteIdentityAsync(cancellationToken);
                ClearData();
                return ret;
            }
            if (_transaction == null)
            {
                var threadTransaction = _orm.Ado.TransactionCurrentThread;
                if (threadTransaction != null) this.WithTransaction(threadTransaction);
            }

            var before = new Aop.TraceBeforeEventArgs("SplitExecuteIdentityAsync", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                if (_transaction != null || _batchAutoTransaction == false)
                {
                    for (var a = 0; a < ss.Length; a++)
                    {
                        _source = ss[a];
                        _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                        if (a < ss.Length - 1) await this.RawExecuteAffrowsAsync(cancellationToken);
                        else ret = await this.RawExecuteIdentityAsync(cancellationToken);
                    }
                }
                else
                {
                    if (_orm.Ado.MasterPool == null) throw new Exception(CoreErrorStrings.MasterPool_IsNull_UseTransaction);
                    using (var conn = await _orm.Ado.MasterPool.GetAsync())
                    {
                        _transaction = conn.Value.BeginTransaction();
                        var transBefore = new Aop.TraceBeforeEventArgs("BeginTransaction", null);
                        _orm.Aop.TraceBeforeHandler?.Invoke(this, transBefore);
                        try
                        {
                            for (var a = 0; a < ss.Length; a++)
                            {
                                _source = ss[a];
                                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                                if (a < ss.Length - 1) await this.RawExecuteAffrowsAsync(cancellationToken);
                                else ret = await this.RawExecuteIdentityAsync(cancellationToken);
                            }
                            _transaction.Commit();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreErrorStrings.Commit, null));
                        }
                        catch (Exception ex)
                        {
                            _transaction.Rollback();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreErrorStrings.RollBack, ex));
                            throw;
                        }
                        _transaction = null;
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.TraceAfterEventArgs(before, null, exception);
                _orm.Aop.TraceAfterHandler?.Invoke(this, after);
            }
            ClearData();
            return ret;
        }

        async protected virtual Task<List<T1>> SplitExecuteInsertedAsync(int valuesLimit, int parameterLimit, CancellationToken cancellationToken = default)
        {
            var ss = SplitSource(valuesLimit, parameterLimit);
            var ret = new List<T1>();
            if (ss.Any() == false)
            {
                ClearData();
                return ret;
            }
            if (ss.Length == 1)
            {
                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                ret = await this.InternalExecuteInsertedAsync(cancellationToken);
                ClearData();
                return ret;
            }
            if (_transaction == null)
            {
                var threadTransaction = _orm.Ado.TransactionCurrentThread;
                if (threadTransaction != null) this.WithTransaction(threadTransaction);
            }

            var before = new Aop.TraceBeforeEventArgs("SplitExecuteInsertedAsync", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                if (_transaction != null || _batchAutoTransaction == false)
                {
                    for (var a = 0; a < ss.Length; a++)
                    {
                        _source = ss[a]; 
                        _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                        ret.AddRange(await this.InternalExecuteInsertedAsync(cancellationToken));
                    }
                }
                else
                {
                    if (_orm.Ado.MasterPool == null) throw new Exception(CoreErrorStrings.MasterPool_IsNull_UseTransaction);
                    using (var conn = await _orm.Ado.MasterPool.GetAsync())
                    {
                        _transaction = conn.Value.BeginTransaction();
                        var transBefore = new Aop.TraceBeforeEventArgs("BeginTransaction", null);
                        _orm.Aop.TraceBeforeHandler?.Invoke(this, transBefore);
                        try
                        {
                            for (var a = 0; a < ss.Length; a++)
                            {
                                _source = ss[a];
                                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                                ret.AddRange(await this.InternalExecuteInsertedAsync(cancellationToken));
                            }
                            _transaction.Commit();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreErrorStrings.Commit, null));
                        }
                        catch (Exception ex)
                        {
                            _transaction.Rollback();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreErrorStrings.RollBack, ex));
                            throw;
                        }
                        _transaction = null;
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.TraceAfterEventArgs(before, null, exception);
                _orm.Aop.TraceAfterHandler?.Invoke(this, after);
            }
            ClearData();
            return ret;
        }

        async protected virtual Task<int> RawExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            var sql = ToSql();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var affrows = 0;
            Exception exception = null;
            try
            {
                affrows = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params, cancellationToken);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, affrows);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return affrows;
        }

        protected abstract Task<long> RawExecuteIdentityAsync(CancellationToken cancellationToken = default);
        protected abstract Task<List<T1>> RawExecuteInsertedAsync(CancellationToken cancellationToken = default);
        async private Task<List<T1>> InternalExecuteInsertedAsync(CancellationToken cancellationToken = default)
        {
            var ret = await RawExecuteInsertedAsync(cancellationToken);
            if (_table.TypeLazySetOrm != null) ret.ForEach(item => _table.TypeLazySetOrm.Invoke(item, new object[] { _orm }));
            return ret;
        }

        public abstract Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default);
        public abstract Task<long> ExecuteIdentityAsync(CancellationToken cancellationToken = default);
        public abstract Task<List<T1>> ExecuteInsertedAsync(CancellationToken cancellationToken = default);
#endif
    }
}
