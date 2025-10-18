using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.Tests.QuestDb.QuestDbTestModel;
using Xunit;

namespace FreeSql.Tests.QuestDb
{
    public class QuestDbDbFirstTest
    {
        [Fact]
        public void Test_ExistsTable()
        {
            var existsTable = QuestDbTest.Db.DbFirst.ExistsTable(nameof(QuestDb_Model_Test01));
        }

        [Fact]
        public void Test_GetTablesByDatabase()
        {
            var tablesByDatabase = QuestDbTest.Db.DbFirst.GetTablesByDatabase("");
            tablesByDatabase.ForEach(d =>
            {
                Debug.WriteLine(d.Name);
            });
        }

    }
}
