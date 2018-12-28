using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider {
	class SelectGroupingProvider<T1> : ISelectGrouping<T1> {

		internal object _select;
		internal ReadAnonymousTypeInfo _map;
		internal CommonExpression _comonExp;
		public SelectGroupingProvider(object select, ReadAnonymousTypeInfo map, CommonExpression comonExp) {
			_select = select;
			_map = map;
			_comonExp = comonExp;
		}

		string getSelectGroupingMapString(Expression[] members) {
			var read = _map;
			for (var a = 0; a < members.Length; a++) {
				read = read.Childs.Where(z => z.CsName == (members[a] as MemberExpression)?.Member.Name).FirstOrDefault();
				if (read == null) return null;
			}
			return read.DbField;
		}

		public ISelectGrouping<T1> Having(Expression<Func<ISelectGroupingAggregate<T1>, bool>> exp) {
			var sql = _comonExp.ExpressionWhereLambda(null, exp, getSelectGroupingMapString);
			var method = _select.GetType().GetMethod("Having", new[] { typeof(string), typeof(object) });
			method.Invoke(_select, new object[] { sql, null });
			return this;
		}

		public ISelectGrouping<T1> OrderBy<TMember>(Expression<Func<ISelectGroupingAggregate<T1>, TMember>> column) {
			var sql = _comonExp.ExpressionWhereLambda(null, column, getSelectGroupingMapString);
			var method = _select.GetType().GetMethod("OrderBy", new[] { typeof(string), typeof(object) });
			method.Invoke(_select, new object[] { sql, null });
			return this;
		}

		public ISelectGrouping<T1> OrderByDescending<TMember>(Expression<Func<ISelectGroupingAggregate<T1>, TMember>> column) {
			var sql = _comonExp.ExpressionWhereLambda(null, column, getSelectGroupingMapString);
			var method = _select.GetType().GetMethod("OrderBy", new[] { typeof(string), typeof(object) });
			method.Invoke(_select, new object[] { $"{sql} DESC", null });
			return this;
		}

		public List<TReturn> ToList<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, TReturn>> select) {
			var map = new ReadAnonymousTypeInfo();
			var field = new StringBuilder();
			var index = 0;

			_comonExp.ReadAnonymousField(null, field, map, ref index, select, getSelectGroupingMapString);
			var method = _select.GetType().GetMethod("ToListMapReader", BindingFlags.Instance | BindingFlags.NonPublic);
			method = method.MakeGenericMethod(typeof(TReturn));
			return method.Invoke(_select, new object[] { (map, field.Length > 0 ? field.Remove(0, 2).ToString() : null) }) as List<TReturn>;
		}
		public Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, TReturn>> select) {
			var map = new ReadAnonymousTypeInfo();
			var field = new StringBuilder();
			var index = 0;

			_comonExp.ReadAnonymousField(null, field, map, ref index, select, getSelectGroupingMapString);
			var method = _select.GetType().GetMethod("ToListMapReaderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
			method = method.MakeGenericMethod(typeof(TReturn));
			return method.Invoke(_select, new object[] { (map, field.Length > 0 ? field.Remove(0, 2).ToString() : null) }) as Task<List<TReturn>>;
		}

		public string ToSql<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, TReturn>> select) {
			var map = new ReadAnonymousTypeInfo();
			var field = new StringBuilder();
			var index = 0;

			_comonExp.ReadAnonymousField(null, field, map, ref index, select, getSelectGroupingMapString);
			var method = _select.GetType().GetMethod("ToSql", new[] { typeof(string) });
			return method.Invoke(_select, new object[] { field.Length > 0 ? field.Remove(0, 2).ToString() : null }) as string;
		}

		
	}
}
