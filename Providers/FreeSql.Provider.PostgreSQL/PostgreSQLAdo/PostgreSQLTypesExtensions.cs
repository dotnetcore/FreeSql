using Npgsql.LegacyPostgis;
using NpgsqlTypes;
using System;
using System.Collections;

public static partial class PostgreSQLTypesExtensions
{
    /// <summary>
    /// 测量两个经纬度的距离，返回单位：米
    /// </summary>
    /// <param name="that">经纬坐标1</param>
    /// <param name="point">经纬坐标2</param>
    /// <returns>返回距离（单位：米）</returns>
    public static double Distance(this NpgsqlPoint that, NpgsqlPoint point)
    {
        double radLat1 = (double)(that.Y) * Math.PI / 180d;
        double radLng1 = (double)(that.X) * Math.PI / 180d;
        double radLat2 = (double)(point.Y) * Math.PI / 180d;
        double radLng2 = (double)(point.X) * Math.PI / 180d;
        return 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin((radLat1 - radLat2) / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin((radLng1 - radLng2) / 2), 2))) * 6378137;
    }

    /// <summary>
    /// 测量两个经纬度的距离，返回单位：米
    /// </summary>
    /// <param name="that">经纬坐标1</param>
    /// <param name="point">经纬坐标2</param>
    /// <returns>返回距离（单位：米）</returns>
    public static double Distance(this PostgisPoint that, PostgisPoint point)
    {
        double radLat1 = (double)(that.Y) * Math.PI / 180d;
        double radLng1 = (double)(that.X) * Math.PI / 180d;
        double radLat2 = (double)(point.Y) * Math.PI / 180d;
        double radLng2 = (double)(point.X) * Math.PI / 180d;
        return 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin((radLat1 - radLat2) / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin((radLng1 - radLng2) / 2), 2))) * 6378137;
    }

    public static string To1010(this BitArray ba)
    {
        char[] ret = new char[ba.Length];
        for (int a = 0; a < ba.Length; a++) ret[a] = ba[a] ? '1' : '0';
        return new string(ret);
    }

    /// <summary>
    /// 将 1010101010 这样的二进制字符串转换成 BitArray
    /// </summary>
    /// <param name="_1010Str">1010101010</param>
    /// <returns></returns>
    public static BitArray ToBitArray(this string _1010Str)
    {
        if (_1010Str == null) return null;
        BitArray ret = new BitArray(_1010Str.Length);
        for (int a = 0; a < _1010Str.Length; a++) ret[a] = _1010Str[a] == '1';
        return ret;
    }

    public static NpgsqlRange<T> ToNpgsqlRange<T>(this string that)
    {
        var s = that;
        if (string.IsNullOrEmpty(s) || s == "empty") return NpgsqlRange<T>.Empty;
        string s1 = s.Trim('(', ')', '[', ']');
        string[] ss = s1.Split(new char[] { ',' }, 2);
        if (ss.Length != 2) return NpgsqlRange<T>.Empty;
        T t1 = default(T);
        T t2 = default(T);
        if (!string.IsNullOrEmpty(ss[0])) t1 = (T)Convert.ChangeType(ss[0], typeof(T));
        if (!string.IsNullOrEmpty(ss[1])) t2 = (T)Convert.ChangeType(ss[1], typeof(T));
        return new NpgsqlRange<T>(t1, s[0] == '[', s[0] == '(', t2, s[s.Length - 1] == ']', s[s.Length - 1] == ')');
    }
}