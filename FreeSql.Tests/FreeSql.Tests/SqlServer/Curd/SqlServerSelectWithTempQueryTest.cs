using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace FreeSql.Tests.SqlServer
{
    public class SqlServerSelectWithTempQueryTest
    {
        #region issues #1215

        [Fact]
        public void VicDemo20220815()
        {
            var fsql = g.sqlserver;
            var subquery1 = fsql.Select<BaseHeadEntity>().AsType(typeof(BhEntity1)).Where(bh => bh.IsDeleted == false)
                    .FromQuery(fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity1)).As("bi").Where(bi => bi.IsDeleted == false))
                    .InnerJoin(v => v.t1.Id == v.t2.HeadId)
                    .WithTempQuery(v => new
                    {
                        BillHead = v.t1,
                        Quantity = v.t2.Quantity,
                        RefQuantity = fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity2)).As("bi2")
                            .Where(ti2 => ti2.RefHeadId == v.t2.HeadId && ti2.RefItemId == v.t2.Id)
                            .Sum(ti2 => ti2.Quantity),
                    })
                    .Where(v => v.RefQuantity < v.Quantity)
                    .GroupBy(v => v.BillHead.Id)
                    .ToSql(v => v.Key);
            var sql1 = fsql.Select<BaseHeadEntity>().AsType(typeof(BhEntity1))
                .Where($"a.Id IN ({subquery1})").OrderByDescending(vh => vh.Date)
                .ToSql();
            Assert.Equal(@"SELECT a.[IsDeleted], a.[Id], a.[No], a.[Date] 
FROM [bhe_1] a 
WHERE (a.Id IN (SELECT a.[Id] as1 
FROM ( 
    SELECT a.[IsDeleted], a.[Id], a.[No], a.[Date], htb.[Quantity], isnull((SELECT sum(ti2.[Quantity]) 
        FROM [bie_2] ti2 
        WHERE (ti2.[RefHeadId] = htb.[HeadId] AND ti2.[RefItemId] = htb.[Id])), 0) [RefQuantity] 
    FROM [bhe_1] a 
    INNER JOIN ( 
        SELECT bi.[IsDeleted], bi.[Id], bi.[HeadId], bi.[GoodsId], bi.[Quantity], bi.[RefHeadId], bi.[RefItemId] 
        FROM [bie_1] bi 
        WHERE (bi.[IsDeleted] = 0)) htb ON a.[Id] = htb.[HeadId] 
    WHERE (a.[IsDeleted] = 0) ) a 
WHERE (a.[RefQuantity] < a.[Quantity]) 
GROUP BY a.[Id])) 
ORDER BY a.[Date] DESC", sql1);

            var sql2 = fsql.Select<BaseHeadEntity>().AsType(typeof(BhEntity1))
                .Where(vh => fsql.Select<BaseHeadEntity>().AsType(typeof(BhEntity1)).Where(bh => bh.IsDeleted == false)
                    .FromQuery(fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity1)).As("bi").Where(bi => bi.IsDeleted == false))
                    .InnerJoin(v => v.t1.Id == v.t2.HeadId)
                    .WithTempQuery(v => new
                    {
                        BillHead = v.t1,
                        Quantity = v.t2.Quantity,
                        RefQuantity = fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity2)).As("bi2")
                            .Where(ti2 => ti2.RefHeadId == v.t2.HeadId && ti2.RefItemId == v.t2.Id)
                            .Sum(ti2 => ti2.Quantity),
                    })
                    .Where(v => v.RefQuantity < v.Quantity)
                    .Distinct()
                    .ToList(v => v.BillHead.Id).Contains(vh.Id)
                ).OrderByDescending(vh => vh.Date)
                .ToSql();
            Assert.Equal(@"SELECT a.[IsDeleted], a.[Id], a.[No], a.[Date] 
FROM [bhe_1] a 
WHERE (((a.[Id]) in (SELECT DISTINCT v.[Id] 
    FROM ( 
        SELECT bh.[IsDeleted], bh.[Id], bh.[No], bh.[Date], ht2.[Quantity], isnull((SELECT sum(ti2.[Quantity]) 
            FROM [bie_2] ti2 
            WHERE (ti2.[RefHeadId] = ht2.[HeadId] AND ti2.[RefItemId] = ht2.[Id])), 0) [RefQuantity] 
        FROM [bhe_1] bh 
        INNER JOIN ( 
            SELECT bi.[IsDeleted], bi.[Id], bi.[HeadId], bi.[GoodsId], bi.[Quantity], bi.[RefHeadId], bi.[RefItemId] 
            FROM [bie_1] bi 
            WHERE (bi.[IsDeleted] = 0)) ht2 ON bh.[Id] = ht2.[HeadId] 
        WHERE (bh.[IsDeleted] = 0) ) v 
    WHERE (v.[RefQuantity] < v.[Quantity])))) 
ORDER BY a.[Date] DESC", sql2);

            var sql3 = fsql.Select<BaseHeadEntity>().AsType(typeof(BhEntity1))
                .FromQuery(
                    fsql.Select<BaseHeadEntity>().AsType(typeof(BhEntity1)).Where(bh => bh.IsDeleted == false)
                        .FromQuery(fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity1)).As("bi").Where(bi => bi.IsDeleted == false))
                        .InnerJoin(v => v.t1.Id == v.t2.HeadId)
                        .WithTempQuery(v => new
                        {
                            BillHead = v.t1,
                            Quantity = v.t2.Quantity,
                            RefQuantity = fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity2)).As("bi2")
                                .Where(ti2 => ti2.RefHeadId == v.t2.HeadId && ti2.RefItemId == v.t2.Id)
                                .Sum(ti2 => ti2.Quantity),
                        })
                        .Where(v => v.RefQuantity < v.Quantity)
                        .Distinct()
                        .WithTempQuery(v => new { v.BillHead.Id })
                )
                .RightJoin(v => v.t1.Id == v.t2.Id)
                .OrderByDescending(v => v.t1.Date)
                .ToSql(v => v.t1);
            Assert.Equal(@"SELECT a.[IsDeleted] as1, a.[Id] as2, a.[No] as3, a.[Date] as4 
FROM [bhe_1] a 
RIGHT JOIN ( 
    SELECT DISTINCT a.[Id] 
    FROM ( 
        SELECT a.[IsDeleted], a.[Id], a.[No], a.[Date], htb.[Quantity], isnull((SELECT sum(ti2.[Quantity]) 
            FROM [bie_2] ti2 
            WHERE (ti2.[RefHeadId] = htb.[HeadId] AND ti2.[RefItemId] = htb.[Id])), 0) [RefQuantity] 
        FROM [bhe_1] a 
        INNER JOIN ( 
            SELECT bi.[IsDeleted], bi.[Id], bi.[HeadId], bi.[GoodsId], bi.[Quantity], bi.[RefHeadId], bi.[RefItemId] 
            FROM [bie_1] bi 
            WHERE (bi.[IsDeleted] = 0)) htb ON a.[Id] = htb.[HeadId] 
        WHERE (a.[IsDeleted] = 0) ) a 
    WHERE (a.[RefQuantity] < a.[Quantity]) ) htb ON a.[Id] = htb.[Id] 
ORDER BY a.[Date] DESC", sql3);


            fsql.Delete<BhEntity1>().Where("1=1").ExecuteAffrows();
            fsql.Delete<BiEntity1>().Where("1=1").ExecuteAffrows();
            fsql.Delete<BiEntity2>().Where("1=1").ExecuteAffrows();
            var bhid1 = Guid.NewGuid();
            var bhid2 = Guid.NewGuid();
            var bhid3 = Guid.NewGuid();
            fsql.Insert(new[] {
                new BhEntity1 { Id = bhid1, Date = DateTime.Parse("2022-08-16"), IsDeleted = false, No = "20220816_001" },
                new BhEntity1 { Id = bhid2, Date = DateTime.Parse("2022-08-17"), IsDeleted = false, No = "20220817_002" },
                new BhEntity1 { Id = bhid3, Date = DateTime.Parse("2022-08-18"), IsDeleted = false, No = "20220818_003" }
            }).ExecuteAffrows();

            var biid1 = Guid.NewGuid();
            var biid2 = Guid.NewGuid();
            var biid3 = Guid.NewGuid();
            var biid4 = Guid.NewGuid();
            fsql.Insert(new[] {
                new BiEntity1 { Id = biid1, HeadId = bhid1, GoodsId = 1, IsDeleted = false, Quantity = 1110, RefHeadId = bhid1, RefItemId = bhid1 },
                new BiEntity1 { Id = biid2, HeadId = bhid1, GoodsId = 2, IsDeleted = false, Quantity = 1111, RefHeadId = bhid1, RefItemId = bhid1 },
                new BiEntity1 { Id = biid3, HeadId = bhid2, GoodsId = 3, IsDeleted = false, Quantity = 1112, RefHeadId = bhid2, RefItemId = bhid2 },
                new BiEntity1 { Id = biid4, HeadId = bhid3, GoodsId = 4, IsDeleted = false, Quantity = 1113, RefHeadId = bhid3, RefItemId = bhid3 },
            }).ExecuteAffrows();

            fsql.Insert(new[] {
                new BiEntity2 { Id = Guid.NewGuid(), HeadId = bhid1, GoodsId = 11, IsDeleted = false, Quantity = 110, RefHeadId = bhid1, RefItemId = biid1 },
                new BiEntity2 { Id = Guid.NewGuid(), HeadId = bhid1, GoodsId = 12, IsDeleted = false, Quantity = 111, RefHeadId = bhid1, RefItemId = biid1 },
                new BiEntity2 { Id = Guid.NewGuid(), HeadId = bhid2, GoodsId = 13, IsDeleted = false, Quantity = 112, RefHeadId = bhid2, RefItemId = biid2 },
                new BiEntity2 { Id = Guid.NewGuid(), HeadId = bhid3, GoodsId = 14, IsDeleted = false, Quantity = 113, RefHeadId = bhid3, RefItemId = biid3 },
            }).ExecuteAffrows();
            var list1 = fsql.Select<BaseHeadEntity>().AsType(typeof(BhEntity1))
                .FromQuery(
                    fsql.Select<BaseHeadEntity>().AsType(typeof(BhEntity1)).Where(bh => bh.IsDeleted == false)
                        .FromQuery(fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity1)).As("bi").Where(bi => bi.IsDeleted == false))
                        .InnerJoin(v => v.t1.Id == v.t2.HeadId)
                        .WithTempQuery(v => new
                        {
                            BillHead = v.t1,
                            Quantity = v.t2.Quantity,
                            RefQuantity = fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity2)).As("bi2")
                                .Where(ti2 => ti2.RefHeadId == v.t2.HeadId && ti2.RefItemId == v.t2.Id)
                                .Sum(ti2 => ti2.Quantity),
                        })
                        .Where(v => v.RefQuantity < v.Quantity)
                        .Distinct()
                        .WithTempQuery(v => new { v.BillHead.Id })
                )
                .RightJoin(v => v.t1.Id == v.t2.Id)
                .OrderByDescending(v => v.t1.Date)
                .ToList(v => v.t1);
            Assert.Equal(3, list1.Count);
            Assert.Equal(DateTime.Parse("2022-08-18"), list1[0].Date);
            Assert.Equal(bhid3, list1[0].Id);
            Assert.False(list1[0].IsDeleted);
            Assert.Equal("20220818_003", list1[0].No);
            Assert.Equal(DateTime.Parse("2022-08-17"), list1[1].Date);
            Assert.Equal(bhid2, list1[1].Id);
            Assert.False(list1[1].IsDeleted);
            Assert.Equal("20220817_002", list1[1].No);
            Assert.Equal(DateTime.Parse("2022-08-16"), list1[2].Date);
            Assert.Equal(bhid1, list1[2].Id);
            Assert.False(list1[2].IsDeleted);
            Assert.Equal("20220816_001", list1[2].No);


            fsql.Delete<BhEntity3>("1=1").ExecuteAffrows();
            fsql.Delete<BiEntity1>("1=1").ExecuteAffrows();
            fsql.Delete<BiEntity2>("1=1").ExecuteAffrows();
            var bhSource = new List<BhEntity3>()
            {
                new() { Id=Guid.Parse("62b978e5-d97e-6c58-009b-35137f900f41"),No="BH001",Date=new DateTime(2022,08,16),UserField1="BH1UF001",UserField2="BH1UF002",UserField3="BH1UF003" },
                new() { Id=Guid.Parse("62b978e5-d97e-6c58-009b-35137f900f42"),No="BH002",Date=new DateTime(2022,08,16),UserField1="BH2UF001",UserField2="BH2UF002",UserField3="BH2UF003" },
                new() { Id=Guid.Parse("62b978e5-d97e-6c58-009b-35137f900f43"),No="BH003",Date=new DateTime(2022,08,16),UserField1="BH3UF001",UserField2="BH3UF002",UserField3="BH3UF003" },
                new() { Id=Guid.Parse("62b978e5-d97e-6c58-009b-35137f900f44"),No="BH004",Date=new DateTime(2022,08,16),UserField1="BH4UF001",UserField2="BH4UF002",UserField3="BH4UF003" },
                new() { Id=Guid.Parse("62b978e5-d97e-6c58-009b-35137f900f45"),No="BH005",Date=new DateTime(2022,08,16),UserField1="BH5UF001",UserField2="BH5UF002",UserField3="BH5UF003" },
            };
            var bi1Source = new List<BiEntity1>()
            {
                new() { Id=Guid.Parse("62c455b6-1b24-a468-00ef-87c86362d971"),HeadId=Guid.Parse("62b978e5-d97e-6c58-009b-35137f900f41"),GoodsId=1,Quantity=100 },
                new() { Id=Guid.Parse("62c455b6-1b24-a468-00ef-87c86362d972"),HeadId=Guid.Parse("62b978e5-d97e-6c58-009b-35137f900f42"),GoodsId=2,Quantity=100 },
                new() { Id=Guid.Parse("62c455b6-1b24-a468-00ef-87c86362d973"),HeadId=Guid.Parse("62b978e5-d97e-6c58-009b-35137f900f43"),GoodsId=3,Quantity=100 },
                new() { Id=Guid.Parse("62c455b6-1b24-a468-00ef-87c86362d974"),HeadId=Guid.Parse("62b978e5-d97e-6c58-009b-35137f900f44"),GoodsId=4,Quantity=100 },
                new() { Id=Guid.Parse("62c455b6-1b24-a468-00ef-87c86362d975"),HeadId=Guid.Parse("62b978e5-d97e-6c58-009b-35137f900f45"),GoodsId=5,Quantity=100 },
            };
            var bi2Source = new List<BiEntity2>()
            {
                new() { Id=Guid.Parse("62d02b69-8838-2bf0-009d-adcb760ec361"),HeadId=Guid.Parse("62d04bb1-e53d-1ae8-00d9-82bf2d58f631"),GoodsId=1,Quantity=10,RefHeadId=Guid.Parse("62b978e5-d97e-6c58-009b-35137f900f41"),RefItemId=Guid.Parse("62c455b6-1b24-a468-00ef-87c86362d971") },
                new() { Id=Guid.Parse("62d02b69-8838-2bf0-009d-adcb760ec362"),HeadId=Guid.Parse("62d04bb1-e53d-1ae8-00d9-82bf2d58f632"),GoodsId=2,Quantity=10,RefHeadId=Guid.Parse("62b978e5-d97e-6c58-009b-35137f900f42"),RefItemId=Guid.Parse("62c455b6-1b24-a468-00ef-87c86362d972") },
                new() { Id=Guid.Parse("62d02b69-8838-2bf0-009d-adcb760ec363"),HeadId=Guid.Parse("62d04bb1-e53d-1ae8-00d9-82bf2d58f633"),GoodsId=3,Quantity=10,RefHeadId=Guid.Parse("62b978e5-d97e-6c58-009b-35137f900f43"),RefItemId=Guid.Parse("62c455b6-1b24-a468-00ef-87c86362d973") },
                new() { Id=Guid.Parse("62d02b69-8838-2bf0-009d-adcb760ec364"),HeadId=Guid.Parse("62d04bb1-e53d-1ae8-00d9-82bf2d58f634"),GoodsId=4,Quantity=10,RefHeadId=Guid.Parse("62b978e5-d97e-6c58-009b-35137f900f44"),RefItemId=Guid.Parse("62c455b6-1b24-a468-00ef-87c86362d974") },
                new() { Id=Guid.Parse("62d02b69-8838-2bf0-009d-adcb760ec365"),HeadId=Guid.Parse("62d04bb1-e53d-1ae8-00d9-82bf2d58f635"),GoodsId=5,Quantity=10,RefHeadId=Guid.Parse("62b978e5-d97e-6c58-009b-35137f900f45"),RefItemId=Guid.Parse("62c455b6-1b24-a468-00ef-87c86362d975") },
            };
            fsql.InsertOrUpdate<BhEntity3>().SetSource(bhSource).ExecuteAffrows();
            fsql.InsertOrUpdate<BiEntity1>().SetSource(bi1Source).ExecuteAffrows();
            fsql.InsertOrUpdate<BiEntity2>().SetSource(bi2Source).ExecuteAffrows();

            var normal = fsql.Select<BaseHeadEntity>().AsType(typeof(BhEntity3))
                .Where(vh => fsql.Select<BaseHeadEntity>().AsType(typeof(BhEntity3)).Where(bh => bh.IsDeleted == false)
                    .FromQuery(fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity1)).As("bi").Where(bi => bi.IsDeleted == false))
                    .InnerJoin(v => v.t1.Id == v.t2.HeadId)
                    .WithTempQuery(v => new
                    {
                        BillHead = v.t1,
                        Quantity = v.t2.Quantity,
                        RefQuantity = fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity2)).As("bi2")
                            .Where(ti2 => ti2.RefHeadId == v.t2.HeadId && ti2.RefItemId == v.t2.Id)
                            .Sum(ti2 => ti2.Quantity),
                    })
                    .Where(v => v.RefQuantity < v.Quantity)
                    .Distinct()
                    .ToList(v => v.BillHead.Id).Contains(vh.Id)
                ).OrderByDescending(vh => vh.Date)
                .ToList();
            Assert.Equal(bhSource[0], normal[0]);

            var testCreate = () =>
                fsql.Select<BaseHeadEntity>().AsType(typeof(BhEntity3))
                    .FromQuery(
                        fsql.Select<BaseHeadEntity>().AsType(typeof(BhEntity3)).Where(bh => bh.IsDeleted == false)
                            .FromQuery(fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity1)).As("bi").Where(bi => bi.IsDeleted == false))
                            .InnerJoin(v => v.t1.Id == v.t2.HeadId)
                            .WithTempQuery(v => new
                            {
                                BillHead = v.t1,
                                Quantity = v.t2.Quantity,
                                RefQuantity = fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity2)).As("bi2")
                                    .Where(ti2 => ti2.RefHeadId == v.t2.HeadId && ti2.RefItemId == v.t2.Id)
                                    .Sum(ti2 => ti2.Quantity),
                            })
                            .Where(v => v.RefQuantity < v.Quantity)
                            .Distinct()
                            .WithTempQuery(v => new { v.BillHead.Id })
                    )
                    .RightJoin(v => v.t1.Id == v.t2.Id)
                    .OrderByDescending(v => v.t1.Date);

            // 测试：只返回基类的字段，实体类的字段丢失
            var lostFields = testCreate().ToList(v => v.t1);
            Assert.Equal(5, lostFields.Count);
            for (var xxx = 0; xxx < lostFields.Count; xxx++)
            {
                Assert.NotNull(lostFields[xxx] as BhEntity3);
                Assert.Equal(bhSource[xxx].Date, lostFields[xxx].Date);
                Assert.Equal(bhSource[xxx].Id, lostFields[xxx].Id);
                Assert.Equal(bhSource[xxx].IsDeleted, lostFields[xxx].IsDeleted);
                Assert.Equal(bhSource[xxx].No, lostFields[xxx].No);
                Assert.Equal(bhSource[xxx].UserField1, (lostFields[xxx] as BhEntity3).UserField1);
                Assert.Equal(bhSource[xxx].UserField2, (lostFields[xxx] as BhEntity3).UserField2);
                Assert.Equal(bhSource[xxx].UserField3, (lostFields[xxx] as BhEntity3).UserField3);
            }
            Assert.Equal(bhSource[0], lostFields[0]);
            // 测试：直接报类型转换错误
            lostFields = testCreate().ToList(); 
            Assert.Equal(5, lostFields.Count);
            for (var xxx = 0; xxx < lostFields.Count; xxx++)
            {
                Assert.NotNull(lostFields[xxx] as BhEntity3);
                Assert.Equal(bhSource[xxx].Date, lostFields[xxx].Date);
                Assert.Equal(bhSource[xxx].Id, lostFields[xxx].Id);
                Assert.Equal(bhSource[xxx].IsDeleted, lostFields[xxx].IsDeleted);
                Assert.Equal(bhSource[xxx].No, lostFields[xxx].No);
                Assert.Equal(bhSource[xxx].UserField1, (lostFields[xxx] as BhEntity3).UserField1);
                Assert.Equal(bhSource[xxx].UserField2, (lostFields[xxx] as BhEntity3).UserField2);
                Assert.Equal(bhSource[xxx].UserField3, (lostFields[xxx] as BhEntity3).UserField3);
            }
            var list3 = testCreate().ToList(a => a.t2);
            Assert.Equal(5, list3.Count);
            for (var xxx = 0; xxx < list3.Count; xxx++)
            {
                Assert.Equal(bhSource[xxx].Id, list3[xxx].Id);
            }
        }

        [Fact]
        public void VicDemo20220813()
        {
            var fsql = g.sqlserver;
            var id = Guid.Parse("62f83a6d-eb53-0608-0097-d177142cadcb");
            var sql1 = fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity1)).As("bi")
                .Where(bi => bi.HeadId == id && bi.IsDeleted == false)
                .Where(bi => fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity2)).As("ti")
                    .Where(ti => ti.RefHeadId == bi.HeadId && ti.RefItemId == bi.Id)
                    .Sum(ti => ti.Quantity) <= bi.Quantity)
                .ToSql();
            Assert.Equal(@"SELECT bi.[IsDeleted], bi.[Id], bi.[HeadId], bi.[GoodsId], bi.[Quantity], bi.[RefHeadId], bi.[RefItemId] 
FROM [bie_1] bi 
WHERE (bi.[HeadId] = '62f83a6d-eb53-0608-0097-d177142cadcb' AND bi.[IsDeleted] = 0) AND (isnull((SELECT sum(ti.[Quantity]) 
    FROM [bie_2] ti 
    WHERE (ti.[RefHeadId] = bi.[HeadId] AND ti.[RefItemId] = bi.[Id])), 0) <= bi.[Quantity])", sql1);

            var sql2 = fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity1)).As("bi")
                .Where(bi => bi.HeadId == id && bi.IsDeleted == false)
                .Where(bi => bi.HeadId == id && bi.IsDeleted == false)
                    .WithTempQuery(bi => new
                    {
                        bi.Id,
                        BillItem = bi,
                        bi.Quantity,
                        RefQuantity = fsql.Select<BaseItemEntity>().AsType(typeof(BiEntity2)).As("ti")
                            .Where(ti => ti.RefHeadId == bi.HeadId && ti.RefItemId == bi.Id)
                            .Sum(ti => ti.Quantity),
                    })
                    .Where(v => v.RefQuantity < v.Quantity)
                .ToSql();
            Assert.Equal(@"SELECT * 
FROM ( 
    SELECT bi.[Id], bi.[IsDeleted], bi.[Id], bi.[HeadId], bi.[GoodsId], bi.[Quantity], bi.[RefHeadId], bi.[RefItemId], bi.[Quantity], isnull((SELECT sum(ti.[Quantity]) 
        FROM [bie_2] ti 
        WHERE (ti.[RefHeadId] = bi.[HeadId] AND ti.[RefItemId] = bi.[Id])), 0) [RefQuantity] 
    FROM [bie_1] bi 
    WHERE (bi.[HeadId] = '62f83a6d-eb53-0608-0097-d177142cadcb' AND bi.[IsDeleted] = 0) AND (bi.[HeadId] = '62f83a6d-eb53-0608-0097-d177142cadcb' AND bi.[IsDeleted] = 0) ) a 
WHERE (a.[RefQuantity] < a.[Quantity])", sql2);
        }

        abstract class SoftDelete
        {
            public bool IsDeleted { get; set; }
        }
        abstract class BaseHeadEntity : SoftDelete
        {
            public Guid Id { get; set; }
            public string No { get; set; }
            public DateTime Date { get; set; }
        }
        [Table(Name = "bhe_1")]
        class BhEntity1 : BaseHeadEntity { }
        [Table(Name = "bhe_2")]
        class BhEntity2 : BaseHeadEntity { }
        [Table(Name = "bhe_3")]
        class BhEntity3 : BaseHeadEntity
        {
            [Column(Name = "f_usr_001")]
            public string UserField1 { get; set; }
            [Column(Name = "f_usr_002")]
            public string UserField2 { get; set; }
            [Column(Name = "f_usr_003")]
            public string UserField3 { get; set; }

            public override bool Equals(object obj)
            {
                if (base.Equals(obj)) return true;
                var as1 = obj as BhEntity3;
                if (as1 is null) return false;
                return base.Id == as1.Id &&
                    this.UserField1 == as1.UserField1 &&
                    this.UserField2 == as1.UserField2 &&
                    this.UserField3 == as1.UserField3;
            }
        }
        abstract class BaseItemEntity : SoftDelete
        {
            public Guid Id { get; set; }
            public Guid HeadId { get; set; }
            public int GoodsId { get; set; }
            public decimal Quantity { get; set; }
            public Guid? RefHeadId { get; set; }
            public Guid? RefItemId { get; set; }
        }
        [Table(Name = "bie_1")]
        class BiEntity1 : BaseItemEntity { }
        [Table(Name = "bie_2")]
        class BiEntity2 : BaseItemEntity { }
        #endregion

        [Fact]
        public void SingleTablePartitionBy()
        {
            var fsql = g.sqlserver;

            fsql.Delete<SingleTablePartitionBy_User>().Where("1=1").ExecuteAffrows();
            fsql.Insert(new[] {
                new SingleTablePartitionBy_User { Id = 1, Nickname = "name01" },
                new SingleTablePartitionBy_User { Id = 2, Nickname = "name01" },
                new SingleTablePartitionBy_User { Id = 3, Nickname = "name01" },
                new SingleTablePartitionBy_User { Id = 4, Nickname = "name02" },
                new SingleTablePartitionBy_User { Id = 5, Nickname = "name03" },
                new SingleTablePartitionBy_User { Id = 6, Nickname = "name03" },
            }).ExecuteAffrows();

            var sql01 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql();
            var assertSql01 = @"SELECT * 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [SingleTablePartitionBy_User] a ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql01, sql01);

            var sel01 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql01, sel01.ToSql());

            var list01 = sel01.ToList();
            Assert.Equal(3, list01.Count);
            Assert.Equal(list01[0].rownum, 1);
            Assert.Equal(list01[0].item.Id, 1);
            Assert.Equal(list01[0].item.Nickname, "name01");
            Assert.Equal(list01[1].rownum, 1);
            Assert.Equal(list01[1].item.Id, 4);
            Assert.Equal(list01[1].item.Nickname, "name02");
            Assert.Equal(list01[2].rownum, 1);
            Assert.Equal(list01[2].item.Id, 5);
            Assert.Equal(list01[2].item.Nickname, "name03");


            var sql0111 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .UnionAll(
                    fsql.Select<SingleTablePartitionBy_User>()
                    .WithTempQuery(a => new
                    {
                        item = a,
                        rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderByDescending(a.Id).ToValue()
                    })
                    .Where(a => a.rownum == 2)
                )
                .Where(a => a.rownum == 1 || a.rownum == 2)
                .ToSql();
            var assertSql0111 = @"SELECT * 
FROM ( SELECT * 
    FROM ( 
        SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
        FROM [SingleTablePartitionBy_User] a ) a 
    WHERE (a.[rownum] = 1) 
    UNION ALL 
    SELECT * 
    FROM ( 
        SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id] desc) [rownum] 
        FROM [SingleTablePartitionBy_User] a ) a 
    WHERE (a.[rownum] = 2) ) a 
WHERE ((a.[rownum] = 1 OR a.[rownum] = 2))";
            Assert.Equal(assertSql0111, sql0111);

            var sel0111 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .UnionAll(
                    fsql.Select<SingleTablePartitionBy_User>()
                    .WithTempQuery(a => new
                    {
                        item = a,
                        rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderByDescending(a.Id).ToValue()
                    })
                    .Where(a => a.rownum == 2)
                )
                .Where(a => a.rownum == 1 || a.rownum == 2);
            Assert.Equal(assertSql0111, sel0111.ToSql());

            var list0111 = sel0111.ToList();
            Assert.Equal(5, list0111.Count);
            Assert.Equal(list0111[0].rownum, 1);
            Assert.Equal(list0111[0].item.Id, 1);
            Assert.Equal(list0111[0].item.Nickname, "name01");
            Assert.Equal(list0111[1].rownum, 1);
            Assert.Equal(list0111[1].item.Id, 4);
            Assert.Equal(list0111[1].item.Nickname, "name02");
            Assert.Equal(list0111[2].rownum, 1);
            Assert.Equal(list0111[2].item.Id, 5);
            Assert.Equal(list0111[2].item.Nickname, "name03");
            Assert.Equal(list0111[3].rownum, 2);
            Assert.Equal(list0111[3].item.Id, 2);
            Assert.Equal(list0111[3].item.Nickname, "name01");
            Assert.Equal(list0111[4].rownum, 2);
            Assert.Equal(list0111[4].item.Id, 5);
            Assert.Equal(list0111[4].item.Nickname, "name03");


            var sql011 = fsql.Select<SingleTablePartitionBy_User>()
                .GroupBy(a => a.Nickname)
                .WithTempQuery(g => new { min = g.Min(g.Value.Id) })
                .From<SingleTablePartitionBy_User>()
                .InnerJoin((a, b) => a.min == b.Id)
                .ToSql((a, b) => new { item1 = a, item2 = b });
            var assertSql011 = @"SELECT a.[min] as1, b.[Id] as2, b.[Nickname] as3 
FROM ( 
    SELECT min(a.[Id]) [min] 
    FROM [SingleTablePartitionBy_User] a 
    GROUP BY a.[Nickname] ) a 
INNER JOIN [SingleTablePartitionBy_User] b ON a.[min] = b.[Id]";
            Assert.Equal(assertSql011, sql011);

            var sel011 = fsql.Select<SingleTablePartitionBy_User>()
                .GroupBy(a => a.Nickname)
                .WithTempQuery(g => new { min = g.Min(g.Value.Id) })
                .From<SingleTablePartitionBy_User>()
                .InnerJoin((a, b) => a.min == b.Id);
            Assert.Equal(assertSql011, sel011.ToSql((a, b) => new { item1 = a, item2 = b }));

            var list011 = sel011.ToList((a, b) => new { item1 = a, item2 = b });
            Assert.Equal(3, list011.Count);
            Assert.Equal(list011[0].item1.min, 1);
            Assert.Equal(list011[0].item2.Id, 1);
            Assert.Equal(list011[0].item2.Nickname, "name01");
            Assert.Equal(list011[1].item1.min, 4);
            Assert.Equal(list011[1].item2.Id, 4);
            Assert.Equal(list011[1].item2.Nickname, "name02");
            Assert.Equal(list011[2].item1.min, 5);
            Assert.Equal(list011[2].item2.Id, 5);
            Assert.Equal(list011[2].item2.Nickname, "name03");


            var sql02 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql(a => a.item);
            var assertSql02 = @"SELECT a.[Id] as1, a.[Nickname] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [SingleTablePartitionBy_User] a ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql02, sql02);

            var sel02 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql02, sel02.ToSql(a => a.item));

            var list02 = sel02.ToList(a => a.item);
            Assert.Equal(3, list02.Count);
            Assert.Equal(list02[0].Id, 1);
            Assert.Equal(list02[0].Nickname, "name01");
            Assert.Equal(list02[1].Id, 4);
            Assert.Equal(list02[1].Nickname, "name02");
            Assert.Equal(list02[2].Id, 5);
            Assert.Equal(list02[2].Nickname, "name03");


            var sql03 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql(a => new
                {
                    a.item.Id,
                    a.rownum
                });
            var assertSql03 = @"SELECT a.[Id] as1, a.[rownum] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [SingleTablePartitionBy_User] a ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql03, sql03);

            var sel03 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql03, sel03.ToSql(a => new
            {
                a.item.Id,
                a.rownum
            }));

            var list03 = sel03.ToList(a => new
            {
                a.item.Id,
                a.rownum
            });
            Assert.Equal(3, list03.Count);
            Assert.Equal(list03[0].rownum, 1);
            Assert.Equal(list03[0].Id, 1);
            Assert.Equal(list03[1].rownum, 1);
            Assert.Equal(list03[1].Id, 4);
            Assert.Equal(list03[2].rownum, 1);
            Assert.Equal(list03[2].Id, 5);



            var sql04 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    a.Id,
                    a.Nickname,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql(a => new SingleTablePartitionBy_UserDto());
            var assertSql04 = @"SELECT a.[Id] as1, a.[rownum] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [SingleTablePartitionBy_User] a ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql04, sql04);

            var sel04 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    a.Id,
                    a.Nickname,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql04, sel04.ToSql(a => new SingleTablePartitionBy_UserDto()));

            var list04 = sel04.ToList<SingleTablePartitionBy_UserDto>();
            Assert.Equal(3, list04.Count);
            Assert.Equal(list04[0].rownum, 1);
            Assert.Equal(list04[0].Id, 1);
            Assert.Equal(list04[1].rownum, 1);
            Assert.Equal(list04[1].Id, 4);
            Assert.Equal(list04[2].rownum, 1);
            Assert.Equal(list04[2].Id, 5);


            var sql05 = fsql.Select<TwoTablePartitionBy_User>()
                 .Where(a => a.Id > 0)
                 .WithTempQuery(a => new
                 {
                     a.Id,
                     a.Nickname
                 })
                 .GroupBy(a => new { a.Nickname })
                 .WithTempQuery(a => new
                 {
                     a.Key,
                     sum1 = a.Sum(a.Value.Id),
                     cou1 = a.Count()
                 })
                 .ToSql();
            var assertSql05 = @"SELECT * 
FROM ( 
    SELECT a.[Nickname], sum(a.[Id]) [sum1], count(1) [cou1] 
    FROM ( 
        SELECT a.[Id], a.[Nickname] 
        FROM [TwoTablePartitionBy_User] a 
        WHERE (a.[Id] > 0) ) a 
    GROUP BY a.[Nickname] ) a";
            Assert.Equal(assertSql05, sql05);
            var list05 = fsql.Select<TwoTablePartitionBy_User>()
                 .Where(a => a.Id > 0)
                 .WithTempQuery(a => new
                 {
                     a.Id,
                     a.Nickname
                 })
                 .GroupBy(a => new { a.Nickname })
                 .WithTempQuery(a => new
                 {
                     a.Key,
                     sum1 = a.Sum(a.Value.Id),
                     cou1 = a.Count()
                 })
                 .ToList();
            Assert.Equal(3, list05.Count);
            Assert.Equal("name01", list05[0].Key.Nickname);
            Assert.Equal(6, list05[0].sum1);
            Assert.Equal(3, list05[0].cou1);
            Assert.Equal("name02", list05[1].Key.Nickname);
            Assert.Equal(4, list05[1].sum1);
            Assert.Equal(1, list05[1].cou1);
            Assert.Equal("name03", list05[2].Key.Nickname);
            Assert.Equal(11, list05[2].sum1);
            Assert.Equal(2, list05[2].cou1);
        }
        class SingleTablePartitionBy_User
        {
            public int Id { get; set; }
            public string Nickname { get; set; }
        }
        class SingleTablePartitionBy_UserDto
        {
            public int Id { get; set; }
            public int rownum { get; set; }
        }


        [Fact]
        public void TwoTablePartitionBy()
        {
            var fsql = g.sqlserver;

            fsql.Delete<TwoTablePartitionBy_User>().Where("1=1").ExecuteAffrows();
            fsql.Delete<TwoTablePartitionBy_UserExt>().Where("1=1").ExecuteAffrows();
            fsql.Insert(new[] {
                new TwoTablePartitionBy_User { Id = 1, Nickname = "name01" },
                new TwoTablePartitionBy_User { Id = 2, Nickname = "name01" },
                new TwoTablePartitionBy_User { Id = 3, Nickname = "name01" },
                new TwoTablePartitionBy_User { Id = 4, Nickname = "name02" },
                new TwoTablePartitionBy_User { Id = 5, Nickname = "name03" },
                new TwoTablePartitionBy_User { Id = 6, Nickname = "name03" },
            }).ExecuteAffrows();
            fsql.Insert(new[] {
                new TwoTablePartitionBy_UserExt { UserId = 1, Remark = "remark01" },
                new TwoTablePartitionBy_UserExt { UserId = 2, Remark = "remark02" },
                new TwoTablePartitionBy_UserExt { UserId = 3, Remark = "remark03" },
                new TwoTablePartitionBy_UserExt { UserId = 4, Remark = "remark04" },
                new TwoTablePartitionBy_UserExt { UserId = 5, Remark = "remark05" },
                new TwoTablePartitionBy_UserExt { UserId = 6, Remark = "remark06" },
            }).ExecuteAffrows();

            var sql01 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql();
            var assertSql01 = @"SELECT * 
FROM ( 
    SELECT a.[Id], a.[Nickname], b.[UserId], b.[Remark], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a 
    INNER JOIN [TwoTablePartitionBy_UserExt] b ON a.[Id] = b.[UserId] ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql01, sql01);

            var sel01 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql01, sel01.ToSql());

            var list01 = sel01.ToList();
            Assert.Equal(3, list01.Count);
            Assert.Equal(list01[0].rownum, 1);
            Assert.Equal(list01[0].user.Id, 1);
            Assert.Equal(list01[0].user.Nickname, "name01");
            Assert.Equal(list01[0].userext.Remark, "remark01");
            Assert.Equal(list01[1].rownum, 1);
            Assert.Equal(list01[1].user.Id, 4);
            Assert.Equal(list01[1].user.Nickname, "name02");
            Assert.Equal(list01[1].userext.Remark, "remark04");
            Assert.Equal(list01[2].rownum, 1);
            Assert.Equal(list01[2].user.Id, 5);
            Assert.Equal(list01[2].user.Nickname, "name03");
            Assert.Equal(list01[2].userext.Remark, "remark05");


            var sql02 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql(a => a.user);
            var assertSql02 = @"SELECT a.[Id] as1, a.[Nickname] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], b.[UserId], b.[Remark], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a 
    INNER JOIN [TwoTablePartitionBy_UserExt] b ON a.[Id] = b.[UserId] ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql02, sql02);

            var sel02 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql02, sel02.ToSql(a => a.user));

            var list02 = sel02.ToList(a => a.user);
            Assert.Equal(3, list02.Count);
            Assert.Equal(list02[0].Id, 1);
            Assert.Equal(list02[0].Nickname, "name01");
            Assert.Equal(list02[1].Id, 4);
            Assert.Equal(list02[1].Nickname, "name02");
            Assert.Equal(list02[2].Id, 5);
            Assert.Equal(list02[2].Nickname, "name03");


            var sql022 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql(a => a.userext);
            var assertSql022 = @"SELECT a.[UserId] as1, a.[Remark] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], b.[UserId], b.[Remark], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a 
    INNER JOIN [TwoTablePartitionBy_UserExt] b ON a.[Id] = b.[UserId] ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql022, sql022);

            var sel022 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql022, sel022.ToSql(a => a.userext));

            var list022 = sel022.ToList(a => a.userext);
            Assert.Equal(3, list022.Count);
            Assert.Equal(list022[0].UserId, 1);
            Assert.Equal(list022[0].Remark, "remark01");
            Assert.Equal(list022[1].UserId, 4);
            Assert.Equal(list022[1].Remark, "remark04");
            Assert.Equal(list022[2].UserId, 5);
            Assert.Equal(list022[2].Remark, "remark05");


            var sql03 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql(a => new
                {
                    a.user.Id,
                    a.rownum
                });
            var assertSql03 = @"SELECT a.[Id] as1, a.[rownum] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], b.[UserId], b.[Remark], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a 
    INNER JOIN [TwoTablePartitionBy_UserExt] b ON a.[Id] = b.[UserId] ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql03, sql03);

            var sel03 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql03, sel03.ToSql(a => new
            {
                a.user.Id,
                a.rownum
            }));

            var list03 = sel03.ToList(a => new
            {
                a.user.Id,
                a.rownum
            });
            Assert.Equal(3, list03.Count);
            Assert.Equal(list03[0].rownum, 1);
            Assert.Equal(list03[0].Id, 1);
            Assert.Equal(list03[1].rownum, 1);
            Assert.Equal(list03[1].Id, 4);
            Assert.Equal(list03[2].rownum, 1);
            Assert.Equal(list03[2].Id, 5);



            var sql04 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    a.Id,
                    a.Nickname,
                    b.Remark,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql(a => new TwoTablePartitionBy_UserDto());
            var assertSql04 = @"SELECT a.[Id] as1, a.[rownum] as2, a.[Remark] as3 
FROM ( 
    SELECT a.[Id], a.[Nickname], b.[Remark], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a 
    INNER JOIN [TwoTablePartitionBy_UserExt] b ON a.[Id] = b.[UserId] ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql04, sql04);

            var sel04 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    a.Id,
                    a.Nickname,
                    b.Remark,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql04, sel04.ToSql(a => new TwoTablePartitionBy_UserDto()));

            var list04 = sel04.ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(3, list04.Count);
            Assert.Equal(list04[0].rownum, 1);
            Assert.Equal(list04[0].Id, 1);
            Assert.Equal(list04[0].remark, "remark01");
            Assert.Equal(list04[1].rownum, 1);
            Assert.Equal(list04[1].Id, 4);
            Assert.Equal(list04[1].remark, "remark04");
            Assert.Equal(list04[2].rownum, 1);
            Assert.Equal(list04[2].Id, 5);
            Assert.Equal(list04[2].remark, "remark05");


            var sql05 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .WithTempQuery(a => a.user)
                .Where(a => a.Nickname == "name03")
                .ToSql(a => new TwoTablePartitionBy_UserDto());
            var assertSql05 = @"SELECT a.[Id] as1 
FROM ( 
    SELECT a.[Id], a.[Nickname] 
    FROM ( 
        SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
        FROM [TwoTablePartitionBy_User] a ) a 
    WHERE (a.[rownum] = 1) ) a 
WHERE (a.[Nickname] = N'name03')";
            Assert.Equal(sql05, assertSql05);
            var list05 = fsql.Select<TwoTablePartitionBy_User>()
                 .WithTempQuery(a => new
                 {
                     user = a,
                     rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                 })
                 .Where(a => a.rownum == 1)
                 .WithTempQuery(a => a.user)
                 .Where(a => a.Nickname == "name03")
                 .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list05.Count, 1);
            Assert.Equal(5, list05[0].Id);
            Assert.Equal(0, list05[0].rownum);
            Assert.Null(list05[0].remark);


            var sql06 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .WithTempQuery(a => a.user)
                .From<TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Nickname == "name03" || a.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql06 = @"SELECT a.[Id] as1, b.[Remark] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname] 
    FROM ( 
        SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
        FROM [TwoTablePartitionBy_User] a ) a 
    WHERE (a.[rownum] = 1) ) a 
INNER JOIN [TwoTablePartitionBy_UserExt] b ON a.[Id] = b.[UserId] 
WHERE ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql06, assertSql06);
            var list06 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .WithTempQuery(a => a.user)
                .From<TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Nickname == "name03" || a.Nickname == "name02")
                .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list06.Count, 2);
            Assert.Equal(list06[0].rownum, 0);
            Assert.Equal(list06[0].Id, 4);
            Assert.Equal(list06[0].remark, "remark04");
            Assert.Equal(list06[1].rownum, 0);
            Assert.Equal(list06[1].Id, 5);
            Assert.Equal(list06[1].remark, "remark05");


            var sql061 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .WithTempQuery(a => a.user)
                .From<TwoTablePartitionBy_UserExt>()
                .AsTable((type, old) => type == typeof(TwoTablePartitionBy_UserExt) ? old.Replace("TwoTablePartitionBy_", "") : old)
                .InnerJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Nickname == "name03" || a.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql061 = @"SELECT a.[Id] as1, b.[Remark] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname] 
    FROM ( 
        SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
        FROM [TwoTablePartitionBy_User] a ) a 
    WHERE (a.[rownum] = 1) ) a 
INNER JOIN [UserExt] b ON a.[Id] = b.[UserId] 
WHERE ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql061, assertSql061);


            var sql07 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .WithTempQuery(a => a.user)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>())
                .InnerJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Nickname == "name03" || a.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql07 = @"SELECT a.[Id] as1, b.[Remark] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname] 
    FROM ( 
        SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
        FROM [TwoTablePartitionBy_User] a ) a 
    WHERE (a.[rownum] = 1) ) a 
INNER JOIN [TwoTablePartitionBy_UserExt] b ON a.[Id] = b.[UserId] 
WHERE ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql07, assertSql07);
            var list07 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .WithTempQuery(a => a.user)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>())
                .InnerJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Nickname == "name03" || a.Nickname == "name02")
                .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list07.Count, 2);
            Assert.Equal(list07[0].rownum, 0);
            Assert.Equal(list07[0].Id, 4);
            Assert.Equal(list07[0].remark, "remark04");
            Assert.Equal(list07[1].rownum, 0);
            Assert.Equal(list07[1].Id, 5);
            Assert.Equal(list07[1].remark, "remark05");


            var sql08 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0))
                .InnerJoin((a, b) => a.user.Id == b.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql08 = @"SELECT a.[rownum] as1, b.[Remark] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a ) a 
INNER JOIN ( 
    SELECT a.[UserId], a.[Remark] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0)) b ON a.[Id] = b.[UserId] 
WHERE (a.[rownum] = 1) AND ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql08, assertSql08);
            var list08 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0))
                .InnerJoin((a, b) => a.user.Id == b.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list08.Count, 2);
            Assert.Equal(list08[0].rownum, 1);
            Assert.Equal(list08[0].Id, 0);
            Assert.Equal(list08[0].remark, "remark04");
            Assert.Equal(list08[1].rownum, 1);
            Assert.Equal(list08[1].Id, 0);
            Assert.Equal(list08[1].remark, "remark05");


            var sql09 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).WithTempQuery(b => new { b.UserId, b.Remark }))
                .InnerJoin((a, b) => a.user.Id == b.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql09 = @"SELECT a.[rownum] as1, b.[Remark] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a ) a 
INNER JOIN ( 
    SELECT a.[UserId], a.[Remark] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0) ) b ON a.[Id] = b.[UserId] 
WHERE (a.[rownum] = 1) AND ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql09, assertSql09);
            var list09 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).WithTempQuery(b => new { b.UserId, b.Remark }))
                .InnerJoin((a, b) => a.user.Id == b.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list09.Count, 2);
            Assert.Equal(list09[0].rownum, 1);
            Assert.Equal(list09[0].Id, 0);
            Assert.Equal(list09[0].remark, "remark04");
            Assert.Equal(list09[1].rownum, 1);
            Assert.Equal(list09[1].Id, 0);
            Assert.Equal(list09[1].remark, "remark05");


            var sql091 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).GroupBy(b => new { b.UserId, b.Remark }).WithTempQuery(b => b.Key))
                .InnerJoin((a, b) => a.user.Id == b.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql091 = @"SELECT a.[rownum] as1, b.[Remark] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a ) a 
INNER JOIN ( 
    SELECT a.[UserId], a.[Remark] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0) 
    GROUP BY a.[UserId], a.[Remark] ) b ON a.[Id] = b.[UserId] 
WHERE (a.[rownum] = 1) AND ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql091, assertSql091);
            var list091 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).GroupBy(b => new { b.UserId, b.Remark }).WithTempQuery(b => b.Key))
                .InnerJoin((a, b) => a.user.Id == b.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list091.Count, 2);
            Assert.Equal(list091[0].rownum, 1);
            Assert.Equal(list091[0].Id, 0);
            Assert.Equal(list091[0].remark, "remark04");
            Assert.Equal(list091[1].rownum, 1);
            Assert.Equal(list091[1].Id, 0);
            Assert.Equal(list091[1].remark, "remark05");


            var sql10 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).GroupBy(b => new { b.UserId, b.Remark }).WithTempQuery(b => new { b.Key, rownum = b.Sum(b.Value.UserId) }))
                .InnerJoin((a, b) => a.user.Id == b.Key.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql10 = @"SELECT a.[rownum] as1 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a ) a 
INNER JOIN ( 
    SELECT a.[UserId], a.[Remark], sum(a.[UserId]) [rownum] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0) 
    GROUP BY a.[UserId], a.[Remark] ) b ON a.[Id] = b.[UserId] 
WHERE (a.[rownum] = 1) AND ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql10, assertSql10);
            var list10 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).GroupBy(b => new { b.UserId, b.Remark }).WithTempQuery(b => new { b.Key, rownum = b.Sum(b.Value.UserId) }))
                .InnerJoin((a, b) => a.user.Id == b.Key.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list10.Count, 2);
            Assert.Equal(list10[0].rownum, 1);
            Assert.Equal(list10[0].Id, 0);
            Assert.Null(list10[0].remark);
            Assert.Equal(list10[1].rownum, 1);
            Assert.Equal(list10[1].Id, 0);
            Assert.Null(list10[1].remark);


            var sql11 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).GroupBy(b => b.UserId).WithTempQuery(b => new { uid = b.Key, rownum = b.Sum(b.Value.UserId) }))
                .InnerJoin((a, b) => a.user.Id == b.uid)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql11 = @"SELECT a.[rownum] as1 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a ) a 
INNER JOIN ( 
    SELECT a.[UserId] [uid], sum(a.[UserId]) [rownum] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0) 
    GROUP BY a.[UserId] ) b ON a.[Id] = b.[uid] 
WHERE (a.[rownum] = 1) AND ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql11, assertSql11);
            var list11 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).GroupBy(b => b.UserId).WithTempQuery(b => new { uid = b.Key, rownum = b.Sum(b.Value.UserId) }))
                .InnerJoin((a, b) => a.user.Id == b.uid)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list11.Count, 2);
            Assert.Equal(list11[0].rownum, 1);
            Assert.Equal(list11[0].Id, 0);
            Assert.Null(list11[0].remark);
            Assert.Equal(list11[1].rownum, 1);
            Assert.Equal(list11[1].Id, 0);
            Assert.Null(list11[1].remark);


            var sql12 = fsql.Select<TwoTablePartitionBy_User>()
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>()
                    .Where(b => b.UserId > 0))
                .LeftJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Id > 0 && b.UserId > 0)
                .GroupBy((a, b) => new { a.Nickname })
                .ToSql(g => new
                {
                    g.Key,
                    sum1 = g.Sum(g.Value.Item1.Id),
                    sum2 = g.Sum(g.Value.Item2.UserId),
                });
            var assertSql12 = @"SELECT a.[Nickname], sum(a.[Id]) as1, sum(b.[UserId]) as2 
FROM [TwoTablePartitionBy_User] a 
LEFT JOIN ( 
    SELECT a.[UserId], a.[Remark] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0)) b ON a.[Id] = b.[UserId] 
WHERE (a.[Id] > 0 AND b.[UserId] > 0) 
GROUP BY a.[Nickname]";
            Assert.Equal(sql12, assertSql12);
            var list12 = fsql.Select<TwoTablePartitionBy_User>()
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>()
                    .Where(b => b.UserId > 0))
                .LeftJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Id > 0 && b.UserId > 0)
                .GroupBy((a, b) => new { a.Nickname })
                .ToList(g => new
                {
                    g.Key,
                    sum1 = g.Sum(g.Value.Item1.Id),
                    sum2 = g.Sum(g.Value.Item2.UserId),
                });
            Assert.Equal(list12.Count, 3);
            Assert.Equal("name01", list12[0].Key.Nickname);
            Assert.Equal(6, list12[0].sum1);
            Assert.Equal(6, list12[0].sum2);
            Assert.Equal("name02", list12[1].Key.Nickname);
            Assert.Equal(4, list12[1].sum1);
            Assert.Equal(4, list12[1].sum2);
            Assert.Equal("name03", list12[2].Key.Nickname);
            Assert.Equal(11, list12[2].sum1);
            Assert.Equal(11, list12[2].sum2);


            var sql122 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => a)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>()
                    .Where(b => b.UserId > 0)
                    .GroupBy(b => b.UserId)
                    .WithTempQuery(b => new
                    {
                        b.Value.UserId,
                        sum1 = b.Sum(b.Value.UserId)
                    }))
                .LeftJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Id > 0 && b.UserId > 0)
                .ToSql((a, b) => new
                {
                    a.Nickname, b.sum1, b.UserId, a.Id
                });
            var assertSql122 = @"SELECT a.[Nickname] as1, b.[sum1] as2, b.[UserId] as3, a.[Id] as4 
FROM ( 
    SELECT a.[Id], a.[Nickname] 
    FROM [TwoTablePartitionBy_User] a ) a 
LEFT JOIN ( 
    SELECT a.[UserId], sum(a.[UserId]) [sum1] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0) 
    GROUP BY a.[UserId] ) b ON a.[Id] = b.[UserId] 
WHERE (a.[Id] > 0 AND b.[UserId] > 0)";
            Assert.Equal(sql122, assertSql122);
            var list122 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => a)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>()
                    .Where(b => b.UserId > 0)
                    .GroupBy(b => b.UserId)
                    .WithTempQuery(b => new
                    {
                        b.Value.UserId,
                        sum1 = b.Sum(b.Value.UserId)
                    }))
                .LeftJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Id > 0 && b.UserId > 0)
                .ToList((a, b) => new
                {
                    a.Nickname, b.sum1, b.UserId, a.Id
                });
            Assert.Equal(list122.Count, 6);
            Assert.Equal("name01", list122[0].Nickname);
            Assert.Equal(1, list122[0].sum1);
            Assert.Equal(1, list122[0].UserId);
            Assert.Equal(1, list122[0].Id);
            Assert.Equal("name01", list122[1].Nickname);
            Assert.Equal(2, list122[1].sum1);
            Assert.Equal(2, list122[1].UserId);
            Assert.Equal(2, list122[1].Id);
            Assert.Equal("name01", list122[2].Nickname);
            Assert.Equal(3, list122[2].sum1);
            Assert.Equal(3, list122[2].UserId);
            Assert.Equal(3, list122[2].Id);
            Assert.Equal("name02", list122[3].Nickname);
            Assert.Equal(4, list122[3].sum1);
            Assert.Equal(4, list122[3].UserId);
            Assert.Equal(4, list122[3].Id);
            Assert.Equal("name03", list122[4].Nickname);
            Assert.Equal(5, list122[4].sum1);
            Assert.Equal(5, list122[4].UserId);
            Assert.Equal(5, list122[4].Id);
            Assert.Equal("name03", list122[5].Nickname);
            Assert.Equal(6, list122[5].sum1);
            Assert.Equal(6, list122[5].UserId);
            Assert.Equal(6, list122[5].Id);


            var sql123 = fsql.Select<TwoTablePartitionBy_User>()
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>()
                    .Where(b => b.UserId > 0)
                    .GroupBy(b => b.UserId)
                    .WithTempQuery(b => new
                    {
                        b.Value.UserId,
                        sum1 = b.Sum(b.Value.UserId)
                    }))
                .LeftJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Id > 0 && b.UserId > 0)
                .GroupBy((a, b) => new { a.Nickname })
                .ToSql(g => new
                {
                    g.Key,
                    sum1 = g.Sum(g.Value.Item1.Id),
                    sum2 = g.Sum(g.Value.Item2.UserId),
                    sum3 = g.Sum(g.Value.Item2.sum1)
                });
            var assertSql123 = @"SELECT a.[Nickname], sum(a.[Id]) as1, sum(b.[UserId]) as2, sum(b.[sum1]) as3 
FROM [TwoTablePartitionBy_User] a 
LEFT JOIN ( 
    SELECT a.[UserId], sum(a.[UserId]) [sum1] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0) 
    GROUP BY a.[UserId] ) b ON a.[Id] = b.[UserId] 
WHERE (a.[Id] > 0 AND b.[UserId] > 0) 
GROUP BY a.[Nickname]";
            Assert.Equal(sql123, assertSql123);
            var list123 = fsql.Select<TwoTablePartitionBy_User>()
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>()
                    .Where(b => b.UserId > 0)
                    .GroupBy(b => b.UserId)
                    .WithTempQuery(b => new
                    {
                        b.Value.UserId,
                        sum1 = b.Sum(b.Value.UserId)
                    }))
                .LeftJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Id > 0 && b.UserId > 0)
                .GroupBy((a, b) => new { a.Nickname })
                .ToList(g => new
                {
                    g.Key,
                    sum1 = g.Sum(g.Value.Item1.Id),
                    sum2 = g.Sum(g.Value.Item2.UserId),
                    sum3 = g.Sum(g.Value.Item2.sum1)
                });
            Assert.Equal(list123.Count, 3);
            Assert.Equal("name01", list123[0].Key.Nickname);
            Assert.Equal(6, list123[0].sum1);
            Assert.Equal(6, list123[0].sum2);
            Assert.Equal(6, list123[0].sum3);
            Assert.Equal("name02", list123[1].Key.Nickname);
            Assert.Equal(4, list123[1].sum1);
            Assert.Equal(4, list123[1].sum2);
            Assert.Equal(4, list123[1].sum3);
            Assert.Equal("name03", list123[2].Key.Nickname);
            Assert.Equal(11, list123[2].sum1);
            Assert.Equal(11, list123[2].sum2);
            Assert.Equal(11, list123[2].sum3);


            var sql13 = fsql.Select<TwoTablePartitionBy_User>().AsTable((_, old) => old.Replace("TwoTablePartitionBy_", ""))
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().AsTable((_, old) => old.Replace("TwoTablePartitionBy_", ""))
                    .Where(b => b.UserId > 0))
                .LeftJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Id > 0 && b.UserId > 0)
                .GroupBy((a, b) => new { a.Nickname })
                .ToSql(g => new
                {
                    g.Key,
                    sum1 = g.Sum(g.Value.Item1.Id),
                    sum2 = g.Sum(g.Value.Item2.UserId),
                });
            var assertSql13 = @"SELECT a.[Nickname], sum(a.[Id]) as1, sum(b.[UserId]) as2 
FROM [User] a 
LEFT JOIN ( 
    SELECT a.[UserId], a.[Remark] 
    FROM [UserExt] a 
    WHERE (a.[UserId] > 0)) b ON a.[Id] = b.[UserId] 
WHERE (a.[Id] > 0 AND b.[UserId] > 0) 
GROUP BY a.[Nickname]";
            Assert.Equal(sql13, assertSql13);


            var sql14 = fsql.Select<TwoTablePartitionBy_User>()
                .Where(a => a.Id > 0)
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0))
                .InnerJoin((a, b) => a.item.Id == b.UserId)
                .ToSql((a, b) => new
                {
                    user = a.item,
                    rownum = a.rownum,
                    userext = b
                });
            var assertSql14 = @"SELECT a.[Id] as1, a.[Nickname] as2, a.[rownum] as3, b.[UserId] as4, b.[Remark] as5 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a 
    WHERE (a.[Id] > 0) ) a 
INNER JOIN ( 
    SELECT a.[UserId], a.[Remark] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0)) b ON a.[Id] = b.[UserId] 
WHERE (a.[rownum] = 1)";
            Assert.Equal(sql14, assertSql14);
            var list14 = fsql.Select<TwoTablePartitionBy_User>()
                .Where(a => a.Id > 0)
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0))
                .InnerJoin((a, b) => a.item.Id == b.UserId)
                .ToList((a, b) => new
                {
                    user = a.item,
                    rownum = a.rownum,
                    userext = b
                });
            Assert.Equal(list14.Count, 3);
            Assert.Equal(list14[0].rownum, 1);
            Assert.Equal(list14[0].user.Id, 1);
            Assert.Equal(list14[0].user.Nickname, "name01");
            Assert.Equal(list14[0].userext.UserId, 1);
            Assert.Equal(list14[0].userext.Remark, "remark01");
            Assert.Equal(list14[1].rownum, 1);
            Assert.Equal(list14[1].user.Id, 4);
            Assert.Equal(list14[1].user.Nickname, "name02");
            Assert.Equal(list14[1].userext.UserId, 4);
            Assert.Equal(list14[1].userext.Remark, "remark04");
            Assert.Equal(list14[2].rownum, 1);
            Assert.Equal(list14[2].user.Id, 5);
            Assert.Equal(list14[2].user.Nickname, "name03");
            Assert.Equal(list14[2].userext.UserId, 5);
            Assert.Equal(list14[2].userext.Remark, "remark05");


            var sql15 = fsql.Select<TwoTablePartitionBy_User>()
                 .WithTempQuery(a => new
                 {
                     user = a,
                     rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                 })
                 .Where(a => a.rownum == 1)
                 .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0)
                     .GroupBy(b => new { b.UserId, b.Remark })
                     .WithTempQuery(b => new { b.Key, sum1 = b.Sum(b.Value.UserId) }))
                 .InnerJoin((a, b) => a.user.Id == b.Key.UserId)
                 .Where((a, b) => a.user.Nickname == "name02" || a.user.Nickname == "name03")
                 .ToSql((a, b) => new
                 {
                     user = a.user,
                     rownum = a.rownum,
                     groupby = b
                 }, FieldAliasOptions.AsProperty);
            var assertSql15 = @"SELECT a.[Id], a.[Nickname], a.[rownum], b.[UserId], b.[Remark], b.[sum1] 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a ) a 
INNER JOIN ( 
    SELECT a.[UserId], a.[Remark], sum(a.[UserId]) [sum1] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0) 
    GROUP BY a.[UserId], a.[Remark] ) b ON a.[Id] = b.[UserId] 
WHERE (a.[rownum] = 1) AND ((a.[Nickname] = N'name02' OR a.[Nickname] = N'name03'))";
            Assert.Equal(sql15, assertSql15);
            var list15 = fsql.Select<TwoTablePartitionBy_User>()
                 .WithTempQuery(a => new
                 {
                     user = a,
                     rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                 })
                 .Where(a => a.rownum == 1)
                 .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0)
                     .GroupBy(b => new { b.UserId, b.Remark })
                     .WithTempQuery(b => new { b.Key, sum1 = b.Sum(b.Value.UserId) }))
                 .InnerJoin((a, b) => a.user.Id == b.Key.UserId)
                 .Where((a, b) => a.user.Nickname == "name02" || a.user.Nickname == "name03")
                 .ToList((a, b) => new
                 {
                     user = a.user,
                     rownum = a.rownum,
                     groupby = b
                 });
            Assert.Equal(list15.Count, 2);
            Assert.Equal("remark04", list15[0].groupby.Key.Remark);
            Assert.Equal(4, list15[0].groupby.Key.UserId);
            Assert.Equal(4, list15[0].groupby.sum1);
            Assert.Equal(1, list15[0].rownum);
            Assert.Equal(4, list15[0].user.Id);
            Assert.Equal("name02", list15[0].user.Nickname);
            Assert.Equal("remark05", list15[1].groupby.Key.Remark);
            Assert.Equal(5, list15[1].groupby.Key.UserId);
            Assert.Equal(5, list15[1].groupby.sum1);
            Assert.Equal(1, list15[1].rownum);
            Assert.Equal(5, list15[1].user.Id);
            Assert.Equal("name03", list15[1].user.Nickname);


            var sql16 = fsql.Select<TwoTablePartitionBy_User>()
                 .WithTempQuery(a => new
                 {
                     user = a,
                     rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                 })
                 .Where(a => a.rownum == 1)
                 .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0)
                     .GroupBy(b => new { b.UserId, b.Remark })
                     .WithTempQuery(b => new { b.Key, sum1 = b.Sum(b.Value.UserId) }))
                 .InnerJoin((a, b) => a.user.Id == b.Key.UserId)
                 .Where((a, b) => a.user.Nickname == "name02" || a.user.Nickname == "name03")
                 .ToSql((a, b) => new
                 {
                     user = a.user,
                     rownum = a.rownum,
                     groupby = b,
                     subquery1 = fsql.Select<TwoTablePartitionBy_UserDto>().Where(c => c.Id == a.user.Id).Count(),
                     subquery2 = fsql.Select<TwoTablePartitionBy_UserDto>().Where(c => c.Id == b.Key.UserId).Count(),
                 }, FieldAliasOptions.AsProperty);
            var assertSql16 = @"SELECT a.[Id], a.[Nickname], a.[rownum], b.[UserId], b.[Remark], b.[sum1], (SELECT count(1) 
    FROM [TwoTablePartitionBy_UserDto] c 
    WHERE (c.[Id] = a.[Id])) [subquery1], (SELECT count(1) 
    FROM [TwoTablePartitionBy_UserDto] c 
    WHERE (c.[Id] = b.[UserId])) [subquery2] 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a ) a 
INNER JOIN ( 
    SELECT a.[UserId], a.[Remark], sum(a.[UserId]) [sum1] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0) 
    GROUP BY a.[UserId], a.[Remark] ) b ON a.[Id] = b.[UserId] 
WHERE (a.[rownum] = 1) AND ((a.[Nickname] = N'name02' OR a.[Nickname] = N'name03'))";
            Assert.Equal(sql16, assertSql16);
            var list16 = fsql.Select<TwoTablePartitionBy_User>()
                 .WithTempQuery(a => new
                 {
                     user = a,
                     rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                 })
                 .Where(a => a.rownum == 1)
                 .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0)
                     .GroupBy(b => new { b.UserId, b.Remark })
                     .WithTempQuery(b => new { b.Key, sum1 = b.Sum(b.Value.UserId) }))
                 .InnerJoin((a, b) => a.user.Id == b.Key.UserId)
                 .Where((a, b) => a.user.Nickname == "name02" || a.user.Nickname == "name03")
                 .ToList((a, b) => new
                 {
                     user = a.user,
                     rownum = a.rownum,
                     groupby = b,
                     subquery1 = fsql.Select<TwoTablePartitionBy_UserDto>().Where(c => c.Id == a.user.Id).Count(),
                     subquery2 = fsql.Select<TwoTablePartitionBy_UserDto>().Where(c => c.Id == b.Key.UserId).Count(),
                 });
            Assert.Equal(list16.Count, 2);
            Assert.Equal("remark04", list16[0].groupby.Key.Remark);
            Assert.Equal(4, list16[0].groupby.Key.UserId);
            Assert.Equal(4, list16[0].groupby.sum1);
            Assert.Equal(1, list16[0].rownum);
            Assert.Equal(4, list16[0].user.Id);
            Assert.Equal("name02", list16[0].user.Nickname);
            Assert.Equal(0, list16[0].subquery1);
            Assert.Equal(0, list16[0].subquery2);
            Assert.Equal("remark05", list16[1].groupby.Key.Remark);
            Assert.Equal(5, list16[1].groupby.Key.UserId);
            Assert.Equal(5, list16[1].groupby.sum1);
            Assert.Equal(1, list16[1].rownum);
            Assert.Equal(5, list16[1].user.Id);
            Assert.Equal("name03", list16[1].user.Nickname);
            Assert.Equal(0, list16[1].subquery1);
            Assert.Equal(0, list16[1].subquery2);
        }
        class TwoTablePartitionBy_User
        {
            public int Id { get; set; }
            public string Nickname { get; set; }
        }
        class TwoTablePartitionBy_UserExt
        {
            public int UserId { get; set; }
            public string Remark { get; set; }
        }
        class TwoTablePartitionBy_UserDto
        {
            public int Id { get; set; }
            public int rownum { get; set; }
            public string remark { get; set; }
        }
    }
}
