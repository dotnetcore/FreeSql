﻿using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

    public abstract class Select1Provider<T1> : Select0Provider<ISelect<T1>, T1>, ISelect<T1>
    {
        public Select1Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere)
        {
            _whereGlobalFilter = _orm.GlobalFilter.GetFilters();
        }

        protected ISelect<T1> InternalFrom(LambdaExpression lambdaExp)
        {
            if (lambdaExp == null) return this;
            for (var a = 1; a < lambdaExp.Parameters.Count; a++)
            {
                var tb = _commonUtils.GetTableByEntity(lambdaExp.Parameters[a].Type);
                if (tb == null) throw new ArgumentException(CoreStrings.Type_Error_Name(lambdaExp.Parameters[a].Name));
                _tables.Add(new SelectTableInfo { Table = tb, Alias = lambdaExp.Parameters[a].Name, On = null, Type = SelectTableInfoType.From });
            }
            var exp = lambdaExp.Body;
            if (exp?.NodeType == ExpressionType.Call)
            {
                var expCall = exp as MethodCallExpression;
                var stockCall = new Stack<MethodCallExpression>();
                while (expCall != null)
                {
                    stockCall.Push(expCall);
                    expCall = expCall.Object as MethodCallExpression;
                }
                while (stockCall.Any())
                {
                    expCall = stockCall.Pop();

                    switch (expCall.Method.Name)
                    {
                        case "Where": this.InternalWhere(expCall.Arguments[0]); break;
                        case "WhereIf":
                            if ((bool)Expression.Lambda(expCall.Arguments[0]).Compile().DynamicInvoke())
                                this.InternalWhere(expCall.Arguments[1]);
                            break;
                        case "OrderBy":
                            if (expCall.Arguments.Count == 2 && expCall.Arguments[0].Type == typeof(bool))
                            {
                                var ifcond = _commonExpression.ExpressionSelectColumn_MemberAccess(null, _tableRule, null, SelectTableInfoType.From, expCall.Arguments[0], false, _diymemexpWithTempQuery);
                                if (ifcond == "1" || ifcond == "'t'")
                                    this.InternalOrderBy(expCall.Arguments.LastOrDefault());
                                break;
                            }
                            this.InternalOrderBy(expCall.Arguments.LastOrDefault());
                            break;
                        case "OrderByDescending":
                            if (expCall.Arguments.Count == 2 && expCall.Arguments[0].Type == typeof(bool))
                            {
                                var ifcond = _commonExpression.ExpressionSelectColumn_MemberAccess(null, _tableRule, null, SelectTableInfoType.From, expCall.Arguments[0], false, _diymemexpWithTempQuery);
                                if (ifcond == "1" || ifcond == "'t'" || ifcond == "-1")//MsAccess -1
                                    this.InternalOrderByDescending(expCall.Arguments.LastOrDefault());
                                break;
                            }
                            this.InternalOrderByDescending(expCall.Arguments.LastOrDefault());
                            break;

                        case "LeftJoin": this.InternalJoin(expCall.Arguments[0], SelectTableInfoType.LeftJoin); break;
                        case "InnerJoin": this.InternalJoin(expCall.Arguments[0], SelectTableInfoType.InnerJoin); break;
                        case "RightJoin": this.InternalJoin(expCall.Arguments[0], SelectTableInfoType.RightJoin); break;
                        default: throw new NotImplementedException(CoreStrings.Not_Implemented_Name(expCall.Method.Name));
                    }
                }
            }
            return this;
        }

        public ISelect<T1> As(string alias)
        {
            var oldAs = _tables.First().Alias;
            var newAs = string.IsNullOrEmpty(alias) ? "a" : alias;
            if (oldAs != newAs)
            {
                _tables.First().Alias = newAs;
                var wh = _where.ToString();
                _where.Replace($" {oldAs}.", $" {newAs}.");
            }
            return this;
        }

        public double Avg<TMember>(Expression<Func<T1, TMember>> column)
        {
            if (column == null) return default(double);
            _tables[0].Parameter = column.Parameters[0];
            return this.InternalAvg(column?.Body);
        }

        public abstract ISelect<T1, T2> From<T2>(Expression<Func<ISelectFromExpression<T1>, T2, ISelectFromExpression<T1>>> exp = null) where T2 : class;
        public abstract ISelect<T1, T2, T3> From<T2, T3>(Expression<Func<ISelectFromExpression<T1>, T2, T3, ISelectFromExpression<T1>>> exp = null) where T2 : class where T3 : class;
        public abstract ISelect<T1, T2, T3, T4> From<T2, T3, T4>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, ISelectFromExpression<T1>>> exp = null) where T2 : class where T3 : class where T4 : class;
        public abstract ISelect<T1, T2, T3, T4, T5> From<T2, T3, T4, T5>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, ISelectFromExpression<T1>>> exp = null) where T2 : class where T3 : class where T4 : class where T5 : class;
        public abstract ISelect<T1, T2, T3, T4, T5, T6> From<T2, T3, T4, T5, T6>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, ISelectFromExpression<T1>>> exp = null) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class;
        public abstract ISelect<T1, T2, T3, T4, T5, T6, T7> From<T2, T3, T4, T5, T6, T7>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, ISelectFromExpression<T1>>> exp = null) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class;
        public abstract ISelect<T1, T2, T3, T4, T5, T6, T7, T8> From<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, ISelectFromExpression<T1>>> exp = null) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class;
        public abstract ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, ISelectFromExpression<T1>>> exp = null) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class;
        public abstract ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, ISelectFromExpression<T1>>> exp = null) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class;

        public abstract ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, ISelectFromExpression<T1>>> exp = null) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class;
        public abstract ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, ISelectFromExpression<T1>>> exp = null) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class;
        public abstract ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, ISelectFromExpression<T1>>> exp = null) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class;
        public abstract ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, ISelectFromExpression<T1>>> exp = null) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class;
        public abstract ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, ISelectFromExpression<T1>>> exp = null) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class;
        public abstract ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, ISelectFromExpression<T1>>> exp = null) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class where T16 : class;

        public ISelect<T1, T2> FromQuery<T2>(ISelect<T2> select2) where T2 : class
        {
            var ret = From<T2>();
            return FromQueryMulti(ret, new[] { typeof(T2) }, new[] { select2 as Select0Provider });
        }

        public TQuery FromQueryMulti<TQuery>(TQuery ret, Type[] entityTypes, Select0Provider[] querys)
        {
            var retsp = ret as Select0Provider;
            var rettbs = retsp._tables;
            var sql2List = new string[querys.Length];
            for (var a = 0; a < querys.Length; a++)
            {
                var select2 = querys[a];
                var entityType2 = entityTypes[a];
                var rettb2 = rettbs[a + 1];
                if (rettb2.Table == null) rettb2.Table = TableInfo.GetDefaultTable(entityType2);
                (retsp._diymemexpWithTempQuery as WithTempQueryParser)?.Append(select2, rettb2);
                string sql2 = null;
                if (select2._diymemexpWithTempQuery == null)
                {
                    if (select2._tableRule == null && select2._tables[0].Table.Type == entityType2 && select2.IsDefaultSqlContent == true)
                        continue;
                    var selectorParameter = Expression.Parameter(entityType2, select2._tables[0].Alias);
                    var selector = Expression.Lambda(selectorParameter, selectorParameter);
                    select2._tables[0].Parameter = selectorParameter;
                    var af = select2.GetExpressionField(selector, FieldAliasOptions.AsProperty);
                    sql2 = select2.ToSqlBase(af.field);
                }
                else
                {
                    if (retsp._diymemexpWithTempQuery == null)
                        retsp._diymemexpWithTempQuery = new WithTempQueryParser(null, null, null, null).Append(select2, rettb2);
                    if (select2._tableRule != null && select2.IsDefaultSqlContent == true)
                    {
                        sql2 = select2._tableRule(select2._tables[0].Table.Type, null);
                        if (sql2.StartsWith("(") && sql2.EndsWith(")")) sql2 = sql2.Substring(1, sql2.Length - 2);
                        if (sql2.StartsWith(" \r\n")) sql2 = sql2.Substring(3);
                    }
                    if (string.IsNullOrWhiteSpace(sql2))
                        sql2 = select2.ToSqlBase("*");
                }
                sql2List[a] = sql2;
            }
            if (retsp._tableRules.Count > 0)
            {
                var tbrules = retsp._tableRules.ToList();
                retsp._tableRules.Clear();
                tbrules.ForEach(tbrule =>
                {
                    var tbruler1 = tbrule(typeof(T1), retsp._tables[0].Table.DbName);
                    if (string.IsNullOrWhiteSpace(tbruler1) == false)
                        retsp._tableRules.Add((type, old) =>
                        {
                            if (type == typeof(T1)) return tbruler1;
                            for (var a = 0; a < entityTypes.Length; a++)
                                if (entityTypes[a] == type) return string.IsNullOrWhiteSpace(sql2List[a]) ? old : $"( \r\n{sql2List[a]})";
                            return old;
                        });
                });
            }
            if (retsp._tableRules.Count == 0)
            {
                retsp.AsTableBase((type, old) =>
                {
                    for (var a = 0; a < entityTypes.Length; a++)
                        if (entityTypes[a] == type) return string.IsNullOrWhiteSpace(sql2List[a]) ? old : $"( \r\n{sql2List[a]})";
                    return old;
                });
            }
            return ret;
        }
        public ISelect<T1, T2, T3> FromQuery<T2, T3>(ISelect<T2> select2, ISelect<T3> select3) where T2 : class where T3 : class
        {
            var ret = From<T2, T3>();
            return FromQueryMulti(ret, new[] { typeof(T2), typeof(T3) }, new[] { select2 as Select0Provider, select3 as Select0Provider });
        }
        public ISelect<T1, T2, T3, T4> FromQuery<T2, T3, T4>(ISelect<T2> select2, ISelect<T3> select3, ISelect<T4> select4) where T2 : class where T3 : class where T4 : class
        {
            var ret = From<T2, T3, T4>();
            return FromQueryMulti(ret, new[] { typeof(T2), typeof(T3), typeof(T4) }, new[] { select2 as Select0Provider, select3 as Select0Provider, select4 as Select0Provider });
        }
        public ISelect<T1, T2, T3, T4, T5> FromQuery<T2, T3, T4, T5>(ISelect<T2> select2, ISelect<T3> select3, ISelect<T4> select4, ISelect<T5> select5) where T2 : class where T3 : class where T4 : class where T5 : class
        {
            var ret = From<T2, T3, T4, T5>();
            return FromQueryMulti(ret, new[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, new[] { select2 as Select0Provider, select3 as Select0Provider, select4 as Select0Provider, select5 as Select0Provider });
        }
        
        public ISelect<T1> UnionAll(params ISelect<T1>[] querys)
        {
            var ret = (_orm as BaseDbProvider).CreateSelectProvider<T1>(null) as Select1Provider<T1>;
            var sb = new StringBuilder().Append(LocalGetQuerySql(this));
            foreach (var select2 in querys)
                sb.Append(" \r\nUNION ALL \r\n").Append(LocalGetQuerySql(select2));
            ret.WithSql(sb.ToString());
            sb.Clear();
            ret._commandTimeout = _commandTimeout;
            ret._connection = _connection;
            ret._transaction = _transaction;
            ret._whereGlobalFilter = new List<GlobalFilter.Item>(_whereGlobalFilter.ToArray());
            ret._cancel = _cancel;
            ret._diymemexpWithTempQuery = _diymemexpWithTempQuery;
            ret._tables[0] = _tables[0];
            ret._params = _params;
            return ret;

            string LocalGetQuerySql(ISelect<T1> query)
            {
                var query2 = query as Select0Provider;
                if (query2._diymemexpWithTempQuery == null ||
                    query2.IsDefaultSqlContent == false) return query.ToSql();
				var sql2 = query2._tableRule(query2._tables[0].Table.Type, null);
				if (sql2.StartsWith("(") && sql2.EndsWith(")")) sql2 = sql2.Substring(1, sql2.Length - 2);
				if (sql2.StartsWith(" \r\n")) sql2 = sql2.Substring(3);
                return sql2;
			}
        }

        public ISelectGrouping<TKey, T1> GroupBy<TKey>(Expression<Func<T1, TKey>> columns)
        {
            if (columns == null) return this.InternalGroupBy<TKey, T1>(columns);
            _tables[0].Parameter = columns.Parameters[0];
            return this.InternalGroupBy<TKey, T1>(columns);
        }

        public TMember Max<TMember>(Expression<Func<T1, TMember>> column)
        {
            if (column == null) return default(TMember);
            _tables[0].Parameter = column.Parameters[0];
            return this.InternalMax<TMember>(column.Body);
        }
        public TMember Min<TMember>(Expression<Func<T1, TMember>> column)
        {
            if (column == null) return default(TMember);
            _tables[0].Parameter = column.Parameters[0];
            return this.InternalMin<TMember>(column.Body);
        }
        public void OrderByReflection(LambdaExpression column, bool isDescending)
        {
            if (column == null) return;
            _tables[0].Parameter = column.Parameters[0];
            if (isDescending) this.InternalOrderByDescending(column.Body);
            else this.InternalOrderBy(column.Body);
        }
        public ISelect<T1> OrderBy<TMember>(Expression<Func<T1, TMember>> column) => this.OrderBy(true, column);
        public ISelect<T1> OrderBy<TMember>(bool condition, Expression<Func<T1, TMember>> column)
        {
            if (condition == false || column == null) return this;
            _tables[0].Parameter = column.Parameters[0];
            return this.InternalOrderBy(column.Body);
        }
        public ISelect<T1> OrderByDescending<TMember>(Expression<Func<T1, TMember>> column) => this.OrderByDescending(true, column);
        public ISelect<T1> OrderByDescending<TMember>(bool condition, Expression<Func<T1, TMember>> column)
        {
            if (condition == false || column == null) return this;
            _tables[0].Parameter = column.Parameters[0];
            return this.InternalOrderByDescending(column.Body);
        }
        public ISelect<T1> OrderByIf<TMember>(bool condition, Expression<Func<T1, TMember>> column, bool descending = false) =>
            descending ? this.OrderByDescending(condition, column) : this.OrderBy(condition, column);

        public decimal Sum<TMember>(Expression<Func<T1, TMember>> column)
        {
            if (column == null) return default(decimal);
            _tables[0].Parameter = column.Parameters[0];
            return this.InternalSum(column.Body);
        }

        class IncludeManyNewInit
        {
            public TableInfo Table { get; }
            public Dictionary<string, IncludeManyNewInit> Childs { get; } = new Dictionary<string, IncludeManyNewInit>();
            public Expression CurrentExpression { get; }
            public bool IsOutputPrimary { get; set; }
            public List<ColumnInfo> OutputColumns { get; } = new List<ColumnInfo>();
            public IncludeManyNewInit(TableInfo table, Expression currentExpression)
            {
                this.Table = table;
                this.CurrentExpression = currentExpression;
            }
        }
        public List<TReturn> ToList<TReturn>(Expression<Func<T1, TReturn>> select)
        {
            if (select == null) return this.InternalToList<TReturn>(select?.Body);
            _tables[0].Parameter = select.Parameters[0];
            if (_includeToList?.Any() != true) return this.InternalToList<TReturn>(select.Body);

            var findIncludeMany = new List<string>(); //支持指定已经使用 IncudeMany 的导航属性
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = 0;
            _commonExpression.ReadAnonymousField(_tables, _tableRule, field, map, ref index, select.Body, this, _diymemexpWithTempQuery, _whereGlobalFilter, findIncludeMany, null, true);
            var af = new ReadAnonymousTypeAfInfo(map, field.Length > 0 ? field.Remove(0, 2).ToString() : null);
            if (findIncludeMany.Any() == false) return this.ToListMapReaderPrivate<TReturn>(af, null);

            var parmExp = Expression.Parameter(_tables[0].Table.Type, _tables[0].Alias);
            var incNewInit = new IncludeManyNewInit(_tables[0].Table, parmExp);
            foreach (var inc in _includeInfo)
            {
                var curIncNewInit = incNewInit;
                Expression curParmExp = parmExp;
                for (var a = 0; a < inc.Value.Item1.Length - 1; a++)
                {
                    curParmExp = Expression.MakeMemberAccess(parmExp, inc.Value.Item1[a].Member);
                    if (curIncNewInit.Childs.ContainsKey(inc.Value.Item1[a].Member.Name) == false)
                        curIncNewInit.Childs.Add(inc.Value.Item1[a].Member.Name, curIncNewInit = new IncludeManyNewInit(_orm.CodeFirst.GetTableByEntity(inc.Value.Item1[a].Type), curParmExp));
                    else
                        curIncNewInit = curIncNewInit.Childs[inc.Value.Item1[a].Member.Name];
                }
                curIncNewInit.IsOutputPrimary = true;
                var outcols = inc.Value.Item2.Columns.Where(a => a.Attribute.IsPrimary == false).ToArray();
                if (outcols.Any()) curIncNewInit.OutputColumns.AddRange(outcols);
            }
            MemberInitExpression GetIncludeManyNewInitExpression(IncludeManyNewInit imni)
            {
                var bindings = new List<MemberBinding>();
                if (imni.IsOutputPrimary)
                {
                    bindings.AddRange(imni.Table.Primarys.Select(a => Expression.Bind(imni.Table.Properties[a.CsName], Expression.MakeMemberAccess(imni.CurrentExpression, imni.Table.Properties[a.CsName]))));
                    if (imni.OutputColumns.Any()) bindings.AddRange(imni.OutputColumns.Select(a => Expression.Bind(imni.Table.Properties[a.CsName], Expression.MakeMemberAccess(imni.CurrentExpression, imni.Table.Properties[a.CsName]))));
                }
                if (imni.Childs.Any()) bindings.AddRange(imni.Childs.Select(a => Expression.Bind(imni.Table.Properties[a.Key], GetIncludeManyNewInitExpression(a.Value))));
                var pgarrayToManys = imni.Table.GetAllTableRef().Select(tr =>
                {
                    if (tr.Value.RefType != TableRefType.PgArrayToMany) return null;
                    var reftb = _orm.CodeFirst.GetTableByEntity(tr.Value.RefEntityType);
                    if (tr.Value.RefColumns[0] == reftb.Primarys[0])
                    {
                        bindings.Add(Expression.Bind(imni.Table.Properties[tr.Value.Columns[0].CsName], Expression.MakeMemberAccess(imni.CurrentExpression, imni.Table.Properties[tr.Value.Columns[0].CsName])));
                        return tr.Key;
                    }
                    return null;
                }).ToList();
                return Expression.MemberInit(imni.Table.Type.InternalNewExpression(), bindings);
            }

            var otherNewInit = GetIncludeManyNewInitExpression(incNewInit); //获取 IncludeMany 包含的最简化字段
            if (otherNewInit.Bindings.Any() == false) return this.ToListMapReaderPrivate<TReturn>(af, null);

            var otherMap = new ReadAnonymousTypeInfo();
            field.Clear();
            _commonExpression.ReadAnonymousField(_tables, _tableRule, field, otherMap, ref index, otherNewInit, this, _diymemexpWithTempQuery, _whereGlobalFilter, null, null, true);
            var otherRet = new List<object>();
            var otherAf = new ReadAnonymousTypeOtherInfo(field.ToString(), otherMap, otherRet);

            af.fillIncludeMany = new List<NativeTuple<string, IList, int>>();
            var ret = this.ToListMapReaderPrivate<TReturn>(af, new[] { otherAf });
            this.SetList(otherRet.Select(a => (T1)a).ToList()); //级联加载

            foreach (var fim in af.fillIncludeMany)
            {
                var splitKeys = fim.Item1.Split('.');
                var otherRetItem = otherRet[fim.Item3];
                var otherRetItemType = _tables[0].Table.Type;
                foreach (var splitKey in splitKeys)
                {
                    otherRetItem = _orm.GetEntityValueWithPropertyName(otherRetItemType, otherRetItem, splitKey);
                    otherRetItemType = _orm.CodeFirst.GetTableByEntity(otherRetItemType).Properties[splitKey].PropertyType;
                }
                if (otherRetItem == null) continue;
                var otherList = otherRetItem as IEnumerable;
                foreach (var otherListItem in otherList) fim.Item2.Add(otherListItem);
            }
            return ret;
        }
        public List<TDto> ToList<TDto>()
        {
            if (typeof(T1) == typeof(TDto)) return ToList() as List<TDto>;
            if (_tables.FirstOrDefault()?.Table.Type == typeof(object)) return ToList<TDto>("*");
            return ToList(GetToListDtoSelector<TDto>());
        }
        Expression<Func<T1, TDto>> GetToListDtoSelector<TDto>()
        {
            var expParam = _tables[0].Parameter ?? Expression.Parameter(typeof(T1), "a");
            var expBinds = _tables[0].Table.Columns
                .Where(a => a.Value.CsType != a.Value.Attribute.MapType)
                .Select(a => new { DtoProperty = typeof(TDto).GetProperty(a.Value.CsName), EntityProperty = _tables[0].Table.Properties[a.Value.CsName], Column = a.Value })
                .Where(a => a.DtoProperty != null)
                .Select(a =>
                    a.DtoProperty.PropertyType == a.EntityProperty.PropertyType ?
                    Expression.Bind(a.DtoProperty, Expression.MakeMemberAccess(expParam, a.EntityProperty)) :
                    Expression.Bind(a.DtoProperty, Expression.Convert(Expression.MakeMemberAccess(expParam, a.EntityProperty), a.DtoProperty.PropertyType))
                )
                .ToArray();
            return Expression.Lambda<Func<T1, TDto>>(
                Expression.MemberInit(typeof(TDto).InternalNewExpression(), expBinds),
                expParam);
        }
        public void ToChunk<TReturn>(Expression<Func<T1, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            if (select == null || done == null) return;
            _tables[0].Parameter = select.Parameters[0];
            this.InternalToChunk<TReturn>(select.Body, size, done);
        }

        public DataTable ToDataTable<TReturn>(Expression<Func<T1, TReturn>> select)
        {
            if (select == null) return this.InternalToDataTable(select?.Body);
            _tables[0].Parameter = select.Parameters[0];
            return this.InternalToDataTable(select?.Body);
        }

        public string ToSql<TReturn>(Expression<Func<T1, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex)
        {
            if (select == null) return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
            _tables[0].Parameter = select.Parameters[0];
            return this.InternalToSql<TReturn>(select?.Body, fieldAlias);
        }

        public TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, TReturn>> select)
        {
            if (select == null) return default(TReturn);
            _tables[0].Parameter = select.Parameters[0];
            return this.InternalToAggregate<TReturn>(select?.Body);
        }
        public ISelect<T1> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, TReturn>> select, out TReturn result)
        {
            result = ToAggregate(select);
            return this;
        }

        public ISelect<T1> Where(Expression<Func<T1, bool>> exp) => WhereIf(true, exp);
        public ISelect<T1> WhereIf(bool condition, Expression<Func<T1, bool>> exp)
        {
            if (condition == false || exp == null) return this;
            _tables[0].Parameter = exp.Parameters[0];
            return this.InternalWhere(exp?.Body);
        }

        public ISelect<T1> Where<T2>(Expression<Func<T1, T2, bool>> exp) where T2 : class
        {
            if (exp == null) return this;
            _tables[0].Parameter = exp.Parameters[0];
            return this.InternalWhere(exp?.Body);
        }

        public ISelect<T1> Where<T2>(Expression<Func<T2, bool>> exp) where T2 : class
        {
            if (exp == null) return this;
            //_tables[0].Parameter = exp.Parameters[0];
            return this.InternalWhere(exp?.Body);
        }

        public ISelect<T1> Where<T2, T3>(Expression<Func<T1, T2, T3, bool>> exp) where T2 : class where T3 : class
        {
            if (exp == null) return this;
            _tables[0].Parameter = exp.Parameters[0];
            return this.InternalWhere(exp?.Body);
        }

        public ISelect<T1> Where<T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> exp) where T2 : class where T3 : class where T4 : class
        {
            if (exp == null) return this;
            _tables[0].Parameter = exp.Parameters[0];
            return this.InternalWhere(exp?.Body);
        }

        public ISelect<T1> Where<T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> exp) where T2 : class where T3 : class where T4 : class where T5 : class
        {
            if (exp == null) return this;
            _tables[0].Parameter = exp.Parameters[0];
            return this.InternalWhere(exp?.Body);
        }
        public ISelect<T1> WhereDynamic(object dywhere, bool not = false)
        {
            if (dywhere is DynamicFilterInfo dyfilter)
            {
                if (not == false) return this.WhereDynamicFilter(dyfilter);

                var oldwhere = _where.ToString();
                _where.Clear();

                this.WhereDynamicFilter(dyfilter);
                var newwhere = _where.ToString();
                _where.Clear();

                return this
                    .Where(oldwhere)
                    .WhereIf(string.IsNullOrWhiteSpace(newwhere) == false, $"not({newwhere})");
            }
            var wheresql = _commonUtils.WhereObject(_tables.First().Table, $"{_tables.First().Alias}.", dywhere);
            return not == false ? this.Where(wheresql) : this.Where($"not({wheresql})");
        }

        public ISelect<T1> WhereCascade(Expression<Func<T1, bool>> exp)
        {
            if (exp != null) _whereGlobalFilter.Add(new GlobalFilter.Item { Name = "WhereCascade", Only = false, Where = exp });
            return this;
        }

        public ISelect<T1> WithSql(string sql, object parms = null)
        {
            this.AsTable((type, old) =>
            {
                if (type == _tables[0].Table?.Type && string.IsNullOrEmpty(sql) == false) return $"( {sql} )";
                return old;
            });
            if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
            return this;
        }
        public ISelect<T1> WithMemory(IEnumerable<T1> source)
        {
            var list = source?.Select(a => (object)a).ToList();
            if (list.Any() != true) throw new Exception(CoreStrings.Cannot_Be_NULL_Name(nameof(source)));
			var sb = new StringBuilder();
            (_orm.InsertOrUpdate<object>().AsType(_tables[0].Table.Type) as InsertOrUpdateProvider<object>)
                .WriteSourceSelectUnionAll(list, sb, _params, true);

            try { return WithSql(sb.ToString()); }
            finally { sb.Clear(); }
        }

        public ISelect<TDto> WithTempQuery<TDto>(Expression<Func<T1, TDto>> selector) => InternalWithTempQuery<TDto>(selector);

        public bool Any(Expression<Func<T1, bool>> exp)
        {
            var oldwhere = _where.ToString();
            var ret = this.Where(exp).Any();
            _where.Clear().Append(oldwhere);
            return ret;
        }

        public TReturn ToOne<TReturn>(Expression<Func<T1, TReturn>> select) => this.Limit(1).ToList(select).FirstOrDefault();
        public TDto ToOne<TDto>() => this.Limit(1).ToList<TDto>().FirstOrDefault();

        public TReturn First<TReturn>(Expression<Func<T1, TReturn>> select) => this.ToOne(select);
        public TDto First<TDto>() => this.ToOne<TDto>();

        public override List<T1> ToList(bool includeNestedMembers = false) => base.ToList(_isIncluded || includeNestedMembers);

        public int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, TTargetEntity>> select) where TTargetEntity : class => base.InternalInsertInto<TTargetEntity>(tableName, select);

        public ISelect<T1> IncludeByPropertyNameIf(bool condition, string property) => condition ? IncludeByPropertyName(property, null) : this;
        public ISelect<T1> IncludeByPropertyNameIf(bool condition, string property, Expression<Action<ISelect<object>>> then) => condition ? IncludeByPropertyName(property, then) : this;
        public ISelect<T1> IncludeByPropertyName(string property) => IncludeByPropertyName(property, null);
        public ISelect<T1> IncludeByPropertyName(string property, Expression<Action<ISelect<object>>> then)
        {
            var exp = ConvertStringPropertyToExpression(property, true);
            if (exp == null) throw new ArgumentException($"{CoreStrings.Cannot_Resolve_ExpressionTree(nameof(property))}");
            var memExp = exp as MemberExpression;
            if (memExp == null) throw new ArgumentException($"{CoreStrings.Cannot_Resolve_ExpressionTree(nameof(property))}2");
            var parTb = _commonUtils.GetTableByEntity(memExp.Expression.Type);
            if (parTb == null) throw new ArgumentException($"{CoreStrings.Cannot_Resolve_ExpressionTree(nameof(property))}3");
            var parTbref = parTb.GetTableRef(memExp.Member.Name, true, true);
            if (parTbref == null) throw new ArgumentException(CoreStrings.Not_Valid_Navigation_Property(nameof(property)));
            switch (parTbref.RefType)
            {
                case TableRefType.ManyToMany:
                case TableRefType.OneToMany:
                case TableRefType.PgArrayToMany:
                    var funcType = typeof(Func<,>).MakeGenericType(typeof(T1), typeof(IEnumerable<>).MakeGenericType(parTbref.RefEntityType));
                    if (_tables[0].Table.Type != typeof(T1))
                    {
                        var expParm = Expression.Parameter(typeof(T1), _tables[0].Alias);
                        exp = new ReplaceMemberExpressionVisitor().Replace(exp, _tables[0].Parameter, Expression.Convert(expParm, _tables[0].Table.Type));
                        _tables[0].Parameter = expParm;
                    }
                    var navigateSelector = Expression.Lambda(funcType, exp, _tables[0].Parameter);
                    var incMethod = this.GetType().GetMethod("IncludeMany");
                    if (incMethod == null) throw new Exception(CoreStrings.RunTimeError_Reflection_IncludeMany);
                    Delegate newthen = null;
                    if (then != null)
                    {
                        var newthenParm = Expression.Parameter(typeof(ISelect<>).MakeGenericType(parTbref.RefEntityType));
                        var newthenLambdaBody = new ReplaceIncludeByPropertyNameParameterVisitor().Modify(then, newthenParm);
                        var newthenLambda = Expression.Lambda(typeof(Action<>).MakeGenericType(newthenParm.Type), newthenLambdaBody, newthenParm);
                        newthen = newthenLambda.Compile();
                    }
                    incMethod.MakeGenericMethod(parTbref.RefEntityType).Invoke(this, new object[] { navigateSelector, newthen });
                    break;
                case TableRefType.ManyToOne:
                case TableRefType.OneToOne:
                    _isIncluded = true;
                    var curTb = _commonUtils.GetTableByEntity(exp.Type);
                    _commonExpression.ExpressionWhereLambda(_tables, _tableRule, Expression.MakeMemberAccess(exp, curTb.Properties[curTb.ColumnsByCs.First().Value.CsName]), _diymemexpWithTempQuery, null, null);
                    break;
            }
            return this;
        }
        public class ReplaceIncludeByPropertyNameParameterVisitor : ExpressionVisitor
        {
            private Expression _replaceExp;
            private ParameterExpression oldParameter;
            public Expression Modify(LambdaExpression lambda, Expression replaceExp)
            {
                this._replaceExp = replaceExp;
                this.oldParameter = lambda.Parameters.FirstOrDefault();
                return Visit(lambda.Body);
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression?.NodeType == ExpressionType.Parameter && node.Expression == oldParameter)
                    return Expression.Property(_replaceExp, node.Member.Name);
                return base.VisitMember(node);
            }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Object?.Type == oldParameter.Type)
                {
                    var methodParameterTypes = node.Method.GetParameters().Select(a => a.ParameterType).ToArray();
                    var method = _replaceExp.Type.GetMethod(node.Method.Name, methodParameterTypes);
                    if (method == null && _replaceExp.Type.IsInterface)
                    {
                        foreach (var baseInterface in _replaceExp.Type.GetInterfaces())
                        {
                            method = baseInterface.GetMethod(node.Method.Name, methodParameterTypes);
                            if (method != null) break;
                        }
                    }
                    if (node.Object?.NodeType == ExpressionType.Parameter && node.Object == oldParameter)
                        return Expression.Call(_replaceExp, method, node.Arguments);
                    return Expression.Call(Visit(node.Object), method, node.Arguments);
                }
                return base.VisitMethodCall(node);
            }
        }

        bool _isIncluded = false;
        public ISelect<T1> IncludeIf<TNavigate>(bool condition, Expression<Func<T1, TNavigate>> navigateSelector) where TNavigate : class => condition ? Include(navigateSelector) : this;
        public ISelect<T1> Include<TNavigate>(Expression<Func<T1, TNavigate>> navigateSelector) where TNavigate : class
        {
            var expBody = navigateSelector?.Body;
            if (expBody == null) return this;
            if (expBody.NodeType != ExpressionType.MemberAccess) throw new Exception(CoreStrings.Include_ParameterType_Error_Use_MemberAccess);
            if (typeof(IEnumerable).IsAssignableFrom(expBody.Type)) throw new Exception(CoreStrings.Include_ParameterType_Error_Use_IncludeMany);
            var tb = _commonUtils.GetTableByEntity(expBody.Type);
            if (tb == null) throw new Exception(CoreStrings.Include_ParameterType_Error);

            _isIncluded = true;
            _tables[0].Parameter = navigateSelector.Parameters[0];
            _commonExpression.ExpressionWhereLambda(_tables, _tableRule, Expression.MakeMemberAccess(expBody, tb.Properties[tb.ColumnsByCs.First().Value.CsName]), _diymemexpWithTempQuery, null, null);
            return this;
        }

        static NativeTuple<ParameterExpression, List<MemberExpression>> GetExpressionStack(Expression exp)
        {
            Expression tmpExp = exp;
            ParameterExpression param = null;
            var members = new Stack<MemberExpression>();
            var isbreak = false;
            while (isbreak == false)
            {
                switch (tmpExp.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        var memExp = tmpExp as MemberExpression;
                        tmpExp = memExp.Expression;
                        members.Push(memExp);
                        continue;
                    case ExpressionType.Parameter:
                        param = tmpExp as ParameterExpression;
                        isbreak = true;
                        break;
                    case ExpressionType.Convert:
                        var convertExp = tmpExp as UnaryExpression;
                        if (convertExp?.Operand.NodeType == ExpressionType.Parameter)
                        {
                            param = convertExp.Operand as ParameterExpression;
                            isbreak = true;
                            break;
                        }
                        throw new Exception(CoreStrings.Expression_Error_Use_Successive_MemberAccess_Type(exp));
                    default:
                        throw new Exception(CoreStrings.Expression_Error_Use_Successive_MemberAccess_Type(exp));
                }
            }
            if (param == null) throw new Exception(CoreStrings.Expression_Error_Use_ParameterExpression(exp));
            return NativeTuple.Create(param, members.ToList());
        }
        static MethodInfo GetEntityValueWithPropertyNameMethod = typeof(EntityUtilExtensions).GetMethod("GetEntityValueWithPropertyName");
        static ConcurrentDictionary<Type, ConcurrentDictionary<string, MethodInfo>> _dicTypeMethod = Utils.GlobalCacheFactory.CreateCacheItem(new ConcurrentDictionary<Type, ConcurrentDictionary<string, MethodInfo>>());
        public ISelect<T1> IncludeMany<TNavigate>(Expression<Func<T1, IEnumerable<TNavigate>>> navigateSelector, Action<ISelect<TNavigate>> then = null) where TNavigate : class
        {
            var throwNavigateSelector = new Exception(CoreStrings.IncludeMany_ParameterType_Error_Use_MemberAccess);
            
            var expBody = navigateSelector?.Body;
            if (expBody == null) return this;
            if (expBody.NodeType == ExpressionType.Convert) expBody = (expBody as UnaryExpression)?.Operand; //- 兼容 Vb.Net 无法使用 IncludeMany 的问题；
            MethodCallExpression whereExp = null;
            int takeNumber = 0;
            Expression<Func<TNavigate, TNavigate>> selectExp = null;
            while (expBody.NodeType == ExpressionType.Call)
            {
                throwNavigateSelector = new Exception(CoreStrings.IncludeMany_ParameterTypeError(nameof(navigateSelector)));
                var callExp = (expBody as MethodCallExpression);
                switch (callExp.Method.Name)
                {
                    case "Where":
                        whereExp = callExp;
                        break;
                    case "Take":
                        takeNumber = int.Parse(_commonExpression.ExpressionLambdaToSql(callExp.Arguments[1], new CommonExpression.ExpTSC { }));
                        break;
                    case "Select":
                        selectExp = (callExp.Arguments[1] as Expression<Func<TNavigate, TNavigate>>);
                        if (selectExp?.Parameters.Count != 1) throw new Exception(CoreStrings.IncludeMany_ParameterError_OnlyUseOneParameter(nameof(navigateSelector)));
                        break;
                    default: throw throwNavigateSelector;
                }
                expBody = callExp.Object ?? callExp.Arguments.FirstOrDefault();
            }

            if (expBody.NodeType != ExpressionType.MemberAccess) throw throwNavigateSelector;
            var collMem = expBody as MemberExpression;
            var ges = GetExpressionStack(collMem.Expression);
            var membersParam = ges.Item1;
            var members = ges.Item2;
            var tb = _commonUtils.GetTableByEntity(collMem.Expression.Type);
            if (tb == null) throw throwNavigateSelector;
            var collMemElementType = (collMem.Type.IsGenericType ? collMem.Type.GetGenericArguments().FirstOrDefault() : collMem.Type.GetElementType());
            if (typeof(TNavigate) != collMemElementType)
                throw new Exception(CoreStrings.IncludeMany_ParameterError_Select_ReturnConsistentType(nameof(navigateSelector), collMemElementType));
            var tbNav = _commonUtils.GetTableByEntity(typeof(TNavigate));
            if (tbNav == null) throw new Exception(CoreStrings.TypeError_CannotUse_IncludeMany(typeof(TNavigate).FullName));

            if (collMem.Expression.NodeType != ExpressionType.Parameter)
                _commonExpression.ExpressionWhereLambda(_tables, _tableRule, Expression.MakeMemberAccess(collMem.Expression, tb.Properties[tb.ColumnsByCs.First().Value.CsName]), _diymemexpWithTempQuery, null, null);

            TableRef tbref = null;
            var tbrefOneToManyColumns = new List<List<MemberExpression>>(); //临时 OneToMany 三个表关联，第三个表需要前两个表确定
            if (whereExp == null)
            {
                tbref = tb.GetTableRef(collMem.Member.Name, true, true);
                if (tbref == null) throw new Exception(CoreStrings.IncludeMany_NotValid_Navigation(tb.Type.DisplayCsharp(), collMem.Member.Name));
            }
            else
            {
                //处理临时关系映射
                tbref = new TableRef
                {
                    RefType = TableRefType.OneToMany,
                    Property = tb.Properties[collMem.Member.Name],
                    RefEntityType = tbNav.Type
                };
                foreach (var whereExpArg in whereExp.Arguments)
                {
                    if (whereExpArg.NodeType != ExpressionType.Lambda) continue;
                    var whereExpArgLamb = whereExpArg as LambdaExpression;

                    Action<Expression> actWeiParse = null;
                    actWeiParse = expOrg =>
                    {
                        var binaryExp = expOrg as BinaryExpression;
                        if (binaryExp == null) throw throwNavigateSelector;

                        switch (binaryExp.NodeType)
                        {
                            case ExpressionType.AndAlso:
                                actWeiParse(binaryExp.Left);
                                actWeiParse(binaryExp.Right);
                                break;
                            case ExpressionType.Equal:
                                Expression leftExp = binaryExp.Left;
                                Expression rightExp = binaryExp.Right;
                                while (leftExp.NodeType == ExpressionType.Convert) leftExp = (leftExp as UnaryExpression)?.Operand;
                                while (rightExp.NodeType == ExpressionType.Convert) rightExp = (rightExp as UnaryExpression)?.Operand;
                                var leftP1MemberExp = leftExp as MemberExpression;
                                var rightP1MemberExp = rightExp as MemberExpression;
                                if (leftP1MemberExp == null || rightP1MemberExp == null) throw throwNavigateSelector;

                                if (leftP1MemberExp.Expression == whereExpArgLamb.Parameters[0])
                                {
                                    var rightGes = GetExpressionStack(rightP1MemberExp.Expression);
                                    var rightMembersParam = rightGes.Item1;
                                    var rightMembers = rightGes.Item2;
                                    if (rightMembersParam != membersParam) throw throwNavigateSelector;
                                    var isCollMemEquals = rightMembers.Count == members.Count;
                                    if (isCollMemEquals)
                                    {
                                        for (var l = 0; l < members.Count; l++)
                                            if (members[l].Member != rightMembers[l].Member)
                                            {
                                                isCollMemEquals = false;
                                                break;
                                            }
                                    }
                                    if (isCollMemEquals)
                                    {
                                        tbref.Columns.Add(tb.ColumnsByCs[rightP1MemberExp.Member.Name]);
                                        tbrefOneToManyColumns.Add(null);
                                    }
                                    else
                                    {
                                        var tmpTb = _commonUtils.GetTableByEntity(rightP1MemberExp.Expression.Type);
                                        if (tmpTb == null) throw throwNavigateSelector;
                                        tbref.Columns.Add(tmpTb.ColumnsByCs[rightP1MemberExp.Member.Name]);
                                        tbrefOneToManyColumns.Add(rightMembers);
                                    }
                                    tbref.RefColumns.Add(tbNav.ColumnsByCs[leftP1MemberExp.Member.Name]);
                                    return;
                                }
                                if (rightP1MemberExp.Expression == whereExpArgLamb.Parameters[0])
                                {
                                    var leftGes = GetExpressionStack(leftP1MemberExp.Expression);
                                    var leftMembersParam = leftGes.Item1;
                                    var leftMembers = leftGes.Item2;
                                    if (leftMembersParam != membersParam) throw throwNavigateSelector;
                                    var isCollMemEquals = leftMembers.Count == members.Count;
                                    if (isCollMemEquals)
                                    {
                                        for (var l = 0; l < members.Count; l++)
                                            if (members[l].Member != leftMembers[l].Member)
                                            {
                                                isCollMemEquals = false;
                                                break;
                                            }
                                    }
                                    if (isCollMemEquals)
                                    {
                                        tbref.Columns.Add(tb.ColumnsByCs[leftP1MemberExp.Member.Name]);
                                        tbrefOneToManyColumns.Add(null);
                                    }
                                    else
                                    {
                                        var tmpTb = _commonUtils.GetTableByEntity(leftP1MemberExp.Expression.Type);
                                        if (tmpTb == null) throw throwNavigateSelector;
                                        tbref.Columns.Add(tmpTb.ColumnsByCs[leftP1MemberExp.Member.Name]);
                                        tbrefOneToManyColumns.Add(leftMembers);
                                    }
                                    tbref.RefColumns.Add(tbNav.ColumnsByCs[rightP1MemberExp.Member.Name]);
                                    return;
                                }

                                throw throwNavigateSelector;
                            default: throw throwNavigateSelector;
                        }
                    };
                    actWeiParse(whereExpArgLamb.Body);
                    break;
                }
                if (tbref.Columns.Any() == false) throw throwNavigateSelector;
            }

            if (members.Count >= 2) _isIncluded = true;
#if net40
            Action<object, bool> includeToListSyncOrAsync = (listObj, isAsync) =>
            {
                isAsync = false;
#else
            Func<object, bool, CancellationToken, Task> includeToListSyncOrAsync = async (listObj, isAsync, cancellationToken) =>
            {
#endif

                var list = listObj as List<T1>;
                if (list == null)
                {
                    if (typeof(T1) == typeof(object))
                    {
                        var ilist = listObj as IList;
                        if (ilist != null)
                        {
                            list = new List<T1>();
                            foreach (var iitem in ilist)
                                list.Add((T1)iitem);
                        }
                    }
                    if (list == null) return;
                }
                if (list.Any() == false) return;
                if (tbref.Columns.Any() == false) return;

                var t1parm = Expression.Parameter(typeof(T1));
                Expression membersExp = t1parm;
                Expression membersExpNotNull = null;
                if (typeof(T1) != _tables[0].Table.Type) membersExp = Expression.TypeAs(membersExp, _tables[0].Table.Type);
                foreach (var mem in members)
                {
                    membersExp = Expression.MakeMemberAccess(membersExp, mem.Member);
                    var expNotNull = Expression.NotEqual(membersExp, Expression.Constant(null));
                    if (membersExpNotNull == null) membersExpNotNull = expNotNull;
                    else membersExpNotNull = Expression.AndAlso(membersExpNotNull, expNotNull);
                }
                //members.Clear(); 此行影响 ToChunk 第二次

                var isObservableCollection = collMem.Type == typeof(ObservableCollection<TNavigate>);
                var listValueExp = Expression.Parameter(typeof(List<TNavigate>), "listValue");
                var setListValue = membersExpNotNull == null ?
                    Expression.Lambda<Action<T1, List<TNavigate>>>(
                        isObservableCollection ?
                        (Expression)Expression.IfThen(
                            Expression.NotEqual(listValueExp, Expression.Constant(null, typeof(List<TNavigate>))),
                            Expression.Assign(Expression.MakeMemberAccess(membersExp, collMem.Member), Expression.New(typeof(ObservableCollection<TNavigate>).GetConstructor(new[] { typeof(List<TNavigate>) }), listValueExp))
                        ) :
                        Expression.Assign(Expression.MakeMemberAccess(membersExp, collMem.Member), Expression.TypeAs(listValueExp, collMem.Type))
                        , t1parm, listValueExp).Compile() :
                    Expression.Lambda<Action<T1, List<TNavigate>>>(Expression.IfThen(membersExpNotNull,
                        isObservableCollection ?
                        (Expression)Expression.IfThen(
                            Expression.NotEqual(listValueExp, Expression.Constant(null, typeof(List<TNavigate>))),
                            Expression.Assign(Expression.MakeMemberAccess(membersExp, collMem.Member), Expression.New(typeof(ObservableCollection<TNavigate>).GetConstructor(new[] { typeof(List<TNavigate>) }), listValueExp))
                        ) :
                        Expression.Assign(Expression.MakeMemberAccess(membersExp, collMem.Member), Expression.TypeAs(listValueExp, collMem.Type))
                        ), t1parm, listValueExp).Compile();

                var returnTarget = Expression.Label(typeof(object));
                var propertyNameExp = Expression.Parameter(typeof(string), "propertyName");
                var getListValue1 = membersExpNotNull == null ?
                    Expression.Lambda<Func<T1, string, object>>(
                        Expression.Block(
                            Expression.IfThenElse(
                                Expression.Equal(propertyNameExp, Expression.Constant("")), //propertyName == "" 返回自身
                                Expression.Return(returnTarget, membersExp),
                                Expression.Return(returnTarget, Expression.Call(null, GetEntityValueWithPropertyNameMethod, Expression.Constant(_orm), Expression.Constant(membersExp.Type), membersExp, propertyNameExp))
                            ),
                            Expression.Label(returnTarget, Expression.Default(typeof(object)))
                        ), t1parm, propertyNameExp).Compile() :
                    Expression.Lambda<Func<T1, string, object>>(
                        Expression.Block(
                            Expression.IfThen(
                                membersExpNotNull,
                                Expression.IfThenElse(
                                    Expression.Equal(propertyNameExp, Expression.Constant("")),
                                    Expression.Return(returnTarget, membersExp),
                                    Expression.Return(returnTarget, Expression.Call(null, GetEntityValueWithPropertyNameMethod, Expression.Constant(_orm), Expression.Constant(membersExp.Type), membersExp, propertyNameExp))
                                )
                            ),
                            Expression.Label(returnTarget, Expression.Default(typeof(object)))
                        ), t1parm, propertyNameExp).Compile();

                var getListValue2 = new List<Func<T1, string, object>>();
                for (var j = 0; j < tbrefOneToManyColumns.Count; j++)
                {
                    if (tbrefOneToManyColumns[j] == null)
                    {
                        getListValue2.Add(null);
                        continue;
                    }
                    Expression tbrefOneToManyColumnsMembers = t1parm;
                    Expression tbrefOneToManyColumnsMembersNotNull = null;
                    foreach (var mem in tbrefOneToManyColumns[j])
                    {
                        tbrefOneToManyColumnsMembers = Expression.MakeMemberAccess(tbrefOneToManyColumnsMembers, mem.Member);
                        var expNotNull = Expression.NotEqual(membersExp, Expression.Constant(null));
                        if (tbrefOneToManyColumnsMembersNotNull == null) tbrefOneToManyColumnsMembersNotNull = expNotNull;
                        else tbrefOneToManyColumnsMembersNotNull = Expression.AndAlso(tbrefOneToManyColumnsMembersNotNull, expNotNull);
                    }
                    tbrefOneToManyColumns[j].Clear();
                    getListValue2.Add(tbrefOneToManyColumnsMembersNotNull == null ?
                        Expression.Lambda<Func<T1, string, object>>(
                            Expression.Block(
                                Expression.Return(returnTarget, Expression.Call(null, GetEntityValueWithPropertyNameMethod, Expression.Constant(_orm), Expression.Constant(tbrefOneToManyColumnsMembers.Type), tbrefOneToManyColumnsMembers, propertyNameExp)),
                                Expression.Label(returnTarget, Expression.Default(typeof(object)))
                            ), t1parm, propertyNameExp).Compile() :
                        Expression.Lambda<Func<T1, string, object>>(
                            Expression.Block(
                                Expression.IfThen(
                                    tbrefOneToManyColumnsMembersNotNull,
                                    Expression.Return(returnTarget, Expression.Call(null, GetEntityValueWithPropertyNameMethod, Expression.Constant(_orm), Expression.Constant(tbrefOneToManyColumnsMembers.Type), tbrefOneToManyColumnsMembers, propertyNameExp))
                                ),
                                Expression.Label(returnTarget, Expression.Default(typeof(object)))
                            ), t1parm, propertyNameExp).Compile());
                }
                tbrefOneToManyColumns.Clear();
                Func<T1, string, int, object> getListValue = (item, propName, colIndex) =>
                {
                    if (getListValue2.Any() && getListValue2[colIndex] != null) return getListValue2[colIndex](item, propName);
                    return getListValue1(item, propName);
                };

                if (list.Where(a => getListValue1(a, "") == null).Count() == list.Count) return; //Parent.Childs 当 parent 都是 NULL 就没有和要向下查询了
                foreach (var item in list)
                    setListValue(item, null);

                Action<List<TNavigate>, TableInfo> fillOneToManyData = (subList, tbref2) =>
                {
                    if (subList.Any() == false)
                    {
                        foreach (var item in list)
                            setListValue(item, new List<TNavigate>());
                        return;
                    }

                    Dictionary<string, List<Tuple<T1, List<TNavigate>>>> dicList = new Dictionary<string, List<Tuple<T1, List<TNavigate>>>>();
                    foreach (var item in list)
                    {
                        if (tbref.Columns.Count == 1)
                        {
                            var dicListKey = getListValue(item, tbref.Columns[0].CsName, 0)?.ToString();
                            if (dicListKey == null) continue;
                            var dicListVal = Tuple.Create(item, new List<TNavigate>());
                            if (dicList.TryGetValue(dicListKey, out var items) == false)
                                dicList.Add(dicListKey, items = new List<Tuple<T1, List<TNavigate>>>());
                            items.Add(dicListVal);
                        }
                        else
                        {
                            var sb = new StringBuilder();
                            for (var z = 0; z < tbref.Columns.Count; z++)
                            {
                                if (z > 0) sb.Append("*$*");
                                sb.Append(getListValue(item, tbref.Columns[z].CsName, z));
                            }
                            var dicListKey = sb.ToString();
                            var dicListVal = Tuple.Create(item, new List<TNavigate>());
                            if (dicList.TryGetValue(dicListKey, out var items) == false)
                                dicList.Add(dicListKey, items = new List<Tuple<T1, List<TNavigate>>>());
                            items.Add(dicListVal);
                            sb.Clear();
                        }
                    }
                    var parentNavs = new List<string>();
                    foreach (var tr2 in tbref2.GetAllTableRef())
                    {
                        var tr2ref = tr2.Value;
                        if (tr2ref.Exception != null) continue;
                        if (tbref2.ColumnsByCs.ContainsKey(tr2.Key)) continue;
                        if (tbref2.ColumnsByCsIgnore.ContainsKey(tr2.Key)) continue;
                        if (tr2ref.RefType != TableRefType.ManyToOne) continue;
                        if (tr2ref.RefEntityType != tb.Type) continue;
                        if (string.Join(",", tr2ref.Columns.Select(a => a.CsName).OrderBy(a => a)) != string.Join(",", tbref.RefColumns.Select(a => a.CsName).OrderBy(a => a))) continue; //- 修复 IncludeMany 只填充子属性中双向关系的 ManyToOne 对象值；防止把 ManyToOne 多个相同类型的导航属性值都填充了
                        parentNavs.Add(tr2.Key);
                    }
                    foreach (var nav in subList)
                    {
                        string key = null;
                        if (tbref.RefColumns.Count == 1)
                        {
                            key = _orm.GetEntityValueWithPropertyName(tbref.RefEntityType, nav, tbref.RefColumns[0].CsName)?.ToString() ?? "";
                        }
                        else
                        {
                            var sb = new StringBuilder();
                            for (var z = 0; z < tbref.RefColumns.Count; z++)
                            {
                                if (z > 0) sb.Append("*$*");
                                sb.Append(_orm.GetEntityValueWithPropertyName(tbref.RefEntityType, nav, tbref.RefColumns[z].CsName));
                            }
                            key = sb.ToString();
                            sb.Clear();
                        }
                        if (dicList.TryGetValue(key, out var t1items) == false) continue;
                        foreach (var t1item in t1items)
                            t1item.Item2.Add(nav);

                        //将子集合的，多对一，对象设置为当前对象
                        foreach (var parentNav in parentNavs)
                            foreach (var t1item in t1items)
                                _orm.SetEntityValueWithPropertyName(tbref.RefEntityType, nav, parentNav, getListValue1(t1item.Item1, "")); //propertyName == "" 返回自身
                    }
                    foreach (var t1items in dicList.Values)
                        foreach (var t1item in t1items)
                            setListValue(t1item.Item1, t1item.Item2);
                    dicList.Clear();
                };

                if (tbref.RefType == TableRefType.OneToMany && _includeManySubListOneToManyTempValue1 != null && _includeManySubListOneToManyTempValue1 is List<TNavigate>)
                {
                    fillOneToManyData(_includeManySubListOneToManyTempValue1 as List<TNavigate>, _commonUtils.GetTableByEntity(tbref.RefEntityType));
                    return;
                }

                var subSelect = _orm.Select<TNavigate>()
                    .DisableGlobalFilter()
                    .WithConnection(_connection)
                    .WithTransaction(_transaction)
                    .TrackToList(_trackToList) as Select1Provider<TNavigate>;
                if (_tableRules?.Any() == true)
                    foreach (var tr in _tableRules) subSelect.AsTable(tr);

                if (_whereGlobalFilter.Any())
                    subSelect._whereGlobalFilter.AddRange(_whereGlobalFilter.ToArray());

                //subSelect._aliasRule = _aliasRule; //把 SqlServer 查询锁传递下去
                then?.Invoke(subSelect);
                var subSelectT1Alias = subSelect._tables[0].Alias;
                var oldWhere = subSelect._where.ToString();
                if (oldWhere.StartsWith(" AND ")) oldWhere = oldWhere.Remove(0, 5);

                if (selectExp != null)
                {
                    var tmpinitExp = selectExp.Body as MemberInitExpression;
                    var newinitExpBindings = tmpinitExp.Bindings.ToList();
                    foreach (var tbrefCol in tbref.RefColumns)
                    {
                        if (newinitExpBindings.Any(a => a.Member.Name == tbrefCol.CsName)) continue;
                        var tmpMemberInfo = tbrefCol.Table.Properties[tbrefCol.CsName];
                        newinitExpBindings.Add(Expression.Bind(tmpMemberInfo, Expression.MakeMemberAccess(selectExp.Parameters[0], tmpMemberInfo)));
                    }
                    if (subSelect._includeToList.Any()) //如果还有向下 IncludeMany，要把它的主键也查出来
                    {
                        foreach (var tbrefPkCol in _commonUtils.GetTableByEntity(tbref.RefEntityType).Primarys)
                        {
                            if (newinitExpBindings.Any(a => a.Member.Name == tbrefPkCol.CsName)) continue;
                            var tmpMemberInfo = tbrefPkCol.Table.Properties[tbrefPkCol.CsName];
                            newinitExpBindings.Add(Expression.Bind(tmpMemberInfo, Expression.MakeMemberAccess(selectExp.Parameters[0], tmpMemberInfo)));
                        }
                    }
                    Expression newinitExp = Expression.MemberInit(tmpinitExp.NewExpression, newinitExpBindings.ToList());
                    var selectExpParam = subSelect._tables[0].Parameter ?? Expression.Parameter(typeof(TNavigate), subSelectT1Alias);
                    newinitExp = new NewExpressionVisitor(selectExpParam, selectExp.Parameters[0]).Replace(newinitExp);
                    selectExp = Expression.Lambda<Func<TNavigate, TNavigate>>(newinitExp, selectExpParam);
                }

                switch (tbref.RefType)
                {
                    case TableRefType.OneToMany:
                        if (true)
                        {
                            var subList = new List<TNavigate>();
                            var tbref2 = _commonUtils.GetTableByEntity(tbref.RefEntityType);
                            Func<Dictionary<string, bool>> getWhereDic = () =>
                            {
                                var sbDic = new Dictionary<string, bool>();
                                for (var y = 0; y < list.Count; y++)
                                {
                                    var sbWhereOne = new StringBuilder();
                                    sbWhereOne.Append("(");
                                    for (var z = 0; z < tbref.Columns.Count; z++)
                                    {
                                        if (z > 0) sbWhereOne.Append(" AND ");
                                        sbWhereOne.Append(_commonUtils.FormatSql($"{subSelectT1Alias}.{_commonUtils.QuoteSqlName(tbref.RefColumns[z].Attribute.Name)}={{0}}", Utils.GetDataReaderValue(tbref.RefColumns[z].Attribute.MapType, getListValue(list[y], tbref.Columns[z].CsName, z))));
                                    }
                                    sbWhereOne.Append(")");
                                    var whereOne = sbWhereOne.ToString();
                                    sbWhereOne.Clear();
                                    if (sbDic.ContainsKey(whereOne) == false) sbDic.Add(whereOne, true);
                                }
                                return sbDic;
                            };
                            if (takeNumber > 0)
                            {
                                Select0Provider<ISelect<TNavigate>, TNavigate>.GetAllFieldExpressionTreeInfo af = null;
                                ReadAnonymousTypeAfInfo mf = default;
                                if (selectExp == null) af = subSelect.GetAllFieldExpressionTreeLevelAll();
                                else mf = subSelect.GetExpressionField(selectExp);
                                var sbSql = new StringBuilder();
                                var sbDic = getWhereDic();
                                foreach (var sbd in sbDic)
                                {
                                    subSelect._where.Clear();
                                    subSelect.Where(sbd.Key).Where(oldWhere).Limit(takeNumber);
                                    sbSql.Append("\r\nUNION ALL\r\nselect * from (").Append(subSelect.ToSql(selectExp == null ? af.Field : mf.field)).Append(") ftb");
                                }
                                sbSql.Remove(0, 13);
                                if (sbDic.Count == 1) sbSql.Remove(0, 15).Remove(sbSql.Length - 5, 5);
                                sbDic.Clear();

                                if (isAsync)
                                {
#if net40
#else
                                    if (selectExp == null) subList = await subSelect.ToListAfPrivateAsync(sbSql.ToString(), af, null, cancellationToken);
                                    else subList = await subSelect.ToListMrPrivateAsync<TNavigate>(sbSql.ToString(), mf, null, cancellationToken);
#endif
                                }
                                else
                                {
                                    if (selectExp == null) subList = subSelect.ToListAfPrivate(sbSql.ToString(), af, null);
                                    else subList = subSelect.ToListMrPrivate<TNavigate>(sbSql.ToString(), mf, null);
                                }
                                sbSql.Clear();
                            }
                            else
                            {
                                subSelect._where.Clear();
                                if (tbref.Columns.Count == 1)
                                {
                                    var arrExp = Expression.NewArrayInit(tbref.RefColumns[0].CsType,
                                        list.Select(a => getListValue(a, tbref.Columns[0].CsName, 0)).Distinct()
                                            .Select(a => Expression.Constant(Utils.GetDataReaderValue(tbref.RefColumns[0].CsType, a), tbref.RefColumns[0].CsType)).ToArray());
                                    var otmExpParm1 = Expression.Parameter(typeof(TNavigate), "a");
                                    var containsMethod = _dicTypeMethod.GetOrAdd(tbref.RefColumns[0].CsType, et => new ConcurrentDictionary<string, MethodInfo>()).GetOrAdd("Contains", mn =>
                                        typeof(Enumerable).GetMethods().Where(a => a.Name == mn).First()).MakeGenericMethod(tbref.RefColumns[0].CsType);
                                    var refCol = Expression.MakeMemberAccess(otmExpParm1, tbref2.Properties[tbref.RefColumns[0].CsName]);
                                    //if (refCol.Type.IsNullableType()) refCol = Expression.Property(refCol, CommonExpression._dicNullableValueProperty.GetOrAdd(refCol.Type, ct1 => ct1.GetProperty("Value")));
                                    subSelect.Where(Expression.Lambda<Func<TNavigate, bool>>(
                                        Expression.Call(null, containsMethod, arrExp, refCol), otmExpParm1));
                                }
                                else
                                {
                                    var sbDic = getWhereDic();
                                    var sbWhere = new StringBuilder();
                                    foreach (var sbd in sbDic)
                                        sbWhere.Append(" OR ").Append(sbd.Key);
                                    subSelect.Where(sbWhere.Remove(0, 4).ToString());
                                    sbWhere.Clear();
                                    sbDic.Clear();
                                }
                                subSelect.Where(oldWhere);

                                if (isAsync)
                                {
#if net40
#else
                                    if (selectExp == null) subList = await subSelect.ToListAsync(true, cancellationToken);
                                    else subList = await subSelect.ToListAsync<TNavigate>(selectExp, cancellationToken);
#endif
                                }
                                else
                                {
                                    if (selectExp == null) subList = subSelect.ToList(true);
                                    else subList = subSelect.ToList<TNavigate>(selectExp);
                                }
                            }

                            fillOneToManyData(subList, tbref2);
                        }
                        break;
                    case TableRefType.ManyToMany:
                        if (true)
                        {
                            List<TNavigate> subList = null;
                            List<object> midList = new List<object>();
                            var tbref2 = _commonUtils.GetTableByEntity(tbref.RefEntityType);
                            var tbrefMid = _commonUtils.GetTableByEntity(tbref.RefMiddleEntityType);
                            var tbrefMidName = _tableRules?.FirstOrDefault()?.Invoke(tbref.RefMiddleEntityType, tbrefMid.DbName) ?? tbrefMid.DbName;
                            var sbJoin = new StringBuilder().Append($"{_commonUtils.QuoteSqlName(tbrefMidName)} midtb ON ");
                            if (_orm.CodeFirst.IsAutoSyncStructure && tbrefMid.Type != typeof(object)) _orm.CodeFirst.SyncStructure(tbrefMid.Type, tbrefMidName);
                            for (var z = 0; z < tbref.RefColumns.Count; z++)
                            {
                                if (z > 0) sbJoin.Append(" AND ");
                                sbJoin.Append($"midtb.{_commonUtils.QuoteSqlName(tbref.MiddleColumns[tbref.Columns.Count + z].Attribute.Name)} = a.{_commonUtils.QuoteSqlName(tbref.RefColumns[z].Attribute.Name)}");
                                if (_whereGlobalFilter.Any())
                                {
                                    var cascade = _commonExpression.GetWhereCascadeSql(new SelectTableInfo { Alias = "midtb", AliasInit = "midtb", Table = tbrefMid, Type = SelectTableInfoType.InnerJoin }, _whereGlobalFilter, true);
                                    if (string.IsNullOrEmpty(cascade) == false)
                                        sbJoin.Append(" AND ").Append(cascade);
                                }
                            }
                            subSelect.InnerJoin(sbJoin.ToString());
                            sbJoin.Clear();

                            Select0Provider<ISelect<TNavigate>, TNavigate>.GetAllFieldExpressionTreeInfo af = null;
                            ReadAnonymousTypeAfInfo mf = default;
                            if (selectExp == null) af = subSelect.GetAllFieldExpressionTreeLevelAll();
                            else mf = subSelect.GetExpressionField(selectExp);
                            ReadAnonymousTypeAfInfo otherData = null;
                            var sbSql = new StringBuilder();

                            if (_selectExpression == null)
                            {
                                var field = new StringBuilder();
                                var read = new ReadAnonymousTypeInfo();
                                read.CsType = (tbrefMid.TypeLazy ?? tbrefMid.Type);
                                read.Consturctor = read.CsType.InternalGetTypeConstructor0OrFirst();
                                read.IsEntity = true;
                                read.Table = tbrefMid;
                                foreach (var col in tbrefMid.Columns.Values)
                                {
                                    if (tbref.MiddleColumns.Where(a => a.CsName == col.CsName).Any() == false) continue;
                                    var child = new ReadAnonymousTypeInfo
                                    {
                                        CsName = col.CsName,
                                        CsType = col.CsType,
                                        DbField = $"midtb.{_commonUtils.QuoteSqlName(col.Attribute.Name)}",
                                        MapType = col.Attribute.MapType,
                                        Property = tbrefMid.Properties[col.CsName]
                                    };
                                    read.Childs.Add(child);
                                    field.Append(", ").Append(_commonUtils.RereadColumn(col, child.DbField));
                                }
                                otherData = new ReadAnonymousTypeAfInfo(read, field.ToString());
                            }
                            Func<Dictionary<string, bool>> getWhereDic = () =>
                            {
                                var sbDic = new Dictionary<string, bool>();
                                for (var y = 0; y < list.Count; y++)
                                {
                                    var sbWhereOne = new StringBuilder();
                                    sbWhereOne.Append("(");
                                    for (var z = 0; z < tbref.Columns.Count; z++)
                                    {
                                        if (z > 0) sbWhereOne.Append(" AND ");
                                        sbWhereOne.Append(_commonUtils.FormatSql($" midtb.{_commonUtils.QuoteSqlName(tbref.MiddleColumns[z].Attribute.Name)}={{0}}", Utils.GetDataReaderValue(tbref.MiddleColumns[z].Attribute.MapType, getListValue1(list[y], tbref.Columns[z].CsName))));
                                    }
                                    sbWhereOne.Append(")");
                                    var whereOne = sbWhereOne.ToString();
                                    sbWhereOne.Clear();
                                    if (sbDic.ContainsKey(whereOne) == false) sbDic.Add(whereOne, true);
                                }
                                return sbDic;
                            };

                            if (takeNumber > 0)
                            {
                                var sbDic = getWhereDic();
                                foreach (var sbd in sbDic)
                                {
                                    subSelect._where.Clear();
                                    subSelect.Where(sbd.Key).Where(oldWhere).Limit(takeNumber);
                                    sbSql.Append("\r\nUNION ALL\r\nselect * from (").Append(subSelect.ToSql($"{(selectExp == null ? af.Field : mf.field)}{otherData?.field}")).Append(") ftb");
                                }
                                sbSql.Remove(0, 13);
                                if (sbDic.Count == 1) sbSql.Remove(0, 15).Remove(sbSql.Length - 5, 5);
                                sbDic.Clear();
                            }
                            else
                            {
                                subSelect._where.Clear();
                                if (tbref.Columns.Count == 1)
                                {
                                    subSelect.Where(_commonUtils.FormatSql($"midtb.{_commonUtils.QuoteSqlName(tbref.MiddleColumns[0].Attribute.Name)} in {{0}}", list.Select(a => Utils.GetDataReaderValue(tbref.MiddleColumns[0].Attribute.MapType, getListValue1(a, tbref.Columns[0].CsName))).Distinct()));
                                }
                                else
                                {
                                    var sbDic = getWhereDic();
                                    var sbWhere = new StringBuilder();
                                    foreach (var sbd in sbDic)
                                        sbWhere.Append(" OR ").Append(sbd.Key);
                                    subSelect.Where(sbWhere.Remove(0, 4).ToString());
                                    sbWhere.Clear();
                                    sbDic.Clear();
                                }
                                subSelect.Where(oldWhere);
                                sbSql.Append(subSelect.ToSql($"{(selectExp == null ? af.Field : mf.field)}{otherData?.field}"));
                            }

                            if (isAsync)
                            {
#if net40
#else
                                if (selectExp == null) subList = await subSelect.ToListAfPrivateAsync(sbSql.ToString(), af, otherData == null ? null : new[] { new ReadAnonymousTypeOtherInfo(otherData.field, otherData.map, midList) }, cancellationToken);
                                else subList = await subSelect.ToListMrPrivateAsync<TNavigate>(sbSql.ToString(), mf, otherData == null ? null : new[] { new ReadAnonymousTypeOtherInfo(otherData.field, otherData.map, midList) }, cancellationToken);
#endif
                            }
                            else
                            {
                                if (selectExp == null) subList = subSelect.ToListAfPrivate(sbSql.ToString(), af, otherData == null ? null : new[] { new ReadAnonymousTypeOtherInfo(otherData.field, otherData.map, midList) });
                                else subList = subSelect.ToListMrPrivate<TNavigate>(sbSql.ToString(), mf, otherData == null ? null : new[] { new ReadAnonymousTypeOtherInfo(otherData.field, otherData.map, midList) });
                            }
                            if (subList.Any() == false)
                            {
                                foreach (var item in list)
                                    setListValue(item, new List<TNavigate>());
                                return;
                            }

                            Dictionary<string, List<Tuple<T1, List<TNavigate>>>> dicList = new Dictionary<string, List<Tuple<T1, List<TNavigate>>>>();
                            foreach (var item in list)
                            {
                                if (tbref.Columns.Count == 1)
                                {
                                    var dicListKey = getListValue1(item, tbref.Columns[0].CsName)?.ToString();
                                    if (dicListKey == null) continue;
                                    var dicListVal = Tuple.Create(item, new List<TNavigate>());
                                    if (dicList.TryGetValue(dicListKey, out var items) == false)
                                        dicList.Add(dicListKey, items = new List<Tuple<T1, List<TNavigate>>>());
                                    items.Add(dicListVal);
                                }
                                else
                                {
                                    var sb = new StringBuilder();
                                    for (var z = 0; z < tbref.Columns.Count; z++)
                                    {
                                        if (z > 0) sb.Append("*$*");
                                        sb.Append(getListValue1(item, tbref.Columns[z].CsName));
                                    }
                                    var dicListKey = sb.ToString();
                                    var dicListVal = Tuple.Create(item, new List<TNavigate>());
                                    if (dicList.TryGetValue(dicListKey, out var items) == false)
                                        dicList.Add(dicListKey, items = new List<Tuple<T1, List<TNavigate>>>());
                                    items.Add(dicListVal);
                                    sb.Clear();
                                }
                            }
                            for (var a = 0; a < subList.Count; a++)
                            {
                                string key = null;
                                if (tbref.Columns.Count == 1)
                                    key = _orm.GetEntityValueWithPropertyName(tbref.RefMiddleEntityType, midList[a], tbref.MiddleColumns[0].CsName)?.ToString() ?? "";
                                else
                                {
                                    var sb = new StringBuilder();
                                    for (var z = 0; z < tbref.Columns.Count; z++)
                                    {
                                        if (z > 0) sb.Append("*$*");
                                        sb.Append(_orm.GetEntityValueWithPropertyName(tbref.RefMiddleEntityType, midList[a], tbref.MiddleColumns[z].CsName));
                                    }
                                    key = sb.ToString();
                                    sb.Clear();
                                }
                                if (dicList.TryGetValue(key, out var t1items) == false) continue;
                                foreach (var t1item in t1items)
                                    t1item.Item2.Add(subList[a]);
                            }
                            foreach (var t1items in dicList.Values)
                                foreach (var t1item in t1items)
                                    setListValue(t1item.Item1, t1item.Item2);
                            dicList.Clear();
                        }
                        break;
                    case TableRefType.PgArrayToMany:
                        if (true)
                        {
                            var subList = new List<TNavigate>();
                            var tbref2 = _commonUtils.GetTableByEntity(tbref.RefEntityType);
                            if (tbref.RefColumns[0] == tbref2.Primarys[0])
                            {
                                var listKeys = list.Select(a =>
                                {
                                    var arrVal = getListValue(a, tbref.Columns[0].CsName, 0) as Array;
                                    if (arrVal == null) return null;
                                    var arrObjVal = new object[arrVal.Length];
                                    arrVal.CopyTo(arrObjVal, 0);
                                    return arrObjVal;
                                }).ToArray();
                                var arrExp = Expression.NewArrayInit(tbref.RefColumns[0].CsType, listKeys.Where(a => a != null).SelectMany(a => a).Distinct()
                                    .Select(a => Expression.Constant(Utils.GetDataReaderValue(tbref.RefColumns[0].CsType, a), tbref.RefColumns[0].CsType)).ToArray());
                                if (arrExp.Expressions.Any())
                                {
                                    var otmExpParm1 = Expression.Parameter(typeof(TNavigate), "a");
                                    var containsMethod = _dicTypeMethod.GetOrAdd(tbref.RefColumns[0].CsType, et => new ConcurrentDictionary<string, MethodInfo>()).GetOrAdd("Contains", mn =>
                                        typeof(Enumerable).GetMethods().Where(a => a.Name == mn).First()).MakeGenericMethod(tbref.RefColumns[0].CsType);
                                    var refCol = Expression.MakeMemberAccess(otmExpParm1, tbref2.Properties[tbref.RefColumns[0].CsName]);
                                    subSelect.Where(Expression.Lambda<Func<TNavigate, bool>>(
                                        Expression.Call(null, containsMethod, arrExp, refCol), otmExpParm1));

                                    if (isAsync)
                                    {
#if net40
#else
                                        if (selectExp == null) subList = await subSelect.ToListAsync(true, cancellationToken);
                                        else subList = await subSelect.ToListAsync<TNavigate>(selectExp, cancellationToken);
#endif
                                    }
                                    else
                                    {
                                        if (selectExp == null) subList = subSelect.ToList(true);
                                        else subList = subSelect.ToList<TNavigate>(selectExp);
                                    }
                                }

                                if (subList.Any() == false)
                                {
                                    foreach (var item in list)
                                        setListValue(item, new List<TNavigate>());
                                    return;
                                }
                                var dicSubList = subList.ToDictionary(a => EntityUtilExtensions.GetEntityValueWithPropertyName(_orm, tbref.RefEntityType, a, tbref.RefColumns[0].CsName)?.ToString(), a => a);

                                var parentNavs = new List<string>();
                                foreach (var tr2 in tbref2.GetAllTableRef())
                                {
                                    var tr2ref = tr2.Value;
                                    if (tr2ref.Exception != null) continue;
                                    if (tbref2.ColumnsByCs.ContainsKey(tr2.Key)) continue;
                                    if (tbref2.ColumnsByCsIgnore.ContainsKey(tr2.Key)) continue;
                                    if (tr2ref.RefType != TableRefType.ManyToOne) continue;
                                    if (tr2ref.RefEntityType != tb.Type) continue;
                                    if (string.Join(",", tr2ref.Columns.Select(a => a.CsName).OrderBy(a => a)) != string.Join(",", tbref.RefColumns.Select(a => a.CsName).OrderBy(a => a))) continue; //- 修复 IncludeMany 只填充子属性中双向关系的 ManyToOne 对象值；防止把 ManyToOne 多个相同类型的导航属性值都填充了
                                    parentNavs.Add(tr2.Key);
                                }
                                for (var y = 0; y < list.Count; y++)
                                {
                                    var item = list[y];
                                    var dicListKeys = listKeys[y];
                                    if (dicListKeys == null) continue;
                                    var navs = new List<TNavigate>();
                                    foreach (var dlk in dicListKeys)
                                    {
                                        if (dlk == null)
                                        {
                                            navs.Add(null);
                                            continue;
                                        }
                                        var dicListKey = dlk.ToString();
                                        dicSubList.TryGetValue(dicListKey, out var nav);
                                        navs.Add(nav);
                                    }
                                    setListValue(item, navs);
                                }
                                dicSubList.Clear();
                                subList.Clear();
                            }
                            else if (tbref.Columns[0] == tb.Primarys[0])
                            {
                                var listKeys = list.Select(a => getListValue(a, tbref.Columns[0].CsName, 0)).Distinct()
                                    .Select(a => Utils.GetDataReaderValue(tbref.RefColumns[0].CsType.GetElementType(), a)).ToArray();
                                var listKeysSql = _commonUtils.GetNoneParamaterSqlValue(subSelect._params, "arrtm", tbref.RefColumns[0], tbref.RefColumns[0].CsType, listKeys);
                                subSelect.Where($"{subSelectT1Alias}.{_commonUtils.QuoteSqlName(tbref.RefColumns[0].Attribute.Name)} && {listKeysSql}");

                                if (isAsync)
                                {
#if net40
#else
                                    if (selectExp == null) subList = await subSelect.ToListAsync(true, cancellationToken);
                                    else subList = await subSelect.ToListAsync<TNavigate>(selectExp, cancellationToken);
#endif
                                }
                                else
                                {
                                    if (selectExp == null) subList = subSelect.ToList(true);
                                    else subList = subSelect.ToList<TNavigate>(selectExp);
                                }

                                if (subList.Any() == false)
                                {
                                    foreach (var item in list)
                                        setListValue(item, new List<TNavigate>());
                                    return;
                                }
                                var subListDic = subList.Select(a => {
                                    var arrVal = EntityUtilExtensions.GetEntityValueWithPropertyName(_orm, tbref2.Type, a, tbref.RefColumns[0].CsName) as Array;
                                    var arrObjVal = new object[arrVal.Length];
                                    arrVal.CopyTo(arrObjVal, 0);
                                    return arrObjVal.Select(b => NativeTuple.Create(a, b?.ToString()));
                                }).SelectMany(a => a).GroupBy(a => a.Item2).ToDictionary(a => a.Key, a => a.Select(b => b.Item1).ToList());
                                foreach (var item in list)
                                {
                                    var itemKey = getListValue(item, tbref.Columns[0].CsName, 0)?.ToString();
                                    subListDic.TryGetValue(itemKey, out var navs);
                                    setListValue(item, navs);
                                }
                            }
                        }
                        break;
                }
            };

#if net40
            _includeToList.Add(listObj => includeToListSyncOrAsync(listObj, false));
#else
            _includeToList.Add(listObj =>
            {
                var task = includeToListSyncOrAsync(listObj, false, default);
                if (task.Exception != null) throw task.Exception.InnerException ?? task.Exception;
            });
            _includeToListAsync.Add((listObj, cancellationToken) => includeToListSyncOrAsync(listObj, true, cancellationToken));
#endif
            var includeValue = new MemberExpression[members.Count + 1];
            for (var a = 0; a < members.Count; a++) includeValue[a] = members[a];
            includeValue[includeValue.Length - 1] = expBody as MemberExpression;
            var includeKey = $"{string.Join(".", includeValue.Select(a => a.Member.Name))}";
            if (_includeInfo.ContainsKey(includeKey) == false) _includeInfo.Add(includeKey, NativeTuple.Create(includeValue, tbref));
            return this;
        }

        internal object _includeManySubListOneToManyTempValue1 = null;
        internal void SetList(IEnumerable<T1> list)
        {
            foreach (var include in _includeToList) include?.Invoke(list);
            _trackToList?.Invoke(list);
        }

#if net40
#else
        async internal Task SetListAsync(IEnumerable<T1> list, CancellationToken cancellationToken = default)
        {
            foreach (var include in _includeToListAsync) await include?.Invoke(list, cancellationToken);
            _trackToList?.Invoke(list);
        }

        public Task<double> AvgAsync<TMember>(Expression<Func<T1, TMember>> column, CancellationToken cancellationToken = default)
        {
            if (column == null) return Task.FromResult(default(double));
            _tables[0].Parameter = column.Parameters[0];
            return this.InternalAvgAsync(column?.Body, cancellationToken);
        }
        public Task<TMember> MaxAsync<TMember>(Expression<Func<T1, TMember>> column, CancellationToken cancellationToken = default)
        {
            if (column == null) return Task.FromResult(default(TMember));
            _tables[0].Parameter = column.Parameters[0];
            return this.InternalMaxAsync<TMember>(column?.Body, cancellationToken);
        }
        public Task<TMember> MinAsync<TMember>(Expression<Func<T1, TMember>> column, CancellationToken cancellationToken = default)
        {
            if (column == null) return Task.FromResult(default(TMember));
            _tables[0].Parameter = column.Parameters[0];
            return this.InternalMinAsync<TMember>(column?.Body, cancellationToken);
        }
        public Task<decimal> SumAsync<TMember>(Expression<Func<T1, TMember>> column, CancellationToken cancellationToken = default)
        {
            if (column == null) return Task.FromResult(default(decimal));
            _tables[0].Parameter = column.Parameters[0];
            return this.InternalSumAsync(column?.Body, cancellationToken);
        }
        async public Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, TReturn>> select, CancellationToken cancellationToken = default)
        {
            if (select == null) return await this.InternalToListAsync<TReturn>(select?.Body, cancellationToken);
            _tables[0].Parameter = select.Parameters[0];
            if (_includeToList?.Any() != true) return await this.InternalToListAsync<TReturn>(select.Body, cancellationToken);

            var findIncludeMany = new List<string>(); //支持指定已经使用 IncudeMany 的导航属性
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = 0;
            _commonExpression.ReadAnonymousField(_tables, _tableRule, field, map, ref index, select.Body, this, _diymemexpWithTempQuery, _whereGlobalFilter, findIncludeMany, null, true);
            var af = new ReadAnonymousTypeAfInfo(map, field.Length > 0 ? field.Remove(0, 2).ToString() : null);
            if (findIncludeMany.Any() == false) return await this.ToListMapReaderPrivateAsync<TReturn>(af, null, cancellationToken);

            var parmExp = Expression.Parameter(_tables[0].Table.Type, _tables[0].Alias);
            var incNewInit = new IncludeManyNewInit(_tables[0].Table, parmExp);
            foreach (var inc in _includeInfo)
            {
                var curIncNewInit = incNewInit;
                Expression curParmExp = parmExp;
                for (var a = 0; a < inc.Value.Item1.Length - 1; a++)
                {
                    curParmExp = Expression.MakeMemberAccess(parmExp, inc.Value.Item1[a].Member);
                    if (curIncNewInit.Childs.ContainsKey(inc.Value.Item1[a].Member.Name) == false)
                        curIncNewInit.Childs.Add(inc.Value.Item1[a].Member.Name, curIncNewInit = new IncludeManyNewInit(_orm.CodeFirst.GetTableByEntity(inc.Value.Item1[a].Type), curParmExp));
                    else
                        curIncNewInit = curIncNewInit.Childs[inc.Value.Item1[a].Member.Name];
                }
                curIncNewInit.IsOutputPrimary = true;
                var outcols = inc.Value.Item2.Columns.Where(a => a.Attribute.IsPrimary == false).ToArray();
                if (outcols.Any()) curIncNewInit.OutputColumns.AddRange(outcols);
            }
            MemberInitExpression GetIncludeManyNewInitExpression(IncludeManyNewInit imni)
            {
                var bindings = new List<MemberBinding>();
                if (imni.IsOutputPrimary)
                {
                    bindings.AddRange(imni.Table.Primarys.Select(a => Expression.Bind(imni.Table.Properties[a.CsName], Expression.MakeMemberAccess(imni.CurrentExpression, imni.Table.Properties[a.CsName]))));
                    if (imni.OutputColumns.Any()) bindings.AddRange(imni.OutputColumns.Select(a => Expression.Bind(imni.Table.Properties[a.CsName], Expression.MakeMemberAccess(imni.CurrentExpression, imni.Table.Properties[a.CsName]))));
                }
                if (imni.Childs.Any()) bindings.AddRange(imni.Childs.Select(a => Expression.Bind(imni.Table.Properties[a.Key], GetIncludeManyNewInitExpression(a.Value))));
                var pgarrayToManys = imni.Table.GetAllTableRef().Select(tr =>
                {
                    if (tr.Value.RefType != TableRefType.PgArrayToMany) return null;
                    var reftb = _orm.CodeFirst.GetTableByEntity(tr.Value.RefEntityType);
                    if (tr.Value.RefColumns[0] == reftb.Primarys[0])
                    {
                        bindings.Add(Expression.Bind(imni.Table.Properties[tr.Value.Columns[0].CsName], Expression.MakeMemberAccess(imni.CurrentExpression, imni.Table.Properties[tr.Value.Columns[0].CsName])));
                        return tr.Key;
                    }
                    return null;
                }).ToList();
                return Expression.MemberInit(imni.Table.Type.InternalNewExpression(), bindings);
            }

            var otherNewInit = GetIncludeManyNewInitExpression(incNewInit); //获取 IncludeMany 包含的最简化字段
            if (otherNewInit.Bindings.Any() == false) return await this.ToListMapReaderPrivateAsync<TReturn>(af, null, cancellationToken);

            var otherMap = new ReadAnonymousTypeInfo();
            field.Clear();
            _commonExpression.ReadAnonymousField(_tables, _tableRule, field, otherMap, ref index, otherNewInit, this, _diymemexpWithTempQuery, _whereGlobalFilter, null, null, true);
            var otherRet = new List<object>();
            var otherAf = new ReadAnonymousTypeOtherInfo(field.ToString(), otherMap, otherRet);

            af.fillIncludeMany = new List<NativeTuple<string, IList, int>>();
            var ret = await this.ToListMapReaderPrivateAsync<TReturn>(af, new[] { otherAf }, cancellationToken);
            await this.SetListAsync(otherRet.Select(a => (T1)a).ToList(), cancellationToken); //级联加载

            foreach (var fim in af.fillIncludeMany)
            {
                var splitKeys = fim.Item1.Split('.');
                var otherRetItem = otherRet[fim.Item3];
                var otherRetItemType = _tables[0].Table.Type;
                foreach (var splitKey in splitKeys)
                {
                    otherRetItem = _orm.GetEntityValueWithPropertyName(otherRetItemType, otherRetItem, splitKey);
                    otherRetItemType = _orm.CodeFirst.GetTableByEntity(otherRetItemType).Properties[splitKey].PropertyType;
                }
                if (otherRetItem == null) continue;
                var otherList = otherRetItem as IEnumerable;
                foreach (var otherListItem in otherList) fim.Item2.Add(otherListItem);
            }
            return ret;
        }
        async public Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default)
        {
            if (typeof(T1) == typeof(TDto)) return await ToListAsync(false, cancellationToken) as List<TDto>;
            if (_tables.FirstOrDefault()?.Table.Type == typeof(object)) return await ToListAsync<TDto>("*", cancellationToken);
            return await ToListAsync(GetToListDtoSelector<TDto>(), cancellationToken);
        }

        public Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class => base.InternalInsertIntoAsync<TTargetEntity>(tableName, select, cancellationToken);

        public Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, TReturn>> select, CancellationToken cancellationToken = default)
        {
            if (select == null) return this.InternalToDataTableAsync(select?.Body, cancellationToken);
            _tables[0].Parameter = select.Parameters[0];
            return this.InternalToDataTableAsync(select?.Body, cancellationToken);
        }
        public Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, TReturn>> select, CancellationToken cancellationToken = default)
        {
            if (select == null) return Task.FromResult(default(TReturn));
            _tables[0].Parameter = select.Parameters[0];
            return this.InternalToAggregateAsync<TReturn>(select?.Body, cancellationToken);
        }

        async public Task<bool> AnyAsync(Expression<Func<T1, bool>> exp, CancellationToken cancellationToken = default)
        {
            var oldwhere = _where.ToString();
            var ret = await this.Where(exp).AnyAsync(cancellationToken);
            _where.Clear().Append(oldwhere);
            return ret;
        }
        async public Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, TReturn>> select, CancellationToken cancellationToken = default) => (await this.Limit(1).ToListAsync(select, cancellationToken)).FirstOrDefault();
        async public Task<TDto> ToOneAsync<TDto>(CancellationToken cancellationToken = default) => (await this.Limit(1).ToListAsync<TDto>(cancellationToken)).FirstOrDefault();
        public Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, TReturn>> select, CancellationToken cancellationToken = default) => this.ToOneAsync(select, cancellationToken);
        public Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default) => this.ToOneAsync<TDto>(cancellationToken);
        public override Task<List<T1>> ToListAsync(bool includeNestedMembers, CancellationToken cancellationToken = default) => base.ToListAsync(_isIncluded || includeNestedMembers, cancellationToken);
#endif
    }
}