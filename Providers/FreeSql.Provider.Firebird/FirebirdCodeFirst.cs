using FreeSql.Internal;
using FreeSql.Internal.Model;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FreeSql.DataAnnotations;

namespace FreeSql.Firebird
{

    class FirebirdCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {

        public FirebirdCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm, commonUtils, commonExpression) { }

        static object _dicCsToDbLock = new object();
        static Dictionary<string, CsToDb<FbDbType>> _dicCsToDb = new Dictionary<string, CsToDb<FbDbType>>() {
                { typeof(sbyte).FullName, CsToDb.New(FbDbType.SmallInt, "smallint","smallint NOT NULL", false, false, 0) },{ typeof(sbyte?).FullName, CsToDb.New(FbDbType.SmallInt, "smallint", "smallint", false, true, null) },
                { typeof(short).FullName, CsToDb.New(FbDbType.SmallInt, "smallint","smallint NOT NULL", false, false, 0) },{ typeof(short?).FullName, CsToDb.New(FbDbType.SmallInt, "smallint", "smallint", false, true, null) },
                { typeof(int).FullName, CsToDb.New(FbDbType.Integer, "integer","integer NOT NULL", false, false, 0) },{ typeof(int?).FullName, CsToDb.New(FbDbType.Integer, "integer", "integer", false, true, null) },
                { typeof(long).FullName, CsToDb.New(FbDbType.BigInt, "bigint","bigint NOT NULL", false, false, 0) },{ typeof(long?).FullName, CsToDb.New(FbDbType.BigInt, "bigint", "bigint", false, true, null) },

                { typeof(byte).FullName, CsToDb.New(FbDbType.SmallInt, "smallint","smallint NOT NULL", false, false, 0) },{ typeof(byte?).FullName, CsToDb.New(FbDbType.SmallInt, "smallint", "smallint", false, true, null) },
                { typeof(ushort).FullName, CsToDb.New(FbDbType.Integer, "integer","integer NOT NULL", false, false, 0) },{ typeof(ushort?).FullName, CsToDb.New(FbDbType.Integer, "integer", "integer", false, true, null) },
                { typeof(uint).FullName, CsToDb.New(FbDbType.BigInt, "bigint","bigint NOT NULL", false, false, 0) },{ typeof(uint?).FullName, CsToDb.New(FbDbType.BigInt, "bigint", "bigint", false, true, null) },
                { typeof(ulong).FullName, CsToDb.New(FbDbType.Decimal, "decimal","decimal(18,0) NOT NULL", false, false, 0) },{ typeof(ulong?).FullName, CsToDb.New(FbDbType.Decimal, "decimal", "decimal(18,0)", false, true, null) },

                { typeof(float).FullName, CsToDb.New(FbDbType.Float, "float","float NOT NULL", false, false, 0) },{ typeof(float?).FullName, CsToDb.New(FbDbType.Float, "float", "float", false, true, null) },
                { typeof(double).FullName, CsToDb.New(FbDbType.Double, "double precision","double precision NOT NULL", false, false, 0) },{ typeof(double?).FullName, CsToDb.New(FbDbType.Double, "double precision", "double precision", false, true, null) },
                { typeof(decimal).FullName, CsToDb.New(FbDbType.Decimal, "decimal", "decimal(10,2) NOT NULL", false, false, 0) },{ typeof(decimal?).FullName, CsToDb.New(FbDbType.Numeric, "decimal", "decimal(10,2)", false, true, null) },

                { typeof(string).FullName, CsToDb.New(FbDbType.VarChar, "varchar", "varchar(200)", false, null, "") },

                { typeof(TimeSpan).FullName, CsToDb.New(FbDbType.Time, "time","time NOT NULL", false, false, 0) },{ typeof(TimeSpan?).FullName, CsToDb.New(FbDbType.Time, "time", "time",false, true, null) },
                { typeof(DateTime).FullName, CsToDb.New(FbDbType.TimeStamp, "timestamp", "timestamp NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateTime?).FullName, CsToDb.New(FbDbType.TimeStamp, "timestamp", "timestamp", false, true, null) },

                { typeof(bool).FullName, CsToDb.New(FbDbType.Boolean, "boolean","boolean NOT NULL", null, false, false) },{ typeof(bool?).FullName, CsToDb.New(FbDbType.Boolean, "boolean","boolean", null, true, null) },
                { typeof(byte[]).FullName, CsToDb.New(FbDbType.Binary, "blob", "blob", false, null, new byte[0]) },

                { typeof(Guid).FullName, CsToDb.New(FbDbType.Guid, "char(36)", "char(36) NOT NULL", false, false, Guid.Empty) },{ typeof(Guid?).FullName, CsToDb.New(FbDbType.Guid, "char(36)", "char(36)", false, true, null) },
            };

        public override DbInfoResult GetDbInfo(Type type)
        {
            var info = GetDbInfoNoneArray(type);
            if (info == null) return null;
            return new DbInfoResult((int)info.type, info.dbtype, info.dbtypeFull, info.isnullable, info.defaultValue);
            //var isarray = type.FullName != "System.Byte[]" && type.IsArray;
            //var elementType = isarray ? type.GetElementType() : type;
            //var info = GetDbInfoNoneArray(elementType);
            //if (info == null) return null;
            //if (isarray == false) return new DbInfoResult((int)info.type, info.dbtype, info.dbtypeFull, info.isnullable, info.defaultValue);
            //var dbtypefull = Regex.Replace(info.dbtypeFull, $@"{info.dbtype}(\s*\([^\)]+\))?", "$0[]").Replace(" NOT NULL", "");
            //return new DbInfoResult((int)(info.type | FbDbType.Array), $"{info.dbtype}[]", dbtypefull, null, Array.CreateInstance(elementType, 0));
        }
        CsToDb<FbDbType> GetDbInfoNoneArray(Type type)
        {
            if (_dicCsToDb.TryGetValue(type.FullName, out var trydc)) return trydc;
            if (type.IsArray) return null;
            var enumType = type.IsEnum ? type : null;
            if (enumType == null && type.IsNullableType() && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments.First().IsEnum) enumType = type.GenericTypeArguments.First();
            if (enumType != null)
            {
                var newItem = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any() ?
                    CsToDb.New(FbDbType.BigInt, "bigint", $"bigint{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, enumType.CreateInstanceGetDefaultValue()) :
                    CsToDb.New(FbDbType.Integer, "integer", $"integer{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, enumType.CreateInstanceGetDefaultValue());
                if (_dicCsToDb.ContainsKey(type.FullName) == false)
                {
                    lock (_dicCsToDbLock)
                    {
                        if (_dicCsToDb.ContainsKey(type.FullName) == false)
                            _dicCsToDb.Add(type.FullName, newItem);
                    }
                }
                return newItem;
            }
            return null;
        }

        protected override string GetComparisonDDLStatements(params TypeAndName[] objects)
        {
            var sb = new StringBuilder();

            foreach (var obj in objects)
            {
                if (sb.Length > 0) sb.Append("\r\n");
                var tb = _commonUtils.GetTableByEntity(obj.entityType);
                if (tb == null) throw new Exception(CoreStrings.S_Type_IsNot_Migrable(obj.entityType.FullName));
                if (tb.Columns.Any() == false) throw new Exception(CoreStrings.S_Type_IsNot_Migrable_0Attributes(obj.entityType.FullName));
                var tbname = tb.DbName;
                var tboldname = tb.DbOldName; //旧表名
                if (string.IsNullOrEmpty(obj.tableName) == false)
                {
                    var tbtmpname = obj.tableName;
                    if (tbname != tbtmpname)
                    {
                        tbname = tbtmpname;
                        tboldname = null;
                    }
                }

                if (_orm.Ado.ExecuteScalar(CommandType.Text, string.Format(" select 1 from rdb$relations where rdb$system_flag = 0 and trim(rdb$relation_name) = '{0}'", tbname)) == null)
                { //表不存在
                    if (tboldname != null)
                    {
                        if (_orm.Ado.ExecuteScalar(CommandType.Text, string.Format(" select 1 from rdb$relations where rdb$system_flag = 0 and trim(rdb$relation_name) = '{0}'", tboldname)) == null)
                            //旧表不存在
                            tboldname = null;
                    }
                    if (tboldname == null)
                    {
                        //创建表
                        var createTableName = _commonUtils.QuoteSqlName(tbname);
                        sb.Append("CREATE TABLE ").Append(createTableName).Append(" ( ");
                        foreach (var tbcol in tb.ColumnsByPosition)
                        {
                            sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType);
                            if (tbcol.Attribute.IsIdentity == true) sb.Append(" GENERATED BY DEFAULT AS IDENTITY");
                            sb.Append(",");
                        }
                        if (tb.Primarys.Any())
                        {
                            var pkname = $"{tbname}_PKEY";
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
                            if (uk.Columns.Any(a => a.IsDesc)) sb.Append("DESC ");
                            sb.Append("INDEX ").Append(_commonUtils.QuoteSqlName(ReplaceIndexName(uk.Name, tbname))).Append(" ON ").Append(createTableName).Append("(");
                            foreach (var tbcol in uk.Columns)
                            {
                                sb.Append(_commonUtils.QuoteSqlName(tbcol.Column.Attribute.Name));
                                sb.Append(", ");
                            }
                            sb.Remove(sb.Length - 2, 2).Append(");\r\n");
                        }
                        //备注
                        foreach (var tbcol in tb.ColumnsByPosition)
                        {
                            if (string.IsNullOrEmpty(tbcol.Comment) == false)
                                sb.Append("COMMENT ON COLUMN ").Append(_commonUtils.QuoteSqlName($"{tbname}.{tbcol.Attribute.Name}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment)).Append(";\r\n");
                        }
                        if (string.IsNullOrEmpty(tb.Comment) == false)
                            sb.Append("COMMENT ON TABLE ").Append(_commonUtils.QuoteSqlName(tbname)).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tb.Comment)).Append(";\r\n");
                        continue;
                    }
                    sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tboldname)).Append(" RENAME TO ").Append(_commonUtils.QuoteSqlName(tbname)).Append(";\r\n");
                }
                else
                    tboldname = null; //如果新表已经存在，不走改表名逻辑

                //对比字段，只可以修改类型、增加字段、有限的修改字段名；保证安全不删除字段
                var sql = _commonUtils.FormatSql($@"
select
trim(a.rdb$field_name),
case
  when b.rdb$field_sub_type = 2 then (select 'DECIMAL(' || rdb$field_precision || ',' || abs(rdb$field_scale) || ')' from rdb$types where b.rdb$field_sub_type = 2 and rdb$type = b.rdb$field_sub_type and rdb$field_name = 'RDB$FIELD_SUB_TYPE' rows 1)
  when b.rdb$field_type = 14 then 'CHAR(' || b.rdb$character_length || ')'
  when b.rdb$field_type = 37 then 'VARCHAR(' || b.rdb$character_length || ')'
  when b.rdb$field_type = 8 then 'INTEGER'
  when b.rdb$field_type = 16 then 'BIGINT'
  when b.rdb$field_type = 27 then 'DOUBLE PRECISION'
  when b.rdb$field_type = 7 then 'SMALLINT'
  else
    (select trim(rdb$type_name) from rdb$types where rdb$type = b.rdb$field_type and rdb$field_name = 'RDB$FIELD_TYPE' rows 1) || 
    coalesce((select ' SUB_TYPE ' || rdb$type from rdb$types where b.rdb$field_type = 261 and rdb$type = b.rdb$field_sub_type and rdb$field_name = 'RDB$FIELD_SUB_TYPE' rows 1),'')
  end || trim(case when b.rdb$dimensions = 1 then '[]' else '' end),
case when a.rdb$null_flag = 1 then 0 else 1 end,
{((_orm.Ado as FirebirdAdo)?.IsFirebird2_5 == true ? "0" : "case when a.rdb$identity_type = 1 then 1 else 0 end")},
a.rdb$description
from rdb$relation_fields a
inner join rdb$fields b on b.rdb$field_name = a.rdb$field_source
inner join rdb$relations d on d.rdb$relation_name = a.rdb$relation_name
where a.rdb$system_flag = 0 and trim(d.rdb$relation_name) = {{0}}
order by a.rdb$relation_name, a.rdb$field_position", tboldname ?? tbname);
                var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                var tbstruct = ds.ToDictionary(a => string.Concat(a[0]), a =>
                {
                    var a1 = string.Concat(a[1]);
                    if (a1 == "BLOB SUB_TYPE 0") a1 = "BLOB";
                    return new
                    {
                        column = string.Concat(a[0]),
                        sqlType = a1,
                        is_nullable = string.Concat(a[2]) == "1",
                        is_identity = string.Concat(a[3]) == "1",
                        comment = string.Concat(a[4])
                    };
                }, StringComparer.CurrentCultureIgnoreCase);

                var existsPrimary = _orm.Ado.ExecuteScalar(_commonUtils.FormatSql(@" select 1 from rdb$relation_constraints d where d.rdb$constraint_type = 'PRIMARY KEY' and trim(d.rdb$relation_name) = {0}", tbname));
                foreach (var tbcol in tb.ColumnsByPosition)
                {
                    if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
                        string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol))
                    {
                        var isCommentChanged = tbstructcol.comment != (tbcol.Comment ?? "");
                        var isDbTypeChanged = tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType, StringComparison.CurrentCultureIgnoreCase) == false;

                        if (isDbTypeChanged ||
                            tbcol.Attribute.IsIdentity != tbstructcol.is_identity)
                        {
                            if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable && tbcol.Attribute.IsNullable == false && tbcol.DbDefaultValue != "NULL" && tbcol.Attribute.IsIdentity == false)
                                sb.Append("UPDATE ").Append(_commonUtils.QuoteSqlName(tbname)).Append(" SET ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" = ").Append(tbcol.DbDefaultValue).Append(" WHERE ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" IS NULL;\r\n");
                            sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tbname)).Append(" ALTER COLUMN ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" TYPE ").Append(tbcol.Attribute.DbType.Replace("NOT NULL", ""));
                            if (tbcol.Attribute.IsIdentity == true && tbstructcol.is_identity == false) sb.Append(" GENERATED BY DEFAULT AS IDENTITY");
                            sb.Append(";\r\n");
                        }
                        //if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable)
                        //    sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tbname)).Append(" ALTER COLUMN ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(tbcol.Attribute.IsNullable ? " SET DEFAULT NULL" : $" SET DEFAULT {tbcol.DbDefaultValue}").Append(";\r\n");
                        if (string.Compare(tbstructcol.column, tbcol.Attribute.OldName, true) == 0) //修改列名
                            sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tbname)).Append(" ALTER COLUMN ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" TO ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(";\r\n");
                        if (isCommentChanged)
                            sb.Append("COMMENT ON COLUMN ").Append(_commonUtils.QuoteSqlName($"{tbname}.{tbcol.Attribute.Name}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment ?? "")).Append(";\r\n");
                        continue;
                    }
                    //添加列
                    sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tbname)).Append(" ADD ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType.Replace("NOT NULL", ""));
                    if (tbcol.Attribute.IsNullable == false && tbcol.DbDefaultValue != "NULL" && tbcol.Attribute.IsIdentity == false) sb.Append(" DEFAULT ").Append(tbcol.DbDefaultValue);
                    if (tbcol.Attribute.IsIdentity == true) sb.Append(" GENERATED BY DEFAULT AS IDENTITY");
                    if (tbcol.Attribute.DbType.Contains("NOT NULL")) sb.Append(" NOT NULL");
                    sb.Append(";\r\n");
                    if (string.IsNullOrEmpty(tbcol.Comment) == false)
                        sb.Append("COMMENT ON COLUMN ").Append(_commonUtils.QuoteSqlName($"{tbname}.{tbcol.Attribute.Name}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment ?? "")).Append(";\r\n");
                }
                var dsuksql = _commonUtils.FormatSql(@"
select
trim(c.rdb$field_name),
trim(d.rdb$index_name),
coalesce(d.rdb$index_type, 0),
case when d.rdb$unique_flag = 1 then 1 else 0 end
from rdb$indices d
inner join rdb$index_segments c on c.rdb$index_name = d.rdb$index_name
where trim(d.rdb$relation_name) = {0}", tboldname ?? tbname);
                var dsuk = _orm.Ado.ExecuteArray(CommandType.Text, dsuksql).Select(a => new[] { string.Concat(a[0]), string.Concat(a[1]), string.Concat(a[2]), string.Concat(a[3]) });
                foreach (var uk in tb.Indexes)
                {
                    if (string.IsNullOrEmpty(uk.Name) || uk.Columns.Any() == false) continue;
                    var ukname = ReplaceIndexName(uk.Name, tbname);
                    var dsukfind1 = dsuk.Where(a => string.Compare(a[1], ukname, true) == 0).ToArray();
                    if (dsukfind1.Any() == false || dsukfind1.Length != uk.Columns.Length || 
                        dsukfind1.Where(a => (a[3] == "1") == uk.IsUnique && uk.Columns.Where(b => string.Compare(b.Column.Attribute.Name, a[0], true) == 0).Any()).Count() != uk.Columns.Length ||
                        dsukfind1.Any(a => a[2] == "1") && !uk.Columns.Any(a => a.IsDesc))
                    {
                        if (dsukfind1.Any()) sb.Append("DROP INDEX ").Append(_commonUtils.QuoteSqlName(ukname))
                                //.Append(" ON ").Append(_commonUtils.QuoteSqlName(tbname))
                                .Append(";\r\n");
                        sb.Append("CREATE ");
                        if (uk.IsUnique) sb.Append("UNIQUE ");
                        if (uk.Columns.Any(a => a.IsDesc)) sb.Append("DESC ");
                        sb.Append("INDEX ").Append(_commonUtils.QuoteSqlName(ukname)).Append(" ON ").Append(_commonUtils.QuoteSqlName(tbname)).Append("(");
                        foreach (var tbcol in uk.Columns)
                        {
                            sb.Append(_commonUtils.QuoteSqlName(tbcol.Column.Attribute.Name));
                            sb.Append(", ");
                        }
                        sb.Remove(sb.Length - 2, 2).Append(");\r\n");
                    }
                }
                var dbcomment = string.Concat(_orm.Ado.ExecuteScalar(CommandType.Text, _commonUtils.FormatSql(@" select trim(rdb$external_description) from rdb$relations where rdb$system_flag=0 and trim(rdb$relation_name) = {0}", tbname)));
                if (dbcomment != (tb.Comment ?? ""))
                    sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tbname)).Append(" COMMENT ").Append(" ").Append(_commonUtils.FormatSql("{0}", tb.Comment ?? "")).Append(";\r\n");
            }
            return sb.Length == 0 ? null : sb.ToString();
        }

        public override int ExecuteDDLStatements(string ddl)
        {
            if (string.IsNullOrEmpty(ddl)) return 0;
            var scripts = ddl.Split(new string[] { ";\r\n" }, StringSplitOptions.None).Where(a => string.IsNullOrEmpty(a.Trim()) == false).ToArray();

            if (scripts.Any() == false) return 0;

            var affrows = 0;
            foreach (var script in scripts)
                affrows += base.ExecuteDDLStatements(script);
            return affrows;
        }
    }
}