FreeSql.Provider.Odbc 实现 ODBC 访问数据库，ODBC 属于比较原始的技术，更新慢，各大数据库厂支持得标准不一，不到万不得已最好别用 odbc，坑比较多。

FreeSql.Provider.Odbc 做了四种数据库的专用实现：SqlServer、PostgreSQL、Oracle、MySql，和一种通用实现。

专用实现比较有针对性，和原来的 FreeSql.Provider.SqlServer ado.net 相比，只支持较少的基础类型，其他功能几乎都有，包括 CodeFirst 自动迁移。

# 通用实现

通用实现为了让用户自己适配更多的数据库，比如连接 mssql 2000、db2 等数据库。

使用者只需求重写类 FreeSql.Odbc.Default.OdbcAdapter 就可以自定义访问不同的数据库。

我们默认做了一套 sqlserver 的语法和映射适配，代码在 Default/OdbcAdapter.cs，请查看代码了解。

```csharp
class Mssql2000Adapter : FreeSql.Odbc.Default.OdbcAdapter
{
    public override string InsertAfterGetIdentitySql => "SELECT SCOPE_IDENTITY()";
    //可以重写更多的设置
}

fsql.SetOdbcAdapter(new Mssql2000Adapter());
```

适配好新的 OdbcAdapter 后，请在 FreeSqlBuilder.Build 之后调用 IFreeSql.SetOdbcAdapter 方法生效。
