using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace FreeSql.Tests.QuestDb.Utils
{
    /// <summary>
    /// 单元测试的排序策略
    /// </summary>
    public class TestOrders : ITestCaseOrderer
    {
        /// <summary>
        /// 执行顺序
        /// </summary>
        /// <typeparam name="TTestCase"></typeparam>
        /// <param name="testCases"></param>
        /// <returns></returns>
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            string typeName = typeof(OrderAttribute).AssemblyQualifiedName;
            var result = testCases.ToList();
            result.Sort((x, y) =>
            {
                var xOrder = x.TestMethod.Method.GetCustomAttributes(typeName)?.FirstOrDefault();
                if (xOrder == null)
                {
                    return 0;
                }
                var yOrder = y.TestMethod.Method.GetCustomAttributes(typeName)?.FirstOrDefault();
                if (yOrder == null)
                {
                    return 0;
                }
                var sortX = xOrder.GetNamedArgument<int>("Sort");
                var sortY = yOrder.GetNamedArgument<int>("Sort");
                //按照Order标签上的Sort属性，从小到大的顺序执行
                return sortX - sortY;
            });
            return result;
        }
    }
}
