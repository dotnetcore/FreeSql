using FreeSql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;

public static partial class FreeSqlGlobalExtensions
{
    static Lazy<Dictionary<Type, bool>> dicIsNumberType = new Lazy<Dictionary<Type, bool>>(() => new Dictionary<Type, bool>
    {
        [typeof(sbyte)] = true,
        [typeof(sbyte?)] = true,
        [typeof(short)] = true,
        [typeof(short?)] = true,
        [typeof(int)] = true,
        [typeof(int?)] = true,
        [typeof(long)] = true,
        [typeof(long?)] = true,
        [typeof(byte)] = true,
        [typeof(byte?)] = true,
        [typeof(ushort)] = true,
        [typeof(ushort?)] = true,
        [typeof(uint)] = true,
        [typeof(uint?)] = true,
        [typeof(ulong)] = true,
        [typeof(ulong?)] = true,
        [typeof(double)] = true,
        [typeof(double?)] = true,
        [typeof(float)] = true,
        [typeof(float?)] = true,
        [typeof(decimal)] = true,
        [typeof(decimal?)] = true
    });
    public static bool IsNumberType(this Type that) => that == null ? false : dicIsNumberType.Value.ContainsKey(that);
    public static bool IsNullableType(this Type that) => that?.FullName.StartsWith("System.Nullable`1[") == true;
    public static bool IsAnonymousType(this Type that) => that?.FullName.StartsWith("<>f__AnonymousType") == true;
    public static Type NullableTypeOrThis(this Type that) => that?.IsNullableType() == true ? that.GenericTypeArguments.First() : that;
    internal static string NotNullAndConcat(this string that, params object[] args) => string.IsNullOrEmpty(that) ? null : string.Concat(new object[] { that }.Concat(args));

    /// <summary>
    /// 测量两个经纬度的距离，返回单位：米
    /// </summary>
    /// <param name="that">经纬坐标1</param>
    /// <param name="point">经纬坐标2</param>
    /// <returns>返回距离（单位：米）</returns>
    public static double Distance(this Point that, Point point)
    {
        double radLat1 = (double)(that.Y) * Math.PI / 180d;
        double radLng1 = (double)(that.X) * Math.PI / 180d;
        double radLat2 = (double)(point.Y) * Math.PI / 180d;
        double radLng2 = (double)(point.X) * Math.PI / 180d;
        return 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin((radLat1 - radLat2) / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin((radLng1 - radLng2) / 2), 2))) * 6378137;
    }

    static ConcurrentDictionary<Type, FieldInfo[]> _dicGetFields = new ConcurrentDictionary<Type, FieldInfo[]>();
    public static object GetEnum<T>(this IDataReader dr, int index)
    {
        var value = dr.GetString(index);
        var t = typeof(T);
        var fs = _dicGetFields.GetOrAdd(t, t2 => t2.GetFields());
        foreach (var f in fs)
            if (f.GetCustomAttribute<DescriptionAttribute>()?.Description == value || f.Name == value) return Enum.Parse(t, f.Name, true);
        return null;
    }

    public static string ToDescriptionOrString(this Enum item)
    {
        string name = item.ToString();
        var desc = item.GetType().GetField(name)?.GetCustomAttribute<DescriptionAttribute>();
        return desc?.Description ?? name;
    }
    public static long ToInt64(this Enum item)
    {
        return Convert.ToInt64(item);
    }
    public static IEnumerable<T> ToSet<T>(this long value)
    {
        var ret = new List<T>();
        if (value == 0) return ret;
        var t = typeof(T);
        var fs = _dicGetFields.GetOrAdd(t, t2 => t2.GetFields());
        foreach (var f in fs)
        {
            if (f.FieldType != t) continue;
            object o = Enum.Parse(t, f.Name, true);
            long v = (long)o;
            if ((value & v) == v) ret.Add((T)o);
        }
        return ret;
    }

    /// <summary>
    /// 将 IEnumable&lt;T&gt; 转成 ISelect&lt;T&gt;，以便使用 FreeSql 的查询功能。此方法用于 Lambad 表达式中，快速进行集合导航的查询。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="that"></param>
    /// <returns></returns>
    public static ISelect<TEntity> AsSelect<TEntity>(this IEnumerable<TEntity> that) where TEntity : class
    {
        throw new NotImplementedException();
    }
    public static ISelect<TEntity> AsSelect<TEntity>(this IEnumerable<TEntity> that, IFreeSql orm = null) where TEntity : class
    {
        return orm?.Select<TEntity>();
    }

    public static FreeSql.ISelect<T> Queryable<T>(this IFreeSql freesql) where T : class => freesql.Select<T>();

    #region 多表查询
    /// <summary>
    /// 多表查询
    /// </summary>
    /// <returns></returns>
    public static ISelect<T1, T2> Select<T1, T2>(this IFreeSql freesql) where T1 : class where T2 : class =>
        freesql.Select<T1>().From<T2>((s, b) => s);
    /// <summary>
    /// 多表查询
    /// </summary>
    /// <returns></returns>
    public static ISelect<T1, T2, T3> Select<T1, T2, T3>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class =>
        freesql.Select<T1>().From<T2, T3>((s, b, c) => s);
    /// <summary>
    /// 多表查询
    /// </summary>
    /// <returns></returns>
    public static ISelect<T1, T2, T3, T4> Select<T1, T2, T3, T4>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class =>
        freesql.Select<T1>().From<T2, T3, T4>((s, b, c, d) => s);
    /// <summary>
    /// 多表查询
    /// </summary>
    /// <returns></returns>
    public static ISelect<T1, T2, T3, T4, T5> Select<T1, T2, T3, T4, T5>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5>((s, b, c, d, e) => s);
    /// <summary>
    /// 多表查询
    /// </summary>
    /// <returns></returns>
    public static ISelect<T1, T2, T3, T4, T5, T6> Select<T1, T2, T3, T4, T5, T6>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6>((s, b, c, d, e, f) => s);
    /// <summary>
    /// 多表查询
    /// </summary>
    /// <returns></returns>
    public static ISelect<T1, T2, T3, T4, T5, T6, T7> Select<T1, T2, T3, T4, T5, T6, T7>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6, T7>((s, b, c, d, e, f, g) => s);
    /// <summary>
    /// 多表查询
    /// </summary>
    /// <returns></returns>
    public static ISelect<T1, T2, T3, T4, T5, T6, T7, T8> Select<T1, T2, T3, T4, T5, T6, T7, T8>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6, T7, T8>((s, b, c, d, e, f, g, h) => s);
    /// <summary>
    /// 多表查询
    /// </summary>
    /// <returns></returns>
    public static ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6, T7, T8, T9>((s, b, c, d, e, f, g, h, i) => s);
    /// <summary>
    /// 多表查询
    /// </summary>
    /// <returns></returns>
    public static ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6, T7, T8, T9, T10>((s, b, c, d, e, f, g, h, i, j) => s);
    #endregion
}