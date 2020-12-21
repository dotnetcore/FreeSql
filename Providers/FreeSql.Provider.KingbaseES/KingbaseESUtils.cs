using FreeSql.Internal;
using FreeSql.Internal.Model;
using Kdbndp;
using KdbndpTypes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FreeSql.KingbaseES
{

    class KingbaseESUtils : CommonUtils
    {
        public KingbaseESUtils(IFreeSql orm) : base(orm)
        {
        }

        static Array getParamterArrayValue(Type arrayType, object value, object defaultValue)
        {
            var valueArr = value as Array;
            var len = valueArr.GetLength(0);
            var ret = Array.CreateInstance(arrayType, len);
            for (var a = 0; a < len; a++)
            {
                var item = valueArr.GetValue(a);
                ret.SetValue(item == null ? defaultValue : getParamterValue(item.GetType(), item, 1), a);
            }
            return ret;
        }
        static Dictionary<string, Func<object, object>> dicGetParamterValue = new Dictionary<string, Func<object, object>> {
            { typeof(uint).FullName, a => long.Parse(string.Concat(a)) }, { typeof(uint[]).FullName, a => getParamterArrayValue(typeof(long), a, 0) }, { typeof(uint?[]).FullName, a => getParamterArrayValue(typeof(long?), a, null) },
            { typeof(ulong).FullName, a => decimal.Parse(string.Concat(a)) }, { typeof(ulong[]).FullName, a => getParamterArrayValue(typeof(decimal), a, 0) }, { typeof(ulong?[]).FullName, a => getParamterArrayValue(typeof(decimal?), a, null) },
            { typeof(ushort).FullName, a => int.Parse(string.Concat(a)) }, { typeof(ushort[]).FullName, a => getParamterArrayValue(typeof(int), a, 0) }, { typeof(ushort?[]).FullName, a => getParamterArrayValue(typeof(int?), a, null) },
            { typeof(byte).FullName, a => short.Parse(string.Concat(a)) }, { typeof(byte[]).FullName, a => getParamterArrayValue(typeof(short), a, 0) }, { typeof(byte?[]).FullName, a => getParamterArrayValue(typeof(short?), a, null) },
            { typeof(sbyte).FullName, a => short.Parse(string.Concat(a)) }, { typeof(sbyte[]).FullName, a => getParamterArrayValue(typeof(short), a, 0) }, { typeof(sbyte?[]).FullName, a => getParamterArrayValue(typeof(short?), a, null) },
            { typeof(char).FullName, a => string.Concat(a).Replace('\0', ' ').ToCharArray().FirstOrDefault() },
        };
        static object getParamterValue(Type type, object value, int level = 0)
        {
            if (type.FullName == "System.Byte[]") return value;
            if (type.IsArray && level == 0)
            {
                var elementType = type.GetElementType();
                Type enumType = null;
                if (elementType.IsEnum) enumType = elementType;
                else if (elementType.IsNullableType())
                {
                    var genericTypesFirst = elementType.GetGenericArguments().First();
                    if (genericTypesFirst.IsEnum) enumType = genericTypesFirst;
                }
                if (enumType != null) return enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any() ?
                    getParamterArrayValue(typeof(long), value, elementType.IsEnum ? null : enumType.CreateInstanceGetDefaultValue()) :
                    getParamterArrayValue(typeof(int), value, elementType.IsEnum ? null : enumType.CreateInstanceGetDefaultValue());
                return dicGetParamterValue.TryGetValue(type.FullName, out var trydicarr) ? trydicarr(value) : value;
            }
            if (type.IsNullableType()) type = type.GetGenericArguments().First();
            if (type.IsEnum) return (int)value;
            if (dicGetParamterValue.TryGetValue(type.FullName, out var trydic)) return trydic(value);
            return value;
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            if (value != null) value = getParamterValue(type, value);
            var ret = new KdbndpParameter { ParameterName = QuoteParamterName(parameterName), Value = value };
            //if (value.GetType().IsEnum || value.GetType().GenericTypeArguments.FirstOrDefault()?.IsEnum == true) {
            //	ret.DataTypeName = "";
            //} else {
            var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
            if (tp != null) ret.KdbndpDbType = (KdbndpDbType)tp.Value;
            //}
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<KdbndpParameter>(sql, obj, null, (name, type, value) =>
            {
                if (value != null) value = getParamterValue(type, value);
                var ret = new KdbndpParameter { ParameterName = $"@{name.ToUpper()}", Value = value };
                //if (value.GetType().IsEnum || value.GetType().GenericTypeArguments.FirstOrDefault()?.IsEnum == true) {
                //	ret.DataTypeName = "";
                //} else {
                var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
                if (tp != null) ret.KdbndpDbType = (KdbndpDbType)tp.Value;
                //}
                return ret;
            });

        public override string FormatSql(string sql, params object[] args) => sql?.FormatKingbaseES(args);
        public override string QuoteSqlName(params string[] name)
        {
            if (name.Length == 1)
            {
                var nametrim = name[0].Trim();
                if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                    return nametrim; //原生SQL
                if (nametrim.StartsWith("\"") && nametrim.EndsWith("\""))
                    return nametrim;
                return $"\"{nametrim.Replace(".", "\".\"")}\"";
            }
            return $"\"{string.Join("\".\"", name)}\"";
        }
        public override string TrimQuoteSqlName(string name)
        {
            var nametrim = name.Trim();
            if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                return nametrim; //原生SQL
            return $"{nametrim.Trim('"').Replace("\".\"", ".").Replace(".\"", ".")}";
        }
        public override string[] SplitTableName(string name) => GetSplitTableNames(name, '"', '"', 2);
        public override string QuoteParamterName(string name) => $"@{name.ToUpper()}";
        public override string IsNull(string sql, object value) => $"coalesce({sql}, {value})";
        public override string StringConcat(string[] objs, Type[] types) => $"{string.Join(" || ", objs.Select((a, b) => b == 0 ? $"{a}::text" : a))}"; //First ::text
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";
        public override string Div(string left, string right, Type leftType, Type rightType) => $"{left} / {right}";
        public override string Now => "current_timestamp";
        public override string NowUtc => "(current_timestamp at time zone 'UTC')";

        public override string QuoteWriteParamterAdapter(Type type, string paramterName) => paramterName;
        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName) => columnName;

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type.IsNumberType()) return string.Format(CultureInfo.InvariantCulture, "{0}", value);
            value = getParamterValue(type, value);
            var type2 = value.GetType();
            if (type2 == typeof(byte[])) return $"'\\x{CommonUtils.BytesSqlRaw(value as byte[])}'";
            if (type2 == typeof(TimeSpan) || type2 == typeof(TimeSpan?))
            {
                var ts = (TimeSpan)value;
                return $"'{Math.Min(24, (int)Math.Floor(ts.TotalHours))}:{ts.Minutes}:{ts.Seconds}'";
            }
            else if (value is Array)
            {
                var valueArr = value as Array;
                var eleType = type2.GetElementType();
                var len = valueArr.GetLength(0);
                var sb = new StringBuilder().Append("ARRAY[");
                for (var a = 0; a < len; a++)
                {
                    var item = valueArr.GetValue(a);
                    if (a > 0) sb.Append(",");
                    sb.Append(GetNoneParamaterSqlValue(specialParams, specialParamFlag, col, eleType, item));
                }
                sb.Append("]");
                var dbinfo = _orm.CodeFirst.GetDbInfo(type);
                if (dbinfo != null) sb.Append("::").Append(dbinfo.dbtype);
                return sb.ToString();
            }
            else if (dicGetParamterValue.ContainsKey(type2.FullName))
            {
                value = string.Concat(value);
            }
            return FormatSql("{0}", value, 1);
        }
    }
}
