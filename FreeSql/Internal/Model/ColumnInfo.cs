using FreeSql.DataAnnotations;
using System;

namespace FreeSql.Internal.Model {
	public class ColumnInfo {
		public TableInfo Table { get; set; }
		public string CsName { get; set; }
		public Type CsType { get; set; }
		public ColumnAttribute Attribute { get; set; }
	}
}