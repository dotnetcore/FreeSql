using System;
using Xunit;

namespace FreeSql.Tests.Provider.TDengine.TDengine.TDengineAdo
{
    public class TDengineAdoTest
    {
        IFreeSql fsql => g.tdengine;

        [Fact]
        public void ExecuteConnectTest()
        {
            var executeConnectTest = fsql.Ado.ExecuteConnectTest();
            Assert.True(executeConnectTest);
        }
    }
}
