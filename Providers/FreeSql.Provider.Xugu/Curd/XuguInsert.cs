using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XuguClient;

namespace FreeSql.Xugu.Curd
{

    class XuguInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        public XuguInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        internal IFreeSql InternalOrm => _orm;
        internal TableInfo InternalTable => _table;
        internal DbParameter[] InternalParams => _params;
        internal DbConnection InternalConnection => _connection;
        internal DbTransaction InternalTransaction => _transaction;
        internal CommonUtils InternalCommonUtils => _commonUtils;
        internal CommonExpression InternalCommonExpression => _commonExpression;
        internal List<T1> InternalSource => _source;
        internal Dictionary<string, bool> InternalIgnore => _ignore;
        internal void InternalClearData() => ClearData();

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(_batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
        public override long ExecuteIdentity() => base.SplitExecuteIdentity(_batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
        public override List<T1> ExecuteInserted() => base.SplitExecuteInserted(_batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);

        protected override long RawExecuteIdentity()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            long ret = 0;
            Aop.CurdBeforeEventArgs before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            Exception exception = null;


            try
            {
                var identCols = _table.Columns.Where(a => a.Value.Attribute.IsIdentity == true);
                if (identCols.Any() == false)
                {
                    //var ex = new Exception("对没有自增主键的表执行插入操作时不能要求返回自增主键");
                    //exception = ex; 
                    _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params);
                    return 0;

                }
                _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, cmd =>
                {
                    var rowid = (cmd as XGCommand).get_insert_rowid();

                    //using (var command = cmd.Connection.CreateCommand()) {
                    //command.CommandType = CommandType.Text;
                    var sqlIdentity = $"SELECT {_commonUtils.QuoteSqlName(identCols.First().Value.Attribute.Name)} FROM {_table.DbName} WHERE \"ROWID\"='{rowid}'";


                    //command.CommandText = sql;
                    if (!long.TryParse(_orm.Ado.ExecuteScalar(CommandType.Text, sqlIdentity, _params).ToString(), out ret))
                    {

                    };
                    //    command.Dispose();
                    //}

                }, _params);
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
            return ret;
        }

        protected override List<T1> RawExecuteInserted()
        {





            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return null;

            Aop.CurdBeforeEventArgs before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            Exception exception = null;

            var ret = new List<T1>();
            try
            {
                var sbColumn = new StringBuilder();
                var colidx = 0;
                foreach (var col in _table.Columns.Values)
                {
                    if (colidx > 0) sbColumn.Append(", ");
                    sbColumn.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                    ++colidx;
                }

                _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, cmd =>
                {
                    var rowid = (cmd as XGCommand).get_insert_rowid();

                    var sqlIdentity = $"SELECT {sbColumn} FROM {_table.DbName} WHERE \"ROWID\"='{rowid}'";

                    ret = _orm.Ado.Query<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction, CommandType.Text, sqlIdentity, _commandTimeout, _params);

                }, _params);
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
            return ret;

        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => base.SplitExecuteAffrowsAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);
        public override Task<long> ExecuteIdentityAsync(CancellationToken cancellationToken = default) => base.SplitExecuteIdentityAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);
        public override Task<List<T1>> ExecuteInsertedAsync(CancellationToken cancellationToken = default) => base.SplitExecuteInsertedAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);

        async protected override Task<long> RawExecuteIdentityAsync(CancellationToken cancellationToken = default)
        {


            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                var identCols = _table.Columns.Where(a => a.Value.Attribute.IsIdentity == true);
                if (identCols.Any() == false)
                {
                    //var ex = new Exception("对没有自增主键的表执行插入操作时不能要求返回自增主键");
                    //exception = ex; 
                    _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params);
                    return 0;

                }
                await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, cmd =>
                {
                    var rowid = (cmd as XGCommand).get_insert_rowid();
                    var sqlIdentity = $"SELECT {_commonUtils.QuoteSqlName(identCols.First().Value.Attribute.Name)} FROM {_table.DbName} WHERE \"ROWID\"='{rowid}'";
                    if (!long.TryParse(_orm.Ado.ExecuteScalar(CommandType.Text, sqlIdentity, _params).ToString(), out ret))
                    {

                    };
                    return Task.CompletedTask;
                }, _params, cancellationToken);
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
            return ret;

        }
        async protected override Task<List<T1>> RawExecuteInsertedAsync(CancellationToken cancellationToken = default)
        {





            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return null;

            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                var sbColumn = new StringBuilder();
                var colidx = 0;
                foreach (var col in _table.Columns.Values)
                {
                    if (colidx > 0) sbColumn.Append(", ");
                    sbColumn.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                    ++colidx;
                }
                await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, cmd =>
                {
                    var rowid = (cmd as XGCommand).get_insert_rowid();

                    var sqlIdentity = $"SELECT {sbColumn} FROM {_table.DbName} WHERE \"ROWID\"='{rowid}'";

                    ret = _orm.Ado.Query<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction, CommandType.Text, sqlIdentity, _commandTimeout, _params);
                    return Task.CompletedTask;
                }, _params, cancellationToken);
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
            return ret;
        }
#endif
    }
}
