using Densen.Models.ids;
using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Extensions;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.Odbc.Default;
using Microsoft.Data.SqlClient;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace base_entity
{
    static class Program
    {
        class TestConfig
        {
            public int clicks { get; set; }
            public string title { get; set; }
        }
        [Table(Name = "sysconfig")]
        public class S_SysConfig<T> : BaseEntity<S_SysConfig<T>>
        {
            [Column(IsPrimary = true)]
            public string Name { get; set; }

            [JsonMap]
            public T Config { get; set; }

            public T Config2 { get; set; }
        }

        public class Products : BaseEntity<Products, int>
        {
            public string title { get; set; }
            public int testint { get; set; }
        }

        static AsyncLocal<IUnitOfWork> _asyncUow = new AsyncLocal<IUnitOfWork>();

        public class TestEnumCls
        {
            public CollationTypeEnum val { get; set; } = CollationTypeEnum.Binary;
        }

        class Sys_reg_user
        {
            public Guid Id { get; set; }
            public Guid OwnerId { get; set; }
            public string UnionId { get; set; }

            [Navigate(nameof(OwnerId))]
            public Sys_owner Owner { get; set; }
        }
        class Sys_owner
        {
            public Guid Id { get; set; }
            public Guid RegUserId { get; set; }

            [Navigate(nameof(RegUserId))]
            public Sys_reg_user RegUser { get; set; }
        }

        public class tttorder
        {
            [Column(IsPrimary = true)]
            public long Id { get; set; }
            public string Title { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }


            public tttorder(string title, int quantity, decimal price)
            {
                Id = DateTime.Now.Ticks;
                Title = title;
                Quantity = quantity;
                Price = price;
            }
        }

        class B
        {
            public long Id { get; set; }
        }

        class A
        {
            public long BId { get; set; }
            public B B { get; set; }
        }

        [Table(Name = "as_table_log_{yyyyMMddHH}", AsTable = "createtime=2022-1-1 11(1 month)")]
        class AsTableLog
        {
            public Guid id { get; set; }
            public string msg { get; set; }
            public DateTime createtime { get; set; }
            public int click { get; set; }
        }

        public class SomeEntity
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            [Column(MapType = typeof(JToken))]
            public Customer Customer { get; set; }
        }

        public class Customer    // Mapped to a JSON column in the table
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public Order[] Orders { get; set; }
        }

        public class Order       // Part of the JSON column
        {
            public decimal Price { get; set; }
            public string ShippingAddress { get; set; }
        }

        [Table(Name = "tb_tmttld"), OraclePrimaryKeyName("TMTTLD_PK01")]
        class TopicMapTypeToListDto
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int Clicks { get; set; }
            public int TypeGuid { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
            [JsonMap]
            public List<int> CouponIds { get; set; }
        }
        class TopicMapTypeToListDtoMap
        {
            public int Id { get; set; }
            public int Clicks { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
            public List<int> CouponIds { get; set; }
        }
        class TopicMapTypeToListDtoMap2
        {
            public int Id { get; set; }
            public int Clicks { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }
        class tuint256tb_01
        {
            public Guid Id { get; set; }
            public BigInteger Number { get; set; }
        }
        class CommandTimeoutCascade : IDisposable
        {
            public static AsyncLocal<int> _asyncLocalTimeout = new AsyncLocal<int>();
            public CommandTimeoutCascade(int timeout) => _asyncLocalTimeout.Value = timeout;
            public void Dispose() => _asyncLocalTimeout.Value = 0;
        }
        class EnterpriseInfo
        {
            [Column(IsPrimary = true, DbType = "varchar(60)")]
            public string id { get; set; }

            [Column(DbType = "varchar(128)")]
            public string img { get; set; }
        }

        public class SongRepository : BaseRepository<TUserImg, int>
        {
            public SongRepository(IFreeSql fsql) : base(fsql, null, null)
            {
                //fsql.CodeFirst.Entity<TUserImg>(a =>
                //    {
                //        //a.Property(b => b.Id).DbType("varchar(100)");
                //        a.Property(b => b.UserId).Stringlength(120);
                //        a.Property(b=>b.UserId).
                //    });
                fsql.CodeFirst
                    .ConfigEntity<TUserImg>(a =>
                    {
                        a.Property(b => b.UserId).StringLength(120);
                    });
                //var info= fsql.CodeFirst.GetTableByEntity(typeof(TUserImg));
                var sql = fsql.CodeFirst.GetComparisonDDLStatements<TUserImg>();
                var t1 = fsql.CodeFirst.GetComparisonDDLStatements(typeof(TUserImg), "TUserImg");
                fsql.CodeFirst.SyncStructure<TUserImg>(); ;//同步表结构

                var debug = sql;
            }

            //在这里增加 CURD 以外的方法
        }
        public interface IEntity
        {

        }
        /// <summary>
        /// 用户图片2
        /// </summary>
        public partial class TUserImg : IEntity
        {

            ///<summary>
            ///主键
            ///</summary>
            public string Id { get; set; }

            ///<summary>
            ///企业
            ///</summary>
            public string EnterpriseId { get; set; }

            ///<summary>
            ///用户id
            ///</summary>
            public string UserId { get; set; }

            ///<summary>
            ///图片
            ///</summary>
            public string Img { get; set; }

            ///<summary>
            ///创建人Id
            ///</summary>
            public string CId { get; set; }

            ///<summary>
            ///创建人
            ///</summary>
            public string CName { get; set; }

            ///<summary>
            ///创建日期
            ///</summary>
            public DateTime CTime { get; set; }

            ///<summary>
            ///创建日期2
            ///</summary>
            public DateTime CTime2 { get; set; }
        }
        class Rsbasedoc2
        {
            [Column(StringLength = 100)]
            public string Id { get; set; }

            [Column(StringLength = 100)]
            public string Name { get; set; }
        }

        interface IDeleteSoft
        {
            /// <summary>
            /// 软删除
            /// </summary>
            bool IsDeleted { get; set; }
        }
        class TestComment01 : IDeleteSoft
        {
            public bool IsDeleted { get; set; }
        }

        static void TestExp(IFreeSql fsql)
        {
            var tasks = new List<Task>();

            for (var a = 0; a < 1000; a++)
            {
                var task = Task.Run(async () =>
                {
                    var name = "123";
                    var result = await fsql.Select<Rsbasedoc2>()
                        .Where(t => t.Name == name
                            && fsql.Select<Rsbasedoc2>().Any(t2 => t2.Id == t.Id)).ToListAsync();
                });
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
        }

        class TreeModel
        {
            public int id { get; set; }
            public int parentid { get; set; }
            public string code { get; set; }

            [Navigate(nameof(parentid))]
            public TreeModel Parent { get; set; }
            [Navigate(nameof(parentid))]
            public List<TreeModel> Childs { get; set; }
        }

        class DateModel
        {
            public DateTime Date { get; set; }
        }
        record TestClass(string Name)
        {
            public Guid Id { get; set; }
            public string[] Tags { get; init; } = Array.Empty<string>();
        }

        class BaseModel<T>
        {
            public static int fsql;
        }
        class StringNulable
        {
            [Column(IsPrimary = true)]
            public string id { get; set; }
            [Column(StringLength = -1, IsNullable = true)]
            public string code1 { get; set; }
            [Column(StringLength = -1, IsNullable = false)]
            public string code2 { get; set; }
        }
        public class CCC
        {
            public int bb { get; set; }
            public int aa { get; set; }
        }
        public class BBB
        {
            public int bb { get; set; }
        }
        [Table(Name = "AAA_attr")]
        [Index("xxx1", nameof(aa))]
        [Index("xxx2", nameof(aa))]
        public class AAA
        {
            [Column(Name = "aa_attr")]
            public int aa { get; set; }
        }

        [Table(Name = "db2.sql_AAA_attr")]
        [Index("{tablename}_xxx1", nameof(aa))]
        [Index("{tablename}_xxx2", nameof(aa))]
        public class SqliteAAA
        {
            [Column(Name = "aa_attr")]
            public int aa { get; set; }
        }

        public class JoinTest01
        {
            public int id { get; set; }
            public string code { get; set; }
            public string parentcode { get; set; }
            public string name { get; set; }
            [Column(MapType = typeof(string))]
            public string JoinTest01Enum { get; set; }
            [Column(MapType = typeof(int))]
            public JoinTest01Enum JoinTest01Enum2 { get; set; }

            [Navigate(nameof(parentcode), TempPrimary = nameof(JoinTest01.code))]
            public JoinTest01 Parent { get; set; }
            [Navigate(nameof(JoinTest01.parentcode), TempPrimary = nameof(code))]
            public List<JoinTest01> Childs { get; set; }
        }
        public enum JoinTest01Enum { f1, f2, f3 }

        public class AccessOdbcAdapter : OdbcAdapter
        {
            public override string UnicodeStringRawSql(object value, ColumnInfo mapColumn) => value == null ? "NULL" : string.Concat("'", value.ToString().Replace("'", "''"), "'");
        }
        private static IFreeSql CreateInstance(string connectString, DataType type)
        {
            IFreeSql client = new FreeSqlBuilder()
                .UseConnectionString(type, connectString)
                .Build();
            if (DataType.Odbc.Equals(type))
            {
                client.SetOdbcAdapter(new AccessOdbcAdapter());
            }
            return client;
        }

        class TJson01
        {
            public Guid id { get; set; }
            [JsonMap]
            public DJson02 Json02 { get; set; }
            [JsonMap]
            public DJson02 Json03 { get; set; }
        }
        class TJson02
        {
            public Guid id { get; set; }
            [JsonMap]
            public DJson02 Json02 { get; set; }
        }
        public class DJson02
        {
            public string code { get; set; }
            public string parentcode { get; set; }
            public string name { get; set; }
        }
        class VersionBytes01
        {
            public Guid id { get; set; }
            public string name { get; set; }
            [Column(IsVersion = true)]
            public byte[] version { get; set; }
        }
        public class Xpb
        {
            [Column(Name = "ID", IsPrimary = true)]
            public string Id { get; set; }
            [Column(Name = "XP", StringLength = -1)]
            public byte[] Bytes { get; set; }
            [Column(Name = "UPLOAD_TIME")]
            public DateTime UploadTime { get; set; }

        }
        public static void VersionBytes(IFreeSql fsql)
        {

            fsql.Aop.ConfigEntityProperty += (_, e) =>
            {
                if (fsql.Ado.DataType == DataType.SqlServer &&
                    e.Property.Name == "version")
                {
                    e.ModifyResult.DbType = "timestamp";
                    e.ModifyResult.CanInsert = false;
                    e.ModifyResult.CanUpdate = false;
                }
            };

            fsql.Delete<VersionBytes01>().Where("1=1").ExecuteAffrows();
            var item = new VersionBytes01 { name = "name01" };
            fsql.Insert(item).ExecuteAffrows();
            var itemVersion = item.version;

            item = fsql.Select<VersionBytes01>().Where(a => a.id == item.id).First();

            item.name = "name02";
            var sql = fsql.Update<VersionBytes01>().SetSource(item).ToSql();
            if (1 != fsql.Update<VersionBytes01>().SetSource(item).ExecuteAffrows()) throw new Exception("不相同");

            //item.name = "name03";
            //if (1 != fsql.Update<VersionBytes01>().SetSource(item).ExecuteAffrows()) throw new Exception("不相同");

            if (1 != fsql.Update<VersionBytes01>().Set(a => a.name, "name04").Where(a => a.id == item.id).ExecuteAffrows()) throw new Exception("不相同");
        }
        public class City
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Point Center { get; set; }
        }
        public abstract class BaseEntity2
        {
            [Column(IsPrimary = true, IsIdentity = true)]
            public long Id { get; set; }
        }

        public class Student : BaseEntity2
        {
            public string Name { get; set; }
        }

        static void Main(string[] args)
        {
            var pams = new Dictionary<string, string>();
            var sql2rscs = Utils.ReplaceSqlConstString("'', 'SARTEN ACERO VITR.18CM''''GRAFIT''''', 'a",
                pams, "@lantin1");

            //using (IFreeSql client = CreateInstance(@"Driver={Microsoft Access Driver (*.mdb)};DBQ=d:/accdb/2007.accdb", DataType.Odbc))
            //{
            //    client.Aop.AuditValue += (_, e) =>
            //    {
            //        if (e.Object is Dictionary<string, object> dict)
            //        {
            //            foreach(var key in dict.Keys)
            //            {
            //                var val = dict[key];
            //                if (val == DBNull.Value) dict[key] = null;
            //            }
            //            e.ObjectAuditBreak = true;
            //        }
            //    };
            //    Dictionary<string, object> data = new Dictionary<string, object>();
            //    data.Add("ExpNo", "RSP0950008");
            //    data.Add("SPoint", "RSP0950004");
            //    data.Add("EPoint", "RSP095000440");
            //    data.Add("PType", "RS");
            //    data.Add("GType", "窨井轮廓线");
            //    data.Add("LineStyle", 2);
            //    data.Add("Memo", DBNull.Value);
            //    data.Add("ClassID", DBNull.Value);
            //    var kdkdksqlxx = client.InsertDict(data).AsTable("FZLINE").ToSql();
            //}


            BaseModel<User1>.fsql = 1;
            BaseModel<UserGroup>.fsql = 2;
            Console.WriteLine(BaseModel<User1>.fsql);
            Console.WriteLine(BaseModel<UserGroup>.fsql);


            #region 初始化 IFreeSql
            var fsql = new FreeSql.FreeSqlBuilder()
                .UseAutoSyncStructure(true)
                .UseNoneCommandParameter(true)
                .UseNameConvert(NameConvertType.ToLower)
                //.UseMappingPriority(MappingPriorityType.Attribute, MappingPriorityType.FluentApi, MappingPriorityType.Aop)


                .UseConnectionString(FreeSql.DataType.Sqlite, "data source=:memory:")
                .UseConnectionString(DataType.Sqlite, "data source=C:\\Users\\28810\\Desktop\\github\\FreeSql\\Examples\\base_entity\\AspNetRoleClaims\\ids_api.db")
                //.UseConnectionString(DataType.Sqlite, "data source=db1.db;attachs=db2.db")
                //.UseSlave("data source=test1.db", "data source=test2.db", "data source=test3.db", "data source=test4.db")
                //.UseSlaveWeight(10, 1, 1, 5)


                .UseConnectionString(FreeSql.DataType.Firebird, @"database=localhost:D:\fbdata\EXAMPLES.fdb;user=sysdba;password=123456;max pool size=5")
                //.UseQuoteSqlName(false)

                .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;min pool size=1;Max pool size=2;AllowLoadLocalInfile=true")

                .UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=3;TrustServerCertificate=true")

                //.UseConnectionString(FreeSql.DataType.PostgreSQL, "Host=192.168.164.10;Port=5432;Username=postgres;Password=123456;Database=tedb;Pooling=true;Maximum Pool Size=2")
                //.UseConnectionString(FreeSql.DataType.PostgreSQL, "Host=192.168.164.10;Port=5432;Username=postgres;Password=123456;Database=toc;Pooling=true;Maximum Pool Size=2")
                //.UseNameConvert(FreeSql.Internal.NameConvertType.ToLower)

                //.UseConnectionString(FreeSql.DataType.Oracle, "user id=user1;password=123456;data source=//127.0.0.1:1521/XE;Pooling=true;Max Pool Size=2")
                //.UseNameConvert(FreeSql.Internal.NameConvertType.ToUpper)

                //.UseConnectionString(FreeSql.DataType.Dameng, "server=127.0.0.1;port=5236;user id=2user;password=123456789;database=2user;poolsize=5;min pool size=1")
                //.UseNameConvert(FreeSql.Internal.NameConvertType.ToUpper)

                //.UseConnectionString(FreeSql.DataType.OdbcMySql, "Driver={MySQL ODBC 8.0 Unicode Driver};Server=127.0.0.1;Persist Security Info=False;Trusted_Connection=Yes;UID=root;PWD=root;DATABASE=cccddd_odbc;Charset=utf8;SslMode=none;Max pool size=2")

                //.UseConnectionString(FreeSql.DataType.OdbcSqlServer, "Driver={SQL Server};Server=.;Persist Security Info=False;Trusted_Connection=Yes;Integrated Security=True;DATABASE=freesqlTest_odbc;Pooling=true;Max pool size=3")

                //.UseConnectionString(FreeSql.DataType.OdbcPostgreSQL, "Driver={PostgreSQL Unicode(x64)};Server=192.168.164.10;Port=5432;UID=postgres;PWD=123456;Database=tedb_odbc;Pooling=true;Maximum Pool Size=2")
                //.UseNameConvert(FreeSql.Internal.NameConvertType.ToLower)

                //.UseConnectionString(FreeSql.DataType.OdbcOracle, "Driver={Oracle in XE};Server=//127.0.0.1:1521/XE;Persist Security Info=False;Trusted_Connection=Yes;UID=odbc1;PWD=123456")
                //.UseNameConvert(FreeSql.Internal.NameConvertType.ToUpper)

                //.UseConnectionString(FreeSql.DataType.OdbcDameng, "Driver={DM8 ODBC DRIVER};Server=127.0.0.1:5236;Persist Security Info=False;Trusted_Connection=Yes;UID=USER1;PWD=123456789")

                .UseMonitorCommand(cmd =>
                {
                    Console.WriteLine(cmd.CommandText + "\r\n");
                    //cmd.CommandText = null; //不执行
                })
                .UseLazyLoading(true)
                .UseGenerateCommandParameterWithLambda(true)
                .Build();
            BaseEntity.Initialization(fsql, () => _asyncUow.Value);
            #endregion

            var bulkUsers = new[] {
                new IdentityUser1 { Nickname = "nickname11", Username = "username11" },
                new IdentityUser1 { Nickname = "nickname12", Username = "username12" },
                new IdentityUser1 { Nickname = "nickname13", Username = "username13" },

                new IdentityUser1 { Nickname = "nickname21", Username = "username21" },
                new IdentityUser1 { Nickname = "nickname22", Username = "username22" },
                new IdentityUser1 { Nickname = "nickname23", Username = "username23" }
            };
            fsql.Insert(bulkUsers).NoneParameter().ExecuteAffrows();
            fsql.Insert(bulkUsers).IgnoreInsertValueSql(a => a.Nickname).NoneParameter().ExecuteAffrows();
            bulkUsers = fsql.Select<IdentityUser1>().OrderByDescending(a => a.Id).Limit(3).ToList().ToArray();
            bulkUsers[0].Nickname += "_bulkupdate";
            bulkUsers[1].Nickname += "_bulkupdate";
            bulkUsers[2].Nickname += "_bulkupdate";
            fsql.Update<IdentityUser1>().SetSource(bulkUsers).ExecuteSqlBulkCopy();


            var objtsql1 = fsql.Select<object>().WithSql("select * from user1").ToList();
            var objtsql2 = fsql.Select<object>().WithSql("select * from user1").ToList<User1>();

            var astsql = fsql.Select<AsTableLog, Sys_owner>()
                .InnerJoin((a, b) => a.id == b.Id)
                .OrderBy((a,b) => a.createtime)
                .ToSql();


            //var table = fsql.CodeFirst.GetTableByEntity(typeof(AsTableLog));
            //table.SetAsTable(new ModAsTableImpl(fsql), table.ColumnsByCs[nameof(AsTableLog.click)]);


            var testitems = new[]
            {
                new AsTableLog{ msg = "msg01", createtime = DateTime.Parse("2022-1-1 13:00:11"), click = 1 },
                new AsTableLog{ msg = "msg02", createtime = DateTime.Parse("2022-1-2 14:00:12"), click = 2 },
                new AsTableLog{ msg = "msg03", createtime = DateTime.Parse("2022-2-2 15:00:13"), click = 3 },
                new AsTableLog{ msg = "msg04", createtime = DateTime.Parse("2022-2-8 15:00:13"), click = 4 },
                new AsTableLog{ msg = "msg05", createtime = DateTime.Parse("2022-3-8 15:00:13"), click = 5 },
                new AsTableLog{ msg = "msg06", createtime = DateTime.Parse("2022-4-8 15:00:13"), click = 6 },
                new AsTableLog{ msg = "msg07", createtime = DateTime.Parse("2022-6-8 15:00:13"), click = 7 },
                new AsTableLog{ msg = "msg08", createtime = DateTime.Parse("2022-7-1"), click = 9},
                new AsTableLog{ msg = "msg09", createtime = DateTime.Parse("2022-7-1 11:00:00"), click = 10},
                new AsTableLog{ msg = "msg10", createtime = DateTime.Parse("2022-7-1 12:00:00"), click = 10}
            };
            var sqlatb = fsql.Insert(testitems).NoneParameter();
            var sqlat = sqlatb.ToSql();
            var sqlatr = sqlatb.ExecuteAffrows();

            var sqlatc1 = fsql.Delete<AsTableLog>().Where(a => a.id == Guid.NewGuid() && a.createtime == DateTime.Parse("2022-3-8 15:00:13"));
            var sqlatca1 = sqlatc1.ToSql();
            var sqlatcr1 = sqlatc1.ExecuteAffrows();

            var sqlatc2 = fsql.Delete<AsTableLog>().Where(a => a.id == Guid.NewGuid() && a.createtime == DateTime.Parse("2021-3-8 15:00:13"));
            var sqlatca2 = sqlatc2.ToSql();
            var sqlatcr2 = sqlatc2.ExecuteAffrows();

            var sqlatc = fsql.Delete<AsTableLog>().Where(a => a.id == Guid.NewGuid() && a.createtime.Between(DateTime.Parse("2022-3-1"), DateTime.Parse("2022-5-1")));
            var sqlatca = sqlatc.ToSql();
            var sqlatcr = sqlatc.ExecuteAffrows();

            var sqlatd1 = fsql.Update<AsTableLog>().SetSource(testitems[0]);
            var sqlatd101 = sqlatd1.ToSql();
            var sqlatd102 = sqlatd1.ExecuteAffrows();

            var sqlatd2 = fsql.Update<AsTableLog>().SetSource(testitems[5]);
            var sqlatd201 = sqlatd2.ToSql();
            var sqlatd202 = sqlatd2.ExecuteAffrows();

            var sqlatd3 = fsql.Update<AsTableLog>().SetSource(testitems);
            var sqlatd301 = sqlatd3.ToSql();
            var sqlatd302 = sqlatd3.ExecuteAffrows();

            var sqlatd4 = fsql.Update<AsTableLog>(Guid.NewGuid()).Set(a => a.msg == "newmsg");
            var sqlatd401 = sqlatd4.ToSql();
            var sqlatd402 = sqlatd4.ExecuteAffrows();

            var sqlatd5 = fsql.Update<AsTableLog>(Guid.NewGuid()).Set(a => a.msg == "newmsg").Where(a => a.createtime.Between(DateTime.Parse("2022-3-1"), DateTime.Parse("2022-5-1")));
            var sqlatd501 = sqlatd5.ToSql();
            var sqlatd502 = sqlatd5.ExecuteAffrows();

            var sqlatd6 = fsql.Update<AsTableLog>(Guid.NewGuid()).Set(a => a.msg == "newmsg").Where(a => a.createtime > DateTime.Parse("2022-3-1") && a.createtime < DateTime.Parse("2022-5-1"));
            var sqlatd601 = sqlatd6.ToSql();
            var sqlatd602 = sqlatd6.ExecuteAffrows();

            var sqlatd7 = fsql.Update<AsTableLog>(Guid.NewGuid()).Set(a => a.msg == "newmsg").Where(a => a.createtime > DateTime.Parse("2022-3-1"));
            var sqlatd701 = sqlatd7.ToSql();
            var sqlatd702 = sqlatd7.ExecuteAffrows();

            var sqlatd8 = fsql.Update<AsTableLog>(Guid.NewGuid()).Set(a => a.msg == "newmsg").Where(a => a.createtime < DateTime.Parse("2022-5-1"));
            var sqlatd801 = sqlatd8.ToSql();
            var sqlatd802 = sqlatd8.ExecuteAffrows();

            var sqlatd12 = fsql.InsertOrUpdate<AsTableLog>().SetSource(testitems[0]);
            var sqlatd1201 = sqlatd12.ToSql();
            var sqlatd1202 = sqlatd12.ExecuteAffrows();

            var sqlatd22 = fsql.InsertOrUpdate<AsTableLog>().SetSource(testitems[5]);
            var sqlatd2201 = sqlatd22.ToSql();
            var sqlatd2202 = sqlatd22.ExecuteAffrows();

            var sqlatd32 = fsql.InsertOrUpdate<AsTableLog>().SetSource(testitems);
            var sqlatd3201 = sqlatd32.ToSql();
            var sqlatd3202 = sqlatd32.ExecuteAffrows();

            var sqls1 = fsql.Select<AsTableLog>().OrderBy(a => a.createtime);
            var sqls101 = sqls1.ToSql();
            var sqls102 = sqls1.ToList();

            var sqls2 = fsql.Select<AsTableLog>().Where(a => a.createtime.Between(DateTime.Parse("2022-3-1"), DateTime.Parse("2022-5-1")));
            var sqls201 = sqls2.ToSql();
            var sqls202 = sqls2.ToList();

            var sqls3 = fsql.Select<AsTableLog>().Where(a => a.createtime > DateTime.Parse("2022-3-1") && a.createtime < DateTime.Parse("2022-5-1"));
            var sqls301 = sqls3.ToSql();
            var sqls302 = sqls3.ToList();

            var sqls4 = fsql.Select<AsTableLog>().Where(a => a.createtime > DateTime.Parse("2022-3-1"));
            var sqls401 = sqls4.ToSql();
            var sqls402 = sqls4.ToList();

            var sqls5 = fsql.Select<AsTableLog>().Where(a => a.createtime < DateTime.Parse("2022-5-1"));
            var sqls501 = sqls5.ToSql();
            var sqls502 = sqls5.ToList();

            var sqls6 = fsql.Select<AsTableLog>().Where(a => a.createtime < DateTime.Parse("2022-5-1")).Limit(10).OrderBy(a => a.createtime);
            var sqls601 = sqls6.ToSql();
            var sqls602 = sqls6.ToList();

            var sqls7 = fsql.Select<AsTableLog>().Where(a => a.createtime < DateTime.Parse("2022-5-1")).ToAggregate(g => new
            {
                sum1 = g.Sum(g.Key.click),
                cou1 = g.Count(),
                avg1 = g.Avg(g.Key.click),
                min = g.Min(g.Key.click),
                max = g.Max(g.Key.click)
            });


            var usergroupRepository = fsql.GetAggregateRootRepository<UserGroup>();
            usergroupRepository.Delete(a => true);
            usergroupRepository.Insert(new[]{
                new UserGroup
                {
                    CreateTime = DateTime.Now,
                    GroupName = "group1",
                    UpdateTime = DateTime.Now,
                    Sort = 1,
                    User1s = new List<User1>
                    {
                        new User1 { Nickname = "nickname11", Username = "username11", Description = "desc11" },
                        new User1 { Nickname = "nickname12", Username = "username12", Description = "desc12" },
                        new User1 { Nickname = "nickname13", Username = "username13", Description = "desc13" },
                    }
                },
                new UserGroup
                {
                    CreateTime = DateTime.Now,
                    GroupName = "group2",
                    UpdateTime = DateTime.Now,
                    Sort = 2,
                    User1s = new List<User1>
                    {
                        new User1 { Nickname = "nickname21", Username = "username21", Description = "desc21" },
                        new User1 { Nickname = "nickname22", Username = "username22", Description = "desc22" },
                        new User1 { Nickname = "nickname23", Username = "username23", Description = "desc23" },
                    }
                },
            });
            var ugroupFirst = usergroupRepository.Select.First();
            ugroupFirst.Sort++;
            usergroupRepository.Update(ugroupFirst);
            var userRepository = fsql.GetAggregateRootRepository<User1>();

            var testsublist1 = fsql.Select<UserGroup>()
                .First(a => new
                {
                    a.Id,
                    list = userRepository.Select.Where(b => b.GroupId == a.Id).ToList(),
                    list2 = userRepository.Select.Where(b => b.GroupId == a.Id).ToList(b => b.Nickname),
                });

            //fsql.CodeFirst.GetTableByEntity(typeof(User1)).Columns.Values.ToList().ForEach(col =>
            //{
            //    col.Comment = "";
            //});
            fsql.Insert(Enumerable.Range(0, 100).Select(a => new User1 { Id = Guid.NewGuid(), Nickname = $"nickname{a}", Username = $"username{a}", Description = $"desc{a}" }).ToArray()).ExecuteAffrows();

            fsql.InsertOrUpdate<User1>()
                .SetSource(fsql.Select<User1>().ToList())
                .ExecuteMySqlBulkCopy();

            var updatejoin01 = fsql.Update<User1>()
                .Join(fsql.Select<UserGroup>(), (a, b) => a.GroupId == b.Id)
                .Set((a, b) => a.Nickname == b.GroupName)
                .ExecuteAffrows();
            var updatejoin02 = fsql.Update<User1>()
                .Join<UserGroup>((a, b) => a.GroupId == b.Id)
                .Set((a, b) => a.Nickname == b.GroupName)
                .ExecuteAffrows();
            var updatejoin03 = fsql.Update<User1>()
                .Join<UserGroup>((a, b) => a.GroupId == b.Id)
                .Set((a, b) => a.Nickname == "b.groupname")
                .ExecuteAffrows();

            var sql1c2 = fsql.Select<User1>()
                .GroupBy(a => new { a.Nickname, a.Avatar })
                .WithTempQuery(b => new
                {
                    sum = b.Sum(b.Value.Sort),
                    b.Key.Nickname,
                    b.Key.Avatar,
                })
                .OrderByDescending(arg => arg.sum)
                .ToSql(arg => new
                {
                    str1 = string.Concat(arg.Nickname, '-', arg.Avatar, '-'),
                    str2 = string.Concat(arg.Nickname, '-', arg.Avatar)
                });   //报错 多括号
                //.ToOne(arg => string.Concat(arg.Nickname, '-', arg.Avatar)); //正常
            Console.WriteLine(sql1c2);

            var xp = new Xpb()
            {
                Id = "L23035555",
                UploadTime = DateTime.Now,
                Bytes = Convert.FromBase64String("/9j/4AAQSkZJRgABAQAAAQABAAD/4QAaWV9ZW05+OTg5CwkIG2R6e2VkZhYfAw0A/9sAQwABAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB/9sAQwEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB/8AAEQgBuQFmAwEiAAIRAQMRAf/EAB8AAAEFAQEBAQEBAAAAAAAAAAABAgMEBQYHCAkKC//EALUQAAIBAwMCBAMFBQQEAAABfQECAwAEEQUSITFBBhNRYQcicRQygZGhCCNCscEVUtHwJDNicoIJChYXGBkaJSYnKCkqNDU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6g4SFhoeIiYqSk5SVlpeYmZqio6Slpqeoqaqys7S1tre4ubrCw8TFxsfIycrS09TV1tfY2drh4uPk5ebn6Onq8fLz9PX29/j5+v/EAB8BAAMBAQEBAQEBAQEAAAAAAAABAgMEBQYHCAkKC//EALURAAIBAgQEAwQHBQQEAAECdwABAgMRBAUhMQYSQVEHYXETIjKBCBRCkaGxwQkjM1LwFWJy0QoWJDThJfEXGBkaJicoKSo1Njc4OTpDREVGR0hJSlNUVVZXWFlaY2RlZmdoaWpzdHV2d3h5eoKDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uLj5OXm5+jp6vLz9PX29/j5+v/aAAwDAQACEQMRAD8A+f6KKK/zvP8AsoCiiigDPb+1Begg2R03dECojmN/glBM29pkt8Ll2X5c7QBhmGGbfwX0lzZS2NyY4Y7qM3MT7VD2ofMwPySh3li/dgbQYnCtFJGXeWPSor1KOazoKChg8uvDD1MM5ywcJzqQqU1SlKo5Np1eVN+0ioybnNScozcT8xzfwvwud18XUx3GXiH7DE53lefUcBheKauCwWX4vKc2lm+Hw+BhhcLSqxy+dZ0qFXA4itiaMcPhcJLDqhjMPTxaKKKK8s/TgooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACsb+yjJMmpS/ZIdc+xfYG1G1tFLpamb7SbWNp2kke3E+HZJWMcjqJRDFJjbs0V2YPH4nASnPDSpwnNcspToUa94WlGVJxr06kHTqKX7yDi4z5YcyfKj47jHgPhvj3C4TA8T4fMMXgsDiaeOw2HwOeZ3kfsswoV8PiMJmEcTkWYZbjY43A1MP/ALHXhiYyoRr4qEdK8xF3BV3EFsDcVUqpbHJVSzlQTyFLMQOCx6laKK5G7tt2u23oklr2SSSXZJJLZKx9fCCpwhTi5uMIxhFznOrNqKUU51asp1Kk2l706k5Tm7ynKUm2zOetFFFDberbb7vUcYxglGEVGK2jFKKXolZIlDKYmV3kLKVMK4LINx/e8mQCPICn5YnLkDLIF+aKiitataVaNCMo017Cj7FThBRnUj7WrVUq81rVqR9r7GM5XcaFKjRXuUopcODy+ngauY1aVbFVFmWOWPlQr15VcPg6n1LBYKdDLqLSjg8LWlg3j61Cn7tXM8ZmGNk3VxdQKKKKxO8KKKKAIoUljRlmm89zNcOH8tYtsUk8kkEO1OD9ngaO38w/NL5Xmv8AO7VLRRVSk5ylN8qcpOTUYxhFOTu+WEFGEI3ekYRjGK0ikkkZUKMMPQo4em6sqdClTowlXr18VXlClBQi62JxNSticRVcYp1K+Iq1a9ad6lWpOpKUmUUUVJqFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAf/9k=")
            };
            fsql.InsertOrUpdate<Xpb>().SetSource(xp).ExecuteAffrows();

            var xpsql01 = fsql.InsertOrUpdateDict(new Dictionary<string, object>
            {
                [""] = "Xpb",
                ["ID"] = "L230355551",
                ["UPLOAD_TIME"] = DateTime.Now,
                ["XP"] = Convert.FromBase64String("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FFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAf/9k=")
            }).WherePrimary("Id").ExecuteAffrows();

        fsql.UseJsonMap();

            fsql.Select<User1>().IncludeMany(a => a.Roles);

            var displayNameTb = fsql.CodeFirst.GetTableByEntity(typeof(DeviceCodes));

            var joinsql1 = fsql.Select<JoinTest01>()
                .Include(a => a.Parent.Parent)
                .Where(a => a.Parent.Parent.code == "001")
                .Where(a => a.JoinTest01Enum == JoinTest01Enum.f3.ToString())
                .Where(a => object.Equals(a.JoinTest01Enum, JoinTest01Enum.f3))
                .Where(a => new[] { JoinTest01Enum.f2, JoinTest01Enum.f3 }.Contains(a.JoinTest01Enum2))
                .ToSql();

            var atimpl = fsql.CodeFirst.GetTableByEntity(typeof(AsTableLog))
                .AsTableImpl;

            atimpl.GetTableNameByColumnValue(DateTime.Parse("2023-7-1"), autoExpand: true);

            //var dywhere = new DynamicFilterInfo { Field = "AspNetRoless.Name", Operator = DynamicFilterOperator.Equal, Value = "Admin" };
            //var method = typeof(ISelect<object>).GetMethod("WhereDynamicFilter");
            //var users4 = fsql.Select<AspNetUsers>().IncludeByPropertyName("AspNetUserRoless", then => then.WhereDynamicFilter(dywhere)).ToList();


            var type = typeof(Student);

            var sw111 = fsql.Queryable<object>()
                .AsType(type)
                .Where(s => (s as BaseEntity2).Id == 1)
                .ToSql();

            Console.WriteLine(sw111);

            var testsql01 = fsql.Select<User1>()
                //.GroupBy(a => new { a.Avatar, a.GroupId })
                //.Having(g => g.Sum(g.Value.Sort) > 0)
                .WithTempQuery(a => new
                {
                    a.Avatar, a.GroupId, sum1 = SqlExt.Sum(a.Sort).ToValue()
                })
                .Where(a => a.sum1 > 0)
                .ToSql();


            fsql.Aop.ParseExpression += (_, e) =>
            {
                if (fsql.Ado.DataType == DataType.PostgreSQL)
                {
                    if (e.Expression is MethodCallExpression callExp &&
                        callExp.Object is not null && typeof(Geometry).IsAssignableFrom(callExp.Method.DeclaringType))
                    {
                        var instance = callExp.Object;
                        var arguments = callExp.Arguments;
                        var Function = new Func<string, Expression[], Type, string>((dbfunc, args, returnType) =>
                        {
                            return $"{dbfunc}({string.Join(", ", args.Select(a => e.FreeParse(a)))})";
                        });
                        e.Result = callExp.Method.Name switch
                        {
                            nameof(Geometry.AsBinary) => Function("ST_AsBinary", new[] { instance }, typeof(byte[])),
                            nameof(Geometry.AsText) => Function("ST_AsText", new[] { instance }, typeof(string)),
                            nameof(Geometry.Buffer) => Function("ST_Buffer", new[] { instance }.Concat(arguments).ToArray(), typeof(Geometry)),
                            nameof(Geometry.Contains) => Function("ST_Contains", new[] { instance, arguments[0] }, typeof(bool)),
                            nameof(Geometry.ConvexHull) => Function("ST_ConvexHull", new[] { instance }, typeof(Geometry)),
                            nameof(Geometry.CoveredBy) => Function("ST_CoveredBy", new[] { instance, arguments[0] }, typeof(bool)),
                            nameof(Geometry.Covers) => Function("ST_Covers", new[] { instance, arguments[0] }, typeof(bool)),
                            nameof(Geometry.Crosses) => Function("ST_Crosses", new[] { instance, arguments[0] }, typeof(bool)),
                            nameof(Geometry.Disjoint) => Function("ST_Disjoint", new[] { instance, arguments[0] }, typeof(bool)),
                            nameof(Geometry.Difference) => Function("ST_Difference", new[] { instance, arguments[0] }, typeof(Geometry)),
                            nameof(Geometry.Distance) => Function("ST_Distance", new[] { instance }.Concat(arguments).ToArray(), typeof(double)),
                            nameof(Geometry.EqualsExact) => Function("ST_OrderingEquals", new[] { instance, arguments[0] }, typeof(bool)),
                            nameof(Geometry.EqualsTopologically) => Function("ST_Equals", new[] { instance, arguments[0] }, typeof(bool)),
                            nameof(Geometry.GetGeometryN) => Function("ST_GeometryN", new[] { instance, arguments[0] }, typeof(Geometry)),
                            nameof(Polygon.GetInteriorRingN) => Function("ST_InteriorRingN", new[] { instance, arguments[0] }, typeof(Geometry)),
                            nameof(LineString.GetPointN) => Function("ST_PointN", new[] { instance, arguments[0] }, typeof(Geometry)),
                            nameof(Geometry.Intersection) => Function("ST_Intersection", new[] { instance, arguments[0] }, typeof(Geometry)),
                            nameof(Geometry.Intersects) => Function("ST_Intersects", new[] { instance, arguments[0] }, typeof(bool)),
                            nameof(Geometry.IsWithinDistance) => Function("ST_DWithin", new[] { instance }.Concat(arguments).ToArray(), typeof(bool)),
                            nameof(Geometry.Normalized) => Function("ST_Normalize", new[] { instance }, typeof(Geometry)),
                            nameof(Geometry.Overlaps) => Function("ST_Overlaps", new[] { instance, arguments[0] }, typeof(bool)),
                            nameof(Geometry.Relate) => Function("ST_Relate", new[] { instance, arguments[0], arguments[1] }, typeof(bool)),
                            nameof(Geometry.Reverse) => Function("ST_Reverse", new[] { instance }, typeof(Geometry)),
                            nameof(Geometry.SymmetricDifference) => Function("ST_SymDifference", new[] { instance, arguments[0] }, typeof(Geometry)),
                            nameof(Geometry.ToBinary) => Function("ST_AsBinary", new[] { instance }, typeof(byte[])),
                            nameof(Geometry.ToText) => Function("ST_AsText", new[] { instance }, typeof(string)),
                            nameof(Geometry.Touches) => Function("ST_Touches", new[] { instance, arguments[0] }, typeof(bool)),
                            nameof(Geometry.Within) => Function("ST_Within", new[] { instance, arguments[0] }, typeof(bool)),

                            nameof(Geometry.Union) when arguments.Count == 0 => Function("ST_UnaryUnion", new[] { instance }, typeof(Geometry)),
                            nameof(Geometry.Union) when arguments.Count == 1 => Function("ST_Union", new[] { instance, arguments[0] }, typeof(Geometry)),

                            _ => null
                        };

                    }
                }
            };
            if (fsql.Ado.DataType == DataType.PostgreSQL)
            {
                Npgsql.NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();
                var geo = new Point(10, 20);
                fsql.Select<City>()
                    .Where(a => geo.Distance(a.Center) < 100).ToList();
            }

            var items = new List<User1>();
            for (var a = 0; a < 3; a++) items.Add(new User1 { Id = Guid.NewGuid(), Avatar = $"avatar{a}" });
            var sqltest01 = fsql.Update<User1>()
                .SetSource(items)
                .UpdateColumns(a => a.Avatar)
                .Set(a => a.Sort + 1).ToSql();

            //VersionBytes(fsql);


            fsql.Delete<TJson01>().Where(a => true).ExecuteAffrows();
            fsql.Insert(new TJson01
            {
                Json02 = new DJson02 { code = "002", name = "name002", parentcode = "002_parent" },
                Json03 = new DJson02 { code = "003", name = "name003", parentcode = "003_parent" },
            }).NoneParameter(false).ExecuteAffrows();
            var tjson01 = fsql.Select<TJson01>().First();

            fsql.Delete<TJson02>().Where(a => true).ExecuteAffrows();
            fsql.Insert(new TJson02
            {
                Json02 = new DJson02 { code = "0022", name = "name0022", parentcode = "0022_parent" },
            }).NoneParameter(false).ExecuteAffrows();
            var tjson02 = fsql.Select<TJson02>().First();


            var sqlv01 = fsql.Select<BaseDataEntity>().AsType(typeof(GoodsData))
                .ToSql(v => new GoodsDataDTO()
                {
                    Id = v.Id,
                    GoodsNo = v.Code,
                    GoodsName = v.Name,
                });
            // 解析会连带导出 CategoryId ，但是如果查询别名不是 a 时就会重置到基类表
            // SELECT a.`CategoryId` as1, v.`Id` as2, v.`Code` as3, v.`Name` as4 
            // FROM `FreeSqlTest`.`bdd_1` a, `BaseDataEntity` v

            var groupsql01 = fsql.Select<User1>()
                .GroupBy(a => new
                {
                    djjg = a.Id,
                    qllx = a.Nickname,
                    hjhs = a.GroupId
                })
                .ToSql(g => new
                {
                    g.Key.djjg,
                    g.Key.qllx,
                    xhjsl = g.Count(g.Key.djjg),
                    hjzhs = g.Sum(g.Key.hjhs),
                    blywsl = g.Count()
                }, FieldAliasOptions.AsProperty);

            using (var uow = fsql.CreateUnitOfWork())
            {
                uow.Orm.Select<User1>().ForUpdate().ToList();
            }

            var listaaaddd = new List<User1>();
            for (int i = 0; i < 2; i++)
            {
                listaaaddd.Add(new User1 { Nickname = $"测试文本:{i}" });
            }
            fsql.Select<User1>();
            fsql.Transaction(() =>
            {

                fsql.Insert(listaaaddd).ExecuteAffrows();  //加在事务里就出错
            });

            fsql.Select<IdentityTable>().Count();

            var dkdksql = fsql.Select<User1>().WithLock().From<UserGroup>()
                .InnerJoin<UserGroup>((user, usergroup) => user.GroupId == usergroup.Id && usergroup.GroupName == "xxx")
                .ToSql();

            //Func<string> getName1 = () => "xxx";
            //fsql.GlobalFilter.Apply<User1>("fil1", a => a.Nickname == getName1());
            //var gnsql2 = fsql.Select<User1>().ToSql();

            using (var ctx9 = fsql.CreateDbContext())
            {
                //var uset = ctx9.Set<UserGroup>();
                //var item = new UserGroup
                //{
                //    GroupName = "group1"
                //};
                //uset.Add(item);
                //item.GroupName = "group1_2";
                //uset.Update(item);
                var uset = ctx9.Set<User1>();
                var item = new User1
                {
                    Nickname = "nick1",
                    Username = "user1"
                };
                uset.Add(item);
                item.Nickname = "nick1_2";
                item.Username = "user1_2";
                uset.Update(item);

                ctx9.SaveChanges();
            }

            var strs = new string[] { "a", "b", "c" };
            var strssql1 = fsql.Select<User1>().Where(a => strs.Any(b => b == a.Nickname)).ToSql();
            var strssql2 = fsql.Select<User1>().Where(a => strs.Any(b => a.Nickname.Contains(b))).ToSql();
            var objs = new UserGroup[] { new UserGroup { GroupName = "a", Id = 1 }, new UserGroup { GroupName = "b", Id = 2 }, new UserGroup { GroupName = "c", Id = 3 } };
            var objssql1 = fsql.Select<User1>().Where(a => objs.Any(b => b.GroupName == a.Nickname && b.Id == a.GroupId)).ToSql();


            var tttsqlext01 = fsql.Select<User1>().ToSql(a => new
            {
                cou = SqlExt.Count(1).Over().PartitionBy(a.Id).ToValue(),
                avg = SqlExt.Avg(1).Over().PartitionBy(a.Id).ToValue()

            });



            //fsql.CodeFirst.SyncStructure<SqliteAAA>();

            fsql.CodeFirst.Entity<JoinTest01>(a => a.Property(p => p.code).IsRequired());
            var repo1010 = fsql.GetRepository<JoinTest01>();
            var jtitem = new JoinTest01 { id = 100 };
            repo1010.Attach(jtitem);
            jtitem.name = "name01";
            repo1010.Update(jtitem);

            var sqlt0a1 = fsql.InsertOrUpdate<抖店实时销售金额表>()
                .SetSource(new 抖店实时销售金额表
                {
                    ID = 1,
                    品牌名称 = "NIKE",
                })
                .ToSql();

            fsql.UseMessagePackMap();

            fsql.Delete<MessagePackMapInfo>().Where("1=1").ExecuteAffrows();
            fsql.Insert(new MessagePackMapInfo { id = Guid.NewGuid(), Info = new MessagePackMap01 { name = "0023 中国国家1", address = "001address" } }).ExecuteAffrows();
            var rem1 = fsql.Select<MessagePackMapInfo>().ToList();

            var result1x = fsql.Ado.QuerySingle(() => new
            {
                DateTime.Now,
                DateTime.UtcNow,
                Math.PI
            });

            var testsublist2 = fsql.Select<UserGroup>()
                .GroupBy(a => new { a.Id })
                .WithTempQuery(a => a.Key)
                .First(a => new
                {
                    id = a,
                    list = userRepository.Select.Where(b => b.GroupId == a.Id).ToList(),
                    list2 = userRepository.Select.Where(b => b.GroupId == a.Id).ToList(b => b.Nickname),
                });


            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("id", 1);
            dic.Add("name", "xxxx");
            dic.Add("rowVersion", 100);
            var diclist = new List<Dictionary<string, object>>();
            diclist.Add(dic);
            diclist.Add(new Dictionary<string, object>
            {
                ["id"] = 2,
                ["name"] = "123,1234,123444",
                ["rowVersion"] = 1
            });

            var sqss = fsql.InsertDict(dic).AsTable("table1").ToSql();
            var sqss2 = fsql.InsertDict(diclist).AsTable("table1").ToSql();
            sqss = fsql.InsertDict(dic).AsTable("table1").NoneParameter().ToSql();
            sqss2 = fsql.InsertDict(diclist).AsTable("table1").NoneParameter().ToSql();

            dic["xxx"] = null;
            dic["yyy"] = 111;
            var sqlupd1 = fsql.UpdateDict(dic).AsTable("table1").WherePrimary("id").ToSql();
            var sqlupd2 = fsql.UpdateDict(diclist).AsTable("table1").WherePrimary("id").ToSql();
            var sqlupd11 = fsql.UpdateDict(dic).AsTable("table1").WherePrimary("id").NoneParameter(false).ToSql();
            var sqlupd22 = fsql.UpdateDict(diclist).AsTable("table1").WherePrimary("id").NoneParameter(false).ToSql();

            var sqlupd111 = fsql.UpdateDict(dic).AsTable("table1").WherePrimary("id").IsVersion("rowVersion").NoneParameter(false).ToSql();
            var sqlupd221 = fsql.UpdateDict(diclist).AsTable("table1").WherePrimary("id").IsVersion("rowVersion").NoneParameter(false).ToSql();
            //fsql.UpdateDict(dic).AsTable("table1").WherePrimary("id").IsVersion("rowVersion").NoneParameter(false).ExecuteAffrows();
            //fsql.UpdateDict(diclist).AsTable("table1").WherePrimary("id").IsVersion("rowVersion").NoneParameter(false).ExecuteAffrows();

            var sqldel1 = fsql.DeleteDict(dic).AsTable("table1").ToSql();
            var sqldel2 = fsql.DeleteDict(diclist).AsTable("table1").ToSql();
            diclist[1]["title"] = "newtitle";
            var sqldel3 = fsql.DeleteDict(diclist).AsTable("table1").ToSql();
            diclist.Clear();
            diclist.Add(new Dictionary<string, object>
            {
                ["id"] = 1
            });
            diclist.Add(new Dictionary<string, object>
            {
                ["id"] = 2
            });
            var sqldel4 = fsql.DeleteDict(diclist).AsTable("table1").ToSql();

            


            fsql.Aop.ConfigEntity += (_, e) =>
            {
                Console.WriteLine("Aop.ConfigEntity: " + e.ModifyResult.Name);
                e.ModifyIndexResult.Add(new IndexAttribute("xxx2", "aa", true));
            };
            fsql.Aop.ConfigEntityProperty += (_, e) =>
            {
                Console.WriteLine("Aop.ConfigEntityProperty: " + e.ModifyResult.Name);
            };
            fsql.CodeFirst.ConfigEntity<AAA>(t =>
            {
                t.Name("AAA_fluentapi");
                t.Property(a => a.aa).Name("AA_fluentapi");
            });
            fsql.Select<AAA>();

            fsql.Select<AAA>();



            var sqlskdfj = fsql.Select<object>().AsType(typeof(BBB)).ToSql(a => new CCC());


            var dbpars = new List<DbParameter>();

            var a1id1 = Guid.NewGuid();
            var a1id2 = Guid.NewGuid();
            //fsql.CodeFirst.IsGenerateCommandParameterWithLambda = true;
            var sql1a0 = fsql.Select<User1>()
                .WithParameters(dbpars)
                .Where(a => a.Id == a1id1)

                .UnionAll(
                    fsql.Select<User1>()
                        .WithParameters(dbpars)
                        .Where(a => a.Id == a1id2),

                    fsql.Select<User1>()
                        .WithParameters(dbpars)
                        .Where(a => a.Id == a1id2)
                )
                .Where(a => a.Id == a1id1 || a.Id == a1id2)
                .ToSql();
            var sql1a1 = fsql.Select<User1>()
                .Where(a => a.Id == a1id1)
                .UnionAll(
                    fsql.Select<User1>()
                    .Where(a => a.Id == a1id2)
                )
                .Where(a => a.Id == a1id1 || a.Id == a1id2)
                .ToSql();
            var sql1a2 = fsql.Select<User1, UserGroup>()
                .InnerJoin((a, b) => a.GroupId == b.Id)
                .Where((a, b) => a.Id == a1id1)
                .WithTempQuery((a, b) => new { user = a, group = b }) //匿名类型

                .UnionAll(
                    fsql.Select<User1, UserGroup>()
                        .InnerJoin((a, b) => a.GroupId == b.Id)
                        .Where((a, b) => a.Id == a1id2)
                        .WithTempQuery((a, b) => new { user = a, group = b }) //匿名类型
                )

                .Where(a => a.user.Id == a1id1 || a.user.Id == a1id2)
                .ToSql();


            var ddlsql01 = fsql.CodeFirst.GetComparisonDDLStatements<StringNulable>();

            Expression<Func<HzyTuple<User1, Group, Group, Group, Group, Group>, bool>> where = null;
            where = where.Or(a => a.t6.Index > 0);
            where = where.Or(a => a.t5.Index > 0);
            where = where.Or(a => a.t4.Index > 0);
            where = where.Or(a => a.t3.Index > 0);
            where = where.Or(a => a.t2.Index > 0);
            where = where.Or(a => a.t1.Nickname.Length > 0);

            var sql11224333 = fsql.Select<User1, Group, Group, Group, Group, Group>().Where(where).ToSql();


            fsql.UseJsonMap();

            fsql.CodeFirst.ConfigEntity<TestClass>(cf =>
            {
                cf.Property(p => p.Name).IsNullable(false);
                cf.Property(p => p.Tags).JsonMap();
            });

            fsql.Insert(new TestClass("test 1")
            {
                Tags = new[] { "a", "b" },
            })
            .ExecuteAffrows();
            var records = fsql.Queryable<TestClass>().ToList();


            InitData();
            InitData();

            var dates = Enumerable.Range(0, 5)
                .Select(a => new DateModel { Date = DateTime.Parse("2022-08-01").AddDays(a) })
                .ToArray();
            var datesSql1 = fsql.Select<User1>()
                .GroupBy(a => a.CreateTime.Date)
                .WithTempQuery(a => new
                {
                    date = a.Key,
                    sum1 = a.Sum(a.Value.Nickname.Length)
                })
                .FromQuery(fsql.Select<DateModel>().WithMemory(dates))
                .RightJoin((a, b) => a.date == b.Date)
                .ToSql();



            var treeSql1 = fsql.Select<TreeModel>()
                .WhereCascade(a => a.code == "xxx")
                .Where(a => a.id == 123)
                .AsTreeCte()
                .ToSql();

            var list = new List<User1>();
            list.Add(new User1 { Id = Guid.NewGuid() });
            list.Add(new User1 { Id = Guid.NewGuid() });
            list.Add(new User1 { Id = Guid.NewGuid() });

            var listSql3 = fsql.InsertOrUpdate<User1>().IfExistsDoNothing().SetSource(list).ToSql();

            var listSql = fsql.Select<User1>()
                .WithMemory(list)
                .ToSql();

            var listSql2 = fsql.Select<UserGroup>()
                .FromQuery(fsql.Select<User1>().WithMemory(list))
                .InnerJoin((a, b) => a.Id == b.GroupId)
                .ToSql();

            var listSql2Result = fsql.Select<UserGroup>()
                .FromQuery(fsql.Select<User1>().WithMemory(list))
                .InnerJoin((a, b) => a.Id == b.GroupId)
                .ToList();


            var atimpl2 = fsql.CodeFirst.GetTableByEntity(typeof(AsTableLog))
                .AsTableImpl;

            atimpl2.GetTableNameByColumnValue(DateTime.Parse("2023-7-1"), autoExpand: true);


            fsql.Select<User1, UserGroup>()
                .InnerJoin((a, b) => a.GroupId == b.Id)
                .Where((a, b) => b.GroupName == "group1")
                .WithTempQuery((a, b) => new
                {
                    User = a,
                    GroupName = b.GroupName,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(b.GroupName).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToList();

            var sqlt1 = fsql.Select<User1>()
                .ToSql(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                }, FieldAliasOptions.AsProperty);
            sqlt1 = fsql.Select<User1>()
                .WithSql(sqlt1)
                .Where("a.rownum = 1")
                .ToSql();

            var sqlt2 = fsql.Select<User1>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql();
            var sqlt22 = fsql.Select<User1>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToList();
            var sqlt23 = fsql.Select<User1>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToList(a => new
                {
                    a.item,
                    a.rownum
                });

            var sqlt24 = fsql.Select<User1>()
                .GroupBy(a => new { a.Nickname })
                .WithTempQuery(g => new
                {
                    g.Key.Nickname,
                    sum1 = g.Sum(g.Value.Avatar.Length)
                })
                .Where(a => a.Nickname != null)
                .ToList();



            Func<string> getName = () => "xxx";
            fsql.GlobalFilter.Apply<User1>("fil1", a => a.Nickname == getName());
            var gnsql = fsql.Select<User1>().ToSql();

            Dictionary<string, object> dic22 = new Dictionary<string, object>();
            dic22.Add("id", 1);
            dic22.Add("name", "xxxx");
            dic22.Add("bytes", Encoding.UTF8.GetBytes("我是中国人"));
            var sqlBytes = fsql.InsertDict(dic22).AsTable("table1").ToSql();

            var sqlToYear = fsql.Select<User1>().ToSql(a => a.CreateTime.Year);

            TestExp(fsql);

            fsql.CodeFirst.GetTableByEntity(typeof(TestComment01));

            fsql.Select<TUserImg>();

            var srepo = new SongRepository(fsql);

            var sql122234 = fsql.CodeFirst.GetComparisonDDLStatements<EnterpriseInfo>();

            if (fsql.Ado.DataType == DataType.PostgreSQL)
            {
                fsql.CodeFirst.IsNoneCommandParameter = false;
                fsql.Aop.AuditDataReader += (_, e) =>
                {
                    var dbtype = e.DataReader.GetDataTypeName(e.Index);
                    var m = Regex.Match(dbtype, @"numeric\((\d+)\)", RegexOptions.IgnoreCase);
                    if (m.Success && int.Parse(m.Groups[1].Value) > 19)
                        e.Value = e.DataReader.GetFieldValue<BigInteger>(e.Index); //否则会报溢出错误
                };

                var num = BigInteger.Parse("57896044618658097711785492504343953926634992332820282019728792003956564819968");
                fsql.Delete<tuint256tb_01>().Where("1=1").ExecuteAffrows();
                if (1 != fsql.Insert(new tuint256tb_01()).ExecuteAffrows()) throw new Exception("not equal");
                var find = fsql.Select<tuint256tb_01>().ToList();
                if (find.Count != 1) throw new Exception("not single");
                if ("0" != find[0].Number.ToString()) throw new Exception("not equal");
                var item = new tuint256tb_01 { Number = num };
                if (1 != fsql.Insert(item).ExecuteAffrows()) throw new Exception("not equal");
                find = fsql.Select<tuint256tb_01>().Where(a => a.Id == item.Id).ToList();
                if (find.Count != 1) throw new Exception("not single");
                if (item.Number != find[0].Number) throw new Exception("not equal");
                num = num - 1;
                item.Number = num;
                if (1 != fsql.Update<tuint256tb_01>().SetSource(item).ExecuteAffrows()) throw new Exception("not equal");
                find = fsql.Select<tuint256tb_01>().Where(a => a.Id == item.Id).ToList();
                if (find.Count != 1) throw new Exception("not single");
                if ("57896044618658097711785492504343953926634992332820282019728792003956564819967" != find[0].Number.ToString()) throw new Exception("not equal");

                num = BigInteger.Parse("57896044618658097711785492504343953926634992332820282019728792003956564819968");
                fsql.Delete<tuint256tb_01>().Where("1=1").ExecuteAffrows();
                if (1 != fsql.Insert(new tuint256tb_01()).NoneParameter().ExecuteAffrows()) throw new Exception("not equal");
                find = fsql.Select<tuint256tb_01>().ToList();
                if (find.Count != 1) throw new Exception("not single");
                if ("0" != find[0].Number.ToString()) throw new Exception("not equal");
                item = new tuint256tb_01 { Number = num };
                if (1 != fsql.Insert(item).NoneParameter().ExecuteAffrows()) throw new Exception("not equal");
                find = fsql.Select<tuint256tb_01>().Where(a => a.Id == item.Id).ToList();
                if (find.Count != 1) throw new Exception("not single");
                if (item.Number != find[0].Number) throw new Exception("not equal");
                num = num - 1;
                item.Number = num;
                if (1 != fsql.Update<tuint256tb_01>().NoneParameter().SetSource(item).ExecuteAffrows()) throw new Exception("not equal");
                find = fsql.Select<tuint256tb_01>().Where(a => a.Id == item.Id).ToList();
                if (find.Count != 1) throw new Exception("not single");
                if ("57896044618658097711785492504343953926634992332820282019728792003956564819967" != find[0].Number.ToString()) throw new Exception("not equal");
            }



            fsql.Aop.CommandBefore += (_, e) =>
            {
                if (CommandTimeoutCascade._asyncLocalTimeout.Value > 0)
                    e.Command.CommandTimeout = CommandTimeoutCascade._asyncLocalTimeout.Value;
            };

            using (new CommandTimeoutCascade(1000))
            {
                fsql.Select<Order>().ToList();
                fsql.Select<Order>().ToList();
                fsql.Select<Order>().ToList();
            }


            var sql1 = fsql.Select<User1, UserGroup>()
                .RawJoin("FULL JOIN UserGroup b ON b.id = a.GroupId")
                .Where((a, b) => a.IsDeleted == false)
                .ToSql((a, b) => new
                {
                    user = a,
                    group = b
                });
            sql1 = sql1.Replace("INNER JOIN ", "FULL JOIN ");

            var tinc01 = fsql.Select<UserGroup>().IncludeMany(a => a.User1s.Where(b => b.GroupId == a.Id)).ToList();

            fsql.CodeFirst.IsGenerateCommandParameterWithLambda = true;
            var tsub01 = fsql.Select<UserGroup>()
                .ToList(a => new
                {
                    users1 = fsql.Select<User1>().Where(b => b.GroupId == a.Id).ToList(),
                    users2 = fsql.Select<User1>().Where(b => b.GroupId == a.Id).ToList(b => new
                    {
                        userid = b.Id,
                        username = b.Username,
                        users11 = fsql.Select<User1>().Where(c => c.GroupId == a.Id).ToList(),
                        users22 = fsql.Select<User1>().Where(c => c.GroupId == a.Id).ToList(c => new
                        {
                            userid = c.Id,
                            username = c.Username,
                        }),
                        groups11 = fsql.Select<UserGroup>().Where(c => c.Id == b.GroupId).ToList(),
                        groups22 = fsql.Select<UserGroup>().Where(c => c.Id == b.GroupId).ToList(c => new
                        {
                            c.Id,
                            c.GroupName,
                            username = b.Username,
                        })
                    }),
                    users3 = fsql.Select<User1>().Limit(10).ToList(),
                    users4 = fsql.Select<User1>().Limit(10).ToList(b => new
                    {
                        userid = b.Id,
                        username = b.Username
                    })
                    //users3 = fsql.Ado.Query<User1>("select * from user1 where groupid = @id", new { id = a.Id })
                });
            var typs = fsql.Select<User1>();
            var typs2 = typeof(ISelect<User1>).GetInterfaces().SelectMany(a => a.GetMethods().Where(b => b.Name == "ToListAsync")).ToArray();

            var tsub02 = fsql.Select<UserGroup>()
                .ToListAsync(a => new
                {
                    users1 = fsql.Select<User1>().Where(b => b.GroupId == a.Id).ToList(),
                    users2 = fsql.Select<User1>().Where(b => b.GroupId == a.Id).ToList(b => new
                    {
                        userid = b.Id,
                        username = b.Username,
                        users11 = fsql.Select<User1>().Where(c => c.GroupId == a.Id).ToList(),
                        users22 = fsql.Select<User1>().Where(c => c.GroupId == a.Id).ToList(c => new
                        {
                            userid = c.Id,
                            username = c.Username,
                        }),
                        groups11 = fsql.Select<UserGroup>().Where(c => c.Id == b.GroupId).ToList(),
                        groups22 = fsql.Select<UserGroup>().Where(c => c.Id == b.GroupId).ToList(c => new
                        {
                            c.Id,
                            c.GroupName,
                            username = b.Username,
                        })
                    }),
                    users3 = fsql.Select<User1>().Limit(10).ToList(),
                    users4 = fsql.Select<User1>().Limit(10).ToList(b => new
                    {
                        userid = b.Id,
                        username = b.Username
                    })
                    //users3 = fsql.Ado.Query<User1>("select * from user1 where groupid = @id", new { id = a.Id })
                }).Result;



            fsql.UseJsonMap();

            //var txt1 = fsql.Ado.Query<(string, string)>("select '꧁꫞꯭丑小鸭꫞꧂', '123123中国人' from dual");

            fsql.Insert(new Order { ShippingAddress = "'꧁꫞꯭丑小鸭꫞꧂'" }).ExecuteAffrows();
            fsql.Insert(new Order { ShippingAddress = "'123123中国人'" }).ExecuteAffrows();
            var lst1 = fsql.Select<Order>().ToList();

            var lst2 = fsql.Select<Order>().ToListIgnore(a => new
            {
                a.ShippingAddress
            });

            fsql.Delete<TopicMapTypeToListDto>().Where("1=1").ExecuteAffrows();
            fsql.Insert(new[]
            {
                new TopicMapTypeToListDto{
                    Clicks = 100,
                    Title = "testMapTypeTitle1",
                    CouponIds = new List<int> { 1, 2, 3, 4 }
                },
                new TopicMapTypeToListDto{
                    Clicks = 101,
                    Title = "testMapTypeTitle2",
                    CouponIds = new List<int> { 1, 2, 3, 1 }
                },
                new TopicMapTypeToListDto{
                    Clicks = 102,
                    Title = "testMapTypeTitle3",
                    CouponIds = new List<int> { 1 }
                },
                new TopicMapTypeToListDto{
                    Clicks = 103,
                    Title = "testMapTypeTitle4",
                    CouponIds = new List<int>()
                },
                new TopicMapTypeToListDto{
                    Clicks = 103,
                    Title = "testMapTypeTitle5",
                },
            }).ExecuteAffrows();
            var dtomaplist2 = fsql.Select<TopicMapTypeToListDto>().ToList<TopicMapTypeToListDtoMap>();
            var dtomaplist22 = fsql.Select<TopicMapTypeToListDto>().ToList<TopicMapTypeToListDtoMap2>();
            var dtomaplist0 = fsql.Select<TopicMapTypeToListDto>().ToList();
            var dtomaplist1 = fsql.Select<TopicMapTypeToListDto>().ToList(a => new TopicMapTypeToListDtoMap
            {
                CouponIds = a.CouponIds
            });

            int LocalConcurrentDictionaryIsTypeKey(Type dictType, int level = 1)
            {
                if (dictType.IsGenericType == false) return 0;
                if (dictType.GetGenericTypeDefinition() != typeof(ConcurrentDictionary<,>)) return 0;
                var typeargs = dictType.GetGenericArguments();
                if (typeargs[0] == typeof(Type) || typeargs[0] == typeof(ColumnInfo) || typeargs[0] == typeof(TableInfo)) return level;
                if (level > 2) return 0;
                return LocalConcurrentDictionaryIsTypeKey(typeargs[1], level + 1);
            }

            var fds = typeof(FreeSql.Internal.Utils).GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(a => LocalConcurrentDictionaryIsTypeKey(a.FieldType) > 0).ToArray();
            var ttypes1 = typeof(IFreeSql).Assembly.GetTypes().Select(a => new
            {
                Type = a,
                ConcurrentDictionarys = a.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(b => LocalConcurrentDictionaryIsTypeKey(b.FieldType) > 0).ToArray()
            }).Where(a => a.ConcurrentDictionarys.Length > 0).ToArray();
            var ttypes2 = typeof(IBaseRepository).Assembly.GetTypes().Select(a => new
            {
                Type = a,
                ConcurrentDictionarys = a.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(b => LocalConcurrentDictionaryIsTypeKey(b.FieldType) > 0).ToArray()
            }).Where(a => a.ConcurrentDictionarys.Length > 0).ToArray();

            #region pgsql poco
            //fsql.Aop.ParseExpression += (_, e) =>
            //{
            //    //解析 POCO Jsonb   a.Customer.Name
            //    if (e.Expression is MemberExpression memExp)
            //    {
            //        var parentMemExps = new Stack<MemberExpression>();
            //        parentMemExps.Push(memExp);
            //        while (true)
            //        {
            //            switch (memExp.Expression.NodeType)
            //            {
            //                case ExpressionType.MemberAccess:
            //                    memExp = memExp.Expression as MemberExpression;
            //                    if (memExp == null) return;
            //                    parentMemExps.Push(memExp);
            //                    break;
            //                case ExpressionType.Parameter:
            //                    var tb = fsql.CodeFirst.GetTableByEntity(memExp.Expression.Type);
            //                    if (tb == null) return;
            //                    if (tb.ColumnsByCs.TryGetValue(parentMemExps.Pop().Member.Name, out var trycol) == false) return;
            //                    if (new[] { typeof(JToken), typeof(JObject), typeof(JArray) }.Contains(trycol.Attribute.MapType.NullableTypeOrThis()) == false) return;
            //                    var tmpcol = tb.ColumnsByPosition.OrderBy(a => a.Attribute.Name.Length).First();
            //                    var result = e.FreeParse(Expression.MakeMemberAccess(memExp.Expression, tb.Properties[tmpcol.CsName]));
            //                    result = result.Replace(tmpcol.Attribute.Name, trycol.Attribute.Name);
            //                    while (parentMemExps.Any())
            //                    {
            //                        memExp = parentMemExps.Pop();
            //                        result = $"{result}->>'{memExp.Member.Name}'";
            //                    }
            //                    e.Result = result;
            //                    return;
            //            }
            //        }
            //    }
            //};

            //void RegisterPocoType(Type pocoType)
            //{
            //    var methodJsonConvertDeserializeObject = typeof(JsonConvert).GetMethod("DeserializeObject", new[] { typeof(string), typeof(Type) });
            //    var methodJsonConvertSerializeObject = typeof(JsonConvert).GetMethod("SerializeObject", new[] { typeof(object), typeof(JsonSerializerSettings) });
            //    var jsonConvertSettings = JsonConvert.DefaultSettings?.Invoke() ?? new JsonSerializerSettings();
            //    FreeSql.Internal.Utils.dicExecuteArrayRowReadClassOrTuple[pocoType] = true;
            //    FreeSql.Internal.Utils.GetDataReaderValueBlockExpressionObjectToStringIfThenElse.Add((LabelTarget returnTarget, Expression valueExp, Expression elseExp, Type type) =>
            //    {
            //        return Expression.IfThenElse(
            //            Expression.TypeIs(valueExp, pocoType),
            //            Expression.Return(returnTarget, Expression.Call(methodJsonConvertSerializeObject, Expression.Convert(valueExp, typeof(object)), Expression.Constant(jsonConvertSettings)), typeof(object)),
            //            elseExp);
            //    });
            //    FreeSql.Internal.Utils.GetDataReaderValueBlockExpressionSwitchTypeFullName.Add((LabelTarget returnTarget, Expression valueExp, Type type) =>
            //    {
            //        if (type == pocoType) return Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(methodJsonConvertDeserializeObject, Expression.Convert(valueExp, typeof(string)), Expression.Constant(type)), type));
            //        return null;
            //    });
            //}

            //var seid = fsql.Insert(new SomeEntity
            //{
            //    Customer = JsonConvert.DeserializeObject<Customer>(@"{
            //    ""Age"": 25,
            //    ""Name"": ""Joe"",
            //    ""Orders"": [
            //        { ""OrderPrice"": 9, ""ShippingAddress"": ""Some address 1"" },
            //        { ""OrderPrice"": 23, ""ShippingAddress"": ""Some address 2"" }
            //    ]
            //}")
            //}).ExecuteIdentity();
            //var selist = fsql.Select<SomeEntity>().ToList();

            //var joes = fsql.Select<SomeEntity>()
            //    .Where(e => e.Customer.Name == "Joe")
            //    .ToSql();
            #endregion

            

            fsql.Aop.AuditValue += new EventHandler<FreeSql.Aop.AuditValueEventArgs>((_, e) =>
            {

            });




            for (var a = 0; a < 10000; a++)
                fsql.Select<User1>().First();

            for (var a = 0; a < 1000; a++)
            {
                fsql.Transaction(() =>
                {
                    var tran = fsql.Ado.TransactionCurrentThread;
                    tran.Rollback();
                });
            }

            fsql.UseJsonMap();
            var bid1 = 10;
            var list1 = fsql.Select<A>()
                .Where(a => a.BId == bid1);
            var aid1 = 11;
            var select2 = fsql.Select<B>();
            (select2 as Select0Provider)._params = (list1 as Select0Provider)._params;
            var list2 = select2
                .Where(a => list1.ToList(B => B.BId).Contains(a.Id))
                .Where(a => a.Id == aid1)
                .ToSql();

            //fsql.Aop.CommandBefore += (s, e) =>
            //{
            //    e.States["xxx"] = 111;
            //};
            //fsql.Aop.CommandAfter += (s, e) =>
            //{
            //    var xxx = e.States["xxx"];
            //};

            //fsql.Aop.TraceBefore += (s, e) =>
            //{
            //    e.States["xxx"] = 222;
            //};
            //fsql.Aop.TraceAfter += (s, e) =>
            //{
            //    var xxx = e.States["xxx"];
            //};

            //fsql.Aop.SyncStructureBefore += (s, e) =>
            //{
            //    e.States["xxx"] = 333;
            //};
            //fsql.Aop.SyncStructureAfter += (s, e) =>
            //{
            //    var xxx = e.States["xxx"];
            //};

            //fsql.Aop.CurdBefore += (s, e) =>
            //{
            //    e.States["xxx"] = 444;
            //};
            //fsql.Aop.CurdAfter += (s, e) =>
            //{
            //    var xxx = e.States["xxx"];
            //};

            fsql.Insert(new tttorder("xx1", 1, 10)).ExecuteAffrows();
            fsql.Insert(new tttorder("xx2", 2, 20)).ExecuteAffrows();

            var tttorders = fsql.Select<tttorder>().Limit(2).ToList();

            var tsql1 = fsql.Select<Sys_reg_user>()
                .Include(a => a.Owner)
                .Where(a => a.UnionId == "xxx")
                .ToSql();
            var tsql2 = fsql.Select<Sys_owner>()
                .Where(a => a.RegUser.UnionId == "xxx2")
                .ToSql();


            var names = (fsql.Select<object>() as Select0Provider)._commonUtils.SplitTableName("`Backups.ProductStockBak`");


            var dbparams = fsql.Ado.GetDbParamtersByObject(new { id = 1, name = "xxx" });





            var sql = fsql.CodeFirst.GetComparisonDDLStatements(typeof(EMSServerModel.Model.User), "testxsx001");

            var test01 = EMSServerModel.Model.User.Select.IncludeMany(a => a.Roles).ToList();
            var test02 = EMSServerModel.Model.UserRole.Select.ToList();
            var test01tb = EMSServerModel.Model.User.Orm.CodeFirst.GetTableByEntity(typeof(EMSServerModel.Model.User));

            var us = User1.Select.Limit(10).ToList();

            new Products { title = "product-1" }.Save();
            new Products { title = "product-2" }.Save();
            new Products { title = "product-3" }.Save();
            new Products { title = "product-4" }.Save();
            new Products { title = "product-5" }.Save();

            var wdy1 = JsonConvert.DeserializeObject<DynamicFilterInfo>(@"
{
  ""Logic"" : ""And"",
  ""Filters"" :
  [
    {
      ""Logic"" : ""Or"",
      ""Filters"" :
      [
        {
          ""Field"" : ""title"",
          ""Operator"" : ""contains"",
          ""Value"" : """",
        },
        {
          ""Field"" : ""title"",
          ""Operator"" : ""contains"",
          ""Value"" : ""product-2222"",
        }
      ]
    },
    {
      ""Field"" : ""title"",
      ""Operator"" : ""eq"",
      ""Value"" : ""product-2""
    },
    {
      ""Field"" : ""title"",
      ""Operator"" : ""eq"",
      ""Value"" : ""product-3""
    },
    {
      ""Field"" : ""title"",
      ""Operator"" : ""eq"",
      ""Value"" : ""product-4""
    },
    {
      ""Field"" : ""testint"",
      ""Operator"" : ""Range"",
      ""Value"" : [100,200]
    },
    {
      ""Field"" : ""testint"",
      ""Operator"" : ""Range"",
      ""Value"" : [""101"",""202""]
    },
    {
      ""Field"" : ""testint"",
      ""Operator"" : ""contains"",
      ""Value"" : ""123""
    },
  ]
}
");
            var config = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = null,
                AllowTrailingCommas = true,
                IgnoreNullValues = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters = { new JsonStringEnumConverter() }
            };
            var wdy2 = System.Text.Json.JsonSerializer.Deserialize<DynamicFilterInfo>(@"
{
  ""Logic"" : 1,
  ""Filters"" :
  [
    {
      ""Field"" : ""title"",
      ""Operator"" : 8,
      ""Value"" : ""product-1"",
      ""Filters"" :
      [
        {
          ""Field"" : ""title"",
          ""Operator"" : 0,
          ""Value"" : ""product-1111""
        }
      ]
    },
    {
      ""Field"" : ""title"",
      ""Operator"" : 8,
      ""Value"" : ""product-2""
    },
    {
      ""Field"" : ""title"",
      ""Operator"" : 8,
      ""Value"" : ""product-3""
    },
    {
      ""Field"" : ""title"",
      ""Operator"" : 8,
      ""Value"" : ""product-4""
    },
    {
      ""Field"" : ""testint"",
      ""Operator"" : 8,
      ""Value"" : 11
    },
    {
      ""Field"" : ""testint"",
      ""Operator"" : 8,
      ""Value"" : ""12""
    },
    {
      ""Field"" : ""testint"",
      ""Operator"" : ""Range"",
      ""Value"" : [100,200]
    },
    {
      ""Field"" : ""testint"",
      ""Operator"" : ""Range"",
      ""Value"" : [""101"",""202""]
    }
  ]
}
", config);
            Products.Select.WhereDynamicFilter(wdy1).ToList();
            Products.Select.WhereDynamicFilter(wdy2).ToList();

            var items1 = Products.Select.Limit(10).OrderByDescending(a => a.CreateTime).ToList();
            var items2 = fsql.Select<Products>().Limit(10).OrderByDescending(a => a.CreateTime).ToList();

            BaseEntity.Orm.UseJsonMap();
            BaseEntity.Orm.UseJsonMap();
            BaseEntity.Orm.CodeFirst.ConfigEntity<S_SysConfig<TestConfig>>(a =>
            {
                a.Property(b => b.Config2).JsonMap();
            });

            new S_SysConfig<TestConfig> { Name = "testkey11", Config = new TestConfig { clicks = 11, title = "testtitle11" }, Config2 = new TestConfig { clicks = 11, title = "testtitle11" } }.Save();
            new S_SysConfig<TestConfig> { Name = "testkey22", Config = new TestConfig { clicks = 22, title = "testtitle22" }, Config2 = new TestConfig { clicks = 11, title = "testtitle11" } }.Save();
            new S_SysConfig<TestConfig> { Name = "testkey33", Config = new TestConfig { clicks = 33, title = "testtitle33" }, Config2 = new TestConfig { clicks = 11, title = "testtitle11" } }.Save();
            var testconfigs11 = S_SysConfig<TestConfig>.Select.ToList();
            var testconfigs11tb = S_SysConfig<TestConfig>.Select.ToDataTable();
            var testconfigs111 = S_SysConfig<TestConfig>.Select.ToList(a => a.Name);
            var testconfigs112 = S_SysConfig<TestConfig>.Select.ToList(a => a.Config);
            var testconfigs1122 = S_SysConfig<TestConfig>.Select.ToList(a => new { a.Name, a.Config });
            var testconfigs113 = S_SysConfig<TestConfig>.Select.ToList(a => a.Config2);
            var testconfigs1133 = S_SysConfig<TestConfig>.Select.ToList(a => new { a.Name, a.Config2 });

            var repo = BaseEntity.Orm.Select<TestConfig>().Limit(10).ToList();


            //void ConfigEntityProperty(object sender, FreeSql.Aop.ConfigEntityPropertyEventArgs e)
            //{
            //    if (e.Property.PropertyType == typeof(byte[]))
            //    {
            //        var orm = sender as IFreeSql;
            //        switch (orm.Ado.DataType)
            //        {
            //            case DataType.SqlServer:
            //                e.ModifyResult.DbType = "image";
            //                break;
            //            case DataType.MySql:
            //                e.ModifyResult.DbType = "longblob";
            //                break;
            //        }
            //    }
            //}
            //fsql.Aop.ConfigEntityProperty += ConfigEntityProperty;






            Console.WriteLine("按任意键结束。。。");
            Console.ReadKey();
        }

        static void InitData()
        {
            Task.Run(async () =>
            {
                using (var uow = BaseEntity.Orm.CreateUnitOfWork())
                {
                    _asyncUow.Value = uow;
                    try
                    {
                        var id = (await new User1().SaveAsync()).Id;
                    }
                    finally
                    {
                        _asyncUow.Value = null;
                    }
                    uow.Commit();
                }

                var ug1 = new UserGroup();
                ug1.GroupName = "分组一";
                await ug1.InsertAsync();

                var ug2 = new UserGroup();
                ug2.GroupName = "分组二";
                await ug2.InsertAsync();

                var u1 = new User1();

                u1.GroupId = ug1.Id;
                await u1.SaveAsync();

                await u1.DeleteAsync();
                await u1.RestoreAsync();

                u1.Nickname = "x1";
                await u1.UpdateAsync();

                var u11 = await User1.FindAsync(u1.Id);
                u11.Description = "备注";
                await u11.SaveAsync();

                await u11.DeleteAsync();

                var slslsl = Newtonsoft.Json.JsonConvert.SerializeObject(u1);
                var u11null = User1.Find(u1.Id);

                var u11s = User1.Where(a => a.Group.Id == ug1.Id).Limit(10).ToList();

                var u11s2 = User1.Select.LeftJoin<UserGroup>((a, b) => a.GroupId == b.Id).Limit(10).ToList();

                var ug1s = UserGroup.Select
                    .IncludeMany(a => a.User1s)
                    .Limit(10).ToList();

                var ug1s2 = UserGroup.Select.Where(a => a.User1s.AsSelect().Any(b => b.Nickname == "x1")).Limit(10).ToList();

                var r1 = new Role();
                r1.Id = "管理员";
                await r1.SaveAsync();

                var r2 = new Role();
                r2.Id = "超级会员";
                await r2.SaveAsync();

                var ru1 = new RoleUser1();
                ru1.User1Id = u1.Id;
                ru1.RoleId = r1.Id;
                await ru1.SaveAsync();

                ru1.RoleId = r2.Id;
                await ru1.SaveAsync();

                var u1roles = await User1.Select.IncludeMany(a => a.Roles).ToListAsync();
                var u1roles2 = await User1.Select.Where(a => a.Roles.AsSelect().Any(b => b.Id == "xx")).ToListAsync();

            }).Wait();
        }

        public static List<T1> ToListIgnore<T1>(this ISelect<T1> that, Expression<Func<T1, object>> selector)
        {
            if (selector == null) return that.ToList();
            var s0p = that as Select0Provider;
            var tb = s0p._tables[0];
            var parmExp = tb.Parameter ?? Expression.Parameter(tb.Table.Type, tb.Alias);
            var initExps = tb.Table.Columns.Values
                .Where(a => a.Attribute.IsIgnore == false)
                .Select(a => new
                {
                    exp = Expression.Bind(tb.Table.Properties[a.CsName], Expression.MakeMemberAccess(parmExp, tb.Table.Properties[a.CsName])),
                    ignored = TestMemberExpressionVisitor.IsExists(selector, Expression.MakeMemberAccess(parmExp, tb.Table.Properties[a.CsName]))
                })
                .Where(a => a.ignored == false)
                .Select(a => a.exp)
                .ToArray();
            var lambda = Expression.Lambda<Func<T1, T1>>(
                Expression.MemberInit(
                    Expression.New(tb.Table.Type),
                    initExps
                ),
                parmExp
            );
            return that.ToList(lambda);
        }
        class TestMemberExpressionVisitor : ExpressionVisitor
        {
            public string MemberExpString;
            public bool Result { get; private set; }

            public static bool IsExists(Expression selector, Expression memberExp)
            {
                var visitor = new TestMemberExpressionVisitor { MemberExpString = memberExp.ToString() };
                visitor.Visit(selector);
                return visitor.Result;
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                if (!Result && node.ToString() == MemberExpString) Result = true;
                return node;
            }
        }
    }

    public class 抖店实时销售金额表
    {
        /// <summary>
        /// ID
        /// </summary>
        [Column(Name = "ID", IsPrimary = true)]
        public int ID { get; set; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        [Column(Name = "店铺名称")]
        public string 店铺名称 { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        [Column(Name = "日期")]
        public DateTime 日期 { get; set; }

        /// <summary>
        /// 品牌名称
        /// </summary>
        [Column(Name = "品牌名称")]
        public string 品牌名称 { get; set; }

        /// <summary>
        /// 成交金额
        /// </summary>
        [Column(Name = "成交金额")]
        public decimal? 成交金额 { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column(Name = "更新时间", CanInsert = false, CanUpdate = true, ServerTime = DateTimeKind.Local)]
        public DateTime 更新时间 { get; set; }
    }

    abstract class BaseDataEntity
    {
        public Guid Id { get; set; }
        public virtual int CategoryId { get; set; }
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
    }
    [Table(Name = "`bdd_1`")]
    class GoodsData : BaseDataEntity
    {
        public override Int32 CategoryId { get; set; }
        public override string Code { get; set; }
        public override string Name { get; set; }
    }
    class GoodsDataDTO
    {
        public Guid Id { get; set; }
        public int CategoryId { get; set; }
        public string GoodsNo { get; set; }
        public string GoodsName { get; set; }
    }
}
