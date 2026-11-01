# FreeSql SonnetDB 提供程序

当前分支面向 **.NET 10**，依赖公开预览 **SonnetDB 4.5.0-preview.ef266.5**。SonnetDB 稳定 4.5、对应 Server 产物和本提供程序正式包尚未发布；PR 仍为 Draft，本文描述源码接入状态，不把预览包或本地测试写成正式发行证据。

适配范围是将 FreeSql 已有接口映射到 SonnetDB 的 SQL、类型和 ADO.NET 行为。当前验收覆盖嵌入式连接；远程 REST 与帧协议尚未完成端到端验收，不能仅凭上游 Issue 关闭或本地测试通过将其标记为完成。剩余依赖及验收条件见 [兼容性缺口清单](docs/sonnetdb-gap-issue.md)。

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

`ROWVERSION` 由 SonnetDB 生成。提供程序保留 FreeSql 的旧版本匹配与 `DbUpdateVersionException` 行为，不向版本列赋值。事务用于关系表 DML；嵌入式关系 DDL 可在 SonnetDB 4.5 workspace 合同中提交或回滚，远程 DDL 事务仍需正式包组合验收。时序测量写入、修改、删除继续遵循独立的 measurement 合同。

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
| 关系表更新边界 | 无 WHERE 批量 UPDATE、普通单列 INT 主键值更新、无显式主键关系表 DML；均需使用包含对应 SonnetDB 4.5 实现的正式包复验 |
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
| `DateTime` / `DateTimeOffset` | 默认沿用关系 `DATETIME` 毫秒合同；显式 `DbType = "DATETIME2"` / `"DATETIMEOFFSET"` 使用 4.5 原生精度合同；不将关系表时间误当作时序毫秒值 |
| `DateOnly` | `DATETIME`，以日期值写入并读回 `DateOnly` |
| `TimeOnly` | 原生 `TIME` |
| `TimeSpan` | `INT` ticks，支持负值、跨日时长、完整 Int64 范围和可空值；`TIME` 列应使用 `TimeOnly` |
| `byte[]` | `BLOB` |
| `System.Text.Json` DOM | `JSON`；时序测量中沿用 `FIELD STRING` |

SonnetDB 的 `INT` 为 Int64。`ulong` 只支持不超过 `long.MaxValue` 的值；`BigInteger` 尚不支持。关系表的精确 DECIMAL 映射不改变时序 FLOAT FIELD 的数值语义。

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

## 待验收项

正式 4.5 包身份、远程连接一致性、远程 DDL 事务、schema/table 的 ADO 结构化元数据、注释清除、字符串长度元数据、降序索引、主键迁移和 StoredProcedure 消费者回归，统一维护在 [兼容性缺口清单](docs/sonnetdb-gap-issue.md)。PR 的依赖状态以实现、可复现的执行结果和发布合同共同验收，不以 Issue 的开关状态代替验证。
