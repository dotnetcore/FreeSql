using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace FreeSql.Odbc.Dameng
{

    class OdbcDamengInsertOrUpdate<T1> : Internal.CommonProvider.InsertOrUpdateProvider<T1> where T1 : class
    {
        public OdbcDamengInsertOrUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        public override string ToSql()
        {
            if (_source?.Any() != true) return null;

            var sqls = new string[2];
            var dbParams = new List<DbParameter>();
            var ds = SplitSourceByIdentityValueIsNull(_source);
            if (ds.Item1.Any()) sqls[0] = getMergeSql(ds.Item1);
            if (ds.Item2.Any()) sqls[1] = getInsertSql(ds.Item2);
            _params = dbParams.ToArray();
            if (ds.Item2.Any() == false) return sqls[0];
            if (ds.Item1.Any() == false) return sqls[1];
            return string.Join("\r\n\r\n;\r\n\r\n", sqls);

            string getMergeSql(List<T1> data)
            {
                if (_table.Primarys.Any() == false) throw new Exception($"InsertOrUpdate 功能执行 merge into 要求实体类 {_table.CsName} 必须有主键");

                var sb = new StringBuilder().Append("MERGE INTO ").Append(_commonUtils.QuoteSqlName(TableRuleInvoke())).Append(" t1 \r\nUSING (");
                WriteSourceSelectUnionAll(data, sb, dbParams);
                sb.Append(" ) t2 ON (").Append(string.Join(" AND ", _table.Primarys.Select(a => $"t1.{_commonUtils.QuoteSqlName(a.Attribute.Name)} = t2.{a.Attribute.Name}"))).Append(") \r\n");

                var cols = _table.Columns.Values.Where(a => a.Attribute.IsPrimary == false && a.Attribute.CanUpdate == true && _updateIgnore.ContainsKey(a.Attribute.Name) == false);
                if (_doNothing == false && cols.Any())
                    sb.Append("WHEN MATCHED THEN \r\n")
                        .Append("  update set ").Append(string.Join(", ", cols.Select(a =>
                            a.Attribute.IsVersion && a.Attribute.MapType != typeof(byte[]) ?
                            $"{_commonUtils.QuoteSqlName(a.Attribute.Name)} = t1.{_commonUtils.QuoteSqlName(a.Attribute.Name)} + 1" :
                            $"{_commonUtils.QuoteSqlName(a.Attribute.Name)} = t2.{a.Attribute.Name}"
                            ))).Append(" \r\n");

                cols = _table.Columns.Values.Where(a => a.Attribute.CanInsert == true);
                if (cols.Any())
                    sb.Append("WHEN NOT MATCHED THEN \r\n")
                        .Append("  insert (").Append(string.Join(", ", cols.Select(a => _commonUtils.QuoteSqlName(a.Attribute.Name)))).Append(") \r\n")
                        .Append("  values (").Append(string.Join(", ", cols.Select(a => $"t2.{a.Attribute.Name}"))).Append(")");

                return sb.ToString();
            }
            string getInsertSql(List<T1> data)
            {
                var insert = _orm.Insert<T1>()
                    .AsTable(_tableRule).AsType(_table.Type)
                    .WithConnection(_connection)
                    .WithTransaction(_transaction)
                    .NoneParameter(true) as Internal.CommonProvider.InsertProvider<T1>;
                insert._source = data;
                var sql = insert.ToSql();
                if (string.IsNullOrEmpty(sql)) return null;
                if (insert._params?.Any() == true) dbParams.AddRange(insert._params);
                return sql;
            }
        }
    }
}