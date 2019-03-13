using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;

public static class FreeSqlGlobalExtensions {

	public static FreeSql.ISelect<T> Queryable<T>(this IFreeSql freesql) where T : class => freesql.Select<T>();

	static Lazy<Dictionary<Type, bool>> dicIsNumberType = new Lazy<Dictionary<Type, bool>>(() => new Dictionary<Type, bool> {
		[typeof(sbyte)] = true, [typeof(sbyte?)] = true,
		[typeof(short)] = true, [typeof(short?)] = true,
		[typeof(int)] = true, [typeof(int?)] = true,
		[typeof(long)] = true, [typeof(long?)] = true,
		[typeof(byte)] = true, [typeof(byte?)] = true,
		[typeof(ushort)] = true, [typeof(ushort?)] = true,
		[typeof(uint)] = true, [typeof(uint?)] = true,
		[typeof(ulong)] = true, [typeof(ulong?)] = true,
		[typeof(double)] = true, [typeof(double?)] = true,
		[typeof(float)] = true, [typeof(float?)] = true,
		[typeof(decimal)] = true, [typeof(decimal?)] = true
	});
	public static bool IsNumberType(this Type that) => that == null ? false : dicIsNumberType.Value.ContainsKey(that);
	public static bool IsNullableType(this Type that) => that?.FullName.StartsWith("System.Nullable`1[") == true;
	internal static Type NullableTypeOrThis(this Type that) => that?.IsNullableType() == true ? that.GenericTypeArguments.First() : that;

	/// <summary>
	/// 测量两个经纬度的距离，返回单位：米
	/// </summary>
	/// <param name="that">经纬坐标1</param>
	/// <param name="point">经纬坐标2</param>
	/// <returns>返回距离（单位：米）</returns>
	public static double Distance(this Point that, Point point) {
		double radLat1 = (double)(that.Y) * Math.PI / 180d;
		double radLng1 = (double)(that.X) * Math.PI / 180d;
		double radLat2 = (double)(point.Y) * Math.PI / 180d;
		double radLng2 = (double)(point.X) * Math.PI / 180d;
		return 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin((radLat1 - radLat2) / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin((radLng1 - radLng2) / 2), 2))) * 6378137;
	}

	static ConcurrentDictionary<Type, FieldInfo[]> _dicGetFields = new ConcurrentDictionary<Type, FieldInfo[]>();
	public static object GetEnum<T>(this IDataReader dr, int index) {
		var value = dr.GetString(index);
		var t = typeof(T);
		var fs = _dicGetFields.GetOrAdd(t, t2 => t2.GetFields());
		foreach (var f in fs)
			if (f.GetCustomAttribute<DescriptionAttribute>()?.Description == value || f.Name == value) return Enum.Parse(t, f.Name, true);
		return null;
	}

	public static string ToDescriptionOrString(this Enum item) {
		string name = item.ToString();
		var desc = item.GetType().GetField(name)?.GetCustomAttribute<DescriptionAttribute>();
		return desc?.Description ?? name;
	}
	public static long ToInt64(this Enum item) {
		return Convert.ToInt64(item);
	}
	public static IEnumerable<T> ToSet<T>(this long value) {
		var ret = new List<T>();
		if (value == 0) return ret;
		var t = typeof(T);
		var fs = _dicGetFields.GetOrAdd(t, t2 => t2.GetFields());
		foreach (var f in fs) {
			if (f.FieldType != t) continue;
			object o = Enum.Parse(t, f.Name, true);
			long v = (long)o;
			if ((value & v) == v) ret.Add((T)o);
		}
		return ret;
	}
}