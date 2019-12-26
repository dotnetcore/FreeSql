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
using System.Text;

namespace FreeSql.Tests
{
    public class UnitTest2
    {

        public partial class SysModulePermission
        {
            /// <summary>
            /// 菜单权限ID
            /// </summary>
            [Column(IsPrimary = true, OldName = "SysModulePermissionId")]
            public String Id { get; set; }

            /// <summary>
            /// 菜单主键ID
            /// </summary>
            public String SysModuleId { get; set; }

            /// <summary>
            /// 按钮主键ID
            /// </summary>
            public String SysModuleButtonId { get; set; }

            /// <summary>
            /// 菜单权限
            /// </summary>
            public Int32 Status { get; set; }
        }
        public partial class SysModule
        {
            /// <summary>
            /// 主键
            /// </summary>
            [Column(IsPrimary = true, OldName = "SysModuleId")]
            public String Id { get; set; }

            /// <summary>
            /// 父级ID
            /// </summary>
            public String ParentId { get; set; }

            /// <summary>
            /// 名称
            /// </summary>
            public String Name { get; set; }

            /// <summary>
            /// 图标
            /// </summary>
            public String Icon { get; set; }

            /// <summary>
            /// 链接地址
            /// </summary>
            public String UrlAddress { get; set; }

            /// <summary>
            /// 是否公开
            /// </summary>
            public Int32 IsShow { get; set; }

            /// <summary>
            /// 排序
            /// </summary>
            public Int32? Sort { get; set; }

            /// <summary>
            /// 备注
            /// </summary>
            public String Description { get; set; }

            /// <summary>
            /// 创建日期
            /// </summary>
            public DateTime CreateTime { get; set; }

        }
        public partial class SysModuleButton
        {
            /// <summary>
            /// 按钮主键
            /// </summary>
            [Column(IsPrimary = true, OldName = "SysModuleButtonId")]
            public String Id { get; set; }

            /// <summary>
            /// 名称
            /// </summary>
            public String Name { get; set; }

            /// <summary>
            /// 事件名称
            /// </summary>
            public String EventName { get; set; }

            /// <summary>
            /// 编码
            /// </summary>
            public String EnCode { get; set; }

            /// <summary>
            /// 图标
            /// </summary>
            public String Icon { get; set; }

            /// <summary>
            /// 排序
            /// </summary>
            public Int32? Sort { get; set; }

            /// <summary>
            /// 创建日期
            /// </summary>
            public DateTime CreateTime { get; set; }
        }
        partial class SysModulePermission
        {
            [Navigate("SysModuleButtonId")]
            public SysModuleButton Button { get; set; }
        }
        partial class SysModule
        {
            [Navigate("SysModuleId")]
            public List<SysModulePermission> Permissions { get; set; }
        }
        partial class SysModuleButton
        {
        }

        public class LinUser
        {
            public long id { get; set; }
        }

        public class Comment
        {
            public Guid Id { get; set; }
            /// <summary>
            /// 回复的文本内容
            /// </summary>
            public string Text { get; set; }
            [Navigate("CreateUserId")]
            public LinUser UserInfo { get; set; }
            public long? CreateUserId { get; set; }
        }

        public class UserLike
        {
            public Guid Id { get; set; }
            public Guid SubjectId { get; set; }
            public long? CreateUserId { get; set; }
        }

        public class TestMySqlStringIsNullable
        {
            public Guid id { get; set; }
            public string nvarchar { get; set; }
            [Column(IsNullable = true)]
            public string nvarchar_null { get; set; }
            [Column(IsNullable = false)]
            public string nvarchar_notnull { get; set; }

            [Column(DbType = "varchar(100)")]
            public string varchar { get; set; }
            [Column(IsNullable = true, DbType = "varchar(100)")]
            public string varchar_null { get; set; }
            [Column(IsNullable = false, DbType = "varchar(100)")]
            public string varchar_notnull { get; set; }
        }

        public class TestIgnoreDefaultValue { 
            public Guid Id { get; set; }

            [Column(IsIgnore = true)]
            public double? quantity { get; set; } = 100f;

            public DateTime ct1 { get; set; }
            public DateTime? ct2 { get; set; }
        }

        public class TBatInst
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class gf_t1
        {
            public Guid id { get; set; }
            public int rowstate { get; set; }
        }
        public class gf_t2
        {
            public Guid id { get; set; }
            public decimal rowstate { get; set; }
        }
        public class gf_t3
        {
            public Guid id { get; set; }
            public decimal rowstate { get; set; }
        }

        public class gfDto
        {
            public int rowstate { get; set; }
            public dfDto2 dto2 { get; set; }
        }
        public class dfDto2
        {
            public int id { get; set; }
            public decimal rowstate { get; set; }
        }

        public class otot1
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string name { get; set; }

            public otot2 t2 { get; set; }
        }
        public class otot2
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string title { get; set; }
        }

        [Fact]
        public void Test02()
        {
            g.sqlite.Update<TestIgnoreDefaultValue>(Guid.Empty).Set(a => a.ct1 == a.ct2).ExecuteAffrows();

            g.sqlite.Insert(new otot1 { name = "otot1_name1" }).ExecuteAffrows();

            var otolst1 = g.sqlite.Select<otot1>()
                .LeftJoin(a => a.id == a.t2.id)
                .ToList();

            var otolst2 = g.sqlite.Select<otot1, otot2>()
                .LeftJoin((a, b) => a.id == b.id)
                .ToList((a, b) => new
                {
                    a,
                    b
                });




            var testcf = g.sqlite.CodeFirst.GetComparisonDDLStatements(typeof(dfDto2), "main.test2");


            var u1 = new userinfo {
                name = "111",
                departments = new List<departments>(new[]{
                    new departments { deptname = "dep1" },
                    new departments { deptname = "dep1" }
                })
            };
            var kwrepo = g.sqlite.GetRepository<userinfo>();
            kwrepo.Insert(u1);


            g.sqlite.GlobalFilter.Apply<gf_t1>("gft1", a => a.rowstate > -1)
                .Apply<gf_t2>("gft2", a => a.rowstate > -2)
                .Apply<gf_t3>("gft3", a => a.rowstate > -3);

            var tksk1 = g.sqlite.Select<gf_t1, gf_t2, gf_t3>()
                .InnerJoin((a, b, c) => a.id == b.id)
                .Where((a, b, c) => c.rowstate > 10)
                .ToList();

            var tksk2 = g.sqlite.Select<gf_t1, gf_t2, gf_t3>()
                .InnerJoin((a, b, c) => a.id == b.id)
                .Where((a, b, c) => c.rowstate > 10)
                .ToList();

            var dtot2 = g.sqlite.Select<gf_t1>().ToList(a => new gfDto
            {
                dto2 = new dfDto2
                {
                    rowstate = a.rowstate
                }
            });

            List<(Guid, DateTime)> contains2linqarr = new List<(Guid, DateTime)>();
            Assert.Equal("SELECT 1 as1 FROM \"TestIgnoreDefaultValue\" a WHERE (1=0)", g.sqlite.Select<TestIgnoreDefaultValue>().Where(a => contains2linqarr.Contains(a.Id, a.ct1)).ToSql(a => 1).Replace("\r\n", ""));
            g.sqlite.Select<TestIgnoreDefaultValue>().Where(a => contains2linqarr.Contains(a.Id, a.ct1)).ToList();

            contains2linqarr.Add((Guid.NewGuid(), DateTime.Now));
            contains2linqarr.Add((Guid.NewGuid(), DateTime.Now));
            contains2linqarr.Add((Guid.NewGuid(), DateTime.Now));
            g.sqlite.Select<TestIgnoreDefaultValue>()
                .Where(a => contains2linqarr.Contains(a.Id, a.ct1)).ToList();












            List<(Guid, DateTime, DateTime?)> contains3linqarr = new List<(Guid, DateTime, DateTime?)>();
            Assert.Equal("SELECT 1 as1 FROM \"TestIgnoreDefaultValue\" a WHERE (1=0)", g.sqlite.Select<TestIgnoreDefaultValue>().Where(a => contains3linqarr.Contains(a.Id, a.ct1, a.ct2)).ToSql(a => 1).Replace("\r\n", ""));
            g.sqlite.Select<TestIgnoreDefaultValue>().Where(a => contains3linqarr.Contains(a.Id, a.ct1, a.ct2)).ToList();

            contains3linqarr.Add((Guid.NewGuid(), DateTime.Now, DateTime.Now));
            contains3linqarr.Add((Guid.NewGuid(), DateTime.Now, DateTime.Now));
            contains3linqarr.Add((Guid.NewGuid(), DateTime.Now, DateTime.Now));
            g.sqlite.Select<TestIgnoreDefaultValue>().Where(a => contains3linqarr.Contains(a.Id, a.ct1, a.ct2)).ToList();

            var start = DateTime.Now.Date;
            var end = DateTime.Now.AddDays(1).Date.AddMilliseconds(-1);
            var textbetween = g.sqlite.Select<TestIgnoreDefaultValue>()
                .Where(a => a.ct1.Between(start, end))
                .ToList();

            var textbetweenend = g.sqlite.Select<TestIgnoreDefaultValue>()
                .Where(a => a.ct1.BetweenEnd(start, end))
                .ToList();

            g.mysql.GlobalFilter.Apply<gf_t1>("gft1", a => a.rowstate > -1)
                .Apply<gf_t2>("gft2", a => a.rowstate > -2)
                .Apply<gf_t3>("gft3", a => a.rowstate > -3);

            var gft1 = g.mysql.Select<gf_t1>().Where(a => a.id == Guid.NewGuid()).ToList();
            var gft2 = g.mysql.Select<gf_t2>().Where(a => a.id == Guid.NewGuid()).ToList();
            var gft3 = g.mysql.Select<gf_t3>().Where(a => a.id == Guid.NewGuid()).ToList();

            g.sqlserver.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.mysql.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.pgsql.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.oracle.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.sqlite.Delete<TBatInst>().Where("1=1").ExecuteAffrows();

            g.sqlserver.Insert(Enumerable.Range(0, 1048).Select(a => new TBatInst { Name = "test" + a }).ToList()).ExecuteAffrows();
            g.mysql.Insert(Enumerable.Range(0, 1048).Select(a => new TBatInst { Name = "test" + a }).ToList()).ExecuteAffrows();
            g.pgsql.Insert(Enumerable.Range(0, 1048).Select(a => new TBatInst { Name = "test" + a }).ToList()).ExecuteAffrows();
            g.oracle.Insert(Enumerable.Range(0, 1048).Select(a => new TBatInst { Name = "test" + a }).ToList()).ExecuteAffrows();
            g.sqlite.Insert(Enumerable.Range(0, 1048).Select(a => new TBatInst { Name = "test" + a }).ToList()).ExecuteAffrows();

            Assert.Equal(1048, g.sqlserver.Select<TBatInst>().Count());
            Assert.Equal(1048, g.mysql.Select<TBatInst>().Count());
            Assert.Equal(1048, g.pgsql.Select<TBatInst>().Count());
            Assert.Equal(1048, g.oracle.Select<TBatInst>().Count());
            Assert.Equal(1048, g.sqlite.Select<TBatInst>().Count());

            //----

            g.sqlserver.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.mysql.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.pgsql.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.oracle.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.sqlite.Delete<TBatInst>().Where("1=1").ExecuteAffrows();

            g.sqlserver.Insert(Enumerable.Range(0, 1048).Select(a => new TBatInst { Name = "test" + a }).ToList()).NoneParameter().ExecuteAffrows();
            g.mysql.Insert(Enumerable.Range(0, 1048).Select(a => new TBatInst { Name = "test" + a }).ToList()).NoneParameter().ExecuteAffrows();
            g.pgsql.Insert(Enumerable.Range(0, 1048).Select(a => new TBatInst { Name = "test" + a }).ToList()).NoneParameter().ExecuteAffrows();
            g.oracle.Insert(Enumerable.Range(0, 1048).Select(a => new TBatInst { Name = "test" + a }).ToList()).NoneParameter().ExecuteAffrows();
            g.sqlite.Insert(Enumerable.Range(0, 1048).Select(a => new TBatInst { Name = "test" + a }).ToList()).NoneParameter().ExecuteAffrows();

            Assert.Equal(1048, g.sqlserver.Select<TBatInst>().Count());
            Assert.Equal(1048, g.mysql.Select<TBatInst>().Count());
            Assert.Equal(1048, g.pgsql.Select<TBatInst>().Count());
            Assert.Equal(1048, g.oracle.Select<TBatInst>().Count());
            Assert.Equal(1048, g.sqlite.Select<TBatInst>().Count());

            //----

            g.sqlserver.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.mysql.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.pgsql.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.oracle.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.sqlite.Delete<TBatInst>().Where("1=1").ExecuteAffrows();

            g.sqlserver.Insert(Enumerable.Range(0, 3348).Select(a => new TBatInst { Name = "test" + a }).ToList()).ExecuteAffrows();
            g.mysql.Insert(Enumerable.Range(0, 3348).Select(a => new TBatInst { Name = "test" + a }).ToList()).ExecuteAffrows();
            g.pgsql.Insert(Enumerable.Range(0, 3348).Select(a => new TBatInst { Name = "test" + a }).ToList()).ExecuteAffrows();
            g.oracle.Insert(Enumerable.Range(0, 3348).Select(a => new TBatInst { Name = "test" + a }).ToList()).ExecuteAffrows();
            g.sqlite.Insert(Enumerable.Range(0, 3348).Select(a => new TBatInst { Name = "test" + a }).ToList()).ExecuteAffrows();

            Assert.Equal(3348, g.sqlserver.Select<TBatInst>().Count());
            Assert.Equal(3348, g.mysql.Select<TBatInst>().Count());
            Assert.Equal(3348, g.pgsql.Select<TBatInst>().Count());
            Assert.Equal(3348, g.oracle.Select<TBatInst>().Count());
            Assert.Equal(3348, g.sqlite.Select<TBatInst>().Count());

            //----

            g.sqlserver.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.mysql.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.pgsql.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.oracle.Delete<TBatInst>().Where("1=1").ExecuteAffrows();
            g.sqlite.Delete<TBatInst>().Where("1=1").ExecuteAffrows();

            g.sqlserver.Insert(Enumerable.Range(0, 3348).Select(a => new TBatInst { Name = "test" + a }).ToList()).NoneParameter().ExecuteAffrows();
            g.mysql.Insert(Enumerable.Range(0, 3348).Select(a => new TBatInst { Name = "test" + a }).ToList()).NoneParameter().ExecuteAffrows();
            g.pgsql.Insert(Enumerable.Range(0, 3348).Select(a => new TBatInst { Name = "test" + a }).ToList()).NoneParameter().ExecuteAffrows();
            g.oracle.Insert(Enumerable.Range(0, 3348).Select(a => new TBatInst { Name = "test" + a }).ToList()).NoneParameter().ExecuteAffrows();
            g.sqlite.Insert(Enumerable.Range(0, 3348).Select(a => new TBatInst { Name = "test" + a }).ToList()).NoneParameter().ExecuteAffrows();

            Assert.Equal(3348, g.sqlserver.Select<TBatInst>().Count());
            Assert.Equal(3348, g.mysql.Select<TBatInst>().Count());
            Assert.Equal(3348, g.pgsql.Select<TBatInst>().Count());
            Assert.Equal(3348, g.oracle.Select<TBatInst>().Count());
            Assert.Equal(3348, g.sqlite.Select<TBatInst>().Count());


            //var serverTime = g.pgsql.Select<TestIgnoreDefaultValue>().Limit(1).First(a => DateTime.UtcNow);
            //var timeOffset = DateTime.UtcNow.Subtract(serverTime); //减去数据库时间

            //g.pgsql.Aop.AuditValue += new EventHandler<Aop.AuditValueEventArgs>((_, e) =>
            //{
            //    if (e.Column.Attribute.MapType.NullableTypeOrThis() == typeof(DateTime))
            //    {
            //        if (e.Value == null || (DateTime)e.Value == default(DateTime))
            //        {
            //            e.Value = DateTime.Now.Subtract(timeOffset);
            //            return;
            //        }
            //    }
            //});


            g.pgsql.Delete<TestIgnoreDefaultValue>().Where("1=1").ExecuteAffrows();
            g.pgsql.GetRepository<TestIgnoreDefaultValue>().Insert(new TestIgnoreDefaultValue[]
            {
                new TestIgnoreDefaultValue(),
                new TestIgnoreDefaultValue(),
                new TestIgnoreDefaultValue()
            });
            var testttt = g.pgsql.Select<TestIgnoreDefaultValue>().Limit(10).ToList();

            var slsksd = g.mysql.Update<UserLike>().SetSource(new UserLike { Id = Guid.NewGuid(), CreateUserId = 1000, SubjectId = Guid.NewGuid() })
                .UpdateColumns(a => new
                {
                    a.SubjectId
                }).NoneParameter().ToSql();

            g.mysql.Aop.ParseExpression = (s, e) =>
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


            var dbs = g.sqlserver.DbFirst.GetDatabases();
            var tbs = g.sqlserver.DbFirst.GetTablesByDatabase("ds_shop");

            var dicParamslist = g.sqlite.Select<SysModule>().Page(1, 10)
                .Where("id > @id and id > @id2 and id > @id3",
                    new Dictionary<string, int> { ["id"] = 1, ["id2"] = 2, ["id3"] = 3 })
                .ToList();

            var list111 = g.sqlite.Select<SysModule>()
               .Page(1, 10)
               .ToList(a => new { Id = a.Id })
               .Select(a => new SysModule { Id = a.Id }).ToList()
               .IncludeMany(g.sqlite, a => a.Permissions, then => then.Include(a => a.Button));


            var list222 = g.sqlite.Select<SysModule>()
                .IncludeMany(m => m.Permissions, then => then.Include(a => a.Button))
                .Page(1, 10)
                .ToList();

            var comments1 = g.mysql.Select<Comment, UserLike>()
                .LeftJoin((a, b) => a.Id == b.SubjectId)
                .ToList((a, b) => new { comment = a, b.SubjectId, user = a.UserInfo });





            var comments2 = g.mysql.Select<Comment>()
    .Include(r => r.UserInfo)
    .From<UserLike>((z, b) => z.LeftJoin(u => u.Id == b.SubjectId))
    .ToList((a, b) => new { comment = a, b.SubjectId, user = a.UserInfo });

            g.sqlite.Delete<SysModulePermission>().Where("1=1").ExecuteAffrows();
            g.sqlite.Delete<SysModuleButton>().Where("1=1").ExecuteAffrows();
            g.sqlite.Delete<SysModule>().Where("1=1").ExecuteAffrows();

            var menu1 = new SysModule { Id = "menu1", Name = "菜单1" };
            var menu2 = new SysModule { Id = "menu2", Name = "菜单2" };
            g.sqlite.Insert(new[] { menu1, menu2 }).ExecuteAffrows();

            var button1 = new SysModuleButton { Id = "button1", Name = "添加" };
            var button2 = new SysModuleButton { Id = "button2", Name = "修改" };
            var button3 = new SysModuleButton { Id = "button3", Name = "删除" };
            var button4 = new SysModuleButton { Id = "button4", Name = "查询" };
            g.sqlite.Insert(new[] { button1, button2, button3, button4 }).ExecuteAffrows();

            g.sqlite.Insert(new[] {
                new SysModulePermission { Id = "menu1_button1", SysModuleId = menu1.Id, SysModuleButtonId = button1.Id },
                new SysModulePermission { Id = "menu1_button2", SysModuleId = menu1.Id, SysModuleButtonId = button2.Id },
                new SysModulePermission { Id = "menu1_button3", SysModuleId = menu1.Id, SysModuleButtonId = button3.Id },
                new SysModulePermission { Id = "menu1_button4", SysModuleId = menu1.Id, SysModuleButtonId = button4.Id },

                new SysModulePermission { Id = "menu2_button1", SysModuleId = menu2.Id, SysModuleButtonId = button1.Id },
                new SysModulePermission { Id = "menu2_button2", SysModuleId = menu2.Id, SysModuleButtonId = button2.Id },
                new SysModulePermission { Id = "menu2_button3", SysModuleId = menu2.Id, SysModuleButtonId = button3.Id },
                new SysModulePermission { Id = "menu2_button4", SysModuleId = menu2.Id, SysModuleButtonId = button4.Id },
            }).ExecuteAffrows();


            var list123123 = g.sqlite.Select<SysModule>()
                .IncludeMany(m => m.Permissions.Where(p => p.SysModuleId == m.Id),
                    then => then.LeftJoin(p => p.Button.Id == p.SysModuleButtonId))
                .ToList();


            var sql = g.sqlite.Select<SysModule>()
                .ToSql(a => a.CreateTime.FormatDateTime("yyyy-MM-dd"));


            var parm1 = "11";
            var parm2 = "22";
            var parm3 = "33";
            var testparmSelect = g.sqlserver.Select<TestMySqlStringIsNullable>()
                .Where(a =>
                    a.nvarchar == "11" &&
                    a.nvarchar_notnull == "22" &&
                    a.nvarchar_null == "33" &&
                    a.varchar == "11" &&
                    a.varchar_notnull == "22" &&
                    a.varchar_null == "33" &&

                    a.nvarchar == parm1 &&
                    a.nvarchar_notnull == parm2 &&
                    a.nvarchar_null == parm3 &&
                    a.varchar == parm3 &&
                    a.varchar_notnull == parm2 &&
                    a.varchar_null == parm3 &&

                    a.nvarchar == parm1.SetDbParameter(10) &&
                    a.nvarchar_notnull == parm2.SetDbParameter(11) &&
                    a.nvarchar_null == parm3.SetDbParameter(12) &&
                    a.varchar == parm3.SetDbParameter(13) &&
                    a.varchar_notnull == parm2.SetDbParameter(14) &&
                    a.varchar_null == parm3.SetDbParameter(15) &&


                    "11" == a.nvarchar &&
                    "22" == a.nvarchar_notnull &&
                    "33" == a.nvarchar_null &&
                    "11" == a.varchar &&
                    "22" == a.varchar_notnull &&
                    "33" == a.varchar_null &&

                    parm1 == a.nvarchar &&
                    parm2 == a.nvarchar_notnull &&
                    parm3 == a.nvarchar_null &&
                    parm1 == a.varchar &&
                    parm2 == a.varchar_notnull &&
                    parm3 == a.varchar_null &&

                    parm1.SetDbParameter(10) == a.nvarchar &&
                    parm2.SetDbParameter(11) == a.nvarchar_notnull &&
                    parm3.SetDbParameter(12) == a.nvarchar_null &&
                    parm1.SetDbParameter(13) == a.varchar &&
                    parm2.SetDbParameter(14) == a.varchar_notnull &&
                    parm3.SetDbParameter(15) == a.varchar_null

                    );

            //g.sqlserver.CodeFirst.IsGenerateCommandParameterWithLambda = true;
            var name = "testname";
            var sdfsdgselect1 = g.sqlserver.Select<TestMySqlStringIsNullable>().Where(a => a.varchar == name);
            var sdfsdgselect2 = g.sqlserver.Select<TestMySqlStringIsNullable>().Where(a => a.varchar == name.SetDbParameter(10));

            g.sqlserver.Select<TestMySqlStringIsNullable>().Where(a => a.varchar == name).ToList();
            g.sqlserver.Select<TestMySqlStringIsNullable>().Where(a => a.varchar == name.SetDbParameter(10)).ToList();

            var testarr = new string[] { "1", "2" };
            var sdfsdgselect3 = g.sqlserver.Select<TestMySqlStringIsNullable>().Where(a => testarr.Contains(a.varchar));
            g.sqlserver.Select<TestMySqlStringIsNullable>().Where(a => testarr.Contains(a.varchar)).ToList();

        }
    }

    [ExpressionCall]
    public static class DbFunc
    {
        static ThreadLocal<ExpressionCallContext> context = new ThreadLocal<ExpressionCallContext>();

        public static string FormatDateTime(this DateTime that, string arg1)
        {
            return $"date_format({context.Value.ParsedContent["that"]}, {context.Value.ParsedContent["arg1"]})";
        }

        /// <summary>
        /// 设置表达式中的 string 参数化长度，优化执行计划
        /// </summary>
        /// <param name="that"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string SetDbParameter(this string that, int size)
        {
            if (context.Value.DbParameter != null)
            {
                //已经参数化了，开启了全局表达式参数化功能：UseGenerateCommandParameterWithLambda(true)
                context.Value.DbParameter.Size = size;
                return context.Value.ParsedContent["that"];
            }
            var guid = Guid.NewGuid().ToString("N").ToLower();
            context.Value.UserParameters.Add(new SqlParameter
            {
                ParameterName = guid,
                SqlDbType = System.Data.SqlDbType.VarChar,
                Size = size,
                Value = that
            });
            return $"@{guid}";
        }
    }
}
