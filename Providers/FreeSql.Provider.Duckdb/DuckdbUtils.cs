using DuckDB.NET.Data;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Numerics;
using static DuckDB.NET.Native.NativeMethods;
using System.Text;
using System.Security.AccessControl;
using ColumnInfo = FreeSql.Internal.Model.ColumnInfo;

namespace FreeSql.Duckdb
{

    class DuckdbUtils : CommonUtils
    {
        public DuckdbUtils(IFreeSql orm) : base(orm)
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
            { typeof(BigInteger).FullName, a => BigInteger.Parse(string.Concat(a), System.Globalization.NumberStyles.Any) }, { typeof(BigInteger[]).FullName, a => getParamterArrayValue(typeof(BigInteger), a, 0) }, { typeof(BigInteger?[]).FullName, a => getParamterArrayValue(typeof(BigInteger?), a, null) },
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
            var ret = new DuckDBParameter { ParameterName = QuoteParamterName(parameterName), Value = value };
            //var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
            //if (tp != null) ret.DbType = (DbType)tp.Value;
            //if (col != null)
            //{
            //    var dbtype = (DbType)_orm.DbFirst.GetDbType(new DatabaseModel.DbColumnInfo { DbTypeText = col.DbTypeText });
            //    if (dbtype != 0)
            //    {
            //        ret.DbType = dbtype;
            //        //if (col.DbSize != 0) ret.Size = col.DbSize;
            //        if (col.DbPrecision != 0) ret.Precision = col.DbPrecision;
            //        if (col.DbScale != 0) ret.Scale = col.DbScale;
            //    }
            //}
            _params?.Add(ret);
            return ret;
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            Utils.GetDbParamtersByObject<DuckDBParameter>(sql, obj, "$", (name, type, value) =>
            {
                if (value != null) value = getParamterValue(type, value);
                var ret = new DuckDBParameter { ParameterName = name, Value = value };
                //if (value.GetType().IsEnum || value.GetType().GenericTypeArguments.FirstOrDefault()?.IsEnum == true) {
                //	ret.DataTypeName = "";
                //} else {
                //var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
                //if (tp != null) ret.DbType = (DbType)tp.Value; DuckDBParameter DbType 未对齐
                //}
                return ret;
            });

        public override string FormatSql(string sql, params object[] args) => sql?.FormatDuckdb(args);
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
        public override string TrimQuoteSqlName(string name)
        {
            var nametrim = name.Trim();
            if (nametrim.StartsWith("(") && nametrim.EndsWith(")"))
                return nametrim; //原生SQL
            return $"{nametrim.Trim('"').Replace("\".\"", ".").Replace(".\"", ".")}";
        }
        public override string[] SplitTableName(string name) => GetSplitTableNames(name, '"', '"', 2);
        public override string QuoteParamterName(string name) => $"${name}";
        public override string IsNull(string sql, object value) => $"coalesce({sql}, {value})";
        public override string StringConcat(string[] objs, Type[] types) => $"{string.Join(" || ", objs)}";
        public override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";
        public override string Div(string left, string right, Type leftType, Type rightType) => $"{left} / {right}";
        public override string Now => "current_timestamp";
        public override string NowUtc => "current_timestamp";

        public override string QuoteWriteParamterAdapter(Type type, string paramterName) => paramterName;
        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName) => columnName;

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value)
        {
            if (value == null) return "NULL";
            if (type.IsNumberType()) return string.Format(CultureInfo.InvariantCulture, "{0}", value);
            var type2 = type;
            if (type2 == typeof(byte[])) return FormatSql("{0}", value, 1);
            //array
            if (value is Array)
            {
                var valueArr = value as Array;
                var eleType = type2.GetElementType();
                var eleTypeNullable = eleType.NullableTypeOrThis();
                var len = valueArr.GetLength(0);
                var sb = new StringBuilder().Append("[");
                for (var a = 0; a < len; a++)
                {
                    var item = valueArr.GetValue(a);
                    if (eleType != eleTypeNullable) item = Utils.GetDataReaderValue(eleTypeNullable, item);
                    if (a > 0) sb.Append(",");
                    sb.Append(GetNoneParamaterSqlValue(specialParams, specialParamFlag, col, eleType, item));
                }
                sb.Append("]");
                //var dbinfo = _orm.CodeFirst.GetDbInfo(type);
                //if (dbinfo != null) sb.Append("::").Append(dbinfo.dbtype);
                return sb.ToString();
            }
            if (type2.IsGenericType)
            {
                var typeDefinition = type2.GetGenericTypeDefinition();
                //list
                if (typeDefinition == typeof(List<>))
                {
                    var valueArr = value as IList;
                    var eleType = type2.GenericTypeArguments.FirstOrDefault().NullableTypeOrThis();
                    var eleTypeNullable = eleType.NullableTypeOrThis();
                    var len = valueArr.Count;
                    var sb = new StringBuilder().Append("[");
                    for (var a = 0; a < len; a++)
                    {
                        var item = valueArr[a];
                        if (eleType != eleTypeNullable) item = Utils.GetDataReaderValue(eleTypeNullable, item);
                        if (a > 0) sb.Append(",");
                        sb.Append(GetNoneParamaterSqlValue(specialParams, specialParamFlag, col, eleType, item));
                    }
                    sb.Append("]");
                    //var dbinfo = _orm.CodeFirst.GetDbInfo(type);
                    //if (dbinfo != null) sb.Append("::").Append(dbinfo.dbtype);
                    return sb.ToString();
                }
                //struct
                //if (typeDefinition == typeof(Dictionary<string, object>))
                //{
                //    var dict = value as Dictionary<string, object>;
                //    if (dict.Count == 0) return "NULL";
                //    var sb = new StringBuilder("{");
                //    var idx = 0;
                //    foreach (var key in dict.Keys)
                //    {
                //        var val = dict[key];
                //        if (val == null) continue;
                //        if (idx > 0) sb.Append(",");
                //        sb.Append("'").Append(FormatSql("{0}", val, 1)).Append("':");
                //        sb.Append(GetNoneParamaterSqlValue(specialParams, specialParamFlag, col, val.GetType(), val));
                //        idx++;
                //    }
                //    return sb.Append("}").ToString();
                //}
                //map
                if (typeDefinition == typeof(Dictionary<,>))
                {
                    var dict = value as IDictionary;
                    var sb = new StringBuilder("map([");
                    var idx = 0;
                    Type tkey = null;
                    foreach (var key in dict.Keys)
                    {
                        if (tkey == null) tkey = key.GetType();
                        if (idx > 0) sb.Append(",");
                        sb.Append(GetNoneParamaterSqlValue(specialParams, specialParamFlag, col, tkey, key));
                        idx++;
                    }
                    sb.Append("],[");
                    idx = 0;
                    tkey = null;
                    foreach (var val in dict.Values)
                    {
                        if (val == null) continue;
                        if (tkey == null) tkey = val.GetType();
                        if (idx > 0) sb.Append(",");
                        sb.Append(GetNoneParamaterSqlValue(specialParams, specialParamFlag, col, tkey, val));
                        idx++;
                    }
                    return sb.Append("])").ToString();
                }
            }
            if (type2 == typeof(BitArray))
            {
                var ba = value as BitArray;
                char[] ba1010 = new char[ba.Length];
                for (int a = 0; a < ba.Length; a++) ba1010[a] = ba[a] ? '1' : '0';
                return $"bit '{new string(ba1010)}'";
            }
            return FormatSql("{0}", value, 1);
        }
    }
}
