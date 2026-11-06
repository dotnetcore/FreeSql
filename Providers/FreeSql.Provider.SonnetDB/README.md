# FreeSql SonnetDB 提供程序

## 2026-11-06 #2291 follow-up

- SonnetDB 依赖继续固定为 NuGet.org 正式 `SonnetDB` `5.0.0`；其 `SonnetDB.Core` / `SonnetDB.Protocol` 传递依赖也均为 `5.0.0`，三个包的 nuspec repository commit 和实际 net10.0 DLL ProductVersion 均绑定 `84433f9edcceb84605520ebecc4b2879e3c4aa79`。
- Provider 自身保持 FreeSql 上游版本 `3.5.311`，本地打包 nuspec 明确依赖 `SonnetDB` `5.0.0`；不把 SonnetDB 版本号用于 FreeSql 包。
- 修复 `GetTablesByDatabase("")` 和空白 selector 在结构化 Views 元数据路径中丢视图的问题，并加入两项 DbFirst 回归。完整 Provider 套件为 **237/237 PASS，0 FAIL，0 SKIP**。
- NuGet.org 公开 FreeSql Core 仍为 `3.5.311` 且没有 `DataType.SonnetDB`；`FreeSql.Provider.SonnetDB` flat-container 仍返回 404。因此公开 FreeSql/SonnetDB 组合仍待上游合入 #2291、发布 Core/Provider 后再验收。

当前分支面向 **.NET 10**，依赖 **SonnetDB 5.0.0**。接入 PR #2234 已于 **2026-11-01** 合并；兼容后续 PR #2291 仍待上游合并。SonnetDB 5.0.0 固定源码为 `455bf3b53b1e56df0064d8f574b0595cbb0b609e`，正式发行状态与公开包验收见 [兼容性缺口清单](docs/sonnetdb-gap-issue.md)。

适配范围是将 FreeSql 已有接口映射到 SonnetDB 的 SQL、类型和 ADO.NET 行为。5.0.0 候选的完整 Provider 回归为 **237/237**，独立消费者在嵌入式、REST 和 `frame-http2` 连接分别通过 25 项业务检查及重开后的 8 项检查；[验收程序](../../FreeSql.Tests/FreeSql.Tests.Provider.SonnetDB.ReleaseAcceptance/README.md)记录实际程序集身份与 Server 协议收据。候选验证和公开 FreeSql 包交付分别记录。

**公开 FreeSql 包仍有发布阻塞**：本轮实查稳定 Core `3.5.311` 不含 `DataType.SonnetDB`，`FreeSql.Provider.SonnetDB` 公共索引为 404。单独编译 Provider、或把候选 Provider 配上这个公开 Core，不能作为用户可安装的发布组合；需要 FreeSql 发布包含 #2234 与 #2291 的 Core 和 Provider。

## 连接与实体模型

```csharp
using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Provider.SonnetDB.Attributes;

using var fsql = new FreeSqlBuilder()
    .UseConnectionString(DataType.SonnetDB, "Data Source=./sonnet-data")
    .UseAutoSyncStructure(true)
    .Build();
```

带有 `[SonnetDBTable]` 的实体使用关系表。未标记的实体继续使用时序测量映射；从 DbFirst 生成关系实体时，也需要保留这个标记。

```csharp
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
device.Id = fsql.Insert(device).ExecuteIdentity();
var loaded = fsql.Select<Device>().Where(a => a.Id == device.Id).ToOne();
loaded.Name = "pump-02";
fsql.Update<Device>().SetSource(loaded).ExecuteAffrows();
```

`ROWVERSION` 由 SonnetDB 生成。提供程序保留 FreeSql 的旧版本匹配与 `DbUpdateVersionException` 行为，不向版本列赋值。事务用于关系表 DML；受支持的关系 DDL 可在 workspace 中提交或回滚，三种连接均已有独立消费者证据。时序测量写入、修改、删除继续遵循独立的 measurement 合同。

时序测量通过 TAG 描述维度，通过 FIELD 保存观测值。`time` 是数据库内置时间列，按 Unix 毫秒保存。

```csharp
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

## 当前已实现的映射

| 范围 | FreeSql 接口与行为 |
| --- | --- |
| 关系表写入 | 参数化 CRUD、批量更新、`AsType` 动态实体，以及字典 CRUD |
| 原子 UPSERT | `InsertOrUpdate` / `InsertOrUpdateDict` 映射到原生 `ON CONFLICT`，支持主键或唯一索引目标、选择更新列、`UpdateSet` 和 `IfExistsDoNothing` |
| DML 结果 | 插入、更新、删除的实体返回及异步入口使用原生 `RETURNING`；`ExecuteUpdated` 额外支持既有标量/匿名投影，保留重命名列、NULL 与行版本 |
| 关系表更新边界 | 无 WHERE 批量 UPDATE、普通单列 INT 主键值更新、无显式主键关系表 DML；包含在完整嵌入式 Provider 套件中 |
| 查询组合 | CTE、`AsTreeCte` 递归查询、`UnionAll`、多行 `WithMemory`、`WithSql`、派生查询和 `InsertInto` / `InsertIntoAsync` |
| 关系表锁定读 | 显式事务中的 `ForUpdate()` / `ForUpdate(nowait: true, skipLocked: true)` 映射为 `FOR UPDATE` / `FOR UPDATE NOWAIT` / `FOR UPDATE SKIP LOCKED`；不静默降级 |
| JOIN | 关系表 `INNER` / `LEFT` / `RIGHT JOIN`、原始 `FULL JOIN`、多源 `CROSS JOIN`，以及现有 `Update.Join` |
| 时序测量 | 追加写入、查询、删除，以及测量作为主数据源与一个关系维表的 `INNER JOIN` |
| VECTOR | 显式维度 `FIELD VECTOR(N)` 的 `float[]` 参数与字面量读写；现有 `Update` 接口可单次替换一个 VECTOR FIELD |
| 结构映射 | 关系表 CodeFirst / DbFirst、主键、自增、普通及唯一索引、已支持的表列重命名与列索引迁移，以及视图列结构读取 |

字典更新和 UPSERT 按 FreeSql 约定使用 `WherePrimary("Id")` 指定冲突或匹配列。UPSERT 的唯一索引目标保留数据库自增主键的生成行为；不会以“先查询再写入”模拟原子性。

measurement 的追加、删除、VECTOR 和数值 FIELD 更新属于独立时序模型；其边界不代表关系表能力。关系表的 `VECTOR/GEOPOINT` 列仍由 SonnetDB 产品边界明确拒绝，见 [SonnetDB #349](https://github.com/IoTSharp/SonnetDB/issues/349)。`FOR SHARE` 仍未实现，见 [SonnetDB #347](https://github.com/IoTSharp/SonnetDB/issues/347)。

## 类型与表达式语义

| CLR 类型 | 关系表映射 |
| --- | --- |
| `decimal` | 原生 `DECIMAL`，支持 `Column(Precision = ..., Scale = ...)` 或显式 `DECIMAL(p,s)`，保留精度元数据和差异迁移 |
| `DateTime` / `DateTimeOffset` | 默认沿用关系 `DATETIME` 毫秒合同；显式 `DbType = "DATETIME2"` / `"DATETIMEOFFSET"` 使用原生 100ns 合同；`DATETIMEOFFSET` 返回 UTC 时刻、零 offset |
| `DateOnly` | `DATETIME`，以日期值写入并读回 `DateOnly` |
| `TimeOnly` | 原生 `TIME` |
| `TimeSpan` | `INT` ticks，支持负值、跨日时长、完整 Int64 范围和可空值；`TIME` 列应使用 `TimeOnly` |
| `byte[]` | `BLOB` |
| `System.Text.Json` DOM | `JSON`；时序测量中沿用 `FIELD STRING` |

SonnetDB 的 `INT` 为 Int64。`ulong` 只支持不超过 `long.MaxValue` 的值；`BigInteger` 尚不支持。关系表的精确 DECIMAL 映射不改变时序 FLOAT FIELD 的数值语义。

精细时间的参数按实际列类型绑定，无参数 INSERT/UPDATE 与 UPSERT 使用显式 `CAST(... AS DATETIME2/DATETIMEOFFSET)` 字面量，保留完整 ticks。默认 `DATETIME` 类型不变；不能以隐式字符串转换或截断亚毫秒值绕过类型合同。

已有字符串表达式已映射到数据库函数，包括 UTF-16 长度和索引、`Trim` 系列、`Substring`、`IndexOf`、`Replace`、填充，以及静态或动态的 `Contains` / `StartsWith` / `EndsWith`。模式值按普通文本处理，保留百分号、下划线和引号的含义；带 `StringComparison` 的重载仅翻译能够保持语义的形式。

数值转换区分显式整数截断与 `Convert.ToInt64` 的中点取偶；整数位运算和位移保留 CLR 位宽及移位计数规则。日期加减、日期差、`TimeSpan` 的 ticks、组件与 `Total*` 成员使用相应日期或整数函数。

DECIMAL 的 `Abs`、`Floor`、`Ceiling`、`Truncate`、`Math.Round(decimal)` 和 `Math.Round(decimal, int)` 使用 SonnetDB 的精确十进制合同；digits 常量限制在 0–28。带 `MidpointRounding` 的重载仍明确拒绝，不静默忽略舍入模式。

## JSON、函数与表值查询

关系表 JSON 列支持字符串和 `System.Text.Json` DOM。已有 `SonnetDBFunctions.JsonValue` 接收静态 JSON 路径，结果按数据库返回的 JSON 文本读取；对象和数组结果可由应用反序列化。JSON 路径索引沿用提供程序专属的 `SonnetDBJsonIndex` 特性，当前为单列、单路径、非唯一索引。

关系表可使用现有 `SqlExt` 的 ANSI 基础聚合窗口、`RowNumber` / `Rank` / `DenseRank` 排名窗口及 `Ntile` 分桶窗口，支持 `Over`、`PartitionBy` 和 `OrderBy` 组合。字符串聚合通过现有 `SqlExt.StringAgg` 入口映射到数据库函数。

`SonnetDBFunctions` 保留时序窗口函数、PID、Modbus 和查询结果伪列等已有入口；差分、变化率、平滑等时序专有窗口仅用于 MEASUREMENT。

`SonnetDBTableValuedFunctions` 已提供以下强类型查询入口，DTO 查询会跳过自动建表：

- `SelectForecast<T>`、`SelectKnn<T>`：预测与时序向量检索。
- `SelectJsonEach<T>`、`SelectJsonTable<T>`：JSON 文件表值查询。
- `SelectVectorSearch<T>`、`SelectHybridSearch<T>`：文档向量及全文融合检索。
- `SelectMeasurementHybridSearch<T>`：时序 KNN 与关联文档融合检索。

文档集合、全文索引和向量索引的完整 ORM 结构迁移不属于当前关系表/测量映射。物理外键也不会由 `Navigate` 自动创建；DbFirst 读取数据库中已有的外键信息。视图定义等无法由 FreeSql 共享模型承载的信息，不会放入其他字段冒充完整元数据。

## 交付状态与边界

最新 SonnetDB 源码已实现 schema/table 的 ADO 结构化元数据、字符串长度合同和 EXISTS NULL 等值访问路径修复。提供程序按实际列类型绑定 DateTimeOffset 参数；DbFirst 优先保留结构化视图身份，不支持时回退旧版补查。

完整套件覆盖已有映射；三种连接的独立旅程额外核验关键 CRUD、RETURNING、UPSERT、DECIMAL、精细时间、竞争锁、DDL 提交/回滚、STRING→INT 主键迁移、视图去重、元数据、存储过程与 measurement 分组。它们不代表任意主键类型转换、全部模型恢复或生产负载验收。正式 SonnetDB 交付与公开 FreeSql Core/Provider 的剩余条件统一维护在 [兼容性缺口清单](docs/sonnetdb-gap-issue.md)。
