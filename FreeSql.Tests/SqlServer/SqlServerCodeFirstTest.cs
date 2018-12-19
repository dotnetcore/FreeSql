using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.SqlServer {
	public class SqlServerCodeFirstTest {
		[Fact]
		public void GetComparisonDDLStatements() {

			var sql = g.sqlserver.CodeFirst.GetComparisonDDLStatements<TableAllType>();
			

		}


		[Table(Name = "tb_alltype")]
		class TableAllType {
			[Column(IsIdentity = true, IsPrimary = true)]
			public int Id { get; set; }

			[Column(OldName = "testFieldBool1111")]
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
			public DateTimeOffset testFieldDateTimeOffset { get; set; }
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
			public DateTimeOffset? testFieldDateTimeNullableOffset { get; set; }
			public Guid? testFieldGuidNullable { get; set; }

			public TableAllTypeEnumType1 testFieldEnum1 { get; set; }
			public TableAllTypeEnumType1? testFieldEnum1Nullable { get; set; }
			public TableAllTypeEnumType2 testFieldEnum2 { get; set; }
			public TableAllTypeEnumType2? testFieldEnum2Nullable { get; set; }
		}

		public enum TableAllTypeEnumType1 { e1, e2, e3, e5 }
		[Flags] public enum TableAllTypeEnumType2 { f1, f2, f3 }
	}
}
