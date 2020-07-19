using FreeSql.DataAnnotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using Xunit;

namespace FreeSql.Tests.ShenTongExpression
{
    public class OtherTest
    {

        ISelect<TableAllType> select => g.shentong.Select<TableAllType>();

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
            //g.shentong.Aop.CurdAfter = (s, e) => {
            //    Trace.WriteLine(e.CurdType + ": " + e.ElapsedMilliseconds + "ms " + e.Sql.Replace("\n", ""));
            //};
            IEnumerable<int> testlinqlist = new List<int>(new[] { 1, 2, 3 });
            var testlinq = select.Where(a => testlinqlist.Contains(a.testFieldInt)).ToList();

            //var sql1 = select.Where(a => a.testFieldIntArray.Contains(1)).ToList();
            //var sql2 = select.Where(a => a.testFieldIntArray.Contains(1) == false).ToList();
            //var sql121 = select.Where(a => a.testFieldStringArray.Contains("aaa") == false).ToList();

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

            //var sql1111112 = select.ToList(a => inarray);
            //var sql1111113 = select.ToList(a => a.testFieldIntArray);


            //var sql3 = select.Where(a => a.testFieldIntArray.Any()).ToList();
            //var sql4 = select.Where(a => a.testFieldIntArray.Any() == false).ToList();

            //var sql5 = select.ToList(a => a.testFieldIntArray.Concat(new[] { 1, 2, 3 }));

            //var sql6 = select.Where(a => a.testFieldIntArray.GetLength(1) > 0).ToList();
            //var sql7 = select.Where(a => a.testFieldIntArray.GetLongLength(1) > 0).ToList();
            //var sql8 = select.Where(a => a.testFieldIntArray.Length > 0).ToList();
            //var sql9 = select.Where(a => a.testFieldIntArray.Count() > 0).ToList();

            var inarray2n = Enumerable.Range(1, 3333).ToArray();
            var sql1111111 = select.Where(a => inarray2n.Contains(a.testFieldInt)).ToList();
            var sql1122222 = select.Where(a => inarray2n.Contains(a.testFieldInt) == false).ToList();
            var sql1133333 = select.Where(a => !inarray2n.Contains(a.testFieldInt)).ToList();
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

            public TableAllTypeEnumType1 testFieldEnum1 { get; set; }
            public TableAllTypeEnumType1? testFieldEnum1Nullable { get; set; }
            public TableAllTypeEnumType2 testFieldEnum2 { get; set; }
            public TableAllTypeEnumType2? testFieldEnum2Nullable { get; set; }

            //* array */
            //public bool[] testFieldBoolArray { get; set; }
            //public sbyte[] testFieldSByteArray { get; set; }
            //public short[] testFieldShortArray { get; set; }
            //public int[] testFieldIntArray { get; set; }
            //public long[] testFieldLongArray { get; set; }
            //public byte[] testFieldByteArray { get; set; }
            //public ushort[] testFieldUShortArray { get; set; }
            //public uint[] testFieldUIntArray { get; set; }
            //public ulong[] testFieldULongArray { get; set; }
            //public double[] testFieldDoubleArray { get; set; }
            //public float[] testFieldFloatArray { get; set; }
            //public decimal[] testFieldDecimalArray { get; set; }
            //public TimeSpan[] testFieldTimeSpanArray { get; set; }
            //public DateTime[] testFieldDateTimeArray { get; set; }
            //public byte[][] testFieldBytesArray { get; set; }
            //public string[] testFieldStringArray { get; set; }
            //public Guid[] testFieldGuidArray { get; set; }

            //public bool?[] testFieldBoolArrayNullable { get; set; }
            //public sbyte?[] testFieldSByteArrayNullable { get; set; }
            //public short?[] testFieldShortArrayNullable { get; set; }
            //public int?[] testFieldIntArrayNullable { get; set; }
            //public long?[] testFielLongArrayNullable { get; set; }
            //public byte?[] testFieldByteArrayNullable { get; set; }
            //public ushort?[] testFieldUShortArrayNullable { get; set; }
            //public uint?[] testFieldUIntArrayNullable { get; set; }
            //public ulong?[] testFieldULongArrayNullable { get; set; }
            //public double?[] testFieldDoubleArrayNullable { get; set; }
            //public float?[] testFieldFloatArrayNullable { get; set; }
            //public decimal?[] testFieldDecimalArrayNullable { get; set; }
            //public TimeSpan?[] testFieldTimeSpanArrayNullable { get; set; }
            //public DateTime?[] testFieldDateTimeArrayNullable { get; set; }
            //public Guid?[] testFieldGuidArrayNullable { get; set; }

            //public TableAllTypeEnumType1[] testFieldEnum1Array { get; set; }
            //public TableAllTypeEnumType1?[] testFieldEnum1ArrayNullable { get; set; }
            //public TableAllTypeEnumType2[] testFieldEnum2Array { get; set; }
            //public TableAllTypeEnumType2?[] testFieldEnum2ArrayNullable { get; set; }
        }

        public enum TableAllTypeEnumType1 { e1, e2, e3, e5 }
        [Flags] public enum TableAllTypeEnumType2 { f1, f2, f3 }
    }
}
