using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;

namespace FreeSql.Tests.Provider.SonnetDB;

public sealed class SonnetDBDbFirstVectorTests
{
    [Fact]
    public void DbFirst_MeasurementVector_ExposesFloatArrayType()
    {
        var path = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-DbFirstVectorTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={path}")
            .Build();

        fsql.CodeFirst.SyncStructure<VectorMetric>();
        var table = fsql.DbFirst.GetTableByName("sonnet_dbfirst_vector_metric");
        Assert.NotNull(table);

        var column = Assert.Single(table!.Columns.Where(item => item.Name == "Embedding"));
        Assert.Equal(typeof(float[]), column.CsType);
        Assert.Equal("float[]", fsql.DbFirst.GetCsType(column));
        Assert.Equal("(float[]){0}", fsql.DbFirst.GetCsParse(column));
        Assert.Equal("(float[]){0}", fsql.DbFirst.GetCsConvert(column));
        Assert.Equal("GetValue", fsql.DbFirst.GetDataReaderMethod(column));
    }

    [Table(Name = "sonnet_dbfirst_vector_metric")]
    private sealed class VectorMetric
    {
        [Column(Name = "time")]
        public DateTime Time { get; set; }

        [Column(DbType = "FIELD VECTOR(3)", MapType = typeof(float[]))]
        public float[] Embedding { get; set; } = [];
    }
}
