using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Sqlite.Curd;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;

namespace FreeSql.Sqlite
{

    public class SqliteProvider<TMark> : IFreeSql<TMark>
    {

        public ISelect<T1> Select<T1>() where T1 : class => new SqliteSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public ISelect<T1> Select<T1>(object dywhere) where T1 : class => new SqliteSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public IInsert<T1> Insert<T1>() where T1 : class => new SqliteInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public IInsert<T1> Insert<T1>(T1 source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(T1[] source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(List<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(IEnumerable<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IUpdate<T1> Update<T1>() where T1 : class => new SqliteUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public IUpdate<T1> Update<T1>(object dywhere) where T1 : class => new SqliteUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public IDelete<T1> Delete<T1>() where T1 : class => new SqliteDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public IDelete<T1> Delete<T1>(object dywhere) where T1 : class => new SqliteDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public IAdo Ado { get; }
        public IAop Aop { get; }
        public ICodeFirst CodeFirst { get; }
        public IDbFirst DbFirst => throw new NotImplementedException("FreeSql.Provider.Sqlite 未实现该功能");
        public SqliteProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new SqliteUtils(this);
            this.InternalCommonExpression = new SqliteExpression(this.InternalCommonUtils);

            this.Ado = new SqliteAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.CodeFirst = new SqliteCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            if (connectionFactory != null) this.CodeFirst.IsNoneCommandParameter = true;
        }

        internal CommonUtils InternalCommonUtils { get; }
        internal CommonExpression InternalCommonExpression { get; }

        public void Transaction(Action handler) => Ado.Transaction(handler);
        public void Transaction(TimeSpan timeout, Action handler) => Ado.Transaction(timeout, handler);
        public void Transaction(IsolationLevel isolationLevel, TimeSpan timeout, Action handler) => Ado.Transaction(isolationLevel, timeout, handler);

        public GlobalFilter GlobalFilter { get; } = new GlobalFilter();

        ~SqliteProvider() => this.Dispose();
        int _disposeCounter;
        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
