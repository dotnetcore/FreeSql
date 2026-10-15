# SonnetDB 兼容性与发布依赖

核验日期：2026-10-15 至 2026-10-16（北京时间）。最新源码仍未满足全部现有 FreeSql 需求，暂不发布新版本，PR 保持 Draft。
已实现的引擎能力由提供程序接入，尚缺的引擎合同复用已有任务，没有覆盖的缺口单独建任务。
Issue 关闭、源码提交、公开包和 FreeSql 回归分别记录。

## 验证对象

| 对象 | 身份 |
| --- | --- |
| 最新正式 NuGet | `SonnetDB 4.0.0` |
| 本分支公开预览依赖 | `4.5.0-preview.ef266.5`；nuspec 源码 `1af23d8f299febfdf66b040ab3419980396acf46` |
| 最新已提交 main | `c7b307f605d90e4c6638ff3c11bad06e23543c61` |
| 本地最新实现 | main 副本叠加 14 个 src 文件；摘要 `229290828390704919BF464DC829A97CCE72E51244ED93D5510EBC7A82696036` |

本地源码包含尚未提交的实现，通过隔离快照验证，不修改原工作树。
本地源码快照捕获于 2026-10-15 23:58:43（北京时间）；以下状态对应这一明确基线。
本轮验证为嵌入式执行，不代替远程 Server/客户端组合的验收。

## 已接入的现有接口

以下为本轮实现和回归范围，下文未完成的依赖仍单独跟踪：

- 原子 `InsertOrUpdate`/字典 UPSERT、identity 和既有唯一冲突键配置。
- 插入/更新/删除返回实体，`ExecuteUpdated` 额外支持既有标量/匿名投影，`InsertInto`、CTE、递归树、`UnionAll`、多行 `WithMemory`、关系表 JOIN 和 `Update.Join` 内连接更新；measurement 主源与单个关系维表 INNER JOIN。
- 字符串、整数位运算/移位、三角函数、排名窗口、字符串聚合、日期差与 duration 映射。
- DECIMAL 参数/字面量和精确基础表达式，DateOnly/TimeOnly、INT ticks TimeSpan、NULL 时间回读、数值时间投影、二进制及 VECTOR 参数。
- 关系 Serializable 冲突传播、空结果 HasRows/元数据、CodeFirst 已发布类型及迁移。
- SQL 改写按词法 token 处理，字符串中的 SELECT、SET、列名或引号保持原值。

上游 #171–#198 已关闭，但必须按各自实现范围解读。#190 的文档化拒绝不代表锁定读完成；
#193/#198 的关系 VECTOR/GEOPOINT、任意精度整数边界不扩入此次任务。

#211、#219、#220、#221、#222、#223、#225、#228、#235 以及 Decimal DDL、VALUES 表达式、
CAST 聚合、UUID、small integers、binary、LIKE 转义等实现均已进入公开预览。
不能继续标为“上游未实现”或“只有 main，尚无可安装包”。正式发布仍由 #230 跟踪。

## 本地源码通过，仍待提交与发布

| 依赖 | 现有 FreeSql 入口 | 已提交 main | 本地源码验收 |
| --- | --- | --- | --- |
| [#224](https://github.com/IoTSharp/SonnetDB/issues/224) | measurement `GroupBy/Having/OrderBy/Limit/ToList` | 未通过 | 复合聚合投影、HAVING、隐藏聚合排序、多键排序通过，真实 FreeSql 链路返回正确行 |
| [#229](https://github.com/IoTSharp/SonnetDB/issues/229) | `IAdo.Query<T1,T2>`、`ExecuteDataSet` 同步/异步 | 未通过 | 多结果消费者验证通过，保留首组空结果的列名和类型；直接 ADO 分号边界通过 |
| [#257](https://github.com/IoTSharp/SonnetDB/issues/257) | 全表关系 UPDATE 的原生执行合同 | 未通过 | 无 WHERE 的关系 UPDATE 已通过原生探针；FreeSql 安全谓词规则仍适用 |

以上不计为发布完成。需要将已验证实现提交、进入可安装产物，再运行相同消费者回归。

## 尚未满足的上游依赖

| 任务 | 现有 FreeSql 能力及最小缺口 | 最新源码结论 |
| --- | --- | --- |
| [#226](https://github.com/IoTSharp/SonnetDB/issues/226) | CodeFirst 存量主键重命名/定义迁移 | 主键列重命名仍拒绝；不能以普通列 ALTER 代表完成 |
| [#227](https://github.com/IoTSharp/SonnetDB/issues/227) | 关系 CodeFirst 事务内 DDL 提交/回滚 | 活动事务内 ALTER 仍拒绝 |
| [#233](https://github.com/IoTSharp/SonnetDB/issues/233) | 事务内 `ForUpdate()` 最小实际锁定读 | FOR UPDATE 仍未实现；ROWVERSION/乐观 Serializable 不等价 |
| [#236](https://github.com/IoTSharp/SonnetDB/issues/236) | DateTime/DateTimeOffset 精确关系持久化 | `.4567890Z` 仍读成 `.4560000Z` |
| [#312](https://github.com/IoTSharp/SonnetDB/issues/312) | `Update<T>().Set(a => a.Id, ...)` 普通主键值更新 | 原生 UPDATE PRIMARY KEY 被拒绝；与 #226 的 DDL 迁移分开验收 |
| [#313](https://github.com/IoTSharp/SonnetDB/issues/313) | measurement 普通数值/字符串 FIELD 更新 | 引擎仅允许 VECTOR FIELD 更新；不要求修改 TAG/time |
| [#232](https://github.com/IoTSharp/SonnetDB/issues/232) | 表名 schema/namespace 映射 | 复用现有任务，不能静默去掉 schema |
| [#237](https://github.com/IoTSharp/SonnetDB/issues/237) | CodeFirst/DbFirst 表与列注释 | 复用 #237；#239 已按重复关闭，关闭不代表实现完成 |
| [#240](https://github.com/IoTSharp/SonnetDB/issues/240) | `Column.StringLength` 有界字符串约束 | 复用现有任务，需要持久化和元数据合同 |
| [#250](https://github.com/IoTSharp/SonnetDB/issues/250) | `IndexAttribute` 中 DESC 列方向 | 仅依赖索引方向部分，不引入该任务其他 ORM 需求 |
| [#255](https://github.com/IoTSharp/SonnetDB/issues/255) | 无显式主键关系实体写入 | 复用已有任务，不重复创建 #253 的重叠范围 |
| [#247](https://github.com/IoTSharp/SonnetDB/issues/247) | `IAdo` 已有 StoredProcedure/输出参数 | 复用现有 ADO 任务，不增加 FreeSql API |

[#314](https://github.com/IoTSharp/SonnetDB/issues/314) 跟踪 DECIMAL 舍入：`round` 仍经过 double，不能满足现有 `Math.Round(decimal)` 的精确语义。
基础 Abs/Floor/Ceiling/Truncate 已由提供程序采用精确等价表达式，Round 保留明确诊断。
精确输入、实际输出和最小验收条件见 #314。

## 发布与合并条件

- [ ] 完成适用上游依赖，并提交已经通过本地验证的 #224/#229 实现。
- [ ] [#230](https://github.com/IoTSharp/SonnetDB/issues/230) 发布可对应源码的正式 NuGet、tag、发行说明及适用 Server 产物。
- [ ] FreeSql 升级正式依赖，重新验证完整测试与相关远程合同。
- [ ] 各依赖具备数据库执行、FreeSql 消费者和包身份的对应证据后解除 Draft。

本轮不增加 FreeSql 公共接口。核心改动仅在既有 `UpdateJoin` 和 `ForUpdate` 分支中接入
SonnetDB：前者复用已支持语法，后者在引擎实现前准确拒绝，避免静默降级普通查询。
不要求公开 savepoint、原生 COPY/Frame、PostgreSQL 特有类型或 SonnetDB 全部模型 ORM 化。