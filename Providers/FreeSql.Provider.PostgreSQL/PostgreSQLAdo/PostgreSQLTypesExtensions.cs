#if !NET40
using Npgsql.LegacyPostgis;
#endif
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

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

#if !NET40
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
#endif

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

#if !NET40
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
#endif

#if NET40
    private static readonly Regex NpgsqlPointRegex = new Regex("\\((-?\\d+.?\\d*),(-?\\d+.?\\d*)\\)");
    public static NpgsqlPoint NpgsqlPointParse(string s)
    {
        Match i = NpgsqlPointRegex.Match(s);
        if (!i.Success)
        {
            throw new FormatException("Not a valid point: " + s);
        }
        return new NpgsqlPoint(float.Parse(i.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), float.Parse(i.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
    }

    private static readonly Regex NpgsqlLSegRegex = new Regex("\\[\\((-?\\d+.?\\d*),(-?\\d+.?\\d*)\\),\\((-?\\d+.?\\d*),(-?\\d+.?\\d*)\\)\\]");
    public static NpgsqlLSeg NpgsqlLSegParse(string s)
    {
        Match i = NpgsqlLSegRegex.Match(s);
        if (!i.Success)
        {
            throw new FormatException("Not a valid line: " + s);
        }
        return new NpgsqlLSeg(new NpgsqlPoint(float.Parse(i.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), float.Parse(i.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)), new NpgsqlPoint(float.Parse(i.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), float.Parse(i.Groups[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)));
    }

    private static readonly Regex NpgsqlBoxRegex = new Regex("\\((-?\\d+.?\\d*),(-?\\d+.?\\d*)\\),\\((-?\\d+.?\\d*),(-?\\d+.?\\d*)\\)");
    public static NpgsqlBox NpgsqlBoxParse(string s)
    {
        Match i = NpgsqlBoxRegex.Match(s);
        return new NpgsqlBox(new NpgsqlPoint(float.Parse(i.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), float.Parse(i.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)), new NpgsqlPoint(float.Parse(i.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), float.Parse(i.Groups[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)));
    }

    public static NpgsqlPath NpgsqlPathParse(string s)
    {
        bool open;
        switch (s[0])
        {
            case '[':
                open = true;
                break;
            case '(':
                open = false;
                break;
            default:
                throw new Exception("Invalid path string: " + s);
        }
        NpgsqlPath result = new NpgsqlPath(open);
        int j = 1;
        while (true)
        {
            int i2 = s.IndexOf(')', j);
            result.Add(NpgsqlPointParse(s.Substring(j, i2 - j + 1)));
            if (s[i2 + 1] != ',')
            {
                break;
            }
            j = i2 + 2;
        }
        return result;
    }

    public static NpgsqlPolygon NpgsqlPolygonParse(string s)
    {
        List<NpgsqlPoint> points = new List<NpgsqlPoint>();
        int j = 1;
        while (true)
        {
            int i2 = s.IndexOf(')', j);
            points.Add(NpgsqlPointParse(s.Substring(j, i2 - j + 1)));
            if (s[i2 + 1] != ',')
            {
                break;
            }
            j = i2 + 2;
        }
        return new NpgsqlPolygon(points);
    }

    private static readonly Regex NpgsqlCircleRegex = new Regex("<\\((-?\\d+.?\\d*),(-?\\d+.?\\d*)\\),(\\d+.?\\d*)>");
    public static NpgsqlCircle NpgsqlCircleParse(string s)
    {
        Match i = NpgsqlCircleRegex.Match(s);
        if (!i.Success)
        {
            throw new FormatException("Not a valid circle: " + s);
        }
        return new NpgsqlCircle(new NpgsqlPoint(float.Parse(i.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), float.Parse(i.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)), float.Parse(i.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
    }
#endif
}