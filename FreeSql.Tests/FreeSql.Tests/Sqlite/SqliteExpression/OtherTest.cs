using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.SqliteExpression
{
    public class OtherTest
    {

        ISelect<TableAllType> select => g.sqlite.Select<TableAllType>();

        public OtherTest()
        {
        }

        [Fact]
        public void Boolean()
        {
            var t1 = select.Where(a => a.Bool == true).ToList();
            var t2 = select.Where(a => a.Bool != true).ToList();
            var t3 = select.Where(a => a.Bool == false).ToList();
            var t4 = select.Where(a => !a.Bool).ToList();
            var t5 = select.Where(a => a.Bool).ToList();

            var t11 = select.Where(a => a.BoolNullable == true).ToList();
            var t22 = select.Where(a => a.BoolNullable != true).ToList();
            var t33 = select.Where(a => a.BoolNullable == false).ToList();
            var t44 = select.Where(a => !a.BoolNullable.Value).ToList();
            var t55 = select.Where(a => a.BoolNullable.Value).ToList();
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
