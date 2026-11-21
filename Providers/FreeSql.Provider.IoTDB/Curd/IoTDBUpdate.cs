using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;

namespace FreeSql.IoTDB.Curd
{
    internal sealed class IoTDBUpdate<T1> : UpdateProvider<T1>
    {
        public IoTDBUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere) { }

        public override string ToSql() => throw Unsupported();
        public override int ExecuteAffrows() => throw Unsupported();
        protected override List<TReturn> ExecuteUpdated<TReturn>(IEnumerable<ColumnInfo> columns) => throw Unsupported();
        protected override List<TReturn> RawExecuteUpdated<TReturn>(IEnumerable<ColumnInfo> columns) => throw Unsupported();
        protected override Task<List<TReturn>> RawExecuteUpdatedAsync<TReturn>(IEnumerable<ColumnInfo> columns, CancellationToken cancellationToken = default) => Task.FromException<List<TReturn>>(Unsupported());
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => Task.FromException<int>(Unsupported());
        protected override Task<List<TReturn>> ExecuteUpdatedAsync<TReturn>(IEnumerable<ColumnInfo> columns, CancellationToken cancellationToken = default) => Task.FromException<List<TReturn>>(Unsupported());
        protected override void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys) => throw Unsupported();
        protected override void ToSqlWhen(StringBuilder sb, ColumnInfo[] primarys, object source) => throw Unsupported();

        static NotSupportedException Unsupported() => new NotSupportedException("IoTDB Tree does not support UPDATE. Use INSERT for point replacement or DELETE with a bounded time predicate followed by INSERT.");
    }
}
