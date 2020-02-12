using FreeSql.Internal;
using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Odbc.SqlServer
{

    class OdbcSqlServerInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        public OdbcSqlServerInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(_batchValuesLimit > 0 ? _batchValuesLimit : 1000, _batchParameterLimit > 0 ? _batchParameterLimit : 2100);
        public override long ExecuteIdentity() => base.SplitExecuteIdentity(_batchValuesLimit > 0 ? _batchValuesLimit : 1000, _batchParameterLimit > 0 ? _batchParameterLimit : 2100);
        public override List<T1> ExecuteInserted() => base.SplitExecuteInserted(_batchValuesLimit > 0 ? _batchValuesLimit : 1000, _batchParameterLimit > 0 ? _batchParameterLimit : 2100);

        protected override int RawExecuteAffrows()
        {
            var versionGreaterThan10 = (_commonUtils as OdbcSqlServerUtils).ServerVersion > 10;
            var sql = versionGreaterThan10 ? this.ToSql() : this.ToSqlValuesOrSelectUnionAll(false);
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            var affrows = 0;
            Exception exception = null;
            try
            {
                affrows = _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _params);
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
        protected override long RawExecuteIdentity()
        {
            var versionGreaterThan10 = (_commonUtils as OdbcSqlServerUtils).ServerVersion > 10;
            var sql = versionGreaterThan10 ? this.ToSql() : this.ToSqlValuesOrSelectUnionAll(false);
            if (string.IsNullOrEmpty(sql)) return 0;

            sql = string.Concat(sql, "; SELECT SCOPE_IDENTITY();");
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(_connection, _transaction, CommandType.Text, sql, _params)), out ret);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfter?.Invoke(this, after);
            }
            return ret;
        }
        protected override List<T1> RawExecuteInserted()
        {
            var versionGreaterThan10 = (_commonUtils as OdbcSqlServerUtils).ServerVersion > 10;
            var sql = versionGreaterThan10 ? this.ToSql() : this.ToSqlValuesOrSelectUnionAll(false);
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(" OUTPUT ");
            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.QuoteReadColumn(col.CsType, col.Attribute.MapType, $"INSERTED.{_commonUtils.QuoteSqlName(col.Attribute.Name)}")).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }

            if (versionGreaterThan10)
            {
                var validx = sql.IndexOf(") VALUES");
                if (validx == -1) throw new ArgumentException("找不到 VALUES");
                sb.Insert(0, sql.Substring(0, validx + 1));
                sb.Append(sql.Substring(validx + 1));
            }
            else
            {
                var validx = sql.IndexOf(") SELECT ");
                if (validx == -1) throw new ArgumentException("找不到 SELECT");
                sb.Insert(0, sql.Substring(0, validx + 1));
                sb.Append(sql.Substring(validx + 1));
            }

            sql = sb.ToString();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                ret = _orm.Ado.Query<T1>(_connection, _transaction, CommandType.Text, sql, _params);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfter?.Invoke(this, after);
            }
            return ret;
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync() => base.SplitExecuteAffrowsAsync(1000, 2100);
        public override Task<long> ExecuteIdentityAsync() => base.SplitExecuteIdentityAsync(1000, 2100);
        public override Task<List<T1>> ExecuteInsertedAsync() => base.SplitExecuteInsertedAsync(1000, 2100);

        async protected override Task<int> RawExecuteAffrowsAsync()
        {
            var versionGreaterThan10 = (_commonUtils as OdbcSqlServerUtils).ServerVersion > 10;
            var sql = versionGreaterThan10 ? this.ToSql() : this.ToSqlValuesOrSelectUnionAll(false);
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            var affrows = 0;
            Exception exception = null;
            try
            {
                affrows = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _params);
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
        async protected override Task<long> RawExecuteIdentityAsync()
        {
            var versionGreaterThan10 = (_commonUtils as OdbcSqlServerUtils).ServerVersion > 10;
            var sql = versionGreaterThan10 ? this.ToSql() : this.ToSqlValuesOrSelectUnionAll(false);
            if (string.IsNullOrEmpty(sql)) return 0;

            sql = string.Concat(sql, "; SELECT SCOPE_IDENTITY();");
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(_connection, _transaction, CommandType.Text, sql, _params)), out ret);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfter?.Invoke(this, after);
            }
            return ret;
        }
        async protected override Task<List<T1>> RawExecuteInsertedAsync()
        {
            var versionGreaterThan10 = (_commonUtils as OdbcSqlServerUtils).ServerVersion > 10;
            var sql = versionGreaterThan10 ? this.ToSql() : this.ToSqlValuesOrSelectUnionAll(false);
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(" OUTPUT ");
            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.QuoteReadColumn(col.CsType, col.Attribute.MapType, $"INSERTED.{_commonUtils.QuoteSqlName(col.Attribute.Name)}")).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }

            if (versionGreaterThan10)
            {
                var validx = sql.IndexOf(") VALUES");
                if (validx == -1) throw new ArgumentException("找不到 VALUES");
                sb.Insert(0, sql.Substring(0, validx + 1));
                sb.Append(sql.Substring(validx + 1));
            }
            else
            {
                var validx = sql.IndexOf(") SELECT ");
                if (validx == -1) throw new ArgumentException("找不到 SELECT");
                sb.Insert(0, sql.Substring(0, validx + 1));
                sb.Append(sql.Substring(validx + 1));
            }

            sql = sb.ToString();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                ret = await _orm.Ado.QueryAsync<T1>(_connection, _transaction, CommandType.Text, sql, _params);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfter?.Invoke(this, after);
            }
            return ret;
        }
#endif
    }
}