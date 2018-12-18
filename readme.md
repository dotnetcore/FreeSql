# FreeSql

* [Insert 插入数据](Docs/insert.md)
* [Update 更新数据](Docs/update.md)
* [Delete 删除数据](Docs/delete.md)
* [Select 查询数据](Docs/select.md)

* [CodeFirst 快速开发](Docs/codefirst.md)
* [DbFirst 快速开发](Docs/dbfirst.md)
* [DbFirst 生成器](Docs/generator.md)

# 查询数据

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

# 更多文档整理中。。。