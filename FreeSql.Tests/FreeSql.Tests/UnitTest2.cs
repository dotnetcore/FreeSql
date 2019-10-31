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

namespace FreeSql.Tests
{
    public class UnitTest2
    {

        public partial class SysModulePermission
        {
            /// <summary>
            /// 菜单权限ID
            /// </summary>
            [Column(IsPrimary = true)] public String SysModulePermissionId { get; set; }

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
            [Column(IsPrimary = true)]
            public String SysModuleId { get; set; }

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
            [Column(IsPrimary = true)]
            public String SysModuleButtonId { get; set; }

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
            public SysModuleButton Button { get; set; }
        }
        partial class SysModule
        {
            public List<SysModulePermission> Permissions { get; set; }
        }
        partial class SysModuleButton
        {
        }

        [Fact]
        public void Test02()
        {

            g.sqlite.Delete<SysModulePermission>().Where("1=1").ExecuteAffrows();
            g.sqlite.Delete<SysModuleButton>().Where("1=1").ExecuteAffrows();
            g.sqlite.Delete<SysModule>().Where("1=1").ExecuteAffrows();

            var menu1 = new SysModule { SysModuleId = "menu1", Name = "菜单1" };
            var menu2 = new SysModule { SysModuleId = "menu2", Name = "菜单2" };
            g.sqlite.Insert(new[] { menu1, menu2 }).ExecuteAffrows();

            var button1 = new SysModuleButton { SysModuleButtonId = "button1", Name = "添加" };
            var button2 = new SysModuleButton { SysModuleButtonId = "button2", Name = "修改" };
            var button3 = new SysModuleButton { SysModuleButtonId = "button3", Name = "删除" };
            var button4 = new SysModuleButton { SysModuleButtonId = "button4", Name = "查询" };
            g.sqlite.Insert(new[] { button1, button2, button3, button4 }).ExecuteAffrows();

            g.sqlite.Insert(new[] {
                new SysModulePermission { SysModulePermissionId = "menu1_button1", SysModuleId = menu1.SysModuleId, SysModuleButtonId = button1.SysModuleButtonId },
                new SysModulePermission { SysModulePermissionId = "menu1_button2", SysModuleId = menu1.SysModuleId, SysModuleButtonId = button2.SysModuleButtonId },
                new SysModulePermission { SysModulePermissionId = "menu1_button3", SysModuleId = menu1.SysModuleId, SysModuleButtonId = button3.SysModuleButtonId },
                new SysModulePermission { SysModulePermissionId = "menu1_button4", SysModuleId = menu1.SysModuleId, SysModuleButtonId = button4.SysModuleButtonId },

                new SysModulePermission { SysModulePermissionId = "menu2_button1", SysModuleId = menu2.SysModuleId, SysModuleButtonId = button1.SysModuleButtonId },
                new SysModulePermission { SysModulePermissionId = "menu2_button2", SysModuleId = menu2.SysModuleId, SysModuleButtonId = button2.SysModuleButtonId },
                new SysModulePermission { SysModulePermissionId = "menu2_button3", SysModuleId = menu2.SysModuleId, SysModuleButtonId = button3.SysModuleButtonId },
                new SysModulePermission { SysModulePermissionId = "menu2_button4", SysModuleId = menu2.SysModuleId, SysModuleButtonId = button4.SysModuleButtonId },
            }).ExecuteAffrows();

            //var list = g.sqlite.Select<SysModule>()
            //   .IncludeMany(m => m.Buttons)
            //   .Page(1, 10)
            //   .ToList();

            var list = g.sqlite.Select<SysModule>()
                .IncludeMany(m => m.Permissions.Where(p => p.SysModuleId == m.SysModuleId),
                    then => then.LeftJoin(p => p.Button.SysModuleButtonId == p.SysModuleButtonId))
                .ToList();
        }
    }
}
