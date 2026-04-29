using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBProviderTests
{
    static readonly DateTime TestTime = new(2026, 4, 30, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Build_WithSonnetDBDataType_WiresProvider()
    {
        using var fsql = CreateFreeSql();

        Assert.Equal(DataType.SonnetDB, fsql.Ado.DataType);
        Assert.StartsWith("FreeSql.SonnetDB.SonnetDBProvider`1", fsql.GetType().FullName);
    }

    [Fact]
    public void Select_ToSql_GeneratesSonnetDBSql()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<SonnetMetric>()
            .Where(a => a.Host == "server-01" && a.Time >= TestTime)
            .Limit(10)
            .ToSql();

        Assert.Equal(
            "SELECT time, \"Host\", \"Usage\", \"Ok\" \r\n" +
            "FROM \"sonnet_metric\" \r\n" +
            "WHERE (\"Host\" = 'server-01' AND time >= 1777536000000) \r\n" +
            "limit 10",
            sql);
    }

    [Fact]
    public void Insert_ToSql_GeneratesSonnetDBSql()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Insert(new SonnetMetric
        {
            Time = TestTime,
            Host = "server-01",
            Usage = 0.71,
            Ok = true
        }).ToSql();

        Assert.Equal(
            "INSERT INTO \"sonnet_metric\"(time, \"Host\", \"Usage\", \"Ok\") VALUES(1777536000000, 'server-01', 0.71, true)",
            sql);
    }

    [Fact]
    public void Delete_ToSql_GeneratesSonnetDBSql()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Delete<SonnetMetric>()
            .Where(a => a.Host == "server-01")
            .ToSql();

        Assert.Equal("DELETE FROM \"sonnet_metric\" WHERE (\"Host\" = 'server-01')", sql);
    }

    static IFreeSql CreateFreeSql()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-Tests", Guid.NewGuid().ToString("N"));
        return new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseNoneCommandParameter(true)
            .Build();
    }

    [Table(Name = "sonnet_metric")]
    sealed class SonnetMetric
    {
        [Column(Name = "time")]
        public DateTime Time { get; set; }

        [SonnetDBTag]
        public string Host { get; set; } = "";

        [SonnetDBField]
        public double Usage { get; set; }

        [SonnetDBField]
        public bool Ok { get; set; }
    }
}
