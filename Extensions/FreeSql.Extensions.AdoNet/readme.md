FreeSql AdoNet 扩展包，增加 IDbConnection/IDbTransaction 对象的扩展方法 Select/Insert/Update/Delete 实现 CRUD。

## 如果在 abp-vnext 中使用？

本方法依赖 Volo.Abp.Dapper，所以也依赖 Volo.Abp.EntityFrameworkCore

提示：FreeSql 兼容 EFCore 99% 的实体特性

> dotnet add package FreeSql.Extensions.AdoNet

```csharp
IDapperRepository repo = ...;

repo.DbConnection.Select<T>().Where(...).ToList();

repo.DbConnection.Insert(new T {}).ExecuteAffrows();

repo.DbConnection.Update().SetSource(new T {}).ExecuteAffrows();

repo.DbConnection.Delete<T>().Where(...).ExecuteAffrows();

IFreeSql fsql = repo.DbConnection.GetFreeSql(); //获取 IFreeSql 实例
```
