using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{

    partial class UpdateProvider<T1>
    {
#if net40
#else
        async protected Task<int> SplitExecuteAffrowsAsync(int valuesLimit, int parameterLimit)
        {
            var ss = SplitSource(valuesLimit, parameterLimit);
            var ret = 0;
            if (ss.Length <= 1)
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
        async protected Task<List<T1>> SplitExecuteUpdatedAsync(int valuesLimit, int parameterLimit)
        {
            var ss = SplitSource(valuesLimit, parameterLimit);
            var ret = new List<T1>();
            if (ss.Length <= 1)
            {
                ret = await this.RawExecuteUpdatedAsync();
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
                    ret.AddRange(await this.RawExecuteUpdatedAsync());
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
                            ret.AddRange(await this.RawExecuteUpdatedAsync());
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

        async protected Task<int> RawExecuteAffrowsAsync()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;
            var dbParms = _params.Concat(_paramsSource).ToArray();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, sql, dbParms);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            var affrows = 0;
            Exception exception = null;
            try
            {
                affrows = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, dbParms);
                ValidateVersionAndThrow(affrows);
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
        protected abstract Task<List<T1>> RawExecuteUpdatedAsync();

        public abstract Task<int> ExecuteAffrowsAsync();
        public abstract Task<List<T1>> ExecuteUpdatedAsync();
#endif
    }
}