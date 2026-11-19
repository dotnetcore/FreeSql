using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FreeSql.DatabaseModel;
using FreeSql.Internal;

namespace FreeSql.IoTDB
{
    internal sealed class IoTDBDbFirst : IDbFirst
    {
        readonly IFreeSql _orm;
        readonly CommonUtils _commonUtils;

        public IoTDBDbFirst(IFreeSql orm, CommonUtils commonUtils)
        {
            _orm = orm;
            _commonUtils = commonUtils;
        }

        public List<string> GetDatabases() => _orm.Ado.Query<string>(CommandType.Text, "SHOW DATABASES").Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        public List<DbTableInfo> GetTablesByDatabase(params string[] database)
        {
            var prefix = database?.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
            if (string.IsNullOrWhiteSpace(prefix)) prefix = (_orm.Ado as IoTDBAdo)?.Database;
            var sql = string.IsNullOrWhiteSpace(prefix)
                ? "SHOW TIMESERIES root.**"
                : "SHOW TIMESERIES " + IoTDBUtils.RequireDevicePath(prefix) + ".**";
            var rows = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            var tableColumns = new Dictionary<string, List<DbColumnInfo>>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                if (row == null || row.Length == 0) continue;
                var path = Convert.ToString(row[0]);
                if (string.IsNullOrWhiteSpace(path)) continue;
                var split = path.Split('.');
                if (split.Length < 4) continue;
                var device = string.Join(".", split.Take(split.Length - 1));
                var typeName = row.Length > 3 ? Convert.ToString(row[3]) : "TEXT";
                var type = ParseDataType(typeName);
                if (!tableColumns.TryGetValue(device, out var columns)) tableColumns.Add(device, columns = new List<DbColumnInfo>());
                columns.Add(new DbColumnInfo
                {
                    Name = split[split.Length - 1],
                    CsType = type.CsType,
                    DbType = (int)type.DbType,
                    DbTypeText = type.DbName,
                    DbTypeTextFull = type.DbName,
                    IsNullable = true,
                    Position = columns.Count + 1
                });
            }

            return tableColumns.Select(pair =>
            {
                var dbTable = new DbTableInfo { Id = pair.Key, Name = pair.Key, Schema = ExtractDatabaseName(pair.Key), Type = DbTableType.TABLE };
                var timeColumn = new DbColumnInfo
                {
                    Table = dbTable,
                    Name = "time",
                    CsType = typeof(DateTime),
                    DbType = (int)DbType.DateTime,
                    DbTypeText = "TIMESTAMP",
                    DbTypeTextFull = "TIMESTAMP",
                    IsPrimary = true,
                    IsNullable = false,
                    Position = 0
                };
                dbTable.Columns.Add(timeColumn);
                dbTable.Primarys.Add(timeColumn);
                foreach (var column in pair.Value)
                {
                    column.Table = dbTable;
                    dbTable.Columns.Add(column);
                }
                return dbTable;
            }).OrderBy(table => table.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public DbTableInfo GetTableByName(string name, bool ignoreCase = true)
        {
            var comparer = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return GetTablesByDatabase().FirstOrDefault(table => string.Equals(table.Name, name, comparer));
        }

        public bool ExistsTable(string name, bool ignoreCase = true) => GetTableByName(name, ignoreCase) != null;

        public int GetDbType(DbColumnInfo column) => (int)ParseDataType(column.DbTypeText).DbType;
        public string GetCsConvert(DbColumnInfo column) => "(" + GetCsType(column) + ")";
        public string GetCsTypeValue(DbColumnInfo column) => GetCsType(column) + "." + (column.IsNullable ? "MinValue" : "MinValue");
        public string GetCsType(DbColumnInfo column) => ParseDataType(column.DbTypeText).CsType.FullName;
        public Type GetCsTypeInfo(DbColumnInfo column) => ParseDataType(column.DbTypeText).CsType;
        public string GetDataReaderMethod(DbColumnInfo column)
        {
            var type = GetCsTypeInfo(column);
            if (type == typeof(bool)) return "GetBoolean";
            if (type == typeof(int)) return "GetInt32";
            if (type == typeof(long)) return "GetInt64";
            if (type == typeof(float)) return "GetFloat";
            if (type == typeof(double)) return "GetDouble";
            if (type == typeof(DateTime)) return "GetDateTime";
            if (type == typeof(byte[])) return "GetFieldValue<byte[]>";
            return "GetString";
        }
        public string GetCsStringify(DbColumnInfo column) => "Convert.ToString({0}, System.Globalization.CultureInfo.InvariantCulture)";
        public string GetCsParse(DbColumnInfo column) => "Convert.ChangeType({0}, typeof(" + GetCsType(column) + "), System.Globalization.CultureInfo.InvariantCulture)";
        public List<DbEnumInfo> GetEnumsByDatabase(params string[] database) => new List<DbEnumInfo>();

        static (Type CsType, DbType DbType, string DbName) ParseDataType(string value)
        {
            switch ((value ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "BOOLEAN": return (typeof(bool), DbType.Boolean, "BOOLEAN");
                case "INT32": return (typeof(int), DbType.Int32, "INT32");
                case "INT64": return (typeof(long), DbType.Int64, "INT64");
                case "FLOAT": return (typeof(float), DbType.Single, "FLOAT");
                case "DOUBLE": return (typeof(double), DbType.Double, "DOUBLE");
                case "BLOB": return (typeof(byte[]), DbType.Binary, "BLOB");
                case "TIMESTAMP": return (typeof(DateTime), DbType.DateTime, "TIMESTAMP");
                case "DATE": return (typeof(DateTime), DbType.Date, "DATE");
                case "TEXT":
                case "STRING":
                default: return (typeof(string), DbType.String, "TEXT");
            }
        }

        static string ExtractDatabaseName(string devicePath)
        {
            var parts = devicePath.Split('.');
            return parts.Length >= 3 ? string.Join(".", parts.Take(parts.Length - 1)) : null;
        }
    }
}
