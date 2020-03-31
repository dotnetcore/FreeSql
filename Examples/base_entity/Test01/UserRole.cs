using Newtonsoft.Json;
using FreeSql.DataAnnotations;
using FreeSql;

namespace EMSServerModel.Model
{
	/// <summary>
	/// 用户角色关系表aa111
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class UserRole : BaseEntity<UserRole>{
		/// <summary>
		/// 角色编号1
		/// </summary>
		[JsonProperty]
		public long RoleId { get; set; }
		/// <summary>
		/// 角色导航
		/// </summary>
		[Navigate("RoleId")]
		public Role Roles { get; set; }

		/// <summary>
		/// 用户编号
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string UserId { get; set; }
		/// <summary>
		/// 用户导航
		/// </summary>
		[Navigate("UserId")]
		public User Users { get; set; }

	}

}
