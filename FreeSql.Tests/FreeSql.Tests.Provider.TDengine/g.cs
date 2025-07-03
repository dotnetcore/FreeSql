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
                .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}\r\n"))
                .UseNoneCommandParameter(true)
                .Build();
            return fsql;
        });

        public static IFreeSql tdengine => tdengineLazy.Value;
    }
}