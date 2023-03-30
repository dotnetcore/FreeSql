using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using XuguClient;

namespace FreeSql.Xugu
{

    class XuguCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {
        public readonly string DefaultSchema = "SYSDBA";//默认模式
        public XuguCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm, commonUtils, commonExpression) { 
        }

        static object _dicCsToDbLock = new object();
        static Dictionary<string, CsToDb<XGDbType>> _dicCsToDb = new Dictionary<string, CsToDb<XGDbType>>() {

                { 
                    typeof(byte).FullName, 
                    CsToDb.New(XGDbType.SmallInt, "TINYINT","TINYINT NOT NULL", false, false, 0) 
                },

                { typeof(byte?).FullName, CsToDb.New(XGDbType.SmallInt, "TINYINT", "TINYINT", false, true, null) },
                { typeof(short).FullName, CsToDb.New(XGDbType.SmallInt, "SMALLINT","SMALLINT NOT NULL", false, false, 0) },{ typeof(short?).FullName, CsToDb.New(XGDbType.SmallInt, "SMALLINT", "SMALLINT", false, true, null) },
                { typeof(int).FullName, CsToDb.New(XGDbType.Int, "INTEGER","INTEGER NOT NULL", false, false, 0) },{ typeof(int?).FullName, CsToDb.New(XGDbType.Int, "INTEGER", "INTEGER", false, true, null) },
                { typeof(long).FullName, CsToDb.New(XGDbType.BigInt, "BIGINT","BIGINT NOT NULL", false, false, 0) },{ typeof(long?).FullName, CsToDb.New(XGDbType.BigInt, "BIGINT", "BIGINT", false, true, null) },

                 
                { typeof(ushort).FullName, CsToDb.New(XGDbType.Int, "INT","INT NOT NULL", false, false, 0) },{ typeof(ushort?).FullName, CsToDb.New(XGDbType.Int, "INT", "INT", false, true, null) },
                { typeof(uint).FullName, CsToDb.New(XGDbType.BigInt, "BIGINT","BIGINT NOT NULL", false, false, 0) },{ typeof(uint?).FullName, CsToDb.New(XGDbType.BigInt, "BIGINT", "BIGINT", false, true, null) },
                { typeof(ulong).FullName, CsToDb.New(XGDbType.Numeric, "NUMERIC","NUMERIC(20,0) NOT NULL", false, false, 0) },{ typeof(ulong?).FullName, CsToDb.New(XGDbType.Numeric, "NUMERIC", "NUMERIC(20,0)", false, true, null) },

                { typeof(float).FullName, CsToDb.New(XGDbType.Real, "FLOAT","FLOAT NOT NULL", false, false, 0) },{ typeof(float?).FullName, CsToDb.New(XGDbType.Real, "FLOAT", "FLOAT", false, true, null) },
                { typeof(double).FullName, CsToDb.New(XGDbType.Double, "DOUBLE","DOUBLE NOT NULL", false, false, 0) },{ typeof(double?).FullName, CsToDb.New(XGDbType.Double, "DOUBLE", "DOUBLE", false, true, null) },
                { typeof(decimal).FullName, CsToDb.New(XGDbType.Numeric, "NUMERIC", "NUMERIC(10,2) NOT NULL", false, false, 0) },{ typeof(decimal?).FullName, CsToDb.New(XGDbType.Numeric, "NUMERIC", "NUMERIC(10,2)", false, true, null) },

                { typeof(string).FullName, CsToDb.New(XGDbType.VarChar, "VARCHAR", "VARCHAR(255)", false, null, "") },
               
                { typeof(char).FullName, CsToDb.New(XGDbType.Char, "CHAR", "CHAR(1)", false, null, '\0') },

                //{ typeof(TimeSpan).FullName, CsToDb.New(XGDbType.Time, "time","time NOT NULL", false, false, 0) },{ typeof(TimeSpan?).FullName, CsToDb.New(XGDbType.Time, "time", "time",false, true, null) },
                { typeof(DateTime).FullName, CsToDb.New(XGDbType.DateTime, "DATETIME", "DATETIME NOT NULL", false, false, new DateTime(1970,1,1)) },
                { typeof(DateTime?).FullName, CsToDb.New(XGDbType.DateTime, "DATETIME", "DATETIME", false, true, null) },

                { typeof(bool).FullName, CsToDb.New(XGDbType.Bool, "BOOLEAN","BOOLEAN NOT NULL", null, false, false) },{ typeof(bool?).FullName, CsToDb.New(XGDbType.Bool, "BOOLEAN","BOOLEAN", null, true, null) },

                { typeof(byte[]).FullName, CsToDb.New(XGDbType.VarBinary, "blob", "blob NULL", false, null, new byte[0]) },
                 { typeof(Guid).FullName, CsToDb.New(XGDbType.Char, "char", "char(36) NOT NULL", false, false, Guid.Empty) },{ typeof(Guid?).FullName, CsToDb.New(XGDbType.Char, "char", "char(36) NULL", false, true, null) },

               

                //{ typeof(Dictionary<string, string>).FullName, CsToDb.New(XGDbType.Hstore, "hstore", "hstore", false, null, new Dictionary<string, string>()) },
  
        };


        public override DbInfoResult GetDbInfo(Type type)
        {
            _dicCsToDb.TryGetValue(type.FullName, out var info);
            if (info == null) return null;
            return new DbInfoResult((int)info.type, info.dbtype, info.dbtypeFull, info.isnullable, info.defaultValue);
        }
        internal static string GetXuguSqlTypeFullName(object[] row)
        {
            var a = row;
            var sqlType = string.Concat(a[1]).ToUpper();
            var data_length = long.Parse(string.Concat(a[2])); 
            var char_used = string.Concat(a[5]);
            bool.TryParse(a[9]?.ToString(), out var isVar);
            if (sqlType == "CHAR" && isVar)
            {
                sqlType = "VARCHAR";
            }

            if (data_length <= 0)
            {
            }
            else if (sqlType.StartsWith("TIMESTAMP", StringComparison.CurrentCultureIgnoreCase))
            {
            }
            else if (sqlType.StartsWith("DATETIME", StringComparison.CurrentCultureIgnoreCase))
            {
            }
            else if (sqlType.StartsWith("BLOB"))
            {
            }
            else if (sqlType.StartsWith("CLOB"))
            {
            } 
            else if (sqlType.ToUpper()== "NUMERIC")
            {
                //data_length=655362
                //实际类型是 NUMRIC(10,2) 计算得到的是 NUMRIC(10,12)
                //标度计算错误

                var data_precision= data_length % 65536;
                var data_scale = data_length / 65536;
                sqlType += $"({data_scale},{data_precision})";
            }
            else if (sqlType.ToLower() == "float")
            { }
            else
                sqlType += $"({data_length})";
            return sqlType;
        }
        protected override string GetComparisonDDLStatements(params TypeAndName[] objects)
        {
            var sb = new StringBuilder();
            var seqcols = new List<NativeTuple<ColumnInfo, string[], bool>>(); //序列

            foreach (var obj in objects)
            {
                if (sb.Length > 0) sb.Append("\r\n");
                var tb = _commonUtils.GetTableByEntity(obj.entityType);
                if (tb == null) throw new Exception(CoreStrings.S_Type_IsNot_Migrable(obj.entityType.FullName));
                if (tb.Columns.Any() == false) throw new Exception(CoreStrings.S_Type_IsNot_Migrable_0Attributes(obj.entityType.FullName));
                var tbname = _commonUtils.SplitTableName(tb.DbName);
                if (tbname?.Length == 1) tbname = new[] { DefaultSchema, tbname[0] };

                var tboldname = _commonUtils.SplitTableName(tb.DbOldName); //旧表名

                if (tboldname?.Length == 1) tboldname = new[] { DefaultSchema, tboldname[0] };
                if (string.IsNullOrEmpty(obj.tableName) == false)
                {
                    var tbtmpname = _commonUtils.SplitTableName(obj.tableName);
                    if (tbtmpname?.Length == 1) tbtmpname = new[] { DefaultSchema, tbtmpname[0] };
                    if (tbname[0] != tbtmpname[0] || tbname[1] != tbtmpname[1])
                    {
                        tbname = tbtmpname;
                        tboldname = null;
                    }
                }
                //codefirst 不支持表名、模式名、数据库名中带 .

                if (_orm.Ado.ExecuteScalar(CommandType.Text, _commonUtils.FormatSql(" select 1 from all_schemas where schema_name={0}", tbname[0])) == null) //创建模式
                    throw new Exception($"模式“{tbname[0]}”不存在，请手动创建");
                    //sb.Append($"CREATE SCHEMA {tbname[0]};\r\n");
                    //sb.Append("CREATE SCHEMA IF NOT EXISTS ").Append(tbname[0]).Append(";\r\n");

                var sbalter = new StringBuilder();
                var istmpatler = false; //创建临时表，导入数据，删除旧表，修改

                //虚谷
                var sql0 = string.Format("select 1 from all_tables a inner join all_schemas b on b.schema_id = a.schema_id where b.schema_name || '.' || a.table_name = '{0}.{1}'", tbname);

            
                //判断表是否存在
                if (_orm.Ado.ExecuteScalar(CommandType.Text, sql0) == null)
                { 
                    //表不存在
                    if (tboldname != null)
                    {
                        if (_orm.Ado.ExecuteScalar(CommandType.Text, string.Format(" select 1 from all_tables a inner join all_schemas b on b.schema_id = a.schema_id  where b.schema_name || '.' || a.table_name = '{0}.{1}'", tboldname)) == null)
                            //旧表不存在
                            tboldname = null;
                    }
                    if (tboldname == null)
                    {
                        //创建表
                        var createTableName = _commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}");
                        sb.Append("CREATE TABLE ").Append(createTableName).Append(" ( ");
                        foreach (var tbcol in tb.ColumnsByPosition)
                        {
                            sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ");
                            if (tbcol.Attribute.IsIdentity == true)
                            {
                                sb.Append(tbcol.Attribute.DbType.Replace("NOT NULL",""));
                                sb.Append(" identity(1,1)");
                                //seqcols.Add(NativeTuple.Create(tbcol, tbname, true));
                            }
                            else
                            {
                                sb.Append(tbcol.Attribute.DbType.Replace("NTEXT", "CLOB").Replace("TEXT", "CLOB"));
                            }
                            sb.Append(",");
                        }
                        if (tb.Primarys.Any())
                        {
                            var pkname = $"{tbname[0]}_{tbname[1]}_pkey";
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
                            sb.Append("INDEX ").Append(_commonUtils.QuoteSqlName(ReplaceIndexName(uk.Name, tbname[1]))).Append(" ON ").Append(createTableName).Append("(");
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
                    //如果新表，旧表在一个数据库和模式下，直接修改表名
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

              
                var sql = _commonUtils.FormatSql($@"
select 
a.COL_NAME,
a.TYPE_NAME,
a.SCALE,
case when a.NOT_NULL then '0' else '1' end as is_nullable,
seq.SEQ_ID,
seq.IS_SYS,
a.COMMENTS,
tb.table_name,
ns.schema_id,
a.`VARYING`
from all_columns as a  
left join all_tables tb on a.table_id=tb.table_id
left join all_schemas ns on tb.schema_id = ns.schema_id
left join all_sequences seq on seq.SEQ_ID=a.Serial_ID
WHERE ns.SCHEMA_NAME={{0}} and tb.TABLE_NAME={{1}}

", tboldname ?? tbname);
                var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                //从系统表读取的现有字段的基本信息
                var tbstruct = ds.ToDictionary(a => string.Concat(a[0]), a =>
                {
                    var sqlType = GetXuguSqlTypeFullName(a);    
                    var max_length = long.Parse(string.Concat(a[2]));
                    //long SEQ_ID =  0;
                    long.TryParse(a[4]?.ToString() ?? "0", out long SEQ_ID);
                    var SEQ_IS_SYS = (a[5]?.ToString() ?? "false")?.ToLower();
                    try
                    {
                        return new
                        {
                            column = string.Concat(a[0]),
                            sqlType = string.Concat(sqlType),
                            max_length = long.Parse(string.Concat(a[2]) ?? "0"),
                            is_nullable = string.Concat(a[3]) == "1",
                            is_identity = (SEQ_ID > 0 && SEQ_IS_SYS == "true"),
                            comment = string.Concat(a[6])
                        };
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                   
                }, StringComparer.CurrentCultureIgnoreCase);

                if (istmpatler == false)
                {
                    //基本信息对比 比如名称 添加列 数据类型 Identity
                    foreach (var tbcol in tb.ColumnsByPosition)
                    {
                        //对别数据库中列和C#中的信息是否一致
                        if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
                            string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol))
                        {
                            var isCommentChanged = tbstructcol.comment != (tbcol.Comment ?? "");
                            var sqlTypeSize = tbstructcol.sqlType;
                            //如果数据中是CHAR或VARCHAR 要加上C#中定义的长度
                            if (sqlTypeSize.Contains("(") == false)
                            {
                                switch (sqlTypeSize)
                                {
                                    case "char":
                                    case "varchar":
                                        sqlTypeSize = $"{sqlTypeSize}({tbstructcol.max_length})"; break;
                                }
                            }

                            var dbType = tbcol.Attribute.DbType.Replace("NTEXT", "CLOB").Replace("TEXT", "CLOB");
                            if (dbType.StartsWith(sqlTypeSize, StringComparison.CurrentCultureIgnoreCase) == false)
                                sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ALTER COLUMN ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" ").Append(tbcol.Attribute.DbType.Split(' ').First().Replace("NTEXT", "CLOB").Replace("TEXT", "CLOB")).Append(";\r\n");//为了适应FreeSQL的长文本规则采用Replace



                            if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable)
                            {
                                if (tbcol.Attribute.IsNullable != true || tbcol.Attribute.IsNullable == true && tbcol.Attribute.IsPrimary == false)
                                {
                                    if (tbcol.Attribute.IsNullable == false)
                                        sbalter.Append("UPDATE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" SET ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" = ").Append(tbcol.DbDefaultValue).Append(" WHERE ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" IS NULL;\r\n");
                                    sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ALTER COLUMN ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" ").Append(tbcol.Attribute.IsNullable == true ? "DROP" : "SET").Append(" NOT NULL;\r\n");
                                }
                            }
                            if (tbcol.Attribute.IsIdentity != tbstructcol.is_identity)
                            {
                                //sbalter.Append(" identity(1,1)");
                                seqcols.Add(NativeTuple.Create(tbcol, tbname, tbcol.Attribute.IsIdentity == true));
                            }
                            if (string.Compare(tbstructcol.column, tbcol.Attribute.OldName, true) == 0)
                                //修改列名
                                sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" RENAME COLUMN ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" TO ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(";\r\n");

                            if (isCommentChanged)
                                sbalter.Append("COMMENT ON COLUMN ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}.{tbcol.Attribute.Name}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment)).Append(";\r\n");

                            continue;
                        }
                        //添加列
                        sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ADD COLUMN ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType.Split(' ').First()).Append(";\r\n");
                        sbalter.Append("UPDATE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" SET ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" = ").Append(tbcol.DbDefaultValue).Append(";\r\n");
                        if (tbcol.Attribute.IsNullable == false) sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ALTER COLUMN ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" SET NOT NULL;\r\n");
                        if (tbcol.Attribute.IsIdentity == true)
                        {
                            seqcols.Add(NativeTuple.Create(tbcol, tbname, tbcol.Attribute.IsIdentity == true));
                        }
                        if (string.IsNullOrEmpty(tbcol.Comment) == false) sbalter.Append("COMMENT ON COLUMN ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}.{tbcol.Attribute.Name}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment)).Append(";\r\n");
                    }
                   
                    var dsuksql = _commonUtils.FormatSql($@"
select 
keys,
a.index_name,
case when a.IS_UNIQUE  then 1 else 0 end as IsUnique,
t.table_name,
ns.SCHEMA_NAME,
a.IS_SYS
from all_INDEXES a
left join all_tables t on a.TABLE_ID=t.TABLE_ID
left join all_schemas ns on t.SCHEMA_ID=ns.SCHEMA_ID
WHERE a.IS_Primary = false and t.table_name={{1}} and schema_name={{0}}",
                    tboldname ?? tbname);

                    var dsuk = _orm.Ado.ExecuteArray(CommandType.Text, dsuksql).Select(a => new[] { string.Concat(a[0]), string.Concat(a[1]), string.Concat(a[2]), string.Concat(a[3]), string.Concat(a[4]) }).ToList();
                    foreach (var uk in tb.Indexes)
                    {
                        if (string.IsNullOrEmpty(uk.Name) || uk.Columns.Any() == false) continue;
                        //获取C#中定义的索引名称
                        var ukname = ReplaceIndexName(uk.Name, tbname[1]);
                        //数据库和C#定义名称一致的
                        var dsukfind1 = dsuk.FirstOrDefault(a => string.Compare(a[1], ukname, true) == 0);
                        
                        if (dsukfind1==null || //没有找到一致的索引，表示新定义的
                            dsukfind1[0].Split(',').Length!=uk.Columns.Length ||
                            uk.Columns.Any(x=>dsukfind1[0].ToLower().IndexOf( x.Column.Attribute.Name.ToLower())==-1) ||
                            uk.IsUnique != (dsukfind1[2]== "1")
                            )
                        {
                            if (dsukfind1 != null) sbalter.Append("DROP INDEX ").Append($"\"{dsukfind1[4]}\".\"{dsukfind1[3]}\".\"{ukname}\"").Append(";\r\n");
                            sbalter.Append("CREATE ");
                            if (uk.IsUnique) sbalter.Append("UNIQUE ");
                            sbalter.Append("INDEX ").Append(_commonUtils.QuoteSqlName(ukname)).Append(" ON ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append("(");
                            foreach (var tbcol in uk.Columns)
                            {
                                sbalter.Append(_commonUtils.QuoteSqlName(tbcol.Column.Attribute.Name));
                                //if (tbcol.IsDesc) sbalter.Append(" DESC");
                                sbalter.Append(", ");
                            }
                            sbalter.Remove(sbalter.Length - 2, 2).Append(");\r\n");
                        }
                    }
                }
                if (istmpatler == false)
                {
                    //描述
                    var dbcomment = string.Concat(_orm.Ado.ExecuteScalar(CommandType.Text, _commonUtils.FormatSql($@" 
select COMMENTS,TABLE_NAME,SCHEMA_NAME from all_tables as a
left join all_schemas as b on a.SCHEMA_ID=b.SCHEMA_ID
WHERE a.Table_Name={{1}} and b.SCHEMA_NAME={{0}}
",tbname)));
                    if ((dbcomment??"") != (tb.Comment ?? ""))
                        sbalter.Append("COMMENT ON TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tb.Comment)).Append(";\r\n");

                    sb.Append(sbalter);
                    continue;
                }

                //约束primary key

                var oldpk = _orm.Ado.ExecuteScalar(CommandType.Text, _commonUtils.FormatSql(@" 
select cons_name from all_constraints as a
left join all_tables as b on a.Table_ID=b.Table_ID
left Join all_SCHEMAS AS c on b.SCHEMA_ID=c.SCHEMA_ID
where b.TABLE_NAME={0} and c.SCHEMA_NAME={1} and a.cons_TYPE='P'
", tbname))?.ToString();
                if (string.IsNullOrEmpty(oldpk) == false)
                    sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" DROP CONSTRAINT ").Append(oldpk).Append(";\r\n");

                //创建临时表，数据导进临时表，然后删除原表，将临时表改名为原表名
                var tablename = tboldname == null ? _commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}") : _commonUtils.QuoteSqlName($"{tboldname[0]}.{tboldname[1]}");

                //创建临时表
                var tmptablename = _commonUtils.QuoteSqlName($"{tbname[0]}.FreeSqlTmp_{tbname[1]}");
                sb.Append("CREATE TABLE IF NOT EXISTS ").Append(tmptablename).Append(" ( ");
                foreach (var tbcol in tb.ColumnsByPosition)
                {
                    sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType);
                    if (tbcol.Attribute.IsIdentity == true)
                    {
                        //sb.Append(" identity(1,1)");
                         seqcols.Add(NativeTuple.Create(tbcol, tbname, true));
                    }
                    sb.Append(",");
                }



                if (tb.Primarys.Any())
                {
                    var pkname = $"{tbname[0]}_{tbname[1]}_pkey";
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
                        sb.Append("COMMENT ON COLUMN ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.FreeSqlTmp_{tbname[1]}.{tbcol.Attribute.Name}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment)).Append(";\r\n");
                }
                if (string.IsNullOrEmpty(tb.Comment) == false)
                    sb.Append("COMMENT ON TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.FreeSqlTmp_{tbname[1]}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tb.Comment)).Append(";\r\n");

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
                            insertvalue = $"cast({insertvalue} as {tbcol.Attribute.DbType.Split(' ').First()})";
                        if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable)
                            insertvalue = $"coalesce({insertvalue},{tbcol.DbDefaultValue})";
                    }
                    else if (tbcol.Attribute.IsNullable == false)
                        insertvalue = tbcol.DbDefaultValue;
                    sb.Append(insertvalue).Append(", ");
                }
                sb.Remove(sb.Length - 2, 2).Append(" FROM ").Append(tablename).Append(";\r\n");
                sb.Append("DROP TABLE ").Append(tablename).Append(";\r\n");
                sb.Append("ALTER TABLE ").Append(tmptablename).Append(" RENAME TO ").Append(_commonUtils.QuoteSqlName(tbname[1])).Append(";\r\n");
                //创建表的索引
                foreach (var uk in tb.Indexes)
                {
                    sb.Append("CREATE ");
                    if (uk.IsUnique) sb.Append("UNIQUE ");
                    sb.Append("INDEX ").Append(_commonUtils.QuoteSqlName(ReplaceIndexName(uk.Name, tbname[1]))).Append(" ON ").Append(tablename).Append("(");
                    foreach (var tbcol in uk.Columns)
                    {
                        sb.Append(_commonUtils.QuoteSqlName(tbcol.Column.Attribute.Name));
                        if (tbcol.IsDesc) sb.Append(" DESC");
                        sb.Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2).Append(");\r\n");
                }

                
            }


            //foreach (var seqcol in seqcols)
            //{
            //    var tbname = seqcol.Item2;
            //    var seqname = Utils.GetCsName($"{tbname[0]}.{tbname[1]}_{seqcol.Item1.Attribute.Name}_sequence_name").ToLower();
            //    var tbname2 = _commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}");
            //    var colname2 = _commonUtils.QuoteSqlName(seqcol.Item1.Attribute.Name);
            //    sb.Append("ALTER TABLE ").Append(tbname2).Append(" ALTER COLUMN ").Append(colname2).Append(" SET DEFAULT null;\r\n");
            //    sb.Append("DROP SEQUENCE IF EXISTS ").Append(seqname).Append(";\r\n");
            //    if (seqcol.Item3)
            //    {
            //        sb.Append("CREATE SEQUENCE ").Append(seqname).Append(";\r\n");
            //        sb.Append("ALTER TABLE ").Append(tbname2).Append(" ALTER COLUMN ").Append(colname2).Append(" SET DEFAULT nextval('").Append(seqname).Append("'::regclass);\r\n");
            //        sb.Append(" SELECT case when max(").Append(colname2).Append(") is null then 0 else setval('").Append(seqname).Append("', max(").Append(colname2).Append(")) end FROM ").Append(tbname2).Append(";\r\n");
            //    }
            //}
            Console.Write(sb.ToString());
            //throw new Exception(sb.ToString());
            return sb.Length == 0 ? null : sb.ToString();
        }
    }
}