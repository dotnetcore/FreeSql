using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{
    public class SelectGroupingProvider : BaseDiyMemberExpression
    {
        public IFreeSql _orm;
        public Select0Provider _select;
        public CommonExpression _comonExp;
        public List<SelectTableInfo> _tables;
        public int _groupByLimit, _groupBySkip;

        public SelectGroupingProvider(IFreeSql orm, Select0Provider select, ReadAnonymousTypeInfo map, string field, CommonExpression comonExp, List<SelectTableInfo> tables)
        {
            _orm = orm;
            _select = select;
            _map = map;
            _field = field;
            _comonExp = comonExp;
            _tables = tables;
        }

        public override string ParseExp(Expression[] members)
        {
            if (members.Any() == false) return _map.DbField;
            var parentName = ((members.FirstOrDefault() as MemberExpression)?.Expression as MemberExpression)?.Member.Name;
            switch (parentName)
            {
                case "Key":
                    var read = _map;
                    for (var a = 0; a < members.Length; a++)
                    {
                        read = read.Childs.Where(z => z.CsName == (members[a] as MemberExpression)?.Member.Name).FirstOrDefault();
                        if (read == null) return null;
                    }
                    return read.DbField;
                case "Value":
                    var tb = _tables.First();
                    var foridx = 0;
                    if (members.Length > 1)
                    {
                        var mem0 = (members.FirstOrDefault() as MemberExpression);
                        var mem0Name = mem0?.Member.Name;
                        if (mem0Name?.StartsWith("Item") == true && int.TryParse(mem0Name.Substring(4), out var tryitemidx))
                        {
                            if (tryitemidx == 1) foridx++;
                            else
                            {
                                //var alias = $"SP10{(char)(96 + tryitemidx)}";
                                var tmptb = _tables.Where((a, idx) => //a.AliasInit == alias && 
                                    a.Table.Type == mem0.Type && idx == tryitemidx - 1).FirstOrDefault();
                                if (tmptb != null)
                                {
                                    tb = tmptb;
                                    foridx++;
                                }
                            }
                        }
                    }
                    var parmExp = Expression.Parameter(tb.Table.Type, tb.Alias);
                    Expression retExp = parmExp;
                    for (var a = foridx; a < members.Length; a++)
                    {
                        switch (members[a].NodeType)
                        {
                            case ExpressionType.Call:
                                retExp = Expression.Call(retExp, (members[a] as MethodCallExpression).Method);
                                break;
                            case ExpressionType.MemberAccess:
                                retExp = Expression.MakeMemberAccess(retExp, (members[a] as MemberExpression).Member);
                                break;
                            default:
                                return null;
                        }
                    }
                    return _comonExp.ExpressionLambdaToSql(retExp, new CommonExpression.ExpTSC { _tables = _tables, tbtype = SelectTableInfoType.From, isQuoteName = true, isDisableDiyParse = true, style = CommonExpression.ExpressionStyle.Where });
            }
            return null;
        }

        public void InternalHaving(Expression exp)
        {
            var sql = _comonExp.ExpressionWhereLambda(null, exp, this, null, null);
            var method = _select.GetType().GetMethod("Having", new[] { typeof(string), typeof(object) });
            method.Invoke(_select, new object[] { sql, null });
        }
        public void InternalOrderBy(Expression exp, bool isDescending)
        {
            var sql = _comonExp.ExpressionWhereLambda(null, exp, this, null, null);
            var method = _select.GetType().GetMethod("OrderBy", new[] { typeof(string), typeof(object) });
            method.Invoke(_select, new object[] { isDescending ? $"{sql} DESC" : sql, null });
        }
        public object InternalToList(Expression select, Type elementType)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = 0;

            _comonExp.ReadAnonymousField(null, field, map, ref index, select, null, this, null, null, false);
            if (map.Childs.Any() == false && map.MapType == null) map.MapType = elementType;
            var method = _select.GetType().GetMethod("ToListMrPrivate", BindingFlags.Instance | BindingFlags.NonPublic);
            method = method.MakeGenericMethod(elementType);
            var fieldSql = field.Length > 0 ? field.Remove(0, 2).ToString() : null;
            return method.Invoke(_select, new object[] { InternalToSql(fieldSql), new ReadAnonymousTypeAfInfo(map, fieldSql), null });
        }
        public IEnumerable<KeyValuePair<object, object>> InternalToKeyValuePairs(Expression elementSelector, Type elementType)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = 0;

            _comonExp.ReadAnonymousField(null, field, map, ref index, elementSelector, null, this, null, null, false);
            if (map.Childs.Any() == false && map.MapType == null) map.MapType = elementType;
            var method = _select.GetType().GetMethod("ToListMrPrivate", BindingFlags.Instance | BindingFlags.NonPublic);
            method = method.MakeGenericMethod(elementType);
            var fieldSql = field.Length > 0 ? field.Remove(0, 2).ToString() : null;
            var otherAf = new ReadAnonymousTypeOtherInfo(_field, _map, new List<object>());
            var values = method.Invoke(_select, new object[] { InternalToSql($"{fieldSql}{_field}"), new ReadAnonymousTypeAfInfo(map, fieldSql), new[] { otherAf } }) as IList;
            return otherAf.retlist.Select((a, b) => new KeyValuePair<object, object>(a, values[b]));
        }
        public string InternalToSql(Expression select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = fieldAlias == FieldAliasOptions.AsProperty ? CommonExpression.ReadAnonymousFieldAsCsName : 0;

            _comonExp.ReadAnonymousField(null, field, map, ref index, select, null, this, null, null, false);
            var fieldSql = field.Length > 0 ? field.Remove(0, 2).ToString() : null;
            return InternalToSql(fieldSql);
        }
        public string InternalToSql(string field)
        {
            if (string.IsNullOrEmpty(field))
                throw new ArgumentException("参数 field 未指定");

            var isNestedPageSql = false;
            switch (_orm.Ado.DataType)
            {
                case DataType.Oracle:
                case DataType.OdbcOracle:
                case DataType.Dameng:
                case DataType.OdbcDameng: //Oracle、Dameng 分组时，嵌套分页
                    isNestedPageSql = true;
                    break;
                default:
                    _select._limit = _groupByLimit;
                    _select._skip = _groupBySkip;
                    break;
            }
            var method = _select.GetType().GetMethod("ToSql", new[] { typeof(string) });
            var sql = method.Invoke(_select, new object[] { field }) as string;
            if (isNestedPageSql == false)
            {
                _select._limit = 0;
                _select._skip = 0;
                return sql;
            }
            if (_groupByLimit == 0 && _groupBySkip == 0) return sql;
            return _orm.Select<object>().As("t").WithSql(sql).Limit(_groupByLimit).Skip(_groupBySkip).ToSql("t.*");
        }
    }

    public class SelectGroupingProvider<TKey, TValue> : SelectGroupingProvider, ISelectGrouping<TKey, TValue>
    {
        public SelectGroupingProvider(IFreeSql orm, Select0Provider select, ReadAnonymousTypeInfo map, string field, CommonExpression comonExp, List<SelectTableInfo> tables)
            :base(orm, select, map, field, comonExp, tables) { }

        public string ToSql<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex)
        {
            _lambdaParameter = select?.Parameters[0];
            return InternalToSql(select, fieldAlias);
        }
        public string ToSql(string field) => InternalToSql(field);

        public ISelectGrouping<TKey, TValue> Skip(int offset)
        {
            _groupBySkip = offset;
            return this;
        }
        public ISelectGrouping<TKey, TValue> Offset(int offset) => this.Skip(offset);
        public ISelectGrouping<TKey, TValue> Limit(int limit)
        {
            _groupByLimit = limit;
            return this;
        }
        public ISelectGrouping<TKey, TValue> Take(int limit) => this.Limit(limit);
        public ISelectGrouping<TKey, TValue> Page(int pageNumber, int pageSize)
        {
            _groupBySkip = Math.Max(0, pageNumber - 1) * pageSize;
            _groupByLimit = pageSize;
            return this;
        }

        public ISelectGrouping<TKey, TValue> Page(BasePagingInfo pagingInfo)
        {
            pagingInfo.Count = this.Count();
            _groupBySkip = Math.Max(0, pagingInfo.PageNumber - 1) * pagingInfo.PageSize;
            _groupByLimit = pagingInfo.PageSize;
            return this;
        }

        public long Count() => _select._cancel?.Invoke() == true ? 0 : long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(_select._connection, _select._transaction, CommandType.Text, $"select count(1) from ({this.ToSql($"1{_comonExp._common.FieldAsAlias("as1")}")}) fta", _select._commandTimeout, _select._params.ToArray())), out var trylng) ? trylng : default(long);
        public ISelectGrouping<TKey, TValue> Count(out long count)
        {
            count = this.Count();
            return this;
        }

        public ISelectGrouping<TKey, TValue> Having(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, bool>> exp)
        {
            _lambdaParameter = exp?.Parameters[0];
            InternalHaving(exp);
            return this;
        }
        public ISelectGrouping<TKey, TValue> OrderBy<TMember>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TMember>> column)
        {
            _lambdaParameter = column?.Parameters[0];
            InternalOrderBy(column, false);
            return this;
        }
        public ISelectGrouping<TKey, TValue> OrderByDescending<TMember>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TMember>> column)
        {
            _lambdaParameter = column?.Parameters[0];
            InternalOrderBy(column, true);
            return this;
        }

        public List<TReturn> Select<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select) => ToList(select);
        public List<TReturn> ToList<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select)
        {
            _lambdaParameter = select?.Parameters[0];
            return InternalToList(select, typeof(TReturn)) as List<TReturn>;
        }
        public Dictionary<TKey, TElement> ToDictionary<TElement>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TElement>> elementSelector)
        {
            _lambdaParameter = elementSelector?.Parameters[0];
            return InternalToKeyValuePairs(elementSelector, typeof(TElement)).ToDictionary(a => (TKey)a.Key, a => (TElement)a.Value);
        }

#if net40
#else
        async public Task<long> CountAsync(CancellationToken cancellationToken = default) => _select._cancel?.Invoke() == true ? 0 : long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(_select._connection, _select._transaction, CommandType.Text, $"select count(1) from ({this.ToSql($"1{_comonExp._common.FieldAsAlias("as1")}")}) fta", _select._commandTimeout, _select._params.ToArray(), cancellationToken)), out var trylng) ? trylng : default(long);

        public Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select, CancellationToken cancellationToken = default)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = 0;

            _lambdaParameter = select?.Parameters[0];
            _comonExp.ReadAnonymousField(null, field, map, ref index, select, null, this, null, null, false);
            if (map.Childs.Any() == false && map.MapType == null) map.MapType = typeof(TReturn);
            var method = _select.GetType().GetMethod("ToListMrPrivateAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            method = method.MakeGenericMethod(typeof(TReturn));
            var fieldSql = field.Length > 0 ? field.Remove(0, 2).ToString() : null;
            return method.Invoke(_select, new object[] { InternalToSql(fieldSql), new ReadAnonymousTypeAfInfo(map, fieldSql), null, cancellationToken }) as Task<List<TReturn>>;
        }
        async public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TElement>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TElement>> elementSelector, CancellationToken cancellationToken = default)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = 0;

            _lambdaParameter = elementSelector?.Parameters[0];
            _comonExp.ReadAnonymousField(null, field, map, ref index, elementSelector, null, this, null, null, false);
            if (map.Childs.Any() == false && map.MapType == null) map.MapType = typeof(TElement);
            var method = _select.GetType().GetMethod("ToListMrPrivateAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            method = method.MakeGenericMethod(typeof(TElement));
            var fieldSql = field.Length > 0 ? field.Remove(0, 2).ToString() : null;
            var otherAf = new ReadAnonymousTypeOtherInfo(_field, _map, new List<object>());
            var values = await (method.Invoke(_select, new object[] { InternalToSql($"{fieldSql}{_field}"), new ReadAnonymousTypeAfInfo(map, fieldSql), new[] { otherAf }, cancellationToken }) as Task<List<TElement>>);
            return otherAf.retlist.Select((a, b) => new KeyValuePair<TKey, TElement>((TKey)a, values[b])).ToDictionary(a => a.Key, a => a.Value);
        }
#endif
    }
}
