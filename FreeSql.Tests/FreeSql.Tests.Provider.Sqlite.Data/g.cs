using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

public class g
{

    static Lazy<IFreeSql> sqliteLazy = new Lazy<IFreeSql>(() =>
    {

        string dataSubDirectory = Path.Combine(AppContext.BaseDirectory);

        if (!Directory.Exists(dataSubDirectory))
            Directory.CreateDirectory(dataSubDirectory);

        AppDomain.CurrentDomain.SetData("DataDirectory", dataSubDirectory);

        var fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|local.db")
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
                .Build();

        return fsql;
    }
   );
    public static IFreeSql sqlite => sqliteLazy.Value;
}
