// SonnetDB SELECT 查询实现。
// SonnetDB 的 SELECT 查询 SQL 生成器。
//
// SonnetDB 4.0 与标准 SQL 的主要差异：
//   1. 关系表和时序测量均支持表别名及“别名.列名”限定；
//      关系表支持标准 JOIN，时序测量支持与一个关系维表的 INNER JOIN。
//   2. ORDER BY 须位于 LIMIT / OFFSET 之前（与标准 SQL 相同，但需严格保证）。
//   3. FreeSql Count() 查询会生成 "1 as1" 占位符，
//      SonnetDB 不接受裸 1 作为 SELECT 字段，需通过 NormalizeSelectField
//      将其改写为 "count(1) as1"。
//   4. CTE、递归 CTE 和 UNION ALL 使用数据库原生实现。
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
using System.Threading;
using System.Threading.Tasks;
using SonnetDB.Sql;

namespace FreeSql.SonnetDB.Curd
{

    class SonnetDBSelect<T1> : FreeSql.Internal.CommonProvider.Select1Provider<T1>, ISelect<T1>
    {

        /// <summary>
        /// 生成 SonnetDB SELECT 语句的核心静态方法。
        /// 处理 FROM、JOIN、WHERE、GROUP BY、HAVING、ORDER BY、LIMIT、OFFSET 子句，
        /// 并在 SonnetDB 不支持的 JOIN 形态下提前给出明确异常。
        /// </summary>
        internal static string ToSqlStatic(CommonUtils _commonUtils, CommonExpression _commonExpression, string _select, bool _distinct, string field, StringBuilder _join, StringBuilder _where, string _groupby, string _having, string _orderby, int _skip, int _limit, List<SelectTableInfo> _tables, List<Dictionary<Type, string>> tbUnions, Func<Type, string, string> _aliasRule, string _tosqlAppendContent, List<GlobalFilter.Item> _whereGlobalFilter, IFreeSql _orm)
        {
            // 先校验查询能力，避免被拒绝的 JOIN 在自动同步结构前产生 DDL 副作用。
            ValidateJoinCapabilities(_tables, _join);

            // FreeSql 的公共 AsTreeCte 实现为未知方言生成 WITH；SonnetDB
            // 的递归语法要求 RECURSIVE，仅调整该已有入口生成的前缀。
            if (Regex.IsMatch(_select ?? string.Empty, @"^\s*WITH\s+""?as_tree_cte""?\s+as\s*\(", RegexOptions.IgnoreCase))
                _select = Regex.Replace(_select, @"^(\s*WITH)\s+", "$1 RECURSIVE ", RegexOptions.IgnoreCase);

            // WithSql wraps its SQL in a derived table. SonnetDB accepts WITH
            // at statement scope, so move definitions out of derived sources
            // while retaining their SELECT and the caller's filters.
            var cteDefinitions = new List<string>();
            var recursiveCte = false;
            tbUnions = tbUnions.Select(union => union.ToDictionary(item => item.Key,
                item => HoistCteDefinitions(item.Value, cteDefinitions, ref recursiveCte))).ToList();
            field = EnsureMeasurementSortTime(field, _orderby, _tables);

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
                    if (a < tbsfrom.Length - 1) sb.Append(" \r\nCROSS JOIN ");
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
            // 保留关系表和时序测量的表别名。
            // 保留列限定符，避免同名列产生歧义。
            var sql = sb.Append(_tosqlAppendContent).ToString();
            if (cteDefinitions.Count > 0)
            {
                var definitions = string.Join(", ", cteDefinitions.Distinct(StringComparer.Ordinal));
                var match = Regex.Match(sql, @"^\s*WITH(?:\s+RECURSIVE)?\s+", RegexOptions.IgnoreCase);
                if (match.Success)
                    sql = "WITH " + (recursiveCte || match.Value.IndexOf("RECURSIVE", StringComparison.OrdinalIgnoreCase) >= 0 ? "RECURSIVE " : "") + definitions + ", " + sql.Substring(match.Length);
                else sql = "WITH " + (recursiveCte ? "RECURSIVE " : "") + definitions + "\r\n" + sql;
            }
            return sql;
        }

        static string HoistCteDefinitions(string source, List<string> definitions, ref bool recursive)
        {
            if (string.IsNullOrWhiteSpace(source) || source.IndexOf("WITH", StringComparison.OrdinalIgnoreCase) < 0) return source;
            while (true)
            {
                var tokens = SqlLexer.Tokenize(source);
                var withIndex = -1;
                for (var index = 0; index < tokens.Count; index++)
                    if (tokens[index].Kind == TokenKind.KeywordWith)
                    { withIndex = index; break; }
                if (withIndex < 0) return source;
                var depth = 0;
                var selectIndex = -1;
                for (var index = withIndex + 1; index < tokens.Count; index++)
                {
                    if (tokens[index].Kind == TokenKind.LeftParen) depth++;
                    else if (tokens[index].Kind == TokenKind.RightParen) depth--;
                    else if (depth == 0 && tokens[index].Kind == TokenKind.KeywordSelect) { selectIndex = index; break; }
                }
                if (selectIndex < 0) return source;
                var start = withIndex + 1;
                if (string.Equals(tokens[start].Text, "RECURSIVE", StringComparison.OrdinalIgnoreCase))
                { recursive = true; start++; }
                definitions.Add(source.Substring(tokens[start].Position, tokens[selectIndex].Position - tokens[start].Position).Trim());
                source = source.Remove(tokens[withIndex].Position, tokens[selectIndex].Position - tokens[withIndex].Position);
            }
        }

        static string EnsureMeasurementSortTime(string field, string orderBy, List<SelectTableInfo> tables)
        {
            if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(orderBy) ||
                field.IndexOf("to_utc_datetime", StringComparison.OrdinalIgnoreCase) < 0) return field;
            var from = tables.FirstOrDefault(table => table.Type == SelectTableInfoType.From);
            if (from?.Table == null || !SonnetDBModel.IsMeasurement(from.Table) ||
                tables.Any(table => table.Type == SelectTableInfoType.InnerJoin || table.Type == SelectTableInfoType.LeftJoin ||
                    table.Type == SelectTableInfoType.RightJoin || table.Type == SelectTableInfoType.RawJoin)) return field;
            if (!Regex.IsMatch(orderBy, @"\btime\b", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(field, @"\bAS\s+""time""(?:\s*,|\s*$)", RegexOptions.IgnoreCase)) return field;

            // The measurement sorter requires the integer timestamp in the
            // result under its native name. Typed readers consume their known
            // projection first, leaving this final value for server ordering.
            return field + ", " + from.Alias + ".time AS \"time\"";
        }

        public new List<TTuple> ToList<TTuple>(string field) => base.ToList<TTuple>(NormalizeScalarField<TTuple>(field));

        public new Task<List<TTuple>> ToListAsync<TTuple>(string field, CancellationToken cancellationToken = default) =>
            base.ToListAsync<TTuple>(NormalizeScalarField<TTuple>(field), cancellationToken);

        public new List<TReturn> ToList<TReturn>(Expression<Func<T1, TReturn>> select)
        {
            if (select == null) return base.ToList(select);
            if (typeof(TReturn) == typeof(TimeSpan))
            {
                var ticks = Expression.Lambda<Func<T1, long>>(
                    Expression.Property(select.Body, nameof(TimeSpan.Ticks)), select.Parameters);
                return base.ToList(ticks).ConvertAll(value => (TReturn)(object)TimeSpan.FromTicks(value));
            }
            if (typeof(TReturn) == typeof(TimeSpan?))
                return base.ToList(DurationTicksSelector(select)).ConvertAll(value =>
                    (TReturn)(object)(value.HasValue ? TimeSpan.FromTicks(value.Value) : (TimeSpan?)null));
            using (SonnetDBExpression.BeginDurationProjection(select))
                return base.ToList(select);
        }

        public new async Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, TReturn>> select,
            CancellationToken cancellationToken = default)
        {
            if (select == null) return await base.ToListAsync(select, cancellationToken);
            if (typeof(TReturn) == typeof(TimeSpan))
            {
                var ticks = Expression.Lambda<Func<T1, long>>(
                    Expression.Property(select.Body, nameof(TimeSpan.Ticks)), select.Parameters);
                return (await base.ToListAsync(ticks, cancellationToken))
                    .ConvertAll(value => (TReturn)(object)TimeSpan.FromTicks(value));
            }
            if (typeof(TReturn) == typeof(TimeSpan?))
                return (await base.ToListAsync(DurationTicksSelector(select), cancellationToken)).ConvertAll(value =>
                    (TReturn)(object)(value.HasValue ? TimeSpan.FromTicks(value.Value) : (TimeSpan?)null));
            using (SonnetDBExpression.BeginDurationProjection(select))
                return await base.ToListAsync(select, cancellationToken);
        }

        public new string ToSql<TReturn>(Expression<Func<T1, TReturn>> select,
            FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex)
        {
            using (SonnetDBExpression.BeginDurationProjection(select))
                return base.ToSql(select, fieldAlias);
        }

        internal static IDisposable BeginProjection(LambdaExpression select, List<SelectTableInfo> tables)
        {
            if (select != null)
                for (var index = 0; index < select.Parameters.Count; index++) tables[index].Parameter = select.Parameters[index];
            return SonnetDBExpression.BeginDurationProjection(select);
        }

        static Expression<Func<T1, long?>> DurationTicksSelector<TReturn>(Expression<Func<T1, TReturn>> select)
        {
            // Read scalar durations as their stored Int64 value. Formatting
            // the projected column as text can change ORDER BY alias semantics.
            var ticks = Expression.Property(Expression.Property(select.Body, "Value"), nameof(TimeSpan.Ticks));
            return Expression.Lambda<Func<T1, long?>>(Expression.Condition(
                Expression.Property(select.Body, "HasValue"), Expression.Convert(ticks, typeof(long?)),
                Expression.Constant(null, typeof(long?))), select.Parameters);
        }

        static string NormalizeScalarField<TTuple>(string field)
        {
            var type = typeof(TTuple).NullableTypeOrThis();
            if (type == typeof(TimeSpan)) return SonnetDBUtils.FormatTimeSpanReadSql(field);
            return type == typeof(DateTime) || type == typeof(DateTimeOffset) ? $"to_utc_datetime({field})" : field;
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

            // 原始 JOIN 的具体形态由数据库校验；也用于 AsTreeCte 的递归成员。
            if (hasRawJoin || sources.All(a => a.Table == null || !SonnetDBModel.IsMeasurement(a.Table))) return;
            if (sources.Length == 1) return;

            var from = sources.Where(a => a.Type == SelectTableInfoType.From).ToArray();
            if (from.Length != 1 || !SonnetDBModel.IsMeasurement(from[0].Table) ||
                explicitJoins.Length != 1 || explicitJoins[0].Type != SelectTableInfoType.InnerJoin ||
                !SonnetDBModel.IsTable(explicitJoins[0].Table))
                throw new NotSupportedException(
                    "SonnetDB 时序测量仅支持作为 FROM 数据源，与一个关系维表进行 INNER JOIN；其他 JOIN 组合请使用关系表。");
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
    class SonnetDBSelect<T1, T2> : FreeSql.Internal.CommonProvider.Select2Provider<T1, T2>, ISelect<T1, T2> where T2 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        List<TReturn> ISelect<T1, T2>.ToList<TReturn>(Expression<Func<T1, T2, TReturn>> select)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToList<TReturn>(select?.Body);
        }

        async Task<List<TReturn>> ISelect<T1, T2>.ToListAsync<TReturn>(
            Expression<Func<T1, T2, TReturn>> select, CancellationToken cancellationToken)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return await InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }

        string ISelect<T1, T2>.ToSql<TReturn>(Expression<Func<T1, T2, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3> : FreeSql.Internal.CommonProvider.Select3Provider<T1, T2, T3>, ISelect<T1, T2, T3> where T2 : class where T3 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        List<TReturn> ISelect<T1, T2, T3>.ToList<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToList<TReturn>(select?.Body);
        }

        async Task<List<TReturn>> ISelect<T1, T2, T3>.ToListAsync<TReturn>(
            Expression<Func<T1, T2, T3, TReturn>> select, CancellationToken cancellationToken)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return await InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }

        string ISelect<T1, T2, T3>.ToSql<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4> : FreeSql.Internal.CommonProvider.Select4Provider<T1, T2, T3, T4>, ISelect<T1, T2, T3, T4> where T2 : class where T3 : class where T4 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        List<TReturn> ISelect<T1, T2, T3, T4>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToList<TReturn>(select?.Body);
        }

        async Task<List<TReturn>> ISelect<T1, T2, T3, T4>.ToListAsync<TReturn>(
            Expression<Func<T1, T2, T3, T4, TReturn>> select, CancellationToken cancellationToken)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return await InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }

        string ISelect<T1, T2, T3, T4>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5> : FreeSql.Internal.CommonProvider.Select5Provider<T1, T2, T3, T4, T5>, ISelect<T1, T2, T3, T4, T5> where T2 : class where T3 : class where T4 : class where T5 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        List<TReturn> ISelect<T1, T2, T3, T4, T5>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToList<TReturn>(select?.Body);
        }

        async Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5>.ToListAsync<TReturn>(
            Expression<Func<T1, T2, T3, T4, T5, TReturn>> select, CancellationToken cancellationToken)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return await InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }

        string ISelect<T1, T2, T3, T4, T5>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6> : FreeSql.Internal.CommonProvider.Select6Provider<T1, T2, T3, T4, T5, T6>, ISelect<T1, T2, T3, T4, T5, T6> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToList<TReturn>(select?.Body);
        }

        async Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6>.ToListAsync<TReturn>(
            Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select, CancellationToken cancellationToken)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return await InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }

        string ISelect<T1, T2, T3, T4, T5, T6>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7> : FreeSql.Internal.CommonProvider.Select7Provider<T1, T2, T3, T4, T5, T6, T7>, ISelect<T1, T2, T3, T4, T5, T6, T7> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToList<TReturn>(select?.Body);
        }

        async Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7>.ToListAsync<TReturn>(
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select, CancellationToken cancellationToken)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return await InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8> : FreeSql.Internal.CommonProvider.Select8Provider<T1, T2, T3, T4, T5, T6, T7, T8>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToList<TReturn>(select?.Body);
        }

        async Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToListAsync<TReturn>(
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select, CancellationToken cancellationToken)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return await InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> : FreeSql.Internal.CommonProvider.Select9Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToList<TReturn>(select?.Body);
        }

        async Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToListAsync<TReturn>(
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select, CancellationToken cancellationToken)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return await InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : FreeSql.Internal.CommonProvider.Select10Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToList<TReturn>(select?.Body);
        }

        async Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToListAsync<TReturn>(
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select, CancellationToken cancellationToken)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return await InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }

    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : FreeSql.Internal.CommonProvider.Select11Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToList<TReturn>(select?.Body);
        }

        async Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToListAsync<TReturn>(
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select, CancellationToken cancellationToken)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return await InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : FreeSql.Internal.CommonProvider.Select12Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToList<TReturn>(select?.Body);
        }

        async Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToListAsync<TReturn>(
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select, CancellationToken cancellationToken)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return await InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : FreeSql.Internal.CommonProvider.Select13Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToList<TReturn>(select?.Body);
        }

        async Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToListAsync<TReturn>(
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select, CancellationToken cancellationToken)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return await InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : FreeSql.Internal.CommonProvider.Select14Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToList<TReturn>(select?.Body);
        }

        async Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToListAsync<TReturn>(
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select, CancellationToken cancellationToken)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return await InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : FreeSql.Internal.CommonProvider.Select15Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToList<TReturn>(select?.Body);
        }

        async Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToListAsync<TReturn>(
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select, CancellationToken cancellationToken)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return await InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
    class SonnetDBSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : FreeSql.Internal.CommonProvider.Select16Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class where T16 : class
    {
        public SonnetDBSelect(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) { }
        List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToList<TReturn>(select?.Body);
        }

        async Task<List<TReturn>> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToListAsync<TReturn>(
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select, CancellationToken cancellationToken)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return await InternalToListAsync<TReturn>(select?.Body, cancellationToken);
        }

        string ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.ToSql<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>> select, FieldAliasOptions fieldAlias)
        {
            using (SonnetDBSelect<T1>.BeginProjection(select, _tables))
                return InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public override string ToSql(string field = null) => SonnetDBSelect<T1>.ToSqlStatic(_commonUtils, _commonExpression, _select, _distinct, field ?? this.GetAllFieldExpressionTreeLevel2().Field, _join, _where, _groupby, _having, _orderby, _skip, _limit, _tables, this.GetTableRuleUnions(), _aliasRule, _tosqlAppendContent, _whereGlobalFilter, _orm);
    }
}
