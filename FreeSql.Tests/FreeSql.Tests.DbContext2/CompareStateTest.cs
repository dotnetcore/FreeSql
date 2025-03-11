using FreeSql.DataAnnotations;
using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FreeSql.Tests.DbContext2
{
    public class CompareStateTest
    {
        [Fact]
        public void TestCompareState()
        {
            using (var fsql = g.CreateMemory())
            {
                fsql.Insert(new AppInfoEntity { AppID = "03DN8CW8", AppName = "app_01" }).ExecuteAffrows();
                var repo = fsql.GetRepository<AppInfoEntity>();

                var appInfo = repo.Where(info => info.AppID == "03DN8CW8").First();
                appInfo = repo.Where(info => info.AppID == "03DN8CW8").First();
                var compareDic = new Dictionary<string, object[]>();
                var updateInfo = "";

                repo.Attach(appInfo);
                appInfo.AppName = "测试";
                compareDic = repo.CompareState(appInfo);
                Console.WriteLine(appInfo.AppName);

            }
        }

        public class AppInfoEntity
        {
            [Column(IsPrimary = true, Name = "APP_ID")]
            public string AppID { get; set; }
            [Column(Name = "APP_NAME")]
            public string AppName { get; set; }
        }

    }
}
