using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace FreeSql {
	public class DbContextOptionsBuilder {

		internal IFreeSql _fsql;

		public DbContextOptionsBuilder UseFreeSql(IFreeSql orm) {
			_fsql = orm;
			return this;
		}
	}
}
