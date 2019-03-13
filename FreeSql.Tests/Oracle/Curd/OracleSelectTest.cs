using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Oracle {
	public class OracleSelectTest {

		ISelect<Topic> select => g.oracle.Select<Topic>();

		[Table(Name = "tb_topic22")]
		class Topic {
			[Column(IsIdentity = true, IsPrimary = true)]
			public int Id { get; set; }
			public int Clicks { get; set; }
			public int TestTypeInfoGuid { get; set; }
			public TestTypeInfo Type { get; set; }
			public string Title { get; set; }
			public DateTime CreateTime { get; set; }
		}
		class TestTypeInfo {
			public int Guid { get; set; }
			public int ParentId { get; set; }
			public TestTypeParentInfo Parent { get; set; }
			public string Name { get; set; }
		}
		class TestTypeParentInfo {
			public int Id { get; set; }
			public string Name { get; set; }

			public List<TestTypeInfo> Types { get; set; }
		}

		[Fact]
		public void ToDataTable() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

			Assert.Equal(1, g.oracle.Insert<Topic>().AppendData(items.First()).ExecuteAffrows());
			Assert.Equal(10, g.oracle.Insert<Topic>().AppendData(items).ExecuteAffrows());

			items = Enumerable.Range(0, 9989).Select(a => new Topic { Title = "newtitle" + a, CreateTime = DateTime.Now }).ToList();
			Assert.Equal(9989, g.oracle.Insert<Topic>(items).ExecuteAffrows());

			var dt1 = select.Limit(10).ToDataTable();
			var dt2 = select.Limit(10).ToDataTable("id, 111222");
			var dt3 = select.Limit(10).ToDataTable(a => new { a.Id, a.Type.Name, now = DateTime.Now });
		}
		[Fact]
		public void ToList() {
		}
		[Fact]
		public void ToOne() {
		}
		[Fact]
		public void ToSql() {
		}
		[Fact]
		public void Any() {
			var count = select.Where(a => 1 == 1).Count();
			Assert.False(select.Where(a => 1 == 2).Any());
			Assert.Equal(count > 0, select.Where(a => 1 == 1).Any());
		}
		[Fact]
		public void Count() {
			var count = select.Where(a => 1 == 1).Count();
			select.Where(a => 1 == 1).Count(out var count2);
			Assert.Equal(count, count2);
			Assert.Equal(0, select.Where(a => 1 == 2).Count());
		}
		[Fact]
		public void Master() {
			Assert.StartsWith(" SELECT", select.Master().Where(a => 1 == 1).ToSql());
		}
		[Fact]
		public void Caching() {
			var result1 = select.Where(a => 1 == 1).Caching(20, "testcaching").ToList();
			var testcaching1 = g.oracle.Cache.Get("testcaching");
			Assert.NotNull(testcaching1);
			var result2 = select.Where(a => 1 == 1).Caching(20, "testcaching").ToList();
			var testcaching2 = g.oracle.Cache.Get("testcaching");
			Assert.NotNull(testcaching2);
			Assert.Equal(result1.Count, result1.Count);
		}
		[Fact]
		public void From() {
			//�������
			var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				 .LeftJoin(a => a.TestTypeInfoGuid == b.Guid)
				 .LeftJoin(a => b.ParentId == c.Id)
				);
			var sql = query2.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" b ON a.\"TESTTYPEINFOGUID\" = b.\"GUID\" LEFT JOIN \"TESTTYPEPARENTINFO\" c ON b.\"PARENTID\" = c.\"ID\"", sql);
			query2.ToList();
		}
		[Fact]
		public void LeftJoin() {
			//����е�������a.Type��a.Type.Parent ���ǵ�������
			var query = select.LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid);
			var sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\"", sql);
			query.ToList();

			query = select.LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid && a.Type.Name == "xxx");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND a__Type.\"NAME\" = 'xxx'", sql);
			query.ToList();

			query = select.LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid && a.Type.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON 1 = 1 LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND a__Type.\"NAME\" = 'xxx' WHERE (a__Type__Parent.\"ID\" = 10)", sql);
			query.ToList();

			//���û�е�������
			query = select.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\"", sql);
			query.ToList();

			query = select.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "xxx");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND b.\"NAME\" = 'xxx'", sql);
			query.ToList();

			query = select.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEPARENTINFO\" b__Parent ON 1 = 1 LEFT JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND b.\"NAME\" = 'xxx' WHERE (b__Parent.\"ID\" = 10)", sql);
			query.ToList();

			//�������
			query = select
				.LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid)
				.LeftJoin(a => a.Type.Parent.Id == a.Type.ParentId);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\" LEFT JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\"", sql);
			query.ToList();

			query = select
				.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid)
				.LeftJoin<TestTypeParentInfo>((a, c) => c.Id == a.Type.ParentId);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\" LEFT JOIN \"TESTTYPEPARENTINFO\" c ON c.\"ID\" = b.\"PARENTID\"", sql);
			query.ToList();

			//���û�е�������b��c������ϵ
			var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				 .LeftJoin(a => a.TestTypeInfoGuid == b.Guid)
				 .LeftJoin(a => b.ParentId == c.Id));
			sql = query2.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" b ON a.\"TESTTYPEINFOGUID\" = b.\"GUID\" LEFT JOIN \"TESTTYPEPARENTINFO\" c ON b.\"PARENTID\" = c.\"ID\"", sql);
			query2.ToList();

			//������϶����㲻��
			query = select.LeftJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\"");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\"", sql);
			query.ToList();

			query = select.LeftJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\" and b.\"NAME\" = :bname", new { bname = "xxx" });
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\" and b.\"NAME\" = :bname", sql);
			query.ToList();
		}
		[Fact]
		public void InnerJoin() {
			//����е�������a.Type��a.Type.Parent ���ǵ�������
			var query = select.InnerJoin(a => a.Type.Guid == a.TestTypeInfoGuid);
			var sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\"", sql);
			query.ToList();

			query = select.InnerJoin(a => a.Type.Guid == a.TestTypeInfoGuid && a.Type.Name == "xxx");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND a__Type.\"NAME\" = 'xxx'", sql);
			query.ToList();

			query = select.InnerJoin(a => a.Type.Guid == a.TestTypeInfoGuid && a.Type.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON 1 = 1 INNER JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND a__Type.\"NAME\" = 'xxx' WHERE (a__Type__Parent.\"ID\" = 10)", sql);
			query.ToList();

			//���û�е�������
			query = select.InnerJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\"", sql);
			query.ToList();

			query = select.InnerJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "xxx");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND b.\"NAME\" = 'xxx'", sql);
			query.ToList();

			query = select.InnerJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEPARENTINFO\" b__Parent ON 1 = 1 INNER JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND b.\"NAME\" = 'xxx' WHERE (b__Parent.\"ID\" = 10)", sql);
			query.ToList();

			//�������
			query = select
				.InnerJoin(a => a.Type.Guid == a.TestTypeInfoGuid)
				.InnerJoin(a => a.Type.Parent.Id == a.Type.ParentId);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\" INNER JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\"", sql);
			query.ToList();

			query = select
				.InnerJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid)
				.InnerJoin<TestTypeParentInfo>((a, c) => c.Id == a.Type.ParentId);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\" INNER JOIN \"TESTTYPEPARENTINFO\" c ON c.\"ID\" = b.\"PARENTID\"", sql);
			query.ToList();

			//���û�е�������b��c������ϵ
			var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				 .InnerJoin(a => a.TestTypeInfoGuid == b.Guid)
				 .InnerJoin(a => b.ParentId == c.Id));
			sql = query2.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" b ON a.\"TESTTYPEINFOGUID\" = b.\"GUID\" INNER JOIN \"TESTTYPEPARENTINFO\" c ON b.\"PARENTID\" = c.\"ID\"", sql);
			query2.ToList();

			//������϶����㲻��
			query = select.InnerJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\"");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\"", sql);
			query.ToList();

			query = select.InnerJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\" and b.\"NAME\" = :bname", new { bname = "xxx" });
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a INNER JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\" and b.\"NAME\" = :bname", sql);
			query.ToList();

		}
		[Fact]
		public void RightJoin() {
			//����е�������a.Type��a.Type.Parent ���ǵ�������
			var query = select.RightJoin(a => a.Type.Guid == a.TestTypeInfoGuid);
			var sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\"", sql);
			query.ToList();

			query = select.RightJoin(a => a.Type.Guid == a.TestTypeInfoGuid && a.Type.Name == "xxx");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND a__Type.\"NAME\" = 'xxx'", sql);
			query.ToList();

			query = select.RightJoin(a => a.Type.Guid == a.TestTypeInfoGuid && a.Type.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON 1 = 1 RIGHT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND a__Type.\"NAME\" = 'xxx' WHERE (a__Type__Parent.\"ID\" = 10)", sql);
			query.ToList();

			//���û�е�������
			query = select.RightJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\"", sql);
			query.ToList();

			query = select.RightJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "xxx");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND b.\"NAME\" = 'xxx'", sql);
			query.ToList();

			query = select.RightJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a LEFT JOIN \"TESTTYPEPARENTINFO\" b__Parent ON 1 = 1 RIGHT JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND b.\"NAME\" = 'xxx' WHERE (b__Parent.\"ID\" = 10)", sql);
			query.ToList();

			//�������
			query = select
				.RightJoin(a => a.Type.Guid == a.TestTypeInfoGuid)
				.RightJoin(a => a.Type.Parent.Id == a.Type.ParentId);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\" RIGHT JOIN \"TESTTYPEPARENTINFO\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\"", sql);
			query.ToList();

			query = select
				.RightJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid)
				.RightJoin<TestTypeParentInfo>((a, c) => c.Id == a.Type.ParentId);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\" RIGHT JOIN \"TESTTYPEPARENTINFO\" c ON c.\"ID\" = b.\"PARENTID\"", sql);
			query.ToList();

			//���û�е�������b��c������ϵ
			var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				 .RightJoin(a => a.TestTypeInfoGuid == b.Guid)
				 .RightJoin(a => b.ParentId == c.Id));
			sql = query2.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" b ON a.\"TESTTYPEINFOGUID\" = b.\"GUID\" RIGHT JOIN \"TESTTYPEPARENTINFO\" c ON b.\"PARENTID\" = c.\"ID\"", sql);
			query2.ToList();

			//������϶����㲻��
			query = select.RightJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\"");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\"", sql);
			query.ToList();

			query = select.RightJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\" and b.\"NAME\" = :bname", new { bname = "xxx" });
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a RIGHT JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\" and b.\"NAME\" = :bname", sql);
			query.ToList();

		}
		[Fact]
		public void Where() {
			//����е�������a.Type��a.Type.Parent ���ǵ�������
			var query = select.Where(a => a.Id == 10);
			var sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE (a.\"ID\" = 10)", sql);
			query.ToList();

			query = select.Where(a => a.Id == 10 && a.Id > 10 || a.Clicks > 100);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE (a.\"ID\" = 10 AND a.\"ID\" > 10 OR a.\"CLICKS\" > 100)", sql);
			query.ToList();

			query = select.Where(a => a.Id == 10).Where(a => a.Clicks > 100);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE (a.\"ID\" = 10) AND (a.\"CLICKS\" > 100)", sql);
			query.ToList();

			query = select.Where(a => a.Type.Name == "typeTitle");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEINFO\" a__Type WHERE (a__Type.\"NAME\" = 'typeTitle')", sql);
			query.ToList();

			query = select.Where(a => a.Type.Name == "typeTitle" && a.Type.Guid == a.TestTypeInfoGuid);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEINFO\" a__Type WHERE (a__Type.\"NAME\" = 'typeTitle' AND a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\")", sql);
			query.ToList();

			query = select.Where(a => a.Type.Parent.Name == "tparent");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEINFO\" a__Type, \"TESTTYPEPARENTINFO\" a__Type__Parent WHERE (a__Type__Parent.\"NAME\" = 'tparent')", sql);
			query.ToList();
			
			//���û�е������ԣ��򵥶������
			query = select.Where<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "typeTitle");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEINFO\" b WHERE (b.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND b.\"NAME\" = 'typeTitle')", sql);
			query.ToList();

			query = select.Where<TestTypeInfo>((a, b) => b.Name == "typeTitle" && b.Guid == a.TestTypeInfoGuid);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEINFO\" b WHERE (b.\"NAME\" = 'typeTitle' AND b.\"GUID\" = a.\"TESTTYPEINFOGUID\")", sql);
			query.ToList();

			query = select.Where<TestTypeInfo, TestTypeParentInfo>((a, b, c) => c.Name == "tparent");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEPARENTINFO\" c WHERE (c.\"NAME\" = 'tparent')", sql);
			query.ToList();

			//����һ�� From ��Ķ������
			var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				.Where(a => a.Id == 10 && c.Name == "xxx")
				.Where(a => b.ParentId == 20));
			sql = query2.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEPARENTINFO\" c, \"TESTTYPEINFO\" b WHERE (a.\"ID\" = 10 AND c.\"NAME\" = 'xxx') AND (b.\"PARENTID\" = 20)", sql);
			query2.ToList();

			//������϶����㲻��
			query = select.Where("a.\"CLICKS\" > 100 and a.\"ID\" = :id", new { id = 10 });
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE (a.\"CLICKS\" > 100 and a.\"ID\" = :id)", sql);
			query.ToList();
		}
		[Fact]
		public void WhereIf() {
			//����е�������a.Type��a.Type.Parent ���ǵ�������
			var query = select.WhereIf(true, a => a.Id == 10);
			var sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE (a.\"ID\" = 10)", sql);
			query.ToList();

			query = select.WhereIf(true, a => a.Id == 10 && a.Id > 10 || a.Clicks > 100);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE (a.\"ID\" = 10 AND a.\"ID\" > 10 OR a.\"CLICKS\" > 100)", sql);
			query.ToList();

			query = select.WhereIf(true, a => a.Id == 10).WhereIf(true, a => a.Clicks > 100);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE (a.\"ID\" = 10) AND (a.\"CLICKS\" > 100)", sql);
			query.ToList();

			query = select.WhereIf(true, a => a.Type.Name == "typeTitle");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEINFO\" a__Type WHERE (a__Type.\"NAME\" = 'typeTitle')", sql);
			query.ToList();

			query = select.WhereIf(true, a => a.Type.Name == "typeTitle" && a.Type.Guid == a.TestTypeInfoGuid);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEINFO\" a__Type WHERE (a__Type.\"NAME\" = 'typeTitle' AND a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\")", sql);
			query.ToList();

			query = select.WhereIf(true, a => a.Type.Parent.Name == "tparent");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEINFO\" a__Type, \"TESTTYPEPARENTINFO\" a__Type__Parent WHERE (a__Type__Parent.\"NAME\" = 'tparent')", sql);
			query.ToList();

			//����һ�� From ��Ķ������
			var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				.WhereIf(true, a => a.Id == 10 && c.Name == "xxx")
				.WhereIf(true, a => b.ParentId == 20));
			sql = query2.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a, \"TESTTYPEPARENTINFO\" c, \"TESTTYPEINFO\" b WHERE (a.\"ID\" = 10 AND c.\"NAME\" = 'xxx') AND (b.\"PARENTID\" = 20)", sql);
			query2.ToList();

			//������϶����㲻��
			query = select.WhereIf(true, "a.\"CLICKS\" > 100 and a.\"ID\" = :id", new { id = 10 });
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a WHERE (a.\"CLICKS\" > 100 and a.\"ID\" = :id)", sql);
			query.ToList();

			// ==========================================WhereIf(false)

			//����е�������a.Type��a.Type.Parent ���ǵ�������
			query = select.WhereIf(false, a => a.Id == 10);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a", sql);
			query.ToList();

			query = select.WhereIf(false, a => a.Id == 10 && a.Id > 10 || a.Clicks > 100);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a", sql);
			query.ToList();

			query = select.WhereIf(false, a => a.Id == 10).WhereIf(false, a => a.Clicks > 100);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a", sql);
			query.ToList();

			query = select.WhereIf(false, a => a.Type.Name == "typeTitle");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a", sql);
			query.ToList();

			query = select.WhereIf(false, a => a.Type.Name == "typeTitle" && a.Type.Guid == a.TestTypeInfoGuid);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a", sql);
			query.ToList();

			query = select.WhereIf(false, a => a.Type.Parent.Name == "tparent");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a", sql);
			query.ToList();

			//����һ�� From ��Ķ������
			query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				.WhereIf(false, a => a.Id == 10 && c.Name == "xxx")
				.WhereIf(false, a => b.ParentId == 20));
			sql = query2.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a", sql);
			query2.ToList();

			//������϶����㲻��
			query = select.WhereIf(false, "a.\"CLICKS\" > 100 and a.\"ID\" = :id", new { id = 10 });
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22\" a", sql);
			query.ToList();
		}
		[Fact]
		public void WhereExists() {
			var sql2222 = select.Where(a => select.Where(b => b.Id == a.Id).Any()).ToList();

			sql2222 = select.Where(a =>
				select.Where(b => b.Id == a.Id && select.Where(c => c.Id == b.Id).Where(d => d.Id == a.Id).Where(e => e.Id == b.Id)

				.Offset(a.Id)

				.Any()
				).Any()
			).ToList();
		}
		[Fact]
		public void GroupBy() {
			var groupby = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				.Where(a => a.Id == 1)
			)
			.GroupBy((a, b, c) => new { tt2 = a.Title.Substring(0, 2), mod4 = a.Id % 4 })
			.Having(a => a.Count() > 0 && a.Avg(a.Key.mod4) > 0 && a.Max(a.Key.mod4) > 0)
			.Having(a => a.Count() < 300 || a.Avg(a.Key.mod4) < 100)
			.OrderBy(a => a.Key.tt2)
			.OrderByDescending(a => a.Count())
			.ToList(a => new {
				a.Key.tt2,
				cou1 = a.Count(),
				arg1 = a.Avg(a.Key.mod4),
				ccc2 = a.Key.tt2 ?? "now()",
				//ccc = Convert.ToDateTime("now()"), partby = Convert.ToDecimal("sum(num) over(PARTITION BY server_id,os,rid,chn order by id desc)")
			});
		}
		[Fact]
		public void ToAggregate() {
			var sql = select.ToAggregate(a => new { sum = a.Sum(a.Key.Id + 11.11), avg = a.Avg(a.Key.Id), count = a.Count(), max = a.Max(a.Key.Id), min = a.Min(a.Key.Id) });
		}
		[Fact]
		public void OrderBy() {
		}
		[Fact]
		public void Skip_Offset() {
		}
		[Fact]
		public void Take_Limit() {
		}
		[Fact]
		public void Page() {
		}
		[Fact]
		public void Sum() {
		}
		[Fact]
		public void Min() {
		}
		[Fact]
		public void Max() {
		}
		[Fact]
		public void Avg() {
		}
		[Fact]
		public void As() {
		}

		[Fact]
		public void AsTable() {
			Func<Type, string, string> tableRule = (type, oldname) => {
				if (type == typeof(Topic)) return oldname + "AsTable1";
				else if (type == typeof(TestTypeInfo)) return oldname + "AsTable2";
				return oldname + "AsTable";
			};

			//����е�������a.Type��a.Type.Parent ���ǵ�������
			var query = select.LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid).AsTable(tableRule);
			var sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22AsTable1\" a LEFT JOIN \"TESTTYPEINFOAsTable2\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\"", sql);

			query = select.LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid && a.Type.Name == "xxx").AsTable(tableRule);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22AsTable1\" a LEFT JOIN \"TESTTYPEINFOAsTable2\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND a__Type.\"NAME\" = 'xxx'", sql);

			query = select.LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid && a.Type.Name == "xxx").Where(a => a.Type.Parent.Id == 10).AsTable(tableRule);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22AsTable1\" a LEFT JOIN \"TESTTYPEPARENTINFOAsTable\" a__Type__Parent ON 1 = 1 LEFT JOIN \"TESTTYPEINFOAsTable2\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND a__Type.\"NAME\" = 'xxx' WHERE (a__Type__Parent.\"ID\" = 10)", sql);

			//���û�е�������
			query = select.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid).AsTable(tableRule);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22AsTable1\" a LEFT JOIN \"TESTTYPEINFOAsTable2\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\"", sql);

			query = select.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "xxx").AsTable(tableRule);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22AsTable1\" a LEFT JOIN \"TESTTYPEINFOAsTable2\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND b.\"NAME\" = 'xxx'", sql);

			query = select.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "xxx").Where(a => a.Type.Parent.Id == 10).AsTable(tableRule);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22AsTable1\" a LEFT JOIN \"TESTTYPEPARENTINFOAsTable\" b__Parent ON 1 = 1 LEFT JOIN \"TESTTYPEINFOAsTable2\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\" AND b.\"NAME\" = 'xxx' WHERE (b__Parent.\"ID\" = 10)", sql);

			//�������
			query = select
				.LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid)
				.LeftJoin(a => a.Type.Parent.Id == a.Type.ParentId).AsTable(tableRule);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a__Type.\"GUID\", a__Type.\"PARENTID\", a__Type.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22AsTable1\" a LEFT JOIN \"TESTTYPEINFOAsTable2\" a__Type ON a__Type.\"GUID\" = a.\"TESTTYPEINFOGUID\" LEFT JOIN \"TESTTYPEPARENTINFOAsTable\" a__Type__Parent ON a__Type__Parent.\"ID\" = a__Type.\"PARENTID\"", sql);

			query = select
				.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid)
				.LeftJoin<TestTypeParentInfo>((a, c) => c.Id == a.Type.ParentId).AsTable(tableRule);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22AsTable1\" a LEFT JOIN \"TESTTYPEINFOAsTable2\" b ON b.\"GUID\" = a.\"TESTTYPEINFOGUID\" LEFT JOIN \"TESTTYPEPARENTINFOAsTable\" c ON c.\"ID\" = b.\"PARENTID\"", sql);

			//���û�е�������b��c������ϵ
			var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				 .LeftJoin(a => a.TestTypeInfoGuid == b.Guid)
				 .LeftJoin(a => b.ParentId == c.Id)).AsTable(tableRule);
			sql = query2.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", b.\"GUID\", b.\"PARENTID\", b.\"NAME\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22AsTable1\" a LEFT JOIN \"TESTTYPEINFOAsTable2\" b ON a.\"TESTTYPEINFOGUID\" = b.\"GUID\" LEFT JOIN \"TESTTYPEPARENTINFOAsTable\" c ON b.\"PARENTID\" = c.\"ID\"", sql);

			//������϶����㲻��
			query = select.LeftJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\"").AsTable(tableRule);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22AsTable1\" a LEFT JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\"", sql);

			query = select.LeftJoin("\"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\" and b.\"NAME\" = :bname", new { bname = "xxx" }).AsTable(tableRule);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"ID\", a.\"CLICKS\", a.\"TESTTYPEINFOGUID\", a.\"TITLE\", a.\"CREATETIME\" FROM \"TB_TOPIC22AsTable1\" a LEFT JOIN \"TESTTYPEINFO\" b on b.\"GUID\" = a.\"TESTTYPEINFOGUID\" and b.\"NAME\" = :bname", sql);
		}
	}
}
