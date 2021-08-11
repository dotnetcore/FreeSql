[中文](README.zh-CN.md) | **English**

# Preface

I have tried ADO.NET, Dapper, EF, and Repository storage, and even wrote a generator tool myself to do common CRUD operations.

Their operation is inconvenient:

- Need to declare before use;

- Each entity class corresponds to an operation class (or DAL, DbContext, Repository).

BaseEntity is a very simple way of CodeFirst development, especially for single-table or multi-table CRUD operations. BaseEntity uses "inheritance" to save the repetitive code (creation time, ID and other fields) and functions of each entity class, and at the same time, it is not necessary to consider the use of repository when performing CURD operations.


This article will introduce a very simple CRUD operation method of BaseEntity.

# Features

- Automatically migrate the entity structure (CodeFirst) to the database;

- Directly perform CRUD operations on entities;

- Simplify user-defined entity types, eliminating hard-coded primary keys, common fields and their configuration (such as CreateTime, UpdateTime);

- Logic delete of single-table and multi-table query;

# Declaring

> dotnet add package FreeSql.Extensions.BaseEntity

> dotnet add package FreeSql.Provider.Sqlite

1. Define an auto-increment primary key of type `int`. When the `TKey` of `BaseEntity` is specified as `int/long`, the primary key will be considered as auto-increment;

```csharp
public class UserGroup : BaseEntity<UserGroup, int>
{
    public string GroupName { get; set; }
}
```

If you don't want the primary key to be an auto-increment key, you can override the attribute:

```csharp
public class UserGroup : BaseEntity<UserGroup, int>
{
    [Column(IsIdentity = false)]
    public override int Id { get; set; }
    public string GroupName { get; set; }
}
```
> For more information about the attributes of entities, please refer to: https://github.com/dotnetcore/FreeSql/wiki/Entity-Attributes

2. Define an entity whose primary key is Guid type, when saving data, it will automatically generate ordered and non-repeated Guid values (you don't need to specify `Guid.NewGuid()` yourself);

```csharp
public class User : BaseEntity<UserGroup, Guid>
{
    public string UserName { get; set; }
}
```

# Usage of CRUD

```csharp
//Insert Data
var item = new UserGroup { GroupName = "Group One" };
item.Insert();

//Update Data
item.GroupName = "Group Two";
item.Update();

//Insert or Update Data
item.Save();

//Logic Delete
item.Delete();

//Recover Logic Delete
item.Restore();

//Get the object by the primary key
var item = UserGroup.Find(1);

//Query Data
var items = UserGroup.Where(a => a.Id > 10).ToList();
```

`{ENTITY_TYPE}.Select` returns a query object, the same as `FreeSql.ISelect`.

In the multi-table query, the logic delete condition will be attached to the query of each table.

> For more information about query data, please refer to: https://github.com/2881099/FreeSql/wiki/Query-Data

# Transaction Suggestion

Because the `AsyncLocal` platform is not compatible, the transaction is managed by the outside.

```csharp
static AsyncLocal<IUnitOfWork> _asyncUow = new AsyncLocal<IUnitOfWork>();

BaseEntity.Initialization(fsql, () => _asyncUow.Value);
```

At the beginning of `Scoped`: `_asyncUow.Value = fsql.CreateUnitOfWork();` (You can also use the `UnitOfWorkManager` object to get uow)

At the end of `Scoped`: `_asyncUow.Value = null;`

as follows:

```csharp
using (var uow = fsql.CreateUnitOfWork())
{
    _asyncUow.Value = uow;

    try
    {
        //todo ... BaseEntity internal CURD method keeps using uow transaction
    }
    finally
    {
        _asyncUow.Value = null;
    }
    
    uow.Commit();
}
```
