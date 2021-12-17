using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FreeSql.DataAnnotations;
using FreeSql.Internal.ObjectPool;
using System.Data.Common;
using System.Data.Odbc;

namespace FreeSql.GBase
{

    class GBaseCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {

        public override bool IsNoneCommandParameter { get => true; set => base.IsNoneCommandParameter = true; }
        public GBaseCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm, commonUtils, commonExpression) { }

        static object _dicCsToDbLock = new object();
        static Dictionary<string, CsToDb<OdbcType>> _dicCsToDb = new Dictionary<string, CsToDb<OdbcType>>() {
                { typeof(sbyte).FullName, CsToDb.New(OdbcType.SmallInt, "smallint","smallint NOT NULL", false, false, 0) },{ typeof(sbyte?).FullName, CsToDb.New(OdbcType.SmallInt, "smallint", "smallint", false, true, null) },
                { typeof(short).FullName, CsToDb.New(OdbcType.SmallInt, "smallint","smallint NOT NULL", false, false, 0) },{ typeof(short?).FullName, CsToDb.New(OdbcType.SmallInt, "smallint", "smallint", false, true, null) },
                { typeof(int).FullName, CsToDb.New(OdbcType.Int, "integer","integer NOT NULL", false, false, 0) },{ typeof(int?).FullName, CsToDb.New(OdbcType.Int, "integer", "integer", false, true, null) },
                { typeof(long).FullName, CsToDb.New(OdbcType.BigInt, "bigint","bigint NOT NULL", false, false, 0) },{ typeof(long?).FullName, CsToDb.New(OdbcType.BigInt, "bigint", "bigint", false, true, null) },

                { typeof(byte).FullName, CsToDb.New(OdbcType.SmallInt, "smallint","smallint NOT NULL", false, false, 0) },{ typeof(byte?).FullName, CsToDb.New(OdbcType.SmallInt, "smallint", "smallint", false, true, null) },
                { typeof(ushort).FullName, CsToDb.New(OdbcType.Int, "integer","integer NOT NULL", false, false, 0) },{ typeof(ushort?).FullName, CsToDb.New(OdbcType.Int, "integer", "integer", false, true, null) },
                { typeof(uint).FullName, CsToDb.New(OdbcType.BigInt, "bigint","bigint NOT NULL", false, false, 0) },{ typeof(uint?).FullName, CsToDb.New(OdbcType.BigInt, "bigint", "bigint", false, true, null) },
                { typeof(ulong).FullName, CsToDb.New(OdbcType.Decimal, "decimal","decimal(20,0) NOT NULL", false, false, 0) },{ typeof(ulong?).FullName, CsToDb.New(OdbcType.Decimal, "decimal", "decimal(20,0)", false, true, null) },

                { typeof(float).FullName, CsToDb.New(OdbcType.Real, "real","real NOT NULL", false, false, 0) },{ typeof(float?).FullName, CsToDb.New(OdbcType.Real, "real", "real", false, true, null) },
                { typeof(double).FullName, CsToDb.New(OdbcType.Double, "float","float NOT NULL", false, false, 0) },{ typeof(double?).FullName, CsToDb.New(OdbcType.Double, "float", "float", false, true, null) },
                { typeof(decimal).FullName, CsToDb.New(OdbcType.Decimal, "decimal", "decimal(10,2) NOT NULL", false, false, 0) },{ typeof(decimal?).FullName, CsToDb.New(OdbcType.Decimal, "decimal", "decimal(10,2)", false, true, null) },

                { typeof(string).FullName, CsToDb.New(OdbcType.VarChar, "varchar", "varchar(255)", false, null, "") },

                { typeof(TimeSpan).FullName, CsToDb.New(OdbcType.Time, "interval day to fraction","interval day(3) to fraction(3) NOT NULL", false, false, 0) },{ typeof(TimeSpan?).FullName, CsToDb.New(OdbcType.Time, "interval day to fraction", "interval day(3) to fraction(3) NULL",false, true, null) },
                { typeof(DateTime).FullName, CsToDb.New(OdbcType.DateTime, "datetime year to fraction", "datetime year to fraction(3) NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateTime?).FullName, CsToDb.New(OdbcType.DateTime, "datetime year to fraction", "datetime year to fraction(3)", false, true, null) },

                { typeof(bool).FullName, CsToDb.New(OdbcType.Bit, "boolean","boolean NOT NULL", null, false, false) },{ typeof(bool?).FullName, CsToDb.New(OdbcType.Bit, "boolean","boolean", null, true, null) },
                { typeof(byte[]).FullName, CsToDb.New(OdbcType.VarBinary, "byte", "byte", false, null, new byte[0]) },

                { typeof(Guid).FullName, CsToDb.New(OdbcType.UniqueIdentifier, "char(36)", "char(36) NOT NULL", false, false, Guid.Empty) },{ typeof(Guid?).FullName, CsToDb.New(OdbcType.UniqueIdentifier, "char(36)", "char(36)", false, true, null) },
            };

        public override DbInfoResult GetDbInfo(Type type)
        {
            var info = GetDbInfoNoneArray(type);
            if (info == null) return null;
            return new DbInfoResult((int)info.type, info.dbtype, info.dbtypeFull, info.isnullable, info.defaultValue);
        }
        CsToDb<OdbcType> GetDbInfoNoneArray(Type type)
        {
            if (_dicCsToDb.TryGetValue(type.FullName, out var trydc)) return trydc;
            if (type.IsArray) return null;
            var enumType = type.IsEnum ? type : null;
            if (enumType == null && type.IsNullableType() && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments.First().IsEnum) enumType = type.GenericTypeArguments.First();
            if (enumType != null)
            {
                var newItem = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any() ?
                    CsToDb.New(OdbcType.BigInt, "bigint", $"bigint{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, enumType.CreateInstanceGetDefaultValue()) :
                    CsToDb.New(OdbcType.Int, "integer", $"integer{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, enumType.CreateInstanceGetDefaultValue());
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
            Object<DbConnection> conn = null;
            string database = null;

            try
            {
                conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5));
                database = conn.Value.Database;

                var sb = new StringBuilder();
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

                    if (string.Compare(tbname[0], database, true) != 0) //创建数据库
                    {
                        try
                        {
                            LocalExecuteScalar(tbname[0], $" select first 1 1 from syscolcomms");
                        }
                        catch
                        {
                            sb.Append($"CREATE DATABASE IF NOT EXISTS ").Append(_commonUtils.QuoteSqlName(tbname[0])).Append(";\r\n");
                        }
                    }

                    //创建表
                    var createTableName = _commonUtils.QuoteSqlName(tbname[0], tbname[1]);
                    sb.Append("CREATE TABLE IF NOT EXISTS ").Append(createTableName).Append(" ( ");
                    foreach (var tbcol in tb.ColumnsByPosition)
                    {
                        sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType);
                        sb.Append(",");
                    }
                    if (tb.Primarys.Any())
                    {
                        sb.Append(" \r\n  PRIMARY KEY (");
                        foreach (var tbcol in tb.Primarys) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                        sb.Remove(sb.Length - 2, 2).Append("),");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("\r\n)");
                    sb.Append(";\r\n");
                    //创建表的索引
                    foreach (var uk in tb.Indexes)
                    {
                        sb.Append("CREATE ");
                        if (uk.IsUnique) sb.Append("UNIQUE ");
                        sb.Append("INDEX IF NOT EXISTS ").Append(_commonUtils.QuoteSqlName(uk.Name)).Append(" ON ").Append(createTableName).Append("(");
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
                            sb.Append("COMMENT ON COLUMN ").Append(_commonUtils.QuoteSqlName($"{tbname.Last()}.{tbcol.Attribute.Name}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment)).Append(";\r\n");
                    }
                    if (string.IsNullOrEmpty(tb.Comment) == false)
                        sb.Append("COMMENT ON TABLE ").Append(_commonUtils.QuoteSqlName(tbname.Last())).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tb.Comment)).Append(";\r\n");
                    continue;
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
            }
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