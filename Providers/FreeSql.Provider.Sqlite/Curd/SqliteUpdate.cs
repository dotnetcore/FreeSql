using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Sqlite.Curd
{

    class SqliteUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1> where T1 : class
    {

        public SqliteUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(_batchRowsLimit > 0 ? _batchRowsLimit : 200, _batchParameterLimit > 0 ? _batchParameterLimit : 999);
        public override List<T1> ExecuteUpdated() => base.SplitExecuteUpdated(_batchRowsLimit > 0 ? _batchRowsLimit : 200, _batchParameterLimit > 0 ? _batchParameterLimit : 999);


        protected override List<T1> RawExecuteUpdated()
        {
            throw new NotImplementedException();
        }

        protected override void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys)
        {
            if (_table.Primarys.Length == 1)
            {
                var pk = _table.Primarys.First();
                caseWhen.Append(_commonUtils.QuoteReadColumn(pk.CsType, pk.Attribute.MapType, _commonUtils.QuoteSqlName(pk.Attribute.Name)));
                return;
            }
            caseWhen.Append("CONCAT(");
            var pkidx = 0;
            foreach (var pk in _table.Primarys)
            {
                if (pkidx > 0) caseWhen.Append(", ");
                caseWhen.Append(_commonUtils.QuoteReadColumn(pk.CsType, pk.Attribute.MapType, _commonUtils.QuoteSqlName(pk.Attribute.Name)));
                ++pkidx;
            }
            caseWhen.Append(")");
        }

        protected override void ToSqlWhen(StringBuilder sb, ColumnInfo[] primarys, object d)
        {
            if (_table.Primarys.Length == 1)
            {
                sb.Append(_commonUtils.FormatSql("{0}", _table.Primarys.First().GetMapValue(d)));
                return;
            }
            sb.Append("CONCAT(");
            var pkidx = 0;
            foreach (var pk in _table.Primarys)
            {
                if (pkidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.FormatSql("{0}", pk.GetMapValue(d)));
                ++pkidx;
            }
            sb.Append(")");
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync() => base.SplitExecuteAffrowsAsync(200, 999);
        public override Task<List<T1>> ExecuteUpdatedAsync() => base.SplitExecuteUpdatedAsync(200, 999);

        protected override Task<List<T1>> RawExecuteUpdatedAsync()
        {
            throw new NotImplementedException();
        }
#endif
    }
}
