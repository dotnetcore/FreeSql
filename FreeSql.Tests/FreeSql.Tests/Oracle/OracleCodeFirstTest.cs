using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace FreeSql.Tests.Oracle
{
    public class OracleCodeFirstTest
    {

        [Fact]
        public void 中文表_字段()
        {
            var sql = g.oracle.CodeFirst.GetComparisonDDLStatements<测试中文表>();
            g.oracle.CodeFirst.SyncStructure<测试中文表>();

            var item = new 测试中文表
            {
                标题 = "测试标题",
                创建时间 = DateTime.Now
            };
            Assert.Equal(1, g.oracle.Insert<测试中文表>().AppendData(item).ExecuteAffrows());
            Assert.NotEqual(Guid.Empty, item.编号);
            var item2 = g.oracle.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);
        }
        class 测试中文表
        {
            [Column(IsPrimary = true)]
            public Guid 编号 { get; set; }

            public string 标题 { get; set; }

            public DateTime 创建时间 { get; set; }
        }

        [Fact]
        public void AddUniques()
        {
            var sql = g.oracle.CodeFirst.GetComparisonDDLStatements<AddUniquesInfo>();
            g.oracle.CodeFirst.SyncStructure<AddUniquesInfo>();
        }
        [Table(Name = "AddUniquesInfo", OldName = "AddUniquesInfo2")]
        class AddUniquesInfo
        {
            public Guid id { get; set; }
            [Column(Unique = "uk_phone")]
            public string phone { get; set; }

            [Column(Unique = "uk_group_index, uk_group_index22")]
            public string group { get; set; }
            [Column(Unique = "uk_group_index")]
            public int index { get; set; }
            [Column(Unique = "uk_group_index22")]
            public string index22 { get; set; }
        }
        [Fact]
        public void AddField()
        {
            var sql = g.oracle.CodeFirst.GetComparisonDDLStatements<TopicAddField>();

            var id = g.oracle.Insert<TopicAddField>().AppendData(new TopicAddField { }).ExecuteIdentity();

            //var inserted = g.oracle.Insert<TopicAddField>().AppendData(new TopicAddField { }).ExecuteInserted();
        }

        [Table(Name = "TopicAddField", OldName = "xxxtb.TopicAddField")]
        public class TopicAddField
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }

            public string name { get; set; }

            [Column(DbType = "varchar2(200 char) not null", OldName = "title")]
            public string title2 { get; set; } = "10";

            [Column(IsIgnore = true)]
            public DateTime ct { get; set; } = DateTime.Now;
        }

        [Fact]
        public void GetComparisonDDLStatements()
        {

            var sql = g.oracle.CodeFirst.GetComparisonDDLStatements<TableAllType>();
            if (string.IsNullOrEmpty(sql) == false)
            {
                Assert.Equal(@"CREATE TABLE IF NOT EXISTS `cccddd`.`tb_alltype` ( 
  `Id` INT(11) NOT NULL AUTO_INCREMENT, 
  `Bool` BIT(1) NOT NULL, 
  `SByte` TINYINT(3) NOT NULL, 
  `Short` SMALLINT(6) NOT NULL, 
  `Int` INT(11) NOT NULL, 
  `Long` BIGINT(20) NOT NULL, 
  `Byte` TINYINT(3) UNSIGNED NOT NULL, 
  `UShort` SMALLINT(5) UNSIGNED NOT NULL, 
  `UInt` INT(10) UNSIGNED NOT NULL, 
  `ULong` BIGINT(20) UNSIGNED NOT NULL, 
  `Double` DOUBLE NOT NULL, 
  `Float` FLOAT NOT NULL, 
  `Decimal` DECIMAL(10,2) NOT NULL, 
  `TimeSpan` TIME NOT NULL, 
  `DateTime` DATETIME NOT NULL, 
  `Bytes` VARBINARY(255), 
  `String` VARCHAR(255), 
  `Guid` VARCHAR(36), 
  `BoolNullable` BIT(1), 
  `SByteNullable` TINYINT(3), 
  `ShortNullable` SMALLINT(6), 
  `IntNullable` INT(11), 
  `testFielLongNullable` BIGINT(20), 
  `ByteNullable` TINYINT(3) UNSIGNED, 
  `UShortNullable` SMALLINT(5) UNSIGNED, 
  `UIntNullable` INT(10) UNSIGNED, 
  `ULongNullable` BIGINT(20) UNSIGNED, 
  `DoubleNullable` DOUBLE, 
  `FloatNullable` FLOAT, 
  `DecimalNullable` DECIMAL(10,2), 
  `TimeSpanNullable` TIME, 
  `DateTimeNullable` DATETIME, 
  `GuidNullable` VARCHAR(36), 
  `Point` POINT, 
  `LineString` LINESTRING, 
  `Polygon` POLYGON, 
  `MultiPoint` MULTIPOINT, 
  `MultiLineString` MULTILINESTRING, 
  `MultiPolygon` MULTIPOLYGON, 
  `Enum1` ENUM('E1','E2','E3') NOT NULL, 
  `Enum1Nullable` ENUM('E1','E2','E3'), 
  `Enum2` SET('F1','F2','F3') NOT NULL, 
  `Enum2Nullable` SET('F1','F2','F3'), 
  PRIMARY KEY (`Id`)
) Engine=InnoDB;
", sql);
            }

            //sql = g.oracle.CodeFirst.GetComparisonDDLStatements<Tb_alltype>();
        }

        IInsert<TableAllType> insert => g.oracle.Insert<TableAllType>();
        ISelect<TableAllType> select => g.oracle.Select<TableAllType>();

        [Fact]
        public void CurdAllField()
        {
            var item = new TableAllType { };
            item.Id = (int)insert.AppendData(item).ExecuteIdentity();

            var newitem = select.Where(a => a.Id == item.Id).ToOne();

            var item2 = new TableAllType
            {
                Bool = true,
                BoolNullable = true,
                Byte = 255,
                ByteNullable = 127,
                Bytes = Encoding.UTF8.GetBytes("我是中国人"),
                DateTime = DateTime.Now,
                DateTimeNullable = DateTime.Now.AddHours(-1),
                Decimal = 99.99M,
                DecimalNullable = 99.98M,
                Double = 999.99,
                DoubleNullable = 999.98,
                Enum1 = TableAllTypeEnumType1.e5,
                Enum1Nullable = TableAllTypeEnumType1.e3,
                Enum2 = TableAllTypeEnumType2.f2,
                Enum2Nullable = TableAllTypeEnumType2.f3,
                Float = 19.99F,
                FloatNullable = 19.98F,
                Guid = Guid.NewGuid(),
                GuidNullable = Guid.NewGuid(),
                Int = int.MaxValue,
                IntNullable = int.MinValue,
                SByte = 100,
                SByteNullable = 99,
                Short = short.MaxValue,
                ShortNullable = short.MinValue,
                String = "我是中国人string",
                TimeSpan = TimeSpan.FromSeconds(999),
                TimeSpanNullable = TimeSpan.FromSeconds(60),
                UInt = uint.MaxValue,
                UIntNullable = uint.MinValue,
                ULong = ulong.MaxValue,
                ULongNullable = ulong.MinValue,
                UShort = ushort.MaxValue,
                UShortNullable = ushort.MinValue,
                testFielLongNullable = long.MinValue
            };
            item2.Id = (int)insert.AppendData(item2).ExecuteIdentity();
            var newitem2 = select.Where(a => a.Id == item2.Id).ToOne();

            var items = select.ToList();
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
