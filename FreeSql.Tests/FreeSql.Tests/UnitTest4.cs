using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using static FreeSql.Tests.UnitTest1;

namespace FreeSql.Tests
{
    public class UnitTest4
    {
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


        [Fact]
        public void TestHzyTuple()
        {
            var xxxhzytuple = g.sqlite.Select<Templates, TaskBuild>()
                    .LeftJoin(w => w.t1.Id2 == w.t2.TemplatesId)
                    .Where(w => w.t1.Code == "xxx" && w.t2.OptionsEntity03 == true)
                    .OrderBy(w => w.t1.AddTime)
                    .ToSql();

            var xxxhzytupleGroupBy = g.sqlite.Select<Templates, TaskBuild>()
                    .LeftJoin(w => w.t1.Id2 == w.t2.TemplatesId)
                    .Where(w => w.t1.Code == "xxx" && w.t2.OptionsEntity03 == true)
                    .GroupBy(w => new { w.t1 })
                    .OrderBy(w => w.Key.t1.AddTime)
                    .ToSql(w => new { w.Key.t1 });

        }



    }
}
