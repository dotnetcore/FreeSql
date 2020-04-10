using FreeSql.DataAnnotations;
using FreeSql;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using Npgsql.LegacyPostgis;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Data.SqlClient;
using kwlib;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FreeSql.Tests.Linq
{
    public class QueryableTest
    {
        class qt01
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string name { get; set; }

            [Navigate(nameof(qt01_item.qt01id))]
            public List<qt01_item> items { get; set; }
        }
        class qt01_item
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string title { get; set; }
            public int qt01id { get; set; }
        }
        IFreeSql fsql => g.sqlite;

        [Fact]
        public void Any()
        {
            var sd = new[] {
                new qt01{
                    name = "any01",
                    items = new List<qt01_item>(new []{
                        new qt01_item { title = "any01_item01" },
                        new qt01_item { title = "any01_item02" }
                    })
                },
                new qt01{
                    name = "any02",
                    items = new List<qt01_item>(new []{
                        new qt01_item { title = "any02_item01" },
                        new qt01_item { title = "any02_item02" }
                    })
                }
            };
            var repo = fsql.GetRepository<qt01>();
            repo.DbContextOptions.EnableAddOrUpdateNavigateList = true;
            repo.Insert(sd);

            Assert.True(fsql.Select<qt01>().AsQueryable().Any());
            Assert.True(fsql.Select<qt01>().AsQueryable().Any(a => a.id == sd[0].id));
            Assert.False(fsql.Select<qt01>().AsQueryable().Any(a => a.id == sd[0].id && sd[0].id == 0));
        }

        [Fact]
        public void Max()
        {
            var avg = fsql.Select<qt01>().AsQueryable().Max(a => a.id);
            Assert.True(avg > 0);
        }
        [Fact]
        public void Min()
        {
            var avg = fsql.Select<qt01>().AsQueryable().Min(a => a.id);
            Assert.True(avg > 0);
        }
        [Fact]
        public void Sum()
        {
            var avg = fsql.Select<qt01>().AsQueryable().Sum(a => a.id);
            Assert.True(avg > 0);
        }
        [Fact]
        public void Average()
        {
            var avg = fsql.Select<qt01>().AsQueryable().Average(a => a.id);
            Assert.True(avg > 0);
        }

        [Fact]
        public void Contains()
        {
            Assert.True(fsql.Select<qt01>().AsQueryable().Contains(new qt01 { id = 1 }));
            Assert.False(fsql.Select<qt01>().AsQueryable().Contains(new qt01 { id = 0 }));
        }

        [Fact]
        public void Distinct()
        {
            fsql.Select<qt01>().AsQueryable().Distinct().Select(a => a.name).ToList();
        }

        [Fact]
        public void ElementAt()
        {
            Assert.Equal(fsql.Select<qt01>().Skip(1).First().id, fsql.Select<qt01>().AsQueryable().ElementAt(1).id);
            Assert.Equal(fsql.Select<qt01>().Skip(2).First().id, fsql.Select<qt01>().AsQueryable().ElementAt(2).id);
            Assert.Equal(fsql.Select<qt01>().Skip(1).First().id, fsql.Select<qt01>().AsQueryable().ElementAtOrDefault(1).id);
            Assert.Equal(fsql.Select<qt01>().Skip(2).First().id, fsql.Select<qt01>().AsQueryable().ElementAtOrDefault(2).id);
        }

        [Fact]
        public void First()
        {
            Assert.Equal(fsql.Select<qt01>().First().id, fsql.Select<qt01>().AsQueryable().First().id);
            Assert.Equal(fsql.Select<qt01>().First().id, fsql.Select<qt01>().AsQueryable().FirstOrDefault().id);
        }

        [Fact]
        public void Single()
        {
            Assert.Equal(fsql.Select<qt01>().First().id, fsql.Select<qt01>().AsQueryable().Single().id);
            Assert.Equal(fsql.Select<qt01>().First().id, fsql.Select<qt01>().AsQueryable().SingleOrDefault().id);
        }

        [Fact]
        public void OrderBy()
        {
            Assert.Equal(fsql.Select<qt01>().OrderBy(a => a.id).First().id, fsql.Select<qt01>().AsQueryable().OrderBy(a => a.id).Single().id);
            Assert.Equal(fsql.Select<qt01>().OrderBy(a => a.id).First().id, fsql.Select<qt01>().AsQueryable().OrderBy(a => a.id).SingleOrDefault().id);
        }
        [Fact]
        public void OrderByDescending()
        {
            Assert.Equal(fsql.Select<qt01>().OrderByDescending(a => a.id).First().id, fsql.Select<qt01>().AsQueryable().OrderByDescending(a => a.id).Single().id);
            Assert.Equal(fsql.Select<qt01>().OrderByDescending(a => a.id).First().id, fsql.Select<qt01>().AsQueryable().OrderByDescending(a => a.id).SingleOrDefault().id);
        }
        [Fact]
        public void ThenBy()
        {
            Assert.Equal(fsql.Select<qt01>().OrderBy(a => a.id).OrderBy(a => a.id).First().id, fsql.Select<qt01>().AsQueryable().OrderBy(a => a.id).OrderBy(a => a.id).Single().id);
            Assert.Equal(fsql.Select<qt01>().OrderBy(a => a.id).OrderBy(a => a.id).First().id, fsql.Select<qt01>().AsQueryable().OrderBy(a => a.id).OrderBy(a => a.id).SingleOrDefault().id);

            Assert.Equal(fsql.Select<qt01>().OrderBy(a => a.id).OrderBy(a => a.id).First().id, fsql.Select<qt01>().AsQueryable().OrderBy(a => a.id).ThenBy(a => a.id).Single().id);
            Assert.Equal(fsql.Select<qt01>().OrderBy(a => a.id).OrderBy(a => a.id).First().id, fsql.Select<qt01>().AsQueryable().OrderBy(a => a.id).ThenBy(a => a.id).SingleOrDefault().id);
        }
        [Fact]
        public void ThenByDescending()
        {
            Assert.Equal(fsql.Select<qt01>().OrderBy(a => a.id).OrderByDescending(a => a.id).First().id, fsql.Select<qt01>().AsQueryable().OrderBy(a => a.id).OrderByDescending(a => a.id).Single().id);
            Assert.Equal(fsql.Select<qt01>().OrderBy(a => a.id).OrderByDescending(a => a.id).First().id, fsql.Select<qt01>().AsQueryable().OrderBy(a => a.id).OrderByDescending(a => a.id).SingleOrDefault().id);

            Assert.Equal(fsql.Select<qt01>().OrderBy(a => a.id).OrderByDescending(a => a.id).First().id, fsql.Select<qt01>().AsQueryable().OrderBy(a => a.id).ThenByDescending(a => a.id).Single().id);
            Assert.Equal(fsql.Select<qt01>().OrderBy(a => a.id).OrderByDescending(a => a.id).First().id, fsql.Select<qt01>().AsQueryable().OrderBy(a => a.id).ThenByDescending(a => a.id).SingleOrDefault().id);
        }

        [Fact]
        public void Select()
        {
            Assert.Equal(fsql.Select<qt01>().First(a => a.name), fsql.Select<qt01>().AsQueryable().Select(a => a.name).Single());
            Assert.Equal(fsql.Select<qt01>().First(a => new { a.name }).name, fsql.Select<qt01>().AsQueryable().Select(a => new { a.name }).Single().name);
        }

        [Fact]
        public void Where()
        {
            Assert.Equal(fsql.Select<qt01>().First(a => a.name), fsql.Select<qt01>().AsQueryable().Select(a => a.name).Single());
            Assert.Equal(fsql.Select<qt01>().First(a => new { a.name }).name, fsql.Select<qt01>().AsQueryable().Select(a => new { a.name }).Single().name);
        }

        [Fact]
        public void Skip()
        {
            Assert.Equal(fsql.Select<qt01>().Skip(2).First(a => a.name), fsql.Select<qt01>().AsQueryable().Skip(2).Select(a => a.name).Single());
            Assert.Equal(fsql.Select<qt01>().Skip(2).First(a => new { a.name }).name, fsql.Select<qt01>().AsQueryable().Skip(2).Select(a => new { a.name }).Single().name);
        }

        [Fact]
        public void Take()
        {
            Assert.Equal(fsql.Select<qt01>().Skip(2).First(a => a.name), fsql.Select<qt01>().AsQueryable().Skip(2).Take(1).Select(a => a.name).ToList().FirstOrDefault());
            Assert.Equal(fsql.Select<qt01>().Skip(2).First(a => new { a.name }).name, fsql.Select<qt01>().AsQueryable().Skip(2).Take(1).Select(a => new { a.name }).ToList().FirstOrDefault().name);
        }
    }

}
