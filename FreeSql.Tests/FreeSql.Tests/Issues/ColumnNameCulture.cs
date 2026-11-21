using System;
using System.Globalization;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Issues
{
    // Column and property names were looked up with the current culture's case-insensitive comparison. Under tr-TR,
    // "TITLE" doesn't match "Title" ("I" lower-cases to the dotless "ı"), so upper-case column names, which Oracle,
    // Firebird and Dameng return for unquoted identifiers, are not mapped to their properties.
    public class ColumnNameCulture
    {
        class CnEntity { public int Id { get; set; } public string Title { get; set; } }

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
        public void AdoQuery_UpperCaseColumn_IsMappedToProperty()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.Sqlite, "Data Source=:memory:")
                    .Build();
                var row = fsql.Ado.Query<CnEntity>("select 5 as ID, 'x' as TITLE").Single();
                Assert.Equal(5, row.Id);
                Assert.Equal("x", row.Title);
            });
        }

        [Fact]
        public void TableInfo_UpperCaseColumnName_IsFound()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.Sqlite, "Data Source=:memory:")
                    .Build();
                var table = fsql.CodeFirst.GetTableByEntity(typeof(CnEntity));
                Assert.True(table.Columns.ContainsKey("TITLE"));
                Assert.True(table.ColumnsByCs.ContainsKey("TITLE"));
            });
        }
    }
}
