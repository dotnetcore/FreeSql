using FreeSql.Internal;
using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Odbc.Default
{

    class OdbcInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        OdbcUtils _utils;
        public OdbcInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
            _utils = _commonUtils as OdbcUtils;
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(_utils.Adapter.InsertBatchSplitLimit, 255);
        public override long ExecuteIdentity() => base.SplitExecuteIdentity(_utils.Adapter.InsertBatchSplitLimit, 255);
        public override List<T1> ExecuteInserted() => base.SplitExecuteInserted(_utils.Adapter.InsertBatchSplitLimit, 255);

        protected override long RawExecuteIdentity()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            Object<DbConnection> poolConn = null;
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, string.Concat(sql, $"; {_utils.Adapter.InsertAfterGetIdentitySql}"), _params);
            _orm.Aop.CurdBefore?.Invoke(this, before);
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
                _orm.Ado.ExecuteNonQuery(conn, _transaction, CommandType.Text, sql, _params);
                ret = long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(conn, _transaction, CommandType.Text, $" {_utils.Adapter.InsertAfterGetIdentitySql}")), out var trylng) ? trylng : 0;
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
                _orm.Aop.CurdAfter?.Invoke(this, after);
            }
            return ret;
        }

        protected override List<T1> RawExecuteInserted() => throw new NotImplementedException("FreeSql.Odbc.Default 未实现该功能");

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync() => base.SplitExecuteAffrowsAsync(_utils.Adapter.InsertBatchSplitLimit, 255);
        public override Task<long> ExecuteIdentityAsync() => base.SplitExecuteIdentityAsync(_utils.Adapter.InsertBatchSplitLimit, 255);
        public override Task<List<T1>> ExecuteInsertedAsync() => base.SplitExecuteInsertedAsync(_utils.Adapter.InsertBatchSplitLimit, 255);
        
        async protected override Task<long> RawExecuteIdentityAsync()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            Object<DbConnection> poolConn = null;
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, string.Concat(sql, $"; {_utils.Adapter.InsertAfterGetIdentitySql}"), _params);
            _orm.Aop.CurdBefore?.Invoke(this, before);
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
                await _orm.Ado.ExecuteNonQueryAsync(conn, _transaction, CommandType.Text, sql, _params);
                ret = long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(conn, _transaction, CommandType.Text, $" {_utils.Adapter.InsertAfterGetIdentitySql}")), out var trylng) ? trylng : 0;
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
                _orm.Aop.CurdAfter?.Invoke(this, after);
            }
            return ret;
        }
        protected override Task<List<T1>> RawExecuteInsertedAsync() => throw new NotImplementedException("FreeSql.Odbc.Default 未实现该功能");
#endif
    }
}