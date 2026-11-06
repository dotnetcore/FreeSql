# SonnetDB 5.0 兼容性与发行验收

核验日期：2026-11-06（北京时间）。FreeSql [#2234](https://github.com/dotnetcore/FreeSql/pull/2234) 已合并；本后续 [#2291](https://github.com/dotnetcore/FreeSql/pull/2291) 可供上游审阅。SonnetDB [#230](https://github.com/IoTSharp/SonnetDB/issues/230) 保持 OPEN，直到新的 FreeSql Core/Provider 公开发行并通过最终公开组合验收。

## 版本与来源

| 对象 | 本轮实际身份 | 状态 |
| --- | --- | --- |
| SonnetDB、SonnetDB.Core、SonnetDB.Protocol | NuGet.org 正式 `5.0.0`；repository commit 与实际 DLL ProductVersion 均绑定 `84433f9edcceb84605520ebecc4b2879e3c4aa79` | PASS：独占空缓存、来源/载荷/签名及运行 DLL |
| SonnetDB Server | 重新下载公开 `sonnetdb-full-5.0.0-win-x64.zip`，NativeAOT Server 同为 source844 | PASS：archive digest、Server hash、版本及真实协议 |
| 本轮 FreeSql Core/Provider | 测试源码 `85388bae4b9a21a603588c7e5faa4435e1f1c790`；本地候选包 `3.5.311-sonnetdb230.20261106.1` | PASS：五目标 Core 构建/打包，独立纯包消费者 |
| NuGet.org FreeSql | 公开稳定 `3.5.311` | FAIL：新缓存负控实报 CS0117，缺 `DataType.SonnetDB` |
| NuGet.org FreeSql.Provider.SonnetDB | flat-container 索引 HTTP 404 | 尚无公开包 |
| 完全公开 FreeSql + SonnetDB 组合 | 需要上游发布包含 #2234/#2291 的 Core 和 Provider | NOT_RUN |

FreeSql 源码中的正式版本仍遵循上游 `3.5.311`。候选版本仅用于本轮本地验收，未上传 NuGet.org，也未伪装成已发布同名包。消费者只有 PackageReference；本地 feed 仅允许 FreeSql 两包，SonnetDB 三包从 NuGet.org 恢复。5.0.0 之后的 SonnetDB main 修复不会追溯进入既有正式包。

## 本轮实际验证

| 门禁 | 结果 |
| --- | --- |
| 完整 Provider 套件 | **237/237 PASS，0 FAIL，0 SKIP** |
| embedded 纯包消费者 | **30 项业务 + 9 项独立重开 PASS** |
| REST 纯包消费者 | **30 项业务 + 9 项独立重开 PASS** |
| 原生 HTTP/2 Frame 纯包消费者 | **30 项业务 + 9 项独立重开 PASS** |
| 正式 Server 实际请求 | REST SQL **81**、HTTP/2 Frame **45**、HTTP/2 事务 **21** |
| 包来源和实际加载身份 | 三个 SonnetDB 包均 source844；六消费者的 FreeSql DLL 均 source85388bae4，hash 与安装包载荷相同 |
| SonnetDB 三包签名 | `dotnet nuget verify --all` PASS |
| Provider / 消费者 scoped Format、Node 语法、diff | PASS；Format 工作区加载警告保留 |

本轮业务旅程保留原有 CRUD、INSERT/UPDATE/DELETE RETURNING、原子 UPSERT/DO NOTHING、精确 DECIMAL、100ns DATETIME2、DATETIMEOFFSET UTC ticks、锁竞争、DDL 提交/回滚、STRING→INT 主键迁移、存储过程、多结果及空首结果。新增空/空白数据库 selector 的视图发现、NULL 往返、预取消与取消后连接继续使用。三种连接在同一目录重开后核验已提交状态和元数据；远程 Server 实际终止后重新启动。完整套件含 SQL 生成和嵌入式执行，不能把 237 项都算为三协议测试。

原始代码、运行入口见 [ReleaseAcceptance](../../../FreeSql.Tests/FreeSql.Tests.Provider.SonnetDB.ReleaseAcceptance/README.md)，本轮包/DLL/Server 身份和统计见 [机器清单](sonnetdb-release-acceptance-20261106.json)。先前候选 455bf3b5 的发行阻塞属于历史；本报告使用已公开的最终 source844，未将旧测试改名为新结果。

## FreeSql 关系合同与模型边界

| 范围 | 当前实际覆盖和限制 |
| --- | --- |
| 关系 CRUD / 返回值 / UPSERT | 参数化及既有无参数路径、RETURNING、影响行数、生成版本值、冲突与 DO NOTHING |
| 精确类型与 NULL | DECIMAL 原生精度、显式 DATETIME2/DATETIMEOFFSET 100ns、默认 DATETIME 毫秒合同、NULL 读回；DATETIMEOFFSET 返回 UTC ticks/零 offset |
| 标识符 / 元数据 | 保留 schema/name 与原始拼写、结构化视图去重；空和空白数据库 selector 代表当前库；未知库不会返回当前库对象 |
| 事务 / DDL | 消费者验证显式事务内建表/写入提交和回滚、重开、受支持的主键列重命名与 STRING→INT 转换；不承诺任意主键转换 |
| 锁 / 取消 / 重开 | NOWAIT、SKIP LOCKED、回滚释放、预取消及后续连接可用；不把预取消扩称运行中取消/网络断连全覆盖 |
| measurement | 独立时序模型；本轮验证同步/异步 GroupBy、HAVING、聚合排序/分页；其 FIELD/更新限制不当作关系表缺陷 |
| 产品边界 | 关系 VECTOR/GEOPOINT 明确拒绝；FOR SHARE 没有当前 FreeSql 公共入口；共享 IndexInfo 无 INCLUDE API |

没有新增 FreeSql 核心公共 API。公开 savepoint、原生 COPY、九模型 ORM 化、物理掉电、生产负载、全模型恢复及 Server/MQ 全实例恢复不在本 PR 的验收范围。Server 进程重启通过不代表这些项目通过。

## 剩余关闭条件

- [x] SonnetDB 5.0.0 正式包、tag、发行说明及匹配 Server 已公开。
- [x] 本轮 Provider 完整套件、独立候选包和三协议/重开完成。
- [x] #2291 的空/空白数据库 selector review 意见已修复并有真实消费者验证。
- [ ] 上游合入 #2291。
- [ ] 上游正式发布含接入的新 FreeSql Core/Provider。
- [ ] 独占空缓存、完全公开 FreeSql/SonnetDB 组合重跑同一消费者旅程后，关闭 #230。

上述上游决定和公开发行尚未发生。当前可由贡献者完成的代码、固定 5.0.0 候选验证和审阅材料已完成，等待维护者审阅。
