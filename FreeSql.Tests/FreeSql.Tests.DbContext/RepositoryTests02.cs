using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FreeSql.Tests.DbContext
{
    public class RepositoryTests02
    {
        [Fact]
        public void TestMethod1()
        {
            using (IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=:memory:")
                .UseMonitorCommand(cmd => Trace.WriteLine($"Sql：{cmd.CommandText}"))//监听SQL语句
                .UseAutoSyncStructure(true) //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
                .Build())
            {
                fsql.GlobalFilter.ApplyIf<User>("TenantFilter", () => TenantManager.Current > 0, a => a.TenantId == TenantManager.Current);

                fsql.Aop.AuditValue += (_, e) =>
                {
                    if (TenantManager.Current > 0 && e.Property.PropertyType == typeof(int) && e.Property.Name == "TenantId")
                    {
                        e.Value = TenantManager.Current;
                    };
                };

                IBaseRepository<User> resp = fsql.GetRepository<User>();
                resp.Delete(a => a.ID != null);
                Assert.True(resp != null);



                TenantManager.Current = 1;

                resp.InsertOrUpdate(new User()
                {
                    uname = "zhaoqin",
                });

                resp.InsertOrUpdate(new User()
                {
                    uname = "wanghuan",
                });
                long cc = resp.Where(a => a.ID != null).Count();
                Assert.True(cc == 2);



                TenantManager.Current = 2;

                resp.InsertOrUpdate(new User()
                {
                    uname = "zhaoqin1",
                });

                resp.InsertOrUpdate(new User()
                {
                    uname = "wanghuan1",
                });
                long c = resp.Where(a => a.ID != null).Count();
                Assert.True(c == 2);



                TenantManager.Current = 0;

                Assert.True(resp.Where(a => a.ID != null).Count() == 4);


                //多租户启用,但表达式想取消,这个可以成功
                TenantManager.Current = 2;
                long count1 = fsql.Select<User>().DisableGlobalFilter().Count();
                Assert.True(count1 == 4);


                Console.WriteLine("仓储的过滤器禁止,但不成功.");
                //仓储的过滤器禁止,但不成功.
                using (resp.DataFilter.DisableAll())
                {

                    long count2 = resp.Where(a => a.ID != null).Count();

                    Assert.True(count2 == 4);
                }

            }
        }


        public class TenantManager
        {
            // 注意一定是 static 静态化
            static AsyncLocal<int> _asyncLocal = new AsyncLocal<int>();

            public static int Current
            {
                get => _asyncLocal.Value;
                set => _asyncLocal.Value = value;
            }
        }

        public class BaseModel
        {
            [Column(IsIdentity = true)]
            public int? ID { get; set; }
            public int TenantId { get; set; }
        }

        public class User : BaseModel
        {

            public Guid cateId { get; set; }
            public Cate cate { get; set; }

            public string uname { get; set; }
            public int age { get; set; }

            public List<Group> groups { get; set; } = new List<Group>();
        }

        public class Cate : BaseModel
        {
            public string catename { get; set; }

            public List<User> users { get; set; }
        }

        public class Group : BaseModel
        {
            public string groupname { get; set; }

            public List<User> users { get; set; } = new List<User>();
        }

        public class User_Group
        {
            public Guid UserId { get; set; }
            public User user { get; set; }

            public Guid GroupId { get; set; }
            public Group group { get; set; }
        }

    }
}

