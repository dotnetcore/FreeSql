# FreeSql 对 SonnetDB 3.1 关系表能力的支持缺口

## 背景

FreeSql 提供程序已按 SonnetDB 3.1.0 发布版（NuGet 包 `SonnetDB` 3.1.0）完成关系表和时序测量的能力核对。本文用于向 SonnetDB 上游提交问题单，明确需要数据库端补齐的能力，以及提供程序当前对版本边界的处理方式。

## 已提交的上游 Issue

本轮核对已将当前主线仍存在、且未被既有 Issue 覆盖的项目提交到
`IoTSharp/SonnetDB`。发布版 `v3.1.0` 已缺失但当前 `main` 已实现的能力，不重复按
“主线缺失”立项；其中 `ON CONFLICT DO UPDATE` 仍作为原子 UPSERT 的独立缺口跟踪。

- [#184：原子 UPSERT 的 `ON CONFLICT DO UPDATE`](https://github.com/IoTSharp/SonnetDB/issues/184)
- [#185：视图、外键和文档集合的 ADO.NET `GetSchema` 元数据投影](https://github.com/IoTSharp/SonnetDB/issues/185)
- [#186：VECTOR 参数与远程 `float[]` 结果编解码](https://github.com/IoTSharp/SonnetDB/issues/186)
- [#187：主键、`AUTO_INCREMENT`、`ROWVERSION` 的安全 DDL 演进](https://github.com/IoTSharp/SonnetDB/issues/187)
- [#188：`CREATE TABLE` 中的命名外键约束](https://github.com/IoTSharp/SonnetDB/issues/188)
- [#189：递归 `WITH RECURSIVE`](https://github.com/IoTSharp/SonnetDB/issues/189)
- [#190：`SELECT ... FOR UPDATE` 与行锁合同](https://github.com/IoTSharp/SonnetDB/issues/190)
- [#191：`UPDATE ... JOIN` 的关系表语义](https://github.com/IoTSharp/SonnetDB/issues/191)
- [#192：`ceil`、`floor`、`exp`、`power` 数学标量函数](https://github.com/IoTSharp/SonnetDB/issues/192)
- [#193：关系表 `VECTOR`/`GEOPOINT` 的产品边界或支持合同](https://github.com/IoTSharp/SonnetDB/issues/193)
- [#194：`UPDATE ... RETURNING` 与 `DELETE ... RETURNING`](https://github.com/IoTSharp/SonnetDB/issues/194)
- [#195：`INSERT ... SELECT` 查询插入](https://github.com/IoTSharp/SonnetDB/issues/195)
- [#196：时序测量与关系表 JOIN 的参数绑定执行](https://github.com/IoTSharp/SonnetDB/issues/196)
- [#197：`INSERT ... RETURNING` 的跨协议结果合同](https://github.com/IoTSharp/SonnetDB/issues/197)
- [#198：`BigInteger` 或任意精度整数边界](https://github.com/IoTSharp/SonnetDB/issues/198)

以下已存在的上游 Issue 已覆盖本草案中的对应部分，因此本轮不重复创建：
[#171](https://github.com/IoTSharp/SonnetDB/issues/171)（非递归 CTE）、
[#172](https://github.com/IoTSharp/SonnetDB/issues/172)（关系表窗口函数）、
[#174](https://github.com/IoTSharp/SonnetDB/issues/174)（字符串函数）、
[#175](https://github.com/IoTSharp/SonnetDB/issues/175)（日期差）、
[#177](https://github.com/IoTSharp/SonnetDB/issues/177)（标准 JOIN）、
[#178](https://github.com/IoTSharp/SonnetDB/issues/178)（`UNION ALL`）、
[#179](https://github.com/IoTSharp/SonnetDB/issues/179)（`ILIKE`）、
[#180](https://github.com/IoTSharp/SonnetDB/issues/180)（JSON）和
[#183](https://github.com/IoTSharp/SonnetDB/issues/183)（位运算）。

## 已验证的 3.1.0 能力

- 关系表 `CREATE TABLE`、`INSERT`、`UPDATE`、`DELETE`。
- `INSERT ... RETURNING id` 和 `INSERT ... RETURNING *`。
- `AUTO_INCREMENT`、`ROWVERSION`、`SHOW TABLES`、`DESCRIBE TABLE`、`GetSchema`。
- 关系视图可通过 `SHOW VIEWS` 枚举，并用 `SELECT * FROM view LIMIT 0` 获取列结构；提供程序将其标记为 `DbTableType.VIEW`。由于 3.1 的 `GetSchema("Tables")` 不包含视图，当前实现需要额外的目录查询。
- 关系表事务和 ADO.NET 参数化命令。
- 纯关系表之间的参数化 `INNER JOIN`、`LEFT JOIN`、链式多表 JOIN 和 JOIN 后聚合。
- 普通 `UNION` 查询；发布版结果默认去重。
- `forecast(...)` 和时序测量 `knn(...)` 表值函数；发布版返回预测列、距离列、TAG 和 FIELD，
  查询向量必须使用原生字面量。
- `json_each(...)`/`json_table(...)` JSON 文件表值函数，以及文档集合的
  `vector_search(...)`、`hybrid_search(...)` 查询入口。
- 未命名的外键定义可执行，例如 `FOREIGN KEY (device_id) REFERENCES devices(id)`。
- 时序测量的追加写入、查询和删除语义。
- 关系表 JSON 列的 `json_value(json_column, path)` 查询；提供程序同时提供 `SonnetDBFunctions.JsonValue`，路径经过 SQL 字面量转义，不能使用动态路径。对象/数组结果按 JSON 文本返回，由调用方负责反序列化。

以下限制已在发布版文档和实际行为中确认：时序测量写入不能放在事务中，DDL 不能在事务中执行；纯关系表的 `INNER JOIN`、`LEFT JOIN` 和 JOIN 后聚合可以执行，但不支持 `RIGHT JOIN`、`FULL JOIN`，时序测量参与 JOIN 存在参数绑定执行缺陷；普通 `UNION` 默认去重，尚不支持 `UNION ALL`；窗口函数受时序查询语义限制，且不支持与聚合混用；不支持 `UPDATE ... RETURNING` 和 `DELETE ... RETURNING`。

另外，3.1.0 仍存在以下已复现的问题：时序测量到关系表的 `INNER JOIN` 在 ADO.NET 参数绑定路径失败、`CREATE TABLE` 中带名称的外键约束被解析器拒绝，以及 `UNION ALL` 未按标准语义解析。详情见下文。

## FreeSql 提供程序当前状态

提供程序已支持：

- 通过 `[SonnetDBTable]` 区分关系表与历史时序测量模型。
- DbFirst 通过 FreeSql 共享的 `DbTableInfo` 返回表结构，外部实体生成器无法从该共享模型携带 SonnetDB 专有的 `[SonnetDBTable]`；从关系表生成实体后，必须手工添加 `using FreeSql.Provider.SonnetDB.Attributes;` 和 `[SonnetDBTable]`，否则提供程序会按时序测量模型处理。时序测量实体的 TAG/FIELD 角色也应核对并按需手工补充 `[SonnetDBTag]` 或 `[SonnetDBField]`。
- 关系表 CodeFirst、DbFirst、主键、自增列、`ROWVERSION`、索引、`DATETIME`、`BLOB` 和 `JSON` 映射，以及安全的表/列重命名、列和索引差异迁移；`JSON` 列支持字符串和 `System.Text.Json` DOM（`JsonDocument`、`JsonElement`、`JsonNode`、`JsonObject`、`JsonArray`、`JsonValue`）读写，底层按 SonnetDB 合同以 JSON 文本存储。
- 关系表 CRUD、事务、参数化命令和 `INSERT ... RETURNING`。
- FreeSql 动态字典的 `InsertDict`、`UpdateDict`、`DeleteDict` 入口按关系表处理；`UpdateDict` 按核心 API 约定需要显式 `WherePrimary`，`InsertOrUpdateDict` 仍因缺少原子 UPSERT 而拒绝。
- 关系表的参数化 `INNER JOIN`、`LEFT JOIN`、链式多表 JOIN 和 JOIN 后聚合。
- 普通 `UNION` 查询；当前 FreeSql 表规则合并模板仍生成 `UNION ALL`，因此该路径尚未宣称支持。
- 外键边界与 FreeSql 核心模型保持一致：`Navigate` 只描述 ORM 导航关系，不是物理外键声明；SonnetDB CodeFirst 不会根据导航属性推导或创建 `FOREIGN KEY`，DbFirst 只读取数据库中已经存在的外键元数据。
- 时序测量的追加写入、查询、删除和 TAG/FIELD 映射；由于已知的 ADO.NET 执行缺陷，当前拒绝时序测量参与的 JOIN 查询。
- `ISelect.ToUpdate()` 和 `ISelect.ToDelete()` 已调整为在 `IN` 子查询中投影明确的主键列，兼容 SonnetDB 3.1 对子查询单列结果的要求；复合主键使用统一别名。
- `ISelect.WithMemory()` 的单行数据可生成普通派生查询；多行数据需要 `UNION ALL`，提供程序会在 SQL 生成前给出中文提示。该内部路径直接复用 SQL 生成辅助方法，不会调用公开的 `InsertOrUpdate` 执行器。
- 对数据库端不支持的操作抛出中文 `NotSupportedException`，不生成看似可执行但语义错误的 SQL。
- `ISelect.InsertInto()`/`InsertIntoAsync()` 和递归 CTE（`AsTreeCte`）在 SQL 生成前给出中文不支持提示，不把 SonnetDB 解析器无法执行的语句发送到数据库。原始 `WithSql` 或 `AsTable` 表规则中的 `WITH`/递归 CTE 也会在生成派生表 SQL 前被拒绝；`WithTempQuery` 在 FreeSql 中表示普通派生查询，不等同于 `WITH` CTE，当前仍按生成的具体子查询形态处理。
- `ForUpdate` 和 `UpdateJoin` 在生成或创建执行器前给出中文不支持提示；关系表可使用事务和 `ROWVERSION`，或先查主键再执行单表更新。
- 时序测量的 `float[]` 属性可通过 `Column(DbType = "FIELD VECTOR(N)", MapType = typeof(float[]))` 映射为原生 VECTOR；非参数化 SQL 会生成 `[v1, v2, ...]` 字面量，并在读取端还原为 `float[]`。关系表的 VECTOR 仍在 SonnetDB 3.1 解析器层明确不支持，提供程序会在生成 DDL 前抛出中文异常。
- SonnetDB 3.1.0 的 ADO.NET 参数绑定不支持 `float[]` VECTOR 参数。提供程序不会把它静默转成字符串或普通 `(v1, v2)` 列表；参数化写入会在发送前给出中文提示，调用方可暂时启用 `UseNoneCommandParameter(true)` 使用原生向量字面量。
- 通用 `FreeSql.Extensions.JsonMap` 不属于本 provider 的兼容层；SonnetDB provider 只维护关系表 JSON/DOM 类型、`SonnetDBFunctions.JsonValue` 和 `SonnetDBJsonIndexAttribute`。对象序列化、嵌套属性映射以及 JsonMap 的行为由通用扩展自行维护，不在本 provider 的版本合同内。
- SonnetDB 的 JSON DOM 在关系表中按 `JSON` 列使用，在时序测量中按 `FIELD STRING` 保存；DbFirst 返回 `string` 是当前兼容合同，不能把时序 FIELD 自动还原为 JSON DOM。
- `SonnetDBTableValuedFunctions` 已补充 `forecast`、`knn`、`json_each`/`json_table`、
  `vector_search` 和 `hybrid_search` 的安全 SQL 生成及强类型 DTO 查询入口；`SelectJsonEach<T>` 与
  `SelectJsonTable<T>` 均可使用，`SelectHybridSearch<T>` 对应文档全文和向量融合，
  `SelectMeasurementHybridSearch<T>` 对应 measurement KNN 与关联文档融合。表值函数会自动包成派生查询，
  不会因 `UseAutoSyncStructure` 创建虚假的实体表。`VectorDistance`、`VectorScore`、`Bm25Score`、
  `HybridScore`、`MeasurementDistance`、`MeasurementScore`、`DocumentVectorDistance`、
  `DocumentVectorScore` 和 `TextScore` 等结果伪列也已通过 `SonnetDBFunctions` 暴露。
- 文档集合的 CodeFirst、DbFirst、全文/向量索引迁移和远程 `TableDirect`/帧协议批量写入仍不属于当前
  FreeSql 提供程序的关系/时序 ORM 模型；调用方可通过表值函数查询，但不能把文档集合当作 `[SonnetDBTable]` 关系表建模。

## 关系表窗口函数缺口

SonnetDB 3.1 的函数注册表将以下函数定义为窗口函数：
`difference`、`delta`、`increase`、`derivative`、`non_negative_derivative`、`rate`、
`irate`、`cumulative_sum`、`running_sum`、`running_min`、`running_max`、`integral`、
`moving_average`、`ewma`、`holt_winters`、`fill`、`locf`、`interpolate`、
`state_changes`、`state_duration`、`pid_series`、`anomaly` 和 `changepoint`。

发布版窗口执行器按时序测量的序列和时间顺序构造窗口状态；关系表执行器
只接受标量函数和聚合投影，收到上述窗口函数时会拒绝。因此，FreeSql 提供程序保留
时序测量上的原生 SQL 翻译，但在关系表的表达式翻译阶段直接抛出中文
`NotSupportedException`，避免把必然失败的 SQL 发送到数据库。该限制已由关系实体和
时序实体的 `ToSql` 回归测试覆盖。

如果 SonnetDB 计划支持关系表窗口函数，请补充稳定的 SQL 与 ADO.NET 合同：

- 窗口分区键、排序键、同值排序和跨分区状态隔离；
- NULL、缺失列、首行/窗口边界、时间戳精度和状态重置语义；
- 与 JOIN、GROUP BY、聚合投影、子查询、分页及事务的组合规则；
- 多行结果的类型、可空性、列元数据以及嵌入式、REST、帧协议的一致返回；
- 参数化函数参数、错误码和资源/窗口大小限制。

在上述合同发布并通过三种连接模式的协议测试前，提供程序将继续拒绝关系 `TABLE` 窗口
函数；调用方可改用关系表支持的标量/聚合查询或在应用层计算。

## 视图 DbFirst 边界与元数据缺口

SonnetDB 3.1 的 `SHOW VIEWS`、`DESCRIBE VIEW` 和
`information_schema.views` 只描述当前连接所选的数据库；3.1.0 的 ADO.NET
`GetSchema("Tables")` 目前只返回关系表，`GetSchema("Columns")` 也不会为逻辑视图
补充列。提供程序因此在 `LoadViews` 中先枚举 `SHOW VIEWS`（不支持时回退到
`information_schema.views`），再执行 `SELECT * FROM <view> LIMIT 0` 推断列结构，
并把结果标记为 `DbTableType.VIEW`。

`IDbFirst.GetTablesByDatabase` 不能把当前连接库的视图冒充为其他数据库。提供程序会
将数据库参数与目录连接的当前数据库比较；传入空参数表示当前库，传入不匹配的数据库时
返回空集合。`GetTableByName` 同样只查询当前连接库。嵌入式连接的数据库名是数据源路径，
远程连接的数据库名以连接字符串为准。默认 FreeSql 连接池每次创建并关闭独立连接，
因此不能依赖某个临时连接上的 `USE`/`ChangeDatabase` 影响后续 DbFirst 请求；若注入单例
连接工厂并允许切库，应由调用方保证目录查询期间不并发切换数据库。两种模式都必须保持
这一过滤语义一致。

FreeSql 的共享 `DbTableInfo` 只有表名、类型和列等结构字段，没有视图定义、直接依赖
关系或 `created_utc` 字段；当前 DbFirst 不会把 `DESCRIBE VIEW` 返回的这些信息藏在
`Comment` 或其他字段中。因而“可发现并读取列结构”已支持，但不是完整的视图元数据
闭环。若需要完整反向工程，应由 FreeSql 核心增加视图元数据承载字段，或由 SonnetDB
提供明确的 ADO.NET 视图集合及定义/依赖列，并保证嵌入式、REST、帧协议的列名、数据库
标识、大小写和空值语义一致。

## JSON 路径索引与 JsonMap 边界

SonnetDB 3.1.0 已提供关系表 JSON 路径索引能力，语法为
`CREATE JSON INDEX idx_devices_site ON devices (metadata, '$.site')`；当前合同限制为一列一个路径，并且只有
`json_value(json_col, '$.path') = literal` 形式会做等值下推。SonnetDB ADO.NET 的
`GetSchema("Indexes")` 已返回 `JSON_PATH` 列。提供程序使用专属的
`SonnetDBJsonIndexAttribute` 声明路径：
`[SonnetDBJsonIndex("ix_device_site", "Metadata", "$.site")]`。它只在
`FreeSql.Provider.SonnetDB` 内部参与 CodeFirst/DbFirst，JSON 路径不会写入 FreeSql
通用的 `IndexInfo`、`DbIndexInfo` 或 `IndexAttribute`。提供程序只生成 SonnetDB 当前支持
的单列、非唯一 JSON 路径索引；唯一索引、复合路径和额外索引选项会明确抛出中文异常。

通用 `FreeSql.Extensions.JsonMap` 不属于 SonnetDB provider 的兼容层，仍有以下边界需要保持清晰：

- provider 特性只负责 JSON 路径索引声明，不自动改变通用 JsonMap 的序列化行为；索引请使用 `SonnetDBJsonIndexAttribute` 或迁移 DDL；
- 数组下标和复杂 JSON 谓词请使用 `SonnetDBFunctions.JsonValue`；路径仍须是经过 SQL 字面量转义的静态字符串，不能使用动态路径；
- 如果 SonnetDB 后续版本或不同连接模式不再返回 `JSON_PATH`，请保持嵌入式和远程驱动的元数据列、路径规范化和空值语义一致。

另外，`System.Text.Json` 和 `GeoPoint` 的 FreeSql 类型处理器（`TypeHandler`）当前通过
`Utils.TypeHandlers` 进程级注册。SonnetDB 使用 `TryAdd`，因此不会覆盖已经由其他
提供程序或用户注册的处理器；但 FreeSql 核心尚无提供程序作用域，若 SonnetDB
先初始化，后续提供程序也可能看到这些处理器。提供程序释放时不能移除全局注册，
否则可能破坏其他提供程序或已经编译的读取表达式。若需要严格隔离，应由 FreeSql
核心增加按提供程序或 `CommonUtils` 作用域注册类型处理器的 API。

### JSON 能力矩阵

| 能力 | SonnetDB 3.1.0 合同 | FreeSql SonnetDB 提供程序 | 当前结论 |
| --- | --- | --- | --- |
| 关系表 JSON 列读写 | JSON 按 UTF-8 文本存储 | 字符串和 `System.Text.Json` DOM 均可读写 | 已支持 |
| `json_value` 路径投影/过滤 | 支持属性、数组下标和对象/数组结果 | `SonnetDBFunctions.JsonValue` 返回 JSON 文本；通用对象序列化/JsonMap 不由 provider 扩展 | 已支持；动态路径和复杂谓词需显式辅助方法，DOM 需应用层反序列化 |
| 关系表 JSON 路径索引 | `CREATE JSON INDEX ... (json_col, '$.path')`；`GetSchema("Indexes")` 返回 `JSON_PATH` | `SonnetDBJsonIndexAttribute` 声明；CodeFirst 生成并比较单列非唯一索引；DbFirst 在 provider 内保留路径 | 已支持当前合同；唯一/复合/额外选项不支持 |
| 时序测量 JSON | 不支持 `FIELD JSON`，应使用 `FIELD STRING` | JSON DOM 映射为字符串 FIELD | 已按合同处理 |
| 文档集合 | 独立文档模型和 JSON 路径索引 | FreeSql SonnetDB 提供程序暂无文档集合模型 | 不在当前关系/时序 ORM 范围 |

关系表 JSON 文本的校验语义仍需上游明确：v3.1.0 的类型转换路径会把字符串保存到 `JSON` 列，非法 JSON 通常在后续
`json_value` 解析时才暴露；文档集合则有明确的写入校验和规范化。建议 SonnetDB 在后续版本统一说明关系表 JSON
是否应在写入时拒绝非法文本、错误码及参数化行为；提供程序当前保留字符串合同，不在客户端重复改变数据库语义。

## 表值函数、文档集合和远程协议边界

SonnetDB 3.1.0 发布版已经包含以下能力，但它们不属于传统关系表/时序测量的 FreeSql
实体映射合同：

| SonnetDB 能力 | 当前 FreeSql 提供程序状态 | 结论 |
| --- | --- | --- |
| `forecast(...)` | `SelectForecast<T>` 生成安全的派生表查询 | 已补齐查询入口；预测结果 DTO 由调用方声明 |
| 时序测量 `knn(...)` | `SelectKnn<T>` 生成向量字面量和度量参数 | 已补齐查询入口；参数化 VECTOR 仍受驱动限制 |
| `json_each(...)`/`json_table(...)` | `SelectJsonEach<T>`、`SelectJsonTable<T>` 可映射三列结果 | 已补齐两个同义查询入口；文件路径由数据库进程解释 |
| 文档集合 `vector_search`/`hybrid_search` | `SelectVectorSearch<T>`、`SelectHybridSearch<T>`、`SelectMeasurementHybridSearch<T>` 及结果伪列函数 | 已补齐文档模式和 measurement KNN 关联文档模式的 SQL 查询入口，但无文档集合实体模型 |
| 文档集合/全文/向量索引 DDL | 没有 CodeFirst/DbFirst 模型 | 提供程序范围外，需原生 SonnetDB 管理 API |
| `TableDirect`/SQL 帧批量写入 | 没有 FreeSql 专用批量协议适配 | 提供程序仍使用 ADO.NET SQL 批量路径 |

表值函数参数中的时序测量、列名和路径由辅助类进行标识符/字面量转义；向量使用
原生 `[v1, v2, ...]` 字面量。SonnetDB 3.1.0 的 SQL 解析器不允许给 TVF 直接追加
普通表别名，因此提供程序会生成 `FROM (SELECT * FROM <tvf>) a`，并在自动同步结构时
跳过 TVF DTO。若上游改变 TVF 的列顺序、别名解析或向量返回编码，应补充嵌入式、REST
和帧协议的 `GetFieldType`/`GetValue` 一致性测试。

## 需要 SonnetDB 补齐的能力

### 1. 原子关系表 UPSERT

请提供稳定的 SQL 和 ADO.NET 合同，例如：

```sql
INSERT INTO devices (id, name)
VALUES (@id, @name)
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;
```

至少需要明确冲突目标、`DO NOTHING`、`DO UPDATE`、受影响行数、并发语义、参数化行为，以及与 `AUTO_INCREMENT` 和 `ROWVERSION` 的交互。当前 3.1.0 没有原子 `INSERT ... ON CONFLICT`，提供程序因此拒绝 `InsertOrUpdate`，不会退化成存在竞态的“先查后写”。

### 2. `UPDATE ... RETURNING`

请支持更新后返回完整行或指定列，并明确多行返回顺序、自增列/生成列/`ROWVERSION` 的返回值、受影响行数和事务内行为。

### 3. `DELETE ... RETURNING`

请支持删除前的完整行或指定列返回，并明确多行返回顺序、受影响行数、参数化命令和事务内行为。

### 4. `INSERT RETURNING` 的跨协议结果合同

SonnetDB 3.1.0 已能执行多行 `INSERT ... RETURNING`，不应将其表述为缺少
多行解析或执行能力。仍需补齐并固定嵌入式、远程 REST/NDJSON 与帧协议一致的
结果合同：

- 多行结果的顺序是否稳定；
- `AUTO_INCREMENT`、`ROWVERSION`、默认值和其他生成列何时可见；
- ADO.NET `ExecuteScalar`、`ExecuteReader`、`ExecuteNonQuery` 的返回约定；
- 批量命令和事务中的一致语义。

请补充跨协议集成测试和正式文档；`UPDATE ... RETURNING`、`DELETE ... RETURNING`
仍属于第 2、3 项的未实现能力，不应与本项混淆。

### 5. 命名外键约束的 DDL 合同

以下 DDL 在 3.1.0 中被解析器拒绝：

```sql
CREATE TABLE orders (
    id INT PRIMARY KEY,
    device_id INT,
    CONSTRAINT fk_orders_device FOREIGN KEY (device_id) REFERENCES devices(id)
);
```

移除 `CONSTRAINT fk_orders_device` 后，未命名的 `FOREIGN KEY` 可以创建成功。请支持标准的命名外键语法，或明确一个可稳定使用的等价 DDL 合同；同时请定义命名约束在 `GetSchema`、建表失败诊断及后续 `ALTER TABLE` 操作中的可见性。ORM 需要稳定的约束名称来比对和维护 CodeFirst 架构。

### 6. `INNER JOIN` 参数绑定执行缺陷

请修复 3.1.0 嵌入式驱动中参数绑定后的 JOIN AST 丢失问题。当前复现路径如下：

```sql
SELECT a.time, a."Host", b."Id"
FROM "sonnet_metric" a
INNER JOIN "sonnet_device" b ON a."Host" = b."Name"
```

SQL 解析和 `ToSql` 输出正确，但通过 ADO.NET 执行时返回“JOIN 执行器要求 SELECT 包含 JOIN 子句”。分析显示，参数绑定后的 `SqlParameterBinder.BindSelect` 填充了 `JoinClauses`，而 `JoinSqlExecutor` 仍读取为空的 `statement.Join`。请统一 AST 字段或让执行器读取正确字段。

纯关系表 JOIN 已可执行，本问题仅涉及“时序测量到关系表”的 JOIN 执行路径。修复后请至少验证该路径的无参数和带参数查询，并覆盖返回字段、筛选条件和连接条件中的参数。

### 7. `UNION ALL` 解析缺陷

SonnetDB 3.1.0 的 SQL 解析器只实现普通 `UNION`：解析 `UNION` 后会直接读取下一个 `SELECT`，没有消费或处理 `ALL`。执行器随后会对普通 `UNION` 结果去重。因此以下语句在 3.1.0 中不能按标准 `UNION ALL` 语义执行：

```sql
SELECT id FROM devices
UNION ALL
SELECT id FROM archived_devices;
```

当前 FreeSql 提供程序在表规则合并路径中固定生成 `UNION ALL`，会把该 SQL 交给无法解析 `ALL` 的发布版驱动。请在 SonnetDB 中补充 `UNION ALL` 的解析和执行合同，明确重复行保留、分支列数与类型、`ORDER BY`/分页以及参数绑定行为。在上游支持前，提供程序应明确拒绝此路径，不能静默改写为会去重的普通 `UNION`。

`ISelect.WithMemory()` 也会复用这段内存数据查询生成逻辑：单行数据只需要一个普通派生查询，
在 3.1.0 中可以执行；两行及以上数据必须拼接 `UNION ALL`，因此提供程序会在生成 SQL 前
抛出中文 `NotSupportedException`。该路径仅生成查询用的 `SELECT` 片段，不会为了借用代码而
创建或执行公开的 `InsertOrUpdate` 提供程序；补齐 `UNION ALL` 后才可开放多行 `WithMemory`。

### 8. 字符串比较和 LIKE 转义合同

SonnetDB 3.1.0 只有大小写敏感的 `=` 和 `LIKE`。FreeSql 的
`string.StartsWith`、`EndsWith`、`Contains` 的 `StringComparison` 重载无法
保持 `OrdinalIgnoreCase`、区域性比较等 .NET 语义；提供程序当前会在 SQL 生成
阶段明确拒绝这些重载，而不是静默忽略比较参数。请补充大小写不敏感比较的稳定
SQL 合同（例如 `ILIKE` 或明确的比较选项），并定义其 Unicode/区域性规则。

3.1.0 的 LIKE 执行器把反斜杠作为模式转义字符，但 SQL 语法没有显式 `ESCAPE`
子句或运行时模式转义函数。提供程序可以对编译期常量转义 `%`、`_`、反斜杠和
单引号；对于运行时参数无法保证通配符语义，当前会明确拒绝。请提供标准的
`ESCAPE` 语法或等价的安全转义函数，并补充参数化 LIKE 的协议测试。

FreeSql 的 `string.Length` 也需要稳定的标量函数合同。3.1.0 函数注册表没有
`length`，提供程序当前在 SQL 生成阶段拒绝该成员，避免把未知函数发送到数据库。
请补充字符串长度函数的 NULL、Unicode 字符/字节计数和参数绑定语义。

`string.Trim`、`TrimStart`、`TrimEnd` 也没有可稳定映射的 `trim`、`ltrim`、
`rtrim` 函数。提供程序当前明确拒绝这些调用，避免错误地改变 Unicode 空白字符
语义。请补充函数、NULL 传播、Unicode 空白集合和参数绑定合同。

### 9. 日期差和 `TimeSpan` 组件语义

SonnetDB 3.1 已提供 `to_unix_milliseconds`，因此 FreeSql 可以安全翻译
`DateTime.Subtract(...).TotalDays`、`TotalHours`、`TotalMinutes`、`TotalSeconds`
和 `TotalMilliseconds`，也可以用 `date_part` 组合出 `TimeOfDay`。但是发布版函数
注册表没有 `floor` 或 `date_diff`，无法在负数和跨日场景保持 .NET
`TimeSpan.Days`、`Hours`、`Minutes`、`Seconds`、`Milliseconds`、`Ticks` 的组件语义。
当前提供程序会在 SQL 生成阶段明确抛出中文异常，而不是生成近似结果。

请提供一个有明确负数舍入规则的 `date_diff`、`floor` 或等价能力，并说明时区、
精度、负差值、溢出及 ADO.NET 参数绑定合同。补齐后，FreeSql 可以恢复这些组件
属性的端到端翻译和执行测试。

同一标量函数缺口还影响 `Math.Exp`、`Math.Ceiling`、`Math.Floor` 和 `Math.Pow`。
3.1.0 未注册 `exp`、`ceil`、`floor`、`power`，提供程序不会用近似表达式替代。
请补齐函数名、参数/返回类型、边界值、溢出、`NaN`/无穷值和 NULL 传播合同，或在
SQL 参考中明确这些函数不在产品范围内。

### 10. 关系表 JSON 写入和路径参数合同

3.1.0 的关系表 `JSON` 列按文本保存，非法 JSON 通常在后续 `json_value` 解析时
才暴露；文档集合则有独立的写入校验和规范化。请统一关系表 JSON 的写入校验时机、
错误码、`NULL`、缺失值和非法文本行为，以及参数化写入与非参数化字面量的差异。

当前 `json_value(json_column, path)` 要求路径是字符串字面量，且需要明确属性、
数组下标、对象/数组结果、`NULL` 和非法文本的返回或异常语义。请补充参数化路径
（或稳定的路径转义函数）及对应协议测试，避免客户端只能拒绝动态路径。

### 11. JSON 路径索引能力和元数据一致性

当前执行器只接受单个 JSON 列、单一路径、非唯一索引。请明确后续版本对唯一、
复合、部分和稀疏 JSON 索引的支持范围，并同步 `SHOW INDEXES`
和 `GetSchema("Indexes")` 的 `JSON_PATH`、唯一性、列顺序及远程驱动返回合同。
FreeSql 当前只生成并比较单列非唯一 JSON 路径索引，其他声明会明确抛出中文异常。

### 12. 外键元数据在嵌入式与远程驱动间保持一致

服务端 schema 响应已经包含关系表外键；问题在于 3.1.0 的远程 ADO.NET DTO 没有
接收 `ForeignKeys`，并在远程表结构转换时固定传入空集合，导致
`GetSchema`/DbFirst 在嵌入式和远程模式下结果不同。请让远程驱动完整投影已有的
协议字段，而不是重新设计服务端目录：外键名称、本表列、引用表/列、删除动作均应
保留，并覆盖单表查询和全库枚举。

### 13. `INSERT ... SELECT` 与 `ISelect.InsertInto` 合同

FreeSql 的 `ISelect.InsertInto()` 会生成如下查询插入语句：

```sql
INSERT INTO archive_devices (id, name)
SELECT id, name FROM devices;
```

SonnetDB 3.1.0 的 `ParseInsert` 在列列表后只接受 `VALUES`，不会把 `SELECT` 解析为查询插入。请补充 `INSERT ... SELECT` 的语法、列数和类型校验、参数绑定、事务及受影响行数合同，并覆盖多表查询和异步 ADO.NET 执行。当前 FreeSql 提供程序会在生成 SQL 前以中文异常拒绝 `ISelect.InsertInto`，请改用分批 `INSERT ... VALUES`。

### 14. 递归 CTE 与普通 `WITH` 查询

FreeSql 的 `AsTreeCte()` 会生成 `WITH`/`WITH RECURSIVE` 查询，并在递归分支中使用 `UNION ALL`。`WithTempQuery()` 则是派生查询（通常表现为 `FROM (SELECT ...)`），不应与 CTE 混为一谈。SonnetDB 3.1.0 发布版解析器和执行器没有完整的 CTE 语法与递归执行合同。请补充：CTE 列别名、递归终止条件、`UNION ALL` 语义、参数绑定、排序分页、嵌套 CTE 以及 ADO.NET 结果映射；另请明确派生查询的嵌套、参数绑定和结果映射合同。

在上游支持前，FreeSql 提供程序会在 `AsTreeCte()` 入口，以及 `WithSql`/`AsTable` 提供的原始表规则检测到 `WITH`（包括包在派生表括号中的形式）时明确拒绝，并提示改写为普通查询或分步处理，避免把不完整的 CTE SQL 发送到 SonnetDB。`WithTempQuery()` 生成的是普通派生查询，不在该统一 CTE 拒绝范围内；如其具体派生查询形态超出 SonnetDB 3.1.0 能力，应由调用方根据生成 SQL 选择改写方式。

### 15. 锁定读与更新联接

FreeSql 的 `ISelect.ForUpdate()` 和 `IUpdate.Join()` 分别用于事务内锁定读和带联接条件的更新。SonnetDB 3.1.0 的关系表执行器没有与这两个入口对应的稳定 SQL/事务合同，提供程序当前以中文异常拒绝，建议先使用普通事务配合 `ROWVERSION` 乐观并发控制，或先查询主键再执行单表更新。

请补充 `SELECT ... FOR UPDATE` 的锁粒度、等待/超时、跳过锁定、事务隔离级别和 ADO.NET 行为；同时补充 `UPDATE ... JOIN` 的连接条件、受影响行数、重复匹配、事务回滚和参数绑定合同。完成后 FreeSql 可增加对应的端到端测试并移除限制。

### 16. 视图元数据统一合同

请为视图提供与关系表一致、稳定的元数据接口，并统一嵌入式与远程连接的行为：

- `GetSchema("Tables")` 是否包含视图，或提供稳定的 `Views` 集合；
- 视图名称、所属数据库/模式、定义文本、列名、顺序、类型、可空性和注释；
- `SHOW VIEWS`、`DESCRIBE VIEW`（或等价命令）以及 ADO.NET `GetSchema` 的列名和返回类型；
- 视图筛选参数、跨数据库枚举、大小写规则和不存在视图时的错误合同；
- 视图依赖关系、只读属性以及视图变更后的元数据缓存失效规则。

SonnetDB 3.1 的提供程序当前通过 `SHOW VIEWS`（失败时回退到
`information_schema.views`）枚举名称，再用 `LIMIT 0` 推断列结构，并在 FreeSql 中标记为
`DbTableType.VIEW`。在上游提供统一合同前，定义文本和完整视图依赖信息无法可靠地从远程
驱动取得，且 `GetSchema("Tables")` 与 `SHOW VIEWS` 的结果集合并不一致。

### 17. JOIN 能力元数据与实际执行能力不一致

SonnetDB 3.1.0 的 SQL 解析器和关系表执行器已经接受 `INNER JOIN`、`LEFT JOIN` 以及
`LEFT OUTER JOIN`，FreeSql 提供程序也已通过参数化关系表回归测试验证 `LEFT JOIN`。
但 `SndbConnection.GetSchema("DataSourceInformation")` 返回的
`SupportedJoinOperators` 当前固定为 `Inner`，没有声明 `LeftOuter`。依赖 ADO.NET
元数据决定可用 JOIN 类型的 ORM、设计器或迁移工具会因此错误拒绝实际可执行的
`LEFT JOIN`；这与嵌入式和远程执行能力都不一致。

请将该字段改为至少包含 `Inner | LeftOuter`，并明确 `RightOuter`、`FullOuter` 仍不支持时
的位掩码、SQL 语法（含 `LEFT OUTER`）、参数绑定和执行结果语义；嵌入式、REST、帧协议的
元数据也应保持一致。请补充 `GetSchema` 元数据测试以及 `INNER`、`LEFT`、多表链式 JOIN
的 ADO.NET 参数化测试，避免元数据和执行器再次漂移。

当前 FreeSql 提供程序不依据该不完整的元数据字段拒绝 `LEFT JOIN`，而是按已验证的
关系表执行能力生成 SQL；上游修正元数据后无需修改调用方代码。

### 18. 位运算语法或等价函数

FreeSql 可生成 `&`、`|`、`^`、`~`、`<<` 和 `>>` 位运算。SonnetDB 3.1.0
词法分析器没有这些运算符，也没有可由提供程序稳定映射的等价标量函数；提供程序
当前在 SQL 生成阶段抛出中文异常，不发送不可解析的 SQL。

请补充位与、位或、异或、按位取反和左右移位的 SQL 语法或函数，并明确整数类型、
NULL 传播、负数右移、移位量越界、溢出以及 ADO.NET 参数绑定语义。补齐后应覆盖
关系表和时序测量的查询、更新条件及常量/参数两种路径。

### 19. 关系表特殊列的 CodeFirst 演进

SonnetDB 3.1.0 的 `ALTER TABLE` 不能为既有关系表新增或修改主键、
`AUTO_INCREMENT`、`ROWVERSION`。FreeSql 在 CodeFirst 对这些变化提前拒绝，
避免生成会被解析器拒绝或破坏既有数据的 DDL；这使已有表无法通过常规迁移路径
演进为带身份列或乐观并发列的模型。

请明确支持的迁移策略：可选的原地 `ALTER TABLE` 语法，或官方的重建表/数据回填/
约束重建原子迁移协议。无论采用何种方案，均应定义主键冲突、自增序列起点、
ROWVERSION 初值和更新规则、外键/索引依赖、事务回滚、远程 ADO.NET 受影响行数及
`GetSchema` 刷新语义。

### 20. VECTOR 的 ADO.NET 参数与远程结果合同

SonnetDB 3.1 的时序测量支持 `FIELD VECTOR(N)`，原生 SQL 向量字面量格式为
`[1, 2, 3]`。关系表 `VECTOR` 会被解析器明确拒绝，因此不能把同一列定义直接
迁移到 `TABLE`。FreeSql 提供程序已经补齐时序测量的 `float[]` 模型识别、字面量
生成和本地读取映射，但仍受到发布版驱动协议边界限制。

以下参数化路径在 3.1.0 发布版无法闭环：

```csharp
command.CommandText =
    "INSERT INTO docs (embedding, time) VALUES (@embedding, @time)";
command.Parameters.AddWithValue("@embedding", new float[] { 1, 2, 3 });
```

驱动的参数绑定器只接受基础标量和 `GeoPoint`，对 `float[]` 会报告不支持的参数类型；
把向量改成字符串 `"[1,2,3]"` 也不能绕过问题，因为解析器要求原生 `[ ... ]` 向量
字面量，带引号的字符串不是 VECTOR 值。请为 ADO.NET 增加明确的 VECTOR 参数类型
（建议保留维度并按 `float32` 编码），并统一 `DbType`、`GetFieldType`、空值、维度
不匹配和参数化 INSERT/UPDATE/查询函数的错误合同。

远程 REST/NDJSON 路径还需要单独修复结果编码：服务端行写入器当前没有覆盖
`float[]`，会退化为 `System.Single[]` 字符串；客户端读取器对 JSON 数组也只保留
原始文本。请让服务端把 VECTOR 输出为数值数组，并让远程 ADO.NET `GetValue` /
`GetFieldType` 返回 `float[]`，同时覆盖 `centroid(vector)`、普通 VECTOR 投影和
空值行。二进制 SQL 帧已经有 VECTOR 编解码，可作为 REST 合同的参考实现。

在上游发布修复前，FreeSql 对参数化 VECTOR 明确抛出中文 `NotSupportedException`，
不会把向量误写成字符串；使用 `UseNoneCommandParameter(true)` 时才生成安全的
`[v1, v2, ...]` 字面量。请在 SonnetDB 修复后补充嵌入式、REST 和帧协议三种模式的
端到端测试，并确认维度校验、NULL 和聚合返回类型一致。

### 21. BigInteger 与任意精度整数

FreeSql 可以把 `System.Numerics.BigInteger` 作为实体属性或参数传递给部分数据库
提供程序。SonnetDB 3.1 只有 int64 范围的 `INT`，ADO.NET 参数绑定器也没有
`BigInteger` 的字面量或参数协议。把值直接传给驱动会在执行阶段才得到“参数类型不支持”，
而把它转成 `long` 又可能静默溢出，不能作为兼容实现。

请在 SonnetDB 中明确以下两种方案之一：

- 增加任意精度数值类型及其 SQL、参数、结果和元数据合同；或
- 明确 `BigInteger` 只能在应用层转成 int64/字符串，并给出范围检查与错误码。

在上游补齐前，FreeSql SonnetDB 提供程序会在 CodeFirst 建模、参数化写入和非参数化
字面量生成前抛出中文 `NotSupportedException`。调用方应改用 `long`/`ulong`（并接受
SonnetDB INT 的范围限制）或自行把大整数存为字符串。

### 22. 表值函数的稳定 SQL/ADO.NET 合同

SonnetDB 3.1.0 已实现 `forecast(...)`、时序测量 `knn(...)`、
`json_each(...)`/`json_table(...)`、文档集合 `vector_search(...)` 和
`hybrid_search(...)`。请为这些入口补充并长期保持以下合同：

- TVF 输出列名、顺序、数据类型、可空性和别名规则，尤其是 `forecast` 的
  `time/value/lower/upper`、`knn` 的 `distance`、以及文档检索的伪列；
- 结果行的 `GetFieldType`、`GetValue`、`GetSchemaTable` 和空值行为；
- 向量维度、非法分量、度量名称、`k`/分页、TAG/时间过滤和参数绑定错误码；
- 嵌入式、REST/NDJSON 与帧协议在 `float[]`、GEOPOINT、DATETIME 和 JSON 文本上的一致编码；
- TVF 与派生表、JOIN、排序、分页、事务和异步 ADO.NET 读取的组合规则。

FreeSql 提供程序已提供 `SonnetDBTableValuedFunctions` 查询入口，并使用派生表兼容
SonnetDB 3.1 的别名解析；当前仍不把文档集合或其索引声明伪装成关系表
CodeFirst/DbFirst 模型。若 SonnetDB 后续改变 TVF 合同，请先提供兼容版本或能力元数据，
再由提供程序增加对应映射。

### 23. 文档集合、全文和向量索引的远程 ADO.NET 元数据投影

SonnetDB 服务端 schema 合同已经包含文档集合以及 JSON 路径、全文和向量索引信息；
3.1.0 的远程 ADO.NET `RemoteSchemaResponse` 只投影 measurement 和关系表，导致
这些既有字段不能通过 `GetSchema` 或 DbFirst 到达客户端。请让远程 DTO 和目录接口
投影既有服务端字段，而不是重新设计文档目录，并明确：

- 集合名称、文档主键、JSON 文本列、路径索引/全文索引/向量索引的统一目录视图；
- 索引路径、维度、度量、HNSW 参数、全文分词配置和唯一性等字段的 `GetSchema` 合同；
- 文档插入、更新、删除、批量写入、校验失败和错误码的异步参数化合同；
- 远程与嵌入式目录、权限和快照版本的一致行为。

在远程投影合同出现前，这一项仍是 FreeSql 提供程序的模型边界，不应把普通关系表
`[SonnetDBTable]` 映射误认为文档集合支持。

### 24. 高吞吐批量写入和远程帧协议

SonnetDB 3.1.0 远程 ADO.NET 支持 SQL 帧、向量帧以及 `CommandType.TableDirect` 等
高吞吐路径，但 FreeSql 提供程序当前只通过标准 ADO.NET SQL 生成关系表 `INSERT ... VALUES`
和时序追加写入，不能主动选择 `TableDirect`、Line Protocol、JSON points 或列式帧。
请补充：

- `TableDirect` 的表/测量 schema、列顺序、事务、取消、重试和受影响行数合同；
- 帧协议与 REST/NDJSON 的 DATETIME、VECTOR、JSON、GEOPOINT 类型协商和错误帧映射；
- 批量写入与 ROWVERSION、幂等请求、分片/分批边界以及异步读取的兼容规则；
- ADO.NET 能力元数据，使 ORM 可以在运行时选择安全的高吞吐路径。

在上游提供稳定能力元数据和测试前，提供程序不会把普通 SQL 批量路径静默改写为
未验证的帧协议。

### 25. 关系表的 VECTOR 与 GEOPOINT 类型

SonnetDB 3.1.0 的关系表解析器拒绝 `VECTOR`，FreeSql 也会在关系表 CodeFirst
阶段明确拒绝 `VECTOR` 和 `GEOPOINT`，因此这两种实体属性目前只能用于时序测量
的 FIELD。若 SonnetDB 的产品边界是“关系表不支持这些类型”，请在 SQL 参考、
类型矩阵和 ADO.NET 元数据中明确这一点；若计划支持，请补齐关系表 DDL、
`ALTER TABLE`、参数化读写、NULL、维度和坐标校验、索引、远程结果类型以及
`GetSchema` 合同。FreeSql 在获得稳定合同前不会把时序 FIELD 映射静默迁移到
关系表。

## 版本说明

后续未发布工作树提交 `d664ead6` 已出现 `UPDATE/DELETE RETURNING` 和 `ON CONFLICT` 相关实现，但该提交不属于当前已发布的 NuGet 3.1.0。请在发布说明中明确这些能力对应的版本，避免提供程序按未发布代码建立依赖。

## 期望

完成上述能力后，请补充 SQL 参考、协议测试和 ADO.NET 集成测试。FreeSql 提供程序将在对应稳定版本发布后移除当前限制，并增加端到端回归测试。
