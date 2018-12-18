using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using MySql.Data.MySqlClient;
using System;
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

		public bool IsAutoSyncStructure { get; set; } = true;

		static object _dicCsToDbLock = new object();
		static Dictionary<string, (MySqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable)> _dicCsToDb = new Dictionary<string, (MySqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable)>() {
				{ typeof(bool).FullName,  (MySqlDbType.Bit, "bit","bit(1) NOT NULL", null, false) },{ typeof(bool?).FullName,  (MySqlDbType.Bit, "bit","bit(1)", null, true) },

				{ typeof(sbyte).FullName,  (MySqlDbType.Byte, "tinyint", "tinyint(3) NOT NULL", false, false) },{ typeof(sbyte?).FullName,  (MySqlDbType.Byte, "tinyint", "tinyint(3)", false, true) },
				{ typeof(short).FullName,  (MySqlDbType.Int16, "smallint","smallint(6) NOT NULL", false, false) },{ typeof(short?).FullName,  (MySqlDbType.Int16, "smallint", "smallint(6)", false, true) },
				{ typeof(int).FullName,  (MySqlDbType.Int32, "int", "int(11) NOT NULL", false, false) },{ typeof(int?).FullName,  (MySqlDbType.Int32, "int", "int(11)", false, true) },
				{ typeof(long).FullName,  (MySqlDbType.Int64, "bigint","bigint(20) NOT NULL", false, false) },{ typeof(long?).FullName,  (MySqlDbType.Int64, "bigint","bigint(20)", false, true) },

				{ typeof(byte).FullName,  (MySqlDbType.UByte, "tinyint","tinyint(3) unsigned NOT NULL", true, false) },{ typeof(byte?).FullName,  (MySqlDbType.UByte, "tinyint","tinyint(3) unsigned", true, true) },
				{ typeof(ushort).FullName,  (MySqlDbType.UInt16, "smallint","smallint(5) unsigned NOT NULL", true, false) },{ typeof(ushort?).FullName,  (MySqlDbType.UInt16, "smallint", "smallint(5) unsigned", true, true) },
				{ typeof(uint).FullName,  (MySqlDbType.UInt32, "int", "int(10) unsigned NOT NULL", true, false) },{ typeof(uint?).FullName,  (MySqlDbType.UInt32, "int", "int(10) unsigned", true, true) },
				{ typeof(ulong).FullName,  (MySqlDbType.UInt64, "bigint", "bigint(20) unsigned NOT NULL", true, false) },{ typeof(ulong?).FullName,  (MySqlDbType.UInt64, "bigint", "bigint(20) unsigned", true, true) },

				{ typeof(double).FullName,  (MySqlDbType.Double, "double", "double NOT NULL", false, false) },{ typeof(double?).FullName,  (MySqlDbType.Double, "double", "double", false, true) },
				{ typeof(float).FullName,  (MySqlDbType.Float, "float","float NOT NULL", false, false) },{ typeof(float?).FullName,  (MySqlDbType.Float, "float","float", false, true) },
				{ typeof(decimal).FullName,  (MySqlDbType.Decimal, "decimal", "decimal(10,2) NOT NULL", false, false) },{ typeof(decimal?).FullName,  (MySqlDbType.Decimal, "decimal", "decimal(10,2)", false, true) },

				{ typeof(TimeSpan).FullName,  (MySqlDbType.Time, "time","time NOT NULL", false, false) },{ typeof(TimeSpan?).FullName,  (MySqlDbType.Time, "time", "time",false, true) },
				{ typeof(DateTime).FullName,  (MySqlDbType.DateTime, "datetime", "datetime NOT NULL", false, false) },{ typeof(DateTime?).FullName,  (MySqlDbType.DateTime, "datetime", "datetime", false, true) },

				{ typeof(byte[]).FullName,  (MySqlDbType.VarBinary, "varbinary", "varbinary(255)", false, null) },
				{ typeof(string).FullName,  (MySqlDbType.VarChar, "varchar", "varchar(255)", false, null) },

				{ typeof(Guid).FullName,  (MySqlDbType.VarChar, "char", "char(36)", false, false) },{ typeof(Guid?).FullName,  (MySqlDbType.VarChar, "char", "char(36)", false, true) },

				{ typeof(MygisPoint).FullName,  (MySqlDbType.Geometry, "point", "point", false, null) },
				{ typeof(MygisLineString).FullName,  (MySqlDbType.Geometry, "linestring", "linestring", false, null) },
				{ typeof(MygisPolygon).FullName,  (MySqlDbType.Geometry, "polygon", "polygon", false, null) },
				{ typeof(MygisMultiPoint).FullName,  (MySqlDbType.Geometry, "multipoint","multipoint", false, null) },
				{ typeof(MygisMultiLineString).FullName,  (MySqlDbType.Geometry, "multilinestring","multilinestring", false, null) },
				{ typeof(MygisMultiPolygon).FullName,  (MySqlDbType.Geometry, "multipolygon", "multipolygon", false, null) },
			};

		public (int type, string dbtype, string dbtypeFull, bool? isnullable)? GetDbInfo(Type type) {
			if (_dicCsToDb.TryGetValue(type.FullName, out var trydc)) return new (int, string, string, bool?)?(((int)trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable));
			var enumType = type.IsEnum ? type : null;
			if (enumType == null && type.FullName.StartsWith("System.Nullable`1[") && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments.First().IsEnum) enumType = type.GenericTypeArguments.First();
			if (enumType != null) {
				var names = string.Join(",", Enum.GetNames(enumType).Select(a => _commonUtils.FormatSql("{0}", a)));
				var newItem = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any() ?
					(MySqlDbType.Set, "set", $"set({names}){(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true) :
					(MySqlDbType.Enum, "enum", $"enum({names}){(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true);
				if (_dicCsToDb.ContainsKey(type.FullName) == false) {
					lock (_dicCsToDbLock) {
						if (_dicCsToDb.ContainsKey(type.FullName) == false)
							_dicCsToDb.Add(type.FullName, newItem);
					}
				}
				return ((int)newItem.Item1, newItem.Item2, newItem.Item3, newItem.Item5);
			}
			return null;
		}

		public string GetComparisonDDLStatements<TEntity>() => this.GetComparisonDDLStatements(typeof(TEntity));
		public string GetComparisonDDLStatements(params Type[] entityTypes) {
			string database = "";
			using (var conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5))) {
				database = conn.Value.Database;
			}
			var sb = new StringBuilder();
			foreach (var entityType in entityTypes) {
				if (sb.Length > 0) sb.Append("\r\n");
				var tb = _commonUtils.GetTableByEntity(entityType);
				var tboldname = tb.DbOldName?.Split(new[] { '.' }, 2); //旧表名
				if (tboldname?.Length == 1) tboldname = new[] { database, tboldname[0] };

				var isRenameTable = false;
				var tbname = tb.DbName.Split(new[] { '.' }, 2);
				if (tbname.Length == 1) tbname = new[] { database, tbname[0] };
				if (_orm.Ado.ExecuteScalar(CommandType.Text, "SELECT 1 FROM information_schema.TABLES WHERE table_schema={0} and table_name={1}".FormatMySql(tbname)) == null) { //表不存在

					if (tboldname != null && _orm.Ado.ExecuteScalar(CommandType.Text, "SELECT 1 FROM information_schema.TABLES WHERE table_schema={0} and table_name={1}".FormatMySql(tboldname)) != null) { //旧表存在
																																																			   //修改表名
						sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tboldname[0]}.{tboldname[1]}")).Append(" RENAME TO ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(";\r\n");
						isRenameTable = true;

					} else {
						//创建表
						sb.Append("CREATE TABLE IF NOT EXISTS ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" (");
						foreach (var tbcol in tb.Columns.Values) {
							sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ");
							sb.Append(tbcol.Attribute.DbType.ToUpper());
							if (tbcol.Attribute.IsIdentity && tbcol.Attribute.DbType.IndexOf("AUTO_INCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" AUTO_INCREMENT");
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
				}
				//对比字段，只可以修改类型、增加字段、有限的修改字段名；保证安全不删除字段
				var addcols = new Dictionary<string, ColumnInfo>(StringComparer.CurrentCultureIgnoreCase);
				foreach (var tbcol in tb.Columns) addcols.Add(tbcol.Value.Attribute.Name, tbcol.Value);
				var surplus = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
				var dbcols = new List<DbColumnInfo>();
				var sql = @"select
a.column_name,
a.column_type,
case when a.is_nullable = 'YES' then 1 else 0 end 'is_nullable',
case when locate('auto_increment', a.extra) > 0 then 1 else 0 end 'is_identity'
from information_schema.columns a
where a.table_schema in ({0}) and a.table_name in ({1})".FormatMySql(isRenameTable ? tboldname : tbname);
				var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
				foreach (var row in ds) {
					string column = string.Concat(row[0]);
					string sqlType = string.Concat(row[1]);
					bool is_nullable = string.Concat(row[2]) == "1";
					bool is_identity = string.Concat(row[3]) == "1";
					bool is_unsigned = sqlType.EndsWith(" unsigned");

					if (addcols.TryGetValue(column, out var trycol)) {
						if ((trycol.Attribute.DbType.IndexOf(" unsigned", StringComparison.CurrentCultureIgnoreCase) != -1) != is_unsigned ||
							Regex.Replace(trycol.Attribute.DbType, @"\([^\)]+\)", m => Regex.Replace(m.Groups[0].Value, @"\s", "")).StartsWith(sqlType, StringComparison.CurrentCultureIgnoreCase) == false ||
							(trycol.Attribute.DbType.IndexOf("NOT NULL", StringComparison.CurrentCultureIgnoreCase) == -1) != is_nullable ||
							trycol.Attribute.IsIdentity != is_identity) {
							sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" MODIFY ").Append(_commonUtils.QuoteSqlName(column)).Append(" ").Append(trycol.Attribute.DbType.ToUpper());
							if (trycol.Attribute.IsIdentity && trycol.Attribute.DbType.IndexOf("AUTO_INCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" AUTO_INCREMENT");
							sb.Append(";\r\n");
						}
						addcols.Remove(column);
					} else
						surplus.Add(column, true); //记录剩余字段
				}
				foreach (var addcol in addcols.Values) {
					if (string.IsNullOrEmpty(addcol.Attribute.OldName) == false && surplus.ContainsKey(addcol.Attribute.OldName)) { //修改列名
						sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" CHANGE COLUMN ").Append(_commonUtils.QuoteSqlName(addcol.Attribute.OldName)).Append(" ").Append(_commonUtils.QuoteSqlName(addcol.Attribute.Name)).Append(" ").Append(addcol.Attribute.DbType.ToUpper());
						if (addcol.Attribute.IsIdentity && addcol.Attribute.DbType.IndexOf("AUTO_INCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" AUTO_INCREMENT");
						sb.Append(";\r\n");

					} else { //添加列
						sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ADD ").Append(_commonUtils.QuoteSqlName(addcol.Attribute.Name)).Append(" ").Append(addcol.Attribute.DbType.ToUpper());
						if (addcol.Attribute.IsIdentity && addcol.Attribute.DbType.IndexOf("AUTO_INCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" AUTO_INCREMENT");
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