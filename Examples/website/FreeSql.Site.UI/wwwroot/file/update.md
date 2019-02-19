# 更新数据

| 方法 | 返回值 | 参数 | 描述 |
| - | - | - | - |
| SetSource | \<this\> | T1 \| IEnumerable<T1> | 更新数据，设置更新的实体 |
| IgnoreColumns | \<this\> | Lambda | 忽略的列 |
| Set | \<this\> | Lambda, value | 设置列的新值，Set(a => a.Name, "newvalue") |
| Set | \<this\> | Lambda | 设置列的的新值为基础上增加，Set(a => a.Clicks + 1)，相当于 clicks=clicks+1; |
| SetRaw | \<this\> | string, parms | 设置值，自定义SQL语法，SetRaw("title = ?title", new { title = "newtitle" }) |
| Where | \<this\> | Lambda | 表达式条件，仅支持实体基础成员（不包含导航对象） |
| Where | \<this\> | string, parms | 原生sql语法条件，Where("id = ?id", new { id = 1 }) |
| Where | \<this\> | T1 \| IEnumerable<T1> | 传入实体或集合，将其主键作为条件 |
| WhereExists | \<this\> | ISelect | 子查询是否存在 |
| ToSql | string | | 返回即将执行的SQL语句 |
| ExecuteAffrows | long | | 执行SQL语句，返回影响的行数 |
| ExecuteUpdated | List\<T1\> | | 执行SQL语句，返回更新后的记录 |

### 列优先级

> 全部列 < 指定列(Set/SetRaw) < 忽略列(IgnoreColumns)

### 测试代码

```csharp
[Table(Name = "tb_topic")]
class Topic {
	[Column(IsIdentity = true, IsPrimary = true)]
	public int Id { get; set; }
	public int Clicks { get; set; }
	public TestTypeInfo Type { get; set; }
	public string Title { get; set; }
	public DateTime CreateTime { get; set; }
}

IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=10")
    .Build();
IUpdate<Topic> update => fsql.Update<Topic>();
```

### 动态条件
```csharp
Update<Topic>(object dywhere)
```
dywhere 支持

* 主键值
* new[] { 主键值1, 主键值2 }
* Topic对象
* new[] { Topic对象1, Topic对象2 }
* new { id = 1 }

### 更新指定列
```csharp
var t1 = fsql.Update<Topic>(1).Set(a => a.CreateTime, DateTime.Now).ToSql();
//UPDATE `tb_topic` SET `CreateTime` = '2018-12-08 00:04:59' WHERE (`Id` = 1)
```

### 更新指定列，累加
```csharp
var t2 = fsql.Update<Topic>(1).Set(a => a.Clicks + 1).ToSql();
//UPDATE `tb_topic` SET `Clicks` = ifnull(`Clicks`,0) + 1 WHERE (`Id` = 1)
```

### 保存实体
```csharp
var item = new Topic { Id = 1, Title = "newtitle" };
var t3 = update.SetSource(item).ToSql();
//UPDATE `tb_topic` SET `Clicks` = ?p_0, `Title` = ?p_1, `CreateTime` = ?p_2 WHERE (`Id` = 1)
```

### 保存实体，忽略一些列
```csharp
var t4 = update.SetSource(item).IgnoreColumns(a => a.Clicks).ToSql();
//UPDATE `tb_topic` SET `Title` = ?p_0, `CreateTime` = ?p_1 WHERE (`Id` = 1)
var t5 = update.SetSource(item).IgnoreColumns(a => new { a.Clicks, a.CreateTime }).ToSql();
//UPDATE `tb_topic` SET `Title` = ?p_0 WHERE (`Id` = 1)
```

### 批量保存
```csharp
var items = new List<Topic>();
for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

var t6 = update.SetSource(items).ToSql();
//UPDATE `tb_topic` SET `Clicks` = CASE `Id` WHEN 1 THEN ?p_0 WHEN 2 THEN ?p_1 WHEN 3 THEN ?p_2 WHEN 4 THEN ?p_3 WHEN 5 THEN ?p_4 WHEN 6 THEN ?p_5 WHEN 7 THEN ?p_6 WHEN 8 THEN ?p_7 WHEN 9 THEN ?p_8 WHEN 10 THEN ?p_9 END, `Title` = CASE `Id` WHEN 1 THEN ?p_10 WHEN 2 THEN ?p_11 WHEN 3 THEN ?p_12 WHEN 4 THEN ?p_13 WHEN 5 THEN ?p_14 WHEN 6 THEN ?p_15 WHEN 7 THEN ?p_16 WHEN 8 THEN ?p_17 WHEN 9 THEN ?p_18 WHEN 10 THEN ?p_19 END, `CreateTime` = CASE `Id` WHEN 1 THEN ?p_20 WHEN 2 THEN ?p_21 WHEN 3 THEN ?p_22 WHEN 4 THEN ?p_23 WHEN 5 THEN ?p_24 WHEN 6 THEN ?p_25 WHEN 7 THEN ?p_26 WHEN 8 THEN ?p_27 WHEN 9 THEN ?p_28 WHEN 10 THEN ?p_29 END WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))
```

> 批量保存的场景，先查询20条记录，根据本地很复杂的规则把集合的值改完后

> 传统做法是循环20次保存，用 case when 只要一次就行

### 批量保存，忽略一些列
```csharp
var t7 = update.SetSource(items).IgnoreColumns(a => new { a.Clicks, a.CreateTime }).ToSql();
//UPDATE `tb_topic` SET `Title` = CASE `Id` WHEN 1 THEN ?p_0 WHEN 2 THEN ?p_1 WHEN 3 THEN ?p_2 WHEN 4 THEN ?p_3 WHEN 5 THEN ?p_4 WHEN 6 THEN ?p_5 WHEN 7 THEN ?p_6 WHEN 8 THEN ?p_7 WHEN 9 THEN ?p_8 WHEN 10 THEN ?p_9 END WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))
```

### 批量更新指定列
```csharp
var t8 = update.SetSource(items).Set(a => a.CreateTime, DateTime.Now).ToSql();
//UPDATE `tb_topic` SET `CreateTime` = ?p_0 WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))
```

> 指定列更新后，批量保存将失效

### 更新条件

> 除了顶上介绍的 dywhere 构造参数外，还支持 Where lambda/sql 方法

```csharp
var t9 = update.Set(a => a.Title, "新标题").Where(a => a.Id == 1).ToSql();
//UPDATE `tb_topic` SET `Title` = '新标题' WHERE (Id = 1)
```

### 自定义SQL

```csharp
var t10 = update.SetRaw("Title = {0}", "新标题").Where("Id = {0}", 1).ToSql();
//UPDATE `tb_topic` SET Title = '新标题' WHERE (Id = 1)
//sql语法条件，参数使用 {0}，与 string.Format 保持一致，无须加单引号，错误的用法：'{0}'
```

### 执行命令

| 方法 | 返回值 | 参数 | 描述 |
| - | - | - | - |
| ExecuteAffrows | long | | 执行SQL语句，返回影响的行数 |
| ExecuteUpdated | List\<T1\> | | 执行SQL语句，返回更新后的记录 |