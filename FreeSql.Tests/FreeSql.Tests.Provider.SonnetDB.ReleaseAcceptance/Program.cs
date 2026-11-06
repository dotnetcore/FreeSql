using System.Data;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;
using SonnetDB.Data;

Console.OutputEncoding = System.Text.Encoding.UTF8;
if (args.Length != 2 || args[0] is not ("exercise" or "reopen"))
    throw new ArgumentException("Usage: <exercise|reopen> <result.json>; set SONNETDB_ACCEPTANCE_CONNECTION.");
var connectionString = Environment.GetEnvironmentVariable("SONNETDB_ACCEPTANCE_CONNECTION")
    ?? throw new ArgumentException("SONNETDB_ACCEPTANCE_CONNECTION is required.");
var report = new AcceptanceReport { Scenario = args[0] };
foreach (var assembly in new[] { typeof(SndbConnection).Assembly, typeof(SonnetDB.Engine.Tsdb).Assembly,
    typeof(SonnetDB.IO.SpanReader).Assembly, typeof(FreeSqlBuilder).Assembly,
    typeof(SonnetDBTableAttribute).Assembly })
{
    report.Assemblies[assembly.GetName().Name!] = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        + ":sha256:" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location)));
}
try
{
    using var fsql = new FreeSqlBuilder().UseConnectionString(DataType.SonnetDB, connectionString).Build();
    var expectedTime = new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc).AddTicks(1234567);
    var expectedOffset = new DateTimeOffset(2026, 11, 2, 3, 0, 0, TimeSpan.FromHours(5.5)).AddTicks(7654321);
    const decimal exact = 123456789.1234567890123456789m;
    if (args[0] == "exercise")
    {
        fsql.CodeFirst.SyncStructure<AcceptanceOrder>();
        var inserted = (await fsql.Insert(new AcceptanceOrder
        {
            Id = 1, Name = "created", Amount = exact, At = expectedTime, Offset = expectedOffset
        }).ExecuteInsertedAsync()).Single();
        Check(inserted.Id == 1 && inserted.Version == 1, "insert-returning-generated-values");
        var updated = fsql.Update<AcceptanceOrder>().Where(x => x.Id == 1).Set(x => x.Name, "updated")
            .ExecuteUpdated().Single();
        Check(updated.Name == "updated" && updated.Version == 2, "update-returning");
        Check(await fsql.InsertOrUpdate<AcceptanceOrder>().SetSource(new AcceptanceOrder
        {
            Id = 1, Name = "upserted", Amount = exact, At = expectedTime, Offset = expectedOffset
        }).UpdateColumns(x => x.Name).ExecuteAffrowsAsync() == 1, "atomic-upsert");
        Check(fsql.InsertOrUpdate<AcceptanceOrder>().SetSource(new AcceptanceOrder { Id = 1 })
            .IfExistsDoNothing().ExecuteAffrows() == 0, "upsert-do-nothing");
        await fsql.Insert(new AcceptanceOrder { Id = 2, Name = "delete", At = expectedTime, Offset = expectedOffset })
            .ExecuteAffrowsAsync();
        Check((await fsql.Delete<AcceptanceOrder>().Where(x => x.Id == 2).ExecuteDeletedAsync()).Single().Name == "delete",
            "delete-returning");
        using (var holder = fsql.Ado.MasterPool.Get())
        using (var contender = fsql.Ado.MasterPool.Get())
        using (var transaction = holder.Value.BeginTransaction())
        using (var competing = contender.Value.BeginTransaction())
        {
            Check(fsql.Select<AcceptanceOrder>().WithTransaction(transaction).Where(x => x.Id == 1)
                .ForUpdate(nowait: true).ToOne().Id == 1, "for-update-nowait");
            Check(fsql.Select<AcceptanceOrder>().WithTransaction(competing).Where(x => x.Id == 1)
                .ForUpdate(skipLocked: true).ToList().Count == 0, "skip-locked-competing-row");
            transaction.Rollback();
            Check(fsql.Select<AcceptanceOrder>().WithTransaction(competing).Where(x => x.Id == 1)
                .ForUpdate(nowait: true).ToOne().Id == 1, "row-lock-released-after-rollback");
            competing.Rollback();
        }
        using (var lease = fsql.Ado.MasterPool.Get())
        {
            using (var transaction = lease.Value.BeginTransaction())
            {
                Execute("CREATE TABLE release_ddl_rollback (Id INT, PRIMARY KEY(Id))", transaction);
                Execute("INSERT INTO release_ddl_rollback(Id) VALUES(9)", transaction);
                transaction.Rollback();
            }
            Check(!fsql.DbFirst.ExistsTable("release_ddl_rollback"), "ddl-rollback-leaves-no-table");
            using (var transaction = lease.Value.BeginTransaction())
            {
                Execute("CREATE TABLE release_ddl_commit (Id INT, PRIMARY KEY(Id))", transaction);
                Execute("INSERT INTO release_ddl_commit(Id) VALUES(9)", transaction);
                transaction.Commit();
            }
        }
        Check(fsql.Ado.Query<long>("SELECT Id FROM release_ddl_commit").Single() == 9, "ddl-commit-data-visible");
        fsql.Ado.ExecuteNonQuery("CREATE TABLE release_keys (Id STRING, PRIMARY KEY(Id))");
        fsql.Ado.ExecuteNonQuery("INSERT INTO release_keys(Id) VALUES('7')");
        fsql.Ado.ExecuteNonQuery("ALTER TABLE release_keys RENAME COLUMN Id TO KeyId");
        fsql.Ado.ExecuteNonQuery("ALTER TABLE release_keys ALTER COLUMN KeyId TYPE INT");
        Check(fsql.Ado.Query<long>("SELECT KeyId FROM release_keys").Single() == 7, "primary-key-rename-and-type-migration");
        fsql.Ado.ExecuteNonQuery("CREATE VIEW release_order_view AS SELECT \"Id\",\"Name\" FROM release_acceptance_order");
        Check(fsql.DbFirst.GetTablesByDatabase().Count(x => x.Name == "release_order_view") == 1, "dbfirst-view-identity-unique");
        Check(fsql.DbFirst.GetTablesByDatabase(string.Empty).Count(x => x.Name == "release_order_view") == 1,
            "dbfirst-view-empty-database-selector");
        Check(fsql.DbFirst.GetTablesByDatabase("   ").Count(x => x.Name == "release_order_view") == 1,
            "dbfirst-view-whitespace-database-selector");
        using (var lease = fsql.Ado.MasterPool.Get())
        using (var command = lease.Value.CreateCommand())
        using (var canceled = new CancellationTokenSource())
        {
            command.CommandText = "SELECT 1";
            canceled.Cancel();
            bool observedCancellation = false;
            try { using var reader = await command.ExecuteReaderAsync(canceled.Token); }
            catch (OperationCanceledException) { observedCancellation = true; }
            Check(observedCancellation, "pre-canceled-command");
            Check(Convert.ToInt64(await command.ExecuteScalarAsync()) == 1, "connection-usable-after-cancellation");
        }
        fsql.Ado.ExecuteNonQuery("CREATE PROCEDURE release_exchange(IN input INT,INOUT state INT,OUT output STRING) LANGUAGE SQL AS BEGIN SET @state=@state+@input; SELECT @state AS first; SELECT 'second' AS second; SET @output='done'; RETURN 7; END");
        var state = new SndbParameter("state", 10L) { Direction = ParameterDirection.InputOutput, DbType = DbType.Int64 };
        var output = new SndbParameter("output", "old") { Direction = ParameterDirection.Output, DbType = DbType.String };
        var status = new SndbParameter("status", -1L) { Direction = ParameterDirection.ReturnValue, DbType = DbType.Int64 };
        var procedure = fsql.Ado.ExecuteDataSet(CommandType.StoredProcedure, "release_exchange",
            new SndbParameter("input", 2L), state, output, status);
        Check(procedure.Tables.Count == 2 && Convert.ToInt64(procedure.Tables[0].Rows[0][0]) == 12
            && (string)procedure.Tables[1].Rows[0][0] == "second", "stored-procedure-multiple-results");
        Check(Equals(state.Value, 12L) && Equals(output.Value, "done") && Equals(status.Value, 7L), "stored-procedure-output-parameters");
        var multi = fsql.Ado.ExecuteDataSet("SELECT \"Id\" FROM release_acceptance_order WHERE \"Id\"=404; SELECT \"Id\" FROM release_acceptance_order");
        Check(multi.Tables.Count == 2 && multi.Tables[0].Rows.Count == 0 && multi.Tables[0].Columns.Count == 1
            && multi.Tables[1].Rows.Count == 1, "select-multiple-results-empty-first");
        fsql.CodeFirst.SyncStructure<AcceptanceMetric>();
        await fsql.Insert(new[]
        {
            new AcceptanceMetric { Time = expectedTime.AddTicks(-1234567), Host = "edge-a", Reading = 1 },
            new AcceptanceMetric { Time = expectedTime.AddTicks(-1234567).AddMilliseconds(1), Host = "edge-a", Reading = 2 },
            new AcceptanceMetric { Time = expectedTime.AddTicks(-1234567), Host = "edge-b", Reading = 1 }
        }).ExecuteAffrowsAsync();
        var groups = fsql.Select<AcceptanceMetric>().GroupBy(x => x.Host)
            .Having(g => g.Sum(g.Value.Reading) > 2).OrderByDescending(g => g.Sum(g.Value.Reading)).Limit(1)
            .ToList(g => new { Host = g.Key, Total = g.Sum(g.Value.Reading) });
        Check(groups.Count == 1 && groups[0].Host == "edge-a" && groups[0].Total == 3, "measurement-group-having-order-page");
        var asyncGroups = await fsql.Select<AcceptanceMetric>().GroupBy(x => x.Host)
            .Having(g => g.Sum(g.Value.Reading) > 2).OrderByDescending(g => g.Sum(g.Value.Reading)).Limit(1)
            .ToListAsync(g => new { Host = g.Key, Total = g.Sum(g.Value.Reading) });
        Check(asyncGroups.Count == 1 && asyncGroups[0].Host == "edge-a" && asyncGroups[0].Total == 3,
            "measurement-group-having-order-page-async");
    }
    var loaded = (await fsql.Select<AcceptanceOrder>().ToListAsync()).Single();
    Check(loaded.Name == "upserted" && loaded.Version == 3, "committed-crud-state");
    Check(loaded.Amount == exact, "decimal-full-precision");
    Check(loaded.Note is null, "nullable-string-roundtrip");
    Check(loaded.At.Ticks == expectedTime.Ticks, "datetime2-100ns");
    Check(loaded.Offset.UtcTicks == expectedOffset.UtcTicks && loaded.Offset.Offset == TimeSpan.Zero, "datetimeoffset-utc-ticks");
    Check(fsql.Ado.Query<long>("SELECT Id FROM release_ddl_commit").Single() == 9, "committed-ddl-reopen");
    Check(fsql.Ado.Query<long>("SELECT KeyId FROM release_keys").Single() == 7, "migrated-primary-key-reopen");
    var metadata = fsql.DbFirst.GetTableByName("release_acceptance_order");
    Check(metadata.Columns.Single(x => x.Name == "Name").MaxLength == 32, "dbfirst-declared-string-length");
    Check(metadata.Columns.Single(x => x.Name == "Amount").Precision == 28
        && metadata.Columns.Single(x => x.Name == "Amount").Scale == 19, "dbfirst-decimal-metadata");
    report.Status = "PASS";

    void Execute(string sql, System.Data.Common.DbTransaction transaction)
    {
        using var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
catch (Exception exception)
{
    report.Status = "FAIL";
    report.Failure = exception.GetType().FullName + ": " + exception.Message;
    throw;
}
finally
{
    File.WriteAllText(args[1], JsonSerializer.Serialize(report, AcceptanceJsonContext.Default.AcceptanceReport));
    Console.WriteLine($"{report.Status}: {report.Checks.Count} business checks; evidence: {args[1]}");
}
void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException("Failed: " + name);
    report.Checks.Add(name);
}

[SonnetDBTable]
[Table(Name = "release_acceptance_order")]
public sealed class AcceptanceOrder
{
    [Column(IsPrimary = true)]
    public long Id { get; set; }
    [Column(StringLength = 32)]
    public string Name { get; set; } = "";
    public string? Note { get; set; }
    [Column(DbType = "DECIMAL(28,19)")]
    public decimal Amount { get; set; }
    [Column(DbType = "DATETIME2")]
    public DateTime At { get; set; }
    [Column(DbType = "DATETIMEOFFSET")]
    public DateTimeOffset Offset { get; set; }
    [Column(IsVersion = true)]
    public long Version { get; set; }
}

[Table(Name = "release_acceptance_metric")]
public sealed class AcceptanceMetric
{
    [Column(Name = "time")]
    public DateTime Time { get; set; }
    [SonnetDBTag]
    public string Host { get; set; } = "";
    [SonnetDBField]
    public double Reading { get; set; }
}

internal sealed class AcceptanceReport
{
    public string Scenario { get; set; } = "";
    public string Status { get; set; } = "RUNNING";
    public string? Failure { get; set; }
    public List<string> Checks { get; set; } = new();
    public Dictionary<string, string> Assemblies { get; set; } = new();
}

[JsonSerializable(typeof(AcceptanceReport))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class AcceptanceJsonContext : JsonSerializerContext;
