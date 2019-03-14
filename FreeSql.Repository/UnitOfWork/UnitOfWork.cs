using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql {
	class UnitOfWork : IUnitOfWork {

		IFreeSql _fsql;
		Object<DbConnection> _conn;
		DbTransaction _tran;

		bool _isCommitOrRoolback = false;

		public UnitOfWork(IFreeSql fsql) {
			_fsql = fsql;
		}

		DbTransaction BeginTransaction() {
			_conn = _fsql.Ado.MasterPool.Get();
			try {
				_tran = _conn.Value.BeginTransaction();
			} catch {
				_fsql.Ado.MasterPool.Return(_conn);
				_conn = null;
				throw;
			}
			return _tran;
		}

		public void Commit() {
			_isCommitOrRoolback = true;
			if (_conn != null) {
				try {
					_tran.Commit();
				} finally {
					_fsql.Ado.MasterPool.Return(_conn);
				}
			}
		}
		public void Rollback() {
			_isCommitOrRoolback = true;
			if (_conn != null) {
				try {
					_tran.Rollback();
				} finally {
					_fsql.Ado.MasterPool.Return(_conn);
				}
			}
		}
		public void Dispose() {
			if (_isCommitOrRoolback == false) {
				this.Commit();
			}
		}

		public DefaultRepository<TEntity, TKey> GetRepository<TEntity, TKey>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class {
			var repos = new DefaultRepository<TEntity, TKey>(_fsql, filter);
			repos._tran = BeginTransaction();
			return repos;
		}
		public GuidRepository<TEntity> GetGuidRepository<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<string, string> asTable = null) where TEntity : class {
			var repos = new GuidRepository<TEntity>(_fsql, filter, asTable);
			repos._tran = BeginTransaction();
			return repos;
		}
	}
}
