using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Odbc.Oracle
{
    public class OracleSelectTest
    {

        ISelect<Topic> select => g.oracle.Select<Topic>();

        [Table(Name = "tb_topic22")]
        class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int Clicks { get; set; }
            public int TypeGuid { get; set; }
            public TestTypeInfo Type { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }
        class TestTypeInfo
        {
            [Column(IsIdentity = true)]
            public int Guid { get; set; }
            public int ParentId { get; set; }
            public TestTypeParentInfo Parent { get; set; }
            public string Name { get; set; }
        }
        class TestTypeParentInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<TestTypeInfo> Types { get; set; }
        }

        class TopicInserts
        {
            public Guid Id { get; set; }
            public int Clicks { get; set; }
            public int TypeGuid { get; set; }
            public TestTypeInfo Type { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }
        public partial class Song
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public DateTime? Create_time { get; set; }
            public bool? Is_deleted { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }

            public virtual ICollection<Tag> Tags { get; set; }
        }
        public partial class Song_tag
        {
            public int Song_id { get; set; }
            public virtual Song Song { get; set; }

            public int Tag_id { get; set; }
            public virtual Tag Tag { get; set; }
        }
        public partial class Tag
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public int? Parent_id { get; set; }
            public virtual Tag Parent { get; set; }

            public decimal? Ddd { get; set; }
            public string Name { get; set; }

            public virtual ICollection<Song> Songs { get; set; }
            public virtual ICollection<Tag> Tags { get; set; }
        }

        [Fact]
        public void AsSelect()
        {
            //OneToOne、ManyToOne
            var t0 = g.oracle.Select<Tag>().Where(a => a.Parent.Parent.Name == "粤语").ToSql();
            //SELECT a.`Id`, a.`Parent_id`, a__Parent.`Id` as3, a__Parent.`Parent_id` as4, a__Parent.`Ddd`, a__Parent.`Name`, a.`Ddd` as7, a.`Name` as8 
            //FROM `Tag` a 
            //LEFT JOIN `Tag` a__Parent ON a__Parent.`Id` = a.`Parent_id` 
            //LEFT JOIN `Tag` a__Parent__Parent ON a__Parent__Parent.`Id` = a__Parent.`Parent_id` 
            //WHERE (a__Parent__Parent.`Name` = '粤语')

            //OneToMany
            var t1 = g.oracle.Select<Tag>().Where(a => a.Tags.AsSelect().Any(t => t.Parent.Id == 10)).ToSql();
            //SELECT a.`Id`, a.`Parent_id`, a.`Ddd`, a.`Name` 
            //FROM `Tag` a 
            //WHERE (exists(SELECT 1 
            //    FROM `Tag` t 
            //    LEFT JOIN `Tag` t__Parent ON t__Parent.`Id` = t.`Parent_id` 
            //    WHERE (t__Parent.`Id` = 10) AND (t.`Parent_id` = a.`Id`) 
            //    limit 0,1))

            //ManyToMany
            var t2 = g.oracle.Select<Song>().Where(s => s.Tags.AsSelect().Any(t => t.Name == "国语")).ToSql();
            //SELECT a.`Id`, a.`Create_time`, a.`Is_deleted`, a.`Title`, a.`Url` 
            //FROM `Song` a
            //WHERE(exists(SELECT 1
            //    FROM `Song_tag` Mt_Ms
            //    WHERE(Mt_Ms.`Song_id` = a.`Id`) AND(exists(SELECT 1
            //        FROM `Tag` t
            //        WHERE(t.`Name` = '国语') AND(t.`Id` = Mt_Ms.`Tag_id`)
            //        limit 0, 1))
            //    limit 0, 1))
        }

        [Fact]
        public void Lazy()
        {
            var tags = g.oracle.Select<Tag>().Where(a => a.Parent.Name == "xxx")
                .LeftJoin(a => a.Parent_id == a.Parent.Id)
                .ToSql();

            var songs = g.oracle.Select<Song>().Limit(10).ToList();
        }

        [Fact]
        public void ToDataTable()
        {
            var items = new List<TopicInserts>();
            for (var a = 0; a < 11; a++) items.Add(new TopicInserts { Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

            //Assert.Equal(1, g.oracle.Insert<TopicInserts>().AppendData(items.First()).ExecuteAffrows());
            Assert.Equal(11, g.oracle.Insert<TopicInserts>().AppendData(items).ExecuteAffrows());

            //items = Enumerable.Range(0, 9989).Select(a => new TopicInserts { Title = "newtitle" + a, CreateTime = DateTime.Now }).ToList();
            //Assert.Equal(9989, g.oracle.Insert<TopicInserts>(items).ExecuteAffrows());

            //var dt1 = select.ToDataTable();
            //var dt2 = select.ToDataTable("id, 111222");
            //var dt3 = select.ToDataTable(a => new { a.Id, a.Type.Name, now = DateTime.Now });
        }
        class TestDto
        {
            public int id { get; set; }
            public string name { get; set; } //这是join表的属性
            public int ParentId { get; set; } //这是join表的属性
        }
        class TestDto2
        {
            public int id { get; set; }
            public string name { get; set; } //这是join表的属性
            public int ParentId { get; set; } //这是join表的属性

            public TestDto2() { }
            public TestDto2(int id, string name)
            {
                this.id = id;
                this.name = name;
            }
        }
        [Fact]
        public void ToList()
        {

            var testDto1 = select.Limit(10).ToList(a => new TestDto { id = a.Id, name = a.Title });
            var testDto2 = select.Limit(10).ToList(a => new TestDto());
            var testDto3 = select.Limit(10).ToList(a => new TestDto { });
            var testDto4 = select.Limit(10).ToList(a => new TestDto() { });

            var testDto11 = select.LeftJoin(a => a.Type.Guid == a.TypeGuid).Limit(10).ToList(a => new TestDto { id = a.Id, name = a.Title });
            var testDto22 = select.LeftJoin(a => a.Type.Guid == a.TypeGuid).Limit(10).ToList(a => new TestDto());
            var testDto33 = select.LeftJoin(a => a.Type.Guid == a.TypeGuid).Limit(10).ToList(a => new TestDto { });
            var testDto44 = select.LeftJoin(a => a.Type.Guid == a.TypeGuid).Limit(10).ToList(a => new TestDto() { });

            var testDto211 = select.Limit(10).ToList(a => new TestDto2(a.Id, a.Title));
            var testDto212 = select.Limit(10).ToList(a => new TestDto2());
            var testDto213 = select.Limit(10).ToList(a => new TestDto2 { });
            var testDto214 = select.Limit(10).ToList(a => new TestDto2() { });
            var testDto215 = select.Limit(10).ToList<TestDto2>();

            var testDto2211 = select.LeftJoin(a => a.Type.Guid == a.TypeGuid).Limit(10).ToList(a => new TestDto2(a.Id, a.Title));
            var testDto2222 = select.LeftJoin(a => a.Type.Guid == a.TypeGuid).Limit(10).ToList(a => new TestDto2());
            var testDto2233 = select.LeftJoin(a => a.Type.Guid == a.TypeGuid).Limit(10).ToList(a => new TestDto2 { });
            var testDto2244 = select.LeftJoin(a => a.Type.Guid == a.TypeGuid).Limit(10).ToList(a => new TestDto2() { });
            var testDto2255 = select.LeftJoin(a => a.Type.Guid == a.TypeGuid).Limit(10).ToList<TestDto2>();

            g.oracle.Insert<TestGuidIdToList>().AppendData(new TestGuidIdToList()).ExecuteAffrows();
            var testGuidId5 = g.oracle.Select<TestGuidIdToList>().ToList();
            var testGuidId6 = g.oracle.Select<TestGuidIdToList>().ToList(a => a.id);

            var t11 = select.Where(a => a.Type.Name.Length > 0).ToList(true);
            var t21 = select.Where(a => a.Type.Parent.Name.Length > 0).ToList(true);

            g.oracle.Delete<District>().Where("1=1").ExecuteAffrows();
            var repo = g.oracle.GetRepository<District>();
            repo.DbContextOptions.EnableAddOrUpdateNavigateList = true;
            repo.Insert(new District
            {
                Code = "001",
                Name = "001_name",
                Childs = new List<District>(new[] {
                    new District{
                        Code = "001_01",
                        Name = "001_01_name"
                    },
                    new District{
                        Code = "001_02",
                        Name = "001_02_name"
                    }
                })
            });
            var ddd = g.oracle.Select<District>().LeftJoin(d => d.ParentCode == d.Parent.Code).ToTreeList();
            Assert.Single(ddd);
            Assert.Equal(2, ddd[0].Childs.Count);
        }
        public class District
        {
            [Column(IsPrimary = true, StringLength = 6)]
            public string Code { get; set; }

            [Column(StringLength = 20, IsNullable = false)]
            public string Name { get; set; }

            [Column(StringLength = 6)]
            public string ParentCode { get; set; }

            [Navigate(nameof(ParentCode))]
            public District Parent { get; set; }

            [Navigate(nameof(ParentCode))]
            public List<District> Childs { get; set; }
        }
        [Fact]
        public void ToDictionary()
        {
            g.oracle.Insert(new Topic { Title = "xxx" }).ExecuteAffrows();
            var testDto1 = select.Limit(10).ToDictionary(a => a.Id);
            var testDto2 = select.Limit(10).ToDictionary(a => a.Id, a => new { a.Id, a.Title });

            var repo = g.oracle.GetRepository<Topic>();
            var dic = repo.Select.Limit(10).ToDictionary(a => a.Id);
            var first = dic.First().Value;
            first.Clicks++;
            repo.Update(first);
        }
        class TestGuidIdToList
        {
            public Guid id { get; set; }
            public string title { get; set; } = Guid.NewGuid().ToString();
        }
        [Fact]
        public void ToOne()
        {
            var testnotfind = select.Where("1=2").First(a => a.CreateTime);
            Assert.Equal(default(DateTime), testnotfind);
        }
        [Fact]
        public void ToSql()
        {
        }
        [Fact]
        public void Any()
        {
            var count = select.Where(a => 1 == 1).Count();
            Assert.False(select.Where(a => 1 == 2).Any());
            Assert.Equal(count > 0, select.Where(a => 1 == 1).Any());

            var sql2222 = select.Where(a =>
                select.Where(b => b.Id == a.Id &&
                    select.Where(c => c.Id == b.Id).Where(d => d.Id == a.Id).Where(e => e.Id == b.Id)
                    //.Offset(a.Id)
                    .Any()
                ).Any(c => c.Id == a.Id + 10)
            );
            var sql2222Tolist = sql2222.ToList();

            var collectionSelect = select.Where(a =>
                a.Type.Guid == a.TypeGuid &&
                a.Type.Parent.Id == a.Type.ParentId &&
                a.Type.Parent.Types.AsSelect().Where(b => b.Name == a.Title).Any(b => b.ParentId == a.Type.Parent.Id)
            );
            collectionSelect.ToList();
        }
        [Fact]
        public void Count()
        {
            var count = select.Where(a => 1 == 1).Count();
            select.Where(a => 1 == 1).Count(out var count2);
            Assert.Equal(count, count2);
            Assert.Equal(0, select.Where(a => 1 == 2).Count());

            var subquery = select.ToSql(a => new
            {
                all = a,
                count = select.Where(b => b.Id > 0 && b.Id == a.Id).Count()
            });
            var subqueryList = select.ToList(a => new
            {
                all = a,
                count = select.Where(b => b.Id > 0 && b.Id == a.Id).Count()
            });
        }
        [Fact]
        public void Master()
        {
            Assert.StartsWith(" SELECT", select.Master().Where(a => 1 == 1).ToSql());
        }
        [Fact]
        public void From()
        {
            var query2 = select.From<TestTypeInfo>((s, b) => s
                 .LeftJoin(a => a.TypeGuid == b.Guid)
                );
            var sql2 = query2.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" b ON a.\"TYPEGUID\" = b.\"GUID\"", sql2);
            query2.ToList();

            var query3 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
                 .LeftJoin(a => a.TypeGuid == b.Guid)
                 .LeftJoin(a => b.ParentId == c.Id)
                );
            var sql3 = query3.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" b ON a.\"TYPEGUID\" = b.\"GUID\" LEFT JOIN \"TESTTYPEPARENTINFO\" c ON b.\"PARENTID\" = c.\"ID\"", sql3);
            query3.ToList();
        }
        [Fact]
        public void LeftJoin()
        {
            //����е�������a.Type��a.Type.Parent ���ǵ�������
            var query = select.LeftJoin(a => a.Type.Guid == a.TypeGuid);
            var sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\"", sql);
            query.ToList();

            query = select.LeftJoin(a => a.Type.Guid == a.TypeGuid && a.Type.Name == "xxx");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" AND a__Type.\"NAME\" = 'xxx'", sql);
            query.ToList();

            query = select.LeftJoin(a => a.Type.Guid == a.TypeGuid && a.Type.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" AND a__Type.\"NAME\" = 'xxx' LEFT JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\" WHERE (a__Type__Parent.\"ID\" = 10)", sql);
            query.ToList();

            //���û�е�������
            query = select.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TypeGuid);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TYPEGUID\"", sql);
            query.ToList();

            query = select.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TypeGuid && b.Name == "xxx");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TYPEGUID\" AND b.\"NAME\" = 'xxx'", sql);
            query.ToList();

            query = select.LeftJoin<TestTypeInfo>((a, a__Type) => a__Type.Guid == a.TypeGuid && a__Type.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" AND a__Type.\"NAME\" = 'xxx' LEFT JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\" WHERE (a__Type__Parent.\"ID\" = 10)", sql);
            query.ToList();

            //�������
            query = select
                .LeftJoin(a => a.Type.Guid == a.TypeGuid)
                .LeftJoin(a => a.Type.Parent.Id == a.Type.ParentId);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" LEFT JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\"", sql);
            query.ToList();

            query = select
                .LeftJoin<TestTypeInfo>((a, a__Type) => a__Type.Guid == a.TypeGuid)
                .LeftJoin<TestTypeParentInfo>((a, c) => c.Id == a.Type.ParentId);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" LEFT JOIN \"TESTTYPEPARENTINFO\" c ON c.\"ID\" = a__Type.\"PARENTID\"", sql);
            query.ToList();

            //���û�е�������b��c������ϵ
            var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
                 .LeftJoin(a => a.TypeGuid == b.Guid)
                 .LeftJoin(a => b.ParentId == c.Id));
            sql = query2.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" b ON a.\"TYPEGUID\" = b.\"GUID\" LEFT JOIN \"TESTTYPEPARENTINFO\" c ON b.\"PARENTID\" = c.\"ID\"", sql);
            query2.ToList();

            //������϶����㲻��
            query = select.LeftJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\"");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\"", sql);
            query.ToList();

            query = select.LeftJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\" and b.\"NAME\" = :bname", new { bname = "xxx" });
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\" and b.\"NAME\" = :bname", sql);
            query.ToList();
        }
        [Fact]
        public void InnerJoin()
        {
            //����е�������a.Type��a.Type.Parent ���ǵ�������
            var query = select.InnerJoin(a => a.Type.Guid == a.TypeGuid);
            var sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\"", sql);
            query.ToList();

            query = select.InnerJoin(a => a.Type.Guid == a.TypeGuid && a.Type.Name == "xxx");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" AND a__Type.\"NAME\" = 'xxx'", sql);
            query.ToList();

            query = select.InnerJoin(a => a.Type.Guid == a.TypeGuid && a.Type.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" AND a__Type.\"NAME\" = 'xxx' LEFT JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\" WHERE (a__Type__Parent.\"ID\" = 10)", sql);
            query.ToList();

            //���û�е�������
            query = select.InnerJoin<TestTypeInfo>((a, b) => b.Guid == a.TypeGuid);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TYPEGUID\"", sql);
            query.ToList();

            query = select.InnerJoin<TestTypeInfo>((a, b) => b.Guid == a.TypeGuid && b.Name == "xxx");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TYPEGUID\" AND b.\"NAME\" = 'xxx'", sql);
            query.ToList();

            query = select.InnerJoin<TestTypeInfo>((a, a__Type) => a__Type.Guid == a.TypeGuid && a__Type.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" AND a__Type.\"NAME\" = 'xxx' LEFT JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\" WHERE (a__Type__Parent.\"ID\" = 10)", sql);
            query.ToList();

            //�������
            query = select
                .InnerJoin(a => a.Type.Guid == a.TypeGuid)
                .InnerJoin(a => a.Type.Parent.Id == a.Type.ParentId);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" INNER JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\"", sql);
            query.ToList();

            query = select
                .InnerJoin<TestTypeInfo>((a, a__Type) => a__Type.Guid == a.TypeGuid)
                .InnerJoin<TestTypeParentInfo>((a, c) => c.Id == a.Type.ParentId);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" INNER JOIN \"TESTTYPEPARENTINFO\" c ON c.\"ID\" = a__Type.\"PARENTID\"", sql);
            query.ToList();

            //���û�е�������b��c������ϵ
            var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
                 .InnerJoin(a => a.TypeGuid == b.Guid)
                 .InnerJoin(a => b.ParentId == c.Id));
            sql = query2.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" b ON a.\"TYPEGUID\" = b.\"GUID\" INNER JOIN \"TESTTYPEPARENTINFO\" c ON b.\"PARENTID\" = c.\"ID\"", sql);
            query2.ToList();

            //������϶����㲻��
            query = select.InnerJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\"");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\"", sql);
            query.ToList();

            query = select.InnerJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\" and b.\"NAME\" = :bname", new { bname = "xxx" });
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\" and b.\"NAME\" = :bname", sql);
            query.ToList();

        }
        [Fact]
        public void RightJoin()
        {
            //����е�������a.Type��a.Type.Parent ���ǵ�������
            var query = select.RightJoin(a => a.Type.Guid == a.TypeGuid);
            var sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\"", sql);
            query.ToList();

            query = select.RightJoin(a => a.Type.Guid == a.TypeGuid && a.Type.Name == "xxx");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" AND a__Type.\"NAME\" = 'xxx'", sql);
            query.ToList();

            query = select.RightJoin(a => a.Type.Guid == a.TypeGuid && a.Type.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" AND a__Type.\"NAME\" = 'xxx' LEFT JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\" WHERE (a__Type__Parent.\"ID\" = 10)", sql);
            query.ToList();

            //���û�е�������
            query = select.RightJoin<TestTypeInfo>((a, b) => b.Guid == a.TypeGuid);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TYPEGUID\"", sql);
            query.ToList();

            query = select.RightJoin<TestTypeInfo>((a, b) => b.Guid == a.TypeGuid && b.Name == "xxx");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TYPEGUID\" AND b.\"NAME\" = 'xxx'", sql);
            query.ToList();

            query = select.RightJoin<TestTypeInfo>((a, a__Type) => a__Type.Guid == a.TypeGuid && a__Type.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" AND a__Type.\"NAME\" = 'xxx' LEFT JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\" WHERE (a__Type__Parent.\"ID\" = 10)", sql);
            query.ToList();

            //�������
            query = select
                .RightJoin(a => a.Type.Guid == a.TypeGuid)
                .RightJoin(a => a.Type.Parent.Id == a.Type.ParentId);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" RIGHT JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\"", sql);
            query.ToList();

            query = select
                .RightJoin<TestTypeInfo>((a, a__Type) => a__Type.Guid == a.TypeGuid)
                .RightJoin<TestTypeParentInfo>((a, c) => c.Id == a.Type.ParentId);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" RIGHT JOIN \"TESTTYPEPARENTINFO\" c ON c.\"ID\" = a__Type.\"PARENTID\"", sql);
            query.ToList();

            //���û�е�������b��c������ϵ
            var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
                 .RightJoin(a => a.TypeGuid == b.Guid)
                 .RightJoin(a => b.ParentId == c.Id));
            sql = query2.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" b ON a.\"TYPEGUID\" = b.\"GUID\" RIGHT JOIN \"TESTTYPEPARENTINFO\" c ON b.\"PARENTID\" = c.\"ID\"", sql);
            query2.ToList();

            //������϶����㲻��
            query = select.RightJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\"");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\"", sql);
            query.ToList();

            query = select.RightJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\" and b.\"NAME\" = :bname", new { bname = "xxx" });
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\" and b.\"NAME\" = :bname", sql);
            query.ToList();

        }
        [Fact]
        public void Where()
        {
            var sqltmp1 = select.Where(a => a.Id == 0 && (a.Title == "x" || a.Title == "y") && a.Clicks == 1).ToSql();
            var sqltmp2 = select.Where(a => a.Id.Equals(true) && (a.Title.Equals("x") || a.Title.Equals("y")) && a.Clicks.Equals(1)).ToSql();
            var sqltmp3 = select.Where(a => a.Id == 0).Where(a => ((a.Title == "x" && a.Title == "z") || a.Title == "y")).ToSql();

            var sqltmp4 = select.Where(a => (a.Id - 10) / 2 > 0).ToSql();

            //����е�������a.Type��a.Type.Parent ���ǵ�������
            var query = select.Where(a => a.Id == 10);
            var sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE (a.\"ID\" = 10)", sql);
            query.ToList();

            query = select.Where(a => a.Id == 10 && a.Id > 10 || a.Clicks > 100);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE ((a.\"ID\" = 10 AND a.\"ID\" > 10 OR a.\"CLICKS\" > 100))", sql);
            query.ToList();

            query = select.Where(a => a.Id == 10).Where(a => a.Clicks > 100);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE (a.\"ID\" = 10) AND (a.\"CLICKS\" > 100)", sql);
            query.ToList();

            query = select.Where(a => a.Type.Name == "typeTitle");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" WHERE (a__Type.\"NAME\" = 'typeTitle')", sql);
            query.ToList();

            query = select.Where(a => a.Type.Name == "typeTitle" && a.Type.Guid == a.TypeGuid);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" WHERE (a__Type.\"NAME\" = 'typeTitle' AND a__Type.\"GUID\" = a.\"TYPEGUID\")", sql);
            query.ToList();

            query = select.Where(a => a.Type.Parent.Name == "tparent");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" LEFT JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\" WHERE (a__Type__Parent.\"NAME\" = 'tparent')", sql);
            query.ToList();

            //���û�е������ԣ��򵥶������
            query = select.Where<TestTypeInfo>((a, b) => b.Guid == a.TypeGuid && b.Name == "typeTitle");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEINFO\" b WHERE (b.\"GUID\" = a.\"TYPEGUID\" AND b.\"NAME\" = 'typeTitle')", sql);
            query.ToList();

            query = select.Where<TestTypeInfo>((a, b) => b.Name == "typeTitle" && b.Guid == a.TypeGuid);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEINFO\" b WHERE (b.\"NAME\" = 'typeTitle' AND b.\"GUID\" = a.\"TYPEGUID\")", sql);
            query.ToList();

            query = select.Where<TestTypeInfo, TestTypeParentInfo>((a, b, c) => c.Name == "tparent");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEPARENTINFO\" c WHERE (c.\"NAME\" = 'tparent')", sql);
            query.ToList();

            //����һ�� From ��Ķ������
            var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
                .Where(a => a.Id == 10 && c.Name == "xxx")
                .Where(a => b.ParentId == 20));
            sql = query2.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEINFO\" b, \"TESTTYPEPARENTINFO\" c WHERE (a.\"ID\" = 10 AND c.\"NAME\" = 'xxx') AND (b.\"PARENTID\" = 20)", sql);
            query2.ToList();

            //������϶����㲻��
            query = select.Where("a.\"CLICKS\" > 100 and a.\"ID\" = :id", new { id = 10 });
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE (a.\"CLICKS\" > 100 and a.\"ID\" = :id)", sql);
            query.ToList();
        }
        [Fact]
        public void WhereIf()
        {
            //����е�������a.Type��a.Type.Parent ���ǵ�������
            var query = select.WhereIf(true, a => a.Id == 10);
            var sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE (a.\"ID\" = 10)", sql);
            query.ToList();

            query = select.WhereIf(true, a => a.Id == 10 && a.Id > 10 || a.Clicks > 100);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE ((a.\"ID\" = 10 AND a.\"ID\" > 10 OR a.\"CLICKS\" > 100))", sql);
            query.ToList();

            query = select.WhereIf(true, a => a.Id == 10).WhereIf(true, a => a.Clicks > 100);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE (a.\"ID\" = 10) AND (a.\"CLICKS\" > 100)", sql);
            query.ToList();

            query = select.WhereIf(true, a => a.Type.Name == "typeTitle");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" WHERE (a__Type.\"NAME\" = 'typeTitle')", sql);
            query.ToList();

            query = select.WhereIf(true, a => a.Type.Name == "typeTitle" && a.Type.Guid == a.TypeGuid);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" WHERE (a__Type.\"NAME\" = 'typeTitle' AND a__Type.\"GUID\" = a.\"TYPEGUID\")", sql);
            query.ToList();

            query = select.WhereIf(true, a => a.Type.Parent.Name == "tparent");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" LEFT JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\" WHERE (a__Type__Parent.\"NAME\" = 'tparent')", sql);
            query.ToList();

            //����һ�� From ��Ķ������
            var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
                .WhereIf(true, a => a.Id == 10 && c.Name == "xxx")
                .WhereIf(true, a => b.ParentId == 20));
            sql = query2.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEINFO\" b, \"TESTTYPEPARENTINFO\" c WHERE (a.\"ID\" = 10 AND c.\"NAME\" = 'xxx') AND (b.\"PARENTID\" = 20)", sql);
            query2.ToList();

            //������϶����㲻��
            query = select.WhereIf(true, "a.\"CLICKS\" > 100 and a.\"ID\" = :id", new { id = 10 });
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE (a.\"CLICKS\" > 100 and a.\"ID\" = :id)", sql);
            query.ToList();

            // ==========================================WhereIf(false)

            //����е�������a.Type��a.Type.Parent ���ǵ�������
            query = select.WhereIf(false, a => a.Id == 10);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a", sql);
            query.ToList();

            query = select.WhereIf(false, a => a.Id == 10 && a.Id > 10 || a.Clicks > 100);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a", sql);
            query.ToList();

            query = select.WhereIf(false, a => a.Id == 10).WhereIf(false, a => a.Clicks > 100);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a", sql);
            query.ToList();

            query = select.WhereIf(false, a => a.Type.Name == "typeTitle");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a", sql);
            query.ToList();

            query = select.WhereIf(false, a => a.Type.Name == "typeTitle" && a.Type.Guid == a.TypeGuid);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a", sql);
            query.ToList();

            query = select.WhereIf(false, a => a.Type.Parent.Name == "tparent");
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a", sql);
            query.ToList();

            //����һ�� From ��Ķ������
            query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
                .WhereIf(false, a => a.Id == 10 && c.Name == "xxx")
                .WhereIf(false, a => b.ParentId == 20));
            sql = query2.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEINFO\" b, \"TESTTYPEPARENTINFO\" c", sql);
            query2.ToList();

            //������϶����㲻��
            query = select.WhereIf(false, "a.\"CLICKS\" > 100 and a.\"ID\" = :id", new { id = 10 });
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a", sql);
            query.ToList();
        }
        [Fact]
        public void WhereExists()
        {
            var sql2222 = select.Where(a => select.Where(b => b.Id == a.Id).Any()).ToList();

            sql2222 = select.Where(a =>
                select.Where(b => b.Id == a.Id && select.Where(c => c.Id == b.Id).Where(d => d.Id == a.Id).Where(e => e.Id == b.Id)

                //.Offset(a.Id)

                .Any()
                ).Any()
            ).ToList();
        }
        [Fact]
        public void GroupBy()
        {
            var groupby = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
                .Where(a => a.Id == 1)
            )
            .GroupBy((a, b, c) => new { tt2 = a.Title.Substring(0, 2), mod4 = a.Id % 4 })
            .Having(a => a.Count() > 0 && a.Avg(a.Key.mod4) > 0 && a.Max(a.Key.mod4) > 0)
            .Having(a => a.Count() < 300 || a.Avg(a.Key.mod4) < 100)
            .OrderBy(a => a.Key.tt2)
            .OrderByDescending(a => a.Count())
            .Offset(10)
            .Limit(2)
            .Count(out var trycount)
            .ToList(a => new
            {
                a.Key.tt2,
                cou1 = a.Count(),
                cou2 = a.Count(a.Value.Item3.Id),
                arg1 = a.Avg(a.Key.mod4),
                ccc2 = a.Key.tt2 ?? "now()",
                //ccc = Convert.ToDateTime("now()"), partby = Convert.ToDecimal("sum(num) over(PARTITION BY server_id,os,rid,chn order by id desc)")
                ccc3 = a.Max(a.Value.Item3.Id)
            });

            var testpid1 = g.oracle.Insert<TestTypeInfo>().AppendData(new TestTypeInfo { Name = "Name" + DateTime.Now.ToString("yyyyMMddHHmmss") }).ExecuteIdentity();
            g.oracle.Insert<TestInfo>().AppendData(new TestInfo { Title = "Title" + DateTime.Now.ToString("yyyyMMddHHmmss"), CreateTime = DateTime.Now, TypeGuid = (int)testpid1 }).ExecuteAffrows();

            var fkfjfj = select.GroupBy(a => a.Title)
                .ToList(a => a.Sum(a.Value.TypeGuid));

            var aggsql1 = select
                .GroupBy(a => a.Title)
                .ToSql(b => new
                {
                    b.Key,
                    cou = b.Count(),
                    sum2 = b.Sum(b.Value.TypeGuid)
                });
            var aggtolist1 = select
                .GroupBy(a => a.Title)
                .ToList(b => new
                {
                    b.Key,
                    cou = b.Count(),
                    sum2 = b.Sum(b.Value.TypeGuid)
                });
            var aggtolist11 = select
                .GroupBy(a => a.Title)
                .ToDictionary(b => new
                {
                    b.Key,
                    cou = b.Count(),
                    sum2 = b.Sum(b.Value.TypeGuid)
                });

            var aggsql2 = select
                .GroupBy(a => new { a.Title, yyyy = string.Concat(a.CreateTime.Year, '-', a.CreateTime.Month) })
                .ToSql(b => new
                {
                    b.Key.Title,
                    b.Key.yyyy,

                    cou = b.Count(),
                    sum2 = b.Sum(b.Value.TypeGuid)
                });
            var aggtolist2 = select
                .GroupBy(a => new { a.Title, yyyy = string.Concat(a.CreateTime.Year, '-', a.CreateTime.Month) })
                .ToList(b => new
                {
                    b.Key.Title,
                    b.Key.yyyy,

                    cou = b.Count(),
                    sum2 = b.Sum(b.Value.TypeGuid)
                });
            var aggtolist22 = select
                .GroupBy(a => new { a.Title, yyyy = string.Concat(a.CreateTime.Year, '-', a.CreateTime.Month) })
                .ToDictionary(b => new
                {
                    b.Key.Title,
                    b.Key.yyyy,
                    b.Key,
                    cou = b.Count(),
                    sum2 = b.Sum(b.Value.TypeGuid)
                });

            var aggsql3 = select
                .GroupBy(a => a.Title)
                .ToSql(b => new
                {
                    b.Key,
                    cou = b.Count(),
                    sum2 = b.Sum(b.Value.TypeGuid),
                    sum3 = b.Sum(b.Value.Type.Parent.Id)
                });
        }
        [Fact]
        public void ToAggregate()
        {
            var sql = select.ToAggregate(a => new { sum = a.Sum(a.Key.Id + 11.11), avg = a.Avg(a.Key.Id), count = a.Count(), max = a.Max(a.Key.Id), min = a.Min(a.Key.Id) });
        }
        [Fact]
        public void OrderBy()
        {
            var sql = select.Offset(10).OrderBy(a => new Random().NextDouble()).ToList();
        }
        [Fact]
        public void OrderByRandom()
        {
            var t1 = select.OrderByRandom().Limit(10).ToSql("1");
            Assert.Equal(@"SELECT  t.* FROM (SELECT 1 
FROM ""TB_TOPIC22"" a 
ORDER BY dbms_random.value) t WHERE ROWNUM < 11", t1);
            var t2 = select.OrderByRandom().Limit(10).ToList();
        }

        [Fact]
        public void Skip_Offset()
        {
            var sql = select.Offset(10).ToList();
        }
        [Fact]
        public void Take_Limit()
        {
            var sql = select.Limit(10).ToList();
        }
        [Fact]
        public void Page()
        {
            var sql1 = select.Page(1, 10).ToList();
            var sql2 = select.Page(2, 10).ToList();
            var sql3 = select.Page(3, 10).ToList();

            var sql11 = select.OrderBy(a => new Random().NextDouble()).Page(1, 10).ToList();
            var sql22 = select.OrderBy(a => new Random().NextDouble()).Page(2, 10).ToList();
            var sql33 = select.OrderBy(a => new Random().NextDouble()).Page(3, 10).ToList();
        }
        [Fact]
        public void Distinct()
        {
            var t1 = select.Distinct().ToList(a => a.Title);
            var t2 = select.Distinct().Limit(10).ToList(a => a.Title);
        }

        [Fact]
        public void Sum()
        {
            var subquery = select.ToSql(a => new
            {
                all = a,
                count = (long)select.As("b").Sum(b => b.Id)
            });
            Assert.Equal(@"SELECT a.""ID"" as1, a.""CLICKS"" as2, a.""TYPEGUID"" as3, a.""TITLE"" as4, a.""CREATETIME"" as5, (SELECT sum(b.""ID"") 
    FROM ""TB_TOPIC22"" b) as6 
FROM ""TB_TOPIC22"" a", subquery);
            var subqueryList = select.ToList(a => new
            {
                all = a,
                count = (long)select.As("b").Sum(b => b.Id)
            });
        }
        [Fact]
        public void Min()
        {
            var subquery = select.ToSql(a => new
            {
                all = a,
                count = select.As("b").Min(b => b.Id)
            });
            Assert.Equal(@"SELECT a.""ID"" as1, a.""CLICKS"" as2, a.""TYPEGUID"" as3, a.""TITLE"" as4, a.""CREATETIME"" as5, (SELECT min(b.""ID"") 
    FROM ""TB_TOPIC22"" b) as6 
FROM ""TB_TOPIC22"" a", subquery);
            var subqueryList = select.ToList(a => new
            {
                all = a,
                count = select.As("b").Min(b => b.Id)
            });
        }
        [Fact]
        public void Max()
        {
            var subquery = select.ToSql(a => new
            {
                all = a,
                count = select.As("b").Max(b => b.Id)
            });
            Assert.Equal(@"SELECT a.""ID"" as1, a.""CLICKS"" as2, a.""TYPEGUID"" as3, a.""TITLE"" as4, a.""CREATETIME"" as5, (SELECT max(b.""ID"") 
    FROM ""TB_TOPIC22"" b) as6 
FROM ""TB_TOPIC22"" a", subquery);
            var subqueryList = select.ToList(a => new
            {
                all = a,
                count = select.As("b").Max(b => b.Id)
            });
        }
        [Fact]
        public void Avg()
        {
            var subquery = select.ToSql(a => new
            {
                all = a,
                count = select.As("b").Avg(b => b.Id)
            });
            Assert.Equal(@"SELECT a.""ID"" as1, a.""CLICKS"" as2, a.""TYPEGUID"" as3, a.""TITLE"" as4, a.""CREATETIME"" as5, (SELECT avg(b.""ID"") 
    FROM ""TB_TOPIC22"" b) as6 
FROM ""TB_TOPIC22"" a", subquery);
            var subqueryList = select.ToList(a => new
            {
                all = a,
                count = select.As("b").Avg(b => b.Id)
            });
        }
        [Fact]
        public void WhereIn()
        {
            var subquery = select.Where(a => select.As("b").ToList(b => b.Title).Contains(a.Id.ToString())).ToSql();
            Assert.Equal(@"SELECT a.""ID"", a.""CLICKS"", a.""TYPEGUID"", a.""TITLE"", a.""CREATETIME"" 
FROM ""TB_TOPIC22"" a 
WHERE (((to_char(a.""ID"")) in (SELECT b.""TITLE"" 
    FROM ""TB_TOPIC22"" b)))", subquery);
            var subqueryList = select.Where(a => select.As("b").ToList(b => b.Title).Contains(a.Id.ToString())).ToList();
        }
        [Fact]
        public void As()
        {
        }

        [Fact]
        public void AsTable()
        {

            var listt = select.AsTable((a, b) => "(select * from tb_topic where clicks > 10)").Page(1, 10).ToList();

            Func<Type, string, string> tableRule = (type, oldname) =>
            {
                if (oldname.Length > 16) oldname = oldname.Remove(16);
                if (type == typeof(Topic)) return oldname + "_T1";
                else if (type == typeof(TestTypeInfo)) return oldname + "_T2";
                return oldname + "_AT";
            };

            //����е�������a.Type��a.Type.Parent ���ǵ�������
            var query = select.LeftJoin(a => a.Type.Guid == a.TypeGuid).AsTable(tableRule);
            var sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22_T1\" a LEFT JOIN \"TESTTYPEINFO_T2\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\"", sql);

            query = select.LeftJoin(a => a.Type.Guid == a.TypeGuid && a.Type.Name == "xxx").AsTable(tableRule);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22_T1\" a LEFT JOIN \"TESTTYPEINFO_T2\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" AND a__Type.\"NAME\" = 'xxx'", sql);

            query = select.LeftJoin(a => a.Type.Guid == a.TypeGuid && a.Type.Name == "xxx").Where(a => a.Type.Parent.Id == 10).AsTable(tableRule);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22_T1\" a LEFT JOIN \"TESTTYPEINFO_T2\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" AND a__Type.\"NAME\" = 'xxx' LEFT JOIN \"TESTTYPEPARENTIN_AT\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\" WHERE (a__Type__Parent.\"ID\" = 10)", sql);

            //���û�е�������
            query = select.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TypeGuid).AsTable(tableRule);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22_T1\" a LEFT JOIN \"TESTTYPEINFO_T2\" b ON b.\"GUID\" = a.\"TYPEGUID\"", sql);

            query = select.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TypeGuid && b.Name == "xxx").AsTable(tableRule);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22_T1\" a LEFT JOIN \"TESTTYPEINFO_T2\" b ON b.\"GUID\" = a.\"TYPEGUID\" AND b.\"NAME\" = 'xxx'", sql);

            query = select.LeftJoin<TestTypeInfo>((a, a__Type) => a__Type.Guid == a.TypeGuid && a__Type.Name == "xxx").Where(a => a.Type.Parent.Id == 10).AsTable(tableRule);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22_T1\" a LEFT JOIN \"TESTTYPEINFO_T2\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" AND a__Type.\"NAME\" = 'xxx' LEFT JOIN \"TESTTYPEPARENTIN_AT\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\" WHERE (a__Type__Parent.\"ID\" = 10)", sql);

            //�������
            query = select
                .LeftJoin(a => a.Type.Guid == a.TypeGuid)
                .LeftJoin(a => a.Type.Parent.Id == a.Type.ParentId).AsTable(tableRule);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22_T1\" a LEFT JOIN \"TESTTYPEINFO_T2\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" LEFT JOIN \"TESTTYPEPARENTIN_AT\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\"", sql);

            query = select
                .LeftJoin<TestTypeInfo>((a, a__Type) => a__Type.Guid == a.TypeGuid)
                .LeftJoin<TestTypeParentInfo>((a, c) => c.Id == a.Type.ParentId).AsTable(tableRule);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22_T1\" a LEFT JOIN \"TESTTYPEINFO_T2\" a__Type ON a__Type.\"GUID\" = a.\"TYPEGUID\" LEFT JOIN \"TESTTYPEPARENTIN_AT\" c ON c.\"ID\" = a__Type.\"PARENTID\"", sql);

            //���û�е�������b��c������ϵ
            var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
                 .LeftJoin(a => a.TypeGuid == b.Guid)
                 .LeftJoin(a => b.ParentId == c.Id)).AsTable(tableRule);
            sql = query2.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22_T1\" a LEFT JOIN \"TESTTYPEINFO_T2\" b ON a.\"TYPEGUID\" = b.\"GUID\" LEFT JOIN \"TESTTYPEPARENTIN_AT\" c ON b.\"PARENTID\" = c.\"ID\"", sql);

            //������϶����㲻��
            query = select.LeftJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\"").AsTable(tableRule);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22_T1\" a LEFT JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\"", sql);

            query = select.LeftJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\" and b.\"NAME\" = :bname", new { bname = "xxx" }).AsTable(tableRule);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22_T1\" a LEFT JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TYPEGUID\" and b.\"NAME\" = :bname", sql);

            query = select.AsTable((_, old) => old).AsTable((_, old) => old);
            sql = query.ToSql().Replace("\r\n", "");
            Assert.Equal("SELECT  * from (SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a) ftb UNION ALL SELECT  * from (SELECT a.\"ID\", a.\"CLICKS\", a.\"TYPEGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a) ftb", sql);
            query.ToList();

            query = select.AsTable((_, old) => old).AsTable((_, old) => old);
            sql = query.ToSql("count(1) as1").Replace("\r\n", "");
            Assert.Equal("SELECT  * from (SELECT count(1) as1 FROM \"TB_TOPIC22\" a) ftb UNION ALL SELECT  * from (SELECT count(1) as1 FROM \"TB_TOPIC22\" a) ftb", sql);
            query.Count();

            select.AsTable((_, old) => old).AsTable((_, old) => old).Max(a => a.Id);
            select.AsTable((_, old) => old).AsTable((_, old) => old).Min(a => a.Id);
            select.AsTable((_, old) => old).AsTable((_, old) => old).Sum(a => a.Id);
            select.AsTable((_, old) => old).AsTable((_, old) => old).Avg(a => a.Id);

            var sqlsss = select
                .AsTable((type, old) => type == typeof(Topic) ? $"{old}_1" : null)
                .AsTable((type, old) => type == typeof(Topic) ? $"{old}_2" : null)
                .ToSql(a => new
                {
                    a.Id,
                    a.Clicks
                }, FieldAliasOptions.AsProperty);

            var slsld3 = select
                .AsTable((type, old) => type == typeof(Topic) ? $"({sqlsss})" : null)
                .Page(1, 20)
                .ToList(a => new
                {
                    a.Id,
                    a.Clicks
                });
        }

        public class TiOtmModel1
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public virtual TiOtmModel2 model2 { get; set; }

            public string m1name { get; set; }
        }
        public class TiOtmModel2
        {
            [Column(IsPrimary = true)]
            public int model2id { get; set; }
            public virtual TiOtmModel1 model1 { get; set; }

            public string m2setting { get; set; }

            public List<TiOtmModel3> childs { get; set; }
        }
        public class TiOtmModel3
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }

            public int model2111Idaaa { get; set; }
            public string title { get; set; }

            public List<TiOtmModel4> childs2 { get; set; }
        }
        public class TiOtmModel4
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }

            public int model3333Id333 { get; set; }
            public string title444 { get; set; }
        }

        [Fact]
        public void Include_OneToMany()
        {
            var model1 = new TiOtmModel1 { m1name = DateTime.Now.Second.ToString() };
            model1.id = (int)g.oracle.Insert(model1).ExecuteIdentity();
            var model2 = new TiOtmModel2 { model2id = model1.id, m2setting = DateTime.Now.Second.ToString() };
            g.oracle.Insert(model2).ExecuteAffrows();

            var model3_1 = new TiOtmModel3 { model2111Idaaa = model1.id, title = "testmodel3__111" };
            model3_1.id = (int)g.oracle.Insert(model3_1).ExecuteIdentity();
            var model3_2 = new TiOtmModel3 { model2111Idaaa = model1.id, title = "testmodel3__222" };
            model3_2.id = (int)g.oracle.Insert(model3_2).ExecuteIdentity();
            var model3_3 = new TiOtmModel3 { model2111Idaaa = model1.id, title = "testmodel3__333" };
            model3_3.id = (int)g.oracle.Insert(model3_2).ExecuteIdentity();

            var model4s = new[] {
                new TiOtmModel4{ model3333Id333 = model3_1.id, title444 = "testmodel3_4__111" },
                new TiOtmModel4{ model3333Id333 = model3_1.id, title444 = "testmodel3_4__222" },
                new TiOtmModel4{ model3333Id333 = model3_2.id, title444 = "testmodel3_4__111" },
                new TiOtmModel4{ model3333Id333 = model3_2.id, title444 = "testmodel3_4__222" },
                new TiOtmModel4{ model3333Id333 = model3_2.id, title444 = "testmodel3_4__333" }
            };
            Assert.Equal(5, g.oracle.Insert(model4s).ExecuteAffrows());

            var t0 = g.oracle.Select<TiOtmModel2>()
                .IncludeMany(a => a.childs.Where(m3 => m3.model2111Idaaa == a.model2id))
                .Where(a => a.model2id <= model1.id)
                .ToList();

            var t1 = g.oracle.Select<TiOtmModel1>()
                .IncludeMany(a => a.model2.childs.Where(m3 => m3.model2111Idaaa == a.model2.model2id))
                .Where(a => a.id <= model1.id)
                .ToList();

            var t2 = g.oracle.Select<TiOtmModel1>()
                .IncludeMany(a => a.model2.childs.Where(m3 => m3.model2111Idaaa == a.model2.model2id),
                    then => then.IncludeMany(m3 => m3.childs2.Where(m4 => m4.model3333Id333 == m3.id)))
                .Where(a => a.id <= model1.id)
                .ToList();

            var t00 = g.oracle.Select<TiOtmModel2>()
                .IncludeMany(a => a.childs.Take(1).Where(m3 => m3.model2111Idaaa == a.model2id))
                .Where(a => a.model2id <= model1.id)
                .ToList();

            var t11 = g.oracle.Select<TiOtmModel1>()
                .IncludeMany(a => a.model2.childs.Take(1).Where(m3 => m3.model2111Idaaa == a.model2.model2id))
                .Where(a => a.id <= model1.id)
                .ToList();

            var t22 = g.oracle.Select<TiOtmModel1>()
                .IncludeMany(a => a.model2.childs.Take(1).Where(m3 => m3.model2111Idaaa == a.model2.model2id),
                    then => then.IncludeMany(m3 => m3.childs2.Take(2).Where(m4 => m4.model3333Id333 == m3.id)))
                .Where(a => a.id <= model1.id)
                .ToList();

            //---- Select ----

            var at0 = g.oracle.Select<TiOtmModel2>()
                .IncludeMany(a => a.childs.Where(m3 => m3.model2111Idaaa == a.model2id).Select(m3 => new TiOtmModel3 { id = m3.id }))
                .Where(a => a.model2id <= model1.id)
                .ToList();

            var at1 = g.oracle.Select<TiOtmModel1>()
                .IncludeMany(a => a.model2.childs.Where(m3 => m3.model2111Idaaa == a.model2.model2id).Select(m3 => new TiOtmModel3 { id = m3.id }))
                .Where(a => a.id <= model1.id)
                .ToList();

            var at2 = g.oracle.Select<TiOtmModel1>()
                .IncludeMany(a => a.model2.childs.Where(m3 => m3.model2111Idaaa == a.model2.model2id).Select(m3 => new TiOtmModel3 { id = m3.id }),
                    then => then.IncludeMany(m3 => m3.childs2.Where(m4 => m4.model3333Id333 == m3.id).Select(m4 => new TiOtmModel4 { id = m4.id })))
                .Where(a => a.id <= model1.id)
                .ToList();

            var at00 = g.oracle.Select<TiOtmModel2>()
                .IncludeMany(a => a.childs.Take(1).Where(m3 => m3.model2111Idaaa == a.model2id).Select(m3 => new TiOtmModel3 { id = m3.id }))
                .Where(a => a.model2id <= model1.id)
                .ToList();

            var at11 = g.oracle.Select<TiOtmModel1>()
                .IncludeMany(a => a.model2.childs.Take(1).Where(m3 => m3.model2111Idaaa == a.model2.model2id).Select(m3 => new TiOtmModel3 { id = m3.id }))
                .Where(a => a.id <= model1.id)
                .ToList();

            var at22 = g.oracle.Select<TiOtmModel1>()
                .IncludeMany(a => a.model2.childs.Take(1).Where(m3 => m3.model2111Idaaa == a.model2.model2id).Select(m3 => new TiOtmModel3 { id = m3.id }),
                    then => then.IncludeMany(m3 => m3.childs2.Take(2).Where(m4 => m4.model3333Id333 == m3.id).Select(m4 => new TiOtmModel4 { id = m4.id })))
                .Where(a => a.id <= model1.id)
                .ToList();
        }

        public class TiOtmModel11
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public int model2id { get; set; }
            public string m3setting { get; set; }
            public TiOtmModel22 model2 { get; set; }
            public string m1name { get; set; }
        }

        public class TiOtmModel22
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string m2setting { get; set; }
            public string aaa { get; set; }
            public string bbb { get; set; }
            public List<TiOtmModel33> childs { get; set; }
        }
        public class TiOtmModel33
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public int model2Id { get; set; }
            public string title { get; set; }
            public string setting { get; set; }
        }
        [Fact]
        public void Include_OneToMany2()
        {
            string setting = "x";
            var model2 = new TiOtmModel22 { m2setting = DateTime.Now.Second.ToString(), aaa = "aaa" + DateTime.Now.Second, bbb = "bbb" + DateTime.Now.Second };
            model2.id = (int)g.oracle.Insert(model2).ExecuteIdentity();

            var model3s = new[]
            {
                new TiOtmModel33 {model2Id = model2.id, title = "testmodel3__111", setting = setting},
                new TiOtmModel33 {model2Id = model2.id, title = "testmodel3__222", setting = setting},
                new TiOtmModel33 {model2Id = model2.id, title = "testmodel3__333", setting = setting}
            };
            Assert.Equal(3, g.oracle.Insert(model3s).ExecuteAffrows());

            var model1 = new TiOtmModel11 { m1name = DateTime.Now.Second.ToString(), model2id = model2.id, m3setting = setting };
            model1.id = (int)g.oracle.Insert(model1).ExecuteIdentity();

            var t1 = g.oracle.Select<TiOtmModel11>()
                .LeftJoin(a => a.model2id == a.model2.id)
                .IncludeMany(a => a.model2.childs.Where(m3 => m3.model2Id == a.model2.id && m3.setting == a.m3setting))
                .Where(a => a.id <= model1.id)
                .ToList(true);

            var t11 = g.oracle.Select<TiOtmModel11>()
                .LeftJoin(a => a.model2id == a.model2.id)
                .IncludeMany(a => a.model2.childs.Take(1).Where(m3 => m3.model2Id == a.model2.id && m3.setting == a.m3setting))
                .Where(a => a.id <= model1.id)
                .ToList(true);

            //---- Select ----

            var at1 = g.oracle.Select<TiOtmModel11>()
                .LeftJoin(a => a.model2id == a.model2.id)
                .IncludeMany(a => a.model2.childs.Where(m3 => m3.model2Id == a.model2.id && m3.setting == a.m3setting).Select(m3 => new TiOtmModel33 { title = m3.title }))
                .Where(a => a.id <= model1.id)
                .ToList(true);

            var at11 = g.oracle.Select<TiOtmModel11>()
                .LeftJoin(a => a.model2id == a.model2.id)
                .IncludeMany(a => a.model2.childs.Take(1).Where(m3 => m3.model2Id == a.model2.id && m3.setting == a.m3setting).Select(m3 => new TiOtmModel33 { title = m3.title }))
                .Where(a => a.id <= model1.id)
                .ToList(true);
        }

        [Fact]
        public void Include_OneToChilds()
        {
            var tag1 = new Tag
            {
                Ddd = DateTime.Now.Second,
                Name = "test_oneToChilds_01_中国"
            };
            tag1.Id = (int)g.oracle.Insert(tag1).ExecuteIdentity();
            var tag1_1 = new Tag
            {
                Parent_id = tag1.Id,
                Ddd = DateTime.Now.Second,
                Name = "test_oneToChilds_01_北京"
            };
            tag1_1.Id = (int)g.oracle.Insert(tag1_1).ExecuteIdentity();
            var tag1_2 = new Tag
            {
                Parent_id = tag1.Id,
                Ddd = DateTime.Now.Second,
                Name = "test_oneToChilds_01_上海"
            };
            tag1_2.Id = (int)g.oracle.Insert(tag1_2).ExecuteIdentity();

            var tag2 = new Tag
            {
                Ddd = DateTime.Now.Second,
                Name = "test_oneToChilds_02_美国"
            };
            tag2.Id = (int)g.oracle.Insert(tag2).ExecuteIdentity();
            var tag2_1 = new Tag
            {
                Parent_id = tag2.Id,
                Ddd = DateTime.Now.Second,
                Name = "test_oneToChilds_02_纽约"
            };
            tag2_1.Id = (int)g.oracle.Insert(tag2_1).ExecuteIdentity();
            var tag2_2 = new Tag
            {
                Parent_id = tag2.Id,
                Ddd = DateTime.Now.Second,
                Name = "test_oneToChilds_02_华盛顿"
            };
            tag2_2.Id = (int)g.oracle.Insert(tag2_2).ExecuteIdentity();

            var tags0 = g.oracle.Select<Tag>()
                .Include(a => a.Parent)
                .Where(a => a.Id == tag1.Id || a.Id == tag2.Id)
                .ToList();

            var tags1 = g.oracle.Select<Tag>()
                .IncludeMany(a => a.Tags)
                .Include(a => a.Parent)
                .IncludeMany(a => a.Songs)
                .Where(a => a.Id == tag1.Id || a.Id == tag2.Id)
                .ToList();

            var tags2 = g.oracle.Select<Tag>()
                .IncludeMany(a => a.Tags,
                    then => then.Include(a => a.Parent).IncludeMany(a => a.Songs))
                .Include(a => a.Parent)
                .IncludeMany(a => a.Songs)
                .Where(a => a.Id == tag1.Id || a.Id == tag2.Id)
                .ToList();

            var tags3 = g.oracle.Select<Tag>()
                .IncludeMany(a => a.Tags,
                    then => then.Include(a => a.Parent).IncludeMany(a => a.Songs).IncludeMany(a => a.Tags))
                .Include(a => a.Parent)
                .IncludeMany(a => a.Songs)
                .Where(a => a.Id == tag1.Id || a.Id == tag2.Id)
                .ToList();

            var tags11 = g.oracle.Select<Tag>()
                .IncludeMany(a => a.Tags.Take(1))
                .Include(a => a.Parent)
                .IncludeMany(a => a.Songs.Take(1))
                .Where(a => a.Id == tag1.Id || a.Id == tag2.Id)
                .ToList();

            var tags22 = g.oracle.Select<Tag>()
                .IncludeMany(a => a.Tags.Take(1),
                    then => then.Include(a => a.Parent).IncludeMany(a => a.Songs.Take(1)))
                .Include(a => a.Parent)
                .IncludeMany(a => a.Songs.Take(1))
                .Where(a => a.Id == tag1.Id || a.Id == tag2.Id)
                .ToList();

            var tags33 = g.oracle.Select<Tag>()
                .IncludeMany(a => a.Tags.Take(1),
                    then => then.Include(a => a.Parent).IncludeMany(a => a.Songs.Take(1)).IncludeMany(a => a.Tags.Take(1)))
                .Include(a => a.Parent)
                .IncludeMany(a => a.Songs.Take(1))
                .Where(a => a.Id == tag1.Id || a.Id == tag2.Id)
                .ToList();

            // --- Select ---

            var atags0 = g.oracle.Select<Tag>()
                .Include(a => a.Parent)
                .Where(a => a.Id == tag1.Id || a.Id == tag2.Id)
                .ToList();

            var atags1 = g.oracle.Select<Tag>()
                .IncludeMany(a => a.Tags.Select(b => new Tag { Id = b.Id, Name = b.Name }))
                .Include(a => a.Parent)
                .IncludeMany(a => a.Songs.Select(b => new Song { Id = b.Id, Title = b.Title }))
                .Where(a => a.Id == tag1.Id || a.Id == tag2.Id)
                .ToList();

            var atags2 = g.oracle.Select<Tag>()
                .IncludeMany(a => a.Tags.Select(b => new Tag { Id = b.Id, Name = b.Name }),
                    then => then.Include(a => a.Parent).IncludeMany(a => a.Songs))
                .Include(a => a.Parent)
                .IncludeMany(a => a.Songs.Select(b => new Song { Id = b.Id, Title = b.Title }))
                .Where(a => a.Id == tag1.Id || a.Id == tag2.Id)
                .ToList();

            var atags3 = g.oracle.Select<Tag>()
                .IncludeMany(a => a.Tags.Select(b => new Tag { Id = b.Id, Name = b.Name }),
                    then => then.Include(a => a.Parent).IncludeMany(a => a.Songs.Select(b => new Song { Id = b.Id, Title = b.Title })).IncludeMany(a => a.Tags.Select(b => new Tag { Id = b.Id, Name = b.Name })))
                .Include(a => a.Parent)
                .IncludeMany(a => a.Songs.Select(b => new Song { Id = b.Id, Title = b.Title }))
                .Where(a => a.Id == tag1.Id || a.Id == tag2.Id)
                .ToList();

            var atags11 = g.oracle.Select<Tag>()
                .IncludeMany(a => a.Tags.Take(1).Select(b => new Tag { Id = b.Id, Name = b.Name }))
                .Include(a => a.Parent)
                .IncludeMany(a => a.Songs.Take(1).Select(b => new Song { Id = b.Id, Title = b.Title }))
                .Where(a => a.Id == tag1.Id || a.Id == tag2.Id)
                .ToList();

            var atags22 = g.oracle.Select<Tag>()
                .IncludeMany(a => a.Tags.Take(1).Select(b => new Tag { Id = b.Id, Name = b.Name }),
                    then => then.Include(a => a.Parent).IncludeMany(a => a.Songs.Take(1).Select(b => new Song { Id = b.Id, Title = b.Title })))
                .Include(a => a.Parent)
                .IncludeMany(a => a.Songs.Take(1).Select(b => new Song { Id = b.Id, Title = b.Title }))
                .Where(a => a.Id == tag1.Id || a.Id == tag2.Id)
                .ToList();

            var atags33 = g.oracle.Select<Tag>()
                .IncludeMany(a => a.Tags.Take(1).Select(b => new Tag { Id = b.Id, Name = b.Name }),
                    then => then.Include(a => a.Parent).IncludeMany(a => a.Songs.Take(1).Select(b => new Song { Id = b.Id, Title = b.Title })).IncludeMany(a => a.Tags.Take(1).Select(b => new Tag { Id = b.Id, Name = b.Name })))
                .Include(a => a.Parent)
                .IncludeMany(a => a.Songs.Take(1).Select(b => new Song { Id = b.Id, Title = b.Title }))
                .Where(a => a.Id == tag1.Id || a.Id == tag2.Id)
                .ToList();
        }

        [Fact]
        public void Include_ManyToMany()
        {

            var tag1 = new Tag
            {
                Ddd = DateTime.Now.Second,
                Name = "test_manytoMany_01_中国"
            };
            tag1.Id = (int)g.oracle.Insert(tag1).ExecuteIdentity();
            var tag2 = new Tag
            {
                Ddd = DateTime.Now.Second,
                Name = "test_manytoMany_02_美国"
            };
            tag2.Id = (int)g.oracle.Insert(tag2).ExecuteIdentity();
            var tag3 = new Tag
            {
                Ddd = DateTime.Now.Second,
                Name = "test_manytoMany_03_日本"
            };
            tag3.Id = (int)g.oracle.Insert(tag3).ExecuteIdentity();

            var song1 = new Song
            {
                Create_time = DateTime.Now,
                Title = "test_manytoMany_01_我是中国人.mp3",
                Url = "http://ww.baidu.com/"
            };
            song1.Id = (int)g.oracle.Insert(song1).ExecuteIdentity();
            var song2 = new Song
            {
                Create_time = DateTime.Now,
                Title = "test_manytoMany_02_爱你一万年.mp3",
                Url = "http://ww.163.com/"
            };
            song2.Id = (int)g.oracle.Insert(song2).ExecuteIdentity();
            var song3 = new Song
            {
                Create_time = DateTime.Now,
                Title = "test_manytoMany_03_千年等一回.mp3",
                Url = "http://ww.sina.com/"
            };
            song3.Id = (int)g.oracle.Insert(song3).ExecuteIdentity();

            g.oracle.Insert(new Song_tag { Song_id = song1.Id, Tag_id = tag1.Id }).ExecuteAffrows();
            g.oracle.Insert(new Song_tag { Song_id = song2.Id, Tag_id = tag1.Id }).ExecuteAffrows();
            g.oracle.Insert(new Song_tag { Song_id = song3.Id, Tag_id = tag1.Id }).ExecuteAffrows();
            g.oracle.Insert(new Song_tag { Song_id = song1.Id, Tag_id = tag2.Id }).ExecuteAffrows();
            g.oracle.Insert(new Song_tag { Song_id = song3.Id, Tag_id = tag2.Id }).ExecuteAffrows();
            g.oracle.Insert(new Song_tag { Song_id = song3.Id, Tag_id = tag3.Id }).ExecuteAffrows();

            var songs1 = g.oracle.Select<Song>()
                .IncludeMany(a => a.Tags)
                .Where(a => a.Id == song1.Id || a.Id == song2.Id || a.Id == song3.Id)
                .ToList();
            Assert.Equal(3, songs1.Count);
            Assert.Equal(2, songs1[0].Tags.Count);
            Assert.Equal(1, songs1[1].Tags.Count);
            Assert.Equal(3, songs1[2].Tags.Count);

            var songs2 = g.oracle.Select<Song>()
                .IncludeMany(a => a.Tags,
                    then => then.IncludeMany(t => t.Songs))
                .Where(a => a.Id == song1.Id || a.Id == song2.Id || a.Id == song3.Id)
                .ToList();
            Assert.Equal(3, songs2.Count);
            Assert.Equal(2, songs2[0].Tags.Count);
            Assert.Equal(1, songs2[1].Tags.Count);
            Assert.Equal(3, songs2[2].Tags.Count);

            var tags3 = g.oracle.Select<Song_tag>()
                .Include(a => a.Tag.Parent)
                .IncludeMany(a => a.Tag.Songs)
                .Where(a => a.Tag.Id == tag1.Id || a.Tag.Id == tag2.Id)
                .ToList(true);


            var songs11 = g.oracle.Select<Song>()
                .IncludeMany(a => a.Tags.Take(1))
                .Where(a => a.Id == song1.Id || a.Id == song2.Id || a.Id == song3.Id)
                .ToList();
            Assert.Equal(3, songs11.Count);
            Assert.Equal(1, songs11[0].Tags.Count);
            Assert.Equal(1, songs11[1].Tags.Count);
            Assert.Equal(1, songs11[2].Tags.Count);

            var songs22 = g.oracle.Select<Song>()
                .IncludeMany(a => a.Tags.Take(1),
                    then => then.IncludeMany(t => t.Songs.Take(1)))
                .Where(a => a.Id == song1.Id || a.Id == song2.Id || a.Id == song3.Id)
                .ToList();
            Assert.Equal(3, songs22.Count);
            Assert.Equal(1, songs22[0].Tags.Count);
            Assert.Equal(1, songs22[1].Tags.Count);
            Assert.Equal(1, songs22[2].Tags.Count);

            var tags33 = g.oracle.Select<Song_tag>()
                .Include(a => a.Tag.Parent)
                .IncludeMany(a => a.Tag.Songs.Take(1))
                .Where(a => a.Tag.Id == tag1.Id || a.Tag.Id == tag2.Id)
                .ToList(true);

            // --- Select ---

            new List<Song>(new[] { song1, song2, song3 }).IncludeMany(g.oracle, a => a.Tags.Select(b => new Tag { Id = b.Id, Name = b.Name }));

            var asongs1 = g.oracle.Select<Song>()
                .IncludeMany(a => a.Tags.Select(b => new Tag { Id = b.Id, Name = b.Name }))
                .Where(a => a.Id == song1.Id || a.Id == song2.Id || a.Id == song3.Id)
                .ToList();
            Assert.Equal(3, songs1.Count);
            Assert.Equal(2, songs1[0].Tags.Count);
            Assert.Equal(1, songs1[1].Tags.Count);
            Assert.Equal(3, songs1[2].Tags.Count);

            var asongs2 = g.oracle.Select<Song>()
                .IncludeMany(a => a.Tags.Select(b => new Tag { Id = b.Id, Name = b.Name }),
                    then => then.IncludeMany(t => t.Songs.Select(b => new Song { Id = b.Id, Title = b.Title })))
                .Where(a => a.Id == song1.Id || a.Id == song2.Id || a.Id == song3.Id)
                .ToList();
            Assert.Equal(3, songs2.Count);
            Assert.Equal(2, songs2[0].Tags.Count);
            Assert.Equal(1, songs2[1].Tags.Count);
            Assert.Equal(3, songs2[2].Tags.Count);

            var atags3 = g.oracle.Select<Song_tag>()
                .Include(a => a.Tag.Parent)
                .IncludeMany(a => a.Tag.Songs.Select(b => new Song { Id = b.Id, Title = b.Title }))
                .Where(a => a.Tag.Id == tag1.Id || a.Tag.Id == tag2.Id)
                .ToList(true);


            var asongs11 = g.oracle.Select<Song>()
                .IncludeMany(a => a.Tags.Take(1).Select(b => new Tag { Id = b.Id, Name = b.Name }))
                .Where(a => a.Id == song1.Id || a.Id == song2.Id || a.Id == song3.Id)
                .ToList();
            Assert.Equal(3, songs11.Count);
            Assert.Equal(1, songs11[0].Tags.Count);
            Assert.Equal(1, songs11[1].Tags.Count);
            Assert.Equal(1, songs11[2].Tags.Count);

            var asongs22 = g.oracle.Select<Song>()
                .IncludeMany(a => a.Tags.Take(1).Select(b => new Tag { Id = b.Id, Name = b.Name }),
                    then => then.IncludeMany(t => t.Songs.Take(1).Select(b => new Song { Id = b.Id, Title = b.Title })))
                .Where(a => a.Id == song1.Id || a.Id == song2.Id || a.Id == song3.Id)
                .ToList();
            Assert.Equal(3, songs22.Count);
            Assert.Equal(1, songs22[0].Tags.Count);
            Assert.Equal(1, songs22[1].Tags.Count);
            Assert.Equal(1, songs22[2].Tags.Count);

            var atags33 = g.oracle.Select<Song_tag>()
                .Include(a => a.Tag.Parent)
                .IncludeMany(a => a.Tag.Songs.Take(1).Select(b => new Song { Id = b.Id, Title = b.Title }))
                .Where(a => a.Tag.Id == tag1.Id || a.Tag.Id == tag2.Id)
                .ToList(true);
        }

        public class ToDel1Pk
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string name { get; set; }
        }
        public class ToDel2Pk
        {
            [Column(IsPrimary = true)]
            public Guid pk1 { get; set; }
            [Column(IsPrimary = true)]
            public string pk2 { get; set; }
            public string name { get; set; }
        }
        public class ToDel3Pk
        {
            [Column(IsPrimary = true)]
            public Guid pk1 { get; set; }
            [Column(IsPrimary = true)]
            public int pk2 { get; set; }
            [Column(IsPrimary = true)]
            public string pk3 { get; set; }
            public string name { get; set; }
        }
        [Fact]
        public void ToDelete()
        {
            g.oracle.Select<ToDel1Pk>().ToDelete().ExecuteAffrows();
            Assert.Equal(0, g.oracle.Select<ToDel1Pk>().Count());
            g.oracle.Insert(new[] {
                new ToDel1Pk{ name = "name1"},
                new ToDel1Pk{ name = "name2"},
                new ToDel1Pk{ name = "nick1"},
                new ToDel1Pk{ name = "nick2"},
                new ToDel1Pk{ name = "nick3"}
            }).ExecuteAffrows();
            Assert.Equal(2, g.oracle.Select<ToDel1Pk>().Where(a => a.name.StartsWith("name")).ToDelete().ExecuteAffrows());
            Assert.Equal(3, g.oracle.Select<ToDel1Pk>().Count());
            Assert.Equal(3, g.oracle.Select<ToDel1Pk>().Where(a => a.name.StartsWith("nick")).Count());

            g.oracle.Select<ToDel2Pk>().ToDelete().ExecuteAffrows();
            Assert.Equal(0, g.oracle.Select<ToDel2Pk>().Count());
            g.oracle.Insert(new[] {
                new ToDel2Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = "pk2", name = "name1"},
                new ToDel2Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = "pk2", name = "name2"},
                new ToDel2Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = "pk2", name = "nick1"},
                new ToDel2Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = "pk2", name = "nick2"},
                new ToDel2Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = "pk2", name = "nick3"}
            }).ExecuteAffrows();
            Assert.Equal(2, g.oracle.Select<ToDel2Pk>().Where(a => a.name.StartsWith("name")).ToDelete().ExecuteAffrows());
            Assert.Equal(3, g.oracle.Select<ToDel2Pk>().Count());
            Assert.Equal(3, g.oracle.Select<ToDel2Pk>().Where(a => a.name.StartsWith("nick")).Count());

            g.oracle.Select<ToDel3Pk>().ToDelete().ExecuteAffrows();
            Assert.Equal(0, g.oracle.Select<ToDel3Pk>().Count());
            g.oracle.Insert(new[] {
                new ToDel3Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = 1, pk3 = "pk3", name = "name1"},
                new ToDel3Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = 1, pk3 = "pk3", name = "name2"},
                new ToDel3Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = 1, pk3 = "pk3", name = "nick1"},
                new ToDel3Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = 1, pk3 = "pk3", name = "nick2"},
                new ToDel3Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = 1, pk3 = "pk3", name = "nick3"}
            }).ExecuteAffrows();
            Assert.Equal(2, g.oracle.Select<ToDel3Pk>().Where(a => a.name.StartsWith("name")).ToDelete().ExecuteAffrows());
            Assert.Equal(3, g.oracle.Select<ToDel3Pk>().Count());
            Assert.Equal(3, g.oracle.Select<ToDel3Pk>().Where(a => a.name.StartsWith("nick")).Count());
        }

        public class ToUpd1Pk
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string name { get; set; }
        }
        public class ToUpd2Pk
        {
            [Column(IsPrimary = true)]
            public Guid pk1 { get; set; }
            [Column(IsPrimary = true)]
            public string pk2 { get; set; }
            public string name { get; set; }
        }
        public class ToUpd3Pk
        {
            [Column(IsPrimary = true)]
            public Guid pk1 { get; set; }
            [Column(IsPrimary = true)]
            public int pk2 { get; set; }
            [Column(IsPrimary = true)]
            public string pk3 { get; set; }
            public string name { get; set; }
        }
        [Fact]
        public void ToUpdate()
        {
            g.oracle.Select<ToUpd1Pk>().ToDelete().ExecuteAffrows();
            Assert.Equal(0, g.oracle.Select<ToUpd1Pk>().Count());
            g.oracle.Insert(new[] {
                new ToUpd1Pk{ name = "name1"},
                new ToUpd1Pk{ name = "name2"},
                new ToUpd1Pk{ name = "nick1"},
                new ToUpd1Pk{ name = "nick2"},
                new ToUpd1Pk{ name = "nick3"}
            }).ExecuteAffrows();
            Assert.Equal(2, g.oracle.Select<ToUpd1Pk>().Where(a => a.name.StartsWith("name")).ToUpdate().Set(a => a.name, "nick?").ExecuteAffrows());
            Assert.Equal(5, g.oracle.Select<ToUpd1Pk>().Count());
            Assert.Equal(5, g.oracle.Select<ToUpd1Pk>().Where(a => a.name.StartsWith("nick")).Count());

            g.oracle.Select<ToUpd2Pk>().ToDelete().ExecuteAffrows();
            Assert.Equal(0, g.oracle.Select<ToUpd2Pk>().Count());
            g.oracle.Insert(new[] {
                new ToUpd2Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = "pk2", name = "name1"},
                new ToUpd2Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = "pk2", name = "name2"},
                new ToUpd2Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = "pk2", name = "nick1"},
                new ToUpd2Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = "pk2", name = "nick2"},
                new ToUpd2Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = "pk2", name = "nick3"}
            }).ExecuteAffrows();
            Assert.Equal(2, g.oracle.Select<ToUpd2Pk>().Where(a => a.name.StartsWith("name")).ToUpdate().Set(a => a.name, "nick?").ExecuteAffrows());
            Assert.Equal(5, g.oracle.Select<ToUpd2Pk>().Count());
            Assert.Equal(5, g.oracle.Select<ToUpd2Pk>().Where(a => a.name.StartsWith("nick")).Count());

            g.oracle.Select<ToUpd3Pk>().ToDelete().ExecuteAffrows();
            Assert.Equal(0, g.oracle.Select<ToUpd3Pk>().Count());
            g.oracle.Insert(new[] {
                new ToUpd3Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = 1, pk3 = "pk3", name = "name1"},
                new ToUpd3Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = 1, pk3 = "pk3", name = "name2"},
                new ToUpd3Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = 1, pk3 = "pk3", name = "nick1"},
                new ToUpd3Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = 1, pk3 = "pk3", name = "nick2"},
                new ToUpd3Pk{ pk1 = FreeUtil.NewMongodbId(), pk2 = 1, pk3 = "pk3", name = "nick3"}
            }).ExecuteAffrows();
            Assert.Equal(2, g.oracle.Select<ToUpd3Pk>().Where(a => a.name.StartsWith("name")).ToUpdate().Set(a => a.name, "nick?").ExecuteAffrows());
            Assert.Equal(5, g.oracle.Select<ToUpd3Pk>().Count());
            Assert.Equal(5, g.oracle.Select<ToUpd3Pk>().Where(a => a.name.StartsWith("nick")).Count());
        }

        [Fact]
        public void ForUpdate()
        {
            var orm = g.oracle;

            Assert.Equal("安全起见，请务必在事务开启之后，再使用 ForUpdate",
                Assert.Throws<Exception>(() => orm.Select<ToUpd1Pk>().ForUpdate().Limit(1).ToList())?.Message);

            orm.Transaction(() =>
            {
                var sql = orm.Select<ToUpd1Pk>().ForUpdate().Limit(1).ToSql().Replace("\r\n", "");
                Assert.Equal("SELECT a.\"ID\", a.\"NAME\" FROM \"TOUPD1PK\" a WHERE ROWNUM < 2 for update", sql);
                orm.Select<ToUpd1Pk>().ForUpdate().Limit(1).ToList();

                sql = orm.Select<ToUpd1Pk>().ForUpdate(true).Limit(1).ToSql().Replace("\r\n", "");
                Assert.Equal("SELECT a.\"ID\", a.\"NAME\" FROM \"TOUPD1PK\" a WHERE ROWNUM < 2 for update nowait", sql);
                orm.Select<ToUpd1Pk>().ForUpdate(true).Limit(1).ToList();
            });
        }

        [Fact]
        public void ToTreeList()
        {
            var fsql = g.oracle;
            fsql.Delete<BaseDistrict>().Where("1=1").ExecuteAffrows();
            var repo = fsql.GetRepository<VM_District_Child>();
            repo.DbContextOptions.EnableAddOrUpdateNavigateList = true;
            repo.DbContextOptions.NoneParameter = true;
            repo.Insert(new VM_District_Child
            {
                Code = "100000",
                Name = "中国",
                Childs = new List<VM_District_Child>(new[] {
                    new VM_District_Child
                    {
                        Code = "110000",
                        Name = "北京",
                        Childs = new List<VM_District_Child>(new[] {
                            new VM_District_Child{ Code="110100", Name = "北京市" },
                            new VM_District_Child{ Code="110101", Name = "东城区" },
                        })
                    }
                })
            });

            var t1 = fsql.Select<VM_District_Parent>()
                .InnerJoin(a => a.ParentCode == a.Parent.Code)
                .Where(a => a.Code == "110101")
                .ToList(true);
            Assert.Single(t1);
            Assert.Equal("110101", t1[0].Code);
            Assert.NotNull(t1[0].Parent);
            Assert.Equal("110000", t1[0].Parent.Code);

            var t2 = fsql.Select<VM_District_Parent>()
                .InnerJoin(a => a.ParentCode == a.Parent.Code)
                .InnerJoin(a => a.Parent.ParentCode == a.Parent.Parent.Code)
                .Where(a => a.Code == "110101")
                .ToList(true);
            Assert.Single(t2);
            Assert.Equal("110101", t2[0].Code);
            Assert.NotNull(t2[0].Parent);
            Assert.Equal("110000", t2[0].Parent.Code);
            Assert.NotNull(t2[0].Parent.Parent);
            Assert.Equal("100000", t2[0].Parent.Parent.Code);

            var t3 = fsql.Select<VM_District_Child>().ToTreeList();
            Assert.Single(t3);
            Assert.Equal("100000", t3[0].Code);
            Assert.Single(t3[0].Childs);
            Assert.Equal("110000", t3[0].Childs[0].Code);
            Assert.Equal(2, t3[0].Childs[0].Childs.Count);
            Assert.Equal("110100", t3[0].Childs[0].Childs[0].Code);
            Assert.Equal("110101", t3[0].Childs[0].Childs[1].Code);

            t3 = fsql.Select<VM_District_Child>().Where(a => a.Name == "中国").AsTreeCte().OrderBy(a => a.Code).ToTreeList();
            Assert.Single(t3);
            Assert.Equal("100000", t3[0].Code);
            Assert.Single(t3[0].Childs);
            Assert.Equal("110000", t3[0].Childs[0].Code);
            Assert.Equal(2, t3[0].Childs[0].Childs.Count);
            Assert.Equal("110100", t3[0].Childs[0].Childs[0].Code);
            Assert.Equal("110101", t3[0].Childs[0].Childs[1].Code);

            t3 = fsql.Select<VM_District_Child>().Where(a => a.Name == "中国").AsTreeCte().OrderBy(a => a.Code).ToList();
            Assert.Equal(4, t3.Count);
            Assert.Equal("100000", t3[0].Code);
            Assert.Equal("110000", t3[1].Code);
            Assert.Equal("110100", t3[2].Code);
            Assert.Equal("110101", t3[3].Code);

            t3 = fsql.Select<VM_District_Child>().Where(a => a.Name == "北京").AsTreeCte().OrderBy(a => a.Code).ToList();
            Assert.Equal(3, t3.Count);
            Assert.Equal("110000", t3[0].Code);
            Assert.Equal("110100", t3[1].Code);
            Assert.Equal("110101", t3[2].Code);

            var t4 = fsql.Select<VM_District_Child>().Where(a => a.Name == "中国").AsTreeCte(a => a.Name).OrderBy(a => a.Code)
                .ToList(a => new { item = a, level = Convert.ToInt32("a.cte_level"), path = "a.cte_path" });
            Assert.Equal(4, t4.Count);
            Assert.Equal("100000", t4[0].item.Code);
            Assert.Equal("110000", t4[1].item.Code);
            Assert.Equal("110100", t4[2].item.Code);
            Assert.Equal("110101", t4[3].item.Code);
            Assert.Equal("中国", t4[0].path);
            Assert.Equal("中国 -> 北京", t4[1].path);
            Assert.Equal("中国 -> 北京 -> 北京市", t4[2].path);
            Assert.Equal("中国 -> 北京 -> 东城区", t4[3].path);

            t4 = fsql.Select<VM_District_Child>().Where(a => a.Name == "中国").AsTreeCte(a => a.Name + "[" + a.Code + "]").OrderBy(a => a.Code)
                .ToList(a => new { item = a, level = Convert.ToInt32("a.cte_level"), path = "a.cte_path" });
            Assert.Equal(4, t4.Count);
            Assert.Equal("100000", t4[0].item.Code);
            Assert.Equal("110000", t4[1].item.Code);
            Assert.Equal("110100", t4[2].item.Code);
            Assert.Equal("110101", t4[3].item.Code);
            Assert.Equal("中国[100000]", t4[0].path);
            Assert.Equal("中国[100000] -> 北京[110000]", t4[1].path);
            Assert.Equal("中国[100000] -> 北京[110000] -> 北京市[110100]", t4[2].path);
            Assert.Equal("中国[100000] -> 北京[110000] -> 东城区[110101]", t4[3].path);

            var select = fsql.Select<VM_District_Child>()
                .Where(a => a.Name == "中国")
                .AsTreeCte()
                //.OrderBy("a.cte_level desc") //递归层级
                ;
            // var list = select.ToList(); //自己调试看查到的数据
            select.ToUpdate().Set(a => a.testint, 855).ExecuteAffrows();
            Assert.Equal(855, fsql.Select<VM_District_Child>()
                .Where(a => a.Name == "中国")
                .AsTreeCte().Distinct().First(a => a.testint));

            Assert.Equal(4, select.ToDelete().ExecuteAffrows());
            Assert.False(fsql.Select<VM_District_Child>()
                .Where(a => a.Name == "中国")
                .AsTreeCte().Any());
        }

        [Table(Name = "D_District")]
        public class BaseDistrict
        {
            [Column(IsPrimary = true, StringLength = 6)]
            public string Code { get; set; }

            [Column(StringLength = 20, IsNullable = false)]
            public string Name { get; set; }

            [Column(StringLength = 6)]
            public virtual string ParentCode { get; set; }

            public int testint { get; set; }
        }
        [Table(Name = "D_District", DisableSyncStructure = true)]
        public class VM_District_Child : BaseDistrict
        {
            public override string ParentCode { get => base.ParentCode; set => base.ParentCode = value; }

            [Navigate(nameof(ParentCode))]
            public List<VM_District_Child> Childs { get; set; }
        }
        [Table(Name = "D_District", DisableSyncStructure = true)]
        public class VM_District_Parent : BaseDistrict
        {
            public override string ParentCode { get => base.ParentCode; set => base.ParentCode = value; }

            [Navigate(nameof(ParentCode))]
            public VM_District_Parent Parent { get; set; }
        }
    }
}
