using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Sqlite.Curd
{

    class SqliteInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        public SqliteInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(5000, 999);
        public override Task<int> ExecuteAffrowsAsync() => base.SplitExecuteAffrowsAsync(5000, 999);
        public override long ExecuteIdentity() => base.SplitExecuteIdentity(5000, 999);
        public override Task<long> ExecuteIdentityAsync() => base.SplitExecuteIdentityAsync(5000, 999);
        public override List<T1> ExecuteInserted() => base.SplitExecuteInserted(5000, 999);
        public override Task<List<T1>> ExecuteInsertedAsync() => base.SplitExecuteInsertedAsync(5000, 999);


        protected override long RawExecuteIdentity()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            sql = string.Concat(sql, "; SELECT last_insert_rowid();");
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

            sql = string.Concat(sql, "; SELECT last_insert_rowid();");
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

            this.RawExecuteAffrows();
            return _source;
        }
        async protected override Task<List<T1>> RawExecuteInsertedAsync()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            await this.RawExecuteAffrowsAsync();
            return _source;
        }
    }
}
