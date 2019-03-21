
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Internal.CommonProvider {
	class AopProvider : IAop {
		public EventHandler<AopToListEventArgs> ToList { get; set; }
		public EventHandler<AopWhereEventArgs> Where { get; set; }
		public EventHandler<AopParseExpressionEventArgs> ParseExpression { get; set; }
	}
}
