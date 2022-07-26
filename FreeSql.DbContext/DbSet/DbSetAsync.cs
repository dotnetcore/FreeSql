using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#if net40
#else
namespace FreeSql
{
    partial class DbSet<TEntity>
    {
        Task DbContextFlushCommandAsync(CancellationToken cancellationToken)
        {
            _dicUpdateTimes.Clear();
            return _db.FlushCommandAsync(cancellationToken);
        }

        async Task<int> DbContextBatchAddAsync(EntityState[] adds, CancellationToken cancellationToken)
        {
            if (adds.Any() == false) return 0;
            var affrows = await this.OrmInsert(adds.Select(a => a.Value)).ExecuteAffrowsAsync(cancellationToken);
            _db._entityChangeReport.AddRange(adds.Select(a => new DbContext.EntityChangeReport.ChangeInfo { EntityType = _entityType, Object = a.Value, Type = DbContext.EntityChangeType.Insert }));
            return affrows;
        }

        #region Add
        async Task AddPrivAsync(TEntity data, bool isCheck, CancellationToken cancellationToken)
        {
            if (isCheck && CanAdd(data, true) == false) return;
            if (_tableReturnColumns.Length > 0)
            {
                //有自增，马上执行
                switch (_db.OrmOriginal.Ado.DataType)
                {
                    case DataType.SqlServer:
                    case DataType.OdbcSqlServer:
                    case DataType.PostgreSQL:
                    case DataType.OdbcPostgreSQL:
                    case DataType.KingbaseES:
                    case DataType.OdbcKingbaseES:
                    case DataType.ShenTong:
                    case DataType.Firebird: //firebird 只支持单条插入 returning
                        if (_tableIdentitys.Length == 1 && _tableReturnColumns.Length == 1)
                        {
                            await DbContextFlushCommandAsync(cancellationToken);
                            var idtval = await this.OrmInsert(data).ExecuteIdentityAsync(cancellationToken);
                            IncrAffrows(1);
                            _db.OrmOriginal.SetEntityValueWithPropertyName(_entityType, data, _tableIdentitys[0].CsName, idtval);
                            _db._entityChangeReport.Add(new DbContext.EntityChangeReport.ChangeInfo { EntityType = _entityType, Object = data, Type = DbContext.EntityChangeType.Insert });
                            Attach(data);
                            if (_db.Options.EnableCascadeSave)
                                await AddOrUpdateNavigateAsync(data, true, null, cancellationToken);
                        }
                        else
                        {
                            await DbContextFlushCommandAsync(cancellationToken);
                            var newval = (await this.OrmInsert(data).ExecuteInsertedAsync(cancellationToken)).First();
                            _db._entityChangeReport.Add(new DbContext.EntityChangeReport.ChangeInfo { EntityType = _entityType, Object = newval, Type = DbContext.EntityChangeType.Insert });
                            IncrAffrows(1);
                            _db.OrmOriginal.MapEntityValue(_entityType, newval, data);
                            Attach(newval);
                            if (_db.Options.EnableCascadeSave)
                                await AddOrUpdateNavigateAsync(data, true, null, cancellationToken);
                        }
                        return;
                    default:
                        if (_tableIdentitys.Length == 1)
                        {
                            await DbContextFlushCommandAsync(cancellationToken);
                            var idtval = await this.OrmInsert(data).ExecuteIdentityAsync(cancellationToken);
                            IncrAffrows(1);
                            _db.OrmOriginal.SetEntityValueWithPropertyName(_entityType, data, _tableIdentitys[0].CsName, idtval);
                            _db._entityChangeReport.Add(new DbContext.EntityChangeReport.ChangeInfo { EntityType = _entityType, Object = data, Type = DbContext.EntityChangeType.Insert });
                            Attach(data);
                            if (_db.Options.EnableCascadeSave)
                                await AddOrUpdateNavigateAsync(data, true, null, cancellationToken);
                            return;
                        }
                        break;
                }
            }
            EnqueueToDbContext(DbContext.EntityChangeType.Insert, CreateEntityState(data));
            Attach(data);
            if (_db.Options.EnableCascadeSave)
                await AddOrUpdateNavigateAsync(data, true, null, cancellationToken);
        }
        public Task AddAsync(TEntity data, CancellationToken cancellationToken = default) => AddPrivAsync(data, true, cancellationToken);
        async public Task AddRangeAsync(IEnumerable<TEntity> data, CancellationToken cancellationToken = default)
        {
            if (CanAdd(data, true) == false) return;
            if (data.ElementAtOrDefault(1) == default(TEntity))
            {
                await AddAsync(data.First(), cancellationToken);
                return;
            }
            if (_tableReturnColumns.Length > 0)
            {
                //有自增，马上执行
                switch (_db.OrmOriginal.Ado.DataType)
                {
                    case DataType.SqlServer:
                    case DataType.OdbcSqlServer:
                    case DataType.PostgreSQL:
                    case DataType.OdbcPostgreSQL:
                    case DataType.KingbaseES:
                    case DataType.OdbcKingbaseES:
                    case DataType.ShenTong:
                        await DbContextFlushCommandAsync(cancellationToken);
                        var rets = await this.OrmInsert(data).ExecuteInsertedAsync(cancellationToken);
                        if (rets.Count != data.Count()) throw new Exception(DbContextStrings.SpecialError_BatchAdditionFailed(_db.OrmOriginal.Ado.DataType));
                        _db._entityChangeReport.AddRange(rets.Select(a => new DbContext.EntityChangeReport.ChangeInfo { EntityType = _entityType, Object = a, Type = DbContext.EntityChangeType.Insert }));
                        var idx = 0;
                        foreach (var s in data)
                            _db.OrmOriginal.MapEntityValue(_entityType, rets[idx++], s);
                        IncrAffrows(rets.Count);
                        AttachRange(rets);
                        if (_db.Options.EnableCascadeSave)
                            foreach (var item in data)
                                await AddOrUpdateNavigateAsync(item, true, null, cancellationToken);
                        return;
                    default:
                        if (_tableIdentitys.Length == 1)
                        {
                            foreach (var s in data)
                                await AddPrivAsync(s, false, cancellationToken);
                            return;
                        }
                        break;
                }
            }
            //进入队列，等待 SaveChanges 时执行
            foreach (var item in data)
                EnqueueToDbContext(DbContext.EntityChangeType.Insert, CreateEntityState(item));
            AttachRange(data);
            if (_db.Options.EnableCascadeSave)
                foreach (var item in data)
                    await AddOrUpdateNavigateAsync(item, true, null, cancellationToken);
        }

        async public Task SaveManyAsync(TEntity item, string propertyName, CancellationToken cancellationToken = default)
        {
            if (item == null) return;
            if (string.IsNullOrEmpty(propertyName)) return;
            if (_table.Properties.TryGetValue(propertyName, out var prop) == false) throw new KeyNotFoundException(DbContextStrings.NotFound_Property(_table.Type.FullName, propertyName));
            if (_table.ColumnsByCsIgnore.ContainsKey(propertyName)) throw new ArgumentException(DbContextStrings.TypeHasSetProperty_IgnoreAttribute(_table.Type.FullName, propertyName));

            var tref = _table.GetTableRef(propertyName, true);
            if (tref == null) return;
            switch (tref.RefType)
            {
                case TableRefType.OneToOne:
                case TableRefType.ManyToOne:
                case TableRefType.PgArrayToMany:
                    throw new ArgumentException(DbContextStrings.PropertyOfType_IsNot_OneToManyOrManyToMany(_table.Type.FullName, propertyName));
            }

            await DbContextFlushCommandAsync(cancellationToken);
            var oldEnable = _db.Options.EnableCascadeSave;
            _db.Options.EnableCascadeSave = false;
            try
            {
                await AddOrUpdateNavigateAsync(item, false, propertyName, cancellationToken);
                if (tref.RefType == TableRefType.OneToMany)
                {
                    await DbContextFlushCommandAsync(cancellationToken);
                    //删除没有保存的数据，求出主体的条件
                    var deleteWhereParentParam = Expression.Parameter(typeof(object), "a");
                    Expression whereParentExp = null;
                    for (var colidx = 0; colidx < tref.Columns.Count; colidx++)
                    {
                        var whereExp = Expression.Equal(
                            Expression.MakeMemberAccess(Expression.Convert(deleteWhereParentParam, tref.RefEntityType), tref.RefColumns[colidx].Table.Properties[tref.RefColumns[colidx].CsName]),
                            Expression.Constant(
                                FreeSql.Internal.Utils.GetDataReaderValue(
                                    tref.Columns[colidx].CsType,
                                    _db.OrmOriginal.GetEntityValueWithPropertyName(_table.Type, item, tref.Columns[colidx].CsName)), tref.RefColumns[colidx].CsType)
                            );
                        if (whereParentExp == null) whereParentExp = whereExp;
                        else whereParentExp = Expression.AndAlso(whereParentExp, whereExp);
                    }
                    var propValEach = GetItemValue(item, prop) as IEnumerable;
                    var subDelete = _db.OrmOriginal.Delete<object>().AsType(tref.RefEntityType)
                        .WithTransaction(_uow?.GetOrBeginTransaction())
                        .Where(Expression.Lambda<Func<object, bool>>(whereParentExp, deleteWhereParentParam));
                    foreach (var propValItem in propValEach)
                    {
                        subDelete.WhereDynamic(propValEach, true);
                        break;
                    }
                    await subDelete.ExecuteAffrowsAsync(cancellationToken);
                }
            }
            finally
            {
                _db.Options.EnableCascadeSave = oldEnable;
            }
        }
        async Task AddOrUpdateNavigateAsync(TEntity item, bool isAdd, string propertyName, CancellationToken cancellationToken)
        {
            Func<PropertyInfo, Task> action = async prop =>
            {
                if (_table.ColumnsByCsIgnore.ContainsKey(prop.Name)) return;
                if (_table.ColumnsByCs.ContainsKey(prop.Name)) return;

                var tref = _table.GetTableRef(prop.Name, false); //防止非正常的导航属性报错
                if (tref == null) return;
                DbSet<object> refSet = null;
                switch (tref.RefType)
                {
                    case TableRefType.OneToOne:
                        refSet = GetDbSetObject(tref.RefEntityType);
                        var propValItem = GetItemValue(item, prop);
                        if (propValItem == null) return;
                        for (var colidx = 0; colidx < tref.Columns.Count; colidx++)
                        {
                            var val = FreeSql.Internal.Utils.GetDataReaderValue(tref.RefColumns[colidx].CsType, _db.OrmOriginal.GetEntityValueWithPropertyName(_table.Type, item, tref.Columns[colidx].CsName));
                            _db.OrmOriginal.SetEntityValueWithPropertyName(tref.RefEntityType, propValItem, tref.RefColumns[colidx].CsName, val);
                        }
                        if (isAdd) await refSet.AddAsync(propValItem);
                        else await refSet.AddOrUpdateAsync(propValItem);
                        return;
                    case TableRefType.ManyToOne:
                    case TableRefType.PgArrayToMany:
                        return;
                }

                var propValEach = GetItemValue(item, prop) as IEnumerable;
                if (propValEach == null) return;
                refSet = GetDbSetObject(tref.RefEntityType);
                switch (tref.RefType)
                {
                    case TableRefType.ManyToMany:
                        var curList = new List<object>();
                        foreach (var propValItem in propValEach)
                        {
                            curList.Add(propValItem);
                            var flagExists = refSet.ExistsInStates(propValItem);
                            if (flagExists == false)
                                flagExists = await refSet.Select.WhereDynamic(propValItem).AnyAsync(cancellationToken);
                            if (refSet.CanAdd(propValItem, false) && flagExists != true)
                                await refSet.AddAsync(propValItem, cancellationToken);
                        }
                        var midSelectParam = Expression.Parameter(typeof(object), "a");
                        var midWheres = new List<Expression<Func<object, bool>>>();
                        for (var colidx = 0; colidx < tref.Columns.Count; colidx++)
                            midWheres.Add(Expression.Lambda<Func<object, bool>>(Expression.Equal(
                                Expression.MakeMemberAccess(Expression.Convert(midSelectParam, tref.RefMiddleEntityType), tref.MiddleColumns[colidx].Table.Properties[tref.MiddleColumns[colidx].CsName]),
                                Expression.Constant(
                                    FreeSql.Internal.Utils.GetDataReaderValue(
                                        tref.MiddleColumns[colidx].CsType,
                                        _db.OrmOriginal.GetEntityValueWithPropertyName(_table.Type, item, tref.Columns[colidx].CsName)), tref.MiddleColumns[colidx].CsType)
                                ), midSelectParam));

                        if (curList.Any() == false) //全部删除
                        {
                            var delall = _db.OrmOriginal.Delete<object>()
                                .AsType(tref.RefMiddleEntityType)
                                .WithTransaction(_uow?.GetOrBeginTransaction());
                            foreach (var midWhere in midWheres) delall.Where(midWhere);
                            var sql = delall.ToSql();
                            await delall.ExecuteAffrowsAsync(cancellationToken);
                            _db._entityChangeReport.Add(new DbContext.EntityChangeReport.ChangeInfo { EntityType = _entityType, Object = sql, Type = DbContext.EntityChangeType.SqlRaw });
                        }
                        else //保存
                        {
                            var midSet = GetDbSetObject(tref.RefMiddleEntityType);
                            List<object> midList = null;
                            if (isAdd == false)
                            {
                                var midSelect = midSet.Select;
                                foreach (var midWhere in midWheres) midSelect.Where(midWhere);
                                midList = await midSelect.ToListAsync(false, cancellationToken);
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
                                        var refcol = tref.RefColumns[midcolidx - tref.Columns.Count];
                                        var midval = FreeSql.Internal.Utils.GetDataReaderValue(refcol.CsType, _db.OrmOriginal.GetEntityValueWithPropertyName(tref.RefMiddleEntityType, midItem, tref.MiddleColumns[midcolidx].CsName));
                                        var refval = FreeSql.Internal.Utils.GetDataReaderValue(refcol.CsType, _db.OrmOriginal.GetEntityValueWithPropertyName(tref.RefEntityType, curList[curIdx], refcol.CsName));
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
                                    for (var delIdx = curContains.Count - 1; delIdx >= 0; delIdx--)
                                        curList.RemoveAt(curContains[delIdx]);
                                else
                                    midListDel.Add(midItem);
                            }
                            midSet.RemoveRange(midListDel); //删除未保存的项
                            foreach (var curItem in curList)
                            {
                                var newItem = tref.RefMiddleEntityType.CreateInstanceGetDefaultValue();
                                for (var colidx = 0; colidx < tref.Columns.Count; colidx++)
                                {
                                    var val = FreeSql.Internal.Utils.GetDataReaderValue(tref.MiddleColumns[colidx].CsType, _db.OrmOriginal.GetEntityValueWithPropertyName(_table.Type, item, tref.Columns[colidx].CsName));
                                    _db.OrmOriginal.SetEntityValueWithPropertyName(tref.RefMiddleEntityType, newItem, tref.MiddleColumns[colidx].CsName, val);
                                }
                                for (var midcolidx = tref.Columns.Count; midcolidx < tref.MiddleColumns.Count; midcolidx++)
                                {
                                    var refcol = tref.RefColumns[midcolidx - tref.Columns.Count];
                                    var refval = FreeSql.Internal.Utils.GetDataReaderValue(tref.MiddleColumns[midcolidx].CsType, _db.OrmOriginal.GetEntityValueWithPropertyName(tref.RefEntityType, curItem, refcol.CsName));
                                    _db.OrmOriginal.SetEntityValueWithPropertyName(tref.RefMiddleEntityType, newItem, tref.MiddleColumns[midcolidx].CsName, refval);
                                }
                                midListAdd.Add(newItem);
                            }
                            await midSet.AddRangeAsync(midListAdd, cancellationToken);
                        }
                        break;
                    case TableRefType.OneToMany:
                        var addList = new List<object>();
                        var addOrUpdateList = new List<object>();
                        foreach (var propValItem in propValEach)
                        {
                            for (var colidx = 0; colidx < tref.Columns.Count; colidx++)
                            {
                                var val = FreeSql.Internal.Utils.GetDataReaderValue(tref.RefColumns[colidx].CsType, _db.OrmOriginal.GetEntityValueWithPropertyName(_table.Type, item, tref.Columns[colidx].CsName));
                                _db.OrmOriginal.SetEntityValueWithPropertyName(tref.RefEntityType, propValItem, tref.RefColumns[colidx].CsName, val);
                            }
                            if (isAdd) addList.Add(propValItem);
                            else
                            {
                                var flagExists = refSet.ExistsInStates(propValItem);
                                if (flagExists == null) addList.Add(propValItem); //自增/Guid
                                else addOrUpdateList.Add(propValItem); //统一状态管理
                            }
                        }
                        if (addList.Any()) await refSet.AddRangeAsync(addList, cancellationToken);
                        if (addOrUpdateList.Any())
                        {
                            var existsList = await refSet.Select.WhereDynamic(addOrUpdateList).ToListAsync(false, cancellationToken);
                            foreach (var aouItem in addOrUpdateList) await refSet.AddOrUpdateAsync(aouItem, cancellationToken);
                        }
                        break;
                }
            };

            if (string.IsNullOrEmpty(propertyName))
                foreach (var prop in _table.Properties)
                    await action(prop.Value);
            else if (_table.Properties.TryGetValue(propertyName, out var prop))
                await action(prop);
        }
        #endregion

        #region UpdateAsync
        Task<int> DbContextBatchUpdateAsync(EntityState[] ups, CancellationToken cancellationToken) => DbContextBatchUpdatePrivAsync(ups, false, cancellationToken);
        Task<int> DbContextBatchUpdateNowAsync(EntityState[] ups, CancellationToken cancellationToken) => DbContextBatchUpdatePrivAsync(ups, true, cancellationToken);
        async Task<int> DbContextBatchUpdatePrivAsync(EntityState[] ups, bool isLiveUpdate, CancellationToken cancellationToken)
        {
            if (ups.Any() == false) return 0;
            var uplst1 = ups[ups.Length - 1];
            var uplst2 = ups.Length > 1 ? ups[ups.Length - 2] : null;

            if (_states.TryGetValue(uplst1.Key, out var lstval1) == false) return -999;
            var lstval2 = default(EntityState);
            if (uplst2 != null && _states.TryGetValue(uplst2.Key, out lstval2) == false) throw new Exception(DbContextStrings.SpecialError_UpdateFailedDataNotTracked(_db.OrmOriginal.GetEntityString(_entityType, uplst2.Value)));

            var cuig1 = _db.OrmOriginal.CompareEntityValueReturnColumns(_entityType, uplst1.Value, lstval1.Value, true);
            var cuig2 = uplst2 != null ? _db.OrmOriginal.CompareEntityValueReturnColumns(_entityType, uplst2.Value, lstval2.Value, true) : null;

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

                var update = this.OrmUpdate(null).SetSource(data.Select(a => a.Value)).IgnoreColumns(cuig);
                var affrows = await update.ExecuteAffrowsAsync(cancellationToken);
                _db._entityChangeReport.AddRange(data.Select(a => new DbContext.EntityChangeReport.ChangeInfo
                {
                    EntityType = _entityType,
                    Object = a.Value,
                    BeforeObject = _states.TryGetValue(a.Key, out var beforeVal) ? CreateEntityState(beforeVal.Value).Value : null,
                    Type = DbContext.EntityChangeType.Update
                }));

                foreach (var newval in data)
                {
                    if (_states.TryGetValue(newval.Key, out var tryold))
                        _db.OrmOriginal.MapEntityValue(_entityType, newval.Value, tryold.Value);
                    if (newval.OldValue != null)
                        _db.OrmOriginal.MapEntityValue(_entityType, newval.Value, newval.OldValue);
                }
                return affrows;
            }

            //等待下次对比再保存
            return 0;
        }
        async public Task UpdateAsync(TEntity data, CancellationToken cancellationToken = default)
        {
            var exists = ExistsInStates(data);
            if (exists == null) throw new Exception(DbContextStrings.CannotUpdate_PrimaryKey_NotSet(_db.OrmOriginal.GetEntityString(_entityType, data)));
            if (exists == false)
            {
                var olddata = await OrmSelect(data).FirstAsync(cancellationToken);
                if (olddata == null) throw new Exception(DbContextStrings.CannotUpdate_RecordDoesNotExist(_db.OrmOriginal.GetEntityString(_entityType, data)));
            }

            await UpdateRangePrivAsync(new[] { data }, true, cancellationToken);
        }
        public Task UpdateRangeAsync(IEnumerable<TEntity> data, CancellationToken cancellationToken = default) => UpdateRangePrivAsync(data, true, cancellationToken);
        async Task UpdateRangePrivAsync(IEnumerable<TEntity> data, bool isCheck, CancellationToken cancellationToken)
        {
            if (CanUpdate(data, true) == false) return;
            foreach (var item in data)
            {
                if (_dicUpdateTimes.ContainsKey(item))
                {
                    var itemCopy = CreateEntityState(item).Value;
                    await DbContextFlushCommandAsync(cancellationToken);
                    if (_table.VersionColumn != null)
                    {
                        var itemVersion = _db.OrmOriginal.GetEntityValueWithPropertyName(_entityType, item, _table.VersionColumn.CsName);
                        _db.OrmOriginal.MapEntityValue(_entityType, itemCopy, item);
                        _db.OrmOriginal.SetEntityValueWithPropertyName(_entityType, item, _table.VersionColumn.CsName, itemVersion);
                    }
                    else
                        _db.OrmOriginal.MapEntityValue(_entityType, itemCopy, item);
                }
                _dicUpdateTimes.Add(item, 1);

                var state = CreateEntityState(item);
                state.OldValue = item;
                EnqueueToDbContext(DbContext.EntityChangeType.Update, state);
            }
            if (_db.Options.EnableCascadeSave)
                foreach (var item in data)
                    await AddOrUpdateNavigateAsync(item, false, null, cancellationToken);
        }
        #endregion

        #region RemoveAsync
        async Task<int> DbContextBatchRemoveAsync(EntityState[] dels, CancellationToken cancellationToken)
        {
            if (dels.Any() == false) return 0;
            var affrows = await this.OrmDelete(dels.Select(a => a.Value)).ExecuteAffrowsAsync(cancellationToken);
            _db._entityChangeReport.AddRange(dels.Select(a => new DbContext.EntityChangeReport.ChangeInfo { EntityType = _entityType, Object = a.Value, Type = DbContext.EntityChangeType.Delete }));
            return affrows;
        }
        async public Task<int> RemoveAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await DbContextFlushCommandAsync(cancellationToken);
            return await this.OrmDelete(null).Where(predicate).ExecuteAffrowsAsync(cancellationToken);
        }
        #endregion

        #region AddOrUpdateAsync
        async public Task AddOrUpdateAsync(TEntity data, CancellationToken cancellationToken = default)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (_table.Primarys.Any() == false) throw new Exception(DbContextStrings.CannotAdd_EntityHasNo_PrimaryKey(_db.OrmOriginal.GetEntityString(_entityType, data)));

            var flagExists = ExistsInStates(data);
            if (flagExists == false)
            {
                var olddata = await OrmSelect(data).FirstAsync(cancellationToken);
                flagExists = olddata != null;
            }

            if (flagExists == true && CanUpdate(data, false))
            {
                await DbContextFlushCommandAsync(cancellationToken);
                var affrows = _db._affrows;
                await UpdateRangePrivAsync(new[] { data }, false, cancellationToken);
                await DbContextFlushCommandAsync(cancellationToken);
                affrows = _db._affrows - affrows;
                if (affrows > 0) return;
            }
            if (CanAdd(data, false))
            {
                _db.OrmOriginal.ClearEntityPrimaryValueWithIdentity(_entityType, data);
                await AddPrivAsync(data, false, cancellationToken);
            }
        }
        #endregion

        #region RemoveCascadeAsync
        public Task<List<object>> RemoveCascadeByDatabaseAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) => RemoveRangeCascadeByMemoryOrDatabaseAsync(Select.Where(predicate).ToList(), false, cancellationToken);
        async internal protected Task<List<object>> RemoveRangeCascadeByMemoryOrDatabaseAsync(IEnumerable<TEntity> data, bool inMemory, CancellationToken cancellationToken = default)
        {
            //临时调用同步方法，后续会改
            return await Task.FromResult(RemoveRangeCascadeByMemoryOrDatabase(data, inMemory));
        }
        #endregion
    }
}
#endif