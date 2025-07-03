using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

public class g
{
    
    static Lazy<IFreeSql> oracleLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.Oracle, "Provider=OraOLEDB.Oracle;user id=9user;password=123456;data source=//127.0.0.1:1521/XE;Pooling=true;Max Pool Size=2")
        .UseAutoSyncStructure(true)
        //.UseGenerateCommandParameterWithLambda(true)
        .UseLazyLoading(true)
        .UseNameConvert(FreeSql.Internal.NameConvertType.ToUpper)
        //.UseNoneCommandParameter(true)

        .UseMonitorCommand(
            cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
            //, (cmd, traceLog) => Console.WriteLine(traceLog)
            )
        .Build());
    public static IFreeSql oracle => oracleLazy.Value;


}
