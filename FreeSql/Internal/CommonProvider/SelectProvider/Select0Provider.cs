using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{
    public abstract partial class Select0Provider
    {
        public int _limit, _skip;
        public string _select = "SELECT ", _orderby, _groupby, _having;
        public StringBuilder _where = new StringBuilder();
        public List<DbParameter> _params = new List<DbParameter>();
        public List<DbParameter> _paramsInit;
        public List<SelectTableInfo> _tables = new List<SelectTableInfo>();
        public List<Func<Type, string, string>> _tableRules = new List<Func<Type, string, string>>();
        public Func<Type, string, string> _tableRule => _tableRules?.FirstOrDefault();
        public Func<Type, string, string> _aliasRule;
        public string _tosqlAppendContent;
        public StringBuilder _join = new StringBuilder();
        public IFreeSql _orm;
        public CommonUtils _commonUtils;
        public CommonExpression _commonExpression;
        public DbTransaction _transaction;
        public DbConnection _connection;
        public int _commandTimeout = 0;
        public Action<object> _trackToList;
        public List<Action<object>> _includeToList = new List<Action<object>>();
#if net40
#else
        public List<Func<object, CancellationToken, Task>> _includeToListAsync = new List<Func<object, CancellationToken, Task>>();
#endif
        public Dictionary<string, MemberExpression[]> _includeInfo = new Dictionary<string, MemberExpression[]>();
        public bool _distinct;
        public Expression _selectExpression;
        public List<GlobalFilter.Item> _whereGlobalFilter;
        public Func<bool> _cancel;
        public bool _is_AsTreeCte;
        public BaseDiyMemberExpression _diymemexpWithTempQuery;
        public Func<DbTransaction> _resolveHookTransaction;

        public bool IsDefaultSqlContent => _distinct == false && _is_AsTreeCte == false && _tables.Count == 1 && _where.Length == 0 && _join.Length == 0 &&
            string.IsNullOrWhiteSpace(_orderby) && string.IsNullOrWhiteSpace(_groupby) && string.IsNullOrWhiteSpace(_tosqlAppendContent) &&
            _aliasRule == null && _selectExpression == null;

        public Select0Provider()
        {
            _paramsInit = _params;
        }

        int _disposeCounter;
        ~Select0Provider()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            _where.Clear();
            if (_paramsInit == _params) _params.Clear(); //子查询与主查询共享，并发导致错误清除了主查询参数化信息 https://github.com/dotnetcore/FreeSql/issues/1155
            _tables.Clear();
            _tableRules.Clear();
            _join.Clear();
            _trackToList = null;
            _includeToList.Clear();
#if net40
#else
            _includeToListAsync.Clear();
#endif
            _includeInfo.Clear();
            _selectExpression = null;
            _whereGlobalFilter?.Clear();
            _cancel = null;
            _diymemexpWithTempQuery = null;
        }

        public abstract string ToSqlBase(string field = null);
        public abstract void AsTableBase(Func<Type, string, string> tableRule);

        public static void CopyData(Select0Provider from, Select0Provider to, ReadOnlyCollection<ParameterExpression> lambParms)
        {
            if (to == null) return;
            to._limit = from._limit;
            to._skip = from._skip;
            to._select = from._select;
            to._orderby = from._orderby;
            to._groupby = from._groupby;
            to._having = from._having;
            to._where = new StringBuilder().Append(from._where.ToString());
            to._params = new List<DbParameter>(from._params.ToArray());

            if (lambParms == null)
            {
                var fromParentTables = from._tables.Where(a => a.Type == SelectTableInfoType.Parent).ToList();
                var fromTables = fromParentTables.Any() ? from._tables.Where(a => a.Type != SelectTableInfoType.Parent).ToList() : from._tables;
                if (to._tables.Count <= fromTables.Count)
                    to._tables = new List<SelectTableInfo>(fromTables);
                else
                {
                    to._tables = new List<SelectTableInfo>(to._tables);
                    for (var a = 0; a < fromTables.Count; a++)
                        to._tables[a] = fromTables[a];
                }
                if (fromParentTables.Any())
                    to._tables.AddRange(fromParentTables);
            }
            else
            {
                var findedIndexs = new List<int>();
                var _multiTables = to._tables;
                _multiTables[0] = from._tables[0];
                for (var a = 1; a < lambParms.Count; a++)
                {
                    var tbIndex = from._tables.FindIndex(b => b.Alias == lambParms[a].Name && b.Table.Type == lambParms[a].Type); ;
                    if (tbIndex != -1)
                    {
                        findedIndexs.Add(tbIndex);
                        _multiTables[a] = from._tables[tbIndex];
                    }
                    else
                    {
                        _multiTables[a].Alias = lambParms[a].Name;
                        _multiTables[a].Parameter = lambParms[a];
                    }
                }
                for (var a = 1; a < from._tables.Count; a++)
                {
                    if (findedIndexs.Contains(a)) continue;
                    _multiTables.Add(from._tables[a]);
                }
            }
            to._tableRules = new List<Func<Type, string, string>>(from._tableRules.ToArray());
            to._aliasRule = from._aliasRule;
            to._join = new StringBuilder().Append(from._join.ToString());
            //to._orm = from._orm;
            //to._commonUtils = from._commonUtils;
            //to._commonExpression = from._commonExpression;
            to._transaction = from._transaction;
            to._connection = from._connection;
            to._trackToList = from._trackToList;
            to._includeToList = new List<Action<object>>(from._includeToList.ToArray());
#if net40
#else
            to._includeToListAsync = new List<Func<object, CancellationToken, Task>>(from._includeToListAsync.ToArray());
#endif
            to._distinct = from._distinct;
            to._selectExpression = from._selectExpression;
            to._whereGlobalFilter = new List<GlobalFilter.Item>(from._whereGlobalFilter.ToArray());
            to._cancel = from._cancel;
            to._is_AsTreeCte = from._is_AsTreeCte;
            to._diymemexpWithTempQuery = from._diymemexpWithTempQuery;
        }

        internal class WithTempQueryParser : BaseDiyMemberExpression
        {
            public List<InsideInfo> _insideSelectList = new List<InsideInfo>();
            public List<SelectTableInfo> _outsideTable = new List<SelectTableInfo>();
            public WithTempQueryParser(Select0Provider insideSelect, SelectGroupingProvider insideSelectGroup, Expression selector, SelectTableInfo outsideTable)
            {
                if (selector != null)
                {
                    _insideSelectList.Add(new InsideInfo(insideSelect, insideSelectGroup, selector));
                    _outsideTable.Add(outsideTable);
                }
            }
            public class InsideInfo
            {
                public Select0Provider InsideSelect { get; }
                public SelectGroupingProvider InsideSelectGroup { get; }
                public CommonExpression InsideComonExp;
                public string InsideField { get; }
                public ReadAnonymousTypeAfInfo InsideAf { get; }
                public ReadAnonymousTypeInfo InsideMap { get; }

                public InsideInfo(Select0Provider insideSelect, SelectGroupingProvider insideSelectGroup, Expression selector)
                {
                    InsideSelect = insideSelect;
                    InsideSelectGroup = insideSelectGroup;

                    InsideMap = new ReadAnonymousTypeInfo();
                    var field = new StringBuilder();
                    var index = CommonExpression.ReadAnonymousFieldAsCsName; //AsProperty

                    if (selector != null)
                    {
                        if (insideSelectGroup != null)
                            InsideSelect._commonExpression.ReadAnonymousField(null, insideSelect._tableRule, field, InsideMap, ref index, selector, insideSelect, InsideSelectGroup, null, null, null, false);
                        else if (insideSelect != null)
                            InsideSelect._commonExpression.ReadAnonymousField(insideSelect._tables, InsideSelect._tableRule, field, InsideMap, ref index, selector, null, insideSelect._diymemexpWithTempQuery, insideSelect._whereGlobalFilter, null, null, false); //不走 DTO 映射，不处理 IncludeMany
                    }
                    InsideField = field.Length > 0 ? field.Remove(0, 2).ToString() : null;
                    InsideAf = new ReadAnonymousTypeAfInfo(InsideMap, "*");
                    field.Clear();
                    if (InsideMap.Childs.Where(a => a.Childs.Count > 1).Count() > 1)
                    {
                        var childs = InsideMap.GetAllChilds();
                        var duplicateNames = childs.GroupBy(a => a.DbNestedField).Where(a => a.Count() > 1).ToList();
                        if (duplicateNames.Count > 0)
                        {
                            foreach (var duplicateName in duplicateNames)
                            {
                                var dupmapIdx = 0;
                                foreach (var dupmap in duplicateName)
                                {
                                    if (++dupmapIdx == 1) continue;
                                    var newfield = insideSelect._commonUtils.TrimQuoteSqlName(dupmap.DbNestedField);
                                    while (InsideField.Contains($"{newfield}{dupmapIdx}"))
                                        ++dupmapIdx;
                                    dupmap.DbNestedField = insideSelect._commonUtils.QuoteSqlName($"{newfield}{dupmapIdx}");
                                }
                            }
                            foreach (var child in childs)
                                field.Append(", ").Append(child.DbField).Append(InsideSelect._commonExpression.EndsWithDbNestedField(child.DbField, child.DbNestedField) ? "" : InsideSelect._commonUtils.FieldAsAlias(child.DbNestedField));
                            InsideField = field.Length > 0 ? field.Remove(0, 2).ToString() : null;
                            field.Clear();
                        }
                    }
                }
            }

            public WithTempQueryParser Append(Select0Provider select, SelectTableInfo outsideTable)
            {
                //select is ISelect<T>
                if (outsideTable != null && select?._diymemexpWithTempQuery is WithTempQueryParser withTempQuery)
                {
                    _insideSelectList.Add(withTempQuery._insideSelectList[0]);
                    _outsideTable.Add(outsideTable);
                }
                return this;
            }

            public SelectTableInfo ParseExpMatchedTable { get; private set; }
            public override string ParseExp(Expression[] members)
            {
                ParseExpMapResult = null;
                ParseExpMatchedTable = GetOutsideSelectTable(members.FirstOrDefault()?.GetParameter());
                if (ParseExpMatchedTable == null) return null;

                var insideIndex = _outsideTable.FindIndex(a => a == ParseExpMatchedTable);
                if (insideIndex == -1)
                {
                    ParseExpMatchedTable = null;
                    return null;
                }
                var insideData = _insideSelectList[insideIndex];
                if (members.Any() == false)
                {
                    ParseExpMapResult = insideData.InsideMap;
                    return $"{ParseExpMatchedTable.Alias}.{insideData.InsideMap.DbNestedField}";
                }
                var read = insideData.InsideMap;
                if (members.Length == 1 && members[0] == ParseExpMatchedTable.Parameter)
                {
                    ParseExpMapResult = read;
                    return $"{ParseExpMatchedTable.Alias}.{read.DbNestedField}";
                }
                for (var a = members[0] == ParseExpMatchedTable.Parameter ? 1 : 0; a < members.Length; a++)
                {
                    read = read.Childs.Where(z => z.CsName == (members[a] as MemberExpression)?.Member.Name).FirstOrDefault();
                    if (read == null) return null;
                }
                ParseExpMapResult = read;
                return $"{ParseExpMatchedTable.Alias}.{read.DbNestedField}";
            }
            public SelectTableInfo GetOutsideSelectTable(ParameterExpression parameterExp)
            {
                if (parameterExp == null) return null;
                var find = _outsideTable.Where(a => a.Parameter == parameterExp).ToArray();
                if (find.Length == 1) return find[0];
                find = _outsideTable.Where(a => a.Table.Type == parameterExp.Type).ToArray();
                if (find.Length == 1) return find[0];
                find = _outsideTable.Where(a => a.Alias == parameterExp.Name).ToArray();
                if (find.Length == 1) return find[0];
                return null;
            }
        }

        public Expression ConvertStringPropertyToExpression(string property, bool fromFirstTable = false)
        {
            if (string.IsNullOrEmpty(property)) return null;
            var field = property.Split('.').Select(a => a.Trim()).ToArray();
            Expression exp = null;

            if (field.Length == 1 && fromFirstTable == false)
            {
                foreach (var tb in _tables)
                {
                    if (tb.Table.ColumnsByCs.TryGetValue(field[0], out var col) &&
                        tb.Table.Properties.TryGetValue(field[0], out var prop))
                    {
                        tb.Parameter = Expression.Parameter(tb.Table.Type, tb.Alias);
                        exp = Expression.MakeMemberAccess(tb.Parameter, prop);
                        break;
                    }
                }
                if (exp == null) throw new Exception(CoreStrings.Cannot_Match_Property(property));
            }
            else
            {
                var firstTb = _tables[0];
                var firstTbs = _tables.Where(a => a.AliasInit == field[0]).ToArray();
                if (firstTbs.Length == 1) firstTb = firstTbs[0];
                else if (fromFirstTable == false)
                {
                    firstTbs = _tables.Where(a => a.Table.Type.Name == field[0]).ToArray();
                    if (firstTbs.Length == 1) firstTb = firstTbs[0];
                }

                firstTb.Parameter = Expression.Parameter(firstTb.Table.Type, firstTb.Alias);
                var currentType = firstTb.Table.Type;
                Expression currentExp = firstTb.Parameter;

                for (var x = firstTbs.Length == 1 ? 1 : 0; x < field.Length; x++)
                {
                    var tmp1 = field[x];
                    if (_commonUtils.GetTableByEntity(currentType).Properties.TryGetValue(tmp1, out var prop) == false)
                        throw new ArgumentException($"{currentType.DisplayCsharp()} {CoreStrings.NotFound_PropertyName(tmp1)}");
                    currentType = prop.PropertyType;
                    currentExp = Expression.MakeMemberAccess(currentExp, prop);
                }
                exp = currentExp;
            }
            return exp;
        }

        public static MethodInfo _MethodDataReaderIsDBNull = typeof(DbDataReader).GetMethod("IsDBNull", new Type[] { typeof(int) });
        public static Dictionary<Type, MethodInfo> _dicMethodDataReaderGetValue = new Dictionary<Type, MethodInfo>
        {
            [typeof(bool)] = typeof(DbDataReader).GetMethod("GetBoolean", new Type[] { typeof(int) }),
            [typeof(int)] = typeof(DbDataReader).GetMethod("GetInt32", new Type[] { typeof(int) }),
            [typeof(long)] = typeof(DbDataReader).GetMethod("GetInt64", new Type[] { typeof(int) }),
            [typeof(double)] = typeof(DbDataReader).GetMethod("GetDouble", new Type[] { typeof(int) }),
            [typeof(float)] = typeof(DbDataReader).GetMethod("GetFloat", new Type[] { typeof(int) }),
            [typeof(decimal)] = typeof(DbDataReader).GetMethod("GetDecimal", new Type[] { typeof(int) }),
            [typeof(DateTime)] = typeof(DbDataReader).GetMethod("GetDateTime", new Type[] { typeof(int) }),
            [typeof(string)] = typeof(DbDataReader).GetMethod("GetString", new Type[] { typeof(int) }),
            //[typeof(Guid)] = typeof(DbDataReader).GetMethod("GetGuid", new Type[] { typeof(int) }) 有些驱动不兼容
        };

        public static MethodInfo MethodStringContains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        public static MethodInfo MethodStringStartsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        public static MethodInfo MethodStringEndsWith = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
        static ConcurrentDictionary<string, MethodInfo> MethodEnumerableDic = new ConcurrentDictionary<string, MethodInfo>();
        public static MethodInfo GetMethodEnumerable(string methodName) => MethodEnumerableDic.GetOrAdd(methodName, et =>
        {
            var methods = typeof(Enumerable).GetMethods().Where(a => a.Name == et);
            if (et == "Select")
                return methods.Where(a => a.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>)).FirstOrDefault();
            return methods.FirstOrDefault();
        });

        public List<NativeTuple<string, DbParameter[], ReadAnonymousTypeOtherInfo>> _SameSelectPendingShareData;
        internal Select0Provider SetSameSelectPendingShareData(List<NativeTuple<string, DbParameter[], ReadAnonymousTypeOtherInfo>> data)
        {
            _SameSelectPendingShareData = data;
            //_SameSelectPendingShareData?.ForEach(a => _params.AddRange(a?.Item2 ?? new DbParameter[0])); #1205 SqlServer BUG 参数化重复
            if (_SameSelectPendingShareData?.Any() == true)
            {
                var last = _SameSelectPendingShareData.Last();
                if (last == null && _SameSelectPendingShareData.Count > 1) last = _SameSelectPendingShareData[_SameSelectPendingShareData.Count - 2];
                if (last != null) 
                    _params.AddRange(last.Item2 ?? new DbParameter[0]);
            }
            return this;
        }
        internal bool SameSelectPending(ref string sql, ReadAnonymousTypeOtherInfo csspsod)
        {
            if (_SameSelectPendingShareData != null)
            {
                if (_SameSelectPendingShareData.Any() == false || _SameSelectPendingShareData.Last() != null)
                {
                    _SameSelectPendingShareData.Add(NativeTuple.Create(sql, _params.ToArray(), csspsod));
                    return true;
                }
                _SameSelectPendingShareData[_SameSelectPendingShareData.Count - 1] = NativeTuple.Create(sql, _params.ToArray(), csspsod);
                var sbSql = new StringBuilder(); //last == null flush flag
                for (var a = 0; a < _SameSelectPendingShareData.Count; a++)
                    sbSql.Append("\r\nUNION ALL\r\nselect * from (").Append(_SameSelectPendingShareData[a].Item1).Append(") ftb");
                sbSql.Remove(0, 13);
                if (_SameSelectPendingShareData.Count == 1) sbSql.Remove(0, 15).Remove(sbSql.Length - 5, 5);
                sql = sbSql.ToString();
                //dbParms = cssps.Select(a => a.Item2).SelectMany(a => a).ToArray();
                sbSql.Clear();
            }
            return false;
        }
        internal static Expression SetSameSelectPendingShareDataWithExpression(Expression exp, List<NativeTuple<string, DbParameter[], ReadAnonymousTypeOtherInfo>> data)
        {
            var callExp = exp as MethodCallExpression;
            var callExpStack = new Stack<MethodCallExpression>();
            callExpStack.Push(callExp);
            callExp = callExp.Object as MethodCallExpression; //忽略第一个方法
            while (callExp != null)
            {
                if (callExp?.Type.FullName.StartsWith("FreeSql.ISelect`") == true)
                {
                    callExpStack.Push(callExp);
                    callExp = callExp.Object as MethodCallExpression;
                    continue;
                }
                return exp;
            }
            callExp = callExpStack.Pop();
            Expression newExp = Expression.Call(
                Expression.Convert(callExp, typeof(Select0Provider)),
                typeof(Select0Provider).GetMethod(nameof(SetSameSelectPendingShareData), BindingFlags.NonPublic | BindingFlags.Instance),
                Expression.Constant(data, typeof(List<NativeTuple<string, DbParameter[], ReadAnonymousTypeOtherInfo>>))
            );
            newExp = Expression.Convert(newExp, callExp.Type);
            while (callExpStack.Any())
            {
                callExp = callExpStack.Pop();
                newExp = Expression.Call(
                    newExp,
                    callExp.Method,
                    callExp.Arguments
                );
            }
            return newExp;
        }

        public ReadAnonymousTypeAfInfo GetExpressionField(Expression newexp, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = fieldAlias == FieldAliasOptions.AsProperty ? CommonExpression.ReadAnonymousFieldAsCsName : 0;

            _commonExpression.ReadAnonymousField(_tables, _tableRule, field, map, ref index, newexp, this, _diymemexpWithTempQuery, _whereGlobalFilter, null, null, true);
            return new ReadAnonymousTypeAfInfo(map, field.Length > 0 ? field.Remove(0, 2).ToString() : null);
        }
        public string GetNestSelectSql(Expression select, string affield, Func<string, string> ToSql)
        {
            var allMemExps = new FindAllMemberExpressionVisitor(this);
            allMemExps.Visit(select);
            var fieldAlias = new Dictionary<string, bool>();
            var fieldReplaced = new Dictionary<string, bool>();
            var field = new StringBuilder();
            foreach (var memExp in allMemExps.Result)
            {
                var gef = GetExpressionField(memExp.Item1, FieldAliasOptions.AsProperty);
                var geffield = gef.field;
                if (fieldReplaced.ContainsKey(geffield)) continue;
                fieldReplaced.Add(geffield, true);

                field.Append(", ").Append(gef.field);
                if (fieldAlias.ContainsKey(memExp.Item2.Attribute.Name))
                {
                    field.Append(_commonUtils.FieldAsAlias($"aas{fieldAlias.Count}"));
                    affield = affield.Replace(geffield, $"ftba.aas{fieldAlias.Count}");
                }
                else
                {
                    fieldAlias.Add(memExp.Item2.Attribute.Name, true);
                    affield = affield.Replace(geffield, $"ftba.{string.Join(".", geffield.Split('.').Skip(1))}");
                }
            }

            var sql = ToSql(field.Remove(0, 2).ToString());
            sql = $"{_select} {affield} FROM ( \r\n    {sql.Replace("\r\n", "\r\n    ")}\r\n) ftba";
            field.Clear();
            return sql;
        }
        public class FindAllMemberExpressionVisitor : ExpressionVisitor
        {
            public List<NativeTuple<MemberExpression, ColumnInfo>> Result { get; set; } = new List<NativeTuple<MemberExpression, ColumnInfo>>();
            Select0Provider _select;
            public FindAllMemberExpressionVisitor(Select0Provider select) => _select = select;

            protected override Expression VisitMember(MemberExpression node)
            {
                var exps = new Stack<Expression>();
                Expression exp = node;
                while (exp != null)
                {
                    switch (exp.NodeType)
                    {
                        case ExpressionType.Parameter:
                            exps.Push(exp);
                            exp = null;
                            continue;
                        case ExpressionType.MemberAccess:
                            exps.Push(exp);
                            exp = (exp as MemberExpression)?.Expression;
                            continue;
                    }
                    return base.VisitMember(node);
                }
                if (exps.Any() == false) return base.VisitMember(node);
                var firstExp = exps.Pop() as ParameterExpression;
                if (firstExp == null) return base.VisitMember(node);
                var tb = _select._tables.Find(a => a.Parameter == firstExp)?.Table;
                if (tb == null) return base.VisitMember(node);

                if (tb.Columns.Any() == false && _select._diymemexpWithTempQuery != null) //匿名类，嵌套查询 DTO
                {
                    Result.Add(NativeTuple.Create(node, default(ColumnInfo)));
                    return node;
                }

                while (exps.Any())
                {
                    var memExp = exps.Pop() as MemberExpression;
                    if (tb.ColumnsByCs.TryGetValue(memExp.Member.Name, out var trycol) && exps.Any() == false)
                    {
                        Result.Add(NativeTuple.Create(node, trycol));
                        return node;
                    }
                    if (tb.Properties.ContainsKey(memExp.Member.Name))
                    {
                        tb = _select._commonUtils.GetTableByEntity(memExp.Type);
                        if (tb == null) return base.VisitMember(node);
                    }
                }
                return base.VisitMember(node);
            }
        }
        public class ReplaceMemberExpressionVisitor : ExpressionVisitor
        {
            Expression _findExp;
            Expression _replaceExp;
            public Expression Replace(Expression exp, Expression find, Expression replace) // object repval)
            {
                _findExp = find;
                _replaceExp = replace;
                //_replaceExp = Expression.Constant(repval, find.Type);
                return this.Visit(exp);
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                if (_findExp == node) return _replaceExp;
                if (node.Expression?.NodeType == ExpressionType.Parameter && node.Expression == _findExp)
                    return Expression.Property(_replaceExp, node.Member.Name);
                return base.VisitMember(node);
            }
        }
    }

    public abstract partial class Select0Provider<TSelect, T1> : Select0Provider, ISelect0<TSelect, T1> where TSelect : class
    {
        public Select0Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
            _tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T1)), Alias = "a", On = null, Type = SelectTableInfoType.From });
            this.Where(_commonUtils.WhereObject(_tables.First().Table, "a.", dywhere));
            if (_orm.CodeFirst.IsAutoSyncStructure && typeof(T1) != typeof(object)) _orm.CodeFirst.SyncStructure<T1>();
        }

        public TSelect TrackToList(Action<object> track)
        {
            _trackToList = track;
            return this as TSelect;
        }

        public TSelect Cancel(Func<bool> cancel)
        {
            _cancel = cancel;
            return this as TSelect;
        }

        public TSelect WithTransaction(DbTransaction transaction)
        {
            _transaction = transaction;
            if (transaction != null) _connection = transaction.Connection;
            return this as TSelect;
        }
        public TSelect WithConnection(DbConnection connection)
        {
            if (_transaction?.Connection != connection) _transaction = null;
            _connection = connection;
            return this as TSelect;
        }
        public TSelect WithParameters(List<DbParameter> parameters)
        {
            if (parameters != null) _params = parameters;
            return this as TSelect;
        }
        public TSelect CommandTimeout(int timeout)
        {
            _commandTimeout = timeout;
            return this as TSelect;
        }

        public TSelect GroupBy(string sql, object parms = null)
        {
            _groupby = sql;
            if (string.IsNullOrEmpty(_groupby)) return this as TSelect;
            _groupby = string.Concat(" \r\nGROUP BY ", _groupby);
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(_groupby, parms));
            return this as TSelect;
        }
        public TSelect Having(string sql, object parms = null)
        {
            if (string.IsNullOrEmpty(_groupby) || string.IsNullOrEmpty(sql)) return this as TSelect;
            _having = string.Concat(_having, " AND (", sql, ")");
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
            return this as TSelect;
        }

        public TSelect LeftJoin(Expression<Func<T1, bool>> exp)
        {
            if (exp == null) return this as TSelect;
            _tables[0].Parameter = exp.Parameters[0];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }
        public TSelect InnerJoin(Expression<Func<T1, bool>> exp)
        {
            if (exp == null) return this as TSelect;
            _tables[0].Parameter = exp.Parameters[0];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }
        public TSelect RightJoin(Expression<Func<T1, bool>> exp)
        {
            if (exp == null) return this as TSelect;
            _tables[0].Parameter = exp.Parameters[0];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }
        public TSelect LeftJoin<T2>(Expression<Func<T1, T2, bool>> exp)
        {
            if (exp == null) return this as TSelect;
            _tables[0].Parameter = exp.Parameters[0];
            if (_tables.Count > 1 && _tables[1].Table.Type == typeof(T2)) _tables[1].Parameter = exp.Parameters[1];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
        }
        public TSelect InnerJoin<T2>(Expression<Func<T1, T2, bool>> exp)
        {
            if (exp == null) return this as TSelect;
            _tables[0].Parameter = exp.Parameters[0];
            if (_tables.Count > 1 && _tables[1].Table.Type == typeof(T2)) _tables[1].Parameter = exp.Parameters[1];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
        }
        public TSelect RightJoin<T2>(Expression<Func<T1, T2, bool>> exp)
        {
            if (exp == null) return this as TSelect;
            _tables[0].Parameter = exp.Parameters[0];
            if (_tables.Count > 1 && _tables[1].Table.Type == typeof(T2)) _tables[1].Parameter = exp.Parameters[1];
            return this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
        }

        public TSelect InnerJoin(string sql, object parms = null)
        {
            if (string.IsNullOrEmpty(sql)) return this as TSelect;
            _join.Append(" \r\nINNER JOIN ").Append(sql);
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
            return this as TSelect;
        }
        public TSelect LeftJoin(string sql, object parms = null)
        {
            if (string.IsNullOrEmpty(sql)) return this as TSelect;
            _join.Append(" \r\nLEFT JOIN ").Append(sql);
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
            return this as TSelect;
        }
        public TSelect RightJoin(string sql, object parms = null)
        {
            if (string.IsNullOrEmpty(sql)) return this as TSelect;
            _join.Append(" \r\nRIGHT JOIN ").Append(sql);
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
            return this as TSelect;
        }
        public TSelect RawJoin(string sql)
        {
            if (string.IsNullOrEmpty(sql)) return this as TSelect;
            _join.Append(" \r\n").Append(sql);

            //fsql.Select<User1, UserGroup>().RawJoin("FULL JOIN UserGroup b ON b.id = a.GroupId").ToSql((a, b) => new { user = a, group = b });
            foreach (var tb in _tables)
                if (sql.Contains($" {tb.Table.DbName} ") || sql.Contains($" {_commonUtils.QuoteSqlName(tb.Table.DbName)} "))
                    tb.Type = SelectTableInfoType.RawJoin;
            return this as TSelect;
        }

        public TSelect Limit(int limit)
        {
            _limit = limit;
            return this as TSelect;
        }
        public TSelect Master()
        {
            _select = $" {_select.Trim()} ";
            return this as TSelect;
        }
        public TSelect Offset(int offset) => this.Skip(offset) as TSelect;

        public TSelect OrderBy(string sql, object parms = null) => this.OrderBy(true, sql, parms);
        public TSelect OrderBy(bool condition, string sql, object parms = null)
        {
            if (condition == false) return this as TSelect;
            if (string.IsNullOrEmpty(sql)) _orderby = null;
            var isnull = string.IsNullOrEmpty(_orderby);
            _orderby = string.Concat(isnull ? " \r\nORDER BY " : "", _orderby, isnull ? "" : ", ", sql);
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
            return this as TSelect;
        }
        public TSelect Page(int pageNumber, int pageSize)
        {
            this.Skip(Math.Max(0, pageNumber - 1) * pageSize);
            return this.Limit(pageSize) as TSelect;
        }

        public TSelect Page(BasePagingInfo pagingInfo)
        {
            pagingInfo.Count = this.Count();
            this.Skip(Math.Max(0, pagingInfo.PageNumber - 1) * pagingInfo.PageSize);
            return this.Limit(pagingInfo.PageSize) as TSelect;
        }

        public TSelect Skip(int offset)
        {
            _skip = offset;
            return this as TSelect;
        }
        public TSelect Take(int limit) => this.Limit(limit) as TSelect;

        public TSelect Distinct()
        {
            _distinct = true;
            return this as TSelect;
        }

        string GetToDeleteWhere(string alias)
        {
            var pks = _tables[0].Table.Primarys;
            var old_selectVal = _select;
            switch (_orm.Ado.DataType)
            {
                case DataType.Dameng:
                case DataType.OdbcDameng: //达梦不能这样
                case DataType.Oracle:
                case DataType.OdbcOracle:
                case DataType.Firebird:
                case DataType.GBase:
                    break;
                default:
                    _select = "SELECT ";
                    break;
            }
            try
            {
                if (pks.Length == 1)
                    return $"{_commonUtils.QuoteSqlName(_tables[0].Table.Primarys[0].Attribute.Name)} in (select * from ({this.ToSql($"{_tables[0].Alias}.{_commonUtils.QuoteSqlName(_tables[0].Table.Primarys[0].Attribute.Name)}")}) {alias})";
                else
                {
                    var concatTypes = new Type[pks.Length * 2 - 1];
                    var concatMainCols = new string[pks.Length * 2 - 1];
                    var concatInCols = new string[pks.Length * 2 - 1];
                    var concatSplit = _commonUtils.FormatSql("{0}", $",{alias},");
                    for (var a = 0; a < pks.Length; a++)
                    {
                        concatTypes[a * 2] = pks[a].CsType;
                        concatMainCols[a * 2] = _commonUtils.QuoteSqlName(pks[a].Attribute.Name);
                        concatInCols[a * 2] = $"{_tables[0].Alias}.{_commonUtils.QuoteSqlName(pks[a].Attribute.Name)}";
                        if (a < pks.Length - 1)
                        {
                            concatTypes[a * 2 + 1] = typeof(string);
                            concatMainCols[a * 2 + 1] = concatSplit;
                            concatInCols[a * 2 + 1] = concatSplit;
                        }
                    }
                    return $"{_commonUtils.StringConcat(concatMainCols, concatTypes)} in (select * from ({this.ToSql($"{_commonUtils.StringConcat(concatInCols, concatTypes)} as as1")}) {alias})";
                }
            }
            finally
            {
                _select = old_selectVal;
            }
        }
        public IDelete<T1> ToDelete()
        {
            if (_tables[0].Table.Primarys.Any() == false) throw new Exception(CoreStrings.Entity_Must_Primary_Key("ToDelete", _tables[0].Table.CsName));
            var del = (_orm as BaseDbProvider).CreateDeleteProvider<T1>(null) as DeleteProvider<T1>;
            if (_tables[0].Table.Type != typeof(T1)) del.AsType(_tables[0].Table.Type);
            if (_params.Any()) del._params = new List<DbParameter>(_params.ToArray());
            if (_whereGlobalFilter.Any()) del._whereGlobalFilter = new List<GlobalFilter.Item>(_whereGlobalFilter.ToArray());
            del.WithConnection(_connection).WithTransaction(_transaction).CommandTimeout(_commandTimeout);
            if (_is_AsTreeCte == false)
            {
                var trytbname = "";
                del.AsTable(old => GetTableRuleUnions().FirstOrDefault()?.TryGetValue(_tables[0].Table.Type, out trytbname) == true && trytbname.IndexOf(' ') == -1 ? trytbname : null);
            }
            switch (_orm.Ado.DataType)
            {
                case DataType.Dameng:
                case DataType.OdbcDameng: //达梦不能这样
                case DataType.Oracle:
                case DataType.OdbcOracle:
                case DataType.Firebird:
                case DataType.GBase:
                    break;
                default:
                    var beforeSql = this._select;
                    if (beforeSql.EndsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
                    {
                        beforeSql = beforeSql.Substring(0, beforeSql.Length - 7);
                        if (string.IsNullOrEmpty(beforeSql) == false)
                            del._interceptSql = sb => sb.Insert(0, beforeSql);
                    }
                    break;
            }
            return del.Where(GetToDeleteWhere("ftb_del"));
        }
        public IUpdate<T1> ToUpdate()
        {
            if (_tables[0].Table.Primarys.Any() == false) throw new Exception(CoreStrings.Entity_Must_Primary_Key("ToUpdate", _tables[0].Table.CsName));
            var upd = (_orm as BaseDbProvider).CreateUpdateProvider<T1>(null) as UpdateProvider<T1>;
            if (_tables[0].Table.Type != typeof(T1)) upd.AsType(_tables[0].Table.Type);
            if (_params.Any()) upd._params = new List<DbParameter>(_params.ToArray());
            if (_whereGlobalFilter.Any()) upd._whereGlobalFilter = new List<GlobalFilter.Item>(_whereGlobalFilter.ToArray());
            upd.WithConnection(_connection).WithTransaction(_transaction).CommandTimeout(_commandTimeout);
            if (_is_AsTreeCte == false)
            {
                var trytbname = "";
                upd.AsTable(old => GetTableRuleUnions().FirstOrDefault()?.TryGetValue(_tables[0].Table.Type, out trytbname) == true && trytbname.IndexOf(' ') == -1 ? trytbname : null);
            }
            switch (_orm.Ado.DataType)
            {
                case DataType.Dameng:
                case DataType.OdbcDameng: //达梦不能这样
                case DataType.Oracle:
                case DataType.OdbcOracle:
                case DataType.Firebird:
                case DataType.GBase:
                    break;
                default:
                    var beforeSql = this._select;
                    if (beforeSql.EndsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
                    {
                        beforeSql = beforeSql.Substring(0, beforeSql.Length - 7);
                        if (string.IsNullOrEmpty(beforeSql) == false)
                            upd._interceptSql = sb => sb.Insert(0, beforeSql);
                    }
                    break;
            }
            return upd.Where(GetToDeleteWhere("ftb_upd"));
        }

        protected List<Dictionary<Type, string>> GetTableRuleUnions()
        {
            var unions = new List<Dictionary<Type, string>>();
            var trs = _tableRules.Any() ? _tableRules : new List<Func<Type, string, string>>(new[] { new Func<Type, string, string>((type, oldname) => null) });

            if (trs.Count == 1 && _tables.Any(a => a.Table != null && a.Table.AsTableImpl != null && string.IsNullOrWhiteSpace(trs[0](a.Table.Type, a.Table.DbName)) == true))
            {
                string[] LocalGetTableNames(SelectTableInfo tb)
                {
                    var trname = trs[0](tb.Table.Type, tb.Table.DbName);
                    if (tb.Table.AsTableImpl != null && string.IsNullOrWhiteSpace(trname) == true)
                    {
                        string[] aret = null;
                        if (_where.Length == 0) aret = tb.Table.AsTableImpl.AllTables;
                        else aret = tb.Table.AsTableImpl.GetTableNamesBySqlWhere(_where.ToString(), _params, tb, _commonUtils);
                        if (aret.Any() == false) aret = tb.Table.AsTableImpl.AllTables.Take(1).ToArray();

                        for (var a = 0; a < aret.Length; a++)
                        {
                            if (_orm.CodeFirst.IsSyncStructureToLower) aret[a] = aret[a].ToLower();
                            if (_orm.CodeFirst.IsSyncStructureToUpper) aret[a] = aret[a].ToUpper();
                            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(tb.Table.Type, aret[a]);
                        }
                        return aret;
                    }
                    if (string.IsNullOrWhiteSpace(trname) == false)
                    {
                        if (trname.IndexOf(' ') == -1) //还可以这样：select.AsTable((a, b) => "(select * from tb_topic where clicks > 10)").Page(1, 10).ToList()
                        {
                            if (_orm.CodeFirst.IsSyncStructureToLower) trname = trname.ToLower();
                            if (_orm.CodeFirst.IsSyncStructureToUpper) trname = trname.ToUpper();
                            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(tb.Table.Type, trname);
                        }
                        else
                            trname = trname.Replace(" \r\n", " \r\n    ");
                        return new string[] { trname };
                    }
                    return new string[] { tb.Table.DbName };
                }
                var tbnames = _tables.GroupBy(a => a.Table.Type).Select(g => _tables.Where(a => a.Table.Type == g.Key).FirstOrDefault()).Select(a => new { Tb = a, Names = LocalGetTableNames(a) }).ToList();
                var dict = new Dictionary<Type, string>();
                tbnames.ForEach(a =>
                {
                    dict.Add(a.Tb.Table.Type, a.Names[0]);
                });
                unions.Add(dict);
                for (var a = 0; a < tbnames.Count; a++)
                {
                    if (tbnames[a].Names.Length <= 1) continue;
                    var unionsCount = unions.Count;
                    for (var b = 1; b < tbnames[a].Names.Length; b++)
                    {
                        for (var d = 0; d < unionsCount; d++)
                        {
                            dict = new Dictionary<Type, string>();
                            foreach (var uit in unions[d])
                                dict.Add(uit.Key, uit.Key == tbnames[a].Tb.Table.Type ? tbnames[a].Names[b] : uit.Value);
                            unions.Add(dict);
                        }
                    }
                }
                return unions;
            }
            if (trs.Any() == false) trs.Add(new Func<Type, string, string>((type, oldname) => null));
            foreach (var tr in trs)
            {
                var dict = new Dictionary<Type, string>();
                foreach (var tb in _tables)
                {
                    if (tb.Type == SelectTableInfoType.Parent) continue;
                    if (dict.ContainsKey(tb.Table.Type)) continue;
                    var name = tr?.Invoke(tb.Table.Type, tb.Table.DbName);
                    if (string.IsNullOrEmpty(name)) name = tb.Table.DbName;
                    else
                    {
                        if (name.IndexOf(' ') == -1) //还可以这样：select.AsTable((a, b) => "(select * from tb_topic where clicks > 10)").Page(1, 10).ToList()
                        {
                            if (_orm.CodeFirst.IsSyncStructureToLower) name = name.ToLower();
                            if (_orm.CodeFirst.IsSyncStructureToUpper) name = name.ToUpper();
                            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(tb.Table.Type, name);
                        }
                        else
                            name = name.Replace(" \r\n", " \r\n    ");
                    }
                    dict.Add(tb.Table.Type, name);
                }
                unions.Add(dict);
            }
            return unions;
        }
        public override void AsTableBase(Func<Type, string, string> tableRule) => AsTable(tableRule);
        public TSelect AsTable(Func<Type, string, string> tableRule)
        {
            if (_tableRules.Count == 1 && _diymemexpWithTempQuery != null && _diymemexpWithTempQuery is WithTempQueryParser tempQueryParser)
            {
                var oldTableRule = _tableRules[0];
                var newTableRule = tableRule;
                _tableRules.Clear();
                tableRule = (type, old) =>
                {
                    old = oldTableRule(type, old);
                    var newname = newTableRule(type, old);
                    return string.IsNullOrWhiteSpace(newname) ? old : newname;
                };
            }
            if (tableRule != null) _tableRules.Add(tableRule);
            return this as TSelect;
        }
        public TSelect AsAlias(Func<Type, string, string> aliasRule)
        {
            if (aliasRule != null) _aliasRule = aliasRule;
            return this as TSelect;
        }
        public TSelect AsType(Type entityType)
        {
            if (entityType == typeof(object)) throw new Exception(CoreStrings.TypeAsType_NotSupport_Object("ISelect"));
            if (entityType == _tables[0].Table.Type) return this as TSelect;
            var newtb = _commonUtils.GetTableByEntity(entityType);
            _tables[0].Table = newtb ?? throw new Exception(CoreStrings.Type_AsType_Parameter_Error("ISelect"));
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(entityType);
            return this as TSelect;
        }
        public override string ToSqlBase(string field = null) => ToSql(field);
        public abstract string ToSql(string field = null);

        public TSelect Where(string sql, object parms = null) => this.WhereIf(true, sql, parms);
        public TSelect WhereIf(bool condition, string sql, object parms = null)
        {
            if (condition == false || string.IsNullOrEmpty(sql)) return this as TSelect;
            _where.Append(" AND (").Append(sql).Append(")");
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
            return this as TSelect;
        }

        public TSelect OrderByPropertyName(string property, bool isAscending = true) => OrderByPropertyNameIf(true, property, isAscending);
        public TSelect OrderByPropertyNameIf(bool condition, string property, bool isAscending = true)
        {
            if (condition == false) return this as TSelect;
            Expression exp = ConvertStringPropertyToExpression(property);
            if (exp == null) return this as TSelect;
            var field = _commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, exp, true, _diymemexpWithTempQuery);
            if (isAscending) return this.OrderBy(field);
            return this.OrderBy($"{field} DESC");
        }

        public TSelect WhereDynamicFilter(DynamicFilterInfo filter)
        {
            if (filter == null) return this as TSelect;
            var sb = new StringBuilder();
            if (IsIgnoreFilter(filter)) filter.Field = "";
            ParseFilter(DynamicFilterLogic.And, filter, true);
            this.Where(sb.ToString());
            sb.Clear();
            return this as TSelect;

            void ParseFilter(DynamicFilterLogic logic, DynamicFilterInfo fi, bool isend)
            {
                if (string.IsNullOrEmpty(fi.Field) == false)
                {
                    Expression exp = null;
                    switch (fi.Operator)
                    {
                        case DynamicFilterOperator.Custom:
                            var fiValueCustomArray = fi.Field?.ToString().Split(new[] { ' ' }, 2);
                            if (fiValueCustomArray.Length != 2) throw new ArgumentException(CoreStrings.CustomFieldSeparatedBySpaces);
                            if (string.IsNullOrWhiteSpace(fiValueCustomArray[0])) throw new ArgumentException(CoreStrings.Custom_StaticMethodName_IsNotNull);
                            if (string.IsNullOrWhiteSpace(fiValueCustomArray[1])) throw new ArgumentException(CoreStrings.Custom_Reflection_IsNotNull);
                            var fiValue1Type = Type.GetType(fiValueCustomArray[1]);
                            if (fiValue1Type == null) throw new ArgumentException(CoreStrings.NotFound_Reflection(fiValueCustomArray[1]));
                            var fiValue0Method = fiValue1Type.GetMethod(fiValueCustomArray[0], new Type[] { typeof(string) });
                            if (fiValue0Method == null) fiValue0Method = fiValue1Type.GetMethod(fiValueCustomArray[0], new Type[] { typeof(object), typeof(string) });
                            if (fiValue0Method == null) throw new ArgumentException(CoreStrings.NotFound_Static_MethodName(fiValueCustomArray[0]));
                            if (MethodIsDynamicFilterCustomAttribute(fiValue0Method) == false) throw new ArgumentException(CoreStrings.Custom_StaticMethodName_NotSet_DynamicFilterCustom(fiValueCustomArray[0]));
                            var fiValue0MethodReturn = fiValue0Method?.Invoke(null, fiValue0Method.GetParameters()
                                    .Select(a => a.ParameterType == typeof(object) ? (object)this : 
                                        (a.ParameterType == typeof(string) ? (object)(fi.Value?.ToString()) : (object)null))
                                    .ToArray());
                            exp = fiValue0MethodReturn is Expression expression ? expression : Expression.Call(typeof(SqlExt).GetMethod("InternalRawSql", BindingFlags.NonPublic | BindingFlags.Static), Expression.Constant(fiValue0MethodReturn?.ToString(), typeof(string)));
                            break;

                        case DynamicFilterOperator.Contains:
                        case DynamicFilterOperator.StartsWith:
                        case DynamicFilterOperator.EndsWith:
                        case DynamicFilterOperator.NotContains:
                        case DynamicFilterOperator.NotStartsWith:
                        case DynamicFilterOperator.NotEndsWith:
                            exp = ConvertStringPropertyToExpression(fi.Field);
                            if (exp.Type != typeof(string)) exp = Expression.TypeAs(exp, typeof(string));
                            break;
                        default:
                            exp = ConvertStringPropertyToExpression(fi.Field);
                            break;
                    }
                    switch (fi.Operator)
                    {
                        case DynamicFilterOperator.Contains: exp = Expression.Call(exp, MethodStringContains, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fi.Value?.ToString()), exp.Type)); break;
                        case DynamicFilterOperator.StartsWith: exp = Expression.Call(exp, MethodStringStartsWith, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fi.Value?.ToString()), exp.Type)); break;
                        case DynamicFilterOperator.EndsWith: exp = Expression.Call(exp, MethodStringEndsWith, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fi.Value?.ToString()), exp.Type)); break;
                        case DynamicFilterOperator.NotContains: exp = Expression.Not(Expression.Call(exp, MethodStringContains, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fi.Value?.ToString()), exp.Type))); break;
                        case DynamicFilterOperator.NotStartsWith: exp = Expression.Not(Expression.Call(exp, MethodStringStartsWith, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fi.Value?.ToString()), exp.Type))); break;
                        case DynamicFilterOperator.NotEndsWith: exp = Expression.Not(Expression.Call(exp, MethodStringEndsWith, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fi.Value?.ToString()), exp.Type))); break;

                        case DynamicFilterOperator.Eq:
                        case DynamicFilterOperator.Equals:
                        case DynamicFilterOperator.Equal: exp = Expression.Equal(exp, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fi.Value?.ToString()), exp.Type)); break;
                        case DynamicFilterOperator.NotEqual: exp = Expression.NotEqual(exp, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fi.Value?.ToString()), exp.Type)); break;

                        case DynamicFilterOperator.GreaterThan: exp = Expression.Call(typeof(SqlExt).GetMethod("GreaterThan").MakeGenericMethod(exp.Type), exp, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fi.Value?.ToString()), exp.Type)); break;
                        case DynamicFilterOperator.GreaterThanOrEqual: exp = Expression.Call(typeof(SqlExt).GetMethod("GreaterThanOrEqual").MakeGenericMethod(exp.Type), exp, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fi.Value?.ToString()), exp.Type)); break;
                        case DynamicFilterOperator.LessThan: exp = Expression.Call(typeof(SqlExt).GetMethod("LessThan").MakeGenericMethod(exp.Type), exp, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fi.Value?.ToString()), exp.Type)); break;
                        case DynamicFilterOperator.LessThanOrEqual: exp = Expression.Call(typeof(SqlExt).GetMethod("LessThanOrEqual").MakeGenericMethod(exp.Type), exp, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fi.Value?.ToString()), exp.Type)); break;
                        case DynamicFilterOperator.Range:
                            var fiValueRangeArray = getFiListValue();
                            if (fiValueRangeArray.Length != 2) throw new ArgumentException(CoreStrings.Range_Comma_Separateda_By2Char);
                            exp = Expression.AndAlso(
                                Expression.GreaterThanOrEqual(exp, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fiValueRangeArray[0]), exp.Type)),
                                Expression.LessThan(exp, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fiValueRangeArray[1]), exp.Type)));
                            break;
                        case DynamicFilterOperator.DateRange:
                            var fiValueDateRangeArray = getFiListValue();
                            if (fiValueDateRangeArray?.Length != 2) throw new ArgumentException(CoreStrings.DateRange_Comma_Separateda_By2Char);
                            if (Regex.IsMatch(fiValueDateRangeArray[1], @"^\d\d\d\d[\-/]\d\d?[\-/]\d\d?$")) fiValueDateRangeArray[1] = DateTime.Parse(fiValueDateRangeArray[1]).AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
                            else if (Regex.IsMatch(fiValueDateRangeArray[1], @"^\d\d\d\d[\-/]\d\d?$")) fiValueDateRangeArray[1] = DateTime.Parse($"{fiValueDateRangeArray[1]}-01").AddMonths(1).ToString("yyyy-MM-dd HH:mm:ss");
                            else if (Regex.IsMatch(fiValueDateRangeArray[1], @"^\d\d\d\d$")) fiValueDateRangeArray[1] = DateTime.Parse($"{fiValueDateRangeArray[1]}-01-01").AddYears(1).ToString("yyyy-MM-dd HH:mm:ss");
                            else if (Regex.IsMatch(fiValueDateRangeArray[1], @"^\d\d\d\d[\-/]\d\d?[\-/]\d\d? \d\d?$")) fiValueDateRangeArray[1] = DateTime.Parse($"{fiValueDateRangeArray[1]}:00:00").AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
                            else if (Regex.IsMatch(fiValueDateRangeArray[1], @"^\d\d\d\d[\-/]\d\d?[\-/]\d\d? \d\d?:\d\d?$")) fiValueDateRangeArray[1] = DateTime.Parse($"{fiValueDateRangeArray[1]}:00").AddMinutes(1).ToString("yyyy-MM-dd HH:mm:ss");
                            else if (!Regex.IsMatch(fiValueDateRangeArray[1], @"^\d\d\d\d[\-/]\d\d?[\-/]\d\d? \d\d?:\d\d?:\d\d?$")) throw new ArgumentException(CoreStrings.DateRange_DateFormat_yyyy);

                            if (Regex.IsMatch(fiValueDateRangeArray[0], @"^\d\d\d\d[\-/]\d\d?$")) fiValueDateRangeArray[0] = DateTime.Parse($"{fiValueDateRangeArray[0]}-01").ToString("yyyy-MM-dd HH:mm:ss");
                            else if (Regex.IsMatch(fiValueDateRangeArray[0], @"^\d\d\d\d$")) fiValueDateRangeArray[0] = DateTime.Parse($"{fiValueDateRangeArray[0]}-01-01").ToString("yyyy-MM-dd HH:mm:ss");
                            else if (Regex.IsMatch(fiValueDateRangeArray[0], @"^\d\d\d\d[\-/]\d\d?[\-/]\d\d? \d\d?$")) fiValueDateRangeArray[0] = DateTime.Parse($"{fiValueDateRangeArray[0]}:00:00").ToString("yyyy-MM-dd HH:mm:ss");
                            else if (Regex.IsMatch(fiValueDateRangeArray[0], @"^\d\d\d\d[\-/]\d\d?[\-/]\d\d? \d\d?:\d\d?$")) fiValueDateRangeArray[0] = DateTime.Parse($"{fiValueDateRangeArray[0]}:00").ToString("yyyy-MM-dd HH:mm:ss");

                            exp = Expression.AndAlso(
                                Expression.GreaterThanOrEqual(exp, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fiValueDateRangeArray[0]), exp.Type)),
                                Expression.LessThan(exp, Expression.Constant(Utils.GetDataReaderValue(exp.Type, fiValueDateRangeArray[1]), exp.Type)));
                            break;
                        case DynamicFilterOperator.Any:
                        case DynamicFilterOperator.NotAny:
                            var fiValueAnyArray = getFiListValue();
                            if (fiValueAnyArray.Length == 0) break;
                            var fiValueAnyArrayType = exp.Type.MakeArrayType();
                            exp = Expression.Call(GetMethodEnumerable("Contains").MakeGenericMethod(exp.Type), Expression.Constant(Utils.GetDataReaderValue(fiValueAnyArrayType, fiValueAnyArray), fiValueAnyArrayType), exp);
                            if (fi.Operator == DynamicFilterOperator.NotAny) exp = Expression.Not(exp);
                            break;
                    }

                    string[] getFiListValue()
                    {
                        if (fi.Value is string fiValueString) return fiValueString.Split(',');
                        if (fi.Value is IEnumerable fiValueIe)
                        {
                            var fiValueList = new List<string>();
                            foreach (var fiValueIeItem in fiValueIe)
                            {
                                if (fiValueIeItem is DateTime fiValueIeItemDateTime)
                                    fiValueList.Add(fiValueIeItemDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                else
                                    fiValueList.Add(string.Concat(fiValueIeItem));
                            }
                            return fiValueList.ToArray();
                        }
                        var fiValueType = fi.Value.GetType();
                        if (fiValueType.FullName == "System.Text.Json.JsonElement")
                        {
                            var fiValueKind = fiValueType.GetProperty("ValueKind").GetValue(fi.Value, null).ToString();
                            if (fiValueKind == "Array")
                            {
                                fiValueIe = fiValueType.GetMethod("EnumerateArray", new Type[0])?.Invoke(fi.Value, null) as IEnumerable;
                                var fiValueList = new List<string>();
                                foreach (var fiValueIeItem in fiValueIe)
                                {
                                    if (fiValueIeItem is DateTime fiValueIeItemDateTime)
                                        fiValueList.Add(fiValueIeItemDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                    else
                                        fiValueList.Add(string.Concat(fiValueIeItem));
                                }
                                return fiValueList.ToArray();
                            }
                            return fi.Value.ToString().Split(',');
                        }
                        return new string[0];
                    }

                    var sql = _commonExpression.ExpressionWhereLambda(_tables, _tableRule, exp, _diymemexpWithTempQuery, null, _params);

                    sb.Append(sql);
                }
                if (fi.Filters?.Any() == true)
                {
                    fi.Filters = fi.Filters.Where(a => IsIgnoreFilter(a) == false).ToList(); //忽略 like '%%'

                    if (fi.Filters.Any())
                    {
                        if (string.IsNullOrEmpty(fi.Field) == false)
                            sb.Append(" AND ");
                        if (fi.Logic == DynamicFilterLogic.Or) sb.Append("(");
                        for (var x = 0; x < fi.Filters.Count; x++)
                            ParseFilter(fi.Logic, fi.Filters[x], x == fi.Filters.Count - 1);
                        if (fi.Logic == DynamicFilterLogic.Or) sb.Append(")");
                    }
                }

                if (isend == false)
                {
                    if (string.IsNullOrEmpty(fi.Field) == false || fi.Filters?.Any() == true)
                    {
                        switch (logic)
                        {
                            case DynamicFilterLogic.And: sb.Append(" AND "); break;
                            case DynamicFilterLogic.Or: sb.Append(" OR "); break;
                        }
                    }
                }
            }

            bool IsIgnoreFilter(DynamicFilterInfo testFilter)
            {
                return string.IsNullOrEmpty(testFilter.Field) == false &&
                    new[] { DynamicFilterOperator.Contains, DynamicFilterOperator.StartsWith, DynamicFilterOperator.EndsWith }.Contains(testFilter.Operator) &&
                    string.IsNullOrEmpty(testFilter.Value?.ToString());
            }
        }
        static ConcurrentDictionary<MethodInfo, bool> _dicMethodIsDynamicFilterCustomAttribute = new ConcurrentDictionary<MethodInfo, bool>();
        static bool MethodIsDynamicFilterCustomAttribute(MethodInfo method) => _dicMethodIsDynamicFilterCustomAttribute.GetOrAdd(method, m =>
        {
            object[] attrs = null;
            try
            {
                attrs = m.GetCustomAttributes(false).ToArray(); //.net core 反射存在版本冲突问题，导致该方法异常
            }
            catch { }

            var dyattr = attrs?.Where(a =>
            {
                return ((a as Attribute)?.TypeId as Type)?.Name == "DynamicFilterCustomAttribute";
            }).FirstOrDefault();
            return dyattr != null;
        });

        public TSelect DisableGlobalFilter(params string[] name)
        {
            if (_whereGlobalFilter.Any() == false) return this as TSelect;
            if (name?.Any() != true)
            {
                _whereGlobalFilter.Clear();
                return this as TSelect;
            }
            foreach (var n in name)
            {
                if (n == null) continue;
                var idx = _whereGlobalFilter.FindIndex(a => string.Compare(a.Name, n, true) == 0);
                if (idx == -1) continue;
                _whereGlobalFilter.RemoveAt(idx);
            }
            return this as TSelect;
        }
        public TSelect ForUpdate(bool noawait = false)
        {
            if (_transaction == null && _orm.Ado.TransactionCurrentThread != null) this.WithTransaction(_orm.Ado.TransactionCurrentThread);
            if (_transaction == null && _resolveHookTransaction != null) this.WithTransaction(_resolveHookTransaction());
            if (_transaction == null) throw new Exception($"{CoreStrings.Begin_Transaction_Then_ForUpdate}");
            switch (_orm.Ado.DataType)
            {
                case DataType.MySql:
                case DataType.OdbcMySql:
                    _tosqlAppendContent = $"{_tosqlAppendContent} for update";
                    break;
                case DataType.SqlServer:
                case DataType.OdbcSqlServer:
                    _aliasRule = (_, old) => $"{old} With(UpdLock, RowLock{(noawait ? ", NoWait" : "")})";
                    break;
                case DataType.PostgreSQL:
                case DataType.OdbcPostgreSQL:
                case DataType.KingbaseES:
                case DataType.OdbcKingbaseES:
                    _tosqlAppendContent = $"{_tosqlAppendContent} for update{(noawait ? " nowait" : "")}";
                    break;
                case DataType.Oracle:
                case DataType.OdbcOracle:
                case DataType.Dameng:
                case DataType.OdbcDameng:
                    _tosqlAppendContent = $"{_tosqlAppendContent} for update{(noawait ? " nowait" : "")}";
                    break;
                case DataType.Sqlite:
                    break;
                case DataType.GBase:
                case DataType.ShenTong: //神通测试中发现，不支持 nowait
                    _tosqlAppendContent = $"{_tosqlAppendContent} for update";
                    break;
                case DataType.Firebird:
                    _tosqlAppendContent = $"{_tosqlAppendContent} for update with lock";
                    break;
            }
            return this as TSelect;
        }

        public ISelect<TDto> InternalWithTempQuery<TDto>(Expression selector)
        {
            if (_orm.CodeFirst.IsAutoSyncStructure)
                (_orm.CodeFirst as CodeFirstProvider)._dicSycedTryAdd(typeof(TDto)); //._dicSyced.TryAdd(typeof(TReturn), true);
            var ret = (_orm as BaseDbProvider).CreateSelectProvider<TDto>(null) as Select1Provider<TDto>;
            ret._commandTimeout = _commandTimeout;
            ret._connection = _connection;
            ret._transaction = _transaction;
            ret._whereGlobalFilter = new List<GlobalFilter.Item>(_whereGlobalFilter.ToArray());
            ret._cancel = _cancel;
            ret._params.AddRange(_params);
            if (ret._tables[0].Table == null) ret._tables[0].Table = TableInfo.GetDefaultTable(typeof(TDto));
            if (selector is LambdaExpression lambdaExp && lambdaExp != null)
            {
                for (var a = 0; a < lambdaExp.Parameters.Count; a++)
                    _tables[a].Parameter = lambdaExp.Parameters[a];
            }
            var parser = new WithTempQueryParser(this, null, selector, ret._tables[0]);
            var sql = $"\r\n{this.ToSql(parser._insideSelectList[0].InsideField)}";
            ret.WithSql(sql);
            ret._diymemexpWithTempQuery = parser;
            return ret;
        }

        public bool Any()
        {
            this.Limit(1);
            return this.ToList<int>($"{1}{_commonUtils.FieldAsAlias("as1")}").Sum() > 0; //这里的 Sum 为了分表查询
        }
        public long Count()
        {
            var tmpOrderBy = _orderby;
            var tmpSkip = _skip;
            var tmpLimit = _limit;
            var tmpDistinct = _distinct;
            _orderby = null; //解决 select count(1) from t order by id 这样的 SQL 错误
            _skip = 0;
            _limit = 0;
            _distinct = false;
            try
            {
                var countField = "1";
                if (tmpDistinct && _selectExpression != null) countField = $"distinct {this.GetExpressionField(_selectExpression, FieldAliasOptions.AsProperty).field}";
                return this.ToList<int>($"count({countField}){_commonUtils.FieldAsAlias("as1")}").Sum(); //这里的 Sum 为了分表查询
            }
            finally
            {
                _orderby = tmpOrderBy;
                _skip = tmpSkip;
                _limit = tmpLimit;
                _distinct = tmpDistinct;
            }
        }
        public TSelect Count(out long count)
        {
            count = this.Count();
            return this as TSelect;
        }
        public List<T1> ToList() => ToList(false);
        public virtual List<T1> ToList(bool includeNestedMembers)
        {
            if (_diymemexpWithTempQuery != null && _diymemexpWithTempQuery is WithTempQueryParser withTempQueryParser)
            {
                if (withTempQueryParser._outsideTable[0] != _tables[0])
                {
                    var tps = _tables.Select(a =>
                    {
                        var tp = Expression.Parameter(a.Table.Type, a.Alias);
                        a.Parameter = tp;
                        return tp;
                    }).ToArray();
                    return this.InternalToList<T1>(tps[0]);
                }
                return this.ToListMapReaderPrivate<T1>(withTempQueryParser._insideSelectList[0].InsideAf, null);
            }
            if (_selectExpression != null) return this.InternalToList<T1>(_selectExpression);
            return this.ToListPrivate(includeNestedMembers == false ? this.GetAllFieldExpressionTreeLevel2() : this.GetAllFieldExpressionTreeLevelAll(), null);
        }
        public T1 ToOne()
        {
            this.Limit(1);
            return this.ToList().FirstOrDefault();
        }
        public T1 First() => this.ToOne();

#if net40
#else
        async public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            this.Limit(1);
            return (await this.ToListAsync<int>($"1{_commonUtils.FieldAsAlias("as1")}", cancellationToken)).Sum() > 0; //这里的 Sum 为了分表查询
        }
        async public Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            var tmpOrderBy = _orderby;
            var tmpSkip = _skip;
            var tmpLimit = _limit;
            var tmpDistinct = _distinct;
            _orderby = null;
            _skip = 0;
            _limit = 0;
            _distinct = false;
            try
            {
                var countField = "1";
                if (tmpDistinct && _selectExpression != null) countField = $"distinct {this.GetExpressionField(_selectExpression, FieldAliasOptions.AsProperty).field}";
                return (await this.ToListAsync<int>($"count({countField}){_commonUtils.FieldAsAlias("as1")}", cancellationToken)).Sum(); //这里的 Sum 为了分表查询
            }
            finally
            {
                _orderby = tmpOrderBy;
                _skip = tmpSkip;
                _limit = tmpLimit;
                _distinct = tmpDistinct;
            }
        }
        public Task<List<T1>> ToListAsync(CancellationToken cancellationToken = default) => ToListAsync(false, cancellationToken);
        public virtual Task<List<T1>> ToListAsync(bool includeNestedMembers = false, CancellationToken cancellationToken = default)
        {
            if (_diymemexpWithTempQuery != null && _diymemexpWithTempQuery is WithTempQueryParser withTempQueryParser)
            {
                if (withTempQueryParser._outsideTable[0] != _tables[0])
                {
                    var tps = _tables.Select(a =>
                    {
                        var tp = Expression.Parameter(a.Table.Type, a.Alias);
                        a.Parameter = tp;
                        return tp;
                    }).ToArray();
                    return this.InternalToListAsync<T1>(tps[0], cancellationToken);
                }
                return this.ToListMapReaderPrivateAsync<T1>((_diymemexpWithTempQuery as WithTempQueryParser)._insideSelectList[0].InsideAf, null, cancellationToken);
            }
            if (_selectExpression != null) return this.InternalToListAsync<T1>(_selectExpression, cancellationToken);
            return this.ToListPrivateAsync(includeNestedMembers == false ? this.GetAllFieldExpressionTreeLevel2() : this.GetAllFieldExpressionTreeLevelAll(), null, cancellationToken);
        }
        async public Task<T1> ToOneAsync(CancellationToken cancellationToken = default)
        {
            this.Limit(1);
            return (await this.ToListAsync(false, cancellationToken)).FirstOrDefault();
        }
        public Task<T1> FirstAsync(CancellationToken cancellationToken = default) => this.ToOneAsync(cancellationToken);
#endif
    }
}