using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using Newtonsoft.Json.Linq;
using Npgsql.LegacyPostgis;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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

		static object _dicCsToDbLock = new object();
		static Dictionary<string, (NpgsqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable)> _dicCsToDb = new Dictionary<string, (NpgsqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable)>() {

				{ typeof(sbyte).FullName,  (NpgsqlDbType.Smallint, "int2","int2 NOT NULL", false, false) },{ typeof(sbyte?).FullName,  (NpgsqlDbType.Smallint, "int2", "int2", false, true) },
				{ typeof(short).FullName,  (NpgsqlDbType.Smallint, "int2","int2 NOT NULL", false, false) },{ typeof(short?).FullName,  (NpgsqlDbType.Smallint, "int2", "int2", false, true) },
				{ typeof(int).FullName,  (NpgsqlDbType.Integer, "int4","int4 NOT NULL", false, false) },{ typeof(int?).FullName,  (NpgsqlDbType.Integer, "int4", "int4", false, true) },
				{ typeof(long).FullName,  (NpgsqlDbType.Bigint, "int8","int8 NOT NULL", false, false) },{ typeof(long?).FullName,  (NpgsqlDbType.Bigint, "int8", "int8", false, true) },

				{ typeof(byte).FullName,  (NpgsqlDbType.Smallint, "int2","int2 NOT NULL", false, false) },{ typeof(byte?).FullName,  (NpgsqlDbType.Smallint, "int2", "int2", false, true) },
				{ typeof(ushort).FullName,  (NpgsqlDbType.Smallint, "int2","int2 NOT NULL", false, false) },{ typeof(ushort?).FullName,  (NpgsqlDbType.Smallint, "int2", "int2", false, true) },
				{ typeof(uint).FullName,  (NpgsqlDbType.Integer, "int4","int4 NOT NULL", false, false) },{ typeof(uint?).FullName,  (NpgsqlDbType.Integer, "int4", "int4", false, true) },
				{ typeof(ulong).FullName,  (NpgsqlDbType.Bigint, "int8","int8 NOT NULL", false, false) },{ typeof(ulong?).FullName,  (NpgsqlDbType.Bigint, "int8", "int8", false, true) },

				{ typeof(float).FullName,  (NpgsqlDbType.Real, "float4","float4 NOT NULL", false, false) },{ typeof(float?).FullName,  (NpgsqlDbType.Real, "float4", "float4", false, true) },
				{ typeof(double).FullName,  (NpgsqlDbType.Double, "float8","float8 NOT NULL", false, false) },{ typeof(double?).FullName,  (NpgsqlDbType.Double, "float8", "float8", false, true) },
				{ typeof(decimal).FullName,  (NpgsqlDbType.Numeric, "numeric", "numeric(10,2) NOT NULL", false, false) },{ typeof(decimal?).FullName,  (NpgsqlDbType.Numeric, "numeric", "numeric(10,2)", false, true) },

				{ typeof(string).FullName,  (NpgsqlDbType.Varchar, "varchar", "varchar(255)", false, null) },

				{ typeof(TimeSpan).FullName,  (NpgsqlDbType.Time, "time","time NOT NULL", false, false) },{ typeof(TimeSpan?).FullName,  (NpgsqlDbType.Time, "time", "time",false, true) },
				{ typeof(DateTime).FullName,  (NpgsqlDbType.Timestamp, "timestamp", "timestamp NOT NULL", false, false) },{ typeof(DateTime?).FullName,  (NpgsqlDbType.Timestamp, "timestamp", "timestamp", false, true) },

				{ typeof(bool).FullName,  (NpgsqlDbType.Boolean, "bool","bool NOT NULL", null, false) },{ typeof(bool?).FullName,  (NpgsqlDbType.Bit, "bool","bool", null, true) },
				{ typeof(Byte[]).FullName,  (NpgsqlDbType.Bytea, "bytea", "bytea", false, null) },
				{ typeof(BitArray).FullName,  (NpgsqlDbType.Varbit, "varbit", "varbit(64)", false, null) },

				{ typeof(NpgsqlPoint).FullName,  (NpgsqlDbType.Point, "point", "point NOT NULL", false, false) },{ typeof(NpgsqlPoint?).FullName,  (NpgsqlDbType.Point, "point", "point", false, true) },
				{ typeof(NpgsqlLine).FullName,  (NpgsqlDbType.Line, "line", "line NOT NULL", false, false) },{ typeof(NpgsqlLine?).FullName,  (NpgsqlDbType.Line, "line", "line", false, true) },
				{ typeof(NpgsqlLSeg).FullName,  (NpgsqlDbType.LSeg, "lseg", "lseg NOT NULL", false, false) },{ typeof(NpgsqlLSeg?).FullName,  (NpgsqlDbType.LSeg, "lseg", "lseg", false, true) },
				{ typeof(NpgsqlBox).FullName,  (NpgsqlDbType.Box, "box", "box NOT NULL", false, false) },{ typeof(NpgsqlBox?).FullName,  (NpgsqlDbType.Box, "box", "box", false, true) },
				{ typeof(NpgsqlPath).FullName,  (NpgsqlDbType.Path, "path", "path NOT NULL", false, false) },{ typeof(NpgsqlPath?).FullName,  (NpgsqlDbType.Path, "path", "path", false, true) },
				{ typeof(NpgsqlPolygon).FullName,  (NpgsqlDbType.Polygon, "polygon", "polygon NOT NULL", false, false) },{ typeof(NpgsqlPolygon?).FullName,  (NpgsqlDbType.Polygon, "polygon", "polygon", false, true) },
				{ typeof(NpgsqlCircle).FullName,  (NpgsqlDbType.Circle, "circle", "circle NOT NULL", false, false) },{ typeof(NpgsqlCircle?).FullName,  (NpgsqlDbType.Circle, "circle", "circle", false, true) },

				{ typeof((IPAddress Address, int Subnet)).FullName,  (NpgsqlDbType.Cidr, "cidr", "cidr NOT NULL", false, false) },{ typeof((IPAddress Address, int Subnet)?).FullName,  (NpgsqlDbType.Cidr, "cidr", "cidr", false, true) },
				{ typeof(IPAddress).FullName,  (NpgsqlDbType.Inet, "inet", "inet", false, null) },
				{ typeof(PhysicalAddress).FullName,  (NpgsqlDbType.MacAddr, "macaddr", "macaddr", false, null) },

				{ typeof(JToken).FullName,  (NpgsqlDbType.Jsonb, "jsonb", "jsonb", false, null) },
				{ typeof(JObject).FullName,  (NpgsqlDbType.Jsonb, "jsonb", "jsonb", false, null) },
				{ typeof(JArray).FullName,  (NpgsqlDbType.Jsonb, "jsonb", "jsonb", false, null) },
				{ typeof(Guid).FullName,  (NpgsqlDbType.Uuid, "uuid", "uuid NOT NULL", false, false) },{ typeof(Guid?).FullName,  (NpgsqlDbType.Uuid, "uuid", "uuid", false, true) },

				{ typeof(NpgsqlRange<int>).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Integer, "int4range", "int4range NOT NULL", false, false) },{ typeof(NpgsqlRange<int>?).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Integer, "int4range", "int4range", false, true) },
				{ typeof(NpgsqlRange<long>).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Bigint, "int8range", "int8range NOT NULL", false, false) },{ typeof(NpgsqlRange<long>?).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Bigint, "int8range", "int8range", false, true) },
				{ typeof(NpgsqlRange<decimal>).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Numeric, "numrange", "numrange NOT NULL", false, false) },{ typeof(NpgsqlRange<decimal>?).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Numeric, "numrange", "numrange", false, true) },
				{ typeof(NpgsqlRange<DateTime>).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Timestamp, "tsrange", "tsrange NOT NULL", false, false) },{ typeof(NpgsqlRange<DateTime>?).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Timestamp, "tsrange", "tsrange", false, true) },

				{ typeof(Dictionary<string, string>).FullName,  (NpgsqlDbType.Hstore, "hstore", "hstore", false, null) },
				{ typeof(PostgisPoint).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null) },
				{ typeof(PostgisLineString).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null) },
				{ typeof(PostgisPolygon).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null) },
				{ typeof(PostgisMultiPoint).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null) },
				{ typeof(PostgisMultiLineString).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null) },
				{ typeof(PostgisMultiPolygon).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null) },
				{ typeof(PostgisGeometry).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null) },
				{ typeof(PostgisGeometryCollection).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null) },
			};

		public (int type, string dbtype, string dbtypeFull, bool? isnullable)? GetDbInfo(Type type) {
			var elementType = type.IsArray ? type.GetElementType() : type;
			var info = GetDbInfoNoneArray(elementType);
			if (info == null) return null;
			if (type.IsArray == false) return ((int)info.Value.type, info.Value.dbtype, info.Value.dbtypeFull, info.Value.isnullable);
			var dbype = $"{info.Value.dbtype}[]";
			return ((int)(info.Value.type | NpgsqlDbType.Array), dbype, info.Value.dbtypeFull.Replace(info.Value.dbtype, dbype), info.Value.isnullable);
		}
		(NpgsqlDbType type, string dbtype, string dbtypeFull, bool? isnullable)? GetDbInfoNoneArray(Type type) {
			if (_dicCsToDb.TryGetValue(type.FullName, out var trydc)) return new (NpgsqlDbType, string, string, bool?)?((trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable));
			var enumType = type.IsEnum ? type : null;
			if (enumType == null && type.FullName.StartsWith("System.Nullable`1[") && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments.First().IsEnum) enumType = type.GenericTypeArguments.First();
			if (enumType != null) {
				var newItem = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any() ?
					(NpgsqlDbType.Varchar, "varchar", $"varchar(32){(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true) :
					(NpgsqlDbType.Varchar, "varchar", $"varchar(32){(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true);
				if (_dicCsToDb.ContainsKey(type.FullName) == false) {
					lock (_dicCsToDbLock) {
						if (_dicCsToDb.ContainsKey(type.FullName) == false)
							_dicCsToDb.Add(type.FullName, newItem);
					}
				}
				return (newItem.Item1, newItem.Item2, newItem.Item3, newItem.Item5);
			}
			return null;
		}

		public string GetComparisonDDLStatements<TEntity>() => this.GetComparisonDDLStatements(typeof(TEntity));
		public string GetComparisonDDLStatements(params Type[] entityTypes) {
			var sb = new StringBuilder();
			var seqcols = new List<(ColumnInfo, string[], bool)>(); //序列
			foreach (var entityType in entityTypes) {
				if (sb.Length > 0) sb.Append("\r\n");
				var tb = _commonUtils.GetTableByEntity(entityType);
				var tboldname = tb.DbOldName?.Split(new[] { '.' }, 2); //旧表名
				if (tboldname?.Length == 1) tboldname = new[] { "public", tboldname[0] };

				var isRenameTable = false;
				var tbname = tb.DbName.Split(new[] { '.' }, 2);
				if (tbname.Length == 1) tbname = new[] { "public", tbname[0] };
				if (_orm.Ado.ExecuteScalar(CommandType.Text, "select 1 from pg_tables a inner join pg_namespace b on b.nspname = a.schemaname where b.nspname || '.' || a.tablename = {0}.{1}".FormatPostgreSQL(tbname)) == null) { //表不存在

					if (tboldname != null && _orm.Ado.ExecuteScalar(CommandType.Text, "select 1 from pg_tables a inner join pg_namespace b on b.nspname = a.schemaname where b.nspname || '.' || a.tablename = {0}.{1}".FormatPostgreSQL(tboldname)) != null) { //旧表存在
																																																															//修改表名
						sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tboldname[0]}.{tboldname[1]}")).Append(" RENAME TO ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(";\r\n");
						isRenameTable = true;

					} else {
						//创建表
						sb.Append("CREATE TABLE IF NOT EXISTS ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" (");
						foreach (var tbcol in tb.Columns.Values) {
							sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType.ToUpper()).Append(",");
							if (tbcol.Attribute.IsIdentity) seqcols.Add((tbcol, tbname, true));
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
case when a.attnotnull then '0' else '1' end as is_nullable,
case when e.adsrc = 1 then '1' else '0' end as is_identity,
a.attndims
from pg_class c
inner join pg_attribute a on a.attnum > 0 and a.attrelid = c.oid
inner join pg_type t on t.oid = a.atttypid
left join pg_type t2 on t2.oid = t.typelem
left join pg_description d on d.objoid = a.attrelid and d.objsubid = a.attnum
left join pg_attrdef e on e.adrelid = a.attrelid and e.adnum = a.attnum
inner join pg_namespace ns on ns.oid = c.relnamespace
inner join pg_namespace ns2 on ns2.oid = t.typnamespace
where ns.nspname = {0} and c.relname = {1}".FormatPostgreSQL(isRenameTable ? tboldname : tbname);
				var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
				foreach (var row in ds) {
					string column = string.Concat(row[0]);
					string sqlType = string.Concat(row[3]);
					long max_length = long.Parse(string.Concat(row[2]));
					bool is_nullable = string.Concat(row[4]) == "1";
					bool is_identity = string.Concat(row[5]).StartsWith(@"nextval('") && string.Concat(row[6]).EndsWith(@"_seq'::regclass)");
					var attndims = long.Parse(string.Concat(row[6]));
					if (attndims > 0) sqlType += "[]";

					if (addcols.TryGetValue(column, out var trycol)) {
						if (trycol.Attribute.DbType.ToLower().StartsWith(sqlType.ToLower()) == false ||
							(trycol.Attribute.DbType.IndexOf("NOT NULL") == -1) != is_nullable) {
							sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ALTER COLUMN ").Append(_commonUtils.QuoteSqlName(column)).Append(" TYPE ").Append(trycol.Attribute.DbType.ToUpper()).Append(";\r\n");
						}
						if (trycol.Attribute.IsIdentity != is_identity) seqcols.Add((trycol, tbname, trycol.Attribute.IsIdentity));
						addcols.Remove(column);
					} else {
						if (trycol.Attribute.IsIdentity != is_identity) seqcols.Add((trycol, tbname, trycol.Attribute.IsIdentity));
						surplus.Add(column, true); //记录剩余字段
					}
				}
				foreach (var addcol in addcols.Values) {
					if (string.IsNullOrEmpty(addcol.Attribute.OldName) == false && surplus.ContainsKey(addcol.Attribute.OldName)) { //修改列名
						sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" RENAME COLUMN ").Append(_commonUtils.QuoteSqlName(addcol.Attribute.OldName)).Append(" TO ").Append(_commonUtils.QuoteSqlName(addcol.Attribute.Name)).Append(";\r\n");
					} else { //添加列
						sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ADD COLUMN ").Append(_commonUtils.QuoteSqlName(addcol.Attribute.Name)).Append(" ").Append(addcol.Attribute.DbType.ToUpper()).Append(";\r\n");
					}
				}
			}
			foreach(var seqcol in seqcols) {
				var tbname = seqcol.Item2;
				var seqname = Utils.GetCsName($"{tbname[0]}.{tbname[1]}_{seqcol.Item1.Attribute.Name}_sequence_name");
				var tbname2 = _commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}");
				var colname2 = _commonUtils.QuoteSqlName(seqcol.Item1.Attribute.Name);
				sb.Append("ALTER TABLE ").Append(tbname2).Append(" ALTER COLUMN ").Append(colname2).Append(" SET DEFAULT null;");
				sb.Append("DROP SEQUENCE IF EXISTS ").Append(seqname).Append(";");
				if (seqcol.Item3) {
					sb.Append("CREATE SEQUENCE ").Append(seqname).Append(" START WITH (select coalesce(max(").Append(colname2).Append("),1) from ").Append(tbname2).Append(");");
					sb.Append("ALTER TABLE ").Append(tbname2).Append(" ALTER COLUMN ").Append(colname2).Append(" SET DEFAULT nextval('").Append(seqname).Append("'::regclass);");
				}
			}
			return sb.Length == 0 ? null : sb.ToString();
		}

		ConcurrentDictionary<string, bool> dicSyced = new ConcurrentDictionary<string, bool>();
		public bool SyncStructure<TEntity>() => this.SyncStructure(typeof(TEntity));
		public bool SyncStructure(params Type[] entityTypes) {
			if (entityTypes == null) return true;
			var syncTypes = entityTypes.Where(a => dicSyced.ContainsKey(a.FullName) == false).ToArray();
			if (syncTypes.Any() == false) return true;
			var ddl = this.GetComparisonDDLStatements(syncTypes);
			if (string.IsNullOrEmpty(ddl)) {
				foreach (var syncType in syncTypes) dicSyced.TryAdd(syncType.FullName, true);
				return true;
			}
			var affrows = _orm.Ado.ExecuteNonQuery(CommandType.Text, ddl);
			foreach (var syncType in syncTypes) dicSyced.TryAdd(syncType.FullName, true);
			return affrows > 0;
		}

	}
}