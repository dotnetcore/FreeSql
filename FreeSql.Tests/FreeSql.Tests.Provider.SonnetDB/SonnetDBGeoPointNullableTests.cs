using FreeSql.DataAnnotations;
using FreeSql.SonnetDB;
using SonnetDB.Model;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBGeoPointNullableTests
{
    [Fact]
    public void NullableGeoPoint_RoundTripsNonNullValue()
    {
        var path = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-GeoPointNullableTests", Guid.NewGuid().ToString("N"));
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={path}")
            .UseNoneCommandParameter(true)
            .UseAutoSyncStructure(true)
            .Build();
        var time = new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc);
        var point = new GeoPoint(31.2, 121.4);

        Assert.Equal(1, fsql.Insert(new NullableGeoMetric { Time = time, Position = point }).ExecuteAffrows());

        var loaded = fsql.Select<NullableGeoMetric>().Where(a => a.Time == time).ToOne();
        Assert.NotNull(loaded);
        Assert.Equal(point, loaded!.Position);
    }

    [Table(Name = "sonnet_geo_metric_nullable")]
    private sealed class NullableGeoMetric
    {
        [Column(Name = "time")]
        public DateTime Time { get; set; }

        [Column(DbType = "FIELD GEOPOINT")]
        public GeoPoint? Position { get; set; }
    }
}
