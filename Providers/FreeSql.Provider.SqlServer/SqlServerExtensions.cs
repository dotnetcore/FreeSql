using FreeSql;
using FreeSql.Internal.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using FreeSql.Internal.CommonProvider;
using System.Linq;
using System.Text;
using System.Data.Common;
using FreeSql.Internal.ObjectPool;
#if microsoft
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using System.Threading.Tasks;

public static partial class FreeSqlSqlServerGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatSqlServer(this string that, params object[] args) => _sqlserverAdo.Addslashes(that, args);
    static FreeSql.SqlServer.SqlServerAdo _sqlserverAdo = new FreeSql.SqlServer.SqlServerAdo();

    #region ExecuteSqlBulkCopy
    /// <summary>
    /// 批量插入或更新（操作的字段数量超过 2000 时收益大）<para></para>
    /// 实现原理：使用 SqlBulkCopy 插入临时表，再执行 MERGE INTO t1 using (select * from #temp) ...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <param name="copyOptions"></param>
    /// <param name="batchSize"></param>
    /// <param name="bulkCopyTimeout"></param>
    /// <returns></returns>
    public static int ExecuteSqlBulkCopy<T>(this IInsertOrUpdate<T> that, SqlBulkCopyOptions copyOptions = SqlBulkCopyOptions.Default, int? batchSize = null, int? bulkCopyTimeout = null) where T : class
    {
        var upsert = that as InsertOrUpdateProvider<T>;
        if (upsert._source.Any() != true || upsert._tempPrimarys.Any() == false) return 0;
        var state = ExecuteSqlBulkCopyState(upsert);
        return UpdateProvider.ExecuteBulkUpsert(upsert, state, insert => insert.ExecuteSqlBulkCopy(copyOptions, batchSize, bulkCopyTimeout));
    }
    static NativeTuple<string, string, string, string, string[]> ExecuteSqlBulkCopyState<T>(InsertOrUpdateProvider<T> upsert) where T : class
    {
        if (upsert._source.Any() != true) return null;
        var _table = upsert._table;
        var _commonUtils = upsert._commonUtils;
        var updateTableName = upsert._tableRule?.Invoke(_table.DbName) ?? _table.DbName;
        var tempTableName = $"#Temp_{Guid.NewGuid().ToString("N")}";
        if (upsert._orm.CodeFirst.IsSyncStructureToLower) tempTableName = tempTableName.ToLower();
        if (upsert._orm.CodeFirst.IsSyncStructureToUpper) tempTableName = tempTableName.ToUpper();
        if (upsert._connection == null && upsert._orm.Ado.TransactionCurrentThread != null)
            upsert.WithTransaction(upsert._orm.Ado.TransactionCurrentThread);
        var sql1 = $"SELECT {string.Join(", ", _table.Columns.Values.Select(a => _commonUtils.QuoteSqlName(a.Attribute.Name)))} INTO {tempTableName} FROM {_commonUtils.QuoteSqlName(updateTableName)} WHERE 1=2";
        try
        {
            upsert._sourceSql = $"select * from {tempTableName}";
            var sql2 = upsert.ToSql();
            var sql3 = $"DROP TABLE {tempTableName}";
            return NativeTuple.Create(sql1, sql2, sql3, tempTableName, _table.Columns.Values.Select(a => a.Attribute.Name).ToArray());
        }
        finally
        {
            upsert._sourceSql = null;
        }
    }
    /// <summary>
    /// 批量更新（更新字段数量超过 2000 时收益大）<para></para>
    /// 实现原理：使用 SqlBulkCopy 插入临时表，再使用 UPDATE INNER JOIN 联表更新
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <param name="copyOptions"></param>
    /// <param name="batchSize"></param>
    /// <param name="bulkCopyTimeout"></param>
    /// <returns></returns>
    public static int ExecuteSqlBulkCopy<T>(this IUpdate<T> that, SqlBulkCopyOptions copyOptions = SqlBulkCopyOptions.Default, int? batchSize = null, int? bulkCopyTimeout = null) where T : class
    {
        var update = that as UpdateProvider<T>;
        if (update._source.Any() != true || update._tempPrimarys.Any() == false) return 0;
        var state = ExecuteSqlBulkCopyState(update);
        return UpdateProvider.ExecuteBulkUpdate(update, state, insert => insert.ExecuteSqlBulkCopy(copyOptions, batchSize, bulkCopyTimeout));
    }
    static NativeTuple<string, string, string, string, string[]> ExecuteSqlBulkCopyState<T>(UpdateProvider<T> update) where T : class
    {
        if (update._source.Any() != true) return null;
        var _table = update._table;
        var _commonUtils = update._commonUtils;
        var updateTableName = update._tableRule?.Invoke(_table.DbName) ?? _table.DbName;
        var tempTableName = $"#Temp_{Guid.NewGuid().ToString("N")}";
        if (update._orm.CodeFirst.IsSyncStructureToLower) tempTableName = tempTableName.ToLower();
        if (update._orm.CodeFirst.IsSyncStructureToUpper) tempTableName = tempTableName.ToUpper();
        if (update._connection == null && update._orm.Ado.TransactionCurrentThread != null)
            update.WithTransaction(update._orm.Ado.TransactionCurrentThread);
        var setColumns = new List<string>();
        var pkColumns = new List<string>();
        foreach (var col in _table.Columns.Values)
        {
            if (update._tempPrimarys.Any(a => a.CsName == col.CsName)) pkColumns.Add(col.Attribute.Name);
            else if (col.Attribute.IsIdentity == false && col.Attribute.IsVersion == false && update._ignore.ContainsKey(col.Attribute.Name) == false) setColumns.Add(col.Attribute.Name);
        }
        var sql1 = $"SELECT {string.Join(", ", pkColumns.Select(a => _commonUtils.QuoteSqlName(a)))}, {string.Join(", ", setColumns.Select(a => _commonUtils.QuoteSqlName(a)))} INTO {tempTableName} FROM {_commonUtils.QuoteSqlName(updateTableName)} WHERE 1=2";
        var sb = new StringBuilder().Append("UPDATE ").Append(" a SET \r\n  ").Append(string.Join(", \r\n  ", setColumns.Select(col => $"a.{_commonUtils.QuoteSqlName(col)} = b.{_commonUtils.QuoteSqlName(col)}")));
        sb.Append(" \r\nFROM ").Append(_commonUtils.QuoteSqlName(updateTableName)).Append(" a ")
            .Append(" \r\nINNER JOIN ").Append(tempTableName).Append(" b ON ").Append(string.Join(" AND ", pkColumns.Select(col => $"a.{_commonUtils.QuoteSqlName(col)} = b.{_commonUtils.QuoteSqlName(col)}")));
        var sql2 = sb.ToString();
        sb.Clear();
        var sql3 = $"DROP TABLE {tempTableName}";
        return NativeTuple.Create(sql1, sql2, sql3, tempTableName, pkColumns.Concat(setColumns).ToArray());
    }

    /// <summary>
    /// SqlServer SqlCopyBulk 批量插入功能<para></para>
    /// 使用 IgnoreColumns/InsertColumns 设置忽略/指定导入的列<para></para>
    /// 使用 WithConnection/WithTransaction 传入连接/事务对象<para></para>
    /// 提示：若本方法不能满足，请使用 IInsert&lt;T&gt;.ToDataTable 方法得到 DataTable 对象后，自行处理。<para></para>
    /// SqlCopyBulk 与 insert into t values(..),(..),(..) 性能测试参考：<para></para>
    /// 插入180000行，52列：21,065ms 与 402,355ms，10列：4,248ms 与 47,204ms<para></para>
    /// 插入10000行，52列：578ms 与 24,847ms，10列：127ms 与 2,275ms<para></para>
    /// 插入5000行，52列：326ms 与 11,465ms，10列：71ms 与 1,108ms<para></para>
    /// 插入2000行，52列：139ms 与 4,971ms，10列：30ms 与 488ms<para></para>
    /// 插入1000行，52列：105ms 与 2,437ms，10列：48ms 与 279ms<para></para>
    /// 插入500行，52列：79ms 与 915ms，10列：14ms 与 123ms<para></para>
    /// 插入100行，52列：60ms 与 138ms，10列：11ms 与 35ms<para></para>
    /// 插入50行，52列：48ms 与 88ms，10列：10ms 与 16ms<para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <param name="copyOptions"></param>
    /// <param name="batchSize"></param>
    /// <param name="bulkCopyTimeout"></param>
    public static void ExecuteSqlBulkCopy<T>(this IInsert<T> that, SqlBulkCopyOptions copyOptions = SqlBulkCopyOptions.Default, int? batchSize = null, int? bulkCopyTimeout = null) where T : class
    {
        var insert = that as FreeSql.SqlServer.Curd.SqlServerInsert<T>;
        if (insert == null) throw new Exception(CoreErrorStrings.S_Features_Unique("ExecuteSqlBulkCopy", "SqlServer"));

        if (insert._insertIdentity) copyOptions = copyOptions | SqlBulkCopyOptions.KeepIdentity;
        var dt = that.ToDataTable();
        if (dt.Rows.Count == 0) return;

        Action<SqlBulkCopy> writeToServer = bulkCopy =>
        {
            if (batchSize.HasValue) bulkCopy.BatchSize = batchSize.Value;
            if (bulkCopyTimeout.HasValue) bulkCopy.BulkCopyTimeout = bulkCopyTimeout.Value;
            bulkCopy.DestinationTableName =
                        dt.TableName.IndexOf(' ') >= 0 && !(dt.TableName.StartsWith("[") && dt.TableName.EndsWith("]"))
                        ? $"[{dt.TableName}]"
                        : dt.TableName;
            for (int i = 0; i < dt.Columns.Count; i++)
                bulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
            bulkCopy.WriteToServer(dt);
        };

        try
        {
            if (insert.InternalConnection == null && insert.InternalTransaction == null)
            {
                if (insert._orm.Ado?.TransactionCurrentThread != null)
                    using (var bulkCopy = new SqlBulkCopy(insert._orm.Ado.TransactionCurrentThread.Connection as SqlConnection, copyOptions, insert._orm.Ado.TransactionCurrentThread as SqlTransaction))
                        writeToServer(bulkCopy);
                else
                    using (var conn = insert.InternalOrm.Ado.MasterPool.Get())
                    {
                        using (var bulkCopy = copyOptions == SqlBulkCopyOptions.Default ?
                            new SqlBulkCopy(conn.Value as SqlConnection) :
                            new SqlBulkCopy(conn.Value as SqlConnection, copyOptions, null))
                        {
                            writeToServer(bulkCopy);
                        }
                    }
            }
            else if (insert.InternalTransaction != null)
            {
                using (var bulkCopy = new SqlBulkCopy(insert.InternalTransaction.Connection as SqlConnection, copyOptions, insert.InternalTransaction as SqlTransaction))
                {
                    writeToServer(bulkCopy);
                }
            }
            else if (insert.InternalConnection != null)
            {
                var conn = insert.InternalConnection as SqlConnection;
                var isNotOpen = false;
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    isNotOpen = true;
                    conn.Open();
                }
                try
                {
                    using (var bulkCopy = copyOptions == SqlBulkCopyOptions.Default ?
                        new SqlBulkCopy(conn) :
                        new SqlBulkCopy(conn, copyOptions, null))
                    {
                        writeToServer(bulkCopy);
                    }
                }
                finally
                {
                    if (isNotOpen)
                        conn.Close();
                }
            }
            else
            {
                throw new NotImplementedException($"ExecuteSqlBulkCopy {CoreErrorStrings.S_Not_Implemented_FeedBack}");
            }
        }
        finally
        {
            dt.Clear();
        }
    }
#if net40
#else
    public static Task<int> ExecuteSqlBulkCopyAsync<T>(this IInsertOrUpdate<T> that, SqlBulkCopyOptions copyOptions = SqlBulkCopyOptions.Default, int? batchSize = null, int? bulkCopyTimeout = null, CancellationToken cancellationToken = default) where T : class
    {
        var upsert = that as InsertOrUpdateProvider<T>;
        if (upsert._source.Any() != true || upsert._tempPrimarys.Any() == false) return Task.FromResult(0);
        var state = ExecuteSqlBulkCopyState(upsert);
        return UpdateProvider.ExecuteBulkUpsertAsync(upsert, state, insert => insert.ExecuteSqlBulkCopyAsync(copyOptions, batchSize, bulkCopyTimeout, cancellationToken));
    }
    public static Task<int> ExecuteSqlBulkCopyAsync<T>(this IUpdate<T> that, SqlBulkCopyOptions copyOptions = SqlBulkCopyOptions.Default, int? batchSize = null, int? bulkCopyTimeout = null, CancellationToken cancellationToken = default) where T : class
    {
        var update = that as UpdateProvider<T>;
        if (update._source.Any() != true || update._tempPrimarys.Any() == false) return Task.FromResult(0);
        var state = ExecuteSqlBulkCopyState(update);
        return UpdateProvider.ExecuteBulkUpdateAsync(update, state, insert => insert.ExecuteSqlBulkCopyAsync(copyOptions, batchSize, bulkCopyTimeout, cancellationToken));
    }
    async public static Task ExecuteSqlBulkCopyAsync<T>(this IInsert<T> that, SqlBulkCopyOptions copyOptions = SqlBulkCopyOptions.Default, int? batchSize = null, int? bulkCopyTimeout = null, CancellationToken cancellationToken = default) where T : class
    {
        var insert = that as FreeSql.SqlServer.Curd.SqlServerInsert<T>;
        if (insert == null) throw new Exception(CoreErrorStrings.S_Features_Unique("ExecuteSqlBulkCopyAsync", "SqlServer"));

        if (insert._insertIdentity) copyOptions = copyOptions | SqlBulkCopyOptions.KeepIdentity;
        var dt = that.ToDataTable();
        if (dt.Rows.Count == 0) return;

        Func<SqlBulkCopy, Task> writeToServerAsync = bulkCopy =>
        {
            if (batchSize.HasValue) bulkCopy.BatchSize = batchSize.Value;
            if (bulkCopyTimeout.HasValue) bulkCopy.BulkCopyTimeout = bulkCopyTimeout.Value;
            bulkCopy.DestinationTableName = dt.TableName;
            for (int i = 0; i < dt.Columns.Count; i++)
                bulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
            return bulkCopy.WriteToServerAsync(dt, cancellationToken);
        };

        try
        {
            if (insert.InternalConnection == null && insert.InternalTransaction == null)
            {
                if (insert._orm.Ado?.TransactionCurrentThread != null)
                    using (var bulkCopy = new SqlBulkCopy(insert._orm.Ado.TransactionCurrentThread.Connection as SqlConnection, copyOptions, insert._orm.Ado.TransactionCurrentThread as SqlTransaction))
                        await writeToServerAsync(bulkCopy);
                else
                    using (var conn = await insert.InternalOrm.Ado.MasterPool.GetAsync())
                    {
                        using (var bulkCopy = copyOptions == SqlBulkCopyOptions.Default ?
                            new SqlBulkCopy(conn.Value as SqlConnection) :
                            new SqlBulkCopy(conn.Value as SqlConnection, copyOptions, null))
                        {
                            await writeToServerAsync(bulkCopy);
                        }
                    }
            }
            else if (insert.InternalTransaction != null)
            {
                using (var bulkCopy = new SqlBulkCopy(insert.InternalTransaction.Connection as SqlConnection, copyOptions, insert.InternalTransaction as SqlTransaction))
                {
                    await writeToServerAsync(bulkCopy);
                }
            }
            else if (insert.InternalConnection != null)
            {
                var conn = insert.InternalConnection as SqlConnection;
                var isNotOpen = false;
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    isNotOpen = true;
                    await conn.OpenAsync(cancellationToken);
                }
                try
                {
                    using (var bulkCopy = copyOptions == SqlBulkCopyOptions.Default ?
                        new SqlBulkCopy(conn) :
                        new SqlBulkCopy(conn, copyOptions, null))
                    {
                        await writeToServerAsync(bulkCopy);
                    }
                }
                finally
                {
                    if (isNotOpen)
                        conn.Close();
                }
            }
            else
            {
                throw new NotImplementedException($"ExecuteSqlBulkCopyAsync {CoreErrorStrings.S_Not_Implemented_FeedBack}");
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
