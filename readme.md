FreeSql是一个功能强大的NETStandard库，用于对象关系映射程序(O/RM)，以便于开发人员能够使用 .NETStandard 对象来处理数据库，不必经常编写大部分数据访问代码。

| Package Name |  NuGet | Downloads | |
|--------------|  ------- |  ---- | -- |
| FreeSql | [![nuget](https://img.shields.io/nuget/v/FreeSql.svg?style=flat-square)](https://www.nuget.org/packages/FreeSql) | [![stats](https://img.shields.io/nuget/dt/FreeSql.svg?style=flat-square)](https://www.nuget.org/stats/packages/FreeSql?groupby=Version) |
| FreeSql.Repository | [![nuget](https://img.shields.io/nuget/v/FreeSql.Repository.svg?style=flat-square)](https://www.nuget.org/packages/FreeSql.Repository) | [![stats](https://img.shields.io/nuget/dt/FreeSql.Repository.svg?style=flat-square)](https://www.nuget.org/stats/packages/FreeSql.Repository?groupby=Version) | [Wiki](https://github.com/2881099/FreeSql/wiki/Repository) |

## 特性

- [x] 支持 CodeFirst 迁移；
- [x] 支持 DbFirst 从数据库导入实体类，支持三种模板生成器；
- [x] 采用 ExpressionTree 高性能读取数据；
- [x] 支持深入的类型映射，比如pgsql的数组类型，堪称匠心制作；
- [x] 支持丰富的表达式函数；
- [x] 支持导航属性查询，和延时加载；
- [x] 支持同步/异步数据库操作方法，丰富多彩的链式查询方法；
- [x] 支持读写分离、分表分库；
- [x] 支持多种数据库，MySql/SqlServer/PostgreSQL/Oracle/Sqlite；

[《Select查询数据文档》](https://github.com/2881099/FreeSql/wiki/%e6%9f%a5%e8%af%a2) | [《Update更新数据文档》](https://github.com/2881099/FreeSql/wiki/%e4%bf%ae%e6%94%b9) | [《Insert插入数据文档》](https://github.com/2881099/FreeSql/wiki/%e6%b7%bb%e5%8a%a0) | [《Delete删除数据文档》](https://github.com/2881099/FreeSql/wiki/%e5%88%a0%e9%99%a4)

[《Expression 表达式函数文档》](https://github.com/2881099/FreeSql/wiki/%e8%a1%a8%e8%be%be%e5%bc%8f%e5%87%bd%e6%95%b0) | [《CodeFirst 快速开发文档》](https://github.com/2881099/FreeSql/wiki/CodeFirst) | [《DbFirst 快速开发文档》](https://github.com/2881099/FreeSql/wiki/DbFirst) | [更新日志](https://github.com/2881099/FreeSql/wiki/%e6%9b%b4%e6%96%b0%e6%97%a5%e5%bf%97)

## 快速开始
```csharp
var connstr = "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;" + 
    "Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=10";

IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.MySql, connstr)
    .UseSlave("connectionString1", "connectionString2") //使用从数据库，支持多个

    .UseMonitorCommand(
        cmd => Console.WriteLine(cmd.CommandText), //监听SQL命令对象，在执行前
        (cmd, traceLog) => Console.WriteLine(traceLog)) //监听SQL命令对象，在执行后

    .UseLogger(null) //使用日志，不指定默认输出控制台 ILogger
    .UseCache(null) //使用缓存，不指定默认使用内存 IDistributedCache

    .UseAutoSyncStructure(true) //自动同步实体结构到数据库
    .UseSyncStructureToLower(true) //转小写同步结构
    .UseSyncStructureToUpper(true) //转大写同步结构

    .UseLazyLoading(true) //延时加载导航属性对象，导航属性需要声明 virtual
    .Build();
```

# 实体

FreeSql 使用模型执行数据访问，模型由实体类表示数据库表或视图，用于查询和保存数据。 有关详细信息，请参阅创建模型。

可从现有数据库生成实体模型，提供 IDbFirst 生成实体模型。

或者手动创建模型，基于模型创建或修改数据库，提供 ICodeFirst 同步结构的 API（甚至可以做到开发阶段自动同步）。

```csharp
[Table(Name = "tb_topic")]
class Topic {
    [Column(IsIdentity = true)]
    public int Id { get; set; }
    public int Clicks { get; set; }
    public string Title { get; set; }
    public DateTime CreateTime { get; set; }

    public int TypeId { get; set; }
    public TopicType Type { get; set; } //导航属性
}
class TopicType {
    [Column(IsIdentity = true)]
    public int Id { get; set; }
    public string Name { get; set; }

    public int ClassId { get; set; }
    public TopicTypeClass Class { get; set; } //导航属性
}
class TopicTypeClass {
    public int Id { get; set; }
    public string Name { get; set; }
}
```

# Part1 查询
```csharp
List<Topic> t1 = fsql.Select<Topic>().Where(a => a.Id > 0).ToList();

//返回普通字段 + 导航对象 Type 的数据
List<Topic> t2 = fsql.Select<Topic>().LeftJoin(a => a.Type.Id == a.TypeId).ToList();

//返回一个字段
List<int> t3 = fsql.Select<Topic>().Where(a => a.Id > 0).ToList(a => a.Id);

//返回匿名类型
List<匿名类型> t4 = fsql.Select<Topic>().Where(a => a.Id > 0).ToList(a => new { a.Id, a.Title });

//返回元组
List<(int, string)> t5 = fsql.Select<Topic>().Where(a => a.Id > 0).ToList<(int, string)>("id, title");

//返回SQL字段
List<匿名类> t4 = select.Where(a => a.Id > 0).Skip(100).Limit(200)
    .ToList(a => new {
        a.Id, a.Title,
        cstitle = "substr(a.title, 0, 2)", //将 substr(a.title, 0, 2) 作为查询字段
        csnow = Convert.ToDateTime("now()"), //将 now() 作为查询字段
        //奇思妙想：怎么查询开窗函数的结果
    });
```
### 联表之一：使用导航属性
```csharp
sql = fsql.Select<Topic>()
    .LeftJoin(a => a.Type.Id == a.TypeId)
    .ToSql();

sql = fsql.Select<Topic>()
    .LeftJoin(a => a.Type.Id == a.TypeId)
    .LeftJoin(a => a.Type.Class.Id == a.Type.ClassId)
    .ToSql();
```
### 联表之二：无导航属性
```csharp
sql = fsql.Select<Topic>()
    .LeftJoin<TopicType>((a, b) => b.Id == a.TypeId)
    .ToSql();

sql = fsql.Select<Topic>()
    .LeftJoin<TopicType>((a, b) => b.Id == a.TypeId)
    .LeftJoin<TopicTypeClass>((a, c) => c.Id == a.Type.ClassId)
    .ToSql();
```
### 联表之三：b, c 条件怎么设？试试这种！
```csharp
sql = fsql.Select<Topic>()
    .From<TopicType, TopicTypeClass>((s, b, c) => s
    .LeftJoin(a => a.TypeId == b.Id)
    .LeftJoin(a => b.ClassId == c.Id))
    .ToSql();
```
### 联表之四：原生SQL联表
```csharp
sql = fsql.Select<Topic>()
    .LeftJoin("TopicType b on b.Id = a.TypeId and b.Name = ?bname", new { bname = "xxx" })
    .ToSql();
```
### 分组聚合
```csharp
var groupby = fsql.Select<Topic>()
    .GroupBy(a => new { tt2 = a.Title.Substring(0, 2), mod4 = a.Id % 4 })
    .Having(a => a.Count() > 0 && a.Avg(a.Key.mod4) > 0 && a.Max(a.Key.mod4) > 0)
    .Having(a => a.Count() < 300 || a.Avg(a.Key.mod4) < 100)
    .OrderBy(a => a.Key.tt2)
    .OrderByDescending(a => a.Count())
    .ToList(a => new { a.Key.tt2, cou1 = a.Count(), arg1 = a.Avg(a.Key.mod4) });
//SELECT substr(a.`Title`, 1, 2) as1, count(1) as2, avg((a.`Id` % 4)) as3 
//FROM `xxx` a 
//GROUP BY substr(a.`Title`, 1, 2), (a.`Id` % 4) 
//HAVING (count(1) > 0 AND avg((a.`Id` % 4)) > 0 AND max((a.`Id` % 4)) > 0) AND (count(1) < 300 OR avg((a.`Id` % 4)) < 100) 
//ORDER BY substr(a.`Title`, 1, 2), count(1) DESC
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
> 更多资料：[《Select查询数据》](https://github.com/2881099/FreeSql/wiki/%e6%9f%a5%e8%af%a2)

## 性能测试

### FreeSql Query & Dapper Query

Elapsed: 00:00:00.6733199; Query Entity Counts: 131072; ORM: Dapper

Elapsed: 00:00:00.4554230; Query Tuple Counts: 131072; ORM: Dapper

Elapsed: 00:00:00.6846146; Query Dynamic Counts: 131072; ORM: Dapper

Elapsed: 00:00:00.6818111; Query Entity Counts: 131072; ORM: FreeSql*

Elapsed: 00:00:00.6060042; Query Tuple Counts: 131072; ORM: FreeSql*

Elapsed: 00:00:00.4211323; Query ToList<Tuple> Counts: 131072; ORM: FreeSql*

Elapsed: 00:00:01.0236285; Query Dynamic Counts: 131072; ORM: FreeSql*

### FreeSql ToList & Dapper Query

Elapsed: 00:00:00.6707125; ToList Entity Counts: 131072; ORM: FreeSql*

Elapsed: 00:00:00.6495301; Query Entity Counts: 131072; ORM: Dapper

[查看测试代码](FreeSql.Tests.PerformanceTests/MySqlAdoTest.cs)

> 测试方法：运行两次，以第二次性能报告，避免了首个运行慢不公平的情况。

FreeSql 目前使用的ExpressionTree+缓存，因为支持更为复杂的数据类型，所以比 Dapper Emit 慢少许。

# Part2 添加
```csharp
var items = new List<Topic>();
for (var a = 0; a < 10; a++)
    items.Add(new Topic { Title = $"newtitle{a}", Clicks = a * 100 });

var t1 = fsql.Insert<Topic>().AppendData(items.First()).ToSql();

//批量插入
var t2 = fsql.Insert<Topic>().AppendData(items).ToSql();

//插入指定的列
var t3 = fsql.Insert<Topic>().AppendData(items)
    .InsertColumns(a => a.Title).ToSql();

var t4 = fsql.Insert<Topic>().AppendData(items)
    .InsertColumns(a => new { a.Title, a.Clicks }).ToSql();

//忽略列
var t5 = fsql.Insert<Topic>().AppendData(items)
    .IgnoreColumns(a => a.CreateTime).ToSql();

var t6 = fsql.Insert<Topic>().AppendData(items)
    .IgnoreColumns(a => new { a.Title, a.CreateTime }).ToSql();
```
### 执行命令
| 方法 | 返回值 | 描述 |
| - | - | - |
| ExecuteAffrows | long | 执行SQL语句，返回影响的行数 |
| ExecuteIdentity | long | 执行SQL语句，返回自增值 |
| ExecuteInserted | List\<Topic\> | 执行SQL语句，返回插入后的记录 |
> 更多资料：[《Insert添加数据》](https://github.com/2881099/FreeSql/wiki/%e6%b7%bb%e5%8a%a0)

# Part3 修改
```csharp
var t1 = fsql.Update<Topic>(1).Set(a => a.CreateTime, DateTime.Now).ToSql();
//UPDATE `tb_topic` SET `CreateTime` = '2018-12-08 00:04:59' WHERE (`Id` = 1)

//更新指定列，累加
var t2 = fsql.Update<Topic>(1).Set(a => a.Clicks + 1).ToSql();
//UPDATE `tb_topic` SET `Clicks` = ifnull(`Clicks`,0) + 1 WHERE (`Id` = 1)

//保存实体
var item = new Topic { Id = 1, Title = "newtitle" };
var t3 = fsql.Update<Topic>().SetSource(item).ToSql();
//UPDATE `tb_topic` SET `Clicks` = ?p_0, `Title` = ?p_1, `CreateTime` = ?p_2 WHERE (`Id` = 1)

//忽略列
var t4 = fsql.Update<Topic>().SetSource(item)
    .IgnoreColumns(a => a.Clicks).ToSql();
//UPDATE `tb_topic` SET `Title` = ?p_0, `CreateTime` = ?p_1 WHERE (`Id` = 1)

var t5 = fsql.Update<Topic>().SetSource(item)
    .IgnoreColumns(a => new { a.Clicks, a.CreateTime }).ToSql();
//UPDATE `tb_topic` SET `Title` = ?p_0 WHERE (`Id` = 1)

//批量保存
```csharp
var items = new List<Topic>();
for (var a = 0; a < 10; a++)
    items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

var t6 = fsql.Update<Topic>().SetSource(items).ToSql();
//UPDATE `tb_topic` SET `Clicks` = CASE `Id` WHEN 1 THEN ?p_0 WHEN 2 THEN ?p_1 
//WHEN 3 THEN ?p_2 WHEN 4 THEN ?p_3 WHEN 5 THEN ?p_4 WHEN 6 THEN ?p_5 WHEN 7 THEN ?p_6 
//WHEN 8 THEN ?p_7 WHEN 9 THEN ?p_8 WHEN 10 THEN ?p_9 END, `Title` = CASE `Id` 
//WHEN 1 THEN ?p_10 WHEN 2 THEN ?p_11 WHEN 3 THEN ?p_12 WHEN 4 THEN ?p_13 WHEN 5 THEN ?p_14 
//WHEN 6 THEN ?p_15 WHEN 7 THEN ?p_16 WHEN 8 THEN ?p_17 WHEN 9 THEN ?p_18 WHEN 10 THEN ?p_19 END, 
//`CreateTime` = CASE `Id` WHEN 1 THEN ?p_20 WHEN 2 THEN ?p_21 WHEN 3 THEN ?p_22 WHEN 4 THEN ?p_23 
//WHEN 5 THEN ?p_24 WHEN 6 THEN ?p_25 WHEN 7 THEN ?p_26 WHEN 8 THEN ?p_27 WHEN 9 THEN ?p_28 
//WHEN 10 THEN ?p_29 END WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))

//批量保存的时候，也可以忽略一些列
var t7 = fsql.Update<Topic>().SetSource(items)
    .IgnoreColumns(a => new { a.Clicks, a.CreateTime }).ToSql();
//UPDATE `tb_topic` SET `Title` = CASE `Id` WHEN 1 THEN ?p_0 WHEN 2 THEN ?p_1 WHEN 3 THEN ?p_2 
//WHEN 4 THEN ?p_3 WHEN 5 THEN ?p_4 WHEN 6 THEN ?p_5 WHEN 7 THEN ?p_6 WHEN 8 THEN ?p_7 WHEN 9 
//THEN ?p_8 WHEN 10 THEN ?p_9 END WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))

//批量更新指定列
var t8 = fsql.Update<Topic>().SetSource(items).Set(a => a.CreateTime, DateTime.Now).ToSql();
//UPDATE `tb_topic` SET `CreateTime` = ?p_0 WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))
```
### 更新条件
```csharp
fsql.Update<Topic>(object dywhere)
```
dywhere 支持
* 主键值
* new[] { 主键值1, 主键值2 }
* Topic对象
* new[] { Topic对象1, Topic对象2 }
* new { id = 1 }
```csharp
var t9 = fsql.Update<Topic>().Set(a => a.Title, "新标题").Where(a => a.Id == 1).ToSql();
//UPDATE `tb_topic` SET `Title` = '新标题' WHERE (Id = 1)
```
### 自定义SQL
```csharp
var t10 = fsql.Update<Topic>().SetRaw("Title = {0}", "新标题").Where("Id = {0}", 1).ToSql();
//UPDATE `tb_topic` SET Title = '新标题' WHERE (Id = 1)
//sql语法条件，参数使用 {0}，与 string.Format 保持一致，无须加单引号，错误的用法：'{0}'
```
### 执行命令
| 方法 | 返回值 | 参数 | 描述 |
| - | - | - | - |
| ExecuteAffrows | long | | 执行SQL语句，返回影响的行数 |
| ExecuteUpdated | List\<T1\> | | 执行SQL语句，返回更新后的记录 |
> 更多资料：[《Update更新数据》](https://github.com/2881099/FreeSql/wiki/%e4%bf%ae%e6%94%b9)

# Part4 删除
详情查看：[《Delete 删除数据》](https://github.com/2881099/FreeSql/wiki/%e5%88%a0%e9%99%a4)

# Part5 表达式函数
详情查看：[《Expression 表达式函数》](https://github.com/2881099/FreeSql/wiki/%e8%a1%a8%e8%be%be%e5%bc%8f%e5%87%bd%e6%95%b0)

# Part6 事务

```csharp
fsql.Transaction(() => {
     //code
});
```

没异常就提交，有异常会回滚。

代码体内只可以使用同步方法，因为事务对象挂靠在线程关联上，使用异步方法会切换线程。

## 贡献者名单

[systemhejiyong](https://github.com/systemhejiyong)