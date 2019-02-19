# 查询数据

## 测试代码

```csharp
IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=10")
    .Build();
ISelect<Topic> select => fsql.Select<Topic>();

[Table(Name = "tb_topic")]
class Topic {
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }
    public int Clicks { get; set; }
    public int TestTypeInfoGuid { get; set; }
    public TestTypeInfo Type { get; set; }
    public string Title { get; set; }
    public DateTime CreateTime { get; set; }
}
class TestTypeInfo {
    public int Guid { get; set; }
    public int ParentId { get; set; }
    public TestTypeParentInfo Parent { get; set; }
    public string Name { get; set; }
}
class TestTypeParentInfo {
    public int Id { get; set; }
    public string Name { get; set; }

    public List<TestTypeInfo> Types { get; set; }
}
```

# Where

### 表达式函数支持

#### String 对象方法
StartsWith, EndsWith, Contains, ToLower, ToUpper, Substring, Length, IndexOf, PadLeft, PadRight, Trim, TrimStart, TrimEnd, Replace, CompareTo

#### Math 方法
...

### 单表
```csharp
var sql = select.Where(a => a.Id == 10).ToSql();
///SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, a.`Title` as4, a.`CreateTime` as5 FROM `tb_topic` a WHERE (a.`Id` = 10)

sql = select.Where(a => a.Id == 10 && a.Id > 10 || a.Clicks > 100).ToSql();
///SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, a.`Title` as4, a.`CreateTime` as5 FROM `tb_topic` a WHERE (a.`Id` = 10 AND a.`Id` > 10 OR a.`Clicks` > 100)
```

### 多表，使用导航属性
```csharp
sql = select.Where(a => a.Type.Name == "typeTitle" && a.Type.Guid == a.TestTypeInfoGuid).ToSql();
///SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a.`Title` as7, a.`CreateTime` as8 FROM `tb_topic` a, `TestTypeInfo` a__Type WHERE (a__Type.`Name` = 'typeTitle' AND a__Type.`Guid` = a.`TestTypeInfoGuid`)

sql = select.Where(a => a.Type.Parent.Name == "tparent").ToSql();
//SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a.`Title` as7, a.`CreateTime` as8 FROM `tb_topic` a, `TestTypeInfo` a__Type, `TestTypeParentInfo` a__Type__Parent WHERE (a__Type__Parent.`Name` = 'tparent')
```

### 多表，没有导航属性
```csharp
sql = select.Where<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid && b.Name == "typeTitle").ToSql();
//SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, b.`Guid` as4, b.`ParentId` as5, b.`Name` as6, a.`Title` as7, a.`CreateTime` as8 FROM `tb_topic` a, `TestTypeInfo` b WHERE (b.`Guid` = a.`TestTypeInfoGuid` AND b.`Name` = 'typeTitle')

sql = select.Where<TestTypeInfo, TestTypeParentInfo>((a, b, c) => c.Name == "tparent").ToSql();
//SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, a.`Title` as4, a.`CreateTime` as5 FROM `tb_topic` a, `TestTypeParentInfo` c WHERE (c.`Name` = 'tparent')
```

### 多表，任意查
```csharp
sql = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
    .Where(a => a.Id == 10 && c.Name == "xxx")
    .Where(a => b.ParentId == 20)).ToSql();
//SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, b.`Guid` as4, b.`ParentId` as5, b.`Name` as6, a.`Title` as7, a.`CreateTime` as8 FROM `tb_topic` a, `TestTypeParentInfo` c, `TestTypeInfo` b WHERE (a.`Id` = 10 AND c.`Name` = 'xxx') AND (b.`ParentId` = 20)
```

### 原生SQL
```csharp
sql = select.Where("a.clicks > 100 && a.id = ?id", new { id = 10 }).ToSql();
//SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, a.`Title` as4, a.`CreateTime` as5 FROM `tb_topic` a WHERE (a.clicks > 100 && a.id = ?id)
```

> 以上条件查询，支持 WhereIf

# 联表

### 使用导航属性联表
```csharp
sql = select.LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid).ToSql();
//SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a.`Title` as7, a.`CreateTime` as8 FROM `tb_topic` a LEFT JOIN `TestTypeInfo` a__Type ON a__Type.`Guid` = a.`TestTypeInfoGuid`

sql = select
    .LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid)
    .LeftJoin(a => a.Type.Parent.Id == a.Type.ParentId).ToSql();
//SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a.`Title` as7, a.`CreateTime` as8 FROM `tb_topic` a LEFT JOIN `TestTypeInfo` a__Type ON a__Type.`Guid` = a.`TestTypeInfoGuid` LEFT JOIN `TestTypeParentInfo` a__Type__Parent ON a__Type__Parent.`Id` = a__Type.`ParentId`
```

### 没有导航属性联表
```csharp
sql = select.LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid).ToSql();
//SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, b.`Guid` as4, b.`ParentId` as5, b.`Name` as6, a.`Title` as7, a.`CreateTime` as8 FROM `tb_topic` a LEFT JOIN `TestTypeInfo` b ON b.`Guid` = a.`TestTypeInfoGuid`

sql = select
    .LeftJoin<TestTypeInfo>((a, b) => b.Guid == a.TestTypeInfoGuid)
    .LeftJoin<TestTypeParentInfo>((a, c) => c.Id == a.Type.ParentId).ToSql();
//SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, b.`Guid` as4, b.`ParentId` as5, b.`Name` as6, a.`Title` as7, a.`CreateTime` as8 FROM `tb_topic` a LEFT JOIN `TestTypeInfo` b ON b.`Guid` = a.`TestTypeInfoGuid` LEFT JOIN `TestTypeParentInfo` c ON c.`Id` = b.`ParentId`
```

### 联表任意查
```csharp
sql = select.From<TestTypeInfo, TestTypeParentInfo>((s, b, c) => s
    .LeftJoin(a => a.TestTypeInfoGuid == b.Guid)
    .LeftJoin(a => b.ParentId == c.Id)).ToSql();
//SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, b.`Guid` as4, b.`ParentId` as5, b.`Name` as6, a.`Title` as7, a.`CreateTime` as8 FROM `tb_topic` a LEFT JOIN `TestTypeInfo` b ON a.`TestTypeInfoGuid` = b.`Guid` LEFT JOIN `TestTypeParentInfo` c ON b.`ParentId` = c.`Id`
```

### 原生SQL联表
```csharp
sql = select.LeftJoin("TestTypeInfo b on b.Guid = a.TestTypeInfoGuid and b.Name = ?bname", new { bname = "xxx" }).ToSql();
//SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, a.`Title` as4, a.`CreateTime` as5 FROM `tb_topic` a LEFT JOIN TestTypeInfo b on b.Guid = a.TestTypeInfoGuid and b.Name = ?bname
```

# 查询数据

### 返回 List
```csharp
List<Topic> t1 = select.Where(a => a.Id > 0).Skip(100).Limit(200).ToList();
```

### 返回 List + 导航属性的数据
```csharp
List<Topic> t2 = select.LeftJoin(a => a.Type.Guid == a.TestTypeInfoGuid).ToList();
//此时会返回普通字段 + 导航对象 Type 的数据
```

### 指定字段返回
```csharp
//返回一个字段
List<int> t3 = select.Where(a => a.Id > 0).Skip(100).Limit(200).ToList(a => a.Id);

//返回匿名类
List<匿名类> t4 = select.Where(a => a.Id > 0).Skip(100).Limit(200).ToList(a => new { a.Id, a.Title });

//返回元组
List<(int, string)> t5 = select.Where(a => a.Id > 0).Skip(100).Limit(200).ToList<(int, string)>("id, title");
```

### 执行SQL返回数据
```csharp
class xxx {
    public int Id { get; set; }
    public string Path { get; set; }
    public string Title2 { get; set; }
}

List<xxx> t6 = fsql.Ado.Query<xxx>("select * from song");
List<(int, string ,string)> t7 = fsql.Ado.Query<(int, string, string)>("select * from song");
List<dynamic> t8 = fsql.Ado.Query<dynamic>("select * from song");
```

# 更多文档整理中。。。

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