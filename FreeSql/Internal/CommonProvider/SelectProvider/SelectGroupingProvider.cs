using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{
    public class SelectGroupingProvider<TKey, TValue> : ISelectGrouping<TKey, TValue>
    {
        internal IFreeSql _orm;
        internal object _select;
        internal ReadAnonymousTypeInfo _map;
        internal CommonExpression _comonExp;
        internal List<SelectTableInfo> _tables;
        public SelectGroupingProvider(IFreeSql orm, object select, ReadAnonymousTypeInfo map, CommonExpression comonExp, List<SelectTableInfo> tables)
        {
            _orm = orm;
            _select = select;
            _map = map;
            _comonExp = comonExp;
            _tables = tables;
        }

        string getSelectGroupingMapString(Expression[] members)
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

        public ISelectGrouping<TKey, TValue> Having(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, bool>> exp)
        {
            var sql = _comonExp.ExpressionWhereLambda(null, exp, getSelectGroupingMapString, null, null);
            var method = _select.GetType().GetMethod("Having", new[] { typeof(string), typeof(object) });
            method.Invoke(_select, new object[] { sql, null });
            return this;
        }

        public ISelectGrouping<TKey, TValue> OrderBy<TMember>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TMember>> column)
        {
            var sql = _comonExp.ExpressionWhereLambda(null, column, getSelectGroupingMapString, null, null);
            var method = _select.GetType().GetMethod("OrderBy", new[] { typeof(string), typeof(object) });
            method.Invoke(_select, new object[] { sql, null });
            return this;
        }

        public ISelectGrouping<TKey, TValue> OrderByDescending<TMember>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TMember>> column)
        {
            var sql = _comonExp.ExpressionWhereLambda(null, column, getSelectGroupingMapString, null, null);
            var method = _select.GetType().GetMethod("OrderBy", new[] { typeof(string), typeof(object) });
            method.Invoke(_select, new object[] { $"{sql} DESC", null });
            return this;
        }

        public List<TReturn> ToList<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = 0;

            _comonExp.ReadAnonymousField(null, field, map, ref index, select, getSelectGroupingMapString, null, true);
            if (map.Childs.Any() == false && map.MapType == null) map.MapType = typeof(TReturn);
            var method = _select.GetType().GetMethod("ToListMapReader", BindingFlags.Instance | BindingFlags.NonPublic);
            method = method.MakeGenericMethod(typeof(TReturn));
            return method.Invoke(_select, new object[] { (map, field.Length > 0 ? field.Remove(0, 2).ToString() : null) }) as List<TReturn>;
        }
        
        public List<TReturn> Select<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select) => ToList(select);

        public string ToSql<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = fieldAlias == FieldAliasOptions.AsProperty ? CommonExpression.ReadAnonymousFieldAsCsName : 0;

            _comonExp.ReadAnonymousField(null, field, map, ref index, select, getSelectGroupingMapString, null, true);
            var method = _select.GetType().GetMethod("ToSql", new[] { typeof(string) });
            return method.Invoke(_select, new object[] { field.Length > 0 ? field.Remove(0, 2).ToString() : null }) as string;
        }
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

#if net40
#else
        async public Task<long> CountAsync() => long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync($"select count(1) from ({this.ToSql($"1{_comonExp._common.FieldAsAlias("as1")}")}) fta")), out var trylng) ? trylng : default(long);

        public Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = 0;

            _comonExp.ReadAnonymousField(null, field, map, ref index, select, getSelectGroupingMapString, null, true);
            if (map.Childs.Any() == false && map.MapType == null) map.MapType = typeof(TReturn);
            var method = _select.GetType().GetMethod("ToListMapReaderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            method = method.MakeGenericMethod(typeof(TReturn));
            return method.Invoke(_select, new object[] { (map, field.Length > 0 ? field.Remove(0, 2).ToString() : null) }) as Task<List<TReturn>>;
        }
#endif
    }
}
