using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FreeSql.Tests.QuestDb.QuestDbIssue
{
    internal class QuestDbIssue : QuestDbTest
    {


        [Fact]
        public void Issue1757()
        {
            restFsql.CodeFirst.SyncStructure<Test0111>();
            var count=  fsql.Insert(new List<Test0111>() {
                new Test0111(){ 
                    CreateTime=DateTime.Now,
                    CustomId=2,  Name="test111",
                    Price=2,
                    Value=2 }
            }).ExecuteQuestBulkCopyAsync();

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