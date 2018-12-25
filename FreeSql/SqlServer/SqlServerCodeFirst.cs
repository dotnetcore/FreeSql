using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.SqlServer {

	class SqlServerCodeFirst : ICodeFirst {
		IFreeSql _orm;
		protected CommonUtils _commonUtils;
		protected CommonExpression _commonExpression;
		public SqlServerCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) {
			_orm = orm;
			_commonUtils = commonUtils;
			_commonExpression = commonExpression;
		}

		public bool IsAutoSyncStructure { get; set; } = true;

		static object _dicCsToDbLock = new object();
		static Dictionary<string, (SqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable)> _dicCsToDb = new Dictionary<string, (SqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable)>() {
				{ typeof(bool).FullName,  (SqlDbType.Bit, "bit","bit NOT NULL", null, false) },{ typeof(bool?).FullName,  (SqlDbType.Bit, "bit","bit", null, true) },

				{ typeof(sbyte).FullName,  (SqlDbType.TinyInt, "tinyint", "tinyint NOT NULL", false, false) },{ typeof(sbyte?).FullName,  (SqlDbType.TinyInt, "tinyint", "tinyint", false, true) },
				{ typeof(short).FullName,  (SqlDbType.SmallInt, "smallint","smallint NOT NULL", false, false) },{ typeof(short?).FullName,  (SqlDbType.SmallInt, "smallint", "smallint", false, true) },
				{ typeof(int).FullName,  (SqlDbType.Int, "int", "int NOT NULL", false, false) },{ typeof(int?).FullName,  (SqlDbType.Int, "int", "int", false, true) },
				{ typeof(long).FullName,  (SqlDbType.BigInt, "bigint","bigint NOT NULL", false, false) },{ typeof(long?).FullName,  (SqlDbType.BigInt, "bigint","bigint", false, true) },

				{ typeof(byte).FullName,  (SqlDbType.TinyInt, "tinyint","tinyint NOT NULL", true, false) },{ typeof(byte?).FullName,  (SqlDbType.TinyInt, "tinyint","tinyint", true, true) },
				{ typeof(ushort).FullName,  (SqlDbType.SmallInt, "smallint","smallint NOT NULL", true, false) },{ typeof(ushort?).FullName,  (SqlDbType.SmallInt, "smallint", "smallint", true, true) },
				{ typeof(uint).FullName,  (SqlDbType.Int, "int", "int NOT NULL", true, false) },{ typeof(uint?).FullName,  (SqlDbType.Int, "int", "int", true, true) },
				{ typeof(ulong).FullName,  (SqlDbType.BigInt, "bigint", "bigint NOT NULL", true, false) },{ typeof(ulong?).FullName,  (SqlDbType.BigInt, "bigint", "bigint", true, true) },

				{ typeof(double).FullName,  (SqlDbType.Float, "float", "float NOT NULL", false, false) },{ typeof(double?).FullName,  (SqlDbType.Float, "float", "float", false, true) },
				{ typeof(float).FullName,  (SqlDbType.Real, "real","real NOT NULL", false, false) },{ typeof(float?).FullName,  (SqlDbType.Real, "real","real", false, true) },
				{ typeof(decimal).FullName,  (SqlDbType.Decimal, "decimal", "decimal(10,2) NOT NULL", false, false) },{ typeof(decimal?).FullName,  (SqlDbType.Decimal, "decimal", "decimal(10,2)", false, true) },

				{ typeof(TimeSpan).FullName,  (SqlDbType.Time, "time","time NOT NULL", false, false) },{ typeof(TimeSpan?).FullName,  (SqlDbType.Time, "time", "time",false, true) },
				{ typeof(DateTime).FullName,  (SqlDbType.DateTime, "datetime", "datetime NOT NULL", false, false) },{ typeof(DateTime?).FullName,  (SqlDbType.DateTime, "datetime", "datetime", false, true) },
				{ typeof(DateTimeOffset).FullName,  (SqlDbType.DateTimeOffset, "datetimeoffset", "datetimeoffset NOT NULL", false, false) },{ typeof(DateTimeOffset?).FullName,  (SqlDbType.DateTimeOffset, "datetimeoffset", "datetimeoffset", false, true) },

				{ typeof(byte[]).FullName,  (SqlDbType.VarBinary, "varbinary", "varbinary(255)", false, null) },
				{ typeof(string).FullName,  (SqlDbType.NVarChar, "nvarchar", "nvarchar(255)", false, null) },

				{ typeof(Guid).FullName,  (SqlDbType.UniqueIdentifier, "uniqueidentifier", "uniqueidentifier NOT NULL", false, false) },{ typeof(Guid?).FullName,  (SqlDbType.UniqueIdentifier, "uniqueidentifier", "uniqueidentifier", false, true) },
			};

		public (int type, string dbtype, string dbtypeFull, bool? isnullable)? GetDbInfo(Type type) {
			if (_dicCsToDb.TryGetValue(type.FullName, out var trydc)) return new (int, string, string, bool?)?(((int)trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable));
			var enumType = type.IsEnum ? type : null;
			if (enumType == null && type.FullName.StartsWith("System.Nullable`1[") && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments.First().IsEnum) enumType = type.GenericTypeArguments.First();
			if (enumType != null) {
				var newItem = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any() ?
					(SqlDbType.BigInt, "bigint", $"bigint{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true) :
					(SqlDbType.Int, "int", $"int{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true);
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
			var conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5));
			var database = conn.Value.Database;
			Func<string, string, object> ExecuteScalar = (db, sql) => {
				if (string.Compare(database, db) != 0) try { conn.Value.ChangeDatabase(db); } catch { }
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
					var tbname = tb.DbName.Split(new[] { '.' }, 3);
					if (tbname?.Length == 1) tbname = new[] { database, "dbo", tbname[0] };
					if (tbname?.Length == 2) tbname = new[] { database, tbname[0], tbname[1] };

					var tboldname = tb.DbOldName?.Split(new[] { '.' }, 3); //旧表名
					if (tboldname?.Length == 1) tboldname = new[] { database, "dbo", tboldname[0] };
					if (tboldname?.Length == 2) tboldname = new[] { database, tboldname[0], tboldname[1] };

					if (string.Compare(tbname[0], database, true) != 0 && ExecuteScalar(database, $"select 1 from sys.databases where name='{tbname[0]}'") == null) //创建数据库
						ExecuteScalar(database, $"if not exists(select 1 from sys.databases where name='{tbname[0]}')\r\n\tcreate database [{tbname[0]}];");
					if (string.Compare(tbname[1], "dbo", true) != 0 && ExecuteScalar(tbname[0], $"select 1 from sys.schemas where name='{tbname[1]}'") == null) //创建模式
						ExecuteScalar(tbname[0], $"create schema [{tbname[1]}] authorization [dbo]");

					var sbalter = new StringBuilder();
					var istmpatler = false; //创建临时表，导入数据，删除旧表，修改
					if (ExecuteScalar(tbname[0], $"select 1 from dbo.sysobjects where id = object_id(N'[{tbname[1]}].[{tbname[2]}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1") == null) { //表不存在
						if (tboldname != null) {
							if (string.Compare(tboldname[0], tbname[0], true) != 0 && ExecuteScalar(database, $"select 1 from sys.databases where name='{tboldname[0]}'") == null ||
								string.Compare(tboldname[1], tbname[1], true) != 0 && ExecuteScalar(tboldname[0], $"select 1 from sys.schemas where name='{tboldname[1]}'") == null ||
								ExecuteScalar(tboldname[0], $"select 1 from dbo.sysobjects where id = object_id(N'[{tboldname[1]}].[{tboldname[2]}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1") == null)
								//数据库或模式或表不存在
								tboldname = null;
						}
						if (tboldname == null) {
							//创建新表
							sb.Append("use ").Append(_commonUtils.QuoteSqlName(tbname[0])).Append(";\r\nCREATE TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[1]}.{tbname[2]}")).Append(" (");
							foreach (var tbcol in tb.Columns.Values) {
								sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ");
								sb.Append(tbcol.Attribute.DbType);
								if (tbcol.Attribute.IsIdentity && tbcol.Attribute.DbType.IndexOf("identity", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" identity(1,1)");
								if (tbcol.Attribute.IsPrimary) sb.Append(" primary key");
								sb.Append(",");
							}
							sb.Remove(sb.Length - 1, 1).Append("\r\n);\r\n");
							continue;
						}
						//如果新表，旧表在一个数据库和模式下，直接修改表名
						if (string.Compare(tbname[0], tboldname[0], true) == 0 &&
							string.Compare(tbname[1], tboldname[1], true) == 0)
							sbalter.Append("use ").Append(_commonUtils.QuoteSqlName(tbname[0])).Append(_commonUtils.FormatSql(";\r\nEXEC sp_rename {0}, {1};\r\n", _commonUtils.QuoteSqlName($"{tboldname[0]}.{tboldname[1]}"), _commonUtils.QuoteSqlName(tbname[2])));
						else {
							//如果新表，旧表不在一起，创建新表，导入数据，删除旧表
							istmpatler = true;
						}
					} else
						tboldname = null; //如果新表已经存在，不走改表名逻辑

					//对比字段，只可以修改类型、增加字段、有限的修改字段名；保证安全不删除字段
					var sql = string.Format(@"
use [{0}];
select
a.name 'Column'
,b.name + case 
 when b.name in ('Char', 'VarChar', 'NChar', 'NVarChar', 'Binary', 'VarBinary') then '(' + 
  case when a.max_length = -1 then 'MAX' 
  when b.name in ('NChar', 'NVarchar') then cast(a.max_length / 2 as varchar)
  else cast(a.max_length as varchar) end + ')'
 when b.name in ('Numeric', 'Decimal') then '(' + cast(a.precision as varchar) + ',' + cast(a.scale as varchar) + ')'
 else '' end as 'SqlType'
,case when a.is_nullable = 1 then '1' else '0' end 'IsNullable'
,case when a.is_identity = 1 then '1' else '0' end 'IsIdentity'
from sys.columns a
inner join sys.types b on b.user_type_id = a.user_type_id
left join sys.extended_properties AS c ON c.major_id = a.object_id AND c.minor_id = a.column_id
left join sys.tables d on d.object_id = a.object_id
left join sys.schemas e on e.schema_id = d.schema_id
where a.object_id in (object_id(N'[{1}].[{2}]'));
use " + database, tboldname ?? tbname);
					var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
					var tbstruct = ds.ToDictionary(a => string.Concat(a[0]), a => new {
						column = string.Concat(a[0]),
						sqlType = string.Concat(a[1]),
						is_nullable = string.Concat(a[2]) == "1",
						is_identity = string.Concat(a[3]) == "1"
					}, StringComparer.CurrentCultureIgnoreCase);

					if (istmpatler == false) {
						foreach (var tbcol in tb.Columns.Values) {
							if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
								string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol)) {
								if (tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType, StringComparison.CurrentCultureIgnoreCase) == false ||
									tbcol.Attribute.IsNullable != tbstructcol.is_nullable ||
									tbcol.Attribute.IsIdentity != tbstructcol.is_identity) {
									istmpatler = true;
									break;
								}
								if (tbstructcol.column == tbcol.Attribute.OldName) {
									//修改列名
									sbalter.Append(_commonUtils.FormatSql("EXEC sp_rename {0}, {1}, 'COLUMN';\r\n", $"{tbname[0]}.{tbname[1]}.{tbname[2]}.{tbcol.Attribute.OldName}", tbcol.Attribute.Name));
								}
								continue;
							}
							//添加列
							sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}.{tbname[2]}")).Append(" ADD ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType);
							if (tbcol.Attribute.IsIdentity && tbcol.Attribute.DbType.IndexOf("identity", StringComparison.CurrentCultureIgnoreCase) == -1) sbalter.Append(" identity(1,1)");
							var addcoldbdefault = tb.Properties[tbcol.CsName].GetValue(Activator.CreateInstance(tb.Type));
							if (tbcol.Attribute.IsNullable == false) addcoldbdefault = tbcol.Attribute.DbDefautValue;
							if (addcoldbdefault != null) sbalter.Append(_commonUtils.FormatSql(" default({0})", addcoldbdefault));
							sbalter.Append(";\r\n");
						}
					}
					if (istmpatler == false) {
						sb.Append(sbalter);
						continue;
					}
					//创建临时表，数据导进临时表，然后删除原表，将临时表改名为原表名
					bool idents = false;
					var tablename = tboldname == null ? _commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}.{tbname[2]}") : _commonUtils.QuoteSqlName($"{tboldname[0]}.{tboldname[1]}.{tboldname[2]}");
					var tmptablename = _commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}.FreeSqlTmp_{tbname[2]}");
					sb.Append("BEGIN TRANSACTION\r\n")
						.Append("SET QUOTED_IDENTIFIER ON\r\n")
						.Append("SET ARITHABORT ON\r\n")
						.Append("SET NUMERIC_ROUNDABORT OFF\r\n")
						.Append("SET CONCAT_NULL_YIELDS_NULL ON\r\n")
						.Append("SET ANSI_NULLS ON\r\n")
						.Append("SET ANSI_PADDING ON\r\n")
						.Append("SET ANSI_WARNINGS ON\r\n")
						.Append("COMMIT\r\n");
					sb.Append("BEGIN TRANSACTION;\r\n");
					//创建临时表
					sb.Append("CREATE TABLE ").Append(tmptablename).Append(" (");
					foreach (var tbcol in tb.Columns.Values) {
						sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ");
						sb.Append(tbcol.Attribute.DbType);
						if (tbcol.Attribute.IsIdentity && tbcol.Attribute.DbType.IndexOf("identity", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" identity(1,1)");
						if (tbcol.Attribute.IsPrimary) sb.Append(" primary key");
						sb.Append(",");
						idents = idents || tbcol.Attribute.IsIdentity;
					}
					sb.Remove(sb.Length - 1, 1).Append("\r\n);\r\n");
					sb.Append("ALTER TABLE ").Append(tmptablename).Append(" SET (LOCK_ESCALATION = TABLE);\r\n");
					if (idents) sb.Append("SET IDENTITY_INSERT ").Append(tmptablename).Append(" ON;\r\n");
					sb.Append("IF EXISTS(SELECT 1 FROM ").Append(tablename).Append(")\r\n");
					sb.Append("\tEXEC('INSERT INTO ").Append(tmptablename).Append(" (");
					foreach (var tbcol in tb.Columns.Values) {
						if (tbstruct.ContainsKey(tbcol.Attribute.Name) ||
							string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.ContainsKey(tbcol.Attribute.OldName)) { //导入旧表存在的字段
							sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
						}
					}
					sb.Remove(sb.Length - 2, 2).Append(")\r\n\t\tSELECT ");
					foreach (var tbcol in tb.Columns.Values) {
						if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
							string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol)) {
							var insertvalue = _commonUtils.QuoteSqlName(tbstructcol.column);
							if (tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType, StringComparison.CurrentCultureIgnoreCase) == false) {
								var tbcoldbtype = tbcol.Attribute.DbType.Split(' ').First();
								insertvalue = $"cast({insertvalue} as {tbcoldbtype})";
							}
							if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable) {
								insertvalue = $"isnull({insertvalue},{_commonUtils.FormatSql("{0}", tbcol.Attribute.DbDefautValue).Replace("'", "''")})";
							}
							sb.Append(insertvalue).Append(", ");
						}
					}
					sb.Remove(sb.Length - 2, 2).Append(" FROM ").Append(tablename).Append(" WITH (HOLDLOCK TABLOCKX)');\r\n");
					if (idents) sb.Append("SET IDENTITY_INSERT ").Append(tmptablename).Append(" OFF;\r\n");
					sb.Append("DROP TABLE ").Append(tablename).Append(";\r\n");
					sb.Append("EXECUTE sp_rename N'").Append(tmptablename).Append("', N'").Append(tbname[2]).Append("', 'OBJECT' ;\r\n");
					sb.Append("COMMIT;\r\n");
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