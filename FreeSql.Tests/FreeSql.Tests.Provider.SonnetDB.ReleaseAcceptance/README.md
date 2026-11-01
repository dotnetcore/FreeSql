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
