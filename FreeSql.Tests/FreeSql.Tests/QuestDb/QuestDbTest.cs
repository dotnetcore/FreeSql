using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Tests.QuestDb
{
    public class QuestDbTest
    {
        public static IFreeSql fsql = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.QuestDb,
                @"host=192.168.0.36;port=8812;username=admin;password=quest;database=qdb;ServerCompatibilityMode=NoTypeLoading;")
            .UseMonitorCommand(cmd => Debug.WriteLine($"Sql：{cmd.CommandText}")) //监听SQL语句
            .UseNoneCommandParameter(true)
            .Build();

        public static IFreeSql restFsql = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.QuestDb,
                @"host=192.168.0.36;port=8812;username=admin;password=quest;database=qdb;ServerCompatibilityMode=NoTypeLoading;")
            .UseMonitorCommand(cmd => Debug.WriteLine($"Sql：{cmd.CommandText}")) //监听SQL语句
            .UseQuestDbRestAPI("192.168.0.36:9001", "admin", "ushahL(aer2r")
            .Build();
    }
}