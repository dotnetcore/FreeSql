FreeSql.Repository 作为扩展，实现了通用仓储层功能。与其他规范标准一样，仓储层也有相应的规范定义。FreeSql.Repository 参考 abp vnext 接口，定义和实现基础的仓储层（CURD），应该算比较通用的方法吧。

## 安装

> dotnet add package FreeSql.Repository

## 定义

```csharp
static IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document.db;Pooling=true;Max Pool Size=10")
    .UseAutoSyncStructure(true) //自动迁移实体的结构到数据库
    .Build(); //请务必定义成 Singleton 单例模式

public class Song {
    [Column(IsIdentity = true)]
    public int Id { get; set; }
    public string Title { get; set; }
}
```

## 使用方法

1、IFreeSql 的扩展方法；

```csharp
var curd = fsql.GetRepository<Song>();
```

> 注意：Repository对象多线程不安全

2、继承实现；

```csharp
public class SongRepository : BaseRepository<Song, int> {
    public SongRepository(IFreeSql fsql) : base(fsql, null, null) {}

    //在这里增加 CURD 以外的方法
}
```

3、依赖注入；

```csharp
public void ConfigureServices(IServiceCollection services) {
    
    services.AddSingleton<IFreeSql>(Fsql);
    services.AddFreeRepository(filter => filter
        .Apply<ISoftDelete>("SoftDelete", a => a.IsDeleted == false)
        .Apply<ITenant>("Tenant", a => a.TenantId == 1)
        ,
        this.GetType().Assembly
    );
}

//在控制器使用
public SongsController(GuidRepository<Song> repos1) {
}
```

> 依赖注入的方式可实现全局【过滤与验证】的设定，方便租户功能的设计；

更多资料：[《过滤器、全局过滤器》](https://github.com/2881099/FreeSql/wiki/%e8%bf%87%e6%bb%a4%e5%99%a8)

## 过滤与验证

假设我们有User(用户)、Topic(主题)两个实体，在领域类中定义了两个仓储：

```csharp
var userRepository = fsql.GetGuidRepository<User>();
var topicRepository = fsql.GetGuidRepository<Topic>();
```

在开发过程中，总是担心 topicRepository 的数据安全问题，即有可能查询或操作到其他用户的主题。因此我们在v0.0.7版本进行了改进，增加了 filter lambda 表达式参数。

```csharp
var userRepository = fsql.GetGuidRepository<User>(a => a.Id == 1);
var topicRepository = fsql.GetGuidRepository<Topic>(a => a.UserId == 1);
```

* 在查询/修改/删除时附加此条件，从而达到不会修改其他用户的数据；
* 在添加时，使用表达式验证数据的合法性，若不合法则抛出异常；

## 分表与分库

FreeSql 提供 AsTable 分表的基础方法，GuidRepository 作为分存式仓储将实现了分表与分库（不支持跨服务器分库）的封装。

```csharp
var logRepository = fsql.GetGuidRepository<Log>(null, oldname => $"{oldname}_{DateTime.Now.ToString("YYYYMM")}");
```

上面我们得到一个日志仓储按年月分表，使用它 CURD 最终会操作 Log_201903 表。

注意事项：

* v0.11.12以后的版本可以使用 CodeFirst 迁移分表；
* 不可在分表分库的实体类型中使用《延时加载》；

## 兼容问题

FreeSql 支持多种数据库，分别为 MySql/SqlServer/PostgreSQL/Oracle/Sqlite/达梦/人大金仓/翰高/MsAccess，虽然他们都为关系型数据库，但各自有着独特的技术亮点，有许多亮点值得我们使用；

比如 SqlServer 提供的 output inserted 特性，在表使用了自增或数据库定义了默认值的时候，使用它可以快速将 insert 的数据返回。PostgreSQL 也有相应的功能，如此方便却不是每个数据库都支持。

IRepository 接口定义：

```csharp
TEntity Insert(TEntity entity);
Task<TEntity> InsertAsync(TEntity entity);
```

于是我们做了两种仓库层实现：

- BaseRepository 采用 ExecuteInserted 执行；
- GuidRepository 采用 ExecuteAffrows 执行（兼容性好）；

当采用了不支持的数据库时（Sqlite/MySql/Oracle），建议：

* 使用 uuid 作为主键（即 Guid）；
* 避免使用数据库的默认值功能；
* 仓储层实现请使用 GuidRepository；

## UnitOfWork

UnitOfWork 可将多个仓储放在一个单元管理执行，最终通用 Commit 执行所有操作，内部采用了数据库事务；

```csharp
using (var uow = fsql.CreateUnitOfWork()) {
    var songRepo = uow.GetRepository<Song>();
    var userRepo = uow.GetRepository<User>();

    //上面两个仓储，由同一UnitOfWork uow 创建
    //在此执行仓储操作
    
    //这里不受异步方便影响

    uow.Commit();
}
```

参考：在 asp.net core 中注入工作单元方法

```csharp
//第一步：
public class UnitOfWorkRepository<TEntity, TKey> : BaseRepository<TEntity, TKey>
{
  public UnitOfWorkRepository(IFreeSql fsql, IUnitOfWork uow) : base(fsql, null, null) 
  {
    this.UnitOfWork = uow;
  }
}
public class UnitOfWorkRepository<TEntity> : BaseRepository<TEntity, int>
{
  public UnitOfWorkRepository(IFreeSql fsql, IUnitOfWork uow) : base(fsql, null, null)
  {
    this.UnitOfWork = uow;
  }
}

//第二步：
public void ConfigureServices(IServiceCollection services)
{
  services.AddSingleton<IFreeSql>(fsql);
  services.AddScoped<FreeSql.IUnitOfWork>(sp => fsql.CreateUnitOfWork());

  services.AddScoped(typeof(IReadOnlyRepository<>), typeof(UnitOfWorkRepository<>));
  services.AddScoped(typeof(IBasicRepository<>), typeof(UnitOfWorkRepository<>));
  services.AddScoped(typeof(BaseRepository<>), typeof(UnitOfWorkRepository<>));

  services.AddScoped(typeof(IReadOnlyRepository<,>), typeof(UnitOfWorkRepository<,>));
  services.AddScoped(typeof(IBasicRepository<,>), typeof(UnitOfWorkRepository<,>));
  services.AddScoped(typeof(BaseRepository<,>), typeof(UnitOfWorkRepository<,>));

  //批量注入程序集内的所有自建仓储类，可以根据自己需要来修改
  Assembly[] assemblies = new [] { typeof(XxxRepository).Assembly };
  if (assemblies?.Any() == true)
      foreach (var asse in assemblies)
        foreach (var repo in asse.GetTypes().Where(a => a.IsAbstract == false && typeof(UnitOfWorkRepository).IsAssignableFrom(a)))
          services.AddScoped(repo);
}
```

## 联级保存

请移步文档[《联级保存》](https://github.com/2881099/FreeSql/wiki/%e8%81%94%e7%ba%a7%e4%bf%9d%e5%ad%98)

## 实体变化事件

全局设置：

```csharp
fsql.SetDbContextOptions(opt => {
    opt.OnEntityChange = report => {
        Console.WriteLine(report);
    };
});
```

单独设置 DbContext 或者 UnitOfWork：

```csharp
var ctx = fsql.CreateDbContext();
ctx.Options.OnEntityChange = report => {
    Console.WriteLine(report);
};

var uow = fsql.CreateUnitOfWork();
uow.OnEntityChange = report => {
    Console.WriteLine(report);
};
```

参数 report 是一个 List 集合，集合元素的类型定义如下：

```csharp
public class ChangeInfo
{
    public object Object { get; set; }
    public EntityChangeType Type { get; set; }
}
public enum EntityChangeType { Insert, Update, Delete, SqlRaw }
```

| 变化类型 | 说明 |
| -- | -- |
| Insert | 实体对象被插入 |
| Update | 实体对象被更新 |
| Delete | 实体对象被删除 |
| SqlRaw | 执行了SQL语句 |

SqlRaw 目前有两处地方比较特殊：
- 多对多联级更新导航属性的时候，对中间表的全部删除操作；
- 通用仓储类 BaseRepository 有一个 Delete 方法，参数为表达式，而并非实体；
```csharp
int Delete(Expression<Func<TEntity, bool>> predicate);
```

DbContext.SaveChanges，或者 Repository 对实体的 Insert/Update/Delete，或者 UnitOfWork.Commit 操作都会最多触发一次该事件。