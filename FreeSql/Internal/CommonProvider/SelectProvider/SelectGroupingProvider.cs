//using FreeSql.Internal.Model;
//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Text;

//namespace FreeSql.Internal.CommonProvider {
//	public class SelectGroupingProvider<T1, T2> : ISelectGrouping<T1> where T2 : class {

//		internal Select1Provider<T2> _select;
//		internal ReadAnonymousTypeInfo _map;
//		internal CommonExpression _comonExp;
//		internal SelectTableInfo _table;
//		SelectGroupingProvider(CommonExpression comonExp, Expression exp) {
//			_comonExp = comonExp;
//			//var columns = _comonExp.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(_select._tables, columns, true);
//			_table = new SelectTableInfo { Alias = "", On = "", Table = _comonExp._common.GetTableByEntity(typeof(T1)), Type = SelectTableInfoType.From };
//		}

//		public ISelectGrouping<T1> Having(Expression<Func<ISelectGroupingAggregate<T1>, bool>> exp) {
//			_select.Having(_comonExp.ExpressionWhereLambda(new List<SelectTableInfo>(new[] { _table }), exp));
//			return this;
//		}

//		public ISelectGrouping<T1> OrderBy<TMember>(Expression<Func<ISelectGroupingAggregate<T1>, TMember>> column) {
//			var columnMap = new List<SelectColumnInfo>();
//			_comonExp.ExpressionSelectColumn_MemberAccess(new List<SelectTableInfo>(new[] { _table }), columnMap, SelectTableInfoType.From, column, true);

//			_select.OrderBy();
//			return this;
//		}

//		public ISelectGrouping<T1> OrderByDescending<TMember>(Expression<Func<ISelectGroupingAggregate<T1>, TMember>> column) {
//			_select.OrderBy(" DESC");
//			return this;
//		}

//		public List<TReturn> ToList<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, TReturn>> select) {
//			throw new NotImplementedException();
//		}
//	}
//}
