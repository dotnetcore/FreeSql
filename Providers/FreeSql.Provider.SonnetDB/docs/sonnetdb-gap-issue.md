# SonnetDB 兼容性与发布依赖

核验日期：2026-10-24（北京时间）。FreeSql PR [#2234](https://github.com/dotnetcore/FreeSql/pull/2234) 仍为 Draft。SonnetDB 源码 main 已经实现了多项此前阻塞 FreeSql 的关系 SQL/ADO 合同，但稳定 4.5、对应 Server 产物和 FreeSql 正式消费者验收仍未完成。

## 版本与证据边界

| 对象 | 当前身份 | 可依赖范围 |
| --- | --- | --- |
| SonnetDB 稳定版 | `4.0.0` | 已发布包合同 |
| SonnetDB 公开预览 | `4.5.0-preview.ef266.5`，源码 `1af23d8f` | 预览包合同；不包含后续全部 main 实现 |
| SonnetDB 当前 main | `4617b2a4406e77b7147736c53c3e975dd64d65d2` | 源码实现和对应仓库测试；不等同于已发布包 |
| FreeSql provider 当前分支 | `fb1ad796f` 加本地适配改动 | 本地源码/测试；不等同于已发布 FreeSql 包 |

本地 provider 回归当前为 **229/229 通过、0 跳过**。这只覆盖现有预览包可执行的嵌入式合同和 SQL 生成；远程 REST/h2c/HTTP2、正式 SonnetDB 4.5 包、发布后 FreeSql 消费者仍分别验收。

## 已接入的关系表能力

- 参数化 INSERT/SELECT/UPDATE/DELETE、批量更新、ROWVERSION 乐观并发和实体/投影 `RETURNING`。
- `InsertOrUpdate` / 字典 UPSERT 的原子 `ON CONFLICT` 映射。
- CTE、递归 CTE、UNION ALL、WithMemory、派生查询、InsertInto，以及关系表 JOIN 和 Update.Join。
- 无 WHERE 关系 UPDATE、普通单列 INT 主键值更新、无显式主键关系表 DML：provider 已生成对应 SQL，必须用包含实现的正式包做最终消费者回归。
- 显式事务内 `ForUpdate()` / `ForUpdate(nowait: true)` 映射为 `FOR UPDATE` / `FOR UPDATE NOWAIT`；provider 对 `SKIP LOCKED` 保持 fail-closed。
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

## 当前明确未完成或需要数据库继续提供的关系能力

- `SKIP LOCKED` 与 `FOR SHARE`：已创建 [SonnetDB #347](https://github.com/IoTSharp/SonnetDB/issues/347)。`ForUpdate(skipLocked: true)` 不得静默降级。
- schema/table 的 Data/Server/远程 `GetSchema` 结构化身份：已创建 [SonnetDB #348](https://github.com/IoTSharp/SonnetDB/issues/348)。在 `TABLE_SCHEMA` 等元数据完整前，DbFirst 不猜测非 `main` schema。
- 关系表 `VECTOR/GEOPOINT` 列：已创建 [SonnetDB #349](https://github.com/IoTSharp/SonnetDB/issues/349)。provider 当前按产品边界明确拒绝，不把 measurement/document 专用类型伪装成关系列。
- measurement STRING FIELD 属于时序模型，已单独核查后关闭 [SonnetDB #346](https://github.com/IoTSharp/SonnetDB/issues/346)，不计入关系表能力缺口。

## 发布门禁

- [ ] SonnetDB 发布稳定 4.5 NuGet、tag、Server 产物和发行说明，并绑定源码身份。
- [ ] provider 升级正式依赖，重跑 229 项回归、关系消费者、远程组合、打包和包身份检查。
- [ ] 完成远程 DDL、schema metadata、StoredProcedure、DATETIME2、主键迁移和锁定读边界的独立证据。
- [ ] 刷新 PR #2234 正文和评论，区分源码已实现、provider 已接入、正式包待发布、远程 `NOT_RUN` 与未知状态；在此之前不解除 Draft 或发布新 provider 版本。

本轮不增加 FreeSql 公共 API，也不把 measurement 专用限制当作关系表缺口。公开 savepoint、原生 COPY/Frame、SonnetDB 全模型 ORM 化和 PostgreSQL 专有类型不在本 provider 交付范围。
