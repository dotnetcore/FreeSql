# FreeSql / SonnetDB 下一会话交接

日期：2026-10-24，Asia/Shanghai。用户要求：**在新会话中解决除发布 SonnetDB 4.5 以外的工作**。本文件用于新会话接续；当前会话不继续功能实现。

## 用户目标与授权

完成 FreeSql SonnetDB provider 需要的关系数据库能力：核查 FreeSql PR 及相关 SonnetDB PR/issue，补齐 provider；数据库确有缺口时在 SonnetDB 实现并验证，更新关联记录。用户明确要求把实际缺口发到 SonnetDB，更新 PR 正文并回复最新核查结果。

FreeSql 的主要目标是关系表。measurement / 时序模型的限制不得被当作关系数据库缺口，也不得作为关系 provider 的无依据发布阻塞项。必须证明某项来自既有 FreeSql 入口，再决定属于数据库实现、provider 实现、测试不足还是产品边界。

不执行 SonnetDB 4.5 正式发布，也不发布 FreeSql provider。其余可以现在完成的源码、测试、候选包消费、真实本地 Server 组合和文档/PR/issue 工作应现在完成，不能全部推给正式包发布以后。更新现有 PR 的授权包含与本任务有关的 scoped commit/push 和评论；不推定合并或正式发行授权。

## 工作区与已交付基线

- FreeSql：`D:\source\FreeSql`，分支 `master`，当前 HEAD / origin/master：`072ed609aef84a2503def741b398b37318dbc964`。
- FreeSql 上游 PR：<https://github.com/dotnetcore/FreeSql/pull/2234>，OPEN / Draft。上一轮已推送代码并刷新正文与评论：<https://github.com/dotnetcore/FreeSql/pull/2234#issuecomment-6381882770>。
- SonnetDB：`D:\source\SonnetDB`。上一轮确认 main / origin/main 为 `4617b2a4406e77b7147736c53c3e975dd64d65d2`；新会话须重新核对，不能当作永久事实。
- SonnetDB 有大量无关文档、博客、发布证据等 dirty 文件，必须保留。仅修改/暂存本任务明确拥有的路径或 hunks，不 reset、clean、stash-all 或广泛 add。
- 上一轮确认稳定版为 4.0.0，公开预览为 `4.5.0-preview.ef266.5`（对应源码 `1af23d8f`）。它不包含后续所有 main 能力；新会话应核对实时版本，禁止冒充已正式发布 4.5。

## 重要证据限制

`072ed609` 的提交标题含 complete，**并不表示全部关系消费者已闭环**。229 项回归包含 SQL 生成及不支持断言，多项新能力只是解除 provider guard 或改为生成 SQL 的断言，不能由 229/229 推断数据库执行、事务、精度或远程均通过。

上一轮 Release build / package 通过，0 errors，NU5104 提示稳定 provider 依赖预发布 SonnetDB。当前会话在启动交接前误运行了一次基线测试，229/229 通过、0 skipped，未新增功能测试。证据：

- `FreeSql.Tests/FreeSql.Tests.Provider.SonnetDB/TestResults/sonnetdb-consumer.trx`。
- `C:\Users\mysti\.codex\skills\.local\process-6503b478762a4d368afe6c33b32e7c23.json`。

该次进程根 PID 46488，结束于 2026-10-24T08:51:41Z，exit 0、cleanupComplete=true；代理回读根及六个后代均已退出。构建生成的 `FreeSql/FreeSql.xml` 313 行差异经主智能体核对后恢复，不能混入后续功能提交。测试证据保留。

## 尚待逐项确认和完成的工作

以下是静态审查线索；除已注明的基线测试，**尚未执行新增消费者验证**。不得直接作为已确认数据库缺陷或完成结论对外发表。

1. **DbFirst / schema 身份**：`SonnetDBDbFirst` 将两段名的首段当数据库，`GetObjectName` 等路径丢 schema；`FindTable` 可退回裸名。核对 CodeFirst ExistsTable / rename、DESCRIBE、SHOW INDEXES、SELECT、外键引用等路径。SonnetDB Core 有 schema 身份，但上一轮 Data 的 Build*Schema 仍写空 TABLE_SCHEMA，需要数据库侧及 Server 元数据接线和双 schema 同名表消费者，不能仅写文档声称 fail-closed。
2. **DDL / 注释 / 字符串长度 / 索引**：DbFirst table REMARKS 尚待检查，列已开始读取 REMARKS；COMMENT 替换/清除及幂等需要执行验证。`GetTableDataType` 若先把默认 attr.DbType=STRING 规范化立即返回，可能绕过 StringLength fallback；验证显式 STRING(n)、MaxLength、缩短失败原子性。DESC 已输出，但 DbFirst AddIndex 仍可能总设 IsDesc=false，IsSameIndex 未比较方向；INCLUDE 不属于共享 IndexInfo API，不为此擅自扩 API。
3. **主键迁移与 keyless DML**：引擎已有主键 rename/type/definition 迁移，provider 仍对特殊列定义变化报拒绝；不能仅换拒绝文案称完成。用现有 CodeFirst 入口验证 OldName、类型/键定义变化、索引/约束、失败恢复；普通单列非自增 INT 主键值更新、keyless INSERT/SELECT/UPDATE/DELETE/重复行/事务/RETURNING 必须实际执行。复杂外键拓扑是否支持须以引擎合同和实测决定。
4. **时间类型**：显式 DATETIME2/DATETIMEOFFSET 已开始接线，但默认 DATETIME 仍是毫秒。核对 typed 参数、非参数字面量、表达式常量、AddslashesProcessParam、实体/匿名/标量/可空读回、RETURNING 及日期运算；DbFirst DATETIMEOFFSET 不应静默降成 DateTime。raw ADO col=null 路径可能把 DateTime/DateTimeOffset 转 Unix ms。先复现，再以模型/显式类型维持关系与 measurement 的不同语义，禁止放宽引擎真实类型不匹配守卫来凑测试。
5. **ADO 消费者**：SonnetDBAdo 静态看透传了 CommandType 和参数对象，但标准 StoredProcedure、OUT/INOUT/ReturnValue、Reader 关闭回写、多结果 Query<T1,T2>/ExecuteDataSet 同异步均缺实际 FreeSql 证据。`AdoProviderAsync.ExecuteReaderMultipleAsync` 结果切换处疑似调用同步 NextResult，核对取消/超时合同；共享核心修复须有实际复现、影响评估和相关 provider 回归，优先将 SonnetDB 适配保留在 provider。
6. **锁定读**：已生成 FOR UPDATE/NOWAIT，尚需真实事务、多连接竞争、超时/取消、提交/回滚释放、排序分页与不支持形状执行。SKIP LOCKED 是现有 FreeSql ForUpdate(skipLocked:true) 的明确需求。FOR SHARE 没有在本轮确认既有 FreeSql 专门入口，应与 SKIP LOCKED 分开验收，避免无依据扩大必选范围。
7. **关系 SQL / 数值 / 组合**：对真正关系表执行 GroupBy/Having/聚合投影/多键排序/分页、JOIN/Update.Join、CTE、UPSERT/RETURNING、无 WHERE UPDATE、decimal Round 大于 2^53/正负 midpoint/0..28/NULL/元数据。历史 measurement #224 不替代关系表 GroupBy 证据。未支持的 .NET 重载需要确认是否确为数据库缺口，不为所有类型/函数全集随意开单。
8. **真实连接与候选包**：不要等待正式 4.5 才完成可现在运行的测试。以可追溯的已提交 SonnetDB main 构建任务专用本地候选 feed/Server，隔离包版本与缓存，记录 SHA/包哈希/客户端-服务端组合，实际跑嵌入式与适用 REST/h2c/HTTP2 ADO。源 DLL 替换只能算二进制试验，不能当正式包消费。远程 DDL 事务若确实未实现，应在 SonnetDB 补齐并做 FreeSql 消费者；区分 NOT_RUN 与 FAIL。

## Issue / PR 处理

- #347 OPEN：<https://github.com/IoTSharp/SonnetDB/issues/347>，SKIP LOCKED / FOR SHARE。继续核查，优先交付已有 FreeSql skipLocked 入口必需范围，FOR SHARE 单独标识。
- #348 OPEN：<https://github.com/IoTSharp/SonnetDB/issues/348>，schema/table 结构化 ADO 元数据。
- #349 OPEN：<https://github.com/IoTSharp/SonnetDB/issues/349>，关系 VECTOR/GEOPOINT。现有 #193 允许文档化产品边界，不意味着 FreeSql 关系适配必须实现 PostgreSQL 全部专有类型；先核实实际入口与必要性，避免重复或过度扩大需求。
- #346 CLOSED：<https://github.com/IoTSharp/SonnetDB/issues/346>，measurement STRING FIELD 已按用户纠正移出关系缺口，不重新作为关系发布阻塞。
- #227 / #233 / #193 上一轮已评论，但使用了同一段宽泛文本；应按每个 issue 实际范围补充准确执行结果，不能复用无关内容或批量修改/关闭。
- 原 #230 已按源码范围关闭，正式发布证据与消费者未完成。其余历史关闭项逐项按最新源码/测试复核，不从 closed 推断包已发布。
- PR 正文曾声称 DbFirst 非-main schema fail-closed、关系合同已补齐；这些文字必须与最终实现/消费者结果核对并纠正。最终改写应按用户关系表主目标组织，而不是继续混入 measurement 限制。

## 推荐执行顺序和完成标准

先读取两仓适用 AGENTS / 状态 / 当前源码 / CI，确定路径归属；建立需求—provider—数据库—消费者矩阵。集中修复上述确认原因；主智能体持有集成和 Git 操作，代理分文件并行审查/实现，dotnet 构建测试串行以免资源竞争。完成关系消费者与适用本地真实 Server 组合后，回读 exact-head CI 和 PR 状态，更新文档与逐项 issue。

除正式发行外，应交付：可审查的 scoped commits、对应测试和候选包/Server 身份、所有真实 FAIL/UNKNOWN/NOT_RUN 的责任与结果、更新后的 PR/issue。不能仅靠旧229项/SQL生成测试宣称全部支持；不能将当前能完成的工作简单标成“等4.5后再做”。

机器规则：仅用 PowerShell 7（`C:\Program Files\PowerShell\7\pwsh.exe`）；搜索/循环/重试/批次有数量与墙钟上限、取消/进度；进程记录 PID/创建时间/命令/父链并只回收自建树；临时文件精确核对路径后清理。网络失败按单命令代理 127.0.0.1:7890 重试。Graphify 禁用。向代理重复这些约束，保留无关 dirty 文件。SSH Bash 通过机器规定的 stdin wrapper，不用 PowerShell 可展开字符串嵌 Bash。

## 本会话收口（2026-10-24）

- FreeSql provider owned changes are ready for commit from the `072ed609` baseline: DbFirst schema/remarks/index direction/DateTimeOffset mapping and qualified-name fallback; CodeFirst STRING(n), DESC comparison and OldName handling; shared `ForUpdate(skipLocked: true)` SQL generation with the stale provider guard removed; targeted tests updated accordingly.
- FreeSql provider validation: **230/230 PASS, 0 skipped**, provider build 0 errors with the existing `NU5104` preview dependency warning; DbFirst qualified-name 3/3 and foreign-key metadata 2/2 PASS; `git diff --check` PASS.
- SonnetDB database changes were committed and pushed through upstream merge as `2d7a6d5cd589b31f888b65088715b55f8ae39cce` on `origin/main`. It contains `FOR UPDATE SKIP LOCKED` parser/AST/executor locking and schema identity in ADO/INFORMATION_SCHEMA/Server/Remote DTOs. Core/ADO 92/92 and REST remote 65/65 PASS; mixed REST/frame-http2 batch 99/131 retained 32 exact HTTP/2 environment failures.
- Candidate evidence: a source-traceable SonnetDB main candidate passed embedded ADO multi-result sync/async and DataSet probes; public preview `4.5.0-preview.ef266.5` failed that same multi-result consumer and is not treated as main evidence. No SonnetDB 4.5 formal release or FreeSql provider release was performed.
- Remaining gates are explicit: formal 4.5 package and final consumer readback are `NOT_RUN`; local frame-http2 endpoint is unavailable in this host; `FOR SHARE` and relational VECTOR/GEOPOINT remain product boundaries. Measurement limits remain outside the relational matrix.
