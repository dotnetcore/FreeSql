using FreeSql.DataAnnotations;
using System;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.OracleExpression {
	public class OtherTest {

		ISelect<TableAllType> select => g.oracle.Select<TableAllType>();

		public OtherTest() {
		}


		[Fact]
		public void Array() {
			//in not in
			var sql111 = select.Where(a => new[] { 1, 2, 3 }.Contains(a.Int)).ToList();
			//var sql112 = select.Where(a => new[] { 1, 2, 3 }.Contains(a.Int) == false).ToList();
			var sql113 = select.Where(a => !new[] { 1, 2, 3 }.Contains(a.Int)).ToList();

			var inarray = new[] { 1, 2, 3 };
			var sql1111 = select.Where(a => inarray.Contains(a.Int)).ToList();
			//var sql1122 = select.Where(a => inarray.Contains(a.Int) == false).ToList();
			var sql1133 = select.Where(a => !inarray.Contains(a.Int)).ToList();
		}

		[Table(Name = "tb_alltype")]
		class TableAllType {
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
