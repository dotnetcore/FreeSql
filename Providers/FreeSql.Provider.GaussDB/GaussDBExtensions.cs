using FreeSql;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.GaussDB;
using FreeSql.GaussDB.Curd;
using HuaweiCloud.GaussDB;
using HuaweiCloud.GaussDBTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public static partial class FreeSqlGaussDBGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatGaussDB(this string that, params object[] args) => _gaussdbAdo.Addslashes(that, args);
    static FreeSql.GaussDB.GaussDBAdo _gaussdbAdo = new FreeSql.GaussDB.GaussDBAdo();

    /// <summary>
    /// GaussDB9.5+ 特有的功能，On Conflict Do Update<para></para>
    /// 注意：此功能会开启插入【自增列】
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="that"></param>
    /// <param name="columns">默认是以主键作为重复判断，也可以指定其他列：a => a.Name | a => new{a.Name,a.Time} | a => new[]{"name","time"}</param>
    /// <returns></returns>
    public static OnConflictDoUpdate<T1> OnConflictDoUpdate<T1>(this IInsert<T1> that, Expression<Func<T1, object>> columns = null) where T1 : class => new FreeSql.GaussDB.Curd.OnConflictDoUpdate<T1>(that.InsertIdentity(), columns);

    /// <summary>
    /// GaussDB<para></para>
    /// select distinct on(subject) * from score order by subject, score desc, name;
    /// </summary>
    public static ISelect<T1> DistinctOn<T1>(this ISelect<T1> query, Expression<Func<T1, object>> selector)
    {
        var select = query as FreeSql.GaussDB.Curd.GaussDBSelect<T1>;
        if (select == null) throw new Exception($"{nameof(DistinctOn)} 是 FreeSql.Provider.GaussDB 特有的功能");
        var s0p = select as Select0Provider;
        var orderByOld = s0p._orderby;
        var orderByNew = "";
        try
        {
            s0p._orderby = null;
            select.OrderBy(selector);
            orderByNew = s0p._orderby;
        }
        finally
        {
            s0p._orderby = orderByOld;
        }
        if (orderByNew.StartsWith(" \r\nORDER BY "))
            s0p._select = $"{s0p._select} distinct on({orderByNew.Substring(12)}) ";
        return select;
    }

    #region ExecuteGaussDBCopy
    /// <summary>
    /// 批量插入或更新（操作的字段数量超过 2000 时收益大）<para></para>
    /// 实现原理：使用 GaussDBCopy 插入临时表，再执行 INSERT INTO t1 select * from #temp ON CONFLICT(""id"") DO UPDATE SET ...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <returns></returns>
    public static int ExecuteGaussDBCopy<T>(this IInsertOrUpdate<T> that, int? timeout = null) where T : class
    {
        var upsert = that as InsertOrUpdateProvider<T>;
        if (upsert._source.Any() != true || upsert._tempPrimarys.Any() == false) return 0;
        var state = ExecuteGaussDBCopyState(upsert);
        return UpdateProvider.ExecuteBulkUpsert(upsert, state, insert => insert.ExecuteGaussDBCopy(timeout));
    }
    static NativeTuple<string, string, string, string, string[]> ExecuteGaussDBCopyState<T>(InsertOrUpdateProvider<T> upsert) where T : class
    {
        if (upsert._source.Any() != true) return null;
        var _table = upsert._table;
        var _commonUtils = upsert._commonUtils;
        var updateTableName = upsert._tableRule?.Invoke(_table.DbName) ?? _table.DbName;
        var tempTableName = $"temp_{Guid.NewGuid().ToString("N")}";
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
            if ((upsert._orm as IGaussDBProviderOptions)?.UseMergeInto == true) //#2170
            {
                upsert._sourceSql = $"select * from {tempTableName}";
                var sql2 = upsert.ToSql();
                var sql3 = $"DROP TABLE {tempTableName}";
                return NativeTuple.Create(sql1, sql2, sql3, tempTableName, _table.Columns.Values.Select(a => a.Attribute.Name).ToArray());
            }
            else
            {
                upsert._sourceSql = $"select __**__ from {tempTableName}";
                var sql2 = upsert.ToSql();
                if (string.IsNullOrWhiteSpace(sql2) == false)
                {
                    var field = sql2.Substring(sql2.IndexOf("\"(") + 2);
                    field = field.Remove(field.IndexOf(upsert._sourceSql)).TrimEnd().TrimEnd(')');
                    sql2 = sql2.Replace(upsert._sourceSql, $"select {field} from {tempTableName}");
                }
                var sql3 = $"DROP TABLE {_commonUtils.QuoteSqlName(tempTableName)}";
                return NativeTuple.Create(sql1, sql2, sql3, tempTableName, _table.Columns.Values.Select(a => a.Attribute.Name).ToArray());
            }
        }
        finally
        {
            upsert._sourceSql = null;
        }
    }
    /// <summary>
    /// 批量更新（更新字段数量超过 2000 时收益大）<para></para>
    /// 实现原理：使用 GaussDBCopy 插入临时表，再使用 UPDATE INNER JOIN 联表更新
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <returns></returns>
    public static int ExecuteGaussDBCopy<T>(this IUpdate<T> that, int? timeout = null) where T : class
    {
        var update = that as UpdateProvider<T>;
        if (update._source.Any() != true || update._tempPrimarys.Any() == false) return 0;
        var state = ExecuteGaussDBCopyState(update);
        return UpdateProvider.ExecuteBulkUpdate(update, state, insert => insert.ExecuteGaussDBCopy(timeout));
    }
    static NativeTuple<string, string, string, string, string[]> ExecuteGaussDBCopyState<T>(UpdateProvider<T> update) where T : class
    {
        if (update._source.Any() != true) return null;
        var _table = update._table;
        var _commonUtils = update._commonUtils;
        var updateTableName = update._tableRule?.Invoke(_table.DbName) ?? _table.DbName;
        var tempTableName = $"temp_{Guid.NewGuid().ToString("N")}";
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
    /// GaussDB COPY 批量导入功能，封装了 GaussDBConnection.BeginBinaryImport 方法<para></para>
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
    /// <param name="timeout">超时时间，单位秒，Framework不支持</param>
    public static void ExecuteGaussDBCopy<T>(this IInsert<T> that, int? timeout = null) where T : class
    {
        var insert = that as FreeSql.GaussDB.Curd.GaussDBInsert<T>;
        if (insert == null) throw new Exception(CoreErrorStrings.S_Features_Unique("ExecuteGaussDBCopy", "GaussDB"));

        var dt = that.ToDataTable();
        if (dt.Rows.Count == 0) return;

        Action<GaussDBConnection> binaryImport = conn =>
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
#if !NETFRAMEWORK
                if (timeout.HasValue)
                {
                    writer.Timeout = TimeSpan.FromSeconds(timeout.Value);
                }
#endif
                foreach (DataRow item in dt.Rows)
                {
                    writer.StartRow();
                    foreach (DataColumn col in dt.Columns)
                    {
                        GaussDBDbType? gaussdbDbType = null;
                        var trycol = insert._table.Columns[col.ColumnName];
                        var tp = insert._orm.CodeFirst.GetDbInfo(trycol.Attribute.MapType)?.type;
                        if (tp != null) gaussdbDbType = (GaussDBDbType)tp.Value;
                        if (gaussdbDbType == null || gaussdbDbType == GaussDBDbType.Unknown)
                            writer.Write(item[col.ColumnName]);
                        else if (gaussdbDbType == GaussDBDbType.Date && item[col.ColumnName] != null && (item[col.ColumnName] is DateTime || item[col.ColumnName] is DateTime?))
                            writer.Write(((DateTime)item[col.ColumnName]).ToString("yyyy-MM-dd"), gaussdbDbType.Value);
                        else
                            writer.Write(item[col.ColumnName], gaussdbDbType.Value);
                    }
                    //writer.WriteRow(item.ItemArray); #1532
                }
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
                    binaryImport(conn.Value as GaussDBConnection);
                }
            }
            else if (insert.InternalTransaction != null)
            {
                binaryImport(insert.InternalTransaction.Connection as GaussDBConnection);
            }
            else if (insert.InternalConnection != null)
            {
                var conn = insert.InternalConnection as GaussDBConnection;
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
                throw new NotImplementedException($"ExecuteGaussDBCopy {CoreErrorStrings.S_Not_Implemented_FeedBack}");
            }
        }
        finally
        {
            dt.Clear();
        }
    }

#if net45
#else
    public static Task<int> ExecuteGaussDBCopyAsync<T>(this IInsertOrUpdate<T> that, CancellationToken cancellationToken = default) where T : class
    {
        var upsert = that as InsertOrUpdateProvider<T>;
        if (upsert._source.Any() != true || upsert._tempPrimarys.Any() == false) return Task.FromResult(0);
        var state = ExecuteGaussDBCopyState(upsert);
        return UpdateProvider.ExecuteBulkUpsertAsync(upsert, state, insert => insert.ExecuteGaussDBCopyAsync(cancellationToken));
    }
    public static Task<int> ExecuteGaussDBCopyAsync<T>(this IUpdate<T> that, CancellationToken cancellationToken = default) where T : class
    {
        var update = that as UpdateProvider<T>;
        if (update._source.Any() != true || update._tempPrimarys.Any() == false) return Task.FromResult(0);
        var state = ExecuteGaussDBCopyState(update);
        return UpdateProvider.ExecuteBulkUpdateAsync(update, state, insert => insert.ExecuteGaussDBCopyAsync(cancellationToken));
    }
    async public static Task ExecuteGaussDBCopyAsync<T>(this IInsert<T> that, CancellationToken cancellationToken = default) where T : class
    {
        var insert = that as FreeSql.GaussDB.Curd.GaussDBInsert<T>;
        if (insert == null) throw new Exception(CoreErrorStrings.S_Features_Unique("ExecuteGaussDBCopyAsync", "GaussDB"));

        var dt = that.ToDataTable();
        if (dt.Rows.Count == 0) return;
        Func<GaussDBConnection, Task> binaryImportAsync = async conn =>
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
                {
                    await writer.StartRowAsync(cancellationToken);
                    foreach (DataColumn col in dt.Columns)
                    {
                        GaussDBDbType? gaussdbDbType = null;
                        var trycol = insert._table.Columns[col.ColumnName];
                        var tp = insert._orm.CodeFirst.GetDbInfo(trycol.Attribute.MapType)?.type;
                        if (tp != null) gaussdbDbType = (GaussDBDbType)tp.Value;
                        if (gaussdbDbType == null || gaussdbDbType == GaussDBDbType.Unknown)
                            await writer.WriteAsync(item[col.ColumnName], cancellationToken);
                        else if (gaussdbDbType == GaussDBDbType.Date && item[col.ColumnName] != null && (item[col.ColumnName] is DateTime || item[col.ColumnName] is DateTime?))
                            await writer.WriteAsync(((DateTime)item[col.ColumnName]).ToString("yyyy-MM-dd"), gaussdbDbType.Value, cancellationToken);
                        else
                            await writer.WriteAsync(item[col.ColumnName], gaussdbDbType.Value, cancellationToken);
                    }
                    //await writer.WriteRowAsync(cancellationToken, item.ItemArray); #1532
                }
                await writer.CompleteAsync(cancellationToken);
            }
            copyFromCommand.Clear();
        };

        try
        {
            if (insert.InternalConnection == null && insert.InternalTransaction == null)
            {
                using (var conn = await insert.InternalOrm.Ado.MasterPool.GetAsync())
                {
                    await binaryImportAsync(conn.Value as GaussDBConnection);
                }
            }
            else if (insert.InternalTransaction != null)
            {
                await binaryImportAsync(insert.InternalTransaction.Connection as GaussDBConnection);
            }
            else if (insert.InternalConnection != null)
            {
                var conn = insert.InternalConnection as GaussDBConnection;
                var isNotOpen = false;
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    isNotOpen = true;
                    await conn.OpenAsync(cancellationToken);
                }
                try
                {
                    await binaryImportAsync(conn);
                }
                finally
                {
                    if (isNotOpen)
                        await conn.CloseAsync();
                }
            }
            else
            {
                throw new NotImplementedException($"ExecuteGaussDBCopyAsync {CoreErrorStrings.S_Not_Implemented_FeedBack}");
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
