using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using FreeSql.SonnetDB;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBDateTimeTests
{
    [Fact]
    public void RelationshipTable_DateTimeExpressions_UseNativeFunctions()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<TemporalRecord>()
            .Where(a => a.OccurredAt.Year == 2026 &&
                        a.OccurredAt.AddDays(1) <= DateTime.Now &&
                        a.OffsetAt.AddHours(2) <= DateTimeOffset.UtcNow &&
                        a.OffsetAt.ToUnixTimeMilliseconds() > 0)
            .ToSql();

        Assert.Contains("date_part('year', a.\"OccurredAt\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("date_add_datetime(a.\"OccurredAt\", 1, 'day')", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("current_datetime()", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("date_add_datetime_offset(a.\"OffsetAt\", 2, 'hour')", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("current_utc_datetime_offset()", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("to_unix_milliseconds(a.\"OffsetAt\")", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RelationshipTable_DateTimeAndOffset_CompareWithUnixMillisecondLiterals()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var occurredAt = new DateTime(2026, 7, 10, 15, 48, 56, DateTimeKind.Utc);
        var offsetAt = new DateTimeOffset(2026, 7, 10, 15, 48, 56, TimeSpan.Zero);
        var source = new TemporalRecord
        {
            Id = 1,
            OccurredAt = occurredAt,
            OffsetAt = offsetAt
        };

        Assert.Equal(1, fsql.Insert(source).ExecuteAffrows());

        var sql = fsql.Select<TemporalRecord>()
            .Where(a => a.OccurredAt == occurredAt && a.OffsetAt == offsetAt)
            .ToSql();
        Assert.Contains(new DateTimeOffset(occurredAt).ToUnixTimeMilliseconds().ToString(), sql, StringComparison.Ordinal);
        Assert.Contains(offsetAt.ToUnixTimeMilliseconds().ToString(), sql, StringComparison.Ordinal);

        var loaded = fsql.Select<TemporalRecord>()
            .Where(a => a.OccurredAt == occurredAt && a.OffsetAt == offsetAt)
            .ToOne();

        Assert.NotNull(loaded);
        Assert.Equal(occurredAt, loaded!.OccurredAt);
        Assert.Equal(offsetAt, loaded.OffsetAt);
    }

    [Fact]
    public void RelationshipTable_DateTimeAndOffset_UseNativeParameters()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true, noneCommandParameter: false,
            generateCommandParameterWithLambda: true);
        var occurredAt = new DateTime(2026, 7, 11, 15, 48, 56, DateTimeKind.Utc);
        var offsetAt = new DateTimeOffset(2026, 7, 11, 15, 48, 56, TimeSpan.Zero);
        Aop.CurdBeforeEventArgs? selectCommand = null;
        fsql.Aop.CurdBefore += (_, e) =>
        {
            if (e.CurdType == Aop.CurdType.Select) selectCommand = e;
        };

        Assert.Equal(1, fsql.Insert(new TemporalRecord
        {
            Id = 2,
            OccurredAt = occurredAt,
            OffsetAt = offsetAt
        }).ExecuteAffrows());

        var loaded = fsql.Select<TemporalRecord>()
            .Where(a => a.OccurredAt == occurredAt && a.OffsetAt == offsetAt)
            .ToOne();

        Assert.NotNull(loaded);
        Assert.NotNull(selectCommand?.DbParms);
        Assert.Contains(selectCommand!.DbParms!, parameter => parameter.Value is DateTime);
        Assert.Contains(selectCommand.DbParms!, parameter => parameter.Value is DateTimeOffset);
    }

    static IFreeSql CreateFreeSql(bool autoSyncStructure = false, bool noneCommandParameter = true,
        bool generateCommandParameterWithLambda = false)
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-DateTimeTests", Guid.NewGuid().ToString("N"));
        var builder = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseNoneCommandParameter(noneCommandParameter)
            .UseGenerateCommandParameterWithLambda(generateCommandParameterWithLambda);
        if (autoSyncStructure) builder.UseAutoSyncStructure(true);
        return builder.Build();
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_temporal_record")]
    sealed class TemporalRecord
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public DateTime OccurredAt { get; set; }

        public DateTimeOffset OffsetAt { get; set; }
    }
}
