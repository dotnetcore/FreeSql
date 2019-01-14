using FreeSql.Site.DAL.Helper;
using System;

namespace FreeSql.Site.DAL
{
    public static class Db
    {
        public static System.Collections.Generic.Dictionary<string, IFreeSql> ConnectionPool = new System.Collections.Generic.Dictionary<string, IFreeSql>();

        private static string getConnectionString(string sDatabaseType)
        {
            return AppSettingsManager.Get($"DbContexts:{sDatabaseType}:ConnectionString");
        }

        private static IFreeSql SelectDBType(string dbtype)
        {
            if (!ConnectionPool.ContainsKey(dbtype))
            {
                ConnectionPool.Add(dbtype, new FreeSql.FreeSqlBuilder()
                    .UseConnectionString(FreeSql.DataType.MySql, getConnectionString(dbtype))
                    .Build());
            }
            return ConnectionPool[dbtype];
        }

        public static IFreeSql DB(this DataBaseType t)
        {
            return SelectDBType(t.ToString());
        }
    }

    public enum DataBaseType
    {
        MySql,
        SqlServer,
        PostgreSQL,
        Oracle,
        Sqlite
    }
}
