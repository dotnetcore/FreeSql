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
using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace FreeSql.Internal {
	class Utils {

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

				if ((colattr.IsNullable == false || colattr.IsIdentity || colattr.IsPrimary) && colattr.DbType.Contains("NOT NULL") == false) {
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
			if (trytb.Primarys.Any() == false) {
				trytb.Primarys = trytb.Columns.Values.Where(a => a.Attribute.IsIdentity).ToArray();
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
		static ConcurrentDictionary<Type, Func<Type, Dictionary<string, int>, object[], int, RowInfo>> _dicExecuteArrayRowReadClassOrTuple = new ConcurrentDictionary<Type, Func<Type, Dictionary<string, int>, object[], int, RowInfo>>();
		internal class RowInfo {
			public object Value { get; set; }
			public int DataIndex { get; set; }
			public RowInfo(object value, int dataIndex) {
				this.Value = value;
				this.DataIndex = dataIndex;
			}
			public static ConstructorInfo Constructor = typeof(RowInfo).GetConstructor(new[] { typeof(object), typeof(int) });
			public static PropertyInfo PropertyValue = typeof(RowInfo).GetProperty("Value");
			public static PropertyInfo PropertyDataIndex = typeof(RowInfo).GetProperty("DataIndex");
		}
		internal static RowInfo ExecuteArrayRowReadClassOrTuple(Type type, Dictionary<string, int> names, object[] row, int dataIndex = 0) {
			var func = _dicExecuteArrayRowReadClassOrTuple.GetOrAdd(type, s => {
				var returnTarget = Expression.Label(typeof(RowInfo));
				var typeExp = Expression.Parameter(typeof(Type), "type");
				var namesExp = Expression.Parameter(typeof(Dictionary<string, int>), "names");
				var rowExp = Expression.Parameter(typeof(object[]), "row");
				var dataIndexExp = Expression.Parameter(typeof(int), "dataIndex");

				if (type.IsArray) return Expression.Lambda<Func<Type, Dictionary<string, int>, object[], int, RowInfo>>(
					Expression.New(RowInfo.Constructor,
						Expression.Call(MethodGetDataReaderValue, new Expression[] { typeExp, Expression.ArrayAccess(rowExp, dataIndexExp) }), 
						Expression.Add(dataIndexExp, Expression.Constant(1))
					), new[] { typeExp, namesExp, rowExp, dataIndexExp }).Compile();

				var typeGeneric = type;
				if (typeGeneric.FullName.StartsWith("System.Nullable`1[")) typeGeneric = type.GenericTypeArguments.First();
				if (typeGeneric.IsEnum ||
					dicExecuteArrayRowReadClassOrTuple.ContainsKey(typeGeneric))
					return Expression.Lambda<Func<Type, Dictionary<string, int>, object[], int, RowInfo>>(
					Expression.New(RowInfo.Constructor,
						Expression.Call(MethodGetDataReaderValue, new Expression[] { typeExp, Expression.ArrayAccess(rowExp, dataIndexExp) }),
						Expression.Add(dataIndexExp, Expression.Constant(1))
					), new[] { typeExp, namesExp, rowExp, dataIndexExp }).Compile();

				if (type.Namespace == "System" && (type.FullName == "System.String" || type.IsValueType)) { //值类型，或者元组
					bool isTuple = type.Name.StartsWith("ValueTuple`");
					if (isTuple) {
						var ret2Exp = Expression.Variable(type, "ret");
						var read2Exp = Expression.Variable(typeof(RowInfo), "read");
						var read2ExpValue = Expression.MakeMemberAccess(read2Exp, RowInfo.PropertyValue);
						var read2ExpDataIndex = Expression.MakeMemberAccess(read2Exp, RowInfo.PropertyDataIndex);
						var block2Exp = new List<Expression>();

						var fields = type.GetFields();
						foreach (var field in fields) {
							block2Exp.AddRange(new Expression[] {
								//Expression.TryCatch(Expression.Block(
								//	typeof(void),
									Expression.Assign(read2Exp, Expression.Call(MethodExecuteArrayRowReadClassOrTuple, new Expression[] { Expression.Constant(field.FieldType), namesExp, rowExp, dataIndexExp })),
									Expression.IfThen(Expression.GreaterThan(read2ExpDataIndex, dataIndexExp), 
										Expression.Assign(dataIndexExp, read2ExpDataIndex)),
									Expression.Assign(Expression.MakeMemberAccess(ret2Exp, field), Expression.Convert(read2ExpValue, field.FieldType))
								//), 
								//Expression.Catch(typeof(Exception), Expression.Block(
								//		Expression.IfThen(Expression.Equal(read2ExpDataIndex, Expression.Constant(0)), Expression.Throw(Expression.Constant(new Exception(field.Name + "," + 0)))),
								//		Expression.IfThen(Expression.Equal(read2ExpDataIndex, Expression.Constant(1)), Expression.Throw(Expression.Constant(new Exception(field.Name + "," + 1)))),
								//		Expression.IfThen(Expression.Equal(read2ExpDataIndex, Expression.Constant(2)), Expression.Throw(Expression.Constant(new Exception(field.Name + "," + 2)))),
								//		Expression.IfThen(Expression.Equal(read2ExpDataIndex, Expression.Constant(3)), Expression.Throw(Expression.Constant(new Exception(field.Name + "," + 3)))),
								//		Expression.IfThen(Expression.Equal(read2ExpDataIndex, Expression.Constant(4)), Expression.Throw(Expression.Constant(new Exception(field.Name + "," + 4))))
								//	)
								//))
							});
						}
						block2Exp.AddRange(new Expression[] {
							Expression.Return(returnTarget, Expression.New(RowInfo.Constructor, Expression.Convert(ret2Exp, typeof(object)), dataIndexExp)),
							Expression.Label(returnTarget, Expression.Default(typeof(RowInfo)))
						});
						return Expression.Lambda<Func<Type, Dictionary<string, int>, object[], int, RowInfo>>(
							Expression.Block(new[] { ret2Exp, read2Exp }, block2Exp), new[] { typeExp, namesExp, rowExp, dataIndexExp }).Compile();
					}
					var rowLenExp = Expression.ArrayLength(rowExp);
					return Expression.Lambda<Func<Type, Dictionary<string, int>, object[], int, RowInfo>>(
						Expression.Block(
							Expression.IfThen(
								Expression.LessThan(dataIndexExp, rowLenExp),
									Expression.Return(returnTarget, Expression.New(RowInfo.Constructor, 
										Expression.Call(MethodGetDataReaderValue, new Expression[] { typeExp, Expression.ArrayAccess(rowExp, dataIndexExp) }), 
										Expression.Add(dataIndexExp, Expression.Constant(1))))
							),
							Expression.Label(returnTarget, Expression.Default(typeof(RowInfo)))
						), new[] { typeExp, namesExp, rowExp, dataIndexExp }).Compile();
				}

				if (type == typeof(object) && names != null) {
					Func<Type, Dictionary<string, int>, object[], int, RowInfo> dynamicFunc = (type2, names2, row2, dataindex2) => {
						dynamic expando = new System.Dynamic.ExpandoObject(); //动态类型字段 可读可写
						var expandodic = (IDictionary<string, object>)expando;
						foreach (var name in names2)
							expandodic.Add(name.Key, row2[name.Value]);
						return new RowInfo(expando, names2.Count);
					};
					return dynamicFunc;// Expression.Lambda<Func<Type, Dictionary<string, int>, object[], int, RowInfo>>(null);
				}

				//类注入属性
				var retExp = Expression.Variable(type, "ret");
				var readExp = Expression.Variable(typeof(RowInfo), "read");
				var readExpValue = Expression.MakeMemberAccess(readExp, RowInfo.PropertyValue);
				var readExpDataIndex = Expression.MakeMemberAccess(readExp, RowInfo.PropertyDataIndex);
				var tryidxExp = Expression.Variable(typeof(int), "tryidx");
				var blockExp = new List<Expression>();
				blockExp.Add(Expression.Assign(retExp, Expression.New(type.GetConstructor(new Type[0]))));
				
				var props = type.GetProperties();
				foreach (var prop in props) {
					var propGetSetMethod = prop.GetSetMethod();
					blockExp.AddRange(new Expression[] {
						Expression.Assign(tryidxExp, dataIndexExp),
						Expression.IfThen(Expression.Not(Expression.And(
							Expression.NotEqual(namesExp, Expression.Constant(null)),
							Expression.Not(Expression.Call(namesExp, namesExp.Type.GetMethod("TryGetValue"), Expression.Constant(prop.Name), tryidxExp)))),
							Expression.Block(
								//Expression.Assign(tryidxExp, Expression.Call(namesExp, namesExp Expression.Constant(prop.Name))),
								Expression.Assign(readExp, Expression.Call(MethodExecuteArrayRowReadClassOrTuple, new Expression[] { Expression.Constant(prop.PropertyType), namesExp, rowExp, tryidxExp })),
								Expression.IfThen(Expression.GreaterThan(readExpDataIndex, dataIndexExp), 
									Expression.Assign(dataIndexExp, readExpDataIndex)),
								Expression.IfThen(Expression.NotEqual(readExpValue, Expression.Constant(null)), 
									Expression.Call(retExp, propGetSetMethod, Expression.Convert(readExpValue, prop.PropertyType)))
							)
						)
					});
				}
				blockExp.AddRange(new Expression[] {
					Expression.Return(returnTarget, Expression.New(RowInfo.Constructor, retExp, dataIndexExp)),
					Expression.Label(returnTarget, Expression.Default(typeof(RowInfo)))
				});
				return Expression.Lambda<Func<Type, Dictionary<string, int>, object[], int, RowInfo>>(
					Expression.Block(new[] { retExp, readExp, tryidxExp }, blockExp), new[] { typeExp, namesExp, rowExp, dataIndexExp }).Compile();
			});

			return func(type, names, row, dataIndex);
		}

		static MethodInfo MethodExecuteArrayRowReadClassOrTuple = typeof(Utils).GetMethod("ExecuteArrayRowReadClassOrTuple", BindingFlags.Static | BindingFlags.NonPublic);
		static MethodInfo MethodGetDataReaderValue = typeof(Utils).GetMethod("GetDataReaderValue", BindingFlags.Static | BindingFlags.NonPublic);

		static ConcurrentDictionary<string, Action<object, object>> _dicFillPropertyValue = new ConcurrentDictionary<string, Action<object, object>>();
		internal static void FillPropertyValue(object info, string memberAccessPath, object value) {
			var typeObj = info.GetType();
			var typeValue = value.GetType();
			var key = "FillPropertyValue_" + typeObj.FullName + "_" + typeValue.FullName;
			var act = _dicFillPropertyValue.GetOrAdd($"{key}.{memberAccessPath}", s => {
				var parmInfo = Expression.Parameter(typeof(object), "info");
				var parmValue = Expression.Parameter(typeof(object), "value");
				Expression exp = Expression.Convert(parmInfo, typeObj);
				foreach (var pro in memberAccessPath.Split('.'))
					exp = Expression.PropertyOrField(exp, pro) ?? throw new Exception(string.Concat(exp.Type.FullName, " 没有定义属性 ", pro));

				var value2 = Expression.Call(MethodGetDataReaderValue, Expression.Constant(exp.Type), parmValue);
				var value3 = Expression.Convert(parmValue, typeValue);
				exp = Expression.Assign(exp, value3);
				return Expression.Lambda<Action<object, object>>(exp, parmInfo, parmValue).Compile();
			});
			act(info, value);
		}

		static ConcurrentDictionary<Type, Func<object, object>> _dicGetDataReaderValue = new ConcurrentDictionary<Type, Func<object, object>>();
		internal static object GetDataReaderValue(Type type, object value) {
			if (value == null || value == DBNull.Value) return null;

			var func = _dicGetDataReaderValue.GetOrAdd(type, k => {
				var returnTarget = Expression.Label(typeof(object));
				var parmExp = Expression.Parameter(typeof(object), "value");

				if (type.FullName == "System.Byte[]") return Expression.Lambda<Func<object, object>>(parmExp, parmExp).Compile();

				if (type.IsArray) {
					var elementType = type.GetElementType();
					var valueArr = value as Array;

					if (elementType == valueArr.GetType().GetElementType()) return Expression.Lambda<Func<object, object>>(parmExp, parmExp).Compile();

					var arr = Expression.Variable(type, "arr");
					var arrlen = Expression.Variable(typeof(int), "arrlen");
					var x = Expression.Variable(typeof(int), "x");
					var label = Expression.Label(typeof(int));
					var ret = Expression.NewArrayBounds(elementType, arrlen);
					return Expression.Lambda<Func<object, object>>(
						Expression.Block(
							new[] { arr, arrlen, x },
							Expression.Assign(arr, Expression.Convert(parmExp, type)),
							Expression.Assign(arrlen, Expression.ArrayLength(arr)),
							Expression.Assign(x, Expression.Constant(0)),
							ret,
							Expression.Loop(
								Expression.IfThenElse(
									Expression.LessThan(x, arrlen),
									Expression.Block(
										Expression.Assign(
											Expression.ArrayAccess(ret, x), 
											Expression.Call(
												MethodGetDataReaderValue,
												Expression.Constant(elementType, typeof(Type)),
												Expression.ArrayAccess(arr, x)
											)
										),
										Expression.PostIncrementAssign(x)
									),
									Expression.Break(label, x)
								),
								label
							)
						), parmExp).Compile();
				}

				if (type.FullName.StartsWith("System.Nullable`1[")) type = type.GenericTypeArguments.First();
				if (type.IsEnum) return Expression.Lambda<Func<object, object>>(
					Expression.Call(
						typeof(Enum).GetMethod("Parse", new[] { typeof(Type), typeof(string), typeof(bool) }),
						Expression.Constant(type, typeof(Type)),
						Expression.Convert(parmExp, typeof(string)),
						Expression.Constant(true, typeof(bool))
					) , parmExp).Compile();

				switch (type.FullName) {
					case "System.Guid":
						if (value.GetType() != type) return Expression.Lambda<Func<object, object>>(
							Expression.Block(
								Expression.TryCatch(
									Expression.Return(returnTarget, Expression.Call(typeof(Guid).GetMethod("Parse"), Expression.Convert(parmExp, typeof(string)))),
									Expression.Catch(typeof(Exception), 
										Expression.Return(returnTarget, Expression.Constant(Guid.Empty)))
								),
								Expression.Label(returnTarget, Expression.Default(type))
							), parmExp).Compile();

						return Expression.Lambda<Func<object, object>>(parmExp, parmExp).Compile();

					case "MygisPoint": return Expression.Lambda<Func<object, object>>(
							Expression.TypeAs(
								Expression.Call(typeof(MygisPoint).GetMethod("Parse"), Expression.Convert(parmExp, typeof(string))),
								typeof(MygisPoint)
							), parmExp).Compile();
					case "MygisLineString": return Expression.Lambda<Func<object, object>>(
							Expression.TypeAs(
								Expression.Call(typeof(MygisLineString).GetMethod("Parse"), Expression.Convert(parmExp, typeof(string))), 
								typeof(MygisLineString)
							), parmExp).Compile();
					case "MygisPolygon": return Expression.Lambda<Func<object, object>>(
							Expression.TypeAs(
								Expression.Call(typeof(MygisPolygon).GetMethod("Parse"), Expression.Convert(parmExp, typeof(string))), 
								typeof(MygisPolygon)
							), parmExp).Compile();
					case "MygisMultiPoint": return Expression.Lambda<Func<object, object>>(
							Expression.TypeAs(
								Expression.Call(typeof(MygisMultiPoint).GetMethod("Parse"), Expression.Convert(parmExp, typeof(string))), 
								typeof(MygisMultiPoint)
							), parmExp).Compile();
					case "MygisMultiLineString": return Expression.Lambda<Func<object, object>>(
							Expression.TypeAs(
								Expression.Call(typeof(MygisMultiLineString).GetMethod("Parse"), Expression.Convert(parmExp, typeof(string))), 
								typeof(MygisMultiLineString)
							), parmExp).Compile();
					case "MygisMultiPolygon": return Expression.Lambda<Func<object, object>>(
							Expression.TypeAs(
								Expression.Call(typeof(MygisMultiPolygon).GetMethod("Parse"), Expression.Convert(parmExp, typeof(string))), 
								typeof(MygisMultiPolygon)
							), parmExp).Compile();
					case "Newtonsoft.Json.Linq.JToken": return Expression.Lambda<Func<object, object>>(
							Expression.TypeAs(
								Expression.Call(typeof(JToken).GetMethod("Parse", new[] { typeof(string) }), Expression.Convert(parmExp, typeof(string))), 
								typeof(JToken)
							), parmExp).Compile();
					case "Newtonsoft.Json.Linq.JObject": return Expression.Lambda<Func<object, object>>(
							Expression.TypeAs(
								Expression.Call(typeof(JObject).GetMethod("Parse", new[] { typeof(string) }), Expression.Convert(parmExp, typeof(string))), 
								typeof(JObject)
							), parmExp).Compile();
					case "Newtonsoft.Json.Linq.JArray": return Expression.Lambda<Func<object, object>>(
							Expression.TypeAs(
								Expression.Call(typeof(JArray).GetMethod("Parse", new[] { typeof(string) }), Expression.Convert(parmExp, typeof(string))), 
								typeof(JArray)
							), parmExp).Compile();
					case "Npgsql.LegacyPostgis.PostgisGeometry": return Expression.Lambda<Func<object, object>>(parmExp, parmExp).Compile();
				}
				if (type != value.GetType()) {
					if (type.FullName == "System.TimeSpan") return Expression.Lambda<Func<object, object>>(
						Expression.Call(
							typeof(TimeSpan).GetMethod("FromSeconds"),
							Expression.Call(typeof(double).GetMethod("Parse", new[] { typeof(string) }), Expression.Convert(parmExp, typeof(string)))
						), parmExp).Compile();
					return Expression.Lambda<Func<object, object>>(
						Expression.Call(typeof(Convert).GetMethod("ChangeType", new[] { typeof(object), typeof(Type) }), parmExp, Expression.Constant(type, typeof(Type)))
					, parmExp).Compile();
				}
				return Expression.Lambda<Func<object, object>>(parmExp, parmExp).Compile();
			});
			return func(value);
		}
		internal static string GetCsName(string name) {
			name = Regex.Replace(name.TrimStart('@'), @"[^\w]", "_");
			return char.IsLetter(name, 0) ? name : string.Concat("_", name);
		}
	}
}