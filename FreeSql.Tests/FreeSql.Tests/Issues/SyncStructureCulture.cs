using System;
using System.Globalization;
using FreeSql.DataAnnotations;
using Xunit;

namespace FreeSql.Tests.Issues
{
    // CodeFirst upper-cased the column types it read from the database with the current culture and compared them to
    // the entity's types with the current culture's case-insensitive comparison. Under tr-TR, "datetime" upper-cases to
    // "DATETİME", which doesn't match "DATETIME", so a table created with lower-case type names was rebuilt (copied,
    // dropped and renamed) on every structure sync.
    public class SyncStructureCulture
    {
        [Table(Name = "sync_structure_culture")]
        class SscEntity
        {
            [Column(IsPrimary = true)] public int Id { get; set; }
            public DateTime Created { get; set; }
            public decimal Price { get; set; }
        }

        [Fact]
        public void LowerCaseColumnTypes_AreNotChanged_UnderTurkish()
        {
            var old = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
                using var fsql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.Sqlite, "Data Source=:memory:")
                    .Build();
                fsql.Ado.ExecuteNonQuery("create table sync_structure_culture (Id integer NOT NULL, Created datetime NOT NULL, Price decimal(10,2) NOT NULL, PRIMARY KEY(Id))");

                Assert.Equal("", fsql.CodeFirst.GetComparisonDDLStatements<SscEntity>() ?? "");
            }
            finally { CultureInfo.CurrentCulture = old; }
        }
    }
}
