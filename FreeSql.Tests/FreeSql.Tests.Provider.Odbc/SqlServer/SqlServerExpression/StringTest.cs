using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Odbc.SqlServerExpression
{
    [Collection("SqlServerCollection")]
    public class StringTest
    {

        ISelect<Topic> select => g.sqlserver.Select<Topic>();

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
            [Column(IsIdentity = true)]
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
        class TestEqualsGuid
        {
            public Guid id { get; set; }
        }

        [Fact]
        public void Equals__()
        {
            var list = new List<object>();
            list.Add(select.Where(a => a.Title.Equals("aaa")).ToList());
            list.Add(g.sqlserver.Select<TestEqualsGuid>().Where(a => a.id.Equals(Guid.Empty)).ToList());
        }

        [Fact]
        public void StringJoin()
        {
            var fsql = g.sqlserver;
            fsql.Delete<StringJoin01>().Where("1=1").ExecuteAffrows();
            fsql.Insert(new[] { new StringJoin01 { name = "北京" }, new StringJoin01 { name = "上海" }, new StringJoin01 { name = "深圳" }, }).ExecuteAffrows();

            var val1 = string.Join(",", fsql.Select<StringJoin01>().ToList(a => a.name));
            var val2 = fsql.Select<StringJoin01>().ToList(a => string.Join(",", fsql.Select<StringJoin01>().As("b").ToList(b => b.name)));
            Assert.Equal(val1, val2[0]);

            val1 = string.Join("**", fsql.Select<StringJoin01>().ToList(a => a.name));
            val2 = fsql.Select<StringJoin01>().ToList(a => string.Join("**", fsql.Select<StringJoin01>().As("b").ToList(b => b.name)));
            Assert.Equal(val1, val2[0]);

            val1 = string.Join(",", fsql.Select<StringJoin01>().ToList(a => a.id));
            val2 = fsql.Select<StringJoin01>().ToList(a => string.Join(",", fsql.Select<StringJoin01>().As("b").ToList(b => b.id)));
            Assert.Equal(val1, val2[0]);

            val1 = string.Join("**", fsql.Select<StringJoin01>().ToList(a => a.id));
            val2 = fsql.Select<StringJoin01>().ToList(a => string.Join("**", fsql.Select<StringJoin01>().As("b").ToList(b => b.id)));
            Assert.Equal(val1, val2[0]);
        }
        class StringJoin01
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string name { get; set; }
        }

        [Fact]
        public void First()
        {
            Assert.Equal('x', select.First(a => "x1".First()));
            Assert.Equal('z', select.First(a => "z1".First()));
        }
        [Fact]
        public void FirstOrDefault()
        {
            Assert.Equal('x', select.First(a => "x1".FirstOrDefault()));
            Assert.Equal('z', select.First(a => "z1".FirstOrDefault()));
        }

        [Fact]
        public void Format()
        {
            var item = g.sqlserver.GetRepository<Topic>().Insert(new Topic { Clicks = 101, Title = "我是中国人101", CreateTime = DateTime.Parse("2020-7-5") });
            var sql = select.WhereDynamic(item).ToSql(a => new
            {
                str = $"x{a.Id + 1}z-{a.CreateTime.ToString("yyyyMM")}{a.Title}",
                str2 = string.Format("{0}x{0}z-{1}{2}", a.Id + 1, a.CreateTime.ToString("yyyyMM"), a.Title)
            });
            Assert.Equal($@"SELECT N'x'+isnull(cast((a.[Id] + 1) as varchar), '')+N'z-'+isnull(substring(convert(char(8), cast(a.[CreateTime] as datetime), 112), 1, 6), '')+N''+isnull(a.[Title], '')+N'' as1, N''+isnull(cast((a.[Id] + 1) as varchar), '')+N'x'+isnull(cast((a.[Id] + 1) as varchar), '')+N'z-'+isnull(substring(convert(char(8), cast(a.[CreateTime] as datetime), 112), 1, 6), '')+N''+isnull(a.[Title], '')+N'' as2 
FROM [tb_topic] a 
WHERE (a.[Id] = {item.Id})", sql);

            var item2 = select.WhereDynamic(item).First(a => new
            {
                str = $"x{a.Id + 1}z-{a.CreateTime.ToString("yyyyMM")}{a.Title}",
                str2 = string.Format("{0}x{0}z-{1}{2}", a.Id + 1, a.CreateTime.ToString("yyyyMM"), a.Title)
            });
            Assert.NotNull(item2);
            Assert.Equal($"x{item.Id + 1}z-{item.CreateTime.ToString("yyyyMM")}{item.Title}", item2.str);
            Assert.Equal(string.Format("{0}x{0}z-{1}{2}", item.Id + 1, item.CreateTime.ToString("yyyyMM"), item.Title), item2.str2);
        }

        [Fact]
        public void Format4()
        {
            //3个 {} 时，Arguments 解析出来是分开的
            //4个 {} 时，Arguments[1] 只能解析这个出来，然后里面是 NewArray []
            var item = g.sqlserver.GetRepository<Topic>().Insert(new Topic { Clicks = 101, Title = "我是中国人101", CreateTime = DateTime.Parse("2020-7-5") });
            var sql = select.WhereDynamic(item).ToSql(a => new
            {
                str = $"x{a.Id + 1}z-{a.CreateTime.ToString("yyyyMM")}{a.Title}{a.Title}",
                str2 = string.Format("{0}x{0}z-{1}{2}{3}", a.Id + 1, a.CreateTime.ToString("yyyyMM"), a.Title, a.Title)
            });
            Assert.Equal($@"SELECT N'x'+isnull(cast((a.[Id] + 1) as varchar), '')+N'z-'+isnull(substring(convert(char(8), cast(a.[CreateTime] as datetime), 112), 1, 6), '')+N''+isnull(a.[Title], '')+N''+isnull(a.[Title], '')+N'' as1, N''+isnull(cast((a.[Id] + 1) as varchar), '')+N'x'+isnull(cast((a.[Id] + 1) as varchar), '')+N'z-'+isnull(substring(convert(char(8), cast(a.[CreateTime] as datetime), 112), 1, 6), '')+N''+isnull(a.[Title], '')+N''+isnull(a.[Title], '')+N'' as2 
FROM [tb_topic] a 
WHERE (a.[Id] = {item.Id})", sql);

            var item2 = select.WhereDynamic(item).First(a => new
            {
                str = $"x{a.Id + 1}z-{a.CreateTime.ToString("yyyyMM")}{a.Title}{a.Title}",
                str2 = string.Format("{0}x{0}z-{1}{2}{3}", a.Id + 1, a.CreateTime.ToString("yyyyMM"), a.Title, a.Title)
            });
            Assert.NotNull(item2);
            Assert.Equal($"x{item.Id + 1}z-{item.CreateTime.ToString("yyyyMM")}{item.Title}{item.Title}", item2.str);
            Assert.Equal(string.Format("{0}x{0}z-{1}{2}{3}", item.Id + 1, item.CreateTime.ToString("yyyyMM"), item.Title, item.Title), item2.str2);
        }

        [Fact]
        public void Empty()
        {
            var data = new List<object>();
            data.Add(select.Where(a => (a.Title ?? "") == string.Empty).ToSql());
        }

        [Fact]
        public void StartsWith()
        {
            var list = new List<object>();
            list.Add(select.Where(a => a.Title.StartsWith("aaa")).ToList());
            list.Add(select.Where(a => a.Title.StartsWith(a.Title)).ToList());
            list.Add(select.Where(a => a.Title.StartsWith(a.Title + 1)).ToList());
            list.Add(select.Where(a => a.Title.StartsWith(a.Type.Name)).ToList());

            list.Add(select.Where(a => (a.Title + "aaa").StartsWith("aaa")).ToList());
            list.Add(select.Where(a => (a.Title + "aaa").StartsWith(a.Title)).ToList());
            list.Add(select.Where(a => (a.Title + "aaa").StartsWith(a.Title + 1)).ToList());
            list.Add(select.Where(a => (a.Title + "aaa").StartsWith(a.Type.Name)).ToList());
        }
        [Fact]
        public void EndsWith()
        {
            var list = new List<object>();
            list.Add(select.Where(a => a.Title.EndsWith("aaa")).ToList());
            list.Add(select.Where(a => a.Title.EndsWith(a.Title)).ToList());
            list.Add(select.Where(a => a.Title.EndsWith(a.Title + 1)).ToList());
            list.Add(select.Where(a => a.Title.EndsWith(a.Type.Name)).ToList());

            list.Add(select.Where(a => (a.Title + "aaa").EndsWith("aaa")).ToList());
            list.Add(select.Where(a => (a.Title + "aaa").EndsWith(a.Title)).ToList());
            list.Add(select.Where(a => (a.Title + "aaa").EndsWith(a.Title + 1)).ToList());
            list.Add(select.Where(a => (a.Title + "aaa").EndsWith(a.Type.Name)).ToList());
        }
        [Fact]
        public void Contains()
        {
            var list = new List<object>();
            list.Add(select.Where(a => a.Title.Contains("aaa")).ToList());
            list.Add(select.Where(a => a.Title.Contains(a.Title)).ToList());
            list.Add(select.Where(a => a.Title.Contains(a.Title + 1)).ToList());
            list.Add(select.Where(a => a.Title.Contains(a.Type.Name)).ToList());

            list.Add(select.Where(a => (a.Title + "aaa").Contains("aaa")).ToList());
            list.Add(select.Where(a => (a.Title + "aaa").Contains(a.Title)).ToList());
            list.Add(select.Where(a => (a.Title + "aaa").Contains(a.Title + 1)).ToList());
            list.Add(select.Where(a => (a.Title + "aaa").Contains(a.Type.Name)).ToList());
        }
        [Fact]
        public void ToLower()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Title.ToLower() == "aaa").ToList());
            data.Add(select.Where(a => a.Title.ToLower() == a.Title).ToList());
            data.Add(select.Where(a => a.Title.ToLower() == (a.Title + 1)).ToList());
            data.Add(select.Where(a => a.Title.ToLower() == a.Type.Name).ToList());

            data.Add(select.Where(a => (a.Title.ToLower() + "aaa").ToLower() == "aaa").ToList());
            data.Add(select.Where(a => (a.Title.ToLower() + "aaa").ToLower() == a.Title).ToList());
            data.Add(select.Where(a => (a.Title.ToLower() + "aaa").ToLower() == (a.Title + 1)).ToList());
            data.Add(select.Where(a => (a.Title.ToLower() + "aaa").ToLower() == a.Type.Name).ToList());
        }
        [Fact]
        public void ToUpper()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Title.ToUpper() == "aaa").ToList());
            data.Add(select.Where(a => a.Title.ToUpper() == a.Title).ToList());
            data.Add(select.Where(a => a.Title.ToUpper() == (a.Title + 1)).ToList());
            data.Add(select.Where(a => a.Title.ToUpper() == a.Type.Name).ToList());

            data.Add(select.Where(a => (a.Title.ToUpper() + "aaa").ToUpper() == "aaa").ToList());
            data.Add(select.Where(a => (a.Title.ToUpper() + "aaa").ToUpper() == a.Title).ToList());
            data.Add(select.Where(a => (a.Title.ToUpper() + "aaa").ToUpper() == (a.Title + 1)).ToList());
            data.Add(select.Where(a => (a.Title.ToUpper() + "aaa").ToUpper() == a.Type.Name).ToList());
        }
        [Fact]
        public void Substring()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Title.Substring(0) == "aaa").ToList());
            data.Add(select.Where(a => a.Title.Substring(0) == a.Title).ToList());
            data.Add(select.Where(a => a.Title.Substring(0) == (a.Title + 1)).ToList());
            data.Add(select.Where(a => a.Title.Substring(0) == a.Type.Name).ToList());

            data.Add(select.Where(a => (a.Title.Substring(0) + "aaa").Substring(a.Title.Length) == "aaa").ToList());
            data.Add(select.Where(a => (a.Title.Substring(0) + "aaa").Substring(0, a.Title.Length) == a.Title).ToList());
            data.Add(select.Where(a => (a.Title.Substring(0) + "aaa").Substring(0, 3) == (a.Title + 1)).ToList());
            data.Add(select.Where(a => (a.Title.Substring(0) + "aaa").Substring(1, 2) == a.Type.Name).ToList());
        }
        [Fact]
        public void Length()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Title.Length == 0).ToList());
            data.Add(select.Where(a => a.Title.Length == 1).ToList());
            data.Add(select.Where(a => a.Title.Length == a.Title.Length + 1).ToList());
            data.Add(select.Where(a => a.Title.Length == a.Type.Name.Length).ToList());

            data.Add(select.Where(a => (a.Title + "aaa").Length == 0).ToList());
            data.Add(select.Where(a => (a.Title + "aaa").Length == 1).ToList());
            data.Add(select.Where(a => (a.Title + "aaa").Length == a.Title.Length + 1).ToList());
            data.Add(select.Where(a => (a.Title + "aaa").Length == a.Type.Name.Length).ToList());
        }
        [Fact]
        public void IndexOf()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Title.IndexOf("aaa") == -1).ToList());
            data.Add(select.Where(a => a.Title.IndexOf("aaa", 2) == -1).ToList());
            data.Add(select.Where(a => a.Title.IndexOf("aaa", 2) == (a.Title.Length + 1)).ToList());
            data.Add(select.Where(a => a.Title.IndexOf("aaa", 2) == a.Type.Name.Length + 1).ToList());

            data.Add(select.Where(a => (a.Title + "aaa").IndexOf("aaa") == -1).ToList());
            data.Add(select.Where(a => (a.Title + "aaa").IndexOf("aaa", 2) == -1).ToList());
            data.Add(select.Where(a => (a.Title + "aaa").IndexOf("aaa", 2) == (a.Title.Length + 1)).ToList());
            data.Add(select.Where(a => (a.Title + "aaa").IndexOf("aaa", 2) == a.Type.Name.Length + 1).ToList());
        }
        [Fact]
        public void PadLeft()
        {
            //var data = new List<object>();
            //data.Add(select.Where(a => a.Title.PadLeft(10, 'a') == "aaa").ToList());
            //data.Add(select.Where(a => a.Title.PadLeft(10, 'a') == a.Title).ToList());
            //data.Add(select.Where(a => a.Title.PadLeft(10, 'a') == (a.Title + 1)).ToList());
            //data.Add(select.Where(a => a.Title.PadLeft(10, 'a') == a.Type.Name).ToList());

            //data.Add(select.Where(a => (a.Title.PadLeft(10, 'a') + "aaa").PadLeft(20, 'b') == "aaa").ToList());
            //data.Add(select.Where(a => (a.Title.PadLeft(10, 'a') + "aaa").PadLeft(20, 'b') == a.Title).ToList());
            //data.Add(select.Where(a => (a.Title.PadLeft(10, 'a') + "aaa").PadLeft(20, 'b') == (a.Title + 1)).ToList());
            //data.Add(select.Where(a => (a.Title.PadLeft(10, 'a') + "aaa").PadLeft(20, 'b') == a.Type.Name).ToList());
        }
        [Fact]
        public void PadRight()
        {
            //var data = new List<object>();
            //data.Add(select.Where(a => a.Title.PadRight(10, 'a') == "aaa").ToList());
            //data.Add(select.Where(a => a.Title.PadRight(10, 'a') == a.Title).ToList());
            //data.Add(select.Where(a => a.Title.PadRight(10, 'a') == (a.Title + 1)).ToList());
            //data.Add(select.Where(a => a.Title.PadRight(10, 'a') == a.Type.Name).ToList());

            //data.Add(select.Where(a => (a.Title.PadRight(10, 'a') + "aaa").PadRight(20, 'b') == "aaa").ToList());
            //data.Add(select.Where(a => (a.Title.PadRight(10, 'a') + "aaa").PadRight(20, 'b') == a.Title).ToList());
            //data.Add(select.Where(a => (a.Title.PadRight(10, 'a') + "aaa").PadRight(20, 'b') == (a.Title + 1)).ToList());
            //data.Add(select.Where(a => (a.Title.PadRight(10, 'a') + "aaa").PadRight(20, 'b') == a.Type.Name).ToList());
        }
        [Fact]
        public void Trim()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Title.Trim() == "aaa").ToList());
            data.Add(select.Where(a => a.Title.Trim('a') == a.Title).ToList());
            data.Add(select.Where(a => a.Title.Trim('a', 'b') == (a.Title + 1)).ToList());
            data.Add(select.Where(a => a.Title.Trim('a', 'b', 'c') == a.Type.Name).ToList());

            data.Add(select.Where(a => (a.Title.Trim() + "aaa").Trim() == "aaa").ToList());
            data.Add(select.Where(a => (a.Title.Trim('a') + "aaa").Trim('a') == a.Title).ToList());
            data.Add(select.Where(a => (a.Title.Trim('a', 'b') + "aaa").Trim('a', 'b') == (a.Title + 1)).ToList());
            data.Add(select.Where(a => (a.Title.Trim('a', 'b', 'c') + "aaa").Trim('a', 'b', 'c') == a.Type.Name).ToList());
        }
        [Fact]
        public void TrimStart()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Title.TrimStart() == "aaa").ToList());
            data.Add(select.Where(a => a.Title.TrimStart('a') == a.Title).ToList());
            data.Add(select.Where(a => a.Title.TrimStart('a', 'b') == (a.Title + 1)).ToList());
            data.Add(select.Where(a => a.Title.TrimStart('a', 'b', 'c') == a.Type.Name).ToList());

            data.Add(select.Where(a => (a.Title.TrimStart() + "aaa").TrimStart() == "aaa").ToList());
            data.Add(select.Where(a => (a.Title.TrimStart('a') + "aaa").TrimStart('a') == a.Title).ToList());
            data.Add(select.Where(a => (a.Title.TrimStart('a', 'b') + "aaa").TrimStart('a', 'b') == (a.Title + 1)).ToList());
            data.Add(select.Where(a => (a.Title.TrimStart('a', 'b', 'c') + "aaa").TrimStart('a', 'b', 'c') == a.Type.Name).ToList());
        }
        [Fact]
        public void TrimEnd()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Title.TrimEnd() == "aaa").ToList());
            data.Add(select.Where(a => a.Title.TrimEnd('a') == a.Title).ToList());
            data.Add(select.Where(a => a.Title.TrimEnd('a', 'b') == (a.Title + 1)).ToList());
            data.Add(select.Where(a => a.Title.TrimEnd('a', 'b', 'c') == a.Type.Name).ToList());

            data.Add(select.Where(a => (a.Title.TrimEnd() + "aaa").TrimEnd() == "aaa").ToList());
            data.Add(select.Where(a => (a.Title.TrimEnd('a') + "aaa").TrimEnd('a') == a.Title).ToList());
            data.Add(select.Where(a => (a.Title.TrimEnd('a', 'b') + "aaa").TrimEnd('a', 'b') == (a.Title + 1)).ToList());
            data.Add(select.Where(a => (a.Title.TrimEnd('a', 'b', 'c') + "aaa").TrimEnd('a', 'b', 'c') == a.Type.Name).ToList());
        }
        [Fact]
        public void Replace()
        {
            var data = new List<object>();
            data.Add(select.Where(a => a.Title.Replace("a", "b") == "aaa").ToList());
            data.Add(select.Where(a => a.Title.Replace("a", "b").Replace("b", "c") == a.Title).ToList());
            data.Add(select.Where(a => a.Title.Replace("a", "b").Replace("b", "c").Replace("c", "a") == (a.Title + 1)).ToList());
            data.Add(select.Where(a => a.Title.Replace("a", "b").Replace("b", "c").Replace(a.Type.Name, "a") == a.Type.Name).ToList());

            data.Add(select.Where(a => (a.Title.Replace("a", "b") + "aaa").TrimEnd() == "aaa").ToList());
            data.Add(select.Where(a => (a.Title.Replace("a", "b").Replace("b", "c") + "aaa").TrimEnd('a') == a.Title).ToList());
            data.Add(select.Where(a => (a.Title.Replace("a", "b").Replace("b", "c").Replace("c", "a") + "aaa").TrimEnd('a', 'b') == (a.Title + 1)).ToList());
            data.Add(select.Where(a => (a.Title.Replace("a", "b").Replace("b", "c").Replace(a.Type.Name, "a") + "aaa").TrimEnd('a', 'b', 'c') == a.Type.Name).ToList());
        }
        [Fact]
        public void CompareTo()
        {
            //var data = new List<object>();
            //data.Add(select.Where(a => a.Title.CompareTo(a.Title) == 0).ToList());
            //data.Add(select.Where(a => a.Title.CompareTo(a.Title) > 0).ToList());
            //data.Add(select.Where(a => a.Title.CompareTo(a.Title + 1) == 0).ToList());
            //data.Add(select.Where(a => a.Title.CompareTo(a.Title + a.Type.Name) == 0).ToList());

            //data.Add(select.Where(a => (a.Title + "aaa").CompareTo("aaa") == 0).ToList());
            //data.Add(select.Where(a => (a.Title + "aaa").CompareTo(a.Title) > 0).ToList());
            //data.Add(select.Where(a => (a.Title + "aaa").CompareTo(a.Title + 1) == 0).ToList());
            //data.Add(select.Where(a => (a.Title + "aaa").CompareTo(a.Type.Name) == 0).ToList());
        }

        [Fact]
        public void string_IsNullOrEmpty()
        {
            var data = new List<object>();
            data.Add(select.Where(a => string.IsNullOrEmpty(a.Title)).ToList());
            //data.Add(select.Where(a => string.IsNullOrEmpty(a.Title) == false).ToList());
            data.Add(select.Where(a => !string.IsNullOrEmpty(a.Title)).ToList());
        }

        [Fact]
        public void string_IsNullOrWhiteSpace()
        {
            var data = new List<object>();
            data.Add(select.Where(a => string.IsNullOrWhiteSpace(a.Title)).ToList());
            data.Add(select.Where(a => string.IsNullOrWhiteSpace(a.Title) == false).ToList());
            data.Add(select.Where(a => !string.IsNullOrWhiteSpace(a.Title)).ToList());
        }
    }
}
