using RazorEngine.Templating;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Console = Colorful.Console;

namespace FreeSql.Generator
{
    class ConsoleApp
    {
        string ArgsRazorRaw { get; }
        string ArgsRazor { get; }
        bool[] ArgsNameOptions { get; }
        string ArgsNameSpace { get; }
        DataType ArgsDbType { get; }
        string ArgsConnectionString { get; }
        string ArgsFileName { get; }
        string ArgsOutput { get; }

        public ConsoleApp(string[] args, ManualResetEvent wait)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var gb2312 = Encoding.GetEncoding("GB2312");
            if (gb2312 != null)
            {
                try
                {
                    Console.OutputEncoding = gb2312;
                    Console.InputEncoding = gb2312;
                }
                catch { }
            }

            //var ntjson = Assembly.LoadFile(@"C:\Users\28810\Desktop\testfreesql\bin\Debug\netcoreapp2.2\publish\testfreesql.dll");

            //using (var gen = new Generator(new GeneratorOptions()))
            //{
            //    gen.TraceLog = log => Console.WriteFormatted(log + "\r\n", Color.DarkGray);
            //    gen.Build(ArgsOutput, new[] { typeof(ojbk.Entities.AuthRole) }, false);
            //}

            var version = "v" + string.Join(".", typeof(ConsoleApp).Assembly.GetName().Version.ToString().Split('.').Where((a, b) => b <= 2));
            Console.WriteAscii(" FreeSql", Color.Violet);
            Console.WriteFormatted(@"
  # Github # {0} {1}
", Color.SlateGray,
new Colorful.Formatter("https://github.com/2881099/FreeSql", Color.DeepSkyBlue),
new Colorful.Formatter("v" + string.Join(".", typeof(ConsoleApp).Assembly.GetName().Version.ToString().Split('.').Where((a, b) => b <= 2)), Color.SlateGray));

            ArgsNameOptions = new[] { false, false, false, false };
            ArgsOutput = Directory.GetCurrentDirectory();
            ArgsFileName = "{name}.cs";
            string args0 = args[0].Trim().ToLower();
            if (args[0] == "?" || args0 == "--help" || args0 == "-help")
            {

                Console.WriteFormatted(@"
    {0}

    更新工具：dotnet tool update -g FreeSql.Generator


  # 快速开始 #

  > {1} {2} 1 {3} 0,0,0,0 {4} MyProject {5} ""MySql,Data Source=127.0.0.1;...""

     -Razor 1                  * 选择模板：实体类+特性
     
     -Razor 2                  * 选择模板：实体类+特性+导航属性
     
     -Razor ""d:\diy.cshtml""    * 自定义模板文件
     
     -NameOptions              * 总共4个布尔值，分别对应：
                               # 首字母大写
                               # 首字母大写,其他小写
                               # 全部小写
                               # 下划线转驼峰
                               
     -NameSpace                * 命名空间
     
     -DB ""{6},Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=数据库;Charset=utf8;SslMode=none;Max pool size=2""
     
     -DB ""{7},Data Source=.;Integrated Security=True;Initial Catalog=数据库;Pooling=true;Max Pool Size=2""
     
     -DB ""{8},Host=192.168.164.10;Port=5432;Username=postgres;Password=123456;Database=数据库;Pooling=true;Maximum Pool Size=2""
     
     -DB ""{9},user id=user1;password=123456;data source=//127.0.0.1:1521/XE;Pooling=true;Max Pool Size=2""
     
     -DB ""{10},Driver={DM8 ODBC DRIVER};Server=127.0.0.1:5236;Persist Security Info=False;Trusted_Connection=Yes;UID=USER1;PWD=123456789;Max pool size=2""
                               {10} 是国产达梦数据库，需要使用 ODBC 连接

     -FileName                 文件名，默认：{name}.cs

     -Output                   保存路径，默认为当前 shell 所在目录
                               {11}

", Color.SlateGray,
new Colorful.Formatter("使用 FreeSql 快速生成数据库的实体类", Color.SlateGray),
new Colorful.Formatter("FreeSql.Generator", Color.White),
new Colorful.Formatter("-Razor", Color.ForestGreen),
new Colorful.Formatter("-NameOptions", Color.ForestGreen),
new Colorful.Formatter("-NameSpace", Color.ForestGreen),
new Colorful.Formatter("-DB", Color.ForestGreen),
new Colorful.Formatter("MySql", Color.Yellow),
new Colorful.Formatter("SqlServer", Color.Yellow),
new Colorful.Formatter("PostgreSQL", Color.Yellow),
new Colorful.Formatter("Oracle", Color.Yellow),
new Colorful.Formatter("OdbcDameng", Color.Yellow),
new Colorful.Formatter("推荐在实体类目录创建 gen.bat，双击它重新所有实体类", Color.ForestGreen)
);
                wait.Set();
                return;
            }
            for (int a = 0; a < args.Length; a++)
            {
                switch (args[a])
                {
                    case "-Razor":
                        ArgsRazorRaw = args[a + 1].Trim();
                        switch (ArgsRazorRaw)
                        {
                            case "1": ArgsRazor = RazorContentManager.实体类_特性_cshtml; break;
                            case "2": ArgsRazor = RazorContentManager.实体类_特性_导航属性_cshtml; break;
                            default: ArgsRazor = File.ReadAllText(args[a + 1]); break;
                        }
                        a++;
                        break;

                    case "-NameOptions":
                        ArgsNameOptions = args[a + 1].Split(',').Select(opt => opt == "1").ToArray();
                        if (ArgsNameOptions.Length != 4) throw new ArgumentException("-NameOptions 参数错误，格式为：0,0,0,0");
                        a++;
                        break;
                    case "-NameSpace":
                        ArgsNameSpace = args[a + 1];
                        a++;
                        break;
                    case "-DB":
                        var dbargs = args[a + 1].Split(',', 2);
                        if (dbargs.Length != 2) throw new ArgumentException("-DB 参数错误，格式为：MySql,ConnectionString");
                        switch (dbargs[0].Trim().ToLower())
                        {
                            case "mysql": ArgsDbType = DataType.MySql; break;
                            case "sqlserver": ArgsDbType = DataType.SqlServer; break;
                            case "postgresql": ArgsDbType = DataType.PostgreSQL; break;
                            case "oracle": ArgsDbType = DataType.Oracle; break;
                            case "odbcdameng": ArgsDbType = DataType.OdbcDameng; break;
                            default: throw new ArgumentException($"-DB 参数错误，不支持的类型：{dbargs[0]}");
                        }
                        ArgsConnectionString = dbargs[1].Trim();
                        if (string.IsNullOrEmpty(ArgsConnectionString)) throw new ArgumentException($"-DB 参数错误，未提供 ConnectionString");
                        a++;
                        break;
                    case "-FileName":
                        ArgsFileName = args[a + 1];
                        a++;
                        break;
                    case "-Output":
                        ArgsOutput = args[a + 1];
                        a++;
                        break;
                }
            }

            ArgsOutput = ArgsOutput.Trim().TrimEnd('/', '\\');
            ArgsOutput += ArgsOutput.Contains("\\") ? "\\" : "/";
            if (!Directory.Exists(ArgsOutput))
                Directory.CreateDirectory(ArgsOutput);

            RazorEngine.Engine.Razor = RazorEngineService.Create(new RazorEngine.Configuration.TemplateServiceConfiguration
            {
                EncodedStringFactory = new RazorEngine.Text.RawStringFactory() // Raw string encoding.
            });
            var razorId = Guid.NewGuid().ToString("N");
            RazorEngine.Engine.Razor.Compile(ArgsRazor, razorId);

            var outputCounter = 0;
            using (IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(ArgsDbType, ArgsConnectionString)
                .UseAutoSyncStructure(false)
                .UseMonitorCommand(cmd => Console.WriteFormatted(cmd.CommandText + "\r\n", Color.SlateGray))
                .Build())
            {
                var tables = fsql.DbFirst.GetTablesByDatabase();
                var outputTables = tables;

                
                //开始生成操作
                foreach (var table in outputTables)
                {
                    var sw = new StringWriter();
                    var model = new RazorModel(fsql, ArgsNameSpace, ArgsNameOptions, tables, table);
                    RazorEngine.Engine.Razor.Run(razorId, sw, null, model);

                    StringBuilder plus = new StringBuilder();
                    plus.AppendLine("//------------------------------------------------------------------------------");
                    plus.AppendLine("// <auto-generated>");
                    plus.AppendLine("//     此代码由工具 FreeSql.Generator 生成。");
                    plus.AppendLine("//     运行时版本:" + Environment.Version.ToString());
                    plus.AppendLine("//     Website: https://github.com/2881099/FreeSql");
                    plus.AppendLine("//     对此文件的更改可能会导致不正确的行为，并且如果");
                    plus.AppendLine("//     重新生成代码，这些更改将会丢失。");
                    plus.AppendLine("// </auto-generated>");
                    plus.AppendLine("//------------------------------------------------------------------------------");
                    plus.Append(sw.ToString());
                    plus.AppendLine();

                    var outputFile = $"{ArgsOutput}{ArgsFileName.Replace("{name}", model.GetCsName(table.Name))}";
                    File.WriteAllText(outputFile, plus.ToString());
                    Console.WriteFormatted(" OUT -> " + outputFile + "\r\n", Color.DeepSkyBlue);
                    ++outputCounter;
                }
            }

            var rebuildBat = ArgsOutput + "__重新生成.bat";
            if (File.Exists(rebuildBat) == false)
            {
                File.WriteAllText(rebuildBat, @$"
FreeSql.Generator -Razor {ArgsRazorRaw} -NameOptions {string.Join(",", ArgsNameOptions.Select(a => a ? 1 : 0))} -NameSpace {ArgsNameSpace} -DB ""{ArgsDbType},{ArgsConnectionString}""
"); 
                Console.WriteFormatted(" OUT -> " + rebuildBat + "    (以后) 双击它重新生成实体\r\n", Color.Magenta);
                ++outputCounter;
            }

            Console.WriteFormatted($"\r\n[{DateTime.Now.ToString("MM-dd HH:mm:ss")}] 生成完毕，总共生成了 {outputCounter} 个文件，目录：\"{ArgsOutput}\"\r\n", Color.DarkGreen);

            Console.ReadKey();
            wait.Set();
        }
    }
}

