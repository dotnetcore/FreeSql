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
                //	a.Name("xxdkdkdk1").SelectFilter("a.Id22 > 0");
                //	a.Property(b => b.Id).Name("Id22").IsIdentity(true);
                //	a.Property(b => b.name).DbType("varchar(100)").IsNullable(true);
                //})

                .ConfigEntity(typeof(TestFluenttb1), a =>
                {
                    a.Name("xxdkdkdk1222").SelectFilter("a.Id22dd > 1");
                    a.Property("Id").Name("Id22dd").IsIdentity(true);
                    a.Property("Name").DbType("varchar(101)").IsNullable(true);
                })

                .ConfigEntity<TestFluenttb2>(a =>
                {
                    a.Name("xxdkdkdk2").SelectFilter("a.Idx > 0");
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
    }
}
