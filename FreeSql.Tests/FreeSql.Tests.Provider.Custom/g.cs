using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;


public class g
{
    static Lazy<IFreeSql> mysqlLazy = new Lazy<IFreeSql>(() =>
    {
        var fsql = new FreeSql.FreeSqlBuilder()
            .UseConnectionFactory(FreeSql.DataType.CustomMySql, () => new MySqlConnection("Server=127.0.0.1;Persist Security Info=False;UID=root;PWD=root;DATABASE=cccddd_custom;Charset=utf8;SslMode=none;"))
            .UseAutoSyncStructure(true)
            .UseMonitorCommand(
                cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
                (cmd, traceLog) => Console.WriteLine(traceLog))
            .UseLazyLoading(true)
            .Build();
        fsql.SetDbProviderFactory(MySqlConnectorFactory.Instance);
        return fsql;
    });
    public static IFreeSql mysql => mysqlLazy.Value;

    static Lazy<IFreeSql> sqlserverLazy = new Lazy<IFreeSql>(() =>
    {
        var fsql = new FreeSql.FreeSqlBuilder()
            .UseConnectionFactory(FreeSql.DataType.CustomSqlServer, () => new SqlConnection("Server=.;Persist Security Info=False;Trusted_Connection=Yes;Integrated Security=True;DATABASE=freesqlTest_custom;Pooling=true;"))
            .UseAutoSyncStructure(true)
            .UseMonitorCommand(
                cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
                (cmd, traceLog) => Console.WriteLine(traceLog))
            .UseLazyLoading(true)
            .Build();
        fsql.SetDbProviderFactory(SqlClientFactory.Instance);
        return fsql;
    });
    public static IFreeSql sqlserver => sqlserverLazy.Value;

    static Lazy<IFreeSql> oracleLazy = new Lazy<IFreeSql>(() =>
    {
        var fsql = new FreeSql.FreeSqlBuilder()
            .UseConnectionFactory(FreeSql.DataType.CustomOracle, () => new OracleConnection("user id=1custom;password=123456;data source=//127.0.0.1:1521/XE;Pooling=true;Max Pool Size=2"))
            .UseAutoSyncStructure(true)
            .UseLazyLoading(true)
            .UseNameConvert(FreeSql.Internal.NameConvertType.ToUpper)
            //.UseNoneCommandParameter(true)

            .UseMonitorCommand(
                cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
                (cmd, traceLog) => Console.WriteLine(traceLog))
            .Build();
        fsql.SetDbProviderFactory(OracleClientFactory.Instance);
        return fsql;
    });
    public static IFreeSql oracle => oracleLazy.Value;

    static Lazy<IFreeSql> pgsqlLazy = new Lazy<IFreeSql>(() =>
    {
        var fsql = new FreeSql.FreeSqlBuilder()
            .UseConnectionFactory(FreeSql.DataType.CustomPostgreSQL, () => new NpgsqlConnection("Server=192.168.164.10;Port=5432;UID=postgres;PWD=123456;Database=tedb_custom;Pooling=true;"))
            .UseAutoSyncStructure(true)
            .UseNameConvert(FreeSql.Internal.NameConvertType.ToLower)
            .UseLazyLoading(true)
            .UseMonitorCommand(
                cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
                (cmd, traceLog) => Console.WriteLine(traceLog))
            .Build();
        fsql.SetDbProviderFactory(NpgsqlFactory.Instance);
        return fsql;
    });
    public static IFreeSql pgsql => pgsqlLazy.Value;

}
