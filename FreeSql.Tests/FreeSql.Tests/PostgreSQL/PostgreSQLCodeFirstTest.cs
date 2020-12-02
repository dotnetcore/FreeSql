using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Npgsql.LegacyPostgis;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Xunit;

namespace FreeSql.Tests.PostgreSQL
{
    public class PostgreSQLCodeFirstTest
    {
        [Fact]
        public void InsertUpdateParameter()
        {
            var fsql = g.pgsql;
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
        public void DateTime_1()
        {
            var item1 = new TS_DATETIME01 { CreateTime = DateTime.Now };
            Assert.Equal(1, g.pgsql.Insert(item1).ExecuteAffrows());

            var item2 = g.pgsql.Select<TS_DATETIME01>().Where(a => a.Id == item1.Id).First();
            Assert.NotNull(item2.CreateTime);
            Assert.True(1 > Math.Abs(item2.CreateTime.Value.Subtract(item1.CreateTime.Value).TotalSeconds));

            item1.CreateTime = DateTime.Now;
            Assert.Equal(1, g.pgsql.Update<TS_DATETIME01>().SetSource(item1).ExecuteAffrows());
            item2 = g.pgsql.Select<TS_DATETIME01>().Where(a => a.Id == item1.Id).First();
            Assert.NotNull(item2.CreateTime);
            Assert.True(1 > Math.Abs(item2.CreateTime.Value.Subtract(item1.CreateTime.Value).TotalSeconds));
        }
        class TS_DATETIME01
        {
            public Guid Id { get; set; }
            [Column(DbType = "timestamp NULL")]
            public DateTime? CreateTime { get; set; }
        }
        [Fact]
        public void DateTime_2()
        {
            var item1 = new TS_DATETIME02 { CreateTime = DateTime.Now };
            Assert.Equal(1, g.pgsql.Insert(item1).ExecuteAffrows());

            var item2 = g.pgsql.Select<TS_DATETIME02>().Where(a => a.Id == item1.Id).First();
            Assert.NotNull(item2.CreateTime);
            Assert.True(1 > Math.Abs(item2.CreateTime.Value.Subtract(item1.CreateTime.Value).TotalSeconds));

            item1.CreateTime = DateTime.Now;
            Assert.Equal(1, g.pgsql.Update<TS_DATETIME02>().SetSource(item1).ExecuteAffrows());
            item2 = g.pgsql.Select<TS_DATETIME02>().Where(a => a.Id == item1.Id).First();
            Assert.NotNull(item2.CreateTime);
            Assert.True(1 > Math.Abs(item2.CreateTime.Value.Subtract(item1.CreateTime.Value).TotalSeconds));
        }
        class TS_DATETIME02
        {
            public Guid Id { get; set; }
            [Column(DbType = "timestamp NOT NULL")]
            public DateTime? CreateTime { get; set; }
        }

        [Fact]
        public void Blob()
        {
            var str1 = string.Join(",", Enumerable.Range(0, 10000).Select(a => "我是中国人"));
            var data1 = Encoding.UTF8.GetBytes(str1);

            var item1 = new TS_BLB01 { Data = data1 };
            Assert.Equal(1, g.pgsql.Insert(item1).ExecuteAffrows());

            var item2 = g.pgsql.Select<TS_BLB01>().Where(a => a.Id == item1.Id).First();
            Assert.Equal(item1.Data.Length, item2.Data.Length);

            var str2 = Encoding.UTF8.GetString(item2.Data);
            Assert.Equal(str1, str2);

            //NoneParameter
            item1 = new TS_BLB01 { Data = data1 };
            Assert.Equal(1, g.pgsql.Insert<TS_BLB01>().NoneParameter().AppendData(item1).ExecuteAffrows());

            item2 = g.pgsql.Select<TS_BLB01>().Where(a => a.Id == item1.Id).First();
            Assert.Equal(item1.Data.Length, item2.Data.Length);

            str2 = Encoding.UTF8.GetString(item2.Data);
            Assert.Equal(str1, str2);
        }
        class TS_BLB01
        {
            public Guid Id { get; set; }
            [MaxLength(-1)]
            public byte[] Data { get; set; }
        }

        [Fact]
        public void StringLength()
        {
            var dll = g.pgsql.CodeFirst.GetComparisonDDLStatements<TS_SLTB>();
            g.pgsql.CodeFirst.SyncStructure<TS_SLTB>();
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
            var sql = g.pgsql.CodeFirst.GetComparisonDDLStatements<测试中文表>();
            g.pgsql.CodeFirst.SyncStructure<测试中文表>();

            var item = new 测试中文表
            {
                标题 = "测试标题",
                创建时间 = DateTime.Now
            };
            Assert.Equal(1, g.pgsql.Insert<测试中文表>().AppendData(item).ExecuteAffrows());
            Assert.NotEqual(Guid.Empty, item.编号);
            var item2 = g.pgsql.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新";
            Assert.Equal(1, g.pgsql.Update<测试中文表>().SetSource(item).ExecuteAffrows());
            item2 = g.pgsql.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo";
            var repo = g.pgsql.GetRepository<测试中文表>();
            Assert.Equal(1, repo.Update(item));
            item2 = g.pgsql.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo22";
            Assert.Equal(1, repo.Update(item));
            item2 = g.pgsql.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
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
            var sql = g.pgsql.CodeFirst.GetComparisonDDLStatements<AddUniquesInfo>();
            g.pgsql.CodeFirst.SyncStructure<AddUniquesInfo>();
            g.pgsql.CodeFirst.SyncStructure(typeof(AddUniquesInfo), "AddUniquesInfo1");
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
            var sql = g.pgsql.CodeFirst.GetComparisonDDLStatements<TopicAddField>();
            Assert.True(string.IsNullOrEmpty(sql)); //测试运行两次后
            g.pgsql.Select<TopicAddField>();
            var id = g.pgsql.Insert<TopicAddField>().AppendData(new TopicAddField { }).ExecuteIdentity();
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

            var sql = g.pgsql.CodeFirst.GetComparisonDDLStatements<TableAllType>();
            g.pgsql.Select<TableAllType>();
        }

        IInsert<TableAllType> insert => g.pgsql.Insert<TableAllType>();
        ISelect<TableAllType> select => g.pgsql.Select<TableAllType>();

        [Fact]
        public void CurdAllField()
        {
            NpgsqlConnection.GlobalTypeMapper.UseLegacyPostgis();

            var sql1 = select.Where(a => a.testFieldIntArray.Contains(1)).ToSql();
            var sql2 = select.Where(a => a.testFieldIntArray.Contains(1)).ToSql();

            var item = new TableAllType { };
            item.Id = (int)insert.AppendData(item).ExecuteIdentity();

            var newitem = select.Where(a => a.Id == item.Id).ToOne();

            var item2 = new TableAllType
            {
                testFieldBitArray = new BitArray(Encoding.UTF8.GetBytes("我是")),
                testFieldBitArrayArray = new[] { new BitArray(Encoding.UTF8.GetBytes("中国")), new BitArray(Encoding.UTF8.GetBytes("公民")) },
                testFieldBool = true,
                testFieldBoolArray = new[] { true, true, false, false },
                testFieldBoolArrayNullable = new bool?[] { true, true, null, false, false },
                testFieldBoolNullable = true,
                testFieldByte = byte.MaxValue,
                testFieldByteArray = new byte[] { 0, 1, 2, 3, 4, 5, 6 },
                testFieldByteArrayNullable = new byte?[] { 0, 1, 2, 3, null, 4, 5, 6 },
                testFieldByteNullable = byte.MinValue,
                testFieldBytes = Encoding.UTF8.GetBytes("我是中国人"),
                testFieldBytesArray = new[] { Encoding.UTF8.GetBytes("我是中国人"), Encoding.UTF8.GetBytes("我是中国人") },
                testFieldCidr = (IPAddress.Parse("10.0.0.0"), 8),
                testFieldCidrArray = new[] { (IPAddress.Parse("10.0.0.0"), 8), (IPAddress.Parse("192.168.0.0"), 16) },
                testFieldCidrArrayNullable = new (IPAddress, int)?[] { (IPAddress.Parse("10.0.0.0"), 8), null, (IPAddress.Parse("192.168.0.0"), 16) },
                testFieldCidrNullable = (IPAddress.Parse("192.168.0.0"), 16),
                testFieldDateTime = DateTime.Now,
                testFieldDateTimeArray = new[] { DateTime.Now, DateTime.Now.AddHours(2) },
                testFieldDateTimeArrayNullable = new DateTime?[] { DateTime.Now, null, DateTime.Now.AddHours(2) },
                testFieldDateTimeNullable = DateTime.Now.AddDays(-1),
                testFieldDecimal = 999.99M,
                testFieldDecimalArray = new[] { 999.91M, 999.92M, 999.93M },
                testFieldDecimalArrayNullable = new decimal?[] { 998.11M, 998.12M, 998.13M },
                testFieldDecimalNullable = 111.11M,
                testFieldDouble = 888.88,
                testFieldDoubleArray = new[] { 888.81, 888.82, 888.83 },
                testFieldDoubleArrayNullable = new double?[] { 888.11, 888.12, null, 888.13 },
                testFieldDoubleNullable = 222.22,
                testFieldEnum1 = TableAllTypeEnumType1.e3,
                testFieldEnum1Array = new[] { TableAllTypeEnumType1.e5, TableAllTypeEnumType1.e2, TableAllTypeEnumType1.e1 },
                testFieldEnum1ArrayNullable = new TableAllTypeEnumType1?[] { TableAllTypeEnumType1.e5, TableAllTypeEnumType1.e2, null, TableAllTypeEnumType1.e1 },
                testFieldEnum1Nullable = TableAllTypeEnumType1.e2,
                testFieldEnum2 = TableAllTypeEnumType2.f2,
                testFieldEnum2Array = new[] { TableAllTypeEnumType2.f3, TableAllTypeEnumType2.f1 },
                testFieldEnum2ArrayNullable = new TableAllTypeEnumType2?[] { TableAllTypeEnumType2.f3, null, TableAllTypeEnumType2.f1 },
                testFieldEnum2Nullable = TableAllTypeEnumType2.f3,
                testFieldFloat = 777.77F,
                testFieldFloatArray = new[] { 777.71F, 777.72F, 777.73F },
                testFieldFloatArrayNullable = new float?[] { 777.71F, 777.72F, null, 777.73F },
                testFieldFloatNullable = 333.33F,
                testFieldGuid = Guid.NewGuid(),
                testFieldGuidArray = new[] { Guid.NewGuid(), Guid.NewGuid() },
                testFieldGuidArrayNullable = new Guid?[] { Guid.NewGuid(), null, Guid.NewGuid() },
                testFieldGuidNullable = Guid.NewGuid(),
                testFieldHStore = new Dictionary<string, string> { { "111", "value111" }, { "222", "value222" }, { "333", "value333" } },
                testFieldHStoreArray = new[] { new Dictionary<string, string> { { "111", "value111" }, { "222", "value222" }, { "333", "value333" } }, new Dictionary<string, string> { { "444", "value444" }, { "555", "value555" }, { "666", "value666" } } },
                testFieldInet = IPAddress.Parse("192.168.1.1"),
                testFieldInetArray = new[] { IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.2"), IPAddress.Parse("192.168.1.3") },
                testFieldInt = int.MaxValue,
                testFieldInt4range = new NpgsqlRange<int>(10, 20),
                testFieldInt4rangeArray = new[] { new NpgsqlRange<int>(10, 20), new NpgsqlRange<int>(50, 100), new NpgsqlRange<int>(200, 300) },
                testFieldInt4rangeArrayNullable = new NpgsqlRange<int>?[] { new NpgsqlRange<int>(10, 20), new NpgsqlRange<int>(50, 100), null, new NpgsqlRange<int>(200, 300) },
                testFieldInt4rangeNullable = new NpgsqlRange<int>(100, 200),
                testFieldInt8range = new NpgsqlRange<long>(100, 200),
                testFieldInt8rangeArray = new[] { new NpgsqlRange<long>(100, 200), new NpgsqlRange<long>(500, 1000), new NpgsqlRange<long>(2000, 3000) },
                testFieldInt8rangeArrayNullable = new NpgsqlRange<long>?[] { new NpgsqlRange<long>(100, 200), new NpgsqlRange<long>(500, 1000), null, new NpgsqlRange<long>(2000, 3000) },
                testFieldInt8rangeNullable = new NpgsqlRange<long>(1000, 2000),
                testFieldIntArray = new[] { 1, 2, 3, 4, 5 },
                testFieldIntArrayNullable = new int?[] { 1, 2, 3, null, 4, 5 },
                testFieldIntNullable = int.MinValue,
                testFieldJArray = JArray.Parse("[1,2,3,4,5]"),
                testFieldJArrayArray = new[] { JArray.Parse("[1,2,3,4,5]"), JArray.Parse("[10,20,30,40,50]") },
                testFieldJObject = JObject.Parse("{ \"a\":1, \"b\":2, \"c\":3 }"),
                testFieldJObjectArray = new[] { JObject.Parse("{ \"a\":1, \"b\":2, \"c\":3 }"), JObject.Parse("{ \"a\":10, \"b\":20, \"c\":30 }") },
                testFieldJToken = JToken.Parse("{ \"a\":1, \"b\":2, \"c\":3, \"d\":[1,2,3,4,5] }"),
                testFieldJTokenArray = new[] { JToken.Parse("{ \"a\":1, \"b\":2, \"c\":3, \"d\":[1,2,3,4,5] }"), JToken.Parse("{ \"a\":10, \"b\":20, \"c\":30, \"d\":[10,20,30,40,50] }") },
                testFieldLong = long.MaxValue,
                testFieldLongArray = new long[] { 10, 20, 30, 40, 50 },
                testFieldMacaddr = PhysicalAddress.Parse("A1-A2-CD-DD-FF-02"),
                testFieldMacaddrArray = new[] { PhysicalAddress.Parse("A1-A2-CD-DD-FF-02"), PhysicalAddress.Parse("A2-22-22-22-22-02") },
                testFieldNpgsqlBox = new NpgsqlBox(10, 100, 100, 10),
                testFieldNpgsqlBoxArray = new[] { new NpgsqlBox(10, 100, 100, 10), new NpgsqlBox(200, 2000, 2000, 200) },
                testFieldNpgsqlBoxArrayNullable = new NpgsqlBox?[] { new NpgsqlBox(10, 100, 100, 10), null, new NpgsqlBox(200, 2000, 2000, 200) },
                testFieldNpgsqlBoxNullable = new NpgsqlBox(200, 2000, 2000, 200),
                testFieldNpgsqlCircle = new NpgsqlCircle(50, 50, 100),
                testFieldNpgsqlCircleArray = new[] { new NpgsqlCircle(50, 50, 100), new NpgsqlCircle(80, 80, 100) },
                testFieldNpgsqlCircleArrayNullable = new NpgsqlCircle?[] { new NpgsqlCircle(50, 50, 100), null, new NpgsqlCircle(80, 80, 100) },
                testFieldNpgsqlCircleNullable = new NpgsqlCircle(80, 80, 100),
                testFieldNpgsqlLine = new NpgsqlLine(30, 30, 30),
                testFieldNpgsqlLineArray = new[] { new NpgsqlLine(30, 30, 30), new NpgsqlLine(35, 35, 35) },
                testFieldNpgsqlLineArrayNullable = new NpgsqlLine?[] { new NpgsqlLine(30, 30, 30), null, new NpgsqlLine(35, 35, 35) },
                testFieldNpgsqlLineNullable = new NpgsqlLine(60, 60, 60),
                testFieldNpgsqlLSeg = new NpgsqlLSeg(80, 10, 800, 20),
                testFieldNpgsqlLSegArray = new[] { new NpgsqlLSeg(80, 10, 800, 20), new NpgsqlLSeg(180, 20, 260, 50) },
                testFieldNpgsqlLSegArrayNullable = new NpgsqlLSeg?[] { new NpgsqlLSeg(80, 10, 800, 20), null, new NpgsqlLSeg(180, 20, 260, 50) },
                testFieldNpgsqlLSegNullable = new NpgsqlLSeg(180, 20, 260, 50),
                testFieldNpgsqlPath = new NpgsqlPath(new NpgsqlPoint(10, 10), new NpgsqlPoint(15, 10), new NpgsqlPoint(17, 10), new NpgsqlPoint(19, 10)),
                testFieldNpgsqlPathArray = new[] { new NpgsqlPath(new NpgsqlPoint(10, 10), new NpgsqlPoint(15, 10), new NpgsqlPoint(17, 10), new NpgsqlPoint(19, 10)), new NpgsqlPath(new NpgsqlPoint(210, 10), new NpgsqlPoint(215, 10), new NpgsqlPoint(217, 10), new NpgsqlPoint(219, 10)) },
                testFieldNpgsqlPathArrayNullable = new NpgsqlPath?[] { new NpgsqlPath(new NpgsqlPoint(10, 10), new NpgsqlPoint(15, 10), new NpgsqlPoint(17, 10), new NpgsqlPoint(19, 10)), null, new NpgsqlPath(new NpgsqlPoint(210, 10), new NpgsqlPoint(215, 10), new NpgsqlPoint(217, 10), new NpgsqlPoint(219, 10)) },
                testFieldNpgsqlPathNullable = new NpgsqlPath(new NpgsqlPoint(210, 10), new NpgsqlPoint(215, 10), new NpgsqlPoint(217, 10), new NpgsqlPoint(219, 10)),
                testFieldNpgsqlPoint = new NpgsqlPoint(666, 666),
                testFieldNpgsqlPointArray = new[] { new NpgsqlPoint(666, 666), new NpgsqlPoint(888, 888) },
                testFieldNpgsqlPointArrayNullable = new NpgsqlPoint?[] { new NpgsqlPoint(666, 666), null, new NpgsqlPoint(888, 888) },
                testFieldNpgsqlPointNullable = new NpgsqlPoint(888, 888),
                testFieldNpgsqlPolygon = new NpgsqlPolygon(new NpgsqlPoint(36, 30), new NpgsqlPoint(36, 50), new NpgsqlPoint(38, 80), new NpgsqlPoint(36, 30)),
                testFieldNpgsqlPolygonArray = new[] { new NpgsqlPolygon(new NpgsqlPoint(36, 30), new NpgsqlPoint(36, 50), new NpgsqlPoint(38, 80), new NpgsqlPoint(36, 30)), new NpgsqlPolygon(new NpgsqlPoint(136, 130), new NpgsqlPoint(136, 150), new NpgsqlPoint(138, 180), new NpgsqlPoint(136, 130)) },
                testFieldNpgsqlPolygonArrayNullable = new NpgsqlPolygon?[] { new NpgsqlPolygon(new NpgsqlPoint(36, 30), new NpgsqlPoint(36, 50), new NpgsqlPoint(38, 80), new NpgsqlPoint(36, 30)), null, new NpgsqlPolygon(new NpgsqlPoint(136, 130), new NpgsqlPoint(136, 150), new NpgsqlPoint(138, 180), new NpgsqlPoint(136, 130)) },
                testFieldNpgsqlPolygonNullable = new NpgsqlPolygon(new NpgsqlPoint(136, 130), new NpgsqlPoint(136, 150), new NpgsqlPoint(138, 180), new NpgsqlPoint(136, 130)),
                testFieldNumrange = new NpgsqlRange<decimal>(888.88M, 999.99M),
                testFieldNumrangeArray = new[] { new NpgsqlRange<decimal>(888.88M, 999.99M), new NpgsqlRange<decimal>(18888.88M, 19998.99M) },
                testFieldNumrangeArrayNullable = new NpgsqlRange<decimal>?[] { new NpgsqlRange<decimal>(888.88M, 999.99M), null, new NpgsqlRange<decimal>(18888.88M, 19998.99M) },
                testFieldNumrangeNullable = new NpgsqlRange<decimal>(18888.88M, 19998.99M),
                testFieldPostgisGeometry = new PostgisLineString(new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110) }),
                testFieldPostgisGeometryArray = new PostgisGeometry[] {
                    new PostgisPoint(555,551),
                    new PostgisLineString(new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110) }),
                    new PostgisPolygon(new []{ new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110), new Coordinate2D(300, 310), new Coordinate2D(10, 11) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } }),
                    new PostgisMultiLineString(new[] { new PostgisLineString(new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110) }) , new PostgisLineString(new[] { new Coordinate2D(20, 21), new Coordinate2D(200, 210) }) }),
                    new PostgisMultiPoint(new[] { new Coordinate2D(20, 21), new Coordinate2D(210, 220) }),
                    new PostgisMultiPolygon(new []{
                        new PostgisPolygon(new []{ new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110), new Coordinate2D(300, 310), new Coordinate2D(10, 11) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } }),
                        new PostgisPolygon(new []{ new[] { new Coordinate2D(50, 51), new Coordinate2D(500, 510), new Coordinate2D(800, 810), new Coordinate2D(50, 51) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } })
                    })
                },
                testFieldPostgisGeometryCollection = new PostgisGeometryCollection(new PostgisGeometry[] {
                    new PostgisPoint(555,551),
                    new PostgisLineString(new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110) }),
                    new PostgisPolygon(new []{ new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110), new Coordinate2D(300, 310), new Coordinate2D(10, 11) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } }),
                    new PostgisMultiLineString(new[] { new PostgisLineString(new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110) }) , new PostgisLineString(new[] { new Coordinate2D(20, 21), new Coordinate2D(200, 210) }) }),
                    new PostgisMultiPoint(new[] { new Coordinate2D(20, 21), new Coordinate2D(210, 220) }),
                    new PostgisMultiPolygon(new []{
                        new PostgisPolygon(new []{ new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110), new Coordinate2D(300, 310), new Coordinate2D(10, 11) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } }),
                        new PostgisPolygon(new []{ new[] { new Coordinate2D(50, 51), new Coordinate2D(500, 510), new Coordinate2D(800, 810), new Coordinate2D(50, 51) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } })
                    })
                }),
                testFieldPostgisGeometryCollectionArray = new[] {
                      new PostgisGeometryCollection(new PostgisGeometry[] {
                        new PostgisPoint(555,551),
                        new PostgisLineString(new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110) }),
                        new PostgisPolygon(new []{ new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110), new Coordinate2D(300, 310), new Coordinate2D(10, 11) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } }),
                        new PostgisMultiLineString(new[] { new PostgisLineString(new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110) }) , new PostgisLineString(new[] { new Coordinate2D(20, 21), new Coordinate2D(200, 210) }) }),
                        new PostgisMultiPoint(new[] { new Coordinate2D(20, 21), new Coordinate2D(210, 220) }),
                        new PostgisMultiPolygon(new []{
                            new PostgisPolygon(new []{ new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110), new Coordinate2D(300, 310), new Coordinate2D(10, 11) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } }),
                            new PostgisPolygon(new []{ new[] { new Coordinate2D(50, 51), new Coordinate2D(500, 510), new Coordinate2D(800, 810), new Coordinate2D(50, 51) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } })
                        })
                    }),new PostgisGeometryCollection(new PostgisGeometry[] {
                        new PostgisPoint(555,551),
                        new PostgisLineString(new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110) }),
                        new PostgisPolygon(new []{ new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110), new Coordinate2D(300, 310), new Coordinate2D(10, 11) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } }),
                        new PostgisMultiLineString(new[] { new PostgisLineString(new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110) }) , new PostgisLineString(new[] { new Coordinate2D(20, 21), new Coordinate2D(200, 210) }) }),
                        new PostgisMultiPoint(new[] { new Coordinate2D(20, 21), new Coordinate2D(210, 220) }),
                        new PostgisMultiPolygon(new []{
                            new PostgisPolygon(new []{ new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110), new Coordinate2D(300, 310), new Coordinate2D(10, 11) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } }),
                            new PostgisPolygon(new []{ new[] { new Coordinate2D(50, 51), new Coordinate2D(500, 510), new Coordinate2D(800, 810), new Coordinate2D(50, 51) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } })
                        })
                    })
                },
                testFieldPostgisLineString = new PostgisLineString(new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110) }),
                testFieldPostgisLineStringArray = new[] { new PostgisLineString(new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110) }), new PostgisLineString(new[] { new Coordinate2D(20, 21), new Coordinate2D(200, 220) }) },
                testFieldPostgisMultiPoint = new PostgisMultiPoint(new[] { new Coordinate2D(20, 21), new Coordinate2D(210, 220) }),
                testFieldPostgisMultiPointArray = new[] { new PostgisMultiPoint(new[] { new Coordinate2D(20, 21), new Coordinate2D(210, 220) }), new PostgisMultiPoint(new[] { new Coordinate2D(120, 121), new Coordinate2D(1210, 1220) }) },
                testFieldPostgisPoint = new PostgisPoint(555, 551),
                testFieldPostgisPointArray = new[] { new PostgisPoint(555, 551), new PostgisPoint(53355, 3551) },
                testFieldPostgisPolygon = new PostgisPolygon(new[] { new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110), new Coordinate2D(300, 310), new Coordinate2D(10, 11) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } }),
                testFieldPostgisPolygonArray = new[]{
                    new PostgisPolygon(new []{ new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110), new Coordinate2D(300, 310), new Coordinate2D(10, 11) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } }),
                    new PostgisPolygon(new []{ new[] { new Coordinate2D(50, 51), new Coordinate2D(500, 510), new Coordinate2D(800, 810), new Coordinate2D(50, 51) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } })
                },
                testFieldPostgisPostgisMultiLineString = new PostgisMultiLineString(new[] { new PostgisLineString(new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110) }), new PostgisLineString(new[] { new Coordinate2D(20, 21), new Coordinate2D(200, 210) }) }),
                testFieldPostgisPostgisMultiLineStringArray = new[] {
                    new PostgisMultiLineString(new[] { new PostgisLineString(new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110) }) , new PostgisLineString(new[] { new Coordinate2D(20, 21), new Coordinate2D(200, 210) }) }),
                    new PostgisMultiLineString(new[] { new PostgisLineString(new[] { new Coordinate2D(20, 21), new Coordinate2D(200, 220) }) , new PostgisLineString(new[] { new Coordinate2D(820, 821), new Coordinate2D(800, 810) }) })
                },
                testFieldPostgisPostgisMultiPolygon = new PostgisMultiPolygon(new[]{
                    new PostgisPolygon(new []{ new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110), new Coordinate2D(300, 310), new Coordinate2D(10, 11) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } }),
                    new PostgisPolygon(new []{ new[] { new Coordinate2D(50, 51), new Coordinate2D(500, 510), new Coordinate2D(800, 810), new Coordinate2D(50, 51) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } })
                }),
                testFieldPostgisPostgisMultiPolygonArray = new[] {
                    new PostgisMultiPolygon(new[]{
                        new PostgisPolygon(new []{ new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110), new Coordinate2D(300, 310), new Coordinate2D(10, 11) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } }),
                        new PostgisPolygon(new []{ new[] { new Coordinate2D(50, 51), new Coordinate2D(500, 510), new Coordinate2D(800, 810), new Coordinate2D(50, 51) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } })
                    }),
                    new PostgisMultiPolygon(new[]{
                        new PostgisPolygon(new []{ new[] { new Coordinate2D(10, 11), new Coordinate2D(100, 110), new Coordinate2D(300, 310), new Coordinate2D(10, 11) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } }),
                        new PostgisPolygon(new []{ new[] { new Coordinate2D(50, 51), new Coordinate2D(500, 510), new Coordinate2D(800, 810), new Coordinate2D(50, 51) }, new[] { new Coordinate2D(20, 11), new Coordinate2D(200, 110), new Coordinate2D(100, 310), new Coordinate2D(20, 11) } })
                    })
                },
                testFieldSByte = sbyte.MaxValue,
                testFieldSByteArray = new sbyte[] { 1, 2, 3, 4, 5 },
                testFieldSByteArrayNullable = new sbyte?[] { 1, 2, 3, null, 4, 5 },
                testFieldSByteNullable = sbyte.MinValue,
                testFieldShort = short.MaxValue,
                testFieldShortArray = new short[] { 1, 2, 3, 4, 5 },
                testFieldShortArrayNullable = new short?[] { 1, 2, 3, null, 4, 5 },
                testFieldShortNullable = short.MinValue,
                testFieldString = "我是中国人string'\\?!@#$%^&*()_+{}}{~?><<>",
                testFieldChar = 'X',
                testFieldStringArray = new[] { "我是中国人String1", "我是中国人String2", null, "我是中国人String3" },
                testFieldTimeSpan = TimeSpan.FromDays(1),
                testFieldTimeSpanArray = new[] { TimeSpan.FromDays(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60) },
                testFieldTimeSpanArrayNullable = new TimeSpan?[] { TimeSpan.FromDays(1), TimeSpan.FromSeconds(10), null, TimeSpan.FromSeconds(60) },
                testFieldTimeSpanNullable = TimeSpan.FromSeconds(90),
                testFieldTsrange = new NpgsqlRange<DateTime>(DateTime.Now, DateTime.Now.AddMonths(1)),
                testFieldTsrangeArray = new[] { new NpgsqlRange<DateTime>(DateTime.Now, DateTime.Now.AddMonths(1)), new NpgsqlRange<DateTime>(DateTime.Now, DateTime.Now.AddMonths(2)) },
                testFieldTsrangeArrayNullable = new NpgsqlRange<DateTime>?[] { new NpgsqlRange<DateTime>(DateTime.Now, DateTime.Now.AddMonths(1)), null, new NpgsqlRange<DateTime>(DateTime.Now, DateTime.Now.AddMonths(2)) },
                testFieldTsrangeNullable = new NpgsqlRange<DateTime>(DateTime.Now, DateTime.Now.AddMonths(2)),
                testFieldUInt = uint.MaxValue,
                testFieldUIntArray = new uint[] { 1, 2, 3, 4, 5 },
                testFieldUIntArrayNullable = new uint?[] { 1, 2, 3, null, 4, 5 },
                testFieldUIntNullable = uint.MinValue,
                testFieldULong = ulong.MaxValue,
                testFieldULongArray = new ulong[] { 10, 20, 30, 40, 50 },
                testFieldULongArrayNullable = new ulong?[] { 10, 20, 30, null, 40, 50 },
                testFieldULongNullable = ulong.MinValue,
                testFieldUShort = ushort.MaxValue,
                testFieldUShortArray = new ushort[] { 11, 12, 13, 14, 15 },
                testFieldUShortArrayNullable = new ushort?[] { 11, 12, 13, null, 14, 15 },
                testFieldUShortNullable = ushort.MinValue,
                testFielLongArrayNullable = new long?[] { 500, 600, 700, null, 999, 1000 },
                testFielLongNullable = long.MinValue
            };

            var sqlPar = insert.AppendData(item2).ToSql();
            var sqlText = insert.AppendData(item2).NoneParameter().ToSql();
            var item3NP = insert.AppendData(item2).NoneParameter().ExecuteInserted();

            var item3 = insert.AppendData(item2).ExecuteInserted().First();
            var newitem2 = select.Where(a => a.Id == item3.Id && object.Equals(a.testFieldJToken["a"], "1")).ToOne();
            Assert.Equal(item2.testFieldString, newitem2.testFieldString);
            Assert.Equal(item2.testFieldChar, newitem2.testFieldChar);

            item3 = insert.NoneParameter().AppendData(item2).ExecuteInserted().First();
            newitem2 = select.Where(a => a.Id == item3.Id && object.Equals(a.testFieldJToken["a"], "1")).ToOne();
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
            public NpgsqlPoint testFieldNpgsqlPoint { get; set; }
            public NpgsqlLine testFieldNpgsqlLine { get; set; }
            public NpgsqlLSeg testFieldNpgsqlLSeg { get; set; }
            public NpgsqlBox testFieldNpgsqlBox { get; set; }
            public NpgsqlPath testFieldNpgsqlPath { get; set; }
            public NpgsqlPolygon testFieldNpgsqlPolygon { get; set; }
            public NpgsqlCircle testFieldNpgsqlCircle { get; set; }
            public (IPAddress Address, int Subnet) testFieldCidr { get; set; }
            public NpgsqlRange<int> testFieldInt4range { get; set; }
            public NpgsqlRange<long> testFieldInt8range { get; set; }
            public NpgsqlRange<decimal> testFieldNumrange { get; set; }
            public NpgsqlRange<DateTime> testFieldTsrange { get; set; }

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
            public NpgsqlPoint? testFieldNpgsqlPointNullable { get; set; }
            public NpgsqlLine? testFieldNpgsqlLineNullable { get; set; }
            public NpgsqlLSeg? testFieldNpgsqlLSegNullable { get; set; }
            public NpgsqlBox? testFieldNpgsqlBoxNullable { get; set; }
            public NpgsqlPath? testFieldNpgsqlPathNullable { get; set; }
            public NpgsqlPolygon? testFieldNpgsqlPolygonNullable { get; set; }
            public NpgsqlCircle? testFieldNpgsqlCircleNullable { get; set; }
            public (IPAddress Address, int Subnet)? testFieldCidrNullable { get; set; }
            public NpgsqlRange<int>? testFieldInt4rangeNullable { get; set; }
            public NpgsqlRange<long>? testFieldInt8rangeNullable { get; set; }
            public NpgsqlRange<decimal>? testFieldNumrangeNullable { get; set; }
            public NpgsqlRange<DateTime>? testFieldTsrangeNullable { get; set; }

            public BitArray testFieldBitArray { get; set; }
            public IPAddress testFieldInet { get; set; }
            public PhysicalAddress testFieldMacaddr { get; set; }
            public JToken testFieldJToken { get; set; }
            public JObject testFieldJObject { get; set; }
            public JArray testFieldJArray { get; set; }
            public Dictionary<string, string> testFieldHStore { get; set; }
            public PostgisPoint testFieldPostgisPoint { get; set; }
            public PostgisLineString testFieldPostgisLineString { get; set; }
            public PostgisPolygon testFieldPostgisPolygon { get; set; }
            public PostgisMultiPoint testFieldPostgisMultiPoint { get; set; }
            public PostgisMultiLineString testFieldPostgisPostgisMultiLineString { get; set; }
            public PostgisMultiPolygon testFieldPostgisPostgisMultiPolygon { get; set; }
            public PostgisGeometry testFieldPostgisGeometry { get; set; }
            public PostgisGeometryCollection testFieldPostgisGeometryCollection { get; set; }

            public TableAllTypeEnumType1 testFieldEnum1 { get; set; }
            public TableAllTypeEnumType1? testFieldEnum1Nullable { get; set; }
            public TableAllTypeEnumType2 testFieldEnum2 { get; set; }
            public TableAllTypeEnumType2? testFieldEnum2Nullable { get; set; }

            /* array */
            public bool[] testFieldBoolArray { get; set; }
            public sbyte[] testFieldSByteArray { get; set; }
            public short[] testFieldShortArray { get; set; }
            public int[] testFieldIntArray { get; set; }
            public long[] testFieldLongArray { get; set; }
            public byte[] testFieldByteArray { get; set; }
            public ushort[] testFieldUShortArray { get; set; }
            public uint[] testFieldUIntArray { get; set; }
            public ulong[] testFieldULongArray { get; set; }
            public double[] testFieldDoubleArray { get; set; }
            public float[] testFieldFloatArray { get; set; }
            public decimal[] testFieldDecimalArray { get; set; }
            public TimeSpan[] testFieldTimeSpanArray { get; set; }
            public DateTime[] testFieldDateTimeArray { get; set; }
            public byte[][] testFieldBytesArray { get; set; }
            public string[] testFieldStringArray { get; set; }
            public Guid[] testFieldGuidArray { get; set; }
            public NpgsqlPoint[] testFieldNpgsqlPointArray { get; set; }
            public NpgsqlLine[] testFieldNpgsqlLineArray { get; set; }
            public NpgsqlLSeg[] testFieldNpgsqlLSegArray { get; set; }
            public NpgsqlBox[] testFieldNpgsqlBoxArray { get; set; }
            public NpgsqlPath[] testFieldNpgsqlPathArray { get; set; }
            public NpgsqlPolygon[] testFieldNpgsqlPolygonArray { get; set; }
            public NpgsqlCircle[] testFieldNpgsqlCircleArray { get; set; }
            public (IPAddress Address, int Subnet)[] testFieldCidrArray { get; set; }
            public NpgsqlRange<int>[] testFieldInt4rangeArray { get; set; }
            public NpgsqlRange<long>[] testFieldInt8rangeArray { get; set; }
            public NpgsqlRange<decimal>[] testFieldNumrangeArray { get; set; }
            public NpgsqlRange<DateTime>[] testFieldTsrangeArray { get; set; }

            public bool?[] testFieldBoolArrayNullable { get; set; }
            public sbyte?[] testFieldSByteArrayNullable { get; set; }
            public short?[] testFieldShortArrayNullable { get; set; }
            public int?[] testFieldIntArrayNullable { get; set; }
            public long?[] testFielLongArrayNullable { get; set; }
            public byte?[] testFieldByteArrayNullable { get; set; }
            public ushort?[] testFieldUShortArrayNullable { get; set; }
            public uint?[] testFieldUIntArrayNullable { get; set; }
            public ulong?[] testFieldULongArrayNullable { get; set; }
            public double?[] testFieldDoubleArrayNullable { get; set; }
            public float?[] testFieldFloatArrayNullable { get; set; }
            public decimal?[] testFieldDecimalArrayNullable { get; set; }
            public TimeSpan?[] testFieldTimeSpanArrayNullable { get; set; }
            public DateTime?[] testFieldDateTimeArrayNullable { get; set; }
            public Guid?[] testFieldGuidArrayNullable { get; set; }
            public NpgsqlPoint?[] testFieldNpgsqlPointArrayNullable { get; set; }
            public NpgsqlLine?[] testFieldNpgsqlLineArrayNullable { get; set; }
            public NpgsqlLSeg?[] testFieldNpgsqlLSegArrayNullable { get; set; }
            public NpgsqlBox?[] testFieldNpgsqlBoxArrayNullable { get; set; }
            public NpgsqlPath?[] testFieldNpgsqlPathArrayNullable { get; set; }
            public NpgsqlPolygon?[] testFieldNpgsqlPolygonArrayNullable { get; set; }
            public NpgsqlCircle?[] testFieldNpgsqlCircleArrayNullable { get; set; }
            public (IPAddress Address, int Subnet)?[] testFieldCidrArrayNullable { get; set; }
            public NpgsqlRange<int>?[] testFieldInt4rangeArrayNullable { get; set; }
            public NpgsqlRange<long>?[] testFieldInt8rangeArrayNullable { get; set; }
            public NpgsqlRange<decimal>?[] testFieldNumrangeArrayNullable { get; set; }
            public NpgsqlRange<DateTime>?[] testFieldTsrangeArrayNullable { get; set; }

            public BitArray[] testFieldBitArrayArray { get; set; }
            public IPAddress[] testFieldInetArray { get; set; }
            public PhysicalAddress[] testFieldMacaddrArray { get; set; }
            public JToken[] testFieldJTokenArray { get; set; }
            public JObject[] testFieldJObjectArray { get; set; }
            public JArray[] testFieldJArrayArray { get; set; }
            public Dictionary<string, string>[] testFieldHStoreArray { get; set; }
            public PostgisPoint[] testFieldPostgisPointArray { get; set; }
            public PostgisLineString[] testFieldPostgisLineStringArray { get; set; }
            public PostgisPolygon[] testFieldPostgisPolygonArray { get; set; }
            public PostgisMultiPoint[] testFieldPostgisMultiPointArray { get; set; }
            public PostgisMultiLineString[] testFieldPostgisPostgisMultiLineStringArray { get; set; }
            public PostgisMultiPolygon[] testFieldPostgisPostgisMultiPolygonArray { get; set; }
            public PostgisGeometry[] testFieldPostgisGeometryArray { get; set; }
            public PostgisGeometryCollection[] testFieldPostgisGeometryCollectionArray { get; set; }

            public TableAllTypeEnumType1[] testFieldEnum1Array { get; set; }
            public TableAllTypeEnumType1?[] testFieldEnum1ArrayNullable { get; set; }
            public TableAllTypeEnumType2[] testFieldEnum2Array { get; set; }
            public TableAllTypeEnumType2?[] testFieldEnum2ArrayNullable { get; set; }
        }

        public enum TableAllTypeEnumType1 { e1, e2, e3, e5 }
        [Flags] public enum TableAllTypeEnumType2 { f1, f2, f3 }
    }
}
