using FreeSql;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace FreeSql
{
    class DbContextScopedFreeSql : IFreeSql
    {
        public IFreeSql _originalFsql;
        Func<DbContext> _resolveDbContext;
        Func<IUnitOfWork> _resolveUnitOfWork;
        DbContextScopedFreeSql() { }

        public static DbContextScopedFreeSql Create(IFreeSql fsql, Func<DbContext> resolveDbContext, Func<IUnitOfWork> resolveUnitOfWork)
        {
            if (fsql == null) return null;
            var scopedfsql = fsql as DbContextScopedFreeSql;
            if (scopedfsql == null)
                return new DbContextScopedFreeSql
                {
                    _originalFsql = fsql,
                    _resolveDbContext = resolveDbContext,
                    _resolveUnitOfWork = resolveUnitOfWork,
                    Ado = new ScopeTransactionAdo(fsql.Ado as AdoProvider, () =>
                    {
                        var db = resolveDbContext?.Invoke();
                        db?.FlushCommand();
                        return resolveUnitOfWork?.Invoke()?.GetOrBeginTransaction();
                    })
                };
            return Create(scopedfsql._originalFsql, resolveDbContext, resolveUnitOfWork);
        }

        class ScopeTransactionAdo : AdoProvider
        {
            AdoProvider _ado;
            public ScopeTransactionAdo(AdoProvider ado, Func<DbTransaction> resolveTran) : base(ado.DataType, null, null)
            {
                _ado = ado;
                base.ResolveTransaction = resolveTran;
                base.ConnectionString = ado.ConnectionString;
                base.SlaveConnectionStrings = ado.SlaveConnectionStrings;
                base.Identifier = ado.Identifier;
                base.MasterPool = ado.MasterPool;
                base._util = ado._util;
            }
            public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn) => _ado.AddslashesProcessParam(param, mapType, mapColumn);
            public override DbCommand CreateCommand() => _ado.CreateCommand();
            public override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _ado.GetDbParamtersByObject(sql, obj);
            public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex) => _ado.ReturnConnection(pool, conn, ex);
        }
        public IAdo Ado { get; private set; }
        public IAop Aop => _originalFsql.Aop;
        public ICodeFirst CodeFirst => _originalFsql.CodeFirst;
        public IDbFirst DbFirst => _originalFsql.DbFirst;
        public GlobalFilter GlobalFilter => _originalFsql.GlobalFilter;
        public void Dispose() { }

        public void Transaction(Action handler) => _originalFsql.Transaction(handler);
        public void Transaction(IsolationLevel isolationLevel, Action handler) => _originalFsql.Transaction(isolationLevel, handler);

        public ISelect<T1> Select<T1>() where T1 : class
        {
            var db = _resolveDbContext?.Invoke();
            db?.FlushCommand();
            var select = _originalFsql.Select<T1>().WithTransaction(_resolveUnitOfWork?.Invoke()?.GetOrBeginTransaction(false));
            if (db?.Options.EnableGlobalFilter == false) select.DisableGlobalFilter();
            return select;
        }
        public ISelect<T1> Select<T1>(object dywhere) where T1 : class => Select<T1>().WhereDynamic(dywhere);

        public IDelete<T1> Delete<T1>() where T1 : class
        {
            var db = _resolveDbContext?.Invoke();
            db?.FlushCommand();
            var delete = _originalFsql.Delete<T1>().WithTransaction(_resolveUnitOfWork?.Invoke()?.GetOrBeginTransaction());
            if (db?.Options.EnableGlobalFilter == false) delete.DisableGlobalFilter();
            return delete;
        }
        public IDelete<T1> Delete<T1>(object dywhere) where T1 : class => Delete<T1>().WhereDynamic(dywhere);

        public IUpdate<T1> Update<T1>() where T1 : class
        {
            var db = _resolveDbContext?.Invoke();
            db?.FlushCommand();
            var update = _originalFsql.Update<T1>().WithTransaction(_resolveUnitOfWork?.Invoke()?.GetOrBeginTransaction());
            if (db?.Options.NoneParameter != null) update.NoneParameter(db.Options.NoneParameter.Value);
            if (db?.Options.EnableGlobalFilter == false) update.DisableGlobalFilter();
            return update;
        }
        public IUpdate<T1> Update<T1>(object dywhere) where T1 : class => Update<T1>().WhereDynamic(dywhere);

        public IInsert<T1> Insert<T1>() where T1 : class
        {
            var db = _resolveDbContext?.Invoke();
            db?.FlushCommand();
            var insert = _originalFsql.Insert<T1>().WithTransaction(_resolveUnitOfWork?.Invoke()?.GetOrBeginTransaction());
            if (db?.Options.NoneParameter != null) insert.NoneParameter(db.Options.NoneParameter.Value);
            return insert;
        }
        public IInsert<T1> Insert<T1>(T1 source) where T1 : class => Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(T1[] source) where T1 : class => Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(List<T1> source) where T1 : class => Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(IEnumerable<T1> source) where T1 : class => Insert<T1>().AppendData(source);

        public IInsertOrUpdate<T1> InsertOrUpdate<T1>() where T1 : class
        {
            var db = _resolveDbContext?.Invoke();
            db?.FlushCommand();
            return _originalFsql.InsertOrUpdate<T1>().WithTransaction(_resolveUnitOfWork?.Invoke()?.GetOrBeginTransaction());
        }
    }
}
