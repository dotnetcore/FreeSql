using Dm;
using FreeSql;
using System;

public static partial class FreeSqlDamengGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatDameng(this string that, params object[] args) => _damengAdo.Addslashes(that, args);
    static FreeSql.Dameng.DamengAdo _damengAdo = new FreeSql.Dameng.DamengAdo();

    #region ExecuteDmBulkCopy
    /// <summary>
    /// 达梦 CopyBulk 批量插入功能<para></para>
    /// 使用 IgnoreColumns/InsertColumns 设置忽略/指定导入的列<para></para>
    /// 使用 WithConnection/WithTransaction 传入连接/事务对象<para></para>
    /// 提示：若本方法不能满足，请使用 IInsert&lt;T&gt;.ToDataTable 方法得到 DataTable 对象后，自行处理。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <param name="copyOptions"></param>
    /// <param name="batchSize"></param>
    /// <param name="bulkCopyTimeout"></param>
    public static void ExecuteDmBulkCopy<T>(this IInsert<T> that, DmBulkCopyOptions copyOptions = DmBulkCopyOptions.Default, int? batchSize = null, int? bulkCopyTimeout = null) where T : class
    {
        var insert = that as FreeSql.Dameng.Curd.DamengInsert<T>;
        if (insert == null) throw new Exception("ExecuteDmBulkCopy 是 FreeSql.Provider.Dameng 特有的功能");

        var dt = that.ToDataTable();
        if (dt.Rows.Count == 0) return;

        Action<DmBulkCopy> writeToServer = bulkCopy =>
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
                    using (var bulkCopy = copyOptions == DmBulkCopyOptions.Default ?
                        new DmBulkCopy(conn.Value as DmConnection) :
                        new DmBulkCopy(conn.Value as DmConnection, copyOptions, insert.InternalTransaction as DmTransaction))
                    {
                        writeToServer(bulkCopy);
                    }
                }
            }
            else if (insert.InternalTransaction != null)
            {
                using (var bulkCopy = new DmBulkCopy(insert.InternalTransaction.Connection as DmConnection, copyOptions, insert.InternalTransaction as DmTransaction))
                {
                    writeToServer(bulkCopy);
                }
            }
            else if (insert.InternalConnection != null)
            {
                var conn = insert.InternalConnection as DmConnection;
                var isNotOpen = false;
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    isNotOpen = true;
                    conn.Open();
                }
                try
                {
                    using (var bulkCopy = copyOptions == DmBulkCopyOptions.Default ?
                        new DmBulkCopy(conn) :
                        new DmBulkCopy(conn, copyOptions, null))
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
                throw new NotImplementedException("ExecuteDmBulkCopy 未实现错误，请反馈给作者");
            }
        }
        finally
        {
            dt.Clear();
        }
    }
    #endregion
}
