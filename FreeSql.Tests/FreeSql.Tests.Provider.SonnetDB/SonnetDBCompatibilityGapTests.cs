using FreeSql.DataAnnotations;
using FreeSql.Internal;
using FreeSql.Provider.SonnetDB.Attributes;
using FreeSql.SonnetDB;
using SonnetDB.Data;
using System.Data.Common;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBCompatibilityGapTests
{
    [Fact]
    public void Metadata_DeclaresOnlyInnerWhileProviderSupportsLeftJoin()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-MetadataTests", Guid.NewGuid().ToString("N"));
        using (var connection = new SndbConnection($"Data Source={dataPath}"))
        {
            connection.Open();
            var metadata = connection.GetSchema(DbMetaDataCollectionNames.DataSourceInformation);
            var declaredJoins = (SupportedJoinOperators)Convert.ToInt32(
                metadata.Rows[0][DbMetaDataColumnNames.SupportedJoinOperators]);

            // 3.1.0 的驱动元数据暂时只声明 INNER；上游修复后允许扩展为 Inner | LeftOuter。
            Assert.Equal(SupportedJoinOperators.Inner, declaredJoins);
        }

        using var fsql = CreateFreeSql();
        var sql = fsql.Select<CompatibilityTemporal, CompatibilityVersionedRow>()
            .LeftJoin((left, right) => left.Id == right.Id)
            .ToSql();

        // 提供程序不依赖不完整的元数据字段，关系表 LEFT JOIN 仍应正常生成。
        Assert.Contains("LEFT JOIN", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UnsupportedSelectMutationEntrypoints_AreValidatedBeforeSqlGeneration()
    {
        using var fsql = CreateFreeSql();

        var insertInto = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CompatibilityTextRow>()
                .InsertInto<CompatibilityTextRow>("sonnet_compatibility_text",
                    a => new CompatibilityTextRow { Id = a.Id, Name = a.Name, OtherName = a.OtherName }));
        Assert.Contains("INSERT ... SELECT", insertInto.Message, StringComparison.Ordinal);

        var toUpdate = fsql.Select<CompatibilityTemporal>()
            .Where(a => a.Id > 0)
            .ToUpdate()
            .Set(a => a.StartedAt, DateTime.UnixEpoch);
        var toUpdateSql = toUpdate.ToSql();
        Assert.DoesNotContain("select * from", toUpdateSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("select ftb_upd.\"Id\"", toUpdateSql, StringComparison.OrdinalIgnoreCase);

        var toDelete = fsql.Select<CompatibilityTemporal>().Where(a => a.Id > 0).ToDelete();
        var toDeleteSql = toDelete.ToSql();
        Assert.DoesNotContain("select * from", toDeleteSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("select ftb_del.\"Id\"", toDeleteSql, StringComparison.OrdinalIgnoreCase);

        var forUpdate = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CompatibilityTextRow>().ForUpdate());
        Assert.Contains("FOR UPDATE", forUpdate.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InsertIntoAsync_IsRejectedBeforeOpeningAConnection()
    {
        using var fsql = CreateFreeSql();

        var exception = await Assert.ThrowsAsync<NotSupportedException>(() =>
            fsql.Select<CompatibilityTextRow>()
                .InsertIntoAsync<CompatibilityTextRow>("sonnet_compatibility_text",
                    a => new CompatibilityTextRow { Id = a.Id, Name = a.Name, OtherName = a.OtherName }));

        Assert.Contains("INSERT ... SELECT", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UpdateJoin_IsRejectedBeforeCreatingTheJoinProvider()
    {
        using var fsql = CreateFreeSql();

        var exception = Assert.Throws<NotSupportedException>(() =>
            fsql.Update<CompatibilityTemporal>()
                .Join<CompatibilityTemporal>((left, right) => left.Id == right.Id));

        Assert.Contains("UPDATE JOIN", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StringComparisonOverloads_AreRejectedWithChineseMessage()
    {
        using var fsql = CreateFreeSql();

        var contains = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CompatibilityTextRow>()
                .Where(a => a.Name.Contains("abc", StringComparison.OrdinalIgnoreCase))
                .ToSql());
        Assert.Contains("StringComparison", contains.Message, StringComparison.Ordinal);

        var startsWith = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CompatibilityTextRow>()
                .Where(a => a.Name.StartsWith("abc", StringComparison.Ordinal))
                .ToSql());
        Assert.Contains("StringComparison", startsWith.Message, StringComparison.Ordinal);

        var staticEquals = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CompatibilityTextRow>()
                .Where(a => string.Equals(a.Name, "abc", StringComparison.OrdinalIgnoreCase))
                .ToSql());
        Assert.Contains("StringComparison", staticEquals.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void LikeLiteral_EscapesWildcardBackslashAndQuote()
    {
        using var fsql = CreateFreeSql();
        const string literal = "%_\\'";

        var sql = fsql.Select<CompatibilityTextRow>()
            .Where(a => a.Name.Contains(literal))
            .ToSql();

        var escaped = literal
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal)
            .Replace("'", "''", StringComparison.Ordinal);
        Assert.Contains($"like '%{escaped}%'", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LikeDynamicPattern_IsRejectedBeforeIncorrectSqlIsGenerated()
    {
        using var fsql = CreateFreeSql();

        var ex = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CompatibilityTextRow>()
                .Where(a => a.Name.Contains(a.OtherName))
                .ToSql());

        Assert.Contains("动态模式", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MeasurementCalendarDateAdds_UseSonnetDBDateFunctions()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<CompatibilityMeasurement>()
            .Where(a => a.Time.AddYears(1) > DateTime.UtcNow &&
                        a.Time.AddMonths(2) > DateTime.UtcNow &&
                        a.Time.AddMicroseconds(3) > DateTime.UtcNow)
            .ToSql();

        Assert.Contains("to_unix_milliseconds(date_add_datetime", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("'year'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("'month'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("'microsecond'", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DateTimeSubtract_UsesUnixMillisecondDifference()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<CompatibilityTemporal>()
            .Where(a => a.FinishedAt.Subtract(a.StartedAt).TotalSeconds > 1)
            .ToSql();

        Assert.Contains("to_unix_milliseconds", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/ 1000.0", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DateTimeSubtract_TotalTimeSpanMembers_KeepTheirUnits()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<CompatibilityTemporal>()
            .Where(a => a.FinishedAt.Subtract(a.StartedAt).TotalDays > 0 &&
                        a.FinishedAt.Subtract(a.StartedAt).TotalHours > 0 &&
                        a.FinishedAt.Subtract(a.StartedAt).TotalMinutes > 0 &&
                        a.FinishedAt.Subtract(a.StartedAt).TotalMilliseconds > 0 &&
                        a.FinishedAt.Subtract(a.StartedAt).TotalSeconds > 0)
            .ToSql();

        Assert.Contains("/ 86400000.0", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/ 3600000.0", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/ 60000.0", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/ 1000.0", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("to_unix_milliseconds", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DateTimeSubtract_ComponentMembers_AreRejectedWithChineseMessage()
    {
        using var fsql = CreateFreeSql();

        var ex = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CompatibilityTemporal>()
                .Where(a => a.FinishedAt.Subtract(a.StartedAt).Days > 0)
                .ToSql());

        Assert.Contains("TimeSpan.Days", ex.Message, StringComparison.Ordinal);
        Assert.Contains("日期差函数", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DateTimeOffset_TimeOfDay_UsesDatePartMilliseconds()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<CompatibilityOffset>()
            .Where(a => a.OccurredAt.TimeOfDay > TimeSpan.FromHours(12))
            .ToSql();

        Assert.Contains("date_part('hour'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("date_part('millisecond'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("3600000", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void DateTime_TimeOfDay_UsesDatePartMilliseconds()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<CompatibilityTemporal>()
            .Where(a => a.StartedAt.TimeOfDay > TimeSpan.FromHours(12))
            .ToSql();

        Assert.Contains("date_part('hour'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("date_part('millisecond'", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DateTimeOffsetSubtract_TotalMembers_UseUnixMillisecondDifference()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<CompatibilityOffset>()
            .Where(a => a.OccurredAt.Subtract(DateTimeOffset.UtcNow).TotalSeconds < 0 &&
                        a.OccurredAt.Subtract(DateTimeOffset.UtcNow).TotalMilliseconds < 0)
            .ToSql();

        Assert.Contains("to_unix_milliseconds", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/ 1000.0", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DateTimeOffsetSubtract_ComponentMembers_AreRejectedWithChineseMessage()
    {
        using var fsql = CreateFreeSql();

        var ex = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CompatibilityOffset>()
                .Where(a => a.OccurredAt.Subtract(DateTimeOffset.UtcNow).Days > 0)
                .ToSql());

        Assert.Contains("TimeSpan.Days", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DateTimeSubtract_Ticks_AreRejectedWithChineseMessage()
    {
        using var fsql = CreateFreeSql();

        var ex = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CompatibilityTemporal>()
                .Where(a => a.FinishedAt.Subtract(a.StartedAt).Ticks > 0)
                .ToSql());

        Assert.Contains("TimeSpan.Ticks", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DateTimeOffsetSubtract_Ticks_AreRejectedWithChineseMessage()
    {
        using var fsql = CreateFreeSql();

        var ex = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CompatibilityOffset>()
                .Where(a => a.OccurredAt.Subtract(DateTimeOffset.UtcNow).Ticks > 0)
                .ToSql());

        Assert.Contains("TimeSpan.Ticks", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MeasurementDateTimeSubtract_UsesMillisecondStorage()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<CompatibilityMeasurement>()
            .Where(a => a.Time.Subtract(DateTime.UtcNow).TotalMinutes > 1)
            .ToSql();

        Assert.Contains("a.time -", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("to_unix_milliseconds", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/ 60000.0", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DateTimeSubtract_TimeSpanOverload_IsRejectedWithChineseMessage()
    {
        using var fsql = CreateFreeSql();

        var ex = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CompatibilityTemporal>()
                .Where(a => a.StartedAt.Subtract(TimeSpan.FromDays(1)) > a.FinishedAt)
                .ToSql());

        Assert.Contains("DateTime.Subtract", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RowVersion_UpdateChecksOldValueAndKeepsDatabaseGeneratedVersion()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-CompatibilityGapTests", Guid.NewGuid().ToString("N"))}")
            .UseNoneCommandParameter(false)
            .UseAutoSyncStructure(true)
            .Build();

        var source = new CompatibilityVersionedRow { Name = "initial" };
        source.Id = fsql.Insert(source).ExecuteIdentity();
        var current = fsql.Select<CompatibilityVersionedRow>().Where(a => a.Id == source.Id).ToOne();
        var stale = fsql.Select<CompatibilityVersionedRow>().Where(a => a.Id == source.Id).ToOne();
        Assert.NotNull(current);
        Assert.NotNull(stale);
        Assert.Equal(1, current!.Version);

        current.Name = "current";
        Assert.Equal(1, fsql.Update<CompatibilityVersionedRow>().SetSource(current).ExecuteAffrows());
        Assert.Equal(2, current.Version);

        stale!.Name = "stale";
        var conflict = Assert.Throws<DbUpdateVersionException>(() =>
            fsql.Update<CompatibilityVersionedRow>().SetSource(stale).ExecuteAffrows());
        Assert.Contains("UPDATE", conflict.Sql, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(conflict.DbParams);
        Assert.Contains(stale, conflict.EntitySource);

        var persisted = fsql.Select<CompatibilityVersionedRow>().Where(a => a.Id == source.Id).ToOne();
        Assert.Equal("current", persisted!.Name);
        Assert.Equal(2, persisted.Version);
    }

    [Fact]
    public async Task RowVersion_AsyncUpdateChecksOldValue()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-CompatibilityGapTests", Guid.NewGuid().ToString("N"))}")
            .UseAutoSyncStructure(true)
            .Build();

        var source = new CompatibilityVersionedRow { Name = "initial" };
        source.Id = await fsql.Insert(source).ExecuteIdentityAsync();
        var stale = await fsql.Select<CompatibilityVersionedRow>().Where(a => a.Id == source.Id).ToOneAsync();
        var current = await fsql.Select<CompatibilityVersionedRow>().Where(a => a.Id == source.Id).ToOneAsync();
        Assert.NotNull(stale);
        Assert.NotNull(current);

        current!.Name = "current";
        Assert.Equal(1, await fsql.Update<CompatibilityVersionedRow>().SetSource(current).ExecuteAffrowsAsync());
        stale!.Name = "stale";

        var conflict = await Assert.ThrowsAsync<DbUpdateVersionException>(() =>
            fsql.Update<CompatibilityVersionedRow>().SetSource(stale).ExecuteAffrowsAsync());
        Assert.Contains("ROWVERSION", conflict.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("UPDATE", conflict.Sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UnionAll_IsRejectedBeforeSendingUnsupportedSql()
    {
        using var fsql = CreateFreeSql();

        var first = fsql.Select<CompatibilityTextRow>().Where(a => a.Id > 0);
        var second = fsql.Select<CompatibilityTextRow>().Where(a => a.Id < 0);
        var ex = Assert.Throws<NotSupportedException>(() => first.UnionAll(second).ToSql());

        Assert.Contains("UNION ALL", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("3.1", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InsertIntoSelect_IsRejectedWithChineseMessage()
    {
        using var fsql = CreateFreeSql();

        var ex = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CompatibilityTextRow>()
                .InsertInto(null, a => new CompatibilityTextRow
                {
                    Name = a.Name,
                    OtherName = a.OtherName
                }));

        Assert.Contains("INSERT ... SELECT", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("分批 INSERT", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InsertIntoSelectAsync_IsRejectedWithChineseMessage()
    {
        using var fsql = CreateFreeSql();

        var ex = await Assert.ThrowsAsync<NotSupportedException>(() =>
            fsql.Select<CompatibilityTextRow>()
                .InsertIntoAsync(null, a => new CompatibilityTextRow
                {
                    Name = a.Name,
                    OtherName = a.OtherName
                }));

        Assert.Contains("INSERT ... SELECT", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RecursiveCte_IsRejectedWithChineseMessage()
    {
        using var fsql = CreateFreeSql();

        var ex = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CompatibilityTreeRow>()
                .Where(a => a.Id == 1)
                .AsTreeCte()
                .ToSql());

        Assert.Contains("CTE", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AsTreeCte", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WithTempQuery_IsGeneratedAsDerivedQuery()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<CompatibilityTextRow>()
            .WithTempQuery(a => new { a.Id, a.Name })
            .Where(a => a.Id == 1)
            .ToSql();

        Assert.Contains("FROM (", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WITH", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TableRuleUnionAll_IsRejectedBeforeSqlGeneration()
    {
        using var fsql = CreateFreeSql();

        var ex = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<CompatibilityTextRow>()
                .AsTable((_, _) => "(SELECT 1 AS \"Id\" UNION ALL SELECT 2 AS \"Id\")")
                .ToSql());

        Assert.Contains("UNION ALL", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NestedCollectionProjection_RejectsReaderGeneratedUnionAll()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-NestedUnionTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseAutoSyncStructure(true)
            .Build();

        fsql.Insert(new[]
        {
            new NestedUnionParent { Id = 1, Name = "父一" },
            new NestedUnionParent { Id = 2, Name = "父二" }
        }).ExecuteAffrows();
        fsql.Insert(new[]
        {
            new NestedUnionChild { Id = 11, ParentId = 1, Name = "子一" },
            new NestedUnionChild { Id = 21, ParentId = 2, Name = "子二" }
        }).ExecuteAffrows();

        var ex = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<NestedUnionParent>()
                .OrderBy(a => a.Id)
                .ToList(a => new NestedUnionProjection
                {
                    Id = a.Id,
                    Children = fsql.Select<NestedUnionChild>()
                        .Where(child => child.ParentId == a.Id)
                        .ToList(child => child.Name)
                }));

        Assert.Contains("UNION ALL", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("嵌套查询", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NestedCollectionProjection_WithSingleOuterRow_RemainsSupported()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-NestedUnionSingleTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseAutoSyncStructure(true)
            .Build();

        fsql.Insert(new NestedUnionParent { Id = 1, Name = "父一" }).ExecuteAffrows();
        fsql.Insert(new NestedUnionChild { Id = 11, ParentId = 1, Name = "子一" }).ExecuteAffrows();

        var row = Assert.Single(fsql.Select<NestedUnionParent>().ToList(a => new NestedUnionProjection
        {
            Id = a.Id,
            Children = fsql.Select<NestedUnionChild>()
                .Where(child => child.ParentId == a.Id)
                .ToList(child => child.Name)
        }));

        Assert.Equal(1, row.Id);
        Assert.Equal(new[] { "子一" }, row.Children);
    }

    [Fact]
    public async Task NestedCollectionProjectionAsync_RejectsReaderGeneratedUnionAll()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-NestedUnionAsyncTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseAutoSyncStructure(true)
            .Build();

        await fsql.Insert(new[]
        {
            new NestedUnionParent { Id = 1, Name = "父一" },
            new NestedUnionParent { Id = 2, Name = "父二" }
        }).ExecuteAffrowsAsync();
        await fsql.Insert(new[]
        {
            new NestedUnionChild { Id = 11, ParentId = 1, Name = "子一" },
            new NestedUnionChild { Id = 21, ParentId = 2, Name = "子二" }
        }).ExecuteAffrowsAsync();

        var ex = await Assert.ThrowsAsync<NotSupportedException>(() =>
            fsql.Select<NestedUnionParent>()
                .OrderBy(a => a.Id)
                .ToListAsync(a => new NestedUnionProjection
                {
                    Id = a.Id,
                    Children = fsql.Select<NestedUnionChild>()
                        .Where(child => child.ParentId == a.Id)
                        .ToList(child => child.Name)
                }));

        Assert.Contains("UNION ALL", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    static IFreeSql CreateFreeSql()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-CompatibilityGapTests", Guid.NewGuid().ToString("N"));
        return new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .Build();
    }

    [Table(Name = "sonnet_compatibility_text")]
    sealed class CompatibilityTextRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string OtherName { get; set; } = string.Empty;
    }

    [Table(Name = "sonnet_compatibility_measurement")]
    sealed class CompatibilityMeasurement
    {
        [Column(Name = "time")]
        public DateTime Time { get; set; }

        [SonnetDBTag]
        public string Host { get; set; } = string.Empty;

        [SonnetDBField]
        public double Value { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_compatibility_temporal")]
    sealed class CompatibilityTemporal
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime FinishedAt { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_compatibility_offset")]
    sealed class CompatibilityOffset
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public DateTimeOffset OccurredAt { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_compatibility_versioned")]
    sealed class CompatibilityVersionedRow
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Column(IsVersion = true)]
        public long Version { get; set; }
    }

    [Table(Name = "sonnet_compatibility_tree")]
    sealed class CompatibilityTreeRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public long ParentId { get; set; }

        [Navigate(nameof(ParentId))]
        public CompatibilityTreeRow Parent { get; set; }

        [Navigate(nameof(ParentId))]
        public List<CompatibilityTreeRow> Children { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_nested_union_parent")]
    sealed class NestedUnionParent
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_nested_union_child")]
    sealed class NestedUnionChild
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public long ParentId { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    sealed class NestedUnionProjection
    {
        public long Id { get; set; }

        public List<string> Children { get; set; }
    }
}
