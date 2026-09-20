using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using FreeSql.SonnetDB;

namespace FreeSql.Tests.Provider.SonnetDB;

public sealed class SonnetDBVectorTests
{
    [Fact]
    public void VectorLiteral_UsesNativeBracketSyntax()
    {
        using var fsql = CreateFreeSql();
        var query = new float[] { 1f, 2.5f, -3f };

        var sql = fsql.Select<VectorMetric>().ToSql(a => new
        {
            Distance = SonnetDBFunctions.CosineDistance(a.Embedding, query)
        });

        Assert.Contains("cosine_distance(a.\"Embedding\", [1, 2.5, -3])", sql,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("(1, 2.5, -3)", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void InlineVectorLiteral_UsesNativeBracketSyntax()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<VectorMetric>().ToSql(a => new
        {
            Distance = SonnetDBFunctions.CosineDistance(a.Embedding, new float[] { 1f, 2.5f, -3f })
        });

        Assert.True(sql.Contains("cosine_distance(a.\"Embedding\", [1, 2.5, -3])",
            StringComparison.OrdinalIgnoreCase), sql);
        Assert.DoesNotContain("(1, 2.5, -3)", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void VectorInsert_UsesNativeBracketSyntaxWhenParametersAreDisabled()
    {
        using var fsql = CreateFreeSql();
        var sql = fsql.Insert(new VectorMetric
        {
            Time = new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc),
            Embedding = [1f, 0.5f, -2f]
        }).ToSql();

        Assert.Contains("[1, 0.5, -2]", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("(1, 0.5, -2)", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void VectorParameter_ThrowsChineseCompatibilityError()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-VectorParameterTests", Guid.NewGuid().ToString("N"))}")
            .UseNoneCommandParameter(false)
            .UseGenerateCommandParameterWithLambda(true)
            .Build();

        var exception = Assert.Throws<NotSupportedException>(() => fsql.Insert(new VectorMetric
        {
            Time = new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc),
            Embedding = [1f, 0.5f, -2f]
        }).ToSql());

        Assert.Contains("VECTOR", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("参数绑定", exception.Message, StringComparison.Ordinal);
        Assert.Contains("UseNoneCommandParameter", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void VectorRoundTrip_UsesFloatArrayResultHandler()
    {
        using var fsql = CreateFreeSql(autoSyncStructure: true);
        var item = new VectorMetric
        {
            Time = new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc),
            Embedding = [1f, 0.5f, -2f]
        };

        Assert.Equal(1, fsql.Insert(item).ExecuteAffrows());
        var loaded = fsql.Select<VectorMetric>().Where(a => a.Time == item.Time).ToOne();

        Assert.NotNull(loaded);
        Assert.Equal(item.Embedding, loaded!.Embedding);
    }

    [Fact]
    public void VectorColumn_OnRelationshipTable_IsRejectedBeforeDdl()
    {
        using var fsql = CreateFreeSql();
        var exception = Assert.Throws<NotSupportedException>(() =>
            fsql.CodeFirst.GetComparisonDDLStatements<RelationshipVectorMetric>());

        Assert.Contains("关系表不支持 VECTOR", exception.Message, StringComparison.Ordinal);
    }

    private static IFreeSql CreateFreeSql(bool autoSyncStructure = false)
    {
        return new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-VectorTests", Guid.NewGuid().ToString("N"))}")
            .UseNoneCommandParameter(true)
            .UseAutoSyncStructure(autoSyncStructure)
            .Build();
    }

    [Table(Name = "sonnet_vector_metric")]
    private sealed class VectorMetric
    {
        [Column(Name = "time")]
        public DateTime Time { get; set; }

        [Column(DbType = "FIELD VECTOR(3)", MapType = typeof(float[]))]
        public float[] Embedding { get; set; } = [];
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_relationship_vector_metric")]
    private sealed class RelationshipVectorMetric
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        [Column(DbType = "VECTOR(3)", MapType = typeof(float[]))]
        public float[] Embedding { get; set; } = [];
    }
}
