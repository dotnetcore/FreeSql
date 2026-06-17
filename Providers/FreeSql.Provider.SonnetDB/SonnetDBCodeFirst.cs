// SonnetDBCodeFirst.cs
// SonnetDB Code First DDL 生成器。
//
// 功能：根据 C# 实体类的属性类型与 Attribute 配置，生成 SonnetDB 的
//       CREATE MEASUREMENT DDL 语句，并在 IsAutoSyncStructure = true 时自动建表。
//
// SonnetDB 数据模型说明：
//   MEASUREMENT —— 等价于关系型"表"，每条记录隐含 time 列（Unix 毫秒整数）。
//   TAG         —— 字符串维度列，被索引，用于过滤/分组；语义上是维度键。
//   FIELD       —— 数值/布尔观测列，存储实际测量值。
//   建表要求：每张 measurement 至少需要一个 FIELD 列，否则写入会失败。
//
// C# 类型 → SonnetDB DDL 类型映射（主要规则）：
//   bool / bool?                → BOOL
//   short/int/long 及无符号整型  → INT（SonnetDB 内部统一为 int64）
//   float / double / decimal    → FLOAT（内部为 float64）
//   string / char / Guid        → STRING（默认映射为 TAG）
//   DateTime / DateTimeOffset   → INT（存储 Unix 毫秒时间戳，与 time 列语义一致）
//   枚举                         → INT
//
// 特殊类型处理：
//   VECTOR(N)   — NormalizeFieldType 直接透传，保留括号内维度数
//   GEOPOINT    — 规范化为大写 "GEOPOINT"
//
// 列角色判定优先级（GetColumnDefinition）：
//   1. col.Attribute.DbType 显式包含 "TAG" 或 "FIELD" 前缀 → 直接使用
//   2. 属性上有 [SonnetDBTag]  → TAG
//   3. 属性上有 [SonnetDBField] → FIELD
//   4. 映射类型为 string/char/Guid → TAG（默认字符串为维度）
//   5. 其他 → FIELD

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

        /// <summary>
        /// C# 完整类型名 → SonnetDB 类型映射表。
        /// <para>DateTime / DateTimeOffset 映射为 INT（Unix 毫秒），不使用 SonnetDB 原生时间类型。</para>
        /// <para>所有整型（含无符号）均映射为 INT，SonnetDB 内部存储为 int64。</para>
        /// </summary>
        static readonly Dictionary<string, CsToDb<DbType>> _dicCsToDb = new Dictionary<string, CsToDb<DbType>>
        {
            { typeof(bool).FullName,            CsToDb.New(DbType.Boolean, "BOOL",  "BOOL",  null,  false, false) },
            { typeof(bool?).FullName,           CsToDb.New(DbType.Boolean, "BOOL",  "BOOL",  null,  true,  null) },
            { typeof(short).FullName,           CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, 0) },
            { typeof(short?).FullName,          CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(int).FullName,             CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, 0) },
            { typeof(int?).FullName,            CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(long).FullName,            CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, 0L) },
            { typeof(long?).FullName,           CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(byte).FullName,            CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, 0) },
            { typeof(byte?).FullName,           CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(sbyte).FullName,           CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, 0) },
            { typeof(sbyte?).FullName,          CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(ushort).FullName,          CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, 0) },
            { typeof(ushort?).FullName,         CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(uint).FullName,            CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, 0) },
            { typeof(uint?).FullName,           CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(ulong).FullName,           CsToDb.New(DbType.Decimal, "INT",   "INT",   false, false, 0) },
            { typeof(ulong?).FullName,          CsToDb.New(DbType.Decimal, "INT",   "INT",   false, true,  null) },
            { typeof(float).FullName,           CsToDb.New(DbType.Double,  "FLOAT", "FLOAT", false, false, 0) },
            { typeof(float?).FullName,          CsToDb.New(DbType.Double,  "FLOAT", "FLOAT", false, true,  null) },
            { typeof(double).FullName,          CsToDb.New(DbType.Double,  "FLOAT", "FLOAT", false, false, 0) },
            { typeof(double?).FullName,         CsToDb.New(DbType.Double,  "FLOAT", "FLOAT", false, true,  null) },
            { typeof(decimal).FullName,         CsToDb.New(DbType.Double,  "FLOAT", "FLOAT", false, false, 0) },
            { typeof(decimal?).FullName,        CsToDb.New(DbType.Double,  "FLOAT", "FLOAT", false, true,  null) },
            { typeof(string).FullName,          CsToDb.New(DbType.String,  "STRING","STRING",false, true,  null) },
            { typeof(char).FullName,            CsToDb.New(DbType.String,  "STRING","STRING",false, true,  null) },
            { typeof(char?).FullName,           CsToDb.New(DbType.String,  "STRING","STRING",false, true,  null) },
            { typeof(Guid).FullName,            CsToDb.New(DbType.String,  "STRING","STRING",false, false, Guid.Empty) },
            { typeof(Guid?).FullName,           CsToDb.New(DbType.String,  "STRING","STRING",false, true,  null) },
            // DateTime / DateTimeOffset 存储为 Unix 毫秒整数（与隐式 time 列语义一致）。
            { typeof(DateTime).FullName,        CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, new DateTime(1970, 1, 1)) },
            { typeof(DateTime?).FullName,       CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
            { typeof(DateTimeOffset).FullName,  CsToDb.New(DbType.Int64,   "INT",   "INT",   false, false, DateTimeOffset.UnixEpoch) },
            { typeof(DateTimeOffset?).FullName, CsToDb.New(DbType.Int64,   "INT",   "INT",   false, true,  null) },
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
                // 枚举类型统一映射为 INT，存储枚举底层整数值。
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

        /// <summary>
        /// 为指定实体类型生成 SonnetDB <c>CREATE MEASUREMENT</c> DDL 语句。
        /// <para>若 measurement 已存在（通过 <c>SELECT * LIMIT 0</c> 验证），则跳过不生成。</para>
        /// <para>若实体中没有任何 FIELD 列，抛出异常（SonnetDB 要求至少一个 FIELD）。</para>
        /// </summary>
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
                // 若 measurement 已存在则跳过（SonnetDB CodeFirst 仅支持建表，不支持 ALTER）。
                if (ExistsMeasurement(tbname)) continue;

                var fieldCount = 0;
                var ddlColumns = new List<string>();
                foreach (var col in tb.ColumnsByPosition)
                {
                    // time 列是 SonnetDB 隐式内置列，不需要在 CREATE MEASUREMENT 中声明。
                    if (IsTimeColumn(col)) continue;
                    var ddl = GetColumnDefinition(tb, col);
                    if (ddl.IndexOf(" FIELD", StringComparison.OrdinalIgnoreCase) >= 0) fieldCount++;
                    ddlColumns.Add(ddl);
                }

                // SonnetDB 要求每张 measurement 至少包含一个 FIELD 列。
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

        /// <summary>
        /// 通过执行 <c>SELECT * LIMIT 0</c> 判断 measurement 是否已存在。
        /// </summary>
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

        /// <summary>
        /// 为单列生成 DDL 片段，格式为 <c>"colName" TAG</c> 或 <c>"colName" FIELD FLOAT</c> 等。
        /// <para>判断列角色的优先级：显式 DbType 前缀 → [SonnetDBTag]/[SonnetDBField] Attribute → 类型推断。</para>
        /// </summary>
        string GetColumnDefinition(TableInfo tb, ColumnInfo col)
        {
            var quotedName = _commonUtils.QuoteSqlName(col.Attribute.Name);
            var dbType = (col.Attribute.DbType ?? "").Trim();
            // 如果 DbType 已经显式包含 TAG 或 FIELD 前缀，直接规范化后使用。
            if (dbType.StartsWith("TAG", StringComparison.OrdinalIgnoreCase) ||
                dbType.StartsWith("FIELD", StringComparison.OrdinalIgnoreCase))
                return $"{quotedName} {NormalizeColumnDefinition(dbType)}";

            // 根据 Attribute 或类型推断列角色。
            if (IsTagColumn(tb, col)) return $"{quotedName} TAG";
            return $"{quotedName} FIELD {NormalizeFieldType(dbType)}";
        }

        /// <summary>
        /// 判断列是否应映射为 TAG（索引字符串维度）。
        /// <para>优先级：[SonnetDBTag] > [SonnetDBField] > 类型推断（string/char/Guid → TAG）。</para>
        /// </summary>
        bool IsTagColumn(TableInfo tb, ColumnInfo col)
        {
            if (tb.Properties.TryGetValue(col.CsName, out var property))
            {
                if (property.GetCustomAttribute<SonnetDBTagAttribute>() != null) return true;
                if (property.GetCustomAttribute<SonnetDBFieldAttribute>() != null) return false;
            }
            // 字符串类型及 Guid 默认视为 TAG（维度字段）。
            var mapType = col.Attribute.MapType.NullableTypeOrThis();
            return mapType == typeof(string) || mapType == typeof(char) || mapType == typeof(Guid);
        }

        /// <summary>
        /// 判断列是否为隐式 time 列（建表时无需声明）。
        /// </summary>
        static bool IsTimeColumn(ColumnInfo col) => string.Equals(col.Attribute.Name, "time", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 规范化完整列定义字符串（含 TAG/FIELD 前缀）。
        /// 去除 NOT NULL / DEFAULT 等修饰词，统一大写关键字。
        /// </summary>
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

        /// <summary>
        /// 将 DbType 字符串规范化为 SonnetDB FIELD 数据类型。
        /// <list type="bullet">
        ///   <item>VECTOR(N)   — 直接透传，保留维度数</item>
        ///   <item>GEOPOINT    — 规范化为大写</item>
        ///   <item>float/double/decimal/real → FLOAT</item>
        ///   <item>int/bigint/long/int64     → INT</item>
        ///   <item>bool/boolean              → BOOL</item>
        ///   <item>text/varchar/char/string  → STRING</item>
        ///   <item>空或未知                   → STRING（默认）</item>
        /// </list>
        /// </summary>
        static string NormalizeFieldType(string dbType)
        {
            if (string.IsNullOrWhiteSpace(dbType)) return "STRING";
            var normalized = StripColumnModifiers(dbType).ToUpperInvariant();
            // VECTOR(N) 透传，保留括号内的维度参数。
            if (normalized.Contains("VECTOR")) return normalized;
            if (normalized.Contains("GEOPOINT")) return "GEOPOINT";
            if (normalized.Contains("FLOAT") || normalized.Contains("DOUBLE") || normalized.Contains("REAL") || normalized.Contains("DECIMAL")) return "FLOAT";
            if (normalized.Contains("BIGINT") || normalized.Contains("LONG") || normalized.Contains("INT64") || normalized.Contains("INTEGER") || normalized.Contains("INT")) return "INT";
            if (normalized.Contains("BOOLEAN") || normalized.Contains("BOOL")) return "BOOL";
            if (normalized.Contains("TEXT") || normalized.Contains("VARCHAR") || normalized.Contains("CHAR") || normalized.Contains("STRING")) return "STRING";
            return normalized;
        }

        /// <summary>
        /// 去除列定义中的 NOT NULL / NULL / DEFAULT 等修饰词，并规范化空白。
        /// 同时将 float64/int64/boolean/text/varchar/char 别名统一为 SonnetDB 标准类型名。
        /// </summary>
        static string StripColumnModifiers(string dbType)
        {
            var normalized = Regex.Replace(dbType.Trim(), @"\s+NOT\s+NULL\b|\s+NULL\b", "", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, @"\s+DEFAULT\s+('[^']*'|""[^""]*""|\S+)", "", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
            switch (normalized.ToUpperInvariant())
            {
                case "FLOAT64":  return "FLOAT";
                case "INT64":    return "INT";
                case "BOOLEAN":  return "BOOL";
                case "TEXT":
                case "VARCHAR":
                case "CHAR":     return "STRING";
                default:         return normalized;
            }
        }
    }
}
