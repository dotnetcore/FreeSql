using FreeSql.Internal.ObjectPool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
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
            internal Aop.TraceBeforeEventArgs AopBefore;
            internal Object<DbConnection> Connection;
            internal DbTransaction Transaction;
            internal DateTime RunTime;
            internal TimeSpan Timeout;

            public Transaction2(Object<DbConnection> conn, DbTransaction tran, TimeSpan timeout)
            {
                Connection = conn;
                Transaction = tran;
                RunTime = DateTime.Now;
                Timeout = timeout;
            }
        }

        private ConcurrentDictionary<int, Transaction2> _trans = new ConcurrentDictionary<int, Transaction2>();

        public DbTransaction TransactionCurrentThread => _trans.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var conn) && conn.Transaction?.Connection != null ? conn.Transaction : null;
        public Aop.TraceBeforeEventArgs TransactionCurrentThreadAopBefore => _trans.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var conn) && conn.Transaction?.Connection != null ? conn.AopBefore : null;

        public void BeginTransaction(IsolationLevel? isolationLevel)
        {
            if (TransactionCurrentThread != null) return;

            int tid = Thread.CurrentThread.ManagedThreadId;
            Transaction2 tran = null;
            Object<DbConnection> conn = null;
            var before = new Aop.TraceBeforeEventArgs("ThreadTransaction", isolationLevel);
            _util?._orm?.Aop.TraceBeforeHandler?.Invoke(this, before);

            try
            {
                conn = MasterPool.Get();
                tran = new Transaction2(conn, isolationLevel == null ? conn.Value.BeginTransaction() : conn.Value.BeginTransaction(isolationLevel.Value), TimeSpan.FromSeconds(60));
                tran.AopBefore = before;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"数据库出错（开启事务）{ex.Message} \r\n{ex.StackTrace}");
                MasterPool.Return(conn);
                var after = new Aop.TraceAfterEventArgs(before, "", ex);
                _util?._orm?.Aop.TraceAfterHandler?.Invoke(this, after);
                throw;
            }
            if (_trans.ContainsKey(tid)) CommitTransaction();
            _trans.TryAdd(tid, tran);
        }

        private void CommitTimeoutTransaction()
        {
            //关闭 fsql.Transaction 线程事务自动提交机制 https://github.com/dotnetcore/FreeSql/issues/323
            //if (_trans.Count > 0)
            //{
            //    var trans = _trans.Values.Where(st2 => DateTime.Now.Subtract(st2.RunTime) > st2.Timeout).ToArray();
            //    foreach (var tran in trans) CommitTransaction(true, tran, null, "Timeout自动提交");
            //}
        }
        private void CommitTransaction(bool isCommit, Transaction2 tran, Exception rollbackException, string remark = null)
        {
            if (tran == null || tran.Transaction == null || tran.Connection == null) return;
            _trans.TryRemove(tran.Connection.LastGetThreadId, out var oldtran);

            Exception ex = null;
            if (string.IsNullOrEmpty(remark)) remark = isCommit ? "提交" : "回滚";
            try
            {
                if (tran.Transaction.Connection != null) //用户自行 Commit、Rollback
                {
                    Trace.WriteLine($"线程{tran.Connection.LastGetThreadId}事务{remark}");
                    if (isCommit) tran.Transaction.Commit();
                    else tran.Transaction.Rollback();
                }
            }
            catch (Exception ex2)
            {
                ex = ex2;
                Trace.WriteLine($"数据库出错（{remark}事务）：{ex.Message} {ex.StackTrace}");
            }
            finally
            {
                ReturnConnection(MasterPool, tran.Connection, ex); //MasterPool.Return(tran.Conn, ex);

                var after = new Aop.TraceAfterEventArgs(tran.AopBefore, remark, ex ?? rollbackException);
                _util?._orm?.Aop.TraceAfterHandler?.Invoke(this, after);
            }
        }
        public void CommitTransaction()
        {
            if (_trans.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var tran)) CommitTransaction(true, tran, null);
        }
        public void RollbackTransaction(Exception ex)
        {
            if (_trans.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var tran)) CommitTransaction(false, tran, ex);
        }

        public void Transaction(Action handler) => TransactionInternal(null, handler);
        public void Transaction(IsolationLevel isolationLevel, Action handler) => TransactionInternal(isolationLevel, handler);

        void TransactionInternal(IsolationLevel? isolationLevel, Action handler)
        {
            var requireTran = TransactionCurrentThread == null;
            try
            {
                if (requireTran) BeginTransaction(isolationLevel);
                handler();
                if (requireTran) CommitTransaction();
            }
            catch (Exception ex)
            {
                if (requireTran) RollbackTransaction(ex);
                throw;
            }
        }

        ~AdoProvider() => this.Dispose();
        int _disposeCounter;
        public void Dispose()
        {
            if (ResolveTransaction != null) return;
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            try
            {
                var trans = _trans?.Values.ToArray();
                if (trans != null) foreach (var tran in trans) CommitTransaction(false, tran, null, "Dispose自动提交");
            }
            catch { }

            IObjectPool<DbConnection>[] pools = null;
            for (var a = 0; a < 10; a++)
            {
                try
                {
                    pools = SlavePools?.ToArray();
                    SlavePools?.Clear();
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
                    try { pool?.Dispose(); } catch { }
                }
            }
            try { MasterPool?.Dispose(); } catch { }
        }
    }
}