using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

public class g
{
    static Lazy<IFreeSql> duckdbLazy = new Lazy<IFreeSql>(() =>
    {
        return new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.DuckDB, "DataSource=duckdb01.db")
            .UseAutoSyncStructure(true)
            .UseNameConvert(FreeSql.Internal.NameConvertType.ToLower)
            .UseMonitorCommand(
                cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
                //, (cmd, traceLog) => Console.WriteLine(traceLog)
                )
            .Build();
    });
    public static IFreeSql duckdb => duckdbLazy.Value;
}
