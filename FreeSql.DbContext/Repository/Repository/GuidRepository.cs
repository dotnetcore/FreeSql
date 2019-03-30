using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql {
	public class GuidRepository<TEntity> :
		BaseRepository<TEntity, Guid>
		where TEntity : class {

		public GuidRepository(IFreeSql fsql) : this(fsql, null, null) {

		}
		public GuidRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter, Func<string, string> asTable) : base(fsql, filter, asTable) {
		}
	}
}
