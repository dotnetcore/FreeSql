using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


public class g
{
    static Lazy<IFreeSql> mysqlLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.OdbcMySql, "Driver={MySQL ODBC 8.0 Unicode Driver};Server=127.0.0.1;Persist Security Info=False;Trusted_Connection=Yes;UID=root;PWD=root;DATABASE=cccddd_odbc;Charset=utf8;SslMode=none;Max pool size=2")
        //.UseConnectionFactory(FreeSql.DataType.OdbcMySql, () => new System.Data.Odbc.OdbcConnection("Driver={MySQL ODBC 8.0 Unicode Driver};Server=127.0.0.1;Persist Security Info=False;Trusted_Connection=Yes;UID=root;PWD=root;DATABASE=cccddd_odbc;Charset=utf8;SslMode=none;"))
        //.UseConnectionString(FreeSql.DataType.OdbcMySql, "Driver={MySQL ODBC 8.0 Unicode Driver};Server=192.168.164.10;port=33061;Persist Security Info=False;Trusted_Connection=Yes;UID=root;PWD=root;DATABASE=cccddd_odbc;Charset=utf8;SslMode=none;Max pool size=2")
        .UseAutoSyncStructure(true)
        .UseMonitorCommand(
            cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
            (cmd, traceLog) => Console.WriteLine(traceLog))
        .UseLazyLoading(true)
        .Build());
    public static IFreeSql mysql => mysqlLazy.Value;

    static Lazy<IFreeSql> sqlserverLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.OdbcSqlServer, "Driver={SQL Server};Server=.;Persist Security Info=False;Trusted_Connection=Yes;Integrated Security=True;DATABASE=issues684_odbc;Pooling=true;Max pool size=3")
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
        .UseConnectionString(FreeSql.DataType.OdbcOracle, "Driver={Oracle in XE};Server=//127.0.0.1:1521/XE;Persist Security Info=False;Trusted_Connection=Yes;UID=1odbc;PWD=123456")
        //.UseConnectionFactory(FreeSql.DataType.OdbcOracle, () => new System.Data.Odbc.OdbcConnection("Driver={Oracle in XE};Server=//127.0.0.1:1521/XE;Persist Security Info=False;Trusted_Connection=Yes;UID=1odbc;PWD=123456"))
        .UseAutoSyncStructure(true)
        .UseLazyLoading(true)
        .UseNameConvert(FreeSql.Internal.NameConvertType.ToUpper)
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
        .UseNameConvert(FreeSql.Internal.NameConvertType.ToLower)
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
        .UseConnectionString(FreeSql.DataType.OdbcDameng, "Driver={DM8 ODBC DRIVER};Server=127.0.0.1:5236;Persist Security Info=False;Trusted_Connection=Yes;UID=1user;PWD=123456789")
        //.UseConnectionFactory(FreeSql.DataType.OdbcDameng, () => new System.Data.Odbc.OdbcConnection("Driver={DM8 ODBC DRIVER};Server=127.0.0.1:5236;Persist Security Info=False;Trusted_Connection=Yes;UID=1user;PWD=123456789"))
        .UseAutoSyncStructure(true)
        .UseLazyLoading(true)
        .UseNameConvert(FreeSql.Internal.NameConvertType.ToUpper)
        //.UseNoneCommandParameter(true)

        .UseMonitorCommand(
            cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
            (cmd, traceLog) => Console.WriteLine(traceLog))
        .Build());
    public static IFreeSql dameng => damemgLazy.Value;

    static Lazy<IFreeSql> kingbaseESLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.OdbcKingbaseES, "Driver={KingbaseES 8.2 ODBC Driver ANSI};Server=127.0.0.1;Port=54321;UID=USER2;PWD=123456789;database=TEST")
        //.UseConnectionFactory(FreeSql.DataType.OdbcKingbaseES, () => new System.Data.Odbc.OdbcConnection("Driver={KingbaseES 8.2 ODBC Driver ANSI};Server=127.0.0.1;Port=54321;UID=USER2;PWD=123456789;database=TEST"))
        .UseAutoSyncStructure(true)
        .UseLazyLoading(true)
        .UseNameConvert(FreeSql.Internal.NameConvertType.ToUpper)
        //.UseNoneCommandParameter(true)

        .UseMonitorCommand(
            cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
            (cmd, traceLog) => Console.WriteLine(traceLog))
        .Build());
    public static IFreeSql kingbaseES => kingbaseESLazy.Value;


    //启动神舟通用数据库 /etc/init.d/oscardb_OSRDBd start
    //SYSDBA 密码 szoscar55


}
