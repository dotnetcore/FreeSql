using FreeSql.DataAnnotations;
using FreeSql.Internal.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text.Json.Nodes;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _1113
    {
        [Fact]
        public void IncludeLevelTest()
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
                // 使用扩展方法加载到指定层级
                var list3 = freeSql.Select<Order>().IncludeLevel(3).ToList();
            }
        }

        [Fact]
        public void DynamicLinqTest()
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

                freeSql.Insert(
                        Enumerable.Range(1, 100)
                            .Select(i => new Topic { Title = $"new topic {i}", Clicks = 100 })
                            .ToList())
                    .ExecuteAffrows();

                // 常规用法
                var list1 = freeSql.Select<Topic>()
                    .Where(t => t.Title.StartsWith("new topic 1"))
                    .ToList();

                // 借助 DynamicExpressionParser
                var list2 = freeSql.Select<Topic>()
                  .WhereDynamicLinq("Title.StartsWith(\"new topic 1\")")
                  .ToList();

                // 拓展 DynamicFilter
                var dynmaicFilterInfo = new DynamicFilterInfo
                {
                    Field = $"{nameof(DynamicLinqCustom.DynamicLinq)} {typeof(DynamicLinqCustom).FullName},{typeof(DynamicLinqCustom).Assembly.FullName}",
                    Operator = DynamicFilterOperator.Custom,
                    Value = "Title.StartsWith(\"new topic 1\")",
                };
                var list3 = freeSql.Select<Topic>()
                  .WhereDynamicFilter(dynmaicFilterInfo)
                  .ToList();
            }
        }

        [Fact]
        public void WhereByPropertyTest()
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

                // 根据导航属性过滤数据
                //var list1 = freeSql.Select<Order>().Where(t => t.OrderItems.Any(t1 => t1.Material.Units.Any(t2 => t2.Code == "KG"))).ToList();
                var filterInfo1 = new DynamicFilterInfo
                {
                    Field = "Code",
                    Operator = DynamicFilterOperator.Eq,
                    Value = "KG",
                };
                var list1 = freeSql.Select<Order>().Where(t => t.OrderItems.Any(t1 => t1.Material.Units.AsSelect().WhereDynamicFilter(filterInfo1).Any())).ToList();

                // 导航属性如果是 OneToOne 或者 ManyToOne 默认支持
                var filterInfo2 = new DynamicFilterInfo
                {
                    Field = "Orgnization.Company.Code",
                    Operator = DynamicFilterOperator.Eq,
                    Value = "CO001",
                };
                //var list2 = freeSql.Select<Order>().Where(t => t.Orgnization.Company.Code == "CO001").ToList();
                var list2 = freeSql.Select<Order>().WhereDynamicFilter(filterInfo2).ToList();

                // 实现效果 OrderItems.Material.Units.Code == "KG"
                var list3 = freeSql.Select<Order>().Where("OrderItems.Material.Units.Code", DynamicFilterOperator.Eq, "KG").ToList();

                // 实现效果 OrderItems.Material.Code == "TEST1"
                // Error SQL:
                // SELECT a."Id", a."Code", a."OrgnizationId"
                // FROM "Order" a
                // WHERE (exists(SELECT 1
                //     FROM "OrderItem" a
                //     LEFT JOIN "Material" a__Material ON a__Material."Id" = a."MaterialId"
                //     WHERE (a__Material."Code" = 'TEST1') AND (a."OrderId" = a."Id")
                //     limit 0,1))
                var list4 = freeSql.Select<Order>().Where("OrderItems.Material.Code", DynamicFilterOperator.Eq, "TEST1").ToList();


                // 拓展 DynamicFilter
                var dynmaicFilterInfo = new DynamicFilterInfo
                {
                    Field = $"{nameof(DynamicLinqCustom.WhereNavigation)} {typeof(DynamicLinqCustom).FullName},{typeof(DynamicLinqCustom).Assembly.FullName}",
                    Operator = DynamicFilterOperator.Custom,
                    Value = JsonConvert.SerializeObject(new DynamicFilterInfo { Field = "OrderItems.Material.Units.Code", Operator = DynamicFilterOperator.Eq, Value = "KG" }),
                };
                var list5 = freeSql.Select<Order>()
                  .WhereDynamicFilter(dynmaicFilterInfo)
                  .ToList();
            }
        }

        public class Topic
        {
            [Column(IsPrimary = true)] public Guid Id { get; set; }
            public string Title { get; set; }
            public int Clicks { get; set; }
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

    public class DynamicLinqCustom
    {
        [DynamicFilterCustom]
        public static LambdaExpression DynamicLinq(object sender, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) value = "1==2";
            ParameterExpression t = Expression.Parameter(sender.GetType().GetGenericArguments()[0], "t");
            var exp = DynamicExpressionParser.ParseLambda(new ParameterExpression[] { t }, typeof(bool), value);
            return exp;
        }

        [DynamicFilterCustom]
        public static LambdaExpression WhereNavigation(object sender, string value)
        {
            var filter = Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicFilterInfo>(value);
            var method = typeof(FreeSql_1113_Extensions).GetMethods().First(a => a.Name == "GetWhereExpression" && a.GetParameters().Length == 2);
            method = method.MakeGenericMethod(sender.GetType().GenericTypeArguments[0]);
            var exp = method.Invoke(null, new[] { sender, filter });
            return exp as LambdaExpression;
        }
    }

    static class FreeSql_1113_Extensions
    {
        public static ISelect<T1> Where<T1>(this ISelect<T1> select, string field, DynamicFilterOperator filterOperator, object value)
        {
            var filter = new DynamicFilterInfo { Field = field, Operator = filterOperator, Value = value };
            Expression<Func<T1, bool>> exp = GetWhereExpression(select, filter);
            return select.Where(exp);
        }

        public static Expression<Func<T1, bool>> GetWhereExpression<T1>(ISelect<T1> select, DynamicFilterInfo filter)
        {
            var properties = filter.Field.Split('.');
            var tree = TableRefTree.GetTableRefTree(select, properties.Length, properties);

            // 检索
            var treeList = GetTreeList(tree).ToList();
            var deepest = treeList.Last();
            var collectionNodes = treeList.Where(a => a.TableRef.RefType == TableRefType.OneToMany || a.TableRef.RefType == TableRefType.ManyToMany).ToList();

            if (deepest.Level != properties.Length)
                throw new Exception($"当前类型{typeof(T1)}导航属性{filter.Field}匹配检索失败");
            if (collectionNodes.Count == 0)
                throw new Exception($"当前类型{typeof(T1)}导航属性{filter.Field}不包含{TableRefType.OneToMany}或者{nameof(TableRefType.ManyToMany)}关系");

            var selectMethod = typeof(FreeSqlGlobalExtensions).GetMethods().First(a => a.ContainsGenericParameters && a.GetParameters().Count() == 1);
            var parameterExpression = Expression.Parameter(typeof(T1), "t");
            var manyLevel = 0;
            var exp = GetWhereExpression(select, deepest, filter, properties, null, ref manyLevel);
            return exp as Expression<Func<T1, bool>>;
        }

        private static Expression GetWhereExpression<T1>(ISelect<T1> select, TableRefTree deepest, DynamicFilterInfo filterInfo, string[] properties, Expression body, ref int manyLevel)
        {
            while (deepest.Parent != null && deepest.TableRef.RefType != TableRefType.ManyToMany && deepest.TableRef.RefType != TableRefType.OneToMany)
            {
                deepest = deepest.Parent;
            }
            manyLevel++;
            if (manyLevel == 1)
            {
                filterInfo = new DynamicFilterInfo
                {
                    Field = string.Join(".", properties.Skip(deepest.Level - 1)),
                    Operator = filterInfo.Operator,
                    Value = filterInfo.Value,
                };
                deepest = deepest.Parent;
                return GetWhereExpression(select, deepest, filterInfo, properties, body, ref manyLevel);
            }
            if (body == null)
            {
                body = Expression.Parameter(deepest.TableInfo.Type, $"t{deepest.Level}");
                var sub = deepest;
                do
                {
                    sub = sub.Subs.First();
                    body = Expression.Property(body, sub.TableRef.Property);
                } while (sub.TableRef.RefType != TableRefType.ManyToMany && sub.TableRef.RefType != TableRefType.OneToMany);

                var selectMethod = typeof(FreeSqlGlobalExtensions).GetMethods().First(a => a.Name == "AsSelect" && a.ContainsGenericParameters && a.GetParameters().Count() == 1).MakeGenericMethod(sub.TableRef.RefEntityType);
                body = Expression.Call(null, selectMethod, body);
                var asMethod = typeof(ISelect<>).MakeGenericType(sub.TableRef.RefEntityType).GetMethod("As");
                var constExpression = Expression.Constant($"t{deepest.Level}");
                body = Expression.Call(body, asMethod, constExpression);
                var whereDynamicFilterMethod = typeof(ISelect0<,>).MakeGenericType(typeof(ISelect<>).MakeGenericType(sub.TableRef.RefEntityType), sub.TableRef.RefEntityType).GetMethod("WhereDynamicFilter");
                body = Expression.Call(body, whereDynamicFilterMethod, Expression.Constant(filterInfo));
                var anyMethod = typeof(ISelect0<,>).MakeGenericType(typeof(ISelect<>).MakeGenericType(sub.TableRef.RefEntityType), sub.TableRef.RefEntityType).GetMethod("Any");
                body = Expression.Call(body, anyMethod);

                deepest = deepest.Parent;
            }
            else
            {
                Expression subBody = Expression.Parameter(deepest.TableInfo.Type, $"t{deepest.Level}");
                var sub = deepest;
                do
                {
                    sub = sub.Subs.First();
                    subBody = Expression.Property(subBody, sub.TableRef.Property);
                } while (sub.TableRef.RefType != TableRefType.ManyToMany && sub.TableRef.RefType != TableRefType.OneToMany);

                var selectMethod = typeof(FreeSqlGlobalExtensions).GetMethods().First(a => a.Name == "AsSelect" && a.ContainsGenericParameters && a.GetParameters().Count() == 1).MakeGenericMethod(sub.TableRef.RefEntityType);
                subBody = Expression.Call(null, selectMethod, subBody);
                var anyMethod = typeof(ISelect<>).MakeGenericType(sub.TableRef.RefEntityType).GetMethod("Any");

                var funcType = typeof(Func<,>).MakeGenericType(sub.TableRef.RefEntityType, typeof(bool));
                var parameterBody = Expression.Parameter(sub.TableInfo.Type, $"t{sub.Level}");
                var lambda = Expression.Lambda(funcType, body, parameterBody);
                body = Expression.Call(subBody, anyMethod, lambda);

                deepest = deepest.Parent;
            }

            if (deepest == null)
            {
                var funcType = typeof(Func<,>).MakeGenericType(typeof(T1), typeof(bool));
                var parameterBody = Expression.Parameter(typeof(T1), "t1");
                return Expression.Lambda(funcType, body, parameterBody);
            }
            else
            {
                return GetWhereExpression(select, deepest, filterInfo, properties, body, ref manyLevel);
            }
        }

        private static IEnumerable<TableRefTree> GetTreeList(TableRefTree tree)
        {
            if (tree.Subs == null || tree.Subs.Count == 0) yield break;
            yield return tree.Subs[0];
            foreach (var sub in GetTreeList(tree.Subs[0]))
            {
                yield return sub;
            }
        }

        public static ISelect<T1> WhereDynamicLinq<T1>(this ISelect<T1> select, string expression)
        {
            ParameterExpression t = Expression.Parameter(typeof(T1), "t");
            var exp = DynamicExpressionParser.ParseLambda(new ParameterExpression[] { t }, typeof(bool), expression);
            return select.Where((Expression<Func<T1, bool>>)exp);
        }

        public static ISelect<T1> IncludeLevel<T1>(this ISelect<T1> select, int level)
        {
            var tree = TableRefTree.GetTableRefTree(select, level);
            return select.IncludeLevel(level, tree);
        }
        private static ISelect<T1> IncludeLevel<T1>(this ISelect<T1> select, int level, TableRefTree tree, ParameterExpression parameterExpression = null, MemberExpression bodyExpression = null)
        {
            var includeMethod = select.GetType().GetMethod("Include");
            var includeManyMethod = select.GetType().GetMethod("IncludeMany");
            parameterExpression ??= Expression.Parameter(tree.TableInfo.Type, "t");
            foreach (var sub in tree.Subs)
            {
                switch (sub.TableRef.RefType)
                {
                    case TableRefType.ManyToOne:
                    case TableRefType.OneToOne:
                        {
                            var body = bodyExpression == null ? Expression.Property(parameterExpression, sub.TableRef.Property) : Expression.Property(bodyExpression, sub.TableRef.Property);
                            if (sub.Subs.Count == 0)
                            {
                                var funcType = typeof(Func<,>).MakeGenericType(parameterExpression.Type, sub.TableRef.RefEntityType);
                                var navigateSelector = Expression.Lambda(funcType, body, parameterExpression);
                                includeMethod.MakeGenericMethod(sub.TableRef.RefEntityType).Invoke(select, new object[] { navigateSelector });
                            }
                            else
                            {
                                select.IncludeLevel(level, sub, parameterExpression, body);
                            }
                        }
                        break;
                    case TableRefType.ManyToMany:
                    case TableRefType.OneToMany:
                        {
                            var body = bodyExpression == null ? Expression.Property(parameterExpression, sub.TableRef.Property) : Expression.Property(bodyExpression, sub.TableRef.Property);
                            object then = null;
                            if (sub.Subs.Count > 0)
                            {
                                //var thenSelectType = select.GetType().GetGenericTypeDefinition().MakeGenericType(sub.TableRef.RefEntityType);
                                var thenSelectType = typeof(ISelect<>).MakeGenericType(sub.TableRef.RefEntityType);
                                var thenType = typeof(Action<>).MakeGenericType(thenSelectType);
                                var thenParameter = Expression.Parameter(thenSelectType, "then");
                                var thenMethod = typeof(FreeSql_1113_Extensions).GetMethod(nameof(IncludeLevel)).MakeGenericMethod(sub.TableRef.RefEntityType);
                                var thenLevelConst = Expression.Constant(level - sub.Level + 1);
                                var thenBody = Expression.Call(null, thenMethod, thenParameter, thenLevelConst);
                                var thenExpression = Expression.Lambda(thenType, thenBody, thenParameter);
                                then = thenExpression.Compile();
                            }
                            var funcType = typeof(Func<,>).MakeGenericType(parameterExpression.Type, typeof(IEnumerable<>).MakeGenericType(sub.TableRef.RefEntityType));
                            var navigateSelector = Expression.Lambda(funcType, body, parameterExpression);
                            includeManyMethod.MakeGenericMethod(sub.TableRef.RefEntityType).Invoke(select, new object[] { navigateSelector, then });
                        }
                        break;
                }
            }

            return select;
        }

        static bool CheckRepeat(this TableRefTree tree, List<Type> types = null)
        {
            if (tree.Parent == null) return false;
            if (types == null)
            {
                types = new List<System.Type> { tree.TableInfo.Type };
                return CheckRepeat(tree.Parent, types);
            }
            return types.Contains(tree.TableInfo.Type) || CheckRepeat(tree.Parent, types);
        }

        class TableRefTree
        {
            public int Level { get; set; }
            public TableRefTree Parent { get; set; }
            public TableInfo TableInfo { get; set; }
            public TableRef TableRef { get; set; }
            public List<TableRefTree> Subs { get; set; }

            public static TableRefTree GetTableRefTree<T1>(ISelect<T1> select, int maxLevel, string[] properties = null)
            {
                var orm = select.GetType().GetField("_orm").GetValue(select) as IFreeSql;
                var tableInfo = orm.CodeFirst.GetTableByEntity(typeof(T1));
                var tree = new TableRefTree()
                {
                    Level = 1,
                    TableInfo = tableInfo,
                };
                tree.Subs = GetTableRefTree(orm, tree, maxLevel, properties).ToList();
                return tree;
            }

            public static IEnumerable<TableRefTree> GetTableRefTree(IFreeSql orm, TableRefTree tree, int maxLevel, string[] properties = null)
            {
                if (tree.Level > maxLevel) yield break;
                var tableRefs = tree.TableInfo.Properties.Where(property => properties == null || string.Equals(properties[tree.Level - 1], property.Key, StringComparison.OrdinalIgnoreCase)).Select(a => tree.TableInfo.GetTableRef(a.Key, false)).Where(a => a != null).ToList();
                foreach (var tableRef in tableRefs)
                {
                    var tableInfo = orm.CodeFirst.GetTableByEntity(tableRef.RefEntityType);
                    var sub = new TableRefTree()
                    {
                        Level = tree.Level + 1,
                        TableInfo = tableInfo,
                        TableRef = tableRef,
                        Parent = tree,
                    };

                    // 排除重复类型
                    if (sub.CheckRepeat()) continue;

                    sub.Subs = GetTableRefTree(orm, sub, maxLevel, properties).ToList();
                    yield return sub;
                }
            }
        }
    }
}
