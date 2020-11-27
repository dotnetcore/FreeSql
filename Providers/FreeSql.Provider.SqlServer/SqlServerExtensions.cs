using FreeSql;
using FreeSql.Internal.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
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

    /// <summary>
    /// SqlServer with(nolock) 查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <param name="lockType"></param>
    /// <param name="rule">多表查询时的锁规则</param>
    /// <returns></returns>
    public static ISelect<T> WithLock<T>(this ISelect<T> that, SqlServerLock lockType = SqlServerLock.NoLock, Dictionary<Type, bool> rule = null)
        => rule == null ? 
        that.AsAlias((type, old) => $"{old} With({lockType.ToString()})") :
        that.AsAlias((type, old) => rule.TryGetValue(type, out var trybool) && trybool ? $"{old} With({lockType.ToString()})" : old);

    /// <summary>
    /// 设置全局 SqlServer with(nolock) 查询
    /// </summary>
    /// <param name="that"></param>
    /// <param name="options"></param>
    public static IFreeSql SetGlobalSelectWithLock(this IFreeSql that, SqlServerLock lockType, Dictionary<Type, bool> rule)
    {
        var value = NativeTuple.Create(lockType, rule);
        _dicSetGlobalSelectWithLock.AddOrUpdate(that.Ado.Identifier, value, (_, __) => value);
        return that;
    }
    internal static ConcurrentDictionary<Guid, NativeTuple<SqlServerLock, Dictionary<Type, bool>>> _dicSetGlobalSelectWithLock = new ConcurrentDictionary<Guid, NativeTuple<SqlServerLock, Dictionary<Type, bool>>>();

    #region ExecuteSqlBulkCopy
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
        if (insert == null) throw new Exception("ExecuteSqlBulkCopy 是 FreeSql.Provider.SqlServer 特有的功能");

        var dt = that.ToDataTable();
        if (dt.Rows.Count == 0) return;

        Action<SqlBulkCopy> writeToServer = bulkCopy =>
        {
            if (batchSize.HasValue) bulkCopy.BatchSize = batchSize.Value;
            if (bulkCopyTimeout.HasValue) bulkCopy.BulkCopyTimeout = bulkCopyTimeout.Value;
            bulkCopy.DestinationTableName = dt.TableName;
            for (int i = 0; i < dt.Columns.Count; i++)
                bulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
            bulkCopy.WriteToServer(dt);
        };

        try
        {
            if (insert.InternalConnection == null && insert.InternalTransaction == null)
            {
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
                throw new NotImplementedException("ExecuteSqlBulkCopy 未实现错误，请反馈给作者");
            }
        }
        finally
        {
            dt.Clear();
        }
    }
#if net40
#else
    async public static Task ExecuteSqlBulkCopyAsync<T>(this IInsert<T> that, SqlBulkCopyOptions copyOptions = SqlBulkCopyOptions.Default, int? batchSize = null, int? bulkCopyTimeout = null, CancellationToken cancellationToken = default) where T : class
    {
        var insert = that as FreeSql.SqlServer.Curd.SqlServerInsert<T>;
        if (insert == null) throw new Exception("ExecuteSqlBulkCopyAsync 是 FreeSql.Provider.SqlServer 特有的功能");

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
                throw new NotImplementedException("ExecuteSqlBulkCopyAsync 未实现错误，请反馈给作者");
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

[Flags]
public enum SqlServerLock
{
    NoLock = 1,
    HoldLock = 2,
    UpdLock = 4,
    RowLock = 8,
    ReadCommitted = 16,
    ReadPast = 32,
    ReadUnCommitted = 64,
    RepeaTableRead = 256,
    PagLock = 512,
    Serializable = 1024,
    TabLock = 2048,
    TabLockX = 4096,
    XLock = 8192,
    NoWait = 16384
}