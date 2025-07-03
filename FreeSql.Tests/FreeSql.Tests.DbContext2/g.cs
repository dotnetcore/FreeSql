using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public class g
{
    static Lazy<IFreeSql> sqliteLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|/document22.db;Attachs=xxxtb.db;Pooling=true;Max Pool Size=10")
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
        .UseNoneCommandParameter(true)
        .Build());
    public static IFreeSql sqlite => sqliteLazy.Value;

    public static IFreeSql CreateMemory()
    {
        return new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=:memory:")
            .UseAutoSyncStructure(true)
            .UseNoneCommandParameter(true)
            .UseMonitorCommand(cmd => Trace.WriteLine(cmd.CommandText))
            .Build();
    }

    static Lazy<IFreeSql> firebirdLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.Firebird, @"database=localhost:D:\fbdata\EXAMPLES.fdb;user=sysdba;password=123456;max pool size=5")
        //.UseConnectionFactory(FreeSql.DataType.Firebird, () => new FirebirdSql.Data.FirebirdClient.FbConnection(@"database=localhost:D:\fbdata\EXAMPLES.fdb;user=sysdba;password=123456;max pool size=5"))
        .UseAutoSyncStructure(true)
        .UseLazyLoading(true)
        .UseNameConvert(FreeSql.Internal.NameConvertType.ToUpper)
        //.UseNoneCommandParameter(true)

        .UseMonitorCommand(
            cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
            (cmd, traceLog) => Console.WriteLine(traceLog))
        .Build());
    public static IFreeSql firebird => firebirdLazy.Value;
}
