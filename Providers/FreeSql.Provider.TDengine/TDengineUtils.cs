using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Numerics;
using TDengine.Data.Client;
using TDengine.Driver;

namespace FreeSql.TDengine
{
    internal class TDengineUtils : CommonUtils
    {
        public TDengineUtils(IFreeSql orm) : base(orm)
        {
        }

        public override string Now => "now()";

        public override string NowUtc => throw new NotImplementedException();

        public override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col,
            Type type, object value)
        {
            throw new NotImplementedException();
        }

        public override string Div(string left, string right, Type leftType, Type rightType)
        {
            throw new NotImplementedException();
        }

        public override string FormatSql(string sql, params object[] args)
        {
            throw new NotImplementedException();
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

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj)
            =>
                Utils.GetDbParamtersByObject<DbParameter>(sql, obj, "@", (name, type, value) =>
                {
                    if (value != null) value = getParamterValue(type, value);
                    var ret = new TDengineParameter { ParameterName = $"@{name}", Value = value };
                    //if (value.GetType().IsEnum || value.GetType().GenericTypeArguments.FirstOrDefault()?.IsEnum == true) {
                    //	ret.DataTypeName = "";
                    //} else {
                    var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
                    if (tp != null) ret.DbType = (DbType)tp.Value;
                    //}
                    return ret;
                });

        public override string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag,
            ColumnInfo col, Type type, object value)
        {
            throw new NotImplementedException();
        }

        public override string IsNull(string sql, object value)
        {
            throw new NotImplementedException();
        }

        public override string Mod(string left, string right, Type leftType, Type rightType)
        {
            throw new NotImplementedException();
        }

        public override string QuoteParamterName(string name) => $"@{name}";

        public override string QuoteSqlNameAdapter(params string[] name)
        {
            throw new NotImplementedException();
        }

        public override string QuoteWriteParamterAdapter(Type type, string paramterName)
        {
            throw new NotImplementedException();
        }

        public override string[] SplitTableName(string name)
        {
            throw new NotImplementedException();
        }

        public override string StringConcat(string[] objs, Type[] types)
        {
            throw new NotImplementedException();
        }

        public override string TrimQuoteSqlName(string name)
        {
            throw new NotImplementedException();
        }

        protected override string QuoteReadColumnAdapter(Type type, Type mapType, string columnName)
        {
            throw new NotImplementedException();
        }
    }
}