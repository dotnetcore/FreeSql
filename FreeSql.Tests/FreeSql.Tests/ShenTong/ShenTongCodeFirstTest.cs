using FreeSql.DataAnnotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Xunit;

namespace FreeSql.Tests.ShenTong
{
    public class ShenTongCodeFirstTest
    {
        [Fact]
        public void InsertUpdateParameter()
        {
            var fsql = g.shentong;
            fsql.CodeFirst.SyncStructure<ts_iupstr_bak>();
            var item = new ts_iupstr { id = Guid.NewGuid(), title = string.Join(",", Enumerable.Range(0, 2000).Select(a => "我是中国人")) };
            Assert.Equal(1, fsql.Insert(item).ExecuteAffrows());
            var find = fsql.Select<ts_iupstr>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(find.id, item.id);
            Assert.Equal(find.title, item.title);
        }
        [Table(Name = "ts_iupstr_bak", DisableSyncStructure = true)]
        class ts_iupstr
        {
            public Guid id { get; set; }
            public string title { get; set; }
        }
        class ts_iupstr_bak
        {
            public Guid id { get; set; }
            [Column(StringLength = -1)]
            public string title { get; set; }
        }

        [Fact]
        public void StringLength()
        {
            var dll = g.shentong.CodeFirst.GetComparisonDDLStatements<TS_SLTB>();
            g.shentong.CodeFirst.SyncStructure<TS_SLTB>();
        }
        class TS_SLTB
        {
            public Guid Id { get; set; }
            [Column(StringLength = 50)]
            public string Title { get; set; }

            [Column(IsNullable = false, StringLength = 50)]
            public string TitleSub { get; set; }
        }

        [Fact]
        public void 中文表_字段()
        {
            var sql = g.shentong.CodeFirst.GetComparisonDDLStatements<测试中文表>();
            g.shentong.CodeFirst.SyncStructure<测试中文表>();

            var item = new 测试中文表
            {
                标题 = "测试标题",
                创建时间 = DateTime.Now
            };
            Assert.Equal(1, g.shentong.Insert<测试中文表>().NoneParameter().AppendData(item).ExecuteAffrows());
            Assert.NotEqual(Guid.Empty, item.编号);
            var item2 = g.shentong.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新";
            Assert.Equal(1, g.shentong.Update<测试中文表>().NoneParameter().SetSource(item).ExecuteAffrows());
            item2 = g.shentong.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo";
            var repo = g.shentong.GetRepository<测试中文表>();
            repo.DbContextOptions.NoneParameter = true;
            Assert.Equal(1, repo.Update(item));
            item2 = g.shentong.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo22";
            Assert.Equal(1, repo.Update(item));
            item2 = g.shentong.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);
        }
        class 测试中文表
        {
            [Column(IsPrimary = true)]
            public Guid 编号 { get; set; }

            public string 标题 { get; set; }

            [Column(ServerTime = DateTimeKind.Local, CanUpdate = false)]
            public DateTime 创建时间 { get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime 更新时间 { get; set; }
        }

        [Fact]
        public void AddUniques()
        {
            var sql = g.shentong.CodeFirst.GetComparisonDDLStatements<AddUniquesInfo>();
            g.shentong.CodeFirst.SyncStructure<AddUniquesInfo>();
            g.shentong.CodeFirst.SyncStructure(typeof(AddUniquesInfo), "AddUniquesInfo1");
        }
        [Table(Name = "AddUniquesInfo", OldName = "AddUniquesInfo2")]
        [Index("{tablename}_uk_phone", "phone", true)]
        [Index("{tablename}_uk_group_index", "group,index", true)]
        [Index("{tablename}_uk_group_index22", "group, index22", false)]
        class AddUniquesInfo
        {
            public Guid id { get; set; }
            public string phone { get; set; }

            public string group { get; set; }
            public int index { get; set; }
            public string index22 { get; set; }
        }

        [Fact]
        public void AddField()
        {
            var sql = g.shentong.CodeFirst.GetComparisonDDLStatements<TopicAddField>();
            g.shentong.Select<TopicAddField>();

            var id = g.shentong.Insert<TopicAddField>().AppendData(new TopicAddField { }).ExecuteIdentity();
        }

        [Table(Name = "ccc.TopicAddField", OldName = "TopicAddField")]
        public class TopicAddField
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }

            public string name { get; set; } = "xxx";

            public int clicks { get; set; } = 10;
            //public int name { get; set; } = 3000;

            //[Column(DbType = "varchar(200) not null", OldName = "title")]
            //public string title222 { get; set; } = "333";

            //[Column(DbType = "varchar(200) not null")]
            //public string title222333 { get; set; } = "xxx";

            //[Column(DbType = "varchar(100) not null", OldName = "title122333aaa")]
            //public string titleaaa { get; set; } = "fsdf";


            [Column(IsIgnore = true)]
            public DateTime ct { get; set; } = DateTime.Now;
        }

        [Fact]
        public void GetComparisonDDLStatements()
        {
            var sql = g.shentong.CodeFirst.GetComparisonDDLStatements<TableAllType>();
            Assert.True(string.IsNullOrEmpty(sql)); //测试运行两次后
            g.shentong.Select<TableAllType>();
        }

        IInsert<TableAllType> insert => g.shentong.Insert<TableAllType>();
        ISelect<TableAllType> select => g.shentong.Select<TableAllType>();

        [Fact]
        public void CurdAllField()
        {
            //var sql1 = select.Where(a => a.testFieldIntArray.Contains(1)).ToSql();
            //var sql2 = select.Where(a => a.testFieldIntArray.Contains(1)).ToSql();

            var item = new TableAllType { };
            item.Id = (int)insert.AppendData(item).ExecuteIdentity();

            var newitem = select.Where(a => a.Id == item.Id).ToOne();

            var item2 = new TableAllType
            {
                testFieldBool = true,
                //testFieldBoolArray = new[] { true, true, false, false },
                //testFieldBoolArrayNullable = new bool?[] { true, true, null, false, false },
                testFieldBoolNullable = true,
                testFieldByte = byte.MaxValue,
                //testFieldByteArray = new byte[] { 0, 1, 2, 3, 4, 5, 6 },
                //testFieldByteArrayNullable = new byte?[] { 0, 1, 2, 3, null, 4, 5, 6 },
                testFieldByteNullable = byte.MinValue,
                testFieldBytes = Encoding.UTF8.GetBytes("我是中国人"),
                //testFieldBytesArray = new[] { Encoding.UTF8.GetBytes("我是中国人"), Encoding.UTF8.GetBytes("我是中国人") },
                testFieldDateTime = DateTime.Now,
                //testFieldDateTimeArray = new[] { DateTime.Now, DateTime.Now.AddHours(2) },
                //testFieldDateTimeArrayNullable = new DateTime?[] { DateTime.Now, null, DateTime.Now.AddHours(2) },
                testFieldDateTimeNullable = DateTime.Now.AddDays(-1),
                testFieldDecimal = 999.99M,
                //testFieldDecimalArray = new[] { 999.91M, 999.92M, 999.93M },
                //testFieldDecimalArrayNullable = new decimal?[] { 998.11M, 998.12M, 998.13M },
                testFieldDecimalNullable = 111.11M,
                testFieldDouble = 888.88,
                //testFieldDoubleArray = new[] { 888.81, 888.82, 888.83 },
                //testFieldDoubleArrayNullable = new double?[] { 888.11, 888.12, null, 888.13 },
                testFieldDoubleNullable = 222.22,
                testFieldEnum1 = TableAllTypeEnumType1.e3,
                //testFieldEnum1Array = new[] { TableAllTypeEnumType1.e5, TableAllTypeEnumType1.e2, TableAllTypeEnumType1.e1 },
                //testFieldEnum1ArrayNullable = new TableAllTypeEnumType1?[] { TableAllTypeEnumType1.e5, TableAllTypeEnumType1.e2, null, TableAllTypeEnumType1.e1 },
                testFieldEnum1Nullable = TableAllTypeEnumType1.e2,
                testFieldEnum2 = TableAllTypeEnumType2.f2,
                //testFieldEnum2Array = new[] { TableAllTypeEnumType2.f3, TableAllTypeEnumType2.f1 },
                //testFieldEnum2ArrayNullable = new TableAllTypeEnumType2?[] { TableAllTypeEnumType2.f3, null, TableAllTypeEnumType2.f1 },
                testFieldEnum2Nullable = TableAllTypeEnumType2.f3,
                testFieldFloat = 777.77F,
                //testFieldFloatArray = new[] { 777.71F, 777.72F, 777.73F },
                //testFieldFloatArrayNullable = new float?[] { 777.71F, 777.72F, null, 777.73F },
                testFieldFloatNullable = 333.33F,
                testFieldGuid = Guid.NewGuid(),
                //testFieldGuidArray = new[] { Guid.NewGuid(), Guid.NewGuid() },
                //testFieldGuidArrayNullable = new Guid?[] { Guid.NewGuid(), null, Guid.NewGuid() },
                testFieldGuidNullable = Guid.NewGuid(),
                testFieldInt = int.MaxValue,
                //testFieldIntArray = new[] { 1, 2, 3, 4, 5 },
                //testFieldIntArrayNullable = new int?[] { 1, 2, 3, null, 4, 5 },
                testFieldIntNullable = int.MinValue,
                testFieldLong = long.MaxValue,
                //testFieldLongArray = new long[] { 10, 20, 30, 40, 50 },
                testFieldSByte = sbyte.MaxValue,
                //testFieldSByteArray = new sbyte[] { 1, 2, 3, 4, 5 },
                //testFieldSByteArrayNullable = new sbyte?[] { 1, 2, 3, null, 4, 5 },
                testFieldSByteNullable = sbyte.MinValue,
                testFieldShort = short.MaxValue,
                //testFieldShortArray = new short[] { 1, 2, 3, 4, 5 },
                //testFieldShortArrayNullable = new short?[] { 1, 2, 3, null, 4, 5 },
                testFieldShortNullable = short.MinValue,
                testFieldString = "我是中国人string'\\?!@#$%^&*()_+{}}{~?><<>",
                testFieldChar = 'X',
                //testFieldStringArray = new[] { "我是中国人String1", "我是中国人String2", null, "我是中国人String3" },
                testFieldTimeSpan = TimeSpan.FromHours(10),
                //testFieldTimeSpanArray = new[] { TimeSpan.FromHours(10), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60) },
                //testFieldTimeSpanArrayNullable = new TimeSpan?[] { TimeSpan.FromHours(10), TimeSpan.FromSeconds(10), null, TimeSpan.FromSeconds(60) },
                testFieldTimeSpanNullable = TimeSpan.FromSeconds(90),
                testFieldUInt = uint.MaxValue,
                //testFieldUIntArray = new uint[] { 1, 2, 3, 4, 5 },
                //testFieldUIntArrayNullable = new uint?[] { 1, 2, 3, null, 4, 5 },
                testFieldUIntNullable = uint.MinValue,
                testFieldULong = ulong.MaxValue,
                //testFieldULongArray = new ulong[] { 10, 20, 30, 40, 50 },
                //testFieldULongArrayNullable = new ulong?[] { 10, 20, 30, null, 40, 50 },
                testFieldULongNullable = ulong.MinValue,
                testFieldUShort = ushort.MaxValue,
                //testFieldUShortArray = new ushort[] { 11, 12, 13, 14, 15 },
                //testFieldUShortArrayNullable = new ushort?[] { 11, 12, 13, null, 14, 15 },
                testFieldUShortNullable = ushort.MinValue,
                //testFielLongArrayNullable = new long?[] { 500, 600, 700, null, 999, 1000 },
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

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime testFieldDateTime { get; set; }

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

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime? testFieldDateTimeNullable { get; set; }

            public Guid? testFieldGuidNullable { get; set; }

            public TableAllTypeEnumType1 testFieldEnum1 { get; set; }
            public TableAllTypeEnumType1? testFieldEnum1Nullable { get; set; }
            public TableAllTypeEnumType2 testFieldEnum2 { get; set; }
            public TableAllTypeEnumType2? testFieldEnum2Nullable { get; set; }

            /* array */
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
