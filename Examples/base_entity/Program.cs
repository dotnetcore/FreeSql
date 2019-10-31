using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace base_entity
{
    class Program
    {
        class TestConfig
        {
            public int clicks { get; set; }
            public string title { get; set; }
        }
        [Table(Name = "sysconfig")]
        public class S_SysConfig<T> : BaseEntity<S_SysConfig<T>>
        {
            [Column(IsPrimary = true)]
            public string Name { get; set; }

            [JsonMap]
            public T Config { get; set; }
        }

        static void Main(string[] args)
        {
            #region 初始化 IFreeSql
            BaseEntity.Initialization(new FreeSql.FreeSqlBuilder()
                .UseAutoSyncStructure(true)
                .UseNoneCommandParameter(true)
                .UseConnectionString(FreeSql.DataType.Sqlite, "data source=test.db;max pool size=5")
                .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=2")
                .Build());
            #endregion

            BaseEntity.Orm.UseJsonMap();

            new S_SysConfig<TestConfig> { Name = "testkey11", Config = new TestConfig { clicks = 11, title = "testtitle11" } }.Save();
            new S_SysConfig<TestConfig> { Name = "testkey22", Config = new TestConfig { clicks = 22, title = "testtitle22" } }.Save();
            new S_SysConfig<TestConfig> { Name = "testkey33", Config = new TestConfig { clicks = 33, title = "testtitle33" } }.Save();
            var testconfigs11 = S_SysConfig<TestConfig>.Select.ToList();

            Task.Run(async () =>
            {
                using (var uow = BaseEntity.Begin())
                {
                    var id = (await new User1().SaveAsync()).Id;
                    uow.Commit();
                }

                var ug1 = new UserGroup();
                ug1.GroupName = "分组一";
                await ug1.InsertAsync();

                var ug2 = new UserGroup();
                ug2.GroupName = "分组二";
                await ug2.InsertAsync();

                var u1 = new User1();

                u1.GroupId = ug1.Id;
                await u1.SaveAsync();

                await u1.DeleteAsync();
                await u1.RestoreAsync();

                u1.Nickname = "x1";
                await u1.UpdateAsync();

                var u11 = await User1.FindAsync(u1.Id);
                u11.Description = "备注";
                await u11.SaveAsync();

                await u11.DeleteAsync();

                var slslsl = Newtonsoft.Json.JsonConvert.SerializeObject(u1);
                var u11null = User1.Find(u1.Id);

                var u11s = User1.Where(a => a.Group.Id == ug1.Id).Limit(10).ToList();

                var u11s2 = User1.Select.LeftJoin<UserGroup>((a, b) => a.GroupId == b.Id).Limit(10).ToList();

                var ug1s = UserGroup.Select
                    .IncludeMany(a => a.User1s)
                    .Limit(10).ToList();

                var ug1s2 = UserGroup.Select.Where(a => a.User1s.AsSelect().Any(b => b.Nickname == "x1")).Limit(10).ToList();

                var r1 = new Role();
                r1.Id = "管理员";
                await r1.SaveAsync();

                var r2 = new Role();
                r2.Id = "超级会员";
                await r2.SaveAsync();

                var ru1 = new RoleUser1();
                ru1.User1Id = u1.Id;
                ru1.RoleId = r1.Id;
                await ru1.SaveAsync();

                ru1.RoleId = r2.Id;
                await ru1.SaveAsync();

                var u1roles = User1.Select.IncludeMany(a => a.Roles).ToList();
                var u1roles2 = User1.Select.Where(a => a.Roles.AsSelect().Any(b => b.Id == "xx")).ToList();

            }).Wait();

            Console.WriteLine("按任意键结束。。。");
            Console.ReadKey();
        }
    }
}
