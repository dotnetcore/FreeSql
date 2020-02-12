using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FreeSql.SqlServer
{

    class SqlServerCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {

        public SqlServerCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm, commonUtils, commonExpression) { }

        static object _dicCsToDbLock = new object();
        static Dictionary<string, (SqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)> _dicCsToDb = new Dictionary<string, (SqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)>() {
                { typeof(bool).FullName,  (SqlDbType.Bit, "bit","bit NOT NULL", null, false, false) },{ typeof(bool?).FullName,  (SqlDbType.Bit, "bit","bit", null, true, null) },

                { typeof(sbyte).FullName,  (SqlDbType.SmallInt, "smallint", "smallint NOT NULL", false, false, 0) },{ typeof(sbyte?).FullName,  (SqlDbType.SmallInt, "smallint", "smallint", false, true, null) },
                { typeof(short).FullName,  (SqlDbType.SmallInt, "smallint","smallint NOT NULL", false, false, 0) },{ typeof(short?).FullName,  (SqlDbType.SmallInt, "smallint", "smallint", false, true, null) },
                { typeof(int).FullName,  (SqlDbType.Int, "int", "int NOT NULL", false, false, 0) },{ typeof(int?).FullName,  (SqlDbType.Int, "int", "int", false, true, null) },
                { typeof(long).FullName,  (SqlDbType.BigInt, "bigint","bigint NOT NULL", false, false, 0) },{ typeof(long?).FullName,  (SqlDbType.BigInt, "bigint","bigint", false, true, null) },

                { typeof(byte).FullName,  (SqlDbType.TinyInt, "tinyint","tinyint NOT NULL", true, false, 0) },{ typeof(byte?).FullName,  (SqlDbType.TinyInt, "tinyint","tinyint", true, true, null) },
                { typeof(ushort).FullName,  (SqlDbType.Int, "int","int NOT NULL", true, false, 0) },{ typeof(ushort?).FullName,  (SqlDbType.Int, "int", "int", true, true, null) },
                { typeof(uint).FullName,  (SqlDbType.BigInt, "bigint", "bigint NOT NULL", true, false, 0) },{ typeof(uint?).FullName,  (SqlDbType.BigInt, "bigint", "bigint", true, true, null) },
                { typeof(ulong).FullName,  (SqlDbType.Decimal, "decimal", "decimal(20,0) NOT NULL", true, false, 0) },{ typeof(ulong?).FullName,  (SqlDbType.Decimal, "decimal", "decimal(20,0)", true, true, null) },

                { typeof(double).FullName,  (SqlDbType.Float, "float", "float NOT NULL", false, false, 0) },{ typeof(double?).FullName,  (SqlDbType.Float, "float", "float", false, true, null) },
                { typeof(float).FullName,  (SqlDbType.Real, "real","real NOT NULL", false, false, 0) },{ typeof(float?).FullName,  (SqlDbType.Real, "real","real", false, true, null) },
                { typeof(decimal).FullName,  (SqlDbType.Decimal, "decimal", "decimal(10,2) NOT NULL", false, false, 0) },{ typeof(decimal?).FullName,  (SqlDbType.Decimal, "decimal", "decimal(10,2)", false, true, null) },

                { typeof(TimeSpan).FullName,  (SqlDbType.Time, "time","time NOT NULL", false, false, 0) },{ typeof(TimeSpan?).FullName,  (SqlDbType.Time, "time", "time",false, true, null) },
                { typeof(DateTime).FullName,  (SqlDbType.DateTime, "datetime", "datetime NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateTime?).FullName,  (SqlDbType.DateTime, "datetime", "datetime", false, true, null) },
                { typeof(DateTimeOffset).FullName,  (SqlDbType.DateTimeOffset, "datetimeoffset", "datetimeoffset NOT NULL", false, false, new DateTimeOffset(new DateTime(1970,1,1), TimeSpan.Zero)) },{ typeof(DateTimeOffset?).FullName,  (SqlDbType.DateTimeOffset, "datetimeoffset", "datetimeoffset", false, true, null) },

                { typeof(byte[]).FullName,  (SqlDbType.VarBinary, "varbinary", "varbinary(255)", false, null, new byte[0]) },
                { typeof(string).FullName,  (SqlDbType.NVarChar, "nvarchar", "nvarchar(255)", false, null, "") },

                { typeof(Guid).FullName,  (SqlDbType.UniqueIdentifier, "uniqueidentifier", "uniqueidentifier NOT NULL", false, false, Guid.Empty) },{ typeof(Guid?).FullName,  (SqlDbType.UniqueIdentifier, "uniqueidentifier", "uniqueidentifier", false, true, null) },
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
                var newItem = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any() ?
                    (SqlDbType.BigInt, "bigint", $"bigint{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0)) :
                    (SqlDbType.Int, "int", $"int{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0));
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

        void AddOrUpdateMS_Description(StringBuilder sb, string schema, string table, string column, string comment)
        {
            if (string.IsNullOrEmpty(comment))
            {
                sb.AppendFormat(@"
IF ((SELECT COUNT(1) from fn_listextendedproperty('MS_Description', 
  'SCHEMA', N'{0}', 
  'TABLE', N'{1}', 
  'COLUMN', N'{2}')) > 0) 
  EXEC sp_dropextendedproperty @name = N'MS_Description'
    , @level0type = 'SCHEMA', @level0name = N'{0}'
    , @level1type = 'TABLE', @level1name = N'{1}'
    , @level2type = 'COLUMN', @level2name = N'{2}'
", schema.Replace("'", "''"), table.Replace("'", "''"), column.Replace("'", "''"));
                return;
            }
            sb.AppendFormat(@"
IF ((SELECT COUNT(1) from fn_listextendedproperty('MS_Description', 
  'SCHEMA', N'{0}', 
  'TABLE', N'{1}', 
  'COLUMN', N'{2}')) > 0) 
  EXEC sp_updateextendedproperty @name = N'MS_Description', @value = N'{3}'
    , @level0type = 'SCHEMA', @level0name = N'{0}'
    , @level1type = 'TABLE', @level1name = N'{1}'
    , @level2type = 'COLUMN', @level2name = N'{2}'
ELSE
  EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'{3}'
    , @level0type = 'SCHEMA', @level0name = N'{0}'
    , @level1type = 'TABLE', @level1name = N'{1}'
    , @level2type = 'COLUMN', @level2name = N'{2}'
", schema.Replace("'", "''"), table.Replace("'", "''"), column.Replace("'", "''"), comment?.Replace("'", "''") ?? "");
        }
        protected override string GetComparisonDDLStatements(params (Type entityType, string tableName)[] objects)
        {
            var conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5));
            string database = null;
            try
            {
                database = conn.Value.Database;
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

                foreach (var obj in objects)
                {
                    if (sb.Length > 0) sb.Append("\r\n");
                    var tb = _commonUtils.GetTableByEntity(obj.entityType);
                    if (tb == null) throw new Exception($"类型 {obj.entityType.FullName} 不可迁移");
                    if (tb.Columns.Any() == false) throw new Exception($"类型 {obj.entityType.FullName} 不可迁移，可迁移属性0个");
                    var tbname = _commonUtils.SplitTableName(tb.DbName);
                    if (tbname?.Length == 1) tbname = new[] { database, "dbo", tbname[0] };
                    if (tbname?.Length == 2) tbname = new[] { database, tbname[0], tbname[1] };

                    var tboldname = _commonUtils.SplitTableName(tb.DbOldName); //旧表名
                    if (tboldname?.Length == 1) tboldname = new[] { database, "dbo", tboldname[0] };
                    if (tboldname?.Length == 2) tboldname = new[] { database, tboldname[0], tboldname[1] };
                    if (string.IsNullOrEmpty(obj.tableName) == false)
                    {
                        var tbtmpname = _commonUtils.SplitTableName(obj.tableName);
                        if (tbtmpname?.Length == 1) tbtmpname = new[] { database, "dbo", tbtmpname[0] };
                        if (tbtmpname?.Length == 2) tbtmpname = new[] { database, tbtmpname[0], tbtmpname[1] };
                        if (tbname[0] != tbtmpname[0] || tbname[1] != tbtmpname[1] || tbname[2] != tbtmpname[2])
                        {
                            tbname = tbtmpname;
                            tboldname = null;
                        }
                    }
                    //codefirst 不支持表名、模式名、数据库名中带 .

                    if (string.Compare(tbname[0], database, true) != 0 && ExecuteScalar(database, $" select 1 from sys.databases where name='{tbname[0]}'") == null) //创建数据库
                        ExecuteScalar(database, $"if not exists(select 1 from sys.databases where name='{tbname[0]}')\r\n\tcreate database [{tbname[0]}];");
                    if (string.Compare(tbname[1], "dbo", true) != 0 && ExecuteScalar(tbname[0], $" select 1 from sys.schemas where name='{tbname[1]}'") == null) //创建模式
                        ExecuteScalar(tbname[0], $"create schema [{tbname[1]}] authorization [dbo]");

                    var sbalter = new StringBuilder();
                    var istmpatler = false; //创建临时表，导入数据，删除旧表，修改
                    if (ExecuteScalar(tbname[0], $" select 1 from dbo.sysobjects where id = object_id(N'[{tbname[1]}].[{tbname[2]}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1") == null)
                    { //表不存在
                        if (tboldname != null)
                        {
                            if (string.Compare(tboldname[0], tbname[0], true) != 0 && ExecuteScalar(database, $" select 1 from sys.databases where name='{tboldname[0]}'") == null ||
                                string.Compare(tboldname[1], tbname[1], true) != 0 && ExecuteScalar(tboldname[0], $" select 1 from sys.schemas where name='{tboldname[1]}'") == null ||
                                ExecuteScalar(tboldname[0], $" select 1 from dbo.sysobjects where id = object_id(N'[{tboldname[1]}].[{tboldname[2]}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1") == null)
                                //数据库或模式或表不存在
                                tboldname = null;
                        }
                        if (tboldname == null)
                        {
                            //创建表
                            var createTableName = _commonUtils.QuoteSqlName($"{tbname[1]}.{tbname[2]}");
                            sb.Append("use ").Append(_commonUtils.QuoteSqlName(tbname[0])).Append(";\r\nCREATE TABLE ").Append(createTableName).Append(" ( ");
                            var pkidx = 0;
                            foreach (var tbcol in tb.ColumnsByPosition)
                            {
                                sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType);
                                if (tbcol.Attribute.IsIdentity == true && tbcol.Attribute.DbType.IndexOf("identity", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" identity(1,1)");
                                if (tbcol.Attribute.IsPrimary == true)
                                {
                                    if (tb.Primarys.Length > 1)
                                    {
                                        if (pkidx == tb.Primarys.Length - 1)
                                            sb.Append(" primary key (").Append(string.Join(", ", tb.Primarys.Select(a => _commonUtils.QuoteSqlName(a.Attribute.Name)))).Append(")");
                                    }
                                    else
                                        sb.Append(" primary key");
                                    pkidx++;
                                }
                                sb.Append(",");
                            }
                            sb.Remove(sb.Length - 1, 1).Append("\r\n);\r\n");
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
                            //备注
                            foreach (var tbcol in tb.ColumnsByPosition)
                            {
                                if (string.IsNullOrEmpty(tbcol.Comment) == false)
                                    AddOrUpdateMS_Description(sb, tbname[1], tbname[2], tbcol.Attribute.Name, tbcol.Comment);
                            }
                            continue;
                        }
                        //如果新表，旧表在一个数据库和模式下，直接修改表名
                        if (string.Compare(tbname[0], tboldname[0], true) == 0 &&
                            string.Compare(tbname[1], tboldname[1], true) == 0)
                            sbalter.Append("use ").Append(_commonUtils.QuoteSqlName(tbname[0])).Append(_commonUtils.FormatSql(";\r\nEXEC sp_rename {0}, {1};\r\n", _commonUtils.QuoteSqlName($"{tboldname[0]}.{tboldname[1]}.{tboldname[2]}"), tbname[2]));
                        else
                        {
                            //如果新表，旧表不在一起，创建新表，导入数据，删除旧表
                            istmpatler = true;
                        }
                    }
                    else
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
,(select value from sys.extended_properties where major_id = a.object_id AND minor_id = a.column_id AND name = 'MS_Description') 'Comment'
from sys.columns a
inner join sys.types b on b.user_type_id = a.user_type_id
left join sys.tables d on d.object_id = a.object_id
left join sys.schemas e on e.schema_id = d.schema_id
where a.object_id in (object_id(N'[{1}].[{2}]'));
use " + database, tboldname ?? tbname);
                    var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                    var tbstruct = ds.ToDictionary(a => string.Concat(a[0]), a => new
                    {
                        column = string.Concat(a[0]),
                        sqlType = string.Concat(a[1]),
                        is_nullable = string.Concat(a[2]) == "1",
                        is_identity = string.Concat(a[3]) == "1",
                        comment = string.Concat(a[4])
                    }, StringComparer.CurrentCultureIgnoreCase);

                    if (istmpatler == false)
                    {
                        foreach (var tbcol in tb.ColumnsByPosition)
                        {
                            if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
                                string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol))
                            {
                                var isCommentChanged = tbstructcol.comment != (tbcol.Comment ?? "");
                                if (tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType, StringComparison.CurrentCultureIgnoreCase) == false ||
                                    tbcol.Attribute.IsNullable != tbstructcol.is_nullable ||
                                    tbcol.Attribute.IsIdentity != tbstructcol.is_identity)
                                {
                                    istmpatler = true;
                                    break;
                                }
                                if (string.Compare(tbstructcol.column, tbcol.Attribute.OldName, true) == 0)
                                    //修改列名
                                    sbalter.Append(_commonUtils.FormatSql("EXEC sp_rename {0}, {1}, 'COLUMN';\r\n", $"{tbname[0]}.{tbname[1]}.{tbname[2]}.{tbstructcol.column}", tbcol.Attribute.Name));
                                if (isCommentChanged)
                                    //修改备备注
                                    AddOrUpdateMS_Description(sbalter, tbname[1], tbname[2], tbcol.Attribute.Name, tbcol.Comment);
                                continue;
                            }
                            //添加列
                            sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}.{tbname[2]}")).Append(" ADD ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType);
                            if (tbcol.Attribute.IsIdentity == true && tbcol.Attribute.DbType.IndexOf("identity", StringComparison.CurrentCultureIgnoreCase) == -1) sbalter.Append(" identity(1,1)");
                            if (tbcol.Attribute.IsNullable == false && tbcol.DbDefaultValue != "NULL" && tbcol.Attribute.IsIdentity == false) sbalter.Append(" default(").Append(GetTransferDbDefaultValue(tbcol)).Append(")");
                            sbalter.Append(";\r\n");
                            if (string.IsNullOrEmpty(tbcol.Comment) == false) AddOrUpdateMS_Description(sbalter, tbname[1], tbname[2], tbcol.Attribute.Name, tbcol.Comment);
                        }
                        var dsuksql = string.Format(@"
use [{0}];
select 
c.name
,b.name
,case when a.is_descending_key = 1 then 1 else 0 end
,case when b.is_unique = 1 then 1 else 0 end
from sys.index_columns a
inner join sys.indexes b on b.object_id = a.object_id and b.index_id = a.index_id
left join sys.columns c on c.object_id = a.object_id and c.column_id = a.column_id
where a.object_id in (object_id(N'[{1}].[{2}]')) and b.is_primary_key = 0;
use " + database, tboldname ?? tbname);
                        var dsuk = _orm.Ado.ExecuteArray(CommandType.Text, dsuksql).Select(a => new[] { string.Concat(a[0]), string.Concat(a[1]), string.Concat(a[2]), string.Concat(a[3]) });
                        foreach (var uk in tb.Indexes)
                        {
                            if (string.IsNullOrEmpty(uk.Name) || uk.Columns.Any() == false) continue;
                            var dsukfind1 = dsuk.Where(a => string.Compare(a[1], uk.Name, true) == 0).ToArray();
                            if (dsukfind1.Any() == false || dsukfind1.Length != uk.Columns.Length || dsukfind1.Where(a => (a[3] == "1") == uk.IsUnique && uk.Columns.Where(b => string.Compare(b.Column.Attribute.Name, a[0], true) == 0 && (a[2] == "1") == b.IsDesc).Any()).Count() != uk.Columns.Length)
                            {
                                if (dsukfind1.Any()) sbalter.Append("DROP INDEX ").Append(_commonUtils.QuoteSqlName(uk.Name)).Append(" ON ").Append(_commonUtils.QuoteSqlName($"{tbname[1]}.{tbname[2]}")).Append(";\r\n");
                                sbalter.Append("CREATE ");
                                if (uk.IsUnique) sbalter.Append("UNIQUE ");
                                sbalter.Append("INDEX ").Append(_commonUtils.QuoteSqlName(uk.Name)).Append(" ON ").Append(_commonUtils.QuoteSqlName($"{tbname[1]}.{tbname[2]}")).Append("(");
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
                        if (sbalter.Length > 0)
                            sb.Append(sbalter).Append("\r\nuse " + database);
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
                    sb.Append("CREATE TABLE ").Append(tmptablename).Append(" ( ");
                    var pkidx2 = 0;
                    foreach (var tbcol in tb.ColumnsByPosition)
                    {
                        sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType);
                        if (tbcol.Attribute.IsIdentity == true && tbcol.Attribute.DbType.IndexOf("identity", StringComparison.CurrentCultureIgnoreCase) == -1) sb.Append(" identity(1,1)");
                        if (tbcol.Attribute.IsPrimary == true)
                        {
                            if (tb.Primarys.Length > 1)
                            {
                                if (pkidx2 == tb.Primarys.Length - 1)
                                    sb.Append(" primary key (").Append(string.Join(", ", tb.Primarys.Select(a => _commonUtils.QuoteSqlName(a.Attribute.Name)))).Append(")");
                            }
                            else
                                sb.Append(" primary key");
                            pkidx2++;
                        }
                        sb.Append(",");
                        idents = idents || tbcol.Attribute.IsIdentity == true;
                    }
                    sb.Remove(sb.Length - 1, 1).Append("\r\n);\r\n");
                    //备注
                    foreach (var tbcol in tb.ColumnsByPosition)
                    {
                        if (string.IsNullOrEmpty(tbcol.Comment) == false)
                            AddOrUpdateMS_Description(sb, tbname[1], $"FreeSqlTmp_{tbname[2]}", tbcol.Attribute.Name, tbcol.Comment);
                    }
                    if ((_commonUtils as SqlServerUtils).ServerVersion > 9) //SqlServer 2008+
                        sb.Append("ALTER TABLE ").Append(tmptablename).Append(" SET (LOCK_ESCALATION = TABLE);\r\n");
                    if (idents) sb.Append("SET IDENTITY_INSERT ").Append(tmptablename).Append(" ON;\r\n");
                    sb.Append("IF EXISTS(SELECT 1 FROM ").Append(tablename).Append(")\r\n");
                    sb.Append("\tEXEC('INSERT INTO ").Append(tmptablename).Append(" (");
                    foreach (var tbcol in tb.ColumnsByPosition)
                        sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                    sb.Remove(sb.Length - 2, 2).Append(")\r\n\t\tSELECT ");
                    foreach (var tbcol in tb.ColumnsByPosition)
                    {
                        var insertvalue = "NULL";
                        if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
                            string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol))
                        {
                            insertvalue = _commonUtils.QuoteSqlName(tbstructcol.column);
                            if (tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType, StringComparison.CurrentCultureIgnoreCase) == false)
                                insertvalue = $"cast({insertvalue} as {tbcol.Attribute.DbType.Split(' ').First()})";
                            if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable)
                                insertvalue = $"isnull({insertvalue},{GetTransferDbDefaultValue(tbcol)})";
                        }
                        else if (tbcol.Attribute.IsNullable == false)
                            if (tbcol.DbDefaultValue != "NULL" && tbcol.Attribute.IsIdentity == false)
                                insertvalue = GetTransferDbDefaultValue(tbcol);
                        sb.Append(insertvalue.Replace("'", "''")).Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2).Append(" FROM ").Append(tablename).Append(" WITH (HOLDLOCK TABLOCKX)');\r\n");
                    if (idents) sb.Append("SET IDENTITY_INSERT ").Append(tmptablename).Append(" OFF;\r\n");
                    sb.Append("DROP TABLE ").Append(tablename).Append(";\r\n");
                    sb.Append("EXECUTE sp_rename N'").Append(tmptablename).Append("', N'").Append(tbname[2]).Append("', 'OBJECT';\r\n");
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
                    sb.Append("COMMIT;\r\n");
                }
                return sb.Length == 0 ? null : sb.ToString();
            }
            finally
            {
                try
                {
                    if (string.IsNullOrEmpty(database) == false)
                        conn.Value.ChangeDatabase(database);
                    _orm.Ado.MasterPool.Return(conn);
                }
                catch
                {
                    _orm.Ado.MasterPool.Return(conn, true);
                }
            }
        }
        string GetTransferDbDefaultValue(ColumnInfo col)
        {
            var ddv = col.DbDefaultValue;
            if (string.IsNullOrEmpty(ddv) || ddv == "NULL") return ddv;
            if (col.Attribute.MapType.NullableTypeOrThis() == typeof(DateTime) && DateTime.TryParse(ddv, out var trydt))
            {
                if (col.Attribute.DbType.Contains("SMALLDATETIME") && trydt < new DateTime(1900, 1, 1)) ddv = _commonUtils.FormatSql("{0}", new DateTime(1900, 1, 1));
                else if (col.Attribute.DbType.Contains("DATETIME") && trydt < new DateTime(1753, 1, 1)) ddv = _commonUtils.FormatSql("{0}", new DateTime(1753, 1, 1));
                else if (col.Attribute.DbType.Contains("DATE") && trydt < new DateTime(0001, 1, 1)) ddv = _commonUtils.FormatSql("{0}", new DateTime(0001, 1, 1));
            }
            return ddv;
        }
    }
}