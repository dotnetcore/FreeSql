# 删除数据

| 方法 | 返回值 | 参数 | 描述 |
| - | - | - | - |
| Where | \<this\> | Lambda | 表达式条件，仅支持实体基础成员（不包含导航对象） |
| Where | \<this\> | string, parms | 原生sql语法条件，Where("id = ?id", new { id = 1 }) |
| Where | \<this\> | T1 \| IEnumerable<T1> | 传入实体或集合，将其主键作为条件 |
| WhereExists | \<this\> | ISelect | 子查询是否存在 |
| ToSql | string | | 返回即将执行的SQL语句 |
| ExecuteAffrows | long | | 执行SQL语句，返回影响的行数 |
| ExecuteDeleted | List\<T1\> | | 执行SQL语句，返回被删除的记录 |

### 测试代码

```csharp
IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=10")
    .Build();
IDelete<Topic> delete => fsql.Delete<Topic>();

[Table(Name = "tb_topic")]
class Topic {
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }
    public int Clicks { get; set; }
    public TestTypeInfo Type { get; set; }
    public string Title { get; set; }
    public DateTime CreateTime { get; set; }
}
```

### 动态条件
```csharp
Delete<Topic>(object dywhere)
```
dywhere 支持

* 主键值
* new[] { 主键值1, 主键值2 }
* Topic对象
* new[] { Topic对象1, Topic对象2 }
* new { id = 1 }

```csharp
var t1 = fsql.Delete<Topic>(new[] { 1, 2 }).ToSql();
//DELETE FROM `tb_topic` WHERE (`Id` = 1 OR `Id` = 2)

var t2 = fsql.Delete<Topic>(new Topic { Id = 1, Title = "test" }).ToSql();
//DELETE FROM `tb_topic` WHERE (`Id` = 1)

var t3 = fsql.Delete<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).ToSql();
//DELETE FROM `tb_topic` WHERE (`Id` = 1 OR `Id` = 2)

var t4 = fsql.Delete<Topic>(new { id = 1 }).ToSql();
//DELETE FROM `tb_topic` WHERE (`Id` = 1)
```

### 删除条件

```csharp
var t5 = delete.Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
//DELETE FROM `tb_topic` WHERE (`Id` = 1)

var t6 = delete.Where("id = ?id", new { id = 1 }).ToSql().Replace("\r\n", "");
//DELETE FROM `tb_topic` WHERE (id = ?id)

var item = new Topic { Id = 1, Title = "newtitle" };
var t7 = delete.Where(item).ToSql().Replace("\r\n", "");
//DELETE FROM `tb_topic` WHERE (`Id` = 1)

var items = new List<Topic>();
for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
var t8 = delete.Where(items).ToSql().Replace("\r\n", "");
//DELETE FROM `tb_topic` WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))
```

### 执行命令

| 方法 | 返回值 | 参数 | 描述 |
| - | - | - | - |
| ExecuteAffrows | long | | 执行SQL语句，返回影响的行数 |
| ExecuteDeleted | List\<T1\> | | 执行SQL语句，返回被删除的记录 |