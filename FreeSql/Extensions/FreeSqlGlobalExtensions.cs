using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

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
        [typeof(double)] = false,
        [typeof(double?)] = false,
        [typeof(float)] = false,
        [typeof(float?)] = false,
        [typeof(decimal)] = false,
        [typeof(decimal?)] = false
    });
    public static bool IsIntegerType(this Type that) => that == null ? false : (dicIsNumberType.Value.TryGetValue(that, out var tryval) ? tryval : false);
    public static bool IsNumberType(this Type that) => that == null ? false : dicIsNumberType.Value.ContainsKey(that);
    public static bool IsNullableType(this Type that) => that.IsArray == false && that?.FullName.StartsWith("System.Nullable`1[") == true;
    public static bool IsAnonymousType(this Type that) => that?.FullName.StartsWith("<>f__AnonymousType") == true;
    public static bool IsArrayOrList(this Type that) => that == null ? false : (that.IsArray || typeof(IList).IsAssignableFrom(that));
    public static Type NullableTypeOrThis(this Type that) => that?.IsNullableType() == true ? that.GetGenericArguments().First() : that;
    internal static string NotNullAndConcat(this string that, params object[] args) => string.IsNullOrEmpty(that) ? null : string.Concat(new object[] { that }.Concat(args));
    public static object CreateInstanceGetDefaultValue(this Type that)
    {
        if (that == null) return null;
        if (that == typeof(string)) return default(string);
        if (that.IsArray) return Array.CreateInstance(that, 0);
        var ctorParms = that.InternalGetTypeConstructor0OrFirst(false)?.GetParameters();
        if (ctorParms == null || ctorParms.Any() == false) return Activator.CreateInstance(that, null);
        return Activator.CreateInstance(that, ctorParms.Select(a => Activator.CreateInstance(a.ParameterType, null)).ToArray());
    }
    internal static NewExpression InternalNewExpression(this Type that)
    {
        var ctor = that.InternalGetTypeConstructor0OrFirst();
        return Expression.New(ctor, ctor.GetParameters().Select(a => Expression.Constant(a.ParameterType.CreateInstanceGetDefaultValue(), a.ParameterType)));
    }

    static ConcurrentDictionary<Type, ConstructorInfo> _dicInternalGetTypeConstructor0OrFirst = new ConcurrentDictionary<Type, ConstructorInfo>();
    internal static ConstructorInfo InternalGetTypeConstructor0OrFirst(this Type that, bool isThrow = true)
    {
        var ret = _dicInternalGetTypeConstructor0OrFirst.GetOrAdd(that, tp =>
            tp.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null) ??
            tp.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault());
        if (ret == null && isThrow) throw new ArgumentException($"{that.FullName} 类型无方法访问构造函数");
        return ret;
    }

    static ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _dicGetPropertiesDictIgnoreCase = new ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>>();
    public static Dictionary<string, PropertyInfo> GetPropertiesDictIgnoreCase(this Type that) => that == null ? null : _dicGetPropertiesDictIgnoreCase.GetOrAdd(that, tp =>
    {
        var props = that.GetProperties().GroupBy(p => p.DeclaringType).Reverse().SelectMany(p => p); //将基类的属性位置放在前面 #164
        var dict = new Dictionary<string, PropertyInfo>(StringComparer.CurrentCultureIgnoreCase);
        foreach (var prop in props)
        {
            if (dict.ContainsKey(prop.Name)) continue;
            dict.Add(prop.Name, prop);
        }
        return dict;
    });

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
        foreach (var f in fs) {
            var attr = f.GetCustomAttributes(typeof(DescriptionAttribute), false)?.FirstOrDefault() as DescriptionAttribute;
            if (attr?.Description == value || f.Name == value) return Enum.Parse(t, f.Name, true);
        }
        return null;
    }

    public static string ToDescriptionOrString(this Enum item)
    {
        string name = item.ToString();
        var desc = item.GetType().GetField(name)?.GetCustomAttributes(typeof(DescriptionAttribute), false)?.FirstOrDefault() as DescriptionAttribute;
        return desc?.Description ?? name;
    }
    public static long ToInt64(this Enum item) => Convert.ToInt64(item);
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
    public static ISelect<TEntity> AsSelect<TEntity>(this IEnumerable<TEntity> that) where TEntity : class => throw new NotImplementedException();
    public static ISelect<TEntity> AsSelect<TEntity>(this IEnumerable<TEntity> that, IFreeSql orm = null) where TEntity : class => orm?.Select<TEntity>();
    public static ISelect<T> Queryable<T>(this IFreeSql freesql) where T : class => freesql.Select<T>();

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

    #region IncludeMany
    /// <summary>
    /// 本方法实现从已知的内存 List 数据，进行和 ISelect.IncludeMany 相同功能的贪婪加载<para></para>
    /// 示例：new List&lt;Song&gt;(new[] { song1, song2, song3 }).IncludeMany(g.sqlite, a => a.Tags);<para></para>
    /// 文档：https://github.com/2881099/FreeSql/wiki/%e8%b4%aa%e5%a9%aa%e5%8a%a0%e8%bd%bd#%E5%AF%BC%E8%88%AA%E5%B1%9E%E6%80%A7-onetomanymanytomany
    /// </summary>
    /// <typeparam name="TNavigate"></typeparam>
    /// <param name="navigateSelector">选择一个集合的导航属性，如： .IncludeMany(a => a.Tags)<para></para>
    /// 可以 .Where 设置临时的关系映射，如： .IncludeMany(a => a.Tags.Where(tag => tag.TypeId == a.Id))<para></para>
    /// 可以 .Take(5) 每个子集合只取5条，如： .IncludeMany(a => a.Tags.Take(5))<para></para>
    /// 可以 .Select 设置只查询部分字段，如： (a => new TNavigate { Title = a.Title }) 
    /// </param>
    /// <param name="then">即能 ThenInclude，还可以二次过滤（这个 EFCore 做不到？）</param>
    /// <returns></returns>
    public static List<T1> IncludeMany<T1, TNavigate>(this List<T1> list, IFreeSql orm, Expression<Func<T1, IEnumerable<TNavigate>>> navigateSelector, Action<ISelect<TNavigate>> then = null) where T1 : class where TNavigate : class
    {
        if (list == null || list.Any() == false) return list;
        var select = orm.Select<T1>().IncludeMany(navigateSelector, then) as FreeSql.Internal.CommonProvider.Select1Provider<T1>;
        select.SetList(list);
        return list;
    }

#if net40
#else
    async public static System.Threading.Tasks.Task<List<T1>> IncludeManyAsync<T1, TNavigate>(this List<T1> list, IFreeSql orm, Expression<Func<T1, IEnumerable<TNavigate>>> navigateSelector, Action<ISelect<TNavigate>> then = null) where T1 : class where TNavigate : class
    {
        if (list == null || list.Any() == false) return list;
        var select = orm.Select<T1>().IncludeMany(navigateSelector, then) as FreeSql.Internal.CommonProvider.Select1Provider<T1>;
        await select.SetListAsync(list);
        return list;
    }
#endif
    #endregion
}