using FreeSql;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using Kdbndp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

public static partial class FreeSqlKingbaseESGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatKingbaseES(this string that, params object[] args) => _kingbaseesAdo.Addslashes(that, args);
    static FreeSql.KingbaseES.KingbaseESAdo _kingbaseesAdo = new FreeSql.KingbaseES.KingbaseESAdo();

    #region ExecuteKdbCopy
    /// <summary>
    /// 批量插入或更新（操作的字段数量超过 2000 时收益大）<para></para>
    /// 实现原理：使用 Copy 插入临时表，再执行 INSERT INTO t1 select * from #temp ON CONFLICT(""id"") DO UPDATE SET ...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <returns></returns>
    public static int ExecuteKdbCopy<T>(this IInsertOrUpdate<T> that) where T : class
    {
        var upsert = that as InsertOrUpdateProvider<T>;
        if (upsert._source.Any() != true || upsert._tempPrimarys.Any() == false) return 0;
        var state = ExecuteKdbCopyState(upsert);
        return UpdateProvider.ExecuteBulkUpsert(upsert, state, insert => insert.ExecuteKdbCopy());
    }
    static NativeTuple<string, string, string, string, string[]> ExecuteKdbCopyState<T>(InsertOrUpdateProvider<T> upsert) where T : class
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
        var sb = new StringBuilder().Append("CREATE TEMP TABLE ").Append(_commonUtils.QuoteSqlName(tempTableName)).Append(" ( ");
        foreach (var col in _table.Columns.Values)
        {
            sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" ").Append(col.Attribute.DbType.Replace("NOT NULL", ""));
            sb.Append(",");
        }
        var sql1 = sb.Remove(sb.Length - 1, 1).Append("\r\n) WITH (OIDS=FALSE);").ToString();
        sb.Clear();
        try
        {
            upsert._sourceSql = $"select * from {tempTableName}";
            var sql2 = upsert.ToSql();
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
    /// 实现原理：使用 Copy 插入临时表，再使用 UPDATE INNER JOIN 联表更新
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <returns></returns>
    public static int ExecuteKdbCopy<T>(this IUpdate<T> that) where T : class
    {
        var update = that as UpdateProvider<T>;
        if (update._source.Any() != true || update._tempPrimarys.Any() == false) return 0;
        var state = ExecuteKdbCopyState(update);
        return UpdateProvider.ExecuteBulkUpdate(update, state, insert => insert.ExecuteKdbCopy());
    }
    static NativeTuple<string, string, string, string, string[]> ExecuteKdbCopyState<T>(UpdateProvider<T> update) where T : class
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
        var sb = new StringBuilder().Append("CREATE TEMP TABLE ").Append(_commonUtils.QuoteSqlName(tempTableName)).Append(" ( ");
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
        var sql1 = sb.Remove(sb.Length - 1, 1).Append("\r\n) WITH (OIDS=FALSE);").ToString();

        sb.Clear().Append("UPDATE ").Append(_commonUtils.QuoteSqlName(updateTableName)).Append(" a ")
            .Append("\r\nSET \r\n  ").Append(string.Join(", \r\n  ", setColumns.Select(col => $"{_commonUtils.QuoteSqlName(col)} = b.{_commonUtils.QuoteSqlName(col)}")))
            .Append("\r\nFROM ").Append(_commonUtils.QuoteSqlName(tempTableName)).Append(" b ")
            .Append("\r\nWHERE ").Append(string.Join(" AND ", pkColumns.Select(col => $"a.{_commonUtils.QuoteSqlName(col)} = b.{_commonUtils.QuoteSqlName(col)}")));
        var sql2 = sb.ToString();
        sb.Clear();
        var sql3 = $"DROP TABLE {_commonUtils.QuoteSqlName(tempTableName)}";
        return NativeTuple.Create(sql1, sql2, sql3, tempTableName, pkColumns.Concat(setColumns).ToArray());
    }

    /// <summary>
    /// KingbaseES COPY 批量导入功能，封装了 KdbndpConnection.BeginBinaryImport 方法<para></para>
    /// 使用 IgnoreColumns/InsertColumns 设置忽略/指定导入的列<para></para>
    /// 使用 WithConnection/WithTransaction 传入连接/事务对象<para></para>
    /// 提示：若本方法不能满足，请使用 IInsert&lt;T&gt;.ToDataTable 方法得到 DataTable 对象后，自行处理。<para></para>
    /// COPY 与 insert into t values(..),(..),(..) 性能测试参考：<para></para>
    /// 插入180000行，52列：10,090ms 与 46,756ms，10列：4,081ms 与 9,786ms<para></para>
    /// 插入10000行，52列：583ms 与 3,294ms，10列：167ms 与 568ms<para></para>
    /// 插入5000行，52列：337ms 与 2,269ms，10列：93ms 与 366ms<para></para>
    /// 插入2000行，52列：136ms 与 1,019ms，10列：39ms 与 157ms<para></para>
    /// 插入1000行，52列：88ms 与 374ms，10列：21ms 与 102ms<para></para>
    /// 插入500行，52列：61ms 与 209ms，10列：12ms 与 34ms<para></para>
    /// 插入100行，52列：30ms 与 51ms，10列：4ms 与 9ms<para></para>
    /// 插入50行，52列：25ms 与 37ms，10列：2ms 与 6ms<para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    public static void ExecuteKdbCopy<T>(this IInsert<T> that) where T : class
    {
        var insert = that as FreeSql.KingbaseES.KingbaseESInsert<T>;
        if (insert == null) throw new Exception(CoreStrings.S_Features_Unique("ExecuteKdbCopy", "KingbaseES"));

        var dt = that.ToDataTable();
        if (dt.Rows.Count == 0) return;

        Action<KdbndpConnection> binaryImport = conn =>
        {
            var copyFromCommand = new StringBuilder().Append("COPY ").Append(insert.InternalCommonUtils.QuoteSqlName(dt.TableName)).Append("(");
            var colIndex = 0;
            foreach (DataColumn col in dt.Columns)
            {
                if (colIndex++ > 0) copyFromCommand.Append(", ");
                copyFromCommand.Append(insert.InternalCommonUtils.QuoteSqlName(col.ColumnName));
            }
            copyFromCommand.Append(") FROM STDIN BINARY");
            using (var writer = conn.BeginBinaryImport(copyFromCommand.ToString()))
            {
                foreach (DataRow item in dt.Rows)
                    writer.WriteRow(item.ItemArray);
                writer.Complete();
            }
            copyFromCommand.Clear();
        };

        try
        {
            if (insert.InternalConnection == null && insert.InternalTransaction == null)
            {
                using (var conn = insert.InternalOrm.Ado.MasterPool.Get())
                {
                    binaryImport(conn.Value as KdbndpConnection);
                }
            }
            else if (insert.InternalTransaction != null)
            {
                binaryImport(insert.InternalTransaction.Connection as KdbndpConnection);
            }
            else if (insert.InternalConnection != null)
            {
                var conn = insert.InternalConnection as KdbndpConnection;
                var isNotOpen = false;
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    isNotOpen = true;
                    conn.Open();
                }
                try
                {
                    binaryImport(conn);
                }
                finally
                {
                    if (isNotOpen)
                        conn.Close();
                }
            }
            else
            {
                throw new NotImplementedException($"ExecuteKdbCopy {CoreStrings.S_Not_Implemented_FeedBack}");
            }
        }
        finally
        {
            dt.Clear();
        }
    }
    #endregion
}
