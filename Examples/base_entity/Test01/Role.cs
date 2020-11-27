using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using FreeSql.DataAnnotations;
using FreeSql;

namespace EMSServerModel.Model
{
	/// <summary>
	/// 角色表
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Role : BaseEntity<Role>{
		/// <summary>
		/// 角色编号
		/// </summary>
		[JsonProperty, Column(IsPrimary = true, IsIdentity = true)]
		public long RoleId { get; set; }

		/// <summary>
		/// 角色名称
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string RoleName { get; set; } = string.Empty;

		/// <summary>
		/// 角色描述
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string RoleDesc { get; set; } = string.Empty;

		///// <summary>
		///// 创建时间
		///// </summary>
		//[JsonProperty, Column(DbType = "date")]
		//public DateTime CreateTime { get; set; } = DateTime.Now;

		/// <summary>
		/// 启用
		/// </summary>
		[JsonProperty]
		public bool IsEnable { get; set; } = true;

		/// <summary>
		/// 角色用户多对多导航
		/// </summary>
		[Navigate(ManyToMany = typeof(UserRole))]
		public virtual ICollection<User> Users { get; protected set; }

	}

}
