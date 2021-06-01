using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FreeSql.Tests.Provider.MySqlConnector
{
    public class FreeSqlMySqlConnectorGlobalExtensionsTest
    {
        class BulkCopyValue
        {
            public Guid id { get; set; }
            public DateTime createtime { get; set; }
        }
        [Fact]
        public async Task ExecuteMySqlBulkCopyAsync()
        {
            var fsql = g.mysql;
            fsql.CodeFirst.SyncStructure<BulkCopyValue>();

            List<BulkCopyValue> bulkCopyValues = new List<BulkCopyValue>();
            for (var i = 0; i < 1000; i++)
            {
                bulkCopyValues.Add(new BulkCopyValue() { createtime = DateTime.Now });
            }
            await fsql.Insert<BulkCopyValue>().AppendData(bulkCopyValues).ExecuteMySqlBulkCopyAsync();
        }
    }
}
