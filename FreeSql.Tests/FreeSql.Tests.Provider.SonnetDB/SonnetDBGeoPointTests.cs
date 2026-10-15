using FreeSql.DataAnnotations;
using FreeSql.SonnetDB;
using FreeSql.Provider.SonnetDB.Attributes;
using SonnetDB.Model;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBGeoPointTests
{
    [Fact]
    public void GeoPointConstants_UsePointLiteralAcrossGeoFunctions()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<GeoMetric>().ToSql(a => new
        {
            Distance = SonnetDBFunctions.GeoDistance(a.Position,
                new global::SonnetDB.Model.GeoPoint(31.2, 121.4)),
            Bearing = SonnetDBFunctions.GeoBearing(a.Position,
                new global::SonnetDB.Model.GeoPoint(31.2, 121.4)),
            Speed = SonnetDBFunctions.GeoSpeed(a.Position,
                new global::SonnetDB.Model.GeoPoint(31.2, 121.4), 1000),
            Transform = SonnetDBFunctions.GeoTransform(
                new global::SonnetDB.Model.GeoPoint(31.2, 121.4), "WGS84", "GCJ02")
        });

        Assert.Contains("geo_distance(a.\"Position\", POINT(31.2, 121.4))", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("geo_bearing(a.\"Position\", POINT(31.2, 121.4))", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("geo_speed(a.\"Position\", POINT(31.2, 121.4), 1000)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("geo_transform(POINT(31.2, 121.4), 'WGS84', 'GCJ02')", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("'POINT(", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GeoPointParameter_UsesProviderParameterInParameterizedSql()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-GeoPointTests", Guid.NewGuid().ToString("N"))}")
            .UseNoneCommandParameter(false)
            .UseGenerateCommandParameterWithLambda(true)
            .Build();
        var point = new global::SonnetDB.Model.GeoPoint(31.2, 121.4);
        Aop.CurdBeforeEventArgs? command = null;
        fsql.Aop.CurdBefore += (_, e) =>
        {
            if (e.CurdType == Aop.CurdType.Select) command = e;
        };

        try
        {
            fsql.Select<GeoMetric>().Where(a => a.Position == point).ToList();
        }
        catch
        {
            // 只核对命令在执行前的 SQL 和参数，测试不依赖实际表是否存在。
        }

        Assert.NotNull(command);
        Assert.Contains("@exp_", command!.Sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("'POINT(", command.Sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(command.DbParms ?? Array.Empty<System.Data.Common.DbParameter>(),
            parameter => parameter.Value is global::SonnetDB.Model.GeoPoint);
        Assert.Contains(command.DbParms ?? Array.Empty<System.Data.Common.DbParameter>(),
            parameter => parameter.Value is global::SonnetDB.Model.GeoPoint &&
                         parameter.DbType == System.Data.DbType.Object);
    }

    [Fact]
    public void GeoPointClrValue_RoundTripsThroughMeasurement()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-GeoPointTests", Guid.NewGuid().ToString("N"))}")
            .UseNoneCommandParameter(true)
            .UseAutoSyncStructure(true)
            .Build();
        var point = new global::SonnetDB.Model.GeoPoint(31.2, 121.4);
        var time = new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc);
        Aop.CurdBeforeEventArgs? insertCommand = null;
        fsql.Aop.CurdBefore += (_, e) =>
        {
            if (e.CurdType == Aop.CurdType.Insert) insertCommand = e;
        };
        var insert = fsql.Insert(new GeoMetric { Time = time, Position = point });
        var insertSql = insert.ToSql();

        Assert.Contains("POINT(31.2, 121.4)", insertSql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("'POINT(", insertSql, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, insert.ExecuteAffrows());
        Assert.NotNull(insertCommand);
        Assert.Empty(insertCommand!.DbParms ?? Array.Empty<System.Data.Common.DbParameter>());

        var loaded = fsql.Select<GeoMetric>()
            .Where(a => a.Time == time)
            .ToOne();

        Assert.NotNull(loaded);
        Assert.Equal(point, loaded!.Position);
    }

    [Fact]
    public void NullableGeoPointClrValue_RoundTripsThroughMeasurement()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-GeoPointTests", Guid.NewGuid().ToString("N"))}")
            .UseNoneCommandParameter(true)
            .UseAutoSyncStructure(true)
            .Build();
        var point = new global::SonnetDB.Model.GeoPoint(31.2, 121.4);
        var time = new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc);

        Assert.Equal(1, fsql.Insert(new NullableGeoMetric { Time = time, Position = point }).ExecuteAffrows());

        var loaded = fsql.Select<NullableGeoMetric>()
            .Where(a => a.Time == time)
            .ToOne();

        Assert.NotNull(loaded);
        Assert.Equal(point, loaded!.Position);
    }

    static IFreeSql CreateFreeSql()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-GeoPointTests", Guid.NewGuid().ToString("N"));
        return new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .Build();
    }

    [Table(Name = "sonnet_geo_metric")]
    sealed class GeoMetric
    {
        [Column(Name = "time")]
        public DateTime Time { get; set; }

        [Column(DbType = "FIELD GEOPOINT", MapType = typeof(GeoPoint))]
        public GeoPoint Position { get; set; }
    }

    [Table(Name = "sonnet_nullable_geo_metric")]
    sealed class NullableGeoMetric
    {
        [Column(Name = "time")]
        public DateTime Time { get; set; }

        [Column(DbType = "FIELD GEOPOINT", MapType = typeof(GeoPoint?))]
        public GeoPoint? Position { get; set; }
    }
}
