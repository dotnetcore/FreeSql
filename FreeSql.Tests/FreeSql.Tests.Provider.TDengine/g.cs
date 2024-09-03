using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Tests.Provider.TDengine
{
    internal class g
    {
        static readonly Lazy<IFreeSql> tdengineLazy = new Lazy<IFreeSql>(() =>
        {
            return new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.TDengine,
                    "Host=127.0.0.1;Port=6030;Username=root;Password=taosdata;Protocol=Native;db=test;Min Pool Size=1;Max Poll Size=10")
                .UseAutoSyncStructure(true)
                .UseNameConvert(Internal.NameConvertType.ToLower)
                .UseMonitorCommand(
                    cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " +
                                           cmd.CommandText) //监听SQL命令对象，在执行前
                    //, (cmd, traceLog) => Console.WriteLine(traceLog)
                )
                .Build();
        });

        public static IFreeSql tdengine => tdengineLazy.Value;
    }
}