using FreeSql.DataAnnotations;
using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using Newtonsoft.Json.Linq;
using Npgsql.LegacyPostgis;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.PostgreSQL
{

    class PostgreSQLCodeFirst : Internal.CommonProvider.CodeFirstProvider
    {

        public PostgreSQLCodeFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) : base(orm, commonUtils, commonExpression) { }

        static object _dicCsToDbLock = new object();
        static Dictionary<string, (NpgsqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)> _dicCsToDb = new Dictionary<string, (NpgsqlDbType type, string dbtype, string dbtypeFull, bool? isUnsigned, bool? isnullable, object defaultValue)>() {

                { typeof(sbyte).FullName,  (NpgsqlDbType.Smallint, "int2","int2 NOT NULL", false, false, 0) },{ typeof(sbyte?).FullName,  (NpgsqlDbType.Smallint, "int2", "int2", false, true, null) },
                { typeof(short).FullName,  (NpgsqlDbType.Smallint, "int2","int2 NOT NULL", false, false, 0) },{ typeof(short?).FullName,  (NpgsqlDbType.Smallint, "int2", "int2", false, true, null) },
                { typeof(int).FullName,  (NpgsqlDbType.Integer, "int4","int4 NOT NULL", false, false, 0) },{ typeof(int?).FullName,  (NpgsqlDbType.Integer, "int4", "int4", false, true, null) },
                { typeof(long).FullName,  (NpgsqlDbType.Bigint, "int8","int8 NOT NULL", false, false, 0) },{ typeof(long?).FullName,  (NpgsqlDbType.Bigint, "int8", "int8", false, true, null) },

                { typeof(byte).FullName,  (NpgsqlDbType.Smallint, "int2","int2 NOT NULL", false, false, 0) },{ typeof(byte?).FullName,  (NpgsqlDbType.Smallint, "int2", "int2", false, true, null) },
                { typeof(ushort).FullName,  (NpgsqlDbType.Integer, "int4","int4 NOT NULL", false, false, 0) },{ typeof(ushort?).FullName,  (NpgsqlDbType.Integer, "int4", "int4", false, true, null) },
                { typeof(uint).FullName,  (NpgsqlDbType.Bigint, "int8","int8 NOT NULL", false, false, 0) },{ typeof(uint?).FullName,  (NpgsqlDbType.Bigint, "int8", "int8", false, true, null) },
                { typeof(ulong).FullName,  (NpgsqlDbType.Numeric, "numeric","numeric(20,0) NOT NULL", false, false, 0) },{ typeof(ulong?).FullName,  (NpgsqlDbType.Numeric, "numeric", "numeric(20,0)", false, true, null) },

                { typeof(float).FullName,  (NpgsqlDbType.Real, "float4","float4 NOT NULL", false, false, 0) },{ typeof(float?).FullName,  (NpgsqlDbType.Real, "float4", "float4", false, true, null) },
                { typeof(double).FullName,  (NpgsqlDbType.Double, "float8","float8 NOT NULL", false, false, 0) },{ typeof(double?).FullName,  (NpgsqlDbType.Double, "float8", "float8", false, true, null) },
                { typeof(decimal).FullName,  (NpgsqlDbType.Numeric, "numeric", "numeric(10,2) NOT NULL", false, false, 0) },{ typeof(decimal?).FullName,  (NpgsqlDbType.Numeric, "numeric", "numeric(10,2)", false, true, null) },

                { typeof(string).FullName,  (NpgsqlDbType.Varchar, "varchar", "varchar(255)", false, null, "") },

                { typeof(TimeSpan).FullName,  (NpgsqlDbType.Time, "time","time NOT NULL", false, false, 0) },{ typeof(TimeSpan?).FullName,  (NpgsqlDbType.Time, "time", "time",false, true, null) },
                { typeof(DateTime).FullName,  (NpgsqlDbType.Timestamp, "timestamp", "timestamp NOT NULL", false, false, new DateTime(1970,1,1)) },{ typeof(DateTime?).FullName,  (NpgsqlDbType.Timestamp, "timestamp", "timestamp", false, true, null) },

                { typeof(bool).FullName,  (NpgsqlDbType.Boolean, "bool","bool NOT NULL", null, false, false) },{ typeof(bool?).FullName,  (NpgsqlDbType.Boolean, "bool","bool", null, true, null) },
                { typeof(Byte[]).FullName,  (NpgsqlDbType.Bytea, "bytea", "bytea", false, null, new byte[0]) },
                { typeof(BitArray).FullName,  (NpgsqlDbType.Varbit, "varbit", "varbit(64)", false, null, new BitArray(new byte[64])) },

                { typeof(NpgsqlPoint).FullName,  (NpgsqlDbType.Point, "point", "point NOT NULL", false, false, new NpgsqlPoint(0, 0)) },{ typeof(NpgsqlPoint?).FullName,  (NpgsqlDbType.Point, "point", "point", false, true, null) },
                { typeof(NpgsqlLine).FullName,  (NpgsqlDbType.Line, "line", "line NOT NULL", false, false, new NpgsqlLine(0, 0, 1)) },{ typeof(NpgsqlLine?).FullName,  (NpgsqlDbType.Line, "line", "line", false, true, null) },
                { typeof(NpgsqlLSeg).FullName,  (NpgsqlDbType.LSeg, "lseg", "lseg NOT NULL", false, false, new NpgsqlLSeg(0, 0, 0, 0)) },{ typeof(NpgsqlLSeg?).FullName,  (NpgsqlDbType.LSeg, "lseg", "lseg", false, true, null) },
                { typeof(NpgsqlBox).FullName,  (NpgsqlDbType.Box, "box", "box NOT NULL", false, false, new NpgsqlBox(0, 0, 0, 0)) },{ typeof(NpgsqlBox?).FullName,  (NpgsqlDbType.Box, "box", "box", false, true, null) },
                { typeof(NpgsqlPath).FullName,  (NpgsqlDbType.Path, "path", "path NOT NULL", false, false, new NpgsqlPath(new NpgsqlPoint(0, 0))) },{ typeof(NpgsqlPath?).FullName,  (NpgsqlDbType.Path, "path", "path", false, true, null) },
                { typeof(NpgsqlPolygon).FullName,  (NpgsqlDbType.Polygon, "polygon", "polygon NOT NULL", false, false, new NpgsqlPolygon(new NpgsqlPoint(0, 0), new NpgsqlPoint(0, 0))) },{ typeof(NpgsqlPolygon?).FullName,  (NpgsqlDbType.Polygon, "polygon", "polygon", false, true, null) },
                { typeof(NpgsqlCircle).FullName,  (NpgsqlDbType.Circle, "circle", "circle NOT NULL", false, false, new NpgsqlCircle(0, 0, 0)) },{ typeof(NpgsqlCircle?).FullName,  (NpgsqlDbType.Circle, "circle", "circle", false, true, null) },

                { typeof((IPAddress Address, int Subnet)).FullName,  (NpgsqlDbType.Cidr, "cidr", "cidr NOT NULL", false, false, (IPAddress.Any, 0)) },{ typeof((IPAddress Address, int Subnet)?).FullName,  (NpgsqlDbType.Cidr, "cidr", "cidr", false, true, null) },
                { typeof(IPAddress).FullName,  (NpgsqlDbType.Inet, "inet", "inet", false, null, IPAddress.Any) },
                { typeof(PhysicalAddress).FullName,  (NpgsqlDbType.MacAddr, "macaddr", "macaddr", false, null, PhysicalAddress.None) },

                { typeof(JToken).FullName,  (NpgsqlDbType.Jsonb, "jsonb", "jsonb", false, null, JToken.Parse("{}")) },
                { typeof(JObject).FullName,  (NpgsqlDbType.Jsonb, "jsonb", "jsonb", false, null, JObject.Parse("{}")) },
                { typeof(JArray).FullName,  (NpgsqlDbType.Jsonb, "jsonb", "jsonb", false, null, JArray.Parse("[]")) },
                { typeof(Guid).FullName,  (NpgsqlDbType.Uuid, "uuid", "uuid NOT NULL", false, false, Guid.Empty) },{ typeof(Guid?).FullName,  (NpgsqlDbType.Uuid, "uuid", "uuid", false, true, null) },

                { typeof(NpgsqlRange<int>).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Integer, "int4range", "int4range NOT NULL", false, false, NpgsqlRange<int>.Empty) },{ typeof(NpgsqlRange<int>?).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Integer, "int4range", "int4range", false, true, null) },
                { typeof(NpgsqlRange<long>).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Bigint, "int8range", "int8range NOT NULL", false, false, NpgsqlRange<long>.Empty) },{ typeof(NpgsqlRange<long>?).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Bigint, "int8range", "int8range", false, true, null) },
                { typeof(NpgsqlRange<decimal>).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Numeric, "numrange", "numrange NOT NULL", false, false, NpgsqlRange<decimal>.Empty) },{ typeof(NpgsqlRange<decimal>?).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Numeric, "numrange", "numrange", false, true, null) },
                { typeof(NpgsqlRange<DateTime>).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Timestamp, "tsrange", "tsrange NOT NULL", false, false, NpgsqlRange<DateTime>.Empty) },{ typeof(NpgsqlRange<DateTime>?).FullName,  (NpgsqlDbType.Range | NpgsqlDbType.Timestamp, "tsrange", "tsrange", false, true, null) },

                { typeof(Dictionary<string, string>).FullName,  (NpgsqlDbType.Hstore, "hstore", "hstore", false, null, new Dictionary<string, string>()) },
                { typeof(PostgisPoint).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null, new PostgisPoint(0, 0)) },
                { typeof(PostgisLineString).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null, new PostgisLineString(new []{new Coordinate2D(0, 0),new Coordinate2D(0, 0) })) },
                { typeof(PostgisPolygon).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null, new PostgisPolygon(new []{new []{new Coordinate2D(0, 0),new Coordinate2D(0, 0) }, new []{new Coordinate2D(0, 0),new Coordinate2D(0, 0) } })) },
                { typeof(PostgisMultiPoint).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null, new PostgisMultiPoint(new []{new Coordinate2D(0, 0),new Coordinate2D(0, 0) })) },
                { typeof(PostgisMultiLineString).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null, new PostgisMultiLineString(new[]{new PostgisLineString(new []{new Coordinate2D(0, 0),new Coordinate2D(0, 0) }),new PostgisLineString(new []{new Coordinate2D(0, 0),new Coordinate2D(0, 0) }) })) },
                { typeof(PostgisMultiPolygon).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null, new PostgisMultiPolygon(new[]{new PostgisPolygon(new []{new []{new Coordinate2D(0, 0),new Coordinate2D(0, 0) }, new []{new Coordinate2D(0, 0),new Coordinate2D(0, 0) } }),new PostgisPolygon(new []{new []{new Coordinate2D(0, 0),new Coordinate2D(0, 0) }, new []{new Coordinate2D(0, 0),new Coordinate2D(0, 0) } }) })) },
                { typeof(PostgisGeometry).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null, new PostgisPoint(0, 0)) },
                { typeof(PostgisGeometryCollection).FullName,  (NpgsqlDbType.Geometry, "geometry", "geometry", false, null, new PostgisGeometryCollection(new[]{new PostgisPoint(0, 0),new PostgisPoint(0, 0) })) },
            };

        public override (int type, string dbtype, string dbtypeFull, bool? isnullable, object defaultValue)? GetDbInfo(Type type)
        {
            var isarray = type.FullName != "System.Byte[]" && type.IsArray;
            var elementType = isarray ? type.GetElementType() : type;
            var info = GetDbInfoNoneArray(elementType);
            if (info == null) return null;
            if (isarray == false) return ((int)info.Value.type, info.Value.dbtype, info.Value.dbtypeFull, info.Value.isnullable, info.Value.defaultValue);
            var dbtypefull = Regex.Replace(info.Value.dbtypeFull, $@"{info.Value.dbtype}(\s*\([^\)]+\))?", "$0[]").Replace(" NOT NULL", "");
            return ((int)(info.Value.type | NpgsqlDbType.Array), $"{info.Value.dbtype}[]", dbtypefull, null, Array.CreateInstance(elementType, 0));
        }
        (NpgsqlDbType type, string dbtype, string dbtypeFull, bool? isnullable, object defaultValue)? GetDbInfoNoneArray(Type type)
        {
            if (_dicCsToDb.TryGetValue(type.FullName, out var trydc)) return new (NpgsqlDbType, string, string, bool?, object)?((trydc.type, trydc.dbtype, trydc.dbtypeFull, trydc.isnullable, trydc.defaultValue));
            if (type.IsArray) return null;
            var enumType = type.IsEnum ? type : null;
            if (enumType == null && type.IsNullableType() && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments.First().IsEnum) enumType = type.GenericTypeArguments.First();
            if (enumType != null)
            {
                var newItem = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any() ?
                    (NpgsqlDbType.Bigint, "int8", $"int8{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0)) :
                    (NpgsqlDbType.Integer, "int4", $"int4{(type.IsEnum ? " NOT NULL" : "")}", false, type.IsEnum ? false : true, Enum.GetValues(enumType).GetValue(0));
                if (_dicCsToDb.ContainsKey(type.FullName) == false)
                {
                    lock (_dicCsToDbLock)
                    {
                        if (_dicCsToDb.ContainsKey(type.FullName) == false)
                            _dicCsToDb.Add(type.FullName, newItem);
                    }
                }
                return (newItem.Item1, newItem.Item2, newItem.Item3, newItem.Item5, newItem.Item6);
            }
            return null;
        }

        public override string GetComparisonDDLStatements(params Type[] entityTypes)
        {
            var sb = new StringBuilder();
            var seqcols = new List<(ColumnInfo, string[], bool)>(); //序列

            foreach (var entityType in entityTypes)
            {
                if (sb.Length > 0) sb.Append("\r\n");
                var tb = _commonUtils.GetTableByEntity(entityType);
                if (tb == null) throw new Exception($"类型 {entityType.FullName} 不可迁移");
                if (tb.Columns.Any() == false) throw new Exception($"类型 {entityType.FullName} 不可迁移，可迁移属性0个");
                var tbname = tb.DbName.Split(new[] { '.' }, 2);
                if (tbname?.Length == 1) tbname = new[] { "public", tbname[0] };

                var tboldname = tb.DbOldName?.Split(new[] { '.' }, 2); //旧表名
                if (tboldname?.Length == 1) tboldname = new[] { "public", tboldname[0] };

                if (string.Compare(tbname[0], "public", true) != 0 && _orm.Ado.ExecuteScalar(CommandType.Text, _commonUtils.FormatSql(" select 1 from pg_namespace where nspname={0}", tbname[0])) == null) //创建模式
                    sb.Append("CREATE SCHEMA IF NOT EXISTS ").Append(tbname[0]).Append(";\r\n");

                var sbalter = new StringBuilder();
                var istmpatler = false; //创建临时表，导入数据，删除旧表，修改
                if (_orm.Ado.ExecuteScalar(CommandType.Text, string.Format(" select 1 from pg_tables a inner join pg_namespace b on b.nspname = a.schemaname where b.nspname || '.' || a.tablename = '{0}.{1}'", tbname)) == null)
                { //表不存在
                    if (tboldname != null)
                    {
                        if (_orm.Ado.ExecuteScalar(CommandType.Text, string.Format(" select 1 from pg_tables a inner join pg_namespace b on b.nspname = a.schemaname where b.nspname || '.' || a.tablename = '{0}.{1}'", tboldname)) == null)
                            //旧表不存在
                            tboldname = null;
                    }
                    if (tboldname == null)
                    {
                        //创建表
                        sb.Append("CREATE TABLE IF NOT EXISTS ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ( ");
                        foreach (var tbcol in tb.Columns.Values)
                        {
                            sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType).Append(",");
                            if (tbcol.Attribute.IsIdentity == true) seqcols.Add((tbcol, tbname, true));
                        }
                        if (tb.Primarys.Any())
                        {
                            sb.Append(" \r\n  CONSTRAINT ").Append(tbname[0]).Append("_").Append(tbname[1]).Append("_pkey PRIMARY KEY (");
                            foreach (var tbcol in tb.Primarys) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                            sb.Remove(sb.Length - 2, 2).Append("),");
                        }
                        foreach (var uk in tb.Uniques)
                        {
                            sb.Append(" \r\n  CONSTRAINT ").Append(_commonUtils.QuoteSqlName(uk.Key)).Append(" UNIQUE(");
                            foreach (var tbcol in uk.Value) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                            sb.Remove(sb.Length - 2, 2).Append("),");
                        }
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append("\r\n) WITH (OIDS=FALSE);\r\n");
                        //备注
                        foreach (var tbcol in tb.Columns.Values)
                        {
                            if (string.IsNullOrEmpty(tbcol.Comment) == false)
                                sb.Append("COMMENT ON COLUMN ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}.{tbcol.Attribute.Name}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment)).Append(";\r\n");
                        }
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

                //对比字段，只可以修改类型、增加字段、有限的修改字段名；保证安全不删除字段
                var sql = _commonUtils.FormatSql(@"
select
a.attname,
t.typname,
case when a.atttypmod > 0 and a.atttypmod < 32767 then a.atttypmod - 4 else a.attlen end len,
case when t.typelem > 0 and t.typinput::varchar = 'array_in' then t2.typname else t.typname end,
case when a.attnotnull then '0' else '1' end as is_nullable,
e.adsrc,
a.attndims,
d.description as comment
from pg_class c
inner join pg_attribute a on a.attnum > 0 and a.attrelid = c.oid
inner join pg_type t on t.oid = a.atttypid
left join pg_type t2 on t2.oid = t.typelem
left join pg_description d on d.objoid = a.attrelid and d.objsubid = a.attnum
left join pg_attrdef e on e.adrelid = a.attrelid and e.adnum = a.attnum
inner join pg_namespace ns on ns.oid = c.relnamespace
inner join pg_namespace ns2 on ns2.oid = t.typnamespace
where ns.nspname = {0} and c.relname = {1}", tboldname ?? tbname);
                var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                var tbstruct = ds.ToDictionary(a => string.Concat(a[0]), a =>
                {
                    var attndims = int.Parse(string.Concat(a[6]));
                    var type = string.Concat(a[1]);
                    var sqlType = string.Concat(a[3]);
                    var max_length = long.Parse(string.Concat(a[2]));
                    switch (sqlType.ToLower())
                    {
                        case "bool": case "name": case "bit": case "varbit": case "bpchar": case "varchar": case "bytea": case "text": case "uuid": break;
                        default: max_length *= 8; break;
                    }
                    if (type.StartsWith("_"))
                    {
                        type = type.Substring(1);
                        if (attndims == 0) attndims++;
                    }
                    if (sqlType.StartsWith("_")) sqlType = sqlType.Substring(1);
                    return new
                    {
                        column = string.Concat(a[0]),
                        sqlType = string.Concat(sqlType),
                        max_length = long.Parse(string.Concat(a[2])),
                        is_nullable = string.Concat(a[4]) == "1",
                        is_identity = string.Concat(a[5]).StartsWith(@"nextval('") && string.Concat(a[5]).EndsWith(@"'::regclass)"),
                        attndims,
                        comment = string.Concat(a[7])
                    };
                }, StringComparer.CurrentCultureIgnoreCase);

                if (istmpatler == false)
                {
                    foreach (var tbcol in tb.Columns.Values)
                    {
                        if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
                            string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol))
                        {
                            var isCommentChanged = tbstructcol.comment != (tbcol.Comment ?? "");
                            if (tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType, StringComparison.CurrentCultureIgnoreCase) == false ||
                                tbcol.Attribute.DbType.Contains("[]") != (tbstructcol.attndims > 0))
                                sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ALTER COLUMN ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" TYPE ").Append(tbcol.Attribute.DbType.Split(' ').First()).Append(";\r\n");
                            if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable)
                                sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ALTER COLUMN ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" ").Append(tbcol.Attribute.IsNullable == true ? "DROP" : "SET").Append(" NOT NULL;\r\n");
                            if (tbcol.Attribute.IsIdentity != tbstructcol.is_identity)
                                seqcols.Add((tbcol, tbname, tbcol.Attribute.IsIdentity == true));
                            if (string.Compare(tbstructcol.column, tbcol.Attribute.OldName, true) == 0)
                                //修改列名
                                sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" RENAME COLUMN ").Append(_commonUtils.QuoteSqlName(tbstructcol.column)).Append(" TO ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(";\r\n");
                            if (isCommentChanged)
                                sbalter.Append("COMMENT ON COLUMN ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}.{tbcol.Attribute.Name}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment)).Append(";\r\n");
                            continue;
                        }
                        //添加列
                        sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ADD COLUMN ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType.Split(' ').First()).Append(";\r\n");
                        sbalter.Append("UPDATE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" SET ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" = ").Append(_commonUtils.FormatSql("{0};\r\n", tbcol.Attribute.DbDefautValue));
                        if (tbcol.Attribute.IsNullable == false) sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ALTER COLUMN ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" SET NOT NULL;\r\n");
                        if (tbcol.Attribute.IsIdentity == true) seqcols.Add((tbcol, tbname, tbcol.Attribute.IsIdentity == true));
                        if (string.IsNullOrEmpty(tbcol.Comment) == false) sbalter.Append("COMMENT ON COLUMN ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}.{tbcol.Attribute.Name}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment)).Append(";\r\n");
                    }
                    var dsuksql = _commonUtils.FormatSql(@"
select
c.attname,
b.relname
from pg_index a
inner join pg_class b on b.oid = a.indexrelid
inner join pg_attribute c on c.attnum > 0 and c.attrelid = b.oid
inner join pg_namespace ns on ns.oid = b.relnamespace
inner join pg_class d on d.oid = a.indrelid
where ns.nspname in ({0}) and d.relname in ({1}) and a.indisunique = 't'", tboldname ?? tbname);
                    var dsuk = _orm.Ado.ExecuteArray(CommandType.Text, dsuksql).Select(a => new[] { string.Concat(a[0]), string.Concat(a[1]) });
                    foreach (var uk in tb.Uniques)
                    {
                        if (string.IsNullOrEmpty(uk.Key) || uk.Value.Any() == false) continue;
                        var dsukfind1 = dsuk.Where(a => string.Compare(a[1], uk.Key, true) == 0).ToArray();
                        if (dsukfind1.Any() == false || dsukfind1.Length != uk.Value.Count || dsukfind1.Where(a => uk.Value.Where(b => string.Compare(b.Attribute.Name, a[0], true) == 0).Any()).Count() != uk.Value.Count)
                        {
                            if (dsukfind1.Any()) sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" DROP CONSTRAINT ").Append(_commonUtils.QuoteSqlName(uk.Key)).Append(";\r\n");
                            sbalter.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" ADD CONSTRAINT ").Append(_commonUtils.QuoteSqlName(uk.Key)).Append(" UNIQUE(");
                            foreach (var tbcol in uk.Value) sbalter.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                            sbalter.Remove(sbalter.Length - 2, 2).Append(");\r\n");
                        }
                    }
                }
                if (istmpatler == false)
                {
                    sb.Append(sbalter);
                    continue;
                }
                var oldpk = _orm.Ado.ExecuteScalar(CommandType.Text, _commonUtils.FormatSql(@"select pg_constraint.conname as pk_name from pg_constraint
inner join pg_class on pg_constraint.conrelid = pg_class.oid
inner join pg_namespace on pg_namespace.oid = pg_class.relnamespace
where pg_namespace.nspname={0} and pg_class.relname={1} and pg_constraint.contype='p'
", tbname))?.ToString();
                if (string.IsNullOrEmpty(oldpk) == false)
                    sb.Append("ALTER TABLE ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}")).Append(" DROP CONSTRAINT ").Append(oldpk).Append(";\r\n");

                //创建临时表，数据导进临时表，然后删除原表，将临时表改名为原表名
                var tablename = tboldname == null ? _commonUtils.QuoteSqlName($"{tbname[0]}.{tbname[1]}") : _commonUtils.QuoteSqlName($"{tboldname[0]}.{tboldname[1]}");
                var tmptablename = _commonUtils.QuoteSqlName($"{tbname[0]}.FreeSqlTmp_{tbname[1]}");
                //创建临时表
                sb.Append("CREATE TABLE IF NOT EXISTS ").Append(tmptablename).Append(" ( ");
                foreach (var tbcol in tb.Columns.Values)
                {
                    sb.Append(" \r\n  ").Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(" ").Append(tbcol.Attribute.DbType).Append(",");
                    if (tbcol.Attribute.IsIdentity == true) seqcols.Add((tbcol, tbname, true));
                }
                if (tb.Primarys.Any())
                {
                    sb.Append(" \r\n  CONSTRAINT ").Append(tbname[0]).Append("_").Append(tbname[1]).Append("_pkey PRIMARY KEY (");
                    foreach (var tbcol in tb.Primarys) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                    sb.Remove(sb.Length - 2, 2).Append("),");
                }
                foreach (var uk in tb.Uniques)
                {
                    sb.Append(" \r\n  CONSTRAINT ").Append(_commonUtils.QuoteSqlName(uk.Key)).Append(" UNIQUE(");
                    foreach (var tbcol in uk.Value) sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                    sb.Remove(sb.Length - 2, 2).Append("),");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append("\r\n) WITH (OIDS=FALSE);\r\n");
                //备注
                foreach (var tbcol in tb.Columns.Values)
                {
                    if (string.IsNullOrEmpty(tbcol.Comment) == false)
                        sb.Append("COMMENT ON COLUMN ").Append(_commonUtils.QuoteSqlName($"{tbname[0]}.FreeSqlTmp_{tbname[1]}.{tbcol.Attribute.Name}")).Append(" IS ").Append(_commonUtils.FormatSql("{0}", tbcol.Comment)).Append(";\r\n");
                }
                sb.Append("INSERT INTO ").Append(tmptablename).Append(" (");
                foreach (var tbcol in tb.Columns.Values)
                    sb.Append(_commonUtils.QuoteSqlName(tbcol.Attribute.Name)).Append(", ");
                sb.Remove(sb.Length - 2, 2).Append(")\r\nSELECT ");
                foreach (var tbcol in tb.Columns.Values)
                {
                    var insertvalue = "NULL";
                    if (tbstruct.TryGetValue(tbcol.Attribute.Name, out var tbstructcol) ||
                        string.IsNullOrEmpty(tbcol.Attribute.OldName) == false && tbstruct.TryGetValue(tbcol.Attribute.OldName, out tbstructcol))
                    {
                        insertvalue = _commonUtils.QuoteSqlName(tbstructcol.column);
                        if (tbcol.Attribute.DbType.StartsWith(tbstructcol.sqlType, StringComparison.CurrentCultureIgnoreCase) == false)
                            insertvalue = $"cast({insertvalue} as {tbcol.Attribute.DbType.Split(' ').First()})";
                        if (tbcol.Attribute.IsNullable != tbstructcol.is_nullable)
                            insertvalue = $"coalesce({insertvalue},{_commonUtils.FormatSql("{0}", tbcol.Attribute.DbDefautValue)})";
                    }
                    else if (tbcol.Attribute.IsNullable == false)
                        insertvalue = _commonUtils.FormatSql("{0}", tbcol.Attribute.DbDefautValue);
                    sb.Append(insertvalue).Append(", ");
                }
                sb.Remove(sb.Length - 2, 2).Append(" FROM ").Append(tablename).Append(";\r\n");
                sb.Append("DROP TABLE ").Append(tablename).Append(";\r\n");
                sb.Append("ALTER TABLE ").Append(tmptablename).Append(" RENAME TO ").Append(_commonUtils.QuoteSqlName(tbname[1])).Append(";\r\n");
            }
            foreach (var seqcol in seqcols)
            {
                var tbname = seqcol.Item2;
                var seqname = Utils.GetCsName($"{tbname[0]}.{tbname[1]}_{seqcol.Item1.Attribute.Name}_sequence_name");
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
    }
}