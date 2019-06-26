这是 [FreeSql](https://github.com/2881099/FreeSql) 衍生出来的扩展包，包含 DbContext & DbSet、Repository & UnitOfWork 实现面向对象的特性（QQ群：4336577）。

> dotnet add package FreeSql.DbContext

## 更新日志

### v0.6.5

- 修复 Repository 联级保存的 bug；
- 添加工作单元开启方法；
- 适配 .net framework 4.5、netstandard 2.0；

### v0.6.1

- 拆分 FreeSql 小包引用，各数据库单独包、延时加载包；
- FreeSql.Extensions.LazyLoading
- FreeSql.Provider.MySql
- FreeSql.Provider.PostgreSQL
- FreeSql.Provider.SqlServer
- FreeSql.Provider.Sqlite
- FreeSql.Provider.Oracle
- 移除 IFreeSql.Cache，以及 ISelect.Caching 方法；
- 移除 IFreeSql.Log，包括内部原有的日志输出，改为 Trace.WriteLine；
- IAdo.Query\<dynamic\> 读取返回变为 List\<Dictionary\<string, object\>\>；
- 定义 IFreeSql 和以前一样，移除了 UseCache、UseLogger 方法；

## DbContext & DbSet

```csharp
using (var ctx = new SongContext()) {
    var song = new Song { BigNumber = "1000000000000000000" };
    ctx.Songs.Add(song);

    song.BigNumber = (BigInteger.Parse(song.BigNumber) + 1).ToString();
    ctx.Songs.Update(song);

    var tag = new Tag {
        Name = "testaddsublist",
        Tags = new[] {
            new Tag { Name = "sub1" },
            new Tag { Name = "sub2" },
            new Tag {
                Name = "sub3",
                Tags = new[] {
                    new Tag { Name = "sub3_01" }
                }
            }
        }
    };
    ctx.Tags.Add(tag);

    ctx.SaveChanges();
}
```

## Repository & UnitOfWork

仓储与工作单元一起使用，工作单元具有事务特点。

```csharp
using (var unitOfWork = fsql.CreateUnitOfWork()) {
    var songRepository = unitOfWork.GetRepository<Song, int>();
    var tagRepository = unitOfWork.GetRepository<Tag, int>();

    var song = new Song { BigNumber = "1000000000000000000" };
    songRepository.Insert(song);

    songRepository.Update(song);

    song.BigNumber = (BigInteger.Parse(song.BigNumber) + 1).ToString();
    songRepository.Update(song);

    var tag = new Tag {
        Name = "testaddsublist",
        Tags = new[] {
            new Tag { Name = "sub1" },
            new Tag { Name = "sub2" },
            new Tag {
                Name = "sub3",
                Tags = new[] {
                    new Tag { Name = "sub3_01" }
                }
            }
        }
    };
    tagRepository.Insert(tag);

    ctx.Commit();
}
```

## Repository

简单使用仓储，有状态跟踪，它不包含事务的特点。

```csharp
var songRepository = fsql.GetRepository<Song, int>();
var song = new Song { BigNumber = "1000000000000000000" };
songRepository.Insert(song);
```

## IFreeSql 核心定义

```csharp
var fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\dd2.db;Pooling=true;Max Pool Size=10")
    .UseAutoSyncStructure(true)
    .UseNoneCommandParameter(true)

    .UseMonitorCommand(cmd => Trace.WriteLine(cmd.CommandText))
    .Build();

public class Song {
    [Column(IsIdentity = true)]
    public int Id { get; set; }
    public string BigNumber { get; set; }

    [Column(IsVersion = true)] //乐观锁
    public long versionRow { get; set; }
}
public class Tag {
    [Column(IsIdentity = true)]
    public int Id { get; set; }

    public int? Parent_id { get; set; }
    public virtual Tag Parent { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Tag> Tags { get; set; }
}

public class SongContext : DbContext {
    public DbSet<Song> Songs { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder) {
        builder.UseFreeSql(fsql);
    }
}
```

# 过滤器与验证

假设我们有User(用户)、Topic(主题)两个实体，在领域类中定义了两个仓储：

```csharp
var userRepository = fsql.GetGuidRepository<User>();
var topicRepository = fsql.GetGuidRepository<Topic>();
```

在开发过程中，总是担心 topicRepository 的数据安全问题，即有可能查询或操作到其他用户的主题。因此我们在v0.0.7版本进行了改进，增加了 filter lambad 表达式参数。

```csharp
var userRepository = fsql.GetGuidRepository<User>(a => a.Id == 1);
var topicRepository = fsql.GetGuidRepository<Topic>(a => a.UserId == 1);
```

* 在查询/修改/删除时附加此条件，从而达到不会修改其他用户的数据；
* 在添加时，使用表达式验证数据的合法性，若不合法则抛出异常；

# 分表与分库

FreeSql 提供 AsTable 分表的基础方法，GuidRepository 作为分存式仓储将实现了分表与分库（不支持跨服务器分库）的封装。

```csharp
var logRepository = fsql.GetGuidRepository<Log>(null, oldname => $"{oldname}_{DateTime.Now.ToString("YYYYMM")}");
```

上面我们得到一个日志仓储按年月分表，使用它 CURD 最终会操作 Log_201903 表。

合并两个仓储，实现分表下的联表查询：

```csharp
fsql.GetGuidRepository<User>().Select.FromRepository(logRepository)
    .LeftJoin<Log>(b => b.UserId == a.Id)
    .ToList();
```

注意事项：

* 不能使用 CodeFirst 迁移分表，开发环境时仍然可以迁移 Log 表；
* 不可在分表分库的实体类型中使用《延时加载》；

# 历史版本

### v0.5.23

- 增加 DbSet/Repository FlushState 手工清除状态管理数据；

### v0.5.21

- 修复 AddOrUpdate/InsertOrUpdate 当主键无值时，仍然查询了一次数据库；
- 增加 查询数据时 TrackToList 对导航集合的状态跟踪；
- 完善 AddOrUpdateNavigateList 联级保存，忽略标记 IsIgnore 的集合属性；
- 完成 IFreeSql.Include、IncludeMany 功能；

### v0.5.12

- 增加 工作单元开关，可在任意 Insert/Update/Delete 之前调用，以关闭工作单元使其失效；[PR #1](https://github.com/2881099/FreeSql.DbContext/pull/1)

### v0.5.9

- 增加 linq to sql 的查询语法，以及单元测试，[wiki](https://github.com/2881099/FreeSql/wiki/LinqToSql)；
- 修复 EnableAddOrUpdateNavigateList 设置对异步方法无效的 bug；

### v0.5.8

- 增加 IFreeSql.SetDbContextOptions 设置 DbContext 的功能：开启或禁用连级一对多导航集合属性保存的功能，EnableAddOrUpdateNavigateList（默认开启）；
- 增加 IUnitOfWork.IsolationLevel 设置事务级别；

### v0.5.7

- 修复 UnitOfWork.GetRepository() 事务 bug，原因：仓储的每步操作都提交了事务；

### v0.5.5

- 修复 MapEntityValue 对 IsIgnore 未处理的 bug；

### v0.5.4

- 修复 Repository 追加导航集合的保存 bug；
- 公开 IRepository.Orm 对象；

### v0.5.3

- 修复 实体跟踪的 bug，当查询到的实体自增值为 0 时重现；
- 优化 状态管理字典为 ConcurrentDictionary；

### v0.5.2

- 优化 SqlServer UnitOfWork 使用bug，在 FreeSql 内部解决的；
- 补充 测试与支持联合主键的自增；

### v0.5.1

- 补充 开放 DbContext.UnitOfWork 对象，方便扩展并保持在同一个事务执行；
- 补充 增加 DbSet\<object\>、Repository\<object\> 使用方法，配合 AsType(实体类型)，实现弱类型操作；
- 修复 DbContext.AddOrUpdate 传入 null 时，任然会查询一次数据库的 bug；
- 优化 DbContext.AddOrUpdate 未添加实体主键的错误提醒；
- 修复 DbContext.Set\<object\> 缓存的 bug，使用多种弱类型时发生；
- 修复 IsIgnore 过滤字段后，查询的错误；
- 修复 全局过滤器功能迁移的遗留 bug；

### v0.4.14

- 优化 Add 时未设置主键的错误提醒；

### v0.4.13

- 补充 Repository 增加 Attach 方法；
- 优化 Update/AddOrUpdate 实体的时候，若状态管理不存在，尝试查询一次数据库，以便跟踪对象；

### v0.4.12

- 修复 非自增情况下，Add 后再 Update 该实体时，错误（需要先 Attach 或查询）的 bug；

### v0.4.10

- 补充 开放 DbContext.Orm 对象；
- 修复 OnConfiguring 未配置时注入获取失败的 bug；

### v0.4.6

- 修复 DbSet AddRange/UpdateRange/RemoveRange 参数为空列表时报错，现在不用判断 data.Any() == true 再执行；
- 增加 DbContext 对 DbSet 的快速代理方法(Add/Update/Remove/Attach)；
- 增加 DbContext 通用类，命名为：FreeContext，也可以通过 IFreeSql 扩展方法 CreateDbContext 创建；
- 增加 ISelect NoTracking 扩展方法，查询数据时不追踪（从而提升查询性能）；

### v0.4.5

- 增加 DbSet Attach 方法附加实体，可用于不查询就更新或删除；

### v0.4.2

- 增加 DbSet UpdateAsync/UpdateRangeAsync 方法，当一个实体被更新两次时，会先执行前面的队列；
- 增加 GetRepository 获取联合主键的适用仓储类；
- 增加 DbSet 在 Add/Update 时对导航属性(OneToMany) 的处理（AddOrUpdate）；

### v0.4.1
- 独立 FreeSql.DbContext 项目；
- 实现 Repository + DbSet 统一的状态跟踪与工作单元；
- 增加 DbSet AddOrUpdate 方法；
- 增加 Repository InsertOrUpdate 方法；