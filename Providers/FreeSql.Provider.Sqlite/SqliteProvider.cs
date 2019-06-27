using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Sqlite.Curd;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace FreeSql.Sqlite
{

    public class SqliteProvider<TMark> : IFreeSql<TMark>
    {

        public ISelect<T1> Select<T1>() where T1 : class => new SqliteSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public ISelect<T1> Select<T1>(object dywhere) where T1 : class => new SqliteSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public IInsert<T1> Insert<T1>() where T1 : class => new SqliteInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
        public IInsert<T1> Insert<T1>(T1 source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(T1[] source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(IEnumerable<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
        public IUpdate<T1> Update<T1>() where T1 : class => new SqliteUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public IUpdate<T1> Update<T1>(object dywhere) where T1 : class => new SqliteUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
        public IDelete<T1> Delete<T1>() where T1 : class => new SqliteDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
        public IDelete<T1> Delete<T1>(object dywhere) where T1 : class => new SqliteDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

        public IAdo Ado { get; }
        public IAop Aop { get; }
        public ICodeFirst CodeFirst { get; }
        public IDbFirst DbFirst => null;
        public SqliteProvider(string masterConnectionString, string[] slaveConnectionString)
        {
            this.InternalCommonUtils = new SqliteUtils(this);
            this.InternalCommonExpression = new SqliteExpression(this.InternalCommonUtils);

            this.Ado = new SqliteAdo(this.InternalCommonUtils, masterConnectionString, slaveConnectionString);
            this.Aop = new AopProvider();

            this.CodeFirst = new SqliteCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
        }

        internal CommonUtils InternalCommonUtils { get; }
        internal CommonExpression InternalCommonExpression { get; }

        public void Transaction(Action handler) => Ado.Transaction(handler);

        public void Transaction(Action handler, TimeSpan timeout) => Ado.Transaction(handler, timeout);

        ~SqliteProvider()
        {
            this.Dispose();
        }
        bool _isdisposed = false;
        public void Dispose()
        {
            if (_isdisposed) return;
            (this.Ado as AdoProvider)?.Dispose();
        }
    }
}
