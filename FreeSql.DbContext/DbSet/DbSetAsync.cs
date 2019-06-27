using FreeSql.Extensions.EntityUtil;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreeSql
{
    partial class DbSet<TEntity>
    {

        Task DbContextExecCommandAsync()
        {
            _dicUpdateTimes.Clear();
            return _ctx.ExecCommandAsync();
        }

        async Task<int> DbContextBetchAddAsync(EntityState[] adds)
        {
            if (adds.Any() == false) return 0;
            var affrows = await this.OrmInsert(adds.Select(a => a.Value)).ExecuteAffrowsAsync();
            return affrows;
        }

        #region Add
        async Task AddPrivAsync(TEntity data, bool isCheck)
        {
            if (isCheck && CanAdd(data, true) == false) return;
            if (_tableIdentitys.Length > 0)
            {
                //有自增，马上执行
                switch (_fsql.Ado.DataType)
                {
                    case DataType.SqlServer:
                    case DataType.PostgreSQL:
                        if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1)
                        {
                            await DbContextExecCommandAsync();
                            var idtval = await this.OrmInsert(data).ExecuteIdentityAsync();
                            IncrAffrows(1);
                            _fsql.SetEntityIdentityValueWithPrimary(_entityType, data, idtval);
                            Attach(data);
                            if (_ctx.Options.EnableAddOrUpdateNavigateList)
                                await AddOrUpdateNavigateListAsync(data);
                        }
                        else
                        {
                            await DbContextExecCommandAsync();
                            var newval = (await this.OrmInsert(data).ExecuteInsertedAsync()).First();
                            IncrAffrows(1);
                            _fsql.MapEntityValue(_entityType, newval, data);
                            Attach(newval);
                            if (_ctx.Options.EnableAddOrUpdateNavigateList)
                                await AddOrUpdateNavigateListAsync(data);
                        }
                        return;
                    case DataType.MySql:
                    case DataType.Oracle:
                    case DataType.Sqlite:
                        if (_tableIdentitys.Length == 1 && _table.Primarys.Length == 1)
                        {
                            await DbContextExecCommandAsync();
                            var idtval = await this.OrmInsert(data).ExecuteIdentityAsync();
                            IncrAffrows(1);
                            _fsql.SetEntityIdentityValueWithPrimary(_entityType, data, idtval);
                            Attach(data);
                            if (_ctx.Options.EnableAddOrUpdateNavigateList)
                                await AddOrUpdateNavigateListAsync(data);
                        }
                        return;
                }
            }
            EnqueueToDbContext(DbContext.ExecCommandInfoType.Insert, CreateEntityState(data));
            Attach(data);
            if (_ctx.Options.EnableAddOrUpdateNavigateList)
                await AddOrUpdateNavigateListAsync(data);
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
                switch (_fsql.Ado.DataType)
                {
                    case DataType.SqlServer:
                    case DataType.PostgreSQL:
                        await DbContextExecCommandAsync();
                        var rets = await this.OrmInsert(data).ExecuteInsertedAsync();
                        if (rets.Count != data.Count()) throw new Exception($"特别错误：批量添加失败，{_fsql.Ado.DataType} 的返回数据，与添加的数目不匹配");
                        var idx = 0;
                        foreach (var s in data)
                            _fsql.MapEntityValue(_entityType, rets[idx++], s);
                        IncrAffrows(rets.Count);
                        AttachRange(rets);
                        if (_ctx.Options.EnableAddOrUpdateNavigateList)
                            foreach (var item in data)
                                await AddOrUpdateNavigateListAsync(item);
                        return;
                    case DataType.MySql:
                    case DataType.Oracle:
                    case DataType.Sqlite:
                        foreach (var s in data)
                            await AddPrivAsync(s, false);
                        return;
                }
            }
            else
            {
                //进入队列，等待 SaveChanges 时执行
                foreach (var item in data)
                    EnqueueToDbContext(DbContext.ExecCommandInfoType.Insert, CreateEntityState(item));
                AttachRange(data);
                if (_ctx.Options.EnableAddOrUpdateNavigateList)
                    foreach (var item in data)
                        await AddOrUpdateNavigateListAsync(item);
            }
        }
        async Task AddOrUpdateNavigateListAsync(TEntity item)
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
                    case Internal.Model.TableRefType.ManyToMany:
                        continue;
                    case Internal.Model.TableRefType.OneToMany:
                        if (itemType == null) itemType = item.GetType();
                        if (_table.TypeLazy != null && itemType == _table.TypeLazy)
                        {
                            var lazyField = _dicLazyIsSetField.GetOrAdd(_table.TypeLazy, tl => new ConcurrentDictionary<string, System.Reflection.FieldInfo>()).GetOrAdd(prop.Key, propName =>
                                _table.TypeLazy.GetField($"__lazy__{propName}", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
                            if (lazyField != null)
                            {
                                var lazyFieldValue = (bool)lazyField.GetValue(item);
                                if (lazyFieldValue == false) continue;
                            }
                        }
                        var propVal = prop.Value.GetValue(item);
                        var propValEach = propVal as IEnumerable;
                        if (propValEach == null) continue;
                        object dbset = null;
                        System.Reflection.MethodInfo dbsetAddOrUpdate = null;
                        foreach (var propValItem in propValEach)
                        {
                            if (dbset == null)
                            {
                                dbset = _ctx.Set(tref.RefEntityType);
                                dbsetAddOrUpdate = dbset.GetType().GetMethod("AddOrUpdateAsync", new Type[] { tref.RefEntityType });
                            }
                            for (var colidx = 0; colidx < tref.Columns.Count; colidx++)
                            {
                                tref.RefColumns[colidx].Table.Properties[tref.RefColumns[colidx].CsName]
                                    .SetValue(propValItem, tref.Columns[colidx].Table.Properties[tref.Columns[colidx].CsName].GetValue(item));
                            }
                            Task task = dbsetAddOrUpdate.Invoke(dbset, new object[] { propValItem }) as Task;
                            await task;
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
            if (uplst2 != null && _states.TryGetValue(uplst2.Key, out lstval2) == false) throw new Exception($"特别错误：更新失败，数据未被跟踪：{_fsql.GetEntityString(_entityType, uplst2.Value)}");

            var cuig1 = _fsql.CompareEntityValueReturnColumns(_entityType, uplst1.Value, lstval1.Value, true);
            var cuig2 = uplst2 != null ? _fsql.CompareEntityValueReturnColumns(_entityType, uplst2.Value, lstval2.Value, true) : null;

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

                foreach (var newval in data)
                {
                    if (_states.TryGetValue(newval.Key, out var tryold))
                        _fsql.MapEntityValue(_entityType, newval.Value, tryold.Value);
                    if (newval.OldValue != null)
                        _fsql.MapEntityValue(_entityType, newval.Value, newval.OldValue);
                }
                return affrows;
            }

            //等待下次对比再保存
            return 0;
        }
        async public Task UpdateAsync(TEntity data)
        {
            var exists = ExistsInStates(data);
            if (exists == null) throw new Exception($"不可更新，未设置主键的值：{_fsql.GetEntityString(_entityType, data)}");
            if (exists == false)
            {
                var olddata = await OrmSelect(data).FirstAsync();
                if (olddata == null) throw new Exception($"不可更新，数据库不存在该记录：{_fsql.GetEntityString(_entityType, data)}");
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
                EnqueueToDbContext(DbContext.ExecCommandInfoType.Update, state);
            }
            if (_ctx.Options.EnableAddOrUpdateNavigateList)
                foreach (var item in data)
                    await AddOrUpdateNavigateListAsync(item);
        }
        #endregion

        #region RemoveAsync
        async Task<int> DbContextBetchRemoveAsync(EntityState[] dels)
        {
            if (dels.Any() == false) return 0;
            var affrows = await this.OrmDelete(dels.Select(a => a.Value)).ExecuteAffrowsAsync();
            return Math.Max(dels.Length, affrows);
        }
        #endregion

        #region AddOrUpdateAsync
        async public Task AddOrUpdateAsync(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (_table.Primarys.Any() == false) throw new Exception($"不可添加，实体没有主键：{_fsql.GetEntityString(_entityType, data)}");

            var flagExists = ExistsInStates(data);
            if (flagExists == false)
            {
                var olddata = await OrmSelect(data).FirstAsync();
                if (olddata == null) flagExists = false;
            }

            if (flagExists == true && CanUpdate(data, false))
            {
                await DbContextExecCommandAsync();
                var affrows = _ctx._affrows;
                await UpdateRangePrivAsync(new[] { data }, false);
                await DbContextExecCommandAsync();
                affrows = _ctx._affrows - affrows;
                if (affrows > 0) return;
            }
            if (CanAdd(data, false))
            {
                _fsql.ClearEntityPrimaryValueWithIdentity(_entityType, data);
                await AddPrivAsync(data, false);
            }
        }
        #endregion
    }
}
