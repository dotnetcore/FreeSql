using FreeSql.Internal;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.MsAccess.Curd
{

    class MsAccessInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        public MsAccessInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
            _batchAutoTransaction = false;
        }

        //蛋疼的 access 插入只能一条一条执行，不支持 values(..),(..) 也不支持 select .. UNION ALL select ..
        public override int ExecuteAffrows() => base.SplitExecuteAffrows(1, 1000);
        public override long ExecuteIdentity() => base.SplitExecuteIdentity(1, 1000);
        public override List<T1> ExecuteInserted() => base.SplitExecuteInserted(1, 1000);

        public override IInsert<T1> BatchOptions(int valuesLimit, int parameterLimit, bool autoTransaction = true) => 
            throw new NotImplementedException("蛋疼的 access 插入只能一条一条执行，不支持 values(..),(..) 也不支持 select .. UNION ALL select ..");

        protected override int RawExecuteAffrows()
        {
            var sql = this.ToSql();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var affrows = 0;
            Exception exception = null;
            try
            {
                affrows = _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, affrows);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return affrows;
        }
        protected override long RawExecuteIdentity()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, string.Concat(sql, "; SELECT @@identity;"), _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            long ret = 0;
            Exception exception = null;
            var isUseConnection = _connection != null;
            try
            {
                if (isUseConnection == false)
                {
                    using (var conn = _orm.Ado.MasterPool.Get())
                    {
                        _connection = conn.Value;
                        _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params);
                        long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(_connection, _transaction, CommandType.Text, "SELECT @@identity", _commandTimeout, _params)), out ret);
                    }
                }
                else
                {
                    _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params);
                    long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(_connection, _transaction, CommandType.Text, "SELECT @@identity", _commandTimeout, _params)), out ret);
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                if (isUseConnection == false) _connection = null;
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return ret;
        }
        protected override List<T1> RawExecuteInserted()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var ret = _source.ToList();
            this.RawExecuteAffrows();
            return ret;
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => base.SplitExecuteAffrowsAsync(1, 1000, cancellationToken);
        public override Task<long> ExecuteIdentityAsync(CancellationToken cancellationToken = default) => base.SplitExecuteIdentityAsync(1, 1000, cancellationToken);
        public override Task<List<T1>> ExecuteInsertedAsync(CancellationToken cancellationToken = default) => base.SplitExecuteInsertedAsync(1, 1000, cancellationToken);

        async protected override Task<int> RawExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
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
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, affrows);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return affrows;
        }
        async protected override Task<long> RawExecuteIdentityAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, string.Concat(sql, "; SELECT @@identity;"), _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            long ret = 0;
            Exception exception = null;
            var isUseConnection = _connection != null;
            try
            {
                if (isUseConnection == false)
                {
                    using (var conn = await _orm.Ado.MasterPool.GetAsync())
                    {
                        _connection = conn.Value;
                        await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params, cancellationToken);
                        long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(_connection, _transaction, CommandType.Text, "SELECT @@identity", _commandTimeout, _params, cancellationToken)), out ret);
                    }
                }
                else
                {
                    await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params, cancellationToken);
                    long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(_connection, _transaction, CommandType.Text, "SELECT @@identity", _commandTimeout, _params, cancellationToken)), out ret);
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                if (isUseConnection == false) _connection = null;
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return ret;
        }
        async protected override Task<List<T1>> RawExecuteInsertedAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var ret = _source.ToList();
            await this.RawExecuteAffrowsAsync(cancellationToken);
            return ret;
        }
#endif
    }
}