using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace FreeSql.DbContext
{

    public class FreeSqlBuilderConfiguration
    {
        public DataType DataType { get; set; }
        public string ConnectionString { get; set; }
        public List<string> SlaveList { get; } = new List<string>();
        public Action<DbCommand> Executing { get; set; }
        public Action<DbCommand, string> Executed { get; set; }
        private ILogger Logger { get; set; }
        private IDistributedCache DistributeCache { get; set; }
        public bool IsAutoSyncStructure { get; set; }
        public bool IsSyncStructureToLower { get; set; }
        public bool IsLazyLoading { get; set; }

        public FreeSqlBuilderConfiguration UseConnectionString(DataType dataType, string connectionString)
        {
            DataType = dataType;
            ConnectionString = connectionString;

            return this;
        }

        public FreeSqlBuilderConfiguration UserSlave(string slave)
        {
            SlaveList.Add(slave);

            return this;
        }

        public FreeSqlBuilderConfiguration UseMonitorCommand(Action<DbCommand> executing, Action<DbCommand, string> executed = null)
        {
            Executing = executing;
            Executed = executed;

            return this;
        }

        public FreeSqlBuilderConfiguration UseLogger(ILogger logger)
        {
            Logger = logger;

            return this;
        }

        public FreeSqlBuilderConfiguration UseCache(IDistributedCache distributeCache)
        {
            DistributeCache = distributeCache;

            return this;
        }

       public FreeSqlBuilderConfiguration UseAutoSyncStructure(bool isAutoSyncStructure)
       {
            IsAutoSyncStructure = isAutoSyncStructure;

            return this;
       }
       public FreeSqlBuilderConfiguration UseSyncStructureToLower(bool isSyncStructureToLower)
       {
            IsSyncStructureToLower = isSyncStructureToLower;

            return this;
       }

       public FreeSqlBuilderConfiguration UseLazyLoading(bool isLazyLoading)
       {
            IsLazyLoading = isLazyLoading;

            return this;
       }
    }

    public class FreeSqlDbContextFactory
    {
        private ConcurrentDictionary<Type, FreeSqlBuilderConfiguration> Dictionary = new ConcurrentDictionary<Type, FreeSqlBuilderConfiguration>();
        private CodeFirstFactory CodeFirstFactory = new CodeFirstFactory();
        private FreeSqlBuilder builder = new FreeSqlBuilder();

        public TDbContext GetOrAdd<TDbContext>(FreeSqlBuilderConfiguration contextConfiguration) where TDbContext : FreeSqlDbContext
        {
            var type = typeof(TDbContext);
            var config = Dictionary.GetOrAdd(type, contextConfiguration);
            var context = builder
                .UseConnectionString(config.DataType, config.ConnectionString)
                .UseSlave(config)
                .UseMonitorCommand(config.Executing, config.Executed)
                .UseAutoSyncStructure(config.IsAutoSyncStructure)
                .UseSyncStructureToLower(config.IsSyncStructureToLower)
                .UseLazyLoading(config.IsLazyLoading)
                .Build();

            var dbContext = Activator.CreateInstance(type, context) as TDbContext;

            if (dbContext != null)
            {
                var method = type.GetMethod("InitDbContext", BindingFlags.Instance | BindingFlags.NonPublic);
                method?.Invoke(dbContext, new[] { CodeFirstFactory });
            }

            return dbContext;
        }
    }
}
