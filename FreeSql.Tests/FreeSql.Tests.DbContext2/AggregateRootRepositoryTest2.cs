using FreeSql.DataAnnotations;
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
                var code = SelectAggregateRootStaticCode;
            }

            public override ISelect<Order> Select => base.SelectDiy;
        }

        [Fact]
        public void Test1()
        {
            using (var fsql = g.CreateMemory())
            {
                new OrderRepository(fsql, null);

                fsql.Insert(new[]
                {
                    new Tag { Name = "tag1" },
                    new Tag { Name = "tag2" },
                    new Tag { Name = "tag3" },
                    new Tag { Name = "tag4" },
                    new Tag { Name = "tag5" }
                }).ExecuteAffrows();

                var repo = fsql.GetAggregateRootRepository<Order>();
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

                var order2 = repo.Select.Where(a => a.Id == a.Id).First();
                Assert.NotNull(order2);
                Assert.Equal(order.Id, order2.Id);
                Assert.Equal(order.Field2, order2.Field2);
                Assert.NotNull(order2.Extdata);
                Assert.Equal(order.Extdata.Field3, order2.Extdata.Field3);
                Assert.NotNull(order2.Details);
                Assert.Equal(order.Details.Count, order2.Details.Count);
                Assert.Equal(3, order2.Details.Count);
                for (var a = 0; a < 3; a++)
                {
                    Assert.Equal(order.Details[a].Id, order2.Details[a].Id);
                    Assert.Equal(order.Details[a].OrderId, order2.Details[a].OrderId);
                    Assert.Equal(order.Details[a].Field4, order2.Details[a].Field4);
                    Assert.NotNull(order2.Details[a].Extdata);
                    Assert.Equal(order.Details[a].Extdata.Field5, order2.Details[a].Extdata.Field5);
                }
                Assert.NotNull(order2.Tags);
                Assert.Equal(3, order2.Tags.Count);
                Assert.Equal("tag1", order2.Tags[0].Name);
                Assert.Equal("tag2", order2.Tags[1].Name);
                Assert.Equal("tag3", order2.Tags[2].Name);

            }
        }
        class Order
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public string Field2 { get; set; }

            [Navigate(nameof(Id))]
            public OrderExt Extdata { get; set; }
            [Navigate(nameof(OrderDetail.OrderId))]
            public List<OrderDetail> Details { get; set; }
            [Navigate(ManyToMany = typeof(OrderTag))]
            public List<Tag> Tags { get; set; }
        }
        class OrderExt
        {
            [Key]
            public int OrderId { get; set; }
            public string Field3 { get; set; }

            [Navigate(nameof(OrderId))]
            public Order Order { get; set; }
        }
        class OrderDetail
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public int OrderId { get; set; }
            public string Field4 { get; set; }

            [Navigate(nameof(Id))]
            public OrderDetailExt Extdata { get; set; }
        }
        class OrderDetailExt
        {
            [Key]
            public int OrderDetailId { get; set; }
            public string Field5 { get; set; }

            [Navigate(nameof(OrderDetailId))]
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
