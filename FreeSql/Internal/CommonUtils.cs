using FreeSql.DataAnnotations;
using FreeSql.DatabaseModel;
using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FreeSql.Internal {
	internal abstract class CommonUtils {

		internal abstract string GetNoneParamaterSqlValue(List<DbParameter> specialParams, Type type, object value);
		internal abstract DbParameter AppendParamter(List<DbParameter> _params, string parameterName, Type type, object value);
		internal abstract DbParameter[] GetDbParamtersByObject(string sql, object obj);
		internal abstract string FormatSql(string sql, params object[] args);
		internal abstract string QuoteSqlName(string name);
		internal abstract string TrimQuoteSqlName(string name);
		internal abstract string QuoteParamterName(string name);
		internal abstract string IsNull(string sql, object value);
		internal abstract string StringConcat(string[] objs, Type[] types);
		internal abstract string Mod(string left, string right, Type leftType, Type rightType);
		internal abstract string QuoteWriteParamter(Type type, string paramterName);
		internal abstract string QuoteReadColumn(Type type, string columnName);

		internal IFreeSql _orm { get; set; }
		internal ICodeFirst CodeFirst => _orm.CodeFirst;
		internal TableInfo GetTableByEntity(Type entity) => Utils.GetTableByEntity(entity, this);
		internal List<DbTableInfo> dbTables { get; set; }
		internal object dbTablesLock = new object();

		public CommonUtils(IFreeSql orm) {
			_orm = orm;
		}

		ConcurrentDictionary<Type, TableAttribute> dicConfigEntity = new ConcurrentDictionary<Type, TableAttribute>();
		internal ICodeFirst ConfigEntity<T>(Action<TableFluent<T>> entity) {
			if (entity == null) return _orm.CodeFirst;
			var type = typeof(T);
			var table = dicConfigEntity.GetOrAdd(type, new TableAttribute());
			var fluent = new TableFluent<T>(table);
			entity.Invoke(fluent);
			Utils.RemoveTableByEntity(type, this); //remove cache
			return _orm.CodeFirst;
		}
		internal ICodeFirst ConfigEntity(Type type, Action<TableFluent> entity) {
			if (entity == null) return _orm.CodeFirst;
			var table = dicConfigEntity.GetOrAdd(type, new TableAttribute());
			var fluent = new TableFluent(type, table);
			entity.Invoke(fluent);
			Utils.RemoveTableByEntity(type, this); //remove cache
			return _orm.CodeFirst;
		}
		internal TableAttribute GetConfigEntity(Type type) {
			return dicConfigEntity.TryGetValue(type, out var trytb) ? trytb : null;
		}
		internal TableAttribute GetEntityTableAttribute(Type type) {
			TableAttribute attr = null;
			if (_orm.Aop.ConfigEntity != null) {
				var aope = new Aop.ConfigEntityEventArgs(type);
				_orm.Aop.ConfigEntity(_orm, aope);
				attr = aope.ModifyResult;
			}
			if (attr == null) attr = new TableAttribute();
			if (dicConfigEntity.TryGetValue(type, out var trytb)) {
				if (!string.IsNullOrEmpty(trytb.Name)) attr.Name = trytb.Name;
				if (!string.IsNullOrEmpty(trytb.OldName)) attr.OldName = trytb.OldName;
				if (!string.IsNullOrEmpty(trytb.SelectFilter)) attr.SelectFilter = trytb.SelectFilter;
			}
			var attrs = type.GetCustomAttributes(typeof(TableAttribute), false);
			foreach (var tryattrobj in attrs) {
				var tryattr = tryattrobj as TableAttribute;
				if (tryattr == null) continue;
				if (!string.IsNullOrEmpty(tryattr.Name)) attr.Name = tryattr.Name;
				if (!string.IsNullOrEmpty(tryattr.OldName)) attr.OldName = tryattr.OldName;
				if (!string.IsNullOrEmpty(tryattr.SelectFilter)) attr.SelectFilter = tryattr.SelectFilter;
			}
			if (!string.IsNullOrEmpty(attr.Name)) return attr;
			if (!string.IsNullOrEmpty(attr.OldName)) return attr;
			if (!string.IsNullOrEmpty(attr.SelectFilter)) return attr;
			return null;
		}
		internal ColumnAttribute GetEntityColumnAttribute(Type type, PropertyInfo proto) {
			ColumnAttribute attr = null;
			if (_orm.Aop.ConfigEntityProperty != null) {
				var aope = new Aop.ConfigEntityPropertyEventArgs(type, proto);
				_orm.Aop.ConfigEntityProperty(_orm, aope);
				attr = aope.ModifyResult;
			}
			if (attr == null) attr = new ColumnAttribute();
			if (dicConfigEntity.TryGetValue(type, out var trytb) && trytb._columns.TryGetValue(proto.Name, out var trycol)) {
				if (!string.IsNullOrEmpty(trycol.Name)) attr.Name = trycol.Name;
				if (!string.IsNullOrEmpty(trycol.OldName)) attr.OldName = trycol.OldName;
				if (!string.IsNullOrEmpty(trycol.DbType)) attr.DbType = trycol.DbType;
				if (trycol._IsPrimary != null) attr._IsPrimary = trycol.IsPrimary;
				if (trycol._IsIdentity != null) attr._IsIdentity = trycol.IsIdentity;
				if (trycol._IsNullable != null) attr._IsNullable = trycol.IsNullable;
				if (trycol._IsIgnore != null) attr._IsIgnore = trycol.IsIgnore;
				if (trycol._IsVersion != null) attr._IsVersion = trycol.IsVersion;
				if (trycol._Uniques != null) attr._Uniques = trycol._Uniques;
				if (trycol.MapType != null) attr.MapType = trycol.MapType;
				if (trycol.DbDefautValue != null) attr.DbDefautValue = trycol.DbDefautValue;
			}
			var attrs = proto.GetCustomAttributes(typeof(ColumnAttribute), false);
			foreach (var tryattrobj in attrs) {
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
				if (tryattr._Uniques != null) attr._Uniques = tryattr._Uniques;
				if (tryattr.MapType != null) attr.MapType = tryattr.MapType;
				if (tryattr.DbDefautValue != null) attr.DbDefautValue = tryattr.DbDefautValue;
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
			if (attr._Uniques != null) ret = attr;
			if (attr.MapType != null) ret = attr;
			if (attr.DbDefautValue != null) ret = attr;
			if (ret != null && ret.MapType == null) ret.MapType = proto.PropertyType;
			return ret;
		}

		internal string WhereObject(TableInfo table, string aliasAndDot, object dywhere) {
			if (dywhere == null) return "";
			var type = dywhere.GetType();
			var primarys = table.Primarys;
			var pk1 = primarys.FirstOrDefault();
			if (primarys.Length == 1 && (type == pk1.CsType || type.IsNumberType() && pk1.CsType.IsNumberType())) {
				return $"{aliasAndDot}{this.QuoteSqlName(pk1.Attribute.Name)} = {this.FormatSql("{0}", Utils.GetDataReaderValue(pk1.Attribute.MapType, dywhere))}";
			} else if (primarys.Length > 0 && (type == table.Type || type.BaseType == table.Type)) {
				var sb = new StringBuilder();
				var pkidx = 0;
				foreach (var pk in primarys) {
					if (pkidx > 0) sb.Append(" AND ");
					sb.Append(aliasAndDot).Append(this.QuoteSqlName(pk.Attribute.Name));
					sb.Append(this.FormatSql(" = {0}", pk.GetMapValue(dywhere)));
					++pkidx;
				}
				return sb.ToString();
			} else if (dywhere is IEnumerable) {
				var sb = new StringBuilder();
				var ie = dywhere as IEnumerable;
				var ieidx = 0;
				foreach (var i in ie) {
					var fw = WhereObject(table, aliasAndDot, i);
					if (string.IsNullOrEmpty(fw)) continue;
					if (ieidx > 0) sb.Append(" OR ");
					sb.Append(fw);
					++ieidx;
				}
				return sb.ToString();
			} else {
				var sb = new StringBuilder();
				var ps = type.GetProperties();
				var psidx = 0;
				foreach (var p in ps) {
					if (table.Columns.TryGetValue(p.Name, out var trycol) == false) continue;
					if (psidx > 0) sb.Append(" AND ");
					sb.Append(aliasAndDot).Append(this.QuoteSqlName(trycol.Attribute.Name));
					sb.Append(this.FormatSql(" = {0}", Utils.GetDataReaderValue(trycol.Attribute.MapType, p.GetValue(dywhere))));
					++psidx;
				}
				if (psidx == 0) return "";
				return sb.ToString();
			}
		}

		internal string WhereItems<TEntity>(TableInfo table, string aliasAndDot, IEnumerable<TEntity> items) {
			if (items == null || items.Any() == false) return null;
			if (table.Primarys.Any() == false) return null;
			var its = items.Where(a => a != null).ToArray();

			var pk1 = table.Primarys.FirstOrDefault();
			if (table.Primarys.Length == 1) {
				var sbin = new StringBuilder();
				sbin.Append(aliasAndDot).Append(this.QuoteSqlName(pk1.Attribute.Name));
				var indt = its.Select(a => pk1.GetMapValue(a)).Where(a => a != null).ToArray();
				if (indt.Any() == false) return null;
				if (indt.Length == 1) sbin.Append(" = ").Append(this.FormatSql("{0}", indt.First()));
				else sbin.Append(" IN (").Append(string.Join(",", indt.Select(a => this.FormatSql("{0}", a)))).Append(")");
				return sbin.ToString();
			}
			var dicpk = its.Length > 5 ? new Dictionary<string, bool>() : null;
			var sb = its.Length > 5 ? null : new StringBuilder();
			var iidx = 0;
			foreach (var item in its) {
				var filter = "";
				foreach (var pk in table.Primarys)
					filter += $" AND {aliasAndDot}{this.QuoteSqlName(pk.Attribute.Name)} = {this.FormatSql("{0}", pk.GetMapValue(item))}";
				if (string.IsNullOrEmpty(filter)) continue;
				if (sb != null) {
					sb.Append(" OR (");
					sb.Append(filter.Substring(5));
					sb.Append(")");
					++iidx;
				}
				if (dicpk != null) {
					filter = filter.Substring(5);
					if (dicpk.ContainsKey(filter) == false) {
						dicpk.Add(filter, true);
						++iidx;
					}
				}
				//++iidx;
			}
			if (iidx == 0) return null;
			if (sb == null) {
				sb = new StringBuilder();
				foreach (var fil in dicpk) {
					sb.Append(" OR (");
					sb.Append(fil.Key);
					sb.Append(")");
				}
			}
			return iidx == 1 ? sb.Remove(0, 5).Remove(sb.Length - 1, 1).ToString() : sb.Remove(0, 4).ToString();
		}
	}
}
