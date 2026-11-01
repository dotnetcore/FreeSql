using FreeSql.DataAnnotations;
using FreeSql.SonnetDB;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBTableValuedFunctionTests
{
    [Fact]
    public void Forecast_UsesDerivedTableAndSkipsEntitySynchronization()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);

        var sql = fsql.SelectForecast<ForecastRow>("cpu", "usage", 5, "linear").ToSql();

        Assert.Contains("FROM (SELECT * FROM forecast(\"cpu\", \"usage\", 5, 'linear')) a", sql,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains("a.time", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CREATE MEASUREMENT", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Forecast_DerivedTableExecutesAgainstEmbeddedSonnetDb()
    {
        using var fsql = CreateFreeSql();
        fsql.Ado.ExecuteNonQuery("CREATE MEASUREMENT \"cpu\" (host TAG, usage FIELD FLOAT)");
        fsql.Ado.ExecuteNonQuery("INSERT INTO \"cpu\" (time, host, usage) VALUES " +
            "(1777536000000, 'edge-01', 1), (1777536060000, 'edge-01', 2), " +
            "(1777536120000, 'edge-01', 3)");

        var rows = fsql.SelectForecast<ForecastRow>("cpu", "usage", 2, "linear")
            .Where(a => a.Time > DateTime.UnixEpoch)
            .ToList();

        Assert.Equal(2, rows.Count);
        Assert.All(rows, row => Assert.True(row.Value > 0));
    }

    [Fact]
    public void Knn_UsesNativeVectorLiteralAndMetric()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.SelectKnn<KnnRow>("documents", "embedding", new[] { 1f, 0.5f, -2f }, 3, "l2")
            .Where(a => a.Distance < 2)
            .ToSql();

        Assert.Contains("FROM (SELECT * FROM knn(\"documents\", \"embedding\", [1, 0.5, -2], 3, 'l2')) a", sql,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains("a.\"distance\" < 2", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DocumentSearchPseudoColumnsAreTranslated()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.SelectVectorSearch<DocumentSearchRow>("knowledge", new[] { 1f, 0f, 0f })
            .ToSql(a => new
            {
                a.Id,
                Distance = SonnetDBFunctions.VectorDistance(),
                Score = SonnetDBFunctions.VectorScore(),
                TextScore = SonnetDBFunctions.Bm25Score(),
                Combined = SonnetDBFunctions.HybridScore()
            });

        Assert.Contains("vector_distance()", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("vector_score()", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("bm25_score()", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("hybrid_score()", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DocumentHybridSearch_RequiresTextAndUsesCanonicalArguments()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            SonnetDBTableValuedFunctions.HybridSearch("knowledge", new[] { 1f, 0f, 0f }));
        Assert.Contains("不能为空", exception.Message);

        using var fsql = CreateFreeSql();
        var sql = fsql.SelectHybridSearch<DocumentSearchRow>(
                "knowledge", new[] { 1f, 0f, 0f }, "pump alarm",
                textIndex: "ft_knowledge_body", textField: "$.body",
                textWeight: 0.6d, vectorWeight: 0.4d)
            .ToSql();

        Assert.Contains("FROM (SELECT * FROM hybrid_search(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("source => \"knowledge\"", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("text => 'pump alarm'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("text_index => \"ft_knowledge_body\"", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("text_field => '$.body'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("text_weight => 0.6", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("vector_weight => 0.4", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MeasurementHybridSearch_UsesCanonicalNamedArguments()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.SelectMeasurementHybridSearch<MeasurementHybridSearchRow>(
                measurement: "incidents",
                documents: "knowledge",
                vectorField: "embedding",
                queryVector: new[] { 1f, 0f, 0f },
                measurementJoinTag: "device_id",
                documentJoinPath: "$.device_id",
                k: 5,
                metric: "l2",
                measurementTopK: 40,
                text: "pump alarm",
                textIndex: "ft_knowledge_body",
                textField: "$.body",
                documentJoinIndex: "idx_knowledge_device",
                documentVectorField: "$.embedding",
                measurementWeight: 0.7d,
                textWeight: 0.2d,
                documentVectorWeight: 0.1d)
            .ToSql();

        Assert.Contains("FROM (SELECT * FROM hybrid_search(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("source => \"incidents\"", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("documents => \"knowledge\"", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("vector_field => \"embedding\"", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("measurement_join_tag => \"device_id\"", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("document_join_path => '$.device_id'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("measurement_top_k => 40", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("document_join_index => \"idx_knowledge_device\"", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("document_vector_field => '$.embedding'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("measurement_weight => 0.7", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("text_weight => 0.2", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("document_vector_weight => 0.1", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DocumentHybridSearch_DerivedTableExecutesAgainstEmbeddedSonnetDb()
    {
        using var fsql = CreateFreeSql();
        fsql.Ado.ExecuteNonQuery("CREATE DOCUMENT COLLECTION knowledge");
        fsql.Ado.ExecuteNonQuery("INSERT INTO knowledge (id, document) VALUES " +
            "('kb-pump', '{\"body\":\"pump alarm overheating\",\"embedding\":[1,0,0]}'), " +
            "('kb-fan', '{\"body\":\"fan maintenance\",\"embedding\":[0,1,0]}')");
        fsql.Ado.ExecuteNonQuery("CREATE FULLTEXT INDEX ft_knowledge_body ON knowledge ('$.body') USING unicode");

        var rows = fsql.SelectHybridSearch<DocumentSearchRow>(
                "knowledge", new[] { 1f, 0f, 0f }, "pump alarm", k: 1,
                textIndex: "ft_knowledge_body")
            .ToList();

        Assert.Single(rows);
        Assert.Equal("kb-pump", rows[0].Id);
    }

    [Fact]
    public void MeasurementHybridSearch_DerivedTableExecutesAgainstEmbeddedSonnetDb()
    {
        using var fsql = CreateFreeSql();
        fsql.Ado.ExecuteNonQuery(
            "CREATE MEASUREMENT incidents (device_id TAG, embedding FIELD VECTOR(3), severity FIELD FLOAT)");
        fsql.Ado.ExecuteNonQuery("INSERT INTO incidents (device_id, embedding, severity, time) VALUES " +
            "('pump-1', [1,0,0], 9, 1000), ('fan-1', [0,1,0], 3, 2000)");
        fsql.Ado.ExecuteNonQuery("CREATE DOCUMENT COLLECTION knowledge");
        fsql.Ado.ExecuteNonQuery("INSERT INTO knowledge (id, document) VALUES " +
            "('kb-pump', '{\"device_id\":\"pump-1\",\"title\":\"Pump guide\"}'), " +
            "('kb-fan', '{\"device_id\":\"fan-1\",\"title\":\"Fan guide\"}')");

        var rows = fsql.SelectMeasurementHybridSearch<MeasurementHybridSearchRow>(
                "incidents", "knowledge", "embedding", new[] { 1f, 0f, 0f },
                "device_id", "$.device_id", k: 5)
            .ToList();

        Assert.Equal(new[] { "kb-pump", "kb-fan" }, rows.Select(row => row.DocumentId).ToArray());
    }

    [Fact]
    public void JsonEach_QuotesFileArguments()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.SelectJsonEach<JsonFileRow>("D:\\data\\devices.json", "array", "$.id").ToSql();

        Assert.Contains("FROM (SELECT * FROM json_each('D:\\data\\devices.json', 'array', '$.id')) a", sql,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void JsonTable_UsesTheJsonTableAlias()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.SelectJsonTable<JsonFileRow>("D:\\data\\devices.json", "array", "$.id").ToSql();

        Assert.Contains("FROM (SELECT * FROM json_table('D:\\data\\devices.json', 'array', '$.id')) a", sql,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TableFunctionArgumentsAreValidated()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SonnetDBTableValuedFunctions.Forecast("cpu", "usage", 0, "linear"));
        var exception = Assert.Throws<ArgumentException>(() =>
            SonnetDBTableValuedFunctions.Knn("cpu", "embedding", new[] { float.NaN }, 1));
        Assert.Contains("有限", exception.Message);
        Assert.Throws<ArgumentException>(() => SonnetDBTableValuedFunctions.MeasurementHybridSearch(
            "incidents", "knowledge", "embedding", new[] { 1f, 0f, 0f }, "device_id", "$.device_id",
            textIndex: "ft_knowledge_body"));
        Assert.Throws<ArgumentException>(() => SonnetDBTableValuedFunctions.MeasurementHybridSearch(
            "incidents", "knowledge", "embedding", new[] { 1f, 0f, 0f }, "device_id", "$.device_id",
            documentVectorWeight: 0.1d));
    }

    static IFreeSql CreateFreeSql(bool autoSyncStructure = false)
    {
        var path = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-TvfTests", Guid.NewGuid().ToString("N"));
        var builder = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={path}");
        if (autoSyncStructure) builder.UseAutoSyncStructure(true);
        return builder.Build();
    }

    [Table(Name = "sonnet_forecast_result")]
    sealed class ForecastRow
    {
        [Column(Name = "time")]
        public DateTime Time { get; set; }

        [Column(Name = "value")]
        public double Value { get; set; }

        [Column(Name = "lower")]
        public double Lower { get; set; }

        [Column(Name = "upper")]
        public double Upper { get; set; }
    }

    [Table(Name = "sonnet_knn_result")]
    sealed class KnnRow
    {
        [Column(Name = "distance")]
        public double Distance { get; set; }
    }

    [Table(Name = "sonnet_document_search_result")]
    sealed class DocumentSearchRow
    {
        [Column(Name = "id")]
        public string Id { get; set; } = "";
    }

    [Table(Name = "sonnet_measurement_hybrid_search_result")]
    sealed class MeasurementHybridSearchRow
    {
        [Column(Name = "document_id")]
        public string DocumentId { get; set; } = "";
    }

    [Table(Name = "sonnet_json_file_result")]
    sealed class JsonFileRow
    {
        public string Id { get; set; } = "";
    }
}
