using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using FreeSql.SonnetDB;
using SonnetDB.Model;

namespace FreeSql.Tests.Provider.SonnetDB;

public class SonnetDBFunctionsTests
{
    [Fact]
    public void ToSql_UsesSonnetDB31WindowAndAggregateFunctions()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<FunctionMetric>().ToSql(a => new
        {
            Delta = SonnetDBFunctions.Delta(a.Value),
            Increase = SonnetDBFunctions.Increase(a.Value),
            RunningSum = SonnetDBFunctions.RunningSum(a.Value),
            RunningMin = SonnetDBFunctions.RunningMin(a.Value),
            RunningMax = SonnetDBFunctions.RunningMax(a.Value),
            Digest = SonnetDBFunctions.TDigestAgg(a.Value),
            // ToSql 测试只核对表达式翻译；实际 VECTOR 列由 SonnetDB 架构提供。
            Centroid = SonnetDBFunctions.Centroid(a.Position),
            Bbox = SonnetDBFunctions.TrajectoryBbox(a.Position),
            SpeedMax = SonnetDBFunctions.TrajectorySpeedMax(a.Position, a.Time),
            SpeedAvg = SonnetDBFunctions.TrajectorySpeedAvg(a.Position, a.Time),
            SpeedP95 = SonnetDBFunctions.TrajectorySpeedP95(a.Position, a.Time),
            Anomaly = SonnetDBFunctions.IsAnomaly(a.Value, "zscore", 3),
            ChangepointDefault = SonnetDBFunctions.IsChangepoint(a.Value, "cusum", 3),
            ChangepointWithDrift = SonnetDBFunctions.IsChangepoint(a.Value, "cusum", 3, 0.5),
            TextMode = SonnetDBFunctions.Mode(a.State),
            FlagMode = SonnetDBFunctions.Mode(a.Active),
            TextDistinctCount = SonnetDBFunctions.DistinctCount(a.State),
            FlagDistinctCount = SonnetDBFunctions.DistinctCount(a.Active),
            TextStateChanges = SonnetDBFunctions.StateChanges(a.State),
            FlagStateDuration = SonnetDBFunctions.StateDuration(a.Active)
        });

        Assert.Contains("delta(a.\"Value\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("increase(a.\"Value\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("running_sum(a.\"Value\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("running_min(a.\"Value\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("running_max(a.\"Value\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tdigest_agg(a.\"Value\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("centroid(a.\"Position\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("trajectory_bbox(a.\"Position\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("trajectory_speed_max(a.\"Position\", a.time)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("trajectory_speed_avg(a.\"Position\", a.time)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("trajectory_speed_p95(a.\"Position\", a.time)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("anomaly(a.\"Value\", 'zscore', 3)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("changepoint(a.\"Value\", 'cusum', 3)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("changepoint(a.\"Value\", 'cusum', 3, 0.5)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mode(a.\"State\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mode(a.\"Active\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("distinct_count(a.\"State\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("distinct_count(a.\"Active\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("state_changes(a.\"State\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("state_duration(a.\"Active\")", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToSql_JsonValueUsesEscapedPathForObjectAndStringOverloads()
    {
        using var fsql = CreateFreeSql();
        var path = "$.na'me'); DROP TABLE audit; --";

        var sql = fsql.Select<FunctionMetric>().ToSql(a => new
        {
            StringJson = SonnetDBFunctions.JsonValue(a.State, path),
            ObjectJson = SonnetDBFunctions.JsonValue((object)a.State, path)
        });

        Assert.Contains("json_value(a.\"State\", '$.na''me''); DROP TABLE audit; --')", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, CountOccurrences(sql, "json_value(a.\"State\""));
    }

    [Fact]
    public void ToSql_JsonValueLiteralPath_RemainsSupportedWithParameters()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-JsonPathParameterTests", Guid.NewGuid().ToString("N"))}")
            .UseNoneCommandParameter(false)
            .UseGenerateCommandParameterWithLambda(true)
            .Build();

        var sql = fsql.Select<FunctionMetric>()
            .Where(a => SonnetDBFunctions.JsonValue(a.State, "$.site") == "cn")
            .ToSql();
        Assert.Contains("json_value", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("$.site", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void ToSql_JsonValueDynamicPath_IsRejectedWithChineseMessage()
    {
        using var fsql = CreateFreeSql();
        var invocation = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            fsql.Select<JsonPathMetric>().ToSql(a => SonnetDBFunctions.JsonValue(a.State, a.Path)));
        var ex = Assert.IsType<NotSupportedException>(invocation.InnerException);
        Assert.Contains("字符串字面量", ex.Message, StringComparison.Ordinal);
        Assert.Contains("动态路径", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ToSql_MatchQuotesIndexAndPreservesOptionalArguments()
    {
        using var fsql = CreateFreeSql();

        var fuzzySql = fsql.Select<FunctionMetric>()
            .Where(a => SonnetDBFunctions.Match("ft_docs_body", "$.body", "pump alarm", 20, "fuzzy"))
            .ToSql();
        Assert.Contains("match(\"ft_docs_body\", '$.body', 'pump alarm', 20, 'fuzzy')", fuzzySql,
            StringComparison.OrdinalIgnoreCase);

        var exactSql = fsql.Select<FunctionMetric>()
            .Where(a => SonnetDBFunctions.Match("ft_docs_body", "*", "pump", 0, "exact"))
            .ToSql();
        Assert.Contains("match(\"ft_docs_body\", '*', 'pump', 'exact')", exactSql,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IsNullOrWhiteSpace_MatchesNullAndUnicodeWhitespace()
    {
        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB,
                $"Data Source={Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-WhitespaceTests", Guid.NewGuid().ToString("N"))}")
            .UseAutoSyncStructure(true)
            .Build();

        var values = new[] { (long)1, 2, 3, 4, 5, 6 };
        var rows = new[]
        {
            new WhitespaceRow { Id = values[0], Value = null },
            new WhitespaceRow { Id = values[1], Value = string.Empty },
            new WhitespaceRow { Id = values[2], Value = " \t\r\n" },
            new WhitespaceRow { Id = values[3], Value = "\u00a0\u2003" },
            new WhitespaceRow { Id = values[4], Value = " x " },
            new WhitespaceRow { Id = values[5], Value = "x" },
        };
        Assert.Equal(rows.Length, fsql.Insert(rows).ExecuteAffrows());

        var matched = fsql.Select<WhitespaceRow>()
            .Where(a => string.IsNullOrWhiteSpace(a.Value))
            .OrderBy(a => a.Id)
            .ToList();

        Assert.Equal(new[] { 1L, 2L, 3L, 4L }, matched.Select(a => a.Id).ToArray());
        var sql = fsql.Select<WhitespaceRow>()
            .Where(a => string.IsNullOrWhiteSpace(a.Value))
            .ToSql();
        Assert.Contains("regexp_like", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("^\\s*$", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void ToSql_UsesSonnetDB31GeoTransformsAndAliases()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<FunctionMetric>().ToSql(a => new
        {
            Transform = SonnetDBFunctions.GeoTransform(a.Position, "WGS84", "GCJ02"),
            Gcj = SonnetDBFunctions.GeoWgs84ToGcj02(a.Position),
            Wgs = SonnetDBFunctions.GeoGcj02ToWgs84(a.Position),
            Bd = SonnetDBFunctions.GeoGcj02ToBd09(a.Position),
            GcjAgain = SonnetDBFunctions.GeoBd09ToGcj02(a.Position),
            WgsBd = SonnetDBFunctions.GeoWgs84ToBd09(a.Position),
            BdWgs = SonnetDBFunctions.GeoBd09ToWgs84(a.Position),
            Distance = SonnetDBFunctions.StDistance(a.Position, a.Position),
            GeoWithin = SonnetDBFunctions.IsGeoWithin(a.Position, 31.2, 121.4, 1000),
            GeoBbox = SonnetDBFunctions.IsGeoBbox(a.Position, 31.2, 121.4, 31.3, 121.5),
            Within = SonnetDBFunctions.StWithin(a.Position, 31.2, 121.4, 1000),
            DWithin = SonnetDBFunctions.StDWithin(a.Position, 31.2, 121.4, 1000)
        });

        Assert.Contains("geo_transform(a.\"Position\", 'WGS84', 'GCJ02')", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("geo_wgs84_to_gcj02(a.\"Position\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("geo_gcj02_to_wgs84(a.\"Position\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("geo_gcj02_to_bd09(a.\"Position\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("geo_bd09_to_gcj02(a.\"Position\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("geo_wgs84_to_bd09(a.\"Position\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("geo_bd09_to_wgs84(a.\"Position\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("st_distance(a.\"Position\", a.\"Position\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("geo_within(a.\"Position\", 31.2, 121.4, 1000)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("geo_bbox(a.\"Position\", 31.2, 121.4, 31.3, 121.5)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("st_within(a.\"Position\", 31.2, 121.4, 1000)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("st_dwithin(a.\"Position\", 31.2, 121.4, 1000)", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToSql_UsesGeoPointTrajectoryAggregateOverloads()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<GeoFunctionMetric>().ToSql(a => new
        {
            Lat = SonnetDBFunctions.Lat(a.Position),
            Lon = SonnetDBFunctions.Lon(a.Position),
            Length = SonnetDBFunctions.TrajectoryLength(a.Position),
            Centroid = SonnetDBFunctions.TrajectoryCentroid(a.Position),
            Bbox = SonnetDBFunctions.TrajectoryBbox(a.Position)
        });

        Assert.Contains("lat(a.\"Position\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("lon(a.\"Position\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("trajectory_length(a.\"Position\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("trajectory_centroid(a.\"Position\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("trajectory_bbox(a.\"Position\")", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LegacyGeoAndPidOverloads_KeepSourceCompatibility()
    {
        using var fsql = CreateFreeSql();

#pragma warning disable CS0618
        var sql = fsql.Select<FunctionMetric>().ToSql(a => new
        {
            Estimate = SonnetDBFunctions.PidEstimate(a.Value, "zn"),
            Distance = SonnetDBFunctions.GeoDistance(a.Position, 31.2, 121.4),
            Bearing = SonnetDBFunctions.GeoBearing(a.Position, 31.2, 121.4)
        });
#pragma warning restore CS0618

        Assert.Contains(
            "pid_estimate(a.\"Value\", 'zn', NULL, NULL, NULL, NULL)",
            sql,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "geo_distance(a.\"Position\", POINT(31.2, 121.4))",
            sql,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "geo_bearing(a.\"Position\", POINT(31.2, 121.4))",
            sql,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TimeBucket_ExplainsSonnetDB31GroupingSyntax()
    {
        using var fsql = CreateFreeSql();

        var invocation = Assert.Throws<System.Reflection.TargetInvocationException>(() => fsql.Select<FunctionMetric>()
            .ToSql(a => SonnetDBFunctions.TimeBucket("1m", a.Id)));
        var ex = Assert.IsType<NotSupportedException>(invocation.InnerException);

        Assert.Contains("不支持", ex.Message);
        Assert.Contains("GroupByRaw", ex.Message);
    }

    [Fact]
    public void NonNegativeDifference_ExplainsSonnetDB31Limitation()
    {
        using var fsql = CreateFreeSql();

        var invocation = Assert.Throws<System.Reflection.TargetInvocationException>(() => fsql.Select<FunctionMetric>()
            .ToSql(a => SonnetDBFunctions.NonNegativeDifference(a.Value)));
        var ex = Assert.IsType<NotSupportedException>(invocation.InnerException);

        Assert.Contains("不支持", ex.Message);
        Assert.Contains("Increase", ex.Message);
    }

    [Fact]
    public void WindowFunctions_RejectRelationshipTablesBeforeSqlGeneration()
    {
        using var fsql = CreateFreeSql();

        var deltaInvocation = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            fsql.Select<WindowTable>().ToSql(a => SonnetDBFunctions.Delta(a.Value)));
        var delta = Assert.IsType<NotSupportedException>(deltaInvocation.InnerException);
        Assert.Contains("delta", delta.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("关系表", delta.Message, StringComparison.Ordinal);

        var smoothingInvocation = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            fsql.Select<WindowTable>().ToSql(a => SonnetDBFunctions.MovingAverage(a.Value, 3)));
        var smoothing = Assert.IsType<NotSupportedException>(smoothingInvocation.InnerException);
        Assert.Contains("moving_average", smoothing.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("关系表", smoothing.Message, StringComparison.Ordinal);

        var stateInvocation = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            fsql.Select<WindowTable>().ToSql(a => SonnetDBFunctions.StateChanges(a.State)));
        var state = Assert.IsType<NotSupportedException>(stateInvocation.InnerException);
        Assert.Contains("state_changes", state.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("关系表", state.Message, StringComparison.Ordinal);

        var anomalyInvocation = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            fsql.Select<WindowTable>().ToSql(a => SonnetDBFunctions.IsAnomaly(a.Value, "zscore", 3)));
        var anomaly = Assert.IsType<NotSupportedException>(anomalyInvocation.InnerException);
        Assert.Contains("anomaly", anomaly.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("关系表", anomaly.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WindowFunctions_KeepMeasurementSqlGeneration()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<FunctionMetric>().ToSql(a => new
        {
            Difference = SonnetDBFunctions.Difference(a.Value),
            MovingAverage = SonnetDBFunctions.MovingAverage(a.Value, 3),
            Pid = SonnetDBFunctions.PidSeries(a.Value, 10, 1, 0.1, 0.01),
            Anomaly = SonnetDBFunctions.IsAnomaly(a.Value, "zscore", 3)
        });

        Assert.Contains("difference(a.\"Value\")", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("moving_average(a.\"Value\", 3)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("pid_series(a.\"Value\", 10, 1, 0.1, 0.01)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("anomaly(a.\"Value\", 'zscore', 3)", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UnsupportedScalarFunctions_AreRejectedBeforeSqlGeneration()
    {
        using var fsql = CreateFreeSql();

        var trim = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<FunctionMetric>().ToSql(a => a.State.Trim()));
        Assert.Contains("不支持", trim.Message);

        var exp = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<FunctionMetric>().ToSql(a => Math.Exp(a.Value)));
        Assert.Contains("不支持", exp.Message);

        var power = Assert.Throws<NotSupportedException>(() =>
            fsql.Select<FunctionMetric>().ToSql(a => Math.Pow(a.Value, 2)));
        Assert.Contains("不支持", power.Message);
    }

    [Fact]
    public void LegacyBooleanFunctionEntrypoints_RejectTypeMismatch()
    {
#pragma warning disable CS0618
        using var fsql = CreateFreeSql();

        var anomaly = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            fsql.Select<FunctionMetric>().ToSql(a => SonnetDBFunctions.Anomaly(a.Value, "zscore", 3)));
        Assert.Contains("结果类型不匹配", anomaly.InnerException?.Message);

        var changepoint = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            fsql.Select<FunctionMetric>().ToSql(a => SonnetDBFunctions.Changepoint(a.Value, "cusum", 3, 0.5)));
        Assert.Contains("结果类型不匹配", changepoint.InnerException?.Message);

        var within = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            fsql.Select<FunctionMetric>().ToSql(a => SonnetDBFunctions.GeoWithin(a.Position, 31.2, 121.4, 1000)));
        Assert.Contains("结果类型不匹配", within.InnerException?.Message);

        var bbox = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            fsql.Select<FunctionMetric>().ToSql(a => SonnetDBFunctions.GeoBbox(a.Position, 31.2, 121.4, 31.3, 121.5)));
        Assert.Contains("结果类型不匹配", bbox.InnerException?.Message);
#pragma warning restore CS0618
    }

    static IFreeSql CreateFreeSql()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), "FreeSql-SonnetDB-FunctionTests", Guid.NewGuid().ToString("N"));
        return new FreeSqlBuilder()
            .UseConnectionString(DataType.SonnetDB, $"Data Source={dataPath}")
            .Build();
    }

    static int CountOccurrences(string value, string token)
    {
        var count = 0;
        for (var index = 0; (index = value.IndexOf(token, index, StringComparison.OrdinalIgnoreCase)) >= 0; index += token.Length)
            count++;
        return count;
    }

    [Table(Name = "sonnet_function_metric")]
    sealed class FunctionMetric
    {
        public long Id { get; set; }
        public DateTime Time { get; set; }
        public double Value { get; set; }
        public string Position { get; set; } = "";
        public string State { get; set; } = "";
        public bool Active { get; set; }
    }

    [Table(Name = "sonnet_json_path_metric")]
    sealed class JsonPathMetric
    {
        public long Id { get; set; }
        public string State { get; set; } = "";
        public string Path { get; set; } = "";
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_window_table")]
    sealed class WindowTable
    {
        public long Id { get; set; }
        public double Value { get; set; }
        public string State { get; set; } = "";
    }

    [SonnetDBTable]
    [Table(Name = "sonnet_whitespace_row")]
    sealed class WhitespaceRow
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }
        public string? Value { get; set; }
    }

    [Table(Name = "sonnet_geo_function_metric")]
    sealed class GeoFunctionMetric
    {
        public long Id { get; set; }
        public DateTime Time { get; set; }
        public GeoPoint Position { get; set; }
    }
}
