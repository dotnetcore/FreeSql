using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FreeSql.Tests.QuestDb
{
    public class QuestDbTest
    {
        public static IFreeSql Db = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.QuestDb,
                @"host=localhost;port=8812;username=admin;password=quest;database=qdb;ServerCompatibilityMode=NoTypeLoading;")
            .UseMonitorCommand(cmd => Debug.WriteLine($"Sql：{cmd.CommandText}")) //监听SQL语句
            .UseNoneCommandParameter(true)
            .Build();

        public static IFreeSql RestApiDb = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.QuestDb,
                @"host=localhost;port=8812;username=admin;password=quest;database=qdb;ServerCompatibilityMode=NoTypeLoading;")
            .UseMonitorCommand(cmd => Debug.WriteLine($"Sql：{cmd.CommandText}")) //监听SQL语句
            .UseQuestDbRestAPI("localhost:9000") 
            .Build();

    }

 
}