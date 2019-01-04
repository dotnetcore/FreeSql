using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Oracle {

	class OracleCodeFirst : ICodeFirst {
		IFreeSql _orm;
		protected CommonUtils _commonUtils;
		protected CommonExpression _commonExpression;
		public OracleCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) {
			_orm = orm;
			_commonUtils = commonUtils;
			_commonExpression = commonExpression;
		}

		public bool IsAutoSyncStructure { get; set; } = true;
		public bool IsSyncStructureToLower { get; set; } = false;

		static object _dicCsToDbLock = new object();
		static Dictionary<string, (OracleDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)> _dicCsToDb = new Dictionary<string, (OracleDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)>() {
				{ typeof(bool).FullName,  (OracleDbType.Boolean, "number","number(1) NOT NULL", null, false, false) },{ typeof(bool?).FullName,  (OracleDbType.Boolean, "number","number(1) NULL", null, true, null) },

				{ typeof(sbyte).FullName,  (OracleDbType.Byte, "number", "number(4) NOT NULL", false, false, 0) },{ typeof(sbyte?).FullName,  (OracleDbType.Byte, "number", "number(4) NULL", false, true, null) },
				{ typeof(short).FullName,  (OracleDbType.Int16, "number","number(4) NOT NULL", false, false, 0) },{ typeof(short?).FullName,  (OracleDbType.Int16, "number", "number(4) NULL", false, true, null) },
				{ typeof(int).FullName,  (OracleDbType.Int32, "number", "number(8) NOT NULL", false, false, 0) },{ typeof(int?).FullName,  (OracleDbType.Int32, "number", "number(8) NULL", false, true, null) },
				{ typeof(long).FullName,  (OracleDbType.Int64, "number","number(16) NOT NULL", false, false, 0) },{ typeof(long?).FullName,  (OracleDbType.Int64, "bigint","bigint(16) NULL", false, true, null) },

				{ typeof(byte).FullName,  (OracleDbType.Byte, "number","number(2) NOT NULL", true, false, 0) },{ typeof(byte?).FullName,  (OracleDbType.Byte, "number","number(2) NULL", true, true, null) },
				{ typeof(ushort).FullName,  (OracleDbType.Int16, "number","number(8) NOT NULL", true, false, 0) },{ typeof(ushort?).FullName,  (OracleDbType.Int16, "number", "number(8) NULL", true, true, null) },
				{ typeof(uint).FullName,  (OracleDbType.Int32, "number", "number(16) NOT NULL", true, false, 0) },{ typeof(uint?).FullName,  (OracleDbType.Int32, "number", "number(16) NULL", true, true, null) },
				{ typeof(ulong).FullName,  (OracleDbType.Int64, "number", "number(32) NOT NULL", true, false, 0) },{ typeof(ulong?).FullName,  (OracleDbType.Int64, "number", "number(32) NULL", true, true, null) },

				{ typeof(double).FullName,  (OracleDbType.Double, "double", "double(126) NOT NULL", false, false, 0) },{ typeof(double?).FullName,  (OracleDbType.Double, "double", "double(126) NULL", false, true, null) },
				{ typeof(float).FullName,  (OracleDbType.Single, "float","float(63) NOT NULL", false, false, 0) },{ typeof(float?).FullName,  (OracleDbType.Single, "float","float(63) NULL", false, true, null) },
				{ typeof(decimal).FullName,  (OracleDbType.Decimal, "number", "number(10,2) NOT NULL", false, false, 0) },{ typeof(decimal?).FullName,  (OracleDbType.Decimal, "number", "number(10,2) NULL", false, true, null) },

				{ typeof(TimeSpan).FullName,  (OracleDbType.IntervalDS, "interval day to second","interval day(2) to second(6) NOT NULL", false, false, 0) },{ typeof(TimeSpan?).FullName,  (OracleDbType.IntervalDS, "interval day to second", "interval day(2) to second(6) NULL",false, true, null) },
				{ typeof(DateTime).FullName,  (OracleDbType.TimeStamp, "timestamp", "timestamp(6) NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateTime?).FullName,  (OracleDbType.TimeStamp, "timestamp", "timestamp(6) NULL", false, true, null) },
				{ typeof(DateTimeOffset).FullName,  (OracleDbType.TimeStampLTZ, "timestamp with local time zone", "timestamp(6) with local time zone NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateTime?).FullName,  (OracleDbType.TimeStamp, "timestamp with local time zone", "timestamp(6) with local time zone NULL", false, true, null) },

				{ typeof(byte[]).FullName,  (OracleDbType.Blob, "blog", "blog(4000) NULL", false, null, new byte[0]) },
				{ typeof(string).FullName,  (OracleDbType.NVarchar2, "nvarchar2", "nvarchar2(255) NULL", false, null, "") },

				{ typeof(Guid).FullName,  (OracleDbType.Char, "char", "char(36 BYTE)", false, false, Guid.Empty) },{ typeof(Guid?).FullName,  (OracleDbType.Char, "char", "char(36 BYTE) NULL", false, true, null) },
			};

		public (int type, string dbtype, string dbtypeFull, bool? isnullable, object defaultValue)? GetDbInfo(Type type) {
			if (_dicCsToDb.TryGetValue(type.FullName, out var trydc)) return new (int, string, string, bool?, object)?(((int)trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable, trydc.defaultValue));
			if (type.IsArray) return null;
			var enumType = type.IsEnum ? type : null;
			if (enumType == null && type.FullName.StartsWith("System.Nullable`1[") && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments.First().IsEnum) enumType = type.GenericTypeArguments.First();
			if (enumType != null) {
				var newItem = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any() ?
					(OracleDbType.Int32, "number", $"number(16){(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0)) :
					(OracleDbType.Int64, "number", $"number(32){(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0));
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
			var seqcols = new List<(ColumnInfo, string[], bool)>(); //序列

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

					if (string.Compare(tbname[0], database, true) != 0 && ExecuteScalar(database, " select 1 from sys.dba_users where username={0}".FormatOracleSQL(tbname[0])) == null) //创建数据库
						throw new NotImplementedException($"Oracle CodeFirst 不支持代码创建 tablespace 与 schemas {tbname[0]}");

					var sbalter = new StringBuilder();
					var istmpatler = false; //创建临时表，导入数据，删除旧表，修改
					if (ExecuteScalar(tbname[0], " select 1 from all_tab_comments where owner={0} and table_name={1}".FormatOracleSQL(tbname)) == null) { //表不存在
						if (tboldname != null) {
							if (ExecuteScalar(tboldname[0], " select 1 from all_tab_comments where owner={0} and table_name={1}".FormatOracleSQL(tboldname)) == null)
								//模式或表不存在
								tboldname = null;
						}
						if (tboldname == null) {
							//创建表
							sb.Append("CREATE TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" (");
							foreach (var tbcol in tb.Columns.Values) {
								sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType).Append(",");
								if (tbcol.Attribute.IsIdentity) seqcols.Add((tbcol, tbname, true));
							}
							if (tb.Primarys.Any() == false)
								sb.Remove(sb.Length - 1, 1);
							else {
								sb.Append(" \r\n  CONSTRAINT ").Append(tbname[0]).Append("_").Append(tbname[1]).Append("_pk PRIMARY KEY (");
								foreach (var tbcol in tb.Primarys) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
								sb.Remove(sb.Length - 2, 2).Append(")");
							}
							sb.Append("\r\n) LOGGING \r\nNOCOMPRESS \r\nNOCACHE\r\n;\r\n");
							continue;
						}
						//如果新表，旧表在一个模式下，直接修改表名
						if (string.Compare(tbname[0], tboldname[0], true) == 0)
							sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tboldname[0]}.{tboldname[1]}")).Append(" RENAME TO ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(";\r\n");
						else {
							//如果新表，旧表不在一起，创建新表，导入数据，删除旧表
							istmpatler = true;
						}
					} else
						tboldname = null; //如果新表已经存在，不走改表名逻辑

					//对比字段，只可以修改类型、增加字段、有限的修改字段名；保证安全不删除字段
					var addcols = new Dictionary<string, ColumnInfo>(StringComparer.CurrentCultureIgnoreCase);
					foreach (var tbcol in tb.Columns) addcols.Add(tbcol.Value.Attribute.Name, tbcol.Value);
					var surplus = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
					var dbcols = new List<DbColumnInfo>();
					var sql = $@"
select 
column_name,
data_type,
data_length,
data_precision,
data_scale,
char_used,
case when nullable = 'Y' then 1 else 0 end,
nvl((select 1 from user_sequences where sequence_name='{Utils.GetCsName(string.Join(".", tboldname ?? tbname))}_seq_'||all_tab_columns.column_name), 0)
from all_tab_columns
where owner={{0}} and table_name={{1}}".FormatOracleSQL(tboldname ?? tbname);
					var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
					var tbstruct = ds.ToDictionary(a => string.Concat(a[0]), a => {
						var sqlType = string.Concat(a[1]);
						var data_length = long.Parse(string.Concat(a[2]));
						long.TryParse(string.Concat(a[3]), out var data_precision);
						long.TryParse(string.Concat(a[4]), out var data_scale);
						var char_used = string.Concat(a[5]);
						if (Regex.IsMatch(sqlType, @"INTERVAL DAY\(\d+\) TO SECOND\(\d+\)", RegexOptions.IgnoreCase)) {
						} else if (Regex.IsMatch(sqlType, @"INTERVAL YEAR\(\d+\) TO MONTH", RegexOptions.IgnoreCase)) {
						} else if (sqlType.StartsWith("TIMESTAMP", StringComparison.CurrentCultureIgnoreCase)) {
						} else if (char_used.ToLower() == "c") {
							sqlType += $"({data_length} CHAR)";
						} else if (char_used.ToLower() == "b") {
							sqlType += $"({data_length} BYTE)";
						} else if (sqlType.ToLower() == "float") {
							sqlType += $"({data_precision})";
						} else if (data_precision > 0 && data_scale > 0) {
							sqlType += $"({data_precision},{data_scale})";
						} else {
							sqlType += $"({data_length})";
						}
						return new {
							column = string.Concat(a[0]),
							sqlType,
							is_nullable = string.Concat(a[6]) == "1",
							is_identity = string.Concat(a[7]) == "1"
						};
					}, StringComparer.CurrentCultureIgnoreCase);

					if (istmpatler == false) {
						foreach (var tbcol in tb.Columns.Values) {
							if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
							string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol)) {
								if (tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType, StringComparison.CurrentCultureIgnoreCase) == false)
									sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" MODIFY (").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" ").Append(tbcol.Attribute.DbType).Append(");\r\n");
								if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable) {
									if (tbcol.Attribute.IsNullable == false)
										sbalter.Append("UPDATE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" SET ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(_commonUtils.FormatSql(" = {0}", tbcol.Attribute.DbDefautValue)).Append(" WHERE ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" IS NULL;\r\n");
									sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" MODIFY ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" ").Append(tbcol.Attribute.IsNullable ? "" : "NOT").Append(" NULL;\r\n");
								}
								if (tbcol.Attribute.IsIdentity != tbstructcol.is_identity)
									seqcols.Add((tbcol, tbname, tbcol.Attribute.IsIdentity));
								if (tbstructcol.column == tbcol.Attribute.OldName)
									//修改列名
									sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" RENAME ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.OldName)).Append(" TO ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(";\r\n");
								continue;
							}
							//添加列
							sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ADD COLUMN ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType.Split(' ').First()).Append(";\r\n");
							sbalter.Append("UPDATE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" SET ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(_commonUtils.FormatSql(" = {0};\r\n", tbcol.Attribute.DbDefautValue));
							if (tbcol.Attribute.IsNullable == false) sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" MODIFY ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" NOT NULL;\r\n");
							if (tbcol.Attribute.IsIdentity) seqcols.Add((tbcol, tbname, tbcol.Attribute.IsIdentity));
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
					sb.Append("CREATE TABLE ").Append(tmptablename).Append(" (");
					foreach (var tbcol in tb.Columns.Values) {
						sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType).Append(",");
						if (tbcol.Attribute.IsIdentity) seqcols.Add((tbcol, tbname, true));
					}
					if (tb.Primarys.Any() == false)
						sb.Remove(sb.Length - 1, 1);
					else {
						sb.Append(" \r\n  CONSTRAINT ").Append(tbname[0]).Append("_").Append(tbname[1]).Append("_pk PRIMARY KEY (");
						foreach (var tbcol in tb.Primarys) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
						sb.Remove(sb.Length - 2, 2).Append(")");
					}
					sb.Append("\r\n) LOGGING \r\nNOCOMPRESS \r\nNOCACHE\r\n;\r\n");
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
								insertvalue = $"nvl({insertvalue},{_commonUtils.FormatSql("{0}", tbcol.Attribute.DbDefautValue).Replace("'", "''")})";
						} else if (tbcol.Attribute.IsNullable == false)
							insertvalue = _commonUtils.FormatSql("{0}", tbcol.Attribute.DbDefautValue).Replace("'", "''");
						sb.Append(insertvalue).Append(", ");
					}
					sb.Remove(sb.Length - 2, 2).Append(" FROM ").Append(tablename).Append(";\r\n");
					sb.Append("DROP TABLE ").Append(tablename).Append(";\r\n");
					sb.Append("ALTER TABLE ").Append(tmptablename).Append(" RENAME TO ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(";\r\n");
				}
				foreach (var seqcol in seqcols) {
					var tbname = seqcol.Item2;
					var seqname = Utils.GetCsName($"{tbname[0]}.{tbname[1]}_seq_{seqcol.Item1.Attribute.Name}");
					var tbname2 = _commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}");
					var colname2 = _commonUtils.QuoteSqlName(seqcol.Item1.Attribute.Name);
					sb.Append("DROP SEQUENCE ").Append(seqname).Append(";\r\n");
					if (seqcol.Item3) {
						var startWith = _orm.Ado.ExecuteScalar(CommandType.Text, $" select nvl({colname2},0) from {tbname2}");
						sb.Append("CREATE SEQUENCE ").Append(seqname).Append(" start with ").Append(startWith).Append(";\r\n");
					}
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