# SonnetDB 兼容性与发布依赖

核验日期：2026-11-02（北京时间）。FreeSql PR [#2234](https://github.com/dotnetcore/FreeSql/pull/2234) 已于 2026-11-01 合并，提交为 `05997ef16`。SonnetDB 最新源码的修复与正式发行验收分别记录，不再将已合并 PR 标为 Draft。

## 版本与证据边界

| 对象 | 当前身份 | 可依赖范围 |
| --- | --- | --- |
| SonnetDB 稳定版 | `4.0.0` | 已发布包合同 |
| SonnetDB 公开预览 | `4.5.0-preview.ef266.5`，源码 `1af23d8f` | 预览包合同；不包含后续全部 main 实现 |
| SonnetDB 本次核验 main | `ad8f7a0181c8d2bba8a6472f339bc4bafadc8763` | 源码实现和对应仓库测试；不等同于已发布包 |
| FreeSql provider | `05997ef16` 加提供程序内的时间参数与视图兼容修复 | 不更改 FreeSql 核心或现有公共 API；不等同于已发布 FreeSql 包 |

公开预览依赖和最新源码引用分别运行完整 provider 回归，两组均为 **233/233 通过、0 失败、0 跳过**。测试包含嵌入式执行与 SQL 生成断言；远程 REST/h2c/HTTP2、正式包及发布后消费者需要独立证据。

## 2026-11-02 修复核验

- [#380](https://github.com/IoTSharp/SonnetDB/issues/380)：修复 `48fe989b` 已随 #381 进入本次 main。普通 NULL 等值条件不再放弃 EXISTS 索引路径，保留单独 `IS NULL` 的正确结果；关系 EXISTS 组 22/22 通过。
- [#240](https://github.com/IoTSharp/SonnetDB/issues/240)：`STRING(n)` / `VARCHAR(n)` 的长度约束、迁移、重开及 ADO 元数据已有实现；关系列元数据组 39/39 通过。以上为本次源码定向执行，不代表旧预览包或远程协议的同项验收。
- [#348](https://github.com/IoTSharp/SonnetDB/issues/348)：Data、Server、Remote 的 schema/table 结构化身份已有实现；提供程序优先采用结构化视图身份，并与 Tables 结果按名称和 schema 去重，不支持 Views 元数据时才回退旧版 `SHOW VIEWS` 补查。
- DateTimeOffset 默认映射保留 `DATETIME` 毫秒合同；参数按目标列选择 `DateTime`、`DateTime2` 或 `DateTimeOffset`，不以更改默认建表类型引入迁移。

## 已接入的关系表能力

- 参数化 INSERT/SELECT/UPDATE/DELETE、批量更新、ROWVERSION 乐观并发和实体/投影 `RETURNING`。
- `InsertOrUpdate` / 字典 UPSERT 的原子 `ON CONFLICT` 映射。
- CTE、递归 CTE、UNION ALL、WithMemory、派生查询、InsertInto，以及关系表 JOIN 和 Update.Join。
- 无 WHERE 关系 UPDATE、普通单列 INT 主键值更新、无显式主键关系表 DML：provider 已生成对应 SQL，必须用包含实现的正式包做最终消费者回归。
- 显式事务内 `ForUpdate()` / `ForUpdate(nowait: true, skipLocked: true)` 映射为 `FOR UPDATE` / `FOR UPDATE NOWAIT` / `FOR UPDATE SKIP LOCKED`；`FOR SHARE` 仍保持未支持。
- DECIMAL 参数/精度、精确基础表达式、`Math.Round(decimal[, int])` 的十进制映射；`MidpointRounding` 重载明确拒绝。
- CodeFirst 的 qualified table SQL、嵌入式 DDL 事务拆分、无主键建表、DESC 索引、注释和 `STRING(n)` 生成路径已接入；主键拓扑/类型迁移、远程事务和元数据仍需正式包复验。

measurement 是独立的时序模型。其追加写入、删除、VECTOR 和数值 FIELD 更新边界不能当作关系表缺口；provider 文档和测试将两种模型分开。

## SonnetDB main 已实现、待正式包与消费者验收

| 任务 | 已实现的引擎合同 | FreeSql 仍需验收 |
| --- | --- | --- |
| [#224](https://github.com/IoTSharp/SonnetDB/issues/224) | measurement 分组 HAVING、聚合投影、隐藏聚合排序、多键排序和分页 | 正式包下 GroupBy/Having/OrderBy/Limit/ToList 同步/异步消费者 |
| [#229](https://github.com/IoTSharp/SonnetDB/issues/229) | 多 SELECT、NextResult/Async、空首结果元数据 | `IAdo.Query<T1,T2>`、ExecuteDataSet 同步/异步消费者 |
| [#226](https://github.com/IoTSharp/SonnetDB/issues/226) | 存量主键列重命名、类型/定义迁移 | CodeFirst OldName、失败恢复和正式包消费者 |
| [#227](https://github.com/IoTSharp/SonnetDB/issues/227) | 嵌入式关系 DDL workspace、提交/回滚和失败隔离 | 远程 DDL 事务仍 `NOT_RUN`；正式包组合待验 |
| [#233](https://github.com/IoTSharp/SonnetDB/issues/233) / [#248](https://github.com/IoTSharp/SonnetDB/issues/248) | 单关系基表 FOR UPDATE/NOWAIT | provider 复杂形状/measurement 边界和正式包远程回归 |
| [#236](https://github.com/IoTSharp/SonnetDB/issues/236) | DATETIME2/DateTimeOffset 100ns 精度和远程 typed 链路 | provider 显式 DATETIME2/DATETIMEOFFSET 参数、读回和正式包 |
| [#237](https://github.com/IoTSharp/SonnetDB/issues/237) / [#240](https://github.com/IoTSharp/SonnetDB/issues/240) | COMMENT、STRING(n)、长度/备注元数据 | FreeSql 注释替换/清除、DbFirst MaxLength/REMARKS |
| [#247](https://github.com/IoTSharp/SonnetDB/issues/247) | StoredProcedure、OUT/INOUT/ReturnValue、多结果 | 标准 FreeSql ADO 消费者和正式包 |
| [#250](https://github.com/IoTSharp/SonnetDB/issues/250) | 约束目录、DESC/INCLUDE 索引、自增维护 | FreeSql DESC 回归；共享 IndexInfo 无 INCLUDE API |
| [#255](https://github.com/IoTSharp/SonnetDB/issues/255) | 无显式主键关系表 DML | FreeSql keyless 实体 INSERT/SELECT/UPDATE/DELETE 正式包回归 |
| [#312](https://github.com/IoTSharp/SonnetDB/issues/312) | 普通单列 INT 主键值更新 | Set 主键、冲突/事务/RETURNING 正式包回归 |
| [#314](https://github.com/IoTSharp/SonnetDB/issues/314) | DECIMAL round ToEven、0–28 位 | 大数/负数/边界和正式包消费者 |

## 产品边界与远程验收

- `FOR SHARE` 仍明确拒绝，现有 FreeSql 没有对应公共查询入口；[SonnetDB #347](https://github.com/IoTSharp/SonnetDB/issues/347) 的 `SKIP LOCKED` 已实现。`ForUpdate(skipLocked: true)` 的正式 Client/Server 组合仍需独立执行验收。
- schema/table 的结构化元数据不再列为“未实现”；[SonnetDB #348](https://github.com/IoTSharp/SonnetDB/issues/348) 的正式包和远程 FreeSql 消费者仍单独核验。
- 关系表 `VECTOR/GEOPOINT` 列：已创建 [SonnetDB #349](https://github.com/IoTSharp/SonnetDB/issues/349)。provider 当前按产品边界明确拒绝，不把 measurement/document 专用类型伪装成关系列。
- measurement STRING FIELD 属于时序模型，已单独核查后关闭 [SonnetDB #346](https://github.com/IoTSharp/SonnetDB/issues/346)，不计入关系表能力缺口。

## 发布门禁

- [ ] SonnetDB 交付包含所需能力的正式 NuGet、tag、Server 产物和发行说明，并绑定源码身份及兼容边界；不预设下一正式版本必须为 4.5。
- [ ] provider 升级实际正式依赖，重跑完整回归、关系消费者、远程组合、打包和包身份检查。
- [ ] 完成远程 DDL、schema metadata、StoredProcedure、DATETIME2、主键迁移和锁定读边界的独立证据。
- [x] 确认接入 PR #2234 已合并；该状态不作为数据库及 provider 正式发行的放行证据。

正式发行与消费者验收继续由 [SonnetDB #230](https://github.com/IoTSharp/SonnetDB/issues/230) 跟踪。

本轮不增加 FreeSql 公共 API，也不把 measurement 专用限制当作关系表缺口。公开 savepoint、原生 COPY/Frame、SonnetDB 全模型 ORM 化和 PostgreSQL 专有类型不在本 provider 交付范围。
