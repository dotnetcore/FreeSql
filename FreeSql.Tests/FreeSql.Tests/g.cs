using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

public class g
{

    static Lazy<IFreeSql> mysqlLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=5")
        //.UseConnectionFactory(FreeSql.DataType.MySql, () => new MySql.Data.MySqlClient.MySqlConnection("Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;"))
        .UseAutoSyncStructure(true)
        //.UseGenerateCommandParameterWithLambda(true)
        .UseMonitorCommand(
            cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
            //, (cmd, traceLog) => Console.WriteLine(traceLog)
            )
        .UseLazyLoading(true)
        .Build());
    public static IFreeSql mysql => mysqlLazy.Value;

    static Lazy<IFreeSql> pgsqlLazy = new Lazy<IFreeSql>(() =>
    {
        NpgsqlConnection.GlobalTypeMapper.UseLegacyPostgis();
        return new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.PostgreSQL, "Host=192.168.164.10;Port=5432;Username=postgres;Password=123456;Database=tedb;Pooling=true;Maximum Pool Size=2")
        //.UseConnectionFactory(FreeSql.DataType.PostgreSQL, () => new Npgsql.NpgsqlConnection("Host=192.168.164.10;Port=5432;Username=postgres;Password=123456;Database=tedb;Pooling=true;"))
        .UseAutoSyncStructure(true)
        //.UseGenerateCommandParameterWithLambda(true)
        .UseSyncStructureToLower(true)
        .UseLazyLoading(true)
        .UseMonitorCommand(
            cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
            //, (cmd, traceLog) => Console.WriteLine(traceLog)
            )
        .Build();
    });
    public static IFreeSql pgsql => pgsqlLazy.Value;

    static Lazy<IFreeSql> sqlserverLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=3")
        //.UseConnectionFactory(FreeSql.DataType.SqlServer, () => new System.Data.SqlClient.SqlConnection("Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;"))
        //.UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=192.168.164.129;uid=sa;pwd=123456;Initial Catalog=ds_shop;Pooling=true;Max Pool Size=3")
        //.UseConnectionFactory(FreeSql.DataType.SqlServer, () => new System.Data.SqlClient.SqlConnection("Data Source=192.168.164.129;uid=sa;pwd=123456;Initial Catalog=ds_shop;Pooling=true;"))
        .UseAutoSyncStructure(true)
        //.UseGenerateCommandParameterWithLambda(true)
        .UseMonitorCommand(
            cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
            //, (cmd, traceLog) => Console.WriteLine(traceLog)
            )
        .UseLazyLoading(true)
        .Build());
    public static IFreeSql sqlserver => sqlserverLazy.Value;

    static Lazy<IFreeSql> oracleLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.Oracle, "user id=user1;password=123456;data source=//127.0.0.1:1521/XE;Pooling=true;Max Pool Size=2")
        //.UseConnectionFactory(FreeSql.DataType.Oracle, () => new Oracle.ManagedDataAccess.Client.OracleConnection("user id=user1;password=123456;data source=//127.0.0.1:1521/XE;Pooling=true;"))
        .UseAutoSyncStructure(true)
        //.UseGenerateCommandParameterWithLambda(true)
        .UseLazyLoading(true)
        .UseSyncStructureToUpper(true)
        //.UseNoneCommandParameter(true)

        .UseMonitorCommand(
            cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
            //, (cmd, traceLog) => Console.WriteLine(traceLog)
            )
        .Build());
    public static IFreeSql oracle => oracleLazy.Value;

    static Lazy<IFreeSql> sqliteLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document.db;Attachs=xxxtb.db;")
        //.UseConnectionFactory(FreeSql.DataType.Sqlite, () =>
        //{
        //    var conn = new System.Data.SQLite.SQLiteConnection(@"Data Source=|DataDirectory|\document.db;Pooling=true;");
        //    //conn.Open();
        //    //var cmd = conn.CreateCommand();
        //    //cmd.CommandText = $"attach database [xxxtb.db] as [xxxtb];\r\n";
        //    //cmd.ExecuteNonQuery();
        //    //cmd.Dispose();
        //    return conn;
        //})
        .UseAutoSyncStructure(true)
        //.UseGenerateCommandParameterWithLambda(true)
        .UseLazyLoading(true)
        .UseMonitorCommand(
            cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
            //, (cmd, traceLog) => Console.WriteLine(traceLog)
            )
        .Build());
    public static IFreeSql sqlite => sqliteLazy.Value;


    static Lazy<IFreeSql> msaccessLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.MsAccess, @"Provider=Microsoft.Jet.OleDb.4.0;Data Source=d:/accdb/2003.mdb;max pool size=5")
        .UseConnectionString(FreeSql.DataType.MsAccess, @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=d:/accdb/2007.accdb;max pool size=5")
        .UseAutoSyncStructure(true)
        //.UseGenerateCommandParameterWithLambda(true)
        .UseLazyLoading(true)
        .UseMonitorCommand(
            cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
            //, (cmd, traceLog) => Console.WriteLine(traceLog)
            )
        .Build());
    public static IFreeSql msaccess => msaccessLazy.Value;
}
