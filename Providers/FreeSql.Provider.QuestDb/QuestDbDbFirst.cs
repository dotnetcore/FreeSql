using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
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
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.QuestDb
{
    class QuestDbDbFirst : IDbFirst
    {
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;

        public QuestDbDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public bool IsPg10 => ServerVersion >= 10;

        public int ServerVersion
        {
            get
            {
                if (_ServerVersionValue == 0 && _orm.Ado.MasterPool != null)
                    using (var conn = _orm.Ado.MasterPool.Get())
                    {
                        try
                        {
                            _ServerVersionValue = ParsePgVersion(conn.Value.ServerVersion, 10, 0).Item2;
                        }
                        catch
                        {
                            _ServerVersionValue = 9;
                        }
                    }

                return _ServerVersionValue;
            }
        }

        int _ServerVersionValue = 0;

        public int GetDbType(DbColumnInfo column) => (int)GetNpgsqlDbType(column);

        NpgsqlDbType GetNpgsqlDbType(DbColumnInfo column)
        {
            var dbtype = column.DbTypeText;
            var isarray = dbtype?.EndsWith("[]") == true;
            if (isarray) dbtype = dbtype.Remove(dbtype.Length - 2);
            NpgsqlDbType ret = NpgsqlDbType.Unknown;
            switch (dbtype?.ToLower().TrimStart('_'))
            {
                case "short":
                    ret = NpgsqlDbType.Smallint;
                    break;
                case "int":
                    ret = NpgsqlDbType.Integer;
                    break;
                case "long":
                    ret = NpgsqlDbType.Bigint;
                    break;
                case "numeric":
                    ret = NpgsqlDbType.Numeric;
                    break;
                case "float":
                    ret = NpgsqlDbType.Real;
                    break;
                case "double":
                    ret = NpgsqlDbType.Double;
                    break;
                case "char":
                    ret = NpgsqlDbType.Char;
                    break;
                case "string":
                    ret = NpgsqlDbType.Varchar;
                    break;
                case "timestamp":
                    ret = NpgsqlDbType.Timestamp;
                    break;
                case "timestamptz":
                    ret = NpgsqlDbType.TimestampTz;
                    break;
                case "date":
                    ret = NpgsqlDbType.Date;
                    break;
                case "time":
                    ret = NpgsqlDbType.Time;
                    break;
                case "timetz":
                    ret = NpgsqlDbType.TimeTz;
                    break;
                case "interval":
                    ret = NpgsqlDbType.Interval;
                    break;

                case "bool":
                    ret = NpgsqlDbType.Boolean;
                    break;
                case "bytea":
                    ret = NpgsqlDbType.Bytea;
                    break;
                case "bit":
                    ret = NpgsqlDbType.Bit;
                    break;
                case "varbit":
                    ret = NpgsqlDbType.Varbit;
                    break;

                case "point":
                    ret = NpgsqlDbType.Point;
                    break;
                case "line":
                    ret = NpgsqlDbType.Line;
                    break;
                case "lseg":
                    ret = NpgsqlDbType.LSeg;
                    break;
                case "box":
                    ret = NpgsqlDbType.Box;
                    break;
                case "path":
                    ret = NpgsqlDbType.Path;
                    break;
                case "polygon":
                    ret = NpgsqlDbType.Polygon;
                    break;
                case "circle":
                    ret = NpgsqlDbType.Circle;
                    break;

                case "cidr":
                    ret = NpgsqlDbType.Cidr;
                    break;
                case "inet":
                    ret = NpgsqlDbType.Inet;
                    break;
                case "macaddr":
                    ret = NpgsqlDbType.MacAddr;
                    break;

                case "json":
                    ret = NpgsqlDbType.Json;
                    break;
                case "jsonb":
                    ret = NpgsqlDbType.Jsonb;
                    break;
                case "uuid":
                    ret = NpgsqlDbType.Uuid;
                    break;

                case "int4range":
                    ret = NpgsqlDbType.Range | NpgsqlDbType.Integer;
                    break;
                case "int8range":
                    ret = NpgsqlDbType.Range | NpgsqlDbType.Bigint;
                    break;
                case "numrange":
                    ret = NpgsqlDbType.Range | NpgsqlDbType.Numeric;
                    break;
                case "tsrange":
                    ret = NpgsqlDbType.Range | NpgsqlDbType.Timestamp;
                    break;
                case "tstzrange":
                    ret = NpgsqlDbType.Range | NpgsqlDbType.TimestampTz;
                    break;
                case "daterange":
                    ret = NpgsqlDbType.Range | NpgsqlDbType.Date;
                    break;

                case "hstore":
                    ret = NpgsqlDbType.Hstore;
                    break;
                case "geometry":
                    ret = NpgsqlDbType.Geometry;
                    break;
            }

            return isarray ? (ret | NpgsqlDbType.Array) : ret;
        }

        static readonly
            Dictionary<int, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type
                csNullableTypeInfo, string csTypeValue, string dataReaderMethod)> _dicDbToCs =
                new Dictionary<int, (string csConvert, string csParse, string csStringify, string csType, Type
                    csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)>()
                {
                    {
                        (int)NpgsqlDbType.Smallint,
                        ("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(short), typeof(short?),
                            "{0}.Value", "GetInt16")
                    },
                    {
                        (int)NpgsqlDbType.Integer,
                        ("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(int), typeof(int?), "{0}.Value",
                            "GetInt32")
                    },
                    {
                        (int)NpgsqlDbType.Bigint,
                        ("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?),
                            "{0}.Value", "GetInt64")
                    },
                    {
                        (int)NpgsqlDbType.Numeric,
                        ("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal),
                            typeof(decimal?), "{0}.Value", "GetDecimal")
                    },
                    {
                        (int)NpgsqlDbType.Real,
                        ("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?),
                            "{0}.Value", "GetFloat")
                    },
                    {
                        (int)NpgsqlDbType.Double,
                        ("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?),
                            "{0}.Value", "GetDouble")
                    },
                    {
                        (int)NpgsqlDbType.Money,
                        ("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal),
                            typeof(decimal?), "{0}.Value", "GetDecimal")
                    },

                    {
                        (int)NpgsqlDbType.Char,
                        ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string",
                            typeof(string), typeof(string), "{0}", "GetString")
                    },
                    {
                        (int)NpgsqlDbType.Varchar,
                        ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string",
                            typeof(string), typeof(string), "{0}", "GetString")
                    },
                    {
                        (int)NpgsqlDbType.Text,
                        ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string",
                            typeof(string), typeof(string), "{0}", "GetString")
                    },

                    {
                        (int)NpgsqlDbType.Timestamp,
                        ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?",
                            typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime")
                    },
                    {
                        (int)NpgsqlDbType.TimestampTz,
                        ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?",
                            typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime")
                    },
                    {
                        (int)NpgsqlDbType.Date,
                        ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?",
                            typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime")
                    },
                    {
                        (int)NpgsqlDbType.Time,
                        ("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?",
                            typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue")
                    },
                    {
                        (int)NpgsqlDbType.TimeTz,
                        ("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?",
                            typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue")
                    },
                    {
                        (int)NpgsqlDbType.Interval,
                        ("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?",
                            typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue")
                    },

                    {
                        (int)NpgsqlDbType.Boolean,
                        ("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?),
                            "{0}.Value", "GetBoolean")
                    },
                    {
                        (int)NpgsqlDbType.Bytea,
                        ("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]",
                            typeof(byte[]), typeof(byte[]), "{0}", "GetValue")
                    },
                    {
                        (int)NpgsqlDbType.Bit,
                        ("(BitArray)", "{0}.ToBitArray()", "{0}.To1010()", "BitArray", typeof(BitArray),
                            typeof(BitArray), "{0}", "GetValue")
                    },
                    {
                        (int)NpgsqlDbType.Varbit,
                        ("(BitArray)", "{0}.ToBitArray()", "{0}.To1010()", "BitArray", typeof(BitArray),
                            typeof(BitArray), "{0}", "GetValue")
                    },

                    {
                        (int)NpgsqlDbType.Point,
                        ("(NpgsqlPoint?)", "NpgsqlPoint.Parse({0})", "{0}.ToString()", "NpgsqlPoint",
                            typeof(NpgsqlPoint), typeof(NpgsqlPoint?), "{0}", "GetValue")
                    },
                    {
                        (int)NpgsqlDbType.Line,
                        ("(NpgsqlLine?)", "NpgsqlLine.Parse({0})", "{0}.ToString()", "NpgsqlLine", typeof(NpgsqlLine),
                            typeof(NpgsqlLine?), "{0}", "GetValue")
                    },
                    {
                        (int)NpgsqlDbType.LSeg,
                        ("(NpgsqlLSeg?)", "NpgsqlLSeg.Parse({0})", "{0}.ToString()", "NpgsqlLSeg", typeof(NpgsqlLSeg),
                            typeof(NpgsqlLSeg?), "{0}", "GetValue")
                    },
                    {
                        (int)NpgsqlDbType.Box,
                        ("(NpgsqlBox?)", "NpgsqlBox.Parse({0})", "{0}.ToString()", "NpgsqlBox", typeof(NpgsqlBox),
                            typeof(NpgsqlBox?), "{0}", "GetValue")
                    },
                    {
                        (int)NpgsqlDbType.Path,
                        ("(NpgsqlPath?)", "NpgsqlPath.Parse({0})", "{0}.ToString()", "NpgsqlPath", typeof(NpgsqlPath),
                            typeof(NpgsqlPath?), "{0}", "GetValue")
                    },
                    {
                        (int)NpgsqlDbType.Polygon,
                        ("(NpgsqlPolygon?)", "NpgsqlPolygon.Parse({0})", "{0}.ToString()", "NpgsqlPolygon",
                            typeof(NpgsqlPolygon), typeof(NpgsqlPolygon?), "{0}", "GetValue")
                    },
                    {
                        (int)NpgsqlDbType.Circle,
                        ("(NpgsqlCircle?)", "NpgsqlCircle.Parse({0})", "{0}.ToString()", "NpgsqlCircle",
                            typeof(NpgsqlCircle), typeof(NpgsqlCircle?), "{0}", "GetValue")
                    },

                    {
                        (int)NpgsqlDbType.Cidr,
                        ("((IPAddress, int)?)", "(IPAddress, int)({0})", "{0}.ToString()", "(IPAddress, int)",
                            typeof((IPAddress, int)), typeof((IPAddress, int)?), "{0}", "GetValue")
                    },
                    {
                        (int)NpgsqlDbType.Inet,
                        ("(IPAddress)", "IPAddress.Parse({0})", "{0}.ToString()", "IPAddress", typeof(IPAddress),
                            typeof(IPAddress), "{0}", "GetValue")
                    },
                    {
                        (int)NpgsqlDbType.MacAddr,
                        ("(PhysicalAddress?)", "PhysicalAddress.Parse({0})", "{0}.ToString()", "PhysicalAddress",
                            typeof(PhysicalAddress), typeof(PhysicalAddress), "{0}", "GetValue")
                    },

                    {
                        (int)NpgsqlDbType.Json,
                        ("(JToken)", "JToken.Parse({0})", "{0}.ToString()", "JToken", typeof(JToken), typeof(JToken),
                            "{0}", "GetString")
                    },
                    {
                        (int)NpgsqlDbType.Jsonb,
                        ("(JToken)", "JToken.Parse({0})", "{0}.ToString()", "JToken", typeof(JToken), typeof(JToken),
                            "{0}", "GetString")
                    },
                    {
                        (int)NpgsqlDbType.Uuid,
                        ("(Guid?)", "Guid.Parse({0})", "{0}.ToString()", "Guid", typeof(Guid), typeof(Guid?), "{0}",
                            "GetString")
                    },

                    {
                        (int)(NpgsqlDbType.Range | NpgsqlDbType.Integer),
                        ("(NpgsqlRange<int>?)", "{0}.ToNpgsqlRange<int>()", "{0}.ToString()", "NpgsqlRange<int>",
                            typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>?), "{0}", "GetString")
                    },
                    {
                        (int)(NpgsqlDbType.Range | NpgsqlDbType.Bigint),
                        ("(NpgsqlRange<long>?)", "{0}.ToNpgsqlRange<long>()", "{0}.ToString()", "NpgsqlRange<long>",
                            typeof(NpgsqlRange<long>), typeof(NpgsqlRange<long>?), "{0}", "GetString")
                    },
                    {
                        (int)(NpgsqlDbType.Range | NpgsqlDbType.Numeric),
                        ("(NpgsqlRange<decimal>?)", "{0}.ToNpgsqlRange<decimal>()", "{0}.ToString()",
                            "NpgsqlRange<decimal>", typeof(NpgsqlRange<decimal>), typeof(NpgsqlRange<decimal>?), "{0}",
                            "GetString")
                    },
                    {
                        (int)(NpgsqlDbType.Range | NpgsqlDbType.Timestamp),
                        ("(NpgsqlRange<DateTime>?)", "{0}.ToNpgsqlRange<DateTime>()", "{0}.ToString()",
                            "NpgsqlRange<DateTime>", typeof(NpgsqlRange<DateTime>), typeof(NpgsqlRange<DateTime>?),
                            "{0}", "GetString")
                    },
                    {
                        (int)(NpgsqlDbType.Range | NpgsqlDbType.TimestampTz),
                        ("(NpgsqlRange<DateTime>?)", "{0}.ToNpgsqlRange<DateTime>()", "{0}.ToString()",
                            "NpgsqlRange<DateTime>", typeof(NpgsqlRange<DateTime>), typeof(NpgsqlRange<DateTime>?),
                            "{0}", "GetString")
                    },
                    {
                        (int)(NpgsqlDbType.Range | NpgsqlDbType.Date),
                        ("(NpgsqlRange<DateTime>?)", "{0}.ToNpgsqlRange<DateTime>()", "{0}.ToString()",
                            "NpgsqlRange<DateTime>", typeof(NpgsqlRange<DateTime>), typeof(NpgsqlRange<DateTime>?),
                            "{0}", "GetString")
                    },

                    {
                        (int)NpgsqlDbType.Hstore,
                        ("(Dictionary<string, string>)",
                            "JsonConvert.DeserializeObject<Dictionary<string, string>>({0})",
                            "JsonConvert.SerializeObject({0})", "Dictionary<string, string>",
                            typeof(Dictionary<string, string>), typeof(Dictionary<string, string>), "{0}", "GetValue")
                    },
                    {
                        (int)NpgsqlDbType.Geometry,
                        ("(PostgisGeometry)", "JsonConvert.DeserializeObject<PostgisGeometry>({0})",
                            "JsonConvert.SerializeObject({0})", "PostgisGeometry", typeof(PostgisGeometry),
                            typeof(PostgisGeometry), "{0}", "GetValue")
                    },

                    /*** array ***/

                    {
                        (int)(NpgsqlDbType.Smallint | NpgsqlDbType.Array),
                        ("(short[])", "JsonConvert.DeserializeObject<short[]>({0})", "JsonConvert.SerializeObject({0})",
                            "short[]", typeof(short[]), typeof(short[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Integer | NpgsqlDbType.Array),
                        ("(int[])", "JsonConvert.DeserializeObject<int[]>({0})", "JsonConvert.SerializeObject({0})",
                            "int[]", typeof(int[]), typeof(int[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Bigint | NpgsqlDbType.Array),
                        ("(long[])", "JsonConvert.DeserializeObject<long[]>({0})", "JsonConvert.SerializeObject({0})",
                            "long[]", typeof(long[]), typeof(long[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Numeric | NpgsqlDbType.Array),
                        ("(decimal[])", "JsonConvert.DeserializeObject<decimal[]>({0})",
                            "JsonConvert.SerializeObject({0})", "decimal[]", typeof(decimal[]), typeof(decimal[]),
                            "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Real | NpgsqlDbType.Array),
                        ("(float[])", "JsonConvert.DeserializeObject<float[]>({0})", "JsonConvert.SerializeObject({0})",
                            "float[]", typeof(float[]), typeof(float[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Double | NpgsqlDbType.Array),
                        ("(double[])", "JsonConvert.DeserializeObject<double[]>({0})",
                            "JsonConvert.SerializeObject({0})", "double[]", typeof(double[]), typeof(double[]), "{0}",
                            "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Money | NpgsqlDbType.Array),
                        ("(decimal[])", "JsonConvert.DeserializeObject<decimal[]>({0})",
                            "JsonConvert.SerializeObject({0})", "decimal[]", typeof(decimal[]), typeof(decimal[]),
                            "{0}", "GetValue")
                    },

                    {
                        (int)(NpgsqlDbType.Char | NpgsqlDbType.Array),
                        ("(string[])", "JsonConvert.DeserializeObject<string[]>({0})",
                            "JsonConvert.SerializeObject({0})", "string[]", typeof(string[]), typeof(string[]), "{0}",
                            "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Varchar | NpgsqlDbType.Array),
                        ("(string[])", "JsonConvert.DeserializeObject<string[]>({0})",
                            "JsonConvert.SerializeObject({0})", "string[]", typeof(string[]), typeof(string[]), "{0}",
                            "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Text | NpgsqlDbType.Array),
                        ("(string[])", "JsonConvert.DeserializeObject<string[]>({0})",
                            "JsonConvert.SerializeObject({0})", "string[]", typeof(string[]), typeof(string[]), "{0}",
                            "GetValue")
                    },

                    {
                        (int)(NpgsqlDbType.Timestamp | NpgsqlDbType.Array),
                        ("(DateTime[])", "JsonConvert.DeserializeObject<DateTime[]>({0})",
                            "JsonConvert.SerializeObject({0})", "DateTime[]", typeof(DateTime[]), typeof(DateTime[]),
                            "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.TimestampTz | NpgsqlDbType.Array),
                        ("(DateTime[])", "JsonConvert.DeserializeObject<DateTime[]>({0})",
                            "JsonConvert.SerializeObject({0})", "DateTime[]", typeof(DateTime[]), typeof(DateTime[]),
                            "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Date | NpgsqlDbType.Array),
                        ("(DateTime[])", "JsonConvert.DeserializeObject<DateTime[]>({0})",
                            "JsonConvert.SerializeObject({0})", "DateTime[]", typeof(DateTime[]), typeof(DateTime[]),
                            "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Time | NpgsqlDbType.Array),
                        ("(TimeSpan[])", "JsonConvert.DeserializeObject<TimeSpan[]>({0})",
                            "JsonConvert.SerializeObject({0})", "TimeSpan[]", typeof(TimeSpan[]), typeof(TimeSpan[]),
                            "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.TimeTz | NpgsqlDbType.Array),
                        ("(TimeSpan[])", "JsonConvert.DeserializeObject<TimeSpan[]>({0})",
                            "JsonConvert.SerializeObject({0})", "TimeSpan[]", typeof(TimeSpan[]), typeof(TimeSpan[]),
                            "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Interval | NpgsqlDbType.Array),
                        ("(TimeSpan[])", "JsonConvert.DeserializeObject<TimeSpan[]>({0})",
                            "JsonConvert.SerializeObject({0})", "TimeSpan[]", typeof(TimeSpan[]), typeof(TimeSpan[]),
                            "{0}", "GetValue")
                    },

                    {
                        (int)(NpgsqlDbType.Boolean | NpgsqlDbType.Array),
                        ("(bool[])", "JsonConvert.DeserializeObject<bool[]>({0})", "JsonConvert.SerializeObject({0})",
                            "bool[]", typeof(bool[]), typeof(bool[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Bytea | NpgsqlDbType.Array),
                        ("(byte[][])", "JsonConvert.DeserializeObject<byte[][]>({0})",
                            "JsonConvert.SerializeObject({0})", "byte[][]", typeof(byte[][]), typeof(byte[][]), "{0}",
                            "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Bit | NpgsqlDbType.Array),
                        ("(BitArray[])", "JsonConvert.DeserializeObject<BitArray[]>({0})",
                            "JsonConvert.SerializeObject({0})", "BitArray[]", typeof(BitArray[]), typeof(BitArray[]),
                            "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Varbit | NpgsqlDbType.Array),
                        ("(BitArray[])", "JsonConvert.DeserializeObject<BitArray[]>({0})",
                            "JsonConvert.SerializeObject({0})", "BitArray[]", typeof(BitArray[]), typeof(BitArray[]),
                            "{0}", "GetValue")
                    },

                    {
                        (int)(NpgsqlDbType.Point | NpgsqlDbType.Array),
                        ("(NpgsqlPoint[])", "JsonConvert.DeserializeObject<NpgsqlPoint[]>({0})",
                            "JsonConvert.SerializeObject({0})", "NpgsqlPoint[]", typeof(NpgsqlPoint[]),
                            typeof(NpgsqlPoint[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Line | NpgsqlDbType.Array),
                        ("(NpgsqlLine[])", "JsonConvert.DeserializeObject<BNpgsqlLineitArray[]>({0})",
                            "JsonConvert.SerializeObject({0})", "NpgsqlLine[]", typeof(NpgsqlLine[]),
                            typeof(NpgsqlLine[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.LSeg | NpgsqlDbType.Array),
                        ("(NpgsqlLSeg[])", "JsonConvert.DeserializeObject<NpgsqlLSeg[]>({0})",
                            "JsonConvert.SerializeObject({0})", "NpgsqlLSeg[]", typeof(NpgsqlLSeg[]),
                            typeof(NpgsqlLSeg[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Box | NpgsqlDbType.Array),
                        ("(NpgsqlBox[])", "JsonConvert.DeserializeObject<NpgsqlBox[]>({0})",
                            "JsonConvert.SerializeObject({0})", "NpgsqlBox[]", typeof(NpgsqlBox[]), typeof(NpgsqlBox[]),
                            "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Path | NpgsqlDbType.Array),
                        ("(NpgsqlPath[])", "JsonConvert.DeserializeObject<NpgsqlPath[]>({0})",
                            "JsonConvert.SerializeObject({0})", "NpgsqlPath[]", typeof(NpgsqlPath[]),
                            typeof(NpgsqlPath[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Polygon | NpgsqlDbType.Array),
                        ("(NpgsqlPolygon[])", "JsonConvert.DeserializeObject<NpgsqlPolygon[]>({0})",
                            "JsonConvert.SerializeObject({0})", "NpgsqlPolygon[]", typeof(NpgsqlPolygon[]),
                            typeof(NpgsqlPolygon[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Circle | NpgsqlDbType.Array),
                        ("(NpgsqlCircle[])", "JsonConvert.DeserializeObject<NpgsqlCircle[]>({0})",
                            "JsonConvert.SerializeObject({0})", "NpgsqlCircle[]", typeof(NpgsqlCircle[]),
                            typeof(NpgsqlCircle[]), "{0}", "GetValue")
                    },

                    {
                        (int)(NpgsqlDbType.Cidr | NpgsqlDbType.Array),
                        ("((IPAddress, int)[])", "JsonConvert.DeserializeObject<(IPAddress, int)[]>({0})",
                            "JsonConvert.SerializeObject({0})", "(IPAddress, int)[]", typeof((IPAddress, int)[]),
                            typeof((IPAddress, int)[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Inet | NpgsqlDbType.Array),
                        ("(IPAddress[])", "JsonConvert.DeserializeObject<IPAddress[]>({0})",
                            "JsonConvert.SerializeObject({0})", "IPAddress[]", typeof(IPAddress[]), typeof(IPAddress[]),
                            "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.MacAddr | NpgsqlDbType.Array),
                        ("(PhysicalAddress[])", "JsonConvert.DeserializeObject<PhysicalAddress[]>({0})",
                            "JsonConvert.SerializeObject({0})", "PhysicalAddress[]", typeof(PhysicalAddress[]),
                            typeof(PhysicalAddress[]), "{0}", "GetValue")
                    },

                    {
                        (int)(NpgsqlDbType.Json | NpgsqlDbType.Array),
                        ("(JToken[])", "JsonConvert.DeserializeObject<JToken[]>({0})",
                            "JsonConvert.SerializeObject({0})", "JToken[]", typeof(JToken[]), typeof(JToken[]), "{0}",
                            "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Jsonb | NpgsqlDbType.Array),
                        ("(JToken[])", "JsonConvert.DeserializeObject<JToken[]>({0})",
                            "JsonConvert.SerializeObject({0})", "JToken[]", typeof(JToken[]), typeof(JToken[]), "{0}",
                            "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Uuid | NpgsqlDbType.Array),
                        ("(Guid[])", "JsonConvert.DeserializeObject<Guid[]>({0})", "JsonConvert.SerializeObject({0})",
                            "Guid[]", typeof(Guid[]), typeof(Guid[]), "{0}", "GetValue")
                    },

                    {
                        (int)(NpgsqlDbType.Range | NpgsqlDbType.Integer | NpgsqlDbType.Array),
                        ("(NpgsqlRange<int>[])", "JsonConvert.DeserializeObject<NpgsqlRange<int>[]>({0})",
                            "JsonConvert.SerializeObject({0})", "NpgsqlRange<int>[]", typeof(NpgsqlRange<int>[]),
                            typeof(NpgsqlRange<int>[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Range | NpgsqlDbType.Bigint | NpgsqlDbType.Array),
                        ("(NpgsqlRange<long>[])", "JsonConvert.DeserializeObject<NpgsqlRange<long>[]>({0})",
                            "JsonConvert.SerializeObject({0})", "NpgsqlRange<long>[]", typeof(NpgsqlRange<long>[]),
                            typeof(NpgsqlRange<long>[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Range | NpgsqlDbType.Numeric | NpgsqlDbType.Array),
                        ("(NpgsqlRange<decimal>[])", "JsonConvert.DeserializeObject<NpgsqlRange<decimal>[]>({0})",
                            "JsonConvert.SerializeObject({0})", "NpgsqlRange<decimal>[]",
                            typeof(NpgsqlRange<decimal>[]), typeof(NpgsqlRange<decimal>[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Range | NpgsqlDbType.Timestamp | NpgsqlDbType.Array),
                        ("(NpgsqlRange<DateTime>[])", "JsonConvert.DeserializeObject<NpgsqlRange<DateTime>[]>({0})",
                            "JsonConvert.SerializeObject({0})", "NpgsqlRange<DateTime>[]",
                            typeof(NpgsqlRange<DateTime>[]), typeof(NpgsqlRange<DateTime>[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Range | NpgsqlDbType.TimestampTz | NpgsqlDbType.Array),
                        ("(NpgsqlRange<DateTime>[])", "JsonConvert.DeserializeObject<NpgsqlRange<DateTime>[]>({0})",
                            "JsonConvert.SerializeObject({0})", "NpgsqlRange<DateTime>[]",
                            typeof(NpgsqlRange<DateTime>[]), typeof(NpgsqlRange<DateTime>[]), "{0}", "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Range | NpgsqlDbType.Date | NpgsqlDbType.Array),
                        ("(NpgsqlRange<DateTime>[])", "JsonConvert.DeserializeObject<NpgsqlRange<DateTime>[]>({0})",
                            "JsonConvert.SerializeObject({0})", "NpgsqlRange<DateTime>[]",
                            typeof(NpgsqlRange<DateTime>[]), typeof(NpgsqlRange<DateTime>[]), "{0}", "GetValue")
                    },

                    {
                        (int)(NpgsqlDbType.Hstore | NpgsqlDbType.Array),
                        ("(Dictionary<string, string>[])",
                            "JsonConvert.DeserializeObject<Dictionary<string, string>[]>({0})",
                            "JsonConvert.SerializeObject({0})", "Dictionary<string, string>[]",
                            typeof(Dictionary<string, string>[]), typeof(Dictionary<string, string>[]), "{0}",
                            "GetValue")
                    },
                    {
                        (int)(NpgsqlDbType.Geometry | NpgsqlDbType.Array),
                        ("(PostgisGeometry[])", "JsonConvert.DeserializeObject<PostgisGeometry[]>({0})",
                            "JsonConvert.SerializeObject({0})", "PostgisGeometry[]", typeof(PostgisGeometry[]),
                            typeof(PostgisGeometry[]), "{0}", "GetValue")
                    },
                };

        public string GetCsConvert(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc)
            ? (column.IsNullable ? trydc.csConvert : trydc.csConvert.Replace("?", ""))
            : null;

        public string GetCsParse(DbColumnInfo column) =>
            _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csParse : null;

        public string GetCsStringify(DbColumnInfo column) =>
            _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csStringify : null;

        public string GetCsType(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc)
            ? (column.IsNullable ? trydc.csType : trydc.csType.Replace("?", ""))
            : null;

        public Type GetCsTypeInfo(DbColumnInfo column) =>
            _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csTypeInfo : null;

        public string GetCsTypeValue(DbColumnInfo column) =>
            _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csTypeValue : null;

        public string GetDataReaderMethod(DbColumnInfo column) =>
            _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.dataReaderMethod : null;

        public List<string> GetDatabases()
        {
            throw new NotImplementedException();
        }

        public bool ExistsTable(string name, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var tbnameArray = _commonUtils.SplitTableName(name);
            var tbname = string.Empty;
            if (tbnameArray?.Length == 1)
                tbname = tbnameArray.FirstOrDefault();
            var resList = _orm.Ado.Query<string>(CommandType.Text, @"SHOW TABLES");
            var res = false;
            if (ignoreCase)
                res = resList.Any(s => s.ToLower().Equals(tbname.ToLower()));
            else
                res = resList.Any(s => s.Equals(tbname));
            return res;
        }

        public DbTableInfo GetTableByName(string name, bool ignoreCase = true) =>
            GetTables(null, name, ignoreCase)?.FirstOrDefault();

        public List<DbTableInfo> GetTablesByDatabase(params string[] database) => GetTables(database, null, false);

        public List<DbTableInfo> GetTables(string[] database, string tablename, bool ignoreCase)
        {
            var resList = _orm.Ado.Query<string>(CommandType.Text, @"SHOW TABLES");

            var tables = new List<DbTableInfo>();

            resList.ForEach(s =>
            {
                var tableColumns = _orm.Ado.ExecuteDataTable($"SHOW COLUMNS FROM '{s}'");
                List<DbColumnInfo> dbColumnInfos = new List<DbColumnInfo>();
                var dbTableInfo = new DbTableInfo()
                {
                    Name = s,
                    Columns = new List<DbColumnInfo>()
                };
                foreach (DataRow tableColumnsRow in tableColumns.Rows)
                {
                    dbColumnInfos.Add(new DbColumnInfo()
                    {
                        Name = tableColumnsRow["column"].ToString(),
                        DbTypeText = tableColumnsRow["type"].ToString(),
                        Table = dbTableInfo,
                    });
                }

                dbTableInfo.Columns = dbColumnInfos;
                tables.Add(dbTableInfo);
            });
            return tables;
        }

        public List<DbEnumInfo> GetEnumsByDatabase(params string[] database)
        {
            throw new NotImplementedException();
        }

        public static NativeTuple<bool, int, int> ParsePgVersion(string versionString, int v1, int v2)
        {
            int[] version = new int[] { 0, 0 };
            var vmatch = Regex.Match(versionString, @"(\d+)\.(\d+)");
            if (vmatch.Success)
            {
                version[0] = int.Parse(vmatch.Groups[1].Value);
                version[1] = int.Parse(vmatch.Groups[2].Value);
            }
            else
            {
                vmatch = Regex.Match(versionString, @"(\d+)");
                version[0] = int.Parse(vmatch.Groups[1].Value);
            }

            if (version[0] > v1)
                return NativeTuple.Create(true, version[0], version[1]);
            if (version[0] == v1 && version[1] >= v2)
                return NativeTuple.Create(true, version[0], version[1]);
            return NativeTuple.Create(false, version[0], version[1]);
        }
    }
}