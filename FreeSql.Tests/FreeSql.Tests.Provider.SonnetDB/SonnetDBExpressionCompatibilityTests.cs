using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using System.Globalization;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBExpressionCompatibilityTests
{
    [Fact]
    public void StringScalars_ExecuteWithUtf16IndicesAndLiteralDynamicPatterns()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var source = new ExpressionProbeRow
        {
            Id = 1,
            Text = " \u2003A%_\\'中🙂xy \t",
            Pattern = "%_\\'",
            OccurredAt = DateTime.UnixEpoch,
            FinishedAt = DateTime.UnixEpoch
        };
        Assert.Equal(1, fsql.Insert(source).ExecuteAffrows());

        var result = Assert.Single(fsql.Select<ExpressionProbeRow>().ToList(a => new
        {
            a.Text.Length,
            Trim = a.Text.Trim(),
            TrimStart = a.Text.TrimStart(' ', '\u2003'),
            TrimEnd = a.Text.TrimEnd(' ', '\t'),
            Slice = a.Text.Substring(2, 5),
            Tail = a.Text.Substring(2),
            Position = a.Text.IndexOf("中"),
            Replaced = a.Text.Replace("中", "汉"),
            Left = a.Text.PadLeft(25, '_'),
            Right = a.Text.PadRight(25),
            Contains = a.Text.Contains(a.Pattern),
            Starts = a.Text.StartsWith(a.Pattern),
            Ends = a.Text.EndsWith(a.Pattern)
        }));

        Assert.Equal(source.Text.Length, result.Length);
        Assert.Equal(source.Text.Trim(), result.Trim);
        Assert.Equal(source.Text.TrimStart(' ', '\u2003'), result.TrimStart);
        Assert.Equal(source.Text.TrimEnd(' ', '\t'), result.TrimEnd);
        Assert.Equal(source.Text.Substring(2, 5), result.Slice);
        Assert.Equal(source.Text.Substring(2), result.Tail);
        Assert.Equal(source.Text.IndexOf("中", StringComparison.Ordinal), result.Position);
        Assert.Equal(source.Text.Replace("中", "汉"), result.Replaced);
        Assert.Equal(source.Text.PadLeft(25, '_'), result.Left);
        Assert.Equal(source.Text.PadRight(25), result.Right);
        Assert.True(result.Contains);
        Assert.False(result.Starts);
        Assert.False(result.Ends);
    }

    [Theory]
    [InlineData(-1.25)]
    [InlineData(0.5)]
    public void MathScalars_ExecuteWithClrResults(double value)
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        Assert.Equal(1, fsql.Insert(new ExpressionProbeRow
        {
            Id = 1, Amount = value, OccurredAt = DateTime.UnixEpoch, FinishedAt = DateTime.UnixEpoch
        }).ExecuteAffrows());

        var result = Assert.Single(fsql.Select<ExpressionProbeRow>().ToList(a => new
        {
            Exp = Math.Exp(a.Amount),
            Ceiling = Math.Ceiling(a.Amount),
            Floor = Math.Floor(a.Amount),
            Power = Math.Pow(a.Amount, 2),
            Sin = Math.Sin(a.Amount),
            Cos = Math.Cos(a.Amount),
            Tan = Math.Tan(a.Amount),
            Asin = Math.Asin(a.Amount / 2),
            Acos = Math.Acos(a.Amount / 2),
            Atan = Math.Atan(a.Amount),
            Atan2 = Math.Atan2(a.Amount, 2)
        }));

        Assert.Equal(Math.Exp(value), result.Exp, 12);
        Assert.Equal(Math.Ceiling(value), result.Ceiling);
        Assert.Equal(Math.Floor(value), result.Floor);
        Assert.Equal(Math.Pow(value, 2), result.Power, 12);
        Assert.Equal(Math.Sin(value), result.Sin, 12);
        Assert.Equal(Math.Cos(value), result.Cos, 12);
        Assert.Equal(Math.Tan(value), result.Tan, 12);
        Assert.Equal(Math.Asin(value / 2), result.Asin, 12);
        Assert.Equal(Math.Acos(value / 2), result.Acos, 12);
        Assert.Equal(Math.Atan(value), result.Atan, 12);
        Assert.Equal(Math.Atan2(value, 2), result.Atan2, 12);
    }

    [Theory]
    [InlineData(-1.5)]
    [InlineData(2.5)]
    public void NumericConversions_ExecuteBeforeComparisonAndDistinguishCastFromConvert(double value)
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        Assert.Equal(1, fsql.Insert(new ExpressionProbeRow
        {
            Id = 1, Amount = value, Text = "12.75",
            OccurredAt = DateTime.UnixEpoch, FinishedAt = DateTime.UnixEpoch
        }).ExecuteAffrows());

        var truncated = (long)value;
        var rounded = Convert.ToInt64(value);
        Assert.Equal(1, fsql.Select<ExpressionProbeRow>()
            .Where(a => (long)a.Amount == truncated && Convert.ToInt64(a.Amount) == rounded).Count());
        var result = Assert.Single(fsql.Select<ExpressionProbeRow>().ToList(a => new
        {
            Floating = Convert.ToDouble(a.Text),
            Exact = Convert.ToDecimal(a.Text),
            Boolean = Convert.ToBoolean(a.Amount),
            Text = Convert.ToString(a.Integer)
        }));
        Assert.Equal(12.75, result.Floating);
        Assert.Equal(12.75m, result.Exact);
        Assert.True(result.Boolean);
        Assert.Equal("0", result.Text);
    }

    [Theory]
    [InlineData("9007199254740993.25")]
    [InlineData("-9007199254740993.25")]
    [InlineData("2.5")]
    [InlineData("3.5")]
    [InlineData("-2.5")]
    [InlineData("-3.5")]
    public void DecimalMathAndIntegerConversion_PreservePrecisionAndMidpointToEven(string text)
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var value = decimal.Parse(text, CultureInfo.InvariantCulture);
        Assert.Equal(1, fsql.Insert(new DecimalProbeRow { Id = 1, Value = value }).ExecuteAffrows());

        var row = Assert.Single(fsql.Select<DecimalProbeRow>().ToList(a => new
        {
            Absolute = Math.Abs(a.Value),
            Floor = Math.Floor(a.Value),
            Ceiling = Math.Ceiling(a.Value),
            Truncated = Math.Truncate(a.Value),
            RoundedInteger = Convert.ToInt64(a.Value)
        }));

        Assert.Equal(Math.Abs(value), row.Absolute);
        Assert.Equal(Math.Floor(value), row.Floor);
        Assert.Equal(Math.Ceiling(value), row.Ceiling);
        Assert.Equal(Math.Truncate(value), row.Truncated);
        Assert.Equal(Convert.ToInt64(value), row.RoundedInteger);
    }

    [Fact]
    public void DecimalRound_RejectsDatabaseFloat64PrecisionLoss()
    {
        using var fsql = CreateFreeSql();
        var exception = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<DecimalProbeRow>().ToSql(a => Math.Round(a.Value, 2)));

        Assert.Contains("DECIMAL", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Float64", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Math.Round(decimal)", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(1, 31, 1L)]
    [InlineData(-17, -1, -17L)]
    [InlineData(int.MaxValue, 32, long.MaxValue)]
    [InlineData(int.MinValue, 65, long.MinValue)]
    public void IntegerShifts_MaskCountsAndPreserveClrWidth(int value, int count, long wide)
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        Assert.Equal(1, fsql.Insert(new ExpressionProbeRow
        {
            Id = 1, Integer = value, Shift = count, WideInteger = wide,
            OccurredAt = DateTime.UnixEpoch, FinishedAt = DateTime.UnixEpoch
        }).ExecuteAffrows());

        var result = Assert.Single(fsql.Select<ExpressionProbeRow>().ToList(a => new
        {
            Left = a.Integer << a.Shift,
            Right = a.Integer >> a.Shift,
            WideLeft = a.WideInteger << a.Shift,
            WideRight = a.WideInteger >> a.Shift
        }));

        Assert.Equal(value << count, result.Left);
        Assert.Equal(value >> count, result.Right);
        Assert.Equal(wide << count, result.WideLeft);
        Assert.Equal(wide >> count, result.WideRight);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    public void DateDifference_ExecutesNegativeComponentsAndTickArithmetic(int direction)
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var start = new DateTime(2026, 10, 1, 12, 34, 56, DateTimeKind.Utc);
        var difference = new TimeSpan(2, 3, 4, 5, 678);
        if (direction < 0) difference = -difference;
        var finish = start.Add(difference);
        var extraTicks = direction * 1234L;
        difference = difference.Add(TimeSpan.FromTicks(extraTicks));
        Assert.Equal(1, fsql.Insert(new ExpressionProbeRow
        {
            Id = 1, OccurredAt = start, FinishedAt = finish
        }).ExecuteAffrows());

        var result = Assert.Single(fsql.Select<ExpressionProbeRow>().ToList(a => new
        {
            Days = (a.FinishedAt.AddTicks(extraTicks) - a.OccurredAt).Days,
            Hours = (a.FinishedAt.AddTicks(extraTicks) - a.OccurredAt).Hours,
            Minutes = a.FinishedAt.AddTicks(extraTicks).Subtract(a.OccurredAt).Minutes,
            Seconds = a.FinishedAt.AddTicks(extraTicks).Subtract(a.OccurredAt).Seconds,
            Milliseconds = a.FinishedAt.AddTicks(extraTicks).Subtract(a.OccurredAt).Milliseconds,
            Ticks = (a.FinishedAt.AddTicks(extraTicks) - a.OccurredAt).Ticks,
            TotalDays = (a.FinishedAt.AddTicks(extraTicks) - a.OccurredAt).TotalDays,
            TotalHours = (a.FinishedAt.AddTicks(extraTicks) - a.OccurredAt).TotalHours,
            TotalMinutes = (a.FinishedAt.AddTicks(extraTicks) - a.OccurredAt).TotalMinutes,
            TotalSeconds = (a.FinishedAt.AddTicks(extraTicks) - a.OccurredAt).TotalSeconds,
            TotalMilliseconds = (a.FinishedAt.AddTicks(extraTicks) - a.OccurredAt).TotalMilliseconds,
            Formatted = a.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss.fffffff")
        }));

        Assert.Equal(difference.Days, result.Days);
        Assert.Equal(difference.Hours, result.Hours);
        Assert.Equal(difference.Minutes, result.Minutes);
        Assert.Equal(difference.Seconds, result.Seconds);
        Assert.Equal(difference.Milliseconds, result.Milliseconds);
        Assert.Equal(difference.Ticks, result.Ticks);
        Assert.Equal(difference.TotalDays, result.TotalDays, 10);
        Assert.Equal(difference.TotalHours, result.TotalHours, 10);
        Assert.Equal(difference.TotalMinutes, result.TotalMinutes, 10);
        Assert.Equal(difference.TotalSeconds, result.TotalSeconds, 10);
        Assert.Equal(difference.TotalMilliseconds, result.TotalMilliseconds, 6);
        Assert.Equal(start.ToString("yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture), result.Formatted);
    }

    [Fact]
    public void ExistingSqlExtWindows_ExecutePartitionedRankAndAggregate()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        Assert.Equal(4, fsql.Insert(new[]
        {
            new ExpressionProbeRow { Id = 1, Pattern = "a", Amount = 2, OccurredAt = DateTime.UnixEpoch, FinishedAt = DateTime.UnixEpoch },
            new ExpressionProbeRow { Id = 2, Pattern = "a", Amount = 2, OccurredAt = DateTime.UnixEpoch, FinishedAt = DateTime.UnixEpoch },
            new ExpressionProbeRow { Id = 3, Pattern = "a", Amount = 5, OccurredAt = DateTime.UnixEpoch, FinishedAt = DateTime.UnixEpoch },
            new ExpressionProbeRow { Id = 4, Pattern = "b", Amount = 7, OccurredAt = DateTime.UnixEpoch, FinishedAt = DateTime.UnixEpoch }
        }).ExecuteAffrows());

        var rows = fsql.Select<ExpressionProbeRow>().OrderBy(a => a.Id).ToList(a => new
        {
            a.Id,
            Number = SqlExt.RowNumber().Over().PartitionBy(a.Pattern).OrderBy(a.Id).ToValue(),
            Rank = SqlExt.Rank().Over().PartitionBy(a.Pattern).OrderBy(a.Amount).ToValue(),
            DenseRank = SqlExt.DenseRank().Over().PartitionBy(a.Pattern).OrderBy(a.Amount).ToValue(),
            Tile = SqlExt.Ntile(2).Over().PartitionBy(a.Pattern).OrderBy(a.Id).ToValue(),
            Total = SqlExt.Sum(a.Amount).Over().PartitionBy(a.Pattern).ToValue()
        });

        Assert.Equal(new[] { 1, 2, 3, 1 }, rows.Select(a => a.Number));
        Assert.Equal(new[] { 1, 1, 3, 1 }, rows.Select(a => a.Rank));
        Assert.Equal(new[] { 1, 1, 2, 1 }, rows.Select(a => a.DenseRank));
        Assert.Equal(new[] { 1, 1, 2, 1 }, rows.Select(a => a.Tile));
        Assert.Equal(new[] { 9m, 9m, 9m, 7m }, rows.Select(a => a.Total));
    }

    [Fact]
    public void ExistingSqlExtStringAgg_ExecutesGroupedValuesWithNullsAndDuplicates()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        Assert.Equal(7, fsql.Insert(new[]
        {
            new ExpressionProbeRow { Id = 1, Pattern = "a", Text = "one" },
            new ExpressionProbeRow { Id = 2, Pattern = "a", Text = null! },
            new ExpressionProbeRow { Id = 3, Pattern = "a", Text = "one" },
            new ExpressionProbeRow { Id = 4, Pattern = "a", Text = "two" },
            new ExpressionProbeRow { Id = 5, Pattern = "b", Text = null! },
            new ExpressionProbeRow { Id = 6, Pattern = "b", Text = "other" },
            new ExpressionProbeRow { Id = 7, Pattern = "c", Text = null! }
        }).ExecuteAffrows());

        var rows = fsql.Select<ExpressionProbeRow>()
            .GroupBy(a => a.Pattern)
            .ToList(group => new
            {
                Pattern = group.Key,
                Joined = SqlExt.StringAgg(group.Value.Text, "::")
            }).ToDictionary(row => row.Pattern, row => row.Joined);

        Assert.Equal(3, rows.Count);
        Assert.Equal(new[] { "one", "one", "two" },
            rows["a"].Split("::", StringSplitOptions.None).OrderBy(value => value, StringComparer.Ordinal));
        Assert.Equal("other", rows["b"]);
        Assert.Null(rows["c"]);
    }

    [Theory]
    [InlineData(1838456781234L)]
    [InlineData(-1838456781234L)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void EntityTimeSpanMembers_KeepTickStorageAndNegativeComponents(long ticks)
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var duration = TimeSpan.FromTicks(ticks);
        Assert.Equal(1, fsql.Insert(new DurationProbeRow { Id = 1, Duration = duration }).ExecuteAffrows());

        var row = Assert.Single(fsql.Select<DurationProbeRow>().ToList(a => new
        {
            a.Duration.Ticks,
            a.Duration.Days,
            a.Duration.Hours,
            a.Duration.Minutes,
            a.Duration.Seconds,
            a.Duration.Milliseconds,
            a.Duration.TotalDays,
            a.Duration.TotalHours,
            a.Duration.TotalMinutes,
            a.Duration.TotalSeconds,
            a.Duration.TotalMilliseconds
        }));

        Assert.Equal(duration.Ticks, row.Ticks);
        Assert.Equal(duration.Days, row.Days);
        Assert.Equal(duration.Hours, row.Hours);
        Assert.Equal(duration.Minutes, row.Minutes);
        Assert.Equal(duration.Seconds, row.Seconds);
        Assert.Equal(duration.Milliseconds, row.Milliseconds);
        Assert.InRange(Math.Abs(duration.TotalDays - row.TotalDays), 0, Math.Max(1, Math.Abs(duration.TotalDays)) * 1e-14);
        Assert.InRange(Math.Abs(duration.TotalHours - row.TotalHours), 0, Math.Max(1, Math.Abs(duration.TotalHours)) * 1e-14);
        Assert.InRange(Math.Abs(duration.TotalMinutes - row.TotalMinutes), 0, Math.Max(1, Math.Abs(duration.TotalMinutes)) * 1e-14);
        Assert.InRange(Math.Abs(duration.TotalSeconds - row.TotalSeconds), 0, Math.Max(1, Math.Abs(duration.TotalSeconds)) * 1e-14);
        Assert.InRange(Math.Abs(duration.TotalMilliseconds - row.TotalMilliseconds), 0, Math.Max(1, Math.Abs(duration.TotalMilliseconds)) * 1e-14);
    }

    [Fact]
    public void DateDifference_DirectTimeSpanProjectionUsesTickUnits()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var start = new DateTime(2026, 10, 1, 12, 34, 56, DateTimeKind.Utc);
        var difference = new TimeSpan(-2, -3, -4, -5, -678);
        Assert.Equal(1, fsql.Insert(new ExpressionProbeRow
        {
            Id = 1, OccurredAt = start, FinishedAt = start.Add(difference)
        }).ExecuteAffrows());

        var result = Assert.Single(fsql.Select<ExpressionProbeRow>().ToList(a => new
        {
            Method = a.FinishedAt.Subtract(a.OccurredAt),
            Binary = a.FinishedAt - a.OccurredAt
        }));

        Assert.Equal(difference, result.Method);
        Assert.Equal(difference, result.Binary);
    }

    [Fact]
    public async Task DateDifference_MultiTableDurationProjectionsPreserveTickArithmetic()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var start = new DateTime(2026, 10, 1, 12, 34, 56, DateTimeKind.Utc);
        // The published relational DATETIME contract stores milliseconds.
        // Int64 duration columns cover tick precision independently.
        var difference = TimeSpan.FromTicks(-1838456780000);
        Assert.Equal(1, fsql.Insert(new ExpressionProbeRow
        {
            Id = 1, OccurredAt = start, FinishedAt = start.Add(difference)
        }).ExecuteAffrows());

        var scalar = Assert.Single(fsql.Select<ExpressionProbeRow, ExpressionProbeRow>()
            .InnerJoin((a, b) => a.Id == b.Id)
            .ToList((a, b) => b.FinishedAt - a.OccurredAt));
        var result = Assert.Single(await fsql.Select<ExpressionProbeRow, ExpressionProbeRow>()
            .InnerJoin((a, b) => a.Id == b.Id)
            .Where((a, b) => (b.FinishedAt - a.OccurredAt).Ticks < -1)
            .OrderBy((a, b) => (b.FinishedAt - a.OccurredAt).Ticks)
            .ToListAsync((a, b) => new
            {
                Method = b.FinishedAt.Subtract(a.OccurredAt),
                Binary = b.FinishedAt - a.OccurredAt,
                Days = (b.FinishedAt - a.OccurredAt).Days,
                Ticks = (b.FinishedAt - a.OccurredAt).Ticks
            }));

        Assert.Equal(difference, scalar);
        Assert.Equal(difference, result.Method);
        Assert.Equal(difference, result.Binary);
        Assert.Equal(difference.Days, result.Days);
        Assert.Equal(difference.Ticks, result.Ticks);
    }

    [Fact]
    public void MathRound_MidpointRoundingOverloads_AreRejected()
    {
        using var fsql = CreateFreeSql();

        var mode = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<ExpressionProbeRow>()
                .Where(a => Math.Round(a.Amount, MidpointRounding.AwayFromZero) > 0)
                .ToSql());
        Assert.Contains("MidpointRounding", mode.Message, StringComparison.Ordinal);
        Assert.Contains("数学语义", mode.Message, StringComparison.Ordinal);

        var digitsAndMode = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<ExpressionProbeRow>()
                .Where(a => Math.Round(a.Amount, 2, MidpointRounding.AwayFromZero) > 0)
                .ToSql());
        Assert.Contains("MidpointRounding", digitsAndMode.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DateTimeParse_DynamicColumn_IsRejected()
    {
        using var fsql = CreateFreeSql();

        var exception = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<ExpressionProbeRow>()
                .Where(a => DateTime.Parse(a.Text) > a.OccurredAt)
                .ToSql());

        Assert.Contains("DateTime.Parse", exception.Message, StringComparison.Ordinal);
        Assert.Contains("动态字符串", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DateTimeParse_CapturedConstant_IsRenderedAsUnixMilliseconds()
    {
        using var fsql = CreateFreeSql();
        var text = "2024-01-02T03:04:05Z";
        var expected = DateTimeOffset.Parse(text, CultureInfo.InvariantCulture).ToUnixTimeMilliseconds();

        var sql = fsql.Select<ExpressionProbeRow>()
            .Where(a => DateTime.Parse(text) > a.OccurredAt)
            .ToSql();

        Assert.Contains(expected.ToString(CultureInfo.InvariantCulture), sql, StringComparison.Ordinal);
    }

    [Fact]
    public void ConvertProviderOverloads_AreRejected()
    {
        using var fsql = CreateFreeSql();

        var exception = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<ExpressionProbeRow>()
                .Where(a => Convert.ToInt32(a.Text, CultureInfo.InvariantCulture) > 0)
                .ToSql());

        Assert.Contains("Convert.ToInt32", exception.Message, StringComparison.Ordinal);
        Assert.Contains("IFormatProvider", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ConvertToDateTime_DynamicString_IsRejected()
    {
        using var fsql = CreateFreeSql();

        var exception = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<ExpressionProbeRow>()
                .Where(a => Convert.ToDateTime(a.Text) > a.OccurredAt)
                .ToSql());

        Assert.Contains("Convert.ToDateTime", exception.Message, StringComparison.Ordinal);
        Assert.Contains("动态字符串", exception.Message, StringComparison.Ordinal);
    }

    static IFreeSql CreateFreeSql(bool autoSyncStructure = false)
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-ExpressionCompatibilityTests", Guid.NewGuid().ToString("N"));
        return new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseAutoSyncStructure(autoSyncStructure)
            .UseNoneCommandParameter(!autoSyncStructure)
            .Build();
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_decimal_expression_probe")]
    sealed class DecimalProbeRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public decimal Value { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_duration_probe")]
    sealed class DurationProbeRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public TimeSpan Duration { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_expression_probe")]
    sealed class ExpressionProbeRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public double Amount { get; set; }

        public int Integer { get; set; }

        public long WideInteger { get; set; }

        public int Shift { get; set; }

        [Column(IsNullable = true)]
        public string Text { get; set; } = string.Empty;

        public string Pattern { get; set; } = string.Empty;

        public DateTime OccurredAt { get; set; }

        public DateTime FinishedAt { get; set; }
    }
}
