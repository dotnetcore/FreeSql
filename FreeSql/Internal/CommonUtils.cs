using FreeSql.DataAnnotations;
using FreeSql.DatabaseModel;
using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace FreeSql.Internal
{
    public abstract class CommonUtils
    {

        public abstract string GetNoneParamaterSqlValue(List<DbParameter> specialParams, string specialParamFlag, ColumnInfo col, Type type, object value);
        public abstract DbParameter AppendParamter(List<DbParameter> _params, string parameterName, ColumnInfo col, Type type, object value);
        public abstract DbParameter[] GetDbParamtersByObject(string sql, object obj);
        public abstract string FormatSql(string sql, params object[] args);
        public abstract string QuoteSqlName(params string[] name);
        public abstract string TrimQuoteSqlName(string name);
        public abstract string[] SplitTableName(string name);
        public static string[] GetSplitTableNames(string name, char leftQuote, char rightQuote, int size)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (name.IndexOf(leftQuote) == -1) return name.Split(new[] { '.' }, size);
            name = Regex.Replace(name, 
                (leftQuote == '[' ? "\\" : "") + 
                leftQuote + @"([^" + (rightQuote == ']' ? "\\" : "") + rightQuote + @"]+)" +
                (rightQuote == ']' ? "\\" : "") + 
                rightQuote, m => m.Groups[1].Value.Replace('.', '?'));
            var ret = name.Split(new[] { '.' }, size);
            for (var a = 0; a < ret.Length; a++)
                ret[a] = ret[a].Replace('?', '.');
            return ret;
        }
        public abstract string QuoteParamterName(string name);
        public abstract string IsNull(string sql, object value);
        public abstract string StringConcat(string[] objs, Type[] types);
        public abstract string Mod(string left, string right, Type leftType, Type rightType);
        public abstract string Div(string left, string right, Type leftType, Type rightType);
        public abstract string Now { get; }
        public abstract string NowUtc { get; }
        public abstract string QuoteWriteParamterAdapter(Type type, string paramterName);
        protected abstract string QuoteReadColumnAdapter(Type type, Type mapType, string columnName);
        public string RewriteColumn(ColumnInfo col, string sql)
        {
            if (string.IsNullOrWhiteSpace(col?.Attribute.RewriteSql) == false)
                return string.Format(col.Attribute.RewriteSql, sql);
            return sql;
        }
        public string RereadColumn(ColumnInfo col, string columnName)
        {
            var result = QuoteReadColumnAdapter(col.CsType, col.Attribute.MapType, columnName);
            if (string.IsNullOrWhiteSpace(col?.Attribute.RereadSql) == false)
                return string.Format(col.Attribute.RereadSql, result);
            return result;
        }
        public virtual string FieldAsAlias(string alias) => $" {alias}";
        public virtual string IIF(string test, string ifTrue, string ifElse) => $"case when {test} then {ifTrue} else {ifElse} end";
        public static string BytesSqlRaw(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var vc in bytes) sb.Append(vc.ToString("X2"));
            return sb.ToString();
        }

        public IFreeSql _orm { get; set; }
        public ICodeFirst CodeFirst => _orm.CodeFirst;
        public TableInfo GetTableByEntity(Type entity) => Utils.GetTableByEntity(entity, this);
        public List<DbTableInfo> dbTables { get; set; }
        public object dbTablesLock = new object();

        public CommonUtils(IFreeSql orm)
        {
            _orm = orm;
        }

        ConcurrentDictionary<Type, TableAttribute> dicConfigEntity = new ConcurrentDictionary<Type, TableAttribute>();
        public ICodeFirst ConfigEntity<T>(Action<TableFluent<T>> entity)
        {
            if (entity == null) return _orm.CodeFirst;
            var type = typeof(T);
            var table = dicConfigEntity.GetOrAdd(type, new TableAttribute());
            var fluent = new TableFluent<T>(table);
            entity.Invoke(fluent);
            Utils.RemoveTableByEntity(type, this); //remove cache
            return _orm.CodeFirst;
        }
        public ICodeFirst ConfigEntity(Type type, Action<TableFluent> entity)
        {
            if (entity == null) return _orm.CodeFirst;
            var table = dicConfigEntity.GetOrAdd(type, new TableAttribute());
            var fluent = new TableFluent(type, table);
            entity.Invoke(fluent);
            Utils.RemoveTableByEntity(type, this); //remove cache
            return _orm.CodeFirst;
        }
        public TableAttribute GetConfigEntity(Type type)
        {
            return dicConfigEntity.TryGetValue(type, out var trytb) ? trytb : null;
        }
        public TableAttribute GetEntityTableAttribute(Type type)
        {
            TableAttribute attr = null;
            if (_orm.Aop.ConfigEntityHandler != null)
            {
                var aope = new Aop.ConfigEntityEventArgs(type);
                _orm.Aop.ConfigEntityHandler(_orm, aope);
                attr = aope.ModifyResult;
            }
            if (attr == null) attr = new TableAttribute();
            if (dicConfigEntity.TryGetValue(type, out var trytb))
            {
                if (!string.IsNullOrEmpty(trytb.Name)) attr.Name = trytb.Name;
                if (!string.IsNullOrEmpty(trytb.OldName)) attr.OldName = trytb.OldName;
                if (trytb._DisableSyncStructure != null) attr._DisableSyncStructure = trytb.DisableSyncStructure;
            }
            var attrs = type.GetCustomAttributes(typeof(TableAttribute), false);
            foreach (var tryattrobj in attrs)
            {
                var tryattr = tryattrobj as TableAttribute;
                if (tryattr == null) continue;
                if (!string.IsNullOrEmpty(tryattr.Name)) attr.Name = tryattr.Name;
                if (!string.IsNullOrEmpty(tryattr.OldName)) attr.OldName = tryattr.OldName;
                if (tryattr._DisableSyncStructure != null) attr._DisableSyncStructure = tryattr.DisableSyncStructure;
            }
            if (!string.IsNullOrEmpty(attr.Name)) return attr;
            if (!string.IsNullOrEmpty(attr.OldName)) return attr;
            if (attr._DisableSyncStructure != null) return attr;
            return null;
        }
        public ColumnAttribute GetEntityColumnAttribute(Type type, PropertyInfo proto)
        {
            ColumnAttribute attr = null;
            if (_orm.Aop.ConfigEntityPropertyHandler != null)
            {
                var aope = new Aop.ConfigEntityPropertyEventArgs(type, proto);
                _orm.Aop.ConfigEntityPropertyHandler(_orm, aope);
                attr = aope.ModifyResult;
            }
            if (attr == null) attr = new ColumnAttribute();
            if (dicConfigEntity.TryGetValue(type, out var trytb) && trytb._columns.TryGetValue(proto.Name, out var trycol))
            {
                if (!string.IsNullOrEmpty(trycol.Name)) attr.Name = trycol.Name;
                if (!string.IsNullOrEmpty(trycol.OldName)) attr.OldName = trycol.OldName;
                if (!string.IsNullOrEmpty(trycol.DbType)) attr.DbType = trycol.DbType;
                if (trycol._IsPrimary != null) attr._IsPrimary = trycol.IsPrimary;
                if (trycol._IsIdentity != null) attr._IsIdentity = trycol.IsIdentity;
                if (trycol._IsNullable != null) attr._IsNullable = trycol.IsNullable;
                if (trycol._IsIgnore != null) attr._IsIgnore = trycol.IsIgnore;
                if (trycol._IsVersion != null) attr._IsVersion = trycol.IsVersion;
                if (trycol.MapType != null) attr.MapType = trycol.MapType;
                if (trycol._Position != null) attr._Position = trycol.Position;
                if (trycol._CanInsert != null) attr._CanInsert = trycol.CanInsert;
                if (trycol._CanUpdate != null) attr._CanUpdate = trycol.CanUpdate;
                if (trycol.ServerTime != DateTimeKind.Unspecified) attr.ServerTime = trycol.ServerTime;
                if (trycol._StringLength != null) attr.StringLength = trycol.StringLength;
                if (!string.IsNullOrEmpty(trycol.InsertValueSql)) attr.InsertValueSql = trycol.InsertValueSql;
                if (trycol._Precision != null) attr.Precision = trycol.Precision;
                if (trycol._Scale != null) attr.Scale = trycol.Scale;
                if (!string.IsNullOrEmpty(trycol.RewriteSql)) attr.RewriteSql = trycol.RewriteSql;
                if (!string.IsNullOrEmpty(trycol.RereadSql)) attr.RereadSql = trycol.RereadSql;
            }
            var attrs = proto.GetCustomAttributes(typeof(ColumnAttribute), false);
            foreach (var tryattrobj in attrs)
            {
                var tryattr = tryattrobj as ColumnAttribute;
                if (tryattr == null) continue;
                if (!string.IsNullOrEmpty(tryattr.Name)) attr.Name = tryattr.Name;
                if (!string.IsNullOrEmpty(tryattr.OldName)) attr.OldName = tryattr.OldName;
                if (!string.IsNullOrEmpty(tryattr.DbType)) attr.DbType = tryattr.DbType;
                if (tryattr._IsPrimary != null) attr._IsPrimary = tryattr.IsPrimary;
                if (tryattr._IsIdentity != null) attr._IsIdentity = tryattr.IsIdentity;
                if (tryattr._IsNullable != null) attr._IsNullable = tryattr.IsNullable;
                if (tryattr._IsIgnore != null) attr._IsIgnore = tryattr.IsIgnore;
                if (tryattr._IsVersion != null) attr._IsVersion = tryattr.IsVersion;
                if (tryattr.MapType != null) attr.MapType = tryattr.MapType;
                if (tryattr._Position != null) attr._Position = tryattr.Position;
                if (tryattr._CanInsert != null) attr._CanInsert = tryattr.CanInsert;
                if (tryattr._CanUpdate != null) attr._CanUpdate = tryattr.CanUpdate;
                if (tryattr.ServerTime != DateTimeKind.Unspecified) attr.ServerTime = tryattr.ServerTime;
                if (tryattr._StringLength != null) attr.StringLength = tryattr.StringLength;
                if (!string.IsNullOrEmpty(tryattr.InsertValueSql)) attr.InsertValueSql = tryattr.InsertValueSql; 
                if (tryattr._Precision != null) attr.Precision = tryattr.Precision;
                if (tryattr._Scale != null) attr.Scale = tryattr.Scale;
                if (!string.IsNullOrEmpty(tryattr.RewriteSql)) attr.RewriteSql = tryattr.RewriteSql;
                if (!string.IsNullOrEmpty(tryattr.RereadSql)) attr.RereadSql = tryattr.RereadSql;
            }
            ColumnAttribute ret = null;
            if (!string.IsNullOrEmpty(attr.Name)) ret = attr;
            if (!string.IsNullOrEmpty(attr.OldName)) ret = attr;
            if (!string.IsNullOrEmpty(attr.DbType)) ret = attr;
            if (attr._IsPrimary != null) ret = attr;
            if (attr._IsIdentity != null) ret = attr;
            if (attr._IsNullable != null) ret = attr;
            if (attr._IsIgnore != null) ret = attr;
            if (attr._IsVersion != null) ret = attr;
            if (attr.MapType != null) ret = attr;
            if (attr._Position != null) ret = attr;
            if (attr._CanInsert != null) ret = attr;
            if (attr._CanUpdate != null) ret = attr;
            if (attr.ServerTime != DateTimeKind.Unspecified) ret = attr;
            if (attr._StringLength != null) ret = attr;
            if (!string.IsNullOrEmpty(attr.InsertValueSql)) ret = attr;
            if (attr._Precision != null) ret = attr;
            if (attr._Scale != null) ret = attr;
            if (!string.IsNullOrEmpty(attr.RewriteSql)) ret = attr;
            if (!string.IsNullOrEmpty(attr.RereadSql)) ret = attr;
            if (ret != null && ret.MapType == null) ret.MapType = proto.PropertyType;
            return ret;
        }
        public NavigateAttribute GetEntityNavigateAttribute(Type type, PropertyInfo proto)
        {
            var attr = new NavigateAttribute();
            if (dicConfigEntity.TryGetValue(type, out var trytb) && trytb._navigates.TryGetValue(proto.Name, out var trynav))
            {
                if (!string.IsNullOrEmpty(trynav.Bind)) attr.Bind = trynav.Bind;
                if (trynav.ManyToMany != null) attr.ManyToMany = trynav.ManyToMany;
            }
            var attrs = proto.GetCustomAttributes(typeof(NavigateAttribute), false);
            foreach (var tryattrobj in attrs)
            {
                trynav = tryattrobj as NavigateAttribute;
                if (trynav == null) continue;
                if (!string.IsNullOrEmpty(trynav.Bind)) attr.Bind = trynav.Bind;
                if (trynav.ManyToMany != null) attr.ManyToMany = trynav.ManyToMany;
            }
            NavigateAttribute ret = null;
            if (!string.IsNullOrEmpty(attr.Bind)) ret = attr;
            if (attr.ManyToMany != null) ret = attr;
            return ret;
        }
        public IndexAttribute[] GetEntityIndexAttribute(Type type)
        {
            var ret = new Dictionary<string, IndexAttribute>();
            if (_orm.Aop.ConfigEntityHandler != null)
            {
                var aope = new Aop.ConfigEntityEventArgs(type);
                _orm.Aop.ConfigEntityHandler(_orm, aope);
                foreach (var idxattr in aope.ModifyIndexResult)
                    if (!string.IsNullOrEmpty(idxattr.Name) && !string.IsNullOrEmpty(idxattr.Fields))
                    {
                        if (ret.ContainsKey(idxattr.Name)) ret.Remove(idxattr.Name);
                        ret.Add(idxattr.Name, new IndexAttribute(idxattr.Name, idxattr.Fields) { _IsUnique = idxattr._IsUnique });
                    }
            }
            if (dicConfigEntity.TryGetValue(type, out var trytb))
            {
                foreach (var idxattr in trytb._indexs.Values)
                    if (!string.IsNullOrEmpty(idxattr.Name) && !string.IsNullOrEmpty(idxattr.Fields))
                    {
                        if (ret.ContainsKey(idxattr.Name)) ret.Remove(idxattr.Name);
                        ret.Add(idxattr.Name, new IndexAttribute(idxattr.Name, idxattr.Fields) { _IsUnique = idxattr._IsUnique });
                    }
            }
            var attrs = type.GetCustomAttributes(typeof(IndexAttribute), true);
            foreach (var tryattrobj in attrs)
            {
                var idxattr = tryattrobj as IndexAttribute;
                if (idxattr == null) continue;
                if (!string.IsNullOrEmpty(idxattr.Name) && !string.IsNullOrEmpty(idxattr.Fields))
                {
                    if (ret.ContainsKey(idxattr.Name)) ret.Remove(idxattr.Name);
                    ret.Add(idxattr.Name, new IndexAttribute(idxattr.Name, idxattr.Fields) { _IsUnique = idxattr._IsUnique });
                }
            }
            return ret.Values.ToArray();
        }

        public string WhereObject(TableInfo table, string aliasAndDot, object dywhere)
        {
            if (dywhere == null) return "";
            var type = dywhere.GetType();
            var primarys = table.Primarys;
            var pk1 = primarys.FirstOrDefault();
            if (primarys.Length == 1 && (type == pk1.CsType || type.IsNumberType() && pk1.CsType.IsNumberType()))
            {
                return $"{aliasAndDot}{this.QuoteSqlName(pk1.Attribute.Name)} = {RewriteColumn(pk1, GetNoneParamaterSqlValue(null, null, pk1, pk1.Attribute.MapType, Utils.GetDataReaderValue(pk1.Attribute.MapType, dywhere)))}";
            }
            else if (primarys.Length > 0 && (type == table.Type || type.BaseType == table.Type))
            {
                var sb = new StringBuilder();
                var pkidx = 0;
                foreach (var pk in primarys)
                {
                    if (pkidx > 0) sb.Append(" AND ");
                    sb.Append(aliasAndDot).Append(this.QuoteSqlName(pk.Attribute.Name)).Append(" = ");
                    sb.Append(RewriteColumn(pk, GetNoneParamaterSqlValue(null, null, pk, pk.Attribute.MapType, pk.GetDbValue(dywhere))));
                    ++pkidx;
                }
                return sb.ToString();
            }
            else if (primarys.Length == 1 && type == typeof(string))
            {
                return $"{aliasAndDot}{this.QuoteSqlName(pk1.Attribute.Name)} = {RewriteColumn(pk1, GetNoneParamaterSqlValue(null, null, pk1, pk1.Attribute.MapType, Utils.GetDataReaderValue(pk1.Attribute.MapType, dywhere)))}";
            }
            else if (primarys.Length == 1 && dywhere is IEnumerable)
            {
                var sb = new StringBuilder();
                var ie = dywhere as IEnumerable;
                var ieidx = 0;
                var isEntityType = false;
                var isAny = false;
                sb.Append(aliasAndDot).Append(this.QuoteSqlName(pk1.Attribute.Name)).Append(" IN ("); //or会造成扫全表
                foreach (var i in ie)
                {
                    isAny = true;
                    if (ieidx > 0) sb.Append(',');
                    if (ieidx == 0)
                    {
                        var itype = i.GetType();
                        isEntityType = (itype == table.Type || itype.BaseType == table.Type);
                    }
                    if (isEntityType) sb.Append(RewriteColumn(primarys[0], GetNoneParamaterSqlValue(null, null, primarys[0], primarys[0].Attribute.MapType, primarys[0].GetDbValue(i))));
                    else sb.Append(RewriteColumn(pk1, GetNoneParamaterSqlValue(null, null, pk1, pk1.Attribute.MapType, Utils.GetDataReaderValue(pk1.Attribute.MapType, i))));
                    ++ieidx;
                }
                if (isAny == false) return "";
                sb.Append(')');
                return sb.ToString();
            }
            else if (dywhere is IEnumerable)
            {
                var sb = new StringBuilder();
                var ie = dywhere as IEnumerable;
                var ieidx = 0;
                foreach (var i in ie)
                {
                    var fw = WhereObject(table, aliasAndDot, i);
                    if (string.IsNullOrEmpty(fw)) continue;
                    if (ieidx > 0) sb.Append(" OR ");
                    sb.Append(fw);
                    ++ieidx;
                }
                return sb.ToString();
            }
            else
            {
                var sb = new StringBuilder();
                var ps = type.GetPropertiesDictIgnoreCase().Values;
                var psidx = 0;
                foreach (var p in ps)
                {
                    table.Columns.TryGetValue(p.Name, out var trycol);
                    if (trycol == null) table.ColumnsByCs.TryGetValue(p.Name, out trycol);
                    if (trycol == null) continue;

                    if (psidx > 0) sb.Append(" AND ");
                    sb.Append(aliasAndDot).Append(this.QuoteSqlName(trycol.Attribute.Name)).Append(" = ");
                    sb.Append(RewriteColumn(trycol, GetNoneParamaterSqlValue(null, null, trycol, trycol.Attribute.MapType, Utils.GetDataReaderValue(trycol.Attribute.MapType, p.GetValue(dywhere, null)))));
                    ++psidx;
                }
                if (psidx == 0) return "";
                return sb.ToString();
            }
        }

        public string WhereItems<TEntity>(ColumnInfo[] primarys, string aliasAndDot, IEnumerable<TEntity> items)
        {
            if (items == null || items.Any() == false) return null;
            if (primarys.Any() == false) return null;
            var its = items.Where(a => a != null).ToArray();

            var pk1 = primarys.FirstOrDefault();
            if (primarys.Length == 1)
            {
                var sbin = new StringBuilder();
                sbin.Append(aliasAndDot).Append(this.QuoteSqlName(pk1.Attribute.Name));
                var indt = its.Select(a => pk1.GetDbValue(a)).Where(a => a != null).ToArray();
                if (indt.Any() == false) return null;
                if (indt.Length == 1) sbin.Append(" = ").Append(RewriteColumn(pk1, GetNoneParamaterSqlValue(null, null, pk1, pk1.Attribute.MapType, indt.First())));
                else sbin.Append(" IN (").Append(string.Join(",", indt.Select(a => RewriteColumn(pk1, GetNoneParamaterSqlValue(null, null, pk1, pk1.Attribute.MapType, a))))).Append(')');
                return sbin.ToString();
            }
            var dicpk = its.Length > 5 ? new Dictionary<string, bool>() : null;
            var sb = its.Length > 5 ? null : new StringBuilder();
            var iidx = 0;
            foreach (var item in its)
            {
                var filter = "";
                foreach (var pk in primarys)
                    filter += $" AND {aliasAndDot}{this.QuoteSqlName(pk.Attribute.Name)} = {RewriteColumn(pk, GetNoneParamaterSqlValue(null, null, pk, pk.Attribute.MapType, pk.GetDbValue(item)))}";
                if (string.IsNullOrEmpty(filter)) continue;
                if (sb != null)
                {
                    sb.Append(" OR (");
                    sb.Append(filter.Substring(5));
                    sb.Append(')');
                    ++iidx;
                }
                if (dicpk != null)
                {
                    filter = filter.Substring(5);
                    if (dicpk.ContainsKey(filter) == false)
                    {
                        dicpk.Add(filter, true);
                        ++iidx;
                    }
                }
                //++iidx;
            }
            if (iidx == 0) return null;
            if (sb == null)
            {
                sb = new StringBuilder();
                foreach (var fil in dicpk)
                {
                    sb.Append(" OR (");
                    sb.Append(fil.Key);
                    sb.Append(')');
                }
            }
            return iidx == 1 ? sb.Remove(0, 5).Remove(sb.Length - 1, 1).ToString() : sb.Remove(0, 4).ToString();
        }

        /// <summary>
        /// 动态读取 DescriptionAttribute 注释文本
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetPropertyCommentByDescriptionAttribute(Type type)
        {
            var dic = new Dictionary<string, string>();
            GetDydesc(null); //class注释

            var props = type.GetPropertiesDictIgnoreCase().Values;
            foreach (var prop in props)
                GetDydesc(prop);

            return dic;

            void GetDydesc(PropertyInfo prop)
            {
                object[] attrs = null;
                try
                {
                    attrs = prop == null ? 
                        type.GetCustomAttributes(false).ToArray() : 
                        prop.GetCustomAttributes(false).ToArray(); //.net core 反射存在版本冲突问题，导致该方法异常
                }
                catch { }

                var dyattr = attrs?.Where(a => {
                    return ((a as Attribute)?.TypeId as Type)?.Name == "DescriptionAttribute";
                }).FirstOrDefault();
                if (dyattr != null)
                {
                    var valueProp = dyattr.GetType().GetProperties().Where(a => a.PropertyType == typeof(string)).FirstOrDefault();
                    var comment = valueProp?.GetValue(dyattr, null)?.ToString();
                    if (string.IsNullOrEmpty(comment) == false)
                        dic.Add(prop == null ? 
                            "" : 
                            prop.Name, comment);
                }
            }
        }

        static int _CodeBaseNotSupportedException = 0;
        /// <summary>
        /// 通过属性的注释文本，通过 xml 读取
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Dict：key=属性名，value=注释</returns>
        public static Dictionary<string, string> GetProperyCommentBySummary(Type type)
        {
            return LocalGetComment(type, 0);

            Dictionary<string, string> LocalGetComment(Type localType, int level)
            {
                if (localType.Assembly.IsDynamic) return null;
                //动态生成的程序集，访问不了 Assembly.Location/Assembly.CodeBase
                var regex = new Regex(@"\.(dll|exe)", RegexOptions.IgnoreCase);
                var xmlPath = regex.Replace(localType.Assembly.Location, ".xml");
                if (File.Exists(xmlPath) == false)
                {
                    if (_CodeBaseNotSupportedException == 1) return null;
                    try
                    {
                        if (string.IsNullOrEmpty(localType.Assembly.CodeBase)) return null;
                    }
                    catch (NotSupportedException) //NotSupportedException: CodeBase is not supported on assemblies loaded from a single-file bundle.
                    {
                        Interlocked.Exchange(ref _CodeBaseNotSupportedException, 1);
                        return null;
                    }

                    xmlPath = regex.Replace(localType.Assembly.CodeBase, ".xml");
                    if (xmlPath.StartsWith("file:///") && Uri.TryCreate(xmlPath, UriKind.Absolute, out var tryuri))
                        xmlPath = tryuri.LocalPath;
                    if (File.Exists(xmlPath) == false) return null;
                }

                var dic = new Dictionary<string, string>();
                var sReader = new StringReader(File.ReadAllText(xmlPath));
                using (var xmlReader = XmlReader.Create(sReader))
                {
                    XPathDocument xpath = null;
                    try
                    {
                        xpath = new XPathDocument(xmlReader);
                    }
                    catch
                    {
                        return null;
                    }
                    var xmlNav = xpath.CreateNavigator();

                    var className = (localType.IsNested ? $"{localType.Namespace}.{localType.DeclaringType.Name}.{localType.Name}" : $"{localType.Namespace}.{localType.Name}").Trim('.');
                    var node = xmlNav.SelectSingleNode($"/doc/members/member[@name='T:{className}']/summary");
                    if (node != null)
                    {
                        var comment = node.InnerXml.Trim(' ', '\r', '\n', '\t');
                        if (string.IsNullOrEmpty(comment) == false) dic.Add("", comment); //class注释
                    }

                    var props = localType.GetPropertiesDictIgnoreCase().Values;
                    foreach (var prop in props)
                    {
                        className = (prop.DeclaringType.IsNested ? $"{prop.DeclaringType.Namespace}.{prop.DeclaringType.DeclaringType.Name}.{prop.DeclaringType.Name}" : $"{prop.DeclaringType.Namespace}.{prop.DeclaringType.Name}").Trim('.');
                        node = xmlNav.SelectSingleNode($"/doc/members/member[@name='P:{className}.{prop.Name}']/summary");
                        if (node == null)
                        {
                            if (level == 0 && prop.DeclaringType.Assembly != localType.Assembly)
                            {
                                var cbs = LocalGetComment(prop.DeclaringType, level + 1);
                                if (cbs != null && cbs.TryGetValue(prop.Name, out var otherComment) && string.IsNullOrEmpty(otherComment) == false)
                                    dic.Add(prop.Name, otherComment);
                            }
                            continue;
                        }
                        var comment = node.InnerXml.Trim(' ', '\r', '\n', '\t');
                        if (string.IsNullOrEmpty(comment)) continue;

                        dic.Add(prop.Name, comment);
                    }
                }
                return dic;
            }
        }

        public static void PrevReheatConnectionPool(ObjectPool<DbConnection> pool, int minPoolSize)
        {
            if (minPoolSize <= 0) minPoolSize = Math.Min(5, pool.Policy.PoolSize);
            if (minPoolSize > pool.Policy.PoolSize) minPoolSize = pool.Policy.PoolSize;
            var initTestOk = true;
            var initStartTime = DateTime.Now;
            var initConns = new ConcurrentBag<Object<DbConnection>>();

            try
            {
                var conn = pool.Get();
                initConns.Add(conn);
                pool.Policy.OnCheckAvailable(conn);
            }
            catch
            {
                initTestOk = false; //预热一次失败，后面将不进行
            }
            for (var a = 1; initTestOk && a < minPoolSize; a += 10)
            {
                if (initStartTime.Subtract(DateTime.Now).TotalSeconds > 3) break; //预热耗时超过3秒，退出
                var b = Math.Min(minPoolSize - a, 10); //每10个预热
                var initTasks = new Task[b];
                for (var c = 0; c < b; c++)
                {
                    initTasks[c] = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var conn = pool.Get();
                            initConns.Add(conn);
                            pool.Policy.OnCheckAvailable(conn);
                        }
                        catch
                        {
                            initTestOk = false;  //有失败，下一组退出预热
                        }
                    });
                }
                Task.WaitAll(initTasks);
            }
            while (initConns.TryTake(out var conn)) pool.Return(conn);
        }
    }
}
