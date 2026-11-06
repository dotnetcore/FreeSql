# 会话交接

## 2026-11-06 #2291 follow-up

- 本次从 PR head `a2268c1ff580b77d9ee202b3d34f230be59eab8b` 创建隔离 worktree，原 `D:\source\FreeSql` 保持干净；FreeSql 仓库没有额外 `AGENTS.md`，本 worktree 的既有 HANDOFF 历史保留。
- 实时 GitHub 回读：`dotnetcore/FreeSql#2291` 仍 OPEN、未合入；上游没有 issue 评论，Copilot 有一条行级评论指出 `GetTablesByDatabase("")` / 空白 selector 在结构化 Views 元数据路径中丢视图。
- Provider 最小修复已完成：空/空白数据库 selector 通过 `MatchesDatabase` 时按当前连接库处理；新增空字符串与空白字符串 DbFirst 视图回归。完整 Provider 套件 **237/237 PASS，0 FAIL，0 SKIP**，`git diff --check` PASS。
- 依赖边界已实测：独占空 NuGet 缓存、仅 NuGet.org 源恢复 PASS；`SonnetDB`、`SonnetDB.Core`、`SonnetDB.Protocol` 均是正式 `5.0.0`，nuspec repository commit 与实际 net10.0 DLL ProductVersion 绑定 `84433f9edcceb84605520ebecc4b2879e3c4aa79`。Provider 自身仍是 FreeSql `3.5.311`，打包 nuspec 明确依赖 SonnetDB `5.0.0`。
- NuGet.org 公开 FreeSql Core 仍为 `3.5.311` 且缺 `DataType.SonnetDB`；`FreeSql.Provider.SonnetDB` flat-container 仍 404。公开组合尚未运行，不能把本地 Provider 包或固定候选 Core 当成上游交付。
- 下一步：在本 worktree 完成最终格式/状态核对后提交并推送 PR head；通过 GitHub 评论向维护者报告修复、237/237、正式5.0.0来源和公开组合边界，邀请审阅合并；同步 SonnetDB #230 实际结果并保持 OPEN，直到上游新 Core/Provider 发布后完成公开组合重跑。

## 2026-11-02 SonnetDB #230 正式交付验收

- 本 worktree 原来没有 HANDOFF.md；本次开始修改前创建，记录此缺口。
- 基线 539070a9774e114df781fffe5a69115186cf8e82，分支 codex/sonnetdb-pr-readiness-20261102，GitHub PR dotnetcore/FreeSql#2291。开始时工作树干净。
- 用户要求完成 SonnetDB#230：核验现有实现、补足正式依赖和消费者验收，证据完整后关闭。SonnetDB#2234 对应的 FreeSql 接入 PR 已合并，不能再记为 Draft。
- SonnetDB 5.0.0 依赖、精细时间无参数 CAST 修复、四项新增回归和独立 ReleaseAcceptance 已完成。FreeSql 核心零修改。完整 Provider 237/237、0 fail/skip；独立候选 NuGet Core/Provider 消费者以全新缓存恢复/构建，并在 embedded/REST/frame-http2 各通过25项业务和8项重开检查。HTTP/2 Frame/事务与 REST SQL 均有真实服务日志收据。
- SonnetDB tag v5.0.0 已推送，固定455bf3b53b1e56df0064d8f574b0595cbb0b609e。十二类候选门禁READY；正式Publish41348714535两平台合同通过，但首个NuGet推送因API Key过期403失败，GitHub Release跳过。Docker41348714479成功，Connectors41348714371三平台构建成功、上传因前置Publish失败而失败。不能将候选通过写成正式包已公开。
- 已请用户更新IoTSharp/SonnetDB仓库NUGET_API_KEY；不要接收/保存明文密钥，不移动tag。恢复时先重跑原Publish的failed jobs，然后恢复Connectors上传、更新正式发行说明、核验十个公开包及匹配Server并运行新缓存消费者。
- 公开FreeSql Core3.5.311缺少DataType.SonnetDB；候选Provider+公开Core的独立构建实报CS0117。公开Provider索引404。本地候选同版本Core/Provider只能证明候选可用，不能冒充NuGet.org交付。#230保留开放，等上游合入#2291并发布新Core/Provider后再次完整验收。
- 原始证据保留在D:/source/SonnetDB/artifacts/release-followup-20261102：freesql-tests-candidate/provider-5.0-candidate-fixed.trx、freesql-all-packages-preflight/result.json、freesql-public-core-build.log。不要覆盖公共Core失败探针。DATETIMEOFFSET按UTC ticks/零offset合同验收，主键迁移使用受支持STRING→INT；不是任意类型迁移、物理掉电或生产负载证据。
- 当前账号对 dotnetcore/FreeSql 只有 READ 权限；可更新 IoTSharp fork 和现有 PR，不能把未合入的变更记为上游已完成。
