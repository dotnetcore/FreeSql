<h1 align="center"> 🦄 FreeSql, 第一款国产 AOT ORM！</h1><div align="center">

FreeSql 是一款功能强大的对象关系映射（O/RM）组件，支持 .NET Core 2.1+、.NET Framework 4.0+、Xamarin 以及 AOT。

[![Member project of .NET Core Community](https://img.shields.io/badge/member%20project%20of-NCC-9e20c9.svg)](https://github.com/dotnetcore)
[![nuget](https://img.shields.io/nuget/v/FreeSql.svg?style=flat-square)](https://www.nuget.org/packages/FreeSql) 
[![stats](https://img.shields.io/nuget/dt/FreeSql.svg?style=flat-square)](https://www.nuget.org/stats/packages/FreeSql?groupby=Version) 
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/2881099/FreeSql/master/LICENSE.txt)

<p align="center">
    <a href="README.md">English</a> |   
    <span>中文</span>
</p>

</div>

- 🛠 支持 CodeFirst 模式，即便使用 Access 数据库也支持数据迁移；
- 💻 支持 DbFirst 模式，支持从数据库导入实体类，或使用[实体类生成工具](https://github.com/2881099/FreeSql/wiki/DbFirst)生成实体类；
- ⛳ 支持 深入的类型映射，比如 PgSql 的数组类型等；
- ✒ 支持 丰富的表达式函数，以及灵活的自定义解析；
- 🏁 支持 导航属性一对多、多对多贪婪加载，以及延时加载；
- 📃 支持 读写分离、分表分库、过滤器、乐观锁、悲观锁；
- 🌳 支持 MySql/SqlServer/PostgreSQL/Oracle/Sqlite/Firebird/达梦/人大金仓/南大通用/虚谷/神舟通用/翰高/ClickHouse/DuckDB/TDengine/QuestDB/Access 等数据库；

QQ群：561616019(在线)、4336577(已满)、8578575(已满)、52508226(已满)

## 📚 文档开源（免费）

[部署本地文档](https://github.com/2881099/FreeSql.Wiki.VuePress)、[在线文档](https://freesql.net)

> 示范项目

- [zhontai.net Admin 后台管理系统](https://github.com/zhontai/Admin.Core)
- [A simple and practical CMS implemented by .NET Core](https://github.com/luoyunchong/lin-cms-dotnetcore)
- [AIBPM 一个开源的工作流引擎](https://github.com/leooneone/aibpm.plus)
- [NetAdmin C#12/.NET9、FreeSql 快速开发框架](https://github.com/nsnail/NetAdmin)

<p align="center">
  <img src="https://github.com/dotnetcore/FreeSql/raw/master/functions13.png"/>
</p>

## 🚀 快速入门

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

### 🔎 Query (查询)
```csharp
//OneToOne、ManyToOne
fsql.Select<Tag>().Where(a => a.Parent.Parent.Name == "粤语").ToList();

//OneToMany
fsql.Select<Tag>().IncludeMany(a => a.Tags, then => then.Where(sub => sub.Name == "foo")).ToList();

//ManyToMany
fsql.Select<Song>()
  .IncludeMany(a => a.Tags, then => then.Where(sub => sub.Name == "foo"))
  .Where(s => s.Tags.Any(t => t.Name == "国语"))
  .ToList();

//Other
fsql.Select<YourType>()
  .Where(a => a.IsDelete == 0)
  .WhereIf(keyword != null, a => a.UserName.Contains(keyword))
  .WhereIf(role_id > 0, a => a.RoleId == role_id)
  .Where(a => a.Nodes.Any(t => t.Parent.Id == t.UserId))
  .Count(out var total)
  .Page(page, size)
  .OrderByDescending(a => a.Id)
  .ToList()
```
[更多信息](https://github.com/2881099/FreeSql/wiki/%e6%9f%a5%e8%af%a2)

```csharp
fsql.Select<Song>().Where(a => new[] { 1, 2, 3 }.Contains(a.Id)).ToList();

fsql.Select<Song>().Where(a => a.CreateTime.Date == DateTime.Today).ToList();

fsql.Select<Song>().OrderBy(a => Guid.NewGuid()).Limit(10).ToList();

fsql.Select<Song>().ToList(a => new
{
    a.Id,
    Tags = fsql.Select<Tag>().ToList(),
    SongTags = fsql.Select<SongTag>().Where(b => b.TopicId == a.Id).ToList()
});
```
[更多信息](https://github.com/2881099/FreeSql/wiki/%e8%a1%a8%e8%be%be%e5%bc%8f%e5%87%bd%e6%95%b0) 

### 🚁 Repository (仓储)

> dotnet add package FreeSql.Repository

```csharp
[Transactional]
public void Add() {
  var repo = ioc.GetService<BaseRepository<Tag>>();
  repo.DbContextOptions.EnableCascadeSave = true;

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

参考：[在 ASP.NET Core 中使用 `TransactionalAttribute` + `UnitOfWorkManager` 实现多种事务传播](https://github.com/dotnetcore/FreeSql/issues/289)

## 💪 Performance (性能)

FreeSql Query 与 Dapper Query 的对比：

```shell
Elapsed: 00:00:00.6733199; Query Entity Counts: 131072; ORM: Dapper

Elapsed: 00:00:00.4554230; Query Tuple Counts: 131072; ORM: Dapper

Elapsed: 00:00:00.6846146; Query Dynamic Counts: 131072; ORM: Dapper

Elapsed: 00:00:00.6818111; Query Entity Counts: 131072; ORM: FreeSql*

Elapsed: 00:00:00.6060042; Query Tuple Counts: 131072; ORM: FreeSql*

Elapsed: 00:00:00.4211323; Query ToList<Tuple> Counts: 131072; ORM: FreeSql*

Elapsed: 00:00:01.0236285; Query Dynamic Counts: 131072; ORM: FreeSql*
```

FreeSql ToList 与 Dapper Query 的对比：

```shell
Elapsed: 00:00:00.6707125; ToList Entity Counts: 131072; ORM: FreeSql*

Elapsed: 00:00:00.6495301; Query Entity Counts: 131072; ORM: Dapper
```

[更多信息](https://github.com/2881099/FreeSql/wiki/%e6%80%a7%e8%83%bd)

## 👯 Contributors (贡献者)

<a href="https://contributors-img.web.app/image?repo=dotnetcore/FreeSql">
  <img src="https://contributors-img.web.app/image?repo=dotnetcore/FreeSql" />
</a>

以及其他为本项目提出重要建议的朋友们，他们包括：

[systemhejiyong](https://github.com/systemhejiyong), 
[LambertW](https://github.com/LambertW), 
[mypeng1985](https://github.com/mypeng1985), 
[stulzq](https://github.com/stulzq), 
[movingsam](https://github.com/movingsam), 
[ALer-R](https://github.com/ALer-R), 
[zouql](https://github.com/zouql), 
深圳|凉茶, 
[densen2014](https://github.com/densen2014), 
[LiaoLiaoWuJu](https://github.com/LiaoLiaoWuJu), 
[hd2y](https://github.com/hd2y), 
[tky753](https://github.com/tky753), 
[feijie999](https://github.com/feijie999), 
constantine, 
[JohnZhou2020](https://github.com/JohnZhou2020), 
[mafeng8](https://github.com/mafeng8), 
[VicBilibily](https://github.com/VicBilibily), 
[Soar](https://github.com/sgf),
[quzhen91](https://github.com/quzhen91),
homejun,
[d4ilys](https://github.com/d4ilys) 等。

## 💕 Donation (捐赠)

L\*y 58元、花花 88元、麦兜很乖 50元、网络来者 2000元、John 99.99元、alex 666元、bacongao 36元、无名 100元、Eternity 188元、无名 10元、⌒.Helper~..oO 66元、习惯与被习惯 100元、无名 100元、蔡易喋 88.88元、中讯科技 1000元、Good Good Work 24元、Nothing 100元、兰州天擎赵 500元、哈利路亚 300元、
无名 100元、蛰伏 99.99元、TCYM 66.66元、MOTA 5元、LDZXG 30元、Near 30元、建爽 66元、无名 200元、LambertWu 100元、无名 18.88元、乌龙 50元、无名 100元、陳怼怼 66.66元、陳怼怼 66.66元、丁淮 100元、李伟坚-Excel催化剂 100元、白狐 6.66元、她微笑的脸y 30元、Eternity²º²¹ 588元、夜归柴门 88元、蔡易喋 666.66元、
*礼 10元、litrpa 88元、Alax CHOW 200元、Daily 66元、k*t 66元、蓝 100元、\*菜 10元、生命如歌 1000元、山鸡 88元、平凡 100元、大树 1000元、软软的毛毛虫 66.66元、问卷星 2000元、与你无关 5000元

> 超级感谢你的打赏。

- [Alipay](https://www.cnblogs.com/FreeSql/gallery/image/338860.html)

- [WeChat](https://www.cnblogs.com/FreeSql/gallery/image/338859.html)

## 🗄 License (许可证)

[MIT](LICENSE)
