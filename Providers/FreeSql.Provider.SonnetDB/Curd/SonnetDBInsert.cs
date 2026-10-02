using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.SonnetDB.Curd
{
    class SonnetDBInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        public SonnetDBInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
            if (SonnetDBModel.IsTable(_table) && _table.VersionColumn != null)
                _ignore[_table.VersionColumn.Attribute.Name] = true;
        }

        public override int ExecuteAffrows()
        {
            EnsureMeasurementTransactionIsNotUsed();
            if (SonnetDBModel.IsMeasurement(_table)) _batchAutoTransaction = false;
            return base.SplitExecuteAffrows(_batchValuesLimit > 0 ? _batchValuesLimit : 5000,
                _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
        }

        public override long ExecuteIdentity()
        {
            EnsureMeasurementTransactionIsNotUsed();
            if (SonnetDBModel.IsMeasurement(_table)) _batchAutoTransaction = false;
            return base.SplitExecuteIdentity(_batchValuesLimit > 0 ? _batchValuesLimit : 5000,
                _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
        }

        public override List<T1> ExecuteInserted()
        {
            SonnetDBModel.EnsureTable(_table, "INSERT ... RETURNING");
            EnsureMeasurementTransactionIsNotUsed();
            return base.SplitExecuteInserted(_batchValuesLimit > 0 ? _batchValuesLimit : 5000,
                _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
        }

        public override string ToSql()
        {
            // InsertProvider 不会自动排除 IsVersion 列。
            // 关系表的 ROWVERSION 从行插入时即由 SonnetDB 维护，
            // 因此即使调用方通过 InsertColumns 显式选择，也不得写入。
            if (SonnetDBModel.IsTable(_table) && _table.VersionColumn != null)
            {
                return ToSqlValuesOrSelectUnionAll(true,
                    new List<string> { _table.VersionColumn.CsName });
            }
            return base.ToSql();
        }

        protected override long RawExecuteIdentity()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            var identity = SonnetDBModel.IsTable(_table)
                ? _table.ColumnsByPosition.FirstOrDefault(column => column.Attribute.IsIdentity)
                : null;
            if (identity != null)
                sql = string.Concat(sql, " RETURNING ", _commonUtils.QuoteSqlName(identity.Attribute.Name));

            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            long ret = 0;
            object executeResult = null;
            Exception exception = null;
            try
            {
                if (identity == null)
                {
                    ret = _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text,
                        sql, _commandTimeout, _params);
                    executeResult = ret;
                }
                else
                {
                    long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(_connection, _transaction, CommandType.Text,
                        sql, _commandTimeout, _params)), out ret);
                    executeResult = ret;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, executeResult);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return identity == null ? 0 : ret;
        }

        protected override List<T1> RawExecuteInserted()
        {
            SonnetDBModel.EnsureTable(_table, "INSERT ... RETURNING");
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();
            sql = string.Concat(sql, " RETURNING *");
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            List<T1> ret = null;
            Exception exception = null;
            try
            {
                ret = _orm.Ado.Query<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction,
                    CommandType.Text, sql, _commandTimeout, _params);
                return ret;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            EnsureMeasurementTransactionIsNotUsed();
            if (SonnetDBModel.IsMeasurement(_table)) _batchAutoTransaction = false;
            return base.SplitExecuteAffrowsAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 5000,
                _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);
        }

        public override Task<long> ExecuteIdentityAsync(CancellationToken cancellationToken = default)
        {
            EnsureMeasurementTransactionIsNotUsed();
            if (SonnetDBModel.IsMeasurement(_table)) _batchAutoTransaction = false;
            return base.SplitExecuteIdentityAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 5000,
                _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);
        }

        public override Task<List<T1>> ExecuteInsertedAsync(CancellationToken cancellationToken = default)
        {
            SonnetDBModel.EnsureTable(_table, "INSERT ... RETURNING");
            EnsureMeasurementTransactionIsNotUsed();
            return base.SplitExecuteInsertedAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 5000,
                _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);
        }

        async protected override Task<long> RawExecuteIdentityAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            var identity = SonnetDBModel.IsTable(_table)
                ? _table.ColumnsByPosition.FirstOrDefault(column => column.Attribute.IsIdentity)
                : null;
            if (identity != null)
                sql = string.Concat(sql, " RETURNING ", _commonUtils.QuoteSqlName(identity.Attribute.Name));

            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            long ret = 0;
            object executeResult = null;
            Exception exception = null;
            try
            {
                if (identity == null)
                {
                    ret = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction,
                        CommandType.Text, sql, _commandTimeout, _params, cancellationToken);
                    executeResult = ret;
                }
                else
                {
                    long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(_connection, _transaction,
                        CommandType.Text, sql, _commandTimeout, _params, cancellationToken)), out ret);
                    executeResult = ret;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, executeResult);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return identity == null ? 0 : ret;
        }

        protected override async Task<List<T1>> RawExecuteInsertedAsync(CancellationToken cancellationToken = default)
        {
            SonnetDBModel.EnsureTable(_table, "INSERT ... RETURNING");
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();
            sql = string.Concat(sql, " RETURNING *");
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            List<T1> ret = null;
            try
            {
                ret = await _orm.Ado.QueryAsync<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction,
                    CommandType.Text, sql, _commandTimeout, _params, cancellationToken);
                return ret;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
        }
#endif

        void EnsureMeasurementTransactionIsNotUsed()
        {
            if (!SonnetDBModel.IsMeasurement(_table)) return;
            if (_transaction != null || _orm.Ado.TransactionCurrentThread != null)
                throw new NotSupportedException(
                    "SonnetDB 时序测量写入不能在事务中执行；请使用 BatchOptions(..., autoTransaction: false)，或改用关系表。");
        }
    }
}
