using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.ShenTong.Curd;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace FreeSql.ShenTong
{

    public class ShenTongProvider<TMark> : IFreeSql<TMark>
    {
        public ISelect<T1> Select<T1>() where T1 : class => new ShenTongSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public ISelect<T1> Select<T1>(object dywhere) where T1 : class => new ShenTongSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public IInsert<T1> Insert<T1>() where T1 : class => new ShenTongInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public IInsert<T1> Insert<T1>(T1 source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(T1[] source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(List<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(IEnumerable<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IUpdate<T1> Update<T1>() where T1 : class => new ShenTongUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public IUpdate<T1> Update<T1>(object dywhere) where T1 : class => new ShenTongUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public IDelete<T1> Delete<T1>() where T1 : class => new ShenTongDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public IDelete<T1> Delete<T1>(object dywhere) where T1 : class => new ShenTongDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public IInsertOrUpdate<T1> InsertOrUpdate<T1>() where T1 : class => new ShenTongInsertOrUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);

        public IAdo Ado { get; }
        public IAop Aop { get; }
        public ICodeFirst CodeFirst { get; }
        public IDbFirst DbFirst { get; }
        public ShenTongProvider(string masterConnectionString, string[] slaveConnectionString, Func<DbConnection> connectionFactory = null)
        {
            this.InternalCommonUtils = new ShenTongUtils(this);
            this.InternalCommonExpression = new ShenTongExpression(this.InternalCommonUtils);

            this.Ado = new ShenTongAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString, connectionFactory);
            this.Aop = new AopProvider();

            this.DbFirst = new ShenTongDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
            this.CodeFirst = new ShenTongCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        internal CommonUtils InternalCommonUtils { get; }
        internal CommonExpression InternalCommonExpression { get; }

        public void Transaction(Action handler) => Ado.Transaction(handler);
        public void Transaction(IsolationLevel isolationLevel, Action handler) => Ado.Transaction(isolationLevel, handler);

        public GlobalFilter GlobalFilter { get; } = new GlobalFilter();

        ~ShenTongProvider() => this.Dispose();
        int _disposeCounter;
        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
