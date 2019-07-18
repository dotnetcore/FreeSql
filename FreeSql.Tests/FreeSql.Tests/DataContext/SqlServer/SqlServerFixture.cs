using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FreeSql.Tests.DataContext.SqlServer
{
    public class SqlServerFixture : IDisposable
    {
        public SqlServerFixture()
        {
            sqlServerLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
             .UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=2")
              //.UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=(localdb)\\mssqllocaldb;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=10")
              .UseAutoSyncStructure(true)
              .UseLazyLoading(true)
              .UseMonitorCommand(t => Trace.WriteLine(t.CommandText))
              .Build());

            // ... initialize data in the test database ...
        }

        public void Dispose()
        {
            // ... clean up test data from the database ...
            ClearDataBase();
        }

        private void ClearDataBase()
        {
            var dataTables = SqlServer.DbFirst.GetTablesByDatabase();
            if (dataTables.Any(item => item.Name == "TopicAddField" && item.Schema == "dbo2"))
            {
                SqlServer.Ado.ExecuteNonQuery("TRUNCATE TABLE dbo2.TopicAddField ");
                SqlServer.Ado.ExecuteNonQuery("DROP TABLE dbo2.TopicAddField");
                SqlServer.Ado.ExecuteNonQuery("DROP SCHEMA dbo2");
            }

            var tempTables = new string[] { "cccccdddwww", "song", "tag", "Song_tag", "tb_alltype", "tb_topic", "tb_topic22",
                "tb_topic22211", "tb_topic111333", "TestTypeInfo", "TestTypeInfo333", "TestTypeParentInfo",
                "TestTypeParentInfo23123", "xxdkdkdk1222", "xxx"};
            foreach (var tempTable in tempTables)
            {
                DeleteTmpTable(dataTables, tempTable);
            }
        }

        private void DeleteTmpTable(List<DatabaseModel.DbTableInfo> dbTables, string deleteTableName, string schemaName = "dbo")
        {
            if (dbTables.Any(item => item.Name.ToLower() == deleteTableName.ToLower() && item.Schema.ToLower() == schemaName.ToLower()))
            {
                SqlServer.Ado.ExecuteNonQuery($"TRUNCATE TABLE {schemaName.ToLower()}.{deleteTableName}");
                SqlServer.Ado.ExecuteNonQuery($"DROP TABLE {schemaName.ToLower()}.{deleteTableName}");
            }
        }

        private Lazy<IFreeSql> sqlServerLazy;
        public IFreeSql SqlServer => sqlServerLazy.Value;
    }
}
