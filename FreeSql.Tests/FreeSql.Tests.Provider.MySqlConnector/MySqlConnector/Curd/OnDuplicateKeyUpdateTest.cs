using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.MySqlConnector
{
    public class OnDuplicateKeyUpdateTest
    {
        class TestOnDuplicateKeyUpdateInfo
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string title { get; set; }
            public DateTime time { get; set; }
        }

        [Fact]
        public void ExecuteAffrows()
        {
            g.mysql.Delete<TestOnDuplicateKeyUpdateInfo>(new[] { 100, 101, 102 }).ExecuteAffrows();
            var odku1 = g.mysql.Insert(new TestOnDuplicateKeyUpdateInfo { id = 100, title = "title-100", time = DateTime.Parse("2000-01-01") }).NoneParameter().OnDuplicateKeyUpdate();
            Assert.Equal(odku1.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`, `time`) VALUES(100, 'title-100', '2000-01-01 00:00:00.000')
ON DUPLICATE KEY UPDATE
`title` = VALUES(`title`), 
`time` = VALUES(`time`)");
            Assert.Equal(1, odku1.ExecuteAffrows());

            var odku2 = g.mysql.Insert(new[] {
                new TestOnDuplicateKeyUpdateInfo { id = 100, title = "title-100", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 101, title = "title-101", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 102, title = "title-102", time = DateTime.Parse("2000-01-01") }
            }).NoneParameter().OnDuplicateKeyUpdate();
            Assert.Equal(odku2.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`, `time`) VALUES(100, 'title-100', '2000-01-01 00:00:00.000'), (101, 'title-101', '2000-01-01 00:00:00.000'), (102, 'title-102', '2000-01-01 00:00:00.000')
ON DUPLICATE KEY UPDATE
`title` = VALUES(`title`), 
`time` = VALUES(`time`)");
            odku2.ExecuteAffrows();
        }

        [Fact]
        public void IgnoreColumns()
        {
            g.mysql.Delete<TestOnDuplicateKeyUpdateInfo>(new[] { 200, 201, 202 }).ExecuteAffrows();
            var odku1 = g.mysql.Insert(new TestOnDuplicateKeyUpdateInfo { id = 200, title = "title-200", time = DateTime.Parse("2000-01-01") }).IgnoreColumns(a => a.time).NoneParameter().OnDuplicateKeyUpdate();
            Assert.Equal(odku1.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`) VALUES(200, 'title-200')
ON DUPLICATE KEY UPDATE
`title` = VALUES(`title`), 
`time` = '2000-01-01 00:00:00.000'");
            Assert.Equal(1, odku1.ExecuteAffrows());

            var odku2 = g.mysql.Insert(new[] {
                new TestOnDuplicateKeyUpdateInfo { id = 200, title = "title-200", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 201, title = "title-201", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 202, title = "title-202", time = DateTime.Parse("2000-01-01") }
            }).IgnoreColumns(a => a.time).NoneParameter().OnDuplicateKeyUpdate();
            Assert.Equal(odku2.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`) VALUES(200, 'title-200'), (201, 'title-201'), (202, 'title-202')
ON DUPLICATE KEY UPDATE
`title` = VALUES(`title`), 
`time` = CASE `id` 
WHEN 200 THEN '2000-01-01 00:00:00.000' 
WHEN 201 THEN '2000-01-01 00:00:00.000' 
WHEN 202 THEN '2000-01-01 00:00:00.000' END");
            odku2.ExecuteAffrows();


            g.mysql.Delete<TestOnDuplicateKeyUpdateInfo>(new[] { 200, 201, 202 }).ExecuteAffrows();
            odku1 = g.mysql.Insert(new TestOnDuplicateKeyUpdateInfo { id = 200, title = "title-200", time = DateTime.Parse("2000-01-01") }).IgnoreColumns(a => a.time).NoneParameter().OnDuplicateKeyUpdate().IgnoreColumns(a => a.title);
            Assert.Equal(odku1.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`) VALUES(200, 'title-200')
ON DUPLICATE KEY UPDATE
`time` = '2000-01-01 00:00:00.000'");
            Assert.Equal(1, odku1.ExecuteAffrows());

            odku2 = g.mysql.Insert(new[] {
                new TestOnDuplicateKeyUpdateInfo { id = 200, title = "title-200", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 201, title = "title-201", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 202, title = "title-202", time = DateTime.Parse("2000-01-01") }
            }).IgnoreColumns(a => a.time).NoneParameter().OnDuplicateKeyUpdate().IgnoreColumns(a => a.title);
            Assert.Equal(odku2.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`) VALUES(200, 'title-200'), (201, 'title-201'), (202, 'title-202')
ON DUPLICATE KEY UPDATE
`time` = CASE `id` 
WHEN 200 THEN '2000-01-01 00:00:00.000' 
WHEN 201 THEN '2000-01-01 00:00:00.000' 
WHEN 202 THEN '2000-01-01 00:00:00.000' END");
            odku2.ExecuteAffrows();
        }

        [Fact]
        public void UpdateColumns()
        {
            g.mysql.Delete<TestOnDuplicateKeyUpdateInfo>(new[] { 300, 301, 302 }).ExecuteAffrows();
            var odku1 = g.mysql.Insert(new TestOnDuplicateKeyUpdateInfo { id = 300, title = "title-300", time = DateTime.Parse("2000-01-01") }).InsertColumns(a => a.title).NoneParameter().OnDuplicateKeyUpdate();
            Assert.Equal(odku1.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`) VALUES(300, 'title-300')
ON DUPLICATE KEY UPDATE
`title` = VALUES(`title`), 
`time` = '2000-01-01 00:00:00.000'");
            Assert.Equal(1, odku1.ExecuteAffrows());

            var odku2 = g.mysql.Insert(new[] {
                new TestOnDuplicateKeyUpdateInfo { id = 300, title = "title-300", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 301, title = "title-301", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 302, title = "title-302", time = DateTime.Parse("2000-01-01") }
            }).InsertColumns(a => a.title).NoneParameter().OnDuplicateKeyUpdate();
            Assert.Equal(odku2.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`) VALUES(300, 'title-300'), (301, 'title-301'), (302, 'title-302')
ON DUPLICATE KEY UPDATE
`title` = VALUES(`title`), 
`time` = CASE `id` 
WHEN 300 THEN '2000-01-01 00:00:00.000' 
WHEN 301 THEN '2000-01-01 00:00:00.000' 
WHEN 302 THEN '2000-01-01 00:00:00.000' END");
            odku2.ExecuteAffrows();


            g.mysql.Delete<TestOnDuplicateKeyUpdateInfo>(new[] { 300, 301, 302 }).ExecuteAffrows();
            odku1 = g.mysql.Insert(new TestOnDuplicateKeyUpdateInfo { id = 300, title = "title-300", time = DateTime.Parse("2000-01-01") }).InsertColumns(a => a.title).NoneParameter().OnDuplicateKeyUpdate().UpdateColumns(a => a.time);
            Assert.Equal(odku1.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`) VALUES(300, 'title-300')
ON DUPLICATE KEY UPDATE
`time` = '2000-01-01 00:00:00.000'");
            Assert.Equal(1, odku1.ExecuteAffrows());

            odku2 = g.mysql.Insert(new[] {
                new TestOnDuplicateKeyUpdateInfo { id = 300, title = "title-300", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 301, title = "title-301", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 302, title = "title-302", time = DateTime.Parse("2000-01-01") }
            }).InsertColumns(a => a.title).NoneParameter().OnDuplicateKeyUpdate().UpdateColumns(a => a.time);
            Assert.Equal(odku2.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`) VALUES(300, 'title-300'), (301, 'title-301'), (302, 'title-302')
ON DUPLICATE KEY UPDATE
`time` = CASE `id` 
WHEN 300 THEN '2000-01-01 00:00:00.000' 
WHEN 301 THEN '2000-01-01 00:00:00.000' 
WHEN 302 THEN '2000-01-01 00:00:00.000' END");
            odku2.ExecuteAffrows();
        }

        [Fact]
        public void Set()
        {
            g.mysql.Delete<TestOnDuplicateKeyUpdateInfo>(new[] { 400, 401, 402 }).ExecuteAffrows();
            var odku1 = g.mysql.Insert(new TestOnDuplicateKeyUpdateInfo { id = 400, title = "title-400", time = DateTime.Parse("2000-01-01") }).NoneParameter().OnDuplicateKeyUpdate().Set(a => a.time, DateTime.Parse("2020-1-1"));
            Assert.Equal(odku1.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`, `time`) VALUES(400, 'title-400', '2000-01-01 00:00:00.000')
ON DUPLICATE KEY UPDATE
`time` = '2020-01-01 00:00:00.000'");
            Assert.Equal(1, odku1.ExecuteAffrows());

            var odku2 = g.mysql.Insert(new[] {
                new TestOnDuplicateKeyUpdateInfo { id = 400, title = "title-400", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 401, title = "title-401", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 402, title = "title-402", time = DateTime.Parse("2000-01-01") }
            }).NoneParameter().OnDuplicateKeyUpdate().Set(a => a.time, DateTime.Parse("2020-1-1"));
            Assert.Equal(odku2.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`, `time`) VALUES(400, 'title-400', '2000-01-01 00:00:00.000'), (401, 'title-401', '2000-01-01 00:00:00.000'), (402, 'title-402', '2000-01-01 00:00:00.000')
ON DUPLICATE KEY UPDATE
`time` = '2020-01-01 00:00:00.000'");
            odku2.ExecuteAffrows();


            var dt2020 = DateTime.Parse("2020-1-1");
            g.mysql.Delete<TestOnDuplicateKeyUpdateInfo>(new[] { 400, 401, 402 }).ExecuteAffrows();
            odku1 = g.mysql.Insert(new TestOnDuplicateKeyUpdateInfo { id = 400, title = "title-400", time = DateTime.Parse("2000-01-01") }).NoneParameter().OnDuplicateKeyUpdate().Set(a => a.time == dt2020);
            Assert.Equal(odku1.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`, `time`) VALUES(400, 'title-400', '2000-01-01 00:00:00.000')
ON DUPLICATE KEY UPDATE
`time` = '2020-01-01 00:00:00.000'");
            Assert.Equal(1, odku1.ExecuteAffrows());

            odku2 = g.mysql.Insert(new[] {
                new TestOnDuplicateKeyUpdateInfo { id = 400, title = "title-400", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 401, title = "title-401", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 402, title = "title-402", time = DateTime.Parse("2000-01-01") }
            }).NoneParameter().OnDuplicateKeyUpdate().Set(a => a.time == dt2020);
            Assert.Equal(odku2.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`, `time`) VALUES(400, 'title-400', '2000-01-01 00:00:00.000'), (401, 'title-401', '2000-01-01 00:00:00.000'), (402, 'title-402', '2000-01-01 00:00:00.000')
ON DUPLICATE KEY UPDATE
`time` = '2020-01-01 00:00:00.000'");
            odku2.ExecuteAffrows();


            g.mysql.Delete<TestOnDuplicateKeyUpdateInfo>(new[] { 400, 401, 402 }).ExecuteAffrows();
            odku1 = g.mysql.Insert(new TestOnDuplicateKeyUpdateInfo { id = 400, title = "title-400", time = DateTime.Parse("2000-01-01") }).NoneParameter().OnDuplicateKeyUpdate().Set(a => new { time = dt2020, title = a.title + "123" });
            Assert.Equal(odku1.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`, `time`) VALUES(400, 'title-400', '2000-01-01 00:00:00.000')
ON DUPLICATE KEY UPDATE
`time` = '2020-01-01 00:00:00.000', `title` = concat(`title`, '123')");
            Assert.Equal(1, odku1.ExecuteAffrows());

            odku2 = g.mysql.Insert(new[] {
                new TestOnDuplicateKeyUpdateInfo { id = 400, title = "title-400", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 401, title = "title-401", time = DateTime.Parse("2000-01-01") },
                new TestOnDuplicateKeyUpdateInfo { id = 402, title = "title-402", time = DateTime.Parse("2000-01-01") }
            }).NoneParameter().OnDuplicateKeyUpdate().Set(a => new { time = dt2020, title = a.title + "123" });
            Assert.Equal(odku2.ToSql(), @"INSERT INTO `TestOnDuplicateKeyUpdateInfo`(`id`, `title`, `time`) VALUES(400, 'title-400', '2000-01-01 00:00:00.000'), (401, 'title-401', '2000-01-01 00:00:00.000'), (402, 'title-402', '2000-01-01 00:00:00.000')
ON DUPLICATE KEY UPDATE
`time` = '2020-01-01 00:00:00.000', `title` = concat(`title`, '123')");
            odku2.ExecuteAffrows();
        }

        [Fact]
        public void SetRaw()
        {

        }

    }
}
