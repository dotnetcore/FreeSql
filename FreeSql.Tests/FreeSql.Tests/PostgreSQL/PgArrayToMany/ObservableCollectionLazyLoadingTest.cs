using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace FreeSql.Tests.PostgreSQL.PgArrayToMany
{
    public class ObservableCollectionLazyLoadingTest
    {

        [Table(Name = "pgarray_tomany_user_observablecollection_lazyloading")]
        public class User
        {
            [Column(IsPrimary = true)]
            public int UserId { get; set; }
            public int[] RoleIds { get; set; }
            public string UserName { get; set; }

            [Navigate(nameof(RoleIds))]
            public virtual List<Role> Roles { get; set; }
        }

        [Table(Name = "pgarray_tomany_role_observablecollection_lazyloading")]
        public class Role
        {
            [Column(IsPrimary = true)]
            public int RoleId { get; set; }
            public string RoleName { get; set; }

            [Navigate(nameof(User.RoleIds))]
            public virtual List<User> Users { get; set; }
        }

        [Fact]
        public void ObservableCollectionLazyLoading()
        {
            var fsql = g.pgsql;
            fsql.Delete<User>().Where("1=1").ExecuteAffrows();
            fsql.Delete<Role>().Where("1=1").ExecuteAffrows();

            var roles = new[]
            {
                new Role { RoleId = 1, RoleName = "role1" },
                new Role { RoleId = 2, RoleName = "role2" },
                new Role { RoleId = 3, RoleName = "role3" }
            };
            Assert.Equal(3, fsql.Insert(roles).ExecuteAffrows());
            var users = new[]
            {
                new User { UserId = 11, RoleIds = new [] { 1,2 }, UserName = "user1" },
                new User { UserId = 12, RoleIds = new [] { 1,2,3 }, UserName = "user2" },
                new User { UserId = 13, RoleIds = new [] { 1,3 }, UserName = "user3" },
                new User { UserId = 14, RoleIds = new [] { 3,2,1 }, UserName = "user4" },
                new User { UserId = 15, RoleIds = null, UserName = "user5" },
                new User { UserId = 16, RoleIds = new int[0], UserName = "user6" },
            };
            Assert.Equal(6, fsql.Insert(users).ExecuteAffrows());

            var users5Select = fsql.Select<User>().Where(a => a.Roles.Any(b => b.RoleName == "role1"));
            Assert.Equal(@"SELECT a.""userid"", a.""roleids"", a.""username"" 
FROM ""pgarray_tomany_user_observablecollection_lazyloading"" a 
WHERE (exists(SELECT 1 
    FROM ""pgarray_tomany_role_observablecollection_lazyloading"" sub_b 
    WHERE (a.""roleids"" @> ARRAY[sub_b.""roleid""]::int4[]) AND (sub_b.""rolename"" = 'role1') 
    limit 1))", users5Select.ToSql());
            var users5 = users5Select.ToList();
            Assert.Equal(4, users5.Count);
            users5Select = fsql.Select<User>().Where(a => a.Roles.AsSelect().Any(b => b.RoleName == "role1"));
            Assert.Equal(@"SELECT a.""userid"", a.""roleids"", a.""username"" 
FROM ""pgarray_tomany_user_observablecollection_lazyloading"" a 
WHERE (exists(SELECT 1 
    FROM ""pgarray_tomany_role_observablecollection_lazyloading"" b 
    WHERE (b.""rolename"" = 'role1') AND (a.""roleids"" @> ARRAY[b.""roleid""]::int4[]) 
    limit 1))", users5Select.ToSql());
            users5 = users5Select.ToList();
            Assert.Equal(4, users5.Count);

            var roles5Select = fsql.Select<Role>().Where(a => a.Users.Any(b => b.UserName == "user1"));
            Assert.Equal(@"SELECT a.""roleid"", a.""rolename"" 
FROM ""pgarray_tomany_role_observablecollection_lazyloading"" a 
WHERE (exists(SELECT 1 
    FROM ""pgarray_tomany_user_observablecollection_lazyloading"" sub_b 
    WHERE (sub_b.""roleids"" @> ARRAY[a.""roleid""]::int4[]) AND (sub_b.""username"" = 'user1') 
    limit 1))", roles5Select.ToSql());
            var roles5 = roles5Select.ToList();
            Assert.Equal(2, roles5.Count);
            roles5Select = fsql.Select<Role>().Where(a => a.Users.AsSelect().Any(b => b.UserName == "user1"));
            Assert.Equal(@"SELECT a.""roleid"", a.""rolename"" 
FROM ""pgarray_tomany_role_observablecollection_lazyloading"" a 
WHERE (exists(SELECT 1 
    FROM ""pgarray_tomany_user_observablecollection_lazyloading"" b 
    WHERE (b.""username"" = 'user1') AND (b.""roleids"" @> ARRAY[a.""roleid""]::int4[]) 
    limit 1))", roles5Select.ToSql());
            roles5 = roles5Select.ToList();
            Assert.Equal(2, roles5.Count);


            var users4 = fsql.Select<User>().IncludeMany(a => a.Roles).ToList(a => new
            {
                user = a, roles = a.Roles
            });
            var roles4 = fsql.Select<Role>().IncludeMany(a => a.Users).ToList(a => new
            {
                role = a,
                users = a.Users
            });


            var users3 = fsql.Select<User>().IncludeMany(a => a.Roles).ToList();
            Assert.Equal(6, users3.Count);
            var users2 = users3;
            Assert.Equal(11, users2[0].UserId);
            Assert.Equal(12, users2[1].UserId);
            Assert.Equal(13, users2[2].UserId);
            Assert.Equal(14, users2[3].UserId);
            Assert.Equal(15, users2[4].UserId);
            Assert.Equal(16, users2[5].UserId);
            Assert.Equal("user1", users2[0].UserName);
            Assert.Equal("user2", users2[1].UserName);
            Assert.Equal("user3", users2[2].UserName);
            Assert.Equal("user4", users2[3].UserName);
            Assert.Equal("user5", users2[4].UserName);
            Assert.Equal("user6", users2[5].UserName);
            Assert.Equal("1,2", string.Join(",", users2[0].RoleIds));
            Assert.Equal("1,2,3", string.Join(",", users2[1].RoleIds));
            Assert.Equal("1,3", string.Join(",", users2[2].RoleIds));
            Assert.Equal("3,2,1", string.Join(",", users2[3].RoleIds));
            Assert.Null(users2[4].RoleIds);
            Assert.Empty(users2[5].RoleIds);

            var roles2 = users3[0].Roles;
            Assert.Equal(1, roles2[0].RoleId);
            Assert.Equal("role1", roles2[0].RoleName);
            Assert.Equal(2, roles2[1].RoleId);
            Assert.Equal("role2", roles2[1].RoleName);

            roles2 = users3[1].Roles;
            Assert.Equal(1, roles2[0].RoleId);
            Assert.Equal("role1", roles2[0].RoleName);
            Assert.Equal(2, roles2[1].RoleId);
            Assert.Equal("role2", roles2[1].RoleName);
            Assert.Equal(3, roles2[2].RoleId);
            Assert.Equal("role3", roles2[2].RoleName);

            roles2 = users3[2].Roles;
            Assert.Equal(1, roles2[0].RoleId);
            Assert.Equal("role1", roles2[0].RoleName);
            Assert.Equal(3, roles2[1].RoleId);
            Assert.Equal("role3", roles2[1].RoleName);

            roles2 = users3[3].Roles;
            Assert.Equal(3, roles2[0].RoleId);
            Assert.Equal("role3", roles2[0].RoleName);
            Assert.Equal(2, roles2[1].RoleId);
            Assert.Equal("role2", roles2[1].RoleName);
            Assert.Equal(1, roles2[2].RoleId);
            Assert.Equal("role1", roles2[2].RoleName);

            Assert.Null(users3[4].Roles);
            Assert.Empty(users3[5].Roles);

            var roles3 = fsql.Select<Role>().IncludeMany(a => a.Users).ToList();
            Assert.Equal(3, roles3.Count);
            roles2 = roles3;
            Assert.Equal(1, roles2[0].RoleId);
            Assert.Equal("role1", roles2[0].RoleName);
            Assert.Equal(2, roles2[1].RoleId);
            Assert.Equal("role2", roles2[1].RoleName);
            Assert.Equal(3, roles2[2].RoleId);
            Assert.Equal("role3", roles2[2].RoleName);

            users2 = roles2[0].Users;
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

            var role = fsql.Select<Role>().Where(a => a.RoleId == 1).First();
            Assert.IsNotType<Role>(role);

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

            var user = fsql.Select<User>().Where(a => a.UserId == 11).First();
            Assert.IsNotType<User>(user);

            roles2 = user.Roles;
            Assert.Equal(2, roles2.Count);
            Assert.Equal(1, roles2[0].RoleId);
            Assert.Equal("role1", roles2[0].RoleName);
            Assert.Equal(2, roles2[1].RoleId);
            Assert.Equal("role2", roles2[1].RoleName);
        }
    }
}
