using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Provider.SonnetDB.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
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

        public override List<T1> ExecuteDeleted() => throw new NotImplementedException($"FreeSql.Provider.SonnetDB {CoreErrorStrings.S_Not_Implemented_Feature}");

        public override int ExecuteAffrows()
        {
            var affrows = 0;
            DbParameter[] dbParms = null;
            ToSqlFetch(sb =>
            {
                if (dbParms == null) dbParms = _params.ToArray();
                var sql = sb.ToString();
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, sql, dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    var countSql = BuildCountSql(sql);
                    var counted = string.IsNullOrEmpty(countSql) ? 0 : GetCount(countSql, dbParms);
                    var deleted = _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms);
                    affrows += counted > 0 ? counted : deleted;
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
            var affrows = 0;
            DbParameter[] dbParms = null;
            await ToSqlFetchAsync(async sb =>
            {
                if (dbParms == null) dbParms = _params.ToArray();
                var sql = sb.ToString();
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, sql, dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    var countSql = BuildCountSql(sql);
                    var counted = string.IsNullOrEmpty(countSql) ? 0 : await GetCountAsync(countSql, dbParms, cancellationToken);
                    var deleted = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
                    affrows += counted > 0 ? counted : deleted;
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

        public override Task<List<T1>> ExecuteDeletedAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException($"FreeSql.Provider.SonnetDB {CoreErrorStrings.S_Not_Implemented_Feature}");
#endif

        string BuildCountSql(string deleteSql)
        {
            const string prefix = "DELETE FROM ";
            if (deleteSql.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == false) return null;
            var whereIndex = deleteSql.IndexOf(" WHERE ", StringComparison.OrdinalIgnoreCase);
            if (whereIndex == -1) return null;
            var tableName = deleteSql.Substring(prefix.Length, whereIndex - prefix.Length);
            return $"SELECT count({GetCountField()}) as1 FROM {tableName}{deleteSql.Substring(whereIndex)}";
        }

        string GetCountField()
        {
            var col = _table.ColumnsByPosition.FirstOrDefault(a => IsFieldColumn(_table, a));
            return col == null ? "*" : _commonUtils.QuoteSqlName(col.Attribute.Name);
        }

        static bool IsFieldColumn(TableInfo tb, ColumnInfo col)
        {
            if (string.Equals(col.Attribute.Name, "time", StringComparison.OrdinalIgnoreCase)) return false;
            var dbType = (col.Attribute.DbType ?? "").Trim();
            if (dbType.StartsWith("FIELD", StringComparison.OrdinalIgnoreCase)) return true;
            if (dbType.StartsWith("TAG", StringComparison.OrdinalIgnoreCase)) return false;
            if (tb.Properties.TryGetValue(col.CsName, out var property))
            {
                if (property.GetCustomAttribute<SonnetDBFieldAttribute>() != null) return true;
                if (property.GetCustomAttribute<SonnetDBTagAttribute>() != null) return false;
            }
            var mapType = col.Attribute.MapType.NullableTypeOrThis();
            return mapType != typeof(string) && mapType != typeof(char) && mapType != typeof(Guid);
        }

        int GetCount(string countSql, DbParameter[] dbParms)
        {
            var val = _orm.Ado.ExecuteScalar(_connection, _transaction, CommandType.Text, countSql, _commandTimeout, dbParms);
            return long.TryParse(string.Concat(val), out var ret) ? ret > int.MaxValue ? int.MaxValue : (int)ret : 0;
        }

#if net40
#else
        async Task<int> GetCountAsync(string countSql, DbParameter[] dbParms, CancellationToken cancellationToken)
        {
            var val = await _orm.Ado.ExecuteScalarAsync(_connection, _transaction, CommandType.Text, countSql, _commandTimeout, dbParms, cancellationToken);
            return long.TryParse(string.Concat(val), out var ret) ? ret > int.MaxValue ? int.MaxValue : (int)ret : 0;
        }
#endif
    }
}
