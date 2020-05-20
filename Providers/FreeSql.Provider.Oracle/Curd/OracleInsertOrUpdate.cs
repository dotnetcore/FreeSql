using FreeSql.Internal;
using System;
using System.Linq;
using System.Text;

namespace FreeSql.Oracle.Curd
{

    class OracleInsertOrUpdate<T1> : Internal.CommonProvider.InsertOrUpdateProvider<T1> where T1 : class
    {
        public OracleInsertOrUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        public override string ToSql()
        {
            if (_source?.Any() != true) return null;
            if (_table.Primarys.Any() == false) throw new Exception($"InsertOrUpdate 功能要求实体类 {_table.CsName} 必须有主键");

            var sb = new StringBuilder().Append("MERGE INTO ").Append(_commonUtils.QuoteSqlName(TableRuleInvoke())).Append(" t1 \r\n")
                .Append("USING (");
            WriteSourceSelectUnionAll(sb);
            sb.Append(" ) t2 ON (").Append(string.Join(" AND ", _table.Primarys.Select(a => $"t1.{_commonUtils.QuoteSqlName(a.Attribute.Name)} = t2.{a.Attribute.Name}"))).Append(") \r\n");

            var cols = _table.Columns.Values.Where(a => a.Attribute.IsPrimary == false && a.Attribute.CanUpdate == true);
            if (cols.Any())
                sb.Append("WHEN MATCHED THEN \r\n")
                    .Append("  update set ").Append(string.Join(", ", cols.Select(a =>
                        a.Attribute.IsVersion ?
                        $"{_commonUtils.QuoteSqlName(a.Attribute.Name)} = t1.{_commonUtils.QuoteSqlName(a.Attribute.Name)} + 1" :
                        $"{_commonUtils.QuoteSqlName(a.Attribute.Name)} = t2.{a.Attribute.Name}"
                        ))).Append(" \r\n");

            cols = _table.Columns.Values;
            sb.Append("WHEN NOT MATCHED THEN \r\n")
                .Append("  insert (").Append(string.Join(", ", cols.Select(a => _commonUtils.QuoteSqlName(a.Attribute.Name)))).Append(") \r\n")
                .Append("  values (").Append(string.Join(", ", cols.Select(a => $"t2.{a.Attribute.Name}"))).Append(")");

            return sb.ToString();
        }
    }
}