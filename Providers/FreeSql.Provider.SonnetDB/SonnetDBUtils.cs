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

        public override string QuoteSqlNameAdapter(params string[] name)
        {
            if (name.Length == 1)
            {
                var nametrim = name[0].Trim();
                if (nametrim.StartsWith("(") && nametrim.EndsWith(")")) return nametrim;
                if (nametrim.StartsWith("\"") && nametrim.EndsWith("\"")) return nametrim;
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
        public override string IsNull(string sql, object value) => $"coalesce({sql}, {value})";
        public override string StringConcat(string[] objs, Type[] types) => $"concat({string.Join(", ", objs)})";
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";
        public override string Div(string left, string right, Type leftType, Type rightType) => $"{left} / {right}";
        public override string Now => DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
        public override string NowUtc => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
        public override string QuoteWriteParamterAdapter(Type type, string paramterName) => paramterName;
        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName) => columnName;

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type != null && type.IsNullableType()) type = type.GenericTypeArguments.First();
            if (type == typeof(DateTime))
            {
                var dt = (DateTime)value;
                if (dt.Kind == DateTimeKind.Unspecified) dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                return new DateTimeOffset(dt).ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
            }
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
