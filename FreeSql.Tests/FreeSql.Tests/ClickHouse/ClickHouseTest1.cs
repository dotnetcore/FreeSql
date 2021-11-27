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
            [FreeSql.DataAnnotations.Column(IsPrimary = true)]
            public long Id { get; set; }
            [Now]
            public DateTime CreateTime { get; set; }

            [FreeSql.DataAnnotations.Column(IsNullable = true )]
            public string Name { get; set; }

            [FreeSql.DataAnnotations.Column(IsNullable = false)]
            public int Age { get; set; }

            public bool State { get; set; }

            [FreeSql.DataAnnotations.Column(IsNullable = true)]
            public bool Enable { get; set; }

            public DateTime? UpdateTime { get; set; }

            public int? Points { get; set; }
        }
        [FreeSql.DataAnnotations.Table(Name = "ClickHouseTest")]
        public class TestClickHouse
        {
            [FreeSql.DataAnnotations.Column(IsPrimary = true)]
            [Now]
            public long Id { get; set; }

            public string Name { get; set; }
        }
        class NowAttribute: Attribute { }

        [Fact]
        public void AuditValue()
        {
            var id  = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
            var item = new TestClickHouse();
            item.Id = id;
            item.Name = "李四";
            EventHandler<Aop.AuditValueEventArgs> audit = (s, e) =>
            {
                if (e.Property.GetCustomAttribute<NowAttribute>(false) != null)
                    e.Value = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
            };
            g.clickHouse.Aop.AuditValue += audit;

            g.clickHouse.Insert(item).ExecuteAffrows();

            g.clickHouse.Aop.AuditValue -= audit;

            Assert.Equal(item.Id, id);
        }


        [Fact]
        public void CreateTalbe()
        {
            g.clickHouse.CodeFirst.SyncStructure<TestAuditValue>();
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

        [Fact]
        public void TestRepositorySelect()
        {
            var fsql = g.clickHouse;
            var list=fsql.GetRepository<TestClickHouse>().Where(o => o.Id > 900)
                .ToList();

        }

        [Fact]
        public void TestRepositoryInsert()
        {
            var fsql = g.clickHouse;
            long id = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(),0);
            var list=fsql.GetRepository<TestClickHouse>().Insert(new TestClickHouse { Id= id, Name="张三"});
            var data=fsql.GetRepository<TestClickHouse,long>().Get(id);
        }

        [Fact]
        public void TestDateTime()
        {
            var fsql = g.clickHouse;
            long id = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
            DateTime createTime=DateTime.Now;
            fsql.Insert(new TestAuditValue {
                Id = id, CreateTime = createTime, Age =18,Name="张三"
            }).ExecuteAffrows();
            
            var date1 = fsql.GetRepository<TestAuditValue>().Where(o => o.CreateTime == createTime)
                .ToList();
            var date2 = fsql.GetRepository<TestAuditValue>().Where(o => o.CreateTime.Date == createTime.Date)
                .ToList();
            var date3 = fsql.GetRepository<TestAuditValue>().Where(o => o.CreateTime.Year == createTime.Year)
                .ToList();
            var date4 = fsql.GetRepository<TestAuditValue>().Where(o => o.CreateTime.Month == createTime.Month)
                .ToList();
            var date5 = fsql.GetRepository<TestAuditValue>().Where(o => o.CreateTime.Second == createTime.Second)
                .ToList();
            var date6 = fsql.GetRepository<TestAuditValue>().Where(o => o.CreateTime.Millisecond == createTime.Millisecond)
                .ToList();
            var date7 = fsql.GetRepository<TestAuditValue>().Where(o => o.CreateTime.AddSeconds(10) < createTime)
                .ToList();

        }

        [Fact]
        public void TestRepositoryUpdateTime()
        {
            //暂时无法修改
            var fsql = g.clickHouse;
            var repository=fsql.GetRepository<TestAuditValue>();
            var list = repository.Select.ToList();
            list.ForEach(o=>o.UpdateTime = DateTime.Now);
            repository.Update(list);

        }

        [Fact]
        public void TestUpdateTime()
        {
            var fsql = g.clickHouse;
            var state=fsql.GetRepository<TestAuditValue>().UpdateDiy.Set(o=>o.UpdateTime,DateTime.Now).Where(o=>1==1).ExecuteAffrows();


        }

    }
}
