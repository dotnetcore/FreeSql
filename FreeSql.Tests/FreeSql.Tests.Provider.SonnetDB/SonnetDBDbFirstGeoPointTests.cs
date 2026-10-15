using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using FreeSql.SonnetDB;
using SonnetDB.Model;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBDbFirstGeoPointTests
{
    [Fact]
    public void DbFirst_MeasurementGeoPoint_ExposesNativeClrType()
    {
        var path = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-DbFirstGeoPointTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={path}")
            .Build();

        fsql.CodeFirst.SyncStructure<GeoMetric>();
        var table = fsql.DbFirst.GetTableByName("sonnet_dbfirst_geo_metric");
        Assert.NotNull(table);
        var column = Assert.Single(table!.Columns.Where(a => a.Name == "Position"));
        Assert.Equal(typeof(GeoPoint?), column.CsType);
        Assert.Equal("global::SonnetDB.Model.GeoPoint?", fsql.DbFirst.GetCsType(column));
        Assert.Equal("(global::SonnetDB.Model.GeoPoint){0}", fsql.DbFirst.GetCsParse(column));
        Assert.Equal("{0}.ToString()", fsql.DbFirst.GetCsStringify(column));
        Assert.Equal("{0}.Value", fsql.DbFirst.GetCsTypeValue(column));
    }

    [Table(Name = "sonnet_dbfirst_geo_metric")]
    private sealed class GeoMetric
    {
        [Column(Name = "time")]
        public DateTime Time { get; set; }

        [Column(DbType = "FIELD GEOPOINT")]
        public GeoPoint Position { get; set; }
    }
}
