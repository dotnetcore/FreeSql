[中文](README.zh-CN.MD) | **English**

FreeSql extension package, map *ValueObject* to `typeof(string)`, install the extension package:

> dotnet add package FreeSql.Extensions.JsonMap

```csharp
fsql.UseJsonMap(); //Turn on function

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
