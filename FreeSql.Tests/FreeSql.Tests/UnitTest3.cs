using FreeSql.DataAnnotations;
using FreeSql;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using Npgsql.LegacyPostgis;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Data.SqlClient;
using kwlib;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Net;
using System.Collections;

namespace FreeSql.Tests
{
    public class UnitTest3
    {

        public class Song23
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        public class Author23
        {
            public long Id { get; set; }
            public long SongId { get; set; }
            public string Name { get; set; }
        }

        public class TestDbContext : DbContext
        {
            public TestDbContext(IFreeSql orm) : base(orm, null)
            {
            }
            public DbSet<Song23> Songs { get; set; }
            public DbSet<Author23> Authors { get; set; }
        }

        /// <summary>
        /// 父级
        /// </summary>
        public class BaseModel
        {
            [Column(IsPrimary = true)]
            public string ID { get; set; }

            /// <summary>
            /// 创建人
            /// </summary>
            public string UserID { get; set; } = "Admin";

            /// <summary>
            /// 创建时间
            /// </summary>
            [Column(ServerTime = DateTimeKind.Utc)]
            public DateTime CreateTime { get; set; }

            /// <summary>
            /// 备注
            /// </summary>
            public string Description { get; set; }
        }
        public class Menu : BaseModel
        {
            public string SubNameID { get; set; }

            /// <summary>
            /// 菜单名称
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 英文名称
            /// </summary>
            public string EnName { get; set; }

            /// <summary>
            /// 链接地址
            /// </summary>
            public string Url { get; set; }

            /// <summary>
            /// 父级菜单 一级为 0
            /// </summary>
            public string ParentID { get; set; }

            /// <summary>
            /// 按钮操作 逗号分隔
            /// </summary>
            public string OperationIds { get; set; }

            /// <summary>
            /// 导航属性
            /// </summary>
            public virtual Menu Parent { get; set; }


            [Column(IsIgnore = true)]
            public string OperationNames { get; set; }

            [Column(IsIgnore = true)]
            public string SystemName { get; set; }

        }
        class SubSystem
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public interface EdiInterface
        {
            List<EdiItem> Children { get; set; }
        }

        [Table(Name = "EDI")]
        public class Edi : EdiInterface
        {
            [Column(Name = "EDI_ID")] public long Id { get; set; }

            public List<EdiItem> Children { get; set; }
        }
        [Table(Name = "EDI_ITEM")]
        public class EdiItem
        {
            [Column(Name = "EDII_ID")] public long Id { get; set; }
            [Column(Name = "EDII_EDI_ID")] public long EdiId { get; set; }
        }

        public class Song123
        {
            public long Id { get; set; }
            protected Song123() { }
            public Song123(long id) => Id = id;
        }
        public class Author123
        {
            public long Id { get; set; }
            public long SongId { get; set; }
            public Author123(long id, long songId)
            {
                Id = id;
                SongId = songId;
            }
        }

        [Fact]
        public void Test03()
        {
            var aff1 = g.sqlite.GetRepository<Edi, long>().Delete(10086);
            var aff2 = g.sqlite.Delete<Edi>(10086).ExecuteAffrows();
            Assert.Equal(aff1, aff2);


            var testStringFormat = g.sqlite.Select<Edi>().First(a => new {
                str = $"x{a.Id}_{DateTime.Now.ToString("yyyyMM")}z",
                str2 = string.Format("{0}x{0}_{1}z", a.Id, DateTime.Now.ToString("yyyyMM"))
            });



            var sql123 = g.sqlserver.Select<Edi>()

                .WithSql(
                    g.sqlserver.Select<Edi>().ToSql(a => new { a.Id }, FieldAliasOptions.AsProperty) + 
                    " UNION ALL " +
                    g.sqlserver.Select<Edi>().ToSql(a => new { a.Id }, FieldAliasOptions.AsProperty))
                
                .Page(1, 10).ToSql("Id");

            var sqlextMax1 = g.mysql.Select<EdiItem>()
                .GroupBy(a => a.Id)
                .ToSql(a => new
                {
                    Id = a.Key, EdiId = SqlExt.Max(a.Key).Over().ToValue()
                });

            var sqlextGroupConcat = g.mysql.Select<Edi, EdiItem>()
                .InnerJoin((a, b) => b.Id == a.Id)
                .ToSql((a, b) => new
                {
                    Id = a.Id,
                    EdiId = b.Id,
                    case1 = SqlExt.Case()
                        .When(a.Id == 1, 10)
                        .When(a.Id == 2, 11)
                        .When(a.Id == 3, 12)
                        .When(a.Id == 4, 13)
                        .When(a.Id == 5, SqlExt.Case().When(b.Id == 1, 10000).Else(999).End())
                        .End(),
                    groupct1 = SqlExt.GroupConcat(a.Id).Distinct().OrderBy(b.EdiId).Separator("_").ToValue()
                });
            var sqlextGroupConcatToList = g.mysql.Select<Edi, EdiItem>()
                .InnerJoin((a, b) => b.Id == a.Id)
                .ToList((a, b) => new
                {
                    Id = a.Id,
                    EdiId = b.Id,
                    case1 = SqlExt.Case()
                        .When(a.Id == 1, 10)
                        .When(a.Id == 2, 11)
                        .When(a.Id == 3, 12)
                        .When(a.Id == 4, 13)
                        .When(a.Id == 5, SqlExt.Case().When(b.Id == 1, 10000).Else(999).End())
                        .End(),
                    groupct1 = SqlExt.GroupConcat(a.Id).Distinct().OrderBy(b.EdiId).Separator("_").ToValue()
                });

            var sqlextCase = g.sqlserver.Select<Edi, EdiItem>()
                .InnerJoin((a, b) => b.Id == a.Id)
                .ToSql((a, b) => new
                {
                    Id = a.Id,
                    EdiId = b.Id,
                    case1 = SqlExt.Case()
                        .When(a.Id == 1, 10)
                        .When(a.Id == 2, 11)
                        .When(a.Id == 3, 12)
                        .When(a.Id == 4, 13)
                        .When(a.Id == 5, SqlExt.Case().When(b.Id == 1, 10000).Else(999).End())
                        .End(),
                    over1 = SqlExt.Rank().Over().OrderBy(a.Id).OrderByDescending(b.EdiId).ToValue(),
                });
            var sqlextCaseToList = g.sqlserver.Select<Edi, EdiItem>()
                .InnerJoin((a, b) => b.Id == a.Id)
                .ToList((a, b) => new
                {
                    Id = a.Id,
                    EdiId = b.Id,
                    case1 = SqlExt.Case()
                        .When(a.Id == 1, 10)
                        .When(a.Id == 2, 11)
                        .When(a.Id == 3, 12)
                        .When(a.Id == 4, 13)
                        .When(a.Id == 5, SqlExt.Case().When(b.Id == 1, 10000).Else(999).End())
                        .End(),
                    over1 = SqlExt.Rank().Over().OrderBy(a.Id).OrderByDescending(b.EdiId).ToValue(),
                });


            var sqlextOver = g.sqlserver.Select<Edi, EdiItem>()
                .InnerJoin((a, b) => b.Id == a.Id)
                .ToSql((a, b) => new
                {
                    Id = a.Id,
                    EdiId = b.Id,
                    over1 = SqlExt.Rank().Over().OrderBy(a.Id).OrderByDescending(b.EdiId).ToValue()
                });
            var sqlextOverToList = g.sqlserver.Select<Edi, EdiItem>()
                .InnerJoin((a, b) => b.Id == a.Id)
                .ToList((a, b) => new
                {
                    Id = a.Id,
                    EdiId = b.Id,
                    over1 = SqlExt.Rank().Over().OrderBy(a.Id).OrderByDescending(b.EdiId).ToValue()
                });

            var tttrule = 8;
            var tttid = new long[] { 18, 19, 4017 };
            g.sqlserver.Update<Author123>().Set(it => it.SongId == (short)(it.SongId & ~tttrule)).Where(it => (it.SongId & tttrule) == tttrule && !tttid.Contains(it.Id)).ExecuteAffrows();

            g.sqlite.Delete<Song123>().Where("1=1").ExecuteAffrows();
            g.sqlite.Delete<Author123>().Where("1=1").ExecuteAffrows();
            g.sqlite.Insert(new Song123(1)).ExecuteAffrows();
            g.sqlite.Insert(new Author123(11, 1)).ExecuteAffrows();
            var song = g.sqlite.Select<Song123>()
                .From<Author123>((a, b) => a.InnerJoin(a1 => a1.Id == b.SongId))
                .First((a, b) => a); // throw error
            Console.WriteLine(song == null);

            g.sqlite.Select<Edi>().ToList();

            var itemId2 = 2;
            var itemId = 1;
            var edi = g.sqlite.Select<Edi>()
                .Where(a => a.Id == itemId2 && g.sqlite.Select<EdiItem>().Where(b => b.Id == itemId).Any())
                .First(a => a); //#231

            var lksdjkg1 = g.sqlite.Select<Edi>()
                .AsQueryable().Where(a => a.Id > 0).Where(a => a.Id == 1).ToList();

            var lksdjkg11 = g.sqlite.Select<Edi>()
               .AsQueryable().Where(a => a.Id > 0).Where(a => a.Id == 1).Any();

            var lksdjkg2 = g.sqlite.Select<Edi>()
                .AsQueryable().Where(a => a.Id > 0).First();

            var lksdjkg3 = g.sqlite.Select<Edi>()
                .AsQueryable().Where(a => a.Id > 0).FirstOrDefault();


            var sql222efe = g.sqlite.Select<Edi, EdiItem>()
                .InnerJoin((a, b) => b.Id == g.sqlite.Select<EdiItem>().As("c").Where(c => c.EdiId == a.Id).OrderBy(c => c.Id).ToOne(c => c.Id))
                .ToSql((a, b) => new
                {
                    Id = a.Id,
                    EdiId = b.Id
                });

            var subSyetemId = "xxx";
            var list = g.sqlite.Select<Menu, SubSystem>()
                .LeftJoin((a,b) => a.SubNameID == b.Id)
                .WhereIf(!string.IsNullOrEmpty(subSyetemId), (a, s) => a.SubNameID == subSyetemId)
                .ToList((a, s) => new Menu
                {
                    ID = a.ID,
                    SystemName = s.Name,
                    SubNameID = s.Id,
                    CreateTime = a.CreateTime,
                    Description = a.Description,
                    EnName = a.EnName,
                    Name = a.Name,
                    OperationIds = a.OperationIds,
                    Parent = a.Parent,
                    ParentID = a.ParentID,
                    Url = a.Url,
                    UserID = a.UserID
                });



            var context = new TestDbContext(g.sqlite);

            var sql = context.Songs
                .Where(a =>
                    context.Authors
                        //.Select  //加上这句就不报错，不加上报 variable 'a' of type 'Song' referenced from scope '', but it is not defined
                        .Where(b => b.SongId == a.Id)
                        .Any())
                .ToSql(a => a.Name);

            sql = context.Songs
                .Where(a =>
                    context.Authors
                        .Select  //加上这句就不报错，不加上报 variable 'a' of type 'Song' referenced from scope '', but it is not defined
                        .Where(b => b.SongId == a.Id)
                        .Any())
                .ToSql(a => a.Name);

            //using (var conn = new SqlConnection("Data Source=.;Integrated Security=True;Initial Catalog=webchat-abc;Pooling=true;Max Pool Size=13"))
            //{
            //    conn.Open();
            //    conn.Close();
            //}

            //using (var fsql = new FreeSql.FreeSqlBuilder()
            //    .UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=webchat-abc;Pooling=true;Max Pool Size=13")
            //    .UseAutoSyncStructure(true)
            //    //.UseGenerateCommandParameterWithLambda(true)
            //    .UseMonitorCommand(
            //        cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
            //        //, (cmd, traceLog) => Console.WriteLine(traceLog)
            //        )
            //    .UseLazyLoading(true)
            //    .Build())
            //{
            //    fsql.Select<ut3_t1>().ToList();
            //}

            //var testByte = new TestByte { pic = File.ReadAllBytes(@"C:\Users\28810\Desktop\71500003-0ad69400-289e-11ea-85cb-36a54f52ebc0.png") };
            //var sql = g.sqlserver.Insert(testByte).NoneParameter().ToSql();
            //g.sqlserver.Insert(testByte).NoneParameter().ExecuteAffrows();

            //var getTestByte = g.sqlserver.Select<TestByte>(testByte).First();

            //File.WriteAllBytes(@"C:\Users\28810\Desktop\71500003-0ad69400-289e-11ea-85cb-36a54f52ebc0_write.png", getTestByte.pic);

            var ib = new IdleBus<IFreeSql>(TimeSpan.FromMinutes(10));
            ib.Notice += (_, e2) => Trace.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] 线程{Thread.CurrentThread.ManagedThreadId}：{e2.Log}");

            ib.Register("db1", () => new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=3")
                .UseAutoSyncStructure(true)
                .UseGenerateCommandParameterWithLambda(true)
                .UseMonitorCommand(cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText))
                .UseLazyLoading(true)
                .Build());
            ib.Register("db2", () => new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Oracle, "user id=user1;password=123456;data source=//127.0.0.1:1521/XE;Pooling=true;Max Pool Size=3")
                .UseAutoSyncStructure(true)
                .UseGenerateCommandParameterWithLambda(true)
                .UseLazyLoading(true)
                .UseNameConvert(FreeSql.Internal.NameConvertType.ToUpper)
                .UseMonitorCommand(cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText))
                .Build());
            ib.Register("db3", () => new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document.db;Attachs=xxxtb.db;Pooling=true;Max Pool Size=3")
                .UseAutoSyncStructure(true)
                .UseGenerateCommandParameterWithLambda(true)
                .UseLazyLoading(true)
                .UseMonitorCommand(cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText))
                .Build());
            //...注入很多个

            var fsql = ib.Get("db1"); //使用的时候用 Get 方法，不要存其引用关系
            var sqlparamId = 100;
            fsql.Select<ut3_t1>().Limit(10).Where(a => a.id == sqlparamId).ToList();

            fsql = ib.Get("db2");
            fsql.Select<ut3_t1>().Limit(10).Where(a => a.id == sqlparamId).ToList();

            fsql = ib.Get("db3");
            fsql.Select<ut3_t1>().Limit(10).Where(a => a.id == sqlparamId).ToList();

            fsql = g.sqlserver;
            fsql.Insert<OrderMain>(new OrderMain { OrderNo = "1001", OrderTime = new DateTime(2019, 12, 01) }).ExecuteAffrows();
            fsql.Insert<OrderDetail>(new OrderDetail { OrderNo = "1001", ItemNo = "I001", Qty = 1 }).ExecuteAffrows();
            fsql.Insert<OrderDetail>(new OrderDetail { OrderNo = "1001", ItemNo = "I002", Qty = 1 }).ExecuteAffrows();
            fsql.Insert<OrderDetail>(new OrderDetail { OrderNo = "1001", ItemNo = "I003", Qty = 1 }).ExecuteAffrows();
            fsql.Insert<OrderMain>(new OrderMain { OrderNo = "1002", OrderTime = new DateTime(2019, 12, 02) }).ExecuteAffrows();
            fsql.Insert<OrderDetail>(new OrderDetail { OrderNo = "1002", ItemNo = "I011", Qty = 1 }).ExecuteAffrows();
            fsql.Insert<OrderDetail>(new OrderDetail { OrderNo = "1002", ItemNo = "I012", Qty = 1 }).ExecuteAffrows();
            fsql.Insert<OrderDetail>(new OrderDetail { OrderNo = "1002", ItemNo = "I013", Qty = 1 }).ExecuteAffrows();
            fsql.Ado.Query<object>("select * from orderdetail left join ordermain on orderdetail.orderno=ordermain.orderno where ordermain.orderno='1001'");


            g.oracle.Delete<SendInfo>().Where("1=1").ExecuteAffrows();
            g.oracle.Insert(new[]
                {
                    new SendInfo{ Code = "001", Binary = Encoding.UTF8.GetBytes("我是中国人") },
                    new SendInfo{ Code = "002", Binary = Encoding.UTF8.GetBytes("我是地球人") },
                    new SendInfo{ Code = "003", Binary = Encoding.UTF8.GetBytes("我是.net")},
                    new SendInfo{ Code = "004", Binary = Encoding.UTF8.GetBytes("我是freesql") },
                    new SendInfo{ Code = "005", Binary = Encoding.UTF8.GetBytes("我是freesql233") },
                })
                .NoneParameter()
                .BatchOptions(3, 200)
                .BatchProgress(a => Trace.WriteLine($"{a.Current}/{a.Total}"))
                .ExecuteAffrows();

            var slslsl = g.oracle.Select<SendInfo>().ToList();

            var slsls1Ids = slslsl.Select(a => a.ID).ToArray();
            var slslss2 = g.oracle.Select<SendInfo>().Where(a => slsls1Ids.Contains(a.ID)).ToList();

            var mt_codeId = Guid.Parse("2f48c5ca-7257-43c8-9ee2-0e16fa990253");
            Assert.Equal(1, g.oracle.Insert(new SendInfo { ID = mt_codeId, Code = "mt_code", Binary = Encoding.UTF8.GetBytes("我是mt_code") })
                .ExecuteAffrows());
            var mt_code = g.oracle.Select<SendInfo>().Where(a => a.ID == mt_codeId).First();
            Assert.NotNull(mt_code);
            Assert.Equal(mt_codeId, mt_code.ID);
            Assert.Equal("mt_code", mt_code.Code);

            mt_code = g.oracle.Select<SendInfo>().Where(a => a.ID == Guid.Parse("2f48c5ca725743c89ee20e16fa990253".ToUpper())).First();
            Assert.NotNull(mt_code);
            Assert.Equal(mt_codeId, mt_code.ID);
            Assert.Equal("mt_code", mt_code.Code);

            mt_codeId = Guid.Parse("2f48c5ca-7257-43c8-9ee2-0e16fa990251");
            Assert.Equal(1, g.oracle.Insert(new SendInfo { ID = mt_codeId, Code = "mt_code2", Binary = Encoding.UTF8.GetBytes("我是mt_code2") })
                .NoneParameter()
                .ExecuteAffrows());
            mt_code = g.oracle.Select<SendInfo>().Where(a => a.ID == mt_codeId).First();
            Assert.NotNull(mt_code);
            Assert.Equal(mt_codeId, mt_code.ID);
            Assert.Equal("mt_code2", mt_code.Code);

            mt_code = g.oracle.Select<SendInfo>().Where(a => a.ID == Guid.Parse("2f48c5ca725743c89ee20e16fa990251".ToUpper())).First();
            Assert.NotNull(mt_code);
            Assert.Equal(mt_codeId, mt_code.ID);
            Assert.Equal("mt_code2", mt_code.Code);

            var id = g.oracle.Insert(new TestORC12()).ExecuteIdentity();
        }

        class TestORC12
        {
            [Column(IsIdentity = true, InsertValueSql = "\"TAG_SEQ_ID\".nextval")]
            public int Id { get; set; }
        }

        [Table(Name = "t_text")]
        public class SendInfo
        {
            [Column(IsPrimary = true, DbType = "raw(16)", MapType = typeof(byte[]))]
            public Guid ID { get; set; }

            [Column(Name = "YPID5")]
            public string Code { get; set; }
        
            public byte[] Binary { get; set; }

            [Column(ServerTime = DateTimeKind.Utc, CanUpdate = false)]
            public DateTime 创建时间 { get; set; }

            [Column(ServerTime = DateTimeKind.Utc)]
            public DateTime 更新时间 { get; set; }

            [Column(InsertValueSql = "'123'")]
            public string InsertValue2 { get; set; }
        }

        class TestByte
        {
            public Guid id { get; set; }

            [Column(DbType = "varbinary(max)")]
            public byte[] pic { get; set; }
        }

        class ut3_t1
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string name { get; set; }
        }
        class ut3_t2
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string name { get; set; }
        }

        public class OrderMain
        {
            public string OrderNo { get; set; }
            public DateTime OrderTime { get; set; }
            public decimal Amount { get; set; }
        }
        public class OrderDetail
        {
            public string OrderNo { get; set; }
            public string ItemNo { get; set; }
            public decimal Qty { get; set; }
        }
    }

}


