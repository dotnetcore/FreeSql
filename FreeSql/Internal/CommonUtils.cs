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
		internal abstract string QuoteParamterName(string name);
		internal abstract string IsNull(string sql, object value);
		internal abstract string StringConcat(string left, string right, Type leftType, Type rightType);
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
			var attr = type.GetCustomAttributes(typeof(TableAttribute), false).LastOrDefault() as TableAttribute;
			if (dicConfigEntity.TryGetValue(type, out var trytb) == false) return attr;
			if (attr == null) attr = new TableAttribute();

			if (string.IsNullOrEmpty(attr.Name)) attr.Name = trytb.Name;
			if (string.IsNullOrEmpty(attr.OldName)) attr.OldName = trytb.OldName;
			if (string.IsNullOrEmpty(attr.SelectFilter)) attr.SelectFilter = trytb.SelectFilter;
			return attr;
		}
		internal ColumnAttribute GetEntityColumnAttribute(Type type, PropertyInfo proto) {
			var attr = proto.GetCustomAttributes(typeof(ColumnAttribute), false).LastOrDefault() as ColumnAttribute;
			if (dicConfigEntity.TryGetValue(type, out var trytb) == false) return attr;
			if (trytb._columns.TryGetValue(proto.Name, out var trycol) == false) return attr;
			if (attr == null) attr = new ColumnAttribute();

			if (string.IsNullOrEmpty(attr.Name)) attr.Name = trycol.Name;
			if (string.IsNullOrEmpty(attr.OldName)) attr.OldName = trycol.OldName;
			if (string.IsNullOrEmpty(attr.DbType)) attr.DbType = trycol.DbType;
			if (attr._IsPrimary == null) attr._IsPrimary = trycol.IsPrimary;
			if (attr._IsIdentity == null) attr._IsIdentity = trycol.IsIdentity;
			if (attr._IsNullable == null) attr._IsNullable = trycol.IsNullable;
			if (attr._IsIgnore == null) attr._IsIgnore = trycol.IsIgnore;
			if (attr._IsVersion == null) attr._IsVersion = trycol.IsVersion;
			if (attr.DbDefautValue == null) attr.DbDefautValue = trycol.DbDefautValue;
			return attr;
		}

		internal string WhereObject(TableInfo table, string aliasAndDot, object dywhere) {
			if (dywhere == null) return "";
			var type = dywhere.GetType();
			var primarys = table.Columns.Values.Where(a => a.Attribute.IsPrimary == true).ToArray();
			if (primarys.Length == 1 && (type == primarys.First().CsType || type.IsNumberType() && primarys.First().CsType.IsNumberType())) {
				return $"{aliasAndDot}{this.QuoteSqlName(primarys.First().Attribute.Name)} = {this.FormatSql("{0}", dywhere)}";
			} else if (primarys.Length > 0 && (type == table.Type || type.BaseType == table.Type)) {
				var sb = new StringBuilder();
				var pkidx = 0;
				foreach (var pk in primarys) {
					var prop = type.GetProperty(pk.CsName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
					if (pkidx > 0) sb.Append(" AND ");
					sb.Append(aliasAndDot).Append(this.QuoteSqlName(pk.Attribute.Name));
					sb.Append(this.FormatSql(" = {0}", prop.GetValue(dywhere)));
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
					sb.Append(this.FormatSql(" = {0}", p.GetValue(dywhere)));
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

			if (table.Primarys.Length == 1) {
				var sbin = new StringBuilder();
				sbin.Append(aliasAndDot).Append(this.QuoteSqlName(table.Primarys.First().Attribute.Name));
				var indt = its.Select(a => /*this.FormatSql("{0}", _orm.GetEntityKeyValues(a))*/
					table.Properties.TryGetValue(table.Primarys.First().CsName, out var trycol) ? this.FormatSql("{0}", trycol.GetValue(a)) : null).Where(a => a != null).ToArray();
				if (indt.Any() == false) return null;
				if (indt.Length == 1) sbin.Append(" = ").Append(indt.First());
				else sbin.Append(" IN (").Append(string.Join(",", indt)).Append(")");
				return sbin.ToString();
			}
			var dicpk = its.Length > 5 ? new Dictionary<string, bool>() : null;
			var sb = its.Length > 5 ? null : new StringBuilder();
			var iidx = 0;
			foreach (var item in its) {
				var filter = "";
				for (var a = 0; a < table.Primarys.Length; a++) {
					if (table.Properties.TryGetValue(table.Primarys[a].CsName, out var trycol) == false) continue;
					filter += $" AND {aliasAndDot}{this.QuoteSqlName(table.Primarys[a].Attribute.Name)} = {this.FormatSql("{0}", trycol.GetValue(item))}";
				}
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
