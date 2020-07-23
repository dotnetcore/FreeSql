## 🦄 　FreeSql

[![nuget](https://img.shields.io/nuget/v/FreeSql.svg?style=flat-square)](https://www.nuget.org/packages/FreeSql) [![stats](https://img.shields.io/nuget/dt/FreeSql.svg?style=flat-square)](https://www.nuget.org/stats/packages/FreeSql?groupby=Version) [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/2881099/FreeSql/master/LICENSE.txt)

FreeSql 是功能强大的对象关系映射技术(O/RM)，支持 .NETCore 2.1+ 或 .NETFramework 4.0+ 或 Xamarin。

- [x] 支持 CodeFirst 迁移，哪怕使用 Access 数据库也支持；
- [x] 支持 DbFirst 从数据库导入实体类，[安装实体类生成工具](https://github.com/2881099/FreeSql/wiki/DbFirst)；
- [x] 支持 深入的类型映射，比如pgsql的数组类型；
- [x] 支持 丰富的表达式函数，以及灵活的自定义解析；
- [x] 支持 导航属性一对多、多对多贪婪加载，以及延时加载；
- [x] 支持 读写分离、分表分库、过滤器、乐观锁、悲观锁；
- [x] 支持 MySql/SqlServer/PostgreSQL/Oracle/Sqlite/达梦/人大金仓/神舟通用/Access；

## 📚 　Documentation

| | |
| - | - |
| <img src="https://images.cnblogs.com/cnblogs_com/FreeSql/1656841/o_200424151826freesql_doc1.png" width="30" height="46"/> | [《新人学习指引》](https://www.cnblogs.com/FreeSql/p/11531300.html) \| [《Select》](https://github.com/2881099/FreeSql/wiki/%e6%9f%a5%e8%af%a2) \| [《Update》](https://github.com/2881099/FreeSql/wiki/%e4%bf%ae%e6%94%b9) \| [《Insert》](https://github.com/2881099/FreeSql/wiki/%e6%b7%bb%e5%8a%a0) \| [《Delete》](https://github.com/2881099/FreeSql/wiki/%e5%88%a0%e9%99%a4) |
| <img src="https://images.cnblogs.com/cnblogs_com/FreeSql/1656841/o_200424151831freesql_doc2.png" width="30" height="46"/> | [《表达式函数》](https://github.com/2881099/FreeSql/wiki/%e8%a1%a8%e8%be%be%e5%bc%8f%e5%87%bd%e6%95%b0) \| [《CodeFirst》](https://github.com/2881099/FreeSql/wiki/CodeFirst) \| [《DbFirst》](https://github.com/2881099/FreeSql/wiki/DbFirst) \| [《过滤器》](https://github.com/2881099/FreeSql/wiki/%e8%bf%87%e6%bb%a4%e5%99%a8) |
| <img src="https://images.cnblogs.com/cnblogs_com/FreeSql/1656841/o_200424151835freesql_doc3.png" width="30" height="46"/> | [《Repository》](https://github.com/2881099/FreeSql/wiki/Repository) \| [《UnitOfWork》](https://github.com/2881099/FreeSql/wiki/%e5%b7%a5%e4%bd%9c%e5%8d%95%e5%85%83) \| [《AOP》](https://github.com/2881099/FreeSql/wiki/AOP) \| [《DbContext》](https://github.com/2881099/FreeSql/wiki/DbContext) |
| <img src="https://images.cnblogs.com/cnblogs_com/FreeSql/1656841/o_200424151840freesql_doc4.png" width="30" height="46"/> | [《读写分离》](https://github.com/2881099/FreeSql/wiki/%e8%af%bb%e5%86%99%e5%88%86%e7%a6%bb) \| [《分区分表》](https://github.com/2881099/FreeSql/wiki/%e5%88%86%e5%8c%ba%e5%88%86%e8%a1%a8) \| [《黑科技》](https://github.com/2881099/FreeSql/wiki/%E9%AA%9A%E6%93%8D%E4%BD%9C) \| [《常见问题》](https://github.com/dotnetcore/FreeSql/wiki/%E5%B8%B8%E8%A7%81%E9%97%AE%E9%A2%98)  \| [*更新日志*](https://github.com/2881099/FreeSql/wiki/%e6%9b%b4%e6%96%b0%e6%97%a5%e5%bf%97) |

> FreeSql 提供多种使用习惯，请根据实际情况选择团队合适的一种：

- 要么FreeSql，原始用法；
- 要么[FreeSql.Repository](https://github.com/2881099/FreeSql/wiki/Repository)，仓储+工作单元习惯；
- 要么[FreeSql.DbContext](https://github.com/2881099/FreeSql/wiki/DbContext)，有点像efcore的使用习惯；
- 要么[FreeSql.BaseEntity](https://github.com/2881099/FreeSql/tree/master/Examples/base_entity)，求简单使用这个；

> 示范项目

- [zhontai.net Admin 后台管理系统](https://github.com/zhontai/Admin.Core)
- [A simple and practical CMS implememted by .NET Core](https://github.com/luoyunchong/lin-cms-dotnetcore)
- [iusaas.com SaaS 企业应用管理系统](https://github.com/alonsoalon/TenantSite.Server)
- [EasyCms 企业建站，事业单位使用的CMS管理系统](https://github.com/jasonyush/EasyCMS)
- [内容管理系统](https://github.com/hejiyong/fscms)

<p align="center">
  <img src="https://images.cnblogs.com/cnblogs_com/kellynic/133561/o_200610084147functions09.png"/>
</p>

## 🚀 　Quick start

> dotnet add package FreeSql.Provider.Sqlite

```csharp
static IFreeSql fsql = new FreeSql.FreeSqlBuilder()
  .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=document.db")
  .UseAutoSyncStructure(true) //自动同步实体结构到数据库
  .Build(); //请务必定义成 Singleton 单例模式

class Song {
  [Column(IsIdentity = true)]
  public int Id { get; set; }
  public string Title { get; set; }
  public string Url { get; set; }
  public DateTime CreateTime { get; set; }
  
  public ICollection<Tag> Tags { get; set; }
}
class Song_tag {
  public int Song_id { get; set; }
  public Song Song { get; set; }
  
  public int Tag_id { get; set; }
  public Tag Tag { get; set; }
}
class Tag {
  [Column(IsIdentity = true)]
  public int Id { get; set; }
  public string Name { get; set; }
  
  public int? Parent_id { get; set; }
  public Tag Parent { get; set; }
  
  public ICollection<Song> Songs { get; set; }
  public ICollection<Tag> Tags { get; set; }
}
```

## 🔎 　Query
```csharp
//OneToOne、ManyToOne
fsql.Select<Tag>().Where(a => a.Parent.Parent.Name == "粤语").ToList();

//OneToMany
fsql.Select<Tag>().IncludeMany(a => a.Tags, then => then.Where(sub => sub.Name == "xxx")).ToList();

//ManyToMany
fsql.Select<Song>()
  .IncludeMany(a => a.Tags, then => then.Where(sub => sub.Name == "xxx"))
  .Where(s => s.Tags.AsSelect().Any(t => t.Name == "国语"))
  .ToList();

//Other
fsql.Select<Xxx>()
  .Where(a => a.IsDelete == 0)
  .WhereIf(keyword != null, a => a.UserName.Contains(keyword))
  .WhereIf(role_id > 0, a => a.RoleId == role_id)
  .Where(a => a.Nodes.AsSelect().Any(t => t.Parent.Id == t.UserId))
  .Count(out var total)
  .Page(page, size)
  .OrderByDescending(a => a.Id)
  .ToList()
```
[More..](https://github.com/2881099/FreeSql/wiki/%e6%9f%a5%e8%af%a2)

```csharp
fsql.Select<Song>().Where(a => new[] { 1, 2, 3 }.Contains(a.Id)).ToList();

fsql.Select<Song>().Where(a => a.CreateTime.Date == DateTime.Today).ToList();

fsql.Select<Song>().OrderBy(a => Guid.NewGuid()).Limit(10).ToList();
```
[More..](https://github.com/2881099/FreeSql/wiki/%e8%a1%a8%e8%be%be%e5%bc%8f%e5%87%bd%e6%95%b0) 

## 🚁 　Repository

> dotnet add package FreeSql.Repository

```csharp
[Transactional]
public void Add() {
  var repo = ioc.GetService<BaseRepository<Tag>>();
  repo.DbContextOptions.EnableAddOrUpdateNavigateList = true;

  var item = new Tag {
    Name = "testaddsublist",
    Tags = new[] {
      new Tag { Name = "sub1" },
      new Tag { Name = "sub2" }
    }
  };
  repo.Insert(item);
}
```

参考：[在 asp.net core 中使用 TransactionalAttribute + UnitOfWorkManager 实现多种事务传播](https://github.com/dotnetcore/FreeSql/issues/289)

## 💪 　Performance

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

[More..](https://github.com/2881099/FreeSql/wiki/%e6%80%a7%e8%83%bd)

## 👯 　Contributors

[systemhejiyong](https://github.com/systemhejiyong)、
[LambertW](https://github.com/LambertW)、
[mypeng1985](https://github.com/mypeng1985)、
[stulzq](https://github.com/stulzq)、
[movingsam](https://github.com/movingsam)、
[ALer-R](https://github.com/ALer-R)、
[zouql](https://github.com/zouql)、
深圳|凉茶、
[densen2014](https://github.com/densen2014)、
[LiaoLiaoWuJu](https://github.com/LiaoLiaoWuJu)、
[hd2y](https://github.com/hd2y)、
[tky753](https://github.com/tky753)、
[feijie999](https://github.com/feijie999)、
constantine

QQ群：4336577(已满)、8578575(在线)

## 💕 　Donation

L*y 58元、花花 88元、麦兜很乖 50元、网络来者 2000元、John 99.99元、alex 666元、bacongao 36元、无名 100元、Eternity 188元、无名 10元、⌒.Helper~..oO 66元、习惯与被习惯 100元、无名 100元、蔡易喋 88.88元、中讯科技 1000元、Good Good Work 24元、炽焰 6.6元、Nothing 100元、兰州天擎赵 500元

> Thank you for your donation

| [Alipay](https://images.cnblogs.com/cnblogs_com/kellynic/133561/o_200417052520IMG_7936(20200123-155553).png) | [WeChat](https://images.cnblogs.com/cnblogs_com/kellynic/133561/o_200417052707IMG_7935(20200123-155553).png) |
| - | - |
| <img height="300" src="https://images.cnblogs.com/cnblogs_com/kellynic/133561/o_200417052520IMG_7936(20200123-155553).png"/> | <img height="300" src="https://images.cnblogs.com/cnblogs_com/kellynic/133561/o_200417052707IMG_7935(20200123-155553).png"/> |
