using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace FreeSql.Tests.PostgreSQL
{
    public class PostgreSQLPgArrayToManyTest
    {

        [Table(Name = "pgarray_tomany_user_lazyloading")]
        public class UserLazyLoading
        {
            [Column(IsPrimary = true)]
            public int UserId { get; set; }
            public int[] RoleIds { get; set; }
            public string UserName { get; set; }

            [Navigate(nameof(RoleIds))]
            public virtual List<RoleLazyLoading> Roles { get; set; }
        }

        [Table(Name = "pgarray_tomany_role_lazyloading")]
        public class RoleLazyLoading
        {
            [Column(IsPrimary = true)]
            public int RoleId { get; set; }
            public string RoleName { get; set; }

            [Navigate(nameof(UserLazyLoading.RoleIds))]
            public virtual List<UserLazyLoading> Users { get; set; }
        }

        [Fact]
        public void LazyLoading()
        {
            var fsql = g.pgsql;
            fsql.Delete<UserLazyLoading>().Where("1=1").ExecuteAffrows();
            fsql.Delete<RoleLazyLoading>().Where("1=1").ExecuteAffrows();

            var roles = new[]
            {
                new RoleLazyLoading { RoleId = 1, RoleName = "role1" },
                new RoleLazyLoading { RoleId = 2, RoleName = "role2" },
                new RoleLazyLoading { RoleId = 3, RoleName = "role3" }
            };
            Assert.Equal(3, fsql.Insert(roles).ExecuteAffrows());
            var users = new[]
            {
                new UserLazyLoading { UserId = 11, RoleIds = new [] { 1,2 }, UserName = "user1" },
                new UserLazyLoading { UserId = 12, RoleIds = new [] { 1,2,3 }, UserName = "user2" },
                new UserLazyLoading { UserId = 13, RoleIds = new [] { 1,3 }, UserName = "user3" },
                new UserLazyLoading { UserId = 14, RoleIds = new [] { 3,2,1 }, UserName = "user4" },
                new UserLazyLoading { UserId = 15, RoleIds = null, UserName = "user5" },
                new UserLazyLoading { UserId = 16, RoleIds = new int[0], UserName = "user6" },
            };
            Assert.Equal(6, fsql.Insert(users).ExecuteAffrows());

            var roles3 = fsql.Select<RoleLazyLoading>().IncludeMany(a => a.Users).ToList();
            Assert.Equal(3, roles3.Count);
            var roles2 = roles3;
            Assert.Equal(1, roles2[0].RoleId);
            Assert.Equal("role1", roles2[0].RoleName);
            Assert.Equal(2, roles2[1].RoleId);
            Assert.Equal("role2", roles2[1].RoleName);
            Assert.Equal(3, roles2[2].RoleId);
            Assert.Equal("role3", roles2[2].RoleName);

            var users2 = roles2[0].Users;
            Assert.Equal(4, users2.Count);
            Assert.Equal(11, users2[0].UserId);
            Assert.Equal("user1", users2[0].UserName);
            Assert.Equal("1,2", string.Join(",", users2[0].RoleIds));
            Assert.Equal(12, users2[1].UserId);
            Assert.Equal("user2", users2[1].UserName);
            Assert.Equal("1,2,3", string.Join(",", users2[1].RoleIds));
            Assert.Equal(13, users2[2].UserId);
            Assert.Equal("user3", users2[2].UserName);
            Assert.Equal("1,3", string.Join(",", users2[2].RoleIds));
            Assert.Equal(14, users2[3].UserId);
            Assert.Equal("user4", users2[3].UserName);
            Assert.Equal("3,2,1", string.Join(",", users2[3].RoleIds));

            users2 = roles2[1].Users;
            Assert.Equal(3, users2.Count);
            Assert.Equal(11, users2[0].UserId);
            Assert.Equal("user1", users2[0].UserName);
            Assert.Equal("1,2", string.Join(",", users2[0].RoleIds));
            Assert.Equal(12, users2[1].UserId);
            Assert.Equal("user2", users2[1].UserName);
            Assert.Equal("1,2,3", string.Join(",", users2[1].RoleIds));
            Assert.Equal(14, users2[2].UserId);
            Assert.Equal("user4", users2[2].UserName);
            Assert.Equal("3,2,1", string.Join(",", users2[2].RoleIds));

            users2 = roles2[2].Users;
            Assert.Equal(3, users2.Count);
            Assert.Equal(12, users2[0].UserId);
            Assert.Equal("user2", users2[0].UserName);
            Assert.Equal("1,2,3", string.Join(",", users2[0].RoleIds));
            Assert.Equal(13, users2[1].UserId);
            Assert.Equal("user3", users2[1].UserName);
            Assert.Equal("1,3", string.Join(",", users2[1].RoleIds));
            Assert.Equal(14, users2[2].UserId);
            Assert.Equal("user4", users2[2].UserName);
            Assert.Equal("3,2,1", string.Join(",", users2[2].RoleIds));

            var role = fsql.Select<RoleLazyLoading>().Where(a => a.RoleId == 1).First();
            Assert.IsNotType<RoleLazyLoading>(role);

            users2 = role.Users;
            Assert.Equal(4, users2.Count);
            Assert.Equal(11, users2[0].UserId);
            Assert.Equal("user1", users2[0].UserName);
            Assert.Equal("1,2", string.Join(",", users2[0].RoleIds));
            Assert.Equal(12, users2[1].UserId);
            Assert.Equal("user2", users2[1].UserName);
            Assert.Equal("1,2,3", string.Join(",", users2[1].RoleIds));
            Assert.Equal(13, users2[2].UserId);
            Assert.Equal("user3", users2[2].UserName);
            Assert.Equal("1,3", string.Join(",", users2[2].RoleIds));
            Assert.Equal(14, users2[3].UserId);
            Assert.Equal("user4", users2[3].UserName);
            Assert.Equal("3,2,1", string.Join(",", users2[3].RoleIds));

            roles2 = users2[0].Roles;
            Assert.Equal(2, roles2.Count);
            Assert.Equal(1, roles2[0].RoleId);
            Assert.Equal("role1", roles2[0].RoleName);
            Assert.Equal(2, roles2[1].RoleId);
            Assert.Equal("role2", roles2[1].RoleName);

            roles2 = users2[1].Roles;
            Assert.Equal(3, roles2.Count);
            Assert.Equal(1, roles2[0].RoleId);
            Assert.Equal("role1", roles2[0].RoleName);
            Assert.Equal(2, roles2[1].RoleId);
            Assert.Equal("role2", roles2[1].RoleName);
            Assert.Equal(3, roles2[2].RoleId);
            Assert.Equal("role3", roles2[2].RoleName);

            roles2 = users2[2].Roles;
            Assert.Equal(2, roles2.Count);
            Assert.Equal(1, roles2[0].RoleId);
            Assert.Equal("role1", roles2[0].RoleName);
            Assert.Equal(3, roles2[1].RoleId);
            Assert.Equal("role3", roles2[1].RoleName);

            roles2 = users2[3].Roles;
            Assert.Equal(3, roles2.Count);
            Assert.Equal(3, roles2[0].RoleId);
            Assert.Equal("role3", roles2[0].RoleName);
            Assert.Equal(2, roles2[1].RoleId);
            Assert.Equal("role2", roles2[1].RoleName);
            Assert.Equal(1, roles2[2].RoleId);
            Assert.Equal("role1", roles2[2].RoleName);

            var user = fsql.Select<UserLazyLoading>().Where(a => a.UserId == 11).First();
            Assert.IsNotType<UserLazyLoading>(user);

            roles2 = user.Roles;
            Assert.Equal(2, roles2.Count);
            Assert.Equal(1, roles2[0].RoleId);
            Assert.Equal("role1", roles2[0].RoleName);
            Assert.Equal(2, roles2[1].RoleId);
            Assert.Equal("role2", roles2[1].RoleName);
        }
    }
}
