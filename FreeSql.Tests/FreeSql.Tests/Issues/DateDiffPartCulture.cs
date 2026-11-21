using System;
using System.Globalization;
using Xunit;

namespace FreeSql.Tests.Issues
{
    // SqlExt.DateDiff upper- or lower-cases the date part with the current culture. Under tr-TR "minute" upper-cases
    // to "MİNUTE" (invalid in MySQL) and "MINUTE" lower-cases to "mınute", which PostgreSQL's branch doesn't know.
    public class DateDiffPartCulture
    {
        class DdEntity { public int Id { get; set; } public DateTime A { get; set; } public DateTime B { get; set; } }

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

        [Fact]
        public void MySql_MinutePart_IsUpperCasedInvariantly()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.MySql, "Data Source=127.0.0.1;Port=1;User ID=root;Password=x;Initial Catalog=t", typeof(FreeSql.MySql.MySqlProvider<>))
                    .Build();
                var sql = fsql.Select<DdEntity>().Where(a => SqlExt.DateDiff("minute", a.A, a.B) > 5).ToSql();
                Assert.Contains("TIMESTAMPDIFF(MINUTE,", sql);
            });
        }

        [Fact]
        public void PostgreSQL_UpperCaseMinutePart_IsRecognized()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.PostgreSQL, "Host=127.0.0.1;Port=1;Username=x;Password=x;Database=t", typeof(FreeSql.PostgreSQL.PostgreSQLProvider<>))
                    .Build();
                var sql = fsql.Select<DdEntity>().Where(a => SqlExt.DateDiff("MINUTE", a.A, a.B) > 5).ToSql();
                Assert.Contains("/ 60)", sql);
            });
        }
    }
}
