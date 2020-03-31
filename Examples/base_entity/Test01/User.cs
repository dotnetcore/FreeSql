using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using FreeSql.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using FreeSql;

namespace EMSServerModel.Model
{
	/// <summary>
	/// 用户表bb123123
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class User : BaseEntity<User> {

		//[JsonProperty, Column(IsIdentity = true)]
		//public long Id { get; set; }

		/// <summary>
		/// 编号
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)", IsPrimary = true)]
		public string UserId { get; set; } = string.Empty;

		/// <summary>
		/// 头像
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string Avatar { get; set; } = string.Empty;

		/// <summary>
		/// 姓名
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string UserName { get; set; } = string.Empty;

		/// <summary>
		/// 艺名
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string NickName { get; set; } = string.Empty;

		/// <summary>
		/// 电话
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string Tel { get; set; } = string.Empty;

		/// <summary>
		/// 性别
		/// </summary>
		[JsonProperty]
		public Sex Sex { get; set; } = Sex.男;

		/// <summary>
		/// 证件号
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string UID { get; set; } = string.Empty;

		/// <summary>
		/// 生日
		/// </summary>
		[JsonProperty, Column(DbType = "date")]
		public DateTime? DateOfBirth { get; set; }


		/// <summary>
		/// 出生地
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string PlaceOfBirth { get; set; } = string.Empty;

		/// <summary>
		/// 居住地
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string Addr { get; set; } = string.Empty;


		/// <summary>
		/// 密码
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string Pwd { get; set; } = string.Empty;

		
		/// <summary>
		/// 部门编号
		/// </summary>
		[JsonProperty]
		public long? DeptId { get; set; }

		/// <summary>
		/// 职务编号
		/// </summary>
		[JsonProperty]
		public long? TitleId { get; set; }

		
		///// <summary>
		///// 创建时间
		///// </summary>
		//[JsonProperty, Column(DbType = "date")]
		//public DateTime CreateTime { get; set; } = DateTime.Now;

		/// <summary>
		/// 国籍
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string Nationality { get; set; } = string.Empty;

		/// <summary>
		/// 经手人
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string Handler { get; set; } = string.Empty;

		/// <summary>
		/// 启用
		/// </summary>
		[JsonProperty]
		public bool IsEnable { get; set; } = true;


		/// <summary>
		/// 备注
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(100)")]
		public string Memos { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Navigate(ManyToMany = typeof(UserRole))]
		public virtual ICollection<Role> Roles { get; protected set; }

	}
	/// <summary>
	/// 性别枚举
	/// </summary>
	public enum Sex
	{
		/// <summary>
		/// 女=0
		/// </summary>
		女=0,
		/// <summary>
		/// 男=1
		/// </summary>
		男=1
	}

}
