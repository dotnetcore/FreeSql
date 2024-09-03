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
        private static readonly Lazy<IFreeSql> tdengineLazy = new Lazy<IFreeSql>(() =>
        {
             var fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.TDengine,
                    "host=localhost;port=6030;username=root;password=taosdata;protocol=Native;db=test;")
                .UseAutoSyncStructure(true)
                .UseNameConvert(Internal.NameConvertType.ToLower)
                .UseMonitorCommand(
                    cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " +
                                           cmd.CommandText) //监听SQL命令对象，在执行前
                    //, (cmd, traceLog) => Console.WriteLine(traceLog)
                )
                .Build();
             return fsql;
        });

        public static IFreeSql tdengine => tdengineLazy.Value;
    }
}