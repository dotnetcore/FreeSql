// SonnetDB SELECT 查询实现。
// SonnetDB 的 SELECT 查询 SQL 生成器。
//
// SonnetDB 3.1 与标准 SQL 的主要差异：
//   1. 关系表和时序测量均支持表别名及“别名.列名”限定；
//      关系表支持 INNER/LEFT JOIN，但发布版参数绑定路径无法执行时序测量 JOIN。
//   2. ORDER BY 须位于 LIMIT / OFFSET 之前（与标准 SQL 相同，但需严格保证）。
//   3. FreeSql Count() 查询会生成 "1 as1" 占位符，
//      SonnetDB 不接受裸 1 作为 SELECT 字段，需通过 NormalizeSelectField
//      将其改写为 "count(1) as1"。
//   4. 3.1.0 仅支持普通 UNION，不支持 UNION ALL；所有会生成 UNION ALL
//      的表规则合并和 ISelect.UnionAll 路径都会在 SQL 生成前明确拒绝。
//
// 所有多表重载（T1~T16）均复用 SonnetDBSelect<T1>.ToSqlStatic，
// 以保持 SQL 生成逻辑的单一来源。

using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.SonnetDB.Curd
{

    class SonnetDBSelect<T1> : FreeSql.Internal.CommonProvider.Select1Provider<T1>
    {

        /// <summary>
        /// 生成 SonnetDB SELECT 语句的核心静态方法。
        /// 处理 FROM、JOIN、WHERE、GROUP BY、HAVING、ORDER BY、LIMIT、OFFSET 子句，
        /// 并在 SonnetDB 不支持的 JOIN 形态下提前给出明确异常。
        /// </summary>
        internal static string ToSqlStatic(CommonUtils _commonUtils, CommonExpression _commonExpression, string _select, bool _distinct, string field, StringBuilder _join, StringBuilder _where, string _groupby, string _having, string _orderby, int _skip, int _limit, List<SelectTableInfo> _tables, List<Dictionary<Type, string>> tbUnions, Func<Type, string, string> _aliasRule, string _tosqlAppendContent, List<GlobalFilter.Item> _whereGlobalFilter, IFreeSql _orm)
        {
            // 先校验查询能力，避免被拒绝的 JOIN 在自动同步结构前产生 DDL 副作用。
            ValidateCteCapabilities(_select, tbUnions);
            ValidateJoinCapabilities(_tables, _join);
            ValidateUnionCapabilities(tbUnions, _select);

            if (_orm.CodeFirst.IsAutoSyncStructure)
            {
                // 表值函数没有可同步的实体表结构。过滤掉对应的 DTO，避免
                // 自动建出一个与 forecast/knn 结果无关的时序测量。
                var syncTypes = _tables
                    .Where(a => a?.Table?.Type != null)
                    .Where(a => tbUnions?.Any(union =>
                        union != null && union.TryGetValue(a.Table.Type, out var tableName) &&
                        SonnetDBUtils.IsTableValuedFunctionExpression(tableName)) != true)
                    .Select(a => a.Table.Type)
                    .Distinct()
                    .ToArray();
                if (syncTypes.Length > 0)
                    _orm.CodeFirst.SyncStructure(syncTypes);
            }

            if (_whereGlobalFilter.Any())
                foreach (var tb in _tables.Where(a => a.Type != SelectTableInfoType.Parent))
                {
                    tb.Cascade = _commonExpression.GetWhereCascadeSql(tb, _whereGlobalFilter.Where(a => a.Before == false), true);
                    tb.CascadeBefore = _commonExpression.GetWhereCascadeSql(tb, _whereGlobalFilter.Where(a => a.Before == true), true);
                }

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
                // NormalizeSelectField 负责把 FreeSql 对 Count() 生成的 "1 as1" 改写为 "count(1) as1"。
                sb.Append(NormalizeSelectField(field, _commonUtils, _tables)).Append(" \r\nFROM ");
                var tbsjoin = _tables.Where(a => a.Type != SelectTableInfoType.From && a.Type != SelectTableInfoType.Parent && a.Type != SelectTableInfoType.WithoutJoin).ToArray();
                var tbsfrom = _tables.Where(a => a.Type == SelectTableInfoType.From).ToArray();
                for (var a = 0; a < tbsfrom.Length; a++)
                {
                    sb.Append(_commonUtils.QuoteSqlName(tbUnion[tbsfrom[a].Table.Type])).Append(" ").Append(_aliasRule?.Invoke(tbsfrom[a].Table.Type, tbsfrom[a].Alias) ?? tbsfrom[a].Alias);
                    if (tbsjoin.Length > 0)
                    {
                        // 如果存在 JOIN 查询，将 from t1, t2 改为 from t1 inner join t2 on 1 = 1。
                        for (var b = 1; b < tbsfrom.Length; b++)
                        {
                            sb.Append(" \r\nINNER JOIN ").Append(_commonUtils.QuoteSqlName(tbUnion[tbsfrom[b].Table.Type])).Append(" ").Append(_aliasRule?.Invoke(tbsfrom[b].Table.Type, tbsfrom[b].Alias) ?? tbsfrom[b].Alias);

                            if (string.IsNullOrEmpty(tbsfrom[b].NavigateCondition) &&
                                 string.IsNullOrEmpty(tbsfrom[b].On) &&
                                 string.IsNullOrEmpty(tbsfrom[b].Cascade) &&
                                 string.IsNullOrEmpty(tbsfrom[b].CascadeBefore)) sb.Append(" ON 1 = 1");
                            else sb.Append(" ON ").Append(string.Join(" AND ", new[]
                                {
                                    tbsfrom[b].CascadeBefore,
                                    tbsfrom[b].NavigateCondition ?? tbsfrom[b].On,
                                    tbsfrom[b].Cascade
                                }.Where(sql => string.IsNullOrEmpty(sql) == false)));
                        }
                        break;
                    }
                    else
                    {
                        if (a > 0 && !string.IsNullOrEmpty(tbsfrom[a].CascadeBefore)) sbnav.Append(" AND ").Append(tbsfrom[a].CascadeBefore);
                        if (!string.IsNullOrEmpty(tbsfrom[a].NavigateCondition)) sbnav.Append(" AND (").Append(tbsfrom[a].NavigateCondition).Append(")");
                        if (!string.IsNullOrEmpty(tbsfrom[a].On)) sbnav.Append(" AND (").Append(tbsfrom[a].On).Append(")");
                        if (a > 0 && !string.IsNullOrEmpty(tbsfrom[a].Cascade)) sbnav.Append(" AND ").Append(tbsfrom[a].Cascade);
                    }
                    if (a < tbsfrom.Length - 1) sb.Append(", ");
                }
                foreach (var tb in tbsjoin)
                {
                    switch (tb.Type)
                    {
                        case SelectTableInfoType.Parent:
                        case SelectTableInfoType.RawJoin:
                        case SelectTableInfoType.WithoutJoin:
                            continue;
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
                    sb.Append(_commonUtils.QuoteSqlName(tbUnion[tb.Table.Type])).Append(" ").Append(_aliasRule?.Invoke(tb.Table.Type, tb.Alias) ?? tb.Alias)
                        .Append(" ON ").Append(string.Join(" AND ", new[]
                        {
                            tb.CascadeBefore,
                            tb.On ?? tb.NavigateCondition,
                            tb.Cascade
                        }.Where(sql => string.IsNullOrEmpty(sql) == false)));
                    if (!string.IsNullOrEmpty(tb.On) && !string.IsNullOrEmpty(tb.NavigateCondition)) sbnav.Append(" AND (").Append(tb.NavigateCondition).Append(")");
                }
                if (_join.Length > 0) sb.Append(_join);

                if (!string.IsNullOrEmpty(_tables[0].CascadeBefore)) sbnav.Append(" AND ").Append(_tables[0].CascadeBefore);
                sbnav.Append(_where);
                if (!string.IsNullOrEmpty(_tables[0].Cascade)) sbnav.Append(" AND ").Append(_tables[0].Cascade);

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
                // ORDER BY 必须在 LIMIT / OFFSET 之前输出（SonnetDB 语法要求）。
                if (string.IsNullOrEmpty(_orderby) == false) sb.Append(_orderby);
                if (_limit > 0)
                    sb.Append(" \r\nlimit ").Append(_limit);
                if (_skip > 0)
                    sb.Append(" \r\noffset ").Append(_skip);

                sbnav.Clear();
                if (tbUnionsGt0) sb.Append(") ftb");
            }
            // SonnetDB 3.1 支持关系表和时序测量的表别名。
            // 保留列限定符，避免同名列产生歧义。
            return sb.Append(_tosqlAppendContent).ToString();
        }

        static void ValidateJoinCapabilities(List<SelectTableInfo> tables, StringBuilder rawJoin)
        {
            if (tables == null || tables.Count == 0) return;

            // Parent（外层引用项）是相关子查询引用的外层表，不是当前 SELECT 的实际数据源；
            // WithoutJoin（无连接项）也可能只是表达式解析留下的辅助条目。
            // 只有真实 From/JOIN 条目才参与 SonnetDB 模型能力判断。
            var sources = tables.Where(a => a != null &&
                a.Type != SelectTableInfoType.Parent &&
                a.Type != SelectTableInfoType.WithoutJoin).ToArray();
            var explicitJoins = sources.Where(a => a.Type == SelectTableInfoType.LeftJoin ||
                                                   a.Type == SelectTableInfoType.InnerJoin ||
                                                   a.Type == SelectTableInfoType.RightJoin).ToArray();
            var hasRawJoin = (rawJoin != null && rawJoin.Length > 0) ||
                             sources.Any(a => a.Type == SelectTableInfoType.RawJoin);

            // 字符串 JOIN 无法确认参与对象是否都是关系表，也可能包含 SonnetDB 不支持的语法。
            if (hasRawJoin)
                throw new NotSupportedException(
                    "SonnetDB 3.1 已拒绝原始 SQL JOIN：无法确认参与对象是否为关系表，且不支持 RIGHT/FULL 等任意 JOIN 语法。");

            if (explicitJoins.Any(a => a.Type == SelectTableInfoType.RightJoin))
                throw new NotSupportedException(
                    "SonnetDB 3.1 的关系表 JOIN 仅支持 INNER JOIN 和 LEFT JOIN，不支持 RIGHT JOIN。");

            // 没有显式 JOIN 时，多个 From 会生成隐式笛卡尔积；发布版关系执行器不接受这种写法。
            if (explicitJoins.Length == 0 && sources.Count(a => a.Type == SelectTableInfoType.From) > 1)
                throw new NotSupportedException(
                    "SonnetDB 3.1 不支持没有显式 JOIN 条件的多根 FROM 查询（隐式笛卡尔积），请使用 INNER JOIN 或 LEFT JOIN 并提供连接条件。");

            if (explicitJoins.Length == 0) return;

            // 参数绑定后，时序 JOIN 的 AST 会被放入 JoinClauses，而时序执行器仍读取旧字段，
            // 因此当前版本只能安全执行关系表之间的 JOIN。
            if (sources.Any(a => a.Table != null && SonnetDBModel.IsMeasurement(a.Table)))
                throw new NotSupportedException(
                    "SonnetDB 3.1 发布版的参数绑定路径会丢失时序测量 JOIN AST，当前拒绝时序测量参与 JOIN；请改用关系表或拆分查询。");
        }

        static void ValidateUnionCapabilities(List<Dictionary<Type, string>> tableUnions, string selectPrefix)
        {
            // SonnetDB 3.1 的 ParseSelect 在 UNION 后只接受 SELECT，不会消费 ALL。
            // 表规则分支和 ISelect.UnionAll 都会把 UNION ALL 交给该解析器，必须在
            // 生成/执行前拒绝，不能静默改写为会去重的普通 UNION。
            if (tableUnions == null) return;
            if (tableUnions.Count > 1 ||
                tableUnions.SelectMany(a => a?.Values ?? Enumerable.Empty<string>())
                    .Any(ContainsUnionAll) ||
                ContainsUnionAll(selectPrefix))
            {
                throw new NotSupportedException(
                    "SonnetDB 3.1 不支持 UNION ALL；当前查询或表规则合并会生成 UNION ALL，" +
                    "请改用普通 UNION（结果会去重）或等待 SonnetDB 补齐 UNION ALL 解析与执行。");
            }
        }

        static void ValidateCteCapabilities(string selectPrefix, List<Dictionary<Type, string>> tableUnions)
        {
            if (ContainsCte(selectPrefix) ||
                tableUnions?.SelectMany(a => a?.Values ?? Enumerable.Empty<string>()).Any(ContainsCte) == true)
                throw new NotSupportedException(
                    "SonnetDB 3.1 不支持 WITH/递归 CTE 查询；FreeSql AsTreeCte 或 WithSql 表规则不可用，请改写为普通查询或分步处理。");
        }

        static bool ContainsCte(string sql)
        {
            // WithSql 会把原始 SQL 包在括号中；允许任意空白和一层或多层
            // 派生表括号，避免 WITH 被当作普通表名继续生成 SQL。
            return string.IsNullOrEmpty(sql) == false &&
                Regex.IsMatch(sql, @"(?:^|[\(\s])WITH(?:\s+RECURSIVE)?\s+[A-Za-z_""\[]", RegexOptions.IgnoreCase);
        }

        static bool ContainsUnionAll(string sql)
        {
            return string.IsNullOrEmpty(sql) == false &&
                Regex.IsMatch(sql, @"\bUNION\s+ALL\b", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 规范化 SELECT 字段列表。
        /// <para>FreeSql 对 <c>.Count()</c> 查询会生成裸 <c>1 as1</c> 作为 SELECT 字段占位符，
        /// SonnetDB 不接受裸整数作为 SELECT 字段，需将其改写为 <c>count(1) as1</c>。</para>
        /// <para>SonnetDB 1.1.0+ 原生支持 <c>count(1)</c> 等价于 <c>count(*)</c>。</para>
        /// </summary>
        // SonnetDB 1.1.0+ 原生支持 count(1)，其语义等同于 count(*)。
        // 仅将 FreeSql Count() 查询生成的裸 "1 as1" 改写为 "count(1) as1"。
        static string NormalizeSelectField(string field, CommonUtils commonUtils, List<SelectTableInfo> tables)
        {
            field = Regex.Replace(field, @"^\s*1\s+as1\s*$", "count(1) as1", RegexOptions.IgnoreCase);
            return field;
        }

        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override ISelect<T1, T2> From<T2>(Expression<Func<ISelectFromExpression<T1>, T2, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new SonnetDBSelect<T1, T2>(_orm, _commonUtils, _commonExpression, null); SonnetDBSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3> From<T2, T3>(Expression<Func<ISelectFromExpression<T1>, T2, T3, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new SonnetDBSelect<T1, T2, T3>(_orm, _commonUtils, _commonExpression, null); SonnetDBSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4> From<T2, T3, T4>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new SonnetDBSelect<T1, T2, T3, T4>(_orm, _commonUtils, _commonExpression, null); SonnetDBSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5> From<T2, T3, T4, T5>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new SonnetDBSelect<T1, T2, T3, T4, T5>(_orm, _commonUtils, _commonExpression, null); SonnetDBSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6> From<T2, T3, T4, T5, T6>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new SonnetDBSelect<T1, T2, T3, T4, T5, T6>(_orm, _commonUtils, _commonExpression, null); SonnetDBSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7> From<T2, T3, T4, T5, T6, T7>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7>(_orm, _commonUtils, _commonExpression, null); SonnetDBSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8> From<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8>(_orm, _commonUtils, _commonExpression, null); SonnetDBSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>(_orm, _commonUtils, _commonExpression, null); SonnetDBSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(_orm, _commonUtils, _commonExpression, null); SonnetDBSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(_orm, _commonUtils, _commonExpression, null); SonnetDBSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(_orm, _commonUtils, _commonExpression, null); SonnetDBSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(_orm, _commonUtils, _commonExpression, null); SonnetDBSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(_orm, _commonUtils, _commonExpression, null); SonnetDBSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(_orm, _commonUtils, _commonExpression, null); SonnetDBSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, ISelectFromExpression<T1>>> exp) { this.InternalFrom(exp); var ret = new SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(_orm, _commonUtils, _commonExpression, null); SonnetDBSelect<T1>.CopyData(this, ret, exp?.Parameters); return ret; }
        public override string ToSql(string field = null) => ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2> : FreeSql.Internal.CommonProvider.Select2Provider<T1, T2> where T2 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3> : FreeSql.Internal.CommonProvider.Select3Provider<T1, T2, T3> where T2 : class where T3 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4> : FreeSql.Internal.CommonProvider.Select4Provider<T1, T2, T3, T4> where T2 : class where T3 : class where T4 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5> : FreeSql.Internal.CommonProvider.Select5Provider<T1, T2, T3, T4, T5> where T2 : class where T3 : class where T4 : class where T5 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6> : FreeSql.Internal.CommonProvider.Select6Provider<T1, T2, T3, T4, T5, T6> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7> : FreeSql.Internal.CommonProvider.Select7Provider<T1, T2, T3, T4, T5, T6, T7> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8> : FreeSql.Internal.CommonProvider.Select8Provider<T1, T2, T3, T4, T5, T6, T7, T8> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> : FreeSql.Internal.CommonProvider.Select9Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : FreeSql.Internal.CommonProvider.Select10Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }

    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : FreeSql.Internal.CommonProvider.Select11Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : FreeSql.Internal.CommonProvider.Select12Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : FreeSql.Internal.CommonProvider.Select13Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : FreeSql.Internal.CommonProvider.Select14Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : FreeSql.Internal.CommonProvider.Select15Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : FreeSql.Internal.CommonProvider.Select16Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class where T16 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
}
