using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


public class g
{

    static Lazy<IFreeSql> mysqlLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=2")
        .UseAutoSyncStructure(true)
        .UseMonitorCommand(
            cmd =>
            {
                Trace.WriteLine(cmd.CommandText);
            }, //监听SQL命令对象，在执行前
            (cmd, traceLog) =>
            {
                Console.WriteLine(traceLog);
            }) //监听SQL命令对象，在执行后
        .UseLazyLoading(true)
        .Build());
    public static IFreeSql mysql => mysqlLazy.Value;

    static Lazy<IFreeSql> pgsqlLazy = new Lazy<IFreeSql>(() =>
    {
        NpgsqlConnection.GlobalTypeMapper.UseLegacyPostgis();
        return new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.PostgreSQL, "Host=192.168.164.10;Port=5432;Username=postgres;Password=123456;Database=tedb;Pooling=true;Maximum Pool Size=2")
        .UseAutoSyncStructure(true)
        .UseSyncStructureToLower(true)
        .UseLazyLoading(true)
        .UseMonitorCommand(
            cmd =>
            {
                Trace.WriteLine(cmd.CommandText);
            }, //监听SQL命令对象，在执行前
            (cmd, traceLog) =>
            {
                Console.WriteLine(traceLog);
            }) //监听SQL命令对象，在执行后
        .Build();
    });
    public static IFreeSql pgsql => pgsqlLazy.Value;

    static Lazy<IFreeSql> sqlserverLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=3")
        .UseAutoSyncStructure(true)
        .UseMonitorCommand(
            cmd =>
            {
                Trace.WriteLine(cmd.CommandText);
            }, //监听SQL命令对象，在执行前
            (cmd, traceLog) =>
            {
                Console.WriteLine(traceLog);
            }) //监听SQL命令对象，在执行后
        .UseLazyLoading(true)
        .Build());
    public static IFreeSql sqlserver => sqlserverLazy.Value;

    static Lazy<IFreeSql> oracleLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.Oracle, "user id=user1;password=123456;data source=//127.0.0.1:1521/XE;Pooling=true;Max Pool Size=2")
        .UseAutoSyncStructure(true)
        .UseLazyLoading(true)
        .UseSyncStructureToUpper(true)
        //.UseNoneCommandParameter(true)

        .UseMonitorCommand(
            cmd =>
            {
                Trace.WriteLine(cmd.CommandText);
            }, //监听SQL命令对象，在执行前
            (cmd, traceLog) =>
            {
                Console.WriteLine(traceLog);
            }) //监听SQL命令对象，在执行后
        .Build());
    public static IFreeSql oracle => oracleLazy.Value;

    static Lazy<IFreeSql> sqliteLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document.db;Attachs=xxxtb.db;Pooling=true;Max Pool Size=2")
        .UseAutoSyncStructure(true)
        .UseLazyLoading(true)
        .UseMonitorCommand(
            cmd =>
            {
                Trace.WriteLine(cmd.CommandText);
            }, //监听SQL命令对象，在执行前
            (cmd, traceLog) =>
            {
                Console.WriteLine(traceLog);
            }) //监听SQL命令对象，在执行后
        .Build());
    public static IFreeSql sqlite => sqliteLazy.Value;
}
