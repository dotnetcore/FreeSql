using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FreeSql.PostgreSQL {

	class PostgreSQLCodeFirst : ICodeFirst {
		IFreeSql _orm;
		protected CommonUtils _commonUtils;
		protected CommonExpression _commonExpression;
		public PostgreSQLCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) {
			_orm = orm;
			_commonUtils = commonUtils;
			_commonExpression = commonExpression;
		}

		public bool IsAutoSyncStructure { get; set; } = true;

		static readonly Dictionary<string, (NpgsqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable)> _dicCsToDb = new Dictionary<string, (NpgsqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable)>() {

				{ "System.Int16",  (NpgsqlDbType.Smallint, "int2","int2 NOT NULL", false, false) },{ "System.Nullable`1[System.Int16]",  (NpgsqlDbType.Smallint, "int2", "int2", false, true) },
				{ "System.Int32",  (NpgsqlDbType.Integer, "int4","int4 NOT NULL", false, false) },{ "System.Nullable`1[System.Int32]",  (NpgsqlDbType.Integer, "int4", "int4", false, true) },
				{ "System.Int64",  (NpgsqlDbType.Bigint, "int8","int8 NOT NULL", false, false) },{ "System.Nullable`1[System.Int64]",  (NpgsqlDbType.Bigint, "int8", "int8", false, true) },

				{ "System.Single",  (NpgsqlDbType.Real, "float4","float4 NOT NULL", false, false) },{ "System.Nullable`1[System.Single]",  (NpgsqlDbType.Real, "float4", "float4", false, true) },
				{ "System.Double",  (NpgsqlDbType.Double, "float8","float8 NOT NULL", false, false) },{ "System.Nullable`1[System.Double]",  (NpgsqlDbType.Double, "float8", "float8", false, true) },
				{ "System.Decimal",  (NpgsqlDbType.Numeric, "numeric", "numeric(10,2) NOT NULL", false, false) },{ "System.Nullable`1[System.Decimal]",  (NpgsqlDbType.Numeric, "numeric", "numeric(10,2)", false, true) },

				{ "System.String",  (NpgsqlDbType.Varchar, "varchar", "varchar(255)", false, null) },

				{ "System.TimeSpan",  (NpgsqlDbType.Time, "time","time NOT NULL", false, false) },{ "System.Nullable`1[System.TimeSpan]",  (NpgsqlDbType.Time, "time", "time",false, true) },
				{ "System.DateTime",  (NpgsqlDbType.Timestamp, "timestamp", "timestamp NOT NULL", false, false) },{ "System.Nullable`1[System.DateTime]",  (NpgsqlDbType.Timestamp, "timestamp", "timestamp", false, true) },

				{ "System.Boolean",  (NpgsqlDbType.Boolean, "bool","bool NOT NULL", null, false) },{ "System.Nullable`1[System.Boolean]",  (NpgsqlDbType.Bit, "bool","bool", null, true) },
				{ "System.Byte[]",  (NpgsqlDbType.Bytea, "bytea", "bytea", false, null) },
				{ "System.BitArray",  (NpgsqlDbType.Varbit, "varbit", "varbit(255)", false, null) },

				{ "NpgsqlTypes.NpgsqlPoint",  (NpgsqlDbType.Point, "point", "point", false, false) },{ "System.Nullable`1[NpgsqlTypes.NpgsqlPoint]",  (NpgsqlDbType.Point, "point", "point", false, true) },
				{ "NpgsqlTypes.NpgsqlLine",  (NpgsqlDbType.Line, "line", "line", false, false) },{ "System.Nullable`1[NpgsqlTypes.NpgsqlLine]",  (NpgsqlDbType.Line, "line", "line", false, true) },
				{ "NpgsqlTypes.NpgsqlLSeg",  (NpgsqlDbType.LSeg, "lseg", "lseg", false, false) },{ "System.Nullable`1[NpgsqlTypes.NpgsqlLSeg]",  (NpgsqlDbType.LSeg, "lseg", "lseg", false, true) },
				{ "NpgsqlTypes.NpgsqlBox",  (NpgsqlDbType.Box, "box", "box", false, false) },{ "System.Nullable`1[NpgsqlTypes.NpgsqlBox]",  (NpgsqlDbType.Box, "box", "box", false, true) },
				{ "NpgsqlTypes.NpgsqlPath",  (NpgsqlDbType.Path, "path", "path", false, false) },{ "System.Nullable`1[NpgsqlTypes.NpgsqlPath]",  (NpgsqlDbType.Path, "path", "path", false, true) },
				{ "NpgsqlTypes.NpgsqlPolygon",  (NpgsqlDbType.Polygon, "polygon", "polygon", false, false) },{ "System.Nullable`1[NpgsqlTypes.NpgsqlPolygon]",  (NpgsqlDbType.Polygon, "polygon", "polygon", false, true) },
				{ "NpgsqlTypes.NpgsqlCircle",  (NpgsqlDbType.Circle, "circle", "circle", false, false) },{ "System.Nullable`1[NpgsqlTypes.NpgsqlCircle]",  (NpgsqlDbType.Circle, "circle", "circle", false, true) },

				{ "System.ValueTuple`2[[System.Net.IPAddress, System.Int32]]",  (NpgsqlDbType.Cidr, "cidr", "cidr", false, false) },{ "System.Nullable`1[System.ValueTuple`2[[System.Net.IPAddress, System.Int32]]]",  (NpgsqlDbType.Cidr, "cidr", "cidr", false, true) },
				{ "System.Net.IPAddress",  (NpgsqlDbType.Inet, "inet", "inet", false, null) },
				{ "System.Net.NetworkInformation.PhysicalAddress",  (NpgsqlDbType.MacAddr, "macaddr", "macaddr", false, null) },

				{ "Newtonsoft.Json.Linq.JToken",  (NpgsqlDbType.Jsonb, "jsonb", "jsonb", false, null) },
				{ "Newtonsoft.Json.Linq.JObject",  (NpgsqlDbType.Jsonb, "jsonb", "jsonb", false, null) },
				{ "Newtonsoft.Json.Linq.JArray",  (NpgsqlDbType.Jsonb, "jsonb", "jsonb", false, null) },
				{ "System.Guid",  (NpgsqlDbType.Uuid, "uuid", "uuid", false, false) },{ "System.Nullable`1[System.Guid]",  (NpgsqlDbType.Uuid, "uuid", "uuid", false, true) },

				{ "NpgsqlTypes.NpgsqlRange<int>",  (NpgsqlDbType.Range | NpgsqlDbType.Integer, "int4range", "int4range", false, false) },{ "System.Nullable`1[NpgsqlTypes.NpgsqlRange<int>]",  (NpgsqlDbType.Range | NpgsqlDbType.Integer, "int4range", "int4range", false, true) },
				{ "NpgsqlTypes.NpgsqlRange<long>",  (NpgsqlDbType.Range | NpgsqlDbType.Bigint, "int8range", "int8range", false, false) },{ "System.Nullable`1[NpgsqlTypes.NpgsqlRange<long>]",  (NpgsqlDbType.Range | NpgsqlDbType.Bigint, "int8range", "int8range", false, true) },
				{ "NpgsqlTypes.NpgsqlRange<decimal>",  (NpgsqlDbType.Range | NpgsqlDbType.Numeric, "numrange", "numrange", false, false) },{ "System.Nullable`1[NpgsqlTypes.NpgsqlRange<decimal>]",  (NpgsqlDbType.Range | NpgsqlDbType.Numeric, "numrange", "numrange", false, true) },
				{ "NpgsqlTypes.NpgsqlRange<DateTime>",  (NpgsqlDbType.Range | NpgsqlDbType.Timestamp, "tsrange", "tsrange", false, false) },{ "System.Nullable`1[NpgsqlTypes.NpgsqlRange<DateTime>]",  (NpgsqlDbType.Range | NpgsqlDbType.Timestamp, "tsrange", "tsrange", false, true) },

				{ "Dictionary<string, string>",  (NpgsqlDbType.Hstore, "hstore", "hstore", false, null) },
				{ "Npgsql.LegacyPostgis.PostgisGeometry",  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null) },
			};

		public (int type, string dbtype, string dbtypeFull, bool? isnullable)? GetDbInfo(Type type) {
			var enumType = type.IsEnum ? type : null;
			if (enumType == null && type.FullName.StartsWith("System.Nullable`1[") && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments.First().IsEnum) enumType = type.GenericTypeArguments.First();
			if (enumType != null) {
				return ((int)NpgsqlDbType.Integer, "int4", $"int4{(type.IsEnum ? " NOT NULL" : "")}", type.IsEnum ? false : true);
			}
			return _dicCsToDb.TryGetValue(type.FullName, out var trydc) ? new (int, string, string, bool?)?(((int)trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable)) : null;
		}

		public string GetComparisonDDLStatements<TEntity>() => this.GetComparisonDDLStatements(typeof(TEntity));
		public string GetComparisonDDLStatements(params Type[] entityTypes) {
			var sb = new StringBuilder();
			foreach (var entityType in entityTypes) {
				if (sb.Length > 0) sb.Append("\r\n");
				var tb = _commonUtils.GetTableByEntity(entityType);
				var tboldname = tb.DbOldName?.Split(new[] { '.' }, 2); //旧表名
				if (tboldname?.Length == 1) tboldname = new[] { "public", tboldname[0] };

				var isRenameTable = false;
				var tbname = tb.DbName.Split(new[] { '.' }, 2);
				if (tbname.Length == 1) tbname = new[] { "public", tbname[0] };
				if (_orm.Ado.ExecuteScalar(CommandType.Text, "select 1 from pg_tables a inner join pg_namespace b on b.nspname = a.schemaname where b.nspname || '.' || a.tablename = {0}.{1}".FormatMySql(tbname)) == null) { //表不存在

					if (tboldname != null && _orm.Ado.ExecuteScalar(CommandType.Text, "select 1 from pg_tables a inner join pg_namespace b on b.nspname = a.schemaname where b.nspname || '.' || a.tablename = {0}.{1}".FormatMySql(tboldname)) != null) { //旧表存在
																																																															//修改表名
						sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tboldname[0]}.{tboldname[1]}")).Append(" RENAME TO ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(";\r\n");
						isRenameTable = true;

					} else {
						//创建表
						var seqcols = new List<ColumnInfo>();
						sb.Append("CREATE TABLE IF NOT EXISTS ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" (");
						foreach (var tbcol in tb.Columns.Values) {
							sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ");
							sb.Append(tbcol.Attribute.DbType.ToUpper());
							if (tbcol.Attribute.IsIdentity && tbcol.Attribute.DbType.IndexOf("serial", StringComparison.CurrentCultureIgnoreCase) == -1) seqcols.Add(tbcol);
							sb.Append(",");
						}
						if (tb.Primarys.Any() == false)
							sb.Remove(sb.Length - 1, 1);
						else {
							sb.Append(" \r\n  CONSTRAINT ").Append(tbname[0]).Append("_").Append(tbname[1]).Append("_pkey PRIMARY KEY (");
							foreach (var tbcol in tb.Primarys) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
							sb.Remove(sb.Length - 2, 2).Append(")");
						}
						sb.Append("\r\n) WITH (OIDS=FALSE);\r\n");
						continue;
					}
				}
				//对比字段，只可以修改类型、增加字段、有限的修改字段名；保证安全不删除字段
				var addcols = new Dictionary<string, ColumnInfo>(StringComparer.CurrentCultureIgnoreCase);
				foreach (var tbcol in tb.Columns) addcols.Add(tbcol.Value.Attribute.Name, tbcol.Value);
				var surplus = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
				var dbcols = new List<DbColumnInfo>();
				var sql = @"select
a.attname,
t.typname,
case when a.atttypmod > 0 and a.atttypmod < 32767 then a.atttypmod - 4 else a.attlen end len,
case when t.typelem = 0 then t.typname else t2.typname end,
case when a.attnotnull then 0 else 1 end as is_nullable,
e.adsrc as is_identity
from pg_class c
inner join pg_attribute a on a.attnum > 0 and a.attrelid = c.oid
inner join pg_type t on t.oid = a.atttypid
left join pg_type t2 on t2.oid = t.typelem
left join pg_description d on d.objoid = a.attrelid and d.objsubid = a.attnum
left join pg_attrdef e on e.adrelid = a.attrelid and e.adnum = a.attnum
inner join pg_namespace ns on ns.oid = c.relnamespace
inner join pg_namespace ns2 on ns2.oid = t.typnamespace
where ns.nspname = {0} and c.relname = {1}".FormatMySql(isRenameTable ? tboldname : tbname);
				var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
				foreach (var row in ds) {
					string column = string.Concat(row[0]);
					string sqlType = string.Concat(row[3]);
					long max_length = long.Parse(string.Concat(row[2]));
					bool is_nullable = string.Concat(row[4]) == "1";
					bool is_identity = string.Concat(row[5]).StartsWith(@"nextval('") && string.Concat(row[6]).EndsWith(@"_seq'::regclass)");

					if (addcols.TryGetValue(column, out var trycol)) {
						if (trycol.Attribute.DbType.ToLower().StartsWith(sqlType.ToLower()) == false ||
							(trycol.Attribute.DbType.IndexOf("NOT NULL") == -1) != is_nullable ||
							trycol.Attribute.IsIdentity != is_identity) {
							sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ALTER COLUMN ").Append(_commonUtils.QuoteSqlName(column)).Append(" TYPE ").Append(trycol.Attribute.DbType.ToUpper());
							if (trycol.Attribute.IsIdentity) sb.Append(" AUTO_INCREMENT");
							sb.Append(";\r\n");
						}
						addcols.Remove(column);
					} else
						surplus.Add(column, true); //记录剩余字段
				}
				foreach (var addcol in addcols.Values) {
					if (string.IsNullOrEmpty(addcol.Attribute.OldName) == false && surplus.ContainsKey(addcol.Attribute.OldName)) { //修改列名
						sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" RENAME COLUMN ").Append(_commonUtils.QuoteSqlName(addcol.Attribute.OldName)).Append(" TO ").Append(_commonUtils.QuoteSqlName(addcol.Attribute.Name)).Append(";\r\n");
						if (addcol.Attribute.IsIdentity) sb.Append(" AUTO_INCREMENT");
						sb.Append(";\r\n");

					} else { //添加列
						sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ADD COLUMN ").Append(_commonUtils.QuoteSqlName(addcol.Attribute.Name)).Append(" ").Append(addcol.Attribute.DbType.ToUpper());
						if (addcol.Attribute.IsIdentity) sb.Append(" AUTO_INCREMENT");
						sb.Append(";\r\n");
					}
				}
			}
			return sb.Length == 0 ? null : sb.ToString();
		}

		public bool SyncStructure<TEntity>() => this.SyncStructure(typeof(TEntity));
		public bool SyncStructure(params Type[] entityTypes) {
			var ddl = this.GetComparisonDDLStatements(entityTypes);
			if (string.IsNullOrEmpty(ddl)) return true;
			try {
				return _orm.Ado.ExecuteNonQuery(CommandType.Text, ddl) > 0;
			} catch {
				return false;
			}
		}

	}
}