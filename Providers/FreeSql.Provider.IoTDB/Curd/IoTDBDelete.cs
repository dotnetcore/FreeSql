using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;

namespace FreeSql.IoTDB.Curd
{
    internal sealed class IoTDBDelete<T1> : DeleteProvider<T1>
    {
        static readonly Regex TimePredicate = new Regex(@"(?<![\w])(?:`time`|time)(?![\w])", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public IoTDBDelete(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere) { }

        public override string ToSql()
        {
            EnsureTimePredicate();
            return base.ToSql();
        }

        public override int ExecuteAffrows()
        {
            EnsureTimePredicate();
            return base.ExecuteAffrows();
        }

        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            EnsureTimePredicate();
            return base.ExecuteAffrowsAsync(cancellationToken);
        }

        public override List<T1> ExecuteDeleted() => throw new NotSupportedException("IoTDB Tree does not support DELETE RETURNING.");
        public override Task<List<T1>> ExecuteDeletedAsync(CancellationToken cancellationToken = default) =>
            Task.FromException<List<T1>>(new NotSupportedException("IoTDB Tree does not support DELETE RETURNING."));

        void EnsureTimePredicate()
        {
            if (!TimePredicate.IsMatch(_where.ToString()))
                throw new InvalidOperationException("IoTDB Tree deletes require an explicit time predicate to bound the deleted data.");
            if (_table.AsTableImpl != null)
                throw new NotSupportedException("IoTDB Tree deletes do not support FreeSql AsTable partition routing.");
        }
    }
}
