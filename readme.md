FreeSql 是一个功能强大的 .NETStandard 库，用于对象关系映射程序(O/RM)，支持 .NETCore 2.1+ 或 .NETFramework 4.6.1+（QQ群：4336577）。

| Package Name |  NuGet | Downloads |
|--------------|  ------- |  ---- |
| FreeSql | [![nuget](https://img.shields.io/nuget/v/FreeSql.svg?style=flat-square)](https://www.nuget.org/packages/FreeSql) | [![stats](https://img.shields.io/nuget/dt/FreeSql.svg?style=flat-square)](https://www.nuget.org/stats/packages/FreeSql?groupby=Version) |
| [FreeSql.Repository](https://github.com/2881099/FreeSql/wiki/Repository) | [![nuget](https://img.shields.io/nuget/v/FreeSql.Repository.svg?style=flat-square)](https://www.nuget.org/packages/FreeSql.Repository) | [![stats](https://img.shields.io/nuget/dt/FreeSql.Repository.svg?style=flat-square)](https://www.nuget.org/stats/packages/FreeSql.Repository?groupby=Version) |
| [FreeSql.DbContext](https://github.com/2881099/FreeSql.DbContext) | [![nuget](https://img.shields.io/nuget/v/FreeSql.DbContext.svg?style=flat-square)](https://www.nuget.org/packages/FreeSql.DbContext) | [![stats](https://img.shields.io/nuget/dt/FreeSql.DbContext.svg?style=flat-square)](https://www.nuget.org/stats/packages/FreeSql.DbContext?groupby=Version) |
| [FreeSql.AdminLTE](https://github.com/2881099/FreeSql.AdminLTE) | [![nuget](https://img.shields.io/nuget/v/FreeSql.AdminLTE.svg?style=flat-square)](https://www.nuget.org/packages/FreeSql.AdminLTE) | [![stats](https://img.shields.io/nuget/dt/FreeSql.AdminLTE.svg?style=flat-square)](https://www.nuget.org/stats/packages/FreeSql.AdminLTE?groupby=Version) |
| [FreeSql.Connection.Extensions](https://github.com/2881099/FreeSql.Connection.Extensions) | [![nuget](https://img.shields.io/nuget/v/FreeSql.Connection.Extensions.svg?style=flat-square)](https://www.nuget.org/packages/FreeSql.Connection.Extensions) | [![stats](https://img.shields.io/nuget/dt/FreeSql.Connection.Extensions.svg?style=flat-square)](https://www.nuget.org/stats/packages/FreeSql.Connection.Extensions?groupby=Version) |

# 特性

- [x] 支持 CodeFirst 迁移；
- [x] 支持 DbFirst 从数据库导入实体类，提供失血、贫血、充血三种生成模板；
- [x] 大量采用 ExpressionTree 技术提升性能；
- [x] 支持深入的类型映射，比如pgsql的数组类型，堪称匠心制作；
- [x] 支持丰富的表达式函数；
- [x] 支持导航属性查询，和延时加载；
- [x] 支持同步/异步数据库操作方法，链式查询方法；
- [x] 支持读写分离、分表分库，租户设计，过滤器，乐观锁；
- [x] 支持多种数据库，MySql/SqlServer/PostgreSQL/Oracle/Sqlite；

| | |
| - | - |
| ![image](https://user-images.githubusercontent.com/16286519/55138232-f5e19e80-516d-11e9-9144-173cc7e52845.png) | [《Select》](https://github.com/2881099/FreeSql/wiki/%e6%9f%a5%e8%af%a2) \| [《Update》](https://github.com/2881099/FreeSql/wiki/%e4%bf%ae%e6%94%b9) \| [《Insert》](https://github.com/2881099/FreeSql/wiki/%e6%b7%bb%e5%8a%a0) \| [《Delete》](https://github.com/2881099/FreeSql/wiki/%e5%88%a0%e9%99%a4) |
| ![image](https://user-images.githubusercontent.com/16286519/55138241-faa65280-516d-11e9-8b27-139dea46e4df.png) | [《表达式函数》](https://github.com/2881099/FreeSql/wiki/%e8%a1%a8%e8%be%be%e5%bc%8f%e5%87%bd%e6%95%b0) \| [《CodeFirst》](https://github.com/2881099/FreeSql/wiki/CodeFirst) \| [《DbFirst》](https://github.com/2881099/FreeSql/wiki/DbFirst) |
| ![image](https://user-images.githubusercontent.com/16286519/55138263-06921480-516e-11e9-8da9-81f18a18b694.png) | [《Repository》](https://github.com/2881099/FreeSql/wiki/Repository) \| [《UnitOfWork》](https://github.com/2881099/FreeSql/wiki/%e5%b7%a5%e4%bd%9c%e5%8d%95%e5%85%83) \| [《过滤器》](https://github.com/2881099/FreeSql/wiki/%e8%bf%87%e6%bb%a4%e5%99%a8) \| [《乐观锁》](https://github.com/2881099/FreeSql/wiki/%e4%bf%ae%e6%94%b9) \| [《DbContext》](https://github.com/2881099/FreeSql/wiki/DbContext) |
| ![image](https://user-images.githubusercontent.com/16286519/55138284-0eea4f80-516e-11e9-8764-29264807f402.png) | [《读写分离》](https://github.com/2881099/FreeSql/wiki/%e8%af%bb%e5%86%99%e5%88%86%e7%a6%bb) \| [《分区分表》](https://github.com/2881099/FreeSql/wiki/%e5%88%86%e5%8c%ba%e5%88%86%e8%a1%a8) \| [《租户》](https://github.com/2881099/FreeSql/wiki/%e7%a7%9f%e6%88%b7) \| [更新日志](https://github.com/2881099/FreeSql/wiki/%e6%9b%b4%e6%96%b0%e6%97%a5%e5%bf%97) |

# Quick start

> dotnet add package FreeSql

```csharp
IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.Sqlite, 
        @"Data Source=|DataDirectory|\document.db;Pooling=true;Max Pool Size=10")
    .UseAutoSyncStructure(true) //自动同步实体结构到数据库
    .Build();

class Song {
    [Column(IsIdentity = true)]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public DateTime CreateTime { get; set; }

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
    public string Name { get; set; }

    public int? Parent_id { get; set; }
    public virtual Tag Parent { get; set; }

    public virtual ICollection<Song> Songs { get; set; }
    public virtual ICollection<Tag> Tags { get; set; }
}
```

# Query
```csharp
//OneToOne、ManyToOne
var t0 = fsql.Select<Tag>().Where(a => a.Parent.Parent.Name == "粤语").ToList();

//OneToMany
var t1 = fsql.Select<Tag>().Where(a => a.Tags.AsSelect().Any(t => t.Parent.Id == 10)).ToList();

//ManyToMany
var t2 = fsql.Select<Song>().Where(s => s.Tags.AsSelect().Any(t => t.Name == "国语")).ToList();
```
更多前往Wiki：[《Select 查询数据文档》](https://github.com/2881099/FreeSql/wiki/%e6%9f%a5%e8%af%a2)

```csharp
var t3 = fsql.Select<Song>.Where(a => new[] { 1, 2, 3 }.Contains(a.Id)).ToList();
```
```csharp
var t4 = fsql.Select<Song>s.Where(a => a.CreateTime.Date == DateTime.Now.Date).ToList();
```
```csharp
var t5 = fsql.Select<Song>s.OrderBy(a => Guid.NewGuid()).Limit(1).ToList();
```
更多前往Wiki：[《表达式函数》](https://github.com/2881099/FreeSql/wiki/%e8%a1%a8%e8%be%be%e5%bc%8f%e5%87%bd%e6%95%b0) 

# Repository & UnitOfWork
> dotnet add package FreeSql.Repository

```csharp
using (var uow = fsql.CreateUnitOfWork()) {

    var songRepository = uow.GetRepository<Song, int>();
    var tagRepository = uow.GetRepository<Tag, int>();

    await songRepository.InsertAsync(new Song());
    await tagRepository.InsertAsync(new Tag());

    uow.Commit();
}
```

# DbContext & DbSet
> dotnet add package FreeSql.DbContext

```csharp
public class SongContext : DbContext {

    public DbSet<Song> Songs { get; set; }
    public DbSet<Song> Tags { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder) {
        builder.UseFreeSql(fsql);
    }
}

using (var ctx = new SongContext()) {
    var song = new Song { };
    await ctx.Songs.AddAsync(song);
    var id = song.Id;

    var adds = Enumerable.Range(0, 100).Select(a => new Song { Title = "xxxx" + a, Url = "url222" }).ToList();
    await ctx.Songs.AddRangeAsync(adds);

    for (var a = 0; a < adds.Count; a++)
        adds[a].Title = "dkdkdkdk" + a;

    ctx.Songs.UpdateRange(adds);
    ctx.Songs.RemoveRange(adds.Skip(10).Take(20).ToList());
    ctx.Songs.Update(adds.Last());

    await ctx.SaveChangesAsync();
}
```

# DataFilter & Tenant

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services) {
    services.AddSingleton<IFreeSql>(fsql);
    services.AddMvc();

    var builder = new ContainerBuilder();

    builder.RegisterFreeRepository(filter => filter
        .Apply<ISoftDelete>("SoftDelete", a => a.IsDeleted == false)
        .Apply<ITenant>("Tenant", a => a.TenantId == 1)
    );

    builder.Populate(services);
    var container = builder.Build();
    return new AutofacServiceProvider(container);
}
```

Temporary disable:
```csharp
var songRepository = fsql.GetRepository<Song, int>();

using (songRepository.DataFilter.Disable("Tenant")) {
    //Tenant Invalid
}
//Tenant restore
```

# Performance

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

[Test code](FreeSql.Tests.PerformanceTests/MySqlAdoTest.cs)

# Contributors

[systemhejiyong](https://github.com/systemhejiyong)
[LambertW](https://github.com/LambertW)
