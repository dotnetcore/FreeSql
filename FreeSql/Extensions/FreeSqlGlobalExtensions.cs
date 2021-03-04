using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Internal.CommonProvider;
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
    public static bool IsNullableType(this Type that) => that.IsArray == false && that?.FullName.StartsWith("System.Nullable`1[") == true;
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
    internal static string DisplayCsharp(this Type type, bool isNameSpace = true)
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

    static ConcurrentDictionary<Type, ConstructorInfo> _dicInternalGetTypeConstructor0OrFirst = new ConcurrentDictionary<Type, ConstructorInfo>();
    internal static ConstructorInfo InternalGetTypeConstructor0OrFirst(this Type that, bool isThrow = true)
    {
        var ret = _dicInternalGetTypeConstructor0OrFirst.GetOrAdd(that, tp =>
            tp.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null) ??
            tp.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(a => a.IsPublic ? 0 : 1).FirstOrDefault());
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
    /// 示例：new List&lt;Song&gt;(new[] { song1, song2, song3 }).IncludeMany(g.sqlite, a => a.Tags);<para></para>
    /// 文档：https://github.com/2881099/FreeSql/wiki/%e8%b4%aa%e5%a9%aa%e5%8a%a0%e8%bd%bd#%E5%AF%BC%E8%88%AA%E5%B1%9E%E6%80%A7-onetomanymanytomany
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
        var tb = select._tables[0].Table;
        var navs = tb.Properties.Select(a => tb.GetTableRef(a.Key, false))
            .Where(a => a != null &&
                a.RefType == FreeSql.Internal.Model.TableRefType.OneToMany &&
                a.RefEntityType == tb.Type).ToArray();

        if (navs.Length != 1) throw new ArgumentException($"{tb.Type.FullName} 不是父子关系，无法使用该功能");
        var tbref = navs[0];

        var cteName = "as_tree_cte";
        if (select._orm.CodeFirst.IsSyncStructureToLower) cteName = cteName.ToLower();
        if (select._orm.CodeFirst.IsSyncStructureToUpper) cteName = cteName.ToUpper();

        switch (select._orm.Ado.DataType) //MySql5.6
        {
            case DataType.MySql:
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
                    if (tbref.Columns.Count > 1) throw new ArgumentException($"{tb.Type.FullName} 是父子关系，但是 MySql 8.0 以下版本中不支持组合多主键");
                    var mysql56Sql = "";
                    if (up == false)
                    {
                        mysql56Sql = $@"SELECT cte_tbc.cte_level, {select.GetAllFieldExpressionTreeLevel2().Field}
  FROM (
    SELECT @cte_ids as cte_ids, (
      SELECT @cte_ids := group_concat({select._commonUtils.QuoteSqlName(tbref.Columns[0].Attribute.Name)}) 
      FROM {select._commonUtils.QuoteSqlName(tb.DbName)} 
      WHERE find_in_set({select._commonUtils.QuoteSqlName(tbref.RefColumns[0].Attribute.Name)}, @cte_ids)
    ) as cte_cids, @cte_level := @cte_idcte_levels + 1 as cte_level
    FROM {select._commonUtils.QuoteSqlName(tb.DbName)}, (
      SELECT @cte_ids := a.{select._commonUtils.QuoteSqlName(tbref.Columns[0].Attribute.Name)}, @cte_idcte_levels := 0 
      FROM {select._commonUtils.QuoteSqlName(tb.DbName)} a
      WHERE 1=1{select._where}
      LIMIT 1) cte_tbb
    WHERE @cte_ids IS NOT NULL
  ) cte_tbc, {select._commonUtils.QuoteSqlName(tb.DbName)} a
  WHERE find_in_set(a.{select._commonUtils.QuoteSqlName(tbref.Columns[0].Attribute.Name)}, cte_tbc.cte_ids)";
                        select.WithSql(mysql56Sql).OrderBy("a.cte_level DESC");
                        select._where.Clear();
                        return select;
                    }
                    mysql56Sql = $@"SELECT cte_tbc.cte_level, {select.GetAllFieldExpressionTreeLevel2().Field}
FROM (
    SELECT @cte_pid as cte_id, (SELECT @cte_pid := {select._commonUtils.QuoteSqlName(tbref.RefColumns[0].Attribute.Name)} FROM {select._commonUtils.QuoteSqlName(tb.DbName)} WHERE {select._commonUtils.QuoteSqlName(tbref.Columns[0].Attribute.Name)} = cte_id) as cte_pid, @cte_level := @cte_level + 1 as cte_level
    FROM {select._commonUtils.QuoteSqlName(tb.DbName)}, (
      SELECT @cte_pid := a.{select._commonUtils.QuoteSqlName(tbref.Columns[0].Attribute.Name)}, @cte_level := 0 
      FROM {select._commonUtils.QuoteSqlName(tb.DbName)} a
      WHERE 1=1{select._where}
      LIMIT 1) cte_tbb
) cte_tbc
JOIN {select._commonUtils.QuoteSqlName(tb.DbName)} a ON cte_tbc.cte_id = a.{select._commonUtils.QuoteSqlName(tbref.Columns[0].Attribute.Name)}";
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
                    sql1ctePath = select._commonExpression.ExpressionWhereLambda(select._tables, Expression.Call(typeof(Convert).GetMethod("ToString", new Type[] { typeof(string) }), pathSelector?.Body), null, null, null);
                    break;
                default:
                    sql1ctePath = select._commonExpression.ExpressionWhereLambda(select._tables, pathSelector?.Body, null, null, null);
                    break;
            }
            sql1ctePath = $"{sql1ctePath} as cte_path, ";
        }
        var sql1 = select.ToSql($"0 as cte_level, {sql1ctePath}{select.GetAllFieldExpressionTreeLevel2().Field}").Trim();

        select._where.Clear();
        select.As("wct2");
        var sql2Field = select.GetAllFieldExpressionTreeLevel2().Field;
        var sql2InnerJoinOn = up == false ?
            string.Join(" and ", tbref.Columns.Select((a, z) => $"wct2.{select._commonUtils.QuoteSqlName(tbref.RefColumns[z].Attribute.Name)} = wct1.{select._commonUtils.QuoteSqlName(a.Attribute.Name)}")) :
            string.Join(" and ", tbref.Columns.Select((a, z) => $"wct2.{select._commonUtils.QuoteSqlName(a.Attribute.Name)} = wct1.{select._commonUtils.QuoteSqlName(tbref.RefColumns[z].Attribute.Name)}"));

        var sql2ctePath = "";
        if (pathSelector != null)
        {
            select._tables[0].Parameter = pathSelector?.Parameters[0];
            var wct2ctePath = select._commonExpression.ExpressionWhereLambda(select._tables, pathSelector?.Body, null, null, null);
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
        var sql2 = select
            .AsAlias((type, old) => type == tb.Type ? old.Replace("wct2", "wct1") : old)
            .AsTable((type, old) => type == tb.Type ? cteName : old)
            .InnerJoin($"{select._commonUtils.QuoteSqlName(tb.DbName)} wct2 ON {sql2InnerJoinOn}")
            .ToSql($"wct1.cte_level + 1 as cte_level, {sql2ctePath}{sql2Field}").Trim();

        var newSelect = select._orm.Select<T1>()
            .WithConnection(select._connection)
            .WithTransaction(select._transaction)
            .TrackToList(select._trackToList)
            .AsType(tb.Type)
            .AsTable((type, old) => type == tb.Type ? cteName : old)
            .WhereIf(level > 0, $"a.cte_level < {level + 1}")
            .OrderBy(up, "a.cte_level desc") as Select1Provider<T1>;

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
        throw new NotSupportedException($"{s0p._orm.Ado.DataType} 不支持 OrderByRandom 随机排序");
    }
    #endregion
}
