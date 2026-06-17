// SonnetDBUtils.cs
// SonnetDB SQL 工具函数集。
//
// 主要职责：
//   - 参数化 SQL 绑定（AppendParamter / GetDbParamtersByObject）
//   - 标识符引用（QuoteSqlNameAdapter）：time 列不加双引号（SonnetDB 关键字）
//   - 空值替换：IsNull → coalesce(sql, value)
//   - 字符串拼接：StringConcat → concat(...)
//   - 当前时间：Now / NowUtc → Unix 毫秒整数字符串
//   - 字面量输出（GetNoneParamaterSqlValue）：DateTime/DateTimeOffset → Unix 毫秒，数组 → (v1,v2,...)

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
using SndbParameter = global::SonnetDB.Data.SndbParameter;

namespace FreeSql.SonnetDB
{
    class SonnetDBUtils : CommonUtils
    {
        public SonnetDBUtils(IFreeSql orm) : base(orm) { }

        /// <summary>
        /// 特殊 C# 类型 → 数据库兼容值的转换函数表。
        /// 无符号整型、char、BigInteger 等需要在绑定参数前转换为 SonnetDB 驱动接受的类型。
        /// </summary>
        static readonly Dictionary<string, Func<object, object>> _dicGetParamterValue = new Dictionary<string, Func<object, object>>
        {
            { typeof(uint).FullName, a => long.Parse(string.Concat(a), CultureInfo.InvariantCulture) },
            { typeof(ulong).FullName, a => decimal.Parse(string.Concat(a), CultureInfo.InvariantCulture) },
            { typeof(ushort).FullName, a => int.Parse(string.Concat(a), CultureInfo.InvariantCulture) },
            { typeof(byte).FullName, a => short.Parse(string.Concat(a), CultureInfo.InvariantCulture) },
            { typeof(sbyte).FullName, a => short.Parse(string.Concat(a), CultureInfo.InvariantCulture) },
            { typeof(char).FullName, a => string.Concat(a).Replace('\0', ' ').ToCharArray().FirstOrDefault() },
            { typeof(BigInteger).FullName, a => BigInteger.Parse(string.Concat(a), NumberStyles.Any, CultureInfo.InvariantCulture) },
        };

        static object GetParamterValue(Type type, object value)
        {
            if (type == null || value == null) return value;
            if (type.IsNullableType()) type = type.GenericTypeArguments.First();
            if (type.IsEnum) return Convert.ToInt64(value, CultureInfo.InvariantCulture);
            if (_dicGetParamterValue.TryGetValue(type.FullName, out var trydic)) return trydic(value);
            return value;
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            if (value != null) value = GetParamterValue(type, value);
            var ret = new SndbParameter { ParameterName = QuoteParamterName(parameterName), Value = value ?? DBNull.Value };
            var dbType = _orm.CodeFirst.GetDbInfo(type)?.type;
            if (dbType != null) ret.DbType = (DbType)dbType.Value;
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<SndbParameter>(sql, obj, "@", (name, type, value) =>
            {
                if (value != null) value = GetParamterValue(type, value);
                var ret = new SndbParameter { ParameterName = $"@{name}", Value = value ?? DBNull.Value };
                var dbType = _orm.CodeFirst.GetDbInfo(type)?.type;
                if (dbType != null) ret.DbType = (DbType)dbType.Value;
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
        ///   <item>DateTime / DateTimeOffset → Unix 毫秒整数（SonnetDB time 列格式）</item>
        ///   <item>数值类型 → 不变字符串（使用 InvariantCulture 避免区域格式问题）</item>
        ///   <item>数组 → <c>(v1, v2, ...)</c> 或 <c>(NULL)</c>（空数组）</item>
        ///   <item>其他 → 通过 FormatSql 转义字符串</item>
        /// </list>
        /// </summary>
        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type != null && type.IsNullableType()) type = type.GenericTypeArguments.First();
            if (type == typeof(DateTime))
            {
                var dt = (DateTime)value;
                // Unspecified Kind 视为 UTC，避免时区歧义。
                if (dt.Kind == DateTimeKind.Unspecified) dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                return new DateTimeOffset(dt).ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
            }
            // DateTimeOffset 直接转 Unix 毫秒，保留时区偏移。
            if (type == typeof(DateTimeOffset)) return ((DateTimeOffset)value).ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
            if (type != null && type.IsNumberType()) return string.Format(CultureInfo.InvariantCulture, "{0}", value);
            value = GetParamterValue(type, value);
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
            return FormatSql("{0}", value, 1);
        }
    }
}
