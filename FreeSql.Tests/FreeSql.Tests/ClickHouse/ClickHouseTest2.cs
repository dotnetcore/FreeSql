using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;
using Xunit;

namespace FreeSql.Tests.ClickHouse
{
    public class ClickHouseTest2
    {
        private static IFreeSql fsql = new FreeSqlBuilder().UseConnectionString(DataType.ClickHouse,
                "Host=127.0.0.1;Port=8123;Database=test;Compress=True;Min Pool Size=1")
            .UseMonitorCommand(cmd => Console.WriteLine($"线程：{cmd.CommandText}\r\n"))
            .UseNoneCommandParameter(true)
            .Build();
        [Fact]
        public void CodeFirst()
        {
            fsql.CodeFirst.SyncStructure(typeof(CollectDataEntityUpdate01));
        }
    }
}