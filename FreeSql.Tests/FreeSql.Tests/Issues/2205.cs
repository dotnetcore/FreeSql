using FreeSql.DataAnnotations;
using System.Globalization;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _2205
    {
        [Fact]
        public void DbTypeUpperCaseIsCultureInvariant()
        {
            var culture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                using (var fsql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.Sqlite, "Data Source=:memory:")
                    .Build())
                {
                    var table = fsql.CodeFirst.GetTableByEntity(typeof(TurkishCultureDbType));
                    Assert.Equal("INT NOT NULL", table.ColumnsByCs["Id"].Attribute.DbType);
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        class TurkishCultureDbType
        {
            [Column(IsPrimary = true, DbType = "int")]
            public int Id { get; set; }
        }
    }
}
