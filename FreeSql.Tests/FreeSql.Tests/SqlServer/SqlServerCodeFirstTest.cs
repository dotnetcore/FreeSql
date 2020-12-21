using FreeSql.DataAnnotations;
using FreeSql.Tests.DataContext.SqlServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Xunit;

namespace FreeSql.Tests.SqlServer
{
    public class SqlServerCodeFirstTest
    {
        [Fact]
        public void GeographyCrud()
        {
            var fsql = g.sqlserver;
            fsql.Delete<ts_geocrud01>().Where("1=1").ExecuteAffrows();
            var id1 = Guid.NewGuid();
            var geo1 = "LINESTRING (-122.36 47.656, -122.343 47.656)";
            Assert.Equal(1, fsql.Insert(new ts_geocrud01 { id = id1, geo = geo1 }).ExecuteAffrows());
            var item1 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First();
            Assert.NotNull(item1);
            Assert.Equal(id1, item1.id);
            Assert.Equal(geo1, item1.geo);

            //NoneParameter
            fsql.Delete<ts_geocrud01>().Where("1=1").ExecuteAffrows();
            id1 = Guid.NewGuid();
            geo1 = "LINESTRING (-122.36 47.656, -122.343 47.656)";
            Assert.Equal(1, fsql.Insert(new ts_geocrud01 { id = id1, geo = geo1 }).NoneParameter().ExecuteAffrows());
            item1 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First();
            Assert.NotNull(item1);
            Assert.Equal(id1, item1.id);
            Assert.Equal(geo1, item1.geo);

            var item2 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => a.geo);
            Assert.Equal(geo1, item2);
            var item3 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new { a.geo });
            Assert.Equal(geo1, item3.geo);
            var item4 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new ts_geocrud01 { geo = a.geo });
            Assert.Equal(geo1, item4.geo);
            var item5 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).ToList<ts_geocurd01_dto1>().FirstOrDefault();
            Assert.Equal(geo1, item5.geo);
            var item6 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new ts_geocurd01_dto1 { geo = a.geo });
            Assert.Equal(geo1, item6.geo);

            //Update SetSource
            geo1 = "COMPOUNDCURVE (CIRCULARSTRING (-122.358 47.653, -122.348 47.649, -122.348 47.658), CIRCULARSTRING (-122.348 47.658, -122.358 47.658, -122.358 47.653))";
            item1.geo = geo1;
            Assert.Equal(1, fsql.Update<ts_geocrud01>().SetSource(item1).ExecuteAffrows());
            item1 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First();
            Assert.NotNull(item1);
            Assert.Equal(id1, item1.id);
            Assert.Equal(geo1, item1.geo);

            item2 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => a.geo);
            Assert.Equal(geo1, item2);
            item3 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new { a.geo });
            Assert.Equal(geo1, item3.geo);
            item4 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new ts_geocrud01 { geo = a.geo });
            Assert.Equal(geo1, item4.geo);
            item5 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).ToList<ts_geocurd01_dto1>().FirstOrDefault();
            Assert.Equal(geo1, item5.geo);
            item6 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new ts_geocurd01_dto1 { geo = a.geo });
            Assert.Equal(geo1, item6.geo);

            //Update SetSource NoneParameter
            geo1 = "COMPOUNDCURVE (CIRCULARSTRING (-122.358 47.653, -122.348 47.649, -122.348 47.658), CIRCULARSTRING (-122.348 47.658, -122.358 47.658, -122.358 47.653))";
            item1.geo = geo1;
            Assert.Equal(1, fsql.Update<ts_geocrud01>().SetSource(item1).NoneParameter().ExecuteAffrows());
            item1 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First();
            Assert.NotNull(item1);
            Assert.Equal(id1, item1.id);
            Assert.Equal(geo1, item1.geo);

            item2 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => a.geo);
            Assert.Equal(geo1, item2);
            item3 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new { a.geo });
            Assert.Equal(geo1, item3.geo);
            item4 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new ts_geocrud01 { geo = a.geo });
            Assert.Equal(geo1, item4.geo);
            item5 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).ToList<ts_geocurd01_dto1>().FirstOrDefault();
            Assert.Equal(geo1, item5.geo);
            item6 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new ts_geocurd01_dto1 { geo = a.geo });
            Assert.Equal(geo1, item6.geo);

            //Update Set
            geo1 = "POLYGON ((-122.358 47.653, -122.348 47.649, -122.348 47.658, -122.358 47.658, -122.358 47.653))";
            Assert.Equal(1, fsql.Update<ts_geocrud01>().Where(a => a.id == id1).Set(a => a.geo, geo1).ExecuteAffrows());
            item1 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First();
            Assert.NotNull(item1);
            Assert.Equal(id1, item1.id);
            Assert.Equal(geo1, item1.geo);

            item2 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => a.geo);
            Assert.Equal(geo1, item2);
            item3 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new { a.geo });
            Assert.Equal(geo1, item3.geo);
            item4 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new ts_geocrud01 { geo = a.geo });
            Assert.Equal(geo1, item4.geo);
            item5 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).ToList<ts_geocurd01_dto1>().FirstOrDefault();
            Assert.Equal(geo1, item5.geo);
            item6 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new ts_geocurd01_dto1 { geo = a.geo });
            Assert.Equal(geo1, item6.geo);

            //Update Set NoneParameter
            geo1 = "POLYGON ((-122.358 47.653, -122.348 47.649, -122.348 47.658, -122.358 47.658, -122.358 47.653))";
            Assert.Equal(1, fsql.Update<ts_geocrud01>().NoneParameter().Where(a => a.id == id1).Set(a => a.geo, geo1).ExecuteAffrows());
            item1 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First();
            Assert.NotNull(item1);
            Assert.Equal(id1, item1.id);
            Assert.Equal(geo1, item1.geo);

            item2 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => a.geo);
            Assert.Equal(geo1, item2);
            item3 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new { a.geo });
            Assert.Equal(geo1, item3.geo);
            item4 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new ts_geocrud01 { geo = a.geo });
            Assert.Equal(geo1, item4.geo);
            item5 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).ToList<ts_geocurd01_dto1>().FirstOrDefault();
            Assert.Equal(geo1, item5.geo);
            item6 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new ts_geocurd01_dto1 { geo = a.geo });
            Assert.Equal(geo1, item6.geo);

            //Update Set Multi
            geo1 = "LINESTRING (-122.36 47.656, -122.343 47.656)";
            Assert.Equal(1, fsql.Update<ts_geocrud01>().Where(a => a.id == id1).Set(a => new
            {
                geo = geo1
            }).ExecuteAffrows());
            item1 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First();
            Assert.NotNull(item1);
            Assert.Equal(id1, item1.id);
            Assert.Equal(geo1, item1.geo);

            item2 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => a.geo);
            Assert.Equal(geo1, item2);
            item3 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new { a.geo });
            Assert.Equal(geo1, item3.geo);
            item4 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new ts_geocrud01 { geo = a.geo });
            Assert.Equal(geo1, item4.geo);
            item5 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).ToList<ts_geocurd01_dto1>().FirstOrDefault();
            Assert.Equal(geo1, item5.geo);
            item6 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new ts_geocurd01_dto1 { geo = a.geo });
            Assert.Equal(geo1, item6.geo);

            //Update Set Multi NoneParameter
            geo1 = "LINESTRING (-122.36 47.656, -122.343 47.656)";
            Assert.Equal(1, fsql.Update<ts_geocrud01>().NoneParameter().Where(a => a.id == id1).Set(a => new
            {
                geo = geo1
            }).ExecuteAffrows());
            item1 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First();
            Assert.NotNull(item1);
            Assert.Equal(id1, item1.id);
            Assert.Equal(geo1, item1.geo);

            item2 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => a.geo);
            Assert.Equal(geo1, item2);
            item3 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new { a.geo });
            Assert.Equal(geo1, item3.geo);
            item4 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new ts_geocrud01 { geo = a.geo });
            Assert.Equal(geo1, item4.geo);
            item5 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).ToList<ts_geocurd01_dto1>().FirstOrDefault();
            Assert.Equal(geo1, item5.geo);
            item6 = fsql.Select<ts_geocrud01>().Where(a => a.id == id1).First(a => new ts_geocurd01_dto1 { geo = a.geo });
            Assert.Equal(geo1, item6.geo);

            //批量
            fsql.Delete<ts_geocrud01>().Where("1=1").ExecuteAffrows();
            id1 = Guid.NewGuid();
            geo1 = "LINESTRING (-122.36 47.656, -122.343 47.656)";
            var id2 = Guid.NewGuid();
            var geo2 = "POLYGON ((-122.358 47.653, -122.348 47.649, -122.348 47.658, -122.358 47.658, -122.358 47.653))";
            Assert.Equal(2, fsql.Insert(new[] { new ts_geocrud01 { id = id1, geo = geo1 }, new ts_geocrud01 { id = id2, geo = geo2 } }).ExecuteAffrows());
            var items = fsql.Select<ts_geocrud01>().ToList();
            Assert.Equal(2, items.Count);

            items[0].geo = "POLYGON EMPTY";
            items[1].geo = "LINESTRING (0 0, 2 2, 1 0)";
            Assert.Equal(2, fsql.Update<ts_geocrud01>().SetSource(items).ExecuteAffrows());
            item1 = fsql.Select<ts_geocrud01>().Where(a => a.id == items[0].id).First();
            Assert.NotNull(item1);
            Assert.Equal(items[0].id, item1.id);
            Assert.Equal(items[0].geo, item1.geo);
            item1 = fsql.Select<ts_geocrud01>().Where(a => a.id == items[1].id).First();
            Assert.NotNull(item1);
            Assert.Equal(items[1].id, item1.id);
            Assert.Equal(items[1].geo, item1.geo);

            //批量 NoneParameter
            fsql.Delete<ts_geocrud01>().Where("1=1").ExecuteAffrows();
            id1 = Guid.NewGuid();
            geo1 = "LINESTRING (-122.36 47.656, -122.343 47.656)";
            id2 = Guid.NewGuid();
            geo2 = "POLYGON ((-122.358 47.653, -122.348 47.649, -122.348 47.658, -122.358 47.658, -122.358 47.653))";
            Assert.Equal(2, fsql.Insert(new[] { new ts_geocrud01 { id = id1, geo = geo1 }, new ts_geocrud01 { id = id2, geo = geo2 } }).NoneParameter().ExecuteAffrows());
            items = fsql.Select<ts_geocrud01>().ToList();
            Assert.Equal(2, items.Count);

            items[0].geo = "POLYGON EMPTY";
            items[1].geo = "LINESTRING (0 0, 2 2, 1 0)";
            Assert.Equal(2, fsql.Update<ts_geocrud01>().NoneParameter().SetSource(items).ExecuteAffrows());
            item1 = fsql.Select<ts_geocrud01>().Where(a => a.id == items[0].id).First();
            Assert.NotNull(item1);
            Assert.Equal(items[0].id, item1.id);
            Assert.Equal(items[0].geo, item1.geo);
            item1 = fsql.Select<ts_geocrud01>().Where(a => a.id == items[1].id).First();
            Assert.NotNull(item1);
            Assert.Equal(items[1].id, item1.id);
            Assert.Equal(items[1].geo, item1.geo);
        }
        class ts_geocrud01
        {
            public Guid id { get; set; }
            [Column(DbType = "geography", RewriteSql = "geography::STGeomFromText({0},4236)", RereadSql = "{0}.STAsText()")]
            public string geo { get; set; }
        }
        class ts_geocurd01_dto1
        {
            public string geo { get; set; }
        }

        [Fact]
        public void InsertUpdateParameter()
        {
            var fsql = g.sqlserver;
            fsql.CodeFirst.SyncStructure<ts_iupstr_bak>();
            var item = new ts_iupstr { id = Guid.NewGuid(), title = string.Join(",", Enumerable.Range(0, 2000).Select(a => "我是中国人")) };
            Assert.Equal(1, fsql.Insert(item).ExecuteAffrows());
            var find = fsql.Select<ts_iupstr>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(find.id, item.id);
            Assert.Equal(find.title, item.title);
        }
        [Table(Name = "ts_iupstr_bak", DisableSyncStructure = true)]
        class ts_iupstr
        {
            public Guid id { get; set; }
            public string title { get; set; }
        }
        class ts_iupstr_bak
        {
            public Guid id { get; set; }
            [Column(StringLength = -1)]
            public string title { get; set; }
        }

        [Fact] 
        public void EnumStartValue1()
        {
            var fsql = g.sqlserver;
            fsql.Delete<TS_ESV1>().Where("1=1").ExecuteAffrows();

            var repo = fsql.GetRepository<TS_ESV1>();
            var item1 = repo.Insert(new TS_ESV1 { Status = TS_TSV1_Status.Status1 });
            Assert.True(fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).Any());
            Assert.Equal(TS_TSV1_Status.Status1, fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).First().Status);
            Assert.Equal(TS_TSV1_Status.Status1, fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).First(a => a.Status));
            Assert.True(repo.Select.Where(a => a.Id == item1.Id).Any());
            Assert.Equal(TS_TSV1_Status.Status1, repo.Select.Where(a => a.Id == item1.Id).First().Status);
            Assert.Equal(TS_TSV1_Status.Status1, repo.Select.Where(a => a.Id == item1.Id).First(a => a.Status));

            Assert.Equal($"UPDATE [TS_ESV1] SET [Status] = 1 WHERE ([Id] = '{item1.Id}')", fsql.Update<TS_ESV1>().Where(a => a.Id == item1.Id).NoneParameter().Set(a => a.Status, TS_TSV1_Status.Status1).ToSql().Replace("\r\n", ""));
            Assert.Equal($"UPDATE [TS_ESV1] SET [Status] = 3 WHERE ([Id] = '{item1.Id}')", fsql.Update<TS_ESV1>().Where(a => a.Id == item1.Id).NoneParameter().Set(a => a.Status, TS_TSV1_Status.Status2).ToSql().Replace("\r\n", ""));
            Assert.Equal($"UPDATE [TS_ESV1] SET [Status] = 5 WHERE ([Id] = '{item1.Id}')", fsql.Update<TS_ESV1>().Where(a => a.Id == item1.Id).NoneParameter().Set(a => a.Status, TS_TSV1_Status.Status3).ToSql().Replace("\r\n", ""));
            Assert.Equal($"UPDATE [TS_ESV1] SET [Status] = 1 WHERE ([Id] = '{item1.Id}')", fsql.Update<TS_ESV1>().Where(a => a.Id == item1.Id).NoneParameter().Set(a => a.Status == TS_TSV1_Status.Status1).ToSql().Replace("\r\n", ""));
            Assert.Equal($"UPDATE [TS_ESV1] SET [Status] = 3 WHERE ([Id] = '{item1.Id}')", fsql.Update<TS_ESV1>().Where(a => a.Id == item1.Id).NoneParameter().Set(a => a.Status == TS_TSV1_Status.Status2).ToSql().Replace("\r\n", ""));
            Assert.Equal($"UPDATE [TS_ESV1] SET [Status] = 5 WHERE ([Id] = '{item1.Id}')", fsql.Update<TS_ESV1>().Where(a => a.Id == item1.Id).NoneParameter().Set(a => a.Status == TS_TSV1_Status.Status3).ToSql().Replace("\r\n", ""));

            item1.Status = TS_TSV1_Status.Status1;
            repo.Update(item1);
            Assert.True(fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).Any());
            Assert.Equal(TS_TSV1_Status.Status1, fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).First().Status);
            Assert.Equal(TS_TSV1_Status.Status1, fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).First(a => a.Status));
            Assert.True(repo.Select.Where(a => a.Id == item1.Id).Any());
            Assert.Equal(TS_TSV1_Status.Status1, repo.Select.Where(a => a.Id == item1.Id).First().Status);
            Assert.Equal(TS_TSV1_Status.Status1, repo.Select.Where(a => a.Id == item1.Id).First(a => a.Status));

            item1.Status = TS_TSV1_Status.Status2;
            repo.Update(item1);
            Assert.True(fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).Any());
            Assert.Equal(TS_TSV1_Status.Status2, fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).First().Status);
            Assert.Equal(TS_TSV1_Status.Status2, fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).First(a => a.Status));
            Assert.True(repo.Select.Where(a => a.Id == item1.Id).Any());
            Assert.Equal(TS_TSV1_Status.Status2, repo.Select.Where(a => a.Id == item1.Id).First().Status);
            Assert.Equal(TS_TSV1_Status.Status2, repo.Select.Where(a => a.Id == item1.Id).First(a => a.Status));

            item1.Status = TS_TSV1_Status.Status3;
            repo.Update(item1);
            Assert.True(fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).Any());
            Assert.Equal(TS_TSV1_Status.Status3, fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).First().Status);
            Assert.Equal(TS_TSV1_Status.Status3, fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).First(a => a.Status));
            Assert.True(repo.Select.Where(a => a.Id == item1.Id).Any());
            Assert.Equal(TS_TSV1_Status.Status3, repo.Select.Where(a => a.Id == item1.Id).First().Status);
            Assert.Equal(TS_TSV1_Status.Status3, repo.Select.Where(a => a.Id == item1.Id).First(a => a.Status));

            item1.Status = TS_TSV1_Status.Status3;
            fsql.GetRepository<TS_ESV1>().Update(item1);
            Assert.True(fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).Any());
            Assert.Equal(TS_TSV1_Status.Status3, fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).First().Status);
            Assert.Equal(TS_TSV1_Status.Status3, fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).First(a => a.Status));
            Assert.True(repo.Select.Where(a => a.Id == item1.Id).Any());
            Assert.Equal(TS_TSV1_Status.Status3, repo.Select.Where(a => a.Id == item1.Id).First().Status);
            Assert.Equal(TS_TSV1_Status.Status3, repo.Select.Where(a => a.Id == item1.Id).First(a => a.Status));

            item1.Status = TS_TSV1_Status.Status2;
            fsql.GetRepository<TS_ESV1>().Update(item1);
            Assert.True(fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).Any());
            Assert.Equal(TS_TSV1_Status.Status2, fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).First().Status);
            Assert.Equal(TS_TSV1_Status.Status2, fsql.Select<TS_ESV1>().Where(a => a.Id == item1.Id).First(a => a.Status));
            Assert.True(repo.Select.Where(a => a.Id == item1.Id).Any());
            Assert.Equal(TS_TSV1_Status.Status2, repo.Select.Where(a => a.Id == item1.Id).First().Status);
            Assert.Equal(TS_TSV1_Status.Status2, repo.Select.Where(a => a.Id == item1.Id).First(a => a.Status));
        }
        public class TS_ESV1
        {
            public Guid Id { get; set; }
            public TS_TSV1_Status Status { get; set; }
        }
        public enum TS_TSV1_Status
        {
            Status1 = 1,
            Status2 = 3,
            Status3 = 5
        }

        [Fact]
        public void Blob()
        {
            var str1 = string.Join(",", Enumerable.Range(0, 10000).Select(a => "我是中国人"));
            var data1 = Encoding.UTF8.GetBytes(str1);

            var item1 = new TS_BLB01 { Data = data1 };
            Assert.Equal(1, g.sqlserver.Insert(item1).ExecuteAffrows());

            var item2 = g.sqlserver.Select<TS_BLB01>().Where(a => a.Id == item1.Id).First();
            Assert.Equal(item1.Data.Length, item2.Data.Length);

            var str2 = Encoding.UTF8.GetString(item2.Data);
            Assert.Equal(str1, str2);

            //NoneParameter
            item1 = new TS_BLB01 { Data = data1 };
            Assert.Equal(1, g.sqlserver.Insert<TS_BLB01>().NoneParameter().AppendData(item1).ExecuteAffrows());

            item2 = g.sqlserver.Select<TS_BLB01>().Where(a => a.Id == item1.Id).First();
            Assert.Equal(item1.Data.Length, item2.Data.Length);

            str2 = Encoding.UTF8.GetString(item2.Data);
            Assert.Equal(str1, str2);
        }
        class TS_BLB01
        {
            public Guid Id { get; set; }
            [MaxLength(-1)]
            public byte[] Data { get; set; }
        }

        [Fact]
        public void StringLength()
        {
            var dll = g.sqlserver.CodeFirst.GetComparisonDDLStatements<TS_SLTB>();
            g.sqlserver.CodeFirst.SyncStructure<TS_SLTB>();
        }
        class TS_SLTB
        {
            public Guid Id { get; set; }
            [Column(StringLength = 50)]
            public string Title { get; set; }

            [Column(IsNullable = false, StringLength = 50)]
            public string TitleSub { get; set; }
        }

        [Fact]
        public void 表名中有点()
        {
            var item = new tbdot01 { name = "insert" };
            g.sqlserver.Insert(item).ExecuteAffrows();

            var find = g.sqlserver.Select<tbdot01>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal("insert", find.name);

            Assert.Equal(1, g.sqlserver.Update<tbdot01>().Set(a => a.name == "update").Where(a => a.id == item.id).ExecuteAffrows());
            find = g.sqlserver.Select<tbdot01>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal("update", find.name);

            Assert.Equal(1, g.sqlserver.Delete<tbdot01>().Where(a => a.id == item.id).ExecuteAffrows());
            find = g.sqlserver.Select<tbdot01>().Where(a => a.id == item.id).First();
            Assert.Null(find);
        }
        /// <summary>
        /// 表中带点
        /// </summary>
        [Table(Name = "[freesql.T].[dbo].[sys.tbdot01]")]
        class tbdot01
        {
            /// <summary>
            /// 主键
            /// </summary>
            public Guid id { get; set; }
            public string name { get; set; }
        }

        [Fact]
        public void 中文表_字段()
        {
            var sql = g.sqlserver.CodeFirst.GetComparisonDDLStatements<测试中文表>();
            g.sqlserver.CodeFirst.SyncStructure<测试中文表>();

            var item = new 测试中文表
            {
                标题 = "测试标题",
                创建时间 = DateTime.Now
            };
            Assert.Equal(1, g.sqlserver.Insert<测试中文表>().AppendData(item).ExecuteAffrows());
            Assert.NotEqual(Guid.Empty, item.编号);
            var item2 = g.sqlserver.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新";
            Assert.Equal(1, g.sqlserver.Update<测试中文表>().SetSource(item).ExecuteAffrows());
            item2 = g.sqlserver.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo";
            var repo = g.sqlserver.GetRepository<测试中文表>();
            Assert.Equal(1, repo.Update(item));
            item2 = g.sqlserver.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo22";
            Assert.Equal(1, repo.Update(item));
            item2 = g.sqlserver.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);
        }
        class 测试中文表
        {
            [Column(IsPrimary = true)]
            public Guid 编号 { get; set; }

            public string 标题 { get; set; }

            [Column(ServerTime = DateTimeKind.Local, CanUpdate = false)]
            public DateTime 创建时间 { get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime 更新时间 { get; set; }
        }


        [Fact]
        public void AddUniques()
        {
            var sql = g.sqlserver.CodeFirst.GetComparisonDDLStatements<AddUniquesInfo>();
            g.sqlserver.CodeFirst.SyncStructure<AddUniquesInfo>();
            g.sqlserver.CodeFirst.SyncStructure(typeof(AddUniquesInfo), "AddUniquesInfo1");
        }
        [Table(Name = "AddUniquesInfo", OldName = "AddUniquesInfo2")]
        [Index("uk_phone", "phone", true)]
        [Index("uk_group_index", "group,index", true)]
        [Index("uk_group_index22", "group, index22", false)]
        class AddUniquesInfo
        {
            public Guid id { get; set; }
            public string phone { get; set; }

            public string group { get; set; }
            public int index { get; set; }
            public string index22 { get; set; }
        }

        [Fact]
        public void AddField()
        {
            var sql = g.sqlserver.CodeFirst.GetComparisonDDLStatements<TopicAddField>();

            var id = g.sqlserver.Insert<TopicAddField>().AppendData(new TopicAddField { }).ExecuteIdentity();
        }

        [Table(Name = "dbo2.TopicAddField", OldName = "tedb1.dbo.TopicAddField")]
        public class TopicAddField
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }

            public int name { get; set; } = 3000;

            [Column(DbType = "varchar(200) not null", OldName = "title")]
            public string title222 { get; set; } = "333";

            [Column(DbType = "varchar(200) not null")]
            public string title222333 { get; set; } = "xxx";

            [Column(DbType = "varchar(100) not null", OldName = "title122333aaa")]
            public string titleaaa { get; set; } = "fsdf";

            [Column(IsIgnore = true)]
            public DateTime ct { get; set; } = DateTime.Now;
        }

        [Fact]
        public void GetComparisonDDLStatements()
        {
            var sql = g.sqlserver.CodeFirst.GetComparisonDDLStatements<TableAllType>();
            Assert.True(string.IsNullOrEmpty(sql)); //测试运行两次后
            sql = g.sqlserver.CodeFirst.GetComparisonDDLStatements<Tb_alltype>();
        }

        IInsert<TableAllType> insert => g.sqlserver.Insert<TableAllType>();
        ISelect<TableAllType> select => g.sqlserver.Select<TableAllType>();

        [Fact]
        public void CurdAllField()
        {
            var item = new TableAllType { };
            item.Id = (int)insert.AppendData(item).ExecuteIdentity();

            var newitem = select.Where(a => a.Id == item.Id).ToOne();

            var item2 = new TableAllType
            {
                testFieldBool = true,
                testFieldBoolNullable = true,
                testFieldByte = byte.MaxValue,
                testFieldByteNullable = byte.MinValue,
                testFieldBytes = Encoding.GetEncoding("gb2312").GetBytes("我是中国人"),
                testFieldDateTime = DateTime.Now,
                testFieldDateTimeNullable = DateTime.Now.AddHours(1),
                testFieldDateTimeNullableOffset = new DateTimeOffset(DateTime.Now.AddHours(1), TimeSpan.FromHours(8)),
                testFieldDateTimeOffset = new DateTimeOffset(DateTime.Now, TimeSpan.FromHours(8)),
                testFieldDecimal = 998.99M,
                testFieldDecimalNullable = 999.12M,
                testFieldDouble = 99.199,
                testFieldDoubleNullable = 99.211,
                testFieldEnum1 = TableAllTypeEnumType1.e2,
                testFieldEnum1Nullable = TableAllTypeEnumType1.e3,
                testFieldEnum2 = TableAllTypeEnumType2.f3,
                testFieldEnum2Nullable = TableAllTypeEnumType2.f2,
                testFieldFloat = 0.99F,
                testFieldFloatNullable = 0.11F,
                testFieldGuid = Guid.NewGuid(),
                testFieldGuidNullable = Guid.NewGuid(),
                testFieldInt = int.MaxValue,
                testFieldIntNullable = int.MinValue,
                testFieldLong = long.MaxValue,
                testFieldSByte = sbyte.MaxValue,
                testFieldSByteNullable = sbyte.MinValue,
                testFieldShort = short.MaxValue,
                testFieldShortNullable = short.MinValue,
                testFieldString = "我是中国人string'\\?!@#$%^&*()_+{}}{~?><<>",
                testFieldChar = 'X',
                testFieldTimeSpan = TimeSpan.FromSeconds(999),
                testFieldTimeSpanNullable = TimeSpan.FromSeconds(30),
                testFieldUInt = uint.MaxValue,
                testFieldUIntNullable = uint.MinValue,
                testFieldULong = ulong.MaxValue,
                testFieldULongNullable = ulong.MinValue,
                testFieldUShort = ushort.MaxValue,
                testFieldUShortNullable = ushort.MinValue,
                testFielLongNullable = long.MinValue
            };

            var sqlPar = insert.AppendData(item2).ToSql();
            var sqlText = insert.AppendData(item2).NoneParameter().ToSql();
            var item3NP = insert.AppendData(item2).NoneParameter().ExecuteInserted();

            var sqlTestUpdate = g.sqlserver.Update<TableAllType>().SetSource(item3NP).NoneParameter().ToSql();

            var item3 = insert.AppendData(item2).ExecuteInserted();
            var newitem2 = select.Where(a => a.Id == item3[0].Id).ToOne();
            Assert.Equal(item2.testFieldString, newitem2.testFieldString);
            Assert.Equal(item2.testFieldChar, newitem2.testFieldChar);

            item3 = insert.NoneParameter().AppendData(item2).ExecuteInserted();
            newitem2 = select.Where(a => a.Id == item3[0].Id).ToOne();
            Assert.Equal(item2.testFieldString, newitem2.testFieldString);
            Assert.Equal(item2.testFieldChar, newitem2.testFieldChar);

            var items = select.ToList();
            var itemstb = select.ToDataTable();
        }

        [JsonObject(MemberSerialization.OptIn), Table(Name = "dbo.tb_alltype")]
        public partial class Tb_alltype
        {

            [JsonProperty, Column(Name = "Id", DbType = "int", IsPrimary = true, IsIdentity = true)]
            public int Id { get; set; }


            [JsonProperty, Column(Name = "testFieldBool1111", DbType = "bit")]
            public bool TestFieldBool1111 { get; set; }


            [JsonProperty, Column(Name = "testFieldBoolNullable", DbType = "bit", IsNullable = true)]
            public bool? TestFieldBoolNullable { get; set; }


            [JsonProperty, Column(Name = "testFieldByte", DbType = "tinyint")]
            public sbyte TestFieldByte { get; set; }


            [JsonProperty, Column(Name = "testFieldByteNullable", DbType = "tinyint", IsNullable = true)]
            public sbyte? TestFieldByteNullable { get; set; }


            [JsonProperty, Column(Name = "testFieldBytes", DbType = "varbinary(255)", IsNullable = true)]
            public byte[] TestFieldBytes { get; set; }


            [JsonProperty, Column(Name = "testFieldDateTime", DbType = "datetime")]
            public DateTime TestFieldDateTime { get; set; }


            [JsonProperty, Column(Name = "testFieldDateTimeNullable", DbType = "datetime", IsNullable = true)]
            public DateTime? TestFieldDateTimeNullable { get; set; }


            [JsonProperty, Column(Name = "testFieldDateTimeNullableOffset", DbType = "datetimeoffset", IsNullable = true)]
            public DateTime? TestFieldDateTimeNullableOffset { get; set; }


            [JsonProperty, Column(Name = "testFieldDateTimeOffset", DbType = "datetimeoffset")]
            public DateTime TestFieldDateTimeOffset { get; set; }


            [JsonProperty, Column(Name = "testFieldDecimal", DbType = "decimal(10,2)")]
            public decimal TestFieldDecimal { get; set; }


            [JsonProperty, Column(Name = "testFieldDecimalNullable", DbType = "decimal(10,2)", IsNullable = true)]
            public decimal? TestFieldDecimalNullable { get; set; }


            [JsonProperty, Column(Name = "testFieldDouble", DbType = "float")]
            public double TestFieldDouble { get; set; }


            [JsonProperty, Column(Name = "testFieldDoubleNullable", DbType = "float", IsNullable = true)]
            public double? TestFieldDoubleNullable { get; set; }


            [JsonProperty, Column(Name = "testFieldEnum1", DbType = "int")]
            public int TestFieldEnum1 { get; set; }


            [JsonProperty, Column(Name = "testFieldEnum1Nullable", DbType = "int", IsNullable = true)]
            public int? TestFieldEnum1Nullable { get; set; }


            [JsonProperty, Column(Name = "testFieldEnum2", DbType = "bigint")]
            public long TestFieldEnum2 { get; set; }


            [JsonProperty, Column(Name = "testFieldEnum2Nullable", DbType = "bigint", IsNullable = true)]
            public long? TestFieldEnum2Nullable { get; set; }


            [JsonProperty, Column(Name = "testFieldFloat", DbType = "real")]
            public float TestFieldFloat { get; set; }


            [JsonProperty, Column(Name = "testFieldFloatNullable", DbType = "real", IsNullable = true)]
            public float? TestFieldFloatNullable { get; set; }


            [JsonProperty, Column(Name = "testFieldGuid", DbType = "uniqueidentifier")]
            public Guid TestFieldGuid { get; set; }


            [JsonProperty, Column(Name = "testFieldGuidNullable", DbType = "uniqueidentifier", IsNullable = true)]
            public Guid? TestFieldGuidNullable { get; set; }


            [JsonProperty, Column(Name = "testFieldInt", DbType = "int")]
            public int TestFieldInt { get; set; }


            [JsonProperty, Column(Name = "testFieldIntNullable", DbType = "int", IsNullable = true)]
            public int? TestFieldIntNullable { get; set; }


            [JsonProperty, Column(Name = "testFieldLong", DbType = "bigint")]
            public long TestFieldLong { get; set; }


            [JsonProperty, Column(Name = "testFieldSByte", DbType = "tinyint")]
            public sbyte TestFieldSByte { get; set; }


            [JsonProperty, Column(Name = "testFieldSByteNullable", DbType = "tinyint", IsNullable = true)]
            public sbyte? TestFieldSByteNullable { get; set; }


            [JsonProperty, Column(Name = "testFieldShort", DbType = "smallint")]
            public short TestFieldShort { get; set; }


            [JsonProperty, Column(Name = "testFieldShortNullable", DbType = "smallint", IsNullable = true)]
            public short? TestFieldShortNullable { get; set; }


            [JsonProperty, Column(Name = "testFieldString", DbType = "nvarchar(255)", IsNullable = true)]
            public string TestFieldString { get; set; }


            [JsonProperty, Column(Name = "testFieldChar", DbType = "char(1)", IsNullable = true)]
            public char testFieldChar { get; set; }


            [JsonProperty, Column(Name = "testFieldTimeSpan", DbType = "time")]
            public TimeSpan TestFieldTimeSpan { get; set; }


            [JsonProperty, Column(Name = "testFieldTimeSpanNullable", DbType = "time", IsNullable = true)]
            public TimeSpan? TestFieldTimeSpanNullable { get; set; }


            [JsonProperty, Column(Name = "testFieldUInt", DbType = "int")]
            public int TestFieldUInt { get; set; }


            [JsonProperty, Column(Name = "testFieldUIntNullable", DbType = "int", IsNullable = true)]
            public int? TestFieldUIntNullable { get; set; }


            [JsonProperty, Column(Name = "testFieldULong", DbType = "bigint")]
            public long TestFieldULong { get; set; }


            [JsonProperty, Column(Name = "testFieldULongNullable", DbType = "bigint", IsNullable = true)]
            public long? TestFieldULongNullable { get; set; }


            [JsonProperty, Column(Name = "testFieldUShort", DbType = "smallint")]
            public short TestFieldUShort { get; set; }


            [JsonProperty, Column(Name = "testFieldUShortNullable", DbType = "smallint", IsNullable = true)]
            public short? TestFieldUShortNullable { get; set; }


            [JsonProperty, Column(Name = "testFielLongNullable", DbType = "bigint", IsNullable = true)]
            public long? TestFielLongNullable { get; set; }
        }

        [Table(Name = "tb_alltype")]
        class TableAllType
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }

            [Column(Name = "testFieldBool1111")]
            public bool testFieldBool { get; set; }
            public sbyte testFieldSByte { get; set; }
            public short testFieldShort { get; set; }
            public int testFieldInt { get; set; }
            public long testFieldLong { get; set; }
            public byte testFieldByte { get; set; }
            public ushort testFieldUShort { get; set; }
            public uint testFieldUInt { get; set; }
            public ulong testFieldULong { get; set; }
            public double testFieldDouble { get; set; }
            public float testFieldFloat { get; set; }
            public decimal testFieldDecimal { get; set; }
            public TimeSpan testFieldTimeSpan { get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime testFieldDateTime { get; set; }
            [Column(ServerTime = DateTimeKind.Local)]
            public DateTimeOffset testFieldDateTimeOffset { get; set; }

            public byte[] testFieldBytes { get; set; }
            public string testFieldString { get; set; }
            public char testFieldChar { get; set; }
            public Guid testFieldGuid { get; set; }

            public bool? testFieldBoolNullable { get; set; }
            public sbyte? testFieldSByteNullable { get; set; }
            public short? testFieldShortNullable { get; set; }
            public int? testFieldIntNullable { get; set; }
            public long? testFielLongNullable { get; set; }
            public byte? testFieldByteNullable { get; set; }
            public ushort? testFieldUShortNullable { get; set; }
            public uint? testFieldUIntNullable { get; set; }
            public ulong? testFieldULongNullable { get; set; }
            public double? testFieldDoubleNullable { get; set; }
            public float? testFieldFloatNullable { get; set; }
            public decimal? testFieldDecimalNullable { get; set; }
            public TimeSpan? testFieldTimeSpanNullable { get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime? testFieldDateTimeNullable { get; set; }
            [Column(ServerTime = DateTimeKind.Local)]
            public DateTimeOffset? testFieldDateTimeNullableOffset { get; set; }

            public Guid? testFieldGuidNullable { get; set; }

            public TableAllTypeEnumType1 testFieldEnum1 { get; set; }
            public TableAllTypeEnumType1? testFieldEnum1Nullable { get; set; }
            public TableAllTypeEnumType2 testFieldEnum2 { get; set; }
            public TableAllTypeEnumType2? testFieldEnum2Nullable { get; set; }
        }

        public enum TableAllTypeEnumType1 { e1, e2, e3, e5 }
        [Flags] public enum TableAllTypeEnumType2 { f1, f2, f3 }
    }
}
