using FreeSql.Internal;
using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.SqlServer.Curd
{

    class SqlServerInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        public SqlServerInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(1000, 2100);
        public override Task<int> ExecuteAffrowsAsync() => base.SplitExecuteAffrowsAsync(1000, 2100);
        public override long ExecuteIdentity() => base.SplitExecuteIdentity(1000, 2100);
        public override Task<long> ExecuteIdentityAsync() => base.SplitExecuteIdentityAsync(1000, 2100);
        public override List<T1> ExecuteInserted() => base.SplitExecuteInserted(1000, 2100);
        public override Task<List<T1>> ExecuteInsertedAsync() => base.SplitExecuteInsertedAsync(1000, 2100);


        protected override long RawExecuteIdentity()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            sql = string.Concat(sql, "; SELECT SCOPE_IDENTITY();");
            var before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Insert, sql, _params);
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
        async protected override Task<long> RawExecuteIdentityAsync()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            sql = string.Concat(sql, "; SELECT SCOPE_IDENTITY();");
            var before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Insert, sql, _params);
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

        protected override List<T1> RawExecuteInserted()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(" OUTPUT ");
            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.QuoteReadColumn(col.Attribute.MapType, $"INSERTED.{_commonUtils.QuoteSqlName(col.Attribute.Name)}")).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }

            var validx = sql.IndexOf(") VALUES");
            if (validx == -1) throw new ArgumentException("找不到 VALUES");
            sb.Insert(0, sql.Substring(0, validx + 1));
            sb.Append(sql.Substring(validx + 1));

            sql = sb.ToString();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Insert, sql, _params);
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
        async protected override Task<List<T1>> RawExecuteInsertedAsync()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(" OUTPUT ");
            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.QuoteReadColumn(col.Attribute.MapType, $"INSERTED.{_commonUtils.QuoteSqlName(col.Attribute.Name)}")).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }

            var validx = sql.IndexOf(") VALUES");
            if (validx == -1) throw new ArgumentException("找不到 VALUES");
            sb.Insert(0, sql.Substring(0, validx + 1));
            sb.Append(sql.Substring(validx + 1));

            sql = sb.ToString();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Insert, sql, _params);
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
    }
}