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
        [Fact]
        public void Test1v1()
        {
            using (var fsql = g.CreateMemory())
            {
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
            }
        }
        class User
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            [Navigate(nameof(Id))]
            public UserExt Ext { get; set; }
        }
        class UserExt
        {
            [Column(IsPrimary = true)]
            public int UserId { get; set; }
            public string Remark { get; set; }
            [Navigate(nameof(UserId))]
            public User Org { get; set; }
        }
    }
}
