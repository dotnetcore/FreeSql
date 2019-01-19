using System;
using System.Collections.Generic;
using System.Reflection;

namespace FreeSql.Internal.Model {
	class TableInfo {
		public Type Type { get; set; }
		public Type TypeLazy { get; set; }
		public MethodInfo TypeLazySetOrm { get; set; }
		public Dictionary<string, PropertyInfo> Properties { get; set; } = new Dictionary<string, PropertyInfo>(StringComparer.CurrentCultureIgnoreCase);
		public Dictionary<string, ColumnInfo> Columns { get; set; } = new Dictionary<string, ColumnInfo>(StringComparer.CurrentCultureIgnoreCase);
		public Dictionary<string, ColumnInfo> ColumnsByCs { get; set; } = new Dictionary<string, ColumnInfo>(StringComparer.CurrentCultureIgnoreCase);
		public ColumnInfo[] Primarys { get; set; }
		public string CsName { get; set; }
		public string DbName { get; set; }
		public string DbOldName { get; set; }
		public string SelectFilter { get; set; }
	}
}