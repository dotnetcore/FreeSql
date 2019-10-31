using FreeSql.DataAnnotations;
using FreeSql.Tests.DataContext.SqlServer;
using System;
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
            var t1 = _sqlserverFixture.SqlServer.Ado.MasterPool.StatisticsFullily;
        }

        [Fact]
        public void SlavePools()
        {
            var t2 = _sqlserverFixture.SqlServer.Ado.SlavePools.Count;
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

        }
        [Fact]
        public void ExecuteScalar()
        {

        }

        [Fact]
        public void Query()
        {

            //var tt1 = _sqlserverFixture.SqlServer.Select<xxx>()
            //	.LeftJoin(a => a.ParentId == a.Parent.Id)
            //	.ToSql(a => new { a.Id, a.Title });

            //var tt2result = _sqlserverFixture.SqlServer.Select<xxx>()
            //	.LeftJoin(a => a.ParentId == a.Parent.Id)
            //	.ToList(a => new { a.Id, a.Title });

            //var tt = _sqlserverFixture.SqlServer.Select<xxx>()
            //	.LeftJoin<xxx>((a, b) => b.Id == a.Id)
            //	.ToSql(a => new { a.Id, a.Title });

            //var ttresult = _sqlserverFixture.SqlServer.Select<xxx>()
            //	.LeftJoin<xxx>((a, b) => b.Id == a.Id)
            //	.ToList(a => new { a.Id, a.Title });

            var tnsql1 = _sqlserverFixture.SqlServer.Select<xxx>().Where(a => a.Id > 0).Where(b => b.Title != null).Page(1, 3).ToSql(a => a.Id);
            var tnsql2 = _sqlserverFixture.SqlServer.Select<xxx>().Where(a => a.Id > 0).Where(b => b.Title != null).Page(2, 3).ToSql(a => a.Id);

            var tn1 = _sqlserverFixture.SqlServer.Select<xxx>().Where(a => a.Id > 0).Where(b => b.Title != null).Page(1, 3).ToList(a => a.Id);
            var tn2 = _sqlserverFixture.SqlServer.Select<xxx>().Where(a => a.Id > 0).Where(b => b.Title != null).Page(2, 3).ToList(a => a.Id);

            var t3 = _sqlserverFixture.SqlServer.Ado.Query<xxx>("select * from xxx");

            var t4 = _sqlserverFixture.SqlServer.Ado.Query<(int, int, string, string DateTime)>("select * from xxx");

            var t5 = _sqlserverFixture.SqlServer.Ado.Query<dynamic>(System.Data.CommandType.Text, "select * from xxx where Id = @Id",
                new System.Data.SqlClient.SqlParameter("Id", 1));
        }

        [Fact]
        public void QueryMultipline()
        {
            var tnsql1 = _sqlserverFixture.SqlServer.Select<xxx>().Where(a => a.Id > 0).Where(b => b.Title != null).Page(1, 3).ToSql(a => a.Id);

            var t3 = _sqlserverFixture.SqlServer.Ado.Query<xxx, (int, string, string), dynamic>("select * from xxx; select * from xxx; select * from xxx");
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
