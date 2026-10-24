using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using FreeSql.SonnetDB;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBCompositeBatchUpdateTests
{
    [Fact]
    public void BatchUpdate_WithCompositePrimaryKey_ExecutesWithoutStringConcatenation()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-CompositeBatchUpdateTests", Guid.NewGuid().ToString("N"))}")
            .UseAutoSyncStructure(true)
            .Build();
        var rows = new[]
        {
            new Row { TenantId = 1, DeviceId = 2, Name = "a" },
            new Row { TenantId = 3, DeviceId = 4, Name = "b" },
        };
        Assert.Equal(2, fsql.Insert(rows).ExecuteAffrows());

        rows[0].Name = "a2";
        rows[1].Name = "b2";
        Assert.Equal(2, fsql.Update<Row>().SetSource(rows).ExecuteAffrows());

        var loaded = fsql.Select<Row>().OrderBy(a => a.TenantId).ToList();
        Assert.Equal(new[] { "a2", "b2" }, loaded.Select(a => a.Name).ToArray());
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_composite_batch_update")]
    sealed class Row
    {
        [Column(IsPrimary = true)] public long TenantId { get; set; }
        [Column(IsPrimary = true)] public long DeviceId { get; set; }
        public string Name { get; set; } = "";
    }
}
