using System;
using System.Globalization;
using Xunit;

namespace FreeSql.Tests.Issues
{
    // The connection pools read "User Id", "Max pool size" and similar keys from the connection string with
    // RegexOptions.IgnoreCase, which compares with the current culture. Under tr-TR the "I" of "User\s+Id" doesn't
    // match "i", so a connection string written as "user id=..." was refused, and "MAX POOL SIZE=7" was ignored.
    public class ConnectionStringCulture
    {
        static void RunUnderTurkish(Action action)
        {
            var old = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
                action();
            }
            finally { CultureInfo.CurrentCulture = old; }
        }

        [Theory]
        [InlineData("user id=test;password=test;data source=//127.0.0.1:1/test;Pooling=false")]
        [InlineData("UID=test;PWD=test;DATA SOURCE=//127.0.0.1:1/test;Pooling=false")]
        public void Oracle_UserIdIsFoundUnderTurkishCulture(string connectionString)
        {
            RunUnderTurkish(() =>
            {
                using var fsql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.Oracle, connectionString, typeof(FreeSql.Oracle.OracleProvider<>))
                    .Build();
                Assert.NotNull(fsql.Ado.MasterPool);
            });
        }

        [Fact]
        public void Oracle_MaxPoolSizeIsReadUnderTurkishCulture()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.Oracle, "User Id=test;Password=test;Data Source=//127.0.0.1:1/test;MAX POOL SIZE=7", typeof(FreeSql.Oracle.OracleProvider<>))
                    .Build();
                // the pool adds a small increment per connection string to the configured size; without the key it is 100 + 1
                Assert.InRange(fsql.Ado.MasterPool.Policy.PoolSize, 8, 12);
            });
        }

        [Fact]
        public void Dameng_UserIdIsFoundUnderTurkishCulture()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.Dameng, "server=127.0.0.1;port=1;user id=test;password=test;database=test;poolsize=5", typeof(FreeSql.Dameng.DamengProvider<>))
                    .Build();
                Assert.NotNull(fsql.Ado.MasterPool);
            });
        }
    }
}
