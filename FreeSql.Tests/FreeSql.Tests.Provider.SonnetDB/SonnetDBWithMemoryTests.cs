using FreeSql.Provider.SonnetDB.Attributes;

namespace FreeSql.Tests.Provider.SonnetDB;

public sealed class SonnetDBWithMemoryTests
{
    [Fact]
    public void WithMemory_SingleRow_DoesNotRequireUpsertProvider()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<MemoryRow>()
            .WithMemory(new[] { new MemoryRow { Id = 7, Name = "缓存" } })
            .ToSql();

        Assert.Contains("FROM (", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SELECT 7 as \"Id\"", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("'缓存'", sql, StringComparison.Ordinal);

        var row = fsql.Select<MemoryRow>()
            .WithMemory(new[] { new MemoryRow { Id = 7, Name = "缓存" } })
            .ToOne();
        Assert.NotNull(row);
        Assert.Equal(7, row!.Id);
        Assert.Equal("缓存", row.Name);
    }

    [Fact]
    public void WithMemory_MultipleRows_ReportsUnionAllCapability()
    {
        using var fsql = CreateFreeSql();

        var exception = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<MemoryRow>()
                .WithMemory(new[]
                {
                    new MemoryRow { Id = 7, Name = "一" },
                    new MemoryRow { Id = 8, Name = "二" },
                })
                .ToSql());

        Assert.Contains("UNION ALL", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WithMemory_SingleRow_NullValue_RoundTrips()
    {
        using var fsql = CreateFreeSql();

        var row = fsql.Select<NullableMemoryRow>()
            .WithMemory(new[] { new NullableMemoryRow { Id = 9, Name = null } })
            .ToOne();

        Assert.NotNull(row);
        Assert.Equal(9, row!.Id);
        Assert.Null(row.Name);
    }

    static IFreeSql CreateFreeSql() => new FreeSqlBuilder()
        .UseConnectionString(DataType.SonnetDB,
            $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-WithMemoryTests", Guid.NewGuid().ToString("N"))}")
        .Build();

    [SonnetDBTable]
    [FreeSql.DataAnnotations.Table(Name = "sonnet_with_memory")]
    sealed class MemoryRow
    {
        [FreeSql.DataAnnotations.Column(IsPrimary = true)]
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    [SonnetDBTable]
    [FreeSql.DataAnnotations.Table(Name = "sonnet_with_memory_null")]
    sealed class NullableMemoryRow
    {
        [FreeSql.DataAnnotations.Column(IsPrimary = true)]
        public long Id { get; set; }

        public string? Name { get; set; }
    }
}
