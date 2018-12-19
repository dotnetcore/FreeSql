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

			sql = g.sqlserver.CodeFirst.GetComparisonDDLStatements<Tb_alltype>();
		}

		[JsonObject(MemberSerialization.OptIn), Table(Name = "dbo.tb_alltype")]
		public partial class Tb_alltype {

			[JsonProperty, Column(Name = "Id", DbType = "int", IsPrimary = true, IsIdentity = true)]
			public int Id { get; set; }


			[JsonProperty, Column(Name = "testFieldBool1111", DbType = "bit")]
			public bool TestFieldBool1111 { get; set; }


			[JsonProperty, Column(Name = "testFieldBoolNullable", DbType = "bit", IsNullable = true)]
			public bool? TestFieldBoolNullable { get; set; }


			[JsonProperty, Column(Name = "testFieldByte", DbType = "tinyint")]
			public sbyte TestFieldByte { get; set; }


			[JsonProperty, Column(Name = "testFieldByteNullable", DbType = "tinyint", IsNullable = true)]
			public sbyte? TestFieldByteNullable { get; set; }


			[JsonProperty, Column(Name = "testFieldBytes", DbType = "varbinary(255)", IsNullable = true)]
			public byte[] TestFieldBytes { get; set; }


			[JsonProperty, Column(Name = "testFieldDateTime", DbType = "datetime")]
			public DateTime TestFieldDateTime { get; set; }


			[JsonProperty, Column(Name = "testFieldDateTimeNullable", DbType = "datetime", IsNullable = true)]
			public DateTime? TestFieldDateTimeNullable { get; set; }


			[JsonProperty, Column(Name = "testFieldDateTimeNullableOffset", DbType = "datetimeoffset", IsNullable = true)]
			public DateTime? TestFieldDateTimeNullableOffset { get; set; }


			[JsonProperty, Column(Name = "testFieldDateTimeOffset", DbType = "datetimeoffset")]
			public DateTime TestFieldDateTimeOffset { get; set; }


			[JsonProperty, Column(Name = "testFieldDecimal", DbType = "decimal(10,2)")]
			public decimal TestFieldDecimal { get; set; }


			[JsonProperty, Column(Name = "testFieldDecimalNullable", DbType = "decimal(10,2)", IsNullable = true)]
			public decimal? TestFieldDecimalNullable { get; set; }


			[JsonProperty, Column(Name = "testFieldDouble", DbType = "float")]
			public double TestFieldDouble { get; set; }


			[JsonProperty, Column(Name = "testFieldDoubleNullable", DbType = "float", IsNullable = true)]
			public double? TestFieldDoubleNullable { get; set; }


			[JsonProperty, Column(Name = "testFieldEnum1", DbType = "int")]
			public int TestFieldEnum1 { get; set; }


			[JsonProperty, Column(Name = "testFieldEnum1Nullable", DbType = "int", IsNullable = true)]
			public int? TestFieldEnum1Nullable { get; set; }


			[JsonProperty, Column(Name = "testFieldEnum2", DbType = "bigint")]
			public long TestFieldEnum2 { get; set; }


			[JsonProperty, Column(Name = "testFieldEnum2Nullable", DbType = "bigint", IsNullable = true)]
			public long? TestFieldEnum2Nullable { get; set; }


			[JsonProperty, Column(Name = "testFieldFloat", DbType = "real")]
			public float TestFieldFloat { get; set; }


			[JsonProperty, Column(Name = "testFieldFloatNullable", DbType = "real", IsNullable = true)]
			public float? TestFieldFloatNullable { get; set; }


			[JsonProperty, Column(Name = "testFieldGuid", DbType = "uniqueidentifier")]
			public Guid TestFieldGuid { get; set; }


			[JsonProperty, Column(Name = "testFieldGuidNullable", DbType = "uniqueidentifier", IsNullable = true)]
			public Guid? TestFieldGuidNullable { get; set; }


			[JsonProperty, Column(Name = "testFieldInt", DbType = "int")]
			public int TestFieldInt { get; set; }


			[JsonProperty, Column(Name = "testFieldIntNullable", DbType = "int", IsNullable = true)]
			public int? TestFieldIntNullable { get; set; }


			[JsonProperty, Column(Name = "testFieldLong", DbType = "bigint")]
			public long TestFieldLong { get; set; }


			[JsonProperty, Column(Name = "testFieldSByte", DbType = "tinyint")]
			public sbyte TestFieldSByte { get; set; }


			[JsonProperty, Column(Name = "testFieldSByteNullable", DbType = "tinyint", IsNullable = true)]
			public sbyte? TestFieldSByteNullable { get; set; }


			[JsonProperty, Column(Name = "testFieldShort", DbType = "smallint")]
			public short TestFieldShort { get; set; }


			[JsonProperty, Column(Name = "testFieldShortNullable", DbType = "smallint", IsNullable = true)]
			public short? TestFieldShortNullable { get; set; }


			[JsonProperty, Column(Name = "testFieldString", DbType = "nvarchar(255)", IsNullable = true)]
			public string TestFieldString { get; set; }


			[JsonProperty, Column(Name = "testFieldTimeSpan", DbType = "time")]
			public TimeSpan TestFieldTimeSpan { get; set; }


			[JsonProperty, Column(Name = "testFieldTimeSpanNullable", DbType = "time", IsNullable = true)]
			public TimeSpan? TestFieldTimeSpanNullable { get; set; }


			[JsonProperty, Column(Name = "testFieldUInt", DbType = "int")]
			public int TestFieldUInt { get; set; }


			[JsonProperty, Column(Name = "testFieldUIntNullable", DbType = "int", IsNullable = true)]
			public int? TestFieldUIntNullable { get; set; }


			[JsonProperty, Column(Name = "testFieldULong", DbType = "bigint")]
			public long TestFieldULong { get; set; }


			[JsonProperty, Column(Name = "testFieldULongNullable", DbType = "bigint", IsNullable = true)]
			public long? TestFieldULongNullable { get; set; }


			[JsonProperty, Column(Name = "testFieldUShort", DbType = "smallint")]
			public short TestFieldUShort { get; set; }


			[JsonProperty, Column(Name = "testFieldUShortNullable", DbType = "smallint", IsNullable = true)]
			public short? TestFieldUShortNullable { get; set; }


			[JsonProperty, Column(Name = "testFielLongNullable", DbType = "bigint", IsNullable = true)]
			public long? TestFielLongNullable { get; set; }
		}

			[Table(Name = "tb_alltype")]
		class TableAllType {
			[Column(IsIdentity = true, IsPrimary = true)]
			public int Id { get; set; }

			[Column(Name = "testFieldBool1111")]
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
