using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


public class g
{
    static Lazy<IFreeSql> mysqlLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.OdbcMySql, "Driver={MySQL ODBC 8.0 Unicode Driver};Server=127.0.0.1;Persist Security Info=False;Trusted_Connection=Yes;UID=root;PWD=root;DATABASE=cccddd_odbc;Charset=utf8;SslMode=none;Max pool size=2")
        .UseAutoSyncStructure(true)
        .UseMonitorCommand(
            cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
            (cmd, traceLog) => Console.WriteLine(traceLog))
        .UseLazyLoading(true)
        .Build());
    public static IFreeSql mysql => mysqlLazy.Value;

    static Lazy<IFreeSql> sqlserverLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.OdbcSqlServer, "Driver={SQL Server};Server=.;Persist Security Info=False;Trusted_Connection=Yes;Integrated Security=True;DATABASE=freesqlTest_odbc;Pooling=true;Max pool size=3")
        .UseAutoSyncStructure(true)
        .UseMonitorCommand(
            cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
            (cmd, traceLog) => Console.WriteLine(traceLog))
        .UseLazyLoading(true)
        .Build());
    public static IFreeSql sqlserver => sqlserverLazy.Value;

    static Lazy<IFreeSql> oracleLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.OdbcOracle, "Driver={Oracle in XE};Server=//127.0.0.1:1521/XE;Persist Security Info=False;Trusted_Connection=Yes;UID=odbc1;PWD=123456")
        .UseAutoSyncStructure(true)
        .UseLazyLoading(true)
        .UseSyncStructureToUpper(true)
        //.UseNoneCommandParameter(true)

        .UseMonitorCommand(
            cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
            (cmd, traceLog) => Console.WriteLine(traceLog))
        .Build());
    public static IFreeSql oracle => oracleLazy.Value;

    static Lazy<IFreeSql> pgsqlLazy = new Lazy<IFreeSql>(() =>
    {
        return new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.OdbcPostgreSQL, "Driver={PostgreSQL Unicode(x64)};Server=192.168.164.10;Port=5432;UID=postgres;PWD=123456;Database=tedb_odbc;Pooling=true;Maximum Pool Size=2")
        .UseAutoSyncStructure(true)
        .UseSyncStructureToLower(true)
        .UseLazyLoading(true)
        .UseMonitorCommand(
            cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
            (cmd, traceLog) => Console.WriteLine(traceLog))
        .Build();
    });
    public static IFreeSql pgsql => pgsqlLazy.Value;

    static Lazy<IFreeSql> odbcLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.Odbc, "Driver={SQL Server};Server=.;Persist Security Info=False;Trusted_Connection=Yes;Integrated Security=True;DATABASE=freesqlTest_odbc;Pooling=true;Max pool size=5")
        .UseMonitorCommand(
            cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
            (cmd, traceLog) => Console.WriteLine(traceLog))
        .UseLazyLoading(true)
        .Build());
    public static IFreeSql odbc => odbcLazy.Value;
}
