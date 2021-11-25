using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace FreeSql.Tests.MySql
{
    public class ClickHouseTest1
    {

        class TestAuditValue
        {
            public Guid id { get; set; }
            [Now]
            public DateTime createtime { get; set; }
        }
        [FreeSql.DataAnnotations.Table(Name = "ClickHouseTest")]
        public class TestClickHouse
        {
            public long Id { get; set; }

            public string Name { get; set; }
        }
        class NowAttribute: Attribute { }

        [Fact]
        public void AuditValue()
        {
            var date = DateTime.Now.Date;
            var item = new TestAuditValue();

            EventHandler<Aop.AuditValueEventArgs> audit = (s, e) =>
             {
                 if (e.Property.GetCustomAttribute<NowAttribute>(false) != null)
                     e.Value = DateTime.Now.Date;
             };
            g.mysql.Aop.AuditValue += audit;

            g.mysql.Insert(item).ExecuteAffrows();

            g.mysql.Aop.AuditValue -= audit;

            Assert.Equal(item.createtime, date);
        }

        [Fact]
        public void TestInsert()
        {
            var fsql = g.clickHouse;
            List<TestClickHouse> list=new List<TestClickHouse>();
            for (int i = 0; i < 1000; i++)
            {
                list.Add(new TestClickHouse()
                {
                    Id = i,
                    Name = $"测试{i}"
                });
            }
            fsql.Insert(list).ExecuteAffrows();
            var items = fsql.Select<TestClickHouse>().Where(o=>o.Id>900).OrderByDescending(o=>o.Id).ToList();
            Assert.Equal(100, items.Count);
        }
        [Fact]
        public void TestPage()
        {
            var fsql = g.clickHouse;

            var list=fsql.Select<TestClickHouse>()
                .Page(1,100)
                .Where(o=>o.Id>200&&o.Id<500)
                .Count(out var count).ToList();
            Assert.Equal(100, list.Count);
        }
        [Fact]
        public void TestDelete()
        {
            var fsql = g.clickHouse;
            var count1=fsql.Select<TestClickHouse>().Count();
            fsql.Delete<TestClickHouse>().Where(o => o.Id < 500).ExecuteAffrows();
            var count2 = fsql.Select<TestClickHouse>().Count();
            Assert.NotEqual(count1, count2);
        }
        [Fact]
        public void TestUpdate()
        {
            var fsql = g.clickHouse;
            fsql.Update<TestClickHouse>().Where(o => o.Id > 900)
                .Set(o=>o.Name,"修改后的值")
                .ExecuteAffrows();

        }

    }
}
