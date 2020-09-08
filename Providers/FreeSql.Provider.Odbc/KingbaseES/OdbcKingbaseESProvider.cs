using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;

namespace FreeSql.Odbc.KingbaseES
{

    public class OdbcKingbaseESProvider<TMark> : IFreeSql<TMark>
    {

        public ISelect<T1> Select<T1>() where T1 : class => new OdbcKingbaseESSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public ISelect<T1> Select<T1>(object dywhere) where T1 : class => new OdbcKingbaseESSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public IInsert<T1> Insert<T1>() where T1 : class => new OdbcKingbaseESInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public IInsert<T1> Insert<T1>(T1 source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(T1[] source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(List<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(IEnumerable<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IUpdate<T1> Update<T1>() where T1 : class => new OdbcKingbaseESUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public IUpdate<T1> Update<T1>(object dywhere) where T1 : class => new OdbcKingbaseESUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public IDelete<T1> Delete<T1>() where T1 : class => new OdbcKingbaseESDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public IDelete<T1> Delete<T1>(object dywhere) where T1 : class => new OdbcKingbaseESDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public IInsertOrUpdate<T1> InsertOrUpdate<T1>() where T1 : class => new OdbcKingbaseESInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public IAdo Ado { get; }
        public IAop Aop { get; }
        public ICodeFirst CodeFirst { get; }
        public IDbFirst DbFirst { get; }
        public OdbcKingbaseESProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new OdbcKingbaseESUtils(this);
            this.InternalCommonExpression = new OdbcKingbaseESExpression(this.InternalCommonUtils);

            this.Ado = new OdbcKingbaseESAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new OdbcKingbaseESDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new OdbcKingbaseESCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        internal CommonUtils InternalCommonUtils { get; }
        internal CommonExpression InternalCommonExpression { get; }

        public void Transaction(Action handler) => Ado.Transaction(handler);
        public void Transaction(IsolationLevel isolationLevel, Action handler) => Ado.Transaction(isolationLevel, handler);

        public GlobalFilter GlobalFilter { get; } = new GlobalFilter();

        ~OdbcKingbaseESProvider() => this.Dispose();
        int _disposeCounter;
        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
