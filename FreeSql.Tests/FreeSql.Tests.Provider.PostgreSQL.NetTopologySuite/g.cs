using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

public class g
{
    static Lazy<IFreeSql> pgsqlLazy = new Lazy<IFreeSql>(() =>
    {
        NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();
        return new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.PostgreSQL, "Host=192.168.164.10;Port=5432;Username=postgres;Password=123456;Database=tedb;ArrayNullabilityMode=Always;Pooling=true;Maximum Pool Size=2")
        //.UseConnectionFactory(FreeSql.DataType.PostgreSQL, () => new Npgsql.NpgsqlConnection("Host=192.168.164.10;Port=5432;Username=postgres;Password=123456;Database=tedb;Pooling=true;"))
        .UseAutoSyncStructure(true)
        //.UseGenerateCommandParameterWithLambda(true)
        .UseNameConvert(FreeSql.Internal.NameConvertType.ToLower)
        .UseLazyLoading(true)
        .UseMonitorCommand(
            cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
            //, (cmd, traceLog) => Console.WriteLine(traceLog)
            )
        .Build();
    });
    public static IFreeSql pgsql => pgsqlLazy.Value;
}
