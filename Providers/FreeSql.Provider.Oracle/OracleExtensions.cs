using FreeSql;
#if oledb
using System.Data.OleDb;
#else
using Oracle.ManagedDataAccess.Client;
#endif
using System;

public static partial class FreeSqlOracleGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatOracle(this string that, params object[] args) => _oracleAdo.Addslashes(that, args);
    static FreeSql.Oracle.OracleAdo _oracleAdo = new FreeSql.Oracle.OracleAdo();

#if oledb
#else
    #region ExecuteOracleBulkCopy
    /// <summary>
    /// Oracle CopyBulk 批量插入功能<para></para>
    /// 使用 IgnoreColumns/InsertColumns 设置忽略/指定导入的列<para></para>
    /// 使用 WithConnection/WithTransaction 传入连接/事务对象<para></para>
    /// 提示：若本方法不能满足，请使用 IInsert&lt;T&gt;.ToDataTable 方法得到 DataTable 对象后，自行处理。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <param name="copyOptions"></param>
    /// <param name="batchSize"></param>
    /// <param name="bulkCopyTimeout"></param>
    public static void ExecuteOracleBulkCopy<T>(this IInsert<T> that, OracleBulkCopyOptions copyOptions = OracleBulkCopyOptions.Default, int? batchSize = null, int? bulkCopyTimeout = null) where T : class
    {
        var insert = that as FreeSql.Oracle.Curd.OracleInsert<T>;
        if (insert == null) throw new Exception(CoreStrings.S_Features_Unique("ExecuteOracleBulkCopy", "Oracle"));

        var dt = that.ToDataTable();
        if (dt.Rows.Count == 0) return;
        
        Action<OracleBulkCopy> writeToServer = bulkCopy =>
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
                if (insert._orm.Ado?.TransactionCurrentThread != null)
                    using (var bulkCopy = new OracleBulkCopy(insert._orm.Ado.TransactionCurrentThread.Connection as OracleConnection, copyOptions))
                        writeToServer(bulkCopy);
                else
                    using (var conn = insert.InternalOrm.Ado.MasterPool.Get())
                    {
                        using (var bulkCopy = copyOptions == OracleBulkCopyOptions.Default ?
                            new OracleBulkCopy(conn.Value as OracleConnection) :
                            new OracleBulkCopy(conn.Value as OracleConnection, copyOptions))
                        {
                            writeToServer(bulkCopy);
                        }
                    }
            }
            else if (insert.InternalTransaction != null)
            {
                using (var bulkCopy = copyOptions == OracleBulkCopyOptions.Default ?
                    new OracleBulkCopy(insert.InternalTransaction.Connection as OracleConnection) :
                    new OracleBulkCopy(insert.InternalTransaction.Connection as OracleConnection, copyOptions))
                {
                    writeToServer(bulkCopy);
                }
            }
            else if (insert.InternalConnection != null)
            {
                var conn = insert.InternalConnection as OracleConnection;
                var isNotOpen = false;
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    isNotOpen = true;
                    conn.Open();
                }
                try
                {
                    using (var bulkCopy = copyOptions == OracleBulkCopyOptions.Default ?
                        new OracleBulkCopy(conn) :
                        new OracleBulkCopy(conn, copyOptions))
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
                throw new NotImplementedException($"ExecuteOracleBulkCopy {CoreStrings.S_Not_Implemented_FeedBack}");
            }
        }
        finally
        {
            dt.Clear();
        }
    }
    #endregion
#endif
}
