using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using FreeSql.SonnetDB;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBNullableDateTimeTests
{
    [Fact]
    public void DateTimeOffsetBinarySubtract_UsesUnixMillisecondDifference()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<NullableTemporalRow>()
            .Where(a => (a.OccurredAt - DateTimeOffset.UtcNow).TotalSeconds < 0)
            .ToSql();

        Assert.Contains("to_unix_milliseconds", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/ 1000.0", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NullableDateTimeValueMembers_UseDateFunctions()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<NullableTemporalRow>()
            .Where(a => a.StartedAt.Value.Year == 2026 &&
                        a.StartedAt.Value.TimeOfDay > TimeSpan.FromHours(1))
            .ToSql();

        Assert.Contains("date_part('year'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("date_part('hour'", sql, StringComparison.OrdinalIgnoreCase);
    }

    static IFreeSql CreateFreeSql()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-NullableDateTimeTests", Guid.NewGuid().ToString("N"));
        return new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .Build();
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_nullable_temporal")]
    sealed class NullableTemporalRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTimeOffset OccurredAt { get; set; }
    }
}
