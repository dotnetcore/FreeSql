using FreeSql.Internal;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Custom
{

    class CustomInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        CustomUtils _utils;
        public CustomInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
            _utils = _commonUtils as CustomUtils;
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(_batchValuesLimit > 0 ? _batchValuesLimit : _utils.Adapter.InsertBatchSplitLimit, _batchParameterLimit > 0 ? _batchParameterLimit : 255);
        public override long ExecuteIdentity() => base.SplitExecuteIdentity(_batchValuesLimit > 0 ? _batchValuesLimit : _utils.Adapter.InsertBatchSplitLimit, _batchParameterLimit > 0 ? _batchParameterLimit : 255);
        public override List<T1> ExecuteInserted() => base.SplitExecuteInserted(_batchValuesLimit > 0 ? _batchValuesLimit : _utils.Adapter.InsertBatchSplitLimit, _batchParameterLimit > 0 ? _batchParameterLimit : 255);

        protected override long RawExecuteIdentity()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            Object<DbConnection> poolConn = null;
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, string.Concat(sql, $"; {_utils.Adapter.InsertAfterGetIdentitySql}"), _params);
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
                ret = long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(conn, _transaction, CommandType.Text, $" {_utils.Adapter.InsertAfterGetIdentitySql}", _commandTimeout)), out var trylng) ? trylng : 0;
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

        protected override List<T1> RawExecuteInserted() => throw new NotImplementedException("FreeSql.Provider.Custom 未实现该功能");

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => base.SplitExecuteAffrowsAsync(_batchValuesLimit > 0 ? _batchValuesLimit : _utils.Adapter.InsertBatchSplitLimit, _batchParameterLimit > 0 ? _batchParameterLimit : 255, cancellationToken);
        public override Task<long> ExecuteIdentityAsync(CancellationToken cancellationToken = default) => base.SplitExecuteIdentityAsync(_batchValuesLimit > 0 ? _batchValuesLimit : _utils.Adapter.InsertBatchSplitLimit, _batchParameterLimit > 0 ? _batchParameterLimit : 255, cancellationToken);
        public override Task<List<T1>> ExecuteInsertedAsync(CancellationToken cancellationToken = default) => base.SplitExecuteInsertedAsync(_batchValuesLimit > 0 ? _batchValuesLimit : _utils.Adapter.InsertBatchSplitLimit, _batchParameterLimit > 0 ? _batchParameterLimit : 255, cancellationToken);

        async protected override Task<long> RawExecuteIdentityAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            Object<DbConnection> poolConn = null;
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, string.Concat(sql, $"; {_utils.Adapter.InsertAfterGetIdentitySql}"), _params);
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
                ret = long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(conn, _transaction, CommandType.Text, $" {_utils.Adapter.InsertAfterGetIdentitySql}", _commandTimeout, null, cancellationToken)), out var trylng) ? trylng : 0;
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
        protected override Task<List<T1>> RawExecuteInsertedAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException("FreeSql.Provider.Custom 未实现该功能");
#endif
    }
}