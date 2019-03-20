FreeSql 是一个功能强大的 .NETStandard 库，用于对象关系映射程序(O/RM)，便于开发人员能够使用 .NETStandard 对象来处理数据库，不必经常编写大部分数据访问代码。支持 .NETCore 2.1+ 或 .NETFramework 4.6.1+。

| Package Name |  NuGet | Downloads |
|--------------|  ------- |  ---- |
| FreeSql | [![nuget](https://img.shields.io/nuget/v/FreeSql.svg?style=flat-square)](https://www.nuget.org/packages/FreeSql) | [![stats](https://img.shields.io/nuget/dt/FreeSql.svg?style=flat-square)](https://www.nuget.org/stats/packages/FreeSql?groupby=Version) |
| [FreeSql.Repository](https://github.com/2881099/FreeSql/wiki/Repository) | [![nuget](https://img.shields.io/nuget/v/FreeSql.Repository.svg?style=flat-square)](https://www.nuget.org/packages/FreeSql.Repository) | [![stats](https://img.shields.io/nuget/dt/FreeSql.Repository.svg?style=flat-square)](https://www.nuget.org/stats/packages/FreeSql.Repository?groupby=Version) |
| [FreeSql.DbContext](https://github.com/2881099/FreeSql/wiki/DbContext) | [![nuget](https://img.shields.io/nuget/v/FreeSql.DbContext.svg?style=flat-square)](https://www.nuget.org/packages/FreeSql.DbContext) | [![stats](https://img.shields.io/nuget/dt/FreeSql.DbContext.svg?style=flat-square)](https://www.nuget.org/stats/packages/FreeSql.DbContext?groupby=Version) |

# 特性

- [x] 支持 CodeFirst 迁移；
- [x] 支持 DbFirst 从数据库导入实体类，支持三种模板生成器；
- [x] 采用 ExpressionTree 高性能读取数据；
- [x] 支持深入的类型映射，比如pgsql的数组类型，堪称匠心制作；
- [x] 支持丰富的表达式函数；
- [x] 支持导航属性查询，和延时加载；
- [x] 支持同步/异步数据库操作方法，丰富多彩的链式查询方法；
- [x] 支持读写分离、分表分库，租户设计；
- [x] 支持多种数据库，MySql/SqlServer/PostgreSQL/Oracle/Sqlite；

| | |
| - | - |
| 入门 | [《Select》](https://github.com/2881099/FreeSql/wiki/%e6%9f%a5%e8%af%a2) \| [《Update》](https://github.com/2881099/FreeSql/wiki/%e4%bf%ae%e6%94%b9) \| [《Insert》](https://github.com/2881099/FreeSql/wiki/%e6%b7%bb%e5%8a%a0) \| [《Delete》](https://github.com/2881099/FreeSql/wiki/%e5%88%a0%e9%99%a4) |
| 新手 | [《表达式函数》](https://github.com/2881099/FreeSql/wiki/%e8%a1%a8%e8%be%be%e5%bc%8f%e5%87%bd%e6%95%b0) \| [《CodeFirst》](https://github.com/2881099/FreeSql/wiki/CodeFirst) \| [《DbFirst》](https://github.com/2881099/FreeSql/wiki/DbFirst) |
| 高手 | [《Repository》](https://github.com/2881099/FreeSql/wiki/Repository) \| [《UnitOfWork》](https://github.com/2881099/FreeSql/wiki/%e5%b7%a5%e4%bd%9c%e5%8d%95%e5%85%83) \| [《过滤器》](https://github.com/2881099/FreeSql/wiki/%e8%bf%87%e6%bb%a4%e5%99%a8) \| [《DbContext》](https://github.com/2881099/FreeSql/wiki/DbContext) |
| 不朽 | [《读写分离》](https://github.com/2881099/FreeSql/wiki/%e8%af%bb%e5%86%99%e5%88%86%e7%a6%bb) \| [《分区分表》](https://github.com/2881099/FreeSql/wiki/%e5%88%86%e5%8c%ba%e5%88%86%e8%a1%a8) \| [《租户》](https://github.com/2881099/FreeSql/wiki/%e7%a7%9f%e6%88%b7) \| [更新日志](https://github.com/2881099/FreeSql/wiki/%e6%9b%b4%e6%96%b0%e6%97%a5%e5%bf%97) |

# 快速开始
```csharp
var connstr = "Data Source=127.0.0.1;User ID=root;Password=root;" + 
    "Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=10";

IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.MySql, connstr)
    .UseSlave("connectionString1", "connectionString2")
    //读写分离，使用从数据库，支持多个

    .UseMonitorCommand(
        cmd => Console.WriteLine(cmd.CommandText),
        //监听SQL命令对象，在执行前
        (cmd, traceLog) => Console.WriteLine(traceLog))
        //监听SQL命令对象，在执行后

    .UseLogger(null)
    //使用日志，不指定默认输出控制台 ILogger
    .UseCache(null)
    //使用缓存，不指定默认使用内存 IDistributedCache

    .UseAutoSyncStructure(true)
    //自动同步实体结构到数据库
    .UseSyncStructureToLower(true)
    //转小写同步结构
    .UseSyncStructureToUpper(true)
    //转大写同步结构
    .UseConfigEntityFromDbFirst(true)
    //若无配置实体类主键、自增，可从数据库导入
    .UseNoneCommandParameter(true)
    //不使用命令参数化执行，针对 Insert/Update，也可临时使用 IInsert/IUpdate.NoneParameter() 

    .UseLazyLoading(true)
    //延时加载导航属性对象，导航属性需要声明 virtual
    .Build();
```

# 实体

FreeSql 使用模型执行数据访问，模型由实体类表示数据库表或视图，用于查询和保存数据。

可从现有数据库生成实体模型，提供 IDbFirst 生成实体模型。

或者手动创建模型，基于模型创建或修改数据库，提供 ICodeFirst 同步结构的 API（甚至可以做到开发阶段自动同步）。

```csharp
class Song {
    [Column(IsIdentity = true)]
    public int Id { get; set; }
    public DateTime? Create_time { get; set; }
    public bool? Is_deleted { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }

    public virtual ICollection<Tag> Tags { get; set; }
}
class Song_tag {
    public int Song_id { get; set; }
    public virtual Song Song { get; set; }

    public int Tag_id { get; set; }
    public virtual Tag Tag { get; set; }
}
class Tag {
    [Column(IsIdentity = true)]
    public int Id { get; set; }
    public int? Parent_id { get; set; }
    public virtual Tag Parent { get; set; }

    public decimal? Ddd { get; set; }
    public string Name { get; set; }

    public virtual ICollection<Song> Songs { get; set; }
    public virtual ICollection<Tag> Tags { get; set; }
}
```

# 查询

```csharp
//OneToOne、ManyToOne
var t0 = fsql.Select<Tag>().Where(a => a.Parent.Parent.Name == "粤语").ToSql();
//SELECT a.`Id`, a.`Parent_id`, a__Parent.`Id` as3, a__Parent.`Parent_id` as4, a__Parent.`Ddd`, a__Parent.`Name`, a.`Ddd` as7, a.`Name` as8 
//FROM `Tag` a 
//LEFT JOIN `Tag` a__Parent ON a__Parent.`Id` = a.`Parent_id` 
//LEFT JOIN `Tag` a__Parent__Parent ON a__Parent__Parent.`Id` = a__Parent.`Parent_id` 
//WHERE (a__Parent__Parent.`Name` = '粤语')

//OneToMany
var t1 = fsql.Select<Tag>().Where(a => a.Tags.AsSelect().Any(t => t.Parent.Id == 10)).ToSql();
//SELECT a.`Id`, a.`Parent_id`, a.`Ddd`, a.`Name` 
//FROM `Tag` a 
//WHERE (exists(SELECT 1 
//	FROM `Tag` t 
//	LEFT JOIN `Tag` t__Parent ON t__Parent.`Id` = t.`Parent_id` 
//	WHERE (t__Parent.`Id` = 10) AND (t.`Parent_id` = a.`Id`) 
//	limit 0,1))

//ManyToMany
var t2 = fsql.Select<Song>().Where(s => s.Tags.AsSelect().Any(t => t.Name == "国语")).ToSql();
//SELECT a.`Id`, a.`Create_time`, a.`Is_deleted`, a.`Title`, a.`Url` 
//FROM `Song` a
//WHERE(exists(SELECT 1
//	FROM `Song_tag` Mt_Ms
//	WHERE(Mt_Ms.`Song_id` = a.`Id`) AND(exists(SELECT 1
//		FROM `Tag` t
//		WHERE(t.`Name` = '国语') AND(t.`Id` = Mt_Ms.`Tag_id`)
//		limit 0, 1))
//	limit 0, 1))
```
更多前往wiki：[《Select查询数据文档》](https://github.com/2881099/FreeSql/wiki/%e6%9f%a5%e8%af%a2)

# 表达式函数

```csharp
var t1 = select.Where(a => new[] { 1, 2, 3 }.Contains(a.testFieldInt)).ToSql();
//SELECT a.`Id`, a.`Clicks`, a.`TestTypeInfoGuid`, a.`Title`, a.`CreateTime` 
//FROM `Song` a 
//WHERE (a.`Id` in (1,2,3))
```

查找今天创建的数据

```csharp
var t2 = select.Where(a => a.CreateTime.Date == DateTime.Now.Date).ToSql();
```

SqlServer 下随机获取记录

```csharp
var t3 = select.OrderBy(a => Guid.NewGuid()).Limit(1).ToSql();
//SELECT top 1 ...
//FROM [Song] a 
//ORDER BY newid()
```

更多前往wiki：[《Expression 表达式函数文档》](https://github.com/2881099/FreeSql/wiki/%e8%a1%a8%e8%be%be%e5%bc%8f%e5%87%bd%e6%95%b0) 

# 返回数据

```csharp
List<Song> t1 = fsql.Select<Song>().Where(a => a.Id > 0).ToList();

//返回普通字段 + 导航对象 Type 的数据
List<Song> t2 = fsql.Select<Song>().LeftJoin(a => a.Type.Id == a.TypeId).ToList();

//返回一个字段
List<int> t3 = fsql.Select<Song>().Where(a => a.Id > 0).ToList(a => a.Id);

//返回匿名类型
List<匿名类型> t4 = fsql.Select<Song>().Where(a => a.Id > 0).ToList(a => new { a.Id, a.Title });

//返回元组
List<(int, string)> t5 = fsql.Select<Song>().Where(a => a.Id > 0).ToList<(int, string)>("id, title");

//返回SQL字段
List<匿名类> t4 = select.Where(a => a.Id > 0).Skip(100).Limit(200)
    .ToList(a => new {
        a.Id, a.Title,
        cstitle = "substr(a.title, 0, 2)", //将 substr(a.title, 0, 2) 作为查询字段
        csnow = Convert.ToDateTime("now()"), //将 now() 作为查询字段
        //奇思妙想：怎么查询开窗函数的结果
    });
```
执行SQL返回数据
```csharp
List<Song> t6 = fsql.Ado.Query<Song>("select * from song");
List<(int, string ,string)> t7 = fsql.Ado.Query<(int, string, string)>("select id,title,url from song");
List<dynamic> t8 = fsql.Ado.Query<dynamic>("select * from song");
```
更多前往wiki：[《Select查询数据》](https://github.com/2881099/FreeSql/wiki/%e6%9f%a5%e8%af%a2)

# 性能测试

FreeSql Query & Dapper Query
```shell
Elapsed: 00:00:00.6733199; Query Entity Counts: 131072; ORM: Dapper

Elapsed: 00:00:00.4554230; Query Tuple Counts: 131072; ORM: Dapper

Elapsed: 00:00:00.6846146; Query Dynamic Counts: 131072; ORM: Dapper

Elapsed: 00:00:00.6818111; Query Entity Counts: 131072; ORM: FreeSql*

Elapsed: 00:00:00.6060042; Query Tuple Counts: 131072; ORM: FreeSql*

Elapsed: 00:00:00.4211323; Query ToList<Tuple> Counts: 131072; ORM: FreeSql*

Elapsed: 00:00:01.0236285; Query Dynamic Counts: 131072; ORM: FreeSql*
```

FreeSql ToList & Dapper Query
```shell
Elapsed: 00:00:00.6707125; ToList Entity Counts: 131072; ORM: FreeSql*

Elapsed: 00:00:00.6495301; Query Entity Counts: 131072; ORM: Dapper
```

测试方法：运行两次，以第二次性能报告，避免了首个运行慢不公平的情况。[查看测试代码](FreeSql.Tests.PerformanceTests/MySqlAdoTest.cs)

FreeSql 目前使用的ExpressionTree+缓存，因为支持更为复杂的数据类型，所以比 Dapper Emit 慢少许。

# 贡献者名单

[systemhejiyong](https://github.com/systemhejiyong)
[LambertW](https://github.com/LambertW)
