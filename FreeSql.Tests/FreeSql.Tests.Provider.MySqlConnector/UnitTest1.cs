using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FreeSql.Tests.MySqlConnector
{

    public class UnitTest1
    {
        public class TestAddEnum
        {
            public Guid Id { get; set; }
            public TestAddEnumType Type { get; set; }
            public PermissionTypeEnum TestType { get; set; }
        }
        public enum TestAddEnumType { 中国人, 日本人 }

        public enum PermissionTypeEnum
        {
            /// <summary>
            /// 菜单
            /// </summary>
            Menu = 1,
            /// <summary>
            /// 接口
            /// </summary>
            Api = 2
        }

        [Fact]
        public void Test1()
        {
            g.mysql.Select<TestAddEnum>().ToList();


            var soc = g.mysql.GetRepository<Jhfw.Models.BankOutlets, int>();
            var item1 = soc.Where(d => d.Id == 1).Include(x => x.OneBankOutlets).Include(d => d.OneBanks).First();
            var item2 = soc.Where(d => d.Id == 1).Include(d => d.OneBanks).Include(x => x.OneBankOutlets).First();
        }
    }
}
