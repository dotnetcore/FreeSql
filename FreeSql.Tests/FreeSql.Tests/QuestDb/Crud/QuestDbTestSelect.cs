using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;
using FreeSql.Tests.QuestDb.QuestDbTestModel;
using NetTopologySuite.Operation.Valid;
using Xunit;
using static FreeSql.Tests.QuestDb.QuestDbTest;

namespace FreeSql.Tests.QuestDb.Crud
{
    public class QuestDbTestSelect
    {
        [Fact]
        public void TestNormal()
        {
            var sql = fsql.Select<QuestDb_Model_Test01>().ToSql();
            Debug.WriteLine(sql);
            Assert.Equal(
                @"SELECT a.""Primarys"", a.""Id"", a.""NameUpdate"", a.""NameInsert"", a.""Activos"", a.""CreateTime"", a.""UpdateTime"", a.""IsCompra"" 
FROM ""QuestDb_Model_Test01"" a", sql);

            var sqlWhere = fsql.Select<QuestDb_Model_Test01>().Where(q =>
                q.UpdateTime.Value.BetweenEnd(DateTime.Parse("2023-02-17 09:35:00"),
                    DateTime.Parse("2023-02-17 10:20:00"))).ToSql();
            Debug.WriteLine(sqlWhere);
            Assert.Equal(
                @"SELECT a.""Primarys"", a.""Id"", a.""NameUpdate"", a.""NameInsert"", a.""Activos"", a.""CreateTime"", a.""UpdateTime"", a.""IsCompra"" 
FROM ""QuestDb_Model_Test01"" a 
WHERE (a.""UpdateTime"" >= '2023-02-17 09:35:00.000000' and a.""UpdateTime"" < '2023-02-17 10:20:00.000000')",
                sqlWhere);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void TestPageAndCount(int page)
        {
            var pageSize = 5;
            var select = fsql.Select<QuestDb_Model_Test01>().Count(out var total).Page(page, pageSize);
            var sql = select.ToSql();
            Debug.WriteLine(sql);
            switch (page)
            {
                case 1:
                    Assert.Equal(
                        @"SELECT a.""Primarys"", a.""Id"", a.""NameUpdate"", a.""NameInsert"", a.""Activos"", a.""CreateTime"", a.""UpdateTime"", a.""IsCompra"" 
FROM ""QuestDb_Model_Test01"" a 
limit 5", sql);
                    break;
                case 2:
                    Assert.Equal(
                        @"SELECT a.""Primarys"", a.""Id"", a.""NameUpdate"", a.""NameInsert"", a.""Activos"", a.""CreateTime"", a.""UpdateTime"", a.""IsCompra"" 
FROM ""QuestDb_Model_Test01"" a 
limit 5,10", sql);
                    break;
                case 3:
                    Assert.Equal(
                        @"SELECT a.""Primarys"", a.""Id"", a.""NameUpdate"", a.""NameInsert"", a.""Activos"", a.""CreateTime"", a.""UpdateTime"", a.""IsCompra"" 
FROM ""QuestDb_Model_Test01"" a 
limit 10,15", sql);
                    break;
            }
        }

        [Fact]
        public void TestNavigation()
        {
            var select = fsql.Select<Topic>()
                .LeftJoin(a => a.Category.Id == a.CategoryId)
                .LeftJoin(a => a.Category.Parent.Id == a.Category.ParentId)
                .Where(a => a.Category.Parent.Id > 0);
            var sql = select.ToSql();
            select.ToList();
            Debug.WriteLine(sql);
            Assert.Equal(
                @"SELECT a.""Id"", a.""Title"", a.""Clicks"", a.""CreateTime"", a.""CategoryId"", a__Category.""Id"" as6, a__Category.""Name"", a__Category.""ParentId"" 
FROM ""Topic"" a 
LEFT JOIN ""Category"" a__Category ON a__Category.""Id"" = a.""CategoryId"" 
LEFT JOIN ""CategoryType"" a__Category__Parent ON a__Category__Parent.""Id"" = a__Category.""ParentId"" 
WHERE (a__Category__Parent.""Id"" > 0)", sql);
        }

        [Fact]
        public void TestComplexJoin()
        {
            var select = fsql.Select<Topic, Category, CategoryType>()
                .LeftJoin(w => w.t1.CategoryId == w.t2.Id)
                .LeftJoin(w => w.t2.ParentId == w.t3.Id)
                .Where(w => w.t3.Id > 0);
            var sql = select.ToSql(w => new { w.t1, w.t2, w.t3 });
            Debug.WriteLine(sql);
            select.ToList(w => new { w.t1, w.t2, w.t3 });
            Assert.Equal(
                @"SELECT a.""Id"" as1, a.""Title"" as2, a.""Clicks"" as3, a.""CreateTime"" as4, a.""CategoryId"" as5, b.""Id"" as6, b.""Name"" as7, b.""ParentId"" as8, c.""Id"" as9, c.""Name"" as10 
FROM ""Topic"" a 
LEFT JOIN ""Category"" b ON a.""CategoryId"" = b.""Id"" 
LEFT JOIN ""CategoryType"" c ON b.""ParentId"" = c.""Id"" 
WHERE (c.""Id"" > 0)", sql);
        }

        [Fact]
        public void TestUnionAll()
        {
            var select = fsql.Select<QuestDb_Model_Test01>().Where(a => a.IsCompra == true)
                .UnionAll(
                    fsql.Select<QuestDb_Model_Test01>().Where(a => a.IsCompra == true),
                    fsql.Select<QuestDb_Model_Test01>().Where(a => a.IsCompra == true)
                )
                .Where(a => a.IsCompra == true);
            var sql = select.ToSql();
            Debug.WriteLine(sql);
            select.ToList();
            Assert.Equal(
                @"SELECT a.""Primarys"", a.""Id"", a.""NameUpdate"", a.""NameInsert"", a.""Activos"", a.""CreateTime"", a.""UpdateTime"", a.""IsCompra"" 
FROM ( SELECT a.""Primarys"", a.""Id"", a.""NameUpdate"", a.""NameInsert"", a.""Activos"", a.""CreateTime"", a.""UpdateTime"", a.""IsCompra"" 
    FROM ""QuestDb_Model_Test01"" a 
    WHERE (a.""IsCompra"" = True) 
    UNION ALL 
    SELECT a.""Primarys"", a.""Id"", a.""NameUpdate"", a.""NameInsert"", a.""Activos"", a.""CreateTime"", a.""UpdateTime"", a.""IsCompra"" 
    FROM ""QuestDb_Model_Test01"" a 
    WHERE (a.""IsCompra"" = True) 
    UNION ALL 
    SELECT a.""Primarys"", a.""Id"", a.""NameUpdate"", a.""NameInsert"", a.""Activos"", a.""CreateTime"", a.""UpdateTime"", a.""IsCompra"" 
    FROM ""QuestDb_Model_Test01"" a 
    WHERE (a.""IsCompra"" = True) ) a 
WHERE (a.""IsCompra"" = True)", sql);
        }

        [Fact]
        public void TestSampleBy()
        {
            var selectSql = fsql.Select<QuestDb_Model_Test01>()
                .SampleBy(1, SampleUnits.d)
                .WithTempQuery(q => new { q.Id, q.Activos, count = SqlExt.Count(q.Id).ToValue() })
                .Where(q => q.Id != "1")
                .ToSql();
            Debug.WriteLine(selectSql);
            var sql = @"SELECT * 
FROM ( 
    SELECT a.""Id"", a.""Activos"", count(a.""Id"") ""count"" 
    FROM ""QuestDb_Model_Test01"" a
SAMPLE BY 1d
 ) a 
WHERE (a.""Id"" <> '1')";
            Assert.Equal(selectSql, sql);
        }

        [Fact]
        public void TestLatestOn()
        {
            var selectSql = fsql.Select<QuestDb_Model_Test01>()
                .LatestOn(q => q.CreateTime, q => new { q.Id, q.NameUpdate })
                .ToSql();
            Debug.WriteLine(selectSql);
            var sql =
                @"SELECT a.""Primarys"", a.""Id"", a.""NameUpdate"", a.""NameInsert"", a.""Activos"", a.""CreateTime"", a.""UpdateTime"", a.""IsCompra"" 
FROM ""QuestDb_Model_Test01"" a
LATEST ON CreateTime PARTITION BY Id,NameUpdate ";
            Assert.Equal(selectSql, sql);
        }

        [Fact]
        public void TestGroup()
        {
            //QUEDTDB的GroupBy PostgrSql有所不同
            var selectSql = fsql.Select<QuestDb_Model_Test01>()
                .WithTempQuery(q => new { q.Id, q.Activos, count = SqlExt.Count(q.Id).ToValue() })
                .Where(q => q.Id != "1" && q.count > 1)
                .ToSql();
            Debug.WriteLine(selectSql);
            var sql = @"SELECT * 
FROM ( 
    SELECT a.""Id"", a.""Activos"", count(a.""Id"") ""count"" 
    FROM ""QuestDb_Model_Test01"" a ) a 
WHERE (a.""Id"" <> '1' AND a.""count"" > 1)";
            Assert.Equal(selectSql, sql);
        }
    }
}