using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics;
using System.Text;
using System.Threading;

public class g
{


    static Lazy<IFreeSql> gbaseLazy = new Lazy<IFreeSql>(() =>
    {
        var build = new OdbcConnectionStringBuilder();
        build.Driver = "GBase ODBC DRIVER (64-Bit)";    // 在系统中注册的驱动名称
        build.Add("Host", "192.168.164.134");          // 主机地址或者IP地址
        build.Add("Service", "9088");                   // 数据库服务器的使用的端口号
        build.Add("Server", "gbase01");                 // 数据库服务名称
        build.Add("Database", "testdb");                // 数据库名（DBNAME）
        build.Add("Protocol", "onsoctcp");              // 网络协议名称
        build.Add("Uid", "gbasedbt");                   // 用户
        build.Add("Pwd", "GBase123");                   // 密码
        build.Add("Db_locale", "zh_CN.utf8");           // 数据库字符集
        build.Add("Client_locale", "zh_CN.utf8");       // 客户端字符集

        return new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.GBase, build.ConnectionString)
            .UseAutoSyncStructure(true)
            .UseLazyLoading(true)

            .UseMonitorCommand(
                cmd => Trace.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
                (cmd, traceLog) => Console.WriteLine(traceLog))
            .Build();
    });
    public static IFreeSql gbase => gbaseLazy.Value;

}
