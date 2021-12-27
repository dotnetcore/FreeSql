using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Xunit;

namespace FreeSql.Tests.Internal
{

    public class UtilsTest
    {
        [Fact]
        public void TestGetDbParamtersByObject()
        {
            var ps = FreeSql.Internal.Utils.
                   GetDbParamtersByObject<DbParameter>("select @p",
                   new { p = (DbParameter)new SqlParameter() { ParameterName = "p", Value = "test" } },
                   "@",
                   (name, type, value) =>
                      {
                          if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
                          var ret = new SqlParameter { ParameterName = $"@{name}", Value = value };
                          return ret;
                      });
            Assert.Single(ps);
            Assert.Equal("test", ps[0].Value);
            Assert.Equal("p", ps[0].ParameterName);
            Assert.Equal(typeof(SqlParameter), ps[0].GetType());


            var ps2 = FreeSql.Internal.Utils.
                   GetDbParamtersByObject<DbParameter>("select @p",
                   new Dictionary<string, DbParameter> { { "p", (DbParameter)new SqlParameter() { ParameterName = "p", Value = "test" } } },
                   "@",
                   (name, type, value) =>
                   {
                       if (value?.Equals(DateTime.MinValue) == true) value = new DateTime(1970, 1, 1);
                       var ret = new SqlParameter { ParameterName = $"@{name}", Value = value };
                       return ret;
                   });
            Assert.Single(ps2);
            Assert.Equal("test", ps2[0].Value);
            Assert.Equal("p", ps2[0].ParameterName);
            Assert.Equal(typeof(SqlParameter), ps2[0].GetType());
        }
    }
}
