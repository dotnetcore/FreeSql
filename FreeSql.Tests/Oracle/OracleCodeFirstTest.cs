using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace FreeSql.Tests.Oracle {
	public class OracleCodeFirstTest {

		[Fact]
		public void AddField() {
			var sql = g.oracle.CodeFirst.GetComparisonDDLStatements<TopicAddField>();

			var id = g.oracle.Insert<TopicAddField>().AppendData(new TopicAddField { }).ExecuteIdentity();

			//var inserted = g.oracle.Insert<TopicAddField>().AppendData(new TopicAddField { }).ExecuteInserted();
		}

		[Table(Name = "TopicAddField", OldName = "xxxtb.TopicAddField")]
		public class TopicAddField {
			[Column(IsIdentity = true)]
			public int Id { get; set; }

			public string name { get; set; }

			[Column(DbType = "varchar2(200 char) not null", OldName = "title")]
			public string title2 { get; set; } = "10";
		}

		[Fact]
		public void GetComparisonDDLStatements() {

			var sql = g.oracle.CodeFirst.GetComparisonDDLStatements<TableAllType>();
			if (string.IsNullOrEmpty(sql) == false) {
				Assert.Equal(@"CREATE TABLE IF NOT EXISTS `cccddd`.`tb_alltype` ( 
  `Id` INT(11) NOT NULL AUTO_INCREMENT, 
  `testFieldBool` BIT(1) NOT NULL, 
  `testFieldSByte` TINYINT(3) NOT NULL, 
  `testFieldShort` SMALLINT(6) NOT NULL, 
  `testFieldInt` INT(11) NOT NULL, 
  `testFieldLong` BIGINT(20) NOT NULL, 
  `testFieldByte` TINYINT(3) UNSIGNED NOT NULL, 
  `testFieldUShort` SMALLINT(5) UNSIGNED NOT NULL, 
  `testFieldUInt` INT(10) UNSIGNED NOT NULL, 
  `testFieldULong` BIGINT(20) UNSIGNED NOT NULL, 
  `testFieldDouble` DOUBLE NOT NULL, 
  `testFieldFloat` FLOAT NOT NULL, 
  `testFieldDecimal` DECIMAL(10,2) NOT NULL, 
  `testFieldTimeSpan` TIME NOT NULL, 
  `testFieldDateTime` DATETIME NOT NULL, 
  `testFieldBytes` VARBINARY(255), 
  `testFieldString` VARCHAR(255), 
  `testFieldGuid` VARCHAR(36), 
  `testFieldBoolNullable` BIT(1), 
  `testFieldSByteNullable` TINYINT(3), 
  `testFieldShortNullable` SMALLINT(6), 
  `testFieldIntNullable` INT(11), 
  `testFielLongNullable` BIGINT(20), 
  `testFieldByteNullable` TINYINT(3) UNSIGNED, 
  `testFieldUShortNullable` SMALLINT(5) UNSIGNED, 
  `testFieldUIntNullable` INT(10) UNSIGNED, 
  `testFieldULongNullable` BIGINT(20) UNSIGNED, 
  `testFieldDoubleNullable` DOUBLE, 
  `testFieldFloatNullable` FLOAT, 
  `testFieldDecimalNullable` DECIMAL(10,2), 
  `testFieldTimeSpanNullable` TIME, 
  `testFieldDateTimeNullable` DATETIME, 
  `testFieldGuidNullable` VARCHAR(36), 
  `testFieldPoint` POINT, 
  `testFieldLineString` LINESTRING, 
  `testFieldPolygon` POLYGON, 
  `testFieldMultiPoint` MULTIPOINT, 
  `testFieldMultiLineString` MULTILINESTRING, 
  `testFieldMultiPolygon` MULTIPOLYGON, 
  `testFieldEnum1` ENUM('E1','E2','E3') NOT NULL, 
  `testFieldEnum1Nullable` ENUM('E1','E2','E3'), 
  `testFieldEnum2` SET('F1','F2','F3') NOT NULL, 
  `testFieldEnum2Nullable` SET('F1','F2','F3'), 
  PRIMARY KEY (`Id`)
) Engine=InnoDB CHARACTER SET utf8;
", sql);
			}

			//sql = g.oracle.CodeFirst.GetComparisonDDLStatements<Tb_alltype>();
		}

		IInsert<TableAllType> insert => g.oracle.Insert<TableAllType>();
		ISelect<TableAllType> select => g.oracle.Select<TableAllType>();

		[Fact]
		public void CurdAllField() {
			var item = new TableAllType { };
			item.Id = (int)insert.AppendData(item).ExecuteIdentity();

			var newitem = select.Where(a => a.Id == item.Id).ToOne();

			var item2 = new TableAllType {
				testFieldBool = true,
				testFieldBoolNullable = true,
				testFieldByte = 255,
				testFieldByteNullable = 127,
				testFieldBytes = Encoding.UTF8.GetBytes("我是中国人"),
				testFieldDateTime = DateTime.Now,
				testFieldDateTimeNullable = DateTime.Now.AddHours(-1),
				testFieldDecimal = 99.99M,
				testFieldDecimalNullable = 99.98M,
				testFieldDouble = 999.99,
				testFieldDoubleNullable = 999.98,
				testFieldEnum1 = TableAllTypeEnumType1.e5,
				testFieldEnum1Nullable = TableAllTypeEnumType1.e3,
				testFieldEnum2 = TableAllTypeEnumType2.f2,
				testFieldEnum2Nullable = TableAllTypeEnumType2.f3,
				testFieldFloat = 19.99F,
				testFieldFloatNullable = 19.98F,
				testFieldGuid = Guid.NewGuid(),
				testFieldGuidNullable = Guid.NewGuid(),
				testFieldInt = int.MaxValue,
				testFieldIntNullable = int.MinValue,
				testFieldLineString = new MygisLineString(new[] { new MygisCoordinate2D(10, 10), new MygisCoordinate2D(50, 10) }),
				testFieldLong = long.MaxValue,
				testFieldMultiLineString = new MygisMultiLineString(new[] {
					new[] { new MygisCoordinate2D(10, 10), new MygisCoordinate2D(50, 10) },
					new[] { new MygisCoordinate2D(50, 10), new MygisCoordinate2D(10, 100) } }),
				testFieldMultiPoint = new MygisMultiPoint(new[] { new MygisCoordinate2D(11, 11), new MygisCoordinate2D(51, 11) }),
				testFieldMultiPolygon = new MygisMultiPolygon(new[] {
					new MygisPolygon(new[] {
						new[] { new MygisCoordinate2D(10, 10), new MygisCoordinate2D(50, 10), new MygisCoordinate2D(10, 50), new MygisCoordinate2D(10, 10) },
						new[] { new MygisCoordinate2D(10, 10), new MygisCoordinate2D(50, 10), new MygisCoordinate2D(10, 50), new MygisCoordinate2D(10, 10) },
						new[] { new MygisCoordinate2D(10, 10), new MygisCoordinate2D(50, 10), new MygisCoordinate2D(10, 50), new MygisCoordinate2D(10, 10) },
						new[] { new MygisCoordinate2D(10, 10), new MygisCoordinate2D(50, 10), new MygisCoordinate2D(10, 50), new MygisCoordinate2D(10, 10) } }),
					new MygisPolygon(new[] {
						new[] { new MygisCoordinate2D(10, 10), new MygisCoordinate2D(50, 10), new MygisCoordinate2D(10, 50), new MygisCoordinate2D(10, 10) },
						new[] { new MygisCoordinate2D(10, 10), new MygisCoordinate2D(50, 10), new MygisCoordinate2D(10, 50), new MygisCoordinate2D(10, 10) },
						new[] { new MygisCoordinate2D(10, 10), new MygisCoordinate2D(50, 10), new MygisCoordinate2D(10, 50), new MygisCoordinate2D(10, 10) },
						new[] { new MygisCoordinate2D(10, 10), new MygisCoordinate2D(50, 10), new MygisCoordinate2D(10, 50), new MygisCoordinate2D(10, 10) } }) }),
				testFieldPoint = new MygisPoint(99, 99),
				testFieldPolygon = new MygisPolygon(new[] {
					new[] { new MygisCoordinate2D(10, 10), new MygisCoordinate2D(50, 10), new MygisCoordinate2D(10, 50), new MygisCoordinate2D(10, 10) },
						new[] { new MygisCoordinate2D(10, 10), new MygisCoordinate2D(50, 10), new MygisCoordinate2D(10, 50), new MygisCoordinate2D(10, 10) },
						new[] { new MygisCoordinate2D(10, 10), new MygisCoordinate2D(50, 10), new MygisCoordinate2D(10, 50), new MygisCoordinate2D(10, 10) },
						new[] { new MygisCoordinate2D(10, 10), new MygisCoordinate2D(50, 10), new MygisCoordinate2D(10, 50), new MygisCoordinate2D(10, 10) } }),
				testFieldSByte = 100,
				testFieldSByteNullable = 99,
				testFieldShort = short.MaxValue,
				testFieldShortNullable = short.MinValue,
				testFieldString = "我是中国人string",
				testFieldTimeSpan = TimeSpan.FromSeconds(999),
				testFieldTimeSpanNullable = TimeSpan.FromSeconds(60),
				testFieldUInt = uint.MaxValue,
				testFieldUIntNullable = uint.MinValue,
				testFieldULong = ulong.MaxValue,
				testFieldULongNullable = ulong.MinValue,
				testFieldUShort = ushort.MaxValue,
				testFieldUShortNullable = ushort.MinValue,
				testFielLongNullable = long.MinValue
			};
			item2.Id = (int)insert.AppendData(item2).ExecuteIdentity();
			var newitem2 = select.Where(a => a.Id == item2.Id).ToOne();

			var items = select.ToList();
		}

		[Table(Name = "tb_alltype")]
		class TableAllType {
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

			public MygisPoint testFieldPoint { get; set; }
			public MygisLineString testFieldLineString { get; set; }
			public MygisPolygon testFieldPolygon { get; set; }
			public MygisMultiPoint testFieldMultiPoint { get; set; }
			public MygisMultiLineString testFieldMultiLineString { get; set; }
			public MygisMultiPolygon testFieldMultiPolygon { get; set; }

			public TableAllTypeEnumType1 testFieldEnum1 { get; set; }
			public TableAllTypeEnumType1? testFieldEnum1Nullable { get; set; }
			public TableAllTypeEnumType2 testFieldEnum2 { get; set; }
			public TableAllTypeEnumType2? testFieldEnum2Nullable { get; set; }
		}

		public enum TableAllTypeEnumType1 { e1, e2, e3, e5 }
		[Flags] public enum TableAllTypeEnumType2 { f1, f2, f3 }
	}
}
