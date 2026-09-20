using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBJsonMapTests
{
    [Fact]
    public void JsonMap_UsesJsonColumn_RoundTripsAndTranslatesNestedPath()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-JsonMapTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseAutoSyncStructure(true)
            .Build();
        fsql.UseJsonMap();

        var table = fsql.CodeFirst.GetTableByEntity(typeof(JsonMapRow));
        Assert.NotNull(table);
        Assert.Equal("JSON", table!.ColumnsByCs[nameof(JsonMapRow.Metadata)].DbTypeText,
            ignoreCase: true);

        var source = new JsonMapRow
        {
            Id = 1,
            Metadata = new JsonMapPayload { Site = "cn", Region = "华东" }
        };
        Assert.Equal(1, fsql.Insert(source).ExecuteAffrows());

        var loaded = fsql.Select<JsonMapRow>().Where(a => a.Id == 1).ToOne();
        Assert.NotNull(loaded);
        Assert.Equal("cn", loaded!.Metadata.Site);
        Assert.Equal("华东", loaded.Metadata.Region);

        var sql = fsql.Select<JsonMapRow>()
            .Where(a => a.Metadata.Site == "cn")
            .ToSql();
        Assert.Contains("json_value", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("$.Site", sql, StringComparison.Ordinal);

        var matched = fsql.Select<JsonMapRow>()
            .Where(a => a.Metadata.Site == "cn")
            .ToOne();
        Assert.NotNull(matched);
        Assert.Equal(1, matched!.Id);
    }

    [Fact]
    public void JsonMap_NullReference_RoundTripsAsNull()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-JsonMapNullTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseAutoSyncStructure(true)
            .Build();
        fsql.UseJsonMap();

        Assert.Equal(1, fsql.Insert(new NullableJsonMapRow { Id = 1, Metadata = null }).ExecuteAffrows());
        var loaded = fsql.Select<NullableJsonMapRow>().Where(a => a.Id == 1).ToOne();

        Assert.NotNull(loaded);
        Assert.Null(loaded!.Metadata);
    }

    [Fact]
    public void JsonMap_OnMeasurement_UsesFieldString()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-JsonMapMeasurementTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .UseAutoSyncStructure(true)
            .Build();
        fsql.UseJsonMap();

        var ddl = fsql.CodeFirst.GetComparisonDDLStatements<JsonMapMeasurement>();
        Assert.Contains("\"Metadata\" FIELD STRING", ddl, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"Metadata\" TAG", ddl, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(1, fsql.Insert(new JsonMapMeasurement
        {
            Time = DateTime.UtcNow,
            Metadata = new JsonMapPayload { Site = "cn", Region = "华东" }
        }).ExecuteAffrows());
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_json_map_row")]
    sealed class JsonMapRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        [JsonMap]
        public JsonMapPayload Metadata { get; set; } = new JsonMapPayload();
    }

    sealed class JsonMapPayload
    {
        public string Site { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_json_map_null_row")]
    sealed class NullableJsonMapRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        [JsonMap]
        public JsonMapPayload? Metadata { get; set; }
    }

    [Table(Name = "sonnet_json_map_measurement")]
    sealed class JsonMapMeasurement
    {
        [Column(Name = "time")]
        public DateTime Time { get; set; }

        [JsonMap]
        public JsonMapPayload Metadata { get; set; } = new JsonMapPayload();
    }
}
