FreeSql 扩展包，将值对象映射成 typeof(string)，安装扩展包：

> dotnet add package FreeSql.Extensions.JsonMap

```csharp
fsql.UseJsonMap(); //开启功能

class TestConfig
{
    public int clicks { get; set; }
    public string title { get; set; }
}

[Table(Name = "sysconfig")]
public class S_SysConfig<T>
{
    [Column(IsPrimary = true)]
    public string Name { get; set; }

    [JsonMap]
    public T Config { get; set; }
}
```