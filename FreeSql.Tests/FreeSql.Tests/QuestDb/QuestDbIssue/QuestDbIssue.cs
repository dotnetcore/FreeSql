using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FreeSql.Tests.QuestDb.QuestDbIssue
{
    public class QuestDbIssue : QuestDbTest
    {
        [Fact]
        public void Issue1757()
        {
            restFsql.CodeFirst.SyncStructure<Test0111>();
            var count = fsql.Insert(new List<Test0111>()
            {
                new()
                {
                    CreateTime = DateTime.Now,
                    CustomId = 3, Name = "test333",
                    Price = 3,
                    Value = 3
                }
            }).ExecuteQuestDbBulkCopy();

            Assert.True(count > 0);

            var list = fsql.Select<Test0111>().ToList();
        }


        [Fact]
        public void Issue1757Many()
        {
            restFsql.CodeFirst.SyncStructure<Test0111>();
            var count = fsql.Insert(new List<Test0111>()
            {
                new()
                {
                    CreateTime = DateTime.Now,
                    CustomId = 4, Name = "test444",
                    Price = 4,
                    Value = 4
                },
                new()
                {
                    CreateTime = DateTime.Now,
                    CustomId = 5, Name = "test555",
                    Price = 5,
                    Value = 5
                },
                new()
                {
                    CreateTime = DateTime.Now,
                    CustomId = 6, Name = "test666",
                    Price = 6,
                    Value = 6
                }
            }).ExecuteQuestDbBulkCopy();

            Assert.True(count > 0);

            var list = fsql.Select<Test0111>().ToList();
        }
    }

    public class Test0111
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateTime CreateTime { get; set; }
        public long CustomId { get; set; }

        public double Value { get; set; }
    }
}