using System.Collections.Generic;
using System.Linq;

namespace FreeSql.DatabaseModel {
	public class DbTableInfo {
		/// <summary>
		/// 唯一标识
		/// </summary>
		public string Id { get; internal set; }
		/// <summary>
		/// SqlServer下是Owner、PostgreSQL下是Schema、MySql下是数据库名
		/// </summary>
		public string Schema { get; internal set; }
		/// <summary>
		/// 表名
		/// </summary>
		public string Name { get; internal set; }
		/// <summary>
		/// 表备注，SqlServer下是扩展属性 MS_Description
		/// </summary>
		public string Comment { get; internal set; }
		/// <summary>
		/// 表/视图
		/// </summary>
		public DbTableType Type { get; set; }
		/// <summary>
		/// 列
		/// </summary>
		public List<DbColumnInfo> Columns { get; internal set; } = new List<DbColumnInfo>();
		/// <summary>
		/// 自增列
		/// </summary>
		public List<DbColumnInfo> Identitys { get; internal set; } = new List<DbColumnInfo>();
		/// <summary>
		/// 主键/组合
		/// </summary>
		public List<DbColumnInfo> Primarys { get; internal set; } = new List<DbColumnInfo>();
		/// <summary>
		/// 唯一键/组合
		/// </summary>
		public Dictionary<string, List<DbColumnInfo>> UniquesDict { get; internal set; } = new Dictionary<string, List<DbColumnInfo>>();
		/// <summary>
		/// 索引/组合
		/// </summary>
		public Dictionary<string, List<DbColumnInfo>> IndexesDict { get; internal set; } = new Dictionary<string, List<DbColumnInfo>>();
		/// <summary>
		/// 外键
		/// </summary>
		public Dictionary<string, DbForeignInfo> ForeignsDict { get; internal set; } = new Dictionary<string, DbForeignInfo>();

		public List<List<DbColumnInfo>> Uniques => UniquesDict.Values.ToList();
		public List<List<DbColumnInfo>> Indexes => IndexesDict.Values.ToList();
		public List<DbForeignInfo> Foreigns => ForeignsDict.Values.ToList();
	}

	public enum DbTableType {
		TABLE, VIEW, StoreProcedure
	}
}
