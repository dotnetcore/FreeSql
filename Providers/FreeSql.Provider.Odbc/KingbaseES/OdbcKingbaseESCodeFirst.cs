using FreeSql.DataAnnotations;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Odbc.KingbaseES
{

    class OdbcKingbaseESCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {
        public override bool IsNoneCommandParameter { get => true; set => base.IsNoneCommandParameter = true; }
        public OdbcKingbaseESCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm, commonUtils, commonExpression) { }

        static object _dicCsToDbLock = new object();
        static Dictionary<string, CsToDb<OdbcType>> _dicCsToDb = new Dictionary<string, CsToDb<OdbcType>>() {
                { typeof(sbyte).FullName, CsToDb.New(OdbcType.TinyInt, "tinyint","tinyint NOT NULL", false, false, 0) },{ typeof(sbyte?).FullName, CsToDb.New(OdbcType.TinyInt, "tinyint", "tinyint", false, true, null) },
                { typeof(short).FullName, CsToDb.New(OdbcType.SmallInt, "int2","int2 NOT NULL", false, false, 0) },{ typeof(short?).FullName, CsToDb.New(OdbcType.SmallInt, "int2", "int2", false, true, null) },
                { typeof(int).FullName, CsToDb.New(OdbcType.Int, "int4","int4 NOT NULL", false, false, 0) },{ typeof(int?).FullName, CsToDb.New(OdbcType.Int, "int4", "int4", false, true, null) },
                { typeof(long).FullName, CsToDb.New(OdbcType.BigInt, "int8","int8 NOT NULL", false, false, 0) },{ typeof(long?).FullName, CsToDb.New(OdbcType.BigInt, "int8", "int8", false, true, null) },

                { typeof(byte).FullName, CsToDb.New(OdbcType.SmallInt, "int2","int2 NOT NULL", false, false, 0) },{ typeof(byte?).FullName, CsToDb.New(OdbcType.SmallInt, "int2", "int2", false, true, null) },
                { typeof(ushort).FullName, CsToDb.New(OdbcType.Int, "int4","int4 NOT NULL", false, false, 0) },{ typeof(ushort?).FullName, CsToDb.New(OdbcType.Int, "int4", "int4", false, true, null) },
                { typeof(uint).FullName, CsToDb.New(OdbcType.BigInt, "int8","int8 NOT NULL", false, false, 0) },{ typeof(uint?).FullName, CsToDb.New(OdbcType.BigInt, "int8", "int8", false, true, null) },
                { typeof(ulong).FullName, CsToDb.New(OdbcType.Numeric, "numeric","numeric(20,0) NOT NULL", false, false, 0) },{ typeof(ulong?).FullName, CsToDb.New(OdbcType.Numeric, "numeric", "numeric(20,0)", false, true, null) },

                { typeof(float).FullName, CsToDb.New(OdbcType.Real, "float4","float4 NOT NULL", false, false, 0) },{ typeof(float?).FullName, CsToDb.New(OdbcType.Real, "float4", "float4", false, true, null) },
                { typeof(double).FullName, CsToDb.New(OdbcType.Double, "float8","float8 NOT NULL", false, false, 0) },{ typeof(double?).FullName, CsToDb.New(OdbcType.Double, "float8", "float8", false, true, null) },
                { typeof(decimal).FullName, CsToDb.New(OdbcType.Numeric, "numeric", "numeric(10,2) NOT NULL", false, false, 0) },{ typeof(decimal?).FullName, CsToDb.New(OdbcType.Numeric, "numeric", "numeric(10,2)", false, true, null) },

                { typeof(string).FullName, CsToDb.New(OdbcType.VarChar, "varchar", "varchar(255)", false, null, "") },

                { typeof(TimeSpan).FullName, CsToDb.New(OdbcType.Time, "time","time NOT NULL", false, false, 0) },{ typeof(TimeSpan?).FullName, CsToDb.New(OdbcType.Time, "time", "time",false, true, null) },
                { typeof(DateTime).FullName, CsToDb.New(OdbcType.DateTime, "timestamp", "timestamp NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateTime?).FullName, CsToDb.New(OdbcType.DateTime, "timestamp", "timestamp", false, true, null) },

                { typeof(bool).FullName, CsToDb.New(OdbcType.Bit, "bool","bool NOT NULL", null, false, false) },{ typeof(bool?).FullName, CsToDb.New(OdbcType.Bit, "bool","bool", null, true, null) },
                { typeof(Byte[]).FullName, CsToDb.New(OdbcType.VarBinary, "bytea", "bytea", false, null, new byte[0]) },

                { typeof(Guid).FullName, CsToDb.New(OdbcType.UniqueIdentifier, "char", "char(36) NOT NULL", false, false, Guid.Empty) },{ typeof(Guid?).FullName, CsToDb.New(OdbcType.UniqueIdentifier, "char", "char(36)", false, true, null) },
            };

        public override DbInfoResult GetDbInfo(Type type)
        {
            if (_dicCsToDb.TryGetValue(type.FullName, out var trydc)) return new DbInfoResult((int)trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable, trydc.defaultValue);
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
                    CsToDb.New(OdbcType.BigInt, "int8", $"int8{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, enumType.CreateInstanceGetDefaultValue()) :
                    CsToDb.New(OdbcType.Int, "int4", $"int4{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, enumType.CreateInstanceGetDefaultValue());
                if (_dicCsToDb.ContainsKey(type.FullName) == false)
                {
                    lock (_dicCsToDbLock)
                    {
                        if (_dicCsToDb.ContainsKey(type.FullName) == false)
                            _dicCsToDb.Add(type.FullName, newItem);
                    }
                }
                return new DbInfoResult((int)newItem.type, newItem.dbtype, newItem.dbtypeFull, newItem.isnullable, newItem.defaultValue);
            }
            return null;
        }

        protected override string GetComparisonDDLStatements(params TypeAndName[] objects)
        {
            var userId = (_orm.Ado.MasterPool as OdbcKingbaseESConnectionPool)?.UserId;
            if (string.IsNullOrEmpty(userId))
                using (var conn = _orm.Ado.MasterPool.Get())
                {
                    userId = OdbcKingbaseESConnectionPool.GetUserId(conn.Value.ConnectionString);
                }

            var defaultSchema = "PUBLIC";
            var seqcols = new List<NaviteTuple<ColumnInfo, string[], bool>>(); //序列：列，表，自增
            var seqnameDel = new List<string>(); //要删除的序列+触发器

            var sb = new StringBuilder();
            foreach (var obj in objects)
            {
                if (sb.Length > 0) sb.Append("\r\n");
                var tb = _commonUtils.GetTableByEntity(obj.entityType);
                if (tb == null) throw new Exception($"类型 {obj.entityType.FullName} 不可迁移");
                if (tb.Columns.Any() == false) throw new Exception($"类型 {obj.entityType.FullName} 不可迁移，可迁移属性0个");
                var tbname = _commonUtils.SplitTableName(tb.DbName);
                if (tbname?.Length == 1) tbname = new[] { defaultSchema, tbname[0] };

                var tboldname = _commonUtils.SplitTableName(tb.DbOldName); //旧表名
                if (tboldname?.Length == 1) tboldname = new[] { defaultSchema, tboldname[0] };
                var primaryKeyName = (obj.entityType.GetCustomAttributes(typeof(OraclePrimaryKeyNameAttribute), false)?.FirstOrDefault() as OraclePrimaryKeyNameAttribute)?.Name;
                if (string.IsNullOrEmpty(obj.tableName) == false)
                {
                    var tbtmpname = _commonUtils.SplitTableName(obj.tableName);
                    if (tbtmpname?.Length == 1) tbtmpname = new[] { defaultSchema, tbtmpname[0] };
                    if (tbname[0] != tbtmpname[0] || tbname[1] != tbtmpname[1])
                    {
                        tbname = tbtmpname;
                        tboldname = null;
                        primaryKeyName = null;
                    }
                }
                //codefirst 不支持表名中带 .

                if (string.Compare(tbname[0], defaultSchema) != 0 && _orm.Ado.ExecuteScalar(CommandType.Text, _commonUtils.FormatSql(" SELECT 1 FROM information_schema.schemata WHERE schema_name={0}", tbname[0])) == null) //创建数据库
                    sb.Append("CREATE SCHEMA IF NOT EXISTS ").Append(tbname[0]).Append(";\r\n");

                var sbalter = new StringBuilder();
                var istmpatler = false; //创建临时表，导入数据，删除旧表，修改
                if (_orm.Ado.ExecuteScalar(CommandType.Text, _commonUtils.FormatSql(" select 1 from information_schema.tables where table_schema={0} and table_name={1}", tbname)) == null)
                { //表不存在
                    if (tboldname != null)
                    {
                        if (_orm.Ado.ExecuteScalar(CommandType.Text, _commonUtils.FormatSql(" select 1 from information_schema.tables where table_schema={0} and table_name={1}", tboldname)) == null)
                            //模式或表不存在
                            tboldname = null;
                    }
                    if (tboldname == null)
                    {
                        //创建表
                        var createTableName = _commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}");
                        sb.Append("CREATE TABLE ").Append(createTableName).Append(" ( ");
                        foreach (var tbcol in tb.ColumnsByPosition)
                        {
                            sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType).Append(",");
                            if (tbcol.Attribute.IsIdentity == true) seqcols.Add(NaviteTuple.Create(tbcol, tbname, true));
                        }
                        if (tb.Primarys.Any())
                        {
                            var pkname = primaryKeyName ?? $"{tbname[0]}_{tbname[1]}_PK1";
                            sb.Append(" \r\n  CONSTRAINT ").Append(_commonUtils.QuoteSqlName(pkname)).Append(" PRIMARY KEY (");
                            foreach (var tbcol in tb.Primarys) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                            sb.Remove(sb.Length - 2, 2).Append("),");
                        }
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append("\r\n);\r\n");
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
                                sb.Append("COMMENT ON COLUMN ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}.{tbcol.Attribute.Name}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment)).Append(";\r\n");
                        }
                        if (string.IsNullOrEmpty(tb.Comment) == false)
                            sb.Append("COMMENT ON TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tb.Comment)).Append(";\r\n");
                        continue;
                    }
                    //如果新表，旧表在一个模式下，直接修改表名
                    if (string.Compare(tbname[0], tboldname[0], true) == 0)
                        sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tboldname[0]}.{tboldname[1]}")).Append(" RENAME TO ").Append(_commonUtils.QuoteSqlName($"{tbname[1]}")).Append(";\r\n");
                    else
                    {
                        //如果新表，旧表不在一起，创建新表，导入数据，删除旧表
                        istmpatler = true;
                    }
                }
                else
                    tboldname = null; //如果新表已经存在，不走改表名逻辑

                //对比字段，只可以修改类型、增加字段、有限的修改字段名；保证安全不删除字段
                var sql = _commonUtils.FormatSql($@"
select 
a.column_name,
a.data_type,
a.data_length,
a.data_precision,
a.data_scale,
a.char_used,
case when a.nullable = 'N' then 0 else 1 end,
nvl((select 1 from user_sequences where upper(sequence_owner)={{0}} and upper(sequence_name)=upper({{2}}||'_'||{{1}}||'_seq_'||a.column_name) limit 1), 0),
b.comments
from all_tab_columns a
left join all_col_comments b on b.owner = a.owner and b.table_name = a.table_name and b.column_name = a.column_name
where a.owner={{0}} and a.table_name={{1}}", userId, (tboldname ?? tbname)[1], (tboldname ?? tbname)[0]);
                var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds.Length != ds.Select(a => string.Concat(a[0])).Count()) throw new Exception("检查到多个模式下存在相同的表名，无法完成对比结构");
                var tbstruct = ds.ToDictionary(a => string.Concat(a[0]), a =>
                {
                    var sqlType = GetKingbaseESSqlTypeFullName(a);
                    return new
                    {
                        column = string.Concat(a[0]),
                        sqlType,
                        is_nullable = string.Concat(a[6]) == "1",
                        is_identity = string.Concat(a[7]) == "1",
                        comment = string.Concat(a[8])
                    };
                }, StringComparer.CurrentCultureIgnoreCase);

                if (istmpatler == false)
                {
                    foreach (var tbcol in tb.ColumnsByPosition)
                    {
                        var dbtypeNoneNotNull = Regex.Replace(tbcol.Attribute.DbType, @"NOT\s+NULL", "NULL");
                        if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
                            string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol))
                        {
                            var isCommentChanged = tbstructcol.comment != (tbcol.Comment ?? "");
                            if (tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType, StringComparison.CurrentCultureIgnoreCase) == false)
                            {
                                istmpatler = true;
                                if (istmpatler && tbcol.Attribute.DbType.StartsWith("varchar", StringComparison.CurrentCultureIgnoreCase) && tbstructcol.sqlType.StartsWith("varchar", StringComparison.CurrentCultureIgnoreCase)
                                    && Regex.Match(tbcol.Attribute.DbType, @"\(\d+").Groups[0].Value == Regex.Match(tbstructcol.sqlType, @"\(\d+").Groups[0].Value)
                                    istmpatler = false;
                                if (istmpatler && Regex.IsMatch(tbcol.Attribute.DbType, @"\(\d+") == false && Regex.IsMatch(tbstructcol.sqlType, @"\(\d+")
                                    && string.Compare(tbcol.Attribute.DbType, Regex.Replace(tbstructcol.sqlType, @"\([^\)]+\)", ""), StringComparison.CurrentCultureIgnoreCase) == 0)
                                    istmpatler = false;
                                if (istmpatler) 
                                    break;
                            }
                            if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable)
                            {
                                if (tbcol.Attribute.IsNullable == false)
                                    sbalter.Append("UPDATE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" SET ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" = ").Append(tbcol.DbDefaultValue).Append(" WHERE ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" IS NULL;\r\n");
                                sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ALTER COLUMN ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(tbcol.Attribute.IsNullable == true ? " DROP NOT NULL" : " SET NOT NULL").Append(";\r\n");
                            }
                            if (string.Compare(tbstructcol.column, tbcol.Attribute.OldName, true) == 0)
                            {
                                if (tbstructcol.is_identity)
                                    seqnameDel.Add(Utils.GetCsName($"{tbname[1]}_seq_{tbstructcol.column}"));
                                //修改列名
                                sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" RENAME COLUMN ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" TO ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(";\r\n");
                                if (tbcol.Attribute.IsIdentity)
                                    seqcols.Add(NaviteTuple.Create(tbcol, tbname, tbcol.Attribute.IsIdentity == true));
                            }
                            else if (tbcol.Attribute.IsIdentity != tbstructcol.is_identity)
                                seqcols.Add(NaviteTuple.Create(tbcol, tbname, tbcol.Attribute.IsIdentity == true));
                            if (isCommentChanged)
                                sbalter.Append("COMMENT ON COLUMN ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}.{tbcol.Attribute.Name}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment ?? "")).Append(";\r\n");
                            continue;
                        }
                        //添加列
                        sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ADD (").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(dbtypeNoneNotNull).Append(");\r\n");
                        if (tbcol.Attribute.IsNullable == false)
                        {
                            sbalter.Append("UPDATE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" SET ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" = ").Append(tbcol.DbDefaultValue).Append(";\r\n");
                            sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ALTER COLUMN ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" SET NOT NULL;\r\n");
                        }
                        if (tbcol.Attribute.IsIdentity == true) seqcols.Add(NaviteTuple.Create(tbcol, tbname, tbcol.Attribute.IsIdentity == true));
                        if (string.IsNullOrEmpty(tbcol.Comment) == false) sbalter.Append("COMMENT ON COLUMN ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}.{tbcol.Attribute.Name}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment ?? "")).Append(";\r\n");
                    }
                }
                if (istmpatler == false)
                {
                    var dsuksql = _commonUtils.FormatSql(@"
select
c.column_name,
a.index_name,
case when c.descend = 'DESC' then 1 else 0 end,
case when a.uniqueness = 'UNIQUE' then 1 else 0 end
from all_indexes a,
all_ind_columns c 
where a.index_name = c.index_name
and a.table_owner = c.table_owner
and a.table_name = c.table_name
and a.owner in ({0}) and a.table_name in ({1})
and not exists(select 1 from all_constraints where index_name = a.index_name and constraint_type = 'P')", userId, (tboldname ?? tbname)[1]);
                    var dsuk = _orm.Ado.ExecuteArray(CommandType.Text, dsuksql).Select(a => new[] { string.Concat(a[0]).Trim('"'), string.Concat(a[1]), string.Concat(a[2]), string.Concat(a[3]) }).ToArray();
                    foreach (var uk in tb.Indexes)
                    {
                        if (string.IsNullOrEmpty(uk.Name) || uk.Columns.Any() == false) continue;
                        var dsukfind1 = dsuk.Where(a => string.Compare(a[1], uk.Name, true) == 0).ToArray();
                        if (dsukfind1.Any() == false || dsukfind1.Length != uk.Columns.Length || dsukfind1.Where(a => uk.Columns.Where(b => (a[3] == "1") == uk.IsUnique && string.Compare(b.Column.Attribute.Name, a[0], true) == 0 && (a[2] == "1") == b.IsDesc).Any()).Count() != uk.Columns.Length)
                        {
                            if (dsukfind1.Any()) sbalter.Append("DROP INDEX ").Append(_commonUtils.QuoteSqlName(uk.Name)).Append(";\r\n");
                            sbalter.Append("CREATE ");
                            if (uk.IsUnique) sbalter.Append("UNIQUE ");
                            sbalter.Append("INDEX ").Append(_commonUtils.QuoteSqlName(uk.Name)).Append(" ON ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append("(");
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
                    var dbcomment = string.Concat(_orm.Ado.ExecuteScalar(CommandType.Text, _commonUtils.FormatSql(@" select comments from all_tab_comments where owner = {0} and table_name = {1} and table_type = 'TABLE'", userId, tbname[1])));
                    if (dbcomment != (tb.Comment ?? ""))
                        sb.Append("COMMENT ON TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tb.Comment)).Append(";\r\n");

                    sb.Append(sbalter);
                    continue;
                }
                var oldpk = _orm.Ado.ExecuteScalar(CommandType.Text, _commonUtils.FormatSql(@" select constraint_name from user_constraints where owner={0} and table_name={1} and constraint_type='P'", userId, tbname[1]))?.ToString();
                if (string.IsNullOrEmpty(oldpk) == false)
                    sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" DROP CONSTRAINT ").Append(_commonUtils.QuoteSqlName(oldpk)).Append(";\r\n");

                //创建临时表，数据导进临时表，然后删除原表，将临时表改名为原表名
                var tablename = tboldname == null ? _commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}") : _commonUtils.QuoteSqlName($"{tboldname[0]}.{tboldname[1]}");
                var tmptablename = _commonUtils.QuoteSqlName($"{tbname[0]}.FTmp_{tbname[1]}");
                //创建临时表
                sb.Append("CREATE TABLE ").Append(tmptablename).Append(" ( ");
                foreach (var tbcol in tb.ColumnsByPosition)
                {
                    sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType).Append(",");
                    if (tbcol.Attribute.IsIdentity == true) seqcols.Add(NaviteTuple.Create(tbcol, tbname, true));
                }
                if (tb.Primarys.Any())
                {
                    var pkname = primaryKeyName ?? $"{tbname[0]}_{tbname[1]}_PK2";
                    sb.Append(" \r\n  CONSTRAINT ").Append(_commonUtils.QuoteSqlName(pkname)).Append(" PRIMARY KEY (");
                    foreach (var tbcol in tb.Primarys) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                    sb.Remove(sb.Length - 2, 2).Append("),");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append("\r\n);\r\n");
                //备注
                foreach (var tbcol in tb.ColumnsByPosition)
                {
                    if (string.IsNullOrEmpty(tbcol.Comment) == false)
                        sb.Append("COMMENT ON COLUMN ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.FTmp_{tbname[1]}.{tbcol.Attribute.Name}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment)).Append(";\r\n");
                }
                if (string.IsNullOrEmpty(tb.Comment) == false)
                    sb.Append("COMMENT ON TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.FTmp_{tbname[1]}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tb.Comment)).Append(";\r\n");

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
                            var dbtypeNoneNotNull = Regex.Replace(tbcol.Attribute.DbType, @"(NOT\s+)?NULL", "");
                            insertvalue = $"cast({insertvalue} as {dbtypeNoneNotNull})";
                        }
                        if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable)
                            insertvalue = $"nvl({insertvalue},{tbcol.DbDefaultValue})";
                    }
                    else if (tbcol.Attribute.IsNullable == false)
                        insertvalue = tbcol.DbDefaultValue;
                    sb.Append(insertvalue.Replace("'", "''")).Append(", ");
                }
                sb.Remove(sb.Length - 2, 2).Append(" FROM ").Append(tablename).Append(";\r\n");
                sb.Append("DROP TABLE ").Append(tablename).Append(";\r\n");
                sb.Append("ALTER TABLE ").Append(tmptablename).Append(" RENAME TO ").Append(_commonUtils.QuoteSqlName($"{tbname[1]}")).Append(";\r\n");
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
            foreach (var seqcol in seqcols)
            {
                var tbname = seqcol.Item2;
                var seqname = Utils.GetCsName($"{tbname[0]}.{tbname[1]}_seq_{seqcol.Item1.Attribute.Name}").ToUpper();
                var tbname2 = _commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}");
                var colname2 = _commonUtils.QuoteSqlName(seqcol.Item1.Attribute.Name);
                sb.Append("ALTER TABLE ").Append(tbname2).Append(" ALTER COLUMN ").Append(colname2).Append(" SET DEFAULT null;\r\n");
                sb.Append("DROP SEQUENCE IF EXISTS ").Append(seqname).Append(";\r\n");
                if (seqcol.Item3)
                {
                    sb.Append("CREATE SEQUENCE ").Append(seqname).Append(";\r\n");
                    sb.Append("ALTER TABLE ").Append(tbname2).Append(" ALTER COLUMN ").Append(colname2).Append(" SET DEFAULT nextval('").Append(seqname).Append("'::regclass);\r\n");
                    sb.Append(" SELECT case when max(").Append(colname2).Append(") is null then 0 else setval('").Append(seqname).Append("', max(").Append(colname2).Append(")) end FROM ").Append(tbname2).Append(";\r\n");
                }
            }
            return sb.Length == 0 ? null : sb.ToString();
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

        internal static string GetKingbaseESSqlTypeFullName(object[] row)
        {
            var a = row;
            var sqlType = string.Concat(a[1]).ToUpper();
            var data_length = long.Parse(string.Concat(a[2]));
            long.TryParse(string.Concat(a[3]), out var data_precision);
            long.TryParse(string.Concat(a[4]), out var data_scale);
            //var char_used = string.Concat(a[5]);
            if (sqlType.StartsWith("TIMESTAMP", StringComparison.CurrentCultureIgnoreCase))
            {
            }
            else if (sqlType.StartsWith("BLOB"))
            {
            }
            else if (sqlType == "DOUBLE" || sqlType == "FLOAT")
            {
            }
            else if (sqlType == "INT2" || sqlType == "INT4" || sqlType == "INT8")
            {
            }
            else if (sqlType == "TINYINT")
            {
            }
            else if (sqlType == "BOOL")
            {
            }
            else if (sqlType == "FLOAT4" || sqlType == "FLOAT8")
            {
            }
            else if (sqlType == "TIME" || sqlType == "TIMESTAMP")
            {
            }
            else if (sqlType == "NUMERIC")
                sqlType += $"({data_precision},{data_scale})";
            else if (data_precision > 0 && data_scale > 0)
                sqlType += $"({data_precision},{data_scale})";
            else if (data_precision > 0)
                sqlType += $"({data_precision})";
            else
                sqlType += $"({data_length})";
            return sqlType;
        }
    }
}