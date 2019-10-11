using FreeSql.Extensions.EntityUtil;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace FreeSql
{
    partial class DbSet<TEntity>
    {

        Task DbContextExecCommandAsync()
        {
            _dicUpdateTimes.Clear();
            return _db.ExecCommandAsync();
        }

        async Task<int> DbContextBetchAddAsync(EntityState[] adds)
        {
            if (adds.Any() == false) return 0;
            var affrows = await this.OrmInsert(adds.Select(a => a.Value)).ExecuteAffrowsAsync();
            _db._entityChangeReport.AddRange(adds.Select(a => new DbContext.EntityChangeInfo { Object = a.Value, Type = DbContext.EntityChangeType.Insert }));
            return affrows;
        }

        #region Add
        async Task AddPrivAsync(TEntity data, bool isCheck)
        {
            if (isCheck && CanAdd(data, true) == false) return;
            if (_tableIdentitys.Length > 0)
            {
                //有自增，马上执行
                switch (_db.Orm.Ado.DataType)
                {
                    case DataType.SqlServer:
                    case DataType.OdbcSqlServer:
                    case DataType.PostgreSQL:
                    case DataType.OdbcPostgreSQL:
                        if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1)
                        {
                            await DbContextExecCommandAsync();
                            var idtval = await this.OrmInsert(data).ExecuteIdentityAsync();
                            IncrAffrows(1);
                            _db.Orm.SetEntityIdentityValueWithPrimary(_entityType, data, idtval);
                            _db._entityChangeReport.Add(new DbContext.EntityChangeInfo { Object = data, Type = DbContext.EntityChangeType.Insert });
                            Attach(data);
                            if (_db.Options.EnableAddOrUpdateNavigateList)
                                await AddOrUpdateNavigateListAsync(data, true);
                        }
                        else
                        {
                            await DbContextExecCommandAsync();
                            var newval = (await this.OrmInsert(data).ExecuteInsertedAsync()).First();
                            _db._entityChangeReport.Add(new DbContext.EntityChangeInfo { Object = newval, Type = DbContext.EntityChangeType.Insert });
                            IncrAffrows(1);
                            _db.Orm.MapEntityValue(_entityType, newval, data);
                            Attach(newval);
                            if (_db.Options.EnableAddOrUpdateNavigateList)
                                await AddOrUpdateNavigateListAsync(data, true);
                        }
                        return;
                    default:
                        if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1)
                        {
                            await DbContextExecCommandAsync();
                            var idtval = await this.OrmInsert(data).ExecuteIdentityAsync();
                            IncrAffrows(1);
                            _db.Orm.SetEntityIdentityValueWithPrimary(_entityType, data, idtval);
                            _db._entityChangeReport.Add(new DbContext.EntityChangeInfo { Object = data, Type = DbContext.EntityChangeType.Insert });
                            Attach(data);
                            if (_db.Options.EnableAddOrUpdateNavigateList)
                                await AddOrUpdateNavigateListAsync(data, true);
                        }
                        return;
                }
            }
            EnqueueToDbContext(DbContext.EntityChangeType.Insert, CreateEntityState(data));
            Attach(data);
            if (_db.Options.EnableAddOrUpdateNavigateList)
                await AddOrUpdateNavigateListAsync(data, true);
        }
        public Task AddAsync(TEntity data) => AddPrivAsync(data, true);
        async public Task AddRangeAsync(IEnumerable<TEntity> data)
        {
            if (CanAdd(data, true) == false) return;
            if (data.ElementAtOrDefault(1) == default(TEntity))
            {
                await AddAsync(data.First());
                return;
            }
            if (_tableIdentitys.Length > 0)
            {
                //有自增，马上执行
                switch (_db.Orm.Ado.DataType)
                {
                    case DataType.SqlServer:
                    case DataType.OdbcSqlServer:
                    case DataType.PostgreSQL:
                    case DataType.OdbcPostgreSQL:
                        await DbContextExecCommandAsync();
                        var rets = await this.OrmInsert(data).ExecuteInsertedAsync();
                        if (rets.Count != data.Count()) throw new Exception($"特别错误：批量添加失败，{_db.Orm.Ado.DataType} 的返回数据，与添加的数目不匹配");
                        _db._entityChangeReport.AddRange(rets.Select(a => new DbContext.EntityChangeInfo { Object = a, Type = DbContext.EntityChangeType.Insert }));
                        var idx = 0;
                        foreach (var s in data)
                            _db.Orm.MapEntityValue(_entityType, rets[idx++], s);
                        IncrAffrows(rets.Count);
                        AttachRange(rets);
                        if (_db.Options.EnableAddOrUpdateNavigateList)
                            foreach (var item in data)
                                await AddOrUpdateNavigateListAsync(item, true);
                        return;
                    default:
                        foreach (var s in data)
                            await AddPrivAsync(s, false);
                        return;
                }
            }
            else
            {
                //进入队列，等待 SaveChanges 时执行
                foreach (var item in data)
                    EnqueueToDbContext(DbContext.EntityChangeType.Insert, CreateEntityState(item));
                AttachRange(data);
                if (_db.Options.EnableAddOrUpdateNavigateList)
                    foreach (var item in data)
                        await AddOrUpdateNavigateListAsync(item, true);
            }
        }
        async Task AddOrUpdateNavigateListAsync(TEntity item, bool isAdd)
        {
            Type itemType = null;
            foreach (var prop in _table.Properties)
            {
                if (_table.ColumnsByCsIgnore.ContainsKey(prop.Key)) continue;
                if (_table.ColumnsByCs.ContainsKey(prop.Key)) continue;

                var tref = _table.GetTableRef(prop.Key, true);
                if (tref == null) continue;
                switch (tref.RefType)
                {
                    case Internal.Model.TableRefType.OneToOne:
                    case Internal.Model.TableRefType.ManyToOne:
                        continue;
                }

                object propVal = null;
                if (itemType == null) itemType = item.GetType();
                if (_table.TypeLazy != null && itemType == _table.TypeLazy)
                {
                    var lazyField = _dicLazyIsSetField.GetOrAdd(_table.TypeLazy, tl => new ConcurrentDictionary<string, FieldInfo>()).GetOrAdd(prop.Key, propName =>
                        _table.TypeLazy.GetField($"__lazy__{propName}", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance));
                    if (lazyField != null)
                    {
                        var lazyFieldValue = (bool)lazyField.GetValue(item);
                        if (lazyFieldValue == false) continue;
                    }
                    propVal = prop.Value.GetValue(item);
                }
                else
                {
                    propVal = prop.Value.GetValue(item);
                    if (propVal == null) continue;
                }

                var propValEach = propVal as IEnumerable;
                if (propValEach == null) continue;
                DbSet<object> refSet = GetDbSetObject(tref.RefEntityType);
                switch (tref.RefType)
                {
                    case Internal.Model.TableRefType.ManyToMany:
                        var curList = new List<object>();
                        foreach (var propValItem in propValEach)
                        {
                            curList.Add(propValItem);
                            var flagExists = refSet.ExistsInStates(propValItem);
                            if (flagExists == false)
                                flagExists = await refSet.Select.WhereDynamic(propValItem).AnyAsync();
                            if (refSet.CanAdd(propValItem, false) && flagExists != true)
                                await refSet.AddAsync(propValItem);
                        }
                        var midSelectParam = Expression.Parameter(typeof(object), "a");
                        var midWheres = new List<Expression<Func<object, bool>>>();
                        for (var colidx = 0; colidx < tref.Columns.Count; colidx++)
                            midWheres.Add(Expression.Lambda<Func<object, bool>>(Expression.Equal(
                                Expression.MakeMemberAccess(Expression.Convert(midSelectParam, tref.RefMiddleEntityType), tref.MiddleColumns[colidx].Table.Properties[tref.MiddleColumns[colidx].CsName]),
                                Expression.Constant(
                                    FreeSql.Internal.Utils.GetDataReaderValue(
                                        tref.MiddleColumns[colidx].CsType,
                                        _db.Orm.GetEntityValueWithPropertyName(_table.Type, item, tref.Columns[colidx].CsName)), tref.MiddleColumns[colidx].CsType)
                                ), midSelectParam));

                        if (curList.Any() == false) //全部删除
                        {
                            var delall = _db.Orm.Delete<object>().AsType(tref.RefMiddleEntityType);
                            foreach (var midWhere in midWheres) delall.Where(midWhere);
                            var sql = delall.ToSql();
                            await delall.ExecuteAffrowsAsync();
                            _db._entityChangeReport.Add(new DbContext.EntityChangeInfo { Object = sql, Type = DbContext.EntityChangeType.SqlRaw });
                        }
                        else //保存
                        {
                            var midSet = GetDbSetObject(tref.RefMiddleEntityType);
                            List<object> midList = null;
                            if (isAdd == false)
                            {
                                var midSelect = midSet.Select;
                                foreach (var midWhere in midWheres) midSelect.Where(midWhere);
                                midList = await midSelect.ToListAsync();
                            }
                            else
                                midList = new List<object>();
                            var midListDel = new List<object>();
                            var midListAdd = new List<object>();

                            foreach (var midItem in midList)
                            {
                                var curContains = new List<int>();
                                for (var curIdx = 0; curIdx < curList.Count; curIdx++)
                                {
                                    var isEquals = true;
                                    for (var midcolidx = tref.Columns.Count; midcolidx < tref.MiddleColumns.Count; midcolidx++)
                                    {
                                        var refcol = tref.Columns[midcolidx - tref.Columns.Count];
                                        var midval = FreeSql.Internal.Utils.GetDataReaderValue(refcol.CsType, _db.Orm.GetEntityValueWithPropertyName(tref.RefMiddleEntityType, midItem, tref.MiddleColumns[midcolidx].CsName));
                                        var refval = FreeSql.Internal.Utils.GetDataReaderValue(refcol.CsType, _db.Orm.GetEntityValueWithPropertyName(tref.RefEntityType, curList[curIdx], tref.Columns[midcolidx - tref.Columns.Count].CsName));
                                        if (object.Equals(midval, refval) == false)
                                        {
                                            isEquals = false;
                                            break;
                                        }
                                    }
                                    if (isEquals)
                                        curContains.Add(curIdx);
                                }
                                if (curContains.Any())
                                    foreach (var curIdx in curContains)
                                        curList.RemoveAt(curIdx);
                                else
                                    midListDel.Add(midItem);
                            }
                            midSet.RemoveRange(midListDel); //删除未保存的项
                            foreach (var curItem in curList)
                            {
                                var newItem = Activator.CreateInstance(tref.RefMiddleEntityType);
                                for (var colidx = 0; colidx < tref.Columns.Count; colidx++)
                                {
                                    var val = FreeSql.Internal.Utils.GetDataReaderValue(tref.MiddleColumns[colidx].CsType, _db.Orm.GetEntityValueWithPropertyName(_table.Type, item, tref.Columns[colidx].CsName));
                                    _db.Orm.SetEntityValueWithPropertyName(tref.RefMiddleEntityType, newItem, tref.MiddleColumns[colidx].CsName, val);
                                }
                                for (var midcolidx = tref.Columns.Count; midcolidx < tref.MiddleColumns.Count; midcolidx++)
                                {
                                    var refcol = tref.Columns[midcolidx - tref.Columns.Count];
                                    var refval = FreeSql.Internal.Utils.GetDataReaderValue(tref.MiddleColumns[midcolidx].CsType, _db.Orm.GetEntityValueWithPropertyName(tref.RefEntityType, curItem, refcol.CsName));
                                    _db.Orm.SetEntityValueWithPropertyName(tref.RefMiddleEntityType, newItem, tref.MiddleColumns[midcolidx].CsName, refval);
                                }
                                midListAdd.Add(newItem);
                            }
                            await midSet.AddRangeAsync(midListAdd);
                        }
                        break;
                    case Internal.Model.TableRefType.OneToMany:
                        foreach (var propValItem in propValEach)
                        {
                            for (var colidx = 0; colidx < tref.Columns.Count; colidx++)
                            {
                                var val = FreeSql.Internal.Utils.GetDataReaderValue(tref.RefColumns[colidx].CsType, _db.Orm.GetEntityValueWithPropertyName(_table.Type, item, tref.Columns[colidx].CsName));
                                _db.Orm.SetEntityValueWithPropertyName(tref.RefEntityType, propValItem, tref.RefColumns[colidx].CsName, val);
                            }
                            await refSet.AddOrUpdateAsync(propValItem);
                        }
                        break;
                }
            }
        }
        #endregion

        #region UpdateAsync
        Task<int> DbContextBetchUpdateAsync(EntityState[] ups) => DbContextBetchUpdatePrivAsync(ups, false);
        Task<int> DbContextBetchUpdateNowAsync(EntityState[] ups) => DbContextBetchUpdatePrivAsync(ups, true);
        async Task<int> DbContextBetchUpdatePrivAsync(EntityState[] ups, bool isLiveUpdate)
        {
            if (ups.Any() == false) return 0;
            var uplst1 = ups[ups.Length - 1];
            var uplst2 = ups.Length > 1 ? ups[ups.Length - 2] : null;

            if (_states.TryGetValue(uplst1.Key, out var lstval1) == false) return -999;
            var lstval2 = default(EntityState);
            if (uplst2 != null && _states.TryGetValue(uplst2.Key, out lstval2) == false) throw new Exception($"特别错误：更新失败，数据未被跟踪：{_db.Orm.GetEntityString(_entityType, uplst2.Value)}");

            var cuig1 = _db.Orm.CompareEntityValueReturnColumns(_entityType, uplst1.Value, lstval1.Value, true);
            var cuig2 = uplst2 != null ? _db.Orm.CompareEntityValueReturnColumns(_entityType, uplst2.Value, lstval2.Value, true) : null;

            List<EntityState> data = null;
            string[] cuig = null;
            if (uplst2 != null && string.Compare(string.Join(",", cuig1), string.Join(",", cuig2)) != 0)
            {
                //最后一个不保存
                data = ups.ToList();
                data.RemoveAt(ups.Length - 1);
                cuig = cuig2;
            }
            else if (isLiveUpdate)
            {
                //立即保存
                data = ups.ToList();
                cuig = cuig1;
            }

            if (data?.Count > 0)
            {

                if (cuig.Length == _table.Columns.Count)
                    return ups.Length == data.Count ? -998 : -997;

                var updateSource = data.Select(a => a.Value).ToArray();
                var update = this.OrmUpdate(null).SetSource(updateSource).IgnoreColumns(cuig);

                var affrows = await update.ExecuteAffrowsAsync();
                _db._entityChangeReport.AddRange(updateSource.Select(a => new DbContext.EntityChangeInfo { Object = a, Type = DbContext.EntityChangeType.Update }));

                foreach (var newval in data)
                {
                    if (_states.TryGetValue(newval.Key, out var tryold))
                        _db.Orm.MapEntityValue(_entityType, newval.Value, tryold.Value);
                    if (newval.OldValue != null)
                        _db.Orm.MapEntityValue(_entityType, newval.Value, newval.OldValue);
                }
                return affrows;
            }

            //等待下次对比再保存
            return 0;
        }
        async public Task UpdateAsync(TEntity data)
        {
            var exists = ExistsInStates(data);
            if (exists == null) throw new Exception($"不可更新，未设置主键的值：{_db.Orm.GetEntityString(_entityType, data)}");
            if (exists == false)
            {
                var olddata = await OrmSelect(data).FirstAsync();
                if (olddata == null) throw new Exception($"不可更新，数据库不存在该记录：{_db.Orm.GetEntityString(_entityType, data)}");
            }

            await UpdateRangePrivAsync(new[] { data }, true);
        }
        public Task UpdateRangeAsync(IEnumerable<TEntity> data) => UpdateRangePrivAsync(data, true);
        async Task UpdateRangePrivAsync(IEnumerable<TEntity> data, bool isCheck)
        {
            if (CanUpdate(data, true) == false) return;
            foreach (var item in data)
            {
                if (_dicUpdateTimes.ContainsKey(item))
                    await DbContextExecCommandAsync();
                _dicUpdateTimes.Add(item, 1);

                var state = CreateEntityState(item);
                state.OldValue = item;
                EnqueueToDbContext(DbContext.EntityChangeType.Update, state);
            }
            if (_db.Options.EnableAddOrUpdateNavigateList)
                foreach (var item in data)
                    await AddOrUpdateNavigateListAsync(item, false);
        }
        #endregion

        #region RemoveAsync
        async Task<int> DbContextBetchRemoveAsync(EntityState[] dels)
        {
            if (dels.Any() == false) return 0;
            var affrows = await this.OrmDelete(dels.Select(a => a.Value)).ExecuteAffrowsAsync();
            _db._entityChangeReport.AddRange(dels.Select(a => new DbContext.EntityChangeInfo { Object = a.Value, Type = DbContext.EntityChangeType.Delete }));
            return Math.Max(dels.Length, affrows);
        }
        #endregion

        #region AddOrUpdateAsync
        async public Task AddOrUpdateAsync(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (_table.Primarys.Any() == false) throw new Exception($"不可添加，实体没有主键：{_db.Orm.GetEntityString(_entityType, data)}");

            var flagExists = ExistsInStates(data);
            if (flagExists == false)
            {
                var olddata = await OrmSelect(data).FirstAsync();
                flagExists = olddata != null;
            }

            if (flagExists == true && CanUpdate(data, false))
            {
                await DbContextExecCommandAsync();
                var affrows = _db._affrows;
                await UpdateRangePrivAsync(new[] { data }, false);
                await DbContextExecCommandAsync();
                affrows = _db._affrows - affrows;
                if (affrows > 0) return;
            }
            if (CanAdd(data, false))
            {
                _db.Orm.ClearEntityPrimaryValueWithIdentity(_entityType, data);
                await AddPrivAsync(data, false);
            }
        }
        #endregion
    }
}
