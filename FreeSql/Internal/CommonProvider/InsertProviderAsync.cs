using FreeSql.Internal.Model;
using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{

    partial class InsertProvider<T1>
    {
#if net40
#else
        async protected Task<int> SplitExecuteAffrowsAsync(int valuesLimit, int parameterLimit)
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
                ret = await this.RawExecuteAffrowsAsync();
                ClearData();
                return ret;
            }
            if (_transaction == null)
                this.WithTransaction(_orm.Ado.TransactionCurrentThread);

            if (_transaction != null)
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    ret += await this.RawExecuteAffrowsAsync();
                }
            }
            else
            {
                using (var conn = await _orm.Ado.MasterPool.GetAsync())
                {
                    _transaction = conn.Value.BeginTransaction();
                    try
                    {
                        for (var a = 0; a < ss.Length; a++)
                        {
                            _source = ss[a];
                            ret += await this.RawExecuteAffrowsAsync();
                        }
                        _transaction.Commit();
                    }
                    catch
                    {
                        _transaction.Rollback();
                        throw;
                    }
                    _transaction = null;
                }
            }
            ClearData();
            return ret;
        }

        async protected Task<long> SplitExecuteIdentityAsync(int valuesLimit, int parameterLimit)
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
                ret = await this.RawExecuteIdentityAsync();
                ClearData();
                return ret;
            }
            if (_transaction == null)
                this.WithTransaction(_orm.Ado.TransactionCurrentThread);

            if (_transaction != null)
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    if (a < ss.Length - 1) await this.RawExecuteAffrowsAsync();
                    else ret = await this.RawExecuteIdentityAsync();
                }
            }
            else
            {
                using (var conn = await _orm.Ado.MasterPool.GetAsync())
                {
                    _transaction = conn.Value.BeginTransaction();
                    try
                    {
                        for (var a = 0; a < ss.Length; a++)
                        {
                            _source = ss[a];
                            if (a < ss.Length - 1) await this.RawExecuteAffrowsAsync();
                            else ret = await this.RawExecuteIdentityAsync();
                        }
                        _transaction.Commit();
                    }
                    catch
                    {
                        _transaction.Rollback();
                        throw;
                    }
                    _transaction = null;
                }
            }
            ClearData();
            return ret;
        }
        
        async protected Task<List<T1>> SplitExecuteInsertedAsync(int valuesLimit, int parameterLimit)
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
                ret = await this.RawExecuteInsertedAsync();
                ClearData();
                return ret;
            }
            if (_transaction == null)
                this.WithTransaction(_orm.Ado.TransactionCurrentThread);

            if (_transaction != null)
            {
                for (var a = 0; a < ss.Length; a++)
                {
                    _source = ss[a];
                    ret.AddRange(await this.RawExecuteInsertedAsync());
                }
            }
            else
            {
                using (var conn = await _orm.Ado.MasterPool.GetAsync())
                {
                    _transaction = conn.Value.BeginTransaction();
                    try
                    {
                        for (var a = 0; a < ss.Length; a++)
                        {
                            _source = ss[a];
                            ret.AddRange(await this.RawExecuteInsertedAsync());
                        }
                        _transaction.Commit();
                    }
                    catch
                    {
                        _transaction.Rollback();
                        throw;
                    }
                    _transaction = null;
                }
            }
            ClearData();
            return ret;
        }

        async protected virtual Task<int> RawExecuteAffrowsAsync()
        {
            var sql = ToSql();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            var affrows = 0;
            Exception exception = null;
            try
            {
                affrows = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _params);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, affrows);
                _orm.Aop.CurdAfter?.Invoke(this, after);
            }
            return affrows;
        }

        protected abstract Task<long> RawExecuteIdentityAsync();
        protected abstract Task<List<T1>> RawExecuteInsertedAsync();

        public abstract Task<int> ExecuteAffrowsAsync();
        public abstract Task<long> ExecuteIdentityAsync();
        public abstract Task<List<T1>> ExecuteInsertedAsync();
#endif
    }
}

