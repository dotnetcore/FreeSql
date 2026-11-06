# SonnetDB 正式组合消费者验收

此独立程序引用当前 FreeSql SonnetDB Provider 项目及其真实 `SonnetDB 5.0.0` NuGet 依赖，不引用 SonnetDB 源码项目。需要 .NET 10、Node.js 22 或更新版本，以及同一版本发行包中的本机 Server 可执行文件。正式发行前可使用明确的候选 feed 预检，但不能把候选 feed 的结果记录为公开 NuGet 验收。

先用新的包缓存恢复并构建，再运行完整 Provider 测试和独立消费者：

```powershell
dotnet restore FreeSql.Tests/FreeSql.Tests.Provider.SonnetDB.ReleaseAcceptance/FreeSql.Tests.Provider.SonnetDB.ReleaseAcceptance.csproj --packages <new-package-cache>
dotnet build FreeSql.Tests/FreeSql.Tests.Provider.SonnetDB.ReleaseAcceptance/FreeSql.Tests.Provider.SonnetDB.ReleaseAcceptance.csproj -c Release --no-restore
dotnet test FreeSql.Tests/FreeSql.Tests.Provider.SonnetDB/FreeSql.Tests.Provider.SonnetDB.csproj -c Release
node FreeSql.Tests/FreeSql.Tests.Provider.SonnetDB.ReleaseAcceptance/run.mjs <absolute-server-executable> <new-evidence-directory>
```

`run.mjs` 拥有测试 Server 的完整生命周期，以独立目录、临时令牌和 loopback HTTP/1.1、HTTP/2 端口运行。它依次执行 embedded、REST 和 `frame-http2` 的写读旅程，再终止并重启 Server，分别重开并验证持久数据。连接字符串只经子进程环境传递，报告不保存令牌。脚本检查实际 Server 日志中的 REST SQL、HTTP/2 Frame 和 HTTP/2 事务请求，缺少任一协议收据均失败。

旅程覆盖参数化 CRUD、原生 INSERT/UPDATE/DELETE RETURNING、UPSERT/DO NOTHING、100ns DATETIME2、DATETIMEOFFSET UTC 时刻、精确 DECIMAL、竞争锁定读、事务 DDL 提交/回滚、受支持的主键重命名与 STRING→INT 迁移、结构化视图去重、列元数据、存储过程输出参数及多结果、空首结果、measurement 分组/HAVING/排序/分页的同步与异步入口。DATETIMEOFFSET 按 SonnetDB 合同返回零 offset；主键类型迁移也不承诺任意类型间转换。

每个阶段的 JSON 记录业务检查及实际加载程序集的 informational version 和 SHA-256，根报告绑定 Server/消费者的 SHA-256 与协议收据。终止 Server 后重开不等于物理掉电、全部模型恢复、生产负载或安装器实机验收。完整 237 项 Provider 套件和本程序的检查数量分别记录，不相加为覆盖率。

## 2026-11-06 纯包组合实测

测试源码为 `85388bae4b9a21a603588c7e5faa4435e1f1c790`。本地 FreeSql Core/Provider 使用独立候选版本 `3.5.311-sonnetdb230.20261106.1`，正式 SonnetDB 三包与公开 Server 均为 `5.0.0+84433f9e` 对应载荷。完整 Provider 237/237、三协议各30业务+9重开全部通过；真实 Server 日志为 REST SQL81、HTTP/2 Frame45、HTTP/2事务21。新增空/空白数据库 selector、NULL 往返、预取消和后续连接可用断言；运行中取消/断网不由预取消结果推断。

复验纯包组合时，把本目录的 `Program.cs` 原字节复制到一个独立目录，用新 net10.0 控制台项目仅引用 `FreeSql`、`FreeSql.Provider.SonnetDB` 和固定 `[5.0.0]` 的 `SonnetDB`，不得保留 ProjectReference。本地候选的两包要使用独立预发布版本；恢复前核对新缓存为空，并通过 NuGet source mapping 将两包精确映射到本地 feed，其它包只映射 NuGet.org。若目录位于另一个仓库内，显式隔离其 Directory.Build/Directory.Packages 配置。

先构建所有 Core 目标框架和 Provider，再 pack --no-build；本仓库 GeneratePackageOnBuild=true，未构建全部目标框架时直接 pack 可能因缺少 net40 DLL 返回 NU5026。对最终两包核对 nuspec source commit、DLL informational version/hash，再恢复并构建独立消费者。`run.mjs` 的第三个参数可指定其 DLL：

```powershell
$env:SONNETDB_ACCEPTANCE_DOTNET = 'C:\Program Files\dotnet\dotnet.exe'
node run.mjs <public-server-executable> <new-evidence-directory> <standalone-consumer-dll>
```

本机所有 restore/build/test/format/pack/消费者运行都持有 `sonnetdb-heavy-20261106.lock` 的 FileShare.None，设置 DOTNET_PROCESSOR_COUNT=2、MSBuild -m:1 并禁用持久构建服务；使用有界执行器回收本任务的全部子进程。签名验证、三包 nuspec repository commit、缓存 `.nupkg.metadata` 来源与六个消费者实际 DLL 均单独核对。

NuGet.org 公开 FreeSql Core3.5.311 的新缓存负控仍实报 CS0117，Provider flat-container仍404；纯候选包组合通过不代表上游已合入/发包。完整身份见 [机器清单](../../Providers/FreeSql.Provider.SonnetDB/docs/sonnetdb-release-acceptance-20261106.json)。原始 stdout/TRX/native receipts 保留在本机 `D:/source/SonnetDB/artifacts/issue230-freesql-final-20261106/`；包和二进制不提交仓库。
