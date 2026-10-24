using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using FreeSql.SonnetDB;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBJsonClrTests
{
    [Fact]
    public void RelationshipTable_JsonClrTypes_RoundTripThroughCodeFirstAndCrud()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true, noneCommandParameter: false,
            generateCommandParameterWithLambda: true);
        var ddl = fsql.CodeFirst.GetComparisonDDLStatements<JsonClrRecord>();

        Assert.Contains("\"Document\" JSON NULL", ddl, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"Element\" JSON NOT NULL", ddl, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"Node\" JSON NULL", ddl, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"OptionalElement\" JSON NULL", ddl, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"ObjectNode\" JSON NULL", ddl, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"ArrayNode\" JSON NULL", ddl, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"ValueNode\" JSON NULL", ddl, StringComparison.OrdinalIgnoreCase);

        Aop.CurdBeforeEventArgs? insertCommand = null;
        fsql.Aop.CurdBefore += (_, e) =>
        {
            if (e.CurdType == Aop.CurdType.Insert) insertCommand = e;
        };

        using var document = JsonDocument.Parse("{\"kind\":\"document\",\"value\":1}");
        using var elementSource = JsonDocument.Parse("{\"kind\":\"element\",\"value\":2}");
        var node = JsonNode.Parse("{\"kind\":\"node\",\"value\":3}");
        var optionalElement = JsonDocument.Parse("{\"kind\":\"optional\",\"value\":4}").RootElement.Clone();
        var objectNode = JsonNode.Parse("{\"kind\":\"object\",\"value\":5}")!.AsObject();
        var arrayNode = JsonNode.Parse("[\"array\",6]")!.AsArray();
        var valueNode = JsonValue.Create(7);
        var source = new JsonClrRecord
        {
            Id = 1,
            Document = document,
            Element = elementSource.RootElement.Clone(),
            Node = node,
            OptionalElement = optionalElement,
            ObjectNode = objectNode,
            ArrayNode = arrayNode,
            ValueNode = valueNode,
        };

        Assert.Equal(1, fsql.Insert(source).ExecuteAffrows());
        Assert.NotNull(insertCommand?.DbParms);
        Assert.Contains(insertCommand!.DbParms!, parameter =>
            parameter.DbType == System.Data.DbType.String &&
            parameter.Value is string text && text.Contains("\"kind\"", StringComparison.Ordinal));

        var loaded = fsql.Select<JsonClrRecord>()
            .Where(a => a.Id == source.Id)
            .ToOne();

        Assert.NotNull(loaded);
        Assert.Equal("document", loaded!.Document!.RootElement.GetProperty("kind").GetString());
        Assert.Equal(2, loaded.Element.GetProperty("value").GetInt32());
        Assert.Equal("node", loaded.Node!["kind"]!.GetValue<string>());
        Assert.Equal(3, loaded.Node["value"]!.GetValue<int>());
        Assert.Equal("optional", loaded.OptionalElement!.Value.GetProperty("kind").GetString());
        Assert.Equal("object", loaded.ObjectNode!["kind"]!.GetValue<string>());
        Assert.Equal(6, loaded.ArrayNode![1]!.GetValue<int>());
        Assert.Equal(7, loaded.ValueNode!.GetValue<int>());
        loaded.Document.Dispose();

        var dbFirstTable = fsql.DbFirst.GetTableByName("sonnet_json_clr_record");
        var dbFirstDocument = Assert.Single(dbFirstTable!.Columns
            .Where(column => string.Equals(column.Name, "Document", StringComparison.OrdinalIgnoreCase)));
        Assert.Equal(typeof(string), dbFirstDocument.CsType);
        Assert.Contains("JSON", dbFirstDocument.DbTypeTextFull, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UndefinedJsonElement_IsSerializedAsJsonNull()
    {
        foreach (var noneCommandParameter in new[] { false, true })
        {
            using var fsql = CreateFreeSql(autoSyncStructure: true,
                noneCommandParameter: noneCommandParameter,
                generateCommandParameterWithLambda: !noneCommandParameter);
            var source = new JsonClrRecord
            {
                Id = 1,
                Element = default
            };

            Assert.Equal(1, fsql.Insert(source).ExecuteAffrows());
            var loaded = fsql.Select<JsonClrRecord>().Where(item => item.Id == source.Id).ToOne();

            Assert.NotNull(loaded);
            Assert.Equal(JsonValueKind.Null, loaded!.Element.ValueKind);
        }
    }

    [Fact]
    public void RelationshipTable_JsonClrNullValues_RoundTripAsNull()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var source = new JsonClrRecord
        {
            Id = 1,
            Element = default,
            Document = null,
            Node = null,
            OptionalElement = null,
            ObjectNode = null,
            ArrayNode = null,
            ValueNode = null,
        };

        Assert.Equal(1, fsql.Insert(source).ExecuteAffrows());
        var loaded = fsql.Select<JsonClrRecord>().Where(a => a.Id == source.Id).ToOne();

        Assert.NotNull(loaded);
        Assert.Equal(JsonValueKind.Null, loaded!.Element.ValueKind);
        Assert.Null(loaded.Document);
        Assert.Null(loaded.Node);
        Assert.Null(loaded.OptionalElement);
        Assert.Null(loaded.ObjectNode);
        Assert.Null(loaded.ArrayNode);
        Assert.Null(loaded.ValueNode);
    }

    [Fact]
    public void InvalidJsonFromDatabase_UsesChineseConversionError()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        Assert.Equal(1, fsql.Insert(new RawJsonRow { Id = 1, Metadata = "{}" }).ExecuteAffrows());

        fsql.Ado.ExecuteNonQuery(
            "UPDATE \"sonnet_invalid_json\" SET \"Metadata\" = 'not-json' WHERE \"Id\" = 1");

        var exception = Assert.Throws<Exception>(() =>
            fsql.Select<InvalidJsonDocumentRow>().Where(a => a.Id == 1).ToOne());
        Assert.Contains("SonnetDB 返回的 JSON 文本无效", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Measurement_JsonClrType_UsesStringFieldAndRoundTrips()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var ddl = fsql.CodeFirst.GetComparisonDDLStatements<JsonMetric>();

        Assert.Contains("FIELD STRING", ddl, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("FIELD JSON", ddl, StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse("{\"source\":\"measurement\",\"ok\":true}");
        var time = new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc);
        Assert.Equal(1, fsql.Insert(new JsonMetric { Time = time, Payload = document }).ExecuteAffrows());

        var loaded = fsql.Select<JsonMetric>()
            .Where(a => a.Time == time)
            .ToOne();

        Assert.NotNull(loaded?.Payload);
        Assert.Equal("measurement", loaded!.Payload!.RootElement.GetProperty("source").GetString());
        Assert.True(loaded.Payload.RootElement.GetProperty("ok").GetBoolean());
        loaded.Payload.Dispose();
    }

    static IFreeSql CreateFreeSql(bool autoSyncStructure = false, bool noneCommandParameter = true,
        bool generateCommandParameterWithLambda = false)
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-JsonClrTests", Guid.NewGuid().ToString("N"));
        var builder = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseNoneCommandParameter(noneCommandParameter)
            .UseGenerateCommandParameterWithLambda(generateCommandParameterWithLambda);
        if (autoSyncStructure) builder.UseAutoSyncStructure(true);
        return builder.Build();
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_json_clr_record")]
    sealed class JsonClrRecord
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public JsonDocument? Document { get; set; }

        public JsonElement Element { get; set; }

        public JsonNode? Node { get; set; }

        public JsonElement? OptionalElement { get; set; }

        public JsonObject? ObjectNode { get; set; }

        public JsonArray? ArrayNode { get; set; }

        public JsonValue? ValueNode { get; set; }
    }

    [Table(Name = "sonnet_json_metric")]
    sealed class JsonMetric
    {
        [Column(Name = "time")]
        public DateTime Time { get; set; }

        [SonnetDBField]
        public JsonDocument? Payload { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_invalid_json")]
    sealed class RawJsonRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        [Column(DbType = "JSON NULL")]
        public string? Metadata { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_invalid_json")]
    sealed class InvalidJsonDocumentRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        public JsonDocument? Metadata { get; set; }
    }
}
