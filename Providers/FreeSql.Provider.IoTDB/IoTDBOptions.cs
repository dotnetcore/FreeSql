using System;
using System.Data.Common;
using Apache.IoTDB.Data;

namespace FreeSql.IoTDB
{
    internal sealed class IoTDBOptions
    {
        public string ConnectionString { get; private set; }
        public string Database { get; private set; }
        public bool UseAdoConnectionPool { get; private set; }

        public static IoTDBOptions Parse(string connectionString)
        {
            var options = new IoTDBOptions();
            if (connectionString == null) return options;

            var raw = connectionString;
            if (raw.StartsWith("AdoConnectionPool,", StringComparison.OrdinalIgnoreCase))
            {
                options.UseAdoConnectionPool = true;
                raw = raw.Substring("AdoConnectionPool,".Length);
            }

            var builder = new DbConnectionStringBuilder { ConnectionString = raw };
            foreach (var key in new[] { "Database", "DefaultDatabase", "Initial Catalog", "db" })
            {
                if (!builder.TryGetValue(key, out var value)) continue;
                if (options.Database == null) options.Database = Convert.ToString(value);
                builder.Remove(key);
            }

            var driverBuilder = new IoTDBConnectionStringBuilder(builder.ConnectionString);
            options.ConnectionString = driverBuilder.ConnectionString;
            return options;
        }
    }
}
