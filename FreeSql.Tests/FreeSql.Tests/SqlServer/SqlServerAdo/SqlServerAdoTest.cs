using FreeSql.DataAnnotations;
using FreeSql.Tests.DataContext.SqlServer;
using Microsoft.Data.SqlClient;
using NetTaste;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Xunit;

namespace FreeSql.Tests.SqlServer
{
    [Collection("SqlServerCollection")]
    public class SqlServerAdoTest
    {
        SqlServerFixture _sqlserverFixture;

        public SqlServerAdoTest(SqlServerFixture sqlserverFixture)
        {
            _sqlserverFixture = sqlserverFixture;
        }

        [Fact]
        public void Pool()
        {
            var t1 = g.sqlserver.Ado.MasterPool.StatisticsFullily;
        }

        [Fact]
        public void SlavePools()
        {
            var t2 = g.sqlserver.Ado.SlavePools.Count;
        }

        [Fact]
        public void ExecuteTest()
        {
            Assert.True(g.sqlserver.Ado.ExecuteConnectTest());
        }
        [Fact]
        public void ExecuteReader()
        {

        }
        [Fact]
        public void ExecuteArray()
        {
            
        }
        [Fact]
        public void ExecuteNonQuery()
        {
            var ps = new[]
            {
                new SqlParameter("@TableName", "tb1"),
                new SqlParameter("@FInterID", SqlDbType.Int)
            };
            ps[1].Direction = System.Data.ParameterDirection.Output;
            g.sqlserver.Ado.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.GetICMaxNum", ps);
            Assert.Equal(100, ps[1].Value);
        }

        [Fact]
        public void ComandFluent()
        {
            var fsql = g.sqlserver;

            DbParameter p2 = null;
            fsql.Ado.CommandFluent("dbo.GetICMaxNum")
                .CommandType(CommandType.StoredProcedure)
                .WithParameter("TableName", "tb1")
                .WithParameter("FInterID", null, p =>
                {
                    p2 = p;
                    p.DbType = DbType.Int32;
                    p.Direction = ParameterDirection.Output;
                })
                .ExecuteNonQuery();
            Assert.Equal(100, p2.Value);

            DbParameter p3 = null;
            fsql.Ado.CommandFluent("dbo.GetICMaxNum", new Dictionary<string, object>
                {
                    ["TableName"] = "tb1"
                    // 更多参数
                })
                .WithParameter("FInterID", null, p =>
                {
                    p3 = p;
                    p.DbType = DbType.Int32;
                    p.Direction = ParameterDirection.Output;
                })
                .CommandType(CommandType.StoredProcedure)
                .ExecuteNonQuery();
            Assert.Equal(100, p3.Value);
        }

        [Fact]
        public void ExecuteScalar()
        {

        }

        [Fact]
        public void Query()
        {

            //var tt1 = g.sqlserver.Select<xxx>()
            //    .LeftJoin(a => a.ParentId == a.Parent.Id)
            //    .ToSql(a => new { a.Id, a.Title });

            //var tt2result = g.sqlserver.Select<xxx>()
            //    .LeftJoin(a => a.ParentId == a.Parent.Id)
            //    .ToList(a => new { a.Id, a.Title });

            //var tt = g.sqlserver.Select<xxx>()
            //    .LeftJoin<xxx>((a, b) => b.Id == a.Id)
            //    .ToSql(a => new { a.Id, a.Title });

            //var ttresult = g.sqlserver.Select<xxx>()
            //    .LeftJoin<xxx>((a, b) => b.Id == a.Id)
            //    .ToList(a => new { a.Id, a.Title });

            var tnsql1 = g.sqlserver.Select<xxx>().Where(a => a.Id > 0).Where(b => b.Title != null).Page(1, 3).ToSql(a => a.Id);
            var tnsql2 = g.sqlserver.Select<xxx>().Where(a => a.Id > 0).Where(b => b.Title != null).Page(2, 3).ToSql(a => a.Id);

            var tn1 = g.sqlserver.Select<xxx>().Where(a => a.Id > 0).Where(b => b.Title != null).Page(1, 3).ToList(a => a.Id);
            var tn2 = g.sqlserver.Select<xxx>().Where(a => a.Id > 0).Where(b => b.Title != null).Page(2, 3).ToList(a => a.Id);

            var t3 = g.sqlserver.Ado.Query<xxx>("select * from xxx");

            var t4 = g.sqlserver.Ado.Query<(int, int, string, string DateTime)>("select * from xxx");

            var t5 = g.sqlserver.Ado.Query<dynamic>("select * from xxx where Id = @id",
                new Dictionary<string, object> { ["id"] = 1 });

            var t6 = g.sqlserver.Ado.Query<xxx>("select * from xxx where Id in @ids", new { ids = new[] { 1, 2, 3 } });

            var t7 = g.sqlserver.Ado.Query<xxx>("select * from xxx where Title in @titles", new { titles = new[] { "title1", "title2", "title2" } });

        }

        [Fact]
        public void QueryMultipline()
        {
            var tnsql1 = g.sqlserver.Select<xxx>().Where(a => a.Id > 0).Where(b => b.Title != null).Page(1, 3).ToSql(a => a.Id);

            var t3 = g.sqlserver.Ado.Query<xxx, (int, string, string), dynamic>("select * from xxx; select * from xxx; select * from xxx");
        }

        class xxx
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public xxx Parent { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }
            public DateTime Create_time { get; set; }
        }
    }
}
