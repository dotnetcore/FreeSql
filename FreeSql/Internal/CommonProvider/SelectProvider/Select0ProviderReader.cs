using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FreeSql.Internal.CommonProvider.AdoProvider;

namespace FreeSql.Internal.CommonProvider
{
    partial class Select0Provider<TSelect, T1>
    {
        public DataTable ToDataTableByPropertyName(string[] properties)
        {
            if (properties?.Any() != true) throw new ArgumentException($"{CoreErrorStrings.Properties_Cannot_Null}");
            var sbfield = new StringBuilder();
            for (var propIdx = 0; propIdx < properties.Length; propIdx++)
            {
                var property = properties[propIdx];
                var exp = ConvertStringPropertyToExpression(property);
                if (exp == null) throw new Exception(CoreErrorStrings.Property_Cannot_Find(property));
                var field = _commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, exp, true, _diymemexpWithTempQuery);
                if (propIdx > 0) sbfield.Append(", ");
                sbfield.Append(field);
                //if (field != property)
                sbfield.Append(_commonUtils.FieldAsAlias(_commonUtils.QuoteSqlName("test").Replace("test", property)));
            }
            var sbfieldStr = sbfield.ToString();
            sbfield.Clear();
            return ToDataTable(sbfieldStr);
        }
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

        public List<TTuple> ToList<TTuple>(string field) => ToListQfPrivate<TTuple>(this.ToSql(field), field);
        public List<TTuple> ToListQfPrivate<TTuple>(string sql, string field)
        {
            var ret = new List<TTuple>();
            if (_cancel?.Invoke() == true) return ret;
            if (string.IsNullOrEmpty(sql)) return ret;
            var dbParms = _params.ToArray();
            var type = typeof(TTuple);
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
                            other.retlist.Add(_commonExpression.ReadAnonymous(other.read, fetch.Object, ref idx, false, null, retCount, null, null));
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
            ReadAnonymousTypeOtherInfo csspsod = null;
            if (_SameSelectPendingShareData != null)
            {
                var ods = new List<ReadAnonymousTypeOtherInfo>();
                if (otherData?.Any() == true) ods.AddRange(otherData);
                ods.Add(csspsod = new ReadAnonymousTypeOtherInfo($", {(_SameSelectPendingShareData.Any() && _SameSelectPendingShareData.Last() == null ? _SameSelectPendingShareData.Count - 1 : _SameSelectPendingShareData.Count)}{_commonUtils.FieldAsAlias("fsql_subsel_rowidx")}", new ReadAnonymousTypeInfo { CsType = typeof(int) }, new List<object>()));
                otherData = ods.ToArray();
            }

            string sql = null;
            if (otherData?.Length > 0)
            {
                var sbField = new StringBuilder().Append(af.Field);
                foreach (var other in otherData)
                    sbField.Append(other.field);
                sql = this.ToSql(sbField.ToString().TrimStart(','));
            }
            else
                sql = this.ToSql(af.Field);

            if (SameSelectPending(ref sql, csspsod)) return new List<T1>();
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
                            other.retlist.Add(_commonExpression.ReadAnonymous(other.read, fetch.Object, ref idx, false, null, ret.Object.Count - 1, null, null));
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
                sql = this.ToSql(sbField.ToString().TrimStart(','));
            }
            else
                sql = this.ToSql(af.Field);

            ToListAfChunkPrivate(chunkSize, chunkDone, sql, af, otherData);
        }
        public void ToChunk(int size, Action<FetchCallbackArgs<List<T1>>> done)
        {
            if (_diymemexpWithTempQuery is WithTempQueryParser withTempQueryParser && withTempQueryParser != null)
            {
                if (withTempQueryParser._outsideTable[0] != _tables[0])
                {
                    var tp = Expression.Parameter(_tables[0].Table.Type, _tables[0].Alias);
                    _tables[0].Parameter = tp;
                    this.InternalToChunk<T1>(tp, size, done);
                    return;
                }
                var af = withTempQueryParser._insideSelectList[0].InsideAf;
                this.ToListMrChunkPrivate(size, done, this.ToSql(af.field), af);
                return;
            }
            if (_selectExpression != null) throw new ArgumentException(CoreErrorStrings.Before_Chunk_Cannot_Use_Select);
            this.ToListChunkPrivate(size, done, !_isIncluded ? this.GetAllFieldExpressionTreeLevel2() : this.GetAllFieldExpressionTreeLevelAll(), null);
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
                    ret.Object.Add((TReturn)_commonExpression.ReadAnonymous(af.map, fetch.Object, ref index, false, null, ret.Object.Count, af.fillIncludeMany, af.fillSubSelectMany));
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
            var af = !_isIncluded ? this.GetAllFieldExpressionTreeLevel2() : this.GetAllFieldExpressionTreeLevelAll();
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
            var dbParms = _params.ToArray();
            var type = typeof(TReturn);
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var retCount = 0;
            Exception exception = null;
            try
            {
                _orm.Ado.ExecuteReader(_connection, _transaction, fetch =>
                {
                    var index = -1;
                    ret.Add((TReturn)_commonExpression.ReadAnonymous(af.map, fetch.Object, ref index, false, null, retCount, af.fillIncludeMany, af.fillSubSelectMany));
                    if (otherData != null)
                        foreach (var other in otherData)
                            other.retlist.Add(_commonExpression.ReadAnonymous(other.read, fetch.Object, ref index, false, null, retCount, null, null));
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
            ReadAnonymousTypeOtherInfo csspsod = null;
            if (_SameSelectPendingShareData != null)
            {
                var ods = new List<ReadAnonymousTypeOtherInfo>();
                if (otherData?.Any() == true) ods.AddRange(otherData);
                ods.Add(csspsod = new ReadAnonymousTypeOtherInfo($", {(_SameSelectPendingShareData.Any() && _SameSelectPendingShareData.Last() == null ? _SameSelectPendingShareData.Count - 1 : _SameSelectPendingShareData.Count)}{_commonUtils.FieldAsAlias("fsql_subsel_rowidx")}", new ReadAnonymousTypeInfo { CsType = typeof(int) }, new List<object>()));
                otherData = ods.ToArray();
            }

            string sql = null;
            if (otherData?.Length > 0)
            {
                var sbField = new StringBuilder().Append(af.field);
                foreach (var other in otherData)
                    sbField.Append(other.field);
                sql = this.ToSql(sbField.ToString().TrimStart(','));
            }
            else
                sql = this.ToSql(af.field);

            if (SameSelectPending(ref sql, csspsod)) return new List<TReturn>();
            return ToListMrPrivate<TReturn>(sql, af, otherData);
        }
        protected List<TReturn> ToListMapReader<TReturn>(ReadAnonymousTypeAfInfo af) => ToListMapReaderPrivate<TReturn>(af, null);
        static ConcurrentDictionary<string, GetAllFieldExpressionTreeInfo> _dicGetAllFieldExpressionTree = new ConcurrentDictionary<string, GetAllFieldExpressionTreeInfo>();
        public class GetAllFieldExpressionTreeInfo
        {
            public string Field { get; set; }
            public int FieldCount { get; set; }
            public Func<IFreeSql, DbDataReader, T1> Read { get; set; }
        }
        public GetAllFieldExpressionTreeInfo GetAllFieldExpressionTreeLevelAll()
        {
            return _dicGetAllFieldExpressionTree.GetOrAdd($"*{string.Join("+", _tables.Select(a => $"{_orm.Ado.DataType}-{a.Table.DbName}-{a.Table.Type.FullName}-{a.Alias}-{a.Type}"))}", s =>
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
                        var columnSql = $"{tbi.Alias}.{quoteName}";
                        var rereadSql = _commonUtils.RereadColumn(col, columnSql);
                        field.Append(rereadSql);
                        ++index;
                        if (dicfield.ContainsKey(quoteName)) field.Append(_commonUtils.FieldAsAlias($"as{index}"));
                        else
                        {
                            dicfield.Add(quoteName, true);
                            if (rereadSql != columnSql) field.Append(_commonUtils.FieldAsAlias(quoteName));
                        }
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
                                if (++k >= parentNameSplits.Length)
                                {
                                    iscontinue = true;
                                    break;
                                }
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
        public GetAllFieldExpressionTreeInfo GetAllFieldExpressionTreeLevel2(bool isRereadSql = true)
        {
            if (_diymemexpWithTempQuery != null)
            {
                return new GetAllFieldExpressionTreeInfo
                {
                    Field = "*",
                    Read = (orm, dr) => throw new Exception("GetAllFieldExpressionTreeInfo.Read Is Null")
                };
            }
            if (_selectExpression != null) //ToSql
            {
                var af = this.GetExpressionField(_selectExpression);
                return new GetAllFieldExpressionTreeInfo
                {
                    Field = af.field,
                    Read = (orm, dr) => throw new Exception("GetAllFieldExpressionTreeInfo.Read Is Null")
                };
            }
            if (_OldAuditDataReaderHandler != _orm.Aop.AuditDataReaderHandler)
            {
                _OldAuditDataReaderHandler = _orm.Aop.AuditDataReaderHandler; //清除单表 ExppressionTree
                _dicGetAllFieldExpressionTree.TryRemove($"{_orm.Ado.DataType}-{_tables[0].Table.DbName}-{_tables[0].Table.Type.FullName}-{_tables[0].Alias}-{_tables[0].Type}", out var oldet);
            }
            return _dicGetAllFieldExpressionTree.GetOrAdd(string.Join("+", _tables.Select(a => $"{_orm.Ado.DataType}-{a.Table.DbName}-{a.Table.Type.FullName}-{a.Alias}-{a.Type}-{(isRereadSql ? 1 : 0)}")), s =>
            {
                var tb1 = _tables.First().Table;
                var type = tb1.TypeLazy ?? tb1.Type;
                var props = tb1.Properties;

                if (type == typeof(object) && typeof(T1) == typeof(object))
                {
                    return new GetAllFieldExpressionTreeInfo
                    {
                        Field = "*",
                        Read = (orm, dr) =>
                        {
                            //dynamic expando = new DynamicDictionary(); //动态类型字段 可读可写
                            var expandodic = new Dictionary<string, object>();// (IDictionary<string, object>)expando;
                            var fc = dr.FieldCount;
                            for (var a = 0; a < fc; a++)
                            {
                                var name = dr.GetName(a);
                                //expando[name] = row2.GetValue(a);
                                if (expandodic.ContainsKey(name)) continue;
                                expandodic.Add(name, Utils.InternalDataReaderGetValue(_commonUtils, dr, a, null));
                            }
                            //expando = expandodic;
                            return (T1)((object)expandodic);
                        }
                    };
                }

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
                        var columnSql = $"{tb.Alias}.{quoteName}";
                        var rereadSql = isRereadSql ? _commonUtils.RereadColumn(col, columnSql) : columnSql;
                        field.Append(rereadSql);
                        ++index;
                        if (dicfield.ContainsKey(quoteName)) field.Append(_commonUtils.FieldAsAlias($"as{index}"));
                        else
                        {
                            dicfield.Add(quoteName, true);
                            if (rereadSql != columnSql) field.Append(_commonUtils.FieldAsAlias(quoteName));
                        }
                    }
                    else
                    {
                        var tb2 = _tables.Where((a, b) => b > 0 &&
                            (a.Type == SelectTableInfoType.InnerJoin || a.Type == SelectTableInfoType.LeftJoin || a.Type == SelectTableInfoType.RightJoin) &&
                            string.IsNullOrEmpty(a.On) == false &&
                            a.Alias.StartsWith($"{tb.Alias}__") && //开头结尾完全匹配
                            a.Alias.EndsWith($"__{prop.Name}") //不清楚会不会有其他情况 求大佬优化
                            ).FirstOrDefault(); //判断 b > 0 防止 parent 递归关系
                        if (tb2 == null && props.Where(pw => pw.Value.PropertyType == prop.PropertyType).Take(2).Count() == 1)
                            tb2 = _tables.Where((a, b) => b > 0 &&
                                (a.Type == SelectTableInfoType.InnerJoin || a.Type == SelectTableInfoType.LeftJoin || a.Type == SelectTableInfoType.RightJoin) &&
                                string.IsNullOrEmpty(a.On) == false &&
                                a.Table.Type == prop.PropertyType).FirstOrDefault();
                        if (tb2 == null) continue;
                        foreach (var col2 in tb2.Table.Columns.Values)
                        {
                            if (index > 0) field.Append(", ");
                            var quoteName = _commonUtils.QuoteSqlName(col2.Attribute.Name);
                            var columnSql = $"{tb2.Alias}.{quoteName}";
                            var rereadSql = isRereadSql ? _commonUtils.RereadColumn(col2, columnSql) : columnSql;
                            field.Append(rereadSql);
                            ++index;
                            ++otherindex;
                            if (dicfield.ContainsKey(quoteName)) field.Append(_commonUtils.FieldAsAlias($"as{index}"));
                            else
                            {
                                dicfield.Add(quoteName, true);
                                if (rereadSql != columnSql) field.Append(_commonUtils.FieldAsAlias(quoteName));
                            }
                        }
                    }
                    //只读到二级属性
                    var propGetSetMethod = prop.GetSetMethod(true);
                    Expression readExpAssign = null; //加速缓存
                    if (prop.PropertyType.IsArray) readExpAssign = Expression.New(Utils.RowInfo.Constructor,
                        Utils.GetDataReaderValueBlockExpression(prop.PropertyType, Expression.Call(Utils.MethodDataReaderGetValue, new Expression[] { Expression.Constant(_commonUtils), rowExp, dataIndexExp, Expression.Constant(prop) })),
                        Expression.Add(dataIndexExp, Expression.Constant(1))
                    );
                    else
                    {
                        var proptypeGeneric = prop.PropertyType;
                        if (proptypeGeneric.IsNullableType()) proptypeGeneric = proptypeGeneric.GetGenericArguments().First();
                        if (proptypeGeneric.IsEnum ||
                            Utils.dicExecuteArrayRowReadClassOrTuple.ContainsKey(proptypeGeneric)) readExpAssign = Expression.New(Utils.RowInfo.Constructor,
                                Utils.GetDataReaderValueBlockExpression(prop.PropertyType, Expression.Call(Utils.MethodDataReaderGetValue, new Expression[] { Expression.Constant(_commonUtils), rowExp, dataIndexExp, Expression.Constant(prop) })),
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
                        var colprop = tb.Table.Properties[col.CsName];
                        var propGetSetMethod = colprop.GetSetMethod(true);
                        if (col.CsType == col.Attribute.MapType &&
                            _orm.Aop.AuditDataReaderHandler == null &&
                            _dicMethodDataReaderGetValue.TryGetValue(col.Attribute.MapType.NullableTypeOrThis(), out var drGetValueMethod))
                        {
                            if (_dicMethodDataReaderGetValueOverride.TryGetValue(_orm.Ado.DataType, out var drDictOverride) && drDictOverride.TryGetValue(col.Attribute.MapType.NullableTypeOrThis(), out var drDictOverrideGetValueMethod))
                                drGetValueMethod = drDictOverrideGetValueMethod;

                            Expression drvalExp = Expression.Call(rowExp, drGetValueMethod, Expression.Constant(colidx));
                            if (col.CsType.IsNullableType() || drGetValueMethod.ReturnType != col.CsType) drvalExp = Expression.Convert(drvalExp, col.CsType);
                            drvalExp = Expression.Condition(Expression.Call(rowExp, _MethodDataReaderIsDBNull, Expression.Constant(colidx)), Expression.Default(col.CsType), drvalExp);

                            if (drvalType.IsArray || drvalType.IsEnum || Utils.dicExecuteArrayRowReadClassOrTuple.ContainsKey(drvalType))
                            {
                                var drvalExpCatch = Utils.GetDataReaderValueBlockExpression(
                                    col.CsType,
                                    Expression.Call(Utils.MethodDataReaderGetValue, new Expression[] { Expression.Constant(_commonUtils), rowExp, Expression.Constant(colidx), Expression.Constant(colprop) })
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
                                    Expression.Call(Utils.MethodDataReaderGetValue, new Expression[] { Expression.Constant(_commonUtils), rowExp, Expression.Constant(colidx), Expression.Constant(colprop) })
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
                    FieldCount = index,
                    Read = Expression.Lambda<Func<IFreeSql, DbDataReader, T1>>(Expression.Block(new[] { retExp, dataIndexExp, readExp }, blockExp), new[] { ormExp, rowExp }).Compile()
                };
            });
        }

        protected double InternalAvg(Expression exp)
        {
            var field = $"avg({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, exp, true, _diymemexpWithTempQuery)}){_commonUtils.FieldAsAlias("as1")}";
            if (_limit <= 0 && _skip <= 0)
            {
                var list = this.ToList<double>(field);
                return list.Sum() / list.Count;
            }

            var sql = GetNestSelectSql(exp, field, ToSql);
            var list2 = ToListQfPrivate<double>(sql, field);
            return list2.Sum() / list2.Count;
        }
        protected TMember InternalMax<TMember>(Expression exp)
        {
            var field = $"max({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, exp, true, _diymemexpWithTempQuery)}){_commonUtils.FieldAsAlias("as1")}";
            if (_limit <= 0 && _skip <= 0) return this.ToList<TMember>(field).Max();

            var sql = GetNestSelectSql(exp, field, ToSql);
            return ToListQfPrivate<TMember>(sql, field).Max();
        }
        protected TMember InternalMin<TMember>(Expression exp)
        {
            var field = $"min({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, exp, true, _diymemexpWithTempQuery)}){_commonUtils.FieldAsAlias("as1")}";
            if (_limit <= 0 && _skip <= 0) return this.ToList<TMember>(field).Min();

            var sql = GetNestSelectSql(exp, field, ToSql);
            return ToListQfPrivate<TMember>(sql, field).Min();
        }
        protected decimal InternalSum(Expression exp)
        {
            var field = $"sum({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, exp, true, _diymemexpWithTempQuery)}){_commonUtils.FieldAsAlias("as1")}";
            if (_limit <= 0 && _skip <= 0) return this.ToList<decimal>(field).Sum();

            var sql = GetNestSelectSql(exp, field, ToSql);
            return ToListQfPrivate<decimal>(sql, field).Sum();
        }

        public ISelectGrouping<TKey, TValue> InternalGroupBy<TKey, TValue>(Expression columns)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = CommonExpression.ReadAnonymousFieldAsCsNameGroupBy; //临时规则，不返回 as1

            _commonExpression.ReadAnonymousField(_tables, _tableRule, field, map, ref index, columns, null, _diymemexpWithTempQuery, _whereGlobalFilter, null, null, false); //不走 DTO 映射，不处理 IncludeMany
            var sql = field.ToString();
            this.GroupBy(sql.Length > 0 ? sql.Substring(2) : null);
            return new SelectGroupingProvider<TKey, TValue>(_orm, this, map, sql, _commonExpression, _tables);
        }
        public TSelect InternalGroupBySelf(Expression column)
        {
            _groupBySelfFlag = true;
            if (column.NodeType == ExpressionType.Lambda) column = (column as LambdaExpression)?.Body;
            switch (column?.NodeType)
            {
                case ExpressionType.New:
                    var newExp = column as NewExpression;
                    if (newExp == null) break;
                    this.GroupBy(string.Join(", ", newExp.Members.Select((b, a) => _commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, newExp.Arguments[a], true, _diymemexpWithTempQuery))));
                    return this as TSelect;
            }
            return this.GroupBy(_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, column, true, _diymemexpWithTempQuery));
        }
        public TSelect InternalJoin(Expression exp, SelectTableInfoType joinType)
        {
            _commonExpression.ExpressionJoinLambda(_tables, _tableRule, joinType, exp, _diymemexpWithTempQuery, _whereGlobalFilter);
            return this as TSelect;
        }
        protected TSelect InternalJoin<T2>(Expression exp, SelectTableInfoType joinType)
        {
            var tb = _commonUtils.GetTableByEntity(typeof(T2));
            if (tb == null) throw new ArgumentException(CoreErrorStrings.T2_Type_Error);
            _tables.Add(new SelectTableInfo { Table = tb, Alias = $"IJ{_tables.Count}", On = null, Type = joinType });
            _commonExpression.ExpressionJoinLambda(_tables, _tableRule, joinType, exp, _diymemexpWithTempQuery, _whereGlobalFilter);
            return this as TSelect;
        }
        public TSelect InternalOrderBy(Expression column)
        {
            if (column.NodeType == ExpressionType.Lambda) column = (column as LambdaExpression)?.Body;
            switch (column?.NodeType)
            {
                case ExpressionType.New:
                    var newExp = column as NewExpression;
                    if (newExp == null) break;
                    for (var a = 0; a < newExp.Members.Count; a++) this.OrderBy(_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, newExp.Arguments[a], true, _diymemexpWithTempQuery));
                    return this as TSelect;
            }
            return this.OrderBy(_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, column, true, _diymemexpWithTempQuery));
        }
        public TSelect InternalOrderByDescending(Expression column)
        {
            if (column.NodeType == ExpressionType.Lambda) column = (column as LambdaExpression)?.Body;
            switch (column?.NodeType)
            {
                case ExpressionType.New:
                    var newExp = column as NewExpression;
                    if (newExp == null) break;
                    for (var a = 0; a < newExp.Members.Count; a++) this.OrderBy($"{_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, newExp.Arguments[a], true, _diymemexpWithTempQuery)} DESC");
                    return this as TSelect;
            }
            return this.OrderBy($"{_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, column, true, _diymemexpWithTempQuery)} DESC");
        }

        public List<TReturn> InternalToList<TReturn>(Expression select)
        {
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = 0;
            var findSubSelectMany = new List<Expression>();

            _commonExpression.ReadAnonymousField(_tables, _tableRule, field, map, ref index, select, this, _diymemexpWithTempQuery, _whereGlobalFilter, null, findSubSelectMany, _groupBySelfFlag == false);
            var af = new ReadAnonymousTypeAfInfo(map, field.Length > 0 ? field.Remove(0, 2).ToString() : null);
            if (findSubSelectMany.Any() == false) return this.ToListMapReaderPrivate<TReturn>(af, new ReadAnonymousTypeOtherInfo[0]);

            af.fillSubSelectMany = new List<NativeTuple<Expression, IList, int>>();
            //查询 SubSelectMany
            var otherAfmanys = findSubSelectMany.Select(a =>
            {
                var vst = new FindAllMemberExpressionVisitor(this);
                vst.Visit(a);
                var finds = vst.Result;

                var afs = new List<NativeTuple<MemberExpression, ColumnInfo, ReadAnonymousTypeOtherInfo>>();
                foreach (var find in finds)
                {
                    var otherMap = new ReadAnonymousTypeInfo();
                    field.Clear();
                    _commonExpression.ReadAnonymousField(_tables, _tableRule, field, otherMap, ref index, find.Item1, this, _diymemexpWithTempQuery, _whereGlobalFilter, null, null, _groupBySelfFlag == false);
                    var otherRet = new List<object>();
                    var otherAf = new ReadAnonymousTypeOtherInfo(field.ToString(), otherMap, otherRet);
                    afs.Add(NativeTuple.Create(find.Item1, find.Item2, otherAf));
                }
                return afs;
            }).ToList();
            var otherAfdic = otherAfmanys.SelectMany(a => a).GroupBy(a => a.Item1.ToString()).ToDictionary(a => a.Key, a => a.ToList());
            var otherAfs = otherAfdic.Select(a => a.Value.First().Item3).ToArray();
            var ret = this.ToListMapReaderPrivate<TReturn>(af, otherAfs);
            if (ret.Any() == false || otherAfmanys.Any() == false) return ret;

            var rmev = new ReplaceMemberExpressionVisitor();

            for (var a = 0; a < otherAfmanys.Count; a++)
            {
                if (otherAfmanys[a].Any() == false)
                {
                    var otherList = Expression.Lambda(findSubSelectMany[a]).Compile().DynamicInvoke() as IEnumerable;
                    foreach (var otherListItem in otherList)
                        for (int b = a, c = 0; b < af.fillSubSelectMany?.Count; b += otherAfmanys.Count, c++)
                            af.fillSubSelectMany[b].Item2.Add(otherListItem);
                    continue;
                }
                var sspShareData = new List<NativeTuple<string, DbParameter[], ReadAnonymousTypeOtherInfo>>();
                try
                {
                    var newexp = findSubSelectMany[a];
                    var newexpParms = otherAfmanys[a].Select(d =>
                    {
                        var newexpParm = Expression.Parameter(d.Item1.Type);
                        newexp = rmev.Replace(newexp, d.Item1, newexpParm);
                        return newexpParm;
                    }).ToArray();
                    newexp = SetSameSelectPendingShareDataWithExpression(newexp, sspShareData);
                    var newexpFunc = Expression.Lambda(newexp, newexpParms).Compile();

                    var newexpParamVals = otherAfmanys[a].Select(d => otherAfdic[d.Item1.ToString()].First().Item3.retlist).ToArray();
                    for (int b = a, c = 0; b < af.fillSubSelectMany?.Count; b += otherAfmanys.Count, c++)
                    {
                        var vals = newexpParamVals.Select(d => d[c]).ToArray();
                        if (c == ret.Count - 1) sspShareData.Add(null); //flush flag
                        var diret = newexpFunc.DynamicInvoke(vals);
                        if (c < ret.Count - 1) continue;
                        var otherList = diret as IEnumerable;
                        var retlistidx = 0;
                        foreach (var otherListItem in otherList)
                        {
                            var retlist = sspShareData[0].Item3.retlist;
                            while (retlistidx >= retlist.Count)
                            {
                                sspShareData.RemoveAt(0);
                                retlist = sspShareData[0].Item3.retlist;
                                retlistidx = 0;
                            }
                            int.TryParse(retlist[retlistidx++]?.ToString(), out var tryrowidx);
                            af.fillSubSelectMany[tryrowidx * otherAfmanys.Count + a].Item2.Add(otherListItem);
                        }
                    }
                }
                finally
                {
                    sspShareData.Clear();
                }
            }
            return ret;
        }
        protected string InternalToSql<TReturn>(Expression select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex)
        {
            var af = this.GetExpressionField(select, fieldAlias);
            return this.ToSql(af.field);
        }
        protected string InternalGetInsertIntoToSql<TTargetEntity>(string tableName, Expression select)
        {
            var tb = _orm.CodeFirst.GetTableByEntity(typeof(TTargetEntity));
            if (tb == null) throw new ArgumentException(CoreErrorStrings.InsertInto_TypeError(typeof(TTargetEntity).DisplayCsharp()));
            if (string.IsNullOrEmpty(tableName)) tableName = tb.DbName;
            if (_orm.CodeFirst.IsSyncStructureToLower) tableName = tableName.ToLower();
            if (_orm.CodeFirst.IsSyncStructureToUpper) tableName = tableName.ToUpper();
            if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(TTargetEntity), tableName);

            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = -10000; //临时规则，不返回 as1

            _commonExpression.ReadAnonymousField(_tables, _tableRule, field, map, ref index, select, null, _diymemexpWithTempQuery, _whereGlobalFilter, null, null, false); //不走 DTO 映射，不处理 IncludeMany
            
            var childs = map.Childs;
            if (childs.Any() == false) throw new ArgumentException(CoreErrorStrings.InsertInto_No_Property_Selected(typeof(TTargetEntity).DisplayCsharp()));
            foreach (var col in tb.Columns.Values)
            {
                if (col.Attribute.IsIdentity && string.IsNullOrEmpty(col.DbInsertValue)) continue;
                if (col.Attribute.CanInsert == false) continue;
                if (childs.Any(a => a.CsName == col.CsName)) continue;
                var dbfield = string.IsNullOrWhiteSpace(col.DbInsertValue) == false ? col.DbInsertValue : col.DbDefaultValue;
                childs.Add(new ReadAnonymousTypeInfo { DbField = dbfield, CsName = col.CsName });
            }
            var selectField = string.Join(", ", childs.Select(a => a.DbField));
            var cteWithSql = "";
            if (_is_AsTreeCte && this._select.TrimStart().StartsWith("WITH") && this._select.EndsWith("SELECT "))
            {
                cteWithSql = this._select.Substring(0, this._select.Length - 7);
                this._select = "SELECT ";
            }
            var selectSql = this.ToSql(selectField);
            var insertField = string.Join(", ", childs.Select(a => _commonUtils.QuoteSqlName(tb.ColumnsByCs[a.CsName].Attribute.Name)));
            var sql = $"{cteWithSql}INSERT INTO {_commonUtils.QuoteSqlName(tableName)}({insertField})\r\n{selectSql}";
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

                _commonExpression.ReadAnonymousField(_tables, _tableRule, field, map, ref index, select, null, _diymemexpWithTempQuery, _whereGlobalFilter, null, null, false); //不走 DTO 映射，不处理 IncludeMany
                var af = new ReadAnonymousTypeAfInfo(map, field.Length > 0 ? field.Remove(0, 2).ToString() : null);
                if (GetTableRuleUnions().Count <= 1) return this.ToListMapReader<TReturn>(af).FirstOrDefault();

                var sql = GetNestSelectSql(select, af.field, ToSql);
                return ToListMrPrivate<TReturn>(sql, af, null).FirstOrDefault();
            }
            finally
            {
                _orderby = tmpOrderBy;
            }
        }

        public TSelect InternalWhere(Expression exp) => exp == null ? this as TSelect : this.Where(_commonExpression.ExpressionWhereLambda(_tables, _tableRule, exp, _diymemexpWithTempQuery, _whereGlobalFilter, _params));

        #region Async
#if net40
#else
        public Task<DataTable> ToDataTableByPropertyNameAsync(string[] properties, CancellationToken cancellationToken)
        {
            if (properties?.Any() != true) throw new ArgumentException($"{CoreErrorStrings.Properties_Cannot_Null}");
            var sbfield = new StringBuilder();
            for (var propIdx = 0; propIdx < properties.Length; propIdx++)
            {
                var property = properties[propIdx];
                var exp = ConvertStringPropertyToExpression(property);
                if (exp == null) throw new Exception(CoreErrorStrings.Property_Cannot_Find(property));
                var field = _commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, exp, true, _diymemexpWithTempQuery);
                if (propIdx > 0) sbfield.Append(", ");
                sbfield.Append(field);
                //if (field != property)
                sbfield.Append(_commonUtils.FieldAsAlias(_commonUtils.QuoteSqlName("test").Replace("test", property)));
            }
            var sbfieldStr = sbfield.ToString();
            sbfield.Clear();
            return ToDataTableAsync(sbfieldStr, cancellationToken);
        }
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

        public Task<List<TTuple>> ToListAsync<TTuple>(string field, CancellationToken cancellationToken) => ToListQfPrivateAsync<TTuple>(this.ToSql(field), field, cancellationToken);
        async public Task<List<TTuple>> ToListQfPrivateAsync<TTuple>(string sql, string field, CancellationToken cancellationToken)
        {
            var ret = new List<TTuple>();
            if (_cancel?.Invoke() == true) return ret;
            if (string.IsNullOrEmpty(sql)) return ret;
            var dbParms = _params.ToArray();
            var type = typeof(TTuple);
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
                            other.retlist.Add(_commonExpression.ReadAnonymous(other.read, fetch.Object, ref idx, false, null, retCount, null, null));
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
            ReadAnonymousTypeOtherInfo csspsod = null;
            if (_SameSelectPendingShareData != null)
            {
                var ods = new List<ReadAnonymousTypeOtherInfo>();
                if (otherData?.Any() == true) ods.AddRange(otherData);
                ods.Add(csspsod = new ReadAnonymousTypeOtherInfo($", {(_SameSelectPendingShareData.Any() && _SameSelectPendingShareData.Last() == null ? _SameSelectPendingShareData.Count - 1 : _SameSelectPendingShareData.Count)}{_commonUtils.FieldAsAlias("fsql_subsel_rowidx")}", new ReadAnonymousTypeInfo { CsType = typeof(int) }, new List<object>()));
                otherData = ods.ToArray();
            }

            string sql = null;
            if (otherData?.Length > 0)
            {
                var sbField = new StringBuilder().Append(af.Field);
                foreach (var other in otherData)
                    sbField.Append(other.field);
                sql = this.ToSql(sbField.ToString().TrimStart(','));
            }
            else
                sql = this.ToSql(af.Field);

            if (SameSelectPending(ref sql, csspsod)) return Task.FromResult(new List<T1>());
            return ToListAfPrivateAsync(sql, af, otherData, cancellationToken);
        }
        #region ToChunkAsync
        async internal Task ToListMrChunkPrivateAsync<TReturn>(int chunkSize, Func<FetchCallbackArgs<List<TReturn>>, Task> chunkDone, string sql, ReadAnonymousTypeAfInfo af, CancellationToken cancellationToken)
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
                await _orm.Ado.ExecuteReaderAsync(_connection, _transaction, async fetch =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        fetch.IsBreak = true;
                        return;
                    }
                    var index = -1;
                    ret.Object.Add((TReturn)_commonExpression.ReadAnonymous(af.map, fetch.Object, ref index, false, null, ret.Object.Count, af.fillIncludeMany, af.fillSubSelectMany));
                    retCount++;
                    if (chunkSize > 0 && chunkSize == ret.Object.Count)
                    {
                        checkDoneTimes++;

                        foreach (var include in _includeToListAsync) await include?.Invoke(ret.Object, cancellationToken);
                        _trackToList?.Invoke(ret.Object);
                        await chunkDone(ret);
                        fetch.IsBreak = ret.IsBreak;

                        ret.Object.Clear();
                    }
                }, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
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
                foreach (var include in _includeToListAsync) await include?.Invoke(ret.Object, cancellationToken);
                _trackToList?.Invoke(ret.Object);
                await chunkDone(ret);
            }
        }
        public Task InternalToChunkAsync<TReturn>(Expression select, int size, Func<FetchCallbackArgs<List<TReturn>>, Task> done, CancellationToken cancellationToken)
        {
            var af = this.GetExpressionField(select);
            var sql = this.ToSql(af.field);
            return this.ToListMrChunkPrivateAsync<TReturn>(size, done, sql, af, cancellationToken);
        }
        #endregion

        public Task<Dictionary<TKey, T1>> ToDictionaryAsync<TKey>(Func<T1, TKey> keySelector, CancellationToken cancellationToken) => ToDictionaryAsync(keySelector, a => a, cancellationToken);
        async public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>(Func<T1, TKey> keySelector, Func<T1, TElement> elementSelector, CancellationToken cancellationToken)
        {
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null) throw new ArgumentNullException(nameof(elementSelector));

            var ret = new Dictionary<TKey, TElement>();
            if (_cancel?.Invoke() == true) return ret;
            var af = !_isIncluded ? this.GetAllFieldExpressionTreeLevel2() : this.GetAllFieldExpressionTreeLevelAll();
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
            var dbParms = _params.ToArray();
            var type = typeof(TReturn);
            var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var retCount = 0;
            Exception exception = null;
            try
            {
                await _orm.Ado.ExecuteReaderAsync(_connection, _transaction, fetch =>
                {
                    var index = -1;
                    ret.Add((TReturn)_commonExpression.ReadAnonymous(af.map, fetch.Object, ref index, false, null, retCount, af.fillIncludeMany, af.fillSubSelectMany));
                    if (otherData != null)
                        foreach (var other in otherData)
                            other.retlist.Add(_commonExpression.ReadAnonymous(other.read, fetch.Object, ref index, false, null, retCount, null, null));
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
            ReadAnonymousTypeOtherInfo csspsod = null;
            if (_SameSelectPendingShareData != null)
            {
                var ods = new List<ReadAnonymousTypeOtherInfo>();
                if (otherData?.Any() == true) ods.AddRange(otherData);
                ods.Add(csspsod = new ReadAnonymousTypeOtherInfo($", {(_SameSelectPendingShareData.Any() && _SameSelectPendingShareData.Last() == null ? _SameSelectPendingShareData.Count - 1 : _SameSelectPendingShareData.Count)}{_commonUtils.FieldAsAlias("fsql_subsel_rowidx")}", new ReadAnonymousTypeInfo { CsType = typeof(int) }, new List<object>()));
                otherData = ods.ToArray();
            }

            string sql = null;
            if (otherData?.Length > 0)
            {
                var sbField = new StringBuilder().Append(af.field);
                foreach (var other in otherData)
                    sbField.Append(other.field);
                sql = this.ToSql(sbField.ToString().TrimStart(','));
            }
            else
                sql = this.ToSql(af.field);

            if (SameSelectPending(ref sql, csspsod)) return Task.FromResult(new List<TReturn>());
            return ToListMrPrivateAsync<TReturn>(sql, af, otherData, cancellationToken);
        }
        protected Task<List<TReturn>> ToListMapReaderAsync<TReturn>(ReadAnonymousTypeAfInfo af, CancellationToken cancellationToken) => ToListMapReaderPrivateAsync<TReturn>(af, null, cancellationToken);

        async protected Task<double> InternalAvgAsync(Expression exp, CancellationToken cancellationToken)
        {
            var field = $"avg({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, exp, true, _diymemexpWithTempQuery)}){_commonUtils.FieldAsAlias("as1")}";
            if (_limit <= 0 && _skip <= 0)
            {
                var list = await this.ToListAsync<double>(field, cancellationToken);
                return list.Sum() / list.Count;
            }

            var sql = GetNestSelectSql(exp, field, ToSql);
            var list2 = await ToListQfPrivateAsync<double>(sql, field, cancellationToken);
            return list2.Sum() / list2.Count;
        }
        async protected Task<TMember> InternalMaxAsync<TMember>(Expression exp, CancellationToken cancellationToken)
        {
            var field = $"max({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, exp, true, _diymemexpWithTempQuery)}){_commonUtils.FieldAsAlias("as1")}";
            if (_limit <= 0 && _skip <= 0) return (await this.ToListAsync<TMember>(field, cancellationToken)).Max();

            var sql = GetNestSelectSql(exp, field, ToSql);
            return (await ToListQfPrivateAsync<TMember>(sql, field, cancellationToken)).Max();
        }
        async protected Task<TMember> InternalMinAsync<TMember>(Expression exp, CancellationToken cancellationToken)
        {
            var field = $"min({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, exp, true, _diymemexpWithTempQuery)}){_commonUtils.FieldAsAlias("as1")}";
            if (_limit <= 0 && _skip <= 0) return (await this.ToListAsync<TMember>(field, cancellationToken)).Min();

            var sql = GetNestSelectSql(exp, field, ToSql);
            return (await ToListQfPrivateAsync<TMember>(sql, field, cancellationToken)).Min();
        }
        async protected Task<decimal> InternalSumAsync(Expression exp, CancellationToken cancellationToken)
        {
            var field = $"sum({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, _tableRule, null, SelectTableInfoType.From, exp, true, _diymemexpWithTempQuery)}){_commonUtils.FieldAsAlias("as1")}";
            if (_limit <= 0 && _skip <= 0) return (await this.ToListAsync<decimal>(field, cancellationToken)).Sum();

            var sql = GetNestSelectSql(exp, field, ToSql);
            return (await ToListQfPrivateAsync<decimal>(sql, field, cancellationToken)).Sum();
        }

        static ConcurrentDictionary<Type, MethodInfo[]> _dicGetMethodsByName = new ConcurrentDictionary<Type, MethodInfo[]>();
        async protected Task<List<TReturn>> InternalToListAsync<TReturn>(Expression select, CancellationToken cancellationToken)
        {
            //【注意】：此异步有特别逻辑，因为要处理子查询集合 ToList -> ToListAsync，原因是 LambdaExpression 表达式树内不支持 await Async
            var map = new ReadAnonymousTypeInfo();
            var field = new StringBuilder();
            var index = 0;
            var findSubSelectMany = new List<Expression>();

            _commonExpression.ReadAnonymousField(_tables, _tableRule, field, map, ref index, select, this, _diymemexpWithTempQuery, _whereGlobalFilter, null, findSubSelectMany, _groupBySelfFlag == false);
            var af = new ReadAnonymousTypeAfInfo(map, field.Length > 0 ? field.Remove(0, 2).ToString() : null);
            if (findSubSelectMany.Any() == false) return await this.ToListMapReaderPrivateAsync<TReturn>(af, new ReadAnonymousTypeOtherInfo[0], cancellationToken);

            af.fillSubSelectMany = new List<NativeTuple<Expression, IList, int>>();
            //查询 SubSelectMany
            var otherAfmanys = findSubSelectMany.Select(a =>
            {
                var vst = new FindAllMemberExpressionVisitor(this);
                vst.Visit(a);
                var finds = vst.Result;

                var afs = new List<NativeTuple<MemberExpression, ColumnInfo, ReadAnonymousTypeOtherInfo>>();
                foreach (var find in finds)
                {
                    var otherMap = new ReadAnonymousTypeInfo();
                    field.Clear();
                    _commonExpression.ReadAnonymousField(_tables, _tableRule, field, otherMap, ref index, find.Item1, this, _diymemexpWithTempQuery, _whereGlobalFilter, null, null, _groupBySelfFlag == false);
                    var otherRet = new List<object>();
                    var otherAf = new ReadAnonymousTypeOtherInfo(field.ToString(), otherMap, otherRet);
                    afs.Add(NativeTuple.Create(find.Item1, find.Item2, otherAf));
                }
                return afs;
            }).ToList();
            var otherAfdic = otherAfmanys.SelectMany(a => a).GroupBy(a => a.Item1.ToString()).ToDictionary(a => a.Key, a => a.ToList());
            var otherAfs = otherAfdic.Select(a => a.Value.First().Item3).ToArray();
            var ret = await this.ToListMapReaderPrivateAsync<TReturn>(af, otherAfs, cancellationToken);
            if (ret.Any() == false || otherAfmanys.Any() == false) return ret;

            var rmev = new ReplaceMemberExpressionVisitor();

            for (var a = 0; a < otherAfmanys.Count; a++)
            {
                if (otherAfmanys[a].Any() == false)
                {
                    var otherList = Expression.Lambda(findSubSelectMany[a]).Compile().DynamicInvoke() as IEnumerable;
                    foreach (var otherListItem in otherList)
                        for (int b = a, c = 0; b < af.fillSubSelectMany?.Count; b += otherAfmanys.Count, c++)
                            af.fillSubSelectMany[b].Item2.Add(otherListItem);
                    continue;
                }
                var sspShareData = new List<NativeTuple<string, DbParameter[], ReadAnonymousTypeOtherInfo>>();
                try
                {
                    var newexp = findSubSelectMany[a];
                    var newexpParms = otherAfmanys[a].Select(d =>
                    {
                        var newexpParm = Expression.Parameter(d.Item1.Type);
                        newexp = rmev.Replace(newexp, d.Item1, newexpParm);
                        return newexpParm;
                    }).ToArray();
                    var newexpCallExp = (newexp as MethodCallExpression);
                    if (newexpCallExp?.Object != null) {
                        var asyncMethods = _dicGetMethodsByName.GetOrAdd(newexpCallExp.Object.Type, dgmbn => dgmbn.GetMethods().Where(c => c.Name == $"{newexpCallExp.Method.Name}Async")
                            .Concat(dgmbn.GetInterfaces().SelectMany(b => b.GetMethods().Where(c => c.Name == $"{newexpCallExp.Method.Name}Async"))).ToArray());
                        var asyncMethod = asyncMethods.Length == 1 ? asyncMethods.First() : null;
                        var newexpMethodGenericArgs = newexpCallExp.Method.GetGenericArguments();
                        var newexpMethodParmArgs = newexpCallExp.Method.GetParameters();
                        if (asyncMethods.Length > 1)
                        {
                            asyncMethods = asyncMethods
                                .Where(b =>
                                {
                                    var bGenericArgs = b.GetGenericArguments();
                                    return bGenericArgs.Length == newexpMethodGenericArgs.Length;
                                })
                                .Select(b => newexpMethodGenericArgs.Length == 0 ? b : b.MakeGenericMethod(newexpMethodGenericArgs))
                                .Where(b =>
                                {
                                    var bParmArgs = b.GetParameters();
                                    return bParmArgs.Length - 1 == newexpMethodParmArgs.Length && newexpMethodParmArgs.Where((c, d) => c.ParameterType == bParmArgs[d].ParameterType).Count() == newexpMethodParmArgs.Length;
                                }).ToArray();
                            if (asyncMethods.Length == 1) asyncMethod = asyncMethods.First();
                        }
                        if (asyncMethod != null)
                            newexp = Expression.Call(newexpCallExp.Object, asyncMethod, newexpCallExp.Arguments.Concat(new[] { Expression.Constant(cancellationToken, typeof(CancellationToken)) }).ToArray());
                    }
                    newexp = SetSameSelectPendingShareDataWithExpression(newexp, sspShareData);
                    var newexpFunc = Expression.Lambda(newexp, newexpParms).Compile();

                    var newexpParamVals = otherAfmanys[a].Select(d => otherAfdic[d.Item1.ToString()].First().Item3.retlist).ToArray();
                    for (int b = a, c = 0; b < af.fillSubSelectMany?.Count; b += otherAfmanys.Count, c++)
                    {
                        var vals = newexpParamVals.Select(d => d[c]).ToArray();
                        if (c == ret.Count - 1) sspShareData.Add(null); //flush flag
                        var diretTask = newexpFunc.DynamicInvoke(vals) as Task;

                        if (c < ret.Count - 1) continue;
                        await diretTask;
                        var diret = diretTask.GetTaskReflectionResult();
                        var otherList = diret as IEnumerable;
                        var retlistidx = 0;
                        foreach (var otherListItem in otherList)
                        {
                            var retlist = sspShareData[0].Item3.retlist;
                            while (retlistidx >= retlist.Count)
                            {
                                sspShareData.RemoveAt(0);
                                retlist = sspShareData[0].Item3.retlist;
                                retlistidx = 0;
                            }
                            int.TryParse(retlist[retlistidx++]?.ToString(), out var tryrowidx);
                            af.fillSubSelectMany[tryrowidx * otherAfmanys.Count + a].Item2.Add(otherListItem);
                        }
                    }
                }
                finally
                {
                    sspShareData.Clear();
                }
            }
            return ret;
        }

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

                _commonExpression.ReadAnonymousField(_tables, _tableRule, field, map, ref index, select, null, _diymemexpWithTempQuery, _whereGlobalFilter, null, null, false); //不走 DTO 映射，不处理 IncludeMany
                var af = new ReadAnonymousTypeAfInfo(map, field.Length > 0 ? field.Remove(0, 2).ToString() : null);
                if (GetTableRuleUnions().Count <= 1) return (await this.ToListMapReaderAsync<TReturn>(af, cancellationToken)).FirstOrDefault();

                var sql = GetNestSelectSql(select, af.field, ToSql);
                return (await ToListMrPrivateAsync<TReturn>(sql, af, null, cancellationToken)).FirstOrDefault();
            }
            finally
            {
                _orderby = tmpOrderBy;
            }
        }
#endif

#if ns21
        #region ToChunkAsyncEnumerable
        class LocalAsyncEnumerable<TReturn> : IAsyncEnumerable<List<TReturn>>
        {
            internal Func<CancellationToken, IAsyncEnumerator<List<TReturn>>> _GetAsyncEnumerator;
            public IAsyncEnumerator<List<TReturn>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => _GetAsyncEnumerator(cancellationToken);
        }
        class LocalAsyncEnumerator<TReturn> : IAsyncEnumerator<List<TReturn>>
        {
            internal Func<List<TReturn>> _Current;
            internal Func<ValueTask<bool>> _MoveNextAsync;
            internal Func<ValueTask> _DisposeAsync;

            public List<TReturn> Current => _Current();
            public ValueTask<bool> MoveNextAsync() => _MoveNextAsync();
            public ValueTask DisposeAsync() => _DisposeAsync();
        }

        internal IAsyncEnumerator<List<T1>> ToListAfChunkPrivateAsyncEnumerable(int chunkSize, string sql, GetAllFieldExpressionTreeInfo af, ReadAnonymousTypeOtherInfo[] otherData, CancellationToken cancellationToken)
        {
            if (_cancel?.Invoke() == true) return new LocalAsyncEnumerator<T1>
            {
                _Current = () => null,
                _MoveNextAsync = () => new ValueTask<bool>(false),
                _DisposeAsync = () => new ValueTask()
            };
            Exception exception = null;
            var retCount = 0;
            DbDataReaderAsyncEnumerator dataReaderAsyncEnumerator = null;
            List<T1> items = null;
            async ValueTask<bool> LocalMoveNextAsync()
            {
                try
                {
                    if (dataReaderAsyncEnumerator == null)
                    {
                        var dbParms = _params.ToArray();
                        var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
                        _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
                        dataReaderAsyncEnumerator = await (_orm.Ado as AdoProvider).ExecuteReaderMultipleAsync(1, _connection, _transaction, null, null, CommandType.Text, sql, _commandTimeout, dbParms, true, cancellationToken);
                        if (dataReaderAsyncEnumerator == null) return false;
                    }
                    if (!await dataReaderAsyncEnumerator.Reader.ReadAsync(cancellationToken))
                    {
                        items = null;
                        return false;
                    }
                    items = null;
                    for (var a = 0; a < chunkSize; a++)
                    {
                        if (!await dataReaderAsyncEnumerator.Reader.ReadAsync(cancellationToken))
                        {
                            if (a == 0) return false;
                            break;
                        }
                        if (a == 0) items = new List<T1>();
                        items.Add(af.Read(_orm, dataReaderAsyncEnumerator.Reader));
                        if (otherData != null)
                        {
                            var idx = af.FieldCount - 1;
                            foreach (var other in otherData)
                                other.retlist.Add(_commonExpression.ReadAnonymous(other.read, dataReaderAsyncEnumerator.Reader, ref idx, false, null, items.Count - 1, null, null));
                        }
                        retCount++;
                    }
                    foreach (var include in _includeToListAsync) await include?.Invoke(items, cancellationToken);
                    _trackToList?.Invoke(items);
                    return true;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return false;
                }
            }
            async ValueTask LocalDisposeAsync()
            {
                items?.Clear();
                await dataReaderAsyncEnumerator.Dispose(exception);
                dataReaderAsyncEnumerator = null;
            }
            return new LocalAsyncEnumerator<T1>
            {
                _Current = () => items,
                _MoveNextAsync = LocalMoveNextAsync,
                _DisposeAsync = LocalDisposeAsync
            };
        }
        internal IAsyncEnumerable<List<T1>> ToListChunkPrivateAsyncEnumerable(int chunkSize, GetAllFieldExpressionTreeInfo af, ReadAnonymousTypeOtherInfo[] otherData)
        {
            string sql = null;
            if (otherData?.Length > 0)
            {
                var sbField = new StringBuilder().Append(af.Field);
                foreach (var other in otherData)
                    sbField.Append(other.field);
                sql = this.ToSql(sbField.ToString().TrimStart(','));
            }
            else
                sql = this.ToSql(af.Field);

            return new LocalAsyncEnumerable<T1>
            {
                _GetAsyncEnumerator = (cancellationToken) => this.ToListAfChunkPrivateAsyncEnumerable(chunkSize, sql, af, otherData, cancellationToken)
            };
        }
        public IAsyncEnumerable<List<T1>> ToChunkAsyncEnumerable(int size)
        {
            if (_diymemexpWithTempQuery is WithTempQueryParser withTempQueryParser && withTempQueryParser != null)
            {
                if (withTempQueryParser._outsideTable[0] != _tables[0])
                {
                    var tp = Expression.Parameter(_tables[0].Table.Type, _tables[0].Alias);
                    _tables[0].Parameter = tp;
                    return this.InternalToChunkAsyncEnumerable<T1>(tp, size);
                }
                var af = withTempQueryParser._insideSelectList[0].InsideAf;
                return new LocalAsyncEnumerable<T1>
                {
                    _GetAsyncEnumerator = (cancellationToken) => this.ToListMrChunkPrivateAsyncEnumerable<T1>(size, this.ToSql(af.field), af, cancellationToken)
                };
            }
            if (_selectExpression != null) throw new ArgumentException(CoreErrorStrings.Before_Chunk_Cannot_Use_Select);
            return this.ToListChunkPrivateAsyncEnumerable(size, !_isIncluded ? this.GetAllFieldExpressionTreeLevel2() : this.GetAllFieldExpressionTreeLevelAll(), null);
        }


        internal IAsyncEnumerator<List<TReturn>> ToListMrChunkPrivateAsyncEnumerable<TReturn>(int chunkSize, string sql, ReadAnonymousTypeAfInfo af, CancellationToken cancellationToken)
        {
            if (_cancel?.Invoke() == true) return new LocalAsyncEnumerator<TReturn>
            {
                _Current = () => null,
                _MoveNextAsync = () => new ValueTask<bool>(false),
                _DisposeAsync = () => new ValueTask()
            };
            Exception exception = null;
            var retCount = 0;
            DbDataReaderAsyncEnumerator dataReaderAsyncEnumerator = null;
            List<TReturn> items = null;
            async ValueTask<bool> LocalMoveNextAsync()
            {
                try
                {
                    if (dataReaderAsyncEnumerator == null)
                    {
                        var type = typeof(TReturn);
                        var dbParms = _params.ToArray();
                        var before = new Aop.CurdBeforeEventArgs(_tables[0].Table.Type, _tables[0].Table, Aop.CurdType.Select, sql, dbParms);
                        _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
                        dataReaderAsyncEnumerator = await (_orm.Ado as AdoProvider).ExecuteReaderMultipleAsync(1, _connection, _transaction, null, null, CommandType.Text, sql, _commandTimeout, dbParms, true, cancellationToken);
                        if (dataReaderAsyncEnumerator == null) return false;
                    }
                    items = null;
                    for (var a = 0; a < chunkSize; a++)
                    {
                        if (!await dataReaderAsyncEnumerator.Reader.ReadAsync(cancellationToken))
                        {
                            if (a == 0) return false;
                            break;
                        }
                        if (a == 0) items = new List<TReturn>();
                        var index = -1;
                        items.Add((TReturn)_commonExpression.ReadAnonymous(af.map, dataReaderAsyncEnumerator.Reader, ref index, false, null, items.Count, af.fillIncludeMany, af.fillSubSelectMany));
                        retCount++;
                    }
                    foreach (var include in _includeToListAsync) await include?.Invoke(items, cancellationToken);
                    _trackToList?.Invoke(items);
                    return true;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return false;
                }
            }
            async ValueTask LocalDisposeAsync()
            {
                items?.Clear();
                await dataReaderAsyncEnumerator.Dispose(exception);
                dataReaderAsyncEnumerator = null;
            }
            return new LocalAsyncEnumerator<TReturn>
            {
                _Current = () => items,
                _MoveNextAsync = LocalMoveNextAsync,
                _DisposeAsync = LocalDisposeAsync
            };
        }
        public IAsyncEnumerable<List<TReturn>> InternalToChunkAsyncEnumerable<TReturn>(Expression select, int chunkSize)
        {
            var af = this.GetExpressionField(select);
            var sql = this.ToSql(af.field);
            return new LocalAsyncEnumerable<TReturn>
            {
                _GetAsyncEnumerator = (cancellationToken) => this.ToListMrChunkPrivateAsyncEnumerable<TReturn>(chunkSize, sql, af, cancellationToken)
            };
        }
        #endregion
#endif
        #endregion
    }
}
