using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Xunit;

namespace FreeSql.Tests.Internal
{

    public class GlobalFilterTest
    {
        [Fact]
        public void TestGlobalFilterTest()
        {
            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.SqlServer, "test")
                .UseAdoConnectionPool(true)
                .UseNoneCommandParameter(true)
                .Build())
            {
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("softdelete", a => a.IsDeleted == true);
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("tenrant", a => a.TenrantId == 100, before: true);

                var sql = fsql.Select<TestGFilter1>().Where(a => a.id > 10).ToSql();
                Assert.Equal(@"SELECT a.[id], a.[TenrantId], a.[IsDeleted] 
FROM [TestGFilter1] a 
WHERE (a.[TenrantId] = 100) AND (a.[id] > 10) AND (a.[IsDeleted] = 1)", sql);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "test")
                .UseAdoConnectionPool(true)
                .UseNoneCommandParameter(true)
                .Build())
            {
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("softdelete", a => a.IsDeleted == true);
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("tenrant", a => a.TenrantId == 100, before: true);

                var sql = fsql.Select<TestGFilter1>().Where(a => a.id > 10).ToSql();
                Assert.Equal(@"SELECT a.""id"", a.""TenrantId"", a.""IsDeleted"" 
FROM ""TestGFilter1"" a 
WHERE (a.""TenrantId"" = 100) AND (a.""id"" > 10) AND (a.""IsDeleted"" = 1)", sql);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.ShenTong, "test")
                .UseAdoConnectionPool(true)
                .UseNoneCommandParameter(true)
                .Build())
            {
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("softdelete", a => a.IsDeleted == true);
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("tenrant", a => a.TenrantId == 100, before: true);

                var sql = fsql.Select<TestGFilter1>().Where(a => a.id > 10).ToSql();
                Assert.Equal(@"SELECT a.""id"", a.""TenrantId"", a.""IsDeleted"" 
FROM ""TestGFilter1"" a 
WHERE (a.""TenrantId"" = 100) AND (a.""id"" > 10) AND (a.""IsDeleted"" = 't')", sql);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.QuestDb, "test")
                .UseAdoConnectionPool(true)
                .UseNoneCommandParameter(true)
                .Build())
            {
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("softdelete", a => a.IsDeleted == true);
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("tenrant", a => a.TenrantId == 100, before: true);

                var sql = fsql.Select<TestGFilter1>().Where(a => a.id > 10).ToSql();
                Assert.Equal(@"SELECT a.""id"", a.""TenrantId"", a.""IsDeleted"" 
FROM ""TestGFilter1"" a 
WHERE (a.""TenrantId"" = 100) AND (a.""id"" > 10) AND (a.""IsDeleted"" = true)", sql);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.PostgreSQL, "test")
                .UseAdoConnectionPool(true)
                .UseNoneCommandParameter(true)
                .Build())
            {
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("softdelete", a => a.IsDeleted == true);
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("tenrant", a => a.TenrantId == 100, before: true);

                var sql = fsql.Select<TestGFilter1>().Where(a => a.id > 10).ToSql();
                Assert.Equal(@"SELECT a.""id"", a.""TenrantId"", a.""IsDeleted"" 
FROM ""TestGFilter1"" a 
WHERE (a.""TenrantId"" = 100) AND (a.""id"" > 10) AND (a.""IsDeleted"" = 't')", sql);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Oracle, "test")
                .UseAdoConnectionPool(true)
                .UseNoneCommandParameter(true)
                .Build())
            {
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("softdelete", a => a.IsDeleted == true);
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("tenrant", a => a.TenrantId == 100, before: true);

                var sql = fsql.Select<TestGFilter1>().Where(a => a.id > 10).ToSql();
                Assert.Equal(@"SELECT a.""id"", a.""TenrantId"", a.""IsDeleted"" 
FROM ""TestGFilter1"" a 
WHERE (a.""TenrantId"" = 100) AND (a.""id"" > 10) AND (a.""IsDeleted"" = 1)", sql);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.MySql, "test")
                .UseAdoConnectionPool(true)
                .UseNoneCommandParameter(true)
                .Build())
            {
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("softdelete", a => a.IsDeleted == true);
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("tenrant", a => a.TenrantId == 100, before: true);

                var sql = fsql.Select<TestGFilter1>().Where(a => a.id > 10).ToSql();
                Assert.Equal(@"SELECT a.`id`, a.`TenrantId`, a.`IsDeleted` 
FROM `TestGFilter1` a 
WHERE (a.`TenrantId` = 100) AND (a.`id` > 10) AND (a.`IsDeleted` = 1)", sql);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.MsAccess, "test")
                .UseAdoConnectionPool(true)
                .UseNoneCommandParameter(true)
                .Build())
            {
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("softdelete", a => a.IsDeleted == true);
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("tenrant", a => a.TenrantId == 100, before: true);

                var sql = fsql.Select<TestGFilter1>().Where(a => a.id > 10).ToSql();
                Assert.Equal(@"SELECT a.[id], a.[TenrantId], a.[IsDeleted] 
FROM [TestGFilter1] a 
WHERE (a.[TenrantId] = 100) AND (a.[id] > 10) AND (a.[IsDeleted] = -1)", sql);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.KingbaseES, "test")
                .UseAdoConnectionPool(true)
                .UseNoneCommandParameter(true)
                .Build())
            {
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("softdelete", a => a.IsDeleted == true);
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("tenrant", a => a.TenrantId == 100, before: true);

                var sql = fsql.Select<TestGFilter1>().Where(a => a.id > 10).ToSql();
                Assert.Equal(@"SELECT a.""id"", a.""TenrantId"", a.""IsDeleted"" 
FROM ""TestGFilter1"" a 
WHERE (a.""TenrantId"" = 100) AND (a.""id"" > 10) AND (a.""IsDeleted"" = 't')", sql);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Dameng, "test")
                .UseAdoConnectionPool(true)
                .UseNoneCommandParameter(true)
                .Build())
            {
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("softdelete", a => a.IsDeleted == true);
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("tenrant", a => a.TenrantId == 100, before: true);

                var sql = fsql.Select<TestGFilter1>().Where(a => a.id > 10).ToSql();
                Assert.Equal(@"SELECT a.""id"", a.""TenrantId"", a.""IsDeleted"" 
FROM ""TestGFilter1"" a 
WHERE (a.""TenrantId"" = 100) AND (a.""id"" > 10) AND (a.""IsDeleted"" = 1)", sql);
            }

            using (var fsql = new FreeSqlBuilder()
                .UseConnectionString( DataType.ClickHouse, "test")
                .UseAdoConnectionPool(true)
                .UseNoneCommandParameter(true)
                .Build())
            {
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("softdelete", a => a.IsDeleted == true);
                fsql.GlobalFilter.ApplyOnly<TestGFilter1>("tenrant", a => a.TenrantId == 100, before: true);

                var sql = fsql.Select<TestGFilter1>().Where(a => a.id > 10).ToSql();
                Assert.Equal(@"SELECT a.`id`, a.`TenrantId`, a.`IsDeleted` 
FROM `TestGFilter1` a 
WHERE (a.`TenrantId` = 100) AND (a.`id` > 10) AND (a.`IsDeleted` = 1)", sql);
            }
        }

        class TestGFilter1
        {
            public int id { get; set; }
            public int TenrantId { get; set; }
            public bool IsDeleted { get; set; }
        }
    }
}
