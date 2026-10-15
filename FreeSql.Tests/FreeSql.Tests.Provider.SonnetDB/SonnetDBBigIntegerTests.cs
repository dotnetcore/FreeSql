using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using System.Numerics;

namespace FreeSql.Tests.Provider.SonnetDB;

public sealed class SonnetDBBigIntegerTests
{
    [Fact]
    public void CodeFirst_BigInteger_IsRejectedWithChineseCompatibilityError()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-BigIntegerTests", Guid.NewGuid().ToString("N"))}")
            .Build();

        var exception = Assert.Throws<NotSupportedException>(() =>
            fsql.CodeFirst.GetComparisonDDLStatements<BigIntegerRow>());

        Assert.Contains("BigInteger", exception.Message, StringComparison.Ordinal);
        Assert.Contains("任意精度", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NonParameterizedBigInteger_IsRejectedBeforeSqlFormatting()
    {
        var exception = Assert.Throws<NotSupportedException>(() =>
            "select {0}".FormatSonnetDB(BigInteger.Parse("9223372036854775808")));

        Assert.Contains("BigInteger", exception.Message, StringComparison.Ordinal);
        Assert.Contains("任意精度", exception.Message, StringComparison.Ordinal);
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_big_integer_row")]
    private sealed class BigIntegerRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public BigInteger Value { get; set; }
    }
}
