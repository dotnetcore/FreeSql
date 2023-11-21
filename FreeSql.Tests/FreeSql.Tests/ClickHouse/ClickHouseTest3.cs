using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;
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
}