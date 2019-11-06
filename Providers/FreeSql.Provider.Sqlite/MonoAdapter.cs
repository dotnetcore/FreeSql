using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace FreeSql.Sqlite
{
    internal class MonoAdapter
    {

        static bool? _isMono;
        static object _isMonoLock = new object();
        static Assembly _monoAssemly;
        static Type _monoSqliteConnectionType;
        static Type _monoSqliteCommandType;
        static Type _monoSqliteParameterType;
        static Type _monoSqliteExceptionType;

        static bool IsMono
        {
            get
            {
                if (_isMono != null) return _isMono == true;
                lock (_isMonoLock)
                {
                    Assembly ass = null;
                    try
                    {
                        ass = Assembly.Load("Mono.Data.Sqlite");
                    }
                    catch { }
                    _isMono = ass != null;
                    if (_isMono == false) return false;

                    _monoAssemly = ass;
                    _monoSqliteConnectionType = _monoAssemly.GetType("Mono.Data.Sqlite.SqliteConnection");
                    _monoSqliteCommandType = _monoAssemly.GetType("Mono.Data.Sqlite.SqliteCommand");
                    _monoSqliteParameterType = _monoAssemly.GetType("Mono.Data.Sqlite.SqliteParameter");
                    _monoSqliteExceptionType = _monoAssemly.GetType("Mono.Data.Sqlite.SqliteException");
                }
                return true;
            }
        }

        public static DbConnection GetSqliteConnection(string connectionString)
        {
            if (IsMono == false) return new System.Data.SQLite.SQLiteConnection(connectionString);
            return Activator.CreateInstance(_monoSqliteConnectionType, new object[] { connectionString }) as DbConnection;
        }

        public static DbCommand GetSqliteCommand()
        {
            if (IsMono == false) return new System.Data.SQLite.SQLiteCommand();
            return Activator.CreateInstance(_monoSqliteCommandType, new object[0]) as DbCommand;
        }

        public static DbParameter GetSqliteParameter()
        {
            if (IsMono == false) return new System.Data.SQLite.SQLiteParameter();
            return Activator.CreateInstance(_monoSqliteParameterType, new object[0]) as DbParameter;
        }

        public static bool IsSqliteException(Exception exception)
        {
            if (exception == null) return false;
            if (IsMono == false) return exception is System.Data.SQLite.SQLiteException;
            return exception.GetType() == _monoSqliteExceptionType;
        }

    }
}
