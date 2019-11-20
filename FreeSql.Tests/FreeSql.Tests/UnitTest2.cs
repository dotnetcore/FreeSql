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

        [Fact]
        public void Test02()
        {
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
        }
    }

    [ExpressionCall]
    public static class DbFunc
    {
        static ThreadLocal<ExpressionCallContext> context = new ThreadLocal<ExpressionCallContext>();

        public static string FormatDateTime(this DateTime that, string arg1)
        {
            return $"date_format({context.Value.Values["arg1"]})";
        }
    }
}
