using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using System.Globalization;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBExpressionCompatibilityTests
{
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

    static IFreeSql CreateFreeSql()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-ExpressionCompatibilityTests", Guid.NewGuid().ToString("N"));
        return new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .Build();
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_expression_probe")]
    sealed class ExpressionProbeRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public double Amount { get; set; }

        public string Text { get; set; } = string.Empty;

        public DateTime OccurredAt { get; set; }
    }
}
