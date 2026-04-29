using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Provider.SonnetDB.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.SonnetDB
{
    class SonnetDBCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {
        public SonnetDBCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        static readonly object _dicCsToDbLock = new object();
        static readonly Dictionary<string, CsToDb<DbType>> _dicCsToDb = new Dictionary<string, CsToDb<DbType>>
        {
            { typeof(bool).FullName, CsToDb.New(DbType.Boolean, "BOOL", "BOOL", null, false, false) },
            { typeof(bool?).FullName, CsToDb.New(DbType.Boolean, "BOOL", "BOOL", null, true, null) },
            { typeof(short).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, false, 0) },
            { typeof(short?).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, true, null) },
            { typeof(int).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, false, 0) },
            { typeof(int?).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, true, null) },
            { typeof(long).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, false, 0L) },
            { typeof(long?).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, true, null) },
            { typeof(byte).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, false, 0) },
            { typeof(byte?).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, true, null) },
            { typeof(sbyte).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, false, 0) },
            { typeof(sbyte?).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, true, null) },
            { typeof(ushort).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, false, 0) },
            { typeof(ushort?).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, true, null) },
            { typeof(uint).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, false, 0) },
            { typeof(uint?).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, true, null) },
            { typeof(ulong).FullName, CsToDb.New(DbType.Decimal, "INT", "INT", false, false, 0) },
            { typeof(ulong?).FullName, CsToDb.New(DbType.Decimal, "INT", "INT", false, true, null) },
            { typeof(float).FullName, CsToDb.New(DbType.Double, "FLOAT", "FLOAT", false, false, 0) },
            { typeof(float?).FullName, CsToDb.New(DbType.Double, "FLOAT", "FLOAT", false, true, null) },
            { typeof(double).FullName, CsToDb.New(DbType.Double, "FLOAT", "FLOAT", false, false, 0) },
            { typeof(double?).FullName, CsToDb.New(DbType.Double, "FLOAT", "FLOAT", false, true, null) },
            { typeof(decimal).FullName, CsToDb.New(DbType.Double, "FLOAT", "FLOAT", false, false, 0) },
            { typeof(decimal?).FullName, CsToDb.New(DbType.Double, "FLOAT", "FLOAT", false, true, null) },
            { typeof(string).FullName, CsToDb.New(DbType.String, "STRING", "STRING", false, true, null) },
            { typeof(char).FullName, CsToDb.New(DbType.String, "STRING", "STRING", false, true, null) },
            { typeof(char?).FullName, CsToDb.New(DbType.String, "STRING", "STRING", false, true, null) },
            { typeof(Guid).FullName, CsToDb.New(DbType.String, "STRING", "STRING", false, false, Guid.Empty) },
            { typeof(Guid?).FullName, CsToDb.New(DbType.String, "STRING", "STRING", false, true, null) },
            { typeof(DateTime).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, false, new DateTime(1970, 1, 1)) },
            { typeof(DateTime?).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, true, null) },
            { typeof(DateTimeOffset).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, false, DateTimeOffset.UnixEpoch) },
            { typeof(DateTimeOffset?).FullName, CsToDb.New(DbType.Int64, "INT", "INT", false, true, null) },
        };

        public override DbInfoResult GetDbInfo(Type type)
        {
            if (_dicCsToDb.TryGetValue(type.FullName, out var trydc))
                return new DbInfoResult((int)trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable, trydc.defaultValue);

            if (type.IsArray) return null;
            var enumType = type.IsEnum ? type : null;
            if (enumType == null && type.IsNullableType() && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments.First().IsEnum)
                enumType = type.GenericTypeArguments.First();
            if (enumType != null)
            {
                var newItem = CsToDb.New(DbType.Int64, "INT", "INT", false, type.IsEnum ? false : true, enumType.CreateInstanceGetDefaultValue());
                if (_dicCsToDb.ContainsKey(type.FullName) == false)
                {
                    lock (_dicCsToDbLock)
                    {
                        if (_dicCsToDb.ContainsKey(type.FullName) == false) _dicCsToDb.Add(type.FullName, newItem);
                    }
                }
                return new DbInfoResult((int)newItem.type, newItem.dbtype, newItem.dbtypeFull, newItem.isnullable, newItem.defaultValue);
            }
            return null;
        }

        protected override string GetComparisonDDLStatements(params TypeSchemaAndName[] objects)
        {
            var sb = new StringBuilder();
            foreach (var obj in objects)
            {
                var tb = obj.tableSchema;
                if (tb == null) throw new Exception(CoreErrorStrings.S_Type_IsNot_Migrable(obj.tableSchema.Type.FullName));
                if (tb.Columns.Any() == false) throw new Exception(CoreErrorStrings.S_Type_IsNot_Migrable_0Attributes(obj.tableSchema.Type.FullName));

                var tbname = string.IsNullOrEmpty(obj.tableName) ? tb.DbName : obj.tableName;
                var tableParts = _commonUtils.SplitTableName(tbname);
                tbname = tableParts.LastOrDefault() ?? tbname;
                if (ExistsMeasurement(tbname)) continue;

                var fieldCount = 0;
                var ddlColumns = new List<string>();
                foreach (var col in tb.ColumnsByPosition)
                {
                    if (IsTimeColumn(col)) continue;
                    var ddl = GetColumnDefinition(tb, col);
                    if (ddl.IndexOf(" FIELD", StringComparison.OrdinalIgnoreCase) >= 0) fieldCount++;
                    ddlColumns.Add(ddl);
                }

                if (fieldCount == 0)
                    throw new Exception($"SonnetDB measurement '{tbname}' must contain at least one FIELD column. Mark one property with [SonnetDBField] or set Column.DbType = \"FIELD <type>\".");

                if (sb.Length > 0) sb.AppendLine();
                sb.Append("CREATE MEASUREMENT ").Append(_commonUtils.QuoteSqlName(tbname)).Append(" (");
                for (var a = 0; a < ddlColumns.Count; a++)
                {
                    if (a > 0) sb.Append(",");
                    sb.AppendLine().Append("  ").Append(ddlColumns[a]);
                }
                sb.AppendLine().Append(");");
            }

            return sb.Length == 0 ? null : sb.ToString();
        }

        bool ExistsMeasurement(string name)
        {
            try
            {
                _orm.Ado.ExecuteDataTable($"SELECT * FROM {_commonUtils.QuoteSqlName(name)} LIMIT 0");
                return true;
            }
            catch
            {
                return false;
            }
        }

        string GetColumnDefinition(TableInfo tb, ColumnInfo col)
        {
            var quotedName = _commonUtils.QuoteSqlName(col.Attribute.Name);
            var dbType = (col.Attribute.DbType ?? "").Trim();
            if (dbType.StartsWith("TAG", StringComparison.OrdinalIgnoreCase) ||
                dbType.StartsWith("FIELD", StringComparison.OrdinalIgnoreCase))
                return $"{quotedName} {NormalizeColumnDefinition(dbType)}";

            if (IsTagColumn(tb, col)) return $"{quotedName} TAG";
            return $"{quotedName} FIELD {NormalizeFieldType(dbType)}";
        }

        bool IsTagColumn(TableInfo tb, ColumnInfo col)
        {
            if (tb.Properties.TryGetValue(col.CsName, out var property))
            {
                if (property.GetCustomAttribute<SonnetDBTagAttribute>() != null) return true;
                if (property.GetCustomAttribute<SonnetDBFieldAttribute>() != null) return false;
            }
            var mapType = col.Attribute.MapType.NullableTypeOrThis();
            return mapType == typeof(string) || mapType == typeof(char) || mapType == typeof(Guid);
        }

        static bool IsTimeColumn(ColumnInfo col) => string.Equals(col.Attribute.Name, "time", StringComparison.OrdinalIgnoreCase);

        static string NormalizeColumnDefinition(string dbType)
        {
            var normalized = StripColumnModifiers(dbType);
            if (normalized.StartsWith("TAG", StringComparison.OrdinalIgnoreCase))
            {
                var tagType = normalized.Substring(3).Trim();
                return string.IsNullOrEmpty(tagType) || tagType.Equals("STRING", StringComparison.OrdinalIgnoreCase) ? "TAG" : $"TAG {tagType}";
            }
            if (normalized.StartsWith("FIELD", StringComparison.OrdinalIgnoreCase))
                return $"FIELD {NormalizeFieldType(normalized.Substring(5))}";
            return normalized;
        }

        static string NormalizeFieldType(string dbType)
        {
            if (string.IsNullOrWhiteSpace(dbType)) return "STRING";
            var normalized = StripColumnModifiers(dbType).ToUpperInvariant();
            if (normalized.Contains("VECTOR")) return normalized;
            if (normalized.Contains("GEOPOINT")) return "GEOPOINT";
            if (normalized.Contains("FLOAT") || normalized.Contains("DOUBLE") || normalized.Contains("REAL") || normalized.Contains("DECIMAL")) return "FLOAT";
            if (normalized.Contains("BIGINT") || normalized.Contains("LONG") || normalized.Contains("INT64") || normalized.Contains("INTEGER") || normalized.Contains("INT")) return "INT";
            if (normalized.Contains("BOOLEAN") || normalized.Contains("BOOL")) return "BOOL";
            if (normalized.Contains("TEXT") || normalized.Contains("VARCHAR") || normalized.Contains("CHAR") || normalized.Contains("STRING")) return "STRING";
            return normalized;
        }

        static string StripColumnModifiers(string dbType)
        {
            var normalized = Regex.Replace(dbType.Trim(), @"\s+NOT\s+NULL\b|\s+NULL\b", "", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, @"\s+DEFAULT\s+('[^']*'|""[^""]*""|\S+)", "", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
            switch (normalized.ToUpperInvariant())
            {
                case "FLOAT64":
                    return "FLOAT";
                case "INT64":
                    return "INT";
                case "BOOLEAN":
                    return "BOOL";
                case "TEXT":
                case "VARCHAR":
                case "CHAR":
                    return "STRING";
                default:
                    return normalized;
            }
        }
    }
}
