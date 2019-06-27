using System;
using Xunit;

namespace FreeSql.Tests
{
    public class RepositoryTests
    {

        [Fact]
        public void AddUpdate()
        {
            var repos = g.sqlite.GetGuidRepository<AddUpdateInfo>();

            var item = repos.Insert(new AddUpdateInfo());
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));

            item = repos.Insert(new AddUpdateInfo { Id = Guid.NewGuid() });
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));

            item.Title = "xxx";
            repos.Update(item);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));

            Console.WriteLine(repos.UpdateDiy.Where(a => a.Id == item.Id).Set(a => a.Clicks + 1).ToSql());
            repos.UpdateDiy.Where(a => a.Id == item.Id).Set(a => a.Clicks + 1).ExecuteAffrows();

            item = repos.Find(item.Id);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));
        }

        [Fact]
        public void UpdateAttach()
        {
            var repos = g.sqlite.GetGuidRepository<AddUpdateInfo>();

            var item = new AddUpdateInfo { Id = Guid.NewGuid() };
            repos.Attach(item);

            item.Title = "xxx";
            repos.Update(item);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));

            Console.WriteLine(repos.UpdateDiy.Where(a => a.Id == item.Id).Set(a => a.Clicks + 1).ToSql());
            repos.UpdateDiy.Where(a => a.Id == item.Id).Set(a => a.Clicks + 1).ExecuteAffrows();

            item = repos.Find(item.Id);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));
        }

        [Fact]
        public void UpdateWhenNotExists()
        {
            var repos = g.sqlite.GetGuidRepository<AddUpdateInfo>();

            var item = new AddUpdateInfo { Id = Guid.NewGuid() };
            item.Title = "xxx";
            Assert.Throws<Exception>(() => repos.Update(item));
        }

        [Fact]
        public void Update()
        {
            g.sqlite.Insert(new AddUpdateInfo()).ExecuteAffrows();

            var repos = g.sqlite.GetGuidRepository<AddUpdateInfo>();

            var item = new AddUpdateInfo { Id = g.sqlite.Select<AddUpdateInfo>().First().Id };

            item.Title = "xxx";
            repos.Update(item);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));
        }

        public class AddUpdateInfo
        {

            public Guid Id { get; set; }
            public string Title { get; set; }

            public int Clicks { get; set; } = 10;
        }

        [Fact]
        public void UnitOfWorkRepository()
        {
            foreach (var fsql in new[] { g.sqlite, /*g.mysql, g.pgsql, g.oracle, g.sqlserver*/ })
            {

                fsql.CodeFirst.ConfigEntity<FlowModel>(f =>
                {
                    f.Property(b => b.UserId).IsPrimary(true);
                    f.Property(b => b.Id).IsPrimary(true).IsIdentity(true);
                    f.Property(b => b.Name).IsNullable(false);
                });

                FlowModel flow = new FlowModel()
                {
                    CreateTime = DateTime.Now,
                    Name = "aaa",
                    LastModifyTime = DateTime.Now,
                    UserId = 1,
                };
                var flowRepos = fsql.GetRepository<FlowModel>();
                flowRepos.Insert(flow);

                //事务添加
                flow = new FlowModel()
                {
                    CreateTime = DateTime.Now,
                    Name = "aaa",
                    LastModifyTime = DateTime.Now,
                    UserId = 1,
                };
                using (var uow = fsql.CreateUnitOfWork())
                {
                    flowRepos = uow.GetRepository<FlowModel>();
                    flowRepos.Insert(flow);
                    uow.Commit();
                }
            }
        }

        [Fact]
        public void UnitOfWorkRepositoryWithDisableBeforeInsert()
        {
            foreach (var fsql in new[] { g.sqlite, })
            {
                fsql.CodeFirst.ConfigEntity<FlowModel>(f =>
                {
                    f.Property(b => b.UserId).IsPrimary(true);
                    f.Property(b => b.Id).IsPrimary(true).IsIdentity(true);
                    f.Property(b => b.Name).IsNullable(false);
                });

                var flowRepos = fsql.GetRepository<FlowModel>();

                var flow = new FlowModel()
                {
                    CreateTime = DateTime.Now,
                    Name = "aaa",
                    LastModifyTime = DateTime.Now,
                    UserId = 1,
                };

                //清理掉数据库中已存在的数据，为了接下来的插入测试
                flowRepos.Delete(a => a.UserId == 1 && a.Name == "aaa");

                using (var uow = fsql.CreateUnitOfWork())
                {
                    //关闭工作单元（不会开始事务）
                    uow.Close();
                    var uowFlowRepos = uow.GetRepository<FlowModel>();
                    uowFlowRepos.Insert(flow);
                    //已关闭工作单元，提不提交都没影响，此处注释来确定工作单元开关是否生效：关闭了，不Commit也应该插入数据
                    //uow.Commit();
                }

                Assert.True(flowRepos.Select.Any(a => a.UserId == 1 && a.Name == "aaa"));
            }

        }

        [Fact]
        public void UnitOfWorkRepositoryWithDisableAfterInsert()
        {
            foreach (var fsql in new[] { g.sqlite, })
            {
                fsql.CodeFirst.ConfigEntity<FlowModel>(f =>
                {
                    f.Property(b => b.UserId).IsPrimary(true);
                    f.Property(b => b.Id).IsPrimary(true).IsIdentity(true);
                    f.Property(b => b.Name).IsNullable(false);
                });

                var flowRepos = fsql.GetRepository<FlowModel>();

                //清理掉数据库中已存在的数据，为了接下来的插入测试
                flowRepos.Delete(a => a.UserId == 1 && a.Name == "aaa");

                var flow = new FlowModel()
                {
                    CreateTime = DateTime.Now,
                    Name = "aaa",
                    LastModifyTime = DateTime.Now,
                    UserId = 1,
                };


                Assert.Throws<Exception>(() =>
                {
                    using (var uow = fsql.CreateUnitOfWork())
                    {
                        var uowFlowRepos = uow.GetRepository<FlowModel>();
                        uowFlowRepos.Insert(flow);
                        //有了任意 Insert/Update/Delete 调用关闭uow的方法将会发生异常
                        uow.Close();
                        uow.Commit();
                    }

                });
            }
        }

        [Fact]
        public void UnitOfWorkRepositoryWithoutDisable()
        {
            foreach (var fsql in new[] { g.sqlite, })
            {
                fsql.CodeFirst.ConfigEntity<FlowModel>(f =>
                {
                    f.Property(b => b.UserId).IsPrimary(true);
                    f.Property(b => b.Id).IsPrimary(true).IsIdentity(true);
                    f.Property(b => b.Name).IsNullable(false);
                });

                var flowRepos = fsql.GetRepository<FlowModel>();
                if (flowRepos.Select.Any(a => a.UserId == 1 && a.Name == "aaa"))
                {
                    flowRepos.Delete(a => a.UserId == 1);
                }


                var flow = new FlowModel()
                {
                    CreateTime = DateTime.Now,
                    Name = "aaa",
                    LastModifyTime = DateTime.Now,
                    UserId = 1,
                };


                using (var uow = fsql.CreateUnitOfWork())
                {
                    var uowFlowRepos = uow.GetRepository<FlowModel>();
                    uowFlowRepos.Insert(flow);
                    //不调用commit将不会提交数据库更改
                    //uow.Commit();
                }
                Assert.False(flowRepos.Select.Any(a => a.UserId == 1 && a.Name == "aaa"));
            }
        }


        public partial class FlowModel
        {
            public int UserId { get; set; }
            public int Id { get; set; }
            public int? ParentId { get; set; }
            public string Name { get; set; }
            public DateTime CreateTime { get; set; }
            public DateTime LastModifyTime { get; set; }
            public string Desc { get; set; }
        }

        [Fact]
        public void AsType()
        {
            g.sqlite.Insert(new AddUpdateInfo()).ExecuteAffrows();

            var repos = g.sqlite.GetGuidRepository<object>();
            repos.AsType(typeof(AddUpdateInfo));

            var item = new AddUpdateInfo();
            repos.Insert(item);
            repos.Update(item);

            item.Clicks += 1;
            repos.InsertOrUpdate(item);

            var item2 = repos.Find(item.Id) as AddUpdateInfo;
            Assert.Equal(item.Clicks, item2.Clicks);

            repos.DataFilter.Apply("xxx", a => (a as AddUpdateInfo).Clicks == 2);
            Assert.Null(repos.Find(item.Id));
        }
    }
}
