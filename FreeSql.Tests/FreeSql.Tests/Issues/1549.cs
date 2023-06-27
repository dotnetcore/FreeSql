using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _1549
    {
        [Fact]
        public void Poco()
        {
            var fsql = g.pgsql;

            RegisterPocoType(typeof(Customer)); //注删 POCO 类型
            fsql.Aop.ParseExpression += (_, e) =>
            {
                //解析 POCO Jsonb   a.Customer.Name
                if (e.Expression is MemberExpression memExp)
                {
                    var parentMemExps = new Stack<MemberExpression>();
                    parentMemExps.Push(memExp);
                    while (true)
                    {
                        switch (memExp.Expression.NodeType)
                        {
                            case ExpressionType.MemberAccess:
                                memExp = memExp.Expression as MemberExpression;
                                if (memExp == null) return;
                                parentMemExps.Push(memExp);
                                break;
                            case ExpressionType.Parameter:
                                var tb = fsql.CodeFirst.GetTableByEntity(memExp.Expression.Type);
                                if (tb == null) return;
                                if (tb.ColumnsByCs.TryGetValue(parentMemExps.Pop().Member.Name, out var trycol) == false) return;
                                if (new[] { typeof(JToken), typeof(JObject), typeof(JArray) }.Contains(trycol.Attribute.MapType.NullableTypeOrThis()) == false) return;
                                var tmpcol = tb.ColumnsByPosition.OrderBy(a => a.Attribute.Name.Length).First();
                                var result = e.FreeParse(Expression.MakeMemberAccess(memExp.Expression, tb.Properties[tmpcol.CsName]));
                                result = result.Replace(tmpcol.Attribute.Name, trycol.Attribute.Name);
                                while (parentMemExps.Any())
                                {
                                    memExp = parentMemExps.Pop();
                                    result = $"{result}->>'{memExp.Member.Name}'";
                                }
                                e.Result = result;
                                return;
                            default:
                                return;
                        }
                    }
                }
            };
            fsql.CodeFirst.SyncStructure<SomeEntity>();
            fsql.CodeFirst.SyncStructure<ExtEntity>();
            fsql.Delete<ExtEntity>().Where("1=1").ExecuteAffrows();
            fsql.Delete<SomeEntity>().Where("1=1").ExecuteAffrows();
            fsql.Insert(new SomeEntity()
            {
                Id = 1,
                Customer = new Customer()
                {
                    Name = "1"
                }
            }).ExecuteAffrows();
            fsql.Insert(new ExtEntity()
            {
                Id = 1,
                Name = "1"
            }).ExecuteAffrows();
            var joes = fsql.Select<SomeEntity>()
                .Include(m => m.Ext)
                .Where(a => a.Id == 1)
                .ToList();
        }
        void RegisterPocoType(Type pocoType)
        {
            var methodJsonConvertDeserializeObject = typeof(JsonConvert).GetMethod("DeserializeObject", new[] { typeof(string), typeof(Type) });
            var methodJsonConvertSerializeObject = typeof(JsonConvert).GetMethod("SerializeObject", new[] { typeof(object), typeof(JsonSerializerSettings) });
            var jsonConvertSettings = JsonConvert.DefaultSettings?.Invoke() ?? new JsonSerializerSettings();
            FreeSql.Internal.Utils.dicExecuteArrayRowReadClassOrTuple[pocoType] = true;
            FreeSql.Internal.Utils.GetDataReaderValueBlockExpressionObjectToStringIfThenElse.Add((LabelTarget returnTarget, Expression valueExp, Expression elseExp, Type type) =>
            {
                return Expression.IfThenElse(
                    Expression.TypeIs(valueExp, pocoType),
                    Expression.Return(returnTarget, Expression.Call(methodJsonConvertSerializeObject, Expression.Convert(valueExp, typeof(object)), Expression.Constant(jsonConvertSettings)), typeof(object)),
                    elseExp);
            });
            FreeSql.Internal.Utils.GetDataReaderValueBlockExpressionSwitchTypeFullName.Add((LabelTarget returnTarget, Expression valueExp, Type type) =>
            {
                if (type == pocoType) return Expression.Return(returnTarget, Expression.TypeAs(Expression.Call(methodJsonConvertDeserializeObject, Expression.Convert(valueExp, typeof(string)), Expression.Constant(type)), type));
                return null;
            });
        }
        [Table(Name = "issues1549_SomeEntity")]
        public class SomeEntity
        {
            [Column(IsPrimary = true)]
            public int Id { get; set; }
            [Column(MapType = typeof(JToken))]
            public Customer Customer { get; set; }

            [Navigate(nameof(Id))]
            public ExtEntity Ext { get; set; }
        }

        public class Customer    // Mapped to a JSON column in the table
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public Order[] Orders { get; set; }
        }

        public class Order       // Part of the JSON column
        {
            public decimal Price { get; set; }
            public string ShippingAddress { get; set; }
        }

        [Table(Name = "issues1549_ExtEntity")]
        public class ExtEntity
        {
            [Column(IsPrimary = true)]
            public int Id { get; set; }
            public string Name { get; set; }
        }

    }

}
