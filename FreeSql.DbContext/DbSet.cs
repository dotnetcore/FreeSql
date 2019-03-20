using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql {
	public abstract class DbSet<TEntity> where TEntity : class {

		protected DbContext _ctx;

		public ISelect<TEntity> Select => _ctx._fsql.Select<TEntity>().WithTransaction(_ctx.GetOrBeginTransaction(false));

		public IInsert<TEntity> Insert(TEntity source) => _ctx._fsql.Insert<TEntity>(source).WithTransaction(_ctx.GetOrBeginTransaction());
		public IInsert<TEntity> Insert(TEntity[] source) => _ctx._fsql.Insert<TEntity>(source).WithTransaction(_ctx.GetOrBeginTransaction());
		public IInsert<TEntity> Insert(IEnumerable<TEntity> source) => _ctx._fsql.Insert<TEntity>(source).WithTransaction(_ctx.GetOrBeginTransaction());

		public IUpdate<TEntity> Update => _ctx._fsql.Update<TEntity>().WithTransaction(_ctx.GetOrBeginTransaction());

		public IDelete<TEntity> Delete => _ctx._fsql.Delete<TEntity>().WithTransaction(_ctx.GetOrBeginTransaction());
	}

	internal class BaseDbSet<TEntity> : DbSet<TEntity> where TEntity : class {
		
		public BaseDbSet(DbContext ctx) {
			_ctx = ctx;
		}
	}
}
