using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace FreeSql.Tests.ClickHouse
{
    public class ClickHouseTest3
    {
        private static ITestOutputHelper _output;
        private static IFreeSql _fsql;

        public ClickHouseTest3(ITestOutputHelper output)
        {
            _output = output;
            _fsql = new FreeSqlBuilder().UseConnectionString(DataType.ClickHouse,
                    "Host=192.168.1.123;Port=8123;Database=test;Compress=True;Min Pool Size=1")
                .UseMonitorCommand(cmd => _output.WriteLine($"线程：{cmd.CommandText}\r\n"))
                .UseNoneCommandParameter(true)
                .Build();
        }

        /// <summary>
        /// 测试bool类型映射
        /// </summary>
        [Fact]
        public void TestBoolMappingSync()
        {
            _fsql.CodeFirst.SyncStructure(typeof(BoolMappingTest));
        }

        /// <summary>
        /// 测试bool类型插入
        /// </summary>
        [Fact]
        public void TestBoolMappingInsert()
        {
            _fsql.Insert(new BoolMappingTest
            {
                Name = "Tom",
                Age = 20,
                Id = Guid.NewGuid().ToString(),
                IsDelete = true,
                IsEnable = true
            }).ExecuteAffrows();

            _fsql.Insert(new BoolMappingTest
            {
                Name = "Jess",
                Age = 21,
                Id = Guid.NewGuid().ToString(),
                IsDelete = true,
                IsEnable = false
            }).ExecuteAffrows();

            _fsql.Insert(new BoolMappingTest
            {
                Name = "Daily",
                Age = 22,
                Id = Guid.NewGuid().ToString(),
                IsDelete = false,
                IsEnable = false
            }).ExecuteAffrows();
        }

        /// <summary>
        /// 测试bool类型修改
        /// </summary>
        [Fact]
        public void TestBoolMappingUpdateSet()
        {
            _fsql.Update<BoolMappingTest>()
                .Set(t => t.IsDelete, true)
                .Where(b => b.Age > 10)
                .ExecuteAffrows();
        }

        /// <summary>
        /// 测试bool类型修改
        /// </summary>
        [Fact]
        public void TestBoolMappingUpdate()
        {
            _fsql.Update<BoolMappingTest>()
                .SetSource(new BoolMappingTest
                {
                    Id = "af199304-239a-48da-9c75-1d5e36167d74",
                    IsEnable = false,
                    IsDelete = true,
                    Age = 60,
                    Name = "Update"
                })
                .ExecuteAffrows();
        }


        /// <summary>
        /// 测试bool类型查询
        /// </summary>
        [Fact]
        public void TestBoolMappingSelect()
        {
            var list = _fsql.Select<BoolMappingTest>().ToList();
        }

        /// <summary>
        /// 测试Array类型映射
        /// </summary>
        [Fact]
        public void ArrayBoolMappingSync()
        {
            _fsql.CodeFirst.SyncStructure(typeof(ArrayMappingTestSimple));
        }

        /// <summary>
        /// 测试Array类型插入
        /// </summary>
        [Fact]
        public void ArrayBoolMappingInsert()
        {
            var source = new List<ArrayMappingTestSimple>()
            {
                new()
                {
                    Name = "daily",
                    Tags1 = Array.Empty<string>(),
                    Tags2 = new[] { 3, 45, 100, 400 },
                    Tags3 = new[] { false, true, false }
                }
            };
            var str = _fsql.Insert(source).ExecuteAffrows();
        }

        /// <summary>
        /// 测试Array类型映射
        /// </summary>
        [Fact]
        public void ArrayBoolMappingSelect()
        {
            var list = _fsql.Select<ArrayMappingTestSimple>().ToList();
            _output.WriteLine(JsonConvert.SerializeObject(list));
        }

        /// <summary>
        /// 测试Array常用查询函数
        /// </summary>
        [Fact]
        public void ArraySelectAnySync()
        {
            var sql = _fsql.Select<ArrayMappingTestSimple>().Where(a => !a.Tags1.Any()).ToList(a => a.Name);
            _output.WriteLine(JsonConvert.SerializeObject(sql));
        }


        /// <summary>
        /// 测试Array常用查询函数
        /// </summary>
        [Fact]
        public void ArraySelectLengthSync()
        {
            var sql = _fsql.Select<ArrayMappingTestSimple>().ToList(a => a.Tags1.Count());
            _output.WriteLine(JsonConvert.SerializeObject(sql));

            var sql2 = _fsql.Select<ArrayMappingTestSimple>().Where(a => a.Tags1.Count() > 5).ToList(a => a.Tags1);
            _output.WriteLine(JsonConvert.SerializeObject(sql2));
        }


        /// <summary>
        /// 测试Array常用查询函数
        /// </summary>
        [Fact]
        public void ArraySelectContainsSync()
        {
            var sql = _fsql.Select<ArrayMappingTestSimple>().ToList(a => a.Tags1.Contains("a"));
            _output.WriteLine(JsonConvert.SerializeObject(sql));

            var sql2 = _fsql.Select<ArrayMappingTestSimple>().Where(a => a.Tags2.Contains(2)).ToList(a => a.Tags2);
            _output.WriteLine(JsonConvert.SerializeObject(sql2));
        }

        /// <summary>
        /// 测试Array常用查询函数
        /// </summary>
        [Fact]
        public void ArraySelectConcatSync()
        {
            var list = new List<string>() { "f" };
            var sql = _fsql.Select<ArrayMappingTestSimple>().ToList(a => a.Tags1.Concat(list));
            _output.WriteLine(JsonConvert.SerializeObject(sql));
        }

        /// <summary>
        /// 测试Array常用查询函数
        /// </summary>
        [Fact]
        public void ArraySelectConstContainsSync()
        {
            var list = new List<string>() { "daily", "a" };
            var sql = _fsql.Select<ArrayMappingTestSimple>().Where(a => list.Contains(a.Name)).ToList();
            _output.WriteLine(JsonConvert.SerializeObject(sql));
        }

        /// <summary>
        /// 测试Array常用查询函数
        /// </summary>
        [Fact]
        public void ArraySelectConstLengthSync()
        {
            var sql = _fsql.Select<ArrayMappingTestSimple>().ToList(a => "aaaa".Length);
            _output.WriteLine(JsonConvert.SerializeObject(sql));
        }

        /// <summary>
        /// 测试ArrayFilter测试
        /// </summary>
        [Fact]
        public void ArrayFilterFuncTest()
        {
            //var list = _fsql.Select<ArrayMappingTestSimple>().Where(a => a.Tags2.ArrayFilter(o => o == 1).Any())
            //    .ToSql();


            ////SELECT a.`name`, a.`tags1`, a.`tags2`, a.`tags3` 
            ////FROM `table_test_array_simple` a 
            ////WHERE (arrayFilter(x -> x = '1', a.`tags2`) != [])

            //_output.WriteLine(JsonConvert.SerializeObject(list));
        }

        /// <summary>
        /// 测试ArrayFilter测试
        /// </summary>
        [Fact]
        public void IsPrimaryTest()
        {
            _fsql.CodeFirst.SyncStructure<HttpContextRecord>();
        }

        /// <summary>
        /// https://github.com/dotnetcore/FreeSql/issues/969
        /// </summary>
        [Fact]
        public async Task UriStringIsTooLongTest()
        {
            _fsql.CodeFirst.SyncStructure<TestTable>();
            var json =
                "[{\"date\":\"2021-12-19T02:47:53.4365075 08:00\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}][{\"date\":\"2021-12-19T02:47:53.4365075 08:00\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}[{\"date\":\"2021-12-19T02:47:53.4365075 08:00\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}[{\"date\":\"2021-12-19T02:47:53.4365075 08:00\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}[{\"date\":\"2021-12-19T02:47:53.4365075 08:00\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}[{\"date\":\"2021-12-19T02:47:53.4365075 08:00\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}[{\"date\":\"2021-12-19T02:47:53.4365075 08:00\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}[{\"date\":\"2021-12-19T02:47:53.4365075 08:00\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}[{\"date\":\"2021-12-19T02:47:53.4365075 08:00\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}[{\"date\":\"2021-12-19T02:47:53.4365075 08:00\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}[{\"date\":\"2021-12-19T02:47:53.4365075 08:00\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}[{\"date\":\"2021-12-19T02:47:53.4365075 08:00\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}[{\"date\":\"2021-12-19T02:47:53.4365075 08:00\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}[{\"date\":\"2021-12-19T02:47:53.4365075 08:00\",\"temperatureC\":6,\"temperatureF\":42,\"summary\":\"Balmy\"},{\"date\":\"2021-12-20T02:47:53.4366893 08:00\",\"temperatureC\":36,\"temperatureF\":96,\"summary\":\"Bracing\"},{\"date\":\"2021-12-21T02:47:53.4366903 08:00\",\"temperatureC\":-15,\"temperatureF\":6,\"summary\":\"Bracing\"},{\"date\":\"2021-12-22T02:47:53.4366904 08:00\",\"temperatureC\":14,\"temperatureF\":57,\"summary\":\"Cool\"},{\"date\":\"2021-12-23T02:47:53.4366905 08:00\",\"temperatureC\":29,\"temperatureF\":84,\"summary\":\"Mild\"}";

            json += json;
            json += json;
            json += json;
            json += json;
            json += json;
            json += json;
            json += json;
            json += json;
            json += json;
            json += json;
            json += json;
            json += json;
            json += json;
            json += json;

            var t = new TestTable
            {
                Id = Guid.NewGuid().ToString(),
                Content = json,
                Content2 = json,
                Time = DateTime.Now
            };

            //单个插入报错
            await _fsql.Insert(t).ExecuteAffrowsAsync();

            // await _fsql.Insert(t).ExecuteQuestBulkCopyAsync();
        }


        /// <summary>
        /// 测试BulkCopy单条
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TestBulkCopySingle()
        {
            var t = new TestTable
            {
                Id = Guid.NewGuid().ToString(),
                Content = "1",
                Content2 = "2",
                Time = DateTime.Now
            };

            //单个插入报错
            await _fsql.Insert(t).ExecuteAffrowsAsync();

            await _fsql.Insert(t).ExecuteClickHouseBulkCopyAsync();

            _fsql.Insert(t).ExecuteClickHouseBulkCopy();
        }

        /// <summary>
        /// 测试BulkCopy多条
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TestBulkCopyMany()
        {
            var t = new List<TestTable>();

            foreach (var i in Enumerable.Range(0, 10))
            {
                t.Add(new TestTable
                {
                    Id = Guid.NewGuid().ToString(),
                    Content = i.ToString(),
                    Content2 = i.ToString(),
                    Time = DateTime.Now
                });
            }

            //单个插入报错
            await _fsql.Insert(t).ExecuteAffrowsAsync();

            //BulkCopy不会报错
            await _fsql.Insert(t).ExecuteClickHouseBulkCopyAsync(); 

            _fsql.Insert(t).ExecuteClickHouseBulkCopy();
        }
    }


    [Table(Name = "table_test_bool")]
    public class BoolMappingTest
    {
        [Column(IsPrimary = true, Name = "id")]
        public string Id { set; get; }

        [Column(Name = "name")] public string Name { get; set; }

        [Column(Name = "age")] public int Age { get; set; }

        [Column(Name = "is_delete")] public bool IsDelete { get; set; }

        [Column(Name = "is_enable")] public bool? IsEnable { get; set; }
    }

    [Table(Name = "table_test_array")]
    public class ArrayMappingTest
    {
        [Column(Name = "name", IsPrimary = true)]
        public string Name { get; set; }

        [Column(Name = "tags1")] public IEnumerable<string> Tags1 { get; set; }

        [Column(Name = "tags2")] public IList<string> Tags2 { get; set; }

        [Column(Name = "tags3")] public List<string> Tags3 { get; set; }

        [Column(Name = "tags4")] public ArrayList Tags4 { get; set; }

        [Column(Name = "tags5")] public Array Tags5 { get; set; }

        [Column(Name = "tags6")] public List<int> Tags6 { get; set; }

        [Column(Name = "tags7")] public IEnumerable<bool> Tags7 { get; set; }
    }

    [Table(Name = "table_test_array_simple")]
    public class ArrayMappingTestSimple

    {
        [Column(Name = "name", IsPrimary = true)]
        public string Name { get; set; }

        [Column(Name = "tags1")] public string[] Tags1 { get; set; }

        [Column(Name = "tags2")] public int[] Tags2 { get; set; }

        [Column(Name = "tags3")] public bool[] Tags3 { get; set; }
    }

    /// <summary>
    /// Http请求信息统计
    /// </summary>
    [Table(Name = "http_context_record")]
    public class HttpContextRecord
    {
        [Column(Name = "id", IsPrimary = true)]
        public string Id { get; set; }

        /// <summary>
        /// 请求模板
        /// </summary>
        [Column(Name = "request_total_key", StringLength = 80)]
        public string RequestTotalKey { get; set; }

        /// <summary>
        /// 请求量
        /// </summary>
        [Column(Name = "total")]
        public long Total { get; set; }

        /// <summary>
        /// 记录请求类型
        /// </summary>
        [Column(Name = "type")]
        public int Type { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        [Column(Name = "add_time")]
        public DateTime AddTime { get; set; }
    }

    public class ContentRecord
    {
        [Column(IsPrimary = true)] public string Id { get; set; }

        public string? Content1 { get; set; }

        public string? Content2 { get; set; }

        public string? Content3 { get; set; }

        public string Content4 { get; set; }
    }

    internal class TestTable
    {
        [Required] [Column(IsIdentity = true)] public string Id { get; set; }

        [Column(StringLength = -2)] public string Content { get; set; }

        [Column(StringLength = -2)] public string Content2 { get; set; }

        [Column(DbType = "DateTime64(3, 'Asia/Shanghai')")]
        public DateTime Time { get; set; }

        public override string ToString()
        {
            return $"Id:{Id}  Content:{Content}  Content2:{Content2}  Time:{Time}";
        }
    }
}