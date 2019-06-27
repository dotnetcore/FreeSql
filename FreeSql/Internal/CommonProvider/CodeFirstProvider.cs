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
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{

    public abstract partial class CodeFirstProvider : ICodeFirst
    {

        protected IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        public CodeFirstProvider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public bool IsAutoSyncStructure { get; set; } = false;
        public bool IsSyncStructureToLower { get; set; } = false;
        public bool IsSyncStructureToUpper { get; set; } = false;
        public bool IsConfigEntityFromDbFirst { get; set; } = false;
        public bool IsNoneCommandParameter { get; set; } = false;
        public bool IsLazyLoading { get; set; } = false;

        public abstract (int type, string dbtype, string dbtypeFull, bool? isnullable, object defaultValue)? GetDbInfo(Type type);

        public ICodeFirst ConfigEntity<T>(Action<TableFluent<T>> entity) => _commonUtils.ConfigEntity(entity);
        public ICodeFirst ConfigEntity(Type type, Action<TableFluent> entity) => _commonUtils.ConfigEntity(type, entity);
        public TableAttribute GetConfigEntity(Type type) => _commonUtils.GetConfigEntity(type);
        public TableInfo GetTableByEntity(Type type) => _commonUtils.GetTableByEntity(type);

        public string GetComparisonDDLStatements<TEntity>() => this.GetComparisonDDLStatements(typeof(TEntity));
        public abstract string GetComparisonDDLStatements(params Type[] entityTypes);

        static object syncStructureLock = new object();
        internal ConcurrentDictionary<string, bool> dicSyced = new ConcurrentDictionary<string, bool>();
        public bool SyncStructure<TEntity>() => this.SyncStructure(typeof(TEntity));
        public bool SyncStructure(params Type[] entityTypes)
        {
            if (entityTypes == null) return false;
            var syncTypes = entityTypes.Where(a => dicSyced.ContainsKey(a.FullName) == false && GetTableByEntity(a)?.DisableSyncStructure == false).ToArray();
            if (syncTypes.Any() == false) return false;
            var before = new Aop.SyncStructureBeforeEventArgs(entityTypes);
            _orm.Aop.SyncStructureBefore?.Invoke(this, before);
            Exception exception = null;
            string ddl = null;
            try
            {
                lock (syncStructureLock)
                {
                    ddl = this.GetComparisonDDLStatements(syncTypes);
                    if (string.IsNullOrEmpty(ddl))
                    {
                        foreach (var syncType in syncTypes) dicSyced.TryAdd(syncType.FullName, true);
                        return true;
                    }
                    var affrows = _orm.Ado.ExecuteNonQuery(CommandType.Text, ddl);
                    foreach (var syncType in syncTypes) dicSyced.TryAdd(syncType.FullName, true);
                    return affrows > 0;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.SyncStructureAfterEventArgs(before, ddl, exception);
                _orm.Aop.SyncStructureAfter?.Invoke(this, after);
            }
        }
    }
}