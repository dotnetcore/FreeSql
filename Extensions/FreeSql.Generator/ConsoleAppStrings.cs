using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Generator
{
    public static class ConsoleAppStrings
    {
        /// <summary>
        /// 更新工具：dotnet tool update -g FreeSql.Generator
        /// </summary>
        public static string UpdateTool => CoreErrorStrings.Language == "cn" ?
            @"更新工具：dotnet tool update -g FreeSql.Generator" :
            @"Update tool: dotnet tool update -g FreeSql.Generator";

        /// <summary>
        /// 快速开始
        /// </summary>
        public static string QuickStart => CoreErrorStrings.Language == "cn" ?
            @"快速开始" :
            @"Quick Start";

        /// <summary>
        /// 选择模板：实体类+特性
        /// </summary>
        public static string SelectTemplate1 => CoreErrorStrings.Language == "cn" ?
            @"选择模板：实体类+特性" :
            @"Select template: Entity class + attributes";

        /// <summary>
        /// 选择模板：实体类+特性+导航属性
        /// </summary>
        public static string SelectTemplate2 => CoreErrorStrings.Language == "cn" ?
            @"选择模板：实体类+特性+导航属性" :
            @"Select template: Entity class + attributes + navigation properties";

        /// <summary>
        /// 自定义模板文件，如乱码请修改为UTF8(不带BOM)编码格式
        /// </summary>
        public static string CustomTemplateFile => CoreErrorStrings.Language == "cn" ?
            @"自定义模板文件，如乱码请修改为UTF8(不带BOM)编码格式" :
            @"Custom template file, if garbled please modify to UTF8 (without BOM) encoding format";

        /// <summary>
        /// 4个布尔值对应：
        /// </summary>
        public static string NameOptionsDescription => CoreErrorStrings.Language == "cn" ?
            @"4个布尔值对应：" :
            @"4 boolean values correspond to:";

        /// <summary>
        /// 首字母大写
        /// </summary>
        public static string FirstLetterUppercase => CoreErrorStrings.Language == "cn" ?
            @"首字母大写" :
            @"First letter uppercase";

        /// <summary>
        /// 首字母大写，其他小写
        /// </summary>
        public static string FirstLetterUppercaseOthersLowercase => CoreErrorStrings.Language == "cn" ?
            @"首字母大写，其他小写" :
            @"First letter uppercase, others lowercase";

        /// <summary>
        /// 全部小写
        /// </summary>
        public static string AllLowercase => CoreErrorStrings.Language == "cn" ?
            @"全部小写" :
            @"All lowercase";

        /// <summary>
        /// 下划线转驼峰
        /// </summary>
        public static string UnderscoreToCamelCase => CoreErrorStrings.Language == "cn" ?
            @"下划线转驼峰" :
            @"Underscore to camelCase";

        /// <summary>
        /// 命名空间
        /// </summary>
        public static string NameSpace => CoreErrorStrings.Language == "cn" ?
            @"命名空间" :
            @"Namespace";

        /// <summary>
        /// 数据库
        /// </summary>
        public static string Database => CoreErrorStrings.Language == "cn" ?
            @"数据库" :
            @"database";

        /// <summary>
        /// 达梦数据库
        /// </summary>
        public static string DamengDatabase => CoreErrorStrings.Language == "cn" ?
            @"达梦数据库" :
            @"Dameng Database";

        /// <summary>
        /// 人大金仓数据库
        /// </summary>
        public static string KingbaseESDatabase => CoreErrorStrings.Language == "cn" ?
            @"人大金仓数据库" :
            @"KingbaseES Database";

        /// <summary>
        /// 神舟通用数据库
        /// </summary>
        public static string ShenTongDatabase => CoreErrorStrings.Language == "cn" ?
            @"神舟通用数据库" :
            @"ShenTong Database";

        /// <summary>
        /// 默认生成：表+视图+存储过程
        /// </summary>
        public static string DefaultGeneration => CoreErrorStrings.Language == "cn" ?
            @"默认生成：表+视图+存储过程" :
            @"Default generation: Table + View + StoreProcedure";

        /// <summary>
        /// 如果不想生成视图和存储过程 -Filter View+StoreProcedure
        /// </summary>
        public static string FilterDescription => CoreErrorStrings.Language == "cn" ?
            @"如果不想生成视图和存储过程 -Filter View+StoreProcedure" :
            @"If you don't want to generate views and stored procedures -Filter View+StoreProcedure";

        /// <summary>
        /// 表名或正则表达式，只生成匹配的表，如：dbo\.TB_.+
        /// </summary>
        public static string MatchDescription => CoreErrorStrings.Language == "cn" ?
            @"表名或正则表达式，只生成匹配的表，如：dbo\.TB_.+" :
            @"Table name or regex, only generate matching tables, e.g.: dbo\.TB_.+";

        /// <summary>
        /// Newtonsoft.Json、System.Text.Json、不生成
        /// </summary>
        public static string JsonDescription => CoreErrorStrings.Language == "cn" ?
            @"Newtonsoft.Json、System.Text.Json、不生成" :
            @"Newtonsoft.Json, System.Text.Json, None";

        /// <summary>
        /// 文件名，默认：{name}.cs
        /// </summary>
        public static string FileNameDescription => CoreErrorStrings.Language == "cn" ?
            @"文件名，默认：{name}.cs" :
            @"File name, default: {name}.cs";

        /// <summary>
        /// 保存路径，默认为当前 shell 所在目录
        /// </summary>
        public static string OutputDescription => CoreErrorStrings.Language == "cn" ?
            @"保存路径，默认为当前 shell 所在目录" :
            @"Save path, default is current shell directory";

        /// <summary>
        /// 语言
        /// </summary>
        public static string LangDescription => CoreErrorStrings.Language == "cn" ?
            @"语言，cn=中文，en=英文" :
            @"Language, cn=Chinese, en=English";

        /// <summary>
        /// FreeSql 快速生成数据库的实体类
        /// </summary>
        public static string ToolDescription => CoreErrorStrings.Language == "cn" ?
            @"FreeSql 快速生成数据库的实体类" :
            @"FreeSql quickly generates entity classes from database";

        /// <summary>
        /// 推荐在实体类目录创建 gen.bat，双击它重新所有实体类
        /// </summary>
        public static string RecommendGenBat => CoreErrorStrings.Language == "cn" ?
            @"推荐在实体类目录创建 gen.bat，双击它重新所有实体类" :
            @"Recommend creating gen.bat in entity class directory, double-click to regenerate all entity classes";

        /// <summary>
        /// Ignore {0} -> {1}
        /// </summary>
        public static string IgnoreTable(object type, object name) => CoreErrorStrings.Language == "cn" ?
            $@" Ignore {type} -> {name}" :
            $@" Ignore {type} -> {name}";

        /// <summary>
        /// OUT {0} -> {1}
        /// </summary>
        public static string OutTable(object type, object name) => CoreErrorStrings.Language == "cn" ?
            $@" OUT {type} -> {name}" :
            $@" OUT {type} -> {name}";

        /// <summary>
        /// __重新生成.bat
        /// </summary>
        public static string RebuildBatFileName => CoreErrorStrings.Language == "cn" ?
            @"__重新生成.bat" :
            @"__rebuild.bat";

        /// <summary>
        /// __razor.cshtml.txt
        /// </summary>
        public static string RazorCshtmlFileName => CoreErrorStrings.Language == "cn" ?
            @"__razor.cshtml.txt" :
            @"__razor.cshtml.txt";

        /// <summary>
        /// (以后) 编辑它自定义模板生成
        /// </summary>
        public static string EditTemplateHint => CoreErrorStrings.Language == "cn" ?
            @"(以后) 编辑它自定义模板生成" :
            @"(Later) Edit it to customize template generation";

        /// <summary>
        /// (以后) 双击它重新生成实体
        /// </summary>
        public static string RebuildHint => CoreErrorStrings.Language == "cn" ?
            @"(以后) 双击它重新生成实体" :
            @"(Later) Double-click it to regenerate entities";

        /// <summary>
        /// 生成完毕，总共生成了 {0} 个文件，目录："{1}"
        /// </summary>
        public static string GenerationComplete(int count, string output) => CoreErrorStrings.Language == "cn" ?
            $@"生成完毕，总共生成了 {count} 个文件，目录：""{output}""" :
            $@"Generation complete, total {count} files generated, directory: ""{output}""";
    }
}
