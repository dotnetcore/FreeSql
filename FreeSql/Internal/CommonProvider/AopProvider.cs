
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Internal.CommonProvider {
	class AopProvider : IAop {
		public EventHandler<AopToListEventArgs> ToList { get; set; }
		public EventHandler<AopWhereEventArgs> Where { get; set; }
		public EventHandler<AopParseExpressionEventArgs> ParseExpression { get; set; }
		public EventHandler<AopConfigEntityEventArgs> ConfigEntity { get; set; }
		public EventHandler<AopConfigEntityPropertyEventArgs> ConfigEntityProperty { get; set; }
		public EventHandler<AopOnUpdatedEventArgs> OnUpdated { get; set; }
		public EventHandler<AopOnInsertedEventArgs> OnInserted { get; set; }
		public EventHandler<AopOnDeletedEventArgs> OnDeleted { get; set; }
		public EventHandler<AopOnSelectedEventArgs> OnSelected { get; set; }
	}
}
