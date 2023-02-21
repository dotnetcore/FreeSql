using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Provider.QuestDb.Subtable;
using Newtonsoft.Json.Linq;
using Npgsql.LegacyPostgis;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.QuestDb
{
    class QuestDbCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {
        public QuestDbCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm,
            commonUtils, commonExpression)
        {
        }

        static object _dicCsToDbLock = new object();

        static Dictionary<string, CsToDb<NpgsqlDbType>> _dicCsToDb = new Dictionary<string, CsToDb<NpgsqlDbType>>()
        {
            { typeof(sbyte).FullName, CsToDb.New(NpgsqlDbType.Smallint, "byte", "byte NOT NULL", false, false, 0) },
            { typeof(sbyte?).FullName, CsToDb.New(NpgsqlDbType.Smallint, "byte", "byte", false, true, null) },
            { typeof(short).FullName, CsToDb.New(NpgsqlDbType.Smallint, "short", "short NOT NULL", false, false, 0) },
            { typeof(short?).FullName, CsToDb.New(NpgsqlDbType.Smallint, "short", "short", false, true, null) },
            { typeof(int).FullName, CsToDb.New(NpgsqlDbType.Integer, "int", "int NOT NULL", false, false, 0) },
            { typeof(int?).FullName, CsToDb.New(NpgsqlDbType.Integer, "int", "int", false, true, null) },
            { typeof(long).FullName, CsToDb.New(NpgsqlDbType.Bigint, "long", "long NOT NULL", false, false, 0) },
            { typeof(long?).FullName, CsToDb.New(NpgsqlDbType.Bigint, "long", "long", false, true, null) },

            { typeof(byte).FullName, CsToDb.New(NpgsqlDbType.Smallint, "byte", "byte NOT NULL", false, false, 0) },
            { typeof(byte?).FullName, CsToDb.New(NpgsqlDbType.Smallint, "byte", "byte", false, true, null) },
            { typeof(ushort).FullName, CsToDb.New(NpgsqlDbType.Integer, "short", "short NOT NULL", false, false, 0) },
            { typeof(ushort?).FullName, CsToDb.New(NpgsqlDbType.Integer, "short", "short", false, true, null) },
            { typeof(uint).FullName, CsToDb.New(NpgsqlDbType.Bigint, "int", "int NOT NULL", false, false, 0) },
            { typeof(uint?).FullName, CsToDb.New(NpgsqlDbType.Bigint, "int", "int", false, true, null) },
            {
                typeof(ulong).FullName, CsToDb.New(NpgsqlDbType.Numeric, "long", "long NOT NULL", false, false, 0)
            },
            {
                typeof(ulong?).FullName, CsToDb.New(NpgsqlDbType.Numeric, "long", "long", false, true, null)
            },

            { typeof(float).FullName, CsToDb.New(NpgsqlDbType.Real, "float", "float NOT NULL", false, false, 0) },
            { typeof(float?).FullName, CsToDb.New(NpgsqlDbType.Real, "float", "float", false, true, null) },
            { typeof(double).FullName, CsToDb.New(NpgsqlDbType.Double, "double", "double NOT NULL", false, false, 0) },
            { typeof(double?).FullName, CsToDb.New(NpgsqlDbType.Double, "double", "double", false, true, null) },
            {
                typeof(decimal).FullName,
                CsToDb.New(NpgsqlDbType.Numeric, "double", "double NOT NULL", false, false, 0)
            },
            {
                typeof(decimal?).FullName,
                CsToDb.New(NpgsqlDbType.Numeric, "double", "double", false, true, null)
            },

            { typeof(string).FullName, CsToDb.New(NpgsqlDbType.Varchar, "string", "string", false, null, "") },
            { typeof(char).FullName, CsToDb.New(NpgsqlDbType.Char, "char", "char)", false, null, '\0') },

            {
                typeof(TimeSpan).FullName,
                CsToDb.New(NpgsqlDbType.Time, "timestamp", "timestamp NOT NULL", false, false, 0)
            },
            { typeof(TimeSpan?).FullName, CsToDb.New(NpgsqlDbType.Time, "timestamp", "timestamp", false, true, null) },
            {
                typeof(DateTime).FullName,
                CsToDb.New(NpgsqlDbType.Timestamp, "timestamp", "timestamp NOT NULL", false, false,
                    new DateTime(1970, 1, 1))
            },
            {
                typeof(DateTime?).FullName,
                CsToDb.New(NpgsqlDbType.Timestamp, "timestamp", "timestamp", false, true, null)
            },

            {
                typeof(bool).FullName,
                CsToDb.New(NpgsqlDbType.Boolean, "boolean", "boolean NOT NULL", null, false, false)
            },
            { typeof(bool?).FullName, CsToDb.New(NpgsqlDbType.Boolean, "boolean", "boolean", null, true, null) },
            //{ typeof(Byte[]).FullName, CsToDb.New(NpgsqlDbType.Bytea, "bytea", "bytea", false, null, new byte[0]) },
            //{
            //    typeof(BitArray).FullName,
            //    CsToDb.New(NpgsqlDbType.Varbit, "varbit", "varbit(64)", false, null, new BitArray(new byte[64]))
            //},
            {
                typeof(BigInteger).FullName,
                CsToDb.New(NpgsqlDbType.Numeric, "long", "long NOT NULL", false, false, 0)
            },
            {
                typeof(BigInteger?).FullName,
                CsToDb.New(NpgsqlDbType.Numeric, "long", "long", false, true, null)
            }
        };

        public override DbInfoResult GetDbInfo(Type type)
        {
            if (_dicCsToDb.TryGetValue(type.FullName, out var trydc))
                return new DbInfoResult((int)trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable,
                    trydc.defaultValue);
            if (type.IsArray)
                return null;
            return null;
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
                if (tb.Columns.Any() == false)
                    throw new Exception(CoreStrings.S_Type_IsNot_Migrable_0Attributes(obj.entityType.FullName));
                var tbnameArray = _commonUtils.SplitTableName(tb.DbName);
                var tbname = string.Empty;
                if (tbnameArray?.Length == 1) tbname = tbnameArray.FirstOrDefault();

                var tboldnameArray = _commonUtils.SplitTableName(tb.DbOldName);
                var tboldname = string.Empty;
                if (tboldnameArray?.Length == 1)
                    tboldname = tboldnameArray.FirstOrDefault();

                var sbalter = new StringBuilder();
                var allTable = _orm.Ado.Query<string>(CommandType.Text,
                    @"SHOW TABLES");
                //如果旧表名和现表名均不存在，则直接创建表
                if (string.IsNullOrWhiteSpace(tboldname) && allTable.Any(s => s.Equals(tbname)) == false)
                {
                    //创建表
                    var createTableName = _commonUtils.QuoteSqlName(tbname);
                    sbalter.Append("CREATE TABLE ").Append(createTableName).Append(" ( ");
                    foreach (var tbcol in tb.ColumnsByPosition)
                    {
                        sbalter.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ")
                            .Append(tbcol.Attribute.DbType);
                        if (tbcol.Attribute.IsIdentity == true)
                        {
                            //No IsIdentity
                        }

                        sbalter.Append(",");
                    }

                    sbalter.Remove(sbalter.Length - 1, 1);
                    sbalter.Append(") ");
                    if (tb.Indexes.Any())
                    {
                        sbalter.Append(",\r\n");
                    }

                    //创建表的索引
                    foreach (var uk in tb.Indexes)
                    {
                        sbalter.Append($"INDEX (");
                        foreach (var tbcol in uk.Columns)
                        {
                            if (tbcol.Column.Attribute.DbType != "SYMBOL")
                                throw new Exception("索引只能是string类型且[Column(DbType = \"symbol\")]");
                            sbalter.Append($"{_commonUtils.QuoteSqlName(tbcol.Column.Attribute.Name)}");
                        }

                        sbalter.Append($"),");
                    }

                    sbalter.Remove(sbalter.Length - 1, 1);
                    //是否存在分表
                    foreach (var propety in obj.entityType.GetProperties())
                    {
                        var timeAttr = propety.GetCustomAttribute<AutoSubtableAttribute>();
                        if (timeAttr != null)
                        {
                            var colName = tb.Columns.FirstOrDefault(it => it.Key == propety.Name).Value;
                            sbalter.Append(
                                $" TIMESTAMP({colName.Attribute.Name}) PARTITION BY {timeAttr.SubtableType};{Environment.NewLine}");
                        }
                    }
                }
                //如果旧表名特性存在，旧表名在数据库中存在，现表名在数据库中不存在，直接走修改表名逻辑
                else if (string.IsNullOrWhiteSpace(tboldname) == false && allTable.Any(s => s.Equals(tboldname)) &&
                         allTable.Any(s => s.Equals(tbname)) == false)
                {
                    //修改表名
                    sbalter.Append("RENAME TABLE ")
                        .Append(_commonUtils.QuoteSqlName(tboldname))
                        .Append(" TO ").Append(_commonUtils.QuoteSqlName(tbname))
                        .Append($";{Environment.NewLine}");
                }
                //如果旧表名特性存在，旧表名在数据库中不存在，现表名在数据库中存在，对比列
                //如果旧表名特性不存在 现表名在数据库中存在，对比列
                else if ((string.IsNullOrWhiteSpace(tboldname) == false &&
                          allTable.Any(s => s.Equals(tboldname)) == false &&
                          allTable.Any(s => s.Equals(tbname)) == true)
                         || (string.IsNullOrWhiteSpace(tboldname) == true &&
                             allTable.Any(s => s.Equals(tbname)) == true))

                {
                    //查询列
                    var questDbColumnInfo = _orm.Ado.ExecuteArray($"SHOW COLUMNS FROM '{tbname}'").Select(o => new
                    {
                        columnName = o[0].ToString(),
                        indexed = Convert.ToBoolean(o[2])
                    }).ToList();
                    //对比列
                    foreach (var tbcol in tb.ColumnsByPosition)
                    {
                        //如果旧列名存在 现列名均不存在  直接添加列
                        if (questDbColumnInfo.Any(a => a.columnName.Equals(tbcol.Attribute.OldName)) == false
                            && questDbColumnInfo.Any(a => a.columnName.Equals(tbcol.Attribute.Name)) ==
                            false)
                        {
                            sbalter.Append("ALTER TABLE ").Append(tbname)
                                .Append(" ADD COLUMN ").Append(tbcol.Attribute.Name).Append(" ")
                                .Append(tbcol.Attribute.DbType).Append($";{Environment.NewLine}");
                            questDbColumnInfo.Add(new
                            {
                                columnName = tbcol.Attribute.Name,
                                indexed = false
                            });
                        }
                        //如果旧列名存在，现列名不存在，直接修改列名
                        else if (questDbColumnInfo.Any(a =>
                                     a.columnName.ToString().Equals(tbcol.Attribute.OldName)) == true
                                 && questDbColumnInfo.Any(a => a.columnName.ToString().Equals(tbcol.Attribute.Name)) ==
                                 false)
                        {
                            sbalter.Append("ALTER TABLE ").Append(tbname)
                                .Append(" RENAME COLUMN ").Append(tbcol.Attribute.OldName).Append(" TO ")
                                .Append(tbcol.Attribute.Name).Append($";{Environment.NewLine}");
                        }
                    }

                    //对比索引
                    foreach (var uk in tb.Indexes)
                    {
                        if (string.IsNullOrEmpty(uk.Name) || uk.Columns.Any() == false)
                            continue;
                        var ukname = ReplaceIndexName(uk.Name, tbname);
                        //先判断表中有没此字段的索引
                        var isIndex = questDbColumnInfo
                            .Where(a => a.columnName.ToString().Equals(uk.Columns.First().Column.Attribute.Name))
                            .FirstOrDefault()?.indexed;
                        //如果此字段不是索引
                        if (isIndex != null && isIndex == false)
                        {
                            //创建索引
                            sbalter.Append($"ALTER TABLE {tbname} ALTER COLUMN ");
                            foreach (var tbcol in uk.Columns)
                            {
                                if (tbcol.Column.Attribute.DbType != "SYMBOL")
                                    throw new Exception("索引只能是string类型且[Column(DbType = \"symbol\")]");
                                sbalter.Append($"{tbcol.Column.Attribute.Name}");
                            }

                            sbalter.Append($" ADD INDEX;{Environment.NewLine}");
                        }
                    }
                }

                sb.Append(sbalter);
            }

            return sb.Length == 0 ? null : sb.ToString();
        }
    }
}