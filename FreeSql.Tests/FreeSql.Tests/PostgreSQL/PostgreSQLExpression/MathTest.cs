using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.PostgreSQLExpression
{
    public class MathTest
    {

        ISelect<Topic> select => g.pgsql.Select<Topic>();

        [Table(Name = "tb_topic")]
        class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int Clicks { get; set; }
            public int TypeGuid { get; set; }
            public TestTypeInfo Type { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }
        class TestTypeInfo
        {
            public int Guid { get; set; }
            public int ParentId { get; set; }
            public TestTypeParentInfo Parent { get; set; }
            public string Name { get; set; }
        }
        class TestTypeParentInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<TestTypeInfo> Types { get; set; }
        }

        [Fact]
        public void PI()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Math.PI + a.Clicks > 0).ToList());
        }
        [Fact]
        public void Abs()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Math.Abs(-a.Clicks) > 0).ToList());
        }
        [Fact]
        public void Sign()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Math.Sign(-a.Clicks) > 0).ToList());
        }
        [Fact]
        public void Floor()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Math.Floor(a.Clicks + 0.5) == a.Clicks).ToList());
        }
        [Fact]
        public void Ceiling()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Math.Ceiling(a.Clicks + 0.5) == a.Clicks + 1).ToList());
        }
        [Fact]
        public void Round()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Math.Round(a.Clicks + 0.5) == a.Clicks).ToList());
            data.Add(select.Where(a => Math.Round(a.Clicks + 0.5, 1) > a.Clicks).ToList());
        }
        [Fact]
        public void Exp()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Clicks < 100).Where(a => Math.Exp(1) == a.Clicks + 1).Limit(10).ToList());
        }
        [Fact]
        public void Log()
        {
            var data = new List<object>();
            //data.Add(select.Where(a => a.Clicks < 100).Where(a => Math.Log(a.Clicks + 0.5) == a.Clicks + 1).Limit(10).ToList());
        }
        [Fact]
        public void Log10()
        {
            var data = new List<object>();
            //data.Add(select.Where(a => a.Clicks < 100).Where(a => Math.Log10(a.Clicks + 0.5) == a.Clicks + 1).Limit(10).ToList());
        }
        [Fact]
        public void Pow()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Clicks < 100).Where(a => Math.Pow(2, a.Clicks) == a.Clicks + 1).Limit(10).ToList());
        }
        [Fact]
        public void Sqrt()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Clicks < 100).Where(a => Math.Sqrt(Math.Pow(2, a.Clicks)) == a.Clicks + 1).Limit(10).ToList());
        }
        [Fact]
        public void Cos()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Clicks < 100).Where(a => Math.Cos(Math.Pow(2, a.Clicks)) == a.Clicks + 1).Limit(10).ToList());
        }
        [Fact]
        public void Sin()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Clicks < 100).Where(a => Math.Sin(Math.Pow(2, a.Clicks)) == a.Clicks + 1).Limit(10).ToList());
        }
        [Fact]
        public void Tan()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Clicks < 100).Where(a => Math.Tan(Math.Pow(2, a.Clicks)) == a.Clicks + 1).Limit(10).ToList());
        }
        [Fact]
        public void Acos()
        {
            var data = new List<object>();
            //data.Add(select.Where(a => a.Clicks < 100).Where(a => Math.Acos(Math.Pow(2, a.Clicks)) == a.Clicks + 1).Limit(10).ToList());
        }
        [Fact]
        public void Asin()
        {
            var data = new List<object>();
            //data.Add(select.Where(a => a.Clicks < 100).Where(a => Math.Asin(Math.Pow(2, a.Clicks)) == a.Clicks + 1).Limit(10).ToList());
        }
        [Fact]
        public void Atan()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Clicks < 100).Where(a => Math.Atan(Math.Pow(2, a.Clicks)) == a.Clicks + 1).Limit(10).ToList());
        }
        [Fact]
        public void Atan2()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Clicks < 100).Where(a => Math.Atan2(2, a.Clicks) == a.Clicks + 1).Limit(10).ToList());
        }
        [Fact]
        public void Truncate()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Clicks < 100).Where(a => Math.Truncate(a.Clicks * 1.0 / 3) == a.Clicks + 1).Limit(10).ToList());
        }
    }
}
