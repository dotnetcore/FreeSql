using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBCteCompatibilityTests
{
    [Fact]
    public void WithSql_CteExecutesThroughExistingQueryApi()
    {
        using var fsql = CreateFreeSql();

        var row = Assert.Single(fsql.Select<CteProbeRow>()
            .WithSql("WITH source_rows AS (SELECT 1 AS \"Id\") SELECT * FROM source_rows")
            .ToList());
        Assert.Equal(1, row.Id);
    }

    [Fact]
    public void AsTable_NestedCteExecutesWithoutLosingProjection()
    {
        using var fsql = CreateFreeSql();

        var row = Assert.Single(fsql.Select<CteProbeRow>()
            .AsTable((_, _) => "(SELECT * FROM (WITH source_rows AS (SELECT 1 AS \"Id\") SELECT * FROM source_rows) nested_source)")
            .ToList());
        Assert.Equal(1, row.Id);
    }

    static IFreeSql CreateFreeSql()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-CteCompatibilityTests", Guid.NewGuid().ToString("N"));
        return new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .Build();
    }

    [Table(Name = "sonnet_cte_probe")]
    [SonnetDBTable]
    sealed class CteProbeRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }
    }
}
