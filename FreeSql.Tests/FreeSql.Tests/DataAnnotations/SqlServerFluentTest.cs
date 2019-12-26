using FreeSql.DataAnnotations;
using FreeSql.Tests.DataContext.SqlServer;
using System;
using System.Data.SqlClient;
using Xunit;

namespace FreeSql.Tests.DataAnnotations
{
    [Collection("SqlServerCollection")]
    public class SqlServerFluentTest
    {

        SqlServerFixture _sqlserverFixture;

        public SqlServerFluentTest(SqlServerFixture sqlserverFixture)
        {
            _sqlserverFixture = sqlserverFixture;
        }

        [Fact]
        public void DisableSyncStructure()
        {
            Assert.Throws<SqlException>(() => g.sqlserver.Select<ModelDisableSyncStructure>().ToList());

            g.sqlserver.Select<ModelSyncStructure>().ToList();
        }
        [Table(DisableSyncStructure = true)]
        class ModelDisableSyncStructure
        {
            [Column(IsPrimary = false)]
            public int pkid { get; set; }
        }
        class ModelSyncStructure
        {
            [Column(IsPrimary = false)]
            public int pkid { get; set; }
        }

        [Fact]
        public void Fluent()
        {
            g.sqlserver.CodeFirst
                //.ConfigEntity<TestFluenttb1>(a => {
                //	a.Name("xxdkdkdk1");
                //	a.Property(b => b.Id).Name("Id22").IsIdentity(true);
                //	a.Property(b => b.name).DbType("varchar(100)").IsNullable(true);
                //})

                .ConfigEntity(typeof(TestFluenttb1), a =>
                {
                    a.Name("xxdkdkdk1222");
                    a.Property("Id").Name("Id22dd").IsIdentity(true);
                    a.Property("Name").DbType("varchar(101)").IsNullable(true);
                })

                .ConfigEntity<TestFluenttb2>(a =>
                {
                    a.Name("xxdkdkdk2");
                    a.Property(b => b.Id).Name("Id22").IsIdentity(true);
                    a.Property(b => b.name).DbType("varchar(100)").IsNullable(true);
                })
                ;

            var ddl1 = g.sqlserver.CodeFirst.GetComparisonDDLStatements<TestFluenttb1>();
            var ddl2 = g.sqlserver.CodeFirst.GetComparisonDDLStatements<TestFluenttb2>();

            var t1id = g.sqlserver.Insert<TestFluenttb1>().AppendData(new TestFluenttb1 { }).ExecuteIdentity();
            var t1 = g.sqlserver.Select<TestFluenttb1>(t1id).ToOne();

            var t2lastId = g.sqlserver.Select<TestFluenttb2>().Max(a => a.Id);
            var t2affrows = g.sqlserver.Insert<TestFluenttb2>().AppendData(new TestFluenttb2 { Id = t2lastId + 1 }).ExecuteAffrows();
            var t2 = g.sqlserver.Select<TestFluenttb2>(t2lastId + 1).ToOne();
        }

        [Fact]
        public void GroupPrimaryKey()
        {
            g.sqlserver.CodeFirst.SyncStructure<TestgroupkeyTb>();
            g.mysql.CodeFirst.SyncStructure<TestgroupkeyTb>();
            g.pgsql.CodeFirst.SyncStructure<TestgroupkeyTb>();
            g.sqlite.CodeFirst.SyncStructure<TestgroupkeyTb>();
            g.oracle.CodeFirst.SyncStructure<TestgroupkeyTb>();
        }

        class TestFluenttb1
        {
            public int Id { get; set; }

            public string name { get; set; } = "defaultValue";
        }

        [Table(Name = "cccccdddwww")]
        class TestFluenttb2
        {
            [Column(Name = "Idx", IsPrimary = true, IsIdentity = false)]
            public int Id { get; set; }

            public string name { get; set; } = "defaultValue";
        }

        [Table(Name = "test_groupkey")]
        class TestgroupkeyTb
        {
            [Column(IsPrimary = true)]
            public int Id { get; set; }
            [Column(IsPrimary = true)]
            public int id2 { get; set; }


            public string name { get; set; } = "defaultValue";
        }

        [Fact]
        public void IsIgnore()
        {
            var item = new TestIsIgnore { };
            Assert.Equal(1, g.sqlserver.Insert<TestIsIgnore>().AppendData(item).ExecuteAffrows());

            var find = g.sqlserver.Select<TestIsIgnore>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
        }

        class TestIsIgnore
        {
            public Guid id { get; set; }

            [Column(IsIgnore = true)]
            public bool isignore { get; set; }
        }

        [Fact]
        public void AutoPrimary()
        {
            var tb1 = g.sqlserver.CodeFirst.GetTableByEntity(typeof(pkfalse_t1));
            var tb2 = g.sqlserver.CodeFirst.GetTableByEntity(typeof(pkfalse_t2));
            var tb3 = g.sqlserver.CodeFirst.GetTableByEntity(typeof(pkfalse_t3));

            Assert.True(tb1.ColumnsByCs["id"].Attribute.IsPrimary);
            Assert.False(tb2.ColumnsByCs["id"].Attribute.IsPrimary);
            Assert.True(tb3.ColumnsByCs["id"].Attribute.IsPrimary);
        }

        class pkfalse_t1
        {
            public int id { get; set; }
        }
        class pkfalse_t2
        {
            [Column(IsPrimary = false)]
            public int id { get; set; }
        }
        class pkfalse_t3
        {
            [Column(IsPrimary = true)]
            public int id { get; set; }
        }
    }

}
