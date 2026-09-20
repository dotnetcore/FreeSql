using FreeSql.DataAnnotations;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBCteCompatibilityTests
{
    [Fact]
    public void WithSql_CteIsRejectedBeforeDerivedTableSqlGeneration()
    {
        using var fsql = CreateFreeSql();

        var exception = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CteProbeRow>()
                .WithSql("WITH source_rows AS (SELECT 1 AS \"Id\") SELECT * FROM source_rows")
                .ToSql());

        Assert.Contains("CTE", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WithSql", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AsTable_NestedCteIsRejectedBeforeSqlGeneration()
    {
        using var fsql = CreateFreeSql();

        var exception = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CteProbeRow>()
                .AsTable((_, _) => "(SELECT * FROM (WITH source_rows AS (SELECT 1 AS \"Id\") SELECT * FROM source_rows))")
                .ToSql());

        Assert.Contains("WITH", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CTE", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    static IFreeSql CreateFreeSql()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-CteCompatibilityTests", Guid.NewGuid().ToString("N"));
        return new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .Build();
    }

    [Table(Name = "sonnet_cte_probe")]
    sealed class CteProbeRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }
    }
}
