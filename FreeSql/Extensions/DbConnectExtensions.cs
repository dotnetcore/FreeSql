#if NET40
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Common
{
    public static class DbConnectExtensions
    {
        public static Task OpenAsync(this DbConnection connection)
        {
            return Task.Factory.StartNew(() => connection.Open());
        }

        public static Task<int> ExecuteNonQueryAsync(this DbCommand cmd)
        {
            return Task.Factory.StartNew(() => cmd.ExecuteNonQuery());
        }

        public static Task<object> ExecuteScalarAsync(this DbCommand cmd)
        {
            return Task.Factory.StartNew(() => cmd.ExecuteScalar());
        }

        public static Task<DbDataReader> ExecuteReaderAsync(this DbCommand cmd)
        {
            return Task.Factory.StartNew(() => cmd.ExecuteReader());
        }

        public static Task<bool> IsDBNullAsync(this DbDataReader dr, int ordinal)
        {
            return Task.Factory.StartNew(() => dr.IsDBNull(ordinal));
        }

        public static Task<T> GetFieldValueAsync<T>(this DbDataReader dr, int ordinal)
        {
            return Task.Factory.StartNew(() => (T)dr.GetValue(ordinal));
        }

        public static Task<bool> ReadAsync(this DbDataReader dr)
        {
            return Task.Factory.StartNew(() => dr.Read());
        }
    }
}
#endif