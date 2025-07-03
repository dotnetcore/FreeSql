using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FreeSql.Tests.DbContext2
{
    public class AggregateRootRepositoryTest
    {
        class UserRepository : AggregateRootRepository<User>
        {
            public UserRepository(IFreeSql fsql, UnitOfWorkManager uowManager) : base(uowManager?.Orm ?? fsql)
            {
            }

            public override ISelect<User> Select => base.SelectDiy;
        }

        [Fact]
        public void Test1v1()
        {
            using (var fsql = g.CreateMemory())
            {
                new UserRepository(fsql, null);

                var code = AggregateRootUtils.GetAutoIncludeQueryStaicCode(null, fsql, typeof(User));
                Assert.Equal(@"//fsql.Select<User>()
SelectDiy
    .Include(a => a.Ext)", code);

                var repo = fsql.GetAggregateRootRepository<User>();
                var user = new User
                {
                    UserName = "admin01",
                    Password = "admin01_pwd",
                    Ext = new UserExt
                    {
                        Remark = "admin01_remark"
                    }
                };
                repo.Insert(user);
                Assert.Equal(1, user.Id);
                Assert.Equal(1, user.Ext.UserId);

                user = repo.Where(a => a.Id == 1).First();
                Assert.NotNull(user);
                Assert.Equal(1, user.Id);
                Assert.Equal("admin01", user.UserName);
                Assert.Equal("admin01_pwd", user.Password);
                Assert.NotNull(user.Ext);
                Assert.Equal(1, user.Ext.UserId);
                Assert.Equal("admin01_remark", user.Ext.Remark);

                var users = new[]{
                    new User
                    {
                        UserName = "admin02",
                        Password = "admin02_pwd",
                        Ext = new UserExt
                        {
                            Remark = "admin02_remark"
                        }
                    },
                    new User
                    {
                        UserName = "admin03",
                        Password = "admin03_pwd",
                        Ext = new UserExt
                        {
                            Remark = "admin03_remark"
                        }
                    },
                };
                repo.Insert(users);
                Assert.Equal(2, users[0].Id);
                Assert.Equal(2, users[0].Ext.UserId);
                Assert.Equal(3, users[1].Id);
                Assert.Equal(3, users[1].Ext.UserId);

                users = repo.Where(a => a.Id > 1).ToList().ToArray();
                Assert.Equal(2, users.Length);
                Assert.Equal(2, users[0].Id);
                Assert.Equal("admin02", users[0].UserName);
                Assert.Equal("admin02_pwd", users[0].Password);
                Assert.NotNull(users[0].Ext);
                Assert.Equal(2, users[0].Ext.UserId);
                Assert.Equal("admin02_remark", users[0].Ext.Remark);
                Assert.Equal(3, users[1].Id);
                Assert.Equal("admin03", users[1].UserName);
                Assert.Equal("admin03_pwd", users[1].Password);
                Assert.NotNull(users[1].Ext);
                Assert.Equal(3, users[1].Ext.UserId);
                Assert.Equal("admin03_remark", users[1].Ext.Remark);

                user.Ext.Remark = "admin01_remark changed01";
                repo.Update(user);
                user = repo.Where(a => a.Id == 1).First();
                Assert.NotNull(user);
                Assert.Equal(1, user.Id);
                Assert.Equal("admin01", user.UserName);
                Assert.Equal("admin01_pwd", user.Password);
                Assert.NotNull(user.Ext);
                Assert.Equal(1, user.Ext.UserId);
                Assert.Equal("admin01_remark changed01", user.Ext.Remark);

                var affrows = repo.Delete(user);
                Assert.Equal(2, affrows);
                Assert.False(fsql.Select<User>().Where(a => a.Id == 1).Any());
                Assert.False(fsql.Select<UserExt>().Where(a => a.UserId == 1).Any());

                var deleted = repo.DeleteCascadeByDatabase(a => a.Id == 2 || a.Id == 3);
                Assert.NotNull(deleted);
                Assert.Equal(4, deleted.Count);
                Assert.False(fsql.Select<User>().Where(a => a.Id == 2).Any());
                Assert.False(fsql.Select<UserExt>().Where(a => a.UserId == 2).Any());
                Assert.False(fsql.Select<User>().Where(a => a.Id == 3).Any());
                Assert.False(fsql.Select<UserExt>().Where(a => a.UserId == 3).Any());
                users = new[]
                {
                    (User)deleted[3],
                    (User)deleted[1],
                };
                users[0].Ext = (UserExt)deleted[2];
                users[1].Ext = (UserExt)deleted[0];
                Assert.Equal(2, users.Length);
                Assert.Equal(2, users[0].Id);
                Assert.Equal("admin02", users[0].UserName);
                Assert.Equal("admin02_pwd", users[0].Password);
                Assert.NotNull(users[0].Ext);
                Assert.Equal(2, users[0].Ext.UserId);
                Assert.Equal("admin02_remark", users[0].Ext.Remark);
                Assert.Equal(3, users[1].Id);
                Assert.Equal("admin03", users[1].UserName);
                Assert.Equal("admin03_pwd", users[1].Password);
                Assert.NotNull(users[1].Ext);
                Assert.Equal(3, users[1].Ext.UserId);
                Assert.Equal("admin03_remark", users[1].Ext.Remark);

            }
        }
        class User
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public UserExt Ext { get; set; }
        }
        class UserExt
        {
            [Column(IsPrimary = true)]
            public int UserId { get; set; }
            public string Remark { get; set; }
            public User Org { get; set; }
        }
    }
}
