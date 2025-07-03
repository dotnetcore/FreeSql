using FreeSql.DataAnnotations;
using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{

    public abstract partial class CodeFirstProvider : ICodeFirst
    {
        public IFreeSql _orm;
        public CommonUtils _commonUtils;
        public CommonExpression _commonExpression;
        public CodeFirstProvider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public virtual bool IsAutoSyncStructure { get; set; } = false;
        public bool IsSyncStructureToLower { get; set; } = false;
        public bool IsSyncStructureToUpper { get; set; } = false;
        public bool IsConfigEntityFromDbFirst { get; set; } = false;
        public virtual bool IsNoneCommandParameter { get; set; } = false;
        public virtual bool IsGenerateCommandParameterWithLambda { get; set; } = false;
        public bool IsLazyLoading { get; set; } = false;

        public abstract DbInfoResult GetDbInfo(Type type);

        public ICodeFirst ConfigEntity<T>(Action<TableFluent<T>> entity) => _commonUtils.ConfigEntity(entity);
        public ICodeFirst ConfigEntity(Type type, Action<TableFluent> entity) => _commonUtils.ConfigEntity(type, entity);
        public TableAttribute GetConfigEntity(Type type) => _commonUtils.GetConfigEntity(type);
        public TableInfo GetTableByEntity(Type type) => _commonUtils.GetTableByEntity(type);

        protected string GetTableNameLowerOrUpper(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) return "";
            if (IsSyncStructureToLower) tableName = tableName.ToLower();
            if (IsSyncStructureToUpper) tableName = tableName.ToUpper();
            return tableName;
        }
        public string GetComparisonDDLStatements<TEntity>() =>
            this.GetComparisonDDLStatements(new TypeSchemaAndName(GetTableByEntity(typeof(TEntity)), ""));
        public string GetComparisonDDLStatements(params Type[] entityTypes) => entityTypes == null ? null : 
            this.GetComparisonDDLStatements(entityTypes.Distinct().Select(a => new TypeSchemaAndName(GetTableByEntity(a), "")).ToArray());
        public string GetComparisonDDLStatements(Type entityType, string tableName) =>
           this.GetComparisonDDLStatements(new TypeSchemaAndName(GetTableByEntity(entityType), GetTableNameLowerOrUpper(tableName)));
		public string GetComparisonDDLStatements(TableInfo tableSchema, string tableName) =>
		   this.GetComparisonDDLStatements(new TypeSchemaAndName(tableSchema, GetTableNameLowerOrUpper(tableName)));
		protected abstract string GetComparisonDDLStatements(params TypeSchemaAndName[] objects);
        public class TypeSchemaAndName
        {
            public TableInfo tableSchema { get; }
            public string tableName { get; }
            public TypeSchemaAndName(TableInfo tableSchema, string tableName)
            {
                this.tableSchema = tableSchema;
                this.tableName = tableName;
            }
        }

        static object syncStructureLock = new object();
        object _dicSycedLock = new object();
        public ConcurrentDictionary<Type, ConcurrentDictionary<string, bool>> _dicSynced = new ConcurrentDictionary<Type, ConcurrentDictionary<string, bool>>();
        internal ConcurrentDictionary<string, bool> _dicSycedGetOrAdd(Type entityType)
        {
            if (_dicSynced.TryGetValue(entityType, out var trydic) == false)
                lock (_dicSycedLock)
                    if (_dicSynced.TryGetValue(entityType, out trydic) == false)
                        _dicSynced.TryAdd(entityType, trydic = new ConcurrentDictionary<string, bool>());
            return trydic;
        }
        public void _dicSycedTryAdd(Type entityType, string tableName = null) =>
            _dicSycedGetOrAdd(entityType).TryAdd(GetTableNameLowerOrUpper(tableName), true);

        public void SyncStructure<TEntity>() =>
            this.SyncStructure(new TypeSchemaAndName(GetTableByEntity(typeof(TEntity)), ""));
        public void SyncStructure(params Type[] entityTypes) => 
            this.SyncStructure(entityTypes?.Distinct().Select(a => new TypeSchemaAndName(GetTableByEntity(a), "")).ToArray());
        public void SyncStructure(Type entityType, string tableName, bool isForceSync) =>
            this.SyncStructure(GetTableByEntity(entityType), tableName, isForceSync);
		public void SyncStructure(TableInfo tableSchema, string tableName, bool isForceSync = false)
        {
            tableName = GetTableNameLowerOrUpper(tableName);
			if (isForceSync && tableSchema?.Type != null && _dicSynced.TryGetValue(tableSchema.Type, out var dic)) dic.TryRemove(tableName, out var old);
			this.SyncStructure(new TypeSchemaAndName(tableSchema, tableName));
		}

        protected void SyncStructure(params TypeSchemaAndName[] objects)
        {
            if (objects == null) return;
            var syncObjects = objects.Where(a => a.tableSchema?.Type != null &&
                    (
                        a.tableSchema.Type == typeof(object) && a.tableSchema.Columns.Any() 
                        || 
                        a.tableSchema.Type != typeof(object) && _dicSycedGetOrAdd(a.tableSchema.Type).ContainsKey(GetTableNameLowerOrUpper(a.tableName)) == false
                    ) && 
                    a.tableSchema?.DisableSyncStructure == false)
                .Select(a => new TypeSchemaAndName(a.tableSchema, GetTableNameLowerOrUpper(a.tableName)))
                .Where(a => !(string.IsNullOrEmpty(a.tableName) == true && a.tableSchema?.AsTableImpl != null))
                .ToArray();
            if (syncObjects.Any() == false) return;
            var before = new Aop.SyncStructureBeforeEventArgs(syncObjects.Select(a => a.tableSchema.Type).ToArray());
            _orm.Aop.SyncStructureBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            string ddl = null;
            try
            {
                lock (syncStructureLock)
                {
                    ddl = this.GetComparisonDDLStatements(syncObjects);
                    if (string.IsNullOrEmpty(ddl))
                    {
                        foreach (var syncObject in syncObjects) _dicSycedTryAdd(syncObject.tableSchema.Type, syncObject.tableName);
                        return;
                    }
                    var affrows = ExecuteDDLStatements(ddl);
                    foreach (var syncObject in syncObjects) _dicSycedTryAdd(syncObject.tableSchema.Type, syncObject.tableName);
                    return;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.SyncStructureAfterEventArgs(before, ddl, exception);
                _orm.Aop.SyncStructureAfterHandler?.Invoke(this, after);
            }
        }

        public virtual int ExecuteDDLStatements(string ddl) => string.IsNullOrWhiteSpace(ddl) ? 0 : _orm.Ado.ExecuteNonQuery(CommandType.Text, ddl);

        public static string ReplaceIndexName(string indexName, string tbname) => string.IsNullOrEmpty(indexName) ? indexName : Regex.Replace(indexName, @"\{\s*TableName\s*\}", tbname, RegexOptions.IgnoreCase);
    }
}