using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


public class g
{
    static Lazy<IFreeSql> mysqlLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.OdbcMySql, "Driver={MySQL ODBC 8.0 Unicode Driver};Server=127.0.0.1;Persist Security Info=False;Trusted_Connection=Yes;UID=root;PWD=root;DATABASE=cccddd_odbc;Charset=utf8;SslMode=none;Max pool size=2")
        //.UseConnectionFactory(FreeSql.DataType.OdbcMySql, () => new System.Data.Odbc.OdbcConnection("Driver={MySQL ODBC 8.0 Unicode Driver};Server=127.0.0.1;Persist Security Info=False;Trusted_Connection=Yes;UID=root;PWD=root;DATABASE=cccddd_odbc;Charset=utf8;SslMode=none;"))
        .UseAutoSyncStructure(true)
        .UseMonitorCommand(
            cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
            (cmd, traceLog) => Console.WriteLine(traceLog))
        .UseLazyLoading(true)
        .Build());
    public static IFreeSql mysql => mysqlLazy.Value;

    static Lazy<IFreeSql> sqlserverLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.OdbcSqlServer, "Driver={SQL Server};Server=.;Persist Security Info=False;Trusted_Connection=Yes;Integrated Security=True;DATABASE=freesqlTest_odbc;Pooling=true;Max pool size=3")
        //.UseConnectionFactory(FreeSql.DataType.OdbcSqlServer, () => new System.Data.Odbc.OdbcConnection("Driver={SQL Server};Server=.;Persist Security Info=False;Trusted_Connection=Yes;Integrated Security=True;DATABASE=freesqlTest_odbc;Pooling=true;"))
        //.UseConnectionString(FreeSql.DataType.OdbcSqlServer, "Driver={SQL Server};Server=192.168.164.129;Persist Security Info=False;Trusted_Connection=Yes;UID=sa;PWD=123456;DATABASE=ds_shop;")
        //.UseConnectionFactory(FreeSql.DataType.OdbcSqlServer, () => new System.Data.Odbc.OdbcConnection("Driver={SQL Server};Server=192.168.164.129;Persist Security Info=False;Trusted_Connection=Yes;UID=sa;PWD=123456;DATABASE=ds_shop;"))
        .UseAutoSyncStructure(true)
        .UseMonitorCommand(
            cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
            (cmd, traceLog) => Console.WriteLine(traceLog))
        .UseLazyLoading(true)
        .Build());
    public static IFreeSql sqlserver => sqlserverLazy.Value;

    static Lazy<IFreeSql> oracleLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.OdbcOracle, "Driver={Oracle in XE};Server=//127.0.0.1:1521/XE;Persist Security Info=False;Trusted_Connection=Yes;UID=odbc1;PWD=123456")
        //.UseConnectionFactory(FreeSql.DataType.OdbcOracle, () => new System.Data.Odbc.OdbcConnection("Driver={Oracle in XE};Server=//127.0.0.1:1521/XE;Persist Security Info=False;Trusted_Connection=Yes;UID=odbc1;PWD=123456"))
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
        //.UseConnectionFactory(FreeSql.DataType.OdbcPostgreSQL, () => new System.Data.Odbc.OdbcConnection("Driver={PostgreSQL Unicode(x64)};Server=192.168.164.10;Port=5432;UID=postgres;PWD=123456;Database=tedb_odbc;Pooling=true;"))
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
        //.UseConnectionFactory(FreeSql.DataType.Odbc, () => new System.Data.Odbc.OdbcConnection("Driver={SQL Server};Server=.;Persist Security Info=False;Trusted_Connection=Yes;Integrated Security=True;DATABASE=freesqlTest_odbc;Pooling=true;"))
        .UseMonitorCommand(
            cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
            (cmd, traceLog) => Console.WriteLine(traceLog))
        .UseLazyLoading(true)
        .Build());
    public static IFreeSql odbc => odbcLazy.Value;

    static Lazy<IFreeSql> damemgLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.OdbcDameng, "Driver={DM8 ODBC DRIVER};Server=127.0.0.1:5236;Persist Security Info=False;Trusted_Connection=Yes;UID=USER1;PWD=123456789")
        //.UseConnectionFactory(FreeSql.DataType.OdbcDameng, () => new System.Data.Odbc.OdbcConnection("Driver={DM8 ODBC DRIVER};Server=127.0.0.1:5236;Persist Security Info=False;Trusted_Connection=Yes;UID=USER1;PWD=123456789"))
        .UseAutoSyncStructure(true)
        .UseLazyLoading(true)
        .UseSyncStructureToUpper(true)
        //.UseNoneCommandParameter(true)

        .UseMonitorCommand(
            cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
            (cmd, traceLog) => Console.WriteLine(traceLog))
        .Build());
    public static IFreeSql dameng => damemgLazy.Value;

    //启动南大通用数据库 oninit -vy
    static Lazy<IFreeSql> gbaseLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.OdbcDameng, "Driver={GBase ODBC DRIVER (64-bit)};Server=192.168.164.10:5236;Persist Security Info=False;Trusted_Connection=Yes;UID=USER1;PWD=123456789")
        //.UseConnectionFactory(FreeSql.DataType.OdbcDameng, () => new System.Data.Odbc.OdbcConnection("Driver={DM8 ODBC DRIVER};Server=127.0.0.1:5236;Persist Security Info=False;Trusted_Connection=Yes;UID=USER1;PWD=123456789"))
        .UseAutoSyncStructure(true)
        .UseLazyLoading(true)
        .UseSyncStructureToUpper(true)
        //.UseNoneCommandParameter(true)

        .UseMonitorCommand(
            cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
            (cmd, traceLog) => Console.WriteLine(traceLog))
        .Build());
    public static IFreeSql gbase => gbaseLazy.Value;


    //启动神州通用数据库 /etc/init.d/oscardb_OSRDBd start
    //SYSDBA 密码 szoscar55


}
