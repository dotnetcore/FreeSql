using System.Linq.Expressions;

namespace FreeSql.Internal.Model {
	class SelectTableInfo {
		public TableInfo Table { get; set; }
		public string Alias { get; set; }
		public string On { get; set; }
		public string NavigateCondition { get; set; }
		public ParameterExpression Parameter { get; set; }
		public SelectTableInfoType Type { get; set; }
	}
	enum SelectTableInfoType { From, LeftJoin, InnerJoin, RightJoin, Parent }
}
