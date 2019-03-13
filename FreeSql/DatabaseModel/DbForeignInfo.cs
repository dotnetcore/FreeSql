using System.Collections.Generic;

namespace FreeSql.DatabaseModel {
	public class DbForeignInfo {
		public DbTableInfo Table { get; internal set; }
		public List<DbColumnInfo> Columns { get; internal set; } = new List<DbColumnInfo>();
		public DbTableInfo ReferencedTable { get; internal set; }
		public List<DbColumnInfo> ReferencedColumns { get; internal set; } = new List<DbColumnInfo>();

	}
}
