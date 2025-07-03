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
        [Fact]
        public void TestNormalRestUpdate()
        {
            var updateTime = DateTime.Now;
            var updateObj = restFsql.Update<QuestDb_Model_Test01>()
                .Set(q => q.NameUpdate, "UpdateNow")
                //     .Set(q => q.CreateTime, DateTime.Now)   分表的时间不可以随便改
                .Where(q => q.Id == "1");
            var updateSql = updateObj.ToSql();
            Debug.WriteLine(updateSql);
            var sql =
                $@"UPDATE ""QuestDb_Model_Test01"" SET ""NameUpdate"" = 'UpdateNow' 
WHERE (""Id"" = '1')";
            Debug.WriteLine(sql);
            Assert.Equal(updateSql, sql);
            var result = updateObj.ExecuteAffrows();
            Assert.True(result > 0);
        }

        [Fact]
        public void TestRestUpdateByModel()
        {
            var primary = Guid.NewGuid().ToString();
            //先插入
            restFsql.Insert(new QuestDb_Model_Test01()
            {
                Primarys = primary,
                CreateTime = DateTime.Now,
                Activos = 100.21,
                Id = primary,
                IsCompra = true,
                NameInsert = "NameInsert",
                NameUpdate = "NameUpdate"
            }).ExecuteAffrows();
            var updateModel = new QuestDb_Model_Test01
            {
                Primarys = primary,
                Id = primary,
                Activos = 12.65,
            };
            var updateObj = restFsql.Update<QuestDb_Model_Test01>().SetSourceIgnore(updateModel, o => o == null);
            var sql = updateObj.ToSql();
            Debug.WriteLine(sql);
            var result = updateObj.ExecuteAffrows();
            var resultAsync = restFsql.Update<QuestDb_Model_Test01>().SetSourceIgnore(updateModel, o => o == null)
                .ExecuteAffrows();
            Assert.True(result > 0);
            Assert.True(resultAsync > 0);
            Assert.Equal(
                @$"UPDATE ""QuestDb_Model_Test01"" SET ""Primarys"" = '{primary}', ""NameInsert"" = 'NameDefault', ""Activos"" = 12.65 
WHERE (""Id"" = '{primary}')", sql);
        }

        [Fact]
        public async Task TestRestUpdateIgnoreColumnsAsync()
        {
            var primary = Guid.NewGuid().ToString();
            var updateTime = DateTime.Now;
            //先插入
            restFsql.Insert(new QuestDb_Model_Test01()
            {
                Primarys = primary,
                CreateTime = DateTime.Now,
                Activos = 100.21,
                Id = primary,
                IsCompra = true,
                NameInsert = "NameInsert",
                NameUpdate = "NameUpdate"
            }).ExecuteAffrows();
            var updateModel = new QuestDb_Model_Test01
            {
                Id = primary,
                Activos = 12.65,
                IsCompra = true,
                CreateTime = DateTime.Now
            };
            var updateObj = restFsql.Update<QuestDb_Model_Test01>().SetSource(updateModel)
                .IgnoreColumns(q => new { q.Id, q.CreateTime });
            var sql = updateObj.ToSql();
            Debug.WriteLine(sql);
            var result = updateObj.ExecuteAffrows();
            var resultAsync = await restFsql.Update<QuestDb_Model_Test01>().SetSource(updateModel)
                .IgnoreColumns(q => new { q.Id, q.CreateTime }).ExecuteAffrowsAsync();
            Assert.True(result > 0);
            Assert.True(resultAsync > 0);
            Assert.Equal(
                $@"UPDATE ""QuestDb_Model_Test01"" SET ""Primarys"" = NULL, ""NameUpdate"" = NULL, ""NameInsert"" = 'NameDefault', ""Activos"" = 12.65, ""UpdateTime"" = NULL, ""IsCompra"" = True 
WHERE (""Id"" = '{primary}')", sql);
        }

        [Fact]
        public async Task TestUpdateToUpdateAsync()
        {
            //官网demo有问题，暂时放弃此功能
            var result = await restFsql.Select<QuestDb_Model_Test01>().Where(q => q.Id == "IdAsync" && q.NameInsert == null)
                .ToUpdate()
                .Set(q => q.UpdateTime, DateTime.Now)
                .ExecuteAffrowsAsync();
        }
    }
}