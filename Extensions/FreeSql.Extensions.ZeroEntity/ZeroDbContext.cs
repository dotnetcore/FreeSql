using FreeSql.DataAnnotations;
using FreeSql.Extensions.ZeroEntity.Models;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using T = System.Collections.Generic.Dictionary<string, object>;

namespace FreeSql.Extensions.ZeroEntity
{

    /*
理解本机制之前，请先忘记 Repository/DbContext 等之前有关级联的内容，他们没有关联。

schemas[] 是一组表映射信息定义，包含表名、列名、导航属性、索引等信息
导航属性：OneToOne/OneToMany/ManyToOne/ManyToMany
聚合根：OneToOne/OneToMany/多对多中间表，作为一个整体看待
外部根：ManyToOne/ManyToMany外部表，作为外部看待，它有自己的聚合根整体
User 为聚合根
UserExt/UserClaim/UserRole 这三个表是子成员，一起存储/删除
Role 为外部根（相对 User 而言，它自己是独立的聚合根）

CURD 都是基于 schemas[0] 聚合根进行操作
查询：贪婪加载所有子成员，以及外部根，以及外部根的外部根（递归）
状态管理：快照聚合根副本（由于外部根也是聚合根，即外部根与聚合根是并行存储关系）

对比保存：
将当前操作的聚合根与状态管理的副本进行对比，计算出发生变化的列
OneToOne 副本NULL 最新Object 添加新记录
OneToOne 副本Object 最新NULL 删除副本记录
OneToOne 副本Object 最新Object 发生变化则更新，否则忽略
OneToMany 副本NULL/Empty 最新List 添加最新List记录
OneToMany 副本List 最新NULL 忽略
OneToMany 副本List 最新Empty 删除副本List记录
OneToMany 副本List 最新List 对比保存，计算出添加/更新/删除三种行为
多对多中间表 与 OneToMany 一致

插入：
OneToOne 级联插入
OneToMany 级联插入
ManyToOne 先对比保存外部根，关联外部根ID，再插入聚合根
ManyToMany 先对比保存外部根，插入聚合根，再插入中间表

更新：
OneToOne 级联对比保存
OneToMany 级联对比保存
ManyToOne 先对比保存外部根，再关联外部根ID，再更新聚合根
ManyToMany 先对比保存外部根，再更新聚合根，再对比保存中间表

删除：
OneToOne 级联删除
OneToMany 级联删除
ManyToOne 忽略
ManyToMany 级联删除中间表（注意不删除外部根）
	 */
    public partial class ZeroDbContext
    {
        internal IFreeSql _orm;
        internal DbTransaction _transaction;
        internal int _commandTimeout;
        internal List<ZeroTableInfo> _tables;

        /// <summary>
        /// 创建新的ZeroDbCotext实例
        /// </summary>
        /// <param name="orm">IfreeSql 对象</param>
        /// <param name="schemas">动态表结构描述</param>
        /// <param name="syncStructure">是否强制同步表结构</param>
        /// <exception cref="SchemaValidationResult"> Schema 未验证通过时抛出验证异常</exception>
        public ZeroDbContext(IFreeSql orm, TableDescriptor[] schemas, bool syncStructure = false)
        {
            _orm = orm;
            _tables = ValidateSchemaToInfoInternal(orm, schemas);
            if (syncStructure || orm.CodeFirst.IsAutoSyncStructure)
            {
                foreach (var table in _tables)
                    orm.CodeFirst.SyncStructure(table, table.DbName, false);
            }
        }

        /// <summary>
        /// 初始化一个 ZeroDbContext 对象，暂不指定任何Schema
        /// </summary>
        /// <param name="orm"></param>
        public ZeroDbContext(IFreeSql orm)
        {
            _orm = orm;
            _tables = new List<ZeroTableInfo>();
        }

        public SchemaValidationResult ValidateSchema(IEnumerable<TableDescriptor> schemas)
        {
            try
            {
                ValidateSchemaToInfoInternal(_orm, schemas);
            }
            catch (SchemaValidationException ex)
            {
                return new SchemaValidationResult(ex.Message);
            }
            return SchemaValidationResult.SuccessedResult;
        }

        public TableInfo GetTableInfo(string name) => _tables.Where(a => a.CsName == name).FirstOrDefault();

        public void SyncStructure()
        {
            foreach (var table in _tables)
                _orm.CodeFirst.SyncStructure(table, table.DbName, false);
        }

        public void SyncStructure(TableDescriptor[] schemas)
        {
            _tables = ValidateSchemaToInfoInternal(_orm, schemas);
            foreach (var table in _tables)
                _orm.CodeFirst.SyncStructure(table, table.DbName, false);
        }

        /// <summary>
        /// 同步指定表结构
        /// </summary>
        /// <param name="name"></param>
        public void SyncTableStructure(string name)
        {
            var table = GetTableInfo(name);
            _orm.CodeFirst.SyncStructure(table, table.DbName, false);
        }


        static List<ZeroTableInfo> ValidateSchemaToInfoInternal(IFreeSql orm, IEnumerable<TableDescriptor> schemas)
        {
            var common = (orm.Ado as AdoProvider)._util;
            var tables = new List<ZeroTableInfo>();
            foreach (var dtd in schemas)
            {
                if (string.IsNullOrWhiteSpace(dtd.Name)) continue;
                if (string.IsNullOrWhiteSpace(dtd.DbName)) dtd.DbName = dtd.Name;
                var tabattr = new TableAttribute
                {
                    Name = dtd.DbName,
                    AsTable = dtd.AsTable,
                };
                var tabindexs = dtd.Indexes.Select(a => new IndexAttribute(a.Name, a.Fields, a.IsUnique)
                {
                    IndexMethod = a.IndexMethod,
                });
                var tab = new ZeroTableInfo();
                tab.Comment = dtd.Comment;
                tab.Type = typeof(object);
                tab.CsName = dtd.Name;
                tab.DbName = dtd.DbName;
                var isQuery = tab.DbName.StartsWith("(") && tab.DbName.EndsWith(")");

                if (isQuery == false)
                {
                    if (orm.CodeFirst.IsSyncStructureToLower) tab.DbName = tab.DbName.ToLower();
                    if (orm.CodeFirst.IsSyncStructureToUpper) tab.DbName = tab.DbName.ToUpper();
                }
                tab.DisableSyncStructure = isQuery || dtd.DisableSyncStructure;
                tab.IsDictionaryType = true;
                var columnsList = new List<ColumnInfo>();
                foreach (var dtdcol in dtd.Columns)
                {
                    if (string.IsNullOrWhiteSpace(dtdcol.Name) || dtdcol.MapType == null || tab.ColumnsByCs.ContainsKey(dtdcol.Name)) continue;
                    var tp = common.CodeFirst.GetDbInfo(dtdcol.MapType);
                    var colattr = dtdcol.ToAttribute();
                    var col = Utils.ColumnAttributeToInfo(tab, null, colattr.Name, colattr.MapType, false, ref colattr, tp, common);
                    if (col == null) continue;

                    col.Comment = dtdcol.Comment;
                    tab.Columns.Add(col.Attribute.Name, col);
                    tab.ColumnsByCs.Add(col.CsName, col);
                    columnsList.Add(col);
                }
                Utils.AuditTableInfo(tab, tabattr, tabindexs, columnsList, common);
                columnsList.Clear();
                tables.Add(tab);
            }
            var tabindex = 0;
            foreach (var dtd in schemas)
            {
                var tab = tables[tabindex++];
                foreach (var dtdnav in dtd.Navigates)
                {
                    if (tab.Navigates.ContainsKey(dtdnav.Name)) continue;
                    var error = $"表“{tab.CsName}”导航属性 {dtdnav.Name} 配置错误：";
                    var nav = new ZeroTableRef();
                    nav.NavigateKey = dtdnav.Name;
                    nav.Table = tab;
                    nav.RefTable = tables.Where(a => a.CsName == dtdnav.RelTable).FirstOrDefault();

                    if (nav.RefTable == null) throw new SchemaValidationException($"{error}未定义“{dtdnav.RelTable}”");

                    switch (dtdnav.Type)
                    {
                        case TableDescriptor.NavigateType.OneToOne:
                            nav.RefType = TableRefType.OneToOne;
                            nav.Columns.AddRange(nav.Table.Primarys.Select(a => a.CsName));
                            if (string.IsNullOrWhiteSpace(dtdnav.Bind))
                                nav.RefColumns.AddRange(nav.RefTable.Primarys.Select(a => a.CsName));
                            else
                                nav.RefColumns.AddRange(dtdnav.Bind.Split(',')
                                    .Select(a => nav.RefTable.ColumnsByCs.TryGetValue(a.Trim(), out var refcol) ? refcol.CsName : "")
                                    .Where(a => string.IsNullOrWhiteSpace(a) == false));
                            break;
                        case TableDescriptor.NavigateType.ManyToOne:
                            nav.RefType = TableRefType.ManyToOne;
                            nav.Columns.AddRange(dtdnav.Bind.Split(',')
                                    .Select(a => nav.Table.ColumnsByCs.TryGetValue(a.Trim(), out var refcol) ? refcol.CsName : "")
                                    .Where(a => string.IsNullOrWhiteSpace(a) == false));
                            nav.RefColumns.AddRange(nav.RefTable.Primarys.Select(a => a.CsName));
                            break;
                        case TableDescriptor.NavigateType.OneToMany:
                            nav.RefType = TableRefType.OneToMany;
                            nav.Columns.AddRange(nav.Table.Primarys.Select(a => a.CsName));
                            nav.RefColumns.AddRange(dtdnav.Bind.Split(',')
                                    .Select(a => nav.RefTable.ColumnsByCs.TryGetValue(a.Trim(), out var refcol) ? refcol.CsName : "")
                                    .Where(a => string.IsNullOrWhiteSpace(a) == false));
                            break;
                        case TableDescriptor.NavigateType.ManyToMany:
                            nav.RefType = TableRefType.ManyToMany;
                            var midtab = tables.Where(a => a.CsName == dtdnav.ManyToMany).FirstOrDefault();
                            nav.RefMiddleTable = midtab;
                            if (nav.RefMiddleTable == null) throw new SchemaValidationException($"{error}ManyToMany未定义“{dtdnav.ManyToMany}”");
                            var midtabRaw = schemas.Where(a => a.Name == midtab.CsName).FirstOrDefault();
                            var midTabNav1 = midtabRaw.Navigates.Where(a => a.Type == TableDescriptor.NavigateType.ManyToOne && a.RelTable == nav.Table.CsName).FirstOrDefault();
                            if (midTabNav1 == null) throw new SchemaValidationException($"{error}ManyToMany中间表“{dtdnav.ManyToMany}”没有与表“{nav.Table.CsName}”形成 ManyToOne 关联");
                            var midTabNav1Columns = midTabNav1.Bind.Split(',')
                                    .Select(a => midtab.ColumnsByCs.TryGetValue(a.Trim(), out var refcol) ? refcol.CsName : "")
                                    .Where(a => string.IsNullOrWhiteSpace(a) == false).ToArray();
                            var midTabNav2 = midtabRaw.Navigates.Where(a => a.Type == TableDescriptor.NavigateType.ManyToOne && a.RelTable == nav.RefTable.CsName).FirstOrDefault();
                            if (midTabNav2 == null) throw new SchemaValidationException($"{error}ManyToMany中间表“{dtdnav.ManyToMany}”没有与表“{nav.RefTable.CsName}”形成 ManyToOne 关联");
                            var midTabNav2Columns = midTabNav2.Bind.Split(',')
                                    .Select(a => midtab.ColumnsByCs.TryGetValue(a.Trim(), out var refcol) ? refcol.CsName : "")
                                    .Where(a => string.IsNullOrWhiteSpace(a) == false).ToArray();
                            if (midTabNav1Columns.Length != nav.Table.Primarys.Length) throw new SchemaValidationException($"{error}ManyToMany中间表“{dtdnav.ManyToMany}”关联字段的数目不相等");
                            if (midTabNav1Columns.Where((a, idx) => midtab.ColumnsByCs[a].CsType != nav.Table.Primarys[idx].CsType).Any()) throw new SchemaValidationException($"{error}ManyToMany中间表“{dtdnav.ManyToMany}”关联字段的类型不相等");
                            if (midTabNav2Columns.Length != nav.RefTable.Primarys.Length) throw new SchemaValidationException($"{error}ManyToMany中间表“{dtdnav.ManyToMany}”与表“{nav.RefTable.CsName}”关联字段的数目不相等");
                            if (midTabNav2Columns.Where((a, idx) => midtab.ColumnsByCs[a].CsType != nav.RefTable.Primarys[idx].CsType).Any()) throw new SchemaValidationException($"{error}ManyToMany中间表“{dtdnav.ManyToMany}”与表“{nav.RefTable.CsName}”关联字段的类型不相等");
                            nav.Columns.AddRange(nav.Table.Primarys.Select(a => a.CsName));
                            nav.MiddleColumns.AddRange(midTabNav1Columns);
                            nav.MiddleColumns.AddRange(midTabNav2Columns);
                            nav.RefColumns.AddRange(nav.RefTable.Primarys.Select(a => a.CsName));
                            break;
                    }
                    switch (dtdnav.Type)
                    {
                        case TableDescriptor.NavigateType.OneToOne:
                        case TableDescriptor.NavigateType.ManyToOne:
                        case TableDescriptor.NavigateType.OneToMany:
                            if (nav.Columns.Any() == false || nav.Columns.Count != nav.RefColumns.Count) throw new SchemaValidationException($"{error}与表“{dtdnav.RelTable}”关联字段的数目不相等");
                            if (nav.Columns.Where((a, idx) => nav.Table.ColumnsByCs[a].CsType != nav.RefTable.ColumnsByCs[nav.RefColumns[idx]].CsType).Any()) throw new SchemaValidationException($"{error}与表“{dtdnav.RelTable}”关联字段的类型不匹配");
                            break;
                    }
                    tab.Navigates.Add(dtdnav.Name, nav);
                }
            }
            return tables;
        }

        public ZeroDbContext WithTransaction(DbTransaction value)
        {
            _transaction = value;
            return this;
        }
        public ZeroDbContext CommandTimeout(int seconds)
        {
            _commandTimeout = seconds;
            return this;
        }
        void TransactionInvoke(Action handler)
        {
            if (_transaction == null)
            {
                var threadTransaction = _orm.Ado.TransactionCurrentThread;
                if (threadTransaction != null) this.WithTransaction(threadTransaction);
            }
            if (_transaction != null)
            {
                handler?.Invoke();
                return;
            }
            using (var conn = _orm.Ado.MasterPool.Get())
            {
                _transaction = conn.Value.BeginTransaction();
                var transBefore = new Aop.TraceBeforeEventArgs("BeginTransaction", null);
                try
                {
                    _orm.Aop.TraceBeforeHandler?.Invoke(this, transBefore);
                    handler?.Invoke();
                    _transaction.Commit();
                    _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreErrorStrings.Commit, null));
                }
                catch (Exception ex)
                {
                    _transaction.Rollback();
                    _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreErrorStrings.RollBack, ex));
                    throw;
                }
                finally
                {
                    _transaction = null;
                }
            }
        }
        /// <summary>
        /// 【有状态管理】自动 Include 查询
        /// </summary>
        public SelectImpl Select => new SelectImpl(this, _tables[0].CsName).IncludeAll();
        /// <summary>
        /// 【无状态管理】指定表查询
        /// </summary>
        public SelectImpl SelectNoTracking(string tableName) => new SelectImpl(this, tableName).NoTracking();

        public int Insert(T entity) => Insert(new[] { entity });
        public int Insert(IEnumerable<T> entities)
        {
            _cascadeAffrows = 0;
            _cascadeIgnores.Clear();
            try
            {
                TransactionInvoke(() =>
                {
                    AuditCascade(_tables[0], entities);
                    InsertCascade(_tables[0], entities, true);
                });
                return _cascadeAffrows;
            }
            finally
            {
                _cascadeAffrows = 0;
                _cascadeIgnores.Clear();
            }
        }
        public int Update(T entity) => Update(new[] { entity });
        public int Update(IEnumerable<T> entities)
        {
            _cascadeAffrows = 0;
            _cascadeIgnores.Clear();
            try
            {
                TransactionInvoke(() =>
                {
                    AuditCascade(_tables[0], entities);
                    UpdateCascade(_tables[0], entities, true);
                });
                return _cascadeAffrows;
            }
            finally
            {
                _cascadeAffrows = 0;
                _cascadeIgnores.Clear();
            }
        }
        public int Delete(T entity) => Delete(new[] { entity });
        public int Delete(IEnumerable<T> entities)
        {
            _cascadeAffrows = 0;
            _cascadeIgnores.Clear();
            try
            {
                TransactionInvoke(() =>
                {
                    AuditCascade(_tables[0], entities);
                    DeleteCascade(_tables[0], entities, null);
                });
                return _cascadeAffrows;
            }
            finally
            {
                _cascadeAffrows = 0;
                _cascadeIgnores.Clear();
            }
        }
        public void FlushState()
        {
            _states.Clear();
        }
        public void Attach(T entity)
        {
            AuditCascade(_tables[0], entity);
            AttachCascade(_tables[0], entity, true);
        }

        void AuditCascade(ZeroTableInfo entityTable, IEnumerable<T> entities)
        {
            if (entities == null) return;
            foreach (var entity in entities) AuditCascade(entityTable, entity);
        }
        internal void AuditCascade(ZeroTableInfo entityTable, T entity)
        {
            var ignores = new Dictionary<string, Dictionary<string, bool>>(); //比如 Tree 结构可以递归添加
            LocalAuditCascade(entityTable, entity);
            ignores.Clear();

            void LocalAuditCascade(ZeroTableInfo table, T entityFrom)
            {
                if (entityFrom == null) return;

                var stateKey = GetEntityKeyString(table, entityFrom);
                if (ignores.TryGetValue(table.DbName, out var stateKeys) == false) ignores.Add(table.DbName, stateKeys = new Dictionary<string, bool>());
                if (stateKey != null)
                {
                    if (stateKeys.ContainsKey(stateKey)) return;
                    stateKeys.Add(stateKey, true);
                }

                foreach (var col in table.Columns.Values)
                    if (entityFrom.TryGetValue(col.CsName, out var colval))
                        entityFrom[col.CsName] = Utils.GetDataReaderValue(col.CsType, colval);

                foreach (var nav in table.Navigates)
                {
                    if (entityFrom.TryGetValue(nav.Key, out var propvalFrom) == false || propvalFrom == null) continue;
                    switch (nav.Value.RefType)
                    {
                        case TableRefType.OneToOne:
                            {
                                if (propvalFrom is T valFrom == false) valFrom = LocalAuditJsonElement(nv => entityFrom[nav.Key] = nv, propvalFrom) as T;
                                if (valFrom == null) continue;
                                SetNavigateRelationshipValue(nav.Value, entityFrom, valFrom);
                                LocalAuditCascade(nav.Value.RefTable, valFrom);
                            }
                            break;
                        case TableRefType.OneToMany:
                            {
                                if (propvalFrom is IEnumerable valFromList == false) valFromList = LocalAuditJsonElement(nv => entityFrom[nav.Key] = nv, propvalFrom) as IEnumerable;
                                else if (valFromList != null)
                                {
                                    foreach (var fromObj in valFromList)
                                    {
                                        if (fromObj is T == false) valFromList = LocalAuditJsonElement(nv => entityFrom[nav.Key] = nv, propvalFrom) as IEnumerable;
                                        break;
                                    }
                                }
                                if (valFromList == null) continue;
                                SetNavigateRelationshipValue(nav.Value, entityFrom, valFromList);
                                foreach (var fromObj in valFromList)
                                {
                                    if (fromObj is T fromItem == false || fromItem == null) continue;
                                    LocalAuditCascade(nav.Value.RefTable, fromItem);
                                }
                            }
                            break;
                        case TableRefType.ManyToMany:
                            {
                                if (propvalFrom is IEnumerable valFromList == false) valFromList = LocalAuditJsonElement(nv => entityFrom[nav.Key] = nv, propvalFrom) as IEnumerable;
                                else if (valFromList != null)
                                {
                                    foreach (var fromObj in valFromList)
                                    {
                                        if (fromObj is T == false) valFromList = LocalAuditJsonElement(nv => entityFrom[nav.Key] = nv, propvalFrom) as IEnumerable;
                                        break;
                                    }
                                }
                                if (valFromList == null) continue;
                                foreach (var fromObj in valFromList)
                                {
                                    if (fromObj is T fromItem == false || fromItem == null) continue;
                                    LocalAuditCascade(nav.Value.RefTable, fromItem);
                                }
                            }
                            break;
                        case TableRefType.ManyToOne:
                            {
                                if (propvalFrom is T valFrom == false) valFrom = LocalAuditJsonElement(nv => entityFrom[nav.Key] = nv, propvalFrom) as T;
                                if (valFrom == null) continue;
                                LocalAuditCascade(nav.Value.RefTable, valFrom);
                            }
                            break;
                    }
                }
            }

            object LocalAuditJsonElement(Func<object, object> resetValue, object value)
            {
                if (value == null) return null;
                var valueType = value.GetType();
                switch (valueType.FullName)
                {
                    case "Newtonsoft.Json.Linq.JArray":
                        {
                            if (value is IEnumerable valueEach == false) return resetValue?.Invoke(null);
                            var newValue = new List<object>();
                            foreach (var valueItem in valueEach)
                                newValue.Add(LocalAuditJsonElement(null, valueItem));
                            return resetValue?.Invoke(newValue) ?? newValue;
                        }
                    case "Newtonsoft.Json.Linq.JObject":
                        {
                            if (_AuditJsonElementMethodJObjectToObject == null)
                                lock (_AuditJsonElementMethodLock)
                                    if (_AuditJsonElementMethodJObjectToObject == null)
                                        _AuditJsonElementMethodJObjectToObject = valueType.GetMethod("ToObject", new[] { typeof(Type) });

                            var newValue = _AuditJsonElementMethodJObjectToObject.Invoke(value, new object[] { typeof(T) });
                            return resetValue?.Invoke(newValue) ?? newValue;
                        }
                    case "System.Text.Json.JsonElement":
                        {
                            if (_AuditJsonElementPropertyJsonElementValueKind == null)
                                lock (_AuditJsonElementMethodLock)
                                    if (_AuditJsonElementPropertyJsonElementValueKind == null)
                                    {
                                        _AuditJsonElementPropertyJsonElementValueKind = valueType.GetProperty("ValueKind");
                                        _AuditJsonElementMethodJsonElementEnumerateObject = valueType.GetMethod("EnumerateObject", new Type[0]);
                                        _AuditJsonElementMethodJsonElementEnumerateArray = valueType.GetMethod("EnumerateArray", new Type[0]);
                                    }

                            var valueKind = _AuditJsonElementPropertyJsonElementValueKind.GetValue(value, null).ToString();
                            switch (valueKind)
                            {
                                case "Object":
                                    {
                                        var valueEach = _AuditJsonElementMethodJsonElementEnumerateObject.Invoke(value, new object[0]) as IEnumerable;
                                        if (valueEach == null) return resetValue?.Invoke(null);
                                        var newValue = new T();
                                        foreach (var valueItem in valueEach)
                                        {
                                            if (valueItem == null) continue;
                                            var valueItemType = valueItem.GetType();
                                            if (_AuditJsonElementPropertyJsonPropertyName == null)
                                                lock (_AuditJsonElementMethodLock)
                                                    if (_AuditJsonElementPropertyJsonPropertyName == null)
                                                    {
                                                        _AuditJsonElementPropertyJsonPropertyName = valueItemType.GetProperty("Name");
                                                        _AuditJsonElementPropertyJsonPropertyValue = valueItemType.GetProperty("Value");
                                                    }

                                            var name = _AuditJsonElementPropertyJsonPropertyName.GetValue(valueItem, null)?.ToString();
                                            if (name != null) newValue[name] = _AuditJsonElementPropertyJsonPropertyValue.GetValue(valueItem, null);
                                        }
                                        return resetValue?.Invoke(newValue) ?? newValue;
                                    }
                                case "Array":
                                    {
                                        var valueEach = _AuditJsonElementMethodJsonElementEnumerateArray.Invoke(value, new object[0]) as IEnumerable;
                                        if (valueEach == null) return resetValue?.Invoke(null);
                                        var newValue = new List<object>();
                                        foreach (var valueItem in valueEach)
                                            newValue.Add(LocalAuditJsonElement(null, valueItem));
                                        return resetValue?.Invoke(newValue) ?? newValue;
                                    }
                            }
                            break;
                        }
                }
                return value;
            }
        }
        static object _AuditJsonElementMethodLock = new object();
        static MethodInfo _AuditJsonElementMethodJObjectToObject, _AuditJsonElementMethodJsonElementEnumerateObject, _AuditJsonElementMethodJsonElementEnumerateArray;
        static PropertyInfo _AuditJsonElementPropertyJsonElementValueKind, _AuditJsonElementPropertyJsonPropertyName, _AuditJsonElementPropertyJsonPropertyValue;

        public class ChangeReport
        {
            public class ChangeInfo
            {
                public T Object { get; set; }
                /// <summary>
                /// Type = Update 的时候，获取更新之前的对象
                /// </summary>
                public T BeforeObject { get; set; }
                public ChangeType Type { get; set; }
                public string TableName { get; set; }
            }
            public enum ChangeType { Insert, Update, Delete }
            /// <summary>
            /// 实体变化记录
            /// </summary>
            public List<ChangeInfo> Report { get; } = new List<ChangeInfo>();
            /// <summary>
            /// 实体变化事件
            /// </summary>
            public Action<List<ChangeInfo>> OnChange { get; set; }
        }
        internal List<ChangeReport.ChangeInfo> _changeReport = new List<ChangeReport.ChangeInfo>();
        int _cascadeAffrows = 0;
        Dictionary<string, Dictionary<string, bool>> _cascadeIgnores = new Dictionary<string, Dictionary<string, bool>>(); //比如 Tree 结构可以递归添加
        Dictionary<string, Dictionary<string, bool>> _cascadeAuditEntityIgnores = new Dictionary<string, Dictionary<string, bool>>();
        bool CanCascade(TableInfo entityTable, T entity, bool isadd)
        {
            var stateKey = GetEntityKeyString(entityTable, entity, false);
            if (stateKey == null) return true;
            if (_cascadeIgnores.TryGetValue(entityTable.DbName, out var stateKeys) == false)
            {
                if (isadd)
                {
                    _cascadeIgnores.Add(entityTable.DbName, stateKeys = new Dictionary<string, bool>());
                    stateKeys.Add(stateKey, true);
                }
                return true;
            }
            if (stateKeys.ContainsKey(stateKey) == false)
            {
                if (isadd) stateKeys.Add(stateKey, true);
                return true;
            }
            return false;
        }
        void InsertCascade(ZeroTableInfo entityTable, IEnumerable<T> entities, bool cascade)
        {
            var navs = entityTable.Navigates.OrderBy(a => a.Value.RefType).ThenBy(a => a.Key).ToArray();
            SaveOutsideCascade(entities, navs);

            if (entityTable.Primarys.Any(col => col.Attribute.IsIdentity))
            {
                foreach (var idcol in entityTable.Primarys.Where(col => col.Attribute.IsIdentity))
                    foreach (var entity in entities)
                        entity.Remove(idcol.CsName);
            }
            LocalAddRange(entityTable, entities);
            foreach (var entity in entities)
            {
                if (cascade == false) AttachCascade(entityTable, entity, false);
                CanCascade(entityTable, entity, true); //刷新 _cascadeIgnores
            }
            if (cascade == false) return;

            foreach (var nav in navs)
            {
                switch (nav.Value.RefType)
                {
                    case TableRefType.OneToOne:
                        {
                            var otoList = entities.Select(entity =>
                            {
                                if (entity.TryGetValue(nav.Key, out var otoItemObj) == false ||
                                    otoItemObj is T otoItem == false ||
                                    otoItem == null || CanCascade(nav.Value.RefTable, otoItem, false) == false) return null;
                                SetNavigateRelationshipValue(nav.Value, entity, otoItem);
                                return otoItem;
                            }).Where(entity => entity != null).ToArray();
                            if (otoList.Any())
                                InsertCascade(nav.Value.RefTable, otoList, true);
                            break;
                        }
                    case TableRefType.OneToMany:
                        {
                            var otmList = entities.Select(entity =>
                            {
                                if (entity.TryGetValue(nav.Key, out var otmEachObj) == false ||
                                    otmEachObj is IEnumerable otmEach == false ||
                                    otmEach == null) return null;
                                var otmItems = new List<T>();
                                foreach (var otmItemObj in otmEach)
                                {
                                    var otmItem = otmItemObj as T;
                                    if (otmItem == null || CanCascade(nav.Value.RefTable, otmItem, false) == false) continue;
                                    otmItems.Add(otmItem);
                                }
                                SetNavigateRelationshipValue(nav.Value, entity, otmItems);
                                return otmItems;
                            }).Where(entity => entity != null).SelectMany(entity => entity).ToArray();
                            if (otmList.Any())
                                InsertCascade(nav.Value.RefTable, otmList, true);
                            break;
                        }
                    case TableRefType.ManyToMany:
                        {
                            var mtmMidList = new List<T>();
                            foreach (var entity in entities)
                            {
                                var mids = GetManyToManyObjects(nav.Value, entity, nav.Key);
                                if (mids?.Any() == true) mtmMidList.AddRange(mids);
                            }
                            if (mtmMidList.Any())
                                InsertCascade(nav.Value.RefMiddleTable, mtmMidList, false);
                            break;
                        }
                }
            }

            foreach (var entity in entities) AttachCascade(entityTable, entity, false);

            #region LocalAdd
            InsertProvider<T> OrmInsert(TableInfo table)
            {
                var insertDict = _orm.Insert<T>().WithTransaction(_transaction).CommandTimeout(_commandTimeout) as InsertProvider<T>;
                insertDict._table = table;
                return insertDict;
            }
            void LocalAdd(TableInfo table, T data, bool isCheck, ColumnInfo[] _tableReturnColumns, ColumnInfo[] _tableIdentitys)
            {
                if (isCheck && LocalCanAdd(table, data, true) == false) return;
                if (_tableReturnColumns == null) _tableReturnColumns = table.ColumnsByPosition.Where(a => a.Attribute.IsIdentity || string.IsNullOrWhiteSpace(a.DbInsertValue) == false).ToArray();
                if (_tableReturnColumns.Length > 0)
                {
                    if (_tableIdentitys == null) _tableIdentitys = table.ColumnsByPosition.Where(a => a.Attribute.IsIdentity).ToArray();
                    switch (_orm.Ado.DataType)
                    {
                        case DataType.SqlServer:
                        case DataType.OdbcSqlServer:
                        case DataType.CustomSqlServer:
                        case DataType.PostgreSQL:
                        case DataType.OdbcPostgreSQL:
                        case DataType.CustomPostgreSQL:
                        case DataType.KingbaseES:
                        case DataType.ShenTong:
                        case DataType.DuckDB:
                        case DataType.Firebird: //firebird 只支持单条插入 returning
                        case DataType.Xugu:
                            if (_tableIdentitys.Length == 1 && _tableReturnColumns.Length == 1)
                            {
                                var idtval = OrmInsert(table).AppendData(data).ExecuteIdentity();
                                _cascadeAffrows++;
                                data[_tableIdentitys[0].CsName] = Utils.GetDataReaderValue(_tableIdentitys[0].CsType, idtval);
                                _changeReport.Add(new ChangeReport.ChangeInfo { TableName = table.DbName, Object = data, Type = ChangeReport.ChangeType.Insert });
                            }
                            else
                            {
                                var newval = OrmInsert(table).AppendData(data).ExecuteInserted().First();
                                _cascadeAffrows++;
                                foreach (var col in table.Columns.Values)
                                    if (newval.TryGetValue(col.CsName, out var colval))
                                        data[col.CsName] = Utils.GetDataReaderValue(col.CsType, colval);
                                _changeReport.Add(new ChangeReport.ChangeInfo { TableName = table.DbName, Object = data, Type = ChangeReport.ChangeType.Insert });
                            }
                            return;
                        default:
                            if (_tableIdentitys.Length == 1)
                            {
                                var idtval = OrmInsert(table).AppendData(data).ExecuteIdentity();
                                _cascadeAffrows++;
                                data[_tableIdentitys[0].CsName] = Utils.GetDataReaderValue(_tableIdentitys[0].CsType, idtval);
                                _changeReport.Add(new ChangeReport.ChangeInfo { TableName = table.DbName, Object = data, Type = ChangeReport.ChangeType.Insert });
                                return;
                            }
                            break;
                    }
                }
                OrmInsert(table).AppendData(data).ExecuteAffrows();
                _cascadeAffrows++;
                _changeReport.Add(new ChangeReport.ChangeInfo { TableName = table.DbName, Object = data, Type = ChangeReport.ChangeType.Insert });
            }
            void LocalAddRange(TableInfo table, IEnumerable<T> data)
            {
                if (data == null) throw new ArgumentNullException(nameof(data));
                if (data is List<T> == false) data = data?.ToList();
                if (data.Any() == false) return;
                foreach (var s in data) if (LocalCanAdd(table, s, true) == false) return;

                var _tableReturnColumns = table.ColumnsByPosition.Where(a => a.Attribute.IsIdentity || string.IsNullOrWhiteSpace(a.DbInsertValue) == false).ToArray();
                if (_tableReturnColumns.Length > 0)
                {
                    var _tableIdentitys = table.ColumnsByPosition.Where(a => a.Attribute.IsIdentity).ToArray();
                    switch (_orm.Ado.DataType)
                    {
                        case DataType.SqlServer:
                        case DataType.OdbcSqlServer:
                        case DataType.CustomSqlServer:
                        case DataType.PostgreSQL:
                        case DataType.OdbcPostgreSQL:
                        case DataType.CustomPostgreSQL:
                        case DataType.KingbaseES:
                        case DataType.ShenTong:
                        case DataType.DuckDB:
                            var rets = OrmInsert(table).AppendData(data).ExecuteInserted();
                            _cascadeAffrows += rets.Count;
                            if (rets.Count != data.Count()) throw new Exception($"特别错误：批量添加失败，{_orm.Ado.DataType} 的返回数据，与添加的数目不匹配");
                            var idx = 0;
                            foreach (var s in data)
                            {
                                foreach (var col in table.Columns.Values)
                                    if (rets[idx].TryGetValue(col.CsName, out var colval))
                                        s[col.CsName] = Utils.GetDataReaderValue(col.CsType, colval);
                                idx++;
                            }
                            _changeReport.AddRange(data.Select(a => new ChangeReport.ChangeInfo { TableName = table.DbName, Object = a, Type = ChangeReport.ChangeType.Insert }));
                            return;
                        default:
                            if (_tableIdentitys.Length == 1)
                            {
                                foreach (var s in data)
                                    LocalAdd(table, s, false, _tableReturnColumns, _tableIdentitys);
                                return;
                            }
                            break;
                    }
                }
                _cascadeAffrows += OrmInsert(table).AppendData(data).ExecuteAffrows();
                _changeReport.AddRange(data.Select(a => new ChangeReport.ChangeInfo { TableName = table.DbName, Object = a, Type = ChangeReport.ChangeType.Insert }));
            }
            bool LocalCanAdd(TableInfo table, T data, bool isThrow)
            {
                if (data == null)
                {
                    if (isThrow) throw new ArgumentNullException(nameof(data));
                    return false;
                }
                if (table.Primarys.Any() == false)
                {
                    if (isThrow) throw new Exception($"不可添加，实体没有主键：{GetEntityString(table, data)}");
                    return false;
                }
                InsertProvider<T>.AuditDataValue(this, data, _orm, table, null);
                var key = GetEntityKeyString(table, data, true);
                if (string.IsNullOrEmpty(key))
                {
                    switch (_orm.Ado.DataType)
                    {
                        case DataType.SqlServer:
                        case DataType.OdbcSqlServer:
                        case DataType.CustomSqlServer:
                        case DataType.PostgreSQL:
                        case DataType.OdbcPostgreSQL:
                        case DataType.CustomPostgreSQL:
                        case DataType.KingbaseES:
                        case DataType.ShenTong:
                        case DataType.DuckDB:
                        case DataType.ClickHouse:
                            return true;
                        default:
                            if (table.Primarys.Where(a => a.Attribute.IsIdentity).Count() == 1 && table.Primarys.Length == 1)
                                return true;
                            if (isThrow) throw new Exception($"不可添加，未设置主键的值：{GetEntityString(table, data)}");
                            return false;
                    }
                }
                else
                {
                    //if (_states.ContainsKey(key))
                    //{
                    //	if (isThrow) throw new Exception($"不可添加，已存在于状态管理：{GetEntityString(table, data)}");
                    //	return false;
                    //}
                    if (_orm.Ado.DataType == DataType.ClickHouse) return true;
                    var idcol = table.Primarys.FirstOrDefault(a => a.Attribute.IsIdentity);
                    if (table.Primarys.Any(a => a.Attribute.IsIdentity))
                    {
                        if (idcol != null && data.TryGetValue(idcol.CsName, out var idval) && (long)idval > 0)
                        {
                            if (isThrow) throw new Exception($"不可添加，自增属性有值：{GetEntityString(table, data)}");
                            return false;
                        }
                    }
                }
                return true;
            }
            #endregion
        }

        void SaveOutsideCascade(IEnumerable<T> entities, IEnumerable<KeyValuePair<string, ZeroTableRef>> navs)
        {
            foreach (var nav in navs)
            {
                List<NativeTuple<T, T>> outsideList = null;
                switch (nav.Value.RefType)
                {
                    case TableRefType.ManyToOne:
                        outsideList = new List<NativeTuple<T, T>>();
                        foreach (var entity in entities)
                        {
                            if (entity.TryGetValue(nav.Key, out var outsideItemObj) == false ||
                                outsideItemObj is T outsideItem == false ||
                                outsideItem == null) continue;
                            outsideList.Add(NativeTuple.Create(entity, outsideItem));
                        }
                        break;
                    case TableRefType.ManyToMany:
                        outsideList = new List<NativeTuple<T, T>>();
                        foreach (var entity in entities)
                        {
                            if (entity.TryGetValue(nav.Key, out var mtmEachObj) == false ||
                                mtmEachObj is IEnumerable mtmEach == false ||
                                mtmEach == null) continue;
                            foreach (var mtmItemObj in mtmEach)
                            {
                                var mtmItem = mtmItemObj as T;
                                if (mtmItem == null) continue;
                                outsideList.Add(NativeTuple.Create(entity, mtmItem));
                            }
                        }
                        break;
                    default:
                        continue;
                }
                SaveOutsideCascade(nav.Value.RefTable, nav.Value, outsideList);
            }
        }
        void SaveOutsideCascade(ZeroTableInfo entityTable, ZeroTableRef nav, IEnumerable<NativeTuple<T, T>> outsideData)
        {
            outsideData = outsideData.Where(a => CanCascade(entityTable, a.Item2, true)).ToList();
            if (outsideData.Any() == false) return;
            var exists = outsideData.Select(a => new { outside = a, exists = ExistsInStates(entityTable, a.Item2) }).ToArray();
            var existsFalse = exists.Where(a => a.exists == false).Select(a => a.outside).ToArray();
            if (existsFalse.Any())
            {
                var query = new SelectImpl(this, entityTable.CsName).IncludeAll();
                var existsFalseResult = query.Where(existsFalse.Select(a => a.Item2)).ToList();
                foreach (var item in existsFalseResult) AttachCascade(entityTable, item, true);
            }
            var outsideListInsert = exists.Where(a => a.exists == null).Select(a => a.outside).Concat(
                existsFalse.Where(a => ExistsInStates(entityTable, a.Item2) == false)
                ).ToArray();
            if (outsideListInsert.Any())
            {
                var list = outsideListInsert.Select(a => a.Item2).ToArray();
                InsertCascade(entityTable, list, true); //插入
                if (nav != null && nav.RefType == TableRefType.ManyToOne)
                    foreach (var outside in outsideListInsert)
                        SetNavigateRelationshipValue(nav, outside.Item1, outside.Item2);
            }
            var outsideListUpdate = exists.Where(a => a.exists == true).Select(a => a.outside.Item2).Concat(
                existsFalse.Where(a => ExistsInStates(entityTable, a.Item2) == true).Select(a => a.Item2)
                ).ToArray();
            if (outsideListUpdate.Any())
            {
                UpdateCascade(entityTable, outsideListUpdate, true); //更新
            }
        }

        void UpdateCascade(ZeroTableInfo entityTable, IEnumerable<T> entities, bool cascade)
        {
            SaveOutsideCascade(entities, entityTable.Navigates.OrderBy(a => a.Value.RefType).ThenBy(a => a.Key));
            var tracking = new TrackingChangeInfo();
            foreach (var entity in entities)
            {
                var stateKey = GetEntityKeyString(entityTable, entity);
                if (_states.TryGetValue(entityTable.DbName, out var kv) == false || kv.TryGetValue(stateKey, out var state) == false) throw new Exception($"{nameof(ZeroDbContext)} 查询之后，才可以更新数据 {GetEntityString(entityTable, entity)}");
                CompareEntityValue(entityTable, state.Value, entity, tracking);
            }
            SaveTrackingChange(tracking);
            foreach (var entity in entities) AttachCascade(entityTable, entity, false);
        }
        void DeleteCascade(ZeroTableInfo entityTable, IEnumerable<T> entities, List<object> deletedOutput)
        {
            var tracking = new TrackingChangeInfo();
            foreach (var entity in entities)
            {
                var stateKey = GetEntityKeyString(entityTable, entity);
                if (stateKey == null) continue;
                CompareEntityValue(entityTable, entity, null, tracking);
                _states.Remove(stateKey);
            }
            tracking.InsertLog.Clear();
            tracking.UpdateLog.Clear();
            SaveTrackingChange(tracking);
        }
        void SaveTrackingChange(TrackingChangeInfo tracking)
        {
            var insertLogDict = tracking.InsertLog.GroupBy(a => a.Item1).ToDictionary(a => a.Key, a => tracking.InsertLog.Where(b => b.Item1 == a.Key).Select(b => b.Item2).ToArray());
            foreach (var il in insertLogDict)
                InsertCascade(il.Key, il.Value, true);

            for (var a = tracking.DeleteLog.Count - 1; a >= 0; a--)
            {
                var del = _orm.Delete<object>().WithTransaction(_transaction).CommandTimeout(_commandTimeout).AsTable(tracking.DeleteLog[a].Item1.DbName);
                var where = (del as DeleteProvider)._commonUtils.WhereItems(tracking.DeleteLog[a].Item1.Primarys, "", tracking.DeleteLog[a].Item2, (del as DeleteProvider)._params);
                _cascadeAffrows += del.Where(where).ExecuteAffrows();
                _changeReport?.AddRange(tracking.DeleteLog[a].Item2.Select(x =>
                    new ChangeReport.ChangeInfo
                    {
                        Type = ChangeReport.ChangeType.Delete,
                        TableName = tracking.DeleteLog[a].Item1.DbName,
                        Object = x
                    }));
                if (_states.TryGetValue(tracking.DeleteLog[a].Item1.DbName, out var kv))
                    foreach (var entity in tracking.DeleteLog[a].Item2)
                        kv.Remove(GetEntityKeyString(tracking.DeleteLog[a].Item1, entity));
            }

            var updateLogDict = tracking.UpdateLog.GroupBy(a => a.Item1).ToDictionary(a => a.Key, a => tracking.UpdateLog.Where(b => b.Item1 == a.Key).Select(b => new
            {
                BeforeObject = b.Item2,
                AfterObject = b.Item3,
                UpdateColumns = b.Item4,
                UpdateColumnsString = string.Join(",", b.Item4.OrderBy(c => c))
            }).ToArray());
            var updateLogDict2 = updateLogDict.ToDictionary(a => a.Key, a =>
                 a.Value.GroupBy(b => b.UpdateColumnsString).ToDictionary(b => b.Key, b => a.Value.Where(c => c.UpdateColumnsString == b.Key).ToArray()));
            foreach (var dl in updateLogDict2)
            {
                foreach (var dl2 in dl.Value)
                {
                    var upd = _orm.Update<T>().WithTransaction(_transaction).CommandTimeout(_commandTimeout);
                    var updProvider = (upd as UpdateProvider);
                    updProvider._table = dl.Key;
                    updProvider._tempPrimarys = dl.Key?.Primarys ?? new ColumnInfo[0];
                    updProvider._versionColumn = dl.Key?.VersionColumn;
                    _cascadeAffrows += upd
                        .SetSource(dl2.Value.Select(a => a.AfterObject).ToArray())
                        .UpdateColumns(dl2.Value.First().UpdateColumns.ToArray())
                        .ExecuteAffrows();
                    _changeReport?.AddRange(dl2.Value.Select(x =>
                        new ChangeReport.ChangeInfo
                        {
                            Type = ChangeReport.ChangeType.Update,
                            TableName = dl.Key.DbName,
                            Object = x.AfterObject,
                            BeforeObject = x.BeforeObject
                        }));
                }
            }
        }

        #region EntityState
        internal void AttachCascade(ZeroTableInfo entityTable, T entity, bool includeOutside)
        {
            var ignores = new Dictionary<string, Dictionary<string, bool>>(); //比如 Tree 结构可以递归添加
            LocalAttachCascade(entityTable, entity, true);
            ignores.Clear();

            void LocalAttachCascade(ZeroTableInfo table, T entityFrom, bool flag)
            {
                if (flag == false) return;
                if (entityFrom == null) return;

                var key = GetEntityKeyString(table, entityFrom);
                if (key == null) return;
                var state = new EntityState(new T(), key);

                LocalMapEntityValue(table, entityFrom, state.Value);

                if (_states.TryGetValue(table.DbName, out var kv) == false) _states.Add(table.DbName, kv = new Dictionary<string, EntityState>());
                if (kv.ContainsKey(state.Key)) kv[state.Key] = state;
                else kv.Add(state.Key, state);
            }
            bool LocalMapEntityValue(ZeroTableInfo table, T entityFrom, T entityTo)
            {
                if (entityFrom == null || entityTo == null) return true;

                var stateKey = GetEntityKeyString(table, entityFrom);
                if (stateKey == null) return false;
                if (ignores.TryGetValue(table.DbName, out var stateKeys) == false) ignores.Add(table.DbName, stateKeys = new Dictionary<string, bool>());
                if (stateKeys.ContainsKey(stateKey)) return false;
                stateKeys.Add(stateKey, true);

                foreach (var col in table.Columns.Values)
                    if (entityFrom.TryGetValue(col.CsName, out var colval))
                        entityTo[col.CsName] = colval;

                foreach (var nav in table.Navigates)
                {
                    if (entityFrom.TryGetValue(nav.Key, out var propvalFrom) == false || propvalFrom == null)
                    {
                        entityTo.Remove(nav.Key);
                        continue;
                    }
                    switch (nav.Value.RefType)
                    {
                        case TableRefType.OneToOne:
                            {
                                var valFrom = propvalFrom as T;
                                if (valFrom == null)
                                {
                                    //entityTo.Remove(nav.Key);
                                    continue;
                                }
                                var valTo = new T();
                                SetNavigateRelationshipValue(nav.Value, entityFrom, valFrom);
                                if (LocalMapEntityValue(nav.Value.RefTable, valFrom, valTo))
                                    entityTo[nav.Key] = valTo;
                            }
                            break;
                        case TableRefType.OneToMany:
                            {
                                var valFromList = propvalFrom as IEnumerable;
                                if (valFromList == null)
                                {
                                    //entityTo.Remove(nav.Key);
                                    continue;
                                }
                                SetNavigateRelationshipValue(nav.Value, entityFrom, valFromList);
                                var valTo = new List<T>();
                                foreach (var fromObj in valFromList)
                                {
                                    if (fromObj is T fromItem == false || fromItem == null) continue;
                                    var toItem = new T();
                                    if (LocalMapEntityValue(nav.Value.RefTable, fromItem, toItem))
                                        valTo.Add(toItem);
                                }
                                entityTo[nav.Key] = valTo;
                            }
                            break;
                        case TableRefType.ManyToMany:
                            {
                                var valFromList = propvalFrom as IEnumerable;
                                if (valFromList == null)
                                {
                                    //entityTo.Remove(nav.Key);
                                    continue;
                                }
                                var valTo = new List<T>();
                                foreach (var fromObj in valFromList)
                                {
                                    if (fromObj is T fromItem == false || fromItem == null) continue;
                                    LocalAttachCascade(nav.Value.RefTable, fromItem, includeOutside); //创建新的 states
                                    var toItem = new T();
                                    foreach (var col in nav.Value.RefTable.Primarys) //多对多状态只存储 PK
                                        if (fromItem.TryGetValue(col.CsName, out var colval))
                                            toItem[col.CsName] = colval;
                                    valTo.Add(toItem);
                                }
                                entityTo[nav.Key] = valTo;
                            }
                            break;
                        case TableRefType.ManyToOne:
                            {
                                var valFrom = propvalFrom as T;
                                if (valFrom == null)
                                {
                                    //entityTo.Remove(nav.Key);
                                    continue;
                                }
                                LocalAttachCascade(nav.Value.RefTable, valFrom, includeOutside); //创建新的 states
                                var valTo = new T();
                                foreach (var col in nav.Value.RefTable.Columns.Values)
                                    if (valFrom.TryGetValue(col.CsName, out var colval))
                                        valTo[col.CsName] = colval;
                                entityTo[nav.Key] = valTo;
                            }
                            break;
                    }
                }
                return true;
            }
        }
        class EntityState
        {
            public EntityState(T value, string key)
            {
                this.Value = value;
                this.Key = key;
                this.Time = DateTime.Now;
            }
            public T OldValue { get; set; }
            public T Value { get; set; }
            public string Key { get; set; }
            public DateTime Time { get; set; }
        }
        Dictionary<string, Dictionary<string, EntityState>> _states = new Dictionary<string, Dictionary<string, EntityState>>();
        bool? ExistsInStates(ZeroTableInfo table, T data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var key = GetEntityKeyString(table, data);
            if (string.IsNullOrEmpty(key)) return null;
            return _states.TryGetValue(table.DbName, out var kv) && kv.ContainsKey(key);
        }
        string GetEntityKeyString(TableInfo table, T data, bool genGuid = false)
        {
            if (genGuid)
            {
                foreach (var col in table.Primarys)
                {
                    if (col.CsType == typeof(Guid))
                    {
                        if (data.TryGetValue(col.CsName, out var colval) == false || CompareEntityPropertyValue(col.CsType, colval, Guid.Empty))
                            data[col.CsName] = FreeUtil.NewMongodbId();
                    }
                    else if (col.CsType == typeof(Guid?))
                    {
                        if (data.TryGetValue(col.CsName, out var colval) == false || CompareEntityPropertyValue(col.CsType, colval, Guid.Empty) || CompareEntityPropertyValue(col.CsType, colval, null))
                            data[col.CsName] = FreeUtil.NewMongodbId();
                    }
                }
            }
            foreach (var col in table.Primarys)
            {
                if (col.Attribute.IsIdentity)
                {
                    if (data.TryGetValue(col.CsName, out var colval) == false || CompareEntityPropertyValue(col.CsType, colval, col.CsType.CreateInstanceGetDefaultValue()))
                        return null;
                }
                else if (col.CsType == typeof(Guid))
                {
                    if (data.TryGetValue(col.CsName, out var colval) == false || CompareEntityPropertyValue(col.CsType, colval, Guid.Empty))
                        return null;
                }
                else if (col.CsType == typeof(Guid?))
                {
                    if (data.TryGetValue(col.CsName, out var colval) == false || CompareEntityPropertyValue(col.CsType, colval, Guid.Empty) || CompareEntityPropertyValue(col.CsType, colval, null))
                        return null;
                }
            }
            return $"{table.DbName}:{string.Join("*|_,[,_|*", table.Primarys.Select(a => data.TryGetValue(a.Attribute.Name, out var val) ? val : null))}";
        }
        string GetEntityString(TableInfo table, T data) => $"({string.Join(", ", table.ColumnsByCs.Select(a => data.TryGetValue(a.Key, out var colval) ? colval : ""))})";
        #endregion

        class TrackingChangeInfo
        {
            public List<NativeTuple<ZeroTableInfo, T>> InsertLog { get; } = new List<NativeTuple<ZeroTableInfo, T>>();
            public List<NativeTuple<ZeroTableInfo, T, T, List<string>>> UpdateLog { get; } = new List<NativeTuple<ZeroTableInfo, T, T, List<string>>>();
            public List<NativeTuple<ZeroTableInfo, T[]>> DeleteLog { get; } = new List<NativeTuple<ZeroTableInfo, T[]>>();
        }
        void CompareEntityValue(ZeroTableInfo rootTable, T rootEntityBefore, T rootEntityAfter, TrackingChangeInfo tracking)
        {
            var rootIgnores = new Dictionary<string, Dictionary<string, bool>>(); //比如 Tree 结构可以递归添加
            LocalCompareEntityValue(rootTable, rootEntityBefore, rootEntityAfter, true);
            rootIgnores.Clear();

            void LocalCompareEntityValue(ZeroTableInfo table, T entityBefore, T entityAfter, bool cascade)
            {
                if (entityBefore != null)
                {
                    var stateKey = $":before://{GetEntityKeyString(table, entityBefore)}";
                    if (rootIgnores.TryGetValue(table.DbName, out var stateKeys) == false) rootIgnores.Add(table.DbName, stateKeys = new Dictionary<string, bool>());
                    if (stateKeys.ContainsKey(stateKey)) return;
                    stateKeys.Add(stateKey, true);
                }
                if (entityAfter != null)
                {
                    var stateKey = $":after://{GetEntityKeyString(table, entityAfter)}";
                    if (rootIgnores.TryGetValue(table.DbName, out var stateKeys) == false) rootIgnores.Add(table.DbName, stateKeys = new Dictionary<string, bool>());
                    if (stateKeys.ContainsKey(stateKey)) return;
                    stateKeys.Add(stateKey, true);
                }

                if (entityBefore == null && entityAfter == null) return;
                if (entityBefore == null && entityAfter != null)
                {
                    tracking.InsertLog.Add(NativeTuple.Create(table, entityAfter));
                    return;
                }
                if (entityBefore != null && entityAfter == null)
                {
                    tracking.DeleteLog.Add(NativeTuple.Create(table, new[] { entityBefore }));
                    NavigateReader(table, entityBefore, (path, nav, ctb, stackvs) =>
                    {
                        var dellist = (stackvs.Last() as object[] ?? new[] { stackvs.Last() }).Select(a => a as T).Where(a => a != null).ToArray();
                        tracking.DeleteLog.Add(NativeTuple.Create(ctb, dellist));
                    });
                    return;
                }
                var changes = new List<string>();
                foreach (var col in table.ColumnsByCs.Values)
                {
                    if (col.Attribute.IsVersion) continue;
                    entityBefore.TryGetValue(col.CsName, out var propvalBefore);
                    entityAfter.TryGetValue(col.CsName, out var propvalAfter);
                    //if (object.Equals(propvalBefore, propvalAfter) == false) changes.Add(col.CsName);
                    if (CompareEntityPropertyValue(col.CsType, propvalBefore, propvalAfter) == false) changes.Add(col.CsName);
                    continue;
                }
                if (changes.Any()) tracking.UpdateLog.Add(NativeTuple.Create(table, entityBefore, entityAfter, changes));
                if (cascade == false) return;

                foreach (var nav in table.Navigates.OrderBy(a => a.Value.RefType).ThenBy(a => a.Key))
                {
                    entityBefore.TryGetValue(nav.Key, out var propvalBefore);
                    entityAfter.TryGetValue(nav.Key, out var propvalAfter);
                    switch (nav.Value.RefType)
                    {
                        case TableRefType.OneToOne:
                            SetNavigateRelationshipValue(nav.Value, entityBefore, propvalBefore);
                            SetNavigateRelationshipValue(nav.Value, entityAfter, propvalAfter);
                            LocalCompareEntityValue(nav.Value.RefTable, propvalBefore as T, propvalAfter as T, true);
                            break;
                        case TableRefType.OneToMany:
                            SetNavigateRelationshipValue(nav.Value, entityBefore, propvalBefore);
                            SetNavigateRelationshipValue(nav.Value, entityAfter, propvalAfter);
                            LocalCompareEntityValueCollection(nav.Value.RefTable, propvalBefore as IEnumerable, propvalAfter as IEnumerable, true);
                            break;
                        case TableRefType.ManyToMany:
                            var middleValuesBefore = GetManyToManyObjects(nav.Value, entityBefore, nav.Key);
                            var middleValuesAfter = GetManyToManyObjects(nav.Value, entityAfter, nav.Key);
                            LocalCompareEntityValueCollection(nav.Value.RefMiddleTable, middleValuesBefore, middleValuesAfter, false);
                            break;
                    }
                }
            }
            void LocalCompareEntityValueCollection(ZeroTableInfo elementTable, IEnumerable collectionBefore, IEnumerable collectionAfter, bool cascade)
            {
                if (collectionBefore == null && collectionAfter == null) return;
                if (collectionBefore == null && collectionAfter != null)
                {
                    foreach (var itemObj in collectionAfter)
                    {
                        if (itemObj is T item == false || item == null) continue;
                        tracking.InsertLog.Add(NativeTuple.Create(elementTable, item));
                    }
                    return;
                }
                if (collectionBefore != null && collectionAfter == null)
                {
                    return;
                }
                Dictionary<string, T> dictBefore = new Dictionary<string, T>();
                Dictionary<string, T> dictAfter = new Dictionary<string, T>();
                foreach (var itemObj in collectionBefore)
                {
                    if (itemObj is T item == false || item == null) continue;
                    var key = GetEntityKeyString(elementTable, item);
                    if (key != null) dictBefore.Add(key, item);
                }
                foreach (var itemObj in collectionAfter)
                {
                    if (itemObj is T item == false || item == null) continue;
                    var key = GetEntityKeyString(elementTable, item);
                    if (key != null)
                    {
                        if (dictAfter.ContainsKey(key) == false)
                            dictAfter.Add(key, item);
                    }
                    else tracking.InsertLog.Add(NativeTuple.Create(elementTable, item));
                }
                foreach (var key in dictBefore.Keys.ToArray())
                {
                    if (dictAfter.ContainsKey(key) == false)
                    {
                        var value = dictBefore[key];
                        tracking.DeleteLog.Add(NativeTuple.Create(elementTable, new[] { value }));
                        NavigateReader(elementTable, value, (path, nav, ctb, stackvs) =>
                        {
                            var dellist = (stackvs.Last() as object[] ?? new[] { stackvs.Last() }).Select(a => a as T).Where(a => a != null).ToArray();
                            tracking.DeleteLog.Add(NativeTuple.Create(ctb, dellist));
                        });
                        dictBefore.Remove(key);
                    }
                }
                foreach (var key in dictAfter.Keys.ToArray())
                {
                    if (dictBefore.ContainsKey(key) == false)
                    {
                        tracking.InsertLog.Add(NativeTuple.Create(elementTable, dictAfter[key]));
                        dictAfter.Remove(key);
                    }
                }
                foreach (var key in dictBefore.Keys)
                    LocalCompareEntityValue(elementTable, dictBefore[key], dictAfter.TryGetValue(key, out var afterItem) ? afterItem : null, cascade);
            }
            void NavigateReader(ZeroTableInfo readerTable, T readerEntity, Action<string, ZeroTableRef, ZeroTableInfo, List<object>> callback)
            {
                var ignores = new Dictionary<string, Dictionary<string, bool>>(); //比如 Tree 结构可以递归添加
                var statckPath = new Stack<string>();
                var stackValues = new List<object>();
                statckPath.Push("_");
                stackValues.Add(readerEntity);
                LocalNavigateReader(readerTable, readerEntity);
                ignores.Clear();

                void LocalNavigateReader(ZeroTableInfo table, T entity)
                {
                    if (entity == null) return;
                    var stateKey = GetEntityKeyString(table, entity);
                    if (stateKey == null) return;
                    if (ignores.TryGetValue(table.DbName, out var stateKeys) == false) ignores.Add(table.DbName, stateKeys = new Dictionary<string, bool>());
                    if (stateKeys.ContainsKey(stateKey)) return;
                    stateKeys.Add(stateKey, true);

                    foreach (var nav in table.Navigates.OrderBy(a => a.Value.RefType).ThenBy(a => a.Key))
                    {
                        switch (nav.Value.RefType)
                        {
                            case TableRefType.OneToOne:
                                if (entity.TryGetValue(nav.Key, out var propval) == false || propval == null) continue;
                                statckPath.Push(nav.Key);
                                stackValues.Add(propval);
                                SetNavigateRelationshipValue(nav.Value, entity, propval);
                                callback?.Invoke(string.Join(".", statckPath), nav.Value, nav.Value.RefTable, stackValues);
                                LocalNavigateReader(nav.Value.RefTable, propval as T);
                                stackValues.RemoveAt(stackValues.Count - 1);
                                statckPath.Pop();
                                break;
                            case TableRefType.OneToMany:
                                if (entity.TryGetValue(nav.Key, out var propval2) == false || propval2 is IEnumerable propvalOtm == false || propvalOtm == null) continue;
                                SetNavigateRelationshipValue(nav.Value, entity, propvalOtm);
                                var propvalOtmList = new List<object>();
                                foreach (var val in propvalOtm)
                                    propvalOtmList.Add(val);
                                statckPath.Push($"{nav.Key}[]");
                                stackValues.Add(propvalOtmList.ToArray());
                                callback?.Invoke(string.Join(".", statckPath), nav.Value, nav.Value.RefTable, stackValues);
                                foreach (var val in propvalOtm)
                                    LocalNavigateReader(nav.Value.RefTable, val as T);
                                stackValues.RemoveAt(stackValues.Count - 1);
                                statckPath.Pop();
                                break;
                            case TableRefType.ManyToMany:
                                var middleValues = GetManyToManyObjects(nav.Value, entity, nav.Key)?.ToArray();
                                if (middleValues == null) continue;
                                statckPath.Push($"{nav.Key}[]");
                                stackValues.Add(middleValues);
                                callback?.Invoke(string.Join(".", statckPath), nav.Value, nav.Value.RefMiddleTable, stackValues);
                                stackValues.RemoveAt(stackValues.Count - 1);
                                statckPath.Pop();
                                break;
                        }
                    }
                }
            }
        }

        static ConcurrentDictionary<Type, bool> _dicCompareEntityPropertyValue = new ConcurrentDictionary<Type, bool>
        {
            [typeof(string)] = true,
            [typeof(DateTime)] = true,
            [typeof(DateTime?)] = true,
            [typeof(DateTimeOffset)] = true,
            [typeof(DateTimeOffset?)] = true,
            [typeof(TimeSpan)] = true,
            [typeof(TimeSpan?)] = true,
        };
        internal static bool CompareEntityPropertyValue(Type type, object propvalBefore, object propvalAfter)
        {
            if (propvalBefore == null && propvalAfter == null) return true;
            if (type.IsNumberType() ||
                _dicCompareEntityPropertyValue.ContainsKey(type) ||
                type.IsEnum ||
                type.IsValueType ||
                type.NullableTypeOrThis().IsEnum) return object.Equals(propvalBefore, propvalAfter);
            if (propvalBefore == null && propvalAfter != null) return false;
            if (propvalBefore != null && propvalAfter == null) return false;

            if (Utils.dicExecuteArrayRowReadClassOrTuple.ContainsKey(type))
            {
                if (type.FullName.StartsWith("Newtonsoft."))
                    return object.Equals(propvalBefore.ToString(), propvalAfter.ToString());

                if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    var dictBefore = (propvalBefore as IDictionary);
                    var dictAfter = (propvalAfter as IDictionary);
                    if (dictBefore.Count != dictAfter.Count) return false;
                    foreach (var key in dictBefore.Keys)
                    {
                        if (dictAfter.Contains(key) == false) return false;
                        var valBefore = dictBefore[key];
                        var valAfter = dictAfter[key];
                        if (valBefore == null && valAfter == null) continue;
                        if (valBefore == null && valAfter != null) return false;
                        if (valBefore != null && valAfter == null) return false;
                        if (CompareEntityPropertyValue(valBefore.GetType(), valBefore, valAfter) == false) return false;
                    }
                    return true;
                }

                if (type.IsArrayOrList())
                {
                    var enumableBefore = propvalBefore as IEnumerable;
                    var enumableAfter = propvalAfter as IEnumerable;
                    var itorBefore = enumableBefore.GetEnumerator();
                    var itorAfter = enumableAfter.GetEnumerator();
                    while (true)
                    {
                        var moveNextBefore = itorBefore.MoveNext();
                        var moveNextAfter = itorAfter.MoveNext();
                        if (moveNextBefore != moveNextAfter) return false;
                        if (moveNextBefore == false) break;
                        var currentBefore = itorBefore.Current;
                        var currentAfter = itorAfter.Current;
                        if (currentBefore == null && enumableAfter == null) continue;
                        if (currentBefore == null && currentAfter != null) return false;
                        if (currentBefore != null && currentAfter == null) return false;
                        if (CompareEntityPropertyValue(currentBefore.GetType(), currentBefore, currentAfter) == false) return false;
                    }
                    return true;
                }

                if (type.FullName.StartsWith("System.") ||
                    type.FullName.StartsWith("Npgsql.") ||
                    type.FullName.StartsWith("NetTopologySuite."))
                    return object.Equals(propvalBefore, propvalAfter);

                if (type.IsClass)
                {
                    foreach (var prop in type.GetProperties())
                    {
                        var valBefore = prop.GetValue(propvalBefore, new object[0]);
                        var valAfter = prop.GetValue(propvalAfter, new object[0]);
                        if (CompareEntityPropertyValue(prop.PropertyType, valBefore, valAfter) == false) return false;
                    }
                    return true;
                }
            }
            return object.Equals(propvalBefore, propvalAfter);
        }

        List<T> GetManyToManyObjects(ZeroTableRef nav, T entity, string navName)
        {
            if (nav.RefType != TableRefType.ManyToMany) return null;
            if (entity.TryGetValue(navName, out var rightsObj) == false || rightsObj is IEnumerable rights == false || rights == null) return null;
            var middles = new List<T>();
            foreach (var ritem in rights)
            {
                if (ritem is T right == false || right == null) continue;
                var midval = new T();
                for (var x = 0; x < nav.Columns.Count; x++)
                    if (entity.TryGetValue(nav.Columns[x], out var colval))
                        midval[nav.MiddleColumns[x]] = colval;

                for (var x = nav.Columns.Count; x < nav.MiddleColumns.Count; x++)
                {
                    var refcol = nav.RefColumns[x - nav.Columns.Count];
                    if (right.TryGetValue(refcol, out var refval) == false ||
                        refval == nav.RefTable.ColumnsByCs[refcol].CsType.CreateInstanceGetDefaultValue()) throw new Exception($"ManyToMany 关联对象的主键属性({nav.RefTable.CsName}.{refcol})不能为空");
                    midval[nav.MiddleColumns[x]] = refval;
                }
                middles.Add(midval);
            }
            return middles;
        }
        void SetNavigateRelationshipValue(ZeroTableRef nav, T leftItem, object rightItem)
        {
            switch (nav.RefType)
            {
                case TableRefType.OneToOne:
                    {
                        if (rightItem == null || rightItem is T rightItemDict == false || rightItemDict == null) return;
                        for (var idx = 0; idx < nav.Columns.Count; idx++)
                        {
                            if (leftItem.TryGetValue(nav.Columns[idx], out var colval))
                                rightItemDict[nav.RefColumns[idx]] = colval;
                        }
                    }
                    break;
                case TableRefType.OneToMany:
                    {
                        if (rightItem == null || rightItem is IEnumerable rightEachOtm == false || rightEachOtm == null) return;
                        foreach (var rightEle in rightEachOtm)
                        {
                            if (rightEle is T rightItemDict == false || rightItemDict == null) continue;
                            for (var idx = 0; idx < nav.Columns.Count; idx++)
                                if (leftItem.TryGetValue(nav.Columns[idx], out var colval))
                                    rightItemDict[nav.RefColumns[idx]] = colval;
                        }
                    }
                    break;
                case TableRefType.ManyToOne:
                    for (var idx = 0; idx < nav.RefColumns.Count; idx++)
                    {
                        if (rightItem is T rightItemDict == false || rightItemDict == null)
                            leftItem[nav.Columns[idx]] = nav.Table.ColumnsByCs[nav.Columns[idx]].CsType.CreateInstanceGetDefaultValue();
                        else if (rightItemDict.TryGetValue(nav.RefColumns[idx], out var colval))
                            leftItem[nav.Columns[idx]] = colval;
                    }
                    break;
            }
        }
    }
}
