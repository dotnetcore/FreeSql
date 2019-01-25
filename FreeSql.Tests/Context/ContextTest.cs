using FreeSql.DbContext;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FreeSql.Tests.Context
{
    public class ContextTest
    {
        [Fact]
        public void DbContextFactoryTest()
        {
            var connstr = "Data Source=D:\\db\\_Cache.db;Pooling=true;FailIfMissing=false";
            var factory = new FreeSqlDbContextFactory();

            var context = factory.GetOrAdd<UserContext>(
                new FreeSqlBuilderConfiguration()
                    .UseConnectionString(FreeSql.DataType.Sqlite, connstr)
                    //.UseSlave("connectionString1", "connectionString2") //使用从数据库，支持多个

                    .UseMonitorCommand(
                        cmd => Console.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
                        (cmd, traceLog) => Console.WriteLine(traceLog)) //监听SQL命令对象，在执行后

                    .UseLogger(null) //使用日志，不指定默认输出控制台 ILogger
                    .UseCache(null) //使用缓存，不指定默认使用内存 IDistributedCache

                    .UseAutoSyncStructure(true) //自动同步实体结构到数据库
                    .UseSyncStructureToLower(true) //转小写同步结构

                    .UseLazyLoading(true) //延时加载导航属性对象，导航属性需要声明 virtual
            );
        }
    }

    public class UserModel
    {
        public string Account { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
    }

    public class LogModel
    {
        public UserModel User { get; set; }
        public DateTimeOffset Time { get; set; }
    }

    public class UserContext : FreeSqlDbContext
    {
        public FreeSqlDbSet<UserModel> Users { get; set; }
        public FreeSqlDbSet<LogModel> Logs { get; set; }

        public UserContext(IFreeSql context) : base(context)
        {
        }
    }
}
