using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.PostgreSQL {
	public class PostgreSQLSelectTest {

		ISelect<Topic> select => g.pgsql.Select<Topic>();

		[Table(Name = "tb_topic")]
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
		public void ToList() {
			var t1 = g.pgsql.Select<TestInfo>().Where("").Where(a => a.Id > 0).Skip(100).Limit(200).ToSql();
			var t2 = g.pgsql.Select<TestInfo>().As("b").Where("").Where(a => a.Id > 0).Skip(100).Limit(200).ToSql();


			var sql1 = select.LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid).ToSql();
			var sql2 = select.LeftJoin<TestTypeInfo>((a, b) => a.TestTypeInfoGuid == b.Guid && b.Name == "111").ToSql();
			var sql3 = select.LeftJoin("TestTypeInfo b on b.Guid = a.TypeGuid").ToSql();

			//g.pgsql.Select<TestInfo, TestTypeInfo, TestTypeParentInfo>().Join((a, b, c) => new Model.JoinResult3(
			//   Model.JoinType.LeftJoin, a.TypeGuid == b.Guid,
			//   Model.JoinType.InnerJoin, c.Id == b.ParentId && c.Name == "xxx")
			//);

			//var sql4 = select.From<TestTypeInfo, TestTypeParentInfo>((a, b, c) => new SelectFrom()
			//	.InnerJoin(a.TypeGuid == b.Guid)
			//	.LeftJoin(c.Id == b.ParentId)
			//	.Where(b.Name == "xxx"))
			//.Where(a => a.Id == 1).ToSql();

			var sql4 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				.InnerJoin(a => a.TestTypeInfoGuid == b.Guid)
				.LeftJoin(a => c.Id == b.ParentId)
				.Where(a => b.Name == "xxx")).ToSql();
			//.Where(a => a.Id == 1).ToSql();


			var list111 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				.InnerJoin(a => a.TestTypeInfoGuid == b.Guid)
				.LeftJoin(a => c.Id == b.ParentId)
				.Where(a => b.Name != "xxx"));
			var list111sql = list111.ToSql();
			var list111data = list111.ToList((a, b, c) => new {
					a.Id,
					title_substring = a.Title.Substring(0, 1),
					a.Type,
					ccc = new { a.Id, a.Title },
					tp = a.Type,
					tp2 = new {
						a.Id,
						tp2 = a.Type.Name
					},
					tp3 = new {
						a.Id,
						tp33 = new {
							a.Id
						}
					}
				});

			var ttt122 = g.pgsql.Select<TestTypeParentInfo>().Where(a => a.Id > 0).ToSql();
			var sql5 = g.pgsql.Select<TestInfo>().From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s).Where((a, b, c) => a.Id == b.ParentId).ToSql();
			var t11112 = g.pgsql.Select<TestInfo>().ToList(a => new {
				a.Id,
				a.Title,
				a.Type,
				ccc = new { a.Id, a.Title },
				tp = a.Type,
				tp2 = new {
					a.Id,
					tp2 = a.Type.Name
				},
				tp3 = new {
					a.Id,
					tp33 = new {
						a.Id
					}
				}

			});

			var t100 = g.pgsql.Select<TestInfo>().Where("").Where(a => a.Id > 0).Skip(100).Limit(200).Caching(50).ToList();
			var t101 = g.pgsql.Select<TestInfo>().As("b").Where("").Where(a => a.Id > 0).Skip(100).Limit(200).Caching(50).ToList();


			var t1111 = g.pgsql.Select<TestInfo>().ToList(a => new { a.Id, a.Title, a.Type });

			var t2222 = g.pgsql.Select<TestInfo>().ToList(a => new { a.Id, a.Title, a.Type.Name });
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
			var testcaching1 = g.pgsql.Cache.Get("testcaching");
			Assert.NotNull(testcaching1);
			var result2 = select.Where(a => 1 == 1).Caching(20, "testcaching").ToList();
			var testcaching2 = g.pgsql.Cache.Get("testcaching");
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
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeinfo\" b ON a.\"testtypeinfoguid\" = b.\"guid\" LEFT JOIN \"testtypeparentinfo\" c ON b.\"parentid\" = c.\"id\"", sql);
			query2.ToList();
		}
		[Fact]
		public void LeftJoin() {
			//����е�������a.Type��a.Type.Parent ���ǵ�������
			var query = select.LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid);
			var sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeinfo\" a__Type ON a__Type.\"guid\" = a.\"testtypeinfoguid\"", sql);
			query.ToList();

			query = select.LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid && a.Type.Name == "xxx");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeinfo\" a__Type ON a__Type.\"guid\" = a.\"testtypeinfoguid\" AND a__Type.\"name\" = 'xxx'", sql);
			query.ToList();

			query = select.LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid && a.Type.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeparentinfo\" a__Type__Parent ON 1 = 1 LEFT JOIN \"testtypeinfo\" a__Type ON a__Type.\"guid\" = a.\"testtypeinfoguid\" AND a__Type.\"name\" = 'xxx' WHERE (a__Type__Parent.\"id\" = 10)", sql);
			query.ToList();

			//���û�е�������
			query = select.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeinfo\" b ON b.\"guid\" = a.\"testtypeinfoguid\"", sql);
			query.ToList();

			query = select.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "xxx");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeinfo\" b ON b.\"guid\" = a.\"testtypeinfoguid\" AND b.\"name\" = 'xxx'", sql);
			query.ToList();

			query = select.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeparentinfo\" b__Parent ON 1 = 1 LEFT JOIN \"testtypeinfo\" b ON b.\"guid\" = a.\"testtypeinfoguid\" AND b.\"name\" = 'xxx' WHERE (b__Parent.\"id\" = 10)", sql);
			query.ToList();

			//�������
			query = select
				.LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid)
				.LeftJoin(a => a.Type.Parent.Id == a.Type.ParentId);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeinfo\" a__Type ON a__Type.\"guid\" = a.\"testtypeinfoguid\" LEFT JOIN \"testtypeparentinfo\" a__Type__Parent ON a__Type__Parent.\"id\" = a__Type.\"parentid\"", sql);
			query.ToList();

			query = select
				.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid)
				.LeftJoin<TestTypeParentInfo>((a, c) => c.Id == a.Type.ParentId);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeinfo\" b ON b.\"guid\" = a.\"testtypeinfoguid\" LEFT JOIN \"testtypeparentinfo\" c ON c.\"id\" = b.\"parentid\"", sql);
			query.ToList();

			//���û�е�������b��c������ϵ
			var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				 .LeftJoin(a => a.TestTypeInfoGuid == b.Guid)
				 .LeftJoin(a => b.ParentId == c.Id));
			sql = query2.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeinfo\" b ON a.\"testtypeinfoguid\" = b.\"guid\" LEFT JOIN \"testtypeparentinfo\" c ON b.\"parentid\" = c.\"id\"", sql);
			query2.ToList();

			//������϶����㲻��
			query = select.LeftJoin("\"testtypeinfo\" b on b.\"guid\" = a.\"testtypeinfoguid\"");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeinfo\" b on b.\"guid\" = a.\"testtypeinfoguid\"", sql);
			query.ToList();

			query = select.LeftJoin("\"testtypeinfo\" b on b.\"guid\" = a.\"testtypeinfoguid\" and b.\"name\" = @bname", new { bname = "xxx" });
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeinfo\" b on b.\"guid\" = a.\"testtypeinfoguid\" and b.\"name\" = @bname", sql);
			query.ToList();
		}
		[Fact]
		public void InnerJoin() {
			//����е�������a.Type��a.Type.Parent ���ǵ�������
			var query = select.InnerJoin(a => a.Type.Guid == a.TestTypeInfoGuid);
			var sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a INNER JOIN \"testtypeinfo\" a__Type ON a__Type.\"guid\" = a.\"testtypeinfoguid\"", sql);
			query.ToList();

			query = select.InnerJoin(a => a.Type.Guid == a.TestTypeInfoGuid && a.Type.Name == "xxx");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a INNER JOIN \"testtypeinfo\" a__Type ON a__Type.\"guid\" = a.\"testtypeinfoguid\" AND a__Type.\"name\" = 'xxx'", sql);
			query.ToList();

			query = select.InnerJoin(a => a.Type.Guid == a.TestTypeInfoGuid && a.Type.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeparentinfo\" a__Type__Parent ON 1 = 1 INNER JOIN \"testtypeinfo\" a__Type ON a__Type.\"guid\" = a.\"testtypeinfoguid\" AND a__Type.\"name\" = 'xxx' WHERE (a__Type__Parent.\"id\" = 10)", sql);
			query.ToList();

			//���û�е�������
			query = select.InnerJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a INNER JOIN \"testtypeinfo\" b ON b.\"guid\" = a.\"testtypeinfoguid\"", sql);
			query.ToList();

			query = select.InnerJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "xxx");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a INNER JOIN \"testtypeinfo\" b ON b.\"guid\" = a.\"testtypeinfoguid\" AND b.\"name\" = 'xxx'", sql);
			query.ToList();

			query = select.InnerJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeparentinfo\" b__Parent ON 1 = 1 INNER JOIN \"testtypeinfo\" b ON b.\"guid\" = a.\"testtypeinfoguid\" AND b.\"name\" = 'xxx' WHERE (b__Parent.\"id\" = 10)", sql);
			query.ToList();

			//�������
			query = select
				.InnerJoin(a => a.Type.Guid == a.TestTypeInfoGuid)
				.InnerJoin(a => a.Type.Parent.Id == a.Type.ParentId);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a INNER JOIN \"testtypeinfo\" a__Type ON a__Type.\"guid\" = a.\"testtypeinfoguid\" INNER JOIN \"testtypeparentinfo\" a__Type__Parent ON a__Type__Parent.\"id\" = a__Type.\"parentid\"", sql);
			query.ToList();

			query = select
				.InnerJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid)
				.InnerJoin<TestTypeParentInfo>((a, c) => c.Id == a.Type.ParentId);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a INNER JOIN \"testtypeinfo\" b ON b.\"guid\" = a.\"testtypeinfoguid\" INNER JOIN \"testtypeparentinfo\" c ON c.\"id\" = b.\"parentid\"", sql);
			query.ToList();

			//���û�е�������b��c������ϵ
			var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				 .InnerJoin(a => a.TestTypeInfoGuid == b.Guid)
				 .InnerJoin(a => b.ParentId == c.Id));
			sql = query2.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a INNER JOIN \"testtypeinfo\" b ON a.\"testtypeinfoguid\" = b.\"guid\" INNER JOIN \"testtypeparentinfo\" c ON b.\"parentid\" = c.\"id\"", sql);
			query2.ToList();

			//������϶����㲻��
			query = select.InnerJoin("\"testtypeinfo\" b on b.\"guid\" = a.\"testtypeinfoguid\"");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a INNER JOIN \"testtypeinfo\" b on b.\"guid\" = a.\"testtypeinfoguid\"", sql);
			query.ToList();

			query = select.InnerJoin("\"testtypeinfo\" b on b.\"guid\" = a.\"testtypeinfoguid\" and b.\"name\" = @bname", new { bname = "xxx" });
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a INNER JOIN \"testtypeinfo\" b on b.\"guid\" = a.\"testtypeinfoguid\" and b.\"name\" = @bname", sql);
			query.ToList();

		}
		[Fact]
		public void RightJoin() {
			//����е�������a.Type��a.Type.Parent ���ǵ�������
			var query = select.RightJoin(a => a.Type.Guid == a.TestTypeInfoGuid);
			var sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a RIGHT JOIN \"testtypeinfo\" a__Type ON a__Type.\"guid\" = a.\"testtypeinfoguid\"", sql);
			query.ToList();

			query = select.RightJoin(a => a.Type.Guid == a.TestTypeInfoGuid && a.Type.Name == "xxx");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a RIGHT JOIN \"testtypeinfo\" a__Type ON a__Type.\"guid\" = a.\"testtypeinfoguid\" AND a__Type.\"name\" = 'xxx'", sql);
			query.ToList();

			query = select.RightJoin(a => a.Type.Guid == a.TestTypeInfoGuid && a.Type.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeparentinfo\" a__Type__Parent ON 1 = 1 RIGHT JOIN \"testtypeinfo\" a__Type ON a__Type.\"guid\" = a.\"testtypeinfoguid\" AND a__Type.\"name\" = 'xxx' WHERE (a__Type__Parent.\"id\" = 10)", sql);
			query.ToList();

			//���û�е�������
			query = select.RightJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a RIGHT JOIN \"testtypeinfo\" b ON b.\"guid\" = a.\"testtypeinfoguid\"", sql);
			query.ToList();

			query = select.RightJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "xxx");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a RIGHT JOIN \"testtypeinfo\" b ON b.\"guid\" = a.\"testtypeinfoguid\" AND b.\"name\" = 'xxx'", sql);
			query.ToList();

			query = select.RightJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "xxx").Where(a => a.Type.Parent.Id == 10);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a LEFT JOIN \"testtypeparentinfo\" b__Parent ON 1 = 1 RIGHT JOIN \"testtypeinfo\" b ON b.\"guid\" = a.\"testtypeinfoguid\" AND b.\"name\" = 'xxx' WHERE (b__Parent.\"id\" = 10)", sql);
			query.ToList();

			//�������
			query = select
				.RightJoin(a => a.Type.Guid == a.TestTypeInfoGuid)
				.RightJoin(a => a.Type.Parent.Id == a.Type.ParentId);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a RIGHT JOIN \"testtypeinfo\" a__Type ON a__Type.\"guid\" = a.\"testtypeinfoguid\" RIGHT JOIN \"testtypeparentinfo\" a__Type__Parent ON a__Type__Parent.\"id\" = a__Type.\"parentid\"", sql);
			query.ToList();

			query = select
				.RightJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid)
				.RightJoin<TestTypeParentInfo>((a, c) => c.Id == a.Type.ParentId);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a RIGHT JOIN \"testtypeinfo\" b ON b.\"guid\" = a.\"testtypeinfoguid\" RIGHT JOIN \"testtypeparentinfo\" c ON c.\"id\" = b.\"parentid\"", sql);
			query.ToList();

			//���û�е�������b��c������ϵ
			var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				 .RightJoin(a => a.TestTypeInfoGuid == b.Guid)
				 .RightJoin(a => b.ParentId == c.Id));
			sql = query2.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a RIGHT JOIN \"testtypeinfo\" b ON a.\"testtypeinfoguid\" = b.\"guid\" RIGHT JOIN \"testtypeparentinfo\" c ON b.\"parentid\" = c.\"id\"", sql);
			query2.ToList();

			//������϶����㲻��
			query = select.RightJoin("\"testtypeinfo\" b on b.\"guid\" = a.\"testtypeinfoguid\"");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a RIGHT JOIN \"testtypeinfo\" b on b.\"guid\" = a.\"testtypeinfoguid\"", sql);
			query.ToList();

			query = select.RightJoin("\"testtypeinfo\" b on b.\"guid\" = a.\"testtypeinfoguid\" and b.\"name\" = @bname", new { bname = "xxx" });
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a RIGHT JOIN \"testtypeinfo\" b on b.\"guid\" = a.\"testtypeinfoguid\" and b.\"name\" = @bname", sql);
			query.ToList();

		}
		[Fact]
		public void Where() {
			//����е�������a.Type��a.Type.Parent ���ǵ�������
			var query = select.Where(a => a.Id == 10);
			var sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a WHERE (a.\"id\" = 10)", sql);
			query.ToList();

			query = select.Where(a => a.Id == 10 && a.Id > 10 || a.Clicks > 100);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a WHERE (a.\"id\" = 10 AND a.\"id\" > 10 OR a.\"clicks\" > 100)", sql);
			query.ToList();

			query = select.Where(a => a.Id == 10).Where(a => a.Clicks > 100);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a WHERE (a.\"id\" = 10) AND (a.\"clicks\" > 100)", sql);
			query.ToList();

			query = select.Where(a => a.Type.Name == "typeTitle");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a, \"testtypeinfo\" a__Type WHERE (a__Type.\"name\" = 'typeTitle')", sql);
			query.ToList();

			query = select.Where(a => a.Type.Name == "typeTitle" && a.Type.Guid == a.TestTypeInfoGuid);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a, \"testtypeinfo\" a__Type WHERE (a__Type.\"name\" = 'typeTitle' AND a__Type.\"guid\" = a.\"testtypeinfoguid\")", sql);
			query.ToList();

			query = select.Where(a => a.Type.Parent.Name == "tparent");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a, \"testtypeinfo\" a__Type, \"testtypeparentinfo\" a__Type__Parent WHERE (a__Type__Parent.\"name\" = 'tparent')", sql);
			query.ToList();
			
			//���û�е������ԣ��򵥶������
			query = select.Where<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "typeTitle");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a, \"testtypeinfo\" b WHERE (b.\"guid\" = a.\"testtypeinfoguid\" AND b.\"name\" = 'typeTitle')", sql);
			query.ToList();

			query = select.Where<TestTypeInfo>((a, b) => b.Name == "typeTitle" && b.Guid == a.TestTypeInfoGuid);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a, \"testtypeinfo\" b WHERE (b.\"name\" = 'typeTitle' AND b.\"guid\" = a.\"testtypeinfoguid\")", sql);
			query.ToList();

			query = select.Where<TestTypeInfo, TestTypeParentInfo>((a, b, c) => c.Name == "tparent");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a, \"testtypeparentinfo\" c WHERE (c.\"name\" = 'tparent')", sql);
			query.ToList();

			//����һ�� From ��Ķ������
			var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				.Where(a => a.Id == 10 && c.Name == "xxx")
				.Where(a => b.ParentId == 20));
			sql = query2.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a, \"testtypeparentinfo\" c, \"testtypeinfo\" b WHERE (a.\"id\" = 10 AND c.\"name\" = 'xxx') AND (b.\"parentid\" = 20)", sql);
			query2.ToList();

			//������϶����㲻��
			query = select.Where("a.\"clicks\" > 100 and a.\"id\" = @id", new { id = 10 });
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a WHERE (a.\"clicks\" > 100 and a.\"id\" = @id)", sql);
			query.ToList();
		}
		[Fact]
		public void WhereIf() {
			//����е�������a.Type��a.Type.Parent ���ǵ�������
			var query = select.WhereIf(true, a => a.Id == 10);
			var sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a WHERE (a.\"id\" = 10)", sql);
			query.ToList();

			query = select.WhereIf(true, a => a.Id == 10 && a.Id > 10 || a.Clicks > 100);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a WHERE (a.\"id\" = 10 AND a.\"id\" > 10 OR a.\"clicks\" > 100)", sql);
			query.ToList();

			query = select.WhereIf(true, a => a.Id == 10).WhereIf(true, a => a.Clicks > 100);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a WHERE (a.\"id\" = 10) AND (a.\"clicks\" > 100)", sql);
			query.ToList();

			query = select.WhereIf(true, a => a.Type.Name == "typeTitle");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a, \"testtypeinfo\" a__Type WHERE (a__Type.\"name\" = 'typeTitle')", sql);
			query.ToList();

			query = select.WhereIf(true, a => a.Type.Name == "typeTitle" && a.Type.Guid == a.TestTypeInfoGuid);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a, \"testtypeinfo\" a__Type WHERE (a__Type.\"name\" = 'typeTitle' AND a__Type.\"guid\" = a.\"testtypeinfoguid\")", sql);
			query.ToList();

			query = select.WhereIf(true, a => a.Type.Parent.Name == "tparent");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a__Type.\"guid\", a__Type.\"parentid\", a__Type.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a, \"testtypeinfo\" a__Type, \"testtypeparentinfo\" a__Type__Parent WHERE (a__Type__Parent.\"name\" = 'tparent')", sql);
			query.ToList();

			//����һ�� From ��Ķ������
			var query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				.WhereIf(true, a => a.Id == 10 && c.Name == "xxx")
				.WhereIf(true, a => b.ParentId == 20));
			sql = query2.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", b.\"guid\", b.\"parentid\", b.\"name\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a, \"testtypeparentinfo\" c, \"testtypeinfo\" b WHERE (a.\"id\" = 10 AND c.\"name\" = 'xxx') AND (b.\"parentid\" = 20)", sql);
			query2.ToList();

			//������϶����㲻��
			query = select.WhereIf(true, "a.\"clicks\" > 100 and a.\"id\" = @id", new { id = 10 });
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a WHERE (a.\"clicks\" > 100 and a.\"id\" = @id)", sql);
			query.ToList();

			// ==========================================WhereIf(false)

			//����е�������a.Type��a.Type.Parent ���ǵ�������
			query = select.WhereIf(false, a => a.Id == 10);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a", sql);
			query.ToList();

			query = select.WhereIf(false, a => a.Id == 10 && a.Id > 10 || a.Clicks > 100);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a", sql);
			query.ToList();

			query = select.WhereIf(false, a => a.Id == 10).WhereIf(false, a => a.Clicks > 100);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a", sql);
			query.ToList();

			query = select.WhereIf(false, a => a.Type.Name == "typeTitle");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a", sql);
			query.ToList();

			query = select.WhereIf(false, a => a.Type.Name == "typeTitle" && a.Type.Guid == a.TestTypeInfoGuid);
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a", sql);
			query.ToList();

			query = select.WhereIf(false, a => a.Type.Parent.Name == "tparent");
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a", sql);
			query.ToList();

			//����һ�� From ��Ķ������
			query2 = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
				.WhereIf(false, a => a.Id == 10 && c.Name == "xxx")
				.WhereIf(false, a => b.ParentId == 20));
			sql = query2.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a", sql);
			query2.ToList();

			//������϶����㲻��
			query = select.WhereIf(false, "a.\"clicks\" > 100 and a.\"id\" = @id", new { id = 10 });
			sql = query.ToSql().Replace("\r\n", "");
			Assert.Equal("SELECT a.\"id\", a.\"clicks\", a.\"testtypeinfoguid\", a.\"title\", a.\"createtime\" FROM \"tb_topic\" a", sql);
			query.ToList();
		}
		[Fact]
		public void GroupBy() {
		}
		[Fact]
		public void Having() {
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
	}
}
