using FreeSql.DataAnnotations;
using FreeSql.KingbaseES;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.KingbaseES
{
    public class OnConflictDoUpdateTest
    {
        class TestOnConflictDoUpdateInfo
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string title { get; set; }
            public DateTime? time { get; set; }
        }

        [Fact]
        public void ExecuteAffrows()
        {
            g.kingbaseES.Delete<TestOnConflictDoUpdateInfo>(new[] { 100, 101, 102 }).ExecuteAffrows();
            var odku1 = new KingbaseESOnConflictDoUpdate<TestOnConflictDoUpdateInfo>(g.kingbaseES.Insert(new TestOnConflictDoUpdateInfo { id = 100, title = "title-100", time = DateTime.Parse("2000-01-01") }).NoneParameter().InsertIdentity());
            Assert.Equal(@"INSERT INTO ""TESTONCONFLICTDOUPDATEINFO""(""ID"", ""TITLE"", ""TIME"") VALUES(100, 'title-100', '2000-01-01 00:00:00.000000')
ON CONFLICT(""ID"") DO UPDATE SET
""TITLE"" = EXCLUDED.""TITLE"", 
""TIME"" = EXCLUDED.""TIME""", odku1.ToSql());
            Assert.Equal(1, odku1.ExecuteAffrows());

            var odku2 = new KingbaseESOnConflictDoUpdate<TestOnConflictDoUpdateInfo>(g.kingbaseES.Insert(new[] {
                new TestOnConflictDoUpdateInfo { id = 100, title = "title-100", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 101, title = "title-101", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 102, title = "title-102", time = DateTime.Parse("2000-01-01") }
            }).NoneParameter().InsertIdentity());
            Assert.Equal(@"INSERT INTO ""TESTONCONFLICTDOUPDATEINFO""(""ID"", ""TITLE"", ""TIME"") VALUES(100, 'title-100', '2000-01-01 00:00:00.000000'), (101, 'title-101', '2000-01-01 00:00:00.000000'), (102, 'title-102', '2000-01-01 00:00:00.000000')
ON CONFLICT(""ID"") DO UPDATE SET
""TITLE"" = EXCLUDED.""TITLE"", 
""TIME"" = EXCLUDED.""TIME""", odku2.ToSql());
            odku2.ExecuteAffrows();
        }

        [Fact]
        public void IgnoreColumns()
        {
            g.kingbaseES.Delete<TestOnConflictDoUpdateInfo>(new[] { 200, 201, 202 }).ExecuteAffrows();
            var odku1 = new KingbaseESOnConflictDoUpdate<TestOnConflictDoUpdateInfo>(g.kingbaseES.Insert(new TestOnConflictDoUpdateInfo { id = 200, title = "title-200", time = DateTime.Parse("2000-01-01") }).IgnoreColumns(a => a.time).NoneParameter().InsertIdentity());
            Assert.Equal(@"INSERT INTO ""TESTONCONFLICTDOUPDATEINFO""(""ID"", ""TITLE"") VALUES(200, 'title-200')
ON CONFLICT(""ID"") DO UPDATE SET
""TITLE"" = EXCLUDED.""TITLE"", 
""TIME"" = '2000-01-01 00:00:00.000000'", odku1.ToSql());
            Assert.Equal(1, odku1.ExecuteAffrows());

            var odku2 = new KingbaseESOnConflictDoUpdate<TestOnConflictDoUpdateInfo>(g.kingbaseES.Insert(new[] {
                new TestOnConflictDoUpdateInfo { id = 200, title = "title-200", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 201, title = "title-201", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 202, title = "title-202", time = DateTime.Parse("2000-01-01") }
            }).IgnoreColumns(a => a.time).NoneParameter().InsertIdentity());
            Assert.Equal(@"INSERT INTO ""TESTONCONFLICTDOUPDATEINFO""(""ID"", ""TITLE"") VALUES(200, 'title-200'), (201, 'title-201'), (202, 'title-202')
ON CONFLICT(""ID"") DO UPDATE SET
""TITLE"" = EXCLUDED.""TITLE"", 
""TIME"" = CASE EXCLUDED.""ID"" 
WHEN 200 THEN '2000-01-01 00:00:00.000000' 
WHEN 201 THEN '2000-01-01 00:00:00.000000' 
WHEN 202 THEN '2000-01-01 00:00:00.000000' END::timestamp", odku2.ToSql());
            odku2.ExecuteAffrows();


            g.kingbaseES.Delete<TestOnConflictDoUpdateInfo>(new[] { 200, 201, 202 }).ExecuteAffrows();
            odku1 = new KingbaseESOnConflictDoUpdate<TestOnConflictDoUpdateInfo>(g.kingbaseES.Insert(new TestOnConflictDoUpdateInfo { id = 200, title = "title-200", time = DateTime.Parse("2000-01-01") }).IgnoreColumns(a => a.time).NoneParameter().InsertIdentity()).IgnoreColumns(a => a.title);
            Assert.Equal(@"INSERT INTO ""TESTONCONFLICTDOUPDATEINFO""(""ID"", ""TITLE"") VALUES(200, 'title-200')
ON CONFLICT(""ID"") DO UPDATE SET
""TIME"" = '2000-01-01 00:00:00.000000'", odku1.ToSql());
            Assert.Equal(1, odku1.ExecuteAffrows());

            odku2 = new KingbaseESOnConflictDoUpdate<TestOnConflictDoUpdateInfo>(g.kingbaseES.Insert(new[] {
                new TestOnConflictDoUpdateInfo { id = 200, title = "title-200", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 201, title = "title-201", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 202, title = "title-202", time = DateTime.Parse("2000-01-01") }
            }).IgnoreColumns(a => a.time).NoneParameter().InsertIdentity()).IgnoreColumns(a => a.title);
            Assert.Equal(@"INSERT INTO ""TESTONCONFLICTDOUPDATEINFO""(""ID"", ""TITLE"") VALUES(200, 'title-200'), (201, 'title-201'), (202, 'title-202')
ON CONFLICT(""ID"") DO UPDATE SET
""TIME"" = CASE EXCLUDED.""ID"" 
WHEN 200 THEN '2000-01-01 00:00:00.000000' 
WHEN 201 THEN '2000-01-01 00:00:00.000000' 
WHEN 202 THEN '2000-01-01 00:00:00.000000' END::timestamp", odku2.ToSql());
            odku2.ExecuteAffrows();
        }

        [Fact]
        public void UpdateColumns()
        {
            g.kingbaseES.Delete<TestOnConflictDoUpdateInfo>(new[] { 300, 301, 302 }).ExecuteAffrows();
            var odku1 = new KingbaseESOnConflictDoUpdate<TestOnConflictDoUpdateInfo>(g.kingbaseES.Insert(new TestOnConflictDoUpdateInfo { id = 300, title = "title-300", time = DateTime.Parse("2000-01-01") }).InsertColumns(a => a.title).NoneParameter().InsertIdentity());
            Assert.Equal(@"INSERT INTO ""TESTONCONFLICTDOUPDATEINFO""(""ID"", ""TITLE"") VALUES(300, 'title-300')
ON CONFLICT(""ID"") DO UPDATE SET
""TITLE"" = EXCLUDED.""TITLE"", 
""TIME"" = '2000-01-01 00:00:00.000000'", odku1.ToSql());
            Assert.Equal(1, odku1.ExecuteAffrows());

            var odku2 = new KingbaseESOnConflictDoUpdate<TestOnConflictDoUpdateInfo>(g.kingbaseES.Insert(new[] {
                new TestOnConflictDoUpdateInfo { id = 300, title = "title-300", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 301, title = "title-301", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 302, title = "title-302", time = DateTime.Parse("2000-01-01") }
            }).InsertColumns(a => a.title).NoneParameter().InsertIdentity());
            Assert.Equal(@"INSERT INTO ""TESTONCONFLICTDOUPDATEINFO""(""ID"", ""TITLE"") VALUES(300, 'title-300'), (301, 'title-301'), (302, 'title-302')
ON CONFLICT(""ID"") DO UPDATE SET
""TITLE"" = EXCLUDED.""TITLE"", 
""TIME"" = CASE EXCLUDED.""ID"" 
WHEN 300 THEN '2000-01-01 00:00:00.000000' 
WHEN 301 THEN '2000-01-01 00:00:00.000000' 
WHEN 302 THEN '2000-01-01 00:00:00.000000' END::timestamp", odku2.ToSql());
            odku2.ExecuteAffrows();


            g.kingbaseES.Delete<TestOnConflictDoUpdateInfo>(new[] { 300, 301, 302 }).ExecuteAffrows();
            odku1 = new KingbaseESOnConflictDoUpdate<TestOnConflictDoUpdateInfo>(g.kingbaseES.Insert(new TestOnConflictDoUpdateInfo { id = 300, title = "title-300", time = DateTime.Parse("2000-01-01") }).InsertColumns(a => a.title).NoneParameter().InsertIdentity()).UpdateColumns(a => a.time);
            Assert.Equal(@"INSERT INTO ""TESTONCONFLICTDOUPDATEINFO""(""ID"", ""TITLE"") VALUES(300, 'title-300')
ON CONFLICT(""ID"") DO UPDATE SET
""TIME"" = '2000-01-01 00:00:00.000000'", odku1.ToSql());
            Assert.Equal(1, odku1.ExecuteAffrows());

            odku2 = new KingbaseESOnConflictDoUpdate<TestOnConflictDoUpdateInfo>(g.kingbaseES.Insert(new[] {
                new TestOnConflictDoUpdateInfo { id = 300, title = "title-300", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 301, title = "title-301", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 302, title = "title-302", time = DateTime.Parse("2000-01-01") }
            }).InsertColumns(a => a.title).NoneParameter().InsertIdentity()).UpdateColumns(a => a.time);
            Assert.Equal(@"INSERT INTO ""TESTONCONFLICTDOUPDATEINFO""(""ID"", ""TITLE"") VALUES(300, 'title-300'), (301, 'title-301'), (302, 'title-302')
ON CONFLICT(""ID"") DO UPDATE SET
""TIME"" = CASE EXCLUDED.""ID"" 
WHEN 300 THEN '2000-01-01 00:00:00.000000' 
WHEN 301 THEN '2000-01-01 00:00:00.000000' 
WHEN 302 THEN '2000-01-01 00:00:00.000000' END::timestamp", odku2.ToSql());
            odku2.ExecuteAffrows();
        }

        [Fact]
        public void Set()
        {
            g.kingbaseES.Delete<TestOnConflictDoUpdateInfo>(new[] { 400, 401, 402 }).ExecuteAffrows();
            var odku1 = new KingbaseESOnConflictDoUpdate<TestOnConflictDoUpdateInfo>(g.kingbaseES.Insert(new TestOnConflictDoUpdateInfo { id = 400, title = "title-400", time = DateTime.Parse("2000-01-01") }).NoneParameter().InsertIdentity()).Set(a => a.time, DateTime.Parse("2020-1-1"));
            Assert.Equal(@"INSERT INTO ""TESTONCONFLICTDOUPDATEINFO""(""ID"", ""TITLE"", ""TIME"") VALUES(400, 'title-400', '2000-01-01 00:00:00.000000')
ON CONFLICT(""ID"") DO UPDATE SET
""TIME"" = '2020-01-01 00:00:00.000000'", odku1.ToSql());
            Assert.Equal(1, odku1.ExecuteAffrows());

            var odku2 = new KingbaseESOnConflictDoUpdate<TestOnConflictDoUpdateInfo>(g.kingbaseES.Insert(new[] {
                new TestOnConflictDoUpdateInfo { id = 400, title = "title-400", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 401, title = "title-401", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 402, title = "title-402", time = DateTime.Parse("2000-01-01") }
            }).NoneParameter().InsertIdentity()).Set(a => a.time, DateTime.Parse("2020-1-1"));
            Assert.Equal(@"INSERT INTO ""TESTONCONFLICTDOUPDATEINFO""(""ID"", ""TITLE"", ""TIME"") VALUES(400, 'title-400', '2000-01-01 00:00:00.000000'), (401, 'title-401', '2000-01-01 00:00:00.000000'), (402, 'title-402', '2000-01-01 00:00:00.000000')
ON CONFLICT(""ID"") DO UPDATE SET
""TIME"" = '2020-01-01 00:00:00.000000'", odku2.ToSql());
            odku2.ExecuteAffrows();
        }
    }
}
