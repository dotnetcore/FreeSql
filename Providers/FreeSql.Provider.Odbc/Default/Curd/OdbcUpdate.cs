using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Odbc.Default
{

    class OdbcUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1>
    {
        OdbcUtils _utils;
        public OdbcUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
            _utils = _commonUtils as OdbcUtils;
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(_batchRowsLimit > 0 ? _batchRowsLimit : _utils.Adapter.UpdateBatchSplitLimit, _batchParameterLimit > 0 ? _batchParameterLimit : 255);
		protected override List<TReturn> ExecuteUpdated<TReturn>(IEnumerable<ColumnInfo> columns) => base.SplitExecuteUpdated<TReturn>(_batchRowsLimit > 0 ? _batchRowsLimit : _utils.Adapter.UpdateBatchSplitLimit, _batchParameterLimit > 0 ? _batchParameterLimit : 255, columns);

		protected override List<TReturn> RawExecuteUpdated<TReturn>(IEnumerable<ColumnInfo> columns) => throw new NotImplementedException($"FreeSql.Odbc.Default {CoreStrings.S_Not_Implemented_Feature}");

        protected override void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys)
        {
            if (primarys.Length == 1)
            {
                var pk = primarys.First();
                caseWhen.Append(_commonUtils.RereadColumn(pk, _commonUtils.QuoteSqlName(pk.Attribute.Name)));
                return;
            }
            caseWhen.Append("(");
            var pkidx = 0;
            foreach (var pk in primarys)
            {
                if (pkidx > 0) caseWhen.Append(" + '+' + ");
                caseWhen.Append(_utils.Adapter.CastSql(_commonUtils.RereadColumn(pk, _commonUtils.QuoteSqlName(pk.Attribute.Name)), _utils.Adapter.MappingOdbcTypeVarChar));
                ++pkidx;
            }
            caseWhen.Append(")");
        }

        protected override void ToSqlWhen(StringBuilder sb, ColumnInfo[] primarys, object d)
        {
            if (primarys.Length == 1)
            {
                sb.Append(_commonUtils.FormatSql("{0}", primarys[0].GetDbValue(d)));
                return;
            }
            var pkidx = 0;
            foreach (var pk in primarys)
            {
                if (pkidx > 0) sb.Append(" + '+' + ");
                sb.Append(_utils.Adapter.CastSql(_commonUtils.FormatSql("{0}", pk.GetDbValue(d)), _utils.Adapter.MappingOdbcTypeVarChar));
                ++pkidx;
            }
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => base.SplitExecuteAffrowsAsync(_batchRowsLimit > 0 ? _batchRowsLimit : _utils.Adapter.UpdateBatchSplitLimit, _batchParameterLimit > 0 ? _batchParameterLimit : 255, cancellationToken);
		protected override Task<List<TReturn>> ExecuteUpdatedAsync<TReturn>(IEnumerable<ColumnInfo> columns, CancellationToken cancellationToken = default) => base.SplitExecuteUpdatedAsync<TReturn>(_batchRowsLimit > 0 ? _batchRowsLimit : _utils.Adapter.UpdateBatchSplitLimit, _batchParameterLimit > 0 ? _batchParameterLimit : 255, columns, cancellationToken);

		protected override Task<List<TReturn>> RawExecuteUpdatedAsync<TReturn>(IEnumerable<ColumnInfo> columns, CancellationToken cancellationToken = default) => throw new NotImplementedException($"FreeSql.Odbc.Default {CoreStrings.S_Not_Implemented_Feature}");
#endif
    }
}
