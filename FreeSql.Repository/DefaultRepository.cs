using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql {
	public class DefaultRepository<TEntity, TKey> :
		BaseRepository<TEntity, TKey>
		where TEntity : class {

		public DefaultRepository(IFreeSql fsql) : base(fsql) {
		}
	}
}
