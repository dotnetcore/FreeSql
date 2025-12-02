using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.GBaseExpression
{
    public class TextTest
    {
        class TestStringInfo
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            [Column(StringLength = 32765)]
            public string Name { get; set; }
            
            [Column(StringLength = 500)]
            public string Address { get; set; }
            
            [Column(DbType = "TEXT")]
            public string Unit { get; set; }
 
            [Column(DbType = "CLOB")]
            public string Description { get; set; }
        }
        
        [Fact]
        public void StringInsert()
        {
            g.gbase.Insert(new TestStringInfo(){ Name = "王五", Address = "上海", Unit = "上海" }).ExecuteAffrows();
            g.gbase.Insert(new TestStringInfo(){ Name = "赵六", Address = "广州", Unit = "广州", Description = "COM"}).ExecuteAffrows();

            // 大对象测试
            var name = string.Concat(Enumerable.Repeat("张三", 5000));
            var unit = string.Concat(Enumerable.Repeat("hello 世界", 5000));
            var description = string.Concat(Enumerable.Repeat("for 地球", 5000));
            g.gbase.Insert(new TestStringInfo(){ Name = "张三",Description = description }).ExecuteAffrows();
            g.gbase.Insert(new TestStringInfo(){ Name = name, Address = "广州", Unit = "广州", Description = description}).ExecuteAffrows();
            g.gbase.Insert(new TestStringInfo(){ Name = name, Address = "广州", Unit = unit, Description = description}).ExecuteAffrows();
            
            Assert.True(true);
        }
    }
}
