using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _1113
    {
        [Fact]
        public void PadLeft()
        {
            using (var freeSql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "Data Source=:memory:;")
                .UseAutoSyncStructure(true)
                .Build())
            {
                freeSql.Aop.CurdBefore += (s, e) =>
                {
                    Trace.WriteLine(e.Sql);
                };

                var company = new Company { Id = Guid.NewGuid(), Code = "CO001" };
                var department = new Department { Id = Guid.NewGuid(), Code = "D001", CompanyId = company.Id };
                var orgnization = new Orgnization { Code = "C001", CompanyId = company.Id };
                freeSql.Insert(company).ExecuteAffrows();
                freeSql.Insert(orgnization).ExecuteAffrows();
                freeSql.Insert(department).ExecuteAffrows();

                var materials = new[]
                {
                    new Material{Code="TEST1",Units=new List<Unit>{new Unit{Code = "KG"}}},
                    new Material{Code="TEST2",Units=new List<Unit>{new Unit{Code = "KG"}}}
                };

                var repo1 = freeSql.GetGuidRepository<Material>();
                repo1.DbContextOptions.EnableCascadeSave = true;
                repo1.Insert(materials);


                var order = new Order
                {
                    Code = "X001",
                    OrgnizationId = orgnization.Id,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem{ ItemCode = "01", MaterialId = materials[0].Id },
                        new OrderItem { ItemCode = "02", MaterialId = materials[1].Id },
                    }
                };

                var repo2 = freeSql.GetGuidRepository<Order>();
                repo2.DbContextOptions.EnableCascadeSave = true;
                repo2.Insert(order);

                // 可以完整加载数据
                var list1 = freeSql.Select<Orgnization>().IncludeMany(t => t.Company.Departments).ToList();
                // 只能查询到Orgnization
                var list2 = freeSql.Select<Order>().IncludeMany(t => t.Orgnization.Company.Departments).ToList();
                //freeSql.Select<Order>().IncludeMany(t => t.OrderItems, then => then.IncludeMany(t => t.Material.Units)).ToList().Dump();
            }
        }


        public class Order
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public Guid OrgnizationId { get; set; }
            [Navigate(nameof(OrgnizationId))]
            public Orgnization Orgnization { get; set; }
            [Navigate(nameof(OrderItem.OrderId))]
            public List<OrderItem> OrderItems { get; set; }
        }

        public class OrderItem
        {
            public Guid Id { get; set; }
            public string ItemCode { get; set; }
            public Guid MaterialId { get; set; }
            public Guid OrderId { get; set; }
            [Navigate(nameof(MaterialId))]
            public Material Material { get; set; }
        }

        public class Orgnization
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public Guid CompanyId { get; set; }
            [Navigate(nameof(CompanyId))]
            public Company Company { get; set; }
        }

        public class Company
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            [Navigate(nameof(Department.CompanyId))]
            public List<Department> Departments { get; set; }
        }

        public class Department
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public Guid CompanyId { get; set; }
        }

        public class Material
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            [Navigate(nameof(Unit.MaterialId))]
            public List<Unit> Units { get; set; }
        }

        public class Unit
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public Guid MaterialId { get; set; }
        }

    }

}
