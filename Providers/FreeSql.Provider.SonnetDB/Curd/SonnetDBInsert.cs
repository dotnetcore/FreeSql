using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.SonnetDB.Curd
{
    class SonnetDBInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        public SonnetDBInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(_batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
        public override long ExecuteIdentity() => base.SplitExecuteIdentity(_batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
        public override List<T1> ExecuteInserted() => throw new NotImplementedException($"FreeSql.Provider.SonnetDB {CoreErrorStrings.S_Not_Implemented_Feature}");

        protected override long RawExecuteIdentity()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params);
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

        protected override List<T1> RawExecuteInserted() => throw new NotImplementedException($"FreeSql.Provider.SonnetDB {CoreErrorStrings.S_Not_Implemented_Feature}");

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => base.SplitExecuteAffrowsAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);
        public override Task<long> ExecuteIdentityAsync(CancellationToken cancellationToken = default) => base.SplitExecuteIdentityAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);
        public override Task<List<T1>> ExecuteInsertedAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException($"FreeSql.Provider.SonnetDB {CoreErrorStrings.S_Not_Implemented_Feature}");

        async protected override Task<long> RawExecuteIdentityAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            long ret = 0;
            Exception exception = null;
            try
            {
                await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, _params, cancellationToken);
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

        protected override Task<List<T1>> RawExecuteInsertedAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException($"FreeSql.Provider.SonnetDB {CoreErrorStrings.S_Not_Implemented_Feature}");
#endif
    }
}
