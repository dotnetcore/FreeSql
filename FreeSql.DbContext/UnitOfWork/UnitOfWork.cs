using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql {
	class UnitOfWork : IUnitOfWork {

		protected IFreeSql _fsql;
		protected Object<DbConnection> _conn;
		protected DbTransaction _tran;

		public UnitOfWork(IFreeSql fsql) {
			_fsql = fsql;
		}

		void ReturnObject() {
			_fsql.Ado.MasterPool.Return(_conn);
			_tran = null;
			_conn = null;
		}
		public DbTransaction GetOrBeginTransaction(bool isCreate = true) {

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
		~UnitOfWork() {
			this.Dispose();
		}
		bool _isdisposed = false;
		public void Dispose() {
			if (_isdisposed) return;
			try {
				this.Rollback();
			} finally {
				_isdisposed = true;
				GC.SuppressFinalize(this);
			}
		}

	}
}
