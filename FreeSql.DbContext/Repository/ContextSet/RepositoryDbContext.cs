using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql {
	internal class RepositoryDbContext<TEntity> : DbContext where TEntity : class {

		protected BaseRepository<TEntity> _repos;
		public RepositoryDbContext(IFreeSql orm, BaseRepository<TEntity> repos) : base() {
			_orm = orm;
			_repos = repos;
			_isUseUnitOfWork = false;
		}

		public override object Set(Type entityType) {
			if (_dicSet.ContainsKey(entityType)) return _dicSet[entityType];
			var sd = Activator.CreateInstance(typeof(RepositoryDbSet<>).MakeGenericType(entityType), _repos);
			_dicSet.Add(entityType, sd);
			return sd;
		}

		RepositoryDbSet<TEntity> _dbSet;
		public RepositoryDbSet<TEntity> DbSet => _dbSet ?? (_dbSet = Set<TEntity>() as RepositoryDbSet<TEntity>);
	}
}
