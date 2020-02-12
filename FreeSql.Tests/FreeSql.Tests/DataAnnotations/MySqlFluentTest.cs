using FreeSql.DataAnnotations;
using MySql.Data.MySqlClient;
using System;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.DataAnnotations
{
    public class MySqlFluentTest
    {

        public MySqlFluentTest()
        {
        }

        [Fact]
        public void DisableSyncStructure()
        {
            Assert.Throws<MySqlException>(() => g.mysql.Select<ModelDisableSyncStructure>().ToList());

            g.mysql.Select<ModelSyncStructure>().ToList();
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
        public void AopConfigEntity()
        {
            g.mysql.CodeFirst.ConfigEntity<ModelAopConfigEntity>(a => a.Property(b => b.pkid).IsPrimary(true));

            g.mysql.Aop.ConfigEntity = (s, e) =>
            {
                var attr = e.EntityType.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.TableAttribute), false).FirstOrDefault() as System.ComponentModel.DataAnnotations.Schema.TableAttribute;
                if (attr != null)
                {
                    e.ModifyResult.Name = attr.Name;
                }
            };
            g.mysql.Aop.ConfigEntityProperty = (s, e) =>
            {
                if (e.Property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), false).Any())
                {
                    e.ModifyResult.IsPrimary = true;
                }
            };

            var tsql1 = g.mysql.Select<ModelAopConfigEntity>().WhereDynamic(1).ToSql();
        }
        [System.ComponentModel.DataAnnotations.Schema.Table("xxx")]
        class ModelAopConfigEntity
        {
            [System.ComponentModel.DataAnnotations.Key]
            [Column(IsPrimary = false)]
            public int pkid { get; set; }
        }

        [Fact]
        public void Fluent()
        {
            g.mysql.CodeFirst
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

            var ddl1 = g.mysql.CodeFirst.GetComparisonDDLStatements<TestFluenttb1>();
            var ddl2 = g.mysql.CodeFirst.GetComparisonDDLStatements<TestFluenttb2>();

            var t1id = g.mysql.Insert<TestFluenttb1>().AppendData(new TestFluenttb1 { }).ExecuteIdentity();
            var t1 = g.mysql.Select<TestFluenttb1>(t1id).ToOne();

            var t2lastId = g.mysql.Select<TestFluenttb2>().Max(a => a.Id);
            var t2affrows = g.mysql.Insert<TestFluenttb2>().AppendData(new TestFluenttb2 { Id = t2lastId + 1 }).ExecuteAffrows();
            var t2 = g.mysql.Select<TestFluenttb2>(t2lastId + 1).ToOne();
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

        [Fact]
        public void IsIgnore()
        {
            var item = new TestIsIgnore { };
            Assert.Equal(1, g.mysql.Insert<TestIsIgnore>().AppendData(item).ExecuteAffrows());

            var find = g.mysql.Select<TestIsIgnore>().Where(a => a.id == item.id).First();
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
            var tb1 = g.mysql.CodeFirst.GetTableByEntity(typeof(pkfalse_t1));
            var tb2 = g.mysql.CodeFirst.GetTableByEntity(typeof(pkfalse_t2));
            var tb3 = g.mysql.CodeFirst.GetTableByEntity(typeof(pkfalse_t3));

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

        [Fact]
        public void CanInsert_CanUpdate()
        {
            var item = new TestCanInsert { title = "testtitle", testfield1 = 1000, testfield2 = 1000 };
            var sql = g.mysql.Insert(item).ToSql().Replace("\r\n", "");
            Assert.Equal("INSERT INTO `TestCanInsert`(`id`, `title`, `testfield2`) VALUES(?id_0, ?title_0, ?testfield2_0)", sql);

            Assert.Equal(1, g.mysql.Insert(item).ExecuteAffrows());
            var find = g.mysql.Select<TestCanInsert>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.title, find.title);
            Assert.NotEqual(item.testfield1, find.testfield1);
            Assert.Equal(0, find.testfield1);
            Assert.Equal(item.testfield2, find.testfield2);

            item.title = "testtitle_update";
            item.testfield2 = 0;
            sql = g.mysql.Update<TestCanInsert>().SetSource(item).ToSql().Replace("\r\n", "");
            Assert.Equal($"UPDATE `TestCanInsert` SET `id` = ?p_0, `title` = ?p_1, `testfield1` = ?p_2 WHERE (`id` = '{item.id}')", sql);

            Assert.Equal(1, g.mysql.Update<TestCanInsert>().SetSource(item).ExecuteAffrows());
            find = g.mysql.Select<TestCanInsert>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.title, find.title);
            Assert.Equal(item.testfield1, find.testfield1);
            Assert.NotEqual(item.testfield2, find.testfield2);
            Assert.Equal(1000, find.testfield1);
        }
        [Index("idx_xxx", "testfield1 ASC, testfield2 DESC")]
        class TestCanInsert
        {
            public Guid id { get; set; }
            public string title { get; set; }
            [Column(CanInsert = false)]
            public long testfield1 { get; set; }
            [Column(CanUpdate = false)]
            public long testfield2 { get; set; }
        }
    }
}
