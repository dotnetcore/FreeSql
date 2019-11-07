using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace FreeSql.Sqlite
{
    internal class AdonetPortable
    {

#if ns20
        static bool _IsMicrosoft_Data_Sqlite;
        static object _IsMicrosoft_Data_SqliteLock = new object();

        static T PortableAction<T>(Func<T> systemCreate, Func<T> microsoftCreate)
        {
            if (_IsMicrosoft_Data_Sqlite == false)
            {
                try
                {
                    return systemCreate();
                }
                catch
                {
                    lock (_IsMicrosoft_Data_SqliteLock)
                    {
                        _IsMicrosoft_Data_Sqlite = true;
                    }
                }
            }
            return microsoftCreate();
        }

        public static DbConnection GetSqliteConnection(string connectionString) => PortableAction<DbConnection>(
            () => new System.Data.SQLite.SQLiteConnection(connectionString), 
            () => new Microsoft.Data.Sqlite.SqliteConnection(connectionString));
  
        public static DbCommand GetSqliteCommand() => PortableAction<DbCommand>(
            () => new System.Data.SQLite.SQLiteCommand(),
            () => new Microsoft.Data.Sqlite.SqliteCommand());

        public static DbParameter GetSqliteParameter() => PortableAction<DbParameter>(
            () => new System.Data.SQLite.SQLiteParameter(),
            () => new Microsoft.Data.Sqlite.SqliteParameter());

        public static bool IsSqliteException(Exception exception) => PortableAction<bool>(
            () => exception is System.Data.SQLite.SQLiteException,
            () => exception is Microsoft.Data.Sqlite.SqliteException);
#else

        public static DbConnection GetSqliteConnection(string connectionString) => new System.Data.SQLite.SQLiteConnection(connectionString);
  
        public static DbCommand GetSqliteCommand() => new System.Data.SQLite.SQLiteCommand();

        public static DbParameter GetSqliteParameter() => new System.Data.SQLite.SQLiteParameter();

        public static bool IsSqliteException(Exception exception) => exception is System.Data.SQLite.SQLiteException;
#endif
    }
}
