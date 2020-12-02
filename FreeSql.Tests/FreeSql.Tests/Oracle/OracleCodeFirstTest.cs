using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.Oracle
{
    public class OracleCodeFirstTest
    {
        [Fact]
        public void InsertUpdateParameter()
        {
            var fsql = g.oracle;
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
        public void StringNullToEmpty()
        {
            using (var fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Oracle, "user id=1user;password=123456;data source=//127.0.0.1:1521/XE;Pooling=true;Max Pool Size=5;min pool size=1")
                .UseAutoSyncStructure(true)
                //.UseGenerateCommandParameterWithLambda(true)
                .UseLazyLoading(true)
                .UseNameConvert(FreeSql.Internal.NameConvertType.ToUpper)
                //.UseNoneCommandParameter(true)

                .UseMonitorCommand(
                    cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
                    //, (cmd, traceLog) => Console.WriteLine(traceLog)
                    )
                .Build())
            {
                var repo = fsql.GetRepository<TS_SL361, long>();

                var item1 = new TS_SL361 { CreatorId = "" };
                repo.Insert(item1);
                var item2 = repo.Get(item1.Id);

                Assert.Null(item2.CreatorId);

                fsql.Aop.AuditDataReader += (_, e) =>
                {
                    if (e.DataReader.GetFieldType(e.Index) == typeof(string) && e.Value == DBNull.Value)
                        e.Value = "";
                };

                item1 = new TS_SL361 { CreatorId = "" };
                repo.Insert(item1);
                item2 = repo.Get(item1.Id);

                Assert.Equal(item1.CreatorId, item2.CreatorId);

                fsql.Aop.AuditDataReader -= fsql.Aop.AuditDataReaderHandler;

                item1 = new TS_SL361 { CreatorId = "" };
                repo.Insert(item1);
                item2 = repo.Get(item1.Id);

                Assert.Null(item2.CreatorId);
            }
        }
        class TS_SNTE
        {
            [Column(IsIdentity = true)]
            public long Id { get; set; }
            public string CreatorId { get; set; }
        }

        [Fact]
        public void StringLength36()
        {
            var repo = g.oracle.GetRepository<TS_SL361, long>();

            var item1 = new TS_SL361 { CreatorId = "xxx '123 " };
            repo.Insert(item1);
            var item2 = repo.Get(item1.Id);

            Assert.Equal(item1.CreatorId, item2.CreatorId);
        }
        class TS_SL361
        {
            [Column(IsIdentity = true)]
            public long Id { get; set; }
            [Column(StringLength = 36)]
            public string CreatorId { get; set; }
        }

        [Fact]
        public void NClob_StringLength_1()
        {
            var str1 = string.Join(",", Enumerable.Range(0, 10000).Select(a => "我是中国人"));

            var item1 = new TS_NCLB02 { Data = str1 };
            Assert.Equal(1, g.oracle.Insert(item1).ExecuteAffrows());

            var item2 = g.oracle.Select<TS_NCLB02>().Where(a => a.Id == item1.Id).First();
            Assert.Equal(str1, item2.Data);

            //NoneParameter
            item1 = new TS_NCLB02 { Data = str1 };
            Assert.Equal(1, g.oracle.Insert(item1).NoneParameter().ExecuteAffrows());
            //Oracle.ManagedDataAccess.Client.OracleException:“ORA-01704: 字符串文字太长”

            item2 = g.oracle.Select<TS_NCLB02>().Where(a => a.Id == item1.Id).First();
            Assert.Equal(str1, item2.Data);
        }
        class TS_NCLB02
        {
            public Guid Id { get; set; }
            [Column(StringLength = - 1)]
            public string Data { get; set; }
        }

        [Fact]
        public void NClob()
        {
            var str1 = string.Join(",", Enumerable.Range(0, 10000).Select(a => "我是中国人"));

            var item1 = new TS_NCLB01 { Data = str1 };
            Assert.Equal(1, g.oracle.Insert(item1).ExecuteAffrows());

            var item2 = g.oracle.Select<TS_NCLB01>().Where(a => a.Id == item1.Id).First();
            Assert.Equal(str1, item2.Data);

            //NoneParameter
            item1 = new TS_NCLB01 { Data = str1 };
            Assert.Equal(1, g.oracle.Insert(item1).NoneParameter().ExecuteAffrows());
            //Oracle.ManagedDataAccess.Client.OracleException:“ORA-01704: 字符串文字太长”

            item2 = g.oracle.Select<TS_NCLB01>().Where(a => a.Id == item1.Id).First();
            Assert.Equal(str1, item2.Data);
        }
        class TS_NCLB01
        {
            public Guid Id { get; set; }
            [Column(DbType = "nclob")]
            public string Data { get; set; }
        }
        [Fact]
        public void Clob()
        {
            var str1 = string.Join(",", Enumerable.Range(0, 10000).Select(a => "我是中国人"));

            var item1 = new TS_CLB01 { Data = str1 };
            Assert.Equal(1, g.oracle.Insert(item1).ExecuteAffrows());

            var item2 = g.oracle.Select<TS_CLB01>().Where(a => a.Id == item1.Id).First();
            Assert.Equal(str1, item2.Data);

            //NoneParameter
            item1 = new TS_CLB01 { Data = str1 };
            Assert.Equal(1, g.oracle.Insert(item1).NoneParameter().ExecuteAffrows());
            //Oracle.ManagedDataAccess.Client.OracleException:“ORA-01704: 字符串文字太长”

            item2 = g.oracle.Select<TS_CLB01>().Where(a => a.Id == item1.Id).First();
            Assert.Equal(str1, item2.Data);
        }
        class TS_CLB01
        {
            public Guid Id { get; set; }
            [Column(DbType = "clob")]
            public string Data { get; set; }
        }
        [Fact]
        public void Blob()
        {
            var str1 = string.Join(",", Enumerable.Range(0, 10000).Select(a => "我是中国人"));
            var data1 = Encoding.UTF8.GetBytes(str1);

            var item1 = new TS_BLB01 { Data = data1 };
            Assert.Equal(1, g.oracle.Insert(item1).ExecuteAffrows());

            var item2 = g.oracle.Select<TS_BLB01>().Where(a => a.Id == item1.Id).First();
            Assert.Equal(item1.Data.Length, item2.Data.Length);

            var str2 = Encoding.UTF8.GetString(item2.Data);
            Assert.Equal(str1, str2);

            //NoneParameter
            item1 = new TS_BLB01 { Data = data1 };
            Assert.Equal(1, g.oracle.Insert(item1).NoneParameter().ExecuteAffrows());
            //Oracle.ManagedDataAccess.Client.OracleException:“ORA-01704: 字符串文字太长”

            item2 = g.oracle.Select<TS_BLB01>().Where(a => a.Id == item1.Id).First();
            Assert.Equal(item1.Data.Length, item2.Data.Length);

            str2 = Encoding.UTF8.GetString(item2.Data);
            Assert.Equal(str1, str2);

            Assert.Equal(1, g.oracle.InsertOrUpdate<TS_BLB01>().SetSource(new TS_BLB01 { Data = data1 }).ExecuteAffrows());
            item2 = g.oracle.Select<TS_BLB01>().Where(a => a.Id == item1.Id).First();
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
            var dll = g.oracle.CodeFirst.GetComparisonDDLStatements<TS_SLTB>();
            g.oracle.CodeFirst.SyncStructure<TS_SLTB>();
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
            var sql = g.oracle.CodeFirst.GetComparisonDDLStatements<测试数字表>();
            g.oracle.CodeFirst.SyncStructure<测试数字表>();

            var item = new 测试数字表
            {
                标题 = "测试标题",
                创建时间 = DateTime.Now
            };
            Assert.Equal(1, g.oracle.Insert<测试数字表>().AppendData(item).ExecuteAffrows());
            Assert.NotEqual(Guid.Empty, item.编号);
            var item2 = g.oracle.Select<测试数字表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新";
            Assert.Equal(1, g.oracle.Update<测试数字表>().SetSource(item).ExecuteAffrows());
            item2 = g.oracle.Select<测试数字表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo";
            var repo = g.oracle.GetRepository<测试数字表>();
            Assert.Equal(1, repo.Update(item));
            item2 = g.oracle.Select<测试数字表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo22";
            Assert.Equal(1, repo.Update(item));
            item2 = g.oracle.Select<测试数字表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);
        }
        [Table(Name = "123tb")]
        [OraclePrimaryKeyName("pk1_123tb")]
        class 测试数字表
        {
            [Column(IsPrimary = true, Name = "123id")]
            public Guid 编号 { get; set; }

            [Column(Name = "123title")]
            public string 标题 { get; set; }

            [Column(Name = "123time")]
            public DateTime 创建时间 { get; set; }
        }

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

            item.标题 = "测试标题更新";
            Assert.Equal(1, g.oracle.Update<测试中文表>().SetSource(item).ExecuteAffrows());
            item2 = g.oracle.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo";
            var repo = g.oracle.GetRepository<测试中文表>();
            Assert.Equal(1, repo.Update(item));
            item2 = g.oracle.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo22";
            Assert.Equal(1, repo.Update(item));
            item2 = g.oracle.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
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
            var sql = g.oracle.CodeFirst.GetComparisonDDLStatements<AddUniquesInfo>();
            g.oracle.CodeFirst.SyncStructure<AddUniquesInfo>();
            //g.oracle.CodeFirst.SyncStructure(typeof(AddUniquesInfo), "AddUniquesInfo1");
        }
        [Table(Name = "AddUniquesInfo", OldName = "AddUniquesInfo2")]
        [Index("uk_phone", "phone", true)]
        [Index("uk_group_index", "group,index", true)]
        [Index("uk_group_index22", "group, index22 desc", true)]
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
            Assert.True(string.IsNullOrEmpty(sql)); //测试运行两次后
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

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime DateTime { get; set; }
            [Column(ServerTime = DateTimeKind.Local)]
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

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime? DateTimeNullable { get; set; }
            [Column(ServerTime = DateTimeKind.Local)]
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
