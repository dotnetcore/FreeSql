using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace FreeSql.Tests.KingbaseES
{
    public class KingbaseESCodeFirstTest
    {
        [Fact]
        public void InsertUpdateParameter()
        {
            var fsql = g.kingbaseES;
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
            var dll = g.kingbaseES.CodeFirst.GetComparisonDDLStatements<TS_SLTB>();
            g.kingbaseES.CodeFirst.SyncStructure<TS_SLTB>();
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
        public void 数字表_字段()
        {
            var sql = g.kingbaseES.CodeFirst.GetComparisonDDLStatements<测试数字表>();
            g.kingbaseES.CodeFirst.SyncStructure<测试数字表>();

            var item = new 测试数字表
            {
                标题 = "测试标题",
                创建时间 = DateTime.Now
            };
            Assert.Equal(1, g.kingbaseES.Insert<测试数字表>().AppendData(item).ExecuteAffrows());
            Assert.NotEqual(Guid.Empty, item.编号);
            var item2 = g.kingbaseES.Select<测试数字表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新";
            Assert.Equal(1, g.kingbaseES.Update<测试数字表>().SetSource(item).ExecuteAffrows());
            item2 = g.kingbaseES.Select<测试数字表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo";
            var repo = g.kingbaseES.GetRepository<测试数字表>();
            Assert.Equal(1, repo.Update(item));
            item2 = g.kingbaseES.Select<测试数字表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo22";
            Assert.Equal(1, repo.Update(item));
            item2 = g.kingbaseES.Select<测试数字表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);
        }
        [Table(Name = "123测试数字表")]
        class 测试数字表
        {
            [Column(IsPrimary = true, Name = "123编号")]
            public Guid 编号 { get; set; }

            [Column(Name = "123标题")]
            public string 标题 { get; set; }

            [Column(Name = "123创建时间")]
            public DateTime 创建时间 { get; set; }
        }

        [Fact]
        public void 中文表_字段()
        {
            var sql = g.kingbaseES.CodeFirst.GetComparisonDDLStatements<测试中文表>();
            g.kingbaseES.CodeFirst.SyncStructure<测试中文表>();

            var item = new 测试中文表
            {
                标题 = "测试标题",
                创建时间 = DateTime.Now
            };
            Assert.Equal(1, g.kingbaseES.Insert<测试中文表>().AppendData(item).ExecuteAffrows());
            Assert.NotEqual(Guid.Empty, item.编号);
            var item2 = g.kingbaseES.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新";
            Assert.Equal(1, g.kingbaseES.Update<测试中文表>().SetSource(item).ExecuteAffrows());
            item2 = g.kingbaseES.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo";
            var repo = g.kingbaseES.GetRepository<测试中文表>();
            Assert.Equal(1, repo.Update(item));
            item2 = g.kingbaseES.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo22";
            Assert.Equal(1, repo.Update(item));
            item2 = g.kingbaseES.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
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
            var sql = g.kingbaseES.CodeFirst.GetComparisonDDLStatements<AddUniquesInfo>();
            g.kingbaseES.CodeFirst.SyncStructure<AddUniquesInfo>();
            g.kingbaseES.CodeFirst.SyncStructure(typeof(AddUniquesInfo), "AddUniquesInfo1");
        }
        [Table(Name = "AddUniquesInfo", OldName = "AddUniquesInfo2")]
        [Index("{tablename}_uk_phone", "phone", true)]
        [Index("{tablename}_uk_group_index", "group,index", true)]
        [Index("{tablename}_uk_group_index22", "group, index22", true)]
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
            var sql = g.kingbaseES.CodeFirst.GetComparisonDDLStatements<TopicAddField>();

            var id = g.kingbaseES.Insert<TopicAddField>().AppendData(new TopicAddField { }).ExecuteIdentity();

            //var inserted = g.kingbaseES.Insert<TopicAddField>().AppendData(new TopicAddField { }).ExecuteInserted();
        }

        [Table(Name = "TopicAddField", OldName = "xxxtb.TopicAddField")]
        public class TopicAddField
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }

            public string name { get; set; }

            [Column(DbType = "varchar2(200) not null", OldName = "title")]
            public string title2 { get; set; } = "10";

            [Column(IsIgnore = true)]
            public DateTime ct { get; set; } = DateTime.Now;
        }

        [Fact]
        public void GetComparisonDDLStatements()
        {
            var sql = g.kingbaseES.CodeFirst.GetComparisonDDLStatements<TableAllType>();
            Assert.True(string.IsNullOrEmpty(sql)); //测试运行两次后
            //sql = g.kingbaseES.CodeFirst.GetComparisonDDLStatements<Tb_alltype>();
        }

        IInsert<TableAllType> insert => g.kingbaseES.Insert<TableAllType>();
        ISelect<TableAllType> select => g.kingbaseES.Select<TableAllType>();

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
                String = "我是中国人string'\\?!@#$%^&*()_+{}}{~?><<>",
                Char = 'X',
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
            var sqlPar = insert.AppendData(item2).ToSql();
            var sqlText = insert.AppendData(item2).NoneParameter().ToSql();
            var item3NP = insert.AppendData(item2).NoneParameter().ExecuteIdentity();

            item2.Id = (int)insert.AppendData(item2).ExecuteIdentity();
            var newitem21 = select.Where(a => a.Id == item2.Id).First(a => new
            {
                a.Id,
                a.id2,
                a.SByte,
                a.Short,
                a.Int,
                a.Long,
                a.Byte,
                a.UShort,
                a.UInt,
                a.ULong,
                a.Double,
                a.Float,
                a.Decimal,
                a.TimeSpan,
                a.DateTimeOffSet,
                a.Bytes,
                a.String,
                a.Char,
                a.Guid
            });
            var newitem22 = select.Where(a => a.Id == item2.Id).First(a => new
            {
                a.Id, a.id2, a.SByte, a.Short, a.Int, a.Long, a.Byte, a.UShort, a.UInt, a.ULong, a.Double, a.Float, a.Decimal, a.TimeSpan, a.DateTime, a.DateTimeOffSet, a.Bytes, a.String, a.Char, a.Guid
            });

            var newitem2 = select.Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item2.String, newitem2.String);
            Assert.Equal(item2.Char, newitem2.Char);

            item2.Id = (int)insert.NoneParameter().AppendData(item2).ExecuteIdentity();
            newitem2 = select.Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item2.String, newitem2.String);
            Assert.Equal(item2.Char, newitem2.Char);

            var items = select.ToList();
            var itemstb = select.ToDataTable();
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
            public char Char { get; set; }
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
