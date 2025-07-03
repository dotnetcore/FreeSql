using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace FreeSql.Tests.PostgreSQL
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

        [Table(Name = "demo_class1"), Index("uk_demo1", "name", true)]
        public class DemoClass1
        {
            [Column(Name = "id")]
            public int Id { get; set; }

            [Column(Name = "name", IsNullable = false)]
            public string Name { get; set; }

            [Column(Name = "desc")]
            public string Description { get; set; }

            #region 系统非业务基础字段
            //更新操作忽略此字段 只在OnConflictDoUpdate的插入操作时生效
            [Column(Name = "created_id", CanUpdate = false, InsertValueSql = "1")]
            public virtual int CreatedId { get; set; }

            //插入操作忽略此字段 只在OnConflictDoUpdate的更新操作时生效
            [Column(Name = "modified_id", CanInsert = false, InsertValueSql = "1")]
            [UpdateValueSql("1")]
            public virtual int? ModifiedId { get; set; }

            //更新操作忽略此字段 只在OnConflictDoUpdate的插入操作时生效
            [Column(Name = "created_time", CanUpdate = false, ServerTime = DateTimeKind.Local)]
            public virtual DateTime CreatedTime { get; set; }

            //插入操作忽略此字段 只在OnConflictDoUpdate的更新操作时生效
            [Column(Name = "modified_time", CanInsert = false, ServerTime = DateTimeKind.Local)]
            public virtual DateTime? ModifiedTime { get; set; }
            #endregion
        }
        [Table(Name = "demo_class2")]
        public class DemoClass2
        {
            [Column(Name = "name", IsNullable = false)]
            public string Name { get; set; }

            [Column(Name = "desc")]
            public string Description { get; set; }

            #region 系统非业务基础字段
            //更新操作忽略此字段 只在OnConflictDoUpdate的插入操作时生效
            [Column(Name = "created_id", CanUpdate = false, InsertValueSql = "1")]
            public virtual int CreatedId { get; set; }

            //插入操作忽略此字段 只在OnConflictDoUpdate的更新操作时生效
            [Column(Name = "modified_id", CanInsert = false, InsertValueSql = "1")]
            [UpdateValueSql("1")]
            public virtual int? ModifiedId { get; set; }

            //更新操作忽略此字段 只在OnConflictDoUpdate的插入操作时生效
            [Column(Name = "created_time", CanUpdate = false, ServerTime = DateTimeKind.Local)]
            public virtual DateTime CreatedTime { get; set; }

            //插入操作忽略此字段 只在OnConflictDoUpdate的更新操作时生效
            [Column(Name = "modified_time", CanInsert = false, ServerTime = DateTimeKind.Local)]
            public virtual DateTime? ModifiedTime { get; set; }
            #endregion
        }
        class UpdateValueSqlAttribute : Attribute
        {
            public string Value { get; set; }
            public UpdateValueSqlAttribute(string value) => Value = value;
        }
        [Fact]
        public void Issues1393()
        {
            var fsql = g.pgsql;
            //跟随 FreeSqlBuilder Build 之后初始化，批量设置实体类：
            foreach (var entity in new[] { typeof(DemoClass1) })
            {
                var table = fsql.CodeFirst.GetTableByEntity(entity);
                table.Properties.Values
                    .Select(a => new { Property = a, UpdateValueSql = a.GetCustomAttribute<UpdateValueSqlAttribute>()?.Value })
                    .Where(a => a.UpdateValueSql != null)
                    .ToList()
                    .ForEach(a =>
                    {
                        var col = table.ColumnsByCs[a.Property.Name];
                        col.GetType().GetProperty("DbUpdateValue").SetValue(col, a.UpdateValueSql);
                    });
            }

            var sql = fsql.Insert(Enumerable.Range(1, 5).Select(i => new DemoClass1 { Id = i, Name = $"Name{i}", Description = $"Description{i}" }))
               .NoneParameter()
               .OnConflictDoUpdate(a => new { a.Name })
               .ToSql();
            Assert.Equal(@"INSERT INTO ""demo_class1""(""id"", ""name"", ""desc"", ""created_id"", ""created_time"") VALUES(1, 'Name1', 'Description1', 1, current_timestamp), (2, 'Name2', 'Description2', 1, current_timestamp), (3, 'Name3', 'Description3', 1, current_timestamp), (4, 'Name4', 'Description4', 1, current_timestamp), (5, 'Name5', 'Description5', 1, current_timestamp)
ON CONFLICT(""name"") DO UPDATE SET
""name"" = EXCLUDED.""name"", 
""desc"" = EXCLUDED.""desc"", 
""modified_id"" = 1, 
""modified_time"" = current_timestamp", sql);


            sql = fsql.Insert(Enumerable.Range(1, 5).Select(i => new DemoClass2 { Name = $"Name{i}", Description = $"Description{i}", ModifiedId = 1 }))
               .NoneParameter()
               .OnConflictDoUpdate(a => new { a.Name })
               .ToSql();
            Assert.Equal(@"INSERT INTO ""demo_class2""(""name"", ""desc"", ""created_id"", ""created_time"") VALUES('Name1', 'Description1', 1, current_timestamp), ('Name2', 'Description2', 1, current_timestamp), ('Name3', 'Description3', 1, current_timestamp), ('Name4', 'Description4', 1, current_timestamp), ('Name5', 'Description5', 1, current_timestamp)
ON CONFLICT(""name"") DO UPDATE SET
""name"" = EXCLUDED.""name"", 
""desc"" = EXCLUDED.""desc"", 
""modified_id"" = 1, 
""modified_time"" = current_timestamp", sql);

            //sql = g.pgsql.Insert(data)
            //   .NoneParameter()
            //   .OnConflictDoUpdate(a => new { a.Name })
            //   .UpdateColumns()
            //   .ToSql();
        }


        [Fact]
        public void ExecuteAffrows()
        {
            g.pgsql.Delete<TestOnConflictDoUpdateInfo>(new[] { 100, 101, 102 }).ExecuteAffrows();
            var odku1 = g.pgsql.Insert(new TestOnConflictDoUpdateInfo { id = 100, title = "title-100", time = DateTime.Parse("2000-01-01") }).NoneParameter().OnConflictDoUpdate();
            Assert.Equal(odku1.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"", ""time"") VALUES(100, 'title-100', '2000-01-01 00:00:00.000000')
ON CONFLICT(""id"") DO UPDATE SET
""title"" = EXCLUDED.""title"", 
""time"" = EXCLUDED.""time""");
            Assert.Equal(1, odku1.ExecuteAffrows());

            var odku2 = g.pgsql.Insert(new[] {
                new TestOnConflictDoUpdateInfo { id = 100, title = "title-100", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 101, title = "title-101", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 102, title = "title-102", time = DateTime.Parse("2000-01-01") }
            }).NoneParameter().OnConflictDoUpdate();
            Assert.Equal(odku2.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"", ""time"") VALUES(100, 'title-100', '2000-01-01 00:00:00.000000'), (101, 'title-101', '2000-01-01 00:00:00.000000'), (102, 'title-102', '2000-01-01 00:00:00.000000')
ON CONFLICT(""id"") DO UPDATE SET
""title"" = EXCLUDED.""title"", 
""time"" = EXCLUDED.""time""");
            odku2.ExecuteAffrows();
        }

        [Fact]
        public void IgnoreColumns()
        {
            g.pgsql.Delete<TestOnConflictDoUpdateInfo>(new[] { 200, 201, 202 }).ExecuteAffrows();
            var odku1 = g.pgsql.Insert(new TestOnConflictDoUpdateInfo { id = 200, title = "title-200", time = DateTime.Parse("2000-01-01") }).IgnoreColumns(a => a.time).NoneParameter().OnConflictDoUpdate();
            Assert.Equal(odku1.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"") VALUES(200, 'title-200')
ON CONFLICT(""id"") DO UPDATE SET
""title"" = EXCLUDED.""title"", 
""time"" = '2000-01-01 00:00:00.000000'");
            Assert.Equal(1, odku1.ExecuteAffrows());

            var odku2 = g.pgsql.Insert(new[] {
                new TestOnConflictDoUpdateInfo { id = 200, title = "title-200", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 201, title = "title-201", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 202, title = "title-202", time = DateTime.Parse("2000-01-01") }
            }).IgnoreColumns(a => a.time).NoneParameter().OnConflictDoUpdate();
            Assert.Equal(odku2.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"") VALUES(200, 'title-200'), (201, 'title-201'), (202, 'title-202')
ON CONFLICT(""id"") DO UPDATE SET
""title"" = EXCLUDED.""title"", 
""time"" = '2000-01-01 00:00:00.000000'");
            odku2.ExecuteAffrows();


            g.pgsql.Delete<TestOnConflictDoUpdateInfo>(new[] { 200, 201, 202 }).ExecuteAffrows();
            odku1 = g.pgsql.Insert(new TestOnConflictDoUpdateInfo { id = 200, title = "title-200", time = DateTime.Parse("2000-01-01") }).IgnoreColumns(a => a.time).NoneParameter().OnConflictDoUpdate().IgnoreColumns(a => a.title);
            Assert.Equal(odku1.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"") VALUES(200, 'title-200')
ON CONFLICT(""id"") DO UPDATE SET
""time"" = '2000-01-01 00:00:00.000000'");
            Assert.Equal(1, odku1.ExecuteAffrows());

            odku2 = g.pgsql.Insert(new[] {
                new TestOnConflictDoUpdateInfo { id = 200, title = "title-200", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 201, title = "title-201", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 202, title = "title-202", time = DateTime.Parse("2000-01-01") }
            }).IgnoreColumns(a => a.time).NoneParameter().OnConflictDoUpdate().IgnoreColumns(a => a.title);
            Assert.Equal(odku2.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"") VALUES(200, 'title-200'), (201, 'title-201'), (202, 'title-202')
ON CONFLICT(""id"") DO UPDATE SET
""time"" = '2000-01-01 00:00:00.000000'");
            odku2.ExecuteAffrows();
        }

        [Fact]
        public void UpdateColumns()
        {
            g.pgsql.Delete<TestOnConflictDoUpdateInfo>(new[] { 300, 301, 302 }).ExecuteAffrows();
            var odku1 = g.pgsql.Insert(new TestOnConflictDoUpdateInfo { id = 300, title = "title-300", time = DateTime.Parse("2000-01-01") }).InsertColumns(a => a.title).NoneParameter().OnConflictDoUpdate();
            Assert.Equal(odku1.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"") VALUES(300, 'title-300')
ON CONFLICT(""id"") DO UPDATE SET
""title"" = EXCLUDED.""title"", 
""time"" = '2000-01-01 00:00:00.000000'");
            Assert.Equal(1, odku1.ExecuteAffrows());

            var odku2 = g.pgsql.Insert(new[] {
                new TestOnConflictDoUpdateInfo { id = 300, title = "title-300", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 301, title = "title-301", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 302, title = "title-302", time = DateTime.Parse("2000-01-01") }
            }).InsertColumns(a => a.title).NoneParameter().OnConflictDoUpdate();
            Assert.Equal(odku2.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"") VALUES(300, 'title-300'), (301, 'title-301'), (302, 'title-302')
ON CONFLICT(""id"") DO UPDATE SET
""title"" = EXCLUDED.""title"", 
""time"" = '2000-01-01 00:00:00.000000'");
            odku2.ExecuteAffrows();


            g.pgsql.Delete<TestOnConflictDoUpdateInfo>(new[] { 300, 301, 302 }).ExecuteAffrows();
            odku1 = g.pgsql.Insert(new TestOnConflictDoUpdateInfo { id = 300, title = "title-300", time = DateTime.Parse("2000-01-01") }).InsertColumns(a => a.title).NoneParameter().OnConflictDoUpdate().UpdateColumns(a => a.time);
            Assert.Equal(odku1.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"") VALUES(300, 'title-300')
ON CONFLICT(""id"") DO UPDATE SET
""time"" = '2000-01-01 00:00:00.000000'");
            Assert.Equal(1, odku1.ExecuteAffrows());

            odku2 = g.pgsql.Insert(new[] {
                new TestOnConflictDoUpdateInfo { id = 300, title = "title-300", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 301, title = "title-301", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 302, title = "title-302", time = DateTime.Parse("2000-01-01") }
            }).InsertColumns(a => a.title).NoneParameter().OnConflictDoUpdate().UpdateColumns(a => a.time);
            Assert.Equal(odku2.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"") VALUES(300, 'title-300'), (301, 'title-301'), (302, 'title-302')
ON CONFLICT(""id"") DO UPDATE SET
""time"" = '2000-01-01 00:00:00.000000'");
            odku2.ExecuteAffrows();
        }

        [Fact]
        public void Set()
        {
            g.pgsql.Delete<TestOnConflictDoUpdateInfo>(new[] { 400, 401, 402 }).ExecuteAffrows();
            var odku1 = g.pgsql.Insert(new TestOnConflictDoUpdateInfo { id = 400, title = "title-400", time = DateTime.Parse("2000-01-01") }).NoneParameter().OnConflictDoUpdate().Set(a => a.time, DateTime.Parse("2020-1-1"));
            Assert.Equal(odku1.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"", ""time"") VALUES(400, 'title-400', '2000-01-01 00:00:00.000000')
ON CONFLICT(""id"") DO UPDATE SET
""time"" = '2020-01-01 00:00:00.000000'");
            Assert.Equal(1, odku1.ExecuteAffrows());

            var odku2 = g.pgsql.Insert(new[] {
                new TestOnConflictDoUpdateInfo { id = 400, title = "title-400", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 401, title = "title-401", time = DateTime.Parse("2000-01-01") },
                new TestOnConflictDoUpdateInfo { id = 402, title = "title-402", time = DateTime.Parse("2000-01-01") }
            }).NoneParameter().OnConflictDoUpdate().Set(a => a.time, DateTime.Parse("2020-1-1"));
            Assert.Equal(odku2.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"", ""time"") VALUES(400, 'title-400', '2000-01-01 00:00:00.000000'), (401, 'title-401', '2000-01-01 00:00:00.000000'), (402, 'title-402', '2000-01-01 00:00:00.000000')
ON CONFLICT(""id"") DO UPDATE SET
""time"" = '2020-01-01 00:00:00.000000'");
            odku2.ExecuteAffrows();


//            var dt2020 = DateTime.Parse("2020-1-1");
//            g.pgsql.Delete<TestOnConflictDoUpdateInfo>(new[] { 400, 401, 402 }).ExecuteAffrows();
//            odku1 = g.pgsql.Insert(new TestOnConflictDoUpdateInfo { id = 400, title = "title-400", time = DateTime.Parse("2000-01-01") }).NoneParameter().OnConflictDoUpdate().Set(a => a.time == dt2020);
//            Assert.Equal(odku1.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"", ""time"") VALUES(400, 'title-400', '2000-01-01 00:00:00.000000')
//ON CONFLICT(""id"") DO UPDATE SET
//""time"" = '2020-01-01 00:00:00.000000'");
//            Assert.Equal(1, odku1.ExecuteAffrows());

//            odku2 = g.pgsql.Insert(new[] {
//                new TestOnConflictDoUpdateInfo { id = 400, title = "title-400", time = DateTime.Parse("2000-01-01") },
//                new TestOnConflictDoUpdateInfo { id = 401, title = "title-401", time = DateTime.Parse("2000-01-01") },
//                new TestOnConflictDoUpdateInfo { id = 402, title = "title-402", time = DateTime.Parse("2000-01-01") }
//            }).NoneParameter().OnConflictDoUpdate().Set(a => a.time == dt2020);
//            Assert.Equal(odku2.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"", ""time"") VALUES(400, 'title-400', '2000-01-01 00:00:00.000000'), (401, 'title-401', '2000-01-01 00:00:00.000000'), (402, 'title-402', '2000-01-01 00:00:00.000000')
//ON CONFLICT(""id"") DO UPDATE SET
//""time"" = '2020-01-01 00:00:00.000000'");
//            odku2.ExecuteAffrows();


//            g.pgsql.Delete<TestOnConflictDoUpdateInfo>(new[] { 400, 401, 402 }).ExecuteAffrows();
//            odku1 = g.pgsql.Insert(new TestOnConflictDoUpdateInfo { id = 400, title = "title-400", time = DateTime.Parse("2000-01-01") }).NoneParameter().OnConflictDoUpdate().Set(a => new { time = dt2020, title = a.title + "123" });
//            Assert.Equal(odku1.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"", ""time"") VALUES(400, 'title-400', '2000-01-01 00:00:00.000000')
//ON CONFLICT(""id"") DO UPDATE SET
//""time"" = '2020-01-01 00:00:00.000000', ""title"" = ""testonconflictdoupdateinfo"".""title"" || '123'");
//            Assert.Equal(1, odku1.ExecuteAffrows());

//            odku2 = g.pgsql.Insert(new[] {
//                new TestOnConflictDoUpdateInfo { id = 400, title = "title-400", time = DateTime.Parse("2000-01-01") },
//                new TestOnConflictDoUpdateInfo { id = 401, title = "title-401", time = DateTime.Parse("2000-01-01") },
//                new TestOnConflictDoUpdateInfo { id = 402, title = "title-402", time = DateTime.Parse("2000-01-01") }
//            }).NoneParameter().OnConflictDoUpdate().Set(a => new { time = dt2020, title = a.title + "123" });
//            Assert.Equal(odku2.ToSql(), @"INSERT INTO ""testonconflictdoupdateinfo""(""id"", ""title"", ""time"") VALUES(400, 'title-400', '2000-01-01 00:00:00.000000'), (401, 'title-401', '2000-01-01 00:00:00.000000'), (402, 'title-402', '2000-01-01 00:00:00.000000')
//ON CONFLICT(""id"") DO UPDATE SET
//""time"" = '2020-01-01 00:00:00.000000', ""title"" = ""testonconflictdoupdateinfo"".""title"" || '123'");
//            odku2.ExecuteAffrows();
        }

        [Fact]
        public void SetRaw()
        {

        }

    }
}
