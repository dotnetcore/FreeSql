using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;

public static class MySqlAdoExtensions {
	public static object GetEnum<T>(this IDataReader dr, int index) {
		string value = dr.GetString(index);
		Type t = typeof(T);
		foreach (var f in t.GetFields())
			if (f.GetCustomAttribute<DescriptionAttribute>()?.Description == value || f.Name == value) return Enum.Parse(t, f.Name);
		return null;
	}

	public static string ToDescriptionOrString(this Enum item) {
		string name = item.ToString();
		DescriptionAttribute desc = item.GetType().GetField(name)?.GetCustomAttribute<DescriptionAttribute>();
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
			object o = Enum.Parse(t, f.Name);
			long v = (long) o;
			if ((value & v) == v) ret.Add((T) o);
		}
		return ret;
	}
}