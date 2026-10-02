# FreeSql SonnetDB 提供程序

本包为 FreeSql 提供 SonnetDB 3.1.0 的 ADO.NET ORM 适配。它同时支持关系表
（`TABLE`）和时序测量（`MEASUREMENT`）。当前 SonnetDB 3.1.0 仅发布
`net10.0` 目标框架，因此本提供程序也仅面向 .NET 10。

## 安装

```bash
dotnet add package FreeSql.Provider.SonnetDB --version 3.5.311
```

## 建立连接

```csharp
using FreeSql;

using var fsql = new FreeSqlBuilder()
    .UseConnectionString(
        DataType.SonnetDB,
        "Data Source=./sonnet-data")
    .UseAutoSyncStructure(true)
    .Build();
```

`Data Source` 可以是本地嵌入式数据库目录，也可以使用 SonnetDB 远程连接字符串，
例如 `sonnetdb+http://127.0.0.1:5080/metrics`。远程数据库名称以连接字符串为准；
默认连接池为每次请求创建独立连接，不能依赖临时连接上的 `USE` 或
`ChangeDatabase` 影响后续请求。

## 关系表

使用 `[SonnetDBTable]` 将实体声明为关系表；未标记的实体沿用时序测量映射。

```csharp
using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;

[SonnetDBTable]
[Table(Name = "devices")]
public sealed class Device
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    [Column(IsVersion = true)]
    public long Version { get; set; }
}

var device = new Device { Name = "pump-01" };
fsql.Insert(device).ExecuteAffrows();
var loaded = fsql.Select<Device>()
    .Where(a => a.Name == "pump-01")
    .ToOne();
```

关系表支持 CodeFirst、DbFirst、主键、自增列、`ROWVERSION`、索引、事务、
参数化 CRUD、`INNER JOIN`、`LEFT JOIN`、`JSON` 列和 `INSERT ... RETURNING`。
这里的事务边界是关系表 DML；SonnetDB 3.1 不允许 DDL，或时序测量写入/删除，
在事务中执行。

FreeSql 的 `InsertDict`、`UpdateDict`、`DeleteDict` 动态字典入口也按关系表处理；
`UpdateDict` 仍须按 FreeSql 约定调用 `WherePrimary` 指定主键。`InsertOrUpdateDict`
依赖数据库原子 UPSERT，受 SonnetDB 3.1 能力限制而提前拒绝。

`ISelect.WithMemory()` 的单行数据会生成普通派生查询，可直接用于关系表查询；
多行数据需要 FreeSql 生成 `UNION ALL`，而 SonnetDB 3.1.0 尚不支持该语法，
因此提供程序会在生成 SQL 时给出中文不支持提示。`WithMemory` 不会因为该限制
去调用公开的 `InsertOrUpdate` 执行器。

## 时序测量

时序实体使用 `[SonnetDBTag]` 和 `[SonnetDBField]` 标注维度与观测值；写入采用
追加语义，查询和删除沿用 SonnetDB 的时间序列规则。

```csharp
using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;

[Table(Name = "cpu")]
public sealed class CpuMetric
{
    [Column(Name = "time")]
    public DateTime Time { get; set; }

    [SonnetDBTag]
    public string Host { get; set; } = string.Empty;

    [SonnetDBField]
    public double Usage { get; set; }
}

fsql.Insert(new CpuMetric
{
    Time = DateTime.UtcNow,
    Host = "edge-01",
    Usage = 0.42
}).ExecuteAffrows();
```

## 窗口函数

SonnetDB 3.1 的窗口函数面向时序 `MEASUREMENT`，按每个序列的时间顺序逐行计算。
提供程序保留以下函数的原生 SQL 翻译：

| 类别 | 函数 |
| --- | --- |
| 差分与变化率 | `difference`、`delta`、`increase`、`derivative`、`non_negative_derivative`、`rate`、`irate` |
| 累计与积分 | `cumulative_sum`、`running_sum`、`running_min`、`running_max`、`integral` |
| 平滑 | `moving_average`、`ewma`、`holt_winters` |
| 缺失值处理 | `fill`、`locf`、`interpolate` |
| 状态与控制 | `state_changes`、`state_duration`、`pid_series` |
| 异常与变点 | `anomaly`、`changepoint` |

这些函数不能作用于关系 `TABLE`。提供程序会在 SQL 生成阶段抛出中文异常，不会把关系表窗口 SQL 发送给 SonnetDB；关系表应改用受支持的标量/聚合查询或在应用层计算。

```csharp
var sql = fsql.Select<CpuMetric>()
    .ToSql(a => new { a.Time, Change = SonnetDBFunctions.Delta(a.Usage) });
```

## Modbus 标量函数

SonnetDB 3.1 的 `modbus_int32`、`modbus_uint32` 和 `modbus_float32` 已通过
`SonnetDBFunctions.ModbusInt32`、`ModbusUInt32`、`ModbusFloat32` 暴露。三个函数
均接收两个 16 位寄存器和 `ABCD`、`BADC`、`CDAB` 或 `DCBA` 字节序字符串，分别
返回有符号整数、无符号整数（以 `long` 承载）和 `double`。寄存器范围、空值传播
和字节序校验由 SonnetDB 执行。

## 表值函数和向量检索

SonnetDB 3.1 的 `forecast`、时序测量 `knn`、`json_each`/`json_table`、
文档集合的 `vector_search` 和 `hybrid_search` 是 `FROM` 子句中的表值函数，
不能当作普通标量函数放在实体属性表达式中。提供程序增加了
`SonnetDBTableValuedFunctions`，会校验标识符、向量、算法和检索度量，并把函数包在
派生查询中：

```csharp
using FreeSql.DataAnnotations;
using FreeSql.SonnetDB;

public sealed class ForecastRow
{
    [Column(Name = "time")]
    public DateTime Time { get; set; }

    [Column(Name = "value")]
    public double Value { get; set; }

    [Column(Name = "lower")]
    public double Lower { get; set; }

    [Column(Name = "upper")]
    public double Upper { get; set; }
}

public sealed class KnnRow
{
    [Column(Name = "distance")]
    public double Distance { get; set; }
}

public sealed class JsonFileRow
{
    [Column(Name = "id")]
    public string Id { get; set; } = "";
}

public sealed class DocumentSearchRow
{
    [Column(Name = "id")]
    public string Id { get; set; } = "";
}

public sealed class MeasurementKnowledgeRow
{
    [Column(Name = "document_id")]
    public string DocumentId { get; set; } = "";
}

var rows = fsql.SelectForecast<ForecastRow>("cpu", "usage", 5, "linear").ToList();
var nearest = fsql.SelectKnn<KnnRow>("documents", "embedding", new[] { 1f, 0f, 0f }, 5)
    .ToList();
var imported = fsql.SelectJsonTable<JsonFileRow>("D:\\data\\devices.json", "array", "$.id")
    .ToList();
var documentRows = fsql.SelectHybridSearch<DocumentSearchRow>(
        "knowledge", new[] { 1f, 0f, 0f }, "pump alarm", textIndex: "ft_knowledge_body")
    .ToList();
var knowledgeRows = fsql.SelectMeasurementHybridSearch<MeasurementKnowledgeRow>(
        measurement: "incidents", documents: "knowledge", vectorField: "embedding",
        queryVector: new[] { 1f, 0f, 0f }, measurementJoinTag: "device_id",
        documentJoinPath: "$.device_id", text: "pump alarm", textIndex: "ft_knowledge_body")
    .ToList();
```

向量表值函数使用 SonnetDB 原生 `[v1, v2, ...]` 字面量；3.1.0 的 ADO.NET 参数协议
仍不接受 `float[]` VECTOR 参数。`SelectJsonEach` 与 `SelectJsonTable` 是同一 JSON
文件 TVF 的两个入口。文档集合的 `SelectHybridSearch` 对应“全文 + 文档向量”模式，
必须同时提供非空 `text` 和查询向量；`SelectMeasurementHybridSearch` 对应“measurement
KNN + 关联文档”模式，覆盖 `measurement_top_k`、全文、文档向量、关联 JSON 索引和三类权重，
其中全文与文档向量均可省略。当前提供程序不负责文档集合的 CodeFirst、DbFirst、全文索引或
向量索引迁移；这些模型仍需使用 SonnetDB 原生 DDL/管理 API。
`SonnetDBFunctions.VectorDistance`、`VectorScore`、`Bm25Score`、`HybridScore`、
`MeasurementDistance`、`MeasurementScore`、`DocumentVectorDistance`、
`DocumentVectorScore` 和 `TextScore` 用于投影表值函数返回的结果伪列。

## DbFirst 和 JSON

DbFirst 可以读取关系表、时序测量和视图的列结构。视图通过 `SHOW VIEWS` 枚举，
再用零行查询推断列；SonnetDB 3.1.0 尚未通过 ADO.NET 提供完整的视图定义、依赖
关系和创建时间，因此这些信息不会出现在共享 `DbTableInfo` 中。关系表生成实体
需要手工保留 `[SonnetDBTable]`；共享 DbFirst 模型不会自动携带这个 SonnetDB
专有特性。时序测量的 JSON DOM 按 `FIELD STRING` 返回 `string`，不会自动还原为
JSON DOM 类型。

关系表的 `JSON` 列支持字符串以及 `System.Text.Json` DOM。提供程序不修改
`FreeSql.Extensions.JsonMap` 等通用扩展；需要对象序列化时请在应用层使用自己的
序列化器。`SonnetDBFunctions.JsonValue` 的结果按数据库返回的 JSON 文本读取；对象或数组结果
需要由调用方再反序列化为目标 DOM 类型。

JSON 路径索引使用 provider 专属特性，不会扩展 FreeSql 的通用索引模型：

```csharp
[SonnetDBJsonIndex("ix_device_site", nameof(Metadata), "$.site")]
public JsonDocument Metadata { get; set; }
```

当前 SonnetDB 3.1 只支持单个 JSON 列、单一路径和非唯一索引；唯一索引、复合路径
和额外索引选项会在 CodeFirst 阶段明确拒绝。

## 3.1.0 的明确边界

以下能力在 SonnetDB 3.1.0 没有稳定的 SQL 或 ADO.NET 合同，提供程序会在 SQL
生成前抛出中文 `NotSupportedException`：

- 原子 `UPSERT`（`InsertOrUpdate`）；
- `UPDATE ... RETURNING`、`DELETE ... RETURNING` 和 `INSERT ... SELECT`；
- 递归 CTE、`SELECT ... FOR UPDATE`、`UPDATE ... JOIN` 和 `UNION ALL`；
- `RIGHT JOIN`、`FULL JOIN`；
- 时序测量参与的 JOIN（发布版参数绑定路径存在缺陷）。
- 关系表上的窗口函数；窗口函数只适用于时序测量。
- `BigInteger`/任意精度整数；SonnetDB 3.1 的 `INT` 只有 int64 范围，提供程序会在建模或参数绑定前给出中文提示。
- `string.Length`、带 `StringComparison` 的字符串重载、动态 `LIKE` 模式和
  `Trim` 系列；静态 `StartsWith`、`EndsWith`、`Contains` 只生成大小写敏感的
  `LIKE`，模式中的通配符会在生成阶段转义。
- 日期差结果的 `TimeSpan.Days`、`Hours`、`Minutes`、`Seconds`、`Milliseconds`、
  `Ticks` 等组件；直接对已得到的毫秒差使用 `Total*` 或已明确支持的日期加减函数
  仍可用，不能把日期差组件值当作完整 `date_diff` 语义。
- 位运算 `&`、`|`、`^`、`~`、`<<`、`>>`；SonnetDB 3.1 没有稳定的等价 SQL 函数。
- 时序 `FIELD VECTOR(N)` 仅支持显式维度和非参数化向量字面量；`float[]` 的 ADO.NET
  参数化，以及远程 REST/NDJSON 的数组结果，在 3.1 发布版不能闭环。关系表 `VECTOR`
  和 `GEOPOINT` 列也会被明确拒绝。
- JSON 路径索引仅支持单个 JSON 列、单一路径、非唯一默认 B-tree；唯一、复合或其他
  索引选项会被拒绝。`json_value` 路径必须是静态字符串，不能使用动态路径。
- `ulong` 只能使用不超过 `long.MaxValue` 的值；`decimal` 映射为 `FLOAT`，不保证任意
  精度。视图定义/依赖、远程外键元数据以及 `SupportedJoinOperators` 的完整合同仍需
  SonnetDB 上游统一。
- 文档集合的完整 ORM 模型、全文索引/向量索引 CodeFirst、DbFirst，以及
  远程 `TableDirect`/帧协议批量写入尚未纳入 FreeSql 提供程序；当前仅提供表值函数查询入口。

不能把这些操作静默改写为语义不同的 SQL。已核对的上游缺口、复现 SQL 和建议的
SonnetDB 问题单内容见仓库中的
[`docs/sonnetdb-gap-issue.md`](https://github.com/2881099/FreeSql/blob/master/Providers/FreeSql.Provider.SonnetDB/docs/sonnetdb-gap-issue.md)。
