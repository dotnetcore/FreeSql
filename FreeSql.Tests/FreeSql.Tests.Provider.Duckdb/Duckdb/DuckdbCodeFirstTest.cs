using FreeSql.DataAnnotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace FreeSql.Tests.Duckdb
{
    public class DuckdbCodeFirstTest
    {
        IFreeSql fsql => g.duckdb;

        [Fact]
        public void UInt256Crud2()
        {
            var num = BigInteger.Parse("170141183460469231731687303715884105727");
            fsql.Delete<tuint256tb_01>().Where("1=1").ExecuteAffrows();
            Assert.Equal(1, fsql.Insert(new tuint256tb_01()).ExecuteAffrows());
            var find = fsql.Select<tuint256tb_01>().ToList();
            Assert.Single(find);
            Assert.Equal("0", find[0].Number.ToString());
            var item = new tuint256tb_01 { Number = num };
            Assert.Equal(1, fsql.Insert(item).ExecuteAffrows());
            find = fsql.Select<tuint256tb_01>().Where(a => a.Id == item.Id).ToList();
            Assert.Single(find);
            Assert.Equal(item.Number, find[0].Number);
            num = num - 1;
            item.Number = num;
            Assert.Equal(1, fsql.Update<tuint256tb_01>().SetSource(item).ExecuteAffrows());
            find = fsql.Select<tuint256tb_01>().Where(a => a.Id == item.Id).ToList();
            Assert.Single(find);
            Assert.Equal("170141183460469231731687303715884105726", find[0].Number.ToString());

            num = BigInteger.Parse("170141183460469231731687303715884105727");
            fsql.Delete<tuint256tb_01>().Where("1=1").ExecuteAffrows();
            Assert.Equal(1, fsql.Insert(new tuint256tb_01()).NoneParameter().ExecuteAffrows());
            find = fsql.Select<tuint256tb_01>().ToList();
            Assert.Single(find);
            Assert.Equal("0", find[0].Number.ToString());
            item = new tuint256tb_01 { Number = num };
            Assert.Equal(1, fsql.Insert(item).NoneParameter().ExecuteAffrows());
            find = fsql.Select<tuint256tb_01>().Where(a => a.Id == item.Id).ToList();
            Assert.Single(find);
            Assert.Equal(item.Number, find[0].Number);
            num = num - 1;
            item.Number = num;
            Assert.Equal(1, fsql.Update<tuint256tb_01>().NoneParameter().SetSource(item).ExecuteAffrows());
            find = fsql.Select<tuint256tb_01>().Where(a => a.Id == item.Id).ToList();
            Assert.Single(find);
            Assert.Equal("170141183460469231731687303715884105726", find[0].Number.ToString());
        }
        class tuint256tb_01
        {
            public Guid Id { get; set; }
            public BigInteger Number { get; set; }
        }

        IInsert<TableAllType> insert => fsql.Insert<TableAllType>();
        ISelect<TableAllType> select => fsql.Select<TableAllType>();

        [Fact]
        public void CurdAllField()
        {
            var sql1 = select.Where(a => a.testFieldIntArray.Contains(1)).ToSql();
            var lst1 = select.Where(a => a.testFieldIntArray.Contains(1)).ToList();

            var item = new TableAllType { };
            item.Id = (int)insert.AppendData(item).ExecuteIdentity();

            var newitem = select.Where(a => a.Id == item.Id).ToOne();

            var item2 = new TableAllType
            {
                testFieldBitArray = new BitArray(Encoding.UTF8.GetBytes("我是")),
                testFieldBitArrayArray = new[] { new BitArray(Encoding.UTF8.GetBytes("中国")), new BitArray(Encoding.UTF8.GetBytes("公民")) },
                testFieldBool = true,
                testFieldBoolArray = new[] { true, true, false, false },
                testFieldBoolArrayNullable = new bool?[] { true, true, null, false, false },
                testFieldBoolNullable = true,
                testFieldByte = byte.MaxValue,
                testFieldByteArray = new byte[] { 0, 1, 2, 3, 4, 5, 6 },
                testFieldByteArrayNullable = new byte?[] { 0, 1, 2, 3, null, 4, 5, 6 },
                testFieldByteNullable = byte.MinValue,
                testFieldBytes = Encoding.UTF8.GetBytes("我是中国人"),
                testFieldBytesArray = new[] { Encoding.UTF8.GetBytes("我是中国人"), Encoding.UTF8.GetBytes("我是中国人") },
                testFieldDateTime = DateTime.Now,
                testFieldDateTimeArray = new[] { DateTime.Now, DateTime.Now.AddHours(2) },
                testFieldDateTimeArrayNullable = new DateTime?[] { DateTime.Now, null, DateTime.Now.AddHours(2) },
                testFieldDateTimeNullable = DateTime.Now.AddDays(-1),
                testFieldDateOnly = DateOnly.FromDateTime(DateTime.Now),
                testFieldDateOnlyArray = new[] { DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now.AddHours(2)) },
                testFieldDateOnlyArrayNullable = new DateOnly?[] { DateOnly.FromDateTime(DateTime.Now), null, DateOnly.FromDateTime(DateTime.Now.AddHours(2)) },
                testFieldDateOnlyNullable = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                testFieldDecimal = 999.99M,
                testFieldDecimalArray = new[] { 999.91M, 999.92M, 999.93M },
                testFieldDecimalArrayNullable = new decimal?[] { 998.11M, 998.12M, 998.13M },
                testFieldDecimalNullable = 111.11M,
                testFieldDouble = 888.88,
                testFieldDoubleArray = new[] { 888.81, 888.82, 888.83 },
                testFieldDoubleArrayNullable = new double?[] { 888.11, 888.12, null, 888.13 },
                testFieldDoubleNullable = 222.22,
                testFieldEnum1 = TableAllTypeEnumType1.e3,
                testFieldEnum1Array = new[] { TableAllTypeEnumType1.e5, TableAllTypeEnumType1.e2, TableAllTypeEnumType1.e1 },
                testFieldEnum1ArrayNullable = new TableAllTypeEnumType1?[] { TableAllTypeEnumType1.e5, TableAllTypeEnumType1.e2, null, TableAllTypeEnumType1.e1 },
                testFieldEnum1Nullable = TableAllTypeEnumType1.e2,
                testFieldEnum2 = TableAllTypeEnumType2.f2,
                testFieldEnum2Array = new[] { TableAllTypeEnumType2.f3, TableAllTypeEnumType2.f1 },
                testFieldEnum2ArrayNullable = new TableAllTypeEnumType2?[] { TableAllTypeEnumType2.f3, null, TableAllTypeEnumType2.f1 },
                testFieldEnum2Nullable = TableAllTypeEnumType2.f3,
                testFieldFloat = 777.77F,
                testFieldFloatArray = new[] { 777.71F, 777.72F, 777.73F },
                testFieldFloatArrayNullable = new float?[] { 777.71F, 777.72F, null, 777.73F },
                testFieldFloatNullable = 333.33F,
                testFieldGuid = Guid.NewGuid(),
                testFieldGuidArray = new[] { Guid.NewGuid(), Guid.NewGuid() },
                testFieldGuidArrayNullable = new Guid?[] { Guid.NewGuid(), null, Guid.NewGuid() },
                testFieldGuidNullable = Guid.NewGuid(),
                testFieldStruct = new Dictionary<string, object> { { "111", "value111" }, { "222", 222 }, { "333", "value333" } },
                testFieldStructArray = new[] { new Dictionary<string, object> { { "111", "value111" }, { "222", 222 }, { "333", "value333" } }, new Dictionary<string, object> { { "444", "value444" }, { "555", 555 }, { "666", "value666" } } },
                testFieldInt = int.MaxValue,
                testFieldIntArray = new[] { 1, 2, 3, 4, 5 },
                testFieldIntArrayNullable = new int?[] { 1, 2, 3, null, 4, 5 },
                testFieldIntNullable = int.MinValue,
                testFieldLong = long.MaxValue,
                testFieldLongArray = new long[] { 10, 20, 30, 40, 50 },
                
                testFieldSByte = sbyte.MaxValue,
                testFieldSByteArray = new sbyte[] { 1, 2, 3, 4, 5 },
                testFieldSByteArrayNullable = new sbyte?[] { 1, 2, 3, null, 4, 5 },
                testFieldSByteNullable = sbyte.MinValue,
                testFieldShort = short.MaxValue,
                testFieldShortArray = new short[] { 1, 2, 3, 4, 5 },
                testFieldShortArrayNullable = new short?[] { 1, 2, 3, null, 4, 5 },
                testFieldShortNullable = short.MinValue,
                testFieldString = "我是中国人string'\\?!@#$%^&*()_+{}}{~?><<>",
                testFieldChar = 'X',
                testFieldStringArray = new[] { "我是中国人String1", "我是中国人String2", null, "我是中国人String3" },
                testFieldTimeSpan = TimeSpan.FromHours(11),
                testFieldTimeSpanArray = new[] { TimeSpan.FromHours(11), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60) },
                testFieldTimeSpanArrayNullable = new TimeSpan?[] { TimeSpan.FromHours(11), TimeSpan.FromSeconds(10), null, TimeSpan.FromSeconds(60) },
                testFieldTimeSpanNullable = TimeSpan.FromSeconds(90),
                testFieldTimeOnly = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)),
                testFieldTimeOnlyArray = new[] { TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)), TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(10)), TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(60)) },
                testFieldTimeOnlyArrayNullable = new TimeOnly?[] { TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)), TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(10)), null, TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(60)) },
                testFieldTimeOnlyNullable = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(90)),
                testFieldUInt = uint.MaxValue,
                testFieldUIntArray = new uint[] { 1, 2, 3, 4, 5 },
                testFieldUIntArrayNullable = new uint?[] { 1, 2, 3, null, 4, 5 },
                testFieldUIntNullable = uint.MinValue,
                testFieldULong = ulong.MaxValue,
                testFieldULongArray = new ulong[] { 10, 20, 30, 40, 50 },
                testFieldULongArrayNullable = new ulong?[] { 10, 20, 30, null, 40, 50 },
                testFieldULongNullable = ulong.MinValue,
                testFieldUShort = ushort.MaxValue,
                testFieldUShortArray = new ushort[] { 11, 12, 13, 14, 15 },
                testFieldUShortArrayNullable = new ushort?[] { 11, 12, 13, null, 14, 15 },
                testFieldUShortNullable = ushort.MinValue,
                testFielLongArrayNullable = new long?[] { 500, 600, 700, null, 999, 1000 },
                testFielLongNullable = long.MinValue
            };

            var sqlPar = insert.AppendData(item2).ToSql();
            var sqlText = insert.AppendData(item2).NoneParameter().ToSql();
            var item3NP = insert.AppendData(item2).NoneParameter().ExecuteInserted();

            var item3 = insert.AppendData(item2).ExecuteInserted().First();
            var newitem2 = select.Where(a => a.Id == item3.Id).ToOne();
            Assert.Equal(item2.testFieldString, newitem2.testFieldString);
            Assert.Equal(item2.testFieldChar, newitem2.testFieldChar);

            item3 = insert.NoneParameter().AppendData(item2).ExecuteInserted().First();
            newitem2 = select.Where(a => a.Id == item3.Id).ToOne();
            Assert.Equal(item2.testFieldString, newitem2.testFieldString);
            Assert.Equal(item2.testFieldChar, newitem2.testFieldChar);

            var items = select.ToList();
            var itemstb = select.ToDataTable();
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
            public TimeOnly testFieldTimeOnly{ get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime testFieldDateTime { get; set; }
            public DateOnly testFieldDateOnly { get; set; }

            public byte[] testFieldBytes { get; set; }
            public string testFieldString { get; set; }
            public char testFieldChar { get; set; }
            public Guid testFieldGuid { get; set; }

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
            public TimeOnly? testFieldTimeOnlyNullable { get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime? testFieldDateTimeNullable { get; set; }
            public DateOnly? testFieldDateOnlyNullable { get; set; }

            public Guid? testFieldGuidNullable { get; set; }

            public BitArray testFieldBitArray { get; set; }
            public Dictionary<string, object> testFieldStruct { get; set; }

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
            public TimeOnly[] testFieldTimeOnlyArray { get; set; }
            public DateTime[] testFieldDateTimeArray { get; set; }
            public DateOnly[] testFieldDateOnlyArray { get; set; }
            public byte[][] testFieldBytesArray { get; set; }
            public string[] testFieldStringArray { get; set; }
            public Guid[] testFieldGuidArray { get; set; }

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
            public TimeOnly?[] testFieldTimeOnlyArrayNullable { get; set; }
            public DateTime?[] testFieldDateTimeArrayNullable { get; set; }
            public DateOnly?[] testFieldDateOnlyArrayNullable { get; set; }
            public Guid?[] testFieldGuidArrayNullable { get; set; }

            public BitArray[] testFieldBitArrayArray { get; set; }
            public Dictionary<string, object>[] testFieldStructArray { get; set; }

            public TableAllTypeEnumType1[] testFieldEnum1Array { get; set; }
            public TableAllTypeEnumType1?[] testFieldEnum1ArrayNullable { get; set; }
            public TableAllTypeEnumType2[] testFieldEnum2Array { get; set; }
            public TableAllTypeEnumType2?[] testFieldEnum2ArrayNullable { get; set; }
        }

        public enum TableAllTypeEnumType1 { e1, e2, e3, e5 }
        [Flags] public enum TableAllTypeEnumType2 { f1, f2, f3 }
    }
}
