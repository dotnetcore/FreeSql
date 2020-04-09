using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{
    public class SelectGroupingProvider
    {
        public IFreeSql _orm;
        public object _select;
        public ReadAnonymousTypeInfo _map;
        public string _field;
        public CommonExpression _comonExp;
        public List<SelectTableInfo> _tables;

        public SelectGroupingProvider(IFreeSql orm, object select, ReadAnonymousTypeInfo map, string field, CommonExpression comonExp, List<SelectTableInfo> tables)
        {
            _orm = orm;
            _select = select;
            _map = map;
            _field = field;
            _comonExp = comonExp;
            _tables = tables;
        }

        public string getSelectGroupingMapString(Expression[] members)
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
            var sql = _comonExp.ExpressionWhereLambda(null, exp, getSelectGroupingMapString, null, null);
            var method = _select.GetType().GetMethod("Having", new[] { typeof(string), typeof(object) });
            method.Invoke(_select, new object[] { sql, null });
        }
        public void InternalOrderBy(Expression exp, bool isDescending)
        {
            var sql = _comonExp.ExpressionWhereLambda(null, exp, getSelectGroupingMapString, null, null);
            var method = _select.GetType().GetMethod("OrderBy", new[] { typeof(string), typeof(object) });
            method.Invoke(_select, new object[] { isDescending ? $"{sql} DESC" : sql, null });
        }
        public object InternalToList(Expression select, Type elementType, bool isAsync)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = 0;

            _comonExp.ReadAnonymousField(null, field, map, ref index, select, getSelectGroupingMapString, null, false);
            if (map.Childs.Any() == false && map.MapType == null) map.MapType = elementType;
            var method = _select.GetType().GetMethod(isAsync ? "ToListMapReaderAsync" : "ToListMapReader", BindingFlags.Instance | BindingFlags.NonPublic);
            method = method.MakeGenericMethod(elementType);
            return method.Invoke(_select, new object[] { new ReadAnonymousTypeAfInfo(map, field.Length > 0 ? field.Remove(0, 2).ToString() : null) });
        }
        public IEnumerable<KeyValuePair<object, object>> InternalToKeyValuePairs(Expression elementSelector, Type elementType)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = 0;

            _comonExp.ReadAnonymousField(null, field, map, ref index, elementSelector, getSelectGroupingMapString, null, false);
            if (map.Childs.Any() == false && map.MapType == null) map.MapType = elementType;
            var method = _select.GetType().GetMethod("ToListMapReaderPrivate", BindingFlags.Instance | BindingFlags.NonPublic);
            method = method.MakeGenericMethod(elementType);
            var otherAf = new ReadAnonymousTypeOtherInfo(_field, _map, new List<object>());
            var values = method.Invoke(_select, new object[] { new ReadAnonymousTypeAfInfo(map, field.Length > 0 ? field.Remove(0, 2).ToString() : null), new[] { otherAf } }) as IList;
            return otherAf.retlist.Select((a, b) => new KeyValuePair<object, object>(a, values[b]));
        }
        public string InternalToSql(Expression select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = fieldAlias == FieldAliasOptions.AsProperty ? CommonExpression.ReadAnonymousFieldAsCsName : 0;

            _comonExp.ReadAnonymousField(null, field, map, ref index, select, getSelectGroupingMapString, null, false);
            var method = _select.GetType().GetMethod("ToSql", new[] { typeof(string) });
            return method.Invoke(_select, new object[] { field.Length > 0 ? field.Remove(0, 2).ToString() : null }) as string;
        }
    }

    public class SelectGroupingProvider<TKey, TValue> : SelectGroupingProvider, ISelectGrouping<TKey, TValue>
    {
        public SelectGroupingProvider(IFreeSql orm, object select, ReadAnonymousTypeInfo map, string field, CommonExpression comonExp, List<SelectTableInfo> tables)
            :base(orm, select, map, field, comonExp, tables) { }

        public string ToSql<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex) => InternalToSql(select, fieldAlias);
        public string ToSql(string field)
        {
            if (string.IsNullOrEmpty(field))
                throw new ArgumentException("参数 field 未指定");

            var method = _select.GetType().GetMethod("ToSql", new[] { typeof(string) });
            return method.Invoke(_select, new object[] { field }) as string;
        }

        public ISelectGrouping<TKey, TValue> Skip(int offset)
        {
            var method = _select.GetType().GetMethod("Skip", new[] { typeof(int) });
            method.Invoke(_select, new object[] { offset });
            return this;
        }
        public ISelectGrouping<TKey, TValue> Offset(int offset) => this.Skip(offset);
        public ISelectGrouping<TKey, TValue> Limit(int limit)
        {
            var method = _select.GetType().GetMethod("Limit", new[] { typeof(int) });
            method.Invoke(_select, new object[] { limit });
            return this;
        }
        public ISelectGrouping<TKey, TValue> Take(int limit) => this.Limit(limit);
        public ISelectGrouping<TKey, TValue> Page(int pageNumber, int pageSize)
        {
            var method = _select.GetType().GetMethod("Page", new[] { typeof(int), typeof(int) });
            method.Invoke(_select, new object[] { pageNumber, pageSize });
            return this;
        }

        public long Count() => long.TryParse(string.Concat(_orm.Ado.ExecuteScalar($"select count(1) from ({this.ToSql($"1{_comonExp._common.FieldAsAlias("as1")}")}) fta")), out var trylng) ? trylng : default(long);
        public ISelectGrouping<TKey, TValue> Count(out long count)
        {
            count = this.Count();
            return this;
        }

        public ISelectGrouping<TKey, TValue> Having(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, bool>> exp)
        {
            InternalHaving(exp);
            return this;
        }
        public ISelectGrouping<TKey, TValue> OrderBy<TMember>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TMember>> column)
        {
            InternalOrderBy(column, false);
            return this;
        }
        public ISelectGrouping<TKey, TValue> OrderByDescending<TMember>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TMember>> column)
        {
            InternalOrderBy(column, true);
            return this;
        }

        public List<TReturn> Select<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select) => ToList(select);
        public List<TReturn> ToList<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select) => InternalToList(select, typeof(TReturn), false) as List<TReturn>;
        public Dictionary<TKey, TElement> ToDictionary<TElement>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TElement>> elementSelector) => InternalToKeyValuePairs(elementSelector, typeof(TElement)).ToDictionary(a => (TKey)a.Key, a => (TElement)a.Value);

#if net40
#else
        async public Task<long> CountAsync() => long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync($"select count(1) from ({this.ToSql($"1{_comonExp._common.FieldAsAlias("as1")}")}) fta")), out var trylng) ? trylng : default(long);

        public Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select) => InternalToList(select, typeof(TReturn), true) as Task<List<TReturn>>;
        async public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TElement>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TElement>> elementSelector)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = 0;

            _comonExp.ReadAnonymousField(null, field, map, ref index, elementSelector, getSelectGroupingMapString, null, false);
            if (map.Childs.Any() == false && map.MapType == null) map.MapType = typeof(TElement);
            var method = _select.GetType().GetMethod("ToListMapReaderPrivateAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            method = method.MakeGenericMethod(typeof(TElement));
            var otherAf = new ReadAnonymousTypeOtherInfo(_field, _map, new List<object>());
            var values = await (method.Invoke(_select, new object[] { new ReadAnonymousTypeAfInfo(map, field.Length > 0 ? field.Remove(0, 2).ToString() : null), new[] { otherAf } }) as Task<List<TElement>>);
            return otherAf.retlist.Select((a, b) => new KeyValuePair<TKey, TElement>((TKey)a, values[b])).ToDictionary(a => a.Key, a => a.Value);
        }
#endif
    }
}
