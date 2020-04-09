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

namespace FreeSql.Tests
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
    }

}
