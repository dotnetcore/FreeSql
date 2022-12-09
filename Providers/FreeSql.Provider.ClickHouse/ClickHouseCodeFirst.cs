using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Common;
using FreeSql.Internal.ObjectPool;
using ClickHouse.Client.ADO;

namespace FreeSql.ClickHouse
{
    class ClickHouseCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {
        public ClickHouseCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm,
            commonUtils, commonExpression)
        {
        }

        static object _dicCsToDbLock = new object();

        static Dictionary<string, CsToDb<DbType>> _dicCsToDb = new Dictionary<string, CsToDb<DbType>>()
        {
            { typeof(bool).FullName, CsToDb.New(DbType.SByte, "Int8", "Int8", null, false, false) },
            { typeof(bool?).FullName, CsToDb.New(DbType.SByte, "Int8", "Nullable(Int8)", null, true, null) },

            { typeof(sbyte).FullName, CsToDb.New(DbType.SByte, "Int8", "Int8", false, false, 0) },
            { typeof(sbyte?).FullName, CsToDb.New(DbType.SByte, "Int8", "Nullable(Int8)", false, true, null) },
            { typeof(short).FullName, CsToDb.New(DbType.Int16, "Int16", "Int16", false, false, 0) },
            { typeof(short?).FullName, CsToDb.New(DbType.Int16, "Int16", "Nullable(Int16)", false, true, null) },
            { typeof(int).FullName, CsToDb.New(DbType.Int32, "Int32", "Int32", false, false, 0) },
            { typeof(int?).FullName, CsToDb.New(DbType.Int32, "Int32", "Nullable(Int32)", false, true, null) },
            { typeof(long).FullName, CsToDb.New(DbType.Int64, "Int64", "Int64", false, false, 0) },
            { typeof(long?).FullName, CsToDb.New(DbType.Int64, "Int64", "Nullable(Int64)", false, true, null) },

            { typeof(byte).FullName, CsToDb.New(DbType.Byte, "UInt8", "UInt8", true, false, 0) },
            { typeof(byte?).FullName, CsToDb.New(DbType.Byte, "UInt8", "Nullable(UInt8)", true, true, null) },
            { typeof(ushort).FullName, CsToDb.New(DbType.UInt16, "UInt16", "UInt16", true, false, 0) },
            { typeof(ushort?).FullName, CsToDb.New(DbType.UInt16, "UInt16", "Nullable(UInt16)", true, true, null) },
            { typeof(uint).FullName, CsToDb.New(DbType.UInt32, "UInt32", "UInt32", true, false, 0) },
            { typeof(uint?).FullName, CsToDb.New(DbType.UInt32, "UInt32", "Nullable(UInt32)", true, true, null) },
            { typeof(ulong).FullName, CsToDb.New(DbType.UInt64, "UInt64", "UInt64", true, false, 0) },
            { typeof(ulong?).FullName, CsToDb.New(DbType.UInt64, "UInt64", "Nullable(UInt64)", true, true, null) },

            { typeof(double).FullName, CsToDb.New(DbType.Double, "Float64", "Float64", false, false, 0) },
            { typeof(double?).FullName, CsToDb.New(DbType.Double, "Float64", "Nullable(Float64)", false, true, null) },
            { typeof(float).FullName, CsToDb.New(DbType.Single, "Float32", "Float32", false, false, 0) },
            { typeof(float?).FullName, CsToDb.New(DbType.Single, "Float32", "Nullable(Float32)", false, true, null) },
            {
                typeof(decimal).FullName,
                CsToDb.New(DbType.Decimal, "Decimal(38, 19)", "Decimal(38, 19)", false, false, 0)  //Nullable(Decimal(38, 19))
            },
            {
                typeof(decimal?).FullName,
                CsToDb.New(DbType.Decimal, "Nullable(Decimal(38, 19))", "Nullable(Decimal(38, 19))", false, true, null)
            },

            {
                typeof(DateTime).FullName,
                CsToDb.New(DbType.DateTime, "DateTime('Asia/Shanghai')", "DateTime('Asia/Shanghai')", false, false,
                    new DateTime(1970, 1, 1))
            },
            {
                typeof(DateTime?).FullName,
                CsToDb.New(DbType.DateTime, "DateTime('Asia/Shanghai')", "Nullable(DateTime('Asia/Shanghai'))", false,
                    true, null)
            },

            { typeof(string).FullName, CsToDb.New(DbType.String, "String", "String", false, null, "") },
            { typeof(char).FullName, CsToDb.New(DbType.String, "String", "String", false, false, "") },
            { typeof(char?).FullName, CsToDb.New(DbType.Single, "String", "Nullable(String)", false, true, null) },
            { typeof(Guid).FullName, CsToDb.New(DbType.String, "String", "String", false, false, Guid.Empty) },
            { typeof(Guid?).FullName, CsToDb.New(DbType.String, "String", "Nullable(String)", false, true, null) },
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
            Object<DbConnection> conn = null;
            string database = null;

            try
            {
                conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5));
                database = conn.Value.Database;

                var sb = new StringBuilder();
                foreach (var obj in objects)
                {
                    if (sb.Length > 0)
                        sb.Append("\r\n");
                    var tb = _commonUtils.GetTableByEntity(obj.entityType);
                    if (tb == null)
                        throw new Exception(CoreStrings.S_Type_IsNot_Migrable(obj.entityType.FullName));
                    if (tb.Columns.Any() == false)
                        throw new Exception(CoreStrings.S_Type_IsNot_Migrable_0Attributes(obj.entityType.FullName));
                    var tbname = _commonUtils.SplitTableName(tb.DbName);
                    if (tbname?.Length == 1)
                        tbname = new[] { database, tbname[0] };

                    var tboldname = _commonUtils.SplitTableName(tb.DbOldName); //旧表名
                    if (tboldname?.Length == 1)
                        tboldname = new[] { database, tboldname[0] };
                    if (string.IsNullOrEmpty(obj.tableName) == false)
                    {
                        var tbtmpname = _commonUtils.SplitTableName(obj.tableName);
                        if (tbtmpname?.Length == 1)
                            tbtmpname = new[] { database, tbtmpname[0] };
                        if (tbname[0] != tbtmpname[0] || tbname[1] != tbtmpname[1])
                        {
                            tbname = tbtmpname;
                            tboldname = null;
                        }
                    }

                    if (string.Compare(tbname[0], database, true) != 0 && LocalExecuteScalar(database,
                            _commonUtils.FormatSql(" select 1 from system.databases d where name={0}", tbname[0])) ==
                        null) //创建数据库
                        sb.Append($"CREATE DATABASE IF NOT EXISTS ").Append(_commonUtils.QuoteSqlName(tbname[0]))
                            .Append(" ENGINE=Ordinary;\r\n");

                    var sbalter = new StringBuilder();
                    var istmpatler = false; //创建临时表，导入数据，删除旧表，修改
                    if (LocalExecuteScalar(tbname[0],
                            _commonUtils.FormatSql(" SELECT 1 FROM system.tables t WHERE database ={0} and name ={1}",
                                tbname)) == null)
                    {
                        //表不存在
                        if (tboldname != null)
                        {
                            if (string.Compare(tboldname[0], tbname[0], true) != 0 && LocalExecuteScalar(database,
                                    _commonUtils.FormatSql(" select 1 from system.databases where name={0}",
                                        tboldname[0])) == null ||
                                LocalExecuteScalar(tboldname[0],
                                    _commonUtils.FormatSql(
                                        " SELECT 1 FROM system.tables WHERE database={0} and name={1}", tboldname)) ==
                                null)
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
                                //如果这里是主键就是不为Nullable
                                tbcol.Attribute.DbType =
                                    CkNullableAdapter(tbcol.Attribute.DbType, tbcol.Attribute.IsPrimary);
                                tbcol.Attribute.DbType = CkIntAdapter(tbcol.Attribute.DbType);
                                sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ")
                                    .Append(tbcol.Attribute.DbType);
                                if (string.IsNullOrEmpty(tbcol.Comment) == false)
                                    sb.Append(" COMMENT ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment));
                                sb.Append(",");
                            }

                            foreach (var uk in tb.Indexes)
                            {
                                sb.Append(" \r\n  ");
                                sb.Append("INDEX ")
                                    .Append(_commonUtils.QuoteSqlName(ReplaceIndexName(uk.Name, tbname[1])));

                                sb.Append(" (");
                                foreach (var tbcol in uk.Columns)
                                {
                                    sb.Append(_commonUtils.QuoteSqlName(tbcol.Column.Attribute.Name));
                                    sb.Append(", ");
                                }

                                sb.Remove(sb.Length - 2, 2).Append(") ");
                                sb.Append("TYPE set(8192) GRANULARITY 5,");
                            }

                            sb.Remove(sb.Length - 1, 1);
                            sb.Append("\r\n) ");
                            sb.Append("\r\nENGINE = MergeTree()");

                            if (tb.Primarys.Any())
                            {
                                sb.Append(" \r\nORDER BY ( ");
                                var ls = new StringBuilder();
                                foreach (var tbcol in tb.Primarys)
                                    ls.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                                sb.Append(ls);
                                sb.Remove(sb.Length - 2, 2);
                                sb.Append(" )");
                                sb.Append(" \r\nPRIMARY KEY ");
                                sb.Append(ls);
                                sb.Remove(sb.Length - 2, 2).Append(",");
                            }

                            sb.Remove(sb.Length - 1, 1);
                            //if (string.IsNullOrEmpty(tb.Comment) == false)
                            //    sb.Append(" Comment=").Append(_commonUtils.FormatSql("{0}", tb.Comment));
                            sb.Append(" SETTINGS index_granularity = 8192;\r\n");
                            continue;
                        }

                        //如果新表，旧表在一个数据库下，直接修改表名
                        if (string.Compare(tbname[0], tboldname[0], true) == 0)
                            sbalter.Append("RENAME TABLE ")
                                .Append(_commonUtils.QuoteSqlName(tboldname[0], tboldname[1]))
                                .Append(" TO ").Append(_commonUtils.QuoteSqlName(tbname[0], tbname[1]))
                                .Append(";\r\n");
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
a.name,
a.type,
if(ilike(a.`type`, 'Nullable(%S%)'),'is_nullable','0'),
a.comment as comment,
a.is_in_partition_key,
a.is_in_sorting_key,
a.is_in_primary_key,
a.is_in_sampling_key
from system.columns a
where a.database in ({0}) and a.table in ({1})", tboldname ?? tbname);
                    var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                    var tbstruct = ds.ToDictionary(a => string.Concat(a[0]), a =>
                    {
                        return new
                        {
                            column = string.Concat(a[0]),
                            sqlType = (string)a[1],
                            is_nullable = a[1]?.ToString().Contains("Nullable"),
                            is_identity = false,
                            comment = string.Concat(a[3]),
                            is_primary = string.Concat(a[6]) == "1",
                        };
                    }, StringComparer.CurrentCultureIgnoreCase);


                    if (istmpatler == false)
                    {
                        var existsPrimary = tbstruct.Any(o => o.Value.is_primary);
                        //对比列
                        foreach (var tbcol in tb.ColumnsByPosition)
                        {
                            //表中有这个字段
                            var condition1 = tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol);
                            var condition2 = string.IsNullOrEmpty(tbcol.Attribute.OldName) == false;
                            if (condition1 ||
                                condition2 && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol))
                            {
                                var isCommentChanged = tbstructcol.comment != (tbcol.Comment ?? "");
                                var ckDbType = CkNullableAdapter(tbcol.Attribute.DbType, tbcol.Attribute.IsPrimary);
                                ckDbType = CkIntAdapter(ckDbType);
                                var typeCondition1 = RemoveSpaceComparison(tbstructcol.sqlType, ckDbType) == false;
                                var typeCondition2 = tbcol.Attribute.IsNullable != tbstructcol.is_nullable;
                                if (typeCondition1 || typeCondition1 || isCommentChanged)
                                {
                                    sbalter.Append("ALTER TABLE ")
                                        .Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}"))
                                        .Append(" MODIFY COLUMN ").Append(_commonUtils.QuoteSqlName(tbstructcol.column))
                                        .Append(tbcol.Attribute.IsNullable &&
                                                tbcol.Attribute.DbType.Contains("Nullable") == false
                                            ? $"Nullable({tbcol.Attribute.DbType.Split(' ').First()})"
                                            : tbcol.Attribute.DbType.Split(' ').First())
                                        .Append(";\r\n");
                                }

                                if (isCommentChanged)
                                    sbalter.Append("ALTER TABLE ")
                                        .Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}"))
                                        .Append(" COMMENT COLUMN ")
                                        .Append(_commonUtils.QuoteSqlName(tbstructcol.column))
                                        .Append(_commonUtils.FormatSql("{0}", tbcol.Comment ?? "")).Append(";\r\n");
                                if (string.Compare(tbstructcol.column, tbcol.Attribute.OldName, true) == 0)
                                    sbalter.Append("ALTER TABLE ")
                                        .Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}"))
                                        .Append(" RENAME COLUMN ").Append(_commonUtils.QuoteSqlName(tbstructcol.column))
                                        .Append(" TO ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name))
                                        .Append(";\r\n");
                                continue;
                            }

                            //添加列
                            sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tbname[0], tbname[1]))
                                .Append(" ADD Column ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ")
                                .Append(tbcol.Attribute.DbType);
                            if (tbcol.Attribute.IsNullable == false && tbcol.DbDefaultValue != "NULL" &&
                                tbcol.Attribute.IsIdentity == false)
                                sbalter.Append(" DEFAULT ").Append(tbcol.DbDefaultValue);
                            if (string.IsNullOrEmpty(tbcol.Comment) == false)
                                sbalter.Append(" COMMENT ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment ?? ""));
                            sbalter.Append(";\r\n");
                        }

                        var indexSelectSql = _commonUtils.FormatSql(
                            @"SELECT name,expr FROM system.data_skipping_indices WHERE database={0}",
                            tboldname ?? tbname);
                        var indexCollect = _orm.Ado.Query<ClickHouseTableIndex>(CommandType.Text, indexSelectSql);
                        //对比索引
                        foreach (var uk in tb.Indexes)
                        {
                            if (string.IsNullOrEmpty(uk.Name) || uk.Columns.Any() == false)
                                continue;
                            var ukname = ReplaceIndexName(uk.Name, tbname[1]);
                            //先判断表中有没此字段的索引
                            if (indexCollect.Any(c =>
                                    RemoveSpaceComparison(c.expr,
                                        string.Join(',', uk.Columns.Select(i => i.Column.CsName)))))
                            {
                                //有这个字段的索引，但是名称不一样 修改名 , ClickHouse不支持修改列
                                //if (!indexCollect.Where(c => c.name == uk.Name).Any())
                                //{
                                //    
                                //    sbalter.Append("ALTER TABLE ")
                                //        .Append(_commonUtils.QuoteSqlName(tbname[0], tbname[1]))
                                //        .Append(" DROP INDEX ").Append(ukname).Append(" ").Append(";\r\n");

                                //    //添加
                                //    sbalter.Append("ALTER TABLE ")
                                //        .Append(_commonUtils.QuoteSqlName(tbname[0], tbname[1]))
                                //        .Append(" ADD INDEX ").Append(ukname).Append(" (");
                                //    foreach (var tbcol in uk.Columns)
                                //    {
                                //        sbalter.Append(_commonUtils.QuoteSqlName(tbcol.Column.Attribute.Name));
                                //        sbalter.Append(", ");
                                //    }

                                //    sbalter.Remove(sbalter.Length - 2, 2).Append(") TYPE set(8192) GRANULARITY 5")
                                //        .Append(";\r\n");
                                //}
                            }
                            else
                            {
                                //创建索引
                                sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tbname[0], tbname[1]))
                                    .Append(" ADD INDEX ").Append(ukname).Append(" (");
                                foreach (var tbcol in uk.Columns)
                                {
                                    sbalter.Append(_commonUtils.QuoteSqlName(tbcol.Column.Attribute.Name));
                                    sbalter.Append(", ");
                                }

                                sbalter.Remove(sbalter.Length - 2, 2).Append(") TYPE set(8192) GRANULARITY 5")
                                    .Append(";\r\n");
                            }
                        }
                    }

                    if (istmpatler == false)
                    {
                        sb.Append(sbalter);
                        Console.WriteLine(sb.ToString());
                        continue;
                    }

                    //创建临时表，数据导进临时表，然后删除原表，将临时表改名为原表名
                    var tablename = tboldname == null
                        ? _commonUtils.QuoteSqlName(tbname[0], tbname[1])
                        : _commonUtils.QuoteSqlName(tboldname[0], tboldname[1]);
                    var tmptablename = _commonUtils.QuoteSqlName(tbname[0], $"FreeSqlTmp_{tbname[1]}");
                    //创建临时表
                    sb.Append("CREATE TABLE IF NOT EXISTS ").Append(tmptablename).Append(" ( ");
                    foreach (var tbcol in tb.ColumnsByPosition)
                    {
                        tbcol.Attribute.DbType = tbcol.Attribute.DbType.Replace(" NOT NULL", "");
                        sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ")
                            .Append(tbcol.Attribute.DbType);
                        if (string.IsNullOrEmpty(tbcol.Comment) == false)
                            sb.Append(" COMMENT ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment));
                        sb.Append(",");
                    }

                    foreach (var uk in tb.Indexes)
                    {
                        sb.Append(" \r\n  ");
                        sb.Append("INDEX ").Append(_commonUtils.QuoteSqlName(ReplaceIndexName(uk.Name, tbname[1])))
                            .Append("(");
                        foreach (var tbcol in uk.Columns)
                        {
                            sb.Append(_commonUtils.QuoteSqlName(tbcol.Column.Attribute.Name));
                            sb.Append("TYPE set(8192) GRANULARITY 5, ");
                        }

                        sb.Remove(sb.Length - 2, 2).Append("),");
                    }

                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("\r\n) ");
                    sb.Append("\r\nENGINE = MergeTree()");

                    if (tb.Primarys.Any())
                    {
                        sb.Append(" \r\nORDER BY ( ");
                        var ls = new StringBuilder();
                        foreach (var tbcol in tb.Primarys)
                            ls.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                        sb.Append(ls);
                        sb.Remove(sb.Length - 2, 2);
                        sb.Append(" )");
                        sb.Append(" \r\nPRIMARY KEY ");
                        sb.Append(ls);
                        sb.Remove(sb.Length - 2, 2).Append(",");
                    }

                    sb.Remove(sb.Length - 1, 1);
                    //if (string.IsNullOrEmpty(tb.Comment) == false)
                    //    sb.Append(" Comment=").Append(_commonUtils.FormatSql("{0}", tb.Comment));
                    sb.Append(" SETTINGS index_granularity = 8192;\r\n");

                    sb.Append("INSERT INTO ").Append(tmptablename).Append(" SELECT ");
                    foreach (var tbcol in tb.ColumnsByPosition)
                    {
                        var insertvalue = "NULL";
                        if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
                            string.IsNullOrEmpty(tbcol.Attribute.OldName) == false &&
                            tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol))
                        {
                            insertvalue = _commonUtils.QuoteSqlName(tbstructcol.column);
                            if (tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType,
                                    StringComparison.CurrentCultureIgnoreCase) == false)
                            {
                                //insertvalue = $"cast({insertvalue} as {tbcol.Attribute.DbType.Split(' ').First()})";
                            }

                            if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable)
                                insertvalue = $"ifnull({insertvalue},{tbcol.DbDefaultValue})";
                        }
                        else if (tbcol.Attribute.IsNullable == false)
                            if (tbcol.DbDefaultValue != "NULL")
                                insertvalue = tbcol.DbDefaultValue;

                        sb.Append(insertvalue).Append(", ");
                    }

                    sb.Remove(sb.Length - 2, 2).Append(" FROM ").Append(tablename).Append(";\r\n");
                    sb.Append("DROP TABLE ").Append(tablename).Append(";\r\n");
                    sb.Append("RENAME TABLE ").Append(tmptablename).Append(" TO ")
                        .Append(_commonUtils.QuoteSqlName(tbname[0], tbname[1])).Append(";\r\n");
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

            object LocalExecuteScalar(string db, string sql)
            {
                if (string.Compare(database, db) != 0)
                    conn.Value.ChangeDatabase(db);
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
                    if (string.Compare(database, db) != 0)
                        conn.Value.ChangeDatabase(database);
                }
            }

            string CkNullablePrimaryAdapter(string dbType, bool isPrimary)
            {
                return isPrimary
                    ? dbType.Replace("Nullable(", "").Replace(")", "")
                    : dbType.Replace(" NOT NULL", "");
            }
            string CkNullableAdapter(string dbType, bool isPrimary)
            {
                return isPrimary
                    ? dbType.Replace("Nullable(", "").Replace(")","").Replace(" NOT NULL", "")
                    : dbType.Replace(" NOT NULL", "");
            }


            string CkIntAdapter(string dbType)
            {
                var result = dbType;
                if (dbType.ToLower().Contains("int64"))
                {
                    if (dbType.Contains("Nullable"))
                    {
                        result = "Nullable(Int64)";
                    }
                    else
                    {
                        result = "Int64";
                    }
                }
                else if (dbType.ToLower().Contains("int"))
                {
                    if (dbType.Contains("Nullable"))
                    {
                        result = "Nullable(Int32)";
                    }
                    else
                    {
                        result = "Int32";
                    }
                }

                return result;
            }

            //去除空格后比较
            bool RemoveSpaceComparison(string a, string b)
            {
                a = Regex.Replace(a, @"\s", "").ToLower();
                b = Regex.Replace(b, @"\s", "").ToLower();
                return a == b;
            }
        }

        public override int ExecuteDDLStatements(string ddl)
        {
            if (string.IsNullOrEmpty(ddl))
                return 0;
            var scripts = ddl.Split(new string[] { ";\r\n" }, StringSplitOptions.None)
                .Where(a => string.IsNullOrEmpty(a.Trim()) == false).ToArray();

            if (scripts.Any() == false)
                return 0;

            var affrows = 0;
            foreach (var script in scripts)
                affrows += base.ExecuteDDLStatements(script);
            return affrows;
        }
    }

    internal class ClickHouseTableIndex
    {
        public string name
        {
            get; set;
        }
        public string expr
        {
            get; set;
        }
    }
}