using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using XY.Model.Business;
using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;

namespace FreeSql.Tests.ClickHouse
{
    public class ClickHouseTest1
    {
        private class TestAuditValue
        {
            [FreeSql.DataAnnotations.Column(IsPrimary = true)]
            public long Id { get; set; }

            [Now]
            public DateTime CreateTime { get; set; }

            [FreeSql.DataAnnotations.Column(IsNullable = true)]
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
            [FreeSql.DataAnnotations.Column(IsPrimary = true, IsIdentity = true)]
            [Now]
            public long Id { get; set; }

            public string Name { get; set; }
            public Decimal Money { get; set; }
        }

        private class NowAttribute : Attribute
        { }

        [Fact]
        public void AuditValue()
        {
            var id = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
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
        }

        [Fact]
        public void CreateTalbe()
        {
            g.clickHouse.CodeFirst.SyncStructure<TestAuditValue>();
        }

        [Fact]
        public void TestInsert()
        {
            Stopwatch stopwatch = new Stopwatch();
            var fsql = g.clickHouse;
            List<TestClickHouse> list = new List<TestClickHouse>();
            List<CollectDataEntity> list1 = new List<CollectDataEntity>();
            var date = DateTime.Now;
            for (int i = 1; i < 1000000; i++)
            {
                //list.Add(new TestClickHouse
                //{
                //     Id=i, Name=i.ToString()
                //});

                list1.Add(new CollectDataEntity
                {
                    Id = new Random().Next(),
                    CollectTime = DateTime.Now,
                    DataFlag = "1",
                    EquipmentCode = "11",
                    Guid = "11111",
                    UnitStr = "111",
                    PropertyCode = "1111"
                });
            }
            fsql.Delete<CollectDataEntity>().Where(t => 1 == 1).ExecuteAffrows();
            stopwatch.Start();
            var insert = fsql.Insert(list1);
            stopwatch.Stop();
            Debug.WriteLine("审计数据用时：" + stopwatch.ElapsedMilliseconds.ToString());
            stopwatch.Restart();
            insert.ExecuteAffrows();
            //fsql.GetRepository<CollectDataEntity>().Insert(list1);
            stopwatch.Stop();
            Debug.WriteLine("转换并插入用时：" + stopwatch.ElapsedMilliseconds.ToString());
            //var items = fsql.Select<TestClickHouse>().Where(o=>o.Id>900).OrderByDescending(o=>o.Id).ToList();
            //Assert.Equal(100, items.Count);
        }

        [Fact]
        public void TestPage()
        {
            var fsql = g.clickHouse;

            var list = fsql.Select<TestClickHouse>()
                .Page(1, 100)
                .Where(o => o.Id > 200 && o.Id < 500)
                .Count(out var count).ToList();
            //Assert.Equal(100, list.Count);
        }

        [Fact]
        public void TestDelete()
        {
            var fsql = g.clickHouse;
            var count1 = fsql.Select<TestClickHouse>().Count();
            fsql.Delete<TestClickHouse>().Where(o => o.Id < 500).ExecuteAffrows();
            var count2 = fsql.Select<TestClickHouse>().Count();
            //Assert.NotEqual(count1, count2);
        }

        [Fact]
        public void TestUpdate()
        {
            var fsql = g.clickHouse;
            fsql.Update<TestClickHouse>().Where(o => o.Id > 900)
                .Set(o => o.Name, "修改后的值")
                .ExecuteAffrows();
        }

        [Fact]
        public void TestRepositorySelect()
        {
            var fsql = g.clickHouse;
            var list = fsql.GetRepository<TestClickHouse>().Where(o => o.Id > 900)
                .ToList();
        }

        [Fact]
        public void TestRepositoryInsert()
        {
            var fsql = g.clickHouse;
            long id = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
            var list = fsql.GetRepository<TestClickHouse>().Insert(new TestClickHouse { Id = id, Name = "张三" });
            var data = fsql.GetRepository<TestClickHouse, long>().Get(id);
        }

        [Fact]
        public void TestDateTime()
        {
            var fsql = g.clickHouse;
            long id = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
            DateTime createTime = DateTime.Now;
            fsql.Insert(new TestAuditValue
            {
                Id = id,
                CreateTime = createTime,
                Age = 18,
                Name = "张三"
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
        public void TestUpdateTime()
        {
            var fsql = g.clickHouse;
            var state = fsql.GetRepository<TestAuditValue>().UpdateDiy.Set(o => o.UpdateTime, DateTime.Now).Where(o => 1 == 1).ExecuteAffrows();
            //var state1 = fsql.GetRepository<TestAuditValue>().UpdateDiy.Set(o => o.UpdateTime, null).Where(o => 1 == 1).ExecuteAffrows();
        }

        [Fact]
        public void TestRepositoryUpdateTime()
        {
            Stopwatch stopwatch = new Stopwatch();
            var fsql = g.clickHouse;
            var repository = fsql.GetRepository<TestAuditValue>();
            List<TestAuditValue> list = new List<TestAuditValue>();
            for (int i = 1; i < 5; i++)
            {
                list.Add(new TestAuditValue
                {
                    Id = new Random().Next(),
                    Age = 1,
                    Name = i.ToString(),
                    State = true,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    Enable = false
                });
            }
            list = repository.Insert(list);
            //var list = repository.Select.ToList();
            list.ForEach(o => o.UpdateTime = DateTime.Now);
            list.ForEach(o => o.Enable = true);
            stopwatch.Start();
            repository.Update(list);
            stopwatch.Stop();
            Debug.WriteLine("更新用时：" + stopwatch.ElapsedMilliseconds.ToString());
        }

        [Fact]
        public async void TestInsertUpdateData()
        {
            //g.clickHouse.CodeFirst.SyncStructure<CollectDataEntity>();
            Stopwatch stopwatch = new Stopwatch();
            var fsql = g.clickHouse;
            var repository = fsql.GetRepository<CollectDataEntity>();
            await repository.DeleteAsync(o => o.Id > 0);
            List<CollectDataEntity> tables = new List<CollectDataEntity>();
            for (int i = 1; i < 3; i++)
            {
                tables.Add(new CollectDataEntity
                {
                    Id = new Random().Next(),
                    CollectTime = DateTime.Now,
                    DataFlag = "1",
                    EquipmentCode = "11",
                    UnitStr = "111",
                    PropertyCode = "1111",
                    NumericValue = 1111.1119999912500M
                });
            }

            var insert = repository.Orm.Insert(tables);
            insert.ExecuteAffrows();
            var list = repository.Orm.Select<CollectDataEntity>().ToList();
            //var list = repository.Insert(tables);
            //var list = repository.Select.ToList();
            //list.ForEach(o=>o.EquipmentCode = "666");
            //stopwatch.Start();
            //await repository.UpdateAsync(list);
            //stopwatch.Stop();
            Debug.WriteLine("更新用时：" + stopwatch.ElapsedMilliseconds.ToString());
        }

        [Fact]
        public async void TestInsertDecimalData()
        {
            //g.clickHouse.CodeFirst.SyncStructure<CollectDataEntity>();
            Stopwatch stopwatch = new Stopwatch();
            var fsql = g.clickHouse;
            var repository = fsql.GetRepository<CollectDataEntity>();
            await repository.DeleteAsync(o => o.Id > 0);

            var insert = repository.Insert(new CollectDataEntity
            {
                Id = new Random().Next(),
                CollectTime = DateTime.Now,
                DataFlag = "1",
                EquipmentCode = "11",
                UnitStr = "111",
                PropertyCode = "1111",
                NumericValue = 1111.1119999912500M
            });
            var list = repository.Orm.Select<CollectDataEntity>().ToList();
            //var list = repository.Insert(tables);
            //var list = repository.Select.ToList();
            //list.ForEach(o=>o.EquipmentCode = "666");
            //stopwatch.Start();
            //await repository.UpdateAsync(list);
            //stopwatch.Stop();
            Debug.WriteLine("更新用时：" + stopwatch.ElapsedMilliseconds.ToString());
        }

        internal class Entity
        {
            [Required]
            public string Id { get; set; }

            [Column(StringLength = -2)]
            public string Content { get; set; }
        }

        [Fact]
        public void TestInsertNoneParameter()
        {
            var json = "[{\"date\":\t\"2021-12-19T02:47:53.4365075 08:00/\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}]";
            var data = new Entity { Id = Guid.NewGuid().ToString(), Content = json };

            var fsql = g.clickHouse;
            fsql.Insert(data).NoneParameter().ExecuteAffrows();
            var item = fsql.Select<Entity>().Where(a => a.Id == data.Id).First();
            Assert.Equal(item.Content, json);
        }

        [Fact]
        public void TestInsertUseParameter()
        {
            var fsql = g.clickHouse;
            fsql.CodeFirst.SyncStructure<Entity>();
            var json = "[{\"date\":\t\"2021-12-19T02:47:53.4365075 08:00/\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}]";
            var data = new Entity { Id = Guid.NewGuid().ToString(), Content = json };
            
            var sql1 = fsql.Insert(data).ToSql();
            fsql.Insert(data).ExecuteAffrows();
            var item = fsql.Select<Entity>().Where(a => a.Id == data.Id).First();
            Assert.Equal(item.Content, json);

            var data2 = new[]{ 
                new Entity { Id = Guid.NewGuid().ToString(), Content = json },
                new Entity { Id = Guid.NewGuid().ToString(), Content = json }
            };
            var sql2 = fsql.Insert(data2).ToSql();
            fsql.Insert(data2).ExecuteAffrows();
            item = fsql.Select<Entity>().Where(a => a.Id == data2[0].Id).First();
            Assert.Equal(item.Content, json);
            item = fsql.Select<Entity>().Where(a => a.Id == data2[1].Id).First();
            Assert.Equal(item.Content, json);
        }

        [Fact]
        public void DbFirst()
        {
            var fsql = g.clickHouse;
            var tbs = fsql.DbFirst.GetTablesByDatabase();
            var tbs2 = fsql.DbFirst.GetTablesByDatabase("default");
        }
    }
}