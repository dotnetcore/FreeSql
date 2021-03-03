using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FreeSql.Internal.CommonProvider
{
    public abstract partial class BaseDbProvider : IFreeSql
    {
        public abstract ISelect<T1> CreateSelectProvider<T1>(object dywhere);
        public abstract IInsert<T1> CreateInsertProvider<T1>() where T1 : class;
        public abstract IUpdate<T1> CreateUpdateProvider<T1>(object dywhere);
        public abstract IDelete<T1> CreateDeleteProvider<T1>(object dywhere);
        public abstract IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() where T1 : class;

        public ISelect<T1> Select<T1>() where T1 : class => CreateSelectProvider<T1>(null);
        public ISelect<T1> Select<T1>(object dywhere) where T1 : class => CreateSelectProvider<T1>(dywhere);
        public IInsert<T1> Insert<T1>() where T1 : class => CreateInsertProvider<T1>();
        public IInsert<T1> Insert<T1>(T1 source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(T1[] source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(List<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(IEnumerable<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IUpdate<T1> Update<T1>() where T1 : class => CreateUpdateProvider<T1>(null);
        public IUpdate<T1> Update<T1>(object dywhere) where T1 : class => CreateUpdateProvider<T1>(dywhere);
        public IDelete<T1> Delete<T1>() where T1 : class => CreateDeleteProvider<T1>(null);
        public IDelete<T1> Delete<T1>(object dywhere) where T1 : class => CreateDeleteProvider<T1>(dywhere);
        public IInsertOrUpdate<T1> InsertOrUpdate<T1>() where T1 : class => CreateInsertOrUpdateProvider<T1>();

        public virtual IAdo Ado { get; protected set; }
        public virtual IAop Aop { get; protected set; }
        public virtual ICodeFirst CodeFirst { get; protected set; }
        public virtual IDbFirst DbFirst { get; protected set; }

        public virtual CommonUtils InternalCommonUtils { get; protected set; }
        public virtual CommonExpression InternalCommonExpression { get; protected set; }

        public void Transaction(Action handler) => Ado.Transaction(handler);
        public void Transaction(IsolationLevel isolationLevel, Action handler) => Ado.Transaction(isolationLevel, handler);

        public virtual GlobalFilter GlobalFilter { get; } = new GlobalFilter();
        public abstract void Dispose();
    }
}
