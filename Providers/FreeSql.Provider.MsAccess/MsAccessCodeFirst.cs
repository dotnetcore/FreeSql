using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.MsAccess
{

    class MsAccessCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {
        public override bool IsNoneCommandParameter { get => true; set => base.IsNoneCommandParameter = true; }
        public MsAccessCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm, commonUtils, commonExpression) { }

        static object _dicCsToDbLock = new object();
        static Dictionary<string, (OleDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)> _dicCsToDb = new Dictionary<string, (OleDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)>() {
                { typeof(bool).FullName,  (OleDbType.Boolean, "bit","bit NOT NULL", null, false, false) },{ typeof(bool?).FullName,  (OleDbType.Boolean, "bit","bit", null, true, null) },

                { typeof(sbyte).FullName,  (OleDbType.TinyInt, "decimal", "decimal(3,0) NOT NULL", false, false, 0) },{ typeof(sbyte?).FullName,  (OleDbType.TinyInt, "decimal", "decimal(3,0)", false, true, null) },
                { typeof(short).FullName,  (OleDbType.SmallInt, "decimal","decimal(6,0) NOT NULL", false, false, 0) },{ typeof(short?).FullName,  (OleDbType.SmallInt, "decimal", "decimal(6,0)", false, true, null) },
                { typeof(int).FullName,  (OleDbType.Integer, "decimal", "decimal(11,0) NOT NULL", false, false, 0) },{ typeof(int?).FullName,  (OleDbType.Integer, "decimal", "decimal(11,0)", false, true, null) },
                { typeof(long).FullName,  (OleDbType.BigInt, "decimal","decimal(20,0) NOT NULL", false, false, 0) },{ typeof(long?).FullName,  (OleDbType.BigInt, "decimal","decimal(20,0)", false, true, null) },
                // access int long 类型是留给自动增长用的，所以这里全映射为 decimal
                { typeof(byte).FullName,  (OleDbType.UnsignedTinyInt, "decimal","decimal(3,0) NOT NULL", true, false, 0) },{ typeof(byte?).FullName,  (OleDbType.UnsignedTinyInt, "decimal","decimal(3,0)", true, true, null) },
                { typeof(ushort).FullName,  (OleDbType.UnsignedSmallInt, "decimal","decimal(5,0) NOT NULL", true, false, 0) },{ typeof(ushort?).FullName,  (OleDbType.UnsignedSmallInt, "decimal", "decimal(5,0)", true, true, null) },
                { typeof(uint).FullName,  (OleDbType.UnsignedInt, "decimal", "decimal(10,0) NOT NULL", true, false, 0) },{ typeof(uint?).FullName,  (OleDbType.UnsignedInt, "decimal", "decimal(10,0)", true, true, null) },
                { typeof(ulong).FullName,  (OleDbType.UnsignedBigInt, "decimal", "decimal(20,0) NOT NULL", true, false, 0) },{ typeof(ulong?).FullName,  (OleDbType.UnsignedBigInt, "decimal", "decimal(20,0)", true, true, null) },

                { typeof(double).FullName,  (OleDbType.Double, "double", "double NOT NULL", false, false, 0) },{ typeof(double?).FullName,  (OleDbType.Double, "double", "double", false, true, null) },
                { typeof(float).FullName,  (OleDbType.Currency, "single","single NOT NULL", false, false, 0) },{ typeof(float?).FullName,  (OleDbType.Currency, "single","single", false, true, null) },
                { typeof(decimal).FullName,  (OleDbType.Decimal, "decimal", "decimal(10,2) NOT NULL", false, false, 0) },{ typeof(decimal?).FullName,  (OleDbType.Decimal, "decimal", "decimal(10,2)", false, true, null) },

                { typeof(TimeSpan).FullName,  (OleDbType.DBTime, "datetime","datetime NOT NULL", false, false, 0) },{ typeof(TimeSpan?).FullName,  (OleDbType.DBTime, "datetime", "datetime",false, true, null) },
                { typeof(DateTime).FullName,  (OleDbType.DBTimeStamp, "datetime", "datetime NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateTime?).FullName,  (OleDbType.DBTimeStamp, "datetime", "datetime", false, true, null) },

                { typeof(byte[]).FullName,  (OleDbType.VarBinary, "varbinary", "varbinary(255)", false, null, new byte[0]) },
                { typeof(string).FullName,  (OleDbType.VarChar, "varchar", "varchar(255)", false, null, "") },

                { typeof(Guid).FullName,  (OleDbType.Guid, "varchar", "varchar(36) NOT NULL", false, false, Guid.Empty) },{ typeof(Guid?).FullName,  (OleDbType.Guid, "varchar", "varchar(36)", false, true, null) },
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
                    (OleDbType.BigInt, "decimal", $"decimal(20,0){(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0)) :
                    (OleDbType.Integer, "decimal", $"decimal(11,0){(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0));
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
                var tbname = tb.DbName;
                var tboldname = tb.DbOldName; //旧表名
                if (string.Compare(tbname, tboldname, true) == 0) tboldname = null;
                if (string.IsNullOrEmpty(obj.tableName) == false)
                {
                    tbname = obj.tableName;
                    tboldname = null;
                }

                var sbalter = new StringBuilder();
                var istmpatler = false; //创建临时表，导入数据，删除旧表，修改
                var isexistsTb = false;

                DataTable schemaTables = null;
                using (var conn = _orm.Ado.MasterPool.Get())
                {
                    schemaTables = conn.Value.GetSchema("Tables");
                }
                var schemaTablesTableNameIndex = 2;
                for (var idx = 0; idx < schemaTables.Columns.Count; idx++)
                    if (string.Compare(schemaTables.Columns[idx].ColumnName, "TABLE_NAME", true) == 0)
                    {
                        schemaTablesTableNameIndex = idx;
                        break;
                    }
                Func<string, bool> existsTable = tn =>
                {
                    foreach (DataRow row in schemaTables.Rows)
                        if (string.Compare(row[schemaTablesTableNameIndex]?.ToString(), tn, true) == 0)
                            return true;
                    return false;
                    //_orm.Ado.ExecuteScalar(CommandType.Text, $" SELECT 1 FROM MsysObjects WHERE Name='{tn}' AND Left([Name],1)<>'~' AND Left([Name],4)<>'Msys' AND Type=1 and Flags=0") != null;
                };
                Action<string> createTable = tn =>
                {
                    tn = _commonUtils.QuoteSqlName(tn);
                    //创建表
                    sb.Append("CREATE TABLE ").Append(tn).Append(" ( ");
                    foreach (var tbcol in tb.ColumnsByPosition)
                    {
                        sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name));
                        if (tbcol.Attribute.IsIdentity == true && tbcol.Attribute.DbType.IndexOf("AUTOINCREMENT", StringComparison.CurrentCultureIgnoreCase) == -1)
                            sb.Append(" AUTOINCREMENT");
                        else
                            sb.Append(" ").Append(tbcol.Attribute.DbType);
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
                };
                Action<string> createTableIndex = tn =>
                {
                    tn = _commonUtils.QuoteSqlName(tn);
                    //创建表的索引
                    foreach (var uk in tb.Indexes)
                    {
                        sb.Append("CREATE ");
                        if (uk.IsUnique) sb.Append("UNIQUE ");
                        sb.Append("INDEX ").Append(_commonUtils.QuoteSqlName(uk.Name)).Append(" ON ").Append(tn).Append("(");
                        foreach (var tbcol in uk.Columns)
                        {
                            sb.Append(_commonUtils.QuoteSqlName(tbcol.Column.Attribute.Name));
                            if (tbcol.IsDesc) sb.Append(" DESC");
                            sb.Append(", ");
                        }
                        sb.Remove(sb.Length - 2, 2).Append(");\r\n");
                    }
                };

                if (tboldname != null)
                {
                    if (existsTable(tboldname) == false)
                        //旧表不存在
                        tboldname = null;
                }
                isexistsTb = existsTable(tbname);
                if (isexistsTb == false)
                { //表不存在
                    if (tboldname == null)
                    {
                        createTable(tbname);
                        createTableIndex(tbname);
                        continue;
                    }
                    //如果新表，旧表不在一起，创建新表，导入数据，删除旧表
                    istmpatler = true;
                }
                if (tboldname != null && isexistsTb == true)
                    throw new Exception($"旧表(OldName)：{tboldname} 存在，数据库已存在 {tbname} 表，无法改名");

                DataTable schemaColumns = null;
                DataTable schemaDataTypes = null;
                DataTable schemaIndexes = null;
                using (var conn = _orm.Ado.MasterPool.Get())
                {
                    schemaColumns = conn.Value.GetSchema("COLUMNS");
                    schemaDataTypes = conn.Value.GetSchema("DATATYPES");
                    schemaIndexes = conn.Value.GetSchema("INDEXES");
                }
                Func<string, string, bool> checkPrimaryKeyByTableNameAndColumn = (tn, cn) =>
                {
                    int table_name_index = 0, primary_key_index = 0, column_name_index = 0;
                    for (var a = 0; a < schemaIndexes.Columns.Count; a++)
                    {
                        switch (schemaIndexes.Columns[a].ColumnName.ToLower())
                        {
                            case "table_name":
                                table_name_index = a;
                                break;
                            case "primary_key":
                                primary_key_index = a;
                                break;
                            case "column_name":
                                column_name_index = a;
                                break;
                        }
                    }
                    foreach (DataRow row in schemaIndexes.Rows)
                    {
                        if (string.Compare(row[table_name_index]?.ToString(), tn, true) != 0) continue;
                        if (string.Compare(row[column_name_index]?.ToString(), cn, true) != 0) continue;
                        return new[] { "1", "True", "true", "t", "yes", "ok" }.Contains(row[primary_key_index]?.ToString());
                    }
                    return false;
                };
                Func<string, List<string[]>> getIndexesByTableName = tn =>
                {
                    int table_name_index = 0, index_name_index = 0, primary_key_index = 0, unique_index = 0, column_name_index = 0, collation_index = 0;
                    for (var a = 0; a < schemaIndexes.Columns.Count; a++)
                    {
                        switch (schemaIndexes.Columns[a].ColumnName.ToLower())
                        {
                            case "table_name":
                                table_name_index = a;
                                break;
                            case "index_name":
                                index_name_index = a;
                                break;
                            case "primary_key":
                                primary_key_index = a;
                                break;
                            case "unique":
                                unique_index = a;
                                break;
                            case "column_name":
                                column_name_index = a;
                                break;
                            case "collation":
                                collation_index = a;
                                break;
                        }
                    }
                    var idxs = new List<string[]>();
                    foreach (DataRow row in schemaIndexes.Rows)
                    {
                        if (string.Compare(row[table_name_index]?.ToString(), tn, true) != 0) continue;
                        if (new[] { "1", "True", "true", "t", "yes", "ok" }.Contains(row[primary_key_index]?.ToString())) continue;
                        var column_name = row[column_name_index]?.ToString();
                        var index_name = row[index_name_index]?.ToString();
                        var isDesc = int.TryParse(row[collation_index]?.ToString(), out var collation) ? (collation == 2) : false;
                        var unique = new[] { "1", "True", "true", "t", "yes", "ok" }.Contains(row[unique_index]?.ToString());
                        idxs.Add(new[] { column_name, index_name, isDesc ? "1" : "0", unique ? "1" : "0" });
                    }
                    return idxs;
                };
                Func<string, Dictionary<string, (string column, string sqlType, bool is_nullable, bool is_identity, string comment)>> getColumnsByTableName = tn =>
                {
                    int table_name_index = 0, column_name_index = 0, is_nullable_index = 0, data_type_index = 0,
                        character_maximum_length_index = 0, character_octet_length_index = 0, numeric_precision_index = 0, numeric_scale_index = 0,
                        datetime_precision_index = 0, description_index = 0;
                    for (var a = 0; a < schemaColumns.Columns.Count; a++)
                    {
                        switch (schemaColumns.Columns[a].ColumnName.ToLower())
                        {
                            case "table_name":
                                table_name_index = a;
                                break;
                            case "column_name":
                                column_name_index = a;
                                break;
                            case "is_nullable":
                                is_nullable_index = a;
                                break;
                            case "data_type":
                                data_type_index = a;
                                break;
                            case "character_maximum_length":
                                character_maximum_length_index = a;
                                break;
                            case "character_octet_length":
                                character_octet_length_index = a;
                                break;
                            case "numeric_precision":
                                numeric_precision_index = a;
                                break;
                            case "numeric_scale":
                                numeric_scale_index = a;
                                break;
                            case "datetime_precision":
                                datetime_precision_index = a;
                                break;
                            case "description":
                                description_index = a;
                                break;
                        }
                    }
                    int datatype_ProviderDbType_index = 0, datatype_NativeDataType_index = 0, datatype_TypeName_index = 0, datatype_IsAutoIncrementable_index = 0;
                    for (var a = 0; a < schemaDataTypes.Columns.Count; a++)
                    {
                        switch (schemaDataTypes.Columns[a].ColumnName.ToLower())
                        {
                            case "providerdbtype":
                                datatype_ProviderDbType_index = a;
                                break;
                            case "nativedatatype":
                                datatype_NativeDataType_index = a;
                                break;
                            case "typename":
                                datatype_TypeName_index = a;
                                break;
                            case "isautoincrementable":
                                datatype_IsAutoIncrementable_index = a;
                                break;
                        }
                    }
                    Func<string, DataRow> getDataType = dtnum =>
                    {
                        DataRow dtRow = null; //这里的写法是为了照顾 SchemaTypes 返回的结构
                        foreach (DataRow dataType in schemaDataTypes.Rows)
                        {
                            if (datatype_ProviderDbType_index >= 0 && dataType[datatype_ProviderDbType_index]?.ToString() == dtnum) dtRow = dataType;
                            if (datatype_NativeDataType_index >= 0 && dataType[datatype_NativeDataType_index]?.ToString() == dtnum) dtRow = dataType;
                        }
                        return dtRow;
                    };
                    var ret = new Dictionary<string, (string column, string sqlType, bool is_nullable, bool is_identity, string comment)>();
                    foreach (DataRow row in schemaColumns.Rows)
                    {
                        if (string.Compare(row[table_name_index]?.ToString(), tn, true) != 0) continue;
                        var column_name = row[column_name_index]?.ToString();
                        var dataTypeRow = getDataType(row[data_type_index]?.ToString());
                        var is_identity = new[] { "1", "True", "true", "t", "yes", "ok" }.Contains(dataTypeRow[datatype_IsAutoIncrementable_index]?.ToString());
                        var is_nullable = new[] { "1", "True", "true", "t", "yes", "ok" }.Contains(row[is_nullable_index]?.ToString());
                        if (is_nullable && checkPrimaryKeyByTableNameAndColumn(tn, column_name)) is_nullable = false;
                        var comment = row[description_index]?.ToString();
                        if (string.IsNullOrEmpty(comment)) comment = null;
                        int.TryParse(row[character_maximum_length_index]?.ToString(), out var character_maximum_length);
                        int.TryParse(row[character_octet_length_index]?.ToString(), out var character_octet_length);
                        int.TryParse(row[numeric_precision_index]?.ToString(), out var numeric_precision);
                        int.TryParse(row[numeric_scale_index]?.ToString(), out var numeric_scale);
                        int.TryParse(row[datetime_precision_index]?.ToString(), out var datetime_precision);
                        var datatype = dataTypeRow[datatype_TypeName_index]?.ToString().ToUpper();
                        if (numeric_precision > 0 && numeric_scale > 0) datatype = $"{datatype}({numeric_precision},{numeric_scale})";
                        else if (character_maximum_length > 0 && character_octet_length > 0) datatype = $"{datatype}({character_maximum_length})";
                        ret.Add(column_name, (column_name, datatype, is_nullable, is_identity, comment));
                    }
                    return ret;
                };
                //对比字段，只可以修改类型、增加字段、有限的修改字段名；保证安全不删除字段
                var tbtmp = tboldname ?? tbname;
                var tbstruct = getColumnsByTableName(tbtmp);

                if (istmpatler == false)
                {
                    foreach (var tbcol in tb.ColumnsByPosition)
                    {
                        var dbtypeNoneNotNull = Regex.Replace(tbcol.Attribute.DbType, @"NOT\s+NULL", "NULL");
                        if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
                        string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol))
                        {
                            if (tbstructcol.sqlType != "LONG" && tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType, StringComparison.CurrentCultureIgnoreCase) == false)
                                istmpatler = true;
                            if (tbstructcol.sqlType != "BIT" && tbcol.Attribute.IsNullable != tbstructcol.is_nullable)
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
                    var dsuk = getIndexesByTableName(tbtmp);
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

                Dictionary<string, bool> dicDropTable = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                Action<string> dropTable = tn => {
                    if (dicDropTable.ContainsKey(tn)) return;
                    dicDropTable.Add(tn, true);
                    sb.Append("DROP TABLE ").Append(_commonUtils.QuoteSqlName(tn)).Append(";\r\n");
                };
                Action<string, string> createTableImportData = (newtn, oldtn) =>
                {
                    if (existsTable(newtn)) dropTable(newtn);
                    createTable(newtn);
                    sb.Append("INSERT INTO ").Append(_commonUtils.QuoteSqlName(newtn)).Append(" (");
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
                                insertvalue = MsAccessUtils.GetCastSql(insertvalue, tbcol.Attribute.MapType);
                            }
                            if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable)
                                insertvalue = $"iif(isnull({insertvalue}),{tbcol.DbDefaultValue},{insertvalue})";
                        }
                        else if (tbcol.Attribute.IsNullable == false)
                            insertvalue = tbcol.DbDefaultValue;
                        sb.Append(insertvalue).Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2).Append(" FROM ").Append(_commonUtils.QuoteSqlName(oldtn)).Append(";\r\n");
                    dropTable(oldtn);
                };

                if (tboldname != null && isexistsTb == true)
                {
                    createTableImportData(tbname + "_FreeSqlBackup", tbname); //备份 tbname 表
                    createTableImportData(tbname, tboldname); //创建新表，把 oldname 旧表数据导入到新表，删除 oldname
                }
                if (tboldname != null && isexistsTb == false)
                {
                    createTableImportData(tbname, tboldname); //创建新表，把 oldname 旧表数据导入到新表，删除 oldname
                }
                if (tboldname == null && isexistsTb == true)
                {
                    createTableImportData(tbname + "_FreeSqlTmp", tbname); //创建 Tmp 表，把 tbname 表数据导入到 Tmp，删除 tbname
                    createTableImportData(tbname, tbname + "_FreeSqlTmp"); //创建 新表，把 Tmp 表数据导入到新表，删除 Tmp
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
    }
}