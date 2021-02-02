using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{
    partial class Select0Provider<TSelect, T1>
    {
        public DataTable ToDataTable(string field = null)
        {
            DataTable ret = null;
            if (_cancel?.Invoke() == true) return ret;
            var sql = this.ToSql(field);
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                ret = _orm.Ado.ExecuteDataTable(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return ret;
        }

        public List<TTuple> ToList<TTuple>(string field)
        {
            var ret = new List<TTuple>();
            if (_cancel?.Invoke() == true) return ret;
            var sql = this.ToSql(field);
            var type = typeof(TTuple);
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                if (type.IsClass)
                    ret = _orm.Ado.Query<TTuple>(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms);
                else
                {
                    var flagStr = $"ToListField:{field}";
                    _orm.Ado.ExecuteReader(_connection, _transaction, fetch =>
                    {
                        var read = Utils.ExecuteArrayRowReadClassOrTuple(flagStr, type, null, fetch.Object, 0, _commonUtils);
                        ret.Add((TTuple)read.Value);
                    }, CommandType.Text, sql, _commandTimeout, dbParms);
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return ret;
        }
        internal List<T1> ToListAfPrivate(string sql, GetAllFieldExpressionTreeInfo af, ReadAnonymousTypeOtherInfo[] otherData)
        {
            var ret = new List<T1>();
            if (_cancel?.Invoke() == true) return ret;
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var retCount = 0;
            Exception exception = null;
            try
            {
                _orm.Ado.ExecuteReader(_connection, _transaction, fetch =>
                {
                    ret.Add(af.Read(_orm, fetch.Object));
                    if (otherData != null)
                    {
                        var idx = af.FieldCount - 1;
                        foreach (var other in otherData)
                            other.retlist.Add(_commonExpression.ReadAnonymous(other.read, fetch.Object, ref idx, false, null, retCount, null));
                    }
                    retCount++;
                }, CommandType.Text, sql, _commandTimeout, dbParms);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            foreach (var include in _includeToList) include?.Invoke(ret);
            _trackToList?.Invoke(ret);
            return ret;
        }
        internal List<T1> ToListPrivate(GetAllFieldExpressionTreeInfo af, ReadAnonymousTypeOtherInfo[] otherData)
        {
            string sql = null;
            if (otherData?.Length > 0)
            {
                var sbField = new StringBuilder().Append(af.Field);
                foreach (var other in otherData)
                    sbField.Append(other.field);
                sql = this.ToSql(sbField.ToString());
            }
            else
                sql = this.ToSql(af.Field);

            return ToListAfPrivate(sql, af, otherData);
        }
        #region ToChunk
        internal void ToListAfChunkPrivate(int chunkSize, Action<FetchCallbackArgs<List<T1>>> chunkDone, string sql, GetAllFieldExpressionTreeInfo af, ReadAnonymousTypeOtherInfo[] otherData)
        {
            if (_cancel?.Invoke() == true) return;
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var ret = new FetchCallbackArgs<List<T1>> { Object = new List<T1>() };
            var retCount = 0;
            Exception exception = null;
            var checkDoneTimes = 0;
            try
            {
                _orm.Ado.ExecuteReader(_connection, _transaction, fetch =>
                {
                    ret.Object.Add(af.Read(_orm, fetch.Object));
                    if (otherData != null)
                    {
                        var idx = af.FieldCount - 1;
                        foreach (var other in otherData)
                            other.retlist.Add(_commonExpression.ReadAnonymous(other.read, fetch.Object, ref idx, false, null, ret.Object.Count - 1, null));
                    }
                    retCount++;
                    if (chunkSize > 0 && chunkSize == ret.Object.Count)
                    {
                        checkDoneTimes++;

                        foreach (var include in _includeToList) include?.Invoke(ret.Object);
                        _trackToList?.Invoke(ret.Object);
                        chunkDone(ret);
                        fetch.IsBreak = ret.IsBreak;

                        ret.Object.Clear();
                        if (otherData != null)
                            foreach (var other in otherData)
                                other.retlist.Clear();
                    }
                }, CommandType.Text, sql, _commandTimeout, dbParms);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, retCount);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            if (ret.Object.Any() || checkDoneTimes == 0)
            {
                foreach (var include in _includeToList) include?.Invoke(ret.Object);
                _trackToList?.Invoke(ret.Object);
                chunkDone(ret);
            }
        }
        internal void ToListChunkPrivate(int chunkSize, Action<FetchCallbackArgs<List<T1>>> chunkDone, GetAllFieldExpressionTreeInfo af, ReadAnonymousTypeOtherInfo[] otherData)
        {
            string sql = null;
            if (otherData?.Length > 0)
            {
                var sbField = new StringBuilder().Append(af.Field);
                foreach (var other in otherData)
                    sbField.Append(other.field);
                sql = this.ToSql(sbField.ToString());
            }
            else
                sql = this.ToSql(af.Field);

            ToListAfChunkPrivate(chunkSize, chunkDone, sql, af, otherData);
        }
        public void ToChunk(int size, Action<FetchCallbackArgs<List<T1>>> done, bool includeNestedMembers = false)
        {
            if (_selectExpression != null) throw new ArgumentException("Chunk 功能之前不可使用 Select");
            this.ToListChunkPrivate(size, done, includeNestedMembers == false ? this.GetAllFieldExpressionTreeLevel2() : this.GetAllFieldExpressionTreeLevelAll(), null);
        }

        internal void ToListMrChunkPrivate<TReturn>(int chunkSize, Action<FetchCallbackArgs<List<TReturn>>> chunkDone, string sql, ReadAnonymousTypeAfInfo af)
        {
            if (_cancel?.Invoke() == true) return;
            var type = typeof(TReturn);
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var ret = new FetchCallbackArgs<List<TReturn>> { Object = new List<TReturn>() };
            var retCount = 0;
            Exception exception = null;
            var checkDoneTimes = 0;
            try
            {
                _orm.Ado.ExecuteReader(_connection, _transaction, fetch =>
                {
                    var index = -1;
                    ret.Object.Add((TReturn)_commonExpression.ReadAnonymous(af.map, fetch.Object, ref index, false, null, ret.Object.Count, af.fillIncludeMany));
                    retCount++;
                    if (chunkSize > 0 && chunkSize == ret.Object.Count)
                    {
                        checkDoneTimes++;

                        foreach (var include in _includeToList) include?.Invoke(ret.Object);
                        _trackToList?.Invoke(ret.Object);
                        chunkDone(ret);
                        fetch.IsBreak = ret.IsBreak;

                        ret.Object.Clear();
                    }
                }, CommandType.Text, sql, _commandTimeout, dbParms);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, retCount);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            if (ret.Object.Any() || checkDoneTimes == 0)
            {
                foreach (var include in _includeToList) include?.Invoke(ret.Object);
                _trackToList?.Invoke(ret.Object);
                chunkDone(ret);
            }
        }
        public void InternalToChunk<TReturn>(Expression select, int size, Action<FetchCallbackArgs<List<TReturn>>> done)
        {
            var af = this.GetExpressionField(select);
            var sql = this.ToSql(af.field);
            this.ToListMrChunkPrivate<TReturn>(size, done, sql, af);
        }
        #endregion

        public Dictionary<TKey, T1> ToDictionary<TKey>(Func<T1, TKey> keySelector) => ToDictionary(keySelector, a => a);
        public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<T1, TKey> keySelector, Func<T1, TElement> elementSelector)
        {
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null) throw new ArgumentNullException(nameof(elementSelector));

            var ret = new Dictionary<TKey, TElement>();
            if (_cancel?.Invoke() == true) return ret;
            var af = this.GetAllFieldExpressionTreeLevel2();
            var sql = this.ToSql(af.Field);
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                _orm.Ado.ExecuteReader(_connection, _transaction, fetch =>
                {
                    var item = af.Read(_orm, fetch.Object);
                    ret.Add(keySelector(item), elementSelector(item));
                }, CommandType.Text, sql, _commandTimeout, dbParms);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            if (typeof(TElement) == typeof(T1)) _trackToList?.Invoke(ret.Values);
            return ret;
        }

        internal List<TReturn> ToListMrPrivate<TReturn>(string sql, ReadAnonymousTypeAfInfo af, ReadAnonymousTypeOtherInfo[] otherData)
        {
            var ret = new List<TReturn>();
            if (_cancel?.Invoke() == true) return ret;
            var type = typeof(TReturn);
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var retCount = 0;
            Exception exception = null;
            try
            {
                _orm.Ado.ExecuteReader(_connection, _transaction, fetch =>
                {
                    var index = -1;
                    ret.Add((TReturn)_commonExpression.ReadAnonymous(af.map, fetch.Object, ref index, false, null, retCount, af.fillIncludeMany));
                    if (otherData != null)
                        foreach (var other in otherData)
                            other.retlist.Add(_commonExpression.ReadAnonymous(other.read, fetch.Object, ref index, false, null, retCount, null));
                    retCount++;
                }, CommandType.Text, sql, _commandTimeout, dbParms);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            if (typeof(TReturn) == typeof(T1))
                foreach (var include in _includeToList) include?.Invoke(ret);
            _trackToList?.Invoke(ret);
            return ret;
        }
        internal List<TReturn> ToListMapReaderPrivate<TReturn>(ReadAnonymousTypeAfInfo af, ReadAnonymousTypeOtherInfo[] otherData)
        {
            string sql = null;
            if (otherData?.Length > 0)
            {
                var sbField = new StringBuilder().Append(af.field);
                foreach (var other in otherData)
                    sbField.Append(other.field);
                sql = this.ToSql(sbField.ToString());
            }
            else
                sql = this.ToSql(af.field);

            return ToListMrPrivate<TReturn>(sql, af, otherData);
        }
        protected List<TReturn> ToListMapReader<TReturn>(ReadAnonymousTypeAfInfo af) => ToListMapReaderPrivate<TReturn>(af, null);
        protected ReadAnonymousTypeAfInfo GetExpressionField(Expression newexp, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = fieldAlias == FieldAliasOptions.AsProperty ? CommonExpression.ReadAnonymousFieldAsCsName : 0;

            _commonExpression.ReadAnonymousField(_tables, field, map, ref index, newexp, this, null, _whereGlobalFilter, null, true);
            return new ReadAnonymousTypeAfInfo(map, field.Length > 0 ? field.Remove(0, 2).ToString() : null);
        }
        static ConcurrentDictionary<string, GetAllFieldExpressionTreeInfo> _dicGetAllFieldExpressionTree = new ConcurrentDictionary<string, GetAllFieldExpressionTreeInfo>();
        public class GetAllFieldExpressionTreeInfo
        {
            public string Field { get; set; }
            public int FieldCount { get; set; }
            public Func<IFreeSql, DbDataReader, T1> Read { get; set; }
        }
        public GetAllFieldExpressionTreeInfo GetAllFieldExpressionTreeLevelAll()
        {
            return _dicGetAllFieldExpressionTree.GetOrAdd($"*{string.Join("+", _tables.Select(a => $"{_orm.Ado.DataType}-{a.Table.DbName}-{a.Alias}-{a.Type}"))}", s =>
            {
                var type = _tables.First().Table.TypeLazy ?? _tables.First().Table.Type;
                var ormExp = Expression.Parameter(typeof(IFreeSql), "orm");
                var rowExp = Expression.Parameter(typeof(DbDataReader), "row");
                var returnTarget = Expression.Label(type);
                var retExp = Expression.Variable(type, "ret");
                var dataIndexExp = Expression.Variable(typeof(int), "dataIndex");
                var readExp = Expression.Variable(typeof(Utils.RowInfo), "read");
                var readExpValue = Expression.MakeMemberAccess(readExp, Utils.RowInfo.PropertyValue);
                var readExpDataIndex = Expression.MakeMemberAccess(readExp, Utils.RowInfo.PropertyDataIndex);
                var blockExp = new List<Expression>();
                blockExp.AddRange(new Expression[] {
                    Expression.Assign(retExp, type.InternalNewExpression()),
                    Expression.Assign(dataIndexExp, Expression.Constant(0))
                });
                //typeof(Topic).GetMethod("get_Type").IsVirtual

                var field = new StringBuilder();
                var dicfield = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
                var tb = _tables.First();
                var index = 0;

                var tborder = new[] { tb }.Concat(_tables.ToArray().Where((a, b) => b > 0).OrderBy(a => a.Alias));
                var tbiindex = 0;
                foreach (var tbi in tborder)
                {
                    if (tbiindex > 0 && tbi.Type == SelectTableInfoType.From) continue;
                    if (tbiindex > 0 && tbi.Alias.StartsWith($"{tb.Alias}__") == false) continue;

                    var typei = tbi.Table.TypeLazy ?? tbi.Table.Type;
                    Expression curExp = retExp;

                    var colidx = 0;
                    foreach (var col in tbi.Table.Columns.Values)
                    {
                        if (index > 0)
                        {
                            field.Append(", ");
                            if (tbiindex > 0 && colidx == 0) field.Append("\r\n");
                        }
                        var quoteName = _commonUtils.QuoteSqlName(col.Attribute.Name);
                        field.Append(_commonUtils.RereadColumn(col, $"{tbi.Alias}.{quoteName}"));
                        ++index;
                        if (dicfield.ContainsKey(quoteName)) field.Append(_commonUtils.FieldAsAlias($"as{index}"));
                        else dicfield.Add(quoteName, true);
                        ++colidx;
                    }
                    tbiindex++;

                    if (tbiindex == 0)
                        blockExp.AddRange(new Expression[] {
                            Expression.Assign(readExp, Expression.Call(Utils.MethodExecuteArrayRowReadClassOrTuple, new Expression[] { Expression.Constant(null, typeof(string)), Expression.Constant(typei), Expression.Constant(null, typeof(int[])), rowExp, dataIndexExp, Expression.Constant(_commonUtils) })),
                            Expression.IfThen(
                                Expression.GreaterThan(readExpDataIndex, dataIndexExp),
                                Expression.Assign(dataIndexExp, readExpDataIndex)
                            ),
                            Expression.IfThen(
                                Expression.NotEqual(readExpValue, Expression.Constant(null)),
                                Expression.Assign(retExp, Expression.Convert(readExpValue, typei))
                            )
                        });
                    else
                    {
                        Expression curExpIfNotNull = Expression.IsTrue(Expression.Constant(true));
                        var curTb = tb;
                        var parentNameSplits = tbi.Alias.Split(new[] { "__" }, StringSplitOptions.None);
                        var iscontinue = false;
                        for (var k = 1; k < parentNameSplits.Length; k++)
                        {
                            var curPropName = parentNameSplits[k];
                            if (curTb.Table.Properties.TryGetValue(parentNameSplits[k], out var tryprop) == false)
                            {
                                k++;
                                curPropName = $"{curPropName}__{parentNameSplits[k]}";
                                if (curTb.Table.Properties.TryGetValue(parentNameSplits[k], out tryprop) == false)
                                {
                                    iscontinue = true;
                                    break;
                                }
                            }
                            curExp = Expression.MakeMemberAccess(curExp, tryprop);
                            if (k + 1 < parentNameSplits.Length)
                                curExpIfNotNull = Expression.AndAlso(curExpIfNotNull, Expression.NotEqual(curExp, Expression.Default(tryprop.PropertyType)));
                            curTb = _tables.Where(a => a.Alias == $"{curTb.Alias}__{curPropName}" && a.Table.Type == tryprop.PropertyType).FirstOrDefault();
                            if (curTb == null)
                            {
                                iscontinue = true;
                                break;
                            }
                        }
                        if (iscontinue) continue;

                        blockExp.Add(
                            Expression.IfThenElse(
                                curExpIfNotNull,
                                Expression.Block(new Expression[] {
                                    Expression.Assign(readExp, Expression.Call(Utils.MethodExecuteArrayRowReadClassOrTuple, new Expression[] { Expression.Constant(null, typeof(string)), Expression.Constant(typei), Expression.Constant(null, typeof(int[])), rowExp, dataIndexExp, Expression.Constant(_commonUtils) })),
                                    Expression.IfThen(
                                        Expression.GreaterThan(readExpDataIndex, dataIndexExp),
                                        Expression.Assign(dataIndexExp, readExpDataIndex)
                                    ),
                                    Expression.IfThen(
                                        Expression.NotEqual(retExp, Expression.Constant(null)),
                                        Expression.IfThenElse(
                                            Expression.NotEqual(readExpValue, Expression.Constant(null)),
                                            Expression.Assign(curExp, Expression.Convert(readExpValue, typei)),
                                            Expression.Assign(curExp, Expression.Constant(null, typei))
                                        )
                                    )
                                }),
                                Expression.Block(
                                    Expression.Assign(readExpValue, Expression.Constant(null, typeof(object))),
                                    Expression.Assign(dataIndexExp, Expression.Constant(index))
                                )
                            )
                        );
                    }

                    if (tbi.Table.TypeLazy != null)
                        blockExp.Add(
                            Expression.IfThen(
                                Expression.NotEqual(readExpValue, Expression.Constant(null)),
                                Expression.Call(Expression.TypeAs(readExpValue, typei), tbi.Table.TypeLazySetOrm, ormExp)
                            )
                        ); //将 orm 传递给 lazy
                }

                blockExp.AddRange(new Expression[] {
                    Expression.Return(returnTarget, retExp),
                    Expression.Label(returnTarget, Expression.Default(type))
                });
                return new GetAllFieldExpressionTreeInfo
                {
                    Field = field.ToString(),
                    FieldCount = index,
                    Read = Expression.Lambda<Func<IFreeSql, DbDataReader, T1>>(Expression.Block(new[] { retExp, dataIndexExp, readExp }, blockExp), new[] { ormExp, rowExp }).Compile()
                };
            });
        }
        static EventHandler<Aop.AuditDataReaderEventArgs> _OldAuditDataReaderHandler;
        public GetAllFieldExpressionTreeInfo GetAllFieldExpressionTreeLevel2()
        {
            if (_selectExpression != null) //ToSql
            {
                var af = this.GetExpressionField(_selectExpression);
                return new GetAllFieldExpressionTreeInfo
                {
                    Field = af.field,
                    Read = (dr, idx) => throw new Exception("GetAllFieldExpressionTreeInfo.Read Is Null")
                };
            }
            if (_OldAuditDataReaderHandler != _orm.Aop.AuditDataReaderHandler)
            {
                _OldAuditDataReaderHandler = _orm.Aop.AuditDataReaderHandler; //清除单表 ExppressionTree
                _dicGetAllFieldExpressionTree.TryRemove($"{_orm.Ado.DataType}-{_tables[0].Table.DbName}-{_tables[0].Alias}-{_tables[0].Type}", out var oldet);
            }
            return _dicGetAllFieldExpressionTree.GetOrAdd(string.Join("+", _tables.Select(a => $"{_orm.Ado.DataType}-{a.Table.DbName}-{a.Alias}-{a.Type}")), s =>
            {
                var tb1 = _tables.First().Table;
                var type = tb1.TypeLazy ?? tb1.Type;
                var props = tb1.Properties;

                var ormExp = Expression.Parameter(typeof(IFreeSql), "orm");
                var rowExp = Expression.Parameter(typeof(DbDataReader), "row");
                var returnTarget = Expression.Label(type);
                var retExp = Expression.Variable(type, "ret");
                var dataIndexExp = Expression.Variable(typeof(int), "dataIndex");
                var readExp = Expression.Variable(typeof(Utils.RowInfo), "read");
                var readExpValue = Expression.MakeMemberAccess(readExp, Utils.RowInfo.PropertyValue);
                var readExpDataIndex = Expression.MakeMemberAccess(readExp, Utils.RowInfo.PropertyDataIndex);
                var blockExp = new List<Expression>();
                blockExp.AddRange(new Expression[] {
                    Expression.Assign(retExp, type.InternalNewExpression()),
                    Expression.Assign(dataIndexExp, Expression.Constant(0))
                });
                //typeof(Topic).GetMethod("get_Type").IsVirtual

                var field = new StringBuilder();
                var dicfield = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
                var tb = _tables.First();
                var index = 0;
                var otherindex = 0;
                foreach (var prop in props.Values)
                {
                    if (tb.Table.ColumnsByCsIgnore.ContainsKey(prop.Name)) continue;

                    if (tb.Table.ColumnsByCs.TryGetValue(prop.Name, out var col))
                    { //普通字段
                        if (index > 0) field.Append(", ");
                        var quoteName = _commonUtils.QuoteSqlName(col.Attribute.Name);
                        field.Append(_commonUtils.RereadColumn(col, $"{tb.Alias}.{quoteName}"));
                        ++index;
                        if (dicfield.ContainsKey(quoteName)) field.Append(_commonUtils.FieldAsAlias($"as{index}"));
                        else dicfield.Add(quoteName, true);
                    }
                    else
                    {
                        var tb2 = _tables.Where((a, b) => b > 0 &&
                            (a.Type == SelectTableInfoType.InnerJoin || a.Type == SelectTableInfoType.LeftJoin || a.Type == SelectTableInfoType.RightJoin) &&
                            string.IsNullOrEmpty(a.On) == false &&
                            a.Alias.StartsWith($"{tb.Alias}__") && //开头结尾完全匹配
                            a.Alias.EndsWith($"__{prop.Name}") //不清楚会不会有其他情况 求大佬优化
                            ).FirstOrDefault(); //判断 b > 0 防止 parent 递归关系
                        if (tb2 == null && props.Where(pw => pw.Value.PropertyType == prop.PropertyType).Count() == 1)
                            tb2 = _tables.Where((a, b) => b > 0 &&
                                (a.Type == SelectTableInfoType.InnerJoin || a.Type == SelectTableInfoType.LeftJoin || a.Type == SelectTableInfoType.RightJoin) &&
                                string.IsNullOrEmpty(a.On) == false &&
                                a.Table.Type == prop.PropertyType).FirstOrDefault();
                        if (tb2 == null) continue;
                        foreach (var col2 in tb2.Table.Columns.Values)
                        {
                            if (index > 0) field.Append(", ");
                            var quoteName = _commonUtils.QuoteSqlName(col2.Attribute.Name);
                            field.Append(_commonUtils.RereadColumn(col2, $"{tb2.Alias}.{quoteName}"));
                            ++index;
                            ++otherindex;
                            if (dicfield.ContainsKey(quoteName)) field.Append(_commonUtils.FieldAsAlias($"as{index}"));
                            else dicfield.Add(quoteName, true);
                        }
                    }
                    //只读到二级属性
                    var propGetSetMethod = prop.GetSetMethod(true);
                    Expression readExpAssign = null; //加速缓存
                    if (prop.PropertyType.IsArray) readExpAssign = Expression.New(Utils.RowInfo.Constructor,
                        Utils.GetDataReaderValueBlockExpression(prop.PropertyType, Expression.Call(Utils.MethodDataReaderGetValue, new Expression[] { Expression.Constant(_commonUtils), rowExp, dataIndexExp })),
                        //Expression.Call(Utils.MethodGetDataReaderValue, new Expression[] { Expression.Constant(prop.PropertyType), Expression.Call(rowExp, Utils.MethodDataReaderGetValue, dataIndexExp) }),
                        Expression.Add(dataIndexExp, Expression.Constant(1))
                    );
                    else
                    {
                        var proptypeGeneric = prop.PropertyType;
                        if (proptypeGeneric.IsNullableType()) proptypeGeneric = proptypeGeneric.GetGenericArguments().First();
                        if (proptypeGeneric.IsEnum ||
                            Utils.dicExecuteArrayRowReadClassOrTuple.ContainsKey(proptypeGeneric)) readExpAssign = Expression.New(Utils.RowInfo.Constructor,
                                Utils.GetDataReaderValueBlockExpression(prop.PropertyType, Expression.Call(Utils.MethodDataReaderGetValue, new Expression[] { Expression.Constant(_commonUtils), rowExp, dataIndexExp })),
                                //Expression.Call(Utils.MethodGetDataReaderValue, new Expression[] { Expression.Constant(prop.PropertyType), Expression.Call(rowExp, Utils.MethodDataReaderGetValue, dataIndexExp) }),
                                Expression.Add(dataIndexExp, Expression.Constant(1))
                        );
                        else
                        {
                            var propLazyType = _commonUtils.GetTableByEntity(prop.PropertyType)?.TypeLazy ?? prop.PropertyType;
                            readExpAssign = Expression.Call(Utils.MethodExecuteArrayRowReadClassOrTuple, new Expression[] { Expression.Constant(null, typeof(string)), Expression.Constant(propLazyType), Expression.Constant(null, typeof(int[])), rowExp, dataIndexExp, Expression.Constant(_commonUtils) });
                        }
                    }
                    blockExp.AddRange(new Expression[] {
                        Expression.Assign(readExp, readExpAssign),
                        Expression.IfThen(Expression.GreaterThan(readExpDataIndex, dataIndexExp),
                            Expression.Assign(dataIndexExp, readExpDataIndex)),
						//Expression.Call(typeof(Trace).GetMethod("WriteLine", new Type[]{typeof(string)}), Expression.Call(typeof(string).GetMethod("Concat", new Type[]{typeof(object) }), readExpValue)),

						tb1.TypeLazy != null ?
                            Expression.IfThenElse(
                                Expression.NotEqual(readExpValue, Expression.Constant(null)),
                                Expression.Call(retExp, propGetSetMethod, Expression.Convert(readExpValue, prop.PropertyType)),
                                Expression.Call(retExp, propGetSetMethod, Expression.Convert(Utils.GetDataReaderValueBlockExpression(prop.PropertyType, Expression.Constant(null)), prop.PropertyType))
                            ) :
                            Expression.IfThen(
                                Expression.NotEqual(readExpValue, Expression.Constant(null)),
                                Expression.Call(retExp, propGetSetMethod, Expression.Convert(readExpValue, prop.PropertyType))
                            )
                    });
                }
                if (tb1.TypeLazy != null)
                    blockExp.Add(
                        Expression.IfThen(
                            Expression.NotEqual(readExpValue, Expression.Constant(null)),
                            Expression.Call(retExp, tb1.TypeLazySetOrm, ormExp)
                        )
                    ); //将 orm 传递给 lazy
                if (otherindex == 0)
                {
                    //不读导航属性，优化单表读取性能
                    blockExp.Clear();
                    blockExp.AddRange(new Expression[]{
                        Expression.Assign(retExp, type.InternalNewExpression()),
                        Expression.Assign(dataIndexExp, Expression.Constant(0))
                    });
                    var colidx = 0;
                    foreach (var col in tb.Table.Columns.Values)
                    {
                        var drvalType = col.Attribute.MapType.NullableTypeOrThis();
                        var propGetSetMethod = tb.Table.Properties[col.CsName].GetSetMethod(true);
                        if (col.CsType == col.Attribute.MapType &&
                            _orm.Aop.AuditDataReaderHandler == null &&
                            _dicMethodDataReaderGetValue.TryGetValue(col.Attribute.MapType.NullableTypeOrThis(), out var drGetValueMethod))
                        {
                            Expression drvalExp = Expression.Call(rowExp, drGetValueMethod, Expression.Constant(colidx));
                            if (col.CsType.IsNullableType()) drvalExp = Expression.Convert(drvalExp, col.CsType);
                            drvalExp = Expression.Condition(Expression.Call(rowExp, _MethodDataReaderIsDBNull, Expression.Constant(colidx)), Expression.Default(col.CsType), drvalExp);

                            if (drvalType.IsArray || drvalType.IsEnum || Utils.dicExecuteArrayRowReadClassOrTuple.ContainsKey(drvalType))
                            {
                                var drvalExpCatch = Utils.GetDataReaderValueBlockExpression(
                                    col.CsType,
                                    Expression.Call(Utils.MethodDataReaderGetValue, new Expression[] { Expression.Constant(_commonUtils), rowExp, Expression.Constant(colidx) })
                                );
                                blockExp.Add(Expression.TryCatch(
                                    Expression.Call(retExp, propGetSetMethod, drvalExp),
                                    Expression.Catch(typeof(Exception),
                                        Expression.Call(retExp, propGetSetMethod, Expression.Convert(drvalExpCatch, col.CsType))
                                        //Expression.Throw(Expression.Constant(new Exception($"{_commonUtils.QuoteSqlName(col.Attribute.Name)} is NULL，除非设置特性 [Column(IsNullable = false)]")))
                                    )));
                            }
                            else
                            {
                                blockExp.Add(Expression.TryCatch(
                                    Expression.Call(retExp, propGetSetMethod, drvalExp),
                                    Expression.Catch(typeof(Exception),
                                        Expression.Call(retExp, propGetSetMethod, Expression.Default(col.CsType))
                                        //Expression.Throw(Expression.Constant(new Exception($"{_commonUtils.QuoteSqlName(col.Attribute.Name)} is NULL，除非设置特性 [Column(IsNullable = false)]")))
                                    )));
                            }
                        }
                        else
                        {
                            if (drvalType.IsArray || drvalType.IsEnum || Utils.dicExecuteArrayRowReadClassOrTuple.ContainsKey(drvalType))
                            {
                                var drvalExp = Utils.GetDataReaderValueBlockExpression(
                                    col.CsType,
                                    Expression.Call(Utils.MethodDataReaderGetValue, new Expression[] { Expression.Constant(_commonUtils), rowExp, Expression.Constant(colidx) })
                                );
                                blockExp.Add(Expression.Call(retExp, propGetSetMethod, Expression.Convert(drvalExp, col.CsType)));
                            }
                        }
                        colidx++;
                    }
                    if (tb1.TypeLazy != null)
                        blockExp.Add(Expression.Call(retExp, tb1.TypeLazySetOrm, ormExp)); //将 orm 传递给 lazy
                }
                blockExp.AddRange(new Expression[] {
                    Expression.Return(returnTarget, retExp),
                    Expression.Label(returnTarget, Expression.Default(type))
                });
                return new GetAllFieldExpressionTreeInfo
                {
                    Field = field.ToString(),
                    Read = Expression.Lambda<Func<IFreeSql, DbDataReader, T1>>(Expression.Block(new[] { retExp, dataIndexExp, readExp }, blockExp), new[] { ormExp, rowExp }).Compile()
                };
            });
        }
        static MethodInfo _MethodDataReaderIsDBNull = typeof(DbDataReader).GetMethod("IsDBNull", new Type[] { typeof(int) });
        static Dictionary<Type, MethodInfo> _dicMethodDataReaderGetValue = new Dictionary<Type, MethodInfo>
        {
            [typeof(bool)] = typeof(DbDataReader).GetMethod("GetBoolean", new Type[] { typeof(int) }),
            [typeof(int)] = typeof(DbDataReader).GetMethod("GetInt32", new Type[] { typeof(int) }),
            [typeof(long)] = typeof(DbDataReader).GetMethod("GetInt64", new Type[] { typeof(int) }),
            [typeof(double)] = typeof(DbDataReader).GetMethod("GetDouble", new Type[] { typeof(int) }),
            [typeof(float)] = typeof(DbDataReader).GetMethod("GetFloat", new Type[] { typeof(int) }),
            [typeof(decimal)] = typeof(DbDataReader).GetMethod("GetDecimal", new Type[] { typeof(int) }),
            [typeof(DateTime)] = typeof(DbDataReader).GetMethod("GetDateTime", new Type[] { typeof(int) }),
            [typeof(string)] = typeof(DbDataReader).GetMethod("GetString", new Type[] { typeof(int) }),
        };

        protected double InternalAvg(Expression exp)
        {
            var list = this.ToList<double>($"avg({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)}){_commonUtils.FieldAsAlias("as1")}");
            return list.Sum() / list.Count;
        }
        protected TMember InternalMax<TMember>(Expression exp) => this.ToList<TMember>($"max({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)}){_commonUtils.FieldAsAlias("as1")}").Max();
        protected TMember InternalMin<TMember>(Expression exp) => this.ToList<TMember>($"min({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)}){_commonUtils.FieldAsAlias("as1")}").Min();
        protected decimal InternalSum(Expression exp) => this.ToList<decimal>($"sum({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)}){_commonUtils.FieldAsAlias("as1")}").Sum();

        public ISelectGrouping<TKey, TValue> InternalGroupBy<TKey, TValue>(Expression columns)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = -10000; //临时规则，不返回 as1

            _commonExpression.ReadAnonymousField(_tables, field, map, ref index, columns, null, null, _whereGlobalFilter, null, false); //不走 DTO 映射，不处理 IncludeMany
            var sql = field.ToString();
            this.GroupBy(sql.Length > 0 ? sql.Substring(2) : null);
            return new SelectGroupingProvider<TKey, TValue>(_orm, this, map, sql, _commonExpression, _tables);
        }
        public TSelect InternalJoin(Expression exp, SelectTableInfoType joinType)
        {
            _commonExpression.ExpressionJoinLambda(_tables, joinType, exp, null, _whereGlobalFilter);
            return this as TSelect;
        }
        protected TSelect InternalJoin<T2>(Expression exp, SelectTableInfoType joinType)
        {
            var tb = _commonUtils.GetTableByEntity(typeof(T2));
            if (tb == null) throw new ArgumentException("T2 类型错误");
            _tables.Add(new SelectTableInfo { Table = tb, Alias = $"IJ{_tables.Count}", On = null, Type = joinType });
            _commonExpression.ExpressionJoinLambda(_tables, joinType, exp, null, _whereGlobalFilter);
            return this as TSelect;
        }
        protected TSelect InternalOrderBy(Expression column) => this.OrderBy(_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, column, true, null));
        protected TSelect InternalOrderByDescending(Expression column) => this.OrderBy($"{_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, column, true, null)} DESC");

        public List<TReturn> InternalToList<TReturn>(Expression select) => this.ToListMapReader<TReturn>(this.GetExpressionField(select));
        protected string InternalToSql<TReturn>(Expression select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex)
        {
            var af = this.GetExpressionField(select, fieldAlias);
            return this.ToSql(af.field);
        }
        protected string InternalGetInsertIntoToSql<TTargetEntity>(string tableName, Expression select)
        {
            var tb = _orm.CodeFirst.GetTableByEntity(typeof(TTargetEntity));
            if (tb == null) throw new ArgumentException($"ISelect.InsertInto() 类型错误: {typeof(TTargetEntity).DisplayCsharp()}");
            if (string.IsNullOrEmpty(tableName)) tableName = tb.DbName;
            if (_orm.CodeFirst.IsSyncStructureToLower) tableName = tableName.ToLower();
            if (_orm.CodeFirst.IsSyncStructureToUpper) tableName = tableName.ToUpper();

            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = -10000; //临时规则，不返回 as1

            _commonExpression.ReadAnonymousField(_tables, field, map, ref index, select, null, null, _whereGlobalFilter, null, false); //不走 DTO 映射，不处理 IncludeMany
            
            var childs = map.Childs;
            if (childs.Any() == false) throw new ArgumentException($"ISelect.InsertInto() 未选择属性: {typeof(TTargetEntity).DisplayCsharp()}");
            foreach(var col in tb.Columns.Values)
            {
                if (col.Attribute.IsIdentity && string.IsNullOrEmpty(col.DbInsertValue)) continue;
                if (col.Attribute.CanInsert == false) continue;
                if (childs.Any(a => a.CsName == col.CsName)) continue;
                var dbfield = string.IsNullOrWhiteSpace(col.DbInsertValue) == false ? col.DbInsertValue : col.DbDefaultValue;
                childs.Add(new ReadAnonymousTypeInfo { DbField = dbfield, CsName = col.CsName });
            }
            var selectField = string.Join(", ", childs.Select(a => a.DbField));
            var selectSql = this.ToSql(selectField);
            var insertField = string.Join(", ", childs.Select(a => _commonUtils.QuoteSqlName(tb.ColumnsByCs[a.CsName].Attribute.Name)));
            var sql = $"INSERT INTO {_commonUtils.QuoteSqlName(tableName)}({insertField})\r\n{selectSql}";
            return sql;
        }
        public int InternalInsertInto<TTargetEntity>(string tableName, Expression select)
        {
            int ret = 0;
            if (_cancel?.Invoke() == true) return ret;
            var sql = this.InternalGetInsertIntoToSql<TTargetEntity>(tableName, select);
            var dbParms = _params.ToArray();
            var tb = _orm.CodeFirst.GetTableByEntity(typeof(TTargetEntity));
            var before = new Aop.CurdBeforeEventArgs(tb.Type, tb, Aop.CurdType.Insert, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                ret = _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return ret;
        }

        protected DataTable InternalToDataTable(Expression select)
        {
            DataTable ret = null;
            if (_cancel?.Invoke() == true) return ret;
            var sql = this.InternalToSql<int>(select, FieldAliasOptions.AsProperty); //DataTable 使用 AsProperty
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                ret = _orm.Ado.ExecuteDataTable(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return ret;
        }

        protected TReturn InternalToAggregate<TReturn>(Expression select)
        {
            var tmpOrderBy = _orderby;
            _orderby = null; //解决 select count(1) from t order by id 这样的 SQL 错误
            try
            {
                var map = new ReadAnonymousTypeInfo();
                var field = new StringBuilder();
                var index = 0;

                _commonExpression.ReadAnonymousField(_tables, field, map, ref index, select, null, null, _whereGlobalFilter, null, false); //不走 DTO 映射，不处理 IncludeMany
                return this.ToListMapReader<TReturn>(new ReadAnonymousTypeAfInfo(map, field.Length > 0 ? field.Remove(0, 2).ToString() : null)).FirstOrDefault();
            }
            finally
            {
                _orderby = tmpOrderBy;
            }
        }

        public TSelect InternalWhere(Expression exp) => exp == null ? this as TSelect : this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp, null, _whereGlobalFilter, _params));

        #region Async
#if net40
#else
        async public Task<DataTable> ToDataTableAsync(string field, CancellationToken cancellationToken)
        {
            DataTable ret = null;
            if (_cancel?.Invoke() == true) return ret;
            var sql = this.ToSql(field);
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                ret = await _orm.Ado.ExecuteDataTableAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return ret;
        }

        async public Task<List<TTuple>> ToListAsync<TTuple>(string field, CancellationToken cancellationToken)
        {
            var ret = new List<TTuple>();
            if (_cancel?.Invoke() == true) return ret;
            var sql = this.ToSql(field);
            var type = typeof(TTuple);
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                if (type.IsClass)
                    ret = await _orm.Ado.QueryAsync<TTuple>(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
                else
                {
                    var flagStr = $"ToListField:{field}";
                    await _orm.Ado.ExecuteReaderAsync(_connection, _transaction, fetch =>
                    {
                        var read = Utils.ExecuteArrayRowReadClassOrTuple(flagStr, type, null, fetch.Object, 0, _commonUtils);
                        ret.Add((TTuple)read.Value);
                        return Task.FromResult(false);
                    }, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return ret;
        }

        async internal Task<List<T1>> ToListAfPrivateAsync(string sql, GetAllFieldExpressionTreeInfo af, ReadAnonymousTypeOtherInfo[] otherData, CancellationToken cancellationToken)
        {
            var ret = new List<T1>();
            if (_cancel?.Invoke() == true) return ret;
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var retCount = 0;
            Exception exception = null;
            try
            {
                await _orm.Ado.ExecuteReaderAsync(_connection, _transaction, fetch =>
                {
                    ret.Add(af.Read(_orm, fetch.Object));
                    if (otherData != null)
                    {
                        var idx = af.FieldCount - 1;
                        foreach (var other in otherData)
                            other.retlist.Add(_commonExpression.ReadAnonymous(other.read, fetch.Object, ref idx, false, null, retCount, null));
                    }
                    retCount++;
                    return Task.FromResult(false);
                }, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            foreach (var include in _includeToListAsync) await include?.Invoke(ret, cancellationToken);
            _trackToList?.Invoke(ret);
            return ret;
        }

        internal Task<List<T1>> ToListPrivateAsync(GetAllFieldExpressionTreeInfo af, ReadAnonymousTypeOtherInfo[] otherData, CancellationToken cancellationToken)
        {
            string sql = null;
            if (otherData?.Length > 0)
            {
                var sbField = new StringBuilder().Append(af.Field);
                foreach (var other in otherData)
                    sbField.Append(other.field);
                sql = this.ToSql(sbField.ToString());
            }
            else
                sql = this.ToSql(af.Field);

            return ToListAfPrivateAsync(sql, af, otherData, cancellationToken);
        }

        public Task<Dictionary<TKey, T1>> ToDictionaryAsync<TKey>(Func<T1, TKey> keySelector, CancellationToken cancellationToken) => ToDictionaryAsync(keySelector, a => a, cancellationToken);
        async public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>(Func<T1, TKey> keySelector, Func<T1, TElement> elementSelector, CancellationToken cancellationToken)
        {
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null) throw new ArgumentNullException(nameof(elementSelector));

            var ret = new Dictionary<TKey, TElement>();
            if (_cancel?.Invoke() == true) return ret;
            var af = this.GetAllFieldExpressionTreeLevel2();
            var sql = this.ToSql(af.Field);
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                await _orm.Ado.ExecuteReaderAsync(_connection, _transaction, fetch =>
                {
                    var item = af.Read(_orm, fetch.Object);
                    ret.Add(keySelector(item), elementSelector(item));
                    return Task.FromResult(false);
                }, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            if (typeof(TElement) == typeof(T1)) _trackToList?.Invoke(ret.Values);
            return ret;
        }

        async internal Task<List<TReturn>> ToListMrPrivateAsync<TReturn>(string sql, ReadAnonymousTypeAfInfo af, ReadAnonymousTypeOtherInfo[] otherData, CancellationToken cancellationToken)
        {
            var ret = new List<TReturn>();
            if (_cancel?.Invoke() == true) return ret;
            var type = typeof(TReturn);
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var retCount = 0;
            Exception exception = null;
            try
            {
                await _orm.Ado.ExecuteReaderAsync(_connection, _transaction, fetch =>
                {
                    var index = -1;
                    ret.Add((TReturn)_commonExpression.ReadAnonymous(af.map, fetch.Object, ref index, false, null, retCount, af.fillIncludeMany));
                    if (otherData != null)
                        foreach (var other in otherData)
                            other.retlist.Add(_commonExpression.ReadAnonymous(other.read, fetch.Object, ref index, false, null, retCount, null));
                    retCount++;
                    return Task.FromResult(false);
                }, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            if (typeof(TReturn) == typeof(T1))
                foreach (var include in _includeToListAsync) await include?.Invoke(ret, cancellationToken);
            _trackToList?.Invoke(ret);
            return ret;
        }
        internal Task<List<TReturn>> ToListMapReaderPrivateAsync<TReturn>(ReadAnonymousTypeAfInfo af, ReadAnonymousTypeOtherInfo[] otherData, CancellationToken cancellationToken)
        {
            string sql = null;
            if (otherData?.Length > 0)
            {
                var sbField = new StringBuilder().Append(af.field);
                foreach (var other in otherData)
                    sbField.Append(other.field);
                sql = this.ToSql(sbField.ToString());
            }
            else
                sql = this.ToSql(af.field);

            return ToListMrPrivateAsync<TReturn>(sql, af, otherData, cancellationToken);
        }
        protected Task<List<TReturn>> ToListMapReaderAsync<TReturn>(ReadAnonymousTypeAfInfo af, CancellationToken cancellationToken) => ToListMapReaderPrivateAsync<TReturn>(af, null, cancellationToken);

        async protected Task<double> InternalAvgAsync(Expression exp, CancellationToken cancellationToken)
        {
            var list = await this.ToListAsync<double>($"avg({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)}){_commonUtils.FieldAsAlias("as1")}", cancellationToken);
            return list.Sum() / list.Count;
        }
        async protected Task<TMember> InternalMaxAsync<TMember>(Expression exp, CancellationToken cancellationToken) => (await this.ToListAsync<TMember>($"max({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)}){_commonUtils.FieldAsAlias("as1")}", cancellationToken)).Max();
        async protected Task<TMember> InternalMinAsync<TMember>(Expression exp, CancellationToken cancellationToken) => (await this.ToListAsync<TMember>($"min({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)}){_commonUtils.FieldAsAlias("as1")}", cancellationToken)).Min();
        async protected Task<decimal> InternalSumAsync(Expression exp, CancellationToken cancellationToken) => (await this.ToListAsync<decimal>($"sum({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)}){_commonUtils.FieldAsAlias("as1")}", cancellationToken)).Sum();

        protected Task<List<TReturn>> InternalToListAsync<TReturn>(Expression select, CancellationToken cancellationToken) => this.ToListMapReaderAsync<TReturn>(this.GetExpressionField(select), cancellationToken);

        async public Task<int> InternalInsertIntoAsync<TTargetEntity>(string tableName, Expression select, CancellationToken cancellationToken)
        {
            int ret = 0;
            if (_cancel?.Invoke() == true) return ret;
            var sql = this.InternalGetInsertIntoToSql<TTargetEntity>(tableName, select);
            var dbParms = _params.ToArray();
            var tb = _orm.CodeFirst.GetTableByEntity(typeof(TTargetEntity));
            var before = new Aop.CurdBeforeEventArgs(tb.Type, tb, Aop.CurdType.Insert, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                ret = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return ret;
        }

        async protected Task<DataTable> InternalToDataTableAsync(Expression select, CancellationToken cancellationToken)
        {
            DataTable ret = null;
            if (_cancel?.Invoke() == true) return ret;
            var sql = this.InternalToSql<int>(select, FieldAliasOptions.AsProperty); //DataTable 使用 AsProperty
            var dbParms = _params.ToArray();
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                ret = await _orm.Ado.ExecuteDataTableAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return ret;
        }

        async protected Task<TReturn> InternalToAggregateAsync<TReturn>(Expression select, CancellationToken cancellationToken)
        {
            var tmpOrderBy = _orderby;
            _orderby = null; //解决 select count(1) from t order by id 这样的 SQL 错误
            try
            {
                var map = new ReadAnonymousTypeInfo();
                var field = new StringBuilder();
                var index = 0;

                _commonExpression.ReadAnonymousField(_tables, field, map, ref index, select, null, null, _whereGlobalFilter, null, false); //不走 DTO 映射，不处理 IncludeMany
                return (await this.ToListMapReaderAsync<TReturn>(new ReadAnonymousTypeAfInfo(map, field.Length > 0 ? field.Remove(0, 2).ToString() : null), cancellationToken)).FirstOrDefault();
            }
            finally
            {
                _orderby = tmpOrderBy;
            }
        }
#endif
        #endregion
    }
}