using System;
using System.Globalization;
using FreeSql.Extensions.ZeroEntity;
using Xunit;

namespace FreeSql.Tests.Issues
{
    // ZeroDbContext matched the Where/WhereColumns operator after lower-casing it with the current culture. Under tr-TR,
    // "IN" lower-cases to "ın" (dotless i), so "IN", "NOT IN", "LIKE" and "CONTAINS" threw "未实现".
    public class ZeroEntityOperatorCulture
    {
        [Theory]
        [InlineData("IN", "Id")]
        [InlineData("NOT IN", "Id")]
        [InlineData("LIKE", "Title")]
        [InlineData("CONTAINS", "Title")]
        public void UpperCaseOperator_IsRecognized_UnderTurkish(string op, string field)
        {
            var old = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
                using var fsql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.Sqlite, "Data Source=:memory:")
                    .Build();
                var schema = new TableDescriptor { Name = "zero_operator_culture" };
                schema.Columns.Add(new TableDescriptor.ColumnDescriptor { Name = "Id", MapType = typeof(int), IsPrimary = true });
                schema.Columns.Add(new TableDescriptor.ColumnDescriptor { Name = "Title", MapType = typeof(string) });
                var ctx = new ZeroDbContext(fsql, new[] { schema });

                object value = field == "Id" ? new[] { 1, 2 } : "x";
                var sql = ctx.SelectNoTracking("zero_operator_culture").Where(field, op, value).ToSql();

                Assert.Contains(field == "Id" ? ") in (" : " LIKE '%x%'", sql);
            }
            finally { CultureInfo.CurrentCulture = old; }
        }
    }
}
