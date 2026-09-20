// SonnetDB 通用工具实现。
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using SndbParameter = global::SonnetDB.Data.SndbParameter;
using SndbGeoPoint = global::SonnetDB.Model.GeoPoint;

namespace FreeSql.SonnetDB
{
    class SonnetDBUtils : CommonUtils
    {
        public SonnetDBUtils(IFreeSql orm) : base(orm) { }

        /// <summary>
        /// SonnetDB 3.1.0 的解析器只接受 INSERT ... VALUES，不支持 INSERT ... SELECT。
        /// </summary>
        public override void ValidateInsertIntoSelect() => throw new NotSupportedException(
            "SonnetDB 3.1 不支持 INSERT ... SELECT；FreeSql ISelect.InsertInto 不可用，请改用分批 INSERT。");

        /// <summary>
        /// SonnetDB 3.1.0 没有可执行的递归 CTE 语法合同。
        /// </summary>
        public override void ValidateRecursiveCte() => throw new NotSupportedException(
            "SonnetDB 3.1 不支持 WITH/递归 CTE 查询；FreeSql AsTreeCte 不可用，请改写为普通查询或分步处理。");

        /// <summary>
        /// FreeSql 读取器会将多条嵌套查询合并为 UNION ALL；SonnetDB 3.1 尚不支持该语法。
        /// </summary>
        public override void ValidateUnionAll() => throw new NotSupportedException(
            "SonnetDB 3.1 不支持 UNION ALL；当前嵌套查询会生成 UNION ALL，请改为单条查询或等待 SonnetDB 补齐支持。");

        /// <summary>
        /// 校验 SonnetDB 3.1 不支持的查询派生操作。
        /// 关系表的 ToUpdate/ToDelete 由公共实现生成明确的单列主键投影，允许继续执行。
        /// </summary>
        public override void ValidateSelectOperation(string operation)
        {
            switch (operation)
            {
                case "ForUpdate":
                    throw new NotSupportedException(
                        "SonnetDB 3.1 不支持 SELECT ... FOR UPDATE；请使用关系表事务并依靠 ROWVERSION 乐观并发控制。" );
                case "UpdateJoin":
                    throw new NotSupportedException(
                        "SonnetDB 3.1 不支持 UPDATE JOIN；请先查询主键，再执行关系表 UPDATE。" );
            }
        }

        /// <summary>
        /// 特殊 C# 类型 → 数据库兼容值的转换函数表。
        /// 无符号整型、char、BigInteger 等需要在绑定参数前转换为 SonnetDB 驱动接受的类型。
        /// </summary>
        static readonly Dictionary<string, Func<object, object>> _dicGetParamterValue = new Dictionary<string, Func<object, object>>
        {
            { typeof(uint).FullName, a => long.Parse(string.Concat(a), CultureInfo.InvariantCulture) },
            { typeof(ushort).FullName, a => int.Parse(string.Concat(a), CultureInfo.InvariantCulture) },
            { typeof(byte).FullName, a => short.Parse(string.Concat(a), CultureInfo.InvariantCulture) },
            { typeof(sbyte).FullName, a => short.Parse(string.Concat(a), CultureInfo.InvariantCulture) },
            // SonnetDB ADO 参数绑定不接受 Char，只接受字符串标量；
            // char 列在 SonnetDB 中也是 STRING，因此统一转为单字符字符串。
            { typeof(char).FullName, a => string.Concat(a).Replace("\0", " ", StringComparison.Ordinal) },
        };

        static bool IsBigInteger(Type type) =>
            type?.NullableTypeOrThis() == typeof(BigInteger);

        internal static NotSupportedException UnsupportedBigInteger(Type type) => new NotSupportedException(
            $"SonnetDB 3.1 不支持 BigInteger/任意精度整数（{type?.FullName ?? typeof(BigInteger).FullName}）；" +
            "请改用 long/ulong（SonnetDB INT 为 int64），或等待 SonnetDB 提供任意精度数值类型。" );

        internal static long ConvertUInt64ToInt64(ulong value)
        {
            if (value > long.MaxValue)
                throw new NotSupportedException(
                    $"SonnetDB 3.1 的 INT 仅支持有符号 int64；ulong 值 {value.ToString(CultureInfo.InvariantCulture)} " +
                    $"超过 long.MaxValue（{long.MaxValue.ToString(CultureInfo.InvariantCulture)}），无法保存。" +
                    "请改用不大于 long.MaxValue 的值。");
            return (long)value;
        }

        static object GetParamterValue(Type type, object value)
        {
            if (type == null || value == null) return value;
            if (type.IsNullableType()) type = type.GenericTypeArguments.First();
            if (value is ulong unsignedValue) return ConvertUInt64ToInt64(unsignedValue);
            if (type.IsEnum)
            {
                if (Enum.GetUnderlyingType(type) == typeof(ulong))
                    return ConvertUInt64ToInt64(Convert.ToUInt64(value, CultureInfo.InvariantCulture));
                return Convert.ToInt64(value, CultureInfo.InvariantCulture);
            }
            if (_dicGetParamterValue.TryGetValue(type.FullName, out var trydic)) return trydic(value);
            return value;
        }

        static Type GetValueType(Type type, object value)
        {
            if (type != null && type.IsNullableType()) type = type.GenericTypeArguments.First();
            return type == null || type == typeof(object) ? value?.GetType() ?? type : type;
        }

        static bool IsRelationshipTable(ColumnInfo col) =>
            col?.Table != null && SonnetDBModel.IsTable(col.Table);

        static bool IsJsonColumn(ColumnInfo col) =>
            IsRelationshipTable(col) &&
            ((col?.Attribute?.DbType?.IndexOf("JSON", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
             (col?.DbTypeText?.IndexOf("JSON", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0);

        static bool IsJsonValue(object value) =>
            value is JsonDocument || value is JsonElement || value is JsonNode;

        static bool IsJsonType(Type type)
        {
            type = type?.NullableTypeOrThis();
            return type == typeof(JsonDocument) || type == typeof(JsonElement) ||
                (type != null && typeof(JsonNode).IsAssignableFrom(type));
        }

        /// <summary>判断列定义是否明确使用 SonnetDB VECTOR 类型。</summary>
        internal static bool IsVectorColumn(ColumnInfo col)
        {
            if (col == null) return false;
            return (col.Attribute?.DbType?.IndexOf("VECTOR", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                || (col.DbTypeText?.IndexOf("VECTOR", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
        }

        /// <summary>
        /// 判断表规则是否为 SonnetDB 已知的表值函数调用。
        /// 表值函数必须包在派生查询中，才能让 FreeSql 的列别名与 SonnetDB
        /// 3.1 的 FROM 语法同时成立。
        /// </summary>
        internal static bool IsTableValuedFunctionExpression(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return Regex.IsMatch(value.Trim(),
                @"^(forecast|knn|json_each|json_table|vector_search|hybrid_search)\s*\(",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        /// <summary>
        /// 将 FreeSql 支持的向量容器复制为 SonnetDB 使用的单精度数组。
        /// 只把明确的浮点向量识别为 VECTOR，避免把普通 IN 数组误当成向量。
        /// </summary>
        internal static bool TryGetVectorValue(object value, out float[] vector)
        {
            switch (value)
            {
                case float[] array:
                    vector = array;
                    return true;
                case ReadOnlyMemory<float> memory:
                    vector = memory.ToArray();
                    return true;
                case Memory<float> memory:
                    vector = memory.ToArray();
                    return true;
                case IReadOnlyList<float> list:
                    vector = list.ToArray();
                    return true;
                default:
                    vector = null;
                    return false;
            }
        }

        /// <summary>生成 SonnetDB 原生向量字面量（例如 <c>[1, 2.5, -3]</c>）。</summary>
        internal static string FormatVectorLiteral(float[] vector)
        {
            if (vector == null || vector.Length == 0)
                throw new NotSupportedException("SonnetDB VECTOR 不接受空向量。");

            var values = new string[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                if (float.IsFinite(vector[i]) == false)
                    throw new NotSupportedException("SonnetDB VECTOR 只接受有限的数值分量。");
                values[i] = vector[i].ToString("R", CultureInfo.InvariantCulture);
            }
            return $"[{string.Join(", ", values)}]";
        }

        static NotSupportedException UnsupportedVectorParameter(Type type) => new NotSupportedException(
            $"SonnetDB 3.1.0 的 ADO.NET 参数绑定不支持 VECTOR 参数（{type?.FullName ?? "未知类型"}）；" +
            "请启用 UseNoneCommandParameter(true) 生成原生 [v1, v2] 字面量，或升级到提供原生向量参数协议的 SonnetDB 版本。");

        static NotSupportedException UnsupportedVectorValue(ColumnInfo col, object value) => new NotSupportedException(
            $"SonnetDB VECTOR 列 '{col?.Attribute?.Name ?? "未知列"}' 需要 float[]/ReadOnlyMemory<float> 值，" +
            $"实际类型为 '{value?.GetType().FullName ?? "NULL"}'。");

        static string GetJsonText(object value, bool serializeObject)
        {
            switch (value)
            {
                case string text:
                    return text;
                case JsonDocument document:
                    return document.RootElement.GetRawText();
                case JsonElement element:
                    // 未定义值没有可读取的原始文本；按 JSON DOM 的序列化约定写入 null。
                    return element.ValueKind == JsonValueKind.Undefined ? "null" : element.GetRawText();
                case JsonNode node:
                    return node.ToJsonString();
                default:
                    if (serializeObject) return JsonSerializer.Serialize(value);
                    return null;
            }
        }

        static DateTime NormalizeUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Unspecified) return DateTime.SpecifyKind(value, DateTimeKind.Utc);
            return value.ToUniversalTime();
        }

        static long ToUnixTimeMilliseconds(DateTime value) =>
            new DateTimeOffset(NormalizeUtc(value)).ToUnixTimeMilliseconds();

        /// <summary>
        /// SonnetDB 的时序测量将时间列返回为 Unix 毫秒整数；关系表 DATETIME
        /// 可能返回原生时间对象。按目标 CLR 类型在提供程序范围内统一还原，
        /// 不改变其他数据库提供程序的读取语义。
        /// </summary>
        public override object ConvertDataReaderValue(Type type, object value)
        {
            if (value != null && value != DBNull.Value)
            {
                var targetType = type?.NullableTypeOrThis();
                if (targetType == typeof(DateTimeOffset))
                    return ConvertDateTimeOffsetValue(value);
                if (targetType == typeof(DateTime))
                    return ConvertDateTimeValue(value);
            }
            return base.ConvertDataReaderValue(type, value);
        }

        internal static DateTimeOffset ConvertDateTimeOffsetValue(object value)
        {
            if (value == null || value == DBNull.Value) return default;
            if (value is DateTimeOffset dateTimeOffset) return dateTimeOffset;
            if (value is DateTime dateTime)
            {
                if (dateTime.Kind == DateTimeKind.Unspecified)
                    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                return new DateTimeOffset(dateTime);
            }
            if (TryGetUnixMilliseconds(value, out var milliseconds))
                return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);

            var text = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal |
                DateTimeStyles.AdjustToUniversal, out var parsed))
                return parsed;
            throw InvalidDateTimeValue(value, nameof(DateTimeOffset));
        }

        internal static DateTime ConvertDateTimeValue(object value)
        {
            if (value == null || value == DBNull.Value) return default;
            if (value is DateTime dateTime) return dateTime;
            if (value is DateTimeOffset dateTimeOffset) return dateTimeOffset.UtcDateTime;
            if (TryGetUnixMilliseconds(value, out var milliseconds))
                return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;

            var text = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (DateTime.TryParse(text, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal |
                DateTimeStyles.AdjustToUniversal, out var parsed))
                return parsed;
            throw InvalidDateTimeValue(value, nameof(DateTime));
        }

        static bool TryGetUnixMilliseconds(object value, out long milliseconds)
        {
            milliseconds = 0;
            switch (value)
            {
                case byte number: milliseconds = number; return true;
                case sbyte number: milliseconds = number; return true;
                case short number: milliseconds = number; return true;
                case ushort number: milliseconds = number; return true;
                case int number: milliseconds = number; return true;
                case uint number: milliseconds = number; return true;
                case long number: milliseconds = number; return true;
                case ulong number when number <= long.MaxValue: milliseconds = (long)number; return true;
                case decimal number when number >= long.MinValue && number <= long.MaxValue:
                    milliseconds = decimal.ToInt64(number); return true;
                case double number when number >= long.MinValue && number <= long.MaxValue:
                    milliseconds = checked((long)number); return true;
                case float number when number >= long.MinValue && number <= long.MaxValue:
                    milliseconds = checked((long)number); return true;
                default:
                    return long.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture),
                        NumberStyles.Integer, CultureInfo.InvariantCulture, out milliseconds);
            }
        }

        static InvalidCastException InvalidDateTimeValue(object value, string targetType) =>
            new InvalidCastException(
                $"SonnetDB 返回的时间值 '{value?.GetType().FullName ?? "NULL"}' 无法转换为 {targetType}。");

        object GetParameterValue(ColumnInfo col, Type type, object value)
        {
            if (value == null || value == DBNull.Value) return value;

            var valueType = GetValueType(type, value);
            if (IsBigInteger(valueType) || value is BigInteger)
                throw UnsupportedBigInteger(valueType ?? value.GetType());
            // 原生 ADO 参数没有列元数据，float[] 只能被驱动解释为 VECTOR；
            // 驱动 3.1 不支持这种参数绑定，必须在发送前给出兼容性提示。
            // 普通 IN 条件的数组会在表达式格式化阶段展开，不会进入此路径。
            if (TryGetVectorValue(value, out _) && (col == null || IsVectorColumn(col)))
                throw UnsupportedVectorParameter(valueType ?? value.GetType());
            if (IsVectorColumn(col) && value is Array)
                throw UnsupportedVectorValue(col, value);
            if (value is byte[])
                return value;
            if (IsJsonColumn(col) || IsJsonType(type) || IsJsonValue(value))
                return GetJsonText(value, IsJsonColumn(col));

            // 时序测量的 time 使用 Unix 毫秒整数；关系表的 DATETIME
            // 保留时间类型，由 ADO 驱动按原生时间参数绑定。
            if (IsRelationshipTable(col) == false)
            {
                if (value is DateTime dateTime) return ToUnixTimeMilliseconds(dateTime);
                if (value is DateTimeOffset dateTimeOffset) return dateTimeOffset.ToUnixTimeMilliseconds();
            }

            return GetParamterValue(valueType, value);
        }

        DbType? GetParameterDbType(ColumnInfo col, Type type, object value)
        {
            if (value is SndbGeoPoint) return DbType.Object;
            if (value is byte[]) return DbType.Binary;
            if (IsJsonColumn(col) || IsJsonType(type) || IsJsonValue(value)) return DbType.String;
            // 测量的 DateTime/DateTimeOffset 已在 GetParameterValue 中转换为 Unix 毫秒。
            // 参数 DbType 也必须同步为 Int64，避免驱动按日期类型再次解释整数。
            var temporalType = GetValueType(type, value);
            if (IsRelationshipTable(col) == false &&
                (temporalType == typeof(DateTime) || temporalType == typeof(DateTimeOffset)))
                return DbType.Int64;
            if (IsRelationshipTable(col))
            {
                if (temporalType == typeof(DateTime)) return DbType.DateTime;
                if (temporalType == typeof(DateTimeOffset)) return DbType.DateTimeOffset;
            }

            var valueType = GetValueType(type, value);
            return valueType == null ? null : (DbType?)_orm.CodeFirst.GetDbInfo(valueType)?.type;
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            value = GetParameterValue(col, type, value);
            var ret = new SndbParameter { ParameterName = QuoteParamterName(parameterName), Value = value ?? DBNull.Value };
            var dbType = GetParameterDbType(col, type, value);
            if (dbType != null) ret.DbType = dbType.Value;
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<SndbParameter>(sql, obj, "@", (name, type, value) =>
            {
                value = GetParameterValue(null, type, value);
                var ret = new SndbParameter { ParameterName = $"@{name}", Value = value ?? DBNull.Value };
                var dbType = GetParameterDbType(null, type, value);
                if (dbType != null) ret.DbType = dbType.Value;
                return ret;
            });

        public override string FormatSql(string sql, params object[] args) => sql?.FormatSonnetDB(args);

        /// <summary>
        /// 将标识符包裹为 SonnetDB 引用格式（双引号）。
        /// <para>特殊规则：<c>time</c> 列是 SonnetDB 内置关键字，不得加引号，直接返回裸字符串 <c>time</c>。</para>
        /// <para>已引用或括号表达式直接透传，不再二次包裹。</para>
        /// </summary>
        public override string QuoteSqlNameAdapter(params string[] name)
        {
            if (name.Length == 1)
            {
                var nametrim = name[0].Trim();
                if (nametrim.StartsWith("(") && nametrim.EndsWith(")")) return nametrim;
                if (nametrim.StartsWith("\"") && nametrim.EndsWith("\"")) return nametrim;
                if (IsTableValuedFunctionExpression(nametrim))
                    return $"(SELECT * FROM {nametrim})";
                // time 是 SonnetDB 内置时间列关键字，不能用双引号包裹。
                if (string.Equals(nametrim, "time", StringComparison.OrdinalIgnoreCase)) return "time";
                return $"\"{nametrim.Replace("\"", "\"\"").Replace(".", "\".\"")}\"";
            }
            return $"\"{string.Join("\".\"", name.Select(a => a.Replace("\"", "\"\"")))}\"";
        }

        public override string TrimQuoteSqlName(string name)
        {
            var nametrim = name.Trim();
            if (nametrim.StartsWith("(") && nametrim.EndsWith(")")) return nametrim;
            return $"{nametrim.Trim('"').Replace("\".\"", ".").Replace(".\"", ".").Replace("\"\"", "\"")}";
        }

        public override string[] SplitTableName(string name) => GetSplitTableNames(name, '"', '"', 2);
        public override string QuoteParamterName(string name) => $"@{name}";
        /// <summary>空值替换，生成 <c>coalesce(sql, value)</c>。SonnetDB 不支持 ISNULL/IFNULL，统一用 coalesce。</summary>
        public override string IsNull(string sql, object value) => $"coalesce({sql}, {value})";
        /// <summary>字符串拼接，生成 <c>concat(a, b, ...)</c>。</summary>
        public override string StringConcat(string[] objs, Type[] types) => $"concat({string.Join(", ", objs)})";
        // SonnetDB 3.1 的 SQL 词法器没有位与、位或、异或和移位运算符。
        // 不能沿用 CommonUtils 的默认符号输出，否则会把不可解析的 SQL
        // 发送到数据库；在表达式翻译阶段给出明确的中文提示。
        public override string BitAnd(string left, string right) => throw UnsupportedBitwise("&");
        public override string BitOr(string left, string right) => throw UnsupportedBitwise("|");
        public override string BitShiftLeft(string left, string right) => throw UnsupportedBitwise("<<");
        public override string BitShiftRight(string left, string right) => throw UnsupportedBitwise(">>");
        public override string BitNot(string value) => throw UnsupportedBitwise("~");
        public override string BitXor(string left, string right) => throw UnsupportedBitwise("^");

        static NotSupportedException UnsupportedBitwise(string operation) => new NotSupportedException(
            $"SonnetDB 3.1 不支持位运算符 {operation}；FreeSql 当前表达式无法翻译，请改为数据库支持的函数或在应用层计算。");

        public override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";
        public override string Div(string left, string right, Type leftType, Type rightType) => $"{left} / {right}";
        /// <summary>当前本地时间，输出为 Unix 毫秒整数字符串（与 SonnetDB time 列存储格式一致）。</summary>
        public override string Now => DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
        /// <summary>当前 UTC 时间，输出为 Unix 毫秒整数字符串。</summary>
        public override string NowUtc => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
        public override string QuoteWriteParamterAdapter(Type type, string paramterName) => paramterName;
        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName) => columnName;

        /// <summary>
        /// 将值转换为非参数化 SQL 字面量。
        /// <list type="bullet">
        ///   <item>日期时间 → Unix 毫秒整数，兼容时序测量与关系表 DATETIME 比较</item>
        ///   <item><c>byte[]</c> → Base64 字符串字面量（SonnetDB BLOB 格式）</item>
        ///   <item>JSON DOM/Node 或 JSON 列对象 → JSON 文本字符串字面量</item>
        ///   <item>数值类型 → 不变字符串（使用 InvariantCulture 避免区域格式问题）</item>
        ///   <item>数组 → <c>(v1, v2, ...)</c> 或 <c>(NULL)</c>（空数组）</item>
        ///   <item>其他 → 通过 FormatSql 转义字符串</item>
        /// </list>
        /// </summary>
        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value)
        {
            if (value == null || value == DBNull.Value) return "NULL";
            var valueType = GetValueType(type, value);

            if (IsBigInteger(valueType) || value is BigInteger)
                throw UnsupportedBigInteger(valueType ?? value.GetType());

            if (TryGetVectorValue(value, out var vector))
            {
                if (col == null || IsVectorColumn(col))
                    return FormatVectorLiteral(vector);
            }
            else if (IsVectorColumn(col))
            {
                throw UnsupportedVectorValue(col, value);
            }

            if (value is SndbGeoPoint geoPoint)
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "POINT({0:R}, {1:R})",
                    geoPoint.Lat,
                    geoPoint.Lon);

            // byte[] 是单个 BLOB 值，不能按 SQL 值列表处理。
            if (value is byte[] bytes)
                return FormatSql("{0}", bytes);

            if (IsJsonValue(value) ||
                (IsJsonColumn(col) && !(value is Array)))
                return FormatSql("{0}", GetJsonText(value, IsJsonColumn(col)));

            if (value is DateTime dateTime)
                return ToUnixTimeMilliseconds(dateTime).ToString(CultureInfo.InvariantCulture);
            if (value is DateTimeOffset dateTimeOffset)
                return dateTimeOffset.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
            if (value is ulong unsignedValue)
                return ConvertUInt64ToInt64(unsignedValue).ToString(CultureInfo.InvariantCulture);
            if (valueType != null && valueType.IsNumberType()) return string.Format(CultureInfo.InvariantCulture, "{0}", value);
            value = GetParamterValue(valueType, value);
            if (value is Array)
            {
                var arr = value as Array;
                var sb = new StringBuilder("(");
                for (var a = 0; a < arr.Length; a++)
                {
                    if (a > 0) sb.Append(",");
                    var item = arr.GetValue(a);
                    sb.Append(GetNoneParamaterSqlValue(specialParams, specialParamFlag, col, item?.GetType(), item));
                }
                if (arr.Length == 0) sb.Append("NULL");
                return sb.Append(")").ToString();
            }
            return FormatSql("{0}", value);
        }
    }
}
