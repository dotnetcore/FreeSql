using FreeSql;
using System;
using System.Data;
using System.Data.Common;
using System.Text;

public static partial class FreeSqlSqliteGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatSqlite(this string that, params object[] args) => _sqliteAdo.Addslashes(that, args);
    static FreeSql.Sqlite.SqliteAdo _sqliteAdo = new FreeSql.Sqlite.SqliteAdo();

    public static void ExecuteSqliteBulkInsert<T>(this IInsert<T> that) where T : class
    {
        var insert = that as FreeSql.Sqlite.Curd.SqliteInsert<T>;
        if (insert == null) throw new Exception(CoreErrorStrings.S_Features_Unique("ExecuteSqliteBulkInsert", "Sqlite"));

        var dt = that.ToDataTable();
        if (dt.Rows.Count == 0) return;

        Action<DbTransaction> writeToServer = (tran) =>
        {
            var insertCmd = tran.Connection.CreateCommand();
            var copyFromCommand = new StringBuilder().Append("INSERT INTO ").Append(insert._commonUtils.QuoteSqlName(dt.TableName)).Append("(");
            var colIndex = 0;
            foreach (DataColumn col in dt.Columns)
            {
                if (colIndex++ > 0) copyFromCommand.Append(", ");
                copyFromCommand.Append(insert._commonUtils.QuoteSqlName(col.ColumnName));
            }
            copyFromCommand.Append(") VALUES ( ");
            colIndex = 0;
            foreach (DataColumn col in dt.Columns)
            {
                if (colIndex++ > 0) copyFromCommand.Append(", ");
                copyFromCommand.Append("@").Append(col.ColumnName);

                var p = insertCmd.CreateParameter();
                p.ParameterName = col.ColumnName;
                var trycol = insert._table.Columns[col.ColumnName];
                var tp = insert._orm.CodeFirst.GetDbInfo(trycol.Attribute.MapType)?.type;

                insertCmd.Parameters.Add(p);
            }
            copyFromCommand.Append(")");
            insertCmd.CommandText = copyFromCommand.ToString();

            foreach (DataRow r in dt.Rows)
            {
                foreach (DataColumn c in dt.Columns)
                {
                    var p = insertCmd.Parameters[c.ColumnName];
                    p.Value = r[c.ColumnName];
                }
                insertCmd.ExecuteNonQuery();
            }
        };

        try
        {
            if (insert._connection == null && insert._transaction == null)
            {
                if (insert._orm.Ado?.TransactionCurrentThread != null)
                {
                    writeToServer(insert._orm.Ado.TransactionCurrentThread);
                }
                else
                {
                    using (var conn = insert._orm.Ado.MasterPool.Get())
                    {
                        using (var tran = conn.Value.BeginTransaction())
                        {
                            writeToServer(tran);
                            tran.Commit();
                        }
                    }
                }
            }
            else if (insert._transaction != null)
            {
                writeToServer(insert._transaction);
            }
            else if (insert._connection != null)
            {
                var isNotOpen = false;
                if (insert._connection.State != System.Data.ConnectionState.Open)
                {
                    isNotOpen = true;
                    insert._connection.Open();
                }
                try
                {
                    using (var tran = insert._connection.BeginTransaction())
                    {
                        writeToServer(tran);
                        tran.Commit();
                    }
                }
                finally
                {
                    if (isNotOpen)
                    {
                        insert._connection.Close();
                    }
                }
            }
            else
            {
                throw new NotImplementedException($"ExecuteSqliteBulkInsert {CoreErrorStrings.S_Not_Implemented_FeedBack}");
            }
        }
        finally
        {
            dt.Clear();
        }
    }
}
