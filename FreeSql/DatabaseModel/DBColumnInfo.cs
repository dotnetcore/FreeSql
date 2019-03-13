using System;

namespace FreeSql.DatabaseModel {
	public class DbColumnInfo {
		/// <summary>
		/// 所属表
		/// </summary>
		public DbTableInfo Table { get; internal set; }
		/// <summary>
		/// 列名
		/// </summary>
		public string Name { get; internal set; }
		/// <summary>
		/// 映射到 C# 类型
		/// </summary>
		public Type CsType { get; internal set; }
		/// <summary>
		/// 数据库枚举类型int值
		/// </summary>
		public int DbType { get; internal set; }
		/// <summary>
		/// 数据库类型，字符串，varchar
		/// </summary>
		public string DbTypeText { get; internal set; }
		/// <summary>
		/// 数据库类型，字符串，varchar(255)
		/// </summary>
		public string DbTypeTextFull { get; internal set; }
		/// <summary>
		/// 最大长度
		/// </summary>
		public int MaxLength { get; internal set; }
		/// <summary>
		/// 主键
		/// </summary>
		public bool IsPrimary { get; internal set; }
		/// <summary>
		/// 自增标识
		/// </summary>
		public bool IsIdentity { get; internal set; }
		/// <summary>
		/// 是否可DBNull
		/// </summary>
		public bool IsNullable { get; internal set; }
		/// <summary>
		/// 备注
		/// </summary>
		public string Coment { get; internal set; }
	}
}
