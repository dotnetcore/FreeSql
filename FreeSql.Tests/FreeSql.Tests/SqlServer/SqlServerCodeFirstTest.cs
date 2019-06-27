using FreeSql.DataAnnotations;
using FreeSql.Tests.DataContext.SqlServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace FreeSql.Tests.SqlServer
{

    [Collection("SqlServerCollection")]
    public class SqlServerCodeFirstTest
    {

        SqlServerFixture _sqlserverFixture;

        public SqlServerCodeFirstTest(SqlServerFixture sqlserverFixture)
        {
            _sqlserverFixture = sqlserverFixture;
        }

        [Fact]
        public void 中文表_字段()
        {
            var sql = _sqlserverFixture.SqlServer.CodeFirst.GetComparisonDDLStatements<测试中文表>();
            _sqlserverFixture.SqlServer.CodeFirst.SyncStructure<测试中文表>();

            var item = new 测试中文表
            {
                标题 = "测试标题",
                创建时间 = DateTime.Now
            };
            Assert.Equal(1, _sqlserverFixture.SqlServer.Insert<测试中文表>().AppendData(item).ExecuteAffrows());
            Assert.NotEqual(Guid.Empty, item.编号);
            var item2 = _sqlserverFixture.SqlServer.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
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
            var sql = _sqlserverFixture.SqlServer.CodeFirst.GetComparisonDDLStatements<AddUniquesInfo>();
            _sqlserverFixture.SqlServer.CodeFirst.SyncStructure<AddUniquesInfo>();
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
            var sql = _sqlserverFixture.SqlServer.CodeFirst.GetComparisonDDLStatements<TopicAddField>();

            var id = _sqlserverFixture.SqlServer.Insert<TopicAddField>().AppendData(new TopicAddField { }).ExecuteIdentity();
        }

        [Table(Name = "dbo2.TopicAddField", OldName = "tedb1.dbo.TopicAddField")]
        public class TopicAddField
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }

            public int name { get; set; } = 3000;

            [Column(DbType = "varchar(200) not null", OldName = "title")]
            public string title222 { get; set; } = "333";

            [Column(DbType = "varchar(200) not null")]
            public string title222333 { get; set; } = "xxx";

            [Column(DbType = "varchar(100) not null", OldName = "title122333aaa")]
            public string titleaaa { get; set; } = "fsdf";

            [Column(IsIgnore = true)]
            public DateTime ct { get; set; } = DateTime.Now;
        }

        [Fact]
        public void GetComparisonDDLStatements()
        {

            var sql = _sqlserverFixture.SqlServer.CodeFirst.GetComparisonDDLStatements<TableAllType>();

            sql = _sqlserverFixture.SqlServer.CodeFirst.GetComparisonDDLStatements<Tb_alltype>();
        }

        IInsert<TableAllType> insert => _sqlserverFixture.SqlServer.Insert<TableAllType>();
        ISelect<TableAllType> select => _sqlserverFixture.SqlServer.Select<TableAllType>();

        [Fact]
        public void CurdAllField()
        {
            var item = new TableAllType { };
            item.Id = (int)insert.AppendData(item).ExecuteIdentity();

            var newitem = select.Where(a => a.Id == item.Id).ToOne();

            var item2 = new TableAllType
            {
                testFieldBool = true,
                testFieldBoolNullable = true,
                testFieldByte = byte.MaxValue,
                testFieldByteNullable = byte.MinValue,
                testFieldBytes = Encoding.GetEncoding("gb2312").GetBytes("我是中国人"),
                testFieldDateTime = DateTime.Now,
                testFieldDateTimeNullable = DateTime.Now.AddHours(1),
                testFieldDateTimeNullableOffset = new DateTimeOffset(DateTime.Now.AddHours(1), TimeSpan.FromHours(8)),
                testFieldDateTimeOffset = new DateTimeOffset(DateTime.Now, TimeSpan.FromHours(8)),
                testFieldDecimal = 998.99M,
                testFieldDecimalNullable = 999.12M,
                testFieldDouble = 99.199,
                testFieldDoubleNullable = 99.211,
                testFieldEnum1 = TableAllTypeEnumType1.e2,
                testFieldEnum1Nullable = TableAllTypeEnumType1.e3,
                testFieldEnum2 = TableAllTypeEnumType2.f3,
                testFieldEnum2Nullable = TableAllTypeEnumType2.f2,
                testFieldFloat = 0.99F,
                testFieldFloatNullable = 0.11F,
                testFieldGuid = Guid.NewGuid(),
                testFieldGuidNullable = Guid.NewGuid(),
                testFieldInt = int.MaxValue,
                testFieldIntNullable = int.MinValue,
                testFieldLong = long.MaxValue,
                testFieldSByte = sbyte.MaxValue,
                testFieldSByteNullable = sbyte.MinValue,
                testFieldShort = short.MaxValue,
                testFieldShortNullable = short.MinValue,
                testFieldString = "我是中国人string",
                testFieldTimeSpan = TimeSpan.FromSeconds(999),
                testFieldTimeSpanNullable = TimeSpan.FromSeconds(30),
                testFieldUInt = uint.MaxValue,
                testFieldUIntNullable = uint.MinValue,
                testFieldULong = ulong.MaxValue,
                testFieldULongNullable = ulong.MinValue,
                testFieldUShort = ushort.MaxValue,
                testFieldUShortNullable = ushort.MinValue,
                testFielLongNullable = long.MinValue
            };

            var sqlPar = insert.AppendData(item2).ToSql();
            var sqlText = insert.AppendData(item2).NoneParameter().ToSql();

            var item3 = insert.AppendData(item2).ExecuteInserted();
            var newitem2 = select.Where(a => a.Id == item2.Id).ToOne();

            var items = select.ToList();
        }

        [JsonObject(MemberSerialization.OptIn), Table(Name = "dbo.tb_alltype")]
        public partial class Tb_alltype
        {

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
        class TableAllType
        {
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
