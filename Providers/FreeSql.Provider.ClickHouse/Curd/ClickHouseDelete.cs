using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.ClickHouse.Curd
{

    class ClickHouseDelete<T1> : Internal.CommonProvider.DeleteProvider<T1>
    {
        public ClickHouseDelete(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        public override List<T1> ExecuteDeleted() => throw new NotImplementedException($"FreeSql.Provider.ClickHouse {CoreStrings.S_Not_Implemented_Feature}");

        public override string ToSql()
        {
            return ReplaceDeleteSql(base.ToSql());
        }

        string ReplaceDeleteSql(string sql) => sql?.Replace("DELETE FROM ", "ALTER TABLE ").Replace(" WHERE ", " DELETE WHERE ");

        public override int ExecuteAffrows()
        {
            var affrows = 0;
            DbParameter[] dbParms = null;
            ToSqlFetch(sb =>
            {
                if (dbParms == null) dbParms = _params.ToArray();
                var sql = ReplaceDeleteSql(sb.ToString());
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, sql, dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    affrows += _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw;
                }
                finally
                {
                    var after = new Aop.CurdAfterEventArgs(before, exception, affrows);
                    _orm.Aop.CurdAfterHandler?.Invoke(this, after);
                }
            });
            if (dbParms != null) this.ClearData();
            return affrows;
        }

#if net40
#else
        public override Task<List<T1>> ExecuteDeletedAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException($"FreeSql.Provider.ClickHouse {CoreStrings.S_Not_Implemented_Feature}");

        async public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            var affrows = 0;
            DbParameter[] dbParms = null;
            await ToSqlFetchAsync(async sb =>
            {
                if (dbParms == null) dbParms = _params.ToArray();
                var sql = ReplaceDeleteSql(sb.ToString());
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, sql, dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    affrows += await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw;
                }
                finally
                {
                    var after = new Aop.CurdAfterEventArgs(before, exception, affrows);
                    _orm.Aop.CurdAfterHandler?.Invoke(this, after);
                }
            });
            if (dbParms != null) this.ClearData();
            return affrows;
        }
#endif
    }
}
