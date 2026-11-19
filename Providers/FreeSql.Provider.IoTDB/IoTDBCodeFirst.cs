using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;

namespace FreeSql.IoTDB
{
    internal sealed class IoTDBCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {
        static readonly Dictionary<Type, string> DataTypes = new Dictionary<Type, string>
        {
            [typeof(bool)] = "BOOLEAN",
            [typeof(byte)] = "INT32",
            [typeof(sbyte)] = "INT32",
            [typeof(short)] = "INT32",
            [typeof(ushort)] = "INT32",
            [typeof(int)] = "INT32",
            [typeof(uint)] = "INT64",
            [typeof(long)] = "INT64",
            [typeof(ulong)] = "INT64",
            [typeof(float)] = "FLOAT",
            [typeof(double)] = "DOUBLE",
            [typeof(decimal)] = "DOUBLE",
            [typeof(string)] = "TEXT",
            [typeof(char)] = "TEXT",
            [typeof(Guid)] = "TEXT",
            [typeof(byte[])] = "BLOB",
            [typeof(DateTime)] = "TIMESTAMP"
        };

        public IoTDBCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression) { }

        public override DbInfoResult GetDbInfo(Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            var actualType = nullableType ?? type;
            if (actualType.IsEnum) actualType = Enum.GetUnderlyingType(actualType);
            if (!DataTypes.TryGetValue(actualType, out var dbType)) return null;
            if (actualType == typeof(DateTime)) return new DbInfoResult((int)DbType.DateTime, "TIMESTAMP", "TIMESTAMP", nullableType != null, nullableType == null ? (object)DateTime.MinValue : null);
            var clrDbType = dbType == "BOOLEAN" ? DbType.Boolean :
                dbType == "INT32" ? DbType.Int32 :
                dbType == "INT64" ? DbType.Int64 :
                dbType == "FLOAT" ? DbType.Single :
                dbType == "DOUBLE" ? DbType.Double :
                dbType == "BLOB" ? DbType.Binary : DbType.String;
            return new DbInfoResult((int)clrDbType, dbType, dbType, nullableType != null, nullableType == null ? DefaultValue(actualType) : null);
        }

        protected override string GetComparisonDDLStatements(params TypeSchemaAndName[] objects)
        {
            var statements = new List<string>();
            foreach (var item in objects)
            {
                var table = item.tableSchema;
                if (table == null || table.ColumnsByPosition == null || table.ColumnsByPosition.Length == 0)
                    throw new InvalidOperationException("IoTDB CodeFirst requires an entity with a timestamp column and at least one measurement.");

                var devicePath = _commonUtils is IoTDBUtils
                    ? IoTDBUtils.RequireDevicePath(string.IsNullOrWhiteSpace(item.tableName) ? table.DbName : item.tableName)
                    : table.DbName;
                var timeColumn = FindTimeColumn(table);
                var measurements = table.ColumnsByPosition.Where(column => column != timeColumn).ToArray();
                if (measurements.Length == 0)
                    throw new InvalidOperationException($"IoTDB entity '{table.Type.FullName}' must define at least one measurement besides its time column.");

                foreach (var column in measurements)
                {
                    if (!IsSafeMeasurementName(column.Attribute.Name))
                        throw new ArgumentException($"Invalid IoTDB measurement name '{column.Attribute.Name}'.");
                    var dataType = ResolveIoTDBType(column);
                    var path = devicePath + "." + column.Attribute.Name;
                    if (_orm.Ado.ExecuteArray("SHOW TIMESERIES " + path).Length == 0)
                        statements.Add($"CREATE TIMESERIES {path} WITH DATATYPE={dataType}, ENCODING=PLAIN, COMPRESSOR=LZ4;");
                }
            }
            return statements.Count == 0 ? null : string.Join(Environment.NewLine, statements);
        }

        internal static ColumnInfo FindTimeColumn(TableInfo table)
        {
            var timeColumns = table.ColumnsByPosition.Where(column =>
                string.Equals(column.Attribute.Name, "time", StringComparison.OrdinalIgnoreCase) &&
                (column.Attribute.MapType == typeof(DateTime) || column.Attribute.MapType == typeof(long) || column.Attribute.MapType == typeof(long?))).ToArray();
            if (timeColumns.Length != 1)
                throw new InvalidOperationException($"IoTDB entity '{table.Type.FullName}' must have exactly one column named 'time' mapped to DateTime or Int64.");
            return timeColumns[0];
        }

        internal static string ResolveIoTDBType(ColumnInfo column)
        {
            var explicitType = column.Attribute.DbType;
            if (!string.IsNullOrWhiteSpace(explicitType))
            {
                var normalized = explicitType.Trim().ToUpperInvariant();
                if (normalized.EndsWith(" NOT NULL", StringComparison.Ordinal))
                    normalized = normalized.Substring(0, normalized.Length - " NOT NULL".Length);
                if (new[] { "BOOLEAN", "INT32", "INT64", "FLOAT", "DOUBLE", "TEXT", "STRING", "BLOB", "DATE", "TIMESTAMP" }.Contains(normalized)) return normalized;
                throw new NotSupportedException($"IoTDB measurement type '{explicitType}' on '{column.CsName}' is not supported.");
            }

            var type = Nullable.GetUnderlyingType(column.Attribute.MapType) ?? column.Attribute.MapType;
            if (type.IsEnum) type = Enum.GetUnderlyingType(type);
            if (DataTypes.TryGetValue(type, out var dbType)) return dbType;
            throw new NotSupportedException($"IoTDB Tree does not support CLR measurement type '{column.Attribute.MapType.FullName}' on '{column.CsName}'. Set a supported IoTDB DbType or a supported MapType.");
        }

        static bool IsSafeMeasurementName(string name) => IoTDBUtils.IsDevicePath("root.db." + name);

        static object DefaultValue(Type type)
        {
            if (type == typeof(string)) return string.Empty;
            if (type == typeof(byte[])) return Array.Empty<byte>();
            if (type.IsValueType) return Activator.CreateInstance(type);
            return null;
        }
    }
}
