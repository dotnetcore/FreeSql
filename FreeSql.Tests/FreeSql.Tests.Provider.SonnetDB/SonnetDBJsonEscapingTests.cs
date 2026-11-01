using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using System.Text.Json;

namespace FreeSql.Tests.Provider.SonnetDB;

public sealed class SonnetDBJsonEscapingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void JsonDocument_WithEscapedText_RoundTrips(bool noneCommandParameter)
    {
        using var fsql = CreateFreeSql(noneCommandParameter);
        using var document = JsonDocument.Parse("{\"path\":\"C:\\\\设备\\\\泵\",\"text\":\"引号 \\\" 与换行\\n保留\"}");
        var source = new EscapedJsonRow { Id = 1, Payload = document };

        Assert.Equal(1, fsql.Insert(source).ExecuteAffrows());
        var loaded = fsql.Select<EscapedJsonRow>().Where(row => row.Id == source.Id).ToOne();

        Assert.NotNull(loaded?.Payload);
        Assert.Equal(document.RootElement.GetRawText(), loaded!.Payload!.RootElement.GetRawText());
        loaded.Payload.Dispose();
    }

    static IFreeSql CreateFreeSql(bool noneCommandParameter)
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-JsonEscapingTests", Guid.NewGuid().ToString("N"));
        var builder = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseNoneCommandParameter(noneCommandParameter)
            .UseGenerateCommandParameterWithLambda(!noneCommandParameter)
            .UseAutoSyncStructure(true);
        return builder.Build();
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_json_escaping")]
    sealed class EscapedJsonRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public JsonDocument? Payload { get; set; }
    }
}
