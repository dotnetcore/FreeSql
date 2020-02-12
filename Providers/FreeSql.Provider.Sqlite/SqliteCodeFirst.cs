using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Sqlite
{

    class SqliteCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {

        public SqliteCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm, commonUtils, commonExpression) { }

        static object _dicCsToDbLock = new object();
        static Dictionary<string, (DbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)> _dicCsToDb = new Dictionary<string, (DbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)>() {
                { typeof(bool).FullName,  (DbType.Boolean, "boolean","boolean NOT NULL", null, false, false) },{ typeof(bool?).FullName,  (DbType.Boolean, "boolean","boolean", null, true, null) },

                { typeof(sbyte).FullName,  (DbType.SByte, "smallint", "smallint NOT NULL", false, false, 0) },{ typeof(sbyte?).FullName,  (DbType.SByte, "smallint", "smallint", false, true, null) },
                { typeof(short).FullName,  (DbType.Int16, "smallint","smallint NOT NULL", false, false, 0) },{ typeof(short?).FullName,  (DbType.Int16, "smallint", "smallint", false, true, null) },
                { typeof(int).FullName,  (DbType.Int32, "integer", "integer NOT NULL", false, false, 0) },{ typeof(int?).FullName,  (DbType.Int32, "integer", "integer", false, true, null) },
                { typeof(long).FullName,  (DbType.Int64, "integer","integer NOT NULL", false, false, 0) },{ typeof(long?).FullName,  (DbType.Int64, "integer","integer", false, true, null) },

                { typeof(byte).FullName,  (DbType.Byte, "int2","int2 NOT NULL", true, false, 0) },{ typeof(byte?).FullName,  (DbType.Byte, "int2","int2", true, true, null) },
                { typeof(ushort).FullName,  (DbType.UInt16, "unsigned","unsigned NOT NULL", true, false, 0) },{ typeof(ushort?).FullName,  (DbType.UInt16, "unsigned", "unsigned", true, true, null) },
                { typeof(uint).FullName,  (DbType.Decimal, "decimal(10,0)", "decimal(10,0) NOT NULL", true, false, 0) },{ typeof(uint?).FullName,  (DbType.Decimal, "decimal(10,0)", "decimal(10,0)", true, true, null) },
                { typeof(ulong).FullName,  (DbType.Decimal, "decimal(21,0)", "decimal(21,0) NOT NULL", true, false, 0) },{ typeof(ulong?).FullName,  (DbType.Decimal, "decimal(21,0)", "decimal(21,0)", true, true, null) },

                { typeof(double).FullName,  (DbType.Double, "double", "double NOT NULL", false, false, 0) },{ typeof(double?).FullName,  (DbType.Double, "double", "double", false, true, null) },
                { typeof(float).FullName,  (DbType.Single, "float","float NOT NULL", false, false, 0) },{ typeof(float?).FullName,  (DbType.Single, "float","float", false, true, null) },
                { typeof(decimal).FullName,  (DbType.Decimal, "decimal", "decimal(10,2) NOT NULL", false, false, 0) },{ typeof(decimal?).FullName,  (DbType.Decimal, "decimal", "decimal(10,2)", false, true, null) },

                { typeof(TimeSpan).FullName,  (DbType.Time, "bigint","bigint NOT NULL", false, false, 0) },{ typeof(TimeSpan?).FullName,  (DbType.Time, "bigint", "bigint",false, true, null) },
                { typeof(DateTime).FullName,  (DbType.DateTime, "datetime", "datetime NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateTime?).FullName,  (DbType.DateTime, "datetime", "datetime", false, true, null) },

                { typeof(byte[]).FullName,  (DbType.Binary, "blob", "blob", false, null, new byte[0]) },
                { typeof(string).FullName,  (DbType.String, "nvarchar", "nvarchar(255)", false, null, "") },

                { typeof(Guid).FullName,  (DbType.Guid, "character", "character(36) NOT NULL", false, false, Guid.Empty) },{ typeof(Guid?).FullName,  (DbType.Guid, "character", "character(36)", false, true, null) },
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
                    (DbType.Int64, "bigint", $"bigint{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0)) :
                    (DbType.Int32, "mediumint", $"mediumint{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0));
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
            var sb = new StringBuilder();
            var sbDeclare = new StringBuilder();
            foreach (var obj in objects)
            {
                if (sb.Length > 0) sb.Append("\r\n");
                var tb = _commonUtils.GetTableByEntity(obj.entityType);
                if (tb == null) throw new Exception($"类型 {obj.entityType.FullName} 不可迁移");
                if (tb.Columns.Any() == false) throw new Exception($"类型 {obj.entityType.FullName} 不可迁移，可迁移属性0个");
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
                var istmpatler = false; //创建临时表，导入数据，删除旧表，修改
                var isIndent = false;
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
                            if (tbcol.Attribute.IsIdentity == true && tbcol.Attribute.DbType.IndexOf("AUTOINCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1)
                            {
                                isIndent = true;
                                sb.Append(" PRIMARY KEY AUTOINCREMENT");
                            }
                            sb.Append(",");
                        }
                        if (isIndent == false && tb.Primarys.Any())
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
                            sb.Append("INDEX ").Append(_commonUtils.QuoteSqlName(uk.Name)).Append(" ON ").Append(tbname[1]).Append("(");
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
                    //如果新表，旧表在一个模式下，直接修改表名
                    if (string.Compare(tbname[0], tboldname[0], true) == 0)
                        sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName(tboldname[0], tboldname[1])).Append(" RENAME TO \"").Append(tbname[1]).Append("\";\r\n");
                    else
                    {
                        //如果新表，旧表不在一起，创建新表，导入数据，删除旧表
                        istmpatler = true;
                    }
                }
                else
                    tboldname = null; //如果新表已经存在，不走改表名逻辑

                //对比字段，只可以修改类型、增加字段、有限的修改字段名；保证安全不删除字段
                var tbtmp = tboldname ?? tbname;
                var dsql = _orm.Ado.ExecuteScalar(CommandType.Text, $" select sql from {tbtmp[0]}.sqlite_master where type='table' and name='{tbtmp[1]}'")?.ToString();
                var ds = _orm.Ado.ExecuteArray(CommandType.Text, $"PRAGMA {_commonUtils.QuoteSqlName(tbtmp[0])}.table_info(\"{tbtmp[1]}\")");
                var tbstruct = ds.ToDictionary(a => string.Concat(a[1]), a =>
                {
                    var is_identity = false;
                    var dsqlIdx = dsql?.IndexOf($"\"{a[1]}\" ");
                    if (dsqlIdx > 0)
                    {
                        var dsqlLastIdx = dsql.IndexOf('\n', dsqlIdx.Value);
                        if (dsqlLastIdx > 0) is_identity = dsql.Substring(dsqlIdx.Value, dsqlLastIdx - dsqlIdx.Value).Contains("AUTOINCREMENT");
                    }
                    return new
                    {
                        column = string.Concat(a[1]),
                        sqlType = string.Concat(a[2]).ToUpper(),
                        is_nullable = string.Concat(a[5]) == "0" && string.Concat(a[3]) == "0",
                        is_identity
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
                            if (tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType, StringComparison.CurrentCultureIgnoreCase) == false)
                                istmpatler = true;
                            if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable)
                                istmpatler = true;
                            if (tbcol.Attribute.IsIdentity != tbstructcol.is_identity)
                                istmpatler = true;
                            if (string.Compare(tbstructcol.column, tbcol.Attribute.OldName, true) == 0)
                                //修改列名
                                istmpatler = true;
                            continue;
                        }
                        //添加列
                        istmpatler = true;
                    }
                    var dsuk = new List<string[]>();
                    var dbIndexes = _orm.Ado.ExecuteArray(CommandType.Text, $"PRAGMA {_commonUtils.QuoteSqlName(tbtmp[0])}.INDEX_LIST(\"{tbtmp[1]}\")");
                    foreach (var dbIndex in dbIndexes)
                    {
                        if (string.Concat(dbIndex[3]) == "pk") continue;
                        var dbIndexesColumns = _orm.Ado.ExecuteArray(CommandType.Text, $"PRAGMA {_commonUtils.QuoteSqlName(tbtmp[0])}.INDEX_INFO({dbIndex[1]})");
                        var dbIndexesSql = string.Concat(_orm.Ado.ExecuteScalar(CommandType.Text, $" SELECT sql FROM sqlite_master WHERE name = '{dbIndex[1]}'"));
                        foreach (var dbcolumn in dbIndexesColumns)
                        {
                            var dbcolumnName = string.Concat(dbcolumn[2]);
                            var isDesc = dbIndexesSql.IndexOf($@"{dbcolumnName}"" DESC", StringComparison.CurrentCultureIgnoreCase) == -1 ? "0" : "1";
                            dsuk.Add(new[] { dbcolumnName, string.Concat(dbIndex[1]), isDesc, string.Concat(dbIndex[2]) });
                        }
                    }
                    foreach (var uk in tb.Indexes)
                    {
                        if (string.IsNullOrEmpty(uk.Name) || uk.Columns.Any() == false) continue;
                        var dsukfind1 = dsuk.Where(a => string.Compare(a[1], uk.Name, true) == 0).ToArray();
                        if (dsukfind1.Any() == false || dsukfind1.Length != uk.Columns.Length || dsukfind1.Where(a => (a[3] == "1") == uk.IsUnique && uk.Columns.Where(b => string.Compare(b.Column.Attribute.Name, a[0], true) == 0 && (a[2] == "1") == b.IsDesc).Any()).Count() != uk.Columns.Length)
                            istmpatler = true;
                    }
                }
                if (istmpatler == false)
                {
                    sb.Append(sbalter);
                    continue;
                }

                //创建临时表，数据导进临时表，然后删除原表，将临时表改名为原表名
                var tablename = tboldname == null ? _commonUtils.QuoteSqlName(tbname[0], tbname[1]) : _commonUtils.QuoteSqlName(tboldname[0], tboldname[1]);
                var tablenameOnlyTb = tboldname == null ? tbname[1] : tboldname[1];
                var tmptablename = _commonUtils.QuoteSqlName(tbname[0], $"_FreeSqlTmp_{tbname[1]}");
                //创建临时表
                isIndent = false;
                sb.Append("CREATE TABLE IF NOT EXISTS ").Append(tmptablename).Append(" ( ");
                foreach (var tbcol in tb.ColumnsByPosition)
                {
                    sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType);
                    if (tbcol.Attribute.IsIdentity == true && tbcol.Attribute.DbType.IndexOf("AUTOINCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1)
                    {
                        isIndent = true;
                        sb.Append(" PRIMARY KEY AUTOINCREMENT");
                    }
                    sb.Append(",");
                }
                if (isIndent == false && tb.Primarys.Any())
                {
                    sb.Append(" \r\n  PRIMARY KEY (");
                    foreach (var tbcol in tb.Primarys) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                    sb.Remove(sb.Length - 2, 2).Append("),");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append("\r\n) \r\n;\r\n");
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
                            insertvalue = $"ifnull({insertvalue},{tbcol.DbDefaultValue})";
                    }
                    else if (tbcol.Attribute.IsNullable == false)
                        insertvalue = tbcol.DbDefaultValue;
                    sb.Append(insertvalue).Append(", ");
                }
                sb.Remove(sb.Length - 2, 2).Append(" FROM ").Append(tablename).Append(";\r\n");
                sb.Append("DROP TABLE ").Append(tablename).Append(";\r\n");
                sb.Append("ALTER TABLE ").Append(tmptablename).Append(" RENAME TO \"").Append(tbname[1]).Append("\";\r\n");
                //创建表的索引
                foreach (var uk in tb.Indexes)
                {
                    sb.Append("CREATE ");
                    if (uk.IsUnique) sb.Append("UNIQUE ");
                    sb.Append("INDEX ").Append(_commonUtils.QuoteSqlName(uk.Name)).Append(" ON \"").Append(tablenameOnlyTb).Append("\"(");
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
    }
}