using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace FreeSql.Internal.CommonProvider
{
    partial class AdoProvider
    {

        class Transaction2
        {
            internal Object<DbConnection> Conn;
            internal DbTransaction Transaction;
            internal DateTime RunTime;
            internal TimeSpan Timeout;

            public Transaction2(Object<DbConnection> conn, DbTransaction tran, TimeSpan timeout)
            {
                Conn = conn;
                Transaction = tran;
                RunTime = DateTime.Now;
                Timeout = timeout;
            }
        }

        private Dictionary<int, Transaction2> _trans = new Dictionary<int, Transaction2>();
        private object _trans_lock = new object();

        public DbTransaction TransactionCurrentThread => _trans.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var conn) && conn.Transaction?.Connection != null ? conn.Transaction : null;

        public void BeginTransaction(TimeSpan timeout)
        {
            if (TransactionCurrentThread != null) return;

            int tid = Thread.CurrentThread.ManagedThreadId;
            Transaction2 tran = null;
            Object<DbConnection> conn = null;

            try
            {
                conn = MasterPool.Get();
                tran = new Transaction2(conn, conn.Value.BeginTransaction(), timeout);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"数据库出错（开启事务）{ex.Message} \r\n{ex.StackTrace}");
                MasterPool.Return(conn);
                throw ex;
            }
            if (_trans.ContainsKey(tid)) CommitTransaction();

            lock (_trans_lock)
                _trans.Add(tid, tran);
        }

        private void AutoCommitTransaction()
        {
            if (_trans.Count > 0)
            {
                Transaction2[] trans = null;
                lock (_trans_lock)
                    trans = _trans.Values.Where(st2 => DateTime.Now.Subtract(st2.RunTime) > st2.Timeout).ToArray();
                foreach (Transaction2 tran in trans) CommitTransaction(true, tran);
            }
        }
        private void CommitTransaction(bool isCommit, Transaction2 tran)
        {
            if (tran == null || tran.Transaction == null || tran.Transaction.Connection == null) return;

            if (_trans.ContainsKey(tran.Conn.LastGetThreadId))
                lock (_trans_lock)
                    if (_trans.ContainsKey(tran.Conn.LastGetThreadId))
                        _trans.Remove(tran.Conn.LastGetThreadId);

            Exception ex = null;
            var f001 = isCommit ? "提交" : "回滚";
            try
            {
                Trace.WriteLine($"线程{tran.Conn.LastGetThreadId}事务{f001}");
                if (isCommit) tran.Transaction.Commit();
                else tran.Transaction.Rollback();
            }
            catch (Exception ex2)
            {
                ex = ex2;
                Trace.WriteLine($"数据库出错（{f001}事务）：{ex.Message} {ex.StackTrace}");
            }
            finally
            {
                ReturnConnection(MasterPool, tran.Conn, ex); //MasterPool.Return(tran.Conn, ex);
            }
        }
        private void CommitTransaction(bool isCommit)
        {
            if (_trans.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var tran)) CommitTransaction(isCommit, tran);
        }
        public void CommitTransaction() => CommitTransaction(true);
        public void RollbackTransaction() => CommitTransaction(false);

        public void Transaction(Action handler)
        {
            Transaction(handler, TimeSpan.FromSeconds(60));
        }
        public void Transaction(Action handler, TimeSpan timeout)
        {
            try
            {
                BeginTransaction(timeout);
                handler();
                CommitTransaction();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                throw ex;
            }
        }

        ~AdoProvider()
        {
            this.Dispose();
        }
        bool _isdisposed = false;
        public void Dispose()
        {
            if (_isdisposed) return;
            try
            {
                Transaction2[] trans = null;
                lock (_trans_lock)
                    trans = _trans.Values.ToArray();
                foreach (Transaction2 tran in trans) CommitTransaction(false, tran);
            }
            catch { }

            ObjectPool<DbConnection>[] pools = null;
            for (var a = 0; a < 10; a++)
            {
                try
                {
                    pools = SlavePools.ToArray();
                    SlavePools.Clear();
                    break;
                }
                catch
                {
                }
            }
            if (pools != null)
            {
                foreach (var pool in pools)
                {
                    try { pool.Dispose(); } catch { }
                }
            }
            try { MasterPool.Dispose(); } catch { }
        }
    }
}