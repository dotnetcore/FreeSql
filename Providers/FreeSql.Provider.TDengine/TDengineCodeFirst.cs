using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using FreeSql.DataAnnotations;
using System.Reflection;
using FreeSql.Internal.ObjectPool;
using FreeSql.Provider.TDengine.Attributes;

namespace FreeSql.TDengine
{
    internal class TDengineCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {
        public TDengineCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm,
            commonUtils, commonExpression)
        {
        }

        static readonly Dictionary<string, CsToDb<DbType>> DicCsToDb = new Dictionary<string, CsToDb<DbType>>()
        {
            { typeof(bool).FullName, CsToDb.New(DbType.Boolean, "BOOL", "BOOL", null, false, null) },
            { typeof(bool?).FullName, CsToDb.New(DbType.Boolean, "BOOL", "BOOL", null, true, null) },
            { typeof(DateTime).FullName, CsToDb.New(DbType.DateTime, "TIMESTAMP", "TIMESTAMP", null, false, null) },
            { typeof(DateTime?).FullName, CsToDb.New(DbType.DateTime, "TIMESTAMP", "TIMESTAMP", null, true, null) },
            { typeof(TimeSpan).FullName, CsToDb.New(DbType.DateTime, "TIMESTAMP", "TIMESTAMP", null, false, null) },
            { typeof(TimeSpan?).FullName, CsToDb.New(DbType.DateTime, "TIMESTAMP", "TIMESTAMP", null, true, null) },
            { typeof(short).FullName, CsToDb.New(DbType.Int16, "SMALLINT", "SMALLINT", null, false, 0) },
            { typeof(short?).FullName, CsToDb.New(DbType.Int16, "SMALLINT", "SMALLINT", null, true, null) },
            { typeof(int).FullName, CsToDb.New(DbType.Int32, "INT", "INT", null, false, 0) },
            { typeof(int?).FullName, CsToDb.New(DbType.Int32, "INT", "INT", null, true, null) },
            { typeof(sbyte).FullName, CsToDb.New(DbType.SByte, "TINYINT", "TINYINT", null, false, 0) },
            { typeof(sbyte?).FullName, CsToDb.New(DbType.SByte, "TINYINT", "TINYINT", null, true, null) },
            { typeof(long).FullName, CsToDb.New(DbType.Int64, "BIGINT", "BIGINT", null, false, 0) },
            { typeof(long?).FullName, CsToDb.New(DbType.Int64, "BIGINT", "BIGINT", null, true, null) },
            { typeof(byte).FullName, CsToDb.New(DbType.Byte, "TINYINT UNSIGNED", "TINYINT UNSIGNED", null, false, 0) },
            {
                typeof(byte?).FullName,
                CsToDb.New(DbType.Byte, "TINYINT UNSIGNED", "TINYINT UNSIGNED", null, true, null)
            },
            {
                typeof(ushort).FullName,
                CsToDb.New(DbType.UInt16, "SMALLINT UNSIGNED", "SMALLINT UNSIGNED", null, false, 0)
            },
            {
                typeof(ushort?).FullName,
                CsToDb.New(DbType.UInt16, "SMALLINT UNSIGNED", "SMALLINT UNSIGNED", null, true, null)
            },
            { typeof(uint).FullName, CsToDb.New(DbType.UInt32, "INT UNSIGNED", "INT UNSIGNED", null, false, 0) },
            { typeof(uint?).FullName, CsToDb.New(DbType.UInt32, "INT UNSIGNED", "INT UNSIGNED", null, true, null) },
            { typeof(ulong).FullName, CsToDb.New(DbType.UInt64, "BIGINT UNSIGNED", "BIGINT UNSIGNED", null, false, 0) },
            {
                typeof(ulong?).FullName,
                CsToDb.New(DbType.UInt64, "BIGINT UNSIGNED", "BIGINT UNSIGNED", null, true, null)
            },
            { typeof(float).FullName, CsToDb.New(DbType.Single, "FLOAT", "FLOAT", null, false, 0) },
            { typeof(float?).FullName, CsToDb.New(DbType.Single, "FLOAT", "FLOAT", null, true, null) },
            { typeof(double).FullName, CsToDb.New(DbType.Double, "DOUBLE", "DOUBLE", null, false, 0) },
            { typeof(double?).FullName, CsToDb.New(DbType.Double, "DOUBLE", "DOUBLE", null, true, null) },
            { typeof(string).FullName, CsToDb.New(DbType.String, "NCHAR", "NCHAR(255)", null, false, 0) },
        };

        public override DbInfoResult GetDbInfo(Type type)
        {
            if (DicCsToDb.TryGetValue(type.FullName, out var trydc))
                return new DbInfoResult((int)trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable,
                    trydc.defaultValue);
            if (type.IsArray) return null;
            return null;
        }

        protected override string GetComparisonDDLStatements(params TypeSchemaAndName[] objects)
        {
            Object<DbConnection> conn = null;
            string database = null;
            var sb = new StringBuilder();
            try
            {
                conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5));
                database = conn.Value.Database;
                foreach (var obj in objects)
                {
                    if (sb.Length > 0) sb.Append(Environment.NewLine);
                    var tb = obj.tableSchema;
                    if (tb == null)
                        throw new Exception(CoreErrorStrings.S_Type_IsNot_Migrable(obj.tableSchema.Type.FullName));
                    if (tb.Columns.Any() == false)
                        throw new Exception(
                            CoreErrorStrings.S_Type_IsNot_Migrable_0Attributes(obj.tableSchema.Type.FullName));

                    var tbName = _commonUtils.SplitTableName(tb.DbName).First();

                    tbName = _commonUtils.QuoteSqlName(database, tbName);

                    if (!TryTableExists(tbName))
                    {
                        TableHandle(ref tb, ref database, tb.Type, ref sb, tbName);
                    }
                }
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


            var ddl = sb.Length == 0 ? null : sb.ToString();
            return ddl;
        }

        private void CreateColumns(ref TableInfo tb, ref StringBuilder sb)
        {
            //创建表
            foreach (var columnInfo in tb.ColumnsByPosition.Where(c =>
                         !c.Table.Properties[c.CsName].IsDefined(typeof(TDengineTagAttribute))))
            {
                sb.Append($" {Environment.NewLine}  ").Append(_commonUtils.QuoteSqlName(columnInfo.Attribute.Name))
                    .Append(" ")
                    .Append(columnInfo.Attribute.DbType);
                sb.Append(",");
            }

            sb.Remove(sb.Length - 1, 1).Append($"{Environment.NewLine})");
        }

        private void TableHandle(ref TableInfo tb, ref string database, Type type, ref StringBuilder sb, string tbName)
        {
            //判断是否超级表
            var subTableAttribute = type.GetCustomAttribute<TDengineSubTableAttribute>();

            //要创建表的为子表
            if (subTableAttribute != null)
            {
                if (_commonUtils is TDengineUtils utils)
                {
                    var superTableDescribe = utils.GetSuperTableDescribe(type);

                    if (superTableDescribe == null) return;

                    var superTableName = _commonUtils.QuoteSqlName(database, superTableDescribe.SuperTableName);
                    var superTableInfo = GetTableByEntity(superTableDescribe.SuperTableType);

                    //判断超表是否存在
                    if (!TryTableExists(superTableName))
                    {
                        //先创建超级表
                        CreateSuperTable(ref superTableInfo, ref sb, superTableName);
                        _orm.Ado.ExecuteNonQuery(sb.ToString());
                        sb = sb.Clear();
                    }

                    var subTableName = _commonUtils.QuoteSqlName(database, subTableAttribute.Name);

                    //创建子表
                    CreateSubTable(ref tb, ref sb, superTableName, subTableName, ref superTableInfo);
                }
            }
            //要创建的为超级表
            else if (type.IsDefined(typeof(TDengineSuperTableAttribute)))
            {
                var superTableAttribute = type.GetCustomAttribute<TDengineSuperTableAttribute>();
                if (superTableAttribute == null) return;
                tbName = _commonUtils.QuoteSqlName(database, superTableAttribute.Name);
                CreateSuperTable(ref tb, ref sb, tbName);
            }
            //创建普通表
            else
            {
                CreateNormalTable(ref tb, ref sb, tbName);
            }
        }


        /// <summary>
        /// 创建子表
        /// </summary>
        /// <param name="childTableInfo"></param>
        /// <param name="sb"></param>
        /// <param name="superTableName"></param>
        private void CreateSubTable(ref TableInfo childTableInfo, ref StringBuilder sb, string superTableName,
            string subTableName, ref TableInfo
                superTableInfo)
        {
            sb.Append($"CREATE TABLE {subTableName}{Environment.NewLine}");
            sb.Append($"USING {superTableName} (");

            var tagCols = superTableInfo.ColumnsByPosition.Where(c =>
                c.Table.Properties[c.CsName].IsDefined(typeof(TDengineTagAttribute))).ToArray();

            var tagValues = new List<object>(tagCols.Count());

            var tableInstance = Activator.CreateInstance(childTableInfo.Type);

            foreach (var columnInfo in tagCols)
            {
                var tagValue = childTableInfo.Properties[columnInfo.CsName].GetValue(tableInstance);
                tagValues.Add(tagValue);
                sb.Append($" {Environment.NewLine}  ").Append(_commonUtils.QuoteSqlName(columnInfo.Attribute.Name))
                    .Append(",");
            }

            sb.Remove(sb.Length - 1, 1).Append($"{Environment.NewLine}) TAGS (");

            foreach (var tagValue in tagValues)
            {
                sb.Append($" {Environment.NewLine}  ").Append(HandleTagValue(tagValue)).Append(",");
            }

            sb.Remove(sb.Length - 1, 1).Append($"{Environment.NewLine});");
        }

        /// <summary>
        /// 创建超级表
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="sb"></param>
        /// <param name="superTableName"></param>
        private void CreateSuperTable(ref TableInfo tb, ref StringBuilder sb, string superTableName)
        {
            sb.Append($"CREATE STABLE {superTableName} (");
            CreateColumns(ref tb, ref sb);
            sb.Append($" TAGS (");

            var columInfos = tb.ColumnsByPosition.Where(c =>
                c.Table.Properties[c.CsName].IsDefined(typeof(TDengineTagAttribute)));

            foreach (var columnInfo in columInfos)
            {
                sb.Append($" {Environment.NewLine}  ").Append(_commonUtils.QuoteSqlName(columnInfo.Attribute.Name))
                    .Append(" ")
                    .Append(columnInfo.Attribute.DbType);
                sb.Append(",");
            }

            sb.Remove(sb.Length - 1, 1).Append($"{Environment.NewLine});");
        }

        /// <summary>
        /// 创建普通表
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="sb"></param>
        /// <param name="normalTableName"></param>
        private void CreateNormalTable(ref TableInfo tb, ref StringBuilder sb, string normalTableName)
        {
            sb.Append($"CREATE TABLE {normalTableName} (");
            CreateColumns(ref tb, ref sb);
            foreach (var columnInfo in tb.ColumnsByPosition.Where(c =>
                         c.Table.Properties[c.CsName].IsDefined(typeof(TDengineTagAttribute))))
            {
                sb.Append($" {Environment.NewLine}  ").Append(_commonUtils.QuoteSqlName(columnInfo.Attribute.Name))
                    .Append(" ")
                    .Append(columnInfo.Attribute.DbType);
                sb.Append(",");
            }

            sb.Remove(sb.Length - 1, 1).Append(");");
        }

        private bool TryTableExists(string tbName)
        {
            var flag = true;
            try
            {
                var executeScalar = _orm.Ado.ExecuteScalar(CommandType.Text,
                    $"DESCRIBE {tbName}");

                if (executeScalar == null)
                {
                    flag = false;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Table does not exist"))
                {
                    flag = false;
                }
            }

            return flag;
        }

        private object HandleTagValue(object tagValue)
        {
            if (tagValue is DateTime || tagValue is string)
            {
                return $"\"{tagValue}\"";
            }
            else
            {
                return tagValue;
            }
        }
    }
}