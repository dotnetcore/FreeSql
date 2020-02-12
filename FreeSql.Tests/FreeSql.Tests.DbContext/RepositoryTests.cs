using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
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
            repos.Update(item); //这行执行 UPDATE "AddUpdateInfo" SET "Title" = 'xxx' WHERE("Id" = '1942fb53-9700-411d-8895-ce4cecdf3257')
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));

            repos.Update(item); //这行不执行 SQL，未变化

            repos.AttachOnlyPrimary(item).Update(item); //这行更新状态值，只有主键值存在，执行更新 set title = xxx

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

        [Fact]
        public void EnableAddOrUpdateNavigateList_OneToMany()
        {
            var repo = g.sqlite.GetRepository<Cagetory>();
            var cts = new[] {
                new Cagetory
                {
                    Name = "分类1",
                    Goodss = new List<Goods>(new[]
                    {
                        new Goods { Name = "商品1" },
                        new Goods { Name = "商品2" },
                        new Goods { Name = "商品3" }
                    })
                },
                new Cagetory
                {
                    Name = "分类2",
                    Goodss = new List<Goods>(new[]
                    {
                        new Goods { Name = "商品4" },
                        new Goods { Name = "商品5" }
                    })
                }
            };
            repo.Insert(cts);
            cts[0].Name = "分类11";
            cts[0].Goodss.Clear();
            cts[1].Name = "分类22";
            cts[1].Goodss.Clear();
            repo.Update(cts);
            cts[0].Name = "分类111";
            cts[0].Goodss.Clear();
            cts[0].Goodss.Add(new Goods { Name = "商品33" });
            cts[1].Name = "分类222";
            cts[1].Goodss.Clear();
            cts[1].Goodss.Add(new Goods { Name = "商品55" });
            repo.Update(cts);
        }
        [Table(Name = "EAUNL_OTM_CT")]
        class Cagetory
        {
            public Guid Id { get; set; }
            public string Name { get; set; }

            [Navigate("CagetoryId")]
            public List<Goods> Goodss { get; set; }
        }
        [Table(Name = "EAUNL_OTM_GD")]
        class Goods
        {
            public Guid Id { get; set; }
            public Guid CagetoryId { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void EnableAddOrUpdateNavigateList_OneToMany_Parent()
        {
            var repo = g.sqlite.GetRepository<CagetoryParent>();
            var cts = new[] {
                new CagetoryParent
                {
                    Name = "分类1",
                    Childs = new List<CagetoryParent>(new[]
                    {
                        new CagetoryParent { Name = "分类1_1" },
                        new CagetoryParent { Name = "分类1_2" },
                        new CagetoryParent { Name = "分类1_3" }
                    })
                },
                new CagetoryParent
                {
                    Name = "分类2",
                    Childs = new List<CagetoryParent>(new[]
                    {
                        new CagetoryParent { Name = "分类2_1" },
                        new CagetoryParent { Name = "分类2_2" }
                    })
                }
            };
            repo.DbContextOptions.EnableAddOrUpdateNavigateList = false; //关闭级联保存功能
            repo.Insert(cts);
            repo.SaveMany(cts[0], "Childs"); //指定保存 Childs 一对多属性
            cts[0].Name = "分类11";
            cts[0].Childs.Clear();
            cts[1].Name = "分类22";
            cts[1].Childs.Clear();
            repo.Update(cts);
            cts[0].Name = "分类111";
            cts[0].Childs.Clear();
            cts[0].Childs.Add(new CagetoryParent { Name = "分类1_33" });
            cts[1].Name = "分类222";
            cts[1].Childs.Clear();
            cts[1].Childs.Add(new CagetoryParent { Name = "分类2_22" });
            repo.Update(cts);
        }
        [Table(Name = "EAUNL_OTMP_CT")]
        class CagetoryParent
        {
            public Guid Id { get; set; }
            public string Name { get; set; }

            public Guid ParentId { get; set; }
            [Navigate("ParentId")]
            public List<CagetoryParent> Childs { get; set; }
        }

        [Fact]
        public void EnableAddOrUpdateNavigateList_ManyToMany()
        {
            var tags = new[] {
                new Tag { TagName = "流行" },
                new Tag { TagName = "80后" },
                new Tag { TagName = "00后" },
                new Tag { TagName = "摇滚" }
            };
            var ss = new[]
            {
                new Song
                {
                    Name = "爱你一万年.mp3",
                    Tags = new List<Tag>(new[]
                    {
                        tags[0], tags[1]
                    })
                },
                new Song
                {
                    Name = "李白.mp3",
                    Tags = new List<Tag>(new[]
                    {
                        tags[0], tags[2]
                    })
                }
            };
            var repo = g.sqlite.GetRepository<Song>();
            //repo.DbContextOptions.EnableAddOrUpdateNavigateList = false; //关闭级联保存功能
            repo.Insert(ss);
            //repo.SaveMany(ss[0], "Tags"); //指定保存 Tags 多对多属性

            ss[0].Name = "爱你一万年.mp5";
            ss[0].Tags.Clear();
            ss[0].Tags.Add(tags[0]);
            ss[1].Name = "李白.mp5";
            ss[1].Tags.Clear();
            ss[1].Tags.Add(tags[3]);
            repo.Update(ss);

            ss[0].Name = "爱你一万年.mp4";
            ss[0].Tags.Clear();
            ss[1].Name = "李白.mp4";
            ss[1].Tags.Clear();
            repo.Update(ss);
        }
        [Table(Name = "EAUNL_MTM_SONG")]
        class Song
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public List<Tag> Tags { get; set; }
        }
        [Table(Name = "EAUNL_MTM_TAG")]
        class Tag
        {
            public Guid Id { get; set; }
            public string TagName { get; set; }
            public List<Song> Songs { get; set; }
        }
        [Table(Name = "EAUNL_MTM_SONGTAG")]
        class SongTag
        {
            public Guid SongId { get; set; }
            public Song Song { get; set; }
            public Guid TagId { get; set; }
            public Tag Tag { get; set; }
        }
    }
}
