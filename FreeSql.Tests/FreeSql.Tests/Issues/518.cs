using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _518
    {
        [Fact]
        public void SelectTest()
        {
            IFreeSql free = g.sqlserver;

            //创建测试数据
            using (var db = free.CreateDbContext())
            {
                db.Set<TestEntity518>().Remove(t => true); //清空旧数据
                db.SaveChanges();

                //插入三条测试数据
                db.Add(new TestEntity518() { ID = "A", Name = "张三", Age = 18 });
                db.Add(new TestEntity518() { ID = "B", Name = "李四", Age = 19 });
                db.Add(new TestEntity518() { ID = "C", Name = "王五", Age = 20 });
                db.SaveChanges();
            }

            //开始测试
            using (var db = free.CreateDbContext())
            {
                var entities = db.Set<TestEntity518>().Where(t => true).ToDictionary(t => t.ID);

                entities["A"].Age = 25;
                db.Update(entities["A"]);

                entities["B"].Age = 26;
                db.Update(entities["B"]);

                entities["C"].Age = 27;
                //entities["C"].Name = "王五5"; //注释掉这一行就不会报错
                db.Update(entities["C"]);

                db.Add(new TestEntity518() { ID = "D", Name = "马六", Age = 30 });

                db.SaveChanges();
            }
        }
        class TestEntity518
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
