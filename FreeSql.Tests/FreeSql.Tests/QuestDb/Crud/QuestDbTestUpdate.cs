using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.Tests.QuestDb.QuestDbTestModel;
using FreeSql.Tests.QuestDb.Utils;
using Xunit;
using static FreeSql.Tests.QuestDb.QuestDbTest;

namespace FreeSql.Tests.QuestDb.Crud
{
    [TestCaseOrderer("FreeSql.Tests.QuestDb.Utils.TestOrders", "FreeSql.Tests")]
    public class QuestDbTestUpdate
    {
        //多线程以及questdb问题转移至 insert中测试
    }
}