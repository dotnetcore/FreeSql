using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
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

		static readonly Dictionary<string, (SqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable)> _dicCsToDb = new Dictionary<string, (SqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable)>() {
				{ "System.Boolean",  (SqlDbType.Bit, "bit","bit NOT NULL", null, false) },{ "System.Nullable`1[System.Boolean]",  (SqlDbType.Bit, "bit","bit", null, true) },

				{ "System.SByte",  (SqlDbType.TinyInt, "tinyint", "tinyint NOT NULL", false, false) },{ "System.Nullable`1[System.SByte]",  (SqlDbType.TinyInt, "tinyint", "tinyint", false, true) },
				{ "System.Int16",  (SqlDbType.SmallInt, "smallint","smallint NOT NULL", false, false) },{ "System.Nullable`1[System.Int16]",  (SqlDbType.SmallInt, "smallint", "smallint", false, true) },
				{ "System.Int32",  (SqlDbType.Int, "int", "int NOT NULL", false, false) },{ "System.Nullable`1[System.Int32]",  (SqlDbType.Int, "int", "int", false, true) },
				{ "System.Int64",  (SqlDbType.BigInt, "bigint","bigint NOT NULL", false, false) },{ "System.Nullable`1[System.Int64]",  (SqlDbType.BigInt, "bigint","bigint", false, true) },

				{ "System.Byte",  (SqlDbType.TinyInt, "tinyint","tinyint NOT NULL", true, false) },{ "System.Nullable`1[System.Byte]",  (SqlDbType.TinyInt, "tinyint","tinyint", true, true) },
				{ "System.UInt16",  (SqlDbType.SmallInt, "smallint","smallint NOT NULL", true, false) },{ "System.Nullable`1[System.UInt16]",  (SqlDbType.SmallInt, "smallint", "smallint", true, true) },
				{ "System.UInt32",  (SqlDbType.Int, "int", "int NOT NULL", true, false) },{ "System.Nullable`1[System.UInt32]",  (SqlDbType.Int, "int", "int", true, true) },
				{ "System.UInt64",  (SqlDbType.BigInt, "bigint", "bigint NOT NULL", true, false) },{ "System.Nullable`1[System.UInt64]",  (SqlDbType.BigInt, "bigint", "bigint", true, true) },

				{ "System.Double",  (SqlDbType.Float, "double", "double NOT NULL", false, false) },{ "System.Nullable`1[System.Double]",  (SqlDbType.Float, "double", "double", false, true) },
				{ "System.Single",  (SqlDbType.Real, "float","float NOT NULL", false, false) },{ "System.Nullable`1[System.Single]",  (SqlDbType.Real, "float","float", false, true) },
				{ "System.Decimal",  (SqlDbType.Decimal, "decimal", "decimal(10,2) NOT NULL", false, false) },{ "System.Nullable`1[System.Decimal]",  (SqlDbType.Decimal, "decimal", "decimal(10,2)", false, true) },

				{ "System.TimeSpan",  (SqlDbType.Time, "time","time NOT NULL", false, false) },{ "System.Nullable`1[System.TimeSpan]",  (SqlDbType.Time, "time", "time",false, true) },
				{ "System.DateTime",  (SqlDbType.DateTime, "datetime", "datetime NOT NULL", false, false) },{ "System.Nullable`1[System.DateTime]",  (SqlDbType.DateTime, "datetime", "datetime", false, true) },
				{ "System.DateTimeOffset",  (SqlDbType.DateTimeOffset, "datetimeoffset", "datetimeoffset NOT NULL", false, false) },{ "System.Nullable`1[System.DateTime]",  (SqlDbType.DateTimeOffset, "datetimeoffset", "datetimeoffset", false, true) },

				{ "System.Byte[]",  (SqlDbType.VarBinary, "varbinary", "varbinary(255)", false, null) },
				{ "System.String",  (SqlDbType.NVarChar, "nvarchar", "nvarchar(255)", false, null) },

				{ "System.Guid",  (SqlDbType.UniqueIdentifier, "uniqueidentifier", "uniqueidentifier", false, false) },{ "System.Guid",  (SqlDbType.UniqueIdentifier, "uniqueidentifier", "uniqueidentifier", false, true) },
			};

		public (int type, string dbtype, string dbtypeFull, bool? isnullable)? GetDbInfo(Type type) {
			var enumType = type.IsEnum ? type : null;
			if (enumType == null && type.FullName.StartsWith("System.Nullable`1[") && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments.First().IsEnum) enumType = type.GenericTypeArguments.First();
			if (enumType != null) {
				return ((int)SqlDbType.Int, "int", "int", type.IsEnum ? false : true);
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
				if (tboldname?.Length == 1) tboldname = new[] { "dbo", tboldname[0] };

				var isRenameTable = false;
				var tbname = tb.DbName.Split(new[] { '.' }, 2);
				if (tbname.Length == 1) tbname = new[] { "dbo", tbname[0] };
				if (_orm.Ado.ExecuteScalar(CommandType.Text, "select 1 from dbo.sysobjects where id = object_id(N'[{0}].[{1}]') and OBJECTPROPERTY(id, N'IsUserTable')".FormatMySql(tbname)) == null) { //表不存在

					if (tboldname != null && _orm.Ado.ExecuteScalar(CommandType.Text, "select 1 from dbo.sysobjects where id = object_id(N'[{0}].[{1}]') and OBJECTPROPERTY(id, N'IsUserTable')".FormatMySql(tboldname)) != null) { //旧表存在
																																																									//修改表名
						sb.Append(_commonUtils.FormatSql("EXEC sp_rename {0}, {1} GO \r\n", _commonUtils.QuoteSqlName($"{tboldname[0]}.{tboldname[1]}"), _commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")));
						isRenameTable = true;

					} else {
						//创建表
						sb.Append("CREATE TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" (");
						foreach (var tbcol in tb.Columns.Values) {
							sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ");
							sb.Append(tbcol.Attribute.DbType.ToUpper());
							if (tbcol.Attribute.IsIdentity && tbcol.Attribute.DbType.IndexOf("identity", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" identity(1,1)");
							if (tbcol.Attribute.IsPrimary) sb.Append(" primary key");
							sb.Append(",");
						}
						sb.Remove(sb.Length - 1, 1).Append("\r\n) GO \r\n");
						continue;
					}
				}
				//对比字段，只可以修改类型、增加字段、有限的修改字段名；保证安全不删除字段
				var addcols = new Dictionary<string, ColumnInfo>(StringComparer.CurrentCultureIgnoreCase);
				foreach (var tbcol in tb.Columns) addcols.Add(tbcol.Value.Attribute.Name, tbcol.Value);
				var surplus = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
				var dbcols = new List<DbColumnInfo>();
				var sql = @"select
a.name 'Column'
,b.name + case 
 when b.name in ('Char', 'VarChar', 'NChar', 'NVarChar', 'Binary', 'VarBinary') then '(' + 
  case when a.max_length = -1 then 'MAX' 
  when b.name in ('NChar', 'NVarchar') then cast(a.max_length / 2 as varchar)
  else cast(a.max_length as varchar) end + ')'
 when b.name in ('Numeric', 'Decimal') then '(' + cast(a.precision as varchar) + ',' + cast(a.scale as varchar) + ')'
 else '' end as 'SqlType'
,a.is_nullable 'IsNullable'
,a.is_identity 'IsIdentity'
from sys.columns a
inner join sys.types b on b.user_type_id = a.user_type_id
left join sys.extended_properties AS c ON c.major_id = a.object_id AND c.minor_id = a.column_id
left join sys.tables d on d.object_id = a.object_id
left join sys.schemas e on e.schema_id = d.schema_id
where a.object_id in (object_id(N'[{0}].[{1}]'))".FormatMySql(isRenameTable ? tboldname : tbname);
				var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
				foreach (var row in ds) {
					string column = string.Concat(row[0]);
					string sqlType = string.Concat(row[1]).ToLower();
					bool is_nullable = string.Concat(row[2]) == "1";
					bool is_identity = string.Concat(row[3]) == "1";

					if (addcols.TryGetValue(column, out var trycol)) {
						if (Regex.Replace(trycol.Attribute.DbType, @"\([^\)]+\)", m => Regex.Replace(m.Groups[0].Value, @"\s", "")).StartsWith(sqlType, StringComparison.CurrentCultureIgnoreCase) == false ||
							(trycol.Attribute.DbType.IndexOf("NOT NULL", StringComparison.CurrentCultureIgnoreCase) == -1) != is_nullable ||
							trycol.Attribute.IsIdentity != is_identity) {
							sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ALTER COLUMN ").Append(_commonUtils.QuoteSqlName(column)).Append(" ").Append(trycol.Attribute.DbType.ToUpper());
							if (trycol.Attribute.IsIdentity && trycol.Attribute.DbType.IndexOf("identity", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" identity(1,1)");
							sb.Append(" GO \r\n");
						}
						addcols.Remove(column);
					} else
						surplus.Add(column, true); //记录剩余字段
				}
				foreach (var addcol in addcols.Values) {
					if (string.IsNullOrEmpty(addcol.Attribute.OldName) == false && surplus.ContainsKey(addcol.Attribute.OldName)) { //修改列名
						sb.Append(_commonUtils.FormatSql("EXEC sp_rename {0}, {1}, 'COLUMN' GO \r\n", _commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}.{addcol.Attribute.OldName}"), _commonUtils.QuoteSqlName(addcol.Attribute.Name)));
						sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ALTER COLUMN ").Append(_commonUtils.QuoteSqlName(addcol.Attribute.Name)).Append(" ").Append(addcol.Attribute.DbType.ToUpper());
						if (addcol.Attribute.IsIdentity && addcol.Attribute.DbType.IndexOf("identity", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" identity(1,1)");
						sb.Append(" GO \r\n");

					} else { //添加列
						sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ADD ").Append(_commonUtils.QuoteSqlName(addcol.Attribute.Name)).Append(" ").Append(addcol.Attribute.DbType.ToUpper());
						if (addcol.Attribute.IsIdentity && addcol.Attribute.DbType.IndexOf("identity", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" identity(1,1)");
						sb.Append(" GO \r\n");
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