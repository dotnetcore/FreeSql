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
            //var personsUpdate = new List<Person>
            //{
            //    new Person
            //    {
            //        Id = 1,
            //        Name = $"test2{DateTime.Now.Millisecond}",
            //        Age = 20,
            //        CreateTime = DateTime.Now
            //    },
            //    new Person
            //    {
            //        Id = 2,
            //        Name = "test3"+ 286,
            //        Age = 22,
            //        CreateTime = DateTime.Now
            //    }
            //};

            //_fsql.Update<Person>().SetSource()
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
    }
}