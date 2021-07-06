using FreeSql.DataAnnotations;
using System;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _804
    {
        [Table(Name = "Users_804")]
        class Users
        {
            [Column(IsPrimary = true, IsIdentity = true)]
            public int Id { get; set; }
            public string UserName { get; set; }

            public DateTime CreateTime { get; set; }
        }

        [Fact]
        public void TestYear()
        {
            var fsql = g.sqlite;
            fsql.Delete<Users>().Where("1=1").ExecuteAffrows();
            fsql.Insert<Users>().AppendData(new Users { UserName = "admn", CreateTime = DateTime.Now }).ExecuteAffrows();
            int year = DateTime.Now.Year;
            string strYear = year.ToString();
            //这个都能查到数据
            var d1 = fsql.Select<Users>().Where(r => r.CreateTime.Year == Convert.ToInt32(strYear)).ToList();
            /*
              SELECT a."Id", a."UserName", a."CreateTime" 
FROM "Users_677" a 
WHERE (strftime('%Y',a."CreateTime") = cast('2021' as smallint))
             */
            var d2 = fsql.Select<Users>().Where(r => r.CreateTime.Year == DateTime.Now.Year).ToList();
            var d3 = fsql.Select<Users>().Where(r => r.CreateTime.Year.ToString() == strYear).ToList();
            var d4 = fsql.Select<Users>().Where(r => Convert.ToInt32(r.CreateTime.Year) == year).ToList();

            //只有这种方式在sqlite下无法查到数据
            var d5 = fsql.Select<Users>().Where(r => r.CreateTime.Year == year).ToList();
            /*
SELECT a."Id", a."UserName", a."CreateTime" 
FROM "Users_677" a 
WHERE (strftime('%Y',a."CreateTime") = 2021)

             */
            Assert.Single(d1);
            Assert.Single(d2);
            Assert.Single(d3);
            Assert.Single(d4);
            Assert.Single(d5);

        }


        [Fact]
        public void TestMonthAndData()
        {
            var fsql = g.sqlite;
            fsql.Delete<Users>().Where("1=1").ExecuteAffrows();
            fsql.Insert<Users>().AppendData(new Users { UserName = "admin", CreateTime = DateTime.Now }).ExecuteAffrows();

            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;

            var d3 = fsql.Select<Users>().ToList(r => new
            {
                r.CreateTime,
                r.CreateTime.Date,
                r.CreateTime.TimeOfDay,
                r.CreateTime.DayOfWeek,
                r.CreateTime.Day,
                r.CreateTime.DayOfYear,
                r.CreateTime.Month,
                r.CreateTime.Year,
                r.CreateTime.Hour,
                r.CreateTime.Minute,
                r.CreateTime.Second,
                r.CreateTime.Millisecond,
                r.CreateTime.Ticks,
            });

            Assert.Single(d3);

            var d3_first = d3.First();

            Assert.Equal(d3_first.Date, d3_first.CreateTime.Date);
            //精度到毫秒
            Assert.Equal((long)(d3_first.TimeOfDay.TotalMilliseconds + 0.5), (long)(d3_first.CreateTime.TimeOfDay.TotalMilliseconds + 0.5));
            Assert.Equal(d3_first.DayOfWeek, d3_first.CreateTime.DayOfWeek);
            Assert.Equal(d3_first.Day, d3_first.CreateTime.Day);
            Assert.Equal(d3_first.DayOfYear, d3_first.CreateTime.DayOfYear);
            Assert.Equal(d3_first.Month, d3_first.CreateTime.Month);
            Assert.Equal(d3_first.Year, d3_first.CreateTime.Year);
            Assert.Equal(d3_first.Hour, d3_first.CreateTime.Hour);
            Assert.Equal(d3_first.Minute, d3_first.CreateTime.Minute);
            Assert.Equal(d3_first.Second, d3_first.CreateTime.Second);
            Assert.Equal(d3_first.Millisecond, d3_first.CreateTime.Millisecond);
            //精度到毫秒 ,四舍五入
            Assert.Equal((long)(d3_first.Ticks / 10000.0 + 0.5), (long)(d3_first.CreateTime.Ticks / 10000.0 + 0.5));


            string strMonth = month.ToString();
            var dmonth1 = fsql.Select<Users>().Where(r => r.CreateTime.Month.ToString() == strMonth).ToList();


            var d1 = fsql.Select<Users>().Where(r => r.CreateTime.Month == month).ToList();
            var d2 = fsql.Select<Users>().Where(r => r.CreateTime.Year == year).ToList();
            var d5 = fsql.Select<Users>().Where(r => r.CreateTime.Year == 2021).ToList();

            Assert.Single(dmonth1);
            Assert.Single(d1);
            Assert.Single(d1);
            Assert.Single(d5);
        }
    }
}
