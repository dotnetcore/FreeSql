using FreeSql.Internal;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Text;

namespace FreeSql.PostgreSQL {

	class PostgreSQLUtils : CommonUtils {
		public PostgreSQLUtils(IFreeSql orm) : base(orm) {
		}

		static Array getParamterArrayValue(Type arrayType, object value, object defaultValue) {
			var valueArr = value as Array;
			var len = valueArr.GetLength(0);
			var ret = Array.CreateInstance(arrayType, len);
			for (var a = 0; a < len; a++) {
				var item = valueArr.GetValue(a);
				ret.SetValue(item == null ? defaultValue : getParamterValue(item.GetType(), item, 1), a);
			}
			return ret;
		}
		static Dictionary<string, Func<object, object>> dicGetParamterValue = new Dictionary<string, Func<object, object>> {
			{ typeof(JToken).FullName, a => string.Concat(a) }, { typeof(JToken[]).FullName, a => getParamterArrayValue(typeof(string), a, null) },
			{ typeof(JObject).FullName, a => string.Concat(a) }, { typeof(JObject[]).FullName, a => getParamterArrayValue(typeof(string), a, null) },
			{ typeof(JArray).FullName, a => string.Concat(a) }, { typeof(JArray[]).FullName, a => getParamterArrayValue(typeof(string), a, null) },
			{ typeof(uint).FullName, a => long.Parse(string.Concat(a)) }, { typeof(uint[]).FullName, a => getParamterArrayValue(typeof(long), a, 0) }, { typeof(uint?[]).FullName, a => getParamterArrayValue(typeof(long?), a, null) },
			{ typeof(ulong).FullName, a => decimal.Parse(string.Concat(a)) }, { typeof(ulong[]).FullName, a => getParamterArrayValue(typeof(decimal), a, 0) }, { typeof(ulong?[]).FullName, a => getParamterArrayValue(typeof(decimal?), a, null) },
			{ typeof(ushort).FullName, a => int.Parse(string.Concat(a)) }, { typeof(ushort[]).FullName, a => getParamterArrayValue(typeof(int), a, 0) }, { typeof(ushort?[]).FullName, a => getParamterArrayValue(typeof(int?), a, null) },
			{ typeof(byte).FullName, a => short.Parse(string.Concat(a)) }, { typeof(byte[]).FullName, a => getParamterArrayValue(typeof(short), a, 0) }, { typeof(byte?[]).FullName, a => getParamterArrayValue(typeof(short?), a, null) },
			{ typeof(sbyte).FullName, a => short.Parse(string.Concat(a)) }, { typeof(sbyte[]).FullName, a => getParamterArrayValue(typeof(short), a, 0) }, { typeof(sbyte?[]).FullName, a => getParamterArrayValue(typeof(short?), a, null) },
			{ typeof(NpgsqlPath).FullName, a => {
				var path = (NpgsqlPath)a;
				try { int count = path.Count; return path; } catch { return new NpgsqlPath(new NpgsqlPoint(0, 0)); }
			} }, { typeof(NpgsqlPath[]).FullName, a => getParamterArrayValue(typeof(NpgsqlPath), a, new NpgsqlPath(new NpgsqlPoint(0, 0))) }, { typeof(NpgsqlPath?[]).FullName, a => getParamterArrayValue(typeof(NpgsqlPath?), a, null) },
			{ typeof(NpgsqlPolygon).FullName, a =>  {
				var polygon = (NpgsqlPolygon)a;
				try { int count = polygon.Count; return polygon; } catch { return new NpgsqlPolygon(new NpgsqlPoint(0, 0), new NpgsqlPoint(0, 0)); }
			} }, { typeof(NpgsqlPolygon[]).FullName, a => getParamterArrayValue(typeof(NpgsqlPolygon), a, new NpgsqlPolygon(new NpgsqlPoint(0, 0), new NpgsqlPoint(0, 0))) }, { typeof(NpgsqlPolygon?[]).FullName, a => getParamterArrayValue(typeof(NpgsqlPolygon?), a, null) },
			{ typeof((IPAddress Address, int Subnet)).FullName, a => {
				var inet = ((IPAddress Address, int Subnet))a;
				if (inet.Address == null) return (IPAddress.Any, inet.Subnet);
				return inet;
			} }, { typeof((IPAddress Address, int Subnet)[]).FullName, a => getParamterArrayValue(typeof((IPAddress Address, int Subnet)), a, (IPAddress.Any, 0)) }, { typeof((IPAddress Address, int Subnet)?[]).FullName, a => getParamterArrayValue(typeof((IPAddress Address, int Subnet)?), a, null) },
		};
		static object getParamterValue(Type type, object value, int level = 0) {
			if (type.FullName == "System.Byte[]") return value;
			if (type.IsArray && level == 0) {
				var elementType = type.GetElementType();
				Type enumType = null;
				if (elementType.IsEnum) enumType = elementType;
				else if (elementType.IsNullableType() && elementType.GenericTypeArguments.First().IsEnum) enumType = elementType.GenericTypeArguments.First();
				if (enumType != null) return enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any() ?
					getParamterArrayValue(typeof(long), value, elementType.IsEnum ? null : Enum.GetValues(enumType).GetValue(0)) :
					getParamterArrayValue(typeof(int), value, elementType.IsEnum ? null : Enum.GetValues(enumType).GetValue(0));
				return dicGetParamterValue.TryGetValue(type.FullName, out var trydicarr) ? trydicarr(value) : value;
			}
			if (type.IsNullableType()) type = type.GenericTypeArguments.First();
			if (type.IsEnum) return (int)value;
			if (dicGetParamterValue.TryGetValue(type.FullName, out var trydic)) return trydic(value);
			return value;
		}

		internal override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, Type type, object value) {
			if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
			else if (_orm.CodeFirst.IsSyncStructureToLower) parameterName = parameterName.ToLower();
			if (value != null) value = getParamterValue(type, value);
			var ret = new NpgsqlParameter { ParameterName = $"@{parameterName}", Value = value };
			//if (value.GetType().IsEnum || value.GetType().GenericTypeArguments.FirstOrDefault()?.IsEnum == true) {
			//	ret.DataTypeName = "";
			//} else {
			var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
			if (tp != null) ret.NpgsqlDbType = (NpgsqlDbType)tp.Value;
			//}
			_params?.Add(ret);
			return ret;
		}

		internal override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
			Utils.GetDbParamtersByObject<NpgsqlParameter>(sql, obj, "@", (name, type, value) => {
				if (value != null) value = getParamterValue(type, value);
				var ret = new NpgsqlParameter { ParameterName = $"@{name}", Value = value };
				//if (value.GetType().IsEnum || value.GetType().GenericTypeArguments.FirstOrDefault()?.IsEnum == true) {
				//	ret.DataTypeName = "";
				//} else {
				var tp = _orm.CodeFirst.GetDbInfo(type)?.type;
				if (tp != null) ret.NpgsqlDbType = (NpgsqlDbType)tp.Value;
				//}
				return ret;
			});

		internal override string FormatSql(string sql, params object[] args) => sql?.FormatPostgreSQL(args);
		internal override string QuoteSqlName(string name) => $"\"{name.Trim('"').Replace(".", "\".\"")}\"";
		internal override string QuoteParamterName(string name) => $"@{(_orm.CodeFirst.IsSyncStructureToLower ? name.ToLower() : name)}";
		internal override string IsNull(string sql, object value) => $"coalesce({sql}, {value})";
		internal override string StringConcat(string left, string right, Type leftType, Type rightType) => $"{left} || {right}";
		internal override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";

		internal override string QuoteWriteParamter(Type type, string paramterName) => paramterName;
		internal override string QuoteReadColumn(Type type, string columnName) => columnName;

		internal override string GetNoneParamaterSqlValue(Type type, object value) {
			if (value == null) return "NULL";
			value = getParamterValue(type, value);
			var type2 = value.GetType();
			if (type2 == typeof(byte[])) {
				var bytes = value as byte[];
				var sb = new StringBuilder().Append("E'\\x");
				foreach (var vc in bytes) {
					if (vc < 10) sb.Append("0");
					sb.Append(vc.ToString("X"));
				}
				return sb.Append("'").ToString(); //val = Encoding.UTF8.GetString(val as byte[]);
			} else if (type2 == typeof(TimeSpan) || type2 == typeof(TimeSpan?)) {
				var ts = (TimeSpan)value;
				value = $"{ts.Hours}:{ts.Minutes}:{ts.Seconds}";
			}
			return FormatSql("{0}", value, 1);
		}

		internal override string DbName => "PostgreSQL";
	}
}
