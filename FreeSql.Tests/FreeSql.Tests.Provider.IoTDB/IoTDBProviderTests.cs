using Apache.IoTDB.Data;
using FreeSql.DataAnnotations;
using FreeSql.IoTDB;
using System.Data;

namespace FreeSql.Tests.Provider.IoTDB;

public class IoTDBProviderTests
{
    [Fact]
    public void ParameterBinder_QuotesValuesAndIgnoresTokensInsideStringsAndComments()
    {
        var time = new DateTime(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);
        var parameters = new[]
        {
            new IoTDBParameter("@status", "ready' OR 1=1 --"),
            new IoTDBParameter("@time", time)
        };

        var sql = IoTDBSqlParameterBinder.Bind(
            "SELECT * FROM root.test.device WHERE status = @status AND time >= @time AND note = '@status' -- @missing",
            parameters);

        Assert.Contains("status = 'ready'' OR 1=1 --'", sql, StringComparison.Ordinal);
        Assert.Contains($"time >= {new DateTimeOffset(time).ToUnixTimeMilliseconds()}", sql, StringComparison.Ordinal);
        Assert.Contains("note = '@status' -- @missing", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void ParameterBinder_RejectsMissingAndUnusedParameters()
    {
        Assert.Throws<InvalidOperationException>(() => IoTDBSqlParameterBinder.Bind(
            "SELECT * FROM root.test.device WHERE value = @missing", Array.Empty<IoTDBParameter>()));

        Assert.Throws<InvalidOperationException>(() => IoTDBSqlParameterBinder.Bind(
            "SELECT * FROM root.test.device", new[] { new IoTDBParameter("@unused", 1) }));
    }

    [Fact]
    public void Select_UsesMappedTreePathAndLimit()
    {
        using var fsql = CreateFreeSql();

        var sql = fsql.Select<DeviceReading>()
            .Where(row => row.temperature >= 18)
            .OrderBy(row => row.time)
            .Limit(20)
            .ToSql();

        Assert.Contains("FROM root.test.device", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("temperature", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LIMIT 20", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TreeProvider_RejectsUnsupportedMutationShapesAndUnboundedDelete()
    {
        using var fsql = CreateFreeSql();

        Assert.Throws<NotSupportedException>(() => fsql.Update<DeviceReading>().Set(row => row.temperature, 20).ToSql());
        Assert.Throws<NotSupportedException>(() => fsql.InsertOrUpdate<DeviceReading>().ToSql());
        Assert.Throws<InvalidOperationException>(() => fsql.Delete<DeviceReading>().ToSql());
    }

    [Fact]
    public void Insert_RequiresSingleStatementShapeAndDisablesBatchTransactions()
    {
        using var fsql = CreateFreeSql();
        var first = new DeviceReading { time = DateTime.UnixEpoch, temperature = 18, status = "ok" };
        var second = new DeviceReading { time = DateTime.UnixEpoch.AddSeconds(1), temperature = 19, status = "ok" };

        var sql = fsql.Insert(first).ToSql();
        Assert.Contains("INSERT INTO root.test.device", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("VALUES", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Throws<NotSupportedException>(() => fsql.Insert(new[] { first, second }).ToSql());
        Assert.Throws<NotSupportedException>(() => fsql.Insert<DeviceReading>().BatchOptions(10, 100, false));
        Assert.Throws<NotSupportedException>(() => fsql.Insert<DeviceReading>().BatchOptions(1, 100, true));
    }

    [Fact]
    public void CodeFirstMapsMeasurementTypesToIoTDBTypes()
    {
        using var fsql = CreateFreeSql();

        Assert.Equal("TIMESTAMP", fsql.CodeFirst.GetDbInfo(typeof(DateTime))?.dbtype);
        Assert.Equal("INT32", fsql.CodeFirst.GetDbInfo(typeof(int))?.dbtype);
        Assert.Equal("TEXT", fsql.CodeFirst.GetDbInfo(typeof(string))?.dbtype);
    }

    [IoTDBRemoteFact]
    public void RemoteSmoke_RoundTripsParameterizedTreeRows()
    {
        var port = int.Parse(Environment.GetEnvironmentVariable("FREESQL_IOTDB_TEST_PORT")!);

        var user = Environment.GetEnvironmentVariable("FREESQL_IOTDB_TEST_USER") ?? "root";
        var password = Environment.GetEnvironmentVariable("FREESQL_IOTDB_TEST_PASSWORD") ?? "root";
        var database = "root.dbeco_freesql_" + Guid.NewGuid().ToString("N");
        var device = database + ".device";
        var connectionString = $"DataSource=127.0.0.1;Port={port};Username={user};Password={password}";
        var firstTime = DateTimeOffset.FromUnixTimeMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).UtcDateTime;
        var secondTime = firstTime.AddSeconds(1);
        var created = false;

        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.IoTDB, connectionString)
            .UseAutoSyncStructure(false)
            .Build();
        try
        {
            fsql.Ado.ExecuteNonQuery($"CREATE DATABASE {database}");
            created = true;
            fsql.CodeFirst.SyncStructure(typeof(DeviceReading), device, true);
            fsql.CodeFirst.SyncStructure(typeof(DeviceReading), device, true);
            var insert = $"INSERT INTO {device}(time,temperature,status) VALUES (@time,@temperature,@status)";
            fsql.Ado.ExecuteNonQuery(insert, new { time = firstTime, temperature = 18.75, status = "ready" });
            fsql.Ado.ExecuteNonQuery(insert, new { time = secondTime, temperature = 19.5, status = "ready" });

            var select = $"SELECT temperature, status FROM {device} WHERE temperature >= @minimum ORDER BY time ASC LIMIT 2";
            var rows = fsql.Ado.Query<DeviceReading>(select, new { minimum = 18.0 });
            Assert.Equal(2, rows.Count);
            Assert.Equal(firstTime, rows[0].time);
            Assert.Equal(18.75, rows[0].temperature, 6);
            Assert.Equal("ready", rows[0].status);
            Assert.Equal(secondTime, rows[1].time);
            Assert.Equal(19.5, rows[1].temperature, 6);
        }
        finally
        {
            if (created) fsql.Ado.ExecuteNonQuery($"DROP DATABASE {database}");
        }
    }

    private static IFreeSql CreateFreeSql() => new FreeSqlBuilder()
        .UseConnectionString(DataType.IoTDB, "DataSource=127.0.0.1;Port=6667;Username=root;Password=root")
        .UseAutoSyncStructure(false)
        .Build();

    [Table(Name = "root.test.device")]
    private sealed class DeviceReading
    {
        [Column(IsPrimary = true)]
        public DateTime time { get; set; }

        public double temperature { get; set; }

        public string status { get; set; } = string.Empty;
    }
}

public sealed class IoTDBRemoteFactAttribute : FactAttribute
{
    public IoTDBRemoteFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FREESQL_IOTDB_TEST_PORT")))
            Skip = "Set FREESQL_IOTDB_TEST_PORT to a local IoTDB RPC tunnel to opt in.";
    }
}
