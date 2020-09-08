using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.MsAccessExpression
{
    public class OtherTest
    {

        ISelect<TableAllType> select => g.msaccess.Select<TableAllType>();

        public OtherTest()
        {
        }

        [Fact]
        public void Div()
        {
            var t1 = select.Where(a => a.Int / 3 > 3).Limit(10).ToList();
            var t2 = select.Where(a => a.Long / 3 > 3).Limit(10).ToList();
            var t3 = select.Where(a => a.Short / 3 > 3).Limit(10).ToList();

            var t4 = select.Where(a => a.Int / 3.0 > 3).Limit(10).ToList();
            var t5 = select.Where(a => a.Long / 3.0 > 3).Limit(10).ToList();
            var t6 = select.Where(a => a.Short / 3.0 > 3).Limit(10).ToList();

            var t7 = select.Where(a => a.Double / 3 > 3).Limit(10).ToList();
            var t8 = select.Where(a => a.Decimal / 3 > 3).Limit(10).ToList();
            var t9 = select.Where(a => a.Float / 3 > 3).Limit(10).ToList();
        }

        [Fact]
        public void Boolean()
        {
            var t1 = select.Where(a => a.Bool == true).ToList();
            var t2 = select.Where(a => a.Bool != true).ToList();
            var t3 = select.Where(a => a.Bool == false).ToList();
            var t4 = select.Where(a => !a.Bool).ToList();
            var t5 = select.Where(a => a.Bool).ToList();
            var t51 = select.WhereCascade(a => a.Bool).Limit(10).ToList();

            var t11 = select.Where(a => a.BoolNullable == true).ToList();
            var t22 = select.Where(a => a.BoolNullable != true).ToList();
            var t33 = select.Where(a => a.BoolNullable == false).ToList();
            var t44 = select.Where(a => !a.BoolNullable.Value).ToList();
            var t55 = select.Where(a => a.BoolNullable.Value).ToList();

            var t111 = select.Where(a => a.Bool == true && a.Id > 0).ToList();
            var t222 = select.Where(a => a.Bool != true && a.Id > 0).ToList();
            var t333 = select.Where(a => a.Bool == false && a.Id > 0).ToList();
            var t444 = select.Where(a => !a.Bool && a.Id > 0).ToList();
            var t555 = select.Where(a => a.Bool && a.Id > 0).ToList();

            var t1111 = select.Where(a => a.BoolNullable == true && a.Id > 0).ToList();
            var t2222 = select.Where(a => a.BoolNullable != true && a.Id > 0).ToList();
            var t3333 = select.Where(a => a.BoolNullable == false && a.Id > 0).ToList();
            var t4444 = select.Where(a => !a.BoolNullable.Value && a.Id > 0).ToList();
            var t5555 = select.Where(a => a.BoolNullable.Value && a.Id > 0).ToList();

            var t11111 = select.Where(a => a.Bool == true && a.Id > 0 && a.Bool == true).ToList();
            var t22222 = select.Where(a => a.Bool != true && a.Id > 0 && a.Bool != true).ToList();
            var t33333 = select.Where(a => a.Bool == false && a.Id > 0 && a.Bool == false).ToList();
            var t44444 = select.Where(a => !a.Bool && a.Id > 0 && !a.Bool).ToList();
            var t55555 = select.Where(a => a.Bool && a.Id > 0 && a.Bool).ToList();

            var t111111 = select.Where(a => a.BoolNullable == true && a.Id > 0 && a.BoolNullable == true).ToList();
            var t222222 = select.Where(a => a.BoolNullable != true && a.Id > 0 && a.BoolNullable != true).ToList();
            var t333333 = select.Where(a => a.BoolNullable == false && a.Id > 0 && a.BoolNullable == false).ToList();
            var t444444 = select.Where(a => !a.BoolNullable.Value && a.Id > 0 && !a.BoolNullable.Value).ToList();
            var t555555 = select.Where(a => a.BoolNullable.Value && a.Id > 0 && a.BoolNullable.Value).ToList();
        }

        [Fact]
        public void Array()
        {
            IEnumerable<int> testlinqlist = new List<int>(new[] { 1, 2, 3 });
            var testlinq = select.Where(a => testlinqlist.Contains(a.Int)).ToList();

            //in not in
            var sql111 = select.Where(a => new[] { 1, 2, 3 }.Contains(a.Int)).ToList();
            var sql112 = select.Where(a => new[] { 1, 2, 3 }.Contains(a.Int) == false).ToList();
            var sql113 = select.Where(a => !new[] { 1, 2, 3 }.Contains(a.Int)).ToList();

            var inarray = new[] { 1, 2, 3 };
            var sql1111 = select.Where(a => inarray.Contains(a.Int)).ToList();
            var sql1122 = select.Where(a => inarray.Contains(a.Int) == false).ToList();
            var sql1133 = select.Where(a => !inarray.Contains(a.Int)).ToList();

            //in not in
            var sql11111 = select.Where(a => new List<int>() { 1, 2, 3 }.Contains(a.Int)).ToList();
            var sql11222 = select.Where(a => new List<int>() { 1, 2, 3 }.Contains(a.Int) == false).ToList();
            var sql11333 = select.Where(a => !new List<int>() { 1, 2, 3 }.Contains(a.Int)).ToList();

            var sql11111a = select.Where(a => new List<int>(new[] { 1, 2, 3 }).Contains(a.Int)).ToList();
            var sql11222b = select.Where(a => new List<int>(new[] { 1, 2, 3 }).Contains(a.Int) == false).ToList();
            var sql11333c = select.Where(a => !new List<int>(new[] { 1, 2, 3 }).Contains(a.Int)).ToList();

            var inarray2 = new List<int>() { 1, 2, 3 };
            var sql111111 = select.Where(a => inarray.Contains(a.Int)).ToList();
            var sql112222 = select.Where(a => inarray.Contains(a.Int) == false).ToList();
            var sql113333 = select.Where(a => !inarray.Contains(a.Int)).ToList();

            var inarray2n = Enumerable.Range(1, 3333).ToArray();
            var sql1111111 = select.Where(a => inarray2n.Contains(a.Int)).ToList();
            var sql1122222 = select.Where(a => inarray2n.Contains(a.Int) == false).ToList();
            var sql1133333 = select.Where(a => !inarray2n.Contains(a.Int)).ToList();
        }

        [Table(Name = "tb_alltype")]
        class TableAllType
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }

            public string id2 { get; set; } = "id2=10";

            public bool Bool { get; set; }
            public sbyte SByte { get; set; }
            public short Short { get; set; }
            public int Int { get; set; }
            public long Long { get; set; }
            public byte Byte { get; set; }
            public ushort UShort { get; set; }
            public uint UInt { get; set; }
            public ulong ULong { get; set; }
            public double Double { get; set; }
            public float Float { get; set; }
            public decimal Decimal { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public DateTime DateTime { get; set; }
            public DateTime DateTimeOffSet { get; set; }
            public byte[] Bytes { get; set; }
            public string String { get; set; }
            public Guid Guid { get; set; }

            public bool? BoolNullable { get; set; }
            public sbyte? SByteNullable { get; set; }
            public short? ShortNullable { get; set; }
            public int? IntNullable { get; set; }
            public long? testFielLongNullable { get; set; }
            public byte? ByteNullable { get; set; }
            public ushort? UShortNullable { get; set; }
            public uint? UIntNullable { get; set; }
            public ulong? ULongNullable { get; set; }
            public double? DoubleNullable { get; set; }
            public float? FloatNullable { get; set; }
            public decimal? DecimalNullable { get; set; }
            public TimeSpan? TimeSpanNullable { get; set; }
            public DateTime? DateTimeNullable { get; set; }
            public DateTime? DateTimeOffSetNullable { get; set; }
            public Guid? GuidNullable { get; set; }

            public TableAllTypeEnumType1 Enum1 { get; set; }
            public TableAllTypeEnumType1? Enum1Nullable { get; set; }
            public TableAllTypeEnumType2 Enum2 { get; set; }
            public TableAllTypeEnumType2? Enum2Nullable { get; set; }
        }

        public enum TableAllTypeEnumType1 { e1, e2, e3, e5 }
        [Flags] public enum TableAllTypeEnumType2 { f1, f2, f3 }
    }
}
