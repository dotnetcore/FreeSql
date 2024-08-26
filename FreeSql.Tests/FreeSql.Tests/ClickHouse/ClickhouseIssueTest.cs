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
    public class ClickhouseIssueTest
    {
        private readonly ITestOutputHelper _output;

        private static IFreeSql _fsql;

        public ClickhouseIssueTest(ITestOutputHelper output)
        {
            _output = output;

            _fsql = new FreeSqlBuilder().UseConnectionString(DataType.ClickHouse,
                    "Host=192.168.1.123;Port=8123;Database=test_issue;Compress=True;Min Pool Size=1")
                .UseMonitorCommand(cmd => _output.WriteLine($"线程：{cmd.CommandText}\r\n"))
                .UseNoneCommandParameter(true)
                .UseAdoConnectionPool(true)
                .Build();
        }

        #region https: //github.com/dotnetcore/FreeSql/issues/1813

        [Fact]
        public void TestIssue1813()
        {
            //普通修改
            _fsql.Update<Person>()
                .Set(p => p.Name == "update_name")
                .Set(p => p.UpdateTime == DateTime.Now)
                .Where(p => p.Id == "25e8d92e-29f2-43ff-b861-9ade0eec4041")
                .ExecuteAffrows();

            //批量修改
            var updatePerson = new List<Person>();
            updatePerson.Add(new Person
            {
                Id = "9cd7af52-85cc-4d26-898a-4020cadb0491",
                Name = "update_name1",
                UpdateTime = DateTime.Now,
                CreateTime = DateTime.Parse("2024-05-30 10:01:02")
            });

            updatePerson.Add(new Person
            {
                Id = "bd9f9ed6-bd03-4675-abb4-12b7fdac7678",
                Name = "update_name2",
                UpdateTime = DateTime.Now,
                CreateTime = DateTime.Parse("2024-05-30 10:01:02")
            });

            var sql = _fsql.Update<Person>().SetSource(updatePerson)
                .UpdateColumns(person => new
                {
                    person.Name,
                    person.UpdateTime,
                    person.CreateTime
                }).ToSql();
        }

        [Fact]
        public void TestIssue1813CodeFirst()
        {
            _fsql.CodeFirst.SyncStructure<Person>();
            var insertSingle = _fsql.Insert(new Person
            {
                Name = $"test{DateTime.Now.Millisecond}",
                Age = 18,
                CreateTime = DateTime.Now
            }).ExecuteAffrows();

            _output.WriteLine(insertSingle.ToString());

            var persons = new List<Person>
            {
                new Person
                {
                    Name = $"test2{DateTime.Now.Millisecond}",
                    Age = 20,
                    CreateTime = DateTime.Now
                },
                new Person
                {
                    Name = "test3" + 286,
                    Age = 22,
                    CreateTime = DateTime.Now
                }
            };

            var insertMany = _fsql.Insert(persons).ExecuteAffrows();
        }

        [Fact]
        public void TestIssue1813CodeFirst2()
        {
            _fsql.CodeFirst.SyncStructure<Person>();
            var insertSingle = _fsql.Insert(new Person
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"test{DateTime.Now.Millisecond}",
                Age = 18,
                CreateTime = DateTime.Now
            }).ExecuteAffrows();

            _output.WriteLine(insertSingle.ToString());

            var persons = new List<Person>
            {
                new Person
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"test2{DateTime.Now.Millisecond}",
                    Age = 20,
                    CreateTime = DateTime.Now
                },
                new Person
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "test3" + 286,
                    Age = 22,
                    CreateTime = DateTime.Now
                }
            };

            var insertMany = _fsql.Insert(persons).ExecuteAffrows();
        }


        public class Person
        {
            [Column(IsPrimary = true, IsIdentity = true)]
            public string Id { get; set; }

            public string Name { get; set; }
            public int Age { get; set; }

            public DateTime CreateTime { get; set; }

            public DateTime? UpdateTime { get; set; }
        }

        #endregion

        #region https: //github.com/dotnetcore/FreeSql/issues/1814

        public class Test1814Table
        {
            [Column(IsPrimary = true, IsIdentity = true)]
            public int Id { get; set; }

            public string Name { get; set; }

            [ClickHousePartition]
            [Column(Name = "create_time")]
            public DateTime CreateTime { get; set; }
        }

        [Fact]
        public void TestIssue1814()
        {
            _fsql.CodeFirst.SyncStructure<Test1814Table>();

            var insert = _fsql.Insert(new Test1814Table
            {
                Name = "test",
                CreateTime = DateTime.Now
            }).ExecuteAffrows();

            var query = _fsql.Select<Test1814Table>().ToList();
        }
        #endregion
    }
}