using FreeSql;
using FreeSql.PostgreSQL.Curd;
using Npgsql;
using System;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public static partial class FreeSqlPostgreSQLGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatPostgreSQL(this string that, params object[] args) => _postgresqlAdo.Addslashes(that, args);
    static FreeSql.PostgreSQL.PostgreSQLAdo _postgresqlAdo = new FreeSql.PostgreSQL.PostgreSQLAdo();

    /// <summary>
    /// PostgreSQL9.5+ 特有的功能，On Conflict Do Update<para></para>
    /// 注意：此功能会开启插入【自增列】
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="that"></param>
    /// <param name="columns">默认是以主键作为重复判断，也可以指定其他列：a => a.Name | a => new{a.Name,a.Time} | a => new[]{"name","time"}</param>
    /// <returns></returns>
    public static OnConflictDoUpdate<T1> OnConflictDoUpdate<T1>(this IInsert<T1> that, Expression<Func<T1, object>> columns = null) where T1 : class => new FreeSql.PostgreSQL.Curd.OnConflictDoUpdate<T1>(that.InsertIdentity(), columns);

    #region ExecutePgCopy
    /// <summary>
    /// PostgreSQL COPY 批量导入功能，封装了 NpgsqlConnection.BeginBinaryImport 方法<para></para>
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
    public static void ExecutePgCopy<T>(this IInsert<T> that) where T : class
    {
        var insert = that as FreeSql.PostgreSQL.Curd.PostgreSQLInsert<T>;
        if (insert == null) throw new Exception("ExecutePgCopy 是 FreeSql.Provider.PostgreSQL 特有的功能");

        var dt = that.ToDataTable();
        if (dt.Rows.Count == 0) return;

        Action<NpgsqlConnection> binaryImport = conn =>
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
                    binaryImport(conn.Value as NpgsqlConnection);
                }
            }
            else if (insert.InternalTransaction != null)
            {
                binaryImport(insert.InternalTransaction.Connection as NpgsqlConnection);
            }
            else if (insert.InternalConnection != null)
            {
                var conn = insert.InternalConnection as NpgsqlConnection;
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
                throw new NotImplementedException("ExecutePgCopy 未实现错误，请反馈给作者");
            }
        }
        finally
        {
            dt.Clear();
        }
    }

#if net45
#else
    async public static Task ExecutePgCopyAsync<T>(this IInsert<T> that, CancellationToken cancellationToken = default) where T : class
    {
        var insert = that as FreeSql.PostgreSQL.Curd.PostgreSQLInsert<T>;
        if (insert == null) throw new Exception("ExecutePgCopyAsync 是 FreeSql.Provider.PostgreSQL 特有的功能");

        var dt = that.ToDataTable();
        if (dt.Rows.Count == 0) return;
        Func<NpgsqlConnection, Task> binaryImportAsync = async conn =>
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
                    await writer.WriteRowAsync(cancellationToken, item.ItemArray);
                writer.Complete();
            }
            copyFromCommand.Clear();
        };

        try
        {
            if (insert.InternalConnection == null && insert.InternalTransaction == null)
            {
                using (var conn = await insert.InternalOrm.Ado.MasterPool.GetAsync())
                {
                    await binaryImportAsync(conn.Value as NpgsqlConnection);
                }
            }
            else if (insert.InternalTransaction != null)
            {
                await binaryImportAsync(insert.InternalTransaction.Connection as NpgsqlConnection);
            }
            else if (insert.InternalConnection != null)
            {
                var conn = insert.InternalConnection as NpgsqlConnection;
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
                throw new NotImplementedException("ExecutePgCopyAsync 未实现错误，请反馈给作者");
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
