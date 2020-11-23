using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{
    partial class DeleteProvider<T1>
    {
#if net40
#else
        async public Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var affrows = 0;
            Exception exception = null;
            try
            {
                affrows = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
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
            this.ClearData();
            return affrows;
        }
        public abstract Task<List<T1>> ExecuteDeletedAsync(CancellationToken cancellationToken = default);
#endif
    }
}
