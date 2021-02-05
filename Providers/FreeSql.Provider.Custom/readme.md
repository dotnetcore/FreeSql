FreeSql.Provider.Custom 实现自定义适配器，访问所有数据库。

# 通用实现

通用实现为了让用户自己适配更多的数据库，比如连接 mssql 2000、db2 等数据库，牺牲了一些功能：

- 不支持 CodeFirst 自动迁移
- 不支持 DbFirst 接口方法的实现
- 不支持 原来的分页方法，需要自行判断 id 进行分页
- 只支持较少的基础类型：bool,sbyte,short,int,long,byte,ushort,uint,ulong,double,float,decimal,DateTime,byte[],string,Guid

使用者只需求重写类 FreeSql.Custom.CustomAdapter 就可以自定义访问不同的数据库。

我们默认做了一套 sqlserver 的语法和映射适配，代码在 [CustomAdapter.cs](https://github.com/2881099/FreeSql/blob/master/Providers/FreeSql.Provider.Custom/CustomAdapter.cs)，请查看代码了解。

```csharp
class Mssql2000Adapter : FreeSql.Custom.CustomAdapter
{
    public override string InsertAfterGetIdentitySql => "SELECT SCOPE_IDENTITY()";
    //可以重写更多的设置
}

static IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.Custom, () => new SqlConnection(@"Data Source=..."))
    .Build(); //be sure to define as singleton mode

fsql.SetCustomAdapter(new Mssql2000Adapter());
```

适配好新的 CustomAdapter 后，请在 FreeSqlBuilder.Build 之后调用 IFreeSql.SetCustomAdapter 方法生效。
