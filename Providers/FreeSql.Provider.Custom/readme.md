# 国产数据库

| 数据库名称 | 提供者 | 系列风格 |
| --- | --- | --- |
| 达梦 | FreeSql.Provider.Dameng | Oracle |
| 神州通用 | FreeSql.Provider.ShenTong | PostgreSQL |
| 人大金仓 | FreeSql.Provider.KingbaseES | PostgreSQL |
| 南大通用 | FreeSql.Provider.GBase | Informix |
| 翰高 | FreeSql.Provider.Custom、FreeSql.Provider.Odbc | PostgreSQL |

由于太多，在此不一一列举，它们大多数语法兼容 MySql、Oracle、SqlServer、PostgreSQL 四种常用数据库之一。

FreeSql.Provider.Custom 提供了这四种数据库适配，并且支持 CodeFirst/DbFirst 以及完整的 FreeSql 功能。

FreeSql.Provider.Custom 不依赖具体 ado.net/odbc/oledb dll 驱动，使用者在外部自行引用 dll 驱动。

访问 MySql 数据库为例：

```csharp
var fsql = new FreeSqlBuilder()
    .UseConnectionFactory(DataType.CustomMySql, () => 
        new MySqlConnection("Data Source=..."))
    .UseNoneParameter(true)
    .UseMonitorCommand(Console.WriteLine(cmd.CommandText))
    .Build();
fsql.SetDbProviderFactory(MySqlConnectorFactory.Instance);
```

若某国产数据库兼容 MySql SQL，先引用对方提供的 DLL，然后：

- 将上面 new MySqlConnection 替换成 new XxxConnection
- 将上面 MySqlConnectorFactory.Instance 替换成对应的 DbProviderFactory

提示：对方 DLL 一般都会提供这两个现实类

# 自定义适配

除了上面，还提供了自定义适配更多的数据库，比如 mssql2000、db2，自定义适配将牺牲一些功能：

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

static IFreeSql fsql = new FreeSqlBuilder()
    .UseConnectionString(DataType.Custom, () => new SqlConnection(@"Data Source=..."))
    .Build(); //be sure to define as singleton mode

fsql.SetCustomAdapter(new Mssql2000Adapter());
```

适配好新的 CustomAdapter 后，请在 FreeSqlBuilder.Build 之后调用 IFreeSql.SetCustomAdapter 方法生效。
