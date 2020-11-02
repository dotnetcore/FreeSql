using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.KingbaseES
{

    class KingbaseESSelect<T1> : FreeSql.Internal.CommonProvider.Select1Provider<T1>
    {

        internal static string ToSqlStatic(CommonUtils _commonUtils, CommonExpression _commonExpression, string _select, bool _distinct, string field, StringBuilder _join, StringBuilder _where, string _groupby, string _having, string _orderby, int _skip, int _limit, List<SelectTableInfo> _tables, List<Dictionary<Type, string>> tbUnions, Func<Type, string, string> _aliasRule, string _tosqlAppendContent, List<GlobalFilter.Item> _whereGlobalFilter, IFreeSql _orm)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure)
                _orm.CodeFirst.SyncStructure(_tables.Select(a => a.Table.Type).ToArray());

            if (_whereGlobalFilter.Any())
                foreach (var tb in _tables.Where(a => a.Type != SelectTableInfoType.Parent))
                    tb.Cascade = _commonExpression.GetWhereCascadeSql(tb, _whereGlobalFilter, true);

            var sb = new StringBuilder();
            var tbUnionsGt0 = tbUnions.Count > 1;
            for (var tbUnionsIdx = 0; tbUnionsIdx < tbUnions.Count; tbUnionsIdx++)
            {
                if (tbUnionsIdx > 0) sb.Append("\r\n \r\nUNION ALL\r\n \r\n");
                if (tbUnionsGt0) sb.Append(_select).Append(" * from (");
                var tbUnion = tbUnions[tbUnionsIdx];

                var sbnav = new StringBuilder();
                sb.Append(_select);
                if (_distinct) sb.Append("DISTINCT ");
                sb.Append(field).Append(" \r\nFROM ");
                var tbsjoin = _tables.Where(a => a.Type != SelectTableInfoType.From).ToArray();
                var tbsfrom = _tables.Where(a => a.Type == SelectTableInfoType.From).ToArray();
                for (var a = 0; a < tbsfrom.Length; a++)
                {
                    sb.Append(_commonUtils.QuoteSqlName(tbUnion[tbsfrom[a].Table.Type])).Append(" ").Append(_aliasRule?.Invoke(tbsfrom[a].Table.Type, tbsfrom[a].Alias) ?? tbsfrom[a].Alias);
                    if (tbsjoin.Length > 0)
                    {
                        //如果存在 join 查询，则处理 from t1, t2 改为 from t1 inner join t2 on 1 = 1
                        for (var b = 1; b < tbsfrom.Length; b++)
                        {
                            sb.Append(" \r\nLEFT JOIN ").Append(_commonUtils.QuoteSqlName(tbUnion[tbsfrom[b].Table.Type])).Append(" ").Append(_aliasRule?.Invoke(tbsfrom[b].Table.Type, tbsfrom[b].Alias) ?? tbsfrom[b].Alias);

                            if (string.IsNullOrEmpty(tbsfrom[b].NavigateCondition) && string.IsNullOrEmpty(tbsfrom[b].On) && string.IsNullOrEmpty(tbsfrom[b].Cascade)) sb.Append(" ON 1 = 1");
                            else
                            {
                                var onSql = tbsfrom[b].NavigateCondition ?? tbsfrom[b].On;
                                sb.Append(" ON ").Append(onSql);
                                if (string.IsNullOrEmpty(tbsfrom[b].Cascade) == false)
                                {
                                    if (string.IsNullOrEmpty(onSql)) sb.Append(tbsfrom[b].Cascade);
                                    else sb.Append(" AND ").Append(tbsfrom[b].Cascade);
                                }
                            }
                        }
                        break;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(tbsfrom[a].NavigateCondition)) sbnav.Append(" AND (").Append(tbsfrom[a].NavigateCondition).Append(")");
                        if (!string.IsNullOrEmpty(tbsfrom[a].On)) sbnav.Append(" AND (").Append(tbsfrom[a].On).Append(")");
                        if (a > 0 && !string.IsNullOrEmpty(tbsfrom[a].Cascade)) sbnav.Append(" AND ").Append(tbsfrom[a].Cascade);
                    }
                    if (a < tbsfrom.Length - 1) sb.Append(", ");
                }
                foreach (var tb in tbsjoin)
                {
                    if (tb.Type == SelectTableInfoType.Parent) continue;
                    switch (tb.Type)
                    {
                        case SelectTableInfoType.LeftJoin:
                            sb.Append(" \r\nLEFT JOIN ");
                            break;
                        case SelectTableInfoType.InnerJoin:
                            sb.Append(" \r\nINNER JOIN ");
                            break;
                        case SelectTableInfoType.RightJoin:
                            sb.Append(" \r\nRIGHT JOIN ");
                            break;
                    }
                    sb.Append(_commonUtils.QuoteSqlName(tbUnion[tb.Table.Type])).Append(" ").Append(_aliasRule?.Invoke(tb.Table.Type, tb.Alias) ?? tb.Alias).Append(" ON ").Append(tb.On ?? tb.NavigateCondition);
                    if (!string.IsNullOrEmpty(tb.Cascade)) sb.Append(" AND ").Append(tb.Cascade);
                    if (!string.IsNullOrEmpty(tb.On) && !string.IsNullOrEmpty(tb.NavigateCondition)) sbnav.Append(" AND (").Append(tb.NavigateCondition).Append(")");
                }
                if (_join.Length > 0) sb.Append(_join);

                sbnav.Append(_where);
                if (!string.IsNullOrEmpty(_tables[0].Cascade))
                    sbnav.Append(" AND ").Append(_tables[0].Cascade);

                if (sbnav.Length > 0)
                {
                    sb.Append(" \r\nWHERE ").Append(sbnav.Remove(0, 5));
                }
                if (string.IsNullOrEmpty(_groupby) == false)
                {
                    sb.Append(_groupby);
                    if (string.IsNullOrEmpty(_having) == false)
                        sb.Append(" \r\nHAVING ").Append(_having.Substring(5));
                }
                sb.Append(_orderby);
                if (_limit > 0)
                    sb.Append(" \r\nlimit ").Append(_limit);
                if (_skip > 0)
                    sb.Append(" \r\noffset ").Append(_skip);

                sbnav.Clear();
                if (tbUnionsGt0) sb.Append(") ftb");
            }
            return sb.Append(_tosqlAppendContent).ToString();
        }

        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override ISelect<T1, T2> From<T2>(Expression<Func<ISelectFromExpression<T1>, T2, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new KingbaseESSelect<T1, T2>(_orm, _commonUtils, _commonExpression, null); KingbaseESSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3> From<T2, T3>(Expression<Func<ISelectFromExpression<T1>, T2, T3, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new KingbaseESSelect<T1, T2, T3>(_orm, _commonUtils, _commonExpression, null); KingbaseESSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4> From<T2, T3, T4>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new KingbaseESSelect<T1, T2, T3, T4>(_orm, _commonUtils, _commonExpression, null); KingbaseESSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5> From<T2, T3, T4, T5>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new KingbaseESSelect<T1, T2, T3, T4, T5>(_orm, _commonUtils, _commonExpression, null); KingbaseESSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6> From<T2, T3, T4, T5, T6>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new KingbaseESSelect<T1, T2, T3, T4, T5, T6>(_orm, _commonUtils, _commonExpression, null); KingbaseESSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7> From<T2, T3, T4, T5, T6, T7>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7>(_orm, _commonUtils, _commonExpression, null); KingbaseESSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8> From<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8>(_orm, _commonUtils, _commonExpression, null); KingbaseESSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>(_orm, _commonUtils, _commonExpression, null); KingbaseESSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(_orm, _commonUtils, _commonExpression, null); KingbaseESSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(_orm, _commonUtils, _commonExpression, null); KingbaseESSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(_orm, _commonUtils, _commonExpression, null); KingbaseESSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(_orm, _commonUtils, _commonExpression, null); KingbaseESSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(_orm, _commonUtils, _commonExpression, null); KingbaseESSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(_orm, _commonUtils, _commonExpression, null); KingbaseESSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(_orm, _commonUtils, _commonExpression, null); KingbaseESSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override string ToSql(string field = null) => ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class KingbaseESSelect<T1, T2> : FreeSql.Internal.CommonProvider.Select2Provider<T1, T2> where T2 : class
    {
        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => KingbaseESSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class KingbaseESSelect<T1, T2, T3> : FreeSql.Internal.CommonProvider.Select3Provider<T1, T2, T3> where T2 : class where T3 : class
    {
        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => KingbaseESSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class KingbaseESSelect<T1, T2, T3, T4> : FreeSql.Internal.CommonProvider.Select4Provider<T1, T2, T3, T4> where T2 : class where T3 : class where T4 : class
    {
        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => KingbaseESSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class KingbaseESSelect<T1, T2, T3, T4, T5> : FreeSql.Internal.CommonProvider.Select5Provider<T1, T2, T3, T4, T5> where T2 : class where T3 : class where T4 : class where T5 : class
    {
        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => KingbaseESSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class KingbaseESSelect<T1, T2, T3, T4, T5, T6> : FreeSql.Internal.CommonProvider.Select6Provider<T1, T2, T3, T4, T5, T6> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class
    {
        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => KingbaseESSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7> : FreeSql.Internal.CommonProvider.Select7Provider<T1, T2, T3, T4, T5, T6, T7> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class
    {
        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => KingbaseESSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8> : FreeSql.Internal.CommonProvider.Select8Provider<T1, T2, T3, T4, T5, T6, T7, T8> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class
    {
        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => KingbaseESSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> : FreeSql.Internal.CommonProvider.Select9Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class
    {
        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => KingbaseESSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : FreeSql.Internal.CommonProvider.Select10Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class
    {
        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => KingbaseESSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }

    class KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : FreeSql.Internal.CommonProvider.Select11Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class
    {
        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => KingbaseESSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : FreeSql.Internal.CommonProvider.Select12Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class
    {
        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => KingbaseESSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : FreeSql.Internal.CommonProvider.Select13Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class
    {
        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => KingbaseESSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : FreeSql.Internal.CommonProvider.Select14Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class
    {
        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => KingbaseESSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : FreeSql.Internal.CommonProvider.Select15Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class
    {
        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => KingbaseESSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class KingbaseESSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : FreeSql.Internal.CommonProvider.Select16Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class where T16 : class
    {
        public KingbaseESSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => KingbaseESSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
}
