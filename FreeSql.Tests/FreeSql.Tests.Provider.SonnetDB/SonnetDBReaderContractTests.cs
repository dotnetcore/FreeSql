using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBReaderContractTests
{
    [Fact]
    public async Task NullableTemporalColumns_PreserveNullInEntitiesAndProjections()
    {
        using var fsql = CreateFreeSql();
        fsql.Insert(new NullableTimeRow { Id = 1 }).ExecuteAffrows();

        var entity = fsql.Select<NullableTimeRow>().ToOne();
        Assert.Null(entity.StartedAt);
        Assert.Null(entity.ObservedAt);
        var projected = (await fsql.Select<NullableTimeRow>()
            .ToListAsync(row => new { row.StartedAt, row.ObservedAt })).Single();
        Assert.Null(projected.StartedAt);
        Assert.Null(projected.ObservedAt);
    }

    [Fact]
    public async Task Measurement_NumericTimeAndHourProjections_RemainNumeric()
    {
        using var fsql = CreateFreeSql();
        var time = new DateTime(2026, 7, 15, 12, 0, 0, DateTimeKind.Utc);
        fsql.Insert(new TimeMetric { Time = time, Value = 2 }).ExecuteAffrows();

        Assert.Equal(new DateTimeOffset(time).ToUnixTimeMilliseconds(),
            fsql.Select<TimeMetric>().ToList<long>("time").Single());
        Assert.Equal(12, fsql.Select<TimeMetric>().ToList(row => row.Time.Hour).Single());
        Assert.Equal(12, (await fsql.Select<TimeMetric>().ToListAsync(row => row.Time.Hour)).Single());
        Assert.Equal(time, fsql.Select<TimeMetric>().ToOne().Time);
        Assert.Equal(time, fsql.Select<TimeMetric>().ToList(row => row.Time).Single());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DateOnlyTimeOnlyAndDurations_RoundTripWithoutLosingTicks(bool noneParameter)
    {
        using var fsql = CreateFreeSql(noneParameter);
        var durations = new[] { TimeSpan.Zero, TimeSpan.FromTicks(123456789),
            TimeSpan.FromTicks(-123456789), TimeSpan.FromDays(3) + TimeSpan.FromTicks(1),
            TimeSpan.MinValue, TimeSpan.MaxValue };
        var source = durations.Select((duration, index) => new ClockRow
        {
            Id = index + 1,
            Day = new DateOnly(2026, 10, 15),
            Clock = new TimeOnly(12, 34, 56).Add(TimeSpan.FromTicks(7890123)),
            Duration = duration,
            OptionalDuration = index == 0 ? null : duration
        }).ToArray();
        Assert.Equal(source.Length, fsql.Insert(source).ExecuteAffrows());
        var loaded = await fsql.Select<ClockRow>().OrderBy(row => row.Id).ToListAsync();
        Assert.Equal(source.Select(row => row.Day), loaded.Select(row => row.Day));
        Assert.Equal(source.Select(row => row.Clock), loaded.Select(row => row.Clock));
        Assert.Equal(durations, loaded.Select(row => row.Duration));
        Assert.Equal(source.Select(row => row.OptionalDuration), loaded.Select(row => row.OptionalDuration));
        Assert.Equal(durations, fsql.Select<ClockRow>().OrderBy(row => row.Id).ToList(row => row.Duration));
        Assert.Equal(durations, fsql.Select<ClockRow>().OrderBy(row => row.Id).ToList<TimeSpan>("\"Duration\""));
        Assert.Equal(durations, await fsql.Select<ClockRow>().OrderBy(row => row.Id).ToListAsync<TimeSpan>("\"Duration\""));
        Assert.Equal(source.Select(row => row.OptionalDuration), fsql.Select<ClockRow>().OrderBy(row => row.Id)
            .ToList(row => row.OptionalDuration));
        Assert.Equal(source.Select(row => row.OptionalDuration), await fsql.Select<ClockRow>().OrderBy(row => row.Id)
            .ToListAsync(row => row.OptionalDuration));
        Assert.Equal(TimeSpan.MinValue, fsql.Select<ClockRow>().OrderBy(row => row.Duration).ToOne().Duration);
        Assert.Equal(TimeSpan.MinValue, fsql.Select<ClockRow>().OrderBy(row => row.Duration).ToList(row => row.Duration).First());
    }

    static IFreeSql CreateFreeSql(bool noneParameter = false) => new FreeSqlBuilder()
        .UseConnectionString(DataType.SonnetDB,
            $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-ReaderContract", Guid.NewGuid().ToString("N"))}")
        .UseAutoSyncStructure(true)
        .UseNoneCommandParameter(noneParameter)
        .Build();

    [SonnetDBTable]
    [Table(Name = "sonnet_nullable_reader")]
    sealed class NullableTimeRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTimeOffset? ObservedAt { get; set; }
    }

    [Table(Name = "sonnet_numeric_time_projection")]
    sealed class TimeMetric
    {
        [Column(Name = "time")]
        public DateTime Time { get; set; }
        public double Value { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_clock_reader")]
    sealed class ClockRow
    {
        [Column(IsPrimary = true)] public long Id { get; set; }
        public DateOnly Day { get; set; }
        public TimeOnly Clock { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan? OptionalDuration { get; set; }
    }
}
