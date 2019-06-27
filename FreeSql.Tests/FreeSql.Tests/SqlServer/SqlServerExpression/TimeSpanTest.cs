using FreeSql.DataAnnotations;
using FreeSql.Tests.DataContext.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.SqlServerExpression
{
    [Collection("SqlServerCollection")]
    public class TimeSpanTest
    {

        SqlServerFixture _sqlserverFixture;

        public TimeSpanTest(SqlServerFixture sqlserverFixture)
        {
            _sqlserverFixture = sqlserverFixture;
        }

        ISelect<Topic> select => _sqlserverFixture.SqlServer.Select<Topic>();

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
        public void Zero()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay > TimeSpan.Zero).ToList());
        }
        [Fact]
        public void MinValue()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay > TimeSpan.MinValue).ToList());
        }
        [Fact]
        public void MaxValue()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay < TimeSpan.MaxValue).ToList());
        }
        [Fact]
        public void Days()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Days == 0).ToList());
        }
        [Fact]
        public void Hours()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Hours > 0).ToSql());
        }
        [Fact]
        public void Milliseconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Milliseconds > 0).ToList());
        }
        [Fact]
        public void Minutes()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Minutes > 0).ToList());
        }
        [Fact]
        public void Seconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Seconds > 0).ToList());
        }
        [Fact]
        public void Ticks()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Ticks > 0).ToList());
        }
        [Fact]
        public void TotalDays()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.TotalDays > 0).ToList());
        }
        [Fact]
        public void TotalHours()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.TotalHours > 0).ToList());
        }
        [Fact]
        public void TotalMilliseconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.TotalMilliseconds > 0).ToList());
        }
        [Fact]
        public void TotalMinutes()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.TotalMinutes > 0).ToSql());
        }
        [Fact]
        public void TotalSeconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.TotalSeconds > 0).ToList());
        }
        [Fact]
        public void Add()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Add(TimeSpan.FromDays(1)) > TimeSpan.Zero).ToList());
        }
        [Fact]
        public void Subtract()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Subtract(TimeSpan.FromDays(1)) > TimeSpan.Zero).ToList());
        }
        [Fact]
        public void CompareTo()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.CompareTo(TimeSpan.FromDays(1)) > 0).ToList());
        }
        [Fact]
        public void this_Equals()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Equals(TimeSpan.FromDays(1))).ToList());
        }
        [Fact]
        public void this_ToString()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.ToString() == "ssss").ToList());
        }

        [Fact]
        public void TimeSpan_Compare()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Compare(a.CreateTime.TimeOfDay, TimeSpan.FromDays(1)) > 0).ToList());
        }
        [Fact]
        public void TimeSpan_Equals()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Equals(a.CreateTime.TimeOfDay, TimeSpan.FromDays(1))).ToList());
        }
        [Fact]
        public void TimeSpan_FromDays()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Equals(a.CreateTime.TimeOfDay, TimeSpan.FromDays(1))).ToList());
        }
        [Fact]
        public void TimeSpan_FromHours()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Equals(a.CreateTime.TimeOfDay, TimeSpan.FromHours(1))).ToList());
        }
        [Fact]
        public void TimeSpan_FromMilliseconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Equals(a.CreateTime.TimeOfDay, TimeSpan.FromMilliseconds(1))).ToList());
        }
        [Fact]
        public void TimeSpan_FromMinutes()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Equals(a.CreateTime.TimeOfDay, TimeSpan.FromMinutes(1))).ToList());
        }
        [Fact]
        public void TimeSpan_FromSeconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Equals(a.CreateTime.TimeOfDay, TimeSpan.FromSeconds(1))).ToList());
        }
        [Fact]
        public void TimeSpan_FromTicks()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Equals(a.CreateTime.TimeOfDay, TimeSpan.FromTicks(1))).ToList());
        }
        [Fact]
        public void TimeSpan_Parse()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Parse(a.CreateTime.TimeOfDay.ToString()) > TimeSpan.Zero).ToList());
        }
    }
}
