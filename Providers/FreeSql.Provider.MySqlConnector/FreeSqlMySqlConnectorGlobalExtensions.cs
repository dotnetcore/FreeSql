using FreeSql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
#if MySqlConnector
using MySqlConnector;
#else
using MySql.Data.MySqlClient;
#endif

public static class FreeSqlMySqlConnectorGlobalExtensions
{
    #region ExecuteMySqlBulkCopy
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
        if (insert == null) throw new Exception("ExecuteMySqlBulkCopy 是 FreeSql.Provider.MySqlConnector 特有的功能");

        var dt = that.ToDataTable();
        if (dt.Rows.Count == 0) return;

        Action<MySqlBulkCopy> writeToServer = bulkCopy =>
        {
            if (bulkCopyTimeout.HasValue) bulkCopy.BulkCopyTimeout = bulkCopyTimeout.Value;
            bulkCopy.DestinationTableName = dt.TableName;
            bulkCopy.WriteToServer(dt);
        };

        try
        {
            if (insert.InternalConnection == null && insert.InternalTransaction == null)
            {
                using (var conn = insert.InternalOrm.Ado.MasterPool.Get())
                {
                    writeToServer(new MySqlBulkCopy(conn.Value as MySqlConnection));
                }
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
                throw new NotImplementedException("ExecuteMySqlBulkCopy 未实现错误，请反馈给作者");
            }
        }
        finally
        {
            dt.Clear();
        }
    }
#if net40
#else
    async public static Task ExecuteMySqlBulkCopyAsync<T>(this IInsert<T> that, int? bulkCopyTimeout = null, CancellationToken cancellationToken = default) where T : class
    {
        var insert = that as FreeSql.MySql.Curd.MySqlInsert<T>;
        if (insert == null) throw new Exception("ExecuteMySqlBulkCopyAsync 是 FreeSql.Provider.MySqlConnector 特有的功能");

        var dt = that.ToDataTable();
        if (dt.Rows.Count == 0) return;

        Func<MySqlBulkCopy, Task> writeToServer = async bulkCopy =>
        {
            if (bulkCopyTimeout.HasValue) bulkCopy.BulkCopyTimeout = bulkCopyTimeout.Value;
            bulkCopy.DestinationTableName = dt.TableName;
            await bulkCopy.WriteToServerAsync(dt, cancellationToken);
        };

        try
        {
            if (insert.InternalConnection == null && insert.InternalTransaction == null)
            {
                using (var conn = insert.InternalOrm.Ado.MasterPool.Get())
                {
                    await writeToServer(new MySqlBulkCopy(conn.Value as MySqlConnection));
                }
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
                throw new NotImplementedException("ExecuteMySqlBulkCopyAsync 未实现错误，请反馈给作者");
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
