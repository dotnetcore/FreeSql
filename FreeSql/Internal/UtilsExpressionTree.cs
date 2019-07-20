using FreeSql.DataAnnotations;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Internal
{
    public class Utils
    {

        static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, TableInfo>> _cacheGetTableByEntity = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, TableInfo>>();
        internal static void RemoveTableByEntity(Type entity, CommonUtils common)
        {
            if (entity.IsAnonymousType() ||
                entity.IsValueType ||
                entity.IsNullableType() ||
                entity.NullableTypeOrThis() == typeof(BigInteger)
                ) return;
            var tbc = _cacheGetTableByEntity.GetOrAdd(common._orm.Ado.DataType, k1 => new ConcurrentDictionary<Type, TableInfo>()); //区分数据库类型缓存
            if (tbc.TryRemove(entity, out var trytb) && trytb?.TypeLazy != null) tbc.TryRemove(trytb.TypeLazy, out var trylz);
        }
        internal static TableInfo GetTableByEntity(Type entity, CommonUtils common)
        {
            if (entity.IsAnonymousType() ||
                entity.IsValueType ||
                entity.IsNullableType() ||
                entity.NullableTypeOrThis() == typeof(BigInteger)
                ) return null;
            var tbc = _cacheGetTableByEntity.GetOrAdd(common._orm.Ado.DataType, k1 => new ConcurrentDictionary<Type, TableInfo>()); //区分数据库类型缓存
            if (tbc.TryGetValue(entity, out var trytb)) return trytb;
            if (common.CodeFirst.GetDbInfo(entity) != null) return null;
            if (typeof(IEnumerable).IsAssignableFrom(entity) && entity.IsGenericParameter == true) return null;
            if (entity.IsArray) return null;

            object entityDefault = null;
            try
            {
                if (entity.IsAbstract == false && entity.IsInterface == false)
                    entityDefault = Activator.CreateInstance(entity);
            }
            catch { }
            var tbattr = common.GetEntityTableAttribute(entity);
            trytb = new TableInfo();
            trytb.Type = entity;
            trytb.Properties = entity.GetProperties().ToDictionary(a => a.Name, a => a, StringComparer.CurrentCultureIgnoreCase);
            trytb.CsName = entity.Name;
            trytb.DbName = (tbattr?.Name ?? entity.Name);
            trytb.DbOldName = tbattr?.OldName;
            if (common.CodeFirst.IsSyncStructureToLower)
            {
                trytb.DbName = trytb.DbName.ToLower();
                trytb.DbOldName = trytb.DbOldName?.ToLower();
            }
            if (common.CodeFirst.IsSyncStructureToUpper)
            {
                trytb.DbName = trytb.DbName.ToUpper();
                trytb.DbOldName = trytb.DbOldName?.ToUpper();
            }
            trytb.SelectFilter = tbattr?.SelectFilter;
            if (tbattr != null) trytb.DisableSyncStructure = tbattr.DisableSyncStructure;
            var propsLazy = new List<(PropertyInfo, bool, bool)>();
            var propsNavObjs = new List<PropertyInfo>();
            var propsComment = CommonUtils.GetProperyCommentBySummary(entity);
            foreach (var p in trytb.Properties.Values)
            {
                var setMethod = trytb.Type.GetMethod($"set_{p.Name}");
                var colattr = common.GetEntityColumnAttribute(entity, p);
                var tp = common.CodeFirst.GetDbInfo(colattr?.MapType ?? p.PropertyType);
                //if (tp == null) continue;
                if (tp == null && colattr == null)
                {
                    if (common.CodeFirst.IsLazyLoading)
                    {
                        var getIsVirtual = trytb.Type.GetMethod($"get_{p.Name}")?.IsVirtual;
                        var setIsVirtual = setMethod?.IsVirtual;
                        if (getIsVirtual == true || setIsVirtual == true)
                            propsLazy.Add((p, getIsVirtual == true, setIsVirtual == true));
                    }
                    propsNavObjs.Add(p);
                    continue;
                }
                if (colattr == null)
                    colattr = new ColumnAttribute
                    {
                        Name = p.Name,
                        DbType = tp.Value.dbtypeFull,
                        IsIdentity = false,
                        IsNullable = tp.Value.isnullable ?? true,
                        IsPrimary = false,
                        IsIgnore = false,
                        Unique = null,
                        MapType = p.PropertyType
                    };
                if (colattr._IsNullable == null) colattr._IsNullable = tp?.isnullable;
                if (string.IsNullOrEmpty(colattr.DbType)) colattr.DbType = tp?.dbtypeFull ?? "varchar(255)";
                if (colattr.DbType.StartsWith("set(") || colattr.DbType.StartsWith("enum("))
                {
                    var leftBt = colattr.DbType.IndexOf('(');
                    colattr.DbType = colattr.DbType.Substring(0, leftBt).ToUpper() + colattr.DbType.Substring(leftBt);
                }
                else
                    colattr.DbType = colattr.DbType.ToUpper();

                if (tp != null && tp.Value.isnullable == null) colattr.IsNullable = tp.Value.dbtypeFull.Contains("NOT NULL") == false;
                if (colattr.DbType?.Contains("NOT NULL") == true) colattr.IsNullable = false;
                if (string.IsNullOrEmpty(colattr.Name)) colattr.Name = p.Name;
                if (common.CodeFirst.IsSyncStructureToLower)
                {
                    colattr.Name = colattr.Name.ToLower();
                    colattr.Unique = colattr.Unique?.ToLower();
                }
                if (common.CodeFirst.IsSyncStructureToUpper)
                {
                    colattr.Name = colattr.Name.ToUpper();
                    colattr.Unique = colattr.Unique?.ToUpper();
                }

                if ((colattr.IsNullable != true || colattr.IsIdentity == true || colattr.IsPrimary == true) && colattr.DbType.Contains("NOT NULL") == false)
                {
                    colattr.IsNullable = false;
                    colattr.DbType += " NOT NULL";
                }
                if (colattr.IsNullable == true && colattr.DbType.Contains("NOT NULL")) colattr.DbType = colattr.DbType.Replace("NOT NULL", "");
                colattr.DbType = Regex.Replace(colattr.DbType, @"\([^\)]+\)", m =>
                {
                    var tmpLt = Regex.Replace(m.Groups[0].Value, @"\s", "");
                    if (tmpLt.Contains("CHAR")) tmpLt = tmpLt.Replace("CHAR", " CHAR");
                    if (tmpLt.Contains("BYTE")) tmpLt = tmpLt.Replace("BYTE", " BYTE");
                    return tmpLt;
                });
                if (colattr.IsIdentity == true && colattr.MapType.IsNumberType() == false)
                    colattr.IsIdentity = false;
                if (setMethod == null) colattr.IsIgnore = true;

                var col = new ColumnInfo
                {
                    Table = trytb,
                    CsName = p.Name,
                    CsType = p.PropertyType,
                    Attribute = colattr
                };
                if (propsComment != null && propsComment.TryGetValue(p.Name, out var trycomment))
                    col.Comment = trycomment;
                if (colattr.IsIgnore)
                {
                    trytb.ColumnsByCsIgnore.Add(p.Name, col);
                    continue;
                }
                if (entityDefault == null) entityDefault = Activator.CreateInstance(entity);
                colattr.DbDefautValue = trytb.Properties[p.Name].GetValue(entityDefault);
                if (colattr.DbDefautValue != null && p.PropertyType != colattr.MapType) colattr.DbDefautValue = Utils.GetDataReaderValue(colattr.MapType, colattr.DbDefautValue);
                if (colattr.DbDefautValue == null) colattr.DbDefautValue = tp?.defaultValue;
                if (colattr.IsNullable == false && colattr.DbDefautValue == null)
                {
                    var citype = colattr.MapType.IsNullableType() ? colattr.MapType.GenericTypeArguments.FirstOrDefault() : colattr.MapType;
                    if (citype.IsArray)
                        colattr.DbDefautValue = Array.CreateInstance(citype, 0);
                    else
                        colattr.DbDefautValue = Activator.CreateInstance(citype);
                }

                trytb.Columns.Add(colattr.Name, col);
                trytb.ColumnsByCs.Add(p.Name, col);
            }
            trytb.VersionColumn = trytb.Columns.Values.Where(a => a.Attribute.IsVersion == true).LastOrDefault();
            if (trytb.VersionColumn != null)
            {
                if (trytb.VersionColumn.Attribute.MapType.IsNullableType() || trytb.VersionColumn.Attribute.MapType.IsNumberType() == false)
                    throw new Exception($"属性{trytb.VersionColumn.CsName} 被标注为行锁（乐观锁）(IsVersion)，但其必须为数字类型，并且不可为 Nullable");
            }
            trytb.Primarys = trytb.Columns.Values.Where(a => a.Attribute.IsPrimary == true).ToArray();
            if (trytb.Primarys.Any() == false)
            {
                var identcol = trytb.Columns.Values.Where(a => a.Attribute.IsIdentity == true).FirstOrDefault();
                if (identcol != null) trytb.Primarys = new[] { identcol };
                if (trytb.Primarys.Any() == false)
                {
                    trytb.Primarys = trytb.Columns.Values.Where(a => string.Compare(a.Attribute.Name, "id", true) == 0).ToArray();
                    if (trytb.Primarys.Any() == false)
                    {
                        trytb.Primarys = trytb.Columns.Values.Where(a => string.Compare(a.Attribute.Name, $"{trytb.DbName}id", true) == 0).ToArray();
                        if (trytb.Primarys.Any() == false)
                        {
                            trytb.Primarys = trytb.Columns.Values.Where(a => string.Compare(a.Attribute.Name, $"{trytb.DbName}_id", true) == 0).ToArray();
                        }
                    }
                }
                foreach (var col in trytb.Primarys)
                    col.Attribute.IsPrimary = true;
            }
            //从数据库查找主键、自增
            if (common.CodeFirst.IsConfigEntityFromDbFirst)
            {
                try
                {
                    if (common._orm.DbFirst != null)
                    {
                        if (common.dbTables == null)
                            lock (common.dbTablesLock)
                                if (common.dbTables == null)
                                    common.dbTables = common._orm.DbFirst.GetTablesByDatabase();

                        var finddbtbs = common.dbTables.Where(a => string.Compare(a.Name, trytb.CsName, true) == 0 || string.Compare(a.Name, trytb.DbName, true) == 0);
                        foreach (var dbtb in finddbtbs)
                        {
                            foreach (var dbident in dbtb.Identitys)
                            {
                                if (trytb.Columns.TryGetValue(dbident.Name, out var trycol) && trycol.Attribute.MapType == dbident.CsType ||
                                    trytb.ColumnsByCs.TryGetValue(dbident.Name, out trycol) && trycol.Attribute.MapType == dbident.CsType)
                                    trycol.Attribute.IsIdentity = true;
                            }
                            foreach (var dbpk in dbtb.Primarys)
                            {
                                if (trytb.Columns.TryGetValue(dbpk.Name, out var trycol) && trycol.Attribute.MapType == dbpk.CsType ||
                                    trytb.ColumnsByCs.TryGetValue(dbpk.Name, out trycol) && trycol.Attribute.MapType == dbpk.CsType)
                                    trycol.Attribute.IsPrimary = true;
                            }
                            foreach (var dbuk in dbtb.UniquesDict)
                            {
                                foreach (var dbcol in dbuk.Value)
                                {
                                    if (trytb.Columns.TryGetValue(dbcol.Name, out var trycol) && trycol.Attribute.MapType == dbcol.CsType ||
                                        trytb.ColumnsByCs.TryGetValue(dbcol.Name, out trycol) && trycol.Attribute.MapType == dbcol.CsType)
                                        if (trycol.Attribute._Uniques?.Contains(dbuk.Key) != true)
                                            trycol.Attribute.Unique += $"," + dbuk.Key;
                                }
                            }
                        }
                    }
                }
                catch { }
                trytb.Primarys = trytb.Columns.Values.Where(a => a.Attribute.IsPrimary == true).ToArray();
            }
            var allunique = trytb.Columns.Values.Where(a => a.Attribute._Uniques != null).SelectMany(a => a.Attribute._Uniques).Distinct();
            trytb.Uniques = allunique.ToDictionary(a => a, a => trytb.Columns.Values.Where(b => b.Attribute._Uniques != null && b.Attribute._Uniques.Contains(a)).ToList());
            tbc.AddOrUpdate(entity, trytb, (oldkey, oldval) => trytb);

            #region 查找导航属性的关系、virtual 属性延时加载，动态产生新的重写类
            var trytbTypeName = trytb.Type.IsNested ? $"{trytb.Type.DeclaringType.Namespace}.{trytb.Type.DeclaringType.Name}.{trytb.Type.Name}" : $"{trytb.Type.Namespace}.{trytb.Type.Name}";
            var trytbTypeLazyName = default(string);
            var overrieds = 0;
            StringBuilder cscode = null;
            if (common.CodeFirst.IsLazyLoading && propsLazy.Any())
            {
                if (trytb.Type.IsPublic == false && trytb.Type.IsNestedPublic == false) throw new Exception($"【延时加载】实体类型 {trytbTypeName} 必须声明为 public");

                trytbTypeLazyName = $"FreeSqlLazyEntity__{Regex.Replace(trytbTypeName, @"[^\w\d]", "_")}";

                cscode = new StringBuilder();
                cscode.AppendLine("using System;")
                    .AppendLine("using FreeSql.DataAnnotations;")
                    .AppendLine("using System.Collections.Generic;")
                    .AppendLine("using System.Linq;")
                    .AppendLine("using Newtonsoft.Json;")
                    .AppendLine()
                    .Append("public class ").Append(trytbTypeLazyName).Append(" : ").Append(trytbTypeName).AppendLine(" {")
                    .AppendLine("	[JsonIgnore] private IFreeSql __fsql_orm__ { get; set; }\r\n");
            }

            foreach (var pnv in propsNavObjs)
            {
                var vp = propsLazy.Where(a => a.Item1 == pnv).FirstOrDefault();
                var isLazy = vp.Item1 != null && !string.IsNullOrEmpty(trytbTypeLazyName);
                var propTypeName = pnv.PropertyType.IsGenericType ?
                    $"{pnv.PropertyType.Namespace}.{pnv.PropertyType.Name.Remove(pnv.PropertyType.Name.IndexOf('`'))}<{string.Join(", ", pnv.PropertyType.GenericTypeArguments.Select(a => a.IsNested ? $"{a.DeclaringType.Namespace}.{a.DeclaringType.Name}.{a.Name}" : $"{a.Namespace}.{a.Name}"))}>" :
                    (pnv.PropertyType.IsNested ? $"{pnv.PropertyType.DeclaringType.Namespace}.{pnv.PropertyType.DeclaringType.Name}.{pnv.PropertyType.Name}" : $"{pnv.PropertyType.Namespace}.{pnv.PropertyType.Name}");

                var pnvAttr = pnv.GetCustomAttribute<NavigateAttribute>();
                var pnvBind = pnvAttr?.Bind?.Split(',').Select(a => a.Trim()).Where(a => !string.IsNullOrEmpty(a)).ToArray();
                var nvref = new TableRef();
                nvref.Property = pnv;

                //List 或 ICollection，一对多、多对多
                var propElementType = pnv.PropertyType.GenericTypeArguments.FirstOrDefault() ?? pnv.PropertyType.GetElementType();
                if (propElementType != null)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(pnv.PropertyType) == false) continue;
                    if (trytb.Primarys.Any() == false)
                    {
                        nvref.Exception = new Exception($"导航属性 {trytbTypeName}.{pnv.Name} 解析错误，实体类型 {trytbTypeName} 缺少主键标识，[Column(IsPrimary = true)]");
                        trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                        //if (isLazy) throw nvref.Exception;
                        continue;
                    }

                    var tbref = propElementType == trytb.Type ? trytb : GetTableByEntity(propElementType, common); //可能是父子关系
                    if (tbref == null) continue;

                    var tbrefTypeName = tbref.Type.IsNested ? $"{tbref.Type.DeclaringType.Namespace}.{tbref.Type.DeclaringType.Name}.{tbref.Type.Name}" : $"{tbref.Type.Namespace}.{tbref.Type.Name}";
                    Type midType = null;
                    var isManyToMany = false;

                    Action valiManyToMany = () =>
                    {
                        if (midType != null)
                        {
                            var midTypeProps = midType.GetProperties();
                            var midTypePropsTrytb = midTypeProps.Where(a => a.PropertyType == trytb.Type).Count();
                            var midTypePropsTbref = midTypeProps.Where(a => a.PropertyType == tbref.Type).Count();
                            if (midTypePropsTrytb != 1 || midTypePropsTbref != 1) midType = null;
                        }
                    };

                    if (pnvAttr?.ManyToMany != null)
                    {
                        isManyToMany = propElementType != trytb.Type &&
                            tbref.Properties.Where(z => (z.Value.PropertyType.GenericTypeArguments.FirstOrDefault() == trytb.Type || z.Value.PropertyType.GetElementType() == trytb.Type) &&
                                z.Value.GetCustomAttribute<NavigateAttribute>()?.ManyToMany == pnvAttr.ManyToMany &&
                                typeof(IEnumerable).IsAssignableFrom(z.Value.PropertyType)).Any();

                        if (isManyToMany == false)
                        {
                            nvref.Exception = new Exception($"【ManyToMany】导航属性 {trytbTypeName}.{pnv.Name} 解析错误，实体类型 {tbrefTypeName} 必须存在对应的 [Navigate(ManyToMany = x)] 集合属性");
                            trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                            //if (isLazy) throw nvref.Exception;
                            continue;
                        }
                        if (isManyToMany)
                        {
                            midType = pnvAttr.ManyToMany;
                            valiManyToMany();
                        }
                    }
                    else
                    {
                        isManyToMany = propElementType != trytb.Type &&
                            pnv.Name.EndsWith($"{tbref.CsName}s", StringComparison.CurrentCultureIgnoreCase) &&
                            tbref.Properties.Where(z => (z.Value.PropertyType.GenericTypeArguments.FirstOrDefault() == trytb.Type || z.Value.PropertyType.GetElementType() == trytb.Type) &&
                                z.Key.EndsWith($"{trytb.CsName}s", StringComparison.CurrentCultureIgnoreCase) &&
                                typeof(IEnumerable).IsAssignableFrom(z.Value.PropertyType)).Any();
                    }
                    if (isManyToMany)
                    {
                        if (tbref.Primarys.Any() == false)
                        {
                            nvref.Exception = new Exception($"【ManyToMany】导航属性 {trytbTypeName}.{pnv.Name} 解析错误，实体类型 {tbrefTypeName} 缺少主键标识，[Column(IsPrimary = true)]");
                            trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                            //if (isLazy) throw nvref.Exception;
                            continue;
                        }
                        if (pnvAttr?.ManyToMany == null)
                        {
                            //中间表怎么查询，比如 Song、Tag、SongTag
                            var midFlagStr = string.Empty;
                            if (pnv.Name.Length >= tbref.CsName.Length - 1)
                                midFlagStr = pnv.Name.Remove(pnv.Name.Length - tbref.CsName.Length - 1);

                            #region 在 trytb 命名空间下查找中间类
                            if (midType == null)
                            {
                                midType = trytb.Type.IsNested ?
                                    trytb.Type.Assembly.GetType($"{trytb.Type.Namespace}.{trytb.Type.DeclaringType.Name}+{trytb.CsName}{tbref.CsName}{midFlagStr}", false, true) : //SongTag
                                    trytb.Type.Assembly.GetType($"{trytb.Type.Namespace}.{trytb.CsName}{tbref.CsName}{midFlagStr}", false, true);
                                valiManyToMany();
                            }
                            if (midType == null)
                            {
                                midType = trytb.Type.IsNested ?
                                    trytb.Type.Assembly.GetType($"{trytb.Type.Namespace}.{trytb.Type.DeclaringType.Name}+{trytb.CsName}_{tbref.CsName}{(string.IsNullOrEmpty(midFlagStr) ? "" : "_")}{midFlagStr}", false, true) : //Song_Tag
                                    trytb.Type.Assembly.GetType($"{trytb.Type.Namespace}.{trytb.CsName}_{tbref.CsName}{(string.IsNullOrEmpty(midFlagStr) ? "" : "_")}{midFlagStr}", false, true);
                                valiManyToMany();
                            }
                            if (midType == null)
                            {
                                midType = trytb.Type.IsNested ?
                                    trytb.Type.Assembly.GetType($"{trytb.Type.Namespace}.{trytb.Type.DeclaringType.Name}+{tbref.CsName}{trytb.CsName}{midFlagStr}", false, true) : //TagSong
                                    trytb.Type.Assembly.GetType($"{trytb.Type.Namespace}.{tbref.CsName}{trytb.CsName}{midFlagStr}", false, true);
                                valiManyToMany();
                            }
                            if (midType == null)
                            {
                                midType = trytb.Type.IsNested ?
                                    trytb.Type.Assembly.GetType($"{trytb.Type.Namespace}.{trytb.Type.DeclaringType.Name}+{tbref.CsName}_{trytb.CsName}{(string.IsNullOrEmpty(midFlagStr) ? "" : "_")}{midFlagStr}", false, true) : //Tag_Song
                                    trytb.Type.Assembly.GetType($"{trytb.Type.Namespace}.{tbref.CsName}_{trytb.CsName}{(string.IsNullOrEmpty(midFlagStr) ? "" : "_")}{midFlagStr}", false, true);
                                valiManyToMany();
                            }
                            #endregion

                            #region 在 tbref 命名空间下查找中间类
                            if (midType == null)
                            {
                                midType = tbref.Type.IsNested ?
                                    tbref.Type.Assembly.GetType($"{tbref.Type.Namespace}.{tbref.Type.DeclaringType.Name}+{trytb.CsName}{tbref.CsName}{midFlagStr}", false, true) : //SongTag
                                    tbref.Type.Assembly.GetType($"{tbref.Type.Namespace}.{trytb.CsName}{tbref.CsName}{midFlagStr}", false, true);
                                valiManyToMany();
                            }
                            if (midType == null)
                            {
                                midType = tbref.Type.IsNested ?
                                    tbref.Type.Assembly.GetType($"{tbref.Type.Namespace}.{tbref.Type.DeclaringType.Name}+{trytb.CsName}_{tbref.CsName}{(string.IsNullOrEmpty(midFlagStr) ? "" : "_")}{midFlagStr}", false, true) : //Song_Tag
                                    tbref.Type.Assembly.GetType($"{tbref.Type.Namespace}.{trytb.CsName}_{tbref.CsName}{(string.IsNullOrEmpty(midFlagStr) ? "" : "_")}{midFlagStr}", false, true);
                                valiManyToMany();
                            }
                            if (midType == null)
                            {
                                midType = tbref.Type.IsNested ?
                                    tbref.Type.Assembly.GetType($"{tbref.Type.Namespace}.{tbref.Type.DeclaringType.Name}+{tbref.CsName}{trytb.CsName}{midFlagStr}", false, true) : //TagSong
                                    tbref.Type.Assembly.GetType($"{tbref.Type.Namespace}.{tbref.CsName}{trytb.CsName}{midFlagStr}", false, true);
                                valiManyToMany();
                            }
                            if (midType == null)
                            {
                                midType = tbref.Type.IsNested ?
                                    tbref.Type.Assembly.GetType($"{tbref.Type.Namespace}.{tbref.Type.DeclaringType.Name}+{tbref.CsName}_{trytb.CsName}{(string.IsNullOrEmpty(midFlagStr) ? "" : "_")}{midFlagStr}", false, true) : //Tag_Song
                                    tbref.Type.Assembly.GetType($"{tbref.Type.Namespace}.{tbref.CsName}_{trytb.CsName}{(string.IsNullOrEmpty(midFlagStr) ? "" : "_")}{midFlagStr}", false, true);
                                valiManyToMany();
                            }
                            #endregion
                        }

                        isManyToMany = midType != null;
                    }
                    if (isManyToMany)
                    {
                        var tbmid = GetTableByEntity(midType, common);
                        var midTypePropsTrytb = tbmid.Properties.Where(a => a.Value.PropertyType == trytb.Type).FirstOrDefault().Value;
                        //g.mysql.Select<Tag>().Where(a => g.mysql.Select<Song_tag>().Where(b => b.Tag_id == a.Id && b.Song_id == 1).Any());
                        var lmbdWhere = isLazy ? new StringBuilder() : null;

                        if (pnvAttr?.ManyToMany != null)
                        {
                            #region 指定 Navigate[ManyToMany = x] 配置多对多关系
                            TableRef trytbTf = null;
                            try
                            {
                                trytbTf = tbmid.GetTableRef(midTypePropsTrytb.Name, true);
                            }
                            catch (Exception ex)
                            {
                                nvref.Exception = new Exception($"【ManyToMany】导航属性 {trytbTypeName}.{pnv.Name} 解析错误，中间类 {tbmid.CsName}.{midTypePropsTrytb.Name} 错误：{ex.Message}");
                                trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                                //if (isLazy) throw nvref.Exception;
                            }
                            if (nvref.Exception == null)
                            {
                                if (trytbTf.RefType != TableRefType.ManyToOne && trytbTf.RefType != TableRefType.OneToOne)
                                {
                                    nvref.Exception = new Exception($"【ManyToMany】导航属性 {trytbTypeName}.{pnv.Name} 解析错误，中间类 {tbmid.CsName}.{midTypePropsTrytb.Name} 导航属性不是【ManyToOne】或【OneToOne】");
                                    trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                                    //if (isLazy) throw nvref.Exception;
                                }
                                else
                                {
                                    nvref.Columns.AddRange(trytbTf.RefColumns);
                                    nvref.MiddleColumns.AddRange(trytbTf.Columns);

                                    if (tbmid.Primarys.Any() == false)
                                        trytbTf.Columns.Select(c => tbmid.ColumnsByCs[c.CsName].Attribute.IsPrimary = true);
                                    if (isLazy)
                                    {
                                        for (var a = 0; a < trytbTf.RefColumns.Count; a++)
                                        {
                                            if (a > 0) lmbdWhere.Append(" && ");
                                            lmbdWhere.Append("b.").Append(trytbTf.Columns[a].CsName).Append(" == this.").Append(trytbTf.RefColumns[a].CsName);
                                        }
                                    }
                                }
                            }
                            if (nvref.Exception == null)
                            {
                                var midTypePropsTbref = tbmid.Properties.Where(a => a.Value.PropertyType == tbref.Type).FirstOrDefault().Value;

                                TableRef tbrefTf = null;
                                try
                                {
                                    tbrefTf = tbmid.GetTableRef(midTypePropsTbref.Name, true);
                                }
                                catch (Exception ex)
                                {
                                    nvref.Exception = new Exception($"【ManyToMany】导航属性 {tbrefTypeName}.{pnv.Name} 解析错误，中间类 {tbmid.CsName}.{midTypePropsTbref.Name} 错误：{ex.Message}");
                                    trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                                    //if (isLazy) throw nvref.Exception;
                                }
                                if (nvref.Exception == null)
                                {
                                    if (tbrefTf.RefType != TableRefType.ManyToOne && tbrefTf.RefType != TableRefType.OneToOne)
                                    {
                                        nvref.Exception = new Exception($"【ManyToMany】导航属性 {tbrefTypeName}.{pnv.Name} 解析错误，中间类 {tbmid.CsName}.{midTypePropsTbref.Name} 导航属性不是【ManyToOne】或【OneToOne】");
                                        trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                                        //if (isLazy) throw nvref.Exception;
                                    }
                                    else
                                    {
                                        nvref.RefColumns.AddRange(tbrefTf.RefColumns);
                                        nvref.MiddleColumns.AddRange(tbrefTf.Columns);

                                        if (tbmid.Primarys.Any() == false)
                                            tbrefTf.Columns.Select(c => tbmid.ColumnsByCs[c.CsName].Attribute.IsPrimary = true);

                                        if (isLazy)
                                        {
                                            for (var a = 0; a < tbrefTf.RefColumns.Count; a++)
                                                lmbdWhere.Append(" && b.").Append(tbrefTf.Columns[a].CsName).Append(" == a.").Append(tbrefTf.RefColumns[a].CsName);
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region 约定配置
                            for (var a = 0; a < trytb.Primarys.Length; a++)
                            {
                                var findtrytbPkCsName = trytb.Primarys[a].CsName.TrimStart('_');
                                if (findtrytbPkCsName.StartsWith(trytb.Type.Name, StringComparison.CurrentCultureIgnoreCase)) findtrytbPkCsName = findtrytbPkCsName.Substring(trytb.Type.Name.Length).TrimStart('_');
                                if (tbmid.ColumnsByCs.TryGetValue($"{midTypePropsTrytb.Name}{findtrytbPkCsName}", out var trycol) == false && //骆峰命名
                                    tbmid.ColumnsByCs.TryGetValue($"{midTypePropsTrytb.Name}_{findtrytbPkCsName}", out trycol) == false //下划线命名
                                    )
                                {

                                }
                                if (trycol != null && trycol.CsType.NullableTypeOrThis() != trytb.Primarys[a].CsType)
                                {
                                    nvref.Exception = new Exception($"【ManyToMany】导航属性 {trytbTypeName}.{pnv.Name} 解析错误，{tbmid.CsName}.{trycol.CsName} 和 {trytb.CsName}.{trytb.Primarys[a].CsName} 类型不一致");
                                    trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                                    //if (isLazy) throw nvref.Exception;
                                    break;
                                }
                                if (trycol == null)
                                {
                                    nvref.Exception = new Exception($"【ManyToMany】导航属性 {trytbTypeName}.{pnv.Name} 在 {tbmid.CsName} 中没有找到对应的字段，如：{midTypePropsTrytb.Name}{findtrytbPkCsName}、{midTypePropsTrytb.Name}_{findtrytbPkCsName}");
                                    trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                                    //if (isLazy) throw nvref.Exception;
                                    break;
                                }

                                nvref.Columns.Add(trytb.Primarys[a]);
                                nvref.MiddleColumns.Add(trycol);
                                if (tbmid.Primarys.Any() == false)
                                    trycol.Attribute.IsPrimary = true;

                                if (isLazy)
                                {
                                    if (a > 0) lmbdWhere.Append(" && ");
                                    lmbdWhere.Append("b.").Append(trycol.CsName).Append(" == this.").Append(trytb.Primarys[a].CsName);
                                }
                            }

                            if (nvref.Exception == null)
                            {
                                var midTypePropsTbref = tbmid.Properties.Where(a => a.Value.PropertyType == tbref.Type).FirstOrDefault().Value;
                                for (var a = 0; a < tbref.Primarys.Length; a++)
                                {
                                    var findtbrefPkCsName = tbref.Primarys[a].CsName.TrimStart('_');
                                    if (findtbrefPkCsName.StartsWith(tbref.Type.Name, StringComparison.CurrentCultureIgnoreCase)) findtbrefPkCsName = findtbrefPkCsName.Substring(tbref.Type.Name.Length).TrimStart('_');
                                    if (tbmid.ColumnsByCs.TryGetValue($"{midTypePropsTbref.Name}{findtbrefPkCsName}", out var trycol) == false && //骆峰命名
                                        tbmid.ColumnsByCs.TryGetValue($"{midTypePropsTbref.Name}_{findtbrefPkCsName}", out trycol) == false //下划线命名
                                        )
                                    {

                                    }
                                    if (trycol != null && trycol.CsType.NullableTypeOrThis() != tbref.Primarys[a].CsType)
                                    {
                                        nvref.Exception = new Exception($"【ManyToMany】导航属性 {trytbTypeName}.{pnv.Name} 解析错误，{tbmid.CsName}.{trycol.CsName} 和 {tbref.CsName}.{tbref.Primarys[a].CsName} 类型不一致");
                                        trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                                        //if (isLazy) throw nvref.Exception;
                                        break;
                                    }
                                    if (trycol == null)
                                    {
                                        nvref.Exception = new Exception($"【ManyToMany】导航属性 {trytbTypeName}.{pnv.Name} 在 {tbmid.CsName} 中没有找到对应的字段，如：{midTypePropsTbref.Name}{findtbrefPkCsName}、{midTypePropsTbref.Name}_{findtbrefPkCsName}");
                                        trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                                        //if (isLazy) throw nvref.Exception;
                                        break;
                                    }

                                    nvref.RefColumns.Add(tbref.Primarys[a]);
                                    nvref.MiddleColumns.Add(trycol);
                                    if (tbmid.Primarys.Any() == false)
                                        trycol.Attribute.IsPrimary = true;

                                    if (isLazy) lmbdWhere.Append(" && b.").Append(trycol.CsName).Append(" == a.").Append(tbref.Primarys[a].CsName);
                                }
                            }
                            #endregion
                        }
                        if (nvref.Columns.Count > 0 && nvref.RefColumns.Count > 0)
                        {
                            nvref.RefMiddleEntityType = tbmid.Type;
                            nvref.RefEntityType = tbref.Type;
                            nvref.RefType = TableRefType.ManyToMany;
                            trytb.AddOrUpdateTableRef(pnv.Name, nvref);

                            if (tbmid.Primarys.Any() == false)
                                tbmid.Primarys = tbmid.Columns.Values.Where(a => a.Attribute.IsPrimary == true).ToArray();
                        }

                        if (isLazy)
                        {
                            cscode.Append("	private bool __lazy__").Append(pnv.Name).AppendLine(" = false;")
                                    .Append("	public override ").Append(propTypeName).Append(" ").Append(pnv.Name).AppendLine(" {");
                            if (vp.Item2)
                            { //get 重写
                                cscode.Append("		get {\r\n")
                                    .Append("			if (base.").Append(pnv.Name).Append(" == null && __lazy__").Append(pnv.Name).AppendLine(" == false) {");

                                if (nvref.Exception == null)
                                    cscode.Append("				base.").Append(pnv.Name).Append(" = __fsql_orm__.Select<").Append(propElementType.IsNested ? $"{propElementType.DeclaringType.Namespace}.{propElementType.DeclaringType.Name}.{propElementType.Name}" : $"{propElementType.Namespace}.{propElementType.Name}")
                                        .Append(">().Where(a => __fsql_orm__.Select<").Append(tbmid.Type.IsNested ? $"{tbmid.Type.DeclaringType.Namespace}.{tbmid.Type.DeclaringType.Name}.{tbmid.Type.Name}" : $"{tbmid.Type.Namespace}.{tbmid.Type.Name}")
                                        .Append(">().Where(b => ").Append(lmbdWhere.ToString()).AppendLine(").Any()).ToList();")
                                        .Append("				__lazy__").Append(pnv.Name).AppendLine(" = true;");
                                else
                                    cscode.Append("				throw new Exception(\"").Append(nvref.Exception.Message.Replace("\r\n", "\\r\\n").Replace("\"", "\\\"")).AppendLine("\");");

                                cscode.Append("			}\r\n")
                                    .Append("			return base.").Append(pnv.Name).AppendLine(";")
                                    .Append("		}\r\n");
                            }
                            if (vp.Item3)
                            { //set 重写
                                cscode.Append("		set => base.").Append(pnv.Name).AppendLine(" = value;");
                            }
                            cscode.AppendLine("	}");
                        }
                    }
                    else
                    { //One To Many
                        List<ColumnInfo> bindColumns = new List<ColumnInfo>();
                        if (pnvBind != null)
                        {
                            foreach (var bi in pnvBind)
                            {
                                if (tbref.ColumnsByCs.TryGetValue(bi, out var trybindcol) == false)
                                {
                                    nvref.Exception = new Exception($"导航属性 {trytbTypeName}.{pnv.Name} 特性 [Navigate] 解析错误，在 {tbrefTypeName} 未找到属性：{bi}");
                                    trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                                    //if (isLazy) throw nvref.Exception;
                                    break;
                                }
                                bindColumns.Add(trybindcol);
                            }
                        }

                        PropertyInfo refprop = null;
                        var refcols = tbref.Properties.Where(z => z.Value.PropertyType == trytb.Type);
                        refprop = refcols.Count() == 1 ? refcols.First().Value : null;
                        var lmbdWhere = isLazy ? new StringBuilder() : null;

                        if (nvref.Exception == null && bindColumns.Any() && bindColumns.Count != trytb.Primarys.Length)
                        {
                            nvref.Exception = new Exception($"导航属性 {trytbTypeName}.{pnv.Name} 特性 [Navigate] Bind 数目({bindColumns.Count}) 与 内部主键数目({trytb.Primarys.Length}) 不相同");
                            trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                            //if (isLazy) throw nvref.Exception;
                        }
                        if (trytb.Primarys.Length > 1)
                        {
                            if (trytb.Primarys.Select(a => a.CsType).Distinct().Count() == trytb.Primarys.Length)
                            {
                                var pkList = trytb.Primarys.ToList();
                                bindColumns.Sort((a, b) => pkList.FindIndex(c => c.CsType == a.CsType).CompareTo(pkList.FindIndex(c => c.CsType == b.CsType)));
                            }
                            else if (string.Compare(string.Join(",", trytb.Primarys.Select(a => a.CsName).OrderBy(a => a)), string.Join(",", bindColumns.Select(a => a.CsName).OrderBy(a => a)), true) == 0)
                            {
                                var pkList = trytb.Primarys.ToList();
                                bindColumns.Sort((a, b) => pkList.FindIndex(c => string.Compare(c.CsName, a.CsName, true) == 0).CompareTo(pkList.FindIndex(c => string.Compare(c.CsName, b.CsName, true) == 0)));
                            }
                        }
                        for (var a = 0; nvref.Exception == null && a < trytb.Primarys.Length; a++)
                        {
                            var findtrytbPkCsName = trytb.Primarys[a].CsName.TrimStart('_');
                            if (findtrytbPkCsName.StartsWith(trytb.Type.Name, StringComparison.CurrentCultureIgnoreCase)) findtrytbPkCsName = findtrytbPkCsName.Substring(trytb.Type.Name.Length).TrimStart('_');
                            var findtrytb = pnv.Name;
                            if (findtrytb.EndsWith($"{tbref.CsName}s", StringComparison.CurrentCultureIgnoreCase)) findtrytb = findtrytb.Substring(0, findtrytb.Length - tbref.CsName.Length - 1);
                            findtrytb += trytb.CsName;

                            var trycol = bindColumns.Any() ? bindColumns[a] : null;
                            if (trycol == null &&
                                tbref.ColumnsByCs.TryGetValue($"{findtrytb}{findtrytbPkCsName}", out trycol) == false && //骆峰命名
                                tbref.ColumnsByCs.TryGetValue($"{findtrytb}_{findtrytbPkCsName}", out trycol) == false //下划线命名
                                )
                            {
                                if (refprop != null &&
                                    tbref.ColumnsByCs.TryGetValue($"{refprop.Name}{findtrytbPkCsName}", out trycol) == false && //骆峰命名
                                    tbref.ColumnsByCs.TryGetValue($"{refprop.Name}_{findtrytbPkCsName}", out trycol) == false) //下划线命名
                                {

                                }
                            }
                            if (trycol != null && trycol.CsType.NullableTypeOrThis() != trytb.Primarys[a].CsType)
                            {
                                nvref.Exception = new Exception($"【OneToMany】导航属性 {trytbTypeName}.{pnv.Name} 解析错误，{trytb.CsName}.{trytb.Primarys[a].CsName} 和 {tbref.CsName}.{trycol.CsName} 类型不一致");
                                trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                                //if (isLazy) throw nvref.Exception;
                                break;
                            }
                            if (trycol == null)
                            {
                                nvref.Exception = new Exception($"【OneToMany】导航属性 {trytbTypeName}.{pnv.Name} 在 {tbref.CsName} 中没有找到对应的字段，如：{findtrytb}{findtrytbPkCsName}、{findtrytb}_{findtrytbPkCsName}" + (refprop == null ? "" : $"、{refprop.Name}{findtrytbPkCsName}、{refprop.Name}_{findtrytbPkCsName}。或者使用 [Navigate] 特性指定关系映射。"));
                                trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                                //if (isLazy) throw nvref.Exception;
                                break;
                            }

                            nvref.Columns.Add(trytb.Primarys[a]);
                            nvref.RefColumns.Add(trycol);

                            if (isLazy && nvref.Exception == null)
                            {
                                if (a > 0) lmbdWhere.Append(" && ");
                                lmbdWhere.Append("a.").Append(trycol.CsName).Append(" == this.").Append(trytb.Primarys[a].CsName);

                                if (refprop == null)
                                { //加载成功后，把列表对应的导航属性值设置为 this，比如 Select<TopicType>().ToOne().Topics 下的 TopicType 属性值全部为 this
                                    var findtrytbName = trycol.CsName;
                                    if (findtrytbName.EndsWith(trytb.Primarys.First().CsName))
                                    {
                                        findtrytbName = findtrytbName.Remove(findtrytbName.Length - trytb.Primarys.First().CsName.Length).TrimEnd('_');
                                        if (tbref.Properties.TryGetValue(findtrytbName, out refprop) && refprop.PropertyType != trytb.Type)
                                            refprop = null;
                                    }
                                }
                            }
                        }
                        if (nvref.Columns.Count > 0 && nvref.RefColumns.Count > 0)
                        {
                            nvref.RefEntityType = tbref.Type;
                            nvref.RefType = TableRefType.OneToMany;
                            trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                        }

                        if (isLazy)
                        {
                            cscode.Append("	private bool __lazy__").Append(pnv.Name).AppendLine(" = false;")
                                .Append("	public override ").Append(propTypeName).Append(" ").Append(pnv.Name).AppendLine(" {");
                            if (vp.Item2)
                            { //get 重写
                                cscode.Append("		get {\r\n")
                                    .Append("			if (base.").Append(pnv.Name).Append(" == null && __lazy__").Append(pnv.Name).AppendLine(" == false) {");

                                if (nvref.Exception == null)
                                {
                                    cscode.Append("				base.").Append(pnv.Name).Append(" = __fsql_orm__.Select<").Append(propElementType.IsNested ? $"{propElementType.DeclaringType.Namespace}.{propElementType.DeclaringType.Name}.{propElementType.Name}" : $"{propElementType.Namespace}.{propElementType.Name}").Append(">().Where(a => ").Append(lmbdWhere.ToString()).AppendLine(").ToList();");
                                    if (refprop != null)
                                    {
                                        cscode.Append("				foreach (var loc1 in base.").Append(pnv.Name).AppendLine(")")
                                            .Append("					loc1.").Append(refprop.Name).AppendLine(" = this;")
                                            .Append("				__lazy__").Append(pnv.Name).AppendLine(" = true;");
                                    }
                                }
                                else
                                    cscode.Append("				throw new Exception(\"").Append(nvref.Exception.Message.Replace("\r\n", "\\r\\n").Replace("\"", "\\\"")).AppendLine("\");");

                                cscode
                                    .Append("			}\r\n")
                                    .Append("			return base.").Append(pnv.Name).AppendLine(";")
                                    .Append("		}\r\n");
                            }
                            if (vp.Item3)
                            { //set 重写
                                cscode.Append("		set => base.").Append(pnv.Name).AppendLine(" = value;");
                            }
                            cscode.AppendLine("	}");
                        }
                    }
                }
                else
                { //一对一、多对一
                    var tbref = pnv.PropertyType == trytb.Type ? trytb : GetTableByEntity(pnv.PropertyType, common); //可能是父子关系
                    if (tbref == null) continue;
                    if (tbref.Primarys.Any() == false)
                    {
                        nvref.Exception = new Exception($"导航属性 {trytbTypeName}.{pnv.Name} 解析错误，实体类型 {propTypeName} 缺少主键标识，[Column(IsPrimary = true)]");
                        trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                        //if (isLazy) throw nvref.Exception;
                    }
                    var tbrefTypeName = tbref.Type.IsNested ? $"{tbref.Type.DeclaringType.Namespace}.{tbref.Type.DeclaringType.Name}.{tbref.Type.Name}" : $"{tbref.Type.Namespace}.{tbref.Type.Name}";
                    var isOnoToOne = pnv.PropertyType != trytb.Type &&
                        tbref.Properties.Where(z => z.Value.PropertyType == trytb.Type).Any() &&
                        tbref.Primarys.Length == trytb.Primarys.Length &&
                        string.Join(",", tbref.Primarys.Select(a => a.CsType.FullName).OrderBy(a => a)) == string.Join(",", trytb.Primarys.Select(a => a.CsType.FullName).OrderBy(a => a));

                    List<ColumnInfo> bindColumns = new List<ColumnInfo>();
                    if (pnvBind != null)
                    {
                        foreach (var bi in pnvBind)
                        {
                            if (trytb.ColumnsByCs.TryGetValue(bi, out var trybindcol) == false)
                            {
                                nvref.Exception = new Exception($"导航属性 {trytbTypeName}.{pnv.Name} 特性 [Navigate] 解析错误，在 {trytbTypeName} 未找到属性：{bi}");
                                trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                                //if (isLazy) throw nvref.Exception;
                                break;
                            }
                            bindColumns.Add(trybindcol);
                        }
                    }
                    var lmbdWhere = new StringBuilder();

                    if (nvref.Exception == null && bindColumns.Any() && bindColumns.Count != tbref.Primarys.Length)
                    {
                        nvref.Exception = new Exception($"导航属性 {trytbTypeName}.{pnv.Name} 特性 [Navigate] Bind 数目({bindColumns.Count}) 与 外部主键数目({tbref.Primarys.Length}) 不相同");
                        trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                        //if (isLazy) throw nvref.Exception;
                    }
                    if (tbref.Primarys.Length > 1)
                    {
                        if (tbref.Primarys.Select(a => a.CsType).Distinct().Count() == tbref.Primarys.Length)
                        {
                            var pkList = tbref.Primarys.ToList();
                            bindColumns.Sort((a, b) => pkList.FindIndex(c => c.CsType == a.CsType).CompareTo(pkList.FindIndex(c => c.CsType == b.CsType)));
                        }
                        else if (string.Compare(string.Join(",", tbref.Primarys.Select(a => a.CsName).OrderBy(a => a)), string.Join(",", bindColumns.Select(a => a.CsName).OrderBy(a => a)), true) == 0)
                        {
                            var pkList = tbref.Primarys.ToList();
                            bindColumns.Sort((a, b) => pkList.FindIndex(c => string.Compare(c.CsName, a.CsName, true) == 0).CompareTo(pkList.FindIndex(c => string.Compare(c.CsName, b.CsName, true) == 0)));
                        }
                    }
                    for (var a = 0; nvref.Exception == null && a < tbref.Primarys.Length; a++)
                    {
                        var findtbrefPkCsName = tbref.Primarys[a].CsName.TrimStart('_');
                        if (findtbrefPkCsName.StartsWith(tbref.Type.Name, StringComparison.CurrentCultureIgnoreCase)) findtbrefPkCsName = findtbrefPkCsName.Substring(tbref.Type.Name.Length).TrimStart('_');

                        var trycol = bindColumns.Any() ? bindColumns[a] : null;
                        if (trycol == null &&
                            trytb.ColumnsByCs.TryGetValue($"{pnv.Name}{findtbrefPkCsName}", out trycol) == false && //骆峰命名
                            trytb.ColumnsByCs.TryGetValue($"{pnv.Name}_{findtbrefPkCsName}", out trycol) == false && //下划线命名
                                                                                                                     //tbref.Primarys.Length == 1 &&
                            trytb.ColumnsByCs.TryGetValue($"{pnv.Name}_Id", out trycol) == false &&
                            trytb.ColumnsByCs.TryGetValue($"{pnv.Name}Id", out trycol) == false
                            )
                        {
                            //一对一，主键与主键查找
                            if (isOnoToOne)
                            {
                                var trytbpks = trytb.Primarys.Where(z => z.CsType == tbref.Primarys[a].CsType); //一对一，按类型
                                if (trytbpks.Count() == 1) trycol = trytbpks.First();
                                else
                                {
                                    trytbpks = trytb.Primarys.Where(z => string.Compare(z.CsName, tbref.Primarys[a].CsName, true) == 0); //一对一，按主键名相同
                                    if (trytbpks.Count() == 1) trycol = trytbpks.First();
                                    else
                                    {
                                        trytbpks = trytb.Primarys.Where(z => string.Compare(z.CsName, $"{tbref.CsName}{tbref.Primarys[a].CsName}", true) == 0); //一对一，主键名 = 表+主键名
                                        if (trytbpks.Count() == 1) trycol = trytbpks.First();
                                        else
                                        {
                                            trytbpks = trytb.Primarys.Where(z => string.Compare(z.CsName, $"{tbref.CsName}_{tbref.Primarys[a].CsName}", true) == 0); //一对一，主键名 = 表+_主键名
                                            if (trytbpks.Count() == 1) trycol = trytbpks.First();
                                        }
                                    }
                                }
                            }
                        }
                        if (trycol != null && trycol.CsType.NullableTypeOrThis() != tbref.Primarys[a].CsType)
                        {
                            nvref.Exception = new Exception($"导航属性 {trytbTypeName}.{pnv.Name} 解析错误，{trytb.CsName}.{trycol.CsName} 和 {tbref.CsName}.{tbref.Primarys[a].CsName} 类型不一致");
                            trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                            //if (isLazy) throw nvref.Exception;
                            break;
                        }
                        if (trycol == null)
                        {
                            nvref.Exception = new Exception($"导航属性 {trytbTypeName}.{pnv.Name} 没有找到对应的字段，如：{pnv.Name}{findtbrefPkCsName}、{pnv.Name}_{findtbrefPkCsName}。或者使用 [Navigate] 特性指定关系映射。");
                            trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                            //if (isLazy) throw nvref.Exception;
                            break;
                        }

                        nvref.Columns.Add(trycol);
                        nvref.RefColumns.Add(tbref.Primarys[a]);

                        if (isLazy && nvref.Exception == null)
                        {
                            if (a > 0) lmbdWhere.Append(" && ");
                            lmbdWhere.Append("a.").Append(tbref.Primarys[a].CsName).Append(" == this.").Append(trycol.CsName);
                        }
                    }
                    if (nvref.Columns.Count > 0 && nvref.RefColumns.Count > 0)
                    {
                        nvref.RefEntityType = tbref.Type;
                        nvref.RefType = isOnoToOne ? TableRefType.OneToOne : TableRefType.ManyToOne;
                        trytb.AddOrUpdateTableRef(pnv.Name, nvref);
                    }

                    if (isLazy)
                    {
                        cscode.Append("	private bool __lazy__").Append(pnv.Name).AppendLine(" = false;")
                            .Append("	public override ").Append(propTypeName).Append(" ").Append(pnv.Name).AppendLine(" {");
                        if (vp.Item2)
                        { //get 重写
                            cscode.Append("		get {\r\n")
                                .Append("			if (base.").Append(pnv.Name).Append(" == null && __lazy__").Append(pnv.Name).AppendLine(" == false) {");

                            if (nvref.Exception == null)
                                cscode.Append("				base.").Append(pnv.Name).Append(" = __fsql_orm__.Select<").Append(propTypeName).Append(">().Where(a => ").Append(lmbdWhere.ToString()).AppendLine(").ToOne();")
                                    .Append("				__lazy__").Append(pnv.Name).AppendLine(" = true;");
                            else
                                cscode.Append("				throw new Exception(\"").Append(nvref.Exception.Message.Replace("\r\n", "\\r\\n").Replace("\"", "\\\"")).AppendLine("\");");

                            cscode
                                .Append("			}\r\n")
                                .Append("			return base.").Append(pnv.Name).AppendLine(";")
                                .Append("		}\r\n");
                        }
                        if (vp.Item3)
                        { //set 重写
                            cscode.Append("		set {\r\n")
                                .Append("			base.").Append(pnv.Name).AppendLine(" = value;")
                                .Append("			__lazy__").Append(pnv.Name).AppendLine(" = true;")
                                .Append("		}\r\n");
                        }
                        cscode.AppendLine("	}");
                    }
                }

                if (isLazy) ++overrieds;
            }
            if (overrieds > 0)
            {
                cscode.AppendLine("}");
                Assembly assembly = null;
                if (MethodLazyLoadingComplier.Value == null) throw new Exception("【延时加载】功能需要安装 FreeSql.Extensions.LazyLoading.dll，可前往 nuget 下载");
                try
                {
                    assembly = MethodLazyLoadingComplier.Value.Invoke(null, new object[] { cscode.ToString() }) as Assembly;
                }
                catch (Exception ex)
                {
                    throw new Exception($"【延时加载】{trytbTypeName} 编译错误：{ex.Message}\r\n\r\n{cscode}");
                }
                var type = assembly.DefinedTypes.Where(a => a.FullName.EndsWith(trytbTypeLazyName)).FirstOrDefault();
                trytb.TypeLazy = type;
                trytb.TypeLazySetOrm = type.GetProperty("__fsql_orm__", BindingFlags.Instance | BindingFlags.NonPublic).GetSetMethod(true);
                tbc.AddOrUpdate(type, trytb, (oldkey, oldval) => trytb);
            }
            #endregion

            return tbc.TryGetValue(entity, out var trytb2) ? trytb2 : trytb;
        }
        static Lazy<MethodInfo> MethodLazyLoadingComplier = new Lazy<MethodInfo>(() =>
        {
            var type = Type.GetType("FreeSql.Extensions.LazyLoading.LazyLoadingComplier,FreeSql.Extensions.LazyLoading");
            return type?.GetMethod("CompileCode");
        });

        public static T[] GetDbParamtersByObject<T>(string sql, object obj, string paramPrefix, Func<string, Type, object, T> constructorParamter)
        {
            if (string.IsNullOrEmpty(sql) || obj == null) return new T[0];
            var ttype = typeof(T);
            var type = obj.GetType();
            if (type == ttype) return new[] { (T)Convert.ChangeType(obj, type) };
            var ret = new List<T>();
            var ps = type.GetProperties();
            foreach (var p in ps)
            {
                if (sql.IndexOf($"{paramPrefix}{p.Name}", StringComparison.CurrentCultureIgnoreCase) == -1) continue;
                var pvalue = p.GetValue(obj);
                if (p.PropertyType == ttype) ret.Add((T)Convert.ChangeType(pvalue, ttype));
                else ret.Add(constructorParamter(p.Name, p.PropertyType, pvalue));
            }
            return ret.ToArray();
        }

        public static Dictionary<Type, bool> dicExecuteArrayRowReadClassOrTuple = new Dictionary<Type, bool>
        {
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
            //[typeof(MygisPoint)] = true,
            //[typeof(MygisLineString)] = true,
            //[typeof(MygisPolygon)] = true,
            //[typeof(MygisMultiPoint)] = true,
            //[typeof(MygisMultiLineString)] = true,
            //[typeof(MygisMultiPolygon)] = true,
            //[typeof(BitArray)] = true,
            //[typeof(NpgsqlPoint)] = true,
            //[typeof(NpgsqlLine)] = true,
            //[typeof(NpgsqlLSeg)] = true,
            //[typeof(NpgsqlBox)] = true,
            //[typeof(NpgsqlPath)] = true,
            //[typeof(NpgsqlPolygon)] = true,
            //[typeof(NpgsqlCircle)] = true,
            //[typeof((IPAddress Address, int Subnet))] = true,
            //[typeof(IPAddress)] = true,
            //[typeof(PhysicalAddress)] = true,
            //[typeof(NpgsqlRange<int>)] = true,
            //[typeof(NpgsqlRange<long>)] = true,
            //[typeof(NpgsqlRange<decimal>)] = true,
            //[typeof(NpgsqlRange<DateTime>)] = true,
            //[typeof(PostgisPoint)] = true,
            //[typeof(PostgisLineString)] = true,
            //[typeof(PostgisPolygon)] = true,
            //[typeof(PostgisMultiPoint)] = true,
            //[typeof(PostgisMultiLineString)] = true,
            //[typeof(PostgisMultiPolygon)] = true,
            //[typeof(PostgisGeometry)] = true,
            //[typeof(PostgisGeometryCollection)] = true,
            //[typeof(Dictionary<string, string>)] = true,
            //[typeof(JToken)] = true,
            //[typeof(JObject)] = true,
            //[typeof(JArray)] = true,
        };
        internal static ConcurrentDictionary<string, ConcurrentDictionary<Type, Func<Type, int[], DbDataReader, int, CommonUtils, RowInfo>>> _dicExecuteArrayRowReadClassOrTuple = new ConcurrentDictionary<string, ConcurrentDictionary<Type, Func<Type, int[], DbDataReader, int, CommonUtils, RowInfo>>>();
        internal class RowInfo
        {
            public object Value { get; set; }
            public int DataIndex { get; set; }
            public RowInfo(object value, int dataIndex)
            {
                this.Value = value;
                this.DataIndex = dataIndex;
            }
            public static ConstructorInfo Constructor = typeof(RowInfo).GetConstructor(new[] { typeof(object), typeof(int) });
            public static PropertyInfo PropertyValue = typeof(RowInfo).GetProperty("Value");
            public static PropertyInfo PropertyDataIndex = typeof(RowInfo).GetProperty("DataIndex");
        }
        internal static MethodInfo MethodDataReaderGetValue = typeof(DbDataReader).GetMethod("GetValue");
        internal static RowInfo ExecuteArrayRowReadClassOrTuple(string flagStr, Type typeOrg, int[] indexes, DbDataReader row, int dataIndex, CommonUtils _commonUtils)
        {
            if (string.IsNullOrEmpty(flagStr)) flagStr = "all";
            var func = _dicExecuteArrayRowReadClassOrTuple
                .GetOrAdd(flagStr, flag => new ConcurrentDictionary<Type, Func<Type, int[], DbDataReader, int, CommonUtils, RowInfo>>())
                .GetOrAdd(typeOrg, type =>
                {
                    var returnTarget = Expression.Label(typeof(RowInfo));
                    var typeExp = Expression.Parameter(typeof(Type), "type");
                    var indexesExp = Expression.Parameter(typeof(int[]), "indexes");
                    var rowExp = Expression.Parameter(typeof(DbDataReader), "row");
                    var dataIndexExp = Expression.Parameter(typeof(int), "dataIndex");
                    var commonUtilExp = Expression.Parameter(typeof(CommonUtils), "commonUtil");

                    if (type.IsArray) return Expression.Lambda<Func<Type, int[], DbDataReader, int, CommonUtils, RowInfo>>(
                        Expression.New(RowInfo.Constructor,
                            GetDataReaderValueBlockExpression(type, Expression.Call(rowExp, MethodDataReaderGetValue, dataIndexExp)),
                            //Expression.Call(MethodGetDataReaderValue, new Expression[] { typeExp, Expression.Call(rowExp, MethodDataReaderGetValue, dataIndexExp) }),
                            Expression.Add(dataIndexExp, Expression.Constant(1))
                        ), new[] { typeExp, indexesExp, rowExp, dataIndexExp, commonUtilExp }).Compile();

                    var typeGeneric = type;
                    if (typeGeneric.IsNullableType()) typeGeneric = type.GenericTypeArguments.First();
                    if (typeGeneric.IsEnum ||
                        dicExecuteArrayRowReadClassOrTuple.ContainsKey(typeGeneric))
                        return Expression.Lambda<Func<Type, int[], DbDataReader, int, CommonUtils, RowInfo>>(
                        Expression.New(RowInfo.Constructor,
                            GetDataReaderValueBlockExpression(type, Expression.Call(rowExp, MethodDataReaderGetValue, dataIndexExp)),
                            //Expression.Call(MethodGetDataReaderValue, new Expression[] { typeExp, Expression.Call(rowExp, MethodDataReaderGetValue, dataIndexExp) }),
                            Expression.Add(dataIndexExp, Expression.Constant(1))
                        ), new[] { typeExp, indexesExp, rowExp, dataIndexExp, commonUtilExp }).Compile();

                    if (type.Namespace == "System" && (type.FullName == "System.String" || type.IsValueType))
                    { //值类型，或者元组
                        bool isTuple = type.Name.StartsWith("ValueTuple`");
                        if (isTuple)
                        {
                            var ret2Exp = Expression.Variable(type, "ret");
                            var read2Exp = Expression.Variable(typeof(RowInfo), "read");
                            var read2ExpValue = Expression.MakeMemberAccess(read2Exp, RowInfo.PropertyValue);
                            var read2ExpDataIndex = Expression.MakeMemberAccess(read2Exp, RowInfo.PropertyDataIndex);
                            var block2Exp = new List<Expression>();

                            var fields = type.GetFields();
                            foreach (var field in fields)
                            {
                                Expression read2ExpAssign = null; //加速缓存
                                if (field.FieldType.IsArray) read2ExpAssign = Expression.New(RowInfo.Constructor,
                                    GetDataReaderValueBlockExpression(field.FieldType, Expression.Call(rowExp, MethodDataReaderGetValue, dataIndexExp)),
                                    //Expression.Call(MethodGetDataReaderValue, new Expression[] { Expression.Constant(field.FieldType), Expression.Call(rowExp, MethodDataReaderGetValue, dataIndexExp) }),
                                    Expression.Add(dataIndexExp, Expression.Constant(1))
                                );
                                else
                                {
                                    var fieldtypeGeneric = field.FieldType;
                                    if (fieldtypeGeneric.IsNullableType()) fieldtypeGeneric = fieldtypeGeneric.GenericTypeArguments.First();
                                    if (fieldtypeGeneric.IsEnum ||
                                        dicExecuteArrayRowReadClassOrTuple.ContainsKey(fieldtypeGeneric)) read2ExpAssign = Expression.New(RowInfo.Constructor,
                                            GetDataReaderValueBlockExpression(field.FieldType, Expression.Call(rowExp, MethodDataReaderGetValue, dataIndexExp)),
                                            //Expression.Call(MethodGetDataReaderValue, new Expression[] { Expression.Constant(field.FieldType), Expression.Call(rowExp, MethodDataReaderGetValue, dataIndexExp) }),
                                            Expression.Add(dataIndexExp, Expression.Constant(1))
                                    );
                                    else
                                    {
                                        read2ExpAssign = Expression.Call(MethodExecuteArrayRowReadClassOrTuple, new Expression[] { Expression.Constant(flagStr), Expression.Constant(field.FieldType), indexesExp, rowExp, dataIndexExp, commonUtilExp });
                                    }
                                }
                                block2Exp.AddRange(new Expression[] {
								//Expression.TryCatch(Expression.Block(
								//	typeof(void),
									Expression.Assign(read2Exp, read2ExpAssign),
                                    Expression.IfThen(Expression.GreaterThan(read2ExpDataIndex, dataIndexExp),
                                        Expression.Assign(dataIndexExp, read2ExpDataIndex)),
                                    Expression.IfThenElse(Expression.Equal(read2ExpValue, Expression.Constant(null)),
                                        Expression.Assign(Expression.MakeMemberAccess(ret2Exp, field), Expression.Default(field.FieldType)),
                                        Expression.Assign(Expression.MakeMemberAccess(ret2Exp, field), Expression.Convert(read2ExpValue, field.FieldType)))
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
                            return Expression.Lambda<Func<Type, int[], DbDataReader, int, CommonUtils, RowInfo>>(
                                Expression.Block(new[] { ret2Exp, read2Exp }, block2Exp), new[] { typeExp, indexesExp, rowExp, dataIndexExp, commonUtilExp }).Compile();
                        }
                        var rowLenExp = Expression.ArrayLength(rowExp);
                        return Expression.Lambda<Func<Type, int[], DbDataReader, int, CommonUtils, RowInfo>>(
                            Expression.Block(
                                Expression.IfThen(
                                    Expression.LessThan(dataIndexExp, rowLenExp),
                                        Expression.Return(returnTarget, Expression.New(RowInfo.Constructor,
                                            GetDataReaderValueBlockExpression(type, Expression.Call(rowExp, MethodDataReaderGetValue, dataIndexExp)),
                                            //Expression.Call(MethodGetDataReaderValue, new Expression[] { typeExp, Expression.Call(rowExp, MethodDataReaderGetValue, dataIndexExp) }),
                                            Expression.Add(dataIndexExp, Expression.Constant(1))))
                                ),
                                Expression.Label(returnTarget, Expression.Default(typeof(RowInfo)))
                            ), new[] { typeExp, indexesExp, rowExp, dataIndexExp, commonUtilExp }).Compile();
                    }

                    if (type == typeof(object) && indexes != null)
                    {
                        Func<Type, int[], DbDataReader, int, CommonUtils, RowInfo> dynamicFunc = (type2, indexes2, row2, dataindex2, commonUtils2) =>
                        {
                            //dynamic expando = new DynamicDictionary(); //动态类型字段 可读可写
                            var expandodic = new Dictionary<string, object>();// (IDictionary<string, object>)expando;
                            var fc = row2.FieldCount;
                            for (var a = 0; a < fc; a++)
                                //expando[row2.GetName(a)] = row2.GetValue(a);
                                expandodic.Add(row2.GetName(a), row2.GetValue(a));
                            //expando = expandodic;
                            return new RowInfo(expandodic, fc);
                        };
                        return dynamicFunc;// Expression.Lambda<Func<Type, int[], DbDataReader, int, RowInfo>>(null);
                    }

                    //类注入属性
                    var typetb = GetTableByEntity(type, _commonUtils);
                    var retExp = Expression.Variable(type, "ret");
                    var readExp = Expression.Variable(typeof(RowInfo), "read");
                    var readExpValue = Expression.MakeMemberAccess(readExp, RowInfo.PropertyValue);
                    var readExpDataIndex = Expression.MakeMemberAccess(readExp, RowInfo.PropertyDataIndex);
                    var readExpValueParms = new List<ParameterExpression>();
                    var readExpsIndex = Expression.Variable(typeof(int), "readsIndex");
                    var tryidxExp = Expression.Variable(typeof(int), "tryidx");
                    var readpknullExp = Expression.Variable(typeof(bool), "isnull2");
                    var readpkvalExp = Expression.Variable(typeof(object), "isnull3val");
                    var indexesLengthExp = Expression.Variable(typeof(int), "indexesLength");
                    var blockExp = new List<Expression>();
                    var ctor = type.GetConstructor(new Type[0]) ?? type.GetConstructors().First();
                    var ctorParms = ctor.GetParameters();
                    if (ctorParms.Length > 0)
                    {
                        blockExp.AddRange(new Expression[] {
                        Expression.Assign(readpknullExp, Expression.Constant(false))
                    });
                        foreach (var ctorParm in ctorParms)
                        {
                            if (typetb.ColumnsByCsIgnore.ContainsKey(ctorParm.Name)) continue;
                            var readType = typetb.ColumnsByCs.TryGetValue(ctorParm.Name, out var trycol) ? trycol.Attribute.MapType : ctorParm.ParameterType;

                            var ispkExp = new List<Expression>();
                            Expression readVal = Expression.Assign(readpkvalExp, Expression.Call(rowExp, MethodDataReaderGetValue, dataIndexExp));
                            Expression readExpAssign = null; //加速缓存
                            if (readType.IsArray) readExpAssign = Expression.New(RowInfo.Constructor,
                                GetDataReaderValueBlockExpression(readType, readpkvalExp),
                                //Expression.Call(MethodGetDataReaderValue, new Expression[] { Expression.Constant(readType), readpkvalExp }),
                                Expression.Add(dataIndexExp, Expression.Constant(1))
                            );
                            else
                            {
                                var proptypeGeneric = readType;
                                if (proptypeGeneric.IsNullableType()) proptypeGeneric = proptypeGeneric.GenericTypeArguments.First();
                                if (proptypeGeneric.IsEnum ||
                                    dicExecuteArrayRowReadClassOrTuple.ContainsKey(proptypeGeneric))
                                {

                                    //判断主键为空，则整个对象不读取
                                    //blockExp.Add(Expression.Assign(readpkvalExp, Expression.Call(rowExp, MethodDataReaderGetValue, dataIndexExp)));
                                    if (trycol?.Attribute.IsPrimary == true)
                                    {
                                        ispkExp.Add(
                                            Expression.IfThen(
                                                Expression.AndAlso(
                                                    Expression.IsFalse(readpknullExp),
                                                    Expression.Or(
                                                        Expression.Equal(readpkvalExp, Expression.Constant(DBNull.Value)),
                                                        Expression.Equal(readpkvalExp, Expression.Constant(null))
                                                    )
                                                ),
                                                Expression.Assign(readpknullExp, Expression.Constant(true))
                                            )
                                        );
                                    }

                                    readExpAssign = Expression.New(RowInfo.Constructor,
                                        GetDataReaderValueBlockExpression(readType, readpkvalExp),
                                        //Expression.Call(MethodGetDataReaderValue, new Expression[] { Expression.Constant(readType), readpkvalExp }),
                                        Expression.Add(dataIndexExp, Expression.Constant(1))
                                    );
                                }
                                else
                                {
                                    readExpAssign = Expression.New(RowInfo.Constructor,
                                        Expression.MakeMemberAccess(Expression.Call(MethodExecuteArrayRowReadClassOrTuple, new Expression[] { Expression.Constant(flagStr), Expression.Constant(readType), indexesExp, rowExp, dataIndexExp, commonUtilExp }), RowInfo.PropertyValue),
                                        Expression.Add(dataIndexExp, Expression.Constant(1)));
                                }
                            }
                            var varctorParm = Expression.Variable(ctorParm.ParameterType, $"ctorParm{ctorParm.Name}");
                            readExpValueParms.Add(varctorParm);

                            if (trycol != null && trycol.Attribute.MapType != ctorParm.ParameterType)
                                ispkExp.Add(Expression.Assign(readExpValue, GetDataReaderValueBlockExpression(ctorParm.ParameterType, readExpValue)));

                            ispkExp.Add(
                                Expression.IfThen(
                                    Expression.IsFalse(readpknullExp),
                                    Expression.IfThenElse(
                                        Expression.Equal(readExpValue, Expression.Constant(null)),
                                        Expression.Assign(varctorParm, Expression.Default(ctorParm.ParameterType)),
                                        Expression.Assign(varctorParm, Expression.Convert(readExpValue, ctorParm.ParameterType))
                                    )
                                )
                            );

                            blockExp.AddRange(new Expression[] {
                            Expression.Assign(tryidxExp, dataIndexExp),
                            readVal,
                            Expression.Assign(readExp, readExpAssign),
                            Expression.IfThen(Expression.GreaterThan(readExpDataIndex, dataIndexExp),
                                Expression.Assign(dataIndexExp, readExpDataIndex)
                            ),
                            Expression.Block(ispkExp)
                        });
                        }
                        blockExp.Add(
                            Expression.IfThen(
                                Expression.IsFalse(readpknullExp),
                                Expression.Assign(retExp, Expression.New(ctor, readExpValueParms))
                            )
                        );
                    }
                    else
                    {
                        blockExp.AddRange(new Expression[] {
                        Expression.Assign(retExp, Expression.New(ctor)),
                        Expression.Assign(indexesLengthExp, Expression.Constant(0)),
                        Expression.IfThen(
                            Expression.NotEqual(indexesExp, Expression.Constant(null)),
                            Expression.Assign(indexesLengthExp, Expression.ArrayLength(indexesExp))
                        ),
                        Expression.Assign(readpknullExp, Expression.Constant(false))
                    });

                        var props = type.GetProperties();//.ToDictionary(a => a.Name, a => a, StringComparer.CurrentCultureIgnoreCase);
                        var propIndex = 0;
                        foreach (var prop in props)
                        {
                            if (typetb.ColumnsByCsIgnore.ContainsKey(prop.Name))
                            {
                                ++propIndex;
                                continue;
                            }
                            var readType = typetb.ColumnsByCs.TryGetValue(prop.Name, out var trycol) ? trycol.Attribute.MapType : prop.PropertyType;

                            var ispkExp = new List<Expression>();
                            var propGetSetMethod = prop.GetSetMethod();
                            Expression readVal = Expression.Assign(readpkvalExp, Expression.Call(rowExp, MethodDataReaderGetValue, tryidxExp));
                            Expression readExpAssign = null; //加速缓存
                            if (readType.IsArray) readExpAssign = Expression.New(RowInfo.Constructor,
                                GetDataReaderValueBlockExpression(readType, readpkvalExp),
                                //Expression.Call(MethodGetDataReaderValue, new Expression[] { Expression.Constant(readType), readpkvalExp }),
                                Expression.Add(tryidxExp, Expression.Constant(1))
                            );
                            else
                            {
                                var proptypeGeneric = readType;
                                if (proptypeGeneric.IsNullableType()) proptypeGeneric = proptypeGeneric.GenericTypeArguments.First();
                                if (proptypeGeneric.IsEnum ||
                                    dicExecuteArrayRowReadClassOrTuple.ContainsKey(proptypeGeneric))
                                {

                                    //判断主键为空，则整个对象不读取
                                    //blockExp.Add(Expression.Assign(readpkvalExp, Expression.Call(rowExp, MethodDataReaderGetValue, dataIndexExp)));
                                    if (trycol?.Attribute.IsPrimary == true)
                                    {
                                        ispkExp.Add(
                                            Expression.IfThen(
                                                Expression.AndAlso(
                                                    Expression.IsFalse(readpknullExp),
                                                    Expression.Or(
                                                        Expression.Equal(readpkvalExp, Expression.Constant(DBNull.Value)),
                                                        Expression.Equal(readpkvalExp, Expression.Constant(null))
                                                    )
                                                ),
                                                Expression.Block(
                                                    Expression.Assign(readpknullExp, Expression.Constant(true)),
                                                    Expression.Assign(retExp, Expression.Constant(null, type))
                                                )
                                            )
                                        );
                                    }

                                    readExpAssign = Expression.New(RowInfo.Constructor,
                                        GetDataReaderValueBlockExpression(readType, readpkvalExp),
                                        //Expression.Call(MethodGetDataReaderValue, new Expression[] { Expression.Constant(readType), readpkvalExp }),
                                        Expression.Add(tryidxExp, Expression.Constant(1))
                                    );
                                }
                                else
                                {
                                    ++propIndex;
                                    continue;
                                    //readExpAssign = Expression.Call(MethodExecuteArrayRowReadClassOrTuple, new Expression[] { Expression.Constant(flagStr), Expression.Constant(readType), indexesExp, rowExp, tryidxExp });
                                }
                            }

                            if (trycol != null && trycol.Attribute.MapType != prop.PropertyType)
                                ispkExp.Add(Expression.Assign(readExpValue, GetDataReaderValueBlockExpression(prop.PropertyType, readExpValue)));

                            ispkExp.Add(
                                Expression.IfThen(
                                    Expression.IsFalse(readpknullExp),
                                    Expression.IfThenElse(
                                        Expression.Equal(readExpValue, Expression.Constant(null)),
                                        Expression.Call(retExp, propGetSetMethod, Expression.Default(prop.PropertyType)),
                                        Expression.Call(retExp, propGetSetMethod, Expression.Convert(readExpValue, prop.PropertyType))
                                    )
                                )
                            );
                            blockExp.AddRange(new Expression[] {
							//以下注释部分为【严格读取】，会损失一点性能，使用 select * from xxx 与属性映射赋值
							Expression.IfThenElse(
                                Expression.LessThan(Expression.Constant(propIndex), indexesLengthExp),
                                Expression.Assign(tryidxExp, Expression.ArrayAccess(indexesExp, Expression.Constant(propIndex))),
                                Expression.Assign(tryidxExp, dataIndexExp)
                            ),
                            Expression.IfThen(
                                Expression.GreaterThanOrEqual(tryidxExp, Expression.Constant(0)),
                                Expression.Block(
                                    readVal,
                                    Expression.Assign(readExp, readExpAssign),
                                    Expression.IfThen(Expression.GreaterThan(readExpDataIndex, dataIndexExp),
                                        Expression.Assign(dataIndexExp, readExpDataIndex)),
                                    Expression.Block(ispkExp)
                                )
                            )
                        });
                            ++propIndex;
                        }
                    }
                    blockExp.AddRange(new Expression[] {
                    Expression.Return(returnTarget, Expression.New(RowInfo.Constructor, retExp, dataIndexExp)),
                    Expression.Label(returnTarget, Expression.Default(typeof(RowInfo)))
                });
                    return Expression.Lambda<Func<Type, int[], DbDataReader, int, CommonUtils, RowInfo>>(
                        Expression.Block(new[] { retExp, readExp, tryidxExp, readpknullExp, readpkvalExp, readExpsIndex, indexesLengthExp }.Concat(readExpValueParms), blockExp), new[] { typeExp, indexesExp, rowExp, dataIndexExp, commonUtilExp }).Compile();
                });
            return func(typeOrg, indexes, row, dataIndex, _commonUtils);
        }

        internal static MethodInfo MethodExecuteArrayRowReadClassOrTuple = typeof(Utils).GetMethod("ExecuteArrayRowReadClassOrTuple", BindingFlags.Static | BindingFlags.NonPublic);
        internal static MethodInfo MethodGetDataReaderValue = typeof(Utils).GetMethod("GetDataReaderValue", BindingFlags.Static | BindingFlags.NonPublic);

        static ConcurrentDictionary<string, Action<object, object>> _dicFillPropertyValue = new ConcurrentDictionary<string, Action<object, object>>();
        internal static void FillPropertyValue(object info, string memberAccessPath, object value)
        {
            var typeObj = info.GetType();
            var typeValue = value.GetType();
            var key = "FillPropertyValue_" + typeObj.FullName + "_" + typeValue.FullName;
            var act = _dicFillPropertyValue.GetOrAdd($"{key}.{memberAccessPath}", s =>
            {
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

        static BigInteger ToBigInteger(string that)
        {
            if (string.IsNullOrEmpty(that)) return 0;
            if (BigInteger.TryParse(that, System.Globalization.NumberStyles.Any, null, out var trybigint)) return trybigint;
            return 0;
        }
        static string ToStringConcat(object obj)
        {
            if (obj == null) return null;
            return string.Concat(obj);
        }

        static ConcurrentDictionary<Type, ConcurrentDictionary<Type, Func<object, object>>> _dicGetDataReaderValue = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, Func<object, object>>>();
        static MethodInfo MethodArrayGetValue = typeof(Array).GetMethod("GetValue", new[] { typeof(int) });
        static MethodInfo MethodArrayGetLength = typeof(Array).GetMethod("GetLength", new[] { typeof(int) });
        static MethodInfo MethodGuidTryParse = typeof(Guid).GetMethod("TryParse", new[] { typeof(string), typeof(Guid).MakeByRefType() });
        static MethodInfo MethodEnumParse = typeof(Enum).GetMethod("Parse", new[] { typeof(Type), typeof(string), typeof(bool) });
        static MethodInfo MethodConvertChangeType = typeof(Convert).GetMethod("ChangeType", new[] { typeof(object), typeof(Type) });
        static MethodInfo MethodTimeSpanFromSeconds = typeof(TimeSpan).GetMethod("FromSeconds");
        static MethodInfo MethodSByteTryParse = typeof(sbyte).GetMethod("TryParse", new[] { typeof(string), typeof(sbyte).MakeByRefType() });
        static MethodInfo MethodShortTryParse = typeof(short).GetMethod("TryParse", new[] { typeof(string), typeof(short).MakeByRefType() });
        static MethodInfo MethodIntTryParse = typeof(int).GetMethod("TryParse", new[] { typeof(string), typeof(int).MakeByRefType() });
        static MethodInfo MethodLongTryParse = typeof(long).GetMethod("TryParse", new[] { typeof(string), typeof(long).MakeByRefType() });
        static MethodInfo MethodByteTryParse = typeof(byte).GetMethod("TryParse", new[] { typeof(string), typeof(byte).MakeByRefType() });
        static MethodInfo MethodUShortTryParse = typeof(ushort).GetMethod("TryParse", new[] { typeof(string), typeof(ushort).MakeByRefType() });
        static MethodInfo MethodUIntTryParse = typeof(uint).GetMethod("TryParse", new[] { typeof(string), typeof(uint).MakeByRefType() });
        static MethodInfo MethodULongTryParse = typeof(ulong).GetMethod("TryParse", new[] { typeof(string), typeof(ulong).MakeByRefType() });
        static MethodInfo MethodDoubleTryParse = typeof(double).GetMethod("TryParse", new[] { typeof(string), typeof(double).MakeByRefType() });
        static MethodInfo MethodFloatTryParse = typeof(float).GetMethod("TryParse", new[] { typeof(string), typeof(float).MakeByRefType() });
        static MethodInfo MethodDecimalTryParse = typeof(decimal).GetMethod("TryParse", new[] { typeof(string), typeof(decimal).MakeByRefType() });
        static MethodInfo MethodTimeSpanTryParse = typeof(TimeSpan).GetMethod("TryParse", new[] { typeof(string), typeof(TimeSpan).MakeByRefType() });
        static MethodInfo MethodDateTimeTryParse = typeof(DateTime).GetMethod("TryParse", new[] { typeof(string), typeof(DateTime).MakeByRefType() });
        static MethodInfo MethodDateTimeOffsetTryParse = typeof(DateTimeOffset).GetMethod("TryParse", new[] { typeof(string), typeof(DateTimeOffset).MakeByRefType() });
        static MethodInfo MethodToString = typeof(Utils).GetMethod("ToStringConcat", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(object) }, null);
        static MethodInfo MethodBigIntegerParse = typeof(Utils).GetMethod("ToBigInteger", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(string) }, null);

        public static ConcurrentBag<Func<LabelTarget, Expression, string, Expression>> GetDataReaderValueBlockExpressionSwitchTypeFullName = new ConcurrentBag<Func<LabelTarget, Expression, string, Expression>>();
        public static Expression GetDataReaderValueBlockExpression(Type type, Expression value)
        {
            var returnTarget = Expression.Label(typeof(object));
            var valueExp = Expression.Variable(typeof(object), "locvalue");
            Func<Expression> funcGetExpression = () =>
            {
                if (type.FullName == "System.Byte[]") return Expression.Return(returnTarget, valueExp);
                if (type.IsArray)
                {
                    var elementType = type.GetElementType();
                    var arrNewExp = Expression.Variable(type, "arrNew");
                    var arrExp = Expression.Variable(typeof(Array), "arr");
                    var arrLenExp = Expression.Variable(typeof(int), "arrLen");
                    var arrXExp = Expression.Variable(typeof(int), "arrX");
                    var arrReadValExp = Expression.Variable(typeof(object), "arrReadVal");
                    var label = Expression.Label(typeof(int));
                    return Expression.IfThenElse(
                        Expression.TypeEqual(valueExp, type),
                        Expression.Return(returnTarget, valueExp),
                        Expression.Block(
                            new[] { arrNewExp, arrExp, arrLenExp, arrXExp, arrReadValExp },
                            Expression.Assign(arrExp, Expression.TypeAs(valueExp, typeof(Array))),
                            Expression.Assign(arrLenExp, Expression.Call(arrExp, MethodArrayGetLength, Expression.Constant(0))),
                            Expression.Assign(arrXExp, Expression.Constant(0)),
                            Expression.Assign(arrNewExp, Expression.NewArrayBounds(elementType, arrLenExp)),
                            Expression.Loop(
                                Expression.IfThenElse(
                                    Expression.LessThan(arrXExp, arrLenExp),
                                    Expression.Block(
                                        Expression.Assign(arrReadValExp, GetDataReaderValueBlockExpression(elementType, Expression.Call(arrExp, MethodArrayGetValue, arrXExp))),
                                        Expression.IfThenElse(
                                            Expression.Equal(arrReadValExp, Expression.Constant(null)),
                                            Expression.Assign(Expression.ArrayAccess(arrNewExp, arrXExp), Expression.Default(elementType)),
                                            Expression.Assign(Expression.ArrayAccess(arrNewExp, arrXExp), Expression.Convert(arrReadValExp, elementType))
                                        ),
                                        Expression.PostIncrementAssign(arrXExp)
                                    ),
                                    Expression.Break(label, arrXExp)
                                ),
                                label
                            ),
                            Expression.Return(returnTarget, arrNewExp)
                        )
                    );
                }
                var typeOrg = type;
                if (type.IsNullableType()) type = type.GenericTypeArguments.First();
                if (type.IsEnum)
                    return Expression.Block(
                        Expression.IfThenElse(
                            Expression.Equal(Expression.TypeAs(valueExp, typeof(string)), Expression.Constant(string.Empty)),
                            Expression.Return(returnTarget, Expression.Convert(Expression.Default(type), typeof(object))),
                            Expression.Return(returnTarget, Expression.Call(MethodEnumParse, Expression.Constant(type, typeof(Type)), Expression.Call(MethodToString, valueExp), Expression.Constant(true, typeof(bool))))
                        )
                    );
                Expression tryparseExp = null;
                Expression tryparseBooleanExp = null;
                ParameterExpression tryparseVarExp = null;
                switch (type.FullName)
                {
                    case "System.Guid":
                        tryparseExp = Expression.Block(
                           new[] { tryparseVarExp = Expression.Variable(typeof(Guid)) },
                           new Expression[] {
                                Expression.IfThenElse(
                                    Expression.IsTrue(Expression.Call(MethodGuidTryParse, Expression.Convert(valueExp, typeof(string)), tryparseVarExp)),
                                    Expression.Return(returnTarget, Expression.Convert(tryparseVarExp, typeof(object))),
                                    Expression.Return(returnTarget, Expression.Convert(Expression.Default(typeOrg), typeof(object)))
                                )
                               }
                           );
                        break;
                    case "System.Numerics.BigInteger": return Expression.Return(returnTarget, Expression.Convert(Expression.Call(MethodBigIntegerParse, Expression.Call(MethodToString, valueExp)), typeof(object)));
                    case "System.TimeSpan":
                        ParameterExpression tryparseVarTsExp, valueStrExp;
                        return Expression.Block(
                               new[] { tryparseVarExp = Expression.Variable(typeof(double)), tryparseVarTsExp = Expression.Variable(typeof(TimeSpan)), valueStrExp = Expression.Variable(typeof(string)) },
                               new Expression[] {
                                    Expression.Assign(valueStrExp, Expression.Call(MethodToString, valueExp)),
                                    Expression.IfThenElse(
                                        Expression.IsTrue(Expression.Call(MethodDoubleTryParse, valueStrExp, tryparseVarExp)),
                                        Expression.Return(returnTarget, Expression.Convert(Expression.Call(MethodTimeSpanFromSeconds, tryparseVarExp), typeof(object))),
                                        Expression.IfThenElse(
                                            Expression.IsTrue(Expression.Call(MethodTimeSpanTryParse, valueStrExp, tryparseVarTsExp)),
                                            Expression.Return(returnTarget, Expression.Convert(tryparseVarTsExp, typeof(object))),
                                            Expression.Return(returnTarget, Expression.Convert(Expression.Default(typeOrg), typeof(object)))
                                        )
                                    )
                               }
                           );
                    case "System.SByte":
                        tryparseExp = Expression.Block(
                           new[] { tryparseVarExp = Expression.Variable(typeof(sbyte)) },
                           new Expression[] {
                                Expression.IfThenElse(
                                    Expression.IsTrue(Expression.Call(MethodSByteTryParse, Expression.Convert(valueExp, typeof(string)), tryparseVarExp)),
                                    Expression.Return(returnTarget, Expression.Convert(tryparseVarExp, typeof(object))),
                                    Expression.Return(returnTarget, Expression.Convert(Expression.Default(typeOrg), typeof(object)))
                                )
                               }
                           );
                        break;
                    case "System.Int16":
                        tryparseExp = Expression.Block(
                              new[] { tryparseVarExp = Expression.Variable(typeof(short)) },
                               new Expression[] {
                                Expression.IfThenElse(
                                    Expression.IsTrue(Expression.Call(MethodShortTryParse, Expression.Convert(valueExp, typeof(string)), tryparseVarExp)),
                                    Expression.Return(returnTarget, Expression.Convert(tryparseVarExp, typeof(object))),
                                    Expression.Return(returnTarget, Expression.Convert(Expression.Default(typeOrg), typeof(object)))
                                )
                               }
                           );
                        break;
                    case "System.Int32":
                        tryparseExp = Expression.Block(
                              new[] { tryparseVarExp = Expression.Variable(typeof(int)) },
                               new Expression[] {
                                Expression.IfThenElse(
                                    Expression.IsTrue(Expression.Call(MethodIntTryParse, Expression.Convert(valueExp, typeof(string)), tryparseVarExp)),
                                    Expression.Return(returnTarget, Expression.Convert(tryparseVarExp, typeof(object))),
                                    Expression.Return(returnTarget, Expression.Convert(Expression.Default(typeOrg), typeof(object)))
                                )
                               }
                           );
                        break;
                    case "System.Int64":
                        tryparseExp = Expression.Block(
                              new[] { tryparseVarExp = Expression.Variable(typeof(long)) },
                               new Expression[] {
                                Expression.IfThenElse(
                                    Expression.IsTrue(Expression.Call(MethodLongTryParse, Expression.Convert(valueExp, typeof(string)), tryparseVarExp)),
                                    Expression.Return(returnTarget, Expression.Convert(tryparseVarExp, typeof(object))),
                                    Expression.Return(returnTarget, Expression.Convert(Expression.Default(typeOrg), typeof(object)))
                                )
                               }
                           );
                        break;
                    case "System.Byte":
                        tryparseExp = Expression.Block(
                              new[] { tryparseVarExp = Expression.Variable(typeof(byte)) },
                               new Expression[] {
                                Expression.IfThenElse(
                                    Expression.IsTrue(Expression.Call(MethodByteTryParse, Expression.Convert(valueExp, typeof(string)), tryparseVarExp)),
                                    Expression.Return(returnTarget, Expression.Convert(tryparseVarExp, typeof(object))),
                                    Expression.Return(returnTarget, Expression.Convert(Expression.Default(typeOrg), typeof(object)))
                                )
                               }
                           );
                        break;
                    case "System.UInt16":
                        tryparseExp = Expression.Block(
                               new[] { tryparseVarExp = Expression.Variable(typeof(ushort)) },
                               new Expression[] {
                                Expression.IfThenElse(
                                    Expression.IsTrue(Expression.Call(MethodUShortTryParse, Expression.Convert(valueExp, typeof(string)), tryparseVarExp)),
                                    Expression.Return(returnTarget, Expression.Convert(tryparseVarExp, typeof(object))),
                                    Expression.Return(returnTarget, Expression.Convert(Expression.Default(typeOrg), typeof(object)))
                                )
                               }
                           );
                        break;
                    case "System.UInt32":
                        tryparseExp = Expression.Block(
                               new[] { tryparseVarExp = Expression.Variable(typeof(uint)) },
                               new Expression[] {
                                Expression.IfThenElse(
                                    Expression.IsTrue(Expression.Call(MethodUIntTryParse, Expression.Convert(valueExp, typeof(string)), tryparseVarExp)),
                                    Expression.Return(returnTarget, Expression.Convert(tryparseVarExp, typeof(object))),
                                    Expression.Return(returnTarget, Expression.Convert(Expression.Default(typeOrg), typeof(object)))
                                )
                               }
                           );
                        break;
                    case "System.UInt64":
                        tryparseExp = Expression.Block(
                              new[] { tryparseVarExp = Expression.Variable(typeof(ulong)) },
                               new Expression[] {
                                Expression.IfThenElse(
                                    Expression.IsTrue(Expression.Call(MethodULongTryParse, Expression.Convert(valueExp, typeof(string)), tryparseVarExp)),
                                    Expression.Return(returnTarget, Expression.Convert(tryparseVarExp, typeof(object))),
                                    Expression.Return(returnTarget, Expression.Convert(Expression.Default(typeOrg), typeof(object)))
                                )
                               }
                           );
                        break;
                    case "System.Single":
                        tryparseExp = Expression.Block(
                              new[] { tryparseVarExp = Expression.Variable(typeof(float)) },
                               new Expression[] {
                                Expression.IfThenElse(
                                    Expression.IsTrue(Expression.Call(MethodFloatTryParse, Expression.Convert(valueExp, typeof(string)), tryparseVarExp)),
                                    Expression.Return(returnTarget, Expression.Convert(tryparseVarExp, typeof(object))),
                                    Expression.Return(returnTarget, Expression.Convert(Expression.Default(typeOrg), typeof(object)))
                                )
                               }
                           );
                        break;
                    case "System.Double":
                        tryparseExp = Expression.Block(
                               new[] { tryparseVarExp = Expression.Variable(typeof(double)) },
                               new Expression[] {
                                Expression.IfThenElse(
                                    Expression.IsTrue(Expression.Call(MethodDoubleTryParse, Expression.Convert(valueExp, typeof(string)), tryparseVarExp)),
                                    Expression.Return(returnTarget, Expression.Convert(tryparseVarExp, typeof(object))),
                                    Expression.Return(returnTarget, Expression.Convert(Expression.Default(typeOrg), typeof(object)))
                                )
                               }
                           );
                        break;
                    case "System.Decimal":
                        tryparseExp = Expression.Block(
                              new[] { tryparseVarExp = Expression.Variable(typeof(decimal)) },
                               new Expression[] {
                                Expression.IfThenElse(
                                    Expression.IsTrue(Expression.Call(MethodDecimalTryParse, Expression.Convert(valueExp, typeof(string)), tryparseVarExp)),
                                    Expression.Return(returnTarget, Expression.Convert(tryparseVarExp, typeof(object))),
                                    Expression.Return(returnTarget, Expression.Convert(Expression.Default(typeOrg), typeof(object)))
                                )
                               }
                           );
                        break;
                    case "System.DateTime":
                        tryparseExp = Expression.Block(
                              new[] { tryparseVarExp = Expression.Variable(typeof(DateTime)) },
                               new Expression[] {
                                Expression.IfThenElse(
                                    Expression.IsTrue(Expression.Call(MethodDateTimeTryParse, Expression.Convert(valueExp, typeof(string)), tryparseVarExp)),
                                    Expression.Return(returnTarget, Expression.Convert(tryparseVarExp, typeof(object))),
                                    Expression.Return(returnTarget, Expression.Convert(Expression.Default(typeOrg), typeof(object)))
                                )
                               }
                           );
                        break;
                    case "System.DateTimeOffset":
                        tryparseExp = Expression.Block(
                              new[] { tryparseVarExp = Expression.Variable(typeof(DateTimeOffset)) },
                               new Expression[] {
                                Expression.IfThenElse(
                                    Expression.IsTrue(Expression.Call(MethodDateTimeOffsetTryParse, Expression.Convert(valueExp, typeof(string)), tryparseVarExp)),
                                    Expression.Return(returnTarget, Expression.Convert(tryparseVarExp, typeof(object))),
                                    Expression.Return(returnTarget, Expression.Convert(Expression.Default(typeOrg), typeof(object)))
                                )
                               }
                           );
                        break;
                    case "System.Boolean":
                        tryparseBooleanExp = Expression.Return(returnTarget,
                                Expression.Convert(
                                    Expression.Not(
                                        Expression.Or(
                                            Expression.Equal(Expression.Convert(valueExp, typeof(string)), Expression.Constant("False")),
                                        Expression.Or(
                                            Expression.Equal(Expression.Convert(valueExp, typeof(string)), Expression.Constant("false")),
                                            Expression.Equal(Expression.Convert(valueExp, typeof(string)), Expression.Constant("0"))))),
                            typeof(object))
                        );
                        break;
                    default:
                        foreach (var switchFunc in GetDataReaderValueBlockExpressionSwitchTypeFullName)
                        {
                            var switchFuncRet = switchFunc(returnTarget, valueExp, type.FullName);
                            if (switchFuncRet != null) return switchFuncRet;
                        }
                        break;
                }
                Expression switchExp = null;
                if (tryparseExp != null)
                    switchExp = Expression.Switch(
                        Expression.Constant(type),
                        Expression.SwitchCase(tryparseExp,
                            Expression.Constant(typeof(Guid)),
                            Expression.Constant(typeof(sbyte)), Expression.Constant(typeof(short)), Expression.Constant(typeof(int)), Expression.Constant(typeof(long)),
                            Expression.Constant(typeof(byte)), Expression.Constant(typeof(ushort)), Expression.Constant(typeof(uint)), Expression.Constant(typeof(ulong)),
                            Expression.Constant(typeof(double)), Expression.Constant(typeof(float)), Expression.Constant(typeof(decimal)),
                            Expression.Constant(typeof(DateTime)), Expression.Constant(typeof(DateTimeOffset))
                        ),
                        Expression.SwitchCase(Expression.Return(returnTarget, Expression.Call(MethodConvertChangeType, valueExp, Expression.Constant(type, typeof(Type)))), Expression.Constant(type))
                    );
                else if (tryparseBooleanExp != null)
                    switchExp = Expression.Switch(
                        Expression.Constant(type),
                        Expression.SwitchCase(tryparseBooleanExp, Expression.Constant(typeof(bool))),
                        Expression.SwitchCase(Expression.Return(returnTarget, Expression.Call(MethodConvertChangeType, valueExp, Expression.Constant(type, typeof(Type)))), Expression.Constant(type))
                    );
                else if (type == typeof(string))
                    switchExp = Expression.Return(returnTarget, Expression.Convert(Expression.Call(MethodToString, valueExp), typeof(object)));
                else
                    switchExp = Expression.Return(returnTarget, Expression.Call(MethodConvertChangeType, valueExp, Expression.Constant(type, typeof(Type))));

                var defaultRetExp = type == typeof(string) ?
                    Expression.Return(returnTarget, Expression.Convert(Expression.Call(MethodToString, valueExp), typeof(object))) :
                    Expression.Return(returnTarget, Expression.Call(MethodConvertChangeType, valueExp, Expression.Constant(type, typeof(Type))));

                return Expression.IfThenElse(
                    Expression.TypeEqual(valueExp, type),
                    Expression.Return(returnTarget, valueExp),
                    Expression.IfThenElse(
                        Expression.TypeEqual(valueExp, typeof(string)),
                        switchExp,
                        defaultRetExp
                    )
                );
            };

            return Expression.Block(
                new[] { valueExp },
                Expression.Assign(valueExp, Expression.Convert(value, typeof(object))),
                Expression.IfThenElse(
                    Expression.Or(
                        Expression.Equal(valueExp, Expression.Constant(null)),
                        Expression.Equal(valueExp, Expression.Constant(DBNull.Value))
                    ),
                    Expression.Return(returnTarget, Expression.Convert(Expression.Default(type), typeof(object))),
                    funcGetExpression()
                ),
                Expression.Label(returnTarget, Expression.Default(typeof(object)))
            );
        }
        public static object GetDataReaderValue(Type type, object value)
        {
            //if (value == null || value == DBNull.Value) return Activator.CreateInstance(type);
            if (type == null) return value;
            var func = _dicGetDataReaderValue.GetOrAdd(type, k1 => new ConcurrentDictionary<Type, Func<object, object>>()).GetOrAdd(value?.GetType() ?? type, valueType =>
            {
                var parmExp = Expression.Parameter(typeof(object), "value");
                var exp = GetDataReaderValueBlockExpression(type, parmExp);
                return Expression.Lambda<Func<object, object>>(exp, parmExp).Compile();
            });
            try
            {
                return func(value);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"ExpressionTree 转换类型错误，值({string.Concat(value)})，类型({value.GetType().FullName})，目标类型({type.FullName})，{ex.Message}");
            }
        }
        public static string GetCsName(string name)
        {
            name = Regex.Replace(name.TrimStart('@'), @"[^\w]", "_");
            return char.IsLetter(name, 0) ? name : string.Concat("_", name);
        }
    }
}