using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBJsonIndexTests
{
    [Fact]
    public void CodeFirst_CreatesJsonPathIndex_AndDbFirstPreservesPath()
    {
        using var fsql = CreateFreeSql();

        var ddl = fsql.CodeFirst.GetComparisonDDLStatements<JsonIndexRow>();
        Assert.Contains("CREATE JSON INDEX IF NOT EXISTS \"ix_sonnet_json_site\"", ddl, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("(\"Metadata\", '$.site')", ddl, StringComparison.Ordinal);

        fsql.CodeFirst.SyncStructure<JsonIndexRow>();

        var table = fsql.DbFirst.GetTableByName("sonnet_json_index");
        var index = Assert.Single(table.Indexes);
        Assert.Equal("ix_sonnet_json_site", index.Name);
        Assert.False(index.IsUnique);
        Assert.Equal("$.site", index.JsonPath);
        Assert.Single(index.Columns);
        Assert.Equal("Metadata", index.Columns[0].Column.Name);
        Assert.Null(fsql.CodeFirst.GetComparisonDDLStatements<JsonIndexRow>());
    }

    [Fact]
    public void JsonPathIndex_RejectsUniqueIndex()
    {
        using var fsql = CreateFreeSql();
        var exception = Assert.Throws<NotSupportedException>(
            () => fsql.CodeFirst.GetComparisonDDLStatements<UniqueJsonIndexRow>());

        Assert.Contains("JSON 路径索引", exception.Message, StringComparison.Ordinal);
        Assert.Contains("唯一", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void JsonPathIndex_RejectsNonJsonColumn()
    {
        using var fsql = CreateFreeSql();
        var exception = Assert.Throws<InvalidOperationException>(
            () => fsql.CodeFirst.GetComparisonDDLStatements<InvalidJsonIndexRow>());

        Assert.Contains("JSON 路径索引列", exception.Message, StringComparison.Ordinal);
        Assert.Contains("JSON 类型", exception.Message, StringComparison.Ordinal);
    }

    static IFreeSql CreateFreeSql()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-JsonIndexTests", Guid.NewGuid().ToString("N"));
        return new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .Build();
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_json_index")]
    [Index("ix_sonnet_json_site", nameof(Metadata), JsonPath = "$['site']")]
    sealed class JsonIndexRow
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        public JsonDocument Metadata { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_json_unique_index")]
    [Index("ix_sonnet_json_unique", nameof(Metadata), true, JsonPath = "$.site")]
    sealed class UniqueJsonIndexRow
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        public JsonDocument Metadata { get; set; }
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_invalid_json_index")]
    [Index("ix_sonnet_invalid_json", nameof(Name), JsonPath = "$.site")]
    sealed class InvalidJsonIndexRow
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        public string Name { get; set; }
    }
}
