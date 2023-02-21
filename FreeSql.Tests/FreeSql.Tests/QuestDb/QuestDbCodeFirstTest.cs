using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;
using FreeSql.Provider.QuestDb.Subtable;
using FreeSql.Tests.QuestDb.QuestDbTestModel;
using Xunit;
using static FreeSql.Tests.QuestDb.QuestDbTest;

namespace FreeSql.Tests.QuestDb
{
    public class QuestDbCodeFirstTest
    {
        [Fact]
        public void Test_SyncStructure()
        {
            fsql.CodeFirst.SyncStructure<QuestDb_Model_Test01>();
            fsql.CodeFirst.SyncStructure(typeof(Topic));
            fsql.CodeFirst.SyncStructure(typeof(Category));
            fsql.CodeFirst.SyncStructure(typeof(CategoryType));
        }

        [Fact]
        public void Test_SyncStructure_Type()
        {
            fsql.CodeFirst.SyncStructure<QuestDb_Model_Type01>();
            var result = fsql.Insert(new QuestDb_Model_Type01()
            {
                TestBool = false,
                TestDecimal = (decimal?)153.02,
                TestDouble = 152.61,
                TestInt = 1,
                TestLong = 1569212,
                TestShort = 2,
                TestString = "string",
                TestTime = DateTime.Now
            }).ExecuteAffrows();
            Assert.Equal(1, result);
        }
    }
}