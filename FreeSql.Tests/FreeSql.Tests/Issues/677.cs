using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.Issues
{
	public class _677
	{
		[Fact]
		public void IncludeMany()
		{
            var fsql = g.sqlite;
			fsql.Delete<Orders>().Where("1=1").ExecuteAffrows();
			fsql.Delete<Users>().Where("1=1").ExecuteAffrows();
			fsql.Delete<Roles>().Where("1=1").ExecuteAffrows();

			fsql.Insert(new[]
			{
				new Users{ Id = 1, UserName = "user1"},
				new Users{ Id = 2, UserName = "user2"},
				new Users{ Id = 3, UserName = "user3"},
			}).NoneParameter().ExecuteAffrows();
			fsql.Insert(new[]
			{
				new Roles{ Id = 1, RoleName = "role1"},
				new Roles{ Id = 2, RoleName = "role2"},
				new Roles{ Id = 3, RoleName = "role3"},
			}).NoneParameter().ExecuteAffrows();
			fsql.Insert(new[]
			{
				new Orders{ Id = 1 },
				new Orders{ Id = 2 },
				new Orders{ Id = 3 },
				new Orders{ Id = 4 },
				new Orders{ Id = 5 },
				new Orders{ Id = 6 },
			}).NoneParameter().ExecuteAffrows();

			var userAndRoleSql = fsql.Select<Users>()
				.As("u") //别名
				.From<Roles>((_, r) => _) //其他表别名
				.InnerJoin((a, b) => a.Id == b.Id)
				.ToSql((a, b) => new
				{
					OrderId = 1, //订单id
					a.UserName,
					b.RoleName
				}, FieldAliasOptions.AsProperty);

			Assert.Equal(@"SELECT 1 ""OrderId"", u.""UserName"", r.""RoleName"" 
FROM ""Users_677"" u 
INNER JOIN ""Roles_677"" r ON u.""Id"" = r.""Id""", userAndRoleSql);

			var items = fsql.Select<Orders>()
				.AsTable((tp, old) => tp == typeof(UserAndRole) ? $"({userAndRoleSql})" : old)
				.IncludeMany(order => order.UserAndRoles)
				.ToList();
			Assert.Equal(6, items.Count);
			Assert.Equal(3, items[0].UserAndRoles.Count);
			Assert.Equal("user1", items[0].UserAndRoles[0].UserName);
			Assert.Equal("user2", items[0].UserAndRoles[1].UserName);
			Assert.Equal("user3", items[0].UserAndRoles[2].UserName);
			Assert.Equal("role1", items[0].UserAndRoles[0].RoleName);
			Assert.Equal("role2", items[0].UserAndRoles[1].RoleName);
			Assert.Equal("role3", items[0].UserAndRoles[2].RoleName);

			Assert.Empty(items[1].UserAndRoles);
			Assert.Empty(items[2].UserAndRoles);
			Assert.Empty(items[3].UserAndRoles);
			Assert.Empty(items[4].UserAndRoles);
			Assert.Empty(items[5].UserAndRoles);
		}

		[Table(Name = "Orders_677")]
		class Orders
		{
			public int Id { get; set; }
			[Navigate(nameof(UserAndRole.OrderId))]
			public List<UserAndRole> UserAndRoles { get; set; }
		}
		[Table(Name = "Users_677")]
		class Users
		{
			public int Id { get; set; }
			public string UserName { get; set; }
		}
		[Table(Name = "Roles_677")]
		class Roles
		{
			public int Id { get; set; }
			public string RoleName { get; set; }
		}
		[Table(DisableSyncStructure = true)]
		class UserAndRole
		{
			public int OrderId { get; set; }
			public string UserName { get; set; }
			public string RoleName { get; set; }
		}
	}
}
