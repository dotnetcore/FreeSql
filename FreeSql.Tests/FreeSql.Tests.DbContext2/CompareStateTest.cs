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

        [Fact]
        async public Task TestIssues()
        {
            using (var freeSql = g.CreateMemory())
            {
                freeSql.Aop.AuditValue += (_, args) =>
                {
                    Console.WriteLine(args.AuditValueType);
                    Console.WriteLine(args.Property.Name);
                };

                var repository = freeSql.GetRepository<People>();
                var people = new People { Name = "John Doe" };
                await repository.InsertOrUpdateAsync(people);
                people.Name = "Tim Doe";
                await repository.InsertOrUpdateAsync(people);
            }
        }

        public class People
        {
            [Column(IsPrimary = true, IsIdentity = true)]
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
