using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Tests.Provider.Xugu
{
    public class g
    {

        static Lazy<IFreeSql> xuguLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.Xugu, "IP=127.0.0.1;DB=SYSTEM;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=UTF8")
            //.UseAutoSyncStructure(true)
            //.UseGenerateCommandParameterWithLambda(true)
            .UseLazyLoading(true)
            //.UseNameConvert(FreeSql.Internal.NameConvertType.ToUpper)
            //.UseNoneCommandParameter(true)

            .UseMonitorCommand(
                cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
                                                                                                                 //, (cmd, traceLog) => Console.WriteLine(traceLog)
                )
            .Build());
        public static IFreeSql xugu => xuguLazy.Value;
    }
}
