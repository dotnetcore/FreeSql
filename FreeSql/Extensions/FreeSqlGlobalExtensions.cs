using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;

public static class FreeSqlGlobalExtensions {

	static Lazy<Dictionary<Type, bool>> dicIsNumberType = new Lazy<Dictionary<Type, bool>>(() => new Dictionary<Type, bool> {
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
		[typeof(decimal)] = true
	});
	public static bool IsNumberType(this Type that) => that == null ? false : dicIsNumberType.Value.ContainsKey(that.GenericTypeArguments.FirstOrDefault() ?? that);

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

	public static object GetEnum<T>(this IDataReader dr, int index) {
		string value = dr.GetString(index);
		Type t = typeof(T);
		foreach (var f in t.GetFields())
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
		List<T> ret = new List<T>();
		if (value == 0) return ret;
		Type t = typeof(T);
		foreach (FieldInfo f in t.GetFields()) {
			if (f.FieldType != t) continue;
			object o = Enum.Parse(t, f.Name, true);
			long v = (long)o;
			if ((value & v) == v) ret.Add((T)o);
		}
		return ret;
	}
}