using System;
using FreeSql.Internal;
using System.Globalization;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class NameConvertCulture
    {
        class NcLower { public int Id { get; set; } public string Image { get; set; } public string ImageUrl { get; set; } }
        class NcUnderscore { public int Id { get; set; } public string Image { get; set; } public string ImageUrl { get; set; } }
        class NcUpper { public int Id { get; set; } public string Title { get; set; } public string Image { get; set; } public string ImageUrl { get; set; } }
        class NcSync { public int Id { get; set; } public string Image { get; set; } public string ImageUrl { get; set; } }

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

        static IFreeSql Create(Func<FreeSqlBuilder, FreeSqlBuilder> cfg) =>
            cfg(new FreeSqlBuilder().UseConnectionString(DataType.Sqlite, "Data Source=:memory:", typeof(FreeSql.Sqlite.SqliteProvider<>))).Build();

        [Fact]
        public void ToLowerNameConvert_IsCultureInvariant()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = Create(b => b.UseNameConvert(NameConvertType.ToLower));
                var table = fsql.CodeFirst.GetTableByEntity(typeof(NcLower));
                Assert.Equal("id", table.ColumnsByCs["Id"].Attribute.Name);
                Assert.Equal("image", table.ColumnsByCs["Image"].Attribute.Name);
                Assert.Equal("nclower", table.DbName);
            });
        }

        [Fact]
        public void PascalCaseToUnderscoreWithLower_IsCultureInvariant()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = Create(b => b.UseNameConvert(NameConvertType.PascalCaseToUnderscoreWithLower));
                var table = fsql.CodeFirst.GetTableByEntity(typeof(NcUnderscore));
                Assert.Equal("id", table.ColumnsByCs["Id"].Attribute.Name);
                Assert.Equal("image_url", table.ColumnsByCs["ImageUrl"].Attribute.Name);
            });
        }

        [Fact]
        public void ToUpperNameConvert_IsCultureInvariant()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = Create(b => b.UseNameConvert(NameConvertType.ToUpper));
                var table = fsql.CodeFirst.GetTableByEntity(typeof(NcUpper));
                Assert.Equal("ID", table.ColumnsByCs["Id"].Attribute.Name);
                Assert.Equal("TITLE", table.ColumnsByCs["Title"].Attribute.Name);
                Assert.Equal("NCUPPER", table.DbName);
            });
        }

        [Fact]
        public void SyncStructureToLower_IsCultureInvariant()
        {
            RunUnderTurkish(() =>
            {
                using var fsql = Create(b => b);
                fsql.CodeFirst.IsSyncStructureToLower = true;
                var table = fsql.CodeFirst.GetTableByEntity(typeof(NcSync));
                Assert.Equal("id", table.ColumnsByCs["Id"].Attribute.Name);
                Assert.Equal("image", table.ColumnsByCs["Image"].Attribute.Name);
            });
        }
    }
}
