using FreeSql.DataAnnotations;
using FreeSql.Internal.Model;
using Newtonsoft.Json.Linq;
using Npgsql.LegacyPostgis;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace FreeSql.Internal {
	class UtilsReflection {

		static ConcurrentDictionary<string, TableInfo> _cacheGetTableByEntity = new ConcurrentDictionary<string, TableInfo>();
		internal static TableInfo GetTableByEntity(Type entity, CommonUtils common) {
			if (entity.FullName.StartsWith("<>f__AnonymousType")) return null;
			if (_cacheGetTableByEntity.TryGetValue($"{common.DbName}-{entity.FullName}", out var trytb)) return trytb; //区分数据库类型缓存
			if (common.CodeFirst.GetDbInfo(entity) != null) return null;

			var tbattr = entity.GetCustomAttributes(typeof(TableAttribute), false).LastOrDefault() as TableAttribute;
			trytb = new TableInfo();
			trytb.Type = entity;
			trytb.Properties = entity.GetProperties().ToDictionary(a => a.Name, a => a, StringComparer.CurrentCultureIgnoreCase);
			trytb.CsName = entity.Name;
			trytb.DbName = (tbattr?.Name ?? entity.Name);
			trytb.DbOldName = tbattr?.OldName;
			if (common.CodeFirst.IsSyncStructureToLower) {
				trytb.DbName = trytb.DbName.ToLower();
				trytb.DbOldName = trytb.DbOldName?.ToLower();
			}
			trytb.SelectFilter = tbattr?.SelectFilter;
			foreach (var p in trytb.Properties.Values) {
				var tp = common.CodeFirst.GetDbInfo(p.PropertyType);
				//if (tp == null) continue;
				var colattr = p.GetCustomAttributes(typeof(ColumnAttribute), false).LastOrDefault() as ColumnAttribute;
				if (tp == null && colattr == null) continue;
				if (colattr == null)
					colattr = new ColumnAttribute {
						Name = p.Name,
						DbType = tp.Value.dbtypeFull,
						IsIdentity = false,
						IsNullable = tp.Value.isnullable ?? true,
						IsPrimary = false,
					};
				if (string.IsNullOrEmpty(colattr.DbType)) colattr.DbType = tp?.dbtypeFull ?? "varchar(255)";
				colattr.DbType = colattr.DbType.ToUpper();

				if (tp != null && tp.Value.isnullable == null) colattr.IsNullable = tp.Value.dbtypeFull.Contains("NOT NULL") == false;
				if (colattr.DbType?.Contains("NOT NULL") == true) colattr.IsNullable = false;
				if (string.IsNullOrEmpty(colattr.Name)) colattr.Name = p.Name;
				if (common.CodeFirst.IsSyncStructureToLower) colattr.Name = colattr.Name.ToLower();

				if ((colattr.IsNullable != true || colattr.IsIdentity == true || colattr.IsPrimary == true) && colattr.DbType.Contains("NOT NULL") == false) {
					colattr.IsNullable = false;
					colattr.DbType += " NOT NULL";
				}
				if (colattr.IsNullable == true && colattr.DbType.Contains("NOT NULL")) colattr.DbType = colattr.DbType.Replace("NOT NULL", "");
				colattr.DbType = Regex.Replace(colattr.DbType, @"\([^\)]+\)", m => {
					var tmpLt = Regex.Replace(m.Groups[0].Value, @"\s", "");
					if (tmpLt.Contains("CHAR")) tmpLt = tmpLt.Replace("CHAR", " CHAR");
					if (tmpLt.Contains("BYTE")) tmpLt = tmpLt.Replace("BYTE", " BYTE");
					return tmpLt;
				});
				colattr.DbDefautValue = trytb.Properties[p.Name].GetValue(Activator.CreateInstance(trytb.Type));
				if (colattr.DbDefautValue == null) colattr.DbDefautValue = tp?.defaultValue;
				if (colattr.IsNullable == false && colattr.DbDefautValue == null)
					colattr.DbDefautValue = Activator.CreateInstance(p.PropertyType.IsNullableType() ? p.PropertyType.GenericTypeArguments.FirstOrDefault() : p.PropertyType);

				var col = new ColumnInfo {
					Table = trytb,
					CsName = p.Name,
					CsType = p.PropertyType,
					Attribute = colattr
				};
				trytb.Columns.Add(colattr.Name, col);
				trytb.ColumnsByCs.Add(p.Name, col);
			}
			trytb.Primarys = trytb.Columns.Values.Where(a => a.Attribute.IsPrimary == true).ToArray();
			if (trytb.Primarys.Any() == false) {
				trytb.Primarys = trytb.Columns.Values.Where(a => a.Attribute.IsIdentity == true).ToArray();
				foreach(var col in trytb.Primarys)
					col.Attribute.IsPrimary = true;
			}
			_cacheGetTableByEntity.TryAdd(entity.FullName, trytb);
			return trytb;
		}

		internal static T[] GetDbParamtersByObject<T>(string sql, object obj, string paramPrefix, Func<string, Type, object, T> constructorParamter) {
			if (string.IsNullOrEmpty(sql) || obj == null) return new T[0];
			var ttype = typeof(T);
			var type = obj.GetType();
			if (type == ttype) return new[] { (T)Convert.ChangeType(obj, type) };
			var ret = new List<T>();
			var ps = type.GetProperties();
			foreach (var p in ps) {
				if (sql.IndexOf($"{paramPrefix}{p.Name}", StringComparison.CurrentCultureIgnoreCase) == -1) continue;
				var pvalue = p.GetValue(obj);
				if (p.PropertyType == ttype) ret.Add((T)Convert.ChangeType(pvalue, ttype));
				else ret.Add(constructorParamter(p.Name, p.PropertyType, pvalue));
			}
			return ret.ToArray();
		}

		static Dictionary<Type, bool> dicExecuteArrayRowReadClassOrTuple = new Dictionary<Type, bool> {
			[typeof(bool)] = true,
			[typeof(sbyte)] = true,
			[typeof(short)] = true,
			[typeof(int)] = true,
			[typeof(long)] = true,
			[typeof(byte)] = true,
			[typeof(ushort)] = true,
			[typeof(uint)] = true,
			[typeof(ulong)] = true,
			[typeof(double)] = true,
			[typeof(float)] = true,
			[typeof(decimal)] = true,
			[typeof(TimeSpan)] = true,
			[typeof(DateTime)] = true,
			[typeof(DateTimeOffset)] = true,
			[typeof(byte[])] = true,
			[typeof(string)] = true,
			[typeof(Guid)] = true,
			[typeof(MygisPoint)] = true,
			[typeof(MygisLineString)] = true,
			[typeof(MygisPolygon)] = true,
			[typeof(MygisMultiPoint)] = true,
			[typeof(MygisMultiLineString)] = true,
			[typeof(MygisMultiPolygon)] = true,
			[typeof(BitArray)] = true,
			[typeof(NpgsqlPoint)] = true,
			[typeof(NpgsqlLine)] = true,
			[typeof(NpgsqlLSeg)] = true,
			[typeof(NpgsqlBox)] = true,
			[typeof(NpgsqlPath)] = true,
			[typeof(NpgsqlPolygon)] = true,
			[typeof(NpgsqlCircle)] = true,
			[typeof((IPAddress Address, int Subnet))] = true,
			[typeof(IPAddress)] = true,
			[typeof(PhysicalAddress)] = true,
			[typeof(NpgsqlRange<int>)] = true,
			[typeof(NpgsqlRange<long>)] = true,
			[typeof(NpgsqlRange<decimal>)] = true,
			[typeof(NpgsqlRange<DateTime>)] = true,
			[typeof(PostgisPoint)] = true,
			[typeof(PostgisLineString)] = true,
			[typeof(PostgisPolygon)] = true,
			[typeof(PostgisMultiPoint)] = true,
			[typeof(PostgisMultiLineString)] = true,
			[typeof(PostgisMultiPolygon)] = true,
			[typeof(PostgisGeometry)] = true,
			[typeof(PostgisGeometryCollection)] = true,
			[typeof(Dictionary<string, string>)] = true,
			[typeof(JToken)] = true,
			[typeof(JObject)] = true,
			[typeof(JArray)] = true,
		};

		internal static ConcurrentDictionary<Type, _dicClassConstructorInfo> _dicClassConstructor = new ConcurrentDictionary<Type, _dicClassConstructorInfo>();
		internal static ConcurrentDictionary<Type, _dicTupleConstructorInfo> _dicTupleConstructor = new ConcurrentDictionary<Type, _dicTupleConstructorInfo>();
		internal class _dicClassConstructorInfo {
			public ConstructorInfo Constructor { get; set; }
			public PropertyInfo[] Properties { get; set; }
		}
		internal class _dicTupleConstructorInfo {
			public ConstructorInfo Constructor { get; set; }
			public Type[] Types { get; set; }
		}
		internal static (object value, int dataIndex) ExecuteArrayRowReadClassOrTuple(Type type, Dictionary<string, int> names, object[] row, int dataIndex = 0) {
			if (type.IsArray) return (GetDataReaderValue(type, row[dataIndex]), dataIndex + 1);
			var typeGeneric = type;
			if (typeGeneric.IsNullableType()) typeGeneric = type.GenericTypeArguments.First();
			if (typeGeneric.IsEnum ||
				dicExecuteArrayRowReadClassOrTuple.ContainsKey(typeGeneric))
				return (GetDataReaderValue(type, row[dataIndex]), dataIndex + 1);
			if (type.Namespace == "System" && (type.FullName == "System.String" || type.IsValueType)) { //值类型，或者元组
				bool isTuple = type.Name.StartsWith("ValueTuple`");
				if (isTuple) {
					if (_dicTupleConstructor.TryGetValue(type, out var tupleInfo) == false) {
						var types = type.GetFields().Select(a => a.FieldType).ToArray();
						tupleInfo = new _dicTupleConstructorInfo { Constructor = type.GetConstructor(types), Types = types };
						_dicTupleConstructor.AddOrUpdate(type, tupleInfo, (t2, c2) => tupleInfo);
					}
					var parms = new object[tupleInfo.Types.Length];
					for (int a = 0; a < parms.Length; a++) {
						var read = ExecuteArrayRowReadClassOrTuple(tupleInfo.Types[a], names, row, dataIndex);
						if (read.dataIndex > dataIndex) dataIndex = read.dataIndex;
						parms[a] = read.value;
					}
					return (tupleInfo.Constructor?.Invoke(parms), dataIndex);
				}
				return (dataIndex >= row.Length || (row[dataIndex] ?? DBNull.Value) == DBNull.Value ? null : GetDataReaderValue(type, row[dataIndex]), dataIndex + 1);
			}
			if (type == typeof(object) && names != null) {
				dynamic expando = new System.Dynamic.ExpandoObject(); //动态类型字段 可读可写
				var expandodic = (IDictionary<string, object>)expando;
				foreach (var name in names)
					expandodic.Add(name.Key, row[name.Value]);
				return (expando, names.Count);
			}
			//类注入属性
			if (_dicClassConstructor.TryGetValue(type, out var classInfo)== false) {
				classInfo = new _dicClassConstructorInfo { Constructor = type.GetConstructor(new Type[0]), Properties = type.GetProperties() };
				_dicClassConstructor.TryAdd(type, classInfo);
			}
			var value = classInfo.Constructor.Invoke(new object[0]);
			foreach(var prop in classInfo.Properties) {
				var tryidx = dataIndex;
				if (names != null && names.TryGetValue(prop.Name, out tryidx) == false) continue;
				var read = ExecuteArrayRowReadClassOrTuple(prop.PropertyType, names, row, tryidx);
				if (read.dataIndex > dataIndex) dataIndex = read.dataIndex;
				prop.SetValue(value, read.value, null);
				//FillPropertyValue(value, p.Name, read.value);
				//p.SetValue(value, read.value);
			}
			return (value, dataIndex);
		}

		internal static void FillPropertyValue(object info, string memberAccessPath, object value) {
			var current = info;
			PropertyInfo prop = null;
			var members = memberAccessPath.Split('.');
			for (var a = 0; a < members.Length; a++) {
				var type = current.GetType();
				prop = type.GetProperty(members[a], BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
				if (prop == null) throw new Exception(string.Concat(type.FullName, " 没有定义属性 ", members[a]));
				if (a < members.Length - 1) current = prop.GetValue(current);
			}
			prop.SetValue(current, GetDataReaderValue(prop.PropertyType, value), null);
		}
		internal static object GetDataReaderValue(Type type, object value) {
			if (value == null || value == DBNull.Value) return null;
			if (type.FullName == "System.Byte[]") return value;
			if (type.IsArray) {
				var elementType = type.GetElementType();
				var valueArr = value as Array;
				if (elementType == valueArr.GetType().GetElementType()) return value;
				var len = valueArr.GetLength(0);
				var ret = Array.CreateInstance(elementType, len);
				for (var a = 0; a < len; a++) {
					var item = valueArr.GetValue(a);
					ret.SetValue(GetDataReaderValue(elementType, item), a);
				}
				return ret;
			}
			if (type.IsNullableType()) type = type.GenericTypeArguments.First();
			if (type.IsEnum) return Enum.Parse(type, string.Concat(value), true);
			switch(type.FullName) {
				case "System.Guid":
					if (value.GetType() != type) return Guid.TryParse(string.Concat(value), out var tryguid) ? tryguid : Guid.Empty;
					return value;
				case "MygisPoint": return MygisPoint.Parse(string.Concat(value)) as MygisPoint;
				case "MygisLineString": return MygisLineString.Parse(string.Concat(value)) as MygisLineString;
				case "MygisPolygon": return MygisPolygon.Parse(string.Concat(value)) as MygisPolygon;
				case "MygisMultiPoint": return MygisMultiPoint.Parse(string.Concat(value)) as MygisMultiPoint;
				case "MygisMultiLineString": return MygisMultiLineString.Parse(string.Concat(value)) as MygisMultiLineString;
				case "MygisMultiPolygon": return MygisMultiPolygon.Parse(string.Concat(value)) as MygisMultiPolygon;
				case "Newtonsoft.Json.Linq.JToken": return JToken.Parse(string.Concat(value));
				case "Newtonsoft.Json.Linq.JObject": return JObject.Parse(string.Concat(value));
				case "Newtonsoft.Json.Linq.JArray": return JArray.Parse(string.Concat(value));
				case "Npgsql.LegacyPostgis.PostgisGeometry": return value;
			}
			if (type != value.GetType()) {
				if (type.FullName == "System.TimeSpan") return TimeSpan.FromSeconds(double.Parse(value.ToString()));
				return Convert.ChangeType(value, type);
			}
			return value;
		}
		internal static string GetCsName(string name) {
			name = Regex.Replace(name.TrimStart('@'), @"[^\w]", "_");
			return char.IsLetter(name, 0) ? name : string.Concat("_", name);
		}
	}
}