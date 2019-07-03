using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.MySql.Curd
{

    class MySqlInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        public MySqlInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(5000, 3000);
        public override Task<int> ExecuteAffrowsAsync() => base.SplitExecuteAffrowsAsync(5000, 3000);
        public override long ExecuteIdentity() => base.SplitExecuteIdentity(5000, 3000);
        public override Task<long> ExecuteIdentityAsync() => base.SplitExecuteIdentityAsync(5000, 3000);
        public override List<T1> ExecuteInserted() => base.SplitExecuteInserted(5000, 3000);
        public override Task<List<T1>> ExecuteInsertedAsync() => base.SplitExecuteInsertedAsync(5000, 3000);


        protected override long RawExecuteIdentity()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            sql = string.Concat(sql, "; SELECT LAST_INSERT_ID();");
            var before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                ret = long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(_connection, _transaction, CommandType.Text, sql, _params)), out var trylng) ? trylng : 0;
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

            sql = string.Concat(sql, "; SELECT LAST_INSERT_ID();");
            var before = new Aop.CurdBeforeEventArgs(_table.Type, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBefore?.Invoke(this, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                ret = long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(_connection, _transaction, CommandType.Text, sql, _params)), out var trylng) ? trylng : 0;
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
            sb.Append(sql).Append(" RETURNING ");

            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.QuoteReadColumn(col.Attribute.MapType, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }
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
            sb.Append(sql).Append(" RETURNING ");

            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.QuoteReadColumn(col.Attribute.MapType, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }
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
