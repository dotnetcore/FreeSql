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

		public UnitOfWork(IFreeSql fsql) {
			_fsql = fsql;
		}

		void ReturnObject() {
			_fsql.Ado.MasterPool.Return(_conn);
			_tran = null;
			_conn = null;
		}
		internal DbTransaction GetOrBeginTransaction(bool isCreate = true) {

			if (_tran != null) return _tran;
			if (isCreate == false) return null;
			if (_conn != null) _fsql.Ado.MasterPool.Return(_conn);

			_conn = _fsql.Ado.MasterPool.Get();
			try {
				_tran = _conn.Value.BeginTransaction();
			} catch {
				ReturnObject();
				throw;
			}
			return _tran;
		}

		public void Commit() {
			if (_tran != null) {
				try {
					_tran.Commit();
				} finally {
					ReturnObject();
				}
			}
		}
		public void Rollback() {
			if (_tran != null) {
				try {
					_tran.Rollback();
				} finally {
					ReturnObject();
				}
			}
		}
		public void Dispose() {
			this.Rollback();
		}

		public DefaultRepository<TEntity, TKey> GetRepository<TEntity, TKey>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class {
			var repos = new DefaultRepository<TEntity, TKey>(_fsql, filter);
			repos._unitOfWork = this;
			return repos;
		}
		public GuidRepository<TEntity> GetGuidRepository<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<string, string> asTable = null) where TEntity : class {
			var repos = new GuidRepository<TEntity>(_fsql, filter, asTable);
			repos._unitOfWork = this;
			return repos;
		}
	}
}
