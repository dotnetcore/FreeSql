using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.OracleExpression
{
    public class DateTimeTest
    {

        ISelect<Topic> select => g.oracle.Select<Topic>();

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
        [Table(Name = "TestTypeParentInf1")]
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
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (cast(date_format(a.`CreateTime`, '%Y-%m-%d') as datetime) = cast(date_format(now(), '%Y-%m-%d') as datetime))
        }
        [Fact]
        public void UtcNow()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Date == DateTime.UtcNow.Date).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (cast(date_format(a.`CreateTime`, '%Y-%m-%d') as datetime) = cast(date_format(utc_timestamp(), '%Y-%m-%d') as datetime))
        }
        [Fact]
        public void MinValue()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Date == DateTime.MinValue.Date).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (cast(date_format(a.`CreateTime`, '%Y-%m-%d') as datetime) = cast(date_format(cast('0001/1/1 0:00:00' as datetime), '%Y-%m-%d') as datetime))
        }
        [Fact]
        public void MaxValue()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Date == DateTime.MaxValue.Date).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (cast(date_format(a.`CreateTime`, '%Y-%m-%d') as datetime) = cast(date_format(cast('9999/12/31 23:59:59' as datetime), '%Y-%m-%d') as datetime))
        }
        [Fact]
        public void Date()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Date == DateTime.Now.Date).ToList());
            data.Add(select.Where(a => a.Type.Time.Date > DateTime.Now.Date).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Date > DateTime.Now.Date).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (cast(date_format(a.`CreateTime`, '%Y-%m-%d') as datetime) = cast(date_format(now(), '%Y-%m-%d') as datetime));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (cast(date_format(a__Type.`Time`, '%Y-%m-%d') as datetime) > cast(date_format(now(), '%Y-%m-%d') as datetime));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (cast(date_format(a__Type__Parent.`Time2`, '%Y-%m-%d') as datetime) > cast(date_format(now(), '%Y-%m-%d') as datetime));
            data.Add(select.Where(a => DateTime.Now.Subtract(a.CreateTime.Date).TotalSeconds > 0).ToList());
            data.Add(select.Where(a => DateTime.Now.Subtract(a.Type.Time.Date).TotalSeconds > 0).ToList());
            data.Add(select.Where(a => DateTime.Now.Subtract(a.Type.Parent.Time2.Date).TotalSeconds > 0).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (((timestampdiff(microsecond, cast(date_format(a.`CreateTime`, '%Y-%m-%d') as datetime), now())) / 1000000) > 0);

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (((timestampdiff(microsecond, cast(date_format(a__Type.`Time`, '%Y-%m-%d') as datetime), now())) / 1000000) > 0);

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (((timestampdiff(microsecond, cast(date_format(a__Type__Parent.`Time2`, '%Y-%m-%d') as datetime), now())) / 1000000) > 0)
        }
        [Fact]
        public void TimeOfDay()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay == DateTime.Now.TimeOfDay).ToList());
            data.Add(select.Where(a => a.Type.Time.TimeOfDay > DateTime.Now.TimeOfDay).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.TimeOfDay > DateTime.Now.TimeOfDay).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE ((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000) = (timestampdiff(microsecond, date_format(now(), '1970-1-1 %H:%i:%s.%f'), now()) + 62135596800000000));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE ((timestampdiff(microsecond, date_format(a__Type.`Time`, '1970-1-1 %H:%i:%s.%f'), a__Type.`Time`) + 62135596800000000) > (timestampdiff(microsecond, date_format(now(), '1970-1-1 %H:%i:%s.%f'), now()) + 62135596800000000));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE ((timestampdiff(microsecond, date_format(a__Type__Parent.`Time2`, '1970-1-1 %H:%i:%s.%f'), a__Type__Parent.`Time2`) + 62135596800000000) > (timestampdiff(microsecond, date_format(now(), '1970-1-1 %H:%i:%s.%f'), now()) + 62135596800000000))
        }
        [Fact]
        public void DayOfWeek()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.DayOfWeek > DateTime.Now.DayOfWeek).ToList());
            data.Add(select.Where(a => a.Type.Time.DayOfWeek > DateTime.Now.DayOfWeek).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.DayOfWeek > DateTime.Now.DayOfWeek).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE ((dayofweek(a.`CreateTime`) - 1) > (dayofweek(now()) - 1));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE ((dayofweek(a__Type.`Time`) - 1) > (dayofweek(now()) - 1));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE ((dayofweek(a__Type__Parent.`Time2`) - 1) > (dayofweek(now()) - 1))
        }
        [Fact]
        public void Day()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Day > DateTime.Now.Day).ToList());
            data.Add(select.Where(a => a.Type.Time.Day > DateTime.Now.Day).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Day > DateTime.Now.Day).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (dayofmonth(a.`CreateTime`) > dayofmonth(now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (dayofmonth(a__Type.`Time`) > dayofmonth(now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (dayofmonth(a__Type__Parent.`Time2`) > dayofmonth(now()))
        }
        [Fact]
        public void DayOfYear()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.DayOfYear > DateTime.Now.DayOfYear).ToList());
            data.Add(select.Where(a => a.Type.Time.DayOfYear > DateTime.Now.DayOfYear).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.DayOfYear > DateTime.Now.DayOfYear).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (dayofyear(a.`CreateTime`) > dayofyear(now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (dayofyear(a__Type.`Time`) > dayofyear(now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (dayofyear(a__Type__Parent.`Time2`) > dayofyear(now()))
        }
        [Fact]
        public void Month()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Month > DateTime.Now.Month).ToList());
            data.Add(select.Where(a => a.Type.Time.Month > DateTime.Now.Month).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Month > DateTime.Now.Month).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (month(a.`CreateTime`) > month(now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (month(a__Type.`Time`) > month(now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (month(a__Type__Parent.`Time2`) > month(now()))
        }
        [Fact]
        public void Year()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Year > DateTime.Now.Year).ToList());
            data.Add(select.Where(a => a.Type.Time.Year > DateTime.Now.Year).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Year > DateTime.Now.Year).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (year(a.`CreateTime`) > year(now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (year(a__Type.`Time`) > year(now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (year(a__Type__Parent.`Time2`) > year(now()))
        }
        [Fact]
        public void Hour()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Hour > DateTime.Now.Hour).ToList());
            data.Add(select.Where(a => a.Type.Time.Hour > DateTime.Now.Hour).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Hour > DateTime.Now.Hour).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (hour(a.`CreateTime`) > hour(now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (hour(a__Type.`Time`) > hour(now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (hour(a__Type__Parent.`Time2`) > hour(now()))
        }
        [Fact]
        public void Minute()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Minute > DateTime.Now.Minute).ToList());
            data.Add(select.Where(a => a.Type.Time.Minute > DateTime.Now.Minute).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Minute > DateTime.Now.Minute).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (minute(a.`CreateTime`) > minute(now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (minute(a__Type.`Time`) > minute(now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (minute(a__Type__Parent.`Time2`) > minute(now()))
        }
        [Fact]
        public void Second()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Second > DateTime.Now.Second).ToList());
            data.Add(select.Where(a => a.Type.Time.Second > DateTime.Now.Second).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Second > DateTime.Now.Second).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (second(a.`CreateTime`) > second(now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (second(a__Type.`Time`) > second(now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (second(a__Type__Parent.`Time2`) > second(now()))
        }
        [Fact]
        public void Millisecond()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Millisecond > DateTime.Now.Millisecond).ToList());
            data.Add(select.Where(a => a.Type.Time.Millisecond > DateTime.Now.Millisecond).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Millisecond > DateTime.Now.Millisecond).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (floor(microsecond(a.`CreateTime`) / 1000) > floor(microsecond(now()) / 1000));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (floor(microsecond(a__Type.`Time`) / 1000) > floor(microsecond(now()) / 1000));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (floor(microsecond(a__Type__Parent.`Time2`) / 1000) > floor(microsecond(now()) / 1000))
        }
        [Fact]
        public void Ticks()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Ticks > DateTime.Now.Ticks).ToList());
            data.Add(select.Where(a => a.Type.Time.Ticks > DateTime.Now.Ticks).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Ticks > DateTime.Now.Ticks).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE ((timestampdiff(microsecond, '1970-1-1', a.`CreateTime`) * 10 + 621355968000000000) > (timestampdiff(microsecond, '1970-1-1', now()) * 10 + 621355968000000000));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE ((timestampdiff(microsecond, '1970-1-1', a__Type.`Time`) * 10 + 621355968000000000) > (timestampdiff(microsecond, '1970-1-1', now()) * 10 + 621355968000000000));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE ((timestampdiff(microsecond, '1970-1-1', a__Type__Parent.`Time2`) * 10 + 621355968000000000) > (timestampdiff(microsecond, '1970-1-1', now()) * 10 + 621355968000000000))
        }
        [Fact]
        public void Add()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Add(TimeSpan.FromDays(1)) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.Add(TimeSpan.FromDays(1)) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Add(TimeSpan.FromDays(1)) > DateTime.Now).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (date_add(a.`CreateTime`, interval ((1 * 86400000000)) microsecond) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (date_add(a__Type.`Time`, interval ((1 * 86400000000)) microsecond) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (date_add(a__Type__Parent.`Time2`, interval ((1 * 86400000000)) microsecond) > now())
        }
        [Fact]
        public void AddDays()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddDays(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddDays(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddDays(1) > DateTime.Now).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (date_add(a.`CreateTime`, interval (1) day) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (date_add(a__Type.`Time`, interval (1) day) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (date_add(a__Type__Parent.`Time2`, interval (1) day) > now())
        }
        [Fact]
        public void AddHours()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddHours(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddHours(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddHours(1) > DateTime.Now).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (date_add(a.`CreateTime`, interval (1) hour) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (date_add(a__Type.`Time`, interval (1) hour) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (date_add(a__Type__Parent.`Time2`, interval (1) hour) > now())
        }
        [Fact]
        public void AddMilliseconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddMilliseconds(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddMilliseconds(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddMilliseconds(1) > DateTime.Now).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (date_add(a.`CreateTime`, interval (1) * 1000 microsecond) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (date_add(a__Type.`Time`, interval (1) * 1000 microsecond) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (date_add(a__Type__Parent.`Time2`, interval (1) * 1000 microsecond) > now())
        }
        [Fact]
        public void AddMinutes()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddMinutes(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddMinutes(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddMinutes(1) > DateTime.Now).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (date_add(a.`CreateTime`, interval (1) minute) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (date_add(a__Type.`Time`, interval (1) minute) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (date_add(a__Type__Parent.`Time2`, interval (1) minute) > now())
        }
        [Fact]
        public void AddMonths()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddMonths(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddMonths(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddMonths(1) > DateTime.Now).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (date_add(a.`CreateTime`, interval (1) month) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (date_add(a__Type.`Time`, interval (1) month) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (date_add(a__Type__Parent.`Time2`, interval (1) month) > now())
        }
        [Fact]
        public void AddSeconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddSeconds(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddSeconds(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddSeconds(1) > DateTime.Now).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (date_add(a.`CreateTime`, interval (1) second) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (date_add(a__Type.`Time`, interval (1) second) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (date_add(a__Type__Parent.`Time2`, interval (1) second) > now())
        }
        [Fact]
        public void AddTicks()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddTicks(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddTicks(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddTicks(1) > DateTime.Now).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (date_add(a.`CreateTime`, interval (1) / 10 microsecond) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (date_add(a__Type.`Time`, interval (1) / 10 microsecond) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (date_add(a__Type__Parent.`Time2`, interval (1) / 10 microsecond) > now())
        }
        [Fact]
        public void AddYears()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddYears(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Time.AddYears(1) > DateTime.Now).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddYears(1) > DateTime.Now).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (date_add(a.`CreateTime`, interval (1) year) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (date_add(a__Type.`Time`, interval (1) year) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (date_add(a__Type__Parent.`Time2`, interval (1) year) > now())
        }
        [Fact]
        public void Subtract()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.Subtract(DateTime.Now).TotalSeconds > 0).ToList());
            data.Add(select.Where(a => a.Type.Time.Subtract(DateTime.Now).TotalSeconds > 0).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Subtract(DateTime.Now).TotalSeconds > 0).ToList());
            //SELECT a."ID", a."CLICKS", a."TYPEGUID", a."TITLE", a."CREATETIME" 
            //FROM "TB_TOPIC111333" a 
            //WHERE ((extract(day from (systimestamp-a."CREATETIME"))*86400+extract(hour from (systimestamp-a."CREATETIME"))*3600+extract(minute from (systimestamp-a."CREATETIME"))*60+extract(second from (systimestamp-a."CREATETIME"))) > 0)

            //SELECT a."ID", a."CLICKS", a."TYPEGUID", a__Type."GUID", a__Type."PARENTID", a__Type."NAME", a__Type."TIME", a."TITLE", a."CREATETIME" 
            //FROM "TB_TOPIC111333" a 
            //LEFT JOIN "TESTTYPEINFO333" a__Type ON a__Type."GUID" = a."TYPEGUID" 
            //WHERE ((extract(day from (systimestamp-a__Type."TIME"))*86400+extract(hour from (systimestamp-a__Type."TIME"))*3600+extract(minute from (systimestamp-a__Type."TIME"))*60+extract(second from (systimestamp-a__Type."TIME"))) > 0)

            //SELECT a."ID", a."CLICKS", a."TYPEGUID", a__Type."GUID", a__Type."PARENTID", a__Type."NAME", a__Type."TIME", a."TITLE", a."CREATETIME" 
            //FROM "TB_TOPIC111333" a 
            //LEFT JOIN "TESTTYPEINFO333" a__Type ON a__Type."GUID" = a."TYPEGUID" 
            //LEFT JOIN "TESTTYPEPARENTINF1" a__Type__Parent ON a__Type__Parent."ID" = a__Type."PARENTID" 
            //WHERE ((extract(day from (systimestamp-a__Type__Parent."TIME2"))*86400+extract(hour from (systimestamp-a__Type__Parent."TIME2"))*3600+extract(minute from (systimestamp-a__Type__Parent."TIME2"))*60+extract(second from (systimestamp-a__Type__Parent."TIME2"))) > 0)
            data.Add(select.Where(a => a.CreateTime.Subtract(TimeSpan.FromDays(1)) > a.CreateTime).ToList());
            data.Add(select.Where(a => a.Type.Time.Subtract(TimeSpan.FromDays(1)) > a.CreateTime).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.Subtract(TimeSpan.FromDays(1)) > a.CreateTime).ToList());
            //SELECT a."ID", a."CLICKS", a."TYPEGUID", a."TITLE", a."CREATETIME" 
            //FROM "TB_TOPIC111333" a 
            //WHERE ((a."CREATETIME"-numtodsinterval((1)*86400,'second')) > a."CREATETIME")
            //SELECT a."ID", a."CLICKS", a."TYPEGUID", a__Type."GUID", a__Type."PARENTID", a__Type."NAME", a__Type."TIME", a."TITLE", a."CREATETIME" 

            //FROM "TB_TOPIC111333" a 
            //LEFT JOIN "TESTTYPEINFO333" a__Type ON a__Type."GUID" = a."TYPEGUID" 
            //WHERE ((a__Type."TIME"-numtodsinterval((1)*86400,'second')) > a."CREATETIME")
            //SELECT a."ID", a."CLICKS", a."TYPEGUID", a__Type."GUID", a__Type."PARENTID", a__Type."NAME", a__Type."TIME", a."TITLE", a."CREATETIME" 

            //FROM "TB_TOPIC111333" a 
            //LEFT JOIN "TESTTYPEINFO333" a__Type ON a__Type."GUID" = a."TYPEGUID" 
            //LEFT JOIN "TESTTYPEPARENTINF1" a__Type__Parent ON a__Type__Parent."ID" = a__Type."PARENTID" 
            //WHERE ((a__Type__Parent."TIME2"-numtodsinterval((1)*86400,'second')) > a."CREATETIME")
        }
        [Fact]
        public void 两个日期相减_效果同Subtract()
        {
            var data = new List<object>();
            data.Add(select.Where(a => (a.CreateTime - DateTime.Now).TotalSeconds > 0).ToList());
            data.Add(select.Where(a => (a.Type.Time - DateTime.Now).TotalSeconds > 0).ToList());
            data.Add(select.Where(a => (a.Type.Parent.Time2 - DateTime.Now).TotalSeconds > 0).ToList());
            //SELECT a."ID", a."CLICKS", a."TYPEGUID", a."TITLE", a."CREATETIME" 
            //FROM "TB_TOPIC111333" a 
            //WHERE ((extract(day from (systimestamp-a."CREATETIME"))*86400+extract(hour from (systimestamp-a."CREATETIME"))*3600+extract(minute from (systimestamp-a."CREATETIME"))*60+extract(second from (systimestamp-a."CREATETIME"))) > 0)

            //SELECT a."ID", a."CLICKS", a."TYPEGUID", a__Type."GUID", a__Type."PARENTID", a__Type."NAME", a__Type."TIME", a."TITLE", a."CREATETIME" 
            //FROM "TB_TOPIC111333" a 
            //LEFT JOIN "TESTTYPEINFO333" a__Type ON a__Type."GUID" = a."TYPEGUID" 
            //WHERE ((extract(day from (systimestamp-a__Type."TIME"))*86400+extract(hour from (systimestamp-a__Type."TIME"))*3600+extract(minute from (systimestamp-a__Type."TIME"))*60+extract(second from (systimestamp-a__Type."TIME"))) > 0)

            //SELECT a."ID", a."CLICKS", a."TYPEGUID", a__Type."GUID", a__Type."PARENTID", a__Type."NAME", a__Type."TIME", a."TITLE", a."CREATETIME" 
            //FROM "TB_TOPIC111333" a 
            //LEFT JOIN "TESTTYPEINFO333" a__Type ON a__Type."GUID" = a."TYPEGUID" 
            //LEFT JOIN "TESTTYPEPARENTINF1" a__Type__Parent ON a__Type__Parent."ID" = a__Type."PARENTID" 
            //WHERE ((extract(day from (systimestamp-a__Type__Parent."TIME2"))*86400+extract(hour from (systimestamp-a__Type__Parent."TIME2"))*3600+extract(minute from (systimestamp-a__Type__Parent."TIME2"))*60+extract(second from (systimestamp-a__Type__Parent."TIME2"))) > 0)
            data.Add(select.Where(a => (a.CreateTime - TimeSpan.FromDays(1)) > a.CreateTime).ToList());
            data.Add(select.Where(a => (a.Type.Time - TimeSpan.FromDays(1)) > a.CreateTime).ToList());
            data.Add(select.Where(a => (a.Type.Parent.Time2 - TimeSpan.FromDays(1)) > a.CreateTime).ToList());
            //SELECT a."ID", a."CLICKS", a."TYPEGUID", a."TITLE", a."CREATETIME" 
            //FROM "TB_TOPIC111333" a 
            //WHERE ((a."CREATETIME"-numtodsinterval((1)*86400,'second')) > a."CREATETIME")
            //SELECT a."ID", a."CLICKS", a."TYPEGUID", a__Type."GUID", a__Type."PARENTID", a__Type."NAME", a__Type."TIME", a."TITLE", a."CREATETIME" 

            //FROM "TB_TOPIC111333" a 
            //LEFT JOIN "TESTTYPEINFO333" a__Type ON a__Type."GUID" = a."TYPEGUID" 
            //WHERE ((a__Type."TIME"-numtodsinterval((1)*86400,'second')) > a."CREATETIME")
            //SELECT a."ID", a."CLICKS", a."TYPEGUID", a__Type."GUID", a__Type."PARENTID", a__Type."NAME", a__Type."TIME", a."TITLE", a."CREATETIME" 

            //FROM "TB_TOPIC111333" a 
            //LEFT JOIN "TESTTYPEINFO333" a__Type ON a__Type."GUID" = a."TYPEGUID" 
            //LEFT JOIN "TESTTYPEPARENTINF1" a__Type__Parent ON a__Type__Parent."ID" = a__Type."PARENTID" 
            //WHERE ((a__Type__Parent."TIME2"-numtodsinterval((1)*86400,'second')) > a."CREATETIME")
        }
        [Fact]
        public void this_Equals()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.AddYears(1).Equals(DateTime.Now)).ToList());
            data.Add(select.Where(a => a.Type.Time.AddYears(1).Equals(DateTime.Now)).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddYears(1).Equals(DateTime.Now)).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE ((date_add(a.`CreateTime`, interval (1) year) = now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE ((date_add(a__Type.`Time`, interval (1) year) = now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE ((date_add(a__Type__Parent.`Time2`, interval (1) year) = now()))
        }
        [Fact]
        public void this_ToString()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.ToString().Equals(DateTime.Now)).ToList());
            data.Add(select.Where(a => a.Type.Time.AddYears(1).ToString().Equals(DateTime.Now)).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddYears(1).ToString().Equals(DateTime.Now)).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE ((date_format(a.`CreateTime`, '%Y-%m-%d %H:%i:%s.%f') = now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE ((date_format(date_add(a__Type.`Time`, interval (1) year), '%Y-%m-%d %H:%i:%s.%f') = now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE ((date_format(date_add(a__Type__Parent.`Time2`, interval (1) year), '%Y-%m-%d %H:%i:%s.%f') = now()))
        }

        [Fact]
        public void DateTime_Compare()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.CompareTo(DateTime.Now) == 0).ToList());
            data.Add(select.Where(a => a.Type.Time.AddYears(1).CompareTo(DateTime.Now) == 0).ToList());
            data.Add(select.Where(a => a.Type.Parent.Time2.AddYears(1).CompareTo(DateTime.Now) == 0).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (((a.`CreateTime`) - (now())) = 0);

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (((date_add(a__Type.`Time`, interval (1) year)) - (now())) = 0);

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (((date_add(a__Type__Parent.`Time2`, interval (1) year)) - (now())) = 0)
        }
        [Fact]
        public void DateTime_DaysInMonth()
        {
            var data = new List<object>();
            data.Add(select.Where(a => DateTime.DaysInMonth(a.CreateTime.Year, a.CreateTime.Month) > 30).ToList());
            data.Add(select.Where(a => DateTime.DaysInMonth(a.Type.Time.Year, a.Type.Time.Month) > 30).ToList());
            data.Add(select.Where(a => DateTime.DaysInMonth(a.Type.Parent.Time2.Year, a.Type.Parent.Time2.Month) > 30).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (dayofmonth(last_day(concat(year(a.`CreateTime`), month(a.`CreateTime`), '-01'))) > 30);

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (dayofmonth(last_day(concat(year(a__Type.`Time`), month(a__Type.`Time`), '-01'))) > 30);

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (dayofmonth(last_day(concat(year(a__Type__Parent.`Time2`), month(a__Type__Parent.`Time2`), '-01'))) > 30)
        }
        [Fact]
        public void DateTime_Equals()
        {
            var data = new List<object>();
            data.Add(select.Where(a => DateTime.Equals(a.CreateTime.AddYears(1), DateTime.Now)).ToList());
            data.Add(select.Where(a => DateTime.Equals(a.Type.Time.AddYears(1), DateTime.Now)).ToList());
            data.Add(select.Where(a => DateTime.Equals(a.Type.Parent.Time2.AddYears(1), DateTime.Now)).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE ((date_add(a.`CreateTime`, interval (1) year) = now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE ((date_add(a__Type.`Time`, interval (1) year) = now()));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE ((date_add(a__Type__Parent.`Time2`, interval (1) year) = now()))
        }
        [Fact]
        public void DateTime_IsLeapYear()
        {
            var data = new List<object>();
            data.Add(select.Where(a => DateTime.IsLeapYear(a.CreateTime.Year)).ToList());
            data.Add(select.Where(a => DateTime.IsLeapYear(a.Type.Time.AddYears(1).Year)).ToList());
            data.Add(select.Where(a => DateTime.IsLeapYear(a.Type.Parent.Time2.AddYears(1).Year)).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (((year(a.`CreateTime`)) % 4 = 0 AND (year(a.`CreateTime`)) % 100 <> 0 OR (year(a.`CreateTime`)) % 400 = 0));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (((year(date_add(a__Type.`Time`, interval (1) year))) % 4 = 0 AND (year(date_add(a__Type.`Time`, interval (1) year))) % 100 <> 0 OR (year(date_add(a__Type.`Time`, interval (1) year))) % 400 = 0));

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (((year(date_add(a__Type__Parent.`Time2`, interval (1) year))) % 4 = 0 AND (year(date_add(a__Type__Parent.`Time2`, interval (1) year))) % 100 <> 0 OR (year(date_add(a__Type__Parent.`Time2`, interval (1) year))) % 400 = 0))
        }
        [Fact]
        public void DateTime_Parse()
        {
            var data = new List<object>();
            data.Add(select.Where(a => DateTime.Parse(a.CreateTime.ToString()) > DateTime.Now).ToList());
            data.Add(select.Where(a => DateTime.Parse(a.Type.Time.AddYears(1).ToString()) > DateTime.Now).ToList());
            data.Add(select.Where(a => DateTime.Parse(a.Type.Parent.Time2.AddYears(1).ToString()) > DateTime.Now).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic111333` a 
            //WHERE (cast(date_format(a.`CreateTime`, '%Y-%m-%d %H:%i:%s.%f') as datetime) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
            //WHERE (cast(date_format(date_add(a__Type.`Time`, interval (1) year), '%Y-%m-%d %H:%i:%s.%f') as datetime) > now());

            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
            //FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
            //WHERE (cast(date_format(date_add(a__Type__Parent.`Time2`, interval (1) year), '%Y-%m-%d %H:%i:%s.%f') as datetime) > now())
        }
    }
}
