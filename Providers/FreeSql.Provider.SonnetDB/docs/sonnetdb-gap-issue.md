# SonnetDB 兼容性与发布依赖

核验日期：2026-11-02（北京时间）。FreeSql [#2234](https://github.com/dotnetcore/FreeSql/pull/2234) 已合并，提交 `05997ef16f0d6a12f051f9a50794a2c21dcd4dc5`；兼容后续 [#2291](https://github.com/dotnetcore/FreeSql/pull/2291) 尚待上游合并。发布门禁继续由 [SonnetDB #230](https://github.com/IoTSharp/SonnetDB/issues/230) 跟踪。

## 固定版本及公开交付

| 对象 | 当前身份 | 证据边界 |
| --- | --- | --- |
| SonnetDB 本轮版本 | `5.0.0`，tag `v5.0.0`，源码 `455bf3b53b1e56df0064d8f574b0595cbb0b609e` | 两平台合同通过；正式 NuGet 推送因 API Key 过期返回 403，公开 NuGet/Server 复验尚未完成 |
| FreeSql Provider | 本 PR 的 5.0.0 依赖、时间参数/字面量及视图修复 | 完整套件与隔离候选包通过，未宣称上游已合并/发包 |
| 公开 FreeSql Core | `3.5.311` | 缺少 `DataType.SonnetDB`，独立消费者构建以 CS0117 失败 |
| 公开 FreeSql.Provider.SonnetDB | NuGet flat-container 索引 404 | 不存在可供普通用户恢复的公开 Provider 包 |

SonnetDB 5.0 存在二进制 API 破坏性变化：共享类型移入 Protocol，分页 API 改为 long。应用与 Provider 必须统一升级并重新编译；需要回退数据库时恢复升级前备份，不能只替换旧 DLL。

仅更新 SonnetDB 依赖不能补齐公开 FreeSql Core。用本地候选 Core/Provider 打包可证明这一源码组合可用，但不能把本地 `3.5.311` 包冒充 NuGet.org 的同名版本。需由上游发布包含 #2234 与 #2291 的新 Core 和 Provider，再以全新缓存执行公开组合验收。

## 本轮实际执行

| 检查 | 已取得结果 |
| --- | --- |
| 原有预览依赖及后续兼容修复 | 历史 233/233 PASS，不覆盖本轮新增四项原生时间回归 |
| SonnetDB 精确 5.0.0 候选依赖，完整 Provider 套件 | **237/237 PASS，0 FAIL，0 SKIP** |
| 候选 Core + Provider NuGet，全新缓存的独立消费者 | restore/build PASS；未引用 FreeSql 或 SonnetDB 源码项目 |
| 业务旅程：embedded、REST、frame-http2 | 每种连接 **25 项 PASS** |
| 独立重开：embedded、REST、frame-http2 | 每种连接 **8 项 PASS**；Server 进程实际终止并重启 |
| 真实 Server 请求收据 | REST SQL 68、HTTP/2 Frame 32、HTTP/2 事务 21 |
| Provider Release pack | PASS，不再有预发布依赖 NU5104 |
| FreeSql Core pack | 原源码五个目标框架 PASS；未修改 FreeSql 核心 |

原始报告记录实际程序集 informational version 与 SHA-256；Server 和消费者 SHA 也分别绑定。验收程序和运行方法见 [ReleaseAcceptance](../../../FreeSql.Tests/FreeSql.Tests.Provider.SonnetDB.ReleaseAcceptance/README.md)。候选产物来自 SonnetDB Actions `41346120155`，正式发行工作流为 `41348714535`；正式公开包复验须单独记录。

本轮修正：

- DateTimeOffset 参数按实际 `DATETIME` / `DATETIME2` / `DATETIMEOFFSET` 目标列绑定，默认毫秒列类型不变。
- 无参数 INSERT、UPDATE 和强制无参数的 UPSERT 对精细时间生成显式 CAST；新增参数/无参数 × INSERT/UPDATE/UPSERT 四种回归。
- DbFirst 优先使用结构化视图身份，与 Tables 结果按 schema/name 去重；不支持 Views 元数据时保留旧版回退。
- DATETIMEOFFSET 按已有引擎合同保存 UTC ticks、返回零 offset；不是保留输入时区。初次验收的 offset 预期已校正，未改变数据库或 FreeSql 核心。
- 主键迁移旅程使用已支持的 STRING→INT；不承诺任意 INT→STRING 转换。

## 已有接口的验收范围

| 上游任务 | 当前证据 |
| --- | --- |
| #224 measurement 完整分组 | 三种连接均通过同步/异步 GroupBy、Having、聚合排序与分页 |
| #229 SELECT 多结果 | 三种连接均通过空首结果的列元数据与后续结果 |
| #226 主键迁移 | 三种连接均通过主键列重命名、STRING→INT 迁移与重开 |
| #227 关系 DDL 事务 | 三种连接均通过建表/写入的提交、回滚及重开 |
| #233 / #248 锁定读 | 三种连接均通过双事务 NOWAIT、SKIP LOCKED 与回滚后释放 |
| #236 精细时间 | 三种连接均通过 DATETIME2 100ns、DATETIMEOFFSET 完整 UTC ticks；参数/无参数路径另有完整套件回归 |
| #237 / #240 注释与长度 | 完整套件覆盖注释及长度；三种连接旅程核验 DbFirst 字符串长度/DECIMAL 元数据 |
| #247 存储过程 | 三种连接均通过 OUT/INOUT/ReturnValue 和两个结果集 |
| #250 约束与索引 | 完整套件覆盖 DESC 索引等已有映射；FreeSql 共享 IndexInfo 没有 INCLUDE API |
| #255 / #312 关系 DML | 完整套件覆盖无主键关系 DML及普通 INT 主键值更新；三种连接旅程覆盖关键 CRUD/RETURNING/UPSERT |
| #314 DECIMAL round | 完整套件覆盖精确舍入；三种连接旅程覆盖完整精度 DECIMAL 写读 |
| #348 schema/table 元数据 | 三种连接均核验结构化视图去重及列元数据；复杂跨 schema 组合不由此推断全部通过 |
| #380 EXISTS/NULL | 已有引擎与公开 ADO 定向回归，修复包含在固定 5.0.0 源码中 |

完整 Provider 套件包含嵌入式执行和 SQL 生成断言，不将其中全部条目冒充三协议测试；独立业务旅程只承诺其实际断言覆盖的范围。

## 产品边界

- measurement 是独立时序模型；STRING FIELD 等限制不列为关系表缺陷。DATETIME 默认仍为毫秒，精细时间须声明原生列类型。
- `FOR SHARE` 仍明确拒绝，FreeSql 当前没有对应公共入口。`ForUpdate(skipLocked: true)` 已有三协议消费者证据。
- 关系表 VECTOR/GEOPOINT 仍依 [SonnetDB #349](https://github.com/IoTSharp/SonnetDB/issues/349) 的产品边界明确拒绝；不伪装成普通关系列。
- 本轮不增加 FreeSql 公共 API，不包括任意精度整数、公开 savepoint、原生 COPY、九模型 ORM 化、物理掉电、生产负载或 Server/MQ 全实例恢复。

## 完整关闭条件

- [ ] SonnetDB 正式十包、tag、发行说明和匹配 Server 已公开，源码身份及公开包消费者复验通过。
- [x] Provider 5.0.0 候选完整回归、独立三协议业务与重开、打包通过。
- [x] 独立候选 Core/Provider 的纯 NuGet 消费者恢复、构建及三协议验收通过。
- [ ] FreeSql 上游合入 #2291，发布包含 SonnetDB 接入的新 Core 与 Provider。
- [ ] 使用新的缓存、公开 FreeSql/SonnetDB 包及正式 Server 完成同一消费者旅程。

这些条件完成前，#230 保持开放。数据库正式发布、上游合并和上游包可安装是三个需要分别证明的状态。
