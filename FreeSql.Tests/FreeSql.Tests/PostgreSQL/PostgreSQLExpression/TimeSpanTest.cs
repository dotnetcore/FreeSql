using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.PostgreSQLExpression
{
    public class TimeSpanTest
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
        public void Zero()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay > TimeSpan.Zero).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000) > 0)
        }
        [Fact]
        public void MinValue()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay > TimeSpan.MinValue).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000) > -922337203685477580)
        }
        [Fact]
        public void MaxValue()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay < TimeSpan.MaxValue).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000) < 922337203685477580)
        }
        [Fact]
        public void Days()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Days == 0).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000)) div 86400000000) = 0)
        }
        [Fact]
        public void Hours()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Hours > 0).ToSql());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000)) div 3600000000) mod 24 > 0)
        }
        [Fact]
        public void Milliseconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Milliseconds > 0).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000)) div 1000 mod 1000) > 0)
        }
        [Fact]
        public void Minutes()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Minutes > 0).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000)) div 60000000 mod 60) > 0)
        }
        [Fact]
        public void Seconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Seconds > 0).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000)) div 1000000 mod 60) > 0)
        }
        [Fact]
        public void Ticks()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Ticks > 0).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000)) * 10) > 0)
        }
        [Fact]
        public void TotalDays()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.TotalDays > 0).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000)) / 86400000000) > 0)
        }
        [Fact]
        public void TotalHours()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.TotalHours > 0).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000)) / 3600000000) > 0)
        }
        [Fact]
        public void TotalMilliseconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.TotalMilliseconds > 0).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000)) / 1000) > 0)
        }
        [Fact]
        public void TotalMinutes()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.TotalMinutes > 0).ToSql());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000)) / 60000000) > 0)
        }
        [Fact]
        public void TotalSeconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.TotalSeconds > 0).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000)) / 1000000) > 0)
        }
        [Fact]
        public void Add()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Add(TimeSpan.FromDays(1)) > TimeSpan.Zero).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE ((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000) + (1 * 86400000000)) > 0)
        }
        [Fact]
        public void Subtract()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Subtract(TimeSpan.FromDays(1)) > TimeSpan.Zero).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE ((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000) - (1 * 86400000000)) > 0)
        }
        [Fact]
        public void CompareTo()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.CompareTo(TimeSpan.FromDays(1)) > 0).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000)) - ((1 * 86400000000))) > 0)
        }
        [Fact]
        public void this_Equals()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.Equals(TimeSpan.FromDays(1))).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE ((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000) = (1 * 86400000000)))
        }
        [Fact]
        public void this_ToString()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.CreateTime.TimeOfDay.ToString() == "ssss").ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (date_format(date_add(cast('0001/1/1 0:00:00' as datetime), interval (timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000)) microsecond), '%Y-%m-%d %H:%i:%s.%f') = 'ssss')
        }

        [Fact]
        public void TimeSpan_Compare()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Compare(a.CreateTime.TimeOfDay, TimeSpan.FromDays(1)) > 0).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000)) - ((1 * 86400000000))) > 0)
        }
        [Fact]
        public void TimeSpan_Equals()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Equals(a.CreateTime.TimeOfDay, TimeSpan.FromDays(1))).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE ((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000) = (1 * 86400000000)))
        }
        [Fact]
        public void TimeSpan_FromDays()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Equals(a.CreateTime.TimeOfDay, TimeSpan.FromDays(1))).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE ((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000) = (1 * 86400000000)))
        }
        [Fact]
        public void TimeSpan_FromHours()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Equals(a.CreateTime.TimeOfDay, TimeSpan.FromHours(1))).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE ((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000) = (1 * 3600000000)))
        }
        [Fact]
        public void TimeSpan_FromMilliseconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Equals(a.CreateTime.TimeOfDay, TimeSpan.FromMilliseconds(1))).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE ((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000) = (1 * 1000)))
        }
        [Fact]
        public void TimeSpan_FromMinutes()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Equals(a.CreateTime.TimeOfDay, TimeSpan.FromMinutes(1))).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE ((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000) = (1 * 60000000)))
        }
        [Fact]
        public void TimeSpan_FromSeconds()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Equals(a.CreateTime.TimeOfDay, TimeSpan.FromSeconds(1))).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE ((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000) = (1 * 1000000)))
        }
        [Fact]
        public void TimeSpan_FromTicks()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Equals(a.CreateTime.TimeOfDay, TimeSpan.FromTicks(1))).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE ((timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000) = (1 / 10)))
        }
        [Fact]
        public void TimeSpan_Parse()
        {
            var data = new List<object>();
            data.Add(select.Where(a => TimeSpan.Parse(a.CreateTime.TimeOfDay.ToString()) > TimeSpan.Zero).ToList());
            //SELECT a.`Id` as1, a.`Clicks` as2, a.`TypeGuid` as3, a.`Title` as4, a.`CreateTime` as5 
            //FROM `tb_topic` a 
            //WHERE (cast(date_format(date_add(cast('0001/1/1 0:00:00' as datetime), interval (timestampdiff(microsecond, date_format(a.`CreateTime`, '1970-1-1 %H:%i:%s.%f'), a.`CreateTime`) + 62135596800000000)) microsecond), '%Y-%m-%d %H:%i:%s.%f') as signed) > 0)
        }
    }
}
