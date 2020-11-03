using FreeSql.DataAnnotations;
using FreeSql;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FreeSql.Tests.Odbc
{
    public static class SqlFunc
    {
        public static T TryTo<T>(this string that)
        {
            return (T)Internal.Utils.GetDataReaderValue(typeof(T), that);
        }

        public static string FormatDateTime()
        {
            return "";
        }
    }

    public class UnitTest1
    {

        public class Order
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public string OrderTitle { get; set; }
            public string CustomerName { get; set; }
            public DateTime TransactionDate { get; set; }
            public virtual List<OrderDetail> OrderDetails { get; set; }
        }
        public class OrderDetail
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }

            public int OrderId { get; set; }
            public virtual Order Order { get; set; }
        }

        ISelect<TestInfo> select => g.mysql.Select<TestInfo>();

        class TestUser
        {
            [Column(IsIdentity = true)]
            public int stringid { get; set; }
            public string accname { get; set; }
            public LogUserOn LogOn { get; set; }

        }
        class LogUserOn
        {
            public int id { get; set; }
            public int userstrId { get; set; }
        }

        class ServiceRequestNew
        {
            public Guid id { get; set; }
            public string acptStaffDeptId { get; set; }
            public DateTime acptTime { get; set; }
            public int crtWrkfmFlag { get; set; }
            [Column(DbType = "nvarchar2(1500)")]
            public string srvReqstCntt { get; set; }
        }

        public class Model1
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }

            public string title { get; set; }

            public ICollection<Model2> Childs { get; set; }

            public int M2Id { get; set; }

            [Column(IsIgnore = true)]
            public List<Model1> TestManys { get; set; }

        }

        public class Model2
        {

            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string Title { get; set; }

            public Model1 Parent { get; set; }
            public int Parent_id { get; set; }

            public string Ccc { get; set; }
            public DateTime Date { get; set; }

            [Column(Name = "Waxxx2")]
            public int Wa_xxx2 { get; set; }
        }

        public class TestEnumable : IEnumerable<TestEnumable>
        {
            public IEnumerator<TestEnumable> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        public class TestEnum
        {
            public Guid id { get; set; }
            public Enum em { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        [Table(Name = "news_article")]
        public class NewsArticle
        {
            /// <summary>
            /// 
            /// </summary>        
            [Column(Name = "article_id", IsIdentity = true, IsPrimary = true)]
            public int ArticleId { get; set; }

            /// <summary>
            /// 
            /// </summary>        
            [Column(Name = "article_title")]
            public string ArticleTitle { get; set; }

            /// <summary>
            /// 
            /// </summary>        
            [Column(Name = "category_id")]
            public int CategoryId { get; set; }

            /// <summary>
            /// 
            /// </summary>        
            [Column(Name = "channel_id")]
            public int ChannelId { get; set; }

            /// <summary>
            /// 类型
            /// </summary>        
            [Column(Name = "type_id")]
            public int TypeId { get; set; }

            /// <summary>
            /// 内容简介
            /// </summary>        
            [Column(Name = "summary")]
            public string Summary { get; set; }

            /// <summary>
            /// 缩略图
            /// </summary>        
            [Column(Name = "thumbnail")]
            public string Thumbnail { get; set; }

            /// <summary>
            /// 点击量
            /// </summary>        
            [Column(Name = "hits")]
            public int Hits { get; set; }

            /// <summary>
            /// 
            /// </summary>        
            [Column(Name = "is_display")]
            public int IsDisplay { get; set; }

            /// <summary>
            /// 
            /// </summary>        
            [Column(Name = "status")]
            public int Status { get; set; }

            /// <summary>
            /// 
            /// </summary>        
            [Column(Name = "create_time")]
            public int CreateTime { get; set; }

            /// <summary>
            /// 
            /// </summary>        
            [Column(Name = "release_time")]
            public int ReleaseTime { get; set; }

            public DateTime testaddtime { get; set; }
            public DateTime? testaddtime2 { get; set; }
        }

        public class NewsArticleDto : NewsArticle
        {

        }


        public class TaskBuildInfo
        {
            [FreeSql.DataAnnotations.Column(IsPrimary = true)]
            public Guid Id { get; set; }
            public Guid TbId { get; set; }
            public Guid DataBaseConfigId { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }

            [Column(IsIgnore = true)]
            public virtual TaskBuild TaskBuild { get; set; }
        }
        public class Templates
        {
            /// <summary>
            /// 测试中文重命名id
            /// </summary>
            [Column(IsPrimary = true, OldName = "Id")]
            public Guid Id2 { get; set; }
            public string Title { get; set; }
            public DateTime AddTime { get; set; } = DateTime.Now;
            public DateTime EditTime { get; set; }
            public string Code { get; set; }
        }
        public class TaskBuild
        {

            [FreeSql.DataAnnotations.Column(IsPrimary = true)]
            public Guid Id { get; set; }
            public string TaskName { get; set; }
            public Guid TemplatesId { get; set; }
            public string GeneratePath { get; set; }
            public string FileName { get; set; }
            public string NamespaceName { get; set; }
            public bool OptionsEntity01 { get; set; } = false;
            public bool OptionsEntity02 { get; set; } = false;
            public bool OptionsEntity03 { get; set; } = false;
            public int OptionsEntity04 { get; set; }
            public int? score { get; set; }

            [Navigate("TbId")]
            public virtual ICollection<TaskBuildInfo> Builds { get; set; }
            public Templates Templates { get; set; }

            public Guid ParentId { get; set; }
            public TaskBuild Parent { get; set; }
        }

        

        void parseExp(object sender, Aop.ParseExpressionEventArgs e)
        {
            if (e.Expression.NodeType == ExpressionType.Call)
            {
                var callExp = e.Expression as MethodCallExpression;
                if (callExp.Object == null && callExp.Arguments.Any() && callExp.Arguments[0].Type == typeof(string))
                {
                    if (callExp.Method.Name == "TryTo")
                    {
                        e.Result = Expression.Lambda(
                            typeof(Func<>).MakeGenericType(callExp.Method.GetGenericArguments().FirstOrDefault()), 
                            e.Expression).Compile().DynamicInvoke()?.ToString();
                        return;
                    }
                }
            }
        }

        public class Class1
        {
            [Column(IsIdentity = true)]
            public long ID { set; get; }

            [Column(IsIdentity = true, OldName = "stu_id_log")]
            public long stu_id { set; get; }
            /// <summary>
            /// 姓名
            /// </summary>
            public string name { set; get; }

            public int age { set; get; }

            public DateTime class2 { set; get; }
        }

        class TestDto
        {
            public Guid Id { get; set; }
            public bool IsLeaf { get; set; }
        }

        public static int GetUNIX_TIMESTAMP() => 0;


        [Table(Name = "bz_web_post")]
        public class Post
        {
            public int Id { get; set; }
            public int AuthorId { get; set; }
            [Navigate("AuthorId")]
            public AuthorTest Author { get; set; }
        }
        [Table(Name = "bz_web_authortest")]
        public class AuthorTest
        {
            public int Id { get; set; }
            public string Name { get; set; }
            [Navigate("AuthorId")]
            public List<Post> Post { get; set; }
        }

        [Fact]
        public void Test1()
        {

            using (var conn = g.oracle.Ado.MasterPool.Get()) {
                var cmd = conn.Value.CreateCommand();
                cmd.CommandText = @"SELECT a.""ID"" as1, a__Type.""NAME"" as2
FROM ""TB_TOPIC22"" a 
LEFT JOIN ""TESTTYPEINFO"" a__Type ON a__Type.""GUID"" = a.""TYPEGUID"" 
WHERE ROWNUM < 11";
                var dr = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);

                var eof = dr.Read();

                dr.Close();
            }


            var ttt = g.oracle.Select<Post>().ToList();


            var testrunsql1 = g.mysql.Select<TaskBuild>().Where(a => a.OptionsEntity04 > DateTime.Now.AddDays(0).ToString("yyyyMMdd").TryTo<int>()).ToSql();
            var testrunsql3 = g.sqlserver.Select<TaskBuild>().Where(a => a.OptionsEntity04 > DateTime.Now.AddDays(0).ToString("yyyyMMdd").TryTo<int>()).ToSql();
            var testrunsql4 = g.oracle.Select<TaskBuild>().Where(a => a.OptionsEntity04 > DateTime.Now.AddDays(0).ToString("yyyyMMdd").TryTo<int>()).ToSql();

            var testformatsql1 = g.mysql.Select<TaskBuild>().Where(a => a.NamespaceName == $"1_{10100}").ToSql();
            var testorderbysql = g.mysql.Select<TaskBuild>().OrderByDescending(a => a.OptionsEntity04 + (a.score ?? 0)).ToSql();

            var floorSql1 = g.mysql.Select<TaskBuild>().Where(a => a.OptionsEntity04 / 10000 == 121212 / 10000).ToSql();
            var floorSql2 = g.mysql.Select<TaskBuild>().Where(a => a.OptionsEntity04 / 10000.0 == 121212 / 10000).ToSql();

            var testBoolSql1 = g.sqlserver.Select<TaskBuild>().Where(a => a.OptionsEntity01).ToSql();
            var testBoolSql2 = g.sqlserver.Select<TaskBuild>().Where(a => a.Id == Guid.NewGuid() && a.OptionsEntity01).ToSql();

            //g.mysql.Aop.AuditValue += (s, e) =>
            //{
            //    if (e.Column.CsType == typeof(long)
            //        && e.Property.GetCustomAttribute<KeyAttribute>(false) != null
            //        && e.Value?.ToString() == "0")
            //        e.Value = new Random().Next();
            //};
            //g.mysql.GetRepository<SystemUser>().Insert(GetSystemUser());

            g.mysql.Aop.ParseExpression += new EventHandler<Aop.ParseExpressionEventArgs>((s, e) =>
            {
                if (e.Expression.NodeType == ExpressionType.Call && (e.Expression as MethodCallExpression).Method.Name == "GetUNIX_TIMESTAMP")
                    e.Result = "UNIX_TIMESTAMP(NOW())";
            });
            var dkkdksdjgj = g.mysql.Select<TaskBuild>().Where(a => a.OptionsEntity04 > GetUNIX_TIMESTAMP()).ToSql();

            var dt1970 = new DateTime(1970, 1, 1);
            var dkkdksdjgj22 = g.mysql.Select<TaskBuild>().Where(a => a.OptionsEntity04 > DateTime.Now.Subtract(dt1970).TotalSeconds).ToSql();


            var xxxhzytuple = g.sqlserver.Select<Templates, TaskBuild>()
                .Where(a => a.t1.Code == "xxx" && a.t2.OptionsEntity03 == true)
                .ToSql();


            var xxxkdkd = g.oracle.Select<Templates, TaskBuild>()
                .InnerJoin((a,b) => true)
                .Where((a,b) => (DateTime.Now - a.EditTime).TotalMinutes > 100)
                .OrderBy((a,b) => g.oracle.Select<Templates>().Where(c => b.Id == c.Id2).Count())
                .ToSql();
            

            g.oracle.Aop.SyncStructureAfter += (s, e) => 
                Trace.WriteLine(e.Sql);

            g.oracle.CodeFirst.SyncStructure<Class1>();

            //g.mysql.Aop.ParseExpression += parseExp;

            //var sqdddd2 = g.mysql.Select<TaskBuild>().ToSql(t => t.OptionsEntity04 == t.NamespaceName.TryTo<int>());

            var sqksdkfjl = g.mysql.Select<TaskBuild>()
                .LeftJoin(a => a.Templates.Id2 == a.TemplatesId)
                .LeftJoin(a => a.Parent.Id == a.Id)
                .LeftJoin(a => a.Parent.Templates.Id2 == a.Parent.TemplatesId)
                .ToSql(a => new
                {
                    code1 = a.Templates.Code,
                    code2 = a.Parent.Templates.Code
                });


            var sqksdkfjl2223 = g.mysql.Select<TaskBuild>().From<TaskBuild, Templates, Templates>((s1, tb2, b1, b2) => s1
                .LeftJoin(a => a.Id == tb2.TemplatesId)
                .LeftJoin(a => a.TemplatesId == b1.Id2)
                .LeftJoin(a => a.TemplatesId == b2.Id2)
            ).ToSql((a, tb2, b1, b2) => new
            {
                code1 = b1.Code,
                code2 = b2.Code
            });

            var tkdkdksql = g.mysql.Select<TaskBuild>().From<Templates, Templates>((a, b, c) =>
                a.LeftJoin(aa => aa.TemplatesId == b.Id2 && b.Code == "xx")
                .LeftJoin(aa => aa.TemplatesId == c.Id2))
                .GroupBy((a, b, c) => new { a.NamespaceName, c.Code })
                .ToSql("a.id");



            var dcksdkdsk = g.mysql.Select<NewsArticle>().Where(a => a.testaddtime2.HasValue).ToSql();
            var dcksdkdsk2 = g.mysql.Select<NewsArticle>().Where(a => !a.testaddtime2.HasValue).ToSql();

            var testgrpsql = g.mysql.Select<TaskBuild>()
                .From<Templates>((a, b) => a.InnerJoin(aa => aa.TemplatesId
                  == b.Id2))
                 .GroupBy((a, b) => b.Code)
                 .ToSql(a => new
                 {
                     a.Key,
                     sss = a.Sum(a.Value.Item1.OptionsEntity04)
                 });

            var testgrpsql2 = g.mysql.Select<TaskBuild>()
                .From<Templates>((a, b) => a.InnerJoin(aa => aa.TemplatesId
                  == b.Id2))
                 .GroupBy((a, b) => b.Code)
                 .ToList(a => new
                 {
                     a.Key,
                     sss = a.Sum(a.Value.Item1.OptionsEntity04)
                 });


            var tbid = g.mysql.Select<TaskBuild>().First()?.Id ?? Guid.Empty;

            var testarray = new[] { 1, 2, 3 };
            var tbidsql1 = g.mysql.Update<TaskBuild>().Where(a => a.Id == tbid)
                .Set(a => new TaskBuild
                {
                    FileName = "111",
                    TaskName = a.TaskName + "333",
                    OptionsEntity02 = false,
                    OptionsEntity04 = testarray[0]
                }).ToSql();
            var tbidsql2 = g.mysql.Update<TaskBuild>().Where(a => a.Id == tbid)
                .Set(a => new
                {
                    FileName = "111",
                    TaskName = a.TaskName + "333",
                    OptionsEntity02 = false,
                    OptionsEntity04 = testarray[0]
                }).ToSql();


            var dkdkdkd = g.oracle.Select<Templates>().ToList();



            //var testaddlist = new List<NewsArticle>();
            //for(var a = 0; a < 133905; a++) {
            //    testaddlist.Add(new NewsArticle {
            //        ArticleTitle = "testaddlist_topic" + a,
            //        Hits = a,
            //    });
            //}
            //g.mysql.Insert<NewsArticle>(testaddlist)
            //    //.NoneParameter()
            //    .ExecuteAffrows();


            g.mysql.Aop.ParseExpression += (s, e) =>
            {
                if (e.Expression.NodeType == ExpressionType.Call)
                {
                    var callExp = e.Expression as MethodCallExpression;
                    if (callExp.Object?.Type == typeof(DateTime) &&
                        callExp.Method.Name == "ToString" &&
                        callExp.Arguments.Count == 1 &&
                        callExp.Arguments[0].Type == typeof(string) &&
                        callExp.Arguments[0].NodeType == ExpressionType.Constant)
                    {
                        var format = (callExp.Arguments[0] as ConstantExpression)?.Value?.ToString();

                        if (string.IsNullOrEmpty(format) == false)
                        {
                            var tmp = e.FreeParse(callExp.Object);

                            switch (format)
                            {
                                case "yyyy-MM-dd HH:mm":
                                    tmp = $"date_format({tmp}, '%Y-%m-%d %H:%i')";
                                    break;
                            }
                            e.Result = tmp;
                        }
                    }
                }
            };

            g.mysql.Select<NewsArticle>().ToList(a => new
            {
                testaddtime = a.testaddtime.ToString("yyyy-MM-dd HH:mm")
            });

            var ttdkdk = g.mysql.Select<NewsArticle>().Where<TaskBuild>(a => a.NamespaceName == "ddd").ToSql();

            var tsqlddd = g.mysql.Select<NewsArticle>().Where(a =>
                g.mysql.Select<TaskBuild>().Where(b => b.NamespaceName == a.ArticleTitle)
                .Where("@id=1", new { id = 1 }).Any()
            ).ToSql();


            g.mysql.SetDbContextOptions(opt => opt.EnableAddOrUpdateNavigateList = true);
            var trepo = g.mysql.GetGuidRepository<TaskBuild>();
            trepo.Insert(new TaskBuild
            {
                TaskName = "tt11",
                Builds = new[] {
                    new TaskBuildInfo {
                         Level = 1,
                         Name = "t111_11"
                    }
                }
            });

            var ttdkdkd = trepo.Select.Where(a => a.Builds.AsSelect().Any()).ToList();

            var list1113233 = trepo.Select.ToList();


            var entity = new NewsArticle
            {
                ArticleId = 1,
                ArticleTitle = "测试标题"
            };
            var where = new NewsArticle
            {
                ArticleId = 1,
                ChannelId = 1,
            };

            g.mysql.Insert(new[] { entity }).ExecuteAffrows();

            var sqldddkdk = g.mysql.Update<NewsArticle>(where)
                .SetSource(entity)
                .UpdateColumns(x => new { x.Status, x.CategoryId, x.ArticleTitle })
                .ToSql();


            var sql1111333 = g.mysql.Update<Model2>()
                .SetSource(new Model2 { id = 1, Title = "xxx", Parent_id = 0 })
                .UpdateColumns(x => new { x.Parent_id, x.Date, x.Wa_xxx2 })
                .NoneParameter()
                .ToSql();


            g.mysql.Insert(new TestEnum { }).ExecuteAffrows();
            var telist = g.mysql.Select<TestEnum>().ToList();

            Assert.Throws<Exception>(() => g.mysql.CodeFirst.SyncStructure<TestEnumable>());

            var TestEnumable = new TestEnumable();


            g.mysql.GetRepository<Model1, int>().Insert(new Model1
            {
                title = "test_" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                M2Id = DateTime.Now.Second + DateTime.Now.Minute,
                Childs = new[] {
                    new Model2 {
                         Title = "model2Test_title_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "0001",
                    },
                    new Model2 {
                         Title = "model2Test_title_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "0002",
                    },
                    new Model2 {
                         Title = "model2Test_title_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "0003",
                    },
                    new Model2 {
                         Title = "model2Test_title_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "0004",
                    }
                }
            });

            var includet1 = g.mysql.Select<Model1>()
                .IncludeMany(a => a.Childs.Take(2), s => s.Where(a => a.id > 0))
                .IncludeMany(a => a.TestManys.Take(1).Where(b => b.id == a.id))
                .Where(a => a.id > 10)
                .ToList();

            var ttt1 = g.mysql.Select<Model1>().Where(a => a.Childs.AsSelect().Any(b => b.Title == "111")).ToList();
            var testpid1 = g.mysql.Insert<TestTypeInfo>().AppendData(new TestTypeInfo { Name = "Name" + DateTime.Now.ToString("yyyyMMddHHmmss") }).ExecuteIdentity();
            g.mysql.Insert<TestInfo>().AppendData(new TestInfo { Title = "Title" + DateTime.Now.ToString("yyyyMMddHHmmss"), CreateTime = DateTime.Now, TypeGuid = (int)testpid1 }).ExecuteAffrows();

            var aggsql1 = select
                .GroupBy(a => a.Title)
                .ToSql(b => new
                {
                    b.Key,
                    cou = b.Count(),
                    sum = b.Sum(b.Key),
                    sum2 = b.Sum(b.Value.TypeGuid)
                });
            var aggtolist1 = select
                .GroupBy(a => a.Title)
                .ToList(b => new
                {
                    b.Key,
                    cou = b.Count(),
                    sum = b.Sum(b.Key),
                    sum2 = b.Sum(b.Value.TypeGuid)
                });

            var aggsql2 = select
                .GroupBy(a => new { a.Title, yyyy = string.Concat(a.CreateTime.Year, '-', a.CreateTime.Month) })
                .ToSql(b => new
                {
                    b.Key.Title,
                    b.Key.yyyy,

                    cou = b.Count(),
                    sum = b.Sum(b.Key.yyyy),
                    sum2 = b.Sum(b.Value.TypeGuid)
                });
            var aggtolist2 = select
                .GroupBy(a => new { a.Title, yyyy = string.Concat(a.CreateTime.Year, '-', a.CreateTime.Month) })
                .ToList(b => new
                {
                    b.Key.Title,
                    b.Key.yyyy,

                    cou = b.Count(),
                    sum = b.Sum(b.Key.yyyy),
                    sum2 = b.Sum(b.Value.TypeGuid)
                });

            var aggsql3 = select
                .GroupBy(a => a.Title)
                .ToSql(b => new
                {
                    b.Key,
                    cou = b.Count(),
                    sum = b.Sum(b.Key),
                    sum2 = b.Sum(b.Value.TypeGuid),
                    sum3 = b.Sum(b.Value.Type.Parent.Parent.Id)
                });

            var sqlrepos = g.mysql.GetRepository<TestTypeParentInfo, int>();
            sqlrepos.Insert(new TestTypeParentInfo
            {
                Name = "testroot",
                Childs = new[] {
                    new TestTypeParentInfo {
                        Name = "testpath2",
                        Childs = new[] {
                            new TestTypeParentInfo {
                                Name = "testpath3",
                                Childs = new[] {
                                    new TestTypeParentInfo {
                                        Name = "11"
                                    }
                                }
                            }
                        }
                    }
                }
            });

            var sql = g.mysql.Select<TestTypeParentInfo>().Where(a => a.Parent.Parent.Parent.Name == "testroot").ToSql();
            var sql222 = g.mysql.Select<TestTypeParentInfo>().Where(a => a.Parent.Parent.Parent.Name == "testroot").ToList();


            Expression<Func<TestInfo, object>> orderBy = null;
            orderBy = a => a.CreateTime;
            var testsql1 = select.OrderBy(orderBy).ToSql();

            orderBy = a => a.Title;

            var testsql2 = select.OrderBy(orderBy).ToSql();


            var testjson = @"[
{
""acptNumBelgCityName"":""泰州"",
""concPrsnName"":""常**"",
""srvReqstTypeName"":""家庭业务→网络质量→家庭宽带→自有宽带→功能使用→游戏过程中频繁掉线→全局流转"",
""srvReqstCntt"":""客户来电表示宽带使用（ 所有）出现（频繁掉线不稳定） ，客户所在地址为（安装地址泰州地区靖江靖城街道工农路科技小区科技3区176号2栋2单元502），联系方式（具体联系方式），烦请协调处理。"",
""acptTime"":""2019-04-15 15:17:05"",
""acptStaffDeptId"":""0003002101010001000600020023""
},
{
""acptNumBelgCityName"":""苏州"",
""concPrsnName"":""龚**"",
""srvReqstTypeName"":""移动业务→基础服务→账/详单→全局流转→功能使用→账/详单信息不准确→全局流转"",
""srvReqstCntt"":""用户参与 2018年苏州任我用关怀活动 送的分钟数500分钟，说自己只使用了116分钟，但是我处查询到本月已经使用了306分钟\r\n，烦请处理"",
""acptTime"":""2019-04-15 15:12:05"",
""acptStaffDeptId"":""0003002101010001000600020023""
}
]";
            //var dic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(testjson);
            var reqs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ServiceRequestNew>>(testjson);
            reqs.ForEach(t =>
            {
                g.oracle.Insert<ServiceRequestNew>(t).ExecuteAffrows();

            });



            var sql111 = g.mysql.Select<TestUser>().AsTable((a, b) => "(select * from TestUser where stringid > 10)").Page(1, 10).ToSql();


            var xxx = g.mysql.Select<TestUser>().GroupBy(a => new { a.stringid }).ToList(a => a.Key.stringid);

            var tuser = g.mysql.Select<TestUser>().Where(u => u.accname == "admin")
                .InnerJoin(a => a.LogOn.id == a.stringid).ToSql();


            var parentSelect1 = select.Where(a => a.Type.Parent.Parent.Parent.Parent.Name == "").Where(b => b.Type.Name == "").ToSql();


            var collSelect1 = g.mysql.Select<Order>().Where(a =>
                a.OrderDetails.AsSelect().Any(b => b.Id > 100)
            );

            var collectionSelect = select.Where(a =>
                //a.Type.Guid == a.TypeGuid &&
                //a.Type.Parent.Id == a.Type.ParentId &&
                a.Type.Parent.Types.AsSelect().Where(b => b.Name == a.Title).Any(
                //b => b.ParentId == a.Type.Parent.Id
                )
            );

            var collectionSelect2 = select.Where(a =>
                a.Type.Parent.Types.AsSelect().Where(b => b.Name == a.Title).Any(
                    b => b.Parent.Name == "xxx" && b.Parent.Parent.Name == "ccc"
                    && b.Parent.Parent.Parent.Types.AsSelect().Any(cb => cb.Name == "yyy")
                )
            );

            var collectionSelect3 = select.Where(a =>
                a.Type.Parent.Types.AsSelect().Where(b => b.Name == a.Title).Any(
                    bbb => bbb.Parent.Types.AsSelect().Where(lv2 => lv2.Name == bbb.Name + "111").Any(
                    )
                )
            );


            var neworder = new Order
            {
                CustomerName = "testCustomer",
                OrderTitle = "xxx#cccksksk",
                TransactionDate = DateTime.Now,
                OrderDetails = new List<OrderDetail>(new[] {
                    new OrderDetail {

                    },
                    new OrderDetail {

                    }
                })
            };

            g.mysql.GetRepository<Order>().Insert(neworder);

            var order = g.mysql.Select<Order>().Where(a => a.Id == neworder.Id).ToOne(); //查询订单表
            if (order == null)
            {
                var orderId = g.mysql.Insert(new Order { }).ExecuteIdentity();
                order = g.mysql.Select<Order>(orderId).ToOne();
            }


            var orderDetail1 = order.OrderDetails; //第一次访问，查询数据库
            var orderDetail2 = order.OrderDetails; //第二次访问，不查
            var order1 = orderDetail1.FirstOrDefault().Order; //访问导航属性，此时不查数据库，因为 OrderDetails 查询出来的时候已填充了该属性


            var queryable = g.mysql.Queryable<TestInfo>().Where(a => a.Id == 1).ToList();

            var sql2222 = select.Where(a =>
                select.Where(b => b.Id == a.Id &&
                    select.Where(c => c.Id == b.Id).Where(d => d.Id == a.Id).Where(e => e.Id == b.Id)
                    //.Offset(a.Id)
                    .Any()
                ).Any()
            ).ToList();


            var groupbysql = g.mysql.Select<TestInfo>().From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
                .Where(a => a.Id == 1)
                .WhereIf(false, a => a.Id == 2)
            )
            .WhereIf(true, (a, b, c) => a.Id == 3)
            .GroupBy((a, b, c) => new { tt2 = a.Title.Substring(0, 2), mod4 = a.Id % 4 })
            .Having(a => a.Count() > 0 && a.Avg(a.Key.mod4) > 0 && a.Max(a.Key.mod4) > 0)
            .Having(a => a.Count() < 300 || a.Avg(a.Key.mod4) < 100)
            .OrderBy(a => a.Key.tt2)
            .OrderByDescending(a => a.Count()).ToSql(a => new
            {
                cou = a.Sum(a.Value.Item1.Id),
                a.Key.mod4,
                a.Key.tt2,
                max = a.Max("a.id"),
                max2 = Convert.ToInt64("max(a.id)")
            });

            var groupbysql2 = g.mysql.Select<TestInfo>().From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
                .Where(a => a.Id == 1)
                .WhereIf(true, a => a.Id == 2)
            )
            .WhereIf(false, (a, b, c) => a.Id == 3)
            .GroupBy((a, b, c) => new { tt2 = a.Title.Substring(0, 2), mod4 = a.Id % 4 })
            .Having(a => a.Count() > 0 && a.Avg(a.Key.mod4) > 0 && a.Max(a.Key.mod4) > 0)
            .Having(a => a.Count() < 300 || a.Avg(a.Key.mod4) < 100)
            .OrderBy(a => a.Key.tt2)
            .OrderByDescending(a => a.Count()).ToSql(a => a.Key.mod4);

            var groupby = g.mysql.Select<TestInfo>().From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
                .Where(a => a.Id == 1)
                .WhereIf(true, a => a.Id == 2)
            )
            .WhereIf(true, (a, b, c) => a.Id == 3)
            .GroupBy((a, b, c) => new { tt2 = a.Title.Substring(0, 2), mod4 = a.Id % 4 })
            .Having(a => a.Count() > 0 && a.Avg(a.Key.mod4) > 0 && a.Max(a.Key.mod4) > 0)
            .Having(a => a.Count() < 300 || a.Avg(a.Key.mod4) < 100)
            .OrderBy(a => a.Key.tt2)
            .OrderByDescending(a => a.Count())
            .ToList(a => new
            {
                a.Key.tt2,
                cou1 = a.Count(),
                empty = "",
                nil = (string)null,
                arg1 = a.Avg(a.Key.mod4),
                ccc2 = a.Key.tt2 ?? "now()",
                //ccc = Convert.ToDateTime("now()"), partby = Convert.ToDecimal("sum(num) over(PARTITION BY server_id,os,rid,chn order by id desc)")
            });

            var arrg = g.mysql.Select<TestInfo>().ToAggregate(a => new { sum = a.Sum(a.Key.Id + 11.11), avg = a.Avg(a.Key.Id), count = a.Count(), max = a.Max(a.Key.Id), min = a.Min(a.Key.Id) });

            var arrg222 = g.mysql.Select<NullAggreTestTable>().ToAggregate(a => new { sum = a.Sum(a.Key.Id + 11.11), avg = a.Avg(a.Key.Id), count = a.Count(), max = a.Max(a.Key.Id), min = a.Min(a.Key.Id) });

            var t1 = g.mysql.Select<TestInfo>().Where("").Where(a => a.Id > 0).Skip(100).Limit(200).ToList();
            var t2 = g.mysql.Select<TestInfo>().As("b").Where("").Where(a => a.Id > 0).Skip(100).Limit(200).ToList();


            var sql1 = select.LeftJoin(a => a.Type.Guid == a.TypeGuid).ToList();
            var sql2 = select.LeftJoin<TestTypeInfo>((a, b) => a.TypeGuid == b.Guid && b.Name == "111").ToList();
            var sql3 = select.LeftJoin("TestTypeInfo b on b.Guid = a.TypeGuid").ToList();

            //g.mysql.Select<TestInfo, TestTypeInfo, TestTypeParentInfo>().Join((a, b, c) => new Model.JoinResult3(
            //   Model.JoinType.LeftJoin, a.TypeGuid == b.Guid,
            //   Model.JoinType.InnerJoin, c.Id == b.ParentId && c.Name == "xxx")
            //);

            //var sql4 = select.From<TestTypeInfo, TestTypeParentInfo>((a, b, c) => new SelectFrom()
            //    .InnerJoin(a.TypeGuid == b.Guid)
            //    .LeftJoin(c.Id == b.ParentId)
            //    .Where(b.Name == "xxx"))
            //.Where(a => a.Id == 1).ToSql();

            var sql4 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
                .InnerJoin(a => a.TypeGuid == b.Guid)
                .LeftJoin(a => c.Id == b.ParentId)
                .Where(a => b.Name == "xxx")).ToList();
            //.Where(a => a.Id == 1).ToSql();


            var list111 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
                .InnerJoin(a => a.TypeGuid == b.Guid)
                .LeftJoin(a => c.Id == b.ParentId)
                .Where(a => b.Name != "xxx"))
                .ToList((a, b, c) => new
                {
                    a.Id,
                    a.Title,
                    a.Type,
                    ccc = new { a.Id, a.Title },
                    tp = a.Type,
                    tp2 = new
                    {
                        a.Id,
                        tp2 = a.Type.Name
                    },
                    tp3 = new
                    {
                        a.Id,
                        tp33 = new
                        {
                            a.Id
                        }
                    }
                });

            var ttt122 = g.mysql.Select<TestTypeParentInfo>().Where(a => a.Id > 0).ToList();




            var sql5 = g.mysql.Select<TestInfo>().From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s).Where((a, b, c) => a.Id == b.ParentId).ToList();





            //((JoinType.LeftJoin, a.TypeGuid == b.Guid), (JoinType.InnerJoin, b.ParentId == c.Id)

            var t11112 = g.mysql.Select<TestInfo>().ToList(a => new
            {
                a.Id,
                a.Title,
                a.Type,
                ccc = new { a.Id, a.Title },
                tp = a.Type,
                tp2 = new
                {
                    a.Id,
                    tp2 = a.Type.Name
                },
                tp3 = new
                {
                    a.Id,
                    tp33 = new
                    {
                        a.Id
                    }
                }

            });

            var t100 = g.mysql.Select<TestInfo>().Where("").Where(a => a.Id > 0).Skip(100).Limit(200).ToList();
            var t101 = g.mysql.Select<TestInfo>().As("b").Where("").Where(a => a.Id > 0).Skip(100).Limit(200).ToList();


            var t1111 = g.mysql.Select<TestInfo>().ToList(a => new { a.Id, a.Title, a.Type });

            var t2222 = g.mysql.Select<TestInfo>().ToList(a => new { a.Id, a.Title, a.Type.Name });

            var t3 = g.mysql.Insert<TestInfo>(new[] { new TestInfo { }, new TestInfo { } }).IgnoreColumns(a => a.Title).ToSql();
            var t4 = g.mysql.Insert<TestInfo>(new[] { new TestInfo { }, new TestInfo { } }).IgnoreColumns(a => new { a.Title, a.CreateTime }).ToSql();
            var t5 = g.mysql.Insert<TestInfo>(new[] { new TestInfo { }, new TestInfo { } }).IgnoreColumns(a => new { a.Title, a.TypeGuid, a.CreateTime }).ToSql();
            var t6 = g.mysql.Insert<TestInfo>(new[] { new TestInfo { }, new TestInfo { } }).InsertColumns(a => new { a.Title }).ToSql();

            var t7 = g.mysql.Update<TestInfo>().ToSql();
            var t8 = g.mysql.Update<TestInfo>().Where(new TestInfo { }).ToSql();
            var t9 = g.mysql.Update<TestInfo>().Where(new[] { new TestInfo { Id = 1 }, new TestInfo { Id = 2 } }).ToSql();
            var t10 = g.mysql.Update<TestInfo>().Where(new[] { new TestInfo { Id = 1 }, new TestInfo { Id = 2 } }).Where(a => a.Title == "111").ToSql();
            var t11 = g.mysql.Update<TestInfo>().SetSource(new[] { new TestInfo { Id = 1, Title = "111" }, new TestInfo { Id = 2, Title = "222" } }).ToSql();
            var t12 = g.mysql.Update<TestInfo>().SetSource(new[] { new TestInfo { Id = 1, Title = "111" }, new TestInfo { Id = 2, Title = "222" } }).Where(a => a.Title == "111").ToSql();

            var t13 = g.mysql.Update<TestInfo>().Set(a => a.Title, "222111").ToSql();
            var t14 = g.mysql.Update<TestInfo>().Set(a => a.Title, "222111").Where(new TestInfo { }).ToSql();
            var t15 = g.mysql.Update<TestInfo>().Set(a => a.Title, "222111").Where(new[] { new TestInfo { Id = 1 }, new TestInfo { Id = 2 } }).ToSql();
            var t16 = g.mysql.Update<TestInfo>().Set(a => a.Title, "222111").Where(new[] { new TestInfo { Id = 1 }, new TestInfo { Id = 2 } }).Where(a => a.Title == "111").ToSql();
            var t17 = g.mysql.Update<TestInfo>().SetSource(new[] { new TestInfo { Id = 1, Title = "111" }, new TestInfo { Id = 2, Title = "222" } }).Set(a => a.Title, "222111").ToSql();
            var t18 = g.mysql.Update<TestInfo>().SetSource(new[] { new TestInfo { Id = 1, Title = "111" }, new TestInfo { Id = 2, Title = "222" } }).Set(a => a.Title, "222111").Where(a => a.Title == "111").ToSql();

            var t19 = g.mysql.Update<TestInfo>().Set(a => a.TypeGuid + 222111).ToSql();
            var t20 = g.mysql.Update<TestInfo>().Set(a => a.TypeGuid + 222111).Where(new TestInfo { }).ToSql();
            var t21 = g.mysql.Update<TestInfo>().Set(a => a.TypeGuid + 222111).Where(new[] { new TestInfo { Id = 1 }, new TestInfo { Id = 2 } }).ToSql();
            var t22 = g.mysql.Update<TestInfo>().Set(a => a.TypeGuid + 222111).Where(new[] { new TestInfo { Id = 1 }, new TestInfo { Id = 2 } }).Where(a => a.Title == "111").ToSql();
            var t23 = g.mysql.Update<TestInfo>().SetSource(new[] { new TestInfo { Id = 1, Title = "111" }, new TestInfo { Id = 2, Title = "222" } }).Set(a => a.TypeGuid + 222111).ToSql();
            var t24 = g.mysql.Update<TestInfo>().SetSource(new[] { new TestInfo { Id = 1, Title = "111" }, new TestInfo { Id = 2, Title = "222" } }).Set(a => a.TypeGuid + 222111).Where(a => a.Title == "111").ToSql();


            var t1000 = g.mysql.Select<ExamPaper>().ToSql();
            var t1001 = g.mysql.Insert<ExamPaper>().AppendData(new ExamPaper()).ToSql();
        }
    }
    class NullAggreTestTable
    {
        [Column(IsIdentity = true)]
        public int Id { get; set; }
    }


    [Table(Name = "TestInfoT1")]
    class TestInfo
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public int TypeGuid { get; set; }
        public TestTypeInfo Type { get; set; }
        public string Title { get; set; }
        public DateTime CreateTime { get; set; }
    }

    [Table(Name = "TestTypeInfoT1")]
    class TestTypeInfo
    {
        [Column(IsIdentity = true)]
        public int Guid { get; set; }
        public int ParentId { get; set; }
        public TestTypeParentInfo Parent { get; set; }
        public string Name { get; set; }
    }

    [Table(Name = "TestTypeParentInfoT1")]
    class TestTypeParentInfo
    {
        [Column(IsIdentity = true)]
        public int Id { get; set; }
        public string Name { get; set; }

        public int ParentId { get; set; }
        public TestTypeParentInfo Parent { get; set; }
        public ICollection<TestTypeParentInfo> Childs { get; set; }

        public List<TestTypeInfo> Types { get; set; }
    }


    /// <summary>
    /// 试卷表
    /// </summary>
    [Table(Name = "exam_paper")]
    public class ExamPaper
    {

        public long id { get; set; }

        /// <summary>
        /// 考核计划ID
        /// </summary>
        public long AssessmentPlanId { get; set; }
        /// <summary>
        /// 总分
        /// </summary>
        public int TotalScore { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime? EndTime { get; set; }

        //[Column(IsIgnore = true)]
        //public ExamStatus Status { get; set; }
        public ExamStatus Status
        {
            get
            {
                if (DateTime.Now <= BeginTime)
                    return ExamStatus.Wait;
                if (BeginTime <= DateTime.Now && (!EndTime.HasValue || DateTime.Now < EndTime))
                    return ExamStatus.Started;
                if (BeginTime <= DateTime.Now && (EndTime.HasValue && EndTime <= DateTime.Now))
                    return ExamStatus.End;
                return ExamStatus.Wait;
            }
        }
    }
    public enum ExamStatus { Wait, Started, End }
}
