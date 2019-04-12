using FreeSql.DataAnnotations;
using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.MySql {

	class MySqlCodeFirst : ICodeFirst {
		IFreeSql _orm;
		protected CommonUtils _commonUtils;
		protected CommonExpression _commonExpression;
		public MySqlCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) {
			_orm = orm;
			_commonUtils = commonUtils;
			_commonExpression = commonExpression;
		}

		public bool IsAutoSyncStructure { get; set; } = false;
		public bool IsSyncStructureToLower { get; set; } = false;
		public bool IsSyncStructureToUpper { get; set; } = false;
		public bool IsConfigEntityFromDbFirst { get; set; } = false;
		public bool IsNoneCommandParameter { get; set; } = false;
		public bool IsLazyLoading { get; set; } = false;

		static object _dicCsToDbLock = new object();
		static Dictionary<string, (MySqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)> _dicCsToDb = new Dictionary<string, (MySqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)>() {
				{ typeof(bool).FullName,  (MySqlDbType.Bit, "bit","bit(1) NOT NULL", null, false, false) },{ typeof(bool?).FullName,  (MySqlDbType.Bit, "bit","bit(1)", null, true, null) },

				{ typeof(sbyte).FullName,  (MySqlDbType.Byte, "tinyint", "tinyint(3) NOT NULL", false, false, 0) },{ typeof(sbyte?).FullName,  (MySqlDbType.Byte, "tinyint", "tinyint(3)", false, true, null) },
				{ typeof(short).FullName,  (MySqlDbType.Int16, "smallint","smallint(6) NOT NULL", false, false, 0) },{ typeof(short?).FullName,  (MySqlDbType.Int16, "smallint", "smallint(6)", false, true, null) },
				{ typeof(int).FullName,  (MySqlDbType.Int32, "int", "int(11) NOT NULL", false, false, 0) },{ typeof(int?).FullName,  (MySqlDbType.Int32, "int", "int(11)", false, true, null) },
				{ typeof(long).FullName,  (MySqlDbType.Int64, "bigint","bigint(20) NOT NULL", false, false, 0) },{ typeof(long?).FullName,  (MySqlDbType.Int64, "bigint","bigint(20)", false, true, null) },

				{ typeof(byte).FullName,  (MySqlDbType.UByte, "tinyint","tinyint(3) unsigned NOT NULL", true, false, 0) },{ typeof(byte?).FullName,  (MySqlDbType.UByte, "tinyint","tinyint(3) unsigned", true, true, null) },
				{ typeof(ushort).FullName,  (MySqlDbType.UInt16, "smallint","smallint(5) unsigned NOT NULL", true, false, 0) },{ typeof(ushort?).FullName,  (MySqlDbType.UInt16, "smallint", "smallint(5) unsigned", true, true, null) },
				{ typeof(uint).FullName,  (MySqlDbType.UInt32, "int", "int(10) unsigned NOT NULL", true, false, 0) },{ typeof(uint?).FullName,  (MySqlDbType.UInt32, "int", "int(10) unsigned", true, true, null) },
				{ typeof(ulong).FullName,  (MySqlDbType.UInt64, "bigint", "bigint(20) unsigned NOT NULL", true, false, 0) },{ typeof(ulong?).FullName,  (MySqlDbType.UInt64, "bigint", "bigint(20) unsigned", true, true, null) },

				{ typeof(double).FullName,  (MySqlDbType.Double, "double", "double NOT NULL", false, false, 0) },{ typeof(double?).FullName,  (MySqlDbType.Double, "double", "double", false, true, null) },
				{ typeof(float).FullName,  (MySqlDbType.Float, "float","float NOT NULL", false, false, 0) },{ typeof(float?).FullName,  (MySqlDbType.Float, "float","float", false, true, null) },
				{ typeof(decimal).FullName,  (MySqlDbType.Decimal, "decimal", "decimal(10,2) NOT NULL", false, false, 0) },{ typeof(decimal?).FullName,  (MySqlDbType.Decimal, "decimal", "decimal(10,2)", false, true, null) },

				{ typeof(TimeSpan).FullName,  (MySqlDbType.Time, "time","time NOT NULL", false, false, 0) },{ typeof(TimeSpan?).FullName,  (MySqlDbType.Time, "time", "time",false, true, null) },
				{ typeof(DateTime).FullName,  (MySqlDbType.DateTime, "datetime(3)", "datetime(3) NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateTime?).FullName,  (MySqlDbType.DateTime, "datetime(3)", "datetime(3)", false, true, null) },

				{ typeof(byte[]).FullName,  (MySqlDbType.VarBinary, "varbinary", "varbinary(255)", false, null, new byte[0]) },
				{ typeof(string).FullName,  (MySqlDbType.VarChar, "varchar", "varchar(255)", false, null, "") },

				{ typeof(Guid).FullName,  (MySqlDbType.VarChar, "char", "char(36) NOT NULL", false, false, Guid.Empty) },{ typeof(Guid?).FullName,  (MySqlDbType.VarChar, "char", "char(36)", false, true, null) },

				{ typeof(MygisPoint).FullName,  (MySqlDbType.Geometry, "point", "point", false, null, new MygisPoint(0, 0)) },
				{ typeof(MygisLineString).FullName,  (MySqlDbType.Geometry, "linestring", "linestring", false, null, new MygisLineString(new[]{new MygisCoordinate2D(),new MygisCoordinate2D()})) },
				{ typeof(MygisPolygon).FullName,  (MySqlDbType.Geometry, "polygon", "polygon", false, null, new MygisPolygon(new[]{new[]{new MygisCoordinate2D(),new MygisCoordinate2D()},new[]{new MygisCoordinate2D(),new MygisCoordinate2D()}})) },
				{ typeof(MygisMultiPoint).FullName,  (MySqlDbType.Geometry, "multipoint","multipoint", false, null, new MygisMultiPoint(new[]{new MygisCoordinate2D(),new MygisCoordinate2D()})) },
				{ typeof(MygisMultiLineString).FullName,  (MySqlDbType.Geometry, "multilinestring","multilinestring", false, null, new MygisMultiLineString(new[]{new[]{new MygisCoordinate2D(),new MygisCoordinate2D()},new[]{new MygisCoordinate2D(),new MygisCoordinate2D()}})) },
				{ typeof(MygisMultiPolygon).FullName,  (MySqlDbType.Geometry, "multipolygon", "multipolygon", false, null, new MygisMultiPolygon(new[]{new MygisPolygon(new[]{new[]{new MygisCoordinate2D(),new MygisCoordinate2D()},new[]{new MygisCoordinate2D(),new MygisCoordinate2D()}}),new MygisPolygon(new[]{new[]{new MygisCoordinate2D(),new MygisCoordinate2D()},new[]{new MygisCoordinate2D(),new MygisCoordinate2D()}})})) },
			};

		public (int type, string dbtype, string dbtypeFull, bool? isnullable, object defaultValue)? GetDbInfo(Type type) {
			if (_dicCsToDb.TryGetValue(type.FullName, out var trydc)) return new (int, string, string, bool?, object)?(((int)trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable, trydc.defaultValue));
			if (type.IsArray) return null;
			var enumType = type.IsEnum ? type : null;
			if (enumType == null && type.IsNullableType() && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments.First().IsEnum) enumType = type.GenericTypeArguments.First();
			if (enumType != null) {
				var names = string.Join(",", Enum.GetNames(enumType).Select(a => _commonUtils.FormatSql("{0}", a)));
				var newItem = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any() ?
					(MySqlDbType.Set, "set", $"set({names}){(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0)) :
					(MySqlDbType.Enum, "enum", $"enum({names}){(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0));
				if (_dicCsToDb.ContainsKey(type.FullName) == false) {
					lock (_dicCsToDbLock) {
						if (_dicCsToDb.ContainsKey(type.FullName) == false)
							_dicCsToDb.Add(type.FullName, newItem);
					}
				}
				return ((int)newItem.Item1, newItem.Item2, newItem.Item3, newItem.Item5, newItem.Item6);
			}
			return null;
		}

		public string GetComparisonDDLStatements<TEntity>() => this.GetComparisonDDLStatements(typeof(TEntity));
		public string GetComparisonDDLStatements(params Type[] entityTypes) {
			var conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5));
			var database = conn.Value.Database;
			Func<string, string, object> ExecuteScalar = (db, sql) => {
				if (string.Compare(database, db) != 0) conn.Value.ChangeDatabase(db);
				try {
					using (var cmd = conn.Value.CreateCommand()) {
						cmd.CommandText = sql;
						cmd.CommandType = CommandType.Text;
						return cmd.ExecuteScalar();
					}
				} finally {
					if (string.Compare(database, db) != 0) conn.Value.ChangeDatabase(database);
				}
			};
			var sb = new StringBuilder();
			try {
				foreach (var entityType in entityTypes) {
					if (sb.Length > 0) sb.Append("\r\n");
					var tb = _commonUtils.GetTableByEntity(entityType);
					var tbname = tb.DbName.Split(new[] { '.' }, 2);
					if (tbname?.Length == 1) tbname = new[] { database, tbname[0] };

					var tboldname = tb.DbOldName?.Split(new[] { '.' }, 2); //旧表名
					if (tboldname?.Length == 1) tboldname = new[] { database, tboldname[0] };

					if (string.Compare(tbname[0], database, true) != 0 && ExecuteScalar(database, " select 1 from pg_database where datname={0}".FormatMySql(tbname[0])) == null) //创建数据库
						sb.Append($"CREATE DATABASE IF NOT EXISTS ").Append(_commonUtils.QuoteSqlName(tbname[0])).Append(" default charset utf8 COLLATE utf8_general_ci;\r\n");

					var sbalter = new StringBuilder();
					var istmpatler = false; //创建临时表，导入数据，删除旧表，修改
					if (ExecuteScalar(tbname[0], " SELECT 1 FROM information_schema.TABLES WHERE table_schema={0} and table_name={1}".FormatMySql(tbname)) == null) { //表不存在
						if (tboldname != null) {
							if (string.Compare(tboldname[0], tbname[0], true) != 0 && ExecuteScalar(database, " select 1 from information_schema.schemata where schema_name={0}".FormatMySql(tboldname[0])) == null ||
								ExecuteScalar(tboldname[0], " SELECT 1 FROM information_schema.TABLES WHERE table_schema={0} and table_name={1}".FormatMySql(tboldname)) == null)
								//数据库或表不存在
								tboldname = null;
						}
						if (tboldname == null) {
							//创建表
							sb.Append("CREATE TABLE IF NOT EXISTS ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" (");
							foreach (var tbcol in tb.Columns.Values) {
								sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ");
								sb.Append(tbcol.Attribute.DbType);
								if (tbcol.Attribute.IsIdentity == true && tbcol.Attribute.DbType.IndexOf("AUTO_INCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" AUTO_INCREMENT");
								sb.Append(",");
							}
							if (tb.Primarys.Any() == false)
								sb.Remove(sb.Length - 1, 1);
							else {
								sb.Append(" \r\n  PRIMARY KEY (");
								foreach (var tbcol in tb.Primarys) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
								sb.Remove(sb.Length - 2, 2).Append(")");
							}
							sb.Append("\r\n) Engine=InnoDB CHARACTER SET utf8;\r\n");
							continue;
						}
						//如果新表，旧表在一个数据库下，直接修改表名
						if (string.Compare(tbname[0], tboldname[0], true) == 0)
							sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tboldname[0]}.{tboldname[1]}")).Append(" RENAME TO ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(";\r\n");
						else {
							//如果新表，旧表不在一起，创建新表，导入数据，删除旧表
							istmpatler = true;
						}
					} else
						tboldname = null; //如果新表已经存在，不走改表名逻辑

					//对比字段，只可以修改类型、增加字段、有限的修改字段名；保证安全不删除字段
					var sql = @"
select
a.column_name,
a.column_type,
case when a.is_nullable = 'YES' then 1 else 0 end 'is_nullable',
case when locate('auto_increment', a.extra) > 0 then 1 else 0 end 'is_identity'
from information_schema.columns a
where a.table_schema in ({0}) and a.table_name in ({1})".FormatMySql(tboldname ?? tbname);
					var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
					var tbstruct = ds.ToDictionary(a => string.Concat(a[0]), a => {
						var a1 = string.Concat(a[1]);
						if (a1 == "datetime") a1 = string.Concat(a1, "(0)");
						return new {
							column = string.Concat(a[0]),
							sqlType = a1,
							is_nullable = string.Concat(a[2]) == "1",
							is_identity = string.Concat(a[3]) == "1",
							is_unsigned = string.Concat(a[1]).EndsWith(" unsigned")
						};
					}, StringComparer.CurrentCultureIgnoreCase);

					if (istmpatler == false) {
						var existsPrimary = ExecuteScalar(tbname[0], "select 1 from INFORMATION_SCHEMA.KEY_COLUMN_USAGE where table_schema={0} and table_name={1} limit 1".FormatMySql(tbname));
						foreach (var tbcol in tb.Columns.Values) {
							var isIdentityChanged = tbcol.Attribute.IsIdentity == true && tbcol.Attribute.DbType.IndexOf("AUTO_INCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1;
							if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
								string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol)) {
								if ((tbcol.Attribute.DbType.IndexOf(" unsigned", StringComparison.CurrentCultureIgnoreCase) != -1) != tbstructcol.is_unsigned ||
								tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType, StringComparison.CurrentCultureIgnoreCase) == false ||
								tbcol.Attribute.IsNullable != tbstructcol.is_nullable ||
								tbcol.Attribute.IsIdentity != tbstructcol.is_identity) {
									sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" MODIFY ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" ").Append(tbcol.Attribute.DbType);
									if (isIdentityChanged) sbalter.Append(" AUTO_INCREMENT").Append(existsPrimary == null ? "" : ", DROP PRIMARY KEY").Append(", ADD PRIMARY KEY(").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(")");
									sbalter.Append(";\r\n");
								}
								if (tbstructcol.column == tbcol.Attribute.OldName) {
									//修改列名
									sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" CHANGE COLUMN ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.OldName)).Append(" ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType);
									if (isIdentityChanged) sb.Append(" AUTO_INCREMENT").Append(existsPrimary == null ? "" : ", DROP PRIMARY KEY").Append(", ADD PRIMARY KEY(").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(")");
									sbalter.Append(";\r\n");
								}
								continue;
							}
							//添加列
							sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ADD ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType);
							if (isIdentityChanged) sbalter.Append(" AUTO_INCREMENT").Append(existsPrimary == null ? "" : ", DROP PRIMARY KEY").Append(", ADD PRIMARY KEY(").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(")");
							sbalter.Append(";\r\n");
						}
					}
					if (istmpatler == false) {
						sb.Append(sbalter);
						continue;
					}

					//创建临时表，数据导进临时表，然后删除原表，将临时表改名为原表名
					var tablename = tboldname == null ? _commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}") : _commonUtils.QuoteSqlName($"{tboldname[0]}.{tboldname[1]}");
					var tmptablename = _commonUtils.QuoteSqlName($"{tbname[0]}.FreeSqlTmp_{tbname[1]}");
					//创建临时表
					sb.Append("CREATE TABLE IF NOT EXISTS ").Append(tmptablename).Append(" (");
					foreach (var tbcol in tb.Columns.Values) {
						sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ");
						sb.Append(tbcol.Attribute.DbType);
						if (tbcol.Attribute.IsIdentity == true && tbcol.Attribute.DbType.IndexOf("AUTO_INCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" AUTO_INCREMENT");
						sb.Append(",");
					}
					if (tb.Primarys.Any() == false)
						sb.Remove(sb.Length - 1, 1);
					else {
						sb.Append(" \r\n  PRIMARY KEY (");
						foreach (var tbcol in tb.Primarys) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
						sb.Remove(sb.Length - 2, 2).Append(")");
					}
					sb.Append("\r\n) Engine=InnoDB CHARACTER SET utf8;\r\n");
					sb.Append("INSERT INTO ").Append(tmptablename).Append(" (");
					foreach (var tbcol in tb.Columns.Values)
						sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
					sb.Remove(sb.Length - 2, 2).Append(")\r\nSELECT ");
					foreach (var tbcol in tb.Columns.Values) {
						var insertvalue = "NULL";
						if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
							string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol)) {
							insertvalue = _commonUtils.QuoteSqlName(tbstructcol.column);
							if (tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType, StringComparison.CurrentCultureIgnoreCase) == false) {
								//insertvalue = $"cast({insertvalue} as {tbcol.Attribute.DbType.Split(' ').First()})";
							}
							if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable)
								insertvalue = $"ifnull({insertvalue},{_commonUtils.FormatSql("{0}", tbcol.Attribute.DbDefautValue)})";
						} else if (tbcol.Attribute.IsNullable == false)
							insertvalue = _commonUtils.FormatSql("{0}", tbcol.Attribute.DbDefautValue);
						sb.Append(insertvalue).Append(", ");
					}
					sb.Remove(sb.Length - 2, 2).Append(" FROM ").Append(tablename).Append(";\r\n");
					sb.Append("DROP TABLE ").Append(tablename).Append(";\r\n");
					sb.Append("ALTER TABLE ").Append(tmptablename).Append(" RENAME TO ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(";\r\n");
				}
				return sb.Length == 0 ? null : sb.ToString();
			} finally {
				try {
					conn.Value.ChangeDatabase(database);
					_orm.Ado.MasterPool.Return(conn);
				} catch {
					_orm.Ado.MasterPool.Return(conn, true);
				}
			}
		}

		static object syncStructureLock = new object();
		ConcurrentDictionary<string, bool> dicSyced = new ConcurrentDictionary<string, bool>();
		public bool SyncStructure<TEntity>() => this.SyncStructure(typeof(TEntity));
		public bool SyncStructure(params Type[] entityTypes) {
			if (entityTypes == null) return true;
			var syncTypes = entityTypes.Where(a => dicSyced.ContainsKey(a.FullName) == false).ToArray();
			if (syncTypes.Any() == false) return true;
			lock (syncStructureLock) {
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
		public ICodeFirst ConfigEntity<T>(Action<TableFluent<T>> entity) => _commonUtils.ConfigEntity(entity);
		public ICodeFirst ConfigEntity(Type type, Action<TableFluent> entity) => _commonUtils.ConfigEntity(type, entity);
		public TableAttribute GetConfigEntity(Type type) => _commonUtils.GetConfigEntity(type);
		public TableInfo GetTableByEntity(Type type) => _commonUtils.GetTableByEntity(type);
	}
}