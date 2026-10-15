using FreeSql.DataAnnotations;
using FreeSql.Internal;
using FreeSql.Provider.SonnetDB.Attributes;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBReleasedCrudTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InsertOrUpdate_UsesAtomicConflictClauseAndSelectedUpdateColumns(bool noneParameter)
    {
        using var fsql = CreateFreeSql(noneParameter);
        fsql.Insert(new Row { Id = 1, Name = "original", Value = 10 }).ExecuteAffrows();

        var source = new[] { new Row { Id = 1, Name = "ignored", Value = 20 }, new Row { Id = 2, Name = "inserted", Value = 30 } };
        var upsert = fsql.InsertOrUpdate<Row>().SetSource(source).UpdateColumns(a => a.Value);
        Assert.Contains("ON CONFLICT", upsert.ToSql(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, upsert.ExecuteAffrows());

        var rows = fsql.Select<Row>().OrderBy(a => a.Id).ToList();
        Assert.Equal(new[] { "original", "inserted" }, rows.Select(a => a.Name));
        Assert.Equal(new long[] { 20, 30 }, rows.Select(a => a.Value));
        Assert.Equal(0, fsql.InsertOrUpdate<Row>().SetSource(new Row { Id = 1, Name = "skipped", Value = 99 })
            .IfExistsDoNothing().ExecuteAffrows());
        Assert.Equal(20, fsql.Select<Row>().Where(a => a.Id == 1).ToOne().Value);
    }

    [Fact]
    public void InsertOrUpdate_UpdateSetCombinesStoredAndExcludedValues()
    {
        using var fsql = CreateFreeSql();
        fsql.Insert(new Row { Id = 1, Name = "before", Value = 10 }).ExecuteAffrows();
        Assert.Equal(1, fsql.InsertOrUpdate<Row>().SetSource(new Row { Id = 1, Name = "after", Value = 5 })
            .UpdateSet((stored, incoming) => stored.Value == stored.Value + incoming.Value).ExecuteAffrows());
        Assert.Equal(15, fsql.Select<Row>().ToOne().Value);
    }

    [Fact]
    public void InsertOrUpdate_UpdateSetPreservesAliasLikeTextInsideStringLiterals()
    {
        using var fsql = CreateFreeSql();
        const string literal = "t1.\"Name\" + t2.\"Value\"; 'quoted'";
        fsql.Insert(new Row { Id = 1, Name = "before", Value = 10 }).ExecuteAffrows();
        Assert.Equal(1, fsql.InsertOrUpdate<Row>().SetSource(new Row { Id = 1, Name = "incoming", Value = 5 })
            .UpdateSet((stored, incoming) => stored.Name == literal).ExecuteAffrows());
        Assert.Equal(literal, fsql.Select<Row>().ToOne().Name);
    }

    [Fact]
    public void InsertOrUpdate_DynamicEntitiesAndDictionariesUseTheirActualModels()
    {
        using var fsql = CreateFreeSql();
        Assert.Equal(1, fsql.InsertOrUpdate<object>().AsType(typeof(Row))
            .SetSource(new Row { Id = 1, Name = "dynamic", Value = 10 }).ExecuteAffrows());
        Assert.Equal(1, fsql.InsertOrUpdateDict(new Dictionary<string, object>
        {
            ["Id"] = 1L, ["Name"] = "dictionary", ["Value"] = 20L
        }).AsTable("sonnet_released_row").WherePrimary("Id").ExecuteAffrows());
        var current = fsql.Select<Row>().ToOne();
        Assert.Equal("dictionary", current.Name);
        Assert.Equal(20, current.Value);
    }

    [Fact]
    public void InsertOrUpdate_UniqueConflictTargetAndSourceQueryUseExistingApi()
    {
        using var fsql = CreateFreeSql();
        fsql.CodeFirst.SyncStructure<UniqueRow>();
        fsql.Insert(new UniqueRow { Id = 1, Name = "unique", Value = 10 }).ExecuteAffrows();
        Assert.Equal(1, fsql.InsertOrUpdate<UniqueRow>()
            .SetSource(new UniqueRow { Id = 2, Name = "unique", Value = 20 }, a => a.Name)
            .UpdateColumns(a => a.Value).ExecuteAffrows());
        var current = fsql.Select<UniqueRow>().ToOne();
        Assert.Equal(1, current.Id);
        Assert.Equal(20, current.Value);
        Assert.Equal(1, fsql.InsertOrUpdate<UniqueRow>()
            .SetSource("SELECT 3 AS \"Id\", 'source' AS \"Name\", 30 AS \"Value\"").ExecuteAffrows());
        Assert.Equal(2, fsql.Select<UniqueRow>().Count());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InsertOrUpdate_UniqueTargetPreservesIdentityGeneration(bool noneParameter)
    {
        using var fsql = CreateFreeSql(noneParameter);
        Assert.Equal(1, fsql.InsertOrUpdate<IdentityUniqueRow>()
            .SetSource(new IdentityUniqueRow { Name = "first", Value = 10 }, row => row.Name).ExecuteAffrows());
        Assert.Equal(1, await fsql.InsertOrUpdate<IdentityUniqueRow>()
            .SetSource(new IdentityUniqueRow { Name = "second", Value = 20 }, row => row.Name).ExecuteAffrowsAsync());

        var rows = fsql.Select<IdentityUniqueRow>().OrderBy(row => row.Name).ToList();
        Assert.Equal(2, rows.Count);
        Assert.All(rows, row => Assert.True(row.Id > 0));
        Assert.NotEqual(rows[0].Id, rows[1].Id);
        var originalId = rows[0].Id;
        Assert.Equal(1, fsql.InsertOrUpdate<IdentityUniqueRow>()
            .SetSource(new IdentityUniqueRow { Name = "first", Value = 30 }, row => row.Name).ExecuteAffrows());
        var updated = fsql.Select<IdentityUniqueRow>().Where(row => row.Name == "first").ToOne();
        Assert.Equal(originalId, updated.Id);
        Assert.Equal(30, updated.Value);
        Assert.Equal(2, fsql.Select<IdentityUniqueRow>().Count());
    }

    [Fact]
    public async Task InsertOrUpdateAsync_CompositeKeyAndIdentityWorkInsideTransactions()
    {
        using var fsql = CreateFreeSql();
        await fsql.Insert(new CompositeRow { Tenant = 1, Id = 1, Value = 10 }).ExecuteAffrowsAsync();
        Assert.Equal(2, await fsql.InsertOrUpdate<CompositeRow>().SetSource(new[]
        {
            new CompositeRow { Tenant = 1, Id = 1, Value = 20 },
            new CompositeRow { Tenant = 2, Id = 1, Value = 30 }
        }).ExecuteAffrowsAsync());
        Assert.Equal(new long[] { 20, 30 }, (await fsql.Select<CompositeRow>().OrderBy(a => a.Tenant).ToListAsync()).Select(a => a.Value));

        fsql.CodeFirst.SyncStructure<IdentityRow>();
        using var connection = fsql.Ado.MasterPool.Get();
        using var transaction = connection.Value.BeginTransaction();
        Assert.Equal(1, await fsql.InsertOrUpdate<IdentityRow>().WithTransaction(transaction)
            .SetSource(new IdentityRow { Name = "generated" }).ExecuteAffrowsAsync());
        Assert.Equal("generated", (await fsql.Select<IdentityRow>().WithTransaction(transaction).ToOneAsync()).Name);
        transaction.Rollback();
        Assert.Equal(0, fsql.Select<IdentityRow>().Count());
    }

    [Fact]
    public async Task Returning_MapsRenamedColumnsAndPreservesFinalRowVersion()
    {
        using var fsql = CreateFreeSql();
        var inserted = Assert.Single(await fsql.Insert(new VersionedRow { Id = 1, Name = "before" }).ExecuteInsertedAsync());
        Assert.Equal("before", inserted.Name);
        Assert.Equal(1, inserted.Version);
        var source = fsql.Select<VersionedRow>().ToOne();
        var stale = fsql.Select<VersionedRow>().ToOne();
        source.Name = "after";
        var updated = Assert.Single(fsql.Update<VersionedRow>().SetSource(source).ExecuteUpdated());
        Assert.Equal("after", updated.Name);
        Assert.Equal(2, updated.Version);
        Assert.Equal(2, source.Version);
        stale.Name = "stale";
        Assert.Throws<DbUpdateVersionException>(() => fsql.Update<VersionedRow>().SetSource(stale).ExecuteUpdated());

        var projected = Assert.Single(await fsql.Update<VersionedRow>().Where(a => a.Id == 1)
            .Set(a => a.Name, "projected").ExecuteUpdatedAsync(a => new { Key = a.Id, Title = a.Name }));
        Assert.Equal(1, projected.Key);
        Assert.Equal("projected", projected.Title);
        var deleted = Assert.Single(await fsql.Delete<VersionedRow>().Where(a => a.Id == 1).ExecuteDeletedAsync());
        Assert.Equal("projected", deleted.Name);
        Assert.Equal(3, deleted.Version);
        Assert.Empty(fsql.Select<VersionedRow>().ToList());
    }

    [Fact]
    public void Returning_UsesTheProvidedTransactionAndReturnsDeletedImages()
    {
        using var fsql = CreateFreeSql();
        fsql.Insert(new Row { Id = 1, Name = "before", Value = 10 }).ExecuteAffrows();
        using var connection = fsql.Ado.MasterPool.Get();
        using var transaction = connection.Value.BeginTransaction();
        var updated = Assert.Single(fsql.Update<Row>().WithTransaction(transaction)
            .Where(a => a.Id == 1).Set(a => a.Value, 20).ExecuteUpdated());
        Assert.Equal(20, updated.Value);
        var deleted = Assert.Single(fsql.Delete<Row>().WithTransaction(transaction).Where(a => a.Id == 1).ExecuteDeleted());
        Assert.Equal(20, deleted.Value);
        transaction.Rollback();
        Assert.Equal(10, fsql.Select<Row>().ToOne().Value);
    }

    [Theory]
    [InlineData(123456789L, false)]
    [InlineData(-123456789L, true)]
    [InlineData(long.MinValue, false)]
    [InlineData(long.MaxValue, true)]
    public async Task Returning_TemporalProjectionsPreserveTicksAndNullableValues(long ticks, bool noneParameter)
    {
        using var fsql = CreateFreeSql(noneParameter);
        var duration = TimeSpan.FromTicks(ticks);
        var day = new DateOnly(2026, 10, 15);
        var clock = new TimeOnly(12, 34, 56).Add(TimeSpan.FromTicks(7890123));
        // DATETIME sub-millisecond preservation remains tracked by upstream
        // #236. Keep that limitation separate from duration ticks conversion.
        var offset = new DateTimeOffset(2026, 10, 15, 12, 34, 56, TimeSpan.FromHours(8)).AddMilliseconds(789);
        var started = offset.UtcDateTime;
        TemporalReturningRow Source(long id, bool missing) => new TemporalReturningRow
        {
            Id = id, Name = "before", Duration = duration, Day = day, Clock = clock,
            Offset = offset, StartedAt = started, OptionalDuration = missing ? null : duration,
            OptionalDay = missing ? null : day, OptionalClock = missing ? null : clock,
            OptionalOffset = missing ? null : offset
        };
        void AssertRow(TemporalReturningRow row, bool missing)
        {
            Assert.Equal(duration, row.Duration);
            Assert.Equal(day, row.Day);
            Assert.Equal(clock, row.Clock);
            Assert.Equal(offset.UtcDateTime, row.Offset.UtcDateTime);
            Assert.Equal(started, row.StartedAt);
            Assert.Equal(missing ? null : (TimeSpan?)duration, row.OptionalDuration);
            Assert.Equal(missing ? null : (DateOnly?)day, row.OptionalDay);
            Assert.Equal(missing ? null : (TimeOnly?)clock, row.OptionalClock);
            Assert.Equal(missing ? null : (DateTime?)offset.UtcDateTime, row.OptionalOffset?.UtcDateTime);
        }

        AssertRow(Assert.Single(fsql.Insert(Source(1, false)).ExecuteInserted()), false);
        AssertRow(Assert.Single(await fsql.Insert(Source(2, true)).ExecuteInsertedAsync()), true);
        IUpdate<TemporalReturningRow> Update(long id) => fsql.Update<TemporalReturningRow>()
            .Where(row => row.Id == id).Set(row => row.Name, "after");
        Assert.Equal(duration, Assert.Single(Update(1).ExecuteUpdated(row => row.Duration)));
        Assert.Equal(duration, Assert.Single(await Update(1).ExecuteUpdatedAsync(row => row.Duration)));
        Assert.Null(Assert.Single(Update(2).ExecuteUpdated(row => row.OptionalDuration)));
        Assert.Equal(duration, Assert.Single(await Update(1).ExecuteUpdatedAsync(row => row.OptionalDuration)));
        Assert.Equal(day, Assert.Single(Update(1).ExecuteUpdated(row => row.Day)));
        Assert.Equal(clock, Assert.Single(await Update(1).ExecuteUpdatedAsync(row => row.Clock)));
        Assert.Equal(offset.UtcDateTime, Assert.Single(Update(1).ExecuteUpdated(row => row.Offset)).UtcDateTime);
        Assert.Equal(started, Assert.Single(await Update(1).ExecuteUpdatedAsync(row => row.StartedAt)));

        var projected = Assert.Single(Update(1).ExecuteUpdated(row => new
        {
            Span = row.Duration, Maybe = row.OptionalDuration, row.Day, row.Clock, row.Offset, row.StartedAt
        }));
        Assert.Equal(duration, projected.Span);
        Assert.Equal(duration, projected.Maybe);
        Assert.Equal(day, projected.Day);
        Assert.Equal(clock, projected.Clock);
        Assert.Equal(offset.UtcDateTime, projected.Offset.UtcDateTime);
        Assert.Equal(started, projected.StartedAt);
        var nullable = Assert.Single(await Update(2).ExecuteUpdatedAsync(row => new
        {
            row.OptionalOffset, row.OptionalClock, row.OptionalDay, row.OptionalDuration, Span = row.Duration
        }));
        Assert.Null(nullable.OptionalOffset);
        Assert.Null(nullable.OptionalClock);
        Assert.Null(nullable.OptionalDay);
        Assert.Null(nullable.OptionalDuration);
        Assert.Equal(duration, nullable.Span);
        AssertRow(Assert.Single(fsql.Delete<TemporalReturningRow>().Where(row => row.Id == 1).ExecuteDeleted()), false);
        AssertRow(Assert.Single(await fsql.Delete<TemporalReturningRow>().Where(row => row.Id == 2).ExecuteDeletedAsync()), true);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MutationSql_PreservesSubqueryLikeTextInsideStringLiterals(bool noneParameter)
    {
        using var fsql = CreateFreeSql(noneParameter);
        const string literal = "prefix select * from ( suffix; 'quoted'";
        fsql.Insert(new Row { Id = 1, Name = "before", Value = 10 }).ExecuteAffrows();
        Assert.Equal(1, fsql.Update<Row>().Where(row => row.Id == 1)
            .NoneParameter(noneParameter).Set(row => row.Name, literal).ExecuteAffrows());
        Assert.Equal(literal, fsql.Select<Row>().ToOne().Name);
        Assert.Equal(1, fsql.Delete<Row>().Where(row => row.Name == literal).ExecuteAffrows());
        Assert.Equal(0, fsql.Select<Row>().Count());
    }

    [Fact]
    public void RowVersionGuard_AllowsColumnNamesInsideUnrelatedStringValues()
    {
        using var fsql = CreateFreeSql();
        const string literal = "text containing \"Version\" = 3, select * from (";
        fsql.Insert(new VersionedRow { Id = 1, Name = "before" }).ExecuteAffrows();
        var updated = Assert.Single(fsql.Update<VersionedRow>().Where(row => row.Id == 1)
            .NoneParameter(true).Set(row => row.Name, literal).ExecuteUpdated());
        Assert.Equal(literal, updated.Name);
        Assert.Equal(2, updated.Version);
        Assert.Throws<NotSupportedException>(() => fsql.Update<VersionedRow>().Where(row => row.Id == 1)
            .Set(row => row.Version, 5).ToSql());
    }

    [Fact]
    public async Task InsertInto_ExecutesFilteredProjectionSynchronouslyAndAsynchronously()
    {
        using var fsql = CreateFreeSql();
        fsql.Insert(new[] { new Row { Id = 1, Name = "one", Value = 10 }, new Row { Id = 2, Name = "two", Value = 20 } }).ExecuteAffrows();
        Assert.Equal(1, fsql.Select<Row>().Where(a => a.Id == 1).InsertInto<ArchiveRow>(null,
            a => new ArchiveRow { Id = a.Id, Name = a.Name, Value = a.Value }));
        Assert.Equal(1, await fsql.Select<Row>().Where(a => a.Id == 2).InsertIntoAsync<ArchiveRow>(null,
            a => new ArchiveRow { Id = a.Id, Name = a.Name, Value = a.Value }));
        Assert.Equal(new[] { "one", "two" }, fsql.Select<ArchiveRow>().OrderBy(a => a.Id).ToList().Select(a => a.Name));
    }

    [Fact]
    public void UnionAll_RetainsDuplicatesAndAllowsTableRuleSources()
    {
        using var fsql = CreateFreeSql();
        fsql.Insert(new Row { Id = 1, Name = "same", Value = 10 }).ExecuteAffrows();
        var first = fsql.Select<Row>().Where(a => a.Id == 1);
        var second = fsql.Select<Row>().Where(a => a.Id == 1);
        Assert.Equal(2, first.UnionAll(second).ToList().Count);
        var rows = fsql.Select<Row>()
            .AsTable((_, _) => "(SELECT 1 AS \"Id\", 'first' AS \"Name\", 1 AS \"Value\" UNION ALL SELECT 1 AS \"Id\", 'second' AS \"Name\", 2 AS \"Value\")")
            .ToList();
        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public void AsTreeCte_TraversesExistingNavigationInBothDirections()
    {
        using var fsql = CreateFreeSql();
        fsql.Insert(new[]
        {
            new TreeRow { Id = 1, ParentId = 0, Name = "root" },
            new TreeRow { Id = 2, ParentId = 1, Name = "child" },
            new TreeRow { Id = 3, ParentId = 2, Name = "leaf" }
        }).ExecuteAffrows();
        var query = fsql.Select<TreeRow>().Where(a => a.Id == 1).AsTreeCte(a => a.Name);
        Assert.Contains("WITH RECURSIVE", query.ToSql(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(new long[] { 1, 2, 3 }, query.OrderBy(a => a.Id).ToList().Select(a => a.Id));
        Assert.Equal(3, fsql.Select<TreeRow>().Where(a => a.Id == 3).AsTreeCte(up: true).ToList().Count);
    }

    [Fact]
    public void Joins_ExposeRightFullAndCrossFormsWithoutChangingPublicApi()
    {
        using var fsql = CreateFreeSql();
        fsql.Insert(new[] { new Row { Id = 1, Name = "one" }, new Row { Id = 2, Name = "two" } }).ExecuteAffrows();
        fsql.Insert(new[] { new ArchiveRow { Id = 2, Name = "two" }, new ArchiveRow { Id = 3, Name = "three" } }).ExecuteAffrows();
        var right = fsql.Select<Row, ArchiveRow>().RightJoin((left, right) => left.Id == right.Id)
            .ToList((left, right) => new { Left = (long?)left.Id, Right = right.Id });
        Assert.Contains(right, a => a.Left == null && a.Right == 3);
        Assert.Equal(3, fsql.Select<Row>()
            .RawJoin("FULL JOIN \"sonnet_released_archive\" b ON a.\"Id\" = b.\"Id\"")
            .ToList<long?>("a.\"Id\"").Count);
        Assert.Equal(4, fsql.Select<Row, ArchiveRow>().ToList((left, right) => new { Left = left.Id, Right = right.Id }).Count);
    }

    [Fact]
    public void UpdateJoin_MapsTheExistingUpdateJoinApiToRelationalSql()
    {
        using var fsql = CreateFreeSql();
        fsql.Insert(new[] { new Row { Id = 1, Name = "before", Value = 1 }, new Row { Id = 2, Name = "unchanged", Value = 2 } }).ExecuteAffrows();
        fsql.Insert(new ArchiveRow { Id = 1, Name = "source", Value = 10 }).ExecuteAffrows();
        Assert.Equal(1, fsql.Update<Row>().Join<ArchiveRow>((target, source) => target.Id == source.Id)
            .Set((target, source) => target.Name == source.Name)
            .Set((target, source) => target.Value == source.Value + 1)
            .Where((target, source) => source.Id == 1).ExecuteAffrows());
        var rows = fsql.Select<Row>().OrderBy(a => a.Id).ToList();
        Assert.Equal("source", rows[0].Name);
        Assert.Equal(11, rows[0].Value);
        Assert.Equal("unchanged", rows[1].Name);
    }

    [Fact]
    public async Task MeasurementVectorUpdate_ReplacesOnlyTheSelectedSamples()
    {
        using var fsql = CreateFreeSql();
        var begin = DateTime.UnixEpoch.AddSeconds(1);
        var end = DateTime.UnixEpoch.AddSeconds(2);
        fsql.Insert(new[]
        {
            new VectorMeasurement { Time = begin, Host = "alpha", Embedding = new float[] { 1, 0 } },
            new VectorMeasurement { Time = end, Host = "alpha", Embedding = new float[] { 0, 1 } }
        }).ExecuteAffrows();
        Assert.Equal(1, await fsql.Update<VectorMeasurement>()
            .Where(a => a.Time >= begin && a.Time < end && a.Host == "alpha")
            .Set(a => a.Embedding, new float[] { 2, 3 }).ExecuteAffrowsAsync());
        var rows = fsql.Select<VectorMeasurement>().OrderBy(a => a.Time).ToList();
        Assert.Equal(new float[] { 2, 3 }, rows[0].Embedding);
        Assert.Equal(new float[] { 0, 1 }, rows[1].Embedding);
        Assert.Throws<NotSupportedException>(() => fsql.Update<VectorMeasurement>().Where(a => a.Time == begin)
            .Set(a => a.Host, "other").ToSql());
        Assert.Throws<NotSupportedException>(() => fsql.Update<VectorMeasurement>().Where(a => a.Time == begin)
            .Set(a => a.Embedding, new float[] { 1, 1 }).ExecuteUpdated());
    }

    [Fact]
    public void DecimalPrecision_MigratesTheDeclaredPrecisionAndBecomesIdempotent()
    {
        using var fsql = CreateFreeSql();
        fsql.Ado.ExecuteNonQuery("CREATE TABLE \"sonnet_released_decimal\" (\"Id\" INT, \"Amount\" DECIMAL(18,4) NULL, PRIMARY KEY(\"Id\"))");
        var ddl = fsql.CodeFirst.GetComparisonDDLStatements<DecimalRow>();
        Assert.Contains("DECIMAL(15,3)", ddl);
        fsql.CodeFirst.SyncStructure<DecimalRow>();
        var column = fsql.DbFirst.GetTableByName("sonnet_released_decimal").Columns.Single(a => a.Name == "Amount");
        Assert.Equal(typeof(decimal), column.CsType);
        Assert.Equal(15, column.Precision);
        Assert.Equal(3, column.Scale);
        Assert.Null(fsql.CodeFirst.GetComparisonDDLStatements<DecimalRow>());
    }

    [Fact]
    public void UnsupportedSchemaAndDescendingIndex_AreRejectedBeforeExecutingDdl()
    {
        using var fsql = CreateFreeSql();
        Assert.Throws<NotSupportedException>(() => fsql.CodeFirst.SyncStructure<QualifiedRow>());
        Assert.Throws<NotSupportedException>(() => fsql.CodeFirst.SyncStructure<DescendingIndexRow>());
        Assert.DoesNotContain("sonnet_released_descending", fsql.Ado.Query<string>("SHOW TABLES"));
    }

    [Fact]
    public void MeasurementInnerJoin_BindsParametersAndReadsEveryMatchingSample()
    {
        using var fsql = CreateFreeSql();
        fsql.Insert(new Row { Id = 1, Name = "alpha" }).ExecuteAffrows();
        fsql.Insert(new[]
        {
            new Measurement { Time = DateTime.UnixEpoch.AddSeconds(1), Host = "alpha", Value = 1 },
            new Measurement { Time = DateTime.UnixEpoch.AddSeconds(2), Host = "alpha", Value = 2 }
        }).ExecuteAffrows();
        var rows = fsql.Select<Measurement, Row>().InnerJoin((sample, dimension) => sample.Host == dimension.Name)
            .Where((sample, dimension) => dimension.Id == 1)
            .OrderBy((sample, dimension) => sample.Time)
            .ToList((sample, dimension) => new { sample.Time, sample.Value, dimension.Id });
        Assert.Equal(2, rows.Count);
        Assert.Equal(new double[] { 1, 2 }, rows.Select(a => a.Value));
        Assert.Equal(DateTime.UnixEpoch.AddSeconds(2), rows[1].Time);
    }

    static IFreeSql CreateFreeSql(bool noneParameter = false) => new FreeSqlBuilder()
        .UseConnectionString(DataType.SonnetDB, $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-ReleasedCrud", Guid.NewGuid().ToString("N"))}")
        .UseNoneCommandParameter(noneParameter).UseAutoSyncStructure(true).Build();

    [SonnetDBTable, Table(Name = "sonnet_released_row")]
    sealed class Row
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long Value { get; set; }
    }

    [SonnetDBTable, Table(Name = "sonnet_released_archive")]
    sealed class ArchiveRow
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long Value { get; set; }
    }

    [SonnetDBTable, Table(Name = "sonnet_released_unique"), Index("ux_released_name", nameof(Name), true)]
    sealed class UniqueRow
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long Value { get; set; }
    }

    [SonnetDBTable, Table(Name = "sonnet_released_composite")]
    sealed class CompositeRow
    {
        [Column(IsPrimary = true)] public long Tenant { get; set; }
        [Column(IsPrimary = true)] public long Id { get; set; }
        public long Value { get; set; }
    }

    [SonnetDBTable, Table(Name = "sonnet_released_identity")]
    sealed class IdentityRow
    {
        [Column(IsPrimary = true, IsIdentity = true)] public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [SonnetDBTable, Table(Name = "sonnet_released_identity_unique"), Index("ux_released_identity_name", nameof(Name), true)]
    sealed class IdentityUniqueRow
    {
        [Column(IsPrimary = true, IsIdentity = true)] public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long Value { get; set; }
    }

    [SonnetDBTable, Table(Name = "sonnet_released_versioned")]
    sealed class VersionedRow
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
        [Column(Name = "db_name")] public string Name { get; set; } = string.Empty;
        [Column(IsVersion = true)] public long Version { get; set; }
    }

    [SonnetDBTable, Table(Name = "sonnet_released_temporal_returning")]
    sealed class TemporalReturningRow
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [Column(Name = "elapsed_ticks")] public TimeSpan Duration { get; set; }
        public TimeSpan? OptionalDuration { get; set; }
        public DateOnly Day { get; set; }
        public DateOnly? OptionalDay { get; set; }
        public TimeOnly Clock { get; set; }
        public TimeOnly? OptionalClock { get; set; }
        public DateTimeOffset Offset { get; set; }
        public DateTimeOffset? OptionalOffset { get; set; }
        public DateTime StartedAt { get; set; }
    }

    [SonnetDBTable, Table(Name = "sonnet_released_tree")]
    sealed class TreeRow
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
        public long ParentId { get; set; }
        public string Name { get; set; } = string.Empty;
        [Navigate(nameof(ParentId))] public List<TreeRow> Children { get; set; } = new();
    }

    [Table(Name = "sonnet_released_measurement")]
    sealed class Measurement
    {
        [Column(Name = "time")] public DateTime Time { get; set; }
        [SonnetDBTag] public string Host { get; set; } = string.Empty;
        [SonnetDBField] public double Value { get; set; }
    }

    [Table(Name = "sonnet_released_vector_measurement")]
    sealed class VectorMeasurement
    {
        [Column(Name = "time")] public DateTime Time { get; set; }
        [SonnetDBTag] public string Host { get; set; } = string.Empty;
        [Column(DbType = "FIELD VECTOR(2)")]
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }

    [SonnetDBTable, Table(Name = "sonnet_released_decimal")]
    sealed class DecimalRow
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
        [Column(Precision = 15, Scale = 3)] public decimal? Amount { get; set; }
    }

    [SonnetDBTable, Table(Name = "schema.sonnet_released_qualified")]
    sealed class QualifiedRow
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
    }

    [SonnetDBTable, Table(Name = "sonnet_released_descending"), Index("ix_released_descending", "Name DESC")]
    sealed class DescendingIndexRow
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
