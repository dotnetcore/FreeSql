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

namespace FreeSql.GBase.Curd
{

    class GBaseInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        public GBaseInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(_batchValuesLimit > 0 ? _batchValuesLimit : 200, _batchParameterLimit > 0 ? _batchParameterLimit : 999);
        public override long ExecuteIdentity() => base.SplitExecuteIdentity(_batchValuesLimit > 0 ? _batchValuesLimit : 200, _batchParameterLimit > 0 ? _batchParameterLimit : 999);
        public override List<T1> ExecuteInserted() => base.SplitExecuteInserted(1, 999);

        public override string ToSql()
        {
            if (_source?.Count <= 1) return base.ToSqlValuesOrSelectUnionAll();
            var sql = base.ToSqlValuesOrSelectUnionAllExtension102(false, null, (rowd, idx, sb) => sb.Append(" FROM dual"));
            var validx = sql.IndexOf(") SELECT ");
            if (validx == -1) throw new ArgumentException("找不到 SELECT");
            return new StringBuilder()
                .Insert(0, sql.Substring(0, validx + 1))
                .Append("\r\nSELECT * FROM (\r\n")
                .Append(sql.Substring(validx + 1))
                .Append("\r\n) ftbtmp")
                .ToString();
        }

        protected override long RawExecuteIdentity()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            var identityType = _table.Primarys.Where(a => a.Attribute.IsIdentity).FirstOrDefault()?.CsType.NullableTypeOrThis();
            var identitySql = "";
            if (identityType != null)
            {
                if (identityType == typeof(int) || identityType == typeof(uint)) identitySql = "SELECT dbinfo('sqlca.sqlerrd1') FROM dual";
                else if (identityType == typeof(long) || identityType == typeof(ulong)) identitySql = "SELECT dbinfo('serial8') FROM dual";
            }
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, string.Concat(sql, $"; {identitySql};"), _params);
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
                        if (string.IsNullOrWhiteSpace(identitySql) == false)
                            long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(_connection, _transaction, CommandType.Text, identitySql, _commandTimeout, _params)), out ret);
                    }
                }
                else
                {
                    _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params);
                    if (string.IsNullOrWhiteSpace(identitySql) == false)
                        long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(_connection, _transaction, CommandType.Text, identitySql, _commandTimeout, _params)), out ret);
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
            throw new NotImplementedException();
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => base.SplitExecuteAffrowsAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 200, _batchParameterLimit > 0 ? _batchParameterLimit : 999, cancellationToken);
        public override Task<long> ExecuteIdentityAsync(CancellationToken cancellationToken = default) => base.SplitExecuteIdentityAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 200, _batchParameterLimit > 0 ? _batchParameterLimit : 999, cancellationToken);
        public override Task<List<T1>> ExecuteInsertedAsync(CancellationToken cancellationToken = default) => base.SplitExecuteInsertedAsync(1, 1000, cancellationToken);

        async protected override Task<long> RawExecuteIdentityAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            var identityType = _table.Primarys.Where(a => a.Attribute.IsIdentity).FirstOrDefault()?.CsType.NullableTypeOrThis();
            var identitySql = "";
            if (identityType != null)
            {
                if (identityType == typeof(int) || identityType == typeof(uint)) identitySql = "SELECT dbinfo('sqlca.sqlerrd1') FROM dual";
                else if (identityType == typeof(long) || identityType == typeof(ulong)) identitySql = "SELECT dbinfo('serial8') FROM dual";
            }
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, string.Concat(sql, $"; {identitySql};"), _params);
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
                        await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params);
                        if (string.IsNullOrWhiteSpace(identitySql) == false)
                            long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(_connection, _transaction, CommandType.Text, identitySql, _commandTimeout, _params)), out ret);
                    }
                }
                else
                {
                    await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params);
                    if (string.IsNullOrWhiteSpace(identitySql) == false)
                        long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(_connection, _transaction, CommandType.Text, identitySql, _commandTimeout, _params)), out ret);
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
        protected override Task<List<T1>> RawExecuteInsertedAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
