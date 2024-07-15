﻿using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using XuguClient;

namespace FreeSql.Xugu
{

    class XuguUtils : CommonUtils
    {
        public XuguUtils(IFreeSql orm) : base(orm)
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
        public override string QuoteSqlNameAdapter(params string[] name)
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
        static Dictionary<string, Func<object, object>> dicGetParamterValue = new Dictionary<string, Func<object, object>> {
            //{ typeof(JToken).FullName, a => string.Concat(a) }, { typeof(JToken[]).FullName, a => getParamterArrayValue(typeof(string), a, null) },
            //{ typeof(JObject).FullName, a => string.Concat(a) }, { typeof(JObject[]).FullName, a => getParamterArrayValue(typeof(string), a, null) },
            //{ typeof(JArray).FullName, a => string.Concat(a) }, { typeof(JArray[]).FullName, a => getParamterArrayValue(typeof(string), a, null) },
            { typeof(uint).FullName, a => long.Parse(string.Concat(a)) }, { typeof(uint[]).FullName, a => getParamterArrayValue(typeof(long), a, 0) }, { typeof(uint?[]).FullName, a => getParamterArrayValue(typeof(long?), a, null) },
            { typeof(ulong).FullName, a => decimal.Parse(string.Concat(a)) }, { typeof(ulong[]).FullName, a => getParamterArrayValue(typeof(decimal), a, 0) }, { typeof(ulong?[]).FullName, a => getParamterArrayValue(typeof(decimal?), a, null) },
            { typeof(ushort).FullName, a => int.Parse(string.Concat(a)) }, { typeof(ushort[]).FullName, a => getParamterArrayValue(typeof(int), a, 0) }, { typeof(ushort?[]).FullName, a => getParamterArrayValue(typeof(int?), a, null) },
            { typeof(byte).FullName, a => short.Parse(string.Concat(a)) }, { typeof(byte[]).FullName, a => getParamterArrayValue(typeof(short), a, 0) }, { typeof(byte?[]).FullName, a => getParamterArrayValue(typeof(short?), a, null) },
            { typeof(sbyte).FullName, a => short.Parse(string.Concat(a)) }, { typeof(sbyte[]).FullName, a => getParamterArrayValue(typeof(short), a, 0) }, { typeof(sbyte?[]).FullName, a => getParamterArrayValue(typeof(short?), a, null) },
            { typeof(char).FullName, a => string.Concat(a).Replace('\0', ' ').ToCharArray().FirstOrDefault() },
            { typeof(BigInteger).FullName, a => BigInteger.Parse(string.Concat(a), System.Globalization.NumberStyles.Any) }, { typeof(BigInteger[]).FullName, a => getParamterArrayValue(typeof(BigInteger), a, 0) }, { typeof(BigInteger?[]).FullName, a => getParamterArrayValue(typeof(BigInteger?), a, null) },


            { typeof((IPAddress Address, int Subnet)).FullName, a => {
                var inet = ((IPAddress Address, int Subnet))a;
                if (inet.Address == null) return (IPAddress.Any, inet.Subnet);
                return inet;
            } },
            { typeof((IPAddress Address, int Subnet)[]).FullName, a => getParamterArrayValue(typeof((IPAddress Address, int Subnet)), a, (IPAddress.Any, 0)) },
            { typeof((IPAddress Address, int Subnet)?[]).FullName, a => getParamterArrayValue(typeof((IPAddress Address, int Subnet)?), a, null) },
        };
        static object getParamterValue(Type type, object value, int level = 0)
        {
            if (type.FullName == "System.Byte[]") return value;
            if (type.FullName == "System.Char[]") return value;
            if (type.IsArray && level == 0)
            {
                var elementType = type.GetElementType();
                Type enumType = null;
                if (elementType.IsEnum) enumType = elementType;
                else if (elementType.IsNullableType() && elementType.GenericTypeArguments.First().IsEnum) enumType = elementType.GenericTypeArguments.First();
                if (enumType != null) return enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any() ?
                    getParamterArrayValue(typeof(long), value, elementType.IsEnum ? null : enumType.CreateInstanceGetDefaultValue()) :
                    getParamterArrayValue(typeof(int), value, elementType.IsEnum ? null : enumType.CreateInstanceGetDefaultValue());
                return dicGetParamterValue.TryGetValue(type.FullName, out var trydicarr) ? trydicarr(value) : value;
            }
            if (type.IsNullableType()) type = type.GenericTypeArguments.First();
            if (type.IsEnum) return (int)value;
            if (dicGetParamterValue.TryGetValue(type.FullName, out var trydic)) return trydic(value);
            return value;
        }

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
            if (value != null) value = getParamterValue(type, value);
            var ret = new XGParameters { ParameterName = parameterName, Value = value };

            var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
            if (col != null)
            {
                var dbtype = (XGDbType?)_orm.DbFirst.GetDbType(new DatabaseModel.DbColumnInfo { DbTypeText = col.DbTypeText });
                if (dbtype != null)
                {
                    if (col.DbPrecision != 0) ret.Precision = col.DbPrecision;
                    if (col.DbScale != 0) ret.Scale = col.DbScale;
                }
            }
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<XGParameters>(sql, obj, ":", (name, type, value) =>
            {
                if (value != null) value = getParamterValue(type, value);
                var ret = new XGParameters { ParameterName = name, Value = value };

                var tp = _orm.CodeFirst.GetDbInfo(type)?.type;

                return ret;
            });

        public override string FormatSql(string sql, params object[] args) => sql?.FormatXuguSQL(args);

        public override string TrimQuoteSqlName(string name)
        {
            var nametrim = name.Trim();
            if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                return nametrim; //原生SQL
            return $"{nametrim.Trim('"').Replace("\".\"", ".").Replace(".\"", ".")}";
        }
        public override string[] SplitTableName(string name) => GetSplitTableNames(name, '"', '"', 2);
        public override string QuoteParamterName(string name) => $":{name}";
        public override string IsNull(string sql, object value) => $"coalesce({sql}, {value})";
        public override string StringConcat(string[] objs, Type[] types) => $"{string.Join(" || ", objs)}";
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";
        public override string Div(string left, string right, Type leftType, Type rightType) => $"{left} / {right}";
        public override string Now => "current_timestamp";
        public override string NowUtc => "(current_timestamp at time zone 'UTC')";

        public override string QuoteWriteParamterAdapter(Type type, string paramterName) => paramterName;
        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName) => columnName;

        static ConcurrentDictionary<Type, bool> _dicIsAssignableFromPostgisGeometry = Utils.GlobalCacheFactory.CreateCacheItem<ConcurrentDictionary<Type, bool>>();
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
            else if (dicGetParamterValue.ContainsKey(type2.FullName))
            {
                value = string.Concat(value);
            }
            return FormatSql("{0}", value, 1);
        }
    }
}
