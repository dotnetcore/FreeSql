using FreeSql.DataAnnotations;
using Newtonsoft.Json.Linq;
using Npgsql;
using Npgsql.LegacyPostgis;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using Xunit;

namespace FreeSql.Tests.PostgreSQLExpression
{
    public class OtherTest
    {

        ISelect<TableAllType> select => g.pgsql.Select<TableAllType>();

        public OtherTest()
        {
            NpgsqlConnection.GlobalTypeMapper.UseLegacyPostgis();
        }

        [Fact]
        public void Div()
        {
            var t1 = select.Where(a => a.testFieldInt / 3 > 3).Limit(10).ToList();
            var t2 = select.Where(a => a.testFieldLong / 3 > 3).Limit(10).ToList();
            var t3 = select.Where(a => a.testFieldShort / 3 > 3).Limit(10).ToList();

            var t4 = select.Where(a => a.testFieldInt / 3.0 > 3).Limit(10).ToList();
            var t5 = select.Where(a => a.testFieldLong / 3.0 > 3).Limit(10).ToList();
            var t6 = select.Where(a => a.testFieldShort / 3.0 > 3).Limit(10).ToList();

            var t7 = select.Where(a => a.testFieldDouble / 3 > 3).Limit(10).ToList();
            var t8 = select.Where(a => a.testFieldDecimal / 3 > 3).Limit(10).ToList();
            var t9 = select.Where(a => a.testFieldFloat / 3 > 3).Limit(10).ToList();
        }

        [Fact]
        public void Boolean()
        {
            var t1 = select.Where(a => a.testFieldBool == true).Limit(10).ToList();
            var t2 = select.Where(a => a.testFieldBool != true).Limit(10).ToList();
            var t3 = select.Where(a => a.testFieldBool == false).Limit(10).ToList();
            var t4 = select.Where(a => !a.testFieldBool).Limit(10).ToList();
            var t5 = select.Where(a => a.testFieldBool).Limit(10).ToList();
            var t51 = select.WhereCascade(a => a.testFieldBool).Limit(10).ToList();

            var t11 = select.Where(a => a.testFieldBoolNullable == true).Limit(10).ToList();
            var t22 = select.Where(a => a.testFieldBoolNullable != true).Limit(10).ToList();
            var t33 = select.Where(a => a.testFieldBoolNullable == false).Limit(10).ToList();
            var t44 = select.Where(a => !a.testFieldBoolNullable.Value).Limit(10).ToList();
            var t55 = select.Where(a => a.testFieldBoolNullable.Value).Limit(10).ToList();

            var t111 = select.Where(a => a.testFieldBool == true && a.Id > 0).Limit(10).ToList();
            var t222 = select.Where(a => a.testFieldBool != true && a.Id > 0).Limit(10).ToList();
            var t333 = select.Where(a => a.testFieldBool == false && a.Id > 0).Limit(10).ToList();
            var t444 = select.Where(a => !a.testFieldBool && a.Id > 0).Limit(10).ToList();
            var t555 = select.Where(a => a.testFieldBool && a.Id > 0).Limit(10).ToList();

            var t1111 = select.Where(a => a.testFieldBoolNullable == true && a.Id > 0).Limit(10).ToList();
            var t2222 = select.Where(a => a.testFieldBoolNullable != true && a.Id > 0).Limit(10).ToList();
            var t3333 = select.Where(a => a.testFieldBoolNullable == false && a.Id > 0).Limit(10).ToList();
            var t4444 = select.Where(a => !a.testFieldBoolNullable.Value && a.Id > 0).Limit(10).ToList();
            var t5555 = select.Where(a => a.testFieldBoolNullable.Value && a.Id > 0).Limit(10).ToList();

            var t11111 = select.Where(a => a.testFieldBool == true && a.Id > 0 && a.testFieldBool == true).Limit(10).ToList();
            var t22222 = select.Where(a => a.testFieldBool != true && a.Id > 0 && a.testFieldBool != true).Limit(10).ToList();
            var t33333 = select.Where(a => a.testFieldBool == false && a.Id > 0 && a.testFieldBool == false).Limit(10).ToList();
            var t44444 = select.Where(a => !a.testFieldBool && a.Id > 0 && !a.testFieldBool).Limit(10).ToList();
            var t55555 = select.Where(a => a.testFieldBool && a.Id > 0 && a.testFieldBool).Limit(10).ToList();

            var t111111 = select.Where(a => a.testFieldBoolNullable == true && a.Id > 0 && a.testFieldBoolNullable == true).Limit(10).ToList();
            var t222222 = select.Where(a => a.testFieldBoolNullable != true && a.Id > 0 && a.testFieldBoolNullable != true).Limit(10).ToList();
            var t333333 = select.Where(a => a.testFieldBoolNullable == false && a.Id > 0 && a.testFieldBoolNullable == false).Limit(10).ToList();
            var t444444 = select.Where(a => !a.testFieldBoolNullable.Value && a.Id > 0 && !a.testFieldBoolNullable.Value).Limit(10).ToList();
            var t555555 = select.Where(a => a.testFieldBoolNullable.Value && a.Id > 0 && a.testFieldBoolNullable.Value).Limit(10).ToList();
        }

        [Fact]
        public void Array()
        {
            //g.pgsql.Aop.CurdAfter = (s, e) => {
            //    Trace.WriteLine(e.CurdType + ": " + e.ElapsedMilliseconds + "ms " + e.Sql.Replace("\n", ""));
            //};
            IEnumerable<int> testlinqlist = new List<int>(new[] { 1, 2, 3 });
            var testlinq = select.Where(a => testlinqlist.Contains(a.testFieldInt)).ToList();

            var sql1 = select.Where(a => a.testFieldIntArray.Contains(1)).ToList();
            var sql2 = select.Where(a => a.testFieldIntArray.Contains(1) == false).ToList();
            var sql121 = select.Where(a => a.testFieldStringArray.Contains("aaa") == false).ToList();

            //in not in
            var sql111 = select.Where(a => new[] { 1, 2, 3 }.Contains(a.testFieldInt)).ToList();
            var sql112 = select.Where(a => new[] { 1, 2, 3 }.Contains(a.testFieldInt) == false).ToList();
            var sql113 = select.Where(a => !new[] { 1, 2, 3 }.Contains(a.testFieldInt)).ToList();

            var inarray = new[] { 1, 2, 3 };
            var sql1111 = select.Where(a => inarray.Contains(a.testFieldInt)).ToSql();
            var sql1122 = select.Where(a => inarray.Contains(a.testFieldInt) == false).ToSql();
            var sql1133 = select.Where(a => !inarray.Contains(a.testFieldInt)).ToSql();

            //in not in
            var sql11111 = select.Where(a => new List<int>() { 1, 2, 3 }.Contains(a.testFieldInt)).ToList();
            var sql11222 = select.Where(a => new List<int>() { 1, 2, 3 }.Contains(a.testFieldInt) == false).ToList();
            var sql11333 = select.Where(a => !new List<int>() { 1, 2, 3 }.Contains(a.testFieldInt)).ToList();

            var sql11111a = select.Where(a => new List<int>(new[] { 1, 2, 3 }).Contains(a.testFieldInt)).ToList();
            var sql11222b = select.Where(a => new List<int>(new[] { 1, 2, 3 }).Contains(a.testFieldInt) == false).ToList();
            var sql11333c = select.Where(a => !new List<int>(new[] { 1, 2, 3 }).Contains(a.testFieldInt)).ToList();

            var inarray2 = new List<int>() { 1, 2, 3 };
            var sql111111 = select.Where(a => inarray.Contains(a.testFieldInt)).ToList();
            var sql112222 = select.Where(a => inarray.Contains(a.testFieldInt) == false).ToList();
            var sql113333 = select.Where(a => !inarray.Contains(a.testFieldInt)).ToList();

            var sql1111112 = select.ToList(a => inarray);
            var sql1111113 = select.ToList(a => a.testFieldIntArray);


            var sql3 = select.Where(a => a.testFieldIntArray.Any()).ToList();
            var sql4 = select.Where(a => a.testFieldIntArray.Any() == false).ToList();

            //var sql5 = select.ToList(a => a.testFieldIntArray.Concat(new[] { 1, 2, 3 }));
            //v5.0.1.1 Unable to cast object of type 'System.Nullable`1[System.Int32][]' to type 'System.Collections.Generic.IEnumerable`1[System.Int32]'.

            var sql6 = select.Where(a => a.testFieldIntArray.GetLength(1) > 0).ToList();
            var sql7 = select.Where(a => a.testFieldIntArray.GetLongLength(1) > 0).ToList();
            var sql8 = select.Where(a => a.testFieldIntArray.Length > 0).ToList();
            var sql9 = select.Where(a => a.testFieldIntArray.Count() > 0).ToList();

            var inarray2n = Enumerable.Range(1, 3333).ToArray();
            var sql1111111 = select.Where(a => inarray2n.Contains(a.testFieldInt)).ToList();
            var sql1122222 = select.Where(a => inarray2n.Contains(a.testFieldInt) == false).ToList();
            var sql1133333 = select.Where(a => !inarray2n.Contains(a.testFieldInt)).ToList();
        }

        [Fact]
        public void Jsonb()
        {

            var sql1 = select.Where(a => a.testFieldJToken.Contains(JToken.Parse("{a:1}"))).Limit(10).ToList();
            var sql2 = select.Where(a => a.testFieldJToken.Contains(JToken.Parse("{a:1}")) == false).Limit(10).ToList();
            var sql111 = select.Where(a => a.testFieldJToken.Contains("{\"a\":1}")).Limit(10).ToList();
            var sql222 = select.Where(a => a.testFieldJToken.Contains("{\"a\":1}") == false).Limit(10).ToList();

            var sql3 = select.Where(a => a.testFieldJObject.ContainsKey("a")).Limit(10).ToList();
            var sql4 = select.Where(a => a.testFieldJObject.ContainsKey("a") == false).Limit(10).ToList();

            var sql5 = select.Where(a => a.testFieldJArray.Contains(1)).Limit(10).ToList();
            var sql6 = select.Where(a => a.testFieldJArray.Contains(1) == false).Limit(10).ToList();
            var sql555 = select.Where(a => a.testFieldJArray.Contains(1)).Limit(10).ToList();
            var sql666 = select.Where(a => a.testFieldJArray.Contains(1) == false).Limit(10).ToList();

            //var sql7 = select.Where(a => a.testFieldJToken.Any()).Limit(10).ToList();
            //var sql8 = select.Where(a => a.testFieldJToken.Any() == false).Limit(10).ToList();

            var sql9 = select.Where(a => a.testFieldJArray.Any()).Limit(10).ToList();
            var sql10 = select.Where(a => a.testFieldJArray.Any() == false).Limit(10).ToList();

            //var sql11 = select.Limit(10).ToList(a => a.testFieldJToken.Concat(JToken.Parse("{a:1}")));
            //var sql12 = select.Limit(10).ToList(a => a.testFieldJObject.Concat(JToken.Parse("{a:1}")));
            //var sql13 = select.Limit(10).ToList(a => a.testFieldJArray.Concat(JToken.Parse("{a:1}")));

            //var sql14 = select.Where(a => a.testFieldJToken.Count() > 0).Limit(10).ToList();
            //var sql15 = select.Where(a => a.testFieldJObject.Count > 0).Limit(10).ToList();
            var sql16 = select.Where(a => a.testFieldJArray.Count() > 0).Limit(10).ToList();
            var sql17 = select.Where(a => a.testFieldJArray.LongCount() > 0).Limit(10).ToList();
            var sql18 = select.Where(a => a.testFieldJArray.Count > 0).Limit(10).ToList();
        }

        [Fact]
        public void HStore()
        {
            var sql1 = select.Where(a => a.testFieldHStore.ContainsKey("a")).ToList();
            var sql2 = select.Where(a => a.testFieldHStore.ContainsKey("a") == false).ToList();

            var sql3 = select.Where(a => a.testFieldHStore["a"] == "xxx").ToList();
        }

        [Table(Name = "tb_alltype")]
        class TableAllType
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }

            public bool testFieldBool { get; set; }
            public sbyte testFieldSByte { get; set; }
            public short testFieldShort { get; set; }
            public int testFieldInt { get; set; }
            public long testFieldLong { get; set; }
            public byte testFieldByte { get; set; }
            public ushort testFieldUShort { get; set; }
            public uint testFieldUInt { get; set; }
            public ulong testFieldULong { get; set; }
            public double testFieldDouble { get; set; }
            public float testFieldFloat { get; set; }
            public decimal testFieldDecimal { get; set; }
            public TimeSpan testFieldTimeSpan { get; set; }
            public DateTime testFieldDateTime { get; set; }
            public byte[] testFieldBytes { get; set; }
            public string testFieldString { get; set; }
            public Guid testFieldGuid { get; set; }
            public NpgsqlPoint testFieldNpgsqlPoint { get; set; }
            public NpgsqlLine testFieldNpgsqlLine { get; set; }
            public NpgsqlLSeg testFieldNpgsqlLSeg { get; set; }
            public NpgsqlBox testFieldNpgsqlBox { get; set; }
            public NpgsqlPath testFieldNpgsqlPath { get; set; }
            public NpgsqlPolygon testFieldNpgsqlPolygon { get; set; }
            public NpgsqlCircle testFieldNpgsqlCircle { get; set; }
            public (IPAddress Address, int Subnet) testFieldCidr { get; set; }
            public NpgsqlRange<int> testFieldInt4range { get; set; }
            public NpgsqlRange<long> testFieldInt8range { get; set; }
            public NpgsqlRange<decimal> testFieldNumrange { get; set; }
            public NpgsqlRange<DateTime> testFieldTsrange { get; set; }

            public bool? testFieldBoolNullable { get; set; }
            public sbyte? testFieldSByteNullable { get; set; }
            public short? testFieldShortNullable { get; set; }
            public int? testFieldIntNullable { get; set; }
            public long? testFielLongNullable { get; set; }
            public byte? testFieldByteNullable { get; set; }
            public ushort? testFieldUShortNullable { get; set; }
            public uint? testFieldUIntNullable { get; set; }
            public ulong? testFieldULongNullable { get; set; }
            public double? testFieldDoubleNullable { get; set; }
            public float? testFieldFloatNullable { get; set; }
            public decimal? testFieldDecimalNullable { get; set; }
            public TimeSpan? testFieldTimeSpanNullable { get; set; }
            public DateTime? testFieldDateTimeNullable { get; set; }
            public Guid? testFieldGuidNullable { get; set; }
            public NpgsqlPoint? testFieldNpgsqlPointNullable { get; set; }
            public NpgsqlLine? testFieldNpgsqlLineNullable { get; set; }
            public NpgsqlLSeg? testFieldNpgsqlLSegNullable { get; set; }
            public NpgsqlBox? testFieldNpgsqlBoxNullable { get; set; }
            public NpgsqlPath? testFieldNpgsqlPathNullable { get; set; }
            public NpgsqlPolygon? testFieldNpgsqlPolygonNullable { get; set; }
            public NpgsqlCircle? testFieldNpgsqlCircleNullable { get; set; }
            public (IPAddress Address, int Subnet)? testFieldCidrNullable { get; set; }
            public NpgsqlRange<int>? testFieldInt4rangeNullable { get; set; }
            public NpgsqlRange<long>? testFieldInt8rangeNullable { get; set; }
            public NpgsqlRange<decimal>? testFieldNumrangeNullable { get; set; }
            public NpgsqlRange<DateTime>? testFieldTsrangeNullable { get; set; }

            public BitArray testFieldBitArray { get; set; }
            public IPAddress testFieldInet { get; set; }
            public PhysicalAddress testFieldMacaddr { get; set; }
            public JToken testFieldJToken { get; set; }
            public JObject testFieldJObject { get; set; }
            public JArray testFieldJArray { get; set; }
            public Dictionary<string, string> testFieldHStore { get; set; }
            public PostgisPoint testFieldPostgisPoint { get; set; }
            public PostgisLineString testFieldPostgisLineString { get; set; }
            public PostgisPolygon testFieldPostgisPolygon { get; set; }
            public PostgisMultiPoint testFieldPostgisMultiPoint { get; set; }
            public PostgisMultiLineString testFieldPostgisPostgisMultiLineString { get; set; }
            public PostgisMultiPolygon testFieldPostgisPostgisMultiPolygon { get; set; }
            public PostgisGeometry testFieldPostgisGeometry { get; set; }
            public PostgisGeometryCollection testFieldPostgisGeometryCollection { get; set; }

            public TableAllTypeEnumType1 testFieldEnum1 { get; set; }
            public TableAllTypeEnumType1? testFieldEnum1Nullable { get; set; }
            public TableAllTypeEnumType2 testFieldEnum2 { get; set; }
            public TableAllTypeEnumType2? testFieldEnum2Nullable { get; set; }

            /* array */
            public bool[] testFieldBoolArray { get; set; }
            public sbyte[] testFieldSByteArray { get; set; }
            public short[] testFieldShortArray { get; set; }
            public int[] testFieldIntArray { get; set; }
            public long[] testFieldLongArray { get; set; }
            public byte[] testFieldByteArray { get; set; }
            public ushort[] testFieldUShortArray { get; set; }
            public uint[] testFieldUIntArray { get; set; }
            public ulong[] testFieldULongArray { get; set; }
            public double[] testFieldDoubleArray { get; set; }
            public float[] testFieldFloatArray { get; set; }
            public decimal[] testFieldDecimalArray { get; set; }
            public TimeSpan[] testFieldTimeSpanArray { get; set; }
            public DateTime[] testFieldDateTimeArray { get; set; }
            public byte[][] testFieldBytesArray { get; set; }
            public string[] testFieldStringArray { get; set; }
            public Guid[] testFieldGuidArray { get; set; }
            public NpgsqlPoint[] testFieldNpgsqlPointArray { get; set; }
            public NpgsqlLine[] testFieldNpgsqlLineArray { get; set; }
            public NpgsqlLSeg[] testFieldNpgsqlLSegArray { get; set; }
            public NpgsqlBox[] testFieldNpgsqlBoxArray { get; set; }
            public NpgsqlPath[] testFieldNpgsqlPathArray { get; set; }
            public NpgsqlPolygon[] testFieldNpgsqlPolygonArray { get; set; }
            public NpgsqlCircle[] testFieldNpgsqlCircleArray { get; set; }
            public (IPAddress Address, int Subnet)[] testFieldCidrArray { get; set; }
            public NpgsqlRange<int>[] testFieldInt4rangeArray { get; set; }
            public NpgsqlRange<long>[] testFieldInt8rangeArray { get; set; }
            public NpgsqlRange<decimal>[] testFieldNumrangeArray { get; set; }
            public NpgsqlRange<DateTime>[] testFieldTsrangeArray { get; set; }

            public bool?[] testFieldBoolArrayNullable { get; set; }
            public sbyte?[] testFieldSByteArrayNullable { get; set; }
            public short?[] testFieldShortArrayNullable { get; set; }
            public int?[] testFieldIntArrayNullable { get; set; }
            public long?[] testFielLongArrayNullable { get; set; }
            public byte?[] testFieldByteArrayNullable { get; set; }
            public ushort?[] testFieldUShortArrayNullable { get; set; }
            public uint?[] testFieldUIntArrayNullable { get; set; }
            public ulong?[] testFieldULongArrayNullable { get; set; }
            public double?[] testFieldDoubleArrayNullable { get; set; }
            public float?[] testFieldFloatArrayNullable { get; set; }
            public decimal?[] testFieldDecimalArrayNullable { get; set; }
            public TimeSpan?[] testFieldTimeSpanArrayNullable { get; set; }
            public DateTime?[] testFieldDateTimeArrayNullable { get; set; }
            public Guid?[] testFieldGuidArrayNullable { get; set; }
            public NpgsqlPoint?[] testFieldNpgsqlPointArrayNullable { get; set; }
            public NpgsqlLine?[] testFieldNpgsqlLineArrayNullable { get; set; }
            public NpgsqlLSeg?[] testFieldNpgsqlLSegArrayNullable { get; set; }
            public NpgsqlBox?[] testFieldNpgsqlBoxArrayNullable { get; set; }
            public NpgsqlPath?[] testFieldNpgsqlPathArrayNullable { get; set; }
            public NpgsqlPolygon?[] testFieldNpgsqlPolygonArrayNullable { get; set; }
            public NpgsqlCircle?[] testFieldNpgsqlCircleArrayNullable { get; set; }
            public (IPAddress Address, int Subnet)?[] testFieldCidrArrayNullable { get; set; }
            public NpgsqlRange<int>?[] testFieldInt4rangeArrayNullable { get; set; }
            public NpgsqlRange<long>?[] testFieldInt8rangeArrayNullable { get; set; }
            public NpgsqlRange<decimal>?[] testFieldNumrangeArrayNullable { get; set; }
            public NpgsqlRange<DateTime>?[] testFieldTsrangeArrayNullable { get; set; }

            public BitArray[] testFieldBitArrayArray { get; set; }
            public IPAddress[] testFieldInetArray { get; set; }
            public PhysicalAddress[] testFieldMacaddrArray { get; set; }
            public JToken[] testFieldJTokenArray { get; set; }
            public JObject[] testFieldJObjectArray { get; set; }
            public JArray[] testFieldJArrayArray { get; set; }
            public Dictionary<string, string>[] testFieldHStoreArray { get; set; }
            public PostgisPoint[] testFieldPostgisPointArray { get; set; }
            public PostgisLineString[] testFieldPostgisLineStringArray { get; set; }
            public PostgisPolygon[] testFieldPostgisPolygonArray { get; set; }
            public PostgisMultiPoint[] testFieldPostgisMultiPointArray { get; set; }
            public PostgisMultiLineString[] testFieldPostgisPostgisMultiLineStringArray { get; set; }
            public PostgisMultiPolygon[] testFieldPostgisPostgisMultiPolygonArray { get; set; }
            public PostgisGeometry[] testFieldPostgisGeometryArray { get; set; }
            public PostgisGeometryCollection[] testFieldPostgisGeometryCollectionArray { get; set; }

            public TableAllTypeEnumType1[] testFieldEnum1Array { get; set; }
            public TableAllTypeEnumType1?[] testFieldEnum1ArrayNullable { get; set; }
            public TableAllTypeEnumType2[] testFieldEnum2Array { get; set; }
            public TableAllTypeEnumType2?[] testFieldEnum2ArrayNullable { get; set; }
        }

        public enum TableAllTypeEnumType1 { e1, e2, e3, e5 }
        [Flags] public enum TableAllTypeEnumType2 { f1, f2, f3 }
    }
}
