using FreeSql.DataAnnotations;
using FreeSql.Internal;
using FreeSql.Provider.SonnetDB.Attributes;
using FreeSql.SonnetDB;

namespace FreeSql.Tests.Provider.SonnetDB;

public sealed class SonnetDBCompositeBatchUpdateEdgeTests
{
    [Fact]
    public void SinglePrimaryBatchUpdate_ExecutesWithSonnetDbCaseGrammar()
    {
        using var fsql = CreateFreeSql();
        var rows = new[]
        {
            new SingleRow { Id = 1, Name = "before-1" },
            new SingleRow { Id = 2, Name = "before-2" },
        };
        Assert.Equal(rows.Length, fsql.Insert(rows).ExecuteAffrows());
        rows[0].Name = "after-1";
        rows[1].Name = "after-2";

        var update = fsql.Update<SingleRow>().SetSource(rows);
        Assert.Equal(rows.Length, update.ExecuteAffrows());
        Assert.Equal(
            new[] { "after-1", "after-2" },
            fsql.Select<SingleRow>().OrderBy(row => row.Id).ToList().Select(row => row.Name).ToArray());
    }

    [Fact]
    public void BitwisePredicate_IsRejectedBeforeSendingUnsupportedSql()
    {
        using var fsql = CreateFreeSql();
        var exception = Assert.Throws<NotSupportedException>(() => fsql.Select<SingleRow>()
            .Where(row => (row.Id & 1) == 0)
            .ToSql());
        Assert.Contains("位运算符", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void StringLengthPredicate_IsRejectedBeforeSendingUnsupportedSql()
    {
        using var fsql = CreateFreeSql();
        var exception = Record.Exception(() => fsql.Select<SingleRow>()
            .Where(row => row.Name.Length > 2)
            .ToList());
        Assert.NotNull(exception);
        Assert.Contains("length", exception!.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("未注册", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ParameterizedCompositeBatchUpdate_SplitsAndUpdatesAllRows()
    {
        using var fsql = CreateFreeSql();
        var rows = Enumerable.Range(1, 4)
            .Select(index => new CompositeRow
            {
                TenantId = index,
                DeviceId = index * 10,
                Name = $"before-{index}",
            })
            .ToArray();

        Assert.Equal(rows.Length, await fsql.Insert(rows).ExecuteAffrowsAsync());
        foreach (var row in rows)
            row.Name = $"after-{row.TenantId}";

        // rowsLimit=3 在基类中会按每批 2 行切分，确保 CASE 和分片路径都被执行。
        var update = fsql.Update<CompositeRow>()
            .SetSource(rows)
            .BatchOptions(rowsLimit: 3, parameterLimit: 100, autoTransaction: false);
        Assert.Equal(rows.Length, await update.ExecuteAffrowsAsync());

        var persisted = await fsql.Select<CompositeRow>()
            .OrderBy(row => row.TenantId)
            .ToListAsync();
        Assert.Equal(rows.Select(row => row.Name).ToArray(), persisted.Select(row => row.Name).ToArray());
    }

    [Fact]
    public void CompositeBatchUpdate_EscapesStringPrimaryKeys()
    {
        using var fsql = CreateFreeSql();
        var rows = new[]
        {
            new StringCompositeRow { Tenant = "tenant'one", Device = "device\\one", Name = "before-1" },
            new StringCompositeRow { Tenant = "tenant-two", Device = "device-two", Name = "before-2" },
        };

        Assert.Equal(rows.Length, fsql.Insert(rows).ExecuteAffrows());
        rows[0].Name = "after-1";
        rows[1].Name = "after-2";

        Assert.Equal(rows.Length, fsql.Update<StringCompositeRow>().SetSource(rows).ExecuteAffrows());
        var persisted = fsql.Select<StringCompositeRow>()
            .OrderBy(row => row.Tenant)
            .ToList();
        Assert.Equal(new[] { "after-1", "after-2" }, persisted.Select(row => row.Name).OrderBy(name => name).ToArray());
    }

    [Fact]
    public async Task CompositeRowVersionBatchUpdate_UsesPerRowExpectedVersion()
    {
        using var fsql = CreateFreeSql();
        var rows = new[]
        {
            new VersionedCompositeRow { TenantId = 1, DeviceId = 10, Name = "one" },
            new VersionedCompositeRow { TenantId = 2, DeviceId = 20, Name = "two" },
        };
        Assert.Equal(rows.Length, await fsql.Insert(rows).ExecuteAffrowsAsync());

        var current = await fsql.Select<VersionedCompositeRow>().OrderBy(row => row.TenantId).ToListAsync();
        var stale = current.Select(row => new VersionedCompositeRow
        {
            TenantId = row.TenantId,
            DeviceId = row.DeviceId,
            Name = row.Name,
            Version = row.Version,
        }).ToArray();
        current[0].Name = "one-updated";
        current[1].Name = "two-updated";

        var update = fsql.Update<VersionedCompositeRow>()
            .SetSource(current)
            .BatchOptions(rowsLimit: 10, parameterLimit: 100, autoTransaction: false);
        Assert.Equal(rows.Length, await update.ExecuteAffrowsAsync());
        Assert.All(current, row => Assert.Equal(2, row.Version));

        stale[0].Name = "stale";
        var conflict = Assert.Throws<DbUpdateVersionException>(() =>
            fsql.Update<VersionedCompositeRow>().SetSource(stale).ExecuteAffrows());
        Assert.Contains("UPDATE", conflict.Sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("行级乐观锁", conflict.Message, StringComparison.Ordinal);
    }

    static IFreeSql CreateFreeSql()
    {
        var path = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-CompositeBatchUpdateEdgeTests", Guid.NewGuid().ToString("N"));
        return new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={path}")
            .UseNoneCommandParameter(false)
            .UseAutoSyncStructure(true)
            .Build();
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_composite_batch_edge")]
    sealed class CompositeRow
    {
        [Column(IsPrimary = true)] public long TenantId { get; set; }
        [Column(IsPrimary = true)] public long DeviceId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_single_batch_edge")]
    sealed class SingleRow
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_string_composite_batch_edge")]
    sealed class StringCompositeRow
    {
        [Column(IsPrimary = true)] public string Tenant { get; set; } = string.Empty;
        [Column(IsPrimary = true)] public string Device { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_versioned_composite_batch_edge")]
    sealed class VersionedCompositeRow
    {
        [Column(IsPrimary = true)] public long TenantId { get; set; }
        [Column(IsPrimary = true)] public long DeviceId { get; set; }
        public string Name { get; set; } = string.Empty;
        [Column(IsVersion = true)] public long Version { get; set; }
    }
}
