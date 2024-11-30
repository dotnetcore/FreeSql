using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using DuckDB.NET.Native;
using System.Collections;

namespace FreeSql.Duckdb
{

    class DuckdbCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {

        public override bool IsNoneCommandParameter { get => true; set => base.IsNoneCommandParameter = true; }
        public DuckdbCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm, commonUtils, commonExpression) { }

        static object _dicCsToDbLock = new object();
        static Dictionary<string, CsToDb<DuckDBType>> _dicCsToDb = new Dictionary<string, CsToDb<DuckDBType>>() {
                { typeof(bool).FullName, CsToDb.New(DuckDBType.Boolean, "boolean","boolean NOT NULL", null, false, false) },{ typeof(bool?).FullName, CsToDb.New(DuckDBType.Boolean, "boolean","boolean", null, true, null) },

                { typeof(sbyte).FullName, CsToDb.New(DuckDBType.TinyInt, "tinyint", "tinyint NOT NULL", false, false, 0) },{ typeof(sbyte?).FullName, CsToDb.New(DuckDBType.TinyInt, "tinyint", "tinyint", false, true, null) },
                { typeof(short).FullName, CsToDb.New(DuckDBType.SmallInt, "smallint","smallint NOT NULL", false, false, 0) },{ typeof(short?).FullName, CsToDb.New(DuckDBType.SmallInt, "smallint", "smallint", false, true, null) },
                { typeof(int).FullName, CsToDb.New(DuckDBType.Integer, "integer", "integer NOT NULL", false, false, 0) },{ typeof(int?).FullName, CsToDb.New(DuckDBType.Integer, "integer", "integer", false, true, null) },
                { typeof(long).FullName, CsToDb.New(DuckDBType.BigInt, "bigint","bigint NOT NULL", false, false, 0) },{ typeof(long?).FullName, CsToDb.New(DuckDBType.BigInt, "bigint","bigint", false, true, null) },

                { typeof(byte).FullName, CsToDb.New(DuckDBType.UnsignedTinyInt, "utinyint","utinyint NOT NULL", true, false, 0) },{ typeof(byte?).FullName, CsToDb.New(DuckDBType.UnsignedTinyInt, "utinyint","utinyint", true, true, null) },
                { typeof(ushort).FullName, CsToDb.New(DuckDBType.UnsignedSmallInt, "usmallint","usmallint NOT NULL", true, false, 0) },{ typeof(ushort?).FullName, CsToDb.New(DuckDBType.UnsignedSmallInt, "usmallint", "usmallint", true, true, null) },
                { typeof(uint).FullName, CsToDb.New(DuckDBType.UnsignedInteger, "uinteger", "uinteger NOT NULL", true, false, 0) },{ typeof(uint?).FullName, CsToDb.New(DuckDBType.UnsignedInteger, "uinteger", "uinteger", true, true, null) },
                { typeof(ulong).FullName, CsToDb.New(DuckDBType.UnsignedBigInt, "ubigint", "ubigint NOT NULL", true, false, 0) },{ typeof(ulong?).FullName, CsToDb.New(DuckDBType.UnsignedBigInt, "ubigint", "ubigint", true, true, null) },

                { typeof(double).FullName, CsToDb.New(DuckDBType.Double, "double", "double NOT NULL", false, false, 0) },{ typeof(double?).FullName, CsToDb.New(DuckDBType.Double, "double", "double", false, true, null) },
                { typeof(float).FullName, CsToDb.New(DuckDBType.Float, "float","float NOT NULL", false, false, 0) },{ typeof(float?).FullName, CsToDb.New(DuckDBType.Float, "float","float", false, true, null) },
                { typeof(decimal).FullName, CsToDb.New(DuckDBType.Decimal, "decimal", "decimal(10,2) NOT NULL", false, false, 0) },{ typeof(decimal?).FullName, CsToDb.New(DuckDBType.Decimal, "decimal", "decimal(10,2)", false, true, null) },

                { typeof(TimeSpan).FullName, CsToDb.New(DuckDBType.Time, "time","time NOT NULL", false, false, 0) },{ typeof(TimeSpan?).FullName, CsToDb.New(DuckDBType.Time, "time", "time",false, true, null) },
                { typeof(DateTime).FullName, CsToDb.New(DuckDBType.Timestamp, "timestamp", "timestamp NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateTime?).FullName, CsToDb.New(DuckDBType.Timestamp, "timestamp", "timestamp", false, true, null) },

#if net60
                { typeof(TimeOnly).FullName, CsToDb.New(DuckDBType.Time, "time", "time NOT NULL", false, false, TimeOnly.MinValue) },{ typeof(TimeOnly?).FullName, CsToDb.New(DuckDBType.Time, "time", "time", false, true, null) },
                { typeof(DateOnly).FullName, CsToDb.New(DuckDBType.Date, "date", "date NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateOnly?).FullName, CsToDb.New(DuckDBType.Date, "date", "date", false, true, null) },
#endif

                { typeof(byte[]).FullName, CsToDb.New(DuckDBType.Blob, "blob", "blob", false, null, new byte[0]) },
                { typeof(string).FullName, CsToDb.New(DuckDBType.Varchar, "varchar", "varchar(255)", false, null, "") },
                { typeof(char).FullName, CsToDb.New(DuckDBType.Varchar, "char", "char(1)", false, null, '\0') },

                { typeof(Guid).FullName, CsToDb.New(DuckDBType.Uuid, "uuid", "uuid NOT NULL", false, false, Guid.Empty) },{ typeof(Guid?).FullName, CsToDb.New(DuckDBType.Uuid, "uuid", "uuid", false, true, null) },

                { typeof(BitArray).FullName, CsToDb.New(DuckDBType.Bit, "bit", "bit", false, null, new BitArray(new byte[64])) },
                { typeof(BigInteger).FullName, CsToDb.New(DuckDBType.HugeInt, "hugeint","hugeint NOT NULL", false, false, 0) },{ typeof(BigInteger?).FullName, CsToDb.New(DuckDBType.HugeInt, "hugeint","hugeint", false, true, null) },
            };

        public override DbInfoResult GetDbInfo(Type type)
        {
            //int[]..List<int>
            var isarray = type.FullName != "System.Byte[]" && type.IsArray;
            Type elementType = isarray ? type.GetElementType().NullableTypeOrThis() : type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                isarray = true;
                elementType = type.GenericTypeArguments[0].NullableTypeOrThis();
            }
            var info = GetDbInfoNoneArray(elementType);
            if (info == null) return null;
            if (isarray)
            {
                var dbtypefull = Regex.Replace(info.dbtypeFull, $@"{info.dbtype}(\s*\([^\)]+\))?", "$0[]").Replace(" NOT NULL", "");
                return new DbInfoResult((int)(info.type | DuckDBType.Array), $"{info.dbtype}[]", dbtypefull, null, Array.CreateInstance(elementType, 0));
            }
            if (type.IsGenericType)
            {
                var typeDefinition = type.GetGenericTypeDefinition();
                //map
                if (typeDefinition == typeof(Dictionary<,>))
                {
                    var tkeyInfo = GetDbInfoNoneArray(type.GenericTypeArguments[0].NullableTypeOrThis());
                    if (tkeyInfo == null) return null;
                    var tvalInfo = GetDbInfoNoneArray(type.GenericTypeArguments[1].NullableTypeOrThis());
                    if (tvalInfo == null) return null;
                    var dbtype = $"map({tkeyInfo.dbtype},{tvalInfo.dbtype})";
                    return new DbInfoResult((int)(info.type | DuckDBType.Array), dbtype, dbtype, null, Array.CreateInstance(elementType, 0));
                }
            }
            return new DbInfoResult((int)info.type, info.dbtype, info.dbtypeFull, info.isnullable, info.defaultValue);
        }
        CsToDb<DuckDBType> GetDbInfoNoneArray(Type type)
        {
            if (_dicCsToDb.TryGetValue(type.FullName, out var trydc)) return trydc;
            if (type.IsArray) return null;
            var enumType = type.IsEnum ? type : null;
            if (enumType == null && type.IsNullableType() && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments.First().IsEnum) enumType = type.GenericTypeArguments.First();
            if (enumType != null)
            {
                var newItem = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any() ?
                    CsToDb.New(DuckDBType.BigInt, "bigint", $"bigint{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, enumType.CreateInstanceGetDefaultValue()) :
                    CsToDb.New(DuckDBType.Integer, "integer", $"integer{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, enumType.CreateInstanceGetDefaultValue());
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

        protected override string GetComparisonDDLStatements(params TypeSchemaAndName[] objects)
        {
            var sb = new StringBuilder();
            var seqcols = new List<NativeTuple<ColumnInfo, string[], bool>>(); //序列
            foreach (var obj in objects)
            {
                if (sb.Length > 0) sb.Append("\r\n");
                var tb = obj.tableSchema;
                if (tb == null) throw new Exception(CoreErrorStrings.S_Type_IsNot_Migrable(obj.tableSchema.Type.FullName));
                if (tb.Columns.Any() == false) throw new Exception(CoreErrorStrings.S_Type_IsNot_Migrable_0Attributes(obj.tableSchema.Type.FullName));
                var tbname = _commonUtils.SplitTableName(tb.DbName);
                if (tbname?.Length == 1) tbname = new[] { "main", tbname[0] };

                var tboldname = _commonUtils.SplitTableName(tb.DbOldName); //旧表名
                if (tboldname?.Length == 1) tboldname = new[] { "main", tboldname[0] };
                if (string.IsNullOrEmpty(obj.tableName) == false)
                {
                    var tbtmpname = _commonUtils.SplitTableName(obj.tableName);
                    if (tbtmpname?.Length == 1) tbtmpname = new[] { "main", tbtmpname[0] };
                    if (tbname[0] != tbtmpname[0] || tbname[1] != tbtmpname[1])
                    {
                        tbname = tbtmpname;
                        tboldname = null;
                    }
                }

                var sbalter = new StringBuilder();
                if (_orm.Ado.ExecuteScalar(CommandType.Text, $" select 1 from {tbname[0]}.sqlite_master where type='table' and name='{tbname[1]}'") == null)
                { //表不存在
                    if (tboldname != null)
                    {
                        if (_orm.Ado.ExecuteScalar(CommandType.Text, $" select 1 from {tboldname[0]}.sqlite_master where type='table' and name='{tboldname[1]}'") == null)
                            //模式或表不存在
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
                            if (tbcol.Attribute.IsIdentity == true)
                                seqcols.Add(NativeTuple.Create(tbcol, tbname, true));
                            sb.Append(",");
                        }
                        if (tb.Primarys.Any())
                        {
                            sb.Append(" \r\n  PRIMARY KEY (");
                            foreach (var tbcol in tb.Primarys) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                            sb.Remove(sb.Length - 2, 2).Append("),");
                        }
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append("\r\n) \r\n;\r\n");
                        //创建表的索引
                        foreach (var uk in tb.Indexes)
                        {
                            sb.Append("CREATE ");
                            if (uk.IsUnique) sb.Append("UNIQUE ");
                            sb.Append("INDEX ").Append(_commonUtils.QuoteSqlName(tbname[0], ReplaceIndexName(uk.Name, tbname[1]))).Append(" ON ").Append(tbname[1]).Append("(");
                            foreach (var tbcol in uk.Columns)
                            {
                                sb.Append(_commonUtils.QuoteSqlName(tbcol.Column.Attribute.Name));
                                if (tbcol.IsDesc) sb.Append(" DESC");
                                sb.Append(", ");
                            }
                            sb.Remove(sb.Length - 2, 2).Append(");\r\n");
                        }
                        //创建表的索引
                        foreach (var uk in tb.Indexes)
                        {
                            sb.Append("CREATE ");
                            if (uk.IsUnique) sb.Append("UNIQUE ");
                            sb.Append("INDEX ");
                            sb.Append(_commonUtils.QuoteSqlName(ReplaceIndexName(uk.Name, tbname[1]))).Append(" ON ").Append(createTableName);
                            sb.Append("(");
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
                        sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tboldname[0], tboldname[1])).Append(" RENAME TO \"").Append(tbname[1]).Append("\";\r\n");
                    else
                    {
                        //如果新表，旧表不在一起，创建新表，导入数据，删除旧表
                    }
                }
                else
                    tboldname = null; //如果新表已经存在，不走改表名逻辑
            }
            foreach (var seqcol in seqcols)
            {
                var tbname = seqcol.Item2;
                var seqname = Internal.Utils.GetCsName($"{tbname[0]}.{tbname[1]}_{seqcol.Item1.Attribute.Name}_sequence_name").ToLower();
                var tbname2 = _commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}");
                var colname2 = _commonUtils.QuoteSqlName(seqcol.Item1.Attribute.Name);
                sb.Append("ALTER TABLE ").Append(tbname2).Append(" ALTER COLUMN ").Append(colname2).Append(" SET DEFAULT null;\r\n");
                sb.Append("DROP SEQUENCE IF EXISTS ").Append(seqname).Append(";\r\n");
                if (seqcol.Item3)
                {
                    sb.Append("CREATE SEQUENCE ").Append(seqname).Append(";\r\n");
                    sb.Append("ALTER TABLE ").Append(tbname2).Append(" ALTER COLUMN ").Append(colname2).Append(" SET DEFAULT nextval('").Append(seqname).Append("');\r\n");
                    //sb.Append(" SELECT case when max(").Append(colname2).Append(") is null then 0 else setval('").Append(seqname).Append("', max(").Append(colname2).Append(")) end FROM ").Append(tbname2).Append(";\r\n");
                }
            }
            return sb.Length == 0 ? null : sb.ToString();
        }
    }
}