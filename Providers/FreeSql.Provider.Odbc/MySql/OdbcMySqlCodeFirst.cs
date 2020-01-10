using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Odbc.MySql
{

    class OdbcMySqlCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {
        public override bool IsNoneCommandParameter { get => true; set => base.IsNoneCommandParameter = true; }
        public OdbcMySqlCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm, commonUtils, commonExpression) { }

        static object _dicCsToDbLock = new object();
        static Dictionary<string, (OdbcType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)> _dicCsToDb = new Dictionary<string, (OdbcType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)>() {
                { typeof(bool).FullName,  (OdbcType.Bit, "bit","bit(1) NOT NULL", null, false, false) },{ typeof(bool?).FullName,  (OdbcType.Bit, "bit","bit(1)", null, true, null) },

                { typeof(sbyte).FullName,  (OdbcType.SmallInt, "tinyint", "tinyint(3) NOT NULL", false, false, 0) },{ typeof(sbyte?).FullName,  (OdbcType.SmallInt, "tinyint", "tinyint(3)", false, true, null) },
                { typeof(short).FullName,  (OdbcType.SmallInt, "smallint","smallint(6) NOT NULL", false, false, 0) },{ typeof(short?).FullName,  (OdbcType.SmallInt, "smallint", "smallint(6)", false, true, null) },
                { typeof(int).FullName,  (OdbcType.Int, "int", "int(11) NOT NULL", false, false, 0) },{ typeof(int?).FullName,  (OdbcType.Int, "int", "int(11)", false, true, null) },
                { typeof(long).FullName,  (OdbcType.BigInt, "bigint","bigint(20) NOT NULL", false, false, 0) },{ typeof(long?).FullName,  (OdbcType.BigInt, "bigint","bigint(20)", false, true, null) },

                { typeof(byte).FullName,  (OdbcType.TinyInt, "tinyint","tinyint(3) unsigned NOT NULL", true, false, 0) },{ typeof(byte?).FullName,  (OdbcType.TinyInt, "tinyint","tinyint(3) unsigned", true, true, null) },
                { typeof(ushort).FullName,  (OdbcType.Int, "smallint","smallint(5) unsigned NOT NULL", true, false, 0) },{ typeof(ushort?).FullName,  (OdbcType.Int, "smallint", "smallint(5) unsigned", true, true, null) },
                { typeof(uint).FullName,  (OdbcType.BigInt, "int", "int(10) unsigned NOT NULL", true, false, 0) },{ typeof(uint?).FullName,  (OdbcType.BigInt, "int", "int(10) unsigned", true, true, null) },
                { typeof(ulong).FullName,  (OdbcType.Decimal, "bigint", "bigint(20) unsigned NOT NULL", true, false, 0) },{ typeof(ulong?).FullName,  (OdbcType.Decimal, "bigint", "bigint(20) unsigned", true, true, null) },

                { typeof(double).FullName,  (OdbcType.Double, "double", "double NOT NULL", false, false, 0) },{ typeof(double?).FullName,  (OdbcType.Double, "double", "double", false, true, null) },
                { typeof(float).FullName,  (OdbcType.Real, "float","float NOT NULL", false, false, 0) },{ typeof(float?).FullName,  (OdbcType.Real, "float","float", false, true, null) },
                { typeof(decimal).FullName,  (OdbcType.Decimal, "decimal", "decimal(10,2) NOT NULL", false, false, 0) },{ typeof(decimal?).FullName,  (OdbcType.Decimal, "decimal", "decimal(10,2)", false, true, null) },

                { typeof(TimeSpan).FullName,  (OdbcType.Time, "time","time NOT NULL", false, false, 0) },{ typeof(TimeSpan?).FullName,  (OdbcType.Time, "time", "time",false, true, null) },
                { typeof(DateTime).FullName,  (OdbcType.DateTime, "datetime(3)", "datetime(3) NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateTime?).FullName,  (OdbcType.DateTime, "datetime(3)", "datetime(3)", false, true, null) },

                { typeof(byte[]).FullName,  (OdbcType.VarBinary, "varbinary", "varbinary(255)", false, null, new byte[0]) },
                { typeof(string).FullName,  (OdbcType.VarChar, "varchar", "varchar(255)", false, null, "") },

                { typeof(Guid).FullName,  (OdbcType.VarChar, "char", "char(36) NOT NULL", false, false, Guid.Empty) },{ typeof(Guid?).FullName,  (OdbcType.VarChar, "char", "char(36)", false, true, null) },
            };

        public override (int type, string dbtype, string dbtypeFull, bool? isnullable, object defaultValue)? GetDbInfo(Type type)
        {
            if (_dicCsToDb.TryGetValue(type.FullName, out var trydc)) return new (int, string, string, bool?, object)?(((int)trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable, trydc.defaultValue));
            if (type.IsArray) return null;
            var enumType = type.IsEnum ? type : null;
            if (enumType == null && type.IsNullableType())
            {
                var genericTypes = type.GetGenericArguments();
                if (genericTypes.Length == 1 && genericTypes.First().IsEnum) enumType = genericTypes.First();
            }
            if (enumType != null)
            {
                var names = string.Join(",", Enum.GetNames(enumType).Select(a => _commonUtils.FormatSql("{0}", a)));
                var newItem = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any() ?
                    (OdbcType.VarChar, "set", $"set({names}){(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0)) :
                    (OdbcType.VarChar, "enum", $"enum({names}){(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0));
                if (_dicCsToDb.ContainsKey(type.FullName) == false)
                {
                    lock (_dicCsToDbLock)
                    {
                        if (_dicCsToDb.ContainsKey(type.FullName) == false)
                            _dicCsToDb.Add(type.FullName, newItem);
                    }
                }
                return ((int)newItem.Item1, newItem.Item2, newItem.Item3, newItem.Item5, newItem.Item6);
            }
            return null;
        }

        protected override string GetComparisonDDLStatements(params (Type entityType, string tableName)[] objects)
        {
            var conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5));
            var database = conn.Value.Database;
            Func<string, string, object> ExecuteScalar = (db, sql) =>
            {
                if (string.Compare(database, db) != 0) conn.Value.ChangeDatabase(db);
                try
                {
                    using (var cmd = conn.Value.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.CommandType = CommandType.Text;
                        return cmd.ExecuteScalar();
                    }
                }
                finally
                {
                    if (string.Compare(database, db) != 0) conn.Value.ChangeDatabase(database);
                }
            };
            var sb = new StringBuilder();
            try
            {
                foreach (var obj in objects)
                {
                    if (sb.Length > 0) sb.Append("\r\n");
                    var tb = _commonUtils.GetTableByEntity(obj.entityType);
                    if (tb == null) throw new Exception($"类型 {obj.entityType.FullName} 不可迁移");
                    if (tb.Columns.Any() == false) throw new Exception($"类型 {obj.entityType.FullName} 不可迁移，可迁移属性0个");
                    var tbname = _commonUtils.SplitTableName(tb.DbName);
                    if (tbname?.Length == 1) tbname = new[] { database, tbname[0] };

                    var tboldname = _commonUtils.SplitTableName(tb.DbOldName); //旧表名
                    if (tboldname?.Length == 1) tboldname = new[] { database, tboldname[0] };
                    if (string.IsNullOrEmpty(obj.tableName) == false)
                    {
                        var tbtmpname = _commonUtils.SplitTableName(obj.tableName);
                        if (tbtmpname?.Length == 1) tbtmpname = new[] { database, tbtmpname[0] };
                        if (tbname[0] != tbtmpname[0] || tbname[1] != tbtmpname[1])
                        {
                            tbname = tbtmpname;
                            tboldname = null;
                        }
                    }

                    if (string.Compare(tbname[0], database, true) != 0 && ExecuteScalar(database, _commonUtils.FormatSql(" select 1 from information_schema.schemata where schema_name={0}", tbname[0])) == null) //创建数据库
                        sb.Append($"CREATE DATABASE IF NOT EXISTS ").Append(_commonUtils.QuoteSqlName(tbname[0])).Append(" default charset utf8 COLLATE utf8_general_ci;\r\n");

                    var sbalter = new StringBuilder();
                    var istmpatler = false; //创建临时表，导入数据，删除旧表，修改
                    if (ExecuteScalar(tbname[0], _commonUtils.FormatSql(" SELECT 1 FROM information_schema.TABLES WHERE table_schema={0} and table_name={1}", tbname)) == null)
                    { //表不存在
                        if (tboldname != null)
                        {
                            if (string.Compare(tboldname[0], tbname[0], true) != 0 && ExecuteScalar(database, _commonUtils.FormatSql(" select 1 from information_schema.schemata where schema_name={0}", tboldname[0])) == null ||
                                ExecuteScalar(tboldname[0], _commonUtils.FormatSql(" SELECT 1 FROM information_schema.TABLES WHERE table_schema={0} and table_name={1}", tboldname)) == null)
                                //数据库或表不存在
                                tboldname = null;
                        }
                        if (tboldname == null)
                        {
                            //创建表
                            var createTableName = _commonUtils.QuoteSqlName(tbname[0], tbname[1]);
                            sb.Append("CREATE TABLE IF NOT EXISTS ").Append(createTableName).Append(" ( ");
                            foreach (var tbcol in tb.ColumnsByPosition)
                            {
                                sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType);
                                if (tbcol.Attribute.IsIdentity == true && tbcol.Attribute.DbType.IndexOf("AUTO_INCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" AUTO_INCREMENT");
                                if (string.IsNullOrEmpty(tbcol.Comment) == false) sb.Append(" COMMENT ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment));
                                sb.Append(",");
                            }
                            if (tb.Primarys.Any())
                            {
                                sb.Append(" \r\n  PRIMARY KEY (");
                                foreach (var tbcol in tb.Primarys) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                                sb.Remove(sb.Length - 2, 2).Append("),");
                            }
                            sb.Remove(sb.Length - 1, 1);
                            sb.Append("\r\n) Engine=InnoDB;\r\n");
                            //创建表的索引
                            foreach (var uk in tb.Indexes)
                            {
                                sb.Append("CREATE ");
                                if (uk.IsUnique) sb.Append("UNIQUE ");
                                sb.Append("INDEX ").Append(_commonUtils.QuoteSqlName(uk.Name)).Append(" ON ").Append(createTableName).Append("(");
                                foreach (var tbcol in uk.Columns)
                                {
                                    sb.Append(_commonUtils.QuoteSqlName(tbcol.Column.Attribute.Name));
                                    if (tbcol.IsDesc) sb.Append(" DESC");
                                    sb.Append(", ");
                                }
                                sb.Remove(sb.Length - 2, 2).Append(");\r\n");
                            }
                            continue;
                        }
                        //如果新表，旧表在一个数据库下，直接修改表名
                        if (string.Compare(tbname[0], tboldname[0], true) == 0)
                            sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tboldname[0], tboldname[1])).Append(" RENAME TO ").Append(_commonUtils.QuoteSqlName(tbname[0], tbname[1])).Append(";\r\n");
                        else
                        {
                            //如果新表，旧表不在一起，创建新表，导入数据，删除旧表
                            istmpatler = true;
                        }
                    }
                    else
                        tboldname = null; //如果新表已经存在，不走改表名逻辑

                    //对比字段，只可以修改类型、增加字段、有限的修改字段名；保证安全不删除字段
                    var sql = _commonUtils.FormatSql(@"
select
a.column_name,
a.column_type,
case when a.is_nullable = 'YES' then 1 else 0 end 'is_nullable',
case when locate('auto_increment', a.extra) > 0 then 1 else 0 end 'is_identity',
a.column_comment 'comment'
from information_schema.columns a
where a.table_schema in ({0}) and a.table_name in ({1})", tboldname ?? tbname);
                    var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                    var tbstruct = ds.ToDictionary(a => string.Concat(a[0]), a =>
                    {
                        var a1 = string.Concat(a[1]);
                        if (a1 == "datetime") a1 = string.Concat(a1, "(0)");
                        return new
                        {
                            column = string.Concat(a[0]),
                            sqlType = a1,
                            is_nullable = string.Concat(a[2]) == "1",
                            is_identity = string.Concat(a[3]) == "1",
                            is_unsigned = string.Concat(a[1]).EndsWith(" unsigned"),
                            comment = string.Concat(a[4])
                        };
                    }, StringComparer.CurrentCultureIgnoreCase);

                    if (istmpatler == false)
                    {
                        var existsPrimary = ExecuteScalar(tbname[0], _commonUtils.FormatSql(" select 1 from information_schema.key_column_usage where table_schema={0} and table_name={1} and constraint_name = 'PRIMARY' limit 1", tbname));
                        foreach (var tbcol in tb.ColumnsByPosition)
                        {
                            var isIdentityChanged = tbcol.Attribute.IsIdentity == true && tbcol.Attribute.DbType.IndexOf("AUTO_INCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1;
                            if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
                                string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol))
                            {
                                var isCommentChanged = tbstructcol.comment != (tbcol.Comment ?? "");
                                var isDbTypeChanged = tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType, StringComparison.CurrentCultureIgnoreCase) == false;
                                if (tbstructcol.sqlType == "datetime(0)" && Regex.IsMatch(tbcol.Attribute.DbType, @"datetime\s+\(", RegexOptions.IgnoreCase) == false)
                                    isDbTypeChanged = tbcol.Attribute.DbType.StartsWith("datetime", StringComparison.CurrentCultureIgnoreCase) == false;

                                if ((tbcol.Attribute.DbType.IndexOf(" unsigned", StringComparison.CurrentCultureIgnoreCase) != -1) != tbstructcol.is_unsigned ||
                                isDbTypeChanged ||
                                tbcol.Attribute.IsNullable != tbstructcol.is_nullable ||
                                tbcol.Attribute.IsIdentity != tbstructcol.is_identity ||
                                isCommentChanged)
                                {
                                    if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable && tbcol.Attribute.IsNullable == false && tbcol.DbDefaultValue != "NULL" && tbcol.Attribute.IsIdentity == false)
                                        sbalter.Append("UPDATE ").Append(_commonUtils.QuoteSqlName(tbname[0], tbname[1])).Append(" SET ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" = ").Append(tbcol.DbDefaultValue).Append(" WHERE ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" IS NULL;\r\n");
                                    sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tbname[0], tbname[1])).Append(" MODIFY ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" ").Append(tbcol.Attribute.DbType);
                                    if (string.IsNullOrEmpty(tbcol.Comment) == false) sbalter.Append(" COMMENT ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment ?? ""));
                                    if (tbcol.Attribute.IsIdentity == true && tbcol.Attribute.DbType.IndexOf("AUTO_INCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1) sbalter.Append(" AUTO_INCREMENT");
                                    if (tbcol.Attribute.IsIdentity == true) sbalter.Append(existsPrimary == null ? "" : ", DROP PRIMARY KEY").Append(", ADD PRIMARY KEY(").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(")");
                                    sbalter.Append(";\r\n");
                                }
                                if (string.Compare(tbstructcol.column, tbcol.Attribute.OldName, true) == 0)
                                {
                                    //修改列名
                                    sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tbname[0], tbname[1])).Append(" CHANGE COLUMN ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType);
                                    if (string.IsNullOrEmpty(tbcol.Comment) == false) sbalter.Append(" COMMENT ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment ?? ""));
                                    if (tbcol.Attribute.IsIdentity == true && tbcol.Attribute.DbType.IndexOf("AUTO_INCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1) sbalter.Append(" AUTO_INCREMENT");
                                    if (tbcol.Attribute.IsIdentity == true) sbalter.Append(existsPrimary == null ? "" : ", DROP PRIMARY KEY").Append(", ADD PRIMARY KEY(").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(")");
                                    sbalter.Append(";\r\n");
                                }
                                continue;
                            }
                            //添加列
                            sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tbname[0], tbname[1])).Append(" ADD ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType);
                            if (tbcol.Attribute.IsNullable == false && tbcol.DbDefaultValue != "NULL" && tbcol.Attribute.IsIdentity == false) sbalter.Append(" DEFAULT ").Append(tbcol.DbDefaultValue);
                            if (string.IsNullOrEmpty(tbcol.Comment) == false) sbalter.Append(" COMMENT ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment ?? ""));
                            if (tbcol.Attribute.IsIdentity == true && tbcol.Attribute.DbType.IndexOf("AUTO_INCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1) sbalter.Append(" AUTO_INCREMENT");
                            if (tbcol.Attribute.IsIdentity == true) sbalter.Append(existsPrimary == null ? "" : ", DROP PRIMARY KEY").Append(", ADD PRIMARY KEY(").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(")");
                            sbalter.Append(";\r\n");
                        }
                        var dsuksql = _commonUtils.FormatSql(@"
select 
a.column_name,
a.index_name 'index_id',
0 'IsDesc',
case when a.non_unique = 0 then 1 else 0 end 'IsUnique'
from information_schema.statistics a
where a.table_schema IN ({0}) and a.table_name IN ({1}) and a.index_name <> 'PRIMARY'", tboldname ?? tbname);
                        var dsuk = _orm.Ado.ExecuteArray(CommandType.Text, dsuksql).Select(a => new[] { string.Concat(a[0]), string.Concat(a[1]), string.Concat(a[2]), string.Concat(a[3]) });
                        foreach (var uk in tb.Indexes)
                        {
                            if (string.IsNullOrEmpty(uk.Name) || uk.Columns.Any() == false) continue;
                            var dsukfind1 = dsuk.Where(a => string.Compare(a[1], uk.Name, true) == 0).ToArray();
                            if (dsukfind1.Any() == false || dsukfind1.Length != uk.Columns.Length || dsukfind1.Where(a => (a[3] == "1") == uk.IsUnique && uk.Columns.Where(b => string.Compare(b.Column.Attribute.Name, a[0], true) == 0 && (a[2] == "1") == b.IsDesc).Any()).Count() != uk.Columns.Length)
                            {
                                if (dsukfind1.Any()) sbalter.Append("DROP INDEX ").Append(_commonUtils.QuoteSqlName(uk.Name)).Append(" ON ").Append(_commonUtils.QuoteSqlName(tbname[0], tbname[1])).Append(";\r\n");
                                sbalter.Append("CREATE ");
                                if (uk.IsUnique) sbalter.Append("UNIQUE ");
                                sbalter.Append("INDEX ").Append(_commonUtils.QuoteSqlName(uk.Name)).Append(" ON ").Append(_commonUtils.QuoteSqlName(tbname[0], tbname[1])).Append("(");
                                foreach (var tbcol in uk.Columns)
                                {
                                    sbalter.Append(_commonUtils.QuoteSqlName(tbcol.Column.Attribute.Name));
                                    if (tbcol.IsDesc) sbalter.Append(" DESC");
                                    sbalter.Append(", ");
                                }
                                sbalter.Remove(sbalter.Length - 2, 2).Append(");\r\n");
                            }
                        }
                    }
                    if (istmpatler == false)
                    {
                        sb.Append(sbalter);
                        continue;
                    }

                    //创建临时表，数据导进临时表，然后删除原表，将临时表改名为原表名
                    var tablename = tboldname == null ? _commonUtils.QuoteSqlName(tbname[0], tbname[1]) : _commonUtils.QuoteSqlName(tboldname[0], tboldname[1]);
                    var tmptablename = _commonUtils.QuoteSqlName(tbname[0], $"FreeSqlTmp_{tbname[1]}");
                    //创建临时表
                    sb.Append("CREATE TABLE IF NOT EXISTS ").Append(tmptablename).Append(" ( ");
                    foreach (var tbcol in tb.ColumnsByPosition)
                    {
                        sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType);
                        if (tbcol.Attribute.IsIdentity == true && tbcol.Attribute.DbType.IndexOf("AUTO_INCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" AUTO_INCREMENT");
                        if (string.IsNullOrEmpty(tbcol.Comment) == false) sb.Append(" COMMENT ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment));
                        sb.Append(",");
                    }
                    if (tb.Primarys.Any())
                    {
                        sb.Append(" \r\n  PRIMARY KEY (");
                        foreach (var tbcol in tb.Primarys) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                        sb.Remove(sb.Length - 2, 2).Append("),");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("\r\n) Engine=InnoDB;\r\n");
                    sb.Append("INSERT INTO ").Append(tmptablename).Append(" (");
                    foreach (var tbcol in tb.ColumnsByPosition)
                        sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                    sb.Remove(sb.Length - 2, 2).Append(")\r\nSELECT ");
                    foreach (var tbcol in tb.ColumnsByPosition)
                    {
                        var insertvalue = "NULL";
                        if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
                            string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol))
                        {
                            insertvalue = _commonUtils.QuoteSqlName(tbstructcol.column);
                            if (tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType, StringComparison.CurrentCultureIgnoreCase) == false)
                            {
                                //insertvalue = $"cast({insertvalue} as {tbcol.Attribute.DbType.Split(' ').First()})";
                            }
                            if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable)
                                insertvalue = $"ifnull({insertvalue},{tbcol.DbDefaultValue})";
                        }
                        else if (tbcol.Attribute.IsNullable == false)
                            if (tbcol.DbDefaultValue != "NULL" && tbcol.Attribute.IsIdentity == false)
                                insertvalue = tbcol.DbDefaultValue;
                        sb.Append(insertvalue).Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2).Append(" FROM ").Append(tablename).Append(";\r\n");
                    sb.Append("DROP TABLE ").Append(tablename).Append(";\r\n");
                    sb.Append("ALTER TABLE ").Append(tmptablename).Append(" RENAME TO ").Append(_commonUtils.QuoteSqlName(tbname[0], tbname[1])).Append(";\r\n");
                    //创建表的索引
                    foreach (var uk in tb.Indexes)
                    {
                        sb.Append("CREATE ");
                        if (uk.IsUnique) sb.Append("UNIQUE ");
                        sb.Append("INDEX ").Append(_commonUtils.QuoteSqlName(uk.Name)).Append(" ON ").Append(tablename).Append("(");
                        foreach (var tbcol in uk.Columns)
                        {
                            sb.Append(_commonUtils.QuoteSqlName(tbcol.Column.Attribute.Name));
                            if (tbcol.IsDesc) sb.Append(" DESC");
                            sb.Append(", ");
                        }
                        sb.Remove(sb.Length - 2, 2).Append(");\r\n");
                    }
                }
                return sb.Length == 0 ? null : sb.ToString();
            }
            finally
            {
                try
                {
                    conn.Value.ChangeDatabase(database);
                    _orm.Ado.MasterPool.Return(conn);
                }
                catch
                {
                    _orm.Ado.MasterPool.Return(conn, true);
                }
            }
        }

        public override int ExecuteDDLStatements(string ddl)
        {
            if (string.IsNullOrEmpty(ddl)) return 0;
            var scripts = ddl.Split(new string[] { ";\r\n" }, StringSplitOptions.None).Where(a => string.IsNullOrEmpty(a.Trim()) == false).ToArray();

            if (scripts.Any() == false) return 0;
            if (scripts.Length == 1) return base.ExecuteDDLStatements(ddl);

            var affrows = 0;
            foreach (var script in scripts)
                affrows += base.ExecuteDDLStatements(script);
            return affrows;
        }
    }
}