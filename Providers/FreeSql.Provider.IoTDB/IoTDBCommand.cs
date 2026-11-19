using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.IoTDB
{
    internal sealed class IoTDBCommand : Apache.IoTDB.Data.IoTDBCommand
    {
        private string BoundSql() => IoTDBSqlParameterBinder.Bind(CommandText, Parameters.Cast<DbParameter>());

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) =>
            ExecuteDbDataReaderAsync(behavior, CancellationToken.None).GetAwaiter().GetResult();

        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Connection.State != ConnectionState.Open) await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            var data = await Connection.SessionPool.ExecuteQueryStatementAsync(BoundSql(), (long)CommandTimeout * 1000).ConfigureAwait(false);
            return new IoTDBSessionDataReader(data, Connection, (behavior & CommandBehavior.CloseConnection) != 0);
        }

        public override int ExecuteNonQuery() => ExecuteNonQueryAsync(CancellationToken.None).GetAwaiter().GetResult();

        public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            if (Connection.State != ConnectionState.Open) await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            foreach (var statement in IoTDBSqlParameterBinder.SplitStatements(BoundSql()))
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var native = new Apache.IoTDB.Data.IoTDBCommand(statement, Connection) { CommandTimeout = CommandTimeout })
                {
                    if (await native.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) < 0)
                        throw new DataException("IoTDB rejected the non-query statement.");
                }
            }
            return 0; // The Tree API reports a status, not an affected row count.
        }

        public override object ExecuteScalar() => ExecuteScalarAsync(CancellationToken.None).GetAwaiter().GetResult();

        public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            using (var reader = await ExecuteDbDataReaderAsync(CommandBehavior.SingleRow, cancellationToken).ConfigureAwait(false))
                return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? reader.GetValue(0) : null;
        }
    }
}
