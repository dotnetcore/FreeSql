using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;

namespace FreeSql.IoTDB.Curd
{
    internal sealed class IoTDBInsert<T1> : InsertProvider<T1> where T1 : class
    {
        public IoTDBInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression) { }

        public override string ToSql()
        {
            if (_source.Count > 1)
                throw new NotSupportedException("IoTDB Tree inserts are emitted one record per statement; use ExecuteAffrows for a batch.");
            var timeColumn = IoTDBCodeFirst.FindTimeColumn(_table);
            if (!timeColumn.Attribute.CanInsert || timeColumn.Attribute.IsIdentity ||
                _ignore.ContainsKey(timeColumn.Attribute.Name) || _ignore.ContainsKey(timeColumn.CsName))
                throw new InvalidOperationException("The IoTDB 'time' column must be included in every insert.");
            if (_table.AsTableImpl != null)
                throw new NotSupportedException("IoTDB Tree inserts do not support FreeSql AsTable partition routing.");

            return base.ToSql();
        }

        public override IInsert<T1> BatchOptions(int valuesLimit, int parameterLimit, bool autoTransaction = true)
        {
            if (valuesLimit > 1)
                throw new NotSupportedException("IoTDB Tree inserts use one record per statement.");
            if (autoTransaction)
                throw new NotSupportedException("IoTDB Tree does not support FreeSql batch transactions.");
            return base.BatchOptions(1, parameterLimit, false);
        }

        public override int ExecuteAffrows()
        {
            EnsureNoTransaction();
            _batchAutoTransaction = false;
            return base.SplitExecuteAffrows(1, _batchParameterLimit > 0 ? _batchParameterLimit : 1000);
        }

        public override long ExecuteIdentity() => throw new NotSupportedException("IoTDB Tree has no auto-generated identity values.");
        public override List<T1> ExecuteInserted() => throw new NotSupportedException("IoTDB Tree does not support INSERT RETURNING.");
        protected override long RawExecuteIdentity() => throw new NotSupportedException("IoTDB Tree has no auto-generated identity values.");
        protected override List<T1> RawExecuteInserted() => throw new NotSupportedException("IoTDB Tree does not support INSERT RETURNING.");
        protected override Task<long> RawExecuteIdentityAsync(CancellationToken cancellationToken = default) =>
            Task.FromException<long>(new NotSupportedException("IoTDB Tree has no auto-generated identity values."));
        protected override Task<List<T1>> RawExecuteInsertedAsync(CancellationToken cancellationToken = default) =>
            Task.FromException<List<T1>>(new NotSupportedException("IoTDB Tree does not support INSERT RETURNING."));

        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) =>
            ExecuteAffrowsAsyncCore(cancellationToken);

        public override Task<long> ExecuteIdentityAsync(CancellationToken cancellationToken = default) =>
            Task.FromException<long>(new NotSupportedException("IoTDB Tree has no auto-generated identity values."));

        public override Task<List<T1>> ExecuteInsertedAsync(CancellationToken cancellationToken = default) =>
            Task.FromException<List<T1>>(new NotSupportedException("IoTDB Tree does not support INSERT RETURNING."));

        async Task<int> ExecuteAffrowsAsyncCore(CancellationToken cancellationToken)
        {
            EnsureNoTransaction();
            _batchAutoTransaction = false;
            return await base.SplitExecuteAffrowsAsync(1,
                _batchParameterLimit > 0 ? _batchParameterLimit : 1000, cancellationToken).ConfigureAwait(false);
        }

        void EnsureNoTransaction()
        {
            if (_transaction != null || _orm.Ado.TransactionCurrentThread != null)
                throw new NotSupportedException("IoTDB Tree inserts cannot participate in FreeSql transactions.");
        }
    }
}
