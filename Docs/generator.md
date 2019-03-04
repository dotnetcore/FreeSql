# 生成器

生成器是基于 dbfirst 开发的辅助工具，适用老项目一键生成实体。生成器采用模板的方式，作者实现了三种生成模板：

| 模板名称 | 类型映射 | 外键导航属性 | 缓存管理 | 失血 | 贫血 | 充血 |
| ------------- | - | - |- | - |- | - |
| simple-entity | √  | X | X | √ | X | X |
| simple-entity-navigation-object | √  | √ | X | √ | X | X |
| rich-entity-navigation-object | √  | √ | √ | X | √ | X |

模板在项目目录：/Templates/MySql

> 更多模板逐步开发中。。。

```csharp
//定义 mysql FreeSql
IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=10")
    .UseAutoSyncStructure(true)

    .UseMonitorCommand(
        cmd => {
            Console.WriteLine(cmd.CommandText);
        }, //监听SQL命令对象，在执行前
        (cmd, traceLog) => {
            Console.WriteLine(traceLog);
        }) //监听SQL命令对象，在执行后
    .Build();

//创建模板生成类实现
var gen = new FreeSql.Generator.TemplateGenerator();
gen.Build(mysql.DbFirst, 
    @"C:\Users\28810\Desktop\github\FreeSql\Templates\MySql\simple-entity",  //模板目录（事先下载）
    @"C:\Users\28810\Desktop\新建文件夹 (9)",  //生成后保存的目录
    "cccddd" //数据库
);
```

## 模板语法

```html
<html>
<head>
<title>{#title}</title>
</head>
<body>

<!--绑定表达式-->
{#表达式}
{##表达式} 当表达式可能发生runtime错误时使用，性能没有上面的高

<!--可嵌套使用，同一标签最多支持3个指令-->
{include ../header.html}
<div @for="i 1, 101">
  <p @if="i === 50" @for="item,index in data">aaa</p>
  <p @else="i % 3 === 0">bbb {#i}</p>
  <p @else="">ccc {#i}</p>
</div>

<!--定义模块，可以将公共模块定义到一个文件中-->
{module module_name1 parms1, 2, 3...}
{/module}
{module module_name2 parms1, 2, 3...}
{/module}

<!--使用模块-->
{import ../module.html as myname}
{#myname.module_name(parms1, 2, 3...)}

<!--继承-->
{extends ../inc/layout.html}
{block body}{/block}

<!--嵌入代码块-->
{%
for (var a = 0; a < 100; a++)
  print(a);
%}

<!--条件分支-->
{if i === 50}
{elseif i > 60}
{else}
{/if}

<!--三种循环-->
{for i 1,101}                可自定义名 {for index2 表达式1 in 表达式2}

{for item,index in items}    可选参数称 index
                             可自定义名 {for item2, index99 in 数组表达式}

{for key,item,index on json} 可选参数 item, index，
                             可自定义名 {for key2, item2, index99 in 对象表达式}
{/for}

<!--不被解析-->
{miss}
此块内容不被bmw.js解析
{/miss}

</body>
</html>
```