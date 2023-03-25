using FreeSql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using FreeSql.Internal.Model;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.ObjectPool;
using System.Linq;
using System.Data.Common;
#if MySqlConnector
using MySqlConnector;
#else
using MySql.Data.MySqlClient;
#endif

public static class FreeSqlMySqlConnectorGlobalExtensions
{
    #region ExecuteMySqlBulkCopy
    /// <summary>
    /// 批量插入或更新（操作的字段数量超过 2000 时收益大）<para></para>
    /// 实现原理：使用 MySqlBulkCopy 插入临时表，再执行 INSERT INTO t1 select * from #temp ON DUPLICATE KEY UPDATE ...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <param name="bulkCopyTimeout"></param>
    /// <returns></returns>
    public static int ExecuteMySqlBulkCopy<T>(this IInsertOrUpdate<T> that, int? bulkCopyTimeout = null) where T : class
    {
        var upsert = that as InsertOrUpdateProvider<T>;
        if (upsert._source.Any() != true || upsert._tempPrimarys.Any() == false) return 0;
        var state = ExecuteMySqlBulkCopyState(upsert);
        return UpdateProvider.ExecuteBulkUpsert(upsert, state, insert => insert.ExecuteMySqlBulkCopy(bulkCopyTimeout));
    }
    static NativeTuple<string, string, string, string, string[]> ExecuteMySqlBulkCopyState<T>(InsertOrUpdateProvider<T> upsert) where T : class
    {
        if (upsert._source.Any() != true) return null;
        var _table = upsert._table;
        var _commonUtils = upsert._commonUtils;
        var updateTableName = upsert._tableRule?.Invoke(_table.DbName) ?? _table.DbName;
        var tempTableName = $"Temp_{Guid.NewGuid().ToString("N")}";
        if (upsert._orm.CodeFirst.IsSyncStructureToLower) tempTableName = tempTableName.ToLower();
        if (upsert._orm.CodeFirst.IsSyncStructureToUpper) tempTableName = tempTableName.ToUpper();
        if (upsert._connection == null && upsert._orm.Ado.TransactionCurrentThread != null)
            upsert.WithTransaction(upsert._orm.Ado.TransactionCurrentThread);
        var sb = new StringBuilder().Append("CREATE TEMPORARY TABLE ").Append(_commonUtils.QuoteSqlName(tempTableName)).Append(" ( ");
        foreach (var col in _table.Columns.Values)
        {
            sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" ").Append(col.Attribute.DbType.Replace("NOT NULL", ""));
            sb.Append(",");
        }
        var sql1 = sb.Remove(sb.Length - 1, 1).Append(" \r\n) Engine=InnoDB;").ToString();
        try
        {
            upsert._sourceSql = $"select __**__ from {tempTableName}";
            var sql2 = upsert.ToSql();
            if (string.IsNullOrWhiteSpace(sql2) == false)
            {
                var field = sql2.Substring(sql2.IndexOf("`(") + 2);
                field = field.Remove(field.IndexOf(upsert._sourceSql)).TrimEnd().TrimEnd(')');
                sql2 = sql2.Replace(upsert._sourceSql, $"select {field} from {tempTableName}");
            }
            var sql3 = $"DROP TABLE {_commonUtils.QuoteSqlName(tempTableName)}";
            return NativeTuple.Create(sql1, sql2, sql3, tempTableName, _table.Columns.Values.Select(a => a.Attribute.Name).ToArray());
        }
        finally
        {
            upsert._sourceSql = null;
        }
    }
    /// <summary>
    /// 批量更新（更新字段数量超过 2000 时收益大）<para></para>
    /// 实现原理：使用 MySqlBulkCopy 插入临时表，再使用 UPDATE INNER JOIN 联表更新
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <param name="bulkCopyTimeout"></param>
    /// <returns></returns>
    public static int ExecuteMySqlBulkCopy<T>(this IUpdate<T> that, int? bulkCopyTimeout = null) where T : class
    {
        var update = that as UpdateProvider<T>;
        if (update._source.Any() != true || update._tempPrimarys.Any() == false) return 0;
        var state = ExecuteMySqlBulkCopyState(update);
        return UpdateProvider.ExecuteBulkUpdate(update, state, insert => insert.ExecuteMySqlBulkCopy(bulkCopyTimeout));
    }
    static NativeTuple<string, string, string, string, string[]> ExecuteMySqlBulkCopyState<T>(UpdateProvider<T> update) where T : class
    {
        if (update._source.Any() != true) return null;
        var _table = update._table;
        var _commonUtils = update._commonUtils;
        var updateTableName = update._tableRule?.Invoke(_table.DbName) ?? _table.DbName;
        var tempTableName = $"Temp_{Guid.NewGuid().ToString("N")}";
        if (update._orm.CodeFirst.IsSyncStructureToLower) tempTableName = tempTableName.ToLower();
        if (update._orm.CodeFirst.IsSyncStructureToUpper) tempTableName = tempTableName.ToUpper();
        if (update._connection == null && update._orm.Ado.TransactionCurrentThread != null)
            update.WithTransaction(update._orm.Ado.TransactionCurrentThread);
        var sb = new StringBuilder().Append("CREATE TEMPORARY TABLE ").Append(_commonUtils.QuoteSqlName(tempTableName)).Append(" ( ");
        var setColumns = new List<string>();
        var pkColumns = new List<string>();
        foreach (var col in _table.Columns.Values)
        {
            if (update._tempPrimarys.Any(a => a.CsName == col.CsName)) pkColumns.Add(col.Attribute.Name);
            else if (col.Attribute.IsIdentity == false && col.Attribute.IsVersion == false && update._ignore.ContainsKey(col.Attribute.Name) == false) setColumns.Add(col.Attribute.Name);
            else continue;
            sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" ").Append(col.Attribute.DbType.Replace("NOT NULL", ""));
            sb.Append(",");
        }
        var sql1 = sb.Remove(sb.Length - 1, 1).Append(" \r\n) Engine=InnoDB;").ToString();

        sb.Clear().Append("UPDATE ").Append(_commonUtils.QuoteSqlName(updateTableName)).Append(" a ")
            .Append(" \r\nINNER JOIN ").Append(_commonUtils.QuoteSqlName(tempTableName)).Append(" b ON ").Append(string.Join(" AND ", pkColumns.Select(col => $"a.{_commonUtils.QuoteSqlName(col)} = b.{_commonUtils.QuoteSqlName(col)}")))
            .Append(" \r\nSET \r\n  ").Append(string.Join(", \r\n  ", setColumns.Select(col => $"a.{_commonUtils.QuoteSqlName(col)} = b.{_commonUtils.QuoteSqlName(col)}")));
        var sql2 = sb.ToString();
        sb.Clear();
        var sql3 = $"DROP TABLE {_commonUtils.QuoteSqlName(tempTableName)}";
        return NativeTuple.Create(sql1, sql2, sql3, tempTableName, pkColumns.Concat(setColumns).ToArray());
    }

    /// <summary>
    /// MySql MySqlCopyBulk 批量插入功能<para></para>
    /// 使用 IgnoreColumns/InsertColumns 设置忽略/指定导入的列<para></para>
    /// 使用 WithConnection/WithTransaction 传入连接/事务对象<para></para>
    /// 提示：若本方法不能满足，请使用 IInsert&lt;T&gt;.ToDataTable 方法得到 DataTable 对象后，自行处理。<para></para>
    /// MySqlCopyBulk 与 insert into t values(..),(..),(..) 性能测试参考：<para></para>
    /// 插入180000行，52列：28,405ms 与 38,481ms，10列：6,504ms 与 11,171ms<para></para>
    /// 插入10000行，52列：1,142ms 与 2,234ms，10列：339ms 与 866ms<para></para>
    /// 插入5000行，52列：657ms 与 1,136ms，10列：257ms 与 366ms<para></para>
    /// 插入2000行，52列：451ms 与 284ms，10列：116ms 与 80ms<para></para>
    /// 插入1000行，52列：435ms 与 239ms，10列：87ms 与 83ms<para></para>
    /// 插入500行，52列：592ms 与 167ms，10列：100ms 与 50ms<para></para>
    /// 插入100行，52列：47ms 与 66ms，10列：16ms 与 24ms<para></para>
    /// 插入50行，52列：22ms 与 30ms，10列：16ms 与 34ms<para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <param name="bulkCopyTimeout"></param>
    public static void ExecuteMySqlBulkCopy<T>(this IInsert<T> that, int? bulkCopyTimeout = null) where T : class
    {
        var insert = that as FreeSql.MySql.Curd.MySqlInsert<T>;
        if (insert == null) throw new Exception(CoreStrings.S_Features_Unique("ExecuteMySqlBulkCopy", "MySqlConnector"));

        var dt = that.ToDataTable();
        if (dt.Rows.Count == 0) return;

        Action<MySqlBulkCopy> writeToServer = bulkCopy =>
        {
            if (bulkCopyTimeout.HasValue) bulkCopy.BulkCopyTimeout = bulkCopyTimeout.Value;
            bulkCopy.DestinationTableName = dt.TableName;
            for (int i = 0; i < dt.Columns.Count; i++)
                bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, dt.Columns[i].ColumnName));
            bulkCopy.WriteToServer(dt);
        };

        try
        {
            if (insert.InternalConnection == null && insert.InternalTransaction == null)
            {
                if (insert._orm.Ado?.TransactionCurrentThread != null)
                    writeToServer(new MySqlBulkCopy(insert._orm.Ado.TransactionCurrentThread.Connection as MySqlConnection, insert._orm.Ado?.TransactionCurrentThread as MySqlTransaction));
                else
                    using (var conn = insert.InternalOrm.Ado.MasterPool.Get())
                        writeToServer(new MySqlBulkCopy(conn.Value as MySqlConnection));
            }
            else if (insert.InternalTransaction != null)
            {
                writeToServer(new MySqlBulkCopy(insert.InternalTransaction.Connection as MySqlConnection, insert.InternalTransaction as MySqlTransaction));
            }
            else if (insert.InternalConnection != null)
            {
                var conn = insert.InternalConnection as MySqlConnection;
                var isNotOpen = false;
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    isNotOpen = true;
                    conn.Open();
                }
                try
                {
                    writeToServer(new MySqlBulkCopy(conn));
                }
                finally
                {
                    if (isNotOpen)
                        conn.Close();
                }
            }
            else
            {
                throw new NotImplementedException($"ExecuteMySqlBulkCopy {CoreStrings.S_Not_Implemented_FeedBack}");
            }
        }
        finally
        {
            dt.Clear();
        }
    }
#if net40
#else
    public static Task<int> ExecuteMySqlBulkCopyAsync<T>(this IInsertOrUpdate<T> that, int? bulkCopyTimeout = null, CancellationToken cancellationToken = default) where T : class
    {
        var upsert = that as UpdateProvider<T>;
        if (upsert._source.Any() != true || upsert._tempPrimarys.Any() == false) return Task.FromResult(0);
        var state = ExecuteMySqlBulkCopyState(upsert);
        return UpdateProvider.ExecuteBulkUpdateAsync(upsert, state, insert => insert.ExecuteMySqlBulkCopyAsync(bulkCopyTimeout, cancellationToken));
    }
    public static Task<int> ExecuteMySqlBulkCopyAsync<T>(this IUpdate<T> that, int? bulkCopyTimeout = null, CancellationToken cancellationToken = default) where T : class
    {
        var update = that as UpdateProvider<T>;
        if (update._source.Any() != true || update._tempPrimarys.Any() == false) return Task.FromResult(0);
        var state = ExecuteMySqlBulkCopyState(update);
        return UpdateProvider.ExecuteBulkUpdateAsync(update, state, insert => insert.ExecuteMySqlBulkCopyAsync(bulkCopyTimeout, cancellationToken));
    }
    async public static Task ExecuteMySqlBulkCopyAsync<T>(this IInsert<T> that, int? bulkCopyTimeout = null, CancellationToken cancellationToken = default) where T : class
    {
        var insert = that as FreeSql.MySql.Curd.MySqlInsert<T>;
        if (insert == null) throw new Exception(CoreStrings.S_Features_Unique("ExecuteMySqlBulkCopyAsync", "MySqlConnector"));

        var dt = that.ToDataTable();
        if (dt.Rows.Count == 0) return;

        Func<MySqlBulkCopy, Task> writeToServer = async bulkCopy =>
        {
            if (bulkCopyTimeout.HasValue) bulkCopy.BulkCopyTimeout = bulkCopyTimeout.Value;
            bulkCopy.DestinationTableName = dt.TableName;
            for (int i = 0; i < dt.Columns.Count; i++)
                bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, dt.Columns[i].ColumnName));
            await bulkCopy.WriteToServerAsync(dt, cancellationToken);
        };

        try
        {
            if (insert.InternalConnection == null && insert.InternalTransaction == null)
            {
                if (insert._orm.Ado?.TransactionCurrentThread != null)
                    await writeToServer(new MySqlBulkCopy(insert._orm.Ado.TransactionCurrentThread.Connection as MySqlConnection, insert._orm.Ado?.TransactionCurrentThread as MySqlTransaction));
                else
                    using (var conn = insert.InternalOrm.Ado.MasterPool.Get())
                        await writeToServer(new MySqlBulkCopy(conn.Value as MySqlConnection));
            }
            else if (insert.InternalTransaction != null)
            {
                await writeToServer(new MySqlBulkCopy(insert.InternalTransaction.Connection as MySqlConnection, insert.InternalTransaction as MySqlTransaction));
            }
            else if (insert.InternalConnection != null)
            {
                var conn = insert.InternalConnection as MySqlConnection;
                var isNotOpen = false;
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    isNotOpen = true;
                    await conn.OpenAsync(cancellationToken);
                }
                try
                {
                    await writeToServer(new MySqlBulkCopy(conn));
                }
                finally
                {
                    if (isNotOpen)
                        await conn.CloseAsync();
                }
            }
            else
            {
                throw new NotImplementedException($"ExecuteMySqlBulkCopyAsync {CoreStrings.S_Not_Implemented_FeedBack}");
            }
        }
        finally
        {
            dt.Clear();
        }
    }
#endif
    #endregion
}
