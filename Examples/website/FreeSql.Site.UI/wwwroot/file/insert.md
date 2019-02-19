# 插入数据

| 方法 | 返回值 | 参数 | 描述 |
| - | - | - | - |
| AppendData | \<this\> | T1 \| IEnumerable<T1> | 追加准备插入的实体 |
| InsertColumns | \<this\> | Lambda | 只插入的列 |
| IgnoreColumns | \<this\> | Lambda | 忽略的列 |
| ToSql | string | | 返回即将执行的SQL语句 |
| ExecuteAffrows | long | | 执行SQL语句，返回影响的行数 |
| ExecuteIdentity | long | | 执行SQL语句，返回自增值 |
| ExecuteInserted | List\<T1\> | | 执行SQL语句，返回插入后的记录 |

### 列优先级

> 全部列 < 指定列(InsertColumns) < 忽略列(IgnoreColumns)

### 测试代码

```csharp
IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=10")
    .Build();
IInsert<Topic> insert => fsql.Insert<Topic>();

[Table(Name = "tb_topic")]
class Topic {
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }
    public int Clicks { get; set; }
    public TestTypeInfo Type { get; set; }
    public string Title { get; set; }
    public DateTime CreateTime { get; set; }
}

var items = new List<Topic>();
for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
```

### 插入

```csharp
var t1 = insert.AppendData(items.First()).ToSql();
//INSERT INTO `tb_topic`(`Clicks`, `Title`, `CreateTime`) VALUES(?Clicks0, ?Title0, ?CreateTime0)
```

### 批量插入

```csharp
var t2 = insert.AppendData(items).ToSql();
//INSERT INTO `tb_topic`(`Clicks`, `Title`, `CreateTime`) VALUES(?Clicks0, ?Title0, ?CreateTime0), (?Clicks1, ?Title1, ?CreateTime1), (?Clicks2, ?Title2, ?CreateTime2), (?Clicks3, ?Title3, ?CreateTime3), (?Clicks4, ?Title4, ?CreateTime4), (?Clicks5, ?Title5, ?CreateTime5), (?Clicks6, ?Title6, ?CreateTime6), (?Clicks7, ?Title7, ?CreateTime7), (?Clicks8, ?Title8, ?CreateTime8), (?Clicks9, ?Title9, ?CreateTime9)
```

### 只想插入指定的列

```csharp
var t3 = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
//INSERT INTO `tb_topic`(`Title`) VALUES(?Title0), (?Title1), (?Title2), (?Title3), (?Title4), (?Title5), (?Title6), (?Title7), (?Title8), (?Title9)

var t4 = insert.AppendData(items).InsertColumns(a =>new { a.Title, a.Clicks }).ToSql();
//INSERT INTO `tb_topic`(`Clicks`, `Title`) VALUES(?Clicks0, ?Title0), (?Clicks1, ?Title1), (?Clicks2, ?Title2), (?Clicks3, ?Title3), (?Clicks4, ?Title4), (?Clicks5, ?Title5), (?Clicks6, ?Title6), (?Clicks7, ?Title7), (?Clicks8, ?Title8), (?Clicks9, ?Title9)
```

### 忽略列

```csharp
var t5 = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
//INSERT INTO `tb_topic`(`Clicks`, `Title`) VALUES(?Clicks0, ?Title0), (?Clicks1, ?Title1), (?Clicks2, ?Title2), (?Clicks3, ?Title3), (?Clicks4, ?Title4), (?Clicks5, ?Title5), (?Clicks6, ?Title6), (?Clicks7, ?Title7), (?Clicks8, ?Title8), (?Clicks9, ?Title9)

var t6 = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).ToSql();
///INSERT INTO `tb_topic`(`Clicks`) VALUES(?Clicks0), (?Clicks1), (?Clicks2), (?Clicks3), (?Clicks4), (?Clicks5), (?Clicks6), (?Clicks7), (?Clicks8), (?Clicks9)
```

### 执行命令

| 方法 | 返回值 | 描述 |
| - | - | - |
| ExecuteAffrows | long | 执行SQL语句，返回影响的行数 |
| ExecuteIdentity | long | 执行SQL语句，返回自增值 |
| ExecuteInserted | List\<T1\> | 执行SQL语句，返回插入后的记录 |
