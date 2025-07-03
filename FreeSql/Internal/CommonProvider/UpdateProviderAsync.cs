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
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{

    partial class UpdateProvider<T1>
    {
#if net40
#else
		async protected virtual Task SplitExecuteAsync(int valuesLimit, int parameterLimit, string traceName, Func<Task> executeAsync, CancellationToken cancellationToken = default)
        {
			var ss = SplitSource(valuesLimit, parameterLimit);
			if (ss.Length <= 1)
			{
				if (_source?.Any() == true) _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                await executeAsync();
				ClearData();
				return;
			}
			if (_transaction == null)
			{
				var threadTransaction = _orm.Ado.TransactionCurrentThread;
				if (threadTransaction != null) this.WithTransaction(threadTransaction);
			}

			var before = new Aop.TraceBeforeEventArgs(traceName, null);
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
						await executeAsync();
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
								await executeAsync();
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
		}

		async protected Task<int> SplitExecuteAffrowsAsync(int valuesLimit, int parameterLimit, CancellationToken cancellationToken = default)
        {
            var ret = 0;
            await SplitExecuteAsync(valuesLimit, parameterLimit, "SplitExecuteAffrowsAsync", async () =>
                ret += await this.RawExecuteAffrowsAsync(cancellationToken)
            );
            return ret;
        }
		async protected Task<List<TReturn>> SplitExecuteUpdatedAsync<TReturn>(int valuesLimit, int parameterLimit, IEnumerable<ColumnInfo> columns, CancellationToken cancellationToken = default)
		{
			var ret = new List<TReturn>();
			await SplitExecuteAsync(valuesLimit, parameterLimit, "SplitExecuteUpdatedAsync", async () =>
				ret.AddRange(await this.RawExecuteUpdatedAsync<TReturn>(columns ?? _table.ColumnsByPosition))
			);
			return ret;
		}

		async protected Task<int> RawExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            var affrows = 0;
            DbParameter[] dbParms = null;
            await ToSqlFetchAsync(async sb =>
            {
                if (dbParms == null) dbParms = _params.Concat(_paramsSource).ToArray();
                var sql = sb.ToString();
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, sql, dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    var affrowstmp = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
                    ValidateVersionAndThrow(affrowstmp, sql, dbParms);
                    affrows += affrowstmp;
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
            });
            return affrows;
		}
		protected abstract Task<List<TReturn>> RawExecuteUpdatedAsync<TReturn>(IEnumerable<ColumnInfo> columns, CancellationToken cancellationToken = default);

		public abstract Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default);
		protected abstract Task<List<TReturn>> ExecuteUpdatedAsync<TReturn>(IEnumerable<ColumnInfo> columns, CancellationToken cancellationToken = default);

		async public Task<List<T1>> ExecuteUpdatedAsync(CancellationToken cancellationToken = default)
		{
			var ret = await ExecuteUpdatedAsync<T1>(_table.ColumnsByPosition, cancellationToken);
            if (_table.TypeLazySetOrm != null) ret.ForEach(item => _table.TypeLazySetOrm.Invoke(item, new object[] { _orm }));
            return ret;
        }
		async public Task<List<TReturn>> ExecuteUpdatedAsync<TReturn>(Expression<Func<T1, TReturn>> returnColumns, CancellationToken cancellationToken = default)
		{
			var cols = _commonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, null, returnColumns?.Body, false, null)
				.Distinct().Select(a => _table.ColumnsByCs.TryGetValue(a, out var c) ? c : null).Where(a => a != null).ToArray();
			var ret = await ExecuteUpdatedAsync<TReturn>(cols, cancellationToken);
            if (_table.TypeLazySetOrm != null) ret.ForEach(item => _table.TypeLazySetOrm.Invoke(item, new object[] { _orm }));
            return ret;
        }
#endif
	}
}
