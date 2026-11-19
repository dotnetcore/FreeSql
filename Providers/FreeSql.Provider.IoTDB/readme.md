# FreeSql IoTDB Provider

FreeSql provider for Apache IoTDB 2.x using the Tree model and the official `Apache.IoTDB.Data` client.

```csharp
var fsql = new FreeSqlBuilder()
    .UseConnectionString(
        DataType.IoTDB,
        "DataSource=127.0.0.1;Port=6667;Username=root;Password=root")
    .UseAutoSyncStructure(false)
    .Build();
```

Map each entity to one absolute IoTDB device path. The `time` member is the timestamp column and must use `DateTime` or `long`; all other members are measurements.

```csharp
[Table(Name = "root.factory.pump01")]
public class PumpReading
{
    [Column(IsPrimary = true)]
    public DateTime time { get; set; }

    public double pressure { get; set; }
    public bool running { get; set; }
}
```

Supported measurements map to IoTDB `BOOLEAN`, `INT32`, `INT64`, `FLOAT`, `DOUBLE`, `TEXT`, `BLOB`, `DATE`, and `TIMESTAMP`. `CodeFirst.SyncStructure` creates missing measurement timeseries with idempotent DDL. Inserts must include `time`; `ExecuteAffrows` splits a batch into one IoTDB `INSERT` statement per record. Normal single-device selection, filters, ordering, `LIMIT`/`OFFSET`, and parameterized ADO commands are supported.

IoTDB Tree is not a relational database. This provider rejects joins, generic `GROUP BY`/`HAVING`, `UPDATE`, atomic upsert, identity/returning operations, partition routing, and deletes without a time predicate. Use raw IoTDB SQL for Tree-specific queries such as `GROUP BY TIME`. Transactions and multi-device entity queries are not exposed as supported features.

When constructing connection strings, use the driver's `DataSource`, `Port`, `Username`, and `Password` keywords. A `Database` option accepted by FreeSql is retained for DbFirst filtering; entities still map to full IoTDB device paths.
