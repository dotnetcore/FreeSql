
using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Extensions.ZeroEntity;
using FreeSql.Internal.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text.Json;

using (var fsql = new FreeSqlBuilder()
	.UseConnectionString(DataType.Sqlite, "data source=111.db")
	.UseAutoSyncStructure(true)
	.UseNoneCommandParameter(true)
	.UseMonitorCommand(cmd => Console.WriteLine(cmd.CommandText + "\r\n"))
	.Build())
{
	var json = JsonConvert.SerializeObject(Helper.GetTestDesc());

	var dyctx = new ZeroDbContext(fsql, JsonConvert.DeserializeObject<TableDescriptor[]>(@"
[
	{
		""Name"":""User"",
		""Comment"":""用户表"",
		""Columns"": [
			{""Name"":""Id"",""IsPrimary"":true,""IsIdentity"":true,""MapType"":""System.Int32""},
			{""Name"":""Name"",""MapType"":""System.String""}
		],
		""Navigates"":[
			{""Name"":""Ext"",""Type"":""OneToOne"",""RelTable"":""UserExt""},
			{""Name"":""Claims"",""Type"":""OneToMany"",""RelTable"":""UserClaim"",""Bind"":""UserId""},
			{""Name"":""Roles"",""Type"":""ManyToMany"",""RelTable"":""Role"",""ManyToMany"":""UserRole""}
		],
		""Indexes"":[]
	},
	{
		""Name"":""UserExt"",
		""Comment"":""用户扩展信息表"",
		""Columns"":[
			{""Name"":""UserId"",""IsPrimary"":true,""MapType"":""System.Int32""},
		],
		""Navigates"":[
			{""Name"":""Remarks"",""Type"":""OneToMany"",""RelTable"":""UserExtRemarks"",""Bind"":""UserId""},
		],
	},
	{
		""Name"":""UserExtRemarks"",
		""Comment"":""用户扩展信息表-子表"",
		""Columns"":[
			{""Name"":""RemarkId"",""IsPrimary"":true,""MapType"":""System.Guid""},
			{""Name"":""UserId"",""MapType"":""System.Int32""},
			{""Name"":""Remark"",""MapType"":""System.String""},
		],
	},
	{
		""Name"":""UserClaim"",
		""Comment"":""一对多测试表"",
		""Columns"":[
			{""Name"":""Id"",""IsPrimary"":true,""IsIdentity"":true,""MapType"":""System.Int32""},
			{""Name"":""UserId"",""MapType"":""System.Int32""},
			{""Name"":""ClaimName"",""MapType"":""System.String""},
		],
	},
	{
		""Name"":""Role"",
		""Comment"":""权限表"",
		""Columns"":[
			{""Name"":""Id"",""IsPrimary"":true,""IsIdentity"":true,""MapType"":""System.Int32""},
			{""Name"":""Name"",""MapType"":""System.String""}
		],
		""Navigates"":[
			{""Name"":""Users"",""Type"":""ManyToMany"",""RelTable"":""User"",""ManyToMany"":""UserRole""}
		],
		""Indexes"":[]
	},
	{
		""Name"":""UserRole"",
		""Comment"":""多对多中间表"",
		""Columns"":[
			{""Name"":""UserId"",""IsPrimary"":true,""MapType"":""System.Int32""},
			{""Name"":""RoleId"",""IsPrimary"":true,""MapType"":""System.Int32""}
		],
		""Navigates"":[
			{""Name"":""User"",""Type"":""ManyToOne"",""RelTable"":""User"",""Bind"":""UserId""},
			{""Name"":""Role"",""Type"":""ManyToOne"",""RelTable"":""Role"",""Bind"":""RoleId""}
		]
	}
]
"));

	var dyrt3 = dyctx.SelectNoTracking("User")
	.Include("Ext.Remarks", then => then.Where("remark", "like", "error"))
	.Include("Roles", then => then.Include("Users", 
		then => then.Include("Ext.Remarks")))
	.ToList();

	var dyrt2 = dyctx.SelectNoTracking("User")
		.LeftJoin("UserExt", "UserId", "User.Id")
		.LeftJoin("UserExt", "UserId", "User.Id")
		//.IncludeAll()
		.WhereExists(q => q.From("UserClaim")
			.WhereColumns("userid", "=", "user.id")
			.WhereExists(q2 => q2.From("User")
				.WhereColumns("id", "=", "UserClaim.userid")))
		.ToList();

	var dyrt1 = dyctx.Select.ToList();
	dyctx.Delete(dyrt1);

	var itemJson = JsonConvert.SerializeObject(new Dictionary<string, object>
	{
		["Name"] = "user1",
		["Ext"] = new Dictionary<string, object>
		{
		},
		["Claims"] = new List<Dictionary<string, object>>
			{
				new Dictionary<string, object>
				{
					["ClaimName"] = "claim1"
				},
				new Dictionary<string, object>
				{
					["ClaimName"] = "claim2"
				},
				new Dictionary<string, object>
				{
					["ClaimName"] = "claim3"
				},
			},

		["Roles"] = new List<Dictionary<string, object>>
			{
				new Dictionary<string, object>
				{
					["Name"] = "role1"
				},
				new Dictionary<string, object>
				{
					["Name"] = "role2"
				},
			},
	});

	var item = JsonConvert.DeserializeObject<Dictionary<string, object>>(@"
{
""Name"":""user1"",
""Ext"":{
	""Remarks"":[{""Remark"":""remark1""},{""Remark"":""remark2""}]
},
""Claims"":[{""ClaimName"":""claim1""},{""ClaimName"":""claim2""},{""ClaimName"":""claim3""}],
""Roles"":[{""Name"":""role1""},{""Name"":""role2""}]
}");

	var item2 = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(@"
{
""Name"":""user1"",
""Ext"":{
	""Remarks"":[{""Remark"":""remark1""},{""Remark"":""remark2""}]
},
""Claims"":[{""ClaimName"":""claim1""},{""ClaimName"":""claim2""},{""ClaimName"":""claim3""}],
""Roles"":[{""Name"":""role1""},{""Name"":""role2""}]
}");
	dyctx.Insert(item2);

	var item3 = JsonConvert.DeserializeObject<Dictionary<string, object>>(@"
{
""Id"":1,
""Name"":""user1111"",
""Ext"":{},
""Claims"":[{""Id"":1,""ClaimName"":""claim1111""},{""Id"":""3"",""ClaimName"":""claim3222222""},{""ClaimName"":""claim0000""}],
""Roles"":[{""Name"":""role111100001""},{""Id"":2,""Name"":""role2""}]
}");
	item3["Id"] = item2["Id"];
	dyctx.Update(item3);
	dyctx.Delete(item3);





	Task.Run(async () =>
	{
		var users = await fsql.Select<User>().IncludeMany(a => a.Roles).ToListAsync();
	}).Wait();
}

[Table(DisableSyncStructure = true)]
class userdto
{
	public int Id { get; set; }
	public bool? isdeleted { get; set; }
}

class User
{
	[Column(IsIdentity = true)]
	public int Id { get; set; } //Id、UserId、User_id
	public string Name { get; set; }
	public UserExt Ext { get; set; }

	[Navigate(nameof(UserClaim.UserId))]
	public List<UserClaim> Claims { get; set; }
	public List<Role> Roles { get; set; }
}
class UserExt
{
	public int UserId { get; set; }
	public User User { get; set; }
}
class UserClaim
{
	[Column(IsIdentity = true)]
	public int Id { get; set; }
	public int UserId { get; set; }
	public string ClaimName { get; set; }
}
class Role
{
	[Column(IsIdentity = true)]
	public int Id { get; set; }
	public string Name { get; set; }

	public List<User> Users { get; set; }
}
class UserRole
{
	public int UserId { get; set; }
	public User User { get; set; }

	public int RoleId { get; set; }
	public Role Role { get; set; }
}

static class Helper
{
	public static TableDescriptor[] GetTestDesc()
	{
		return new[]
		{
			new TableDescriptor
			{
				 Name = "User",
				 Columns =
				 {
					new TableDescriptor.ColumnDescriptor{ Name = "Id", MapType = typeof(int), IsPrimary = true, IsIdentity = true },
					new TableDescriptor.ColumnDescriptor{ Name = "Name", MapType = typeof(string), StringLength = 100 },
				 },
				 Navigates =
				 {
					new TableDescriptor.NavigateDescriptor { Name = "Ext", RelTable = "UserExt", Type = TableDescriptor.NavigateType.OneToOne },
					new TableDescriptor.NavigateDescriptor { Name = "Roles", RelTable = "Role", Type = TableDescriptor.NavigateType.ManyToMany, ManyToMany = "UserRole" },
				 }
			},
			new TableDescriptor
			{
				Name = "UserExt",
				 Columns =
				 {
					new TableDescriptor.ColumnDescriptor{ Name = "UserId", MapType = typeof(int), IsPrimary = true },
					new TableDescriptor.ColumnDescriptor{ Name = "Name", MapType = typeof(string), StringLength = 100 },
				 },
				 Navigates =
				 {
					new TableDescriptor.NavigateDescriptor { Name = "User", RelTable = "User", Type = TableDescriptor.NavigateType.OneToOne },
				 }
			},
			new TableDescriptor
			{
				 Name = "Role",
				 Columns =
				 {
					new TableDescriptor.ColumnDescriptor{ Name = "Id", MapType = typeof(int), IsPrimary = true, IsIdentity = true },
					new TableDescriptor.ColumnDescriptor{ Name = "Name", MapType = typeof(string), StringLength = 50 },
				 },
				 Navigates =
				 {
					new TableDescriptor.NavigateDescriptor { Name = "Users", RelTable = "User", Type = TableDescriptor.NavigateType.ManyToMany, ManyToMany = "UserRole" },
				 }
			},
			new TableDescriptor
			{
				Name = "UserRole",
				 Columns =
				 {
					new TableDescriptor.ColumnDescriptor{ Name = "UserId", MapType = typeof(int), IsPrimary = true },
					new TableDescriptor.ColumnDescriptor{ Name = "RoleId", MapType = typeof(int), IsPrimary = true },
				 },
				 Navigates =
				 {
					new TableDescriptor.NavigateDescriptor { Name = "User", RelTable = "User", Type = TableDescriptor.NavigateType.ManyToOne, Bind = "UserId" },
					new TableDescriptor.NavigateDescriptor { Name = "Role", RelTable = "Role", Type = TableDescriptor.NavigateType.ManyToOne, Bind = "RoleId" },
				 }
			},
		};
	}
}