using FreeSql.DataAnnotations;
using FreeSql.Tests.DataContext.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.SqlServerExpression
{
    [Collection("SqlServerCollection")]
    public class DateTimeTest
    {
        SqlServerFixture _sqlserverFixture;

        public DateTimeTest(SqlServerFixture sqlserverFixture)
        {
            _sqlserverFixture = sqlserverFixture;
        }

        ISelect<Topic> select => _sqlserverFixture.SqlServer.Select<Topic>();

        [Table(Name = "tb_topic111333")]
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
        [Table(Name = "TestTypeInfo333")]
        class TestTypeInfo
        {
            [Column(IsIdentity = true)]
            public int Guid { get; set; }
            public int ParentId { get; set; }
            public TestTypeParentInfo Parent { get; set; }
            public string Name { get; set; }
            public DateTime Time { get; set; }
        }
        [Table(Name = "TestTypeParentInfo23123")]
        class TestTypeParentInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<TestTypeInfo> Types { get; set; }
            public DateTime Time2 { get; set; }
        }
        [Fact]
        public void Now()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Date == DateTime.Now.Date).ToList());
        }
        [Fact]
        public void UtcNow()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Date == DateTime.UtcNow.Date).ToList());
        }
        [Fact]
        public void MinValue()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Date == DateTime.MinValue.Date).ToList());
        }
        [Fact]
        public void MaxValue()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Date == DateTime.MaxValue.Date).ToList());
        }
        [Fact]
        public void Date()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Date == DateTime.Now.Date).ToList());
            data.Add(select.Where(a => a.Type.Time.Date > DateTime.Now.Date).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Date > DateTime.Now.Date).ToList());

            data.Add(select.Where(a => DateTime.Now.Subtract(a.CreateTime.Date).TotalSeconds > 0).ToList());
            data.Add(select.Where(a => DateTime.Now.Subtract(a.Type.Time.Date).TotalSeconds > 0).ToList());
            data.Add(select.Where(a => DateTime.Now.Subtract(a.Type.Parent.Time2.Date).TotalSeconds > 0).ToList());
        }
        [Fact]
        public void TimeOfDay()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay == DateTime.Now.TimeOfDay).ToList());
            data.Add(select.Where(a => a.Type.Time.TimeOfDay > DateTime.Now.TimeOfDay).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.TimeOfDay > DateTime.Now.TimeOfDay).ToList());
        }
        [Fact]
        public void DayOfWeek()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.DayOfWeek > DateTime.Now.DayOfWeek).ToList());
            data.Add(select.Where(a => a.Type.Time.DayOfWeek > DateTime.Now.DayOfWeek).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.DayOfWeek > DateTime.Now.DayOfWeek).ToList());
        }
        [Fact]
        public void Day()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Day > DateTime.Now.Day).ToList());
            data.Add(select.Where(a => a.Type.Time.Day > DateTime.Now.Day).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Day > DateTime.Now.Day).ToList());
        }
        [Fact]
        public void DayOfYear()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.DayOfYear > DateTime.Now.DayOfYear).ToList());
            data.Add(select.Where(a => a.Type.Time.DayOfYear > DateTime.Now.DayOfYear).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.DayOfYear > DateTime.Now.DayOfYear).ToList());
        }
        [Fact]
        public void Month()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Month > DateTime.Now.Month).ToList());
            data.Add(select.Where(a => a.Type.Time.Month > DateTime.Now.Month).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Month > DateTime.Now.Month).ToList());
        }
        [Fact]
        public void Year()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Year > DateTime.Now.Year).ToList());
            data.Add(select.Where(a => a.Type.Time.Year > DateTime.Now.Year).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Year > DateTime.Now.Year).ToList());
        }
        [Fact]
        public void Hour()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Hour > DateTime.Now.Hour).ToList());
            data.Add(select.Where(a => a.Type.Time.Hour > DateTime.Now.Hour).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Hour > DateTime.Now.Hour).ToList());
        }
        [Fact]
        public void Minute()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Minute > DateTime.Now.Minute).ToList());
            data.Add(select.Where(a => a.Type.Time.Minute > DateTime.Now.Minute).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Minute > DateTime.Now.Minute).ToList());
        }
        [Fact]
        public void Second()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Second > DateTime.Now.Second).ToList());
            data.Add(select.Where(a => a.Type.Time.Second > DateTime.Now.Second).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Second > DateTime.Now.Second).ToList());
        }
        [Fact]
        public void Millisecond()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Millisecond > DateTime.Now.Millisecond).ToList());
            data.Add(select.Where(a => a.Type.Time.Millisecond > DateTime.Now.Millisecond).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Millisecond > DateTime.Now.Millisecond).ToList());
        }
        [Fact]
        public void Ticks()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Ticks > DateTime.Now.Ticks).ToList());
            data.Add(select.Where(a => a.Type.Time.Ticks > DateTime.Now.Ticks).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Ticks > DateTime.Now.Ticks).ToList());
        }
        [Fact]
        public void Add()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Add(TimeSpan.FromDays(1)) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.Add(TimeSpan.FromDays(1)) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Add(TimeSpan.FromDays(1)) > DateTime.Now).ToList());
        }
        [Fact]
        public void AddDays()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddDays(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddDays(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddDays(1) > DateTime.Now).ToList());
        }
        [Fact]
        public void AddHours()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddHours(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddHours(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddHours(1) > DateTime.Now).ToList());
        }
        [Fact]
        public void AddMilliseconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddMilliseconds(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddMilliseconds(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddMilliseconds(1) > DateTime.Now).ToList());
        }
        [Fact]
        public void AddMinutes()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddMinutes(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddMinutes(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddMinutes(1) > DateTime.Now).ToList());
        }
        [Fact]
        public void AddMonths()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddMonths(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddMonths(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddMonths(1) > DateTime.Now).ToList());
        }
        [Fact]
        public void AddSeconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddSeconds(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddSeconds(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddSeconds(1) > DateTime.Now).ToList());
        }
        [Fact]
        public void AddTicks()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddTicks(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddTicks(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddTicks(1) > DateTime.Now).ToList());
        }
        [Fact]
        public void AddYears()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddYears(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddYears(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddYears(1) > DateTime.Now).ToList());
        }
        [Fact]
        public void Subtract()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Subtract(DateTime.Now).TotalSeconds > 0).ToList());
            data.Add(select.Where(a => a.Type.Time.Subtract(DateTime.Now).TotalSeconds > 0).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Subtract(DateTime.Now).TotalSeconds > 0).ToList());

            data.Add(select.Where(a => a.CreateTime.Subtract(TimeSpan.FromDays(1)) > a.CreateTime).ToList());
            data.Add(select.Where(a => a.Type.Time.Subtract(TimeSpan.FromDays(1)) > a.CreateTime).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Subtract(TimeSpan.FromDays(1)) > a.CreateTime).ToList());
        }
        [Fact]
        public void 两个日期相减_效果同Subtract()
        {
            var data = new List<object>();
            data.Add(select.Where(a => (a.CreateTime - DateTime.Now).TotalSeconds > 0).ToList());
            data.Add(select.Where(a => (a.Type.Time - DateTime.Now).TotalSeconds > 0).ToList());
            data.Add(select.Where(a => (a.Type.Parent.Time2 - DateTime.Now).TotalSeconds > 0).ToList());

            data.Add(select.Where(a => (a.CreateTime - TimeSpan.FromDays(1)) > a.CreateTime).ToList());
            data.Add(select.Where(a => (a.Type.Time - TimeSpan.FromDays(1)) > a.CreateTime).ToList());
            data.Add(select.Where(a => (a.Type.Parent.Time2 - TimeSpan.FromDays(1)) > a.CreateTime).ToList());
        }
        [Fact]
        public void this_Equals()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddYears(1).Equals(DateTime.Now)).ToList());
            data.Add(select.Where(a => a.Type.Time.AddYears(1).Equals(DateTime.Now)).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddYears(1).Equals(DateTime.Now)).ToList());
        }
        [Fact]
        public void this_ToString()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.ToString().Equals(DateTime.Now)).ToList());
            data.Add(select.Where(a => a.Type.Time.AddYears(1).ToString().Equals(DateTime.Now)).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddYears(1).ToString().Equals(DateTime.Now)).ToList());
        }

        [Fact]
        public void DateTime_Compare()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.CompareTo(DateTime.Now) == 0).ToList());
            data.Add(select.Where(a => a.Type.Time.AddYears(1).CompareTo(DateTime.Now) == 0).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddYears(1).CompareTo(DateTime.Now) == 0).ToList());
        }
        [Fact]
        public void DateTime_DaysInMonth()
        {
            var data = new List<object>();
            data.Add(select.Where(a => DateTime.DaysInMonth(a.CreateTime.Year, a.CreateTime.Month) > 30).ToList());
            data.Add(select.Where(a => DateTime.DaysInMonth(a.Type.Time.Year, a.Type.Time.Month) > 30).ToList());
            data.Add(select.Where(a => DateTime.DaysInMonth(a.Type.Parent.Time2.Year, a.Type.Parent.Time2.Month) > 30).ToList());
        }
        [Fact]
        public void DateTime_Equals()
        {
            var data = new List<object>();
            data.Add(select.Where(a => DateTime.Equals(a.CreateTime.AddYears(1), DateTime.Now)).ToList());
            data.Add(select.Where(a => DateTime.Equals(a.Type.Time.AddYears(1), DateTime.Now)).ToList());
            data.Add(select.Where(a => DateTime.Equals(a.Type.Parent.Time2.AddYears(1), DateTime.Now)).ToList());
        }
        [Fact]
        public void DateTime_IsLeapYear()
        {
            var data = new List<object>();
            data.Add(select.Where(a => DateTime.IsLeapYear(a.CreateTime.Year)).ToList());
            data.Add(select.Where(a => DateTime.IsLeapYear(a.Type.Time.AddYears(1).Year)).ToList());
            data.Add(select.Where(a => DateTime.IsLeapYear(a.Type.Parent.Time2.AddYears(1).Year)).ToList());
        }
        [Fact]
        public void DateTime_Parse()
        {
            var data = new List<object>();
            data.Add(select.Where(a => DateTime.Parse(a.CreateTime.ToString()) > DateTime.Now).ToList());
            data.Add(select.Where(a => DateTime.Parse(a.Type.Time.AddYears(1).ToString()) > DateTime.Now).ToList());
            data.Add(select.Where(a => DateTime.Parse(a.Type.Parent.Time2.AddYears(1).ToString()) > DateTime.Now).ToList());
        }
    }
}
