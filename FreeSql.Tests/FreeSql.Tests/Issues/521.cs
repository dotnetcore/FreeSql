using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _521
    {
        [Fact]
        public void SelectTest()
        {
            IFreeSql fsql = g.sqlserver;

            //fsql.Aop.AuditValue += (s, e) => {
            //    if (e.Column.CsType == typeof(long)
            //        && e.Property.GetCustomAttribute<SnowflakeAttribute>(false) != null
            //        && e.Value?.ToString() == "0")
            //    {
            //        e.Value = 1;
            //    }

            //};


            fsql.Delete<ts521>().Where("1=1").ExecuteAffrows();
            fsql.Insert(new ts521 { ID = 1000000000000000001 }).ExecuteAffrows();

            var item = new List<ts521>();
            item.Add(new ts521 { ID = 1000000000000000001, SpellCode = "ces", Version = 1 });

            fsql.Update<ts521>().SetSource(item).UpdateColumns(info => info.SpellCode).ExecuteAffrows();
        }
        class ts521
        {
            [Key]
            [Snowflake]
            public long ID { get; set; }

            [Description("名字")]
            public string Name { get; set; }

            [Description("账号")]
            [Column(IsNullable = false)]
            public string Account { get; set; }

            [Description("名称拼音首字母")]
            public string SpellCode { get; set; }

            [Description("乐观锁")]
            [Column(IsVersion = true, InsertValueSql = "1")]
            public long Version { get; set; }
        }
        public class SnowflakeAttribute: Attribute { }
    }
}
