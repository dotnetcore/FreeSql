using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Oracle.Curd
{

    class OracleUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1> where T1 : class
    {

        public OracleUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(200, 999);
        public override Task<int> ExecuteAffrowsAsync() => base.SplitExecuteAffrowsAsync(200, 999);
        public override List<T1> ExecuteUpdated() => base.SplitExecuteUpdated(200, 999);
        public override Task<List<T1>> ExecuteUpdatedAsync() => base.SplitExecuteUpdatedAsync(200, 999);


        protected override List<T1> RawExecuteUpdated()
        {
            throw new NotImplementedException();
        }
        protected override Task<List<T1>> RawExecuteUpdatedAsync()
        {
            throw new NotImplementedException();
        }

        protected override void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys)
        {
            if (_table.Primarys.Length == 1)
            {
                caseWhen.Append(_commonUtils.QuoteReadColumn(_table.Primarys.First().Attribute.MapType, _commonUtils.QuoteSqlName(_table.Primarys.First().Attribute.Name)));
                return;
            }
            caseWhen.Append("(");
            var pkidx = 0;
            foreach (var pk in _table.Primarys)
            {
                if (pkidx > 0) caseWhen.Append(" || ");
                caseWhen.Append(_commonUtils.QuoteReadColumn(pk.Attribute.MapType, _commonUtils.QuoteSqlName(pk.Attribute.Name)));
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
            sb.Append("(");
            var pkidx = 0;
            foreach (var pk in _table.Primarys)
            {
                if (pkidx > 0) sb.Append(" || ");
                sb.Append(_commonUtils.FormatSql("{0}", pk.GetMapValue(d)));
                ++pkidx;
            }
            sb.Append(")");
        }
    }
}
