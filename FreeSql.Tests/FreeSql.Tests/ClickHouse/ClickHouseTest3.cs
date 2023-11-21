using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                .UseNoneCommandParameter(false)
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
                new ArrayMappingTestSimple
                {
                    Name = "daily",
                    Tags1 = new [] { "e", "f", "g" },
                    Tags2 = new [] { 3, 45, 100, 400 },
                    Tags3 = new [] { false, true, false }
                }
            };
            var str  = _fsql.Insert(source).ExecuteAffrows();
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

        [Column(Name = "tags1")] public string [] Tags1 { get; set; }

        [Column(Name = "tags2")] public int[] Tags2 { get; set; }

        [Column(Name = "tags3")] public bool [] Tags3 { get; set; }
    }
}