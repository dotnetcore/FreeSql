using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;

namespace FreeSql.IoTDB.Curd
{
    internal sealed class IoTDBSelect<T1> : Select1Provider<T1>
    {
        public IoTDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere) { }

        public override string ToSql(string field = null)
        {
            if (_tables.Count != 1 || _join.Length > 0)
                throw new NotSupportedException("IoTDB Tree queries are scoped to one device path; joins and multi-device FROM clauses are not supported.");
            if (!string.IsNullOrWhiteSpace(_groupby) || !string.IsNullOrWhiteSpace(_having))
                throw new NotSupportedException("Generic SQL GROUP BY/HAVING is not supported by the IoTDB Tree provider. Use IoTDB GROUP BY TIME with raw SQL.");
            if (_is_AsTreeCte || _diymemexpWithTempQuery != null)
                throw new NotSupportedException("FreeSql CTE and nested-query composition is not supported by the IoTDB Tree provider.");
            if (_skip > 0 && _limit <= 0)
                throw new NotSupportedException("IoTDB OFFSET requires a positive LIMIT.");

            var table = _tables[0].Table;
            if (table == null) throw new InvalidOperationException("IoTDB requires a mapped entity device path.");
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(table.Type);
            var path = IoTDBUtils.RequireDevicePath(_tableRule?.Invoke(table.Type, table.DbName) ?? table.DbName);
            if (table.AsTableImpl != null)
                throw new NotSupportedException("IoTDB Tree entities cannot use FreeSql AsTable partition routing.");

            if (_whereGlobalFilter?.Any() == true)
            {
                _tables[0].Cascade = _commonExpression.GetWhereCascadeSql(_tables[0], _whereGlobalFilter.Where(filter => !filter.Before), true);
                _tables[0].CascadeBefore = _commonExpression.GetWhereCascadeSql(_tables[0], _whereGlobalFilter.Where(filter => filter.Before), true);
            }

            var alias = _tables[0].Alias;
            string StripAlias(string sql) => string.IsNullOrEmpty(sql) || string.IsNullOrEmpty(alias)
                ? sql
                : Regex.Replace(sql, @"\b" + Regex.Escape(alias) + @"\s*\.", string.Empty, RegexOptions.IgnoreCase);

            var result = new StringBuilder(_select);
            if (_distinct) result.Append("DISTINCT ");
            result.Append(StripAlias(field ?? GetAllFieldExpressionTreeLevel2().Field)).Append("\r\nFROM ").Append(path);

            var where = new StringBuilder();
            if (!string.IsNullOrEmpty(_tables[0].CascadeBefore)) where.Append(" AND ").Append(_tables[0].CascadeBefore);
            where.Append(_where);
            if (!string.IsNullOrEmpty(_tables[0].Cascade)) where.Append(" AND ").Append(_tables[0].Cascade);
            if (where.Length > 0)
            {
                var whereSql = where.ToString();
                if (whereSql.StartsWith(" AND ", StringComparison.OrdinalIgnoreCase)) whereSql = whereSql.Substring(5);
                result.Append("\r\nWHERE ").Append(StripAlias(whereSql));
            }

            if (!string.IsNullOrWhiteSpace(_orderby)) result.Append(StripAlias(_orderby));
            if (_limit > 0)
            {
                result.Append("\r\nLIMIT ").Append(_limit);
                if (_skip > 0) result.Append(" OFFSET ").Append(_skip);
            }
            if (!string.IsNullOrEmpty(_tosqlAppendContent)) result.Append(_tosqlAppendContent);
            return result.ToString();
        }

        public override ISelect<T1, T2> From<T2>(Expression<Func<ISelectFromExpression<T1>, T2, ISelectFromExpression<T1>>> exp = null) => throw UnsupportedJoin();
        public override ISelect<T1, T2, T3> From<T2, T3>(Expression<Func<ISelectFromExpression<T1>, T2, T3, ISelectFromExpression<T1>>> exp = null) => throw UnsupportedJoin();
        public override ISelect<T1, T2, T3, T4> From<T2, T3, T4>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, ISelectFromExpression<T1>>> exp = null) => throw UnsupportedJoin();
        public override ISelect<T1, T2, T3, T4, T5> From<T2, T3, T4, T5>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, ISelectFromExpression<T1>>> exp = null) => throw UnsupportedJoin();
        public override ISelect<T1, T2, T3, T4, T5, T6> From<T2, T3, T4, T5, T6>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, ISelectFromExpression<T1>>> exp = null) => throw UnsupportedJoin();
        public override ISelect<T1, T2, T3, T4, T5, T6, T7> From<T2, T3, T4, T5, T6, T7>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, ISelectFromExpression<T1>>> exp = null) => throw UnsupportedJoin();
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8> From<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, ISelectFromExpression<T1>>> exp = null) => throw UnsupportedJoin();
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, ISelectFromExpression<T1>>> exp = null) => throw UnsupportedJoin();
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, ISelectFromExpression<T1>>> exp = null) => throw UnsupportedJoin();
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, ISelectFromExpression<T1>>> exp = null) => throw UnsupportedJoin();
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, ISelectFromExpression<T1>>> exp = null) => throw UnsupportedJoin();
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, ISelectFromExpression<T1>>> exp = null) => throw UnsupportedJoin();
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, ISelectFromExpression<T1>>> exp = null) => throw UnsupportedJoin();
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, ISelectFromExpression<T1>>> exp = null) => throw UnsupportedJoin();
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, ISelectFromExpression<T1>>> exp = null) => throw UnsupportedJoin();

        static NotSupportedException UnsupportedJoin() => new NotSupportedException("IoTDB Tree does not support relational joins or multiple device entities in one FreeSql query.");
    }
}
