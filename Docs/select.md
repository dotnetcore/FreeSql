# 查询数据

| 方法 | 返回值 | 参数 | 描述 |
| ------------- | - | - | - |
| ToSql | string | | 返回即将执行的SQL语句 |
| ToList | List<T1> | | 执行SQL查询，返回 T1 实体所有字段的记录，若存在导航属性则一起查询返回，记录不存在时返回 Count 为 0 的列表 |
| ToList\<T\> | List\<T\> | Lambda | 执行SQL查询，返回指定字段的记录，记录不存在时返回 Count 为 0 的列表 |
| ToList\<T\> | List\<T\> | string field | 执行SQL查询，返回 field 指定字段的记录，并以元组或基础类型(int,string,long)接收，记录不存在时返回 Count 为 0 的列表 |
| ToOne | T1 | | 执行SQL查询，返回 T1 实体所有字段的第一条记录，记录不存在时返回 null |
| Any | bool | | 执行SQL查询，是否有记录 |
| Sum | T | Lambda | 指定一个列求和 |
| Min | T | Lambda | 指定一个列求最小值 |
| Max | T | Lambda | 指定一个列求最大值 |
| Avg | T | Lambda | 指定一个列求平均值 |
| 【分页】 |
| Count | long | | 查询的记录数量 |
| Count | \<this\> | out long | 查询的记录数量，以参数out形式返回 |
| Skip | \<this\> | int offset | 查询向后偏移行数 |
| Offset | \<this\> | int offset | 查询向后偏移行数 |
| Limit | \<this\> | int limit | 查询多少条数据 |
| Take | \<this\> | int limit | 查询多少条数据 |
| Page | \<this\> | int pageIndex, int pageSize | 分页 |
| 【条件】 |
| Where | \<this\> | Lambda | 支持多表查询表达式 |
| WhereIf | \<this\> | bool, Lambda | 支持多表查询表达式 |
| Where | \<this\> | string, parms | 原生sql语法条件，Where("id = ?id", new { id = 1 }) |
| WhereIf | \<this\> | bool, string, parms | 原生sql语法条件，WhereIf(true, "id = ?id", new { id = 1 }) |
| WhereLike | \<this\> | Lambda, string, bool | like 查询条件，where title like '%xxx%' or content like '%xxx%' |
| 【分组】 |
| GroupBy | \<this\> | Lambda | 按选择的列分组，GroupBy(a => a.Name) | GroupBy(a => new{a.Name,a.Time}) | GroupBy(a => new[]{"name","time"}) |
| GroupBy | \<this\> | string, parms | 按原生sql语法分组，GroupBy("concat(name, ?cc)", new { cc = 1 }) |
| Having | \<this\> | string, parms | 按原生sql语法聚合条件过滤，Having("count(name) = ?cc", new { cc = 1 }) |
| 【排序】 |
| OrderBy | \<this\> | Lambda | 按列排序，OrderBy(a => a.Time) |
| OrderByDescending | \<this\> | Lambda | 按列倒向排序，OrderByDescending(a => a.Time) |
| OrderBy | \<this\> | string, parms | 按原生sql语法排序，OrderBy("count(name) + ?cc", new { cc = 1 }) |
| 【联表】 |
| LeftJoin | \<this\> | Lambda | 左联查询，可使用导航属性，或指定关联的实体类型 |
| InnerJoin | \<this\> | Lambda | 联接查询，可使用导航属性，或指定关联的实体类型 |
| RightJoin | \<this\> | Lambda | 右联查询，可使用导航属性，或指定关联的实体类型 |
| LeftJoin | \<this\> | string, parms | 左联查询，使用原生sql语法，LeftJoin("type b on b.id = a.id and b.clicks > ?clicks", new { clicks = 1 }) |
| InnerJoin | \<this\> | string, parms | 联接查询，使用原生sql语法，InnerJoin("type b on b.id = a.id and b.clicks > ?clicks", new { clicks = 1 }) |
| RightJoin | \<this\> | string, parms | 右联查询，使用原生sql语法，RightJoin("type b on b.id = a.id and b.clicks > ?clicks", new { clicks = 1 }) |
| From | \<this\> | Lambda | 多表查询，3个表以上使用非常方便，目前设计最大支持10个表 |
| 【其他】 |
| As | \<this\> | string alias = "a" | 指定别名 |
| Master | \<this\> | | 指定从主库查询（默认查询从库） |
| Caching | \<this\> | int seconds, string key = null | 缓存查询结果 |
