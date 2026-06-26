using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Odbc.MySql
{

    class OdbcMySqlInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        public OdbcMySqlInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        internal bool InternalIsIgnoreInto = false;
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


        public override string ToSql()
        {
            if (InternalIsIgnoreInto == false) return base.ToSqlValuesOrSelectUnionAll();
            var sql = base.ToSqlValuesOrSelectUnionAll();
            return $"INSERT IGNORE INTO {sql.Substring(12)}";
        }

        protected override long RawExecuteIdentity()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            Object<DbConnection> poolConn = null;
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, string.Concat(sql, "; SELECT LAST_INSERT_ID();"), _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                var conn = _connection;
                if (_transaction != null) conn = _transaction.Connection;
                if (conn == null)
                {
                    poolConn = _orm.Ado.MasterPool.Get();
                    conn = poolConn.Value;
                }
                _orm.Ado.ExecuteNonQuery(conn, _transaction, CommandType.Text, sql, _commandTimeout, _params);
                ret = long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(conn, _transaction, CommandType.Text, " SELECT LAST_INSERT_ID()", _commandTimeout)), out var trylng) ? trylng : 0;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                if (poolConn != null)
                    _orm.Ado.MasterPool.Return(poolConn);

                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return ret;
        }
        protected override List<T1> RawExecuteInserted()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                var affrows = _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params);

                if (affrows > 0)
                {
                    var pk = _table.Primarys.FirstOrDefault();
                    var tableName = _commonUtils.QuoteSqlName(TableRuleInvoke());

                    var sbSelect = new StringBuilder();
                    sbSelect.Append("SELECT ");
                    var colidx = 0;
                    foreach (var col in _table.Columns.Values)
                    {
                        if (colidx > 0) sbSelect.Append(", ");
                        sbSelect.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name)))
                                .Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                        ++colidx;
                    }
                    sbSelect.Append(" FROM ").Append(tableName);

                    if (pk != null && pk.Attribute.IsIdentity && _insertIdentity == false)
                    {
                        var lastIdObj = _orm.Ado.ExecuteScalar(_connection, _transaction, CommandType.Text,
                            "SELECT LAST_INSERT_ID();", _commandTimeout, null);
                        if (lastIdObj != null && lastIdObj != DBNull.Value)
                        {
                            var lastId = Convert.ToInt64(lastIdObj);
                            sbSelect.Append(" WHERE ").Append(_commonUtils.QuoteSqlName(pk.Attribute.Name))
                                    .Append(" >= ").Append(lastId)
                                    .Append(" ORDER BY ").Append(_commonUtils.QuoteSqlName(pk.Attribute.Name))
                                    .Append(" LIMIT ").Append(affrows);
                        }
                        else
                        {
                            return ret;
                        }
                    }
                    else if (pk != null && _source.Any())
                    {
                        var pkValues = _source.Select(a => pk.GetDbValue(a)).Distinct().ToArray();
                        if (pkValues.Length == 0) return ret;

                        sbSelect.Append(" WHERE ").Append(_commonUtils.QuoteSqlName(pk.Attribute.Name))
                                .Append(" IN (");
                        for (var i = 0; i < pkValues.Length; i++)
                        {
                            if (i > 0) sbSelect.Append(", ");
                            sbSelect.Append(_commonUtils.FormatSql("{0}", pkValues[i]));
                        }
                        sbSelect.Append(")");
                    }
                    else
                    {
                        return ret;
                    }

                    var selectSql = sbSelect.ToString();
                    var before2 = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, selectSql, null);
                    _orm.Aop.CurdBeforeHandler?.Invoke(this, before2);

                    ret = _orm.Ado.Query<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction,
                        CommandType.Text, selectSql, _commandTimeout, null);
                }
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

            Object<DbConnection> poolConn = null;
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, string.Concat(sql, "; SELECT LAST_INSERT_ID();"), _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                var conn = _connection;
                if (_transaction != null) conn = _transaction.Connection;
                if (conn == null)
                {
                    poolConn = _orm.Ado.MasterPool.Get();
                    conn = poolConn.Value;
                }
                await _orm.Ado.ExecuteNonQueryAsync(conn, _transaction, CommandType.Text, sql, _commandTimeout, _params, cancellationToken);
                ret = long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(conn, _transaction, CommandType.Text, " SELECT LAST_INSERT_ID()", _commandTimeout, null, cancellationToken)), out var trylng) ? trylng : 0;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                if (poolConn != null)
                    _orm.Ado.MasterPool.Return(poolConn);

                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return ret;
        }
        async protected override Task<List<T1>> RawExecuteInsertedAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                var affrows = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params, cancellationToken);

                if (affrows > 0)
                {
                    var pk = _table.Primarys.FirstOrDefault();
                    var tableName = _commonUtils.QuoteSqlName(TableRuleInvoke());

                    var sbSelect = new StringBuilder();
                    sbSelect.Append("SELECT ");
                    var colidx = 0;
                    foreach (var col in _table.Columns.Values)
                    {
                        if (colidx > 0) sbSelect.Append(", ");
                        sbSelect.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name)))
                                .Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                        ++colidx;
                    }
                    sbSelect.Append(" FROM ").Append(tableName);

                    if (pk != null && pk.Attribute.IsIdentity && _insertIdentity == false)
                    {
                        var lastIdObj = await _orm.Ado.ExecuteScalarAsync(_connection, _transaction, CommandType.Text,
                            "SELECT LAST_INSERT_ID();", _commandTimeout, null, cancellationToken);
                        if (lastIdObj != null && lastIdObj != DBNull.Value)
                        {
                            var lastId = Convert.ToInt64(lastIdObj);
                            sbSelect.Append(" WHERE ").Append(_commonUtils.QuoteSqlName(pk.Attribute.Name))
                                    .Append(" >= ").Append(lastId)
                                    .Append(" ORDER BY ").Append(_commonUtils.QuoteSqlName(pk.Attribute.Name))
                                    .Append(" LIMIT ").Append(affrows);
                        }
                        else
                        {
                            return ret;
                        }
                    }
                    else if (pk != null && _source.Any())
                    {
                        var pkValues = _source.Select(a => pk.GetDbValue(a)).Distinct().ToArray();
                        if (pkValues.Length == 0) return ret;

                        sbSelect.Append(" WHERE ").Append(_commonUtils.QuoteSqlName(pk.Attribute.Name))
                                .Append(" IN (");
                        for (var i = 0; i < pkValues.Length; i++)
                        {
                            if (i > 0) sbSelect.Append(", ");
                            sbSelect.Append(_commonUtils.FormatSql("{0}", pkValues[i]));
                        }
                        sbSelect.Append(")");
                    }
                    else
                    {
                        return ret;
                    }

                    var selectSql = sbSelect.ToString();
                    var before2 = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, selectSql, null);
                    _orm.Aop.CurdBeforeHandler?.Invoke(this, before2);

                    ret = await _orm.Ado.QueryAsync<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction,
                        CommandType.Text, selectSql, _commandTimeout, null, cancellationToken);
                }
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
