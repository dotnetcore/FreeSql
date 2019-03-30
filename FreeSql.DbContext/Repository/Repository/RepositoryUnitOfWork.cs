using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql {
	class RepositoryUnitOfWork : UnitOfWork, IRepositoryUnitOfWork {

		public RepositoryUnitOfWork(IFreeSql fsql) : base(fsql) {
		}

		public GuidRepository<TEntity> GetGuidRepository<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<string, string> asTable = null) where TEntity : class {
			var repos = new GuidRepository<TEntity>(_fsql, filter, asTable);
			repos._uow = this;
			return repos;
		}

		public DefaultRepository<TEntity, TKey> GetRepository<TEntity, TKey>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class {
			var repos = new DefaultRepository<TEntity, TKey>(_fsql, filter);
			repos._uow = this;
			return repos;
		}
	}
}
