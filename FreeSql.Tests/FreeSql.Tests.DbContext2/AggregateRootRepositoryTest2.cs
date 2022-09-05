using FreeSql.DataAnnotations;
using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FreeSql.Tests.DbContext2
{
    public class AggregateRootRepositoryTest2
    {
        class OrderRepository : AggregateRootRepository<Order>
        {
            public OrderRepository(IFreeSql fsql, UnitOfWorkManager uowManager) : base(uowManager?.Orm ?? fsql)
            {
            }

            public override ISelect<Order> Select => base.SelectDiy;
        }

        [Fact]
        public void Test1()
        {
            using (var fsql = g.CreateMemory())
            {
                new OrderRepository(fsql, null);


                fsql.Aop.CommandAfter += (_, e) =>
                {
                    if (e.Exception is DbUpdateVersionException)
                    {
                        throw new Exception(e.Exception.Message, e.Exception);
                    }
                };

                var code = AggregateRootUtils.GetAutoIncludeQueryStaicCode(null, fsql, typeof(Order));
                var code1 = AggregateRootUtils.GetAutoIncludeQueryStaicCode("code1", fsql, typeof(Order));
                var code2 = AggregateRootUtils.GetAutoIncludeQueryStaicCode("code2", fsql, typeof(Order));
                Assert.Equal(@"//fsql.Select<Order>()
SelectDiy
    .Include(a => a.Extdata)
    .IncludeMany(a => a.Details, then => then
        .Include(b => b.Extdata))
    .IncludeMany(a => a.Tags)", code);
                Assert.Equal(@"//fsql.Select<Order>()
SelectDiy
    .Include(a => a.Extdata)
    .IncludeMany(a => a.Details, then => then
        .Include(b => b.Extdata))
    .IncludeMany(a => a.Tags, then => then
        .IncludeMany(b => b.Orders))", code1);
                Assert.Equal(@"//fsql.Select<Order>()
SelectDiy
    .IncludeMany(a => a.Details)
    .IncludeMany(a => a.Tags)", code2);

                fsql.Insert(new[]
                {
                    new Tag { Name = "tag1" },
                    new Tag { Name = "tag2" },
                    new Tag { Name = "tag3" },
                    new Tag { Name = "tag4" },
                    new Tag { Name = "tag5" }
                }).ExecuteAffrows();

                var repo = fsql.GetAggregateRootRepository<Order>();
                var affrows = 0;
                Order order2 = null;

                LocalTest();
                void LocalTest()
                {
                    var order = new Order
                    {
                        Field2 = "field2",
                        Extdata = new OrderExt { Field3 = "field3" },
                        Details = new List<OrderDetail>
                    {
                        new OrderDetail { Field4 = "field4_01", Extdata = new OrderDetailExt { Field5 = "field5_01" } },
                        new OrderDetail { Field4 = "field4_02", Extdata = new OrderDetailExt { Field5 = "field5_02" } },
                        new OrderDetail { Field4 = "field4_03", Extdata = new OrderDetailExt { Field5 = "field5_03" } },
                    },
                        Tags = fsql.Select<Tag>().Where(a => new[] { 1, 2, 3 }.Contains(a.Id)).ToList()
                    };
                    repo.Insert(order); //级联插入

                    order2 = repo.Select.Where(a => a.Id == a.Id).First();
                    Assert.NotNull(order2);
                    Assert.Equal(order.Id, order2.Id);
                    Assert.Equal(order.Field2, order2.Field2);
                    Assert.NotNull(order2.Extdata);
                    Assert.Equal(order.Extdata.Field3, order2.Extdata.Field3);
                    Assert.NotNull(order2.Details);
                    Assert.Equal(order.Details.Count, order2.Details.Count);
                    for (var a = 0; a < order.Details.Count; a++)
                    {
                        Assert.Equal(order.Details[a].Id, order2.Details[a].Id);
                        Assert.Equal(order.Details[a].OrderId, order2.Details[a].OrderId);
                        Assert.Equal(order.Details[a].Field4, order2.Details[a].Field4);
                        Assert.NotNull(order2.Details[a].Extdata);
                        Assert.Equal(order.Details[a].Extdata.Field5, order2.Details[a].Extdata.Field5);
                    }
                    Assert.NotNull(order2.Tags);
                    Assert.Equal(order.Tags.Count, order2.Tags.Count);
                    for (var a = 0; a < order.Tags.Count; a++)
                        Assert.Equal(order.Tags[a].Id, order2.Tags[a].Id);
                    Assert.Equal("tag1", order2.Tags[0].Name);
                    Assert.Equal("tag2", order2.Tags[1].Name);
                    Assert.Equal("tag3", order2.Tags[2].Name);

                    order.Tags.Add(new Tag { Id = 4 });
                    order.Details.RemoveAt(1);
                    order.Details[0].Extdata.Field5 = "field5_01_01";
                    order.Field2 = "field2_02";
                    affrows = repo.Update(order);
                    Assert.Equal(5, affrows);

                    order2 = repo.Select.Where(a => a.Id == a.Id).First();
                    Assert.NotNull(order2);
                    Assert.Equal(order.Id, order2.Id);
                    Assert.Equal("field2_02", order2.Field2);
                    Assert.NotNull(order2.Extdata);
                    Assert.Equal(order.Extdata.Field3, order2.Extdata.Field3);
                    Assert.NotNull(order2.Details);
                    Assert.Equal(order.Details.Count, order2.Details.Count);
                    Assert.Equal(2, order2.Details.Count);

                    Assert.Equal(order.Details[0].Id, order2.Details[0].Id);
                    Assert.Equal(order.Details[0].OrderId, order2.Details[0].OrderId);
                    Assert.Equal("field4_01", order2.Details[0].Field4);
                    Assert.NotNull(order2.Details[0].Extdata);
                    Assert.Equal("field5_01_01", order2.Details[0].Extdata.Field5);
                    Assert.Equal(order.Details[1].Id, order2.Details[1].Id);
                    Assert.Equal(order.Details[1].OrderId, order2.Details[1].OrderId);
                    Assert.Equal("field4_03", order2.Details[1].Field4);
                    Assert.NotNull(order2.Details[1].Extdata);
                    Assert.Equal("field5_03", order2.Details[1].Extdata.Field5);

                    Assert.NotNull(order2.Tags);
                    Assert.Equal(4, order2.Tags.Count);
                    Assert.Equal("tag1", order2.Tags[0].Name);
                    Assert.Equal("tag2", order2.Tags[1].Name);
                    Assert.Equal("tag3", order2.Tags[2].Name);
                    Assert.Equal("tag4", order2.Tags[3].Name);
                }

                affrows = repo.Delete(order2);
                Assert.Equal(10, affrows);
                Assert.False(fsql.Select<Order>().Where(a => a.Id == 1).Any());
                Assert.False(fsql.Select<OrderExt>().Where(a => a.OrderId == 1).Any());
                Assert.False(fsql.Select<OrderDetail>().Where(a => a.OrderId == 1).Any());
                Assert.False(fsql.Select<OrderTag>().Where(a => a.OrderId == 1).Any());
                Assert.False(fsql.Select<OrderDetailExt>().Any());
                var tags = fsql.Select<Tag>().ToList();
                Assert.Equal(5, tags.Count);
                Assert.Equal("tag1", tags[0].Name);
                Assert.Equal("tag2", tags[1].Name);
                Assert.Equal("tag3", tags[2].Name);
                Assert.Equal("tag4", tags[3].Name);
                Assert.Equal("tag5", tags[4].Name);

                LocalTest();
                var deleted = repo.DeleteCascadeByDatabase(a => a.Id == 2);
                Assert.NotNull(deleted);
                Assert.Equal(10, deleted.Count);
                Assert.False(fsql.Select<Order>().Where(a => a.Id == 2).Any());
                Assert.False(fsql.Select<OrderExt>().Where(a => a.OrderId == 2).Any());
                Assert.False(fsql.Select<OrderDetail>().Where(a => a.OrderId == 2).Any());
                Assert.False(fsql.Select<OrderTag>().Where(a => a.OrderId == 2).Any());
                Assert.False(fsql.Select<OrderDetailExt>().Any());
                tags = fsql.Select<Tag>().ToList();
                Assert.Equal(5, tags.Count);
                Assert.Equal("tag1", tags[0].Name);
                Assert.Equal("tag2", tags[1].Name);
                Assert.Equal("tag3", tags[2].Name);
                Assert.Equal("tag4", tags[3].Name);
                Assert.Equal("tag5", tags[4].Name);

            }
        }
        class Order
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public string Field2 { get; set; }

            [AggregateRootBoundary("code2", Break = true)]
            public OrderExt Extdata { get; set; }
            [Navigate(nameof(OrderDetail.OrderId)), AggregateRootBoundary("code2", BreakThen = true)]
            public List<OrderDetail> Details { get; set; }
            [Navigate(ManyToMany = typeof(OrderTag)), AggregateRootBoundary("code1", Break = false, BreakThen = false)]
            public List<Tag> Tags { get; set; }
        }
        class OrderExt
        {
            [Key]
            public int OrderId { get; set; }
            public string Field3 { get; set; }

            public Order Order { get; set; }
        }
        class OrderDetail
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public int OrderId { get; set; }
            public string Field4 { get; set; }

            [AggregateRootBoundary("name1", Break = true)]
            public OrderDetailExt Extdata { get; set; }
        }
        class OrderDetailExt
        {
            [Key]
            public int OrderDetailId { get; set; }
            public string Field5 { get; set; }

            public OrderDetail OrderDetail { get; set; }
        }
        class OrderTag
        {
            [Key]
            public int OrderId { get; set; }
            [Key]
            public int TagId { get; set; }

            [Navigate(nameof(OrderId))]
            public Order Order { get; set; }
            [Navigate(nameof(TagId))]
            public Tag Tag { get; set; }
        }
        class Tag
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public string Name { get; set; }

            [Navigate(ManyToMany = typeof(OrderTag))]
            public List<Order> Orders { get; set; }
        }
    }
}
