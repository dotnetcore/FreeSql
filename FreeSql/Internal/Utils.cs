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
	class Utils {

		static ConcurrentDictionary<string, TableInfo> _cacheGetTableByEntity = new ConcurrentDictionary<string, TableInfo>();
		internal static TableInfo GetTableByEntity(Type entity, CommonUtils common) {
			if (_cacheGetTableByEntity.TryGetValue($"{common.QuoteSqlName("db")}{entity.FullName}", out var trytb)) return trytb; //区分数据库类型缓存
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
				
				if ((colattr.IsNullable == false || colattr.IsIdentity || colattr.IsPrimary) && colattr.DbType.Contains("NOT NULL") == false) colattr.DbType += " NOT NULL";
				if (colattr.IsNullable == true && colattr.DbType.Contains("NOT NULL")) colattr.DbType = colattr.DbType.Replace("NOT NULL", "");
				colattr.DbType = Regex.Replace(colattr.DbType, @"\([^\)]+\)", m => Regex.Replace(m.Groups[0].Value, @"\s", ""));
				colattr.DbDefautValue = trytb.Properties[p.Name].GetValue(Activator.CreateInstance(trytb.Type));
				if (colattr.DbDefautValue == null) colattr.DbDefautValue = tp?.defaultValue;
				if (colattr.IsNullable == false && colattr.DbDefautValue == null) {
					var consturctorType = p.PropertyType.GenericTypeArguments.FirstOrDefault() ?? p.PropertyType;
					colattr.DbDefautValue = Activator.CreateInstance(consturctorType);
				}

				var col = new ColumnInfo {
					Table = trytb,
					CsName = p.Name,
					CsType = p.PropertyType,
					Attribute = colattr
				};
				trytb.Columns.Add(colattr.Name, col);
				trytb.ColumnsByCs.Add(p.Name, col);
			}
			trytb.Primarys = trytb.Columns.Values.Where(a => a.Attribute.IsPrimary).ToArray();
			if (trytb.Primarys.Any() == false) trytb.Primarys = trytb.Columns.Values.Where(a => a.Attribute.IsIdentity).ToArray();
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

		static Dictionary<string, bool> dicExecuteArrayRowReadClassOrTuple = new Dictionary<string, bool> {
			{ typeof(bool).FullName, true },
			{ typeof(sbyte).FullName, true },
			{ typeof(short).FullName, true },
			{ typeof(int).FullName, true },
			{ typeof(long).FullName, true },
			{ typeof(byte).FullName, true },
			{ typeof(ushort).FullName, true },
			{ typeof(uint).FullName, true },
			{ typeof(ulong).FullName, true },
			{ typeof(double).FullName, true },
			{ typeof(float).FullName, true },
			{ typeof(decimal).FullName, true },
			{ typeof(TimeSpan).FullName, true },
			{ typeof(DateTime).FullName, true },
			{ typeof(DateTimeOffset).FullName, true },
			{ typeof(byte[]).FullName, true },
			{ typeof(string).FullName, true },
			{ typeof(Guid).FullName, true },
			{ typeof(MygisPoint).FullName, true },
			{ typeof(MygisLineString).FullName, true },
			{ typeof(MygisPolygon).FullName, true },
			{ typeof(MygisMultiPoint).FullName, true },
			{ typeof(MygisMultiLineString).FullName, true },
			{ typeof(MygisMultiPolygon).FullName, true },
			{ typeof(BitArray).FullName, true },
			{ typeof(NpgsqlPoint).FullName, true },
			{ typeof(NpgsqlLine).FullName, true },
			{ typeof(NpgsqlLSeg).FullName, true },
			{ typeof(NpgsqlBox).FullName, true },
			{ typeof(NpgsqlPath).FullName, true },
			{ typeof(NpgsqlPolygon).FullName, true },
			{ typeof(NpgsqlCircle).FullName, true },
			{ typeof((IPAddress Address, int Subnet)).FullName, true },
			{ typeof(IPAddress).FullName, true },
			{ typeof(PhysicalAddress).FullName, true },
			{ typeof(NpgsqlRange<int>).FullName, true },
			{ typeof(NpgsqlRange<long>).FullName, true },
			{ typeof(NpgsqlRange<decimal>).FullName, true },
			{ typeof(NpgsqlRange<DateTime>).FullName, true },
			{ typeof(PostgisPoint).FullName, true },
			{ typeof(PostgisLineString).FullName, true },
			{ typeof(PostgisPolygon).FullName, true },
			{ typeof(PostgisMultiPoint).FullName, true },
			{ typeof(PostgisMultiLineString).FullName, true },
			{ typeof(PostgisMultiPolygon).FullName, true },
			{ typeof(PostgisGeometry).FullName, true },
			{ typeof(PostgisGeometryCollection).FullName, true },
			{ typeof(Dictionary<string, string>).FullName, true },
			{ typeof(JToken).FullName, true },
			{ typeof(JObject).FullName, true },
			{ typeof(JArray).FullName, true },
		};
		internal static (object value, int dataIndex) ExecuteArrayRowReadClassOrTuple(Type type, Dictionary<string, int> names, object[] row, int dataIndex = 0) {
			if (type.IsArray) return (GetDataReaderValue(type, row[dataIndex]), dataIndex + 1);
			var typeGeneric = type;
			if (typeGeneric.FullName.StartsWith("System.Nullable`1[")) typeGeneric = type.GenericTypeArguments.First();
			if (typeGeneric.IsEnum ||
				dicExecuteArrayRowReadClassOrTuple.ContainsKey(typeGeneric.FullName))
				return (GetDataReaderValue(type, row[dataIndex]), dataIndex + 1);
			if (type.Namespace == "System" && (type.FullName == "System.String" || type.IsValueType)) { //值类型，或者元组
				bool isTuple = type.Name.StartsWith("ValueTuple`");
				if (isTuple) {
					var fs = type.GetFields();
					var types = new Type[fs.Length];
					var parms = new object[fs.Length];
					for (int a = 0; a < fs.Length; a++) {
						types[a] = fs[a].FieldType;
						var read = ExecuteArrayRowReadClassOrTuple(types[a], names, row, dataIndex);
						if (read.dataIndex > dataIndex) dataIndex = read.dataIndex;
						parms[a] = read.value;
					}
					var constructor = type.GetConstructor(types);
					return (constructor?.Invoke(parms), dataIndex);
				}
				return (dataIndex >= row.Length || (row[dataIndex] ?? DBNull.Value) == DBNull.Value ? null : GetDataReaderValue(type, row[dataIndex]), dataIndex + 1);
			}
			if (type == typeof(object) && names != null) {
				dynamic expando = new System.Dynamic.ExpandoObject(); //动态类型字段 可读可写
				var expandodic = (IDictionary<string, object>)expando;
				foreach (var name in names)
					expandodic[Utils.GetCsName(name.Key)] = row[name.Value];
				return (expando, names.Count);
			}
			//类注入属性
			var value = type.GetConstructor(new Type[0]).Invoke(new object[0]);
			var ps = type.GetProperties();
			foreach(var p in ps) {
				var tryidx = dataIndex;
				if (names != null && names.TryGetValue(p.Name, out tryidx) == false) continue;
				var read = ExecuteArrayRowReadClassOrTuple(p.PropertyType, names, row, tryidx);
				if (read.dataIndex > dataIndex) dataIndex = read.dataIndex;
				FillPropertyValue(value, p.Name, read.value);
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
			if (type.FullName.StartsWith("System.Nullable`1[")) type = type.GenericTypeArguments.First();
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
			if (type != value.GetType()) return Convert.ChangeType(value, type);
			return value;
		}
		internal static string GetCsName(string name) {
			name = Regex.Replace(name.TrimStart('@'), @"[^\w]", "_");
			return char.IsLetter(name, 0) ? name : string.Concat("_", name);
		}
	}
}