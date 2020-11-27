using FreeSql.DataAnnotations;
using System;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Linq
{


    class TestQueryableLinqToSql
    {
        public Guid id { get; set; }

        public string name { get; set; }

        public int click { get; set; } = 10;

        public DateTime createtime { get; set; } = DateTime.Now;
    }
    class TestQueryableLinqToSqlComment
    {
        public Guid id { get; set; }

        public Guid TestLinqToSqlId { get; set; }
        public TestQueryableLinqToSql TEstLinqToSql { get; set; }

        public string text { get; set; }

        public DateTime createtime { get; set; } = DateTime.Now;
    }

    public class QueryableLinqToSqlTests
    {

        [Fact]
        public void Where()
        {
            var item = new TestQueryableLinqToSql { name = Guid.NewGuid().ToString() };
            g.sqlite.Insert<TestQueryableLinqToSql>().AppendData(item).ExecuteAffrows();

            var t1 = (from a in g.sqlite.Select<TestQueryableLinqToSql>().AsQueryable()
                      where a.id == item.id
                      select a).ToList();
            Assert.True(t1.Any());
            Assert.Equal(item.id, t1[0].id);
        }

        [Fact]
        public void Select()
        {
            var item = new TestQueryableLinqToSql { name = Guid.NewGuid().ToString() };
            g.sqlite.Insert<TestQueryableLinqToSql>().AppendData(item).ExecuteAffrows();

            var t1 = (from a in g.sqlite.Select<TestQueryableLinqToSql>().AsQueryable()
                      where a.id == item.id
                      select new { a.id }).ToList();
            Assert.True(t1.Any());
            Assert.Equal(item.id, t1[0].id);
        }

        [Fact]
        public void CaseWhen()
        {
            var item = new TestQueryableLinqToSql { name = Guid.NewGuid().ToString() };
            g.sqlite.Insert<TestQueryableLinqToSql>().AppendData(item).ExecuteAffrows();

            var t1 = (from a in g.sqlite.Select<TestQueryableLinqToSql>().AsQueryable()
                      where a.id == item.id
                      select new
                      {
                          a.id,
                          a.name,
                          testsub = new
                          {
                              time = a.click > 10 ? "大于" : "小于或等于"
                          }
                      }).ToList();
            Assert.True(t1.Any());
            Assert.Equal(item.id, t1[0].id);
            Assert.Equal("小于或等于", t1[0].testsub.time);
        }

        [Fact]
        public void Join()
        {
            var item = new TestQueryableLinqToSql { name = Guid.NewGuid().ToString() };
            g.sqlite.Insert<TestQueryableLinqToSql>().AppendData(item).ExecuteAffrows();
            var comment = new TestQueryableLinqToSqlComment { TestLinqToSqlId = item.id, text = Guid.NewGuid().ToString() };
            g.sqlite.Insert<TestQueryableLinqToSqlComment>().AppendData(comment).ExecuteAffrows();

            var t1 = (from a in g.sqlite.Select<TestQueryableLinqToSql>().AsQueryable()
                      join b in g.sqlite.Select<TestQueryableLinqToSqlComment>().AsQueryable() on a.id equals b.TestLinqToSqlId
                      select a).ToList();
            Assert.True(t1.Any());
            //Assert.Equal(item.id, t1[0].id);

            var t2 = (from a in g.sqlite.Select<TestQueryableLinqToSql>().AsQueryable()
                      join b in g.sqlite.Select<TestQueryableLinqToSqlComment>().AsQueryable() on a.id equals b.TestLinqToSqlId
                      select new { a.id, bid = b.id }).ToList();
            Assert.True(t2.Any());
            //Assert.Equal(item.id, t2[0].id);
            //Assert.Equal(comment.id, t2[0].bid);

            var t3 = (from a in g.sqlite.Select<TestQueryableLinqToSql>().AsQueryable()
                      join b in g.sqlite.Select<TestQueryableLinqToSqlComment>().AsQueryable() on a.id equals b.TestLinqToSqlId
                      where a.id == item.id
                      select new { a.id, bid = b.id }).ToList();
            Assert.True(t3.Any());
            Assert.Equal(item.id, t3[0].id);
            Assert.Equal(comment.id, t3[0].bid);
        }

        [Fact]
        public void LeftJoin()
        {
            var item = new TestQueryableLinqToSql { name = Guid.NewGuid().ToString() };
            g.sqlite.Insert<TestQueryableLinqToSql>().AppendData(item).ExecuteAffrows();
            var comment = new TestQueryableLinqToSqlComment { TestLinqToSqlId = item.id, text = Guid.NewGuid().ToString() };
            g.sqlite.Insert<TestQueryableLinqToSqlComment>().AppendData(comment).ExecuteAffrows();

            var t1 = (from a in g.sqlite.Select<TestQueryableLinqToSql>().AsQueryable()
                      join b in g.sqlite.Select<TestQueryableLinqToSqlComment>().AsQueryable() on a.id equals b.TestLinqToSqlId into temp
                      from tc in temp.DefaultIfEmpty()
                      select a).ToList();
            Assert.True(t1.Any());
            //Assert.Equal(item.id, t1[0].id);

            var t2 = (from a in g.sqlite.Select<TestQueryableLinqToSql>().AsQueryable()
                      join b in g.sqlite.Select<TestQueryableLinqToSqlComment>().AsQueryable() on a.id equals b.TestLinqToSqlId into temp
                      from tc in temp.DefaultIfEmpty()
                      select new { a.id, bid = tc.id }).ToList();
            Assert.True(t2.Any());
            //Assert.Equal(item.id, t2[0].id);
            //Assert.Equal(comment.id, t2[0].bid);

            var t3 = (from a in g.sqlite.Select<TestQueryableLinqToSql>().AsQueryable()
                      join b in g.sqlite.Select<TestQueryableLinqToSqlComment>().AsQueryable() on a.id equals b.TestLinqToSqlId into temp
                      from tc in temp.DefaultIfEmpty()
                      where a.id == item.id
                      select new { a.id, bid = tc.id }).ToList();
            Assert.True(t3.Any());
            Assert.Equal(item.id, t3[0].id);
            Assert.Equal(comment.id, t3[0].bid);
        }

        [Fact]
        public void From()
        {
            var item = new TestQueryableLinqToSql { name = Guid.NewGuid().ToString() };
            g.sqlite.Insert<TestQueryableLinqToSql>().AppendData(item).ExecuteAffrows();
            var comment = new TestQueryableLinqToSqlComment { TestLinqToSqlId = item.id, text = Guid.NewGuid().ToString() };
            g.sqlite.Insert<TestQueryableLinqToSqlComment>().AppendData(comment).ExecuteAffrows();

            var t1 = (from a in g.sqlite.Select<TestQueryableLinqToSql>().AsQueryable()
                      from b in g.sqlite.Select<TestQueryableLinqToSqlComment>().AsQueryable()
                      where a.id == b.TestLinqToSqlId
                      select a).ToList();
            Assert.True(t1.Any());
            //Assert.Equal(item.id, t1[0].id);

            var t2 = (from a in g.sqlite.Select<TestQueryableLinqToSql>().AsQueryable()
                      from b in g.sqlite.Select<TestQueryableLinqToSqlComment>().AsQueryable()
                      where a.id == b.TestLinqToSqlId
                      select new { a.id, bid = b.id }).ToList();
            Assert.True(t2.Any());
            //Assert.Equal(item.id, t2[0].id);
            //Assert.Equal(comment.id, t2[0].bid);

            var t3 = (from a in g.sqlite.Select<TestQueryableLinqToSql>().AsQueryable()
                      from b in g.sqlite.Select<TestQueryableLinqToSqlComment>().AsQueryable()
                      where a.id == b.TestLinqToSqlId
                      where a.id == item.id
                      select new { a.id, bid = b.id }).ToList();
            Assert.True(t3.Any());
            Assert.Equal(item.id, t3[0].id);
            Assert.Equal(comment.id, t3[0].bid);
        }
    }
}
