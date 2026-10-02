using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBTemporalOffsetExecutionTests
{
    [Fact]
    public void Measurement_TemporalParameters_UseInt64DbType()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-TemporalParameterTypeTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseAutoSyncStructure(true)
            .UseNoneCommandParameter(false)
            .UseGenerateCommandParameterWithLambda(true)
            .Build();

        var value = new DateTimeOffset(2026, 7, 15, 9, 10, 11, TimeSpan.FromHours(8));
        fsql.Aop.CurdBefore += (_, e) =>
        {
            if (e.CurdType == Aop.CurdType.Select && e.DbParms?.Any() == true)
                Assert.Contains(e.DbParms, parameter =>
                    parameter.Value is long && parameter.DbType == System.Data.DbType.Int64);
        };

        _ = fsql.Select<MeasurementOffset>()
            .Where(a => a.Time == value)
            .ToOne();
    }

    [Fact]
    public void Measurement_DateTimeOffset_RoundTripsAndFilters()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-TemporalOffsetTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseAutoSyncStructure(true)
            .Build();

        var value = new DateTimeOffset(2026, 7, 12, 15, 48, 56, TimeSpan.FromHours(8));
        var row = new MeasurementOffset { Time = value, Host = "edge-1", Value = 1.5 };
        Assert.Equal(1, fsql.Insert(row).ExecuteAffrows());

        var loaded = fsql.Select<MeasurementOffset>()
            .Where(a => a.Time == value)
            .ToOne();

        Assert.NotNull(loaded);
        Assert.Equal(value.ToUniversalTime(), loaded!.Time.ToUniversalTime());

        var projected = fsql.Select<MeasurementOffset>()
            .Where(a => a.Time == value)
            .ToList(a => new { a.Time })
            .Single();
        Assert.Equal(value.ToUniversalTime(), projected.Time.ToUniversalTime());
    }

    [Fact]
    public void Measurement_DateTime_RoundTripsAndFilters()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-TemporalDateTimeTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseAutoSyncStructure(true)
            .Build();

        var value = new DateTime(2026, 7, 13, 15, 48, 56, DateTimeKind.Utc);
        Assert.Equal(1, fsql.Insert(new MeasurementDateTime
        {
            Time = value,
            Host = "edge-1",
            Value = 2.5
        }).ExecuteAffrows());

        var loaded = fsql.Select<MeasurementDateTime>()
            .Where(a => a.Time == value)
            .ToOne();

        Assert.NotNull(loaded);
        Assert.Equal(value, loaded!.Time);
    }

    [Fact]
    public async Task Measurement_TemporalScalarAndAsyncProjection_RoundTrip()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-TemporalScalarTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseAutoSyncStructure(true)
            .Build();

        var value = new DateTimeOffset(2026, 7, 14, 9, 10, 11, TimeSpan.FromHours(8));
        var row = new MeasurementOffset { Time = value, Host = "scalar-1", Value = 3.5 };
        Assert.Equal(1, fsql.Insert(row).ExecuteAffrows());

        var scalar = fsql.Select<MeasurementOffset>()
            .Where(a => a.Host == row.Host)
            .ToList<DateTimeOffset>("time")
            .Single();
        Assert.Equal(value.ToUniversalTime(), scalar.ToUniversalTime());

        var asyncProjected = (await fsql.Select<MeasurementOffset>()
            .Where(a => a.Host == row.Host)
            .ToListAsync(a => new { a.Time }))
            .Single();
        Assert.Equal(value.ToUniversalTime(), asyncProjected.Time.ToUniversalTime());
    }

    [Table(Name = "sonnet_measurement_offset")]
    sealed class MeasurementOffset
    {
        [Column(Name = "time")]
        public DateTimeOffset Time { get; set; }

        [SonnetDBTag]
        public string Host { get; set; } = string.Empty;

        [SonnetDBField]
        public double Value { get; set; }
    }

    [Table(Name = "sonnet_measurement_datetime")]
    sealed class MeasurementDateTime
    {
        [Column(Name = "time")]
        public DateTime Time { get; set; }

        [SonnetDBTag]
        public string Host { get; set; } = string.Empty;

        [SonnetDBField]
        public double Value { get; set; }
    }
}
