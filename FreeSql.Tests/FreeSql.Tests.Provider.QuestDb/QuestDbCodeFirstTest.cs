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
            Db.CodeFirst.SyncStructure<QuestDb_Model_Test01>();
            Db.CodeFirst.SyncStructure(typeof(Topic));
            Db.CodeFirst.SyncStructure(typeof(Category));
            Db.CodeFirst.SyncStructure(typeof(CategoryType));
        }

        [Fact]
        public void Test_SyncStructure_Type()
        {
            Db.CodeFirst.SyncStructure<QuestDb_Model_Type01>();
            var result = Db.Insert(new QuestDb_Model_Type01()
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