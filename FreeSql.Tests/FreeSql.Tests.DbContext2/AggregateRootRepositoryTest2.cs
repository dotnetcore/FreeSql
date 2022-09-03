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

                var repo = fsql.GetAggregateRootRepository<Order>();
                var order = new Order
                {
                    Field2 = "field2",
                    Extdata = new OrderExt { Field3 = "field3" },
                    Details = new List<OrderDetail>
                    {
                        [0] = new OrderDetail { Field4 = "field4_01", Extdata = new OrderDetailExt { Field5 = "field5_01" } },
                        [0] = new OrderDetail { Field4 = "field4_02", Extdata = new OrderDetailExt { Field5 = "field5_02" } },
                        [0] = new OrderDetail { Field4 = "field4_03", Extdata = new OrderDetailExt { Field5 = "field5_03" } },
                    },
                    Tags = fsql.Select<Tag>().Where(a => new[] { 1, 2, 3 }.Contains(a.Id)).ToList()
                };
                repo.Insert(order); //级联插入
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

            [Navigate(ManyToMany = typeof(Order))]
            public List<Order> Orders { get; set; }
        }
    }
}
