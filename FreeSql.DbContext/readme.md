FreeSql.DbContext 实现类似 EFCore 使用习惯，跟踪对象状态，最终通过 SaveChanges 方法提交事务。

## 安装

> dotnet add package FreeSql.DbContext

## 如何使用

0、通用方法，为啥是0？？？
```
using (var ctx = fsql.CreateDbContext()) {
    //var db1 = ctx.Set<Song>();
    //var db2 = ctx.Set<Tag>();

    var item = new Song { };
    ctx.Add(item);
    ctx.SaveChanges();
}
```

> 注意：DbContext 对象多线程不安全

1、在 OnConfiguring 方法上配置与 IFreeSql 关联

```csharp
public class SongContext : DbContext {

    public DbSet<Song> Songs { get; set; }
    public DbSet<Song> Tags { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder) {
        builder.UseFreeSql(dbcontext_01.Startup.Fsql);
    }
}


public class Song {
    [Column(IsIdentity = true)]
    public int Id { get; set; }
    public DateTime? Create_time { get; set; }
    public bool? Is_deleted { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }

    public virtual ICollection<Tag> Tags { get; set; }
}
public class Song_tag {
    public int Song_id { get; set; }
    public virtual Song Song { get; set; }

    public int Tag_id { get; set; }
    public virtual Tag Tag { get; set; }
}

public class Tag {
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

使用的时候与 EFCore 类似：

```csharp
long id = 0;

using (var ctx = new SongContext()) {

    var song = new Song { };
    await ctx.Songs.AddAsync(song);
    id = song.Id;

    var adds = Enumerable.Range(0, 100)
        .Select(a => new Song { Create_time = DateTime.Now, Is_deleted = false, Title = "xxxx" + a, Url = "url222" })
        .ToList();
    await ctx.Songs.AddRangeAsync(adds);

    for (var a = 0; a < adds.Count; a++)
        adds[a].Title = "dkdkdkdk" + a;

    ctx.Songs.UpdateRange(adds);

    ctx.Songs.RemoveRange(adds.Skip(10).Take(20).ToList());

    //ctx.Songs.Update(adds.First());

    adds.Last().Url = "skldfjlksdjglkjjcccc";
    ctx.Songs.Update(adds.Last());

    //throw new Exception("回滚");

    await ctx.SaveChangesAsync();
}
```

2、注入方式使用

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IFreeSql>(Fsql);
    services.AddFreeDbContext<SongContext>(options => options.UseFreeSql(Fsql));
}
```

在 mvc 中获取：

```csharp
IFreeSql _orm;
public ValuesController(SongContext songContext) {
}
```

## 优先级

OnConfiguring > AddFreeDbContext

## 乐观锁

更新实体数据，在并发情况下极容易造成旧数据将新的记录更新。FreeSql 核心部分已经支持乐观锁。

乐观锁的原理，是利用实体某字段，如：long version，更新前先查询数据，此时 version 为 1，更新时产生的 SQL 会附加 where version = 1，当修改失败时（即 Affrows == 0）抛出异常。

每个实体只支持一个乐观锁，在属性前标记特性：[Column(IsVersion = true)] 即可。

> 无论是使用 FreeSql/FreeSql.Repository/FreeSql.DbContext，每次更新 version 的值都会增加 1

## 说明

- DbContext 操作的数据在最后 SaveChanges 时才批量保存；
- DbContext 内所有操作，使用同一个事务；
- 当实体存在自增时，或者 Add/AddRange 的时候主键值为空，会提前开启事务；
- 支持同步/异步方法；

## 合并机制

db.Add(new Xxx());
db.Add(new Xxx());
db.Add(new Xxx());

这三步，会合并成一个批量插入的语句执行，前提是它们没有自增属性。

适用 Guid 主键，Guid 主键的值不用设置，交给 FreeSql 处理即可，空着的 Guid 主键会在插入时获取有序不重值的 Guid 值。

又比如：

db.Add(new Xxx());
db.Add(new Xxx());
db.Update(xxx);
db.Add(new Xxx());

Guid Id 的情况下，执行三次命令：前两次插入合并执行，update 为一次，后面的 add 为一次。

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
public class EntityChangeInfo
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