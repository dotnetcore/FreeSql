using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public static partial class FreeSqlGlobalExtensions
{
#if net40
#else
    static readonly Lazy<PropertyInfo> _TaskReflectionResultPropertyLazy = new Lazy<PropertyInfo>(() => typeof(Task).GetProperty("Result"));
    internal static object GetTaskReflectionResult(this Task task) => _TaskReflectionResultPropertyLazy.Value.GetValue(task, new object[0]);
#endif

    #region Type 对象扩展方法
    static Lazy<Dictionary<Type, bool>> _dicIsNumberType = new Lazy<Dictionary<Type, bool>>(() => new Dictionary<Type, bool>
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
    public static bool IsIntegerType(this Type that) => that == null ? false : (_dicIsNumberType.Value.TryGetValue(that, out var tryval) ? tryval : false);
    public static bool IsNumberType(this Type that) => that == null ? false : _dicIsNumberType.Value.ContainsKey(that);
    public static bool IsNullableType(this Type that) => that == null ? false : (that.IsArray == false && that.FullName.StartsWith("System.Nullable`1[") == true);
    public static bool IsAnonymousType(this Type that) => that == null ? false : (that.FullName.StartsWith("<>f__AnonymousType") || that.FullName.StartsWith("VB$AnonymousType"));
    public static bool IsArrayOrList(this Type that) => that == null ? false : (that.IsArray || typeof(IList).IsAssignableFrom(that));
    public static Type NullableTypeOrThis(this Type that) => that?.IsNullableType() == true ? that.GetGenericArguments().First() : that;
    internal static string NotNullAndConcat(this string that, params object[] args) => string.IsNullOrEmpty(that) ? null : string.Concat(new object[] { that }.Concat(args));
    /// <summary>
    /// 获取 Type 的原始 c# 文本表示
    /// </summary>
    /// <param name="type"></param>
    /// <param name="isNameSpace"></param>
    /// <returns></returns>
    public static string DisplayCsharp(this Type type, bool isNameSpace = true)
    {
        if (type == null) return null;
        if (type == typeof(void)) return "void";
        if (type.IsGenericParameter) return type.Name;
        if (type.IsArray) return $"{DisplayCsharp(type.GetElementType())}[]";
        var sb = new StringBuilder();
        var nestedType = type;
        while (nestedType.IsNested)
        {
            sb.Insert(0, ".").Insert(0, DisplayCsharp(nestedType.DeclaringType, false));
            nestedType = nestedType.DeclaringType;
        }
        if (isNameSpace && string.IsNullOrEmpty(nestedType.Namespace) == false)
            sb.Insert(0, ".").Insert(0, nestedType.Namespace);

        if (type.IsGenericType == false)
            return sb.Append(type.Name).ToString();

        var genericParameters = type.GetGenericArguments();
        if (type.IsNested && type.DeclaringType.IsGenericType)
        {
            var dic = genericParameters.ToDictionary(a => a.Name);
            foreach (var nestedGenericParameter in type.DeclaringType.GetGenericArguments())
                if (dic.ContainsKey(nestedGenericParameter.Name))
                    dic.Remove(nestedGenericParameter.Name);
            genericParameters = dic.Values.ToArray();
        }
        if (genericParameters.Any() == false)
            return sb.Append(type.Name).ToString();

        sb.Append(type.Name.Remove(type.Name.IndexOf('`'))).Append('<');
        var genericTypeIndex = 0;
        foreach (var genericType in genericParameters)
        {
            if (genericTypeIndex++ > 0) sb.Append(", ");
            sb.Append(DisplayCsharp(genericType, true));
        }
        return sb.Append('>').ToString();
    }
    internal static string DisplayCsharp(this MethodInfo method, bool isOverride)
    {
        if (method == null) return null;
        var sb = new StringBuilder();
        if (method.IsPublic) sb.Append("public ");
        if (method.IsAssembly) sb.Append("internal ");
        if (method.IsFamily) sb.Append("protected ");
        if (method.IsPrivate) sb.Append("private ");
        if (method.IsPrivate) sb.Append("private ");
        if (method.IsStatic) sb.Append("static ");
        if (method.IsAbstract && method.DeclaringType.IsInterface == false) sb.Append("abstract ");
        if (method.IsVirtual && method.DeclaringType.IsInterface == false) sb.Append(isOverride ? "override " : "virtual ");
        sb.Append(method.ReturnType.DisplayCsharp()).Append(' ').Append(method.Name);

        var genericParameters = method.GetGenericArguments();
        if (method.DeclaringType.IsNested && method.DeclaringType.DeclaringType.IsGenericType)
        {
            var dic = genericParameters.ToDictionary(a => a.Name);
            foreach (var nestedGenericParameter in method.DeclaringType.DeclaringType.GetGenericArguments())
                if (dic.ContainsKey(nestedGenericParameter.Name))
                    dic.Remove(nestedGenericParameter.Name);
            genericParameters = dic.Values.ToArray();
        }
        if (genericParameters.Any())
            sb.Append('<')
                .Append(string.Join(", ", genericParameters.Select(a => a.DisplayCsharp())))
                .Append('>');

        sb.Append('(').Append(string.Join(", ", method.GetParameters().Select(a => $"{a.ParameterType.DisplayCsharp()} {a.Name}"))).Append(')');
        return sb.ToString();
    }
    public static object CreateInstanceGetDefaultValue(this Type that)
    {
        if (that == null) return null;
        if (that == typeof(string)) return default(string);
        if (that == typeof(Guid)) return default(Guid);
        if (that == typeof(byte[])) return default(byte[]);
        if (that.IsArray) return Array.CreateInstance(that.GetElementType(), 0);
        if (that.IsInterface || that.IsAbstract) return null;
        var ctorParms = that.InternalGetTypeConstructor0OrFirst(false)?.GetParameters();
        if (ctorParms == null || ctorParms.Any() == false) return Activator.CreateInstance(that, true);
        return Activator.CreateInstance(that, ctorParms
            .Select(a => a.ParameterType.IsInterface || a.ParameterType.IsAbstract || a.ParameterType == typeof(string) || a.ParameterType.IsArray ?
            null :
            Activator.CreateInstance(a.ParameterType, null)).ToArray());
    }
    internal static NewExpression InternalNewExpression(this Type that)
    {
        var ctor = that.InternalGetTypeConstructor0OrFirst();
        return Expression.New(ctor, ctor.GetParameters().Select(a => Expression.Constant(a.ParameterType.CreateInstanceGetDefaultValue(), a.ParameterType)));
    }

    static ConcurrentDictionary<Type, Lazy<ConstructorInfo>> _dicInternalGetTypeConstructor0OrFirst = new ConcurrentDictionary<Type, Lazy<ConstructorInfo>>();
    internal static ConstructorInfo InternalGetTypeConstructor0OrFirst(this Type that, bool isThrow = true)
    {
        var ret = _dicInternalGetTypeConstructor0OrFirst.GetOrAdd(that, tp =>
            new Lazy<ConstructorInfo>(() =>
            {
                return tp.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null) ??
                    tp.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .OrderBy(a => a.IsPublic ? 0 : 1)
                    .FirstOrDefault();
            }));
        if (ret.Value == null && isThrow) throw new ArgumentException(CoreStrings.Type_Cannot_Access_Constructor(that.FullName));
        return ret.Value;
    }

    static ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _dicGetPropertiesDictIgnoreCase = new ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>>();
    public static Dictionary<string, PropertyInfo> GetPropertiesDictIgnoreCase(this Type that) => that == null ? null : _dicGetPropertiesDictIgnoreCase.GetOrAdd(that, tp =>
    {
        var props = that.GetProperties().GroupBy(p => p.DeclaringType).Reverse().SelectMany(p => p); //将基类的属性位置放在前面 #164
        var dict = new Dictionary<string, PropertyInfo>(StringComparer.CurrentCultureIgnoreCase);
        foreach (var prop in props)
        {
            if (dict.TryGetValue(prop.Name, out var existsProp))
            {
                if (existsProp.DeclaringType != prop.DeclaringType) dict[prop.Name] = prop;
                continue;
            }
            dict.Add(prop.Name, prop);
        }
        return dict;
    });
    #endregion

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

    #region Enum 对象扩展方法
    static ConcurrentDictionary<Type, FieldInfo[]> _dicGetFields = new ConcurrentDictionary<Type, FieldInfo[]>();
    public static object GetEnum<T>(this IDataReader dr, int index)
    {
        var value = dr.GetString(index);
        var t = typeof(T);
        var fs = _dicGetFields.GetOrAdd(t, t2 => t2.GetFields());
        foreach (var f in fs)
        {
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
    #endregion

    /// <summary>
    /// 将 IEnumable&lt;T&gt; 转成 ISelect&lt;T&gt;，以便使用 FreeSql 的查询功能。此方法用于 Lambda 表达式中，快速进行集合导航的查询。
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
    public static ISelect<T1, T2, T3> Select<T1, T2, T3>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class =>
        freesql.Select<T1>().From<T2, T3>((s, b, c) => s);
    public static ISelect<T1, T2, T3, T4> Select<T1, T2, T3, T4>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class =>
        freesql.Select<T1>().From<T2, T3, T4>((s, b, c, d) => s);
    public static ISelect<T1, T2, T3, T4, T5> Select<T1, T2, T3, T4, T5>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5>((s, b, c, d, e) => s);
    public static ISelect<T1, T2, T3, T4, T5, T6> Select<T1, T2, T3, T4, T5, T6>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6>((s, b, c, d, e, f) => s);
    public static ISelect<T1, T2, T3, T4, T5, T6, T7> Select<T1, T2, T3, T4, T5, T6, T7>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6, T7>((s, b, c, d, e, f, g) => s);
    public static ISelect<T1, T2, T3, T4, T5, T6, T7, T8> Select<T1, T2, T3, T4, T5, T6, T7, T8>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6, T7, T8>((s, b, c, d, e, f, g, h) => s);
    public static ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6, T7, T8, T9>((s, b, c, d, e, f, g, h, i) => s);
    public static ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6, T7, T8, T9, T10>((s, b, c, d, e, f, g, h, i, j) => s);

    public static ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>((s, b, c, d, e, f, g, h, i, j, k) => s);
    public static ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>((s, b, c, d, e, f, g, h, i, j, k, l) => s);
    public static ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>((s, b, c, d, e, f, g, h, i, j, k, l, m) => s);
    public static ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>((s, b, c, d, e, f, g, h, i, j, k, l, m, n) => s);
    public static ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>((s, b, c, d, e, f, g, h, i, j, k, l, m, n, o) => s);
    public static ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this IFreeSql freesql) where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class where T16 : class =>
        freesql.Select<T1>().From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>((s, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => s);
    #endregion

    #region IncludeMany
    /// <summary>
    /// 本方法实现从已知的内存 List 数据，进行和 ISelect.IncludeMany 相同功能的贪婪加载<para></para>
    /// 示例：new List&lt;Song&gt;(new[] { song1, song2, song3 }).IncludeMany(fsql, a => a.Tags);<para></para>
    /// 文档：https://github.com/dotnetcore/FreeSql/wiki/%E8%B4%AA%E5%A9%AA%E5%8A%A0%E8%BD%BD
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="TNavigate"></typeparam>
    /// <param name="list"></param>
    /// <param name="orm"></param>
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
        if (orm.CodeFirst.IsAutoSyncStructure)
        {
            var tb = orm.CodeFirst.GetTableByEntity(typeof(T1));
            if (tb == null || tb.Primarys.Any() == false)
                (orm.CodeFirst as CodeFirstProvider)._dicSycedTryAdd(typeof(T1)); //._dicSyced.TryAdd(typeof(TReturn), true);
        }
        var select = orm.Select<T1>().IncludeMany(navigateSelector, then) as Select1Provider<T1>;
        select.SetList(list);
        return list;
    }
#if net40
#else
    async public static Task<List<T1>> IncludeManyAsync<T1, TNavigate>(this List<T1> list, IFreeSql orm, Expression<Func<T1, IEnumerable<TNavigate>>> navigateSelector, Action<ISelect<TNavigate>> then = null, CancellationToken cancellationToken = default) where T1 : class where TNavigate : class
    {
        if (list == null || list.Any() == false) return list;
        if (orm.CodeFirst.IsAutoSyncStructure)
        {
            var tb = orm.CodeFirst.GetTableByEntity(typeof(T1));
            if (tb == null || tb.Primarys.Any() == false)
                (orm.CodeFirst as CodeFirstProvider)._dicSycedTryAdd(typeof(T1)); //._dicSyced.TryAdd(typeof(TReturn), true);
        }
        var select = orm.Select<T1>().IncludeMany(navigateSelector, then) as Select1Provider<T1>;
        await select.SetListAsync(list, cancellationToken);
        return list;
    }
#endif
    /// <summary>
    /// 本方法实现从已知的内存 List 数据，进行和 ISelect.IncludeMany/Include 相同功能的贪婪加载<para></para>
    /// 集合：new List&lt;Song&gt;(new[] { song1, song2, song3 }).IncludeByPropertyName(fsql, "Tags", "ParentId=Id", 5, "Id,Name");<para></para>
    /// 普通：new List&lt;Song&gt;(new[] { song1, song2, song3 }).IncludeByPropertyName(fsql, "Catetory"); <para></para>
    /// －－－普通属性 where/take/select 参数将无效<para></para>
    /// 文档：https://github.com/dotnetcore/FreeSql/wiki/%E8%B4%AA%E5%A9%AA%E5%8A%A0%E8%BD%BD
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="list"></param>
    /// <param name="orm"></param>
    /// <param name="property">选择一个集合或普通属性</param>
    /// <param name="where">设置临时的子集合关系映射，格式：子类属性=T1属性，多组以逗号分割</param>
    /// <param name="take">设置子集合只取条数</param>
    /// <param name="select">设置子集合只查询部分字段</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static List<T1> IncludeByPropertyName<T1>(this List<T1> list, IFreeSql orm, string property, string where = null, int take = 0, string select = null, Expression<Action<ISelect<object>>> then = null) where T1 : class
    {
#if net40
        return IncludeByPropertyNameSyncOrAsync<T1>(false, list, orm, property, where, take, select, then);
#else
        var task = IncludeByPropertyNameSyncOrAsync<T1>(false, list, orm, property, where, take, select, then);
        if (task.Exception != null) throw task.Exception.InnerException ?? task.Exception;
        return task.Result;
#endif
    }
#if net40
#else
    public static Task<List<T1>> IncludeByPropertyNameAsync<T1>(this List<T1> list, IFreeSql orm, string property, string where = null, int take = 0, string select = null, Expression<Action<ISelect<object>>> then = null) where T1 : class
    {
        return IncludeByPropertyNameSyncOrAsync<T1>(true, list, orm, property, where, take, select, then);
    }
#endif
    static
#if net40
        List<T1>
#else
        async Task<List<T1>>
#endif
        IncludeByPropertyNameSyncOrAsync<T1>(bool isAsync, List<T1> list, IFreeSql orm, string property, string where, int take, string select, Expression<Action<ISelect<object>>> then) where T1 : class
    {
        if (list?.Any() != true) return list;
        var entityType = typeof(T1) == typeof(object) ? list[0].GetType() : typeof(T1);
        var t1tb = orm.CodeFirst.GetTableByEntity(entityType);
        if (orm.CodeFirst.IsAutoSyncStructure)
        {
            if (t1tb == null || t1tb.Primarys.Any() == false)
                (orm.CodeFirst as CodeFirstProvider)._dicSycedTryAdd(entityType); //._dicSyced.TryAdd(typeof(TReturn), true);
        }
        var props = property.Split('.');
        var t1sel = orm.Select<object>().AsType(entityType) as Select1Provider<object>;
        var t1expFul = t1sel.ConvertStringPropertyToExpression(property, true);
        var t1exp = props.Length == 1 ? t1expFul : t1sel.ConvertStringPropertyToExpression(props[0], true);
        if (t1expFul == null) throw new ArgumentException(CoreStrings.Cannot_Resolve_ExpressionTree(nameof(property)));
        var propElementType = t1expFul.Type.GetGenericArguments().FirstOrDefault() ?? t1expFul.Type.GetElementType();
        if (propElementType != null) //IncludeMany
        {
            if (props.Length > 1)
                IncludeByPropertyName(list, orm, string.Join(".", props.Take(props.Length - 1)));
            var imsel = IncludeManyByPropertyNameCommonGetSelect(orm, entityType, property, where, take, select, then);
#if net40
            imsel.SetList(list);
#else
            if (isAsync) await imsel.SetListAsync(list);
            else imsel.SetList(list);
#endif
            return list;
        }
        var tbtr = t1tb.GetTableRef(props[0], true);
        if (tbtr == null) throw new ArgumentException(CoreStrings.ParameterError_NotValid_Navigation(nameof(property)));
        var reftb = orm.CodeFirst.GetTableByEntity(t1exp.Type);
        var refsel = orm.Select<object>().AsType(t1exp.Type) as Select1Provider<object>;
        if (props.Length > 1)
            refsel.IncludeByPropertyName(string.Join(".", props.Skip(1)));

        var listdic = list.Select(item =>
        {
            var refitem = t1exp.Type.CreateInstanceGetDefaultValue();
            for (var a = 0; a < tbtr.Columns.Count; a++)
            {
                var colval = FreeSql.Extensions.EntityUtil.EntityUtilExtensions.GetPropertyValue(t1tb, item, tbtr.Columns[a].CsName);
                FreeSql.Extensions.EntityUtil.EntityUtilExtensions.SetPropertyValue(reftb, refitem, tbtr.RefColumns[a].CsName, colval);
            }
            return new
            {
                item,
                refitem,
                key = FreeSql.Extensions.EntityUtil.EntityUtilExtensions.GetEntityKeyString(orm, reftb.Type, refitem, false)
            };
        }).GroupBy(a => a.key).ToDictionary(a => a.Key, a => a);
        refsel.WhereDynamic(listdic.Values.Select(a => a.First().refitem).ToList());

#if net40
        var reflist = refsel.ToList();
#else
        var reflist = isAsync ? await refsel.ToListAsync() : refsel.ToList();
#endif

        reflist.ForEach(refitem =>
        {
            var key = FreeSql.Extensions.EntityUtil.EntityUtilExtensions.GetEntityKeyString(orm, reftb.Type, refitem, false);
            foreach (var listitem in listdic[key])
                FreeSql.Extensions.EntityUtil.EntityUtilExtensions.SetPropertyValue(t1tb, listitem.item, property, refitem);
        });
        return list;
    }
    static Select1Provider<object> IncludeManyByPropertyNameCommonGetSelect(IFreeSql orm, Type entityType, string property, string where, int take, string select, Expression<Action<ISelect<object>>> then)
    {
        if (orm.CodeFirst.IsAutoSyncStructure)
        {
            var tb = orm.CodeFirst.GetTableByEntity(entityType);
            if (tb == null || tb.Primarys.Any() == false)
                (orm.CodeFirst as CodeFirstProvider)._dicSycedTryAdd(entityType); //._dicSyced.TryAdd(typeof(TReturn), true);
        }
        var sel = orm.Select<object>().AsType(entityType) as Select1Provider<object>;
        var exp = sel.ConvertStringPropertyToExpression(property, true);
        if (exp == null) throw new ArgumentException(CoreStrings.Cannot_Resolve_ExpressionTree(nameof(property)));
        var memExp = exp as MemberExpression;
        if (memExp == null) throw new ArgumentException($"{CoreStrings.Cannot_Resolve_ExpressionTree(nameof(property))}2");
        var parTb = orm.CodeFirst.GetTableByEntity(memExp.Expression.Type);
        if (parTb == null) throw new ArgumentException($"{CoreStrings.Cannot_Resolve_ExpressionTree(nameof(property))}3");
        var propElementType = exp.Type.GetGenericArguments().FirstOrDefault() ?? exp.Type.GetElementType();
        var reftb = orm.CodeFirst.GetTableByEntity(propElementType);
        if (reftb == null) throw new ArgumentException(CoreStrings.ParameterError_NotValid_Collection(nameof(property)));

        if (string.IsNullOrWhiteSpace(where) == false)
        {
            var refparamExp = Expression.Parameter(reftb.Type);
            var reffuncType = typeof(Func<,>).MakeGenericType(reftb.Type, typeof(bool));
            var refWhereMethod = Select0Provider.GetMethodEnumerable("Where").MakeGenericMethod(reftb.Type);

            var whereSplit = where.Split(',');
            Expression whereExp = null;
            for (var a = 0; a < whereSplit.Length; a++)
            {
                var keyval = whereSplit[a].Split('=').Select(x => x.Trim()).Where(x => string.IsNullOrWhiteSpace(x) == false).ToArray();
                if (keyval.Length != 2) throw new ArgumentException(CoreStrings.ParameterError_NotValid_UseCommas(nameof(where)));

                if (reftb.ColumnsByCs.TryGetValue(keyval[0], out var keycol) == false)
                    throw new ArgumentException(CoreStrings.ParameterError_NotValid_PropertyName(nameof(where), keyval[0], reftb.Type.DisplayCsharp()));
                if (parTb.ColumnsByCs.TryGetValue(keyval[1], out var valcol) == false)
                    throw new ArgumentException(CoreStrings.ParameterError_NotValid_PropertyName(nameof(where), keyval[1], parTb.Type.DisplayCsharp()));

                var tmpExp = Expression.Equal(
                    Expression.Convert(Expression.MakeMemberAccess(refparamExp, reftb.Properties[keyval[0]]), valcol.CsType),
                    Expression.MakeMemberAccess(memExp.Expression, parTb.Properties[keyval[1]]));
                whereExp = whereExp == null ? tmpExp : Expression.And(whereExp, tmpExp);
            }
            whereExp = Expression.Lambda(reffuncType, whereExp, refparamExp);
            exp = Expression.Call(refWhereMethod, exp, whereExp);
        }
        if (take > 0)
        {
            var takeMethod = Select0Provider.GetMethodEnumerable("Take").MakeGenericMethod(reftb.Type);
            exp = Expression.Call(takeMethod, exp, Expression.Constant(take, typeof(int)));
        }
        if (select?.Any() == true)
        {
            var refparamExp = Expression.Parameter(reftb.Type);
            var reffuncType = typeof(Func<,>).MakeGenericType(reftb.Type, reftb.Type);
            var refWhereMethod = Select0Provider.GetMethodEnumerable("Select").MakeGenericMethod(reftb.Type, reftb.Type);

            Expression memberInitExp = Expression.MemberInit(
                reftb.Type.InternalNewExpression(),
                select.Split(',').Select(x => x.Trim()).Where(x => string.IsNullOrWhiteSpace(x) == false).Select(a =>
                {
                    if (reftb.ColumnsByCs.TryGetValue(a, out var col) == false)
                        throw new ArgumentException(CoreStrings.ParameterError_NotValid_PropertyName(nameof(select), a, reftb.Type.DisplayCsharp()));
                    return Expression.Bind(reftb.Properties[col.CsName], Expression.MakeMemberAccess(refparamExp, reftb.Properties[col.CsName]));
                }).ToArray());

            memberInitExp = Expression.Lambda(reffuncType, memberInitExp, refparamExp);
            exp = Expression.Call(refWhereMethod, exp, memberInitExp);
        }
        Delegate newthen = null;
        if (then != null)
        {
            var newthenParm = Expression.Parameter(typeof(ISelect<>).MakeGenericType(reftb.Type));
            var newthenLambdaBody = new Select1Provider<object>.ReplaceIncludeByPropertyNameParameterVisitor().Modify(then, newthenParm);
            var newthenLambda = Expression.Lambda(typeof(Action<>).MakeGenericType(newthenParm.Type), newthenLambdaBody, newthenParm);
            newthen = newthenLambda.Compile();
        }

        var funcType = typeof(Func<,>).MakeGenericType(typeof(object), typeof(IEnumerable<>).MakeGenericType(reftb.Type));
        if (sel._tables[0].Table.Type != typeof(object))
        {
            var expParm = Expression.Parameter(typeof(object), sel._tables[0].Alias);
            exp = new Select0Provider.ReplaceMemberExpressionVisitor().Replace(exp, sel._tables[0].Parameter, Expression.Convert(expParm, sel._tables[0].Table.Type));
            sel._tables[0].Parameter = expParm;
        }
        var navigateSelector = Expression.Lambda(funcType, exp, sel._tables[0].Parameter);
        var incMethod = sel.GetType().GetMethod("IncludeMany");
        if (incMethod == null) throw new Exception(CoreStrings.RunTimeError_Reflection_IncludeMany);
        incMethod.MakeGenericMethod(reftb.Type).Invoke(sel, new object[] { navigateSelector, newthen });
        return sel;
    }
    #endregion

    #region ToTreeList() 父子分类
    /// <summary>
    /// 查询数据，加工为树型 List 返回<para></para>
    /// 注意：实体需要配置父子导航属性
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="that"></param>
    /// <returns></returns>
    public static List<T1> ToTreeList<T1>(this ISelect<T1> that) where T1 : class
    {
        var select = that as Select1Provider<T1>;
        var tb = select._tables[0].Table;
        var navs = tb.Properties.Select(a => tb.GetTableRef(a.Key, false))
            .Where(a => a != null &&
                a.RefType == FreeSql.Internal.Model.TableRefType.OneToMany &&
                a.RefEntityType == tb.Type).ToArray();

        var list = select.ToList();
        if (navs.Length != 1) return list;

        select._trackToList = null;
        select._includeToList.Clear();
        var navigateSelectorParamExp = select._tables[0].Parameter ?? Expression.Parameter(typeof(T1), select._tables[0].Alias);
        var navigateSelector = Expression.Lambda<Func<T1, IEnumerable<T1>>>(Expression.MakeMemberAccess(navigateSelectorParamExp, navs[0].Property), navigateSelectorParamExp);
        select.IncludeMany(navigateSelector);
        select._includeManySubListOneToManyTempValue1 = list;
        select.SetList(list);
        return list.Except(list.SelectMany(a => FreeSql.Extensions.EntityUtil.EntityUtilExtensions.GetEntityValueWithPropertyName(select._orm, tb.Type, a, navs[0].Property.Name) as IEnumerable<T1>)).ToList();
    }
#if net40
#else
    async public static Task<List<T1>> ToTreeListAsync<T1>(this ISelect<T1> that, CancellationToken cancellationToken = default) where T1 : class
    {
        var select = that as Select1Provider<T1>;
        var tb = select._tables[0].Table;
        var navs = tb.Properties.Select(a => tb.GetTableRef(a.Key, false))
            .Where(a => a != null &&
                a.RefType == FreeSql.Internal.Model.TableRefType.OneToMany &&
                a.RefEntityType == tb.Type).ToArray();

        var list = await select.ToListAsync(false, cancellationToken);
        if (navs.Length != 1) return list;

        select._trackToList = null;
        select._includeToList.Clear();
        var navigateSelectorParamExp = select._tables[0].Parameter ?? Expression.Parameter(typeof(T1), select._tables[0].Alias);
        var navigateSelector = Expression.Lambda<Func<T1, IEnumerable<T1>>>(Expression.MakeMemberAccess(navigateSelectorParamExp, navs[0].Property), navigateSelectorParamExp);
        select.IncludeMany(navigateSelector);
        select._includeManySubListOneToManyTempValue1 = list;
        await select.SetListAsync(list, cancellationToken);
        return list.Except(list.SelectMany(a => FreeSql.Extensions.EntityUtil.EntityUtilExtensions.GetEntityValueWithPropertyName(select._orm, tb.Type, a, navs[0].Property.Name) as IEnumerable<T1>)).ToList();
    }
#endif
    #endregion

    #region AsTreeCte(..) 递归查询
    static ConcurrentDictionary<string, string> _dicMySqlVersion = new ConcurrentDictionary<string, string>();
    /// <summary>
    /// 使用递归 CTE 查询树型的所有子记录，或者所有父记录。<para></para>
    /// 通过测试的数据库：MySql8.0、SqlServer、PostgreSQL、Oracle、Sqlite、Firebird、达梦、人大金仓、翰高<para></para>
    /// 返回隐藏字段：.ToList(a =&gt; new { item = a, level = "a.cte_level", path = "a.cte_path" })<para></para>
    /// * v2.0.0 兼容 MySql5.6 向上或向下查询，但不支持 pathSelector/pathSeparator 详细：https://github.com/dotnetcore/FreeSql/issues/536
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="that"></param>
    /// <param name="up">false(默认)：由父级向子级的递归查询<para></para>true：由子级向父级的递归查询</param>
    /// <param name="pathSelector">路径内容选择</param>
    /// <param name="pathSeparator">连接路径内容</param>
    /// <param name="level">递归层级</param>
    /// <returns></returns>
    public static ISelect<T1> AsTreeCte<T1>(this ISelect<T1> that,
        Expression<Func<T1, string>> pathSelector = null,
        bool up = false,
        string pathSeparator = " -> ",
        int level = -1) where T1 : class
    {
        var select = that as Select1Provider<T1>;
        select._is_AsTreeCte = true;
        var tb = select._tables[0].Table;
        var navs = tb.Properties.Select(a => tb.GetTableRef(a.Key, false))
            .Where(a => a != null &&
                a.RefType == FreeSql.Internal.Model.TableRefType.OneToMany &&
                a.RefEntityType == tb.Type).ToArray();

        if (navs.Length != 1) throw new ArgumentException(CoreStrings.Entity_NotParentChild_Relationship(tb.Type.FullName));
        var tbref = navs[0];

        var cteName = "as_tree_cte";
        if (select._orm.CodeFirst.IsSyncStructureToLower) cteName = cteName.ToLower();
        if (select._orm.CodeFirst.IsSyncStructureToUpper) cteName = cteName.ToUpper();

        var tableRule = select._tableRule;
        var tbDbName = (tableRule?.Invoke(tb.Type, tb.DbName) ?? tb.DbName);
        if (select._orm.CodeFirst.IsSyncStructureToLower) tbDbName = tbDbName.ToLower();
        if (select._orm.CodeFirst.IsSyncStructureToUpper) tbDbName = tbDbName.ToUpper();
        if (select._orm.CodeFirst.IsAutoSyncStructure)
            select._orm.CodeFirst.SyncStructure(tb.Type, tbDbName);

        switch (select._orm.Ado.DataType)
        {
            case DataType.GBase:
                //select t.parentid, t.subid, level
                //from a_test t
                //start with subid = '7'
                //connect by prior subid =  parentid;
                var gbsb = new StringBuilder();
                var gbsbWhere = select._where.ToString();
                select._where.Clear();
                if (gbsbWhere.StartsWith(" AND ")) gbsbWhere = gbsbWhere.Remove(0, 5);
                gbsb.Append(select._tosqlAppendContent).Append(" \r\nstart with ").Append(gbsbWhere).Append(" \r\nconnect by prior ");
                if (up) gbsb.Append("a.").Append(select._commonUtils.QuoteSqlName(tbref.Columns[0].Attribute.Name)).Append(" = ").Append("a.").Append(select._commonUtils.QuoteSqlName(tbref.RefColumns[0].Attribute.Name));
                else gbsb.Append("a.").Append(select._commonUtils.QuoteSqlName(tbref.Columns[0].Attribute.Name)).Append(" = ").Append("a.").Append(select._commonUtils.QuoteSqlName(tbref.RefColumns[0].Attribute.Name));
                var gbswstr = gbsb.ToString();
                gbsb.Clear();
                select.AsAlias((_, old) => $"{old} {gbswstr}");
                return select;
            case DataType.MySql: //MySql5.6
            case DataType.OdbcMySql:
                var mysqlConnectionString = select._orm.Ado?.ConnectionString ?? select._connection?.ConnectionString ?? "";
                if (_dicMySqlVersion.TryGetValue(mysqlConnectionString, out var mysqlVersion) == false)
                {
                    if (select._orm.Ado?.ConnectionString != null)
                    {
                        using (var mysqlconn = select._orm.Ado.MasterPool.Get())
                            mysqlVersion = mysqlconn.Value.ServerVersion;
                    }
                    else if (select._connection != null)
                    {
                        var isclosed = select._connection.State != ConnectionState.Open;
                        if (isclosed) select._connection.Open();
                        mysqlVersion = select._connection.ServerVersion;
                        if (isclosed) select._connection.Close();
                    }
                    _dicMySqlVersion.TryAdd(mysqlConnectionString, mysqlVersion);
                }
                if (int.TryParse((mysqlVersion ?? "").Split('.')[0], out var mysqlVersionFirst) && mysqlVersionFirst < 8)
                {
                    if (tbref.Columns.Count > 1) throw new ArgumentException(CoreStrings.Entity_MySQL_VersionsBelow8_NotSupport_Multiple_PrimaryKeys(tb.Type.FullName));
                    var mysql56Sql = "";
                    if (up == false)
                    {
                        mysql56Sql = $@"SELECT cte_tbc.cte_level, {select.GetAllFieldExpressionTreeLevel2().Field}
  FROM (
    SELECT @cte_ids as cte_ids, (
      SELECT @cte_ids := group_concat({select._commonUtils.QuoteSqlName(tbref.Columns[0].Attribute.Name)}) 
      FROM {select._commonUtils.QuoteSqlName(tbDbName)} 
      WHERE find_in_set({select._commonUtils.QuoteSqlName(tbref.RefColumns[0].Attribute.Name)}, @cte_ids)
    ) as cte_cids, @cte_level := @cte_idcte_levels + 1 as cte_level
    FROM {select._commonUtils.QuoteSqlName(tbDbName)}, (
      SELECT @cte_ids := a.{select._commonUtils.QuoteSqlName(tbref.Columns[0].Attribute.Name)}, @cte_idcte_levels := 0 
      FROM {select._commonUtils.QuoteSqlName(tbDbName)} a
      WHERE 1=1{select._where}
      LIMIT 1) cte_tbb
    WHERE @cte_ids IS NOT NULL
  ) cte_tbc, {select._commonUtils.QuoteSqlName(tbDbName)} a
  WHERE find_in_set(a.{select._commonUtils.QuoteSqlName(tbref.Columns[0].Attribute.Name)}, cte_tbc.cte_ids)";
                        select.WithSql(mysql56Sql).OrderBy("a.cte_level DESC");
                        select._where.Clear();
                        return select;
                    }
                    mysql56Sql = $@"SELECT cte_tbc.cte_level, {select.GetAllFieldExpressionTreeLevel2().Field}
FROM (
    SELECT @cte_pid as cte_id, (SELECT @cte_pid := {select._commonUtils.QuoteSqlName(tbref.RefColumns[0].Attribute.Name)} FROM {select._commonUtils.QuoteSqlName(tbDbName)} WHERE {select._commonUtils.QuoteSqlName(tbref.Columns[0].Attribute.Name)} = cte_id) as cte_pid, @cte_level := @cte_level + 1 as cte_level
    FROM {select._commonUtils.QuoteSqlName(tbDbName)}, (
      SELECT @cte_pid := a.{select._commonUtils.QuoteSqlName(tbref.Columns[0].Attribute.Name)}, @cte_level := 0 
      FROM {select._commonUtils.QuoteSqlName(tbDbName)} a
      WHERE 1=1{select._where}
      LIMIT 1) cte_tbb
) cte_tbc
JOIN {select._commonUtils.QuoteSqlName(tbDbName)} a ON cte_tbc.cte_id = a.{select._commonUtils.QuoteSqlName(tbref.Columns[0].Attribute.Name)}";
                    select.WithSql(mysql56Sql).OrderBy("a.cte_level");
                    select._where.Clear();
                    return select;
                }
                break;
        }

        var sql1ctePath = "";
        if (pathSelector != null)
        {
            select._tables[0].Parameter = pathSelector?.Parameters[0];
            switch (select._orm.Ado.DataType)
            {
                case DataType.PostgreSQL:
                case DataType.OdbcPostgreSQL:
                case DataType.KingbaseES:
                case DataType.OdbcKingbaseES:
                case DataType.ShenTong: //神通测试未通过
                case DataType.SqlServer:
                case DataType.OdbcSqlServer:
                case DataType.Firebird:
                case DataType.ClickHouse:
                    sql1ctePath = select._commonExpression.ExpressionWhereLambda(select._tables, select._tableRule, Expression.Call(typeof(Convert).GetMethod("ToString", new Type[] { typeof(string) }), pathSelector?.Body), select._diymemexpWithTempQuery, null, null);
                    break;
                default:
                    sql1ctePath = select._commonExpression.ExpressionWhereLambda(select._tables, select._tableRule, pathSelector?.Body, select._diymemexpWithTempQuery, null, null);
                    break;
            }
            sql1ctePath = $"{sql1ctePath} as cte_path, ";
        }
        var sql1 = select.ToSql($"0 as cte_level, {sql1ctePath}{select.GetAllFieldExpressionTreeLevel2(false).Field}").Trim();

        select._where.Clear();
        select.As("wct2");
        var sql2Field = select.GetAllFieldExpressionTreeLevel2(false).Field;
        var sql2InnerJoinOn = up == false ?
            string.Join(" and ", tbref.Columns.Select((a, z) => $"wct2.{select._commonUtils.QuoteSqlName(tbref.RefColumns[z].Attribute.Name)} = wct1.{select._commonUtils.QuoteSqlName(a.Attribute.Name)}")) :
            string.Join(" and ", tbref.Columns.Select((a, z) => $"wct2.{select._commonUtils.QuoteSqlName(a.Attribute.Name)} = wct1.{select._commonUtils.QuoteSqlName(tbref.RefColumns[z].Attribute.Name)}"));

        var sql2ctePath = "";
        if (pathSelector != null)
        {
            select._tables[0].Parameter = pathSelector?.Parameters[0];
            var wct2ctePath = select._commonExpression.ExpressionWhereLambda(select._tables, select._tableRule, pathSelector?.Body, select._diymemexpWithTempQuery, null, null);
            sql2ctePath = select._commonUtils.StringConcat(
                new string[] {
                    up == false ? "wct1.cte_path" : wct2ctePath,
                    select._commonUtils.FormatSql("{0}", pathSeparator),
                    up == false ? wct2ctePath : "wct1.cte_path"
                }, new Type[] {
                    typeof(string),
                    typeof(string),
                    typeof(string)
                });
            sql2ctePath = $"{sql2ctePath} as cte_path, ";
        }
        if (select._orm.CodeFirst.IsAutoSyncStructure)
            (select._orm.CodeFirst as CodeFirstProvider)._dicSycedTryAdd(tb.Type, cteName); //#476

        select._tableRules.Clear();
        var sql2 = select
            .AsAlias((type, old) => type == tb.Type ? old.Replace("wct2", "wct1") : old)
            .AsTable((type, old) => type == tb.Type ? cteName : (tableRule?.Invoke(type, old) ?? old))
            .InnerJoin($"{select._commonUtils.QuoteSqlName(tbDbName)} wct2 ON {sql2InnerJoinOn}")
            .ToSql($"wct1.cte_level + 1 as cte_level, {sql2ctePath}{sql2Field}").Trim();

        var newSelect = select._orm.Select<T1>()
            .WithConnection(select._connection)
            .WithTransaction(select._transaction)
            .TrackToList(select._trackToList)
            .AsType(tb.Type)
            .AsTable((type, old) => type == tb.Type ? cteName : old)
            .WhereIf(level > 0, $"a.cte_level < {level + 1}")
            .OrderBy(up, "a.cte_level desc") as Select1Provider<T1>;

        newSelect._is_AsTreeCte = true;
        newSelect._params = new List<DbParameter>(select._params.ToArray());
        newSelect._includeInfo = select._includeInfo;
        newSelect._includeManySubListOneToManyTempValue1 = select._includeManySubListOneToManyTempValue1;
        newSelect._includeToList = select._includeToList;
#if net40
#else
        newSelect._includeToListAsync = select._includeToListAsync;
#endif

        var nsselsb = new StringBuilder();
        if (AdoProvider.IsFromSlave(select._select) == false) nsselsb.Append(' '); //读写分离规则，如果强制读主库，则在前面加个空格
        nsselsb.Append("WITH ");
        switch (select._orm.Ado.DataType)
        {
            case DataType.PostgreSQL:
            case DataType.OdbcPostgreSQL:
            case DataType.KingbaseES:
            case DataType.OdbcKingbaseES:
            case DataType.ShenTong: //神通测试未通过
            case DataType.MySql:
            case DataType.OdbcMySql:
            case DataType.Firebird:
                nsselsb.Append("RECURSIVE ");
                break;
        }
        nsselsb.Append(select._commonUtils.QuoteSqlName(cteName));
        switch (select._orm.Ado.DataType)
        {
            case DataType.Oracle: //[Err] ORA-32039: recursive WITH clause must have column alias list
            case DataType.OdbcOracle:
            case DataType.Dameng: //递归 WITH 子句必须具有列别名列表
            case DataType.OdbcDameng:
            case DataType.GBase:
                nsselsb.Append($"(cte_level, {(pathSelector == null ? "" : "cte_path, ")}{sql2Field.Replace("wct2.", "")})");
                break;
        }
        nsselsb.Append(@"
as
(
").Append(sql1).Append("\r\n\r\nunion all\r\n\r\n").Append(sql2).Append(@"
)
SELECT ");
        newSelect._select = nsselsb.ToString();
        nsselsb.Clear();
        return newSelect;
    }
    #endregion

    #region OrderBy Random 随机排序
    /// <summary>
    /// 随机排序<para></para>
    /// 支持：MySql/SqlServer/PostgreSQL/Oracle/Sqlite/Firebird/达梦/金仓/神通<para></para>
    /// 不支持：MsAcess
    /// </summary>
    /// <returns></returns>
    public static TSelect OrderByRandom<TSelect, T1>(this ISelect0<TSelect, T1> that) where TSelect : class
    {
        var s0p = that as Select0Provider;
        switch (s0p._orm.Ado.DataType)
        {
            case DataType.MySql:
            case DataType.OdbcMySql:
            case DataType.ClickHouse:
                return that.OrderBy("rand()");
            case DataType.SqlServer:
            case DataType.OdbcSqlServer:
                return that.OrderBy("newid()");
            case DataType.PostgreSQL:
            case DataType.OdbcPostgreSQL:
            case DataType.KingbaseES:
            case DataType.OdbcKingbaseES:
            case DataType.ShenTong:
                return that.OrderBy("random()");
            case DataType.Oracle:
            case DataType.Dameng:
            case DataType.OdbcOracle:
            case DataType.OdbcDameng:
                return that.OrderBy("dbms_random.value");
            case DataType.Sqlite:
                return that.OrderBy("random()");
            //case DataType.MsAccess:
            //    return that.OrderBy("rnd()");
            case DataType.Firebird:
                return that.OrderBy("rand()");
        }
        throw new NotSupportedException($"{CoreStrings.Not_Support_OrderByRandom(s0p._orm.Ado.DataType)}");
    }
    #endregion

    #region InsertDict/UpdateDict/InsertOrUpdateDict/DeleteDict
    /// <summary>
    /// 插入数据字典 Dictionary&lt;string, object&gt;
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static InsertDictImpl InsertDict(this IFreeSql freesql, Dictionary<string, object> source)
    {
        var insertDict = new InsertDictImpl(freesql);
        insertDict._insertProvider.AppendData(source);
        return insertDict;
    }
    /// <summary>
    /// 插入数据字典，传入 Dictionary&lt;string, object&gt; 集合
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static InsertDictImpl InsertDict(this IFreeSql freesql, IEnumerable<Dictionary<string, object>> source)
    {
        var insertDict = new InsertDictImpl(freesql);
        insertDict._insertProvider.AppendData(source);
        return insertDict;
    }
    /// <summary>
    /// 更新数据字典 Dictionary&lt;string, object&gt;
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static UpdateDictImpl UpdateDict(this IFreeSql freesql, Dictionary<string, object> source)
    {
        var updateDict = new UpdateDictImpl(freesql);
        updateDict._updateProvider.SetSource(source);
        return updateDict;
    }
    /// <summary>
    /// 更新数据字典，传入 Dictionary&lt;string, object&gt; 集合
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static UpdateDictImpl UpdateDict(this IFreeSql freesql, IEnumerable<Dictionary<string, object>> source)
    {
        var updateDict = new UpdateDictImpl(freesql);
        updateDict._updateProvider.SetSource(source);
        return updateDict;
    }
    /// <summary>
    /// 插入或更新数据字典，此功能依赖数据库特性（低版本可能不支持），参考如下：<para></para>
    /// MySql 5.6+: on duplicate key update<para></para>
    /// PostgreSQL 9.4+: on conflict do update<para></para>
    /// SqlServer 2008+: merge into<para></para>
    /// Oracle 11+: merge into<para></para>
    /// Sqlite: replace into<para></para>
    /// 达梦: merge into<para></para>
    /// 人大金仓：on conflict do update<para></para>
    /// 神通：merge into<para></para>
    /// MsAccess：不支持<para></para>
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static InsertOrUpdateDictImpl InsertOrUpdateDict(this IFreeSql freesql, Dictionary<string, object> source)
    {
        var insertOrUpdateDict = new InsertOrUpdateDictImpl(freesql);
        insertOrUpdateDict._insertOrUpdateProvider.SetSource(source);
        return insertOrUpdateDict;
    }
    public static InsertOrUpdateDictImpl InsertOrUpdateDict(this IFreeSql freesql, IEnumerable<Dictionary<string, object>> source)
    {
        var insertOrUpdateDict = new InsertOrUpdateDictImpl(freesql);
        insertOrUpdateDict._insertOrUpdateProvider.SetSource(source);
        return insertOrUpdateDict;
    }
    /// <summary>
    /// 删除数据字典 Dictionary&lt;string, object&gt;
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static DeleteDictImpl DeleteDict(this IFreeSql freesql, Dictionary<string, object> source)
    {
        var deleteDict = new DeleteDictImpl(freesql);
        UpdateProvider<Dictionary<string, object>>.GetDictionaryTableInfo(source, deleteDict._deleteProvider._orm, ref deleteDict._deleteProvider._table);
        var primarys = UpdateDictImpl.GetPrimarys(deleteDict._deleteProvider._table, source.Keys.ToArray());
        deleteDict._deleteProvider.Where(deleteDict._deleteProvider._commonUtils.WhereItems(primarys, "", new[] { source }));
        return deleteDict;
    }
    /// <summary>
    /// 删除数据字典，传入 Dictionary&lt;string, object&gt; 集合
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static DeleteDictImpl DeleteDict(this IFreeSql freesql, IEnumerable<Dictionary<string, object>> source)
    {
        DeleteDictImpl deleteDict = null;
        if (source.Select(a => string.Join(",", a.Keys)).Distinct().Count() == 1)
        {
            deleteDict = new DeleteDictImpl(freesql);
            var sourceFirst = source.FirstOrDefault();
            UpdateProvider<Dictionary<string, object>>.GetDictionaryTableInfo(sourceFirst, deleteDict._deleteProvider._orm, ref deleteDict._deleteProvider._table);
            var primarys = UpdateDictImpl.GetPrimarys(deleteDict._deleteProvider._table, sourceFirst.Keys.ToArray());
            deleteDict._deleteProvider.Where(deleteDict._deleteProvider._commonUtils.WhereItems(primarys, "", source));
            return deleteDict;
        }
        foreach (var item in source)
        {
            var tmpDelteDict = DeleteDict(freesql, item);
            if (deleteDict == null) deleteDict = tmpDelteDict;
            else deleteDict._deleteProvider._where.Append(" OR ").Append(tmpDelteDict._deleteProvider._where);
        }
        return deleteDict ?? new DeleteDictImpl(freesql);
    }

    #region InsertDictImpl
    public class InsertDictImpl
    {
        internal readonly InsertProvider<Dictionary<string, object>> _insertProvider;
        internal InsertDictImpl(IFreeSql orm)
        {
            _insertProvider = orm.Insert<Dictionary<string, object>>() as InsertProvider<Dictionary<string, object>>;
        }

        public InsertDictImpl AsTable(string tableName)
        {
            _insertProvider.AsTable(tableName);
            return this;
        }

        public InsertDictImpl BatchOptions(int valuesLimit, int parameterLimit, bool autoTransaction = true)
        {
            _insertProvider.BatchOptions(valuesLimit, parameterLimit, autoTransaction);
            return this;
        }
        public InsertDictImpl BatchProgress(Action<BatchProgressStatus<Dictionary<string, object>>> callback)
        {
            _insertProvider.BatchProgress(callback);
            return this;
        }

        public InsertDictImpl CommandTimeout(int timeout)
        {
            _insertProvider.CommandTimeout(timeout);
            return this;
        }

        public int ExecuteAffrows() => _insertProvider.ExecuteAffrows();
        public long ExecuteIdentity() => _insertProvider.ExecuteIdentity();
        public List<Dictionary<string, object>> ExecuteInserted() => _insertProvider.ExecuteInserted();

#if net40
#else
        public Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => _insertProvider.ExecuteAffrowsAsync(cancellationToken);
        public Task<long> ExecuteIdentityAsync(CancellationToken cancellationToken = default) => _insertProvider.ExecuteIdentityAsync(cancellationToken);
        public Task<List<Dictionary<string, object>>> ExecuteInsertedAsync(CancellationToken cancellationToken = default) => _insertProvider.ExecuteInsertedAsync(cancellationToken);
#endif

        public InsertDictImpl NoneParameter(bool isNotCommandParameter = true)
        {
            _insertProvider.NoneParameter(isNotCommandParameter);
            return this;
        }

        public DataTable ToDataTable() => _insertProvider.ToDataTable();
        public string ToSql() => _insertProvider.ToSql();

        public InsertDictImpl WithConnection(DbConnection connection)
        {
            _insertProvider.WithConnection(connection);
            return this;
        }
        public InsertDictImpl WithTransaction(DbTransaction transaction)
        {
            _insertProvider.WithTransaction(transaction);
            return this;
        }
    }
    #endregion

    #region UpdateDictImpl
    public class UpdateDictImpl
    {
        internal readonly UpdateProvider<Dictionary<string, object>> _updateProvider;
        internal UpdateDictImpl(IFreeSql orm)
        {
            _updateProvider = orm.Update<Dictionary<string, object>>(null) as UpdateProvider<Dictionary<string, object>>;
        }

        public UpdateDictImpl WherePrimary(params string[] primarys)
        {
            _updateProvider._tempPrimarys = GetPrimarys(_updateProvider._table, primarys);
            return this;
        }
        public static ColumnInfo[] GetPrimarys(TableInfo table, params string[] primarys)
        {
            if (primarys?.Any() != true) throw new ArgumentException(nameof(primarys));
            var pks = new List<ColumnInfo>();
            foreach (var primary in primarys)
            {
                if (table.ColumnsByCs.TryGetValue(string.Concat(primary), out var col)) pks.Add(col);
                else throw new Exception(CoreStrings.GetPrimarys_ParameterError_IsNotDictKey(primary));
            }
            return pks.ToArray();
        }
        public static void SetTablePrimary(TableInfo table, params string[] primarys)
        {
            foreach (var primary in primarys)
            {
                if (table.ColumnsByCs.TryGetValue(string.Concat(primary), out var col)) col.Attribute.IsPrimary = true;
                else throw new Exception(CoreStrings.GetPrimarys_ParameterError_IsNotDictKey(primary));
            }
            table.Primarys = table.Columns.Where(a => a.Value.Attribute.IsPrimary).Select(a => a.Value).ToArray();
        }

        public UpdateDictImpl AsTable(string tableName)
        {
            _updateProvider.AsTable(tableName);
            return this;
        }

        public UpdateDictImpl BatchOptions(int rowsLimit, int parameterLimit, bool autoTransaction = true)
        {
            _updateProvider.BatchOptions(rowsLimit, parameterLimit, autoTransaction);
            return this;
        }
        public UpdateDictImpl BatchProgress(Action<BatchProgressStatus<Dictionary<string, object>>> callback)
        {
            _updateProvider.BatchProgress(callback);
            return this;
        }

        public UpdateDictImpl CommandTimeout(int timeout)
        {
            _updateProvider.CommandTimeout(timeout);
            return this;
        }

        public int ExecuteAffrows() => _updateProvider.ExecuteAffrows();
        public List<Dictionary<string, object>> ExecuteUpdated() => _updateProvider.ExecuteUpdated();

#if net40
#else
        public Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => _updateProvider.ExecuteAffrowsAsync(cancellationToken);
        public Task<List<Dictionary<string, object>>> ExecuteUpdatedAsync(CancellationToken cancellationToken = default) => _updateProvider.ExecuteUpdatedAsync(cancellationToken);
#endif

        public UpdateDictImpl NoneParameter(bool isNotCommandParameter = true)
        {
            _updateProvider.NoneParameter(isNotCommandParameter);
            return this;
        }

        public string ToSql() => _updateProvider.ToSql();

        public UpdateDictImpl WithConnection(DbConnection connection)
        {
            _updateProvider.WithConnection(connection);
            return this;
        }
        public UpdateDictImpl WithTransaction(DbTransaction transaction)
        {
            _updateProvider.WithTransaction(transaction);
            return this;
        }
    }
    #endregion

    #region InsertOrUpdateDictImpl
    public class InsertOrUpdateDictImpl
    {
        internal readonly InsertOrUpdateProvider<Dictionary<string, object>> _insertOrUpdateProvider;
        internal InsertOrUpdateDictImpl(IFreeSql orm)
        {
            _insertOrUpdateProvider = orm.InsertOrUpdate<Dictionary<string, object>>() as InsertOrUpdateProvider<Dictionary<string, object>>;
        }

        public InsertOrUpdateDictImpl WherePrimary(params string[] primarys)
        {
            UpdateDictImpl.SetTablePrimary(_insertOrUpdateProvider._table, primarys);
            _insertOrUpdateProvider._tempPrimarys = _insertOrUpdateProvider._table.Primarys;
            return this;
        }

        public InsertOrUpdateDictImpl AsTable(string tableName)
        {
            _insertOrUpdateProvider.AsTable(tableName);
            return this;
        }

        public InsertOrUpdateDictImpl CommandTimeout(int timeout)
        {
            _insertOrUpdateProvider.CommandTimeout(timeout);
            return this;
        }

        public int ExecuteAffrows() => _insertOrUpdateProvider.ExecuteAffrows();
#if net40
#else
        public Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => _insertOrUpdateProvider.ExecuteAffrowsAsync(cancellationToken);
#endif
        public InsertOrUpdateDictImpl IfExistsDoNothing()
        {
            _insertOrUpdateProvider.IfExistsDoNothing();
            return this;
        }

        public string ToSql() => _insertOrUpdateProvider.ToSql();

        public InsertOrUpdateDictImpl WithConnection(DbConnection connection)
        {
            _insertOrUpdateProvider.WithConnection(connection);
            return this;
        }
        public InsertOrUpdateDictImpl WithTransaction(DbTransaction transaction)
        {
            _insertOrUpdateProvider.WithTransaction(transaction);
            return this;
        }
    }
    #endregion

    #region DeleteDictImpl
    public class DeleteDictImpl
    {
        internal readonly DeleteProvider<Dictionary<string, object>> _deleteProvider;
        internal DeleteDictImpl(IFreeSql orm)
        {
            _deleteProvider = orm.Delete<Dictionary<string, object>>(null) as DeleteProvider<Dictionary<string, object>>;
        }

        public DeleteDictImpl AsTable(string tableName)
        {
            _deleteProvider.AsTable(tableName);
            return this;
        }

        public DeleteDictImpl CommandTimeout(int timeout)
        {
            _deleteProvider.CommandTimeout(timeout);
            return this;
        }

        public int ExecuteAffrows() => _deleteProvider.ExecuteAffrows();
        public List<Dictionary<string, object>> ExecuteDeleted() => _deleteProvider.ExecuteDeleted();

#if net40
#else
        public Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => _deleteProvider.ExecuteAffrowsAsync(cancellationToken);
        public Task<List<Dictionary<string, object>>> ExecuteDeletedAsync(CancellationToken cancellationToken = default) => _deleteProvider.ExecuteDeletedAsync(cancellationToken);
#endif

        public string ToSql() => _deleteProvider.ToSql();

        public DeleteDictImpl WithConnection(DbConnection connection)
        {
            _deleteProvider.WithConnection(connection);
            return this;
        }
        public DeleteDictImpl WithTransaction(DbTransaction transaction)
        {
            _deleteProvider.WithTransaction(transaction);
            return this;
        }
    }
    #endregion

    #endregion


}
