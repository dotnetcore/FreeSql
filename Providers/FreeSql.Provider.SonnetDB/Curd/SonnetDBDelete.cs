using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Provider.SonnetDB.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.SonnetDB.Curd
{
    class SonnetDBDelete<T1> : Internal.CommonProvider.DeleteProvider<T1>
    {
        public SonnetDBDelete(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        public override List<T1> ExecuteDeleted() => throw new NotSupportedException(
            "SonnetDB 3.1 不支持 DELETE ... RETURNING；FreeSql ExecuteDeleted 不可用。");

        public override string ToSql()
        {
            var sql = base.ToSql();
            if (_table?.Primarys == null || _table.Primarys.Length == 0) return sql;
            var projection = _table.Primarys.Length == 1
                ? "ftb_del." + _commonUtils.QuoteSqlName(_table.Primarys[0].Attribute.Name)
                : "ftb_del.as1";
            return SonnetDBMutationSql.RewritePrimaryKeySubquery(sql, projection);
        }

        public override int ExecuteAffrows()
        {
            EnsureMeasurementTransactionIsNotUsed();
            var affrows = 0;
            DbParameter[] dbParms = null;
            ToSqlFetch(sb =>
            {
                if (dbParms == null) dbParms = _params.ToArray();
                var sql = RewriteMutationSql(sb);
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, sql, dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    if (SonnetDBModel.IsMeasurement(_table))
                    {
                        // 测量删除返回墓碑数量（可能是 series × field），而
                        // FreeSql ExecuteAffrows 的约定是返回被删除的记录数。
                        var countSql = BuildCountSql(sql);
                        var counted = string.IsNullOrEmpty(countSql) ? 0 : GetCount(countSql, dbParms);
                        _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms);
                        affrows += counted;
                    }
                    else
                    {
                        affrows += _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms);
                    }
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
        async public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            EnsureMeasurementTransactionIsNotUsed();
            var affrows = 0;
            DbParameter[] dbParms = null;
            await ToSqlFetchAsync(async sb =>
            {
                if (dbParms == null) dbParms = _params.ToArray();
                var sql = RewriteMutationSql(sb);
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, sql, dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    if (SonnetDBModel.IsMeasurement(_table))
                    {
                        var countSql = BuildCountSql(sql);
                        var counted = string.IsNullOrEmpty(countSql) ? 0 : await GetCountAsync(countSql, dbParms, cancellationToken);
                        await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
                        affrows += counted;
                    }
                    else
                    {
                        affrows += await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
                    }
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

        public override Task<List<T1>> ExecuteDeletedAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException(
            "SonnetDB 3.1 不支持 DELETE ... RETURNING；FreeSql ExecuteDeletedAsync 不可用。");
#endif

        void EnsureMeasurementTransactionIsNotUsed()
        {
            if (!SonnetDBModel.IsMeasurement(_table)) return;
            if (_transaction != null || _orm.Ado.TransactionCurrentThread != null)
                throw new NotSupportedException(
                    "SonnetDB 时序测量删除不能在事务中执行；请先提交或回滚当前事务，或改用关系表。");
        }

        string RewriteMutationSql(StringBuilder sql)
        {
            if (_table?.Primarys == null || _table.Primarys.Length == 0) return sql.ToString();
            var projection = _table.Primarys.Length == 1
                ? "ftb_del." + _commonUtils.QuoteSqlName(_table.Primarys[0].Attribute.Name)
                : "ftb_del.as1";
            return SonnetDBMutationSql.RewritePrimaryKeySubquery(sql.ToString(), projection);
        }

        string BuildCountSql(string deleteSql)
        {
            const string prefix = "DELETE FROM ";
            if (deleteSql.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == false) return null;
            var whereIndex = deleteSql.IndexOf(" WHERE ", StringComparison.OrdinalIgnoreCase);
            var tableName = whereIndex == -1
                ? deleteSql.Substring(prefix.Length).Trim()
                : deleteSql.Substring(prefix.Length, whereIndex - prefix.Length);
            return $"SELECT count({GetCountField()}) as1 FROM {tableName}" +
                (whereIndex == -1 ? string.Empty : deleteSql.Substring(whereIndex));
        }

        string GetCountField()
        {
            var col = _table.ColumnsByPosition.FirstOrDefault(a => IsFieldColumn(_table, a));
            return col == null ? "*" : _commonUtils.QuoteSqlName(col.Attribute.Name);
        }

        static bool IsFieldColumn(TableInfo table, ColumnInfo column)
        {
            if (string.Equals(column.Attribute.Name, "time", StringComparison.OrdinalIgnoreCase)) return false;
            var dbType = (column.Attribute.DbType ?? "").Trim();
            if (dbType.StartsWith("FIELD", StringComparison.OrdinalIgnoreCase)) return true;
            if (dbType.StartsWith("TAG", StringComparison.OrdinalIgnoreCase)) return false;
            if (table.Properties.TryGetValue(column.CsName, out var property))
            {
                if (property.GetCustomAttribute<SonnetDBFieldAttribute>() != null) return true;
                if (property.GetCustomAttribute<SonnetDBTagAttribute>() != null) return false;
            }
            var mapType = column.Attribute.MapType.NullableTypeOrThis();
            return mapType != typeof(string) && mapType != typeof(char) && mapType != typeof(Guid);
        }

        int GetCount(string countSql, DbParameter[] dbParms)
        {
            var value = _orm.Ado.ExecuteScalar(_connection, _transaction, CommandType.Text, countSql, _commandTimeout, dbParms);
            return long.TryParse(string.Concat(value), out var count) ? count > int.MaxValue ? int.MaxValue : (int)count : 0;
        }

#if net40
#else
        async Task<int> GetCountAsync(string countSql, DbParameter[] dbParms, CancellationToken cancellationToken)
        {
            var value = await _orm.Ado.ExecuteScalarAsync(_connection, _transaction, CommandType.Text, countSql, _commandTimeout, dbParms, cancellationToken);
            return long.TryParse(string.Concat(value), out var count) ? count > int.MaxValue ? int.MaxValue : (int)count : 0;
        }
#endif
    }
}
