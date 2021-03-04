using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.MsAccess.Curd
{

    class MsAccessUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1>
    {

        public MsAccessUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
            _batchAutoTransaction = false;
        }

        //蛋疼的 access 更新只能一条一条执行，不支持 case .. when .. then .. end，也不支持事务
        public override int ExecuteAffrows() => base.SplitExecuteAffrows(1, 1000);
        public override List<T1> ExecuteUpdated() => base.SplitExecuteUpdated(1, 1000);

        public override IUpdate<T1> BatchOptions(int rowsLimit, int parameterLimit, bool autoTransaction = true) =>
            throw new NotImplementedException("蛋疼的 access 插入只能一条一条执行，不支持 values(..),(..) 也不支持 select .. UNION ALL select ..");

        protected override List<T1> RawExecuteUpdated()
        {
            throw new NotImplementedException();
        }

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
                caseWhen.Append(MsAccessUtils.GetCastSql(_commonUtils.RereadColumn(pk, _commonUtils.QuoteSqlName(pk.Attribute.Name)), typeof(string)));
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
                sb.Append(MsAccessUtils.GetCastSql(_commonUtils.FormatSql("{0}", pk.GetDbValue(d)), typeof(string)));
                ++pkidx;
            }
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => base.SplitExecuteAffrowsAsync(1, 1000, cancellationToken);
        public override Task<List<T1>> ExecuteUpdatedAsync(CancellationToken cancellationToken = default) => base.SplitExecuteUpdatedAsync(1, 1000, cancellationToken);

        protected override Task<List<T1>> RawExecuteUpdatedAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
