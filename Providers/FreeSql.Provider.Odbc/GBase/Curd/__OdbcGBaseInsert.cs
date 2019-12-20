using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Odbc.GBase
{

    class OdbcGBaseInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        public OdbcGBaseInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(_batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
        public override long ExecuteIdentity() => base.SplitExecuteIdentity(_batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
        public override List<T1> ExecuteInserted() => base.SplitExecuteInserted(_batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);

        protected override long RawExecuteIdentity()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            long ret = 0;
            Exception exception = null;
            Aop.CurdBeforeEventArgs before = null;

            var identCols = _table.Columns.Where(a => a.Value.Attribute.IsIdentity == true);
            if (identCols.Any() == false)
            {
                before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
                _orm.Aop.CurdBefore?.Invoke(this, before);
                try
                {
                    ret = _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _params);
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
                return 0;
            }
            sql = string.Concat(sql, " RETURNING ", _commonUtils.QuoteSqlName(identCols.First().Value.Attribute.Name));
            before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBefore?.Invoke(this, before);
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
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var ret = _source.ToList();
            this.RawExecuteAffrows();
            return ret;
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync() => base.SplitExecuteAffrowsAsync(5000, 3000);
        public override Task<long> ExecuteIdentityAsync() => base.SplitExecuteIdentityAsync(5000, 3000);
        public override Task<List<T1>> ExecuteInsertedAsync() => base.SplitExecuteInsertedAsync(5000, 3000);
        
        async protected override Task<long> RawExecuteIdentityAsync()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            long ret = 0;
            Exception exception = null;
            Aop.CurdBeforeEventArgs before = null;

            var identCols = _table.Columns.Where(a => a.Value.Attribute.IsIdentity == true);
            if (identCols.Any() == false)
            {
                before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
                _orm.Aop.CurdBefore?.Invoke(this, before);
                try
                {
                    ret = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _params);
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
                return 0;
            }
            sql = string.Concat(sql, " RETURNING ", _commonUtils.QuoteSqlName(identCols.First().Value.Attribute.Name));
            before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBefore?.Invoke(this, before);
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
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var ret = _source.ToList();
            await this.RawExecuteAffrowsAsync();
            return ret;
        }
#endif
    }
}
