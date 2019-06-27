using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.PostgreSQLExpression
{
    public class ConvertTest
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
        public void ToBoolean()
        {
            var data = new List<object>();
            data.Add(select.Where(a => (Convert.ToBoolean(a.Clicks) ? 1 : 0) > 0).ToList());
            data.Add(select.Where(a => (bool.Parse(a.Clicks.ToString()) ? 1 : 0) > 0).ToList());
        }
        [Fact]
        public void ToByte()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Convert.ToByte(a.Clicks % 255) > 0).ToList());
            data.Add(select.Where(a => byte.Parse((a.Clicks % 255).ToString()) > 0).ToList());
        }
        [Fact]
        public void ToChar()
        {
            var data = new List<object>();
            //data.Add(select.Where(a => Convert.ToChar(a.Clicks) == '1').ToList());
            //data.Add(select.Where(a => char.Parse(a.Clicks.ToString()) == '1').ToList());
        }
        [Fact]
        public void ToDateTime()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Convert.ToDateTime(a.CreateTime.ToString()).Year > 0).ToList());
            data.Add(select.Where(a => DateTime.Parse(a.CreateTime.ToString()).Year > 0).ToList());
        }
        [Fact]
        public void ToDecimal()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Convert.ToDecimal(a.Clicks) > 0).ToList());
            data.Add(select.Where(a => decimal.Parse(a.Clicks.ToString()) > 0).ToList());
        }
        [Fact]
        public void ToDouble()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Convert.ToDouble(a.Clicks) > 0).ToList());
            data.Add(select.Where(a => double.Parse(a.Clicks.ToString()) > 0).ToList());
        }
        [Fact]
        public void ToInt16()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Convert.ToInt16(a.Clicks) > 0).ToList());
            data.Add(select.Where(a => short.Parse(a.Clicks.ToString()) > 0).ToList());
        }
        [Fact]
        public void ToInt32()
        {
            var data = new List<object>();
            data.Add(select.Where(a => (int)a.Clicks > 0).ToList());
            data.Add(select.Where(a => Convert.ToInt32(a.Clicks) > 0).ToList());
            data.Add(select.Where(a => int.Parse(a.Clicks.ToString()) > 0).ToList());
        }
        [Fact]
        public void ToInt64()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Convert.ToInt64(a.Clicks) > 0).ToList());
            data.Add(select.Where(a => long.Parse(a.Clicks.ToString()) > 0).ToList());
        }
        [Fact]
        public void ToSByte()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Convert.ToSByte(a.Clicks % 128) > 0).ToList());
            data.Add(select.Where(a => sbyte.Parse((a.Clicks % 128).ToString()) > 0).ToList());
        }
        [Fact]
        public void ToSingle()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Convert.ToSingle(a.Clicks) > 0).ToList());
            data.Add(select.Where(a => float.Parse(a.Clicks.ToString()) > 0).ToList());
        }
        [Fact]
        public void this_ToString()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Convert.ToString(a.Clicks).Equals("")).ToList());
            data.Add(select.Where(a => a.Clicks.ToString().Equals("")).ToList());
        }
        [Fact]
        public void ToUInt16()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Convert.ToUInt16(a.Clicks) > 0).ToList());
            data.Add(select.Where(a => ushort.Parse(a.Clicks.ToString()) > 0).ToList());
        }
        [Fact]
        public void ToUInt32()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Convert.ToUInt32(a.Clicks) > 0).ToList());
            data.Add(select.Where(a => uint.Parse(a.Clicks.ToString()) > 0).ToList());
        }
        [Fact]
        public void ToUInt64()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Convert.ToUInt64(a.Clicks) > 0).ToList());
            data.Add(select.Where(a => ulong.Parse(a.Clicks.ToString()) > 0).ToList());
        }

        [Fact]
        public void Guid_Parse()
        {
            var data = new List<object>();
            data.Add(select.Where(a => Guid.Parse(Guid.Empty.ToString()) == Guid.Empty).ToList());
        }

        [Fact]
        public void Guid_NewGuid()
        {
            var data = new List<object>();
            //data.Add(select.OrderBy(a => Guid.NewGuid()).Limit(10).ToList());
        }

        [Fact]
        public void Random()
        {
            var data = new List<object>();
            data.Add(select.Where(a => new Random().Next() > a.Clicks).Limit(10).ToList());
            data.Add(select.Where(a => new Random().NextDouble() > a.Clicks).Limit(10).ToList());
        }
    }
}
