using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FreeSql
{
    partial class DbSet<TEntity>
    {

        void DbContextFlushCommand()
        {
            _dicUpdateTimes.Clear();
            _db.FlushCommand();
        }

        int DbContextBatchAdd(EntityState[] adds)
        {
            if (adds.Any() == false) return 0;
            var affrows = this.OrmInsert(adds.Select(a => a.Value)).ExecuteAffrows();
            _db._entityChangeReport.AddRange(adds.Select(a => new DbContext.EntityChangeReport.ChangeInfo { Object = a.Value, Type = DbContext.EntityChangeType.Insert }));
            return affrows;
        }

        #region Add
        void AddPriv(TEntity data, bool isCheck)
        {
            if (isCheck && CanAdd(data, true) == false) return;
            if (_tableIdentitys.Length > 0)
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
                        if (_tableIdentitys.Length == 1)
                        {
                            DbContextFlushCommand();
                            var idtval = this.OrmInsert(data).ExecuteIdentity();
                            IncrAffrows(1);
                            _db.OrmOriginal.SetEntityIdentityValueWithPrimary(_entityType, data, idtval);
                            _db._entityChangeReport.Add(new DbContext.EntityChangeReport.ChangeInfo { Object = data, Type = DbContext.EntityChangeType.Insert });
                            Attach(data);
                            if (_db.Options.EnableAddOrUpdateNavigateList)
                                AddOrUpdateNavigateList(data, true, null);
                        }
                        else
                        {
                            DbContextFlushCommand();
                            var newval = this.OrmInsert(data).ExecuteInserted().First();
                            _db._entityChangeReport.Add(new DbContext.EntityChangeReport.ChangeInfo { Object = newval, Type = DbContext.EntityChangeType.Insert });
                            IncrAffrows(1);
                            _db.OrmOriginal.MapEntityValue(_entityType, newval, data);
                            Attach(newval);
                            if (_db.Options.EnableAddOrUpdateNavigateList)
                                AddOrUpdateNavigateList(data, true, null);
                        }
                        return;
                    default:
                        if (_tableIdentitys.Length == 1)
                        {
                            DbContextFlushCommand();
                            var idtval = this.OrmInsert(data).ExecuteIdentity();
                            IncrAffrows(1);
                            _db.OrmOriginal.SetEntityIdentityValueWithPrimary(_entityType, data, idtval);
                            _db._entityChangeReport.Add(new DbContext.EntityChangeReport.ChangeInfo { Object = data, Type = DbContext.EntityChangeType.Insert });
                            Attach(data);
                            if (_db.Options.EnableAddOrUpdateNavigateList)
                                AddOrUpdateNavigateList(data, true, null);
                        }
                        return;
                }
            }
            EnqueueToDbContext(DbContext.EntityChangeType.Insert, CreateEntityState(data));
            Attach(data);
            if (_db.Options.EnableAddOrUpdateNavigateList)
                AddOrUpdateNavigateList(data, true, null);
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="data"></param>
        public void Add(TEntity data) => AddPriv(data, true);
        public void AddRange(IEnumerable<TEntity> data)
        {
            if (CanAdd(data, true) == false) return;
            if (data.ElementAtOrDefault(1) == default(TEntity))
            {
                Add(data.First());
                return;
            }
            if (_tableIdentitys.Length > 0)
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
                        DbContextFlushCommand();
                        var rets = this.OrmInsert(data).ExecuteInserted();
                        if (rets.Count != data.Count()) throw new Exception($"特别错误：批量添加失败，{_db.OrmOriginal.Ado.DataType} 的返回数据，与添加的数目不匹配");
                        _db._entityChangeReport.AddRange(rets.Select(a => new DbContext.EntityChangeReport.ChangeInfo { Object = a, Type = DbContext.EntityChangeType.Insert }));
                        var idx = 0;
                        foreach (var s in data)
                            _db.OrmOriginal.MapEntityValue(_entityType, rets[idx++], s);
                        IncrAffrows(rets.Count);
                        AttachRange(rets);
                        if (_db.Options.EnableAddOrUpdateNavigateList)
                            foreach (var item in data)
                                AddOrUpdateNavigateList(item, true, null);
                        return;
                    default:
                        foreach (var s in data)
                            AddPriv(s, false);
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
                        AddOrUpdateNavigateList(item, true, null);
            }
        }

        /// <summary>
        /// 保存实体的指定 ManyToMany/OneToMany 导航属性（完整对比）<para></para>
        /// 场景：在关闭级联保存功能之后，手工使用本方法<para></para>
        /// 例子：保存商品的 OneToMany 集合属性，SaveMany(goods, "Skus")<para></para>
        /// 当 goods.Skus 为空(非null)时，会删除表中已存在的所有数据<para></para>
        /// 当 goods.Skus 不为空(非null)时，添加/更新后，删除表中不存在 Skus 集合属性的所有记录
        /// </summary>
        /// <param name="item">实体对象</param>
        /// <param name="propertyName">属性名</param>
        public void SaveMany(TEntity item, string propertyName)
        {
            if (item == null) return;
            if (string.IsNullOrEmpty(propertyName)) return;
            if (_table.Properties.TryGetValue(propertyName, out var prop) == false) throw new KeyNotFoundException($"{_table.Type.FullName} 不存在属性 {propertyName}");
            if (_table.ColumnsByCsIgnore.ContainsKey(propertyName)) throw new ArgumentException($"{_table.Type.FullName} 类型已设置属性 {propertyName} 忽略特性");

            var tref = _table.GetTableRef(propertyName, true);
            if (tref == null) return;
            switch (tref.RefType)
            {
                case Internal.Model.TableRefType.OneToOne:
                case Internal.Model.TableRefType.ManyToOne:
                    throw new ArgumentException($"{_table.Type.FullName} 类型的属性 {propertyName} 不是 OneToMany 或 ManyToMany 特性");
            }

            DbContextFlushCommand();
            var oldEnable = _db.Options.EnableAddOrUpdateNavigateList;
            _db.Options.EnableAddOrUpdateNavigateList = false;
            try
            {
                AddOrUpdateNavigateList(item, false, propertyName);
                if (tref.RefType == Internal.Model.TableRefType.OneToMany)
                {
                    DbContextFlushCommand();
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
                    subDelete.ExecuteAffrows();
                }
            }
            finally
            {
                _db.Options.EnableAddOrUpdateNavigateList = oldEnable;
            }
        }
        void AddOrUpdateNavigateList(TEntity item, bool isAdd, string propertyName)
        {
            Action<PropertyInfo> action = prop =>
            {
                if (_table.ColumnsByCsIgnore.ContainsKey(prop.Name)) return;
                if (_table.ColumnsByCs.ContainsKey(prop.Name)) return;

                var tref = _table.GetTableRef(prop.Name, false); //防止非正常的导航属性报错
                if (tref == null) return;
                switch (tref.RefType)
                {
                    case Internal.Model.TableRefType.OneToOne:
                    case Internal.Model.TableRefType.ManyToOne:
                        return;
                }

                var propValEach = GetItemValue(item, prop) as IEnumerable;
                if (propValEach == null) return;
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
                                flagExists = refSet.Select.WhereDynamic(propValItem).Any();
                            if (refSet.CanAdd(propValItem, false) && flagExists != true)
                                refSet.Add(propValItem);
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
                            delall.ExecuteAffrows();
                            _db._entityChangeReport.Add(new DbContext.EntityChangeReport.ChangeInfo { Object = sql, Type = DbContext.EntityChangeType.SqlRaw });
                        }
                        else //保存
                        {
                            var midSet = GetDbSetObject(tref.RefMiddleEntityType);
                            List<object> midList = null;
                            if (isAdd == false)
                            {
                                var midSelect = midSet.Select;
                                foreach (var midWhere in midWheres) midSelect.Where(midWhere);
                                midList = midSelect.ToList();
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
                                var newItem = Activator.CreateInstance(tref.RefMiddleEntityType);
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
                            midSet.AddRange(midListAdd);
                        }
                        break;
                    case Internal.Model.TableRefType.OneToMany:
                        foreach (var propValItem in propValEach)
                        {
                            for (var colidx = 0; colidx < tref.Columns.Count; colidx++)
                            {
                                var val = FreeSql.Internal.Utils.GetDataReaderValue(tref.RefColumns[colidx].CsType, _db.OrmOriginal.GetEntityValueWithPropertyName(_table.Type, item, tref.Columns[colidx].CsName));
                                _db.OrmOriginal.SetEntityValueWithPropertyName(tref.RefEntityType, propValItem, tref.RefColumns[colidx].CsName, val);
                            }
                            refSet.AddOrUpdate(propValItem);
                        }
                        break;
                }
            };

            if (string.IsNullOrEmpty(propertyName))
                foreach (var prop in _table.Properties)
                    action(prop.Value);
            else if (_table.Properties.TryGetValue(propertyName, out var prop))
                action(prop);
        }
        static ConcurrentDictionary<Type, ConcurrentDictionary<string, FieldInfo>> _dicLazyIsSetField = new ConcurrentDictionary<Type, ConcurrentDictionary<string, FieldInfo>>();
        object GetItemValue(TEntity item, PropertyInfo prop)
        {
            object propVal = null;
            var itemType = item.GetType();
            if (_table.TypeLazy != null && itemType == _table.TypeLazy)
            {
                var lazyField = _dicLazyIsSetField.GetOrAdd(_table.TypeLazy, tl => new ConcurrentDictionary<string, FieldInfo>()).GetOrAdd(prop.Name, propName =>
                    _table.TypeLazy.GetField($"__lazy__{propName}", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance));
                if (lazyField != null)
                {
                    var lazyFieldValue = (bool)lazyField.GetValue(item);
                    if (lazyFieldValue == false) return null;
                }
                propVal = prop.GetValue(item, null);
            }
            else
            {
                propVal = prop.GetValue(item, null);
                if (propVal == null) return null;
            }
            return propVal;
        }
        #endregion

        #region Update
        int DbContextBatchUpdate(EntityState[] ups) => DbContextBatchUpdatePriv(ups, false);
        int DbContextBatchUpdateNow(EntityState[] ups) => DbContextBatchUpdatePriv(ups, true);
        int DbContextBatchUpdatePriv(EntityState[] ups, bool isLiveUpdate)
        {
            if (ups.Any() == false) return 0;
            var uplst1 = ups[ups.Length - 1];
            var uplst2 = ups.Length > 1 ? ups[ups.Length - 2] : null;

            if (_states.TryGetValue(uplst1.Key, out var lstval1) == false) return -999;
            var lstval2 = default(EntityState);
            if (uplst2 != null && _states.TryGetValue(uplst2.Key, out lstval2) == false) throw new Exception($"特别错误：更新失败，数据未被跟踪：{_db.OrmOriginal.GetEntityString(_entityType, uplst2.Value)}");

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
                var affrows = update.ExecuteAffrows();
                _db._entityChangeReport.AddRange(data.Select(a => new DbContext.EntityChangeReport.ChangeInfo
                {
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

        Dictionary<TEntity, byte> _dicUpdateTimes = new Dictionary<TEntity, byte>();
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="data"></param>
        public void Update(TEntity data)
        {
            var exists = ExistsInStates(data);
            if (exists == null) throw new Exception($"不可更新，未设置主键的值：{_db.OrmOriginal.GetEntityString(_entityType, data)}");
            if (exists == false)
            {
                var olddata = OrmSelect(data).First();
                if (olddata == null) throw new Exception($"不可更新，数据库不存在该记录：{_db.OrmOriginal.GetEntityString(_entityType, data)}");
            }

            UpdateRangePriv(new[] { data }, true);
        }
        public void UpdateRange(IEnumerable<TEntity> data) => UpdateRangePriv(data, true);
        void UpdateRangePriv(IEnumerable<TEntity> data, bool isCheck)
        {
            if (CanUpdate(data, true) == false) return;
            foreach (var item in data)
            {
                if (_dicUpdateTimes.ContainsKey(item))
                {
                    var itemCopy = CreateEntityState(item).Value;
                    DbContextFlushCommand();
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
            if (_db.Options.EnableAddOrUpdateNavigateList)
                foreach (var item in data)
                    AddOrUpdateNavigateList(item, false, null);
        }
        #endregion

        #region Remove
        int DbContextBatchRemove(EntityState[] dels)
        {
            if (dels.Any() == false) return 0;
            var affrows = this.OrmDelete(dels.Select(a => a.Value)).ExecuteAffrows();
            _db._entityChangeReport.AddRange(dels.Select(a => new DbContext.EntityChangeReport.ChangeInfo { Object = a.Value, Type = DbContext.EntityChangeType.Delete }));
            return affrows; //https://github.com/dotnetcore/FreeSql/issues/373
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="data"></param>
        public void Remove(TEntity data) => RemoveRange(new[] { data });
        public void RemoveRange(IEnumerable<TEntity> data)
        {
            if (CanRemove(data, true) == false) return;
            foreach (var item in data)
            {
                var state = CreateEntityState(item);
                _states.TryRemove(state.Key, out var trystate);
                _db.OrmOriginal.ClearEntityPrimaryValueWithIdentityAndGuid(_entityType, item);

                EnqueueToDbContext(DbContext.EntityChangeType.Delete, state);
            }
        }
        /// <summary>
        /// 根据 lambda 条件删除数据
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public int Remove(Expression<Func<TEntity, bool>> predicate)
        {
            DbContextFlushCommand();
            return this.OrmDelete(null).Where(predicate).ExecuteAffrows();
        }
        #endregion

        #region AddOrUpdate
        /// <summary>
        /// 添加或更新
        /// </summary>
        /// <param name="data"></param>
        public void AddOrUpdate(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (_table.Primarys.Any() == false) throw new Exception($"不可添加，实体没有主键：{_db.OrmOriginal.GetEntityString(_entityType, data)}");

            var flagExists = ExistsInStates(data);
            if (flagExists == false)
            {
                var olddata = OrmSelect(data).First();
                flagExists = olddata != null;
            }

            if (flagExists == true && CanUpdate(data, false))
            {
                DbContextFlushCommand();
                var affrows = _db._affrows;
                UpdateRangePriv(new[] { data }, false);
                DbContextFlushCommand();
                affrows = _db._affrows - affrows;
                if (affrows > 0) return;
            }
            if (CanAdd(data, false))
            {
                _db.OrmOriginal.ClearEntityPrimaryValueWithIdentity(_entityType, data);
                AddPriv(data, false);
            }
        }
        #endregion

        #region BeginEdit
        protected List<TEntity> _dataEditing;
        protected ConcurrentDictionary<string, EntityState> _statesEditing = new ConcurrentDictionary<string, EntityState>();

        /// <summary>
        /// 开始编辑数据，然后调用方法 EndEdit 分析出添加、修改、删除 SQL 语句进行执行<para></para>
        /// 场景：winform 加载表数据后，一顿添加、修改、删除操作之后，最后才点击【保存】<para></para><para></para>
        /// 示例：https://github.com/dotnetcore/FreeSql/issues/397<para></para>
        /// 注意：* 本方法只支持单表操作，不支持导航属性级联保存
        /// </summary>
        /// <param name="data"></param>
        public void BeginEdit(List<TEntity> data)
        {
            if (data == null) return;
            if (_table.Primarys.Any() == false) throw new Exception($"不可进行编辑，实体没有主键：{_db.OrmOriginal.GetEntityString(_entityType, data.First())}");
            _statesEditing.Clear();
            _dataEditing = data;
            foreach (var item in data)
            {
                var key = _db.OrmOriginal.GetEntityKeyString(_entityType, item, false);
                if (string.IsNullOrEmpty(key)) continue;

                _statesEditing.AddOrUpdate(key, k => CreateEntityState(item), (k, ov) =>
                {
                    _db.OrmOriginal.MapEntityValue(_entityType, item, ov.Value);
                    ov.Time = DateTime.Now;
                    return ov;
                });
            }
        }
        /// <summary>
        /// 完成编辑数据，进行保存动作<para></para>
        /// 该方法根据 BeginEdit 传入的数据状态分析出添加、修改、删除 SQL 语句<para></para>
        /// 注意：* 本方法只支持单表操作，不支持导航属性级联保存
        /// </summary>
        /// <param name="data">可选参数：手工传递最终的 data 值进行对比<para></para>默认：如果不传递，则使用 BeginEdit 传入的 data 引用进行对比</param>
        /// <returns></returns>
        public int EndEdit(List<TEntity> data = null)
        {
            if (data == null) data = _dataEditing;
            var beforeAffrows = 0;
            if (data == null) return 0;
            var oldEnable = _db.Options.EnableAddOrUpdateNavigateList;
            _db.Options.EnableAddOrUpdateNavigateList = false;
            try
            {
                DbContextFlushCommand();
                var addList = new List<TEntity>();
                var ediList = new List<NativeTuple<TEntity, string>>();
                foreach (var item in data)
                {
                    var key = _db.OrmOriginal.GetEntityKeyString(_entityType, item, false);
                    if (_statesEditing.TryRemove(key, out var state) == false)
                    {
                        addList.Add(item);
                        continue;
                    }
                    _states.AddOrUpdate(key, k => state, (k, ov) =>
                    {
                        ov.Value = state.Value;
                        ov.Time = DateTime.Now;
                        return ov;
                    });
                    var edicmp = _db.OrmOriginal.CompareEntityValueReturnColumns(_entityType, item, state.Value, false);
                    if (edicmp.Any())
                        ediList.Add(NativeTuple.Create(item, string.Join(",", edicmp)));
                }
                beforeAffrows = _db._affrows;
                AddRange(addList);
                UpdateRange(ediList.OrderBy(a => a.Item2).Select(a => a.Item1).ToList());

                DbContextFlushCommand();
                var delList = _statesEditing.Values.OrderBy(a => a.Time).ToArray();
                _db._affrows += DbContextBatchRemove(delList); //为了减的少不必要的开销，此处没有直接调用 RemoveRange
                foreach (var state in delList)
                {
                    _db.OrmOriginal.ClearEntityPrimaryValueWithIdentityAndGuid(_entityType, state.Value);
                    _states.TryRemove(state.Key, out var oldstate);
                }
                DbContextFlushCommand();
            }
            finally
            {
                _dataEditing = null;
                _statesEditing.Clear();
                _db.Options.EnableAddOrUpdateNavigateList = oldEnable;
            }
            return _db._affrows - beforeAffrows;
        }
        #endregion
    }
}
