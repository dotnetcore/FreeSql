using FreeSql;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
#if oledb
using System.Data.OleDb;
#else
using Oracle.ManagedDataAccess.Client;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    /// 批量插入或更新（操作的字段数量超过 2000 时收益大）<para></para>
    /// 实现原理：使用 OracleBulkCopy 插入临时表，再执行 MERGE INTO t1 using (select * from #temp) ...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <returns></returns>
    public static int ExecuteOracleBulkCopy<T>(this IInsertOrUpdate<T> that) where T : class
    {
        var upsert = that as InsertOrUpdateProvider<T>;
        if (upsert._source.Any() != true || upsert._tempPrimarys.Any() == false) return 0;
        var state = ExecuteOracleBulkCopyState(upsert);
        return UpdateProvider.ExecuteBulkUpsert(upsert, state, insert => insert.ExecuteOracleBulkCopy());
    }
    static NativeTuple<string, string, string, string, string[]> ExecuteOracleBulkCopyState<T>(InsertOrUpdateProvider<T> upsert) where T : class
    {
        if (upsert._source.Any() != true) return null;
        var _table = upsert._table;
        var _commonUtils = upsert._commonUtils;
        var updateTableName = upsert._tableRule?.Invoke(_table.DbName) ?? _table.DbName;
        var tempTableName = $"Temp_{Guid.NewGuid().ToString("N").ToUpper().Substring(0, 24)}";
        if (upsert._orm.CodeFirst.IsSyncStructureToLower) tempTableName = tempTableName.ToLower();
        if (upsert._orm.CodeFirst.IsSyncStructureToUpper) tempTableName = tempTableName.ToUpper();
        if (upsert._connection == null && upsert._orm.Ado.TransactionCurrentThread != null)
            upsert.WithTransaction(upsert._orm.Ado.TransactionCurrentThread);
        var sb = new StringBuilder().Append("CREATE GLOBAL TEMPORARY TABLE ").Append(_commonUtils.QuoteSqlName(tempTableName)).Append(" ( ");
        foreach (var col in _table.Columns.Values)
        {
            sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" ").Append(col.Attribute.DbType.Replace("NOT NULL", ""));
            sb.Append(",");
        }
        var sql1 = sb.Remove(sb.Length - 1, 1).Append("\r\n) ON COMMIT PRESERVE ROWS").ToString();
        sb.Clear();
        try
        {
            upsert._sourceSql = $"select * from {tempTableName}";
            var sql2 = upsert.ToSql();
            var sql3 = $"BEGIN \r\n" +
                $"execute immediate 'TRUNCATE TABLE {_commonUtils.QuoteSqlName(tempTableName)}';\r\n" +
                $"execute immediate 'DROP TABLE {_commonUtils.QuoteSqlName(tempTableName)}';\r\n" +
                $"END;";
            return NativeTuple.Create(sql1, sql2, sql3, tempTableName, _table.Columns.Values.Select(a => a.Attribute.Name).ToArray());
        }
        finally
        {
            upsert._sourceSql = null;
        }
    }
    /// <summary>
    /// 批量更新（更新字段数量超过 2000 时收益大）<para></para>
    /// 实现原理：使用 OracleBulkCopy 插入临时表，再使用 MERGE INTO 联表更新
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <returns></returns>
    public static int ExecuteOracleBulkCopy<T>(this IUpdate<T> that) where T : class
    {
        var update = that as UpdateProvider<T>;
        if (update._source.Any() != true || update._tempPrimarys.Any() == false) return 0;
        var state = ExecuteOracleBulkCopyState(update);
        return UpdateProvider.ExecuteBulkUpdate(update, state, insert => insert.ExecuteOracleBulkCopy());
    }
    static NativeTuple<string, string, string, string, string[]> ExecuteOracleBulkCopyState<T>(UpdateProvider<T> update) where T : class
    {
        if (update._source.Any() != true) return null;
        var _table = update._table;
        var _commonUtils = update._commonUtils;
        var updateTableName = update._tableRule?.Invoke(_table.DbName) ?? _table.DbName;
        var tempTableName = $"Temp_{Guid.NewGuid().ToString("N").ToUpper().Substring(0, 24)}";
        if (update._orm.CodeFirst.IsSyncStructureToLower) tempTableName = tempTableName.ToLower();
        if (update._orm.CodeFirst.IsSyncStructureToUpper) tempTableName = tempTableName.ToUpper();
        if (update._connection == null && update._orm.Ado.TransactionCurrentThread != null)
            update.WithTransaction(update._orm.Ado.TransactionCurrentThread);
        var sb = new StringBuilder().Append("CREATE GLOBAL TEMPORARY TABLE ").Append(_commonUtils.QuoteSqlName(tempTableName)).Append(" ( ");
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
        var sql1 = sb.Remove(sb.Length - 1, 1).Append("\r\n) ON COMMIT PRESERVE ROWS").ToString();

        sb.Clear().Append("MERGE INTO ").Append(_commonUtils.QuoteSqlName(updateTableName)).Append(" a ")
            .Append(" \r\nUSING ").Append(_commonUtils.QuoteSqlName(tempTableName)).Append(" b ON (").Append(string.Join(" AND ", pkColumns.Select(col => $"a.{_commonUtils.QuoteSqlName(col)} = b.{_commonUtils.QuoteSqlName(col)}")))
                    .Append(") \r\nWHEN MATCHED THEN")
                    .Append(" \r\nUPDATE SET ").Append(string.Join(", \r\n  ", setColumns.Select(col => $"{_commonUtils.QuoteSqlName(col)} = b.{_commonUtils.QuoteSqlName(col)}")));
        var sql2 = sb.ToString();
        sb.Clear();
        var sql3 = $"BEGIN \r\n" +
            $"execute immediate 'TRUNCATE TABLE {_commonUtils.QuoteSqlName(tempTableName)}';\r\n" +
            $"execute immediate 'DROP TABLE {_commonUtils.QuoteSqlName(tempTableName)}';\r\n" +
            $"END;";
        return NativeTuple.Create(sql1, sql2, sql3, tempTableName, pkColumns.Concat(setColumns).ToArray());
    }

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
