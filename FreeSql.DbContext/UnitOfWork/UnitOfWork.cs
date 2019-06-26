using SafeObjectPool;
using System;
using System.Data;
using System.Data.Common;

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


        /// <summary>
        /// 是否启用工作单元
        /// </summary>
        public bool Enable { get; private set; } = true;

        /// <summary>
        /// 禁用工作单元
        /// <exception cref="Exception"></exception>
        /// <para></para>
        /// 若已开启事务（已有Insert/Update/Delete操作），调用此方法将发生异常，建议在执行逻辑前调用
        /// </summary>
        public void Close()
        {
            if (_tran != null)
            {
                throw new Exception("已开启事务，不能禁用工作单元");
            }

            Enable = false;
        }

        public void Open()
        {
            Enable = true;
        }

        public IsolationLevel? IsolationLevel { get; set; }

		public DbTransaction GetOrBeginTransaction(bool isCreate = true) {

			if (_tran != null) return _tran;
			if (isCreate == false) return null;
            if (!Enable) return null;
			if (_conn != null) _fsql.Ado.MasterPool.Return(_conn);

			_conn = _fsql.Ado.MasterPool.Get();
			try {
				_tran = IsolationLevel == null ? 
					_conn.Value.BeginTransaction() : 
					_conn.Value.BeginTransaction(IsolationLevel.Value);
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
