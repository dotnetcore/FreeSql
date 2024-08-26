using FreeSql.DataAnnotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FreeSql.Tests
{
    public class RepositoryTests
    {
        [Fact]
        public void DeleteCascade()
        {
            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "data source=:memory:")
                .UseAutoSyncStructure(true)
                .UseNoneCommandParameter(true)
                .UseMonitorCommand(cmd => Trace.WriteLine(cmd.CommandText))
                .Build())
            {
                fsql.CodeFirst.GetTableByEntity(typeof(DeleteCascadeUserGroup)).ColumnsByCs
                    .Where(a => !new[] { typeof(string), typeof(int), typeof(DateTime), typeof(long) }.Contains(a.Value.Attribute.MapType))
                    .ToArray();

                fsql.GlobalFilter.Apply<DeleteCascadeUserGroup>("soft_delete", a => a.IsDeleted == false);

                fsql.Delete<DeleteCascadeUserGroup>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUserExt>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUser>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUserTag>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeTag>().Where("1=1").ExecuteAffrows();

                var groupRepo = fsql.GetRepository<DeleteCascadeUserGroup>();
                var userRepo = fsql.GetRepository<DeleteCascadeUser>();
                var userextRepo = fsql.GetRepository<DeleteCascadeUserExt>();
                var tagRepo = fsql.GetRepository<DeleteCascadeTag>();
                groupRepo.DbContextOptions.EnableCascadeSave = true;
                userRepo.DbContextOptions.EnableCascadeSave = true;
                userextRepo.DbContextOptions.EnableCascadeSave = true;
                tagRepo.DbContextOptions.EnableCascadeSave = true;
                groupRepo.DbContextOptions.EnableGlobalFilter = false;
                userRepo.DbContextOptions.EnableGlobalFilter = false;
                userextRepo.DbContextOptions.EnableGlobalFilter = false;
                tagRepo.DbContextOptions.EnableGlobalFilter = false;

                //OneToOne InDatabase
                fsql.Delete<DeleteCascadeUser>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUserExt>().Where("1=1").ExecuteAffrows();
                var user = new DeleteCascadeUser { Username = "admin01", Password = "pwd01", UserExt = new DeleteCascadeUserExt { Remark = "用户备注01" } };
                userRepo.Insert(user);
                var ret = userRepo.DeleteCascadeByDatabase(a => a.Id == user.Id);
                Assert.Equal(2, ret.Count);
                Assert.IsType<DeleteCascadeUserExt>(ret[0]);
                Assert.Equal(user.UserExt.UserId, (ret[0] as DeleteCascadeUserExt).UserId);
                Assert.Equal(user.UserExt.Remark, (ret[0] as DeleteCascadeUserExt).Remark);
                Assert.IsType<DeleteCascadeUser>(ret[1]);
                Assert.Equal(user.Id, (ret[1] as DeleteCascadeUser).Id);
                Assert.Equal(user.Username, (ret[1] as DeleteCascadeUser).Username);
                Assert.Equal(user.Password, (ret[1] as DeleteCascadeUser).Password);
                //OneToOne EnableCascadeSave InMemory
                fsql.Delete<DeleteCascadeUser>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUserExt>().Where("1=1").ExecuteAffrows();
                user = new DeleteCascadeUser { Username = "admin01", Password = "pwd01", UserExt = new DeleteCascadeUserExt { Remark = "用户备注01" } };
                userRepo.Insert(user);
                Assert.True(user.Id > 0);
                Assert.True(user.UserExt.UserId > 0);
                var affrows = userRepo.Delete(user);
                Assert.Equal(2, affrows);
                Assert.Equal(0, user.Id);
                Assert.Equal("admin01", user.Username);
                Assert.Equal("pwd01", user.Password);
                Assert.True(user.UserExt.UserId > 0);
                Assert.Equal("用户备注01", user.UserExt.Remark);
                Assert.False(userRepo.Select.Any());
                Assert.False(userextRepo.Select.Any());

                //OneToOne InDatabase 先删除 UserExt
                fsql.Delete<DeleteCascadeUser>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUserExt>().Where("1=1").ExecuteAffrows();
                user = new DeleteCascadeUser { Username = "admin01", Password = "pwd01", UserExt = new DeleteCascadeUserExt { Remark = "用户备注01" } };
                userRepo.Insert(user);
                ret = userextRepo.DeleteCascadeByDatabase(a => a.UserId == user.UserExt.UserId);
                Assert.Equal(2, ret.Count);
                Assert.IsType<DeleteCascadeUserExt>(ret[1]);
                Assert.Equal(user.UserExt.UserId, (ret[1] as DeleteCascadeUserExt).UserId);
                Assert.Equal(user.UserExt.Remark, (ret[1] as DeleteCascadeUserExt).Remark);
                Assert.IsType<DeleteCascadeUser>(ret[0]);
                Assert.Equal(user.Id, (ret[0] as DeleteCascadeUser).Id);
                Assert.Equal(user.Username, (ret[0] as DeleteCascadeUser).Username);
                Assert.Equal(user.Password, (ret[0] as DeleteCascadeUser).Password);
                //OneToOne EnableCascadeSave InMemory 先删除 UserExt
                fsql.Delete<DeleteCascadeUser>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUserExt>().Where("1=1").ExecuteAffrows();
                user = new DeleteCascadeUser { Username = "admin01", Password = "pwd01", UserExt = new DeleteCascadeUserExt { Remark = "用户备注01" } };
                userRepo.Insert(user);
                Assert.True(user.Id > 0);
                Assert.True(user.UserExt.UserId > 0);
                var userext = userextRepo.Where(a => a.UserId == user.Id).Include(a => a.User).First();
                Assert.NotNull(userext);
                Assert.Equal(user.UserExt.UserId, userext.UserId);
                Assert.Equal(user.Id, userext.User.Id);
                affrows = userextRepo.Delete(userext);
                Assert.Equal(2, affrows);
                Assert.Equal(0, userext.User.Id);
                Assert.Equal("admin01", userext.User.Username);
                Assert.Equal("pwd01", userext.User.Password);
                Assert.True(userext.UserId > 0);
                Assert.Equal("用户备注01", userext.Remark);
                Assert.False(userRepo.Select.Any());
                Assert.False(userextRepo.Select.Any());

                //OneToMany InDatabase
                fsql.Delete<DeleteCascadeUserGroup>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUser>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUserExt>().Where("1=1").ExecuteAffrows();
                var group = new DeleteCascadeUserGroup
                {
                    GroupName = "group01",
                    Users = new List<DeleteCascadeUser>
                {
                    new DeleteCascadeUser { Username = "admin01", Password = "pwd01", UserExt = new DeleteCascadeUserExt { Remark = "用户备注01" } },
                    new DeleteCascadeUser { Username = "admin02", Password = "pwd02", UserExt = new DeleteCascadeUserExt { Remark = "用户备注02" } },
                    new DeleteCascadeUser { Username = "admin03", Password = "pwd03", UserExt = new DeleteCascadeUserExt { Remark = "用户备注03" } },
                }
                };
                groupRepo.Insert(group);
                Assert.Equal(group.Id, group.Users[0].GroupId);
                Assert.Equal(group.Id, group.Users[1].GroupId);
                Assert.Equal(group.Id, group.Users[2].GroupId);
                ret = groupRepo.DeleteCascadeByDatabase(a => a.Id == group.Id);
                Assert.Equal(7, ret.Count);
                Assert.IsType<DeleteCascadeUserExt>(ret[0]);
                Assert.Equal(group.Users[0].UserExt.UserId, (ret[0] as DeleteCascadeUserExt).UserId);
                Assert.Equal(group.Users[0].UserExt.Remark, (ret[0] as DeleteCascadeUserExt).Remark);
                Assert.IsType<DeleteCascadeUserExt>(ret[1]);
                Assert.Equal(group.Users[1].UserExt.UserId, (ret[1] as DeleteCascadeUserExt).UserId);
                Assert.Equal(group.Users[1].UserExt.Remark, (ret[1] as DeleteCascadeUserExt).Remark);
                Assert.IsType<DeleteCascadeUserExt>(ret[2]);
                Assert.Equal(group.Users[2].UserExt.UserId, (ret[2] as DeleteCascadeUserExt).UserId);
                Assert.Equal(group.Users[2].UserExt.Remark, (ret[2] as DeleteCascadeUserExt).Remark);
                Assert.IsType<DeleteCascadeUser>(ret[3]);
                Assert.Equal(group.Users[0].Id, (ret[3] as DeleteCascadeUser).Id);
                Assert.Equal(group.Users[0].Username, (ret[3] as DeleteCascadeUser).Username);
                Assert.Equal(group.Users[0].Password, (ret[3] as DeleteCascadeUser).Password);
                Assert.IsType<DeleteCascadeUser>(ret[4]);
                Assert.Equal(group.Users[1].Id, (ret[4] as DeleteCascadeUser).Id);
                Assert.Equal(group.Users[1].Username, (ret[4] as DeleteCascadeUser).Username);
                Assert.Equal(group.Users[1].Password, (ret[4] as DeleteCascadeUser).Password);
                Assert.IsType<DeleteCascadeUser>(ret[5]);
                Assert.Equal(group.Users[2].Id, (ret[5] as DeleteCascadeUser).Id);
                Assert.Equal(group.Users[2].Username, (ret[5] as DeleteCascadeUser).Username);
                Assert.Equal(group.Users[2].Password, (ret[5] as DeleteCascadeUser).Password);
                Assert.IsType<DeleteCascadeUserGroup>(ret[6]);
                Assert.Equal(group.Id, (ret[6] as DeleteCascadeUserGroup).Id);
                Assert.Equal(group.GroupName, (ret[6] as DeleteCascadeUserGroup).GroupName);
                //OneToMany EnableCascadeSave InMemory
                fsql.Delete<DeleteCascadeUserGroup>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUser>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUserExt>().Where("1=1").ExecuteAffrows();
                group = new DeleteCascadeUserGroup
                {
                    GroupName = "group01",
                    Users = new List<DeleteCascadeUser>
                {
                    new DeleteCascadeUser { Username = "admin01", Password = "pwd01", UserExt = new DeleteCascadeUserExt { Remark = "用户备注01" } },
                    new DeleteCascadeUser { Username = "admin02", Password = "pwd02", UserExt = new DeleteCascadeUserExt { Remark = "用户备注02" } },
                    new DeleteCascadeUser { Username = "admin03", Password = "pwd03", UserExt = new DeleteCascadeUserExt { Remark = "用户备注03" } },
                }
                };
                groupRepo.Insert(group);
                Assert.Equal(group.Id, group.Users[0].GroupId);
                Assert.Equal(group.Id, group.Users[1].GroupId);
                Assert.Equal(group.Id, group.Users[2].GroupId);
                affrows = groupRepo.Delete(group);
                Assert.Equal(7, affrows);
                Assert.Equal(0, group.Id);
                Assert.Equal("group01", group.GroupName);
                Assert.Equal(0, group.Users[0].Id);
                Assert.Equal("admin01", group.Users[0].Username);
                Assert.Equal("pwd01", group.Users[0].Password);
                Assert.True(group.Users[0].UserExt.UserId > 0);
                Assert.Equal("用户备注01", group.Users[0].UserExt.Remark);
                Assert.Equal(0, group.Users[1].Id);
                Assert.Equal("admin02", group.Users[1].Username);
                Assert.Equal("pwd02", group.Users[1].Password);
                Assert.True(group.Users[1].UserExt.UserId > 0);
                Assert.Equal("用户备注02", group.Users[1].UserExt.Remark);
                Assert.Equal(0, group.Users[2].Id);
                Assert.Equal("admin03", group.Users[2].Username);
                Assert.Equal("pwd03", group.Users[2].Password);
                Assert.True(group.Users[2].UserExt.UserId > 0);
                Assert.Equal("用户备注03", group.Users[2].UserExt.Remark);
                Assert.False(groupRepo.Select.Any());
                Assert.False(userRepo.Select.Any());
                Assert.False(userextRepo.Select.Any());

                //ManyToMany InDatabase
                fsql.Delete<DeleteCascadeUserGroup>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUser>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUserExt>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeTag>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUserTag>().Where("1=1").ExecuteAffrows();
                var tags = new[] {
                new DeleteCascadeTag { TagName = "tag01" },
                new DeleteCascadeTag { TagName = "tag02" },
                new DeleteCascadeTag { TagName = "tag03" },
                new DeleteCascadeTag { TagName = "tag04" },
                new DeleteCascadeTag { TagName = "tag05" },
                new DeleteCascadeTag { TagName = "tag06" },
                new DeleteCascadeTag { TagName = "tag07" },
                new DeleteCascadeTag { TagName = "tag08" },
            };
                tagRepo.Insert(tags);
                groupRepo.DbContextOptions.EnableCascadeSave = true;
                group = new DeleteCascadeUserGroup
                {
                    GroupName = "group01",
                    Users = new List<DeleteCascadeUser>
                {
                    new DeleteCascadeUser { Username = "admin01", Password = "pwd01", UserExt = new DeleteCascadeUserExt { Remark = "用户备注01" }, Tags = new List<DeleteCascadeTag> { tags[0], tags[2], tags[3], tags[6] } },
                    new DeleteCascadeUser { Username = "admin02", Password = "pwd02", UserExt = new DeleteCascadeUserExt { Remark = "用户备注02" }, Tags = new List<DeleteCascadeTag> { tags[1], tags[2], tags[5] } },
                    new DeleteCascadeUser { Username = "admin03", Password = "pwd03", UserExt = new DeleteCascadeUserExt { Remark = "用户备注03" }, Tags = new List<DeleteCascadeTag> { tags[3], tags[4], tags[6], tags[7] } },
                }
                };
                groupRepo.Insert(group);
                Assert.Equal(group.Id, group.Users[0].GroupId);
                Assert.Equal(group.Id, group.Users[1].GroupId);
                Assert.Equal(group.Id, group.Users[2].GroupId);
                ret = groupRepo.DeleteCascadeByDatabase(a => a.Id == group.Id);
                Assert.Equal(18, ret.Count);

                Assert.IsType<DeleteCascadeUserExt>(ret[0]);
                Assert.Equal(group.Users[0].UserExt.UserId, (ret[0] as DeleteCascadeUserExt).UserId);
                Assert.Equal(group.Users[0].UserExt.Remark, (ret[0] as DeleteCascadeUserExt).Remark);
                Assert.IsType<DeleteCascadeUserExt>(ret[1]);
                Assert.Equal(group.Users[1].UserExt.UserId, (ret[1] as DeleteCascadeUserExt).UserId);
                Assert.Equal(group.Users[1].UserExt.Remark, (ret[1] as DeleteCascadeUserExt).Remark);
                Assert.IsType<DeleteCascadeUserExt>(ret[2]);
                Assert.Equal(group.Users[2].UserExt.UserId, (ret[2] as DeleteCascadeUserExt).UserId);
                Assert.Equal(group.Users[2].UserExt.Remark, (ret[2] as DeleteCascadeUserExt).Remark);

                Assert.IsType<DeleteCascadeUserTag>(ret[3]);
                Assert.Equal(group.Users[0].Id, (ret[3] as DeleteCascadeUserTag).UserId);
                Assert.Equal(tags[0].Id, (ret[3] as DeleteCascadeUserTag).TagId);
                Assert.IsType<DeleteCascadeUserTag>(ret[4]);
                Assert.Equal(group.Users[0].Id, (ret[4] as DeleteCascadeUserTag).UserId);
                Assert.Equal(tags[2].Id, (ret[4] as DeleteCascadeUserTag).TagId);
                Assert.IsType<DeleteCascadeUserTag>(ret[5]);
                Assert.Equal(group.Users[0].Id, (ret[5] as DeleteCascadeUserTag).UserId);
                Assert.Equal(tags[3].Id, (ret[5] as DeleteCascadeUserTag).TagId);
                Assert.IsType<DeleteCascadeUserTag>(ret[6]);
                Assert.Equal(group.Users[0].Id, (ret[6] as DeleteCascadeUserTag).UserId);
                Assert.Equal(tags[6].Id, (ret[6] as DeleteCascadeUserTag).TagId);

                Assert.IsType<DeleteCascadeUserTag>(ret[7]);
                Assert.Equal(group.Users[1].Id, (ret[7] as DeleteCascadeUserTag).UserId);
                Assert.Equal(tags[1].Id, (ret[7] as DeleteCascadeUserTag).TagId);
                Assert.IsType<DeleteCascadeUserTag>(ret[8]);
                Assert.Equal(group.Users[1].Id, (ret[8] as DeleteCascadeUserTag).UserId);
                Assert.Equal(tags[2].Id, (ret[8] as DeleteCascadeUserTag).TagId);
                Assert.IsType<DeleteCascadeUserTag>(ret[9]);
                Assert.Equal(group.Users[1].Id, (ret[9] as DeleteCascadeUserTag).UserId);
                Assert.Equal(tags[5].Id, (ret[9] as DeleteCascadeUserTag).TagId);

                Assert.IsType<DeleteCascadeUserTag>(ret[10]);
                Assert.Equal(group.Users[2].Id, (ret[10] as DeleteCascadeUserTag).UserId);
                Assert.Equal(tags[3].Id, (ret[10] as DeleteCascadeUserTag).TagId);
                Assert.IsType<DeleteCascadeUserTag>(ret[11]);
                Assert.Equal(group.Users[2].Id, (ret[11] as DeleteCascadeUserTag).UserId);
                Assert.Equal(tags[4].Id, (ret[11] as DeleteCascadeUserTag).TagId);
                Assert.IsType<DeleteCascadeUserTag>(ret[12]);
                Assert.Equal(group.Users[2].Id, (ret[12] as DeleteCascadeUserTag).UserId);
                Assert.Equal(tags[6].Id, (ret[12] as DeleteCascadeUserTag).TagId);
                Assert.IsType<DeleteCascadeUserTag>(ret[13]);
                Assert.Equal(group.Users[2].Id, (ret[13] as DeleteCascadeUserTag).UserId);
                Assert.Equal(tags[7].Id, (ret[13] as DeleteCascadeUserTag).TagId);

                Assert.IsType<DeleteCascadeUser>(ret[14]);
                Assert.Equal(group.Users[0].Id, (ret[14] as DeleteCascadeUser).Id);
                Assert.Equal(group.Users[0].Username, (ret[14] as DeleteCascadeUser).Username);
                Assert.Equal(group.Users[0].Password, (ret[14] as DeleteCascadeUser).Password);
                Assert.IsType<DeleteCascadeUser>(ret[15]);
                Assert.Equal(group.Users[1].Id, (ret[15] as DeleteCascadeUser).Id);
                Assert.Equal(group.Users[1].Username, (ret[15] as DeleteCascadeUser).Username);
                Assert.Equal(group.Users[1].Password, (ret[15] as DeleteCascadeUser).Password);
                Assert.IsType<DeleteCascadeUser>(ret[16]);
                Assert.Equal(group.Users[2].Id, (ret[16] as DeleteCascadeUser).Id);
                Assert.Equal(group.Users[2].Username, (ret[16] as DeleteCascadeUser).Username);
                Assert.Equal(group.Users[2].Password, (ret[16] as DeleteCascadeUser).Password);
                Assert.IsType<DeleteCascadeUserGroup>(ret[17]);
                Assert.Equal(group.Id, (ret[17] as DeleteCascadeUserGroup).Id);
                Assert.Equal(group.GroupName, (ret[17] as DeleteCascadeUserGroup).GroupName);

                //ManyToMany EnableCascadeSave InMemory
                fsql.Delete<DeleteCascadeUserGroup>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUser>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUserExt>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeTag>().Where("1=1").ExecuteAffrows();
                fsql.Delete<DeleteCascadeUserTag>().Where("1=1").ExecuteAffrows();
                tags = new[] {
                new DeleteCascadeTag { TagName = "tag01" },
                new DeleteCascadeTag { TagName = "tag02" },
                new DeleteCascadeTag { TagName = "tag03" },
                new DeleteCascadeTag { TagName = "tag04" },
                new DeleteCascadeTag { TagName = "tag05" },
                new DeleteCascadeTag { TagName = "tag06" },
                new DeleteCascadeTag { TagName = "tag07" },
                new DeleteCascadeTag { TagName = "tag08" },
            };
                tagRepo.Insert(tags);
                groupRepo.DbContextOptions.EnableCascadeSave = true;
                group = new DeleteCascadeUserGroup
                {
                    GroupName = "group01",
                    Users = new List<DeleteCascadeUser>
                {
                    new DeleteCascadeUser { Username = "admin01", Password = "pwd01", UserExt = new DeleteCascadeUserExt { Remark = "用户备注01" }, Tags = new List<DeleteCascadeTag> { tags[0], tags[2], tags[3], tags[6] } },
                    new DeleteCascadeUser { Username = "admin02", Password = "pwd02", UserExt = new DeleteCascadeUserExt { Remark = "用户备注02" }, Tags = new List<DeleteCascadeTag> { tags[1], tags[2], tags[5] } },
                    new DeleteCascadeUser { Username = "admin03", Password = "pwd03", UserExt = new DeleteCascadeUserExt { Remark = "用户备注03" }, Tags = new List<DeleteCascadeTag> { tags[3], tags[4], tags[6], tags[7] } },
                }
                };
                groupRepo.Insert(group);
                Assert.Equal(group.Id, group.Users[0].GroupId);
                Assert.Equal(group.Id, group.Users[1].GroupId);
                Assert.Equal(group.Id, group.Users[2].GroupId);
                affrows = groupRepo.Delete(group);
                Assert.Equal(18, affrows);
                Assert.Equal(0, group.Id);
                Assert.Equal("group01", group.GroupName);
                Assert.Equal(0, group.Users[0].Id);
                Assert.Equal("admin01", group.Users[0].Username);
                Assert.Equal("pwd01", group.Users[0].Password);
                Assert.True(group.Users[0].UserExt.UserId > 0);
                Assert.Equal("用户备注01", group.Users[0].UserExt.Remark);
                Assert.Equal(0, group.Users[1].Id);
                Assert.Equal("admin02", group.Users[1].Username);
                Assert.Equal("pwd02", group.Users[1].Password);
                Assert.True(group.Users[1].UserExt.UserId > 0);
                Assert.Equal("用户备注02", group.Users[1].UserExt.Remark);
                Assert.Equal(0, group.Users[2].Id);
                Assert.Equal("admin03", group.Users[2].Username);
                Assert.Equal("pwd03", group.Users[2].Password);
                Assert.True(group.Users[2].UserExt.UserId > 0);
                Assert.Equal("用户备注03", group.Users[2].UserExt.Remark);
                Assert.False(groupRepo.Select.Any());
                Assert.False(userRepo.Select.Any());
                Assert.False(userextRepo.Select.Any());
                Assert.False(fsql.Select<DeleteCascadeUserTag>().Any());
            }
        }
        public class DeleteCascadeUser
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public int GroupId { get; set; }
            public bool IsDeleted { get; set; }

            [Navigate(nameof(Id))]
            public DeleteCascadeUserExt UserExt { get; set; }
            [Navigate(ManyToMany = typeof(DeleteCascadeUserTag))]
            public List<DeleteCascadeTag> Tags { get; set; }
        }
        public class DeleteCascadeUserExt
        {
            [Column(IsPrimary = true)]
            public int UserId { get; set; }
            public string Remark { get; set; }
            public bool IsDeleted { get; set; }

            [Navigate(nameof(UserId))]
            public DeleteCascadeUser User { get; set; }
        }
        public class DeleteCascadeUserGroup
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public string GroupName { get; set; }
            public bool IsDeleted { get; set; }

            [Navigate(nameof(DeleteCascadeUser.GroupId))]
            public List<DeleteCascadeUser> Users { get; set; }
        }
        public class DeleteCascadeTag
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public string TagName { get; set; }
            public bool IsDeleted { get; set; }

            [Navigate(ManyToMany = typeof(DeleteCascadeUserTag))]
            public List<DeleteCascadeUser> Users { get; set; }
        }
        public class DeleteCascadeUserTag
        {
            public int UserId { get; set; }
            public int TagId { get; set; }
            public bool IsDeleted { get; set; }

            [Navigate(nameof(UserId))]
            public DeleteCascadeUser User { get; set; }
            [Navigate(nameof(TagId))]
            public DeleteCascadeTag Tag { get; set; }
        }

        /// <summary>
        /// 更一条无法更新。
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Updatemysql()
        {
            var item1 = new AddUpdateInfo();
            g.mysql.Insert(item1).ExecuteAffrows();
            var item2 = new AddUpdateInfo();
            g.mysql.Insert(item2).ExecuteAffrows();
            var item3 = new AddUpdateInfo();
            g.mysql.Insert(item3).ExecuteAffrows();

            var repos = g.mysql.GetRepository<AddUpdateInfo, Guid>();
            var items = repos.Select.WhereDynamic(new[] { item1, item2, item3 }).ToList();
            items[0].Title = "88";
            //items[1].Title = "88";
            items[2].Title = "88";
            var changed = repos.CompareState(items[0]);
            int x = await repos.UpdateAsync(items);
        }

        [Fact]
        public void AddUpdate()
        {
            var repos = g.sqlite.GetRepository<AddUpdateInfo, Guid>();

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

            repos.Orm.Insert(new AddUpdateInfo()).ExecuteAffrows();
            repos.Orm.Insert(new AddUpdateInfo { Id = Guid.NewGuid() }).ExecuteAffrows();
            repos.Orm.Update<AddUpdateInfo>().Set(a => a.Title == "xxx").Where(a => a.Id == item.Id).ExecuteAffrows();
            item = repos.Orm.Select<AddUpdateInfo>(item).First();
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));
            repos.Orm.Delete<AddUpdateInfo>(item).ExecuteAffrows();
        }

        [Fact]
        public void UpdateAttach()
        {
            var repos = g.sqlite.GetRepository<AddUpdateInfo, Guid>();

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
            var repos = g.sqlite.GetRepository<AddUpdateInfo, Guid>();

            var item = new AddUpdateInfo { Id = Guid.NewGuid() };
            item.Title = "xxx";
            Assert.Throws<Exception>(() => repos.Update(item));
        }

        [Fact]
        public void Update()
        {
            g.sqlite.Insert(new AddUpdateInfo()).ExecuteAffrows();

            var repos = g.sqlite.GetRepository<AddUpdateInfo, Guid>();

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
                    flowRepos.Orm.Select<FlowModel>().ToList();
                    flowRepos.Orm.Ado.ExecuteConnectTest();
                    uow.Commit();
                }
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
                    uowFlowRepos.Orm.Select<FlowModel>().ToList();
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

            var repos = g.sqlite.GetRepository<object, Guid>();
            repos.AsType(typeof(AddUpdateInfo));

            var item = new AddUpdateInfo();
            repos.Insert(item);
            repos.Update(item);

            item.Clicks += 1;
            repos.InsertOrUpdate(item);

            var item2 = repos.Find(item.Id) as AddUpdateInfo;
            Assert.Equal(item.Clicks, item2.Clicks);

            Assert.Null(repos.Find(item.Id));
        }

        [Fact]
        public void EnableCascadeSave_OneToMany()
        {
            var repo = g.sqlite.GetRepository<Cagetory>();
            repo.DbContextOptions.EnableCascadeSave = true;
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

            var cts2 = repo.Select.WhereDynamic(cts).IncludeMany(a => a.Goodss).ToList();
            cts2[0].Goodss[0].Name += 123;
            repo.Update(cts2[0]);
            cts2[0].Goodss[0].Name += 333;
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
        public void EnableCascadeSave_OneToMany_lazyloading()
        {
            var repo = g.sqlite.GetRepository<CagetoryLD>();
            repo.DbContextOptions.EnableCascadeSave = true;
            var cts = new[] {
                new CagetoryLD
                {
                    Name = "分类1",
                    Goodss = new List<GoodsLD>(new[]
                    {
                        new GoodsLD { Name = "商品1" },
                        new GoodsLD { Name = "商品2" },
                        new GoodsLD { Name = "商品3" }
                    })
                },
                new CagetoryLD
                {
                    Name = "分类2",
                    Goodss = new List<GoodsLD>(new[]
                    {
                        new GoodsLD { Name = "商品4" },
                        new GoodsLD { Name = "商品5" }
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
            cts[0].Goodss.Add(new GoodsLD { Name = "商品33" });
            cts[1].Name = "分类222";
            cts[1].Goodss.Clear();
            cts[1].Goodss.Add(new GoodsLD { Name = "商品55" });
            repo.Update(cts);

            var cts2 = repo.Select.WhereDynamic(cts).IncludeMany(a => a.Goodss).ToList();
            cts2[0].Goodss[0].Name += 123;
            repo.Update(cts2[0]);
            cts2[0].Goodss[0].Name += 333;

            cts2 = repo.Select.WhereDynamic(cts).ToList();
            cts2[0].Goodss[0].Name += 123;
            repo.Update(cts2[0]);
            cts2[0].Goodss[0].Name += 333;
        }
        [Table(Name = "EAUNL_OTM_CTLD")]
        public class CagetoryLD
        {
            public Guid Id { get; set; }
            public string Name { get; set; }

            [Navigate("CagetoryId")]
            public virtual List<GoodsLD> Goodss { get; set; }
        }
        [Table(Name = "EAUNL_OTM_GDLD")]
        public class GoodsLD
        {
            public Guid Id { get; set; }
            public Guid CagetoryId { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void EnableCascadeSave_OneToMany_Parent()
        {
            g.sqlite.Delete<CagetoryParent>().Where("1=1").ExecuteAffrows();
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
            repo.DbContextOptions.EnableCascadeSave = true; //打开级联保存功能
            repo.Insert(cts);

            var notreelist1 = repo.Select.ToList();
            var treelist1 = repo.Select.ToTreeList();

            //repo.SaveMany(cts[0], "Childs"); //指定保存 Childs 一对多属性
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
            var treelist2 = repo.Select.ToTreeList();
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
        public void EnableCascadeSave_ManyToMany()
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
            repo.DbContextOptions.EnableCascadeSave = true; //打开级联保存功能
            repo.Insert(ss);

            ss[0].Tags[0].TagName = "流行101";

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

        [Fact]
        public void BeginEdit()
        {
            g.sqlite.Delete<BeginEdit01>().Where("1=1").ExecuteAffrows();
            var repo = g.sqlite.GetRepository<BeginEdit01>();
            var cts = new[] {
                new BeginEdit01 { Name = "分类1" },
                new BeginEdit01 { Name = "分类1_1" },
                new BeginEdit01 { Name = "分类1_2" },
                new BeginEdit01 { Name = "分类1_3" },
                new BeginEdit01 { Name = "分类2" },
                new BeginEdit01 { Name = "分类2_1" },
                new BeginEdit01 { Name = "分类2_2" }
            }.ToList();
            repo.Insert(cts);

            repo.BeginEdit(cts);

            cts.Add(new BeginEdit01 { Name = "分类2_3" });
            cts[0].Name = "123123";
            cts.RemoveAt(1);

            Assert.Equal(3, repo.EndEdit());

            g.sqlite.Delete<BeginEdit01>().Where("1=1").ExecuteAffrows();
            repo = g.sqlite.GetRepository<BeginEdit01>();
            cts = repo.Select.ToList();
            repo.BeginEdit(cts);

            cts.AddRange(new[] {
                new BeginEdit01 { Name = "分类1" },
                new BeginEdit01 { Name = "分类1_1" },
                new BeginEdit01 { Name = "分类1_2" },
                new BeginEdit01 { Name = "分类1_3" },
                new BeginEdit01 { Name = "分类2" },
                new BeginEdit01 { Name = "分类2_1" },
                new BeginEdit01 { Name = "分类2_2" }
            });

            Assert.Equal(7, repo.EndEdit());
        }
        class BeginEdit01
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }
        [Fact]
        public void BeginEditIdentity()
        {
            var fsql = g.sqlserver;
            fsql.Delete<BeginEdit02>().Where("1=1").ExecuteAffrows();
            var repo = fsql.GetRepository<BeginEdit02>();
            var cts = new[] {
                new BeginEdit02 { Name = "分类1" },
                new BeginEdit02 { Name = "分类1_1" },
                new BeginEdit02 { Name = "分类1_2" },
                new BeginEdit02 { Name = "分类1_3" },
                new BeginEdit02 { Name = "分类2" },
                new BeginEdit02 { Name = "分类2_1" },
                new BeginEdit02 { Name = "分类2_2" }
            }.ToList();
            repo.Insert(cts);

            repo.BeginEdit(cts);

            cts.Add(new BeginEdit02 { Name = "分类2_3" });
            cts[0].Name = "123123";
            cts.RemoveAt(1);

            Assert.Equal(3, repo.EndEdit());

            fsql.Delete<BeginEdit02>().Where("1=1").ExecuteAffrows();
            repo = fsql.GetRepository<BeginEdit02>();
            cts = repo.Select.ToList();
            repo.BeginEdit(cts);

            cts.AddRange(new[] {
                new BeginEdit02 { Name = "分类1" },
                new BeginEdit02 { Name = "分类1_1" },
                new BeginEdit02 { Name = "分类1_2" },
                new BeginEdit02 { Name = "分类1_3" },
                new BeginEdit02 { Name = "分类2" },
                new BeginEdit02 { Name = "分类2_1" },
                new BeginEdit02 { Name = "分类2_2" }
            });

            Assert.Equal(7, repo.EndEdit());
        }
        class BeginEdit02
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public string Name { get; set; }
            [Column(ServerTime = DateTimeKind.Utc)]
            public DateTime UpdateTime { get; set; }
        }

        [Fact]
        public void OrmScoped()
        {
            var fsql = g.sqlserver;
            //fsql.Aop.CommandBefore += (s, e) =>
            //{
            //    Console.WriteLine(e.Command.CommandText);
            //};

            var repo = fsql.GetRepository<ts_repo_update_bit>();
            repo.Orm.Ado.ExecuteNonQuery("select 1");

            using (var ctx = fsql.CreateDbContext())
            {
                ctx.Orm.Ado.ExecuteNonQuery("select 1");
            }

            using (var uow = fsql.CreateUnitOfWork())
            {
                uow.Orm.Ado.ExecuteNonQuery("select 1");
            }

            using (var uow = fsql.CreateUnitOfWork())
            {
                repo.UnitOfWork = uow;
                repo.Orm.Ado.ExecuteNonQuery("select 1");
            }
        }

        [Fact]
        public void UpdateBit()
        {
            var fsql = g.sqlserver;

            fsql.Delete<ts_repo_update_bit>().Where("1=1").ExecuteAffrows();
            var id = fsql.Insert(new ts_repo_update_bit()).ExecuteIdentity();
            Assert.True(id > 0);
            var repo = fsql.GetRepository<ts_repo_update_bit>();
            var item = repo.Select.WhereDynamic(id).First();
            Assert.Equal(item.id, id);
            Assert.False(item.bool_val);

            item.bool_val = true;
            repo.Update(item);
            item = repo.Select.WhereDynamic(id).First();
            Assert.Equal(item.id, id);
            Assert.True(item.bool_val);

            item.bool_val = false;
            repo.Update(item);
            item = repo.Select.WhereDynamic(id).First();
            Assert.Equal(item.id, id);
            Assert.False(item.bool_val);

            item.bool_val = false;
            repo.Update(item);
            item = repo.Select.WhereDynamic(id).First();
            Assert.Equal(item.id, id);
            Assert.False(item.bool_val);



            item.bool_val = true;
            repo.InsertOrUpdate(item);
            item = repo.Select.WhereDynamic(id).First();
            Assert.Equal(item.id, id);
            Assert.True(item.bool_val);

            item.bool_val = false;
            repo.InsertOrUpdate(item);
            item = repo.Select.WhereDynamic(id).First();
            Assert.Equal(item.id, id);
            Assert.False(item.bool_val);

            item.bool_val = false;
            repo.InsertOrUpdate(item);
            item = repo.Select.WhereDynamic(id).First();
            Assert.Equal(item.id, id);
            Assert.False(item.bool_val);



            repo.InsertOrUpdate(new ts_repo_update_bit { id = item.id, bool_val = true });
            item = repo.Select.WhereDynamic(id).First();
            Assert.Equal(item.id, id);
            Assert.True(item.bool_val);

            repo.InsertOrUpdate(new ts_repo_update_bit { id = item.id, bool_val = false });
            item = repo.Select.WhereDynamic(id).First();
            Assert.Equal(item.id, id);
            Assert.False(item.bool_val);

            repo.InsertOrUpdate(new ts_repo_update_bit { id = item.id, bool_val = false });
            item = repo.Select.WhereDynamic(id).First();
            Assert.Equal(item.id, id);
            Assert.False(item.bool_val);
        }
        class ts_repo_update_bit
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public bool bool_val { get; set; }
        }

        [Fact]
        public void InsertIdentity()
        {
            var fsql = g.mysql;
            fsql.Delete<TaskDetailTeam>().Where("1=1").ExecuteAffrows();

            var repo = fsql.GetRepository<TaskDetailTeam>();
            
            var team = new TaskDetailTeam();
            repo.Insert(team);

            team = new TaskDetailTeam
            {
                TaskId = 1,
                UserId = 11,
                IsYanShou = 1,
                AccessType = "xxxAccessType1"
            };
            repo.Insert(team);

            var teams = new[]
            {
                new TaskDetailTeam
                {
                    TaskId = 2,
                    UserId = 22,
                    IsYanShou = 2,
                    AccessType = "xxxAccessType2"
                },new TaskDetailTeam
                {
                    TaskId = 3,
                    UserId = 33,
                    IsYanShou = 3,
                    AccessType = "xxxAccessType3"
                }
            };
            repo.Insert(teams);
        }

        [Table(Name = "task_detail_team")]
        public class TaskDetailTeam
        {
            [Column(Name = "id", IsPrimary = true, IsIdentity = true)]
            public int Id { get; set; }

            [Column(Name = "createdAt", DbType = "datetime", ServerTime = DateTimeKind.Local, CanUpdate = false)]
            public DateTime CreatedAt { get; set; }

            [Column(Name = "taskId")]
            public int TaskId { get; set; }

            [Column(Name = "updatedAt", DbType = "datetime", ServerTime = DateTimeKind.Local)]
            public DateTime UpdatedAt { get; set; }

            [Column(Name = "userId")]
            public int UserId { get; set; }

            [Column(Name = "is_yanshou")]
            public int IsYanShou { get; set; }

            [Column(IsIgnore = true)]
            public string AccessType { get; set; }
        }
    }
}
